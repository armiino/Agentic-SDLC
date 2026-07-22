using AgenticSdlc.Host.Prompts;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation;

/// <summary>
/// Alle ausgelagerten Judge-Prompts der Jury, aus Dateien geladen (statt als const im Code).
/// </summary>
/// <remarks>
/// Liegen unter <c>AgenticSdlc.Host/Prompts/jury/*.txt</c> sichtbar versioniert wie die Agent-Prompts.
///  Der Synthese-Haupt-Judge wird pro Artefakt aus Header + Profil + Body zusammengesetzt 
/// (<see cref="BuildSynthesisPrompt"/>); die Verifier sind je ein Prompt.
/// Der <c>Custom</c>-Verifier ist nur aktiv, wenn <c>jury.verification.custom = true</c>.
/// </remarks>
public sealed record JuryPrompts(
    string SynthesisHeader,
    string SynthesisBody,
    string ProfileRequirements,
    string ProfileRisks,
    string ProfileArchitecture,
    string ProfileOpenQuestions,
    string ProfileGeneric,
    string MissingVerification,
    string FalseCertaintyVerification,
    string FalseClaimVerification,
    string CustomVerification,
    string GenFalseClaim,
    string GenFalseCertainty,
    string GenMissingTopic)
{
    /// <summary>Setzt den Synthese-Haupt-Judge-Prompt für ein konkretes Artefakt zusammen (3-in-1-Call).</summary>
    public string BuildSynthesisPrompt(string artifactName)
        => SynthesisHeader + "\n\n" + ProfileFor(artifactName) + "\n\n" + SynthesisBody;

    /// <summary>
    /// DISK-12/G3: Setzt den fokussierten Generierungs-Prompt für EINE Kategorie zusammen
    /// (Header + Profil + kategorie-spezifischer Body). Header/Profil bleiben mit dem 3-in-1-Call
    /// geteilt; nur der Body fokussiert auf eine Kategorie -> höherer Recall schwacher Modelle (B20).
    /// </summary>
    public string BuildCategoryGenerationPrompt(string artifactName, string categoryKey)
    {
        var body = categoryKey switch
        {
            "FALSE_CLAIM" => GenFalseClaim,
            "FALSE_CERTAINTY" => GenFalseCertainty,
            "MISSING_TOPIC" => GenMissingTopic,
            _ => throw new ArgumentOutOfRangeException(
                nameof(categoryKey), categoryKey, "Kein Generierungs-Body für diese Kategorie.")
        };
        return SynthesisHeader + "\n\n" + ProfileFor(artifactName) + "\n\n" + body;
    }

    /// <summary>Wählt das artefakt-spezifische Profil per Dateiname (wie zuvor Evaluator.ArtifactProfile).</summary>
    public string ProfileFor(string artifactName)
    {
        var name = artifactName.ToLowerInvariant();
        if (name.Contains("requirement")) return ProfileRequirements;
        if (name.Contains("risk")) return ProfileRisks;
        if (name.Contains("arch")) return ProfileArchitecture;
        if (name.Contains("open") || name.Contains("question")) return ProfileOpenQuestions;
        return ProfileGeneric;
    }
}

/// <summary>Lädt <see cref="JuryPrompts"/> aus <c>AgenticSdlc.Host/Prompts/jury/</c>.</summary>
public static class JuryPromptLoader
{
    private const string JuryPromptDir = "AgenticSdlc.Host/Prompts/jury";
    private static readonly IReadOnlyDictionary<string, string> NoVariables =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public static JuryPrompts Load(string repoRoot)
        => new(
            SynthesisHeader: Read(repoRoot, "synthesis-header"),
            SynthesisBody: Read(repoRoot, "synthesis-body"),
            ProfileRequirements: Read(repoRoot, "profile-requirements"),
            ProfileRisks: Read(repoRoot, "profile-risks"),
            ProfileArchitecture: Read(repoRoot, "profile-architecture"),
            ProfileOpenQuestions: Read(repoRoot, "profile-open-questions"),
            ProfileGeneric: Read(repoRoot, "profile-generic"),
            MissingVerification: Read(repoRoot, "verify-missing-topic"),
            FalseCertaintyVerification: Read(repoRoot, "verify-false-certainty"),
            FalseClaimVerification: Read(repoRoot, "verify-false-claim"),
            CustomVerification: Read(repoRoot, "verify-custom"),
            GenFalseClaim: Read(repoRoot, "gen-false-claim"),
            GenFalseCertainty: Read(repoRoot, "gen-false-certainty"),
            GenMissingTopic: Read(repoRoot, "gen-missing-topic"));

    private static string Read(string repoRoot, string promptName)
        => PromptTemplateLoader.Load(repoRoot, JuryPromptDir, promptName, NoVariables);
}
