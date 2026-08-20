using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Validation;

namespace Cpa.BoundedMindsLab.Challenge;

public sealed record ChallengeRunSummary(
    ulong Seed,
    string Band,
    int BandIndex,
    double StressScore,
    ExperimentVerdict Verdict,
    double BoundaryMargin,
    IReadOnlyDictionary<string, double> Descriptors,
    IReadOnlyList<ValidationCategorySummary> Categories);

public sealed record ChallengeBandSummary(
    string Band,
    int BandIndex,
    int Runs,
    double MeanStressScore,
    double MinimumStressScore,
    double MaximumStressScore,
    int Support,
    int Mixed,
    int Disconfirm,
    int Inconclusive,
    double MeanBoundaryMargin,
    int BoundaryFailures,
    IReadOnlyList<ValidationCategorySummary> Categories);

public sealed record ChallengeProfileSummary(
    string Id,
    string Name,
    string Experiment,
    string Question,
    string StressDirection,
    string Limitation,
    string BoundaryMarginDescription,
    IReadOnlyList<ChallengeBandSummary> Bands,
    IReadOnlyList<ChallengeRunSummary> Runs,
    string? FirstObservedFailureBand,
    IReadOnlyList<string> Diagnostics);

public sealed record ChallengeReport(
    string Schema,
    string Version,
    string Name,
    ulong CandidateSeedStart,
    ulong CandidateSeedEnd,
    int Bands,
    int SeedsPerBand,
    int TotalRuns,
    IReadOnlyList<ChallengeProfileSummary> Profiles,
    IReadOnlyList<string> Diagnostics);

public static class ChallengeReportBuilder
{
    public static ChallengeReport Create(
        IReadOnlyList<ChallengeSelection> selections,
        IReadOnlyDictionary<(string ProfileId, ulong Seed), ExperimentResult> results)
    {
        ArgumentNullException.ThrowIfNull(selections);
        ArgumentNullException.ThrowIfNull(results);
        var profiles = ChallengePlan.Profiles.Select(profile => BuildProfile(profile, selections, results)).ToArray();
        var diagnostics = new List<string>
        {
            "challenge-v1 is an adversarial search over the frozen Protocol 03-07 world generators. Seeds were selected from world descriptors before experiment outcomes were observed.",
            "development-v1 and the consumed holdout-v1 seeds are excluded from challenge-v1 candidate selection.",
            "Challenge bands are rank bands within each frozen generator, not universal stress units. Cross-profile stress scores are not comparable.",
            "A failure in challenge-v1 is an operating-envelope observation, not a reason to retune the frozen protocol and rerun holdout-v1.",
            "Protocol 04 still lacks the proposed stronger equal-budget alternative control. Its challenge-v1 sweep tests environmental stress only.",
        };
        if (profiles.All(profile => profile.FirstObservedFailureBand is null))
        {
            diagnostics.Add("No challenge profile crossed its registered boundary. Treat this as evidence that the current frozen generators may still be too protected; the next challenge should parameterize worlds beyond their original support rather than merely enlarging the seed search.");
        }

        return new ChallengeReport(
            "cpa-bounded-minds-challenge-v1",
            "0.11.0",
            ChallengePlan.Name,
            ChallengePlan.CandidateSeedStart,
            ChallengePlan.CandidateSeedEnd,
            ChallengePlan.BandCount,
            ChallengePlan.SeedsPerBand,
            profiles.Sum(profile => profile.Runs.Count),
            profiles,
            diagnostics);
    }

