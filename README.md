# CPA Bounded Minds Laboratory

Version **0.3.1**

This repository is the successor experimental program to the completed CPA Cognitive Development Lab. The earlier lab studied development inside one bounded organism. This laboratory moves the boundary outward:

> What develops among bounded minds when they can exchange selected public evidence without collapsing their independent interiors?

The solution is intentionally a laboratory rather than a production CPA implementation. Protocol-local mechanisms remain experimental instruments until repeated pressure shows that they deserve architectural standing.

## Current experimental record

### Protocol 01 - local/shared memory contamination

`01-local-shared-memory-contamination` is a frozen Supported baseline. The accepted five-seed matrix (`101,211,307,401,503`) returned Support in all five histories with all 30 component assertions passing.

Narrow result: compact second-hand developmental traces can provide useful prior structure while remaining weak enough for direct receiver consequence to selectively retain or extinguish their influence.

### Protocol 02 - peer disagreement with preserved interiors

`02-peer-disagreement-preserved-interiors` is also frozen Supported. The accepted five-seed matrix returned Support in all five histories with all 30 component assertions passing. Mean shared-phase RMSE was about `0.11147` with preserved interiors versus `0.14825` after synchronization, while later common consequence reduced preserved disagreement to about `0.03223`.

Narrow result: in the deliberately complementary synthetic world, preserving distinct private error structure provided corrective value compared with prematurely collapsing the peers into one consensus.

The result carries a methodological qualification. Those five seeds varied encounter order and noise much more than developmental circumstance. They were useful perturbation replications, but weaker tests of longitudinal individuality than CPA ultimately requires.

### Protocol 03 - developmental versus doctrinal transfer

`03-developmental-versus-doctrinal-transfer` introduces the next methodological step as part of the experiment itself.

A seed now generates a **lived developmental circumstance**, including:

- which context cells receive stable-compatible, stable-divergent, unstable-transition, or sparse-ambiguous source histories;
- source evidence depth;
- target landscape;
- source and receiver observation noise;
- direction of unstable regime transition;
- encounter order.

The source then exposes two bounded transfer surfaces derived from the same private history:

- `developmental-transfer`: a compact history packet that carries evidence depth and three selected consequence-history segment means, allowing the receiver to calibrate initial foreign standing;
- `doctrinal-transfer`: only the final source rule for each context under one undifferentiated foreign standing.

A `local-only` receiver remains as a reference path.

The protocol asks whether developmental context can retain the head start from stable compatible histories, reduce contamination from source histories that were themselves unstable, remain no worse overall than doctrine, and still submit all foreign authority to direct receiver consequence.

The canonical replication matrix remains `101,211,307,401,503`. In Protocol 03 these values now select materially different world histories rather than mostly shuffling one fixed curriculum.

See `docs/EXPERIMENTS.md` and `docs/FALSIFICATION.md` before interpreting results.

## Solution

```text
Cpa.BoundedMindsLab.sln
src/
  Cpa.BoundedMindsLab.Core/
  Cpa.BoundedMindsLab.Cli/
  Cpa.BoundedMindsLab.Desktop/
docs/
scripts/
```

Projects target .NET 10. The WPF Desktop Lab targets `net10.0-windows`.

## Quick start

```powershell
dotnet build Cpa.BoundedMindsLab.sln -c Release
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --self-test
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --experiment 03-developmental-versus-doctrinal-transfer --seed 101
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --experiment 03-developmental-versus-doctrinal-transfer --replicate 101,211,307,401,503 --output _artifacts/protocol-03-five-seed
```

For live inspection on Windows 11:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The Desktop Lab now opens with `101, 211, 307, 401, 503` already entered in **Seeds** and selects the newest protocol by default.

## Visualization boundary

Experiment execution remains isolated from WPF on a dedicated below-normal-priority worker. Display frames enter a bounded non-blocking queue, a background projector owns telemetry aggregation, and WPF samples already-projected state. Graph snapshots are resolution-bounded, hover works against rendered data, per-line legend visibility is presentation-only, and the maximized graph does not become a second experiment consumer.

The live graph, selector state, UI backlog, dropped display frames, splitter positions, selected metric, and graph cadence are invisible to experiment cognition. The durable `frames.ndjson` journal remains authoritative even when display-only frames are dropped.

The workbench also exposes protocol progress, per-seed judged results, active-seed identity, metric/path filtering, and a graph Seed selector that can revisit each retained seed history independently. These are observation surfaces, never evidence available to the experiment.

## Research stance

Carry forward only what survives pressure: bounded causal execution, persistent private history, compact public surfaces, revisable standing, explicit cost, and provenance distinct from agreement.

The v0.3.0 methodological correction adds another rule: if history is part of the theory, replication should eventually vary **what was lived**, not merely the order in which nearly identical events were encountered.
