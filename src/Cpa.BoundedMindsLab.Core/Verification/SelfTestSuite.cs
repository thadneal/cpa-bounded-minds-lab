using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Communication;
using Cpa.BoundedMindsLab.Development;
using Cpa.BoundedMindsLab.Domain;
using Cpa.BoundedMindsLab.Environments;
using Cpa.BoundedMindsLab.Experiments;

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
        Run("protocol-01-supports-seed-101", TestProtocolOne, passed);
        Run("protocol-02-supports-seed-101", TestProtocolTwo, passed);
        Run("protocol-03-default-seeds-create-distinct-lived-histories", TestProtocolThreeSeedSemantics, passed);
        Run("protocol-03-supports-seed-101", TestProtocolThree, passed);
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
        foreach (var seed in ExperimentDefaults.ReplicationSeeds)
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
