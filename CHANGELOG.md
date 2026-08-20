# Changelog

## 0.6.0 - 2026-08-19

- Froze Protocol 05 as Supported after the canonical five-seed run returned 5/5 Support and 35/35 preregistered checks. Earned convention retained essentially all fresh-negotiation utility while reducing communication work by about 76%, revised changed contexts, retained stable contexts, and outperformed the frozen culture after the world changed.
- Added Protocol 06, `06-incomplete-epistemic-ancestry`, asking whether a bounded receiver can distinguish independent convergence from echoed ancestry when public provenance is missing, partial, or stops at the immediate sender.
- Added `EpistemicAncestryWorld`, a seed-generated fourteen-context/seven-peer family with echo traps, independent convergence, mixed lineages, ambiguous lineages, incomplete origin hints, evidence-depth variation, and imperfect developmental-signature copying.
- Added ancestry-inferred, naive-agreement, and perfect-ancestry oracle treatments. The ordinary path groups likely shared roots from incomplete public hints/signatures and caps corroborative support per inferred lineage; the oracle remains evaluator-only calibration.
- Added eight preregistered Protocol 06 checks covering incomplete provenance, echo discounting, whole-history error, preservation of independent convergence, true-echo recovery, false merging, oracle proximity, and equal bounded communication.
- Added two Protocol 06 self-tests, bringing the invariant suite to 18 checks, including canonical-seed ancestry-world diversity and a seed-101 implementation-drift fixture.
- Reverted the v0.5.2 boundary-batched graph experiment after it produced no noticeable responsiveness gain. Graph snapshots again include the latest accumulated numeric points and update incrementally at the existing adaptive display cadence; experiment execution remains isolated from WPF.
- Added wrapping text to DataGrid text cells and restored functional WPF column resize grippers in the dark header template. Column resizing redistributes width inside the existing table surface without resizing the containing pane/window.
- Added Protocol 06 progress/run-note visualization and metric guidance for ancestry recall, false merges, and effective support-group counts.
- Updated plan, experiments, falsification, architecture, source reconciliation, runbook, validation, README, artifacts, CLI, and assembly versioning for v0.6.0.

## 0.5.2 - 2026-08-19

- Added metric guidance directly to the graph surface: the y-value meaning, x-axis interpretation, and preferred direction/context are shown for every selected metric. Scalar comparison graphs identify the x-axis as treatment/focus path, while time-series graphs identify it as observation/tick order.
- Added `Show all` and `Hide all` controls for the current metric's plotted series in both the embedded and maximized graph views. Existing per-key click visibility remains available.
- Changed live plot rebuilding from incremental sample-driven refresh to boundary-batched depiction. Numeric samples continue to accumulate in the background telemetry store, but a series is exposed to the renderer only after a series transition, phase/developmental boundary, or terminal experiment/run boundary commits the accumulated values.
- Added committed-through tracking to the bounded multi-resolution telemetry series so an in-progress next series cannot leak partial points into a graph whose previous series has just committed.
- Kept selector catalogs, details, timeline, protocol results, durable NDJSON output, and experiment execution independent from the new render batching. No Protocol 01-05 mechanics, thresholds, or scientific telemetry were changed.

## 0.5.1 - 2026-08-19

- Added topology-aware graph rendering for terminal scalar comparison metrics. When two or more plotted paths each publish exactly one value, the Desktop Lab now renders a categorical bar comparison instead of forcing the values onto a time-series line plot.
- Final scalar comparisons ignore incidental completion-tick differences on the x-axis, so metrics such as `communication_packet_count` and final `rmse` compare treatment magnitude directly.
- Preserved clickable legend visibility, dynamic legend sizing, maximized-graph behavior, and bounded rendering. Hidden series are removed from the comparison scale exactly as they are from time-series scaling.
- Added bar hover details and compact treatment labels while keeping the full path available through the existing legend hover surface.
- Kept single-series one-point metrics on the existing point renderer so live time-series metrics do not switch graph type after their first observation.
- No Protocol 01-05 experiment behavior, thresholds, telemetry values, or durable artifacts changed.

