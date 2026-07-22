using AgenticSdlc.Host.Run;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2;

/// <summary>
/// Schreibt das host-seitige Approval-Artefakt nach erfolgreich bestandener Phase-2.1-Validation.
/// Kein Modellurteil — reiner Host-Nachweis dass Workflow und Mindestvalidierung bestanden haben (siehe CL-007).
/// </summary>
public sealed class Phase2ApprovalRecorder
{
    private readonly RunContext _run;

    public Phase2ApprovalRecorder(RunContext run)
    {
        _run = run;
    }

    /// <summary>
    /// Schreibt genau ein Phase-2.1-Approval Artefakt, falls noch keines existiert.
    /// </summary>
    public void RecordApprovalIfNeeded()
    {
        if (_run.ApprovalExists(Phase2Artifacts.ApprovalAction))
        {
            _run.AppendEvent(new
            {
                type = "APPROVAL_SKIPPED_ALREADY_EXISTS",
                action = Phase2Artifacts.ApprovalAction,
                timestampUtc = DateTime.UtcNow
            });
            return;
        }

        var now = DateTime.UtcNow;
        Directory.CreateDirectory(_run.ApprovalsDir);

        var validationReportPath = Phase2Artifacts.ValidationReportPath(_run).Replace('\\', '/');
        var contextPath = Phase2Artifacts.ContextPath(_run).Replace('\\', '/');

        var artifactsForReview = Phase2Artifacts.RequiredDocs
            .Concat([contextPath, validationReportPath])
            .ToArray();

        var payloadObj = new
        {
            runId = _run.RunId,
            phase = Phase2Artifacts.PhaseName,
            action = Phase2Artifacts.ApprovalAction,
            status = "pending_human_review",
            governance = new
            {
                source = "host",
                meaning = "MAF workflow completed and deterministic Phase 2.1 validation passed.",
                limitation = "This is not a deep human content approval and not an LLM-internal rationale."
            },
            preview = new
            {
                artifactsForReview,
                validationReport = validationReportPath,
                requiredDocs = Phase2Artifacts.RequiredDocs,
                contextArtifact = contextPath
            },
            requestedAtUtc = now
        };

        var fileName = Path.Combine(_run.ApprovalsDir, $"approval_{now:yyyyMMddHHmmssfff}.json");
        File.WriteAllText(fileName, JsonSerializer.Serialize(payloadObj, new JsonSerializerOptions { WriteIndented = true }));

        _run.AppendEvent(new
        {
            type = "APPROVAL_RECORDED_BY_HOST",
            runId = _run.RunId,
            phase = Phase2Artifacts.PhaseName,
            action = Phase2Artifacts.ApprovalAction,
            file = Path.GetRelativePath(_run.RunDir, fileName).Replace('\\', '/'),
            timestampUtc = now
        });
    }
}
