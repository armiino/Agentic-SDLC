using System.Text.Json.Serialization;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Analyst;

/// <summary>
/// 1g-B (19.08.2026, core-analyst-design §5): die typisierten Verträge des Core-Analysten — der „Agent für
/// das Ungesagte". Ein FUND ist eine Hypothese mit Pflicht-Herleitung aus konkreten Core-Items; Wahrheit
/// wird er erst nach Autor-Freigabe an Tor 1 (der Analyse-Lauf selbst mutiert NICHTS).
/// </summary>
public sealed record AnalystFinding(
    [property: JsonPropertyName("linse")] string Linse,
    // requirement | architecture | question — exakt die Autor-Front-Dispositionen (gleicher Tor-1-Eingang).
    [property: JsonPropertyName("disposition")] string Disposition,
    [property: JsonPropertyName("statement")] string Statement,
    // Pflicht-Herleitung: WARUM fehlt das — mit Bezug auf die Anker-Items („abgeleitet aus REQ-88: …").
    [property: JsonPropertyName("herleitung")] string Herleitung,
    // §4-1: functional | nfr:<merkmal> | process (Merkmale ISO-25010-nah).
    [property: JsonPropertyName("kategorie")] string Kategorie,
    // Core-Anker-PFLICHT (mind. 1 existierendes Item) — der Form-Checker weist Floskeln ohne Anker ab.
    [property: JsonPropertyName("ankerIds")] IReadOnlyList<string> AnkerIds);

/// <summary>Kritiker-Urteil je Fund (Substanz-Filter; Aussortiertes bleibt SICHTBAR im Report).</summary>
public sealed record AnalystVerdict(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("behalten")] bool Behalten,
    [property: JsonPropertyName("grund")] string? Grund);

/// <summary>Ein bewerteter Fund im Report — mit Dedup-/Kritiker-Ausgang und Neuheits-Markierung.</summary>
public sealed record AnalystReportEintrag(
    [property: JsonPropertyName("fund")] AnalystFinding Fund,
    // neu | weiterhin_offen (im Vorgänger-Report schon da) | dedup_<grund> | kritiker_aussortiert
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("grund")] string? Grund = null);

public sealed record AnalystReport(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("generatedUtc")] DateTime GeneratedUtc,
    [property: JsonPropertyName("coreFingerprint")] string CoreFingerprint,
    [property: JsonPropertyName("eintraege")] IReadOnlyList<AnalystReportEintrag> Eintraege)
{
    public IReadOnlyList<AnalystReportEintrag> InsDelta =>
        [.. Eintraege.Where(e => e.Status is AnalystStatus.Neu or AnalystStatus.WeiterhinOffen)];
}

public static class AnalystStatus
{
    public const string Neu = "neu";
    public const string WeiterhinOffen = "weiterhin_offen";
    public const string KritikerAussortiert = "kritiker_aussortiert";
    public const string DedupCore = "dedup_core";              // existiert bereits als aktives Item
    public const string DedupRejection = "dedup_rejection";    // wurde schon einmal begründet abgelehnt (R-35)
    public const string DedupDecision = "dedup_decision";      // ist schon eine (offene/geklärte) Entscheidung
    public const string DedupLinsen = "dedup_linsen";          // Doppel-Fund zweier Linsen
}

/// <summary>Die vier Blicke des Requirements Engineers (§7: Linsen = Stellschraube, keine Architektur-Frage).</summary>
public sealed record AnalystLens(string Key, string Titel, string Checkliste);

public static class AnalystLenses
{
    public static readonly IReadOnlyList<AnalystLens> All =
    [
        new("funktional", "Funktionale Lücken",
            "Lebenszyklus-Löcher (zu jedem Anlegen: gibt es Ändern/Absagen/Löschen-Regeln?) · Rollen ohne Rechte-Story · fehlende Fehler-/Sonderfälle (offline, ungültige Eingaben) · CRUD-Asymmetrien · Zustands-Übergänge ohne Regel."),
        new("nfr", "Nicht-funktionale Anforderungen (ISO-25010-nah)",
            "performance · security · privacy (Einwilligungen! besonders bei Gesundheits-/Personendaten) · availability/Offline-Verhalten · usability (über Barrierefreiheit hinaus) · maintainability · compliance (DSGVO, Dokumentationspflichten Pflege)."),
        new("arch", "Architektur-Rahmen",
            "Getroffene fachliche Festlegungen OHNE technischen Rahmen · fehlende Festlegungen zu Persistenz/Sync/Auth, wo Anforderungen sie erzwingen · Rahmen, die sich aus NFR-Pflichten ergeben müssten."),
        new("risiko", "Risiken",
            "Datenverlust-/Sync-Risiken · Missbrauchs-/Zugriffsrisiken · Abhängigkeits-Risiken (externe Dienste) · Adoptions-Risiken (Pflege-Alltag). Output IMMER als disposition question (Risiko ist kein Wahrheits-Typ — v1)."),
    ];

    // Slice S ① (21.08., Autor-Kombo „Persona-getriebene Lücken-Findung"): die fünfte Linse — läuft NUR,
    // wenn freigegebene Personas existieren (Autor-Artefakt); ihr Arbeitspaket trägt sie als Kontext mit.
    public static readonly AnalystLens Persona = new("persona", "Persona-Abdeckung",
        "Für jede FREIGEGEBENE Persona: welches ihrer BELEGTEN Bedürfnisse deckt kein Requirement/PBI? "
        + "Welche Kernaufgabe ihres Alltags hat keine Story (Anlegen/Ändern/Einsehen)? Anker = die REQ-Belege "
        + "aus der Persona + per search_core gefundene Items. NIE aus 'Angenommen'-Zonen ableiten — nur aus "
        + "den belegten Bedürfnissen.");

    /// <summary>Linsen dieses Laufs: die vier festen Blicke + Persona-Abdeckung, wenn Personas freigegeben sind.</summary>
    public static IReadOnlyList<AnalystLens> For(string? personasContent)
        => string.IsNullOrWhiteSpace(personasContent) ? All : [.. All, Persona];
}

// ── Workflow-Nachrichten (typisierte Kanten; Muster BaselineFanOut/ClassifyStrip). Core + Vorgänger-Keys
// reisen NICHT als Nachricht, sondern per Konstruktor in den je Lauf gebauten Graphen (State-Isolation). ──

/// <summary>Arbeitspaket EINER Linse (Fan-out; Kanten-Prädikat routet per Lens.Key). Kontext = zusätzliches
/// Material nur dieser Linse (Slice S ①: die freigegebenen Personas für die Abdeckungs-Linse).</summary>
public sealed record AnalystWork(AnalystLens Lens, string Digest, string KollektorFunde, string Kontext = "");

/// <summary>Ergebnis einer Linse (Fan-in-Barrier sammelt alle vier).</summary>
public sealed record AnalystLensResult(string LensKey, IReadOnlyList<AnalystFinding> Findings);

/// <summary>Nach Merge/Dedup: die Kandidaten für den Kritiker + die bereits entschiedenen Einträge.</summary>
public sealed record AnalystMerged(
    IReadOnlyList<AnalystFinding> Kandidaten,
    IReadOnlyList<AnalystReportEintrag> Vorentschieden,
    IReadOnlySet<string> VorgaengerKeys,
    string CoreFingerprint);

/// <summary>Nach dem Kritiker: die fertige Report-Grundlage (Persist rendert + schreibt Delta).</summary>
public sealed record AnalystJudged(IReadOnlyList<AnalystReportEintrag> Eintraege, string CoreFingerprint);
