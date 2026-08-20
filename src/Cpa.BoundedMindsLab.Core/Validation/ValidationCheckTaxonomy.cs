namespace Cpa.BoundedMindsLab.Validation;

public static class ValidationCheckTaxonomy
{
    public const string Manipulation = "manipulation";
    public const string MechanismOutcome = "mechanism-outcome";
    public const string SafetyBoundary = "safety-boundary";
    public const string AccountingConstraint = "accounting-constraint";

    public static string Classify(string assertionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assertionName);

        if (assertionName.StartsWith("seed-generates-", StringComparison.Ordinal)
            || assertionName is "complementary-private-histories"
            or "preserved-disagreement"
            or "typed-surface-preserves-public-disagreement"
            or "provenance-is-partial-rather-than-clean"
            or "seed-generates-strategic-social-world"
            or "strategic-sender-discovers-naive-leverage")
        {
            return Manipulation;
        }

        if (assertionName is "bounded-communication"
            or "bounded-public-exchange"
            or "bounded-developmental-transfer"
            or "typed-communication-remains-bounded"
            or "earned-convention-compresses-communication"
            or "standing-transfer-is-bounded-public-communication"
            or "strategic-public-exchange-is-bounded")
        {
            return AccountingConstraint;
        }

        if (assertionName is "bounded-contamination"
            or "late-local-revision"
            or "provenance-selectivity"
            or "plurality-remains-correctable"
            or "direct-consequence-remains-sovereign"
            or "misleading-dissent-remains-disciplined"
            or "shared-consequence-converges"
            or "changed-conditions-revise-convention"
            or "localized-change-preserves-stable-conventions"
            or "independent-convergence-remains-independent"
            or "independent-roots-are-not-overmerged"
            or "inference-approaches-perfect-ancestry-with-bounded-public-data"
            or "provisional-standing-avoids-inherited-doctrine"
            or "opportunity-cost-remains-bounded-versus-no-transfer"
            or "direct-consequence-revokes-strong-local-mismatch"
            or "betrayal-remains-correctable"
            or "public-claims-do-not-become-authority"
            or "opportunity-cost-remains-bounded-versus-local")
        {
            return SafetyBoundary;
        }

        return MechanismOutcome;
    }
}
