using AgenticSdlc.Host.FullWorkflow.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Ingestion;

/// <summary>
/// R-44/C5-Schritt2 (09.08.2026) — der DATEI-Vertrag des ingest-/arch-ingest-Gates für den resume-Responder
/// (symmetrisch zu github-forward-decisions.json): bis heute konnte das Tor 1 beim resume NUR per
/// accept-all-Flag beantwortet werden — per-Item-Entscheide gingen ausschließlich inline/UI. Diese Datei
/// (geschrieben von UI ODER Steward-Chat) macht den Entscheid durabel: Items ohne apply-Eintrag werden
/// NICHT übernommen (Gates sind heilig — nichts rutscht still durch).
/// </summary>
public sealed record IngestGateDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<IngestGateDecision> Decisions);

public sealed record IngestGateDecision(
    [property: JsonPropertyName("incomingItemId")] string IncomingItemId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("reason")] string? Reason = null);

public static class IngestGateDecisions
{
    public const string FileName = "ingest-gate-decisions.json";

    /// <summary>
    /// 1d/R-57 (18.08.): DIE EINE Lese-Stelle für Tor-1-Entscheide — Chat-Vertrag (ingest-gate-decisions.json)
    /// ODER UI-Vertrag (human-decisions.json), schema-gleich, deshalb EIN Zieltyp. Vorher lasen der
    /// R-35-Ablehnungs-Rekorder und der Standalone-Runner am Vertrag vorbei nur die UI-Datei —
    /// Chat-Ablehnungen fehlten im Projekt-Gedächtnis (R-57) und die Werkbank wies Chat-Läufe ab (N3-Fund).
    /// </summary>
    public static IngestionHumanDecisionsFile? TryLoadAnyFile(string stageDir)
        => LoadFile(Path.Combine(stageDir, FileName))
           ?? LoadFile(Path.Combine(stageDir, "human-decisions.json"));

    private static IngestionHumanDecisionsFile? LoadFile(string path)
        => File.Exists(path)
            ? JsonSerializer.Deserialize<IngestionHumanDecisionsFile>(File.ReadAllText(path), FullWorkflow.JsonFiles.Json)
            : null;

    /// <summary>3b-2-Brücke (Responder-Sicht): akzeptierte Ids + Reviewer aus BEIDEN Verträgen.</summary>
    public static (IReadOnlyList<string> Accepted, string Reviewer)? TryLoadAny(string stageDir)
        => TryLoadAnyFile(stageDir) is { } f ? (Accepted(f.Decisions), f.Reviewer) : null;

    /// <summary>null = keine Datei (Responder fällt auf Flags/Pause zurück).</summary>
    public static (IReadOnlyList<string> Accepted, string Reviewer)? TryLoad(string path)
        => LoadFile(path) is { } f ? (Accepted(f.Decisions), f.Reviewer) : null;

    private static IReadOnlyList<string> Accepted(IReadOnlyList<IngestionHumanDecision> decisions)
        => decisions.Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
                    .Select(d => d.IncomingItemId).ToList();
}
