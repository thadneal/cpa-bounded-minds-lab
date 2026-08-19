# Validation

## Automated invariants

Version 0.3.1 defines **12 self-tests**. In addition to the existing deterministic-random, communication-cost, provenance/standing, Protocol 01, Protocol 02, and frame-sequence checks, the Protocol 03 baseline adds:

- `protocol-03-default-seeds-create-distinct-lived-histories` - all five canonical seeds must produce unique developmental-world fingerprints, with at least four distinct context-history layouts;
- `protocol-03-supports-seed-101` - implementation-drift fixture for the preregistered Protocol 03 mechanism.

The seed-101 protocol fixtures are invariant checks, not experimental evidence.

Run:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --self-test
```

## Protocol 03 result set

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 03-developmental-versus-doctrinal-transfer `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-03-five-seed
```

Before interpreting treatment outcomes, verify that the per-seed `scenario-generated` events differ in history-kind layout, evidence depth, and generated targets/noise. Do not change the seven falsification boundaries after viewing results.

## Desktop validation

On Windows 11 verify:

- the Seeds field opens with `101, 211, 307, 401, 503`;
- multiple seeds execute sequentially and the active-seed badge changes correctly;
- the Live metrics Seed selector accumulates each started seed and can move backward/forward through retained histories;
- selecting an earlier seed switches its graph, Focus path/Metric catalogs, detail tables, Timeline, and Protocol Progress without changing the active experiment;
- the maximized graph reports the graph-selected seed, even when a different seed is currently executing;
- Protocol progress switches correctly among Protocols 01, 02, and 03;
- Protocol 03 progress exposes its source-history and three receiver-path substeps;
- controls remain interactive during maximum-pace execution;
- pause/step/resume/cancel stop or release experiment observation boundaries rather than UI frames;
- graph freeze/maximize/metric selection/line hiding change presentation only;
- Focus path restricts Metric choices to telemetry actually published by that path;
- final-only one-point metrics such as `rmse` remain visible;
- dynamic legend height prevents overlap as key count changes;
- protocol judgments accumulate across seeds independently of display telemetry drops;
- seed visualization stores remain isolated rather than overlaying independent developmental histories;
- advancing to a new seed retains the completed seed for inspection and does not clip its final projected display frames;
- `frames.ndjson` remains complete even if the status bar reports dropped display-only frames.

## Environment note

The source-generation environment used for v0.3.1 does not provide the .NET SDK. Build and self-test claims are intentionally deferred to the Windows development environment.
