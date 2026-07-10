using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// Schmale Agent-Tools für den AGENTISCHEN Derivationsmodus (Stufe 1). Die Agency liegt in der ENTSCHEIDUNG des
/// Agenten (welche Quelle lese ich, was verankere ich, wann schreibe ich) — die Funktionen selbst sind
/// deterministisch und domänen-eng. Bewusst KEIN <c>ReadAnyFile(path)</c>/<c>WriteAnyFile</c> (IST_Soll09.7 §
/// Dateisystemzugriffe): der Agent arbeitet mit fachlichen Identitäten (Artefakttyp, itemId), nicht mit Pfaden,
/// und kann den zugelassenen Evidenzraum NICHT erweitern (Closed-World bleibt).
/// </summary>
/// <remarks>
/// Drei Tools:
/// <list type="bullet">
///   <item><description><c>get_baseline_items</c> — read-only, gescopet auf die Quell-Typen DIESER Ableitung.
///     Der Agent zieht selbst, was er braucht (statt vollgefüttert zu werden). Sammelt <c>retrievedItemIds</c>
///     (alle betrachteten Items) für die Provenienz — die spätere Ableitung nennt davon nur die tatsächlich
///     benutzten <c>sourceArtifactItemIds</c> (IST_Soll09.7 § Tool-Provenienz).</description></item>
///   <item><description><c>check_anchor</c> — deterministische Selbstkontrolle vor dem Verankern.</description></item>
///   <item><description><c>save_derived</c> — der EIGENE Schreibvorgang des Agenten (Stufe 1) in die run-eigene
///     <c>derivations/{spec}</c>-Scope; vergibt stabile <c>{PREFIX}-NN</c>-IDs deterministisch (ID-Stabilität bleibt
///     Host-garantiert, obwohl der Agent den Write auslöst). Guard: nur EIN wirksamer Schreibvorgang
///     (last-write-wins-Schutz).</description></item>
/// </list>
/// Die unabhängige Anker-/Inference-Prüfung läuft danach im Executor als Assurance-Report — Tool-Selbstkontrolle
/// ERSETZT die Nachprüfung NICHT (IST_Soll09.7 § „Tool Use ersetzt keine unabhängige Prüfung").
/// </remarks>
internal sealed class DerivationTools
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly SourceArtifactSet _sources;
    private readonly DerivationSpec _spec;
    private readonly RunContext _run;
    private readonly string _outDir;
    private readonly string _model;
    private readonly IReadOnlyDictionary<string, ArtifactItem> _byId;
    private readonly IReadOnlyDictionary<string, LedgerClaim> _claims;
    private readonly InferenceChecker? _checker;
    private readonly HashSet<string> _retrieved = new(StringComparer.Ordinal);
    private int _saveCount;
    private int _verifyRounds;
    private double? _firstDraftR1;

    public DerivationTools(SourceArtifactSet sources, DerivationSpec spec, RunContext run, string outDir, string model,
        IReadOnlyDictionary<string, LedgerClaim>? ledgerClaims = null, InferenceChecker? checker = null)
    {
        _sources = sources;
        _spec = spec;
        _run = run;
        _outDir = outDir;
        _model = model;
        _byId = sources.ItemsById();
        _claims = ledgerClaims ?? new Dictionary<string, LedgerClaim>();
        _checker = checker;
    }

    /// <summary>Alle vom Agenten via <c>get_baseline_items</c> betrachteten Item-IDs (Provenienz: retrieved ⊇ used).</summary>
    public IReadOnlyCollection<string> RetrievedItemIds => _retrieved;

    /// <summary>Wurde tatsächlich (genau einmal) geschrieben?</summary>
    public bool Saved => _saveCount > 0;

    /// <summary>Wie oft der Agent seinen Entwurf per <c>verify_derived</c> selbst prüfte (0 = angeboten, nicht genutzt).</summary>
    public int VerifyRounds => _verifyRounds;

    /// <summary>Treue-Verletzungsrate des ERSTEN Entwurfs (verify-Runde 1) — Basis für ΔR1 (Korrektur-Gewinn). null = nie geprüft.</summary>
    public double? FirstDraftR1 => _firstDraftR1;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetBaselineItems, "get_baseline_items",
            "Liefert die GEPRÜFTEN Quell-Items (itemId + Text) EINES Artefakttyps dieser Ableitung. Rufe es für jeden Typ, den du für die Ableitung brauchst. Nur die zugelassenen Quelltypen sind verfügbar."),
        AIFunctionFactory.Create(CheckAnchor, "check_anchor",
            "Prüft deterministisch, ob eine itemId in den geladenen Quell-Items existiert. Nutze es zur Selbstkontrolle, bevor du ein Item als Anker verwendest."),
        AIFunctionFactory.Create(SaveDerived, "save_derived",
            "Schreibt DEINE abgeleiteten Items als offizielles derived.json. Jedes Item braucht text + sourceArtifactItemIds (>=1 echte itemId) + optional assumptions + rationale. Stabile IDs werden automatisch vergeben. Rufe es GENAU EINMAL am Ende auf."),
        // ① Drill-down IN der Evidenzkette (bleibt closed-world): tiefer/breiter, aber nur innerhalb der geprüften Menge.
        AIFunctionFactory.Create(GetSourceClaims, "get_source_claims",
            "Drill-down: liefert die zugrunde liegenden Ledger-Claims (die EVIDENZ unter einem geprüften Item) für eine itemId — die Belege, aus denen das Item extrahiert wurde. Nutze es, wenn ein Item-Text allein für die Ableitung nicht reicht."),
        AIFunctionFactory.Create(FindRelatedItems, "find_related_items",
            "Liefert andere GEPRÜFTE Items (auch aus anderen Artefakttypen), die mit der itemId dieselbe Evidenz teilen (gemeinsame Ledger-Claims) — für Cross-Artifact-Bezüge, ohne die Evidenzmenge zu verlassen."),
    ];

    /// <summary>Explorer-Toolset (Modus <c>--explore</c>): die 5 Basis-Tools + Entdeckungs-/Navigations-Primitive
    /// (<c>list_artifacts</c>, <c>search_items</c>, <c>get_item</c>). Der Agent bekommt seine Umwelt NICHT vorgesagt,
    /// sondern entdeckt sie selbst. Absichtlich nur im Explorer-Modus (Mess-Arme sauber trennen; B5).</summary>
    public IReadOnlyList<AITool> BuildExplorer()
    {
        var tools = new List<AITool>
        {
            AIFunctionFactory.Create(ListArtifacts, "list_artifacts",
                "Entdecke deine Umwelt: welche verifizierten Artefakttypen mit wie vielen Items dir zur Verfügung stehen. Rufe es ZUERST."),
            AIFunctionFactory.Create(SearchItems, "search_items",
                "Suche Items per Stichwort über ALLE verfügbaren Artefakte (grobe Volltextsuche). Nützlich, um relevante Items gezielt zu finden."),
            AIFunctionFactory.Create(GetItem, "get_item",
                "Hole EIN einzelnes Item (Text + Artefakttyp) per itemId."),
        };
        tools.AddRange(Build());
        return tools;
    }

    /// <summary>Verify-Loop-Toolset (Modus <c>--verify</c>): das Explorer-Set + <c>verify_derived</c>. Der Agent kann
    /// seinen EIGENEN Entwurf (vor dem Speichern) auf Anker-Existenz UND Treue prüfen lassen, das Urteil sehen und
    /// überarbeiten — die geschlossene Feedbackschleife. Der Judge im Loop ist dieselbe <see cref="InferenceChecker"/>-
    /// Instanz wie die unabhängige Nach-Prüfung (vergleichbares Urteil für ΔR1).</summary>
    public IReadOnlyList<AITool> BuildVerify()
    {
        var tools = new List<AITool>(BuildExplorer())
        {
            AIFunctionFactory.Create(VerifyDerived, "verify_derived",
                "Prüft DEINEN ENTWURF (noch NICHT gespeichert): je Item, ob die Anker existieren (anchorOk) und ob das Risiko aus seinen Ankern folgt (verdict: supported/contradicts/unrelated/unclear) + kurze Begründung. Nutze es VOR save_derived, überarbeite schwache Items und prüfe erneut. Mehrfach erlaubt."),
        };
        return tools;
    }

    // Entdeckung: welche Artefakttypen gibt es überhaupt (der Agent wird NICHT vorab informiert).
    private string ListArtifacts()
    {
        _run.AppendEvent(new { type = "EXPLORE_TOOL_LIST_ARTIFACTS", runId = _run.RunId, spec = _spec.Id, types = _sources.Sources.Count, timestampUtc = DateTime.UtcNow });
        var sb = new StringBuilder();
        sb.AppendLine($"Deine Umwelt = {_sources.Sources.Count} verifizierte Artefakt(e) im Projektzustand:");
        foreach (var s in _sources.Sources)
        {
            var prefix = s.Items.Count > 0 ? (s.Items[0].ItemId.Split('-').FirstOrDefault() ?? "?") : "?";
            sb.AppendLine($"- {s.ArtifactType}: {s.Items.Count} Items (IDs {prefix}-…)");
        }
        return sb.ToString();
    }

    // Navigation: ein einzelnes Item per id (samt Typ).
    private string GetItem(string itemId)
    {
        var id = itemId?.Trim() ?? string.Empty;
        if (!_byId.TryGetValue(id, out var it)) return $"UNBEKANNT: {id} ist nicht in der Umwelt.";
        _retrieved.Add(id);
        var type = _sources.Sources.FirstOrDefault(s => s.Items.Any(x => string.Equals(x.ItemId, id, StringComparison.Ordinal)))?.ArtifactType ?? "?";
        _run.AppendEvent(new { type = "EXPLORE_TOOL_GET_ITEM", runId = _run.RunId, spec = _spec.Id, itemId = id, timestampUtc = DateTime.UtcNow });
        return $"{id} [{type}]: {it.Text}";
    }

    // Navigation: grobe Volltextsuche über alle Artefakte (Stichwort-Match, tolerant).
    private string SearchItems(string query)
    {
        var q = query?.Trim() ?? string.Empty;
        if (q.Length < 2) return "Query zu kurz (mind. 2 Zeichen).";
        var terms = q.Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var hits = new List<string>();
        foreach (var s in _sources.Sources)
            foreach (var it in s.Items)
                if (terms.Any(t => it.Text.Contains(t, StringComparison.OrdinalIgnoreCase)))
                {
                    _retrieved.Add(it.ItemId);
                    hits.Add($"- {it.ItemId} [{s.ArtifactType}]: {it.Text}");
                }
        _run.AppendEvent(new { type = "EXPLORE_TOOL_SEARCH", runId = _run.RunId, spec = _spec.Id, query = q, hits = hits.Count, timestampUtc = DateTime.UtcNow });
        return hits.Count == 0 ? $"Keine Treffer für '{q}'." : $"{hits.Count} Treffer für '{q}':\n{string.Join("\n", hits.Take(30))}";
    }

    // read-only, gescopet auf die tatsächlich vorhandenen Quell-Typen.
    private string GetBaselineItems(string artifactType)
    {
        var src = _sources.Sources.FirstOrDefault(s => string.Equals(s.ArtifactType, artifactType, StringComparison.OrdinalIgnoreCase));
        _run.AppendEvent(new { type = "AGENTIC_TOOL_GET_BASELINE", runId = _run.RunId, spec = _spec.Id, requested = artifactType, found = src is not null, items = src?.Items.Count ?? 0, timestampUtc = DateTime.UtcNow });
        if (src is null)
            return $"(Kein Artefakt '{artifactType}'. Verfügbar: {string.Join(", ", _sources.Sources.Select(s => s.ArtifactType))})";

        var sb = new StringBuilder();
        sb.AppendLine($"[{src.ArtifactType}] {src.Items.Count} geprüfte Items:");
        foreach (var it in src.Items)
        {
            _retrieved.Add(it.ItemId);
            sb.Append("- ").Append(it.ItemId).Append(": ").AppendLine(it.Text);
        }
        return sb.ToString();
    }

    private string CheckAnchor(string itemId)
    {
        var id = itemId?.Trim() ?? string.Empty;
        var ok = _byId.ContainsKey(id);
        _run.AppendEvent(new { type = "AGENTIC_TOOL_CHECK_ANCHOR", runId = _run.RunId, spec = _spec.Id, itemId = id, exists = ok, timestampUtc = DateTime.UtcNow });
        return ok ? $"OK: {id} existiert in den Quell-Items." : $"UNBEKANNT: {id} ist NICHT in den geladenen Quell-Items — nicht als Anker verwenden.";
    }

    // Stufe 1: der Agent SCHREIBT das offizielle derived.json selbst (via Tool). Deterministische ID-Vergabe bleibt
    // Host-Sache (ID-Stabilität), leere-Text-Items werden verworfen (wertlos + würden den Inference-Check verfälschen).
    private string SaveDerived(DerivedItemDto[] items)
    {
        if (Interlocked.Increment(ref _saveCount) > 1)
            return "ABGELEHNT: save_derived wurde bereits aufgerufen. Nur EIN Schreibvorgang pro Lauf.";

        var artifactItems = new List<ArtifactItem>();
        var skippedEmpty = 0;
        var n = 0;
        foreach (var it in items ?? [])
        {
            var text = it.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text)) { skippedEmpty++; continue; }
            n++;
            var anchors = (it.SourceArtifactItemIds ?? []).Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).ToList();
            artifactItems.Add(new ArtifactItem(
                ItemId: $"{_spec.ItemIdPrefix}-{n:D2}", Origin: ArtifactOrigin.Derived, Text: text!,
                SourceClaimIds: [], SourceArtifactItemIds: anchors,
                Assumptions: it.Assumptions ?? [], DerivationRationale: it.Rationale));
        }

        var doc = new ArtifactDocument(
            ArtifactId: _spec.ItemIdPrefix, ArtifactType: _spec.TargetArtifactType, Version: 1,
            Stage: ArtifactDocument.StageDerivation,
            Producer: new ProducerMetadata(_run.RunId, _model, _spec.AgenticPromptName ?? _spec.PromptName),
            Items: artifactItems);

        Directory.CreateDirectory(_outDir);
        File.WriteAllText(Path.Combine(_outDir, "derived.json"), JsonSerializer.Serialize(doc, Json));
        _run.AppendEvent(new { type = "AGENTIC_TOOL_SAVE_DERIVED", runId = _run.RunId, spec = _spec.Id, written = artifactItems.Count, skippedEmpty, timestampUtc = DateTime.UtcNow });
        return $"OK: {artifactItems.Count} abgeleitete Items als derived.json gespeichert{(skippedEmpty > 0 ? $" ({skippedEmpty} ohne Text verworfen)" : "")}.";
    }

    // ① Drill-down: Item → seine Ledger-Claims (Evidenz darunter). Braucht einen geladenen Ledger für die Texte.
    private string GetSourceClaims(string itemId)
    {
        var id = itemId?.Trim() ?? string.Empty;
        if (!_byId.TryGetValue(id, out var item)) return $"UNBEKANNT: {id} ist nicht in den Quell-Items.";
        var claimIds = item.SourceClaimIds ?? [];
        _run.AppendEvent(new { type = "AGENTIC_TOOL_GET_SOURCE_CLAIMS", runId = _run.RunId, spec = _spec.Id, itemId = id, claims = claimIds.Count, ledgerLoaded = _claims.Count > 0, timestampUtc = DateTime.UtcNow });
        if (claimIds.Count == 0) return $"{id} hat keine sourceClaimIds (z. B. weil bereits abgeleitet statt extrahiert).";

        var sb = new StringBuilder();
        sb.AppendLine($"{id} → {claimIds.Count} Ledger-Claim(s) (die Evidenz unter dem Item):");
        foreach (var cid in claimIds)
            sb.AppendLine(_claims.TryGetValue(cid, out var c)
                ? $"- {cid} [{c.Kind}/{c.Status}]: {c.Proposition}"
                : $"- {cid}: (kein Ledger geladen oder Claim nicht gefunden — nur die id)");
        return sb.ToString();
    }

    // ① Drill-across: andere geprüfte Items, die MINDESTENS einen Claim mit itemId teilen (gleiche Evidenz).
    private string FindRelatedItems(string itemId)
    {
        var id = itemId?.Trim() ?? string.Empty;
        if (!_byId.TryGetValue(id, out var item)) return $"UNBEKANNT: {id} ist nicht in den Quell-Items.";
        var mine = (item.SourceClaimIds ?? []).ToHashSet(StringComparer.Ordinal);
        if (mine.Count == 0) return $"{id} hat keine sourceClaimIds → keine claim-basierten Verwandten.";

        var related = new List<string>();
        foreach (var src in _sources.Sources)
            foreach (var other in src.Items)
            {
                if (string.Equals(other.ItemId, id, StringComparison.Ordinal)) continue;
                var shared = (other.SourceClaimIds ?? []).Where(mine.Contains).ToList();
                if (shared.Count == 0) continue;
                _retrieved.Add(other.ItemId);   // auch betrachtet → Provenienz (retrieved ⊇ used)
                related.Add($"- {other.ItemId} [{src.ArtifactType}] (teilt {string.Join(",", shared)}): {other.Text}");
            }

        _run.AppendEvent(new { type = "AGENTIC_TOOL_FIND_RELATED", runId = _run.RunId, spec = _spec.Id, itemId = id, related = related.Count, timestampUtc = DateTime.UtcNow });
        return related.Count == 0
            ? $"{id}: kein anderes Item teilt seine Claims."
            : $"{id} teilt Evidenz mit {related.Count} Item(s):\n{string.Join("\n", related)}";
    }

    // Verify: prüft den ENTWURF des Agenten (schreibt NICHTS). Deterministische Anker-Existenz + InferenceChecker-Treue.
    // Erste Runde stempelt _firstDraftR1 (Roh-Entwurfs-Treue) für ΔR1. Provisorische DRAFT-NN-Refs nur fürs Mapping.
    private async Task<string> VerifyDerived(DerivedItemDto[] items, CancellationToken ct)
    {
        if (_checker is null) return "verify_derived ist in diesem Modus nicht verfügbar.";
        var round = Interlocked.Increment(ref _verifyRounds);

        var draft = new List<ArtifactItem>();
        var n = 0;
        foreach (var it in items ?? [])
        {
            n++;
            var anchors = (it.SourceArtifactItemIds ?? []).Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).ToList();
            draft.Add(new ArtifactItem($"DRAFT-{n:D2}", ArtifactOrigin.Derived, it.Text?.Trim() ?? string.Empty,
                SourceClaimIds: [], SourceArtifactItemIds: anchors, Assumptions: it.Assumptions ?? [], DerivationRationale: it.Rationale));
        }
        if (draft.Count == 0) return "verify_derived: leerer Entwurf — nichts zu prüfen.";

        var report = await _checker.CheckAsync(draft, _byId, ct).ConfigureAwait(false);
        var vById = report.Verdicts.ToDictionary(v => v.ItemId, v => v, StringComparer.Ordinal);

        // ΔR1-Basis: Treue-Verletzungsrate des ERSTEN Entwurfs festhalten.
        if (round == 1)
            _firstDraftR1 = Math.Round((double)draft.Count(d =>
                vById.TryGetValue(d.ItemId, out var v) && v.Verdict is InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated) / draft.Count, 4);

        Directory.CreateDirectory(_outDir);
        File.WriteAllText(Path.Combine(_outDir, $"verify-round-{round:D2}.json"), JsonSerializer.Serialize(report, Json));
        _run.AppendEvent(new { type = "AGENTIC_TOOL_VERIFY", runId = _run.RunId, spec = _spec.Id, round, items = draft.Count, byVerdict = report.ByVerdict, timestampUtc = DateTime.UtcNow });

        var sb = new StringBuilder();
        sb.AppendLine($"verify-Runde {round}: {draft.Count} Entwurfs-Items geprüft.");
        foreach (var d in draft)
        {
            var bad = d.SourceArtifactItemIds.Where(a => !_byId.ContainsKey(a)).ToList();
            var anchorOk = d.SourceArtifactItemIds.Count > 0 && bad.Count == 0;
            var v = vById.TryGetValue(d.ItemId, out var vv) ? vv : null;
            sb.Append($"- {d.ItemId}: anchorOk={anchorOk.ToString().ToLowerInvariant()}");
            if (bad.Count > 0) sb.Append($" (unbekannt: {string.Join(",", bad)})");
            sb.AppendLine($", verdict={(v?.Verdict.ToString() ?? "unclear").ToLowerInvariant()} — {v?.Rationale ?? ""}");
        }
        sb.AppendLine("Überarbeite Items mit anchorOk=false ODER verdict∈{contradicts,unrelated,unclear} (bessere Anker, schärfere Ableitung, oder verwerfen), dann verify erneut ODER speichere final mit save_derived.");
        return sb.ToString();
    }

    /// <summary>Roh-Eingabe eines abgeleiteten Items für <c>save_derived</c> (der Agent baut die IDs NICHT selbst).</summary>
    public sealed record DerivedItemDto(
        [property: JsonPropertyName("text")] string? Text,
        [property: JsonPropertyName("sourceArtifactItemIds")] string[]? SourceArtifactItemIds,
        [property: JsonPropertyName("assumptions")] string[]? Assumptions,
        [property: JsonPropertyName("rationale")] string? Rationale);
}

