using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Experiments;

namespace Cpa.BoundedMindsLab.Desktop.ViewModels;

public readonly record struct PlotPoint(double X, double Y);

public sealed record PlotSeriesSnapshot(
    string Key,
    string Label,
    IReadOnlyList<PlotPoint> Points);

public sealed record MetricPlotSnapshot(
    long Version,
    string Metric,
    IReadOnlyList<PlotSeriesSnapshot> Series);

public sealed record TelemetryCatalogSnapshot(
    long Version,
    IReadOnlyList<string> Series,
    IReadOnlyList<string> Metrics);

public sealed record TelemetryDetailSnapshot(
    long Version,
    string Series,
    IReadOnlyList<MindPublicState> Minds,
    IReadOnlyList<TracePublicState> Traces);

public sealed record TelemetryTimelineItem(
    long Sequence,
    string Experiment,
    string Series,
    int? Tick,
    ExperimentFrameKind Kind,
    string? Phase,
    string Text);

public sealed record TelemetryTimelineSnapshot(
    long Version,
    IReadOnlyList<TelemetryTimelineItem> Items);

public sealed record TelemetryStatusSnapshot(
    long PublishedFrames,
    long DroppedFrames,
    long ProjectedFrames,
    int Backlog,
    double ProjectionMilliseconds,
    long StoreVersion);
