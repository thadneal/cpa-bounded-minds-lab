namespace Cpa.BoundedMindsLab.Falsification;

public static class StrategicInfluenceFalsificationPlan
{
    public const string Name = "strategic-influence-falsification-v1";
    public const int ReplicatesPerCell = 7;

    public static IReadOnlyList<FalsificationProfileDefinition> Profiles { get; } =
    [
        new(
            "p08-delay-vs-adaptation",
            "P08 consequence delay x sender adaptation",
            "08-strategic-public-influence",
            "How much delay between public influence and direct consequence can the accountable receiver tolerate as a strategic sender learns presentation more quickly?",
            "The accountable receiver keeps the frozen Protocol 08 prediction, standing, calibration, and direct-observation equations. Consequence delivery is delayed by a controlled number of same-context exposures. Sender reward learning generalizes the frozen sample-mean update with a speed multiplier; speed 1.0 reproduces the frozen update.",
            Axis("consequence_delay", "Consequence delay", "Same-context exposures between a public influence event and delivery of its direct consequence to the receiver.", 0, 1, 2, 4, 6, 10, 16),
            Axis("sender_adaptation_speed", "Sender adaptation speed", "Multiplier on how quickly the strategic sender updates tactic value from observable receiver influence. 1.0 is the frozen Protocol 08 sender.", 0.50, 1.00, 1.50, 2.00, 3.00, 5.00, 8.00),
            ReplicatesPerCell,
            StrategicInfluenceProbes.EvaluateDelayVersusAdaptation,
            "boundary_margin",
            "Positive means useful aligned help, whole-world capture resistance, and the 5% local-only opportunity-cost allowance still hold. When the naive sender actually reaches the original 0.50 leverage manipulation threshold, the margin also requires at least 0.15 less divergent assertiveness under accountability.",
            "Sender speeds other than 1.0 are adversarial capability extensions, not changes proposed for CPA. The receiver mechanism remains frozen."),
        new(
            "p08-betrayal-timing-vs-severity",
            "P08 betrayal timing x severity",
            "08-strategic-public-influence",
            "How much previously earned standing can remain safely revisable when a peer changes objective at different stages and by different amounts?",
            "Two of twelve contexts begin aligned and then change objective after a controlled exposure count. Betrayal severity directly sets the sender-target divergence after the change while the frozen accountable and naive receiver equations remain unchanged.",
            Axis("betrayal_timing", "Betrayal timing", "Context exposure at which the sender stops matching the receiver and begins pursuing a divergent objective.", 2, 5, 8, 10, 14, 20, 26),
            Axis("betrayal_severity", "Betrayal severity", "Absolute post-change divergence between sender objective and receiver target before clamping to the representable range.", 0.20, 0.35, 0.50, 0.65, 0.80, 1.00, 1.30),
            ReplicatesPerCell,
            StrategicInfluenceProbes.EvaluateBetrayalTimingVersusSeverity,
            "boundary_margin",
            "Positive means the frozen betrayal repair boundary still holds: accountable late betrayal error stays below 20% of naive and final betrayal standing stays at or below 0.10.",
            "The surface separates the amount of prior earned influence from the magnitude of later contradiction. It is not a claim that one fixed standing ceiling should govern all real relationships."),
        new(
            "p08-divergence-vs-delay",
            "P08 divergence prevalence x consequence delay",
            "08-strategic-public-influence",
            "When does keeping a strategically divergent peer available cease to pay for itself as divergence becomes common and consequence becomes sparse or late?",
            "The controlled world varies how many of twelve contexts are persistently divergent while the rest are aligned. Direct consequence is delayed independently. The primary boundary is the frozen 5% opportunity-cost allowance relative to local-only learning, with total capture resistance retained as a secondary metric.",
            Axis("divergence_prevalence", "Divergence prevalence", "Fraction of social contexts in which the sender's objective persistently diverges from the receiver's target.", 0.00, 0.15, 0.30, 0.50, 0.65, 0.80, 1.00),
            Axis("consequence_delay", "Consequence delay", "Same-context exposures before direct receiver consequence is available.", 0, 1, 2, 4, 6, 10, 16),
            ReplicatesPerCell,
            StrategicInfluenceProbes.EvaluateDivergenceVersusDelay,
            "boundary_margin",
            "Positive means accountable social openness remains within 5% of local-only total RMSE. Negative means the peer has become net costly under the controlled prevalence/delay combination.",
            "At zero divergence, naive self-report may legitimately perform well because the sender is actually useful. The primary margin therefore uses local-only opportunity cost rather than requiring accountable treatment to beat naive control everywhere."),
        new(
            "p08-feedback-vs-adaptation",
            "P08 feedback observability x sender adaptation",
            "08-strategic-public-influence",
            "How much can a strategic sender learn from public receiver reactions before consequence-grounded calibration stops reducing exploitability?",
            "The sender receives its ordinary public influence reward only on a controlled fraction of interactions. Sender adaptation speed is varied independently. The receiver still exposes no private standing, local estimate, or calibration state.",
            Axis("feedback_observability", "Feedback observability", "Probability that the sender can use the receiver's public reaction to update its tactic value after an interaction.", 0.00, 0.15, 0.30, 0.50, 0.70, 0.85, 1.00),
            Axis("sender_adaptation_speed", "Sender adaptation speed", "Multiplier on strategic tactic-value learning; 1.0 is the frozen sender update.", 0.50, 1.00, 1.50, 2.00, 3.00, 5.00, 8.00),
            ReplicatesPerCell,
            StrategicInfluenceProbes.EvaluateFeedbackVersusAdaptation,
            "boundary_margin",
            "Positive means accountable treatment still reduces total capture. When the naive sender actually reaches the original 0.50 leverage manipulation threshold, the margin also requires late divergent assertiveness to be at least 0.15 lower under accountability.",
            "This surface explores an adversary capability envelope. It does not assume that more sender observability is intrinsically undesirable; public feedback is also how cooperative peers can adapt."),
        new(
            "p08-aligned-noise-vs-delay",
            "P08 aligned evidence noise x consequence delay",
            "08-strategic-public-influence",
            "Can accountability become so conservative under noisy or delayed consequence that a genuinely useful peer loses earned influence or early benefit?",
            "All controlled contexts are aligned. Receiver observations become progressively noisier while direct consequence is independently delayed. The sender remains strategically adaptive, but there is no hidden objective divergence to justify suppressing it.",
            Axis("observation_noise", "Observation noise", "Amplitude of receiver-side direct-consequence noise around the underlying aligned target.", 0.01, 0.03, 0.06, 0.10, 0.16, 0.24, 0.35),
            Axis("consequence_delay", "Consequence delay", "Same-context exposures before receiver-owned calibration can use direct consequence.", 0, 1, 2, 4, 6, 10, 16),
            ReplicatesPerCell,
            StrategicInfluenceProbes.EvaluateAlignedNoiseVersusDelay,
            "boundary_margin",
            "Positive means accountable influence still improves early aligned RMSE by at least 25% versus local-only and final aligned standing remains at or above 0.85. Negative is an over-deterrence region.",
            "This is deliberately a null-harm probe: there is no strategic divergence. Failure means the defense is suppressing or miscalibrating a useful peer rather than successfully resisting manipulation."),
    ];

    private static FalsificationAxis Axis(string name, string label, string description, params double[] values) =>
        new(name, label, description, values);
}
