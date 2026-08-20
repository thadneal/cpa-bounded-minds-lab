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

Treat analyzer failures as build failures. Warnings are errors. Version 0.8.0 defines **23 invariant/regression tests**. `scripts/verify.ps1` and `scripts/verify.sh` first verify the frozen Protocol 01-07 source hashes before building.

## Development regression

The old five-seed matrix is now explicitly development data:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --replicate 101,211,307,401,503 `
  --output _artifacts/development-v1-regression
```

Use this only to detect regressions against the frozen mechanism-discovery record. Do not treat another 5/5 result as fresh evidence.

## Holdout validation

Run the complete frozen catalog across holdout-v1 without changing mechanisms or falsification boundaries:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --validation `
  --output _artifacts/validation-holdout-v1
```

Expected root artifacts include:

```text
replication-report.json
validation-report.json
validation-summary.md
session-manifest.json
seed-809/
...
seed-8089/
```

Interpret the validation report before changing code. In particular, inspect `mechanism-outcome` and `safety-boundary` failures separately from manipulation/accounting checks, and inspect the preregistered challenge slices. A perfect all-Support result is a reason to review assay sensitivity, not a reason to silently strengthen the claim.

If holdout-v1 causes a mechanism or threshold change, do not rerun the revised mechanism and call holdout-v1 fresh confirmation. Register a new holdout set first.

## Desktop Lab

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The main window title includes the running assembly version. The Run panel defaults to **Holdout v1 (20, frozen)** and all seven protocols selected. The Seed set control can switch to **Development v1 (5, regression only)** or Custom. Editing the seed text automatically marks the set as Custom unless it exactly matches one registered set.

The graph Seed selector can revisit completed histories. Focus path scopes Metric choices. Legend keys remain individually clickable and Show all/Hide all acts on the current metric. Graph telemetry updates incrementally at the adaptive display cadence; experiment execution remains isolated from WPF.

Protocol result assertion detail now includes the validation category (`manipulation`, `mechanism-outcome`, `safety-boundary`, or `accounting-constraint`). Data-grid cells wrap text and headers expose resize grippers; resizing columns redistributes width inside the existing table surface.

## Cancellation and stepping

Pause requests stop at the next observation boundary. `Step observation` releases one boundary. Resume continues the same history. Cancel is cooperative and should retain already-written artifacts. A debugger configured to break on thrown `OperationCanceledException` may stop at the throw site even when the application subsequently handles cancellation normally.
