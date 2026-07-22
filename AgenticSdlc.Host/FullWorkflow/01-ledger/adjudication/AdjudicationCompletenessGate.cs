using System.Text.Json.Serialization;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

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

        // 2) apply_repair-Ziele muessen taxonomie-konform sein (zweite Verteidigungslinie).
        var badRepair = items.Where(i =>
            string.Equals(i.Action, AdjudicationActions.ApplyRepair, StringComparison.OrdinalIgnoreCase)
            && EffectiveAppliedRepairSuggestions(i).Any(s =>
                s.Facet is { } f
                && RepairableFacetValues.TryGetValue(f, out var allowed)
                && (s.Suggested is null || !allowed.Contains(s.Suggested))))
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

        // 4) attach_evidence braucht ein Ziel: referenceTarget ODER (Fallback) das claimId des Items.
        var missingAttachTarget = items.Where(i =>
            string.Equals(i.Action, AdjudicationActions.AttachEvidence, StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(i.ReferenceTarget) && string.IsNullOrWhiteSpace(i.ClaimId))
            .Select(i => i.ItemId).ToList();
        if (missingAttachTarget.Count > 0)
            v.Add(new("error", "MISSING_ATTACH_TARGET", "attach_evidence ohne referenceTarget und ohne claimId.", missingAttachTarget));

        var pending = items.Count(i => string.Equals(i.Action, AdjudicationActions.Defer, StringComparison.OrdinalIgnoreCase));
        // Neu geminteten (accept_gap/promote_to_claim) Claims fehlen noch die Facetten -> Refine-Pass A10 nötig.
        var pendingFacets = items.Count(i =>
            string.Equals(i.Action, AdjudicationActions.PromoteToClaim, StringComparison.OrdinalIgnoreCase)
            || (string.Equals(i.Action, AdjudicationActions.AcceptGap, StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(i.ClaimId)));
        var errors = v.Count(x => x.Severity == "error");
        var checks = new Dictionary<string, object>
        {
            ["items"] = items.Count,
            ["byAction"] = items.GroupBy(i => i.Action ?? "<none>").ToDictionary(g => g.Key, g => g.Count()),
            ["reviewRequired"] = items.Count(i => i.ItemType == "review_required_claim"),
            ["coverageMiss"] = items.Count(i => i.ItemType == "coverage_miss"),
            ["unitSignal"] = items.Count(i => i.ItemType == "unit_signal"),
            ["pendingFacets"] = pendingFacets, // info: so viele neue Claims warten auf ledger-adjudicate-refine (A10)
        };
        return new AdjudicationGateResult(errors == 0, errors, pending, checks, v);
    }

    /// <summary>Baut Audit-Ledger + konsumierbare Projektion aus der ausgefüllten Queue + dem validated Ledger (§1.6).</summary>
    /// <param name="units">step-00 Atomic-Units (unitId -> Unit), damit accept_gap/promote/attach die ECHTE
    /// Transkript-Evidenz der Unit nutzen statt Claim-ID-Refs. null = Fallback (Claim-Ref-Evidenz, altes Verhalten).</param>
    public static (AdjudicatedLedger audit, ConsumableLedger consumable) Project(
        AdjudicationQueue filled, ValidatedLedger validated,
        IReadOnlyDictionary<string, AtomicUnit>? units = null)
    {
        var byClaimId = validated.Entries.ToDictionary(e => e.Entry.Id, e => e.Entry, StringComparer.Ordinal);
        var records = new List<AdjudicationRecord>();

        // Basis: alle approved Claims wandern unverändert in die Projektion. Als geordnete id->Entry-Map,
        // damit attach_evidence den Ziel-Claim in-place anreichern kann (unabhängig von der Item-Reihenfolge).
        var approved = validated.Entries.Where(e => e.ClaimStatus == "approved").Select(e => e.Entry).ToList();
        var consumable = new List<SemanticLedgerEntry>(approved);
        int IndexOfClaim(string id) => consumable.FindIndex(e => string.Equals(e.Id, id, StringComparison.Ordinal));

        // Phase 1: claim-erzeugende/-reparierende Aktionen. attach_evidence wird gesammelt und erst in Phase 2
        // angewandt (Ziel kann auch ein in Phase 1 erst entstandener Claim sein).
        var attachOps = new List<(int recordIndex, AdjudicationItem item, string targetId)>();
        foreach (var i in filled.Items)
        {
            var action = i.Action ?? "";
            SemanticLedgerEntry? resulting = null;

            switch (action.ToLowerInvariant())
            {
                case AdjudicationActions.ApplyRepair when i.ClaimId is { } cid && byClaimId.TryGetValue(cid, out var baseC):
                    resulting = ApplyFacetRepairs(baseC, EffectiveAppliedRepairSuggestions(i));
                    consumable.Add(resulting);
                    break;
                case AdjudicationActions.AcceptGap:
                    // review_required als-ist übernehmen ODER Miss als neuen Claim aufnehmen.
                    resulting = i.ClaimId is { } c2 && byClaimId.TryGetValue(c2, out var baseE)
                        ? baseE
                        : BuildGapClaim(i, units, promoted: false);
                    consumable.Add(resulting);
                    break;
                case AdjudicationActions.PromoteToClaim:
                    // A8: Autor überstimmt den attach-Vorschlag -> EIGENER Claim, claimId bewusst ignoriert.
                    resulting = BuildGapClaim(i, units, promoted: true);
                    consumable.Add(resulting);
                    break;
                case AdjudicationActions.AttachEvidence:
                    // A9: in Phase 2 anwenden. Ziel = referenceTarget ?? claimId.
                    var target = !string.IsNullOrWhiteSpace(i.ReferenceTarget) ? i.ReferenceTarget! : i.ClaimId;
                    if (!string.IsNullOrWhiteSpace(target))
                        attachOps.Add((records.Count, i, target!));
                    break;
                case AdjudicationActions.MergeExisting:
                case AdjudicationActions.MarkCoveredBy:
                case AdjudicationActions.Reject:
                case AdjudicationActions.Defer:
                    // fügen KEINEN Claim hinzu (merge/covered = Referenz, reject = raus, defer = pending).
                    break;
            }

            records.Add(new AdjudicationRecord(i.ItemId, i.ItemType, action, resulting, i.ReferenceTarget,
                string.IsNullOrWhiteSpace(i.ActionReason) ? i.Reason : i.ActionReason!));
        }

        // Phase 2: attach_evidence -> Ziel-Claim real anreichern (Evidenz + sourceUnitId + Note).
        foreach (var (recordIndex, item, targetId) in attachOps)
        {
            var idx = IndexOfClaim(targetId);
            if (idx < 0) continue; // Ziel nicht in der Projektion (z.B. reject/defer) -> nur Audit-Referenz bleibt.
            var enriched = AttachEvidenceTo(consumable[idx], item, units);
            consumable[idx] = enriched;
            records[recordIndex] = records[recordIndex] with { ResultingClaim = enriched, ReferenceTarget = targetId };
        }

        var pending = filled.Items.Count(i => string.Equals(i.Action, AdjudicationActions.Defer, StringComparison.OrdinalIgnoreCase));
        var now = DateTime.UtcNow.ToString("o");
        var audit = new AdjudicatedLedger(filled.SourceValidatedRunId, filled.SourceUnitRunId, "author", now, approved, records, pending);
        var cons = new ConsumableLedger(now, consumable, pending,
            "Finale Claim-Menge = approved + accept_gap/promote_to_claim/apply_repair - reject; attach_evidence reichert Ziel-Claims an; defer bleibt pending. Neue Claims: facetStatus=pending -> ledger-adjudicate-refine (A10).");
        return (audit, cons);
    }

    /// <summary>A9: hängt das echte Unit-Zitat als zusätzliche Evidenz + sourceUnitId + Note an einen bestehenden Claim.</summary>
    private static SemanticLedgerEntry AttachEvidenceTo(SemanticLedgerEntry target, AdjudicationItem i, IReadOnlyDictionary<string, AtomicUnit>? units)
    {
        var quote = ResolveUnitQuote(i, units);
        var evidence = target.Evidence.ToList();
        if (quote is not null && !evidence.Any(e => string.Equals(e.Quote, quote.Quote, StringComparison.Ordinal)))
            evidence.Add(quote);

        var sourceUnitIds = (target.SourceUnitIds ?? []).ToList();
        if (i.UnitId is { } uid && !sourceUnitIds.Contains(uid, StringComparer.Ordinal))
            sourceUnitIds.Add(uid);

        var note = $"attach_evidence: Unit {i.UnitId ?? i.ItemId} vom Autor als Evidenz hinzugefügt"
                   + (string.IsNullOrWhiteSpace(i.ActionReason) ? "." : $" ({i.ActionReason}).");
        var notes = string.IsNullOrWhiteSpace(target.Notes) ? note : $"{target.Notes}\n{note}";

        return target with { Evidence = evidence, SourceUnitIds = sourceUnitIds, Notes = notes };
    }

    /// <summary>Echte Transkript-Evidenz einer Unit aus step-00; Fallback: die (paraphrasierte) Item-Proposition.</summary>
    private static SemanticLedgerEvidence? ResolveUnitQuote(AdjudicationItem i, IReadOnlyDictionary<string, AtomicUnit>? units)
    {
        if (i.UnitId is { } uid && units is not null && units.TryGetValue(uid, out var u))
            return new SemanticLedgerEvidence("transcript", u.Text);
        return string.IsNullOrWhiteSpace(i.Proposition) ? null : new SemanticLedgerEvidence("adjudication", i.Proposition);
    }

    private static IReadOnlyList<AdjudicationSuggestion> EffectiveRepairSuggestions(AdjudicationItem i)
    {
        if (i.SystemSuggestions is { Count: > 0 })
            return i.SystemSuggestions.Where(s => string.Equals(s.Kind, "facet_repair", StringComparison.OrdinalIgnoreCase)).ToList();
        return i.SystemSuggestion is not null && string.Equals(i.SystemSuggestion.Kind, "facet_repair", StringComparison.OrdinalIgnoreCase)
            ? [i.SystemSuggestion]
            : [];
    }

    private static IReadOnlyList<AdjudicationSuggestion> EffectiveAppliedRepairSuggestions(AdjudicationItem i)
        => EffectiveRepairSuggestions(i).Select(s => s with { Suggested = SelectedRepairValue(i, s) }).ToList();

    private static string? SelectedRepairValue(AdjudicationItem item, AdjudicationSuggestion suggestion)
    {
        var selected = NormalizeFacet(suggestion.Facet) switch
        {
            "status" => item.RepairStatus,
            "modality" => item.RepairModality,
            "timescope" => item.RepairTimeScope,
            "scope" => item.RepairScope,
            _ => null
        };
        return string.IsNullOrWhiteSpace(selected) ? suggestion.Suggested : selected;
    }

    private static string NormalizeFacet(string? facet)
        => string.Equals(facet, "timeScope", StringComparison.OrdinalIgnoreCase)
            ? "timescope"
            : (facet ?? "").Trim().ToLowerInvariant();

    private static SemanticLedgerEntry ApplyFacetRepairs(
        SemanticLedgerEntry baseC,
        IReadOnlyList<AdjudicationSuggestion> suggestions)
    {
        var repaired = baseC;
        foreach (var s in suggestions)
            repaired = ApplyFacetRepair(repaired, s);
        return repaired;
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

    /// <summary>Mintet einen NEUEN Claim aus einem Miss-/Unit-Item. Facetten sind bewusst generische Platzhalter
    /// (facetStatus=pending) -> A10-Refine hebt sie auf Pipeline-Niveau. Evidenz ist – wenn die Unit auflösbar ist –
    /// das ECHTE Transkript-Zitat (+ sourceUnitIds), sonst Fallback auf die Claim-Ref-/Proposition-Evidenz.</summary>
    private static SemanticLedgerEntry BuildGapClaim(AdjudicationItem i, IReadOnlyDictionary<string, AtomicUnit>? units, bool promoted)
    {
        var disp = new Dictionary<string, ArtifactDisposition>
        {
            ["requirements"] = new("context", "assumption"),
            ["architecture"] = new("context", "assumption"),
            ["risks"] = new("context", "assumption"),
            ["open-questions"] = new("required", "question"),
        };

        List<SemanticLedgerEvidence> evidence;
        List<string>? sourceUnitIds = null;
        if (i.UnitId is { } uid && units is not null && units.TryGetValue(uid, out var u))
        {
            evidence = [new SemanticLedgerEvidence("transcript", u.Text)]; // echte Erdung
            sourceUnitIds = [uid];
        }
        else
        {
            evidence = i.EvidenceRefs.Select(r => new SemanticLedgerEvidence("adjudication", r)).ToList();
        }

        var origin = promoted ? "promote_to_claim (override attach)" : "accept_gap";
        return new SemanticLedgerEntry(
            Id: $"ADJ-GAP-{(i.UnitId ?? i.ItemId).Replace("::", "-")}",
            Proposition: i.Proposition,
            Kind: "open_requirement",
            Status: "open",
            Modality: "must_note",
            Scope: "author-adjudicated gap",
            TimeScope: "mvp_or_later_unclear",
            Evidence: evidence,
            Disposition: disp,
            RiskLevel: "medium",
            Notes: $"author-adjudicated gap ({origin}); Facetten pending -> ledger-adjudicate-refine (A10).",
            SourceUnitIds: sourceUnitIds,
            FacetStatus: "pending");
    }
}
