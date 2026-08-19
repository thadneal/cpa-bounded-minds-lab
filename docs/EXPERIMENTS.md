# Experiments

## Protocol 01: local versus shared memory contamination

Name: `01-local-shared-memory-contamination`

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
