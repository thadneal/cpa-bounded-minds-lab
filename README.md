# CPA Bounded Minds Laboratory

Version **0.1.9**

This repository begins the successor experimental program to the completed CPA Cognitive Development Lab.

The earlier lab asked what useful organization could develop **inside one bounded organism** under recurrence, consequence, scarcity, and explicit cost. This laboratory moves the research boundary outward:

> What develops among bounded minds when they can exchange selected public evidence without collapsing their independent interiors?

The implementation is intentionally a new laboratory rather than a port of the previous .NET solution. The old `1.0.0` source was used as a final reference for experiment ergonomics, artifact discipline, run control, and visualization lessons. Protocol-local mechanisms from the old lab are not copied into the new cognitive architecture by default.

## Current experiment

`01-local-shared-memory-contamination`

The first protocol asks whether one mind can benefit from another mind's compressed public trace without granting second-hand history the same authority as direct local consequence.

Three receiver paths are compared:

- `local-only`: no transfer;
- `shared-provisional`: public traces remain explicitly foreign and begin with bounded provisional standing;
- `shared-lived-equivalent`: control condition that admits the same transfer with the source's accumulated authority and evidential inertia.

The synthetic world agrees with the source in six context cells and differs in two. A useful transfer mechanism should help early on the compatible cells, withdraw from locally contradictory cells, and remain cheaper than copying private developmental history.

See `docs/EXPERIMENTS.md` and `docs/FALSIFICATION.md` before interpreting results.

## Solution

```text
Cpa.BoundedMindsLab.sln
src/
  Cpa.BoundedMindsLab.Core/
  Cpa.BoundedMindsLab.Cli/
  Cpa.BoundedMindsLab.Desktop/
docs/
scripts/
```

The projects target .NET 10. The WPF desktop application targets `net10.0-windows`.

## Quick start

```powershell
dotnet build Cpa.BoundedMindsLab.sln -c Release
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --self-test
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --experiment 01-local-shared-memory-contamination --seed 101
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --all --replicate 101,211,307,401,503 --output _artifacts/protocol-01-five-seed
```

For live inspection on Windows 11:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

## Visualization performance boundary

The desktop application is rebuilt around a stricter observer boundary than the previous workbench.

Experiment execution runs on a dedicated below-normal-priority worker thread, rather than the WPF dispatcher or shared thread pool. Its display observer only enqueues frames into a bounded, non-blocking queue. A separate background projector converts those frames into a telemetry store. The WPF dispatcher never drains the experiment frame stream and never appends every observation directly into graph controls.

The UI samples projected telemetry at about 15 Hz, while graph rebuilds are independently capped and slow themselves further when projection backlog or render cost rises. Numeric series maintain multi-resolution min/max envelopes so graph snapshots scale primarily with available pixels rather than total historical point count. Fine-resolution live history is retired once it can no longer help a display; the complete history remains in the durable journal. Mouse hit testing searches only rendered points near the current x coordinate. Static graph geometry is cached and resize rebuilding is debounced.

The live workbench also provides previous/next controls beside the metric and focus-path selectors. Focus path is the primary graph selection, and the Metric selector is restricted to telemetry actually published by that path so sequential inspection does not walk through irrelevant empty graphs. The graph also provides full-name/details hover for legend keys, clickable per-line visibility, explicit markers for final-only one-point metrics such as `rmse`, a maximized live window, and a protocol progress strip that marks completed/current major steps and their receiver-path substeps. A Protocol results tab records completed per-seed judgments as Supported, Mixed, Refuted, or Inconclusive and exposes the underlying falsification checks directly from authoritative experiment results rather than display telemetry. The Desktop Lab accepts one or more seeds before a session begins and runs them sequentially on the same background worker. Live telemetry resets at each seed boundary so the graph remains a view of the current history rather than accidentally overlaying independent histories, while protocol judgments accumulate across the session. The output field points to the repository-level `_artifacts` root by default; each desktop session receives its own timestamped child directory with one `seed-N` subdirectory per history plus an aggregate replication report.

If display projection falls behind, display-only frames may be dropped. The durable `frames.ndjson` journal remains complete and is written independently by the core runner. UI backlog, dropped display frames, projector cost, rendered point count, and graph rebuild time are visible in the status bar.

This is an instrumentation rule, not a cognitive rule. Experiment state cannot read the graph, selected metric, splitter layout, frame backlog, render cadence, or dropped display count.

## Research stance

Carry forward the laws that survived prior pressure, including bounded causal execution, persistent local history, private interiors with compact public surfaces, revisable standing, explicit cost, and provenance distinct from agreement.

Do not assume that a successful synthetic controller is a permanent CPA organ. The laboratory exists to make those distinctions earn their way into later Rust synthesis.
