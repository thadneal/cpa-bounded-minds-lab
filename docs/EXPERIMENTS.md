# Experiments

## Protocol 01: local versus shared memory contamination

Name: `01-local-shared-memory-contamination`

Status: **frozen Supported baseline** after the accepted five-seed run (`101,211,307,401,503`), with all 30 preregistered component assertions passing.

### Question

Can one bounded mind gain useful prior structure from another without granting second-hand memory the authority of lived consequence?

### World

The source develops across eight abstract recurring context cells. After 80 direct observations of each cell, traces with sufficient standing may be published.

The receiver later lives in a related environment:

- cells 0-5 preserve the source relationship;
- cells 6-7 differ from the source relationship.

Schedules are balanced but deterministically shuffled by seed.

### Public transfer

The source may publish only `PublicTracePacket` records containing:

```text
source mind id
origin id
context cell
compressed estimate
sender standing
sender evidence count
```

No receiver obtains the sender's private memory object.

### Paths

#### local-only

No transfer. The receiver learns exclusively from direct consequence.

This measures the price of refusing useful inheritance.

#### shared-provisional

The receiver imports the public packet as foreign memory. Initial standing is capped. Direct local experience begins its own local trace rather than pretending the imported evidence was lived locally.

Foreign standing grows when later local consequence confirms the estimate and falls sharply when local consequence contradicts it.

#### shared-lived-equivalent

Control path. The same packet enters local authority with the source's standing and accumulated evidential inertia. It can still adapt, but adaptation is slower because the architecture acts as if another mind's history were the receiver's own history.

This path is intentionally doctrinal. It exists to make contamination visible.

### Metrics

Live metrics include:

- `rolling_rmse`;
- `absolute_error`;
- `prediction` and `target`;
- `local_standing` and `foreign_standing`;
- `mean_foreign_standing`;
- `direct_evidence`;
- `communication_work`;
- `context_cell` and a scoring-only compatibility indicator.

Final metrics separately report early compatible error, early divergent error, late divergent error, final compatible and divergent foreign standing, and total communication work.

### What success would mean

A successful result would support a narrow claim: second-hand developmental structure can be useful when it remains second-hand long enough for local consequence to govern its authority.

It would not establish a mature shared-memory system, a social trust model, a communication language, or a final CPA memory architecture.

---

## Protocol 02: peer disagreement with preserved interiors

Name: `02-peer-disagreement-preserved-interiors`

### Question

Does preserving independent private histories improve later correction when bounded peers disagree, compared with collapsing their state into synchronized consensus?

### World

Two peers encounter the same eight abstract context cells but develop under different private histories. The histories are deliberately complementary for this first assay: for every cell, one peer receives observations near the later shared relationship while the other receives a coherent but conflicting local relationship. Small deterministic observation noise and shuffled schedules vary by seed.

This world is intentionally clean. Protocol 02 asks whether useful independent error structure can matter at all before later protocols add ambiguous overlap, unreliable peers, ancestry uncertainty, or strategic behavior.

### Conditions

#### preserved-interiors

The peers keep their private predictive states separate. During the later shared-consequence phase, each peer exposes only a compact public posture for the current context:

```text
prediction
revisable public standing
```

A deliberately simple experimental negotiation reducer combines those two public postures. Shared consequence is then returned separately to each peer, changing its own estimate and public standing. The reducer is an assay instrument, not a proposed central cognitive organ.

#### synchronized-control

Before the same shared observations begin, an invasive experimental control averages the two private states into one consensus and copies that consensus into both peers. The peers then receive exactly the same shared consequence stream and use the same public negotiation rule.

The control intentionally destroys the disagreement produced by their different histories. It is a contrast condition, not proposed architecture.

### Metrics

Private development exposes:

- `rolling_rmse`;
- `prediction` and `target`;
- `local_standing`;
- `private_evidence`;
- `context_cell`.

Shared-consequence paths expose:

- `rolling_rmse` and `absolute_error`;
- `peer_a_prediction` and `peer_b_prediction`;
- `peer_disagreement`;
- each peer's public standing;
- `best_peer_absolute_error`;
- `communication_work`.

Final metrics report early, late, and lifetime RMSE, initial/final disagreement, early best-peer error, public packet count, and communication work.

### What success would mean

A successful result would support a narrow claim: when bounded minds have developed different error structures, preserving those interiors can keep corrective alternatives available long enough for later shared consequence to decide which local model deserves influence.

Success also requires later convergence. Permanent disagreement would not count as preserved epistemic value. The point is to retain alternative structure long enough to be tested, not to canonize pluralism.

Protocol 02 does not establish a mature negotiation system, social trust, collective intelligence, culture, or a final CPA coalition mechanism.
