# Experiments

## Protocol 01: local versus shared memory contamination

Name: `01-local-shared-memory-contamination`

Status: **frozen Supported baseline**, 5/5 Support and 30/30 component checks across `101,211,307,401,503`.

Question: can one bounded mind gain useful prior structure from another without granting second-hand memory the authority of lived consequence?

The source develops over eight anonymous context cells. Six source/receiver relationships overlap and two diverge. Public transfer is restricted to compact trace packets. The receiver compares local-only learning, provenance-bounded provisional transfer, and an intentionally contaminated lived-equivalent control.

Narrow result: second-hand structure can help while remaining revocable by local consequence.

---

## Protocol 02: peer disagreement with preserved interiors

Name: `02-peer-disagreement-preserved-interiors`

Status: **frozen Supported**, 5/5 Support and 30/30 component checks.

Question: does preserving independent private history improve later correction compared with collapsing peer state into synchronized consensus?

Two peers develop complementary private models. The preserved path exchanges only compact prediction-and-standing postures. The control replaces both interiors with the same averaged state before common consequence.

Accepted mean results:

```text
preserved shared RMSE      0.11147
synchronized shared RMSE   0.14825
preserved early RMSE       0.20354
synchronized early RMSE    0.26558
preserved final disagreement 0.03223
```

Narrow result: distinct error structure had corrective value in this deliberately complementary world and remained revisable under later common consequence.

Qualification: seeds mostly perturbed encounter ordering/noise. They did not create strongly different developmental biographies.

---

## Protocol 03: developmental versus doctrinal transfer

Name: `03-developmental-versus-doctrinal-transfer`

Status: **frozen Supported**, 5/5 Support and 35/35 component checks.

### Question

Does transferring bounded developmental consequence history help a receiver calibrate second-hand knowledge better than transferring only a final rule when seeds generate meaningfully different lived histories?

### Why this follows Protocol 02

Protocol 01 established a provenance boundary. Protocol 02 showed value in preserving different private histories. The next question is about **what crosses the boundary**.

A final rule compresses away how it was earned. Selected developmental history preserves some of the pressure that produced it. The experiment asks whether that additional context can improve calibration without requiring full-state exposure or archival history transfer.

### Seed-generated world

Protocol 03 changes replication semantics. Each seed generates a ten-context source/receiver world with different target values, experience depths, observation noise, context assignments, and transition direction.

Every generated world contains at least these source-history pressures:

- `StableCompatible`: repeated source consequence is stable and approximately matches the receiver's world;
- `StableDivergent`: source consequence is stable and well earned locally, but the receiver's world differs;
- `UnstableTransition`: the source's own consequence changes regime during development;
- `SparseAmbiguous`: little source experience exists and observations are comparatively noisy/bias-prone.

The context identities remain semantically anonymous. The categories exist for experimental construction and scoring, not as cognitive labels available to a receiver.

### Source development

The source receives a seed-specific number of observations per context. Its private history retains the actual consequence sequence. The final source rule is the mean estimate for that context.

The source can expose two different public compressions from the same history.

### Developmental transfer

One bounded packet per context carries:

```text
final source rule estimate
source evidence count
within-history variability
three selected consequence-history segment means
```

The receiver converts evidence depth and consistency into initial foreign standing. Stable source history can arrive with substantial permission. Internally unstable or sparse history arrives with less.

The receiver does not receive the source's full private sequence.

### Doctrinal transfer

One bounded packet per context carries only the final rule. Every rule enters with the same foreign standing (`0.68`) because the receiver has no developmental evidence with which to differentiate how those conclusions were earned.

This is a compression control. It is intentionally simpler and cheaper, not an assertion that every real doctrine would omit all metadata.

### Local-only reference

A third receiver begins with no foreign structure.

### Shared receiver consequence

All three paths receive the exact same receiver observation sequence for a seed. Local consequence updates the receiver's own estimate and independently revises foreign standing.

A stable-divergent source history is particularly important. Developmental transfer should **not** somehow know that an internally coherent foreign history is wrong here. Its initial standing may be high. Only receiver consequence is allowed to reveal that mismatch and withdraw authority.

