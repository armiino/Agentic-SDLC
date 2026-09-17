using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// W2-Kontrolltests T1-T4 (Messprotokoll §5 / Umsetzungsplan §5.2): die vier Governance-Faelle am ECHTEN
// persistierenden Apply-Pfad (IngestionApplyExec.ExecuteAsync mit realem JsonCoreRepository in einem
// Temp-Root — kein Mock, Core-Zustand vor/nach ist der Beleg). Unterschieden wird der FACHLICHE
// Item-Bestand vom zulaessigen Ablehnungs-GEDAECHTNIS (ingest_rejection-Proposals, R-35).
// T1 G05: Op ohne Freigabe -> keine Uebernahme. T2 G06: abgelehnte Op -> keine Uebernahme, Ablehnung
// nachvollziehbar. T3 Replay: derselbe Plan erneut -> kein zweites Apply, keine Duplikate.
// T4 Kontrollfall: freigegebene Op kommt korrekt an (Differenz, Freigabebezug, Historie, Marker).
public sealed class IngestionApplyGovernanceTests
{
    private const string IngestRun = "20260907_101010_govtst";

    private static ProjectStateItem Bestand(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "test", null, 1,
        "baseline-run", null, null, null, null, [], [], new Dictionary<string, string>())
        .WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateItem Incoming(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "MEETING", null, 1,
        "delta-run", null, null, null, null, ["claim-1"], [], new Dictionary<string, string>())
        .WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static StateChangeOperation NewOp(string incomingId, string statement)
        => new(incomingId, StateChangeKind.New, statement, null, null, [], "kontrolltest");

    // Realer Aufbau: Temp-Root mit echtem Core (Seed via SaveAsync), Plan-Ordner mit MeetingDelta-Datei
    // und optionaler Entscheid-Datei (UI-Vertrag human-decisions.json) — exakt die ExecuteAsync-Eingaenge.
    private static async Task<(string Root, string PlanDir, StateChangePlanDocument Plan)> ArrangeAsync(
        string planId, string? decisionsJson)
    {
        var root = Path.Combine(Path.GetTempPath(), $"gov-{Guid.NewGuid():N}");
        await new JsonCoreRepository(root).SaveAsync(Doc(Bestand("REQ-1", "bestehende Anforderung")));

        var planDir = Path.Combine(root, "runs", "ingestion", IngestRun, "plan");
        Directory.CreateDirectory(planDir);
        var deltaPath = Path.Combine(planDir, "meeting-delta.json");
        await new JsonProjectStateRepository(Doc(Incoming("IN-1", "neue Aussage aus dem Meeting"))).SaveAsync(deltaPath);

        var plan = new StateChangePlanDocument(StateChangePlanDocument.CurrentSchemaVersion, planId,
            DateTime.UnixEpoch, deltaPath, [NewOp("IN-1", "neue Aussage aus dem Meeting")]);
        if (decisionsJson is not null)
            await File.WriteAllTextAsync(Path.Combine(planDir, "human-decisions.json"), decisionsJson);
        return (root, planDir, plan);
    }

    private static int SnapshotCount(string root)
    {
        var history = Path.Combine(CorePaths.CoreDir(root), "history");
        return Directory.Exists(history) ? Directory.GetFiles(history).Length : 0;
    }

