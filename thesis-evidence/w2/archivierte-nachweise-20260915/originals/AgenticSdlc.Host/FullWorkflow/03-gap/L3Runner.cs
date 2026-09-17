using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.Derivation;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

/// <summary>
/// L3 Open-World-Ableitung (CLI: <c>l3 &lt;env1.artifact.json&gt; [env2 …] [model] [--dry-run]</c>). Fährt den
/// Prepare-Workflow (§5.3, Workflow 1): aus dem Umwelt-Graphen neue Kandidaten generieren, verankern, in 4 Klassen
/// routen und ein Human-Review-Paket erzeugen. Runs unter <c>runs/l3/&lt;runId&gt;/</c>. Exit: 0 = ok/dry-run,
/// 2 = Usage/IO, 4 = LLM-Fehler.
/// </summary>
/// <remarks>
/// Zwei echte Agenten (Generierung + Anker-Resolution, getrennt = §3), ein wiederverwendeter Judge-Kern
/// (<see cref="InferenceChecker"/> mit L3-Maßstab) und deterministische Executors (Validierung, Routing, Finalize).
/// Logger-Pipeline + ToolCallLogger wie in der Derivation-Familie. Der Human-Loop (Apply, accept/edit/reject) ist die
/// separate Phase 2 und noch nicht Teil von v1.
/// </remarks>
public static class L3Runner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "L3OpenWorldAgent";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l3 <env1.artifact.json> [env2.artifact.json …] [model] [--agentic-coverage|--coverage|--coverage-measure|--coverage-repair [--repair-rounds N]|--research|--exhaustive|--selective] [--provenance|--force-provenance] [--graph <file> …] [--candidates <file>] [--dry-run]");
            return 2;
        }
        var dryRun = args.Contains("--dry-run");
        // Kandidaten-Modus: config l3.candidateMode; CLI --exhaustive/--selective überschreibt.
        // research = L3-Über-Agent (Recherche-first: großes Bild erfassen, erweitern UND Lücken finden; deklariert
        // intent extension|gap + basedOn). Braucht das Provenance-Tool → impliziert es (s. provenanceOn).
        // coverage = L3-Über-Agent als Coverage/Gap-Analyst (v10): Prüflinsen über den ganzen Graphen, deklariert je
        // Kandidat gapCategory + impactIfMissing + requiresHumanDecision (additiv zu intent/basedOn). Provenance nicht
        // erzwungen (konsistent mit dem Kohärenz-Learning) → das Tool ist an, aber "wenn nötig".
        var candidateMode = args.Contains("--agentic-coverage") ? "agentic-coverage"
            : args.Contains("--coverage") || args.Contains("--coverage-measure") || args.Contains("--coverage-repair") ? "coverage"
            : args.Contains("--research") ? "research"
            : args.Contains("--exhaustive") ? "exhaustive"
            : args.Contains("--selective") ? "selective"
            : settings.L3CandidateMode;
        // Coverage-Variante (Schritt 4) — Punkte auf der Priming/Repair-Achse: strong = sichtbarer Voll-Katalog (v2,
        // Einzelpass, primed) · measure = minimal-prime (nur Label-Vokabular, Einzelpass) · repair = measure + gebundener
        // Repair-Loop auf leere Linsen. measure/repair laufen über L3CoverageGenExecutor; strong bleibt der Einzelpass-Baseline.
        var coverageVariant = candidateMode == "agentic-coverage" ? "agentic"
            : candidateMode != "coverage" ? null
            : args.Contains("--coverage-repair") ? "repair"
            : args.Contains("--coverage-measure") ? "measure" : "strong";
        // resolve_provenance-Tool: config l3.provenanceTool; CLI --provenance überschreibt. Default aus (baseline-neutral).
        // --force-provenance = eigener Messpunkt (b): erzwingt das Tool UND wählt Prompt-Varianten, die den Agenten
        // explizit anweisen, resolve_provenance zu nutzen + das Ziel auf gut-verankerte Elaboration schärfen (additiv,
        // die neutralen Prompts bleiben die "angeboten≠genutzt"-Baseline).
        var provenanceForce = args.Contains("--force-provenance");
        var provenanceOn = provenanceForce || candidateMode == "research" || candidateMode == "coverage" || candidateMode == "agentic-coverage" || args.Contains("--provenance") || settings.L3ProvenanceTool;
        // Coverage-Modus: der geteilte Abdeckungs-Rahmen (Linsen). EINE Quelle für Generator-Prompt (RenderGeneratorBlock
        // → {{coverageLenses}}) UND — Schritt 3 — den LensCoverageGate. Nur im coverage-Modus gebaut.
        var coverageSpec = candidateMode is "coverage" or "agentic-coverage" ? CoverageRegistry.Default : null;

        // --candidates <file>: kontrollierte Test-Kandidaten (Plan §11.1) — überspringt Generierung + Resolution und
        // fährt nur die deterministische Klassifikation + den Judge (alle 4 Klassen gezielt provozierbar).
        string? candidatesArg = null;
        string? repairRoundsArg = null;
        var skipValue = new HashSet<int>();
        // --graph <file> (wiederholbar): "DB-Scheibe" — diese Artefakte gehen NUR in die resolve_provenance-Tool-Basis,
        // NICHT in den Generierungs-Prompt. Damit ist Tool-Backing ⊋ Prompt-Env: das Tool kann Upstream-TEXT liefern,
        // den der Agent im Prompt nicht sieht (confound-freier Nutzungstest; simuliert die spätere DB-Umwelt).
        var graphArgs = new List<string>();
        for (var i = 1; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], "--candidates", StringComparison.Ordinal)) { candidatesArg = args[i + 1]; skipValue.Add(i + 1); }
            else if (string.Equals(args[i], "--graph", StringComparison.Ordinal)) { graphArgs.Add(args[i + 1]); skipValue.Add(i + 1); }
            else if (string.Equals(args[i], "--repair-rounds", StringComparison.Ordinal)) { repairRoundsArg = args[i + 1]; skipValue.Add(i + 1); }
        }
        // Repair-Runden: nur in der repair-Variante wirksam; Default 2. measure/strong ⇒ 0 (kein Loop).
        var repairRounds = coverageVariant == "repair" ? (int.TryParse(repairRoundsArg, out var rr) && rr > 0 ? rr : 2) : 0;

        // Positional nach dem Befehl: existierende Dateien = Umwelt-Artefakte (1..N); erstes Nicht-File/Nicht-Flag = Modell.
        var sourcePaths = new List<string>();
        string? modelArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            if (skipValue.Contains(i)) continue;
            var a = args[i];
            if (a.StartsWith("--", StringComparison.Ordinal)) continue;
            var resolved = Resolve(repoRoot, a);
            if (resolved is not null && File.Exists(resolved)) sourcePaths.Add(resolved);
            else if (modelArg is null) modelArg = a;
        }
        var injectPath = candidatesArg is not null ? Resolve(repoRoot, candidatesArg) : null;
        var inject = injectPath is not null;
        if (inject && !File.Exists(injectPath!)) { Console.Error.WriteLine($"[l3] --candidates: Datei fehlt: '{candidatesArg}'."); return 2; }

        var sources = new List<ArtifactDocument>();
        var seenTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sp in sourcePaths)
        {
            var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(sp).ConfigureAwait(false), Json);
            if (doc is null || doc.Items.Count == 0) { Console.Error.WriteLine($"[l3] Umwelt-Artefakt leer/nicht lesbar: {Path.GetRelativePath(repoRoot, sp)}"); return 2; }
            if (seenTypes.Add(doc.ArtifactType)) sources.Add(doc);
        }
        if (sources.Count == 0) { Console.Error.WriteLine("[l3] keine Umwelt-Artefakte gefunden (erwartet 1..N *.artifact.json positional)."); return 2; }
        var env = new SourceArtifactSet(sources);

        // DB-Scheibe: --graph-Artefakte laden und mit der Prompt-Env zu EINER breiteren Tool-Basis vereinen (Prompt-Env
        // gewinnt bei Typ-Kollision). Der Prompt (Generierung/Anker/Routing) bleibt strikt auf `env`; nur das
        // resolve_provenance-Tool sieht `toolEnv`. Ohne --graph ist toolEnv == env (altes Verhalten).
        var toolSources = new List<ArtifactDocument>(sources);
        var toolSeen = new HashSet<string>(seenTypes, StringComparer.OrdinalIgnoreCase);
        var graphTypes = new List<string>();
        foreach (var ga in graphArgs)
        {
            var gp = Resolve(repoRoot, ga);
            if (gp is null || !File.Exists(gp)) { Console.Error.WriteLine($"[l3] --graph: Datei fehlt: '{ga}'."); return 2; }
            var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(gp).ConfigureAwait(false), Json);
            if (doc is null || doc.Items.Count == 0) { Console.Error.WriteLine($"[l3] --graph-Artefakt leer/nicht lesbar: {Path.GetRelativePath(repoRoot, gp)}"); return 2; }
            if (toolSeen.Add(doc.ArtifactType)) { toolSources.Add(doc); graphTypes.Add(doc.ArtifactType); }
        }
        var toolEnv = graphTypes.Count > 0 ? new SourceArtifactSet(toolSources) : env;

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var judgeSettings = !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! } : settings;

        var run = new RunContext(RunId.New(), "l3");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = L3Workflow.WorkflowName, runId = run.RunId,
            env = sourcePaths.Select(p => Path.GetRelativePath(repoRoot, p)).ToArray(), envTypes = sources.Select(s => s.ArtifactType).ToArray(),
            envItems = env.TotalItemCount, provider = settings.LlmProvider, generatorModel = genSettings.ModelId, judgeModel = judgeSettings.ModelId,
            mode = inject ? "from-candidates" : "generate", candidateMode, coverageVariant, coverageRepairRounds = coverageVariant == "repair" ? repairRounds : (int?)null,
            coverageSpec = coverageSpec?.Id, coverageLensCount = coverageSpec?.Lenses.Count,
            provenanceTool = provenanceOn && !inject, provenanceForce = provenanceForce && !inject,
            toolGraphTypes = graphTypes.Count > 0 ? graphTypes.ToArray() : null, toolItems = toolEnv.TotalItemCount,
            candidates = inject ? Path.GetRelativePath(repoRoot, injectPath!) : null,
            timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[l3] runId={run.RunId}  umwelt=[{string.Join(",", sources.Select(s => s.ArtifactType))}] items={env.TotalItemCount}  genModel={genSettings.ModelId} judgeModel={judgeSettings.ModelId}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Judge (immer) — wiederverwendeter InferenceChecker-Kern mit L3-Maßstab, über die Standard-Pipeline.
        var judgeClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, run, "L3-SupportJudge", SourceName);
        var judgePrompt = PromptProvider.Load(repoRoot, Phase, AgentName, "L3SupportJudge1", new Dictionary<string, string>());
        var judge = new InferenceChecker(judgeClient, settings.JuryStructuredOutput, systemPrompt: judgePrompt, reasoning: settings.ReasoningCapture);

        Microsoft.Agents.AI.Workflows.Workflow workflow;
        L3Resolved? injected = null;
        if (inject)
        {
            injected = new L3Resolved(env, await LoadTestCandidatesAsync(injectPath!).ConfigureAwait(false));
            workflow = L3Workflow.BuildFromResolved(new L3AnchorValidateExecutor(run), new L3SupportJudgeExecutor(judge, run), new L3RoutingExecutor(run), new L3FinalizeExecutor(run));
            Console.WriteLine($"[l3] mode=from-candidates: {injected.Items.Count} kontrollierte Test-Kandidaten (Generierung + Resolution übersprungen).");
        }
        else
        {
            // Zwei Agenten (Generierung + Anker-Resolution getrennt) — beide über Standard-Pipeline + ToolCallLogger.
            var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, "L3-CandidateGen", SourceName);
            var resolveClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, "L3-AnchorResolve", SourceName);
            // selective = fokussierte Handvoll; exhaustive = ergiebige verankerte Elaboration je Umwelt-Item.
            // research = Über-Agent (Recherche-first, erweitern+Lücken, intent/basedOn); --force-provenance = Messpunkt b.
            // Im research-Modus bleibt der Resolver neutral: bei kohärentem Env (alle Artefakte positional) ist der ganze
            // Graph im Prompt sichtbar → der Resolver braucht das Tool nicht (nur der Generator recherchiert in die Tiefe).
            // coverage: strong = v2 (spec-getriebener Voll-Katalog via {{coverageLenses}}, Einzelpass);
            // measure/repair = Measure1 (minimal-prime, nur Label-Vokabular via {{coverageLabels}}). v1 bleibt eingefroren.
            var genPromptName = candidateMode == "agentic-coverage" ? "L3CandidateGenCoverageAgentic1"
                : candidateMode == "coverage"
                    ? (coverageVariant == "strong" ? "L3CandidateGenCoverage2" : "L3CandidateGenCoverageMeasure2")
                : candidateMode == "research" ? "L3CandidateGenResearch1"
                : provenanceForce ? "L3CandidateGenExhaustiveProv1"
                : candidateMode == "exhaustive" ? "L3CandidateGenExhaustive1" : "L3CandidateGen1";
            var resolvePromptName = provenanceForce ? "L3AnchorResolveProv1" : "L3AnchorResolve1";
            Console.WriteLine($"[l3] candidateMode={(provenanceForce ? "exhaustive+force-provenance" : candidateMode)} (Prompts {genPromptName} / {resolvePromptName})"
                + (coverageSpec is not null ? $"  coverageSpec={coverageSpec.Id} ({coverageSpec.Lenses.Count} Linsen) variant={coverageVariant}{(repairRounds > 0 ? $" repairRounds={repairRounds}" : "")}" : ""));
            var genVars = new Dictionary<string, string> { ["runId"] = run.RunId };
            if (coverageSpec is not null)
            {
                genVars["coverageLenses"] = coverageSpec.RenderGeneratorBlock();
                genVars["coverageLabels"] = coverageSpec.RenderLabelVocabulary();
            }
            var genPrompt = PromptProvider.Load(repoRoot, Phase, AgentName, genPromptName, genVars);
            var resolvePrompt = PromptProvider.Load(repoRoot, Phase, AgentName, resolvePromptName, new Dictionary<string, string> { ["runId"] = run.RunId });
            // resolve_provenance (optional, config-gated): erlaubt Gen/Resolve die Herkunft eines Umwelt-Items
            // rückzuverfolgen. Gegen SourceArtifactSet gebaut → DB-migrationssicher. Default aus = tools: [] (baseline).
            IReadOnlyList<AITool> tools = [];
            IReadOnlyDictionary<string, LedgerClaim> claims = new Dictionary<string, LedgerClaim>();
            if (provenanceOn)
            {
                var ledgerPath = settings.L3LedgerRun is not null ? Resolve(repoRoot, settings.L3LedgerRun) : null;
                claims = LedgerClaimIndex.LoadOrEmpty(ledgerPath);
                tools = new L3ProvenanceTool(toolEnv, claims, run).Build();
                var graphNote = graphTypes.Count > 0 ? $"; DB-Scheibe: Tool-Basis={toolEnv.TotalItemCount} items (+[{string.Join(",", graphTypes)}]) ⊋ Prompt-Env={env.TotalItemCount}" : "";
                Console.WriteLine($"[l3] provenanceTool=on (resolve_provenance an Gen+Resolve; ledgerClaims={claims.Count}{(ledgerPath is null ? ", kein Ledger-Pfad" : "")}{graphNote})");
            }
            AIAgent resolveAgent = resolveClient.AsAIAgent(instructions: resolvePrompt, name: AgentName, tools: [.. tools]);
            resolveAgent = resolveAgent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            // measure/repair: Start-Knoten = L3CoverageGenExecutor (knoten-interner Repair-Loop; measure ⇒ 0 Runden).
            // strong + alle Nicht-coverage-Modi: der Einzelpass-L3CandidateGenExecutor (unverändert = Baseline).
            var useCoverageLoop = coverageVariant is "measure" or "repair";
            if (candidateMode == "agentic-coverage")
            {
                AIAgent AgentFactory(IReadOnlyList<AITool> agentTools)
                {
                    var agent = genClient.AsAIAgent(instructions: genPrompt, name: AgentName, tools: [.. agentTools]);
                    return agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
                }

                workflow = L3Workflow.Build(
                    new L3AgenticCoverageExecutor(AgentFactory, coverageSpec!, run, genSettings.ModelId, claims, toolEnv),
                    new L3AnchorResolveExecutor(resolveAgent, run),
                    new L3AnchorValidateExecutor(run),
                    new L3SupportJudgeExecutor(judge, run),
                    new L3RoutingExecutor(run),
                    new L3FinalizeExecutor(run, coverageSpec: coverageSpec));
            }
            else
            {
                AIAgent genAgent = genClient.AsAIAgent(instructions: genPrompt, name: AgentName, tools: [.. tools]);
                genAgent = genAgent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
                workflow = useCoverageLoop
                    ? L3Workflow.Build(
                        new L3CoverageGenExecutor(genAgent, coverageSpec!, repairRounds, run),
                        new L3AnchorResolveExecutor(resolveAgent, run),
                        new L3AnchorValidateExecutor(run),
                        new L3SupportJudgeExecutor(judge, run),
                        new L3RoutingExecutor(run),
                        new L3FinalizeExecutor(run, coverageSpec: coverageSpec))
                    : L3Workflow.Build(
                        new L3CandidateGenExecutor(genAgent, run),
                        new L3AnchorResolveExecutor(resolveAgent, run),
                        new L3AnchorValidateExecutor(run),
                        new L3SupportJudgeExecutor(judge, run),
                        new L3RoutingExecutor(run),
                        new L3FinalizeExecutor(run, coverageSpec: coverageSpec));
            }
        }

        if (dryRun)
        {
            Console.WriteLine("[l3] --dry-run: Graph Build()-bar (Agentic/Candidate/CoverageGen → AnchorResolve → Validate[det] → SupportJudge[Judge] → Routing[det] → Finalize; bei --candidates ab Validate). Kein LLM.");
            Console.WriteLine($"[l3] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine("[l3] running open-world prepare workflow...");
        try
        {
            if (inject) await InProcessExecution.Default.RunAsync(workflow, injected!, run.RunId, CancellationToken.None).ConfigureAwait(false);
            else await InProcessExecution.Default.RunAsync(workflow, env, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l3] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var routingPath = Path.Combine(run.RunDir, "routing-report.json");
        if (File.Exists(routingPath))
        {
            using var d = JsonDocument.Parse(await File.ReadAllTextAsync(routingPath).ConfigureAwait(false));
            var total = d.RootElement.GetProperty("total").GetInt32();
            var byClass = d.RootElement.GetProperty("byClass");
            Console.WriteLine($"[l3] fertig: {total} Kandidaten geroutet — {string.Join(", ", byClass.EnumerateObject().Select(p => $"{p.Name}={p.Value.GetInt32()}"))}");
        }
        var tracePath = Path.Combine(run.RunDir, "coverage-repair-trace.json");
        if (File.Exists(tracePath))
        {
            using var d = JsonDocument.Parse(await File.ReadAllTextAsync(tracePath).ConfigureAwait(false));
            var r = d.RootElement;
            Console.WriteLine($"[l3] repair-loop: {r.GetProperty("roundsRun").GetInt32()}/{r.GetProperty("maxRepairRounds").GetInt32()} Runden"
                + $" — Abdeckung {r.GetProperty("initialAddressed").GetInt32()}→{r.GetProperty("finalAddressed").GetInt32()}/{r.GetProperty("totalLenses").GetInt32()} Linsen");
        }
        var coveragePath = Path.Combine(run.RunDir, "lens-coverage-report.json");
        if (File.Exists(coveragePath))
        {
            using var d = JsonDocument.Parse(await File.ReadAllTextAsync(coveragePath).ConfigureAwait(false));
            var r = d.RootElement;
            var unaddr = r.GetProperty("unaddressedLensIds").EnumerateArray().Select(x => x.GetString()).ToArray();
            var mand = r.GetProperty("mandatoryUnaddressedLensIds").EnumerateArray().Select(x => x.GetString()).ToArray();
            Console.WriteLine($"[l3] coverage: {r.GetProperty("addressedLenses").GetInt32()}/{r.GetProperty("totalLenses").GetInt32()} Linsen adressiert"
                + (unaddr.Length > 0 ? $" — LEER: [{string.Join(",", unaddr)}]" : " — vollständig")
                + (mand.Length > 0 ? $"  ⚠ PFLICHT-LINSE LEER: [{string.Join(",", mand)}]" : ""));
        }
        Console.WriteLine($"[l3] -> routing-report.json + human-review-package.json" + (coverageSpec is not null ? " + lens-coverage-report.json" : "") + $"  run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));

    /// <summary>Lädt kontrollierte Test-Kandidaten (mit vorgegebenen Ankern) → <see cref="ResolvedCandidate"/>-Liste,
    /// als hätte die Resolution sie geliefert (Plan §11.1). CandidateId aus der Datei ODER T{n} als Fallback.</summary>
    private static async Task<IReadOnlyList<ResolvedCandidate>> LoadTestCandidatesAsync(string path)
    {
        var file = JsonSerializer.Deserialize<TestCandidatesFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
        var result = new List<ResolvedCandidate>();
        var n = 0;
        foreach (var c in file?.Candidates ?? [])
        {
            if (string.IsNullOrWhiteSpace(c.Text)) continue;
            n++;
            var id = string.IsNullOrWhiteSpace(c.CandidateId) ? $"T{n:D2}" : c.CandidateId!.Trim();
            var candidate = new L3Candidate(id, string.IsNullOrWhiteSpace(c.TargetType) ? "requirement" : c.TargetType!.Trim(),
                c.Text!.Trim(), c.Rationale, c.Assumptions ?? []);
            var anchors = (c.ProposedAnchors ?? [])
                .Where(a => !string.IsNullOrWhiteSpace(a.ItemId))
                .Select(a => new ProposedAnchor(a.ItemId!.Trim(), string.IsNullOrWhiteSpace(a.Relation) ? "relates_to" : a.Relation!.Trim(), a.Reason ?? ""))
                .ToList();
            result.Add(new ResolvedCandidate(candidate, new L3AnchorResolution(anchors, anchors.Count == 0 ? "Test-Kandidat ohne Anker" : null)));
        }
        return result;
    }

    private sealed record TestCandidatesFile(
        [property: System.Text.Json.Serialization.JsonPropertyName("candidates")] IReadOnlyList<TestCandidate>? Candidates);
    private sealed record TestCandidate(
        [property: System.Text.Json.Serialization.JsonPropertyName("candidateId")] string? CandidateId,
        [property: System.Text.Json.Serialization.JsonPropertyName("targetType")] string? TargetType,
        [property: System.Text.Json.Serialization.JsonPropertyName("text")] string? Text,
        [property: System.Text.Json.Serialization.JsonPropertyName("rationale")] string? Rationale,
        [property: System.Text.Json.Serialization.JsonPropertyName("assumptions")] IReadOnlyList<string>? Assumptions,
        [property: System.Text.Json.Serialization.JsonPropertyName("proposedAnchors")] IReadOnlyList<TestAnchor>? ProposedAnchors);
    private sealed record TestAnchor(
        [property: System.Text.Json.Serialization.JsonPropertyName("itemId")] string? ItemId,
        [property: System.Text.Json.Serialization.JsonPropertyName("relation")] string? Relation,
        [property: System.Text.Json.Serialization.JsonPropertyName("reason")] string? Reason);
}
