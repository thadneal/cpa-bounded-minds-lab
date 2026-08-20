using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Cpa.BoundedMindsLab.Desktop.Controls;
using Cpa.BoundedMindsLab.Desktop.Services;
using Cpa.BoundedMindsLab.Desktop.ViewModels;
using Cpa.BoundedMindsLab.Experiments;
using Cpa.BoundedMindsLab.Validation;

namespace Cpa.BoundedMindsLab.Desktop;

public partial class MainWindow : Window
{
    private const string Protocol01Name = "01-local-shared-memory-contamination";
    private const string Protocol02Name = "02-peer-disagreement-preserved-interiors";
    private const string Protocol03Name = "03-developmental-versus-doctrinal-transfer";
    private const string Protocol04Name = "04-bounded-communication-before-language";
    private const string Protocol05Name = "05-emergent-convention-artificial-culture";
    private const string Protocol06Name = "06-incomplete-epistemic-ancestry";
    private const string Protocol07Name = "07-provisional-standing-transfer";
    private const string Protocol08Name = "08-strategic-public-influence";
    private const string Protocol09Name = "09-authority-ancestry-circular-standing";
    private static readonly int[] BoundaryDelays = [0, 2, 10];
    private static readonly char[] SeedSeparators = [',', ';', ' ', '\t', '\r', '\n'];
    private readonly DesktopRunCoordinator _coordinator = new();
    private readonly DispatcherTimer _refreshTimer;
    private long _catalogVersion = -1;
    private long _timelineVersion = -1;
    private long _detailVersion = -1;
    private long _protocolResultsVersion = -1;
    private long _lastPlotStoreVersion = -1;
    private int _lastPlotWidth;
    private long _nextGraphRefreshTimestamp;
    private string? _lastRunOutputDirectory;
    private ulong? _displayedSeed;
    private MetricPlotWindow? _maximizedPlotWindow;
    private bool _updatingSelectors;
    private bool _followActiveGraphSeed = true;
    private bool _closeAfterRun;
    private bool _allowClose;
    private bool _updatingSeedPreset;
    private string? _progressExperiment;

