using AgenticSdlc.Host.Run;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AgenticSdlc.Host.Phases.Phase2.Validation;

/// <summary>
///  einfache Validirung für Phase 2.1.
/// </summary>
/// <remarks>
/// Diese Klasse ist bewusst kein LLM-Reviewer oder eigener Agent.
/// Sie bewertet nicht, ob die Requirements fachlich gut sind, sondern prüft
/// objektive Mindestbedingungen nach dem MAF-Workflow:
///
/// - Existiert das Kontextartefakt?
/// - Existieren alle Pflichtartefakte?
/// - Sind die Dateien nicht leer?
/// - Enthalten Requirements die vereinbarten Mindestabschnitte?
///
/// Warum?
/// MAF stellt Workflows, Agents, Tool-Anbindung und Workflow-Events bereit.
/// Welche SDLC-Artefakte in den runs Pflicht sind und welche Mindeststruktur
/// sie haben müssen, ist aber meine eigene logik. ich will sofort sehen ob was failed oder nicht.
/// </remarks>
public sealed class Phase2ArtifactValidator
{
    // Pflicht-Sektionen aus RequirementsPrompt (aktuell v5).
    // Muss mit den RequiredMarkdownHeadings im aktiven Prompt übereinstimmen.
    //
    // Regex-Hinweis: Patterns prüfen auf Markdown-Heading-Marker (##) damit
    // "Functional Requirements" nicht fälschlicherweise durch "Non-functional Requirements"
    // erfüllt wird — ohne ## würde \bfunctional\s+requirements\b auf beides matchen,
    // weil der Bindestrich eine Wortgrenze ist.
    private static readonly RequiredSection[] RequiredRequirementsSections =
    [
        new("Functional Requirements",      @"#{1,6}\s+functional\s+requirements\b"),
        new("Non-functional Requirements",  @"#{1,6}\s+non\s*[-\s]\s*functional\s+requirements\b"),
        new("Constraints/Compliance",       @"#{1,6}\s+constraints\s*/\s*compliance\b"),
        new("Assumptions and Open Points",  @"#{1,6}\s+assumptions\s+and\s+open\s+points\b"),
        new("Traceability",                 @"#{1,6}\s+traceability\b")
    ];

    private readonly RunContext _run;
    private readonly ActivitySource _activitySource;
    private readonly string _repoRoot;

    public Phase2ArtifactValidator(RunContext run, ActivitySource activitySource, string repoRoot)
    {
        _run = run;
        _activitySource = activitySource;
        _repoRoot = repoRoot;
    }

    /// <summary>
    /// Validiert den objektiven Mindestzustand nach dem Phase-2.1-Workflow.
    /// </summary>
    /// <returns>0 bei Erfolg, 2 bei fehlgeschlagener Mindestvalidierung.</returns>
    public int Validate()
    {
        var findings = new List<Phase2ValidationFinding>();
        var files = new List<Phase2ValidatedFile>();

        ValidateFile(
            displayPath: Phase2Artifacts.ContextPath(_run).Replace('\\', '/'),
            absolutePath: Path.Combine(_repoRoot, Phase2Artifacts.ContextPath(_run)),
            role: "context",
            findings,
            files);

        foreach (var requiredDoc in Phase2Artifacts.RequiredDocs)
        {
            ValidateFile(
                displayPath: requiredDoc,
                absolutePath: Path.Combine(_repoRoot, requiredDoc.Replace('/', Path.DirectorySeparatorChar)),
                role: "required_doc",
                findings,
                files);
        }

        ValidateRequirementsSections(findings);

        var passed = findings.All(f => f.Severity != "error");
        var report = new Phase2ValidationReport(
            RunId: _run.RunId,
            Phase: Phase2Artifacts.PhaseName,
            Passed: passed,
            Files: files,
            Findings: findings,
            TimestampUtc: DateTime.UtcNow);

        WriteReport(report);
        WriteAuditSpan(report);
        WriteValidationEvent(report);

        if (passed)
            return 0;

        WriteDiagnosis(report);

        Console.Error.WriteLine("RUN FAILED - Phase 2.1 validation failed:");
        foreach (var finding in findings.Where(f => f.Severity == "error"))
            Console.Error.WriteLine($" - {finding.Code}: {finding.Path} - {finding.Message}");

        return 2;
    }

    private static void ValidateFile(
        string displayPath,
        string absolutePath,
        string role,
        List<Phase2ValidationFinding> findings,
        List<Phase2ValidatedFile> files)
    {
        if (!File.Exists(absolutePath))
        {
            files.Add(new Phase2ValidatedFile(displayPath, role, Exists: false, SizeBytes: 0));
            findings.Add(new Phase2ValidationFinding(
                Severity: "error",
                Code: "MISSING_FILE",
                Path: displayPath,
                Message: "Required Phase 2.1 file is missing."));
            return;
        }

        var info = new FileInfo(absolutePath);
        files.Add(new Phase2ValidatedFile(displayPath, role, Exists: true, SizeBytes: info.Length));

        var content = File.ReadAllText(absolutePath);
        if (string.IsNullOrWhiteSpace(content))
        {
            findings.Add(new Phase2ValidationFinding(
                Severity: "error",
                Code: "EMPTY_FILE",
                Path: displayPath,
                Message: "Required Phase 2.1 file exists but is empty."));
        }
    }

