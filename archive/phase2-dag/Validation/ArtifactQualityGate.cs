namespace AgenticSdlc.Host.Phases.Phase2.Validation;

/// <summary>Verdikt des deterministischen Artefakt-Gates.</summary>
public enum ArtifactQualityVerdict
{
    /// <summary>Artefakt ist plausibel genug für die Weitergabe (Shared State) bzw. Validation.</summary>
    Valid,
    /// <summary>Datei leer oder nur Whitespace.</summary>
    Empty,
    /// <summary>Vorhanden, aber offensichtlich unbrauchbar (Placeholder/trivial kurz).</summary>
    Invalid
}

/// <summary>Ergebnis einer Gate-Prüfung: Verdikt + (bei Empty/Invalid) Begründung.</summary>
public sealed record ArtifactQualityResult(ArtifactQualityVerdict Verdict, string? Reason)
{
    public bool IsValid => Verdict == ArtifactQualityVerdict.Valid;
    public static readonly ArtifactQualityResult Valid = new(ArtifactQualityVerdict.Valid, null);
}

/// <summary>
/// Deterministisches, geteiltes Gültigkeits-Gate für SDLC-Artefakte (K1 aus todos.md DISK-5).
/// </summary>
/// <remarks>
/// ZWECK: verhindern, dass offensichtlich kaputte Artefakte (Placeholder/Testinhalt, trivial kurz) als
/// Workflow-Zwischenergebnis akzeptiert werden — in Phase 2.1B konkret, damit kein kontaminierter Inhalt
/// in den MAF-Shared-State gelangt und Downstream-Agenten verfälscht (Beleg: Run b4fb45 "# Test Write").
///
/// BEWUSST SCHMAL gehalten: nur Existenz/leer (via Aufrufer bzw. Empty), explizite Placeholder-Marker und
/// eine SEHR niedrige absolute Untergrenze. KEINE rollen-spezifische Mindestlänge, KEINE Pflichtsections —
/// das wäre Duplikation zum Validator/zur Jury und birgt False-Negative-Risiko gegen echte Formatvarianten.
///
/// ABGRENZUNG: Das Gate entscheidet nur GÜLTIGKEIT (deterministisch), nicht QUALITÄT — fachliche Fehler
/// bleiben Sache des Evaluators/der Jury. Diese Trennung darf nicht verwischt werden.
///
/// FAIRNESS: Wird sowohl in den Phase-2.1B-Executors (vor dem State-Write) als auch im gemeinsamen
/// <see cref="Phase2ArtifactValidator"/> (den A UND B am Run-Ende laufen) genutzt → identische Latte für
/// alle Strategien.
/// </remarks>
public static class ArtifactQualityGate
{
    /// <summary>
    /// Absolute Mindestlänge (Zeichen, getrimmt). Bewusst sehr niedrig: fängt nur "non-empty aber trivial"
    /// (z. B. 29-Byte-Testinhalt). Echte SDLC-Artefakte liegen weit darüber.
    /// </summary>
    public const int MinLengthChars = 80;

    // Enge Liste eindeutiger Placeholder-/Testphrasen (case-insensitive Teilstring-Treffer).
    private static readonly string[] PlaceholderMarkers =
    [
        "# test write",
        "this is a test",
        "lorem ipsum"
    ];

    /// <summary>Prüft den Artefakt-Inhalt deterministisch. Existenz der Datei prüft der Aufrufer.</summary>
    public static ArtifactQualityResult Evaluate(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new ArtifactQualityResult(ArtifactQualityVerdict.Empty, "empty or whitespace only");

        var trimmed = content.Trim();

        if (trimmed.Length < MinLengthChars)
            return new ArtifactQualityResult(
                ArtifactQualityVerdict.Invalid,
                $"trivially short ({trimmed.Length} chars < {MinLengthChars})");

        var lower = trimmed.ToLowerInvariant();
        foreach (var marker in PlaceholderMarkers)
        {
            if (lower.Contains(marker, StringComparison.Ordinal))
                return new ArtifactQualityResult(
                    ArtifactQualityVerdict.Invalid,
                    $"placeholder/test marker present: \"{marker}\"");
        }

        return ArtifactQualityResult.Valid;
    }
}
