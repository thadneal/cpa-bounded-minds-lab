namespace Cpa.BoundedMindsLab.Domain;

public enum TraceProvenance
{
    Direct,
    Foreign,
}

public sealed record PublicTracePacket(
    string SourceMindId,
    string OriginId,
    int ContextCell,
    double Estimate,
    double SenderStanding,
    int SenderEvidenceCount);

public sealed record TracePublicState(
    string MindId,
    int ContextCell,
    TraceProvenance Provenance,
    string SourceMindId,
    string OriginId,
    double Estimate,
    double Standing,
    int DirectEvidenceCount,
    int ImportedEvidenceCount);

public sealed record MindPublicState(
    string MindId,
    int LocalTraceCount,
    int ForeignTraceCount,
    double MeanLocalStanding,
    double MeanForeignStanding,
    double LastPrediction,
    double LastTarget,
    double LastAbsoluteError);
