# WPF Desktop Laboratory

## Theme boundary

The Desktop Lab owns its dark palette explicitly rather than depending on the current Windows application theme. `Themes/LabTheme.xaml` overrides both the controls used by the workbench and the framework system brushes that stock WPF templates may consult. This keeps experiment controls, disabled states, dropdowns, tab headers, data-grid headers, scrollbars, and empty navigation surfaces visually consistent across Windows theme settings. The theme is presentation-only and is never visible to experiment execution or telemetry production.

## Purpose

The desktop application is an experimental instrument. It must remain responsive enough to inspect a run without becoming part of the run's cognitive dynamics.

The old CPA Cognitive Development Lab workbench was a useful reference. It already had several sound ideas: background experiment execution, a bounded drop-oldest display channel, custom WPF drawing, per-series compaction, display downsampling, cached drawing groups, and throttled mouse hover.

The successor workbench moves the isolation boundary one step farther. Experiment execution uses a dedicated below-normal-priority thread so WPF does not share the dispatcher or ordinary thread-pool scheduling path with the active experiment.

## New data path

```text
Dedicated below-normal-priority experiment thread
    |
    | IExperimentFrameObserver.Try-style enqueue
    v
bounded ConcurrentQueue
    |                  durable journal is separate and complete
    | background
    v
DisplayTelemetryPipeline projector
    |
    v
TelemetryStore
  bounded raw/recent numeric points
  retiring fine-resolution envelopes
  persistent coarse envelope history
  latest public mind/trace surfaces
  bounded timeline
    |
    | sampled about 15 Hz
    v
WPF dispatcher
    |
    v
FastMetricPlot + tables
```

The dispatcher does not drain experiment frames. It asks for snapshots of already-projected state.

## Backpressure policy

The display queue is bounded to 16,384 frames. If projection falls behind, oldest display-only frames are discarded.

This does not alter:

- experiment state;
- deterministic predictions;
- communication standing;
- verdict computation;
- the core `frames.ndjson` journal.

The status bar reports published, projected, dropped, and queued display frames. A dropped count above zero is an instrumentation warning, not a scientific failure by itself.

## Numeric series

Each metric series is appended by the background projector. Several power-of-scale envelope levels summarize first, last, minimum, and maximum observations for fixed sample buckets.

When WPF requests a graph snapshot, the store chooses a level whose display point count fits the approximate horizontal pixel budget. This avoids rescanning and redrawing every raw sample simply because a run contains many observations.

Raw live points are retained only through 32,768 samples per metric series. Once a series is too large to be useful raw, that raw display copy is released. Fine envelope levels also retire after 4,096 completed envelopes. Coarser levels continue to represent the complete run, with the coarsest level retained without that retirement cap. The complete authoritative history remains the durable journal. This keeps UI memory from becoming a second archival system.

## Rendering

`FastMetricPlot` uses:

- one cached `DrawingGroup` for static chart content;
- one `StreamGeometry` per visible series;
- at most six visible series in a snapshot;
- display points already reduced by the telemetry store;
- hover lookup by binary search around the mouse x coordinate;
- a 40 ms hover throttle;
- 140 ms resize debounce so splitter drag does not rebuild geometry for every size event.

Status and light UI state are sampled by the 67 ms UI timer. Graph rebuilds are separately capped to roughly 10 Hz under normal load, 6 to 7 Hz after a moderately expensive build, and 4 Hz when a build reaches 16 ms or the projection backlog grows beyond 4,096 frames. Hover still uses the cached drawing and does not wait for a fresh graph snapshot. Hundreds or thousands of experiment events can arrive between screen updates without creating hundreds or thousands of WPF invalidations.

## Freeze graph

`Freeze graph (experiment continues)` stops graph snapshot/rebuild work while leaving the experiment worker and telemetry projector active.

This is useful when manipulating tables, splitters, or another application during a very fast run. It is a presentation control only.

## Multi-seed sessions

The Seeds field accepts one or more unsigned seeds separated by commas, spaces, semicolons, or new lines. Duplicate values are ignored so a session cannot accidentally overwrite the same `seed-N` directory. Selected experiments run to completion for one seed before the next seed begins.

The display telemetry store is reset between seeds. The graph, timeline, public mind state, trace state, and protocol progress therefore represent the currently active history only. Independent histories are never merged into one live series. The complete scientific record remains in the per-seed durable artifacts, and `replication-report.json` aggregates the completed session after the final seed. A compact session indicator above the protocol steps shows the active seed index and number of completed histories.

## Run control

Pause, step, resume, and cancel operate at explicit experiment observation boundaries within the current seed.

`Step observation` releases one actual boundary. There is no separate UI-only "next stage" command in this first workbench, avoiding the earlier ambiguity between playback detail and real execution.

