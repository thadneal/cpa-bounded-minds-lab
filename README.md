# CPA Bounded Minds Laboratory

Version **0.5.2**

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

`05-emergent-convention-artificial-culture` is the current targeted protocol. Three bounded peers repeatedly coordinate in a seed-generated twelve-context world. For each context, two coordinated actions begin close enough in cost that no single convention is installed in advance, while private peers often prefer different actions. Repeated successful negotiation can cause the peers' separate local convention memories to converge on one shared habit. Once that habit has standing, one compact convention invocation can replace fresh three-peer negotiation.

The experiment compares:

- `earned-convention`, convention is retained locally, earns standing through successful consequence, compresses later communication, and can lose standing when the world changes;
- `fresh-negotiation`, no convention is retained and three public preference packets are paid on every episode;
- `frozen-convention`, the same culture forms before the regime shift but later consequence cannot revise it.

A seed determines the private cost landscape and which `4..6` contexts later change. Stable contexts remain approximately as lived. Changed contexts make the formerly expensive third action newly useful. There is no central convention registry in the ordinary path.

Protocol 05 asks whether something collective can emerge between bounded minds, reduce coordination cost, remain near the utility of fresh negotiation, and still be locally rewritten by consequence rather than hardening into doctrine.

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
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --experiment 05-emergent-convention-artificial-culture --seed 101
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --experiment 05-emergent-convention-artificial-culture --replicate 101,211,307,401,503 --output _artifacts/protocol-05-five-seed
```

For live inspection on Windows 11:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The Desktop Lab opens with `101, 211, 307, 401, 503` already entered in **Seeds**, selects the newest protocol by default, and includes the running application version in the main and maximized-graph window titles. Graphs explain axis meaning and preferred metric direction/context, provide show-all/hide-all series controls, and rebuild from committed metric batches rather than every incremental numeric sample.

## Visualization boundary

Experiment execution remains isolated from WPF on a dedicated below-normal-priority worker. Display frames enter a bounded non-blocking queue, a background projector owns telemetry aggregation, and WPF samples already-projected state. Graph snapshots are resolution-bounded, hover works against rendered data, per-line legend visibility is presentation-only, and the maximized graph does not become a second experiment consumer.

The live graph, selector state, UI backlog, dropped display frames, splitter positions, selected metric, and graph cadence are invisible to experiment cognition. The durable `frames.ndjson` journal remains authoritative even when display-only frames are dropped.

The workbench also exposes protocol progress, per-seed judged results, active-seed identity, metric/path filtering, and a graph Seed selector that can revisit each retained seed history independently. These are observation surfaces, never evidence available to the experiment.

## Research stance

Carry forward only what survives pressure: bounded causal execution, persistent private history, compact public surfaces, revisable standing, explicit cost, and provenance distinct from agreement.

The v0.3.0 methodological correction remains active: if history is part of the theory, replication should eventually vary **what was lived**, not merely the order in which nearly identical events were encountered.
