namespace Cpa.BoundedMindsLab.Falsification;

public sealed record FalsificationAxis(
    string Name,
    string Label,
    string Description,
    double[] Values);

public sealed record FalsificationProfileDefinition(
    string Id,
    string Name,
    string Protocol,
    string Question,
    string Method,
    FalsificationAxis XAxis,
    FalsificationAxis YAxis,
    int Replicates,
    Func<double, double, ulong, Dictionary<string, double>> Evaluate,
    string PrimaryMarginMetric,
    string PrimaryMarginDescription,
    string InterpretationLimit);

public sealed record FalsificationCellResult(
    string ProfileId,
    string Protocol,
    double X,
    double Y,
    int Replicates,
    double MeanPrimaryMargin,
    double MinimumPrimaryMargin,
    double MaximumPrimaryMargin,
    int NegativeMargins,
    IReadOnlyDictionary<string, double> MeanMetrics);

public sealed record FalsificationProfileReport(
    string Id,
    string Name,
    string Protocol,
    string Question,
    string Method,
    FalsificationAxis XAxis,
    FalsificationAxis YAxis,
    string PrimaryMarginMetric,
    string PrimaryMarginDescription,
    string InterpretationLimit,
    IReadOnlyList<FalsificationCellResult> Cells,
    int CellsWithNegativeMeanMargin,
    int CellsWithAnyNegativeReplicate,
    double MinimumObservedMargin,
    double MaximumObservedMargin);

public sealed record FalsificationReport(
    string Schema,
    string Version,
    string Name,
    int Profiles,
    int Cells,
    int ReplicateRuns,
    IReadOnlyList<FalsificationProfileReport> Results,
    IReadOnlyList<string> Diagnostics);
