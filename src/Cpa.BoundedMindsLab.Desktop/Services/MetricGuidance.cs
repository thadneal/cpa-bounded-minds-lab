namespace Cpa.BoundedMindsLab.Desktop.Services;

public sealed record MetricGuidance(
    string ValueDescription,
    string Preference,
    string TimeXAxisDescription,
    string ComparisonXAxisDescription)
{
    public static MetricGuidance For(string metric)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);
        var normalized = metric;

        if (normalized.Contains("rmse", StringComparison.Ordinal))
        {
            return Guidance(
                "Root mean square prediction error.",
                "Lower is better.");
        }

        if (normalized.Contains("absolute_error", StringComparison.Ordinal) ||
            normalized.EndsWith("_error", StringComparison.Ordinal))
        {
            return Guidance(
                "Prediction or estimation error magnitude.",
                "Lower is better.");
        }

        if (normalized.Contains("regret", StringComparison.Ordinal))
        {
            return Guidance(
                "Utility lost relative to the best available coordinated choice.",
                "Lower is better.");
        }

        if (normalized.Contains("communication_work", StringComparison.Ordinal))
        {
            return Guidance(
                "Normalized communication cost accumulated by the treatment.",
                "Lower is better when outcome quality is comparable.");
        }

        if (normalized.Contains("packet_count", StringComparison.Ordinal) ||
            normalized.Contains("published_trace_count", StringComparison.Ordinal) ||
            normalized.Contains("received_trace_count", StringComparison.Ordinal))
        {
            return Guidance(
                "Number of public packets or transferred traces exchanged.",
                "Lower is better when outcome quality is comparable.");
        }

        if (normalized.Contains("utility", StringComparison.Ordinal))
        {
            return Guidance(
                "Outcome utility earned by the treatment or coordination path.",
                "Higher is better.");
        }

        if (normalized.Contains("coordination_success", StringComparison.Ordinal) ||
            normalized.EndsWith("_success", StringComparison.Ordinal))
        {
            return Guidance(
                "Successful coordination or outcome rate.",
                "Higher is better.");
        }

        if (normalized.Contains("coverage", StringComparison.Ordinal) ||
            normalized.Contains("retention", StringComparison.Ordinal) ||
            normalized.Contains("revision_coverage", StringComparison.Ordinal))
        {
            return Guidance(
                "Share of relevant contexts satisfying the named condition.",
                "Higher is better for the named desired condition.");
        }

        if (normalized.Contains("echo_pair_recall", StringComparison.Ordinal))
        {
            return Guidance(
                "Share of truly shared-root report pairs recovered into the same inferred ancestry group.",
                "Higher is better.");
        }

        if (normalized.Contains("false_merge_rate", StringComparison.Ordinal))
        {
            return Guidance(
                "Share of report pairs from independent hidden roots incorrectly merged as one ancestry group.",
                "Lower is better.");
        }

        if (normalized.Contains("effective_support_groups", StringComparison.Ordinal))
        {
            return Guidance(
                "Number of ancestry-distinct support groups the reducer treats as independently corroborative.",
                "Context-dependent; closer to the true number of independent roots is preferred.");
        }

        if (normalized.Contains("agreement", StringComparison.Ordinal))
        {
            return Guidance(
                "Degree of agreement among the participating peers.",
                "Context-dependent; higher is useful after warranted convergence.");
        }

        if (normalized.Contains("disagreement", StringComparison.Ordinal))
        {
            return Guidance(
                "Distance between peer public predictions or postures.",
                "Context-dependent; informative early disagreement can help, while unresolved late disagreement is usually lower-better.");
        }

        if (normalized.Contains("standing", StringComparison.Ordinal))
        {
            return Guidance(
                "Current permission or influence weight granted to the named evidence or convention.",
                "Context-dependent; higher is preferred only when that influence remains warranted.");
        }

        if (normalized.Contains("uncertainty", StringComparison.Ordinal))
        {
            return Guidance(
                "Current uncertainty carried by the public estimate.",
                "Calibrated is preferred; lower is better only when evidence warrants confidence.");
        }

        if (normalized.Contains("evidence", StringComparison.Ordinal))
        {
            return Guidance(
                "Amount or span of accumulated evidence.",
                "More evidence can strengthen support, but volume alone is not correctness.");
        }

        if (normalized.Contains("shortcut_rate", StringComparison.Ordinal))
        {
            return Guidance(
                "Share of coordination episodes using an earned convention shortcut.",
                "Higher is better only while utility and revisability remain acceptable.");
        }

        if (normalized.Contains("switch_count", StringComparison.Ordinal))
        {
            return Guidance(
                "Number of convention or action switches under changed conditions.",
                "Context-dependent; enough revision to track real change is preferred over either rigidity or churn.");
        }

        if (normalized.Contains("fingerprint", StringComparison.Ordinal) ||
            normalized is "seed" or "context_cell" or "context_kind" or "history_kind" or "regime" or "selected_action" or "target")
        {
            return Guidance(
                "Diagnostic identity or categorical value used to distinguish generated circumstances.",
                "No higher/lower preference; use it to compare histories.");
        }

        if (normalized.Contains("prediction", StringComparison.Ordinal) || normalized.Contains("estimate", StringComparison.Ordinal))
        {
            return Guidance(
                "Current prediction or estimated value.",
                "No intrinsic direction; closer to the relevant target is preferred.");
        }

        if (normalized.EndsWith("_count", StringComparison.Ordinal) || normalized.Contains("contexts", StringComparison.Ordinal) || normalized.Contains("cells", StringComparison.Ordinal))
        {
            return Guidance(
                "Count of the named events, contexts, or structural features.",
                "Context-dependent; interpret against the protocol's expected condition.");
        }

        if (normalized.EndsWith("_rate", StringComparison.Ordinal))
        {
            return Guidance(
                "Rate of the named event or behavior.",
                "Context-dependent; interpret against the protocol's expected condition.");
        }

        return Guidance(
            "Numeric telemetry value emitted by the selected protocol path.",
            "Context-dependent; interpret against the protocol's falsification checks.");
    }

    private static MetricGuidance Guidance(string description, string preference) => new(
        description,
        preference,
        "Observation/tick order; later activity moves to the right.",
        "Treatment or focus path; category position has no temporal meaning.");
}
