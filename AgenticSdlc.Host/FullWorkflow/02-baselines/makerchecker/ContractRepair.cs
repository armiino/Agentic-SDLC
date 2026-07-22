using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.MakerChecker;

/// <summary>
/// MC2 — bounded Repair. Patcht NUR die von einem Verstoß betroffenen Zeilen (nie das ganze Artefakt), damit ein
/// erfundenes/ungegroundetes Detail entfernt oder eine Facetten-Verstärkung entschärft wird — Zeile bleibt sonst
/// unverändert, Citation bleibt erhalten. Bounded LLM (temp=0, structured output, fixer Nenner — FacetValidator-
/// Muster). Der eigentliche Loop (Repair → erneuter Check) liegt beim Aufrufer; diese Klasse macht genau EINEN
/// Patch-Durchgang über die übergebenen Verstöße.
/// </summary>
/// <remarks>
/// Repariert nur Zeilen-Verstöße mit LineNumber: EVIDENCE_UNSUPPORTED_DETAIL (Detail entfernen) und
/// FACET_OVERSTATED (abschwächen). REQUIRED_CLAIM_UNUSED (= Zeile HINZUFÜGEN) und die nicht-reparierbaren
/// Struktur-Codes (MISSING_CITATION/UNKNOWN_ID/WRONG_DISPOSITION → HumanReview) sind hier bewusst NICHT dabei.
/// </remarks>
public sealed class ContractRepair
{
    public const int DefaultBatchSize = 8;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Regex IdRx = new(@"\[([a-zA-Z0-9_\-, ]+)\]", RegexOptions.Compiled);

    private const string SystemPrompt = """
        Du REPARIERST einzelne Artefakt-Zeilen, sodass sie TREU zum zitierten Claim-Paket werden. Du bekommst je
        Zeile: den aktuellen Text, den Verstoß (+ beanstandeten Ausschnitt) und das/die zitierte(n) Claim-Paket(e).

        Regeln:
        - Ändere NUR das Beanstandete. Der Rest der Zeile bleibt wortgleich.
        - EVIDENCE_UNSUPPORTED_DETAIL: ENTFERNE das nicht gedeckte Detail und behalte die belegte Kernaussage.
          Erfinde KEINEN Ersatz. Lieber kürzer und belegt als länger und ungegroundet.
        - FACET_OVERSTATED: schwäche die Formulierung so ab, dass status/modality/timeScope NICHT verstärkt werden
          (z. B. "muss" -> "soll/ist gewünscht", "entschieden" -> "offen"), passend zur genannten Facette.
        - Behalte ALLE [claimId]-Zitate der Zeile unverändert bei.
        - Gib EINE einzelne Zeile zurück (keine Zeilenumbrüche, keine zusätzlichen Listenpunkte).

        Für JEDE Input-Zeile GENAU EIN Objekt mit DERSELBEN ref (gleiche Anzahl, keine zusätzlichen/fehlenden refs).
        Antworte ausschliesslich mit JSON:
        { "items": [ { "ref": "<exakt die Input-ref>", "repairedText": "- … [claimId]" } ] }
        """;

