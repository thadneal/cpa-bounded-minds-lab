using System.Globalization;
using System.Text;
using System.Text.Json;
using Cpa.BoundedMindsLab.Experiments;

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
            version = "0.2.0",
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
            version = "0.2.0",
            status,
            plannedSeeds,
            completedSeeds,
            activeSeed,
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
