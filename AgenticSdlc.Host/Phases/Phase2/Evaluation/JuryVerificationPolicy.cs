namespace AgenticSdlc.Host.Phases.Phase2.Evaluation;

/// <summary>
/// Steuert, welche Fehlerkategorien des Synthese-Judges einen zweiten, kategoriespezifischen
/// Verifikations-Pass bekommen (DISK-7). Pro Kategorie ein eigener Batch-LLM-Call, aber nur,
/// wenn die Kategorie aktiviert ist UND es Kandidaten dieser Kategorie gibt.
/// </summary>
/// <remarks>
/// Hintergrund (DISK-7): Bis zur Einführung war nur <c>MISSING_TOPIC</c> verifiziert; FALSE_CERTAINTY
/// ging ungefiltert in den Score. Genau dort entstanden die meisten False Positives (das Artefakt
/// markiert ein Thema bereits als offen/optional, der Judge zählt es trotzdem als „zu sicher").
/// Belegt an Runs 20260615_154916_4d4d66 / 20260615_160140_807b7d.
///
/// Einheitliches Verdikt-Vokabular für ALLE Kategorien:
///   confirmed = Finding ist real (zählt voll),
///   partial   = teilweise gültig (zählt mit reduzierter Severity-Gewichtung),
///   rejected  = False Positive (zählt nicht, bleibt transparent sichtbar).
///
/// Konservative Defaults: <c>MissingTopic=true</c> (bewährt), <c>FalseCertainty=true</c>
/// (größter FP-Treiber), <c>FalseClaim=false</c> (teurer, da Transkript-Re-Read; Verifier
/// arbeitet bewusst konservativ — siehe Evaluator.FalseClaimVerificationPrompt).
/// </remarks>
/// <param name="Custom">
/// Wenn true: ein EINZIGER ausgelagerter Custom-Verifier (Prompt
/// <c>Prompts/jury/verify-custom.txt</c>) prüft ALLE Kandidaten kategorie-übergreifend und
/// ERSETZT die drei eingebauten Verifier. Die drei Kategorie-Flags werden dann ignoriert,
/// der FALSE_CERTAINTY-Precheck ist aus. Nur zum Experimentieren/Kalibrieren — nicht für A/B/C.
/// </param>
/// <param name="MissingTopicBatchSize">
/// DISK-9: Max. MISSING_TOPIC-Kandidaten pro Verifikations-Call. Größere Listen werden in mehrere
/// Calls aufgeteilt (Index-Remapping pro Chunk). Verhindert den Verifier-Kollaps bei großen Batches
/// (Run 639979: 23/23 fälschlich confirmed). Default 8. Gilt NUR für MISSING_TOPIC.
/// </param>
public sealed record JuryVerificationPolicy(
    bool FalseClaim, bool FalseCertainty, bool MissingTopic, bool Custom = false, int MissingTopicBatchSize = 8)
{
    /// <summary>Legacy-Verhalten vor DISK-7: nur MISSING_TOPIC verifizieren.</summary>
    public static JuryVerificationPolicy MissingOnly { get; } = new(FalseClaim: false, FalseCertainty: false, MissingTopic: true);

    /// <summary>DISK-7-Default: MISSING_TOPIC + FALSE_CERTAINTY, FALSE_CLAIM aus.</summary>
    public static JuryVerificationPolicy Default { get; } = new(FalseClaim: false, FalseCertainty: true, MissingTopic: true);

    public bool AnyEnabled => Custom || FalseClaim || FalseCertainty || MissingTopic;

    /// <summary>Ist die Verifikation für den (Synthese-)Kategorie-Key aktiv? (Im Custom-Modus irrelevant.)</summary>
    public bool IsEnabled(string categoryKey) => categoryKey switch
    {
        "FALSE_CLAIM" => FalseClaim,
        "FALSE_CERTAINTY" => FalseCertainty,
        "MISSING_TOPIC" => MissingTopic,
        _ => false
    };
}
