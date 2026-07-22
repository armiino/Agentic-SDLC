using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// Stufe 0: liefert alle AKTIVEN Requirement-Entitaeten als Kandidaten. Bei ~69 Items ist der Agent
// selbst der semantische Matcher (kein Index noetig) - siehe plan-core-ingestion §7. incomingText wird
// hier (bewusst) ignoriert; spaetere Stufen nutzen ihn zum Filtern/Ranken.
public sealed class ShowAllRequirementRetriever : ICandidateRetriever
{
    private static readonly HashSet<string> Inactive = new(StringComparer.OrdinalIgnoreCase) { "superseded", "retired" };

    public IReadOnlyList<ProjectStateItem> GetCandidates(string incomingText, ProjectStateDocument core)
        => core.Items
            .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase))
            .Where(i => !Inactive.Contains(i.Status))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();
}
