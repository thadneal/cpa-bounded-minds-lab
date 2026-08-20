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

Treat analyzer failures as build failures. Warnings are errors. Version 0.5.0 defines 16 invariant tests.

## Targeted Protocol 05 check

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 05-emergent-convention-artificial-culture `
  --seed 101 `
  --output _artifacts/protocol-05-seed-101
```

Then run the canonical matrix without changing thresholds:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 05-emergent-convention-artificial-culture `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-05-five-seed
```

Review `scenario-generated`, `convention-formed`, `regime-shift`, and `path-complete` events alongside the seven assertions.

## Full-suite checkpoint

After Protocol 05 is interpreted:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --all `
  --replicate 101,211,307,401,503 `
  --output _artifacts/full-suite-0.5.0
```

This runs 25 protocol histories.

## Desktop inspection

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The main window title includes the running assembly version. Seeds default to `101, 211, 307, 401, 503`. Protocol 05 is selected by default.

Protocol 05 progress should move through:

```text
Let a culture form
  seed-specific plural coordination world
  repeated success earns local convention standing
Compare coordination modes
  earned distributed convention
  fresh negotiation baseline
  frozen-convention control
Change the world and judge
  regime shift + seven falsification checks
  protocol verdict
```

## Artifacts

Desktop sessions default to `<repo>/_artifacts/desktop-YYYYMMDD-HHMMSS`. Each seed keeps its own journal and result directories. `frames.ndjson` remains the authoritative high-resolution observation record.
