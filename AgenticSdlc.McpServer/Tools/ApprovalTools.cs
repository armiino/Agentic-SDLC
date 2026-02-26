using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using AgenticSdlc.McpServer.Infrastructure;
using ModelContextProtocol.Server;

namespace AgenticSdlc.McpServer.Tools;

public class ApprovalTools
{
    [McpServerTool]
    public string RequestApproval(string action, string payload)
    {
        //runId aus dem json payload extrahieren
        var runId = ExtractRunIdOrThrow(payload);
        
        var approvalDir = Path.Combine("runs", runId, "approvals");
        var fullDir = RootPolicy.EnforceWrite(approvalDir);
        Directory.CreateDirectory(fullDir);

        //nur eine Approval pro runID
        //wenn schon existiert nichts neues schreiben (problem war mehrere reqzests)
        var existing = Directory.GetFiles(fullDir, "approval_*.json", SearchOption.TopDirectoryOnly)
            .Any(f =>
            {
                try
                {
                    using var doc = JsonDocument.Parse(File.ReadAllText(f));
                    var root = doc.RootElement;

                    var rid = root.TryGetProperty("runId", out var r) ? r.GetString() : null;
                    var act = root.TryGetProperty("action", out var a) ? a.GetString() : null;

                    return string.Equals(rid, runId, StringComparison.Ordinal) &&
                           string.Equals(act, action, StringComparison.Ordinal);
                }
                catch
                {
                    return false;
                }
            });

        if (existing)
            return "Approval already created";
        
        var record = new
        {
            runId,
            action,
            payload,
            requestedAt = DateTime.UtcNow
        };

        // in ms damit nicht aus versehen überschrieben wird
        var fileName = Path.Combine(fullDir, $"approval_{DateTime.UtcNow:yyyyMMddHHmmssfff}.json");
        File.WriteAllText(fileName, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }));

        return "Approval request recorded.";
    }

    private static string ExtractRunIdOrThrow(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("runId", out var rid) && rid.ValueKind == JsonValueKind.String)
            {
                var runId = rid.GetString();
                if (!string.IsNullOrWhiteSpace(runId))
                    return runId!;
            }
        }
        catch (JsonException)
        {
            //TODO
        }

        throw new ArgumentException("payload must be valid JSON and contain a non-empty string property 'runId'.");
    }
}