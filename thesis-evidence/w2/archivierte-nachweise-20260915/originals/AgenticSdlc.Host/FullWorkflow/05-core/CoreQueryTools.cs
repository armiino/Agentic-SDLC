using AgenticSdlc.Host.FullWorkflow.Delta;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// C3 (⚖ K5/K7, 07.08.2026) — der EINE Core-Lese-Werkzeugkasten für Agenten: read-only-AIFunctions um die
/// <see cref="ICoreRepository"/>-Naht (DB-Swap-neutral — die Tools kennen nur die Naht, nie das Medium).
/// Jeder Agent bekommt seine ROLLEN-Teilmenge dieses Kastens (Steward: alle; GitHubInbound später: wenige) —
/// Daten-Fragen laufen über Tools, nie über Agent-fragt-Agent (K7). Kein Tool hier schreibt.
/// </summary>
// Slice S (21.08.): optionale artefaktHinweis-Naht — der Aufrufer (Steward) injiziert eine deterministische
// Frische-Prüfung der Autor-Artefakte (z. B. „C4 kennt ARCH-41 noch nicht"); die Tool-Klasse bleibt
// medium-neutral (K5: kennt weiter NUR die Repository-Naht, nie Dateisystem-Pfade).
public sealed class CoreQueryTools(ICoreRepository repo, Func<ProjectStateDocument, string?>? artefaktHinweis = null)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    private const int ListCap = 50;
    private const int ListTextLength = 140;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetCoreOverviewAsync, "get_core_overview",
            "Read-only-Ueberblick der Projektwahrheit: Item-Zaehlung je Typ, offene Entscheidungen (Parkplatz), Relations-Anzahl und die Kangal-Integritaetslage (Fehler/Warnungen)."),
        AIFunctionFactory.Create(ListCoreItemsAsync, "list_core_items",
            "Listet Items der Projektwahrheit kompakt (id, typ, status, text gekuerzt). Filter: itemType (z. B. pbi|requirement|feature|decision|architecture), sourceRunId = nur Items aus diesem Lauf (Wahrheits-seitige Lauf-Bilanz), status = exakter Status-Wert (z. B. needs_clarify|active|accepted|open_decision|blocked_by_decision — fuer Status-Fragen DIESEN Filter nutzen statt Items einzeln zu oeffnen), query = genau EIN woertlicher Suchbegriff (Substring, case-insensitiv, Umlaut-tolerant; KEINE Operatoren wie OR — fuer Alternativen mehrfach aufrufen), includeSuperseded (Standard false = nur gueltige Wahrheit)."),
        AIFunctionFactory.Create(GetCoreItemAsync, "get_core_item",
            "Liest EIN Item der Projektwahrheit VOLLSTAENDIG (Text, Status-Achsen, Payloads, Historie) inkl. aller Beziehungen (covers, part_of_feature, constrained_by, ...) mit den Texten der Gegenseite."),
    ];

    private async Task<string> GetCoreOverviewAsync()
    {
        if (await LoadAsync().ConfigureAwait(false) is not { } core)
            return NotFound();

        var kangal = CoreKangal.Check(core);
        return JsonSerializer.Serialize(new
        {
            items = core.Items.Count,
            byType = core.Items
                .GroupBy(i => i.ItemType.ToLowerInvariant())
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Count()),
            relations = core.Relations.Count,
            openDecisions = CoreParkplatz.Count(core),
            kangal = new { errors = kangal.Errors.Count, warnings = kangal.Warnings.Count },
            // Slice S: Frische der Autor-Artefakte (null = nichts zu melden) — der Steward bietet Updates AKTIV an.
            artefaktHinweis = artefaktHinweis?.Invoke(core),
        }, Json);
    }

    private async Task<string> ListCoreItemsAsync(string? itemType = null, string? status = null, string? query = null, bool includeSuperseded = false, string? sourceRunId = null)
    {
        if (await LoadAsync().ConfigureAwait(false) is not { } core)
            return NotFound();

        var hits = core.Items
            .Where(i => itemType is null || string.Equals(i.ItemType, itemType, StringComparison.OrdinalIgnoreCase))
            .Where(i => status is null || string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase))
            .Where(i => sourceRunId is null || string.Equals(i.SourceRunId, sourceRunId, StringComparison.OrdinalIgnoreCase))
            .Where(i => includeSuperseded || i.ReadStatus().Validity != Validity.Superseded)
            .Where(i => QueryText.Contains(i.Text, query) || (query is not null && i.ItemId.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();

        return JsonSerializer.Serialize(new
        {
            total = hits.Count,
            returned = Math.Min(hits.Count, ListCap),
            dropped = Math.Max(0, hits.Count - ListCap),   // kein stilles Kappen — Rest via query eingrenzen
            items = hits.Take(ListCap).Select(i => new
            {
                itemId = i.ItemId,
                itemType = i.ItemType,
                status = i.Status,
                text = i.Text.Length <= ListTextLength ? i.Text : i.Text[..ListTextLength] + "…",
            }),
        }, Json);
    }

    private async Task<string> GetCoreItemAsync(string itemId)
    {
        if (await LoadAsync().ConfigureAwait(false) is not { } core)
            return NotFound();

        var item = core.Items.FirstOrDefault(i => string.Equals(i.ItemId, itemId, StringComparison.OrdinalIgnoreCase));
        if (item is null)
            return JsonSerializer.Serialize(new { error = "ITEM_NOT_FOUND", itemId }, Json);

        var texts = core.Items.ToDictionary(i => i.ItemId, i => i.Text, StringComparer.OrdinalIgnoreCase);
        var relations = core.Relations
            .Where(r => string.Equals(r.FromId, item.ItemId, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(r.ToId, item.ItemId, StringComparison.OrdinalIgnoreCase))
            .Select(r =>
            {
                var outgoing = string.Equals(r.FromId, item.ItemId, StringComparison.OrdinalIgnoreCase);
                var otherId = outgoing ? r.ToId : r.FromId;
                return new
                {
                    relationType = r.RelationType,
                    direction = outgoing ? "outgoing" : "incoming",
                    otherId,
                    // Extern-Ziele (gh#n, Claims, ...) haben kein Core-Item — dann nur die Id.
                    otherText = texts.TryGetValue(otherId, out var t)
                        ? (t.Length <= ListTextLength ? t : t[..ListTextLength] + "…") : null,
                };
            })
            .OrderBy(r => r.relationType, StringComparer.Ordinal).ThenBy(r => r.otherId, StringComparer.Ordinal);

        return JsonSerializer.Serialize(new { item, relations }, Json);
    }

    private async Task<ProjectStateDocument?> LoadAsync()
        => await repo.ExistsAsync().ConfigureAwait(false) ? await repo.LoadAsync().ConfigureAwait(false) : null;

    private static string NotFound() => JsonSerializer.Serialize(new { error = "CORE_NOT_FOUND" }, Json);
}
