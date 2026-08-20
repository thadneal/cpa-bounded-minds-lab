namespace Cpa.BoundedMindsLab.Desktop.Services;

public static class ApplicationVersion
{
    public static string Current { get; } =
        typeof(ApplicationVersion).Assembly.GetName().Version?.ToString(3) ?? "unknown";
}