    private static ChallengeProfileSummary BuildProfile(
        ChallengeProfileDefinition profile,
        IReadOnlyList<ChallengeSelection> selections,
        IReadOnlyDictionary<(string ProfileId, ulong Seed), ExperimentResult> results)
    {
        var profileSelections = selections.Where(selection => string.Equals(selection.ProfileId, profile.Id, StringComparison.Ordinal)).ToArray();
        var runs = profileSelections.Select(selection =>
        {
            if (!results.TryGetValue((profile.Id, selection.Seed), out var result))
            {
                throw new InvalidOperationException($"Missing challenge result for {profile.Id} seed {selection.Seed}.");
            }

            return new ChallengeRunSummary(
                selection.Seed,
                selection.Band,
                selection.BandIndex,
                selection.StressScore,
                result.Verdict,
                profile.BoundaryMargin(result.Metrics),
                selection.Descriptors,
                SummarizeCategories(result.Assertions));
        }).OrderBy(run => run.BandIndex).ThenBy(run => run.StressScore).ToArray();

        var bands = runs.GroupBy(run => new { run.Band, run.BandIndex })
            .OrderBy(group => group.Key.BandIndex)
            .Select(group =>
            {
                var items = group.ToArray();
                return new ChallengeBandSummary(
                    group.Key.Band,
                    group.Key.BandIndex,
                    items.Length,
                    items.Average(item => item.StressScore),
                    items.Min(item => item.StressScore),
                    items.Max(item => item.StressScore),
                    items.Count(item => item.Verdict == ExperimentVerdict.Support),
                    items.Count(item => item.Verdict == ExperimentVerdict.Mixed),
                    items.Count(item => item.Verdict == ExperimentVerdict.Disconfirm),
                    items.Count(item => item.Verdict == ExperimentVerdict.Inconclusive),
                    items.Average(item => item.BoundaryMargin),
                    items.Count(item => !double.IsNaN(item.BoundaryMargin) && item.BoundaryMargin < 0.0),
                    MergeCategories(items.SelectMany(item => item.Categories)));
            })
            .ToArray();

        var firstFailure = bands.FirstOrDefault(band => band.BoundaryFailures > 0)?.Band;
        var firstNonSupport = bands.FirstOrDefault(band => band.Mixed > 0 || band.Disconfirm > 0 || band.Inconclusive > 0)?.Band;
        var diagnostics = new List<string>();
        if (firstFailure is null)
        {
            diagnostics.Add("No registered boundary-margin crossing was observed in this profile's selected generator range.");
        }
        else
        {
            diagnostics.Add($"The first selected stress band with a negative registered boundary margin was {firstFailure}.");
        }

        if (firstNonSupport is not null)
        {
            diagnostics.Add($"The first band containing a frozen protocol verdict other than Support was {firstNonSupport}. This may reflect manipulation or another assertion class rather than the profile boundary margin.");
        }

        if (bands.Length >= 2 && bands[^1].MeanBoundaryMargin > bands[0].MeanBoundaryMargin)
        {
            diagnostics.Add("The registered boundary margin improved rather than degraded from the lowest to highest selected stress band. Inspect whether the stress descriptor is actually aligned with the mechanism's failure pressure before interpreting the curve.");
        }

        return new ChallengeProfileSummary(
            profile.Id,
            profile.Name,
            profile.Experiment,
            profile.Question,
            profile.StressDirection,
            profile.Limitation,
            profile.BoundaryMarginDescription,
            bands,
            runs,
            firstFailure,
            diagnostics);
    }

    private static ValidationCategorySummary[] SummarizeCategories(IReadOnlyList<ExperimentAssertion> assertions) =>
        assertions
            .GroupBy(assertion => ValidationCheckTaxonomy.Classify(assertion.Name), StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new ValidationCategorySummary(
                group.Key,
                group.Count(),
                group.Count(assertion => assertion.Passed),
                group.Count(assertion => !assertion.Passed)))
            .ToArray();

    private static ValidationCategorySummary[] MergeCategories(IEnumerable<ValidationCategorySummary> categories) =>
        categories
            .GroupBy(category => category.Category, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new ValidationCategorySummary(
                group.Key,
                group.Sum(category => category.Checks),
                group.Sum(category => category.Passed),
                group.Sum(category => category.Failed)))
            .ToArray();
}
