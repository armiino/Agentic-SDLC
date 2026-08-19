using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow;

/// <summary>
/// 1d Catch-up-Schalter (19.08., Block-E/L-Fund „Scope-Überraschung"): ein Diktat-Lauf zog am classify-/
/// adr-Strip den KOMPLETTEN Bestands-Rückstau nach (45 Items / 17 ADR-Entwürfe für EIN diktiertes
/// Statement — LLM-Runde + Review-Wand). Endform: der Betriebs-Lauf bearbeitet NUR die Items, die ER
/// selbst eingebracht hat (I7: sourceRunId = Auslöser-Lauf); der Rückstau-Abbau ist ein BEWUSSTER Akt
/// (`pipeline-full run --arch-catchup`) bzw. gehört dem Bootstrap-Spiegel (dessen Bahnen bleiben
/// ungescoped — sie SIND der Catch-up). Kein stiller Cap: Zurückgestelltes wird laut gezählt.
/// EINE Quelle für beide Strips (classify + adr).
/// </summary>
public static class RunScopedBacklog
{
    public static (IReadOnlyList<ProjectStateItem> InScope, int DeferredBacklog) Scope(
        IReadOnlyList<ProjectStateItem> backlog, string runId, bool catchUp)
    {
        if (catchUp) return (backlog, 0);
        var own = backlog.Where(i => string.Equals(i.SourceRunId, runId, StringComparison.Ordinal)).ToList();
        return (own, backlog.Count - own.Count);
    }

    /// <summary>Die EINE laute Ansage je Strip, wenn Rückstau bewusst liegen bleibt.</summary>
    public static string DeferredLine(string strip, int deferred)
        => $"[{strip}] {deferred} Bestands-Item(s) unklassifiziert/offen ZURÜCKGESTELLT — Rückstau-Abbau ist ein bewusster Akt: pipeline-full run --arch-catchup.";
}
