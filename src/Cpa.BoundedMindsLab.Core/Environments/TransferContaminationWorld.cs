using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Environments;

public static class TransferContaminationWorld
{
    private static readonly double[] SourceTargets = [0.80, -0.75, 0.55, -0.45, 0.90, -0.85, 0.65, -0.60];
    private static readonly double[] ReceiverTargets = [0.80, -0.75, 0.55, -0.45, 0.90, -0.85, -0.55, 0.55];
    private static readonly int[] CompatibleCells = [0, 1, 2, 3, 4, 5];
    private static readonly int[] DivergentCells = [6, 7];

    public const int ContextCount = 8;
    public const int SourceRepetitionsPerContext = 80;
    public const int ReceiverRepetitionsPerContext = 60;

    public static IReadOnlyList<int> TransferCompatibleCells => CompatibleCells;

    public static IReadOnlyList<int> TransferDivergentCells => DivergentCells;

    public static double SourceTarget(int contextCell) => SourceTargets[ValidateCell(contextCell)];

    public static double ReceiverTarget(int contextCell) => ReceiverTargets[ValidateCell(contextCell)];

    public static bool IsTransferCompatible(int contextCell) => ValidateCell(contextCell) < 6;

    public static IReadOnlyList<int> CreateSourceSchedule(ulong seed) => CreateBalancedSchedule(
        SourceRepetitionsPerContext,
        seed ^ 0xA24BAED4963EE407UL);

    public static IReadOnlyList<int> CreateReceiverSchedule(ulong seed) => CreateBalancedSchedule(
        ReceiverRepetitionsPerContext,
        seed ^ 0x9FB21C651E98DF25UL);

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

    private static int ValidateCell(int contextCell)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(contextCell);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(contextCell, ContextCount);
        return contextCell;
    }
}
