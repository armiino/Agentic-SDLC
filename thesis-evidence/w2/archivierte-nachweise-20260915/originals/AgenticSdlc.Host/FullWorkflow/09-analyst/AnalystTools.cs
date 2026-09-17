using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Analyst;

/// <summary>
/// 1g-B: der Werkzeugkasten EINER Linse — Save-Once mit Ebene-1-Validierung (das bewiesene
/// check-vor-save-Muster des Tor-1-Resolvers): Funde ohne existierenden CORE-ANKER, ohne Herleitung oder
/// mit ungültiger Kategorie/Disposition kommen als Fehlerliste zurück, der Maker iteriert. Der Form-Checker
/// lebt damit IM Maker-Loop (§5.0-Präzisierung) — Substanz prüft danach der Kritiker.
/// </summary>
public sealed class AnalystLensTools(AnalystLens lens, ProjectStateDocument core)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;
    private static readonly IReadOnlySet<string> Dispositionen =
        new HashSet<string>(StringComparer.Ordinal) { "requirement", "architecture", "question" };
    private static readonly IReadOnlySet<string> NfrMerkmale = new HashSet<string>(StringComparer.Ordinal)
    { "performance", "security", "privacy", "availability", "usability", "maintainability", "compliance" };

    private readonly HashSet<string> _coreIds = core.Items.Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);

    public IReadOnlyList<AnalystFinding>? Saved { get; private set; }
    public int CheckRounds { get; private set; }

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetItem, "get_core_item",
            "Voller Text + Typ eines Core-Items (fuer gezieltes Nachbohren; der Digest traegt nur Kurzzeilen)."),
        AIFunctionFactory.Create(Search, "search_core",
            "Stichwort-Suche ueber die AKTIVE Wahrheit (Token-ODER) — pruefe VOR jedem Fund, ob es das Thema schon gibt."),
        AIFunctionFactory.Create(SaveFindings, "save_findings",
            "GENAU EINMAL am Ende: speichert deine Funde. Validierung ist HART (Fehlerliste zurueck = nachbessern): "
            + "je Fund disposition requirement|architecture|question · kategorie functional|nfr:<merkmal>|process · "
            + "herleitung nicht leer · ankerIds = mind. 1 EXISTIERENDES Core-Item. Leere Liste = ehrlich keine Funde."),
    ];

    private string GetItem(string itemId)
    {
        var item = core.Items.FirstOrDefault(i => string.Equals(i.ItemId, itemId, StringComparison.OrdinalIgnoreCase));
        return item is null
            ? JsonSerializer.Serialize(new { error = "NOT_FOUND", itemId }, Json)
            : JsonSerializer.Serialize(new { item.ItemId, item.ItemType, item.Text, status = item.Status,
                rollen = item.Architecture?.Roles, aks = item.Pbi?.AcceptanceCriteria }, Json);
    }

    private string Search(string query)
    {
        var tokens = QueryText.Tokens(query);
        var hits = core.Items
            .Where(i => i.ReadStatus().Validity == Validity.Active)
            .Select(i => (Item: i, Score: tokens.Count(t => QueryText.Fold(i.Text).Contains(t, StringComparison.OrdinalIgnoreCase))))
            .Where(x => x.Score > 0).OrderByDescending(x => x.Score).Take(8)
            .Select(x => new { x.Item.ItemId, x.Item.ItemType, text = x.Item.Text.Length <= 140 ? x.Item.Text : x.Item.Text[..140] });
        return JsonSerializer.Serialize(hits, Json);
    }

    private string SaveFindings(IReadOnlyList<AnalystFinding> findings)
    {
        CheckRounds++;
        var errors = new List<string>();
        for (var i = 0; i < findings.Count; i++)
        {
            var f = findings[i];
            if (string.IsNullOrWhiteSpace(f.Statement)) errors.Add($"#{i + 1}: statement leer.");
            if (!Dispositionen.Contains(f.Disposition)) errors.Add($"#{i + 1}: disposition '{f.Disposition}' (erlaubt: requirement|architecture|question).");
            if (!KategorieGueltig(f.Kategorie)) errors.Add($"#{i + 1}: kategorie '{f.Kategorie}' (erlaubt: functional | nfr:<{string.Join("|", NfrMerkmale)}> | process).");
            if (string.IsNullOrWhiteSpace(f.Herleitung)) errors.Add($"#{i + 1}: herleitung PFLICHT (warum fehlt das — abgeleitet woraus?).");
            var fehlend = (f.AnkerIds ?? []).Where(a => !_coreIds.Contains(a)).ToList();
            if (f.AnkerIds is not { Count: > 0 }) errors.Add($"#{i + 1}: CORE-ANKER-PFLICHT — mind. 1 existierendes Item in ankerIds (Floskeln ohne Projektbezug werden abgewiesen).");
            else if (fehlend.Count > 0) errors.Add($"#{i + 1}: ankerIds unbekannt: {string.Join(", ", fehlend)}.");
        }
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "FINDINGS_INVALID", details = errors }, Json);
        if (Saved is not null) return JsonSerializer.Serialize(new { error = "ALREADY_SAVED", hint = "save_findings ist GENAU EINMAL erlaubt." }, Json);

        Saved = [.. findings.Select(f => f with { Linse = lens.Key })];   // Linsen-Stempel = Harness-Fakt
        return JsonSerializer.Serialize(new { saved = true, count = findings.Count }, Json);
    }

    private static bool KategorieGueltig(string? k) =>
        k is "functional" or "process"
        || (k?.StartsWith("nfr:", StringComparison.Ordinal) == true && NfrMerkmale.Contains(k["nfr:".Length..]));
}

/// <summary>Kritiker-Werkzeug: EIN Sammel-Urteil über ALLE Kandidaten (Vollständigkeit erzwungen;
/// aussortieren nur MIT Grund — der Report zeigt Aussortiertes sichtbar, kein stiller Cap).</summary>
public sealed class AnalystKritikerTools(int erwartet)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;
    public IReadOnlyList<AnalystVerdict>? Saved { get; private set; }

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(SaveVerdicts, "save_verdicts",
            "GENAU EINMAL: je Kandidat (index 0-basiert) behalten=true|false; bei aussortieren ist grund PFLICHT "
            + "(erscheint sichtbar im Report). ALLE Kandidaten beurteilen (Sammel-Akt)."),
    ];

    private string SaveVerdicts(IReadOnlyList<AnalystVerdict> verdicts)
    {
        var errors = new List<string>();
        var seen = verdicts.Select(v => v.Index).ToHashSet();
        foreach (var missing in Enumerable.Range(0, erwartet).Where(i => !seen.Contains(i)))
            errors.Add($"index {missing}: KEIN Urteil — der Sammel-Akt beurteilt ALLE Kandidaten.");
        foreach (var v in verdicts.Where(v => !v.Behalten && string.IsNullOrWhiteSpace(v.Grund)))
            errors.Add($"index {v.Index}: aussortieren OHNE grund (der Report zeigt den Grund — Pflicht).");
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "VERDICTS_INVALID", details = errors }, Json);
        if (Saved is not null) return JsonSerializer.Serialize(new { error = "ALREADY_SAVED" }, Json);
        Saved = verdicts;
        return JsonSerializer.Serialize(new { saved = true, behalten = verdicts.Count(v => v.Behalten) }, Json);
    }
}
