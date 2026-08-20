using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Challenge;
using Cpa.BoundedMindsLab.Communication;
using Cpa.BoundedMindsLab.Development;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;
using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Falsification;
using Cpa.BoundedMindsLab.Validation;

namespace Cpa.BoundedMindsLab.Verification;

public static class SelfTestSuite
{
    private static readonly string[] ClosedProtocolNames =
    [
        "01-local-shared-memory-contamination",
        "02-peer-disagreement-preserved-interiors",
        "03-developmental-versus-doctrinal-transfer",
        "04-bounded-communication-before-language",
        "05-emergent-convention-artificial-culture",
        "06-incomplete-epistemic-ancestry",
        "07-provisional-standing-transfer",
        "08-strategic-public-influence",
        "09-authority-ancestry-circular-standing",
    ];

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
        Run("protocol-08-default-seeds-create-distinct-strategic-influence-worlds", TestProtocolEightSeedSemantics, passed);
        Run("protocol-08-development-fixture-seed-101", TestProtocolEight, passed);
        Run("protocol-09-default-seeds-create-distinct-authority-cascade-worlds", TestProtocolNineSeedSemantics, passed);
        Run("protocol-09-development-fixture-seed-101", TestProtocolNine, passed);
        Run("closeout-catalog-is-exactly-protocols-01-through-09", TestClosedProtocolCatalog, passed);
        Run("validation-seed-sets-are-frozen-and-disjoint", TestValidationSeedSets, passed);
        Run("validation-check-taxonomy-separates-evidence-types", TestValidationTaxonomy, passed);
        Run("validation-report-identifies-development-regression", TestValidationReport, passed);
        Run("challenge-v1-selection-is-deterministic", TestChallengeSelectionDeterminism, passed);
        Run("challenge-v1-excludes-consumed-seeds", TestChallengeSelectionExcludesConsumedSeeds, passed);
        Run("challenge-v1-spans-monotonic-stress-bands", TestChallengeStressBands, passed);
        Run("parameterized-falsification-plan-is-complete", TestParameterizedFalsificationPlan, passed);
        Run("parameterized-p04-comparator-has-equal-public-cost", TestParameterizedProtocolFourComparator, passed);
        Run("parameterized-p06-reaches-complete-provenance-blindness", TestParameterizedProtocolSixBlindness, passed);
        Run("parameterized-p07-separates-prevalence-from-severity", TestParameterizedProtocolSevenAxes, passed);
        Run("p08-holdout-is-frozen-and-disjoint", TestStrategicInfluenceHoldoutSeeds, passed);
        Run("p08-falsification-plan-is-complete", TestStrategicInfluenceFalsificationPlan, passed);
        Run("p08-falsification-null-harm-surface-is-defined", TestStrategicInfluenceNullHarmProbe, passed);
        Run("p09-holdout-is-frozen-and-disjoint", TestAuthorityAncestryHoldoutSeeds, passed);
        Run("p09-falsification-plan-is-complete", TestAuthorityAncestryFalsificationPlan, passed);
        Run("p09-falsification-approximate-ancestry-is-defined", TestAuthorityAncestryApproximateFidelityProbe, passed);
        Run("p09-falsification-null-harm-surface-is-defined", TestAuthorityAncestryNullHarmProbe, passed);
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


