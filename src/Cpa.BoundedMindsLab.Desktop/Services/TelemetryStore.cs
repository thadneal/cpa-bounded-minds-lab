using Cpa.BoundedMindsLab.Desktop.ViewModels;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Experiments;

namespace Cpa.BoundedMindsLab.Desktop.Services;

public sealed class TelemetryStore : IDisposable
{
    private const int MaximumTimelineItems = 750;
    private const int RawPointRetentionLimit = 32_768;
    private const int MaximumEnvelopesPerFineLevel = 4_096;
    private static readonly int[] BucketSizes = [8, 32, 128, 512, 2_048, 8_192, 32_768];
    private readonly ReaderWriterLockSlim _gate = new(LockRecursionPolicy.NoRecursion);
    private readonly Dictionary<string, SeriesHistory> _series = new(StringComparer.Ordinal);
    private readonly HashSet<string> _metrics = new(StringComparer.Ordinal);
    private readonly Queue<TelemetryTimelineItem> _timeline = new();
    private readonly Dictionary<string, string> _activeSeriesByMetric = new(StringComparer.Ordinal);
    private readonly Dictionary<string, long> _metricPlotVersions = new(StringComparer.Ordinal);
    private readonly HashSet<string> _dirtyPlotMetrics = new(StringComparer.Ordinal);
    private long _version;
    private long _catalogVersion;
    private long _timelineVersion;

    public long Version => Interlocked.Read(ref _version);

    public long CatalogVersion => Interlocked.Read(ref _catalogVersion);

    public long TimelineVersion => Interlocked.Read(ref _timelineVersion);

