using Cpa.BoundedMindsLab.Core;

namespace Cpa.BoundedMindsLab.Falsification;

public static class AuthorityAncestryProbes
{
    private const int PeerCount = 5;
    private const int ContextCount = 12;
    private const int RepetitionsPerContext = 26;
    private const int EarlyEvidenceLimit = 5;

    public static Dictionary<string, double> EvaluateGroundingDiversityVersusPeerTrust(double independentRoots, double peerTrust, ulong seed) =>
        Evaluate(new ProbeSettings(IndependentRootDiversity: independentRoots, PeerTrust: peerTrust), seed);

    public static Dictionary<string, double> EvaluateCirculationDepthVersusAncestryFidelity(double socialRounds, double ancestryFidelity, ulong seed) =>
        Evaluate(new ProbeSettings(SocialRounds: ToInt(socialRounds), AncestryFidelity: ancestryFidelity), seed, MarginMode.AncestryFidelity);

    public static Dictionary<string, double> EvaluateCircularStrengthVersusReceiverMismatch(double circularRootStanding, double receiverMismatch, ulong seed) =>
        Evaluate(new ProbeSettings(CircularRootStanding: circularRootStanding, CircularMismatch: receiverMismatch), seed, MarginMode.CircularTradeoff);

    public static Dictionary<string, double> EvaluateConsequenceDelayVersusCirculationDepth(double consequenceDelay, double socialRounds, ulong seed) =>
        Evaluate(new ProbeSettings(ConsequenceDelay: ToInt(consequenceDelay), SocialRounds: ToInt(socialRounds)), seed, MarginMode.WholeHistory);

    public static Dictionary<string, double> EvaluateGroundedNoiseVersusDelay(double consequenceNoise, double consequenceDelay, ulong seed) =>
        Evaluate(
            new ProbeSettings(AllGrounded: true, IndependentRootDiversity: 3.5, ConsequenceNoise: consequenceNoise, ConsequenceDelay: ToInt(consequenceDelay)),
            seed,
            MarginMode.GroundedNullHarm);

    public static Dictionary<string, double> EvaluateNetworkClosureVersusRootCount(double networkClosure, double independentRoots, ulong seed) =>
        Evaluate(
            new ProbeSettings(AllGrounded: true, IndependentRootDiversity: independentRoots, NetworkClosure: networkClosure),
            seed,
            MarginMode.GroundedTopology);

