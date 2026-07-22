using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

public sealed class JsonProjectStateRepository(ProjectStateDocument document) : IProjectStateRepository
{
    private readonly List<ProjectStateProposal> _proposals = document.Proposals.ToList();

    public ProjectStateDocument Document => document with { Proposals = _proposals.ToList() };

    public static async Task<JsonProjectStateRepository> LoadAsync(string path, CancellationToken ct = default)
    {
        var json = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
        var doc = JsonSerializer.Deserialize<ProjectStateDocument>(json, ProjectStateJson.Options)
                  ?? throw new InvalidOperationException($"ProjectState konnte nicht gelesen werden: {path}");
        return new JsonProjectStateRepository(doc);
    }

    public async Task SaveAsync(string path, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(Document, ProjectStateJson.Options), ct).ConfigureAwait(false);
    }

    public ValueTask<IReadOnlyList<ProjectStateItem>> ListItemsAsync(ProjectStateItemQuery? query = null, CancellationToken ct = default)
    {
        var items = document.Items.Where(i => Matches(i, query)).ToList();
        return ValueTask.FromResult<IReadOnlyList<ProjectStateItem>>(items);
    }

    public ValueTask<ProjectStateItem?> GetItemAsync(string itemId, CancellationToken ct = default)
    {
        var item = document.Items.FirstOrDefault(i => string.Equals(i.ItemId, itemId, StringComparison.Ordinal));
        return ValueTask.FromResult(item);
    }

    public ValueTask<IReadOnlyList<ProjectStateItem>> SearchItemsAsync(string query, ProjectStateItemQuery? filter = null, CancellationToken ct = default)
    {
        var terms = (query ?? string.Empty)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var items = document.Items
            .Where(i => Matches(i, filter))
            .Where(i => terms.Length == 0 || terms.All(t => i.Text.Contains(t, StringComparison.OrdinalIgnoreCase)
                                                            || i.ItemId.Contains(t, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        return ValueTask.FromResult<IReadOnlyList<ProjectStateItem>>(items);
    }

    public ValueTask<IReadOnlyList<ProjectStateRelation>> GetRelationsAsync(string itemId, CancellationToken ct = default)
    {
        var relations = document.Relations
            .Where(r => string.Equals(r.FromId, itemId, StringComparison.Ordinal) || string.Equals(r.ToId, itemId, StringComparison.Ordinal))
            .ToList();
        return ValueTask.FromResult<IReadOnlyList<ProjectStateRelation>>(relations);
    }

    public ValueTask<ProjectStateProvenance?> GetProvenanceAsync(string itemId, CancellationToken ct = default)
    {
        var provenance = document.Provenance.FirstOrDefault(p => string.Equals(p.ItemId, itemId, StringComparison.Ordinal));
        return ValueTask.FromResult(provenance);
    }

    public ValueTask<IReadOnlyList<ProjectStateProposal>> ListProposalsAsync(string? status = null, CancellationToken ct = default)
    {
        var proposals = _proposals
            .Where(p => string.IsNullOrWhiteSpace(status) || string.Equals(p.Status, status, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return ValueTask.FromResult<IReadOnlyList<ProjectStateProposal>>(proposals);
    }

    public ValueTask<ProjectStateProposal?> GetProposalAsync(string proposalId, CancellationToken ct = default)
    {
        var proposal = _proposals.FirstOrDefault(p => string.Equals(p.ProposalId, proposalId, StringComparison.Ordinal));
        return ValueTask.FromResult(proposal);
    }

    public ValueTask SaveProposalAsync(ProjectStateProposal proposal, CancellationToken ct = default)
    {
        _proposals.RemoveAll(p => string.Equals(p.ProposalId, proposal.ProposalId, StringComparison.Ordinal));
        _proposals.Add(proposal);
        return ValueTask.CompletedTask;
    }

    public ValueTask ApplyDecisionAsync(string proposalId, string decision, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        var index = _proposals.FindIndex(p => string.Equals(p.ProposalId, proposalId, StringComparison.Ordinal));
        if (index < 0) throw new InvalidOperationException($"UNKNOWN_PROPOSAL: {proposalId}");
        var meta = new Dictionary<string, string>(_proposals[index].Metadata, StringComparer.Ordinal)
        {
            ["decision"] = decision,
            ["decisionAppliedUtc"] = DateTime.UtcNow.ToString("O")
        };
        foreach (var (key, value) in metadata ?? new Dictionary<string, string>())
            meta[key] = value;
        _proposals[index] = _proposals[index] with { Status = decision, Metadata = meta };
        return ValueTask.CompletedTask;
    }

    private static bool Matches(ProjectStateItem item, ProjectStateItemQuery? query)
    {
        if (query is null) return true;
        if (query.ItemTypes is { Count: > 0 } && !query.ItemTypes.Contains(item.ItemType)) return false;
        if (query.Statuses is { Count: > 0 } && !query.Statuses.Contains(item.Status)) return false;
        if (query.Origins is { Count: > 0 } && !query.Origins.Contains(item.Origin)) return false;
        return true;
    }
}

internal static class ProjectStateJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { WriteIndented = true };
}
