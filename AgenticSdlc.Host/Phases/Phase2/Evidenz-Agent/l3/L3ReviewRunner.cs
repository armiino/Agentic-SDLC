using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

/// <summary>
/// L3 Human-Review (CLI: <c>l3-review &lt;l3-run|runId&gt;</c>). Config-gesteuert über <c>l3.reviewMode</c>:
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
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: l3-review <l3-run-dir|runId>   (Modus via config l3.reviewMode: file|interactive)"); return 2; }
        var runDir = ResolveRunDir(repoRoot, args[1]);
        if (runDir is null) { Console.Error.WriteLine($"[l3-review] L3-Lauf '{args[1]}' nicht gefunden."); return 2; }

        var routingPath = Path.Combine(runDir, "routing-report.json");
        if (!File.Exists(routingPath)) { Console.Error.WriteLine("[l3-review] routing-report.json fehlt — erst `l3` (Prepare) laufen lassen."); return 2; }
        var routed = (JsonSerializer.Deserialize<RoutingReport>(await File.ReadAllTextAsync(routingPath).ConfigureAwait(false), Json)?.Items ?? []).ToList();
        var decisionsPath = Path.Combine(runDir, "human-decisions.json");

        var interactive = string.Equals(settings.L3ReviewMode, "interactive", StringComparison.OrdinalIgnoreCase);
        if (!interactive)
        {
            var open = routed.Count(r => r.Class != L3Class.SupportedAnchored);
            Console.WriteLine($"[l3-review] mode=file (config l3.reviewMode). {open} Kandidaten zu entscheiden.");
            Console.WriteLine($"[l3-review] Öffne human-review-package.json, trage je Kandidat accept|edit|reject|revise ein und speichere als human-decisions.json.");
            Console.WriteLine($"[l3-review] Für die UI: setze l3.reviewMode=interactive in run-config.json.  run -> {Path.GetRelativePath(repoRoot, runDir)}");
            return 0;
        }

        // interactive: Umwelt für lazy Kontext-Auflösung rekonstruieren (config.json 'env').
        var env = await LoadEnvAsync(runDir, repoRoot).ConfigureAwait(false);
        var envById = env?.ItemsById() ?? new Dictionary<string, ArtifactItem>();
        var runId = Path.GetFileName(runDir);
        var session = L3ReviewAdapter.BuildSession(runId, routed);
        if (session.Items.Count == 0) { Console.WriteLine("[l3-review] keine zu entscheidenden Kandidaten (alle SupportedAnchored)."); return 0; }

        async Task Persist() => await File.WriteAllTextAsync(decisionsPath, JsonSerializer.Serialize(L3ReviewAdapter.Apply(runId, session), Json)).ConfigureAwait(false);

        var options = new ReviewServerOptions
        {
            Session = session,
            RecomputeResolved = L3ReviewAdapter.Resolved,
            ResolveContext = (_, key) => Task.FromResult(L3ReviewAdapter.ResolveContext(key, envById)),
            OnItemSaved = async _ => await Persist().ConfigureAwait(false),   // Autosave-Sicherheitsnetz
            OpenBrowser = settings.L3ReviewOpenBrowser
        };

        Console.WriteLine($"[l3-review] mode=interactive  runId={runId}  {session.Items.Count} Kandidaten");
        var result = await LocalReviewServerHost.RunAsync(options).ConfigureAwait(false);
        await Persist().ConfigureAwait(false);   // finaler Stand

        Console.WriteLine($"[l3-review] {result.Outcome} — {session.ResolvedCount()}/{session.Items.Count} entschieden -> human-decisions.json");
        Console.WriteLine($"[l3-review] Anwenden: `l3-apply {runId}` (accept/edit/reject) und/oder `l3-revise {runId}` (revise).");
        return 0;
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
