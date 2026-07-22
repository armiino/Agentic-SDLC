using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

public static class LedgerBuildUnitsRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const string SourceName = "AgenticSdlc.Host";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: ledger-build-units <transcript.txt> [modelOverride]");
            return 2;
        }

        var transcriptPath = Resolve(repoRoot, args[1]);
        if (!File.Exists(transcriptPath))
        {
            Console.Error.WriteLine($"[ledger-build-units] Transkript fehlt: {transcriptPath}");
            return 2;
        }

        var modelArg = args.Length >= 3 ? args[2] : null;
        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        var transcript = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);
        var run = new RunContext(RunId.New(), "ledger");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = "LedgerBuilderUnitCoverage",
            stage = "L4-unit-coverage-mvp",
            runId = run.RunId,
            transcript = Path.GetRelativePath(repoRoot, transcriptPath),
            provider = settings.LlmProvider,
            model = judgeSettings.ModelId,
            structuredOutput = settings.JuryStructuredOutput,
            innerCycleLogging = settings.InnerCycleLogging,
            timestampUtc = DateTime.UtcNow
        });

        Console.WriteLine($"[ledger-build-units] runId={run.RunId} transcript={Path.GetRelativePath(repoRoot, transcriptPath)} model={judgeSettings.ModelId}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var baseClient = ChatClientFactory.Create(judgeSettings);
        var extractionClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, UnitAwareCandidateExtractionExecutor.ExecutorName, SourceName);
        var unusedTriageClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, UnusedUnitTriageExecutor.ExecutorName, SourceName);
        var unusedCompareClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, UnusedUnitLedgerCompareExecutor.ExecutorName, SourceName);
        var canonicalClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, CanonicalizationExecutor.ExecutorName, SourceName);
        var coverageRepairClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, CanonicalCoverageRepairExecutor.ExecutorName, SourceName);
        var facetClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, FacetValidationExecutor.ExecutorName, SourceName);

        var extractor = new UnitAwareSemanticLedgerExtractor(extractionClient, settings.JuryStructuredOutput);
        var unusedUnitTriageReviewer = new UnusedUnitTriageReviewer(unusedTriageClient, settings.JuryStructuredOutput);
        var unusedUnitLedgerComparer = new UnusedUnitLedgerComparer(unusedCompareClient, settings.JuryStructuredOutput);
        var canonicalizer = new SemanticLedgerCanonicalizer(canonicalClient, settings.JuryStructuredOutput);
        var coverageRepairer = new CanonicalCoverageRepairer(coverageRepairClient, settings.JuryStructuredOutput);
        var facetValidator = new FacetValidator(facetClient, settings.JuryStructuredOutput);
        var sourceName = Path.GetFileName(transcriptPath);

        var workflow = LedgerBuilderWorkflow.BuildUnitCoverage(
            extractor,
            unusedUnitTriageReviewer,
            unusedUnitLedgerComparer,
            canonicalizer,
            coverageRepairer,
            facetValidator,
            transcript,
            sourceName,
            run);

        Console.WriteLine("[ledger-build-units] running workflow (units -> extraction -> unit-gate -> unused-triage -> unused-compare -> canonicalization -> coverage-repair -> facet-validation)...");
        var workflowRun = await InProcessExecution.Default
            .RunAsync(workflow, transcript, run.RunId, CancellationToken.None)
            .ConfigureAwait(false);

        var hasWorkflowFailure = RecordWorkflowEvents(run, workflowRun);
        var workflowStatus = await workflowRun.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);
        run.AppendEvent(new
        {
            type = "WORKFLOW_FINISHED",
            runId = run.RunId,
            workflow = "LedgerBuilderUnitCoverage",
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
                workflow = "LedgerBuilderUnitCoverage",
                status = "FAILED",
                rootCause = new { code = "LEDGER_UNIT_WORKFLOW_EXECUTOR_FAILED" },
                timestampUtc = DateTime.UtcNow
            });
            Console.Error.WriteLine($"[ledger-build-units] workflow failed -> {Path.GetRelativePath(repoRoot, Path.Combine(run.LogsDir, "diagnosis.json"))}");
            Console.WriteLine($"[ledger-build-units] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 3;
        }

        var unitsFixture = LedgerRunArtifacts.ReadStepOutput<AtomicUnitFixture>(run, "step-00-atomic-units");
        var unitGate = LedgerRunArtifacts.ReadStepOutput<UnitCoverageGateResult>(run, "step-01b-unit-coverage");
        var unusedTriage = LedgerRunArtifacts.ReadStepOutput<UnusedUnitTriageFixture>(run, "step-01c-unused-unit-triage");
        var unusedCompare = LedgerRunArtifacts.ReadStepOutput<UnusedUnitLedgerCompareFixture>(run, "step-01d-unused-unit-ledger-compare");
        var candidateFixture = LedgerRunArtifacts.ReadStepOutput<SemanticLedgerFixture>(run, "step-01-candidate");
        var canonicalFixture = LedgerRunArtifacts.ReadStepOutput<SemanticLedgerFixture>(run, "step-02-canonical");
        var validatedLedger = LedgerRunArtifacts.ReadStepOutput<ValidatedLedger>(run, "step-03-facet-validation");
        if (unitsFixture is null || unitGate is null || unusedTriage is null || unusedCompare is null || candidateFixture is null || canonicalFixture is null || validatedLedger is null)
        {
            WriteDiagnosis(run, new
            {
                runId = run.RunId,
                workflow = "LedgerBuilderUnitCoverage",
                status = "FAILED",
                rootCause = new
                {
                    code = "LEDGER_UNIT_STEP_OUTPUT_MISSING",
                    missingStep00Units = unitsFixture is null,
                    missingStep01Candidate = candidateFixture is null,
                    missingStep01bUnitCoverage = unitGate is null,
                    missingStep01cUnusedUnitTriage = unusedTriage is null,
                    missingStep01dUnusedUnitLedgerCompare = unusedCompare is null,
                    missingStep02Canonical = canonicalFixture is null,
                    missingStep03FacetValidation = validatedLedger is null
                },
                timestampUtc = DateTime.UtcNow
            });
            Console.Error.WriteLine($"[ledger-build-units] required step output missing -> {Path.GetRelativePath(repoRoot, Path.Combine(run.LogsDir, "diagnosis.json"))}");
            Console.WriteLine($"[ledger-build-units] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 3;
        }

        var candidate = candidateFixture.Entries;
        var canonical = canonicalFixture.Entries;
        var validated = validatedLedger.Entries;
        var potentiallyRelevant = unusedTriage.Items.Count(i => i.Triage == "potentially_relevant");
        var missingClaim = unusedCompare.Items.Count(i => i.Verdict == "missing_claim");
        var needsHuman = unusedCompare.Items.Count(i => i.Verdict == "needs_human");
        var alreadyCovered = unusedCompare.Items.Count(i => i.Verdict == "already_covered_indirectly");
        var attachAsEvidence = unusedCompare.Items.Count(i => i.Verdict == "attach_as_evidence");
        Console.WriteLine($"[ledger-build-units] units={unitsFixture.Units.Count} used={unitGate.UsedUnits} unused={unitGate.UnusedUnits.Count}");
        Console.WriteLine($"[ledger-build-units] unused-triage: potentiallyRelevant={potentiallyRelevant} noise={unusedTriage.Items.Count - potentiallyRelevant}");
        Console.WriteLine($"[ledger-build-units] unused-compare: missingClaim={missingClaim} needsHuman={needsHuman} alreadyCovered={alreadyCovered} attachAsEvidence={attachAsEvidence}");
        Console.WriteLine($"[ledger-build-units] candidate={candidate.Count} -> canonical={canonical.Count} -> validated={validated.Count}");

        var gate = LedgerQualityGate.Evaluate(candidate, canonical, validated);
        var gateDir = Path.Combine(run.RunDir, "gate");
        Directory.CreateDirectory(gateDir);
        await File.WriteAllTextAsync(Path.Combine(gateDir, "ledger-quality.json"), JsonSerializer.Serialize(gate, JsonOptions)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(gateDir, "unit-coverage.json"), JsonSerializer.Serialize(unitGate, JsonOptions)).ConfigureAwait(false);

        // Fix: selbst-prüfbarer Trace (Unit -> candidate -> canonical) + Verweis-Validierung (INVALID_ATTACH_REFERENCE).
        var trace = UnusedUnitTrace.Build(unitsFixture.Units, candidate, canonical, unusedCompare.Items);
        await File.WriteAllTextAsync(Path.Combine(run.RunDir, "unused-unit-trace.json"), JsonSerializer.Serialize(trace, JsonOptions)).ConfigureAwait(false);
        Console.WriteLine($"[ledger-build-units] unused-unit-trace: pass={trace.Pass} brokenRefs={trace.BrokenReferenceCount} missingTarget={trace.MissingTargetCount} -> unused-unit-trace.json");

        var overallPass = gate.Pass && unitGate.Pass && trace.Pass;
        Console.WriteLine($"[ledger-build-units] gate: pass={overallPass} ledgerErrors={gate.ErrorCount} unitTracePass={unitGate.Pass} compareTracePass={trace.Pass}");

        if (!overallPass)
        {
            WriteDiagnosis(run, new
            {
                runId = run.RunId,
                workflow = "LedgerBuilderUnitCoverage",
                status = "GATE_FAILED",
                rootCause = new
                {
                    code = "LEDGER_UNIT_QUALITY_GATE_FAILED",
                    ledgerErrors = gate.ErrorCount,
                    unitTracePass = unitGate.Pass,
                    compareTracePass = trace.Pass,
                    invalidAttachReferences = trace.BrokenReferenceCount,
                    missingCompareTargets = trace.MissingTargetCount
                },
                timestampUtc = DateTime.UtcNow
            });
            Console.Error.WriteLine($"[ledger-build-units] GATE FAILED -> {Path.GetRelativePath(repoRoot, gateDir)}");
            Console.WriteLine($"[ledger-build-units] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 4;
        }

        Console.WriteLine($"[ledger-build-units] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static bool RecordWorkflowEvents(RunContext run, Microsoft.Agents.AI.Workflows.Run workflowRun)
    {
        var hasWorkflowFailure = false;
        foreach (var workflowEvent in workflowRun.OutgoingEvents)
        {
            if (workflowEvent is ExecutorCompletedEvent completed)
            {
                run.AppendEvent(new { type = "EXECUTOR_FINISHED", runId = run.RunId, workflow = "LedgerBuilderUnitCoverage", executorId = completed.ExecutorId, eventType = workflowEvent.GetType().Name, data = SafeWorkflowEventData(completed.Data), timestampUtc = DateTime.UtcNow });
                continue;
            }

            if (workflowEvent is ExecutorFailedEvent failed)
            {
                hasWorkflowFailure = true;
                run.AppendEvent(new { type = "EXECUTOR_FAILED", runId = run.RunId, workflow = "LedgerBuilderUnitCoverage", executorId = failed.ExecutorId, eventType = workflowEvent.GetType().Name, error = failed.Data?.Message, errorType = failed.Data?.GetType().FullName, timestampUtc = DateTime.UtcNow });
                continue;
            }

            if (workflowEvent is WorkflowErrorEvent error)
            {
                hasWorkflowFailure = true;
                run.AppendEvent(new { type = "WORKFLOW_ERROR", runId = run.RunId, workflow = "LedgerBuilderUnitCoverage", eventType = workflowEvent.GetType().Name, error = error.Exception?.Message, errorType = error.Exception?.GetType().FullName, timestampUtc = DateTime.UtcNow });
                continue;
            }

            run.AppendEvent(new { type = "WORKFLOW_EVENT", runId = run.RunId, workflow = "LedgerBuilderUnitCoverage", eventType = workflowEvent.GetType().Name, data = SafeWorkflowEventData(workflowEvent.Data), timestampUtc = DateTime.UtcNow });
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

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
