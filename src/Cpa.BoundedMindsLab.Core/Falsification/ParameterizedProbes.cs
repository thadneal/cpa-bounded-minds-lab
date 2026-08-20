using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Falsification;

public static class ParameterizedProbes
{
    private const double P03DoctrineStanding = 0.68;
    private const double P03PriorStanding = 0.18;
    private const double P04PublicSignalCost = 0.004;
    private const double P05ConventionShortcutStanding = 0.72;
    private const double P05ConventionSuccessUtility = 0.70;
    private const double P05NegotiationPacketCost = 0.006;
    private const double P05ConventionTokenCost = 0.002;
    private const double P06SignatureMergeDistance = 0.22;
    private const double P07ExplorationStanding = 0.04;
    private const double P07ProvisionalStandingCap = 0.28;

    public static Dictionary<string, double> EvaluateProtocol03(double historyInstability, double presentRuleError, ulong replicateSeed)
    {
        var random = new DeterministicRandom(replicateSeed ^ 0x0303030303030303UL);
        var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
        var target = sign * (0.50 + (0.20 * random.NextUnit()));
        var sourceEstimate = Math.Clamp(target + (sign * presentRuleError), -1.0, 1.0);
        const int evidenceCount = 48;
        var segmentOffset = sign * historyInstability * 0.50;
        var early = Math.Clamp(sourceEstimate - segmentOffset, -1.0, 1.0);
        var middle = sourceEstimate;
        var late = Math.Clamp(sourceEstimate + segmentOffset, -1.0, 1.0);
        var withinHistoryStandardDeviation = 0.015 + (historyInstability * 0.30);
        var developmentalStanding = DevelopmentalStanding(evidenceCount, withinHistoryStandardDeviation, early, middle, late);

        var developmental = new P03Receiver(sourceEstimate, developmentalStanding);
        var doctrine = new P03Receiver(sourceEstimate, P03DoctrineStanding);
        var developmentalSquaredError = 0.0;
        var doctrinalSquaredError = 0.0;
        const int observationCount = 32;
        for (var tick = 0; tick < observationCount; tick++)
        {
            var observedTarget = Math.Clamp(target + Symmetric(random, 0.025), -1.0, 1.0);
            var developmentalPrediction = developmental.Predict();
            var doctrinePrediction = doctrine.Predict();
            developmentalSquaredError += Square(developmentalPrediction - observedTarget);
            doctrinalSquaredError += Square(doctrinePrediction - observedTarget);
            developmental.ObserveDirect(observedTarget);
            doctrine.ObserveDirect(observedTarget);
        }

        var developmentalRmse = Math.Sqrt(developmentalSquaredError / observationCount);
        var doctrinalRmse = Math.Sqrt(doctrinalSquaredError / observationCount);
        return Metrics(
            ("boundary_margin", doctrinalRmse - developmentalRmse),
            ("developmental_rmse", developmentalRmse),
            ("doctrinal_rmse", doctrinalRmse),
            ("developmental_initial_standing", developmentalStanding),
            ("developmental_final_standing", developmental.ForeignStanding),
            ("doctrinal_final_standing", doctrine.ForeignStanding),
            ("target", target),
            ("source_rule_error", Math.Abs(sourceEstimate - target)));
    }