## 0.5.0 - 2026-08-19

- Froze Protocol 04 as Supported after the canonical five-seed run returned 5/5 Support and 35/35 preregistered checks. Mean total RMSE was about `0.04915` for typed communication versus `0.05341` for early semantic smoothing, with useful dissent preserved, low-quality dissent kept proportionate, and later shared consequence restoring convergence.
- Added Protocol 05, `05-emergent-convention-artificial-culture`, asking whether repeated bounded coordination can create a useful distributed convention without introducing a central culture owner.
- Added `EmergentConventionWorld`, a seed-generated twelve-context, three-peer coordination family where two actions are initially viable, private preferences often differ, and `4..6` contexts later change cost regime.
- Added earned-convention, fresh-negotiation, and frozen-convention treatments with explicit communication cost. The earned path can replace three fresh preference packets with one compact convention invocation after standing is earned.
- Added seven preregistered Protocol 05 checks covering world plurality, convention formation, communication compression, utility, revision after changed conditions, frozen-culture failure, and stable-convention retention.
- Added two Protocol 05 self-tests, bringing the invariant suite to 16 checks, including canonical-seed coordination-world diversity and a seed-101 implementation-drift fixture.
- Added the running Desktop assembly version to the main visualization and maximized graph window titles.
- Updated Desktop Protocol Progress and Run notes for Protocol 05. The newest protocol remains the default selection and the canonical five seeds remain the default session value.
- Updated plan, experiment, falsification, architecture, source-reconciliation, runbook, validation, README, artifact, CLI, and assembly versioning for v0.5.0.

## 0.4.0 - 2026-08-19

- Froze Protocol 03 as Supported after the canonical five-seed run returned 5/5 Support and 35/35 preregistered checks. The accepted record notes that developmental transfer consistently improved calibration over doctrinal transfer while remaining only modestly different from local-only learning overall.
- Added Protocol 04, `04-bounded-communication-before-language`, testing low-dimensional typed public signals against an early semantic-smoothing control while preserving private interiors in both conditions.
- Added `CommunicationBeforeLanguageWorld`, a seed-generated twelve-context/three-peer world with varied informative dissent, misleading dissent, complementary histories, convergent histories, evidence depth, target landscape, noise, salient peer assignment, and encounter order.
- Added explicit Protocol 04 communication cost: three typed public postures per shared observation versus the same initial postures plus two paid three-message smoothing rounds in the control.
- Added seven preregistered Protocol 04 checks covering social-world heterogeneity, retained public disagreement, informative dissent, misleading dissent, whole-history error, convergence under shared consequence, and bounded communication.
- Added two Protocol 04 self-tests, bringing the invariant suite to 14 checks, including canonical-seed social-world diversity and a seed-101 implementation-drift fixture.
- Updated Desktop Protocol Progress and Run notes for Protocol 04. The existing per-seed graph selector, judged results, timeline, metric filtering, and visualization isolation remain unchanged.
- Updated plan, experiment, falsification, architecture, source-reconciliation, runbook, validation, README, artifact, CLI, and assembly versioning for v0.4.0.

## 0.3.1 - 2026-08-19

- Added a graph Seed selector with previous/next navigation beside Metric and Focus path so completed multi-seed histories can be inspected independently.
- Retained each seed's bounded display telemetry store for the duration of the Desktop Lab session instead of discarding prior seed visualization state when the next seed begins.
- Seed selection now scopes the graph, Focus path/Metric catalogs, public-state detail tables, Timeline, and Protocol Progress to the chosen history.
- Live visualization follows the active seed by default. Manually selecting another seed stops auto-follow so an earlier history can remain under inspection while later seeds continue running.
- The maximized graph now reports the seed actually selected for graph inspection rather than always reporting the currently executing seed.
- Seed completion and seed-store rotation drain queued display frames before a history is considered ready for inspection, preserving the UI's expendable/bounded telemetry contract without clipping the tail of the final or an intermediate seed.
- Kept graph rendering and experiment execution isolated: archived stores are read-only after rotation and the experiment still never reads visualization selection or rendering state.

