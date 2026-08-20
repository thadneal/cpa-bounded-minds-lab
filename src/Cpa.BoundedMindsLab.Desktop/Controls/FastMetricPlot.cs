using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Cpa.BoundedMindsLab.Desktop.Services;
using Cpa.BoundedMindsLab.Desktop.ViewModels;

namespace Cpa.BoundedMindsLab.Desktop.Controls;

public sealed class FastMetricPlot : FrameworkElement
{
    private static readonly Brush BackgroundBrush = Freeze(new SolidColorBrush(Color.FromRgb(17, 20, 27)));
    private static readonly Brush GridBrush = Freeze(new SolidColorBrush(Color.FromRgb(55, 62, 76)));
    private static readonly Brush TextBrush = Freeze(new SolidColorBrush(Color.FromRgb(224, 229, 239)));
    private static readonly Brush MutedBrush = Freeze(new SolidColorBrush(Color.FromRgb(143, 153, 171)));
    private static readonly Brush CalloutBrush = Freeze(new SolidColorBrush(Color.FromArgb(235, 28, 33, 43)));
    private static readonly Brush[] SeriesBrushes =
    [
        Freeze(new SolidColorBrush(Color.FromRgb(83, 178, 255))),
        Freeze(new SolidColorBrush(Color.FromRgb(245, 165, 66))),
        Freeze(new SolidColorBrush(Color.FromRgb(122, 210, 132))),
        Freeze(new SolidColorBrush(Color.FromRgb(216, 112, 173))),
        Freeze(new SolidColorBrush(Color.FromRgb(178, 139, 255))),
        Freeze(new SolidColorBrush(Color.FromRgb(242, 213, 95))),
    ];

    private readonly DispatcherTimer _hoverTimer;
    private readonly DispatcherTimer _resizeTimer;
    private readonly List<RenderedSeries> _rendered = [];
    private readonly List<LegendHit> _legendHits = [];
    private readonly List<BarHit> _barHits = [];
    private readonly HashSet<string> _hiddenSeriesVisibility = new(StringComparer.Ordinal);
    private MetricPlotSnapshot? _snapshot;
    private DrawingGroup? _staticDrawing;
    private Rect _plotRect;
    private Point? _mouse;
    private Point? _pendingMouse;
    private long _signature;

