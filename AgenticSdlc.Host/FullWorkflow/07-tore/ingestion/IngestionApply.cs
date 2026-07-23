using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

public sealed record AppliedOperation(string IncomingItemId, string Kind, string? EntityId, string Outcome);

public sealed record IngestionDeltaSummary(int Added, int Refined, int Reaffirmed, int Superseded, int Contradicted, int AlreadyDecided, int Skipped);

public sealed record IngestionApplyReport(
    IReadOnlyList<AppliedOperation> Applied,
    IReadOnlyList<string> Skipped,
    IngestionDeltaSummary Delta);

// affected-view (Blast-Radius) lebt jetzt in CoreViews (Inc 1c-2) — transitiv ueber den Core-Graphen.

// Deterministischer Upsert-by-Identity der vom Menschen akzeptierten Operationen in den Core (kein LLM).
// Der Core ist die ID-Autoritaet: neue Entitaeten bekommen HIER eine stabile Core-ID (REQ-<max+1>).
public static class IngestionApply
{
    public static (ProjectStateDocument UpdatedCore, IngestionApplyReport Report, IReadOnlySet<string> AffectedIds) Apply(
        ProjectStateDocument core,
        ProjectStateDocument meetingDelta,
        StateChangePlanDocument plan,
        ISet<string> acceptedIncomingIds)
    {
        var order = core.Items.Select(i => i.ItemId).ToList();
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var relations = core.Relations.ToList();
        var incomingById = meetingDelta.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);

        var nextReq = MaxSuffix(order, "REQ") + 1;
        var nextDec = MaxSuffix(order, "DEC") + 1;

        var applied = new List<AppliedOperation>();
        var skipped = new List<string>();
        var affected = new HashSet<string>(StringComparer.Ordinal);
        int added = 0, refined = 0, reaffirmed = 0, superseded = 0, contradicted = 0, alreadyDecided = 0;

