# Validation

## Automated invariants

Version 0.5.2 retains **16 self-tests**. Current protocol-specific checks include distinct seed-generated worlds and seed-101 implementation fixtures for Protocols 03, 04, and 05.

Protocol 05 adds:

- `protocol-05-default-seeds-create-distinct-coordination-worlds`;
- `protocol-05-supports-seed-101`.

The seed-101 fixtures are implementation invariants, not experimental evidence.

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --self-test
```

## Protocol 05 result set

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 05-emergent-convention-artificial-culture `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-05-five-seed
```

Before interpreting outcomes, verify unique scenario fingerprints and materially different changed-context layouts. Do not alter the seven preregistered boundaries after seeing results.

## Desktop validation

On Windows 11 verify:

- main and maximized graph titles include the running application version;
- Seeds opens with `101, 211, 307, 401, 503`;
- seed-scoped graph inspection still switches graph, path/metric catalogs, details, Timeline, and Protocol Progress;
- Protocol Progress recognizes Protocols 01 through 05;
- Protocol 05 progress exposes convention formation, earned/fresh/frozen comparison, regime shift, and evaluation;
- graph freeze/maximize/metric selection/line hiding remain presentation-only;
- Focus path restricts Metric choices to data that path publishes;
- final-only one-point metrics remain visible;
- protocol judgments accumulate independently of display telemetry drops;
- `frames.ndjson` remains complete even if display-only frames are dropped.

## Environment note

The source-generation environment used for v0.5.2 does not provide the .NET SDK. Build and self-test claims are deferred to the Windows development environment.


### Desktop scalar comparison rendering (v0.5.1)

After a Protocol 05 run, select `communication_packet_count`. With multiple treatment paths present, the graph should render one categorical bar per visible path rather than isolated points on a tick axis. Toggle a legend entry and confirm the bar and y-scale update. Multi-point metrics must continue to render as time-series lines.


### Desktop metric guidance and boundary-batched rendering (v0.5.2)

On a multi-path metric, verify that the graph header identifies x-axis meaning, y-value meaning, and preferred direction/context. Check at least `rmse`, `communication_work`, `mean_utility`, `standing`, and a disagreement metric.

Use `Hide all`, confirm every current-series line/bar disappears while legend keys remain available, then use `Show all` to restore them. Repeat in the maximized graph and confirm visibility state stays synchronized.

At `Maximum` pace, observe a multi-point metric while an experiment is running. Numeric publication/projection counters should continue advancing, but the graph should rebuild at completed series/phase boundaries rather than on every projected sample. When one path finishes and the next begins, the completed path may appear while the new path remains absent until its own commit boundary. The experiment thread, durable `frames.ndjson`, Timeline, and Protocol results must continue independently.
