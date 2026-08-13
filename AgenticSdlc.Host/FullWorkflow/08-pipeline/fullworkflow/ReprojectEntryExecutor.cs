using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// ONLY-STEWARD Re-Projektion (steward/reprojektion-only-steward.md, 11.08.) — der Recovery-Eingang des Ein-Graphen.
// PROBLEM: scheitert ein Forward am externen Rand (GitHub-Write, z. B. 403), ist die Wahrheit (Core) korrekt, aber die
// Projektion (GitHub) hängt hinterher. Der Lauf ist fertig → kein resume. WARUM DIESER EINGANG: der Steward soll den
// Loop schließen können, ohne auf die Werkbank-CLI zu fallen — durch den DURABLEN Graphen (Thesis-Achse: Steward
// orchestriert den Graphen, auch bei Recovery).
//
// TRUTH-FIRST: das Sync-Delta kommt AUS DEM CORE (CoreViews.GithubSync — die Projektions-View der Wahrheit), NICHT aus
// einem Run-Ordner (keine Chronik-Kopplung). GEMAPPT-ONLY: nur PBIs mit bestätigtem implemented_by_issue → reiner
// 1:1-UPDATE-Reconcile gegen den frischen Snapshot (deterministisch, KEIN Agent, kein L4-all-vs-all — das teure
// Matching lebt nur im unmapped-Pfad, der hier bewusst außen bleibt). Neuer-PBI-CREATE bleibt der Erst-Sync-/Betriebs-
// Bahn vorbehalten. Der Forward-Maker markiert in-sync-PBIs als NoChange → idempotent (mehrfach fahrbar).
//
// Der Eingang speist den VORHANDENEN Forward-Schwanz: er emittiert dieselbe ForwardPrep wie die beiden Bridges →
// Snapshot → Maker → github-forward-gate (HUMAN, durabel, chat-gegatet) → Apply. Kein neuer Downstream-Code.
[SendsMessage(typeof(ForwardPrep))]
[YieldsOutput(typeof(string))]
internal sealed class ReprojectEntryExecutor(RunContext run, string repoRoot) : Executor<ReprojectRequest>("ReprojectEntry")
{
    public override async ValueTask HandleAsync(ReprojectRequest _, IWorkflowContext context, CancellationToken ct = default)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);

        // truth-first + gemappt-only: das Delta ist eine reine Funktion des Cores (Projektions-View), gefiltert auf
        // PBIs MIT Issue-Mapping. Kein runs/-Zugriff (Wächter im Test: ReprojectEntryTests).
        var mapped = CoreViews.GithubSync(core).Entries
            .Where(e => !string.IsNullOrWhiteSpace(e.GithubIssue))
            .ToList();

        if (mapped.Count == 0)
        {
            // Nichts gemappt = nichts zu re-projizieren → SICHTBARER terminaler Stop (Muster ClarifyEntry), KEIN stiller Skip.
            run.AppendEvent(new { type = "REPROJECT_NO_MAPPED", runId = run.RunId, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync("reproject: keine gemappten PBIs im Core — nichts zu re-projizieren.").ConfigureAwait(false);
            return;
        }

        var delta = new GithubSyncDeltaDocument(NewPbis: [], UpdatedPbis: mapped.Select(e => e.PbiId).ToList(), Entries: mapped);
        run.AppendEvent(new { type = "REPROJECT_START", runId = run.RunId, mappedPbis = mapped.Count, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[pipeline-full] Re-Projektion (only-steward, truth-first, gemappt-only): {mapped.Count} PBIs -> Reconcile Core->GitHub "
            + "(Snapshot -> Maker[UPDATE/NoChange] -> forward-gate -> Apply).");
        await context.SendMessageAsync(new ForwardPrep(delta, InitialSync: false)).ConfigureAwait(false);
    }
}