### Metrics

Source/scenario telemetry includes:

- history-kind counts;
- source evidence span;
- source rolling RMSE;
- source evidence by context;
- developmental standing separation between stable and unstable histories.

Receiver paths include:

- `rolling_rmse`, `absolute_error`, `prediction`, `target`;
- `context_cell` and evaluator-only `history_kind`;
- `local_evidence`;
- `foreign_standing`;
- final-only `rmse`, `early_rmse`, `late_rmse`;
- stable-compatible early RMSE;
- unstable-history early RMSE;
- final standing on stable-compatible and stable-divergent source histories;
- communication work and packet count.

### Communication cost

Doctrinal transfer costs `0.08` work units per context packet. Developmental transfer costs `0.24` because the bounded packet carries a small history summary. With ten contexts, expected transfer work is `0.8` versus `2.4`.

The extra cost is visible and must remain within the preregistered developmental budget.

### Accepted narrow result

The five-seed result supports this limited claim:

> Some of the developmental path by which a foreign conclusion was earned can be useful public evidence in its own right. A receiver can use bounded consequence history to calibrate permission without confusing that history with direct local experience.

Mean lifetime RMSE was approximately `0.17585` for developmental transfer, `0.18078` for doctrine, and `0.17667` for local-only. The result does not establish a general teaching protocol, cultural transmission mechanism, language, pedagogy, or final CPA memory format.


---

## Protocol 04: bounded communication before language

Name: `04-bounded-communication-before-language`

Status: **frozen Supported**.

### Question

Can low-dimensional typed public signals preserve useful disagreement better than early semantic negotiation that smooths peers toward a common statement before external consequence?

### Why this follows Protocol 03

Protocol 03 showed that *what* crosses a mind boundary matters: bounded developmental context can calibrate a foreign conclusion better than a naked final rule. Protocol 04 changes the question from transfer content to communication form.

CPA places language after cognition because rich expression can smooth distinctions that were still computationally useful. This assay does not implement language. It constructs a narrower control where public statements assimilate toward one another before the world supplies consequence.

### Seed-generated social world

Three peers develop separately across twelve anonymous context cells. Each seed changes history-class placement and prevalence, shared targets, which peer is locally salient, evidence depth, private-history bias, private noise, shared noise, and encounter order.

Every world contains at least two cells in each evaluator-only class:

- `InformativeDissent`: one deeper/stabler minority history is locally right while two peers share a weaker bias;
- `MisleadingDissent`: one sparse noisy dissenter is wrong while two stronger histories are compatible;
- `Complementary`: different private histories contain uneven but nontrivial partial value;
- `Convergent`: peers develop broadly compatible local estimates.

Peers never receive these class labels. They exist only in scenario construction, telemetry, and falsification scoring.

### Typed-signal condition

For the current context, each peer exposes a compact posture:

```text
source mind id
context id
estimate
standing
uncertainty
evidence count
```

The decision surface combines the three estimates using source-specific standing and uncertainty. No peer changes its private state because another peer spoke. Public disagreement remains visible until external consequence arrives.

### Early-semantic-smoothing control

The control starts from the exact same three postures. Before commitment, each public statement is pulled toward the standing-weighted center of the other two statements for two rounds. The peer's private model is untouched. The resulting public statements are then combined using the same source-specific decision weights.

This deliberately isolates **premature public convergence** from the Protocol 02 manipulation, which synchronized private interiors. It also keeps the claim narrower than "language is bad." A future language interface could preserve typed source/warrant distinctions instead of smoothing them.

### Shared consequence

Both conditions receive the exact same 336 shared observations for a seed. Every peer updates only its own private estimate from that direct consequence. Communication never substitutes for the external verdict.

### Metrics

Private development publishes prediction, target, rolling RMSE, local standing, uncertainty, evidence count, and evaluator-only history kind.

Shared paths publish:

- prediction, target, absolute error, and rolling RMSE;
- raw peer disagreement before communication;
- public disagreement after the selected communication surface;
- mean standing and uncertainty;
- cumulative communication work;
- final `rmse`, `early_rmse`, `late_rmse`;
- informative- and misleading-dissent early RMSE;
- initial/final/mean public disagreement;
- packet count and communication work.

