using Cpa.BoundedMindsLab.Communication;
using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Development;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed class LocalSharedMemoryContaminationExperiment : IExperiment
{
    private const int EarlyWindowTicks = 96;
    private const int LateWindowStart = 288;

    public string Name => "01-local-shared-memory-contamination";

    public string Question =>
        "Can one bounded mind gain useful prior structure from another without granting second-hand memory the authority of lived consequence?";

    public ExperimentResult Run(ExperimentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Emit(
            ExperimentFrameKind.ExperimentStarted,
            Name,
            message: Question);

        var source = DevelopSource(context);
        var packets = source.ExportPublicTraces();
        if (packets.Count != TransferContaminationWorld.ContextCount)
        {
            throw new InvalidOperationException(
                $"Source development exported {packets.Count} traces; expected {TransferContaminationWorld.ContextCount}.");
        }

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            Name,
            "source",
            phase: "public-transfer",
            message: $"Source published {packets.Count} compact trace packets; private memory remains inaccessible.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["published_trace_count"] = packets.Count,
                ["source_mean_standing"] = source.MeanLocalStanding(),
            },
            minds: [source.PublicMindState()],
            traces: source.PublicTraceStates());

        var localOnly = RunReceiverPath(context, packets, ReceiverPath.LocalOnly);
        var provisional = RunReceiverPath(context, packets, ReceiverPath.SharedProvisional);
        var livedEquivalent = RunReceiverPath(context, packets, ReceiverPath.SharedLivedEquivalent);

        var metrics = BuildResultMetrics(localOnly, provisional, livedEquivalent, packets.Count);
        var assertions = BuildAssertions(localOnly, provisional, livedEquivalent, packets.Count);
        var passed = assertions.Count(assertion => assertion.Passed);
        var verdict = passed == assertions.Count
            ? ExperimentVerdict.Support
            : passed >= 4
                ? ExperimentVerdict.Mixed
                : ExperimentVerdict.Disconfirm;
        var interpretation = verdict switch
        {
            ExperimentVerdict.Support =>
                "Second-hand traces supplied useful early structure on compatible contexts while provenance-bounded standing withdrew from locally contradictory contexts. Treating transferred history as lived history contaminated the receiver for much longer.",
            ExperimentVerdict.Mixed =>
                "Transfer showed some useful prior structure, but one or more preregistered boundaries on contamination, revision, selectivity, or cost did not hold.",
            _ =>
                "The bounded-transfer mechanism did not separate useful inheritance from contamination strongly enough to support the proposed distinction between shared memory and lived consequence.",
        };

        var result = new ExperimentResult(Name, Question, verdict, interpretation, metrics, assertions);
        context.Emit(
            ExperimentFrameKind.ExperimentCompleted,
            Name,
            phase: "verdict",
            message: interpretation,
            completion: new ExperimentCompletion(verdict, interpretation, metrics, assertions));
        return result;
    }

    private DevelopmentalMemory DevelopSource(ExperimentContext context)
    {
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            Name,
            "source",
            phase: "source-development",
            message: "Developing one bounded source mind from direct local consequence.");

        var source = new DevelopmentalMemory("source-A");
        var errors = new ErrorAccumulator();
        var schedule = TransferContaminationWorld.CreateSourceSchedule(context.Seed);
        for (var tick = 0; tick < schedule.Count; tick++)
        {
            var cell = schedule[tick];
            var target = TransferContaminationWorld.SourceTarget(cell);
            var prediction = source.Predict(cell);
            var error = prediction - target;
            errors.Add(error);
            source.ObserveDirect(cell, target);

            context.Emit(
                ExperimentFrameKind.MetricSample,
                Name,
                "source",
                tick,
                "source-development",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["context_cell"] = cell,
                    ["prediction"] = prediction,
                    ["target"] = target,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = errors.Rmse,
                    ["local_standing"] = source.StandingFor(cell, TraceProvenance.Direct),
                    ["direct_evidence"] = source.DirectEvidenceFor(cell),
                });

            if (tick % 32 == 31 || tick == schedule.Count - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    Name,
                    "source",
                    tick,
                    "source-development",
                    minds: [source.PublicMindState()],
                    traces: source.PublicTraceStates());
            }
        }

        return source;
    }

    private PathOutcome RunReceiverPath(
        ExperimentContext context,
        IReadOnlyList<PublicTracePacket> packets,
        ReceiverPath path)
    {
        var series = PathName(path);
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            Name,
            series,
            phase: "receiver-development",
            message: PathDescription(path));

        var receiver = new DevelopmentalMemory($"receiver-{series}");
        var channel = new SharedTraceChannel();
        if (path is not ReceiverPath.LocalOnly)
        {
            foreach (var packet in packets)
            {
                channel.Publish(packet);
            }

            foreach (var packet in channel.ReadPublicPackets())
            {
                if (path == ReceiverPath.SharedProvisional)
                {
                    receiver.ImportProvisional(packet);
                }
                else
                {
                    receiver.ImportAsLived(packet);
                }
            }

            context.Emit(
                ExperimentFrameKind.DevelopmentalEvent,
                Name,
                series,
                phase: "receiver-development",
                message: path == ReceiverPath.SharedProvisional
                    ? "Receiver admitted transferred traces as foreign, provisional influence."
                    : "Control admitted transferred traces with lived-equivalent authority and inherited evidential inertia.",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["communication_work"] = channel.CommunicationWork,
                    ["received_trace_count"] = channel.PacketCount,
                },
                minds: [receiver.PublicMindState()],
                traces: receiver.PublicTraceStates());
        }

        var all = new ErrorAccumulator();
        var compatible = new ErrorAccumulator();
        var divergent = new ErrorAccumulator();
        var earlyCompatible = new ErrorAccumulator();
        var earlyDivergent = new ErrorAccumulator();
        var lateDivergent = new ErrorAccumulator();
        var schedule = TransferContaminationWorld.CreateReceiverSchedule(context.Seed);

        for (var tick = 0; tick < schedule.Count; tick++)
        {
            var cell = schedule[tick];
            var target = TransferContaminationWorld.ReceiverTarget(cell);
            var prediction = receiver.Predict(cell);
            var error = prediction - target;
            all.Add(error);
            if (TransferContaminationWorld.IsTransferCompatible(cell))
            {
                compatible.Add(error);
                if (tick < EarlyWindowTicks)
                {
                    earlyCompatible.Add(error);
                }
            }
            else
            {
                divergent.Add(error);
                if (tick < EarlyWindowTicks)
                {
                    earlyDivergent.Add(error);
                }

                if (tick >= LateWindowStart)
                {
                    lateDivergent.Add(error);
                }
            }

            receiver.ObserveDirect(cell, target);
            context.Emit(
                ExperimentFrameKind.MetricSample,
                Name,
                series,
                tick,
                "receiver-development",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["context_cell"] = cell,
                    ["evaluator_transfer_compatible"] = TransferContaminationWorld.IsTransferCompatible(cell) ? 1.0 : 0.0,
                    ["prediction"] = prediction,
                    ["target"] = target,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = all.Rmse,
                    ["local_standing"] = receiver.StandingFor(cell, TraceProvenance.Direct),
                    ["foreign_standing"] = receiver.StandingFor(cell, TraceProvenance.Foreign),
                    ["mean_foreign_standing"] = receiver.MeanForeignStanding(),
                    ["communication_work"] = channel.CommunicationWork,
                    ["direct_evidence"] = receiver.DirectEvidenceFor(cell),
                });

            if (tick % 16 == 15 || tick == schedule.Count - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    Name,
                    series,
                    tick,
                    "receiver-development",
                    minds: [receiver.PublicMindState()],
                    traces: receiver.PublicTraceStates());
            }
        }

        var outcome = new PathOutcome(
            series,
            all.Rmse,
            compatible.Rmse,
            divergent.Rmse,
            earlyCompatible.Rmse,
            earlyDivergent.Rmse,
            lateDivergent.Rmse,
            receiver.MeanForeignStanding(TransferContaminationWorld.TransferCompatibleCells),
            receiver.MeanForeignStanding(TransferContaminationWorld.TransferDivergentCells),
            receiver.MeanLocalStanding(),
            channel.PacketCount,
            channel.CommunicationWork);

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            Name,
            series,
            schedule.Count - 1,
            "path-complete",
            message: $"{series} completed with RMSE {outcome.Rmse:0.000000}.",
            metrics: outcome.ToMetrics(),
            minds: [receiver.PublicMindState()],
            traces: receiver.PublicTraceStates());
        return outcome;
    }

    private static Dictionary<string, double> BuildResultMetrics(
        PathOutcome localOnly,
        PathOutcome provisional,
        PathOutcome livedEquivalent,
        int publishedTraceCount)
    {
        return new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["local_only_rmse"] = localOnly.Rmse,
            ["provisional_rmse"] = provisional.Rmse,
            ["lived_equivalent_rmse"] = livedEquivalent.Rmse,
            ["local_only_early_compatible_rmse"] = localOnly.EarlyCompatibleRmse,
            ["provisional_early_compatible_rmse"] = provisional.EarlyCompatibleRmse,
            ["provisional_early_divergent_rmse"] = provisional.EarlyDivergentRmse,
            ["lived_equivalent_early_divergent_rmse"] = livedEquivalent.EarlyDivergentRmse,
            ["provisional_late_divergent_rmse"] = provisional.LateDivergentRmse,
            ["lived_equivalent_late_divergent_rmse"] = livedEquivalent.LateDivergentRmse,
            ["provisional_final_compatible_foreign_standing"] = provisional.FinalCompatibleForeignStanding,
            ["provisional_final_divergent_foreign_standing"] = provisional.FinalDivergentForeignStanding,
            ["published_trace_count"] = publishedTraceCount,
            ["provisional_communication_work"] = provisional.CommunicationWork,
        };
    }

    private static List<ExperimentAssertion> BuildAssertions(
        PathOutcome localOnly,
        PathOutcome provisional,
        PathOutcome livedEquivalent,
        int publishedTraceCount)
    {
        return
        [
            new ExperimentAssertion(
                "compatible-transfer-benefit",
                provisional.EarlyCompatibleRmse <= localOnly.EarlyCompatibleRmse * 0.25,
                "Provisional second-hand memory should materially reduce early error where the source and receiver environments agree.",
                provisional.EarlyCompatibleRmse,
                localOnly.EarlyCompatibleRmse * 0.25),
            new ExperimentAssertion(
                "bounded-contamination",
                provisional.EarlyDivergentRmse <= livedEquivalent.EarlyDivergentRmse * 0.60,
                "Provenance-bounded transfer should contaminate divergent contexts less than treating another mind's history as lived experience.",
                provisional.EarlyDivergentRmse,
                livedEquivalent.EarlyDivergentRmse * 0.60),
            new ExperimentAssertion(
                "late-local-revision",
                provisional.LateDivergentRmse <= livedEquivalent.LateDivergentRmse * 0.20,
                "Direct local consequence should revise away incompatible foreign influence much faster than lived-equivalent transfer.",
                provisional.LateDivergentRmse,
                livedEquivalent.LateDivergentRmse * 0.20),
            new ExperimentAssertion(
                "provenance-selectivity",
                provisional.FinalCompatibleForeignStanding >= 0.75 && provisional.FinalDivergentForeignStanding <= 0.10,
                "Foreign standing should remain high where later local consequence confirms it and collapse where local consequence contradicts it.",
                provisional.FinalCompatibleForeignStanding - provisional.FinalDivergentForeignStanding,
                0.65),
            new ExperimentAssertion(
                "overall-usefulness",
                provisional.Rmse <= localOnly.Rmse * 1.05,
                "The useful prior should compensate for its bounded contamination over the whole receiver history.",
                provisional.Rmse,
                localOnly.Rmse * 1.05),
            new ExperimentAssertion(
                "bounded-communication",
                provisional.PacketCount == publishedTraceCount && provisional.CommunicationWork <= 4.0,
                "The transfer must use only the compact public trace packets under explicit finite communication cost.",
                provisional.CommunicationWork,
                4.0),
        ];
    }

    private static string PathName(ReceiverPath path) => path switch
    {
        ReceiverPath.LocalOnly => "local-only",
        ReceiverPath.SharedProvisional => "shared-provisional",
        ReceiverPath.SharedLivedEquivalent => "shared-lived-equivalent",
        _ => throw new ArgumentOutOfRangeException(nameof(path)),
    };

    private static string PathDescription(ReceiverPath path) => path switch
    {
        ReceiverPath.LocalOnly =>
            "Receiver develops only from its own local consequence. This is the no-transfer baseline.",
        ReceiverPath.SharedProvisional =>
            "Receiver admits compact public traces as explicitly foreign and provisional, then lets local consequence revise their standing.",
        ReceiverPath.SharedLivedEquivalent =>
            "Control copies the same public traces into local memory with the source's accumulated authority and evidence inertia.",
        _ => throw new ArgumentOutOfRangeException(nameof(path)),
    };

    private enum ReceiverPath
    {
        LocalOnly,
        SharedProvisional,
        SharedLivedEquivalent,
    }

    private sealed record PathOutcome(
        string Series,
        double Rmse,
        double CompatibleRmse,
        double DivergentRmse,
        double EarlyCompatibleRmse,
        double EarlyDivergentRmse,
        double LateDivergentRmse,
        double FinalCompatibleForeignStanding,
        double FinalDivergentForeignStanding,
        double FinalLocalStanding,
        int PacketCount,
        double CommunicationWork)
    {
        public Dictionary<string, double> ToMetrics() => new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["rmse"] = Rmse,
            ["compatible_rmse"] = CompatibleRmse,
            ["divergent_rmse"] = DivergentRmse,
            ["early_compatible_rmse"] = EarlyCompatibleRmse,
            ["early_divergent_rmse"] = EarlyDivergentRmse,
            ["late_divergent_rmse"] = LateDivergentRmse,
            ["final_compatible_foreign_standing"] = FinalCompatibleForeignStanding,
            ["final_divergent_foreign_standing"] = FinalDivergentForeignStanding,
            ["final_local_standing"] = FinalLocalStanding,
            ["communication_work"] = CommunicationWork,
            ["packet_count"] = PacketCount,
        };
    }
}
