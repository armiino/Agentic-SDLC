using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// R-35 Teil 2 (Wiedervorlage), Schreib-Seite: menschliche SKIPS am Tor-1-Gate (mit P2a-Begruendung) werden als
// Proposals in den Core gehoben — Ablehnung = PROJEKT-WISSEN ("das wollen wir bewusst nicht"), KEIN Wahrheits-Item
// (die Proposals-Schiene traegt heute schon rejected-L3-Kandidaten; gleiches Muster). Nur Tor 1: die anderen Gates
// entscheiden Timing/Struktur, keine Wahrheits-Aussagen (Leitregel, aufgefallen §2② R-35).
// Deterministisch, idempotent je (planId, incomingItemId) — ein zweiter Apply desselben Plans dupliziert nichts.
public static class IngestionRejections
{
    public const string ProposalType = "ingest_rejection";

    public static (ProjectStateDocument Core, IReadOnlyList<string> Recorded) Record(
        ProjectStateDocument core, StateChangePlanDocument plan, IReadOnlyList<IngestionHumanDecision> decisions, string planPath)
    {
        var opsByIncoming = plan.Operations
            .GroupBy(o => o.IncomingItemId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var already = core.Proposals
            .Where(p => string.Equals(p.ProposalType, ProposalType, StringComparison.Ordinal))
            .Select(p => (p.Metadata.GetValueOrDefault("planId") ?? "", p.Metadata.GetValueOrDefault("incomingItemId") ?? ""))
            .ToHashSet();

        var next = MaxSuffix(core.Proposals.Select(p => p.ProposalId)) + 1;
        var proposals = core.Proposals.ToList();
        var recorded = new List<string>();

        // 1d/R-57 (18.08.): UI sagt "skip", Chat sagt "reject" — beides ist eine begruendete NICHT-Uebernahme
        // (P2a). Der Filter ist deshalb "alles ausser apply", nicht ein Vokabel-Exakt-Match.
        foreach (var d in decisions.Where(d => !string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase)))
        {
            if (!opsByIncoming.TryGetValue(d.IncomingItemId, out var op)) continue;
            if (already.Contains((plan.PlanId, d.IncomingItemId))) continue;

            var id = $"REJ-{next++:D3}";
            var meta = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["statement"] = op.Statement,
                ["reason"] = d.Reason ?? "",
                ["kind"] = op.Kind,
                // identityKey = deterministischer Exakt-Match-Anker fuer die Wiedervorlage (Adapter, LLM-frei).
                ["identityKey"] = IdentityKey.From(op.Statement),
                ["planId"] = plan.PlanId,
                ["incomingItemId"] = d.IncomingItemId,
                ["date"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            };
            if (!string.IsNullOrWhiteSpace(op.TargetEntityId)) meta["targetEntityId"] = op.TargetEntityId!;
            if (!string.IsNullOrWhiteSpace(op.FeatureKey)) meta["featureKey"] = op.FeatureKey!;

            proposals.Add(new ProjectStateProposal(id, ProposalType, "rejected", SourceRunId: plan.PlanId, PayloadPath: planPath, Metadata: meta));
            recorded.Add(id);
        }

        return recorded.Count == 0 ? (core, recorded) : (core with { Proposals = proposals }, recorded);
    }

    /// <summary>
    /// R-58 (18.08., Abnahme-3.0-Fund): DIE EINE Rejections-Suche — Stichwort-Recall statt Phrasen-Match.
    /// Der Steward suchte „Besuchs-Erinnerung Angehoerige bestaetigt …" als GANZE Phrase → 0 Treffer,
    /// obwohl REJ-007/008 exakt das Thema trugen; parallel hatte das Tor-1-Tool eine EIGENE (bessere)
    /// Token-Suche ohne Umlaut-Faltung — zwei Implementierungen, beide unvollstaendig. Jetzt: EIN Kern —
    /// Token-ODER (QueryText.Tokens: Bindestrich-Split + Faltung), Ranking nach Treffer-Zahl. Recall-first
    /// ist hier RICHTIG (kleiner Warn-Bestand, Zweck = Wiedervorlage-Warnung); die grosse Wahrheits-Suche
    /// bleibt bewusst UND-strikt (R-41).
    /// </summary>
    public static IReadOnlyList<(ProjectStateProposal Proposal, int Score)> Search(
        ProjectStateDocument core, string? query, int limit = 10)
    {
        var tokens = FullWorkflow.QueryText.Tokens(query);
        var all = Of(core);
        if (tokens.Count == 0)
            return all.OrderBy(p => p.ProposalId, StringComparer.Ordinal).Take(limit).Select(p => (p, 0)).ToList();

        return all
            .Select(p =>
            {
                var hay = FullWorkflow.QueryText.Fold(
                    $"{p.ProposalId} {p.Metadata.GetValueOrDefault("statement")} {p.Metadata.GetValueOrDefault("reason")} {p.Metadata.GetValueOrDefault("identityKey")}")
                    .ToLowerInvariant();
                return (Proposal: p, Score: tokens.Count(t => hay.Contains(t, StringComparison.Ordinal)));
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score).ThenBy(x => x.Proposal.ProposalId, StringComparer.Ordinal)
            .Take(limit).ToList();
    }

    public static IReadOnlyList<ProjectStateProposal> Of(ProjectStateDocument core)
        => core.Proposals.Where(p => string.Equals(p.ProposalType, ProposalType, StringComparison.Ordinal)).ToList();

    private static int MaxSuffix(IEnumerable<string> ids)
        => ids.Where(id => id.StartsWith("REJ-", StringComparison.Ordinal))
            .Select(id => id["REJ-".Length..])
            .Where(s => s.Length > 0 && s.All(char.IsDigit))
            .Select(int.Parse).DefaultIfEmpty(0).Max();
}
