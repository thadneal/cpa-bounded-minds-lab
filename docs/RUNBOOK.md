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

Treat analyzer failures as build failures. Warnings are errors. Version 0.10.0 defines **30 invariant/regression tests**. `scripts/verify.ps1` and `scripts/verify.sh` first verify the frozen Protocol 01-07 source hashes before building.

## Development regression

The old five-seed matrix is now explicitly development data:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --replicate 101,211,307,401,503 `
  --output _artifacts/development-v1-regression
```

Use this only to detect regressions against the frozen mechanism-discovery record. Do not treat another 5/5 result as fresh evidence.

## Parameterized falsification (current research run)

After build/self-test and frozen-hash verification:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --falsify `
  --output _artifacts/parameterized-falsification-v1
```

The run produces:

```text
parameterized-plan.json
parameterized-report.json
parameterized-summary.md
p03-history-informativeness.csv
p04-equal-budget-comparator.csv
p05-volatility-surface.csv
p06-ancestry-opacity.csv
p07-reliability-prevalence.csv
p07-reliability-severity.csv
```

Run this phase without changing the registered profiles after inspecting outcomes. Negative margins are expected and useful.

## Consumed challenge-v1 reproducibility

`challenge-v1` is consumed exploratory evidence. Reproduce it only when checking determinism or artifacts:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --challenge `
  --output _artifacts/challenge-v1-repro
```

Do not describe a reproduction as fresh validation.

## Consumed holdout reproducibility

`holdout-v1` was consumed on 2026-08-20. Rerun it only to reproduce the frozen v0.8 result:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --validation `
  --output _artifacts/validation-holdout-v1-repro
```

A rerun is not fresh validation. If a mechanism is changed after seeing holdout-v1, a future confirmation claim requires a new registered holdout.

## Desktop Lab

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The main window title includes the running assembly version. The Run panel now defaults to **Development v1 (5, regression only)** and all seven protocols selected, so opening the visualization does not encourage accidental reuse of consumed holdout-v1 as fresh evidence. The Seed set control still exposes **Holdout v1 (20, consumed / reproducibility only)** and Custom. Editing the seed text automatically marks the set as Custom unless it exactly matches one registered set.

The graph Seed selector can revisit completed histories. Focus path scopes Metric choices. Legend keys remain individually clickable and Show all/Hide all acts on the current metric. Graph telemetry updates incrementally at the adaptive display cadence; experiment execution remains isolated from WPF.

Protocol result assertion detail now includes the validation category (`manipulation`, `mechanism-outcome`, `safety-boundary`, or `accounting-constraint`). Data-grid cells wrap text and headers expose resize grippers; resizing columns redistributes width inside the existing table surface.

## Cancellation and stepping

Pause requests stop at the next observation boundary. `Step observation` releases one boundary. Resume continues the same history. Cancel is cooperative and should retain already-written artifacts. A debugger configured to break on thrown `OperationCanceledException` may stop at the throw site even when the application subsequently handles cancellation normally.
