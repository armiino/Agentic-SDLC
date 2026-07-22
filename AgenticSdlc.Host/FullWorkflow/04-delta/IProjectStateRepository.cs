namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

/// <summary>
/// Speicherneutrale fachliche Operationen auf dem Projektzustand. Implementierungen koennen JSON, SQLite oder spaeter
/// eine andere DB nutzen; L4/Issue-Planning sollen nur diese Operationen kennen.
/// </summary>
public interface IProjectStateRepository
{
    ValueTask<IReadOnlyList<ProjectStateItem>> ListItemsAsync(ProjectStateItemQuery? query = null, CancellationToken ct = default);
    ValueTask<ProjectStateItem?> GetItemAsync(string itemId, CancellationToken ct = default);
    ValueTask<IReadOnlyList<ProjectStateItem>> SearchItemsAsync(string query, ProjectStateItemQuery? filter = null, CancellationToken ct = default);
    ValueTask<IReadOnlyList<ProjectStateRelation>> GetRelationsAsync(string itemId, CancellationToken ct = default);
    ValueTask<ProjectStateProvenance?> GetProvenanceAsync(string itemId, CancellationToken ct = default);
    ValueTask<IReadOnlyList<ProjectStateProposal>> ListProposalsAsync(string? status = null, CancellationToken ct = default);
    ValueTask<ProjectStateProposal?> GetProposalAsync(string proposalId, CancellationToken ct = default);
    ValueTask SaveProposalAsync(ProjectStateProposal proposal, CancellationToken ct = default);
    ValueTask ApplyDecisionAsync(string proposalId, string decision, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken ct = default);
}
