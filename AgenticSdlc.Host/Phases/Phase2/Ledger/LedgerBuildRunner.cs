using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Einstiegspunkt: führt den <see cref="LedgerBuilderWorkflow"/> als echten MAF-Workflow
/// aus und legt die Ergebnisse unter <c>runs/ledger/&lt;runId&gt;/</c> ab.
/// </summary>
/// <remarks>
/// Befehl: <c>ledger-build &lt;transcript.txt&gt; [modelOverride] [fixture.json]</c>.
/// Wird eine Fixture angegeben, matcht der Runner den kanonischen Ledger mit
/// <see cref="SemanticLedgerRecallMatcher"/> gegen die Fixture (Recall/Facet). Provenienz/Logging laufen über die bestehende Infrastruktur
/// (RunContext, AgentChatPipelineBuilder, OtelRunExporters) — keine Parallel-Infrastruktur.
/// </remarks>
public static class LedgerBuildRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const string SourceName = "AgenticSdlc.Host";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: ledger-build <transcript.txt> [modelOverride] [fixture.json]");
            return 2;
        }

        var transcriptPath = Resolve(repoRoot, args[1]);
        if (!File.Exists(transcriptPath)) { Console.Error.WriteLine($"[ledger-build] Transkript fehlt: {transcriptPath}"); return 2; }

        var modelArg = args.Length >= 3 ? args[2] : null;
        var fixturePath = args.Length >= 4 ? Resolve(repoRoot, args[3]) : null;
        if (fixturePath is not null && !File.Exists(fixturePath)) { Console.Error.WriteLine($"[ledger-build] Fixture fehlt: {fixturePath}"); return 2; }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        var transcript = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);

        // Run-Infrastruktur wiederverwenden: eigener Ordner runs/ledger/<runId>.
        var run = new RunContext(RunId.New(), "ledger");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = "LedgerBuilder",
            stage = "L3",
            runId = run.RunId,
            transcript = Path.GetRelativePath(repoRoot, transcriptPath),
            fixture = fixturePath is null ? null : Path.GetRelativePath(repoRoot, fixturePath),
            provider = settings.LlmProvider,
            model = judgeSettings.ModelId,
            structuredOutput = settings.JuryStructuredOutput,
            innerCycleLogging = settings.InnerCycleLogging,
            timestampUtc = DateTime.UtcNow
        });

        Console.WriteLine($"[ledger-build] runId={run.RunId} transcript={Path.GetRelativePath(repoRoot, transcriptPath)} model={judgeSettings.ModelId}");

        // OTel-Export pro Run (wenn aktiviert) — wie der Phasen-Hauptpfad.
        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Pro Executor ein eigener, geloggter LLM-Client (InputContext/ChatDecision/OTel je Executor
        // nach logs/agents/<executor>/) — dieselbe Pipeline wie die Agenten-Phasen.
        var baseClient = ChatClientFactory.Create(judgeSettings);
        var extractionClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, CandidateExtractionExecutor.ExecutorName, SourceName);
        var canonicalClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, CanonicalizationExecutor.ExecutorName, SourceName);
        var facetClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, FacetValidationExecutor.ExecutorName, SourceName);

        var extractor = new SemanticLedgerExtractor(extractionClient, settings.JuryStructuredOutput);
        var canonicalizer = new SemanticLedgerCanonicalizer(canonicalClient, settings.JuryStructuredOutput);
        var facetValidator = new FacetValidator(facetClient, settings.JuryStructuredOutput);

        var workflow = LedgerBuilderWorkflow.Build(extractor, canonicalizer, facetValidator, transcript, run);

        Console.WriteLine("[ledger-build] running workflow (extraction -> canonicalization -> facet-validation)...");
        var workflowRun = await InProcessExecution.Default
            .RunAsync(workflow, transcript, run.RunId, CancellationToken.None)
            .ConfigureAwait(false);

        var hasWorkflowFailure = RecordWorkflowEvents(run, workflowRun);
        var workflowStatus = await workflowRun.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);
        run.AppendEvent(new
        {
            type = "WORKFLOW_FINISHED",
            runId = run.RunId,
            workflow = "LedgerBuilder",
            workflowSessionId = workflowRun.SessionId,
            status = workflowStatus.ToString(),
            hasWorkflowFailure,
            timestampUtc = DateTime.UtcNow
        });

        if (hasWorkflowFailure)
        {
            WriteDiagnosis(run, new
            {
                runId = run.RunId,
                workflow = "LedgerBuilder",
                status = "FAILED",
                rootCause = new
                {
                    code = "LEDGER_WORKFLOW_EXECUTOR_FAILED",
                    message = "The MAF workflow emitted an ExecutorFailedEvent or WorkflowErrorEvent."
                },
                timestampUtc = DateTime.UtcNow
            });
            Console.Error.WriteLine($"[ledger-build] workflow failed -> {Path.GetRelativePath(repoRoot, Path.Combine(run.LogsDir, "diagnosis.json"))}");
            Console.WriteLine($"[ledger-build] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 3;
        }

        var candidateFixture = LedgerRunArtifacts.ReadStepOutput<SemanticLedgerFixture>(run, "step-01-candidate");
        var canonicalFixture = LedgerRunArtifacts.ReadStepOutput<SemanticLedgerFixture>(run, "step-02-canonical");
        var validatedLedger = LedgerRunArtifacts.ReadStepOutput<ValidatedLedger>(run, "step-03-facet-validation");
        if (candidateFixture is null || canonicalFixture is null || validatedLedger is null)
        {
            WriteDiagnosis(run, new
            {
                runId = run.RunId,
                workflow = "LedgerBuilder",
                status = "FAILED",
                rootCause = new
                {
                    code = "LEDGER_STEP_OUTPUT_MISSING",
                    message = "The MAF workflow completed without producing all required ledger step outputs.",
                    missingStep01Candidate = candidateFixture is null,
                    missingStep02Canonical = canonicalFixture is null,
                    missingStep03FacetValidation = validatedLedger is null
                },
                timestampUtc = DateTime.UtcNow
            });
            Console.Error.WriteLine($"[ledger-build] required step output missing -> {Path.GetRelativePath(repoRoot, Path.Combine(run.LogsDir, "diagnosis.json"))}");
            Console.WriteLine($"[ledger-build] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 3;
        }

        var candidate = candidateFixture.Entries;
        var canonical = canonicalFixture.Entries;
        var validated = validatedLedger.Entries;
        Console.WriteLine($"[ledger-build] candidate={candidate.Count} -> canonical={canonical.Count} -> validated={validated.Count}");
        Console.WriteLine($"[ledger-build] steps -> {Path.GetRelativePath(repoRoot, Path.Combine(run.RunDir, "step-01-candidate"))} , step-02-canonical , step-03-facet-validation");

        if (fixturePath is not null)
        {
            await MatchAgainstFixtureAsync(run, repoRoot, fixturePath, canonical, baseClient, settings, judgeSettings.ModelId).ConfigureAwait(false);
        }
        else
        {
            Console.WriteLine("[ledger-build] keine Fixture angegeben -> kein Baseline-Match (nur Ledger erzeugt).");
        }

        Console.WriteLine($"[ledger-build] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static bool RecordWorkflowEvents(RunContext run, Microsoft.Agents.AI.Workflows.Run workflowRun)
    {
        var hasWorkflowFailure = false;

        foreach (var workflowEvent in workflowRun.OutgoingEvents)
        {
            if (workflowEvent is ExecutorCompletedEvent completed)
            {
                run.AppendEvent(new
                {
                    type = "EXECUTOR_FINISHED",
                    runId = run.RunId,
                    workflow = "LedgerBuilder",
                    executorId = completed.ExecutorId,
                    eventType = workflowEvent.GetType().Name,
                    data = SafeWorkflowEventData(completed.Data),
                    timestampUtc = DateTime.UtcNow
                });
                continue;
            }

            if (workflowEvent is ExecutorFailedEvent failed)
            {
                hasWorkflowFailure = true;
                run.AppendEvent(new
                {
                    type = "EXECUTOR_FAILED",
                    runId = run.RunId,
                    workflow = "LedgerBuilder",
                    executorId = failed.ExecutorId,
                    eventType = workflowEvent.GetType().Name,
                    error = failed.Data?.Message,
                    errorType = failed.Data?.GetType().FullName,
                    timestampUtc = DateTime.UtcNow
                });
                continue;
            }

            if (workflowEvent is WorkflowErrorEvent error)
            {
                hasWorkflowFailure = true;
                run.AppendEvent(new
                {
                    type = "WORKFLOW_ERROR",
                    runId = run.RunId,
                    workflow = "LedgerBuilder",
                    eventType = workflowEvent.GetType().Name,
                    error = error.Exception?.Message,
                    errorType = error.Exception?.GetType().FullName,
                    timestampUtc = DateTime.UtcNow
                });
                continue;
            }

            if (workflowEvent is WorkflowOutputEvent output)
            {
                run.AppendEvent(new
                {
                    type = "WORKFLOW_OUTPUT",
                    runId = run.RunId,
                    workflow = "LedgerBuilder",
                    executorId = output.ExecutorId,
                    data = SafeWorkflowEventData(output.Data),
                    timestampUtc = DateTime.UtcNow
                });
                continue;
            }

            run.AppendEvent(new
            {
                type = "WORKFLOW_EVENT",
                runId = run.RunId,
                workflow = "LedgerBuilder",
                eventType = workflowEvent.GetType().Name,
                data = SafeWorkflowEventData(workflowEvent.Data),
                timestampUtc = DateTime.UtcNow
            });
        }

        return hasWorkflowFailure;
    }

    private static object? SafeWorkflowEventData(object? data)
    {
        if (data is null) return null;
        if (data is string or int or long or bool or double or decimal) return data;
        return data.ToString();
    }

    private static void WriteDiagnosis(RunContext run, object diagnosis)
        => File.WriteAllText(
            Path.Combine(run.LogsDir, "diagnosis.json"),
            JsonSerializer.Serialize(diagnosis, JsonOptions));

    /// <summary>EXIT-KRITERIUM L1: kanonischer Ledger gegen bestätigte Fixture (Recall/Facet), wie der Spike-Runner.</summary>
    private static async Task MatchAgainstFixtureAsync(
        RunContext run,
        string repoRoot,
        string fixturePath,
        IReadOnlyList<SemanticLedgerEntry> canonical,
        Microsoft.Extensions.AI.IChatClient baseClient,
        HostSettings settings,
        string model)
    {
        var fixture = JsonSerializer.Deserialize<SemanticLedgerFixture>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var expected = fixture?.Entries.ToList() ?? [];
        if (expected.Count == 0) { Console.Error.WriteLine("[ledger-build] leere Fixture -> kein Match."); return; }

        var matcher = new SemanticLedgerRecallMatcher(baseClient, settings.JuryStructuredOutput);
        var matches = new List<object>();
        int exact = 0, partial = 0, missed = 0;
        var facetExact = new Dictionary<string, int>
        {
            ["proposition"] = 0, ["status"] = 0, ["modality"] = 0, ["scope"] = 0,
            ["timeScope"] = 0, ["evidence"] = 0, ["disposition"] = 0
        };

        foreach (var exp in expected)
        {
            var v = await matcher.MatchAsync(exp, canonical, CancellationToken.None).ConfigureAwait(false);
            if (v.Verdict == "exact") exact++; else if (v.Verdict == "partial") partial++; else missed++;
            if (v.PropositionMatch == "exact") facetExact["proposition"]++;
            if (v.StatusMatch == "exact") facetExact["status"]++;
            if (v.ModalityMatch == "exact") facetExact["modality"]++;
            if (v.ScopeMatch == "exact") facetExact["scope"]++;
            if (v.TimeScopeMatch == "exact") facetExact["timeScope"]++;
            if (v.EvidenceMatch == "exact") facetExact["evidence"]++;
            if (v.DispositionMatch == "exact") facetExact["disposition"]++;
            Console.WriteLine($"[{v.Verdict.ToUpperInvariant()}] {exp.Id} -> {string.Join(",", v.MatchedIds)} :: {v.Reason}");
            matches.Add(new { expected = exp, verdict = v });
        }

        var payload = new
        {
            mode = "ledger-build-fixture-match",
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            model,
            expected = expected.Count,
            canonicalCount = canonical.Count,
            metrics = new
            {
                exact, partial, missed,
                exactRecall = Math.Round(exact / (double)Math.Max(1, expected.Count), 4),
                exactOrPartialRecall = Math.Round((exact + partial) / (double)Math.Max(1, expected.Count), 4),
                facetExact = facetExact.ToDictionary(kv => kv.Key, kv => Math.Round(kv.Value / (double)Math.Max(1, expected.Count), 4))
            },
            matches
        };
        var resultPath = Path.Combine(run.RunDir, "fixture-match.json");
        await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[ledger-build] exact={exact}/{expected.Count} partial={partial}/{expected.Count} missed={missed}/{expected.Count}");
        Console.WriteLine($"[ledger-build] fixture-match -> {Path.GetRelativePath(repoRoot, resultPath)}");
    }

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
