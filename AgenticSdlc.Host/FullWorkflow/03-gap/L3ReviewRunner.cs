using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

/// <summary>
/// L3 Human-Review (CLI: <c>l3-review &lt;l3-run|runId&gt; [--interactive|--file] [--no-browser] [--scope needs-human|all]</c>).
/// Config-gesteuert über <c>l3.reviewMode</c>, CLI-Flags überschreiben fuer Einzelruns:
/// <list type="bullet">
/// <item><b>file</b> (Default): kein UI — weist auf das dateibasierte Paket hin (Mensch editiert
/// <c>human-review-package.json</c> → <c>human-decisions.json</c>).</item>
/// <item><b>interactive</b>: startet die generische <c>AgenticSdlc.HumanReview</c>-UI (Autosave) und schreibt bei
/// „Fertig" die <c>human-decisions.json</c>, die <c>l3-apply</c>/<c>l3-revise</c> konsumieren.</item>
/// </list>
/// Reuse des bestehenden Review-Servers + der Adapter-Konvention. Exit: 0 = ok, 2 = Usage/IO.
/// </summary>
public static class L3ReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: l3-review <l3-run-dir|runId> [--interactive|--file] [--no-browser] [--scope needs-human|all]"); return 2; }
        var runDir = ResolveRunDir(repoRoot, args[1]);
        if (runDir is null) { Console.Error.WriteLine($"[l3-review] L3-Lauf '{args[1]}' nicht gefunden."); return 2; }

        var routingPath = Path.Combine(runDir, "routing-report.json");
        if (!File.Exists(routingPath)) { Console.Error.WriteLine("[l3-review] routing-report.json fehlt — erst `l3` (Prepare) laufen lassen."); return 2; }
        var routed = (JsonSerializer.Deserialize<RoutingReport>(await File.ReadAllTextAsync(routingPath).ConfigureAwait(false), Json)?.Items ?? []).ToList();
        var decisionsPath = Path.Combine(runDir, "human-decisions.json");

        var forceInteractive = args.Contains("--interactive", StringComparer.OrdinalIgnoreCase);
        var forceFile = args.Contains("--file", StringComparer.OrdinalIgnoreCase);
        var noBrowser = args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase);
        var scope = ParseScope(args);
        var includeSupportedAnchored = string.Equals(scope, "all", StringComparison.OrdinalIgnoreCase);
        var interactive = forceInteractive || (!forceFile && string.Equals(settings.L3ReviewMode, "interactive", StringComparison.OrdinalIgnoreCase));
        if (!interactive)
        {
            var open = includeSupportedAnchored ? routed.Count : routed.Count(r => r.Class != L3Class.SupportedAnchored);
            Console.WriteLine($"[l3-review] mode=file (config l3.reviewMode). scope={scope}. {open} Kandidaten zu entscheiden/prüfen.");
            Console.WriteLine(includeSupportedAnchored
                ? "[l3-review] scope=all ist als Kontrollschicht fuer die UI gedacht. Starte mit --interactive --scope all; Entscheidungen landen in human-decisions.json."
                : "[l3-review] Öffne human-review-package.json, trage je Kandidat accept|edit|reject|revise ein und speichere als human-decisions.json.");
            Console.WriteLine($"[l3-review] Für die UI: `l3-review {Path.GetFileName(runDir)} --interactive --scope {scope}` oder setze l3.reviewMode=interactive in run-config.json.  run -> {Path.GetRelativePath(repoRoot, runDir)}");
            return 0;
        }

        // interactive: Umwelt für lazy Kontext-Auflösung rekonstruieren (config.json 'env').
        var env = await LoadEnvAsync(runDir, repoRoot).ConfigureAwait(false);
        var envById = env?.ItemsById() ?? new Dictionary<string, ArtifactItem>();
        var runId = Path.GetFileName(runDir);
        var session = L3ReviewAdapter.BuildSession(runId, routed, includeSupportedAnchored);
        if (session.Items.Count == 0) { Console.WriteLine("[l3-review] keine zu entscheidenden Kandidaten (alle SupportedAnchored)."); return 0; }
        var existingDecisions = await LoadExistingDecisionsAsync(decisionsPath).ConfigureAwait(false);
        L3ReviewAdapter.MergeExistingDecisions(session, existingDecisions);
        foreach (var item in session.Items)
            item.Resolved = L3ReviewAdapter.Resolved(item);

        Console.WriteLine($"[l3-review] mode=interactive  scope={scope}  runId={runId}  {session.Items.Count} Kandidaten");
        if (existingDecisions is not null)
            Console.WriteLine($"[l3-review] Re-Launch: vorhandene human-decisions.json geladen ({session.ResolvedCount()}/{session.Items.Count} resolved).");
        var (_, outcome) = await ReviewUiFlow.RunAsync(session, decisionsPath,
            resolved: L3ReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(L3ReviewAdapter.ResolveContext(key, envById)),
            apply: s => L3ReviewAdapter.Apply(runId, s),
            openBrowser: settings.L3ReviewOpenBrowser && !noBrowser).ConfigureAwait(false);
        Console.WriteLine($"[l3-review] {outcome} — {session.ResolvedCount()}/{session.Items.Count} entschieden -> human-decisions.json");
        Console.WriteLine($"[l3-review] Anwenden: `l3-apply {runId}` (accept/edit/reject) und/oder `l3-revise {runId}` (revise).");
        return 0;
    }

    private static string ParseScope(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--all", StringComparison.OrdinalIgnoreCase)) return "all";
            if (string.Equals(args[i], "--needs-human", StringComparison.OrdinalIgnoreCase)) return "needs-human";
            if (!string.Equals(args[i], "--scope", StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 >= args.Length) return "needs-human";
            var value = args[i + 1].Trim().ToLowerInvariant();
            return value is "all" ? "all" : "needs-human";
        }
        return "needs-human";
    }

    private static async Task<HumanDecisionsFile?> LoadExistingDecisionsAsync(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<HumanDecisionsFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l3-review] WARNUNG: vorhandene human-decisions.json konnte nicht geladen werden: {ex.Message}");
            return null;
        }
    }

    private static async Task<SourceArtifactSet?> LoadEnvAsync(string runDir, string repoRoot)
    {
        var configPath = Path.Combine(runDir, "config.json");
        if (!File.Exists(configPath)) return null;
        using var cfg = JsonDocument.Parse(await File.ReadAllTextAsync(configPath).ConfigureAwait(false));
        if (!cfg.RootElement.TryGetProperty("env", out var envEl) || envEl.ValueKind != JsonValueKind.Array) return null;
        var sources = new List<ArtifactDocument>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in envEl.EnumerateArray())
        {
            var rel = p.GetString();
            if (string.IsNullOrWhiteSpace(rel)) continue;
            var full = Path.IsPathRooted(rel) ? rel : Path.Combine(repoRoot, rel);
            if (!File.Exists(full)) continue;
            var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(full).ConfigureAwait(false), Json);
            if (doc is not null && doc.Items.Count > 0 && seen.Add(doc.ArtifactType)) sources.Add(doc);
        }
        return sources.Count > 0 ? new SourceArtifactSet(sources) : null;
    }

    private static string? ResolveRunDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full)) return full;
        var l3Root = Path.Combine(repoRoot, "runs", "l3");
        return Directory.Exists(l3Root) ? Directory.EnumerateDirectories(l3Root).FirstOrDefault(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)) : null;
    }

    private sealed record RoutingReport([property: JsonPropertyName("items")] IReadOnlyList<L3RoutedCandidate> Items);
}
