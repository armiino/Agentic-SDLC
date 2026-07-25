using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.3 — deterministischer Vorfilter (plan-tor3 §4). NUR die klaren Faelle, kein Urteil:
//   - blocked_by_decision  -> HOLD_BLOCKED (NIE ein CREATE; wartet auf Tor 2), bestehendes Issue nur kommentieren.
//   - needs_clarify + unmapped + holdUnclearNewPbis -> HOLD_CLARIFY (R-26: unbeaufsichtigt kein Auto-Issue, geparkt).
//   - Mapping vorhanden + Issue offen        -> UPDATE_ISSUE (vorgeschlagener Patch, KEIN Auto-Overwrite, Rev 2).
//   - Mapping vorhanden + Issue geschlossen, PBI aktiv/needs_clarify -> FLAG_DRIFT.
//   - Mapping vorhanden, Issue nicht im Snapshot                     -> FLAG_DRIFT (Mapping veraltet).
// Alles OHNE Mapping und NICHT blockiert bleibt der agentischen Stufe (Suche + LINK/CREATE, konservativ).
public static class GithubForwardSeed
{
    private static readonly HashSet<string> Activeish = new(StringComparer.OrdinalIgnoreCase) { "active", "needs_clarify" };

    public static GithubForwardSeedResult Seed(
        IReadOnlyList<GithubSyncEntry> deltaEntries,
        IReadOnlyDictionary<string, GithubMappingRecord> mappingByPbi,
        IReadOnlyList<GithubIssueSnapshot> issues,
        bool holdUnclearNewPbis = false)
    {
        var issueByNumber = issues.GroupBy(i => i.IssueNumber).ToDictionary(g => g.Key, g => g.Last());
        var deterministic = new List<GithubForwardOp>();
        var unmapped = new List<GithubSyncEntry>();

        foreach (var e in deltaEntries)
        {
            var blocked = e.BlockedByOpenDecision
                || string.Equals(e.Status, "blocked_by_decision", StringComparison.OrdinalIgnoreCase);
            var mapping = mappingByPbi.GetValueOrDefault(e.PbiId);

            if (blocked)
            {
                deterministic.Add(new GithubForwardOp(
                    GithubForwardKind.HoldBlocked, e.PbiId, mapping?.IssueNumber, null, null, null, null, null,
                    Anchor: mapping is null ? $"pbi {e.PbiId}" : $"pbi {e.PbiId} -> {CoreGithubMapping.IssueRef(mapping.IssueNumber)}",
                    Rationale: "PBI ist durch eine offene Decision blockiert — kein Issue anlegen (wartet auf Tor 2 Unblock).",
                    Origin: "deterministic"));
                continue;
            }

            if (mapping is null)
            {
                // R-26: neues, noch unklares PBI ohne Mapping + niemand da, der ein Issue autorisiert (accept-all) →
                // PARKEN statt agentisch CREATE erzwingen. Deterministisch, sichtbar, keine Sackgasse. Gemappte
                // needs_clarify bleiben unberuehrt (UPDATE, s. u.). Aufloesen = Folgeschritt (Option C / Parkplatz-Core).
                if (holdUnclearNewPbis && string.Equals(e.Status, "needs_clarify", StringComparison.OrdinalIgnoreCase))
                {
                    deterministic.Add(new GithubForwardOp(
                        GithubForwardKind.HoldClarify, e.PbiId, null, null, null, null, null, null,
                        Anchor: $"pbi {e.PbiId}",
                        Rationale: "PBI ist neu und noch unklar (needs_clarify) — im unbeaufsichtigten Lauf kein Auto-Issue; geparkt bis zur Klaerung (needs_clarify->active).",
                        Origin: "deterministic"));
                    continue;
                }
                unmapped.Add(e);
                continue;
            }

            var anchor = $"pbi {e.PbiId} -> {CoreGithubMapping.IssueRef(mapping.IssueNumber)}";
            if (!issueByNumber.TryGetValue(mapping.IssueNumber, out var issue))
            {
                deterministic.Add(new GithubForwardOp(
                    GithubForwardKind.FlagDrift, e.PbiId, mapping.IssueNumber, null, null, null, null, null, anchor,
                    "Gemapptes Issue nicht im aktuellen Snapshot — Mapping evtl. veraltet, manuell pruefen.", "deterministic"));
            }
            else if (issue.State.Equals("closed", StringComparison.OrdinalIgnoreCase) && Activeish.Contains(e.Status))
            {
                deterministic.Add(new GithubForwardOp(
                    GithubForwardKind.FlagDrift, e.PbiId, mapping.IssueNumber, null, null, null, null, null, anchor,
                    $"Issue #{mapping.IssueNumber} ist geschlossen, PBI aber '{e.Status}' — Drift, manuell pruefen.", "deterministic"));
            }
            else
            {
                deterministic.Add(new GithubForwardOp(
                    GithubForwardKind.UpdateIssue, e.PbiId, mapping.IssueNumber, e.Title, ProposedBody(e), e.CoveredRequirementIds.ToList(),
                    null, null, anchor,
                    "PBI hat sich geaendert — vorgeschlagener Patch/Kommentar (kein Auto-Overwrite).", "deterministic"));
            }
        }

        return new GithubForwardSeedResult(deterministic, unmapped);
    }

    // Vorgeschlagener Issue-Body aus dem Core-Zustand (Beleg, kein freier Text).
    private static string ProposedBody(GithubSyncEntry e)
    {
        var reqs = e.CoveredRequirementIds.Count == 0 ? "-" : string.Join(", ", e.CoveredRequirementIds);
        var readiness = string.IsNullOrWhiteSpace(e.Readiness) ? "-" : e.Readiness;
        return $"PBI {e.PbiId} — Status: {e.Status}, Readiness: {readiness}\n\nAbgedeckte Requirements: {reqs}";
    }
}

public sealed record GithubForwardSeedResult(
    IReadOnlyList<GithubForwardOp> DeterministicOps,
    IReadOnlyList<GithubSyncEntry> UnmappedPbis);
