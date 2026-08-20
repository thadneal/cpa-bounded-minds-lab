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

Treat analyzer failures as build failures. Warnings are errors. Version 0.9.0 defines **26 invariant/regression tests**. `scripts/verify.ps1` and `scripts/verify.sh` first verify the frozen Protocol 01-07 source hashes before building.

## Development regression

The old five-seed matrix is now explicitly development data:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --replicate 101,211,307,401,503 `
  --output _artifacts/development-v1-regression
```

Use this only to detect regressions against the frozen mechanism-discovery record. Do not treat another 5/5 result as fresh evidence.

## Operating-envelope challenge

The authoritative v0.9 experiment is `challenge-v1`:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --challenge `
  --output _artifacts/challenge-v1
```

The harness first writes `challenge-plan.json`, which records outcome-blind seed selection from frozen world descriptors. It then runs 100 selected cases: five Protocol 03-07 profiles x five stress bands x four seeds.

Expected root artifacts include:

```text
challenge-plan.json
challenge-report.json
challenge-summary.md
p03-source-instability/
p04-conflict-density/
p05-regime-shift/
p06-ancestry-visibility/
p07-recommender-fragility/
```

Read boundary margins by stress band before changing any mechanism. Mixed/Disconfirm outcomes and negative margins are useful operating-envelope evidence. If the extreme band remains entirely positive, do not simply mine more seeds indefinitely; the next challenge should parameterize the world beyond the frozen generator support.

Protocol 04's challenge-v1 profile still compares typed communication with the original semantic-smoothing control. The stronger equal-budget alternative remains unresolved.

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
