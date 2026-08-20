using Cpa.BoundedMindsLab.Experiments;

namespace Cpa.BoundedMindsLab.Validation;

public sealed record ValidationCategorySummary(
    string Category,
    int Checks,
    int Passed,
    int Failed);

public sealed record ValidationProtocolSummary(
    string Experiment,
    int Runs,
    int Support,
    int Mixed,
    int Disconfirm,
    int Inconclusive,
    int Assertions,
    int PassedAssertions,
    int FailedAssertions,
    IReadOnlyList<ValidationCategorySummary> Categories,
    IReadOnlyList<string> Diagnostics);

public sealed record ValidationChallengeSummary(
    string Id,
    string Name,
    string Experiment,
    string Description,
    string SelectionRule,
    int MatchingRuns,
    int Support,
    int Mixed,
    int Disconfirm,
    int Inconclusive);

public sealed record ValidationReport(
    string Schema,
    string Version,
    string SeedSet,
    IReadOnlyList<ulong> Seeds,
    bool FullFrozenProtocolSet,
    IReadOnlyList<ValidationProtocolSummary> Protocols,
    IReadOnlyList<ValidationCategorySummary> Categories,
    IReadOnlyList<ValidationChallengeSummary> ChallengeProfiles,
    IReadOnlyList<string> Diagnostics);

public static class ValidationReportBuilder
{
    private sealed record ChallengeDefinition(
        string Id,
        string Name,
        string Experiment,
        string Description,
        string SelectionRule,
        Func<IReadOnlyDictionary<string, double>, bool> Matches);

    private static readonly ChallengeDefinition[] Challenges =
    [
        new(
            "p03-high-source-instability",
            "High source instability",
            "03-developmental-versus-doctrinal-transfer",
            "Holdout worlds with unusually many unstable or sparse source histories. This stresses whether developmental context remains useful when the source's own history is difficult to interpret.",
            "unstable_transition_cells >= 4 OR sparse_ambiguous_cells >= 2",
            metrics => Value(metrics, "unstable_transition_cells") >= 4.0 || Value(metrics, "sparse_ambiguous_cells") >= 2.0),
        new(
            "p04-dense-conflicting-evidence",
            "Dense conflicting social evidence",
            "04-bounded-communication-before-language",
            "Holdout worlds with many dissent contexts or a wide private-evidence span. This stresses typed communication when public differences are both numerous and unevenly warranted.",
            "informative_dissent_cells + misleading_dissent_cells >= 7 OR private_evidence_span >= 35",
            metrics => Value(metrics, "informative_dissent_cells") + Value(metrics, "misleading_dissent_cells") >= 7.0 || Value(metrics, "private_evidence_span") >= 35.0),
        new(
            "p05-high-regime-shift",
            "High regime shift",
            "05-emergent-convention-artificial-culture",
            "Holdout worlds at the high end of changed-context pressure. This tests whether a useful culture remains revisable rather than becoming cheap inertia.",
            "shifted_contexts >= 6",
            metrics => Value(metrics, "shifted_contexts") >= 6.0),
        new(
            "p06-weak-ancestry-visibility",
            "Weak ancestry visibility",
            "06-incomplete-epistemic-ancestry",
            "Holdout worlds with especially incomplete or alias-heavy provenance. This stresses ancestry inference near the edge of what public cues can support.",
            "missing_origin_rate >= 0.45 OR immediate_sender_hint_rate >= 0.28",
            metrics => Value(metrics, "missing_origin_rate") >= 0.45 || Value(metrics, "immediate_sender_hint_rate") >= 0.28),
        new(
            "p07-fragile-recommender-transfer",
            "Fragile recommender transfer",
            "07-provisional-standing-transfer",
            "Holdout worlds where C's standing for recommender A is weak or where many strong recommendations fail to generalize locally.",
            "recommender_credibility <= 0.70 OR strong_local_mismatch_contexts >= 4",
            metrics => Value(metrics, "recommender_credibility") <= 0.70 || Value(metrics, "strong_local_mismatch_contexts") >= 4.0),
    ];

