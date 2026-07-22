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

public sealed record ReadinessView(
    ProjectScope Scope,
    string SourceDirectory,
    string ReadinessReportPath,
    string IssuePlanningInputPath,
    RequirementsReadinessReport Report,
    IssuePlanningInput IssuePlanningInput);

public sealed record IssuePlanningView(
    ProjectScope Scope,
    string SourceDirectory,
    string IssuePlanningInputPath,
    IssuePlanningInput Input);

public sealed record ClarificationPlanningView(
    ProjectScope Scope,
    string SourceDirectory,
    string ClarificationPlanningInputPath,
    ClarificationPlanningInput Input);

public sealed record AcceptedIssuePlanView(
    ProjectScope Scope,
    string SourceDirectory,
    string AcceptedIssuePlanPath,
    string GateReportPath,
    IssuePlanDocument Plan,
    IssuePlanGateReport Gate);

public sealed record AcceptedClarificationPlanView(
    ProjectScope Scope,
    string SourceDirectory,
    string AcceptedClarificationPlanPath,
    string GateReportPath,
    ClarificationPlanDocument Plan,
    ClarificationPlanGateReport Gate);

// Produkt des l4-re-clarify-Knotens: der Product Backlog als abgeleitete View (siehe plan-pb.md).
public sealed record ProductBacklogView(
    ProjectScope Scope,
    string SourceDirectory,
    string ProductBacklogPath,
    ProductBacklogDocument Backlog);

public interface IProjectStateViewRepository
{
    ValueTask<CanonicalRequirementsView> GetCanonicalRequirementsViewAsync(ProjectScope scope, CancellationToken ct = default);
    ValueTask<ReadinessView> GetReadinessViewAsync(ProjectScope scope, CancellationToken ct = default);
    ValueTask<IssuePlanningView> GetIssuePlanningViewAsync(ProjectScope scope, CancellationToken ct = default);
    ValueTask<ClarificationPlanningView> GetClarificationPlanningViewAsync(ProjectScope scope, CancellationToken ct = default);
    ValueTask<AcceptedIssuePlanView> GetAcceptedIssuePlanViewAsync(ProjectScope scope, CancellationToken ct = default);
    ValueTask<AcceptedClarificationPlanView> GetAcceptedClarificationPlanViewAsync(ProjectScope scope, CancellationToken ct = default);
    ValueTask<ProductBacklogView> GetProductBacklogViewAsync(ProjectScope scope, CancellationToken ct = default);
}
