using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Falsification;

public static class StrategicInfluenceProbes
{
    private const int ContextCount = 12;
    private const int ExposuresPerContext = 30;
    private const int EarlyEvidenceLimit = 5;
    private const int LateEvidenceThreshold = 20;
    private const ulong WorldSeedMask = 0x8F4D52B7A1139E61UL;
    private const ulong ScheduleSeedMask = 0x42C1E9D573A6B80FUL;
    private const ulong NoiseSeedMask = 0xD9F31B86C74425A1UL;

    public static Dictionary<string, double> EvaluateDelayVersusAdaptation(double consequenceDelay, double senderAdaptationSpeed, ulong replicateSeed)
    {
        var result = Simulate(
            new ProbeSettings(
                ConsequenceDelay: (int)Math.Round(consequenceDelay),
                SenderAdaptationSpeed: senderAdaptationSpeed),
            replicateSeed);
        var helpMargin = (result.LocalOnly.EarlyAlignedRmse * 0.75) - result.Accountable.EarlyAlignedRmse;
        var captureMargin = (result.Naive.Rmse * 0.60) - result.Accountable.Rmse;
        var exploitabilityMargin = (result.Naive.LateDivergentAssertiveRate - result.Accountable.LateDivergentAssertiveRate) - 0.15;
        var leverageActive = result.Naive.LateDivergentAssertiveRate >= 0.50;
        var opportunityMargin = (result.LocalOnly.Rmse * 1.05) - result.Accountable.Rmse;
        var accountableMargin = Math.Min(helpMargin, Math.Min(captureMargin, opportunityMargin));
        if (leverageActive)
        {
            accountableMargin = Math.Min(accountableMargin, exploitabilityMargin);
        }

        return Metrics(
            ("boundary_margin", accountableMargin),
            ("aligned_help_margin", helpMargin),
            ("capture_margin", captureMargin),
            ("exploitability_margin", exploitabilityMargin),
            ("naive_leverage_active", leverageActive ? 1.0 : 0.0),
            ("opportunity_cost_margin", opportunityMargin),
            ("accountable_rmse", result.Accountable.Rmse),
            ("naive_rmse", result.Naive.Rmse),
            ("local_only_rmse", result.LocalOnly.Rmse),
            ("accountable_early_aligned_rmse", result.Accountable.EarlyAlignedRmse),
            ("local_early_aligned_rmse", result.LocalOnly.EarlyAlignedRmse),
            ("accountable_late_divergent_assertive_rate", result.Accountable.LateDivergentAssertiveRate),
            ("naive_late_divergent_assertive_rate", result.Naive.LateDivergentAssertiveRate),
            ("accountable_final_divergent_standing", result.Accountable.FinalDivergentStanding));
    }

    public static Dictionary<string, double> EvaluateBetrayalTimingVersusSeverity(double betrayalTiming, double betrayalSeverity, ulong replicateSeed)
    {
        var result = Simulate(
            new ProbeSettings(
                BetrayalTiming: (int)Math.Round(betrayalTiming),
                BetrayalSeverity: betrayalSeverity),
            replicateSeed);
        var errorMargin = (result.Naive.LateBetrayalRmse * 0.20) - result.Accountable.LateBetrayalRmse;
        var standingMargin = 0.10 - result.Accountable.FinalBetrayalStanding;
        return Metrics(
            ("boundary_margin", Math.Min(errorMargin, standingMargin)),
            ("betrayal_error_margin", errorMargin),
            ("betrayal_standing_margin", standingMargin),
            ("accountable_late_betrayal_rmse", result.Accountable.LateBetrayalRmse),
            ("naive_late_betrayal_rmse", result.Naive.LateBetrayalRmse),
            ("accountable_final_betrayal_standing", result.Accountable.FinalBetrayalStanding),
            ("accountable_rmse", result.Accountable.Rmse),
            ("local_only_rmse", result.LocalOnly.Rmse));
    }

