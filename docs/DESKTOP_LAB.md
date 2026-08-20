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

Each seed has its own bounded display telemetry store for the lifetime of the Desktop Lab session. When execution advances, the completed store becomes read-only and a fresh store receives the next seed. Independent histories are never merged into one series, but an operator can return to an earlier seed without reloading the durable artifacts. The complete scientific record still belongs to the per-seed `frames.ndjson` artifacts, and `replication-report.json` aggregates the completed session after the final seed. A compact session indicator above the protocol steps shows the actively executing seed index and number of completed histories.

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

For multi-seed sessions, these result rows accumulate across completed seeds independently of the per-seed visualization stores. The summary strip reports counts of the four judgments for each protocol. It does not manufacture an additional aggregate verdict; replication interpretation remains grounded in the saved per-seed results and `replication-report.json`.

## Final-only graph metrics

Some protocol metrics, including `rmse`, are published only when a receiver path completes. Such a series contains one observation, which is valid scientific telemetry but has no line segment to stroke. The custom plot therefore renders a point marker for one-observation series and symmetrically expands a degenerate x-axis around that observation. This prevents a valid final metric from looking like an empty or filtered graph after the run.


## Active seed indicator

v0.2.0 adds a dedicated seed badge beside the session-progress text above Protocol progress. During a multi-seed session it shows both the actual seed value and the current history position, for example `SEED 307 (3/5)`. This remains visible independently of the status bar so screenshots and focused graph inspection still reveal which deterministic history is active.

The maximized graph header reports the seed selected in the graph Seed selector. This can differ from the actively executing seed when an operator has pinned an earlier history for inspection. After a completed or interrupted session, the main active-seed badge retains the last executed seed while the graph selector remains free to inspect any retained history. Both surfaces are presentation-only and cannot alter experiment execution.

## Protocol-aware progress

The progress strip is no longer hard-coded to Protocol 01. It derives the most recently started experiment from the structural timeline and switches labels and completion rules accordingly.

Protocol 02 is shown as:

```text
1. Peers develop
   Mind A private history
   Mind B private history

2. Compare conditions
   Preserved interiors
   Collapse to synchronized state
   Synchronized shared consequence

3. Evaluate
   Six falsification checks
   Protocol verdict
```

When several experiments run within one seed, the progress surface changes as the next protocol starts rather than leaving the completed Protocol 01 steps on screen. The timeline remains the complete projected structural record for the current seed.

## Default experiment selection

The default changed in v0.8.0 because the laboratory is now in validation rather than protocol-development mode. The Desktop Lab opens with all frozen Protocols 01-07 selected and **Holdout v1 (20, frozen)** as the seed preset. Development v1 remains available as a five-seed regression preset, and Custom allows exploratory seeds. Historical sections below describe the default behavior of earlier releases.

## v0.3.0 seed and Protocol 03 updates

The Seeds field now defaults to the canonical replication matrix:

```text
101, 211, 307, 401, 503
```

The values are unchanged for continuity, but Protocol 03 gives them stronger semantics: each seed selects a materially different developmental-world circumstance rather than mostly perturbing one fixed curriculum.

Protocol Progress now recognizes `03-developmental-versus-doctrinal-transfer` and displays its actual major steps and substeps: scenario generation, source development/transfer packaging, local-only receiver development, developmental transfer, doctrinal transfer, and seven-check evaluation.

The active-seed badge continues to identify the currently executing deterministic history. Seed histories remain separate visualization stores, which is especially important now that different seeds are intentionally different worlds; the graph never overlays them as though they formed one continuous history.

## v0.3.1 seed-scoped graph inspection

The Live metrics toolbar now includes a third selector, `Seed`, with previous/next navigation. Seed is an outer scope for the visualization. Choosing it switches the graph, Focus path catalog, Metric catalog, public mind/trace detail, Timeline, and Protocol Progress to that retained history.

During a new session the graph follows the active seed automatically. The first manual seed selection disables auto-follow for the rest of that run, allowing an earlier seed to remain under inspection while later histories continue executing. Selecting a retained seed never changes experiment state or scheduling.

At seed completion the display projector drains the remaining queued frames before the history is considered ready for inspection. A following seed then archives that bounded store and opens a fresh one. This prevents a visualization-only rotation from clipping the tail of a history. Retention remains intentionally display-bounded; the durable per-seed NDJSON journal is still the only complete scientific record.


## v0.4.0 Protocol 04 progress

The Desktop Lab recognizes `04-bounded-communication-before-language` as the newest protocol and selects it by default. Protocol Progress follows the assay rather than reusing Protocol 03 labels:

```text
Build private plurality
  seed-specific social circumstance
  three peers develop private histories
Compare communication forms
  low-dimensional typed signals
  early semantic-smoothing control
  same shared consequence remains sovereign
Evaluate
  seven falsification checks
  protocol verdict
```

Protocol 03 is now displayed in Run notes as a frozen Supported baseline. The existing Seed selector, active-seed badge, per-seed telemetry stores, metric/path filtering, line visibility, maximized graph, Timeline, and judged Protocol results require no protocol-specific changes for the new telemetry.


## v0.5.0 Protocol 05 and version display

