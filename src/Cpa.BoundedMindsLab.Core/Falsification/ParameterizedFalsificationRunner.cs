using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Cpa.BoundedMindsLab.Falsification;

public static class ParameterizedFalsificationRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static FalsificationReport RunV1(string outputDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory is required.", nameof(outputDirectory));
        }
        Directory.CreateDirectory(outputDirectory);
        var profileReports = new List<FalsificationProfileReport>(ParameterizedFalsificationPlan.Profiles.Count);
        foreach (var profile in ParameterizedFalsificationPlan.Profiles)
        {
            Console.WriteLine($"Falsification profile: {profile.Name}");
            var cells = RunProfile(profile);
            var report = BuildProfileReport(profile, cells);
            profileReports.Add(report);
            WriteProfileCsv(outputDirectory, report);
            Console.WriteLine($"  cells with negative mean margin: {report.CellsWithNegativeMeanMargin}/{report.Cells.Count}");
        }

        var allCells = profileReports.Sum(profile => profile.Cells.Count);
        var replicateRuns = profileReports.Sum(profile => profile.Cells.Sum(cell => cell.Replicates));
        var result = new FalsificationReport(
            "cpa-bounded-minds-parameterized-falsification-v1",
            "0.13.0",
            ParameterizedFalsificationPlan.Name,
            profileReports.Count,
            allCells,
            replicateRuns,
            profileReports,
            [
                "parameterized-falsification-v1 is exploratory falsification, not a fresh holdout and not a new Protocol 08.",
                "Frozen Protocol 01-07 experiment and world-generator files remain unchanged; these micro-assays copy selected frozen local equations into controlled intervention probes so causal axes can be separated.",
                "challenge-v1 is consumed developmental evidence. Its composite stress rankings should not be treated as monotonic causal variables.",
                "A negative margin is desired boundary information. Do not tune a mechanism and then reinterpret the same surface as confirmation.",
                "Protocol 04 receives a stronger same-information comparator in this phase. The comparator is an assay control, not proposed CPA machinery.",
            ]);
        File.WriteAllText(Path.Combine(outputDirectory, "parameterized-report.json"), JsonSerializer.Serialize(result, JsonOptions));
        File.WriteAllText(Path.Combine(outputDirectory, "parameterized-plan.json"), JsonSerializer.Serialize(BuildPlanDocument(), JsonOptions));
        File.WriteAllText(Path.Combine(outputDirectory, "parameterized-summary.md"), BuildSummary(result));
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
        if (runs.Length == 0)
        {
            throw new InvalidOperationException($"Falsification profile {profile.Id} produced no replicates.");
        }

        var metricNames = runs.SelectMany(metrics => metrics.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var means = new Dictionary<string, double>(StringComparer.Ordinal);
        for (var metricIndex = 0; metricIndex < metricNames.Length; metricIndex++)
        {
            var metricName = metricNames[metricIndex];
            means[metricName] = runs.Average(metrics => metrics.TryGetValue(metricName, out var value) ? value : 0.0);
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
        schema = "cpa-bounded-minds-parameterized-falsification-plan-v1",
        version = "0.13.0",
        name = ParameterizedFalsificationPlan.Name,
        purpose = "Controlled causal intervention beyond the support of the frozen P03-P07 world generators. This is exploratory falsification, not confirmation.",
        profiles = ParameterizedFalsificationPlan.Profiles.Select(profile => new
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
        builder.AppendLine("# CPA Bounded Minds parameterized falsification summary");
        builder.AppendLine();
        builder.AppendLine($"- Version: {report.Version}");
        builder.AppendLine($"- Phase: {report.Name}");
        builder.AppendLine($"- Profiles: {report.Profiles}");
        builder.AppendLine($"- Grid cells: {report.Cells}");
        builder.AppendLine($"- Deterministic replicate runs: {report.ReplicateRuns}");
        builder.AppendLine();
        builder.AppendLine("This phase is not a new protocol sequence and not a fresh validation set. It deliberately intervenes on causal variables outside the original frozen generator support in order to locate failure surfaces.");
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
            builder.AppendLine($"Observed replicate margin range: `{Format(profile.MinimumObservedMargin)}` to `{Format(profile.MaximumObservedMargin)}`.");
            builder.AppendLine();
            builder.AppendLine("| X | Y | Mean margin | Negative reps |");
            builder.AppendLine("| ---: | ---: | ---: | ---: |");
            foreach (var cell in profile.Cells)
            {
                builder.AppendLine($"| {Format(cell.X)} | {Format(cell.Y)} | {Format(cell.MeanPrimaryMargin)} | {cell.NegativeMargins}/{cell.Replicates} |");
            }

            builder.AppendLine();
            builder.AppendLine($"Interpretation limit: {profile.InterpretationLimit}");
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

    private static ulong ReplicateSeed(string profileId, int xIndex, int yIndex, int replicate)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;
        for (var index = 0; index < profileId.Length; index++)
        {
            hash ^= profileId[index];
            hash *= prime;
        }

        hash ^= (ulong)(xIndex + 1);
        hash *= prime;
        hash ^= (ulong)(yIndex + 1);
        hash *= prime;
        hash ^= (ulong)(replicate + 1);
        hash *= prime;
        return hash;
    }

    private static string Format(double value) => value.ToString("0.000000", CultureInfo.InvariantCulture);
}
