using System.Text.Json;
using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Interactive-Modus / Re-Launch (§9) der Adjudikation: startet den generischen lokalen Review-Server
/// (<see cref="LocalReviewServerHost"/>) UEBER einer bestehenden queue.json und speichert jede Aenderung
/// sofort zurueck (Autosave = Sicherheitsnetz). Bei „Weiter/Fertig" wird — wenn ein validated Ledger
/// mitgegeben ist — apply+Gate+consumable IM SELBEN PROZESS ausgefuehrt; bei „Abbrechen" bleibt der Stand
/// in der queue.json und man kann spaeter erneut einsteigen. Alle drei Einstiege (frischer Interactive-Run,
/// Handedit, Re-Launch) enden bei DERSELBEN queue.json -> demselben apply (Plan §4/§9).
///
///   ledger-adjudicate-ui &lt;queue.json&gt; [validated-ledger.json] [--no-browser]
/// </summary>
public static class LedgerAdjudicateUiRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: ledger-adjudicate-ui <queue.json> [validated-ledger.json] [--no-browser]");
            return 2;
        }

        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var positional = args.Skip(1).Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToArray();
        var queuePath = Resolve(repoRoot, positional[0]);
        var validatedPath = positional.Length >= 2 ? Resolve(repoRoot, positional[1]) : null;

        if (!File.Exists(queuePath)) { Console.Error.WriteLine($"[adjudicate-ui] queue fehlt: {queuePath}"); return 2; }
        if (validatedPath is not null && !File.Exists(validatedPath))
        { Console.Error.WriteLine($"[adjudicate-ui] validated fehlt: {validatedPath}"); return 2; }

        var queue = JsonSerializer.Deserialize<AdjudicationQueue>(await File.ReadAllTextAsync(queuePath), Json)
                    ?? new AdjudicationQueue(null, null, "", []);
        if (validatedPath is not null)
        {
            var validatedForRepairSuggestions = LedgerAdjudicationAdapter.LoadValidated(validatedPath);
            queue = LedgerAdjudicationAdapter.EnrichRepairSuggestions(queue, validatedForRepairSuggestions);
        }

        // Mutabler Arbeitsstand: Items als Liste, indiziert nach ItemId. Jeder Save schreibt die queue neu.
        var items = queue.Items.ToList();
        var index = items.Select((it, i) => (it.ItemId, i)).ToDictionary(x => x.ItemId, x => x.i);
        var writeLock = new SemaphoreSlim(1, 1);

        AdjudicationQueue Current() => queue with { Items = items.ToList() };

        // Anreicherung für die UI: Claim-Katalog (id -> proposition) aus dem validated Ledger + Atomic-Unit-
        // Texte aus step-00. Damit: (a) Referenz-Ziel als Autocomplete, (b) unit_signal-Evidenz zeigt echten
        // Transkript-Text statt nackter Claim-IDs. Beides ist optional/best-effort (fehlt eine Datei -> Fallback).
        var runDir = Path.GetDirectoryName(Path.GetDirectoryName(queuePath));
        string? Step(string s) => runDir is null ? null : Path.Combine(runDir, s, "output.json");

        // Referenz-KATALOG (für merge/mark) = nur FINALE (validated) Claims — dorthin darf gemerged werden.
        var validatedLedgerPath = validatedPath ?? Step("step-03-facet-validation");
        var validatedProps = LoadEntryProps(validatedLedgerPath);
        var validatedSourceUnitIds = LoadEntrySourceUnitIds(validatedLedgerPath);
        var candidateToCanonicalClaimIds = LoadCandidateToCanonicalClaimIds(validatedLedgerPath);
        var referenceDetails = LoadReferenceDetails(validatedLedgerPath);
        // AUFLÖSUNG bezogener IDs in der Evidenz = Union candidate+canonical+validated (unit_signal referenziert
        // oft CANDIDATE-IDs aus step-01, die nach Canonicalization nicht mehr im validierten Ledger stehen).
        var claimProps = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var kv in LoadEntryProps(Step("step-01-candidate"))) claimProps[kv.Key] = kv.Value;
        foreach (var kv in LoadEntryProps(Step("step-02-canonical"))) claimProps[kv.Key] = kv.Value;
        foreach (var kv in validatedProps) claimProps[kv.Key] = kv.Value; // validated gewinnt bei Konflikt
        var units = LoadUnits(Step("step-00-atomic-units"));
        var referenceOptions = validatedProps.Count == 0 ? null
            : validatedProps.OrderBy(kv => kv.Key, StringComparer.Ordinal)
                            .Select(kv => new ReviewOption(kv.Key, $"{kv.Key} — {Truncate(kv.Value, 90)}"))
                            .ToList();
        Console.WriteLine($"[adjudicate-ui] Anreicherung: {validatedProps.Count} validierte Claims (Referenz-Katalog), "
                          + $"{claimProps.Count} Claims gesamt (ID-Auflösung), {units.Count} Units (Evidenz).");

        var session = LedgerAdjudicationReviewAdapter.BuildSession(queue, referenceOptions, candidateToCanonicalClaimIds);
        // Anfangs-Resolved-Flags aus dem geladenen Stand (teil-befuellte queue.json -> Re-Launch).
        foreach (var ri in session.Items) ri.Resolved = LedgerAdjudicationReviewAdapter.Resolved(ri);

        var options = new ReviewServerOptions
        {
            Session = session,
            OpenBrowser = !noBrowser,
            RecomputeResolved = LedgerAdjudicationReviewAdapter.Resolved,
            ResolveContext = (itemId, key) => Task.FromResult(
                LedgerAdjudicationReviewAdapter.ResolveContext(
                    Current(), itemId, key, claimProps, candidateToCanonicalClaimIds, validatedSourceUnitIds, units)),
            ResolveReference = referenceId => Task.FromResult(
                referenceDetails.TryGetValue(referenceId, out var d) ? d.Details : null),
            ResolveReferenceContext = (referenceId, key) => Task.FromResult(
                ResolveReferenceContext(referenceDetails, units, referenceId, key)),
            OnItemSaved = async reviewed =>
            {
                if (!index.TryGetValue(reviewed.ItemId, out var i)) return;
                items[i] = LedgerAdjudicationReviewAdapter.MergeInto(items[i], reviewed);
                await writeLock.WaitAsync();
                try { await File.WriteAllTextAsync(queuePath, JsonSerializer.Serialize(Current(), Json)); }
                finally { writeLock.Release(); }
            },
        };

        Console.WriteLine($"[adjudicate-ui] queue={Rel(repoRoot, queuePath)}  autosave aktiv"
                          + (validatedPath is not null ? $"  · apply-Ziel: {Rel(repoRoot, validatedPath)}" : "  · (nur speichern, apply separat)"));

        var result = await LocalReviewServerHost.RunAsync(options);

        if (result.Outcome == ReviewOutcome.Cancelled)
        {
            Console.WriteLine($"[adjudicate-ui] ABGEBROCHEN — Stand gespeichert in {Rel(repoRoot, queuePath)}.");
            Console.WriteLine($"[adjudicate-ui] später weiter: ledger-adjudicate-ui {Rel(repoRoot, queuePath)}"
                              + (validatedPath is not null ? $" {Rel(repoRoot, validatedPath)}" : ""));
            return 0;
        }

        // FINISHED.
        Console.WriteLine($"[adjudicate-ui] FERTIG — alle Items entschieden, gespeichert in {Rel(repoRoot, queuePath)}.");
        if (validatedPath is null)
        {
            Console.WriteLine("[adjudicate-ui] kein validated Ledger übergeben -> apply separat: "
                              + $"ledger-adjudicate-apply {Rel(repoRoot, queuePath)} <validated-ledger.json>");
            return 0;
        }

        // apply+Gate+consumable im selben Prozess (Wiederverwendung der bestehenden Naht-Logik).
        Console.WriteLine("[adjudicate-ui] wende an (apply + Gate + consumable) …");
        return await LedgerAdjudicateRunner.RunApplyAsync(
            ["ledger-adjudicate-apply", queuePath, validatedPath], repoRoot);
    }

    /// <summary>
    /// Liest id -> proposition aus einem Ledger-Step-Output. Shape-agnostisch: candidate/canonical sind flach
    /// (<c>entries[].id/proposition</c>), validated ist verschachtelt (<c>entries[].entry.id/proposition</c>).
    /// Best-effort — fehlende Datei / Parse-Fehler => leere Map.
    /// </summary>
    private static Dictionary<string, string> LoadEntryProps(string? path)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (path is null || !File.Exists(path)) return map;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (!doc.RootElement.TryGetProperty("entries", out var entries) || entries.ValueKind != JsonValueKind.Array)
                return map;
            foreach (var e in entries.EnumerateArray())
            {
                var obj = e.TryGetProperty("entry", out var inner) && inner.ValueKind == JsonValueKind.Object ? inner : e;
                if (obj.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
                    map[id.GetString()!] = obj.TryGetProperty("proposition", out var p) && p.ValueKind == JsonValueKind.String
                        ? p.GetString()! : "";
            }
        }
        catch { /* best-effort */ }
        return map;
    }

    private static Dictionary<string, string> LoadCandidateToCanonicalClaimIds(string? path)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (path is null || !File.Exists(path)) return map;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (!doc.RootElement.TryGetProperty("entries", out var entries) || entries.ValueKind != JsonValueKind.Array)
                return map;
            foreach (var wrapper in entries.EnumerateArray())
            {
                var obj = wrapper.TryGetProperty("entry", out var inner) && inner.ValueKind == JsonValueKind.Object ? inner : wrapper;
                if (!obj.TryGetProperty("id", out var idEl) || idEl.ValueKind != JsonValueKind.String) continue;
                var canonicalId = idEl.GetString();
                if (string.IsNullOrWhiteSpace(canonicalId)) continue;

                map[canonicalId] = canonicalId;
                if (!obj.TryGetProperty("candidateIds", out var candidateIds) || candidateIds.ValueKind != JsonValueKind.Array)
                    continue;
                foreach (var candidateId in candidateIds.EnumerateArray())
                {
                    if (candidateId.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(candidateId.GetString()))
                        map[candidateId.GetString()!] = canonicalId;
                }
            }
        }
        catch { /* best-effort */ }
        return map;
    }

    private static Dictionary<string, ReferenceDetailData> LoadReferenceDetails(string? path)
    {
        var map = new Dictionary<string, ReferenceDetailData>(StringComparer.Ordinal);
        if (path is null || !File.Exists(path)) return map;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (!doc.RootElement.TryGetProperty("entries", out var entries) || entries.ValueKind != JsonValueKind.Array)
                return map;
            foreach (var wrapper in entries.EnumerateArray())
            {
                var obj = wrapper.TryGetProperty("entry", out var inner) && inner.ValueKind == JsonValueKind.Object ? inner : wrapper;
                if (!obj.TryGetProperty("id", out var idEl) || idEl.ValueKind != JsonValueKind.String) continue;
                var id = idEl.GetString()!;
                var prop = GetString(obj, "proposition") ?? "";
                var status = GetString(obj, "status") ?? "?";
                var modality = GetString(obj, "modality") ?? "?";
                var timeScope = GetString(obj, "timeScope") ?? "null";
                var notes = new List<ReviewNote>();
                notes.Add(new(ReviewNoteKind.Info, "Facetten",
                    $"Verbindlichkeit: {LedgerAdjudicationReviewAdapter.FacetValueLabel("modality", modality)} · "
                    + $"Entscheidungsstand: {LedgerAdjudicationReviewAdapter.FacetValueLabel("status", status)} · "
                    + $"Zeitbezug: {LedgerAdjudicationReviewAdapter.FacetValueLabel("timescope", timeScope)}"));
                if (wrapper.TryGetProperty("claimStatus", out var cs) && cs.ValueKind == JsonValueKind.String)
                    notes.Add(new(ReviewNoteKind.Suggestion, "Claim-Status",
                        LedgerAdjudicationReviewAdapter.ClaimStatusLabel(cs.GetString())));
                if (wrapper.TryGetProperty("validation", out var validation) && validation.ValueKind == JsonValueKind.Object)
                {
                    var verdict = GetString(validation, "verdict");
                    var reason = GetString(validation, "reason");
                    if (!string.IsNullOrWhiteSpace(verdict) || !string.IsNullOrWhiteSpace(reason))
                        notes.Add(new(ReviewNoteKind.Reason, "Validierung",
                            $"{LedgerAdjudicationReviewAdapter.ValidationVerdictLabel(verdict)}"
                            + (string.IsNullOrWhiteSpace(reason) ? "" : $" — {reason}")));
                }

                var evidence = ReadEvidence(obj);
                var sourceUnitIds = ReadStringArray(obj, "sourceUnitIds");
                var details = new ReviewReferenceDetails(
                    id,
                    id,
                    prop,
                    notes,
                    [
                        new(ContextBlockKind.Quote, $"Evidenz öffnen ({evidence.Count})", "evidence"),
                        new(ContextBlockKind.Excerpt, "Transkript-Kontext öffnen", "transcript")
                    ]);
                map[id] = new ReferenceDetailData(details, evidence, sourceUnitIds);
            }
        }
        catch { /* best-effort */ }
        return map;
    }

    private static string ResolveReferenceContext(
        IReadOnlyDictionary<string, ReferenceDetailData> referenceDetails,
        IReadOnlyDictionary<string, LedgerAdjudicationReviewAdapter.UnitRef> units,
        string referenceId,
        string key)
    {
        if (!referenceDetails.TryGetValue(referenceId, out var d)) return "(Referenz nicht gefunden)";
        if (key == "evidence")
            return d.Evidence.Count == 0
                ? "(keine Evidenz hinterlegt)"
                : string.Join("\n\n", d.Evidence.Select((q, n) => $"[{n + 1}] {q}"));
        if (key != "transcript") return "(unbekannter Kontextblock)";
        if (units.Count == 0) return "(kein Transkript-/Unit-Kontext gefunden)";

        var highlightIds = d.SourceUnitIds.ToHashSet(StringComparer.Ordinal);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Transkript-Kontext");
        sb.AppendLine("Marker >> = sourceUnitIds dieses Referenz-Claims.");
        sb.AppendLine();
        foreach (var (id, u) in units.OrderBy(kv => kv.Value.TurnIndex).ThenBy(kv => kv.Key, StringComparer.Ordinal))
        {
            var marker = highlightIds.Contains(id) ? ">> " : "   ";
            sb.Append(marker)
              .Append(id)
              .Append(" | ")
              .Append(u.Speaker)
              .Append(" (Turn ")
              .Append(u.TurnIndex)
              .Append("): ")
              .AppendLine(u.Text);
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Liest id -> sourceUnitIds aus einem Ledger-Step-Output. Damit kann die Review-UI im Transcript-Drawer
    /// exakt die Units highlighten, die der validierte Claim trägt, statt nur Quotes best-effort zu matchen.
    /// </summary>
    private static Dictionary<string, IReadOnlyList<string>> LoadEntrySourceUnitIds(string? path)
    {
        var map = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        if (path is null || !File.Exists(path)) return map;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (!doc.RootElement.TryGetProperty("entries", out var entries) || entries.ValueKind != JsonValueKind.Array)
                return map;
            foreach (var e in entries.EnumerateArray())
            {
                var obj = e.TryGetProperty("entry", out var inner) && inner.ValueKind == JsonValueKind.Object ? inner : e;
                if (!obj.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String) continue;
                if (!obj.TryGetProperty("sourceUnitIds", out var sourceIds) || sourceIds.ValueKind != JsonValueKind.Array) continue;

                var ids = sourceIds.EnumerateArray()
                    .Where(x => x.ValueKind == JsonValueKind.String)
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .Distinct(StringComparer.Ordinal)
                    .Order(StringComparer.Ordinal)
                    .ToList();
                map[id.GetString()!] = ids;
            }
        }
        catch { /* best-effort */ }
        return map;
    }

    private static IReadOnlyList<string> ReadEvidence(JsonElement obj)
    {
        var list = new List<string>();
        if (!obj.TryGetProperty("evidence", out var evidence) || evidence.ValueKind != JsonValueKind.Array) return list;
        foreach (var e in evidence.EnumerateArray())
            if (e.TryGetProperty("quote", out var q) && q.ValueKind == JsonValueKind.String)
                list.Add(q.GetString()!);
        return list;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement obj, string property)
    {
        var list = new List<string>();
        if (!obj.TryGetProperty(property, out var arr) || arr.ValueKind != JsonValueKind.Array) return list;
        foreach (var e in arr.EnumerateArray())
            if (e.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(e.GetString()))
                list.Add(e.GetString()!);
        return list.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
    }

    private static string? GetString(JsonElement obj, string property) =>
        obj.TryGetProperty(property, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() : null;

    /// <summary>Atomic-Unit-Texte unitId -> (Speaker, Turn, Text) aus step-00 (für unit_signal-Evidenz).</summary>
    private static Dictionary<string, LedgerAdjudicationReviewAdapter.UnitRef> LoadUnits(string? unitsPath)
    {
        var map = new Dictionary<string, LedgerAdjudicationReviewAdapter.UnitRef>(StringComparer.Ordinal);
        if (unitsPath is null || !File.Exists(unitsPath)) return map;
        try
        {
            var doc = JsonSerializer.Deserialize<AtomicUnitDoc>(File.ReadAllText(unitsPath), Json);
            foreach (var u in doc?.Units ?? [])
                if (!string.IsNullOrWhiteSpace(u.Id))
                    map[u.Id] = new LedgerAdjudicationReviewAdapter.UnitRef(u.Speaker ?? "?", u.TurnIndex, u.Text ?? "");
        }
        catch { /* best-effort: ohne Units zeigt unit_signal-Evidenz nur die bezogenen Claim-IDs. */ }
        return map;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..(max - 1)] + "…";

    private sealed record AtomicUnitDoc(List<AtomicUnitRow> Units);
    private sealed record AtomicUnitRow(string? Id, string? Speaker, int TurnIndex, string? Text);
    private sealed record ReferenceDetailData(
        ReviewReferenceDetails Details,
        IReadOnlyList<string> Evidence,
        IReadOnlyList<string> SourceUnitIds);

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
    private static string Rel(string repoRoot, string p) => Path.GetRelativePath(repoRoot, p);
}
