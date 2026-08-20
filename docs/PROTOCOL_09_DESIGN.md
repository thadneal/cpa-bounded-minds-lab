# Protocol 09 design: authority ancestry / circular standing

Protocol: `09-authority-ancestry-circular-standing`

Status: **development protocol in v0.13**

## Question

Can a bounded ecology distinguish authority earned from independent consequence from permission that has recursively circulated through locally reasonable endorsements?

## Why this is not Protocol 06 again

Protocol 06 studied **epistemic ancestry**. Several factual reports could appear independent while descending from one underlying observation. Its problem was duplicated evidence.

Protocol 09 studies **authority ancestry**. No factual report needs to be duplicated. Instead, one mind's permission for a source can be transferred to another mind, then transferred again, and eventually return as apparently independent social warrant. The thing being recursively duplicated is permission to matter.

P07 showed that second-hand standing can buy provisional opportunity. P08 showed that a strategic sender can adapt its public surface to exploit standing rules. Neither asks whether individually reasonable standing transfers can form a self-supporting network loop.

## World

Each seed generates twelve contexts across five bounded endorsing peers plus a receiver.

- Four `IndependentGrounding` contexts begin with three or four peers that have separately earned direct standing for the candidate source. The candidate estimate is locally useful.
- Four `CircularAuthorityTrap` contexts begin with one weak direct authority root. The candidate estimate is materially wrong for the receiver. Peers can nevertheless pass endorsements around a ring.
- Two `MixedAuthority` contexts contain two direct roots and partial source mismatch.
- Two `SparseGrounding` contexts contain one useful direct root without a circular pathology.

Seed changes alter context placement, target values, candidate estimates, direct-root position, direct-standing strength, peer trust, and observation noise.

## Treatments

### Authority ancestry

Each compact endorsement preserves two bounded sketches:

- a **root sketch** describing which direct authority roots contributed to the current permission;
- a **path sketch** describing which peers the permission has already traversed.

A peer discounts incoming permission that carries no grounded roots, duplicates roots it already has, or returns through a path that already contains itself. The receiver also discounts overlapping root sketches when combining final endorsements.

The sketches are public coordination metadata, not a global authority registry. Each peer keeps only its own local standing and the compact ancestry attached to messages it receives.

### Recursive endorsement control

The same peers exchange the same number of endorsements at the same communication cost. Each peer locally trusts the predecessor's standing and strengthens its own candidate standing accordingly, but authority ancestry is discarded as a governing distinction.

No individual transfer is intended to be absurd. The control is designed so the pathology emerges only through recursion: a weak initial permission can become high apparent authority without new direct evidence.

### Direct-only baseline

The receiver ignores social permission and learns the candidate only from its own direct consequence.

## Receiver consequence

After social development, all three receiver paths encounter the same seed-specific consequence schedule. Social authority is only an initial permission to influence prediction. Receiver-owned direct consequence updates both local estimates and candidate standing.

This prevents Protocol 09 from treating ancestry as truth. Authority ancestry may shape a prior, but later local consequence remains sovereign.

## Preregistered checks

1. `seed-generates-authority-cascade-world` - every world must contain several independently grounded contexts and several circular traps.
2. `recursive-endorsement-amplifies-circular-authority` - the control must actually instantiate circular authority amplification from a weak initial root.
3. `authority-ancestry-preserves-grounded-opportunity` - ancestry protection must preserve substantial early benefit from independently grounded social permission relative to direct-only learning.
4. `authority-ancestry-discounts-circular-permission` - one weak root echoed through several peers must not count as several independent permissions.
5. `authority-ancestry-reduces-circular-capture` - before much direct consequence arrives, ancestry-sensitive authority must materially reduce error caused by the circular control.
6. `independent-grounding-remains-distinct-from-circular-authority` - several independent roots must retain materially more initial authority than one recursively echoed root.
7. `direct-consequence-revokes-circular-authority` - later receiver consequence must be able to reduce circular standing and late error.
8. `grounded-standing-remains-earned` - the mechanism must not solve circularity by creating generalized social distrust.
9. `whole-history-authority-ancestry-benefit` - ancestry-sensitive authority should outperform recursive endorsement while preserving enough useful social transfer to beat direct-only learning in the development world family.
10. `bounded-authority-exchange` - both social treatments exchange exactly the same bounded number of compact packets at identical explicit cost.

## Interpretation limits

A Supported development result would **not** establish a production authority-ancestry algorithm. In particular:

- the compact bit sketches are laboratory instruments;
- the five-peer ring is a controlled minimal social topology;
- direct-root identity is unusually clean compared with a mature ecology;
- fixed update rates are not proposed CPA constants;
- the experiment does not imply that circular endorsement is always wrong. Independent social institutions can legitimately reinforce one another when their authority remains separately grounded.

The result matters only if it shows a distinct failure mode not already explained by P06-P08.

## Next evidence decision

Run the canonical development matrix first:

```powershell
dotnet run --project src/Cpa.BoundedMindsLab.Cli -c Release --no-build -- `
  --experiment 09-authority-ancestry-circular-standing `
  --replicate 101,211,307,401,503 `
  --output _artifacts/protocol-09-development-v1
```

If P09 produces a coherent new result, freeze it before designing any holdout or falsification surface. If it collapses into a restatement of P06-P08, close the social protocol sequence instead of tuning it into distinctness.
