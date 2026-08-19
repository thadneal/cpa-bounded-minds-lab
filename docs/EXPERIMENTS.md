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

Status: **result pending**.

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

### What success would mean

A successful result would support a narrow claim:

> Some of the developmental path by which a foreign conclusion was earned can be useful public evidence in its own right. A receiver can use bounded consequence history to calibrate permission without confusing that history with direct local experience.

It would not establish a general teaching protocol, cultural transmission mechanism, language, pedagogy, or final CPA memory format.