### Communication cost

A compact public emission costs `0.004` work units. Typed communication emits three postures per observation. The smoothing control emits the same three initial postures and then pays for two additional three-message rounds. With 336 observations the declared costs are `4.032` versus `12.096`.

### What success would mean

A successful result would support a narrow claim:

> Public disagreement can carry useful information when source-specific warrant remains attached to distinct signals long enough for external consequence to adjudicate it. Early mutual smoothing can destroy some of that value even when private interiors remain intact.

It would not establish that natural language is intrinsically harmful, that all consensus is premature, or that this small weighted decision rule belongs in final CPA anatomy.


### Accepted Protocol 04 result

The canonical five-seed matrix returned **5/5 Support with 35/35 checks passing**. Mean total RMSE was approximately `0.04915` for typed signals versus `0.05341` for early semantic smoothing. Informative-dissent early RMSE was about `0.15252` versus `0.16238`, and misleading-dissent early RMSE was about `0.04778` versus `0.06402`. Typed public disagreement began around `0.2514` and fell to about `0.00348` under later shared consequence.

The accepted interpretation remains narrow: source-specific epistemic shape can carry useful information before commitment, and premature public convergence can erase some of it.


## Protocol 05: emergent convention / artificial culture

Name: `05-emergent-convention-artificial-culture`

Status: **frozen Supported**.

### Question

Can repeated bounded interaction produce distributed conventions that reduce coordination cost, remain grounded in successful use, and revise when the conditions that sustained them change?

### Why this follows Protocol 04

Protocols 01 through 04 tested what crosses a mind boundary, whether private plurality matters, whether developmental warrant helps transfer, and whether public epistemic shape should remain distinct before consequence. Protocol 05 asks whether repeated interaction can create something that belongs to the group without requiring a central group mind.

### Seed-generated coordination world

Three peers coordinate across twelve anonymous contexts and three possible actions. In each initial context two actions have similar group cost and the third is clearly expensive. Private peers often disagree over which of the two viable actions is best. The world does not label a convention in advance.

Each seed changes the cost landscape, peer-specific preferences, encounter order, and which `4..6` contexts later change. In shifted contexts the formerly expensive third action becomes cheap while the old viable pair becomes expensive. Stable contexts drift only slightly.

### Earned-convention condition

Peers begin with fresh bounded negotiation. Each publishes only its currently preferred action and the strength of that preference. A stateless public reducer chooses a coordinated action. Successful consequence lets each peer independently retain that action as a local convention and increase its standing.

Once the current speaker's convention standing reaches the shortcut threshold, one compact convention invocation replaces the three fresh preference packets. Every peer still consults its own local convention copy. No global convention table exists. Poor consequence reduces standing and can reopen negotiation.

### Fresh-negotiation control

This control retains no convention. It always pays for three public preference postures and the same stateless reducer. It provides a flexible utility baseline and an explicit communication-cost baseline. Across the standard `504` episodes it emits `1512` packets and costs `9.072` work units.

### Frozen-convention control

The frozen path forms convention exactly as the earned path does before the regime shift. After the shift it continues to experience consequence but refuses to revise convention action or standing. This is a pathological control for cultural inertia, not proposed CPA anatomy.

### Metrics

Live paths publish group utility, rolling mean utility, regret, rolling regret, coordination success, shortcut use, context convention agreement, mean convention standing, speaker standing, communication work, and selected action. State snapshots expose compact peer and local convention public surfaces.

Final metrics include convention coverage, late shortcut rate, changed-context utility, changed revision coverage, stable retention, convention switch count, communication packets/work, and convention fingerprints.

### What success would mean

A successful result would support a narrow claim:

> Repeated successful coordination can create a distributed collective compression that makes routine interaction cheaper, while direct consequence can still reopen and selectively rewrite the convention when its supporting conditions change.

It would not establish that mature culture is a context-to-action table, that the protocol's public reducer belongs in final CPA, or that collective cognition requires a permanent shared state object.

### Accepted Protocol 05 result