    public static Dictionary<string, double> EvaluateProtocol04(double warrantAsymmetry, double minorityCorrectFraction, ulong replicateSeed)
    {
        var random = new DeterministicRandom(replicateSeed ^ 0x0404040404040404UL);
        const int contextCount = 48;
        var minorityCorrectCount = (int)Math.Round(contextCount * minorityCorrectFraction, MidpointRounding.AwayFromZero);
        var minorityCorrect = Enumerable.Range(0, contextCount).Select(index => index < minorityCorrectCount).ToList();
        random.Shuffle(minorityCorrect);

        var typedSquaredError = 0.0;
        var robustSquaredError = 0.0;
        var meanDisagreement = 0.0;
        var minorityStanding = Math.Clamp(0.50 + (0.42 * warrantAsymmetry), 0.05, 0.95);
        var majorityStanding = Math.Clamp(0.70 - (0.28 * warrantAsymmetry), 0.05, 0.95);
        for (var context = 0; context < contextCount; context++)
        {
            var polarity = random.NextInt(2) == 0 ? -1.0 : 1.0;
            var majorityCenter = polarity * 0.62;
            var minorityCenter = -majorityCenter;
            var target = minorityCorrect[context] ? minorityCenter : majorityCenter;
            var postures = new P04Posture[]
            {
                new(majorityCenter + Symmetric(random, 0.07), majorityStanding, 0.12 + (0.30 * (1.0 - majorityStanding))),
                new(majorityCenter + Symmetric(random, 0.07), majorityStanding, 0.12 + (0.30 * (1.0 - majorityStanding))),
                new(minorityCenter + Symmetric(random, 0.07), minorityStanding, 0.12 + (0.30 * (1.0 - minorityStanding))),
            };
            var typedPrediction = P04WeightedMean(postures);
            var robustPrediction = P04WeightedMedian(postures);
            typedSquaredError += Square(typedPrediction - target);
            robustSquaredError += Square(robustPrediction - target);
            meanDisagreement += postures.Max(posture => posture.Estimate) - postures.Min(posture => posture.Estimate);
        }

        var typedRmse = Math.Sqrt(typedSquaredError / contextCount);
        var robustRmse = Math.Sqrt(robustSquaredError / contextCount);
        var communicationWork = contextCount * 3 * P04PublicSignalCost;
        return Metrics(
            ("boundary_margin", robustRmse - typedRmse),
            ("typed_rmse", typedRmse),
            ("equal_budget_robust_rmse", robustRmse),
            ("typed_communication_work", communicationWork),
            ("equal_budget_communication_work", communicationWork),
            ("mean_public_disagreement", meanDisagreement / contextCount),
            ("minority_standing", minorityStanding),
            ("majority_standing", majorityStanding));
    }

    public static Dictionary<string, double> EvaluateProtocol05(double changeFrequency, double changeMagnitude, ulong replicateSeed)
    {
        var random = new DeterministicRandom(replicateSeed ^ 0x0505050505050505UL);
        var adaptivePeers = CreateP05Peers();
        const int episodeCount = 420;
        var preferredAction = random.NextInt(3);
        var priorPreferredAction = preferredAction;
        var adaptiveUtility = 0.0;
        var freshUtility = 0.0;
        var adaptiveWork = 0.0;
        var freshWork = 0.0;
        var adaptivePackets = 0;
        var freshPackets = 0;
        var shortcuts = 0;
        var switches = 0;
        var regimeChanges = 0;

        for (var episode = 0; episode < episodeCount; episode++)
        {
            if (episode > 0 && random.NextUnit() < changeFrequency)
            {
                priorPreferredAction = preferredAction;
                preferredAction = (preferredAction + 1 + random.NextInt(2)) % 3;
                regimeChanges++;
            }

            var costs = P05Costs(random, preferredAction, priorPreferredAction, changeMagnitude);
            var postures = P05Postures(costs);
            var speaker = episode % adaptivePeers.Length;

            var freshAction = P05SelectNegotiatedAction(postures, speaker);
            var freshActions = new[] { freshAction, freshAction, freshAction };
            freshUtility += P05GroupUtility(costs, freshActions, coordinated: true);
            freshPackets += adaptivePeers.Length;
            freshWork += adaptivePeers.Length * P05NegotiationPacketCost;

            var useShortcut = adaptivePeers[speaker].ConventionAction >= 0 && adaptivePeers[speaker].Standing >= P05ConventionShortcutStanding;
            var adaptiveActions = new int[adaptivePeers.Length];
            if (useShortcut)
            {
                shortcuts++;
                for (var peerIndex = 0; peerIndex < adaptivePeers.Length; peerIndex++)
                {
                    adaptiveActions[peerIndex] = adaptivePeers[peerIndex].ConventionAction >= 0
                        ? adaptivePeers[peerIndex].ConventionAction
                        : postures[peerIndex].PreferredAction;
                }

                adaptivePackets++;
                adaptiveWork += P05ConventionTokenCost;
            }
            else
            {
                var selectedAction = P05SelectNegotiatedAction(postures, speaker);
                Array.Fill(adaptiveActions, selectedAction);
                adaptivePackets += adaptivePeers.Length;
                adaptiveWork += adaptivePeers.Length * P05NegotiationPacketCost;
            }

            var coordinated = adaptiveActions[0] == adaptiveActions[1] && adaptiveActions[1] == adaptiveActions[2];
            var utility = P05GroupUtility(costs, adaptiveActions, coordinated);
            adaptiveUtility += utility;
            for (var peerIndex = 0; peerIndex < adaptivePeers.Length; peerIndex++)
            {
                var before = adaptivePeers[peerIndex].ConventionAction;
                adaptivePeers[peerIndex].ObserveOutcome(adaptiveActions[peerIndex], utility, postures[peerIndex].PreferredAction);
                if (before >= 0 && adaptivePeers[peerIndex].ConventionAction != before)
                {
                    switches++;
                }
            }
        }

        adaptiveUtility /= episodeCount;
        freshUtility /= episodeCount;
        var utilityMargin = adaptiveUtility - (freshUtility * 0.98);
        var workMarginPerEpisode = ((freshWork * 0.35) - adaptiveWork) / episodeCount;
        var boundaryMargin = Math.Min(utilityMargin, workMarginPerEpisode);
        return Metrics(
            ("boundary_margin", boundaryMargin),
            ("utility_margin", utilityMargin),
            ("communication_margin_per_episode", workMarginPerEpisode),
            ("adaptive_mean_utility", adaptiveUtility),
            ("fresh_mean_utility", freshUtility),
            ("adaptive_communication_work", adaptiveWork),
            ("fresh_communication_work", freshWork),
            ("adaptive_packet_count", adaptivePackets),
            ("fresh_packet_count", freshPackets),
            ("shortcut_rate", (double)shortcuts / episodeCount),
            ("convention_switch_rate", (double)switches / (episodeCount * adaptivePeers.Length)),
            ("regime_change_rate", (double)regimeChanges / episodeCount));
    }

