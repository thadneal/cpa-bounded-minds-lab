# Validation

Version 0.8.0 begins a validation phase for the frozen Protocol 01-07 mechanism-discovery set. No Protocol 08 is added in this release.

## Why the phase changed

The first seven protocols all returned Support on the canonical five-seed matrix. Those runs remain useful mechanism-discovery evidence, but the seeds were repeatedly used during design and implementation sanity checks. They are therefore development data rather than fresh holdout validation.

Version 0.8.0 makes that distinction explicit and adds tooling that treats all-Support and all-pass outcomes as possible assay-sensitivity warnings rather than automatic confirmation.

## Frozen source boundary

Protocol 01-07 experiment and world source files are frozen at their v0.7.0 contents. Their SHA-256 values are recorded in:

```text
docs/FROZEN_PROTOCOL_SHA256.txt
```

Both `scripts/verify.ps1` and `scripts/verify.sh` check this manifest before building. A validation result is not comparable if a frozen protocol/world file changed unnoticed.

## Seed sets

### development-v1

```text
101, 211, 307, 401, 503
```

Use this set for deterministic regression and historical comparison only. It is not fresh validation.

### holdout-v1

```text
809, 977, 1201, 1429, 1693, 2017, 2371, 2741, 3163, 3581,
4001, 4441, 4871, 5303, 5741, 6211, 6673, 7121, 7603, 8089
```

These twenty seeds were registered in source before their Protocol 01-07 outcomes were inspected. Do not tune protocol mechanics or preregistered thresholds from holdout-v1 and then reuse holdout-v1 as confirmation. If the holdout causes a mechanism change, a future validation claim requires a new holdout set.

Protocols 01 and 02 predate the v0.3 seed-as-lived-circumstance correction. New seed values mostly perturb order/noise there, so their holdout evidence is weaker than Protocols 03-07. The validation report always carries this limitation.

## Check taxonomy

The protocol verdicts remain unchanged, but v0.8 reports assertion evidence in four categories:

- `manipulation`: the world/control actually contains the intended experimental condition;
- `mechanism-outcome`: the proposed behavior produces the functional effect under test;
- `safety-boundary`: the behavior remains selective, revisable, bounded, or avoids the failure mode under pressure;
- `accounting-constraint`: packet/work/resource accounting remains within the explicit budget.

This classification is reporting metadata only. It does not alter any frozen protocol threshold or verdict rule.

## Challenge slices

The first holdout pass preregisters five stress slices over world descriptors already emitted by the frozen protocols:

| Slice | Protocol | Rule |
| --- | --- | --- |
| High source instability | P03 | `unstable_transition_cells >= 4` OR `sparse_ambiguous_cells >= 2` |
| Dense conflicting social evidence | P04 | dissent cells >= 7 OR `private_evidence_span >= 35` |
| High regime shift | P05 | `shifted_contexts >= 6` |
| Weak ancestry visibility | P06 | `missing_origin_rate >= 0.45` OR `immediate_sender_hint_rate >= 0.28` |
| Fragile recommender transfer | P07 | `recommender_credibility <= 0.70` OR `strong_local_mismatch_contexts >= 4` |

These are filters, not treatments. They identify stressful subsets of the holdout histories without changing the world, receiver, thresholds, or communication budget after outcomes are known. If fewer than three holdout runs match a slice, the report flags the coverage as too thin for a strong conclusion.

## Automated outputs

Every completed Desktop session now writes:

```text
replication-report.json
validation-report.json
validation-summary.md
session-manifest.json
seed-*/...
```

`validation-report.json` records the seed-set classification, per-protocol verdict counts, assertion-category totals, challenge-slice outcomes, and diagnostics. `validation-summary.md` is a compact human-readable rendering of the same validation metadata.

The report emits explicit diagnostics when:

- every protocol run returns Support;
- every assertion passes;
- the session is a development or custom seed set rather than holdout-v1;
- only part of the frozen protocol catalog was run;
- a challenge slice has insufficient matching histories.

These diagnostics do not change protocol verdicts. They change how much confidence should be assigned to them.

## Invariant suite

Version 0.8.0 defines **23 self-tests**. The existing seed-101 protocol tests are retained as **development regression fixtures**, not evidence that a fresh validation seed should Support. New checks verify:

1. development-v1 and holdout-v1 remain unique, frozen, and disjoint;
2. representative assertions are separated into the intended evidence categories;
3. a validation report correctly labels the canonical five-seed set as development data and tallies mechanism outcomes separately.

## Authoritative Windows validation sequence

The source-generation environment used to prepare this package does not provide the .NET 10 SDK, so the Windows environment remains authoritative for compilation and analyzers.

```powershell
./scripts/verify.ps1
```

This verifies frozen source hashes, builds Release, and runs all 23 self-tests.

Then run the holdout exactly as registered:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --validation `
  --output _artifacts/validation-holdout-v1
```

Alternatively, launch the Desktop Lab. It now defaults to **Holdout v1 (20, frozen)**, all seven frozen protocols selected, and Maximum pace.

## Interpretation rules for this phase

Do not require 20/20 Support as the definition of success. A Mixed or Disconfirm result can be useful if it identifies where an otherwise valuable mechanism stops working.

Review in this order:

1. confirm frozen hashes, seed set, completion, and world/manipulation checks;
2. inspect mechanism-outcome and safety-boundary failures separately from accounting failures;
3. inspect challenge-slice results and coverage;
4. examine effect size and failure shape, not only the categorical verdict;
5. treat 100% Support as a reason to examine assay sensitivity more closely;
6. only then decide whether a mechanism looks robust enough to inform the eventual CPA substrate.

The goal of v0.8 is an **operating envelope**, not another row of green verdicts.
