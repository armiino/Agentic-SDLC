using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.MakerChecker;

/// <summary>
/// C7 / MC3 — der bounded Evidence-Support-Critic. Prüft, ob jedes konkrete Detail einer Artefakt-Zeile durch ihr
/// ZITIERTES Claim-Paket (proposition + facets + evidence + notes) gedeckt ist. Das ist der Check, den das
/// deterministische MC0-Gate NICHT leisten kann (eine gültige Citation beweist Herkunft, nicht Deckung — die
/// „Cited but Not Verified"-Lücke; s. <c>makerchecker/Plan.md</c> §4.4/§4.5).
/// </summary>
/// <remarks>
/// Bewusst KEIN offener Review gegen das Rohtranskript. Zwei Kostendämpfer aus Plan §4.6:
/// (1) <b>Delta-Vorfilter</b> (deterministisch, 0 LLM): Zeilen, deren Content-Tokens quasi vollständig im Paket
///     stehen, gelten ohne LLM als <c>supported</c> — nur Zeilen mit echtem Detail-Delta gehen an den Critic.
/// (2) <b>Bündelung</b>: mehrere Kandidaten pro Call (FacetValidator-Muster, temp=0, structured output, fixer Nenner).
/// </remarks>
public sealed class ContractCritic
{
    public const int DefaultBatchSize = 10;
    // Zeile gilt ohne LLM als supported, wenn dieser Anteil ihrer Content-Tokens im zitierten Paket vorkommt.
    // Hoch gewählt (konservativ): im Zweifel lieber an den Critic geben, kein erfundenes Detail durchwinken.
    public const double DefaultCoverageSkip = 0.9;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private const string SystemPrompt = """
        Du bist ein EVIDENCE-SUPPORT-Prüfer. Für JEDE Artefakt-Zeile bekommst du die Zeile UND das/die zitierte(n)
        Claim-Paket(e) (proposition, facets, evidence-Zitate, notes). Deine EINZIGE Frage:
        Ist JEDES konkrete Detail der Zeile durch das zitierte Paket gedeckt?

        Prüfe NUR gegen das gelieferte Paket. KEIN Weltwissen, KEIN Rohtranskript, KEINE Annahmen.

        Verdikt pro Zeile (genau EINES):
        - "supported": alle konkreten Details (Funktionen, Zahlen, Namen, Technologien, Bedingungen) stehen im Paket
          (proposition ODER evidence ODER notes). Zusammenfassen/Umformulieren/Paraphrase ist erlaubt.
        - "evidence_unsupported_detail": die Zeile enthält ein KONKRETES Detail, das im Paket NICHT vorkommt
          (erfunden oder aus Weltwissen). Gib den betroffenen Textausschnitt in unsupportedSpan an.
        - "facet_overstated": die Zeile verstärkt den Entscheidungsstand über die Facette hinaus
          (status open->entschieden, modality desired->muss, timeScope unklar/später->MVP).
        - "unclear": nicht entscheidbar.

        WICHTIG (keine Fehlalarme):
        - Eine KLÄRUNGS-Formulierung ist bei modality=must_clarify/must_consider treu, NICHT facet_overstated:
          „muss noch geklärt/evaluiert/spezifiziert/konkretisiert werden" == supported.
        - „muss" ist supported, wenn die Facette modality=must ist.
        - Fehlt in der Zeile ein konkretes Zusatzdetail (reine Paraphrase der proposition) -> supported.

        Für JEDE Input-Zeile GENAU EIN Objekt mit DERSELBEN ref zurück (gleiche Anzahl, keine zusätzlichen/fehlenden refs).
        Antworte ausschliesslich mit JSON:
        {
          "items": [
            { "ref": "<exakt die Input-ref>", "verdict": "supported",
              "rationale": "<kurze Begründung>", "unsupportedSpan": "" }
          ]
        }
        """;