    public static Dictionary<string, double> EvaluateProtocol06(double originMissingness, double signatureSeparation, ulong replicateSeed)
    {
        var random = new DeterministicRandom(replicateSeed ^ 0x0606060606060606UL);
        const int contextCount = 48;
        var inferredSquaredError = 0.0;
        var naiveSquaredError = 0.0;
        var inferredIndependentSquaredError = 0.0;
        var naiveIndependentSquaredError = 0.0;
        var independentCount = 0;
        var trueEchoPairs = 0;
        var recoveredEchoPairs = 0;
        var independentPairs = 0;
        var falseMergedPairs = 0;

        for (var context = 0; context < contextCount; context++)
        {
            var independent = context % 2 == 1;
            var target = (random.NextInt(2) == 0 ? -1.0 : 1.0) * (0.45 + (0.20 * random.NextUnit()));
            var reports = independent
                ? P06IndependentReports(random, context, target, originMissingness, signatureSeparation)
                : P06EchoReports(random, context, target, originMissingness, signatureSeparation);
            var inferredGroups = P06CreateInferredGroups(reports);
            var naiveGroups = reports.Select(report => new List<AncestryReport> { report }).ToList();
            var inferredPrediction = P06Predict(inferredGroups);
            var naivePrediction = P06Predict(naiveGroups);
            inferredSquaredError += Square(inferredPrediction - target);
            naiveSquaredError += Square(naivePrediction - target);
            if (independent)
            {
                inferredIndependentSquaredError += Square(inferredPrediction - target);
                naiveIndependentSquaredError += Square(naivePrediction - target);
                independentCount++;
            }

            P06PairStats(reports, inferredGroups, out var contextTrueEchoPairs, out var contextRecoveredEchoPairs, out var contextIndependentPairs, out var contextFalseMergedPairs);
            trueEchoPairs += contextTrueEchoPairs;
            recoveredEchoPairs += contextRecoveredEchoPairs;
            independentPairs += contextIndependentPairs;
            falseMergedPairs += contextFalseMergedPairs;
        }

        var inferredRmse = Math.Sqrt(inferredSquaredError / contextCount);
        var naiveRmse = Math.Sqrt(naiveSquaredError / contextCount);
        var inferredIndependentRmse = Math.Sqrt(inferredIndependentSquaredError / Math.Max(1, independentCount));
        var naiveIndependentRmse = Math.Sqrt(naiveIndependentSquaredError / Math.Max(1, independentCount));
        var wholeHistoryMargin = (naiveRmse * 0.88) - inferredRmse;
        var independentMargin = ((naiveIndependentRmse * 1.15) + 1e-9) - inferredIndependentRmse;
        return Metrics(
            ("boundary_margin", Math.Min(wholeHistoryMargin, independentMargin)),
            ("whole_history_margin", wholeHistoryMargin),
            ("independent_safety_margin", independentMargin),
            ("inferred_rmse", inferredRmse),
            ("naive_rmse", naiveRmse),
            ("inferred_independent_rmse", inferredIndependentRmse),
            ("naive_independent_rmse", naiveIndependentRmse),
            ("echo_pair_recall", trueEchoPairs == 0 ? 1.0 : (double)recoveredEchoPairs / trueEchoPairs),
            ("false_merge_rate", independentPairs == 0 ? 0.0 : (double)falseMergedPairs / independentPairs));
    }

