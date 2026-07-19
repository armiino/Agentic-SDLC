using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// CLI: github-forward <pbi-update-run> [model] [--issues <snapshot.json>] [--repo owner/name] [--dry-run]
// Forward-Maker (plan-tor3 §4): deterministischer Vorfilter (Seed) + agentischer Rest (Suche -> LINK/CREATE),
// zusammen zu einem GithubForwardPlan -> Gate (inkl. Rev-3-Invariante) -> Review (T3.3) -> gated Apply (T3.4).
public static class GithubForwardRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "GithubForwardAgent";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }
        var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
        string? token = null, modelArg = null, issuesArg = null, repoArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--issues", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { issuesArg = args[++i]; continue; }
            if (string.Equals(a, "--repo", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { repoArg = args[++i]; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[github-forward] unbekanntes Argument: {a}"); return 2; }
            if (token is null) token = a; else modelArg ??= a;
        }
        if (token is null) { Usage(); return 2; }

        // 1) github-sync-Delta des pbi-update-Laufs.
        var planDir = PbiUpdateReviewRunner.ResolvePlanDir(repoRoot, token);
        var deltaPath = planDir is null ? null : Path.Combine(planDir, "applied", "github-sync-delta.json");
        if (deltaPath is null || !File.Exists(deltaPath))
        {
            Console.Error.WriteLine($"[github-forward] github-sync-delta.json fuer '{token}' nicht gefunden - erst pbi-update-apply fahren.");
            return 2;
        }
        var delta = await LoadAsync<GithubSyncDeltaDocument>(deltaPath).ConfigureAwait(false);
        var sourcePbiUpdateRun = Path.GetFileName(Path.GetDirectoryName(planDir!) ?? planDir!);

        // 2) Core-Mappings (Dedup-Basis, T3.1).
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[github-forward] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);
        var mappingByPbi = CoreGithubMapping.ByPbi(core);

        // 3) Issue-Snapshot (T3.2): explizit oder juengster github-snapshot; sonst leer (dann keine Kandidaten).
        var snapshotPath = ResolveSnapshotPath(repoRoot, issuesArg);
        IReadOnlyList<GithubIssueSnapshot> issues = [];
        if (snapshotPath is not null) issues = await GithubReadSource.LoadAsync(snapshotPath).ConfigureAwait(false);
        else Console.WriteLine("[github-forward] WARN: kein Issue-Snapshot — unmapped PBIs koennen nur CREATE (keine LINK-Kandidaten).");

        // 4) Deterministischer Vorfilter.
        var seed = GithubForwardSeed.Seed(delta.Entries, mappingByPbi, issues);

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "github-forward");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        // 5) Agentischer Rest: je unmapped (nicht blockiertem) PBI -> Suche -> LINK/CREATE.
        var agentOps = new List<GithubForwardOp>();
        if (seed.UnmappedPbis.Count > 0 && !dryRun)
        {
            using var otel = OtelRunExporters.TryCreate(settings.OtelEnabled, SourceName,
                Path.Combine(run.LogsDir, "otel-traces.jsonl"), Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
                settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);
            var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "GithubForwardAgent1", new Dictionary<string, string> { ["runId"] = run.RunId });
            var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
            var readTools = new GithubReadTools(issues, repoArg, run);
            var fwdTools = new GithubForwardTools(seed.UnmappedPbis, core, issues, run);
            var agent = client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. readTools.Build(), .. fwdTools.Build()])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            var task = """
                       Fuer JEDES PBI aus get_unmapped_pbis GENAU EIN Op:
                       1. get_pbi(pbiId) lesen (Titel + abgedeckte Requirement-Texte).
                       2. search_issues MIT diesen Begriffen ausfuehren (Anti-Duplikat, Pflicht vor CREATE).
                       3. Plausibler Kandidat (auch menschlich angelegt)? -> LINK (targetIssueNumber + anchor).
                          Sonst -> CREATE_ISSUE mit title, body, searchedQueries (was du gesucht hast) und
                          searchEvidence ("keine plausiblen Treffer ..."). OHNE diese Felder lehnt das Gate ab.
                       check_forward_plan zur Selbstpruefung, dann save_forward_plan GENAU EINMAL.
                       """;
            try { await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: CancellationToken.None).ConfigureAwait(false); }
            catch (Exception ex) { Console.Error.WriteLine($"[github-forward] Forward-Agent fehlgeschlagen: {ex.Message}"); return 4; }
            agentOps = fwdTools.SavedOps.ToList();
        }

        // 6) Plan + Gate.
        var operations = seed.DeterministicOps.Concat(agentOps).ToList();
        var plan = new GithubForwardPlanDocument(
            GithubForwardPlanDocument.CurrentSchemaVersion, $"github-forward-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            DateTime.UtcNow, sourcePbiUpdateRun, repoArg, operations);
        var gate = GithubForwardGate.Check(plan, delta.Entries, issues);

        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-forward-plan.json"), JsonSerializer.Serialize(plan, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-forward-gate-report.json"), JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-forward-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            sourcePbiUpdateRun,
            deltaPbis = delta.Entries.Count,
            deterministic = seed.DeterministicOps.Count,
            unmapped = seed.UnmappedPbis.Count,
            agentOps = agentOps.Count,
            operations = operations.Count,
            snapshot = snapshotPath is null ? null : Path.GetRelativePath(repoRoot, snapshotPath),
            gatePass = gate.Pass,
            gateErrors = gate.Errors.Count,
            byKind = operations.GroupBy(o => o.Kind, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal),
            dryRun,
            timestampUtc = DateTime.UtcNow
        }, Json)).ConfigureAwait(false);

        Console.WriteLine($"[github-forward] deltaPbis={delta.Entries.Count} deterministic={seed.DeterministicOps.Count} unmapped={seed.UnmappedPbis.Count} agentOps={agentOps.Count} ops={operations.Count} gate={(gate.Pass ? "pass" : "fail")} errors={gate.Errors.Count}");
        Console.WriteLine($"[github-forward] byKind: {string.Join(", ", plan.Operations.GroupBy(o => o.Kind, StringComparer.Ordinal).Select(g => $"{g.Key}={g.Count()}"))}");
        foreach (var e in gate.Errors) Console.WriteLine($"[github-forward]   ERROR {e.Code} {e.PbiId}: {e.Message}");
        Console.WriteLine($"[github-forward] -> {Path.GetRelativePath(repoRoot, outDir)} (danach: github-forward-review)");
        return gate.Pass ? 0 : 1;
    }

    // Juengsten github-issues-snapshot.json unter runs/github-snapshot/ finden (falls kein --issues gegeben).
    private static string? ResolveSnapshotPath(string repoRoot, string? issuesArg)
    {
        if (!string.IsNullOrWhiteSpace(issuesArg))
            return Path.IsPathRooted(issuesArg) ? issuesArg : Path.Combine(repoRoot, issuesArg);
        var root = Path.Combine(repoRoot, "runs", "github-snapshot");
        if (!Directory.Exists(root)) return null;
        return Directory.EnumerateFiles(root, "github-issues-snapshot.json", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    private static void Usage()
        => Console.Error.WriteLine("Usage: github-forward <pbi-update-run> [modelId] [--issues <snapshot.json>] [--repo owner/name] [--dry-run]");
}
