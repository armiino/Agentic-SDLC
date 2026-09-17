using System.Text;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

// Deterministische, lesbare Markdown-Projektion des Product Backlog. Wahrheit bleibt product-backlog.json;
// das MD ist nur eine Arbeitsansicht (wie l4-requirements-doc fuer die Baseline). Kein LLM.
public static class ReClarifyBacklogDocRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l4-re-clarify-backlog-doc <l4-re-clarify-run|dir> [--out <product-backlog.md>]");
            return 2;
        }

        string? token = null, output = null;
        for (var i = 1; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { output = args[++i]; continue; }
            token ??= args[i];
        }

        var backlogDir = ResolveBacklogDir(repoRoot, token ?? "");
        if (backlogDir is null)
        {
            Console.Error.WriteLine($"[l4-re-clarify-backlog-doc] Backlog-Lauf '{token}' nicht gefunden.");
            return 2;
        }
        var applied = Path.Combine(backlogDir, "applied", "product-backlog.json");
        var backlogPath = File.Exists(applied) ? applied : Path.Combine(backlogDir, "product-backlog.json");
        if (!File.Exists(backlogPath))
        {
            Console.Error.WriteLine("[l4-re-clarify-backlog-doc] product-backlog.json fehlt.");
            return 2;
        }

        var backlog = JsonSerializer.Deserialize<ProductBacklogDocument>(await File.ReadAllTextAsync(backlogPath).ConfigureAwait(false), Json)!;
        var md = Render(backlog, File.Exists(applied));
        var outPath = output is null
            ? Path.Combine(backlogDir, "product-backlog.md")
            : (Path.IsPathRooted(output) ? output : Path.Combine(repoRoot, output));
        await File.WriteAllTextAsync(outPath, md).ConfigureAwait(false);

        Console.WriteLine($"[l4-re-clarify-backlog-doc] {backlog.Items.Count} PBIs -> {Path.GetRelativePath(repoRoot, outPath)}");
        return 0;
    }

    private static string Render(ProductBacklogDocument b, bool applied)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Product Backlog");
        sb.AppendLine();
        sb.AppendLine($"- backlogId: `{b.BacklogId}` · projectId: `{b.ProjectId}` · baselineId: `{b.BaselineId}`");
        sb.AppendLine($"- Quelle: {(applied ? "akzeptierte View (nach HumanReview/Apply)" : "roher Clarify-Output")} · Stand: {b.CreatedUtc:yyyy-MM-dd HH:mm}Z");
        sb.AppendLine($"- PBIs: **{b.Items.Count}** · Wahrheit bleibt `product-backlog.json` (dies ist die lesbare Projektion).");
        sb.AppendLine();

        // Uebersicht nach Readiness
        var order = new[] { "backlog_ready", "ready_with_nonblocking_questions", "blocked_by_decision" };
        int Rank(string? r) { var i = Array.IndexOf(order, r ?? ""); return i < 0 ? order.Length : i; }
        var items = b.Items.OrderBy(p => Rank(p.Readiness)).ThenBy(p => p.PriorityRank ?? int.MaxValue).ThenBy(p => p.PbiId, StringComparer.Ordinal).ToList();

        sb.AppendLine("## Uebersicht");
        sb.AppendLine();
        sb.AppendLine("| PBI | Titel | MVP | Readiness | Rank | AK | offen |");
        sb.AppendLine("|-----|-------|-----|-----------|------|----|-------|");
        foreach (var p in items)
            sb.AppendLine($"| {p.PbiId} | {Esc(p.Title)} | {p.Mvp ?? "-"} | {p.Readiness ?? "-"} | {p.PriorityRank?.ToString() ?? "-"} | {p.AcceptanceCriteria.Count} | {p.OpenDecisions.Count} |");
        sb.AppendLine();

        sb.AppendLine("## Details");
        sb.AppendLine();
        foreach (var p in items)
        {
            sb.AppendLine($"### {p.PbiId} — {p.Title}");
            sb.AppendLine();
            sb.AppendLine($"`type={p.Type}` · `mvp={p.Mvp ?? "-"}` · `readiness={p.Readiness ?? "-"}` · `rank={p.PriorityRank?.ToString() ?? "-"}`");
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(p.Goal)) { sb.AppendLine($"**Statement:** {Esc(p.Goal!)}"); sb.AppendLine(); }
            if (p.Scope is { } sc && (sc.InScope.Count > 0 || sc.OutOfScope.Count > 0))
            {
                if (sc.InScope.Count > 0) { sb.AppendLine("**In Scope:**"); foreach (var s in sc.InScope) sb.AppendLine($"- {Esc(s)}"); sb.AppendLine(); }
                if (sc.OutOfScope.Count > 0) { sb.AppendLine("**Out of Scope:**"); foreach (var s in sc.OutOfScope) sb.AppendLine($"- {Esc(s)}"); sb.AppendLine(); }
            }
            if (p.AcceptanceCriteria.Count > 0) { sb.AppendLine("**Akzeptanzkriterien:**"); foreach (var a in p.AcceptanceCriteria) sb.AppendLine($"- {Esc(a)}"); sb.AppendLine(); }
            if (p.OpenDecisions.Count > 0)
            {
                sb.AppendLine("**Offene Entscheidungen:**");
                foreach (var o in p.OpenDecisions)
                    sb.AppendLine($"- {(o.BlocksScope ? "**[BLOCKS]** " : "")}[{o.Kind}/{o.Resolution}/{o.Evidence}] {Esc(o.Question)}");
                sb.AppendLine();
            }
            sb.AppendLine($"**Traceability:** requirementIds: {string.Join(", ", p.RequirementIds)}");
            if (p.Dependencies.Count > 0) sb.AppendLine($" · dependencies: {string.Join(", ", p.Dependencies)}");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string Esc(string s) => (s ?? "").Replace("|", "\\|").Replace("\n", " ").Trim();

    private static string? ResolveBacklogDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "product-backlog.json"))) return full;
        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "backlog", "product-backlog.json"))) return Path.Combine(full, "backlog");
        var root = Path.Combine(repoRoot, "runs", "l4-re-clarify");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var backlog = Path.Combine(dir, "backlog");
            if (File.Exists(Path.Combine(backlog, "product-backlog.json"))) return backlog;
            if (File.Exists(Path.Combine(dir, "product-backlog.json"))) return dir;
        }
        return null;
    }
}
