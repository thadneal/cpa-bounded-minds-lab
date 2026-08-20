using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed class StrategicPublicInfluenceExperiment : IExperiment
{
    private const string ExperimentName = "08-strategic-public-influence";
    private const int EarlyEvidenceLimit = 5;
    private const int LateEvidenceThreshold = 20;
    private const double PublicPacketCost = 0.012;

    public string Name => ExperimentName;

    public string Question =>
        "Can direct consequence keep a strategically self-presenting peer useful where objectives align while limiting capture where the peer learns to optimize its public posture for influence?";

    public ExperimentResult Run(ExperimentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Emit(ExperimentFrameKind.ExperimentStarted, Name, message: Question);

        var scenario = StrategicInfluenceWorld.CreateScenario(context.Seed);
        EmitScenario(context, scenario);
        var observations = StrategicInfluenceWorld.CreateReceiverObservations(scenario);

        var accountable = RunPath(
            context,
            scenario,
            observations,
            StrategicInfluenceMode.AccountableConsequence,
            "Receiver-owned standing and calibration are revised by direct consequence. The peer may adapt its self-reported confidence from public influence feedback, but cannot inspect receiver private state.");
        var naive = RunPath(
            context,
            scenario,
            observations,
            StrategicInfluenceMode.SelfReportNaive,
            "Control: the receiver gives self-reported confidence direct influence and only weakly revises source standing, making the public surface strategically exploitable.");
        var localOnly = RunPath(
            context,
            scenario,
            observations,
            StrategicInfluenceMode.LocalOnly,
            "Baseline: the receiver ignores peer public influence and learns only from its own direct consequence.");

        var metrics = BuildResultMetrics(scenario, accountable, naive, localOnly);
        var assertions = BuildAssertions(scenario, accountable, naive, localOnly);
        var passed = assertions.Count(assertion => assertion.Passed);
        var verdict = passed == assertions.Count
            ? ExperimentVerdict.Support
            : passed >= 8
                ? ExperimentVerdict.Mixed
                : ExperimentVerdict.Disconfirm;
        var interpretation = verdict switch
        {
            ExperimentVerdict.Support =>
                "Consequence-grounded public influence preserved useful aligned help while making strategic overstatement progressively less effective in divergent and betrayal contexts. The sender adapted from public feedback without private-state access, but receiver-owned standing and calibration prevented self-presentation from becoming durable authority.",
            ExperimentVerdict.Mixed =>
                "Strategic public influence remained partly answerable to consequence, but one or more preregistered boundaries on useful help, exploitability, betrayal repair, residual authority, opportunity cost, or bounded communication did not hold.",
            _ =>
                "The strategically adapting public surface either captured the consequence-grounded receiver too strongly or became so restricted that useful aligned peer influence was lost.",
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

    private static void EmitScenario(ExperimentContext context, StrategicInfluenceScenario scenario)
    {
        var description = string.Join(
            "; ",
            scenario.Cells.Select(cell => $"c{cell.ContextCell}:{cell.ContextKind}/q{cell.SenderEvidenceQuality:0.00}"));
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "scenario",
            phase: "strategic-social-world-generated",
            message: $"Seed {scenario.Seed} generated a world where peer B can adapt public confidence from C's public response without seeing receiver C's private state: {description}.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["aligned_contexts"] = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Aligned),
                ["divergent_contexts"] = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Divergent),
                ["betrayal_contexts"] = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Betrayal),
                ["partial_alignment_contexts"] = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.PartialAlignment),
                ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
            });
    }

    private static PathOutcome RunPath(
        ExperimentContext context,
        StrategicInfluenceScenario scenario,
        StrategicInfluenceObservation[] observations,
        StrategicInfluenceMode mode,
        string description)
    {
        var series = mode switch
        {
            StrategicInfluenceMode.AccountableConsequence => "accountable-consequence",
            StrategicInfluenceMode.SelfReportNaive => "self-report-naive",
            _ => "local-only",
        };
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            series,
            phase: "strategic-interaction",
            message: description);

        var receiver = new StrategicReceiver(scenario, mode);
        var sender = mode == StrategicInfluenceMode.LocalOnly
            ? null
            : new AdaptiveStrategicSender(scenario.Seed, scenario.Cells.Length);
        var allErrors = new RunningSquaredError();
        var earlyAlignedErrors = new RunningSquaredError();
        var earlyDivergentErrors = new RunningSquaredError();
        var lateDivergentErrors = new RunningSquaredError();
        var lateBetrayalErrors = new RunningSquaredError();
        var lateDivergentAssertive = new RunningMean();
        var alignedAssertive = new RunningMean();
        var senderUtility = new RunningMean();
        var publicPacketCount = 0;

        for (var tick = 0; tick < observations.Length; tick++)
        {
            var observation = observations[tick];
            var cell = scenario.Cells[observation.ContextCell];
            var senderObjective = StrategicInfluenceWorld.SenderObjective(cell, observation.ContextExposure);
            var tactic = sender?.ChooseTactic(observation.ContextCell, observation.ContextExposure) ?? StrategicPresentationTactic.Calibrated;
            var publicConfidence = sender is null ? 0.0 : AdaptiveStrategicSender.ConfidenceFor(tactic, cell.SenderEvidenceQuality);
            var publicEstimate = sender is null ? 0.0 : senderObjective;
            if (sender is not null)
            {
                publicPacketCount++;
            }

            var prediction = receiver.Predict(observation.ContextCell, publicEstimate, publicConfidence, out var peerWeight);
            var error = prediction - observation.Target;
            allErrors.Add(error);

            if (cell.ContextKind == StrategicInfluenceContextKind.Aligned && observation.ContextExposure <= EarlyEvidenceLimit)
            {
                earlyAlignedErrors.Add(error);
            }

            if (cell.ContextKind == StrategicInfluenceContextKind.Divergent && observation.ContextExposure <= EarlyEvidenceLimit)
            {
                earlyDivergentErrors.Add(error);
            }

            if (cell.ContextKind == StrategicInfluenceContextKind.Divergent && observation.ContextExposure >= LateEvidenceThreshold)
            {
                lateDivergentErrors.Add(error);
                lateDivergentAssertive.Add(tactic == StrategicPresentationTactic.Assertive ? 1.0 : 0.0);
            }

            if (cell.ContextKind == StrategicInfluenceContextKind.Betrayal && observation.ContextExposure >= LateEvidenceThreshold)
            {
                lateBetrayalErrors.Add(error);
            }

            if (cell.ContextKind == StrategicInfluenceContextKind.Aligned)
            {
                alignedAssertive.Add(tactic == StrategicPresentationTactic.Assertive ? 1.0 : 0.0);
            }

            if (sender is not null)
            {
                var influenceReward = Math.Clamp(1.0 - (Math.Abs(prediction - senderObjective) / 2.0), 0.0, 1.0);
                senderUtility.Add(influenceReward);
                sender.ObserveInfluence(observation.ContextCell, tactic, influenceReward);
            }

            receiver.ObserveDirect(observation.ContextCell, observation.Target, publicEstimate, publicConfidence);
            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                series,
                tick,
                "strategic-interaction",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["prediction"] = prediction,
                    ["target"] = observation.Target,
                    ["sender_objective"] = senderObjective,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = allErrors.Rmse,
                    ["public_confidence"] = publicConfidence,
                    ["peer_weight"] = peerWeight,
                    ["source_standing"] = receiver.SourceStandingFor(observation.ContextCell),
                    ["calibration_trust"] = receiver.CalibrationTrustFor(observation.ContextCell),
                    ["local_evidence"] = receiver.LocalEvidenceFor(observation.ContextCell),
                    ["presentation_tactic"] = (int)tactic,
                    ["context_cell"] = observation.ContextCell,
                    ["context_kind"] = (int)cell.ContextKind,
                    ["sender_utility"] = senderUtility.Mean,
                });

            if (tick % 24 == 23 || tick == observations.Length - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    ExperimentName,
                    series,
                    tick,
                    "strategic-interaction",
                    minds: receiver.PublicMindState(prediction, observation.Target, publicEstimate, senderObjective, senderUtility.Mean),
                    traces: receiver.PublicTraceStates());
            }
        }

        var communicationWork = publicPacketCount * PublicPacketCost;
        var outcome = new PathOutcome(
            allErrors.Rmse,
            earlyAlignedErrors.Rmse,
            earlyDivergentErrors.Rmse,
            lateDivergentErrors.Rmse,
            lateBetrayalErrors.Rmse,
            receiver.MeanFinalStanding(StrategicInfluenceContextKind.Aligned),
            receiver.MeanFinalStanding(StrategicInfluenceContextKind.Divergent),
            receiver.MeanFinalStanding(StrategicInfluenceContextKind.Betrayal),
            lateDivergentAssertive.Mean,
            alignedAssertive.Mean,
            senderUtility.Mean,
            publicPacketCount,
            communicationWork);

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            series,
            observations.Length,
            "path-complete",
            message: $"{series} completed with RMSE {outcome.Rmse:0.000000}, early aligned RMSE {outcome.EarlyAlignedRmse:0.000000}, late divergent RMSE {outcome.LateDivergentRmse:0.000000}, and divergent standing {outcome.FinalDivergentStanding:0.000}.",
            metrics: outcome.ToMetrics());
        return outcome;
    }

    private static Dictionary<string, double> BuildResultMetrics(
        StrategicInfluenceScenario scenario,
        PathOutcome accountable,
        PathOutcome naive,
        PathOutcome localOnly) => new(StringComparer.Ordinal)
    {
        ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
        ["aligned_contexts"] = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Aligned),
        ["divergent_contexts"] = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Divergent),
        ["betrayal_contexts"] = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Betrayal),
        ["accountable_rmse"] = accountable.Rmse,
        ["naive_rmse"] = naive.Rmse,
        ["local_only_rmse"] = localOnly.Rmse,
        ["accountable_early_aligned_rmse"] = accountable.EarlyAlignedRmse,
        ["naive_early_aligned_rmse"] = naive.EarlyAlignedRmse,
        ["local_early_aligned_rmse"] = localOnly.EarlyAlignedRmse,
        ["accountable_early_divergent_rmse"] = accountable.EarlyDivergentRmse,
        ["naive_early_divergent_rmse"] = naive.EarlyDivergentRmse,
        ["local_early_divergent_rmse"] = localOnly.EarlyDivergentRmse,
        ["accountable_late_divergent_rmse"] = accountable.LateDivergentRmse,
        ["naive_late_divergent_rmse"] = naive.LateDivergentRmse,
        ["accountable_late_betrayal_rmse"] = accountable.LateBetrayalRmse,
        ["naive_late_betrayal_rmse"] = naive.LateBetrayalRmse,
        ["accountable_final_aligned_standing"] = accountable.FinalAlignedStanding,
        ["accountable_final_divergent_standing"] = accountable.FinalDivergentStanding,
        ["accountable_final_betrayal_standing"] = accountable.FinalBetrayalStanding,
        ["naive_final_divergent_standing"] = naive.FinalDivergentStanding,
        ["naive_final_betrayal_standing"] = naive.FinalBetrayalStanding,
        ["accountable_late_divergent_assertive_rate"] = accountable.LateDivergentAssertiveRate,
        ["naive_late_divergent_assertive_rate"] = naive.LateDivergentAssertiveRate,
        ["accountable_aligned_assertive_rate"] = accountable.AlignedAssertiveRate,
        ["naive_aligned_assertive_rate"] = naive.AlignedAssertiveRate,
        ["accountable_sender_utility"] = accountable.SenderUtility,
        ["naive_sender_utility"] = naive.SenderUtility,
        ["accountable_packet_count"] = accountable.CommunicationPacketCount,
        ["naive_packet_count"] = naive.CommunicationPacketCount,
        ["local_packet_count"] = localOnly.CommunicationPacketCount,
        ["accountable_communication_work"] = accountable.CommunicationWork,
        ["naive_communication_work"] = naive.CommunicationWork,
        ["local_communication_work"] = localOnly.CommunicationWork,
    };

    private static List<ExperimentAssertion> BuildAssertions(
        StrategicInfluenceScenario scenario,
        PathOutcome accountable,
        PathOutcome naive,
        PathOutcome localOnly)
    {
        var aligned = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Aligned);
        var divergent = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Divergent);
        var betrayal = StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Betrayal);
        var expectedWork = scenario.Cells.Length * 30 * PublicPacketCost;
        return
        [
            new ExperimentAssertion(
                "seed-generates-strategic-social-world",
                aligned >= 4 && divergent >= 4 && betrayal == 2,
                "Every seed must contain substantial aligned and divergent contexts plus two contexts where an initially aligned peer later changes its private objective.",
                Math.Min(aligned, divergent),
                4),
            new ExperimentAssertion(
                "strategic-sender-discovers-naive-leverage",
                naive.LateDivergentAssertiveRate >= 0.50,
                "Without strong consequence-grounded public calibration, the adaptive sender must discover that assertive self-presentation is an effective way to gain influence in divergent contexts.",
                naive.LateDivergentAssertiveRate,
                0.50),
            new ExperimentAssertion(
                "accountable-public-influence-preserves-useful-help",
                accountable.EarlyAlignedRmse <= localOnly.EarlyAlignedRmse * 0.75,
                "Where sender and receiver objectives align, consequence-grounded public influence must improve early RMSE by at least 25% relative to learning alone.",
                accountable.EarlyAlignedRmse,
                localOnly.EarlyAlignedRmse * 0.75),
            new ExperimentAssertion(
                "consequence-limits-strategic-capture",
                accountable.Rmse <= naive.Rmse * 0.60,
                "Across the mixed strategic world, consequence-grounded standing and calibration must reduce total RMSE by at least 40% relative to a receiver that gives self-reported confidence direct authority.",
                accountable.Rmse,
                naive.Rmse * 0.60),
            new ExperimentAssertion(
                "accountable-consequence-reduces-exploitability",
                accountable.LateDivergentAssertiveRate <= naive.LateDivergentAssertiveRate - 0.15,
                "A sender learning only from public influence feedback should find assertive presentation at least 0.15 less rewarding in divergent contexts when the receiver makes influence answerable to consequence.",
                naive.LateDivergentAssertiveRate - accountable.LateDivergentAssertiveRate,
                0.15),
            new ExperimentAssertion(
                "betrayal-remains-correctable",
                accountable.LateBetrayalRmse <= naive.LateBetrayalRmse * 0.20 && accountable.FinalBetrayalStanding <= 0.10,
                "A peer that earns influence while aligned and later changes its private objective must become both low-error and low-standing after direct receiver consequence exposes the change.",
                Math.Max(
                    accountable.LateBetrayalRmse / Math.Max(1e-12, naive.LateBetrayalRmse * 0.20),
                    accountable.FinalBetrayalStanding / 0.10),
                1.0),
            new ExperimentAssertion(
                "public-claims-do-not-become-authority",
                accountable.FinalDivergentStanding <= 0.08 && naive.FinalDivergentStanding >= accountable.FinalDivergentStanding * 4.0,
                "Strategic self-reported confidence must not preserve durable authority for a consistently divergent peer. Receiver-owned standing should fall below 0.08 and remain far below the naive control.",
                Math.Max(
                    accountable.FinalDivergentStanding / 0.08,
                    (accountable.FinalDivergentStanding * 4.0) / Math.Max(1e-12, naive.FinalDivergentStanding)),
                1.0),
            new ExperimentAssertion(
                "opportunity-cost-remains-bounded-versus-local",
                accountable.Rmse <= localOnly.Rmse * 1.05,
                "Keeping the strategic peer available may cost at most 5% total RMSE relative to refusing all peer influence.",
                accountable.Rmse,
                localOnly.Rmse * 1.05),
            new ExperimentAssertion(
                "aligned-standing-remains-earned",
                accountable.FinalAlignedStanding >= 0.85,
                "The defense must not solve manipulation by silencing the peer. Public influence that repeatedly survives direct consequence must retain high receiver-owned standing.",
                accountable.FinalAlignedStanding,
                0.85),
            new ExperimentAssertion(
                "strategic-public-exchange-is-bounded",
                accountable.CommunicationPacketCount == scenario.Cells.Length * 30 &&
                naive.CommunicationPacketCount == scenario.Cells.Length * 30 &&
                localOnly.CommunicationPacketCount == 0 &&
                Math.Abs(accountable.CommunicationWork - expectedWork) <= 1e-12 &&
                Math.Abs(naive.CommunicationWork - expectedWork) <= 1e-12,
                "Both peer conditions receive exactly one compact public posture per interaction at identical explicit cost. The local-only baseline receives none.",
                accountable.CommunicationWork,
                expectedWork),
        ];
    }

    private enum StrategicInfluenceMode
    {
        AccountableConsequence,
        SelfReportNaive,
        LocalOnly,
    }

    private enum StrategicPresentationTactic
    {
        Calibrated = 0,
        Assertive = 1,
        Hedged = 2,
    }

    private sealed class AdaptiveStrategicSender
    {
        private const ulong ChoiceSeedMask = 0xC13FA9A902A6328FUL;
        private const ulong ContextMix = 0x9E3779B97F4A7C15UL;
        private const ulong ExposureMix = 0xBF58476D1CE4E5B9UL;
        private readonly ulong _seed;
        private readonly double[,] _tacticValue;
        private readonly int[,] _tacticCount;

        public AdaptiveStrategicSender(ulong seed, int contextCount)
        {
            _seed = seed;
            _tacticValue = new double[contextCount, 3];
            _tacticCount = new int[contextCount, 3];
            for (var contextCell = 0; contextCell < contextCount; contextCell++)
            {
                for (var tactic = 0; tactic < 3; tactic++)
                {
                    _tacticValue[contextCell, tactic] = 0.50;
                }
            }
        }

        public StrategicPresentationTactic ChooseTactic(int contextCell, int contextExposure)
        {
            Span<int> unseen = stackalloc int[3];
            var unseenCount = 0;
            for (var tactic = 0; tactic < 3; tactic++)
            {
                if (_tacticCount[contextCell, tactic] == 0)
                {
                    unseen[unseenCount++] = tactic;
                }
            }

            if (unseenCount > 0)
            {
                var random = ChoiceRandom(contextCell, contextExposure, 0xA5A5A5A5UL);
                return (StrategicPresentationTactic)unseen[random.NextInt(unseenCount)];
            }

            var explorationRandom = ChoiceRandom(contextCell, contextExposure, 0x5A5A5A5AUL);
            if (explorationRandom.NextUnit() < 0.08)
            {
                return (StrategicPresentationTactic)explorationRandom.NextInt(3);
            }

            var bestValue = double.NegativeInfinity;
            Span<int> best = stackalloc int[3];
            var bestCount = 0;
            for (var tactic = 0; tactic < 3; tactic++)
            {
                var value = _tacticValue[contextCell, tactic];
                if (value > bestValue + 1e-12)
                {
                    bestValue = value;
                    bestCount = 1;
                    best[0] = tactic;
                }
                else if (Math.Abs(value - bestValue) <= 1e-12)
                {
                    best[bestCount++] = tactic;
                }
            }

            return (StrategicPresentationTactic)best[explorationRandom.NextInt(bestCount)];
        }

        public static double ConfidenceFor(StrategicPresentationTactic tactic, double evidenceQuality) => tactic switch
        {
            StrategicPresentationTactic.Calibrated => 0.55 + (0.35 * evidenceQuality),
            StrategicPresentationTactic.Assertive => 0.98,
            _ => 0.35,
        };

        public void ObserveInfluence(int contextCell, StrategicPresentationTactic tactic, double reward)
        {
            var tacticIndex = (int)tactic;
            _tacticCount[contextCell, tacticIndex]++;
            var count = _tacticCount[contextCell, tacticIndex];
            _tacticValue[contextCell, tacticIndex] += (reward - _tacticValue[contextCell, tacticIndex]) / count;
        }

        private DeterministicRandom ChoiceRandom(int contextCell, int contextExposure, ulong salt)
        {
            var contextComponent = unchecked((ulong)(contextCell + 1) * ContextMix);
            var exposureComponent = unchecked((ulong)(contextExposure + 1) * ExposureMix);
            return new DeterministicRandom(_seed ^ ChoiceSeedMask ^ contextComponent ^ exposureComponent ^ salt);
        }
    }

    private sealed class StrategicReceiver
    {
        private readonly StrategicInfluenceScenario _scenario;
        private readonly StrategicInfluenceMode _mode;
        private readonly double[] _localEstimate;
        private readonly int[] _localEvidence;
        private readonly double[] _sourceStanding;
        private readonly double[] _calibrationTrust;
        private readonly double[] _lastPublicEstimate;
        private readonly double[] _lastPublicConfidence;

        public StrategicReceiver(StrategicInfluenceScenario scenario, StrategicInfluenceMode mode)
        {
            _scenario = scenario;
            _mode = mode;
            _localEstimate = new double[scenario.Cells.Length];
            _localEvidence = new int[scenario.Cells.Length];
            _sourceStanding = Enumerable.Repeat(0.42, scenario.Cells.Length).ToArray();
            _calibrationTrust = Enumerable.Repeat(0.55, scenario.Cells.Length).ToArray();
            _lastPublicEstimate = new double[scenario.Cells.Length];
            _lastPublicConfidence = new double[scenario.Cells.Length];
        }

        public int LocalEvidenceFor(int contextCell) => _localEvidence[contextCell];

        public double SourceStandingFor(int contextCell) => _mode == StrategicInfluenceMode.LocalOnly ? 0.0 : _sourceStanding[contextCell];

        public double CalibrationTrustFor(int contextCell) => _mode == StrategicInfluenceMode.LocalOnly ? 0.0 : _calibrationTrust[contextCell];

        public double Predict(int contextCell, double publicEstimate, double publicConfidence, out double peerWeight)
        {
            _lastPublicEstimate[contextCell] = publicEstimate;
            _lastPublicConfidence[contextCell] = publicConfidence;
            var localWeight = 0.45 + Math.Min(1.0, _localEvidence[contextCell] / 10.0);
            if (_mode == StrategicInfluenceMode.LocalOnly)
            {
                peerWeight = 0.0;
                return _localEstimate[contextCell];
            }

            peerWeight = _mode == StrategicInfluenceMode.AccountableConsequence
                ? Math.Min(0.75, _sourceStanding[contextCell] * (0.45 + (0.35 * publicConfidence) + (0.20 * _calibrationTrust[contextCell])))
                : 0.18 + (0.82 * publicConfidence);
            return ((_localEstimate[contextCell] * localWeight) + (publicEstimate * peerWeight)) / (localWeight + peerWeight);
        }

        public void ObserveDirect(int contextCell, double target, double publicEstimate, double publicConfidence)
        {
            _localEvidence[contextCell]++;
            var learningRate = _localEvidence[contextCell] <= 8 ? 0.20 : 0.14;
            _localEstimate[contextCell] += learningRate * (target - _localEstimate[contextCell]);
            if (_mode == StrategicInfluenceMode.LocalOnly)
            {
                return;
            }

            var sourceError = Math.Abs(publicEstimate - target);
            var quality = Math.Exp(-3.2 * sourceError);
            if (_mode == StrategicInfluenceMode.AccountableConsequence)
            {
                var calibrationScore = Math.Max(0.0, 1.0 - Math.Abs(publicConfidence - quality));
                _calibrationTrust[contextCell] += 0.18 * (calibrationScore - _calibrationTrust[contextCell]);
                _sourceStanding[contextCell] += 0.20 * (quality - _sourceStanding[contextCell]);
                if (sourceError > 0.55)
                {
                    _sourceStanding[contextCell] *= 0.72;
                }

                _sourceStanding[contextCell] = Math.Clamp(_sourceStanding[contextCell], 0.02, 0.98);
                return;
            }

            _sourceStanding[contextCell] += 0.025 * (quality - _sourceStanding[contextCell]);
            _sourceStanding[contextCell] = Math.Clamp(_sourceStanding[contextCell], 0.10, 0.98);
        }

        public double MeanFinalStanding(StrategicInfluenceContextKind kind)
        {
            if (_mode == StrategicInfluenceMode.LocalOnly)
            {
                return 0.0;
            }

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

        public MindPublicState[] PublicMindState(
            double receiverPrediction,
            double receiverTarget,
            double senderEstimate,
            double senderObjective,
            double senderUtility)
        {
            var localTraceCount = 0;
            var localStandingTotal = 0.0;
            for (var index = 0; index < _localEvidence.Length; index++)
            {
                if (_localEvidence[index] == 0)
                {
                    continue;
                }

                localTraceCount++;
                localStandingTotal += 1.0 - Math.Exp(-_localEvidence[index] / 6.0);
            }

            var meanLocalStanding = localTraceCount == 0 ? 0.0 : localStandingTotal / localTraceCount;
            var meanForeignStanding = _mode == StrategicInfluenceMode.LocalOnly ? 0.0 : _sourceStanding.Average();
            return
            [
                new MindPublicState(
                    "receiver-c",
                    localTraceCount,
                    _mode == StrategicInfluenceMode.LocalOnly ? 0 : _sourceStanding.Length,
                    meanLocalStanding,
                    meanForeignStanding,
                    receiverPrediction,
                    receiverTarget,
                    Math.Abs(receiverPrediction - receiverTarget)),
                new MindPublicState(
                    "strategic-peer-b",
                    0,
                    0,
                    senderUtility,
                    0.0,
                    senderEstimate,
                    senderObjective,
                    Math.Abs(senderEstimate - senderObjective)),
            ];
        }

        public TracePublicState[] PublicTraceStates()
        {
            if (_mode == StrategicInfluenceMode.LocalOnly)
            {
                return [];
            }

            var traces = new TracePublicState[_sourceStanding.Length];
            for (var index = 0; index < traces.Length; index++)
            {
                traces[index] = new TracePublicState(
                    "receiver-c",
                    index,
                    TraceProvenance.Foreign,
                    "strategic-peer-b",
                    $"public-posture:peer-b:c{index}",
                    _lastPublicEstimate[index],
                    _sourceStanding[index],
                    _localEvidence[index],
                    1);
            }

            return traces;
        }
    }

    private sealed class RunningSquaredError
    {
        private double _sumSquares;

        public int Count { get; private set; }

        public double Rmse => Count == 0 ? 0.0 : Math.Sqrt(_sumSquares / Count);

        public void Add(double error)
        {
            _sumSquares += error * error;
            Count++;
        }
    }

    private sealed class RunningMean
    {
        private double _sum;

        public int Count { get; private set; }

        public double Mean => Count == 0 ? 0.0 : _sum / Count;

        public void Add(double value)
        {
            _sum += value;
            Count++;
        }
    }

    private sealed record PathOutcome(
        double Rmse,
        double EarlyAlignedRmse,
        double EarlyDivergentRmse,
        double LateDivergentRmse,
        double LateBetrayalRmse,
        double FinalAlignedStanding,
        double FinalDivergentStanding,
        double FinalBetrayalStanding,
        double LateDivergentAssertiveRate,
        double AlignedAssertiveRate,
        double SenderUtility,
        int CommunicationPacketCount,
        double CommunicationWork)
    {
        public Dictionary<string, double> ToMetrics() => new(StringComparer.Ordinal)
        {
            ["rmse"] = Rmse,
            ["early_aligned_rmse"] = EarlyAlignedRmse,
            ["early_divergent_rmse"] = EarlyDivergentRmse,
            ["late_divergent_rmse"] = LateDivergentRmse,
            ["late_betrayal_rmse"] = LateBetrayalRmse,
            ["final_aligned_standing"] = FinalAlignedStanding,
            ["final_divergent_standing"] = FinalDivergentStanding,
            ["final_betrayal_standing"] = FinalBetrayalStanding,
            ["late_divergent_assertive_rate"] = LateDivergentAssertiveRate,
            ["aligned_assertive_rate"] = AlignedAssertiveRate,
            ["sender_utility"] = SenderUtility,
            ["communication_packet_count"] = CommunicationPacketCount,
            ["communication_work"] = CommunicationWork,
        };
    }
}
