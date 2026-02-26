//using System.Text.Json;
using AgenticSdlc.Host.Mcp;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

var runId = RunId.New();
var run = new RunContext(runId);
run.EnsureFolders();
CleanDocsFolder();

var config = new
{
    runId,
    phase = "Phase1-Level1-SINGLE_PASS",
    model = "qwen2.5:14b",
    ollamaBaseUrl = "http://localhost:11434/", //stanni bei lokal ollama
    timestampUtc = DateTime.UtcNow
};
run.WriteConfig(config);

WriteRunChangeNote(run);

static void WriteRunChangeNote(RunContext run)
{
    Console.WriteLine();
    Console.WriteLine("Run change note (what is new in this run?).");
    Console.WriteLine("Type a short note and press Enter. Leave empty for 'nichts neues in diesem Run'.");
    Console.Write("> ");

    var note = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(note))
        note = "nichts neues in diesem Run";

    var content =
        $"""
         runId: {run.RunId} timestampUtc: {DateTime.UtcNow:O}
        {note}
        """;

    // Speichern unter runs/<runId>/logs/changes.txt
    File.WriteAllText(run.ChangesPath, content);

    //auch als Event (damit es in events.jsonl auffindbar ist)
    run.AppendEvent(new { type = "RUN_CHANGE_NOTE", runId = run.RunId, note, timestampUtc = DateTime.UtcNow });
}

run.AppendEvent(new { type = "RUN_STARTED", runId, timestampUtc = DateTime.UtcNow });

// mit MCP local connecten
await using var localMcp = await McpConnections.ConnectLocalAsync();
run.AppendEvent(new { type = "MCP_LOCAL_CONNECTED", runId, timestampUtc = DateTime.UtcNow });

// nur als vorbereitung aktuell passiert hier noch nichts
string? githubConnectError = null;
try
{
    await using var githubMcp = await McpConnections.ConnectGitHubAsync();
    run.AppendEvent(new { type = "MCP_GITHUB_CONNECTED", runId, timestampUtc = DateTime.UtcNow });

    // discovery ja, nutzen aktuell nein 
    _ = await githubMcp.ListToolsAsync().ConfigureAwait(false);
    run.AppendEvent(new { type = "MCP_GITHUB_DISCOVERED", runId, timestampUtc = DateTime.UtcNow });
}
catch (Exception ex)
{
    githubConnectError = ex.Message;
    run.AppendEvent(new { type = "MCP_GITHUB_FAILED", runId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
}

// bereitstellen der MCP-Tools, sodass sie als KI-Tools intepretiert werden können (durch cast möglich)
var localTools = (await localMcp.ListToolsAsync().ConfigureAwait(false))
    .Cast<AITool>()
    .ToList();

run.WriteToolDiscovery(new
{
    runId,
    local = new { count = localTools.Count, tools = localTools.Select(t => t.Name).ToArray() },
    github = new { error = githubConnectError }
});

run.AppendEvent(new { type = "TOOLS_DISCOVERED", runId, localCount = localTools.Count, timestampUtc = DateTime.UtcNow });

//chat client: ollamasharp + Microsoft.Extensions.AI pipeline
IChatClient baseChatClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "qwen2.5:14b");

/*
 automatisches enablen der function/tool invocation in der chat-pipeline
- erkennt tool-calls vom Modell-Output und führt sie aus
- gibt die tool-ergebnisse wieder ins LLM zurück 
- sonst liefert mir das LLM nur ein JSON mit zB (readFile) und ich muss das selsbt Parsen und ausfürehn
*/
IChatClient chat = new ChatClientBuilder(baseChatClient)
    .UseFunctionInvocation()
    .Build();

// Anweisungen für das LLM -> kein gescriptetes verwenden von Tools
var instructions = $$"""
You are an agent working inside a git repository.

Goal:
Create SDLC artifacts from stakeholder transcripts.

Inputs:
- Transcripts are in: input/transcripts/
You MUST discover and process ALL transcript files found there.

Outputs (write via fs_write to docs/):
- docs/requirements.md with sections:
  - Functional Requirements
  - Non-functional Requirements
  - Constraints/Compliance
  - Traceability (each item links to transcript chunks you cite)
- docs/open-questions.md
- docs/risks.md
- docs/architecture.md
Optional:
- docs/issues.json (draft)

Constraints:
- You can only read/write files via available tools.
- Read roots: input/, docs/, runs/
- Write roots: docs/, runs/
- Do NOT invent placeholder artifacts. If transcripts cannot be read, STOP and report the exact error.
- Do NOT print pseudo tool calls. If you need to read/write, you MUST actually call the tool.
- At the end, you MUST call request_approval with:
    action = "phase1_review"
    payload = a JSON string that MUST include "runId": "{runId}" plus created/updated files + a short summary.
RunId for this run: {{runId}}

Start when ready.
""";

// Grundbasis meines ersten agenten + loacl tools 
AIAgent baseAgent = chat.AsAIAgent(
    instructions: instructions,
    name: "Phase1SinglePass",
    tools: [.. localTools]
);

// middleware (tool call logging unter runs/<runId>/logs/events.jsonl) -> wird quasi "drangehängt" 
var toolLogger = new ToolCallLoggerMiddleware(run);
var agent = baseAgent
    .AsBuilder()
    .Use(toolLogger.InvokeAsync)
    .Build();

run.AppendEvent(new
{
    type = "AGENT_STARTED",
    runId,
    agentName = "Phase1SinglePass",
    allowedTools = localTools.Select(t => t.Name).ToArray(),
    timestampUtc = DateTime.UtcNow
});

// Starte den run
var result = await agent.RunAsync("Begin.");
Console.WriteLine(result);

// nach dem run -> angelegte docs.
var requiredDocs = new[]
{
    "docs/requirements.md",
    "docs/open-questions.md",
    "docs/risks.md",
    "docs/architecture.md"
};

var missing = requiredDocs.Where(p => !File.Exists(p)).ToList();
if (missing.Count > 0)
{
    run.AppendEvent(new
    {
        type = "RUN_FAILED",
        runId,
        reason = "Missing required docs",
        missing,
        timestampUtc = DateTime.UtcNow
    });

    Console.Error.WriteLine("RUN FAILED - missing required docs:");
    foreach (var m in missing) Console.Error.WriteLine($" - {m}");
    Environment.ExitCode = 2;
    return;
}

// Snapshot specihern docs -> runs/<runId>/snapshots/docs
CopyDirectory("docs", run.DocsSnapshotDir);

run.AppendEvent(new { type = "RUN_FINISHED", runId, status = "completed", timestampUtc = DateTime.UtcNow });

static void CopyDirectory(string sourceDir, string targetDir)
{
    Directory.CreateDirectory(targetDir);

    if (!Directory.Exists(sourceDir))
        return;

    foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
    {
        var rel = Path.GetRelativePath(sourceDir, file);
        var dest = Path.Combine(targetDir, rel);
        Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
        File.Copy(file, dest, overwrite: true);
    }
}

//docs muss vor jedem run "geleert" werden damit keine alten Daten ausversehen bleiben oder sich etwas vermischt.
static void CleanDocsFolder()
{
    var docsDir = "docs";

    if (!Directory.Exists(docsDir))
        return;

    var files = Directory.GetFiles(docsDir, "*.md", SearchOption.TopDirectoryOnly);

    foreach (var file in files)
    {
        var name = Path.GetFileName(file);

        if (name.Equals(".gitkeep", StringComparison.OrdinalIgnoreCase))
            continue;

        File.Delete(file);
    }
}