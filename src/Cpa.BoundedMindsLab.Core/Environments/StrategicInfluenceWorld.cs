using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Environments;

public enum StrategicInfluenceContextKind
{
    Aligned = 0,
    Divergent = 1,
    Betrayal = 2,
    PartialAlignment = 3,
}

public sealed record StrategicInfluenceCell(
    int ContextCell,
    StrategicInfluenceContextKind ContextKind,
    double ReceiverTarget,
    double DivergentSenderObjective,
    double ReceiverNoiseAmplitude,
    double SenderEvidenceQuality);

public sealed record StrategicInfluenceScenario(
    ulong Seed,
    StrategicInfluenceCell[] Cells,
    ulong Fingerprint);

public readonly record struct StrategicInfluenceObservation(
    int ContextCell,
    double Target,
    int ContextExposure);

public static class StrategicInfluenceWorld
{
    private const int ReceiverRepetitionsPerContext = 30;
    private const int BetrayalAlignedExposureCount = 10;
    private const ulong ScenarioSeedMask = 0xA24BAED4963EE407UL;
    private const ulong ReceiverScheduleSeedMask = 0x9FB21C651E98DF25UL;
    private const ulong ReceiverNoiseSeedMask = 0xD6E8FEB86659FD93UL;

    public const int ContextCount = 12;

    public static StrategicInfluenceScenario CreateScenario(ulong seed)
    {
        var random = new DeterministicRandom(seed ^ ScenarioSeedMask);
        var kinds = new List<StrategicInfluenceContextKind>
        {
            StrategicInfluenceContextKind.Aligned,
            StrategicInfluenceContextKind.Aligned,
            StrategicInfluenceContextKind.Aligned,
            StrategicInfluenceContextKind.Aligned,
            StrategicInfluenceContextKind.Divergent,
            StrategicInfluenceContextKind.Divergent,
            StrategicInfluenceContextKind.Divergent,
            StrategicInfluenceContextKind.Divergent,
            StrategicInfluenceContextKind.Betrayal,
            StrategicInfluenceContextKind.Betrayal,
            RandomSupplementalKind(random),
            RandomSupplementalKind(random),
        };
        random.Shuffle(kinds);

        var cells = new StrategicInfluenceCell[ContextCount];
        for (var contextCell = 0; contextCell < ContextCount; contextCell++)
        {
            var kind = kinds[contextCell];
            var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
            var receiverTarget = sign * (0.35 + (0.50 * random.NextUnit()));
            var receiverNoise = 0.012 + (0.035 * random.NextUnit());
            var senderEvidenceQuality = 0.58 + (0.30 * random.NextUnit());
            var divergentObjective = kind switch
            {
                StrategicInfluenceContextKind.Aligned =>
                    Math.Clamp(receiverTarget + Symmetric(random, 0.06), -1.0, 1.0),
                StrategicInfluenceContextKind.Divergent =>
                    Math.Clamp((-0.70 * receiverTarget) + Symmetric(random, 0.10), -1.0, 1.0),
                StrategicInfluenceContextKind.Betrayal =>
                    Math.Clamp((-0.78 * receiverTarget) + Symmetric(random, 0.08), -1.0, 1.0),
                _ =>
                    Math.Clamp((0.25 * receiverTarget) + Symmetric(random, 0.18), -1.0, 1.0),
            };

            cells[contextCell] = new StrategicInfluenceCell(
                contextCell,
                kind,
                receiverTarget,
                divergentObjective,
                receiverNoise,
                senderEvidenceQuality);
        }

        return new StrategicInfluenceScenario(seed, cells, ComputeFingerprint(cells));
    }

    public static StrategicInfluenceObservation[] CreateReceiverObservations(StrategicInfluenceScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var schedule = new List<int>(scenario.Cells.Length * ReceiverRepetitionsPerContext);
        for (var repetition = 0; repetition < ReceiverRepetitionsPerContext; repetition++)
        {
            for (var contextCell = 0; contextCell < scenario.Cells.Length; contextCell++)
            {
                schedule.Add(contextCell);
            }
        }

        new DeterministicRandom(scenario.Seed ^ ReceiverScheduleSeedMask).Shuffle(schedule);
        var random = new DeterministicRandom(scenario.Seed ^ ReceiverNoiseSeedMask);
        var exposures = new int[scenario.Cells.Length];
        var observations = new StrategicInfluenceObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var contextCell = schedule[index];
            var cell = scenario.Cells[contextCell];
            exposures[contextCell]++;
            observations[index] = new StrategicInfluenceObservation(
                contextCell,
                Math.Clamp(cell.ReceiverTarget + Symmetric(random, cell.ReceiverNoiseAmplitude), -1.0, 1.0),
                exposures[contextCell]);
        }

        return observations;
    }

    public static double SenderObjective(StrategicInfluenceCell cell, int contextExposure)
    {
        ArgumentNullException.ThrowIfNull(cell);
        return cell.ContextKind == StrategicInfluenceContextKind.Betrayal && contextExposure <= BetrayalAlignedExposureCount
            ? cell.ReceiverTarget
            : cell.DivergentSenderObjective;
    }

    public static int CountKind(StrategicInfluenceScenario scenario, StrategicInfluenceContextKind kind)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var count = 0;
        for (var index = 0; index < scenario.Cells.Length; index++)
        {
            if (scenario.Cells[index].ContextKind == kind)
            {
                count++;
            }
        }

        return count;
    }

    private static StrategicInfluenceContextKind RandomSupplementalKind(DeterministicRandom random) => random.NextInt(3) switch
    {
        0 => StrategicInfluenceContextKind.Aligned,
        1 => StrategicInfluenceContextKind.Divergent,
        _ => StrategicInfluenceContextKind.PartialAlignment,
    };

    private static double Symmetric(DeterministicRandom random, double amplitude) =>
        ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private static ulong ComputeFingerprint(StrategicInfluenceCell[] cells)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;
        for (var index = 0; index < cells.Length; index++)
        {
            var cell = cells[index];
            hash ^= (ulong)cell.ContextKind + 1UL;
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.ReceiverTarget));
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.DivergentSenderObjective));
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.ReceiverNoiseAmplitude));
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.SenderEvidenceQuality));
            hash *= prime;
        }

        return hash;
    }
}
