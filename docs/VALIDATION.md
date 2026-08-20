# Validation and Operating-Envelope Method

Version 0.13.0 keeps Protocols 01-08 frozen, records the completed Protocol 08 holdout and strategic falsification as consumed evidence, and opens Protocol 09 authority ancestry / circular standing as development work.

The evidence sets have different jobs:

- `development-v1` is construction and regression data;
- `holdout-v1` is the consumed Protocol 01-07 frozen holdout;
- `challenge-v1` is consumed adversarial exploration;
- `parameterized-falsification-v1` is consumed controlled causal exploration for P03-P07;
- `p08-holdout-v1` is the consumed fresh Protocol 08 holdout;
- `strategic-influence-falsification-v1` is consumed controlled operating-envelope evidence for P08;
- Protocol 09 begins again at development-v1 and must earn a fresh holdout only after its development result is interpreted and its exact assay is frozen.

## Why the validation method changed

The canonical development matrix originally produced Support for every protocol and every seed. Inspection did not reveal duplicated or corrupted data, and many near-overlapping plots were explained by paired treatments sharing the same generated world. The methodological problem was that the five canonical seeds had been repeatedly used during implementation sanity checks. They are therefore development data even when thresholds were written before the user's run.

Version 0.8 froze Protocols 01-07 and their world generators, registered twenty unused seeds, separated evidence categories, and ran the complete set without retuning. Later versions added adversarial seed selection and then controlled causal surfaces. Protocol 08 repeated the improved cadence: development, exact-source freeze, unused holdout, then controlled falsification.

## Frozen source boundary

`docs/FROZEN_PROTOCOL_SHA256.txt` contains SHA-256 hashes for **eight experiment files and eight world generators**, Protocols 01-08. `scripts/verify.ps1` and `scripts/verify.sh` check those hashes before build/self-test.

Version 0.13.0 leaves all sixteen files byte-for-byte unchanged. Protocol 09 is new development work and is deliberately outside the frozen Protocol 01-08 boundary until its development result is reviewed.

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
- source uncertainty, channel uncertainty, context generalizability, and participation eligibility should not be silently collapsed into one standing scalar.

The exact standing caps, learning rates, packet costs, thresholds, bit sketches, grouping rules, and control implementations remain assay instruments.

## Protocol 09 development evidence cadence

Protocol 09 asks whether **permission itself** can acquire apparently independent warrant by recursively circulating through a network of locally reasonable endorsements. This is distinct from P06 evidence ancestry and P08 strategic self-presentation.

The first authoritative run is development-v1 only:

```powershell
./scripts/verify.ps1

dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --experiment 09-authority-ancestry-circular-standing `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-09-development-v1
```

A clean five-seed result is **development evidence**, not validation. Do not register or inspect a P09 holdout until the development result has been interpreted and the exact Protocol 09 experiment/world files have either been frozen or the protocol has been judged redundant.

## Invariant suite

Version 0.13.0 defines **37 self-tests**. Existing validation, challenge, parameterized-falsification, and P08 validation/falsification integrity checks remain. Two new Protocol 09 tests verify that:

- the canonical development seeds generate distinct authority-cascade worlds with the required independent-grounding and circular-trap structure;
- the seed-101 Protocol 09 development fixture satisfies the preregistered synthetic contract.

Those are development/assay-integrity checks. They are not fresh evidence for the Protocol 09 hypothesis.

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
```

## Interpretation rule

Do not tune a frozen mechanism from a consumed holdout or falsification surface and then describe the same evidence as validation. If a mechanism is revised and later deserves confirmation, register a new future holdout before seeing its outcomes.