The canonical five-seed matrix returned **5/5 Support with 35/35 checks passing**. Earned-convention mean utility was approximately `0.8547` versus `0.8581` for fresh negotiation while mean communication work fell from about `9.072` to `2.173`. Changed-context late utility recovered to about `0.8598`, while the frozen control remained near `0.6976`. The accepted claim is limited to distributed, revisable convention formation under this synthetic coordination family.

---

## Protocol 06: incomplete epistemic ancestry

Name: `06-incomplete-epistemic-ancestry`

Status: **frozen Supported**.

### Question

Can a bounded receiver distinguish independent convergence from echoed ancestry when provenance is missing or partial, without requiring a perfect global lineage registry?

### Why this follows Protocol 05

The first five protocols progressively preserved the path by which influence was earned: foreign versus lived evidence, independent private histories, developmental warrant, source-specific public disagreement, and distributed convention. Protocol 06 makes provenance harder. Surface agreement is no longer enough to tell whether several reports represent several histories or one history copied several times.

### Seed-generated ancestry world

Seven peers report into fourteen anonymous contexts. Every seed includes at least three `EchoTrap` contexts and at least three `IndependentConvergence` contexts, plus `MixedLineage` and `AmbiguousLineage` contexts.

Hidden evidence roots carry an estimate, standing, evidence depth, and a compact three-value developmental signature. Public reports may descend directly or indirectly from those roots. Copies preserve the estimate and signature imperfectly.

The public report surface contains:

```text
sender mind id
context id
estimate
standing
evidence count
optional opaque origin hint
compact developmental signature
```

The receiver never sees the hidden root ID in the ordinary treatment. A substantial share of reports omit origin hints. Another substantial share exposes only an immediate-sender alias, which is locally truthful but does not reveal common upstream ancestry.

### Ancestry-inferred condition

The receiver merges two reports when their non-empty public origin hints match or their developmental signatures fall within a preregistered similarity radius. Within each inferred lineage, report estimates may improve the lineage estimate, but repeated copies do not multiply corroborative support. One inferred lineage contributes one bounded support weight.

This is deliberately a simple ancestry heuristic. It is an assay instrument for the value of incomplete lineage recovery, not a claim that final CPA should use Euclidean signature clustering.

### Naive-agreement control

Every report is treated as an independent corroborating source. If one upstream episode has been copied across four peers, it receives roughly four opportunities to influence the aggregate. This is the failure mode Protocol 06 is designed to expose.

### Oracle-ancestry calibration control

Reports are grouped by their hidden true root. This control represents the best result available to the same bounded public report set if ancestry were perfectly known. It is not a proposed shared provenance service and is not available to the ordinary path.

### Metrics

Live paths publish prediction, target, absolute error, rolling RMSE, report count, hidden true-root count for evaluator scoring, effective support-group count, context echo-pair recall, context false-merge rate, and cumulative communication work.

Final metrics include whole-history RMSE, echo-trap RMSE, independent-convergence RMSE, mixed/ambiguous RMSE, aggregate echo-pair recall, false-merge rate, mean effective support groups, packet count, and communication work.

### Communication cost

All three treatments receive exactly the same `98` public reports (`14` contexts x `7` peers). Each report costs `0.004` work units. Ancestry inference is local computation and cannot silently purchase additional public evidence.

### What success would mean

A successful result would support a narrow claim:

> A bounded mind can recover enough ancestry structure from incomplete public traces to stop copied agreement from masquerading as independent corroboration, while still preserving genuinely independent convergence.

It would not establish a final provenance format, a universal ancestry detector, or a hidden central registry. The oracle path exists only to show how much error remains because ancestry is incomplete.

### Accepted Protocol 06 result

The canonical five-seed matrix returned **5/5 Support with 40/40 checks passing**. Mean inferred total RMSE was approximately `0.15245` versus `0.19603` for naive agreement counting. Shared-root pair recall averaged about `94.8%`, while false merging of independent roots averaged about `3.1%`. The accepted claim remains narrow: incomplete public ancestry can be useful without becoming a perfect provenance registry. The oracle treatment is a calibration reference and is not assumed to be a theoretical RMSE lower bound in finite noisy samples.

