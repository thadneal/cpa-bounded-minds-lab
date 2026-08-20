# CPA Bounded Minds Laboratory

Version **0.10.0**

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

## v0.8 holdout result

The first frozen holdout was consumed on **2026-08-20**. Protocols 01-07 remained byte-for-byte frozen while twenty previously unused seeds were run. The result broke the development phase's perfect-Support pattern:

```text
140 protocol runs
121 Support
19 Mixed
0 Disconfirm

1000 preregistered checks
981 passed
19 failed

mechanism outcome   399 / 400
safety boundary     288 / 300
manipulation         174 / 180
accounting           120 / 120
```

The result revised several conclusions rather than merely repeating them:

- **Protocol 03:** developmental context remained useful, but not universally better than doctrine. One holdout world crossed the whole-history mechanism boundary while preserving the expected unstable-history calibration benefit.
- **Protocol 04:** typed communication remained robust in the frozen environmental family. Its only Mixed holdout was a manipulation miss, not a mechanism reversal. The stronger equal-budget comparator remains unresolved.
- **Protocol 05:** distributed convention still compressed coordination cheaply, but revisability occasionally disturbed conventions in contexts that had not changed. Cultural plasticity and cultural stability are therefore competing pressures.
- **Protocol 06:** ancestry-sensitive corroboration still strongly reduced echo amplification, but four safety failures showed that uncertain ancestry can suppress genuinely independent evidence. Ancestry should be treated as uncertain dependence evidence, not hard deduplication.
- **Protocol 07:** provisional standing still improved useful early access and remained much safer than inherited authority, but fragile transfer worlds exposed opportunity cost and occasional residual standing after contradiction. Second-hand standing should buy conditional opportunity, not durable authority.

Protocols 01 and 02 remain useful mechanism demonstrations, but their old seed semantics make their 20/20 holdout outcomes weaker validation evidence than Protocols 03-07.

`holdout-v1` is now **consumed**. It may be rerun for reproducibility, but it must not be treated as fresh validation after any tuning. See `docs/HOLDOUT_V1_RESULTS.md` for the preserved result and revised protocol conclusions.

## v0.9 operating-envelope challenge phase

Version 0.9.0 deliberately adds **no Protocol 08**. The next question is where the existing mechanisms stop working. `challenge-v1` performs an outcome-blind adversarial seed search over the frozen Protocol 03-07 world generators and records performance across five stress bands.

For each profile, candidate seeds `10001-29999` are scored from **world descriptors only**, before experiment outcomes are observed. The development and consumed holdout seeds are excluded. Four seeds are selected from each quintile-like stress band, yielding **20 runs per profile / 100 total challenge runs**.

The five registered profiles are:

- Protocol 03 source instability;
- Protocol 04 conflict density;
- Protocol 05 regime shift;
- Protocol 06 ancestry visibility;
- Protocol 07 recommender fragility.

Each profile reports a signed **boundary margin** where positive remains inside the registered operating envelope and zero is the intended crossover. The challenge report also keeps manipulation, mechanism, safety, and accounting checks separate.

This phase is intentionally adversarial rather than confirmatory. Mixed or Disconfirm outcomes, negative boundary margins, and non-monotonic stress curves are useful evidence. If no profile crosses a boundary, the correct conclusion is not unlimited robustness; it is that the frozen generators remain too protected and a later challenge must parameterize worlds beyond their original support.

One limitation is explicit: Protocol 04's `challenge-v1` profile stresses the environment but **does not yet replace semantic smoothing with the stronger equal-budget alternative** proposed after holdout review. That remains a separate control-strengthening task if P04 continues to resist environmental stress.

## v0.10 parameterized falsification phase

`challenge-v1` is now consumed exploratory evidence. Its 100 runs returned **78 Support / 22 Mixed / 0 Disconfirm**, with **317/320 mechanism**, **207/220 safety**, **133/140 manipulation**, and **78/80 accounting** checks passing. The run exposed real mechanism edges, but it also showed that the composite challenge stress scores were usually not monotonic causal variables. See `docs/CHALLENGE_V1_RESULTS.md`.

