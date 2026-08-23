using AgenticSdlc.Host.Steward;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Politur 1b (18.08., Autor: „der Chat gehört wieder mir"): die Konsolen-Weiche routet Lauf-stdout per
// AsyncLocal-Scope in die Protokoll-Datei; ALLES außerhalb des Scopes fließt unverändert durch (Chat,
// ⚿-Fragen, UI-Server). --debug reicht Scope-Zeilen zusätzlich mit „·"-Präfix durch. Getestet an der
// injizierbaren Mux-Instanz — KEIN globales Console.SetOut im Test (parallel-sicher; AsyncLocal ist je
// async-Fluss, also kollidieren parallele Tests nicht).
public sealed class StewardRunConsoleTests
{
    [Fact]
    public void Scope_geht_ins_Ziel_ausserhalb_fliesst_durch()
    {
        var original = new StringWriter();
        var mux = new StewardRunConsole.Mux(original, debugEcho: false);

        mux.WriteLine("chat-zeile VOR dem Lauf");                          // kein Scope → durch
        var target = new StringWriter();
        using (StewardRunConsole.Redirect(target))
            mux.WriteLine("[pipeline-full] lauf-zeile");                   // Scope → Ziel, NICHT Konsole
        mux.WriteLine("chat-zeile NACH dem Lauf");                         // Scope beendet → wieder durch

        Assert.Contains("VOR dem Lauf", original.ToString());
        Assert.Contains("NACH dem Lauf", original.ToString());
        Assert.DoesNotContain("lauf-zeile", original.ToString());          // der Kernärger ist tot
        Assert.Contains("[pipeline-full] lauf-zeile", target.ToString());
    }

    [Fact]
    public async Task Scope_fliesst_durch_TaskRun_und_await_der_Lauf_Task_bleibt_still()
    {
        var original = new StringWriter();
        var mux = new StewardRunConsole.Mux(original, debugEcho: false);
        var target = new StringWriter();

        // exakt das StewardRunTools-Muster: Redirect INNERHALB des Task.Run-Lambdas (ExecutionContext fließt).
        await Task.Run(async () =>
        {
            using var _ = StewardRunConsole.Redirect(target);
            mux.WriteLine("zeile 1 im lauf");
            await Task.Delay(1);
            mux.WriteLine("zeile 2 nach await");
        });
        mux.WriteLine("steward-zeile danach");

        Assert.DoesNotContain("im lauf", original.ToString());
        Assert.DoesNotContain("nach await", original.ToString());          // AsyncLocal überlebt await
        Assert.Contains("zeile 2 nach await", target.ToString());
        Assert.Contains("steward-zeile danach", original.ToString());
    }

    [Fact]
    public void Debug_Echo_reicht_Scope_Zeilen_MIT_Praefix_zusaetzlich_durch()
    {
        var original = new StringWriter();
        var mux = new StewardRunConsole.Mux(original, debugEcho: true);
        var target = new StringWriter();

        using (StewardRunConsole.Redirect(target))
            mux.WriteLine("[pipeline-full] rohzeile");

        Assert.Contains("[pipeline-full] rohzeile", target.ToString());    // Datei bekommt sie IMMER
        Assert.Contains("· [pipeline-full] rohzeile", original.ToString());// --debug: sichtbar, als Maschine markiert
    }

    // UI-Weg-Stille (18.08., letzte 1b-Lücke): der deklarierte [review-ui]-Nutzer-Kanal (URL!,
    // Browser-Warnung) erreicht den Autor AUCH im Umleitungs-Scope — sonst sässe er ohne URL da,
    // wenn der Browser nicht aufpoppt. Alles andere bleibt still; die Datei behält alles als Beleg.
    [Fact]
    public void ReviewUi_Nutzer_Kanal_erreicht_den_Chat_auch_im_Scope()
    {
        var original = new StringWriter();
        var mux = new StewardRunConsole.Mux(original, debugEcho: false);
        var target = new StringWriter();

        using (StewardRunConsole.Redirect(target))
        {
            mux.WriteLine($"{AgenticSdlc.HumanReview.LocalReviewServerHost.UserChannelPrefix} 3 Items — offen unter: http://127.0.0.1:9999");
            mux.WriteLine("[pbi-update-review] R-43: resume laeuft automatisch an ...");
        }

        Assert.Contains("offen unter: http://127.0.0.1:9999", original.ToString()); // URL erreicht den Autor
        Assert.DoesNotContain("R-43", original.ToString());                          // Roh-Log bleibt still
        Assert.Contains("offen unter", target.ToString());                           // Beleg-Datei hat BEIDES
        Assert.Contains("R-43", target.ToString());
    }

    [Fact]
    public void ReviewUi_Kanal_wird_im_Debug_Modus_nicht_doppelt_gedruckt()
    {
        var original = new StringWriter();
        var mux = new StewardRunConsole.Mux(original, debugEcho: true);
        var target = new StringWriter();

        using (StewardRunConsole.Redirect(target))
            mux.WriteLine($"{AgenticSdlc.HumanReview.LocalReviewServerHost.UserChannelPrefix} offen unter: http://x");

        var einmal = original.ToString().Split("offen unter").Length - 1;
        Assert.Equal(1, einmal);                                                     // Durchreichen gewinnt, kein ·-Echo dazu
    }

    [Fact]
    public void RunLogWriter_puffert_bis_runId_und_traegt_dann_nach()
    {
        var dir = Directory.CreateTempSubdirectory("p1b-").FullName;
        var path = Path.Combine(dir, "logs", "console.log");
        using var w = new StewardRunConsole.RunLogWriter();

        w.WriteLine("banner VOR der runId");                               // start-async: runId kommt per Callback
        w.SetTarget(path);
        w.WriteLine("zeile NACH SetTarget");

        var content = File.ReadAllText(path);
        Assert.Contains("banner VOR der runId", content);                  // Puffer nachgetragen — nichts verloren
        Assert.Contains("zeile NACH SetTarget", content);
    }

    [Fact]
    public void Fehlstart_ohne_runId_verschluckt_den_Vorlauf_nicht()
    {
        using var w = new StewardRunConsole.RunLogWriter();
        w.WriteLine("usage-hinweis des runners");
        Assert.Contains("usage-hinweis", w.DrainBuffered());               // StartPipelineAsync reicht ihn an den Chat
        Assert.Equal("", w.DrainBuffered());
    }

    [Fact]
    public void EndLine_meldet_Pause_Fertig_und_Fehler_wohlgeformt()
    {
        var repo = Directory.CreateTempSubdirectory("p1b-end-").FullName;
        var ckpt = Path.Combine(repo, "runs", "fullworkflow", "R1", "checkpoints");
        Directory.CreateDirectory(ckpt);
        File.WriteAllText(Path.Combine(ckpt, "pointer.json"), """{ "mode": "pbi-gate" }""");

        Assert.Contains("haelt am Checkpoint 'pbi-gate'", StewardRunTools.EndLine(repo, "R1", 0));   // Pause gewinnt
        Assert.Contains("FERTIG", StewardRunTools.EndLine(repo, "R2", 0));                            // kein Pointer + exit 0
        Assert.Contains("Fehler (exit -1)", StewardRunTools.EndLine(repo, "R3", -1));                 // laut, nie still
    }
}

// 1b′ (Stale-Pointer-Härtung): der Status-Kern verkauft NIE einen veralteten Zeiger als Pause — das
// letzte Pause/Resume-Event in DATEI-Reihenfolge entscheidet (Append-Wahrheit, kein Timestamp-Raten).
public sealed class StalePointerStatusTests
{
    private static string Fixture(params string[] eventTypes)
    {
        var repo = Directory.CreateTempSubdirectory("p1bs-").FullName;
        var run = Path.Combine(repo, "runs", "fullworkflow", "RX");
        Directory.CreateDirectory(Path.Combine(run, "checkpoints"));
        Directory.CreateDirectory(Path.Combine(run, "logs"));
        File.WriteAllText(Path.Combine(run, "checkpoints", "pointer.json"),
            """{ "runId":"RX", "sessionId":"s", "checkpointId":"c1", "mode":"pbi-gate", "savedUtc":"2026-08-18T10:00:00Z" }""");
        File.WriteAllLines(Path.Combine(run, "logs", "events.jsonl"),
            eventTypes.Select(t => $$$"""{"type":"{{{t}}}","runId":"RX"}"""));
        return repo;
    }

    [Fact]
    public async Task Gueltiger_Zeiger_bleibt_Paused()
    {
        var repo = Fixture("PIPELINE_START", "PIPELINE_PAUSED");
        var s = await AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repo, "RX");
        Assert.Equal(AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunState.Paused, s!.State);
        Assert.Equal("pbi-gate", s.PausedGate);
    }

    [Fact]
    public async Task Resume_nach_der_Pause_macht_den_Zeiger_ungueltig_Lauf_arbeitet()
    {
        var repo = Fixture("PIPELINE_START", "PIPELINE_PAUSED", "PIPELINE_RESUME");   // der Session-2-Vorfall
        var s = await AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repo, "RX");
        Assert.Equal(AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunState.NotPausedNotFinished, s!.State);
        Assert.Null(s.PausedGate);                                                     // NIE das alte Gate verkaufen
        Assert.Contains(s.NextRequiredAction, a => a.Contains("ARBEITET GERADE"));
    }

    [Fact]
    public async Task Neue_Pause_nach_dem_Resume_macht_den_Zeiger_wieder_gueltig()
    {
        var repo = Fixture("PIPELINE_PAUSED", "PIPELINE_RESUME", "PIPELINE_STILL_PAUSED");  // R-52-Re-Pause zaehlt
        var s = await AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repo, "RX");
        Assert.Equal(AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunState.Paused, s!.State);
        Assert.Equal("pbi-gate", s.PausedGate);
    }
}

