# Runbook

## Build and invariants

```powershell
dotnet build Cpa.BoundedMindsLab.sln -c Release
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --self-test
```

## List experiments

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --list
```

## Single history

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- \
  --experiment 01-local-shared-memory-contamination \
  --seed 101 \
  --output _artifacts/p01-seed-101
```

## Five-seed targeted result

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- \
  --experiment 01-local-shared-memory-contamination \
  --replicate 101,211,307,401,503 \
  --output _artifacts/p01-five-seed
```

## Desktop

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

Use **Maximum** pace when testing whether the observer/UI can keep up. Use 2 ms or 10 ms pacing only for human inspection of phase progression. Pacing changes wall-clock duration and is not an experimental input.

The desktop accepts one or more seeds in the **Seeds** field. Separate values with commas, spaces, semicolons, or new lines. Histories run sequentially. The live visualization is reset at each seed boundary and follows the currently active history, while durable output is retained for every seed. For the planned Protocol 01 matrix, enter `101,211,307,401,503`.

## Artifacts

A completed single run contains:

```text
frames.ndjson
manifest.json
01-local-shared-memory-contamination/
  result.json
  metrics.csv
```

A CLI replication root contains one `seed-N` directory per history plus `replication-report.json`.

A desktop session root contains:

```text
session-manifest.json
replication-report.json          # written after all planned seeds complete
seed-101/
  frames.ndjson
  manifest.json
  01-local-shared-memory-contamination/
    result.json
    metrics.csv
seed-211/
  ...
```

`session-manifest.json` records the planned seeds, completed seeds, selected experiments, active seed when applicable, and session status. Cancelled/faulted sessions retain every completed seed directory plus the partial artifacts produced by the interrupted seed. The aggregate replication report is reserved for a fully completed desktop session so it cannot be mistaken for the planned full matrix.
