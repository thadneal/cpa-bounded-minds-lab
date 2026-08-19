using Cpa.BoundedMindsLab.Observability;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed record ReplicationRunSummary(
    ulong Seed,
    IReadOnlyDictionary<string, ExperimentVerdict> Verdicts,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, double>> Metrics);

public sealed record ReplicationExperimentSummary(
    string Experiment,
    int Support,
    int Mixed,
    int Disconfirm,
    int Inconclusive,
    IReadOnlyDictionary<string, double> MeanMetrics);

public sealed record ReplicationReport(
    string Schema,
    IReadOnlyList<ulong> Seeds,
    IReadOnlyList<ReplicationRunSummary> Runs,
    IReadOnlyList<ReplicationExperimentSummary> Experiments);

public static class ReplicationRunner
{
    public static ReplicationReport Run(
        IReadOnlyList<IExperiment> experiments,
        IReadOnlyList<ulong> seeds,
        string outputDirectory,
        bool quiet = false)
    {
        ArgumentNullException.ThrowIfNull(experiments);
        ArgumentNullException.ThrowIfNull(seeds);
        if (seeds.Count == 0)
        {
            throw new ArgumentException("Provide at least one seed.", nameof(seeds));
        }

        var root = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(root);
        var completedRuns = new List<RunResult>(seeds.Count);
        foreach (var seed in seeds)
        {
            completedRuns.Add(ExperimentRunner.Run(
                experiments,
                seed,
                Path.Combine(root, $"seed-{seed}"),
                quiet: true));
        }

        var report = CreateReport(experiments, completedRuns);
        ArtifactWriter.WriteReplication(report, root);
        if (!quiet)
        {
            foreach (var summary in report.Experiments)
            {
                Console.WriteLine(
                    $"{summary.Experiment}: support={summary.Support}, mixed={summary.Mixed}, disconfirm={summary.Disconfirm}, inconclusive={summary.Inconclusive}");
            }
        }

        return report;
    }

    public static ReplicationReport CreateReport(
        IReadOnlyList<IExperiment> experiments,
        IReadOnlyList<RunResult> completedRuns)
    {
        ArgumentNullException.ThrowIfNull(experiments);
        ArgumentNullException.ThrowIfNull(completedRuns);
        if (completedRuns.Count == 0)
        {
            throw new ArgumentException("Provide at least one completed run.", nameof(completedRuns));
        }

        var runs = completedRuns.Select(ToSummary).ToList();
        var summaries = experiments.Select(experiment => Aggregate(experiment.Name, runs)).ToArray();
        return new ReplicationReport(
            "cpa-bounded-minds-replication-v1",
            completedRuns.Select(run => run.Seed).ToArray(),
            runs,
            summaries);
    }

    private static ReplicationRunSummary ToSummary(RunResult run) => new(
        run.Seed,
        run.Experiments.ToDictionary(result => result.Name, result => result.Verdict, StringComparer.Ordinal),
        run.Experiments.ToDictionary(
            result => result.Name,
            result => result.Metrics,
            StringComparer.Ordinal));

    private static ReplicationExperimentSummary Aggregate(string experiment, List<ReplicationRunSummary> runs)
    {
        var verdicts = runs.Select(run => run.Verdicts[experiment]).ToArray();
        var metricNames = runs
            .SelectMany(run => run.Metrics[experiment].Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var means = metricNames.ToDictionary(
            name => name,
            name => runs.Average(run => run.Metrics[experiment].TryGetValue(name, out var value) ? value : 0.0),
            StringComparer.Ordinal);
        return new ReplicationExperimentSummary(
            experiment,
            verdicts.Count(verdict => verdict == ExperimentVerdict.Support),
            verdicts.Count(verdict => verdict == ExperimentVerdict.Mixed),
            verdicts.Count(verdict => verdict == ExperimentVerdict.Disconfirm),
            verdicts.Count(verdict => verdict == ExperimentVerdict.Inconclusive),
            means);
    }
}
