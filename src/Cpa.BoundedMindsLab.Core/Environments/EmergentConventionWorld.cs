using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Environments;

public enum ConventionContextKind
{
    Stable = 0,
    Shifted = 1,
}

public sealed record ConventionScenarioCell(
    int ContextCell,
    ConventionContextKind ContextKind,
    double[][] InitialPeerCosts,
    double[][] ShiftedPeerCosts,
    int ShiftPreferredAction,
    double InitialViableGap,
    int InitialPreferenceDiversity);

public sealed record EmergentConventionScenario(
    ulong Seed,
    ConventionScenarioCell[] Cells,
    ulong Fingerprint);

public sealed record ConventionEpisode(
    int ContextCell,
    double[][] PeerCosts);

public static class EmergentConventionWorld
{
    public const int ContextCount = 12;
    public const int PeerCount = 3;
    public const int ActionCount = 3;
    public const int FormationRepetitionsPerContext = 22;
    public const int ShiftedRepetitionsPerContext = 20;

    private const ulong ScenarioSeedMask = 0x5C17E0A1D00DFEEDUL;
    private const ulong FormationScheduleSeedMask = 0x1111222233334444UL;
    private const ulong ShiftedScheduleSeedMask = 0x5555666677778888UL;
    private const ulong FormationNoiseSeedMask = 0xABCDEF1029384756UL;
    private const ulong ShiftedNoiseSeedMask = 0x6758493021FEDCBAUL;
    private const double FormationCostNoiseAmplitude = 0.018;
    private const double ShiftedCostNoiseAmplitude = 0.015;

    public static EmergentConventionScenario CreateScenario(ulong seed)
    {
        var random = new DeterministicRandom(seed ^ ScenarioSeedMask);
        var changedCount = 4 + random.NextInt(3);
        var contextOrder = Enumerable.Range(0, ContextCount).ToList();
        random.Shuffle(contextOrder);
        var changed = new HashSet<int>();
        for (var index = 0; index < changedCount; index++)
        {
            changed.Add(contextOrder[index]);
        }

        var cells = new ConventionScenarioCell[ContextCount];
        for (var contextCell = 0; contextCell < ContextCount; contextCell++)
        {
            var actions = new List<int> { 0, 1, 2 };
            random.Shuffle(actions);
            var firstViableAction = actions[0];
            var secondViableAction = actions[1];
            var shiftPreferredAction = actions[2];
            var firstBaseCost = 0.12 + (0.07 * random.NextUnit());
            var secondBaseCost = Math.Clamp(firstBaseCost + Symmetric(random, 0.025), 0.09, 0.23);
            var initiallyExpensiveCost = 0.48 + (0.14 * random.NextUnit());
            var initialPeerCosts = new double[PeerCount][];
            var shiftedPeerCosts = new double[PeerCount][];

            for (var peerIndex = 0; peerIndex < PeerCount; peerIndex++)
            {
                var personalLean = Symmetric(random, 0.055);
                var firstCost = Math.Clamp(firstBaseCost + personalLean + Symmetric(random, 0.022), 0.05, 0.34);
                var secondCost = Math.Clamp(secondBaseCost - personalLean + Symmetric(random, 0.022), 0.05, 0.34);
                var expensiveCost = Math.Clamp(initiallyExpensiveCost + Symmetric(random, 0.05), 0.40, 0.72);
                var initial = new double[ActionCount];
                initial[firstViableAction] = firstCost;
                initial[secondViableAction] = secondCost;
                initial[shiftPreferredAction] = expensiveCost;
                initialPeerCosts[peerIndex] = initial;

                if (changed.Contains(contextCell))
                {
                    var shifted = new double[ActionCount];
                    shifted[shiftPreferredAction] = Math.Clamp(0.10 + (0.08 * random.NextUnit()) + Symmetric(random, 0.018), 0.05, 0.28);
                    shifted[firstViableAction] = Math.Clamp(0.50 + (0.12 * random.NextUnit()) + Symmetric(random, 0.03), 0.38, 0.75);
                    shifted[secondViableAction] = Math.Clamp(0.49 + (0.13 * random.NextUnit()) + Symmetric(random, 0.03), 0.38, 0.75);
                    shiftedPeerCosts[peerIndex] = shifted;
                }
                else
                {
                    var shifted = new double[ActionCount];
                    for (var action = 0; action < ActionCount; action++)
                    {
                        shifted[action] = Math.Clamp(initial[action] + Symmetric(random, 0.012), 0.05, 0.75);
                    }

                    shiftedPeerCosts[peerIndex] = shifted;
                }
            }

            var initialMeanCosts = MeanActionCosts(initialPeerCosts);
            Array.Sort(initialMeanCosts);
            cells[contextCell] = new ConventionScenarioCell(
                contextCell,
                changed.Contains(contextCell) ? ConventionContextKind.Shifted : ConventionContextKind.Stable,
                initialPeerCosts,
                shiftedPeerCosts,
                shiftPreferredAction,
                initialMeanCosts[1] - initialMeanCosts[0],
                CountDistinctPrivatePreferences(initialPeerCosts));
        }

        return new EmergentConventionScenario(seed, cells, ComputeFingerprint(cells));
    }

