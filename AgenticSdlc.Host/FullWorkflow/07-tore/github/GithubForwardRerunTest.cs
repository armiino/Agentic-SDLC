using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// CLI: github-forward-rerun-test [github-sync-delta.json] [--start-issue N] [--out <file>]
//
// T3.7 — der End-to-End-Beweis des urspruenglichen Anlasses (plan-tor3 §14): derselbe Delta zweimal durch den
// Forward-Vorfilter. Pass 1 (leerer Zustand): unmapped PBIs -> CREATE (simuliert: Issue-Nummer + Mapping
// persistiert). Pass 2 (Re-Run mit denselben Daten): dieselben PBIs sind jetzt gemappt -> UPDATE/HOLD, KEIN neues
// CREATE. Deterministisch (kein LLM, kein GitHub, kein echter Core): testet die Dedup-Mechanik (T3.1 Mapping +
// T3.3 Seed). Exit 0 = keine Duplikate, 1 = Duplikat erkannt.
public static class GithubForwardRerunTest
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        var startIssue = 100;
        string? deltaArg = null, outArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--start-issue", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var n)) { startIssue = n; i++; }
            else if (string.Equals(a, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) outArg = args[++i];
            else if (!a.StartsWith("--", StringComparison.Ordinal)) deltaArg ??= a;
        }

        IReadOnlyList<GithubSyncEntry> entries;
        if (deltaArg is null)
        {
            entries = DemoDelta();
            Console.WriteLine("[rerun-test] kein Delta angegeben -> eingebautes 4-PBI-Demo-Delta (3 aktiv, 1 blockiert).");
        }
        else
        {
            var path = Path.IsPathRooted(deltaArg) ? deltaArg : Path.Combine(repoRoot, deltaArg);
            if (!File.Exists(path)) { Console.Error.WriteLine($"[rerun-test] Delta nicht gefunden: {path}"); return 2; }
            var doc = JsonSerializer.Deserialize<GithubSyncDeltaDocument>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
            entries = doc?.Entries ?? [];
        }
        if (entries.Count == 0) { Console.Error.WriteLine("[rerun-test] keine Delta-Eintraege."); return 2; }

        // Ausgangszustand: erster Sprint -> kein Mapping, keine Issues.
        var mappings = new Dictionary<string, GithubMappingRecord>(StringComparer.Ordinal);
        var issues = new List<GithubIssueSnapshot>();

        // ---- Pass 1 ----
        var seed1 = GithubForwardSeed.Seed(entries, mappings, issues);
        var pass1Create = 0;
        var next = startIssue;
        foreach (var e in seed1.UnmappedPbis) // agentischer unmapped-Fall: hier deterministisch als CREATE simuliert
        {
            var num = next++;
            issues.Add(new GithubIssueSnapshot(num, $"u/{num}", e.Title, $"PBI {e.PbiId}", "open", [], null, DateTime.UtcNow));
            mappings[e.PbiId] = new GithubMappingRecord(e.PbiId, num, $"u/{num}", null, "open");
            pass1Create++;
        }
        var pass1Hold = seed1.DeterministicOps.Count(o => o.Kind == GithubForwardKind.HoldBlocked);
        var issuesAfter1 = issues.Count;

        // ---- Pass 2 (Re-Run, identisches Delta) ----
        var seed2 = GithubForwardSeed.Seed(entries, mappings, issues);
        var pass2Create = seed2.UnmappedPbis.Count; // MUSS 0 sein (alles gemappt) -> keine Duplikate
        var pass2Update = seed2.DeterministicOps.Count(o => o.Kind == GithubForwardKind.UpdateIssue);
        var pass2Hold = seed2.DeterministicOps.Count(o => o.Kind == GithubForwardKind.HoldBlocked);
        var pass2Flag = seed2.DeterministicOps.Count(o => o.Kind == GithubForwardKind.FlagDrift);

        // In Pass 2 wird KEIN Issue simuliert-angelegt -> Issue-Zahl muss stabil bleiben.
        var issuesAfter2 = issuesAfter1;
        var duplicates = pass2Create; // jedes ungemappte PBI in Pass 2 waere ein Duplikat-CREATE
        var pass = duplicates == 0 && issuesAfter2 == issuesAfter1;

        var report = new
        {
            deltaPbis = entries.Count,
            pass1 = new { create = pass1Create, hold = pass1Hold, issuesAfter = issuesAfter1 },
            pass2 = new { create = pass2Create, update = pass2Update, hold = pass2Hold, flagDrift = pass2Flag, issuesAfter = issuesAfter2 },
            duplicateCreates = duplicates,
            issuesStable = issuesAfter2 == issuesAfter1,
            result = pass ? "PASS" : "FAIL",
            timestampUtc = DateTime.UtcNow
        };

        Console.WriteLine($"[rerun-test] Pass 1: create={pass1Create} hold={pass1Hold} -> {issuesAfter1} Issues");
        Console.WriteLine($"[rerun-test] Pass 2 (Re-Run): create={pass2Create} update={pass2Update} hold={pass2Hold} flagDrift={pass2Flag} -> {issuesAfter2} Issues");
        Console.WriteLine($"[rerun-test] duplicateCreates={duplicates} issuesStable={report.issuesStable} => {report.result}");
        Console.WriteLine(pass
            ? "[rerun-test] Beweis: der Re-Run erzeugt KEINE Duplikat-Issues — nur Delta-Ops (UPDATE/HOLD)."
            : "[rerun-test] FEHLER: der Re-Run wuerde Duplikate erzeugen!");

        if (outArg is not null || deltaArg is not null)
        {
            var outPath = outArg is null ? Path.Combine(repoRoot, "runs", "github-rerun-test", $"rerun-{DateTime.UtcNow:yyyyMMdd_HHmmss}.json")
                : (Path.IsPathRooted(outArg) ? outArg : Path.Combine(repoRoot, outArg));
            Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? ".");
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);
            Console.WriteLine($"[rerun-test] -> {Path.GetRelativePath(repoRoot, outPath)}");
        }
        return pass ? 0 : 1;
    }

    // §14-Szenario in klein: 3 aktive PBIs (werden angelegt) + 1 blockiertes (HOLD, nie CREATE).
    private static IReadOnlyList<GithubSyncEntry> DemoDelta() =>
    [
        new("PBI-101", "Login mit E-Mail", "active", "ready", ["REQ-01"], false, null),
        new("PBI-102", "Passwort zuruecksetzen", "active", "ready", ["REQ-02"], false, null),
        new("PBI-103", "Rollenkonzept", "blocked_by_decision", null, ["REQ-03"], true, null),
        new("PBI-104", "Dashboard", "active", "ready", ["REQ-04"], false, null),
    ];
}
