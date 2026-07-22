using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.Derivation;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

/// <summary>
/// Schmale Tool-Umwelt für den agentischen L3-Coverage-Modus. Der Agent entscheidet selbst, welche Umweltteile er liest,
/// welche Lücken er formuliert und wann er speichert; die Tools sind deterministische Fachoperationen.
/// </summary>
internal sealed class L3AgenticTools
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    private readonly SourceArtifactSet _env;
    private readonly CoverageSpec _spec;
    private readonly RunContext _run;
    private readonly string _outDir;
    private readonly string _model;
    private readonly IReadOnlyDictionary<string, ArtifactItem> _byId;
    private readonly IReadOnlyDictionary<string, LedgerClaim> _claims;
    private readonly HashSet<string> _retrieved = new(StringComparer.Ordinal);
    private readonly List<L3Candidate> _savedCandidates = new();
    private int _saveCount;
    private int _coverageCheckRounds;
    private int _adequacyCheckRounds;

    public L3AgenticTools(SourceArtifactSet env, CoverageSpec spec, RunContext run, string outDir, string model,
        IReadOnlyDictionary<string, LedgerClaim>? ledgerClaims = null)
    {
        _env = env;
        _spec = spec;
        _run = run;
        _outDir = outDir;
        _model = model;
        _byId = env.ItemsById();
        _claims = ledgerClaims ?? new Dictionary<string, LedgerClaim>();
    }

    public IReadOnlyCollection<string> RetrievedItemIds => _retrieved;
    public IReadOnlyList<L3Candidate> SavedCandidates => _savedCandidates;
    public bool Saved => _saveCount > 0;
    public int CoverageCheckRounds => _coverageCheckRounds;
    public int AdequacyCheckRounds => _adequacyCheckRounds;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ListArtifacts, "list_artifacts",
            "Entdecke die verfügbare L1/L2-Umwelt: Artefakttypen, Item-Zahlen und Beispiel-IDs. Rufe es zuerst."),
        AIFunctionFactory.Create(SearchItems, "search_items",
            "Suche Items per Stichwort über alle verfügbaren Artefakte. Liefert itemId, Artefakttyp und kurzen Textauszug."),
        AIFunctionFactory.Create(GetItem, "get_item",
            "Hole ein einzelnes Umwelt-Item mit Text, Herkunfts-IDs und Artefakttyp per itemId."),
        AIFunctionFactory.Create(GetSourceClaims, "get_source_claims",
            "Drill-down unter ein Umwelt-Item: liefert Ledger-Claims, soweit ein Ledger geladen ist, sonst die Claim-IDs."),
        AIFunctionFactory.Create(FindRelatedItems, "find_related_items",
            "Findet andere Umwelt-Items mit gemeinsamen sourceClaimIds oder sourceArtifactItemIds."),
        AIFunctionFactory.Create(GetCoverageLenses, "get_coverage_lenses",
            "Liefert den verbindlichen L3-Coverage-Linsenkatalog. gapCategory muss exakt eine dieser lens ids sein."),
        AIFunctionFactory.Create(CheckLensCoverage, "check_lens_coverage",
            "READ-ONLY Feedback zu DEINEM Kandidatenentwurf: welche Linsen sind über gapCategory adressiert, welche fehlen?"),
        AIFunctionFactory.Create(CheckLensAdequacy, "check_lens_adequacy",
            "READ-ONLY Substanzfeedback zu DEINEM Entwurf je Linse: substantive, thin, missing, uncertain oder not_applicable."),
        AIFunctionFactory.Create(SaveL3Candidates, "save_l3_candidates",
            "Schreibt DEINE finalen L3-Kandidaten. Rufe es GENAU EINMAL am Ende auf. CandidateIds vergibt der Host stabil.")
    ];

    private string ListArtifacts()
    {
        var rows = _env.Sources.Select(s => new
        {
            artifactType = s.ArtifactType,
            artifactId = s.ArtifactId,
            stage = s.Stage,
            items = s.Items.Count,
            sampleItemIds = s.Items.Take(8).Select(i => i.ItemId).ToArray()
        }).ToArray();
        _run.AppendEvent(new { type = "L3_AGENTIC_TOOL_LIST_ARTIFACTS", runId = _run.RunId, artifacts = rows.Length, items = _env.TotalItemCount, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string SearchItems(string query, int limit = 20)
    {
        var q = (query ?? string.Empty).Trim();
        if (q.Length == 0) return "[]";
        var terms = q.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = _env.Sources.SelectMany(s => s.Items.Select(i => new { source = s, item = i }))
            .Select(x => new { x.source, x.item, score = terms.Count(t => SafeText(x.item).Contains(t, StringComparison.OrdinalIgnoreCase)) })
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score)
            .ThenBy(x => x.item.ItemId, StringComparer.Ordinal)
            .Take(Math.Clamp(limit, 1, 50))
            .Select(x =>
            {
                _retrieved.Add(x.item.ItemId);
                return new
                {
                    itemId = x.item.ItemId,
                    artifactType = x.source.ArtifactType,
                    text = Truncate(SafeText(x.item), 500)
                };
            })
            .ToArray();
        _run.AppendEvent(new { type = "L3_AGENTIC_TOOL_SEARCH_ITEMS", runId = _run.RunId, query = q, returned = result.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(result, Json);
    }

    private string GetItem(string itemId)
    {
        var id = (itemId ?? string.Empty).Trim();
        if (!_byId.TryGetValue(id, out var item)) return $"UNKNOWN_ITEM: '{id}' existiert nicht in der L3-Umwelt.";
        _retrieved.Add(id);
        var artifactType = _env.Sources.FirstOrDefault(s => s.Items.Any(i => i.ItemId == id))?.ArtifactType;
        _run.AppendEvent(new { type = "L3_AGENTIC_TOOL_GET_ITEM", runId = _run.RunId, itemId = id, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new
        {
            itemId = item.ItemId,
            artifactType,
            origin = item.Origin.ToString(),
            text = SafeText(item),
            item.SourceClaimIds,
            item.SourceArtifactItemIds,
            item.Assumptions,
            item.DerivationRationale
        }, Json);
    }

    private string GetSourceClaims(string itemId)
    {
        var id = (itemId ?? string.Empty).Trim();
        if (!_byId.TryGetValue(id, out var item)) return $"UNKNOWN_ITEM: '{id}' existiert nicht in der L3-Umwelt.";
        _retrieved.Add(id);
        var claims = (item.SourceClaimIds ?? []).Select(cid =>
            _claims.TryGetValue(cid, out var c)
                ? new { id = c.Id, c.Proposition, c.Kind, c.Status, c.Scope }
                : new { id = cid, Proposition = "", Kind = (string?)null, Status = (string?)null, Scope = (string?)null })
            .ToArray();
        _run.AppendEvent(new { type = "L3_AGENTIC_TOOL_GET_SOURCE_CLAIMS", runId = _run.RunId, itemId = id, claims = claims.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(claims, Json);
    }

    private string FindRelatedItems(string itemId, int limit = 20)
    {
        var id = (itemId ?? string.Empty).Trim();
        if (!_byId.TryGetValue(id, out var item)) return $"UNKNOWN_ITEM: '{id}' existiert nicht in der L3-Umwelt.";
        _retrieved.Add(id);
        var claimIds = (item.SourceClaimIds ?? []).ToHashSet(StringComparer.Ordinal);
        var upstreamIds = (item.SourceArtifactItemIds ?? []).ToHashSet(StringComparer.Ordinal);
        var result = _env.Sources.SelectMany(s => s.Items.Select(i => new { source = s, item = i }))
            .Where(x => x.item.ItemId != id)
            .Select(x => new
            {
                x.source,
                x.item,
                sharedClaimIds = (x.item.SourceClaimIds ?? []).Where(claimIds.Contains).ToArray(),
                sharedArtifactItemIds = (x.item.SourceArtifactItemIds ?? []).Where(upstreamIds.Contains).ToArray()
            })
            .Where(x => x.sharedClaimIds.Length > 0 || x.sharedArtifactItemIds.Length > 0)
            .Take(Math.Clamp(limit, 1, 50))
            .Select(x =>
            {
                _retrieved.Add(x.item.ItemId);
                return new
                {
                    itemId = x.item.ItemId,
                    artifactType = x.source.ArtifactType,
                    text = Truncate(SafeText(x.item), 500),
                    x.sharedClaimIds,
                    x.sharedArtifactItemIds
                };
            })
            .ToArray();
        _run.AppendEvent(new { type = "L3_AGENTIC_TOOL_FIND_RELATED_ITEMS", runId = _run.RunId, itemId = id, returned = result.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(result, Json);
    }

    private string GetCoverageLenses()
    {
        var payload = new
        {
            specId = _spec.Id,
            _spec.Name,
            lenses = _spec.Lenses.Select(l => new
            {
                lensId = l.Id,
                l.Name,
                description = l.GeneratorDescription,
                l.AddressedCriterion,
                l.RepairHint,
                l.Mandatory
            }).ToArray()
        };
        _run.AppendEvent(new { type = "L3_AGENTIC_TOOL_GET_COVERAGE_LENSES", runId = _run.RunId, spec = _spec.Id, lenses = _spec.Lenses.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(payload, Json);
    }

    private string CheckLensCoverage(L3CandidateDraftDto[] candidates)
    {
        var round = Interlocked.Increment(ref _coverageCheckRounds);
        var items = L3CandidateParsing.AssignIds(ToCandidates(candidates ?? []));
        var report = L3LensCoverage.Evaluate(_spec, items);
        _run.AppendEvent(new
        {
            type = "L3_AGENTIC_TOOL_CHECK_LENS_COVERAGE",
            runId = _run.RunId,
            round,
            candidates = items.Count,
            report.AddressedLenses,
            report.TotalLenses,
            report.MandatoryUnaddressed,
            timestampUtc = DateTime.UtcNow
        });
        return JsonSerializer.Serialize(report, Json);
    }

    private string CheckLensAdequacy(L3CandidateDraftDto[] candidates)
    {
        var round = Interlocked.Increment(ref _adequacyCheckRounds);
        var items = L3CandidateParsing.AssignIds(ToCandidates(candidates ?? []));
        var knownLensIds = _spec.LensIds;
        var byLens = _spec.Lenses.Select(l =>
        {
            var lensItems = items.Where(c => string.Equals(c.GapCategory, l.Id, StringComparison.Ordinal)).ToArray();
            var warnings = new List<string>();
            if (lensItems.Length == 0)
            {
                return new
                {
                    lensId = l.Id,
                    l.Name,
                    l.Mandatory,
                    candidateCount = 0,
                    adequacy = l.Mandatory ? "missing" : "not_applicable",
                    warnings = l.Mandatory
                        ? new List<string> { "missing_candidate_for_mandatory_lens" }
                        : new List<string>(),
                    guidance = l.Mandatory
                        ? "Ergaenze mindestens einen projektspezifischen Kandidaten fuer diese Linse oder begruende spaeter im Review, warum sie nicht anwendbar ist."
                        : "Optionale Linse ohne Kandidat; nur relevant, wenn sie fuer diese Umwelt fachlich passt.",
                    likelyRequiresHumanDecisionCandidateIds = Array.Empty<string>()
                };
            }

            if (lensItems.Any(c => string.IsNullOrWhiteSpace(c.Rationale))) warnings.Add("missing_rationale");
            if (lensItems.Any(c => string.IsNullOrWhiteSpace(c.ImpactIfMissing))) warnings.Add("missing_impact_if_missing");
            if (lensItems.Any(c => (c.BasedOn?.Count ?? 0) == 0)) warnings.Add("missing_based_on");
            if (lensItems.Any(c => string.IsNullOrWhiteSpace(c.Intent))) warnings.Add("missing_intent");
            if (lensItems.Any(c => c.Text.Length < 80)) warnings.Add("candidate_text_too_short_for_review");
            if (lensItems.Any(c => !knownLensIds.Contains(c.GapCategory ?? string.Empty))) warnings.Add("unknown_gap_category");
            if (l.Mandatory && lensItems.Length == 1) warnings.Add("mandatory_lens_single_candidate_only");

            var humanDecisionWarnings = lensItems
                .Where(c => c.RequiresHumanDecision != true && LooksLikeHumanDecision(c))
                .Select(c => c.CandidateId)
                .ToArray();
            if (humanDecisionWarnings.Length > 0) warnings.Add("likely_requires_human_decision");

            var hasCoreFields = lensItems.All(c =>
                !string.IsNullOrWhiteSpace(c.Rationale) &&
                !string.IsNullOrWhiteSpace(c.ImpactIfMissing) &&
                (c.BasedOn?.Count ?? 0) > 0);
            var hasReviewableText = lensItems.All(c => c.Text.Length >= 80);
            var hasLikelyHumanDecisionMiss = humanDecisionWarnings.Length > 0;
            var adequacy = warnings.Count == 0
                ? "substantive"
                : hasCoreFields && hasReviewableText && !hasLikelyHumanDecisionMiss
                    ? "thin"
                    : "uncertain";

            return new
            {
                lensId = l.Id,
                l.Name,
                l.Mandatory,
                candidateCount = lensItems.Length,
                adequacy,
                warnings,
                guidance = adequacy switch
                {
                    "thin" => "Pruefe, ob die Linse nur formal abgedeckt ist. Ergaenze Impact, Projektspezifik oder zweiten Kandidaten, falls die Luecke wesentlich ist.",
                    "uncertain" => "Ueberarbeite vor dem Speichern: Kernfelder, basedOn, Human-Decision-Flag oder Kategorie wirken nicht belastbar.",
                    _ => "Wirkt als reviewbarer, substanzieller Kandidatensatz fuer diese Linse."
                },
                likelyRequiresHumanDecisionCandidateIds = humanDecisionWarnings
            };
        }).ToArray();
        var unknown = items.Where(c => !string.IsNullOrWhiteSpace(c.GapCategory) && !_spec.LensIds.Contains(c.GapCategory!))
            .Select(c => new { c.CandidateId, c.GapCategory })
            .ToArray();
        _run.AppendEvent(new
        {
            type = "L3_AGENTIC_TOOL_CHECK_LENS_ADEQUACY",
            runId = _run.RunId,
            round,
            candidates = items.Count,
            substantive = byLens.Count(l => l.adequacy == "substantive"),
            thin = byLens.Count(l => l.adequacy == "thin"),
            missing = byLens.Count(l => l.adequacy == "missing"),
            uncertain = byLens.Count(l => l.adequacy == "uncertain"),
            notApplicable = byLens.Count(l => l.adequacy == "not_applicable"),
            unknownCategories = unknown.Length,
            timestampUtc = DateTime.UtcNow
        });
        return JsonSerializer.Serialize(new
        {
            round,
            note = "Heuristik: leichtes RE-Substanzfeedback fuer Agentenrevision, keine finale fachliche Freigabe.",
            rubric = new[] { "substantive", "thin", "missing", "uncertain", "not_applicable" },
            byLens,
            unknownCategories = unknown
        }, Json);
    }

    private static bool LooksLikeHumanDecision(L3Candidate candidate)
    {
        var text = string.Join(' ', candidate.Text, candidate.Rationale ?? string.Empty, candidate.ImpactIfMissing ?? string.Empty);
        var markers = new[]
        {
            "entscheid", "festzulegen", "klaeren", "klären", "scope", "mvp", "prior", "rolle", "rollen",
            "berechtigung", "datenschutz", "rechtsgrundlage", "compliance", "einwilligung", "betrieb",
            "offline", "synchron", "import", "schnittstelle", "verantwort", "angehoerige", "angehörige",
            "bewohner", "open-world"
        };
        return markers.Any(m => text.Contains(m, StringComparison.OrdinalIgnoreCase));
    }

    private string SaveL3Candidates(L3CandidateDraftDto[] candidates)
    {
        if (Interlocked.Increment(ref _saveCount) > 1) return "IGNORED: save_l3_candidates wurde bereits erfolgreich aufgerufen.";
        var items = L3CandidateParsing.AssignIds(ToCandidates(candidates ?? []));
        _savedCandidates.Clear();
        _savedCandidates.AddRange(items);
        Directory.CreateDirectory(_outDir);
        File.WriteAllText(Path.Combine(_outDir, "l3-agentic-candidates.json"), JsonSerializer.Serialize(new
        {
            runId = _run.RunId,
            model = _model,
            generatedAtUtc = DateTime.UtcNow,
            retrievedItemIds = _retrieved.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            coverageCheckRounds = _coverageCheckRounds,
            adequacyCheckRounds = _adequacyCheckRounds,
            candidates = items
        }, Json));
        _run.AppendEvent(new { type = "L3_AGENTIC_TOOL_SAVE_CANDIDATES", runId = _run.RunId, candidates = items.Count, retrieved = _retrieved.Count, timestampUtc = DateTime.UtcNow });
        return $"OK: {items.Count} L3-Kandidaten gespeichert. Downstream-Resolve/Validate/Judge/Routing kann starten.";
    }

    private static List<L3Candidate> ToCandidates(IEnumerable<L3CandidateDraftDto> drafts)
    {
        var result = new List<L3Candidate>();
        foreach (var c in drafts)
        {
            var text = c.Text?.Trim();
            if (string.IsNullOrWhiteSpace(text)) continue;
            result.Add(new L3Candidate("",
                string.IsNullOrWhiteSpace(c.TargetType) ? "requirement" : c.TargetType!.Trim(),
                text,
                string.IsNullOrWhiteSpace(c.Rationale) ? null : c.Rationale!.Trim(),
                Clean(c.Assumptions),
                string.IsNullOrWhiteSpace(c.Intent) ? null : c.Intent!.Trim().ToLowerInvariant(),
                Clean(c.BasedOn),
                string.IsNullOrWhiteSpace(c.GapCategory) ? null : c.GapCategory!.Trim().ToLowerInvariant(),
                string.IsNullOrWhiteSpace(c.ImpactIfMissing) ? null : c.ImpactIfMissing!.Trim(),
                c.RequiresHumanDecision));
        }
        return result;
    }

    private static IReadOnlyList<string> Clean(IEnumerable<string?>? values)
        => (values ?? []).Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim()).Distinct(StringComparer.Ordinal).ToArray();

    private static string SafeText(ArtifactItem item) => item.Text ?? string.Empty;
    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max] + "...";

    public sealed record L3CandidateDraftDto(
        [property: JsonPropertyName("text")] string? Text,
        [property: JsonPropertyName("targetType")] string? TargetType,
        [property: JsonPropertyName("intent")] string? Intent,
        [property: JsonPropertyName("gapCategory")] string? GapCategory,
        [property: JsonPropertyName("basedOn")] string[]? BasedOn,
        [property: JsonPropertyName("rationale")] string? Rationale,
        [property: JsonPropertyName("impactIfMissing")] string? ImpactIfMissing,
        [property: JsonPropertyName("assumptions")] string[]? Assumptions,
        [property: JsonPropertyName("requiresHumanDecision")] bool? RequiresHumanDecision);
}
