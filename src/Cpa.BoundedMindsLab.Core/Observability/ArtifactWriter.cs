using System.Globalization;
using System.Text;
using System.Text.Json;
using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Validation;

namespace Cpa.BoundedMindsLab.Observability;

public static class ArtifactWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    public static void WriteRun(RunResult run, string status = "completed")
    {
        ArgumentNullException.ThrowIfNull(run);
        Directory.CreateDirectory(run.OutputDirectory);
        var manifest = new
        {
            schema = "cpa-bounded-minds-run-v1",
            version = "0.11.0",
            status,
            seed = run.Seed,
            experimentCount = run.Experiments.Count,
            experiments = run.Experiments.Select(result => new
            {
                result.Name,
                verdict = result.Verdict.ToString(),
                result.Question,
                result.Interpretation,
                result.Metrics,
                result.Assertions,
            }),
        };
        File.WriteAllText(
            Path.Combine(run.OutputDirectory, "manifest.json"),
            JsonSerializer.Serialize(manifest, JsonOptions));

        foreach (var result in run.Experiments)
        {
            var directory = Path.Combine(run.OutputDirectory, result.Name);
            Directory.CreateDirectory(directory);
            File.WriteAllText(
                Path.Combine(directory, "result.json"),
                JsonSerializer.Serialize(result, JsonOptions));
            WriteMetricsCsv(Path.Combine(directory, "metrics.csv"), result.Metrics);
        }
    }


    public static void WriteSessionManifest(
        string outputDirectory,
        IReadOnlyList<ulong> plannedSeeds,
        IReadOnlyList<ulong> completedSeeds,
        IReadOnlyList<string> experiments,
        string status,
        ulong? activeSeed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentNullException.ThrowIfNull(plannedSeeds);
        ArgumentNullException.ThrowIfNull(completedSeeds);
        ArgumentNullException.ThrowIfNull(experiments);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);

        Directory.CreateDirectory(outputDirectory);
        var manifest = new
        {
            schema = "cpa-bounded-minds-session-v1",
            version = "0.11.0",
            status,
            plannedSeeds,
            completedSeeds,
            activeSeed,
            seedSet = ValidationPlan.ClassifySeedSet(plannedSeeds),
            fullFrozenProtocolSet = ValidationPlan.IsFullFrozenProtocolSet(experiments),
            seedCount = plannedSeeds.Count,
            completedSeedCount = completedSeeds.Count,
            experiments,
        };
        File.WriteAllText(
            Path.Combine(outputDirectory, "session-manifest.json"),
            JsonSerializer.Serialize(manifest, JsonOptions));
    }

    public static void WriteReplication(ReplicationReport report, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(report);
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(
            Path.Combine(outputDirectory, "replication-report.json"),
            JsonSerializer.Serialize(report, JsonOptions));
    }

    public static void WriteValidation(ValidationReport report, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(report);
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(
            Path.Combine(outputDirectory, "validation-report.json"),
            JsonSerializer.Serialize(report, JsonOptions));
        File.WriteAllText(
            Path.Combine(outputDirectory, "validation-summary.md"),
            BuildValidationSummary(report));
    }

    private static string BuildValidationSummary(ValidationReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# CPA Bounded Minds validation summary");
        builder.AppendLine();
        builder.AppendLine($"- Version: {report.Version}");
        builder.AppendLine($"- Seed set: {report.SeedSet}");
        builder.AppendLine($"- Seeds: {string.Join(", ", report.Seeds)}");
        builder.AppendLine($"- Full frozen Protocol 01-07 set: {report.FullFrozenProtocolSet}");
        builder.AppendLine();
        builder.AppendLine("## Protocol outcomes");
        builder.AppendLine();
        builder.AppendLine("| Protocol | Runs | Support | Mixed | Disconfirm | Inconclusive | Assertions passed | Assertions failed |");
        builder.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");
        foreach (var protocol in report.Protocols)
        {
            builder.AppendLine($"| {protocol.Experiment} | {protocol.Runs} | {protocol.Support} | {protocol.Mixed} | {protocol.Disconfirm} | {protocol.Inconclusive} | {protocol.PassedAssertions} | {protocol.FailedAssertions} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Protocol evidence categories");
        builder.AppendLine();
        builder.AppendLine("Each cell is passed/checks. These categories are reporting metadata and do not change the frozen protocol verdict.");
        builder.AppendLine();
        builder.AppendLine("| Protocol | Manipulation | Mechanism outcome | Safety boundary | Accounting |");
        builder.AppendLine("| --- | ---: | ---: | ---: | ---: |");
        foreach (var protocol in report.Protocols)
        {
            builder.AppendLine($"| {protocol.Experiment} | {FormatCategory(protocol, ValidationCheckTaxonomy.Manipulation)} | {FormatCategory(protocol, ValidationCheckTaxonomy.MechanismOutcome)} | {FormatCategory(protocol, ValidationCheckTaxonomy.SafetyBoundary)} | {FormatCategory(protocol, ValidationCheckTaxonomy.AccountingConstraint)} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Check taxonomy");
        builder.AppendLine();
        builder.AppendLine("| Category | Checks | Passed | Failed |");
        builder.AppendLine("| --- | ---: | ---: | ---: |");
        foreach (var category in report.Categories)
        {
            builder.AppendLine($"| {category.Category} | {category.Checks} | {category.Passed} | {category.Failed} |");
        }

        builder.AppendLine();
        builder.AppendLine("## Challenge slices");
        builder.AppendLine();
        builder.AppendLine("Challenge slices are preregistered filters over holdout world descriptors. They do not alter the frozen protocols or tune thresholds.");
        builder.AppendLine();
        builder.AppendLine("| Challenge | Protocol | Matching runs | Support | Mixed | Disconfirm | Inconclusive |");
        builder.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | ---: |");
        foreach (var challenge in report.ChallengeProfiles)
        {
            builder.AppendLine($"| {challenge.Name} | {challenge.Experiment} | {challenge.MatchingRuns} | {challenge.Support} | {challenge.Mixed} | {challenge.Disconfirm} | {challenge.Inconclusive} |");
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

    private static string FormatCategory(ValidationProtocolSummary protocol, string category)
    {
        var summary = protocol.Categories.FirstOrDefault(item => string.Equals(item.Category, category, StringComparison.Ordinal));
        return summary is null ? "n/a" : $"{summary.Passed}/{summary.Checks}";
    }

    private static void WriteMetricsCsv(string path, IReadOnlyDictionary<string, double> metrics)
    {
        var builder = new StringBuilder();
        builder.AppendLine("metric,value");
        foreach (var pair in metrics.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            builder.Append(Escape(pair.Key));
            builder.Append(',');
            builder.AppendLine(pair.Value.ToString("R", CultureInfo.InvariantCulture));
        }

        File.WriteAllText(path, builder.ToString());
    }

    private static string Escape(string value) => value.Contains(',') || value.Contains('"')
        ? $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\""
        : value;
}
