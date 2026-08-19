using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed class BoundedCommunicationBeforeLanguageExperiment : IExperiment
{
    private const string ExperimentName = "04-bounded-communication-before-language";
    private const int EarlyWindowTicks = 96;
    private const int LateWindowStart = 240;
    private const int DisagreementWindowTicks = 48;
    private const int SemanticSmoothingRounds = 2;
    private const double PublicSignalCost = 0.004;

    public string Name => ExperimentName;

    public string Question =>
        "Can low-dimensional typed public signals preserve useful disagreement better than early semantic negotiation that smooths peers toward a common statement before external consequence?";

    public ExperimentResult Run(ExperimentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Emit(
            ExperimentFrameKind.ExperimentStarted,
            Name,
            message: Question);

        var scenario = CommunicationBeforeLanguageWorld.CreateScenario(context.Seed);
        EmitScenario(context, scenario);
        var developedPeers = DevelopPeers(context, scenario);
        EmitPrivateHistoriesComplete(context, scenario, developedPeers);
        var sharedObservations = CommunicationBeforeLanguageWorld.CreateSharedObservations(scenario);

        var typed = RunSharedPath(
            context,
            scenario,
            "typed-signals",
            ClonePeers(developedPeers),
            sharedObservations,
            semanticSmoothing: false,
            "Peers publish bounded estimate, standing, and uncertainty signals. Their public differences remain distinct until the decision surface combines them and external consequence arrives.");

        var smoothed = RunSharedPath(
            context,
            scenario,
            "early-semantic-smoothing",
            ClonePeers(developedPeers),
            sharedObservations,
            semanticSmoothing: true,
            "Control: the same public postures undergo two peer-to-peer smoothing rounds before commitment. This models premature semantic convergence, not language in general, and does not mutate private peer state.");

        var metrics = BuildResultMetrics(scenario, typed, smoothed);
        var assertions = BuildAssertions(scenario, typed, smoothed, sharedObservations.Length);
        var passed = assertions.Count(assertion => assertion.Passed);
        var verdict = passed == assertions.Count
            ? ExperimentVerdict.Support
            : passed >= 5
                ? ExperimentVerdict.Mixed
                : ExperimentVerdict.Disconfirm;
        var interpretation = verdict switch
        {
            ExperimentVerdict.Support =>
                "Low-dimensional typed public signals preserved source-specific disagreement long enough for standing and uncertainty to shape commitment. They retained more useful dissent, spread less low-quality dissent, produced lower total error than the early semantic-smoothing control, and still converged under shared external consequence.",
            ExperimentVerdict.Mixed =>
                "Typed communication preserved some useful plural structure, but one or more preregistered boundaries on informative dissent, misleading dissent, total error, convergence, world heterogeneity, or communication economy did not hold.",
            _ =>
                "Preserving typed public differences did not provide enough advantage over early semantic smoothing in this varied social-history assay to support the claim that communication should remain structured before richer negotiation.",
        };

        var result = new ExperimentResult(Name, Question, verdict, interpretation, metrics, assertions);
        context.Emit(
            ExperimentFrameKind.ExperimentCompleted,
            Name,
            phase: "verdict",
            message: interpretation,
            completion: new ExperimentCompletion(verdict, interpretation, metrics, assertions));
        return result;
    }

    private static void EmitScenario(ExperimentContext context, CommunicationBeforeLanguageScenario scenario)
    {
        GetEvidenceRange(scenario, out var minimumEvidence, out var maximumEvidence);
        var descriptionParts = new string[scenario.Cells.Length];
        for (var index = 0; index < scenario.Cells.Length; index++)
        {
            var cell = scenario.Cells[index];
            descriptionParts[index] = $"c{cell.ContextCell}:{cell.HistoryKind}/p{cell.SalientPeerIndex}";
        }

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "scenario",
            phase: "scenario-generated",
            message: $"Seed {scenario.Seed} generated a social-development circumstance: {string.Join("; ", descriptionParts)}.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["informative_dissent_cells"] = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.InformativeDissent),
                ["misleading_dissent_cells"] = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.MisleadingDissent),
                ["complementary_cells"] = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.Complementary),
                ["convergent_cells"] = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.Convergent),
                ["minimum_private_evidence"] = minimumEvidence,
                ["maximum_private_evidence"] = maximumEvidence,
                ["private_evidence_span"] = maximumEvidence - minimumEvidence,
            });
    }

    private static CommunicationPeer[] DevelopPeers(
        ExperimentContext context,
        CommunicationBeforeLanguageScenario scenario)
    {
        var peers = new CommunicationPeer[CommunicationBeforeLanguageWorld.PeerCount];
        for (var peerIndex = 0; peerIndex < peers.Length; peerIndex++)
        {
            var peer = new CommunicationPeer($"peer-{peerIndex + 1}", scenario.Cells.Length);
            peers[peerIndex] = peer;
            context.Emit(
                ExperimentFrameKind.PhaseChanged,
                ExperimentName,
                peer.MindId,
                phase: "peer-private-development",
                message: $"{peer.MindId} develops from its seed-specific private history without seeing either peer's private state.");

            var errors = new ErrorAccumulator();
            var observations = CommunicationBeforeLanguageWorld.CreatePrivateObservations(scenario, peerIndex);
            for (var tick = 0; tick < observations.Length; tick++)
            {
                var observation = observations[tick];
                var prediction = peer.Predict(observation.ContextCell);
                var error = prediction - observation.Target;
                errors.Add(error);
                peer.ObservePrivate(observation.ContextCell, observation.Target);
                var cell = scenario.Cells[observation.ContextCell];

                context.Emit(
                    ExperimentFrameKind.MetricSample,
                    ExperimentName,
                    peer.MindId,
                    tick,
                    "peer-private-development",
                    metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                    {
                        ["context_cell"] = observation.ContextCell,
                        ["history_kind"] = (double)cell.HistoryKind,
                        ["prediction"] = prediction,
                        ["target"] = observation.Target,
                        ["absolute_error"] = Math.Abs(error),
                        ["rolling_rmse"] = errors.Rmse,
                        ["local_standing"] = peer.StandingFor(observation.ContextCell),
                        ["uncertainty"] = peer.UncertaintyFor(observation.ContextCell),
                        ["private_evidence"] = peer.EvidenceFor(observation.ContextCell),
                    });

                if (tick % 48 == 47 || tick == observations.Length - 1)
                {
                    context.Emit(
                        ExperimentFrameKind.StateSnapshot,
                        ExperimentName,
                        peer.MindId,
                        tick,
                        "peer-private-development",
                        minds: [peer.PublicMindState()],
                        traces: peer.PublicTraceStates());
                }
            }
        }

        return peers;
    }

    private static void EmitPrivateHistoriesComplete(
        ExperimentContext context,
        CommunicationBeforeLanguageScenario scenario,
        CommunicationPeer[] peers)
    {
        var meanDisagreement = MeanScenarioDisagreement(scenario, peers);
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "peers",
            phase: "private-histories-complete",
            message: "All three private histories are developed. Public communication can now expose compact postures without opening those interiors.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["private_mean_disagreement"] = meanDisagreement,
                ["peer_1_mean_standing"] = peers[0].MeanStanding(),
                ["peer_2_mean_standing"] = peers[1].MeanStanding(),
                ["peer_3_mean_standing"] = peers[2].MeanStanding(),
                ["informative_salient_standing_advantage"] = MeanSalientStandingGap(scenario, peers, CommunicationHistoryKind.InformativeDissent, salientShouldLead: true),
                ["misleading_salient_standing_deficit"] = MeanSalientStandingGap(scenario, peers, CommunicationHistoryKind.MisleadingDissent, salientShouldLead: false),
            },
            minds: PublicMindStates(peers),
            traces: PublicTraceStates(peers));
    }

    private static PathOutcome RunSharedPath(
        ExperimentContext context,
        CommunicationBeforeLanguageScenario scenario,
        string series,
        CommunicationPeer[] peers,
        CommunicationSharedObservation[] observations,
        bool semanticSmoothing,
        string description)
    {
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            series,
            phase: semanticSmoothing ? "early-semantic-smoothing" : "typed-communication",
            message: description);

        var allErrors = new ErrorAccumulator();
        var earlyErrors = new ErrorAccumulator();
        var lateErrors = new ErrorAccumulator();
        var informativeEarlyErrors = new ErrorAccumulator();
        var misleadingEarlyErrors = new ErrorAccumulator();
        var publicDisagreements = new double[observations.Length];
        var rawDisagreements = new double[observations.Length];
        var communicationPackets = 0;
        var communicationWork = 0.0;

        for (var tick = 0; tick < observations.Length; tick++)
        {
            var observation = observations[tick];
            var cell = scenario.Cells[observation.ContextCell];
            var postures = CreatePublicPostures(peers, observation.ContextCell);
            var rawDisagreement = RangeOfEstimates(postures);
            rawDisagreements[tick] = rawDisagreement;

            var statements = new double[postures.Length];
            for (var index = 0; index < postures.Length; index++)
            {
                statements[index] = postures[index].Estimate;
            }

            communicationPackets += postures.Length;
            communicationWork += postures.Length * PublicSignalCost;
            if (semanticSmoothing)
            {
                for (var round = 0; round < SemanticSmoothingRounds; round++)
                {
                    statements = SmoothStatements(postures, statements);
                    communicationPackets += postures.Length;
                    communicationWork += postures.Length * PublicSignalCost;
                }
            }

            var prediction = CombineStatements(postures, statements);
            var publicDisagreement = RangeOfValues(statements);
            publicDisagreements[tick] = publicDisagreement;
            var error = prediction - observation.Target;
            allErrors.Add(error);
            if (tick < EarlyWindowTicks)
            {
                earlyErrors.Add(error);
                if (cell.HistoryKind == CommunicationHistoryKind.InformativeDissent)
                {
                    informativeEarlyErrors.Add(error);
                }
                else if (cell.HistoryKind == CommunicationHistoryKind.MisleadingDissent)
                {
                    misleadingEarlyErrors.Add(error);
                }
            }

            if (tick >= LateWindowStart)
            {
                lateErrors.Add(error);
            }

            var meanStanding = MeanPostureStanding(postures);
            var meanUncertainty = MeanPostureUncertainty(postures);
            for (var peerIndex = 0; peerIndex < peers.Length; peerIndex++)
            {
                peers[peerIndex].ObserveShared(observation.ContextCell, observation.Target);
            }

            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                series,
                tick,
                semanticSmoothing ? "early-semantic-smoothing" : "typed-communication",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["context_cell"] = observation.ContextCell,
                    ["history_kind"] = (double)cell.HistoryKind,
                    ["prediction"] = prediction,
                    ["target"] = observation.Target,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = allErrors.Rmse,
                    ["raw_peer_disagreement"] = rawDisagreement,
                    ["public_disagreement"] = publicDisagreement,
                    ["mean_standing"] = meanStanding,
                    ["mean_uncertainty"] = meanUncertainty,
                    ["communication_work"] = communicationWork,
                });

            if (tick % 48 == 47 || tick == observations.Length - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    ExperimentName,
                    series,
                    tick,
                    semanticSmoothing ? "early-semantic-smoothing" : "typed-communication",
                    minds: PublicMindStates(peers),
                    traces: PublicTraceStates(peers));
            }
        }

        var outcome = new PathOutcome(
            series,
            allErrors.Rmse,
            earlyErrors.Rmse,
            lateErrors.Rmse,
            informativeEarlyErrors.Rmse,
            misleadingEarlyErrors.Rmse,
            MeanWindow(publicDisagreements, fromStart: true),
            MeanWindow(publicDisagreements, fromStart: false),
            MeanAll(publicDisagreements),
            MeanWindow(rawDisagreements, fromStart: true),
            communicationPackets,
            communicationWork);
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            series,
            observations.Length,
            "path-complete",
            message: $"{series} completed with RMSE {outcome.Rmse:0.000000}, early informative-dissent RMSE {outcome.InformativeDissentEarlyRmse:0.000000}, and final public disagreement {outcome.FinalPublicDisagreement:0.000000}.",
            metrics: outcome.ToMetrics(),
            minds: PublicMindStates(peers),
            traces: PublicTraceStates(peers));
        return outcome;
    }

    private static Dictionary<string, double> BuildResultMetrics(
        CommunicationBeforeLanguageScenario scenario,
        PathOutcome typed,
        PathOutcome smoothed)
    {
        GetEvidenceRange(scenario, out var minimumEvidence, out var maximumEvidence);
        return new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
            ["informative_dissent_cells"] = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.InformativeDissent),
            ["misleading_dissent_cells"] = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.MisleadingDissent),
            ["complementary_cells"] = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.Complementary),
            ["convergent_cells"] = CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.Convergent),
            ["private_evidence_span"] = maximumEvidence - minimumEvidence,
            ["typed_rmse"] = typed.Rmse,
            ["semantic_smoothed_rmse"] = smoothed.Rmse,
            ["typed_early_rmse"] = typed.EarlyRmse,
            ["semantic_smoothed_early_rmse"] = smoothed.EarlyRmse,
            ["typed_late_rmse"] = typed.LateRmse,
            ["semantic_smoothed_late_rmse"] = smoothed.LateRmse,
            ["typed_informative_dissent_early_rmse"] = typed.InformativeDissentEarlyRmse,
            ["semantic_smoothed_informative_dissent_early_rmse"] = smoothed.InformativeDissentEarlyRmse,
            ["typed_misleading_dissent_early_rmse"] = typed.MisleadingDissentEarlyRmse,
            ["semantic_smoothed_misleading_dissent_early_rmse"] = smoothed.MisleadingDissentEarlyRmse,
            ["typed_initial_public_disagreement"] = typed.InitialPublicDisagreement,
            ["semantic_smoothed_initial_public_disagreement"] = smoothed.InitialPublicDisagreement,
            ["typed_final_public_disagreement"] = typed.FinalPublicDisagreement,
            ["semantic_smoothed_final_public_disagreement"] = smoothed.FinalPublicDisagreement,
            ["typed_initial_raw_disagreement"] = typed.InitialRawDisagreement,
            ["typed_communication_work"] = typed.CommunicationWork,
            ["semantic_smoothed_communication_work"] = smoothed.CommunicationWork,
            ["typed_packet_count"] = typed.CommunicationPacketCount,
            ["semantic_smoothed_packet_count"] = smoothed.CommunicationPacketCount,
        };
    }

    private static List<ExperimentAssertion> BuildAssertions(
        CommunicationBeforeLanguageScenario scenario,
        PathOutcome typed,
        PathOutcome smoothed,
        int observationCount)
    {
        GetEvidenceRange(scenario, out var minimumEvidence, out var maximumEvidence);
        var typedExpectedPackets = observationCount * CommunicationBeforeLanguageWorld.PeerCount;
        var smoothedExpectedPackets = typedExpectedPackets * (SemanticSmoothingRounds + 1);
        return
        [
            new ExperimentAssertion(
                "seed-generates-social-circumstance",
                CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.InformativeDissent) >= 2 &&
                CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.MisleadingDissent) >= 2 &&
                CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.Complementary) >= 2 &&
                CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.Convergent) >= 2 &&
                maximumEvidence - minimumEvidence >= 30,
                "A replication seed must produce a heterogeneous social history containing informative dissent, misleading dissent, complementary perspectives, convergence, and materially unequal evidence depths.",
                maximumEvidence - minimumEvidence,
                30.0),
            new ExperimentAssertion(
                "typed-surface-preserves-public-disagreement",
                typed.InitialPublicDisagreement >= 0.18 &&
                smoothed.InitialPublicDisagreement <= typed.InitialPublicDisagreement * 0.20,
                "The typed surface must keep meaningful public difference visible while the early semantic-smoothing control demonstrably compresses that difference before consequence.",
                typed.InitialPublicDisagreement,
                0.18),
            new ExperimentAssertion(
                "informative-dissent-survives",
                typed.InformativeDissentEarlyRmse <= smoothed.InformativeDissentEarlyRmse * 0.97,
                "On contexts where the better-supported minority perspective is locally correct, preserving separate typed postures must reduce early error relative to smoothing the public statements together.",
                typed.InformativeDissentEarlyRmse,
                smoothed.InformativeDissentEarlyRmse * 0.97),
            new ExperimentAssertion(
                "misleading-dissent-remains-disciplined",
                typed.MisleadingDissentEarlyRmse <= smoothed.MisleadingDissentEarlyRmse * 1.05,
                "Preserving disagreement must not mean blindly privileging dissent. Source-specific standing and uncertainty should keep a sparse noisy minority from making the typed path materially worse than the smoothing control.",
                typed.MisleadingDissentEarlyRmse,
                smoothed.MisleadingDissentEarlyRmse * 1.05),
            new ExperimentAssertion(
                "whole-history-benefit",
                typed.Rmse <= smoothed.Rmse * 0.97,
                "The typed surface must produce a measurable whole-history error advantage rather than winning only on a specially scored minority subset.",
                typed.Rmse,
                smoothed.Rmse * 0.97),
            new ExperimentAssertion(
                "shared-consequence-converges",
                typed.LateRmse <= 0.06 &&
                smoothed.LateRmse <= 0.06 &&
                typed.FinalPublicDisagreement <= 0.05,
                "Preserved public difference must remain revisable. Once all peers live through the same external consequence, both conditions should become accurate and typed disagreement should substantially settle.",
                typed.FinalPublicDisagreement,
                0.05),
            new ExperimentAssertion(
                "typed-communication-remains-bounded",
                typed.CommunicationPacketCount == typedExpectedPackets &&
                smoothed.CommunicationPacketCount == smoothedExpectedPackets &&
                typed.CommunicationWork <= 4.10 &&
                smoothed.CommunicationWork > typed.CommunicationWork,
                "The typed path may publish only one compact posture per peer and observation. The richer negotiation control pays explicitly for two extra smoothing rounds rather than receiving free social computation.",
                typed.CommunicationWork,
                4.10),
        ];
    }

    private static PublicPosture[] CreatePublicPostures(CommunicationPeer[] peers, int contextCell)
    {
        var postures = new PublicPosture[peers.Length];
        for (var index = 0; index < peers.Length; index++)
        {
            postures[index] = peers[index].CreatePublicPosture(contextCell);
        }

        return postures;
    }

    private static double[] SmoothStatements(PublicPosture[] postures, double[] currentStatements)
    {
        var next = new double[currentStatements.Length];
        for (var speakerIndex = 0; speakerIndex < postures.Length; speakerIndex++)
        {
            var numerator = 0.0;
            var denominator = 0.0;
            var otherStanding = 0.0;
            var otherCount = 0;
            for (var peerIndex = 0; peerIndex < postures.Length; peerIndex++)
            {
                if (peerIndex == speakerIndex)
                {
                    continue;
                }

                var peer = postures[peerIndex];
                var socialWeight = Math.Max(0.05, peer.Standing * (1.0 - (0.35 * peer.Uncertainty)));
                numerator += currentStatements[peerIndex] * socialWeight;
                denominator += socialWeight;
                otherStanding += peer.Standing;
                otherCount++;
            }

            var center = denominator <= 1e-12 ? currentStatements[speakerIndex] : numerator / denominator;
            var meanOtherStanding = otherCount == 0 ? 0.0 : otherStanding / otherCount;
            var assimilation = 0.44 + (0.22 * meanOtherStanding);
            next[speakerIndex] = currentStatements[speakerIndex] + (assimilation * (center - currentStatements[speakerIndex]));
        }

        return next;
    }

    private static double CombineStatements(PublicPosture[] postures, double[] statements)
    {
        var numerator = 0.0;
        var denominator = 0.0;
        for (var index = 0; index < postures.Length; index++)
        {
            var weight = PublicWeight(postures[index]);
            numerator += statements[index] * weight;
            denominator += weight;
        }

        return denominator <= 1e-12 ? 0.0 : numerator / denominator;
    }

    private static double PublicWeight(PublicPosture posture) =>
        Math.Max(0.01, posture.Standing * (1.0 - (0.65 * posture.Uncertainty)));

    private static double RangeOfEstimates(PublicPosture[] postures)
    {
        var minimum = double.PositiveInfinity;
        var maximum = double.NegativeInfinity;
        for (var index = 0; index < postures.Length; index++)
        {
            minimum = Math.Min(minimum, postures[index].Estimate);
            maximum = Math.Max(maximum, postures[index].Estimate);
        }

        return maximum - minimum;
    }

    private static double RangeOfValues(double[] values)
    {
        var minimum = double.PositiveInfinity;
        var maximum = double.NegativeInfinity;
        for (var index = 0; index < values.Length; index++)
        {
            minimum = Math.Min(minimum, values[index]);
            maximum = Math.Max(maximum, values[index]);
        }

        return maximum - minimum;
    }

    private static double MeanPostureStanding(PublicPosture[] postures)
    {
        var total = 0.0;
        for (var index = 0; index < postures.Length; index++)
        {
            total += postures[index].Standing;
        }

        return postures.Length == 0 ? 0.0 : total / postures.Length;
    }

    private static double MeanPostureUncertainty(PublicPosture[] postures)
    {
        var total = 0.0;
        for (var index = 0; index < postures.Length; index++)
        {
            total += postures[index].Uncertainty;
        }

        return postures.Length == 0 ? 0.0 : total / postures.Length;
    }

    private static double MeanWindow(double[] values, bool fromStart)
    {
        var count = Math.Min(DisagreementWindowTicks, values.Length);
        if (count == 0)
        {
            return 0.0;
        }

        var start = fromStart ? 0 : values.Length - count;
        var total = 0.0;
        for (var index = start; index < start + count; index++)
        {
            total += values[index];
        }

        return total / count;
    }

    private static double MeanAll(double[] values)
    {
        if (values.Length == 0)
        {
            return 0.0;
        }

        var total = 0.0;
        for (var index = 0; index < values.Length; index++)
        {
            total += values[index];
        }

        return total / values.Length;
    }

    private static CommunicationPeer[] ClonePeers(CommunicationPeer[] peers)
    {
        var clones = new CommunicationPeer[peers.Length];
        for (var index = 0; index < peers.Length; index++)
        {
            clones[index] = peers[index].Clone();
        }

        return clones;
    }

    private static MindPublicState[] PublicMindStates(CommunicationPeer[] peers)
    {
        var states = new MindPublicState[peers.Length];
        for (var index = 0; index < peers.Length; index++)
        {
            states[index] = peers[index].PublicMindState();
        }

        return states;
    }

    private static TracePublicState[] PublicTraceStates(CommunicationPeer[] peers)
    {
        var traces = new List<TracePublicState>();
        for (var index = 0; index < peers.Length; index++)
        {
            traces.AddRange(peers[index].PublicTraceStates());
        }

        return traces.ToArray();
    }

    private static double MeanScenarioDisagreement(
        CommunicationBeforeLanguageScenario scenario,
        CommunicationPeer[] peers)
    {
        var total = 0.0;
        for (var contextCell = 0; contextCell < scenario.Cells.Length; contextCell++)
        {
            total += RangeOfEstimates(CreatePublicPostures(peers, contextCell));
        }

        return scenario.Cells.Length == 0 ? 0.0 : total / scenario.Cells.Length;
    }


    private static double MeanSalientStandingGap(
        CommunicationBeforeLanguageScenario scenario,
        CommunicationPeer[] peers,
        CommunicationHistoryKind kind,
        bool salientShouldLead)
    {
        var count = 0;
        var total = 0.0;
        for (var contextCell = 0; contextCell < scenario.Cells.Length; contextCell++)
        {
            var cell = scenario.Cells[contextCell];
            if (cell.HistoryKind != kind)
            {
                continue;
            }

            var salientStanding = peers[cell.SalientPeerIndex].StandingFor(contextCell);
            var otherStanding = 0.0;
            var otherCount = 0;
            for (var peerIndex = 0; peerIndex < peers.Length; peerIndex++)
            {
                if (peerIndex == cell.SalientPeerIndex)
                {
                    continue;
                }

                otherStanding += peers[peerIndex].StandingFor(contextCell);
                otherCount++;
            }

            var meanOtherStanding = otherCount == 0 ? 0.0 : otherStanding / otherCount;
            total += salientShouldLead
                ? salientStanding - meanOtherStanding
                : meanOtherStanding - salientStanding;
            count++;
        }

        return count == 0 ? 0.0 : total / count;
    }

    private static void GetEvidenceRange(
        CommunicationBeforeLanguageScenario scenario,
        out int minimumEvidence,
        out int maximumEvidence)
    {
        minimumEvidence = int.MaxValue;
        maximumEvidence = int.MinValue;
        for (var cellIndex = 0; cellIndex < scenario.Cells.Length; cellIndex++)
        {
            var histories = scenario.Cells[cellIndex].PeerHistories;
            for (var peerIndex = 0; peerIndex < histories.Length; peerIndex++)
            {
                minimumEvidence = Math.Min(minimumEvidence, histories[peerIndex].EvidenceCount);
                maximumEvidence = Math.Max(maximumEvidence, histories[peerIndex].EvidenceCount);
            }
        }
    }

    private sealed record PublicPosture(
        string SourceMindId,
        int ContextCell,
        double Estimate,
        double Standing,
        double Uncertainty,
        int EvidenceCount);

    private sealed record PathOutcome(
        string Series,
        double Rmse,
        double EarlyRmse,
        double LateRmse,
        double InformativeDissentEarlyRmse,
        double MisleadingDissentEarlyRmse,
        double InitialPublicDisagreement,
        double FinalPublicDisagreement,
        double MeanPublicDisagreement,
        double InitialRawDisagreement,
        int CommunicationPacketCount,
        double CommunicationWork)
    {
        public Dictionary<string, double> ToMetrics() => new(StringComparer.Ordinal)
        {
            ["rmse"] = Rmse,
            ["early_rmse"] = EarlyRmse,
            ["late_rmse"] = LateRmse,
            ["informative_dissent_early_rmse"] = InformativeDissentEarlyRmse,
            ["misleading_dissent_early_rmse"] = MisleadingDissentEarlyRmse,
            ["initial_public_disagreement"] = InitialPublicDisagreement,
            ["final_public_disagreement"] = FinalPublicDisagreement,
            ["mean_public_disagreement"] = MeanPublicDisagreement,
            ["initial_raw_disagreement"] = InitialRawDisagreement,
            ["communication_packet_count"] = CommunicationPacketCount,
            ["communication_work"] = CommunicationWork,
        };
    }

    private sealed class CommunicationPeer
    {
        private readonly double[] _estimates;
        private readonly int[] _evidence;
        private readonly double[] _means;
        private readonly double[] _m2;
        private readonly int[] _sharedEvidence;

        public CommunicationPeer(string mindId, int contextCount)
        {
            MindId = mindId;
            _estimates = new double[contextCount];
            _evidence = new int[contextCount];
            _means = new double[contextCount];
            _m2 = new double[contextCount];
            _sharedEvidence = new int[contextCount];
        }

        public string MindId { get; }

        public double LastPrediction { get; private set; }

        public double LastTarget { get; private set; }

        public double LastAbsoluteError { get; private set; }

        public double Predict(int contextCell)
        {
            LastPrediction = _estimates[contextCell];
            return LastPrediction;
        }

        public void ObservePrivate(int contextCell, double target)
        {
            Guard.Finite(target, nameof(target));
            LastTarget = target;
            LastAbsoluteError = Math.Abs(LastPrediction - target);
            UpdateStatistics(contextCell, target);
            var evidence = _evidence[contextCell];
            if (evidence == 1)
            {
                _estimates[contextCell] = target;
                return;
            }

            var learningRate = Math.Clamp(0.65 / Math.Sqrt(evidence + 3.0), 0.08, 0.28);
            _estimates[contextCell] += learningRate * (target - _estimates[contextCell]);
        }

        public void ObserveShared(int contextCell, double target)
        {
            Guard.Finite(target, nameof(target));
            LastPrediction = _estimates[contextCell];
            LastTarget = target;
            LastAbsoluteError = Math.Abs(LastPrediction - target);
            _sharedEvidence[contextCell]++;
            var learningRate = Math.Max(0.09, 0.26 / Math.Sqrt(1.0 + (0.12 * _sharedEvidence[contextCell])));
            _estimates[contextCell] += learningRate * (target - _estimates[contextCell]);
            UpdateStatistics(contextCell, target);
        }

        public int EvidenceFor(int contextCell) => _evidence[contextCell];

        public double StandingFor(int contextCell)
        {
            var maturity = 1.0 - Math.Exp(-_evidence[contextCell] / 14.0);
            var stability = 1.0 / (1.0 + (10.0 * VarianceFor(contextCell)));
            return Math.Clamp(0.15 + (0.85 * maturity * stability), 0.0, 1.0);
        }

        public double UncertaintyFor(int contextCell)
        {
            var scarcity = Math.Exp(-_evidence[contextCell] / 12.0);
            var noise = Math.Min(1.0, Math.Sqrt(Math.Max(0.0, VarianceFor(contextCell))) / 0.30);
            return Math.Clamp((0.55 * scarcity) + (0.45 * noise), 0.0, 1.0);
        }

        public double MeanStanding()
        {
            var count = 0;
            var total = 0.0;
            for (var contextCell = 0; contextCell < _evidence.Length; contextCell++)
            {
                if (_evidence[contextCell] == 0)
                {
                    continue;
                }

                count++;
                total += StandingFor(contextCell);
            }

            return count == 0 ? 0.0 : total / count;
        }

        public PublicPosture CreatePublicPosture(int contextCell) => new(
            MindId,
            contextCell,
            _estimates[contextCell],
            StandingFor(contextCell),
            UncertaintyFor(contextCell),
            _evidence[contextCell]);

        public CommunicationPeer Clone()
        {
            var clone = new CommunicationPeer(MindId, _estimates.Length)
            {
                LastPrediction = LastPrediction,
                LastTarget = LastTarget,
                LastAbsoluteError = LastAbsoluteError,
            };
            Array.Copy(_estimates, clone._estimates, _estimates.Length);
            Array.Copy(_evidence, clone._evidence, _evidence.Length);
            Array.Copy(_means, clone._means, _means.Length);
            Array.Copy(_m2, clone._m2, _m2.Length);
            Array.Copy(_sharedEvidence, clone._sharedEvidence, _sharedEvidence.Length);
            return clone;
        }

        public MindPublicState PublicMindState()
        {
            var localCount = 0;
            for (var contextCell = 0; contextCell < _evidence.Length; contextCell++)
            {
                if (_evidence[contextCell] > 0)
                {
                    localCount++;
                }
            }

            return new MindPublicState(
                MindId,
                localCount,
                0,
                MeanStanding(),
                0.0,
                LastPrediction,
                LastTarget,
                LastAbsoluteError);
        }

        public TracePublicState[] PublicTraceStates()
        {
            var traces = new List<TracePublicState>(_evidence.Length);
            for (var contextCell = 0; contextCell < _evidence.Length; contextCell++)
            {
                if (_evidence[contextCell] == 0)
                {
                    continue;
                }

                traces.Add(new TracePublicState(
                    MindId,
                    contextCell,
                    TraceProvenance.Direct,
                    MindId,
                    $"{MindId}:{contextCell}",
                    _estimates[contextCell],
                    StandingFor(contextCell),
                    _evidence[contextCell],
                    0));
            }

            return traces.ToArray();
        }

        private void UpdateStatistics(int contextCell, double target)
        {
            var evidence = _evidence[contextCell] + 1;
            var delta = target - _means[contextCell];
            _means[contextCell] += delta / evidence;
            _m2[contextCell] += delta * (target - _means[contextCell]);
            _evidence[contextCell] = evidence;
        }

        private double VarianceFor(int contextCell) =>
            _evidence[contextCell] > 1 ? _m2[contextCell] / (_evidence[contextCell] - 1) : 0.25;
    }
}
