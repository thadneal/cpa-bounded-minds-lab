using System.Windows;
using Cpa.BoundedMindsLab.Desktop.Controls;
using Cpa.BoundedMindsLab.Desktop.Services;

namespace Cpa.BoundedMindsLab.Desktop;

public partial class MetricPlotWindow : Window
{
    public MetricPlotWindow()
    {
        InitializeComponent();
        UpdateSelection(null, null);
    }

    public FastMetricPlot Plot => MetricPlot;

    public void UpdateSelection(string? metric, string? focusSeries)
    {
        MetricText.Text = metric is null ? "Live metric graph" : metric;
        FocusText.Text = focusSeries is null ? "No focus path selected" : $"Focus path: {focusSeries}";
    }

    public void UpdateSeed(ulong? seed, int seedIndex, int seedCount)
    {
        SeedText.Text = seed is { } activeSeed
            ? $"SEED {activeSeed} ({seedIndex}/{seedCount})"
            : "SEED --";
    }

    private void WindowSourceInitialized(object? sender, EventArgs eventArgs) => WindowsDarkMode.Apply(this);

    private void CloseClicked(object sender, RoutedEventArgs eventArgs) => Close();
}
