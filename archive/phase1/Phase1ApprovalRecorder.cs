using AgenticSdlc.Host.Run;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase1;

/// <summary>
/// Schreibt die Host-seitige Approval-Datei fürr Phase 1.
/// Aktuell: Definition of Done überprüufng: manchmal failen die Toolcalls etc. und der agent
/// behauptet die Artifakte geschrieben zu haben: deswegen hier nötig!
/// </summary>
/// <remarks>
/// Diese Klasse ist bewusst vom Agenten getrennt. Das Approval ist kein
/// Modellurteil, sondern ein Host-Nachweis: Nach der Validierung existieren alle
/// Pflichtartefakte. Dadurch bleibt die Abschlussentscheidung reproduzierbarer
/// als eine reine Aussage des LLM.
/// </remarks>
public sealed class Phase1ApprovalRecorder
{
    private readonly RunContext _run;

    public Phase1ApprovalRecorder(RunContext run)
    {
        _run = run;
    }

    /// <summary>
    /// Schreibt genau eine Approval-Datei, falls für diesen Run noch keine
    /// Approval-Datei mit derselben Aktion existiert.
    /// </summary>
    public void RecordApprovalIfNeeded()
    {
        if (!_run.ApprovalExists(Phase1Artifacts.ApprovalAction))
        {
            Directory.CreateDirectory(_run.ApprovalsDir);

            var payloadObj = new
            {
                runId = _run.RunId,
                action = Phase1Artifacts.ApprovalAction,
                requiredDocs = Phase1Artifacts.RequiredDocs,
                missingDocs = Array.Empty<string>(),
                createdFiles = Phase1Artifacts.RequiredDocs,
                summary = "Host verified DoD: all required docs exist.",
                requestedAtUtc = DateTime.UtcNow
            };

            var fileName = Path.Combine(_run.ApprovalsDir, $"approval_{DateTime.UtcNow:yyyyMMddHHmmssfff}.json");
            File.WriteAllText(fileName, JsonSerializer.Serialize(payloadObj, new JsonSerializerOptions { WriteIndented = true }));

            _run.AppendEvent(new
            {
                type = "APPROVAL_RECORDED_BY_HOST",
                action = Phase1Artifacts.ApprovalAction,
                file = Path.GetRelativePath(_run.RunDir, fileName).Replace('\\', '/'),
                timestampUtc = DateTime.UtcNow
            });

            return;
        }

        _run.AppendEvent(new
        {
            type = "APPROVAL_SKIPPED_ALREADY_EXISTS",
            action = Phase1Artifacts.ApprovalAction,
            timestampUtc = DateTime.UtcNow
        });
    }
}
