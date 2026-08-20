using Cpa.BoundedMindsLab.Environments;
using Cpa.BoundedMindsLab.Experiments;

namespace Cpa.BoundedMindsLab.Challenge;

public sealed record ChallengeDescriptor(
    ulong Seed,
    double StressScore,
    IReadOnlyDictionary<string, double> Values);

public sealed record ChallengeSelection(
    string ProfileId,
    string ProfileName,
    string Experiment,
    string Band,
    int BandIndex,
    ulong Seed,
    double StressScore,
    IReadOnlyDictionary<string, double> Descriptors);

public sealed record ChallengeProfileDefinition(
    string Id,
    string Name,
    string Experiment,
    string Question,
    string StressDirection,
    string Limitation,
    Func<ulong, ChallengeDescriptor> Describe,
    Func<IReadOnlyDictionary<string, double>, double> BoundaryMargin,
    string BoundaryMarginDescription);

public static class ChallengePlan
{
    public const string Name = "challenge-v1";
    public const ulong CandidateSeedStart = 10001UL;
    public const ulong CandidateSeedEnd = 29999UL;
    public const int BandCount = 5;
    public const int SeedsPerBand = 4;

    private static readonly string[] BandNames =
    [
        "q1-low",
        "q2-moderate-low",
        "q3-middle",
        "q4-high",
        "q5-extreme",
    ];

    private static readonly Lazy<ChallengeSelection[]> CachedSelections = new(CreateSelections);

    public static IReadOnlyList<ChallengeProfileDefinition> Profiles { get; } =
    [
        new(
            "p03-source-instability",
            "Protocol 03 source instability",
            "03-developmental-versus-doctrinal-transfer",
            "As source histories become less stable and less well evidenced, where does developmental transfer cease to outperform final-rule doctrine?",
            "Higher scores mean more unstable/sparse source histories, greater transition magnitude, and thinner minimum source evidence.",
            "This sweep stays inside the frozen Protocol 03 world generator. It does not synthesize source histories outside that generator's original support.",
            DescribeProtocol03,
            metrics => Value(metrics, "doctrinal_rmse") - Value(metrics, "developmental_rmse"),
            "Positive means developmental transfer has lower whole-history RMSE than doctrine; zero is the crossover."),
        new(
            "p04-conflict-density",
            "Protocol 04 conflict density",
            "04-bounded-communication-before-language",
            "As public disagreement becomes denser and private warrants become more uneven, does preserving typed epistemic shape continue to outperform early semantic smoothing?",
            "Higher scores mean more dissent contexts, wider evidence imbalance, and greater private-target spread.",
            "This is an environmental stress sweep of the frozen Protocol 04 assay. It does not yet replace semantic smoothing with a stronger equal-budget negotiation control; that comparator remains an explicit unresolved test.",
            DescribeProtocol04,
            metrics => (Value(metrics, "semantic_smoothed_rmse") * 0.97) - Value(metrics, "typed_rmse"),
            "Positive means typed communication still satisfies the frozen 3% whole-history advantage requirement over semantic smoothing; zero is the registered boundary."),
        new(
            "p05-regime-shift",
            "Protocol 05 regime shift",
            "05-emergent-convention-artificial-culture",
            "As more of the coordination world changes and the cost landscape moves farther, when does a useful convention become inertia or churn?",
            "Higher scores mean more shifted contexts, larger post-formation cost movement, and more initial private preference diversity.",
            "The sweep searches the frozen convention generator. Its maximum shift remains bounded by that generator rather than introducing arbitrary new regime changes.",
            DescribeProtocol05,
            metrics => Math.Min(
                Value(metrics, "earned_stable_retention_coverage") - 0.90,
                Math.Min(
                    Value(metrics, "earned_changed_revision_coverage") - 0.85,
                    Value(metrics, "earned_changed_shifted_late_utility") - Value(metrics, "frozen_changed_shifted_late_utility") - 0.20)),
            "Positive means stable retention stays above 0.90, changed-context revision stays above 0.85, and adaptive late utility retains the frozen 0.20 advantage over frozen culture; zero marks the first violated boundary."),
        new(
            "p06-ancestry-visibility",
            "Protocol 06 ancestry visibility",
            "06-incomplete-epistemic-ancestry",
            "As origin cues weaken and developmental signatures become harder to separate, when does ancestry-sensitive corroboration become more harmful than naive agreement counting?",
            "Higher scores mean more missing/immediate-sender hints, more ambiguous-lineage contexts, and lower root-signature separation.",
            "The sweep cannot erase provenance beyond what the frozen generator can produce. It is an adversarial search within the registered ancestry world family, not a parameterized missingness intervention.",
            DescribeProtocol06,
            metrics => Math.Min(
                (Value(metrics, "naive_rmse") * 0.88) - Value(metrics, "inferred_rmse"),
                (Value(metrics, "naive_independent_rmse") * 1.15) - Value(metrics, "inferred_independent_rmse")),
            "Positive means ancestry inference still satisfies the frozen 12% whole-history advantage and 15% independent-convergence safety allowance; zero marks the first violated boundary."),
        new(
            "p07-recommender-fragility",
            "Protocol 07 recommender fragility",
            "07-provisional-standing-transfer",
            "As recommender credibility weakens and locally mismatched recommendations become more severe, when does provisional standing cost too much relative to learning alone?",
            "Higher scores mean weaker C-to-A credibility, more strong local mismatch, and larger source/receiver disagreement on those mismatches.",
            "The sweep remains inside the frozen standing-transfer generator, whose recommender credibility floor is 0.68. Lower-credibility interventions require a later parameterized challenge if this search does not expose a clear boundary.",
            DescribeProtocol07,
            metrics => Math.Min(
                (Value(metrics, "no_transfer_rmse") * 1.05) - Value(metrics, "provisional_rmse"),
                0.20 - Value(metrics, "provisional_final_strong_mismatch_standing")),
            "Positive means provisional transfer stays within the frozen 5% opportunity-cost allowance and revokes strong mismatch standing below 0.20; zero marks a safety boundary."),
    ];

