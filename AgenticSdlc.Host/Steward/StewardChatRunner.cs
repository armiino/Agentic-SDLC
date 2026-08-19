using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Steward;

/// <summary>
/// C1b (07.08.2026) — die Steward-Agent-Hülle (Vorlauf-Move C1b): EIN <c>ChatClientAgent</c> (deutscher
/// System-Prompt, Tool-Katalog aus C1a) + lokale Chat-Schleife + Session-Persistenz über die OFFIZIELLE
/// MAF-API (`SerializeSessionAsync`/`DeserializeSessionAsync` → `state/steward/sessions/&lt;name&gt;.json` —
/// I-1-Spike-Muster, K1-⚖). Observability über dieselben Nähte wie alle Stufen-Agenten
/// (AgentChatPipelineBuilder + ToolCallLoggerMiddleware; Logs je Chat-Sitzung unter runs/steward/).
/// CLI: steward [--session name] [--once "Frage"] (--once = eine Frage, Antwort, Ende — skript-/testbar).
/// </summary>
public static class StewardChatRunner
{
    public const string Phase = "steward";
    private const string SourceName = "AgenticSdlc.Host";

    /// <summary>Die EINE Bau-Naht des Steward-Agenten — Client injizierbar (Tests: ScriptedChatClient).
    /// <paramref name="memory"/> = M1-Modus (⚖ K6): null (voller Verlauf) · "count[:N]" (Schiebefenster) ·
    /// "summarize" (LLM-Verdichtung, opt-in) — konfiguriert den OFFIZIELLEN Reducer-Slot der Session-History.</summary>
    public static AIAgent BuildAgent(IChatClient baseClient, HostSettings settings, RunContext run, string repoRoot, string? memory = null,
        // C2d ②: Live-GitHub-Lese-Tools (McpClientTool ERBT von AIFunction — direkt unsere Tool-Sorte).
        IReadOnlyList<AITool>? liveTools = null,
        // Feil ② (Abnahme 4.0): der --session-Name als Harness-Fakt für Herkunfts-Stempel (nie vom Modell).
        string? sessionName = null)
    {
        var prompt = PromptProvider.Load(repoRoot, Phase, "StewardAgent", "StewardAgent1",
            new Dictionary<string, string>
            {
                ["runId"] = run.RunId,
                // Block-H-Fund 17.08.: der Steward kennt sein Projekt-Repo (Quelle: run-config fullworkflow.repo)
                // und fragt den Autor nicht danach. Leer = kein Repo konfiguriert (Prompt behandelt den Fall).
                ["projektRepo"] = settings.GithubRepo ?? "",
            });
        var client = AgentChatPipelineBuilder.Build(baseClient, settings, run, "StewardAgent", SourceName);
        // Die Steward-ROLLEN-Teilmenge des Werkzeugkastens (⚖ K7): C1a-Pipeline-Lesen + C3-Core-Lesen
        // (geteilter Kasten um die ICoreRepository-Naht) + C3-GitHub-Snapshot-Lesen + C1c-Start-Tools
        // (ApprovalRequired — Zustimmung kommt als MAF-nativer Approval-Roundtrip im Chat zurueck).
        var runTools = new StewardRunTools(repoRoot, settings, sessionName: sessionName);   // 1c: die Gate-Submits ketten den Resume über DIESELBE Naht; Feil ②: Session-Name = Harness-Fakt
        IReadOnlyList<AITool> tools =
        [
            .. new StewardReadTools(repoRoot).Build(),
            .. new CoreQueryTools(new JsonCoreRepository(repoRoot)).Build(),
            .. new GithubSnapshotQueryTools(repoRoot).Build(),
            .. new StewardGateTools(repoRoot, chainResume: id => runTools.ChainResumeAsync(id)).Build(),
            .. runTools.Build(),
            .. liveTools ?? [],
        ];
        var reducer = CreateReducer(memory, client);
        return client.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "StewardAgent",
            ChatOptions = new ChatOptions { Instructions = prompt, Tools = [.. tools] },
            ChatHistoryProvider = reducer is null ? null
                : new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions { ChatReducer = reducer }),
        }).AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Use(new StewardToolEchoMiddleware().InvokeAsync).Build();
    }

    // M1 (⚖ K6): der Reducer je Modus. MEAI001 bewusst: die gelieferten Implementierungen sind in M.E.AI 10.6
    // EXPERIMENTELL markiert (Spike-Fund #1) — Slot stabil, Implementierung gepinnt, Spike-Tests = Stolperdraht.
    // summarize nutzt den PIPELINE-Client (Verdichtungs-Calls laufen durch dieselbe Logging-/otel-Schicht).
