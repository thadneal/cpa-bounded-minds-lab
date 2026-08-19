using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Environments;

public readonly record struct PeerObservation(int ContextCell, double Target);

public static class PeerDisagreementWorld
{
    private static readonly double[] SharedTargets = [0.78, -0.66, 0.52, -0.38, 0.88, -0.82, 0.61, -0.57];
    private static readonly int[] PeerAAccurateCells = [0, 2, 5, 7];

    public const int ContextCount = 8;
    public const int PrivateRepetitionsPerContext = 48;
    public const int SharedRepetitionsPerContext = 28;

    public static double SharedTarget(int contextCell) => SharedTargets[ValidateCell(contextCell)];

    public static bool PeerAHasBetterPrivateView(int contextCell) => Array.BinarySearch(PeerAAccurateCells, ValidateCell(contextCell)) >= 0;

    public static PeerObservation[] CreatePrivateObservations(ulong seed, bool peerA)
    {
        var schedule = CreateBalancedSchedule(
            PrivateRepetitionsPerContext,
            seed ^ (peerA ? 0x8CB92BA72F3D8DD7UL : 0x58F38DED6D9A3E51UL));
        var random = new DeterministicRandom(seed ^ (peerA ? 0xD1B54A32D192ED03UL : 0xABC98388FB8FAC03UL));
        var observations = new PeerObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var cell = schedule[index];
            var target = PrivateTarget(cell, peerA) + SymmetricNoise(random, 0.055);
            observations[index] = new PeerObservation(cell, Math.Clamp(target, -1.0, 1.0));
        }

        return observations;
    }

    public static PeerObservation[] CreateSharedObservations(ulong seed)
    {
        var schedule = CreateBalancedSchedule(
            SharedRepetitionsPerContext,
            seed ^ 0x9E3779B97F4A7C15UL);
        var random = new DeterministicRandom(seed ^ 0xC6BC279692B5CC83UL);
        var observations = new PeerObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var cell = schedule[index];
            var target = SharedTarget(cell) + SymmetricNoise(random, 0.025);
            observations[index] = new PeerObservation(cell, Math.Clamp(target, -1.0, 1.0));
        }

        return observations;
    }

    private static double PrivateTarget(int contextCell, bool peerA)
    {
        var validatedCell = ValidateCell(contextCell);
        var accurate = PeerAHasBetterPrivateView(validatedCell) == peerA;
        if (accurate)
        {
            return SharedTargets[validatedCell];
        }

        var offset = validatedCell % 2 == 0 ? 0.08 : -0.08;
        return Math.Clamp((-0.55 * SharedTargets[validatedCell]) + offset, -1.0, 1.0);
    }

    private static List<int> CreateBalancedSchedule(int repetitionsPerContext, ulong seed)
    {
        var values = new List<int>(ContextCount * repetitionsPerContext);
        for (var repetition = 0; repetition < repetitionsPerContext; repetition++)
        {
            for (var contextCell = 0; contextCell < ContextCount; contextCell++)
            {
                values.Add(contextCell);
            }
        }

        new DeterministicRandom(seed).Shuffle(values);
        return values;
    }

    private static double SymmetricNoise(DeterministicRandom random, double amplitude) =>
        ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private static int ValidateCell(int contextCell)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(contextCell);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(contextCell, ContextCount);
        return contextCell;
    }
}
