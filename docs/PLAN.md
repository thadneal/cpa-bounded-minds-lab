# Experimental Plan

## Research center

The successor laboratory asks:

> What develops among bounded minds when they can exchange selected public evidence without collapsing their independent interiors?

## Founding constraints

Unless a protocol explicitly challenges one of them:

1. causal execution is bounded and observable;
2. each mind has persistent private history;
3. inter-mind exchange crosses compact public surfaces only;
4. standing is provisional permission to influence later cognition;
5. provenance and ancestry remain distinct from surface agreement;
6. communication, verification, consultation, and storage have explicit cost;
7. no ordinary path receives unrestricted access to all private interiors;
8. artificial controls are labeled as controls rather than promoted into architecture.

## Seed policy, consumed holdout, and challenge selection

Protocols 01 and 02 are frozen records and keep their original seed semantics. Their seeds mainly perturb schedule and observation noise. Beginning with Protocol 03, a seed selects a member of a preregistered developmental-world family and can change what was lived: evidence depth, source reliability, local target relationships, regime transitions, overlap, and encounter order. Treatments within one seed still receive the same generated world.

Version 0.8 adds a second correction. The canonical five seeds were repeatedly used while building and sanity-checking mechanisms, so they are now named explicitly as development data:

```text
development-v1 = 101, 211, 307, 401, 503
```

They remain useful for deterministic regression but are no longer fresh validation. A separate twenty-seed set is frozen before Protocol 01-07 validation outcomes are inspected:

```text
holdout-v1 = 809, 977, 1201, 1429, 1693, 2017, 2371, 2741, 3163, 3581,
             4001, 4441, 4871, 5303, 5741, 6211, 6673, 7121, 7603, 8089
```

The holdout set was consumed on 2026-08-20 and must not become another tuning/confirmation set. If a protocol is modified after seeing holdout-v1, that mechanism requires a new future validation set rather than another claim on holdout-v1.

Version 0.9 adds `challenge-v1`. Challenge seeds are not a holdout: they are selected adversarially from a fixed candidate range using evaluator-side world descriptors before outcomes are run. They exist to expose failure surfaces.

## Experimental arc

### Protocol 01: local versus shared memory contamination

Status: **frozen Supported**.

Can a receiver gain useful prior structure from a source without treating another history as lived consequence?

Accepted result: 5/5 Support, 30/30 component checks.

### Protocol 02: peer disagreement with preserved interiors

Status: **frozen Supported with replication qualification**.

Can distinct private error structures improve correction relative to premature state synchronization?

Accepted result: 5/5 Support, 30/30 checks. Preserved interiors reduced mean shared-phase error by about 24.8% relative to the synchronization control and later converged under common consequence.

Qualification: seed variation was mostly perturbative rather than meaningfully developmental. This observation motivates the v0.3.0 seed-policy change rather than retroactive redesign of Protocol 02.

### Protocol 03: developmental versus doctrinal transfer

Status: **frozen Supported**.

Accepted result: 5/5 Support, 35/35 checks. The seed-policy correction worked: the five runs varied context-history composition, evidence depth, target landscape, and outcome magnitude. Developmental transfer was consistently better calibrated than doctrinal transfer, especially on internally unstable source histories, while remaining roughly comparable to local-only learning overall.

Narrow result: some of the developmental path by which a conclusion was earned can be useful public evidence without becoming lived authority in the receiver.

### Protocol 04: bounded communication before language

Status: **frozen Supported**.

Accepted result: 5/5 Support, 35/35 checks. Typed public postures produced lower mean total RMSE than early semantic smoothing, preserved useful disagreement, bounded misleading dissent, and later converged under shared consequence.

Narrow result: preserving source-specific epistemic shape before commitment can have value while the situation is unresolved.

### Protocol 05: emergent convention / artificial culture

Status: **frozen Supported**.

Accepted result: 5/5 Support, 35/35 checks. Earned convention retained about 99.6% of fresh-negotiation utility while reducing public communication work by about 76%. Changed contexts revised locally, stable contexts retained convention, and the frozen control remained cheap but failed under changed conditions.

Narrow result: repeated bounded interaction can compress into a distributed, revisable convention without introducing a persistent central culture owner.

### Protocol 06: incomplete epistemic ancestry

Status: **frozen Supported**.

Accepted result: 5/5 Support, 40/40 checks. Inferred ancestry reduced mean total RMSE from about `0.19603` under naive agreement counting to `0.15245`, while recovering about `94.8%` of true shared-root report pairs and falsely merging only about `3.1%` of independent-root pairs on average.

Narrow result: useful ancestry-sensitive corroboration does not require a perfect global provenance registry. Partial public origin hints and compact developmental signatures can preserve enough of evidential origin to discount echoed agreement while leaving genuine independent convergence intact.