        foreach (var op in plan.Operations.Where(o => acceptedIncomingIds.Contains(o.IncomingItemId)))
        {
            if (!incomingById.TryGetValue(op.IncomingItemId, out var incoming))
            {
                skipped.Add($"{op.IncomingItemId}: incoming Item nicht im MeetingDelta");
                continue;
            }

            var claimIds = Union(op.ClaimIds, incoming.SourceClaimIds);
            var statement = string.IsNullOrWhiteSpace(op.Statement) ? incoming.Text : op.Statement;

            switch (op.Kind)
            {
                case StateChangeKind.Restate:
                {
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var t)) { skipped.Add($"{op.IncomingItemId}: RESTATE-Ziel unbekannt"); break; }
                    byId[t.ItemId] = t with { SourceClaimIds = Union(t.SourceClaimIds, op.ClaimIds) };
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, t.ItemId, "reaffirmed"));
                    affected.Add(t.ItemId); reaffirmed++;
                    break;
                }
                case StateChangeKind.Refine:
                {
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var t)) { skipped.Add($"{op.IncomingItemId}: REFINE-Ziel unbekannt"); break; }
                    var history = (t.History ?? []).Append(new ProjectStateItemVersion(
                        t.Version, t.Text, t.Status, t.Origin, t.SourceRunId, t.SourceClaimIds, DateTime.UtcNow, "REFINE (ingestion)")).ToList();
                    byId[t.ItemId] = t with
                    {
                        Text = statement,
                        Version = t.Version + 1,
                        IdentityKey = IdentityKey.From(statement),
                        SourceClaimIds = claimIds,
                        History = history
                    };
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, t.ItemId, "refined"));
                    affected.Add(t.ItemId); refined++;
                    break;
                }
                case StateChangeKind.New:
                case StateChangeKind.NewRelated:
                {
                    var id = $"REQ-{nextReq++:D2}";
                    var meta = new Dictionary<string, string>(StringComparer.Ordinal) { ["ingestedFrom"] = op.IncomingItemId };
                    if (op.Kind == StateChangeKind.NewRelated && !string.IsNullOrWhiteSpace(op.FeatureKey))
                    {
                        meta["featureKey"] = op.FeatureKey!;
                        relations.Add(new ProjectStateRelation(id, op.FeatureKey!, "part_of_feature", "ingestion", new Dictionary<string, string>()));
                    }
                    AddItem(order, byId, NewRequirement(id, statement, incoming, claimIds, meta));
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, id, "added"));
                    affected.Add(id); added++;
                    break;
                }
                case StateChangeKind.Supersede:
                {
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var t)) { skipped.Add($"{op.IncomingItemId}: SUPERSEDE-Ziel unbekannt"); break; }
                    byId[t.ItemId] = t with { Status = "superseded" };
                    var id = $"REQ-{nextReq++:D2}";
                    AddItem(order, byId, NewRequirement(id, statement, incoming, claimIds,
                        new Dictionary<string, string>(StringComparer.Ordinal) { ["ingestedFrom"] = op.IncomingItemId }));
                    relations.Add(new ProjectStateRelation(id, t.ItemId, "supersedes", "ingestion", new Dictionary<string, string>()));
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, id, "superseded"));
                    affected.Add(id); affected.Add(t.ItemId); superseded++;
                    break;
                }
                case StateChangeKind.Contradict:
                {
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var t)) { skipped.Add($"{op.IncomingItemId}: CONTRADICT-Ziel unbekannt"); break; }
                    var id = $"DEC-{nextDec++:D3}";
                    var meta = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["targetEntityId"] = t.ItemId,
                        ["ingestedFrom"] = op.IncomingItemId
                    };
                    AddItem(order, byId, new ProjectStateItem(
                        ItemId: id, ItemType: "decision", Text: $"Widerspruch zu {t.ItemId}: {statement}", Status: "open_decision",
                        Origin: "INGESTION_CONTRADICTION", Stage: null, Version: 1, SourceRunId: incoming.SourceRunId,
                        SourceArtifactId: null, SourceArtifactType: null, SourceDecisionId: null, SourceCandidateId: null,
                        SourceClaimIds: claimIds, SourceArtifactItemIds: [], Metadata: meta,
                        IdentityKey: IdentityKey.From(statement), History: []));
                    relations.Add(new ProjectStateRelation(id, t.ItemId, "contradicts", "ingestion", new Dictionary<string, string>()));
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, id, "contradicted"));
                    affected.Add(id); affected.Add(t.ItemId); contradicted++;
                    break;
                }
                case StateChangeKind.AlreadyDecided:
                {
                    // incoming ist bereits als Open Decision erfasst -> nur Provenienz/Claims an die DEC anheften (No-Op).
                    if (!byId.TryGetValue(op.TargetEntityId ?? "", out var dec) || !string.Equals(dec.ItemType, "decision", StringComparison.OrdinalIgnoreCase))
                    { skipped.Add($"{op.IncomingItemId}: ALREADY_DECIDED-Ziel ist keine Open Decision"); break; }
                    byId[dec.ItemId] = dec with { SourceClaimIds = Union(dec.SourceClaimIds, op.ClaimIds) };
                    applied.Add(new AppliedOperation(op.IncomingItemId, op.Kind, dec.ItemId, "already_decided"));
                    affected.Add(dec.ItemId); alreadyDecided++;
                    break;
                }
                default:
                    skipped.Add($"{op.IncomingItemId}: unbekannte Operation {op.Kind}");
                    break;
            }
        }

        var items = order.Select(id => byId[id]).ToList();
        var updated = core with
        {
            SchemaVersion = ProjectStateDocument.CurrentSchemaVersion,
            Items = items,
            Relations = relations
        };
        var report = new IngestionApplyReport(applied, skipped,
            new IngestionDeltaSummary(added, refined, reaffirmed, superseded, contradicted, alreadyDecided, skipped.Count));
        return (updated, report, affected);
    }

    private static ProjectStateItem NewRequirement(string id, string text, ProjectStateItem incoming, IReadOnlyList<string> claimIds, Dictionary<string, string> meta)
        => new(
            ItemId: id, ItemType: "requirement", Text: text, Status: "accepted", Origin: incoming.Origin,
            Stage: incoming.Stage, Version: 1, SourceRunId: incoming.SourceRunId, SourceArtifactId: incoming.SourceArtifactId,
            SourceArtifactType: incoming.SourceArtifactType, SourceDecisionId: null, SourceCandidateId: null,
            SourceClaimIds: claimIds, SourceArtifactItemIds: incoming.SourceArtifactItemIds, Metadata: meta,
            IdentityKey: IdentityKey.From(text), History: []);

    private static void AddItem(List<string> order, Dictionary<string, ProjectStateItem> byId, ProjectStateItem item)
    {
        byId[item.ItemId] = item;
        order.Add(item.ItemId);
    }

    private static int MaxSuffix(IEnumerable<string> ids, string prefix)
    {
        var p = prefix + "-";
        return ids.Where(id => id.StartsWith(p, StringComparison.Ordinal))
            .Select(id => id[p.Length..])
            .Where(s => s.Length > 0 && s.All(char.IsDigit))
            .Select(int.Parse)
            .DefaultIfEmpty(0)
            .Max();
    }

    // R-19: null-tolerant — Deltas ohne Claim-Provenance (sourceClaimIds=null) sind schema-legal und
    // dürfen den Apply nicht mit ArgumentNullException töten (Fund: Mini-Meeting-3, Run 150814).
    private static IReadOnlyList<string> Union(IEnumerable<string>? a, IEnumerable<string>? b)
        => (a ?? []).Concat(b ?? []).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
}
