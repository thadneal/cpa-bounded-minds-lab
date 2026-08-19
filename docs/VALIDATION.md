# Validation

## Invariant suite

Run:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --self-test
```

Version 0.2.0 defines ten self-tests:

1. deterministic random shuffles repeat under equal seeds;
2. shared trace communication cost is explicit and additive;
3. provisional import remains foreign and capped;
4. lived-equivalent control imports local authority;
5. contradictory local consequence withdraws foreign standing;
6. confirming local consequence renews foreign standing;
7. public export requires earned source standing;
8. Protocol 01 supports its seed-101 synthetic fixture;
9. Protocol 02 supports its seed-101 synthetic fixture;
10. public frame sequence is contiguous.

The seed-101 protocol fixtures are invariant checks on implementation drift. They are not experimental results. Protocol 01 has already completed its accepted five-seed baseline; Protocol 02 still requires the preregistered five-seed result set.

## Protocol 02 result set

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- \
  --experiment 02-peer-disagreement-preserved-interiors \
  --replicate 101,211,307,401,503 \
  --output _artifacts/protocol-02-five-seed
```

Review `replication-report.json` and each seed's `manifest.json` before changing any thresholds. After interpreting the targeted Protocol 02 result, run the two-protocol five-seed full suite as the v0.2.0 checkpoint.

## Desktop validation

On Windows 11, run a maximum-pace history and verify:

- controls remain interactive while observations are arriving;
- pause stops at an observation boundary;
- step releases one boundary;
- resume continues the same history;
- cancel ends cleanly without an unhandled UI exception;
- freezing the graph does not pause experiment execution;
- changing metric/path affects only presentation;
- Focus path lists only histories with numeric telemetry, and Metric lists only metrics actually published by the selected focus path, including while new paths and metrics appear during a run;
- display frame drops, if any, do not create gaps in `frames.ndjson`;
- graph point count remains bounded by display resolution rather than raw history size;
- hover stays responsive on the longest available metric series;
- clicking a legend key hides only that line, marks the key as hidden, rescales axes from the remaining visible lines, and clicking it again restores the line;
- legend visibility remains synchronized between embedded and maximized views, while metric-specific hidden state does not leak into a different metric;
- legend height grows as key count/available width changes, with no key rows overlapping the graph surface;
- selecting a final-only metric such as `rmse` after the run displays visible point markers rather than an apparently empty graph;
- Protocol results accumulates one judged row per completed seed/protocol, maps core `Disconfirm` to the UI label `Refuted`, and exposes the experiment's falsification assertions without depending on display telemetry;
- the seed indicator above Protocol progress clearly shows the active seed value and `current/total` position while a session is running;
- Protocol progress switches from the Protocol 01 source/receiver steps to the Protocol 02 peer-development/preserved-versus-synchronized steps when the active experiment changes;
- a multi-seed desktop session runs seeds in the requested order, resets live visualization at each seed boundary, retains earlier seed judgments in Protocol results, writes one `seed-N` directory per history, and produces `replication-report.json` only after the complete session succeeds.

## Environment note

The source-generation environment used for v0.2.0 does not provide the .NET SDK, so build/self-test claims are intentionally deferred to the Windows development environment.
