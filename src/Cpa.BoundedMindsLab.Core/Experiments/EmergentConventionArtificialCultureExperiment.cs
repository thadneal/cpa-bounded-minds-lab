using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed class EmergentConventionArtificialCultureExperiment : IExperiment
{
    private const string ExperimentName = "05-emergent-convention-artificial-culture";
    private const int FormationLateWindowTicks = 72;
    private const int ShiftedEarlyWindowTicks = 72;
    private const int ShiftedLateWindowTicks = 72;
    private const double ConventionShortcutStanding = 0.72;
    private const double ConventionSuccessUtility = 0.70;
    private const double NegotiationPacketCost = 0.006;
    private const double ConventionTokenCost = 0.002;

    public string Name => ExperimentName;

    public string Question =>
        "Can repeated bounded interaction produce distributed conventions that reduce coordination cost, remain grounded in successful use, and revise when the conditions that sustained them change?";

    public ExperimentResult Run(ExperimentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Emit(ExperimentFrameKind.ExperimentStarted, Name, message: Question);

        var scenario = EmergentConventionWorld.CreateScenario(context.Seed);
        EmitScenario(context, scenario);
        var formationEpisodes = EmergentConventionWorld.CreateFormationEpisodes(scenario);
        var shiftedEpisodes = EmergentConventionWorld.CreateShiftedEpisodes(scenario);

        var adaptive = RunPath(
            context,
            scenario,
            formationEpisodes,
            shiftedEpisodes,
            "earned-convention",
            ConventionPathMode.AdaptiveConvention,
            "Peers negotiate while a local convention is weak. Repeated successful coordination lets each mind independently retain the same context-specific convention. Poor consequence reduces standing and can reopen negotiation.");

        var fresh = RunPath(
            context,
            scenario,
            formationEpisodes,
            shiftedEpisodes,
            "fresh-negotiation",
            ConventionPathMode.FreshNegotiation,
            "Control: peers exchange bounded preference postures on every episode and retain no convention across encounters.");

        var frozen = RunPath(
            context,
            scenario,
            formationEpisodes,
            shiftedEpisodes,
            "frozen-convention",
            ConventionPathMode.FrozenConvention,
            "Control: peers develop the same distributed convention as the adaptive path before the regime shift, then convention action and standing are frozen despite later consequence.");

        var metrics = BuildResultMetrics(scenario, adaptive, fresh, frozen);
        var assertions = BuildAssertions(scenario, adaptive, fresh, frozen, formationEpisodes.Length + shiftedEpisodes.Length);
        var passed = assertions.Count(assertion => assertion.Passed);
        var verdict = passed == assertions.Count
            ? ExperimentVerdict.Support
            : passed >= 5
                ? ExperimentVerdict.Mixed
                : ExperimentVerdict.Disconfirm;
        var interpretation = verdict switch
        {
            ExperimentVerdict.Support =>
                "Repeated bounded interaction produced a distributed convention that compressed routine coordination without a durable central convention owner. The convention preserved nearly all of fresh negotiation's utility, reopened negotiation when consequence made an old habit expensive, rewrote affected contexts, and retained stable conventions elsewhere.",
            ExperimentVerdict.Mixed =>
                "A collective convention formed or reduced communication cost, but one or more preregistered boundaries on utility, revision, stable retention, world heterogeneity, or the frozen-control comparison did not hold.",
            _ =>
                "The distributed convention did not earn enough coordination value, communication economy, or revisability under changed conditions to support the artificial-culture hypothesis in this world family.",
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

    private static void EmitScenario(ExperimentContext context, EmergentConventionScenario scenario)
    {
        var parts = new string[scenario.Cells.Length];
        for (var index = 0; index < scenario.Cells.Length; index++)
        {
            var cell = scenario.Cells[index];
            parts[index] = $"c{cell.ContextCell}:{cell.ContextKind}/d{cell.InitialPreferenceDiversity}/shift-a{cell.ShiftPreferredAction}";
        }

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "scenario",
            phase: "scenario-generated",
            message: $"Seed {scenario.Seed} generated a plural coordination circumstance: {string.Join("; ", parts)}.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["shifted_contexts"] = EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Shifted),
                ["stable_contexts"] = EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Stable),
                ["preference_diverse_contexts"] = EmergentConventionWorld.CountPreferenceDiverseContexts(scenario),
                ["mean_initial_viable_gap"] = EmergentConventionWorld.MeanInitialViableGap(scenario),
                ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
            });
    }

    private static PathOutcome RunPath(
        ExperimentContext context,
        EmergentConventionScenario scenario,
        ConventionEpisode[] formationEpisodes,
        ConventionEpisode[] shiftedEpisodes,
        string series,
        ConventionPathMode mode,
        string description)
    {
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            series,
            phase: "convention-formation",
            message: description);

        var peers = CreatePeers();
        var formationMeasurements = new EpisodeMeasurement[formationEpisodes.Length];
        var shiftedMeasurements = new EpisodeMeasurement[shiftedEpisodes.Length];
        var communicationPacketCount = 0;
        var communicationWork = 0.0;
        var totalUtility = 0.0;
        var totalRegret = 0.0;
        var globalTick = 0;

        for (var tick = 0; tick < formationEpisodes.Length; tick++)
        {
            formationMeasurements[tick] = RunEpisode(
                context,
                scenario,
                series,
                mode,
                peers,
                formationEpisodes[tick],
                globalTick,
                shifted: false,
                allowConventionRevision: mode != ConventionPathMode.FreshNegotiation,
                ref communicationPacketCount,
                ref communicationWork,
                ref totalUtility,
                ref totalRegret);
            globalTick++;
        }

        var formationActions = CaptureConventionActions(peers);
        var formationFingerprint = ConventionFingerprint(formationActions);
        var formationCoverage = ConventionCoverage(formationActions);
        var formationMeanStanding = MeanConventionStanding(peers);
        var formationLateUtility = MeanLast(formationMeasurements, FormationLateWindowTicks, measurement => measurement.Utility);
        var formationLateShortcutRate = MeanLast(formationMeasurements, FormationLateWindowTicks, measurement => measurement.UsedShortcut ? 1.0 : 0.0);
        var formationLateAgreement = MeanLast(formationMeasurements, FormationLateWindowTicks, measurement => measurement.ContextAgreement);

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            series,
            formationEpisodes.Length,
            "convention-formed",
            message: mode == ConventionPathMode.FreshNegotiation
                ? "Fresh negotiation completed the first regime without retaining any durable convention."
                : $"{series} completed convention formation with {formationCoverage:P0} context coverage and {formationLateShortcutRate:P0} shortcut use in the late formation window.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["formation_convention_coverage"] = formationCoverage,
                ["formation_mean_standing"] = formationMeanStanding,
                ["formation_late_utility"] = formationLateUtility,
                ["formation_late_shortcut_rate"] = formationLateShortcutRate,
                ["formation_late_agreement"] = formationLateAgreement,
                ["formation_convention_fingerprint_low32"] = (double)(formationFingerprint & uint.MaxValue),
                ["communication_work"] = communicationWork,
            },
            minds: PublicMindStates(peers),
            traces: PublicTraceStates(peers));

        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            series,
            formationEpisodes.Length,
            "regime-shift",
            message: $"The cost landscape changes on {EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Shifted)} contexts. {(mode == ConventionPathMode.FrozenConvention ? "This control refuses to revise convention state." : "External consequence can now challenge prior social standing.")}");

        for (var tick = 0; tick < shiftedEpisodes.Length; tick++)
        {
            shiftedMeasurements[tick] = RunEpisode(
                context,
                scenario,
                series,
                mode,
                peers,
                shiftedEpisodes[tick],
                globalTick,
                shifted: true,
                allowConventionRevision: mode == ConventionPathMode.AdaptiveConvention,
                ref communicationPacketCount,
                ref communicationWork,
                ref totalUtility,
                ref totalRegret);
            globalTick++;
        }

        var finalActions = CaptureConventionActions(peers);
        var outcome = new PathOutcome(
            totalUtility / Math.Max(1, globalTick),
            totalRegret / Math.Max(1, globalTick),
            formationCoverage,
            formationMeanStanding,
            formationLateUtility,
            formationLateShortcutRate,
            formationLateAgreement,
            MeanFirst(shiftedMeasurements, ShiftedEarlyWindowTicks, measurement => measurement.Utility),
            MeanLast(shiftedMeasurements, ShiftedLateWindowTicks, measurement => measurement.Utility),
            MeanLast(shiftedMeasurements, ShiftedLateWindowTicks, measurement => measurement.UsedShortcut ? 1.0 : 0.0),
            MeanWhere(shiftedMeasurements, measurement => measurement.ChangedContext, measurement => measurement.Utility),
            MeanLastWhere(shiftedMeasurements, ShiftedLateWindowTicks, measurement => measurement.ChangedContext, measurement => measurement.Utility),
            MeanWhere(shiftedMeasurements, measurement => !measurement.ChangedContext, measurement => measurement.Utility),
            ConventionCoverage(finalActions),
            ChangedRevisionCoverage(scenario, finalActions),
            StableRetentionCoverage(scenario, formationActions, finalActions),
            ChangedConventionSwitchCount(scenario, formationActions, finalActions),
            communicationPacketCount,
            communicationWork,
            formationFingerprint,
            ConventionFingerprint(finalActions));

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            series,
            globalTick,
            "path-complete",
            message: $"{series} completed with mean utility {outcome.MeanUtility:0.000000}, communication work {outcome.CommunicationWork:0.000}, changed-context late utility {outcome.ChangedShiftedLateUtility:0.000000}, and final convention agreement {outcome.FinalConventionCoverage:0.000}.",
            metrics: outcome.ToMetrics(),
            minds: PublicMindStates(peers),
            traces: PublicTraceStates(peers));
        return outcome;
    }

    private static EpisodeMeasurement RunEpisode(
        ExperimentContext context,
        EmergentConventionScenario scenario,
        string series,
        ConventionPathMode mode,
        CulturePeer[] peers,
        ConventionEpisode episode,
        int globalTick,
        bool shifted,
        bool allowConventionRevision,
        ref int communicationPacketCount,
        ref double communicationWork,
        ref double totalUtility,
        ref double totalRegret)
    {
        var contextCell = episode.ContextCell;
        var cell = scenario.Cells[contextCell];
        var speakerIndex = globalTick % peers.Length;
        var postures = CreatePreferencePostures(episode.PeerCosts);
        var useShortcut = mode != ConventionPathMode.FreshNegotiation &&
            peers[speakerIndex].ConventionActionFor(contextCell) >= 0 &&
            peers[speakerIndex].StandingFor(contextCell) >= ConventionShortcutStanding;
        var actions = new int[peers.Length];
        double episodeCommunicationWork;

        if (useShortcut)
        {
            for (var peerIndex = 0; peerIndex < peers.Length; peerIndex++)
            {
                var conventionAction = peers[peerIndex].ConventionActionFor(contextCell);
                actions[peerIndex] = conventionAction >= 0 ? conventionAction : postures[peerIndex].PreferredAction;
            }

            communicationPacketCount++;
            episodeCommunicationWork = ConventionTokenCost;
        }
        else
        {
            var selectedAction = SelectNegotiatedAction(postures, speakerIndex);
            Array.Fill(actions, selectedAction);
            communicationPacketCount += peers.Length;
            episodeCommunicationWork = peers.Length * NegotiationPacketCost;
        }

        communicationWork += episodeCommunicationWork;
        var coordinated = AllActionsEqual(actions);
        var utility = GroupUtility(episode.PeerCosts, actions, coordinated);
        var regret = Math.Max(0.0, BestCoordinatedUtility(episode.PeerCosts) - utility);
        totalUtility += utility;
        totalRegret += regret;

        if (mode != ConventionPathMode.FreshNegotiation)
        {
            for (var peerIndex = 0; peerIndex < peers.Length; peerIndex++)
            {
                peers[peerIndex].ObserveOutcome(
                    contextCell,
                    actions[peerIndex],
                    utility,
                    postures[peerIndex].PreferredAction,
                    allowConventionRevision);
            }
        }

        var contextAgreement = ConventionAgreement(peers, contextCell);
        var meanStanding = MeanConventionStanding(peers, contextCell);
        context.Emit(
            ExperimentFrameKind.MetricSample,
            ExperimentName,
            series,
            globalTick,
            shifted ? "regime-shift" : "convention-formation",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["context_cell"] = contextCell,
                ["changed_context"] = cell.ContextKind == ConventionContextKind.Shifted ? 1.0 : 0.0,
                ["regime"] = shifted ? 1.0 : 0.0,
                ["group_utility"] = utility,
                ["rolling_mean_utility"] = totalUtility / (globalTick + 1.0),
                ["regret"] = regret,
                ["rolling_mean_regret"] = totalRegret / (globalTick + 1.0),
                ["coordination_success"] = coordinated ? 1.0 : 0.0,
                ["used_convention_shortcut"] = useShortcut ? 1.0 : 0.0,
                ["context_convention_agreement"] = contextAgreement,
                ["mean_convention_standing"] = meanStanding,
                ["speaker_convention_standing"] = peers[speakerIndex].StandingFor(contextCell),
                ["episode_communication_work"] = episodeCommunicationWork,
                ["communication_work"] = communicationWork,
                ["selected_action"] = coordinated ? actions[0] : -1.0,
            });

        if (globalTick % 48 == 47)
        {
            context.Emit(
                ExperimentFrameKind.StateSnapshot,
                ExperimentName,
                series,
                globalTick,
                shifted ? "regime-shift" : "convention-formation",
                minds: PublicMindStates(peers),
                traces: PublicTraceStates(peers));
        }

        return new EpisodeMeasurement(
            utility,
            regret,
            useShortcut,
            contextAgreement,
            cell.ContextKind == ConventionContextKind.Shifted);
    }

    private static Dictionary<string, double> BuildResultMetrics(
        EmergentConventionScenario scenario,
        PathOutcome adaptive,
        PathOutcome fresh,
        PathOutcome frozen) => new(StringComparer.Ordinal)
    {
        ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
        ["shifted_contexts"] = EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Shifted),
        ["stable_contexts"] = EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Stable),
        ["preference_diverse_contexts"] = EmergentConventionWorld.CountPreferenceDiverseContexts(scenario),
        ["mean_initial_viable_gap"] = EmergentConventionWorld.MeanInitialViableGap(scenario),
        ["earned_mean_utility"] = adaptive.MeanUtility,
        ["fresh_mean_utility"] = fresh.MeanUtility,
        ["frozen_mean_utility"] = frozen.MeanUtility,
        ["earned_mean_regret"] = adaptive.MeanRegret,
        ["fresh_mean_regret"] = fresh.MeanRegret,
        ["frozen_mean_regret"] = frozen.MeanRegret,
        ["earned_formation_convention_coverage"] = adaptive.FormationConventionCoverage,
        ["earned_formation_late_shortcut_rate"] = adaptive.FormationLateShortcutRate,
        ["earned_formation_late_utility"] = adaptive.FormationLateUtility,
        ["fresh_formation_late_utility"] = fresh.FormationLateUtility,
        ["earned_shifted_early_utility"] = adaptive.ShiftedEarlyUtility,
        ["earned_shifted_late_utility"] = adaptive.ShiftedLateUtility,
        ["fresh_shifted_late_utility"] = fresh.ShiftedLateUtility,
        ["frozen_shifted_late_utility"] = frozen.ShiftedLateUtility,
        ["earned_changed_shifted_utility"] = adaptive.ChangedShiftedUtility,
        ["fresh_changed_shifted_utility"] = fresh.ChangedShiftedUtility,
        ["frozen_changed_shifted_utility"] = frozen.ChangedShiftedUtility,
        ["earned_changed_shifted_late_utility"] = adaptive.ChangedShiftedLateUtility,
        ["fresh_changed_shifted_late_utility"] = fresh.ChangedShiftedLateUtility,
        ["frozen_changed_shifted_late_utility"] = frozen.ChangedShiftedLateUtility,
        ["earned_changed_revision_coverage"] = adaptive.ChangedRevisionCoverage,
        ["frozen_changed_revision_coverage"] = frozen.ChangedRevisionCoverage,
        ["earned_stable_retention_coverage"] = adaptive.StableRetentionCoverage,
        ["earned_final_convention_coverage"] = adaptive.FinalConventionCoverage,
        ["earned_changed_convention_switches"] = adaptive.ChangedConventionSwitchCount,
        ["earned_communication_work"] = adaptive.CommunicationWork,
        ["fresh_communication_work"] = fresh.CommunicationWork,
        ["frozen_communication_work"] = frozen.CommunicationWork,
        ["earned_packet_count"] = adaptive.CommunicationPacketCount,
        ["fresh_packet_count"] = fresh.CommunicationPacketCount,
        ["frozen_packet_count"] = frozen.CommunicationPacketCount,
        ["earned_formation_convention_fingerprint_low32"] = (double)(adaptive.FormationConventionFingerprint & uint.MaxValue),
        ["frozen_formation_convention_fingerprint_low32"] = (double)(frozen.FormationConventionFingerprint & uint.MaxValue),
    };

    private static List<ExperimentAssertion> BuildAssertions(
        EmergentConventionScenario scenario,
        PathOutcome adaptive,
        PathOutcome fresh,
        PathOutcome frozen,
        int totalEpisodeCount)
    {
        var shiftedContexts = EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Shifted);
        var stableContexts = EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Stable);
        var preferenceDiverseContexts = EmergentConventionWorld.CountPreferenceDiverseContexts(scenario);
        var meanViableGap = EmergentConventionWorld.MeanInitialViableGap(scenario);
        var expectedFreshPackets = totalEpisodeCount * EmergentConventionWorld.PeerCount;
        var expectedFreshWork = expectedFreshPackets * NegotiationPacketCost;
        return
        [
            new ExperimentAssertion(
                "seed-generates-plural-coordination-circumstance",
                shiftedContexts is >= 4 and <= 6 && stableContexts >= 6 && preferenceDiverseContexts >= 8 && meanViableGap <= 0.05,
                "The seed must create stable and changed contexts, widespread private preference plurality, and no large average gap that installs one obvious initial convention.",
                preferenceDiverseContexts,
                8.0),
            new ExperimentAssertion(
                "distributed-convention-emerges",
                adaptive.FormationConventionCoverage >= 0.90 &&
                adaptive.FormationLateShortcutRate >= 0.80 &&
                adaptive.FormationLateUtility >= fresh.FormationLateUtility * 0.98 &&
                adaptive.FormationConventionFingerprint == frozen.FormationConventionFingerprint,
                "Repeated successful interaction must make local convention memories agree across most contexts, replace most late formation negotiations with shorthand, preserve utility, and form identically in the adaptive and later-frozen controls.",
                adaptive.FormationConventionCoverage,
                0.90),
            new ExperimentAssertion(
                "earned-convention-compresses-communication",
                fresh.CommunicationPacketCount == expectedFreshPackets &&
                Math.Abs(fresh.CommunicationWork - expectedFreshWork) <= 1e-9 &&
                adaptive.CommunicationWork <= 2.75 &&
                adaptive.CommunicationWork <= fresh.CommunicationWork * 0.35 &&
                adaptive.CommunicationPacketCount < fresh.CommunicationPacketCount,
                "Fresh negotiation must pay for three preference packets per episode. The earned convention must reduce whole-history public work to at most 35% of that baseline and remain below 2.75 work units.",
                adaptive.CommunicationWork,
                2.75),
            new ExperimentAssertion(
                "collective-habit-remains-useful",
                adaptive.MeanUtility >= fresh.MeanUtility * 0.98,
                "Communication economy cannot be purchased by materially degrading lived coordination. Earned-convention lifetime utility must remain within 2% of fresh negotiation.",
                adaptive.MeanUtility,
                fresh.MeanUtility * 0.98),
            new ExperimentAssertion(
                "changed-conditions-revise-convention",
                adaptive.ChangedRevisionCoverage >= 0.85 &&
                adaptive.ChangedShiftedLateUtility >= fresh.ChangedShiftedLateUtility * 0.95 &&
                adaptive.FinalConventionCoverage >= 0.90,
                "When old habits become expensive, consequence must reopen coordination and rewrite affected contexts while recovering near fresh-negotiation utility.",
                adaptive.ChangedRevisionCoverage,
                0.85),
            new ExperimentAssertion(
                "frozen-culture-fails-under-pressure",
                frozen.FormationConventionFingerprint == adaptive.FormationConventionFingerprint &&
                frozen.ChangedRevisionCoverage <= 0.10 &&
                adaptive.ChangedShiftedLateUtility >= frozen.ChangedShiftedLateUtility + 0.20,
                "The frozen control must begin from the same culture, refuse later correction, and perform at least 0.20 worse on changed-context late utility than the adaptive culture.",
                adaptive.ChangedShiftedLateUtility - frozen.ChangedShiftedLateUtility,
                0.20),
            new ExperimentAssertion(
                "localized-change-preserves-stable-conventions",
                adaptive.StableRetentionCoverage >= 0.90 && adaptive.ShiftedLateShortcutRate >= 0.80,
                "Revision must remain local. At least 90% of stable contexts should retain their formed convention and the adaptive path should return to mostly shorthand coordination late in the changed regime.",
                adaptive.StableRetentionCoverage,
                0.90),
        ];
    }

    private static CulturePeer[] CreatePeers()
    {
        var peers = new CulturePeer[EmergentConventionWorld.PeerCount];
        for (var peerIndex = 0; peerIndex < peers.Length; peerIndex++)
        {
            peers[peerIndex] = new CulturePeer($"peer-{peerIndex + 1}", EmergentConventionWorld.ContextCount);
        }

        return peers;
    }

    private static PreferencePosture[] CreatePreferencePostures(double[][] peerCosts)
    {
        var postures = new PreferencePosture[peerCosts.Length];
        for (var peerIndex = 0; peerIndex < peerCosts.Length; peerIndex++)
        {
            var bestAction = 0;
            var bestCost = peerCosts[peerIndex][0];
            var secondBestCost = double.PositiveInfinity;
            for (var action = 1; action < peerCosts[peerIndex].Length; action++)
            {
                var cost = peerCosts[peerIndex][action];
                if (cost < bestCost)
                {
                    secondBestCost = bestCost;
                    bestCost = cost;
                    bestAction = action;
                }
                else if (cost < secondBestCost)
                {
                    secondBestCost = cost;
                }
            }

            if (!double.IsFinite(secondBestCost))
            {
                secondBestCost = bestCost;
            }

            postures[peerIndex] = new PreferencePosture(bestAction, Math.Max(0.0, secondBestCost - bestCost));
        }

        return postures;
    }

    private static int SelectNegotiatedAction(PreferencePosture[] postures, int speakerIndex)
    {
        var scores = new double[EmergentConventionWorld.ActionCount];
        for (var peerIndex = 0; peerIndex < postures.Length; peerIndex++)
        {
            var posture = postures[peerIndex];
            scores[posture.PreferredAction] += 1.0 + (4.0 * posture.PreferenceStrength);
        }

        var bestScore = scores[0];
        for (var action = 1; action < scores.Length; action++)
        {
            bestScore = Math.Max(bestScore, scores[action]);
        }

        var speakerPreference = postures[speakerIndex].PreferredAction;
        if (Math.Abs(scores[speakerPreference] - bestScore) <= 1e-12)
        {
            return speakerPreference;
        }

        for (var action = 0; action < scores.Length; action++)
        {
            if (Math.Abs(scores[action] - bestScore) <= 1e-12)
            {
                return action;
            }
        }

        return 0;
    }

    private static bool AllActionsEqual(int[] actions)
    {
        if (actions.Length == 0)
        {
            return false;
        }

        for (var index = 1; index < actions.Length; index++)
        {
            if (actions[index] != actions[0])
            {
                return false;
            }
        }

        return true;
    }

    private static double GroupUtility(double[][] peerCosts, int[] actions, bool coordinated)
    {
        if (!coordinated || actions.Length == 0)
        {
            return 0.0;
        }

        var totalCost = 0.0;
        for (var peerIndex = 0; peerIndex < peerCosts.Length; peerIndex++)
        {
            totalCost += peerCosts[peerIndex][actions[0]];
        }

        var meanCost = peerCosts.Length == 0 ? 1.0 : totalCost / peerCosts.Length;
        return Math.Clamp(1.0 - meanCost, 0.0, 1.0);
    }

    private static double BestCoordinatedUtility(double[][] peerCosts)
    {
        var bestUtility = 0.0;
        for (var action = 0; action < EmergentConventionWorld.ActionCount; action++)
        {
            var totalCost = 0.0;
            for (var peerIndex = 0; peerIndex < peerCosts.Length; peerIndex++)
            {
                totalCost += peerCosts[peerIndex][action];
            }

            var meanCost = peerCosts.Length == 0 ? 1.0 : totalCost / peerCosts.Length;
            bestUtility = Math.Max(bestUtility, Math.Clamp(1.0 - meanCost, 0.0, 1.0));
        }

        return bestUtility;
    }

    private static int[][] CaptureConventionActions(CulturePeer[] peers)
    {
        var actions = new int[peers.Length][];
        for (var peerIndex = 0; peerIndex < peers.Length; peerIndex++)
        {
            actions[peerIndex] = peers[peerIndex].ConventionActions();
        }

        return actions;
    }

    private static double ConventionCoverage(int[][] actions)
    {
        if (actions.Length == 0 || actions[0].Length == 0)
        {
            return 0.0;
        }

        var covered = 0;
        for (var contextCell = 0; contextCell < actions[0].Length; contextCell++)
        {
            var convention = actions[0][contextCell];
            if (convention < 0)
            {
                continue;
            }

            var agreed = true;
            for (var peerIndex = 1; peerIndex < actions.Length; peerIndex++)
            {
                if (actions[peerIndex][contextCell] != convention)
                {
                    agreed = false;
                    break;
                }
            }

            if (agreed)
            {
                covered++;
            }
        }

        return (double)covered / actions[0].Length;
    }

    private static double ChangedRevisionCoverage(EmergentConventionScenario scenario, int[][] finalActions)
    {
        var changedCount = 0;
        var revisedCount = 0;
        for (var contextCell = 0; contextCell < scenario.Cells.Length; contextCell++)
        {
            var cell = scenario.Cells[contextCell];
            if (cell.ContextKind != ConventionContextKind.Shifted)
            {
                continue;
            }

            changedCount++;
            var revised = true;
            for (var peerIndex = 0; peerIndex < finalActions.Length; peerIndex++)
            {
                if (finalActions[peerIndex][contextCell] != cell.ShiftPreferredAction)
                {
                    revised = false;
                    break;
                }
            }

            if (revised)
            {
                revisedCount++;
            }
        }

        return changedCount == 0 ? 0.0 : (double)revisedCount / changedCount;
    }

    private static double StableRetentionCoverage(EmergentConventionScenario scenario, int[][] formationActions, int[][] finalActions)
    {
        var stableCount = 0;
        var retainedCount = 0;
        for (var contextCell = 0; contextCell < scenario.Cells.Length; contextCell++)
        {
            if (scenario.Cells[contextCell].ContextKind != ConventionContextKind.Stable)
            {
                continue;
            }

            stableCount++;
            var retained = formationActions[0][contextCell] >= 0;
            for (var peerIndex = 0; peerIndex < finalActions.Length && retained; peerIndex++)
            {
                retained = finalActions[peerIndex][contextCell] == formationActions[peerIndex][contextCell];
            }

            if (retained)
            {
                retainedCount++;
            }
        }

        return stableCount == 0 ? 0.0 : (double)retainedCount / stableCount;
    }

    private static int ChangedConventionSwitchCount(EmergentConventionScenario scenario, int[][] formationActions, int[][] finalActions)
    {
        var count = 0;
        for (var contextCell = 0; contextCell < scenario.Cells.Length; contextCell++)
        {
            if (scenario.Cells[contextCell].ContextKind != ConventionContextKind.Shifted)
            {
                continue;
            }

            for (var peerIndex = 0; peerIndex < finalActions.Length; peerIndex++)
            {
                if (formationActions[peerIndex][contextCell] >= 0 && finalActions[peerIndex][contextCell] != formationActions[peerIndex][contextCell])
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static double ConventionAgreement(CulturePeer[] peers, int contextCell)
    {
        if (peers.Length < 2)
        {
            return 0.0;
        }

        var agreedPairs = 0;
        var pairCount = 0;
        for (var left = 0; left < peers.Length; left++)
        {
            var leftAction = peers[left].ConventionActionFor(contextCell);
            for (var right = left + 1; right < peers.Length; right++)
            {
                pairCount++;
                if (leftAction >= 0 && leftAction == peers[right].ConventionActionFor(contextCell))
                {
                    agreedPairs++;
                }
            }
        }

        return pairCount == 0 ? 0.0 : (double)agreedPairs / pairCount;
    }

    private static double MeanConventionStanding(CulturePeer[] peers)
    {
        var total = 0.0;
        var count = 0;
        for (var peerIndex = 0; peerIndex < peers.Length; peerIndex++)
        {
            for (var contextCell = 0; contextCell < EmergentConventionWorld.ContextCount; contextCell++)
            {
                if (peers[peerIndex].ConventionActionFor(contextCell) < 0)
                {
                    continue;
                }

                total += peers[peerIndex].StandingFor(contextCell);
                count++;
            }
        }

        return count == 0 ? 0.0 : total / count;
    }

    private static double MeanConventionStanding(CulturePeer[] peers, int contextCell)
    {
        var total = 0.0;
        for (var peerIndex = 0; peerIndex < peers.Length; peerIndex++)
        {
            total += peers[peerIndex].StandingFor(contextCell);
        }

        return peers.Length == 0 ? 0.0 : total / peers.Length;
    }

    private static ulong ConventionFingerprint(int[][] actions)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;
        for (var peerIndex = 0; peerIndex < actions.Length; peerIndex++)
        {
            for (var contextCell = 0; contextCell < actions[peerIndex].Length; contextCell++)
            {
                hash ^= unchecked((ulong)(actions[peerIndex][contextCell] + 2));
                hash *= prime;
            }
        }

        return hash;
    }

    private static double MeanFirst(EpisodeMeasurement[] measurements, int count, Func<EpisodeMeasurement, double> selector)
    {
        var actualCount = Math.Min(count, measurements.Length);
        if (actualCount == 0)
        {
            return 0.0;
        }

        var total = 0.0;
        for (var index = 0; index < actualCount; index++)
        {
            total += selector(measurements[index]);
        }

        return total / actualCount;
    }

    private static double MeanLast(EpisodeMeasurement[] measurements, int count, Func<EpisodeMeasurement, double> selector)
    {
        var actualCount = Math.Min(count, measurements.Length);
        if (actualCount == 0)
        {
            return 0.0;
        }

        var start = measurements.Length - actualCount;
        var total = 0.0;
        for (var index = start; index < measurements.Length; index++)
        {
            total += selector(measurements[index]);
        }

        return total / actualCount;
    }

    private static double MeanWhere(EpisodeMeasurement[] measurements, Func<EpisodeMeasurement, bool> predicate, Func<EpisodeMeasurement, double> selector)
    {
        var count = 0;
        var total = 0.0;
        for (var index = 0; index < measurements.Length; index++)
        {
            if (!predicate(measurements[index]))
            {
                continue;
            }

            total += selector(measurements[index]);
            count++;
        }

        return count == 0 ? 0.0 : total / count;
    }

    private static double MeanLastWhere(EpisodeMeasurement[] measurements, int count, Func<EpisodeMeasurement, bool> predicate, Func<EpisodeMeasurement, double> selector)
    {
        var start = Math.Max(0, measurements.Length - count);
        var matched = 0;
        var total = 0.0;
        for (var index = start; index < measurements.Length; index++)
        {
            if (!predicate(measurements[index]))
            {
                continue;
            }

            total += selector(measurements[index]);
            matched++;
        }

        return matched == 0 ? 0.0 : total / matched;
    }

    private static MindPublicState[] PublicMindStates(CulturePeer[] peers)
    {
        var states = new MindPublicState[peers.Length];
        for (var index = 0; index < peers.Length; index++)
        {
            states[index] = peers[index].PublicMindState();
        }

        return states;
    }

    private static TracePublicState[] PublicTraceStates(CulturePeer[] peers)
    {
        var traces = new List<TracePublicState>();
        for (var index = 0; index < peers.Length; index++)
        {
            traces.AddRange(peers[index].PublicTraceStates());
        }

        return traces.ToArray();
    }

    private enum ConventionPathMode
    {
        AdaptiveConvention,
        FreshNegotiation,
        FrozenConvention,
    }

    private sealed record PreferencePosture(int PreferredAction, double PreferenceStrength);

    private sealed record EpisodeMeasurement(
        double Utility,
        double Regret,
        bool UsedShortcut,
        double ContextAgreement,
        bool ChangedContext);

    private sealed record PathOutcome(
        double MeanUtility,
        double MeanRegret,
        double FormationConventionCoverage,
        double FormationMeanStanding,
        double FormationLateUtility,
        double FormationLateShortcutRate,
        double FormationLateAgreement,
        double ShiftedEarlyUtility,
        double ShiftedLateUtility,
        double ShiftedLateShortcutRate,
        double ChangedShiftedUtility,
        double ChangedShiftedLateUtility,
        double StableShiftedUtility,
        double FinalConventionCoverage,
        double ChangedRevisionCoverage,
        double StableRetentionCoverage,
        int ChangedConventionSwitchCount,
        int CommunicationPacketCount,
        double CommunicationWork,
        ulong FormationConventionFingerprint,
        ulong FinalConventionFingerprint)
    {
        public Dictionary<string, double> ToMetrics() => new(StringComparer.Ordinal)
        {
            ["mean_utility"] = MeanUtility,
            ["mean_regret"] = MeanRegret,
            ["formation_convention_coverage"] = FormationConventionCoverage,
            ["formation_mean_standing"] = FormationMeanStanding,
            ["formation_late_utility"] = FormationLateUtility,
            ["formation_late_shortcut_rate"] = FormationLateShortcutRate,
            ["formation_late_agreement"] = FormationLateAgreement,
            ["shifted_early_utility"] = ShiftedEarlyUtility,
            ["shifted_late_utility"] = ShiftedLateUtility,
            ["shifted_late_shortcut_rate"] = ShiftedLateShortcutRate,
            ["changed_shifted_utility"] = ChangedShiftedUtility,
            ["changed_shifted_late_utility"] = ChangedShiftedLateUtility,
            ["stable_shifted_utility"] = StableShiftedUtility,
            ["final_convention_coverage"] = FinalConventionCoverage,
            ["changed_revision_coverage"] = ChangedRevisionCoverage,
            ["stable_retention_coverage"] = StableRetentionCoverage,
            ["changed_convention_switch_count"] = ChangedConventionSwitchCount,
            ["communication_packet_count"] = CommunicationPacketCount,
            ["communication_work"] = CommunicationWork,
            ["formation_convention_fingerprint_low32"] = (double)(FormationConventionFingerprint & uint.MaxValue),
            ["final_convention_fingerprint_low32"] = (double)(FinalConventionFingerprint & uint.MaxValue),
        };
    }

    private sealed class CulturePeer
    {
        private readonly int[] _conventionActions;
        private readonly double[] _standing;
        private readonly int[] _outcomeCount;

        public CulturePeer(string mindId, int contextCount)
        {
            MindId = mindId;
            _conventionActions = new int[contextCount];
            Array.Fill(_conventionActions, -1);
            _standing = new double[contextCount];
            _outcomeCount = new int[contextCount];
        }

        public string MindId { get; }

        public int LastAction { get; private set; } = -1;

        public double LastUtility { get; private set; }

        public int LastPreferredAction { get; private set; } = -1;

        public int ConventionActionFor(int contextCell) => _conventionActions[contextCell];

        public double StandingFor(int contextCell) => _standing[contextCell];

        public int[] ConventionActions() => (int[])_conventionActions.Clone();

        public void ObserveOutcome(int contextCell, int performedAction, double utility, int preferredAction, bool allowRevision)
        {
            LastAction = performedAction;
            LastUtility = utility;
            LastPreferredAction = preferredAction;
            _outcomeCount[contextCell]++;
            if (!allowRevision)
            {
                return;
            }

            if (utility >= ConventionSuccessUtility && performedAction >= 0)
            {
                if (_conventionActions[contextCell] != performedAction)
                {
                    _conventionActions[contextCell] = performedAction;
                    _standing[contextCell] = 0.30;
                }
                else
                {
                    _standing[contextCell] = Math.Min(0.95, _standing[contextCell] + (0.34 * (1.0 - _standing[contextCell])));
                }

                return;
            }

            _standing[contextCell] *= 0.38;
            if (_standing[contextCell] < 0.25 && preferredAction != _conventionActions[contextCell])
            {
                _conventionActions[contextCell] = preferredAction;
                _standing[contextCell] = 0.15;
            }
        }

        public MindPublicState PublicMindState()
        {
            var conventionCount = 0;
            var totalStanding = 0.0;
            for (var contextCell = 0; contextCell < _conventionActions.Length; contextCell++)
            {
                if (_conventionActions[contextCell] < 0)
                {
                    continue;
                }

                conventionCount++;
                totalStanding += _standing[contextCell];
            }

            var meanStanding = conventionCount == 0 ? 0.0 : totalStanding / conventionCount;
            return new MindPublicState(MindId, conventionCount, 0, meanStanding, 0.0, LastAction, LastPreferredAction, 1.0 - LastUtility);
        }

        public TracePublicState[] PublicTraceStates()
        {
            var traces = new List<TracePublicState>(_conventionActions.Length);
            for (var contextCell = 0; contextCell < _conventionActions.Length; contextCell++)
            {
                if (_conventionActions[contextCell] < 0)
                {
                    continue;
                }

                traces.Add(new TracePublicState(
                    MindId,
                    contextCell,
                    TraceProvenance.Direct,
                    MindId,
                    $"{MindId}:convention:{contextCell}",
                    _conventionActions[contextCell],
                    _standing[contextCell],
                    _outcomeCount[contextCell],
                    0));
            }

            return traces.ToArray();
        }
    }
}
