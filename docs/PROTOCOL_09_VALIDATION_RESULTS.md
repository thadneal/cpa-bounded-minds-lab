# Protocol 09 validation and falsification results

Protocol: `09-authority-ancestry-circular-standing`

Evidence status: **frozen and consumed**

The exact Protocol 09 experiment and world-generator sources were frozen in v0.14.0 before the holdout was executed. The registered holdout was then preserved before the controlled falsification surfaces were interpreted. Version 1.0.0 records the result without changing the frozen Protocol 09 mechanism.

## Fresh holdout

The preregistered `p09-holdout-v1` set contained twenty previously unused seeds:

```text
70111, 71429, 72817, 74209, 75679, 77101, 78593, 80021, 81517, 83003,
84521, 86011, 87539, 89051, 90617, 92141, 93703, 95279, 96821, 98411
```

It returned:

```text
20 / 20 Support
200 / 200 preregistered checks passed

mechanism outcome   100 / 100
safety boundary       40 / 40
manipulation          40 / 40
accounting            20 / 20
```

Mean metrics across the twenty unseen histories:

```text
authority-ancestry total RMSE              0.178808
recursive-endorsement total RMSE           0.201173
direct-only total RMSE                     0.201826

authority-ancestry early grounded RMSE     0.187457
recursive early grounded RMSE              0.164251
direct-only early grounded RMSE            0.427408

authority-ancestry early circular RMSE     0.523200
recursive early circular RMSE              0.646693
direct-only early circular RMSE            0.429927

authority-ancestry initial grounded auth   0.614592
authority-ancestry initial circular auth   0.222243
recursive initial circular authority       0.891651
recursive circular initial peer standing   0.118401
recursive circular amplification           7.533969 x

authority-ancestry final grounded standing 0.891559
authority-ancestry final circular standing 0.026415
recursive final circular standing          0.026415
```

The holdout reproduces the development-stage phenomenon rather than merely clearing thresholds. Recursive endorsement turns weak initial circular standing into high apparent authority without adding independent grounding. Authority ancestry sharply limits that amplification while preserving substantial opportunity from independently grounded peers. Receiver-owned consequence later corrects both social paths.

The tradeoff remains visible. In early circular contexts, direct-only learning is safer than accepting a bad social prior. In early grounded contexts, both social paths gain a large opportunity advantage over direct-only learning. Authority ancestry therefore does not dominate every local condition. Its value is the compromise between useful social openness and resistance to recursively manufactured legitimacy.

`p09-holdout-v1` was consumed on 2026-08-20. Future executions are reproducibility checks only.

## Controlled falsification

`authority-ancestry-falsification-v1` then executed six registered `7 x 7` surfaces with seven deterministic replicates per cell:

```text
6 profiles
294 controlled cells
2,058 deterministic replicate runs
```

Negative margins are boundary evidence, not failed validation counts. The probes deliberately include benign circularity and all-grounded conditions so ancestry caution can become harmful.

### Grounding diversity x peer trust

```text
negative mean cells       0 / 49
negative-replicate cells  0 / 49
replicate margin range    0.068498 to 0.220540
```

Across the entire registered surface, independently earned roots remained distinguishable from one weak root circulating through trusted peers. Increasing peer trust did not erase the value of grounding diversity.

Carry forward:

> Social repetition and independent authority grounding are different sources of warrant. Network confidence should preserve that distinction even when local peer trust is high.

The probe uses explicit root diversity as an intervention. It does not imply that a mature system will possess perfect root identity.

### Circulation depth x ancestry fidelity

```text
negative mean cells       39 / 49
negative-replicate cells  39 / 49
replicate margin range    -0.459000 to 0.113477
```

At one, two, and four social rounds, ancestry protection did not earn its cost anywhere on the registered fidelity range because recursive amplification had not yet become severe enough. At six rounds, only exact ancestry produced a positive primary margin. At eight, ten, and twelve rounds, fidelity of roughly `0.70` or greater produced positive margins while fidelity `0.50` or lower remained negative.

This is a useful limitation rather than a collapse of P09. Authority ancestry becomes valuable when recurrence is deep enough to manufacture apparent independence, and the lineage representation must preserve enough structure for that depth.

Carry forward:

> The required fidelity of authority ancestry should scale with circulation depth. Exact genealogy is not the only useful regime, but weak lineage sketches become insufficient as permission recirculates.

### Circular-root strength x receiver mismatch

```text
negative mean cells       29 / 49
negative-replicate cells  34 / 49
replicate margin range    -0.035607 to 0.037285
```

Ancestry discounting was net harmful in low-mismatch regions and generally became useful once receiver mismatch reached about `0.55`. At the strongest tested root (`0.85`) and extreme mismatch (`1.10`), the combined margin became negative again, showing that root strength and mismatch can interact rather than reduce to one monotonic rule.

Carry forward:

> Circularity is evidence about dependence, not evidence of falsehood. A recursively transmitted source can still be useful, so ancestry should change claims of independence and permission rather than automatically invert trust.

### Consequence delay x circulation depth

```text
negative mean cells       7 / 49
negative-replicate cells  7 / 49
replicate margin range    -0.007286 to 0.067678
```

The only negative row was the shallowest two-round circulation condition, across every tested consequence delay. Four or more rounds stayed positive across delays from zero through fourteen same-context exposures.

Carry forward:

> Delayed consequence increases the developmental period during which social priors matter, but ancestry protection earns its cost only when recurrence has created enough amplification to defend against.

### Grounded consequence noise x delay

This is the all-grounded null-harm surface. It contains no circular trap.

```text
negative mean cells       7 / 49
negative-replicate cells  13 / 49
replicate margin range    -0.104751 to 0.228719
```

All mean cells remained positive through consequence noise `0.22`, across all registered delays. At noise `0.30`, all seven delay cells became negative.

Carry forward:

> Source reliability and consequence-channel reliability must remain distinct. When receiver-owned consequence becomes sufficiently noisy, social caution can become over-deterrence even when every source is genuinely grounded.

### Network closure x independent-root count

```text
negative mean cells       7 / 49
negative-replicate cells  7 / 49
replicate margin range    -0.012174 to 0.174514
```

With one effective independent root, every closure level was negative. With `1.5` or more effective independent roots, every tested closure level was positive.

Carry forward:

> Recurrent topology is not itself the central failure. The more durable variable in this probe is epistemic diversity: whether apparent social support traces back to genuinely distinct grounding.

The ring/chain intervention is deliberately minimal and should not be promoted into a theory of mature social-network topology.

## Revised Protocol 09 conclusion

> Locally reasonable permission transfers can compose into globally unjustified authority when they recursively circulate without new independent grounding. A bounded receiver can reduce that composition failure by preserving ancestry of permission, while still allowing independently grounded social authority to buy useful early opportunity. The protection has an operating envelope: it becomes worthwhile mainly under sufficient recurrence and mismatch, it requires lineage fidelity that grows with circulation depth, and it can become harmful when consequence is too noisy or circular information is actually useful.

The durable result is the distinction among **social repetition**, **independent grounding**, **lineage fidelity**, and **receiver-owned consequence**. The frozen bit sketches, scalar equations, thresholds, packet costs, and synthetic ring topology remain assay instruments.

Both `p09-holdout-v1` and `authority-ancestry-falsification-v1` are consumed evidence. They may be reproduced, but they cannot be reused as fresh confirmation after later mechanism changes.
