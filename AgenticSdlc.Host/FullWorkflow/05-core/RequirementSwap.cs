using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

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

/// <summary>③ E-8 (06.08.) — der GETEILTE Wahrheitsübergang für Rahmen (Analogie RequirementSwap, Zwei-Bahnen-
/// Bauregel): constrained_by zieht vom abgelösten ARCH aufs Neu-ARCH um. Alte Kante wird NIE gelöscht, sondern
/// Historie (`constrained_by_superseded` + movedTo/[decisionId]/resolvedUtc — Rev-2-Muster wie
/// contradicts_resolved). Nutzer: DecisionResolutionApply (Tor 2 ADOPT_NEW) UND PbiUpdateApply (SUPERSEDE_PBI
/// mit arch-Ziel). Idempotent gegen Doppel-Add.</summary>
public static class ConstraintSwap
{
    public const string Relation = "constrained_by";
    public const string SupersededRelation = "constrained_by_superseded";

    public static bool Swap(string pbiId, string oldArchId, string? newArchId,
        List<ProjectStateRelation> relations, string source, string? decisionId = null)
    {
        var ki = relations.FindIndex(r => string.Equals(r.RelationType, Relation, StringComparison.Ordinal)
            && string.Equals(r.FromId, pbiId, StringComparison.Ordinal)
            && string.Equals(r.ToId, oldArchId, StringComparison.Ordinal));
        if (ki < 0) return false;

        var meta = new Dictionary<string, string>(relations[ki].Metadata, StringComparer.Ordinal)
        { ["resolvedUtc"] = DateTime.UtcNow.ToString("O") };
        if (!string.IsNullOrWhiteSpace(newArchId)) meta["movedTo"] = newArchId!;
        if (!string.IsNullOrWhiteSpace(decisionId)) meta["decisionId"] = decisionId!;
        relations[ki] = relations[ki] with { RelationType = SupersededRelation, Metadata = meta };

        if (!string.IsNullOrWhiteSpace(newArchId)
            && !relations.Any(r => string.Equals(r.RelationType, Relation, StringComparison.Ordinal)
                && string.Equals(r.FromId, pbiId, StringComparison.Ordinal)
                && string.Equals(r.ToId, newArchId, StringComparison.Ordinal)))
            relations.Add(new ProjectStateRelation(pbiId, newArchId!, Relation, source, new Dictionary<string, string>()));
        return true;
    }
}
