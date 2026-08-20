namespace Cpa.BoundedMindsLab.Experiments;

public static class ExperimentCatalog
{
    private static readonly IExperiment[] Experiments =
    [
        new LocalSharedMemoryContaminationExperiment(),
        new PeerDisagreementPreservedInteriorsExperiment(),
        new DevelopmentalVersusDoctrinalTransferExperiment(),
        new BoundedCommunicationBeforeLanguageExperiment(),
        new EmergentConventionArtificialCultureExperiment(),
        new IncompleteEpistemicAncestryExperiment(),
        new ProvisionalStandingTransferExperiment(),
        new StrategicPublicInfluenceExperiment(),
    ];

    public static IReadOnlyList<IExperiment> All => Experiments;

    public static IExperiment Get(string name)
    {
        var experiment = Experiments.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        return experiment ?? throw new ArgumentException($"Unknown experiment '{name}'.", nameof(name));
    }
}
