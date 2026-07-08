using System.Text.RegularExpressions;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;

/// <summary>
/// I-a — das deterministische ID-Gate. Wandelt ein geprüftes Baseline-Artefakt (Markdown, Listenzeilen mit
/// <c>[claimId]</c>-Zitaten) in ein <see cref="ArtifactDocument"/> mit STABILEN, strukturierten Item-IDs um.
/// KEIN LLM, reine C#-Logik — der closed-world-prüfbare Baustein, der Artefakt-zu-Artefakt-Provenienz (und später
/// den GitHub-Issue-Marker) überhaupt erst ermöglicht.
/// </summary>
/// <remarks>
/// Bewusst „id-pro-Zeile" (IST_Soll §6 I-a): jede fachliche Zeile wird ein <see cref="ArtifactItem"/> mit stabiler
/// <c>itemId</c> — KEINE Markdown-Zeilennummer (die bei Formatänderung driftet), sondern ein persistierbares Label
/// <c>{PREFIX}-{NN}</c>. Zitierte Claim-IDs werden aus dem Zeilentext extrahiert (<c>sourceClaimIds</c>) und aus dem
/// <c>text</c> entfernt. <c>sourceArtifactItemIds</c> bleibt leer (Extraktion hat keinen Upstream-Artefakt-Anker;
/// das füllt später die Inferenz-Schicht). <c>artifactId</c> = Typ-Präfix (Dokument-Ebene, ein Arbeitsobjekt je
/// Artefakttyp im ersten Stand); projekt-eindeutige Nummerierung (<c>REQ-0012</c>) ist eine spätere Verfeinerung,
/// sobald ein Project State existiert. Der Parser spiegelt <see cref="MakerChecker.ContractChecker"/> (Bullet +
/// <c>[ids]</c>) — hier lokal gehalten, um das MC0-Nachweis-Modul NICHT zu modifizieren.
/// </remarks>
public static class ArtifactIdGate
{
    private static readonly Regex BulletRx = new(@"^\s*[-*]\s+", RegexOptions.Compiled);
    private static readonly Regex IdRx = new(@"\[([a-zA-Z0-9_\-, ]+)\]", RegexOptions.Compiled);

    // Artefakttyp -> Item-ID-Präfix (= Dokument-Arbeitsobjekt). Additiv erweiterbar.
    private static readonly IReadOnlyDictionary<string, string> Prefixes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["requirements"] = "REQ",
            ["risks"] = "RISK",
            ["architecture"] = "ARCH",
            ["open-questions"] = "OQ",
        };

    /// <summary>Vergibt stabile IDs für jede Listenzeile und liefert das strukturierte <see cref="ArtifactDocument"/>
    /// (= E-e <c>artifact.json</c>). Deterministisch: gleiche Eingabe → gleiche IDs.</summary>
    public static ArtifactDocument Assign(string markdown, string artifactType, ProducerMetadata producer)
    {
        var prefix = Prefixes.TryGetValue(artifactType, out var p) ? p : artifactType.Trim().ToUpperInvariant();

        var items = new List<ArtifactItem>();
        var raw = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var n = 0;
        foreach (var line in raw)
        {
            if (!BulletRx.IsMatch(line)) continue;
            n++;
            items.Add(new ArtifactItem(
                ItemId: $"{prefix}-{n:D2}",
                Origin: ArtifactOrigin.Extracted,
                Text: CleanText(line),
                SourceClaimIds: ExtractIds(line),
                SourceArtifactItemIds: []));
        }

        return new ArtifactDocument(
            ArtifactId: prefix,
            ArtifactType: artifactType,
            Version: 1,
            Stage: ArtifactDocument.StageEvidenceBaseline,
            Producer: producer,
            Items: items);
    }

    /// <summary>Zeilentext ohne führenden Bullet-Marker und ohne <c>[claimId]</c>-Zitate.</summary>
    private static string CleanText(string line)
    {
        var noBullet = BulletRx.Replace(line, string.Empty);
        var noIds = IdRx.Replace(noBullet, string.Empty);
        return noIds.Trim();
    }

    private static IReadOnlyList<string> ExtractIds(string line)
    {
        var ids = new List<string>();
        foreach (Match m in IdRx.Matches(line))
            foreach (var tok in m.Groups[1].Value.Split(','))
            {
                var id = tok.Trim();
                if (id.Length == 0) continue;
                if (Regex.IsMatch(id, @"^\d+$")) continue; // reine Zahl = keine Claim-ID (Fußnote)
                ids.Add(id);
            }
        return ids;
    }
}
