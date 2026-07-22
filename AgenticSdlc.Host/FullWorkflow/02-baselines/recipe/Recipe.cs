using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Recipes;

/// <summary>
/// Deklaratives Rezept (§10): beschreibt EINEN Lauf über drei Achsen — (1) Baseline-Modus (build|load) + welche
/// Artefakte, (2) welche Ableitungen (0..N, je eine <see cref="Derivation.DerivationSpec"/>-Id). Der
/// <see cref="RecipeRunner"/> montiert daraus zur Laufzeit den MAF-Graphen (kein handgeschriebener Workflow je Fall).
/// Bewusst spec-referenziert (nicht frei-inline): jede Quell/Ziel-Kombination ist eine benannte, reproduzierbare
/// Behauptung (Forschungs-Warnschild §10: Mechanismus 1..N, gemessen nur wenige Kombinationen).
/// </summary>
public sealed record Recipe(
    [property: JsonPropertyName("baseline")] RecipeBaseline Baseline,
    [property: JsonPropertyName("derivations")] IReadOnlyList<RecipeDerivation>? Derivations);

/// <summary>Baseline-Quelle des Rezepts: frisch erzeugen (build → Fan-out) ODER laden (load → aus fromRun).</summary>
public sealed record RecipeBaseline(
    [property: JsonPropertyName("mode")] string? Mode,               // "build" | "load"
    [property: JsonPropertyName("artifacts")] IReadOnlyList<string>? Artifacts,
    [property: JsonPropertyName("fromRun")] string? FromRun = null); // nur mode:load (Pfad ODER runId unter runs/)

/// <summary>Eine Ableitung im Rezept: verweist auf eine registrierte <see cref="Derivation.DerivationSpec"/>-Id;
/// deren SourceArtifactTypes müssen ⊆ baseline.artifacts sein.</summary>
public sealed record RecipeDerivation(
    [property: JsonPropertyName("spec")] string Spec);