### Protocol 07: provisional standing transfer

Status: **frozen Supported**.

Accepted result: 5/5 Support, 45/45 checks. Provisional standing bought a useful early head start where A's relationship with B generalized to C, but it was not free: provisional total RMSE (`~0.17440`) was slightly worse than refusing standing transfer (`~0.16906`) across the mixed world. The same provisional path was materially safer than inherited authority (`~0.20524` total RMSE), and direct C consequence selectively renewed strong transferable standing (`~0.9695`) while reducing strong mismatched standing (`~0.0712`).

Narrow result: another mind's standing can purchase limited, context-specific opportunity without becoming the receiver's lived authority. The mechanism's cost on non-generalizing recommendations is part of the result, not something to hide.

### Validation phase: frozen Protocols 01-07

Status: **completed in v0.8.0; holdout-v1 consumed**.

The frozen twenty-seed holdout produced 121 Support and 19 Mixed verdicts across 140 protocol runs. It therefore broke the development phase's perfect-Support pattern without requiring any protocol retuning. Across 1,000 preregistered checks, 981 passed and 19 failed. Category-level results were:

- mechanism outcome: `399/400`;
- safety boundary: `288/300`;
- manipulation: `174/180`;
- accounting constraint: `120/120`.

The failures revised the mechanism record:

- P03 showed that developmental context is useful but not guaranteed to beat doctrine on whole-history error;
- P05 exposed a tension between cultural revisability and stable-context retention;
- P06 exposed the danger of over-discounting genuinely independent evidence under uncertain ancestry;
- P07 exposed opportunity cost and occasional residual authority under fragile recommendation transfer;
- P04 remained unusually robust in the frozen family, but its semantic-smoothing control is still weaker than the equal-budget alternative we ultimately want to test.

Protocols 01 and 02 remain mechanism demonstrations with weaker holdout meaning because their seeds predate lived-circumstance semantics.

`holdout-v1` is now consumed. It is retained only for reproducibility. If any frozen mechanism is changed after this point, a future confirmatory claim requires a new holdout.

### Operating-envelope challenge phase: challenge-v1

Status: **implemented in v0.9.0; challenge result pending**.

Version 0.9 still adds no Protocol 08. Instead, it asks where the frozen Protocol 03-07 mechanisms stop working. `challenge-v1` searches a fixed candidate seed range (`10001-29999`) using only world descriptors, before experiment outcomes are observed. Development and holdout seeds are excluded.

Each profile sorts candidate worlds by a preregistered stress score, divides that generator distribution into five ordered bands, and selects four deterministic seeds from each band. This produces 20 runs per profile and 100 runs total.

Registered profiles:

1. **P03 source instability**: unstable/sparse history prevalence, transition magnitude, thin evidence;
2. **P04 conflict density**: dissent prevalence, evidence imbalance, private-target spread;
3. **P05 regime shift**: shifted-context count, cost-landscape movement, private preference diversity;
4. **P06 ancestry visibility**: missing/alias hints, ambiguous lineage, root-signature separation;
5. **P07 recommender fragility**: weak recommender credibility, mismatch prevalence, mismatch magnitude.

Each profile defines a signed boundary margin. Positive remains inside the frozen operating envelope, zero is the registered crossover, and negative indicates a boundary violation. The report also preserves the manipulation/mechanism/safety/accounting taxonomy.

This is intentionally not a second holdout. Challenge seeds are selected adversarially from preregistered descriptors in order to reveal failure surfaces. Outcomes may inform the next architecture or challenge design, but they cannot be used to turn challenge-v1 into confirmatory evidence.

Two limitations are explicit. First, challenge-v1 remains inside the original frozen world-generator support; if no boundaries appear, the next step is parameterized stress beyond that support. Second, P04 still lacks a stronger equal-budget alternative control. Its current challenge profile stresses the environment only.

## Result cadence

For the v0.9 operating-envelope phase:

1. rebuild under the pinned .NET 10 SDK and run the 26 invariant/regression checks;
2. verify `docs/FROZEN_PROTOCOL_SHA256.txt`;
3. run `challenge-v1` once without modifying Protocol 03-07 mechanisms, world generators, or preregistered thresholds;
4. inspect boundary margins by stress band together with verdicts and category-level failures;
5. treat Mixed, Disconfirm, and negative margins as useful boundary evidence rather than failed development;
6. inspect non-monotonic curves carefully, because a stress score that does not track failure pressure is itself evidence that the challenge descriptor is poor;
7. if no selected profile crosses a boundary, do not increase seed count indefinitely. Move to parameterized generator stress beyond the frozen support;
8. do not reuse consumed holdout-v1 as confirmation after any mechanism change. Register a new future holdout only when a revised mechanism is ready for confirmation.
