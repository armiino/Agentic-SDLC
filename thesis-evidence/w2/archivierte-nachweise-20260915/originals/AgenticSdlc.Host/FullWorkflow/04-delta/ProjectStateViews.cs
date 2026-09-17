using AgenticSdlc.Host.FullWorkflow.Backlog;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

public sealed record ProjectScope(
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("analysisType")] string AnalysisType,
    [property: JsonPropertyName("scopeType")] string ScopeType,
    [property: JsonPropertyName("baselineId")] string? BaselineId,
    [property: JsonPropertyName("itemIds")] IReadOnlyList<string> ItemIds,
    [property: JsonPropertyName("includeMappings")] bool IncludeMappings,
    [property: JsonPropertyName("includeLatestQuality")] bool IncludeLatestQuality,
    [property: JsonPropertyName("sourcePath")] string? SourcePath)
{
    public static ProjectScope FromSourcePath(string sourcePath, string analysisType, string scopeType) =>
        new(
            ProjectId: "default",
            AnalysisType: analysisType,
            ScopeType: scopeType,
            BaselineId: null,
            ItemIds: [],
            IncludeMappings: false,
            IncludeLatestQuality: true,
            SourcePath: sourcePath);
}

public sealed record CanonicalRequirementsView(
    ProjectScope Scope,
    string SourceDirectory,
    string BaselinePath,
    string ProvenancePath,
    string QualityReportPath,
    CanonicalRequirementsBaseline Baseline,
    L4ProvenanceMap Provenance,
    L4QualityReport Quality);

public sealed record IssuePlanningView(
    ProjectScope Scope,
    string SourceDirectory,
    string IssuePlanningInputPath,
    IssuePlanningInput Input);

public sealed record AcceptedIssuePlanView(
    ProjectScope Scope,
    string SourceDirectory,
    string AcceptedIssuePlanPath,
    string GateReportPath,
    IssuePlanDocument Plan,
    IssuePlanGateReport Gate);

// Produkt des l4-re-clarify-Knotens: der Product Backlog als abgeleitete View (siehe plan-pb.md).
public sealed record ProductBacklogView(
    ProjectScope Scope,
    string SourceDirectory,
    string ProductBacklogPath,
    ProductBacklogDocument Backlog);

// Schlank seit dem Alt-Ketten-Rückbau (04.08.): nur die LEBEND konsumierten Views bleiben als Repository-Methoden
// (re-clarify + pipeline lesen Canonical; issuplanning liest IssuePlanning). AcceptedIssuePlanView/ProductBacklogView
// leben als Records weiter — re-clarify baut sie selbst (kein Repository-Getter nötig).
public interface IProjectStateViewRepository
{
    ValueTask<CanonicalRequirementsView> GetCanonicalRequirementsViewAsync(ProjectScope scope, CancellationToken ct = default);
    ValueTask<IssuePlanningView> GetIssuePlanningViewAsync(ProjectScope scope, CancellationToken ct = default);
}
