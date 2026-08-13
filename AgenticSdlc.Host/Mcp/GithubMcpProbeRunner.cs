using ModelContextProtocol.Client;

namespace AgenticSdlc.Host.Mcp;

// CLI: mcp-probe github [--local] — der C2d-②-SPIKE als wiederholbare, LLM-freie Probe (K-Methodik/R-38:
// empirischer Beweis gehört zur Beweiskette): verbindet zum OFFIZIELLEN github-mcp-server (Remote readonly
// per Default; --local = Docker-stdio), listet die gemounteten Tools und PRÜFT die Governance-Bedingung
// (readonly ⇒ keine Schreib-Tools sichtbar). Exit 0 = Spike grün.
public static class GithubMcpProbeRunner
{
    private static readonly string[] WriteMarkers = ["create", "update", "delete", "add_", "remove", "close", "reopen", "merge", "write"];

    public static void Register(IDictionary<string, CommandHandler> commands)
        => commands["mcp-probe"] = (args, _, _) => RunAsync(args);

    private static async Task<int> RunAsync(string[] args)
    {
        // Explizites Arg-Parsing statt stillem Ignorieren (Code-Hygiene 10.08.): optionaler Provider-Token
        // `github` (einziger unterstützter — offizielle Doku-Konvention) + `--local`; Unbekanntes = LAUT.
        var local = false;
        for (var i = 1; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--local", StringComparison.OrdinalIgnoreCase)) local = true;
            else if (string.Equals(args[i], "github", StringComparison.OrdinalIgnoreCase)) { /* Provider explizit ok */ }
            else { Console.Error.WriteLine($"[mcp-probe] unbekanntes Argument: '{args[i]}'. Usage: mcp-probe [github] [--local]"); return 2; }
        }
        var token = GithubMcp.ResolveToken();
        if (string.IsNullOrWhiteSpace(token))
        { Console.Error.WriteLine("[mcp-probe] kein Token (GITHUB_AGENTIC_REFACTOR_TOKEN/GITHUB_TEST_TOKEN/GITHUB_TOKEN)."); return 2; }

        Console.WriteLine($"[mcp-probe] verbinde: {(local ? "LOKAL (docker, stdio, READ_ONLY=1, toolset issues)" : $"REMOTE {GithubMcp.RemoteReadonlyIssuesEndpoint}")}");
        try
        {
            await using var client = local
                ? await GithubMcp.ConnectLocalReadonlyIssuesAsync(token!).ConfigureAwait(false)
                : await GithubMcp.ConnectRemoteReadonlyIssuesAsync(token!).ConfigureAwait(false);

            var tools = await client.ListToolsAsync().ConfigureAwait(false);
            Console.WriteLine($"[mcp-probe] server={client.ServerInfo.Name} v{client.ServerInfo.Version} · tools={tools.Count}");
            foreach (var t in tools.OrderBy(t => t.Name, StringComparer.Ordinal))
                Console.WriteLine($"[mcp-probe]   {t.Name} — {Truncate(t.Description)}");

            // Governance-Wache: readonly heisst KEINE Schreib-Tools — sonst ist der Spike ROT (laut).
            var writeish = tools.Where(t => WriteMarkers.Any(m => t.Name.Contains(m, StringComparison.OrdinalIgnoreCase))).ToList();
            if (writeish.Count > 0)
            {
                Console.Error.WriteLine($"[mcp-probe] GOVERNANCE-VERSTOSS: {writeish.Count} schreib-verdächtige Tools trotz readonly: "
                    + string.Join(", ", writeish.Select(t => t.Name)));
                return 1;
            }
            Console.WriteLine("[mcp-probe] readonly-Wache: PASS (keine Schreib-Tools gemountet) — Spike GRUEN.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[mcp-probe] Verbindung/Listing fehlgeschlagen: {ex.Message}");
            Console.Error.WriteLine("[mcp-probe] Auswege: --local (Docker) probieren · Token-Scopes pruefen · Netz pruefen.");
            return 1;
        }
    }

    private static string Truncate(string? s)
        => string.IsNullOrWhiteSpace(s) ? "" : s.Length <= 90 ? s : s[..90] + "…";
}
