using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

// ---- Edge-Payloads der generischen Derivation (Generate → Anker → Check) ----

/// <summary>
/// Multi-Source-Eingabe der Derivation: 1..N geprüfte Quell-Artefakte, aus deren VEREINIGTER Item-Menge abgeleitet
/// wird. Einzelquelle = Set der Länge 1 (rückwärtskompatibel). Zugleich der Workflow-Eingabetyp (<see cref="DerivationGenerateExecutor"/>).
/// </summary>
public sealed record SourceArtifactSet(IReadOnlyList<ArtifactDocument> Sources)
{
    /// <summary>Bequemer Wrapper für Einzelquelle.</summary>
    public static SourceArtifactSet Of(ArtifactDocument source) => new([source]);

    /// <summary>Vereinigte Anker-Grundmenge (ID → Item) über alle Quellen; bei ID-Kollision gewinnt die erste Quelle.</summary>
    public IReadOnlyDictionary<string, ArtifactItem> ItemsById()
    {
        var map = new Dictionary<string, ArtifactItem>(StringComparer.Ordinal);
        foreach (var it in Sources.SelectMany(s => s.Items)) map.TryAdd(it.ItemId, it);
        return map;
    }

    /// <summary>Gesamtzahl der Quell-Items über alle Quellen (Diagnose/Log).</summary>
    public int TotalItemCount => Sources.Sum(s => s.Items.Count);
}

/// <summary>Ein vom Generator-Agenten erzeugtes Roh-Item (vor Anker-Validierung + ID-Vergabe). Generisch für JEDE
/// Ableitung (Risiken, Gap-Requirements, User-Stories …).</summary>
public sealed record RawDerivedItem(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("sourceArtifactItemIds")] IReadOnlyList<string> SourceArtifactItemIds,
    [property: JsonPropertyName("assumptions")] IReadOnlyList<string> Assumptions,
    [property: JsonPropertyName("rationale")] string Rationale);

/// <summary>Ungültig verankertes Item (Anker fehlt/zeigt ins Leere) — Audit, nicht ins konsumierbare Dokument.</summary>
public sealed record InvalidAnchor(string Text, IReadOnlyList<string> Anchors, IReadOnlyList<string> BadIds, string Reason);

public sealed record GeneratedDerivation(SourceArtifactSet Sources, IReadOnlyList<RawDerivedItem> Raw, string Decision);
public sealed record AnchoredDerivation(SourceArtifactSet Sources, IReadOnlyList<ArtifactItem> Items, IReadOnlyList<InvalidAnchor> Invalid, string Decision);

/// <summary>Terminales Ergebnis der Derivation: das (anker-gültige) abgeleitete Dokument + I-c-Verdikte + die
/// verworfenen Anker. Als YieldOutput-Typ zugleich der Ausgabetyp beim Binden per BindAsExecutor.</summary>
public sealed record DerivationResult(
    [property: JsonPropertyName("document")] ArtifactDocument Document,
    [property: JsonPropertyName("verdicts")] IReadOnlyList<InferenceVerdict> Verdicts,
    [property: JsonPropertyName("invalidAnchors")] IReadOnlyList<InvalidAnchor> InvalidAnchors,
    [property: JsonPropertyName("decision")] string Decision);

/// <summary>
/// Verallgemeinerte Derivation als bindbarer MAF-Workflow: <c>Generate (echter AIAgent) → AnchorValidate
/// (deterministisch) → InferenceCheck (bounded Judge)</c>. Eine Ableitung ist ein <see cref="DerivationSpec"/>;
/// derselbe Graph läuft für jede (Risiken, Gap-Requirements, …). Per <c>WithOutputFrom</c> + <c>BindAsExecutor</c>
/// als EIN Knoten in größere Graphen einhängbar.
/// </summary>
public static class DerivationWorkflow
{
    internal static Microsoft.Agents.AI.Workflows.Workflow Build(
        DerivationGenerateExecutor generate,
        DerivationAnchorExecutor anchor,
        DerivationCheckExecutor check)
    {
        var builder = new WorkflowBuilder(generate)
            .WithName($"Derivation-{generate.Spec.Id}")
            .WithDescription($"{generate.Spec.SourceLabel} → {generate.Spec.TargetArtifactType} "
                           + "(Generate[Agent] → AnchorValidate[det] → InferenceCheck[Judge]).");

        builder.AddEdge(generate, anchor);
        builder.AddEdge(anchor, check);
        builder.WithOutputFrom(check);
        return builder.Build();
    }
}
