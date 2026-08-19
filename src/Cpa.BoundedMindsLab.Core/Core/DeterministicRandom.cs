namespace Cpa.BoundedMindsLab.Core;

public sealed class DeterministicRandom
{
    private ulong _state;

    public DeterministicRandom(ulong seed)
    {
        _state = seed == 0 ? 0x9E3779B97F4A7C15UL : seed;
    }

    public ulong NextUInt64()
    {
        var x = _state;
        x ^= x >> 12;
        x ^= x << 25;
        x ^= x >> 27;
        _state = x;
        return x * 2685821657736338717UL;
    }

    public int NextInt(int exclusiveMaximum)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveMaximum);
        return (int)(NextUInt64() % (uint)exclusiveMaximum);
    }

    public double NextUnit() => (NextUInt64() >> 11) * (1.0 / (1UL << 53));

    public void Shuffle<T>(IList<T> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        for (var index = values.Count - 1; index > 0; index--)
        {
            var swap = NextInt(index + 1);
            (values[index], values[swap]) = (values[swap], values[index]);
        }
    }
}