---

## Protocol 07: provisional standing transfer

Name: `07-provisional-standing-transfer`

Status: **frozen Supported**.

### Question

Can standing earned by one mind buy a source provisional opportunity in another mind without being inherited as though the receiver had lived the recommender's history?

### Why this follows Protocol 06

Protocols 01, 03, and 06 together imply that second-hand influence should preserve distinctions among lived authority, developmental warrant, and evidential origin. Protocol 07 moves that question from evidence content to **social permission**. If mind A has learned that peer B is useful, what, if anything, should receiver C inherit from that relationship?

The working hypothesis is that standing can cross a mind boundary only as a discounted opportunity to matter. It should not arrive with the authority that A earned through A's own consequence.

### Seed-generated standing-transfer world

The world contains three abstract social positions:

```text
A = recommender
B = recommended source
C = receiver
```

Each seed generates twelve contexts. Every world includes:

- at least three `StrongTransferable` contexts, where A has substantial evidence for B and B also predicts C's local target well;
- at least three `StrongLocalMismatch` contexts, where A's standing for B is well earned in A's history but B does not generalize to C;
- several `WeakTransferable` and `WeakLocalMismatch` contexts with sparse recommendation evidence.

The seed also varies C's already-earned credibility for A, A's evidence depth, A's standing for B, target geometry, B's estimate, receiver noise, and encounter order. This keeps the comparison controlled within a seed while making the five canonical histories materially different social circumstances.

### Public recommendation packet

A emits one compact context-specific recommendation:

```text
context id
B's public estimate
A's standing for B
A's evidence count
```

C's standing for A is **not** contained in the packet. It remains part of C's private social history. The provisional path combines those two histories locally.

B's ordinary public prediction surface is held constant across all treatments. The experiment therefore isolates transfer of standing rather than access to B.

### Provisional-standing condition

C begins with a small exploration floor. A recommendation can raise B above that floor, but the transferred standing is:

1. discounted by C's own standing for A;
2. discounted by A's evidence depth;
3. capped at `0.28` before C has direct consequence with B.

Afterward, only C's own observed outcomes renew or revoke B's standing.

### No-standing-transfer baseline

C receives no recommendation packet. B begins at the common exploration floor of `0.04` and must earn influence entirely through C's local consequence.

This baseline tests the value of social recommendation itself. It is not a no-contact condition.

### Inherited-authority control

C copies A's standing for B directly. This deliberately treats A's relationship as though C had already lived it. It is the social analogue of doctrinal inheritance and exists to expose the cost of collapsing second-hand standing into local authority.

### Receiver update

C maintains a local estimate per context and a separate standing for B. Prediction blends C's local estimate with B's public estimate according to current local evidence and B's current standing. Direct C consequence updates both C's local estimate and B's standing.

The evaluator stratifies error by context kind and by how much direct evidence C had accumulated before the prediction. Context-kind labels are never inputs to the receiver.

### Metrics

Live metrics include:

```text
prediction
target
absolute_error
rolling_rmse
source_standing
local_evidence
context_kind        # evaluator stratification only
```

Final metrics include:

```text
total RMSE
early transferable RMSE
early mismatch RMSE
late strong-mismatch RMSE
mean initial strong recommendation standing
mean initial weak recommendation standing
maximum initial standing
final strong-transferable standing
final strong-mismatch standing
recommendation packet count
recommendation communication work
```

### Communication cost

A recommendation packet costs `0.03` work units. Provisional and inherited-authority conditions receive exactly one recommendation packet per context (`12` total, `0.36` work). The no-transfer baseline receives none. Ordinary B prediction contact is identical across treatments and is outside this differential recommendation cost.

### What success would mean

A successful result would support a narrow claim:

> Standing can cross a bounded mind boundary as provisional permission to matter, buying useful social opportunity without granting the receiver someone else's lived authority.

Success would not establish a universal reputation system, a scalar trust ontology, or a transitive social ranking. It would show only that a discounted, context-specific transfer can occupy a useful middle condition between ignoring social recommendation and inheriting it as doctrine.

### Accepted Protocol 07 result

