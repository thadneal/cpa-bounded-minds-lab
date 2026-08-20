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

Treat analyzer failures as build failures. Warnings are errors. Version 0.14.0 defines **41 invariant/regression tests**. `scripts/verify.ps1` and `scripts/verify.sh` first verify the frozen Protocol 01-09 source hashes before building.

## Current research run: Protocol 09 fresh holdout, then operating envelope

Protocol 09 development is complete and the exact assay is frozen. The canonical five-seed matrix returned **5/5 Support with 50/50 checks passing**. The next evidence order is fixed: consume the fresh holdout before inspecting the controlled falsification surfaces.

```powershell
./scripts/verify.ps1

# First execution is fresh evidence and consumes p09-holdout-v1.
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p09-validation `
  --output _artifacts/p09-holdout-v1

# Preserve the holdout artifact before this exploratory operating-envelope phase.
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p09-falsify `
  --output _artifacts/authority-ancestry-falsification-v1
```

Do not modify `AuthorityAncestryCircularStandingExperiment.cs` or `AuthorityAncestryWorld.cs` between these phases. Negative falsification margins are useful boundary evidence, not a reason to tune the frozen protocol and reinterpret the same cells as confirmation.

## Development regression

The canonical five-seed set is explicitly development data:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --replicate 101,211,307,401,503 `
  --output _artifacts/development-v1-regression
```

Use this only to detect regressions against the experimental record. Another all-Support run is not fresh confirmation.

## Consumed Protocol 08 reproduction

The twenty-seed `p08-holdout-v1` returned 20/20 Support and is consumed. Its five controlled falsification surfaces are also consumed.

```powershell
# Holdout reproduction only
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-validation `
  --output _artifacts/p08-holdout-v1-repro

# Operating-envelope reproduction only
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-falsify `
  --output _artifacts/strategic-influence-falsification-v1-repro
```

Do not describe reruns as fresh evidence and do not modify the frozen Protocol 08 sources.

## Other consumed evidence reproduction

```powershell
# P03-P07 controlled falsification
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --falsify --output _artifacts/parameterized-falsification-v1-repro

# Adversarial challenge
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --challenge --output _artifacts/challenge-v1-repro

# P01-P07 frozen holdout
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --validation --output _artifacts/validation-holdout-v1-repro
```

## Desktop Lab

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The window title includes the running assembly version. The Run panel remains a development/history workbench rather than the authoritative CLI surface for consumed validation phases. Seed -> Focus Path -> Metric scopes graph inspection. Legend keys remain individually clickable; Show all/Hide all acts on the current metric. Graph telemetry updates incrementally at the adaptive display cadence while experiment execution remains isolated from WPF.

Protocol Progress still recognizes Protocol 09's authority-world generation, social authority development, receiver consequence, and evaluation phases for deterministic reproduction. Run notes should treat Protocols 01-09 as frozen evidence; the fresh P09 holdout and controlled falsification remain CLI-only evidence modes.

Protocol-result assertion detail includes validation category (`manipulation`, `mechanism-outcome`, `safety-boundary`, or `accounting-constraint`). Data-grid cells wrap text and headers expose resize grippers; column resizing redistributes width inside the existing table surface.

## Cancellation and stepping

Pause requests stop at the next observation boundary. `Step observation` releases one boundary. Resume continues the same history. Cancel is cooperative and should retain already-written artifacts. A debugger configured to break on thrown `OperationCanceledException` may stop at the throw site even when the application subsequently handles cancellation normally.
