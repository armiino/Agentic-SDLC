using AgenticSdlc.Host.FullWorkflow.Derivation;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

/// <summary>
/// Deterministische Aggregation mehrerer Anker-Urteile eines Kandidaten zu genau EINER Routing-Klasse (§2.4). Bewusst
/// im Executor (nicht im Prompt), damit die Klassifikation reproduzierbar + prüfbar ist. Reine Funktion → unit-testbar.
/// </summary>
public static class L3Routing
{
    /// <summary>
    /// Regel (§2.4), Reihenfolge = Priorität:
    /// <list type="number">
    /// <item>CONTRADICTED, wenn ≥1 existierender Anker <c>Contradicts</c> ist.</item>
    /// <item>SUPPORTED_ANCHORED, wenn ≥1 existierender Anker <c>Supported</c> ist (nicht-tragende Anker werden verworfen).</item>
    /// <item>WEAK_OR_UNCERTAIN, wenn ≥1 existierender Anker <c>Unclear</c> ist.</item>
    /// <item>UNREFERENCED sonst (keine existierenden Anker ODER alle <c>Unrelated</c>).</item>
    /// </list>
    /// UNKNOWN_ANCHOR (nicht existent, <see cref="AnchorAssessment.Exists"/> false) zählt NICHT als tragfähig; die IDs
    /// werden für das Defekt-/Repair-Audit separat gesammelt.
    /// </summary>
    public static L3RoutedCandidate Route(L3Candidate candidate, IReadOnlyList<AnchorAssessment> anchors, string? noAnchorReason)
    {
        var unknownIds = anchors.Where(a => !a.Exists).Select(a => a.ItemId).ToList();
        var judged = anchors.Where(a => a.Exists && a.Verdict is not null).ToList();

        L3Class cls;
        var kept = new List<string>();
        if (judged.Any(a => a.Verdict == InferenceVerdictKind.Contradicts))
        {
            cls = L3Class.Contradicted;
            kept = judged.Where(a => a.Verdict == InferenceVerdictKind.Contradicts).Select(a => a.ItemId).ToList();
        }
        else if (judged.Any(a => a.Verdict == InferenceVerdictKind.Supported))
        {
            cls = L3Class.SupportedAnchored;
            kept = judged.Where(a => a.Verdict == InferenceVerdictKind.Supported).Select(a => a.ItemId).ToList();
        }
        else if (judged.Any(a => a.Verdict == InferenceVerdictKind.Unclear))
        {
            cls = L3Class.WeakOrUncertain;
            kept = judged.Where(a => a.Verdict == InferenceVerdictKind.Unclear).Select(a => a.ItemId).ToList();
        }
        else
        {
            // keine existierenden Anker ODER alle Unrelated → echter Open-World-Kandidat.
            cls = L3Class.Unreferenced;
        }

        return new L3RoutedCandidate(candidate, anchors, noAnchorReason, cls, kept, unknownIds);
    }
}
