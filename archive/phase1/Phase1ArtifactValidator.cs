using AgenticSdlc.Host.Run;
using System.Diagnostics;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase1;

/// <summary>
/// Prüft die Pflichtartefakte der Phase 1 nach der Agentenausführung.
/// </summary>
/// <remarks>
/// Diese Validierung ist Host-seitig und damit stabiler als eine reine Modellbehauptung.
/// Sie prüft aktuell bewusst nur die Existenz der vier Pflichtdateien.
/// Inhaltliche Qualitätsprüfungen bleiben eine spätere Erweiterung, damit Phase 1 nicht versteckt zu einer Reviewer-Phase wird.
/// </remarks>
public sealed class Phase1ArtifactValidator
{
    private readonly RunContext _run;
    private readonly ActivitySource _activitySource;
    private readonly string _repoRoot;

    public Phase1ArtifactValidator(RunContext run, ActivitySource activitySource, string repoRoot)
    {
        _run = run;
        _activitySource = activitySource;
        _repoRoot = repoRoot;
    }

    /// <summary>
    /// Validiert die Pflichtartefakte und liefert den bekannten Exit-Code zurück.
    /// </summary>
    /// <returns>0 bei Erfolg, 2 wenn mindestens ein Pflichtartefakt fehlt.</returns>
    public int ValidateRequiredDocs()
    {
        var missing = Phase1Artifacts.RequiredDocs
            .Where(rel => !File.Exists(Path.Combine(_repoRoot, rel.Replace('/', Path.DirectorySeparatorChar))))
            .ToList();

        WriteAuditSpan(missing);

        if (missing.Count == 0)
            return 0;

        _run.AppendEvent(new
        {
            type = "RUN_FAILED",
            runId = _run.RunId,
            reason = "Missing required docs",
            missing,
            timestampUtc = DateTime.UtcNow
        });

        WriteDiagnosis(missing);

        Console.Error.WriteLine("RUN FAILED - missing required docs:");
        foreach (var missingDoc in missing)
            Console.Error.WriteLine($" - {missingDoc}");

        return 2;
    }

    private void WriteAuditSpan(IReadOnlyCollection<string> missing)
    {
        /*
         * Der Audit-Span fasst die fachliche Abschlussprüfung technisch zusammen. 
         * OTel zeigt damit nicht nur Chat- und Tool-Spans, 
         * sondern auch, ob die erwarteten Artefakte nach dem Run wirklich existieren.
         */
        using var auditSpan = _activitySource.StartActivity("validation.audit", ActivityKind.Internal);

        auditSpan?.SetTag("run.id", _run.RunId);
        auditSpan?.SetTag("phase", Phase1Artifacts.PhaseName);
        auditSpan?.SetTag("artifact.expected.count", Phase1Artifacts.RequiredDocs.Length);
        auditSpan?.SetTag("artifact.missing.count", missing.Count);
        auditSpan?.SetTag("artifact.expected.paths", string.Join(",", Phase1Artifacts.RequiredDocs));
        auditSpan?.SetTag("artifact.present.paths", string.Join(",", Phase1Artifacts.RequiredDocs.Except(missing)));
        auditSpan?.SetTag("artifact.missing.paths", string.Join(",", missing));

        foreach (var missingDoc in missing)
        {
            auditSpan?.AddEvent(new ActivityEvent(
                "artifact.missing",
                tags: new ActivityTagsCollection { { "artifact.path", missingDoc } }));
        }

        auditSpan?.SetStatus(
            missing.Count > 0 ? ActivityStatusCode.Error : ActivityStatusCode.Ok,
            missing.Count > 0 ? "Missing required docs" : null);
    }

    private void WriteDiagnosis(IReadOnlyCollection<string> missing)
    {
        var diagnosis = new
        {
            runId = _run.RunId,
            phase = Phase1Artifacts.PhaseName,
            status = "FAILED",
            rootCause = new
            {
                code = "VALIDATION_FAILED",
                message = "Missing required docs after agent run.",
                missing
            },
            hints = new[]
            {
                "Check OTel spans: tool.* with outcome=denied/error/cancelled",
                "If no tool.fs_write spans exist for missing paths -> NO_ATTEMPT/PLAN_DRIFT category"
            },
            timestampUtc = DateTime.UtcNow
        };

        File.WriteAllText(
            Path.Combine(_run.LogsDir, "diagnosis.json"),
            JsonSerializer.Serialize(diagnosis, new JsonSerializerOptions { WriteIndented = true })
        );
    }
}
