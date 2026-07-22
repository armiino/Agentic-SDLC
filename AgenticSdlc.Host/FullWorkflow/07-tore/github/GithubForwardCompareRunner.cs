using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// CLI: github-forward-compare [fixtures.json] [--threshold N] [--plan <agent-plan.json>] [--out <file>]
//
// T3.6 — Vergleichspfad-Harness (plan-tor3 §9). Faehrt den DETERMINISTISCHEN Matcher gegen die Drift-Fixtures
// und misst ihn gegen die Gold-Labels (kein LLM). Optional wird ein AGENT-Plan (aus github-forward) gegen
// dieselben Gold-Labels gemessen -> direkter Vergleich agentisch vs deterministisch (Dedup-Recall / Precision).
public static class GithubForwardCompareRunner
{
    private const string DefaultFixtures = "AgenticSdlc.Host/FullWorkflow/07-tore/github/fixtures/drift-fixtures.json";
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        var threshold = 2;
        string? planArg = null, outArg = null, fixturesArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--threshold", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var t)) { threshold = t; i++; }
            else if (string.Equals(a, "--plan", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) planArg = args[++i];
            else if (string.Equals(a, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) outArg = args[++i];
            else if (!a.StartsWith("--", StringComparison.Ordinal)) fixturesArg ??= a;
        }

        var fixturesPath = fixturesArg is null ? Path.Combine(repoRoot, DefaultFixtures)
            : (Path.IsPathRooted(fixturesArg) ? fixturesArg : Path.Combine(repoRoot, fixturesArg));
        if (!File.Exists(fixturesPath)) { Console.Error.WriteLine($"[github-forward-compare] Fixtures nicht gefunden: {fixturesPath}"); return 2; }
        var fixture = await LoadAsync<DriftFixtureDocument>(fixturesPath).ConfigureAwait(false);

        // Deterministischer Kandidat.
        var detDecisions = fixture.Cases.ToDictionary(
            c => c.PbiId,
            c => ToDecision(GithubForwardDeterministicMatcher.Match(c.PbiId, c.Title, c.RequirementTexts, fixture.Issues, threshold)),
            StringComparer.Ordinal);
        var detReport = GithubForwardCompare.Evaluate(fixture, "deterministic", threshold, detDecisions);
        Print(detReport);

        var reports = new List<CompareReport> { detReport };

        // Optional: Agent-Plan gegen dieselben Gold-Labels.
        if (planArg is not null)
        {
            var planPath = Path.IsPathRooted(planArg) ? planArg : Path.Combine(repoRoot, planArg);
            if (!File.Exists(planPath)) { Console.Error.WriteLine($"[github-forward-compare] Agent-Plan nicht gefunden: {planPath}"); return 2; }
            var plan = await LoadAsync<GithubForwardPlanDocument>(planPath).ConfigureAwait(false);
            var agentDecisions = plan.Operations
                .Where(o => GithubForwardKind.Agentic.Contains(o.Kind))
                .GroupBy(o => o.PbiId, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => ToDecision(g.Last()), StringComparer.Ordinal);
            var agentReport = GithubForwardCompare.Evaluate(fixture, "agent", null, agentDecisions);
            Print(agentReport);
            reports.Add(agentReport);
            Console.WriteLine($"[github-forward-compare] DELTA dedupRecall: agent {agentReport.DedupRecall} vs deterministic {detReport.DedupRecall} (+{Math.Round(agentReport.DedupRecall - detReport.DedupRecall, 3)})");
        }

        var outPath = outArg is null ? Path.Combine(repoRoot, "runs", "github-compare", $"compare-{DateTime.UtcNow:yyyyMMdd_HHmmss}.json")
            : (Path.IsPathRooted(outArg) ? outArg : Path.Combine(repoRoot, outArg));
        Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? ".");
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(new { fixture = fixture.Name, threshold, reports }, Json)).ConfigureAwait(false);
        Console.WriteLine($"[github-forward-compare] -> {Path.GetRelativePath(repoRoot, outPath)}");
        return 0;
    }

    private static (string Kind, int? Issue) ToDecision(GithubForwardOp op) => (op.Kind, op.TargetIssueNumber);

    private static void Print(CompareReport r)
    {
        Console.WriteLine($"[github-forward-compare] {r.Candidate}: accuracy={r.Accuracy} dedupRecall={r.DedupRecall} linkPrecision={r.LinkPrecision} (goldLinks={r.GoldLinks} goldCreates={r.GoldCreates})");
        foreach (var c in r.Cases)
        {
            var mark = c.Verdict is GithubForwardCompare.CorrectLink or GithubForwardCompare.CorrectCreate ? "OK  " : "MISS";
            Console.WriteLine($"[github-forward-compare]   {mark} {c.PbiId}: gold={c.GoldKind}{(c.GoldIssue is null ? "" : $" #{c.GoldIssue}")} -> {c.DecisionKind}{(c.DecisionIssue is null ? "" : $" #{c.DecisionIssue}")} [{c.Verdict}]{(c.Note is null ? "" : $" ({c.Note})")}");
        }
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }
}
