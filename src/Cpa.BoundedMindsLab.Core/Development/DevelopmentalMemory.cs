using Cpa.BoundedMindsLab.Core;
using Cpa.BoundedMindsLab.Domain;

namespace Cpa.BoundedMindsLab.Development;

public sealed class DevelopmentalMemory
{
    private readonly Dictionary<int, MemoryTrace> _local = [];
    private readonly Dictionary<int, MemoryTrace> _foreign = [];

    public DevelopmentalMemory(string mindId)
    {
        MindId = Guard.NotBlank(mindId, nameof(mindId));
    }

    public string MindId { get; }

    public double LastPrediction { get; private set; }

    public double LastTarget { get; private set; }

    public double LastAbsoluteError { get; private set; }

    public int LocalTraceCount => _local.Count;

    public int ForeignTraceCount => _foreign.Count;

    public double Predict(int contextCell)
    {
        var numerator = 0.0;
        var denominator = 0.0;
        if (_local.TryGetValue(contextCell, out var local))
        {
            numerator += local.Estimate * local.Standing;
            denominator += local.Standing;
        }

        if (_foreign.TryGetValue(contextCell, out var foreign))
        {
            numerator += foreign.Estimate * foreign.Standing;
            denominator += foreign.Standing;
        }

        LastPrediction = denominator <= 1e-12 ? 0.0 : numerator / denominator;
        return LastPrediction;
    }

    public void ObserveDirect(int contextCell, double target)
    {
        Guard.Finite(target, nameof(target));
        LastTarget = target;
        LastAbsoluteError = Math.Abs(LastPrediction - target);

        if (_foreign.TryGetValue(contextCell, out var foreign))
        {
            var foreignError = Math.Abs(foreign.Estimate - target);
            if (foreignError <= 0.22)
            {
                foreign.Standing += 0.08 * (1.0 - foreign.Standing);
            }
            else if (foreignError >= 0.45)
            {
                foreign.Standing *= 0.58;
            }
            else
            {
                foreign.Standing *= 0.88;
            }

            foreign.Standing = Math.Clamp(foreign.Standing, 0.0, 1.0);
        }

        if (!_local.TryGetValue(contextCell, out var local))
        {
            _local.Add(
                contextCell,
                new MemoryTrace(
                    contextCell,
                    target,
                    standing: 0.32,
                    directEvidenceCount: 1,
                    importedEvidenceCount: 0,
                    sourceMindId: MindId,
                    originId: $"{MindId}:{contextCell}",
                    TraceProvenance.Direct));
            return;
        }

        var learningRate = Math.Clamp(0.55 / Math.Sqrt(local.DirectEvidenceCount + local.ImportedEvidenceCount + 3.0), 0.06, 0.28);
        var localError = Math.Abs(local.Estimate - target);
        local.Estimate += learningRate * (target - local.Estimate);
        var quality = Math.Max(0.0, 1.0 - (localError / 1.5));
        local.Standing += 0.10 * quality * (1.0 - local.Standing);
        local.Standing = Math.Clamp(local.Standing, 0.10, 1.0);
        local.DirectEvidenceCount++;
    }

    public void ImportProvisional(PublicTracePacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        var standing = Math.Min(0.42, packet.SenderStanding * 0.48);
        _foreign[packet.ContextCell] = new MemoryTrace(
            packet.ContextCell,
            packet.Estimate,
            standing,
            directEvidenceCount: 0,
            importedEvidenceCount: packet.SenderEvidenceCount,
            sourceMindId: packet.SourceMindId,
            originId: packet.OriginId,
            TraceProvenance.Foreign);
    }

    public void ImportAsLived(PublicTracePacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        _local[packet.ContextCell] = new MemoryTrace(
            packet.ContextCell,
            packet.Estimate,
            Math.Clamp(packet.SenderStanding, 0.0, 1.0),
            directEvidenceCount: 0,
            importedEvidenceCount: packet.SenderEvidenceCount,
            sourceMindId: packet.SourceMindId,
            originId: packet.OriginId,
            TraceProvenance.Direct);
    }

    public IReadOnlyList<PublicTracePacket> ExportPublicTraces(double minimumStanding = 0.80)
    {
        return _local.Values
            .Where(trace => trace.Standing >= minimumStanding)
            .OrderBy(trace => trace.ContextCell)
            .Select(trace => new PublicTracePacket(
                MindId,
                trace.OriginId,
                trace.ContextCell,
                trace.Estimate,
                trace.Standing,
                trace.DirectEvidenceCount + trace.ImportedEvidenceCount))
            .ToArray();
    }

    public double MeanLocalStanding() => _local.Count == 0 ? 0.0 : _local.Values.Average(trace => trace.Standing);

    public double MeanForeignStanding() => _foreign.Count == 0 ? 0.0 : _foreign.Values.Average(trace => trace.Standing);

    public double MeanForeignStanding(IEnumerable<int> contextCells)
    {
        var values = contextCells
            .Where(_foreign.ContainsKey)
            .Select(cell => _foreign[cell].Standing)
            .ToArray();
        return values.Length == 0 ? 0.0 : values.Average();
    }

    public double StandingFor(int contextCell, TraceProvenance provenance)
    {
        var source = provenance == TraceProvenance.Direct ? _local : _foreign;
        return source.TryGetValue(contextCell, out var trace) ? trace.Standing : 0.0;
    }

    public int DirectEvidenceFor(int contextCell) =>
        _local.TryGetValue(contextCell, out var trace) ? trace.DirectEvidenceCount : 0;

    public MindPublicState PublicMindState() => new(
        MindId,
        _local.Count,
        _foreign.Count,
        MeanLocalStanding(),
        MeanForeignStanding(),
        LastPrediction,
        LastTarget,
        LastAbsoluteError);

    public IReadOnlyList<TracePublicState> PublicTraceStates()
    {
        return _local.Values
            .Concat(_foreign.Values)
            .OrderBy(trace => trace.ContextCell)
            .ThenBy(trace => trace.Provenance)
            .Select(trace => new TracePublicState(
                MindId,
                trace.ContextCell,
                trace.Provenance,
                trace.SourceMindId,
                trace.OriginId,
                trace.Estimate,
                trace.Standing,
                trace.DirectEvidenceCount,
                trace.ImportedEvidenceCount))
            .ToArray();
    }

    private sealed class MemoryTrace
    {
        public MemoryTrace(
            int contextCell,
            double estimate,
            double standing,
            int directEvidenceCount,
            int importedEvidenceCount,
            string sourceMindId,
            string originId,
            TraceProvenance provenance)
        {
            ContextCell = contextCell;
            Estimate = estimate;
            Standing = standing;
            DirectEvidenceCount = directEvidenceCount;
            ImportedEvidenceCount = importedEvidenceCount;
            SourceMindId = sourceMindId;
            OriginId = originId;
            Provenance = provenance;
        }

        public int ContextCell { get; }

        public double Estimate { get; set; }

        public double Standing { get; set; }

        public int DirectEvidenceCount { get; set; }

        public int ImportedEvidenceCount { get; }

        public string SourceMindId { get; }

        public string OriginId { get; }

        public TraceProvenance Provenance { get; }
    }
}
