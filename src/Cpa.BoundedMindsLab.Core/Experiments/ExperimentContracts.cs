using Cpa.BoundedMindsLab.Domain;

namespace Cpa.BoundedMindsLab.Experiments;

public enum ExperimentVerdict
{
    Support,
    Mixed,
    Disconfirm,
    Inconclusive,
}

public sealed record ExperimentAssertion(
    string Name,
    bool Passed,
    string Description,
    double? Actual = null,
    double? Boundary = null);

public sealed record ExperimentResult(
    string Name,
    string Question,
    ExperimentVerdict Verdict,
    string Interpretation,
    IReadOnlyDictionary<string, double> Metrics,
    IReadOnlyList<ExperimentAssertion> Assertions);

public interface IExperiment
{
    string Name { get; }

    string Question { get; }

    ExperimentResult Run(ExperimentContext context);
}

public sealed class ExperimentContext
{
    private readonly IExperimentFrameObserver? _observer;
    private long _sequence;

    public ExperimentContext(
        ulong seed,
        string outputDirectory,
        bool quiet,
        IExperimentFrameObserver? observer,
        ExperimentRunControl? control)
    {
        Seed = seed;
        OutputDirectory = Path.GetFullPath(outputDirectory);
        Quiet = quiet;
        _observer = observer;
        Control = control;
    }

    public ulong Seed { get; }

    public string OutputDirectory { get; }

    public bool Quiet { get; }

    public ExperimentRunControl? Control { get; }

    public long NextSequence => _sequence;

    public void Emit(
        ExperimentFrameKind kind,
        string experiment = "run",
        string series = "run",
        int? tick = null,
        string? phase = null,
        string? message = null,
        IReadOnlyDictionary<string, double>? metrics = null,
        IReadOnlyList<MindPublicState>? minds = null,
        IReadOnlyList<TracePublicState>? traces = null,
        ExperimentCompletion? completion = null)
    {
        var frame = new ExperimentFrame(
            ExperimentFrame.CurrentSchema,
            _sequence++,
            kind,
            experiment,
            series,
            tick,
            phase,
            message,
            metrics,
            minds,
            traces,
            completion);
        _observer?.Observe(frame);
        Control?.Boundary();
    }
}

public sealed record RunResult(
    ulong Seed,
    string OutputDirectory,
    IReadOnlyList<ExperimentResult> Experiments);
