using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Environments;

public enum StandingTransferContextKind
{
    StrongTransferable = 0,
    StrongLocalMismatch = 1,
    WeakTransferable = 2,
    WeakLocalMismatch = 3,
}

public sealed record StandingTransferCell(
    int ContextCell,
    StandingTransferContextKind ContextKind,
    double ReceiverTarget,
    double SourceEstimate,
    double RecommenderStanding,
    int RecommenderEvidenceCount,
    double ReceiverNoiseAmplitude);

public sealed record StandingTransferScenario(
    ulong Seed,
    double RecommenderCredibility,
    StandingTransferCell[] Cells,
    ulong Fingerprint);

public readonly record struct StandingTransferObservation(int ContextCell, double Target);

public static class StandingTransferWorld
{
    private const int ReceiverRepetitionsPerContext = 30;
    private const ulong ScenarioSeedMask = 0xD1342543DE82EF95UL;
    private const ulong ReceiverScheduleSeedMask = 0x94D049BB133111EBUL;
    private const ulong ReceiverNoiseSeedMask = 0xBF58476D1CE4E5B9UL;

    public const int ContextCount = 12;

    public static StandingTransferScenario CreateScenario(ulong seed)
    {
        var random = new DeterministicRandom(seed ^ ScenarioSeedMask);
        var kinds = new List<StandingTransferContextKind>
        {
            StandingTransferContextKind.StrongTransferable,
            StandingTransferContextKind.StrongTransferable,
            StandingTransferContextKind.StrongTransferable,
            StandingTransferContextKind.StrongLocalMismatch,
            StandingTransferContextKind.StrongLocalMismatch,
            StandingTransferContextKind.StrongLocalMismatch,
            StandingTransferContextKind.WeakTransferable,
            StandingTransferContextKind.WeakTransferable,
            StandingTransferContextKind.WeakLocalMismatch,
            StandingTransferContextKind.WeakLocalMismatch,
            (StandingTransferContextKind)random.NextInt(4),
            (StandingTransferContextKind)random.NextInt(4),
        };
        random.Shuffle(kinds);

        // This is C's already-earned standing for recommender A. It is part of C's
        // local social history and varies by seed; the inherited-authority control
        // deliberately ignores this local calibration when copying A's standing for B.
        var recommenderCredibility = 0.68 + (0.28 * random.NextUnit());
        var cells = new StandingTransferCell[ContextCount];
        for (var contextCell = 0; contextCell < ContextCount; contextCell++)
        {
            var kind = kinds[contextCell];
            var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
            var receiverTarget = sign * (0.35 + (0.50 * random.NextUnit()));
            var receiverNoise = 0.015 + (0.035 * random.NextUnit());

            int recommenderEvidence;
            double recommenderStanding;
            double sourceEstimate;
            if (IsStrong(kind))
            {
                recommenderEvidence = 32 + random.NextInt(29);
                recommenderStanding = 0.76 + (0.19 * random.NextUnit());
                sourceEstimate = IsTransferable(kind)
                    ? Math.Clamp(receiverTarget + Symmetric(random, 0.06), -1.0, 1.0)
                    : Math.Clamp((-0.60 * receiverTarget) + Symmetric(random, 0.10), -1.0, 1.0);
            }
            else
            {
                recommenderEvidence = 5 + random.NextInt(11);
                recommenderStanding = 0.28 + (0.30 * random.NextUnit());
                sourceEstimate = IsTransferable(kind)
                    ? Math.Clamp(receiverTarget + Symmetric(random, 0.15), -1.0, 1.0)
                    : Math.Clamp((-0.45 * receiverTarget) + Symmetric(random, 0.22), -1.0, 1.0);
            }

            cells[contextCell] = new StandingTransferCell(
                contextCell,
                kind,
                receiverTarget,
                sourceEstimate,
                recommenderStanding,
                recommenderEvidence,
                receiverNoise);
        }

        return new StandingTransferScenario(seed, recommenderCredibility, cells, ComputeFingerprint(recommenderCredibility, cells));
    }

    public static StandingTransferObservation[] CreateReceiverObservations(StandingTransferScenario scenario)
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
        var observations = new StandingTransferObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var contextCell = schedule[index];
            var cell = scenario.Cells[contextCell];
            observations[index] = new StandingTransferObservation(
                contextCell,
                Math.Clamp(cell.ReceiverTarget + Symmetric(random, cell.ReceiverNoiseAmplitude), -1.0, 1.0));
        }

        return observations;
    }

    public static bool IsTransferable(StandingTransferContextKind kind) =>
        kind is StandingTransferContextKind.StrongTransferable or StandingTransferContextKind.WeakTransferable;

    public static bool IsStrong(StandingTransferContextKind kind) =>
        kind is StandingTransferContextKind.StrongTransferable or StandingTransferContextKind.StrongLocalMismatch;

    public static int CountKind(StandingTransferScenario scenario, StandingTransferContextKind kind)
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

    private static double Symmetric(DeterministicRandom random, double amplitude) =>
        ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private static ulong ComputeFingerprint(double recommenderCredibility, StandingTransferCell[] cells)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;
        hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(recommenderCredibility));
        hash *= prime;
        for (var index = 0; index < cells.Length; index++)
        {
            var cell = cells[index];
            hash ^= (ulong)cell.ContextKind + 1UL;
            hash *= prime;
            hash ^= (ulong)cell.RecommenderEvidenceCount;
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.ReceiverTarget));
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.SourceEstimate));
            hash *= prime;
            hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.RecommenderStanding));
            hash *= prime;
        }

        return hash;
    }
}