Cancellation uses cooperative boundary checks. The desktop worker catches expected `OperationCanceledException` on the worker path after the core records the cancellation frame and partial manifest. A debugger configured to break on first-chance `OperationCanceledException` may still stop when it is thrown, but application handling is explicit.

## Performance instrumentation

The status bar reports:

```text
published display frames
projected display frames
dropped display frames
projector backlog
last projector batch time
rendered graph point count
last graph geometry build time
```

These numbers make it possible to distinguish at least three different pressures:

1. experiment/frame production rate;
2. background display projection cost;
3. WPF graph rendering cost.

Formal scientific metrics should never use these presentation measurements as evidence about cognition.

## Inspection ergonomics

The live metric selector and focus-path selector each have bounded previous/next buttons so an operator can walk the catalog without repeatedly opening a dropdown. The buttons stop at the first and last item rather than wrapping, making position in the catalog explicit.

Graph legend labels remain shortened in the static drawing to protect plot space. Hovering a legend key uses the same throttled custom hit-testing path as point hover and shows the full display label, internal series key, and rendered point count. No WPF child controls are created per legend item.

Legend keys are also presentation controls. Clicking a key hides or restores that series for the current metric. Hidden entries remain in the legend with muted text and a dashed key, so a line can always be brought back. Hover reports whether the line is visible and the click action that will occur. Axis bounds are computed from visible series only, which makes hiding a dominant line useful for inspecting smaller signals. Visibility is keyed by metric plus series rather than by series alone, so isolating a path on one metric does not unexpectedly hide it on another. The embedded and maximized plots share this presentation state.

The legend layout is width-aware rather than fixed at two columns. It uses between one and three columns and calculates the reserved graph-top region from the resulting row count. As series appear or disappear from a snapshot, the plot begins below the actual legend footprint instead of relying on a fixed-height estimate that can overlap the graph.

`Maximize graph` opens a maximized presentation window and moves live graph rebuilding to that plot while it is open. The hidden main plot is not rebuilt in parallel, avoiding duplicate geometry work. Closing the maximized view invalidates the main graph once so it catches up on the next display sample.

The protocol progress strip is derived from projected structural timeline frames. For Protocol 01 it exposes three major movements and their actual substeps: source development/direct consequence and public trace publication; the local-only, provisional-transfer, and lived-equivalent receiver paths; then the six falsification checks and verdict. Completed items are marked `[x]`, the active item `[>]`, and future items `[ ]`. This is an observational projection only and never feeds back into experiment execution.

## Desktop artifact root

The Desktop Lab resolves the repository root by walking upward from the executable until it finds `Cpa.BoundedMindsLab.sln`, then defaults the output field to `<repo>/_artifacts`. Each session writes to a timestamped `desktop-YYYYMMDD-HHMMSS` child directory. Every seed gets its own `seed-N` directory containing the ordinary durable journal and result artifacts. The session root contains `session-manifest.json` while the session is running, cancelled, faulted, or completed, and a completed session also contains `replication-report.json`. `Open output` opens the most recent session directory when one exists, otherwise the configured artifact root. CLI output paths remain explicit, although the documentation examples now use `_artifacts` as the repository convention.

## Focus-path-aware metrics

Focus path is the primary live-graph selection. Its dropdown and previous/next controls expose projected histories that contain numeric telemetry. The Metric dropdown is then filtered to only the numeric metrics actually published by the selected focus path.

When the focus path changes, the current metric is preserved if the new path also publishes it. Otherwise the UI prefers `rolling_rmse` when available and falls back to the first metric on that path. This keeps sequential Metric navigation inside a graphable set instead of stepping through globally known metrics that are empty for the selected path.

## Protocol results

`Protocol results` sits beside the Timeline and is intentionally sourced from completed `ExperimentResult` objects held by the desktop run coordinator rather than from the bounded display-frame queue. A presentation-frame drop must never erase or alter the laboratory's judgment of a completed protocol.

Each completed seed/protocol row shows the Desktop Lab vocabulary `Supported`, `Mixed`, `Refuted`, or `Inconclusive`, plus passed/failed falsification-check counts and the experiment interpretation. Selecting a row exposes every preregistered assertion with its pass state, actual value, boundary, and description. The internal core verdict `Disconfirm` remains unchanged in scientific artifacts and is presented as `Refuted` only in the UI.

For multi-seed sessions, these result rows accumulate across completed seeds while the live telemetry store continues to reset at seed boundaries. The summary strip reports counts of the four judgments for each protocol. It does not manufacture an additional aggregate verdict; replication interpretation remains grounded in the saved per-seed results and `replication-report.json`.

## Final-only graph metrics

Some protocol metrics, including `rmse`, are published only when a receiver path completes. Such a series contains one observation, which is valid scientific telemetry but has no line segment to stroke. The custom plot therefore renders a point marker for one-observation series and symmetrically expands a degenerate x-axis around that observation. This prevents a valid final metric from looking like an empty or filtered graph after the run.

