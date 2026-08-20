# Validation and Operating-Envelope Method

Version 0.10.0 keeps Protocols 01-07 frozen and moves the laboratory from consumed holdout/challenge evidence into controlled parameterized falsification.

The distinction matters:

- `development-v1` is construction/regression data;
- `holdout-v1` was the first frozen confirmatory set and is now consumed;
- `challenge-v1` is consumed adversarial exploration, not a second holdout;
- `parameterized-falsification-v1` is controlled causal exploration outside the frozen generator support, not confirmation.

## Why the validation method changed

The canonical development matrix produced Support for every protocol and every seed. Inspection did not reveal duplicated or corrupted data, and many near-overlapping plots were explained by paired treatments sharing the same generated world. The deeper methodological problem was that those five seeds had been repeatedly used during implementation sanity checks. They were therefore development data even when thresholds were nominally preregistered.

Version 0.8 froze Protocols 01-07 and their world generators, registered twenty previously unused seeds, separated evidence categories, and ran the complete set without retuning.

That holdout was consumed on 2026-08-20.

## Frozen source boundary

`docs/FROZEN_PROTOCOL_SHA256.txt` contains SHA-256 hashes for all seven experiment files and their seven world generators. `scripts/verify.ps1` and `scripts/verify.sh` check those hashes before build/self-test.

Version 0.10.0 leaves those files byte-for-byte unchanged. Challenge and parameterized-falsification tooling are added beside them rather than editing the frozen assays.

## Seed sets

### development-v1

```text
101, 211, 307, 401, 503
```

Purpose: deterministic regression and development comparison only.

### holdout-v1

```text
809, 977, 1201, 1429, 1693, 2017, 2371, 2741, 3163, 3581,
4001, 4441, 4871, 5303, 5741, 6211, 6673, 7121, 7603, 8089
```

Status: **consumed on 2026-08-20**.

Rerunning this set is useful for reproducibility. It is no longer fresh validation and must not be used to confirm a mechanism that has been changed after seeing its results.

## Holdout-v1 result

The frozen seven-protocol run produced:

```text
140 protocol runs
121 Support
19 Mixed
0 Disconfirm

1000 preregistered checks
981 passed
19 failed
```

Evidence categories:

| Category | Passed | Checks | Failed |
| --- | ---: | ---: | ---: |
| mechanism outcome | 399 | 400 | 1 |
| safety boundary | 288 | 300 | 12 |
| manipulation | 174 | 180 | 6 |
| accounting constraint | 120 | 120 | 0 |

Per protocol:

| Protocol | Support | Mixed | Main holdout lesson |
| --- | ---: | ---: | --- |
| P01 | 20 | 0 | Mechanism demonstration remained stable, but old seed semantics limit generalization value. |
| P02 | 20 | 0 | Same qualification as P01; holdout mostly perturbs ordering/noise. |
| P03 | 19 | 1 | Developmental context helps calibration but is not guaranteed to beat doctrine on whole-history error. |
| P04 | 19 | 1 | Frozen typed-vs-smoothing mechanism remained robust; the only Mixed run missed the intended manipulation strength. |
| P05 | 13 | 7 | Cultural revisability can leak into contexts that should remain stable; plasticity and retention compete. |
| P06 | 16 | 4 | Ancestry inference can suppress genuinely independent evidence when dependence is inferred too aggressively. |
| P07 | 14 | 6 | Provisional standing has real opportunity cost and can leave excessive residual authority under fragile transfer. |

The challenge slices from the holdout were informative but unevenly populated. In particular, P05 high-regime-shift matched only two worlds. P06 weak-ancestry-visibility and P07 fragile-recommender-transfer were much more useful because they produced substantial Mixed rates.

## Revised implementation conclusions

The holdout supports carrying forward behavioral pressures, not the protocol-local formulas.

Reasonable implementation pressures now include:

- foreign evidence remains distinguishable from lived consequence;
- private developmental histories are not collapsed merely because minds communicate;
- transferred conclusions may carry bounded information about how they were earned;
- public communication should preserve enough epistemic shape and source distinction to avoid indiscriminate smoothing;
- convention may compress coordination, but must balance revisability against stable retention;
- ancestry is uncertain evidence about dependence, not a hard duplicate identity relation;
- second-hand standing can grant conditional opportunity, but durable authority must remain answerable to local consequence.

The exact standing caps, learning rates, packet costs, thresholds, grouping heuristics, and control implementations remain experimental instruments.

## Check taxonomy

Assertions continue to be classified as:

