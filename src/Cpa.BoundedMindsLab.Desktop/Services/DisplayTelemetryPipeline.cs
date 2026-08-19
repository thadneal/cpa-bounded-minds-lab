using System.Collections.Concurrent;
using System.Diagnostics;
using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Desktop.ViewModels;

namespace Cpa.BoundedMindsLab.Desktop.Services;

public sealed class DisplayTelemetryPipeline : IExperimentFrameObserver, IDisposable
{
    private const int Capacity = 16_384;
    private const int ProjectionBatchSize = 512;
    private readonly ConcurrentQueue<QueuedFrame> _queue = new();
    private readonly object _projectionSync = new();
    private readonly CancellationTokenSource _stop = new();
    private readonly TelemetryStore _store = new();
    private readonly Task _projector;
    private long _published;
    private long _dropped;
    private long _projected;
    private int _backlog;
    private double _projectionMilliseconds;
    private long _generation;
    private bool _disposed;

    public DisplayTelemetryPipeline()
    {
        _projector = Task.Run(ProjectLoopAsync);
    }

    public TelemetryStore Store => _store;

    public void Observe(ExperimentFrame frame)
    {
        if (_disposed)
        {
            return;
        }

        _queue.Enqueue(new QueuedFrame(Volatile.Read(ref _generation), frame));
        Interlocked.Increment(ref _published);
        var count = Interlocked.Increment(ref _backlog);
        while (count > Capacity && _queue.TryDequeue(out _))
        {
            Interlocked.Increment(ref _dropped);
            count = Interlocked.Decrement(ref _backlog);
        }
    }

    public TelemetryStatusSnapshot GetStatus() => new(
        Interlocked.Read(ref _published),
        Interlocked.Read(ref _dropped),
        Interlocked.Read(ref _projected),
        Volatile.Read(ref _backlog),
        Volatile.Read(ref _projectionMilliseconds),
        _store.Version);

    public void Reset()
    {
        lock (_projectionSync)
        {
            Interlocked.Increment(ref _generation);
            while (_queue.TryDequeue(out _))
            {
            }

            Interlocked.Exchange(ref _backlog, 0);
            Interlocked.Exchange(ref _published, 0);
            Interlocked.Exchange(ref _dropped, 0);
            Interlocked.Exchange(ref _projected, 0);
            Volatile.Write(ref _projectionMilliseconds, 0.0);
            _store.Reset();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _stop.Cancel();
        try
        {
            _projector.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // Teardown must not turn an already completed experiment into a UI fault.
        }

        _stop.Dispose();
        _store.Dispose();
    }

    private async Task ProjectLoopAsync()
    {
        var batch = new List<QueuedFrame>(ProjectionBatchSize);
        var projectedFrames = new List<ExperimentFrame>(ProjectionBatchSize);
        while (!_stop.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();
            var appliedCount = 0;
            lock (_projectionSync)
            {
                batch.Clear();
                while (batch.Count < ProjectionBatchSize && _queue.TryDequeue(out var frame))
                {
                    Interlocked.Decrement(ref _backlog);
                    batch.Add(frame);
                }

                var generation = Volatile.Read(ref _generation);
                projectedFrames.Clear();
                foreach (var item in batch)
                {
                    if (item.Generation == generation)
                    {
                        projectedFrames.Add(item.Frame);
                    }
                }

                if (projectedFrames.Count > 0)
                {
                    _store.ApplyBatch(projectedFrames);
                    appliedCount = projectedFrames.Count;
                }
            }

            stopwatch.Stop();
            if (batch.Count == 0)
            {
                await Task.Delay(4, _stop.Token).ConfigureAwait(false);
                continue;
            }

            Interlocked.Add(ref _projected, appliedCount);
            Volatile.Write(ref _projectionMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    private readonly record struct QueuedFrame(long Generation, ExperimentFrame Frame);
}
