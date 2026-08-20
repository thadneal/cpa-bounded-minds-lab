# CPA Bounded Minds Laboratory

Version **0.8.0**

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

`06-incomplete-epistemic-ancestry` is frozen Supported. The canonical five-seed matrix returned Support in all five histories with all 40 preregistered checks passing. Mean total RMSE was about `0.15245` for inferred ancestry versus `0.19603` for naive agreement counting, while shared-root pair recall averaged about `94.8%` and false merging of independent roots averaged about `3.1%`.

Narrow result: a bounded receiver can recover enough epistemic ancestry from incomplete public origin hints and developmental signatures to discount echoed agreement while preserving genuine independent convergence. Perfect global provenance was unnecessary in this synthetic world family. The hidden-root oracle remains an evaluator calibration reference, not a theoretical RMSE lower bound.

### Protocol 07 - provisional standing transfer

`07-provisional-standing-transfer` is frozen Supported. The canonical development matrix returned Support in all five histories with all 45 preregistered checks passing. Mean provisional total RMSE was about `0.17440`, versus `0.16906` for refusing standing transfer and `0.20524` for inherited authority. On transferable contexts, provisional standing reduced early RMSE from about `0.35690` to `0.30333`. On locally mismatched recommendations it remained worse than no transfer (`0.53531` versus `0.47943`) but substantially better than inherited authority (`0.68191`). Direct consequence later drove strong transferable standing to about `0.9695` and strong mismatch standing to about `0.0712`.

Narrow result: second-hand standing can buy useful provisional opportunity without being copied as lived authority, but social transfer carries a measurable cost when recommendations fail to generalize. The useful principle is conditional permission, not a universal reputation scalar.

## v0.8 validation phase

Protocols 01-07 are now frozen as the **mechanism-discovery set**. Version 0.8.0 deliberately does not add Protocol 08. It asks whether the existing mechanisms survive fresh pressure without retuning them.

Two seed sets are now named explicitly:

- `development-v1`: `101,211,307,401,503`. These histories were repeatedly used while constructing and calibrating the first seven assays. They are regression data, not fresh validation.
- `holdout-v1`: twenty previously unused seeds registered in source. Protocol mechanics and falsification thresholds must remain frozen while this set is interpreted.

A completed Desktop session now writes both `validation-report.json` and `validation-summary.md`. Validation separates checks into **manipulation**, **mechanism outcome**, **safety boundary**, and **accounting constraint** categories so exact packet accounting or world-construction checks do not inflate the apparent evidential weight of mechanism outcomes.

The report also applies preregistered **challenge slices** to holdout worlds for Protocols 03-07, including high source instability, dense conflicting social evidence, high regime shift, weak ancestry visibility, and fragile recommender transfer. These slices are filters over world descriptors, not new tuned treatments. Protocols 01 and 02 predate the seed-as-lived-circumstance correction, so their holdout evidence is explicitly marked weaker.

The validation report warns when every run or every assertion still passes. In this phase, observing Mixed or Disconfirm outcomes can be scientifically useful because it begins to reveal an operating envelope.

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
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --validation --output _artifacts/validation-holdout-v1
# Regression only:
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --replicate 101,211,307,401,503 --output _artifacts/development-v1-regression
```

For live inspection on Windows 11:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The Desktop Lab opens with **Holdout v1 (20, frozen)** selected, enters the twenty holdout seeds, selects all frozen Protocols 01-07 by default, and includes the running application version in the main and maximized-graph window titles. Graphs explain axis meaning and preferred metric direction/context, provide show-all/hide-all series controls, and update incrementally from bounded background telemetry at an adaptive display cadence.

## Visualization boundary

Experiment execution remains isolated from WPF on a dedicated below-normal-priority worker. Display frames enter a bounded non-blocking queue, a background projector owns telemetry aggregation, and WPF samples already-projected state. Graph snapshots are resolution-bounded, hover works against rendered data, per-line legend visibility is presentation-only, and the maximized graph does not become a second experiment consumer.

The live graph, selector state, UI backlog, dropped display frames, splitter positions, selected metric, and graph cadence are invisible to experiment cognition. The durable `frames.ndjson` journal remains authoritative even when display-only frames are dropped.

The workbench also exposes protocol progress, per-seed judged results, active-seed identity, metric/path filtering, and a graph Seed selector that can revisit each retained seed history independently. These are observation surfaces, never evidence available to the experiment.

## Research stance

Carry forward only what survives pressure: bounded causal execution, persistent private history, compact public surfaces, revisable standing, explicit cost, and provenance distinct from agreement.

The v0.3.0 methodological correction remains active: if history is part of the theory, replication must vary **what was lived**, not merely the order in which nearly identical events were encountered. Version 0.8 additionally treats the canonical five seeds as development data and preserves a separate frozen holdout set.
