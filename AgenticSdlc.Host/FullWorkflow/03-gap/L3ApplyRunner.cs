using System.Text.Json;
using AgenticSdlc.Host.Configuration;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

/// <summary>
/// L3 Apply-Phase (Workflow 2, §5.3 — CLI: <c>l3-apply &lt;l3-run|runId&gt; [--decisions &lt;human-decisions.json&gt;]</c>).
/// Liest die menschlichen Entscheidungen (accept/edit/reject) zum Human-Review-Paket eines L3-Prepare-Laufs und wendet
/// sie DETERMINISTISCH an: promoviert akzeptierte Kandidaten zu Projektartefakten mit stabiler ID + Provenienz (§7),
/// archiviert abgelehnte auditierbar. Schreibt in denselben Lauf-Ordner. Kein LLM → korrekt ein deterministischer Runner
/// (der NEEDS_REVISION→Reflect-Zweig, der einen Graphen bräuchte, ist noch nicht Teil von v1).
/// Exit: 0 = ok, 2 = Usage/IO.
/// </summary>
public static class L3ApplyRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly JsonSerializerOptions Read = new(JsonSerializerDefaults.Web);

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l3-apply <l3-run-dir|runId> [--decisions <human-decisions.json>]");
            return 2;
        }
        var runDir = ResolveRunDir(repoRoot, args[1]);
        if (runDir is null) { Console.Error.WriteLine($"[l3-apply] L3-Lauf '{args[1]}' nicht gefunden (Pfad oder runId unter runs/l3)."); return 2; }

        string? decisionsArg = null;
        for (var i = 2; i < args.Length - 1; i++)
            if (string.Equals(args[i], "--decisions", StringComparison.Ordinal)) decisionsArg = args[i + 1];
        var decisionsPath = decisionsArg is not null ? Resolve(repoRoot, decisionsArg) : Path.Combine(runDir, "human-decisions.json");
        if (decisionsPath is null || !File.Exists(decisionsPath)) { Console.Error.WriteLine($"[l3-apply] Entscheidungsdatei fehlt: '{decisionsPath}'. (Mensch entscheidet im Human-Review-Paket → human-decisions.json)"); return 2; }

        var routingPath = Path.Combine(runDir, "routing-report.json");
        if (!File.Exists(routingPath)) { Console.Error.WriteLine($"[l3-apply] routing-report.json fehlt in '{Path.GetRelativePath(repoRoot, runDir)}' — erst `l3` (Prepare) laufen lassen."); return 2; }

        var report = JsonSerializer.Deserialize<RoutingReport>(await File.ReadAllTextAsync(routingPath).ConfigureAwait(false), Read);
        var byId = (report?.Items ?? []).ToDictionary(r => r.Candidate.CandidateId, r => r, StringComparer.Ordinal);
        var decisions = JsonSerializer.Deserialize<HumanDecisionsFile>(await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), Read);
        if (decisions is null || decisions.Decisions.Count == 0) { Console.Error.WriteLine("[l3-apply] keine Entscheidungen in der Datei."); return 2; }
        var reviewer = string.IsNullOrWhiteSpace(decisions.Reviewer) ? "human" : decisions.Reviewer!;

        var records = new List<L3DecisionRecord>();
        var promoted = new List<L3PromotedItem>();
        var rejected = new List<L3RejectedItem>();
        var mappings = new List<L3PromotionMapping>();
        int hdec = 0;
        var typeSeq = new Dictionary<string, int>(StringComparer.Ordinal);
        var now = DateTime.UtcNow;

        foreach (var dec in decisions.Decisions)
        {
            if (!byId.TryGetValue(dec.CandidateId, out var cand))
            {
                Console.Error.WriteLine($"[l3-apply] HINWEIS: Entscheidung für unbekannten Kandidaten '{dec.CandidateId}' — übersprungen.");
                continue;
            }
            var decisionId = $"HDEC-{++hdec:D3}";
            var op = dec.Decision.Trim().ToLowerInvariant();

            if (op is "accept" or "edit")
            {
                var type = cand.Candidate.TargetType;
                typeSeq.TryGetValue(type, out var seq); typeSeq[type] = ++seq;
                var itemId = $"L3-{TypePrefix(type)}-{seq:D3}";
                var anchors = (dec.FinalAnchorIds is { Count: > 0 } fa) ? fa : cand.KeptAnchorIds;
                var text = op == "edit" && !string.IsNullOrWhiteSpace(dec.EditedText) ? dec.EditedText!.Trim() : cand.Candidate.Text;
                var origin = anchors.Count > 0 ? "HUMAN_ACCEPTED_ANCHORED" : "HUMAN_ACCEPTED_OPEN_WORLD";

                promoted.Add(new L3PromotedItem(itemId, type, text, origin, "ACCEPTED", decisionId, dec.CandidateId, anchors, Version: 1));
                mappings.Add(new L3PromotionMapping(dec.CandidateId, itemId));
                records.Add(new L3DecisionRecord(decisionId, dec.CandidateId, op, op == "edit" ? dec.EditedText : null, anchors, dec.Reason, reviewer, now, itemId));
            }
            else if (op == "reject")
            {
                rejected.Add(new L3RejectedItem(dec.CandidateId, cand.Candidate.Text, cand.Class, dec.Reason));
                records.Add(new L3DecisionRecord(decisionId, dec.CandidateId, op, null, [], dec.Reason, reviewer, now, null));
            }
            else
            {
                Console.Error.WriteLine($"[l3-apply] HINWEIS: unbekannte Operation '{dec.Decision}' für {dec.CandidateId} (erlaubt: accept|edit|reject) — übersprungen.");
            }
        }

        await File.WriteAllTextAsync(Path.Combine(runDir, "decision-records.json"), JsonSerializer.Serialize(records, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(runDir, "promoted-items.json"), JsonSerializer.Serialize(promoted, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(runDir, "rejected-candidates.json"), JsonSerializer.Serialize(rejected, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(runDir, "promotion-mappings.json"), JsonSerializer.Serialize(mappings, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(runDir, "apply-report.json"), JsonSerializer.Serialize(new
        {
            runId = Path.GetFileName(runDir), reviewer, decisions = records.Count, promoted = promoted.Count, rejected = rejected.Count, appliedUtc = now
        }, Json)).ConfigureAwait(false);
        await File.AppendAllTextAsync(Path.Combine(runDir, "logs", "events.jsonl"),
            JsonSerializer.Serialize(new { type = "L3_APPLIED", promoted = promoted.Count, rejected = rejected.Count, timestampUtc = now }) + "\n").ConfigureAwait(false);

        Console.WriteLine($"[l3-apply] runId={Path.GetFileName(runDir)}  entscheidungen={records.Count}  promoviert={promoted.Count} abgelehnt={rejected.Count}");
        foreach (var m in mappings) Console.WriteLine($"  {m.CandidateId} → {m.PromotedItemId}");
        Console.WriteLine($"[l3-apply] -> promoted-items.json + decision-records.json + promotion-mappings.json  run -> {Path.GetRelativePath(repoRoot, runDir)}");
        return 0;
    }

    private static string TypePrefix(string type) => type.Trim().ToLowerInvariant() switch
    {
        "requirement" or "requirements" => "REQ",
        "risk" or "risks" => "RISK",
        _ => new string(type.Where(char.IsLetter).Take(4).ToArray()).ToUpperInvariant() is { Length: > 0 } p ? p : "ITEM"
    };

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));

    // Pfad ODER runId-Token unter runs/l3.
    private static string? ResolveRunDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full)) return full;
        var l3Root = Path.Combine(repoRoot, "runs", "l3");
        if (!Directory.Exists(l3Root)) return null;
        return Directory.EnumerateDirectories(l3Root).FirstOrDefault(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record RoutingReport(
        [property: System.Text.Json.Serialization.JsonPropertyName("items")] IReadOnlyList<L3RoutedCandidate> Items);
}
