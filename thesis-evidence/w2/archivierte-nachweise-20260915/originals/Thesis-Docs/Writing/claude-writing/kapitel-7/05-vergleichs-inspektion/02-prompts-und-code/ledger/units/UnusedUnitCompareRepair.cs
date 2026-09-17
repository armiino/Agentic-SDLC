namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// R-1 (E2E-Reibungs-Log, 2026-07-23): Deckungs-Urteile (<c>attach_as_evidence</c>/<c>already_covered_indirectly</c>)
/// ohne gueltige Candidate-Referenz sind potenziell halluzinierte Deckung — hinter ihnen kann eine still
/// verlorene Anforderung stecken. Diese pure Regel-Klasse entscheidet, WER repariert werden muss und wie das
/// ehrliche Downgrade aussieht: <c>needs_human</c> → Adjudikations-Queue (Discernment: Unklares gehoert zum
/// Menschen, nicht in den Ganz-Lauf-Fail). Der LLM-Nachfrage-Pass selbst lebt im <c>UnusedUnitLedgerComparer</c>.
/// </summary>
public static class UnusedUnitCompareRepair
{
    public const string DowngradePrefix = "[auto-downgrade: Deckung ohne gueltige Candidate-Referenz] ";

    private static readonly string[] CoverageVerdicts = ["attach_as_evidence", "already_covered_indirectly"];

    /// <summary>true, wenn das Urteil Deckung behauptet, aber keine EXISTIERENDE Candidate-ID benennt.</summary>
    public static bool ClaimsCoverageWithoutValidReference(UnusedUnitLedgerCompareItem item, IReadOnlySet<string> candidateIds)
        => CoverageVerdicts.Contains(item.Verdict, StringComparer.OrdinalIgnoreCase)
           && !(item.RelatedCandidateIds ?? []).Any(candidateIds.Contains);

    /// <summary>Deterministisches Downgrade eines unreparierbaren Deckungs-Urteils.</summary>
    public static UnusedUnitLedgerCompareItem Downgrade(UnusedUnitLedgerCompareItem item) => item with
    {
        Verdict = "needs_human",
        SuggestedAction = "human_review",
        Reason = DowngradePrefix + item.Reason,
        RelatedCandidateIds = []
    };
}