    private const string SchemaTemplate = """
        {
          "type": "object",
          "properties": {
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  __REASONING_PROP__"ref": { "type": "string" },
                  "verdict": { "type": "string" },
                  "rationale": { "type": "string" },
                  "unsupportedSpan": { "type": "string" }
                },
                "required": [__REASONING_REQ__"ref","verdict","rationale","unsupportedSpan"],
                "additionalProperties": false
              }
            }
          },
          "required": ["items"],
          "additionalProperties": false
        }
        """;

    // W1a: Schema je ReasoningCapture-Modus (reasoning-Property/required via Marker-Ersetzung, kein Brace-Doubling).
    private static string BuildSchemaJson(ReasoningCapture reasoning) => SchemaTemplate
        .Replace("__REASONING_PROP__", ReasoningSchema.PropertyJson(reasoning))
        .Replace("__REASONING_REQ__", ReasoningSchema.RequiredToken(reasoning));

    private static readonly HashSet<string> Stop = new(StringComparer.OrdinalIgnoreCase)
    {
        "soll","sollen","sollte","muss","müssen","kann","könnte","können","eine","einen","einem","eines","einer",
        "oder","und","auch","dass","werden","wird","sein","dies","diese","dieser","dieses","nach","über","dem",
        "den","der","die","das","für","mit","von","zum","zur","als","bzw","etc","noch","nicht","wenn","bei","aus",
        "auf","ist","sind","damit","sowie","z.b","z. b","etwa","noch","wie","z.b.","ihre","ihres","ihrem","ihren"
    };

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly int _batchSize;
    private readonly double _coverageSkip;
    private readonly string _systemPrompt;
    private readonly ChatResponseFormat _responseFormat;