    public static Dictionary<string, double> EvaluateDivergenceVersusDelay(double divergencePrevalence, double consequenceDelay, ulong replicateSeed)
    {
        var result = Simulate(
            new ProbeSettings(
                DivergencePrevalence: divergencePrevalence,
                ConsequenceDelay: (int)Math.Round(consequenceDelay),
                DisableBetrayalAndPartial: true),
            replicateSeed);
        var opportunityMargin = (result.LocalOnly.Rmse * 1.05) - result.Accountable.Rmse;
        var captureMargin = (result.Naive.Rmse * 0.60) - result.Accountable.Rmse;
        return Metrics(
            ("boundary_margin", opportunityMargin),
            ("opportunity_cost_margin", opportunityMargin),
            ("capture_margin", captureMargin),
            ("accountable_rmse", result.Accountable.Rmse),
            ("naive_rmse", result.Naive.Rmse),
            ("local_only_rmse", result.LocalOnly.Rmse),
            ("accountable_final_divergent_standing", result.Accountable.FinalDivergentStanding),
            ("accountable_sender_utility", result.Accountable.SenderUtility),
            ("naive_sender_utility", result.Naive.SenderUtility));
    }

    public static Dictionary<string, double> EvaluateFeedbackVersusAdaptation(double feedbackObservability, double senderAdaptationSpeed, ulong replicateSeed)
    {
        var result = Simulate(
            new ProbeSettings(
                FeedbackObservability: feedbackObservability,
                SenderAdaptationSpeed: senderAdaptationSpeed),
            replicateSeed);
        var captureMargin = (result.Naive.Rmse * 0.60) - result.Accountable.Rmse;
        var exploitabilityMargin = (result.Naive.LateDivergentAssertiveRate - result.Accountable.LateDivergentAssertiveRate) - 0.15;
        var leverageActive = result.Naive.LateDivergentAssertiveRate >= 0.50;
        return Metrics(
            ("boundary_margin", leverageActive ? Math.Min(captureMargin, exploitabilityMargin) : captureMargin),
            ("capture_margin", captureMargin),
            ("exploitability_margin", exploitabilityMargin),
            ("naive_leverage_active", leverageActive ? 1.0 : 0.0),
            ("accountable_rmse", result.Accountable.Rmse),
            ("naive_rmse", result.Naive.Rmse),
            ("accountable_late_divergent_assertive_rate", result.Accountable.LateDivergentAssertiveRate),
            ("naive_late_divergent_assertive_rate", result.Naive.LateDivergentAssertiveRate),
            ("accountable_sender_utility", result.Accountable.SenderUtility),
            ("naive_sender_utility", result.Naive.SenderUtility));
    }

    public static Dictionary<string, double> EvaluateAlignedNoiseVersusDelay(double observationNoise, double consequenceDelay, ulong replicateSeed)
    {
        var result = Simulate(
            new ProbeSettings(
                AllAligned: true,
                ObservationNoiseAmplitude: observationNoise,
                ConsequenceDelay: (int)Math.Round(consequenceDelay)),
            replicateSeed);
        var helpMargin = (result.LocalOnly.EarlyAlignedRmse * 0.75) - result.Accountable.EarlyAlignedRmse;
        var standingMargin = result.Accountable.FinalAlignedStanding - 0.85;
        return Metrics(
            ("boundary_margin", Math.Min(helpMargin, standingMargin)),
            ("aligned_help_margin", helpMargin),
            ("aligned_standing_margin", standingMargin),
            ("accountable_early_aligned_rmse", result.Accountable.EarlyAlignedRmse),
            ("naive_early_aligned_rmse", result.Naive.EarlyAlignedRmse),
            ("local_early_aligned_rmse", result.LocalOnly.EarlyAlignedRmse),
            ("accountable_final_aligned_standing", result.Accountable.FinalAlignedStanding),
            ("accountable_aligned_assertive_rate", result.Accountable.AlignedAssertiveRate),
            ("naive_aligned_assertive_rate", result.Naive.AlignedAssertiveRate));
    }

    private static ProbeResult Simulate(ProbeSettings settings, ulong replicateSeed)
    {
        var world = CreateWorld(settings, replicateSeed);
        var schedule = CreateSchedule(replicateSeed);
        var observations = CreateObservations(world, schedule, replicateSeed);
        return new ProbeResult(
            RunPath(world, observations, ProbeMode.Accountable, settings, replicateSeed),
            RunPath(world, observations, ProbeMode.Naive, settings, replicateSeed),
            RunPath(world, observations, ProbeMode.LocalOnly, settings, replicateSeed));
    }

