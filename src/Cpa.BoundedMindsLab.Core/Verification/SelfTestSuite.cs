using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Challenge;
using Cpa.BoundedMindsLab.Communication;
using Cpa.BoundedMindsLab.Development;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;
using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Validation;

namespace Cpa.BoundedMindsLab.Verification;

public static class SelfTestSuite
{
    public static IReadOnlyList<string> RunAll()
    {
        var passed = new List<string>();
        Run("deterministic-random-repeats", TestDeterministicRandom, passed);
        Run("shared-channel-explicit-cost", TestSharedChannelCost, passed);
        Run("provisional-import-preserves-foreign-provenance", TestProvisionalImport, passed);
        Run("lived-equivalent-control-imports-local-authority", TestLivedEquivalentImport, passed);
        Run("foreign-standing-falls-under-local-contradiction", TestForeignStandingFalls, passed);
        Run("foreign-standing-rises-under-local-confirmation", TestForeignStandingRises, passed);
        Run("public-export-requires-earned-standing", TestExportStanding, passed);
        Run("protocol-01-development-fixture-seed-101", TestProtocolOne, passed);
        Run("protocol-02-development-fixture-seed-101", TestProtocolTwo, passed);
        Run("protocol-03-default-seeds-create-distinct-lived-histories", TestProtocolThreeSeedSemantics, passed);
        Run("protocol-03-development-fixture-seed-101", TestProtocolThree, passed);
        Run("protocol-04-default-seeds-create-distinct-social-histories", TestProtocolFourSeedSemantics, passed);
        Run("protocol-04-development-fixture-seed-101", TestProtocolFour, passed);
        Run("protocol-05-default-seeds-create-distinct-coordination-worlds", TestProtocolFiveSeedSemantics, passed);
        Run("protocol-05-development-fixture-seed-101", TestProtocolFive, passed);
        Run("protocol-06-default-seeds-create-distinct-incomplete-ancestry-worlds", TestProtocolSixSeedSemantics, passed);
        Run("protocol-06-development-fixture-seed-101", TestProtocolSix, passed);
        Run("protocol-07-default-seeds-create-distinct-standing-transfer-worlds", TestProtocolSevenSeedSemantics, passed);
        Run("protocol-07-development-fixture-seed-101", TestProtocolSeven, passed);
        Run("validation-seed-sets-are-frozen-and-disjoint", TestValidationSeedSets, passed);
        Run("validation-check-taxonomy-separates-evidence-types", TestValidationTaxonomy, passed);
        Run("validation-report-identifies-development-regression", TestValidationReport, passed);
        Run("challenge-v1-selection-is-deterministic", TestChallengeSelectionDeterminism, passed);
        Run("challenge-v1-excludes-consumed-seeds", TestChallengeSelectionExcludesConsumedSeeds, passed);
        Run("challenge-v1-spans-monotonic-stress-bands", TestChallengeStressBands, passed);
        Run("frame-sequence-is-contiguous", TestFrameSequence, passed);
        return passed;
    }

    private static void TestDeterministicRandom()
    {
        var left = Enumerable.Range(0, 64).ToList();
        var right = Enumerable.Range(0, 64).ToList();
        new DeterministicRandom(211).Shuffle(left);
        new DeterministicRandom(211).Shuffle(right);
        Assert(left.SequenceEqual(right), "Equal seeds must produce equal shuffles.");
    }

    private static void TestSharedChannelCost()
    {
        var channel = new SharedTraceChannel(0.25);
        channel.Publish(new PublicTracePacket("a", "a:1", 1, 0.5, 0.9, 12));
        channel.Publish(new PublicTracePacket("a", "a:2", 2, 0.2, 0.8, 9));
        Assert(channel.PacketCount == 2, "Two packets should be visible.");
        Assert(Math.Abs(channel.CommunicationWork - 0.5) < 1e-12, "Communication work must be explicit and additive.");
    }

    private static void TestProvisionalImport()
    {
        var memory = new DevelopmentalMemory("receiver");
        memory.ImportProvisional(new PublicTracePacket("source", "source:3", 3, 0.7, 1.0, 80));
        Assert(memory.ForeignTraceCount == 1 && memory.LocalTraceCount == 0, "Provisional transfer must remain foreign.");
        Assert(memory.StandingFor(3, TraceProvenance.Foreign) <= 0.42 + 1e-12, "Foreign standing must be capped on admission.");
    }

