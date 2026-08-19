namespace Cpa.BoundedMindsLab.Experiments;

public sealed class ExperimentRunControl : IDisposable
{
    private readonly object _sync = new();
    private readonly CancellationTokenSource _cancellation = new();
    private bool _paused;
    private int _stepPermits;
    private bool _disposed;

    public ExperimentRunControl(int boundaryDelayMilliseconds = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(boundaryDelayMilliseconds);
        BoundaryDelayMilliseconds = boundaryDelayMilliseconds;
    }

    public int BoundaryDelayMilliseconds { get; }

    public bool IsCancellationRequested => _cancellation.IsCancellationRequested;

    public void Pause()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            _paused = true;
            _stepPermits = 0;
        }
    }

    public void Resume()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            _paused = false;
            _stepPermits = 0;
            Monitor.PulseAll(_sync);
        }
    }

    public void Step()
    {
        lock (_sync)
        {
            ThrowIfDisposed();
            _paused = true;
            _stepPermits++;
            Monitor.PulseAll(_sync);
        }
    }

    public void Cancel()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _cancellation.Cancel();
            Monitor.PulseAll(_sync);
        }
    }

    public void Boundary()
    {
        _cancellation.Token.ThrowIfCancellationRequested();
        lock (_sync)
        {
            while (_paused && _stepPermits == 0 && !_cancellation.IsCancellationRequested)
            {
                Monitor.Wait(_sync, TimeSpan.FromMilliseconds(50));
            }

            _cancellation.Token.ThrowIfCancellationRequested();
            if (_paused && _stepPermits > 0)
            {
                _stepPermits--;
            }
        }

        if (BoundaryDelayMilliseconds > 0)
        {
            Thread.Sleep(BoundaryDelayMilliseconds);
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _cancellation.Cancel();
            Monitor.PulseAll(_sync);
        }

        _cancellation.Dispose();
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}
