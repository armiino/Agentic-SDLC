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

// Offline-Evaluator (isolierter Bewertungs-Pfad): bewertet ein bestehendes Artefakt mit dem
// Evaluator, ohne neuen Generierungs-Run / Workflow / Run-Ordner / Change-Note
// ziel: Re-Scoren alter Runs oder zum Testen eines anderen Judge-Modells (evtl später weg..)
if (args.Length > 0 && string.Equals(args[0], "eval-offline", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.OfflineEvaluatorRunner.RunAsync(args, settings, repoRoot);
    return;
}

// DISK-14 Phase 1 (additiv, isoliert): Artefakte deterministisch in Pruefeinheiten zerlegen (kein LLM).
// Verwerfen des Per-Item-Ansatzes = PerItem-Ordner loeschen + diese Zeile entfernen.
if (args.Length > 0 && string.Equals(args[0], "parse-units", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.UnitParseRunner.RunAsync(args, repoRoot);
    return;
}

// DISK-14 Phase 2 (additiv, isoliert): Per-Item-Klassifikation → paralleler GroundingScore (echter LLM-Call)
// Verwerfen = PerItem-Ordner löschen + diese Zeile entfernen.
if (args.Length > 0 && string.Equals(args[0], "classify-units", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ClassifyUnitsRunner.RunAsync(args, settings, repoRoot);
    return;
}

// DISK-14 MISSING/Coverage-Achse (additiv, isoliert): Transkript-Turns -> covered/missing je Artefakt
if (args.Length > 0 && string.Equals(args[0], "coverage-units", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.CoverageRunner.RunAsync(args, settings, repoRoot);
    return;
}

// PILOTtest (isoliert): echter MAF-Agent als Reviewer (vs. post-hoc IEvaluator). Verwerfen = ReviewAgent-Ordner + diese Zeile.
if (args.Length > 0 && string.Equals(args[0], "review-agent", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.ReviewAgent.ReviewAgentRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Z4.2-light (additiv, isoliert, kein LLM): achsen-spezifische *.review.json eines Runs zu EINEM mergen.
if (args.Length > 0 && string.Equals(args[0], "merge-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.Review.ReviewMergeRunner.RunAsync(args, repoRoot);
    return;
}

var runId = RunId.New();
// Output-Ordner unter runs/. Logisch bleibt es Phase 2.1 (AgentPhase); die Strategie-Varianten
// bekommen aber eigene Unterordner, damit A/B/C-Runs auf der Platte getrennt liegen
var run = new RunContext(runId, ResolveRunFolder(settings));
run.EnsureFolders();
CleanDocsFolder();

// runs/<folder>/<runId>. Strategie B/C erhalten eigene Ordner.. A bleibt aus Historie-Gruenden in phase2_1.
static string ResolveRunFolder(HostSettings settings)
    => settings.AgentPhase == "phase2_1"
        ? settings.Phase2ContextStrategy switch
        {
            "artifact_state" => "phase2B",
            "independent_source_reads" => "phase2C",
            _ => "phase2_1"   // message_passing (A)
        }
        : settings.AgentPhase;

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

// Code-Stand-Stempel: gegen welchen Git-Commit lief dieser Run, war der Arbeitsbaum dirty?
// Macht run -> Code rekonstruierbar, ohne pro Run committen zu müssen (siehe commit-rules.md)
var codeVersion = AgenticSdlc.Host.Run.GitStamp.Capture(run, repoRoot);
if (codeVersion.Available)
{
    var state = codeVersion.Dirty
        ? $"DIRTY ({codeVersion.ChangedFiles} Datei(en); Diff -> {codeVersion.TrackedDiffFile})"
        : "clean";
    Console.WriteLine($"[git] {codeVersion.ShortCommit} @ {codeVersion.Branch} — {state}");
}

var config = new
{
    runId,
    phase = ResolvePhaseName(settings.AgentPhase),
    phaseSelector = settings.AgentPhase,
    // Code-Stand dieses Runs (Commit/Branch/dirty). Bei dirty liegt der Diff unter code-version/.
    codeVersion,
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
    jury = new
    {
        enabled = settings.JuryEnabled,
        judgeModel = settings.JuryJudgeModel,
        structuredOutput = settings.JuryStructuredOutput,
        // DISK-12/G3: Call-1 pro Kategorie gesplittet (Mess-Instrument-Parameter, einfrieren für Vergleiche).
        splitGeneration = settings.JurySplitGeneration,
        // DISK-12/B22: effektive aktive Kategorien je Artefakttyp (Default-Profil + run-config-Override).
        categories = new AgenticSdlc.Host.Phases.Phase2.Evaluation.JuryCategoryProfile(
            settings.JuryCategoriesByArtifact).Describe(),
        // DISK-7: aktive Verifikations-Policy pro Kategorie (für reproduzierbare A/B/C-Vergleiche).
        verification = new
        {
            falseClaim = settings.JuryVerifyFalseClaim,
            falseCertainty = settings.JuryVerifyFalseCertainty,
            missingTopic = settings.JuryVerifyMissingTopic,
            custom = settings.JuryVerifyCustom,
            // DISK-9: Batch-Limit des MISSING_TOPIC-Verifiers (Mess-Instrument-Parameter, einfrieren für Vergleiche).
            missingTopicBatchSize = settings.JuryMissingTopicBatchSize
        }
    },
    // Nur für Phase 2.1B (artifact_state) relevant: dokumentiert die aktive Shared-State-Policy.
    phase2BState = (settings.AgentPhase == "phase2_1" && settings.Phase2ContextStrategy == "artifact_state")
        ? new
        {
            writeArtifacts = settings.Phase2BWriteArtifacts,
            reads = settings.Phase2BReads
        }
        : null,
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