The main Desktop Lab and maximized graph window derive their displayed version from the running Desktop assembly. This makes screenshots and result-review sessions attributable to the application build.

The Desktop Lab recognizes `05-emergent-convention-artificial-culture` as the newest protocol and selects it by default. Protocol Progress follows the assay:

```text
Let a culture form
  seed-specific plural coordination world
  repeated success earns local convention standing
Compare coordination modes
  earned distributed convention
  fresh negotiation baseline
  frozen-convention control
Change the world and judge
  regime shift + seven falsification checks
  protocol verdict
```

Protocol 04 now appears in Run notes as a frozen Supported baseline. Existing seed-scoped stores, graph filtering, line visibility, maximized view, Timeline, and judged Protocol results remain observer-only.


## v0.5.1 scalar comparison plots

The graph surface chooses presentation from the observed telemetry topology. Ordinary multi-point metrics remain ordered time-series lines. When two or more candidate paths each publish exactly one observation for a metric, the surface treats that metric as a terminal scalar comparison and renders bars by path. This prevents incidental result-publication ticks from becoming a misleading x-axis for values such as final RMSE or communication packet count. Legend visibility and hover inspection remain available in both modes.


## v0.5.2 metric guidance, visibility controls, and boundary-batched plotting

Every graph now explains the selected metric in-place. The header reports the y-value meaning, the preferred direction or context for interpretation, and the x-axis semantics. Time-series plots use observation/tick order on x; terminal scalar comparisons use treatment/focus path categories. Standing, disagreement, uncertainty, evidence volume, and other non-monotonic quantities are explicitly marked context-dependent rather than being assigned a false universal "higher" or "lower" preference.

`Show all` and `Hide all` act on every series key for the current metric. Clicking an individual legend key still toggles only that line/bar. The embedded and maximized plots continue sharing the same hidden-series state.

Live rendering is now boundary-batched. The background projector still ingests numeric frames continuously, but the graph does not rebuild for each incremental sample. A metric series accumulates privately in the display store until a series transition, protocol phase/developmental boundary, or terminal boundary commits that accumulated segment. Only committed-through points are eligible for plotting, so when the next path starts its partial samples do not appear in the graph that was just committed for the previous path. This reduces dispatcher/render churn at Maximum pace without slowing the experiment or weakening durable telemetry. The UI may intentionally lag the currently accumulating segment until its next meaningful boundary; `frames.ndjson` remains the complete record.

## v0.6.0 Protocol 06, incremental plotting restored, and table inspection

Protocol 06 is now the newest protocol and is selected by default. Protocol Progress follows the incomplete-ancestry assay:

```text
Generate incomplete ancestry
  seed-specific echo and independent histories
  publish partial origin hints + developmental signatures
Compare corroboration rules
  infer ancestry from incomplete public cues
  naive agreement-count control
  perfect-ancestry oracle calibration
Judge ancestry discrimination
  eight falsification checks
  protocol verdict
```

Protocol 05 now appears in Run notes as a frozen Supported baseline.

The v0.5.2 boundary-batched graph experiment is **reverted**. It did not produce a noticeable responsiveness improvement in use. Numeric telemetry remains projected off the dispatcher and display-bounded, but graph snapshots again include the latest accumulated points and may update incrementally at the existing adaptive graph cadence. Durable scientific telemetry remains unchanged.

Data-grid text now wraps inside cells instead of silently clipping long words/phrases. The dark column-header template restores WPF's left/right resize grippers, so table columns can be resized interactively. Resizing a column changes only the internal allocation of the existing DataGrid surface; it does not resize the containing table, right pane, tab, or window. Rows may grow to show wrapped content and the existing table scroll surface absorbs the additional content.

## v0.7.0 Protocol 07 progress

Protocol 07 is now the newest protocol and is selected by default. Protocol Progress follows the social-standing assay:

```text
Receive a social recommendation
  seed-specific transferable + nontransferable relationships
  A publishes bounded standing for B
Compare standing transfer rules
  provisional standing transfer
  no standing transfer baseline
  inherited-authority control
Judge social authority transfer
  nine falsification checks
  protocol verdict
```

Protocol 06 now appears in Run notes as a frozen Supported baseline. The graph remains incrementally updated, seed-scoped, display-bounded, and observer-only. No new protocol logic depends on graph state, selected seed, focus path, metric, legend visibility, table sizing, or render timing.


## v0.8.0 validation workbench

The Desktop Lab now treats seed-set identity as scientific metadata rather than a free-form convenience. The Run panel offers:

```text
Holdout v1 (20, frozen)
Development v1 (5, regression only)
Custom
```

Editing the seed text automatically reclassifies the selection unless it exactly matches a registered set. Holdout v1 is the default and all seven frozen protocols are selected so the ordinary v0.8 run is the validation matrix rather than another targeted mechanism-development run.

Every completed Desktop session writes `validation-report.json` and `validation-summary.md` in addition to the existing replication/session artifacts. The Protocol results assertion table includes a Category column so manipulation, mechanism outcome, safety boundary, and accounting checks remain visibly distinct during review.

No validation metadata is visible to experiment cognition. Seed-set labels, check categories, challenge-slice filters, diagnostic warnings, table layout, and graph state are evaluator/workbench concerns only.
