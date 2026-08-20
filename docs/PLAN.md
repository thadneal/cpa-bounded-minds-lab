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

Status: **completed and consumed in v0.10.0**.

The controlled phase executed six `7 x 7` surfaces with seven deterministic replicates per cell: **294 cells / 2,058 runs**. `176/294` cells had a negative mean primary margin and `1,201/2,058` replicates crossed at least one registered boundary. Full interpretation is preserved in `PARAMETERIZED_FALSIFICATION_V1_RESULTS.md`.

The phase changed the implementation conclusions without rewriting the frozen Protocol 01-07 record:

- P03 developmental instability should modify uncertainty rather than automatically lower standing;
- P04 supports preserving epistemic shape but not one privileged aggregation rule;
- P05 exposed cultural hysteresis when stale convention remains "good enough" to reinforce itself;
- P06 falsified the frozen hard-ish grouping heuristic as a durable design because signature similarity could override negative provenance evidence;
- P07 separated recommender credibility from local generalizability and showed that eventual authority repair can arrive after material opportunity cost.

The surfaces are now consumed exploratory evidence. Any mechanism revised from them requires a later fresh validation design.

### Protocol 08: strategic public influence

Status: **frozen; fresh holdout 20/20 Support; controlled operating envelope characterized in v0.12**.

The preregistered `p08-holdout-v1` run returned 20/20 Support with all 200 assertions passing. Mean accountable RMSE was about `0.17873`, versus `0.35608` for naive self-report authority and `0.18599` for local-only learning. Accountable early aligned RMSE was about `0.23478` versus `0.41033` local-only, while late divergent and betrayal RMSE remained near `0.03` versus roughly `0.42` for the naive receiver.

The five controlled strategic-influence surfaces then exposed the operating envelope:

- delayed consequence erodes the advantage over a manipulable receiver, although the accountable path tends toward local learning rather than runaway capture in the tested range;
- weak or late betrayal can leave standing sticky because revocation has finite latency;
- keeping a persistently divergent peer available becomes an attention/opportunity cost when divergence is common enough;
- public feedback is not intrinsically unsafe because it is also the channel through which accountable consequence reshapes sender incentives;
- noisy consequence can reduce standing for a genuinely aligned peer, so source reliability and consequence-channel reliability require distinct uncertainty.

Narrow result: public influence can remain useful and strategically adaptive without private-state inspection when permission is receiver-owned and consequence-grounded. Carry forward the behavioral constraints, not the scalar standing equations. See `PROTOCOL_08_VALIDATION_RESULTS.md`.

### Protocol 09: authority ancestry / circular standing

Status: **frozen after coherent development-v1; fresh validation and controlled falsification registered in v0.14**.

Question:

> Can a bounded ecology distinguish authority earned from independent consequence from permission that has recursively circulated through locally reasonable endorsements?

The development matrix returned **5/5 Support with 50/50 preregistered checks passing**. Mean total RMSE was `0.17860` for authority ancestry, `0.20144` for recursive endorsement, and `0.20207` for direct-only learning. The recursive control amplified mean circular peer standing from `0.11874` to receiver authority `0.88673` (about 7.47x) without adding independent grounding. Authority ancestry held the same circular contexts to `0.22327`, preserved independently grounded authority near `0.61060`, and later receiver-owned consequence drove circular standing near `0.02328`.

This supports a distinct network-level failure mode: **permission can have ancestry even when evidence itself is not duplicated**. The exact root/path sketch representation remains an assay instrument. See `PROTOCOL_09_DEVELOPMENT_RESULTS.md`.

### v0.14 evidence phase

The exact Protocol 09 experiment and world-generator files are now frozen by SHA-256. Before any Protocol 09 holdout outcome is observed, the lab registers:

- `p09-holdout-v1`: twenty fresh seeds, disjoint from development-v1, holdout-v1, p08-holdout-v1, and challenge-v1;
- `authority-ancestry-falsification-v1`: six controlled 7 x 7 surfaces with seven deterministic replicates per cell.

The controlled surfaces vary:

1. independent grounding diversity x peer trust;
2. circulation depth x ancestry fidelity;
3. circular-root strength x receiver mismatch;
4. consequence delay x circulation depth;
5. grounded consequence noise x delay (null-harm);
6. network closure x independent-root count.

The holdout must run **first** and its artifact must be preserved before the controlled surfaces are interpreted. No mechanism changes are permitted between the fresh holdout and the falsification run.

### Current-lab closure question

The Bounded Minds Laboratory is now in its intended final evidence phase. No Protocol 10 is planned by default. If the frozen P09 result survives fresh holdout with a useful, interpretable operating envelope, move to synthesis and closeout. A new social protocol should appear only if this phase exposes a genuinely independent phenomenon that cannot be expressed as evidence ancestry, standing transfer, strategic influence, or authority ancestry.

The candidate successor **Trace and Interface Laboratory** remains documented in `NEXT_LAB.md`. Its question begins one layer earlier: how an observer detects structured causal influence when the source itself is only indirectly available through an interface.

## Result cadence

For v0.14:

1. rebuild under the pinned .NET 10 SDK and run the **41** invariant/regression checks;
2. verify `docs/FROZEN_PROTOCOL_SHA256.txt` so the exact Protocol 01-09 frozen sources remain unchanged;
3. run `--p09-validation` once and preserve the resulting `p09-holdout-v1` artifact before inspecting or changing anything;
4. review the holdout as independent evidence without tuning the frozen assay;
5. run `--p09-falsify` and interpret failure surfaces as operating-envelope evidence, including regions where ancestry caution becomes net harmful;
6. revise the durable behavioral conclusion, not the frozen scalar equations;
7. close Bounded Minds unless the evidence reveals a truly new social failure mode;
8. write the final handoff to the successor Trace and Interface Laboratory.
