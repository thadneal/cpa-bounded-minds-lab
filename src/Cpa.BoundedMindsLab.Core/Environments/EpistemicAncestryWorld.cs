using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Environments;

public enum AncestryContextKind
{
    EchoTrap = 0,
    IndependentConvergence = 1,
    MixedLineage = 2,
    AmbiguousLineage = 3,
}

public enum AncestryHintKind
{
    Missing = 0,
    RootPreserved = 1,
    ImmediateSender = 2,
}

public sealed record AncestrySignature(double A, double B, double C);

public sealed record AncestryRoot(
    string RootId,
    double Estimate,
    double Standing,
    int EvidenceCount,
    AncestrySignature Signature);

public sealed record AncestryReport(
    string SenderMindId,
    string TrueRootId,
    string? OriginHint,
    AncestryHintKind HintKind,
    double Estimate,
    double Standing,
    int EvidenceCount,
    AncestrySignature Signature);

public sealed record EpistemicAncestryCell(
    int ContextCell,
    AncestryContextKind ContextKind,
    double Target,
    AncestryRoot[] Roots,
    AncestryReport[] Reports);

public sealed record EpistemicAncestryScenario(
    ulong Seed,
    EpistemicAncestryCell[] Cells,
    ulong Fingerprint);

public static class EpistemicAncestryWorld
{
    public const int ContextCount = 14;
    public const int PeerCount = 7;

    private const ulong ScenarioSeedMask = 0x6A6A6A6A12345678UL;
    private const ulong ContextSalt = 0x9E3779B97F4A7C15UL;

    public static EpistemicAncestryScenario CreateScenario(ulong seed)
    {
        var random = new DeterministicRandom(seed ^ ScenarioSeedMask);
        var kinds = new List<AncestryContextKind>(ContextCount)
        {
            AncestryContextKind.EchoTrap,
            AncestryContextKind.EchoTrap,
            AncestryContextKind.EchoTrap,
            AncestryContextKind.IndependentConvergence,
            AncestryContextKind.IndependentConvergence,
            AncestryContextKind.IndependentConvergence,
            AncestryContextKind.MixedLineage,
            AncestryContextKind.MixedLineage,
            AncestryContextKind.MixedLineage,
            AncestryContextKind.AmbiguousLineage,
            AncestryContextKind.AmbiguousLineage,
            AncestryContextKind.AmbiguousLineage,
        };

        for (var extra = kinds.Count; extra < ContextCount; extra++)
        {
            kinds.Add((AncestryContextKind)random.NextInt(4));
        }

        random.Shuffle(kinds);
        var cells = new EpistemicAncestryCell[ContextCount];
        for (var contextCell = 0; contextCell < ContextCount; contextCell++)
        {
            cells[contextCell] = CreateCell(seed, contextCell, kinds[contextCell]);
        }

        return new EpistemicAncestryScenario(seed, cells, ComputeFingerprint(cells));
    }

    public static int CountKind(EpistemicAncestryScenario scenario, AncestryContextKind kind)
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

    public static double MissingOriginRate(EpistemicAncestryScenario scenario) =>
        HintRate(scenario, AncestryHintKind.Missing);

    public static double ImmediateSenderHintRate(EpistemicAncestryScenario scenario) =>
        HintRate(scenario, AncestryHintKind.ImmediateSender);

    private static EpistemicAncestryCell CreateCell(ulong seed, int contextCell, AncestryContextKind kind)
    {
        var contextSeed = unchecked(seed ^ (ContextSalt * (ulong)(contextCell + 1)) ^ 0xABCDEF1234000000UL);
        var random = new DeterministicRandom(contextSeed);
        var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
        var target = sign * (0.25 + (0.55 * random.NextUnit()));
        var rootCount = kind switch
        {
            AncestryContextKind.EchoTrap => 2 + (random.NextUnit() < 0.35 ? 1 : 0),
            AncestryContextKind.IndependentConvergence => 3 + random.NextInt(3),
            _ => 3,
        };
        var signatures = CreateRootSignatures(random, rootCount, kind == AncestryContextKind.AmbiguousLineage);
        var roots = CreateRoots(random, contextCell, kind, target, signatures);
        var reportRootIndexes = CreateReportRootIndexes(random, kind, roots.Length);
        var reports = new AncestryReport[PeerCount];
        for (var peerIndex = 0; peerIndex < reports.Length; peerIndex++)
        {
            var rootIndex = reportRootIndexes[peerIndex];
            var root = roots[rootIndex];
            var distortion = kind == AncestryContextKind.AmbiguousLineage ? 0.12 : 0.045;
            var estimateDistortion = kind == AncestryContextKind.AmbiguousLineage ? 0.035 : 0.018;
            var signature = new AncestrySignature(
                Clamp01(root.Signature.A + Symmetric(random, distortion)),
                Clamp01(root.Signature.B + Symmetric(random, distortion)),
                Clamp01(root.Signature.C + Symmetric(random, distortion)));
            var estimate = Math.Clamp(root.Estimate + Symmetric(random, estimateDistortion), -1.0, 1.0);
            var standing = Math.Clamp(root.Standing * (0.90 + (0.10 * random.NextUnit())), 0.0, 1.0);
            var hint = CreateOriginHint(random, contextCell, peerIndex, rootIndex, kind);
            reports[peerIndex] = new AncestryReport(
                $"peer-{peerIndex + 1}",
                root.RootId,
                hint.Value,
                hint.Kind,
                estimate,
                standing,
                root.EvidenceCount,
                signature);
        }

        return new EpistemicAncestryCell(contextCell, kind, target, roots, reports);
    }

