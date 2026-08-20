# Protocol 09 development result

Protocol: `09-authority-ancestry-circular-standing`

Evidence status: **development-v1, consumed for development; exact assay frozen in v0.14**

Seeds: `101, 211, 307, 401, 503`

Result: **5 Support / 0 Mixed / 0 Disconfirm**, with **50/50 preregistered checks passing**.

This result is strong enough to justify freezing the exact experiment and world-generator sources and moving to a fresh Protocol 09 holdout plus controlled operating-envelope falsification. It is still development evidence because the five canonical seeds were the known regression/development set.

## Mean results

| Metric | Authority ancestry | Recursive endorsement | Direct only |
| --- | ---: | ---: | ---: |
| Total RMSE | 0.17860 | 0.20144 | 0.20207 |
| Early independently grounded RMSE | 0.19597 | 0.17073 | 0.44657 |
| Early circular-trap RMSE | 0.53508 | 0.65838 | 0.44054 |
| Late circular-trap RMSE | 0.03656 | 0.03656 | n/a |
| Initial independently grounded authority | 0.61060 | 0.98000 | n/a |
| Initial circular authority | 0.22327 | 0.88673 | n/a |
| Final independently grounded standing | 0.88382 | n/a | n/a |
| Final circular standing | 0.02328 | 0.02328 | n/a |
| Public packets | 540 | 540 | 0 |
| Communication work | 3.24 | 3.24 | 0 |

The recursive control began with mean circular peer standing of only `0.11874`, yet after repeated locally reasonable endorsements the receiver saw mean circular authority of `0.88673`. That is a mean amplification of about **7.47x without new direct grounding**.

The ancestry-sensitive path held the same one-root circular contexts to mean initial authority of `0.22327` while preserving mean initial authority of `0.61060` in independently grounded contexts. Mean root coverage remained `3.35` in grounded contexts and `1.00` in circular traps.

## Per-seed whole-history RMSE

| Seed | Authority ancestry | Recursive endorsement | Direct only |
| ---: | ---: | ---: | ---: |
| 101 | 0.17725 | 0.19885 | 0.21179 |
| 211 | 0.17842 | 0.19916 | 0.20629 |
| 307 | 0.17722 | 0.19938 | 0.19697 |
| 401 | 0.18804 | 0.21320 | 0.21442 |
| 503 | 0.17208 | 0.19663 | 0.18086 |

Authority ancestry beat recursive endorsement in every development seed. It also beat direct-only learning in every seed, although the margin against direct-only was small in seed 503. That is useful because the result is not a trivial dominance story: direct-only still had the lowest early circular-trap error (`0.44054`) because it refused the bad social prior entirely.

## What the development result supports

### Permission has ancestry distinct from evidence ancestry

Protocol 06 showed that several factual reports can secretly descend from one observation. Protocol 09 demonstrates a different composition failure: several minds can be distinct, each transfer can be locally reasonable, and the network can still manufacture apparent authority by recirculating permission that traces back to one weak root.

The failing object is not duplicated evidence. It is duplicated **standing to influence**.

### Authority ancestry protects before consequence arrives

The ancestry-sensitive path reduced early circular-trap RMSE from `0.65838` under recursive endorsement to `0.53508`, about an 18.7% reduction. It still performed worse than direct-only in that early circular region because accepting any bad social prior has a cost.

This distinction matters. The mechanism is useful because it preserves social opportunity while limiting recursively manufactured authority, not because it makes social cognition universally safer than refusing social influence.

### Independent grounding remains useful

In independently grounded contexts, ancestry-sensitive early RMSE was `0.19597` versus `0.44657` for direct-only learning, a reduction of about 56%. The mechanism therefore did not solve circularity by flattening all social standing.

Recursive endorsement was even faster in this favorable region (`0.17073`) because it granted nearly maximal social authority. That is the visible price of protection: ancestry gives up some best-case acceleration.

### Receiver-owned consequence remains sovereign

Late circular error fell to about `0.03656`, and final circular standing fell to about `0.02328`. Independently grounded standing ended near `0.88382`.

Authority ancestry acts as a prior over permission. It does not replace later local consequence.

## Revised development conclusion

> Locally reasonable social endorsements can compose into globally unjustified authority when the ancestry of permission is lost. Compact ancestry information can preserve useful independently grounded social opportunity while limiting recursive authority amplification, but social openness still carries opportunity cost and later receiver-owned consequence remains necessary.

The bit sketches, five-peer ring, fixed rounds, standing equations, and thresholds remain laboratory instruments.

## Next evidence

Version 0.14 freezes the exact Protocol 09 experiment and world-generator files before any holdout outcome is observed. It registers a fresh twenty-seed `p09-holdout-v1` and a separate six-surface `authority-ancestry-falsification-v1` operating-envelope phase.

Run the holdout first and preserve its artifact. Only then run the exploratory falsification surfaces. Do not modify the frozen Protocol 09 mechanism between those two phases.
