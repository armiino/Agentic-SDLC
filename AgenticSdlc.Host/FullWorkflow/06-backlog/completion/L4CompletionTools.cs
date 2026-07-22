using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

internal sealed class L4AdequacyTools(L4CompletionInput input, RunContext run)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly Dictionary<string, CanonicalRequirement> _requirementsById =
        input.Baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
    private readonly List<L4AdequacyReport> _savedReports = [];

    public bool Saved => _savedReports.Count > 0;
    public L4AdequacyReport? SavedReport => _savedReports.LastOrDefault();

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetDocumentOverview, "get_requirements_document_overview",
            "Liefert Uebersicht, Abschnittsueberschriften und Kennzahlen des aktuellen Requirements-Dokuments."),
        AIFunctionFactory.Create(ListRequirements, "list_requirements",
            "Listet Requirements mit Status, Readiness und kurzer Beschreibung."),
        AIFunctionFactory.Create(GetRequirement, "get_requirement",
            "Liest ein einzelnes Requirement inklusive Readiness und Provenance."),
        AIFunctionFactory.Create(GetRequirementsDocumentExcerpt, "get_requirements_document_excerpt",
            "Liest einen Ausschnitt aus dem Requirements-Dokument."),
        AIFunctionFactory.Create(SaveAdequacyReport, "save_adequacy_report",
            "Speichert dein finales Adequacy-Feedback. Rufe dies genau einmal am Ende auf.")
    ];

    private string GetDocumentOverview()
    {
        var headings = input.RequirementsDocumentText
            .Split('\n')
            .Where(l => l.StartsWith("## ", StringComparison.Ordinal))
            .Select(l => l.Trim())
            .ToArray();
        var readinessSummary = input.Readiness?.Summary;
        run.AppendEvent(new { type = "L4_ADEQUACY_TOOL_OVERVIEW", runId = run.RunId, headings = headings.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new
        {
            input.Baseline.ProjectId,
            input.Baseline.BaselineId,
            requirements = input.Baseline.Requirements.Count,
            openDecisions = input.Baseline.OpenDecisions.Count,
            readiness = readinessSummary,
            headings
        }, Json);
    }

    private string ListRequirements(string status = "", int limit = 120)
    {
        var readinessById = input.Readiness?.Items.ToDictionary(i => i.RequirementId, StringComparer.Ordinal)
                            ?? new Dictionary<string, RequirementReadinessItem>(StringComparer.Ordinal);
        var rows = input.Baseline.Requirements
            .Where(r => string.IsNullOrWhiteSpace(status) || string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => SortKey(r.RequirementId), StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 200))
            .Select(r => new
            {
                r.RequirementId,
                r.Title,
                r.Status,
                readiness = readinessById.GetValueOrDefault(r.RequirementId)?.Readiness ?? "unknown",
                sourceItemIds = r.SourceItemIds,
                text = Truncate(r.Text, 420)
            })
            .ToArray();
        run.AppendEvent(new { type = "L4_ADEQUACY_TOOL_LIST_REQUIREMENTS", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetRequirement(string requirementId)
    {
        var id = (requirementId ?? string.Empty).Trim();
        if (!_requirementsById.TryGetValue(id, out var requirement)) return $"UNKNOWN_REQUIREMENT: {id}";
        var readiness = input.Readiness?.Items.FirstOrDefault(i => string.Equals(i.RequirementId, id, StringComparison.Ordinal));
        var provenance = input.Provenance?.Requirements.FirstOrDefault(p => string.Equals(p.RequirementId, id, StringComparison.Ordinal));
        run.AppendEvent(new { type = "L4_ADEQUACY_TOOL_GET_REQUIREMENT", runId = run.RunId, requirementId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { requirement, readiness, provenance }, Json);
    }

    private string GetRequirementsDocumentExcerpt(int startLine = 1, int lineCount = 120)
    {
        var lines = input.RequirementsDocumentText.Split('\n');
        var start = Math.Clamp(startLine, 1, Math.Max(1, lines.Length));
        var count = Math.Clamp(lineCount, 1, 240);
        var excerpt = lines.Skip(start - 1).Take(count).Select((line, idx) => new { line = start + idx, text = line }).ToArray();
        run.AppendEvent(new { type = "L4_ADEQUACY_TOOL_DOC_EXCERPT", runId = run.RunId, startLine = start, lineCount = excerpt.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(excerpt, Json);
    }

    private string SaveAdequacyReport(L4AdequacyFinding[] findings, string overallAssessment)
    {
        if (Saved) return "ALREADY_SAVED: save_adequacy_report darf nur einmal aufgerufen werden.";
        var report = new L4AdequacyReport(
            SchemaVersion: L4AdequacyReport.CurrentSchemaVersion,
            ReportId: $"l4-adequacy-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.Baseline.ProjectId,
            BaselineId: input.Baseline.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceRequirementsDocumentPath: input.SourceRequirementsDocumentPath,
            Findings: (findings ?? []).ToList(),
            OverallAssessment: overallAssessment ?? "");
        _savedReports.Add(report);
        run.AppendEvent(new { type = "L4_ADEQUACY_TOOL_SAVE", runId = run.RunId, findings = report.Findings.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, findings = report.Findings.Count }, Json);
    }

    private static string Truncate(string value, int max)
    {
        var text = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return text.Length <= max ? text : text[..max] + "...";
    }

    private static string SortKey(string itemId)
    {
        var prefix = new string(itemId.TakeWhile(c => !char.IsDigit(c)).ToArray());
        var digits = new string(itemId.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
        return $"{prefix}{(int.TryParse(digits, out var n) ? n : 0):D6}:{itemId}";
    }
}

internal sealed class L4CompletionTools(L4CompletionInput input, L4AdequacyReport adequacy, RunContext run)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly Dictionary<string, CanonicalRequirement> _requirementsById =
        input.Baseline.Requirements.ToDictionary(r => r.RequirementId, StringComparer.Ordinal);
    private readonly Dictionary<string, L4AdequacyFinding> _findingsById =
        adequacy.Findings.ToDictionary(f => f.FindingId, StringComparer.Ordinal);
    private readonly List<L4CompletionProposalDocument> _savedDocuments = [];
    private int _checkRounds;

    public bool Saved => _savedDocuments.Count > 0;
    public L4CompletionProposalDocument? SavedDocument => _savedDocuments.LastOrDefault();
    public int CheckRounds => _checkRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetAdequacyReport, "get_adequacy_report",
            "Liefert das Adequacy-Feedback, gegen das du Completion-Vorschlaege erzeugst."),
        AIFunctionFactory.Create(GetRequirement, "get_requirement",
            "Liest ein Requirement inklusive Readiness und Provenance."),
        AIFunctionFactory.Create(CheckCompletionProposals, "check_completion_proposals",
            "Prueft CompletionProposalItems deterministisch. Nutze dies vor dem Speichern."),
        AIFunctionFactory.Create(SaveCompletionProposals, "save_completion_proposals",
            "Speichert deine finalen CompletionProposalItems. Rufe dies genau einmal am Ende auf.")
    ];

    private string GetAdequacyReport()
    {
        run.AppendEvent(new { type = "L4_COMPLETION_TOOL_GET_ADEQUACY", runId = run.RunId, findings = adequacy.Findings.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(adequacy, Json);
    }

    private string GetRequirement(string requirementId)
    {
        var id = (requirementId ?? string.Empty).Trim();
        if (!_requirementsById.TryGetValue(id, out var requirement)) return $"UNKNOWN_REQUIREMENT: {id}";
        var readiness = input.Readiness?.Items.FirstOrDefault(i => string.Equals(i.RequirementId, id, StringComparison.Ordinal));
        var provenance = input.Provenance?.Requirements.FirstOrDefault(p => string.Equals(p.RequirementId, id, StringComparison.Ordinal));
        run.AppendEvent(new { type = "L4_COMPLETION_TOOL_GET_REQUIREMENT", runId = run.RunId, requirementId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { requirement, readiness, provenance }, Json);
    }

    private string CheckCompletionProposals(L4CompletionProposalItem[] items)
    {
        var round = Interlocked.Increment(ref _checkRounds);
        var doc = BuildDocument(items ?? []);
        var report = L4CompletionGate.Check(input, adequacy, doc);
        run.AppendEvent(new
        {
            type = "L4_COMPLETION_TOOL_CHECK",
            runId = run.RunId,
            round,
            items = doc.Items.Count,
            pass = report.Pass,
            errors = report.Errors.Count,
            warnings = report.Warnings.Count,
            timestampUtc = DateTime.UtcNow
        });
        return JsonSerializer.Serialize(report, Json);
    }

    private string SaveCompletionProposals(L4CompletionProposalItem[] items)
    {
        if (Saved) return "ALREADY_SAVED: save_completion_proposals darf nur einmal aufgerufen werden.";
        var doc = BuildDocument(items ?? []);
        _savedDocuments.Add(doc);
        run.AppendEvent(new { type = "L4_COMPLETION_TOOL_SAVE", runId = run.RunId, items = doc.Items.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, items = doc.Items.Count }, Json);
    }

    private L4CompletionProposalDocument BuildDocument(IReadOnlyList<L4CompletionProposalItem> items)
        => new(
            SchemaVersion: L4CompletionProposalDocument.CurrentSchemaVersion,
            ProposalId: $"l4-completion-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.Baseline.ProjectId,
            BaselineId: input.Baseline.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceAdequacyReportId: adequacy.ReportId,
            SourceRequirementsDocumentPath: input.SourceRequirementsDocumentPath,
            Items: items.ToList());
}
