using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// T3.5 — deterministischer Apply der VERIFIZIERTEN (menschlich freigegebenen) Reverse-Ops in den Core.
// E4-Invariante: PBI-Status `done` entsteht AUSSCHLIESSLICH hier, ueber ein freigegebenes PBI_DONE — nie aus
// issue-closed allein. Mapping-Statuswechsel laufen ueber T3.1 (CoreGithubMapping.Close). Core nur ueber den Port.
public static class GithubReverseApply
{
    public static (ProjectStateDocument Core, GithubReverseApplyReport Report) Apply(
        ProjectStateDocument core,
        GithubReversePlanDocument plan,
        ISet<string> acceptedOpIds)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var order = core.Items.Select(i => i.ItemId).ToList();
        var mappingCloses = new List<GithubMappingOp>();

        var markedDone = new List<string>();
        var mappingsClosed = new List<string>();
        var flagged = new List<string>();
        var skipped = new List<string>();

        for (var i = 0; i < plan.Operations.Count; i++)
        {
            var op = plan.Operations[i];
            if (!acceptedOpIds.Contains($"op-{i}")) { skipped.Add($"op-{i} {op.PbiId} ({op.Kind}) nicht freigegeben"); continue; }
            if (!byId.TryGetValue(op.PbiId, out var pbi)) { skipped.Add($"op-{i} {op.PbiId} nicht im Core"); continue; }

            switch (op.Kind)
            {
                case GithubReverseKind.PbiDone:
                {
                    // E4: done nur hier, nach Freigabe. History bewahrt den vorherigen Zustand.
                    var history = (pbi.History ?? []).Append(new ProjectStateItemVersion(
                        pbi.Version, pbi.Text, pbi.Status, pbi.Origin, pbi.SourceRunId, pbi.SourceClaimIds, DateTime.UtcNow,
                        $"done via GitHub #{op.IssueNumber} (verifiziert/freigegeben)")).ToList();
                    byId[pbi.ItemId] = pbi with { Status = "done", Version = pbi.Version + 1, History = history };
                    mappingCloses.Add(new GithubMappingOp(op.PbiId, op.IssueNumber, null, null, GithubMappingKind.Close, "REVERSE_DONE"));
                    markedDone.Add($"{op.PbiId} (#{op.IssueNumber})");
                    break;
                }
                case GithubReverseKind.MappingSyncClosed:
                    mappingCloses.Add(new GithubMappingOp(op.PbiId, op.IssueNumber, null, null, GithubMappingKind.Close, "REVERSE_SYNC"));
                    mappingsClosed.Add($"{op.PbiId} (#{op.IssueNumber})");
                    break;
                case GithubReverseKind.FlagReopened:
                    // Bewusst KEIN Auto-Change — nur als bearbeitet vermerkt.
                    flagged.Add($"{op.PbiId} (#{op.IssueNumber})");
                    break;
                default:
                    skipped.Add($"op-{i} {op.PbiId} unbekannt {op.Kind}");
                    break;
            }
        }

        var updated = core with
        {
            SchemaVersion = ProjectStateDocument.CurrentSchemaVersion,
            Items = order.Select(id => byId[id]).ToList()
        };
        // Mapping-Statuswechsel ueber T3.1 (idempotent, gleiche Relation).
        if (mappingCloses.Count > 0) (updated, _) = CoreGithubMapping.Apply(updated, mappingCloses);

        var accepted = markedDone.Count + mappingsClosed.Count + flagged.Count;
        return (updated, new GithubReverseApplyReport(accepted, markedDone, mappingsClosed, flagged, skipped));
    }
}