    public MainWindow()
    {
        var catalog = ExperimentCatalog.All;
        Experiments = new ObservableCollection<ExperimentChoice>(
            catalog.Select(experiment => new ExperimentChoice(experiment.Name, experiment.Question, isSelected: true)));
        InitializeComponent();
        Title = $"CPA Bounded Minds Laboratory v{ApplicationVersion.Current}";
        MetricPlot.SeriesVisibilityChanged += PlotSeriesVisibilityChanged;
        DataContext = this;
        _updatingSeedPreset = true;
        SeedPresetComboBox.SelectedIndex = 1;
        SeedTextBox.Text = string.Join(", ", ExperimentDefaults.DevelopmentSeeds);
        _updatingSeedPreset = false;
        OutputTextBox.Text = ResolveDefaultArtifactRoot();
        ResetProtocolProgress();
        UpdateSelectorNavigationButtons();
        _refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(67),
        };
        _refreshTimer.Tick += RefreshVisualization;
        _refreshTimer.Start();
    }

    public ObservableCollection<ExperimentChoice> Experiments { get; }

    private void WindowSourceInitialized(object? sender, EventArgs eventArgs) => WindowsDarkMode.Apply(this);

    private async void RunClicked(object sender, RoutedEventArgs eventArgs)
    {
        if (_coordinator.IsRunning)
        {
            return;
        }

        if (!TryParseSeeds(SeedTextBox.Text, out var seeds, out var seedError))
        {
            MessageBox.Show(this, seedError, "Invalid seeds", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selected = Experiments.Where(experiment => experiment.IsSelected).Select(experiment => experiment.Name).ToArray();
        if (selected.Length == 0)
        {
            MessageBox.Show(this, "Select at least one experiment.", "No experiments selected", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var artifactRoot = OutputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(artifactRoot))
        {
            MessageBox.Show(this, "Choose an artifact directory.", "Output required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var runOutput = CreateRunOutputDirectory(Path.GetFullPath(artifactRoot));
        _lastRunOutputDirectory = runOutput;
        _displayedSeed = null;
        _followActiveGraphSeed = true;
        ResetVisualization();
        ResetProtocolResultsView();
        SetRunningState(true);
        var seedSet = ValidationPlan.ClassifySeedSet(seeds);
        StatusText.Text = $"Running {seeds.Length} seed(s) [{seedSet}] in succession -> {runOutput}";
        try
        {
            var delay = PaceComboBox.SelectedIndex is >= 0 and < 3 ? BoundaryDelays[PaceComboBox.SelectedIndex] : 0;
            var task = _coordinator.RunAsync(selected, seeds, runOutput, delay);
            var completion = await task;
            RefreshVisualization(this, EventArgs.Empty);
            StatusText.Text = completion.Status == DesktopRunStatus.Cancelled
                ? $"Session cancelled after {completion.CompletedSeedCount}/{completion.PlannedSeedCount} completed seed(s)"
                : $"Session complete: {completion.CompletedSeedCount}/{completion.PlannedSeedCount} seed(s)";
        }
        catch (Exception exception)
        {
            StatusText.Text = "Run faulted";
            if (!_closeAfterRun)
            {
                MessageBox.Show(this, exception.Message, "Experiment failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            SetRunningState(false);
            if (_closeAfterRun)
            {
                _allowClose = true;
                Close();
            }
        }
    }

    private void PauseClicked(object sender, RoutedEventArgs eventArgs)
    {
        if (_coordinator.Pause())
        {
            StatusText.Text = "Pause requested; the experiment will stop at the next observation boundary";
            PauseButton.IsEnabled = false;
            StepButton.IsEnabled = true;
            ResumeButton.IsEnabled = true;
        }
    }

    private void StepClicked(object sender, RoutedEventArgs eventArgs)
    {
        StatusText.Text = _coordinator.Step()
            ? "Releasing exactly one experiment observation boundary"
            : "The run has already reached a terminal boundary";
    }

    private void ResumeClicked(object sender, RoutedEventArgs eventArgs)
    {
        if (_coordinator.Resume())
        {
            StatusText.Text = "Running";
            PauseButton.IsEnabled = true;
            StepButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
        }
    }

    private void CancelClicked(object sender, RoutedEventArgs eventArgs)
    {
        if (_coordinator.Cancel())
        {
            StatusText.Text = "Cancellation requested; waiting for a safe observation boundary";
            CancelButton.IsEnabled = false;
        }
    }

    private void SelectAllClicked(object sender, RoutedEventArgs eventArgs)
    {
        foreach (var experiment in Experiments)
        {
            experiment.IsSelected = true;
        }
    }

    private void SelectNoneClicked(object sender, RoutedEventArgs eventArgs)
    {
        foreach (var experiment in Experiments)
        {
            experiment.IsSelected = false;
        }
    }

    private void SeedPresetSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
    {
        if (_updatingSeedPreset || SeedPresetComboBox.SelectedItem is not ComboBoxItem item)
        {
            return;
        }

        var tag = item.Tag as string;
        IReadOnlyList<ulong>? seeds = tag switch
        {
            "holdout" => ExperimentDefaults.HoldoutSeeds,
            "development" => ExperimentDefaults.DevelopmentSeeds,
            _ => null,
        };
        if (seeds is null)
        {
            return;
        }

        _updatingSeedPreset = true;
        SeedTextBox.Text = string.Join(", ", seeds);
        _updatingSeedPreset = false;
    }

    private void SeedTextChanged(object sender, TextChangedEventArgs eventArgs)
    {
        if (_updatingSeedPreset || SeedPresetComboBox is null)
        {
            return;
        }

        if (!TryParseSeeds(SeedTextBox.Text, out var seeds, out _))
        {
            SetSeedPresetIndex(2);
            return;
        }

        var seedSet = ValidationPlan.ClassifySeedSet(seeds);
        SetSeedPresetIndex(seedSet switch
        {
            ValidationPlan.HoldoutSetName => 0,
            ValidationPlan.DevelopmentSetName => 1,
            _ => 2,
        });
    }

    private void SetSeedPresetIndex(int index)
    {
        if (SeedPresetComboBox.SelectedIndex == index)
        {
            return;
        }

        _updatingSeedPreset = true;
        SeedPresetComboBox.SelectedIndex = index;
        _updatingSeedPreset = false;
    }

    private void OpenOutputClicked(object sender, RoutedEventArgs eventArgs)
    {
        var configuredRoot = Path.GetFullPath(OutputTextBox.Text.Trim());
        var path = _lastRunOutputDirectory is not null && Directory.Exists(_lastRunOutputDirectory)
            ? _lastRunOutputDirectory
            : configuredRoot;
        if (!Directory.Exists(path))
        {
            MessageBox.Show(this, "The output directory does not exist yet.", "Output", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    private void PreviousMetricClicked(object sender, RoutedEventArgs eventArgs) => StepSelection(MetricComboBox, -1);

    private void NextMetricClicked(object sender, RoutedEventArgs eventArgs) => StepSelection(MetricComboBox, 1);

    private void PreviousSeriesClicked(object sender, RoutedEventArgs eventArgs) => StepSelection(SeriesComboBox, -1);

    private void NextSeriesClicked(object sender, RoutedEventArgs eventArgs) => StepSelection(SeriesComboBox, 1);

    private void PreviousGraphSeedClicked(object sender, RoutedEventArgs eventArgs) => StepSelection(GraphSeedComboBox, -1);

    private void NextGraphSeedClicked(object sender, RoutedEventArgs eventArgs) => StepSelection(GraphSeedComboBox, 1);

    private void ShowAllPlotLinesClicked(object sender, RoutedEventArgs eventArgs)
    {
        var plot = _maximizedPlotWindow?.Plot ?? MetricPlot;
        plot.ShowAllCurrentSeries();
    }

    private void HideAllPlotLinesClicked(object sender, RoutedEventArgs eventArgs)
    {
        var plot = _maximizedPlotWindow?.Plot ?? MetricPlot;
        plot.HideAllCurrentSeries();
    }

    private void MaximizeGraphClicked(object sender, RoutedEventArgs eventArgs)
    {
        if (_maximizedPlotWindow is not null)
        {
            if (_maximizedPlotWindow.WindowState == WindowState.Minimized)
            {
                _maximizedPlotWindow.WindowState = WindowState.Maximized;
            }

            _maximizedPlotWindow.Activate();
            return;
        }

        var window = new MetricPlotWindow
        {
            Owner = this,
        };
        window.Closed += MaximizedPlotWindowClosed;
        window.Plot.SeriesVisibilityChanged += PlotSeriesVisibilityChanged;
        window.Plot.SetHiddenSeriesVisibility(MetricPlot.GetHiddenSeriesVisibility(), rebuild: false);
        window.UpdateSelection(MetricComboBox.SelectedItem as string, SeriesComboBox.SelectedItem as string);
        _maximizedPlotWindow = window;
        var session = _coordinator.GetSessionStatus();
        UpdateMaximizedGraphSeed(window, session);
        _lastPlotStoreVersion = -1;
        _lastPlotWidth = 0;
        _nextGraphRefreshTimestamp = 0;
        window.Show();
    }

    private void MetricSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
    {
        if (_updatingSelectors)
        {
            return;
        }

        _detailVersion = -1;
        _lastPlotStoreVersion = -1;
        _nextGraphRefreshTimestamp = 0;
        UpdateSelectorNavigationButtons();
        UpdateMaximizedSelection();
    }

    private void SeriesSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
    {
        if (_updatingSelectors)
        {
            return;
        }

        var currentMetric = MetricComboBox.SelectedItem as string;
        SetMetricOptions(GetSelectedTelemetryStore(_coordinator.Telemetry), currentMetric);
        _detailVersion = -1;
        _lastPlotStoreVersion = -1;
        _nextGraphRefreshTimestamp = 0;
        UpdateSelectorNavigationButtons();
        UpdateMaximizedSelection();
    }

    private void GraphSeedSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
    {
        if (_updatingSelectors)
        {
            return;
        }

        _followActiveGraphSeed = false;
        ResetSelectedSeedVisualization();
        UpdateSelectorNavigationButtons();
        UpdateMaximizedSelection();
        UpdateMaximizedGraphSeed(_maximizedPlotWindow, _coordinator.GetSessionStatus());
    }

    private void ProtocolResultSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
    {
        ResultAssertionsGrid.ItemsSource = ProtocolResultsGrid.SelectedItem is DesktopProtocolResult result
            ? result.Assertions
            : null;
    }

    private void RefreshVisualization(object? sender, EventArgs eventArgs)
    {
        var telemetry = _coordinator.Telemetry;
        var status = telemetry.GetStatus();
        var session = _coordinator.GetSessionStatus();
        if (session.ActiveSeed is { } activeSeed)
        {
            _displayedSeed = activeSeed;
        }

        RefreshGraphSeedOptions(telemetry, session);
        var selectedStore = GetSelectedTelemetryStore(telemetry);
        UpdateSessionProgress(session);
        RefreshProtocolResults(_coordinator.GetProtocolResults());
        var activePlot = _maximizedPlotWindow?.Plot ?? MetricPlot;
        PerformanceText.Text =
            $"display pub {status.PublishedFrames:N0} | projected {status.ProjectedFrames:N0} | dropped {status.DroppedFrames:N0} | backlog {status.Backlog:N0} | projector {status.ProjectionMilliseconds:0.0} ms | plot {activePlot.DisplayPointCount:N0} pts / {activePlot.LastBuildMilliseconds:0.0} ms";

        RefreshCatalog(selectedStore);
        var selectedSeries = SeriesComboBox.SelectedItem as string;
        if (!FreezeGraphCheckBox.IsChecked.GetValueOrDefault() && MetricComboBox.SelectedItem is string metric)
        {
            var width = Math.Max(64, (int)Math.Round(activePlot.ActualWidth));
            var now = Stopwatch.GetTimestamp();
            var graphDue = now >= _nextGraphRefreshTimestamp;
            if (graphDue && (selectedStore.Version != _lastPlotStoreVersion || width != _lastPlotWidth))
            {
                var snapshot = selectedStore.GetPlotSnapshot(metric, selectedSeries, width);
                activePlot.SetSnapshot(snapshot);
                _lastPlotStoreVersion = selectedStore.Version;
                _lastPlotWidth = width;
                var refreshMilliseconds = status.Backlog > 4_096 || activePlot.LastBuildMilliseconds >= 16.0
                    ? 250
                    : activePlot.LastBuildMilliseconds >= 8.0
                        ? 150
                        : 100;
                _nextGraphRefreshTimestamp = now + (long)(Stopwatch.Frequency * (refreshMilliseconds / 1000.0));
            }
        }

        var details = selectedStore.GetDetailSnapshot(selectedSeries);
        if (details.Version != _detailVersion)
        {
            MindGrid.ItemsSource = details.Minds;
            TraceGrid.ItemsSource = details.Traces;
            _detailVersion = details.Version;
        }

        if (selectedStore.TimelineVersion != _timelineVersion)
        {
            var timeline = selectedStore.GetTimeline();
            TimelineGrid.ItemsSource = timeline.Items;
            _timelineVersion = timeline.Version;
            UpdateProtocolProgress(timeline);
            if (CenterTabs.SelectedIndex == 1 && timeline.Items.Count > 0)
            {
                TimelineGrid.ScrollIntoView(timeline.Items[^1]);
            }
        }
    }

    private void RefreshCatalog(TelemetryStore store)
    {
        if (store.CatalogVersion == _catalogVersion)
        {
            return;
        }

        var catalog = store.GetCatalog();
        var currentMetric = MetricComboBox.SelectedItem as string;
        var currentSeries = SeriesComboBox.SelectedItem as string;

        _updatingSelectors = true;
        try
        {
            SeriesComboBox.ItemsSource = catalog.Series;

            if (currentSeries is not null && catalog.Series.Contains(currentSeries, StringComparer.Ordinal))
            {
                SeriesComboBox.SelectedItem = currentSeries;
            }
            else
            {
                string? preferred = null;
                for (var index = 0; index < catalog.Series.Count; index++)
                {
                    var candidate = catalog.Series[index];
                    if (candidate.EndsWith("/shared-provisional", StringComparison.Ordinal))
                    {
                        preferred = candidate;
                        break;
                    }
                }

                SeriesComboBox.SelectedItem = preferred ?? (catalog.Series.Count > 0 ? catalog.Series[0] : null);
            }

            SetMetricOptions(store, currentMetric);
        }
        finally
        {
            _updatingSelectors = false;
        }

        _catalogVersion = catalog.Version;
        _detailVersion = -1;
        _lastPlotStoreVersion = -1;
        _nextGraphRefreshTimestamp = 0;
        UpdateSelectorNavigationButtons();
        UpdateMaximizedSelection();
    }

    private void SetMetricOptions(TelemetryStore store, string? preferredMetric)
    {
        var previousUpdatingState = _updatingSelectors;
        _updatingSelectors = true;
        try
        {
            if (SeriesComboBox.SelectedItem is not string series)
            {
                MetricComboBox.ItemsSource = Array.Empty<string>();
                MetricComboBox.SelectedIndex = -1;
                return;
            }

            var relevantMetrics = store.GetMetricsForSeries(series);
            MetricComboBox.ItemsSource = relevantMetrics;

            if (preferredMetric is not null && relevantMetrics.Contains(preferredMetric, StringComparer.Ordinal))
            {
                MetricComboBox.SelectedItem = preferredMetric;
                return;
            }

            if (relevantMetrics.Contains("rolling_rmse", StringComparer.Ordinal))
            {
                MetricComboBox.SelectedItem = "rolling_rmse";
                return;
            }

            MetricComboBox.SelectedItem = relevantMetrics.Length > 0 ? relevantMetrics[0] : null;
        }
        finally
        {
            _updatingSelectors = previousUpdatingState;
        }
    }

    private void RefreshProtocolResults(DesktopProtocolResultsSnapshot snapshot)
    {
        if (snapshot.Version == _protocolResultsVersion)
        {
            return;
        }

        ulong? selectedSeed = null;
        string? selectedExperiment = null;
        if (ProtocolResultsGrid.SelectedItem is DesktopProtocolResult selected)
        {
            selectedSeed = selected.Seed;
            selectedExperiment = selected.Experiment;
        }

        ProtocolResultsGrid.ItemsSource = snapshot.Results;
        ProtocolResultsSummaryText.Text = BuildProtocolResultSummary(snapshot);

        DesktopProtocolResult? nextSelection = null;
        if (selectedSeed is { } seed && selectedExperiment is not null)
        {
            for (var index = 0; index < snapshot.Results.Count; index++)
            {
                var candidate = snapshot.Results[index];
                if (candidate.Seed == seed && string.Equals(candidate.Experiment, selectedExperiment, StringComparison.Ordinal))
                {
                    nextSelection = candidate;
                    break;
                }
            }
        }

        if (nextSelection is null && snapshot.Results.Count > 0)
        {
            nextSelection = snapshot.Results[^1];
        }

        ProtocolResultsGrid.SelectedItem = nextSelection;
        ResultAssertionsGrid.ItemsSource = nextSelection?.Assertions;
        _protocolResultsVersion = snapshot.Version;
    }

    private static string BuildProtocolResultSummary(DesktopProtocolResultsSnapshot snapshot)
    {
        if (snapshot.Summaries.Count == 0)
        {
            return "No protocol has reached a judged result yet.";
        }

        var parts = new string[snapshot.Summaries.Count];
        for (var index = 0; index < snapshot.Summaries.Count; index++)
        {
            var summary = snapshot.Summaries[index];
            parts[index] = $"{summary.Experiment}: Supported {summary.Supported}, Mixed {summary.Mixed}, Refuted {summary.Refuted}, Inconclusive {summary.Inconclusive}";
        }

        return string.Join("   |   ", parts);
    }

    private void UpdateProtocolProgress(TelemetryTimelineSnapshot timeline)
    {
        var experiment = GetCurrentExperimentName(timeline);
        if (!string.Equals(_progressExperiment, experiment, StringComparison.Ordinal))
        {
            _progressExperiment = experiment;
            if (string.Equals(experiment, Protocol09Name, StringComparison.Ordinal))
            {
                SetProtocol09ProgressLabels();
            }
            else if (string.Equals(experiment, Protocol08Name, StringComparison.Ordinal))
            {
                SetProtocol08ProgressLabels();
            }
            else if (string.Equals(experiment, Protocol07Name, StringComparison.Ordinal))
            {
                SetProtocol07ProgressLabels();
            }
            else if (string.Equals(experiment, Protocol06Name, StringComparison.Ordinal))
            {
                SetProtocol06ProgressLabels();
            }
            else if (string.Equals(experiment, Protocol05Name, StringComparison.Ordinal))
            {
                SetProtocol05ProgressLabels();
            }
            else if (string.Equals(experiment, Protocol04Name, StringComparison.Ordinal))
            {
                SetProtocol04ProgressLabels();
            }
            else if (string.Equals(experiment, Protocol03Name, StringComparison.Ordinal))
            {
                SetProtocol03ProgressLabels();
            }
            else if (string.Equals(experiment, Protocol02Name, StringComparison.Ordinal))
            {
                SetProtocol02ProgressLabels();
            }
            else
            {
                SetProtocol01ProgressLabels();
            }

            SetAllProgressPending();
        }

        if (string.Equals(experiment, Protocol09Name, StringComparison.Ordinal))
        {
            UpdateProtocol09Progress(timeline);
            return;
        }

        if (string.Equals(experiment, Protocol08Name, StringComparison.Ordinal))
        {
            UpdateProtocol08Progress(timeline);
            return;
        }

        if (string.Equals(experiment, Protocol07Name, StringComparison.Ordinal))
        {
            UpdateProtocol07Progress(timeline);
            return;
        }

        if (string.Equals(experiment, Protocol06Name, StringComparison.Ordinal))
        {
            UpdateProtocol06Progress(timeline);
            return;
        }

        if (string.Equals(experiment, Protocol05Name, StringComparison.Ordinal))
        {
            UpdateProtocol05Progress(timeline);
            return;
        }

        if (string.Equals(experiment, Protocol04Name, StringComparison.Ordinal))
        {
            UpdateProtocol04Progress(timeline);
            return;
        }

        if (string.Equals(experiment, Protocol03Name, StringComparison.Ordinal))
        {
            UpdateProtocol03Progress(timeline);
            return;
        }

        if (string.Equals(experiment, Protocol02Name, StringComparison.Ordinal))
        {
            UpdateProtocol02Progress(timeline);
            return;
        }

        if (string.Equals(experiment, Protocol01Name, StringComparison.Ordinal))
        {
            UpdateProtocol01Progress(timeline);
            return;
        }

        SetAllProgressPending();
    }

    private void UpdateProtocol01Progress(TelemetryTimelineSnapshot timeline)
    {
        var experimentStarted = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.ExperimentStarted);
        var sourceStarted = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.PhaseChanged, "source", "source-development");
        var publicTransfer = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.DevelopmentalEvent, "source", "public-transfer");
        var localStarted = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.PhaseChanged, "local-only", "receiver-development");
        var localComplete = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.DevelopmentalEvent, "local-only", "path-complete");
        var provisionalStarted = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.PhaseChanged, "shared-provisional", "receiver-development");
        var provisionalComplete = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.DevelopmentalEvent, "shared-provisional", "path-complete");
        var livedStarted = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.PhaseChanged, "shared-lived-equivalent", "receiver-development");
        var livedComplete = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.DevelopmentalEvent, "shared-lived-equivalent", "path-complete");
        var experimentComplete = HasTimelineEvent(timeline, Protocol01Name, ExperimentFrameKind.ExperimentCompleted);

        SetProgressLine(SourceDirectProgressText,
            publicTransfer ? ProgressMark.Complete : sourceStarted || experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText,
            localStarted ? ProgressMark.Complete : publicTransfer ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourceStepText,
            localStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(SourceProgressCard,
            localStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(ReceiverLocalProgressText,
            localComplete || provisionalStarted ? ProgressMark.Complete : localStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText,
            provisionalComplete || livedStarted ? ProgressMark.Complete : provisionalStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText,
            livedComplete ? ProgressMark.Complete : livedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverStepText,
            livedComplete || experimentComplete ? ProgressMark.Complete : localStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard,
            livedComplete || experimentComplete ? ProgressMark.Complete : localStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(EvaluationAssertionsProgressText,
            experimentComplete ? ProgressMark.Complete : livedComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText,
            experimentComplete ? ProgressMark.Complete : ProgressMark.Pending);
        SetProgressLine(EvaluationStepText,
            experimentComplete ? ProgressMark.Complete : livedComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard,
            experimentComplete ? ProgressMark.Complete : livedComplete ? ProgressMark.Current : ProgressMark.Pending);
    }

    private void UpdateProtocol02Progress(TelemetryTimelineSnapshot timeline)
    {
        var experimentStarted = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.ExperimentStarted);
        var peerAStarted = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.PhaseChanged, "peer-a", "peer-a-private-development");
        var peerBStarted = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.PhaseChanged, "peer-b", "peer-b-private-development");
        var privateComplete = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.DevelopmentalEvent, "peers", "private-histories-complete");
        var preservedStarted = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.PhaseChanged, "preserved-interiors", "shared-consequence");
        var preservedComplete = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.DevelopmentalEvent, "preserved-interiors", "path-complete");
        var syncStarted = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.PhaseChanged, "synchronized-control", "synchronization-control");
        var syncSharedStarted = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.PhaseChanged, "synchronized-control", "shared-consequence");
        var syncComplete = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.DevelopmentalEvent, "synchronized-control", "path-complete");
        var experimentComplete = HasTimelineEvent(timeline, Protocol02Name, ExperimentFrameKind.ExperimentCompleted);

        SetProgressLine(SourceDirectProgressText,
            peerBStarted || privateComplete ? ProgressMark.Complete : peerAStarted || experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText,
            privateComplete || preservedStarted ? ProgressMark.Complete : peerBStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourceStepText,
            preservedStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(SourceProgressCard,
            preservedStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(ReceiverLocalProgressText,
            preservedComplete || syncStarted ? ProgressMark.Complete : preservedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText,
            syncSharedStarted || syncComplete ? ProgressMark.Complete : syncStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText,
            syncComplete ? ProgressMark.Complete : syncSharedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverStepText,
            syncComplete || experimentComplete ? ProgressMark.Complete : preservedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard,
            syncComplete || experimentComplete ? ProgressMark.Complete : preservedStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(EvaluationAssertionsProgressText,
            experimentComplete ? ProgressMark.Complete : syncComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText,
            experimentComplete ? ProgressMark.Complete : ProgressMark.Pending);
        SetProgressLine(EvaluationStepText,
            experimentComplete ? ProgressMark.Complete : syncComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard,
            experimentComplete ? ProgressMark.Complete : syncComplete ? ProgressMark.Current : ProgressMark.Pending);
    }


    private void UpdateProtocol09Progress(TelemetryTimelineSnapshot timeline)
    {
        var experimentStarted = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.ExperimentStarted);
        var scenarioGenerated = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.DevelopmentalEvent, "scenario", "authority-world-generated");
        var ancestryStarted = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.PhaseChanged, "authority-ancestry", "authority-development");
        var ancestryReceiverStarted = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.PhaseChanged, "authority-ancestry", "receiver-consequence");
        var ancestryComplete = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.DevelopmentalEvent, "authority-ancestry", "path-complete");
        var recursiveStarted = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.PhaseChanged, "recursive-endorsement", "authority-development");
        var recursiveComplete = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.DevelopmentalEvent, "recursive-endorsement", "path-complete");
        var directStarted = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.PhaseChanged, "direct-only", "receiver-consequence");
        var directComplete = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.DevelopmentalEvent, "direct-only", "path-complete");
        var experimentComplete = HasTimelineEvent(timeline, Protocol09Name, ExperimentFrameKind.ExperimentCompleted);

        SetProgressLine(SourceDirectProgressText,
            scenarioGenerated ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText,
            ancestryReceiverStarted || ancestryComplete ? ProgressMark.Complete : ancestryStarted ? ProgressMark.Current : scenarioGenerated ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourceStepText,
            ancestryReceiverStarted || ancestryComplete || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(SourceProgressCard,
            ancestryReceiverStarted || ancestryComplete || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(ReceiverLocalProgressText,
            ancestryComplete || recursiveStarted ? ProgressMark.Complete : ancestryReceiverStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText,
            recursiveComplete || directStarted ? ProgressMark.Complete : recursiveStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText,
            directComplete ? ProgressMark.Complete : directStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverStepText,
            directComplete || experimentComplete ? ProgressMark.Complete : ancestryReceiverStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard,
            directComplete || experimentComplete ? ProgressMark.Complete : ancestryReceiverStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(EvaluationAssertionsProgressText,
            experimentComplete ? ProgressMark.Complete : directComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText,
            experimentComplete ? ProgressMark.Complete : ProgressMark.Pending);
        SetProgressLine(EvaluationStepText,
            experimentComplete ? ProgressMark.Complete : directComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard,
            experimentComplete ? ProgressMark.Complete : directComplete ? ProgressMark.Current : ProgressMark.Pending);
    }

    private void UpdateProtocol08Progress(TelemetryTimelineSnapshot timeline)
    {
        var experimentStarted = HasTimelineEvent(timeline, Protocol08Name, ExperimentFrameKind.ExperimentStarted);
        var scenarioGenerated = HasTimelineEvent(timeline, Protocol08Name, ExperimentFrameKind.DevelopmentalEvent, "scenario", "strategic-social-world-generated");
        var accountableStarted = HasTimelineEvent(timeline, Protocol08Name, ExperimentFrameKind.PhaseChanged, "accountable-consequence", "strategic-interaction");
        var accountableComplete = HasTimelineEvent(timeline, Protocol08Name, ExperimentFrameKind.DevelopmentalEvent, "accountable-consequence", "path-complete");
        var naiveStarted = HasTimelineEvent(timeline, Protocol08Name, ExperimentFrameKind.PhaseChanged, "self-report-naive", "strategic-interaction");
        var naiveComplete = HasTimelineEvent(timeline, Protocol08Name, ExperimentFrameKind.DevelopmentalEvent, "self-report-naive", "path-complete");
        var localStarted = HasTimelineEvent(timeline, Protocol08Name, ExperimentFrameKind.PhaseChanged, "local-only", "strategic-interaction");
        var localComplete = HasTimelineEvent(timeline, Protocol08Name, ExperimentFrameKind.DevelopmentalEvent, "local-only", "path-complete");
        var experimentComplete = HasTimelineEvent(timeline, Protocol08Name, ExperimentFrameKind.ExperimentCompleted);

        SetProgressLine(SourceDirectProgressText,
            scenarioGenerated ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText,
            accountableStarted ? ProgressMark.Complete : scenarioGenerated ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourceStepText,
            accountableStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(SourceProgressCard,
            accountableStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(ReceiverLocalProgressText,
            accountableComplete || naiveStarted ? ProgressMark.Complete : accountableStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText,
            naiveComplete || localStarted ? ProgressMark.Complete : naiveStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText,
            localComplete ? ProgressMark.Complete : localStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverStepText,
            localComplete || experimentComplete ? ProgressMark.Complete : accountableStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard,
            localComplete || experimentComplete ? ProgressMark.Complete : accountableStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(EvaluationAssertionsProgressText,
            experimentComplete ? ProgressMark.Complete : localComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText,
            experimentComplete ? ProgressMark.Complete : ProgressMark.Pending);
        SetProgressLine(EvaluationStepText,
            experimentComplete ? ProgressMark.Complete : localComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard,
            experimentComplete ? ProgressMark.Complete : localComplete ? ProgressMark.Current : ProgressMark.Pending);
    }

    private void UpdateProtocol07Progress(TelemetryTimelineSnapshot timeline)
    {
        var experimentStarted = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.ExperimentStarted);
        var scenarioGenerated = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.DevelopmentalEvent, "scenario", "standing-world-generated");
        var recommendationsPublished = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.DevelopmentalEvent, "recommender-a", "recommendations-published");
        var provisionalStarted = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.PhaseChanged, "provisional-standing", "receiver-social-learning");
        var provisionalComplete = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.DevelopmentalEvent, "provisional-standing", "path-complete");
        var noTransferStarted = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.PhaseChanged, "no-standing-transfer", "receiver-social-learning");
        var noTransferComplete = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.DevelopmentalEvent, "no-standing-transfer", "path-complete");
        var inheritedStarted = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.PhaseChanged, "inherited-authority", "receiver-social-learning");
        var inheritedComplete = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.DevelopmentalEvent, "inherited-authority", "path-complete");
        var experimentComplete = HasTimelineEvent(timeline, Protocol07Name, ExperimentFrameKind.ExperimentCompleted);

        SetProgressLine(SourceDirectProgressText,
            scenarioGenerated || recommendationsPublished ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText,
            provisionalStarted ? ProgressMark.Complete : recommendationsPublished ? ProgressMark.Complete : scenarioGenerated ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourceStepText,
            provisionalStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(SourceProgressCard,
            provisionalStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(ReceiverLocalProgressText,
            provisionalComplete || noTransferStarted ? ProgressMark.Complete : provisionalStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText,
            noTransferComplete || inheritedStarted ? ProgressMark.Complete : noTransferStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText,
            inheritedComplete ? ProgressMark.Complete : inheritedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverStepText,
            inheritedComplete || experimentComplete ? ProgressMark.Complete : provisionalStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard,
            inheritedComplete || experimentComplete ? ProgressMark.Complete : provisionalStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(EvaluationAssertionsProgressText,
            experimentComplete ? ProgressMark.Complete : inheritedComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText,
            experimentComplete ? ProgressMark.Complete : ProgressMark.Pending);
        SetProgressLine(EvaluationStepText,
            experimentComplete ? ProgressMark.Complete : inheritedComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard,
            experimentComplete ? ProgressMark.Complete : inheritedComplete ? ProgressMark.Current : ProgressMark.Pending);
    }

    private void UpdateProtocol06Progress(TelemetryTimelineSnapshot timeline)
    {
        var experimentStarted = HasTimelineEvent(timeline, Protocol06Name, ExperimentFrameKind.ExperimentStarted);
        var scenarioGenerated = HasTimelineEvent(timeline, Protocol06Name, ExperimentFrameKind.DevelopmentalEvent, "scenario", "ancestry-world-generated");
        var inferredStarted = HasTimelineEvent(timeline, Protocol06Name, ExperimentFrameKind.PhaseChanged, "ancestry-inferred", "infer-ancestry");
        var inferredComplete = HasTimelineEvent(timeline, Protocol06Name, ExperimentFrameKind.DevelopmentalEvent, "ancestry-inferred", "path-complete");
        var naiveStarted = HasTimelineEvent(timeline, Protocol06Name, ExperimentFrameKind.PhaseChanged, "naive-agreement", "evaluate-reports");
        var naiveComplete = HasTimelineEvent(timeline, Protocol06Name, ExperimentFrameKind.DevelopmentalEvent, "naive-agreement", "path-complete");
        var oracleStarted = HasTimelineEvent(timeline, Protocol06Name, ExperimentFrameKind.PhaseChanged, "oracle-ancestry", "evaluate-reports");
        var oracleComplete = HasTimelineEvent(timeline, Protocol06Name, ExperimentFrameKind.DevelopmentalEvent, "oracle-ancestry", "path-complete");
        var experimentComplete = HasTimelineEvent(timeline, Protocol06Name, ExperimentFrameKind.ExperimentCompleted);

        SetProgressLine(SourceDirectProgressText,
            scenarioGenerated || inferredStarted ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText,
            inferredStarted ? ProgressMark.Complete : scenarioGenerated ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourceStepText,
            inferredStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(SourceProgressCard,
            inferredStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(ReceiverLocalProgressText,
            inferredComplete || naiveStarted ? ProgressMark.Complete : inferredStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText,
            naiveComplete || oracleStarted ? ProgressMark.Complete : naiveStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText,
            oracleComplete ? ProgressMark.Complete : oracleStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverStepText,
            oracleComplete || experimentComplete ? ProgressMark.Complete : inferredStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard,
            oracleComplete || experimentComplete ? ProgressMark.Complete : inferredStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(EvaluationAssertionsProgressText,
            experimentComplete ? ProgressMark.Complete : oracleComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText,
            experimentComplete ? ProgressMark.Complete : ProgressMark.Pending);
        SetProgressLine(EvaluationStepText,
            experimentComplete ? ProgressMark.Complete : oracleComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard,
            experimentComplete ? ProgressMark.Complete : oracleComplete ? ProgressMark.Current : ProgressMark.Pending);
    }

    private void UpdateProtocol05Progress(TelemetryTimelineSnapshot timeline)
    {
        var experimentStarted = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.ExperimentStarted);
        var scenarioGenerated = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.DevelopmentalEvent, "scenario", "scenario-generated");
        var earnedStarted = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.PhaseChanged, "earned-convention", "convention-formation");
        var earnedFormed = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.DevelopmentalEvent, "earned-convention", "convention-formed");
        var earnedShifted = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.PhaseChanged, "earned-convention", "regime-shift");
        var earnedComplete = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.DevelopmentalEvent, "earned-convention", "path-complete");
        var freshStarted = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.PhaseChanged, "fresh-negotiation", "convention-formation");
        var freshComplete = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.DevelopmentalEvent, "fresh-negotiation", "path-complete");
        var frozenStarted = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.PhaseChanged, "frozen-convention", "convention-formation");
        var frozenComplete = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.DevelopmentalEvent, "frozen-convention", "path-complete");
        var experimentComplete = HasTimelineEvent(timeline, Protocol05Name, ExperimentFrameKind.ExperimentCompleted);

        SetProgressLine(SourceDirectProgressText,
            scenarioGenerated || earnedStarted ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText,
            earnedFormed || earnedShifted ? ProgressMark.Complete : earnedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourceStepText,
            earnedShifted || earnedComplete || freshStarted || frozenStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(SourceProgressCard,
            earnedShifted || earnedComplete || freshStarted || frozenStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(ReceiverLocalProgressText,
            earnedComplete || freshStarted ? ProgressMark.Complete : earnedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText,
            freshComplete || frozenStarted ? ProgressMark.Complete : freshStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText,
            frozenComplete ? ProgressMark.Complete : frozenStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverStepText,
            frozenComplete || experimentComplete ? ProgressMark.Complete : earnedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard,
            frozenComplete || experimentComplete ? ProgressMark.Complete : earnedStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(EvaluationAssertionsProgressText,
            experimentComplete ? ProgressMark.Complete : frozenComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText,
            experimentComplete ? ProgressMark.Complete : ProgressMark.Pending);
        SetProgressLine(EvaluationStepText,
            experimentComplete ? ProgressMark.Complete : frozenComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard,
            experimentComplete ? ProgressMark.Complete : frozenComplete ? ProgressMark.Current : ProgressMark.Pending);
    }

    private void UpdateProtocol04Progress(TelemetryTimelineSnapshot timeline)
    {
        var experimentStarted = HasTimelineEvent(timeline, Protocol04Name, ExperimentFrameKind.ExperimentStarted);
        var scenarioGenerated = HasTimelineEvent(timeline, Protocol04Name, ExperimentFrameKind.DevelopmentalEvent, "scenario", "scenario-generated");
        var peerDevelopmentStarted = HasTimelineEvent(timeline, Protocol04Name, ExperimentFrameKind.PhaseChanged, phase: "peer-private-development");
        var privateHistoriesComplete = HasTimelineEvent(timeline, Protocol04Name, ExperimentFrameKind.DevelopmentalEvent, "peers", "private-histories-complete");
        var typedStarted = HasTimelineEvent(timeline, Protocol04Name, ExperimentFrameKind.PhaseChanged, "typed-signals", "typed-communication");
        var typedComplete = HasTimelineEvent(timeline, Protocol04Name, ExperimentFrameKind.DevelopmentalEvent, "typed-signals", "path-complete");
        var smoothedStarted = HasTimelineEvent(timeline, Protocol04Name, ExperimentFrameKind.PhaseChanged, "early-semantic-smoothing", "early-semantic-smoothing");
        var smoothedComplete = HasTimelineEvent(timeline, Protocol04Name, ExperimentFrameKind.DevelopmentalEvent, "early-semantic-smoothing", "path-complete");
        var experimentComplete = HasTimelineEvent(timeline, Protocol04Name, ExperimentFrameKind.ExperimentCompleted);

        SetProgressLine(SourceDirectProgressText,
            peerDevelopmentStarted || scenarioGenerated ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText,
            privateHistoriesComplete || typedStarted ? ProgressMark.Complete : peerDevelopmentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourceStepText,
            typedStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(SourceProgressCard,
            typedStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(ReceiverLocalProgressText,
            typedComplete || smoothedStarted ? ProgressMark.Complete : typedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText,
            smoothedComplete ? ProgressMark.Complete : smoothedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText,
            smoothedComplete ? ProgressMark.Complete : typedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverStepText,
            smoothedComplete || experimentComplete ? ProgressMark.Complete : typedStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard,
            smoothedComplete || experimentComplete ? ProgressMark.Complete : typedStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(EvaluationAssertionsProgressText,
            experimentComplete ? ProgressMark.Complete : smoothedComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText,
            experimentComplete ? ProgressMark.Complete : ProgressMark.Pending);
        SetProgressLine(EvaluationStepText,
            experimentComplete ? ProgressMark.Complete : smoothedComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard,
            experimentComplete ? ProgressMark.Complete : smoothedComplete ? ProgressMark.Current : ProgressMark.Pending);
    }

    private void UpdateProtocol03Progress(TelemetryTimelineSnapshot timeline)
    {
        var experimentStarted = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.ExperimentStarted);
        var scenarioGenerated = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.DevelopmentalEvent, "scenario", "scenario-generated");
        var sourceStarted = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.PhaseChanged, "source", "source-development");
        var transferPrepared = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.DevelopmentalEvent, "source", "transfer-prepared");
        var localStarted = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.PhaseChanged, "local-only", "receiver-development");
        var localComplete = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.DevelopmentalEvent, "local-only", "path-complete");
        var developmentalStarted = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.PhaseChanged, "developmental-transfer", "receiver-development");
        var developmentalComplete = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.DevelopmentalEvent, "developmental-transfer", "path-complete");
        var doctrinalStarted = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.PhaseChanged, "doctrinal-transfer", "receiver-development");
        var doctrinalComplete = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.DevelopmentalEvent, "doctrinal-transfer", "path-complete");
        var experimentComplete = HasTimelineEvent(timeline, Protocol03Name, ExperimentFrameKind.ExperimentCompleted);

        SetProgressLine(SourceDirectProgressText,
            sourceStarted || scenarioGenerated ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText,
            transferPrepared || localStarted ? ProgressMark.Complete : sourceStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(SourceStepText,
            localStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(SourceProgressCard,
            localStarted || experimentComplete ? ProgressMark.Complete : experimentStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(ReceiverLocalProgressText,
            localComplete || developmentalStarted ? ProgressMark.Complete : localStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText,
            developmentalComplete || doctrinalStarted ? ProgressMark.Complete : developmentalStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText,
            doctrinalComplete ? ProgressMark.Complete : doctrinalStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(ReceiverStepText,
            doctrinalComplete || experimentComplete ? ProgressMark.Complete : localStarted ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard,
            doctrinalComplete || experimentComplete ? ProgressMark.Complete : localStarted ? ProgressMark.Current : ProgressMark.Pending);

        SetProgressLine(EvaluationAssertionsProgressText,
            experimentComplete ? ProgressMark.Complete : doctrinalComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText,
            experimentComplete ? ProgressMark.Complete : ProgressMark.Pending);
        SetProgressLine(EvaluationStepText,
            experimentComplete ? ProgressMark.Complete : doctrinalComplete ? ProgressMark.Current : ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard,
            experimentComplete ? ProgressMark.Complete : doctrinalComplete ? ProgressMark.Current : ProgressMark.Pending);
    }

    private void SetProtocol01ProgressLabels()
    {
        SetProgressLabel(SourceStepText, "1. Source develops");
        SetProgressLabel(SourceDirectProgressText, "Direct local consequence");
        SetProgressLabel(SourcePublishProgressText, "Publish compact traces");
        SetProgressLabel(ReceiverStepText, "2. Receivers develop");
        SetProgressLabel(ReceiverLocalProgressText, "Local-only baseline");
        SetProgressLabel(ReceiverProvisionalProgressText, "Provisional transfer");
        SetProgressLabel(ReceiverLivedProgressText, "Lived-equivalent control");
        SetProgressLabel(EvaluationStepText, "3. Evaluate");
        SetProgressLabel(EvaluationAssertionsProgressText, "Six falsification checks");
        SetProgressLabel(EvaluationVerdictProgressText, "Protocol verdict");
    }

    private void SetProtocol02ProgressLabels()
    {
        SetProgressLabel(SourceStepText, "1. Peers develop");
        SetProgressLabel(SourceDirectProgressText, "Mind A private history");
        SetProgressLabel(SourcePublishProgressText, "Mind B private history");
        SetProgressLabel(ReceiverStepText, "2. Compare conditions");
        SetProgressLabel(ReceiverLocalProgressText, "Preserved interiors");
        SetProgressLabel(ReceiverProvisionalProgressText, "Collapse to synchronized state");
        SetProgressLabel(ReceiverLivedProgressText, "Synchronized shared consequence");
        SetProgressLabel(EvaluationStepText, "3. Evaluate");
        SetProgressLabel(EvaluationAssertionsProgressText, "Six falsification checks");
        SetProgressLabel(EvaluationVerdictProgressText, "Protocol verdict");
    }

    private void SetProtocol03ProgressLabels()
    {
        SetProgressLabel(SourceStepText, "1. Build lived source history");
        SetProgressLabel(SourceDirectProgressText, "Seed-specific developmental circumstance");
        SetProgressLabel(SourcePublishProgressText, "Source develops and packages transfer");
        SetProgressLabel(ReceiverStepText, "2. Compare transfer forms");
        SetProgressLabel(ReceiverLocalProgressText, "Local-only baseline");
        SetProgressLabel(ReceiverProvisionalProgressText, "Developmental consequence-history transfer");
        SetProgressLabel(ReceiverLivedProgressText, "Doctrinal final-rule transfer");
        SetProgressLabel(EvaluationStepText, "3. Evaluate");
        SetProgressLabel(EvaluationAssertionsProgressText, "Seven falsification checks");
        SetProgressLabel(EvaluationVerdictProgressText, "Protocol verdict");
    }


    private void SetProtocol09ProgressLabels()
    {
        SetProgressLabel(SourceStepText, "1. Let authority circulate");
        SetProgressLabel(SourceDirectProgressText, "Seed-specific independent roots + circular traps");
        SetProgressLabel(SourcePublishProgressText, "Peers exchange locally reasonable endorsements");
        SetProgressLabel(ReceiverStepText, "2. Compare authority boundaries");
        SetProgressLabel(ReceiverLocalProgressText, "Authority ancestry + direct consequence");
        SetProgressLabel(ReceiverProvisionalProgressText, "Recursive-endorsement control");
        SetProgressLabel(ReceiverLivedProgressText, "Direct-only baseline");
        SetProgressLabel(EvaluationStepText, "3. Judge permission ancestry");
        SetProgressLabel(EvaluationAssertionsProgressText, "Ten falsification checks");
        SetProgressLabel(EvaluationVerdictProgressText, "Protocol verdict");
    }

    private void SetProtocol08ProgressLabels()
    {
        SetProgressLabel(SourceStepText, "1. Let a peer learn public leverage");
        SetProgressLabel(SourceDirectProgressText, "Seed-specific aligned, divergent + betrayal contexts");
        SetProgressLabel(SourcePublishProgressText, "B adapts self-reported confidence from C's public response");
        SetProgressLabel(ReceiverStepText, "2. Compare receiver boundaries");
        SetProgressLabel(ReceiverLocalProgressText, "Consequence-grounded standing + calibration");
        SetProgressLabel(ReceiverProvisionalProgressText, "Self-report-naive control");
        SetProgressLabel(ReceiverLivedProgressText, "Local-only baseline");
        SetProgressLabel(EvaluationStepText, "3. Judge strategic capture and useful influence");
        SetProgressLabel(EvaluationAssertionsProgressText, "Ten falsification checks");
        SetProgressLabel(EvaluationVerdictProgressText, "Protocol verdict");
    }

    private void SetProtocol07ProgressLabels()
    {
        SetProgressLabel(SourceStepText, "1. Receive a social recommendation");
        SetProgressLabel(SourceDirectProgressText, "Seed-specific transferable + nontransferable relationships");
        SetProgressLabel(SourcePublishProgressText, "A publishes bounded standing for B");
        SetProgressLabel(ReceiverStepText, "2. Compare standing transfer rules");
        SetProgressLabel(ReceiverLocalProgressText, "Provisional standing transfer");
        SetProgressLabel(ReceiverProvisionalProgressText, "No standing transfer baseline");
        SetProgressLabel(ReceiverLivedProgressText, "Inherited-authority control");
        SetProgressLabel(EvaluationStepText, "3. Judge social authority transfer");
        SetProgressLabel(EvaluationAssertionsProgressText, "Nine falsification checks");
        SetProgressLabel(EvaluationVerdictProgressText, "Protocol verdict");
    }

    private void SetProtocol06ProgressLabels()
    {
        SetProgressLabel(SourceStepText, "1. Generate incomplete ancestry");
        SetProgressLabel(SourceDirectProgressText, "Seed-specific echo and independent histories");
        SetProgressLabel(SourcePublishProgressText, "Publish partial origin hints + developmental signatures");
        SetProgressLabel(ReceiverStepText, "2. Compare corroboration rules");
        SetProgressLabel(ReceiverLocalProgressText, "Infer ancestry from incomplete public cues");
        SetProgressLabel(ReceiverProvisionalProgressText, "Naive agreement-count control");
        SetProgressLabel(ReceiverLivedProgressText, "Perfect-ancestry oracle calibration");
        SetProgressLabel(EvaluationStepText, "3. Judge ancestry discrimination");
        SetProgressLabel(EvaluationAssertionsProgressText, "Eight falsification checks");
        SetProgressLabel(EvaluationVerdictProgressText, "Protocol verdict");
    }

    private void SetProtocol05ProgressLabels()
    {
        SetProgressLabel(SourceStepText, "1. Let a culture form");
        SetProgressLabel(SourceDirectProgressText, "Seed-specific plural coordination world");
        SetProgressLabel(SourcePublishProgressText, "Repeated success earns local convention standing");
        SetProgressLabel(ReceiverStepText, "2. Compare coordination modes");
        SetProgressLabel(ReceiverLocalProgressText, "Earned distributed convention");
        SetProgressLabel(ReceiverProvisionalProgressText, "Fresh negotiation baseline");
        SetProgressLabel(ReceiverLivedProgressText, "Frozen-convention control");
        SetProgressLabel(EvaluationStepText, "3. Change the world and judge");
        SetProgressLabel(EvaluationAssertionsProgressText, "Regime shift + seven falsification checks");
        SetProgressLabel(EvaluationVerdictProgressText, "Protocol verdict");
    }

    private void SetProtocol04ProgressLabels()
    {
        SetProgressLabel(SourceStepText, "1. Build private plurality");
        SetProgressLabel(SourceDirectProgressText, "Seed-specific social circumstance");
        SetProgressLabel(SourcePublishProgressText, "Three peers develop private histories");
        SetProgressLabel(ReceiverStepText, "2. Compare communication forms");
        SetProgressLabel(ReceiverLocalProgressText, "Low-dimensional typed signals");
        SetProgressLabel(ReceiverProvisionalProgressText, "Early semantic-smoothing control");
        SetProgressLabel(ReceiverLivedProgressText, "Same shared consequence remains sovereign");
        SetProgressLabel(EvaluationStepText, "3. Evaluate");
        SetProgressLabel(EvaluationAssertionsProgressText, "Seven falsification checks");
        SetProgressLabel(EvaluationVerdictProgressText, "Protocol verdict");
    }

    private static void SetProgressLabel(TextBlock textBlock, string label)
    {
        textBlock.Tag = label;
        textBlock.Text = $"[ ] {label}";
    }

    private void ResetVisualization()
    {
        _updatingSelectors = true;
        try
        {
            GraphSeedComboBox.ItemsSource = null;
            GraphSeedComboBox.SelectedIndex = -1;
        }
        finally
        {
            _updatingSelectors = false;
        }

        ResetSelectedSeedVisualization();
    }

    private void ResetSelectedSeedVisualization()
    {
        MetricPlot.Reset();
        _maximizedPlotWindow?.Plot.Reset();
        MindGrid.ItemsSource = null;
        TraceGrid.ItemsSource = null;
        TimelineGrid.ItemsSource = null;
        MetricComboBox.ItemsSource = null;
        SeriesComboBox.ItemsSource = null;
        _catalogVersion = -1;
        _timelineVersion = -1;
        _detailVersion = -1;
        _lastPlotStoreVersion = -1;
        _lastPlotWidth = 0;
        _nextGraphRefreshTimestamp = 0;
        ResetProtocolProgress();
        UpdateSelectorNavigationButtons();
        UpdateMaximizedSelection();
    }

    private void ResetProtocolResultsView()
    {
        ProtocolResultsGrid.ItemsSource = null;
        ProtocolResultsGrid.SelectedItem = null;
        ResultAssertionsGrid.ItemsSource = null;
        ProtocolResultsSummaryText.Text = "No protocol has reached a judged result yet.";
        _protocolResultsVersion = -1;
    }

    private void ResetProtocolProgress()
    {
        _progressExperiment = null;
        SetProtocol09ProgressLabels();
        SetAllProgressPending();
    }

    private void SetAllProgressPending()
    {
        SetProgressLine(SourceStepText, ProgressMark.Pending);
        SetProgressLine(SourceDirectProgressText, ProgressMark.Pending);
        SetProgressLine(SourcePublishProgressText, ProgressMark.Pending);
        SetProgressLine(ReceiverStepText, ProgressMark.Pending);
        SetProgressLine(ReceiverLocalProgressText, ProgressMark.Pending);
        SetProgressLine(ReceiverProvisionalProgressText, ProgressMark.Pending);
        SetProgressLine(ReceiverLivedProgressText, ProgressMark.Pending);
        SetProgressLine(EvaluationStepText, ProgressMark.Pending);
        SetProgressLine(EvaluationAssertionsProgressText, ProgressMark.Pending);
        SetProgressLine(EvaluationVerdictProgressText, ProgressMark.Pending);
        SetProgressCard(SourceProgressCard, ProgressMark.Pending);
        SetProgressCard(ReceiverProgressCard, ProgressMark.Pending);
        SetProgressCard(EvaluationProgressCard, ProgressMark.Pending);
    }

    private void SetProgressLine(TextBlock textBlock, ProgressMark mark)
    {
        var label = textBlock.Tag as string ?? textBlock.Text;
        textBlock.Text = $"{ProgressMarker(mark)} {label}";
        textBlock.Foreground = mark switch
        {
            ProgressMark.Current => (Brush)FindResource("AccentBrush"),
            ProgressMark.Complete => (Brush)FindResource("TextBrush"),
            _ => (Brush)FindResource("MutedBrush"),
        };
    }

    private void SetProgressCard(Border card, ProgressMark mark)
    {
        card.Background = mark == ProgressMark.Current
            ? (Brush)FindResource("SelectionBrush")
            : (Brush)FindResource("PanelAltBrush");
        card.BorderBrush = mark == ProgressMark.Current
            ? (Brush)FindResource("AccentBrush")
            : (Brush)FindResource("BorderBrush");
    }

    private void UpdateSelectorNavigationButtons()
    {
        SetSelectorButtons(MetricComboBox, PreviousMetricButton, NextMetricButton);
        SetSelectorButtons(SeriesComboBox, PreviousSeriesButton, NextSeriesButton);
        SetSelectorButtons(GraphSeedComboBox, PreviousGraphSeedButton, NextGraphSeedButton);
    }

    private void PlotSeriesVisibilityChanged(object? sender, EventArgs eventArgs)
    {
        if (sender is not FastMetricPlot source)
        {
            return;
        }

        var hidden = source.GetHiddenSeriesVisibility();
        if (!ReferenceEquals(source, MetricPlot))
        {
            MetricPlot.SetHiddenSeriesVisibility(hidden, rebuild: _maximizedPlotWindow is null);
        }

        if (_maximizedPlotWindow is { } window && !ReferenceEquals(source, window.Plot))
        {
            window.Plot.SetHiddenSeriesVisibility(hidden);
        }

        _lastPlotStoreVersion = -1;
        _nextGraphRefreshTimestamp = 0;
    }

    private void UpdateMaximizedSelection() => _maximizedPlotWindow?.UpdateSelection(
        MetricComboBox.SelectedItem as string,
        SeriesComboBox.SelectedItem as string);

    private void MaximizedPlotWindowClosed(object? sender, EventArgs eventArgs)
    {
        if (sender is MetricPlotWindow window)
        {
            window.Closed -= MaximizedPlotWindowClosed;
            window.Plot.SeriesVisibilityChanged -= PlotSeriesVisibilityChanged;
            MetricPlot.SetHiddenSeriesVisibility(window.Plot.GetHiddenSeriesVisibility(), rebuild: false);
        }

        _maximizedPlotWindow = null;
        _lastPlotStoreVersion = -1;
        _lastPlotWidth = 0;
        _nextGraphRefreshTimestamp = 0;
    }

    private void SetRunningState(bool isRunning)
    {
        RunButton.IsEnabled = !isRunning;
        PauseButton.IsEnabled = isRunning;
        StepButton.IsEnabled = false;
        ResumeButton.IsEnabled = false;
        CancelButton.IsEnabled = isRunning;
        SeedPresetComboBox.IsEnabled = !isRunning;
        SeedTextBox.IsEnabled = !isRunning;
        PaceComboBox.IsEnabled = !isRunning;
        OutputTextBox.IsEnabled = !isRunning;
    }

    private void WindowClosing(object? sender, CancelEventArgs eventArgs)
    {
        if (_allowClose || !_coordinator.IsRunning)
        {
            _refreshTimer.Stop();
            _coordinator.Dispose();
            return;
        }

        eventArgs.Cancel = true;
        _closeAfterRun = true;
        _coordinator.Cancel();
        StatusText.Text = "Cancelling at a safe boundary before closing";
    }

    private void UpdateSessionProgress(DesktopSessionStatus session)
    {
        if (session.SeedCount == 0)
        {
            SessionProgressText.Text = "Session: idle";
            SeedIndicatorText.Text = "SEED --";
            SeedIndicatorBorder.BorderBrush = (Brush)FindResource("BorderBrush");
            UpdateMaximizedGraphSeed(_maximizedPlotWindow, session);
            return;
        }

        if (session.ActiveSeed is { } activeSeed)
        {
            SessionProgressText.Text = $"History {session.ActiveSeedIndex}/{session.SeedCount} | completed {session.CompletedSeedCount}";
            SeedIndicatorText.Text = $"SEED {activeSeed}  ({session.ActiveSeedIndex}/{session.SeedCount})";
            SeedIndicatorBorder.BorderBrush = (Brush)FindResource("AccentBrush");
            UpdateMaximizedGraphSeed(_maximizedPlotWindow, session);
            return;
        }

        SessionProgressText.Text = session.CompletedSeedCount == session.SeedCount
            ? $"Session complete: {session.CompletedSeedCount}/{session.SeedCount} seeds"
            : $"Session: {session.CompletedSeedCount}/{session.SeedCount} seeds completed";
        if (_displayedSeed is { } displayedSeed)
        {
            var suffix = session.CompletedSeedCount == session.SeedCount ? " complete" : " partial";
            SeedIndicatorText.Text = $"SEED {displayedSeed} ({session.ActiveSeedIndex}/{session.SeedCount}){suffix}";
            UpdateMaximizedGraphSeed(_maximizedPlotWindow, session);
        }
        else
        {
            SeedIndicatorText.Text = "SEED --";
            UpdateMaximizedGraphSeed(_maximizedPlotWindow, session);
        }

        SeedIndicatorBorder.BorderBrush = (Brush)FindResource("BorderBrush");
    }

    private void RefreshGraphSeedOptions(DisplayTelemetryPipeline telemetry, DesktopSessionStatus session)
    {
        var seeds = telemetry.GetAvailableSeeds();
        var currentSeed = GraphSeedComboBox.SelectedItem is ulong selected ? selected : (ulong?)null;
        var targetSeed = currentSeed;

        if (_followActiveGraphSeed && session.ActiveSeed is { } activeSeed && telemetry.ContainsSeed(activeSeed))
        {
            targetSeed = activeSeed;
        }
        else if (targetSeed is not { } retained || !telemetry.ContainsSeed(retained))
        {
            targetSeed = session.ActiveSeed is { } liveSeed && telemetry.ContainsSeed(liveSeed)
                ? liveSeed
                : seeds.Length > 0 ? seeds[^1] : null;
        }

        var optionsChanged = !SeedOptionsMatch(seeds);
        var selectionChanged = targetSeed != currentSeed;
        if (!optionsChanged && !selectionChanged)
        {
            return;
        }

        _updatingSelectors = true;
        try
        {
            if (optionsChanged)
            {
                GraphSeedComboBox.ItemsSource = seeds;
            }

            GraphSeedComboBox.SelectedItem = targetSeed;
        }
        finally
        {
            _updatingSelectors = false;
        }

        if (selectionChanged)
        {
            ResetSelectedSeedVisualization();
        }

        UpdateSelectorNavigationButtons();
        UpdateMaximizedGraphSeed(_maximizedPlotWindow, session);
    }

    private bool SeedOptionsMatch(ulong[] seeds)
    {
        if (GraphSeedComboBox.Items.Count != seeds.Length)
        {
            return false;
        }

        for (var index = 0; index < seeds.Length; index++)
        {
            if (GraphSeedComboBox.Items[index] is not ulong seed || seed != seeds[index])
            {
                return false;
            }
        }

        return true;
    }

    private TelemetryStore GetSelectedTelemetryStore(DisplayTelemetryPipeline telemetry) =>
        GraphSeedComboBox.SelectedItem is ulong seed && telemetry.ContainsSeed(seed)
            ? telemetry.GetStore(seed)
            : telemetry.Store;

    private void UpdateMaximizedGraphSeed(MetricPlotWindow? window, DesktopSessionStatus session)
    {
        if (window is null)
        {
            return;
        }

        if (GraphSeedComboBox.SelectedItem is ulong seed)
        {
            var options = _coordinator.Telemetry.GetAvailableSeeds();
            var seedIndex = Array.IndexOf(options, seed);
            window.UpdateSeed(seed, seedIndex >= 0 ? seedIndex + 1 : 0, options.Length);
            return;
        }

        window.UpdateSeed(session.ActiveSeed, session.ActiveSeedIndex, session.SeedCount);
    }

    private static bool TryParseSeeds(string text, out ulong[] seeds, out string error)
    {
        var tokens = text.Split(SeedSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            seeds = [];
            error = "Enter at least one unsigned whole-number seed.";
            return false;
        }

        var parsed = new List<ulong>(tokens.Length);
        var seen = new HashSet<ulong>();
        foreach (var token in tokens)
        {
            if (!ulong.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var seed))
            {
                seeds = [];
                error = $"Seed '{token}' is not an unsigned whole number.";
                return false;
            }

            if (seen.Add(seed))
            {
                parsed.Add(seed);
            }
        }

        seeds = parsed.ToArray();
        error = string.Empty;
        return true;
    }

    private static void StepSelection(ComboBox comboBox, int direction)
    {
        if (comboBox.Items.Count == 0 || direction == 0)
        {
            return;
        }

        var current = comboBox.SelectedIndex >= 0 ? comboBox.SelectedIndex : 0;
        comboBox.SelectedIndex = Math.Clamp(current + direction, 0, comboBox.Items.Count - 1);
    }

    private static void SetSelectorButtons(ComboBox comboBox, Button previous, Button next)
    {
        previous.IsEnabled = comboBox.SelectedIndex > 0;
        next.IsEnabled = comboBox.SelectedIndex >= 0 && comboBox.SelectedIndex < comboBox.Items.Count - 1;
    }

    private static string? GetCurrentExperimentName(TelemetryTimelineSnapshot timeline)
    {
        for (var index = timeline.Items.Count - 1; index >= 0; index--)
        {
            var item = timeline.Items[index];
            if (item.Kind == ExperimentFrameKind.ExperimentStarted && !string.Equals(item.Experiment, "run", StringComparison.Ordinal))
            {
                return item.Experiment;
            }
        }

        return null;
    }

    private static bool HasTimelineEvent(
        TelemetryTimelineSnapshot timeline,
        string experiment,
        ExperimentFrameKind kind,
        string? series = null,
        string? phase = null)
    {
        for (var index = 0; index < timeline.Items.Count; index++)
        {
            var item = timeline.Items[index];
            if (!string.Equals(item.Experiment, experiment, StringComparison.Ordinal) || item.Kind != kind)
            {
                continue;
            }

            if (series is not null && !string.Equals(item.Series, series, StringComparison.Ordinal))
            {
                continue;
            }

            if (phase is not null && !string.Equals(item.Phase, phase, StringComparison.Ordinal))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static string ProgressMarker(ProgressMark mark) => mark switch
    {
        ProgressMark.Complete => "[x]",
        ProgressMark.Current => "[>]",
        _ => "[ ]",
    };

    private static string ResolveDefaultArtifactRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Cpa.BoundedMindsLab.sln")))
            {
                return Path.Combine(directory.FullName, "_artifacts");
            }

            directory = directory.Parent;
        }

        return Path.Combine(Environment.CurrentDirectory, "_artifacts");
    }

    private static string CreateRunOutputDirectory(string artifactRoot)
    {
        var stem = $"desktop-{DateTimeOffset.Now:yyyyMMdd-HHmmss}";
        var candidate = Path.Combine(artifactRoot, stem);
        var suffix = 1;
        while (Directory.Exists(candidate))
        {
            candidate = Path.Combine(artifactRoot, $"{stem}-{suffix:00}");
            suffix++;
        }

        return candidate;
    }

    private enum ProgressMark
    {
        Pending,
        Current,
        Complete,
    }
}
