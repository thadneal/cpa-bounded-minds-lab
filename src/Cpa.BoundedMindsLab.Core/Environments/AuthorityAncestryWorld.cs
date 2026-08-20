using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Environments;

public enum AuthorityAncestryContextKind
{
    IndependentGrounding = 0,
    CircularAuthorityTrap = 1,
    MixedAuthority = 2,
    SparseGrounding = 3,
}

public sealed record AuthorityAncestryCell(
    int ContextCell,
    AuthorityAncestryContextKind ContextKind,
    double ReceiverTarget,
    double CandidateEstimate,
    double ReceiverNoiseAmplitude,
    int DirectRootCount,
    int RootOffset,
    double BaseDirectStanding,
    double PeerTrust);

public sealed record AuthorityAncestryScenario(
    ulong Seed,
    AuthorityAncestryCell[] Cells,
    ulong Fingerprint);

public readonly record struct AuthorityAncestryObservation(
    int ContextCell,
    double Target,
    int ContextExposure);

public static class AuthorityAncestryWorld
{
    private const int ReceiverRepetitionsPerContext = 26;
    private const ulong ScenarioSeedMask = 0x8F4D3B2A1907C6E5UL;
    private const ulong ReceiverScheduleSeedMask = 0xD1342543DE82EF95UL;
    private const ulong ReceiverNoiseSeedMask = 0x94D049BB133111EBUL;

    public const int ContextCount = 12;
    public const int PeerCount = 5;

    public static AuthorityAncestryScenario CreateScenario(ulong seed)
    {
        var random = new DeterministicRandom(seed ^ ScenarioSeedMask);
        var kinds = new List<AuthorityAncestryContextKind>
        {
            AuthorityAncestryContextKind.IndependentGrounding,
            AuthorityAncestryContextKind.IndependentGrounding,
            AuthorityAncestryContextKind.IndependentGrounding,
            AuthorityAncestryContextKind.IndependentGrounding,
            AuthorityAncestryContextKind.CircularAuthorityTrap,
            AuthorityAncestryContextKind.CircularAuthorityTrap,
            AuthorityAncestryContextKind.CircularAuthorityTrap,
            AuthorityAncestryContextKind.CircularAuthorityTrap,
            AuthorityAncestryContextKind.MixedAuthority,
            AuthorityAncestryContextKind.MixedAuthority,
            AuthorityAncestryContextKind.SparseGrounding,
            AuthorityAncestryContextKind.SparseGrounding,
        };
        random.Shuffle(kinds);

        var cells = new AuthorityAncestryCell[ContextCount];
        for (var contextCell = 0; contextCell < ContextCount; contextCell++)
        {
            var kind = kinds[contextCell];
            var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
            var target = sign * (0.35 + (0.50 * random.NextUnit()));
            var noise = 0.015 + (0.035 * random.NextUnit());
            var directRootCount = kind switch
            {
                AuthorityAncestryContextKind.IndependentGrounding => 3 + random.NextInt(2),
                AuthorityAncestryContextKind.MixedAuthority => 2,
                _ => 1,
            };
            var baseDirectStanding = kind switch
            {
                AuthorityAncestryContextKind.IndependentGrounding => 0.72 + (0.18 * random.NextUnit()),
                AuthorityAncestryContextKind.CircularAuthorityTrap => 0.22 + (0.10 * random.NextUnit()),
                AuthorityAncestryContextKind.MixedAuthority => 0.52 + (0.18 * random.NextUnit()),
                _ => 0.62 + (0.14 * random.NextUnit()),
            };
            var candidateEstimate = kind switch
            {
                AuthorityAncestryContextKind.IndependentGrounding =>
                    Math.Clamp(target + Symmetric(random, 0.07), -1.0, 1.0),
                AuthorityAncestryContextKind.CircularAuthorityTrap =>
                    Math.Clamp((-0.72 * target) + Symmetric(random, 0.10), -1.0, 1.0),
                AuthorityAncestryContextKind.MixedAuthority =>
                    Math.Clamp((0.35 * target) + Symmetric(random, 0.18), -1.0, 1.0),
                _ =>
                    Math.Clamp(target + Symmetric(random, 0.10), -1.0, 1.0),
            };

            cells[contextCell] = new AuthorityAncestryCell(
                contextCell,
                kind,
                target,
                candidateEstimate,
                noise,
                directRootCount,
                random.NextInt(PeerCount),
                baseDirectStanding,
                0.82 + (0.06 * random.NextUnit()));
        }

        return new AuthorityAncestryScenario(seed, cells, ComputeFingerprint(cells));
    }

    public static AuthorityAncestryObservation[] CreateReceiverObservations(AuthorityAncestryScenario scenario)
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
        var observations = new AuthorityAncestryObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var contextCell = schedule[index];
            var cell = scenario.Cells[contextCell];
            exposures[contextCell]++;
            observations[index] = new AuthorityAncestryObservation(
                contextCell,
                Math.Clamp(cell.ReceiverTarget + Symmetric(random, cell.ReceiverNoiseAmplitude), -1.0, 1.0),
                exposures[contextCell]);
        }

        return observations;
    }

    public static int CountKind(AuthorityAncestryScenario scenario, AuthorityAncestryContextKind kind)
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

    public static bool IsDirectRoot(AuthorityAncestryCell cell, int peerIndex)
    {
        ArgumentNullException.ThrowIfNull(cell);
        ArgumentOutOfRangeException.ThrowIfNegative(peerIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(peerIndex, PeerCount);

        for (var rank = 0; rank < cell.DirectRootCount; rank++)
        {
            if ((cell.RootOffset + (rank * 2)) % PeerCount == peerIndex)
            {
                return true;
            }
        }

        return false;
    }

    public static int DirectRootRank(AuthorityAncestryCell cell, int peerIndex)
    {
        ArgumentNullException.ThrowIfNull(cell);
        for (var rank = 0; rank < cell.DirectRootCount; rank++)
        {
            if ((cell.RootOffset + (rank * 2)) % PeerCount == peerIndex)
            {
                return rank;
            }
        }

        return -1;
    }

    private static double Symmetric(DeterministicRandom random, double amplitude) =>
        ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private static ulong ComputeFingerprint(AuthorityAncestryCell[] cells)
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
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.CandidateEstimate));
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.ReceiverNoiseAmplitude));
            hash *= prime;
            hash ^= (ulong)cell.DirectRootCount;
            hash *= prime;
            hash ^= (ulong)cell.RootOffset;
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.BaseDirectStanding));
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.PeerTrust));
            hash *= prime;
        }

        return hash;
    }
}