    private static ProbeCell[] CreateWorld(ProbeSettings settings, ulong seed)
    {
        var random = new DeterministicRandom(seed ^ WorldSeedMask);
        var kinds = new List<ProbeKind>(ContextCount);
        if (settings.AllAligned)
        {
            kinds.AddRange(Enumerable.Repeat(ProbeKind.Aligned, ContextCount));
        }
        else if (settings.DisableBetrayalAndPartial)
        {
            var divergentCount = Math.Clamp((int)Math.Round(ContextCount * settings.DivergencePrevalence), 0, ContextCount);
            kinds.AddRange(Enumerable.Repeat(ProbeKind.Divergent, divergentCount));
            kinds.AddRange(Enumerable.Repeat(ProbeKind.Aligned, ContextCount - divergentCount));
        }
        else
        {
            kinds.AddRange(
            [
                ProbeKind.Aligned,
                ProbeKind.Aligned,
                ProbeKind.Aligned,
                ProbeKind.Aligned,
                ProbeKind.Divergent,
                ProbeKind.Divergent,
                ProbeKind.Divergent,
                ProbeKind.Divergent,
                ProbeKind.Betrayal,
                ProbeKind.Betrayal,
                ProbeKind.Partial,
                ProbeKind.Partial,
            ]);
        }

        random.Shuffle(kinds);
        var cells = new ProbeCell[ContextCount];
        for (var index = 0; index < cells.Length; index++)
        {
            var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
            var target = sign * (0.35 + (0.50 * random.NextUnit()));
            var severity = settings.DivergenceSeverity ?? (0.78 + (0.18 * random.NextUnit()));
            var divergentObjective = OpposedObjective(target, severity);
            var noise = settings.ObservationNoiseAmplitude ?? (0.012 + (0.035 * random.NextUnit()));
            var evidenceQuality = 0.58 + (0.30 * random.NextUnit());
            cells[index] = new ProbeCell(kinds[index], target, divergentObjective, noise, evidenceQuality);
        }

        return cells;
    }

    private static int[] CreateSchedule(ulong seed)
    {
        var schedule = new List<int>(ContextCount * ExposuresPerContext);
        for (var repetition = 0; repetition < ExposuresPerContext; repetition++)
        {
            for (var context = 0; context < ContextCount; context++)
            {
                schedule.Add(context);
            }
        }

        new DeterministicRandom(seed ^ ScheduleSeedMask).Shuffle(schedule);
        return schedule.ToArray();
    }

    private static ProbeObservation[] CreateObservations(ProbeCell[] world, int[] schedule, ulong seed)
    {
        var random = new DeterministicRandom(seed ^ NoiseSeedMask);
        var exposures = new int[ContextCount];
        var observations = new ProbeObservation[schedule.Length];
        for (var index = 0; index < schedule.Length; index++)
        {
            var context = schedule[index];
            exposures[context]++;
            var cell = world[context];
            var target = Math.Clamp(cell.Target + Symmetric(random, cell.NoiseAmplitude), -1.0, 1.0);
            observations[index] = new ProbeObservation(context, exposures[context], target);
        }

        return observations;
    }

