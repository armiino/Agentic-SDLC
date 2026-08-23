using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Observability;
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
if (args.Length > 0)
    args[0] = args[0].Trim();

// ============================================================================================================
// CLI-KOMMANDO-DISPATCH — LANDKARTE (aktualisiert Move 3, 2026-07-22).
// Die frueheren Evaluation-/Phase-Kommandos (eval-offline, parse-units, review*, topic*, claim*, source-claim*,
// evidence-first-spike, human-artifact*, semantic-ledger-extract*, review + AGENT_PHASE phase1/phase2_1) sind
// ARCHIVIERT (archive/, ausserhalb des Builds) — lauffaehig ueber die Stations-Tags:
//   git checkout v-s0-phase1 | v-s1-phase2-dag | v-s3-evaluator-review
//
// [LIVE] aktiver Strang (lebender Core + Tor 1/2/3), inkl. der neuen MAF-nativen Human-Gates (*-hitl):
//   Core:   core-bootstrap-first-transcript, core-seed, core-baseline, core-seed-backlog, core-view, project-state-build
//   Tor 1:  ingest-requirements, ingest-review, ingest-apply, ingest-requirements-hitl
//   Place:  pbi-update, pbi-update-review, pbi-update-apply, pbi-update-hitl
//   Tor 2:  decision-resolve, decision-resolve-agent, decision-review, decision-apply, decision-resolve-hitl, decision-unblock-test
//   Tor 3:  github-map, github-read, github-snapshot, github-write, github-forward, github-forward-hitl,
//           github-forward-review, github-forward-apply, github-forward-compare, github-forward-rerun-test,
//           github-reverse, github-reverse-review, github-reverse-apply
//
// [SUPPORT] L4 / Backlog (re-clarify = der lebende Backlog-Bau; issuplanning = LIVE/Bridge):
//   l4-re-clarify(-review|-apply|-backlog-review|-backlog-apply|-issueplan|-backlog-doc),
//   l4-issuplanning, l4-issuplanning-review, l4-issuplanning-apply
//   (Alt-L4-Kette 04.08. archiviert -> archive/phase2-l4-dormant/ + Thesis-Docs/aktiv/backlog-genealogie.md)
//
// [FRONT] Produkt-Front (Ledger-Pfad, intendierter Transkript->MeetingDelta-Weg — s. lokale PRODUCT-CAPABILITY-MAP):
//   Ledger:           ledger-build, ledger-build-units, ledger-reference-template, ledger-adjudicate,
//                     ledger-adjudicate-apply, ledger-adjudicate-ui, ledger-adjudicate-refine, ledger-validate,
//                     facet-validation-eval, ledger-reference-recall, ledger-reference-recall-fast, ledger-cite-fidelity
//   Front-Mitte:      recipe, baseline-fanout, artifact-branch, assign-artifact-ids (consumable -> Baselines)
//   L3 (dormant):     l3, l3-apply, l3-revise, l3-review
//
// [CAPSTONE/RESEARCH — Kommandos noch aufrufbar, Code in Place bzw. research/]:
//   Contract/Derivation: contract-check, contract-critic, contract-repair, evidence-chain, derive, derive-metrics,
//                        checker-repair-workflow, inference-check, derive-risks, derive-review
//   Tor-3-Vorlaeufer (research/, abgeloest durch github-forward): github-reconciliation(-review|-apply), github-write
//   Verifikations-Spike (S0/S-9): spike-hitl
// ============================================================================================================

// ── Kommando-Dispatch (R1, 2026-07-22): Kommandos registrieren sich je Kettenglied (*Commands.cs).
// Verhalten unveraendert (case-insensitive Lookup statt if-Kette). Unbekannte Kommandos: s.u.
var commands = new Dictionary<string, AgenticSdlc.Host.CommandHandler>(StringComparer.OrdinalIgnoreCase);
AgenticSdlc.Host.FullWorkflow.Ledger.LedgerCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.BaselineCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.Gap.GapCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.Delta.DeltaCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.Core.CoreCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.Backlog.BacklogCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.Core.IngestionCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiUpdateCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.Decision.DecisionCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineCommands.Register(commands);
AgenticSdlc.Host.FullWorkflow.Analyst.AnalystCommands.Register(commands);   // 1g-B: Core-Analyst (core-analysis run)
AgenticSdlc.Host.Steward.StewardCommands.Register(commands);   // C1b: die Steward-Schicht (steward [--session] [--once])
AgenticSdlc.Host.Mcp.GithubMcpProbeRunner.Register(commands);  // C2d ②: MCP-Spike-Probe (LLM-frei, readonly-Wache)
AgenticSdlc.Host.FullWorkflow.HitlSpike.SpikeCommands.Register(commands);
// research/ 2026-07-23 nach archive/ (Typ-Schnitt erledigt) — Kommandos deregistriert.
if (args.Length > 0 && commands.TryGetValue(args[0], out var commandHandler))
{
    Environment.ExitCode = await commandHandler(args, settings, repoRoot);
    return;
}

