# Validation and Operating-Envelope Method

Version 0.9.0 keeps Protocols 01-07 frozen and moves the laboratory from first holdout validation into an adversarial operating-envelope phase.

The distinction matters:

- `development-v1` is construction/regression data;
- `holdout-v1` was the first frozen confirmatory set and is now consumed;
- `challenge-v1` is intentionally adversarial exploration, not a second holdout.

## Why the validation method changed

The canonical development matrix produced Support for every protocol and every seed. Inspection did not reveal duplicated or corrupted data, and many near-overlapping plots were explained by paired treatments sharing the same generated world. The deeper methodological problem was that those five seeds had been repeatedly used during implementation sanity checks. They were therefore development data even when thresholds were nominally preregistered.

Version 0.8 froze Protocols 01-07 and their world generators, registered twenty previously unused seeds, separated evidence categories, and ran the complete set without retuning.

That holdout was consumed on 2026-08-20.

## Frozen source boundary

`docs/FROZEN_PROTOCOL_SHA256.txt` contains SHA-256 hashes for all seven experiment files and their seven world generators. `scripts/verify.ps1` and `scripts/verify.sh` check those hashes before build/self-test.

Version 0.9.0 leaves those files byte-for-byte unchanged. Challenge tooling is added beside them rather than editing the frozen assays.

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

`challenge-v1` is an outcome-blind adversarial search inside the frozen Protocol 03-07 world-generator families.

Candidate seeds are fixed to:

```text
10001-29999
```

The development and consumed holdout seeds are excluded. For each challenge profile, the candidate worlds are scored using **only scenario descriptors**. No experiment result, assertion, RMSE, standing outcome, or protocol verdict participates in seed selection.

The candidates are sorted by profile stress score, divided into five ordered rank bands, and four deterministic seeds are selected from each band. This gives:

```text
5 profiles
5 bands per profile
4 seeds per band
20 runs per profile
100 challenge runs total
```

Stress scores are profile-local ranks. A score from P03 must not be compared numerically with a score from P07.

### P03 source instability

Stress inputs include unstable/sparse history prevalence, transition magnitude, and thin minimum evidence.

Boundary margin:

```text
doctrinal_rmse - developmental_rmse
```

Positive means developmental transfer still has lower whole-history RMSE. Zero is the crossover.

### P04 conflict density

Stress inputs include informative/misleading dissent prevalence, private evidence span, and private-target spread.

Boundary margin:

```text
(semantic_smoothed_rmse * 0.97) - typed_rmse
```

Positive means typed communication still satisfies the frozen 3% whole-history advantage requirement.

Important limitation: this remains the frozen semantic-smoothing control. Challenge v1 does not yet add the stronger equal-budget negotiation alternative proposed after holdout review.

### P05 regime shift

Stress inputs include shifted-context count, mean post-formation cost movement, and preference diversity.

Boundary margin is the minimum of:

```text
earned_stable_retention_coverage - 0.90

earned_changed_revision_coverage - 0.85

earned_changed_shifted_late_utility - frozen_changed_shifted_late_utility - 0.20
```

Positive means the adaptive culture both retains enough unchanged convention and remains better than frozen culture after change.

### P06 ancestry visibility

Stress inputs include missing-origin rate, immediate-sender hints, ambiguous-lineage prevalence, and low root-signature separation.

Boundary margin is the minimum of:

```text
(naive_rmse * 0.88) - inferred_rmse

(naive_independent_rmse * 1.15) - inferred_independent_rmse
```

Positive means ancestry inference remains better overall and stays within the frozen 12% whole-history advantage and 15% independent-evidence safety allowances.

### P07 recommender fragility

Stress inputs include weak receiver-to-recommender credibility, strong local mismatch prevalence, mismatch magnitude, and standing behind those recommendations.

Boundary margin is the minimum of:

```text
(no_transfer_rmse * 1.05) - provisional_rmse

0.20 - provisional_final_strong_mismatch_standing
```

Positive means provisional transfer remains inside the frozen opportunity-cost and revocation safety boundaries.

## Challenge interpretation

`challenge-v1` is designed to fail.

A Mixed verdict, Disconfirm verdict, or negative boundary margin is useful evidence about the mechanism's operating envelope. A monotonic decline in boundary margin as stress rises is especially informative.

A non-monotonic curve is also evidence. It may mean the mechanism has a complex response, or it may mean the registered stress descriptor is not aligned with the actual causal pressure. The report warns when the highest stress band improves relative to the lowest.

If all five profiles remain entirely inside their boundaries, do not simply increase the number of random seeds and call that stronger validation. The correct next step would be a parameterized challenge that deliberately moves world conditions beyond the original frozen generator support.

## Automated outputs

The authoritative challenge command writes:

```text
challenge-plan.json
challenge-report.json
challenge-summary.md
p03-source-instability/
  q1-low/
    seed-N/
      frames.ndjson
      manifest.json
      03-developmental-versus-doctrinal-transfer/
        result.json
        metrics.csv
...
```

`challenge-plan.json` records the complete outcome-blind seed selection and descriptors before any result interpretation.

`challenge-report.json` and `challenge-summary.md` report stress bands, verdicts, evidence categories, signed boundary margins, and the first observed failure band.

## Invariant suite

Version 0.9.0 defines **26 self-tests**. In addition to the existing protocol, telemetry, and validation checks, challenge tests verify that:

- challenge-v1 selection is deterministic;
- development-v1 and consumed holdout-v1 seeds are excluded;
- each profile contains all five registered bands with four unique seeds per band;
- selected stress is nondecreasing by band.

These are assay-integrity checks. They do not assert that any challenge run should Support.

## Authoritative Windows sequence

```powershell
./scripts/verify.ps1

dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --challenge `
  --output _artifacts/challenge-v1
```

To reproduce the consumed holdout without making a new validation claim:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --validation `
  --output _artifacts/validation-holdout-v1-repro
```

## Interpretation rule

Do not tune a frozen mechanism from challenge-v1 and then describe the same challenge run as validation. Challenge results are developmental pressure. If a mechanism is revised and later deserves confirmation, register a new future holdout before seeing its outcomes.
