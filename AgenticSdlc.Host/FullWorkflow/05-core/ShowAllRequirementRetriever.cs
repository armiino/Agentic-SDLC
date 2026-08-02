using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Stufe 0: liefert alle AKTIVEN Requirement-Entitaeten als Kandidaten. Bei ~69 Items ist der Agent
// selbst der semantische Matcher (kein Index noetig) - siehe plan-core-ingestion §7. incomingText wird
// hier (bewusst) ignoriert; spaetere Stufen nutzen ihn zum Filtern/Ranken.
public sealed class ShowAllRequirementRetriever : ICandidateRetriever
{
    public IReadOnlyList<ProjectStateItem> GetCandidates(string incomingText, ProjectStateDocument core)
        => core.Items
            .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase))
            .Where(i => i.ReadStatus().Validity == Validity.Active)   // §5-S4: aktiv (nicht superseded); retired war tot
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();
}
