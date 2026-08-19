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
