# Validation and Operating-Envelope Method

Version 0.14.0 freezes Protocols 01-09, records the coherent Protocol 09 development result, preregisters a fresh Protocol 09 holdout, and adds a separate controlled operating-envelope falsification phase.

The evidence sets have different jobs:

- `development-v1` is construction and regression data;
- `holdout-v1` is the consumed Protocol 01-07 frozen holdout;
- `challenge-v1` is consumed adversarial exploration;
- `parameterized-falsification-v1` is consumed controlled causal exploration for P03-P07;
- `p08-holdout-v1` is the consumed fresh Protocol 08 holdout;
- `strategic-influence-falsification-v1` is consumed controlled operating-envelope evidence for P08;
- `p09-holdout-v1` is the fresh, not-yet-consumed Protocol 09 holdout until its first `--p09-validation` execution;
- `authority-ancestry-falsification-v1` is preregistered controlled operating-envelope exploration for frozen P09 and should be run after the holdout artifact is preserved.

## Why the validation method changed

The canonical development matrix originally produced Support for every protocol and every seed. Inspection did not reveal duplicated or corrupted data, and many near-overlapping plots were explained by paired treatments sharing the same generated world. The methodological problem was that the five canonical seeds had been repeatedly used during implementation sanity checks. They are therefore development data even when thresholds were written before the user's run.

Version 0.8 froze Protocols 01-07 and their world generators, registered twenty unused seeds, separated evidence categories, and ran the complete set without retuning. Later versions added adversarial seed selection and then controlled causal surfaces. Protocol 08 repeated the improved cadence: development, exact-source freeze, unused holdout, then controlled falsification.

## Frozen source boundary

`docs/FROZEN_PROTOCOL_SHA256.txt` contains SHA-256 hashes for **nine experiment files and nine world generators**, Protocols 01-09. `scripts/verify.ps1` and `scripts/verify.sh` check those hashes before build/self-test.

Version 0.14.0 adds the exact Protocol 09 experiment and world-generator files to the frozen boundary after the canonical development result was reviewed. Holdout and falsification code are outside that mechanism boundary and may not alter the frozen P09 equations.

## Seed sets

### development-v1

```text
101, 211, 307, 401, 503
```

Purpose: deterministic regression and development comparison only.

### holdout-v1, Protocols 01-07

```text
809, 977, 1201, 1429, 1693, 2017, 2371, 2741, 3163, 3581,
4001, 4441, 4871, 5303, 5741, 6211, 6673, 7121, 7603, 8089
```

Status: **consumed on 2026-08-20**.

The frozen run produced 121 Support / 19 Mixed / 0 Disconfirm across 140 protocol runs, with 981/1000 preregistered checks passing. Category totals were 399/400 mechanism outcomes, 288/300 safety boundaries, 174/180 manipulation checks, and 120/120 accounting constraints.

### p08-holdout-v1

```text
41047, 42131, 43391, 44621, 45893,
47237, 48611, 49919, 51307, 52709,
54139, 55603, 57143, 58661, 60209,
61781, 63347, 64997, 66617, 68213
```

Status: **consumed on 2026-08-20**.

Protocol 08 returned **20/20 Support with 200/200 checks passing**. Mean total RMSE was `0.17873` for accountable consequence, `0.35608` for self-report naive, and `0.18599` for local-only. Accountable early aligned RMSE was `0.23478` versus `0.41033` local-only. Late divergent and betrayal RMSE were `0.03049` and `0.02973`, versus `0.41716` and `0.42072` in the naive condition.

The holdout does not imply social influence is free. Accountable treatment was worse than local-only in 5/20 histories, with the worst case remaining just inside the frozen 5% opportunity-cost allowance. See `PROTOCOL_08_VALIDATION_RESULTS.md`.

### p09-holdout-v1

```text
70111, 71429, 72817, 74209, 75679,
77101, 78593, 80021, 81517, 83003,
84521, 86011, 87539, 89051, 90617,
92141, 93703, 95279, 96821, 98411
```

Status: **fresh and unconsumed at source release time**.

These twenty seeds were registered only after the exact Protocol 09 experiment and world-generator files were frozen. They are disjoint from development-v1, the consumed Protocol 01-07 holdout, the consumed P08 holdout, and challenge-v1 selected seeds. The first `--p09-validation` execution consumes the set.

The holdout must be preserved before controlled P09 falsification is interpreted. If the holdout exposes a failure, do not modify Protocol 09 and rerun the same seeds as fresh evidence.

## Evidence taxonomy

Assertions are classified as:

1. **manipulation**: did the generated world/control contain the intended pressure?
2. **mechanism outcome**: did the proposed behavior produce the predicted functional effect?
3. **safety boundary**: did the mechanism avoid the failure mode that would make the apparent gain unsafe or brittle?
4. **accounting constraint**: did communication or compute remain inside the explicit budget?

These categories do not change a frozen protocol verdict. They prevent assay mechanics such as packet accounting from being psychologically counted as independent theoretical confirmations.

## Consumed exploratory evidence

### challenge-v1

The frozen challenge returned 78 Support / 22 Mixed / 0 Disconfirm across 100 adversarial runs. Category totals were 317/320 mechanism, 207/220 safety, 133/140 manipulation, and 78/80 accounting.

Its most important methodological result was negative: composite descriptor rankings were not reliably monotonic causal stress variables. Several descriptor components could strengthen the mechanism they were intended to challenge. The exact result is preserved in `CHALLENGE_V1_RESULTS.md`.

