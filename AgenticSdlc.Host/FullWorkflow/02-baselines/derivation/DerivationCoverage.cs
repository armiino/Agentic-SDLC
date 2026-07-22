using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Derivation;

/// <summary>
/// COVERAGE-RECHENSCHAFT (Accountable-Arm) — deterministischer, CLOSED-WORLD-Scorecard über den GEGEBENEN Input:
/// ist jedes Quell-Item am Ende ENTWEDER Anker eines Risikos (covered) ODER bewusst mit Grund verworfen (accounted)?
/// Was übrig bleibt, ist <c>unaccounted</c> — unbehandelt. Das beantwortet „warum nicht mehr Risiken?" und macht den
/// Arm-Vergleich interpretierbar (weniger Risiken bei sauberer Begründung ≠ übersehen).
/// </summary>
/// <remarks>
/// WICHTIG (Abgrenzung zu „completeness deferred"): dies misst NICHT, ob der Agent alle real existierenden Risiken der
/// WELT fand (open-world, weiter verboten), sondern nur, ob er jeden GEGEBENEN Input rechenschaftspflichtig behandelt
/// hat (closed-world, endlich, valide). KEINE Mengen-Quote — nur Rechenschaft.
/// </remarks>
public static class DerivationCoverage
{
    public static CoverageReport Evaluate(
        SourceArtifactSet sources, IReadOnlyList<Artifacts.ArtifactItem> derived,
        IReadOnlyCollection<string> accounted, IReadOnlyList<Dismissal> dismissals)
    {
        var all = sources.ItemsById().Keys.ToHashSet(StringComparer.Ordinal);
        var covered = derived.SelectMany(i => i.SourceArtifactItemIds ?? []).Where(all.Contains).ToHashSet(StringComparer.Ordinal);
        // dismissedRaw = alle vom Agenten via account_uncovered verworfenen (gültigen) Items — SEINE Rohbehauptung.
        var dismissedRaw = accounted.Where(all.Contains).ToHashSet(StringComparer.Ordinal);
        // KOLLISION = Item, das der Agent GLEICHZEITIG als Anker nutzt UND verwirft. Verletzt die Disjunktheit der
        // Rechenschaft: der Host zieht es still auf „covered", aber es ist ein Signal für pauschales Gruppen-Verwerfen.
        var collisions = dismissedRaw.Where(covered.Contains).OrderBy(x => x, StringComparer.Ordinal).ToList();
        // covered gewinnt bei Überlappung (ein tatsächlich genutztes Item gilt nicht als „verworfen").
        var accountedOnly = dismissedRaw.Where(a => !covered.Contains(a)).ToHashSet(StringComparer.Ordinal);
        var unaccounted = all.Where(a => !covered.Contains(a) && !accountedOnly.Contains(a)).OrderBy(x => x, StringComparer.Ordinal).ToList();
        // coverageComplete = host-disjungierte Sicht (jedes Item behandelt). selfAccountingClean = die EHRLICHE Sicht:
        // der Agent selbst hat lückenlos UND kollisionsfrei abgerechnet (nichts unbehandelt, nichts doppelt).
        return new CoverageReport(
            Total: all.Count, Covered: covered.Count, Accounted: accountedOnly.Count, Unaccounted: unaccounted.Count,
            CoverageComplete: unaccounted.Count == 0,
            DismissedRaw: dismissedRaw.Count, Collisions: collisions.Count,
            SelfAccountingClean: unaccounted.Count == 0 && collisions.Count == 0,
            UnaccountedItemIds: unaccounted, CollisionItemIds: collisions, Dismissals: dismissals);
    }
}

/// <summary>Eine bewusst nicht-abgeleitete Item-GRUPPE mit gemeinsamem Grund (Tool <c>account_uncovered</c>).</summary>
public sealed record Dismissal(
    [property: JsonPropertyName("itemIds")] IReadOnlyList<string> ItemIds,
    [property: JsonPropertyName("reason")] string Reason);

public sealed record CoverageReport(
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("covered")] int Covered,
    [property: JsonPropertyName("accounted")] int Accounted,
    [property: JsonPropertyName("unaccounted")] int Unaccounted,
    [property: JsonPropertyName("coverageComplete")] bool CoverageComplete,
    // dismissedRaw = wie viele Items der Agent ROH verworfen hat (vor Disjungierung); collisions = davon auch als Anker
    // genutzt (Disjunktheits-Verletzung). selfAccountingClean = die EHRLICHE Bilanz (nichts unbehandelt UND nichts doppelt).
    [property: JsonPropertyName("dismissedRaw")] int DismissedRaw,
    [property: JsonPropertyName("collisions")] int Collisions,
    [property: JsonPropertyName("selfAccountingClean")] bool SelfAccountingClean,
    [property: JsonPropertyName("unaccountedItemIds")] IReadOnlyList<string> UnaccountedItemIds,
    [property: JsonPropertyName("collisionItemIds")] IReadOnlyList<string> CollisionItemIds,
    [property: JsonPropertyName("dismissals")] IReadOnlyList<Dismissal> Dismissals);