## 0.3.0 - 2026-08-19

- Froze Protocol 02 as a 5/5 Supported result while recording its replication qualification: the five histories mostly varied encounter order and noise rather than developmental circumstance.
- Changed the replication methodology for Protocol 03 and later work. A seed may now generate a different member of a preregistered developmental-world family, varying context-history type, target landscape, evidence depth, observation noise, regime transition, and encounter order while keeping treatment paths within a seed controlled.
- Added `ExperimentDefaults.ReplicationSeeds` for the canonical `101,211,307,401,503` matrix and added an invariant requiring Protocol 03 to produce five unique world fingerprints plus materially different context-history layouts.
- Added Protocol 03, `03-developmental-versus-doctrinal-transfer`, comparing local-only development, bounded consequence-history transfer, and a cheaper final-rule doctrinal control.
- Added seed-specific stable-compatible, stable-divergent, unstable-transition, and sparse-ambiguous source histories. Evaluator labels remain unavailable to receiver cognition.
- Added seven preregistered Protocol 03 falsification checks covering world heterogeneity, history-calibrated standing, stable-history transfer benefit, unstable-history contamination, whole-history noninferiority, receiver-consequence sovereignty, and bounded communication cost.
- Added two Protocol 03 self-tests, bringing the invariant suite to 12 checks.
- Changed the Desktop Lab Seeds default to `101, 211, 307, 401, 503`.
- Added Protocol 03 major-step/substep visualization to the existing Protocol Progress surface while retaining active-seed indication and observer-only UI semantics.
- Updated artifact, CLI, assembly, documentation, runbook, validation, and research-ledger versioning to 0.3.0.
- Protocols 01 and 02 remain byte-for-behavior frozen; their accepted historical seed semantics and thresholds are not retroactively changed.

## 0.2.0 - 2026-08-19

- froze Protocol 01 as the accepted five-seed Supported baseline without changing its mechanics or thresholds;
- added Protocol 02, `02-peer-disagreement-preserved-interiors`, comparing preserved private histories with an explicitly invasive synchronized-state control;
- added the complementary/noisy `PeerDisagreementWorld`, compact public prediction-and-standing exchange, shared-consequence standing revision, and six preregistered Protocol 02 falsification checks;
- added a Protocol 02 seed-101 invariant fixture and expanded the self-test suite to ten checks;
- made the Desktop Lab Protocol progress surface aware of both protocols and able to switch step/substep labels as experiments advance within a seed;
- added a persistent active-seed badge showing the actual seed and current/total position during multi-seed visualization sessions;
- mirrored the active seed into the maximized graph window and retained the last displayed seed after completion so saved/inspected graphs remain attributable;
- made the newest protocol the default Desktop selection, leaving frozen earlier protocols available for explicit full-suite runs;
- updated runbook, experiments, falsification, architecture, validation, research ledger, and quick-start documentation for the new research step.

## 0.1.9 - 2026-08-19

- Added a Protocol results tab beside Timeline. Each completed seed/protocol is judged as `Supported`, `Mixed`, `Refuted`, or `Inconclusive`, with passed/failed falsification-check counts, interpretation, and the full assertion table for the selected result.
- Kept protocol judgments on a coordinator-owned session result surface sourced directly from completed `ExperimentResult` objects instead of expendable display telemetry. Multi-seed sessions therefore retain earlier seed judgments while live graph/state telemetry resets between seeds.
- Added running per-protocol verdict counts across completed seeds without inventing a new aggregate verdict beyond the experiment's existing per-history judgments. The core `Disconfirm` enum remains unchanged for artifact/schema continuity and is displayed as `Refuted` in the Desktop Lab.
- Fixed final-only metrics such as `rmse` appearing blank after a run. These metrics legitimately publish a single point per path; the custom graph now renders an explicit marker for one-point series instead of relying on a zero-segment line geometry.
- Centered single-x observations by symmetrically expanding a degenerate x-axis, making final result markers visible and inspectable rather than pinning them against the plot edge.
- Added Desktop validation coverage for judged protocol results and one-point final metrics.
- No Protocol 01 behavior, falsification thresholds, communication mechanics, durable result values, or experiment execution semantics changed.

