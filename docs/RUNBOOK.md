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

Treat analyzer failures as build failures. Warnings are errors.

## Targeted Protocol 03 check

After invariants pass, a single deterministic smoke history is useful:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 03-developmental-versus-doctrinal-transfer `
  --seed 101 `
  --output _artifacts/protocol-03-seed-101
```

Do not interpret the protocol from the smoke history alone. Run the canonical five-world matrix without changing thresholds:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 03-developmental-versus-doctrinal-transfer `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-03-five-seed
```

Unlike Protocols 01 and 02, these seeds intentionally generate different developmental circumstances. Review scenario metrics and `scenario-generated` events in `frames.ndjson` alongside the final replication report.

## Full-suite checkpoint

After Protocol 03 is interpreted:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --all `
  --replicate 101,211,307,401,503 `
  --output _artifacts/full-suite-0.3.1
```

This runs 15 protocol histories under one replication report.

## Desktop inspection

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The Seeds field defaults to:

```text
101, 211, 307, 401, 503
```

The newest protocol is selected by default. Seeds run in succession. Each started seed retains its own bounded visualization store for the session, and the Live metrics Seed selector can step among them without overlaying histories. Prior judged results remain in the Protocol results tab, while the accent seed badge identifies the actively executing seed and its `current/total` position.

Protocol 03 progress should move through:

```text
Build lived source history
  seed-specific developmental circumstance
  source develops and packages transfer
Compare transfer forms
  local-only baseline
  developmental consequence-history transfer
  doctrinal final-rule transfer
Evaluate
  seven falsification checks
  protocol verdict
```

## Artifacts

Desktop sessions default to `<repo>/_artifacts/desktop-YYYYMMDD-HHMMSS`.

Each seed directory contains `frames.ndjson`, `manifest.json`, and one result/metrics directory per selected protocol. A completed multi-seed session also writes `replication-report.json` at the session root.

`frames.ndjson` is the authoritative high-resolution observation record. The live graph is intentionally a bounded presentation of that record.
