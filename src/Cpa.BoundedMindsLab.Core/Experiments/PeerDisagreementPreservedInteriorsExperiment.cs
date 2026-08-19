using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed class PeerDisagreementPreservedInteriorsExperiment : IExperiment
{
    private const string ExperimentName = "02-peer-disagreement-preserved-interiors";
    private const int EarlyWindowTicks = 64;
    private const int LateWindowStart = 128;
    private const int DisagreementWindowTicks = 32;
    private const double PublicPostureCost = 0.008;

    public string Name => ExperimentName;

    public string Question =>
        "Does preserving independent private histories improve later correction when bounded peers disagree, compared with collapsing their state into synchronized consensus?";

    public ExperimentResult Run(ExperimentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Emit(
            ExperimentFrameKind.ExperimentStarted,
            Name,
            message: Question);

        var peerA = DevelopPeer(context, "peer-a", peerA: true);
        var peerB = DevelopPeer(context, "peer-b", peerA: false);
        var privateComplementarity = MeasurePrivateComplementarity(peerA, peerB);
        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            Name,
            "peers",
            phase: "private-histories-complete",
            message: "Both peers now carry strong but partially conflicting private histories. Their interiors remain separate.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["private_best_peer_rmse"] = privateComplementarity.BestPeerRmse,
                ["private_mean_disagreement"] = privateComplementarity.MeanDisagreement,
            },
            minds: [peerA.PublicMindState(), peerB.PublicMindState()]);

        var sharedObservations = PeerDisagreementWorld.CreateSharedObservations(context.Seed);
        var preserved = RunSharedPath(
            context,
            "preserved-interiors",
            peerA.Clone("peer-a"),
            peerB.Clone("peer-b"),
            sharedObservations,
            "Peers retain distinct private hypotheses. Only compact public prediction-and-standing postures cross the boundary.");

        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            Name,
            "synchronized-control",
            phase: "synchronization-control",
            message: "Control intervention: collapse both private states into the same consensus before shared consequence. This invasive synchronization is an experimental contrast, not proposed architecture.");
        var synchronizedPair = PeerMind.CreateSynchronizedControl(peerA, peerB);
        var synchronized = RunSharedPath(
            context,
            "synchronized-control",
            synchronizedPair.Left,
            synchronizedPair.Right,
            sharedObservations,
            "Both peers begin shared consequence from the same synchronized state, removing their previously distinct error structure.");

        var metrics = BuildResultMetrics(privateComplementarity, preserved, synchronized);
        var assertions = BuildAssertions(privateComplementarity, preserved, synchronized, sharedObservations.Length);
        var passed = assertions.Count(assertion => assertion.Passed);
        var verdict = passed == assertions.Count
            ? ExperimentVerdict.Support
            : passed >= 4
                ? ExperimentVerdict.Mixed
                : ExperimentVerdict.Disconfirm;
        var interpretation = verdict switch
        {
            ExperimentVerdict.Support =>
                "Preserving distinct private histories retained useful alternative hypotheses long enough for shared consequence to assign them different standing. The preserved peers corrected faster and with less total error than the synchronized control, then converged as shared evidence accumulated.",
            ExperimentVerdict.Mixed =>
                "Independent private histories retained some useful corrective structure, but one or more preregistered boundaries on early benefit, total benefit, convergence, complementarity, or public communication did not hold.",
            _ =>
                "Preserved disagreement did not provide enough corrective value over synchronized consensus in this assay to support the claim that independent error structure is computationally useful.",
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

    private static PeerMind DevelopPeer(ExperimentContext context, string mindId, bool peerA)
    {
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            mindId,
            phase: peerA ? "peer-a-private-development" : "peer-b-private-development",
            message: $"{mindId} develops only from its own private local history.");

        var mind = new PeerMind(mindId);
        var errors = new ErrorAccumulator();
        var observations = PeerDisagreementWorld.CreatePrivateObservations(context.Seed, peerA);
        for (var tick = 0; tick < observations.Length; tick++)
        {
            var observation = observations[tick];
            var prediction = mind.Predict(observation.ContextCell);
            var error = prediction - observation.Target;
            errors.Add(error);
            mind.ObservePrivate(observation.ContextCell, observation.Target);

            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                mindId,
                tick,
                peerA ? "peer-a-private-development" : "peer-b-private-development",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["context_cell"] = observation.ContextCell,
                    ["prediction"] = prediction,
                    ["target"] = observation.Target,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = errors.Rmse,
                    ["local_standing"] = mind.LocalStandingFor(observation.ContextCell),
                    ["private_evidence"] = mind.PrivateEvidenceFor(observation.ContextCell),
                });

            if (tick % 32 == 31 || tick == observations.Length - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    ExperimentName,
                    mindId,
                    tick,
                    peerA ? "peer-a-private-development" : "peer-b-private-development",
                    minds: [mind.PublicMindState()],
                    traces: mind.PublicTraceStates());
            }
        }

        return mind;
    }

    private static PathOutcome RunSharedPath(
        ExperimentContext context,
        string series,
        PeerMind left,
        PeerMind right,
        PeerObservation[] observations,
        string description)
    {
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            series,
            phase: "shared-consequence",
            message: description);

        var allErrors = new ErrorAccumulator();
        var earlyErrors = new ErrorAccumulator();
        var lateErrors = new ErrorAccumulator();
        var earlyBestPeerErrors = new ErrorAccumulator();
        var disagreements = new List<double>(observations.Length);
        var communicationPackets = 0;

        for (var tick = 0; tick < observations.Length; tick++)
        {
            var observation = observations[tick];
            var leftPosture = left.CreatePublicPosture(observation.ContextCell);
            var rightPosture = right.CreatePublicPosture(observation.ContextCell);
            communicationPackets += 2;
            var prediction = NegotiatePublicPrediction(leftPosture, rightPosture);
            var error = prediction - observation.Target;
            var disagreement = Math.Abs(leftPosture.Prediction - rightPosture.Prediction);
            var bestPeerError = Math.Min(
                Math.Abs(leftPosture.Prediction - observation.Target),
                Math.Abs(rightPosture.Prediction - observation.Target));

            allErrors.Add(error);
            disagreements.Add(disagreement);
            if (tick < EarlyWindowTicks)
            {
                earlyErrors.Add(error);
                earlyBestPeerErrors.Add(bestPeerError);
            }

            if (tick >= LateWindowStart)
            {
                lateErrors.Add(error);
            }

            left.ObserveShared(observation.ContextCell, observation.Target);
            right.ObserveShared(observation.ContextCell, observation.Target);
            var communicationWork = communicationPackets * PublicPostureCost;

            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                series,
                tick,
                "shared-consequence",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["context_cell"] = observation.ContextCell,
                    ["prediction"] = prediction,
                    ["target"] = observation.Target,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = allErrors.Rmse,
                    ["peer_a_prediction"] = leftPosture.Prediction,
                    ["peer_b_prediction"] = rightPosture.Prediction,
                    ["peer_disagreement"] = disagreement,
                    ["peer_a_public_standing"] = leftPosture.Standing,
                    ["peer_b_public_standing"] = rightPosture.Standing,
                    ["best_peer_absolute_error"] = bestPeerError,
                    ["communication_work"] = communicationWork,
                });

            if (tick % 16 == 15 || tick == observations.Length - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    ExperimentName,
                    series,
                    tick,
                    "shared-consequence",
                    minds: [left.PublicMindState(), right.PublicMindState()],
                    traces: left.PublicTraceStates().Concat(right.PublicTraceStates()).ToArray());
            }
        }

        var initialDisagreement = disagreements.Take(Math.Min(DisagreementWindowTicks, disagreements.Count)).Average();
        var finalDisagreement = disagreements.Skip(Math.Max(0, disagreements.Count - DisagreementWindowTicks)).Average();
        var outcome = new PathOutcome(
            series,
            allErrors.Rmse,
            earlyErrors.Rmse,
            lateErrors.Rmse,
            initialDisagreement,
            finalDisagreement,
            disagreements.Average(),
            earlyBestPeerErrors.Rmse,
            communicationPackets,
            communicationPackets * PublicPostureCost);

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            series,
            observations.Length - 1,
            "path-complete",
            message: $"{series} completed with RMSE {outcome.Rmse:0.000000} and final disagreement {outcome.FinalDisagreement:0.000000}.",
            metrics: outcome.ToMetrics(),
            minds: [left.PublicMindState(), right.PublicMindState()],
            traces: left.PublicTraceStates().Concat(right.PublicTraceStates()).ToArray());
        return outcome;
    }

    private static ComplementarityOutcome MeasurePrivateComplementarity(PeerMind peerA, PeerMind peerB)
    {
        var bestErrors = new ErrorAccumulator();
        var disagreements = new double[PeerDisagreementWorld.ContextCount];
        for (var cell = 0; cell < PeerDisagreementWorld.ContextCount; cell++)
        {
            var target = PeerDisagreementWorld.SharedTarget(cell);
            var predictionA = peerA.Predict(cell);
            var predictionB = peerB.Predict(cell);
            bestErrors.Add(Math.Min(Math.Abs(predictionA - target), Math.Abs(predictionB - target)));
            disagreements[cell] = Math.Abs(predictionA - predictionB);
        }

        return new ComplementarityOutcome(bestErrors.Rmse, disagreements.Average());
    }

    private static double NegotiatePublicPrediction(PeerPublicPosture left, PeerPublicPosture right)
    {
        var leftWeight = Math.Max(0.04, left.Standing);
        var rightWeight = Math.Max(0.04, right.Standing);
        return ((left.Prediction * leftWeight) + (right.Prediction * rightWeight)) / (leftWeight + rightWeight);
    }

    private static Dictionary<string, double> BuildResultMetrics(
        ComplementarityOutcome complementarity,
        PathOutcome preserved,
        PathOutcome synchronized)
    {
        return new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["private_best_peer_rmse"] = complementarity.BestPeerRmse,
            ["private_mean_disagreement"] = complementarity.MeanDisagreement,
            ["preserved_rmse"] = preserved.Rmse,
            ["synchronized_rmse"] = synchronized.Rmse,
            ["preserved_early_rmse"] = preserved.EarlyRmse,
            ["synchronized_early_rmse"] = synchronized.EarlyRmse,
            ["preserved_late_rmse"] = preserved.LateRmse,
            ["synchronized_late_rmse"] = synchronized.LateRmse,
            ["preserved_initial_disagreement"] = preserved.InitialDisagreement,
            ["synchronized_initial_disagreement"] = synchronized.InitialDisagreement,
            ["preserved_final_disagreement"] = preserved.FinalDisagreement,
            ["preserved_early_best_peer_rmse"] = preserved.EarlyBestPeerRmse,
            ["preserved_communication_work"] = preserved.CommunicationWork,
            ["preserved_packet_count"] = preserved.CommunicationPacketCount,
        };
    }

    private static List<ExperimentAssertion> BuildAssertions(
        ComplementarityOutcome complementarity,
        PathOutcome preserved,
        PathOutcome synchronized,
        int sharedObservationCount)
    {
        return
        [
            new ExperimentAssertion(
                "complementary-private-histories",
                complementarity.BestPeerRmse <= 0.08,
                "Before peers meet shared consequence, at least one private history should contain a strong local hypothesis for each context rather than both peers merely being noisy copies of one another.",
                complementarity.BestPeerRmse,
                0.08),
            new ExperimentAssertion(
                "preserved-disagreement",
                preserved.InitialDisagreement >= 0.45 && synchronized.InitialDisagreement <= 0.02,
                "The preserved path must retain substantial public disagreement that the synchronization control intentionally erases.",
                preserved.InitialDisagreement - synchronized.InitialDisagreement,
                0.43),
            new ExperimentAssertion(
                "early-correction-benefit",
                preserved.EarlyRmse <= synchronized.EarlyRmse * 0.82,
                "Independent error structure should let shared consequence improve the preserved group sooner than the synchronized control.",
                preserved.EarlyRmse,
                synchronized.EarlyRmse * 0.82),
            new ExperimentAssertion(
                "whole-history-benefit",
                preserved.Rmse <= synchronized.Rmse * 0.82,
                "Preserving private histories should reduce total shared-phase error rather than merely reshuffle when error occurs.",
                preserved.Rmse,
                synchronized.Rmse * 0.82),
            new ExperimentAssertion(
                "plurality-remains-correctable",
                preserved.LateRmse <= 0.06 && preserved.FinalDisagreement <= 0.08,
                "Preserved disagreement should remain revisable: later shared consequence must bring both prediction error and peer disagreement back down.",
                Math.Max(preserved.LateRmse, preserved.FinalDisagreement),
                0.08),
            new ExperimentAssertion(
                "bounded-public-exchange",
                preserved.CommunicationPacketCount == sharedObservationCount * 2 && preserved.CommunicationWork <= 4.0,
                "The preserved path may negotiate only from two compact public postures per shared observation, under explicit finite communication cost.",
                preserved.CommunicationWork,
                4.0),
        ];
    }

    private readonly record struct PeerPublicPosture(double Prediction, double Standing);

    private sealed record ComplementarityOutcome(double BestPeerRmse, double MeanDisagreement);

    private sealed record PathOutcome(
        string Series,
        double Rmse,
        double EarlyRmse,
        double LateRmse,
        double InitialDisagreement,
        double FinalDisagreement,
        double MeanDisagreement,
        double EarlyBestPeerRmse,
        int CommunicationPacketCount,
        double CommunicationWork)
    {
        public Dictionary<string, double> ToMetrics() => new(StringComparer.Ordinal)
        {
            ["rmse"] = Rmse,
            ["early_rmse"] = EarlyRmse,
            ["late_rmse"] = LateRmse,
            ["initial_disagreement"] = InitialDisagreement,
            ["final_disagreement"] = FinalDisagreement,
            ["mean_disagreement"] = MeanDisagreement,
            ["early_best_peer_rmse"] = EarlyBestPeerRmse,
            ["communication_packet_count"] = CommunicationPacketCount,
            ["communication_work"] = CommunicationWork,
        };
    }

    private sealed class PeerMind
    {
        private readonly double[] _estimates = new double[PeerDisagreementWorld.ContextCount];
        private readonly double[] _localStanding = new double[PeerDisagreementWorld.ContextCount];
        private readonly double[] _publicStanding = new double[PeerDisagreementWorld.ContextCount];
        private readonly int[] _privateEvidence = new int[PeerDisagreementWorld.ContextCount];
        private readonly int[] _sharedEvidence = new int[PeerDisagreementWorld.ContextCount];

        public PeerMind(string mindId)
        {
            MindId = mindId;
            Array.Fill(_publicStanding, 0.72);
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
            LastTarget = target;
            LastAbsoluteError = Math.Abs(LastPrediction - target);
            var evidence = _privateEvidence[contextCell];
            if (evidence == 0)
            {
                _estimates[contextCell] = target;
                _localStanding[contextCell] = 0.30;
                _privateEvidence[contextCell] = 1;
                _publicStanding[contextCell] = 0.30;
                return;
            }

            var learningRate = Math.Clamp(0.50 / Math.Sqrt(evidence + 2.0), 0.06, 0.30);
            var error = Math.Abs(_estimates[contextCell] - target);
            _estimates[contextCell] += learningRate * (target - _estimates[contextCell]);
            var quality = Math.Max(0.0, 1.0 - (error / 1.4));
            _localStanding[contextCell] += 0.11 * quality * (1.0 - _localStanding[contextCell]);
            _localStanding[contextCell] = Math.Clamp(_localStanding[contextCell], 0.10, 1.0);
            _privateEvidence[contextCell]++;
            _publicStanding[contextCell] = _localStanding[contextCell];
        }

        public PeerPublicPosture CreatePublicPosture(int contextCell) =>
            new(_estimates[contextCell], _publicStanding[contextCell]);

        public void ObserveShared(int contextCell, double target)
        {
            LastTarget = target;
            LastPrediction = _estimates[contextCell];
            var error = Math.Abs(_estimates[contextCell] - target);
            LastAbsoluteError = error;
            if (error <= 0.14)
            {
                _publicStanding[contextCell] += 0.22 * (1.0 - _publicStanding[contextCell]);
            }
            else if (error >= 0.55)
            {
                _publicStanding[contextCell] *= 0.56;
            }
            else
            {
                _publicStanding[contextCell] *= 0.84;
            }

            _publicStanding[contextCell] = Math.Clamp(_publicStanding[contextCell], 0.04, 1.0);
            var sharedEvidence = _sharedEvidence[contextCell];
            var learningRate = Math.Clamp(0.40 / Math.Sqrt(sharedEvidence + 2.0), 0.07, 0.26);
            _estimates[contextCell] += learningRate * (target - _estimates[contextCell]);
            _sharedEvidence[contextCell]++;
        }

        public double LocalStandingFor(int contextCell) => _localStanding[contextCell];

        public int PrivateEvidenceFor(int contextCell) => _privateEvidence[contextCell];

        public PeerMind Clone(string mindId)
        {
            var clone = new PeerMind(mindId);
            CopyTo(clone);
            return clone;
        }

        public MindPublicState PublicMindState() => new(
            MindId,
            _privateEvidence.Count(value => value > 0),
            0,
            _publicStanding.Average(),
            0.0,
            LastPrediction,
            LastTarget,
            LastAbsoluteError);

        public List<TracePublicState> PublicTraceStates()
        {
            var traces = new List<TracePublicState>(PeerDisagreementWorld.ContextCount);
            for (var cell = 0; cell < PeerDisagreementWorld.ContextCount; cell++)
            {
                if (_privateEvidence[cell] == 0 && _sharedEvidence[cell] == 0)
                {
                    continue;
                }

                traces.Add(new TracePublicState(
                    MindId,
                    cell,
                    TraceProvenance.Direct,
                    MindId,
                    $"{MindId}:{cell}",
                    _estimates[cell],
                    _publicStanding[cell],
                    _privateEvidence[cell] + _sharedEvidence[cell],
                    0));
            }

            return traces;
        }

        public static (PeerMind Left, PeerMind Right) CreateSynchronizedControl(PeerMind left, PeerMind right)
        {
            var synchronizedLeft = new PeerMind("peer-a-sync");
            var synchronizedRight = new PeerMind("peer-b-sync");
            for (var cell = 0; cell < PeerDisagreementWorld.ContextCount; cell++)
            {
                var leftWeight = Math.Max(0.01, left._localStanding[cell]);
                var rightWeight = Math.Max(0.01, right._localStanding[cell]);
                var estimate = ((left._estimates[cell] * leftWeight) + (right._estimates[cell] * rightWeight)) /
                               (leftWeight + rightWeight);
                var standing = (left._localStanding[cell] + right._localStanding[cell]) / 2.0;
                var evidence = (int)Math.Round((left._privateEvidence[cell] + right._privateEvidence[cell]) / 2.0);
                synchronizedLeft.SetConsensusCell(cell, estimate, standing, evidence);
                synchronizedRight.SetConsensusCell(cell, estimate, standing, evidence);
            }

            return (synchronizedLeft, synchronizedRight);
        }

        private void SetConsensusCell(int contextCell, double estimate, double standing, int evidence)
        {
            _estimates[contextCell] = estimate;
            _localStanding[contextCell] = standing;
            _publicStanding[contextCell] = standing;
            _privateEvidence[contextCell] = evidence;
            _sharedEvidence[contextCell] = 0;
        }

        private void CopyTo(PeerMind target)
        {
            Array.Copy(_estimates, target._estimates, _estimates.Length);
            Array.Copy(_localStanding, target._localStanding, _localStanding.Length);
            Array.Copy(_publicStanding, target._publicStanding, _publicStanding.Length);
            Array.Copy(_privateEvidence, target._privateEvidence, _privateEvidence.Length);
            Array.Copy(_sharedEvidence, target._sharedEvidence, _sharedEvidence.Length);
            target.LastPrediction = LastPrediction;
            target.LastTarget = LastTarget;
            target.LastAbsoluteError = LastAbsoluteError;
        }
    }
}