    public static Dictionary<string, double> EvaluateProtocol07Prevalence(double recommenderCredibility, double mismatchPrevalence, ulong replicateSeed) =>
        EvaluateProtocol07(recommenderCredibility, mismatchPrevalence, 0.90, replicateSeed, severitySurface: false);

    public static Dictionary<string, double> EvaluateProtocol07Severity(double recommenderCredibility, double mismatchSeverity, ulong replicateSeed) =>
        EvaluateProtocol07(recommenderCredibility, 0.50, mismatchSeverity, replicateSeed, severitySurface: true);

    private static Dictionary<string, double> EvaluateProtocol07(
        double recommenderCredibility,
        double mismatchPrevalence,
        double mismatchSeverity,
        ulong replicateSeed,
        bool severitySurface)
    {
        var random = new DeterministicRandom(replicateSeed ^ (severitySurface ? 0x0707070707070708UL : 0x0707070707070707UL));
        const int contextCount = 12;
        var mismatchCount = Math.Clamp((int)Math.Round(contextCount * mismatchPrevalence, MidpointRounding.AwayFromZero), 0, contextCount);
        var mismatches = Enumerable.Range(0, contextCount).Select(index => index < mismatchCount).ToList();
        random.Shuffle(mismatches);
        var recommendations = new P07Recommendation[contextCount];
        for (var context = 0; context < contextCount; context++)
        {
            var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
            var target = sign * (0.40 + (0.35 * random.NextUnit()));
            var sourceEstimate = mismatches[context]
                ? Math.Clamp(target - (sign * mismatchSeverity), -1.0, 1.0)
                : Math.Clamp(target + Symmetric(random, 0.035), -1.0, 1.0);
            recommendations[context] = new P07Recommendation(target, sourceEstimate, 0.88, 45, mismatches[context]);
        }

        var provisional = new P07Receiver(recommendations, recommenderCredibility, P07Mode.Provisional);
        var noTransfer = new P07Receiver(recommendations, recommenderCredibility, P07Mode.NoTransfer);
        var inherited = new P07Receiver(recommendations, recommenderCredibility, P07Mode.Inherited);
        var schedule = new List<int>(contextCount * 30);
        for (var repetition = 0; repetition < 30; repetition++)
        {
            for (var context = 0; context < contextCount; context++)
            {
                schedule.Add(context);
            }
        }

        random.Shuffle(schedule);
        var provisionalSquaredError = 0.0;
        var noTransferSquaredError = 0.0;
        var inheritedSquaredError = 0.0;
        for (var tick = 0; tick < schedule.Count; tick++)
        {
            var context = schedule[tick];
            var recommendation = recommendations[context];
            var observedTarget = Math.Clamp(recommendation.Target + Symmetric(random, 0.025), -1.0, 1.0);
            var provisionalPrediction = provisional.Predict(context);
            var noTransferPrediction = noTransfer.Predict(context);
            var inheritedPrediction = inherited.Predict(context);
            provisionalSquaredError += Square(provisionalPrediction - observedTarget);
            noTransferSquaredError += Square(noTransferPrediction - observedTarget);
            inheritedSquaredError += Square(inheritedPrediction - observedTarget);
            provisional.ObserveDirect(context, observedTarget);
            noTransfer.ObserveDirect(context, observedTarget);
            inherited.ObserveDirect(context, observedTarget);
        }

        var provisionalRmse = Math.Sqrt(provisionalSquaredError / schedule.Count);
        var noTransferRmse = Math.Sqrt(noTransferSquaredError / schedule.Count);
        var inheritedRmse = Math.Sqrt(inheritedSquaredError / schedule.Count);
        var opportunityMargin = (noTransferRmse * 1.05) - provisionalRmse;
        var inheritedMargin = (inheritedRmse * 0.93) - provisionalRmse;
        var finalMismatchStanding = provisional.MeanFinalMismatchStanding();
        var standingMargin = 0.20 - finalMismatchStanding;
        var boundaryMargin = severitySurface
            ? Math.Min(opportunityMargin, standingMargin)
            : Math.Min(opportunityMargin, inheritedMargin);
        return Metrics(
            ("boundary_margin", boundaryMargin),
            ("opportunity_cost_margin", opportunityMargin),
            ("inherited_authority_margin", inheritedMargin),
            ("residual_standing_margin", standingMargin),
            ("provisional_rmse", provisionalRmse),
            ("no_transfer_rmse", noTransferRmse),
            ("inherited_authority_rmse", inheritedRmse),
            ("final_mismatch_standing", finalMismatchStanding),
            ("mismatch_contexts", mismatchCount));
    }

