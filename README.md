# CPA Bounded Minds Laboratory

Version **0.6.0**

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

`04-bounded-communication-before-language` is frozen Supported. The accepted five-seed matrix returned Support in all five histories with all 35 preregistered checks passing. Mean total RMSE was about `0.04915` for typed communication versus `0.05341` for early semantic smoothing. Typed communication preserved informative dissent, spread less low-quality dissent, and converged under later shared consequence.

Narrow result: preserving source-specific epistemic shape through a compact public surface can outperform premature semantic convergence while a situation is unresolved. This does not establish that language itself is harmful.

### Protocol 05 - emergent convention / artificial culture

`05-emergent-convention-artificial-culture` is frozen Supported. The canonical five-seed matrix returned Support in all five histories with all 35 preregistered checks passing. Earned convention preserved about `99.6%` of fresh-negotiation utility while reducing public communication work by about `76%`. Changed contexts were locally revised, stable contexts retained their convention, and the frozen-culture control remained cheap but materially less useful after the world changed.

Narrow result: repeated bounded interaction can produce a distributed convention that compresses routine coordination without requiring a central culture owner, provided local consequence can reopen and revise that convention.

### Protocol 06 - incomplete epistemic ancestry

`06-incomplete-epistemic-ancestry` is the current targeted protocol. Seven peers report into seed-generated contexts where apparent agreement can arise in two importantly different ways: several genuinely independent roots may converge, or several peer reports may descend from one upstream episode that has been copied through the social field.

Public ancestry is deliberately incomplete. Some reports preserve an opaque upstream hint, many omit it, and many expose only an immediate-sender alias. Every report also carries a compact developmental signature that is copied imperfectly rather than acting as a perfect lineage identifier.

The experiment compares:

- `ancestry-inferred`, the receiver groups likely shared ancestry using only the incomplete public hints and signature similarity, then lets each inferred lineage contribute one bounded unit of corroborative support;
- `naive-agreement`, every peer report is counted as independent corroboration;
- `oracle-ancestry`, true hidden roots are supplied only as a calibration ceiling and are not proposed CPA machinery.

The generated world contains echo traps, genuine independent convergence, mixed lineages, and deliberately ambiguous lineages. Protocol 06 succeeds only if inferred ancestry reduces echo-driven error, preserves independent convergence, recovers most true shared-root pairs, avoids excessive false merges, and remains close to the oracle while using the same bounded public report set.

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
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --experiment 06-incomplete-epistemic-ancestry --seed 101
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --experiment 06-incomplete-epistemic-ancestry --replicate 101,211,307,401,503 --output _artifacts/protocol-06-five-seed
```

For live inspection on Windows 11:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The Desktop Lab opens with `101, 211, 307, 401, 503` already entered in **Seeds**, selects the newest protocol by default, and includes the running application version in the main and maximized-graph window titles. Graphs explain axis meaning and preferred metric direction/context, provide show-all/hide-all series controls, and update incrementally from bounded background telemetry at an adaptive display cadence.

## Visualization boundary

Experiment execution remains isolated from WPF on a dedicated below-normal-priority worker. Display frames enter a bounded non-blocking queue, a background projector owns telemetry aggregation, and WPF samples already-projected state. Graph snapshots are resolution-bounded, hover works against rendered data, per-line legend visibility is presentation-only, and the maximized graph does not become a second experiment consumer.

The live graph, selector state, UI backlog, dropped display frames, splitter positions, selected metric, and graph cadence are invisible to experiment cognition. The durable `frames.ndjson` journal remains authoritative even when display-only frames are dropped.

The workbench also exposes protocol progress, per-seed judged results, active-seed identity, metric/path filtering, and a graph Seed selector that can revisit each retained seed history independently. These are observation surfaces, never evidence available to the experiment.

## Research stance

Carry forward only what survives pressure: bounded causal execution, persistent private history, compact public surfaces, revisable standing, explicit cost, and provenance distinct from agreement.

The v0.3.0 methodological correction remains active: if history is part of the theory, replication should eventually vary **what was lived**, not merely the order in which nearly identical events were encountered.