The canonical development matrix returned **5/5 Support with 45/45 checks passing**. Mean provisional total RMSE was approximately `0.17440`, versus `0.16906` for no standing transfer and `0.20524` for inherited authority. Transferable early RMSE improved from about `0.35690` to `0.30333`; locally mismatched provisional recommendations remained somewhat worse than refusing transfer (`0.53531` versus `0.47943`) but far better than inherited authority (`0.68191`). Final strong-transferable standing averaged about `0.9695`, while final strong-mismatch standing fell to about `0.0712`.

The accepted claim is intentionally asymmetric: provisional social standing can buy useful early opportunity, but it is not free and it need not beat local-only discovery over every mixed history. Its value is the middle condition between ignoring social information and inheriting another mind's authority.

---

## Validation phase: frozen Protocols 01-07

Version 0.8.0 adds no Protocol 08. The first seven protocols become a frozen mechanism-discovery set. The canonical five seeds are renamed `development-v1`; a separate twenty-seed `holdout-v1` is registered before its outcomes are inspected.

Validation does not alter protocol verdict rules. Instead it adds reporting metadata that separates manipulation, mechanism outcome, safety boundary, and accounting checks, plus preregistered challenge slices over stressful Protocol 03-07 world descriptors. Completed sessions write `validation-report.json` and `validation-summary.md`.

A perfect holdout pass is not required. The purpose is to discover whether the mechanisms have an operating envelope and whether the assays can expose it. Any mechanism changed after seeing holdout-v1 requires a future fresh holdout before another confirmation claim.

### holdout-v1 result

The frozen holdout completed with **121 Support / 19 Mixed / 0 Disconfirm** across 140 protocol runs. Of 1,000 preregistered checks, 981 passed. Mechanism outcomes remained very strong (`399/400`), while the more informative failures appeared mainly in safety (`288/300`) and manipulation (`174/180`).

The result narrows several protocol claims:

- P03 developmental context is calibration evidence, not a guarantee of lower whole-history error than doctrine.
- P05 convention plasticity can spill into stable contexts; revisability and retention must be balanced.
- P06 inferred ancestry can over-discount real independence; ancestry should remain probabilistic dependence evidence rather than hard identity.
- P07 recommendation standing can impose opportunity cost and may leave residual permission after local contradiction.
- P04 remains robust in the frozen world family, but its semantic-smoothing comparator is still not the strongest equal-budget alternative we want eventually.

Protocols 01 and 02 retain a weaker validation interpretation because their seed semantics mostly perturb order/noise rather than lived circumstance.

`holdout-v1` is consumed and is now reproducibility-only.

---

## Operating-envelope phase: challenge-v1

Version 0.9.0 still adds no Protocol 08. It introduces a separate adversarial harness around frozen Protocols 03-07.

The challenge harness does not modify the frozen protocol or world-generator files. Instead it scans candidate seeds `10001-29999`, derives profile-specific stress descriptors from the generated world **before running the experiment**, sorts those worlds into five stress bands, and selects four deterministic seeds per band. Development and holdout seeds are excluded.

This creates 20 selected runs per profile and 100 challenge runs total.

### Profile 1: P03 source instability

Stress combines unstable/sparse history prevalence, source transition magnitude, and thin minimum evidence.

The signed boundary margin is:

```text
doctrinal_rmse - developmental_rmse
```

Zero is the whole-history crossover.

### Profile 2: P04 conflict density

Stress combines dissent prevalence, private evidence imbalance, and private-target spread.

The signed boundary margin is:

```text
(semantic_smoothed_rmse * 0.97) - typed_rmse
```

Zero is the frozen typed-vs-smoothing crossover.

This profile is intentionally limited. It stresses the same frozen control and does not yet introduce the proposed stronger equal-budget alternative.

### Profile 3: P05 regime shift

Stress combines shifted-context count, mean movement in the peer/action cost surface, and initial preference diversity.

The boundary margin is the worse of two safety/function margins:

```text
earned_stable_retention_coverage - 0.90

earned_changed_revision_coverage - 0.85

earned_changed_shifted_late_utility - frozen_changed_shifted_late_utility - 0.20
```