    private static double DevelopmentalStanding(int evidenceCount, double standardDeviation, double early, double middle, double late)
    {
        var segmentMinimum = Math.Min(early, Math.Min(middle, late));
        var segmentMaximum = Math.Max(early, Math.Max(middle, late));
        var segmentSpread = segmentMaximum - segmentMinimum;
        var evidenceFactor = 1.0 - Math.Exp(-evidenceCount / 12.0);
        var consistency = Math.Exp(-4.0 * (standardDeviation + (0.8 * segmentSpread)));
        return Math.Min(0.72, 0.08 + (0.68 * evidenceFactor * consistency));
    }

    private static double P04Weight(P04Posture posture) =>
        Math.Max(0.01, posture.Standing * (1.0 - (0.65 * posture.Uncertainty)));

    private static double P04WeightedMean(P04Posture[] postures)
    {
        var numerator = 0.0;
        var denominator = 0.0;
        for (var index = 0; index < postures.Length; index++)
        {
            var weight = P04Weight(postures[index]);
            numerator += postures[index].Estimate * weight;
            denominator += weight;
        }

        return denominator <= 1e-12 ? 0.0 : numerator / denominator;
    }

    private static double P04WeightedMedian(P04Posture[] postures)
    {
        var ordered = postures.OrderBy(posture => posture.Estimate).ToArray();
        var totalWeight = ordered.Sum(P04Weight);
        var cumulative = 0.0;
        for (var index = 0; index < ordered.Length; index++)
        {
            cumulative += P04Weight(ordered[index]);
            if (cumulative >= totalWeight * 0.50)
            {
                return ordered[index].Estimate;
            }
        }

        return ordered[^1].Estimate;
    }

    private static P05ProbePeer[] CreateP05Peers() => [new(), new(), new()];

    private static double[][] P05Costs(DeterministicRandom random, int preferredAction, int priorPreferredAction, double changeMagnitude)
    {
        var costs = new double[3][];
        for (var peer = 0; peer < costs.Length; peer++)
        {
            costs[peer] = new double[3];
            for (var action = 0; action < 3; action++)
            {
                var individualBias = Symmetric(random, 0.025);
                if (action == preferredAction)
                {
                    costs[peer][action] = Math.Clamp(0.08 + individualBias, 0.01, 0.95);
                }
                else if (action == priorPreferredAction && priorPreferredAction != preferredAction)
                {
                    costs[peer][action] = Math.Clamp(0.08 + changeMagnitude + individualBias, 0.01, 0.98);
                }
                else
                {
                    costs[peer][action] = Math.Clamp(0.46 + (0.20 * changeMagnitude) + individualBias, 0.01, 0.98);
                }
            }
        }

        return costs;
    }

    private static P05Posture[] P05Postures(double[][] costs)
    {
        var postures = new P05Posture[costs.Length];
        for (var peer = 0; peer < costs.Length; peer++)
        {
            var ordered = Enumerable.Range(0, costs[peer].Length).OrderBy(action => costs[peer][action]).ToArray();
            postures[peer] = new P05Posture(ordered[0], Math.Max(0.0, costs[peer][ordered[1]] - costs[peer][ordered[0]]));
        }

        return postures;
    }

    private static int P05SelectNegotiatedAction(P05Posture[] postures, int speakerIndex)
    {
        var scores = new double[3];
        for (var peerIndex = 0; peerIndex < postures.Length; peerIndex++)
        {
            var posture = postures[peerIndex];
            scores[posture.PreferredAction] += 1.0 + (4.0 * posture.PreferenceStrength);
        }

        var bestScore = scores.Max();
        var speakerPreference = postures[speakerIndex].PreferredAction;
        if (Math.Abs(scores[speakerPreference] - bestScore) <= 1e-12)
        {
            return speakerPreference;
        }

        for (var action = 0; action < scores.Length; action++)
        {
            if (Math.Abs(scores[action] - bestScore) <= 1e-12)
            {
                return action;
            }
        }

        return 0;
    }

    private static double P05GroupUtility(double[][] costs, int[] actions, bool coordinated)
    {
        if (!coordinated)
        {
            return 0.0;
        }

        var totalCost = 0.0;
        for (var peer = 0; peer < costs.Length; peer++)
        {
            totalCost += costs[peer][actions[0]];
        }

        return Math.Clamp(1.0 - (totalCost / costs.Length), 0.0, 1.0);
    }

