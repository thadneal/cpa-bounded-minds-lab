using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Observability;

namespace Cpa.BoundedMindsLab.Validation;

public static class AuthorityAncestryValidationRunner
{
    private const string ExperimentName = "09-authority-ancestry-circular-standing";

    public static ValidationRunResult RunHoldout(string outputDirectory, bool quiet = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        var root = Path.GetFullPath(outputDirectory);
        Directory.CreateDirectory(root);
        var experiments = new[] { ExperimentCatalog.Get(ExperimentName) };
        var completedRuns = new List<RunResult>(ValidationPlan.AuthorityAncestryHoldoutSeeds.Count);
        foreach (var seed in ValidationPlan.AuthorityAncestryHoldoutSeeds)
        {
            completedRuns.Add(ExperimentRunner.Run(experiments, seed, Path.Combine(root, $"seed-{seed}"), quiet: true));
        }

        var replication = ReplicationRunner.CreateReport(experiments, completedRuns);
        var validation = ValidationReportBuilder.Create(completedRuns);
        ArtifactWriter.WriteReplication(replication, root);
        ArtifactWriter.WriteValidation(validation, root);
        ArtifactWriter.WriteSessionManifest(
            root,
            ValidationPlan.AuthorityAncestryHoldoutSeeds,
            ValidationPlan.AuthorityAncestryHoldoutSeeds,
            [ExperimentName],
            "completed",
            null);
        if (!quiet)
        {
            var protocol = validation.Protocols.Single();
            Console.WriteLine($"Protocol 09 holdout: {validation.SeedSet}; {validation.Seeds.Count} seed(s).");
            Console.WriteLine($"{protocol.Experiment}: support={protocol.Support}, mixed={protocol.Mixed}, disconfirm={protocol.Disconfirm}, inconclusive={protocol.Inconclusive}");
        }

        return new ValidationRunResult(replication, validation);
    }
}
