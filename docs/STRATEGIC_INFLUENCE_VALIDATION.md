# Protocol 08 validation and falsification plan

Version 0.12 freezes the exact Protocol 08 development implementation and starts two separate evidence paths.

## Frozen boundary

The following v0.11 development sources are now covered by `docs/FROZEN_PROTOCOL_SHA256.txt`:

- `src/Cpa.BoundedMindsLab.Core/Experiments/StrategicPublicInfluenceExperiment.cs`
- `src/Cpa.BoundedMindsLab.Core/Environments/StrategicInfluenceWorld.cs`

They must not change while `p08-holdout-v1` is being interpreted.

## p08-holdout-v1

Twenty previously unused seeds are preregistered in `ExperimentDefaults.StrategicInfluenceHoldoutSeeds`:

```text
41047, 42131, 43391, 44621, 45893,
47237, 48611, 49919, 51307, 52709,
54139, 55603, 57143, 58661, 60209,
61781, 63347, 64997, 66617, 68213
```

These seeds were registered without executing or inspecting Protocol 08 outcomes. They are disjoint from development-v1, the consumed Protocol 01-07 holdout, and challenge-v1 selected seeds.

The authoritative first run is:

```powershell
./scripts/verify.ps1

dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-validation `
  --output _artifacts/p08-holdout-v1
```

The first execution **consumes** this holdout. Reruns after inspection are reproducibility checks only.

The ordinary frozen Protocol 08 verdict and all ten preregistered assertions remain unchanged. Validation reporting continues to separate manipulation, mechanism-outcome, safety-boundary, and accounting evidence.

## strategic-influence-falsification-v1

This is exploratory operating-envelope evidence, not a second holdout. It may deliberately extend the adversary and consequence schedule beyond the original world generator while keeping the accountable receiver equations frozen.

Five 7 x 7 response surfaces use seven deterministic replicates per cell: **245 cells / 1,715 controlled runs**.

### 1. Consequence delay x sender adaptation

Question: can a strategic sender learn presentation faster than consequence can calibrate influence?

The consequence-delay axis postpones receiver-owned direct observation by `0, 1, 2, 4, 6, 10, 16` same-context exposures. Sender adaptation speed ranges from `0.5x` through `8x`; `1.0x` reproduces the frozen sample-mean tactic update.

The primary margin combines aligned help, capture resistance, and the 5% local-only opportunity-cost allowance. The 0.15 exploitability-reduction boundary is applied only when the naive sender actually reaches the original `0.50` leverage manipulation threshold; absent leverage is not counted as a receiver failure.

### 2. Betrayal timing x severity

Question: how much earned standing remains safely revisable if objective divergence appears early or late and at different magnitudes?

Betrayal begins after exposure `2, 5, 8, 10, 14, 20, 26`, with post-change sender-target divergence from `0.20` through `1.30` before clamping.

The primary boundary retains the frozen betrayal test: late accountable error must stay below 20% of naive error and final betrayal standing must stay at or below `0.10`.

### 3. Divergence prevalence x consequence delay

Question: when does keeping the peer socially available become net harmful compared with learning alone?

The fraction of persistently divergent contexts varies from zero to all contexts while consequence delay varies independently. The primary boundary is the frozen 5% opportunity-cost allowance relative to local-only learning.

This surface deliberately does not require accountable treatment to beat naive treatment when divergence prevalence is zero. In a world where the peer is always useful, aggressive social trust may legitimately win.

### 4. Feedback observability x sender adaptation

Question: how much can the sender learn from receiver public reactions before accountability stops reducing exploitability?

The sender can update tactic value from public influence feedback on `0%` through `100%` of interactions while adaptation speed varies from `0.5x` through `8x`. If the naive sender never reaches the original leverage manipulation threshold, the surface reports that manipulation as inactive rather than treating the missing exploitability contrast as a failure of accountability.

The receiver still exposes no private standing, local estimate, calibration state, or implementation detail.

### 5. Aligned observation noise x consequence delay

Question: can defense become over-deterrence?

Every context is genuinely aligned. There is no hidden divergence or betrayal. Direct receiver consequence becomes progressively noisy and delayed.

The accountable path must still improve early aligned RMSE by at least 25% versus local-only and retain final aligned standing of at least `0.85`. Negative cells are regions where a mechanism designed to resist strategic capture has begun suppressing a useful peer.

## Run order

Run and preserve the fresh holdout first. Interpret it before treating the falsification surfaces as explanatory context.

Then run:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --p08-falsify `
  --output _artifacts/strategic-influence-falsification-v1
```

The falsification runner writes:

- `strategic-falsification-plan.json`
- `strategic-falsification-report.json`
- `strategic-falsification-summary.md`
- one CSV per response surface

A negative margin is expected and useful. Do not change the receiver mechanism and then reinterpret the same surfaces as confirmation.

## Decision after this phase

The next research decision is not automatically Protocol 09. The evidence should answer two questions first:

1. Does strategic public influence have an intelligible operating envelope rather than succeeding only inside the development generator?
2. Does authority/standing circulation among several minds remain a distinct failure mode after strategic public influence is understood?

If the second answer remains yes, Protocol 09 should test circular endorsement and authority cascades. If not, the laboratory should move toward closeout and handoff to the candidate Trace and Interface Laboratory.
