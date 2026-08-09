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

    /// <summary>3b-2-Brücke: liest den Pipeline-Vertrag ODER die human-decisions.json der BESTEHENDEN
    /// ingest-Review-UI (schema-gleich: incomingItemId + apply|skip/reject + reason) aus dem Stufen-Ordner.</summary>
    public static (IReadOnlyList<string> Accepted, string Reviewer)? TryLoadAny(string stageDir)
        => TryLoad(Path.Combine(stageDir, FileName))
           ?? TryLoad(Path.Combine(stageDir, "human-decisions.json"));

    /// <summary>null = keine Datei (Responder fällt auf Flags/Pause zurück).</summary>
    public static (IReadOnlyList<string> Accepted, string Reviewer)? TryLoad(string path)
    {
        if (!File.Exists(path)) return null;
        var file = JsonSerializer.Deserialize<IngestGateDecisionsFile>(File.ReadAllText(path), FullWorkflow.JsonFiles.Json);
        if (file is null) return null;
        var accepted = file.Decisions
            .Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.IncomingItemId).ToList();
        return (accepted, file.Reviewer);
    }
}
