using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.Gap;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

public static class ProjectStateBuilder
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<ProjectStateDocument> BuildAsync(
        string repoRoot,
        IReadOnlyList<string> artifactPaths,
        string? l3RunDir,
        string? projectId = null,
        CancellationToken ct = default)
    {
        var sources = new List<ProjectStateSource>();
        var items = new List<ProjectStateItem>();
        var relations = new List<ProjectStateRelation>();
        var provenance = new List<ProjectStateProvenance>();
        var proposals = new List<ProjectStateProposal>();

        foreach (var path in artifactPaths.Select(p => Resolve(repoRoot, p)))
        {
            var artifact = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), Json)
                           ?? throw new InvalidOperationException($"Artifact konnte nicht gelesen werden: {path}");
            var sourceId = $"artifact:{artifact.ArtifactId}:{artifact.Producer.RunId}";
            sources.Add(new ProjectStateSource(sourceId, "artifact", Rel(repoRoot, path), artifact.Producer.RunId,
                $"{artifact.ArtifactType}/{artifact.Stage}"));

            foreach (var item in artifact.Items)
            {
                items.Add(FromArtifactItem(item, artifact));
                AddArtifactRelations(item, relations);
                provenance.Add(FromArtifactProvenance(item, artifact, sourceId));
            }
        }

        if (!string.IsNullOrWhiteSpace(l3RunDir))
            await AddL3Async(repoRoot, ResolveRunDir(repoRoot, l3RunDir!), sources, items, relations, provenance, proposals, ct).ConfigureAwait(false);

        return new ProjectStateDocument(
            ProjectId: string.IsNullOrWhiteSpace(projectId) ? "default" : projectId!,
            SchemaVersion: ProjectStateDocument.CurrentSchemaVersion,
            CreatedUtc: DateTime.UtcNow,
            Sources: sources,
            Items: items.OrderBy(i => i.ItemId, StringComparer.Ordinal).ToList(),
            Relations: relations.DistinctBy(r => $"{r.FromId}\u001f{r.ToId}\u001f{r.RelationType}\u001f{r.Source}").ToList(),
            Provenance: provenance.OrderBy(p => p.ItemId, StringComparer.Ordinal).ToList(),
            Proposals: proposals.OrderBy(p => p.ProposalId, StringComparer.Ordinal).ToList());
    }

    private static ProjectStateItem FromArtifactItem(ArtifactItem item, ArtifactDocument artifact)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["artifactVersion"] = artifact.Version.ToString(),
            ["producerModel"] = artifact.Producer.Model
        };
        if (!string.IsNullOrWhiteSpace(item.DerivationRationale)) metadata["derivationRationale"] = item.DerivationRationale!;
        if (item.Assumptions is { Count: > 0 }) metadata["assumptions"] = string.Join("\n", item.Assumptions);

        return new ProjectStateItem(
            ItemId: item.ItemId,
            ItemType: NormalizeItemType(artifact.ArtifactType),
            Text: item.Text,
            Origin: item.Origin.ToString(),
            Stage: artifact.Stage,
            Version: artifact.Version,
            SourceRunId: artifact.Producer.RunId,
            SourceArtifactId: artifact.ArtifactId,
            SourceArtifactType: artifact.ArtifactType,
            SourceDecisionId: null,
            SourceCandidateId: null,
            SourceClaimIds: Clean(item.SourceClaimIds),
            SourceArtifactItemIds: Clean(item.SourceArtifactItemIds),
            Metadata: metadata).WithStatus(CoreStatus.From("baseline"));   // §5-S7: Status→Achsen (kein gespeichertes Feld mehr)
    }

    private static ProjectStateProvenance FromArtifactProvenance(ArtifactItem item, ArtifactDocument artifact, string sourceId)
    {
        var links = new List<ProjectStateProvenanceLink>
        {
            new("run", artifact.Producer.RunId, "produced_by", sourceId, new Dictionary<string, string>()),
            new("artifact", artifact.ArtifactId, "contained_in", sourceId, new Dictionary<string, string>
            {
                ["artifactType"] = artifact.ArtifactType,
                ["stage"] = artifact.Stage
            })
        };
        links.AddRange(Clean(item.SourceClaimIds).Select(id =>
            new ProjectStateProvenanceLink("ledger_claim", id, "evidenced_by", sourceId, new Dictionary<string, string>())));
        links.AddRange(Clean(item.SourceArtifactItemIds).Select(id =>
            new ProjectStateProvenanceLink("project_item", id, "derived_from", sourceId, new Dictionary<string, string>())));
        return new ProjectStateProvenance(item.ItemId, links);
    }

    private static void AddArtifactRelations(ArtifactItem item, List<ProjectStateRelation> relations)
    {
        foreach (var claimId in Clean(item.SourceClaimIds))
            relations.Add(new ProjectStateRelation(item.ItemId, claimId, "evidenced_by_ledger_claim", "artifact.sourceClaimIds", new Dictionary<string, string>()));
        foreach (var sourceItemId in Clean(item.SourceArtifactItemIds))
            relations.Add(new ProjectStateRelation(item.ItemId, sourceItemId, "derived_from_project_item", "artifact.sourceArtifactItemIds", new Dictionary<string, string>()));
    }

    private static async Task AddL3Async(
        string repoRoot,
        string runDir,
        List<ProjectStateSource> sources,
        List<ProjectStateItem> items,
        List<ProjectStateRelation> relations,
        List<ProjectStateProvenance> provenance,
        List<ProjectStateProposal> proposals,
        CancellationToken ct)
    {
        var runId = Path.GetFileName(runDir);
        var sourceId = $"l3:{runId}";
        sources.Add(new ProjectStateSource(sourceId, "l3-run", Rel(repoRoot, runDir), runId, "accepted L3 promotions"));

        await AddL3ProposalsAsync(repoRoot, runDir, runId, proposals, ct).ConfigureAwait(false);

        var promotedPath = Path.Combine(runDir, "promoted-items.json");
        if (!File.Exists(promotedPath)) return;
        var promoted = JsonSerializer.Deserialize<IReadOnlyList<L3PromotedItem>>(await File.ReadAllTextAsync(promotedPath, ct).ConfigureAwait(false), Json) ?? [];
        var decisions = await LoadDecisionRecordsAsync(runDir, ct).ConfigureAwait(false);
        var mappings = await LoadMappingsAsync(runDir, ct).ConfigureAwait(false);

        foreach (var item in promoted)
        {
            var decision = decisions.GetValueOrDefault(item.SourceDecisionId);
            var candidateId = item.SourceCandidateId;
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["sourceCandidateId"] = candidateId,
                ["sourceDecisionId"] = item.SourceDecisionId
            };
            if (decision is not null)
            {
                metadata["decision"] = decision.Decision;
                if (!string.IsNullOrWhiteSpace(decision.Reason)) metadata["decisionReason"] = decision.Reason!;
                metadata["reviewer"] = decision.Reviewer;
            }

            items.Add(new ProjectStateItem(
                ItemId: item.ItemId,
                ItemType: item.ItemType,
                Text: item.Text,
                Origin: item.Origin,
                Stage: "l3_promoted",
                Version: item.Version,
                SourceRunId: runId,
                SourceArtifactId: null,
                SourceArtifactType: "l3",
                SourceDecisionId: item.SourceDecisionId,
                SourceCandidateId: candidateId,
                SourceClaimIds: [],
                SourceArtifactItemIds: Clean(item.SourceArtifactItemIds),
                Metadata: metadata).WithStatus(CoreStatus.From(item.Status.ToLowerInvariant())));   // §5-S7: Status→Achsen

            relations.Add(new ProjectStateRelation(item.ItemId, item.SourceDecisionId, "accepted_by_human_decision", "l3.apply", new Dictionary<string, string>()));
            relations.Add(new ProjectStateRelation(item.ItemId, candidateId, "promoted_from_l3_candidate", "l3.apply", new Dictionary<string, string>()));
            foreach (var sourceItemId in Clean(item.SourceArtifactItemIds))
                relations.Add(new ProjectStateRelation(item.ItemId, sourceItemId, "derived_from_project_item", "l3.apply", new Dictionary<string, string>()));

            var links = new List<ProjectStateProvenanceLink>
            {
                new("run", runId, "produced_by", sourceId, new Dictionary<string, string>()),
                new("human_decision", item.SourceDecisionId, "accepted_by", sourceId, new Dictionary<string, string>()),
                new("l3_candidate", candidateId, "promoted_from", sourceId, new Dictionary<string, string>())
            };
            if (mappings.TryGetValue(candidateId, out var mapped) && !string.Equals(mapped, item.ItemId, StringComparison.Ordinal))
                links.Add(new ProjectStateProvenanceLink("project_item", mapped, "mapped_to", sourceId, new Dictionary<string, string>()));
            provenance.Add(new ProjectStateProvenance(item.ItemId, links));
        }
    }

    private static async Task AddL3ProposalsAsync(
        string repoRoot,
        string runDir,
        string runId,
        List<ProjectStateProposal> proposals,
        CancellationToken ct)
    {
        var routingPath = Path.Combine(runDir, "routing-report.json");
        if (!File.Exists(routingPath)) return;
        var routing = JsonSerializer.Deserialize<L3RoutingReport>(await File.ReadAllTextAsync(routingPath, ct).ConfigureAwait(false), Json);
        if (routing is null) return;

        var humanDecisions = await LoadHumanDecisionsAsync(runDir, ct).ConfigureAwait(false);
        var decisionRecords = await LoadDecisionRecordsByCandidateAsync(runDir, ct).ConfigureAwait(false);
        var mappings = await LoadMappingsAsync(runDir, ct).ConfigureAwait(false);
        var rejected = await LoadRejectedAsync(runDir, ct).ConfigureAwait(false);

        foreach (var routed in routing.Items)
        {
            var c = routed.Candidate;
            var humanDecision = humanDecisions.GetValueOrDefault(c.CandidateId);
            var decisionRecord = decisionRecords.GetValueOrDefault(c.CandidateId);
            mappings.TryGetValue(c.CandidateId, out var promotedItemId);
            rejected.TryGetValue(c.CandidateId, out var rejectedReason);

            var status = ProposalStatus(routed, humanDecision, promotedItemId, rejectedReason);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["candidateId"] = c.CandidateId,
                ["class"] = routed.Class.ToString(),
                ["targetType"] = c.TargetType,
                ["text"] = c.Text,
                ["requiresHumanDecision"] = c.RequiresHumanDecision?.ToString() ?? "null"
            };
            Add(metadata, "intent", c.Intent);
            Add(metadata, "gapCategory", c.GapCategory);
            Add(metadata, "impactIfMissing", c.ImpactIfMissing);
            Add(metadata, "rationale", c.Rationale);
            if (c.BasedOn is { Count: > 0 }) metadata["basedOn"] = string.Join(",", c.BasedOn);
            if (c.Assumptions is { Count: > 0 }) metadata["assumptions"] = string.Join("\n", c.Assumptions);
            if (routed.KeptAnchorIds is { Count: > 0 }) metadata["keptAnchorIds"] = string.Join(",", routed.KeptAnchorIds);
            if (routed.UnknownAnchorIds is { Count: > 0 }) metadata["unknownAnchorIds"] = string.Join(",", routed.UnknownAnchorIds);
            Add(metadata, "noAnchorReason", routed.NoAnchorReason);
            if (humanDecision is not null)
            {
                metadata["humanDecision"] = humanDecision.Decision;
                Add(metadata, "humanDecisionReason", humanDecision.Reason);
                Add(metadata, "humanEditedText", humanDecision.EditedText);
            }
            if (decisionRecord is not null)
            {
                metadata["decisionId"] = decisionRecord.DecisionId;
                metadata["reviewer"] = decisionRecord.Reviewer;
                metadata["decisionCreatedUtc"] = decisionRecord.CreatedUtc.ToString("O");
            }
            Add(metadata, "promotedItemId", promotedItemId);
            Add(metadata, "rejectedReason", rejectedReason);

            proposals.Add(new ProjectStateProposal(
                ProposalId: $"l3:{runId}:{c.CandidateId}",
                ProposalType: "l3_candidate",
                Status: status,
                SourceRunId: runId,
                PayloadPath: Rel(repoRoot, routingPath),
                Metadata: metadata));
        }
    }

    private static string ProposalStatus(
        L3RoutedCandidate routed,
        HumanDecision? humanDecision,
        string? promotedItemId,
        string? rejectedReason)
    {
        if (!string.IsNullOrWhiteSpace(promotedItemId)) return "accepted";
        if (!string.IsNullOrWhiteSpace(rejectedReason)) return "rejected";
        if (humanDecision is not null)
            return humanDecision.Decision.Trim().ToLowerInvariant() switch
            {
                "accept" or "edit" => "accepted_pending_apply",
                "reject" => "rejected_pending_apply",
                "revise" => "needs_revision",
                var other when !string.IsNullOrWhiteSpace(other) => other,
                _ => "pending_review"
            };
        return routed.Class == L3Class.SupportedAnchored ? "auto_supported" : "pending_review";
    }

    private static async Task<Dictionary<string, HumanDecision>> LoadHumanDecisionsAsync(string runDir, CancellationToken ct)
    {
        var path = Path.Combine(runDir, "human-decisions.json");
        if (!File.Exists(path)) return new Dictionary<string, HumanDecision>(StringComparer.Ordinal);
        var file = JsonSerializer.Deserialize<HumanDecisionsFile>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), Json);
        return (file?.Decisions ?? []).ToDictionary(d => d.CandidateId, StringComparer.Ordinal);
    }

    private static async Task<Dictionary<string, L3DecisionRecord>> LoadDecisionRecordsByCandidateAsync(string runDir, CancellationToken ct)
    {
        var path = Path.Combine(runDir, "decision-records.json");
        if (!File.Exists(path)) return new Dictionary<string, L3DecisionRecord>(StringComparer.Ordinal);
        var records = JsonSerializer.Deserialize<IReadOnlyList<L3DecisionRecord>>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), Json) ?? [];
        return records.ToDictionary(r => r.CandidateId, StringComparer.Ordinal);
    }

    private static async Task<Dictionary<string, string>> LoadRejectedAsync(string runDir, CancellationToken ct)
    {
        var path = Path.Combine(runDir, "rejected-candidates.json");
        if (!File.Exists(path)) return new Dictionary<string, string>(StringComparer.Ordinal);
        var records = JsonSerializer.Deserialize<IReadOnlyList<L3RejectedItem>>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), Json) ?? [];
        return records.ToDictionary(r => r.CandidateId, r => r.Reason ?? "", StringComparer.Ordinal);
    }

    private static async Task<Dictionary<string, L3DecisionRecord>> LoadDecisionRecordsAsync(string runDir, CancellationToken ct)
    {
        var path = Path.Combine(runDir, "decision-records.json");
        if (!File.Exists(path)) return new Dictionary<string, L3DecisionRecord>(StringComparer.Ordinal);
        var records = JsonSerializer.Deserialize<IReadOnlyList<L3DecisionRecord>>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), Json) ?? [];
        return records.ToDictionary(r => r.DecisionId, StringComparer.Ordinal);
    }

    private static async Task<Dictionary<string, string>> LoadMappingsAsync(string runDir, CancellationToken ct)
    {
        var path = Path.Combine(runDir, "promotion-mappings.json");
        if (!File.Exists(path)) return new Dictionary<string, string>(StringComparer.Ordinal);
        var records = JsonSerializer.Deserialize<IReadOnlyList<L3PromotionMapping>>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), Json) ?? [];
        return records.ToDictionary(r => r.CandidateId, r => r.PromotedItemId, StringComparer.Ordinal);
    }

    private static IReadOnlyList<string> Clean(IEnumerable<string>? values)
        => (values ?? []).Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct(StringComparer.Ordinal).ToArray();

    private static void Add(Dictionary<string, string> metadata, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) metadata[key] = value.Trim();
    }

    private static string NormalizeItemType(string artifactType) => artifactType.Trim().ToLowerInvariant() switch
    {
        "requirements" => "requirement",
        "architecture" => "architecture",
        "risks" => "risk",
        "open-questions" or "open_questions" => "open_question",
        var other => other
    };

    private static string Resolve(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static string ResolveRunDir(string repoRoot, string token)
    {
        var direct = Resolve(repoRoot, token);
        if (Directory.Exists(direct)) return direct;
        var l3Root = Path.Combine(repoRoot, "runs", "l3");
        var match = Directory.Exists(l3Root)
            ? Directory.EnumerateDirectories(l3Root).FirstOrDefault(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase))
            : null;
        return match ?? throw new DirectoryNotFoundException($"L3 run nicht gefunden: {token}");
    }

    private static string Rel(string repoRoot, string path)
        => Path.GetRelativePath(repoRoot, path);

    private sealed record L3RoutingReport(IReadOnlyList<L3RoutedCandidate> Items);
}
