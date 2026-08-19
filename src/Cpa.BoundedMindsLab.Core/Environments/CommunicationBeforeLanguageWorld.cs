using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Environments;

public enum CommunicationHistoryKind
{
    InformativeDissent = 0,
    MisleadingDissent = 1,
    Complementary = 2,
    Convergent = 3,
}

public sealed record CommunicationPeerHistory(
    int EvidenceCount,
    double PrivateTarget,
    double NoiseAmplitude);

public sealed record CommunicationScenarioCell(
    int ContextCell,
    CommunicationHistoryKind HistoryKind,
    double SharedTarget,
    int SalientPeerIndex,
    CommunicationPeerHistory[] PeerHistories,
    double SharedNoiseAmplitude);

public sealed record CommunicationBeforeLanguageScenario(
    ulong Seed,
    CommunicationScenarioCell[] Cells,
    ulong Fingerprint);

public sealed record CommunicationPrivateObservation(int ContextCell, double Target);

public sealed record CommunicationSharedObservation(int ContextCell, double Target);

public static class CommunicationBeforeLanguageWorld
{
    public const int ContextCount = 12;
    public const int PeerCount = 3;
    public const int SharedRepetitionsPerContext = 28;

    private const ulong ScenarioSeedMask = 0x4B0A7D11C0FFEE21UL;
    private const ulong PrivateScheduleSeedMask = 0x1A2B3C4D55667788UL;
    private const ulong PrivateNoiseSeedMask = 0x8899AABBCCDDEEFFUL;
    private const ulong SharedScheduleSeedMask = 0x1029384756ABCDEFUL;
    private const ulong SharedNoiseSeedMask = 0xFEDCBA6547382910UL;
    private const ulong PeerSalt = 0x9E3779B97F4A7C15UL;

    public static CommunicationBeforeLanguageScenario CreateScenario(ulong seed)
    {
        var random = new DeterministicRandom(seed ^ ScenarioSeedMask);
        var counts = new[] { 2, 2, 2, 2 };
        for (var extra = 0; extra < ContextCount - 8; extra++)
        {
            var eligible = new List<int>(4);
            for (var kindIndex = 0; kindIndex < counts.Length; kindIndex++)
            {
                if (counts[kindIndex] < 5)
                {
                    eligible.Add(kindIndex);
                }
            }

            counts[eligible[random.NextInt(eligible.Count)]]++;
        }

        var kinds = new List<CommunicationHistoryKind>(ContextCount);
        for (var kindIndex = 0; kindIndex < counts.Length; kindIndex++)
        {
            for (var occurrence = 0; occurrence < counts[kindIndex]; occurrence++)
            {
                kinds.Add((CommunicationHistoryKind)kindIndex);
            }
        }

        random.Shuffle(kinds);
        var cells = new CommunicationScenarioCell[ContextCount];
        for (var contextCell = 0; contextCell < ContextCount; contextCell++)
        {
            var historyKind = kinds[contextCell];
            var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
            var sharedTarget = sign * (0.35 + (0.45 * random.NextUnit()));
            var salientPeerIndex = random.NextInt(PeerCount);
            var histories = CreatePeerHistories(random, historyKind, sharedTarget, salientPeerIndex);
            var sharedNoise = 0.015 + (0.030 * random.NextUnit());
            cells[contextCell] = new CommunicationScenarioCell(
                contextCell,
                historyKind,
                sharedTarget,
                salientPeerIndex,
                histories,
                sharedNoise);
        }

        return new CommunicationBeforeLanguageScenario(seed, cells, ComputeFingerprint(cells));
    }

    public static CommunicationPrivateObservation[] CreatePrivateObservations(
        CommunicationBeforeLanguageScenario scenario,
        int peerIndex)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentOutOfRangeException.ThrowIfNegative(peerIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(peerIndex, PeerCount);

        var schedule = new List<int>();
        for (var contextCell = 0; contextCell < scenario.Cells.Length; contextCell++)
        {
            var evidenceCount = scenario.Cells[contextCell].PeerHistories[peerIndex].EvidenceCount;
            for (var occurrence = 0; occurrence < evidenceCount; occurrence++)
            {
                schedule.Add(contextCell);
            }
        }

        var peerSeed = (ulong)(peerIndex + 1) * PeerSalt;
        new DeterministicRandom(scenario.Seed ^ PrivateScheduleSeedMask ^ peerSeed).Shuffle(schedule);
        var random = new DeterministicRandom(scenario.Seed ^ PrivateNoiseSeedMask ^ peerSeed);
        var observations = new CommunicationPrivateObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var contextCell = schedule[index];
            var history = scenario.Cells[contextCell].PeerHistories[peerIndex];
            var target = Math.Clamp(
                history.PrivateTarget + Symmetric(random, history.NoiseAmplitude),
                -1.0,
                1.0);
            observations[index] = new CommunicationPrivateObservation(contextCell, target);
        }