#pragma warning disable MEAI001
    public static IChatReducer? CreateReducer(string? memory, IChatClient client) => memory switch
    {
        null => null,
        "summarize" => new SummarizingChatReducer(client, SummarizeKeepMessages, SummarizeThreshold),
        _ when memory.StartsWith("count", StringComparison.OrdinalIgnoreCase) =>
            new MessageCountingChatReducer(
                memory.Split(':') is [_, var n] ? int.Parse(n) : DefaultWindowMessages),
        _ => throw new ArgumentException($"Unbekannter memory-Modus '{memory}' (erlaubt: count[:N] | summarize)."),
    };
#pragma warning restore MEAI001

    /// <summary>M1b-Präzedenz (testbar): CLI-Flag überstimmt run-config-Default.</summary>
    internal static (string? Memory, bool FromConfig) ResolveMemory(string? cliValue, HostSettings settings)
        => cliValue is not null ? (cliValue, false)
         : settings.StewardMemoryMode is not null ? (settings.StewardMemoryMode, true)
         : (null, false);

    private const int DefaultWindowMessages = 40;
    private const int SummarizeKeepMessages = 20;
    private const int SummarizeThreshold = 10;

    // ToolCallContent ist die Basis (nur CallId) — der konkrete Aufruf ist FunctionCallContent (Name/Args).
    private static string ToolName(ToolApprovalRequestContent r)
        => (r.ToolCall as FunctionCallContent)?.Name ?? r.ToolCall.CallId;
    private static string ToolArgs(ToolApprovalRequestContent r)
        => (r.ToolCall as FunctionCallContent)?.Arguments is { } a ? JsonSerializer.Serialize(a) : "{}";

    public static string SessionPath(string repoRoot, string name)
        => Path.Combine(repoRoot, "state", "steward", "sessions", name + ".json");

    // C3-Session-Wache (⚖ K6-Vorstufe): deterministisch, kein LLM — jeder Zug schickt den GANZEN Verlauf
    // ans Modell, deshalb wird Wachstum SICHTBAR gemacht statt still bezahlt. Echte Reducer-Modi = Slice M1.
    private const long SessionWarnBytes = 150 * 1024;

    public static string SessionSizeNote(string path)
    {
        if (!File.Exists(path)) return "";
        var kb = new FileInfo(path).Length / 1024;
        return new FileInfo(path).Length > SessionWarnBytes
            ? $" · ⚠ Session {kb} KB — jeder Zug schickt den ganzen Verlauf ans Modell; neue Session empfohlen (--session <neuer-name>)"
            : $" · Session {kb} KB";
    }

    /// <summary>Session laden (offizieller Deserialize-Weg) oder frisch erzeugen.</summary>
    public static async Task<AgentSession> LoadOrCreateSessionAsync(AIAgent agent, string path)
    {
        if (File.Exists(path))
        {
            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(path).ConfigureAwait(false));
            return await agent.DeserializeSessionAsync(doc.RootElement.Clone()).ConfigureAwait(false);
        }
        return await agent.CreateSessionAsync().ConfigureAwait(false);
    }

    public static async Task SaveSessionAsync(AIAgent agent, AgentSession session, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = await agent.SerializeSessionAsync(session).ConfigureAwait(false);
        await File.WriteAllTextAsync(path, json.GetRawText()).ConfigureAwait(false);
    }

    /// <summary>M1 `--fresh`: leer starten, Bestand NICHT still löschen — rotiert nach &lt;name&gt;.prev.json.</summary>
    public static string? RotateForFresh(string path)
    {
        if (!File.Exists(path)) return null;
        var prev = Path.ChangeExtension(path, ".prev.json");
        File.Move(path, prev, overwrite: true);
        return prev;
    }

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        string sessionName = "default"; string? once = null; string? memory = null; var fresh = false; var debug = false;
        for (var i = 1; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--session", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) sessionName = args[++i];
            else if (string.Equals(args[i], "--once", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) once = args[++i];
            else if (string.Equals(args[i], "--memory", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) memory = args[++i];
            else if (string.Equals(args[i], "--fresh", StringComparison.OrdinalIgnoreCase)) fresh = true;
            else if (string.Equals(args[i], "--debug", StringComparison.OrdinalIgnoreCase)) debug = true;
        }

        // Politur 1b: Lauf-Konsole in die Protokoll-Datei statt ins Chat-Prompt (Autor-Kernärger der
        // UX-Kampagne); --debug reicht die Rohzeilen zusätzlich durch (Präfix „·").
        StewardRunConsole.Install(debugEcho: debug);

        // M1b: run-config-Default (steward.memory) — das CLI-Flag überstimmt (K6-Modi, gleiche Validierung).
        (memory, var memoryFromConfig) = ResolveMemory(memory, settings);

        var run = new RunContext(RunId.New(), "steward");
        run.EnsureFolders();

        // C2d ②: Live-GitHub-Lesen via offiziellem MCP-Server (readonly/x/issues — Governance SERVER-seitig).
        // FAIL-SOFT: ohne Token/Netz läuft der Steward einfach ohne Live-Blick (laut benannt, nie blockierend).
        ModelContextProtocol.Client.McpClient? mcp = null;
        IReadOnlyList<AITool>? liveTools = null;
        string liveNote = "";
        if (settings.StewardGithubLive && Mcp.GithubMcp.ResolveToken() is { } mcpToken)
        {
            try
            {
                mcp = await Mcp.GithubMcp.ConnectRemoteReadonlyIssuesAsync(mcpToken).ConfigureAwait(false);
                var mcpTools = await mcp.ListToolsAsync().ConfigureAwait(false);
                liveTools = [.. mcpTools];
                liveNote = $" · github-live={mcpTools.Count} Lese-Tools (MCP readonly)";
            }
            catch (Exception ex)
            {
                liveNote = " · github-live=aus (MCP nicht erreichbar)";
                Console.Error.WriteLine($"[steward] github-live nicht verfuegbar (weiter ohne): {ex.Message}");
            }
        }
        else if (settings.StewardGithubLive) liveNote = " · github-live=aus (kein Token)";

        await using var _mcp = mcp;
        AIAgent agent;
        try { agent = BuildAgent(ChatClientFactory.Create(settings), settings, run, repoRoot, memory, liveTools, sessionName); }
        catch (ArgumentException ex) { Console.Error.WriteLine($"[steward] {ex.Message}"); return 2; }   // memory-Modus LAUT

        var path = SessionPath(repoRoot, sessionName);
        var rotated = fresh ? RotateForFresh(path) : null;
        var session = await LoadOrCreateSessionAsync(agent, path).ConfigureAwait(false);
        var memoryNote = memory is null ? "" : memory.StartsWith("summarize", StringComparison.OrdinalIgnoreCase)
            ? $" · memory={memory} (LLM-Verdichtung aktiv — kostet Tokens)" : $" · memory={memory}";
        if (memoryFromConfig) memoryNote += " (run-config)";
        var freshNote = rotated is null ? "" : $" · Vorgänger-Stand → {Path.GetFileName(rotated)}";
        Console.WriteLine($"[steward] Sitzung '{sessionName}' ({(File.Exists(path) ? "fortgesetzt" : "neu")}){SessionSizeNote(path)}{memoryNote}{freshNote}{liveNote} · Logs: {Path.GetRelativePath(repoRoot, run.RunDir)} · /exit beendet");

        async Task TurnAsync(string input)
        {
            var response = await agent.RunAsync(input, session).ConfigureAwait(false);
            // K3-Approval-Roundtrip (offizielles Muster: nach JEDEM Run auf offene Anfragen pruefen):
            // startende Tools sind ApprovalRequired — die Zustimmung holt DIESE Schleife beim Autor ein.
            while (true)
            {
                if (!string.IsNullOrWhiteSpace(response.Text)) Console.WriteLine(StewardChatRendering.RenderAnswer(response.Text));
                var requests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();
                if (requests.Count == 0) break;
                if (once is not null)
                {
                    // --once ist nicht-interaktiv: NIE stillschweigend zustimmen (Governance). Anfrage überlebt
                    // die Session-Persistenz (I-1c-Beweis) — interaktiv fortsetzen beantwortet sie.
                    foreach (var r in requests)
                        Console.WriteLine($"[steward] ZUSTIMMUNG NÖTIG für Tool '{ToolName(r)}' — bitte interaktiv fortsetzen: steward --session {sessionName}");
                    break;
                }
                var answers = new List<AIContent>();
                foreach (var r in requests)
                {
                    // 1b-Rest ⚠ (stdin-Puffer, sicherheitsrelevant — Abnahme-Fund „Geister-Zeilen"): vor
                    // JEDER Zustimmungsfrage den Eingabe-Puffer leeren — eine gepufferte „ja"-Zeile aus
                    // einem Paste kann NIE eine ⚿-Zustimmung ausloesen; die Antwort muss frisch kommen.
                    DrainPendingConsoleInput();
                    Console.Write($"[steward] Tool '{ToolName(r)}' will laufen ({ToolArgs(r)}). Zustimmen? (ja/nein) > ");
                    var ok = string.Equals(Console.ReadLine()?.Trim(), "ja", StringComparison.OrdinalIgnoreCase);
                    answers.Add(r.CreateResponse(ok));
                }
                response = await agent.RunAsync(new ChatMessage(ChatRole.User, answers), session).ConfigureAwait(false);
            }
            await SaveSessionAsync(agent, session, path).ConfigureAwait(false);   // nach JEDEM Zug persistieren (K1)
        }

        if (once is not null) { await TurnAsync(once).ConfigureAwait(false); return 0; }

        while (true)
        {
            Console.Write("du> ");
            // Kosmetik-Feil (Abnahme 4.0): waehrend der Prompt wartet, setzen async Lebenszyklus-Zeilen
            // (⏸/✔) sich sichtbar ab und echoen den Prompt neu, statt an ihm zu kleben.
            StewardRunConsole.MarkPromptWaiting(true);
            var input = Console.ReadLine();
            StewardRunConsole.MarkPromptWaiting(false);
            // 1b-Rest ⚠: mehrzeiliger Paste wird nicht mehr STILL zerhackt (Geister-Antworten an spaetere
            // Prompts) — Rest LAUT verwerfen, damit weder Text still verloren geht noch Puffer-Zeilen
            // spaeter „von selbst" antworten. (Terminal only; redirected stdin = No-op.)
            WarnAndDrainIfMoreBuffered();
            if (input is null || string.Equals(input.Trim(), "/exit", StringComparison.OrdinalIgnoreCase)) return 0;
            if (string.IsNullOrWhiteSpace(input)) continue;
            await TurnAsync(input).ConfigureAwait(false);
        }
    }

    // 1b-Rest ⚠ stdin-Puffer-Schutz. Grenze ehrlich: wirkt nur am echten Terminal (KeyAvailable);
    // bei umgeleitetem stdin (Tests/Pipes/--once-Skripte) bewusst No-op. Die strukturelle Voll-Loesung
    // ist die dokumentierte Web-Chat-⚖ (todo-steward-ux-sprache Stufe 3).
    private static void DrainPendingConsoleInput()
    {
        if (Console.IsInputRedirected) return;
        try { while (Console.KeyAvailable) Console.ReadKey(intercept: true); }
        catch (InvalidOperationException) { /* keine Konsole — No-op */ }
    }

    private static void WarnAndDrainIfMoreBuffered()
    {
        if (Console.IsInputRedirected) return;
        var had = false;
        try { while (Console.KeyAvailable) { Console.ReadKey(intercept: true); had = true; } }
        catch (InvalidOperationException) { return; }
        if (had) Console.WriteLine("[steward] ⚠ Mehrzeilige Eingabe erkannt — nur die ERSTE Zeile wurde uebernommen, der Rest verworfen. Bitte als EINE Zeile einfuegen.");
    }
}

/// <summary>CLI-Registrierung der Steward-Schicht (R1-Muster wie alle Kettenglieder).</summary>
public static class StewardCommands
{
    public static void Register(IDictionary<string, CommandHandler> map)
        => map["steward"] = (args, settings, repoRoot) => StewardChatRunner.RunAsync(args, settings, repoRoot);
}
