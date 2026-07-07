using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Ledger;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker;

/// <summary>
/// MC2-CLI: <c>contract-repair &lt;artifact.md&gt; &lt;consumable.json&gt; [model] [out.md]
///   [--repeat k] [--min-votes n] [--max N] [--artifact X]</c>.
/// Der bounded Loop: k-Vote-Critic prüft → <see cref="ContractRepair"/> patcht die bestätigten Zeilen-Verstöße →
/// erneut prüfen, bis clean ODER N erreicht. Schreibt das reparierte Artefakt + <c>repair-log.json</c>.
/// Exit: 0 = konvergiert (clean), 1 = Restverstöße nach N, 2 = Usage/IO, 4 = LLM-Fehler.
/// </summary>
/// <remarks>
/// Der Loop liegt HIER im Runner (Demonstration/Messung im aktuellen Fall). Die produktive Form ist der MAF-
/// Subworkflow (Repair→Checker als conditional edge) — das ist der spätere Schritt MC1/C3 (Plan.md §6, §6.2).
/// </remarks>
public static class ContractRepairRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: contract-repair <artifact.md> <consumable.json> [model] [out.md] [--repeat k] [--min-votes n] [--max N] [--artifact X]");
            return 2;
        }

        var artPath = Resolve(repoRoot, args[1]);
        var consPath = Resolve(repoRoot, args[2]);
        if (!File.Exists(artPath)) { Console.Error.WriteLine($"[contract-repair] artifact fehlt: {artPath}"); return 2; }
        if (!File.Exists(consPath)) { Console.Error.WriteLine($"[contract-repair] consumable fehlt: {consPath}"); return 2; }

        string? modelArg = null, outArg = null;
        int repeat = 3, minVotes = 0, maxIter = 3;
        var artifactDisposition = "requirements";
        for (var i = 3; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--repeat", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) repeat = ParseInt(args[++i], repeat);
            else if (string.Equals(args[i], "--min-votes", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) minVotes = ParseInt(args[++i], minVotes);
            else if (string.Equals(args[i], "--max", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) maxIter = ParseInt(args[++i], maxIter);
            else if (string.Equals(args[i], "--artifact", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) artifactDisposition = args[++i].Trim().ToLowerInvariant();
            else if (args[i].StartsWith("--", StringComparison.Ordinal)) { /* ignore */ }
            else if (modelArg is null) modelArg = args[i];
            else if (outArg is null) outArg = args[i];
        }
        maxIter = Math.Max(1, maxIter);

        var markdown = await File.ReadAllTextAsync(artPath).ConfigureAwait(false);
        var ledger = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consPath).ConfigureAwait(false), Json);
        if (ledger is null || ledger.Claims.Count == 0) { Console.Error.WriteLine("[contract-repair] consumable leer/nicht lesbar."); return 2; }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        var critic = new ContractCritic(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);
        var repair = new ContractRepair(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);

        var mc0Before = ContractChecker.Check(markdown, ledger, artifactDisposition: artifactDisposition);
        Console.WriteLine($"[contract-repair] artifact={Path.GetRelativePath(repoRoot, artPath)} model={judgeSettings.ModelId} k={repeat} max={maxIter} artifactDisp={artifactDisposition}");
        Console.WriteLine($"[contract-repair] MC0(before): pass={mc0Before.Pass} decision={mc0Before.Decision}");

        var iterationLog = new List<object>();
        var converged = false;
        var lastC7 = 0;

        try
        {
            for (var it = 1; it <= maxIter; it++)
            {
                var vote = await critic.CheckWithVoteAsync(markdown, ledger, repeat, minVotes, CancellationToken.None).ConfigureAwait(false);
                lastC7 = vote.Violations.Count;
                Console.WriteLine($"[contract-repair] iter {it}: C7-violations(k-Vote)={lastC7}");

                if (vote.Violations.Count == 0) { converged = true; iterationLog.Add(new { iter = it, c7Violations = 0, repaired = 0 }); break; }
                if (it == maxIter) { iterationLog.Add(new { iter = it, c7Violations = lastC7, repaired = 0, note = "max erreicht, Restverstöße bleiben" }); break; }

                var (patched, repairs) = await repair.RepairAsync(markdown, ledger, vote.Violations, CancellationToken.None).ConfigureAwait(false);
                markdown = patched;
                var changed = repairs.Count(r => r.Changed);
                Console.WriteLine($"[contract-repair] iter {it}: repariert {changed}/{repairs.Count} Zeile(n)");
                foreach (var r in repairs.Where(r => r.Changed))
                    Console.WriteLine($"    L{r.LineNumber} [{r.ViolationCode}]\n      -  {r.OriginalText}\n      +  {r.RepairedText}");
                iterationLog.Add(new { iter = it, c7Violations = lastC7, repaired = changed });
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[contract-repair] fehlgeschlagen: {ex.Message}");
            return 4;
        }

        var outPath = outArg is not null ? Resolve(repoRoot, outArg)
            : Path.Combine(Path.GetDirectoryName(artPath) ?? ".", Path.GetFileNameWithoutExtension(artPath) + ".repaired.md");
        await File.WriteAllTextAsync(outPath, markdown).ConfigureAwait(false);

        var mc0After = ContractChecker.Check(markdown, ledger, artifactDisposition: artifactDisposition);
        var logPath = Path.Combine(Path.GetDirectoryName(outPath) ?? ".", "repair-log.json");
        await File.WriteAllTextAsync(logPath, JsonSerializer.Serialize(new
        {
            converged, iterations = iterationLog, finalC7Violations = lastC7,
            mc0Before = new { mc0Before.Pass, decision = mc0Before.Decision.ToString(), violations = mc0Before.Violations.Count },
            mc0After = new { mc0After.Pass, decision = mc0After.Decision.ToString(), violations = mc0After.Violations.Count }
        }, Json)).ConfigureAwait(false);

        Console.WriteLine($"[contract-repair] converged={converged}  finalC7={lastC7}  MC0(after): pass={mc0After.Pass} ({mc0After.Violations.Count} det. Verstöße)");
        Console.WriteLine($"[contract-repair] repaired -> {Path.GetRelativePath(repoRoot, outPath)}  log -> {Path.GetRelativePath(repoRoot, logPath)}");
        return converged ? 0 : 1;
    }

    private static int ParseInt(string s, int fallback) => int.TryParse(s, out var v) ? v : fallback;
    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
