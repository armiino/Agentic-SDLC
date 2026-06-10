using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase1;
using AgenticSdlc.Host.Phases.Phase2;
using AgenticSdlc.Host.Run;
using System.Diagnostics;

var repoRoot = FindRepoRoot();

DotNetEnv.Env.Load(
    Path.Combine(repoRoot, ".env"),
    new DotNetEnv.LoadOptions(
        setEnvVars: true,
        clobberExistingVars: true,
        onlyExactPath: true
    )
);

var runtimeConfig = RunConfig.Load(repoRoot);
var settings = HostSettings.FromRuntimeConfig(runtimeConfig, repoRoot);
var runId = RunId.New();
var run = new RunContext(runId, settings.AgentPhase);
run.EnsureFolders();
CleanDocsFolder();

var sourceName = "AgenticSdlc.Host";
var activitySource = new ActivitySource(sourceName);
var tracesPath = Path.Combine(run.LogsDir, "otel-traces.jsonl");
var rawTracesPath = Path.Combine(run.LogsDir, "otel-traces.raw.jsonl");
var metricsPath = Path.Combine(run.LogsDir, "otel-metrics.jsonl");

// Providers leben bis Run-Ende:
using var otel = AgenticSdlc.Host.Observability.OtelRunExporters.TryCreate(
    enabled: settings.OtelEnabled,
    sourceName: sourceName,
    tracesPath: tracesPath,
    metricsPath: metricsPath,
    rawTracesPath: settings.OtelRawEnabled ? rawTracesPath : null
);

var config = new
{
    runId,
    phase = ResolvePhaseName(settings.AgentPhase),
    phaseSelector = settings.AgentPhase,
    phase2ContextStrategy = settings.AgentPhase == "phase2_1" ? settings.Phase2ContextStrategy : null,
    prompts = ResolvePromptConfig(settings),
    llmProvider = settings.LlmProvider,
    model = settings.ModelId,
    observability = new
    {
        otelEnabled = settings.OtelEnabled,
        otelSensitive = settings.OtelSensitive,
        otelRawEnabled = settings.OtelRawEnabled,
        innerCycleLogging = settings.InnerCycleLogging
    },
    llmPreview = new
    {
        chars = settings.LlmPreviewChars,
    },
    ollamaBaseUrl = settings.OllamaBaseUrl,
    openRouterBaseUrl = settings.LlmProvider == "openrouter" ? settings.OpenRouterBaseUrl : null,
    timestampUtc = DateTime.UtcNow
};
run.WriteConfig(config);

WriteRunChangeNote(run);

//unter changes.txt immer nachvollziehbar warum der Run gestartet wurde "welche neuerungen"
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

    // Speichern unter runs/<phase>/<runId>/logs/changes.txt
    File.WriteAllText(run.ChangesPath, content);

    //auch als Event (damit es in events.jsonl auffindbar ist)
    run.AppendEvent(new { type = "RUN_CHANGE_NOTE", runId = run.RunId, note, timestampUtc = DateTime.UtcNow });
}

Environment.ExitCode = settings.AgentPhase switch
{
    "phase1" => await RunPhase1Async(),
    "phase2_1" => await RunPhase2_1Async(),
    _ => UnknownPhase(settings.AgentPhase, run)
};

async Task<int> RunPhase1Async()
{
    var runner = new Phase1Runner(
        settings: settings,
        run: run,
        sourceName: sourceName,
        activitySource: activitySource,
        repoRoot: repoRoot
    );

    return await runner.RunAsync();
}

async Task<int> RunPhase2_1Async()
{
    var runner = new Phase2Runner(
        settings: settings,
        run: run,
        sourceName: sourceName,
        activitySource: activitySource,
        repoRoot: repoRoot
    );

    return await runner.RunAsync();
}

static int UnknownPhase(string phase, RunContext run)
{
    run.AppendEvent(new
    {
        type = "RUN_FAILED",
        runId = run.RunId,
        reason = "Unknown AGENT_PHASE.",
        phase,
        allowedPhases = new[] { "phase1", "phase2_1" },
        timestampUtc = DateTime.UtcNow
    });

    Console.Error.WriteLine($"RUN FAILED - Unknown AGENT_PHASE '{phase}'. Allowed values: phase1, phase2_1.");
    return 4;
}

static string ResolvePhaseName(string phase)
    => phase switch
    {
        "phase1" => Phase1Artifacts.PhaseName,
        "phase2_1" => Phase2Artifacts.PhaseName,
        _ => phase
    };

static object ResolvePromptConfig(HostSettings settings)
{
    if (settings.AgentPhase == "phase1")
    {
        return new
        {
            phase1 = new
            {
                agent = "Phase1SinglePass",
                promptName = settings.GetPromptName("Phase1SinglePass")
            }
        };
    }

    if (settings.AgentPhase == "phase2_1")
    {
        return new
        {
            phase2_1 = new
            {
                contextStrategy = settings.Phase2ContextStrategy,
                agents = new
                {
                    context = settings.GetPromptName(Phase2AgentFactory.ContextAgentName),
                    requirements = settings.GetPromptName(Phase2AgentFactory.RequirementsAgentName),
                    risks = settings.GetPromptName(Phase2AgentFactory.RisksAgentName),
                    architecture = settings.GetPromptName(Phase2AgentFactory.ArchitectureAgentName),
                    openQuestions = settings.GetPromptName(Phase2AgentFactory.OpenQuestionsAgentName)
                }
            }
        };
    }

    return new { unknownPhase = settings.AgentPhase };
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

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);

    for (int i = 0; i < 10 && dir is not null; i++)
    {
        var hasGit = Directory.Exists(Path.Combine(dir.FullName, ".git"));
        var hasSlnx = File.Exists(Path.Combine(dir.FullName, "Agentic-SDLC.slnx"));
        var hasSln = File.Exists(Path.Combine(dir.FullName, "Agentic-SDLC.sln"));

        if (hasGit || hasSlnx || hasSln)
            return dir.FullName;

        dir = dir.Parent;
    }

    return Directory.GetCurrentDirectory();
}
