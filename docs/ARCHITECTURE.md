# Laboratory Architecture

## Purpose

This architecture exists to support controlled experiments about development among bounded minds. It is not proposed as final CPA anatomy.

The central boundary is:

```text
private developmental history
        |
        | selected compression
        v
compact public trace packet
        |
        | explicit communication cost
        v
receiving mind
        |
        | provisional influence + local consequence
        v
revised local readiness
```

In ordinary/preserved experimental paths, a sender never exposes its private memory object to another mind. Protocol 01 receivers obtain only `PublicTracePacket` values through `SharedTraceChannel`; Protocol 02 peers expose only current public prediction-and-standing postures. Deliberately invasive controls may violate this boundary only when the violation itself is the experimental contrast.

## Developmental memory

`DevelopmentalMemory` currently separates two stores:

- direct/local traces, changed by lived consequence;
- foreign traces, admitted from another mind's public surface.

Both can influence prediction, but their authority develops differently. A foreign trace enters with capped standing. Later direct consequence can renew it when it fits the receiver's world or rapidly withdraw its standing when it does not.

Protocol 01 deliberately does not introduce a generic `BoundedMind` base class. At this stage the smallest organism capable of testing the question is the developmental memory process plus its private stores and public packet boundary. A reusable mind abstraction should appear only when later protocols reveal structure that actually recurs.

The `shared-lived-equivalent` control deliberately violates this separation. It copies the same public packet into local authority while inheriting the source's accumulated evidential inertia. This is a falsification control for contamination, not a recommended transfer rule.

## Communication

`SharedTraceChannel` is deliberately narrow. It stores only compact public packets and charges fixed work per packet. It has no access to hidden evaluator labels, future receiver targets, model internals, or private source memory.

The first protocol transfers all source traces that have earned the publication threshold. Later work should investigate selection, negotiation, and communication cadence rather than assuming broadcast is free or universal.

## Synthetic world

`TransferContaminationWorld` contains eight abstract context cells. The source and receiver share six target relationships and disagree on two. Cognition receives the context cell and consequence. The evaluator knows which cells are shared or divergent only so the experiment can score transfer benefit and contamination.

The context cells are intentionally semantically unnamed. The protocol asks about authority and transfer mechanics, not a hand-designed cognitive role.

## Observation boundary

Core experiment frames expose only public instrumentation:

- numeric metrics;
- compact `MindPublicState`;
- compact `TracePublicState`;
- developmental and phase events;
- final verdict and assertions.

The desktop visualization consumes those frames. It never inspects private memory dictionaries. Evaluator-only metrics may be exposed for scientific inspection when they are explicitly named as evaluator data, but neither the mind nor the transfer mechanism can read them.

## Relationship to the previous .NET laboratory

The prior laboratory's strongest architectural lesson was restraint: many successful mechanisms were protocol instruments rather than permanent organs. This solution carries forward runtime discipline, deterministic histories, explicit controls, falsification, public observation, and artifact reproducibility.

It intentionally leaves behind the old protocol catalog and its specialized controllers.

The desktop application is also rebuilt rather than copied. The old workbench already used a bounded display channel, custom drawing, point compaction, display downsampling, and throttled hover. Its remaining performance weakness was that WPF still drained frames and applied each frame to several UI-owned views. The new workbench puts experiment execution on a dedicated below-normal-priority thread, moves frame projection itself off the dispatcher, and lets WPF sample already-projected state. Fine live graph levels can retire when they cease to offer useful display resolution; full-fidelity history remains a durable artifact rather than UI heap pressure.

## Protocol 02 peer boundary

Protocol 02 adds no general-purpose collective-mind abstraction. Its two `PeerMind` instances remain protocol-local experimental instruments with context-indexed private estimates, local developmental standing, and consequence-revisable public standing.

The preserved condition uses this boundary:

```text
peer A private state                 peer B private state
        |                                    |
        | current prediction + standing      | current prediction + standing
        v                                    v
       compact public postures under explicit packet cost
                         |
                         | simple experimental negotiation readout
                         v
                 shared consequence
                    /           \
                   v             v
          peer A updates      peer B updates
          its own interior    its own interior
```

The negotiation readout can see only the two current public postures. It cannot inspect either peer's private history, evidence arrays, future consequence, or evaluator labels. Shared consequence is returned to each peer separately and changes each peer's own estimate and public standing.