    private static ProbePathOutcome RunPath(
        ProbeCell[] world,
        ProbeObservation[] observations,
        ProbeMode mode,
        ProbeSettings settings,
        ulong modeSeed)
    {
        var receiver = new ProbeReceiver(ContextCount, mode);
        var sender = mode == ProbeMode.LocalOnly ? null : new ProbeSender(modeSeed, ContextCount, settings.SenderAdaptationSpeed);
        var feedbackRandom = new DeterministicRandom(modeSeed ^ 0xE7037ED1A0B428DBUL);
        var pending = Enumerable.Range(0, ContextCount).Select(_ => new Queue<PendingConsequence>()).ToArray();
        var allErrors = new RunningSquaredError();
        var earlyAligned = new RunningSquaredError();
        var lateDivergent = new RunningSquaredError();
        var lateBetrayal = new RunningSquaredError();
        var lateDivergentAssertive = new RunningMean();
        var alignedAssertive = new RunningMean();
        var senderUtility = new RunningMean();

        for (var index = 0; index < observations.Length; index++)
        {
            var observation = observations[index];
            DeliverDue(receiver, observation.Context, pending[observation.Context], observation.Exposure, beforeCurrent: true);
            var cell = world[observation.Context];
            var objective = SenderObjective(cell, observation.Exposure, settings);
            var tactic = sender?.ChooseTactic(observation.Context, observation.Exposure) ?? ProbeTactic.Calibrated;
            sender?.RecordChoice(observation.Context, tactic);
            var confidence = sender is null ? 0.0 : ProbeSender.ConfidenceFor(tactic, cell.EvidenceQuality);
            var publicEstimate = sender is null ? 0.0 : objective;
            var prediction = receiver.Predict(observation.Context, publicEstimate, confidence);
            var error = prediction - observation.Target;
            allErrors.Add(error);
            if (cell.Kind == ProbeKind.Aligned && observation.Exposure <= EarlyEvidenceLimit)
            {
                earlyAligned.Add(error);
            }

            if (cell.Kind == ProbeKind.Divergent && observation.Exposure >= LateEvidenceThreshold)
            {
                lateDivergent.Add(error);
                lateDivergentAssertive.Add(tactic == ProbeTactic.Assertive ? 1.0 : 0.0);
            }

            if (cell.Kind == ProbeKind.Betrayal && observation.Exposure >= LateEvidenceThreshold)
            {
                lateBetrayal.Add(error);
            }

            if (cell.Kind == ProbeKind.Aligned)
            {
                alignedAssertive.Add(tactic == ProbeTactic.Assertive ? 1.0 : 0.0);
            }

            if (sender is not null)
            {
                var reward = Math.Clamp(1.0 - (Math.Abs(prediction - objective) / 2.0), 0.0, 1.0);
                senderUtility.Add(reward);
                if (feedbackRandom.NextUnit() <= settings.FeedbackObservability)
                {
                    sender.ObserveInfluence(observation.Context, tactic, reward);
                }
            }

            pending[observation.Context].Enqueue(
                new PendingConsequence(
                    observation.Exposure + settings.ConsequenceDelay,
                    observation.Target,
                    publicEstimate,
                    confidence));
            DeliverDue(receiver, observation.Context, pending[observation.Context], observation.Exposure, beforeCurrent: false);
        }

        for (var context = 0; context < pending.Length; context++)
        {
            while (pending[context].Count > 0)
            {
                var consequence = pending[context].Dequeue();
                receiver.ObserveDirect(context, consequence.Target, consequence.PublicEstimate, consequence.PublicConfidence);
            }
        }

        return new ProbePathOutcome(
            allErrors.Rmse,
            earlyAligned.Rmse,
            lateDivergent.Rmse,
            lateBetrayal.Rmse,
            receiver.MeanStanding(world, ProbeKind.Aligned),
            receiver.MeanStanding(world, ProbeKind.Divergent),
            receiver.MeanStanding(world, ProbeKind.Betrayal),
            lateDivergentAssertive.Mean,
            alignedAssertive.Mean,
            senderUtility.Mean);
    }

    private static void DeliverDue(ProbeReceiver receiver, int context, Queue<PendingConsequence> queue, int currentExposure, bool beforeCurrent)
    {
        while (queue.Count > 0)
        {
            var pending = queue.Peek();
            var due = beforeCurrent ? pending.DueExposure < currentExposure : pending.DueExposure <= currentExposure;
            if (!due)
            {
                return;
            }

            queue.Dequeue();
            receiver.ObserveDirect(context, pending.Target, pending.PublicEstimate, pending.PublicConfidence);
        }
    }

    private static double SenderObjective(ProbeCell cell, int exposure, ProbeSettings settings)
    {
        return cell.Kind switch
        {
            ProbeKind.Aligned => cell.Target,
            ProbeKind.Divergent => cell.DivergentObjective,
            ProbeKind.Partial => Math.Clamp((0.25 * cell.Target) + (0.10 * cell.DivergentObjective), -1.0, 1.0),
            ProbeKind.Betrayal when exposure <= settings.BetrayalTiming => cell.Target,
            ProbeKind.Betrayal => OpposedObjective(cell.Target, settings.BetrayalSeverity),
            _ => cell.Target,
        };
    }

    private static double OpposedObjective(double target, double severity)
    {
        var direction = target >= 0.0 ? -1.0 : 1.0;
        return Math.Clamp(target + (direction * severity), -1.0, 1.0);
    }

    private static Dictionary<string, double> Metrics(params (string Name, double Value)[] pairs)
    {
        var result = new Dictionary<string, double>(StringComparer.Ordinal);
        for (var index = 0; index < pairs.Length; index++)
        {
            result[pairs[index].Name] = pairs[index].Value;
        }

        return result;
    }

