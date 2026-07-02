using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

public sealed record AdjudicationGateResult(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("errorCount")] int ErrorCount,
    [property: JsonPropertyName("pendingCount")] int PendingCount,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks,
    [property: JsonPropertyName("violations")] IReadOnlyList<GateViolation> Violations);

/// <summary>
/// L4-analoges deterministisches Gate über eine ausgefüllte Adjudikations-Queue (Bauvorschlag §1.4).
/// `pass=true` heißt: JEDES Item hat eine bewusste Aktion (auch `defer`) — NICHT „alles final entschieden".
/// KEIN LLM. Erzeugt zugleich Audit-Ledger + konsumierbare Projektion (§1.6).
/// </summary>
public static class AdjudicationCompletenessGate
{
    // Quelle der Wahrheit: ledger-taxonomy.md. Hier gespiegelt für die apply_repair-Zielprüfung.
    private static readonly Dictionary<string, string[]> RepairableFacetValues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["status"] = ["decided", "open", "rejected", "uncertain", "required"],
        ["modality"] = ["must", "must_clarify", "must_consider", "must_note", "must_not", "desired", "optional"],
        ["timescope"] = ["mvp", "later_possible", "mvp_or_later_unclear"]
    };

    public static AdjudicationGateResult Evaluate(IReadOnlyList<AdjudicationItem> items)
    {
        var v = new List<GateViolation>();

        // 1) jedes Item hat eine gültige Aktion.
        var unadjRR = items.Where(i => i.ItemType == "review_required_claim" && !AdjudicationActions.IsValid(i.Action)).Select(i => i.ItemId).ToList();
        if (unadjRR.Count > 0)
            v.Add(new("error", "UNADJUDICATED_REVIEW_REQUIRED", "review_required-Claim ohne gültige Aktion.", unadjRR));

        var unadjMiss = items.Where(i => i.ItemType is "coverage_miss" or "unit_signal" && !AdjudicationActions.IsValid(i.Action)).Select(i => i.ItemId).ToList();
        if (unadjMiss.Count > 0)
            v.Add(new("error", "UNADJUDICATED_COVERAGE_MISS", "Coverage-/Unit-Signal ohne gültige Aktion.", unadjMiss));

        // 2) apply_repair-Ziel muss taxonomie-konform sein (zweite Verteidigungslinie).
        var badRepair = items.Where(i =>
            string.Equals(i.Action, AdjudicationActions.ApplyRepair, StringComparison.OrdinalIgnoreCase)
            && i.SystemSuggestion?.Facet is { } f
            && RepairableFacetValues.TryGetValue(f, out var allowed)
            && (i.SystemSuggestion.Suggested is null || !allowed.Contains(i.SystemSuggestion.Suggested)))
            .Select(i => i.ItemId).ToList();
        if (badRepair.Count > 0)
            v.Add(new("error", "INVALID_REPAIR_TARGET", "apply_repair mit Zielwert ausserhalb der Taxonomie.", badRepair));

        // 3) merge_existing / mark_covered_by brauchen ein referenceTarget.
        var missingTarget = items.Where(i =>
            (string.Equals(i.Action, AdjudicationActions.MergeExisting, StringComparison.OrdinalIgnoreCase)
             || string.Equals(i.Action, AdjudicationActions.MarkCoveredBy, StringComparison.OrdinalIgnoreCase))
            && string.IsNullOrWhiteSpace(i.ReferenceTarget))
            .Select(i => i.ItemId).ToList();
        if (missingTarget.Count > 0)
            v.Add(new("error", "MISSING_REFERENCE_TARGET", "merge_existing/mark_covered_by ohne referenceTarget.", missingTarget));

        var pending = items.Count(i => string.Equals(i.Action, AdjudicationActions.Defer, StringComparison.OrdinalIgnoreCase));
        var errors = v.Count(x => x.Severity == "error");
        var checks = new Dictionary<string, object>
        {
            ["items"] = items.Count,
            ["byAction"] = items.GroupBy(i => i.Action ?? "<none>").ToDictionary(g => g.Key, g => g.Count()),
            ["reviewRequired"] = items.Count(i => i.ItemType == "review_required_claim"),
            ["coverageMiss"] = items.Count(i => i.ItemType == "coverage_miss"),
            ["unitSignal"] = items.Count(i => i.ItemType == "unit_signal"),
        };
        return new AdjudicationGateResult(errors == 0, errors, pending, checks, v);
    }

    /// <summary>Baut Audit-Ledger + konsumierbare Projektion aus der ausgefüllten Queue + dem validated Ledger (§1.6).</summary>
    public static (AdjudicatedLedger audit, ConsumableLedger consumable) Project(
        AdjudicationQueue filled, ValidatedLedger validated)
    {
        var byClaimId = validated.Entries.ToDictionary(e => e.Entry.Id, e => e.Entry, StringComparer.Ordinal);
        var records = new List<AdjudicationRecord>();
        var consumable = new List<SemanticLedgerEntry>();

        // Basis: alle approved Claims wandern unverändert in die Projektion.
        var approved = validated.Entries.Where(e => e.ClaimStatus == "approved").Select(e => e.Entry).ToList();
        consumable.AddRange(approved);

        foreach (var i in filled.Items)
        {
            var action = i.Action ?? "";
            SemanticLedgerEntry? resulting = null;

            switch (action.ToLowerInvariant())
            {
                case AdjudicationActions.ApplyRepair when i.ClaimId is { } cid && byClaimId.TryGetValue(cid, out var baseC):
                    resulting = ApplyFacetRepair(baseC, i.SystemSuggestion);
                    consumable.Add(resulting);
                    break;
                case AdjudicationActions.AcceptGap:
                    // review_required als-ist übernehmen ODER Miss als neuen Claim aufnehmen.
                    resulting = i.ClaimId is { } c2 && byClaimId.TryGetValue(c2, out var baseE)
                        ? baseE
                        : BuildGapClaim(i);
                    consumable.Add(resulting);
                    break;
                case AdjudicationActions.MergeExisting:
                case AdjudicationActions.MarkCoveredBy:
                case AdjudicationActions.Reject:
                case AdjudicationActions.Defer:
                    // fügen KEINEN Claim in die Projektion (merge/covered = Referenz, reject = raus, defer = pending).
                    break;
            }

            records.Add(new AdjudicationRecord(i.ItemId, i.ItemType, action, resulting, i.ReferenceTarget,
                string.IsNullOrWhiteSpace(i.ActionReason) ? i.Reason : i.ActionReason!));
        }

        var pending = filled.Items.Count(i => string.Equals(i.Action, AdjudicationActions.Defer, StringComparison.OrdinalIgnoreCase));
        var now = DateTime.UtcNow.ToString("o");
        var audit = new AdjudicatedLedger(filled.SourceValidatedRunId, filled.SourceUnitRunId, "author", now, approved, records, pending);
        var cons = new ConsumableLedger(now, consumable, pending,
            "Finale Claim-Menge = approved + accept_gap/apply_repair - reject; defer bleibt pending (nicht enthalten).");
        return (audit, cons);
    }

    private static SemanticLedgerEntry ApplyFacetRepair(SemanticLedgerEntry baseC, AdjudicationSuggestion? s)
    {
        if (s?.Facet is null || s.Suggested is null) return baseC;
        return s.Facet.ToLowerInvariant() switch
        {
            "status" => baseC with { Status = s.Suggested },
            "modality" => baseC with { Modality = s.Suggested },
            "scope" => baseC with { Scope = s.Suggested },
            "timescope" => baseC with { TimeScope = s.Suggested },
            _ => baseC
        };
    }

    private static SemanticLedgerEntry BuildGapClaim(AdjudicationItem i)
    {
        var disp = new Dictionary<string, ArtifactDisposition>
        {
            ["requirements"] = new("context", "assumption"),
            ["architecture"] = new("context", "assumption"),
            ["risks"] = new("context", "assumption"),
            ["open-questions"] = new("required", "question"),
        };
        return new SemanticLedgerEntry(
            Id: $"ADJ-GAP-{(i.UnitId ?? i.ItemId).Replace("::", "-")}",
            Proposition: i.Proposition,
            Kind: "open_requirement",
            Status: "open",
            Modality: "must_note",
            Scope: "author-adjudicated gap",
            TimeScope: "mvp_or_later_unclear",
            Evidence: i.EvidenceRefs.Select(r => new SemanticLedgerEvidence("adjudication", r)).ToList(),
            Disposition: disp,
            RiskLevel: "medium",
            Notes: "author-adjudicated gap (accept_gap); Facetten pending -> ggf. erneut facet-validieren.");
    }
}