## 0.1.8 - 2026-08-19

- Added clickable live-graph legend keys. Each series can be hidden or restored without changing experiment execution or telemetry, and hidden state is scoped by metric so visibility choices do not leak into unrelated graphs.
- Recomputed graph axes from visible series only, making selective isolation useful rather than leaving hidden lines in the scale calculation. If every line is hidden, the graph leaves the legend active and explains how to restore a series.
- Made hidden legend entries visually distinct with muted text and a dashed key while preserving legend hover details and adding explicit visible/show/hide guidance.
- Replaced the fixed two-column legend spacing assumption with a width-aware one-to-three-column layout whose reserved height grows from the actual row count, preventing changing legend populations from overlapping the plot.
- Synchronized series visibility between the embedded and maximized graph views while avoiding routine rebuild work on the inactive embedded graph.
- Aligned assembly, CLI, and artifact version metadata with the repository version at 0.1.8.
- No Protocol 01 behavior, telemetry values, durable scientific artifacts, thresholds, or execution semantics changed.

## 0.1.7 - 2026-08-19

- Reversed the live selector dependency to match inspection intent: Focus path is now the primary selection, and Metric lists only numeric telemetry actually published by that path.
- Limited Focus path choices to projected histories that contain at least one numeric metric, preventing navigation into event-only paths that cannot produce a graph.
- Preserved the current metric when switching paths if that metric exists on the new path; otherwise the UI prefers `rolling_rmse` and falls back to the first available metric.
- Kept previous/next navigation aligned with the filtered Metric list so stepping metrics cannot intentionally land on an empty graph for the selected focus path.
- No Protocol 01 behavior, thresholds, telemetry contents, durable artifacts, or experiment execution semantics changed.

## 0.1.6 - 2026-08-19

- Added multi-seed Desktop Lab sessions. The Seeds field accepts comma, whitespace, semicolon, or newline separated unsigned seeds, removes duplicates while preserving order, and runs each seed sequentially on the dedicated experiment worker.
- Reset live telemetry and protocol visualization at each seed boundary so independent histories are inspected one at a time rather than merged into misleading graph series.
- Added session progress showing the active seed, its position in the planned matrix, and the number of completed histories.
- Changed desktop artifact layout to a timestamped session root with one `seed-N` directory per history, a durable `session-manifest.json`, and a completed-session `replication-report.json`.
- Reused the core replication aggregation path for Desktop and CLI reporting so verdict counts and mean metrics follow the same representation.
- Preserved pause/step/resume/cancel semantics across seed transitions and retained partial per-seed artifacts when a session is interrupted.
- No Protocol 01 cognitive behavior, thresholds, standing rules, or communication semantics changed.

## 0.1.5 - 2026-08-19

- Fixed the Desktop maximized-plot build failure by importing the shared `WindowsDarkMode` service namespace used by `MetricPlotWindow`.
- Filtered Focus path choices by the currently selected Metric. A path now appears only when its projected history actually contains that metric, and previous/next navigation walks the same relevant subset.
- Made metric-to-path availability invalidate the live selector catalog when a later path begins publishing a metric that was already known globally.
- Preserved the current focus path across metric changes when it remains applicable; otherwise the UI prefers the provisional-sharing path when available and falls back to the first relevant path.
- No protocol behavior, result thresholds, telemetry contents, or experiment execution semantics changed.

## 0.1.4 - 2026-08-19