    private static Dictionary<string, double> Evaluate(ProbeSettings settings, ulong seed, MarginMode marginMode = MarginMode.Discrimination)
    {
        var random = new DeterministicRandom(seed ^ 0xA24BAED4963EE407UL);
        var cells = BuildCells(settings, random);
        var ancestryAuthority = DevelopNetwork(cells, settings, recursive: false);
        var recursiveAuthority = DevelopNetwork(cells, settings, recursive: true);
        var observations = BuildObservations(cells, settings, seed);
        var ancestry = RunReceiver(cells, observations, ancestryAuthority.Authority, settings, social: true);
        var recursive = RunReceiver(cells, observations, recursiveAuthority.Authority, settings, social: true);
        var direct = RunReceiver(cells, observations, new double[ContextCount], settings, social: false);

        var groundedAuthority = MeanForKind(cells, ancestryAuthority.Authority, CellKind.Grounded);
        var circularAuthority = MeanForKind(cells, ancestryAuthority.Authority, CellKind.Circular);
        var recursiveCircularAuthority = MeanForKind(cells, recursiveAuthority.Authority, CellKind.Circular);
        var authorityGap = groundedAuthority - circularAuthority;
        var groundedOpportunity = direct.EarlyGroundedRmse - ancestry.EarlyGroundedRmse;
        var circularProtection = recursive.EarlyCircularRmse - ancestry.EarlyCircularRmse;
        var relativeCircularDiscount = (recursiveCircularAuthority * 0.45) - circularAuthority;
        var wholeHistoryVsRecursive = recursive.Rmse - ancestry.Rmse;
        var wholeHistoryVsDirectAllowance = (direct.Rmse * 1.05) - ancestry.Rmse;
        var finalGroundedStanding = ancestry.FinalGroundedStanding;

        var margin = marginMode switch
        {
            MarginMode.Discrimination => Math.Min(groundedOpportunity, authorityGap - 0.20),
            MarginMode.CircularTradeoff => Math.Min(wholeHistoryVsRecursive, wholeHistoryVsDirectAllowance),
            MarginMode.WholeHistory => Math.Min(wholeHistoryVsRecursive, wholeHistoryVsDirectAllowance),
            MarginMode.GroundedNullHarm => Math.Min((direct.Rmse * 0.95) - ancestry.Rmse, finalGroundedStanding - 0.70),
            MarginMode.GroundedTopology => Math.Min(groundedAuthority - 0.50, groundedOpportunity),
            MarginMode.AncestryFidelity => Math.Min(circularProtection, relativeCircularDiscount),
            _ => throw new InvalidOperationException("Unknown Protocol 09 falsification margin mode."),
        };

        return Metrics(
            ("boundary_margin", margin),
            ("ancestry_rmse", ancestry.Rmse),
            ("recursive_rmse", recursive.Rmse),
            ("direct_rmse", direct.Rmse),
            ("ancestry_early_grounded_rmse", ancestry.EarlyGroundedRmse),
            ("direct_early_grounded_rmse", direct.EarlyGroundedRmse),
            ("ancestry_early_circular_rmse", ancestry.EarlyCircularRmse),
            ("recursive_early_circular_rmse", recursive.EarlyCircularRmse),
            ("ancestry_initial_grounded_authority", groundedAuthority),
            ("ancestry_initial_circular_authority", circularAuthority),
            ("recursive_initial_circular_authority", recursiveCircularAuthority),
            ("authority_gap", authorityGap),
            ("grounded_opportunity_margin", groundedOpportunity),
            ("circular_protection_margin", circularProtection),
            ("relative_circular_discount_margin", relativeCircularDiscount),
            ("ancestry_final_grounded_standing", finalGroundedStanding),
            ("ancestry_final_circular_standing", ancestry.FinalCircularStanding),
            ("recursive_final_circular_standing", recursive.FinalCircularStanding),
            ("effective_independent_roots", settings.IndependentRootDiversity),
            ("peer_trust", settings.PeerTrust),
            ("social_rounds", settings.SocialRounds),
            ("ancestry_fidelity", settings.AncestryFidelity),
            ("circular_root_standing", settings.CircularRootStanding),
            ("circular_mismatch", settings.CircularMismatch),
            ("consequence_delay", settings.ConsequenceDelay),
            ("consequence_noise", settings.ConsequenceNoise),
            ("network_closure", settings.NetworkClosure));
    }

    private static ProbeCell[] BuildCells(ProbeSettings settings, DeterministicRandom random)
    {
        var cells = new ProbeCell[ContextCount];
        for (var context = 0; context < ContextCount; context++)
        {
            var grounded = settings.AllGrounded || context < ContextCount / 2;
            var sign = random.NextInt(2) == 0 ? -1.0 : 1.0;
            var target = sign * (0.35 + (0.45 * random.NextUnit()));
            var candidate = grounded
                ? Math.Clamp(target + Symmetric(random, 0.06), -1.0, 1.0)
                : MismatchedCandidate(target, settings.CircularMismatch, random);
            cells[context] = new ProbeCell(
                grounded ? CellKind.Grounded : CellKind.Circular,
                target,
                candidate,
                grounded ? settings.IndependentRootDiversity : 1.0,
                grounded ? settings.GroundedRootStanding : settings.CircularRootStanding,
                (context + random.NextInt(PeerCount)) % PeerCount);
        }

        return cells;
    }

    private static double MismatchedCandidate(double target, double mismatch, DeterministicRandom random)
    {
        if (mismatch <= 1e-12)
        {
            return Math.Clamp(target + Symmetric(random, 0.03), -1.0, 1.0);
        }

        var direction = target >= 0.0 ? -1.0 : 1.0;
        return Math.Clamp(target + (direction * mismatch) + Symmetric(random, 0.025), -1.0, 1.0);
    }