    private static void TestLivedEquivalentImport()
    {
        var memory = new DevelopmentalMemory("receiver");
        memory.ImportAsLived(new PublicTracePacket("source", "source:3", 3, 0.7, 0.95, 80));
        Assert(memory.LocalTraceCount == 1 && memory.ForeignTraceCount == 0, "Control transfer must enter local authority.");
        Assert(Math.Abs(memory.StandingFor(3, TraceProvenance.Direct) - 0.95) < 1e-12, "Control must inherit source standing.");
    }

    private static void TestForeignStandingFalls()
    {
        var memory = new DevelopmentalMemory("receiver");
        memory.ImportProvisional(new PublicTracePacket("source", "source:6", 6, 0.8, 1.0, 80));
        var before = memory.StandingFor(6, TraceProvenance.Foreign);
        for (var index = 0; index < 8; index++)
        {
            memory.Predict(6);
            memory.ObserveDirect(6, -0.8);
        }

        Assert(memory.StandingFor(6, TraceProvenance.Foreign) < before * 0.05, "Contradictory local consequence should sharply reduce foreign standing.");
    }

    private static void TestForeignStandingRises()
    {
        var memory = new DevelopmentalMemory("receiver");
        memory.ImportProvisional(new PublicTracePacket("source", "source:2", 2, 0.55, 0.8, 80));
        var before = memory.StandingFor(2, TraceProvenance.Foreign);
        for (var index = 0; index < 12; index++)
        {
            memory.Predict(2);
            memory.ObserveDirect(2, 0.55);
        }

        Assert(memory.StandingFor(2, TraceProvenance.Foreign) > before, "Confirming local consequence should renew compatible foreign standing.");
    }

    private static void TestExportStanding()
    {
        var memory = new DevelopmentalMemory("source");
        Assert(memory.ExportPublicTraces().Count == 0, "Unexperienced memory must publish nothing.");
        foreach (var cell in TransferContaminationWorld.CreateSourceSchedule(307))
        {
            memory.Predict(cell);
            memory.ObserveDirect(cell, TransferContaminationWorld.SourceTarget(cell));
        }

        Assert(memory.ExportPublicTraces().Count == TransferContaminationWorld.ContextCount, "Repeated useful direct traces should become publishable.");
    }