    private void ValidateRequirementsSections(List<Phase2ValidationFinding> findings)
    {
        var requirementsPath = Path.Combine(_repoRoot, "docs", "requirements.md");
        if (!File.Exists(requirementsPath))
            return;

        /*
         * Die Section-Prüfung bleibt bewusst deterministisch, wird aber gegen
         * kleine Formatvarianten robuster. Einige Modelle verwenden typografische
         * Bindestriche oder schreiben "Constraints / Compliance" mit Leerzeichen.
         * Inhaltlich ist das für Phase 2.1 dieselbe Mindeststruktur. Deshalb
         * wird hier die Schreibvariante normalisert, statt eine LLM-basierte
         * Bewertung einzuführen
         */
        var content = NormalizeForSectionMatching(File.ReadAllText(requirementsPath));
        foreach (var section in RequiredRequirementsSections)
        {
            if (Regex.IsMatch(content, section.Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                continue;

            findings.Add(new Phase2ValidationFinding(
                Severity: "error",
                Code: "MISSING_REQUIREMENTS_SECTION",
                Path: "docs/requirements.md",
                Message: $"Missing required section: {section.DisplayName}."));
        }
    }

    private static string NormalizeForSectionMatching(string value)
    {
        return value
            .Replace('\u2010', '-')
            .Replace('\u2011', '-')
            .Replace('\u2012', '-')
            .Replace('\u2013', '-')
            .Replace('\u2014', '-')
            .Replace('\u2212', '-');
    }

    private void WriteReport(Phase2ValidationReport report)
    {
        Directory.CreateDirectory(Phase2Artifacts.ValidationDir(_run));
        File.WriteAllText(
            Phase2Artifacts.ValidationReportPath(_run),
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    private void WriteAuditSpan(Phase2ValidationReport report)
    {
        using var auditSpan = _activitySource.StartActivity("validation.phase2_1", ActivityKind.Internal);

        auditSpan?.SetTag("run.id", _run.RunId);
        auditSpan?.SetTag("phase", Phase2Artifacts.PhaseName);
        auditSpan?.SetTag("validation.passed", report.Passed);
        auditSpan?.SetTag("validation.finding.count", report.Findings.Count);
        auditSpan?.SetTag("validation.error.count", report.Findings.Count(f => f.Severity == "error"));
        auditSpan?.SetTag("artifact.expected.count", Phase2Artifacts.RequiredDocs.Length + 1);
        auditSpan?.SetTag("artifact.present.count", report.Files.Count(f => f.Exists));
        auditSpan?.SetTag("validation.report.path", Phase2Artifacts.ValidationReportPath(_run));

        auditSpan?.SetStatus(
            report.Passed ? ActivityStatusCode.Ok : ActivityStatusCode.Error,
            report.Passed ? null : "Phase 2.1 validation failed");
    }

    private void WriteValidationEvent(Phase2ValidationReport report)
    {
        _run.AppendEvent(new
        {
            type = "VALIDATION_RUN",
            runId = _run.RunId,
            phase = Phase2Artifacts.PhaseName,
            passed = report.Passed,
            findingCount = report.Findings.Count,
            errorCount = report.Findings.Count(f => f.Severity == "error"),
            reportPath = Path.GetRelativePath(_run.RunDir, Phase2Artifacts.ValidationReportPath(_run)).Replace('\\', '/'),
            timestampUtc = DateTime.UtcNow
        });
    }

    private void WriteDiagnosis(Phase2ValidationReport report)
    {
        var diagnosis = new
        {
            runId = _run.RunId,
            phase = Phase2Artifacts.PhaseName,
            status = "FAILED",
            rootCause = new
            {
                code = "PHASE2_VALIDATION_FAILED",
                message = "Phase 2.1 minimum artifact validation failed.",
                findings = report.Findings
            },
            hints = new[]
            {
                "Check WORKFLOW_OUTPUT and EXECUTOR_* events to see whether the MAF workflow reached all Specialist Agents.",
                "Check TOOL_CALL_STARTED/FINISHED and FILE_WRITE_ANALYZED events to see whether the expected files were written.",
                "This validation is deterministic and does not judge deep content quality."
            },
            timestampUtc = DateTime.UtcNow
        };

        File.WriteAllText(
            Path.Combine(_run.LogsDir, "diagnosis.json"),
            JsonSerializer.Serialize(diagnosis, new JsonSerializerOptions { WriteIndented = true }));
    }
}

public sealed record Phase2ValidationReport(
    string RunId,
    string Phase,
    bool Passed,
    IReadOnlyList<Phase2ValidatedFile> Files,
    IReadOnlyList<Phase2ValidationFinding> Findings,
    DateTime TimestampUtc);

public sealed record Phase2ValidatedFile(
    string Path,
    string Role,
    bool Exists,
    long SizeBytes);

public sealed record Phase2ValidationFinding(
    string Severity,
    string Code,
    string Path,
    string Message);

internal sealed record RequiredSection(string DisplayName, string Pattern);