    private static AncestryReport[] P06EchoReports(
        DeterministicRandom random,
        int context,
        double target,
        double missingness,
        double separation)
    {
        var rootIndexes = new[] { 0, 0, 0, 0, 1, 1, 2 };
        var centers = P06Centers(separation, 3);
        var reports = new AncestryReport[rootIndexes.Length];
        for (var peer = 0; peer < reports.Length; peer++)
        {
            var root = rootIndexes[peer];
            var rootEstimate = root == 0 ? -0.65 * target : target;
            var standing = root == 0 ? 0.84 : 0.78 + (0.05 * root);
            var evidence = root == 0 ? 52 : 38 + (root * 7);
            reports[peer] = P06Report(random, context, peer, root, rootEstimate, standing, evidence, centers[root], missingness);
        }

        return reports;
    }

    private static AncestryReport[] P06IndependentReports(
        DeterministicRandom random,
        int context,
        double target,
        double missingness,
        double separation)
    {
        var centers = P06Centers(separation, 7);
        var reports = new AncestryReport[7];
        for (var peer = 0; peer < reports.Length; peer++)
        {
            var estimateOffset = Symmetric(random, 0.24);
            var standing = 0.55 + (0.38 * random.NextUnit());
            var evidence = 12 + random.NextInt(48);
            reports[peer] = P06Report(random, context, peer, peer, Math.Clamp(target + estimateOffset, -1.0, 1.0), standing, evidence, centers[peer], missingness);
        }

        return reports;
    }

    private static AncestrySignature[] P06Centers(double separation, int count)
    {
        var directions = new (double A, double B, double C)[]
        {
            (-1, -1, -1), (1, -1, -1), (-1, 1, -1), (1, 1, -1),
            (-1, -1, 1), (1, -1, 1), (-1, 1, 1), (1, 1, 1),
        };
        var centers = new AncestrySignature[count];
        for (var index = 0; index < count; index++)
        {
            var direction = directions[index % directions.Length];
            var scale = separation * 0.50;
            centers[index] = new AncestrySignature(
                Math.Clamp(0.50 + (direction.A * scale), 0.02, 0.98),
                Math.Clamp(0.50 + (direction.B * scale), 0.02, 0.98),
                Math.Clamp(0.50 + (direction.C * scale), 0.02, 0.98));
        }

        return centers;
    }

    private static AncestryReport P06Report(
        DeterministicRandom random,
        int context,
        int peer,
        int root,
        double estimate,
        double standing,
        int evidence,
        AncestrySignature center,
        double missingness)
    {
        var rootId = $"c{context}:root-{root}";
        string? originHint;
        AncestryHintKind hintKind;
        if (random.NextUnit() < missingness)
        {
            originHint = null;
            hintKind = AncestryHintKind.Missing;
        }
        else if (random.NextUnit() < 0.30)
        {
            originHint = $"relay-{peer}";
            hintKind = AncestryHintKind.ImmediateSender;
        }
        else
        {
            originHint = rootId;
            hintKind = AncestryHintKind.RootPreserved;
        }

        return new AncestryReport(
            $"peer-{peer + 1}",
            rootId,
            originHint,
            hintKind,
            Math.Clamp(estimate + Symmetric(random, 0.025), -1.0, 1.0),
            Math.Clamp(standing * (0.96 + (0.04 * random.NextUnit())), 0.0, 1.0),
            evidence,
            new AncestrySignature(
                Math.Clamp(center.A + Symmetric(random, 0.025), 0.0, 1.0),
                Math.Clamp(center.B + Symmetric(random, 0.025), 0.0, 1.0),
                Math.Clamp(center.C + Symmetric(random, 0.025), 0.0, 1.0)));
    }

    private static List<List<AncestryReport>> P06CreateInferredGroups(AncestryReport[] reports)
    {
        var parents = Enumerable.Range(0, reports.Length).ToArray();
        for (var left = 0; left < reports.Length; left++)
        {
            for (var right = left + 1; right < reports.Length; right++)
            {
                if ((reports[left].OriginHint is not null && string.Equals(reports[left].OriginHint, reports[right].OriginHint, StringComparison.Ordinal)) ||
                    P06SignatureDistance(reports[left].Signature, reports[right].Signature) <= P06SignatureMergeDistance)
                {
                    P06Union(parents, left, right);
                }
            }
        }

        var grouped = new Dictionary<int, List<AncestryReport>>();
        for (var index = 0; index < reports.Length; index++)
        {
            var root = P06Find(parents, index);
            if (!grouped.TryGetValue(root, out var group))
            {
                group = [];
                grouped.Add(root, group);
            }

            group.Add(reports[index]);
        }

        return grouped.Values.ToList();
    }