    private static readonly string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": { "ref": { "type": "string" }, "repairedText": { "type": "string" } },
                "required": ["ref","repairedText"],
                "additionalProperties": false
              }
            }
          },
          "required": ["items"],
          "additionalProperties": false
        }
        """;

    private static readonly ChatResponseFormat ResponseFormat = ChatResponseFormat.ForJsonSchema(
        JsonDocument.Parse(SchemaJson).RootElement.Clone(),
        "line_repair_batch",
        "Reparierte Artefakt-Zeilen (nur betroffene, fixer Nenner).");

    private readonly IChatClient _client;
    private readonly bool _structuredOutput;
    private readonly int _batchSize;

    public ContractRepair(IChatClient client, bool structuredOutput = true, int batchSize = DefaultBatchSize)
    {
        _client = client;
        _structuredOutput = structuredOutput;
        _batchSize = Math.Clamp(batchSize, 1, 12);
    }

    /// <summary>Ein Patch-Durchgang: repariert die übergebenen Zeilen-Verstöße und liefert das gepatchte Markdown
    /// + ein Protokoll je Zeile. Zeilennummern bleiben stabil (Zeile→Zeile, kein Hinzufügen/Löschen).</summary>
    public async Task<(string Markdown, IReadOnlyList<RepairResult> Repairs)> RepairAsync(
        string markdown, ConsumableLedger ledger, IReadOnlyList<ContractViolation> violations, CancellationToken ct)
    {
        var claims = new Dictionary<string, SemanticLedgerEntry>(StringComparer.Ordinal);
        foreach (var c in ledger.Claims) claims[c.Id] = c;

        // Nur reparierbare Zeilen-Verstöße mit LineNumber; je Zeile den (ersten) Verstoß nehmen.
        var targets = violations
            .Where(v => v.LineNumber is int && v.Repairable
                        && (v.Code == ContractCodes.EvidenceUnsupportedDetail || v.Code == ContractCodes.FacetOverstated))
            .GroupBy(v => v.LineNumber!.Value)
            .Select(g => g.First())
            .OrderBy(v => v.LineNumber!.Value)
            .ToList();

        var lines = markdown.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var repairs = new List<RepairResult>();
        if (targets.Count == 0) return (markdown, repairs);

        var patched = new Dictionary<int, string>(); // 1-basierte Zeilennummer -> neuer Text
        for (var i = 0; i < targets.Count; i += _batchSize)
        {
            var batch = targets.Skip(i).Take(_batchSize).ToList();
            var byRef = await RepairBatchAsync(batch, lines, claims, ct).ConfigureAwait(false);
            foreach (var (lineNo, text) in byRef) patched[lineNo] = text;
        }

        foreach (var v in targets)
        {
            var lineNo = v.LineNumber!.Value;
            var original = lineNo - 1 < lines.Length ? lines[lineNo - 1] : string.Empty;
            if (patched.TryGetValue(lineNo, out var repaired) && IsValidRepair(original, repaired))
            {
                lines[lineNo - 1] = repaired;
                repairs.Add(new RepairResult(lineNo, original.Trim(), repaired.Trim(), v.Code, Changed: !string.Equals(original.Trim(), repaired.Trim(), StringComparison.Ordinal)));
            }
            else
            {
                // Fail-safe: ungültiger/fehlender Patch -> Original behalten, als "nicht repariert" protokollieren.
                repairs.Add(new RepairResult(lineNo, original.Trim(), original.Trim(), v.Code, Changed: false));
            }
        }

        return (string.Join("\n", lines), repairs);
    }

    private async Task<IReadOnlyDictionary<int, string>> RepairBatchAsync(
        IReadOnlyList<ContractViolation> batch, string[] lines,
        IReadOnlyDictionary<string, SemanticLedgerEntry> claims, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0.0f };
        if (_structuredOutput) options.ResponseFormat = ResponseFormat;

        var response = await _client.GetResponseAsync(
            [new ChatMessage(ChatRole.System, SystemPrompt), new ChatMessage(ChatRole.User, BuildUser(batch, lines, claims))],
            options, ct).ConfigureAwait(false);

        var parsed = Parse(response.Text);
        var result = new Dictionary<int, string>();
        foreach (var item in parsed)
        {
            if (TryParseRef(item.Ref, out var lineNo) && !string.IsNullOrWhiteSpace(item.RepairedText))
                result[lineNo] = NormalizeLine(item.RepairedText);
        }
        return result;
    }

    private static string BuildUser(IReadOnlyList<ContractViolation> batch, string[] lines,
        IReadOnlyDictionary<string, SemanticLedgerEntry> claims)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ZEILEN ({batch.Count}) — gib GENAU {batch.Count} reparierte Zeilen zurück, eine pro ref:");
        sb.AppendLine();
        foreach (var v in batch)
        {
            var lineNo = v.LineNumber!.Value;
            var text = lineNo - 1 < lines.Length ? lines[lineNo - 1].Trim() : v.ArtifactQuote ?? "";
            sb.AppendLine($"ref: L{lineNo}");
            sb.AppendLine($"  aktuell: {text}");
            sb.AppendLine($"  verstoss: {v.Code} — {v.Message}");
            if (!string.IsNullOrWhiteSpace(v.SuggestedAction)) sb.AppendLine($"  hinweis: {v.SuggestedAction}");
            sb.AppendLine("  zitierte claim-pakete:");
            foreach (var id in v.ClaimIds)
            {
                if (!claims.TryGetValue(id, out var p)) continue;
                sb.AppendLine($"    - id: {id}  [status={p.Status}, modality={p.Modality}, timeScope={p.TimeScope ?? "?"}]");
                sb.AppendLine($"      proposition: {p.Proposition}");
                var ev = (p.Evidence ?? []).Select(e => e.Quote).Where(q => !string.IsNullOrWhiteSpace(q)).ToList();
                if (ev.Count > 0) sb.AppendLine($"      evidence: {string.Join(" | ", ev)}");
                if (!string.IsNullOrWhiteSpace(p.Notes)) sb.AppendLine($"      notes: {p.Notes}");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    // Reparatur ist nur gültig, wenn die Zeile weiterhin ein Listenpunkt ist UND alle Original-Citation-ids erhalten bleiben.
    private static bool IsValidRepair(string original, string repaired)
    {
        if (string.IsNullOrWhiteSpace(repaired)) return false;
        if (!Regex.IsMatch(repaired, @"^\s*[-*]\s+")) return false;
        var origIds = ExtractIds(original);
        var newIds = ExtractIds(repaired).ToHashSet(StringComparer.Ordinal);
        return origIds.All(newIds.Contains); // keine Quelle darf verloren gehen
    }

    private static string NormalizeLine(string s)
    {
        var one = s.Replace("\r", " ").Replace("\n", " ").Trim();
        if (one.Length == 0) return one;
        if (!Regex.IsMatch(one, @"^\s*[-*]\s+")) one = "- " + one; // Bullet wiederherstellen, falls das Modell ihn droppte
        return one;
    }

    private static IReadOnlyList<string> ExtractIds(string line)
    {
        var ids = new List<string>();
        foreach (Match m in IdRx.Matches(line))
            foreach (var tok in m.Groups[1].Value.Split(','))
            {
                var id = tok.Trim();
                if (id.Length > 0 && !Regex.IsMatch(id, @"^\d+$")) ids.Add(id);
            }
        return ids;
    }

    private static bool TryParseRef(string? r, out int lineNo)
    {
        lineNo = 0;
        if (string.IsNullOrWhiteSpace(r)) return false;
        var s = r.Trim().TrimStart('L', 'l');
        return int.TryParse(s, out lineNo);
    }

    private static IReadOnlyList<RawRepair> Parse(string? text)
    {
        var json = ExtractJson(text);
        if (json is null) return [];
        try { return JsonSerializer.Deserialize<RawResult>(json, Json)?.Items?.Where(v => !string.IsNullOrWhiteSpace(v.Ref)).ToList() ?? []; }
        catch (JsonException) { return []; }
    }

    private static string? ExtractJson(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{');
        var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record RawResult([property: JsonPropertyName("items")] IReadOnlyList<RawRepair> Items);
    private sealed record RawRepair(
        [property: JsonPropertyName("ref")] string Ref,
        [property: JsonPropertyName("repairedText")] string RepairedText);
}

public sealed record RepairResult(
    [property: JsonPropertyName("lineNumber")] int LineNumber,
    [property: JsonPropertyName("originalText")] string OriginalText,
    [property: JsonPropertyName("repairedText")] string RepairedText,
    [property: JsonPropertyName("violationCode")] string ViolationCode,
    [property: JsonPropertyName("changed")] bool Changed);
