using Cpa.BoundedMindsLab.Experiments;

namespace Cpa.BoundedMindsLab.Validation;

public static class ValidationPlan
{
    public const string DevelopmentSetName = "development-v1";
    public const string HoldoutSetName = "holdout-v1";
    public const string StrategicInfluenceHoldoutSetName = "p08-holdout-v1";
    public const string AuthorityAncestryHoldoutSetName = "p09-holdout-v1";
    public const string CustomSetName = "custom";

    public static IReadOnlyList<string> FrozenProtocolNames { get; } =
    [
        "01-local-shared-memory-contamination",
        "02-peer-disagreement-preserved-interiors",
        "03-developmental-versus-doctrinal-transfer",
        "04-bounded-communication-before-language",
        "05-emergent-convention-artificial-culture",
        "06-incomplete-epistemic-ancestry",
        "07-provisional-standing-transfer",
    ];

    public static IReadOnlyList<ulong> DevelopmentSeeds => ExperimentDefaults.DevelopmentSeeds;

    public static IReadOnlyList<ulong> HoldoutSeeds => ExperimentDefaults.HoldoutSeeds;

    public static IReadOnlyList<ulong> StrategicInfluenceHoldoutSeeds => ExperimentDefaults.StrategicInfluenceHoldoutSeeds;

    public static IReadOnlyList<ulong> AuthorityAncestryHoldoutSeeds => ExperimentDefaults.AuthorityAncestryHoldoutSeeds;

    public static string ClassifySeedSet(IReadOnlyList<ulong> seeds)
    {
        ArgumentNullException.ThrowIfNull(seeds);
        return SameSet(seeds, DevelopmentSeeds)
            ? DevelopmentSetName
            : SameSet(seeds, HoldoutSeeds)
                ? HoldoutSetName
                : SameSet(seeds, StrategicInfluenceHoldoutSeeds)
                    ? StrategicInfluenceHoldoutSetName
                    : SameSet(seeds, AuthorityAncestryHoldoutSeeds)
                        ? AuthorityAncestryHoldoutSetName
                        : CustomSetName;
    }

    public static bool IsFullFrozenProtocolSet(IReadOnlyList<string> experimentNames)
    {
        ArgumentNullException.ThrowIfNull(experimentNames);
        var expected = FrozenProtocolNames.ToHashSet(StringComparer.Ordinal);
        return experimentNames.Count == expected.Count && experimentNames.All(expected.Contains);
    }

    private static bool SameSet(IReadOnlyList<ulong> left, IReadOnlyList<ulong> right)
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        return left.ToHashSet().SetEquals(right);
    }
}