    public static IReadOnlyList<ChallengeSelection> BuildSelections() => CachedSelections.Value;

    private static ChallengeSelection[] CreateSelections()
    {
        var excluded = ExperimentDefaults.DevelopmentSeeds
            .Concat(ExperimentDefaults.HoldoutSeeds)
            .ToHashSet();
        var selections = new List<ChallengeSelection>(Profiles.Count * BandCount * SeedsPerBand);
        foreach (var profile in Profiles)
        {
            var candidates = new List<ChallengeDescriptor>((int)(CandidateSeedEnd - CandidateSeedStart + 1UL));
            for (var seed = CandidateSeedStart; seed <= CandidateSeedEnd; seed++)
            {
                if (!excluded.Contains(seed))
                {
                    candidates.Add(profile.Describe(seed));
                }
            }

            candidates.Sort(static (left, right) =>
            {
                var score = left.StressScore.CompareTo(right.StressScore);
                return score != 0 ? score : left.Seed.CompareTo(right.Seed);
            });

            for (var bandIndex = 0; bandIndex < BandCount; bandIndex++)
            {
                var start = (bandIndex * candidates.Count) / BandCount;
                var endExclusive = ((bandIndex + 1) * candidates.Count) / BandCount;
                var count = endExclusive - start;
                if (count < SeedsPerBand)
                {
                    throw new InvalidOperationException($"Challenge band {bandIndex + 1} for {profile.Id} contains only {count} candidates.");
                }

                for (var sampleIndex = 0; sampleIndex < SeedsPerBand; sampleIndex++)
                {
                    var offset = SeedsPerBand == 1
                        ? count / 2
                        : (int)Math.Round(sampleIndex * (count - 1.0) / (SeedsPerBand - 1.0), MidpointRounding.AwayFromZero);
                    var candidate = candidates[start + offset];
                    selections.Add(new ChallengeSelection(
                        profile.Id,
                        profile.Name,
                        profile.Experiment,
                        BandNames[bandIndex],
                        bandIndex + 1,
                        candidate.Seed,
                        candidate.StressScore,
                        candidate.Values));
                }
            }
        }

        return [.. selections];
    }

