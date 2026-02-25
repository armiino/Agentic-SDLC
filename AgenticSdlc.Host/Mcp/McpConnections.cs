using ModelContextProtocol.Client;

namespace AgenticSdlc.Host.Mcp;

public static class McpConnections
{
    public static async Task<McpClient> ConnectLocalAsync()
    {
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "LocalMcp",
            Command = "dotnet",
            Arguments =
            [
                "run",
                "--project", "AgenticSdlc.McpServer/AgenticSdlc.McpServer.csproj",
                "--no-build"
            ]
        });

        return await McpClient.CreateAsync(transport).ConfigureAwait(false);
    }

    
    //dummy vorbereitungen für später wenn github usage kommt..
    public static async Task<McpClient> ConnectGitHubAsync()
    {
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "GitHubMcp",
            Command = "npx",
            Arguments = ["-y", "--verbose", "@modelcontextprotocol/server-github"]
        });

        return await McpClient.CreateAsync(transport).ConfigureAwait(false);
    }
}