        return observations;
    }

    public static CommunicationSharedObservation[] CreateSharedObservations(CommunicationBeforeLanguageScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var schedule = new List<int>(scenario.Cells.Length * SharedRepetitionsPerContext);
        for (var repetition = 0; repetition < SharedRepetitionsPerContext; repetition++)
        {
            for (var contextCell = 0; contextCell < scenario.Cells.Length; contextCell++)
            {
                schedule.Add(contextCell);
            }
        }

        new DeterministicRandom(scenario.Seed ^ SharedScheduleSeedMask).Shuffle(schedule);
        var random = new DeterministicRandom(scenario.Seed ^ SharedNoiseSeedMask);
        var observations = new CommunicationSharedObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var contextCell = schedule[index];
            var cell = scenario.Cells[contextCell];
            var target = Math.Clamp(
                cell.SharedTarget + Symmetric(random, cell.SharedNoiseAmplitude),
                -1.0,
                1.0);
            observations[index] = new CommunicationSharedObservation(contextCell, target);
        }

        return observations;
    }

    public static int CountKind(CommunicationBeforeLanguageScenario scenario, CommunicationHistoryKind kind)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var count = 0;
        for (var index = 0; index < scenario.Cells.Length; index++)
        {
            if (scenario.Cells[index].HistoryKind == kind)
            {
                count++;
            }
        }

        return count;
    }

    private static CommunicationPeerHistory[] CreatePeerHistories(
        DeterministicRandom random,
        CommunicationHistoryKind historyKind,
        double sharedTarget,
        int salientPeerIndex)
    {
        var histories = new CommunicationPeerHistory[PeerCount];
        switch (historyKind)
        {
            case CommunicationHistoryKind.InformativeDissent:
            {
                var biasDirection = sharedTarget > 0.0 ? -1.0 : 1.0;
                var majorityTarget = Math.Clamp(
                    sharedTarget + (biasDirection * (0.32 + (0.28 * random.NextUnit()))),
                    -1.0,
                    1.0);
                for (var peerIndex = 0; peerIndex < PeerCount; peerIndex++)
                {
                    histories[peerIndex] = peerIndex == salientPeerIndex
                        ? new CommunicationPeerHistory(
                            38 + random.NextInt(23),
                            Math.Clamp(sharedTarget + Symmetric(random, 0.035), -1.0, 1.0),
                            0.020 + (0.025 * random.NextUnit()))
                        : new CommunicationPeerHistory(
                            18 + random.NextInt(17),
                            Math.Clamp(majorityTarget + Symmetric(random, 0.040), -1.0, 1.0),
                            0.030 + (0.035 * random.NextUnit()));
                }

                break;
            }

            case CommunicationHistoryKind.MisleadingDissent:
            {
                var biasDirection = sharedTarget > 0.0 ? -1.0 : 1.0;
                var dissentTarget = Math.Clamp(
                    sharedTarget + (biasDirection * (0.35 + (0.35 * random.NextUnit()))),
                    -1.0,
                    1.0);
                for (var peerIndex = 0; peerIndex < PeerCount; peerIndex++)
                {
                    histories[peerIndex] = peerIndex == salientPeerIndex
                        ? new CommunicationPeerHistory(
                            5 + random.NextInt(8),
                            Math.Clamp(dissentTarget + Symmetric(random, 0.080), -1.0, 1.0),
                            0.090 + (0.080 * random.NextUnit()))
                        : new CommunicationPeerHistory(
                            30 + random.NextInt(23),
                            Math.Clamp(sharedTarget + Symmetric(random, 0.045), -1.0, 1.0),
                            0.025 + (0.035 * random.NextUnit()));
                }

                break;
            }

            case CommunicationHistoryKind.Complementary:
                for (var peerIndex = 0; peerIndex < PeerCount; peerIndex++)
                {
                    if (peerIndex == salientPeerIndex)
                    {
                        histories[peerIndex] = new CommunicationPeerHistory(
                            28 + random.NextInt(25),
                            Math.Clamp(sharedTarget + Symmetric(random, 0.050), -1.0, 1.0),
                            0.030 + (0.040 * random.NextUnit()));
                        continue;
                    }

                    var direction = random.NextInt(2) == 0 ? -1.0 : 1.0;
                    histories[peerIndex] = new CommunicationPeerHistory(
                        12 + random.NextInt(22),
                        Math.Clamp(
                            sharedTarget + (direction * (0.12 + (0.24 * random.NextUnit()))),
                            -1.0,
                            1.0),
                        0.050 + (0.060 * random.NextUnit()));
                }

                break;

            case CommunicationHistoryKind.Convergent:
                for (var peerIndex = 0; peerIndex < PeerCount; peerIndex++)
                {
                    histories[peerIndex] = new CommunicationPeerHistory(
                        22 + random.NextInt(28),
                        Math.Clamp(sharedTarget + Symmetric(random, 0.065), -1.0, 1.0),
                        0.025 + (0.040 * random.NextUnit()));
                }

                break;

            default:
                throw new InvalidOperationException($"Unknown communication history kind {historyKind}.");
        }

        return histories;
    }

    private static double Symmetric(DeterministicRandom random, double amplitude) =>
        ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private static ulong ComputeFingerprint(CommunicationScenarioCell[] cells)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;
        for (var cellIndex = 0; cellIndex < cells.Length; cellIndex++)
        {
            var cell = cells[cellIndex];
            hash ^= (ulong)cell.HistoryKind + 1UL;
            hash *= prime;
            hash ^= (ulong)cell.SalientPeerIndex + 1UL;
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.SharedTarget));
            hash *= prime;
            for (var peerIndex = 0; peerIndex < cell.PeerHistories.Length; peerIndex++)
            {
                var history = cell.PeerHistories[peerIndex];
                hash ^= (ulong)history.EvidenceCount;
                hash *= prime;
                hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(history.PrivateTarget));
                hash *= prime;
            }
        }

        return hash;
    }
}
