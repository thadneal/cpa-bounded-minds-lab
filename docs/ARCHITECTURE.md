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

A sender never exposes its `DevelopmentalMemory` object to another mind. A receiver can obtain only `PublicTracePacket` values through `SharedTraceChannel`.

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
