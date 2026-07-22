using System.Text;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

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

        // Adjudikation ist eine abgeleitete Stufe DES Runs -> in den Run-Ordner (nachvollziehbar neben step-03).
        var outDir = args.Length >= 4 ? Resolve(repoRoot, args[3]) : DefaultAdjudicationDir(repoRoot, validatedPath);
        Directory.CreateDirectory(outDir);
        File.WriteAllText(Path.Combine(outDir, "queue.json"), JsonSerializer.Serialize(queue, Json));
        File.WriteAllText(Path.Combine(outDir, "queue.md"), RenderMarkdown(queue));

        Console.WriteLine($"[adjudicate] items={queue.Items.Count} (review_required + coverage/unit-misses)");
        Console.WriteLine($"[adjudicate] queue -> {Path.GetRelativePath(repoRoot, Path.Combine(outDir, "queue.json"))}  (Aktionen ausfüllen: action/actionReason[/referenceTarget])");
        Console.WriteLine($"[adjudicate] read  -> {Path.GetRelativePath(repoRoot, Path.Combine(outDir, "queue.md"))}");
        Console.WriteLine($"[adjudicate] danach: ledger-adjudicate-apply {Path.GetRelativePath(repoRoot, Path.Combine(outDir, "queue.json"))} {Path.GetRelativePath(repoRoot, validatedPath)}");
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
        filled = LedgerAdjudicationAdapter.EnrichRepairSuggestions(filled, validated);

        // step-00-Units (id -> Unit) für echte Evidenz-Erdung von accept_gap/promote/attach; fehlt sie -> Fallback.
        var units = LoadStep00Units(validatedPath);

        var gate = AdjudicationCompletenessGate.Evaluate(filled.Items);
        var (audit, consumable) = AdjudicationCompletenessGate.Project(filled, validated, units);

        // Output neben die queue.json (die liegt bereits im step-03b-adjudicated/-Ordner des Runs).
        var outDir = args.Length >= 4 ? Resolve(repoRoot, args[3]) : Path.GetDirectoryName(queuePath)!;
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "adjudicated-ledger.json"), JsonSerializer.Serialize(audit, Json));
        await File.WriteAllTextAsync(Path.Combine(outDir, "consumable.json"), JsonSerializer.Serialize(consumable, Json));
        await File.WriteAllTextAsync(Path.Combine(outDir, "gate.json"), JsonSerializer.Serialize(gate, Json));

        Console.WriteLine($"[adjudicate-apply] gate pass={gate.Pass} errors={gate.ErrorCount} pending={gate.PendingCount}");
        foreach (var vi in gate.Violations) Console.WriteLine($"[adjudicate-apply]   [{vi.Severity}] {vi.Code} ({vi.Ids.Count})");

        // attach_evidence, dessen Ziel nicht in der Projektion aufgelöst werden konnte (z.B. referenceTarget fehlt
        // oder zeigt auf einen Candidate/nicht-materialisierten Claim) -> NICHT still: laut melden.
        var unresolvedAttach = audit.Records
            .Where(r => string.Equals(r.Action, AdjudicationActions.AttachEvidence, StringComparison.OrdinalIgnoreCase) && r.ResultingClaim is null)
            .Select(r => r.ItemId).ToList();
        if (unresolvedAttach.Count > 0)
            Console.Error.WriteLine($"[adjudicate-apply] WARN attach_evidence ohne auflösbares Ziel ({unresolvedAttach.Count}): {string.Join(", ", unresolvedAttach)} — referenceTarget auf einen validierten Claim setzen.");
        Console.WriteLine($"[adjudicate-apply] consumable claims={consumable.Claims.Count} (pending={consumable.PendingCount})");
        Console.WriteLine($"[adjudicate-apply] -> {Path.GetRelativePath(repoRoot, outDir)}/{{adjudicated-ledger,consumable,gate}}.json");
        if (!gate.Pass) { Console.Error.WriteLine("[adjudicate-apply] GATE FAILED: nicht jedes Item hat eine (gültige) Aktion."); return 4; }
        return 0;
    }

    /// <summary>Lädt step-00 Atomic-Units (id -> Unit) aus dem Run-Ordner des validated Ledgers.
    /// Fehlt die Datei (alte/Non-Unit-Runs) -> null (Projektion nutzt Fallback-Evidenz).</summary>
    private static IReadOnlyDictionary<string, AtomicUnit>? LoadStep00Units(string validatedPath)
    {
        var dir = Path.GetDirectoryName(validatedPath);              // .../step-03-facet-validation
        var runDir = dir is null ? null : Path.GetDirectoryName(dir); // .../<runId>
        if (runDir is null) return null;
        var unitsPath = Path.Combine(runDir, "step-00-atomic-units", "output.json");
        if (!File.Exists(unitsPath)) return null;
        try
        {
            var fixture = JsonSerializer.Deserialize<AtomicUnitFixture>(File.ReadAllText(unitsPath), Json);
            if (fixture?.Units is not { Count: > 0 }) return null;
            return fixture.Units
                .GroupBy(u => u.Id, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        }
        catch { return null; }
    }

    private static string RenderMarkdown(AdjudicationQueue q)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Adjudikations-Queue (lesbar) — fülle die Aktionen in der .queue.json aus");
        sb.AppendLine();
        sb.AppendLine("Aktionen: `accept_gap | promote_to_claim | attach_evidence | merge_existing | mark_covered_by | reject | apply_repair | defer`");
        sb.AppendLine("(promote_to_claim = unit als eigener Claim; attach_evidence/merge_existing/mark_covered_by brauchen `referenceTarget` (attach: Fallback claimId); apply_repair nutzt alle systemSuggestions.)");
        sb.AppendLine();
        foreach (var i in q.Items)
        {
            sb.AppendLine($"## {i.ItemId}  [{i.ItemType} / {i.SourceMode}]");
            sb.AppendLine($"- proposition: {i.Proposition}");
            var suggestions = EffectiveSuggestions(i).ToList();
            if (suggestions.Count == 1)
                sb.AppendLine($"- systemSuggestion: {FormatSuggestion(suggestions[0])}");
            else if (suggestions.Count > 1)
                sb.AppendLine($"- systemSuggestions: {string.Join(" | ", suggestions.Select(FormatSuggestion))}");
            if (i.EvidenceRefs.Count > 0) sb.AppendLine($"- evidence: {string.Join(" | ", i.EvidenceRefs.Take(3))}");
            sb.AppendLine($"- reason: {i.Reason}");
            sb.AppendLine($"- ACTION: ____   REASON: ____");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static IEnumerable<AdjudicationSuggestion> EffectiveSuggestions(AdjudicationItem item)
    {
        if (item.SystemSuggestions is { Count: > 0 }) return item.SystemSuggestions;
        return item.SystemSuggestion is null ? [] : [item.SystemSuggestion];
    }

    private static string FormatSuggestion(AdjudicationSuggestion s)
        => $"{s.Kind} {(s.Facet is null ? "" : $"{s.Facet}: {s.Observed}->{s.Suggested}")}{(s.Classification is null ? "" : s.Classification)}";

    /// <summary>Default-Ablage: im Run-Ordner des validated Ledgers (nachvollziehbar neben step-03), sonst timestamped.</summary>
    private static string DefaultAdjudicationDir(string repoRoot, string validatedPath)
    {
        // validatedPath = runs/ledger/<runId>/step-03-facet-validation/output.json -> Run-Ordner ableiten.
        var dir = Path.GetDirectoryName(validatedPath);              // .../step-03-facet-validation
        var runDir = dir is null ? null : Path.GetDirectoryName(dir); // .../<runId>
        if (runDir is not null && Directory.Exists(runDir))
            return Path.Combine(runDir, "step-03b-adjudicated");
        return Path.Combine(repoRoot, "runs", "adjudication", $"adj-{DateTime.UtcNow:yyyyMMdd_HHmmss}");
    }

    private static string? RunIdFromPath(string p)
    {
        foreach (var seg in p.Replace('\\', '/').Split('/'))
            if (seg.Length >= 15 && seg.Count(c => c == '_') >= 2) return seg;
        return null;
    }

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
