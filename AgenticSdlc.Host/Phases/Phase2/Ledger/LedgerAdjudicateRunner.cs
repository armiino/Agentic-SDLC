using System.Text;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Zweistufige, deterministische Adjudikations-CLI (Bauvorschlag §1.5, R1-Muster). KEIN LLM.
///   ledger-adjudicate       &lt;validated.json&gt; [miss-signal.json] [outPrefix]  -> queue.json + queue.md
///   ledger-adjudicate-apply &lt;filled-queue.json&gt; &lt;validated.json&gt; [outPrefix] -> adjudicated + consumable + gate
/// </summary>
public static class LedgerAdjudicateRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static Task<int> RunPrepareAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: ledger-adjudicate <validated-ledger.json> [miss-signal.json] [outPrefix]"); return Task.FromResult(2); }
        var validatedPath = Resolve(repoRoot, args[1]);
        if (!File.Exists(validatedPath)) { Console.Error.WriteLine($"[adjudicate] validated fehlt: {validatedPath}"); return Task.FromResult(2); }
        var missPath = args.Length >= 3 ? Resolve(repoRoot, args[2]) : null;
        var missJson = missPath is not null && File.Exists(missPath) ? File.ReadAllText(missPath) : null;
        if (missPath is not null && missJson is null) Console.Error.WriteLine($"[adjudicate] WARN miss-signal nicht gefunden: {missPath}");

        var validated = LedgerAdjudicationAdapter.LoadValidated(validatedPath);
        var validatedRunId = RunIdFromPath(validatedPath);
        var unitRunId = missPath is not null ? RunIdFromPath(missPath) : null;
        var queue = LedgerAdjudicationAdapter.Build(validated, validatedRunId, missJson, unitRunId);

        var outPrefix = args.Length >= 4 ? Resolve(repoRoot, args[3])
            : Path.Combine(repoRoot, "runs", "adjudication", $"queue-{DateTime.UtcNow:yyyyMMdd_HHmmss}");
        Directory.CreateDirectory(Path.GetDirectoryName(outPrefix)!);
        File.WriteAllText(outPrefix + ".queue.json", JsonSerializer.Serialize(queue, Json));
        File.WriteAllText(outPrefix + ".queue.md", RenderMarkdown(queue));

        Console.WriteLine($"[adjudicate] items={queue.Items.Count} (review_required + coverage/unit-misses)");
        Console.WriteLine($"[adjudicate] queue -> {Path.GetRelativePath(repoRoot, outPrefix)}.queue.json  (Aktionen darin ausfüllen: action/actionReason[/referenceTarget])");
        Console.WriteLine($"[adjudicate] read  -> {Path.GetRelativePath(repoRoot, outPrefix)}.queue.md");
        Console.WriteLine($"[adjudicate] danach: ledger-adjudicate-apply <queue.json> {Path.GetRelativePath(repoRoot, validatedPath)}");
        return Task.FromResult(0);
    }

    public static async Task<int> RunApplyAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3) { Console.Error.WriteLine("Usage: ledger-adjudicate-apply <filled-queue.json> <validated-ledger.json> [outPrefix]"); return 2; }
        var queuePath = Resolve(repoRoot, args[1]);
        var validatedPath = Resolve(repoRoot, args[2]);
        if (!File.Exists(queuePath) || !File.Exists(validatedPath)) { Console.Error.WriteLine("[adjudicate-apply] Eingabedatei fehlt."); return 2; }

        var filled = JsonSerializer.Deserialize<AdjudicationQueue>(await File.ReadAllTextAsync(queuePath), Json)
                     ?? new AdjudicationQueue(null, null, "", []);
        var validated = LedgerAdjudicationAdapter.LoadValidated(validatedPath);

        var gate = AdjudicationCompletenessGate.Evaluate(filled.Items);
        var (audit, consumable) = AdjudicationCompletenessGate.Project(filled, validated);

        var outPrefix = args.Length >= 4 ? Resolve(repoRoot, args[3])
            : Path.Combine(Path.GetDirectoryName(queuePath)!, Path.GetFileNameWithoutExtension(queuePath).Replace(".queue", ""));
        Directory.CreateDirectory(Path.GetDirectoryName(outPrefix)!);
        await File.WriteAllTextAsync(outPrefix + ".adjudicated-ledger.json", JsonSerializer.Serialize(audit, Json));
        await File.WriteAllTextAsync(outPrefix + ".consumable.json", JsonSerializer.Serialize(consumable, Json));
        await File.WriteAllTextAsync(outPrefix + ".gate.json", JsonSerializer.Serialize(gate, Json));

        Console.WriteLine($"[adjudicate-apply] gate pass={gate.Pass} errors={gate.ErrorCount} pending={gate.PendingCount}");
        foreach (var vi in gate.Violations) Console.WriteLine($"[adjudicate-apply]   [{vi.Severity}] {vi.Code} ({vi.Ids.Count})");
        Console.WriteLine($"[adjudicate-apply] consumable claims={consumable.Claims.Count} (pending={consumable.PendingCount})");
        Console.WriteLine($"[adjudicate-apply] -> {Path.GetRelativePath(repoRoot, outPrefix)}.{{adjudicated-ledger,consumable,gate}}.json");
        if (!gate.Pass) { Console.Error.WriteLine("[adjudicate-apply] GATE FAILED: nicht jedes Item hat eine (gültige) Aktion."); return 4; }
        return 0;
    }

    private static string RenderMarkdown(AdjudicationQueue q)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Adjudikations-Queue (lesbar) — fülle die Aktionen in der .queue.json aus");
        sb.AppendLine();
        sb.AppendLine("Aktionen: `accept_gap | merge_existing | mark_covered_by | reject | apply_repair | defer`");
        sb.AppendLine("(merge_existing/mark_covered_by brauchen `referenceTarget`; apply_repair nutzt die systemSuggestion.)");
        sb.AppendLine();
        foreach (var i in q.Items)
        {
            sb.AppendLine($"## {i.ItemId}  [{i.ItemType} / {i.SourceMode}]");
            sb.AppendLine($"- proposition: {i.Proposition}");
            if (i.SystemSuggestion is { } s)
                sb.AppendLine($"- systemSuggestion: {s.Kind} {(s.Facet is null ? "" : $"{s.Facet}: {s.Observed}->{s.Suggested}")}{(s.Classification is null ? "" : s.Classification)}");
            if (i.EvidenceRefs.Count > 0) sb.AppendLine($"- evidence: {string.Join(" | ", i.EvidenceRefs.Take(3))}");
            sb.AppendLine($"- reason: {i.Reason}");
            sb.AppendLine($"- ACTION: ____   REASON: ____");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string? RunIdFromPath(string p)
    {
        foreach (var seg in p.Replace('\\', '/').Split('/'))
            if (seg.Length >= 15 && seg.Count(c => c == '_') >= 2) return seg;
        return null;
    }

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
