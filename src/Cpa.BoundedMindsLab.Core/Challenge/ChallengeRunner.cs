using System.Globalization;
using System.Text;
using System.Text.Json;
using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Validation;

namespace Cpa.BoundedMindsLab.Challenge;

public static class ChallengeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public static ChallengeReport RunV1(string outputDirectory, bool quiet = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        var root = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(root);
        var selections = ChallengePlan.BuildSelections();
        WritePlan(root, selections);

        var results = new Dictionary<(string ProfileId, ulong Seed), ExperimentResult>();
        foreach (var selection in selections)
        {
            var experiment = ExperimentCatalog.Get(selection.Experiment);
            var runDirectory = Path.Combine(root, selection.ProfileId, selection.Band, $"seed-{selection.Seed}");
            var run = ExperimentRunner.Run([experiment], selection.Seed, runDirectory, quiet: true);
            var result = run.Experiments.Single();
            results.Add((selection.ProfileId, selection.Seed), result);
            if (!quiet)
            {
                var profile = ChallengePlan.GetProfile(selection.ProfileId);
                var margin = profile.BoundaryMargin(result.Metrics);
                Console.WriteLine($"{selection.ProfileId} {selection.Band} seed {selection.Seed}: stress={selection.StressScore:0.0000}, verdict={result.Verdict}, margin={margin:0.000000}");
            }
        }

        var report = ChallengeReportBuilder.Create(selections, results);
        File.WriteAllText(Path.Combine(root, "challenge-report.json"), JsonSerializer.Serialize(report, JsonOptions));
        File.WriteAllText(Path.Combine(root, "challenge-summary.md"), BuildSummary(report));
        return report;
    }

    private static void WritePlan(string root, IReadOnlyList<ChallengeSelection> selections)
    {
        var plan = new
        {
            schema = "cpa-bounded-minds-challenge-plan-v1",
            version = "0.14.0",
            name = ChallengePlan.Name,
            candidateSeedStart = ChallengePlan.CandidateSeedStart,
            candidateSeedEnd = ChallengePlan.CandidateSeedEnd,
            bands = ChallengePlan.BandCount,
            seedsPerBand = ChallengePlan.SeedsPerBand,
            selectionIsOutcomeBlind = true,
            excludedSeedSets = new[] { ValidationPlan.DevelopmentSetName, ValidationPlan.HoldoutSetName },
            profiles = ChallengePlan.Profiles.Select(profile => new
            {
                profile.Id,
                profile.Name,
                profile.Experiment,
                profile.Question,
                profile.StressDirection,
                profile.Limitation,
                profile.BoundaryMarginDescription,
            }),
            selections,
        };
        File.WriteAllText(Path.Combine(root, "challenge-plan.json"), JsonSerializer.Serialize(plan, JsonOptions));
    }

    private static string BuildSummary(ChallengeReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# CPA Bounded Minds challenge summary");
        builder.AppendLine();
        builder.AppendLine($"- Version: {report.Version}");
        builder.AppendLine($"- Challenge: {report.Name}");
        builder.AppendLine($"- Candidate seed range: {report.CandidateSeedStart}-{report.CandidateSeedEnd}");
        builder.AppendLine($"- Selected runs: {report.TotalRuns}");
        builder.AppendLine($"- Bands per profile: {report.Bands}");
        builder.AppendLine($"- Seeds per band: {report.SeedsPerBand}");
        builder.AppendLine();
        builder.AppendLine("challenge-v1 selects seeds from world descriptors before outcomes are observed. It searches for an operating envelope inside the frozen Protocol 03-07 generator families; it is intentionally not a second holdout set.");

        foreach (var profile in report.Profiles)
        {
            builder.AppendLine();
            builder.AppendLine($"## {profile.Name}");
            builder.AppendLine();
            builder.AppendLine(profile.Question);
            builder.AppendLine();
            builder.AppendLine($"Stress: {profile.StressDirection}");
            builder.AppendLine();
            builder.AppendLine($"Boundary margin: {profile.BoundaryMarginDescription}");
            builder.AppendLine();
            builder.AppendLine("| Band | Runs | Mean stress | Support | Mixed | Disconfirm | Mean boundary margin | Boundary failures | Mechanism | Safety | Manipulation | Accounting |");
            builder.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
            foreach (var band in profile.Bands)
            {
                builder.AppendLine($"| {band.Band} | {band.Runs} | {band.MeanStressScore.ToString("0.0000", CultureInfo.InvariantCulture)} | {band.Support} | {band.Mixed} | {band.Disconfirm} | {band.MeanBoundaryMargin.ToString("0.000000", CultureInfo.InvariantCulture)} | {band.BoundaryFailures} | {FormatCategory(band, ValidationCheckTaxonomy.MechanismOutcome)} | {FormatCategory(band, ValidationCheckTaxonomy.SafetyBoundary)} | {FormatCategory(band, ValidationCheckTaxonomy.Manipulation)} | {FormatCategory(band, ValidationCheckTaxonomy.AccountingConstraint)} |");
            }

            builder.AppendLine();
            builder.AppendLine($"First observed negative boundary-margin band: {profile.FirstObservedFailureBand ?? "none"}");
            builder.AppendLine();
            builder.AppendLine($"Limitation: {profile.Limitation}");
            foreach (var diagnostic in profile.Diagnostics)
            {
                builder.AppendLine($"- {diagnostic}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Diagnostics");
        builder.AppendLine();
        foreach (var diagnostic in report.Diagnostics)
        {
            builder.AppendLine($"- {diagnostic}");
        }

        return builder.ToString();
    }

    private static string FormatCategory(ChallengeBandSummary band, string category)
    {
        var summary = band.Categories.FirstOrDefault(item => string.Equals(item.Category, category, StringComparison.Ordinal));
        return summary is null ? "n/a" : $"{summary.Passed}/{summary.Checks}";
    }
}
