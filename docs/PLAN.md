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

## Seed policy and the v0.8 validation correction

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

The holdout set must not become another tuning set. If a protocol is modified after seeing holdout-v1, that mechanism requires a new validation set rather than another claim on holdout-v1.

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

Status: **implemented in v0.8.0; holdout result pending**.

Version 0.8 deliberately adds no Protocol 08. The next question is whether the mechanism-discovery set survives fresh pressure when protocol code and preregistered thresholds are frozen.

The validation report separates four kinds of evidence:

1. **manipulation**: did the generated world/control actually contain the intended condition?
2. **mechanism outcome**: did the proposed behavior produce the predicted functional result?
3. **safety boundary**: did the behavior remain revisable, selective, or otherwise avoid the failure mode under test?
4. **accounting constraint**: did communication/compute stay within the explicit budget?

These categories remain visible together, but passing an exact packet count no longer psychologically counts as the same kind of confirmation as lower RMSE under a difficult social circumstance.

The first validation pass also defines preregistered challenge **slices** over holdout world descriptors for Protocols 03-07. They do not alter frozen mechanisms or thresholds:

- high source instability (P03);
- dense conflicting social evidence (P04);
- high regime shift (P05);
- weak ancestry visibility (P06);
- fragile recommender transfer (P07).

A validation report should explicitly warn about 100% Support or 100% assertion pass rates. Mixed or Disconfirm outcomes can be valuable because they begin to identify an operating envelope. Protocols 01 and 02 retain a special limitation: their older seed semantics do not produce the same degree of lived-world variation, so fresh seed numbers provide weaker validation there.

## Result cadence

For the v0.8 validation phase:

1. rebuild under the pinned .NET 10 SDK and run the 23 invariant/regression checks;
2. verify `docs/FROZEN_PROTOCOL_SHA256.txt` before running validation;
3. run all seven frozen protocols across `holdout-v1` exactly once without mechanism or threshold changes;
4. inspect protocol verdict rates, mechanism/safety categories, and challenge-slice coverage together;
5. treat all-Support or all-pass outcomes as an assay-sensitivity warning rather than automatic confirmation;
6. record any Mixed/Disconfirm results as candidate operating-envelope evidence before deciding whether a mechanism should be revised;
7. if code is changed from holdout evidence, retire `holdout-v1` for confirmation and register a new holdout set before making a new validation claim.
