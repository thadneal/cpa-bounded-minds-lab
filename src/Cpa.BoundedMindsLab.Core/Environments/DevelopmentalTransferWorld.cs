using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Environments;

public enum SourceHistoryKind
{
    StableCompatible = 0,
    StableDivergent = 1,
    UnstableTransition = 2,
    SparseAmbiguous = 3,
}

public sealed record DevelopmentalTransferCell(
    int ContextCell,
    SourceHistoryKind HistoryKind,
    double ReceiverTarget,
    double ReceiverNoiseAmplitude,
    int SourceEvidenceCount,
    double SourceEarlyTarget,
    double SourceLateTarget,
    double SourceNoiseAmplitude);

public sealed record DevelopmentalTransferScenario(
    ulong Seed,
    IReadOnlyList<DevelopmentalTransferCell> Cells,
    ulong Fingerprint);

public readonly record struct DevelopmentalTransferObservation(int ContextCell, double Target);

public static class DevelopmentalTransferWorld
{
    private const int ReceiverRepetitionsPerContext = 28;
    private const ulong ScenarioSeedMask = 0xA0761D6478BD642FUL;
    private const ulong SourceScheduleSeedMask = 0x8EBC6AF09C88C6E3UL;
    private const ulong SourceNoiseSeedMask = 0xE7037ED1A0B428DBUL;
    private const ulong ReceiverScheduleSeedMask = 0x589965CC75374CC3UL;
    private const ulong ReceiverNoiseSeedMask = 0x1D8E4E27C47D124FUL;

    public const int ContextCount = 10;

    public static DevelopmentalTransferScenario CreateScenario(ulong seed)
    {
        var random = new DeterministicRandom(seed ^ ScenarioSeedMask);
        var historyKinds = new List<SourceHistoryKind>
        {
            SourceHistoryKind.StableCompatible,
            SourceHistoryKind.StableCompatible,
            SourceHistoryKind.StableCompatible,
            SourceHistoryKind.StableDivergent,
            SourceHistoryKind.StableDivergent,
            SourceHistoryKind.UnstableTransition,
            SourceHistoryKind.UnstableTransition,
            SourceHistoryKind.UnstableTransition,
            SourceHistoryKind.SparseAmbiguous,
            (SourceHistoryKind)random.NextInt(4),
        };
        random.Shuffle(historyKinds);

        var cells = new List<DevelopmentalTransferCell>(ContextCount);
        for (var contextCell = 0; contextCell < ContextCount; contextCell++)
        {
            var historyKind = historyKinds[contextCell];
            var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
            var receiverTarget = sign * (0.35 + (0.50 * random.NextUnit()));
            var receiverNoise = 0.015 + (0.035 * random.NextUnit());

            int sourceEvidenceCount;
            double earlyTarget;
            double lateTarget;
            double sourceNoise;
            switch (historyKind)
            {
                case SourceHistoryKind.StableCompatible:
                    sourceEvidenceCount = 28 + random.NextInt(29);
                    earlyTarget = Math.Clamp(receiverTarget + Symmetric(random, 0.035), -1.0, 1.0);
                    lateTarget = earlyTarget;
                    sourceNoise = 0.020 + (0.035 * random.NextUnit());
                    break;
                case SourceHistoryKind.StableDivergent:
                    sourceEvidenceCount = 28 + random.NextInt(29);
                    earlyTarget = Math.Clamp((-0.55 * receiverTarget) + Symmetric(random, 0.090), -1.0, 1.0);
                    lateTarget = earlyTarget;
                    sourceNoise = 0.020 + (0.040 * random.NextUnit());
                    break;
                case SourceHistoryKind.UnstableTransition:
                    sourceEvidenceCount = 24 + random.NextInt(33);
                    var compatibleTarget = Math.Clamp(receiverTarget + Symmetric(random, 0.050), -1.0, 1.0);
                    var divergentTarget = Math.Clamp((-0.50 * receiverTarget) + Symmetric(random, 0.120), -1.0, 1.0);
                    if (random.NextInt(2) == 0)
                    {
                        earlyTarget = compatibleTarget;
                        lateTarget = divergentTarget;
                    }
                    else
                    {
                        earlyTarget = divergentTarget;
                        lateTarget = compatibleTarget;
                    }

                    sourceNoise = 0.030 + (0.055 * random.NextUnit());
                    break;
                case SourceHistoryKind.SparseAmbiguous:
                    sourceEvidenceCount = 4 + random.NextInt(8);
                    earlyTarget = Math.Clamp(receiverTarget + Symmetric(random, 0.450), -1.0, 1.0);
                    lateTarget = earlyTarget;
                    sourceNoise = 0.080 + (0.120 * random.NextUnit());
                    break;
                default:
                    throw new InvalidOperationException($"Unknown source history kind {historyKind}.");
            }

            cells.Add(new DevelopmentalTransferCell(
                contextCell,
                historyKind,
                receiverTarget,
                receiverNoise,
                sourceEvidenceCount,
                earlyTarget,
                lateTarget,
                sourceNoise));
        }

        return new DevelopmentalTransferScenario(seed, cells, ComputeFingerprint(cells));
    }

