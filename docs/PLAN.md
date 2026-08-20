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

Status: **completed and consumed in v0.9.0**.

`challenge-v1` selected 100 adversarial runs from the frozen Protocol 03-07 generator families using preregistered, outcome-blind world descriptors. It returned:

```text
78 Support
22 Mixed
0 Disconfirm

mechanism outcome   317 / 320
safety boundary     207 / 220
manipulation         133 / 140
accounting             78 / 80
```

The challenge exposed useful boundaries, but it also falsified an assumption in the challenge apparatus itself: a composite world-level "stress" score is generally not a monotonic causal variable when its ingredients can help and hurt a mechanism in different ways.

The revised protocol lessons are preserved in `CHALLENGE_V1_RESULTS.md`. In short:

- P03 crossed a real developmental-versus-doctrinal mechanism boundary in an extreme world, but higher source instability could also make developmental context *more* useful;
- P04 did not receive a meaningful adversarial test because greater conflict density fed the condition typed communication was designed to exploit;
- P05 exposed the cost of keeping culture plastic enough to revise while retaining stable convention;
- P06 again showed that soft ancestry weighting can suppress genuine independence even without obvious categorical false merges;
- P07 reached a clear fragile-transfer region, while also exposing a distinct residual-authority failure that cannot be collapsed into opportunity cost.

`challenge-v1` is now consumed exploratory evidence. It may be reproduced, but its selected seeds and composite stress scores are not a future confirmation set.

### Parameterized falsification phase: parameterized-falsification-v1

Status: **implemented in v0.10.0; results pending**.

Version 0.10 still adds no Protocol 08. Instead of searching more generated seeds, it copies the relevant frozen local equations into controlled micro-assays and intervenes directly on causal variables outside the original generator support.

Six `7 x 7` response surfaces are registered, with seven deterministic replicates per cell:

1. **P03 history instability x present rule error**;
2. **P04 warrant asymmetry x minority-correct fraction**, against a stronger same-information, equal-public-budget standing-weighted robust-consensus comparator;
3. **P05 change frequency x change magnitude**, including repeated regime changes not available in the frozen P05 generator;
4. **P06 origin missingness x developmental-signature separation**, including complete provenance blindness;
5. **P07 recommender credibility x mismatch prevalence**;
6. **P07 recommender credibility x strong mismatch severity**.

The P07 severity surface begins in the strong-contradiction regime because the frozen `0.20` residual-standing safety boundary was registered for strongly mismatched recommendations, not for weak disagreement that should rationally retain standing.

Each cell records a signed primary boundary margin and the mean diagnostics needed to understand why it crosses. A negative margin is desired falsification information. These surfaces are exploratory development evidence; if mechanisms are later changed, the same surfaces must not be reinterpreted as fresh confirmation.

The authoritative runner writes `parameterized-report.json`, `parameterized-plan.json`, `parameterized-summary.md`, and one CSV surface per profile.

### Current-lab closure question

The Bounded Minds Laboratory is **not closed** by v0.10. Parameterized falsification should finish before deciding whether the social research question has been exhausted.

Two protocol families still plausibly belong here because they concern influence among already-perceptible bounded peers rather than perception of hidden causes:

- **strategic public influence**: can consequence-based standing limit a peer that learns to shape its public posture strategically in order to gain influence while its private interior remains unavailable?
- **coalition / authority cascade**: can recommendations, standing, prestige, or factional loops become self-reinforcing even when the underlying evidence is not independently renewed?

These are candidates, not commitments. After the falsification surfaces are reviewed, add them only if they test a social failure mode that is not already answered by Protocols 01-07.

A candidate successor **Trace and Interface Laboratory** is documented in `NEXT_LAB.md`. Its question begins one layer earlier: how a bounded observer detects and represents structured causal influence when the source itself is only indirectly available through an interface.

## Result cadence

For the v0.10 parameterized falsification phase:

1. rebuild under the pinned .NET 10 SDK and run the **30** invariant/regression checks;
2. verify `docs/FROZEN_PROTOCOL_SHA256.txt` before running any research command;
3. run `parameterized-falsification-v1` once without modifying frozen Protocol 01-07 experiment/world source or the registered response surfaces;
4. inspect response surfaces, negative replicate counts, and null regions rather than reducing the phase to a Support percentage;
5. treat crossover, non-monotonicity, and comparator wins as useful causal evidence;
6. do not tune a mechanism from these surfaces and then reuse the same surface as confirmation;
7. after review, decide whether strategic-public-influence and authority-cascade protocols still add unresolved bounded-minds evidence;
8. only then prepare a closeout/handoff to the candidate Trace and Interface Laboratory;
9. keep `holdout-v1` and `challenge-v1` reproducibility-only.
