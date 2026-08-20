# Protocol 08 development result

Protocol: `08-strategic-public-influence`

Evidence status: **development-v1, consumed for development**

Seeds: `101, 211, 307, 401, 503`

Result: **5 Support / 0 Mixed / 0 Disconfirm**, with **50/50 preregistered checks passing**.

This result establishes that the assay is worth freezing and validating. It is not independent confirmation because the canonical five-seed matrix is the laboratory's long-running development set.

## Mean results

| Metric | Accountable consequence | Self-report naive | Local only |
| --- | ---: | ---: | ---: |
| Total RMSE | 0.17793 | 0.34698 | 0.17824 |
| Early aligned RMSE | 0.23370 | 0.17892 | 0.40223 |
| Late divergent RMSE | 0.02922 | 0.40325 | n/a |
| Late betrayal RMSE | 0.02631 | 0.34351 | n/a |
| Final aligned standing | 0.90109 | n/a | n/a |
| Final divergent standing | 0.02588 | 0.22642 | n/a |
| Final betrayal standing | 0.03158 | 0.35825 | n/a |
| Late divergent assertive rate | 0.33667 | 0.60788 | n/a |
| Sender utility | 0.75714 | 0.84395 | n/a |

The accountable path reduced total error by about 48.7% relative to naive self-report authority while remaining almost exactly even with local-only learning across the mixed world. That near equality hides a useful tradeoff: accountable treatment was worse than local-only in seeds 101 and 503, and better in seeds 211, 307, and 401.

## What the development result supports

### Public strategy can adapt without private-state access

The sender observes its own objective and the receiver's resulting public prediction. It does not receive receiver-private source standing, calibration trust, local estimate, counterfactual prediction, or implementation state. Yet it learns that assertive self-presentation is more rewarding against the naive receiver.

Mean late divergent assertiveness:

```text
self-report naive        0.60788
accountable consequence  0.33667
```

The treatment therefore changes the ecology of reward around public influence without requiring mind reading or a deception classifier.

### Accountability preserves useful social help, but does not maximize it

Where sender and receiver objectives genuinely align, both peer conditions outperform local-only learning early. Naive self-report is better than the accountable path in this region because it grants the peer more immediate leverage.

```text
local-only early aligned  0.40223
accountable               0.23370
naive self-report         0.17892
```

The accountable receiver sacrifices some best-case social acceleration in exchange for remaining revisable when objectives diverge.

### Betrayal remains correctable in the development family

The two betrayal contexts begin aligned and later change objective. By the late phase, accountable treatment has mean RMSE `0.02631` versus `0.34351` for naive self-report, while accountable final betrayal standing falls to about `0.0316`.

This is a strong development result, but the original world family gives direct consequence quickly and leaves substantial time after betrayal. The next phase therefore varies betrayal timing, severity, and consequence delay directly.

### Over-deterrence is a real question

Seed 503 produced accountable aligned assertiveness of only about `0.0333`, compared with `0.4583` under naive self-report. Useful aligned standing still ended near `0.916`, and early aligned error remained much better than local-only, so this was not a development failure.

It nevertheless motivates an explicit null-harm surface in v0.12: all peers are genuinely aligned while direct consequence becomes noisy or delayed. A defense that suppresses useful social influence in that world should be counted against the mechanism.

## Revised development conclusion

> A receiver can keep a strategically adaptive peer useful without inspecting its private objective when public influence is governed by receiver-owned standing and later consequence. Accountability makes manipulative presentation less rewarding and permits revocation after betrayal, but it carries opportunity cost and may over-deter useful assertiveness under some histories.

The exact standing equation, confidence calibration rule, sender tactic set, and thresholds remain experimental instruments. The durable CPA candidate is the behavioral constraint: **public influence should remain answerable to receiver-owned consequence even when the other mind's interior is unavailable and its public posture is strategic.**
