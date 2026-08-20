using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed class AuthorityAncestryCircularStandingExperiment : IExperiment
{
    private const string ExperimentName = "09-authority-ancestry-circular-standing";
    private const int SocialRounds = 8;
    private const int EarlyEvidenceLimit = 5;
    private const int LateEvidenceThreshold = 20;
    private const double PublicPacketCost = 0.006;

    public string Name => ExperimentName;

    public string Question =>
        "Can a bounded ecology distinguish authority earned from independent consequence from permission that has recursively circulated through locally reasonable endorsements?";

    public ExperimentResult Run(ExperimentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Emit(ExperimentFrameKind.ExperimentStarted, Name, message: Question);

        var scenario = AuthorityAncestryWorld.CreateScenario(context.Seed);
        EmitScenario(context, scenario);
        var observations = AuthorityAncestryWorld.CreateReceiverObservations(scenario);

        var ancestryAware = RunPath(
            context,
            scenario,
            observations,
            AuthorityMode.AuthorityAncestry,
            "Compact authority-root and path sketches prevent a mind from counting recursively returned permission as new independent warrant. Direct receiver consequence remains sovereign.");
        var recursive = RunPath(
            context,
            scenario,
            observations,
            AuthorityMode.RecursiveEndorsement,
            "Control: each local endorsement is individually reasonable, but trusted standing may recursively amplify through the ring without preserving authority ancestry.");
        var directOnly = RunPath(
            context,
            scenario,
            observations,
            AuthorityMode.DirectOnly,
            "Baseline: the receiver ignores all social endorsements and earns candidate standing only from its own consequence.");

        var metrics = BuildResultMetrics(scenario, ancestryAware, recursive, directOnly);
        var assertions = BuildAssertions(scenario, ancestryAware, recursive, directOnly);
        var passed = assertions.Count(assertion => assertion.Passed);
        var verdict = passed == assertions.Count
            ? ExperimentVerdict.Support
            : passed >= 8
                ? ExperimentVerdict.Mixed
                : ExperimentVerdict.Disconfirm;
        var interpretation = verdict switch
        {
            ExperimentVerdict.Support =>
                "Authority ancestry preserved useful independently grounded social opportunity while preventing a weak authority seed from becoming strong merely by circulating through mutually trusted peers. Direct consequence later corrected both social treatments, but ancestry limited the damage before that correction arrived.",
            ExperimentVerdict.Mixed =>
                "Authority ancestry helped on most registered boundaries, but at least one manipulation, benefit, or safety condition failed. Treat the failed boundary as evidence about where circular permission remains unresolved rather than as a reason to tune the frozen result after inspection.",
            _ =>
                "The authority-ancestry mechanism did not reliably separate independently grounded permission from circular endorsement under the preregistered development conditions.",
        };

        var result = new ExperimentResult(Name, Question, verdict, interpretation, metrics, assertions);
        context.Emit(
            ExperimentFrameKind.ExperimentCompleted,
            Name,
            message: interpretation,
            completion: new ExperimentCompletion(result.Verdict, result.Interpretation, result.Metrics, result.Assertions));
        return result;
    }

    private static void EmitScenario(ExperimentContext context, AuthorityAncestryScenario scenario)
    {
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "scenario",
            phase: "authority-world-generated",
            message: $"Generated authority world {scenario.Fingerprint:X16}: {AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.IndependentGrounding)} independently grounded, {AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.CircularAuthorityTrap)} circular traps, {AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.MixedAuthority)} mixed, {AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.SparseGrounding)} sparse.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
                ["independent_grounding_contexts"] = AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.IndependentGrounding),
                ["circular_authority_contexts"] = AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.CircularAuthorityTrap),
                ["mixed_authority_contexts"] = AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.MixedAuthority),
                ["sparse_grounding_contexts"] = AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.SparseGrounding),
            });
    }

    private static PathOutcome RunPath(
        ExperimentContext context,
        AuthorityAncestryScenario scenario,
        AuthorityAncestryObservation[] observations,
        AuthorityMode mode,
        string description)
    {
        var series = SeriesName(mode);
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            series,
            phase: mode == AuthorityMode.DirectOnly ? "receiver-consequence" : "authority-development",
            message: description);

        var network = mode == AuthorityMode.DirectOnly
            ? AuthorityNetworkOutcome.Empty(scenario.Cells.Length)
            : DevelopAuthorityNetwork(context, scenario, mode, series);

        if (mode != AuthorityMode.DirectOnly)
        {
            context.Emit(
                ExperimentFrameKind.PhaseChanged,
                ExperimentName,
                series,
                SocialRounds,
                "receiver-consequence",
                message: $"{series} exposes final compact endorsements to the receiver, then makes their influence answerable to receiver-owned consequence.");
        }

        var receiver = new AuthorityReceiver(scenario, mode, network.ReceiverAuthority);
        var allErrors = new RunningSquaredError();
        var earlyGroundedErrors = new RunningSquaredError();
        var earlyCircularErrors = new RunningSquaredError();
        var lateCircularErrors = new RunningSquaredError();

        for (var tick = 0; tick < observations.Length; tick++)
        {
            var observation = observations[tick];
            var cell = scenario.Cells[observation.ContextCell];
            var prediction = receiver.Predict(observation.ContextCell, cell.CandidateEstimate, out var socialWeight);
            var error = prediction - observation.Target;
            allErrors.Add(error);

            if (cell.ContextKind == AuthorityAncestryContextKind.IndependentGrounding && observation.ContextExposure <= EarlyEvidenceLimit)
            {
                earlyGroundedErrors.Add(error);
            }

            if (cell.ContextKind == AuthorityAncestryContextKind.CircularAuthorityTrap && observation.ContextExposure <= EarlyEvidenceLimit)
            {
                earlyCircularErrors.Add(error);
            }

            if (cell.ContextKind == AuthorityAncestryContextKind.CircularAuthorityTrap && observation.ContextExposure >= LateEvidenceThreshold)
            {
                lateCircularErrors.Add(error);
            }

            receiver.ObserveDirect(observation.ContextCell, observation.Target, cell.CandidateEstimate);
            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                series,
                tick,
                "receiver-consequence",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["prediction"] = prediction,
                    ["target"] = observation.Target,
                    ["candidate_estimate"] = cell.CandidateEstimate,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = allErrors.Rmse,
                    ["social_authority"] = mode == AuthorityMode.DirectOnly ? 0.0 : network.ReceiverAuthority[observation.ContextCell],
                    ["receiver_source_standing"] = receiver.SourceStandingFor(observation.ContextCell),
                    ["social_weight"] = socialWeight,
                    ["local_evidence"] = receiver.LocalEvidenceFor(observation.ContextCell),
                    ["direct_root_count"] = cell.DirectRootCount,
                    ["context_cell"] = observation.ContextCell,
                    ["context_kind"] = (int)cell.ContextKind,
                });

            if (tick % 24 == 23 || tick == observations.Length - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    ExperimentName,
                    series,
                    tick,
                    "receiver-consequence",
                    minds: receiver.PublicMindState(prediction, observation.Target, cell.CandidateEstimate),
                    traces: receiver.PublicTraceStates());
            }
        }

        var publicPacketCount = mode == AuthorityMode.DirectOnly
            ? 0
            : scenario.Cells.Length * AuthorityAncestryWorld.PeerCount * (SocialRounds + 1);
        var communicationWork = publicPacketCount * PublicPacketCost;
        var outcome = new PathOutcome(
            allErrors.Rmse,
            earlyGroundedErrors.Rmse,
            earlyCircularErrors.Rmse,
            lateCircularErrors.Rmse,
            network.MeanInitialPeerStanding(scenario, AuthorityAncestryContextKind.CircularAuthorityTrap),
            network.MeanAuthority(scenario, AuthorityAncestryContextKind.IndependentGrounding),
            network.MeanAuthority(scenario, AuthorityAncestryContextKind.CircularAuthorityTrap),
            receiver.MeanFinalStanding(AuthorityAncestryContextKind.IndependentGrounding),
            receiver.MeanFinalStanding(AuthorityAncestryContextKind.CircularAuthorityTrap),
            network.MeanRootCoverage(scenario, AuthorityAncestryContextKind.IndependentGrounding),
            network.MeanRootCoverage(scenario, AuthorityAncestryContextKind.CircularAuthorityTrap),
            publicPacketCount,
            communicationWork);

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            series,
            observations.Length,
            "path-complete",
            message: $"{series} completed with RMSE {outcome.Rmse:0.000000}, independent authority {outcome.InitialGroundedAuthority:0.000}, circular authority {outcome.InitialCircularAuthority:0.000}, and final circular standing {outcome.FinalCircularStanding:0.000}.",
            metrics: outcome.ToMetrics());
        return outcome;
    }

    private static AuthorityNetworkOutcome DevelopAuthorityNetwork(
        ExperimentContext context,
        AuthorityAncestryScenario scenario,
        AuthorityMode mode,
        string series)
    {
        var peerStanding = new double[scenario.Cells.Length, AuthorityAncestryWorld.PeerCount];
        var rootSketch = new ushort[scenario.Cells.Length, AuthorityAncestryWorld.PeerCount];
        var pathSketch = new ushort[scenario.Cells.Length, AuthorityAncestryWorld.PeerCount];
        var initialMeanStanding = new double[scenario.Cells.Length];

        for (var cellIndex = 0; cellIndex < scenario.Cells.Length; cellIndex++)
        {
            var cell = scenario.Cells[cellIndex];
            var total = 0.0;
            for (var peer = 0; peer < AuthorityAncestryWorld.PeerCount; peer++)
            {
                var rank = AuthorityAncestryWorld.DirectRootRank(cell, peer);
                var standing = rank >= 0
                    ? Math.Clamp(cell.BaseDirectStanding - (0.04 * rank), 0.10, 0.95)
                    : 0.08;
                peerStanding[cellIndex, peer] = standing;
                rootSketch[cellIndex, peer] = rank >= 0 ? PeerBit(peer) : (ushort)0;
                pathSketch[cellIndex, peer] = PeerBit(peer);
                total += standing;
            }

            initialMeanStanding[cellIndex] = total / AuthorityAncestryWorld.PeerCount;
        }

        for (var round = 0; round < SocialRounds; round++)
        {
            var nextStanding = (double[,])peerStanding.Clone();
            var nextRoots = (ushort[,])rootSketch.Clone();
            var nextPaths = (ushort[,])pathSketch.Clone();
            var cycleReturns = 0;
            var socialTransfers = 0;

            for (var cellIndex = 0; cellIndex < scenario.Cells.Length; cellIndex++)
            {
                var cell = scenario.Cells[cellIndex];
                for (var peer = 0; peer < AuthorityAncestryWorld.PeerCount; peer++)
                {
                    var predecessor = (peer + AuthorityAncestryWorld.PeerCount - 1) % AuthorityAncestryWorld.PeerCount;
                    var incomingStanding = peerStanding[cellIndex, predecessor];
                    var trust = Math.Clamp(cell.PeerTrust + (0.015 * ((peer + round) % 3)), 0.0, 0.95);
                    socialTransfers++;
                    var incomingRoots = rootSketch[cellIndex, predecessor];
                    var currentRoots = rootSketch[cellIndex, peer];
                    var returnsToSelf = (pathSketch[cellIndex, predecessor] & PeerBit(peer)) != 0;
                    if (returnsToSelf)
                    {
                        cycleReturns++;
                    }

                    if (mode == AuthorityMode.RecursiveEndorsement)
                    {
                        nextStanding[cellIndex, peer] = Math.Clamp(
                            peerStanding[cellIndex, peer] + (0.34 * incomingStanding * trust),
                            0.02,
                            0.98);
                        nextRoots[cellIndex, peer] = (ushort)(currentRoots | incomingRoots);
                        nextPaths[cellIndex, peer] = (ushort)(
                            pathSketch[cellIndex, peer] |
                            pathSketch[cellIndex, predecessor] |
                            PeerBit(peer));
                        continue;
                    }

                    var incomingRootCount = CountBits(incomingRoots);
                    var overlappingRoots = CountBits((ushort)(incomingRoots & currentRoots));
                    var rootNovelty = incomingRootCount == 0
                        ? 0.12
                        : Math.Max(0.0, 1.0 - ((double)overlappingRoots / incomingRootCount));
                    if (returnsToSelf)
                    {
                        rootNovelty *= 0.05;
                    }

                    var rootFactor = incomingRoots == 0 ? 0.12 : 1.0;
                    nextStanding[cellIndex, peer] = Math.Clamp(
                        peerStanding[cellIndex, peer] + (0.20 * incomingStanding * trust * rootNovelty * rootFactor),
                        0.02,
                        0.95);
                    nextRoots[cellIndex, peer] = (ushort)(currentRoots | incomingRoots);
                    nextPaths[cellIndex, peer] = (ushort)(
                        pathSketch[cellIndex, peer] |
                        pathSketch[cellIndex, predecessor] |
                        PeerBit(peer));
                }
            }

            peerStanding = nextStanding;
            rootSketch = nextRoots;
            pathSketch = nextPaths;
            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                series,
                round,
                "authority-development",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["mean_peer_standing"] = Mean(peerStanding),
                    ["independent_peer_standing"] = MeanPeerStanding(scenario, peerStanding, AuthorityAncestryContextKind.IndependentGrounding),
                    ["circular_peer_standing"] = MeanPeerStanding(scenario, peerStanding, AuthorityAncestryContextKind.CircularAuthorityTrap),
                    ["cycle_return_rate"] = socialTransfers == 0 ? 0.0 : (double)cycleReturns / socialTransfers,
                    ["authority_round"] = round + 1,
                });
        }

        var receiverAuthority = new double[scenario.Cells.Length];
        var rootCoverage = new double[scenario.Cells.Length];
        for (var cellIndex = 0; cellIndex < scenario.Cells.Length; cellIndex++)
        {
            if (mode == AuthorityMode.RecursiveEndorsement)
            {
                var total = 0.0;
                for (var peer = 0; peer < AuthorityAncestryWorld.PeerCount; peer++)
                {
                    total += peerStanding[cellIndex, peer];
                }

                receiverAuthority[cellIndex] = total / AuthorityAncestryWorld.PeerCount;
                rootCoverage[cellIndex] = scenario.Cells[cellIndex].DirectRootCount;
                continue;
            }

            var peers = Enumerable.Range(0, AuthorityAncestryWorld.PeerCount)
                .OrderByDescending(peer => peerStanding[cellIndex, peer])
                .ToArray();
            ushort seenRoots = 0;
            var evidence = 0.0;
            for (var index = 0; index < peers.Length; index++)
            {
                var peer = peers[index];
                var roots = rootSketch[cellIndex, peer];
                var rootCount = CountBits(roots);
                var novelRootCount = CountBits((ushort)(roots & ~seenRoots));
                var novelty = rootCount == 0 ? 0.0 : (double)novelRootCount / rootCount;
                evidence += peerStanding[cellIndex, peer] * (0.08 + (0.92 * novelty));
                seenRoots = (ushort)(seenRoots | roots);
            }

            receiverAuthority[cellIndex] = Math.Min(0.90, 1.0 - Math.Exp(-0.82 * evidence));
            rootCoverage[cellIndex] = CountBits(seenRoots);
        }

        return new AuthorityNetworkOutcome(receiverAuthority, initialMeanStanding, rootCoverage);
    }

    private static Dictionary<string, double> BuildResultMetrics(
        AuthorityAncestryScenario scenario,
        PathOutcome ancestryAware,
        PathOutcome recursive,
        PathOutcome directOnly) => new(StringComparer.Ordinal)
    {
        ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
        ["independent_grounding_contexts"] = AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.IndependentGrounding),
        ["circular_authority_contexts"] = AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.CircularAuthorityTrap),
        ["authority_ancestry_rmse"] = ancestryAware.Rmse,
        ["recursive_endorsement_rmse"] = recursive.Rmse,
        ["direct_only_rmse"] = directOnly.Rmse,
        ["authority_ancestry_early_grounded_rmse"] = ancestryAware.EarlyGroundedRmse,
        ["recursive_early_grounded_rmse"] = recursive.EarlyGroundedRmse,
        ["direct_early_grounded_rmse"] = directOnly.EarlyGroundedRmse,
        ["authority_ancestry_early_circular_rmse"] = ancestryAware.EarlyCircularRmse,
        ["recursive_early_circular_rmse"] = recursive.EarlyCircularRmse,
        ["direct_early_circular_rmse"] = directOnly.EarlyCircularRmse,
        ["authority_ancestry_late_circular_rmse"] = ancestryAware.LateCircularRmse,
        ["recursive_late_circular_rmse"] = recursive.LateCircularRmse,
        ["authority_ancestry_initial_grounded_authority"] = ancestryAware.InitialGroundedAuthority,
        ["authority_ancestry_initial_circular_authority"] = ancestryAware.InitialCircularAuthority,
        ["recursive_initial_grounded_authority"] = recursive.InitialGroundedAuthority,
        ["recursive_initial_circular_authority"] = recursive.InitialCircularAuthority,
        ["recursive_circular_initial_peer_standing"] = recursive.InitialCircularPeerStanding,
        ["recursive_circular_amplification"] = recursive.InitialCircularPeerStanding <= 1e-12
            ? 0.0
            : recursive.InitialCircularAuthority / recursive.InitialCircularPeerStanding,
        ["authority_ancestry_final_grounded_standing"] = ancestryAware.FinalGroundedStanding,
        ["authority_ancestry_final_circular_standing"] = ancestryAware.FinalCircularStanding,
        ["recursive_final_circular_standing"] = recursive.FinalCircularStanding,
        ["authority_ancestry_grounded_root_coverage"] = ancestryAware.GroundedRootCoverage,
        ["authority_ancestry_circular_root_coverage"] = ancestryAware.CircularRootCoverage,
        ["authority_ancestry_packet_count"] = ancestryAware.CommunicationPacketCount,
        ["recursive_packet_count"] = recursive.CommunicationPacketCount,
        ["direct_packet_count"] = directOnly.CommunicationPacketCount,
        ["authority_ancestry_communication_work"] = ancestryAware.CommunicationWork,
        ["recursive_communication_work"] = recursive.CommunicationWork,
        ["direct_communication_work"] = directOnly.CommunicationWork,
    };

    private static List<ExperimentAssertion> BuildAssertions(
        AuthorityAncestryScenario scenario,
        PathOutcome ancestryAware,
        PathOutcome recursive,
        PathOutcome directOnly)
    {
        var independent = AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.IndependentGrounding);
        var circular = AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.CircularAuthorityTrap);
        var expectedPackets = scenario.Cells.Length * AuthorityAncestryWorld.PeerCount * (SocialRounds + 1);
        var expectedWork = expectedPackets * PublicPacketCost;
        var recursiveAmplification = recursive.InitialCircularPeerStanding <= 1e-12
            ? 0.0
            : recursive.InitialCircularAuthority / recursive.InitialCircularPeerStanding;

        return
        [
            new ExperimentAssertion(
                "seed-generates-authority-cascade-world",
                independent >= 4 && circular >= 4,
                "Each seed must contain several contexts with independently grounded authority and several contexts where a weak authority seed can circulate through a trusted endorsement loop.",
                Math.Min(independent, circular),
                4),
            new ExperimentAssertion(
                "recursive-endorsement-amplifies-circular-authority",
                recursive.InitialCircularAuthority >= 0.80 && recursiveAmplification >= 5.0,
                "The locally reasonable recursive control must actually instantiate the failure mode: weak initial permission should become high apparent authority after circulating without new direct evidence.",
                Math.Min(recursive.InitialCircularAuthority / 0.80, recursiveAmplification / 5.0),
                1.0),
            new ExperimentAssertion(
                "authority-ancestry-preserves-grounded-opportunity",
                ancestryAware.EarlyGroundedRmse <= directOnly.EarlyGroundedRmse * 0.65,
                "Authority ancestry must not solve circularity by ignoring social standing. Independently grounded endorsement should reduce early receiver error by at least 35% relative to direct-only learning.",
                ancestryAware.EarlyGroundedRmse,
                directOnly.EarlyGroundedRmse * 0.65),
            new ExperimentAssertion(
                "authority-ancestry-discounts-circular-permission",
                ancestryAware.InitialCircularAuthority <= 0.40 && ancestryAware.InitialCircularAuthority <= recursive.InitialCircularAuthority * 0.45,
                "A weak authority root that returns through several peers must not acquire the apparent independence of several separately earned permissions.",
                Math.Max(
                    ancestryAware.InitialCircularAuthority / 0.40,
                    ancestryAware.InitialCircularAuthority / Math.Max(1e-12, recursive.InitialCircularAuthority * 0.45)),
                1.0),
            new ExperimentAssertion(
                "authority-ancestry-reduces-circular-capture",
                ancestryAware.EarlyCircularRmse <= recursive.EarlyCircularRmse * 0.88,
                "Before much receiver-owned consequence is available, ancestry-sensitive authority must reduce prediction damage from circularly amplified permission by at least 12% relative to recursive endorsement.",
                ancestryAware.EarlyCircularRmse,
                recursive.EarlyCircularRmse * 0.88),
            new ExperimentAssertion(
                "independent-grounding-remains-distinct-from-circular-authority",
                ancestryAware.InitialGroundedAuthority >= ancestryAware.InitialCircularAuthority + 0.30 && ancestryAware.InitialGroundedAuthority >= 0.55,
                "The public authority surface must preserve a material distinction between several independent direct roots and one weak root echoed through a social loop.",
                ancestryAware.InitialGroundedAuthority - ancestryAware.InitialCircularAuthority,
                0.30),
            new ExperimentAssertion(
                "direct-consequence-revokes-circular-authority",
                ancestryAware.FinalCircularStanding <= 0.08 && ancestryAware.LateCircularRmse <= 0.08,
                "Authority ancestry is only a prior. Once the receiver repeatedly experiences contradiction directly, circular permission must lose standing and late prediction error must become small.",
                Math.Max(ancestryAware.FinalCircularStanding / 0.08, ancestryAware.LateCircularRmse / 0.08),
                1.0),
            new ExperimentAssertion(
                "grounded-standing-remains-earned",
                ancestryAware.FinalGroundedStanding >= 0.82,
                "Ancestry protection must not create generalized social distrust. Candidate influence repeatedly confirmed by receiver consequence should retain strong receiver-owned standing.",
                ancestryAware.FinalGroundedStanding,
                0.82),
            new ExperimentAssertion(
                "whole-history-authority-ancestry-benefit",
                ancestryAware.Rmse <= recursive.Rmse * 0.95 && ancestryAware.Rmse <= directOnly.Rmse,
                "Across the mixed authority world, ancestry-sensitive permission should outperform recursive endorsement while retaining enough useful social opportunity to beat learning alone.",
                Math.Max(
                    ancestryAware.Rmse / Math.Max(1e-12, recursive.Rmse * 0.95),
                    ancestryAware.Rmse / Math.Max(1e-12, directOnly.Rmse)),
                1.0),
            new ExperimentAssertion(
                "bounded-authority-exchange",
                ancestryAware.CommunicationPacketCount == expectedPackets &&
                recursive.CommunicationPacketCount == expectedPackets &&
                directOnly.CommunicationPacketCount == 0 &&
                Math.Abs(ancestryAware.CommunicationWork - expectedWork) <= 1e-12 &&
                Math.Abs(recursive.CommunicationWork - expectedWork) <= 1e-12,
                "Both social treatments use the same bounded number of compact endorsement packets. The direct-only baseline receives none.",
                ancestryAware.CommunicationWork,
                expectedWork),
        ];
    }

    private static double Mean(double[,] values)
    {
        var total = 0.0;
        for (var row = 0; row < values.GetLength(0); row++)
        {
            for (var column = 0; column < values.GetLength(1); column++)
            {
                total += values[row, column];
            }
        }

        return total / values.Length;
    }

    private static double MeanPeerStanding(
        AuthorityAncestryScenario scenario,
        double[,] standing,
        AuthorityAncestryContextKind kind)
    {
        var total = 0.0;
        var count = 0;
        for (var cellIndex = 0; cellIndex < scenario.Cells.Length; cellIndex++)
        {
            if (scenario.Cells[cellIndex].ContextKind != kind)
            {
                continue;
            }

            for (var peer = 0; peer < AuthorityAncestryWorld.PeerCount; peer++)
            {
                total += standing[cellIndex, peer];
                count++;
            }
        }

        return count == 0 ? 0.0 : total / count;
    }

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

    private static string SeriesName(AuthorityMode mode) => mode switch
    {
        AuthorityMode.AuthorityAncestry => "authority-ancestry",
        AuthorityMode.RecursiveEndorsement => "recursive-endorsement",
        _ => "direct-only",
    };

    private enum AuthorityMode
    {
        AuthorityAncestry,
        RecursiveEndorsement,
        DirectOnly,
    }

    private sealed class AuthorityReceiver
    {
        private readonly AuthorityAncestryScenario _scenario;
        private readonly AuthorityMode _mode;
        private readonly double[] _localEstimate;
        private readonly int[] _localEvidence;
        private readonly double[] _sourceStanding;

        public AuthorityReceiver(AuthorityAncestryScenario scenario, AuthorityMode mode, double[] initialAuthority)
        {
            _scenario = scenario;
            _mode = mode;
            _localEstimate = new double[scenario.Cells.Length];
            _localEvidence = new int[scenario.Cells.Length];
            _sourceStanding = mode == AuthorityMode.DirectOnly ? new double[scenario.Cells.Length] : (double[])initialAuthority.Clone();
        }

        public int LocalEvidenceFor(int contextCell) => _localEvidence[contextCell];

        public double SourceStandingFor(int contextCell) => _mode == AuthorityMode.DirectOnly ? 0.0 : _sourceStanding[contextCell];

        public double Predict(int contextCell, double candidateEstimate, out double socialWeight)
        {
            var localWeight = 0.42 + Math.Min(1.0, _localEvidence[contextCell] / 10.0);
            if (_mode == AuthorityMode.DirectOnly)
            {
                socialWeight = 0.0;
                return _localEstimate[contextCell];
            }

            socialWeight = Math.Min(0.85, _sourceStanding[contextCell]);
            return ((_localEstimate[contextCell] * localWeight) + (candidateEstimate * socialWeight)) /
                   (localWeight + socialWeight);
        }

        public void ObserveDirect(int contextCell, double target, double candidateEstimate)
        {
            _localEvidence[contextCell]++;
            var learningRate = _localEvidence[contextCell] <= 8 ? 0.20 : 0.14;
            _localEstimate[contextCell] += learningRate * (target - _localEstimate[contextCell]);
            if (_mode == AuthorityMode.DirectOnly)
            {
                return;
            }

            var sourceError = Math.Abs(candidateEstimate - target);
            var quality = Math.Exp(-3.2 * sourceError);
            _sourceStanding[contextCell] += 0.20 * (quality - _sourceStanding[contextCell]);
            if (sourceError > 0.55)
            {
                _sourceStanding[contextCell] *= 0.72;
            }

            _sourceStanding[contextCell] = Math.Clamp(_sourceStanding[contextCell], 0.02, 0.98);
        }

        public double MeanFinalStanding(AuthorityAncestryContextKind kind)
        {
            if (_mode == AuthorityMode.DirectOnly)
            {
                return 0.0;
            }

            var total = 0.0;
            var count = 0;
            for (var index = 0; index < _scenario.Cells.Length; index++)
            {
                if (_scenario.Cells[index].ContextKind == kind)
                {
                    total += _sourceStanding[index];
                    count++;
                }
            }

            return count == 0 ? 0.0 : total / count;
        }

        public MindPublicState[] PublicMindState(double prediction, double target, double candidateEstimate)
        {
            var activeLocal = 0;
            var localStandingTotal = 0.0;
            for (var index = 0; index < _localEvidence.Length; index++)
            {
                if (_localEvidence[index] == 0)
                {
                    continue;
                }

                activeLocal++;
                localStandingTotal += 1.0 - Math.Exp(-_localEvidence[index] / 6.0);
            }

            var localStanding = activeLocal == 0 ? 0.0 : localStandingTotal / activeLocal;
            var socialStanding = _mode == AuthorityMode.DirectOnly ? 0.0 : _sourceStanding.Average();
            return
            [
                new MindPublicState(
                    "receiver-r",
                    activeLocal,
                    _mode == AuthorityMode.DirectOnly ? 0 : _sourceStanding.Length,
                    localStanding,
                    socialStanding,
                    prediction,
                    target,
                    Math.Abs(prediction - target)),
                new MindPublicState(
                    "candidate-source",
                    0,
                    0,
                    0.0,
                    0.0,
                    candidateEstimate,
                    target,
                    Math.Abs(candidateEstimate - target)),
            ];
        }

        public TracePublicState[] PublicTraceStates()
        {
            if (_mode == AuthorityMode.DirectOnly)
            {
                return [];
            }

            var traces = new TracePublicState[_sourceStanding.Length];
            for (var index = 0; index < traces.Length; index++)
            {
                traces[index] = new TracePublicState(
                    "receiver-r",
                    index,
                    TraceProvenance.Foreign,
                    "authority-network",
                    $"authority:c{index}",
                    _scenario.Cells[index].CandidateEstimate,
                    _sourceStanding[index],
                    _localEvidence[index],
                    1);
            }

            return traces;
        }
    }

    private sealed class AuthorityNetworkOutcome
    {
        public AuthorityNetworkOutcome(double[] receiverAuthority, double[] initialMeanStanding, double[] rootCoverage)
        {
            ReceiverAuthority = receiverAuthority;
            InitialMeanStanding = initialMeanStanding;
            RootCoverage = rootCoverage;
        }

        public double[] ReceiverAuthority { get; }

        public double[] InitialMeanStanding { get; }

        public double[] RootCoverage { get; }

        public static AuthorityNetworkOutcome Empty(int contextCount) =>
            new(new double[contextCount], new double[contextCount], new double[contextCount]);

        public double MeanAuthority(AuthorityAncestryScenario scenario, AuthorityAncestryContextKind kind) =>
            MeanForKind(scenario, ReceiverAuthority, kind);

        public double MeanInitialPeerStanding(AuthorityAncestryScenario scenario, AuthorityAncestryContextKind kind) =>
            MeanForKind(scenario, InitialMeanStanding, kind);

        public double MeanRootCoverage(AuthorityAncestryScenario scenario, AuthorityAncestryContextKind kind) =>
            MeanForKind(scenario, RootCoverage, kind);

        private static double MeanForKind(
            AuthorityAncestryScenario scenario,
            double[] values,
            AuthorityAncestryContextKind kind)
        {
            var total = 0.0;
            var count = 0;
            for (var index = 0; index < scenario.Cells.Length; index++)
            {
                if (scenario.Cells[index].ContextKind == kind)
                {
                    total += values[index];
                    count++;
                }
            }

            return count == 0 ? 0.0 : total / count;
        }
    }

    private sealed class RunningSquaredError
    {
        private double _sumSquares;

        public int Count { get; private set; }

        public double Rmse => Count == 0 ? 0.0 : Math.Sqrt(_sumSquares / Count);

        public void Add(double error)
        {
            _sumSquares += error * error;
            Count++;
        }
    }

    private sealed record PathOutcome(
        double Rmse,
        double EarlyGroundedRmse,
        double EarlyCircularRmse,
        double LateCircularRmse,
        double InitialCircularPeerStanding,
        double InitialGroundedAuthority,
        double InitialCircularAuthority,
        double FinalGroundedStanding,
        double FinalCircularStanding,
        double GroundedRootCoverage,
        double CircularRootCoverage,
        int CommunicationPacketCount,
        double CommunicationWork)
    {
        public Dictionary<string, double> ToMetrics() => new(StringComparer.Ordinal)
        {
            ["rmse"] = Rmse,
            ["early_grounded_rmse"] = EarlyGroundedRmse,
            ["early_circular_rmse"] = EarlyCircularRmse,
            ["late_circular_rmse"] = LateCircularRmse,
            ["initial_circular_peer_standing"] = InitialCircularPeerStanding,
            ["initial_grounded_authority"] = InitialGroundedAuthority,
            ["initial_circular_authority"] = InitialCircularAuthority,
            ["final_grounded_standing"] = FinalGroundedStanding,
            ["final_circular_standing"] = FinalCircularStanding,
            ["grounded_root_coverage"] = GroundedRootCoverage,
            ["circular_root_coverage"] = CircularRootCoverage,
            ["communication_packet_count"] = CommunicationPacketCount,
            ["communication_work"] = CommunicationWork,
        };
    }
}
