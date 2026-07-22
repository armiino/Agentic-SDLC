using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.3 — Tools des Forward-Makers fuer den agentischen Teil: je UNMAPPED (nicht blockiertem) PBI entweder
// LINK (nach Suche ein plausibler Kandidat gefunden) ODER CREATE_ISSUE (Suche ausgefuehrt, kein Kandidat).
// Die GitHub-READ-Tools (T3.2, list/get/search/labels) werden im Runner dazukomponiert — sie liefern den
// Beleg, dass der Maker tatsaechlich gesucht hat. save_forward_plan haelt die Rev-3-Invariante (CREATE braucht
// searchedQueries + searchEvidence); check_forward_plan gibt dem Maker vorab dieselbe Rueckmeldung.
internal sealed class GithubForwardTools(
    IReadOnlyList<GithubSyncEntry> unmappedPbis,
    ProjectStateDocument core,
    IReadOnlyList<GithubIssueSnapshot> issues,
    RunContext run)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private readonly Dictionary<string, ProjectStateItem> _byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
    private readonly HashSet<int> _issueNumbers = issues.Select(i => i.IssueNumber).ToHashSet();
    private IReadOnlyList<GithubForwardOp>? _saved;

    public bool Saved => _saved is not null;
    public IReadOnlyList<GithubForwardOp> SavedOps => _saved ?? [];

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetUnmappedPbis, "get_unmapped_pbis",
            "Die PBIs OHNE bekanntes GitHub-Mapping (nicht blockiert). Fuer jedes: erst search_issues, dann LINK oder CREATE_ISSUE."),
        AIFunctionFactory.Create(GetPbi, "get_pbi",
            "Liest ein PBI (Titel, goal, acceptanceCriteria) inkl. der abgedeckten Requirement-Texte — Basis fuer Suche und Issue-Body."),
        AIFunctionFactory.Create(CheckForwardPlan, "check_forward_plan",
            "Prueft deine geplanten Ops deterministisch (Rev-3: CREATE braucht searchedQueries+searchEvidence; LINK-Ziel muss existieren). Vor dem Speichern nutzen."),
        AIFunctionFactory.Create(SaveForwardPlan, "save_forward_plan",
            "Speichert je unmapped PBI GENAU EIN Op: LINK (targetIssueNumber + anchor) ODER CREATE_ISSUE (title+body+searchedQueries+searchEvidence). Genau einmal."),
    ];

    private string GetUnmappedPbis()
    {
        var rows = unmappedPbis.Select(e => new
        {
            pbiId = e.PbiId,
            title = e.Title,
            status = e.Status,
            readiness = e.Readiness,
            coveredRequirementIds = e.CoveredRequirementIds
        }).ToArray();
        run.AppendEvent(new { type = "GITHUB_FWD_UNMAPPED", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string GetPbi(string pbiId)
    {
        var id = (pbiId ?? string.Empty).Trim();
        if (!_byId.TryGetValue(id, out var it) || it.Pbi is null) return $"UNKNOWN_PBI: {id}";
        var coveredTexts = core.Relations
            .Where(r => string.Equals(r.RelationType, "covers", StringComparison.Ordinal) && string.Equals(r.FromId, id, StringComparison.Ordinal))
            .Select(r => _byId.TryGetValue(r.ToId, out var req) ? new { requirementId = req.ItemId, text = req.Text } : null)
            .Where(x => x is not null)
            .ToArray();
        run.AppendEvent(new { type = "GITHUB_FWD_GET_PBI", runId = run.RunId, pbiId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { it.ItemId, it.Status, it.Pbi, coveredRequirements = coveredTexts }, Json);
    }

    private string CheckForwardPlan(GithubForwardOp[] ops)
    {
        var issuesFound = Validate(ops ?? []);
        run.AppendEvent(new { type = "GITHUB_FWD_CHECK", runId = run.RunId, ops = ops?.Length ?? 0, problems = issuesFound.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { ok = issuesFound.Count == 0, problems = issuesFound }, Json);
    }

    private string SaveForwardPlan(GithubForwardOp[] ops)
    {
        if (Saved) return "ALREADY_SAVED: save_forward_plan darf nur einmal aufgerufen werden.";
        _saved = (ops ?? []).Select(Normalize).ToList();
        run.AppendEvent(new { type = "GITHUB_FWD_SAVE", runId = run.RunId, ops = _saved.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, ops = _saved.Count }, Json);
    }

    // Dieselben Kernregeln, die spaeter das Gate prueft — fuer Selbstkorrektur des Makers.
    private List<string> Validate(IReadOnlyList<GithubForwardOp> ops)
    {
        var problems = new List<string>();
        foreach (var op in ops)
        {
            if (!GithubForwardKind.Agentic.Contains(op.Kind) && !string.Equals(op.Kind, GithubForwardKind.NoChange, StringComparison.Ordinal))
                problems.Add($"{op.PbiId}: kind '{op.Kind}' hier nicht erlaubt (LINK|CREATE_ISSUE).");
            if (string.Equals(op.Kind, GithubForwardKind.CreateIssue, StringComparison.Ordinal))
            {
                if (op.SearchedQueries is null || op.SearchedQueries.Count == 0 || string.IsNullOrWhiteSpace(op.SearchEvidence))
                    problems.Add($"{op.PbiId}: CREATE_ISSUE braucht searchedQueries + searchEvidence (Rev-3).");
                if (string.IsNullOrWhiteSpace(op.Title) || string.IsNullOrWhiteSpace(op.Body))
                    problems.Add($"{op.PbiId}: CREATE_ISSUE braucht Titel und Body.");
            }
            if (string.Equals(op.Kind, GithubForwardKind.Link, StringComparison.Ordinal))
            {
                if (op.TargetIssueNumber is null) problems.Add($"{op.PbiId}: LINK braucht targetIssueNumber.");
                else if (!_issueNumbers.Contains(op.TargetIssueNumber.Value)) problems.Add($"{op.PbiId}: LINK-Ziel #{op.TargetIssueNumber} nicht im Snapshot.");
            }
        }
        return problems;
    }

    private static GithubForwardOp Normalize(GithubForwardOp op)
        => op with
        {
            Origin = "agent",
            Labels = op.Labels ?? [],
            SearchedQueries = op.SearchedQueries ?? []
        };
}
