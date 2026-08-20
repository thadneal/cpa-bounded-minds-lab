namespace Cpa.BoundedMindsLab.Experiments;

public static class ExperimentDefaults
{
    public static IReadOnlyList<ulong> DevelopmentSeeds { get; } = [101UL, 211UL, 307UL, 401UL, 503UL];

    public static IReadOnlyList<ulong> HoldoutSeeds { get; } =
    [
        809UL,
        977UL,
        1201UL,
        1429UL,
        1693UL,
        2017UL,
        2371UL,
        2741UL,
        3163UL,
        3581UL,
        4001UL,
        4441UL,
        4871UL,
        5303UL,
        5741UL,
        6211UL,
        6673UL,
        7121UL,
        7603UL,
        8089UL,
    ];

    // Compatibility alias for older tooling and documentation. In v0.8+ this set is explicitly developmental, not holdout validation.
    public static IReadOnlyList<ulong> ReplicationSeeds => DevelopmentSeeds;
}
