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

Treat analyzer failures as build failures. Warnings are errors. Version 0.12.0 defines **35 invariant/regression tests**. `scripts/verify.ps1` and `scripts/verify.sh` first verify the frozen Protocol 01-08 source hashes before building.

## Development regression

The old five-seed matrix is now explicitly development data:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --replicate 101,211,307,401,503 `
  --output _artifacts/development-v1-regression
```

Use this only to detect regressions against the frozen mechanism-discovery record. Do not treat another 5/5 result as fresh evidence.

## Protocol 08 fresh holdout (current research run)

Protocol 08 development-v1 is complete and its exact experiment/world sources are frozen. The next authoritative evidence is the preregistered twenty-seed `p08-holdout-v1` set:

```powershell
./scripts/verify.ps1

dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-validation `
  --output _artifacts/p08-holdout-v1
```

The **first execution consumes this holdout**. Preserve the artifact before changing Protocol 08 or inspecting controlled falsification results. A later rerun is reproducibility only.

## Protocol 08 controlled falsification

After the fresh holdout is preserved and interpreted, map the operating envelope:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-falsify `
  --output _artifacts/strategic-influence-falsification-v1
```

This runs five 7 x 7 surfaces with seven replicates per cell. Negative margins are expected boundary evidence. Do not tune the receiver and then reuse these surfaces as confirmation.

## Consumed parameterized-falsification-v1 reproducibility

`parameterized-falsification-v1` is now consumed exploratory evidence. Reproduce it only for determinism, artifact, or implementation checks:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --falsify `
  --output _artifacts/parameterized-falsification-v1-repro
```

The historical run produced `parameterized-plan.json`, `parameterized-report.json`, `parameterized-summary.md`, and six profile CSV surfaces. Do not describe a reproduction as validation of a revised mechanism.

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

The main window title includes the running assembly version. The Run panel now defaults to **Development v1 (5, regression only)** and all eight protocols selected, so opening the visualization does not encourage accidental reuse of consumed holdout-v1 as fresh evidence. The Seed set control still exposes **Holdout v1 (20, consumed / reproducibility only)** and Custom. Editing the seed text automatically marks the set as Custom unless it exactly matches one registered set.

The graph Seed selector can revisit completed histories. Focus path scopes Metric choices. Legend keys remain individually clickable and Show all/Hide all acts on the current metric. Graph telemetry updates incrementally at the adaptive display cadence; experiment execution remains isolated from WPF.

Protocol result assertion detail now includes the validation category (`manipulation`, `mechanism-outcome`, `safety-boundary`, or `accounting-constraint`). Data-grid cells wrap text and headers expose resize grippers; resizing columns redistributes width inside the existing table surface.

## Cancellation and stepping

Pause requests stop at the next observation boundary. `Step observation` releases one boundary. Resume continues the same history. Cancel is cooperative and should retain already-written artifacts. A debugger configured to break on thrown `OperationCanceledException` may stop at the throw site even when the application subsequently handles cancellation normally.
