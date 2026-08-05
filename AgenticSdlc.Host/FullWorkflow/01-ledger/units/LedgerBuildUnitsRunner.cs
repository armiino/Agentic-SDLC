using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Ergebnis des wiederverwendbaren Ledger-Kerns <see cref="LedgerBuildUnitsRunner.BuildAsync"/>.
/// </summary>
public sealed record LedgerBuildResult(
    bool WorkflowOk,
    bool StepOutputsPresent,
    bool GatePass,
    string ValidatedLedgerPath,
    int UnitCount,
    int CandidateCount,
    int CanonicalCount,
    int ValidatedCount,
    int NeedsHumanCount,
    int MissingClaimCount,
    string? FailureCode)
{
    public bool Success => WorkflowOk && StepOutputsPresent && GatePass;
}

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

        var result = await BuildAsync(
            transcript, Path.GetFileName(transcriptPath), settings, judgeSettings, run, CancellationToken.None).ConfigureAwait(false);

        Console.WriteLine($"[ledger-build-units] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return result switch
        {
            { WorkflowOk: false } => Fail(repoRoot, run, "workflow failed"),
            { StepOutputsPresent: false } => Fail(repoRoot, run, "required step output missing"),
            { GatePass: false } => Fail(repoRoot, run, "GATE FAILED", 4),
            _ => 0
        };
    }

    private static int Fail(string repoRoot, RunContext run, string msg, int code = 3)
    {
        Console.Error.WriteLine($"[ledger-build-units] {msg} -> {Path.GetRelativePath(repoRoot, Path.Combine(run.LogsDir, "diagnosis.json"))}");
        return code;
    }

    /// <summary>
    /// Schritt 5 ② (05.08.) — die MONTAGE-Naht des Ledger-Kerns: baut die 6 Node-Clients + den
    /// LedgerBuilderUnitCoverage-Workflow (Input: Transkript-String, Output: Summary-Yield der
    /// FacetValidation). EINE Quelle für die CLI-Bahn (<see cref="BuildAsync"/>, eigene Ausführung)
    /// UND die Graph-Bahn (pipeline-full bindet den Rückgabewert als sichtbare Kapsel, BindGateFree).
    /// </summary>
    internal static Microsoft.Agents.AI.Workflows.Workflow CreateWorkflow(
        string transcript, string transcriptSourceName, HostSettings settings, HostSettings judgeSettings, RunContext run)
    {
        var baseClient = ChatClientFactory.Create(judgeSettings);
        var extractionClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, UnitAwareCandidateExtractionExecutor.ExecutorName, SourceName);
        var unusedTriageClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, UnusedUnitTriageExecutor.ExecutorName, SourceName);
        var unusedCompareClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, UnusedUnitLedgerCompareExecutor.ExecutorName, SourceName);
        var canonicalClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, CanonicalizationExecutor.ExecutorName, SourceName);
        var coverageRepairClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, CanonicalCoverageRepairExecutor.ExecutorName, SourceName);
        var facetClient = AgentChatPipelineBuilder.Build(baseClient, settings, run, FacetValidationExecutor.ExecutorName, SourceName);

        var extractor = new UnitAwareSemanticLedgerExtractor(extractionClient, settings.JuryStructuredOutput, settings.ReasoningCapture);
        var unusedUnitTriageReviewer = new UnusedUnitTriageReviewer(unusedTriageClient, settings.JuryStructuredOutput, settings.ReasoningCapture);
        var unusedUnitLedgerComparer = new UnusedUnitLedgerComparer(unusedCompareClient, settings.JuryStructuredOutput, settings.ReasoningCapture);
        var canonicalizer = new SemanticLedgerCanonicalizer(canonicalClient, settings.JuryStructuredOutput, settings.ReasoningCapture);
        var coverageRepairer = new CanonicalCoverageRepairer(coverageRepairClient, settings.JuryStructuredOutput, settings.ReasoningCapture);
        var facetValidator = new FacetValidator(facetClient, settings.JuryStructuredOutput, settings.ReasoningCapture);

        return LedgerBuilderWorkflow.BuildUnitCoverage(
            extractor,
            unusedUnitTriageReviewer,
            unusedUnitLedgerComparer,
            canonicalizer,
            coverageRepairer,
            facetValidator,
            transcript,
            transcriptSourceName,
            run);
    }

    /// <summary>
    /// W1e' — der wiederverwendbare Ledger-Kern für die CLI-Bahn: eigener otel-Scope, eigene in-process
    /// Ausführung, dann die geteilte Auswertung (<see cref="EvaluateAsync"/>). Die Graph-Bahn (pipeline-full)
    /// führt den Workflow stattdessen als GEBUNDENE Kapsel im Ein-Graph aus (Schritt 5 ②) und ruft nur
    /// noch <see cref="EvaluateAsync"/> — Montage und Auswertung sind die geteilten Nähte, keine Kopie.
    /// </summary>
    public static async Task<LedgerBuildResult> BuildAsync(
        string transcript, string transcriptSourceName, HostSettings settings, HostSettings judgeSettings,
        RunContext run, CancellationToken ct)
    {
        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var workflow = CreateWorkflow(transcript, transcriptSourceName, settings, judgeSettings, run);

        Console.WriteLine("[ledger-build-units] running workflow (units -> extraction -> unit-gate -> unused-triage -> unused-compare -> canonicalization -> coverage-repair -> facet-validation)...");
        var workflowRun = await InProcessExecution.Default
            .RunAsync(workflow, transcript, run.RunId, ct)
            .ConfigureAwait(false);

        var hasWorkflowFailure = RecordWorkflowEvents(run, workflowRun);
        var workflowStatus = await workflowRun.GetStatusAsync(ct).ConfigureAwait(false);
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
            var failedPath = Path.Combine(run.RunDir, "step-03-facet-validation", "output.json");
            return new LedgerBuildResult(false, false, false, failedPath, 0, 0, 0, 0, 0, 0, "LEDGER_UNIT_WORKFLOW_EXECUTOR_FAILED");
        }

        return await EvaluateAsync(run).ConfigureAwait(false);
    }

    /// <summary>
    /// Schritt 5 ② — die AUSWERTUNGS-Naht des Ledger-Kerns (deterministisch, disk-basiert, kein LLM):
    /// liest die Step-Outputs des Laufs, bewertet Quality-Gate + Unit-Trace, schreibt gate/ + Diagnose und
    /// liefert das <see cref="LedgerBuildResult"/>. Geteilt von CLI-Bahn (<see cref="BuildAsync"/>) und
    /// Graph-Bahn (LedgerSummaryExecutor hinter der Kapsel).
    /// </summary>
    internal static async Task<LedgerBuildResult> EvaluateAsync(RunContext run)
    {
        var validatedPath = Path.Combine(run.RunDir, "step-03-facet-validation", "output.json");

        // Schritt 5 ④: endete der In-Graph-Kanonisierungs-Loop terminal (MaxAttempts/HumanReview), liegt der
        // ehrliche Befund in gate/canonical-gate.json — VOR dem generischen Step-Output-Check melden (sonst
        // würde ein irreführendes STEP_OUTPUT_MISSING die echte Ursache verdecken).
        var canonicalGatePath = Path.Combine(run.RunDir, "gate", "canonical-gate.json");
        if (File.Exists(canonicalGatePath))
        {
            using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(canonicalGatePath).ConfigureAwait(false));
            if (doc.RootElement.TryGetProperty("pass", out var passProp) && !passProp.GetBoolean())
            {
                var decision = doc.RootElement.TryGetProperty("decision", out var d) ? d.GetString() : null;
                var attempts = doc.RootElement.TryGetProperty("attempts", out var a) ? a.GetArrayLength() : 0;
                WriteDiagnosis(run, new
                {
                    runId = run.RunId,
                    workflow = "LedgerBuilderUnitCoverage",
                    status = "GATE_FAILED",
                    rootCause = new { code = "LEDGER_CANONICAL_GATE_FAILED", decision, attempts, detail = "gate/canonical-gate.json" },
                    timestampUtc = DateTime.UtcNow
                });
                var units = LedgerRunArtifacts.ReadStepOutput<AtomicUnitFixture>(run, "step-00-atomic-units");
                var cand = LedgerRunArtifacts.ReadStepOutput<SemanticLedgerFixture>(run, "step-01-candidate");
                var canon = LedgerRunArtifacts.ReadStepOutput<SemanticLedgerFixture>(run, "step-02-canonical");
                return new LedgerBuildResult(true, false, false, validatedPath,
                    units?.Units.Count ?? 0, cand?.Entries.Count ?? 0, canon?.Entries.Count ?? 0, 0, 0, 0,
                    "LEDGER_CANONICAL_GATE_FAILED");
            }
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
            return new LedgerBuildResult(true, false, false, validatedPath,
                unitsFixture?.Units.Count ?? 0, candidateFixture?.Entries.Count ?? 0, canonicalFixture?.Entries.Count ?? 0,
                validatedLedger?.Entries.Count ?? 0, 0, 0, "LEDGER_UNIT_STEP_OUTPUT_MISSING");
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
            return new LedgerBuildResult(true, true, false, validatedPath,
                unitsFixture.Units.Count, candidate.Count, canonical.Count, validated.Count, needsHuman, missingClaim, "LEDGER_UNIT_QUALITY_GATE_FAILED");
        }

        return new LedgerBuildResult(true, true, true, validatedPath,
            unitsFixture.Units.Count, candidate.Count, canonical.Count, validated.Count, needsHuman, missingClaim, null);
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