1. **manipulation**: did the generated world/control actually contain the intended pressure?
2. **mechanism outcome**: did the proposed behavior produce the predicted functional effect?
3. **safety boundary**: did the mechanism avoid the failure mode that would make the apparent gain unsafe or brittle?
4. **accounting constraint**: did communication or compute remain inside the explicit budget?

These categories do not change a frozen protocol verdict. They change how much evidential weight should be assigned to different passing checks.

## challenge-v1

`challenge-v1` is now **consumed exploratory evidence**. The frozen challenge returned 78 Support / 22 Mixed / 0 Disconfirm across 100 adversarial runs. Category results were 317/320 mechanism, 207/220 safety, 133/140 manipulation, and 78/80 accounting checks.

The most important methodological result was that the composite descriptor rankings were not reliably monotonic causal stress variables. Several descriptor components could strengthen the mechanism they were intended to challenge. P04 was the clearest example: more conflict density often increased the value of preserving typed epistemic shape.

The profile-specific conclusions and exact failure examples are preserved in `CHALLENGE_V1_RESULTS.md`. `challenge-v1` can be reproduced, but it must not be used as a new holdout or as confirmation after mechanism changes.

## parameterized-falsification-v1

Version 0.10 replaces composite seed ranking with controlled causal intervention. This is **not validation** in the confirmatory sense. It is exploratory falsification designed to map null, useful, crossover, and harmful regions.

The runner does not change frozen Protocol 01-07 experiment or world-generator source. Instead, protocol-local micro-assays copy selected frozen equations into independent controlled probes so causal variables can be separated and pushed beyond the support of the original generated worlds.

Registered surfaces:

```text
P03  history instability      x present rule error
P04  warrant asymmetry        x minority-correct fraction
P05  change frequency         x change magnitude
P06  origin missingness       x signature separation
P07  recommender credibility  x mismatch prevalence
P07  recommender credibility  x strong mismatch severity
```

Each surface is 7 x 7 with seven deterministic replicates per cell. P04 uses the same public estimate/standing/uncertainty packets and equal communication cost in both arms, strengthening the comparator without promoting the comparator into CPA architecture.

P07 has two surfaces because challenge-v1 showed that opportunity cost and residual authority are distinct failure modes. The severity axis starts at strong mismatch rather than zero, because the frozen residual-standing ceiling applies to strong local contradiction.

A negative primary margin is useful boundary evidence. It does not automatically refute the entire originating protocol, because the micro-assay intentionally isolates one local mechanism from the full protocol ecology.

### Confirmation rule after parameterized falsification

If a frozen mechanism is later revised in response to these surfaces, none of `development-v1`, `holdout-v1`, `challenge-v1`, or `parameterized-falsification-v1` can serve as fresh confirmation of the revision. A future validation set must be registered before its outcomes are examined.

## Automated outputs

The authoritative current falsification command writes:

```text
parameterized-plan.json
parameterized-report.json
parameterized-summary.md
p03-history-informativeness.csv
p04-equal-budget-comparator.csv
p05-volatility-surface.csv
p06-ancestry-opacity.csv
p07-reliability-prevalence.csv
p07-reliability-severity.csv
```

`parameterized-plan.json` records the registered axes, replicate count, primary margin, and interpretation limit. `parameterized-report.json` records every cell's mean/min/max margin, negative-replicate count, and diagnostic metrics. `parameterized-summary.md` presents the complete surface in human-readable form.

Consumed `challenge-v1` and `holdout-v1` keep their historical artifacts for reproduction. They are not rewritten by the parameterized runner.

## Invariant suite

Version 0.10.0 defines **30 self-tests**. In addition to the existing protocol, telemetry, validation, and challenge checks, parameterized-falsification tests verify that:

- challenge-v1 selection is deterministic;
- development-v1 and consumed holdout-v1 seeds are excluded;
- each profile contains all five registered bands with four unique seeds per band;
- selected stress is nondecreasing by band.
- all six registered parameterized profiles expose complete 7 x 7 grids;
- the P04 stronger comparator consumes exactly the same public communication work as the typed path;
- the P06 probe reaches complete origin blindness while retaining finite measurable outputs;
- the two P07 surfaces intervene on prevalence and severity separately.

These are assay-integrity checks. They do not assert that any parameterized cell should remain positive.

## Authoritative Windows sequence

```powershell
./scripts/verify.ps1

dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --falsify `
  --output _artifacts/parameterized-falsification-v1
```

To reproduce the consumed holdout without making a new validation claim:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --validation `
  --output _artifacts/validation-holdout-v1-repro
```

## Interpretation rule

Do not tune a frozen mechanism from challenge-v1 or parameterized-falsification-v1 and then describe the same evidence as validation. Both are developmental pressure. If a mechanism is revised and later deserves confirmation, register a new future holdout before seeing its outcomes.
