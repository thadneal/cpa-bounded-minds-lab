using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Cpa.BoundedMindsLab.Falsification;

public static class AuthorityAncestryFalsificationRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static FalsificationReport RunV1(string outputDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        Directory.CreateDirectory(outputDirectory);
        var profileReports = new List<FalsificationProfileReport>(AuthorityAncestryFalsificationPlan.Profiles.Count);
        foreach (var profile in AuthorityAncestryFalsificationPlan.Profiles)
        {
            Console.WriteLine($"Protocol 09 falsification profile: {profile.Name}");
            var cells = RunProfile(profile);
            var report = BuildProfileReport(profile, cells);
            profileReports.Add(report);
            WriteProfileCsv(outputDirectory, report);
            Console.WriteLine($"  cells with negative mean margin: {report.CellsWithNegativeMeanMargin}/{report.Cells.Count}");
        }

        var allCells = profileReports.Sum(profile => profile.Cells.Count);
        var replicateRuns = profileReports.Sum(profile => profile.Cells.Sum(cell => cell.Replicates));
        var result = new FalsificationReport(
            "cpa-bounded-minds-authority-ancestry-falsification-v1",
            "1.0.0",
            AuthorityAncestryFalsificationPlan.Name,
            profileReports.Count,
            allCells,
            replicateRuns,
            profileReports,
            [
                "authority-ancestry-falsification-v1 is exploratory operating-envelope evidence. It must not be treated as fresh confirmation of Protocol 09.",
                "The exact Protocol 09 experiment and world-generator sources are frozen. These probes copy selected local equations into controlled interventions so root diversity, lineage fidelity, topology, consequence delay, and consequence noise can be varied independently.",
                "Circularity is not treated as evidence of falsehood. Several surfaces deliberately contain useful circular or fully grounded influence so over-deterrence is visible as a failure rather than rewarded as caution.",
                "A negative margin is desired boundary information. Do not tune Protocol 09 from these results and then reuse the same surfaces as confirmation.",
                "The preregistered p09-holdout-v1 artifact was preserved before the first interpretation of these surfaces. Both evidence sets are now consumed and reruns are reproducibility only.",
            ]);

        File.WriteAllText(Path.Combine(outputDirectory, "authority-ancestry-falsification-report.json"), JsonSerializer.Serialize(result, JsonOptions));
        File.WriteAllText(Path.Combine(outputDirectory, "authority-ancestry-falsification-plan.json"), JsonSerializer.Serialize(BuildPlanDocument(), JsonOptions));
        File.WriteAllText(Path.Combine(outputDirectory, "authority-ancestry-falsification-summary.md"), BuildSummary(result));
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

        return [.. cells];
    }

    private static FalsificationCellResult SummarizeCell(
        FalsificationProfileDefinition profile,
        double x,
        double y,
        Dictionary<string, double>[] runs)
    {
        var metricNames = runs.SelectMany(metrics => metrics.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var means = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var metricName in metricNames)
        {
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

    private static FalsificationProfileReport BuildProfileReport(FalsificationProfileDefinition profile, FalsificationCellResult[] cells) => new(
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
        schema = "cpa-bounded-minds-authority-ancestry-falsification-plan-v1",
        version = "1.0.0",
        name = AuthorityAncestryFalsificationPlan.Name,
        purpose = "Controlled operating-envelope falsification of frozen Protocol 09 after its canonical development result. This is exploratory evidence, not confirmation.",
        profiles = AuthorityAncestryFalsificationPlan.Profiles.Select(profile => new
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
        foreach (var metricName in metricNames)
        {
            builder.Append(',').Append(metricName);
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
            foreach (var metricName in metricNames)
            {
                builder.Append(',').Append(Format(cell.MeanMetrics[metricName]));
            }

            builder.AppendLine();
        }

        File.WriteAllText(Path.Combine(outputDirectory, $"{report.Id}.csv"), builder.ToString());
    }

    private static string BuildSummary(FalsificationReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# CPA Bounded Minds Protocol 09 falsification summary");
        builder.AppendLine();
        builder.AppendLine($"- Version: {report.Version}");
        builder.AppendLine($"- Phase: {report.Name}");
        builder.AppendLine($"- Profiles: {report.Profiles}");
        builder.AppendLine($"- Grid cells: {report.Cells}");
        builder.AppendLine($"- Deterministic replicate runs: {report.ReplicateRuns}");
        builder.AppendLine();
        builder.AppendLine("This phase maps where authority ancestry remains useful, where approximate lineage becomes insufficient, and where caution itself becomes harmful. It does not change the frozen Protocol 09 result.");
        foreach (var profile in report.Results)
        {
            builder.AppendLine();
            builder.AppendLine($"## {profile.Name}");
            builder.AppendLine();
            builder.AppendLine(profile.Question);
            builder.AppendLine();
            builder.AppendLine(profile.Method);
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
        foreach (var character in profileId)
        {
            hash ^= character;
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