    public static DevelopmentalTransferObservation[] CreateSourceObservations(DevelopmentalTransferScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var schedule = new List<int>();
        for (var contextCell = 0; contextCell < scenario.Cells.Count; contextCell++)
        {
            var cell = scenario.Cells[contextCell];
            for (var occurrence = 0; occurrence < cell.SourceEvidenceCount; occurrence++)
            {
                schedule.Add(contextCell);
            }
        }

        new DeterministicRandom(scenario.Seed ^ SourceScheduleSeedMask).Shuffle(schedule);
        var random = new DeterministicRandom(scenario.Seed ^ SourceNoiseSeedMask);
        var seen = new int[scenario.Cells.Count];
        var observations = new DevelopmentalTransferObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var contextCell = schedule[index];
            var cell = scenario.Cells[contextCell];
            var ordinal = seen[contextCell]++;
            var progress = cell.SourceEvidenceCount <= 1
                ? 1.0
                : (double)ordinal / (cell.SourceEvidenceCount - 1);
            var baseTarget = progress < 0.5 ? cell.SourceEarlyTarget : cell.SourceLateTarget;
            var target = Math.Clamp(baseTarget + Symmetric(random, cell.SourceNoiseAmplitude), -1.0, 1.0);
            observations[index] = new DevelopmentalTransferObservation(contextCell, target);
        }

        return observations;
    }

    public static DevelopmentalTransferObservation[] CreateReceiverObservations(DevelopmentalTransferScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var schedule = new List<int>(scenario.Cells.Count * ReceiverRepetitionsPerContext);
        for (var repetition = 0; repetition < ReceiverRepetitionsPerContext; repetition++)
        {
            for (var contextCell = 0; contextCell < scenario.Cells.Count; contextCell++)
            {
                schedule.Add(contextCell);
            }
        }

        new DeterministicRandom(scenario.Seed ^ ReceiverScheduleSeedMask).Shuffle(schedule);
        var random = new DeterministicRandom(scenario.Seed ^ ReceiverNoiseSeedMask);
        var observations = new DevelopmentalTransferObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var contextCell = schedule[index];
            var cell = scenario.Cells[contextCell];
            var target = Math.Clamp(
                cell.ReceiverTarget + Symmetric(random, cell.ReceiverNoiseAmplitude),
                -1.0,
                1.0);
            observations[index] = new DevelopmentalTransferObservation(contextCell, target);
        }

        return observations;
    }

    public static bool IsStableCompatible(SourceHistoryKind historyKind) => historyKind == SourceHistoryKind.StableCompatible;

    public static bool IsUnstable(SourceHistoryKind historyKind) => historyKind == SourceHistoryKind.UnstableTransition;

    private static double Symmetric(DeterministicRandom random, double amplitude) =>
        ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private static ulong ComputeFingerprint(List<DevelopmentalTransferCell> cells)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;
        foreach (var cell in cells)
        {
            hash ^= (ulong)cell.HistoryKind + 1UL;
            hash *= prime;
            hash ^= (ulong)cell.SourceEvidenceCount;
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.ReceiverTarget));
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.SourceEarlyTarget));
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.SourceLateTarget));
            hash *= prime;
        }

        return hash;
    }
}
