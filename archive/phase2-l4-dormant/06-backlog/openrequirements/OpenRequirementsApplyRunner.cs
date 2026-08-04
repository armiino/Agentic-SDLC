using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class OpenRequirementsApplyRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        var auditPath = OpenRequirementsReviewRunner.ResolveAuditPath(repoRoot, args[1]);
        if (auditPath is null)
        {
            Console.Error.WriteLine($"[open-requirements-apply] operationalization-audit nicht gefunden: {args[1]}");
            return 2;
        }

        var allowUnreviewed = args.Contains("--allow-unreviewed", StringComparer.OrdinalIgnoreCase);
        var auditDir = Path.GetDirectoryName(auditPath) ?? repoRoot;
        var decisionsPath = Path.Combine(auditDir, "open-requirement-decisions.json");
        if (!File.Exists(decisionsPath) && !allowUnreviewed)
        {
            Console.Error.WriteLine("[open-requirements-apply] open-requirement-decisions.json fehlt. Nutze --allow-unreviewed nur fuer technische Durchstiche.");
            return 2;
        }

        var audit = await LoadAsync<OperationalizationAuditDocument>(auditPath).ConfigureAwait(false);
        var decisions = File.Exists(decisionsPath)
            ? await LoadAsync<OpenRequirementHumanDecisionsFile>(decisionsPath).ConfigureAwait(false)
            : BuildDefaultDecisions(ParentRunId(auditPath), audit);
        var result = Apply(repoRoot, auditPath, audit, decisions, allowUnreviewed);

        var outDir = Path.Combine(auditDir, "applied");
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "accepted-open-requirement-decisions.json"), JsonSerializer.Serialize(decisions, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "clarification-planning-input.json"), JsonSerializer.Serialize(result.Input, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "open-requirements-apply-report.json"), JsonSerializer.Serialize(result.Report, Json)).ConfigureAwait(false);

        Console.WriteLine($"[open-requirements-apply] reviewed={result.Report.Reviewed}/{result.Report.SourceOpenRequirements} missing={result.Report.MissingDecisions.Count} clarificationItems={result.Report.ClarificationItems}");
        var decisionsText = string.Join(", ", result.Report.DecisionsByType.Select(kv => kv.Key + "=" + kv.Value));
        Console.WriteLine($"[open-requirements-apply] decisions={decisionsText}");
        Console.WriteLine($"[open-requirements-apply] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return result.Report.MissingDecisions.Count == 0 || allowUnreviewed ? 0 : 1;
    }

    private static OpenRequirementsApplyResult Apply(
        string repoRoot,
        string auditPath,
        OperationalizationAuditDocument audit,
        OpenRequirementHumanDecisionsFile decisions,
        bool allowUnreviewed)
    {
        var openRequirements = audit.Requirements.Where(r => !r.InIssuePlanningInput).ToArray();
        var decisionsById = decisions.Decisions
            .Where(d => !string.IsNullOrWhiteSpace(d.RequirementId))
            .GroupBy(d => d.RequirementId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var missing = allowUnreviewed
            ? []
            : openRequirements.Select(r => r.RequirementId).Where(id => !decisionsById.ContainsKey(id)).Order(StringComparer.Ordinal).ToArray();
        var clarificationItems = openRequirements
            .Where(r => decisionsById.TryGetValue(r.RequirementId, out var decision) && CreatesClarification(decision.Decision))
            .Select((r, index) => ToClarificationItem(r, decisionsById[r.RequirementId], index + 1))
            .ToArray();
        var report = new OpenRequirementApplyReport(
            RunId: decisions.RunId,
            SourceAuditPath: Path.GetRelativePath(repoRoot, auditPath),
            SourceOpenRequirements: openRequirements.Length,
            Reviewed: decisionsById.Count,
            MissingDecisions: missing,
            DecisionsByType: decisions.Decisions.GroupBy(d => d.Decision, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key.ToLowerInvariant(), g => g.Count(), StringComparer.Ordinal),
            ClarificationItems: clarificationItems.Length,
            TimestampUtc: DateTime.UtcNow);
        var input = new ClarificationPlanningInput(
            SchemaVersion: ClarificationPlanningInput.CurrentSchemaVersion,
            ProjectId: audit.ProjectId,
            BaselineId: audit.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceAuditPath: Path.GetRelativePath(repoRoot, auditPath),
            Items: clarificationItems);
        return new OpenRequirementsApplyResult(input, report);
    }

    private static ClarificationPlanningInputItem ToClarificationItem(OperationalizationRequirementAudit requirement, OpenRequirementHumanDecision decision, int index)
        => new(
            ClarificationId: $"CLAR-{index:D3}",
            SourceRequirementId: requirement.RequirementId,
            Title: string.IsNullOrWhiteSpace(decision.IssueTitle) ? $"Klaerung: {requirement.Title}" : decision.IssueTitle!,
            Question: string.IsNullOrWhiteSpace(decision.Question) ? $"Welche Entscheidung ist fuer {requirement.RequirementId} erforderlich?" : decision.Question!,
            ClarificationType: string.IsNullOrWhiteSpace(decision.ClarificationType) ? "decision" : decision.ClarificationType!,
            Priority: string.IsNullOrWhiteSpace(decision.Priority) ? "medium" : decision.Priority!,
            Readiness: requirement.Readiness,
            RequirementTitle: requirement.Title,
            Rationale: decision.Reason);

    private static bool CreatesClarification(string? decision)
        => decision is not null
           && (decision.Equals("create_clarification_issue", StringComparison.OrdinalIgnoreCase)
               || decision.Equals("break_down_required", StringComparison.OrdinalIgnoreCase)
               || decision.Equals("keep_open_decision", StringComparison.OrdinalIgnoreCase));

    private static OpenRequirementHumanDecisionsFile BuildDefaultDecisions(string runId, OperationalizationAuditDocument audit)
    {
        var session = OpenRequirementsReviewAdapter.BuildSession(runId, audit, readiness: null, scope: "open");
        return OpenRequirementsReviewAdapter.Apply(runId, session);
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string ParentRunId(string auditPath)
    {
        var dir = Path.GetDirectoryName(auditPath) ?? auditPath;
        var applied = Directory.GetParent(dir)?.FullName ?? dir;
        var plan = Directory.GetParent(applied)?.FullName ?? applied;
        var run = Directory.GetParent(plan)?.FullName ?? plan;
        return Path.GetFileName(run);
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: open-requirements-apply <operationalization-audit.json|github-reconciliation-runId> [--allow-unreviewed]");
    }

    private sealed record OpenRequirementsApplyResult(ClarificationPlanningInput Input, OpenRequirementApplyReport Report);
}