The `synchronized-control` is intentionally invasive. It averages both private states before the shared phase and creates two identical copies. This is a falsification contrast for premature consensus. It does not cross `SharedTraceChannel`, it is not counted as legitimate inter-mind communication, and it must not be mistaken for a proposed CPA mechanism.

`PeerDisagreementWorld` remains semantically anonymous. One peer has the more useful private history for four context cells and the other peer for the complementary four. Small deterministic noise prevents the histories from becoming literal duplicate constants while keeping the first disagreement assay interpretable.

## Protocol 03 developmental-transfer boundary

Protocol 03 does not add a general teaching subsystem. Its `SourceMind`, `ReceiverMind`, and transfer records remain protocol-local assay machinery.

The ordinary boundary is:

```text
source private consequence history
            |
            | bounded public compression
            +-------------------------------+
            |                               |
            v                               v
developmental packet                  doctrinal packet
rule estimate                         rule estimate
source evidence depth                 no history detail
3 consequence segment means           uniform standing
history variability
            |                               |
            +---------------+---------------+
                            v
                   receiver private state
                            |
                            | direct local consequence
                            v
                 revised foreign permission
```

The developmental packet is not the source's private history object. It carries one fixed-size summary per anonymous context. Communication cost is explicitly larger than doctrinal transfer and remains bounded.

The receiver uses source evidence depth and consistency only to set **initial foreign standing**. It does not receive evaluator labels such as `StableCompatible`, `StableDivergent`, `UnstableTransition`, or `SparseAmbiguous`. Those labels exist in the synthetic environment and public experiment telemetry only so the assay can score the intended pressure.

This distinction is especially important for stable-divergent histories. Their internal source history is coherent, so developmental calibration may initially trust them. The receiver is not permitted to infer local mismatch from an evaluator category. Only direct receiver consequence can drive their foreign standing toward zero.

## Seed-generated developmental worlds

`DevelopmentalTransferWorld` is the first environment in this repository where a seed changes more than schedule/noise. A seed deterministically generates:

- assignment of developmental history kinds to context cells;
- receiver target values;
- source evidence counts;
- source and receiver noise amplitudes;
- source regime-transition direction and values;
- observation schedules.

The generated family remains bounded and preregistered. Determinism is preserved, while replication now probes materially different biographies.

This is a laboratory-method change, not a cognitive mechanism. Neither source nor receiver can inspect the seed, scenario fingerprint, history-kind label, or future generated parameters.


## Protocol 04 communication boundary

Protocol 04 keeps private interiors intact in **both** treatments. This matters because Protocol 02 already tested private-state synchronization. The new variable is public communication before consequence.

```text
peer 1 private history ----> compact posture --+
peer 2 private history ----> compact posture --+--> commitment --> shared consequence
peer 3 private history ----> compact posture --+                    |
       ^                                                           |
       +---------------- direct local update ----------------------+
```

The typed posture is fixed-size and source-specific:

```text
estimate
standing
uncertainty
source id / context id
evidence count (audit surface)
```

The experiment-local decision readout uses only estimate, standing, and uncertainty to form the current prediction. Source identity and evidence count remain available for observation/accounting but do not create a semantic peer role.

The `early-semantic-smoothing` control inserts two public assimilation rounds between posture publication and commitment:

```text
private interiors unchanged
        |
        v
three public postures
        |
        v
public smoothing round 1
        |
        v
public smoothing round 2
        |
        v
commitment
        |
        v
external consequence
```

The smoothing operation changes only what peers publicly say for the current observation. It never writes those socially adjusted statements back into private memory. This makes the control distinct from Protocol 02's invasive synchronized-interior condition.

Communication cost is explicit. Typed communication emits three compact postures per shared observation. The smoothing control pays for the initial postures plus two additional three-message rounds. No hidden negotiation work is free.

`CommunicationBeforeLanguageWorld` follows the Protocol 03 seed policy. A seed changes the social-history circumstance itself, including the prevalence/placement of informative dissent, misleading dissent, complementary expertise, convergence, evidence depth, target values, and noise. Evaluator labels and future target parameters are never visible to the peers.

Nothing in Protocol 04 establishes a permanent CPA language architecture. The readout and smoothing equations are assay instruments. The architectural candidate being tested is narrower: keep public epistemic shape distinct long enough for consequence to act on it.


## Protocol 05 distributed convention boundary

Protocol 05 introduces no collective-mind object and no central convention registry. Each peer owns a private context-to-action convention memory with local standing. Convention becomes collective only when repeated successful interaction causes those separate memories to converge.