if (args.Length > 0)
{
    Console.Error.WriteLine($"Unknown command '{args[0]}'. Der normale Phase-Runner startet nur ohne CLI-Command.");
    // Gruppiert (S5, Worklist 20.07) — vollstaendige Landkarte s. Banner am Dispatch-Anfang. Alle bleiben aufrufbar.
    Console.Error.WriteLine("[LIVE] core-seed, core-view, project-state-build, ingest-requirements(-hitl), ingest-review, ingest-apply, "
        + "pbi-update(-hitl), pbi-update-review, pbi-update-apply, decision-resolve(-agent|-hitl), decision-review, decision-apply, "
        + "github-forward(-hitl), github-forward-review, github-forward-apply, github-reverse(-review|-apply), github-snapshot, github-read.");
    Console.Error.WriteLine("[SUPPORT/L4] l4-re-clarify(-review|-apply|-backlog-review|-backlog-apply|-issueplan|-backlog-doc), "
        + "l4-issuplanning(-review|-apply). (Alt-L4-Kette archiviert: archive/phase2-l4-dormant/)");
    Console.Error.WriteLine("[FROZEN/Historie — nur Referenz] l3(-review|-apply|-revise), ledger-build, ledger-adjudicate-ui, ledger-validate, "
        + "semantic-ledger-extract, eval-offline, review, review-agent, contract-check, recipe, derive, evidence-chain, "
        + "github-reconciliation(-review|-apply), spike-hitl (u.a. — s. Banner).");
    Environment.ExitCode = 2;
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
    => settings.AgentPhase switch
    {
        // phase2_1/2B/2C-Ordnerwahl archiviert (Move 3, 2026-07-22).
        // Kapitel B (Evidenz-Agent): runs/phase2evidenz-agent/<arm>/<runId> — Arme (ledger|transcript) getrennt.
        "phase2_evidence" => $"phase2evidenz-agent/{settings.EvidenceSource}",
        _ => settings.AgentPhase
    };

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
    // jury-Config archiviert mit Evaluation (Move 3, 2026-07-22) — Jury war S-3-Messinstrument.
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

// "phase1" / "phase2_1" sind archiviert (archive/phase1, archive/phase2-dag, archive/phase2b — Move 3, 2026-07-22).
// Lauffaehige Historie: git checkout v-s0-phase1 / v-s1-phase2-dag / v-s3-evaluator-review.
Environment.ExitCode = settings.AgentPhase switch
{
    "phase2_evidence" => await RunPhase2EvidenceAsync(),
    _ => UnknownPhase(settings.AgentPhase, run)
};

// Kapitel B (Evidenz-Agent): eigener, DÜNNER Runner; komponiert die vorhandenen Bausteine
// (Phase2AgentFactory / Pipeline), lässt Phase2Runner unangetastet. E0 = Gerüst-Durchstich.
async Task<int> RunPhase2EvidenceAsync()
{
    var runner = new AgenticSdlc.Host.FullWorkflow.EvidenceAgentRunner(
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
        allowedPhases = new[] { "phase2_evidence" },
        timestampUtc = DateTime.UtcNow
    });

    Console.Error.WriteLine($"RUN FAILED - Unknown AGENT_PHASE '{phase}'. Allowed: phase2_evidence. (phase1/phase2_1 archiviert -> Tags v-s0/v-s1/v-s3.)");
    return 4;
}

static string ResolvePhaseName(string phase)
    => phase; // phase1/phase2_1-Namensaufloesung archiviert (Move 3); einzig verbliebene Phase: phase2_evidence.

static object ResolvePromptConfig(HostSettings settings)
{
    // phase1/phase2_1-Prompt-Konfig archiviert (Move 3). Verhalten fuer phase2_evidence unveraendert (wie vorher Fallback).
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