Version 0.10.0 therefore adds **no Protocol 08**. `parameterized-falsification-v1` intervenes on controlled causal axes outside the original frozen generator support while leaving Protocols 01-07 and their world generators byte-for-byte unchanged.

Registered surfaces:

- P03 history instability x present rule error;
- P04 warrant asymmetry x minority-correct fraction, with a stronger **equal-public-budget standing-weighted robust consensus** comparator;
- P05 repeated change frequency x change magnitude;
- P06 origin missingness x developmental-signature separation;
- P07 recommender credibility x mismatch prevalence;
- P07 recommender credibility x mismatch severity.

Each surface is `7 x 7` with seven deterministic replicates per cell. The P07 severity surface starts in the strong-contradiction regime because its frozen residual-standing ceiling was registered for strong mismatch rather than weak disagreement. The runner writes JSON, a Markdown summary, and one CSV per profile. Negative boundary margins are desired operating-envelope evidence. These surfaces are exploratory falsification and must not be reused as confirmation after mechanism tuning.

The lab is **not yet being closed**. `docs/NEXT_LAB.md` records two social protocol families that may still belong here after falsification: strategic public influence and coalition/authority cascades. The same document sketches a candidate successor **Trace and Interface Laboratory**, inspired by the attached *Traces of the Other* paper but formulated as synthetic causal/interface research rather than a commitment to conscious realism or the external reality of DMT entities.

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
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --falsify --output _artifacts/parameterized-falsification-v1
# Reproduce consumed challenge-v1 only when needed:
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --challenge --output _artifacts/challenge-v1-repro
# Reproduce the consumed holdout only when needed:
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --validation --output _artifacts/validation-holdout-v1-repro
# Regression only:
dotnet run --project src/Cpa.BoundedMindsLab.Cli -- --replicate 101,211,307,401,503 --output _artifacts/development-v1-regression
```

For live inspection on Windows 11:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Desktop
```

The Desktop Lab opens with **Development v1 (5, regression only)** selected to avoid accidentally treating consumed holdout-v1 as fresh evidence. Holdout v1 remains available as a reproducibility preset. `parameterized-falsification-v1` and consumed `challenge-v1` are CLI-only research runners; they do not use the ordinary Desktop seed selector. The application version remains visible in the main and maximized-graph window titles. Graphs explain axis meaning and preferred metric direction/context, provide show-all/hide-all series controls, and update incrementally from bounded background telemetry at an adaptive display cadence.

## Visualization boundary

Experiment execution remains isolated from WPF on a dedicated below-normal-priority worker. Display frames enter a bounded non-blocking queue, a background projector owns telemetry aggregation, and WPF samples already-projected state. Graph snapshots are resolution-bounded, hover works against rendered data, per-line legend visibility is presentation-only, and the maximized graph does not become a second experiment consumer.

The live graph, selector state, UI backlog, dropped display frames, splitter positions, selected metric, and graph cadence are invisible to experiment cognition. The durable `frames.ndjson` journal remains authoritative even when display-only frames are dropped.

The workbench also exposes protocol progress, per-seed judged results, active-seed identity, metric/path filtering, and a graph Seed selector that can revisit each retained seed history independently. These are observation surfaces, never evidence available to the experiment.

## Research stance

Carry forward only what survives pressure: bounded causal execution, persistent private history, compact public surfaces, revisable standing, explicit cost, and provenance distinct from agreement.

The v0.3.0 methodological correction remains active: if history is part of the theory, replication must vary **what was lived**, not merely the order in which nearly identical events were encountered. Version 0.8 separated development from a frozen holdout; version 0.9 consumed that holdout and searched the frozen generator families adversarially; version 0.10 replaces composite stress ranking with controlled causal intervention surfaces.