    private static ProbeObservation[] BuildObservations(ProbeCell[] cells, ProbeSettings settings, ulong seed)
    {
        var schedule = new List<int>(ContextCount * RepetitionsPerContext);
        for (var repetition = 0; repetition < RepetitionsPerContext; repetition++)
        {
            for (var context = 0; context < ContextCount; context++)
            {
                schedule.Add(context);
            }
        }

        new DeterministicRandom(seed ^ 0x9E3779B97F4A7C15UL).Shuffle(schedule);
        var random = new DeterministicRandom(seed ^ 0xBF58476D1CE4E5B9UL);
        var exposures = new int[ContextCount];
        var observations = new ProbeObservation[schedule.Count];
        for (var index = 0; index < schedule.Count; index++)
        {
            var context = schedule[index];
            exposures[context]++;
            var noise = settings.ConsequenceNoise;
            observations[index] = new ProbeObservation(
                context,
                Math.Clamp(cells[context].Target + Symmetric(random, noise), -1.0, 1.0),
                exposures[context]);
        }

        return observations;
    }

    private static NetworkOutcome DevelopNetwork(ProbeCell[] cells, ProbeSettings settings, bool recursive)
    {
        var standing = new double[ContextCount, PeerCount];
        var roots = new ushort[ContextCount, PeerCount];
        var paths = new ushort[ContextCount, PeerCount];
        for (var context = 0; context < ContextCount; context++)
        {
            var cell = cells[context];
            for (var peer = 0; peer < PeerCount; peer++)
            {
                var rank = RootRank(cell, peer);
                var rootWeight = RootWeight(cell.RootDiversity, rank);
                standing[context, peer] = rank >= 0
                    ? Math.Clamp((cell.BaseRootStanding - (0.04 * rank)) * rootWeight, 0.08, 0.95)
                    : 0.08;
                roots[context, peer] = rank >= 0 ? PeerBit(peer) : (ushort)0;
                paths[context, peer] = PeerBit(peer);
            }
        }

        for (var round = 0; round < settings.SocialRounds; round++)
        {
            var nextStanding = (double[,])standing.Clone();
            var nextRoots = (ushort[,])roots.Clone();
            var nextPaths = (ushort[,])paths.Clone();
            for (var context = 0; context < ContextCount; context++)
            {
                for (var peer = 0; peer < PeerCount; peer++)
                {
                    var predecessor = (peer + PeerCount - 1) % PeerCount;
                    var edgeStrength = peer == 0 ? settings.NetworkClosure : 1.0;
                    if (edgeStrength <= 1e-12)
                    {
                        continue;
                    }

                    var incomingStanding = standing[context, predecessor] * edgeStrength;
                    var trust = Math.Clamp(settings.PeerTrust + (0.015 * ((peer + round) % 3)), 0.0, 0.98);
                    var incomingRoots = roots[context, predecessor];
                    var currentRoots = roots[context, peer];
                    var returnsToSelf = (paths[context, predecessor] & PeerBit(peer)) != 0;
                    if (recursive)
                    {
                        nextStanding[context, peer] = Math.Clamp(
                            standing[context, peer] + (0.34 * incomingStanding * trust),
                            0.02,
                            0.98);
                    }
                    else
                    {
                        var incomingRootCount = CountBits(incomingRoots);
                        var overlappingRoots = CountBits((ushort)(incomingRoots & currentRoots));
                        var trueNovelty = incomingRootCount == 0 ? 0.12 : Math.Max(0.0, 1.0 - ((double)overlappingRoots / incomingRootCount));
                        var novelty = (settings.AncestryFidelity * trueNovelty) + ((1.0 - settings.AncestryFidelity) * 1.0);
                        if (returnsToSelf)
                        {
                            var returnMultiplier = 0.05 + ((1.0 - settings.AncestryFidelity) * 0.95);
                            novelty *= returnMultiplier;
                        }

                        var rootFactor = incomingRoots == 0 ? 0.12 : 1.0;
                        nextStanding[context, peer] = Math.Clamp(
                            standing[context, peer] + (0.20 * incomingStanding * trust * novelty * rootFactor),
                            0.02,
                            0.95);
                    }

                    nextRoots[context, peer] = (ushort)(currentRoots | incomingRoots);
                    nextPaths[context, peer] = (ushort)(paths[context, peer] | paths[context, predecessor] | PeerBit(peer));
                }
            }

            standing = nextStanding;
            roots = nextRoots;
            paths = nextPaths;
        }

        var authority = new double[ContextCount];
        for (var context = 0; context < ContextCount; context++)
        {
            if (recursive)
            {
                var total = 0.0;
                for (var peer = 0; peer < PeerCount; peer++)
                {
                    total += standing[context, peer];
                }

                authority[context] = total / PeerCount;
                continue;
            }

            var orderedPeers = Enumerable.Range(0, PeerCount).OrderByDescending(peer => standing[context, peer]).ToArray();
            ushort seenRoots = 0;
            var evidence = 0.0;
            foreach (var peer in orderedPeers)
            {
                var peerRoots = roots[context, peer];
                var rootCount = CountBits(peerRoots);
                var novelCount = CountBits((ushort)(peerRoots & ~seenRoots));
                var trueNovelty = rootCount == 0 ? 0.0 : (double)novelCount / rootCount;
                var novelty = (settings.AncestryFidelity * trueNovelty) + ((1.0 - settings.AncestryFidelity) * (rootCount == 0 ? 0.0 : 1.0));
                evidence += standing[context, peer] * (0.08 + (0.92 * novelty));
                seenRoots = (ushort)(seenRoots | peerRoots);
            }

            authority[context] = Math.Min(0.90, 1.0 - Math.Exp(-0.82 * evidence));
        }

        return new NetworkOutcome(authority);
    }

