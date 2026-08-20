# holdout-v1 Results and Revised Conclusions

Status: **consumed 2026-08-20**

Source run: frozen Protocol 01-07 catalog on the preregistered twenty-seed `holdout-v1` set. No protocol mechanism, world generator, or preregistered threshold was changed before this run.

## Aggregate result

```text
140 protocol runs
121 Support
19 Mixed
0 Disconfirm
0 Inconclusive

1000 assertions
981 passed
19 failed
```

| Evidence category | Passed | Checks | Failed |
| --- | ---: | ---: | ---: |
| mechanism outcome | 399 | 400 | 1 |
| safety boundary | 288 | 300 | 12 |
| manipulation | 174 | 180 | 6 |
| accounting constraint | 120 | 120 | 0 |

The category split is the primary result. The holdout did not reveal a broad collapse of the central mechanisms, but it did expose meaningful safety and assay boundaries that the five-seed development matrix had hidden.

## Protocol outcomes

| Protocol | Support | Mixed | Assertions passed | Assertions failed |
| --- | ---: | ---: | ---: | ---: |
| P01 local/shared memory contamination | 20 | 0 | 120 | 0 |
| P02 peer disagreement / preserved interiors | 20 | 0 | 120 | 0 |
| P03 developmental vs doctrinal transfer | 19 | 1 | 139 | 1 |
| P04 bounded communication before language | 19 | 1 | 139 | 1 |
| P05 emergent convention / artificial culture | 13 | 7 | 133 | 7 |
| P06 incomplete epistemic ancestry | 16 | 4 | 156 | 4 |
| P07 provisional standing transfer | 14 | 6 | 174 | 6 |

## P01 and P02

Both returned 20/20 Support, but these protocols predate the v0.3 correction that made seed vary the lived circumstance. Their holdout seeds still mainly perturb schedule/noise. Treat them as useful mechanism demonstrations and regression baselines, not strong evidence of broad developmental generalization.

## P03 revised conclusion

P03 returned 19 Support / 1 Mixed. Seed 5303 crossed the actual whole-history mechanism boundary: developmental RMSE was approximately `0.18248` versus `0.18145` for doctrine.

The same world still preserved the intended unstable-history calibration advantage. Across the holdout, developmental context remained useful, but the stronger conclusion "developmental transfer always beats doctrine" is rejected.

Carry-forward pressure:

> Developmental history can be useful calibration evidence about transferred structure. It is not guaranteed to justify its additional cost in every history.

## P04 revised conclusion

P04 returned 19 Support / 1 Mixed. The Mixed world missed the manipulation requirement for sufficient initial public disagreement; the typed mechanism still outperformed smoothing.

The frozen typed-vs-smoothing comparison remained unusually robust. This is useful evidence, but not yet a strong general claim about communication form because the semantic-smoothing control is deliberately destructive and consumes more communication work.

Carry-forward pressure:

> Preserve enough epistemic/source shape to avoid premature collapse while the world is unresolved.

Unresolved test:

> Compare against a stronger equal-budget negotiation/communication alternative rather than only semantic smoothing.

## P05 revised conclusion

P05 returned 13 Support / 7 Mixed. Five Mixed worlds failed the intended preference-diversity manipulation. Two substantive failures retained only `87.5%` of stable conventions, below the frozen `90%` safety boundary.

The central compression result remained strong: convention still reduced communication substantially while staying close to fresh-negotiation utility, and adaptive culture remained far better than frozen culture after changed conditions.

Carry-forward pressure:

> Cultural compression must balance revisability against retention. A mechanism that can reopen convention must also avoid gratuitous churn in unchanged contexts.

## P06 revised conclusion

P06 returned 16 Support / 4 Mixed. All four substantive failures were safety failures on genuine independent convergence. The ancestry mechanism continued to reduce echo amplification and whole-history error strongly, but it occasionally discounted real independence too aggressively.

Carry-forward pressure:

> Ancestry should be uncertain evidence of dependence that softly modulates corroboration. It should not become hard deduplication or categorical equivalence.

The hidden-root oracle remains an evaluator calibration reference rather than an achievable architectural assumption.

## P07 revised conclusion

P07 returned 14 Support / 6 Mixed. Five worlds exceeded the frozen 5% aggregate opportunity-cost allowance relative to refusing standing transfer. One world left strong locally mismatched standing around `0.279`, above the `0.20` revocation boundary, even though prediction had largely corrected.

The useful part of social transfer survived: provisional standing improved early access where the recommendation generalized and remained much safer than inherited authority where it did not.

Carry-forward pressure:

> Second-hand standing may buy conditional opportunity, but it carries real cost and must remain locally revocable. Current prediction accuracy is not sufficient evidence that latent social permission has been corrected.

## Challenge-slice result

| Challenge slice | Matching runs | Support | Mixed |
| --- | ---: | ---: | ---: |
| P03 high source instability | 9 | 9 | 0 |
| P04 dense conflicting social evidence | 20 | 19 | 1 |
| P05 high regime shift | 2 | 1 | 1 |
| P06 weak ancestry visibility | 15 | 11 | 4 |
| P07 fragile recommender transfer | 9 | 4 | 5 |

P05 stress coverage was too thin for a strong envelope conclusion. P06 and especially P07 produced useful boundary pressure.

## Lab-level conclusion

The holdout increased confidence in the laboratory as a mechanism-discovery instrument because it broke the perfect-Support pattern without revealing obvious data duplication or experiment leakage. It does **not** validate CPA as a complete cognitive architecture.

The evidence now supports carrying forward a small set of behavioral pressures while leaving the local equations, constants, thresholds, packet formats, and control-specific algorithms behind as experimental instruments.

The next phase is `challenge-v1`: outcome-blind adversarial stress over frozen P03-P07 world descriptors. `holdout-v1` is consumed and may only be rerun for reproducibility.
