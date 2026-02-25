using System;
using System.IO;
using System.Text.Json;
using AgenticSdlc.McpServer.Infrastructure;
using ModelContextProtocol.Server;

namespace AgenticSdlc.McpServer.Tools;

public class ApprovalTools
{
    [McpServerTool]
    public string RequestApproval(string action, string payload)
    {
        // runId aus dem json payload -> sonst unknown
        string runId = "unknown";
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("runId", out var rid) && rid.ValueKind == JsonValueKind.String)
                runId = rid.GetString() ?? "unknown";
        }
        catch
        {
            // todo: was wenn unknown? wie handle ich das
        }

        var approvalDir = Path.Combine("runs", runId, "approvals");
        var fullDir = RootPolicy.EnforceWrite(approvalDir);
        Directory.CreateDirectory(fullDir);

        var record = new
        {
            runId,
            action,
            payload,
            requestedAt = DateTime.UtcNow
        };

        var fileName = Path.Combine(fullDir, $"approval_{DateTime.UtcNow:yyyyMMddHHmmss}.json");
        File.WriteAllText(fileName, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));

        return "Approval request recorded.";
    }
}