    public static ChallengeProfileDefinition GetProfile(string id)
    {
        var profile = Profiles.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));
        return profile ?? throw new ArgumentException($"Unknown challenge profile '{id}'.", nameof(id));
    }

    private static ChallengeDescriptor DescribeProtocol03(ulong seed)
    {
        var scenario = DevelopmentalTransferWorld.CreateScenario(seed);
        var unstable = scenario.Cells.Count(cell => cell.HistoryKind == SourceHistoryKind.UnstableTransition);
        var sparse = scenario.Cells.Count(cell => cell.HistoryKind == SourceHistoryKind.SparseAmbiguous);
        var minimumEvidence = scenario.Cells.Min(cell => cell.SourceEvidenceCount);
        var maximumEvidence = scenario.Cells.Max(cell => cell.SourceEvidenceCount);
        var transitionMagnitude = scenario.Cells
            .Where(cell => cell.HistoryKind == SourceHistoryKind.UnstableTransition)
            .Select(cell => Math.Abs(cell.SourceEarlyTarget - cell.SourceLateTarget))
            .DefaultIfEmpty(0.0)
            .Average();
        var thinEvidence = 1.0 - Math.Clamp(minimumEvidence / 56.0, 0.0, 1.0);
        var score = unstable + (1.6 * sparse) + (3.0 * transitionMagnitude) + thinEvidence;
        return Descriptor(
            seed,
            score,
            ("unstable_transition_cells", unstable),
            ("sparse_ambiguous_cells", sparse),
            ("minimum_source_evidence", minimumEvidence),
            ("source_evidence_span", maximumEvidence - minimumEvidence),
            ("mean_transition_magnitude", transitionMagnitude));
    }

    private static ChallengeDescriptor DescribeProtocol04(ulong seed)
    {
        var scenario = CommunicationBeforeLanguageWorld.CreateScenario(seed);
        var informative = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.InformativeDissent);
        var misleading = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.MisleadingDissent);
        var evidence = scenario.Cells.SelectMany(cell => cell.PeerHistories).Select(history => history.EvidenceCount).ToArray();
        var evidenceSpan = evidence.Max() - evidence.Min();
        var privateSpread = scenario.Cells
            .Select(cell => cell.PeerHistories.Max(history => history.PrivateTarget) - cell.PeerHistories.Min(history => history.PrivateTarget))
            .Average();
        var score = informative + misleading + (0.045 * evidenceSpan) + (2.0 * privateSpread);
        return Descriptor(
            seed,
            score,
            ("informative_dissent_cells", informative),
            ("misleading_dissent_cells", misleading),
            ("private_evidence_span", evidenceSpan),
            ("mean_private_target_spread", privateSpread));
    }

    private static ChallengeDescriptor DescribeProtocol05(ulong seed)
    {
        var scenario = EmergentConventionWorld.CreateScenario(seed);
        var shifted = EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Shifted);
        var diverse = EmergentConventionWorld.CountPreferenceDiverseContexts(scenario);
        var shiftedCells = scenario.Cells.Where(cell => cell.ContextKind == ConventionContextKind.Shifted).ToArray();
        var meanShiftMagnitude = shiftedCells
            .Select(cell => MeanAbsoluteCostMovement(cell.InitialPeerCosts, cell.ShiftedPeerCosts))
            .DefaultIfEmpty(0.0)
            .Average();
        var score = shifted + (8.0 * meanShiftMagnitude) + (0.08 * diverse);
        return Descriptor(
            seed,
            score,
            ("shifted_contexts", shifted),
            ("stable_contexts", EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Stable)),
            ("preference_diverse_contexts", diverse),
            ("mean_shift_magnitude", meanShiftMagnitude),
            ("mean_initial_viable_gap", EmergentConventionWorld.MeanInitialViableGap(scenario)));
    }

    private static ChallengeDescriptor DescribeProtocol06(ulong seed)
    {
        var scenario = EpistemicAncestryWorld.CreateScenario(seed);
        var missing = EpistemicAncestryWorld.MissingOriginRate(scenario);
        var sender = EpistemicAncestryWorld.ImmediateSenderHintRate(scenario);
        var ambiguous = EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.AmbiguousLineage);
        var meanSeparation = scenario.Cells.Select(MeanRootSignatureSeparation).DefaultIfEmpty(1.0).Average();
        var ambiguityPressure = 1.0 - Math.Clamp(meanSeparation / 0.75, 0.0, 1.0);
        var score = (2.0 * missing) + (1.2 * sender) + (0.30 * ambiguous) + (2.0 * ambiguityPressure);
        return Descriptor(
            seed,
            score,
            ("missing_origin_rate", missing),
            ("immediate_sender_hint_rate", sender),
            ("ambiguous_lineage_contexts", ambiguous),
            ("mean_root_signature_separation", meanSeparation));
    }

    private static ChallengeDescriptor DescribeProtocol07(ulong seed)
    {
        var scenario = StandingTransferWorld.CreateScenario(seed);
        var strongMismatchCells = scenario.Cells
            .Where(cell => cell.ContextKind == StandingTransferContextKind.StrongLocalMismatch)
            .ToArray();
        var strongMismatch = strongMismatchCells.Length;
        var mismatchMagnitude = strongMismatchCells
            .Select(cell => Math.Abs(cell.SourceEstimate - cell.ReceiverTarget))
            .DefaultIfEmpty(0.0)
            .Average();
        var meanMismatchStanding = strongMismatchCells
            .Select(cell => cell.RecommenderStanding)
            .DefaultIfEmpty(0.0)
            .Average();
        var score = (4.0 * (1.0 - scenario.RecommenderCredibility)) + (0.65 * strongMismatch) + (1.5 * mismatchMagnitude) + meanMismatchStanding;
        return Descriptor(
            seed,
            score,
            ("recommender_credibility", scenario.RecommenderCredibility),
            ("strong_local_mismatch_contexts", strongMismatch),
            ("mean_strong_mismatch_magnitude", mismatchMagnitude),
            ("mean_strong_mismatch_recommender_standing", meanMismatchStanding));
    }

    private static ChallengeDescriptor Descriptor(ulong seed, double score, params (string Name, double Value)[] values) =>
        new(seed, score, values.ToDictionary(item => item.Name, item => item.Value, StringComparer.Ordinal));

    private static double MeanAbsoluteCostMovement(double[][] initial, double[][] shifted)
    {
        var total = 0.0;
        var count = 0;
        for (var peer = 0; peer < initial.Length; peer++)
        {
            for (var action = 0; action < initial[peer].Length; action++)
            {
                total += Math.Abs(initial[peer][action] - shifted[peer][action]);
                count++;
            }
        }

        return count == 0 ? 0.0 : total / count;
    }

    private static double MeanRootSignatureSeparation(EpistemicAncestryCell cell)
    {
        if (cell.Roots.Length < 2)
        {
            return 1.0;
        }

        var total = 0.0;
        var count = 0;
        for (var left = 0; left < cell.Roots.Length; left++)
        {
            for (var right = left + 1; right < cell.Roots.Length; right++)
            {
                var a = cell.Roots[left].Signature;
                var b = cell.Roots[right].Signature;
                var da = a.A - b.A;
                var db = a.B - b.B;
                var dc = a.C - b.C;
                total += Math.Sqrt((da * da) + (db * db) + (dc * dc));
                count++;
            }
        }

        return count == 0 ? 1.0 : total / count;
    }

    private static double Value(IReadOnlyDictionary<string, double> metrics, string name) =>
        metrics.TryGetValue(name, out var value) ? value : double.NaN;
}