    public FastMetricPlot()
    {
        Cursor = Cursors.Cross;
        _hoverTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(40),
        };
        _hoverTimer.Tick += FlushHover;
        _resizeTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(140),
        };
        _resizeTimer.Tick += FinishResize;
        MouseMove += MouseMoved;
        MouseLeave += MouseLeft;
        MouseLeftButtonDown += MouseLeftButtonPressed;
    }

    public event EventHandler? SeriesVisibilityChanged;

    public double LastBuildMilliseconds { get; private set; }

    public int DisplayPointCount { get; private set; }

    public string[] GetHiddenSeriesVisibility() => [.. _hiddenSeriesVisibility];

    public void SetHiddenSeriesVisibility(IEnumerable<string> hiddenSeriesVisibility, bool rebuild = true)
    {
        ArgumentNullException.ThrowIfNull(hiddenSeriesVisibility);
        var replacement = new HashSet<string>(hiddenSeriesVisibility, StringComparer.Ordinal);
        if (_hiddenSeriesVisibility.SetEquals(replacement))
        {
            return;
        }

        _hiddenSeriesVisibility.Clear();
        _hiddenSeriesVisibility.UnionWith(replacement);
        _signature = 0;
        if (rebuild && _snapshot is not null)
        {
            RebuildStaticDrawing();
        }
    }


    public void ShowAllCurrentSeries()
    {
        SetCurrentSeriesVisibility(visible: true);
    }

    public void HideAllCurrentSeries()
    {
        SetCurrentSeriesVisibility(visible: false);
    }

    public void SetSnapshot(MetricPlotSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var signature = ComputeSignature(snapshot, ActualWidth, ActualHeight);
        if (_snapshot is not null && signature == _signature)
        {
            return;
        }

        _snapshot = snapshot;
        _signature = signature;
        RebuildStaticDrawing();
    }

    public void Reset()
    {
        _snapshot = null;
        _staticDrawing = null;
        _rendered.Clear();
        _legendHits.Clear();
        _barHits.Clear();
        _hiddenSeriesVisibility.Clear();
        _mouse = null;
        _pendingMouse = null;
        _signature = 0;
        DisplayPointCount = 0;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        drawingContext.DrawRectangle(BackgroundBrush, null, new Rect(RenderSize));
        if (_staticDrawing is not null)
        {
            drawingContext.DrawDrawing(_staticDrawing);
        }
        else
        {
            DrawText(drawingContext, "Waiting for numeric telemetry...", new Point(18, 20), 13, MutedBrush);
        }

        DrawHover(drawingContext);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        _resizeTimer.Stop();
        _resizeTimer.Start();
    }

    private void RebuildStaticDrawing()
    {
        if (_snapshot is null || ActualWidth < 80 || ActualHeight < 80)
        {
            return;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var drawing = new DrawingGroup();
        _rendered.Clear();
        _legendHits.Clear();
        _barHits.Clear();
        DisplayPointCount = 0;
        using (var context = drawing.Open())
        {
            DrawStatic(context, _snapshot);
        }

        if (drawing.CanFreeze)
        {
            drawing.Freeze();
        }

        _staticDrawing = drawing;
        _signature = ComputeSignature(_snapshot, ActualWidth, ActualHeight);
        stopwatch.Stop();
        LastBuildMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
        InvalidateVisual();
    }

    private void DrawStatic(DrawingContext drawingContext, MetricPlotSnapshot snapshot)
    {
        var scalarComparison = snapshot.Series.Count >= 2 &&
            snapshot.Series.All(series => series.Points.Count == 1);
        var guidance = MetricGuidance.For(snapshot.Metric);
        DrawText(drawingContext, snapshot.Metric, new Point(14, 8), 15, TextBrush);
        DrawGuidanceLine(drawingContext, $"Y: {guidance.ValueDescription}", 29);
        DrawGuidanceLine(drawingContext, $"Preferred: {guidance.Preference}", 44);
        DrawGuidanceLine(
            drawingContext,
            $"X: {(scalarComparison ? guidance.ComparisonXAxisDescription : guidance.TimeXAxisDescription)}",
            59);

        if (snapshot.Series.Count == 0)
        {
            DrawText(drawingContext, "No committed observations for this metric/path selection yet.", new Point(18, 88), 12, MutedBrush);
            return;
        }

        var legend = CalculateLegendLayout(snapshot.Series.Count);
        _plotRect = new Rect(
            62,
            legend.Bottom,
            Math.Max(1, ActualWidth - 82),
            Math.Max(1, ActualHeight - legend.Bottom - 38));

        for (var index = 0; index < snapshot.Series.Count; index++)
        {
            var series = snapshot.Series[index];
            var brush = SeriesBrushes[index % SeriesBrushes.Length];
            DrawLegend(drawingContext, legend, index, series, brush, IsSeriesHidden(snapshot.Metric, series.Key));
        }

        var visibleSeries = new List<(int Index, PlotSeriesSnapshot Series)>();
        for (var index = 0; index < snapshot.Series.Count; index++)
        {
            var series = snapshot.Series[index];
            if (!IsSeriesHidden(snapshot.Metric, series.Key) && series.Points.Count > 0)
            {
                visibleSeries.Add((index, series));
            }
        }

        if (visibleSeries.Count == 0)
        {
            DrawText(
                drawingContext,
                "All graph lines are hidden. Click a legend key to show a line.",
                new Point(18, legend.Bottom + 12),
                12,
                MutedBrush);
            _plotRect = Rect.Empty;
            return;
        }

        if (scalarComparison)
        {
            DrawScalarComparison(drawingContext, visibleSeries);
            return;
        }

        var allPoints = new List<PlotPoint>();
        for (var index = 0; index < visibleSeries.Count; index++)
        {
            allPoints.AddRange(visibleSeries[index].Series.Points);
        }

        var minX = allPoints.Min(point => point.X);
        var maxX = allPoints.Max(point => point.X);
        var minY = allPoints.Min(point => point.Y);
        var maxY = allPoints.Max(point => point.Y);
        if (Math.Abs(maxX - minX) < 1e-12)
        {
            minX -= 0.5;
            maxX += 0.5;
        }

        if (Math.Abs(maxY - minY) < 1e-12)
        {
            minY -= 0.5;
            maxY += 0.5;
        }
        else
        {
            var padding = (maxY - minY) * 0.06;
            minY -= padding;
            maxY += padding;
        }

        var axis = new Axis(_plotRect, minX, maxX, minY, maxY);
        DrawGrid(drawingContext, axis);
        for (var index = 0; index < visibleSeries.Count; index++)
        {
            var entry = visibleSeries[index];
            var brush = SeriesBrushes[entry.Index % SeriesBrushes.Length];
            var locations = entry.Series.Points.Select(point => Map(point, axis)).ToArray();
            DisplayPointCount += locations.Length;
            if (locations.Length == 1)
            {
                // A single observation has no stroked line segment. Keep it visible
                // for a one-series metric while multi-series scalar result metrics
                // use the comparison-bar path above.
                drawingContext.DrawEllipse(brush, new Pen(BackgroundBrush, 1.2), locations[0], 4.2, 4.2);
            }
            else
            {
                var geometry = CreateGeometry(locations);
                drawingContext.DrawGeometry(null, new Pen(brush, 1.6), geometry);
            }

            _rendered.Add(new RenderedSeries(entry.Series, brush, locations));
        }
    }


    private void DrawScalarComparison(
        DrawingContext drawingContext,
        List<(int Index, PlotSeriesSnapshot Series)> visibleSeries)
    {
        var values = visibleSeries.Select(entry => entry.Series.Points[0].Y).ToArray();
        var minimumValue = values.Min();
        var maximumValue = values.Max();
        var minimumY = Math.Min(0.0, minimumValue);
        var maximumY = Math.Max(0.0, maximumValue);
        if (Math.Abs(maximumY - minimumY) < 1e-12)
        {
            maximumY = minimumY + 1.0;
        }
        else
        {
            var padding = (maximumY - minimumY) * 0.08;
            if (minimumY < 0.0)
            {
                minimumY -= padding;
            }

            if (maximumY > 0.0)
            {
                maximumY += padding;
            }
        }

        DrawComparisonGrid(drawingContext, minimumY, maximumY);
        DrawText(
            drawingContext,
            "scalar comparison",
            new Point(Math.Max(14.0, ActualWidth - 150.0), 10),
            10.5,
            MutedBrush);

        var zeroY = MapY(0.0, minimumY, maximumY);
        var slotWidth = _plotRect.Width / visibleSeries.Count;
        var barWidth = Math.Max(10.0, Math.Min(92.0, slotWidth * 0.58));
        for (var index = 0; index < visibleSeries.Count; index++)
        {
            var entry = visibleSeries[index];
            var point = entry.Series.Points[0];
            var brush = SeriesBrushes[entry.Index % SeriesBrushes.Length];
            var centerX = _plotRect.Left + (slotWidth * (index + 0.5));
            var valueY = MapY(point.Y, minimumY, maximumY);
            var top = Math.Min(zeroY, valueY);
            var height = Math.Max(1.5, Math.Abs(zeroY - valueY));
            var bar = new Rect(centerX - (barWidth / 2.0), top, barWidth, height);
            drawingContext.DrawRectangle(brush, new Pen(BackgroundBrush, 1.0), bar);

            var valueText = point.Y.ToString("0.###", CultureInfo.InvariantCulture);
            var valueYPosition = point.Y >= 0.0 ? Math.Max(_plotRect.Top, top - 18.0) : Math.Min(_plotRect.Bottom - 16.0, bar.Bottom + 3.0);
            var valueBounds = MeasureText(valueText, 9.5);
            DrawText(
                drawingContext,
                valueText,
                new Point(centerX - (valueBounds.Width / 2.0), valueYPosition),
                9.5,
                TextBrush);

            var category = Shorten(TerminalLabel(entry.Series.Label), Math.Max(8, (int)(slotWidth / 7.0)));
            var categoryBounds = MeasureText(category, 9.0);
            DrawText(
                drawingContext,
                category,
                new Point(centerX - (categoryBounds.Width / 2.0), _plotRect.Bottom + 7.0),
                9.0,
                MutedBrush);

            var location = new Point(centerX, valueY);
            _barHits.Add(new BarHit(entry.Series, bar, point));
            _rendered.Add(new RenderedSeries(entry.Series, brush, [location]));
            DisplayPointCount++;
        }
    }

    private void DrawComparisonGrid(DrawingContext drawingContext, double minimumY, double maximumY)
    {
        var borderPen = new Pen(GridBrush, 1.0);
        var gridPen = new Pen(GridBrush, 0.55);
        drawingContext.DrawRectangle(null, borderPen, _plotRect);
        for (var index = 0; index <= 4; index++)
        {
            var fraction = index / 4.0;
            var y = _plotRect.Bottom - (_plotRect.Height * fraction);
            drawingContext.DrawLine(gridPen, new Point(_plotRect.Left, y), new Point(_plotRect.Right, y));
            var value = minimumY + ((maximumY - minimumY) * fraction);
            DrawText(drawingContext, value.ToString("0.###", CultureInfo.InvariantCulture), new Point(6, y - 7), 9.5, MutedBrush);
        }

        if (minimumY < 0.0 && maximumY > 0.0)
        {
            var zeroY = MapY(0.0, minimumY, maximumY);
            drawingContext.DrawLine(new Pen(MutedBrush, 0.9), new Point(_plotRect.Left, zeroY), new Point(_plotRect.Right, zeroY));
        }
    }

    private double MapY(double value, double minimumY, double maximumY) =>
        _plotRect.Bottom - (((value - minimumY) / (maximumY - minimumY)) * _plotRect.Height);

    private Size MeasureText(string text, double size)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            size,
            TextBrush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
        return new Size(formatted.Width, formatted.Height);
    }

    private static string TerminalLabel(string label)
    {
        var slash = label.LastIndexOf('/');
        return slash >= 0 && slash < label.Length - 1 ? label[(slash + 1)..] : label;
    }

    private void DrawGrid(DrawingContext drawingContext, Axis axis)
    {
        var borderPen = new Pen(GridBrush, 1.0);
        var gridPen = new Pen(GridBrush, 0.55);
        drawingContext.DrawRectangle(null, borderPen, axis.Plot);
        for (var index = 0; index <= 4; index++)
        {
            var fraction = index / 4.0;
            var y = axis.Plot.Bottom - (axis.Plot.Height * fraction);
            drawingContext.DrawLine(gridPen, new Point(axis.Plot.Left, y), new Point(axis.Plot.Right, y));
            var value = axis.MinimumY + ((axis.MaximumY - axis.MinimumY) * fraction);
            DrawText(drawingContext, value.ToString("0.###", CultureInfo.InvariantCulture), new Point(6, y - 7), 9.5, MutedBrush);
        }

        DrawText(drawingContext, axis.MinimumX.ToString("0", CultureInfo.InvariantCulture), new Point(axis.Plot.Left, axis.Plot.Bottom + 7), 9.5, MutedBrush);
        DrawText(drawingContext, axis.MaximumX.ToString("0", CultureInfo.InvariantCulture), new Point(axis.Plot.Right - 36, axis.Plot.Bottom + 7), 9.5, MutedBrush);
    }

    private void DrawGuidanceLine(DrawingContext drawingContext, string text, double y)
    {
        var maximumCharacters = Math.Max(24, (int)Math.Floor((ActualWidth - 28.0) / 6.3));
        DrawText(drawingContext, Shorten(text, maximumCharacters), new Point(14, y), 9.5, MutedBrush);
    }

    private void DrawLegend(
        DrawingContext drawingContext,
        LegendLayout layout,
        int index,
        PlotSeriesSnapshot series,
        Brush brush,
        bool isHidden)
    {
        var column = index % layout.Columns;
        var row = index / layout.Columns;
        var x = layout.Left + (column * layout.ColumnWidth);
        var y = layout.Top + (row * layout.RowHeight);
        var pen = new Pen(brush, isHidden ? 1.2 : 2.4)
        {
            DashStyle = isHidden ? DashStyles.Dash : DashStyles.Solid,
        };
        drawingContext.DrawLine(pen, new Point(x, y + 6), new Point(x + 18, y + 6));

        var textBrush = isHidden ? MutedBrush : TextBrush;
        var maximumCharacters = Math.Max(8, (int)Math.Floor((layout.ColumnWidth - 58.0) / 6.2));
        var label = isHidden ? $"{series.Label} (hidden)" : series.Label;
        DrawText(drawingContext, Shorten(label, maximumCharacters), new Point(x + 24, y - 3), 10.5, textBrush);

        var availableWidth = Math.Max(80.0, Math.Min(layout.ColumnWidth - 8.0, ActualWidth - x - 8.0));
        _legendHits.Add(new LegendHit(series, brush, new Rect(x - 4, y - 5, availableWidth, layout.RowHeight)));
    }

    private LegendLayout CalculateLegendLayout(int itemCount)
    {
        const double left = 16.0;
        const double top = 82.0;
        const double rowHeight = 20.0;
        const double bottomPadding = 12.0;
        var usableWidth = Math.Max(1.0, ActualWidth - (left * 2.0));
        var columns = Math.Clamp((int)Math.Floor(usableWidth / 300.0), 1, 3);
        columns = Math.Min(columns, Math.Max(1, itemCount));
        var rows = (itemCount + columns - 1) / columns;
        var columnWidth = usableWidth / columns;
        var bottom = top + (rows * rowHeight) + bottomPadding;
        return new LegendLayout(left, top, rowHeight, columnWidth, columns, bottom);
    }

    private void DrawHover(DrawingContext drawingContext)
    {
        if (_mouse is not { } mouse)
        {
            return;
        }

        for (var index = 0; index < _legendHits.Count; index++)
        {
            var legend = _legendHits[index];
            if (!legend.Bounds.Contains(mouse))
            {
                continue;
            }

            drawingContext.DrawRoundedRectangle(
                null,
                new Pen(legend.Brush, 1.2),
                legend.Bounds,
                2,
                2);
            var lines = WrapCalloutLines(
            [
                legend.Series.Label,
                $"key: {legend.Series.Key}",
                $"rendered points: {legend.Series.Points.Count:N0}",
                IsSeriesHidden(_snapshot?.Metric ?? string.Empty, legend.Series.Key) ? "visible: no" : "visible: yes",
                IsSeriesHidden(_snapshot?.Metric ?? string.Empty, legend.Series.Key) ? "click to show" : "click to hide",
            ]);
            DrawCallout(drawingContext, lines, new Point(mouse.X, legend.Bounds.Bottom));
            return;
        }

        if (!_plotRect.Contains(mouse) || _rendered.Count == 0)
        {
            return;
        }

        for (var index = 0; index < _barHits.Count; index++)
        {
            var bar = _barHits[index];
            if (!bar.Bounds.Contains(mouse))
            {
                continue;
            }

            drawingContext.DrawRectangle(null, new Pen(TextBrush, 1.2), bar.Bounds);
            var linesForBar = WrapCalloutLines(
            [
                bar.Series.Label,
                $"{_snapshot?.Metric} {bar.Point.Y:0.######}",
                "scalar comparison",
            ]);
            DrawCallout(drawingContext, linesForBar, new Point(mouse.X, bar.Bounds.Top));
            return;
        }

        var nearest = FindNearest(mouse, 16.0);
        if (nearest is null)
        {
            return;
        }

        drawingContext.DrawEllipse(BackgroundBrush, new Pen(nearest.Brush, 2.2), nearest.Location, 5.5, 5.5);
        var linesForPoint = WrapCalloutLines(
        [
            nearest.Series.Label,
            $"sample {nearest.Point.X:0.###}",
            $"{_snapshot?.Metric} {nearest.Point.Y:0.######}",
        ]);
        DrawCallout(drawingContext, linesForPoint, nearest.Location);
    }

    private NearestPoint? FindNearest(Point mouse, double maximumDistance)
    {
        NearestPoint? nearest = null;
        var best = maximumDistance;
        foreach (var rendered in _rendered)
        {
            if (rendered.Locations.Length == 0)
            {
                continue;
            }

            var insertion = LowerBoundX(rendered.Locations, mouse.X);
            var start = Math.Max(0, insertion - 2);
            var end = Math.Min(rendered.Locations.Length - 1, insertion + 2);
            for (var index = start; index <= end; index++)
            {
                var distance = (rendered.Locations[index] - mouse).Length;
                if (distance > best)
                {
                    continue;
                }

                best = distance;
                nearest = new NearestPoint(
                    rendered.Series,
                    rendered.Series.Points[index],
                    rendered.Locations[index],
                    rendered.Brush);
            }
        }

        return nearest;
    }

    private void DrawCallout(DrawingContext drawingContext, List<string> lines, Point anchor)
    {
        var longest = 0;
        for (var index = 0; index < lines.Count; index++)
        {
            longest = Math.Max(longest, lines[index].Length);
        }

        var width = Math.Min(Math.Max(170.0, (longest * 6.8) + 18.0), Math.Max(170.0, ActualWidth - 16.0));
        var height = 12.0 + (lines.Count * 17.0);
        var x = Math.Min(Math.Max(8.0, anchor.X + 12.0), Math.Max(8.0, ActualWidth - width - 8.0));
        var y = Math.Min(Math.Max(8.0, anchor.Y - height - 10.0), Math.Max(8.0, ActualHeight - height - 8.0));
        var rect = new Rect(x, y, width, height);
        drawingContext.DrawRoundedRectangle(CalloutBrush, new Pen(GridBrush, 1.0), rect, 5, 5);
        for (var index = 0; index < lines.Count; index++)
        {
            DrawText(drawingContext, lines[index], new Point(x + 9, y + 6 + (index * 17)), 10.5, index == 0 ? TextBrush : MutedBrush);
        }
    }

    private void MouseMoved(object sender, MouseEventArgs eventArgs)
    {
        var location = eventArgs.GetPosition(this);
        _pendingMouse = location;
        Cursor = IsLegendLocation(location) ? Cursors.Hand : Cursors.Cross;
        if (!_hoverTimer.IsEnabled)
        {
            _hoverTimer.Start();
        }
    }

    private void MouseLeft(object sender, MouseEventArgs eventArgs)
    {
        _hoverTimer.Stop();
        _pendingMouse = null;
        _mouse = null;
        Cursor = Cursors.Cross;
        InvalidateVisual();
    }

    private void MouseLeftButtonPressed(object sender, MouseButtonEventArgs eventArgs)
    {
        if (_snapshot is null)
        {
            return;
        }

        var location = eventArgs.GetPosition(this);
        for (var index = 0; index < _legendHits.Count; index++)
        {
            var legend = _legendHits[index];
            if (!legend.Bounds.Contains(location))
            {
                continue;
            }

            var key = VisibilityKey(_snapshot.Metric, legend.Series.Key);
            if (!_hiddenSeriesVisibility.Add(key))
            {
                _hiddenSeriesVisibility.Remove(key);
            }

            _mouse = location;
            _pendingMouse = null;
            _signature = 0;
            RebuildStaticDrawing();
            SeriesVisibilityChanged?.Invoke(this, EventArgs.Empty);
            eventArgs.Handled = true;
            return;
        }
    }

    private void SetCurrentSeriesVisibility(bool visible)
    {
        if (_snapshot is null || _snapshot.Series.Count == 0)
        {
            return;
        }

        var changed = false;
        foreach (var series in _snapshot.Series)
        {
            var key = VisibilityKey(_snapshot.Metric, series.Key);
            changed |= visible
                ? _hiddenSeriesVisibility.Remove(key)
                : _hiddenSeriesVisibility.Add(key);
        }

        if (!changed)
        {
            return;
        }

        _signature = 0;
        RebuildStaticDrawing();
        SeriesVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool IsLegendLocation(Point location)
    {
        for (var index = 0; index < _legendHits.Count; index++)
        {
            if (_legendHits[index].Bounds.Contains(location))
            {
                return true;
            }
        }

        return false;
    }

    private void FlushHover(object? sender, EventArgs eventArgs)
    {
        if (_pendingMouse is not { } location)
        {
            _hoverTimer.Stop();
            return;
        }

        _mouse = location;
        _pendingMouse = null;
        _hoverTimer.Stop();
        InvalidateVisual();
    }

    private void FinishResize(object? sender, EventArgs eventArgs)
    {
        _resizeTimer.Stop();
        if (_snapshot is null)
        {
            return;
        }

        _signature = ComputeSignature(_snapshot, ActualWidth, ActualHeight);
        RebuildStaticDrawing();
    }

    private long ComputeSignature(MetricPlotSnapshot snapshot, double width, double height)
    {
        var hash = new HashCode();
        hash.Add(snapshot.Metric, StringComparer.Ordinal);
        hash.Add((int)Math.Round(width));
        hash.Add((int)Math.Round(height));
        foreach (var series in snapshot.Series)
        {
            hash.Add(series.Key, StringComparer.Ordinal);
            hash.Add(IsSeriesHidden(snapshot.Metric, series.Key));
            hash.Add(series.Points.Count);
            if (series.Points.Count > 0)
            {
                hash.Add(series.Points[^1].X);
                hash.Add(series.Points[^1].Y);
            }
        }

        return hash.ToHashCode();
    }

    private bool IsSeriesHidden(string metric, string seriesKey) =>
        _hiddenSeriesVisibility.Contains(VisibilityKey(metric, seriesKey));

    private static string VisibilityKey(string metric, string seriesKey) =>
        string.Concat(metric, "\u001f", seriesKey);

    private static StreamGeometry CreateGeometry(Point[] points)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            if (points.Length > 0)
            {
                context.BeginFigure(points[0], isFilled: false, isClosed: false);
                for (var index = 1; index < points.Length; index++)
                {
                    context.LineTo(points[index], isStroked: true, isSmoothJoin: false);
                }
            }
        }

        geometry.Freeze();
        return geometry;
    }

    private static Point Map(PlotPoint point, Axis axis) => new(
        axis.Plot.Left + (((point.X - axis.MinimumX) / (axis.MaximumX - axis.MinimumX)) * axis.Plot.Width),
        axis.Plot.Bottom - (((point.Y - axis.MinimumY) / (axis.MaximumY - axis.MinimumY)) * axis.Plot.Height));

    private static int LowerBoundX(Point[] points, double x)
    {
        var lower = 0;
        var upper = points.Length;
        while (lower < upper)
        {
            var middle = lower + ((upper - lower) / 2);
            if (points[middle].X < x)
            {
                lower = middle + 1;
            }
            else
            {
                upper = middle;
            }
        }

        return lower;
    }

    private Rect DrawText(DrawingContext drawingContext, string text, Point point, double size, Brush brush)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            size,
            brush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
        drawingContext.DrawText(formatted, point);
        return new Rect(point, new Size(formatted.Width, formatted.Height));
    }

    private static List<string> WrapCalloutLines(string[] sourceLines)
    {
        const int maximumCharacters = 72;
        var lines = new List<string>();
        foreach (var source in sourceLines)
        {
            if (source.Length <= maximumCharacters)
            {
                lines.Add(source);
                continue;
            }

            var remaining = source;
            while (remaining.Length > maximumCharacters)
            {
                var split = remaining.LastIndexOf(' ', maximumCharacters);
                if (split < maximumCharacters / 2)
                {
                    split = maximumCharacters;
                }

                lines.Add(remaining[..split].TrimEnd());
                remaining = remaining[split..].TrimStart();
            }

            if (remaining.Length > 0)
            {
                lines.Add(remaining);
            }
        }

        return lines;
    }

    private static string Shorten(string value, int maximum) => value.Length <= maximum
        ? value
        : value[..Math.Max(1, maximum - 3)] + "...";

    private static T Freeze<T>(T value)
        where T : Freezable
    {
        value.Freeze();
        return value;
    }

    private readonly record struct LegendLayout(
        double Left,
        double Top,
        double RowHeight,
        double ColumnWidth,
        int Columns,
        double Bottom);

    private sealed record RenderedSeries(
        PlotSeriesSnapshot Series,
        Brush Brush,
        Point[] Locations);

    private sealed record LegendHit(
        PlotSeriesSnapshot Series,
        Brush Brush,
        Rect Bounds);

    private sealed record BarHit(
        PlotSeriesSnapshot Series,
        Rect Bounds,
        PlotPoint Point);

    private sealed record NearestPoint(
        PlotSeriesSnapshot Series,
        PlotPoint Point,
        Point Location,
        Brush Brush);

    private readonly record struct Axis(
        Rect Plot,
        double MinimumX,
        double MaximumX,
        double MinimumY,
        double MaximumY);
}