    private static void TestProtocolOne()
    {
        var output = CreateTemporaryDirectory();
        try
        {
            var run = ExperimentRunner.Run([ExperimentCatalog.Get("01-local-shared-memory-contamination")], 101, output, quiet: true);
            Assert(run.Experiments.Single().Verdict == ExperimentVerdict.Support, "Protocol 01 should meet its preregistered synthetic boundaries for seed 101.");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static void TestProtocolTwo()
    {
        var output = CreateTemporaryDirectory();
        try
        {
            var run = ExperimentRunner.Run([ExperimentCatalog.Get("02-peer-disagreement-preserved-interiors")], 101, output, quiet: true);
            Assert(run.Experiments.Single().Verdict == ExperimentVerdict.Support, "Protocol 02 should meet its preregistered synthetic boundaries for seed 101.");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }


    private static void TestProtocolThreeSeedSemantics()
    {
        var fingerprints = new HashSet<ulong>();
        var categoryLayouts = new HashSet<string>(StringComparer.Ordinal);
        foreach (var seed in ExperimentDefaults.DevelopmentSeeds)
        {
            var scenario = DevelopmentalTransferWorld.CreateScenario(seed);
            Assert(fingerprints.Add(scenario.Fingerprint), $"Default seed {seed} should produce a distinct developmental-world fingerprint.");
            var layout = string.Join(",", scenario.Cells.Select(cell => (int)cell.HistoryKind));
            categoryLayouts.Add(layout);
        }

        Assert(
            categoryLayouts.Count >= 4,
            "The canonical five-seed matrix should vary which contexts receive stable, divergent, unstable, and sparse histories, not merely shuffle observation order.");
    }

    private static void TestProtocolThree()
    {
        var output = CreateTemporaryDirectory();
        try
        {
            var run = ExperimentRunner.Run([ExperimentCatalog.Get("03-developmental-versus-doctrinal-transfer")], 101, output, quiet: true);
            Assert(run.Experiments.Single().Verdict == ExperimentVerdict.Support, "Protocol 03 should meet its preregistered synthetic boundaries for seed 101.");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }


    private static void TestProtocolFourSeedSemantics()
    {
        var fingerprints = new HashSet<ulong>();
        var categoryLayouts = new HashSet<string>(StringComparer.Ordinal);
        foreach (var seed in ExperimentDefaults.DevelopmentSeeds)
        {
            var scenario = CommunicationBeforeLanguageWorld.CreateScenario(seed);
            Assert(fingerprints.Add(scenario.Fingerprint), $"Default seed {seed} should produce a distinct social-history fingerprint.");
            var layout = string.Join(",", scenario.Cells.Select(cell => (int)cell.HistoryKind));
            categoryLayouts.Add(layout);
            Assert(CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.InformativeDissent) >= 2, "Each Protocol 04 world should contain informative dissent.");
            Assert(CommunicationBeforeLanguageWorld.CountKind(scenario, CommunicationHistoryKind.MisleadingDissent) >= 2, "Each Protocol 04 world should contain misleading dissent.");
        }

        Assert(
            categoryLayouts.Count >= 4,
            "The canonical five-seed matrix should vary the placement and prevalence of social-history conditions, not merely shuffle one fixed communication sequence.");
    }

    private static void TestProtocolFour()
    {
        var output = CreateTemporaryDirectory();
        try
        {
            var run = ExperimentRunner.Run([ExperimentCatalog.Get("04-bounded-communication-before-language")], 101, output, quiet: true);
            Assert(run.Experiments.Single().Verdict == ExperimentVerdict.Support, "Protocol 04 should meet its preregistered synthetic boundaries for seed 101.");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static void TestProtocolFiveSeedSemantics()
    {
        var fingerprints = new HashSet<ulong>();
        var shiftedLayouts = new HashSet<string>(StringComparer.Ordinal);
        foreach (var seed in ExperimentDefaults.DevelopmentSeeds)
        {
            var scenario = EmergentConventionWorld.CreateScenario(seed);
            Assert(fingerprints.Add(scenario.Fingerprint), $"Default seed {seed} should produce a distinct coordination-world fingerprint.");
            var layout = string.Join(",", scenario.Cells.Select(cell => cell.ContextKind == ConventionContextKind.Shifted ? "1" : "0"));
            shiftedLayouts.Add(layout);
            Assert(EmergentConventionWorld.CountKind(scenario, ConventionContextKind.Shifted) is >= 4 and <= 6, "Each Protocol 05 world should contain four to six changed contexts.");
            Assert(EmergentConventionWorld.CountPreferenceDiverseContexts(scenario) >= 8, "Each Protocol 05 world should contain substantial private preference plurality before convention formation.");
        }

        Assert(
            shiftedLayouts.Count >= 4,
            "The canonical five-seed matrix should vary which contexts undergo later coordination pressure, not merely shuffle one fixed convention history.");
    }

    private static void TestProtocolFive()
    {
        var output = CreateTemporaryDirectory();
        try
        {
            var run = ExperimentRunner.Run([ExperimentCatalog.Get("05-emergent-convention-artificial-culture")], 101, output, quiet: true);
            Assert(run.Experiments.Single().Verdict == ExperimentVerdict.Support, "Protocol 05 should meet its preregistered synthetic boundaries for seed 101.");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }


    private static void TestProtocolSixSeedSemantics()
    {
        var fingerprints = new HashSet<ulong>();
        var layouts = new HashSet<string>(StringComparer.Ordinal);
        foreach (var seed in ExperimentDefaults.DevelopmentSeeds)
        {
            var scenario = EpistemicAncestryWorld.CreateScenario(seed);
            Assert(fingerprints.Add(scenario.Fingerprint), $"Default seed {seed} should produce a distinct incomplete-ancestry fingerprint.");
            var layout = string.Join(",", scenario.Cells.Select(cell => (int)cell.ContextKind));
            layouts.Add(layout);
            Assert(EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.EchoTrap) >= 3, "Each Protocol 06 world should contain repeated-source echo traps.");
            Assert(EpistemicAncestryWorld.CountKind(scenario, AncestryContextKind.IndependentConvergence) >= 3, "Each Protocol 06 world should contain genuine independent convergence.");
            Assert(EpistemicAncestryWorld.MissingOriginRate(scenario) >= 0.30, "Each Protocol 06 world should omit explicit ancestry on a substantial fraction of reports.");
        }

        Assert(
            layouts.Count >= 4,
            "The canonical five-seed matrix should vary the placement and prevalence of ancestry conditions, not merely reorder one fixed report set.");
    }

    private static void TestProtocolSix()
    {
        var output = CreateTemporaryDirectory();
        try
        {
            var run = ExperimentRunner.Run([ExperimentCatalog.Get("06-incomplete-epistemic-ancestry")], 101, output, quiet: true);
            Assert(run.Experiments.Single().Verdict == ExperimentVerdict.Support, "Protocol 06 should meet its preregistered synthetic boundaries for seed 101.");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static void TestProtocolSevenSeedSemantics()
    {
        var fingerprints = new HashSet<ulong>();
        var layouts = new HashSet<string>(StringComparer.Ordinal);
        var recommenderCredibilities = new HashSet<int>();
        foreach (var seed in ExperimentDefaults.DevelopmentSeeds)
        {
            var scenario = StandingTransferWorld.CreateScenario(seed);
            Assert(fingerprints.Add(scenario.Fingerprint), $"Default seed {seed} should produce a distinct standing-transfer fingerprint.");
            layouts.Add(string.Join(",", scenario.Cells.Select(cell => (int)cell.ContextKind)));
            recommenderCredibilities.Add((int)Math.Round(scenario.RecommenderCredibility * 1000.0));
            Assert(StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.StrongTransferable) >= 3, "Each Protocol 07 world should contain strongly recommended transferable contexts.");
            Assert(StandingTransferWorld.CountKind(scenario, StandingTransferContextKind.StrongLocalMismatch) >= 3, "Each Protocol 07 world should contain strongly recommended relationships that fail to generalize locally.");
        }

        Assert(layouts.Count >= 4, "The canonical Protocol 07 seeds should vary which contexts inherit useful versus misleading recommendations.");
        Assert(recommenderCredibilities.Count >= 3, "The canonical Protocol 07 seeds should vary C's already-earned standing for recommender A.");
    }

    private static void TestProtocolSeven()
    {
        var output = CreateTemporaryDirectory();
        try
        {
            var run = ExperimentRunner.Run([ExperimentCatalog.Get("07-provisional-standing-transfer")], 101, output, quiet: true);
            Assert(run.Experiments.Single().Verdict == ExperimentVerdict.Support, "Protocol 07 should meet its preregistered synthetic boundaries for seed 101.");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }


    private static void TestValidationSeedSets()
    {
        Assert(ExperimentDefaults.DevelopmentSeeds.Count == 5, "development-v1 must remain the frozen five-seed regression set.");
        Assert(ExperimentDefaults.HoldoutSeeds.Count == 20, "holdout-v1 must contain exactly twenty preregistered seeds.");
        Assert(ExperimentDefaults.DevelopmentSeeds.Distinct().Count() == ExperimentDefaults.DevelopmentSeeds.Count, "Development seeds must be unique.");
        Assert(ExperimentDefaults.HoldoutSeeds.Distinct().Count() == ExperimentDefaults.HoldoutSeeds.Count, "Holdout seeds must be unique.");
        Assert(!ExperimentDefaults.DevelopmentSeeds.Intersect(ExperimentDefaults.HoldoutSeeds).Any(), "Development and holdout seeds must remain disjoint.");
        Assert(ValidationPlan.ClassifySeedSet(ExperimentDefaults.DevelopmentSeeds) == ValidationPlan.DevelopmentSetName, "Development seeds must classify as development-v1.");
        Assert(ValidationPlan.ClassifySeedSet(ExperimentDefaults.HoldoutSeeds) == ValidationPlan.HoldoutSetName, "Holdout seeds must classify as holdout-v1.");
    }

    private static void TestValidationTaxonomy()
    {
        Assert(ValidationCheckTaxonomy.Classify("seed-generates-lived-circumstance") == ValidationCheckTaxonomy.Manipulation, "Seed-generation checks should be manipulation checks.");
        Assert(ValidationCheckTaxonomy.Classify("whole-history-benefit") == ValidationCheckTaxonomy.MechanismOutcome, "Whole-history outcome checks should remain mechanism evidence.");
        Assert(ValidationCheckTaxonomy.Classify("independent-roots-are-not-overmerged") == ValidationCheckTaxonomy.SafetyBoundary, "Overmerge protection should be a safety-boundary check.");
        Assert(ValidationCheckTaxonomy.Classify("bounded-developmental-transfer") == ValidationCheckTaxonomy.AccountingConstraint, "Communication accounting should not be counted as mechanism evidence.");
    }

    private static void TestValidationReport()
    {
        var assertion = new ExperimentAssertion("whole-history-benefit", true, "synthetic validation-report fixture");
        var result = new ExperimentResult(
            "synthetic-protocol",
            "fixture",
            ExperimentVerdict.Support,
            "fixture",
            new Dictionary<string, double>(StringComparer.Ordinal),
            [assertion]);
        var runs = ExperimentDefaults.DevelopmentSeeds
            .Select(seed => new RunResult(seed, string.Empty, [result]))
            .ToArray();
        var report = ValidationReportBuilder.Create(runs);
        Assert(report.SeedSet == ValidationPlan.DevelopmentSetName, "The canonical five seeds must be labeled development data in validation reports.");
        Assert(report.Diagnostics.Any(message => message.Contains("development set", StringComparison.OrdinalIgnoreCase)), "Development-set validation reports must warn against fresh-validation interpretation.");
        Assert(report.Categories.Single(category => category.Category == ValidationCheckTaxonomy.MechanismOutcome).Checks == ExperimentDefaults.DevelopmentSeeds.Count, "Mechanism checks must be tallied separately from accounting and manipulation checks.");
    }

    private static void TestChallengeSelectionDeterminism()
    {
        var first = ChallengePlan.BuildSelections();
        var second = ChallengePlan.BuildSelections();
        Assert(first.Count == ChallengePlan.Profiles.Count * ChallengePlan.BandCount * ChallengePlan.SeedsPerBand, "challenge-v1 must select the registered number of runs.");
        Assert(first.SequenceEqual(second), "challenge-v1 seed selection must be deterministic and depend only on frozen world descriptors.");
    }

    private static void TestChallengeSelectionExcludesConsumedSeeds()
    {
        var consumed = ExperimentDefaults.DevelopmentSeeds.Concat(ExperimentDefaults.HoldoutSeeds).ToHashSet();
        var selections = ChallengePlan.BuildSelections();
        Assert(selections.All(selection => !consumed.Contains(selection.Seed)), "challenge-v1 must not reuse development-v1 or consumed holdout-v1 seeds.");
        foreach (var profile in ChallengePlan.Profiles)
        {
            var profileSeeds = selections.Where(selection => selection.ProfileId == profile.Id).Select(selection => selection.Seed).ToArray();
            Assert(profileSeeds.Distinct().Count() == profileSeeds.Length, $"Challenge profile {profile.Id} must not repeat a selected seed across bands.");
        }
    }

    private static void TestChallengeStressBands()
    {
        var selections = ChallengePlan.BuildSelections();
        foreach (var profile in ChallengePlan.Profiles)
        {
            var groups = selections
                .Where(selection => selection.ProfileId == profile.Id)
                .GroupBy(selection => selection.BandIndex)
                .OrderBy(group => group.Key)
                .ToArray();
            Assert(groups.Length == ChallengePlan.BandCount, $"Challenge profile {profile.Id} must contain all registered stress bands.");
            var priorMaximum = double.NegativeInfinity;
            foreach (var group in groups)
            {
                Assert(group.Count() == ChallengePlan.SeedsPerBand, $"Challenge profile {profile.Id} band {group.Key} must contain the registered number of seeds.");
                var minimum = group.Min(selection => selection.StressScore);
                var maximum = group.Max(selection => selection.StressScore);
                Assert(minimum >= priorMaximum, $"Challenge profile {profile.Id} stress bands must be nondecreasing.");
                priorMaximum = maximum;
            }
        }
    }

    private static void TestFrameSequence()
    {
        var output = CreateTemporaryDirectory();
        var observer = new CollectingObserver();
        try
        {
            ExperimentRunner.Run([ExperimentCatalog.Get("01-local-shared-memory-contamination")], 101, output, quiet: true, observer);
            for (var index = 0; index < observer.Frames.Count; index++)
            {
                Assert(observer.Frames[index].Sequence == index, $"Frame {index} has non-contiguous sequence {observer.Frames[index].Sequence}.");
            }
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cpa-bounded-minds-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static void Run(string name, Action test, List<string> passed)
    {
        test();
        passed.Add(name);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class CollectingObserver : IExperimentFrameObserver
    {
        public List<ExperimentFrame> Frames { get; } = [];

        public void Observe(ExperimentFrame frame) => Frames.Add(frame);
    }
}
