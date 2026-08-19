namespace Cpa.BoundedMindsLab.Core;

public static class Guard
{
    public static string NotBlank(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value must not be blank.", parameterName);
        }

        return value;
    }

    public static double Finite(double value, string parameterName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }

        return value;
    }
}
