using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

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
                    // E4: done nur hier, nach Freigabe. §5-S3: zentrale Status-Naht (neue Felder + Alt-String synchron;
                    // die History-Notiz bewahrt wie zuvor den vorherigen Zustand).
                    byId[pbi.ItemId] = pbi
                        .WithStatus(CoreStatus.From("done"), $"done via GitHub #{op.IssueNumber} (verifiziert/freigegeben)")
                        with { Version = pbi.Version + 1 };
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
