using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed class DevelopmentalVersusDoctrinalTransferExperiment : IExperiment
{
    private const string ExperimentName = "03-developmental-versus-doctrinal-transfer";
    private const int EarlyWindowTicks = 80;
    private const int LateWindowStart = 180;
    private const double DoctrineStanding = 0.68;
    private const double DoctrinePacketCost = 0.08;
    private const double DevelopmentalPacketCost = 0.24;

    public string Name => ExperimentName;

    public string Question =>
        "Does transferring bounded developmental consequence history help a receiver calibrate second-hand knowledge better than transferring only a final rule when seeds generate meaningfully different lived histories?";

    public ExperimentResult Run(ExperimentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Emit(
            ExperimentFrameKind.ExperimentStarted,
            Name,
            message: Question);

        var scenario = DevelopmentalTransferWorld.CreateScenario(context.Seed);
        EmitScenario(context, scenario);
        var source = DevelopSource(context, scenario);
        var developmentalPackets = source.ExportDevelopmentalPackets(scenario);
        var doctrinalPackets = CreateDoctrinalPackets(developmentalPackets);
        EmitTransferPrepared(context, scenario, developmentalPackets, doctrinalPackets);

        var receiverObservations = DevelopmentalTransferWorld.CreateReceiverObservations(scenario);
        var localReceiver = new ReceiverMind("local-only", scenario.Cells.Count);
        var localOnly = RunReceiverPath(
            context,
            scenario,
            receiverObservations,
            localReceiver,
            0.0,
            0,
            "Receiver develops only from direct local consequence.");

        var developmentalReceiver = new ReceiverMind("developmental-transfer", scenario.Cells.Count);
        developmentalReceiver.ImportDevelopmental(developmentalPackets);
        var developmental = RunReceiverPath(
            context,
            scenario,
            receiverObservations,
            developmentalReceiver,
            developmentalPackets.Length * DevelopmentalPacketCost,
            developmentalPackets.Length,
            "Receiver imports a bounded consequence-history packet per source context. Evidence depth and history stability may change initial foreign standing.");

        var doctrinalReceiver = new ReceiverMind("doctrinal-transfer", scenario.Cells.Count);
        doctrinalReceiver.ImportDoctrine(doctrinalPackets);
        var doctrinal = RunReceiverPath(
            context,
            scenario,
            receiverObservations,
            doctrinalReceiver,
            doctrinalPackets.Length * DoctrinePacketCost,
            doctrinalPackets.Length,
            "Receiver imports only the source's final rule per context under one undifferentiated foreign standing. This is the compression control.");

        var metrics = BuildResultMetrics(scenario, developmentalPackets, localOnly, developmental, doctrinal);
        var assertions = BuildAssertions(scenario, developmentalPackets, localOnly, developmental, doctrinal);
        var passed = assertions.Count(assertion => assertion.Passed);
        var verdict = passed == assertions.Count
            ? ExperimentVerdict.Support
            : passed >= 5
                ? ExperimentVerdict.Mixed
                : ExperimentVerdict.Disconfirm;
        var interpretation = verdict switch
        {
            ExperimentVerdict.Support =>
                "Bounded developmental transfer used source consequence history to calibrate how much a foreign rule should matter. It retained a strong head start from stable compatible histories, reduced contamination from unstable histories relative to doctrinal transfer, remained no worse overall than doctrine, and still let direct local consequence revoke stable-but-locally-wrong foreign authority.",
            ExperimentVerdict.Mixed =>
                "Developmental history carried useful calibration signal, but one or more preregistered boundaries on stable transfer benefit, unstable contamination, total error, selective revision, scenario heterogeneity, or communication cost did not hold.",
            _ =>
                "The bounded developmental history packet did not provide enough advantage over final-rule transfer in this varied-history assay to support the claim that developmental context improves second-hand calibration.",
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

    private static void EmitScenario(ExperimentContext context, DevelopmentalTransferScenario scenario)
    {
        var stableCompatible = CountHistoryKind(scenario, SourceHistoryKind.StableCompatible);
        var stableDivergent = CountHistoryKind(scenario, SourceHistoryKind.StableDivergent);
        var unstable = CountHistoryKind(scenario, SourceHistoryKind.UnstableTransition);
        var sparse = CountHistoryKind(scenario, SourceHistoryKind.SparseAmbiguous);
        var minimumEvidence = int.MaxValue;
        var maximumEvidence = int.MinValue;
        for (var index = 0; index < scenario.Cells.Count; index++)
        {
            minimumEvidence = Math.Min(minimumEvidence, scenario.Cells[index].SourceEvidenceCount);
            maximumEvidence = Math.Max(maximumEvidence, scenario.Cells[index].SourceEvidenceCount);
        }

        var description = string.Join(
            "; ",
            scenario.Cells.Select(cell => $"c{cell.ContextCell}:{cell.HistoryKind}/n{cell.SourceEvidenceCount}"));
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "scenario",
            phase: "scenario-generated",
            message: $"Seed {scenario.Seed} generated a lived-history circumstance rather than merely a shuffled fixed curriculum: {description}.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["stable_compatible_cells"] = stableCompatible,
                ["stable_divergent_cells"] = stableDivergent,
                ["unstable_transition_cells"] = unstable,
                ["sparse_ambiguous_cells"] = sparse,
                ["minimum_source_evidence"] = minimumEvidence,
                ["maximum_source_evidence"] = maximumEvidence,
                ["source_evidence_span"] = maximumEvidence - minimumEvidence,
            });
    }

    private static SourceMind DevelopSource(ExperimentContext context, DevelopmentalTransferScenario scenario)
    {
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            "source",
            phase: "source-development",
            message: "The source develops from a seed-specific mixture of stable, divergent, unstable, and sparse consequence histories.");

        var source = new SourceMind(scenario.Cells.Count);
        var errors = new ErrorAccumulator();
        var observations = DevelopmentalTransferWorld.CreateSourceObservations(scenario);
        for (var tick = 0; tick < observations.Length; tick++)
        {
            var observation = observations[tick];
            var prediction = source.Predict(observation.ContextCell);
            var error = prediction - observation.Target;
            errors.Add(error);
            source.Observe(observation.ContextCell, observation.Target);
            var cell = scenario.Cells[observation.ContextCell];

            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                "source",
                tick,
                "source-development",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["context_cell"] = observation.ContextCell,
                    ["history_kind"] = (double)cell.HistoryKind,
                    ["prediction"] = prediction,
                    ["target"] = observation.Target,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = errors.Rmse,
                    ["source_evidence"] = source.EvidenceFor(observation.ContextCell),
                });
        }

        return source;
    }

    private static void EmitTransferPrepared(
        ExperimentContext context,
        DevelopmentalTransferScenario scenario,
        DevelopmentalTransferPacket[] developmentalPackets,
        DoctrinalRulePacket[] doctrinalPackets)
    {
        var stableStanding = MeanDevelopmentalStanding(scenario, developmentalPackets, SourceHistoryKind.StableCompatible);
        var unstableStanding = MeanDevelopmentalStanding(scenario, developmentalPackets, SourceHistoryKind.UnstableTransition);
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "source",
            phase: "transfer-prepared",
            message: "Two bounded public transfer surfaces are prepared from the same source history: a final-rule doctrine and a developmental packet carrying evidence depth plus three selected consequence-history segment means.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["developmental_packet_count"] = developmentalPackets.Length,
                ["doctrinal_packet_count"] = doctrinalPackets.Length,
                ["developmental_communication_work"] = developmentalPackets.Length * DevelopmentalPacketCost,
                ["doctrinal_communication_work"] = doctrinalPackets.Length * DoctrinePacketCost,
                ["stable_developmental_initial_standing"] = stableStanding,
                ["unstable_developmental_initial_standing"] = unstableStanding,
                ["developmental_standing_separation"] = stableStanding - unstableStanding,
            });
    }

    private static PathOutcome RunReceiverPath(
        ExperimentContext context,
        DevelopmentalTransferScenario scenario,
        DevelopmentalTransferObservation[] observations,
        ReceiverMind receiver,
        double communicationWork,
        int communicationPacketCount,
        string description)
    {
        var series = receiver.MindId;
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            series,
            phase: "receiver-development",
            message: description);

        var allErrors = new ErrorAccumulator();
        var earlyErrors = new ErrorAccumulator();
        var lateErrors = new ErrorAccumulator();
        var stableCompatibleEarlyErrors = new ErrorAccumulator();
        var unstableEarlyErrors = new ErrorAccumulator();
        for (var tick = 0; tick < observations.Length; tick++)
        {
            var observation = observations[tick];
            // HistoryKind is evaluator-only stratification metadata. It is never passed into ReceiverMind.
            var cell = scenario.Cells[observation.ContextCell];
            var prediction = receiver.Predict(observation.ContextCell);
            var error = prediction - observation.Target;
            allErrors.Add(error);
            if (tick < EarlyWindowTicks)
            {
                earlyErrors.Add(error);
                if (DevelopmentalTransferWorld.IsStableCompatible(cell.HistoryKind))
                {
                    stableCompatibleEarlyErrors.Add(error);
                }

                if (DevelopmentalTransferWorld.IsUnstable(cell.HistoryKind))
                {
                    unstableEarlyErrors.Add(error);
                }
            }

            if (tick >= LateWindowStart)
            {
                lateErrors.Add(error);
            }

            receiver.ObserveDirect(observation.ContextCell, observation.Target);
            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                series,
                tick,
                "receiver-development",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["context_cell"] = observation.ContextCell,
                    ["history_kind"] = (double)cell.HistoryKind,
                    ["prediction"] = prediction,
                    ["target"] = observation.Target,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = allErrors.Rmse,
                    ["local_evidence"] = receiver.LocalEvidenceFor(observation.ContextCell),
                    ["foreign_standing"] = receiver.ForeignStandingFor(observation.ContextCell),
                });

            if (tick % 40 == 39 || tick == observations.Length - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    ExperimentName,
                    series,
                    tick,
                    "receiver-development",
                    minds: [receiver.PublicMindState()],
                    traces: receiver.PublicTraceStates());
            }
        }

        var outcome = new PathOutcome(
            series,
            allErrors.Rmse,
            earlyErrors.Rmse,
            lateErrors.Rmse,
            stableCompatibleEarlyErrors.Rmse,
            unstableEarlyErrors.Rmse,
            MeanFinalStanding(scenario, receiver, SourceHistoryKind.StableCompatible),
            MeanFinalStanding(scenario, receiver, SourceHistoryKind.StableDivergent),
            communicationWork,
            communicationPacketCount);
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            series,
            observations.Length,
            "path-complete",
            message: $"{series} complete.",
            metrics: outcome.ToMetrics(),
            minds: [receiver.PublicMindState()],
            traces: receiver.PublicTraceStates());
        return outcome;
    }

    private static Dictionary<string, double> BuildResultMetrics(
        DevelopmentalTransferScenario scenario,
        DevelopmentalTransferPacket[] developmentalPackets,
        PathOutcome localOnly,
        PathOutcome developmental,
        PathOutcome doctrinal)
    {
        var minimumEvidence = int.MaxValue;
        var maximumEvidence = int.MinValue;
        for (var index = 0; index < scenario.Cells.Count; index++)
        {
            minimumEvidence = Math.Min(minimumEvidence, scenario.Cells[index].SourceEvidenceCount);
            maximumEvidence = Math.Max(maximumEvidence, scenario.Cells[index].SourceEvidenceCount);
        }

        var stableInitialStanding = MeanDevelopmentalStanding(scenario, developmentalPackets, SourceHistoryKind.StableCompatible);
        var unstableInitialStanding = MeanDevelopmentalStanding(scenario, developmentalPackets, SourceHistoryKind.UnstableTransition);
        return new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
            ["source_evidence_span"] = maximumEvidence - minimumEvidence,
            ["stable_compatible_cells"] = CountHistoryKind(scenario, SourceHistoryKind.StableCompatible),
            ["stable_divergent_cells"] = CountHistoryKind(scenario, SourceHistoryKind.StableDivergent),
            ["unstable_transition_cells"] = CountHistoryKind(scenario, SourceHistoryKind.UnstableTransition),
            ["sparse_ambiguous_cells"] = CountHistoryKind(scenario, SourceHistoryKind.SparseAmbiguous),
            ["stable_developmental_initial_standing"] = stableInitialStanding,
            ["unstable_developmental_initial_standing"] = unstableInitialStanding,
            ["developmental_standing_separation"] = stableInitialStanding - unstableInitialStanding,
            ["local_only_rmse"] = localOnly.Rmse,
            ["developmental_rmse"] = developmental.Rmse,
            ["doctrinal_rmse"] = doctrinal.Rmse,
            ["local_only_early_rmse"] = localOnly.EarlyRmse,
            ["developmental_early_rmse"] = developmental.EarlyRmse,
            ["doctrinal_early_rmse"] = doctrinal.EarlyRmse,
            ["local_stable_compatible_early_rmse"] = localOnly.StableCompatibleEarlyRmse,
            ["developmental_stable_compatible_early_rmse"] = developmental.StableCompatibleEarlyRmse,
            ["doctrinal_stable_compatible_early_rmse"] = doctrinal.StableCompatibleEarlyRmse,
            ["developmental_unstable_early_rmse"] = developmental.UnstableEarlyRmse,
            ["doctrinal_unstable_early_rmse"] = doctrinal.UnstableEarlyRmse,
            ["developmental_late_rmse"] = developmental.LateRmse,
            ["developmental_final_stable_standing"] = developmental.FinalStableCompatibleStanding,
            ["developmental_final_divergent_standing"] = developmental.FinalStableDivergentStanding,
            ["developmental_communication_work"] = developmental.CommunicationWork,
            ["doctrinal_communication_work"] = doctrinal.CommunicationWork,
            ["developmental_packet_count"] = developmental.CommunicationPacketCount,
            ["doctrinal_packet_count"] = doctrinal.CommunicationPacketCount,
        };
    }

    private static List<ExperimentAssertion> BuildAssertions(
        DevelopmentalTransferScenario scenario,
        DevelopmentalTransferPacket[] developmentalPackets,
        PathOutcome localOnly,
        PathOutcome developmental,
        PathOutcome doctrinal)
    {
        var stableInitialStanding = MeanDevelopmentalStanding(scenario, developmentalPackets, SourceHistoryKind.StableCompatible);
        var unstableInitialStanding = MeanDevelopmentalStanding(scenario, developmentalPackets, SourceHistoryKind.UnstableTransition);
        var minimumEvidence = int.MaxValue;
        var maximumEvidence = int.MinValue;
        for (var index = 0; index < scenario.Cells.Count; index++)
        {
            minimumEvidence = Math.Min(minimumEvidence, scenario.Cells[index].SourceEvidenceCount);
            maximumEvidence = Math.Max(maximumEvidence, scenario.Cells[index].SourceEvidenceCount);
        }

        return
        [
            new ExperimentAssertion(
                "seed-generates-lived-circumstance",
                CountHistoryKind(scenario, SourceHistoryKind.StableCompatible) >= 3 &&
                CountHistoryKind(scenario, SourceHistoryKind.StableDivergent) >= 2 &&
                CountHistoryKind(scenario, SourceHistoryKind.UnstableTransition) >= 3 &&
                CountHistoryKind(scenario, SourceHistoryKind.SparseAmbiguous) >= 1 &&
                maximumEvidence - minimumEvidence >= 20,
                "A replication seed must vary developmental circumstance itself: the source history must contain compatible, locally divergent, unstable, and materially unequal evidence depths rather than only reorder a fixed curriculum.",
                maximumEvidence - minimumEvidence,
                20.0),
            new ExperimentAssertion(
                "developmental-history-calibrates-standing",
                stableInitialStanding - unstableInitialStanding >= 0.35,
                "Selected consequence history should let the receiver grant substantially more initial standing to stable source histories than to histories whose own consequence record is unstable.",
                stableInitialStanding - unstableInitialStanding,
                0.35),
            new ExperimentAssertion(
                "stable-history-head-start",
                developmental.StableCompatibleEarlyRmse <= localOnly.StableCompatibleEarlyRmse * 0.55,
                "Developmental transfer must preserve a real early benefit where the source history was stable and compatible instead of becoming caution so strong that useful inheritance disappears.",
                developmental.StableCompatibleEarlyRmse,
                localOnly.StableCompatibleEarlyRmse * 0.55),
            new ExperimentAssertion(
                "unstable-history-contamination-reduced",
                developmental.UnstableEarlyRmse <= doctrinal.UnstableEarlyRmse * 0.95,
                "Where the source's own consequence history was unstable, carrying that history should reduce early contamination compared with importing only its final rule.",
                developmental.UnstableEarlyRmse,
                doctrinal.UnstableEarlyRmse * 0.95),
            new ExperimentAssertion(
                "whole-history-noninferiority-to-doctrine",
                developmental.Rmse <= doctrinal.Rmse,
                "The added developmental context must not buy local calibration by producing greater total receiver error than the cheaper doctrinal transfer.",
                developmental.Rmse,
                doctrinal.Rmse),
            new ExperimentAssertion(
                "direct-consequence-remains-sovereign",
                developmental.LateRmse <= 0.12 &&
                developmental.FinalStableCompatibleStanding >= 0.75 &&
                developmental.FinalStableDivergentStanding <= 0.02,
                "Developmental transfer must remain revisable: direct receiver consequence should retain confirmed foreign structure, extinguish stable-but-locally-wrong foreign authority, and return late error to a low range.",
                developmental.FinalStableCompatibleStanding - developmental.FinalStableDivergentStanding,
                0.73),
            new ExperimentAssertion(
                "bounded-developmental-transfer",
                developmental.CommunicationPacketCount == DevelopmentalTransferWorld.ContextCount &&
                doctrinal.CommunicationPacketCount == DevelopmentalTransferWorld.ContextCount &&
                developmental.CommunicationWork <= 2.5 &&
                doctrinal.CommunicationWork < developmental.CommunicationWork,
                "Developmental context is allowed to cost more than doctrine, but it must cross a compact fixed public surface under an explicit finite communication budget.",
                developmental.CommunicationWork,
                2.5),
        ];
    }

    private static int CountHistoryKind(DevelopmentalTransferScenario scenario, SourceHistoryKind historyKind)
    {
        var count = 0;
        for (var index = 0; index < scenario.Cells.Count; index++)
        {
            if (scenario.Cells[index].HistoryKind == historyKind)
            {
                count++;
            }
        }

        return count;
    }

    private static DoctrinalRulePacket[] CreateDoctrinalPackets(DevelopmentalTransferPacket[] developmentalPackets)
    {
        var packets = new DoctrinalRulePacket[developmentalPackets.Length];
        for (var index = 0; index < developmentalPackets.Length; index++)
        {
            var developmentalPacket = developmentalPackets[index];
            packets[index] = new DoctrinalRulePacket(developmentalPacket.ContextCell, developmentalPacket.RuleEstimate);
        }

        return packets;
    }

    private static double MeanDevelopmentalStanding(
        DevelopmentalTransferScenario scenario,
        DevelopmentalTransferPacket[] developmentalPackets,
        SourceHistoryKind historyKind)
    {
        var sum = 0.0;
        var count = 0;
        for (var index = 0; index < scenario.Cells.Count; index++)
        {
            if (scenario.Cells[index].HistoryKind == historyKind)
            {
                sum += CalculateDevelopmentalStanding(developmentalPackets[index]);
                count++;
            }
        }

        return count == 0 ? 0.0 : sum / count;
    }

    private static double MeanFinalStanding(
        DevelopmentalTransferScenario scenario,
        ReceiverMind receiver,
        SourceHistoryKind historyKind)
    {
        var sum = 0.0;
        var count = 0;
        for (var index = 0; index < scenario.Cells.Count; index++)
        {
            if (scenario.Cells[index].HistoryKind == historyKind)
            {
                sum += receiver.ForeignStandingFor(index);
                count++;
            }
        }

        return count == 0 ? 0.0 : sum / count;
    }

    private static double CalculateDevelopmentalStanding(DevelopmentalTransferPacket packet)
    {
        var segmentMinimum = Math.Min(packet.EarlySegmentMean, Math.Min(packet.MiddleSegmentMean, packet.LateSegmentMean));
        var segmentMaximum = Math.Max(packet.EarlySegmentMean, Math.Max(packet.MiddleSegmentMean, packet.LateSegmentMean));
        var segmentSpread = segmentMaximum - segmentMinimum;
        var evidenceFactor = 1.0 - Math.Exp(-packet.SourceEvidenceCount / 12.0);
        var consistency = Math.Exp(-4.0 * (packet.WithinHistoryStandardDeviation + (0.8 * segmentSpread)));
        return Math.Min(0.72, 0.08 + (0.68 * evidenceFactor * consistency));
    }

    private sealed record DevelopmentalTransferPacket(
        int ContextCell,
        double RuleEstimate,
        int SourceEvidenceCount,
        double WithinHistoryStandardDeviation,
        double EarlySegmentMean,
        double MiddleSegmentMean,
        double LateSegmentMean);

    private sealed record DoctrinalRulePacket(int ContextCell, double RuleEstimate);

    private sealed record PathOutcome(
        string Series,
        double Rmse,
        double EarlyRmse,
        double LateRmse,
        double StableCompatibleEarlyRmse,
        double UnstableEarlyRmse,
        double FinalStableCompatibleStanding,
        double FinalStableDivergentStanding,
        double CommunicationWork,
        int CommunicationPacketCount)
    {
        public Dictionary<string, double> ToMetrics() => new(StringComparer.Ordinal)
        {
            ["rmse"] = Rmse,
            ["early_rmse"] = EarlyRmse,
            ["late_rmse"] = LateRmse,
            ["stable_compatible_early_rmse"] = StableCompatibleEarlyRmse,
            ["unstable_early_rmse"] = UnstableEarlyRmse,
            ["final_stable_compatible_standing"] = FinalStableCompatibleStanding,
            ["final_stable_divergent_standing"] = FinalStableDivergentStanding,
            ["communication_work"] = CommunicationWork,
            ["communication_packet_count"] = CommunicationPacketCount,
        };
    }

    private sealed class SourceMind
    {
        private readonly double[] _estimates;
        private readonly List<double>[] _history;

        public SourceMind(int contextCount)
        {
            _estimates = new double[contextCount];
            _history = new List<double>[contextCount];
            for (var index = 0; index < contextCount; index++)
            {
                _history[index] = [];
            }
        }

        public double Predict(int contextCell) => _estimates[contextCell];

        public int EvidenceFor(int contextCell) => _history[contextCell].Count;

        public void Observe(int contextCell, double target)
        {
            var history = _history[contextCell];
            history.Add(target);
            var count = history.Count;
            _estimates[contextCell] += (target - _estimates[contextCell]) / count;
        }

        public DevelopmentalTransferPacket[] ExportDevelopmentalPackets(DevelopmentalTransferScenario scenario)
        {
            if (scenario.Cells.Count != _history.Length)
            {
                throw new ArgumentException("Scenario context count does not match source state.", nameof(scenario));
            }

            var packets = new DevelopmentalTransferPacket[_history.Length];
            for (var contextCell = 0; contextCell < _history.Length; contextCell++)
            {
                var history = _history[contextCell];
                if (history.Count == 0)
                {
                    throw new InvalidOperationException($"Source context {contextCell} has no developmental evidence.");
                }

                var mean = _estimates[contextCell];
                var variance = 0.0;
                for (var index = 0; index < history.Count; index++)
                {
                    var difference = history[index] - mean;
                    variance += difference * difference;
                }

                variance /= history.Count;
                var early = SegmentMean(history, 0, 3);
                var middle = SegmentMean(history, 1, 3);
                var late = SegmentMean(history, 2, 3);
                var standardDeviation = Math.Sqrt(variance);
                packets[contextCell] = new DevelopmentalTransferPacket(
                    contextCell,
                    mean,
                    history.Count,
                    standardDeviation,
                    early,
                    middle,
                    late);
            }

            return packets;
        }

        private static double SegmentMean(List<double> history, int segment, int segmentCount)
        {
            var start = (segment * history.Count) / segmentCount;
            var end = ((segment + 1) * history.Count) / segmentCount;
            if (end <= start)
            {
                return history[Math.Min(start, history.Count - 1)];
            }

            var sum = 0.0;
            for (var index = start; index < end; index++)
            {
                sum += history[index];
            }

            return sum / (end - start);
        }
    }

    private sealed class ReceiverMind
    {
        private const double PriorStanding = 0.18;
        private readonly double[] _localEstimates;
        private readonly int[] _localEvidence;
        private readonly double[] _foreignEstimates;
        private readonly double[] _foreignStanding;
        private readonly int[] _importedEvidence;

        public ReceiverMind(string mindId, int contextCount)
        {
            MindId = mindId;
            _localEstimates = new double[contextCount];
            _localEvidence = new int[contextCount];
            _foreignEstimates = new double[contextCount];
            _foreignStanding = new double[contextCount];
            _importedEvidence = new int[contextCount];
        }

        public string MindId { get; }

        public double LastPrediction { get; private set; }

        public double LastTarget { get; private set; }

        public double LastAbsoluteError { get; private set; }

        public void ImportDevelopmental(DevelopmentalTransferPacket[] packets)
        {
            for (var index = 0; index < packets.Length; index++)
            {
                var packet = packets[index];
                _foreignEstimates[packet.ContextCell] = packet.RuleEstimate;
                _foreignStanding[packet.ContextCell] = CalculateDevelopmentalStanding(packet);
                _importedEvidence[packet.ContextCell] = packet.SourceEvidenceCount;
            }
        }

        public void ImportDoctrine(DoctrinalRulePacket[] packets)
        {
            for (var index = 0; index < packets.Length; index++)
            {
                var packet = packets[index];
                _foreignEstimates[packet.ContextCell] = packet.RuleEstimate;
                _foreignStanding[packet.ContextCell] = DoctrineStanding;
                _importedEvidence[packet.ContextCell] = 1;
            }
        }

        public double Predict(int contextCell)
        {
            var localStanding = LocalStandingFor(contextCell);
            var denominator = PriorStanding + localStanding + _foreignStanding[contextCell];
            LastPrediction = ((_localEstimates[contextCell] * localStanding) +
                (_foreignEstimates[contextCell] * _foreignStanding[contextCell])) / denominator;
            return LastPrediction;
        }

        public void ObserveDirect(int contextCell, double target)
        {
            LastTarget = target;
            LastAbsoluteError = Math.Abs(LastPrediction - target);
            var evidence = _localEvidence[contextCell];
            if (evidence == 0)
            {
                _localEstimates[contextCell] = target;
            }
            else
            {
                var alpha = Math.Max(0.08, 0.34 / (1.0 + (0.06 * evidence)));
                _localEstimates[contextCell] += alpha * (target - _localEstimates[contextCell]);
            }

            _localEvidence[contextCell]++;
            if (_foreignStanding[contextCell] <= 0.0)
            {
                return;
            }

            var foreignError = Math.Abs(_foreignEstimates[contextCell] - target);
            if (foreignError <= 0.10)
            {
                _foreignStanding[contextCell] += (0.90 - _foreignStanding[contextCell]) * 0.06;
            }
            else if (foreignError >= 0.25)
            {
                _foreignStanding[contextCell] *= 0.72;
            }
            else
            {
                _foreignStanding[contextCell] *= 0.93;
            }
        }

        public int LocalEvidenceFor(int contextCell) => _localEvidence[contextCell];

        public double ForeignStandingFor(int contextCell) => _foreignStanding[contextCell];

        public MindPublicState PublicMindState()
        {
            var localCount = 0;
            var foreignCount = 0;
            var localStandingSum = 0.0;
            var foreignStandingSum = 0.0;
            for (var contextCell = 0; contextCell < _localEvidence.Length; contextCell++)
            {
                if (_localEvidence[contextCell] > 0)
                {
                    localCount++;
                    localStandingSum += LocalStandingFor(contextCell);
                }

                if (_importedEvidence[contextCell] > 0)
                {
                    foreignCount++;
                    foreignStandingSum += _foreignStanding[contextCell];
                }
            }

            return new MindPublicState(
                MindId,
                localCount,
                foreignCount,
                localCount == 0 ? 0.0 : localStandingSum / localCount,
                foreignCount == 0 ? 0.0 : foreignStandingSum / foreignCount,
                LastPrediction,
                LastTarget,
                LastAbsoluteError);
        }

        public TracePublicState[] PublicTraceStates()
        {
            var traces = new List<TracePublicState>(_localEvidence.Length * 2);
            for (var contextCell = 0; contextCell < _localEvidence.Length; contextCell++)
            {
                if (_localEvidence[contextCell] > 0)
                {
                    traces.Add(new TracePublicState(
                        MindId,
                        contextCell,
                        TraceProvenance.Direct,
                        MindId,
                        $"{MindId}:local:{contextCell}",
                        _localEstimates[contextCell],
                        LocalStandingFor(contextCell),
                        _localEvidence[contextCell],
                        0));
                }

                if (_importedEvidence[contextCell] > 0)
                {
                    traces.Add(new TracePublicState(
                        MindId,
                        contextCell,
                        TraceProvenance.Foreign,
                        "source",
                        $"source:transfer:{contextCell}",
                        _foreignEstimates[contextCell],
                        _foreignStanding[contextCell],
                        _localEvidence[contextCell],
                        _importedEvidence[contextCell]));
                }
            }

            return traces.ToArray();
        }

        private double LocalStandingFor(int contextCell) => _localEvidence[contextCell] == 0
            ? 0.0
            : Math.Min(0.95, 0.20 + (0.18 * Math.Log(1.0 + _localEvidence[contextCell])));
    }
}
