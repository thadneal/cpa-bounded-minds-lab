namespace Cpa.BoundedMindsLab.Falsification;

public static class ParameterizedFalsificationPlan
{
    public const string Name = "parameterized-falsification-v1";
    public const int ReplicatesPerCell = 7;

    public static IReadOnlyList<FalsificationProfileDefinition> Profiles { get; } =
    [
        new(
            "p03-history-informativeness",
            "P03 developmental-history informativeness",
            "03-developmental-versus-doctrinal-transfer",
            "When does developmental history improve transfer, and when does remembering instability merely down-weight a final rule that is now locally useful?",
            "Micro-assay copies the frozen P03 developmental-standing calculation and receiver update equations. The axes independently vary history instability and the current local error of the transferred final rule.",
            Axis("history_instability", "History instability", "Segment spread and within-history variation carried by the developmental packet.", 0.00, 0.10, 0.20, 0.35, 0.50, 0.70, 0.90),
            Axis("present_rule_error", "Present rule error", "Absolute error between the source's transferred final rule and the receiver's current local target.", 0.00, 0.05, 0.10, 0.15, 0.25, 0.40, 0.60),
            ReplicatesPerCell,
            ParameterizedProbes.EvaluateProtocol03,
            "boundary_margin",
            "Positive means developmental transfer has lower RMSE than doctrine. Negative means developmental history has become net harmful in this controlled cell.",
            "This probe isolates the frozen local equations rather than reproducing the full ten-context P03 generator. It maps causal pressure, not a new validation rate."),
        new(
            "p04-equal-budget-comparator",
            "P04 equal-budget comparator",
            "04-bounded-communication-before-language",
            "Does the typed public posture still help when the alternative receives the same public packets and uses a robust consensus rule rather than destructive semantic smoothing?",
            "Both paths receive identical estimate, standing, and uncertainty packets. The frozen typed reducer uses its original weighted combination; the stronger comparator uses a standing-weighted median. No extra communication is granted to either path.",
            Axis("warrant_asymmetry", "Warrant asymmetry", "How strongly the minority report's standing differs from the two-report majority.", 0.00, 0.10, 0.20, 0.35, 0.50, 0.70, 0.90),
            Axis("minority_correct_fraction", "Minority-correct fraction", "Fraction of synthetic contexts in which the minority estimate is closer to the hidden target than the majority estimates.", 0.00, 0.15, 0.30, 0.50, 0.70, 0.85, 1.00),
            ReplicatesPerCell,
            ParameterizedProbes.EvaluateProtocol04,
            "boundary_margin",
            "Positive means the frozen typed weighted reducer has lower RMSE than the equal-budget robust-consensus comparator. Negative means the stronger comparator wins.",
            "This is deliberately a control-strengthening assay, not a claim about natural language. It asks whether preserving graded epistemic shape is useful against a competent same-information alternative."),
        new(
            "p05-volatility-surface",
            "P05 convention volatility surface",
            "05-emergent-convention-artificial-culture",
            "At what combination of change frequency and change magnitude does earned convention cease to compress coordination without unacceptable utility loss or churn?",
            "A one-context repeated-interaction micro-world copies the frozen P05 convention standing, shortcut, negotiation, and revision rules. Regime changes are imposed directly rather than selected from the original generator.",
            Axis("change_frequency", "Change frequency", "Probability that the current coordination regime changes before an episode.", 0.00, 0.05, 0.10, 0.20, 0.35, 0.55, 0.80),
            Axis("change_magnitude", "Change magnitude", "Penalty imposed on continuing the prior convention after a regime change.", 0.00, 0.10, 0.20, 0.35, 0.50, 0.70, 0.90),
            ReplicatesPerCell,
            ParameterizedProbes.EvaluateProtocol05,
            "boundary_margin",
            "Positive means the adaptive convention remains within the frozen 2% utility allowance and the 35% communication-work allowance relative to fresh negotiation. Negative marks a controlled failure region.",
            "The probe intentionally goes beyond the original one-shift world family. It is an operating-envelope instrument, not a replacement Protocol 05 result."),
        new(
            "p06-ancestry-opacity",
            "P06 ancestry opacity surface",
            "06-incomplete-epistemic-ancestry",
            "How much provenance loss and signature overlap can the frozen ancestry heuristic tolerate before echo discounting becomes net harmful or independent convergence is suppressed?",
            "Synthetic echo-trap and independent-convergence contexts use the frozen P06 merge distance, report support, grouping, and group prediction equations. Missingness and root-signature separation are controlled directly.",
            Axis("origin_missingness", "Origin missingness", "Probability that a report carries no usable root origin hint.", 0.00, 0.15, 0.30, 0.45, 0.60, 0.80, 1.00),
            Axis("signature_separation", "Signature separation", "Distance between developmental-signature centers belonging to independent roots.", 0.05, 0.10, 0.15, 0.22, 0.30, 0.45, 0.70),
            ReplicatesPerCell,
            ParameterizedProbes.EvaluateProtocol06,
            "boundary_margin",
            "Positive means the frozen 12% whole-history benefit and 15% independent-convergence safety allowance both hold. Negative means at least one boundary fails.",
            "This probe can reach complete provenance blindness and signature overlap beyond the frozen generator. It should reveal when ancestry inference ought to become uncertain rather than confident."),
        new(
            "p07-reliability-prevalence",
            "P07 recommender reliability x mismatch prevalence",
            "07-provisional-standing-transfer",
            "When does second-hand standing stop paying for itself as recommender credibility falls and locally mismatched recommendations become more common?",
            "The probe copies the frozen P07 provisional-standing admission, prediction, direct-learning, and standing-update equations across a controlled twelve-context world.",
            Axis("recommender_credibility", "Recommender credibility", "Receiver C's already-earned standing for recommender A.", 0.00, 0.15, 0.30, 0.45, 0.60, 0.75, 0.95),
            Axis("mismatch_prevalence", "Mismatch prevalence", "Fraction of strongly recommended contexts whose source relationship fails to generalize to the receiver.", 0.00, 0.15, 0.30, 0.50, 0.65, 0.80, 1.00),
            ReplicatesPerCell,
            ParameterizedProbes.EvaluateProtocol07Prevalence,
            "boundary_margin",
            "Positive means provisional transfer stays within 5% of no-transfer total RMSE and at least 7% better than inherited authority. Negative marks a net social-transfer failure region.",
            "Mismatch severity is held strong in this surface so prevalence can vary independently. A second P07 surface varies severity separately."),
        new(
            "p07-reliability-severity",
            "P07 recommender reliability x mismatch severity",
            "07-provisional-standing-transfer",
            "When does otherwise useful social recommendation become sticky or costly as local mismatch severity rises?",
            "The frozen P07 equations are run with mismatch prevalence fixed at one half while recommender credibility and source-target divergence vary independently.",
            Axis("recommender_credibility", "Recommender credibility", "Receiver C's already-earned standing for recommender A.", 0.00, 0.15, 0.30, 0.45, 0.60, 0.75, 0.95),
            Axis("mismatch_severity", "Mismatch severity", "Absolute divergence imposed between a strongly recommended source estimate and the receiver's local target. The sweep begins in the strong-mismatch regime because the frozen 0.20 residual-standing safety boundary was registered only for strong local contradiction.", 0.60, 0.70, 0.80, 0.90, 1.00, 1.10, 1.25),
            ReplicatesPerCell,
            ParameterizedProbes.EvaluateProtocol07Severity,
            "boundary_margin",
            "Positive means opportunity cost remains bounded and final standing for mismatched sources falls below the frozen 0.20 safety ceiling. Negative identifies cost or residual-authority failure.",
            "This separates severity from prevalence so the two failure modes exposed by holdout-v1 and challenge-v1 are not hidden inside one composite stress score."),
    ];

    private static FalsificationAxis Axis(string name, string label, string description, params double[] values) =>
        new(name, label, description, values);
}
