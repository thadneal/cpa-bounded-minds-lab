using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Cpa.BoundedMindsLab.Falsification;

public static class StrategicInfluenceFalsificationRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static FalsificationReport RunV1(string outputDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        Directory.CreateDirectory(outputDirectory);
        var profileReports = new List<FalsificationProfileReport>(StrategicInfluenceFalsificationPlan.Profiles.Count);
        foreach (var profile in StrategicInfluenceFalsificationPlan.Profiles)
        {
            Console.WriteLine($"Strategic falsification profile: {profile.Name}");
            var cells = RunProfile(profile);
            var report = BuildProfileReport(profile, cells);
            profileReports.Add(report);
            WriteProfileCsv(outputDirectory, report);
            Console.WriteLine($"  cells with negative mean margin: {report.CellsWithNegativeMeanMargin}/{report.Cells.Count}");
        }

        var allCells = profileReports.Sum(profile => profile.Cells.Count);
        var replicateRuns = profileReports.Sum(profile => profile.Cells.Sum(cell => cell.Replicates));
        var result = new FalsificationReport(
            "cpa-bounded-minds-strategic-influence-falsification-v1",
            "0.14.0",
            StrategicInfluenceFalsificationPlan.Name,
            profileReports.Count,
            allCells,
            replicateRuns,
            profileReports,
            [
                "strategic-influence-falsification-v1 is exploratory operating-envelope evidence, not a replacement for p08-holdout-v1.",
                "The frozen Protocol 08 experiment and world-generator files are not changed by these probes.",
                "The accountable receiver equations are copied from frozen Protocol 08. Some sender capabilities and consequence schedules deliberately extend beyond the original development world family.",
                "A negative margin is desired boundary information. Do not tune the receiver and then reinterpret the same surface as confirmation.",
                "The aligned-noise surface is a null-harm/over-deterrence assay: there is no hidden sender divergence there.",
            ]);
        File.WriteAllText(Path.Combine(outputDirectory, "strategic-falsification-report.json"), JsonSerializer.Serialize(result, JsonOptions));
        File.WriteAllText(Path.Combine(outputDirectory, "strategic-falsification-plan.json"), JsonSerializer.Serialize(BuildPlanDocument(), JsonOptions));
        File.WriteAllText(Path.Combine(outputDirectory, "strategic-falsification-summary.md"), BuildSummary(result));
        return result;
    }

    private static FalsificationCellResult[] RunProfile(FalsificationProfileDefinition profile)
    {
        var cells = new List<FalsificationCellResult>(profile.XAxis.Values.Length * profile.YAxis.Values.Length);
        for (var xIndex = 0; xIndex < profile.XAxis.Values.Length; xIndex++)
        {
            for (var yIndex = 0; yIndex < profile.YAxis.Values.Length; yIndex++)
            {
                var x = profile.XAxis.Values[xIndex];
                var y = profile.YAxis.Values[yIndex];
                var runs = new Dictionary<string, double>[profile.Replicates];
                for (var replicate = 0; replicate < profile.Replicates; replicate++)
                {
                    runs[replicate] = profile.Evaluate(x, y, ReplicateSeed(profile.Id, xIndex, yIndex, replicate));
                }

                cells.Add(SummarizeCell(profile, x, y, runs));
            }
        }

        return cells.ToArray();
    }

    private static FalsificationCellResult SummarizeCell(
        FalsificationProfileDefinition profile,
        double x,
        double y,
        Dictionary<string, double>[] runs)
    {
        var metricNames = runs.SelectMany(metrics => metrics.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var means = new Dictionary<string, double>(StringComparer.Ordinal);
        for (var metricIndex = 0; metricIndex < metricNames.Length; metricIndex++)
        {
            var metricName = metricNames[metricIndex];
            means[metricName] = runs.Average(metrics => metrics[metricName]);
        }

        var margins = runs.Select(metrics => metrics[profile.PrimaryMarginMetric]).ToArray();
        return new FalsificationCellResult(
            profile.Id,
            profile.Protocol,
            x,
            y,
            runs.Length,
            margins.Average(),
            margins.Min(),
            margins.Max(),
            margins.Count(margin => margin < 0.0),
            means);
    }

    private static FalsificationProfileReport BuildProfileReport(
        FalsificationProfileDefinition profile,
        FalsificationCellResult[] cells) => new(
        profile.Id,
        profile.Name,
        profile.Protocol,
        profile.Question,
        profile.Method,
        profile.XAxis,
        profile.YAxis,
        profile.PrimaryMarginMetric,
        profile.PrimaryMarginDescription,
        profile.InterpretationLimit,
        cells,
        cells.Count(cell => cell.MeanPrimaryMargin < 0.0),
        cells.Count(cell => cell.NegativeMargins > 0),
        cells.Min(cell => cell.MinimumPrimaryMargin),
        cells.Max(cell => cell.MaximumPrimaryMargin));

    private static object BuildPlanDocument() => new
    {
        schema = "cpa-bounded-minds-strategic-influence-falsification-plan-v1",
        version = "0.14.0",
        name = StrategicInfluenceFalsificationPlan.Name,
        purpose = "Controlled falsification of frozen Protocol 08 receiver behavior across delayed consequence, stronger sender adaptation, betrayal, divergence prevalence, feedback observability, and aligned-noise over-deterrence conditions.",
        profiles = StrategicInfluenceFalsificationPlan.Profiles.Select(profile => new
        {
            profile.Id,
            profile.Name,
            profile.Protocol,
            profile.Question,
            profile.Method,
            profile.XAxis,
            profile.YAxis,
            profile.Replicates,
            profile.PrimaryMarginMetric,
            profile.PrimaryMarginDescription,
            profile.InterpretationLimit,
        }),
    };

    private static void WriteProfileCsv(string outputDirectory, FalsificationProfileReport report)
    {
        var metricNames = report.Cells.SelectMany(cell => cell.MeanMetrics.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var builder = new StringBuilder();
        builder.Append(report.XAxis.Name).Append(',')
            .Append(report.YAxis.Name).Append(',')
            .Append("replicates,mean_primary_margin,minimum_primary_margin,maximum_primary_margin,negative_replicates");
        for (var index = 0; index < metricNames.Length; index++)
        {
            builder.Append(',').Append(metricNames[index]);
        }

        builder.AppendLine();
        foreach (var cell in report.Cells)
        {
            builder.Append(Format(cell.X)).Append(',')
                .Append(Format(cell.Y)).Append(',')
                .Append(cell.Replicates.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(Format(cell.MeanPrimaryMargin)).Append(',')
                .Append(Format(cell.MinimumPrimaryMargin)).Append(',')
                .Append(Format(cell.MaximumPrimaryMargin)).Append(',')
                .Append(cell.NegativeMargins.ToString(CultureInfo.InvariantCulture));
            for (var index = 0; index < metricNames.Length; index++)
            {
                builder.Append(',').Append(Format(cell.MeanMetrics[metricNames[index]]));
            }

            builder.AppendLine();
        }

        File.WriteAllText(Path.Combine(outputDirectory, $"{report.Id}.csv"), builder.ToString());
    }

    private static string BuildSummary(FalsificationReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# CPA Bounded Minds Protocol 08 strategic-influence falsification summary");
        builder.AppendLine();
        builder.AppendLine($"- Version: {report.Version}");
        builder.AppendLine($"- Phase: {report.Name}");
        builder.AppendLine($"- Profiles: {report.Profiles}");
        builder.AppendLine($"- Grid cells: {report.Cells}");
        builder.AppendLine($"- Deterministic replicate runs: {report.ReplicateRuns}");
        builder.AppendLine();
        builder.AppendLine("These are controlled failure-surface probes. Negative margins are informative and are not automatically defects in the laboratory.");
        foreach (var profile in report.Results)
        {
            builder.AppendLine();
            builder.AppendLine($"## {profile.Name}");
            builder.AppendLine();
            builder.AppendLine(profile.Question);
            builder.AppendLine();
            builder.AppendLine(profile.Method);
            builder.AppendLine();
            builder.AppendLine($"X axis: **{profile.XAxis.Label}** - {profile.XAxis.Description}");
            builder.AppendLine();
            builder.AppendLine($"Y axis: **{profile.YAxis.Label}** - {profile.YAxis.Description}");
            builder.AppendLine();
            builder.AppendLine($"Primary margin: {profile.PrimaryMarginDescription}");
            builder.AppendLine();
            builder.AppendLine($"Cells with negative mean margin: **{profile.CellsWithNegativeMeanMargin}/{profile.Cells.Count}**. Cells with at least one negative replicate: **{profile.CellsWithAnyNegativeReplicate}/{profile.Cells.Count}**.");
            builder.AppendLine();
            builder.AppendLine(profile.InterpretationLimit);
        }

        return builder.ToString();
    }

    private static ulong ReplicateSeed(string profileId, int xIndex, int yIndex, int replicate)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;
        foreach (var character in profileId)
        {
            hash ^= character;
            hash *= prime;
        }

        hash ^= unchecked((ulong)(xIndex + 1) * 0x9E3779B97F4A7C15UL);
        hash ^= unchecked((ulong)(yIndex + 1) * 0xBF58476D1CE4E5B9UL);
        hash ^= unchecked((ulong)(replicate + 1) * 0x94D049BB133111EBUL);
        return hash;
    }

    private static string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}
