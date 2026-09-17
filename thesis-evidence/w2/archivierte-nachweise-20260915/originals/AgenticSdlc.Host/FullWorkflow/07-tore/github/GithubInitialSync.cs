using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// R-15 / Bootstrap-Plan B5: deterministischer initial-sync — der Erst-Sync eines frischen Repos braucht ein
// Voll-CREATE-Delta (ALLE unmapped PBIs), nicht das inkrementelle update-Delta aus pbi-update. Bisher wurde das
// einmalig als handgelegtes Run-Artefakt simuliert (runsArchive/pbi-update/initial-sync-20260720); dieser Kern
// erzeugt exakt dasselbe Format (GithubSyncDeltaDocument) aus der github-sync-Core-View. Kein LLM, keine
// Interpretation — die Entscheidung CREATE vs. LINK trifft weiterhin der Forward-Agent + Gate (Dry-Run-Default).
public static class GithubInitialSync
{
    /// <summary>Alle unmapped, nicht archivierten PBIs des Cores als Voll-CREATE-Delta (newPbis = alle).</summary>
    public static GithubSyncDeltaDocument BuildDelta(ProjectStateDocument core)
    {
        var unmapped = CoreViews.GithubSync(core).Entries
            .Where(e => string.IsNullOrWhiteSpace(e.GithubIssue))
            .ToList();
        return new GithubSyncDeltaDocument(
            NewPbis: unmapped.Select(e => e.PbiId).ToList(),
            UpdatedPbis: [],
            Entries: unmapped);
    }

    /// <summary>
    /// R-27: deterministischer Erst-Sync-PLAN — je unmapped PBI (NACH dem Seed-Vorfilter: keine blocked/
    /// needs_clarify-Holds mehr dabei) ein gate-konformer CREATE (Title+Body aus dem Core-Zustand,
    /// ehrliche Such-Evidenz: leerer Snapshot ⇒ Duplikatsuche gegenstandslos). R-17-Präzedenz vom 23.07.
    /// </summary>
    // R-48: searchEvidence/rationale parametrisierbar — der Dry-Run-Konsument (GithubForwardAgentRunner) nutzt
    // DIESELBE Naht mit ehrlichen Dry-Run-Texten (Snapshot ist dort NICHT leer; Agent-Dedup laeuft erst im Real-Lauf).
    public static List<GithubForwardOp> BuildCreateOps(IReadOnlyList<GithubSyncEntry> unmapped,
        string? searchEvidence = null, string? rationale = null)
        => unmapped.Select(e => new GithubForwardOp(
            GithubForwardKind.CreateIssue, e.PbiId, null,
            Title: e.Title,
            Body: InitialSyncBody(e),
            Labels: GithubIssueLabels.For(e),   // ④: Familie statt Schöpfungs-Vermerk (initial-sync = Altlast)
            SearchedQueries: [e.Title],
            SearchEvidence: searchEvidence ?? "Initial-Sync (deterministisch): frisches Repo / leerer Issue-Snapshot — keine plausiblen Treffer möglich, Duplikatsuche gegenstandslos.",
            Anchor: e.CoveredRequirementIds.Count == 0 ? $"pbi {e.PbiId}" : $"pbi {e.PbiId} <- {string.Join(", ", e.CoveredRequirementIds)}",
            Rationale: rationale ?? "Erst-Sync eines frischen Repos: unmapped PBI ohne Issue — CREATE deterministisch aus dem Core-Payload (R-15/R-17).",
            Origin: "deterministic")).ToList();

    // Issue-Body aus dem Core-Zustand (Beleg, kein freier Text) — geteilte Struktur-Naht GithubIssueTemplate.
    // ⑥ Klartext ohne interne Codes (Autor-Fund 20.08., #45).
    private static string InitialSyncBody(GithubSyncEntry e)
        => GithubIssueTemplate.Render(e, "Automatisch angelegt aus dem Projekt-Backlog");
}
