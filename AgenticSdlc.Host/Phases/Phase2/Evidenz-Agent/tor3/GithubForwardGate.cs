using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// T3.3 — deterministisches Gate ueber den Forward-Plan (plan-tor3 §4). Kernregeln:
//   - Rev-3-INVARIANTE: CREATE_ISSUE ohne searchedQueries[] + searchEvidence -> CREATE_WITHOUT_SEARCH_EVIDENCE.
//   - Coverage: jedes Delta-PBI hat genau EIN Op (kein PBI faellt durch, keine Doppel-Ops).
//   - Ziel-Ops (UPDATE/COMMENT/LINK/FLAG_DRIFT) brauchen ein existierendes Issue; LINK-Ziel muss im Snapshot sein.
//   - Kein CREATE fuer ein blockiertes PBI; Belegpflicht (Anchor) fuer LINK/UPDATE/COMMENT.
public static class GithubForwardGate
{
    public static GithubForwardGateReport Check(
        GithubForwardPlanDocument plan,
        IReadOnlyList<GithubSyncEntry> deltaEntries,
        IReadOnlyList<GithubIssueSnapshot> issues)
    {
        var errors = new List<GithubForwardGateIssue>();
        var warnings = new List<GithubForwardGateIssue>();

        var issueNumbers = issues.Select(i => i.IssueNumber).ToHashSet();
        var blockedPbis = deltaEntries
            .Where(e => e.BlockedByOpenDecision || string.Equals(e.Status, "blocked_by_decision", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.PbiId).ToHashSet(StringComparer.Ordinal);

        foreach (var op in plan.Operations)
        {
            if (!GithubForwardKind.All.Contains(op.Kind))
            {
                errors.Add(Issue("UNKNOWN_KIND", "error", $"Unbekannte Operation '{op.Kind}'.", op.PbiId));
                continue;
            }

            // Rev-3-Invariante — die Anti-Duplikat-Regel ist hart erzwungen, nicht nur im Prompt.
            if (string.Equals(op.Kind, GithubForwardKind.CreateIssue, StringComparison.Ordinal))
            {
                if (op.SearchedQueries is null || op.SearchedQueries.Count == 0 || string.IsNullOrWhiteSpace(op.SearchEvidence))
                    errors.Add(Issue("CREATE_WITHOUT_SEARCH_EVIDENCE", "error",
                        "CREATE_ISSUE ohne ausgefuehrte Suche (searchedQueries + searchEvidence) — Duplikat-Risiko.", op.PbiId));
                if (string.IsNullOrWhiteSpace(op.Title) || string.IsNullOrWhiteSpace(op.Body))
                    errors.Add(Issue("CREATE_INCOMPLETE", "error", "CREATE_ISSUE braucht Titel und Body.", op.PbiId));
                if (blockedPbis.Contains(op.PbiId))
                    errors.Add(Issue("CREATE_FOR_BLOCKED", "error", "Blockiertes PBI darf kein Issue anlegen (HOLD_BLOCKED).", op.PbiId));
            }

            if (GithubForwardKind.RequireIssueTarget.Contains(op.Kind))
            {
                if (op.TargetIssueNumber is null)
                    errors.Add(Issue("TARGET_REQUIRED", "error", $"'{op.Kind}' braucht targetIssueNumber.", op.PbiId));
                else if (string.Equals(op.Kind, GithubForwardKind.Link, StringComparison.Ordinal) && !issueNumbers.Contains(op.TargetIssueNumber.Value))
                    errors.Add(Issue("LINK_TARGET_UNKNOWN", "error", $"LINK-Ziel #{op.TargetIssueNumber} ist nicht im Snapshot.", op.PbiId));
            }

            // Belegpflicht: jede Zuordnung nennt ihren Anker.
            if ((string.Equals(op.Kind, GithubForwardKind.Link, StringComparison.Ordinal)
                 || string.Equals(op.Kind, GithubForwardKind.UpdateIssue, StringComparison.Ordinal)
                 || string.Equals(op.Kind, GithubForwardKind.Comment, StringComparison.Ordinal))
                && string.IsNullOrWhiteSpace(op.Anchor))
                warnings.Add(Issue("MISSING_ANCHOR", "warning", $"'{op.Kind}' ohne Anker-Beleg.", op.PbiId));
        }

        // Coverage: jedes Delta-PBI genau ein Op.
        var opsByPbi = plan.Operations.GroupBy(o => o.PbiId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        foreach (var e in deltaEntries)
        {
            if (!opsByPbi.TryGetValue(e.PbiId, out var n))
                errors.Add(Issue("PBI_NOT_ADDRESSED", "error", $"Delta-PBI '{e.PbiId}' hat kein Op.", e.PbiId));
            else if (n > 1)
                errors.Add(Issue("DUPLICATE_OP", "error", $"PBI '{e.PbiId}' hat {n} Ops (genau eins erlaubt).", e.PbiId));
        }

        var pass = errors.Count == 0;
        return new GithubForwardGateReport(pass, pass ? "accept" : "block", errors, warnings);
    }

    private static GithubForwardGateIssue Issue(string code, string sev, string msg, string? pbi) => new(code, sev, msg, pbi);
}