```text
peer preferences --> stateless public negotiation --> shared consequence
                                                    |
                                                    v
                                      each peer updates its own
                                      local convention + standing

after convention earns standing:

one convention invocation --> peers consult local copies --> consequence
```

The public reducer receives only current preferred action and preference strength. It cannot inspect private convention arrays, evaluator context kind, future costs, or the regime-shift label.

The one-packet shortcut replaces three fresh preference packets only after local standing is high. Listeners still act from their own convention copies. If copies diverge, coordination can fail and consequence withdraws standing.

The `frozen-convention` control forms the same culture as the adaptive path, then refuses to revise after the world changes. It is a pathological contrast for social inertia, not proposed CPA anatomy.

## Protocol 06 incomplete ancestry boundary

Protocol 06 separates **surface agreement** from **independent evidential origin** without assuming that perfect provenance exists.

```text
hidden evidence root A ----> peer 1 report ----+
          |               \-> peer 2 report ----+--> bounded receiver
          |               \-> peer 3 report ----+      |
          |                                             v
hidden evidence root B --------> peer 4 report ------> ancestry grouping
                                                        |
hidden evidence root C --------> peer 5 report ------> bounded support
```

The ordinary receiver never sees the hidden root IDs. Each report exposes only a compact public surface:

```text
sender id
estimate
standing
evidence depth
optional opaque origin hint
three-value developmental signature
```

An origin hint may be absent, may preserve a shared upstream alias, or may stop at the immediate sender. The developmental signature is copied with noise, so it can suggest common ancestry but is not a perfect hash.

The `ancestry-inferred` reducer merges reports when a non-empty origin hint matches or the developmental signatures are sufficiently close. Reports inside one inferred lineage can refine that lineage's estimate, but the lineage's corroborative weight saturates at the strongest member rather than multiplying with every echo.

The `naive-agreement` control assigns every report its own corroborative group. The `oracle-ancestry` control groups by hidden root and is visible only to the evaluator. It is a calibration ceiling, not an architecture proposal.

All paths receive the same 98 reports and pay the same communication cost. The only treatment difference is how public corroboration is reduced locally. This prevents ancestry inference from hiding extra retrieval, consultation, or communication work.

The signature distance and clustering rule are protocol instruments. A passing result would support the need to preserve ancestry uncertainty and discount probable echoes. It would not establish Euclidean signature clustering, a universal provenance packet, or a global ancestry service as CPA anatomy.

## Protocol 07 provisional standing-transfer boundary

Protocol 07 distinguishes **access to a source** from **permission for that source to influence judgment**.

```text
A's private history with B
        |
        v
bounded recommendation packet
        |
        v
C applies its own standing for A
        |
        +--> provisional standing for B (capped)
        |
        v
B's ordinary public estimate + C's local estimate
        |
        v
C prediction --> C's direct consequence
                    |
                    +--> renew/revoke B standing locally
```

The recommendation packet contains A's context-specific standing for B and A's evidence depth. C's credibility for A remains private to C. The provisional calculation therefore cannot be completed by the sender or by a shared social registry.

All three treatment paths receive the same ordinary B prediction surface. `no-standing-transfer` changes only B's initial permission, not B's visibility. `inherited-authority` deliberately copies A's standing into C without the provisional cap and exists as a pathological control.

The core distinction is:

```text
recommendation may buy attention / opportunity
recommendation does not import lived authority
local consequence can turn provisional standing into locally earned standing
local consequence can also revoke it
```

The scalar standing equation, exploration floor, and cap are protocol instruments. A passing result would support the architectural principle that social standing should remain context-specific, receiver-relative, and revisable. It would not establish a global reputation system or transitive trust graph.


## Validation layer is outside cognition

Version 0.8 adds a validation/reporting layer around the frozen experiments. It is not part of the cognitive architecture.

`ValidationPlan`, `ValidationReportBuilder`, and the Desktop/CLI seed presets can see protocol names, final metrics, assertions, and evaluator world descriptors after a run. They cannot inject validation categories, challenge labels, holdout identity, or diagnostics into a mind's prediction/update path.

The separation is deliberate:

```text
frozen experiment + world
        |
        | ordinary public frames/results
        v
validation/reporting layer
        |
        +--> seed-set classification
        +--> assertion taxonomy
        +--> challenge-slice filtering
        +--> assay-sensitivity diagnostics
```

The protocol/world SHA-256 manifest further protects this boundary by making unnoticed mechanism changes visible before validation.