    private static ReceiverOutcome RunReceiver(
        ProbeCell[] cells,
        ProbeObservation[] observations,
        double[] initialAuthority,
        ProbeSettings settings,
        bool social)
    {
        var localEstimate = new double[ContextCount];
        var localEvidence = new int[ContextCount];
        var standing = social ? (double[])initialAuthority.Clone() : new double[ContextCount];
        var pending = Enumerable.Range(0, ContextCount).Select(_ => new Queue<PendingConsequence>()).ToArray();
        var all = new RunningError();
        var earlyGrounded = new RunningError();
        var earlyCircular = new RunningError();

        foreach (var observation in observations)
        {
            DeliverDue(observation.Context, observation.Exposure, pending, localEstimate, localEvidence, standing, cells, social, beforeCurrent: true);
            var localWeight = 0.42 + Math.Min(1.0, localEvidence[observation.Context] / 10.0);
            var socialWeight = social ? Math.Min(0.85, standing[observation.Context]) : 0.0;
            var prediction = social
                ? ((localEstimate[observation.Context] * localWeight) + (cells[observation.Context].Candidate * socialWeight)) / (localWeight + socialWeight)
                : localEstimate[observation.Context];
            var error = prediction - observation.Target;
            all.Add(error);
            if (observation.Exposure <= EarlyEvidenceLimit)
            {
                if (cells[observation.Context].Kind == CellKind.Grounded)
                {
                    earlyGrounded.Add(error);
                }
                else
                {
                    earlyCircular.Add(error);
                }
            }

            pending[observation.Context].Enqueue(new PendingConsequence(observation.Exposure + settings.ConsequenceDelay, observation.Target));
            DeliverDue(observation.Context, observation.Exposure, pending, localEstimate, localEvidence, standing, cells, social, beforeCurrent: false);
        }

        for (var context = 0; context < ContextCount; context++)
        {
            while (pending[context].Count > 0)
            {
                ApplyDirect(context, pending[context].Dequeue().Target, localEstimate, localEvidence, standing, cells, social);
            }
        }

        return new ReceiverOutcome(
            all.Rmse,
            earlyGrounded.Rmse,
            earlyCircular.Rmse,
            MeanStanding(cells, standing, CellKind.Grounded),
            MeanStanding(cells, standing, CellKind.Circular));
    }

    private static void DeliverDue(
        int context,
        int exposure,
        Queue<PendingConsequence>[] pending,
        double[] localEstimate,
        int[] localEvidence,
        double[] standing,
        ProbeCell[] cells,
        bool social,
        bool beforeCurrent)
    {
        while (pending[context].Count > 0)
        {
            var item = pending[context].Peek();
            var due = beforeCurrent ? item.DueExposure < exposure : item.DueExposure <= exposure;
            if (!due)
            {
                return;
            }

            pending[context].Dequeue();
            ApplyDirect(context, item.Target, localEstimate, localEvidence, standing, cells, social);
        }
    }

