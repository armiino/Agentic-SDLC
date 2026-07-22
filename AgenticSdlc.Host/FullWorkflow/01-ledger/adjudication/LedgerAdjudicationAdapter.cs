using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Bringt die zwei strukturell unterschiedlichen Signalquellen (Normal-Mode `recall-fast`-Misses,
/// Unit-Mode `unused-unit-ledger-compare`) + die `review_required`-Claims auf eine gemeinsame
/// <see cref="AdjudicationItem"/>-Queue (Bauvorschlag §1.1). Reine Transformationslogik, KEIN LLM.
/// </summary>
public static class LedgerAdjudicationAdapter
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    // Unit-Compare-Verdicts, die eine Autor-Entscheidung brauchen. already_covered_indirectly wird bewusst
    // NICHT als Item aufgenommen (bereits aufgelöst; kein Handlungsbedarf).
    private static readonly HashSet<string> DecidableCompareVerdicts =
        new(["missing_claim", "needs_human", "attach_as_evidence"], StringComparer.OrdinalIgnoreCase);

    public static AdjudicationQueue Build(
        ValidatedLedger validated,
        string? validatedRunId,
        string? missSignalJson,     // roher Inhalt einer unused-unit-compare.json ODER recall-fast.json (oder null)
        string? unitRunId)
    {
        var items = new List<AdjudicationItem>();

        // 1) review_required-Claims (beide Modi) -> systemSuggestions = alle FacetIssues mit Repair-Vorschlag.
        // systemSuggestion bleibt als Legacy-/Kurzfeld der erste Vorschlag.
        foreach (var v in validated.Entries.Where(e => e.ClaimStatus == "review_required"))
        {
            var issues = BuildRepairSuggestions(v);
            var issue = issues.FirstOrDefault();
            AdjudicationSuggestion? sug = issue is null ? null
                : new AdjudicationSuggestion("facet_repair", issue.Facet, issue.Observed, issue.Suggested, null);

            items.Add(new AdjudicationItem(
                ItemId: $"RR::{v.Entry.Id}",
                ItemType: "review_required_claim",
                SourceMode: "normal",
                ClaimId: v.Entry.Id,
                UnitId: null,
                Proposition: v.Entry.Proposition,
                EvidenceRefs: v.Entry.Evidence.Select(ev => ev.Quote).ToList(),
                SystemSuggestion: sug,
                Reason: $"verdict={v.Validation.Verdict}; {v.Validation.Reason}",
                SystemSuggestions: issues.Count > 0 ? issues : null));
        }

        // 2) Miss-Signale (optional). Format-Erkennung: unit-compare hat items[].verdict, recall-fast hat missedEntries[].
        if (!string.IsNullOrWhiteSpace(missSignalJson))
        {
            using var doc = JsonDocument.Parse(missSignalJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("items", out var arr) && arr.ValueKind == JsonValueKind.Array
                && arr.EnumerateArray().Any(e => e.TryGetProperty("verdict", out _)))
            {
                // Unit-Mode compare
                foreach (var it in arr.EnumerateArray())
                {
                    var verdict = it.GetProperty("verdict").GetString() ?? "";
                    if (!DecidableCompareVerdicts.Contains(verdict)) continue;
                    var unitId = it.TryGetProperty("unitId", out var u) ? u.GetString() : null;
                    var reason = it.TryGetProperty("reason", out var r) ? r.GetString() ?? "" : "";
                    var prop = it.TryGetProperty("suggestedProposition", out var sp) && sp.ValueKind == JsonValueKind.String
                        ? sp.GetString()! : reason;
                    var related = it.TryGetProperty("relatedCandidateIds", out var rc) && rc.ValueKind == JsonValueKind.Array
                        ? rc.EnumerateArray().Select(x => x.GetString() ?? "").Where(s => s.Length > 0).ToList() : [];

                    items.Add(new AdjudicationItem(
                        ItemId: $"US::{unitId}",
                        ItemType: "unit_signal",
                        SourceMode: "unit",
                        ClaimId: related.Count > 0 ? related[0] : null,
                        UnitId: unitId,
                        Proposition: prop,
                        EvidenceRefs: related,
                        SystemSuggestion: new AdjudicationSuggestion("compare_classification", null, null, null, verdict),
                        Reason: reason));
                }
            }
            else if (root.TryGetProperty("missedEntries", out var misses) && misses.ValueKind == JsonValueKind.Array)
            {
                // Normal-Mode recall-fast
                foreach (var m in misses.EnumerateArray())
                {
                    var id = m.TryGetProperty("id", out var i) ? i.GetString() : null;
                    var prop = m.TryGetProperty("proposition", out var p) ? p.GetString() ?? "" : "";
                    items.Add(new AdjudicationItem(
                        ItemId: $"CM::{id}",
                        ItemType: "coverage_miss",
                        SourceMode: "normal",
                        ClaimId: null,
                        UnitId: null,
                        Proposition: prop,
                        EvidenceRefs: [],
                        SystemSuggestion: null,
                        Reason: $"recall-fast miss (Referenz-Claim {id} nicht segment-covered)"));
                }
            }
        }

        return new AdjudicationQueue(validatedRunId, unitRunId, DateTime.UtcNow.ToString("o"), items);
    }

    public static AdjudicationQueue EnrichRepairSuggestions(AdjudicationQueue queue, ValidatedLedger validated)
    {
        var byClaimId = validated.Entries.ToDictionary(e => e.Entry.Id, StringComparer.Ordinal);
        var items = queue.Items.Select(item =>
        {
            if (item.ItemType != "review_required_claim"
                || item.ClaimId is not { Length: > 0 } claimId
                || item.SystemSuggestions is { Count: > 0 }
                || !byClaimId.TryGetValue(claimId, out var validatedEntry))
                return item;

            var suggestions = BuildRepairSuggestions(validatedEntry);
            if (suggestions.Count == 0) return item;
            return item with
            {
                SystemSuggestion = item.SystemSuggestion ?? suggestions[0],
                SystemSuggestions = suggestions
            };
        }).ToList();

        return queue with { Items = items };
    }

    public static ValidatedLedger LoadValidated(string path)
        => JsonSerializer.Deserialize<ValidatedLedger>(File.ReadAllText(path), Json) ?? new ValidatedLedger([]);

    private static List<AdjudicationSuggestion> BuildRepairSuggestions(ValidatedLedgerEntry entry)
    {
        var issues = entry.Validation.FacetIssues
            .Where(i => i.Suggested is not null)
            .DefaultIfEmpty(entry.Validation.FacetIssues.FirstOrDefault())
            .Where(i => i is not null)
            .Select(i => new AdjudicationSuggestion("facet_repair", i!.Facet, i.Observed, i.Suggested, null))
            .ToList();
        return issues;
    }
}
