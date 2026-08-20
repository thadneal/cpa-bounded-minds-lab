using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Observability;

namespace Cpa.BoundedMindsLab.Validation;

public sealed record ValidationRunResult(
    ReplicationReport Replication,
    ValidationReport Validation);

public static class ValidationRunner
{
    public static ValidationRunResult RunHoldout(string outputDirectory, bool quiet = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        var root = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(root);
        var experiments = ValidationPlan.FrozenProtocolNames.Select(ExperimentCatalog.Get).ToArray();
        var completedRuns = new List<RunResult>(ValidationPlan.HoldoutSeeds.Count);
        foreach (var seed in ValidationPlan.HoldoutSeeds)
        {
            completedRuns.Add(ExperimentRunner.Run(experiments, seed, Path.Combine(root, $"seed-{seed}"), quiet: true));
        }

        var replication = ReplicationRunner.CreateReport(experiments, completedRuns);
        var validation = ValidationReportBuilder.Create(completedRuns);
        ArtifactWriter.WriteReplication(replication, root);
        ArtifactWriter.WriteValidation(validation, root);
        ArtifactWriter.WriteSessionManifest(
            root,
            ValidationPlan.HoldoutSeeds,
            ValidationPlan.HoldoutSeeds,
            experiments.Select(experiment => experiment.Name).ToArray(),
            "completed",
            null);
        if (!quiet)
        {
            Console.WriteLine($"Validation seed set: {validation.SeedSet}; {validation.Seeds.Count} seed(s); {validation.Protocols.Count} protocol(s).");
            foreach (var protocol in validation.Protocols)
            {
                Console.WriteLine($"{protocol.Experiment}: support={protocol.Support}, mixed={protocol.Mixed}, disconfirm={protocol.Disconfirm}, inconclusive={protocol.Inconclusive}");
            }
        }

        return new ValidationRunResult(replication, validation);
    }
}