    public ContractCritic(IChatClient client, bool structuredOutput = true,
        int batchSize = DefaultBatchSize, double coverageSkip = DefaultCoverageSkip,
        ReasoningCapture reasoning = ReasoningCapture.Enforced)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        _batchSize = Math.Clamp(batchSize, 1, 16);
        _coverageSkip = Math.Clamp(coverageSkip, 0.0, 1.0);
        // W1a: reasoning log-only via Prompt-Appendix + Schema-Feld (bei off beides leer).
        _systemPrompt = SystemPrompt + ReasoningSchema.PromptAppendix(reasoning);
        _responseFormat = ChatResponseFormat.ForJsonSchema(
            JsonDocument.Parse(BuildSchemaJson(reasoning)).RootElement.Clone(),
            "evidence_support_batch",
            "Evidence-Support-Verdikt je Artefakt-Zeile gegen das zitierte Claim-Paket (fixer Nenner).");
    }

    public async Task<CriticReport> CheckAsync(string markdown, ConsumableLedger ledger, CancellationToken ct)
    {
        var claims = new Dictionary<string, SemanticLedgerEntry>(StringComparer.Ordinal);
        foreach (var c in ledger.Claims) claims[c.Id] = c;

        var lines = ContractChecker_ParseLines(markdown);
        var verdicts = new List<CriticLineVerdict>();
        var candidates = new List<(int LineNo, string Text, List<SemanticLedgerEntry> Pkgs)>();

        var citedCandidates = 0;
        foreach (var (lineNo, text, ids) in lines)
        {
            var pkgs = ids.Where(claims.ContainsKey).Select(id => claims[id]).ToList();
            if (pkgs.Count == 0) continue; // C1/C2-Fälle sind MC0-Sache, nicht Critic.
            citedCandidates++;

            // Delta-Vorfilter (0 LLM): quasi vollständige Token-Deckung durch das Paket -> supported ohne LLM.
            if (Coverage(text, pkgs) >= _coverageSkip)
            {
                verdicts.Add(new CriticLineVerdict(lineNo, Trim(text), pkgs.Select(p => p.Id).ToList(),
                    C7Verdict.Supported, ByLlm: false, Rationale: "Delta-Vorfilter: Content vollständig im Paket.", UnsupportedSpan: null));
                continue;
            }
            candidates.Add((lineNo, text, pkgs));
        }

        var llmCalls = 0;
        for (var i = 0; i < candidates.Count; i += _batchSize)
        {
            var batch = candidates.Skip(i).Take(_batchSize).ToList();
            verdicts.AddRange(await CheckBatchAsync(batch, ct).ConfigureAwait(false));
            llmCalls++;
        }

        var violations = new List<ContractViolation>();
        foreach (var v in verdicts.Where(v => v.Verdict is C7Verdict.EvidenceUnsupportedDetail or C7Verdict.FacetOverstated))
        {
            var code = v.Verdict == C7Verdict.EvidenceUnsupportedDetail
                ? ContractCodes.EvidenceUnsupportedDetail : ContractCodes.FacetOverstated;
            violations.Add(new ContractViolation(
                code, ContractSeverity.Error, Repairable: true,
                Message: v.Rationale ?? code,
                LineNumber: v.LineNumber, ArtifactQuote: v.Line, ClaimIds: v.ClaimIds,
                LedgerFacets: new Dictionary<string, string>(),
                SuggestedAction: v.Verdict == C7Verdict.EvidenceUnsupportedDetail
                    ? $"Nicht gedecktes Detail entfernen oder belegen: '{v.UnsupportedSpan}'."
                    : "Formulierung an die Facette angleichen."));
        }

        var pass = violations.Count == 0;
        return new CriticReport(
            pass, lines.Count, citedCandidates,
            SkippedByPrefilter: verdicts.Count(v => !v.ByLlm),
            LlmChecked: candidates.Count, LlmCalls: llmCalls,
            Verdicts: verdicts.OrderBy(v => v.LineNumber).ToList(), Violations: violations);
    }

    /// <summary>
    /// Majority-Vote (§4.4-Konsequenz aus dem Test-Retest): führt den Check k-mal aus und aggregiert je Zeile per
    /// Mehrheit. Eine Zeile zählt nur dann als Verstoß, wenn ≥ <paramref name="minViolationVotes"/> der Läufe sie
    /// flaggen (Default = strikte Mehrheit ⌊k/2⌋+1). Das entfernt sowohl spurious False Positives (v2 L27, 1/3) als
    /// auch Grenzfall-Rauschen (risks L6/L41, 2/5) und lässt den stabilen Kern (5/5) stehen.
    /// </summary>
    public async Task<CriticVoteReport> CheckWithVoteAsync(
        string markdown, ConsumableLedger ledger, int k, int minViolationVotes, CancellationToken ct)
    {
        k = Math.Max(1, k);
        var runs = new List<CriticReport>(k);
        for (var i = 0; i < k; i++)
            runs.Add(await CheckAsync(markdown, ledger, ct).ConfigureAwait(false));

        var byLine = new Dictionary<int, List<CriticLineVerdict>>();
        foreach (var r in runs)
            foreach (var v in r.Verdicts)
            {
                if (!byLine.TryGetValue(v.LineNumber, out var list)) { list = []; byLine[v.LineNumber] = list; }
                list.Add(v);
            }

        var effMinVotes = minViolationVotes > 0 ? minViolationVotes : (k / 2 + 1);

        var lineVotes = new List<CriticLineVote>();
        var violations = new List<ContractViolation>();
        foreach (var (lineNo, list) in byLine.OrderBy(kv => kv.Key))
        {
            var dist = list.GroupBy(v => v.Verdict).ToDictionary(g => g.Key.ToString(), g => g.Count());
            var violationVotes = list.Count(v => v.Verdict is C7Verdict.EvidenceUnsupportedDetail or C7Verdict.FacetOverstated);
            var byLlm = list.Any(v => v.ByLlm);
            var line = list[0].Line;
            var claimIds = list[0].ClaimIds;

            C7Verdict aggregated;
            string? rationale = null, span = null;
            if (violationVotes >= effMinVotes)
            {
                // Mehrheits-Verstoßcode unter den Violation-Verdikten (deterministischer Tie-Break).
                var codeGroup = list
                    .Where(v => v.Verdict is C7Verdict.EvidenceUnsupportedDetail or C7Verdict.FacetOverstated)
                    .GroupBy(v => v.Verdict).OrderByDescending(g => g.Count()).ThenBy(g => g.Key.ToString()).First();
                aggregated = codeGroup.Key;
                var rep = codeGroup.First();
                rationale = rep.Rationale; span = rep.UnsupportedSpan;

                var reportCode = aggregated == C7Verdict.EvidenceUnsupportedDetail
                    ? ContractCodes.EvidenceUnsupportedDetail : ContractCodes.FacetOverstated;
                violations.Add(new ContractViolation(
                    reportCode, ContractSeverity.Error, Repairable: true,
                    Message: $"{rationale} (Mehrheit: {violationVotes}/{k})",
                    LineNumber: lineNo, ArtifactQuote: line, ClaimIds: claimIds,
                    LedgerFacets: new Dictionary<string, string> { ["votes"] = FormatDist(dist) },
                    SuggestedAction: aggregated == C7Verdict.EvidenceUnsupportedDetail
                        ? $"Nicht gedecktes Detail entfernen/belegen: '{span}'."
                        : "Formulierung an die Facette angleichen."));
            }
            else
            {
                aggregated = C7Verdict.Supported; // unter Schwelle -> kein Verstoß (Rauschen ausgevotet)
            }

            lineVotes.Add(new CriticLineVote(lineNo, line, claimIds, aggregated, byLlm, dist, violationVotes, list.Count, rationale, span));
        }

        return new CriticVoteReport(
            Pass: violations.Count == 0, K: k, MinViolationVotes: effMinVotes,
            RequirementLines: runs[0].RequirementLines, CitedCandidates: runs[0].CitedCandidates,
            SkippedByPrefilter: runs[0].SkippedByPrefilter, LlmCheckedPerRun: runs[0].LlmChecked,
            TotalLlmCalls: runs.Sum(r => r.LlmCalls),
            Votes: lineVotes, Violations: violations);
    }

    private static string FormatDist(IReadOnlyDictionary<string, int> d)
        => string.Join(",", d.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}"));

    private async Task<IReadOnlyList<CriticLineVerdict>> CheckBatchAsync(
        IReadOnlyList<(int LineNo, string Text, List<SemanticLedgerEntry> Pkgs)> batch, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = _responseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, _systemPrompt), new ChatMessage(ChatRole.User, BuildUser(batch))],
            options, ct).ConfigureAwait(false);

        var parsed = Parse(response.Text).ToDictionary(v => v.Ref, v => v, StringComparer.OrdinalIgnoreCase);

        var results = new List<CriticLineVerdict>(batch.Count);
        var missing = new List<string>();
        foreach (var (lineNo, text, pkgs) in batch)
        {
            var refId = $"L{lineNo}";
            var ids = pkgs.Select(p => p.Id).ToList();
            if (!parsed.TryGetValue(refId, out var raw))
            {
                missing.Add(refId);
                results.Add(new CriticLineVerdict(lineNo, Trim(text), ids, C7Verdict.Unclear, ByLlm: true,
                    Rationale: "Kein Verdikt vom Modell (fail-open zu unclear).", UnsupportedSpan: null));
                continue;
            }
            results.Add(new CriticLineVerdict(lineNo, Trim(text), ids, MapVerdict(raw.Verdict), ByLlm: true,
                Rationale: raw.Rationale, UnsupportedSpan: string.IsNullOrWhiteSpace(raw.UnsupportedSpan) ? null : raw.UnsupportedSpan));
        }
        if (missing.Count > 0)
            Console.Error.WriteLine($"[contract-critic] WARN: fehlende Verdikte für {string.Join(",", missing)} (fail-open=unclear).");
        return results;
    }

    private static string BuildUser(IReadOnlyList<(int LineNo, string Text, List<SemanticLedgerEntry> Pkgs)> batch)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ZEILEN ({batch.Count}) — gib GENAU {batch.Count} Verdikte zurück, eines pro ref:");
        sb.AppendLine();
        foreach (var (lineNo, text, pkgs) in batch)
        {
            sb.AppendLine($"ref: L{lineNo}");
            sb.AppendLine($"  zeile: {StripIds(text)}");
            sb.AppendLine("  zitierte claim-pakete:");
            foreach (var p in pkgs)
            {
                sb.AppendLine($"    - id: {p.Id}  [status={p.Status}, modality={p.Modality}, timeScope={p.TimeScope ?? "?"}]");
                sb.AppendLine($"      proposition: {p.Proposition}");
                var ev = (p.Evidence ?? []).Select(e => e.Quote).Where(q => !string.IsNullOrWhiteSpace(q)).ToList();
                if (ev.Count > 0) sb.AppendLine($"      evidence: {string.Join(" | ", ev)}");
                if (!string.IsNullOrWhiteSpace(p.Notes)) sb.AppendLine($"      notes: {p.Notes}");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    // ---- Delta-Vorfilter: Content-Token-Deckung durch das Paket (crude Stemming für dt. Flexion). ----
    private static double Coverage(string line, IReadOnlyList<SemanticLedgerEntry> pkgs)
    {
        var lineTokens = ContentTokens(StripIds(line));
        if (lineTokens.Count == 0) return 1.0;

        var pkg = new StringBuilder();
        foreach (var p in pkgs)
        {
            pkg.Append(' ').Append(p.Proposition);
            foreach (var e in p.Evidence ?? []) pkg.Append(' ').Append(e.Quote);
            if (p.Notes is not null) pkg.Append(' ').Append(p.Notes);
        }
        var pkgTokens = ContentTokens(pkg.ToString());

        var covered = lineTokens.Count(t => pkgTokens.Any(p => Stem(p) == Stem(t)));
        return (double)covered / lineTokens.Count;
    }

    private static List<string> ContentTokens(string s)
    {
        var tokens = new List<string>();
        foreach (var raw in s.ToLowerInvariant().Split(
            [' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '„', '"', '"', '/', '–', '-', '*', '#'],
            StringSplitOptions.RemoveEmptyEntries))
        {
            if (raw.Length < 4) continue;
            if (Stop.Contains(raw)) continue;
            tokens.Add(raw);
        }
        return tokens;
    }

    private static string Stem(string t) => t.Length <= 5 ? t : t[..5];

    private static string StripIds(string line)
    {
        var i = line.IndexOf('[');
        return i < 0 ? line : line[..i].Trim();
    }

    private static C7Verdict MapVerdict(string? v) => (v ?? "").Trim().ToLowerInvariant() switch
    {
        "supported" => C7Verdict.Supported,
        "evidence_unsupported_detail" => C7Verdict.EvidenceUnsupportedDetail,
        "facet_overstated" => C7Verdict.FacetOverstated,
        _ => C7Verdict.Unclear
    };

    private static IReadOnlyList<RawVerdict> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return [];
        try
        {
            return JsonSerializer.Deserialize<RawResult>(json, Json)?.Items?.Where(v => !string.IsNullOrWhiteSpace(v.Ref)).ToList() ?? [];
        }
        catch (JsonException) { return []; }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private static string Trim(string s) => s.Length <= 220 ? s : s[..220] + "…";

    // Parser identisch zu ContractChecker (Markdown-Listenpunkt + [ids]); hier lokal gehalten, um den Critic
    // eigenständig laufen zu lassen. Bei Änderung dort mitziehen.
    private static List<(int LineNumber, string Text, IReadOnlyList<string> Ids)> ContractChecker_ParseLines(string markdown)
    {
        var result = new List<(int, string, IReadOnlyList<string>)>();
        var raw = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        for (var i = 0; i < raw.Length; i++)
        {
            var line = raw[i];
            if (!System.Text.RegularExpressions.Regex.IsMatch(line, @"^\s*[-*]\s+")) continue;
            var ids = new List<string>();
            foreach (System.Text.RegularExpressions.Match m in
                     System.Text.RegularExpressions.Regex.Matches(line, @"\[([a-zA-Z0-9_\-, ]+)\]"))
                foreach (var tok in m.Groups[1].Value.Split(','))
                {
                    var id = tok.Trim();
                    if (id.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(id, @"^\d+$")) ids.Add(id);
                }
            result.Add((i + 1, line.Trim(), ids));
        }
        return result;
    }

    private sealed record RawResult([property: JsonPropertyName("items")] IReadOnlyList<RawVerdict> Items);
    private sealed record RawVerdict(
        [property: JsonPropertyName("ref")] string Ref,
        [property: JsonPropertyName("verdict")] string Verdict,
        [property: JsonPropertyName("rationale")] string? Rationale,
        [property: JsonPropertyName("unsupportedSpan")] string? UnsupportedSpan);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum C7Verdict { Supported, EvidenceUnsupportedDetail, FacetOverstated, Unclear }

public sealed record CriticLineVerdict(
    [property: JsonPropertyName("lineNumber")] int LineNumber,
    [property: JsonPropertyName("line")] string Line,
    [property: JsonPropertyName("claimIds")] IReadOnlyList<string> ClaimIds,
    [property: JsonPropertyName("verdict")] C7Verdict Verdict,
    [property: JsonPropertyName("byLlm")] bool ByLlm,
    [property: JsonPropertyName("rationale")] string? Rationale,
    [property: JsonPropertyName("unsupportedSpan")] string? UnsupportedSpan);

public sealed record CriticReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("requirementLines")] int RequirementLines,
    [property: JsonPropertyName("citedCandidates")] int CitedCandidates,
    [property: JsonPropertyName("skippedByPrefilter")] int SkippedByPrefilter,
    [property: JsonPropertyName("llmChecked")] int LlmChecked,
    [property: JsonPropertyName("llmCalls")] int LlmCalls,
    [property: JsonPropertyName("verdicts")] IReadOnlyList<CriticLineVerdict> Verdicts,
    [property: JsonPropertyName("violations")] IReadOnlyList<ContractViolation> Violations);

/// <summary>Ein Zeilen-Ergebnis nach Majority-Vote: aggregiertes Verdikt + Stimmenverteilung über die k Läufe.</summary>
public sealed record CriticLineVote(
    [property: JsonPropertyName("lineNumber")] int LineNumber,
    [property: JsonPropertyName("line")] string Line,
    [property: JsonPropertyName("claimIds")] IReadOnlyList<string> ClaimIds,
    [property: JsonPropertyName("aggregatedVerdict")] C7Verdict AggregatedVerdict,
    [property: JsonPropertyName("byLlm")] bool ByLlm,
    [property: JsonPropertyName("votes")] IReadOnlyDictionary<string, int> Votes,
    [property: JsonPropertyName("violationVotes")] int ViolationVotes,
    [property: JsonPropertyName("k")] int K,
    [property: JsonPropertyName("rationale")] string? Rationale,
    [property: JsonPropertyName("unsupportedSpan")] string? UnsupportedSpan);

/// <summary>k-Vote-Report: nur mehrheitlich bestätigte Verstöße zählen (stabil statt single-shot-noisy).</summary>
public sealed record CriticVoteReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("k")] int K,
    [property: JsonPropertyName("minViolationVotes")] int MinViolationVotes,
    [property: JsonPropertyName("requirementLines")] int RequirementLines,
    [property: JsonPropertyName("citedCandidates")] int CitedCandidates,
    [property: JsonPropertyName("skippedByPrefilter")] int SkippedByPrefilter,
    [property: JsonPropertyName("llmCheckedPerRun")] int LlmCheckedPerRun,
    [property: JsonPropertyName("totalLlmCalls")] int TotalLlmCalls,
    [property: JsonPropertyName("votes")] IReadOnlyList<CriticLineVote> Votes,
    [property: JsonPropertyName("violations")] IReadOnlyList<ContractViolation> Violations);