    private static AncestryRoot[] CreateRoots(
        DeterministicRandom random,
        int contextCell,
        AncestryContextKind kind,
        double target,
        AncestrySignature[] signatures)
    {
        var roots = new AncestryRoot[signatures.Length];
        switch (kind)
        {
            case AncestryContextKind.EchoTrap:
            {
                var wrongDirection = target > 0.0 ? -1.0 : 1.0;
                roots[0] = new AncestryRoot(
                    RootId(contextCell, 0),
                    Math.Clamp(target + (wrongDirection * (0.48 + (0.28 * random.NextUnit()))), -1.0, 1.0),
                    0.72 + (0.12 * random.NextUnit()),
                    10 + random.NextInt(13),
                    signatures[0]);
                roots[1] = new AncestryRoot(
                    RootId(contextCell, 1),
                    Math.Clamp(target + Symmetric(random, 0.045), -1.0, 1.0),
                    0.86 + (0.10 * random.NextUnit()),
                    36 + random.NextInt(28),
                    signatures[1]);
                if (roots.Length == 3)
                {
                    roots[2] = new AncestryRoot(
                        RootId(contextCell, 2),
                        Math.Clamp(target + Symmetric(random, 0.10), -1.0, 1.0),
                        0.75 + (0.15 * random.NextUnit()),
                        22 + random.NextInt(25),
                        signatures[2]);
                }

                break;
            }

            case AncestryContextKind.IndependentConvergence:
                for (var rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    roots[rootIndex] = new AncestryRoot(
                        RootId(contextCell, rootIndex),
                        Math.Clamp(target + Symmetric(random, 0.07), -1.0, 1.0),
                        0.78 + (0.18 * random.NextUnit()),
                        22 + random.NextInt(37),
                        signatures[rootIndex]);
                }

                break;

            case AncestryContextKind.MixedLineage:
            {
                var wrongDirection = target > 0.0 ? -1.0 : 1.0;
                roots[0] = new AncestryRoot(
                    RootId(contextCell, 0),
                    Math.Clamp(target + Symmetric(random, 0.055), -1.0, 1.0),
                    0.87 + (0.08 * random.NextUnit()),
                    34 + random.NextInt(27),
                    signatures[0]);
                roots[1] = new AncestryRoot(
                    RootId(contextCell, 1),
                    Math.Clamp(target + (wrongDirection * (0.35 + (0.25 * random.NextUnit()))), -1.0, 1.0),
                    0.72 + (0.15 * random.NextUnit()),
                    14 + random.NextInt(20),
                    signatures[1]);
                roots[2] = new AncestryRoot(
                    RootId(contextCell, 2),
                    Math.Clamp(target + Symmetric(random, 0.18), -1.0, 1.0),
                    0.70 + (0.18 * random.NextUnit()),
                    15 + random.NextInt(30),
                    signatures[2]);
                break;
            }

            case AncestryContextKind.AmbiguousLineage:
            {
                var wrongDirection = target > 0.0 ? -1.0 : 1.0;
                roots[0] = new AncestryRoot(
                    RootId(contextCell, 0),
                    Math.Clamp(target + Symmetric(random, 0.09), -1.0, 1.0),
                    0.78 + (0.12 * random.NextUnit()),
                    20 + random.NextInt(25),
                    signatures[0]);
                roots[1] = new AncestryRoot(
                    RootId(contextCell, 1),
                    Math.Clamp(target + (wrongDirection * (0.25 + (0.25 * random.NextUnit()))), -1.0, 1.0),
                    0.68 + (0.15 * random.NextUnit()),
                    10 + random.NextInt(20),
                    signatures[1]);
                roots[2] = new AncestryRoot(
                    RootId(contextCell, 2),
                    Math.Clamp(target + Symmetric(random, 0.16), -1.0, 1.0),
                    0.70 + (0.15 * random.NextUnit()),
                    12 + random.NextInt(28),
                    signatures[2]);
                break;
            }
        }

        return roots;
    }

    private static int[] CreateReportRootIndexes(DeterministicRandom random, AncestryContextKind kind, int rootCount)
    {
        var counts = kind switch
        {
            AncestryContextKind.EchoTrap when rootCount == 2 => new[] { 4, 3 },
            AncestryContextKind.EchoTrap => new[] { 4, 2, 1 },
            AncestryContextKind.MixedLineage => random.NextUnit() < 0.5 ? new[] { 2, 4, 1 } : new[] { 3, 3, 1 },
            AncestryContextKind.AmbiguousLineage => new[] { 2, 3, 2 },
            _ => CreateIndependentCounts(random, rootCount),
        };

        var indexes = new List<int>(PeerCount);
        for (var rootIndex = 0; rootIndex < counts.Length; rootIndex++)
        {
            for (var occurrence = 0; occurrence < counts[rootIndex]; occurrence++)
            {
                indexes.Add(rootIndex);
            }
        }

        random.Shuffle(indexes);
        return indexes.ToArray();
    }

