# Validation

Version 0.6.0 defines **18 self-tests**. Protocol-specific invariants include distinct seed-generated worlds and seed-101 implementation fixtures for Protocols 03, 04, 05, and 06.

## Required build validation

```powershell
dotnet restore Cpa.BoundedMindsLab.sln
dotnet build Cpa.BoundedMindsLab.sln -c Release
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --self-test
```

Expected result: all 18 self-tests pass with zero analyzer warnings/errors.

The source-generation environment used for v0.6.0 does not provide the .NET SDK. Build and self-test claims are deferred to the Windows development environment.

## Protocol 06 result set

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 06-incomplete-epistemic-ancestry `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-06-five-seed
```

Before interpretation confirm:

- all five seed directories exist;
- each seed contains result JSON, metrics CSV, manifest, and frame journal;
- `replication-report.json` contains five completed histories;
- each seed has a distinct Protocol 06 scenario fingerprint;
- every world contains at least three echo-trap and three independent-convergence contexts;
- missing-origin rate is >= 0.30;
- immediate-sender hint rate is >= 0.20;
- all paths consumed the same 98 report packets.

Then evaluate the eight preregistered checks without changing thresholds.

## Desktop validation

1. Launch the Desktop Lab and confirm the title reports `v0.6.0`.
2. Confirm Seeds defaults to `101, 211, 307, 401, 503` and Protocol 06 is selected by default.
3. Start a five-seed Protocol 06 session at Maximum pace.
4. Confirm the active-seed badge changes as histories advance and the graph Seed selector retains completed seed histories.
5. Confirm Protocol Progress exposes incomplete-ancestry generation, inferred/naive/oracle comparison, and eight-check evaluation.
6. Select a Focus path and verify Metric contains only values published by that path.
7. Confirm time-series metrics update incrementally during an active path rather than waiting for a phase/path boundary.
8. Confirm final scalar metrics with several paths use categorical bars, while multi-point metrics remain line plots.
9. Confirm axis guidance describes x/y meaning and whether higher/lower/context-dependent values are preferred.
10. Toggle individual legend entries and Show all/Hide all. Hidden-series state must not affect experiment output.
11. Maximize the graph and verify the selected seed, metric, focus path, visibility state, and axis guidance remain coherent.
12. In Timeline, Protocol results, assertion detail, Public mind state, and Trace surface tables, resize several column headers. The table/pane/window dimensions must remain fixed while internal column widths change.
13. Narrow a text-heavy table column and verify cell text wraps and rows grow/scroll rather than clipping the text or resizing the containing table.
14. Confirm dropped display frames, graph freeze, selection changes, resizing, and maximized graph use never alter durable artifacts.

## Full-suite checkpoint

After Protocol 06 is interpreted:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --replicate 101,211,307,401,503 `
  --output _artifacts/full-suite-0.6.0
```

This should reproduce the frozen Protocol 01-05 verdict families while adding Protocol 06 histories. Investigate any regression before proceeding.
