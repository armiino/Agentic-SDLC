using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.Ledger;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker;

/// <summary>
/// MC0-CLI: <c>contract-check &lt;requirements.md&gt; &lt;consumable.json&gt; [out.json] [--iteration N] [--max N]</c>.
/// Rein deterministisch (kein LLM, kein Workflow). Schreibt <c>contract-report.json</c> neben das Artefakt
/// (oder an [out]) und gibt eine Konsolen-Zusammenfassung aus. Exit: 0 = Pass, 1 = Violations (Error), 2 = Usage/IO.
/// </summary>
public static class ContractCheckRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        _ = settings;
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: contract-check <artifact.md> <consumable.json> [out.json] [--artifact requirements|risks|architecture|open-questions] [--iteration N] [--max N]");
            return 2;
        }

        var reqPath = Resolve(repoRoot, args[1]);
        var consPath = Resolve(repoRoot, args[2]);
        if (!File.Exists(reqPath)) { Console.Error.WriteLine($"[contract-check] requirements fehlt: {reqPath}"); return 2; }
        if (!File.Exists(consPath)) { Console.Error.WriteLine($"[contract-check] consumable fehlt: {consPath}"); return 2; }

        // optionale Positional-out (erstes Nicht-Flag-Arg nach den Pflichtargumenten) + Flags.
        string? outArg = null;
        var iteration = 1;
        var maxIterations = 3;
        var artifactDisposition = "requirements";
        for (var i = 3; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--iteration", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                { iteration = ParseInt(args[++i], iteration); }
            else if (string.Equals(args[i], "--max", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                { maxIterations = ParseInt(args[++i], maxIterations); }
            else if (string.Equals(args[i], "--artifact", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                { artifactDisposition = args[++i].Trim().ToLowerInvariant(); }
            else if (!args[i].StartsWith("--", StringComparison.Ordinal) && outArg is null)
                { outArg = args[i]; }
        }

        var markdown = await File.ReadAllTextAsync(reqPath).ConfigureAwait(false);
        var ledger = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consPath).ConfigureAwait(false), Json);
        if (ledger is null || ledger.Claims.Count == 0)
        {
            Console.Error.WriteLine("[contract-check] consumable leer/nicht lesbar.");
            return 2;
        }

        var report = ContractChecker.Check(markdown, ledger, iteration, maxIterations, artifactDisposition);

        var outPath = outArg is not null
            ? Resolve(repoRoot, outArg)
            : Path.Combine(Path.GetDirectoryName(reqPath) ?? ".", "contract-report.json");
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);

        PrintSummary(report, ledger.Claims.Count, Path.GetRelativePath(repoRoot, reqPath), Path.GetRelativePath(repoRoot, outPath));
        return report.Pass ? 0 : 1;
    }

    private static void PrintSummary(ContractCheckReport report, int claimCount, string reqRel, string outRel)
    {
        var byCode = report.Violations
            .GroupBy(v => v.Code)
            .ToDictionary(g => g.Key, g => g.Count());

        Console.WriteLine($"[contract-check] artifact={reqRel}  claims={claimCount}");
        Console.WriteLine($"[contract-check] PASS={report.Pass}  decision={report.Decision}  iteration={report.Iteration}");
        Console.WriteLine($"[contract-check] violations={report.Violations.Count} " +
                          $"(errors={report.Violations.Count(v => v.Severity == ContractSeverity.Error)}, " +
                          $"warnings={report.Violations.Count(v => v.Severity == ContractSeverity.Warning)})");
        foreach (var code in new[] { ContractCodes.MissingCitation, ContractCodes.UnknownClaimId,
                     ContractCodes.WrongDisposition, ContractCodes.RequiredClaimUnused, ContractCodes.FacetOverstated })
        {
            if (byCode.TryGetValue(code, out var n) && n > 0)
                Console.WriteLine($"    - {code}: {n}");
        }
        if (report.RepairItems.Count > 0)
            Console.WriteLine($"[contract-check] repairItems={report.RepairItems.Count}");
        Console.WriteLine($"[contract-check] report -> {outRel}");
    }

    private static int ParseInt(string s, int fallback) => int.TryParse(s, out var v) ? v : fallback;

    private static string Resolve(string repoRoot, string p)
        => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
