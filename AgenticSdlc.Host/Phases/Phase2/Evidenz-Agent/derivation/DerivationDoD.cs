using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// DEFINITION OF DONE (Verify-Arm) — der NEUTRALE, deterministische Scorecard auf die vom Agenten selbst-für-fertig
/// erklärte Ausgabe. Der Host ERZWINGT nichts (kein Zwangs-Rückschleife → das würde den Gate messen, nicht die
/// Agency); er STEMPELT nur, ob die DoD erfüllt ist. Der Befund ist die Lücke „Agent sagt fertig" ↔ „DoD erfüllt?".
/// </summary>
/// <remarks>
/// Vier deterministisch prüfbare Kriterien (die Treue-Dimension ② kommt aus dem unabhängigen Inference-Check):
/// ① ≥1 echter Anker · ② Verdikt = supported · ③ Cross-Artifact (≥2 Artefakttypen, weil die Aufgabe „Zusammenspiel"
/// req×arch ist) · ④ rationale vorhanden. Bewusst KEIN Judge hier — der Scorecard bleibt reproduzierbar.
/// </remarks>
public static class DerivationDoD
{
    public static DoDReport Evaluate(ArtifactDocument doc, SourceArtifactSet sources, IReadOnlyList<InferenceVerdict> verdicts)
    {
        var typeById = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var src in sources.Sources)
            foreach (var it in src.Items) typeById[it.ItemId] = src.ArtifactType;
        var verdictById = verdicts.ToDictionary(v => v.ItemId, v => v.Verdict, StringComparer.Ordinal);

        var perItem = new List<DoDItem>(doc.Items.Count);
        foreach (var it in doc.Items)
        {
            var anchors = it.SourceArtifactItemIds ?? [];
            var existing = anchors.Where(typeById.ContainsKey).ToList();
            var types = existing.Select(a => typeById[a]).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            var anchorOk = anchors.Count > 0 && existing.Count == anchors.Count;
            var supported = verdictById.TryGetValue(it.ItemId, out var v) && v == InferenceVerdictKind.Supported;
            var crossArtifact = types >= 2;
            var rationale = !string.IsNullOrWhiteSpace(it.DerivationRationale);
            perItem.Add(new DoDItem(it.ItemId, anchorOk, supported, crossArtifact, rationale,
                Pass: anchorOk && supported && crossArtifact && rationale));
        }

        var pass = doc.Items.Count > 0 && perItem.All(p => p.Pass);
        // Verletzte Kriterien zählen (für die Fehler-Zusammenfassung).
        var failures = new Dictionary<string, int>
        {
            ["anchorOk"] = perItem.Count(p => !p.AnchorOk),
            ["supported"] = perItem.Count(p => !p.Supported),
            ["crossArtifact"] = perItem.Count(p => !p.CrossArtifact),
            ["rationale"] = perItem.Count(p => !p.Rationale),
        };
        return new DoDReport(pass, doc.Items.Count, perItem.Count(p => p.Pass), failures, perItem);
    }
}

public sealed record DoDItem(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("anchorOk")] bool AnchorOk,
    [property: JsonPropertyName("supported")] bool Supported,
    [property: JsonPropertyName("crossArtifact")] bool CrossArtifact,
    [property: JsonPropertyName("rationale")] bool Rationale,
    [property: JsonPropertyName("pass")] bool Pass);

public sealed record DoDReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("passed")] int Passed,
    [property: JsonPropertyName("failuresByCriterion")] IReadOnlyDictionary<string, int> FailuresByCriterion,
    [property: JsonPropertyName("perItem")] IReadOnlyList<DoDItem> PerItem);
