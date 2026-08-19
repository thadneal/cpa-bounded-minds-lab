namespace Cpa.BoundedMindsLab.Core;

public sealed class ErrorAccumulator
{
    private double _sumSquares;

    public int Count { get; private set; }

    public double Rmse => Count == 0 ? 0.0 : Math.Sqrt(_sumSquares / Count);

    public void Add(double error)
    {
        Guard.Finite(error, nameof(error));
        _sumSquares += error * error;
        Count++;
    }
}