// Feinschliff #2 (18.08.): der Chat trennt sichtbar, wer spricht.
public sealed class StewardChatRenderingTests
{
    [Fact]
    public void Antwort_beginnt_als_eigener_Block_mit_steward_Marker()
    {
        var r = AgenticSdlc.Host.Steward.StewardChatRendering.RenderAnswer("Hallo\nZeile 2\n");
        Assert.StartsWith(Environment.NewLine + "steward> Hallo", r);
        Assert.Contains("Zeile 2", r);
        Assert.EndsWith(Environment.NewLine, r);                       // Leerzeile vor dem nächsten du>
        Assert.DoesNotContain("Zeile 2\n\n\n", r);                     // TrimEnd: kein Leerzeilen-Stapel
    }
}

// Feinschliff (18.08., Autor: „zwei list_core_items waren nicht unterscheidbar"): Schritt-Echo mit Arg-Hinweis.
public sealed class StewardToolEchoTests
{
    [Fact]
    public void ArgHint_zeigt_bekannte_Schluessel_kompakt_und_gekappt()
    {
        var hint = AgenticSdlc.Host.Steward.StewardToolEchoMiddleware.ArgHint(
            new Dictionary<string, object?> { ["status"] = "needs_clarify", ["irrelevant"] = "x", ["query"] = new string('q', 60) });
        Assert.Contains("status=needs_clarify", hint);
        Assert.Contains("query=" + new string('q', 40) + "…", hint);   // hart gekappt
        Assert.DoesNotContain("irrelevant", hint);
        Assert.Equal("", AgenticSdlc.Host.Steward.StewardToolEchoMiddleware.ArgHint(null));
    }
}