    [Fact] // T1 / G05 — fachlich gueltiger Plan OHNE Freigabe-Entscheid am echten Apply-Pfad
    public async Task G05_Op_ohne_Freigabe_wird_nicht_uebernommen_und_der_Core_bleibt_fachlich_unveraendert()
    {
        var (root, planDir, plan) = await ArrangeAsync("plan-g05", """
            { "runId":"r-g05", "reviewer":"human (review-ui)", "decisions":[] }
            """);
        try
        {
            var accepted = IngestionApplyExec.AcceptedFromDecisions([]);   // kein einziges "apply"
            var report = await IngestionApplyExec.ExecuteAsync(planDir, plan, accepted, root, IngestRun);

            Assert.Empty(report.Applied);                                   // definierte Zurueckstellung, kein Apply
            var core = await new JsonCoreRepository(root).LoadAsync();
            Assert.Equal("REQ-1", Assert.Single(core.Items).ItemId);        // fachlicher Bestand identisch
            Assert.Empty(core.Proposals);                                   // auch kein Neben-Schreiben
            Assert.Equal(0, SnapshotCount(root));                           // inhaltsgleicher Save = kein Snapshot
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact] // T2 / G06 — ausdruecklich abgelehnte Aenderung laeuft den regulaeren Pfad weiter
    public async Task G06_Abgelehnte_Aenderung_wird_nicht_uebernommen_und_die_Ablehnung_bleibt_nachvollziehbar()
    {
        var (root, planDir, plan) = await ArrangeAsync("plan-g06", """
            { "runId":"r-g06", "reviewer":"human (review-ui)",
              "decisions":[ { "incomingItemId":"IN-1", "decision":"reject", "reason":"Ausserhalb des Produktkerns" } ] }
            """);
        try
        {
            var accepted = IngestionApplyExec.AcceptedFromDecisions([new IngestionHumanDecision("IN-1", "reject", "Ausserhalb des Produktkerns")]);
            var report = await IngestionApplyExec.ExecuteAsync(planDir, plan, accepted, root, IngestRun);

            Assert.Empty(report.Applied);
            var core = await new JsonCoreRepository(root).LoadAsync();
            Assert.Equal("REQ-1", Assert.Single(core.Items).ItemId);        // FACHLICH keine Uebernahme
            var rejection = Assert.Single(IngestionRejections.Of(core));    // Ablehnung nachvollziehbar (R-35-Gedaechtnis)
            Assert.Equal("Ausserhalb des Produktkerns", rejection.Metadata["reason"]);
            Assert.Equal("neue Aussage aus dem Meeting", rejection.Metadata["statement"]);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact] // T3 / Replay — derselbe, bereits angewendete Plan erneut am Apply-Pfad
    public async Task Replay_desselben_Plans_fuehrt_zu_keinem_zweiten_Apply_und_keinen_Duplikaten()
    {
        var (root, planDir, plan) = await ArrangeAsync("plan-replay", """
            { "runId":"r-rp", "reviewer":"human (review-ui)",
              "decisions":[ { "incomingItemId":"IN-1", "decision":"apply" } ] }
            """);
        try
        {
            var accepted = IngestionApplyExec.AcceptedFromDecisions([new IngestionHumanDecision("IN-1", "apply", null)]);
            var first = await IngestionApplyExec.ExecuteAsync(planDir, plan, accepted, root, IngestRun);
            var coreAfterFirst = await File.ReadAllTextAsync(CorePaths.CoreFile(root));
            var snapshotsAfterFirst = SnapshotCount(root);

            var second = await IngestionApplyExec.ExecuteAsync(planDir, plan, accepted, root, IngestRun);

            Assert.Equal(Assert.Single(first.Applied).EntityId, Assert.Single(second.Applied).EntityId);
            Assert.Equal(coreAfterFirst, await File.ReadAllTextAsync(CorePaths.CoreFile(root))); // byte-identisch
            Assert.Equal(snapshotsAfterFirst, SnapshotCount(root));         // kein zweiter Snapshot = kein zweiter Save
            var core = await new JsonCoreRepository(root).LoadAsync();
            Assert.Equal(2, core.Items.Count);                              // KEIN Duplikat (REQ-1 + genau 1 neue)
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact] // T4 / gueltiger Kontrollfall — vergleichbare Aenderung MIT Freigabe kommt korrekt an
    public async Task Kontrollfall_Freigegebene_Aenderung_wird_korrekt_uebernommen_mit_Herkunft_Marker_und_Historie()
    {
        var (root, planDir, plan) = await ArrangeAsync("plan-ok", """
            { "runId":"r-ok", "reviewer":"human (review-ui)",
              "decisions":[ { "incomingItemId":"IN-1", "decision":"apply" } ] }
            """);
        try
        {
            var accepted = IngestionApplyExec.AcceptedFromDecisions([new IngestionHumanDecision("IN-1", "apply", null)]);
            var report = await IngestionApplyExec.ExecuteAsync(planDir, plan, accepted, root, IngestRun);

            var core = await new JsonCoreRepository(root).LoadAsync();
            var added = core.Items.Single(i => i.ItemId == Assert.Single(report.Applied).EntityId);
            Assert.Equal("neue Aussage aus dem Meeting", added.Text);        // erwartete = tatsaechliche Differenz
            Assert.Equal(IngestRun, added.SourceRunId);                      // Freigabebezug/Herkunft (R-31/I7)
            Assert.Equal(1, SnapshotCount(root));                            // Historie: Alt-Stand als Snapshot
            var applied = Path.Combine(planDir, "applied");
            Assert.True(File.Exists(Path.Combine(applied, "applied.marker")));
            Assert.True(File.Exists(Path.Combine(applied, "delta.json")));
            Assert.True(File.Exists(Path.Combine(applied, "core-before.json")));
        }
        finally { Directory.Delete(root, recursive: true); }
    }
}
