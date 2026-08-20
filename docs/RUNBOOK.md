# Runbook

## Prerequisites

- Windows 11 for the WPF Desktop Lab;
- .NET SDK pinned by `global.json`;
- repository working directory at the solution root.

## Build and invariants

```powershell
dotnet restore Cpa.BoundedMindsLab.sln
dotnet build Cpa.BoundedMindsLab.sln -c Release
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --self-test
```

Treat analyzer failures as build failures. Warnings are errors. Version 0.6.0 defines **18 invariant tests**.

## Targeted Protocol 06 check

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 06-incomplete-epistemic-ancestry `
  --seed 101 `
  --output _artifacts/protocol-06-seed-101
```

Then run the canonical matrix without changing thresholds:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 06-incomplete-epistemic-ancestry `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-06-five-seed
```

Inspect these together before interpreting the verdict:

- `missing_origin_rate` and `immediate_sender_hint_rate`;
- `inferred_echo_trap_rmse` versus `naive_echo_trap_rmse`;
- `inferred_rmse` versus `naive_rmse` and `oracle_rmse`;
- `inferred_independent_rmse` versus `naive_independent_rmse`;
- `inferred_echo_pair_recall`;
- `inferred_false_merge_rate`;
- packet count and communication work.

Do not tune the signature merge radius or falsification thresholds after seeing the five-seed matrix. Interpret the result first.

## Full-suite checkpoint

After Protocol 06 is interpreted, run all six protocols across the canonical matrix:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --replicate 101,211,307,401,503 `
  --output _artifacts/full-suite-0.6.0
```

This checkpoint is useful because v0.6.0 adds a new environment/experiment, two invariants, and protocol-aware Desktop progress. Earlier protocol mechanics remain frozen.

## Desktop Lab

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The main window title includes the running assembly version. Seeds default to `101, 211, 307, 401, 503`. Protocol 06 is selected by default.

Protocol 06 progress should move through:

```text
Generate incomplete ancestry
Compare corroboration rules
Judge ancestry discrimination
```

The graph Seed selector can revisit completed seed histories. Focus path scopes Metric choices. Legend keys remain individually clickable and Show all/Hide all acts on the current metric.

Graph telemetry again updates incrementally at the adaptive display cadence. This restores the pre-v0.5.2 depiction behavior after boundary-batched rendering showed no noticeable UI benefit. The experiment worker remains isolated from WPF regardless of graph cadence.

Data-grid cells wrap text. Column headers expose resize grippers. Resizing columns must redistribute width within the existing table surface rather than resize the containing pane or window.

## Cancellation and stepping

Pause requests stop at the next observation boundary. `Step observation` releases one boundary. Resume continues the same history. Cancel is cooperative and should retain already-written artifacts. A debugger configured to break on thrown `OperationCanceledException` may stop at the throw site even when the application subsequently handles cancellation normally.