    public static ConventionEpisode[] CreateFormationEpisodes(EmergentConventionScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        return CreateEpisodes(scenario, FormationRepetitionsPerContext, false, FormationScheduleSeedMask, FormationNoiseSeedMask, FormationCostNoiseAmplitude);
    }

    public static ConventionEpisode[] CreateShiftedEpisodes(EmergentConventionScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        return CreateEpisodes(scenario, ShiftedRepetitionsPerContext, true, ShiftedScheduleSeedMask, ShiftedNoiseSeedMask, ShiftedCostNoiseAmplitude);
    }

    public static int CountKind(EmergentConventionScenario scenario, ConventionContextKind kind)
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

    public static int CountPreferenceDiverseContexts(EmergentConventionScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var count = 0;
        for (var index = 0; index < scenario.Cells.Length; index++)
        {
            if (scenario.Cells[index].InitialPreferenceDiversity >= 2)
            {
                count++;
            }
        }

        return count;
    }

    public static double MeanInitialViableGap(EmergentConventionScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var total = 0.0;
        for (var index = 0; index < scenario.Cells.Length; index++)
        {
            total += scenario.Cells[index].InitialViableGap;
        }

        return scenario.Cells.Length == 0 ? 0.0 : total / scenario.Cells.Length;
    }

    private static ConventionEpisode[] CreateEpisodes(
        EmergentConventionScenario scenario,
        int repetitionsPerContext,
        bool shifted,
        ulong scheduleSeedMask,
        ulong noiseSeedMask,
        double noiseAmplitude)
    {
        var schedule = new List<int>(scenario.Cells.Length * repetitionsPerContext);
        for (var repetition = 0; repetition < repetitionsPerContext; repetition++)
        {
            for (var contextCell = 0; contextCell < scenario.Cells.Length; contextCell++)
            {
                schedule.Add(contextCell);
            }
        }

        new DeterministicRandom(scenario.Seed ^ scheduleSeedMask).Shuffle(schedule);
        var noise = new DeterministicRandom(scenario.Seed ^ noiseSeedMask);
        var episodes = new ConventionEpisode[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var contextCell = schedule[index];
            var sourceCosts = shifted ? scenario.Cells[contextCell].ShiftedPeerCosts : scenario.Cells[contextCell].InitialPeerCosts;
            var peerCosts = new double[PeerCount][];
            for (var peerIndex = 0; peerIndex < PeerCount; peerIndex++)
            {
                var costs = new double[ActionCount];
                for (var action = 0; action < ActionCount; action++)
                {
                    costs[action] = Math.Clamp(sourceCosts[peerIndex][action] + Symmetric(noise, noiseAmplitude), 0.03, 0.80);
                }

                peerCosts[peerIndex] = costs;
            }

            episodes[index] = new ConventionEpisode(contextCell, peerCosts);
        }

        return episodes;
    }

    private static double[] MeanActionCosts(double[][] peerCosts)
    {
        var means = new double[ActionCount];
        for (var action = 0; action < ActionCount; action++)
        {
            var total = 0.0;
            for (var peerIndex = 0; peerIndex < peerCosts.Length; peerIndex++)
            {
                total += peerCosts[peerIndex][action];
            }

            means[action] = peerCosts.Length == 0 ? 0.0 : total / peerCosts.Length;
        }

        return means;
    }

    private static int CountDistinctPrivatePreferences(double[][] peerCosts)
    {
        var preferences = new HashSet<int>();
        for (var peerIndex = 0; peerIndex < peerCosts.Length; peerIndex++)
        {
            var bestAction = 0;
            var bestCost = peerCosts[peerIndex][0];
            for (var action = 1; action < ActionCount; action++)
            {
                if (peerCosts[peerIndex][action] < bestCost)
                {
                    bestCost = peerCosts[peerIndex][action];
                    bestAction = action;
                }
            }

            preferences.Add(bestAction);
        }

        return preferences.Count;
    }

    private static double Symmetric(DeterministicRandom random, double amplitude) =>
        ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private static ulong ComputeFingerprint(ConventionScenarioCell[] cells)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;
        for (var cellIndex = 0; cellIndex < cells.Length; cellIndex++)
        {
            var cell = cells[cellIndex];
            hash ^= (ulong)cell.ContextKind + 1UL;
            hash *= prime;
            hash ^= (ulong)cell.ShiftPreferredAction + 1UL;
            hash *= prime;
            for (var peerIndex = 0; peerIndex < cell.InitialPeerCosts.Length; peerIndex++)
            {
                for (var action = 0; action < cell.InitialPeerCosts[peerIndex].Length; action++)
                {
                    hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.InitialPeerCosts[peerIndex][action]));
                    hash *= prime;
                    hash ^= unchecked((ulong)BitConverter.DoubleToInt64Bits(cell.ShiftedPeerCosts[peerIndex][action]));
                    hash *= prime;
                }
            }
        }

        return hash;
    }
}
