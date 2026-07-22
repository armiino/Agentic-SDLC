using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// T2.0 — der GETEILTE Wahrheitsuebergang "Requirement-Swap auf einem PBI": ein PBI deckt statt des alten (z.B.
// superseded) Requirements das Ersatz-Requirement. Extrahiert aus PbiUpdateApply (SUPERSEDE_PBI), damit Tor 2
// (ADOPT_NEW) und pbi-update GENAU EINEN Mechanismus nutzen — kein zweiter, leicht abweichender Uebergang
// (plan-tor2 Rev 2). Rein deterministisch: mutiert die uebergebene links-Liste + relations-Liste, meldet Zaehler.
public static class RequirementSwap
{
    public const string CoversRelation = "covers";

    public static ProjectStateRelation Covers(string pbiId, string reqId, string source)
        => new(pbiId, reqId, CoversRelation, source, new Dictionary<string, string>());

    public static int RemoveCovers(List<ProjectStateRelation> relations, string pbiId, string reqId)
        => relations.RemoveAll(r => string.Equals(r.RelationType, CoversRelation, StringComparison.Ordinal)
            && string.Equals(r.FromId, pbiId, StringComparison.Ordinal)
            && string.Equals(r.ToId, reqId, StringComparison.Ordinal));

    // Tauscht die Requirement-Abdeckung eines PBIs: oldReqId raus (links + covers-Relation), newReqId rein (optional).
    // Mutiert links UND relations in place; gibt (hinzugefuegt, entfernt) zurueck. Idempotent gegen Doppel-Add.
    public static (int Added, int Removed) SwapCoverage(
        string pbiId, string oldReqId, string? newReqId,
        List<string> links, List<ProjectStateRelation> relations, string source)
    {
        int added = 0, removed = 0;
        if (links.Remove(oldReqId)) removed += RemoveCovers(relations, pbiId, oldReqId);
        if (!string.IsNullOrWhiteSpace(newReqId) && !links.Contains(newReqId))
        {
            links.Add(newReqId!);
            relations.Add(Covers(pbiId, newReqId!, source));
            added++;
        }
        return (added, removed);
    }
}
