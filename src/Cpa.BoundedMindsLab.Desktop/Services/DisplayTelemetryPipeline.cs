using System.Collections.Concurrent;
using System.Diagnostics;
using Cpa.BoundedMindsLab.Desktop.ViewModels;
using Cpa.BoundedMindsLab.Experiments;

namespace Cpa.BoundedMindsLab.Desktop.Services;

public sealed class DisplayTelemetryPipeline : IExperimentFrameObserver, IDisposable
{
    private const int Capacity = 16_384;
    private const int ProjectionBatchSize = 512;
    private readonly ConcurrentQueue<QueuedFrame> _queue = new();
    private readonly object _projectionSync = new();
    private readonly CancellationTokenSource _stop = new();
    private readonly Dictionary<ulong, TelemetryStore> _seedStores = [];
    private readonly List<ExperimentFrame> _projectionBatch = new(ProjectionBatchSize);
    private readonly List<ulong> _seedOrder = [];
    private TelemetryStore _store = new();
    private readonly Task _projector;
    private long _published;
    private long _dropped;
    private long _projected;
    private int _backlog;
    private double _projectionMilliseconds;
    private long _generation;
    private ulong? _activeSeed;
    private bool _disposed;

    public DisplayTelemetryPipeline()
    {
        _projector = Task.Run(ProjectLoopAsync);
    }

    public TelemetryStore Store
    {
        get
        {
            lock (_projectionSync)
            {
                return _store;
            }
        }
    }

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
        Store.Version);

    public ulong[] GetAvailableSeeds()
    {
        lock (_projectionSync)
        {
            return _seedOrder.ToArray();
        }
    }

    public TelemetryStore GetStore(ulong seed)
    {
        lock (_projectionSync)
        {
            if (_activeSeed == seed)
            {
                return _store;
            }

            return _seedStores.TryGetValue(seed, out var store)
                ? store
                : _store;
        }
    }

    public bool ContainsSeed(ulong seed)
    {
        lock (_projectionSync)
        {
            return _activeSeed == seed || _seedStores.ContainsKey(seed);
        }
    }

    public void Flush()
    {
        lock (_projectionSync)
        {
            while (ProjectQueuedFramesLocked() > 0)
            {
            }
        }
    }

    public void Reset()
    {
        lock (_projectionSync)
        {
            AdvanceGenerationAndDrainQueue();
            foreach (var store in _seedStores.Values)
            {
                store.Dispose();
            }

            _seedStores.Clear();
            _seedOrder.Clear();
            _activeSeed = null;
            _store.Dispose();
            _store = new TelemetryStore();
            ResetCounters();
        }
    }

    public void BeginSeed(ulong seed)
    {
        lock (_projectionSync)
        {
            while (ProjectQueuedFramesLocked() > 0)
            {
            }

            Interlocked.Increment(ref _generation);

            if (_activeSeed is { } previousSeed)
            {
                _seedStores[previousSeed] = _store;
            }
            else
            {
                _store.Dispose();
            }

            if (!_seedOrder.Contains(seed))
            {
                _seedOrder.Add(seed);
            }

            if (_seedStores.Remove(seed, out var replaced))
            {
                replaced.Dispose();
            }

            _store = new TelemetryStore();
            _activeSeed = seed;
            ResetCounters();
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

        lock (_projectionSync)
        {
            foreach (var store in _seedStores.Values)
            {
                store.Dispose();
            }

            _seedStores.Clear();
            _seedOrder.Clear();
            _store.Dispose();
        }

        _stop.Dispose();
    }

    private async Task ProjectLoopAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            var stopwatch = Stopwatch.StartNew();
            var batchCount = 0;
            lock (_projectionSync)
            {
                batchCount = ProjectQueuedFramesLocked();
            }

            stopwatch.Stop();
            if (batchCount == 0)
            {
                await Task.Delay(4, _stop.Token).ConfigureAwait(false);
                continue;
            }

            Volatile.Write(ref _projectionMilliseconds, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    private int ProjectQueuedFramesLocked()
    {
        var generation = Volatile.Read(ref _generation);
        _projectionBatch.Clear();
        var dequeued = 0;
        while (dequeued < ProjectionBatchSize && _queue.TryDequeue(out var item))
        {
            Interlocked.Decrement(ref _backlog);
            dequeued++;
            if (item.Generation == generation)
            {
                _projectionBatch.Add(item.Frame);
            }
        }

        if (_projectionBatch.Count > 0)
        {
            _store.ApplyBatch(_projectionBatch);
            Interlocked.Add(ref _projected, _projectionBatch.Count);
        }

        return dequeued;
    }

    private void AdvanceGenerationAndDrainQueue()
    {
        Interlocked.Increment(ref _generation);
        while (_queue.TryDequeue(out _))
        {
        }

        Interlocked.Exchange(ref _backlog, 0);
    }

    private void ResetCounters()
    {
        Interlocked.Exchange(ref _published, 0);
        Interlocked.Exchange(ref _dropped, 0);
        Interlocked.Exchange(ref _projected, 0);
        Interlocked.Exchange(ref _backlog, 0);
        Volatile.Write(ref _projectionMilliseconds, 0.0);
    }

    private readonly record struct QueuedFrame(long Generation, ExperimentFrame Frame);
}