    public long GetMetricPlotVersion(string metric)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);

        _gate.EnterReadLock();
        try
        {
            return _metricPlotVersions.TryGetValue(metric, out var version) ? version : 0;
        }
        finally
        {
            _gate.ExitReadLock();
        }
    }

    public void ApplyBatch(IReadOnlyList<ExperimentFrame> frames)
    {
        ArgumentNullException.ThrowIfNull(frames);
        if (frames.Count == 0)
        {
            return;
        }

        _gate.EnterWriteLock();
        try
        {
            foreach (var frame in frames)
            {
                Apply(frame);
            }

            Interlocked.Increment(ref _version);
        }
        finally
        {
            _gate.ExitWriteLock();
        }
    }

    public TelemetryCatalogSnapshot GetCatalog()
    {
        _gate.EnterReadLock();
        try
        {
            return new TelemetryCatalogSnapshot(
                Interlocked.Read(ref _catalogVersion),
                _series.Values
                    .Where(history =>
                        !string.Equals(history.Key, "run/run", StringComparison.Ordinal) &&
                        history.Metrics.Count > 0)
                    .Select(history => history.Key)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                _metrics.OrderBy(value => value, StringComparer.Ordinal).ToArray());
        }
        finally
        {
            _gate.ExitReadLock();
        }
    }

    public string[] GetMetricsForSeries(string series)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(series);

        _gate.EnterReadLock();
        try
        {
            return _series.TryGetValue(series, out var history)
                ? history.Metrics.Keys.OrderBy(value => value, StringComparer.Ordinal).ToArray()
                : [];
        }
        finally
        {
            _gate.ExitReadLock();
        }
    }

    public MetricPlotSnapshot GetPlotSnapshot(
        string metric,
        string? focusSeries,
        int pixelWidth,
        int maximumSeries = 6)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);
        pixelWidth = Math.Max(64, pixelWidth);
        maximumSeries = Math.Max(1, maximumSeries);

        _gate.EnterReadLock();
        try
        {
            var candidates = _series.Values
                .Where(history => history.Metrics.ContainsKey(metric))
                .OrderByDescending(history => string.Equals(history.Key, focusSeries, StringComparison.Ordinal))
                .ThenByDescending(history => history.LastSequence)
                .Take(maximumSeries)
                .ToArray();
            var series = candidates
                .Select(history => new PlotSeriesSnapshot(
                    history.Key,
                    history.DisplayLabel,
                    history.Metrics[metric].GetDisplayPoints(pixelWidth)))
                .Where(snapshot => snapshot.Points.Count > 0)
                .ToArray();
            return new MetricPlotSnapshot(Version, metric, series);
        }
        finally
        {
            _gate.ExitReadLock();
        }
    }

    public TelemetryDetailSnapshot GetDetailSnapshot(string? series)
    {
        _gate.EnterReadLock();
        try
        {
            if (string.IsNullOrWhiteSpace(series) || !_series.TryGetValue(series, out var history))
            {
                return new TelemetryDetailSnapshot(0, series ?? string.Empty, [], []);
            }

            return new TelemetryDetailSnapshot(
                history.DetailVersion,
                history.Key,
                history.Minds.ToArray(),
                history.Traces.ToArray());
        }
        finally
        {
            _gate.ExitReadLock();
        }
    }

    public TelemetryTimelineSnapshot GetTimeline()
    {
        _gate.EnterReadLock();
        try
        {
            return new TelemetryTimelineSnapshot(Interlocked.Read(ref _timelineVersion), _timeline.ToArray());
        }
        finally
        {
            _gate.ExitReadLock();
        }
    }

    public void Reset()
    {
        _gate.EnterWriteLock();
        try
        {
            _series.Clear();
            _metrics.Clear();
            _timeline.Clear();
            _activeSeriesByMetric.Clear();
            _metricPlotVersions.Clear();
            _dirtyPlotMetrics.Clear();
            Interlocked.Increment(ref _version);
            Interlocked.Increment(ref _catalogVersion);
            Interlocked.Increment(ref _timelineVersion);
        }
        finally
        {
            _gate.ExitWriteLock();
        }
    }

    private void Apply(ExperimentFrame frame)
    {
        var key = SeriesKey(frame);
        if (!_series.TryGetValue(key, out var history))
        {
            history = new SeriesHistory(key, DisplayLabel(frame.Experiment, frame.Series));
            _series.Add(key, history);
            Interlocked.Increment(ref _catalogVersion);
        }

        history.LastSequence = frame.Sequence;
        if (frame.Metrics is { Count: > 0 } metrics)
        {
            var x = frame.Tick ?? frame.Sequence;
            foreach (var pair in metrics)
            {
                if (!double.IsFinite(pair.Value))
                {
                    continue;
                }

                if (_metrics.Add(pair.Key))
                {
                    Interlocked.Increment(ref _catalogVersion);
                }

                if (_activeSeriesByMetric.TryGetValue(pair.Key, out var activeSeries) &&
                    !string.Equals(activeSeries, key, StringComparison.Ordinal))
                {
                    CommitMetricPlot(pair.Key);
                }

                _activeSeriesByMetric[pair.Key] = key;
                _dirtyPlotMetrics.Add(pair.Key);

                if (!history.Metrics.TryGetValue(pair.Key, out var series))
                {
                    series = new MultiResolutionSeries(BucketSizes);
                    history.Metrics.Add(pair.Key, series);
                    // Path-to-metric availability is part of the selector catalog.
                    // A path may begin publishing another metric after it first appears,
                    // so that relationship needs its own invalidation.
                    Interlocked.Increment(ref _catalogVersion);
                }

                series.Append(new IndexedPoint(frame.Sequence, x, pair.Value));
            }
        }

        if (frame.Minds is { Count: > 0 })
        {
            history.Minds = frame.Minds;
            history.DetailVersion++;
        }

        if (frame.Traces is { Count: > 0 })
        {
            history.Traces = frame.Traces;
            history.DetailVersion++;
        }

        if (IsPlotCommitBoundary(frame.Kind))
        {
            CommitDirtyMetricPlots();
        }

        if (ShouldRecordTimeline(frame))
        {
            _timeline.Enqueue(new TelemetryTimelineItem(
                frame.Sequence,
                frame.Experiment,
                frame.Series,
                frame.Tick,
                frame.Kind,
                frame.Phase,
                frame.Message ?? frame.Phase ?? frame.Kind.ToString()));
            while (_timeline.Count > MaximumTimelineItems)
            {
                _timeline.Dequeue();
            }

            Interlocked.Increment(ref _timelineVersion);
        }
    }

    private void CommitMetricPlot(string metric)
    {
        if (!_dirtyPlotMetrics.Remove(metric))
        {
            return;
        }

        if (_activeSeriesByMetric.TryGetValue(metric, out var activeSeries) &&
            _series.TryGetValue(activeSeries, out var history) &&
            history.Metrics.TryGetValue(metric, out var series))
        {
            series.Commit();
        }

        _metricPlotVersions[metric] = _metricPlotVersions.TryGetValue(metric, out var version)
            ? version + 1
            : 1;
    }

    private void CommitDirtyMetricPlots()
    {
        if (_dirtyPlotMetrics.Count == 0)
        {
            return;
        }

        var dirty = _dirtyPlotMetrics.ToArray();
        foreach (var metric in dirty)
        {
            CommitMetricPlot(metric);
        }
    }

    private static bool IsPlotCommitBoundary(ExperimentFrameKind kind) => kind is
        ExperimentFrameKind.PhaseChanged or
        ExperimentFrameKind.DevelopmentalEvent or
        ExperimentFrameKind.ExperimentCompleted or
        ExperimentFrameKind.RunCompleted or
        ExperimentFrameKind.RunCancelled or
        ExperimentFrameKind.RunFaulted;

    private static string SeriesKey(ExperimentFrame frame) =>
        string.Concat(frame.Experiment, "/", frame.Series);

    private static string DisplayLabel(string experiment, string series) =>
        string.Equals(experiment, "run", StringComparison.Ordinal)
            ? series
            : $"{experiment} / {series}";

    private static bool ShouldRecordTimeline(ExperimentFrame frame) => frame.Kind is
        ExperimentFrameKind.ExperimentStarted or
        ExperimentFrameKind.PhaseChanged or
        ExperimentFrameKind.DevelopmentalEvent or
        ExperimentFrameKind.ExperimentCompleted or
        ExperimentFrameKind.RunCompleted or
        ExperimentFrameKind.RunCancelled or
        ExperimentFrameKind.RunFaulted;

    private sealed class SeriesHistory
    {
        public SeriesHistory(string key, string displayLabel)
        {
            Key = key;
            DisplayLabel = displayLabel;
        }

        public string Key { get; }

        public string DisplayLabel { get; }

        public long LastSequence { get; set; }

        public long DetailVersion { get; set; }

        public Dictionary<string, MultiResolutionSeries> Metrics { get; } = new(StringComparer.Ordinal);

        public IReadOnlyList<MindPublicState> Minds { get; set; } = [];

        public IReadOnlyList<TracePublicState> Traces { get; set; } = [];
    }

    private readonly record struct IndexedPoint(long Index, double X, double Y);

    private sealed class MultiResolutionSeries
    {
        private readonly List<IndexedPoint> _raw = [];
        private readonly EnvelopeLevel[] _levels;
        private long _count;
        private long _committedThroughIndex = long.MinValue;

        public MultiResolutionSeries(int[] bucketSizes)
        {
            _levels = bucketSizes
                .Select((size, index) => new EnvelopeLevel(
                    size,
                    index == bucketSizes.Length - 1 ? null : MaximumEnvelopesPerFineLevel))
                .ToArray();
        }

        public void Append(IndexedPoint point)
        {
            _count++;
            if (_count <= RawPointRetentionLimit)
            {
                _raw.Add(point);
            }
            else if (_count == RawPointRetentionLimit + 1L)
            {
                // The durable NDJSON journal owns full-fidelity history. Once a live
                // series is too large to be drawn raw, the UI keeps only its pyramid.
                _raw.Clear();
                _raw.TrimExcess();
            }

            foreach (var level in _levels)
            {
                level.Append(point);
            }
        }

        public void Commit()
        {
            if (_count == 0)
            {
                return;
            }

            if (_raw.Count > 0)
            {
                _committedThroughIndex = _raw[^1].Index;
                return;
            }

            _committedThroughIndex = _levels
                .Where(level => level.IsAvailable)
                .Select(level => level.LastObservedIndex)
                .DefaultIfEmpty(_committedThroughIndex)
                .Max();
        }

        public PlotPoint[] GetDisplayPoints(int pixelWidth)
        {
            if (_count == 0 || _committedThroughIndex == long.MinValue)
            {
                return [];
            }

            var pointBudget = Math.Max(128, pixelWidth * 2);
            if (_raw.Count == _count && _count <= pointBudget)
            {
                return _raw
                    .Where(point => point.Index <= _committedThroughIndex)
                    .Select(point => new PlotPoint(point.X, point.Y))
                    .ToArray();
            }

            EnvelopeLevel? selected = null;
            foreach (var level in _levels)
            {
                if (level.IsAvailable && level.EstimatedDisplayPointCount <= pointBudget)
                {
                    selected = level;
                    break;
                }
            }

            selected ??= _levels.Last(level => level.IsAvailable);
            return selected.Flatten(_committedThroughIndex);
        }
    }

    private sealed class EnvelopeLevel
    {
        private readonly int _bucketSize;
        private readonly int? _maximumCompletedEnvelopes;
        private readonly List<Envelope> _completed = [];
        private EnvelopeBuilder _current;
        private bool _retired;
        private long _lastObservedIndex = long.MinValue;

        public EnvelopeLevel(int bucketSize, int? maximumCompletedEnvelopes)
        {
            _bucketSize = bucketSize;
            _maximumCompletedEnvelopes = maximumCompletedEnvelopes;
        }

        public bool IsAvailable => !_retired;

        public long LastObservedIndex => _lastObservedIndex;

        public int EstimatedDisplayPointCount => (_completed.Count + (_current.Count > 0 ? 1 : 0)) * 4;

        public void Append(IndexedPoint point)
        {
            if (_retired)
            {
                return;
            }

            _lastObservedIndex = point.Index;
            _current.Add(point);
            if (_current.Count < _bucketSize)
            {
                return;
            }

            if (_maximumCompletedEnvelopes is { } maximum && _completed.Count >= maximum)
            {
                // This resolution can no longer fit a human display usefully. Retire
                // it while coarser levels continue representing the complete run.
                _completed.Clear();
                _completed.TrimExcess();
                _current = default;
                _retired = true;
                return;
            }

            _completed.Add(_current.Build());
            _current = default;
        }

        public PlotPoint[] Flatten(long committedThroughIndex)
        {
            if (_retired)
            {
                return [];
            }

            var points = new List<IndexedPoint>(EstimatedDisplayPointCount);
            foreach (var envelope in _completed)
            {
                AddEnvelope(points, envelope);
            }

            if (_current.Count > 0)
            {
                AddEnvelope(points, _current.Build());
            }

            return points
                .Where(point => point.Index <= committedThroughIndex)
                .OrderBy(point => point.Index)
                .DistinctBy(point => point.Index)
                .Select(point => new PlotPoint(point.X, point.Y))
                .ToArray();
        }

        private static void AddEnvelope(List<IndexedPoint> destination, Envelope envelope)
        {
            destination.Add(envelope.First);
            destination.Add(envelope.Minimum);
            destination.Add(envelope.Maximum);
            destination.Add(envelope.Last);
        }
    }

    private struct EnvelopeBuilder
    {
        private IndexedPoint _first;
        private IndexedPoint _last;
        private IndexedPoint _minimum;
        private IndexedPoint _maximum;

        public int Count { get; private set; }

        public void Add(IndexedPoint point)
        {
            if (Count == 0)
            {
                _first = point;
                _last = point;
                _minimum = point;
                _maximum = point;
                Count = 1;
                return;
            }

            _last = point;
            if (point.Y < _minimum.Y)
            {
                _minimum = point;
            }

            if (point.Y > _maximum.Y)
            {
                _maximum = point;
            }

            Count++;
        }

        public Envelope Build()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Cannot build an empty envelope.");
            }

            return new Envelope(_first, _last, _minimum, _maximum);
        }
    }

    public void Dispose()
    {
        _gate.Dispose();
    }

    private readonly record struct Envelope(
        IndexedPoint First,
        IndexedPoint Last,
        IndexedPoint Minimum,
        IndexedPoint Maximum);
}
