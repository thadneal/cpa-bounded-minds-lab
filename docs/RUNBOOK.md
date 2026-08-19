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

Treat analyzer failures as build failures. The repository enables warnings as errors.

## Targeted Protocol 02 check

Start with one seed only after the invariant suite passes:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 02-peer-disagreement-preserved-interiors `
  --seed 101 `
  --output _artifacts/protocol-02-seed-101
```

Then run the preregistered five-seed matrix without changing thresholds:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 02-peer-disagreement-preserved-interiors `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-02-five-seed
```

## Full-suite checkpoint

Protocol 01 is frozen, but the suite is currently only two protocols. After the targeted Protocol 02 result is interpreted, a complete checkpoint is inexpensive and useful:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --all `
  --replicate 101,211,307,401,503 `
  --output _artifacts/full-suite-0.2.0
```

This produces ten histories, five seeds for each protocol, under one replication report.

## Desktop inspection

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The Seeds field accepts several values, for example:

```text
101, 211, 307, 401, 503
```

Each seed runs in succession. The accent seed indicator above Protocol progress shows the currently active seed and its position in the session. Protocol progress changes its labels when execution moves from Protocol 01 to Protocol 02.

For a targeted Protocol 02 desktop run, select only `02-peer-disagreement-preserved-interiors`. Use maximum pace first. Watch the status-bar display backlog and graph build time, but do not treat presentation drops as scientific evidence.

## Artifacts

Desktop sessions default to `<repo>/_artifacts/desktop-YYYYMMDD-HHMMSS`.

Each seed directory contains:

```text
frames.ndjson
manifest.json
<protocol-name>/
  result.json
  metrics.csv
```

The session root contains `session-manifest.json`, and a fully completed multi-seed session also contains `replication-report.json`.

`frames.ndjson` is the authoritative high-resolution observation record. The live graph is intentionally a bounded projection of that record.
