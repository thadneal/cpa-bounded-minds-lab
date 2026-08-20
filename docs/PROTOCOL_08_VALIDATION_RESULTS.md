# Protocol 08 validation and falsification results

Protocol: `08-strategic-public-influence`

Evidence status: **frozen and consumed**

## Fresh holdout

The preregistered `p08-holdout-v1` set was executed after the exact v0.11 experiment and world-generator sources were frozen. It contained twenty previously unused seeds and returned:

```text
20 / 20 Support
200 / 200 preregistered checks passed

mechanism outcome   80 / 80
safety boundary     60 / 60
manipulation        40 / 40
accounting          20 / 20
```

Mean metrics:

```text
accountable total RMSE             0.178726
naive total RMSE                   0.356084
local-only total RMSE              0.185988
accountable early aligned RMSE     0.234783
local early aligned RMSE           0.410327
accountable late divergent RMSE    0.030488
naive late divergent RMSE          0.417158
accountable late betrayal RMSE     0.029732
naive late betrayal RMSE           0.420724
accountable final aligned standing 0.910837
accountable final divergent        0.025567
accountable final betrayal         0.025297
accountable late divergent assertive rate 0.363864
naive late divergent assertive rate       0.666061
```

The result supports a narrow behavioral claim: receiver-owned standing that is repeatedly revised by receiver-owned consequence can preserve useful peer influence while sharply reducing the reward for strategically exaggerated public presentation. The receiver does not need access to the sender's private objective or internal tactic values.

The result does **not** imply that social openness is free. Accountable influence was worse than local-only learning in several holdout histories, with the worst case approaching the frozen 5% opportunity-cost ceiling. The defense also deliberately gives up some upside relative to naive self-report when the peer happens to be genuinely aligned.

## Controlled falsification

`strategic-influence-falsification-v1` ran five controlled 7x7 surfaces with seven deterministic replicates per cell:

```text
5 profiles
245 cells
1,715 deterministic replicate runs
```

### Consequence delay x sender adaptation

Delay mattered more than sender adaptation speed. As consequence became late, the accountable receiver's large advantage over the naive receiver shrank. In the tested range the accountable path generally degraded toward local-only learning rather than catastrophic strategic capture.

Durable conclusion: accountability requires timely enough consequence to keep public influence calibrated. Fast strategic adaptation is less dangerous when consequence still arrives soon enough to revise permission.

### Betrayal timing x severity

Weak betrayal often retained too much standing because mild contradiction resembled ordinary variation. Very late betrayal also left insufficient post-change history before evaluation. The surface therefore mixes two real effects: prior earned authority and finite revocation time.

Durable conclusion: revocation has latency, and contradiction strength should remain visible rather than being flattened into a single betrayal category.

### Divergence prevalence x consequence delay

The cleanest boundary appeared when persistent objective divergence became common. Around the upper end of the controlled prevalence range, keeping the peer available became materially worse than learning locally even when context-level standing still revised correctly.

Durable conclusion: source standing and source **eligibility for attention** are different pressures. A peer can remain locally correctable while becoming globally not worth recruiting often.

### Feedback observability x sender adaptation

Low observability can prevent the sender from learning effective leverage at all. High observability can support manipulation against a naive receiver, but it also gives the accountable ecology the public feedback channel through which consequence changes the sender's learned incentives.

Durable conclusion: secrecy is not the principle. Public feedback can be part of accountability.

### Aligned noise x consequence delay

This null-harm surface contained no hidden objective divergence. Useful early social benefit survived across the grid, but increasing consequence noise progressively reduced final standing for an actually aligned peer.

Durable conclusion: a receiver must distinguish uncertainty about the **source** from uncertainty in its own **consequence channel**. A single scalar standing value is too coarse to represent both.

## Revised Protocol 08 conclusion

> A bounded receiver can make strategically adaptive public influence answerable without inspecting the sender's private interior when permission remains receiver-owned and repeatedly meets consequence. The mechanism has a real operating envelope: delayed or noisy consequence, pervasive divergence, and weak or late betrayal can reduce its value or distort authority. Carry forward the behavioral constraints, not the frozen scalar equations.

`p08-holdout-v1` and `strategic-influence-falsification-v1` are consumed evidence. They may be rerun for reproducibility but must not be reused as fresh validation after later changes.