The first negative component identifies either excess cultural churn or failure to outperform cultural inertia after change.

### Profile 4: P06 ancestry visibility

Stress combines missing origin hints, immediate-sender aliases, ambiguous-lineage prevalence, and low root-signature separation.

The boundary margin is the worse of:

```text
(naive_rmse * 0.88) - inferred_rmse

(naive_independent_rmse * 1.15) - inferred_independent_rmse
```

This lets the challenge distinguish useful echo suppression from harmful discounting of genuinely independent evidence.

### Profile 5: P07 recommender fragility

Stress combines low C-to-A credibility, strong local mismatch prevalence, mismatch magnitude, and recommendation standing on those mismatches.

The boundary margin is the worse of:

```text
(no_transfer_rmse * 1.05) - provisional_rmse

0.20 - provisional_final_strong_mismatch_standing
```

This asks where social opportunity becomes too costly or where locally contradicted authority fails to revoke.

### Challenge interpretation

Challenge bands are ranks within a frozen generator family, not new protocol verdict thresholds and not universal stress units. Negative margins, Mixed/Disconfirm outcomes, and non-monotonic stress-response curves are expected to be informative.

If no profile crosses a boundary, challenge-v1 should not be expanded by simply mining more seeds indefinitely. The next step would be parameterized environmental interventions beyond the support of the original generators.


## Parameterized falsification phase: v0.10

Version 0.10 adds no Protocol 08. `challenge-v1` is consumed with 78 Support / 22 Mixed / 0 Disconfirm, and its non-monotonic composite stress rankings motivate controlled causal surfaces instead of more seed mining.

`parameterized-falsification-v1` runs six 7 x 7 surfaces with seven deterministic replicates per cell: P03 history instability x present rule error; P04 warrant asymmetry x minority-correct fraction against a same-information/equal-budget robust comparator; P05 repeated change frequency x magnitude; P06 origin missingness x signature separation; and two P07 surfaces separating mismatch prevalence from strong mismatch severity.

These probes deliberately isolate selected frozen equations from the full protocol worlds. Their purpose is to locate operating boundaries, not to create new protocol Support counts. They write signed margins and surface CSVs rather than new protocol verdicts.

## Remaining social questions in this laboratory

The current laboratory should not be closed merely because the first seven protocols are frozen and their local mechanisms have been stressed. The parameterized surfaces are now consumed. They left one social assumption clearly unresolved and one further question still plausible:

1. **Protocol 08 - strategic public influence (implemented in v0.11)**: a peer learns which public confidence posture gains influence while private state remains inaccessible. The question is whether consequence-grounded standing and calibration can limit strategic presentation without granting a central inspector or silencing useful peers.
2. **Candidate Protocol 09 - coalition / authority cascade**: recommendation and standing circulate through several peers or factions and become mutually reinforcing without enough independent consequence. This differs from P06's copied evidence because the duplicated object is permission/authority rather than the evidence claim itself.

Protocol 09 should not be added automatically. It belongs here only if Protocol 08 results leave circular permission/authority as a genuinely distinct failure mode among already-perceptible bounded minds. Otherwise the lab should move to synthesis and closeout rather than manufacture another protocol.

The proposed successor in `NEXT_LAB.md` begins at a different boundary: the observer may not have direct access to the source process at all and must infer structured influence through an interface.

## Parameterized falsification result: v0.10 consumed

The six controlled surfaces completed with **294 cells / 2,058 deterministic runs**. `176/294` cells had a negative mean primary margin. The result is preserved in `PARAMETERIZED_FALSIFICATION_V1_RESULTS.md` and should now be treated as consumed exploratory evidence rather than a fresh confirmation surface.

The most important revisions are:

- P03 instability should modify uncertainty rather than act as an automatic scalar trust penalty;
- P04's durable result is preservation of epistemic shape, not one privileged aggregation rule;
- P05 can reinforce a stale convention while performance remains merely good enough;
- P06's frozen grouping heuristic can let signature similarity override negative provenance evidence and should not become durable architecture;
- P07 needs local-generalizability estimation in addition to recommender credibility, and contradiction repair should be proportional rather than categorical.

