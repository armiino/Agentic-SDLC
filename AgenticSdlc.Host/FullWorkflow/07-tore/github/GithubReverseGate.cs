using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.5 — deterministisches Gate ueber den Reverse-Plan. Kern (E4): ein PBI_DONE MUSS verifikationspflichtig sein
// (requiresVerification=true) und auf ein echtes PBI zeigen — `done` darf strukturell nie an der Verifikation vorbei.
public static class GithubReverseGate
{
    public static GithubReverseGateReport Check(ProjectStateDocument core, GithubReversePlanDocument plan)
    {
        var errors = new List<GithubReverseGateIssue>();
        var warnings = new List<GithubReverseGateIssue>();

        var pbiIds = core.Items.Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var mappedPbis = CoreGithubMapping.CurrentMappings(core).Select(m => m.PbiId).ToHashSet(StringComparer.Ordinal);

        foreach (var op in plan.Operations)
        {
            if (!GithubReverseKind.All.Contains(op.Kind))
            {
                errors.Add(Issue("UNKNOWN_KIND", "error", $"Unbekannte Operation '{op.Kind}'.", op.PbiId));
                continue;
            }
            if (!pbiIds.Contains(op.PbiId))
                errors.Add(Issue("UNKNOWN_PBI", "error", $"pbiId '{op.PbiId}' ist kein PBI im Core.", op.PbiId));
            if (!mappedPbis.Contains(op.PbiId))
                errors.Add(Issue("NO_MAPPING", "error", $"PBI '{op.PbiId}' hat kein implemented_by_issue-Mapping.", op.PbiId));

            if (string.Equals(op.Kind, GithubReverseKind.PbiDone, StringComparison.Ordinal))
            {
                if (!op.RequiresVerification)
                    errors.Add(Issue("DONE_WITHOUT_VERIFICATION", "error", "PBI_DONE muss verifikationspflichtig sein (E4).", op.PbiId));
                if (!string.Equals(op.ProposedPbiStatus, "done", StringComparison.OrdinalIgnoreCase))
                    errors.Add(Issue("DONE_STATUS_INVALID", "error", "PBI_DONE muss proposedPbiStatus=done haben.", op.PbiId));
            }
        }

        // Kein PBI mehrfach.
        foreach (var g in plan.Operations.GroupBy(o => o.PbiId, StringComparer.Ordinal).Where(g => g.Count() > 1))
            errors.Add(Issue("DUPLICATE_OP", "error", $"PBI '{g.Key}' hat {g.Count()} Reverse-Ops (genau eins erlaubt).", g.Key));

        var pass = errors.Count == 0;
        return new GithubReverseGateReport(pass, pass ? "accept" : "block", errors, warnings);
    }

    private static GithubReverseGateIssue Issue(string code, string sev, string msg, string? pbi) => new(code, sev, msg, pbi);
}
