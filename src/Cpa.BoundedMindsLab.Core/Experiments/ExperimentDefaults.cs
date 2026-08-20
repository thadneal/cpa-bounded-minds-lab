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


    // Registered in v0.12.0 for Protocol 08 only. These seeds were not used during Protocol 08 development
    // and must be treated as consumed after the first strategic-influence holdout run.
    public static IReadOnlyList<ulong> StrategicInfluenceHoldoutSeeds { get; } =
    [
        41047UL,
        42131UL,
        43391UL,
        44621UL,
        45893UL,
        47237UL,
        48611UL,
        49919UL,
        51307UL,
        52709UL,
        54139UL,
        55603UL,
        57143UL,
        58661UL,
        60209UL,
        61781UL,
        63347UL,
        64997UL,
        66617UL,
        68213UL,
    ];

    // Registered in v0.14.0 for frozen Protocol 09 only. These seeds were selected before any Protocol 09 holdout outcomes were executed or inspected.
    // After the first --p09-validation execution, this set is consumed and reruns are reproducibility only.
    public static IReadOnlyList<ulong> AuthorityAncestryHoldoutSeeds { get; } =
    [
        70111UL,
        71429UL,
        72817UL,
        74209UL,
        75679UL,
        77101UL,
        78593UL,
        80021UL,
        81517UL,
        83003UL,
        84521UL,
        86011UL,
        87539UL,
        89051UL,
        90617UL,
        92141UL,
        93703UL,
        95279UL,
        96821UL,
        98411UL,
    ];

    // Compatibility alias for older tooling and documentation. In v0.8+ this set is explicitly developmental, not holdout validation.
    public static IReadOnlyList<ulong> ReplicationSeeds => DevelopmentSeeds;
}