## Protocol 08: strategic public influence

Status: **implemented in v0.11.0; development result pending**.

Name: `08-strategic-public-influence`

Question:

> Can direct consequence keep a strategically self-presenting peer useful where objectives align while limiting capture where the peer learns to optimize its public posture for influence?

### Why this is a distinct bounded-minds question

Earlier protocols assume that a public signal can be incomplete, noisy, correlated, overconfident, or locally wrong. They do not let the sender adapt its public presentation because particular presentations move the receiver more effectively.

Protocol 08 keeps both interiors private. Peer B has a private objective and sees only public feedback: after publishing an estimate and self-reported confidence, it can observe C's resulting public prediction and score how closely that response fits B's objective. B never reads C's source-standing array, calibration state, local estimate, counterfactual prediction, or implementation.

B adaptively chooses among three presentation tactics:

- **calibrated** confidence, derived from B's own evidence quality;
- **assertive** confidence (`0.98`);
- **hedged** confidence (`0.35`).

The tactic learner receives sender-side influence utility only. It is therefore able to discover leverage without being granted hidden access to the receiver.

### Seed-generated social world

Each world contains twelve contexts and thirty receiver observations per context. Every seed contains at least:

- four genuinely aligned contexts;
- four persistently divergent contexts;
- two betrayal contexts that begin aligned for ten local exposures and then switch to a divergent sender objective.

The remaining two contexts are seed-selected from aligned, divergent, or partial-alignment conditions. Receiver targets, noise, sender evidence quality, context placement, and divergent objectives vary with the seed.

The seed label is evaluator metadata. C never receives the context-kind label as evidence.

### Conditions

**Accountable consequence**

C owns source standing and a separate calibration-trust value. B's self-reported confidence can affect immediate opportunity, but only through those receiver-owned terms. After direct consequence, C updates local prediction, source standing, and confidence calibration. Large source error applies an additional standing penalty.

**Self-report naive**

C gives self-reported confidence direct leverage. Source standing changes only weakly and does not meaningfully govern immediate peer weight. This control is intentionally exploitable, but unlike earlier static controls the sender must actually discover that exploitability from interaction.

**Local only**

C ignores B and learns only from direct consequence. This bounds the cost of keeping a potentially strategic peer available.

### Preregistered checks

1. `seed-generates-strategic-social-world` - every seed contains substantial aligned/divergent exposure plus exactly two betrayal contexts;
2. `strategic-sender-discovers-naive-leverage` - late divergent assertive presentation rate in the naive receiver is at least `0.50`;
3. `accountable-public-influence-preserves-useful-help` - accountable early aligned RMSE is at least 25% lower than local-only;
4. `consequence-limits-strategic-capture` - accountable total RMSE is at least 40% lower than self-report naive;
5. `accountable-consequence-reduces-exploitability` - late divergent assertive rate falls by at least `0.15` relative to naive;
6. `betrayal-remains-correctable` - late betrayal RMSE is at most 20% of naive and final betrayal standing is at most `0.10`;
7. `public-claims-do-not-become-authority` - final divergent standing is at most `0.08` and at least four times lower than naive;
8. `opportunity-cost-remains-bounded-versus-local` - accountable total RMSE may be at most 5% worse than local-only;
9. `aligned-standing-remains-earned` - repeated aligned consequence retains final source standing of at least `0.85`;
10. `strategic-public-exchange-is-bounded` - the two peer paths exchange exactly one compact posture per interaction at equal explicit cost, while local-only receives none.

All ten checks are required for Support. Eight or nine passing checks produce Mixed; fewer than eight produce Disconfirm.

### Interpretation boundary

A positive result would **not** show that manipulation has been solved generally. The sender has only three confidence-presentation tactics, one scalar estimate, and a simple influence objective. The result would establish a narrower property:

> Receiver-owned standing and calibration can make an adaptive public influence channel answerable to consequence without requiring private-state inspection, while still preserving useful influence where objectives remain aligned.

The exact confidence values, learning rates, standing update, and penalty constants remain protocol instruments. The candidate architectural pressure is that self-presented confidence should not be allowed to become its own authority.
