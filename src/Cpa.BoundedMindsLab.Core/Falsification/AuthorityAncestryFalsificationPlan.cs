namespace Cpa.BoundedMindsLab.Falsification;

public static class AuthorityAncestryFalsificationPlan
{
    public const string Name = "authority-ancestry-falsification-v1";
    public const int ReplicatesPerCell = 7;

    public static IReadOnlyList<FalsificationProfileDefinition> Profiles { get; } =
    [
        new(
            "p09-grounding-diversity-peer-trust",
            "P09 grounding diversity x peer trust",
            "09-authority-ancestry-circular-standing",
            "How many independently earned authority roots are needed before useful social permission remains distinguishable from one root circulating through highly trusted peers?",
            "A controlled twelve-context micro-world copies the frozen P09 peer-standing, ancestry discount, receiver aggregation, and direct-learning equations. Independent direct-root count and peer trust vary while circular traps retain one weak root.",
            Axis("independent_root_count", "Independent root count", "Effective number of separately grounded authority roots in useful contexts; half steps give the final root proportional initial standing.", 1.0, 1.5, 2.0, 2.5, 3.0, 4.0, 5.0),
            Axis("peer_trust", "Peer trust", "Local trust multiplier applied when peers transfer standing to one another.", 0.20, 0.40, 0.60, 0.75, 0.85, 0.92, 0.98),
            ReplicatesPerCell,
            AuthorityAncestryProbes.EvaluateGroundingDiversityVersusPeerTrust,
            "boundary_margin",
            "Positive means ancestry-sensitive permission both preserves an early grounded advantage over direct-only learning and maintains a material authority gap between independent grounding and a one-root circular loop.",
            "Root diversity is an effective intervention in this probe, with half steps giving the final root proportional initial standing. It maps the need for independent authority diversity; it does not imply a production system will know root identity perfectly."),
        new(
            "p09-circulation-depth-ancestry-fidelity",
            "P09 circulation depth x ancestry fidelity",
            "09-authority-ancestry-circular-standing",
            "How quickly does circular protection fail when permission circulates farther than the public ancestry sketch can faithfully preserve?",
            "The frozen P09 local transfer equations are copied into a controlled ring. Social rounds vary from shallow transfer to repeated recirculation while ancestry fidelity continuously degrades overlap and return-path recognition toward naive independence counting.",
            Axis("social_rounds", "Circulation depth", "Number of public endorsement rounds before receiver admission.", 1, 2, 4, 6, 8, 10, 12),
            Axis("ancestry_fidelity", "Ancestry fidelity", "Fraction of authority-root overlap and return-path structure that remains usable by the ancestry-sensitive mechanism.", 0.00, 0.15, 0.30, 0.50, 0.70, 0.85, 1.00),
            ReplicatesPerCell,
            AuthorityAncestryProbes.EvaluateCirculationDepthVersusAncestryFidelity,
            "boundary_margin",
            "Positive means ancestry sensitivity still produces less early circular capture than recursive endorsement and keeps circular authority below the registered relative-discount boundary.",
            "Fidelity is an intervention on the laboratory ancestry signal, not a proposed sensor model. The purpose is to learn whether approximate lineage can retain value before exact genealogy is available."),
        new(
            "p09-circular-strength-receiver-mismatch",
            "P09 circular-root strength x receiver mismatch",
            "09-authority-ancestry-circular-standing",
            "When is circular permission actually harmful enough that discounting it pays for the opportunity cost of social caution?",
            "Circular contexts vary the one direct root's initially earned standing and the candidate estimate's mismatch from receiver truth. Grounded contexts remain useful so whole-history opportunity and protection can be compared against recursive endorsement and direct-only learning.",
            Axis("circular_root_standing", "Circular root standing", "Initial direct standing of the single authority root before it circulates.", 0.10, 0.20, 0.30, 0.40, 0.55, 0.70, 0.85),
            Axis("receiver_mismatch", "Receiver mismatch", "Absolute candidate-source error relative to the receiver's local target in circular contexts.", 0.00, 0.10, 0.20, 0.35, 0.55, 0.80, 1.10),
            ReplicatesPerCell,
            AuthorityAncestryProbes.EvaluateCircularStrengthVersusReceiverMismatch,
            "boundary_margin",
            "Positive means ancestry sensitivity beats recursive endorsement while staying within 5% of direct-only whole-history error. Negative cells identify benign or legitimately earned circular influence where discounting costs more than it protects.",
            "Circularity is not treated as evidence of falsehood. This surface deliberately includes low-mismatch regions where a recursively reinforced source may still be useful."),
        new(
            "p09-consequence-delay-circulation-depth",
            "P09 consequence delay x circulation depth",
            "09-authority-ancestry-circular-standing",
            "How much receiver-owned consequence can be delayed before recursively amplified permission causes developmental cost that ancestry protection no longer contains?",
            "Receiver observations follow the frozen P09 learning equations, but direct standing updates are queued for a controlled number of context exposures. Circulation depth varies independently before those consequences arrive.",
            Axis("consequence_delay", "Consequence delay", "Number of same-context exposures before receiver-owned consequence is allowed to revise local estimate and source standing.", 0, 1, 2, 4, 6, 10, 14),
            Axis("social_rounds", "Circulation depth", "Number of public endorsement rounds before receiver admission.", 2, 4, 6, 8, 10, 12, 16),
            ReplicatesPerCell,
            AuthorityAncestryProbes.EvaluateConsequenceDelayVersusCirculationDepth,
            "boundary_margin",
            "Positive means ancestry sensitivity retains a whole-history advantage over recursive endorsement while staying within 5% of direct-only learning despite delayed correction.",
            "Delayed consequence changes the receiver's corrective channel without changing the frozen authority equations. It is an operating-envelope probe, not a proposal to postpone learning."),
        new(
            "p09-grounded-noise-consequence-delay",
            "P09 grounded consequence noise x delay",
            "09-authority-ancestry-circular-standing",
            "Can authority ancestry preserve useful independently grounded social permission when the receiver's own consequence channel is noisy or slow?",
            "Every context is genuinely useful and independently grounded. Consequence noise and delay vary while the social mechanism remains unchanged. This is the Protocol 09 null-harm surface.",
            Axis("consequence_noise", "Consequence noise", "Amplitude of receiver-owned observation noise around the true local target.", 0.00, 0.03, 0.06, 0.10, 0.15, 0.22, 0.30),
            Axis("consequence_delay", "Consequence delay", "Number of same-context exposures before consequence updates are delivered.", 0, 1, 2, 4, 6, 10, 14),
            ReplicatesPerCell,
            AuthorityAncestryProbes.EvaluateGroundedNoiseVersusDelay,
            "boundary_margin",
            "Positive means ancestry-sensitive social help remains at least 5% better than direct-only learning and final grounded standing remains above 0.70 in an all-grounded world.",
            "There is no circular trap in this profile. A negative region is over-deterrence or consequence-channel confusion, not successful defense against authority loops."),
        new(
            "p09-network-closure-root-count",
            "P09 network closure x independent-root count",
            "09-authority-ancestry-circular-standing",
            "Does a densely recurrent social topology itself suppress legitimate authority, or can several independent roots remain useful even when endorsements repeatedly return through the network?",
            "All contexts are useful. The wraparound edge of the five-peer ring is scaled from an acyclic chain to full closure while the number of independent direct roots varies. Frozen P09 transfer and receiver equations are otherwise retained.",
            Axis("network_closure", "Network closure", "Strength of the ring-closing edge that allows authority to return toward its origin.", 0.00, 0.15, 0.30, 0.50, 0.70, 0.85, 1.00),
            Axis("independent_root_count", "Independent root count", "Effective number of separately grounded roots contributing to useful authority; half steps give the final root proportional initial standing.", 1.0, 1.5, 2.0, 2.5, 3.0, 4.0, 5.0),
            ReplicatesPerCell,
            AuthorityAncestryProbes.EvaluateNetworkClosureVersusRootCount,
            "boundary_margin",
            "Positive means useful authority remains above 0.50 and ancestry-sensitive early error remains below direct-only error despite recurrent topology.",
            "This surface varies only one ring-closing edge. It is a minimal topology probe, not a claim that mature social networks are rings."),
    ];

    private static FalsificationAxis Axis(string name, string label, string description, params double[] values) =>
        new(name, label, description, values);
}
