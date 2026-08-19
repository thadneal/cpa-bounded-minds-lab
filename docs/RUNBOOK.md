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

## Targeted Protocol 04 check

After invariants pass, a single deterministic smoke history is useful:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 04-bounded-communication-before-language `
  --seed 101 `
  --output _artifacts/protocol-04-seed-101
```

Do not interpret the protocol from the smoke history alone. Run the canonical five-world matrix without changing thresholds:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --experiment 04-bounded-communication-before-language `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-04-five-seed
```

Review each seed's `scenario-generated` event alongside treatment outcomes. Protocol 04 seeds vary social-history composition and evidence, not just observation ordering.

## Full-suite checkpoint

After Protocol 04 is interpreted:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- `
  --all `
  --replicate 101,211,307,401,503 `
  --output _artifacts/full-suite-0.4.0
```

This runs 20 protocol histories under one replication report.

## Desktop inspection

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The Seeds field defaults to:

```text
101, 211, 307, 401, 503
```

The newest protocol is selected by default. Seeds run in succession. Each started seed retains its own bounded visualization store for the session, and the Live metrics Seed selector can step among them without overlaying histories. Prior judged results remain in the Protocol results tab, while the accent seed badge identifies the actively executing seed and its `current/total` position.

Protocol 04 progress should move through:

```text
Build private plurality
  seed-specific social circumstance
  three peers develop private histories
Compare communication forms
  low-dimensional typed signals
  early semantic-smoothing control
  same shared consequence remains sovereign
Evaluate
  seven falsification checks
  protocol verdict
```

## Artifacts

Desktop sessions default to `<repo>/_artifacts/desktop-YYYYMMDD-HHMMSS`.

Each seed directory contains `frames.ndjson`, `manifest.json`, and one result/metrics directory per selected protocol. A completed multi-seed session also writes `replication-report.json` at the session root.

`frames.ndjson` is the authoritative high-resolution observation record. The live graph is intentionally a bounded presentation of that record.
