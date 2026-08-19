using Cpa.BoundedMindsLab.Domain;

namespace Cpa.BoundedMindsLab.Experiments;

public enum ExperimentFrameKind
{
    RunStarted,
    ExperimentStarted,
    PhaseChanged,
    MetricSample,
    StateSnapshot,
    DevelopmentalEvent,
    ExperimentCompleted,
    RunCompleted,
    RunCancelled,
    RunFaulted,
}

public sealed record ExperimentCompletion(
    ExperimentVerdict Verdict,
    string Interpretation,
    IReadOnlyDictionary<string, double> Metrics,
    IReadOnlyList<ExperimentAssertion> Assertions);

public sealed record ExperimentFrame(
    string Schema,
    long Sequence,
    ExperimentFrameKind Kind,
    string Experiment,
    string Series,
    int? Tick,
    string? Phase,
    string? Message,
    IReadOnlyDictionary<string, double>? Metrics,
    IReadOnlyList<MindPublicState>? Minds,
    IReadOnlyList<TracePublicState>? Traces,
    ExperimentCompletion? Completion)
{
    public const string CurrentSchema = "cpa-bounded-minds-frame-v1";
}

public interface IExperimentFrameObserver
{
    void Observe(ExperimentFrame frame);
}
