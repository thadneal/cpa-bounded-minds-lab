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

Status: **implemented in v0.5.0, result pending**.

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