    public static ValidationReport Create(IReadOnlyList<RunResult> completedRuns)
    {
        ArgumentNullException.ThrowIfNull(completedRuns);
        if (completedRuns.Count == 0)
        {
            throw new ArgumentException("Provide at least one completed run.", nameof(completedRuns));
        }

        var seeds = completedRuns.Select(run => run.Seed).ToArray();
        var experimentNames = completedRuns.SelectMany(run => run.Experiments).Select(result => result.Name).Distinct(StringComparer.Ordinal).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        var protocolSummaries = experimentNames.Select(name => BuildProtocolSummary(name, completedRuns)).ToArray();
        var allCategories = completedRuns
            .SelectMany(run => run.Experiments)
            .SelectMany(result => result.Assertions)
            .GroupBy(assertion => ValidationCheckTaxonomy.Classify(assertion.Name), StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new ValidationCategorySummary(group.Key, group.Count(), group.Count(assertion => assertion.Passed), group.Count(assertion => !assertion.Passed)))
            .ToArray();
        var challengeSummaries = Challenges
            .Where(challenge => experimentNames.Contains(challenge.Experiment, StringComparer.Ordinal))
            .Select(challenge => BuildChallengeSummary(challenge, completedRuns))
            .ToArray();
        var seedSet = ValidationPlan.ClassifySeedSet(seeds);
        var fullProtocolSet = ValidationPlan.IsFullFrozenProtocolSet(experimentNames);
        var diagnostics = new List<string>();

        if (seedSet == ValidationPlan.DevelopmentSetName)
        {
            diagnostics.Add("This is the canonical development set. It is retained for regression comparison and must not be interpreted as fresh holdout validation.");
        }
        else if (seedSet == ValidationPlan.HoldoutSetName)
        {
            diagnostics.Add("This session uses the frozen holdout-v1 seed set. Do not tune protocol mechanics or preregistered thresholds from these outcomes and then reuse holdout-v1 as validation.");
        }
        else
        {
            diagnostics.Add("Custom seeds are useful for exploration but are not the registered development-v1 or holdout-v1 set.");
        }

        if (!fullProtocolSet)
        {
            diagnostics.Add("This session does not contain the complete frozen Protocol 01-07 catalog, so it is a partial validation run.");
        }

        if (experimentNames.Contains("01-local-shared-memory-contamination", StringComparer.Ordinal)
            || experimentNames.Contains("02-peer-disagreement-preserved-interiors", StringComparer.Ordinal))
        {
            diagnostics.Add("Protocols 01 and 02 predate seed-as-lived-circumstance semantics. Different seeds mainly perturb ordering there, so their holdout evidence is weaker than Protocols 03-07 and should be interpreted separately.");
        }
        if (protocolSummaries.Length > 0 && protocolSummaries.All(summary => summary.Support == summary.Runs))
        {
            diagnostics.Add("Every protocol run in this session returned Support. Treat this as an assay-sensitivity warning, not as extra evidence by itself; inspect challenge slices and category-level failures before drawing implementation conclusions.");
        }

        var totalAssertions = protocolSummaries.Sum(summary => summary.Assertions);
        if (totalAssertions > 0 && protocolSummaries.Sum(summary => summary.PassedAssertions) == totalAssertions)
        {
            diagnostics.Add("Every assertion in this session passed. Manipulation and accounting checks are reported separately so they do not inflate the apparent strength of mechanism outcomes.");
        }

        foreach (var challenge in challengeSummaries.Where(summary => summary.MatchingRuns < 3))
        {
            diagnostics.Add($"Challenge profile '{challenge.Name}' matched only {challenge.MatchingRuns} run(s); its stress coverage is too thin for a strong conclusion.");
        }

        return new ValidationReport(
            "cpa-bounded-minds-validation-v1",
            "0.8.0",
            seedSet,
            seeds,
            fullProtocolSet,
            protocolSummaries,
            allCategories,
            challengeSummaries,
            diagnostics);
    }

    private static ValidationProtocolSummary BuildProtocolSummary(string experiment, IReadOnlyList<RunResult> completedRuns)
    {
        var results = completedRuns.SelectMany(run => run.Experiments).Where(result => string.Equals(result.Name, experiment, StringComparison.Ordinal)).ToArray();
        var categories = results.SelectMany(result => result.Assertions)
            .GroupBy(assertion => ValidationCheckTaxonomy.Classify(assertion.Name), StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new ValidationCategorySummary(group.Key, group.Count(), group.Count(assertion => assertion.Passed), group.Count(assertion => !assertion.Passed)))
            .ToArray();
        var diagnostics = new List<string>();
        if (results.Length > 0 && results.All(result => result.Verdict == ExperimentVerdict.Support))
        {
            diagnostics.Add("All observed seeds Support this protocol; no failure boundary was observed in this run set.");
        }

        var mechanismCategory = categories.FirstOrDefault(category => string.Equals(category.Category, ValidationCheckTaxonomy.MechanismOutcome, StringComparison.Ordinal));
        if (mechanismCategory is { Checks: > 0 } && mechanismCategory.Failed == 0)
        {
            diagnostics.Add("All mechanism-outcome checks passed in this run set.");
        }

        return new ValidationProtocolSummary(
            experiment,
            results.Length,
            results.Count(result => result.Verdict == ExperimentVerdict.Support),
            results.Count(result => result.Verdict == ExperimentVerdict.Mixed),
            results.Count(result => result.Verdict == ExperimentVerdict.Disconfirm),
            results.Count(result => result.Verdict == ExperimentVerdict.Inconclusive),
            results.Sum(result => result.Assertions.Count),
            results.Sum(result => result.Assertions.Count(assertion => assertion.Passed)),
            results.Sum(result => result.Assertions.Count(assertion => !assertion.Passed)),
            categories,
            diagnostics);
    }

    private static ValidationChallengeSummary BuildChallengeSummary(ChallengeDefinition challenge, IReadOnlyList<RunResult> completedRuns)
    {
        var matches = completedRuns
            .SelectMany(run => run.Experiments)
            .Where(result => string.Equals(result.Name, challenge.Experiment, StringComparison.Ordinal) && challenge.Matches(result.Metrics))
            .ToArray();
        return new ValidationChallengeSummary(
            challenge.Id,
            challenge.Name,
            challenge.Experiment,
            challenge.Description,
            challenge.SelectionRule,
            matches.Length,
            matches.Count(result => result.Verdict == ExperimentVerdict.Support),
            matches.Count(result => result.Verdict == ExperimentVerdict.Mixed),
            matches.Count(result => result.Verdict == ExperimentVerdict.Disconfirm),
            matches.Count(result => result.Verdict == ExperimentVerdict.Inconclusive));
    }

    private static double Value(IReadOnlyDictionary<string, double> metrics, string name) => metrics.TryGetValue(name, out var value) ? value : double.NaN;
}
