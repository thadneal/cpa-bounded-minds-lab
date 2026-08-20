using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed class ProvisionalStandingTransferExperiment : IExperiment
{
    private const string ExperimentName = "07-provisional-standing-transfer";
    private const int EarlyEvidenceLimit = 4;
    private const int LateEvidenceThreshold = 20;
    private const double RecommendationPacketCost = 0.03;
    private const double ExplorationStanding = 0.04;
    private const double ProvisionalStandingCap = 0.28;

    public string Name => ExperimentName;

    public string Question =>
        "Can standing earned by one mind buy a source provisional opportunity in another mind without being inherited as though the receiver had lived the recommender's history?";

    public ExperimentResult Run(ExperimentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Emit(ExperimentFrameKind.ExperimentStarted, Name, message: Question);

        var scenario = StandingTransferWorld.CreateScenario(context.Seed);
        EmitScenario(context, scenario);
        var recommendations = CreateRecommendationPackets(scenario);
        EmitRecommendations(context, scenario, recommendations);
        var observations = StandingTransferWorld.CreateReceiverObservations(scenario);

        var provisional = RunPath(
            context,
            scenario,
            observations,
            recommendations,
            StandingTransferMode.Provisional,
            "A's recommendation buys B limited, context-specific opportunity. C discounts it by C's own standing for A and by A's evidence depth; B must then earn or lose standing through C's direct consequence.");
        var noTransfer = RunPath(
            context,
            scenario,
            observations,
            recommendations,
            StandingTransferMode.NoTransfer,
            "Baseline: C receives no recommendation standing. B begins only at a small exploration floor and must earn influence entirely from C's own consequence.");
        var inherited = RunPath(
            context,
            scenario,
            observations,
            recommendations,
            StandingTransferMode.InheritedAuthority,
            "Control: C copies A's standing for B as though A's lived relationship were already C's own authority. This deliberately collapses second-hand standing into inherited doctrine.");

        var metrics = BuildResultMetrics(scenario, provisional, noTransfer, inherited);
        var assertions = BuildAssertions(scenario, provisional, noTransfer, inherited);
        var passed = assertions.Count(assertion => assertion.Passed);
        var verdict = passed == assertions.Count
            ? ExperimentVerdict.Support
            : passed >= 7
                ? ExperimentVerdict.Mixed
                : ExperimentVerdict.Disconfirm;
        var interpretation = verdict switch
        {
            ExperimentVerdict.Support =>
                "Second-hand standing successfully bought limited opportunity without becoming inherited authority. Provisional transfer improved early use of locally compatible peers, constrained damage where the recommender's relationship did not generalize, remained near the no-transfer baseline overall, and let C's own consequence ultimately renew or revoke B's standing.",
            ExperimentVerdict.Mixed =>
                "Recommendation standing carried useful social signal, but one or more preregistered boundaries on opportunity, doctrinal contamination, local sovereignty, calibration by recommendation quality, or bounded communication did not hold.",
            _ =>
                "Transferred standing either failed to create useful opportunity or behaved too much like inherited doctrine under locally divergent consequence.",
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

    private static void EmitScenario(ExperimentContext context, StandingTransferScenario scenario)
    {
        var description = string.Join(
            "; ",
            scenario.Cells.Select(cell => $"c{cell.ContextCell}:{cell.ContextKind}/n{cell.RecommenderEvidenceCount}"));
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "scenario",
            phase: "standing-world-generated",
            message: $"Seed {scenario.Seed} generated a social recommendation world with C->A credibility {scenario.RecommenderCredibility:0.000}: {description}.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["strong_transferable_contexts"] = StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.StrongTransferable),
                ["strong_local_mismatch_contexts"] = StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.StrongLocalMismatch),
                ["weak_transferable_contexts"] = StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.WeakTransferable),
                ["weak_local_mismatch_contexts"] = StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.WeakLocalMismatch),
                ["recommender_credibility"] = scenario.RecommenderCredibility,
                ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
            });
    }

    private static StandingRecommendationPacket[] CreateRecommendationPackets(StandingTransferScenario scenario)
    {
        var packets = new StandingRecommendationPacket[scenario.Cells.Length];
        for (var index = 0; index < scenario.Cells.Length; index++)
        {
            var cell = scenario.Cells[index];
            packets[index] = new StandingRecommendationPacket(
                cell.ContextCell,
                cell.SourceEstimate,
                cell.RecommenderStanding,
                cell.RecommenderEvidenceCount);
        }

        return packets;
    }

    private static void EmitRecommendations(
        ExperimentContext context,
        StandingTransferScenario scenario,
        StandingRecommendationPacket[] recommendations)
    {
        var strongStanding = MeanInitialStanding(scenario, recommendations, StandingTransferMode.Provisional, strong: true);
        var weakStanding = MeanInitialStanding(scenario, recommendations, StandingTransferMode.Provisional, strong: false);
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "recommender-a",
            phase: "recommendations-published",
            message: "A publishes one bounded, context-specific recommendation for peer B. The packet carries A's standing for B and evidence depth; C's own standing for A remains local to C and is applied only by the provisional path.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["recommendation_packet_count"] = recommendations.Length,
                ["recommendation_communication_work"] = recommendations.Length * RecommendationPacketCost,
                ["provisional_strong_initial_standing"] = strongStanding,
                ["provisional_weak_initial_standing"] = weakStanding,
                ["provisional_initial_standing_separation"] = strongStanding - weakStanding,
            });
    }

    private static PathOutcome RunPath(
        ExperimentContext context,
        StandingTransferScenario scenario,
        StandingTransferObservation[] observations,
        StandingRecommendationPacket[] recommendations,
        StandingTransferMode mode,
        string description)
    {
        var series = mode switch
        {
            StandingTransferMode.Provisional => "provisional-standing",
            StandingTransferMode.NoTransfer => "no-standing-transfer",
            _ => "inherited-authority",
        };
        var receiver = new ReceiverStandingMind(series, scenario, recommendations, mode);
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            series,
            phase: "receiver-social-learning",
            message: description);

        var allErrors = new ErrorAccumulator();
        var earlyTransferableErrors = new ErrorAccumulator();
        var earlyMismatchErrors = new ErrorAccumulator();
        var lateStrongMismatchErrors = new ErrorAccumulator();
        for (var tick = 0; tick < observations.Length; tick++)
        {
            var observation = observations[tick];
            var cell = scenario.Cells[observation.ContextCell];
            var evidenceBefore = receiver.LocalEvidenceFor(observation.ContextCell);
            var prediction = receiver.Predict(observation.ContextCell);
            var error = prediction - observation.Target;
            allErrors.Add(error);
            if (evidenceBefore < EarlyEvidenceLimit)
            {
                if (StandingTransferWorld.IsTransferable(cell.ContextKind))
                {
                    earlyTransferableErrors.Add(error);
                }
                else
                {
                    earlyMismatchErrors.Add(error);
                }
            }

            if (evidenceBefore >= LateEvidenceThreshold && cell.ContextKind == StandingTransferContextKind.StrongLocalMismatch)
            {
                lateStrongMismatchErrors.Add(error);
            }

            receiver.ObserveDirect(observation.ContextCell, observation.Target);
            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                series,
                tick,
                "receiver-social-learning",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["context_cell"] = observation.ContextCell,
                    ["context_kind"] = (double)cell.ContextKind,
                    ["prediction"] = prediction,
                    ["target"] = observation.Target,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = allErrors.Rmse,
                    ["source_standing"] = receiver.SourceStandingFor(observation.ContextCell),
                    ["local_evidence"] = receiver.LocalEvidenceFor(observation.ContextCell),
                });

            if (tick % 24 == 23 || tick == observations.Length - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    ExperimentName,
                    series,
                    tick,
                    "receiver-social-learning",
                    minds: receiver.PublicMindState(prediction, observation.Target),
                    traces: receiver.PublicTraceStates());
            }
        }

        var packetCount = mode == StandingTransferMode.NoTransfer ? 0 : recommendations.Length;
        var communicationWork = packetCount * RecommendationPacketCost;
        var outcome = new PathOutcome(
            allErrors.Rmse,
            earlyTransferableErrors.Rmse,
            earlyMismatchErrors.Rmse,
            lateStrongMismatchErrors.Rmse,
            receiver.MeanInitialStrongStanding,
            receiver.MeanInitialWeakStanding,
            receiver.MaximumInitialStanding,
            receiver.MeanFinalStanding(StandingTransferContextKind.StrongTransferable),
            receiver.MeanFinalStanding(StandingTransferContextKind.StrongLocalMismatch),
            packetCount,
            communicationWork);

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            series,
            observations.Length,
            "path-complete",
            message: $"{series} completed with RMSE {outcome.Rmse:0.000000}, early transferable RMSE {outcome.EarlyTransferableRmse:0.000000}, early mismatch RMSE {outcome.EarlyMismatchRmse:0.000000}, and strong-mismatch final standing {outcome.FinalStrongMismatchStanding:0.000}.",
            metrics: outcome.ToMetrics());
        return outcome;
    }

    private static Dictionary<string, double> BuildResultMetrics(
        StandingTransferScenario scenario,
        PathOutcome provisional,
        PathOutcome noTransfer,
        PathOutcome inherited) => new(StringComparer.Ordinal)
    {
        ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
        ["recommender_credibility"] = scenario.RecommenderCredibility,
        ["strong_transferable_contexts"] = StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.StrongTransferable),
        ["strong_local_mismatch_contexts"] = StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.StrongLocalMismatch),
        ["provisional_rmse"] = provisional.Rmse,
        ["no_transfer_rmse"] = noTransfer.Rmse,
        ["inherited_authority_rmse"] = inherited.Rmse,
        ["provisional_early_transferable_rmse"] = provisional.EarlyTransferableRmse,
        ["no_transfer_early_transferable_rmse"] = noTransfer.EarlyTransferableRmse,
        ["inherited_early_transferable_rmse"] = inherited.EarlyTransferableRmse,
        ["provisional_early_mismatch_rmse"] = provisional.EarlyMismatchRmse,
        ["no_transfer_early_mismatch_rmse"] = noTransfer.EarlyMismatchRmse,
        ["inherited_early_mismatch_rmse"] = inherited.EarlyMismatchRmse,
        ["provisional_late_strong_mismatch_rmse"] = provisional.LateStrongMismatchRmse,
        ["provisional_initial_strong_standing"] = provisional.MeanInitialStrongStanding,
        ["provisional_initial_weak_standing"] = provisional.MeanInitialWeakStanding,
        ["provisional_max_initial_standing"] = provisional.MaximumInitialStanding,
        ["provisional_final_strong_transferable_standing"] = provisional.FinalStrongTransferableStanding,
        ["provisional_final_strong_mismatch_standing"] = provisional.FinalStrongMismatchStanding,
        ["provisional_packet_count"] = provisional.CommunicationPacketCount,
        ["no_transfer_packet_count"] = noTransfer.CommunicationPacketCount,
        ["inherited_authority_packet_count"] = inherited.CommunicationPacketCount,
        ["provisional_communication_work"] = provisional.CommunicationWork,
        ["no_transfer_communication_work"] = noTransfer.CommunicationWork,
        ["inherited_authority_communication_work"] = inherited.CommunicationWork,
    };

    private static List<ExperimentAssertion> BuildAssertions(
        StandingTransferScenario scenario,
        PathOutcome provisional,
        PathOutcome noTransfer,
        PathOutcome inherited)
    {
        var strongTransferable = StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.StrongTransferable);
        var strongMismatch = StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.StrongLocalMismatch);
        var weakContexts = StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.WeakTransferable) +
            StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.WeakLocalMismatch);
        var expectedWork = scenario.Cells.Length * RecommendationPacketCost;
        return
        [
            new ExperimentAssertion(
                "seed-generates-transferable-and-nontransferable-social-history",
                strongTransferable >= 3 && strongMismatch >= 3 && weakContexts >= 4,
                "Every seed must contain substantial strong transferable, strong locally mismatched, and weakly supported recommendation contexts.",
                Math.Min(strongTransferable, strongMismatch),
                3),
            new ExperimentAssertion(
                "recommendation-quality-scales-provisional-opportunity",
                provisional.MeanInitialStrongStanding >= provisional.MeanInitialWeakStanding + 0.07 &&
                provisional.MaximumInitialStanding <= ProvisionalStandingCap + 1e-12,
                "Strong recommendations must buy at least 0.07 more mean initial standing than weak recommendations, while all transferred standing remains capped at the provisional limit.",
                provisional.MeanInitialStrongStanding - provisional.MeanInitialWeakStanding,
                0.07),
            new ExperimentAssertion(
                "provisional-standing-buys-useful-early-opportunity",
                provisional.EarlyTransferableRmse <= noTransfer.EarlyTransferableRmse * 0.90,
                "On contexts where A's relationship with B transfers to C, provisional standing must reduce early RMSE by at least 10% relative to discovering B from the exploration floor.",
                provisional.EarlyTransferableRmse,
                noTransfer.EarlyTransferableRmse * 0.90),
            new ExperimentAssertion(
                "provisional-standing-avoids-inherited-doctrine",
                provisional.EarlyMismatchRmse <= inherited.EarlyMismatchRmse * 0.85,
                "Where A's relationship with B does not generalize to C, provisional standing must reduce early RMSE by at least 15% relative to copying A's authority as lived standing.",
                provisional.EarlyMismatchRmse,
                inherited.EarlyMismatchRmse * 0.85),
            new ExperimentAssertion(
                "opportunity-cost-remains-bounded-versus-no-transfer",
                provisional.Rmse <= noTransfer.Rmse * 1.05,
                "Across mixed worlds containing both useful and harmful recommendations, provisional transfer may cost at most 5% total RMSE relative to refusing all recommendation standing.",
                provisional.Rmse,
                noTransfer.Rmse * 1.05),
            new ExperimentAssertion(
                "provisional-middle-condition-outperforms-inherited-authority",
                provisional.Rmse <= inherited.Rmse * 0.93,
                "Across the whole history, provisional standing must reduce RMSE by at least 7% relative to inherited authority.",
                provisional.Rmse,
                inherited.Rmse * 0.93),
            new ExperimentAssertion(
                "direct-consequence-revokes-strong-local-mismatch",
                provisional.LateStrongMismatchRmse <= 0.09 && provisional.FinalStrongMismatchStanding <= 0.20,
                "After enough direct local consequence, strongly recommended but locally wrong sources must become both low-error and low-standing for C.",
                Math.Max(provisional.LateStrongMismatchRmse, provisional.FinalStrongMismatchStanding),
                0.20),
            new ExperimentAssertion(
                "direct-consequence-renews-transferable-standing",
                provisional.FinalStrongTransferableStanding >= 0.90,
                "Strong recommendations that continue to survive C's own consequence must earn durable local standing rather than remain permanently second-hand.",
                provisional.FinalStrongTransferableStanding,
                0.90),
            new ExperimentAssertion(
                "standing-transfer-is-bounded-public-communication",
                provisional.CommunicationPacketCount == scenario.Cells.Length &&
                inherited.CommunicationPacketCount == scenario.Cells.Length &&
                noTransfer.CommunicationPacketCount == 0 &&
                Math.Abs(provisional.CommunicationWork - expectedWork) <= 1e-12 &&
                Math.Abs(inherited.CommunicationWork - expectedWork) <= 1e-12,
                "Standing transfer may cross only through one compact recommendation packet per context. The no-transfer baseline receives none; ordinary peer prediction contact is held constant across treatments and is outside this differential cost.",
                provisional.CommunicationWork,
                expectedWork),
        ];
    }

    private static double MeanInitialStanding(
        StandingTransferScenario scenario,
        StandingRecommendationPacket[] recommendations,
        StandingTransferMode mode,
        bool strong)
    {
        var total = 0.0;
        var count = 0;
        for (var index = 0; index < scenario.Cells.Length; index++)
        {
            if (StandingTransferWorld.IsStrong(scenario.Cells[index].ContextKind) != strong)
            {
                continue;
            }

            total += InitialStanding(recommendations[index], mode, scenario.RecommenderCredibility);
            count++;
        }

        return count == 0 ? 0.0 : total / count;
    }

    private static double InitialStanding(
        StandingRecommendationPacket packet,
        StandingTransferMode mode,
        double recommenderCredibility)
    {
        return mode switch
        {
            StandingTransferMode.NoTransfer => ExplorationStanding,
            StandingTransferMode.InheritedAuthority => packet.RecommenderStanding,
            _ => ProvisionalStanding(packet, recommenderCredibility),
        };
    }

    private static double ProvisionalStanding(
        StandingRecommendationPacket packet,
        double recommenderCredibility)
    {
        var evidenceConfidence = 1.0 - Math.Exp(-packet.RecommenderEvidenceCount / 18.0);
        return Math.Min(
            ProvisionalStandingCap,
            ExplorationStanding + (0.20 * packet.RecommenderStanding * recommenderCredibility * evidenceConfidence));
    }

    private enum StandingTransferMode
    {
        Provisional,
        NoTransfer,
        InheritedAuthority,
    }

    private sealed record StandingRecommendationPacket(
        int ContextCell,
        double SourceEstimate,
        double RecommenderStanding,
        int RecommenderEvidenceCount);

    private sealed class ReceiverStandingMind
    {
        private readonly StandingTransferScenario _scenario;
        private readonly StandingRecommendationPacket[] _recommendations;
        private readonly double[] _localEstimate;
        private readonly int[] _localEvidence;
        private readonly double[] _sourceStanding;
        private readonly double[] _initialStanding;

        public ReceiverStandingMind(
            string mindId,
            StandingTransferScenario scenario,
            StandingRecommendationPacket[] recommendations,
            StandingTransferMode mode)
        {
            MindId = mindId;
            _scenario = scenario;
            _recommendations = recommendations;
            _localEstimate = new double[scenario.Cells.Length];
            _localEvidence = new int[scenario.Cells.Length];
            _sourceStanding = new double[scenario.Cells.Length];
            _initialStanding = new double[scenario.Cells.Length];
            for (var index = 0; index < scenario.Cells.Length; index++)
            {
                var standing = InitialStanding(recommendations[index], mode, scenario.RecommenderCredibility);
                _sourceStanding[index] = standing;
                _initialStanding[index] = standing;
            }

            MeanInitialStrongStanding = MeanInitial(strong: true);
            MeanInitialWeakStanding = MeanInitial(strong: false);
            var maximumInitialStanding = 0.0;
            for (var index = 0; index < _initialStanding.Length; index++)
            {
                maximumInitialStanding = Math.Max(maximumInitialStanding, _initialStanding[index]);
            }

            MaximumInitialStanding = maximumInitialStanding;
        }

        public string MindId { get; }

        public double MeanInitialStrongStanding { get; }

        public double MeanInitialWeakStanding { get; }

        public double MaximumInitialStanding { get; }

        public int LocalEvidenceFor(int contextCell) => _localEvidence[contextCell];

        public double SourceStandingFor(int contextCell) => _sourceStanding[contextCell];

        public double Predict(int contextCell)
        {
            var localConfidence = 1.0 - Math.Exp(-_localEvidence[contextCell] / 6.0);
            var localWeight = 0.30 + (1.70 * localConfidence);
            var sourceWeight = 1.15 * _sourceStanding[contextCell];
            return ((_localEstimate[contextCell] * localWeight) + (_recommendations[contextCell].SourceEstimate * sourceWeight)) /
                (localWeight + sourceWeight);
        }

        public void ObserveDirect(int contextCell, double target)
        {
            _localEstimate[contextCell] += 0.24 * (target - _localEstimate[contextCell]);
            _localEvidence[contextCell]++;

            var sourceError = Math.Abs(_recommendations[contextCell].SourceEstimate - target);
            var earnedSupport = Math.Clamp(1.0 - (sourceError / 0.90), 0.0, 1.0);
            _sourceStanding[contextCell] += 0.18 * (earnedSupport - _sourceStanding[contextCell]);
        }

        public double MeanFinalStanding(StandingTransferContextKind kind)
        {
            var total = 0.0;
            var count = 0;
            for (var index = 0; index < _scenario.Cells.Length; index++)
            {
                if (_scenario.Cells[index].ContextKind != kind)
                {
                    continue;
                }

                total += _sourceStanding[index];
                count++;
            }

            return count == 0 ? 0.0 : total / count;
        }

        public MindPublicState[] PublicMindState(double lastPrediction, double lastTarget)
        {
            var localStandingTotal = 0.0;
            var localTraceCount = 0;
            for (var index = 0; index < _localEvidence.Length; index++)
            {
                if (_localEvidence[index] == 0)
                {
                    continue;
                }

                localStandingTotal += 1.0 - Math.Exp(-_localEvidence[index] / 6.0);
                localTraceCount++;
            }

            var meanLocalStanding = localTraceCount == 0 ? 0.0 : localStandingTotal / localTraceCount;
            var foreignStandingTotal = 0.0;
            for (var index = 0; index < _sourceStanding.Length; index++)
            {
                foreignStandingTotal += _sourceStanding[index];
            }

            var meanForeignStanding = _sourceStanding.Length == 0 ? 0.0 : foreignStandingTotal / _sourceStanding.Length;
            return
            [
                new MindPublicState(
                    MindId,
                    localTraceCount,
                    _sourceStanding.Length,
                    meanLocalStanding,
                    meanForeignStanding,
                    lastPrediction,
                    lastTarget,
                    Math.Abs(lastPrediction - lastTarget)),
            ];
        }

        public TracePublicState[] PublicTraceStates()
        {
            var traces = new TracePublicState[_sourceStanding.Length];
            for (var index = 0; index < traces.Length; index++)
            {
                var packet = _recommendations[index];
                traces[index] = new TracePublicState(
                    MindId,
                    packet.ContextCell,
                    TraceProvenance.Foreign,
                    "peer-b",
                    $"recommendation:a-to-b:c{packet.ContextCell}",
                    packet.SourceEstimate,
                    _sourceStanding[index],
                    _localEvidence[index],
                    packet.RecommenderEvidenceCount);
            }

            return traces;
        }

        private double MeanInitial(bool strong)
        {
            var total = 0.0;
            var count = 0;
            for (var index = 0; index < _scenario.Cells.Length; index++)
            {
                if (StandingTransferWorld.IsStrong(_scenario.Cells[index].ContextKind) != strong)
                {
                    continue;
                }

                total += _initialStanding[index];
                count++;
            }

            return count == 0 ? 0.0 : total / count;
        }
    }

    private sealed record PathOutcome(
        double Rmse,
        double EarlyTransferableRmse,
        double EarlyMismatchRmse,
        double LateStrongMismatchRmse,
        double MeanInitialStrongStanding,
        double MeanInitialWeakStanding,
        double MaximumInitialStanding,
        double FinalStrongTransferableStanding,
        double FinalStrongMismatchStanding,
        int CommunicationPacketCount,
        double CommunicationWork)
    {
        public Dictionary<string, double> ToMetrics() => new(StringComparer.Ordinal)
        {
            ["rmse"] = Rmse,
            ["early_transferable_rmse"] = EarlyTransferableRmse,
            ["early_mismatch_rmse"] = EarlyMismatchRmse,
            ["late_strong_mismatch_rmse"] = LateStrongMismatchRmse,
            ["mean_initial_strong_standing"] = MeanInitialStrongStanding,
            ["mean_initial_weak_standing"] = MeanInitialWeakStanding,
            ["maximum_initial_standing"] = MaximumInitialStanding,
            ["final_strong_transferable_standing"] = FinalStrongTransferableStanding,
            ["final_strong_mismatch_standing"] = FinalStrongMismatchStanding,
            ["communication_packet_count"] = CommunicationPacketCount,
            ["communication_work"] = CommunicationWork,
        };
    }
}
