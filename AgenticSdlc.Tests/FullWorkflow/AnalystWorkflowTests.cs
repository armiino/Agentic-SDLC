using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Analyst;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 1g-B (19.08., core-analyst-design §5): der Analyse-Graph LLM-frei am ECHTEN Graphen (MAF-first-Regel:
// Graph-Wiring nur per In-Process-Run-Test beweisbar). Pins: Form-Checker im Maker (Core-Anker-Pflicht),
// vierstufiger Rauschen-Trichter (Dedup gegen Core/DEC/REJ/Linsen + Kritiker sichtbar), NEU vs. WEITERHIN
// OFFEN, Delta im Autor-Front-Vertrag mit origin CoreAnalyst — und: der Lauf mutiert NICHTS.
public sealed class AnalystWorkflowTests
{
    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted") =>
        new ProjectStateItem(id, type, text, "MEETING", null, 1, "run-1", null, null, null, null, [], [],
            new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument MiniCore(params ProjectStateItem[] extra)
        => new("p", 4, DateTime.UnixEpoch, [],
            [Item("REQ-88", "requirement", "Angehörige werden per Push an bestätigte Besuche erinnert."),
             Item("DEC-01", "decision", "Wie lange werden Bemerkungen aufbewahrt?", status: "open_decision"),
             .. extra], [], [], []);

    private static AnalystFinding Fund(string statement, string linse = "nfr", string kategorie = "nfr:privacy",
        string disposition = "requirement", string anker = "REQ-88")
        => new(linse, disposition, statement, $"abgeleitet aus {anker}", kategorie, [anker]);

    // ── Form-Checker (Ebene 1, im Maker-Werkzeug) ──

    [Fact]
    public void SaveFindings_erzwingt_Core_Anker_Kategorie_und_Herleitung()
    {
        var tools = new AnalystLensTools(AnalystLenses.All[1], MiniCore());
        var save = tools.Build().OfType<AIFunction>().Single(f => f.Name == "save_findings");
        string Invoke(AnalystFinding[] fs) => JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(
            save.InvokeAsync(new AIFunctionArguments { ["findings"] = fs }).GetAwaiter().GetResult()))!;

        var bad = Invoke([
            Fund("Ohne Anker.") with { AnkerIds = [] },
            Fund("Falscher Anker.", anker: "REQ-999"),
            Fund("Falsche Kategorie.") with { Kategorie = "nfr:magie" },
            Fund("Ohne Herleitung.") with { Herleitung = "" }]);
        Assert.Contains("CORE-ANKER-PFLICHT", bad);
        Assert.Contains("REQ-999", bad);
        Assert.Contains("nfr:magie", bad);
        Assert.Contains("herleitung PFLICHT", bad);
        Assert.Null(tools.Saved);                                             // nichts halb gespeichert

        var ok = Invoke([Fund("Einwilligung der Angehörigen in Push-Nachrichten fehlt.")]);
        Assert.Contains("\"saved\": true", ok.Replace("\":true", "\": true"));
        Assert.Equal("nfr", Assert.Single(tools.Saved!).Linse);               // Linsen-Stempel = Harness-Fakt
        Assert.Contains("ALREADY_SAVED", Invoke([Fund("Zweiter Versuch.")])); // save-once
    }

    // ── Der ganze Graph, LLM-frei (ScriptedAgents) ──

    private sealed class ScriptedAgent(IReadOnlyList<AITool> tools, string toolName, Func<object> args) : AIAgent
    {
        protected override async Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages,
            AgentSession? session, AgentRunOptions? options, CancellationToken ct)
        {
            var fn = tools.OfType<AIFunction>().First(f => f.Name == toolName);
            await fn.InvokeAsync(new AIFunctionArguments { [toolName == "save_findings" ? "findings" : "verdicts"] = args() }, ct).ConfigureAwait(false);
            return new AgentResponse(new ChatMessage(ChatRole.Assistant, "ok"));
        }
        protected override IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> m, AgentSession? s, AgentRunOptions? o, CancellationToken c) => throw new NotSupportedException();
        private sealed class S : AgentSession;
        protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken c) => ValueTask.FromResult<AgentSession>(new S());
        protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement e, JsonSerializerOptions? j, CancellationToken c) => throw new NotSupportedException();
        protected override ValueTask<JsonElement> SerializeSessionCoreAsync(AgentSession s, JsonSerializerOptions? j, CancellationToken c) => throw new NotSupportedException();
    }

    [Fact]
    public async Task Graph_laeuft_end_zu_end_Trichter_greift_und_Delta_traegt_die_ehrliche_Herkunft()
    {
        // Core mit REJ-Gedächtnis (R-35) — der wortgleiche Fund muss im Dedup hängen bleiben.
        var core = MiniCore();
        (core, _) = IngestionRejections.Record(core,
            new StateChangePlanDocument(1, "alt", DateTime.UnixEpoch, "d",
                [new StateChangeOperation("A-1", StateChangeKind.New, "Erinnerungen auch für unbestätigte Besuche.", null, null, [], "t")]),
            [new IngestionHumanDecision("A-1", "reject", "bewusst zurückgestellt")], "plan.json");

        var run = new RunContext(RunId.New(), "test-analyst"); run.EnsureFolders();
        var outDir = run.OutputDir("analysis");

        // Je Linse ein Skript: nfr liefert 2 Funde (1 guter, 1 = wortgleich zur REJ), funktional 1 Floskel
        // (Kritiker sortiert aus), arch/risiko ehrlich leer.
        Func<IReadOnlyList<AITool>, AIAgent> lensFactory = tools =>
        {
            var lens = ((AnalystLensTools?)null); // Linse steckt im Tool-Satz — Skript wählt über Beschreibung? Nein:
            return new ScriptedAgent(tools, "save_findings", () =>
            {
                // Der Auftrag ist linsen-spezifisch, aber der Fake sieht nur Tools — wir unterscheiden über
                // einen Zähler ist unnötig: alle 4 Linsen liefern DIESELBE Skript-Logik nicht; stattdessen
                // liefert JEDER Aufruf die Funde und der Linsen-Stempel kommt vom Harness. Dubletten fängt
                // der Merge (dedup_linsen) — genau das pinnt dieser Test mit.
                return new[]
                {
                    Fund("Einwilligung der Angehörigen in Push-Nachrichten fehlt."),
                    Fund("Erinnerungen auch für unbestätigte Besuche."),               // = REJ → dedup_rejection
                    Fund("Das System sollte generell sicher sein.", kategorie: "nfr:security"), // Floskel → Kritiker
                };
            });
        };
        Func<IReadOnlyList<AITool>, AIAgent> kritikerFactory = tools =>
            new ScriptedAgent(tools, "save_verdicts", () => new[]
            {
                new AnalystVerdict(0, true, null),
                new AnalystVerdict(1, false, "generische Floskel ohne tragende Herleitung"),
            });

        var vorgaenger = new HashSet<string>(StringComparer.Ordinal)
        { IdentityKey.From("Einwilligung der Angehörigen in Push-Nachrichten fehlt.") };  // → WEITERHIN OFFEN

        var wf = AnalystWorkflow.Build(core, vorgaenger, lensFactory, kritikerFactory, run, outDir);
        var wfRun = await InProcessExecution.Default.RunAsync(wf, new AnalystWorkflow.Trigger(), run.RunId, CancellationToken.None);
        Assert.DoesNotContain(wfRun.OutgoingEvents, e => e is ExecutorFailedEvent or WorkflowErrorEvent);

        var report = JsonSerializer.Deserialize<AnalystReport>(
            await File.ReadAllTextAsync(Path.Combine(outDir, "report.json")), JsonFiles.Json)!;

        // Trichter: 4 Linsen × 3 Funde = 12 → je Statement gewinnt der ERSTE (Linsen-Reihenfolge det.),
        // die 9 Wiederholungen = dedup_linsen; die drei Erst-Funde verteilen sich auf weiterhin_offen /
        // dedup_rejection / kritiker_aussortiert (sichtbar MIT Grund).
        Assert.StartsWith("Einwilligung",
            Assert.Single(report.Eintraege, e => e.Status == AnalystStatus.WeiterhinOffen).Fund.Statement);
        Assert.StartsWith("Erinnerungen auch",
            Assert.Single(report.Eintraege, e => e.Status == AnalystStatus.DedupRejection).Fund.Statement);
        var aussortiert = Assert.Single(report.Eintraege, e => e.Status == AnalystStatus.KritikerAussortiert);
        Assert.Equal("generische Floskel ohne tragende Herleitung", aussortiert.Grund);   // kein stiller Cap
        Assert.Equal(9, report.Eintraege.Count(e => e.Status == AnalystStatus.DedupLinsen));
        Assert.Empty(report.Eintraege.Where(e => e.Status == AnalystStatus.Neu));

        // Delta: NUR der eine tragende Fund, im Autor-Front-Vertrag mit ehrlicher Herkunft — und LADBAR
        // über dieselbe Naht wie --from-delta.
        var deltaPath = Path.Combine(outDir, "delta.json");
        var loaded = (await JsonProjectStateRepository.LoadAsync(deltaPath)).Document;
        var item = Assert.Single(loaded.Items);
        Assert.Equal(AnalystDeltaBuilder.Origin, item.Origin);
        Assert.Equal("nfr:privacy", item.Metadata[RequirementsDocumentProjection.KategorieKey]);
        Assert.Contains("abgeleitet aus REQ-88", item.Metadata["herleitung"]);

        // 1g-Abnahme-Vorprüfungs-Fund (Besteller-Falle): die Analyst-Herkunft ÜBERLEBT den Tor-1-Apply —
        // sonst wäre die NFR-Sektion des Anforderungsdokuments trotz Adoption für immer leer.
        var meta = new Dictionary<string, string>(StringComparer.Ordinal);
        AnalystOriginMeta.CarryOver(item, meta);
        Assert.Equal("nfr:privacy", meta[AnalystOriginMeta.Kategorie]);
        Assert.Contains("REQ-88", meta[AnalystOriginMeta.Herleitung]);
        Assert.Equal("arch", meta[AnalystOriginMeta.Linse]);   // erste Linse alphabetisch gewinnt den Erst-Fund

        // Der Lauf mutiert NICHTS: der Core ist byte-identisch geblieben (kein Gate nötig — bewiesen).
        Assert.Equal(RequirementsDocumentProjection.Fingerprint(core), report.CoreFingerprint);

        var md = await File.ReadAllTextAsync(Path.Combine(outDir, "report.md"));
        Assert.Contains("## WEITERHIN OFFEN", md);
        Assert.Contains("## Aussortiert (Kritiker", md);
    }

    [Fact]
    public void Kollektoren_und_Digest_sind_deterministisch_und_nur_aktiv()
    {
        var core = MiniCore(
            Item("PBI-1", "pbi", "Arbeitspaket") ,
            Item("REQ-ALT", "requirement", "Abgelöst.", status: "superseded"));
        var digest = AnalystCollect.Digest(core);
        Assert.Contains("REQ-88 [requirement]", digest);
        Assert.DoesNotContain("REQ-ALT", digest);                              // Blick = nur aktive Wahrheit

        var funde = AnalystCollect.KollektorFunde(core);
        Assert.Contains("ohne deckendes Arbeitspaket", funde);
        Assert.Contains("REQ-88", funde);
        Assert.Contains("Offene Entscheidungen", funde);
        Assert.Contains("DEC-01", funde);
    }

    // AUSWAHL ≠ Bearbeitung (Autor-⚖ 20.08.): kuratiertes Teil-Delta — wörtlich, Herkunft intakt,
    // Voll-Delta unberührt; ungültige Indices LAUT.
    [Fact]
    public void Auswahl_kuratiert_woertlich_und_laesst_das_Voll_Delta_unberuehrt()
    {
        var repo = Directory.CreateTempSubdirectory("analyst-curate-").FullName;
        var dir = Path.Combine(repo, "runs", "core-analysis", "20260820_000001_cur", "analysis");
        Directory.CreateDirectory(dir);
        var report = new AnalystReport("20260820_000001_cur", DateTime.UnixEpoch, "fp",
            [new AnalystReportEintrag(Fund("Fund eins."), AnalystStatus.Neu),
             new AnalystReportEintrag(Fund("Fund zwei."), AnalystStatus.WeiterhinOffen),
             new AnalystReportEintrag(Fund("Fund drei."), AnalystStatus.Neu)]);
        File.WriteAllText(Path.Combine(dir, "report.json"), JsonSerializer.Serialize(report, JsonFiles.Json));
        File.WriteAllText(Path.Combine(dir, "delta.json"), "VOLL-DELTA-BELEG");

        var (path, count, fehler) = AnalystRunner.CurateDelta(repo, "20260820_000001_cur", [3, 1]);
        Assert.Empty(fehler);
        Assert.Equal(2, count);
        var kuratiert = JsonSerializer.Deserialize<ProjectStateDocument>(File.ReadAllText(path), ProjectStateJson.Options)!;
        Assert.Equal(["Fund eins.", "Fund drei."], kuratiert.Items.Select(i => i.Text));   // wörtlich, sortiert
        Assert.All(kuratiert.Items, i => Assert.Equal(AnalystDeltaBuilder.Origin, i.Origin)); // Herkunft intakt
        Assert.Equal("VOLL-DELTA-BELEG", File.ReadAllText(Path.Combine(dir, "delta.json")));  // Beleg unberührt

        var (_, _, bad) = AnalystRunner.CurateDelta(repo, "20260820_000001_cur", [7]);
        Assert.Contains(bad, f => f.Contains("index 7 ungueltig"));
    }

    [Fact]
    public void Vorgaenger_Keys_kommen_aus_dem_juengsten_Report()
    {
        var repo = Directory.CreateTempSubdirectory("analyst-prev-").FullName;
        Assert.Empty(AnalystRunner.LadeVorgaengerKeys(repo, "x"));             // erster Lauf = leer

        var dir = Path.Combine(repo, "runs", "core-analysis", "20260819_000001_aaa", "analysis");
        Directory.CreateDirectory(dir);
        var report = new AnalystReport("20260819_000001_aaa", DateTime.UnixEpoch, "fp",
            [new AnalystReportEintrag(Fund("Alter Fund."), AnalystStatus.Neu)]);
        File.WriteAllText(Path.Combine(dir, "report.json"), JsonSerializer.Serialize(report, JsonFiles.Json));

        var keys = AnalystRunner.LadeVorgaengerKeys(repo, "20260819_000002_bbb");
        Assert.Contains(IdentityKey.From("Alter Fund."), keys);
    }
}
