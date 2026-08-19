# CPA Bounded Minds Laboratory

Version **0.4.0**

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

`03-developmental-versus-doctrinal-transfer` is frozen Supported. The five canonical seeds all returned Support with all 35 preregistered checks passing, and the seed-policy correction produced materially different lived histories.

Accepted mean results:

```text
developmental total RMSE   0.17585
doctrinal total RMSE       0.18078
local-only total RMSE      0.17667
developmental unstable early RMSE 0.28124
doctrinal unstable early RMSE     0.31044
```

Narrow result: bounded evidence about how a foreign conclusion was earned improved calibration relative to transferring only the final rule. The advantage was moderate rather than universal, and direct receiver consequence remained able to revoke stable foreign structure that was locally wrong.

### Protocol 04 - bounded communication before language

`04-bounded-communication-before-language` is the current targeted protocol. Three peers develop different private histories in a seed-generated social world. The experiment then compares two public communication surfaces while keeping private interiors intact in both conditions:

- `typed-signals`: one bounded source-specific posture per peer and observation containing estimate, standing, and uncertainty;
- `early-semantic-smoothing`: the same initial public postures undergo two peer-to-peer smoothing rounds before commitment and before external consequence.

The control is a model of **premature semantic convergence**, not a claim that natural language necessarily destroys disagreement. It deliberately asks what can be lost when public statements become mutually assimilated before the world has supplied a verdict.

Seeds now generate different mixtures of informative dissent, misleading dissent, complementary expertise, convergent histories, evidence depths, target landscapes, and noise. All treatments within one seed receive the same generated circumstance.

Protocol 04 asks whether typed communication can preserve useful dissent, avoid spreading low-quality dissent, reduce whole-history error, remain bounded in communication work, and still converge when peers later share direct external consequence.

The canonical matrix remains `101,211,307,401,503`. See `docs/EXPERIMENTS.md` and `docs/FALSIFICATION.md` before interpreting results.

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
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --experiment 04-bounded-communication-before-language --seed 101
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --experiment 04-bounded-communication-before-language --replicate 101,211,307,401,503 --output _artifacts/protocol-04-five-seed
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

The v0.3.0 methodological correction remains active: if history is part of the theory, replication should eventually vary **what was lived**, not merely the order in which nearly identical events were encountered.