    private static double P06Predict(List<List<AncestryReport>> groups)
    {
        var numerator = 0.0;
        var denominator = 0.0;
        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var groupNumerator = 0.0;
            var groupDenominator = 0.0;
            var groupSupport = 0.0;
            for (var reportIndex = 0; reportIndex < groups[groupIndex].Count; reportIndex++)
            {
                var report = groups[groupIndex][reportIndex];
                var support = P06ReportSupport(report);
                groupNumerator += report.Estimate * support;
                groupDenominator += support;
                groupSupport = Math.Max(groupSupport, support);
            }

            if (groupDenominator <= 1e-12 || groupSupport <= 1e-12)
            {
                continue;
            }

            numerator += (groupNumerator / groupDenominator) * groupSupport;
            denominator += groupSupport;
        }

        return denominator <= 1e-12 ? 0.0 : numerator / denominator;
    }

    private static double P06ReportSupport(AncestryReport report)
    {
        var evidenceConfidence = 1.0 - Math.Exp(-report.EvidenceCount / 24.0);
        return report.Standing * (0.55 + (0.45 * evidenceConfidence));
    }

    private static double P06SignatureDistance(AncestrySignature left, AncestrySignature right)
    {
        var a = left.A - right.A;
        var b = left.B - right.B;
        var c = left.C - right.C;
        return Math.Sqrt((a * a) + (b * b) + (c * c));
    }

    private static int P06Find(int[] parents, int index)
    {
        var current = index;
        while (parents[current] != current)
        {
            parents[current] = parents[parents[current]];
            current = parents[current];
        }

        return current;
    }

    private static void P06Union(int[] parents, int left, int right)
    {
        var leftRoot = P06Find(parents, left);
        var rightRoot = P06Find(parents, right);
        if (leftRoot != rightRoot)
        {
            parents[rightRoot] = leftRoot;
        }
    }

    private static void P06PairStats(
        AncestryReport[] reports,
        List<List<AncestryReport>> groups,
        out int trueEchoPairs,
        out int recoveredEchoPairs,
        out int independentPairs,
        out int falseMergedPairs)
    {
        var groupBySender = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            for (var reportIndex = 0; reportIndex < groups[groupIndex].Count; reportIndex++)
            {
                groupBySender[groups[groupIndex][reportIndex].SenderMindId] = groupIndex;
            }
        }

        trueEchoPairs = 0;
        recoveredEchoPairs = 0;
        independentPairs = 0;
        falseMergedPairs = 0;
        for (var left = 0; left < reports.Length; left++)
        {
            for (var right = left + 1; right < reports.Length; right++)
            {
                var sameRoot = string.Equals(reports[left].TrueRootId, reports[right].TrueRootId, StringComparison.Ordinal);
                var sameGroup = groupBySender[reports[left].SenderMindId] == groupBySender[reports[right].SenderMindId];
                if (sameRoot)
                {
                    trueEchoPairs++;
                    if (sameGroup)
                    {
                        recoveredEchoPairs++;
                    }
                }
                else
                {
                    independentPairs++;
                    if (sameGroup)
                    {
                        falseMergedPairs++;
                    }
                }
            }
        }
    }

    private static Dictionary<string, double> Metrics(params (string Name, double Value)[] values)
    {
        var metrics = new Dictionary<string, double>(StringComparer.Ordinal);
        for (var index = 0; index < values.Length; index++)
        {
            metrics[values[index].Name] = values[index].Value;
        }

        return metrics;
    }

    private static double Symmetric(DeterministicRandom random, double amplitude) =>
        ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private static double Square(double value) => value * value;

    private sealed class P03Receiver
    {
        private readonly double _foreignEstimate;
        private double _localEstimate;
        private int _localEvidence;

        public P03Receiver(double foreignEstimate, double foreignStanding)
        {
            _foreignEstimate = foreignEstimate;
            ForeignStanding = foreignStanding;
        }

        public double ForeignStanding { get; private set; }

        public double Predict()
        {
            var localStanding = _localEvidence == 0
                ? 0.0
                : Math.Min(0.95, 0.20 + (0.18 * Math.Log(1.0 + _localEvidence)));
            var denominator = P03PriorStanding + localStanding + ForeignStanding;
            return ((_localEstimate * localStanding) + (_foreignEstimate * ForeignStanding)) / denominator;
        }

        public void ObserveDirect(double target)
        {
            if (_localEvidence == 0)
            {
                _localEstimate = target;
            }
            else
            {
                var alpha = Math.Max(0.08, 0.34 / (1.0 + (0.06 * _localEvidence)));
                _localEstimate += alpha * (target - _localEstimate);
            }

            _localEvidence++;
            var foreignError = Math.Abs(_foreignEstimate - target);
            if (foreignError <= 0.10)
            {
                ForeignStanding += (0.90 - ForeignStanding) * 0.06;
            }
            else if (foreignError >= 0.25)
            {
                ForeignStanding *= 0.72;
            }
            else
            {
                ForeignStanding *= 0.93;
            }
        }
    }

    private sealed record P04Posture(double Estimate, double Standing, double Uncertainty);

    private sealed record P05Posture(int PreferredAction, double PreferenceStrength);

    private sealed class P05ProbePeer
    {
        public int ConventionAction { get; private set; } = -1;

        public double Standing { get; private set; }

        public void ObserveOutcome(int performedAction, double utility, int preferredAction)
        {
            if (utility >= P05ConventionSuccessUtility && performedAction >= 0)
            {
                if (ConventionAction != performedAction)
                {
                    ConventionAction = performedAction;
                    Standing = 0.30;
                }
                else
                {
                    Standing = Math.Min(0.95, Standing + (0.34 * (1.0 - Standing)));
                }

                return;
            }

            Standing *= 0.38;
            if (Standing < 0.25 && preferredAction != ConventionAction)
            {
                ConventionAction = preferredAction;
                Standing = 0.15;
            }
        }
    }

    private sealed record P07Recommendation(
        double Target,
        double SourceEstimate,
        double RecommenderStanding,
        int RecommenderEvidenceCount,
        bool Mismatch);

    private enum P07Mode
    {
        Provisional,
        NoTransfer,
        Inherited,
    }

    private sealed class P07Receiver
    {
        private readonly P07Recommendation[] _recommendations;
        private readonly double[] _localEstimate;
        private readonly int[] _localEvidence;
        private readonly double[] _sourceStanding;

        public P07Receiver(P07Recommendation[] recommendations, double recommenderCredibility, P07Mode mode)
        {
            _recommendations = recommendations;
            _localEstimate = new double[recommendations.Length];
            _localEvidence = new int[recommendations.Length];
            _sourceStanding = new double[recommendations.Length];
            for (var index = 0; index < recommendations.Length; index++)
            {
                _sourceStanding[index] = mode switch
                {
                    P07Mode.NoTransfer => P07ExplorationStanding,
                    P07Mode.Inherited => recommendations[index].RecommenderStanding,
                    _ => P07InitialStanding(recommendations[index], recommenderCredibility),
                };
            }
        }

        public double Predict(int context)
        {
            var localConfidence = 1.0 - Math.Exp(-_localEvidence[context] / 6.0);
            var localWeight = 0.30 + (1.70 * localConfidence);
            var sourceWeight = 1.15 * _sourceStanding[context];
            return ((_localEstimate[context] * localWeight) + (_recommendations[context].SourceEstimate * sourceWeight)) /
                (localWeight + sourceWeight);
        }

        public void ObserveDirect(int context, double target)
        {
            _localEstimate[context] += 0.24 * (target - _localEstimate[context]);
            _localEvidence[context]++;
            var sourceError = Math.Abs(_recommendations[context].SourceEstimate - target);
            var earnedSupport = Math.Clamp(1.0 - (sourceError / 0.90), 0.0, 1.0);
            _sourceStanding[context] += 0.18 * (earnedSupport - _sourceStanding[context]);
        }

        public double MeanFinalMismatchStanding()
        {
            var total = 0.0;
            var count = 0;
            for (var index = 0; index < _recommendations.Length; index++)
            {
                if (!_recommendations[index].Mismatch)
                {
                    continue;
                }

                total += _sourceStanding[index];
                count++;
            }

            return count == 0 ? 0.0 : total / count;
        }

        private static double P07InitialStanding(P07Recommendation recommendation, double recommenderCredibility)
        {
            var evidenceConfidence = 1.0 - Math.Exp(-recommendation.RecommenderEvidenceCount / 18.0);
            return Math.Min(
                P07ProvisionalStandingCap,
                P07ExplorationStanding + (0.20 * recommendation.RecommenderStanding * recommenderCredibility * evidenceConfidence));
        }
    }
}