- Added previous/next navigation buttons beside both live Metric and Focus path selectors so catalog options can be inspected sequentially without reopening dropdowns.
- Added throttled legend-key hover details to the custom graph surface, including the full untruncated series name, internal series key, and rendered point count while retaining the low-allocation custom drawing path.
- Added a maximized live graph window. While it is open, live graph snapshots/rebuilds target the maximized plot instead of rebuilding both the main and enlarged graphs.
- Added a compact Protocol 01 progress strip above the center workbench, marking completed/current major steps and the source, transfer-path, and evaluation substeps from projected structural timeline events.
- Changed the Desktop Lab output field default to the repository-level `_artifacts` directory and placed each desktop run in a unique timestamped child directory. `Open output` now prefers the most recent run.
- Replaced the stock GroupBox chrome with a single restrained dark border/header template matching the table-edge weight more closely.
- Preserved experiment behavior, Protocol 01 thresholds, standing semantics, and the observer-only visualization boundary.

## 0.1.3 - 2026-08-19

- Corrected inconsistent Desktop Lab theming observed on Windows, where the navigation surface, native combo boxes, disabled buttons, tab headers, and DataGrid headers could fall back to light system chrome.
- Moved the WPF palette and control templates into a dedicated `Themes/LabTheme.xaml` resource dictionary.
- Added application-scoped system brush overrides plus explicit dark templates for buttons, text boxes, combo boxes, check boxes, tabs, DataGrid headers/cells, and scrollbars.
- Explicitly anchored the main window and left navigation surface to the dark laboratory background so framework fallback brushes cannot expose a white panel.
- No experiment behavior, thresholds, telemetry semantics, graphing architecture, or durable artifacts changed.

## 0.1.2 - 2026-08-19

- Fixed the remaining first-build Desktop Lab compiler/analyzer failures after Core and CLI successfully rebuilt.
- Added an explicit `System.IO` import for output-path normalization in `DesktopRunCoordinator`.
- Replaced LINQ-based fallback selection over the indexable telemetry series catalog with direct indexed access to satisfy CA1826.
- No experiment behavior, thresholds, telemetry semantics, or visualization architecture changed.

## 0.1.1 - 2026-08-19

- Fixed initial .NET 10 analyzer build failures under warnings-as-errors.
- Replaced explicit negative argument validation with the .NET throw helper.
- Made the fixed Protocol 01 synthetic world API static and tightened concrete internal return types where requested by performance analysis.
- Tightened self-test helper parameter typing to its concrete collection type.
- No protocol behavior, preregistered thresholds, telemetry semantics, or Desktop Lab architecture changed.

## 0.1.0 - 2026-08-19

Founding release of the CPA Bounded Minds Laboratory.

- created a new .NET 10 solution instead of extending or porting the frozen CPA Cognitive Development Lab;
- established the successor research boundary around development among bounded minds with persistent private histories and selected public evidence exchange;
- added Protocol 01, `local-shared-memory-contamination`, comparing local-only development, provenance-bounded provisional transfer, and a lived-equivalent transfer control;
- added compact public trace packets, an explicit-cost shared trace channel, direct and foreign trace standing, and consequence-driven revision without exposing sender private state;
- added preregistered component assertions for compatible transfer benefit, bounded contamination, late local revision, provenance selectivity, overall usefulness, and communication cost;
- added deterministic single-history and multi-seed CLI execution, complete NDJSON frame journals, result artifacts, replication aggregation, and nine invariant self-tests;
- recreated the WPF laboratory as an observer-first visualization application rather than copying the former workbench implementation;
- moved experiment execution to a dedicated below-normal-priority thread, and moved display frame projection off the WPF dispatcher into a bounded non-blocking queue and background projector;
- added a sampled telemetry store with bounded/retiring fine-resolution history, multi-resolution numeric envelopes, adaptive graph refresh, cached stream geometry, binary-neighborhood hover lookup, resize debounce, and an explicit graph-freeze control that never pauses the experiment;
- surfaced display backlog, dropped presentation frames, projector time, rendered point count, and graph rebuild time so visualization cost can be distinguished from experiment behavior;
- documented the source reconciliation, new experimental plan, falsification boundaries, desktop performance architecture, validation workflow, and research ledger.

No experimental result is claimed at packaging time because the .NET 10 SDK/compiler is not available in the source-generation environment. Run the invariant suite and the five-seed Protocol-01 matrix before accepting the first result.
