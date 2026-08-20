using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;

namespace Cpa.BoundedMindsLab.Experiments;

public sealed class IncompleteEpistemicAncestryExperiment : IExperiment
{
    private const string ExperimentName = "06-incomplete-epistemic-ancestry";
    private const double SignatureMergeDistance = 0.22;
    private const double PacketCost = 0.004;

    public string Name => ExperimentName;

    public string Question =>
        "Can a bounded receiver distinguish independent convergence from echoed ancestry when provenance is missing or partial, without requiring a perfect global lineage registry?";

    public ExperimentResult Run(ExperimentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Emit(ExperimentFrameKind.ExperimentStarted, Name, message: Question);

        var scenario = EpistemicAncestryWorld.CreateScenario(context.Seed);
        EmitScenario(context, scenario);

        var inferred = RunPath(
            context,
            scenario,
            "ancestry-inferred",
            AncestryPathMode.Inferred,
            "Receiver clusters likely shared ancestry from the incomplete public origin hints and compact developmental signatures. Agreement within one inferred lineage earns only one unit of corroborative support.");
        var naive = RunPath(
            context,
            scenario,
            "naive-agreement",
            AncestryPathMode.Naive,
            "Control: every peer report is counted as independent corroboration even when several reports may descend from the same upstream episode.");
        var oracle = RunPath(
            context,
            scenario,
            "oracle-ancestry",
            AncestryPathMode.Oracle,
            "Calibration control: true hidden root ancestry is supplied to the reducer. This is an assay ceiling, not proposed CPA machinery.");

        var metrics = BuildResultMetrics(scenario, inferred, naive, oracle);
        var assertions = BuildAssertions(scenario, inferred, naive, oracle);
        var passed = assertions.Count(assertion => assertion.Passed);
        var verdict = passed == assertions.Count
            ? ExperimentVerdict.Support
            : passed >= 6
                ? ExperimentVerdict.Mixed
                : ExperimentVerdict.Disconfirm;
        var interpretation = verdict switch
        {
            ExperimentVerdict.Support =>
                "Incomplete public ancestry cues were sufficient to discount echoed agreement without materially suppressing independent convergence. The inferred reducer reduced echo-trap and whole-history error relative to naive agreement while remaining close to the perfect-ancestry calibration control.",
            ExperimentVerdict.Mixed =>
                "The receiver recovered some useful ancestry structure, but one or more preregistered boundaries on echo discounting, independent convergence, clustering accuracy, world incompleteness, or oracle proximity did not hold.",
            _ =>
                "Partial ancestry cues did not reliably prevent copied agreement from masquerading as independent corroboration in this world family.",
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

    private static void EmitScenario(ExperimentContext context, EpistemicAncestryScenario scenario)
    {
        var parts = new string[scenario.Cells.Length];
        for (var index = 0; index < scenario.Cells.Length; index++)
        {
            var cell = scenario.Cells[index];
            parts[index] = $"c{cell.ContextCell}:{cell.ContextKind}/roots{cell.Roots.Length}";
        }

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            "scenario",
            phase: "ancestry-world-generated",
            message: $"Seed {scenario.Seed} generated incomplete ancestry: {string.Join("; ", parts)}.",
            metrics: new Dictionary<string, double>(StringComparer.Ordinal)
            {
                ["echo_trap_contexts"] = EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.EchoTrap),
                ["independent_convergence_contexts"] = EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.IndependentConvergence),
                ["mixed_lineage_contexts"] = EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.MixedLineage),
                ["ambiguous_lineage_contexts"] = EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.AmbiguousLineage),
                ["missing_origin_rate"] = EpistemicAncestryWorld.MissingOriginRate(scenario),
                ["immediate_sender_hint_rate"] = EpistemicAncestryWorld.ImmediateSenderHintRate(scenario),
                ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
            });
    }

    private static PathOutcome RunPath(
        ExperimentContext context,
        EpistemicAncestryScenario scenario,
        string series,
        AncestryPathMode mode,
        string description)
    {
        context.Emit(
            ExperimentFrameKind.PhaseChanged,
            ExperimentName,
            series,
            phase: mode == AncestryPathMode.Inferred ? "infer-ancestry" : "evaluate-reports",
            message: description);

        var totalSquaredError = 0.0;
        var echoSquaredError = 0.0;
        var independentSquaredError = 0.0;
        var mixedSquaredError = 0.0;
        var ambiguousSquaredError = 0.0;
        var echoCount = 0;
        var independentCount = 0;
        var mixedCount = 0;
        var ambiguousCount = 0;
        var trueEchoPairs = 0;
        var recoveredEchoPairs = 0;
        var independentPairs = 0;
        var falseMergedPairs = 0;
        var totalEffectiveGroups = 0;
        var totalTrueRoots = 0;
        var communicationPackets = 0;
        var communicationWork = 0.0;

        for (var contextIndex = 0; contextIndex < scenario.Cells.Length; contextIndex++)
        {
            var cell = scenario.Cells[contextIndex];
            var groups = mode switch
            {
                AncestryPathMode.Naive => CreateNaiveGroups(cell.Reports),
                AncestryPathMode.Oracle => CreateOracleGroups(cell.Reports),
                _ => CreateInferredGroups(cell.Reports),
            };
            var prediction = PredictFromGroups(groups);
            var error = prediction - cell.Target;
            var squaredError = error * error;
            totalSquaredError += squaredError;
            AddKindError(
                cell.ContextKind,
                squaredError,
                ref echoSquaredError,
                ref echoCount,
                ref independentSquaredError,
                ref independentCount,
                ref mixedSquaredError,
                ref mixedCount,
                ref ambiguousSquaredError,
                ref ambiguousCount);

            var pairStats = PairStats.For(cell.Reports, groups);
            trueEchoPairs += pairStats.TrueEchoPairs;
            recoveredEchoPairs += pairStats.RecoveredEchoPairs;
            independentPairs += pairStats.IndependentPairs;
            falseMergedPairs += pairStats.FalseMergedPairs;
            totalEffectiveGroups += groups.Count;
            totalTrueRoots += cell.Roots.Length;
            communicationPackets += cell.Reports.Length;
            communicationWork += cell.Reports.Length * PacketCost;

            context.Emit(
                ExperimentFrameKind.MetricSample,
                ExperimentName,
                series,
                contextIndex,
                phase: "ancestry-evaluation",
                metrics: new Dictionary<string, double>(StringComparer.Ordinal)
                {
                    ["context_cell"] = cell.ContextCell,
                    ["context_kind"] = (double)cell.ContextKind,
                    ["prediction"] = prediction,
                    ["target"] = cell.Target,
                    ["absolute_error"] = Math.Abs(error),
                    ["rolling_rmse"] = Math.Sqrt(totalSquaredError / (contextIndex + 1.0)),
                    ["report_count"] = cell.Reports.Length,
                    ["true_root_count"] = cell.Roots.Length,
                    ["effective_support_groups"] = groups.Count,
                    ["context_echo_pair_recall"] = pairStats.EchoRecall,
                    ["context_false_merge_rate"] = pairStats.FalseMergeRate,
                    ["communication_work"] = communicationWork,
                });

            if (contextIndex % 4 == 3 || contextIndex == scenario.Cells.Length - 1)
            {
                context.Emit(
                    ExperimentFrameKind.StateSnapshot,
                    ExperimentName,
                    series,
                    contextIndex,
                    "ancestry-evaluation",
                    minds: PublicMindState(series, cell, prediction),
                    traces: PublicTraceStates(series, cell));
            }
        }

        var outcome = new PathOutcome(
            Rmse(totalSquaredError, scenario.Cells.Length),
            Rmse(echoSquaredError, echoCount),
            Rmse(independentSquaredError, independentCount),
            Rmse(mixedSquaredError, mixedCount),
            Rmse(ambiguousSquaredError, ambiguousCount),
            trueEchoPairs == 0 ? 1.0 : (double)recoveredEchoPairs / trueEchoPairs,
            independentPairs == 0 ? 0.0 : (double)falseMergedPairs / independentPairs,
            scenario.Cells.Length == 0 ? 0.0 : (double)totalEffectiveGroups / scenario.Cells.Length,
            scenario.Cells.Length == 0 ? 0.0 : (double)totalTrueRoots / scenario.Cells.Length,
            communicationPackets,
            communicationWork);

        context.Emit(
            ExperimentFrameKind.DevelopmentalEvent,
            ExperimentName,
            series,
            scenario.Cells.Length,
            "path-complete",
            message: $"{series} completed with RMSE {outcome.Rmse:0.000000}, echo-trap RMSE {outcome.EchoTrapRmse:0.000000}, echo recall {outcome.EchoPairRecall:0.000}, and false-merge rate {outcome.FalseMergeRate:0.000}.",
            metrics: outcome.ToMetrics());
        return outcome;
    }

    private static List<ReportGroup> CreateNaiveGroups(AncestryReport[] reports)
    {
        var groups = new List<ReportGroup>(reports.Length);
        for (var index = 0; index < reports.Length; index++)
        {
            groups.Add(new ReportGroup([reports[index]]));
        }

        return groups;
    }

    private static List<ReportGroup> CreateOracleGroups(AncestryReport[] reports)
    {
        var byRoot = new Dictionary<string, List<AncestryReport>>(StringComparer.Ordinal);
        for (var index = 0; index < reports.Length; index++)
        {
            var report = reports[index];
            if (!byRoot.TryGetValue(report.TrueRootId, out var group))
            {
                group = [];
                byRoot.Add(report.TrueRootId, group);
            }

            group.Add(report);
        }

        return byRoot.Values.Select(group => new ReportGroup(group.ToArray())).ToList();
    }

    private static List<ReportGroup> CreateInferredGroups(AncestryReport[] reports)
    {
        var parents = new int[reports.Length];
        for (var index = 0; index < parents.Length; index++)
        {
            parents[index] = index;
        }

        for (var left = 0; left < reports.Length; left++)
        {
            for (var right = left + 1; right < reports.Length; right++)
            {
                if (ShouldMerge(reports[left], reports[right]))
                {
                    Union(parents, left, right);
                }
            }
        }

        var groups = new Dictionary<int, List<AncestryReport>>();
        for (var index = 0; index < reports.Length; index++)
        {
            var root = Find(parents, index);
            if (!groups.TryGetValue(root, out var group))
            {
                group = [];
                groups.Add(root, group);
            }

            group.Add(reports[index]);
        }

        return groups.Values.Select(group => new ReportGroup(group.ToArray())).ToList();
    }

    private static bool ShouldMerge(AncestryReport left, AncestryReport right)
    {
        if (left.OriginHint is not null && string.Equals(left.OriginHint, right.OriginHint, StringComparison.Ordinal))
        {
            return true;
        }

        return SignatureDistance(left.Signature, right.Signature) <= SignatureMergeDistance;
    }

    private static int Find(int[] parents, int index)
    {
        var current = index;
        while (parents[current] != current)
        {
            parents[current] = parents[parents[current]];
            current = parents[current];
        }

        return current;
    }

    private static void Union(int[] parents, int left, int right)
    {
        var leftRoot = Find(parents, left);
        var rightRoot = Find(parents, right);
        if (leftRoot != rightRoot)
        {
            parents[rightRoot] = leftRoot;
        }
    }

    private static double PredictFromGroups(List<ReportGroup> groups)
    {
        var numerator = 0.0;
        var denominator = 0.0;
        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var group = groups[groupIndex];
            var groupNumerator = 0.0;
            var groupDenominator = 0.0;
            var groupSupport = 0.0;
            for (var reportIndex = 0; reportIndex < group.Reports.Length; reportIndex++)
            {
                var report = group.Reports[reportIndex];
                var support = ReportSupport(report);
                groupNumerator += report.Estimate * support;
                groupDenominator += support;
                groupSupport = Math.Max(groupSupport, support);
            }

            if (groupDenominator <= 1e-12 || groupSupport <= 1e-12)
            {
                continue;
            }

            numerator += (groupNumerator / groupDenominator) * groupSupport;
            denominator += groupSupport;
        }

        return denominator <= 1e-12 ? 0.0 : numerator / denominator;
    }

    private static double ReportSupport(AncestryReport report)
    {
        var evidenceConfidence = 1.0 - Math.Exp(-report.EvidenceCount / 24.0);
        return report.Standing * (0.55 + (0.45 * evidenceConfidence));
    }

    private static Dictionary<string, double> BuildResultMetrics(
        EpistemicAncestryScenario scenario,
        PathOutcome inferred,
        PathOutcome naive,
        PathOutcome oracle) => new(StringComparer.Ordinal)
    {
        ["scenario_fingerprint_low32"] = (double)(scenario.Fingerprint & uint.MaxValue),
        ["echo_trap_contexts"] = EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.EchoTrap),
        ["independent_convergence_contexts"] = EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.IndependentConvergence),
        ["missing_origin_rate"] = EpistemicAncestryWorld.MissingOriginRate(scenario),
        ["immediate_sender_hint_rate"] = EpistemicAncestryWorld.ImmediateSenderHintRate(scenario),
        ["inferred_rmse"] = inferred.Rmse,
        ["naive_rmse"] = naive.Rmse,
        ["oracle_rmse"] = oracle.Rmse,
        ["inferred_echo_trap_rmse"] = inferred.EchoTrapRmse,
        ["naive_echo_trap_rmse"] = naive.EchoTrapRmse,
        ["oracle_echo_trap_rmse"] = oracle.EchoTrapRmse,
        ["inferred_independent_rmse"] = inferred.IndependentConvergenceRmse,
        ["naive_independent_rmse"] = naive.IndependentConvergenceRmse,
        ["oracle_independent_rmse"] = oracle.IndependentConvergenceRmse,
        ["inferred_mixed_rmse"] = inferred.MixedLineageRmse,
        ["naive_mixed_rmse"] = naive.MixedLineageRmse,
        ["inferred_ambiguous_rmse"] = inferred.AmbiguousLineageRmse,
        ["naive_ambiguous_rmse"] = naive.AmbiguousLineageRmse,
        ["inferred_echo_pair_recall"] = inferred.EchoPairRecall,
        ["inferred_false_merge_rate"] = inferred.FalseMergeRate,
        ["inferred_mean_effective_groups"] = inferred.MeanEffectiveGroups,
        ["oracle_mean_true_roots"] = oracle.MeanTrueRoots,
        ["inferred_communication_work"] = inferred.CommunicationWork,
        ["naive_communication_work"] = naive.CommunicationWork,
        ["oracle_communication_work"] = oracle.CommunicationWork,
        ["inferred_packet_count"] = inferred.CommunicationPacketCount,
        ["naive_packet_count"] = naive.CommunicationPacketCount,
        ["oracle_packet_count"] = oracle.CommunicationPacketCount,
    };

    private static List<ExperimentAssertion> BuildAssertions(
        EpistemicAncestryScenario scenario,
        PathOutcome inferred,
        PathOutcome naive,
        PathOutcome oracle)
    {
        var echoContexts = EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.EchoTrap);
        var independentContexts = EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.IndependentConvergence);
        var missingRate = EpistemicAncestryWorld.MissingOriginRate(scenario);
        var immediateRate = EpistemicAncestryWorld.ImmediateSenderHintRate(scenario);
        var expectedPackets = EpistemicAncestryWorld.ContextCount * EpistemicAncestryWorld.PeerCount;
        var expectedWork = expectedPackets * PacketCost;
        return
        [
            new ExperimentAssertion(
                "seed-generates-incomplete-ancestry-circumstance",
                echoContexts >= 3 && independentContexts >= 3 && missingRate >= 0.30,
                "Every seed must contain both echo traps and genuine independent convergence while at least 30% of public reports omit an origin hint.",
                missingRate,
                0.30),
            new ExperimentAssertion(
                "provenance-is-partial-rather-than-clean",
                immediateRate >= 0.20,
                "At least 20% of reports must carry an immediate-sender alias rather than the hidden upstream root, so explicit labels alone cannot solve ancestry.",
                immediateRate,
                0.20),
            new ExperimentAssertion(
                "echoed-agreement-is-discounted",
                inferred.EchoTrapRmse <= naive.EchoTrapRmse * 0.90,
                "On echo-trap contexts, inferred ancestry must reduce RMSE by at least 10% relative to counting every peer report as independent corroboration.",
                inferred.EchoTrapRmse,
                naive.EchoTrapRmse * 0.90),
            new ExperimentAssertion(
                "ancestry-inference-improves-whole-history-judgment",
                inferred.Rmse <= naive.Rmse * 0.88,
                "Across the entire generated world, inferred ancestry must reduce RMSE by at least 12% relative to naive agreement counting.",
                inferred.Rmse,
                naive.Rmse * 0.88),
            new ExperimentAssertion(
                "independent-convergence-remains-independent",
                inferred.IndependentConvergenceRmse <= (naive.IndependentConvergenceRmse * 1.15) + 1e-9,
                "The ancestry heuristic must not purchase echo resistance by collapsing genuinely independent convergence. Independent-context RMSE may be at most 15% worse than naive counting.",
                inferred.IndependentConvergenceRmse,
                naive.IndependentConvergenceRmse * 1.15),
            new ExperimentAssertion(
                "shared-ancestry-is-recovered-from-partial-cues",
                inferred.EchoPairRecall >= 0.85,
                "At least 85% of peer-report pairs that truly share one hidden root should be placed in the same inferred ancestry group.",
                inferred.EchoPairRecall,
                0.85),
            new ExperimentAssertion(
                "independent-roots-are-not-overmerged",
                inferred.FalseMergeRate <= 0.12,
                "No more than 12% of report pairs from independent hidden roots may be falsely merged by the ancestry heuristic.",
                inferred.FalseMergeRate,
                0.12),
            new ExperimentAssertion(
                "inference-approaches-perfect-ancestry-with-bounded-public-data",
                inferred.Rmse <= oracle.Rmse + 0.03 &&
                inferred.CommunicationPacketCount == expectedPackets &&
                naive.CommunicationPacketCount == expectedPackets &&
                oracle.CommunicationPacketCount == expectedPackets &&
                Math.Abs(inferred.CommunicationWork - expectedWork) <= 1e-9,
                "The incomplete-ancestry path must remain within 0.03 RMSE of the hidden-truth oracle while using exactly the same bounded public report set as both controls.",
                inferred.Rmse - oracle.Rmse,
                0.03),
        ];
    }

    private static MindPublicState[] PublicMindState(
        string series,
        EpistemicAncestryCell cell,
        double prediction)
    {
        var meanStanding = cell.Reports.Length == 0 ? 0.0 : cell.Reports.Average(ReportSupport);
        return
        [
            new MindPublicState(
                series,
                0,
                cell.Reports.Length,
                0.0,
                meanStanding,
                prediction,
                cell.Target,
                Math.Abs(prediction - cell.Target)),
        ];
    }

    private static TracePublicState[] PublicTraceStates(string series, EpistemicAncestryCell cell)
    {
        var traces = new TracePublicState[cell.Reports.Length];
        for (var index = 0; index < traces.Length; index++)
        {
            var report = cell.Reports[index];
            traces[index] = new TracePublicState(
                series,
                cell.ContextCell,
                TraceProvenance.Foreign,
                report.SenderMindId,
                report.OriginHint ?? "unknown",
                report.Estimate,
                ReportSupport(report),
                0,
                report.EvidenceCount);
        }

        return traces;
    }

    private static void AddKindError(
        AncestryContextKind kind,
        double squaredError,
        ref double echoSquaredError,
        ref int echoCount,
        ref double independentSquaredError,
        ref int independentCount,
        ref double mixedSquaredError,
        ref int mixedCount,
        ref double ambiguousSquaredError,
        ref int ambiguousCount)
    {
        switch (kind)
        {
            case AncestryContextKind.EchoTrap:
                echoSquaredError += squaredError;
                echoCount++;
                break;
            case AncestryContextKind.IndependentConvergence:
                independentSquaredError += squaredError;
                independentCount++;
                break;
            case AncestryContextKind.MixedLineage:
                mixedSquaredError += squaredError;
                mixedCount++;
                break;
            case AncestryContextKind.AmbiguousLineage:
                ambiguousSquaredError += squaredError;
                ambiguousCount++;
                break;
        }
    }

    private static double SignatureDistance(AncestrySignature left, AncestrySignature right)
    {
        var a = left.A - right.A;
        var b = left.B - right.B;
        var c = left.C - right.C;
        return Math.Sqrt((a * a) + (b * b) + (c * c));
    }

    private static double Rmse(double squaredError, int count) =>
        count == 0 ? 0.0 : Math.Sqrt(squaredError / count);

    private enum AncestryPathMode
    {
        Inferred,
        Naive,
        Oracle,
    }

    private sealed record ReportGroup(AncestryReport[] Reports);

    private readonly record struct PairStats(
        int TrueEchoPairs,
        int RecoveredEchoPairs,
        int IndependentPairs,
        int FalseMergedPairs)
    {
        public double EchoRecall => TrueEchoPairs == 0 ? 1.0 : (double)RecoveredEchoPairs / TrueEchoPairs;

        public double FalseMergeRate => IndependentPairs == 0 ? 0.0 : (double)FalseMergedPairs / IndependentPairs;

        public static PairStats For(AncestryReport[] reports, List<ReportGroup> groups)
        {
            var groupBySender = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                var group = groups[groupIndex];
                for (var reportIndex = 0; reportIndex < group.Reports.Length; reportIndex++)
                {
                    groupBySender[group.Reports[reportIndex].SenderMindId] = groupIndex;
                }
            }

            var trueEchoPairs = 0;
            var recoveredEchoPairs = 0;
            var independentPairs = 0;
            var falseMergedPairs = 0;
            for (var left = 0; left < reports.Length; left++)
            {
                for (var right = left + 1; right < reports.Length; right++)
                {
                    var sameRoot = string.Equals(reports[left].TrueRootId, reports[right].TrueRootId, StringComparison.Ordinal);
                    var sameGroup = groupBySender[reports[left].SenderMindId] == groupBySender[reports[right].SenderMindId];
                    if (sameRoot)
                    {
                        trueEchoPairs++;
                        if (sameGroup)
                        {
                            recoveredEchoPairs++;
                        }
                    }
                    else
                    {
                        independentPairs++;
                        if (sameGroup)
                        {
                            falseMergedPairs++;
                        }
                    }
                }
            }

            return new PairStats(trueEchoPairs, recoveredEchoPairs, independentPairs, falseMergedPairs);
        }
    }

    private sealed record PathOutcome(
        double Rmse,
        double EchoTrapRmse,
        double IndependentConvergenceRmse,
        double MixedLineageRmse,
        double AmbiguousLineageRmse,
        double EchoPairRecall,
        double FalseMergeRate,
        double MeanEffectiveGroups,
        double MeanTrueRoots,
        int CommunicationPacketCount,
        double CommunicationWork)
    {
        public Dictionary<string, double> ToMetrics() => new(StringComparer.Ordinal)
        {
            ["rmse"] = Rmse,
            ["echo_trap_rmse"] = EchoTrapRmse,
            ["independent_convergence_rmse"] = IndependentConvergenceRmse,
            ["mixed_lineage_rmse"] = MixedLineageRmse,
            ["ambiguous_lineage_rmse"] = AmbiguousLineageRmse,
            ["echo_pair_recall"] = EchoPairRecall,
            ["false_merge_rate"] = FalseMergeRate,
            ["mean_effective_groups"] = MeanEffectiveGroups,
            ["mean_true_roots"] = MeanTrueRoots,
            ["communication_packet_count"] = CommunicationPacketCount,
            ["communication_work"] = CommunicationWork,
        };
    }
}