/// <summary>Minimaler Ledger-Claim für den <c>get_source_claims</c>-Drill-down (Evidenz UNTER einem Item).</summary>
public sealed record LedgerClaim(string Id, string Proposition, string? Kind, string? Status, string? Scope);

/// <summary>Lädt die <c>claims[]</c> aus einer Ledger-<c>consumable.json</c> in ein id→Claim-Register. Best-effort:
/// fehlt der Pfad oder ist die Datei unlesbar, liefert es ein leeres Register (Tools laufen dann nur mit den ids).</summary>
public static class LedgerClaimIndex
{
    public static IReadOnlyDictionary<string, LedgerClaim> LoadOrEmpty(string? path)
    {
        var map = new Dictionary<string, LedgerClaim>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return map;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (doc.RootElement.TryGetProperty("claims", out var claims) && claims.ValueKind == JsonValueKind.Array)
                foreach (var c in claims.EnumerateArray())
                {
                    var id = c.TryGetProperty("id", out var idp) ? idp.GetString() : null;
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    map[id!] = new LedgerClaim(id!,
                        c.TryGetProperty("proposition", out var p) ? p.GetString() ?? "" : "",
                        c.TryGetProperty("kind", out var k) ? k.GetString() : null,
                        c.TryGetProperty("status", out var s) ? s.GetString() : null,
                        c.TryGetProperty("scope", out var sc) ? sc.GetString() : null);
                }
        }
        catch (JsonException) { /* leeres Register → Tools liefern nur ids */ }
        return map;
    }
}