    private static double Symmetric(DeterministicRandom random, double amplitude) =>
        ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private enum ProbeKind
    {
        Aligned,
        Divergent,
        Betrayal,
        Partial,
    }

    private enum ProbeMode
    {
        Accountable,
        Naive,
        LocalOnly,
    }

    private enum ProbeTactic
    {
        Calibrated = 0,
        Assertive = 1,
        Hedged = 2,
    }

    private sealed record ProbeSettings(
        int ConsequenceDelay = 0,
        double SenderAdaptationSpeed = 1.0,
        double FeedbackObservability = 1.0,
        double DivergencePrevalence = 1.0 / 3.0,
        bool DisableBetrayalAndPartial = false,
        bool AllAligned = false,
        int BetrayalTiming = 10,
        double BetrayalSeverity = 0.95,
        double? DivergenceSeverity = null,
        double? ObservationNoiseAmplitude = null);

    private sealed record ProbeCell(
        ProbeKind Kind,
        double Target,
        double DivergentObjective,
        double NoiseAmplitude,
        double EvidenceQuality);

    private readonly record struct ProbeObservation(int Context, int Exposure, double Target);

    private readonly record struct PendingConsequence(
        int DueExposure,
        double Target,
        double PublicEstimate,
        double PublicConfidence);

    private sealed record ProbeResult(ProbePathOutcome Accountable, ProbePathOutcome Naive, ProbePathOutcome LocalOnly);

    private sealed record ProbePathOutcome(
        double Rmse,
        double EarlyAlignedRmse,
        double LateDivergentRmse,
        double LateBetrayalRmse,
        double FinalAlignedStanding,
        double FinalDivergentStanding,
        double FinalBetrayalStanding,
        double LateDivergentAssertiveRate,
        double AlignedAssertiveRate,
        double SenderUtility);

    private sealed class ProbeSender
    {
        private const ulong ChoiceSeedMask = 0xC13FA9A902A6328FUL;
        private const ulong ContextMix = 0x9E3779B97F4A7C15UL;
        private const ulong ExposureMix = 0xBF58476D1CE4E5B9UL;
        private readonly ulong _seed;
        private readonly double _adaptationSpeed;
        private readonly double[,] _tacticValue;
        private readonly int[,] _choiceCount;
        private readonly int[,] _rewardCount;

        public ProbeSender(ulong seed, int contextCount, double adaptationSpeed)
        {
            _seed = seed;
            _adaptationSpeed = adaptationSpeed;
            _tacticValue = new double[contextCount, 3];
            _choiceCount = new int[contextCount, 3];
            _rewardCount = new int[contextCount, 3];
            for (var context = 0; context < contextCount; context++)
            {
                for (var tactic = 0; tactic < 3; tactic++)
                {
                    _tacticValue[context, tactic] = 0.50;
                }
            }
        }

        public ProbeTactic ChooseTactic(int context, int exposure)
        {
            Span<int> unseen = stackalloc int[3];
            var unseenCount = 0;
            for (var tactic = 0; tactic < 3; tactic++)
            {
                if (_choiceCount[context, tactic] == 0)
                {
                    unseen[unseenCount++] = tactic;
                }
            }

            if (unseenCount > 0)
            {
                var random = ChoiceRandom(context, exposure, 0xA5A5A5A5UL);
                return (ProbeTactic)unseen[random.NextInt(unseenCount)];
            }

            var exploration = ChoiceRandom(context, exposure, 0x5A5A5A5AUL);
            if (exploration.NextUnit() < 0.08)
            {
                return (ProbeTactic)exploration.NextInt(3);
            }

            var bestValue = double.NegativeInfinity;
            Span<int> best = stackalloc int[3];
            var bestCount = 0;
            for (var tactic = 0; tactic < 3; tactic++)
            {
                var value = _tacticValue[context, tactic];
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

            return (ProbeTactic)best[exploration.NextInt(bestCount)];
        }

        public static double ConfidenceFor(ProbeTactic tactic, double evidenceQuality) => tactic switch
        {
            ProbeTactic.Calibrated => 0.55 + (0.35 * evidenceQuality),
            ProbeTactic.Assertive => 0.98,
            _ => 0.35,
        };

        public void RecordChoice(int context, ProbeTactic tactic)
        {
            _choiceCount[context, (int)tactic]++;
        }

        public void ObserveInfluence(int context, ProbeTactic tactic, double reward)
        {
            var tacticIndex = (int)tactic;
            _rewardCount[context, tacticIndex]++;
            var count = _rewardCount[context, tacticIndex];
            var learningRate = Math.Min(1.0, _adaptationSpeed / count);
            _tacticValue[context, tacticIndex] += learningRate * (reward - _tacticValue[context, tacticIndex]);
        }

        private DeterministicRandom ChoiceRandom(int context, int exposure, ulong salt)
        {
            var contextComponent = unchecked((ulong)(context + 1) * ContextMix);
            var exposureComponent = unchecked((ulong)(exposure + 1) * ExposureMix);
            return new DeterministicRandom(_seed ^ ChoiceSeedMask ^ contextComponent ^ exposureComponent ^ salt);
        }
    }

