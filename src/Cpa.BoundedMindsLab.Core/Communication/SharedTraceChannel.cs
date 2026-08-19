using Cpa.BoundedMindsLab.Domain;

namespace Cpa.BoundedMindsLab.Communication;

public sealed class SharedTraceChannel
{
    private readonly List<PublicTracePacket> _packets = [];

    public SharedTraceChannel(double costPerPacket = 0.35)
    {
        if (costPerPacket < 0.0 || !double.IsFinite(costPerPacket))
        {
            throw new ArgumentOutOfRangeException(nameof(costPerPacket));
        }

        CostPerPacket = costPerPacket;
    }

    public double CostPerPacket { get; }

    public int PacketCount => _packets.Count;

    public double CommunicationWork => PacketCount * CostPerPacket;

    public void Publish(PublicTracePacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        _packets.Add(packet);
    }

    public IReadOnlyList<PublicTracePacket> ReadPublicPackets() => _packets.ToArray();
}
