using ModelContextProtocol.Client;

namespace AgenticSdlc.Host.Mcp;

/// <summary>
/// C2d ② (09.08.2026, c2d-plan §4 / K11) — die EINE GitHub-MCP-Verbindungs-Naht: MAF konsumiert den
/// OFFIZIELLEN github-mcp-server als externe Tool-Quelle (Feature-Matrix-Beweis MCP im realen Szenario).
/// GOVERNANCE SERVER-SEITIG, nicht per Eigenbau-Filter: Remote-Weg über das `/readonly`-URL-Suffix
/// (strikter Lese-Filter des Servers) + Toolset-Pfad `/x/issues`; Lokal-Weg (Docker) über
/// GITHUB_READ_ONLY=1 + GITHUB_TOOLSETS. Die gelieferten McpClientTool ERBEN von AIFunction (M.E.AI) —
/// sie sind direkt unsere Tool-Sorte, kein Adapter (Doku-Beleg: csharp-sdk getting-started).
/// K11-Grenze bleibt: MCP NUR fürs Agenten-LESEN — deterministische Bahnen (Snapshot/Forward/Vermerk)
/// fahren weiter typisiertes REST.
/// </summary>
public static class GithubMcp
{
    /// <summary>Remote-Endpoint des offiziellen Servers: Toolset `issues`, hart readonly (Server-Filter).</summary>
    public const string RemoteReadonlyIssuesEndpoint = "https://api.githubcopilot.com/mcp/x/issues/readonly";

    public static string? ResolveToken()
        => FirstNonEmpty(
            Environment.GetEnvironmentVariable("GITHUB_AGENTIC_REFACTOR_TOKEN"),
            Environment.GetEnvironmentVariable("GITHUB_TEST_TOKEN"),
            Environment.GetEnvironmentVariable("GITHUB_TOKEN"));

    /// <summary>Remote-Weg (kein lokaler Prozess): Streamable-HTTP + PAT-Bearer.</summary>
    public static async Task<McpClient> ConnectRemoteReadonlyIssuesAsync(string token, CancellationToken ct = default)
    {
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Name = "GithubMcpRemote",
            Endpoint = new Uri(RemoteReadonlyIssuesEndpoint),
            ConnectionTimeout = TimeSpan.FromSeconds(30),
            AdditionalHeaders = new Dictionary<string, string> { ["Authorization"] = $"Bearer {token}" },
        });
        return await McpClient.CreateAsync(transport, cancellationToken: ct).ConfigureAwait(false);
    }

    /// <summary>Lokal-Weg (Docker, stdio): offizielles Image; Governance über Server-Env (READ_ONLY/TOOLSETS).
    /// Token reist als Prozess-Env, NIE als Kommandozeilen-Argument.</summary>
    public static async Task<McpClient> ConnectLocalReadonlyIssuesAsync(string token, CancellationToken ct = default)
    {
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "GithubMcpLocal",
            Command = "docker",
            Arguments =
            [
                "run", "-i", "--rm",
                "-e", "GITHUB_PERSONAL_ACCESS_TOKEN",
                "-e", "GITHUB_READ_ONLY",
                "-e", "GITHUB_TOOLSETS",
                "ghcr.io/github/github-mcp-server",
            ],
            EnvironmentVariables = new Dictionary<string, string?>
            {
                ["GITHUB_PERSONAL_ACCESS_TOKEN"] = token,
                ["GITHUB_READ_ONLY"] = "1",
                ["GITHUB_TOOLSETS"] = "issues",
            },
        });
        return await McpClient.CreateAsync(transport, cancellationToken: ct).ConfigureAwait(false);
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