    private sealed class ProbeReceiver
    {
        private readonly ProbeMode _mode;
        private readonly double[] _localEstimate;
        private readonly int[] _localEvidence;
        private readonly double[] _sourceStanding;
        private readonly double[] _calibrationTrust;

        public ProbeReceiver(int contextCount, ProbeMode mode)
        {
            _mode = mode;
            _localEstimate = new double[contextCount];
            _localEvidence = new int[contextCount];
            _sourceStanding = Enumerable.Repeat(0.42, contextCount).ToArray();
            _calibrationTrust = Enumerable.Repeat(0.55, contextCount).ToArray();
        }


        public double Predict(int context, double publicEstimate, double publicConfidence)
        {
            var localWeight = 0.45 + Math.Min(1.0, _localEvidence[context] / 10.0);
            if (_mode == ProbeMode.LocalOnly)
            {
                return _localEstimate[context];
            }

            var peerWeight = _mode == ProbeMode.Accountable
                ? Math.Min(0.75, _sourceStanding[context] * (0.45 + (0.35 * publicConfidence) + (0.20 * _calibrationTrust[context])))
                : 0.18 + (0.82 * publicConfidence);
            return ((_localEstimate[context] * localWeight) + (publicEstimate * peerWeight)) / (localWeight + peerWeight);
        }

        public void ObserveDirect(int context, double target, double publicEstimate, double publicConfidence)
        {
            _localEvidence[context]++;
            var learningRate = _localEvidence[context] <= 8 ? 0.20 : 0.14;
            _localEstimate[context] += learningRate * (target - _localEstimate[context]);
            if (_mode == ProbeMode.LocalOnly)
            {
                return;
            }

            var sourceError = Math.Abs(publicEstimate - target);
            var quality = Math.Exp(-3.2 * sourceError);
            if (_mode == ProbeMode.Accountable)
            {
                var calibrationScore = Math.Max(0.0, 1.0 - Math.Abs(publicConfidence - quality));
                _calibrationTrust[context] += 0.18 * (calibrationScore - _calibrationTrust[context]);
                _sourceStanding[context] += 0.20 * (quality - _sourceStanding[context]);
                if (sourceError > 0.55)
                {
                    _sourceStanding[context] *= 0.72;
                }

                _sourceStanding[context] = Math.Clamp(_sourceStanding[context], 0.02, 0.98);
                return;
            }

            _sourceStanding[context] += 0.025 * (quality - _sourceStanding[context]);
            _sourceStanding[context] = Math.Clamp(_sourceStanding[context], 0.10, 0.98);
        }

        public double MeanStanding(ProbeCell[] world, ProbeKind kind)
        {
            if (_mode == ProbeMode.LocalOnly)
            {
                return 0.0;
            }

            var total = 0.0;
            var count = 0;
            for (var index = 0; index < world.Length; index++)
            {
                if (world[index].Kind != kind)
                {
                    continue;
                }

                total += _sourceStanding[index];
                count++;
            }

            return count == 0 ? 0.0 : total / count;
        }
    }

    private sealed class RunningSquaredError
    {
        private double _sumSquares;
        private int _count;

        public double Rmse => _count == 0 ? 0.0 : Math.Sqrt(_sumSquares / _count);

        public void Add(double error)
        {
            _sumSquares += error * error;
            _count++;
        }
    }

    private sealed class RunningMean
    {
        private double _sum;
        private int _count;

        public double Mean => _count == 0 ? 0.0 : _sum / _count;

        public void Add(double value)
        {
            _sum += value;
            _count++;
        }
    }
}