    private static void TestProtocolEightSeedSemantics()
    {
        var fingerprints = new HashSet<ulong>();
        var layouts = new HashSet<string>(StringComparer.Ordinal);
        var worldsWithPartialAlignment = 0;
        foreach (var seed in ExperimentDefaults.DevelopmentSeeds)
        {
            var scenario = StrategicInfluenceWorld.CreateScenario(seed);
            Assert(fingerprints.Add(scenario.Fingerprint), $"Default seed {seed} should produce a distinct strategic-influence fingerprint.");
            layouts.Add(string.Join(",", scenario.Cells.Select(cell => (int)cell.ContextKind)));
            Assert(StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Aligned) >= 4, "Each Protocol 08 world should contain several genuinely aligned peer contexts.");
            Assert(StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Divergent) >= 4, "Each Protocol 08 world should contain several strategically divergent peer contexts.");
            Assert(StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.Betrayal) == 2, "Each Protocol 08 world should contain exactly two contexts where alignment later becomes strategic divergence.");
            if (StrategicInfluenceWorld.CountKind(scenario, StrategicInfluenceContextKind.PartialAlignment) > 0)
            {
                worldsWithPartialAlignment++;
            }
        }

        Assert(layouts.Count >= 4, "The canonical Protocol 08 seeds should vary where aligned, divergent, betrayal, and partial-alignment contexts occur.");
        Assert(worldsWithPartialAlignment >= 2, "The canonical Protocol 08 development set should include multiple worlds with partial-alignment contexts rather than only binary agreement/disagreement.");
    }

    private static void TestProtocolEight()
    {
        var output = CreateTemporaryDirectory();
        try
        {
            var run = ExperimentRunner.Run([ExperimentCatalog.Get("08-strategic-public-influence")], 101, output, quiet: true);
            Assert(run.Experiments.Single().Verdict == ExperimentVerdict.Support, "Protocol 08 should meet its preregistered synthetic development boundaries for seed 101.");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }


    private static void TestProtocolNineSeedSemantics()
    {
        var fingerprints = new HashSet<ulong>();
        var layouts = new HashSet<string>(StringComparer.Ordinal);
        var rootOffsets = new HashSet<int>();
        foreach (var seed in ExperimentDefaults.DevelopmentSeeds)
        {
            var scenario = AuthorityAncestryWorld.CreateScenario(seed);
            Assert(fingerprints.Add(scenario.Fingerprint), $"Default seed {seed} should produce a distinct authority-cascade fingerprint.");
            layouts.Add(string.Join(",", scenario.Cells.Select(cell => (int)cell.ContextKind)));
            foreach (var cell in scenario.Cells)
            {
                rootOffsets.Add(cell.RootOffset);
            }

            Assert(AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.IndependentGrounding) == 4, "Each Protocol 09 world should contain four independently grounded authority contexts.");
            Assert(AuthorityAncestryWorld.CountKind(scenario, AuthorityAncestryContextKind.CircularAuthorityTrap) == 4, "Each Protocol 09 world should contain four circular authority traps.");
            Assert(scenario.Cells.Where(cell => cell.ContextKind == AuthorityAncestryContextKind.IndependentGrounding).All(cell => cell.DirectRootCount >= 3), "Independent authority contexts must begin with several direct roots.");
            Assert(scenario.Cells.Where(cell => cell.ContextKind == AuthorityAncestryContextKind.CircularAuthorityTrap).All(cell => cell.DirectRootCount == 1), "Circular authority traps must begin from one weak direct root before permission circulates.");
        }

        Assert(layouts.Count >= 4, "The canonical Protocol 09 seeds should vary where independently grounded, circular, mixed, and sparse authority contexts occur.");
        Assert(rootOffsets.Count >= 4, "Protocol 09 development worlds should vary which peers originate direct authority rather than privilege one fixed network position.");
    }

    private static void TestProtocolNine()
    {
        var output = CreateTemporaryDirectory();
        try
        {
            var run = ExperimentRunner.Run([ExperimentCatalog.Get("09-authority-ancestry-circular-standing")], 101, output, quiet: true);
            Assert(run.Experiments.Single().Verdict == ExperimentVerdict.Support, "Protocol 09 should meet its preregistered synthetic development boundaries for seed 101.");
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }


    private static void TestClosedProtocolCatalog()
    {
        var actual = ExperimentCatalog.All.Select(experiment => experiment.Name).ToArray();
        Assert(actual.SequenceEqual(ClosedProtocolNames), "The v1.0.0 closeout catalog must contain exactly frozen Protocols 01-09 in order; a new social protocol requires a new explicit research decision.");
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
        Assert(ValidationCheckTaxonomy.Classify("strategic-sender-discovers-naive-leverage") == ValidationCheckTaxonomy.Manipulation, "Protocol 08 leverage discovery should remain a manipulation check.");
        Assert(ValidationCheckTaxonomy.Classify("betrayal-remains-correctable") == ValidationCheckTaxonomy.SafetyBoundary, "Protocol 08 betrayal repair should remain a safety-boundary check.");
        Assert(ValidationCheckTaxonomy.Classify("strategic-public-exchange-is-bounded") == ValidationCheckTaxonomy.AccountingConstraint, "Protocol 08 public exchange should remain an accounting check.");
        Assert(ValidationCheckTaxonomy.Classify("recursive-endorsement-amplifies-circular-authority") == ValidationCheckTaxonomy.Manipulation, "Protocol 09 circular amplification should remain a manipulation check.");
        Assert(ValidationCheckTaxonomy.Classify("direct-consequence-revokes-circular-authority") == ValidationCheckTaxonomy.SafetyBoundary, "Protocol 09 revocation should remain a safety-boundary check.");
        Assert(ValidationCheckTaxonomy.Classify("bounded-authority-exchange") == ValidationCheckTaxonomy.AccountingConstraint, "Protocol 09 endorsement traffic should remain an accounting check.");
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

    private static void TestParameterizedFalsificationPlan()
    {
        Assert(ParameterizedFalsificationPlan.Profiles.Count == 6, "parameterized-falsification-v1 should contain the five frozen P03-P07 targets plus a second P07 surface that separates mismatch severity from prevalence.");
        Assert(ParameterizedFalsificationPlan.Profiles.All(profile => profile.XAxis.Values.Length == 7 && profile.YAxis.Values.Length == 7), "Every parameterized falsification profile should expose a 7x7 controlled surface.");
        Assert(ParameterizedFalsificationPlan.Profiles.All(profile => profile.Replicates == ParameterizedFalsificationPlan.ReplicatesPerCell), "Every profile should use the registered replicate count.");
        Assert(ParameterizedFalsificationPlan.Profiles.Select(profile => profile.Id).Distinct(StringComparer.Ordinal).Count() == ParameterizedFalsificationPlan.Profiles.Count, "Parameterized falsification profile identifiers must be unique.");
    }

    private static void TestParameterizedProtocolFourComparator()
    {
        var metrics = ParameterizedProbes.EvaluateProtocol04(0.35, 0.50, 404UL);
        Assert(Math.Abs(metrics["typed_communication_work"] - metrics["equal_budget_communication_work"]) <= 1e-12, "The stronger P04 comparator must receive exactly the same public communication budget as the typed path.");
        Assert(double.IsFinite(metrics["boundary_margin"]), "The equal-budget P04 margin must be finite.");
    }

    private static void TestParameterizedProtocolSixBlindness()
    {
        var metrics = ParameterizedProbes.EvaluateProtocol06(1.0, 0.05, 606UL);
        Assert(double.IsFinite(metrics["boundary_margin"]), "P06 parameterized falsification must remain numerically defined under complete origin-hint missingness and highly overlapping signatures.");
        Assert(metrics["echo_pair_recall"] is >= 0.0 and <= 1.0, "P06 echo recall must remain a probability under complete provenance blindness.");
        Assert(metrics["false_merge_rate"] is >= 0.0 and <= 1.0, "P06 false-merge rate must remain a probability under complete provenance blindness.");
    }

    private static void TestParameterizedProtocolSevenAxes()
    {
        var prevalence = ParameterizedProbes.EvaluateProtocol07Prevalence(0.45, 0.80, 707UL);
        var severity = ParameterizedProbes.EvaluateProtocol07Severity(0.45, 0.80, 707UL);
        Assert(double.IsFinite(prevalence["boundary_margin"]) && double.IsFinite(severity["boundary_margin"]), "Both P07 controlled surfaces must produce finite margins.");
        Assert(Math.Abs(prevalence["mismatch_contexts"] - severity["mismatch_contexts"]) > 1e-12, "P07 prevalence and severity surfaces must not collapse into the same intervention. The prevalence surface varies how many contexts mismatch; the severity surface holds prevalence near one half.");
    }


    private static void TestStrategicInfluenceHoldoutSeeds()
    {
        var seeds = ExperimentDefaults.StrategicInfluenceHoldoutSeeds;
        Assert(seeds.Count == 20, "p08-holdout-v1 must contain exactly twenty preregistered seeds.");
        Assert(seeds.Distinct().Count() == seeds.Count, "p08-holdout-v1 seeds must be unique.");
        var consumed = ExperimentDefaults.DevelopmentSeeds
            .Concat(ExperimentDefaults.HoldoutSeeds)
            .Concat(ChallengePlan.BuildSelections().Select(selection => selection.Seed))
            .ToHashSet();
        Assert(seeds.All(seed => !consumed.Contains(seed)), "p08-holdout-v1 must not reuse development-v1, consumed holdout-v1, or challenge-v1 seeds.");
        Assert(ValidationPlan.ClassifySeedSet(seeds) == ValidationPlan.StrategicInfluenceHoldoutSetName, "The Protocol 08 holdout must classify as p08-holdout-v1.");
    }

    private static void TestStrategicInfluenceFalsificationPlan()
    {
        Assert(StrategicInfluenceFalsificationPlan.Profiles.Count == 5, "Protocol 08 falsification should register five distinct failure-surface profiles.");
        Assert(StrategicInfluenceFalsificationPlan.Profiles.All(profile => profile.XAxis.Values.Length == 7 && profile.YAxis.Values.Length == 7), "Every Protocol 08 falsification profile should expose a 7x7 surface.");
        Assert(StrategicInfluenceFalsificationPlan.Profiles.All(profile => profile.Replicates == StrategicInfluenceFalsificationPlan.ReplicatesPerCell), "Every Protocol 08 falsification profile should use the registered replicate count.");
        Assert(StrategicInfluenceFalsificationPlan.Profiles.Select(profile => profile.Id).Distinct(StringComparer.Ordinal).Count() == StrategicInfluenceFalsificationPlan.Profiles.Count, "Protocol 08 falsification profile identifiers must be unique.");
    }

    private static void TestStrategicInfluenceNullHarmProbe()
    {
        var metrics = StrategicInfluenceProbes.EvaluateAlignedNoiseVersusDelay(0.10, 4.0, 808UL);
        Assert(double.IsFinite(metrics["boundary_margin"]), "The Protocol 08 aligned null-harm probe must produce a finite boundary margin.");
        Assert(metrics["accountable_final_aligned_standing"] is >= 0.0 and <= 1.0, "Protocol 08 aligned standing must remain bounded as a probability-like standing value.");
        Assert(metrics["local_early_aligned_rmse"] >= 0.0 && metrics["accountable_early_aligned_rmse"] >= 0.0, "Protocol 08 aligned error metrics must remain nonnegative.");
    }


    private static void TestAuthorityAncestryHoldoutSeeds()
    {
        var seeds = ExperimentDefaults.AuthorityAncestryHoldoutSeeds;
        Assert(seeds.Count == 20, "p09-holdout-v1 must contain exactly twenty preregistered seeds.");
        Assert(seeds.Distinct().Count() == seeds.Count, "p09-holdout-v1 seeds must be unique.");
        var consumed = ExperimentDefaults.DevelopmentSeeds
            .Concat(ExperimentDefaults.HoldoutSeeds)
            .Concat(ExperimentDefaults.StrategicInfluenceHoldoutSeeds)
            .Concat(ChallengePlan.BuildSelections().Select(selection => selection.Seed))
            .ToHashSet();
        Assert(seeds.All(seed => !consumed.Contains(seed)), "p09-holdout-v1 must not reuse development-v1, either consumed holdout, or challenge-v1 seeds.");
        Assert(ValidationPlan.ClassifySeedSet(seeds) == ValidationPlan.AuthorityAncestryHoldoutSetName, "The Protocol 09 holdout must classify as p09-holdout-v1.");
    }

    private static void TestAuthorityAncestryFalsificationPlan()
    {
        Assert(AuthorityAncestryFalsificationPlan.Profiles.Count == 6, "Protocol 09 falsification should register six distinct operating-envelope profiles.");
        Assert(AuthorityAncestryFalsificationPlan.Profiles.All(profile => profile.XAxis.Values.Length == 7 && profile.YAxis.Values.Length == 7), "Every Protocol 09 falsification profile should expose a 7x7 surface.");
        Assert(AuthorityAncestryFalsificationPlan.Profiles.All(profile => profile.Replicates == AuthorityAncestryFalsificationPlan.ReplicatesPerCell), "Every Protocol 09 falsification profile should use the registered replicate count.");
        Assert(AuthorityAncestryFalsificationPlan.Profiles.Select(profile => profile.Id).Distinct(StringComparer.Ordinal).Count() == AuthorityAncestryFalsificationPlan.Profiles.Count, "Protocol 09 falsification profile identifiers must be unique.");
    }

    private static void TestAuthorityAncestryApproximateFidelityProbe()
    {
        var metrics = AuthorityAncestryProbes.EvaluateCirculationDepthVersusAncestryFidelity(8.0, 0.50, 909UL);
        Assert(double.IsFinite(metrics["boundary_margin"]), "Protocol 09 ancestry-fidelity probe must remain numerically defined with partial lineage information.");
        Assert(metrics["ancestry_initial_circular_authority"] is >= 0.0 and <= 1.0, "Protocol 09 approximate circular authority must remain bounded.");
        Assert(metrics["recursive_initial_circular_authority"] is >= 0.0 and <= 1.0, "Protocol 09 recursive circular authority must remain bounded.");
    }

    private static void TestAuthorityAncestryNullHarmProbe()
    {
        var metrics = AuthorityAncestryProbes.EvaluateGroundedNoiseVersusDelay(0.15, 6.0, 919UL);
        Assert(double.IsFinite(metrics["boundary_margin"]), "Protocol 09 grounded null-harm probe must produce a finite boundary margin.");
        Assert(metrics["ancestry_final_grounded_standing"] is >= 0.0 and <= 1.0, "Protocol 09 grounded standing must remain bounded under noisy delayed consequence.");
        Assert(metrics["ancestry_early_grounded_rmse"] >= 0.0 && metrics["direct_early_grounded_rmse"] >= 0.0, "Protocol 09 grounded error metrics must remain nonnegative.");
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
