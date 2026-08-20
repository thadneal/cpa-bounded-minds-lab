# Protocol 08 validation and falsification record

Protocol 08 is now **frozen, independently validated, and characterized by controlled falsification**. Version 0.13 does not change its experiment or world generator.

## Frozen boundary

The exact v0.11 development sources remain covered by `docs/FROZEN_PROTOCOL_SHA256.txt`:

- `src/Cpa.BoundedMindsLab.Core/Experiments/StrategicPublicInfluenceExperiment.cs`
- `src/Cpa.BoundedMindsLab.Core/Environments/StrategicInfluenceWorld.cs`

## p08-holdout-v1: consumed

The twenty preregistered seeds were run once after the Protocol 08 source freeze:

```text
41047, 42131, 43391, 44621, 45893,
47237, 48611, 49919, 51307, 52709,
54139, 55603, 57143, 58661, 60209,
61781, 63347, 64997, 66617, 68213
```

Result: **20/20 Support, 200/200 preregistered checks passing**.

Key means:

```text
accountable total RMSE          0.17873
self-report-naive total RMSE    0.35608
local-only total RMSE           0.18599
accountable early aligned RMSE  0.23478
local-only early aligned RMSE   0.41033
accountable late divergent RMSE 0.03049
naive late divergent RMSE       0.41716
accountable late betrayal RMSE  0.02973
naive late betrayal RMSE        0.42072
final aligned standing          0.91084
final divergent standing        0.02557
final betrayal standing         0.02530
```

Accountable treatment was nevertheless worse than local-only in 5/20 histories. The worst remained just inside the frozen 5% opportunity-cost allowance. This is important evidence that social openness carries real cost rather than providing free benefit.

The holdout is consumed. Reruns are reproducibility only. See `PROTOCOL_08_VALIDATION_RESULTS.md`.

## strategic-influence-falsification-v1: consumed

Five 7 x 7 response surfaces with seven deterministic replicates per cell produced **245 cells / 1,715 controlled runs**. `128/245` cells had negative mean primary margins. Those negative cells are boundary evidence, not a new aggregate Protocol 08 verdict.

### Consequence delay x sender adaptation

Delay dominated sender adaptation speed. As direct consequence was delayed, accountability's large advantage over the naive receiver progressively disappeared. In the tested range the accountable path generally approached local-only performance rather than becoming catastrophically captured.

### Betrayal timing x severity

Mild contradiction can leave earned authority sticky, and late betrayal leaves less post-change consequence before evaluation. The surface therefore mixes revocation strength with revocation latency. It supports treating contradiction magnitude and elapsed corrective evidence as distinct pressures.

### Divergence prevalence x consequence delay

This produced the clearest opportunity-cost crossover. Accountable social openness remained competitive while useful alignment was common, then became net costly when persistent divergence dominated the social environment. Context standing alone is therefore insufficient; a broader peer eligibility or recruitment pressure may need to learn whether another mind is worth consulting at all.

### Feedback observability x sender adaptation

Public feedback is not intrinsically a vulnerability. It is both the channel by which a strategic sender can learn leverage and the channel by which consequence can make that leverage stop paying. Low observability also weakens the naive manipulation itself, so absent exploitation must not be counted as a defense failure.

### Aligned observation noise x consequence delay

Every peer was genuinely aligned. Aligned-help remained useful throughout, but final standing declined as consequence noise increased. The mechanism was confusing unreliable observation with unreliable source behavior. A durable architecture should keep **source reliability** and **consequence-channel reliability** as different uncertainty types.

## Accepted Protocol 08 conclusion

> Receiver-owned, consequence-grounded permission can keep a strategically adaptive peer useful without private-state inspection, but only within an operating envelope shaped by consequence timeliness, objective alignment, observation quality, and whether the peer remains worth recruiting.

Do not carry the frozen learning rates, confidence presentations, standing formula, or scalar penalty constants into CPA as anatomy. The stronger carry-forward constraints are typed uncertainty, receiver-owned authority, consequence answerability, bounded social opportunity, and the separation of source quality from channel quality.

## Reproduction only

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-validation --output _artifacts/p08-holdout-v1-repro

dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-falsify --output _artifacts/strategic-influence-falsification-v1-repro
```

The next active question is Protocol 09 authority ancestry / circular standing. It tests whether **permission itself** can appear independently warranted after circulating through a social network, a problem not answered by P08's dyadic strategic-influence result.