    private static int[] CreateIndependentCounts(DeterministicRandom random, int rootCount)
    {
        var counts = Enumerable.Repeat(1, rootCount).ToArray();
        for (var remaining = rootCount; remaining < PeerCount; remaining++)
        {
            counts[random.NextInt(rootCount)]++;
        }

        return counts;
    }

    private static AncestrySignature[] CreateRootSignatures(DeterministicRandom random, int count, bool ambiguous)
    {
        var signatures = new List<AncestrySignature>(count);
        var minimumDistance = ambiguous ? 0.18 : 0.32;
        var attempts = 0;
        while (signatures.Count < count)
        {
            attempts++;
            var candidate = new AncestrySignature(
                0.10 + (0.80 * random.NextUnit()),
                0.10 + (0.80 * random.NextUnit()),
                0.10 + (0.80 * random.NextUnit()));
            var separated = true;
            for (var index = 0; index < signatures.Count; index++)
            {
                if (SignatureDistance(candidate, signatures[index]) < minimumDistance)
                {
                    separated = false;
                    break;
                }
            }

            if (separated || signatures.Count == 0 || attempts >= 96)
            {
                signatures.Add(candidate);
                attempts = 0;
            }
        }

        return signatures.ToArray();
    }

    private static (string? Value, AncestryHintKind Kind) CreateOriginHint(
        DeterministicRandom random,
        int contextCell,
        int peerIndex,
        int rootIndex,
        AncestryContextKind kind)
    {
        var rootProbability = kind == AncestryContextKind.AmbiguousLineage ? 0.18 : 0.32;
        var immediateProbability = kind == AncestryContextKind.AmbiguousLineage ? 0.25 : 0.28;
        var draw = random.NextUnit();
        if (draw < rootProbability)
        {
            return ($"a{contextCell:x2}{rootIndex:x1}", AncestryHintKind.RootPreserved);
        }

        if (draw < rootProbability + immediateProbability)
        {
            return ($"s{contextCell:x2}{peerIndex:x1}", AncestryHintKind.ImmediateSender);
        }

        return (null, AncestryHintKind.Missing);
    }

    private static double HintRate(EpistemicAncestryScenario scenario, AncestryHintKind kind)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var total = 0;
        var matching = 0;
        for (var cellIndex = 0; cellIndex < scenario.Cells.Length; cellIndex++)
        {
            var reports = scenario.Cells[cellIndex].Reports;
            for (var reportIndex = 0; reportIndex < reports.Length; reportIndex++)
            {
                total++;
                if (reports[reportIndex].HintKind == kind)
                {
                    matching++;
                }
            }
        }

        return total == 0 ? 0.0 : (double)matching / total;
    }

    private static ulong ComputeFingerprint(EpistemicAncestryCell[] cells)
    {
        var hash = 1469598103934665603UL;
        for (var cellIndex = 0; cellIndex < cells.Length; cellIndex++)
        {
            var cell = cells[cellIndex];
            hash = Mix(hash, (ulong)cell.ContextKind + 1UL);
            hash = Mix(hash, (ulong)Math.Round((cell.Target + 1.0) * 1_000_000.0));
            hash = Mix(hash, (ulong)cell.Roots.Length);
            for (var reportIndex = 0; reportIndex < cell.Reports.Length; reportIndex++)
            {
                var report = cell.Reports[reportIndex];
                hash = Mix(hash, (ulong)report.HintKind + 1UL);
                hash = Mix(hash, (ulong)Math.Round((report.Estimate + 1.0) * 1_000_000.0));
                hash = Mix(hash, (ulong)Math.Round(report.Signature.A * 1_000_000.0));
                hash = Mix(hash, (ulong)Math.Round(report.Signature.B * 1_000_000.0));
                hash = Mix(hash, (ulong)Math.Round(report.Signature.C * 1_000_000.0));
            }
        }

        return hash;
    }

    private static ulong Mix(ulong hash, ulong value) => unchecked((hash ^ value) * 1099511628211UL);

    private static double SignatureDistance(AncestrySignature left, AncestrySignature right)
    {
        var a = left.A - right.A;
        var b = left.B - right.B;
        var c = left.C - right.C;
        return Math.Sqrt((a * a) + (b * b) + (c * c));
    }

    private static string RootId(int contextCell, int rootIndex) => $"root-{contextCell}-{rootIndex}";

    private static double Symmetric(DeterministicRandom random, double amplitude) =>
        ((2.0 * random.NextUnit()) - 1.0) * amplitude;

    private static double Clamp01(double value) => Math.Clamp(value, 0.0, 1.0);
}
