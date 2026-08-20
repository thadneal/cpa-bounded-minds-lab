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

Treat analyzer failures as build failures. Warnings are errors. Version 1.0.0 defines **42 invariant/regression tests**. `scripts/verify.ps1` and `scripts/verify.sh` first verify the frozen Protocol 01-09 source hashes before building.

## Closed-lab checkpoint

The research program is complete at Protocol 09. No fresh evidence run remains in this repository. The final checkpoint is regression and hash verification:

```powershell
./scripts/verify.ps1

# Optional full nine-protocol development regression. This is not fresh evidence.
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --replicate 101,211,307,401,503 `
  --output _artifacts/development-v1-closeout-regression
```

`verify.ps1` verifies the frozen Protocol 01-09 SHA-256 list, builds Release, and runs all 42 self-tests. The full matrix is useful as a final local execution checkpoint after the closeout release, but its five seeds remain development data.

## Consumed Protocol 09 reproduction

The first registered `p09-holdout-v1` run returned **20/20 Support with 200/200 checks passing**. The subsequent six-surface operating-envelope phase executed **294 cells / 2,058 deterministic runs**. Both are consumed evidence.

```powershell
# Holdout reproduction only
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p09-validation `
  --output _artifacts/p09-holdout-v1-repro

# Operating-envelope reproduction only
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p09-falsify `
  --output _artifacts/authority-ancestry-falsification-v1-repro
```

Do not modify the frozen P09 mechanism and reuse these artifacts as confirmation. See `PROTOCOL_09_VALIDATION_RESULTS.md`.

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

Protocol Progress still recognizes Protocol 09's authority-world generation, social authority development, receiver consequence, and evaluation phases for deterministic reproduction. Run notes should treat Protocols 01-09 as frozen evidence; the consumed P09 holdout and controlled falsification remain CLI-only reproducibility modes.

Protocol-result assertion detail includes validation category (`manipulation`, `mechanism-outcome`, `safety-boundary`, or `accounting-constraint`). Data-grid cells wrap text and headers expose resize grippers; column resizing redistributes width inside the existing table surface.

## Cancellation and stepping

Pause requests stop at the next observation boundary. `Step observation` releases one boundary. Resume continues the same history. Cancel is cooperative and should retain already-written artifacts. A debugger configured to break on thrown `OperationCanceledException` may stop at the throw site even when the application subsequently handles cancellation normally.
