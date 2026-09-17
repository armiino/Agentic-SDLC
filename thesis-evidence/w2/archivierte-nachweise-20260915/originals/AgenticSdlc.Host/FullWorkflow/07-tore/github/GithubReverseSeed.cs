using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.5 — deterministischer Reverse-Detektor: vergleicht den GitHub-Zustand (Snapshot) mit dem Core ueber die
// implemented_by_issue-Mappings und schlaegt StateChanges VOR (kein Apply). Kein Agent: der Zustandsdiff ist
// eindeutig; die eigentliche Bedeutungs-Entscheidung ("ist das wirklich fertig?") ist die menschliche Verifikation.
public static class GithubReverseSeed
{
    // §5-S4: die Alt-String-Sets durch typisierte Achsen ersetzt (s. unten): DoneCandidate = aktiv & nicht-done &
    // nicht-blockiert; AlreadyClosedish = IsArchived (superseded ODER done). retired war tot.

    public static IReadOnlyList<GithubReverseOp> Seed(ProjectStateDocument core, IReadOnlyList<GithubIssueSnapshot> issues)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var issueByNumber = issues.GroupBy(i => i.IssueNumber).ToDictionary(g => g.Key, g => g.Last());
        var ops = new List<GithubReverseOp>();

        foreach (var m in CoreGithubMapping.CurrentMappings(core))
        {
            if (!byId.TryGetValue(m.PbiId, out var pbi) || !string.Equals(pbi.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)) continue;
            if (!issueByNumber.TryGetValue(m.IssueNumber, out var issue)) continue; // fehlendes Issue = Forward-Drift, nicht reverse.

            var closed = issue.State.Equals("closed", StringComparison.OrdinalIgnoreCase);
            var cs = pbi.ReadStatus();
            var status = pbi.Status;   // Alt-String nur für Anzeige/Op-Feld (bis S7 gepflegt)
            var canBeDone = cs.Validity == Validity.Active && cs.Progress != Progress.Done && cs.Blocker != Blocker.BlockedByDecision;

            if (closed && canBeDone)
            {
                ops.Add(new GithubReverseOp(GithubReverseKind.PbiDone, m.PbiId, m.IssueNumber, status, "done", RequiresVerification: true,
                    $"Issue #{m.IssueNumber} ist geschlossen, PBI '{status}'. Vorschlag: done — NUR nach deiner Verifikation, dass die Arbeit wirklich fertig ist (E4)."));
            }
            else if (closed && cs.IsArchived && string.Equals(m.OperationalStatus, "open", StringComparison.OrdinalIgnoreCase))
            {
                ops.Add(new GithubReverseOp(GithubReverseKind.MappingSyncClosed, m.PbiId, m.IssueNumber, status, null, RequiresVerification: false,
                    $"Issue #{m.IssueNumber} geschlossen, PBI bereits '{status}'. Nur Mapping-Status auf closed nachziehen (keine PBI-Aenderung)."));
            }
            else if (!closed && (string.Equals(m.OperationalStatus, "closed", StringComparison.OrdinalIgnoreCase) || cs.IsArchived))
            {
                ops.Add(new GithubReverseOp(GithubReverseKind.FlagReopened, m.PbiId, m.IssueNumber, status, null, RequiresVerification: false,
                    $"Issue #{m.IssueNumber} ist (wieder) offen, PBI/Mapping aber abgeschlossen. Drift — bitte pruefen (kein Auto-Change)."));
            }
        }

        return ops;
    }
}