// 1b-Rest: Fortschritts-Vertrag „Checkpoint n von max. m" + open_gate_ui-Fähigkeitsliste.
public sealed class RouteAndUiCapabilityTests
{
    [Fact]
    public void RouteLine_zeigt_Position_Zweig_und_Reststrecke()
    {
        var l = AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunStatusReader.RouteLine("decision-gate")!;
        Assert.Contains("Checkpoint 6 von max. 8 (Betrieb)", l);
        Assert.Contains("danach: pbi-gate → github-forward-gate", l);

        var last = AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunStatusReader.RouteLine("github-forward-gate")!;
        Assert.Contains("letzter Halt", last);

        var boot = AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunStatusReader.RouteLine("cluster-review-gate")!;
        Assert.Contains("(Bootstrap)", boot);
        Assert.Null(AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunStatusReader.RouteLine("unbekannt"));
    }

    [Fact]
    public async Task Pause_Status_traegt_die_Checkpoint_Zeile()
    {
        var repo = Directory.CreateTempSubdirectory("route-").FullName;
        var run = Path.Combine(repo, "runs", "fullworkflow", "RX");
        Directory.CreateDirectory(Path.Combine(run, "checkpoints"));
        File.WriteAllText(Path.Combine(run, "checkpoints", "pointer.json"),
            """{ "runId":"RX", "sessionId":"s", "checkpointId":"c1", "mode":"pbi-gate", "savedUtc":"2026-08-18T10:00:00Z" }""");
        var s = await AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repo, "RX");
        Assert.Contains(s!.NextRequiredAction, a => a.Contains("Checkpoint 7 von max. 8"));
    }

    [Fact]
    public void UiCommandFor_liefert_die_korrekte_Alternative_je_UIonly_Checkpoint()
    {
        // 1g-C / 9k(c)-Vertragswechsel (19.08.): classify + adr sind jetzt STEWARD-BEDIENBAR (open_gate_ui
        // ruft die Standalone-UIs und kettet den Resume explizit) — kein Fremd-Terminal-Verweis mehr.
        Assert.Contains("arch-classify-gate", StewardRunTools.SupportedUiGates);
        Assert.Contains("adr-gate", StewardRunTools.SupportedUiGates);
        Assert.Null(StewardRunTools.UiCommandFor("arch-classify-gate"));
        Assert.Null(StewardRunTools.UiCommandFor("adr-gate"));

        Assert.Contains("ledger-adjudicate-ui", StewardRunTools.UiCommandFor("adjudication-gate"));
        Assert.Null(StewardRunTools.UiCommandFor("ingest-gate"));       // hat open_gate_ui, keine Alternative nötig
        // Phase-1i ② (23.08., R-64-Ausbau): + github-forward-gate — Endbild „Chat ODER UI" an ALLEN Urteils-Gates.
        Assert.Equal(7, StewardRunTools.SupportedUiGates.Length);       // die EINE Fähigkeits-Quelle
    }
}