### parameterized-falsification-v1

The controlled P03-P07 phase produced **294 cells / 2,058 deterministic runs**. `176/294` cells had negative mean primary margins and `1,201/2,058` replicates crossed at least one registered boundary.

The durable revisions were behavioral rather than formula-level:

- P03: developmental instability is information about uncertainty, not an automatic scalar trust penalty;
- P04: preserve epistemic shape, but do not privilege one universal reducer;
- P05: convention can become stale while still performing well enough to reinforce itself;
- P06: ancestry needs positive and negative dependence evidence and should not collapse to hard identity grouping;
- P07: recommender credibility and local generalizability are distinct, while revocation has proportional cost and latency.

See `PARAMETERIZED_FALSIFICATION_V1_RESULTS.md`.

### strategic-influence-falsification-v1

After the fresh P08 holdout was consumed, five 7 x 7 surfaces with seven replicates per cell produced **245 cells / 1,715 controlled runs**. `128/245` cells had a negative mean primary margin. Component interpretation matters more than the raw negative-cell count.

The operating-envelope conclusions are:

- consequence delay progressively removes accountability's advantage over a manipulable receiver, but the tested accountable path generally degrades toward local-only rather than runaway capture;
- mild or late betrayal can leave authority sticky because revocation has finite latency and weak contradiction resembles ordinary variation;
- social openness becomes net costly when persistent objective divergence becomes common enough, suggesting peer eligibility/participation pressure distinct from context standing;
- public feedback is not intrinsically unsafe because it is also the channel through which consequence changes sender incentives;
- noisy consequence can lower standing for a genuinely aligned peer, so source reliability and consequence-channel reliability require distinct uncertainty.

This evidence is consumed. Do not revise the P08 receiver and then describe these same surfaces as confirmation.

## Revised implementation conclusions

The accumulated evidence supports carrying forward behavioral pressures, not protocol-local constants:

- foreign evidence remains distinguishable from lived consequence;
- private developmental histories do not collapse merely because minds communicate;
- developmental context can shape uncertainty without becoming doctrine;
- communication preserves enough epistemic type/source structure for later local aggregation;
- convention can compress recurring coordination, but stability and revisability are competing pressures;
- ancestry is uncertain evidence about dependence, not hard duplicate identity;
- second-hand standing can grant conditional opportunity, but durable authority remains answerable to local consequence;
- receiver-owned consequence can discipline strategically optimized public influence without inspecting another mind's private objective;
- source uncertainty, channel uncertainty, context generalizability, and participation eligibility should not be silently collapsed into one standing scalar;
- permission itself can carry ancestry: several locally reasonable endorsements may still derive their authority from one weak root, so independent authority grounding and social repetition must remain distinguishable.

The exact standing caps, learning rates, packet costs, thresholds, bit sketches, grouping rules, and control implementations remain assay instruments.

## Protocol 09 fresh-validation and falsification cadence

The canonical development result is preserved in `PROTOCOL_09_DEVELOPMENT_RESULTS.md`: **5/5 Support, 50/50 checks passing**, with mean recursive circular amplification of about 7.47x and a materially lower ancestry-sensitive circular authority surface. That development result justified freezing the exact assay.

Run the next evidence in this order:

```powershell
./scripts/verify.ps1

# First execution is fresh validation and consumes p09-holdout-v1.
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p09-validation `
  --output _artifacts/p09-holdout-v1

# Preserve and review the holdout artifact first. Then map the operating envelope.
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p09-falsify `
  --output _artifacts/authority-ancestry-falsification-v1
```

`authority-ancestry-falsification-v1` contains six 7 x 7 surfaces with seven deterministic replicates per cell, for **294 cells / 2,058 controlled runs** when executed. It deliberately includes null-harm and benign-circularity regions so ancestry caution can fail rather than being rewarded automatically.

## Invariant suite

Version 0.14.0 defines **41 self-tests**. Existing validation, challenge, parameterized-falsification, and P08 validation/falsification integrity checks remain. Protocol 09 checks verify the frozen development fixture, fresh holdout disjointness, six-surface falsification plan integrity, partial-ancestry numerical behavior, and the all-grounded null-harm probe.

These are assay-integrity checks. They are not fresh evidence for the Protocol 09 hypothesis and do not execute the p09 holdout.

## Reproducibility commands

Consumed evidence can still be reproduced, but not reclassified as fresh validation:

```powershell
# Protocols 01-07 holdout
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --validation --output _artifacts/validation-holdout-v1-repro

# Protocols 03-07 challenge
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --challenge --output _artifacts/challenge-v1-repro

# P03-P07 controlled surfaces
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --falsify --output _artifacts/parameterized-falsification-v1-repro

# Protocol 08 holdout
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-validation --output _artifacts/p08-holdout-v1-repro

# Protocol 08 controlled surfaces
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-falsify --output _artifacts/strategic-influence-falsification-v1-repro

# Protocol 09 fresh holdout (first execution only)
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p09-validation --output _artifacts/p09-holdout-v1

# Protocol 09 operating envelope after holdout preservation
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p09-falsify --output _artifacts/authority-ancestry-falsification-v1
```

## Interpretation rule

Do not tune a frozen mechanism from a consumed holdout or falsification surface and then describe the same evidence as validation. If a mechanism is revised and later deserves confirmation, register a new future holdout before seeing its outcomes.