    private static void ApplyDirect(
        int context,
        double target,
        double[] localEstimate,
        int[] localEvidence,
        double[] standing,
        ProbeCell[] cells,
        bool social)
    {
        localEvidence[context]++;
        var learningRate = localEvidence[context] <= 8 ? 0.20 : 0.14;
        localEstimate[context] += learningRate * (target - localEstimate[context]);
        if (!social)
        {
            return;
        }

        var sourceError = Math.Abs(cells[context].Candidate - target);
        var quality = Math.Exp(-3.2 * sourceError);
        standing[context] += 0.20 * (quality - standing[context]);
        if (sourceError > 0.55)
        {
            standing[context] *= 0.72;
        }

        standing[context] = Math.Clamp(standing[context], 0.02, 0.98);
    }

    private static int RootRank(ProbeCell cell, int peer)
    {
        var rootSlots = (int)Math.Ceiling(cell.RootDiversity - 1e-12);
        for (var rank = 0; rank < rootSlots; rank++)
        {
            if ((cell.RootOffset + (rank * 2)) % PeerCount == peer)
            {
                return rank;
            }
        }

        return -1;
    }

    private static double RootWeight(double diversity, int rank)
    {
        if (rank < 0)
        {
            return 0.0;
        }

        var whole = (int)Math.Floor(diversity + 1e-12);
        if (rank < whole)
        {
            return 1.0;
        }

        return Math.Clamp(diversity - whole, 0.0, 1.0);
    }

    private static double MeanForKind(ProbeCell[] cells, double[] values, CellKind kind)
    {
        var total = 0.0;
        var count = 0;
        for (var index = 0; index < cells.Length; index++)
        {
            if (cells[index].Kind == kind)
            {
                total += values[index];
                count++;
            }
        }

        return count == 0 ? 0.0 : total / count;
    }

    private static double MeanStanding(ProbeCell[] cells, double[] values, CellKind kind) => MeanForKind(cells, values, kind);

    private static int CountBits(ushort value)
    {
        var count = 0;
        var remaining = value;
        while (remaining != 0)
        {
            count += remaining & 1;
            remaining >>= 1;
        }

        return count;
    }

    private static ushort PeerBit(int peer) => (ushort)(1 << peer);

    private static int ToInt(double value) => Math.Max(0, (int)Math.Round(value, MidpointRounding.AwayFromZero));

    private static double Symmetric(DeterministicRandom random, double amplitude) => ((random.NextUnit() * 2.0) - 1.0) * amplitude;

    private static Dictionary<string, double> Metrics(params (string Name, double Value)[] pairs)
    {
        var result = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var pair in pairs)
        {
            result[pair.Name] = pair.Value;
        }

        return result;
    }

    private enum CellKind
    {
        Grounded,
        Circular,
    }

    private enum MarginMode
    {
        Discrimination,
        CircularTradeoff,
        WholeHistory,
        GroundedNullHarm,
        GroundedTopology,
        AncestryFidelity,
    }

    private sealed record ProbeSettings(
        double IndependentRootDiversity = 3.5,
        double GroundedRootStanding = 0.78,
        double CircularRootStanding = 0.27,
        double PeerTrust = 0.85,
        int SocialRounds = 8,
        double AncestryFidelity = 1.0,
        double CircularMismatch = 0.90,
        int ConsequenceDelay = 0,
        double ConsequenceNoise = 0.035,
        double NetworkClosure = 1.0,
        bool AllGrounded = false);

    private sealed record ProbeCell(CellKind Kind, double Target, double Candidate, double RootDiversity, double BaseRootStanding, int RootOffset);

    private readonly record struct ProbeObservation(int Context, double Target, int Exposure);

    private readonly record struct PendingConsequence(int DueExposure, double Target);

    private sealed record NetworkOutcome(double[] Authority);

    private sealed record ReceiverOutcome(double Rmse, double EarlyGroundedRmse, double EarlyCircularRmse, double FinalGroundedStanding, double FinalCircularStanding);

    private sealed class RunningError
    {
        private double _sumSquares;
        private int _count;

        public double Rmse => _count == 0 ? 0.0 : Math.Sqrt(_sumSquares / _count);

        public void Add(double error)
        {
            _sumSquares += error * error;
            _count++;
        }
    }
}
