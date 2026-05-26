using System.Text;
using System.Text.Json;

namespace AgenticSdlc.Host.Run;

public sealed class RunContext
{
    public string RunId { get; }
    public string PhaseSelector { get; }
    public string RunDir { get; }
    public string LogsDir => Path.Combine(RunDir, "logs");
    public string SnapshotsDir => Path.Combine(RunDir, "snapshots");
    public string DocsSnapshotDir => Path.Combine(SnapshotsDir, "docs");
    public string DiffsDir => Path.Combine(LogsDir, "diffs");
    public string AgentsLogDir => Path.Combine(LogsDir, "agents");

    public string EventsPath => Path.Combine(LogsDir, "events.jsonl");
    public string ToolDiscoveryPath => Path.Combine(LogsDir, "tool-discovery.json");
    public string ConfigPath => Path.Combine(RunDir, "config.json");
    public string ChangesPath => Path.Combine(LogsDir, "changes.txt");
    public string DecisionLogPath => Path.Combine(LogsDir, "decision-log.jsonl");

    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public RunContext(string runId, string phaseSelector)
    {
        RunId = runId;
        PhaseSelector = NormalizePathSegment(phaseSelector);
        RunDir = Path.Combine("runs", PhaseSelector, runId);
    }

    public void EnsureFolders()
    {
        Directory.CreateDirectory(RunDir);
        Directory.CreateDirectory(LogsDir);
        Directory.CreateDirectory(SnapshotsDir);
        Directory.CreateDirectory(DocsSnapshotDir);
        Directory.CreateDirectory(DiffsDir);
        Directory.CreateDirectory(AgentsLogDir);
        if (!File.Exists(EventsPath)) File.WriteAllText(EventsPath, "");
        // erweiterung: separates Log fuer erklaerungspflichtige Agent-Handlungen, damit "warum geschrieben?" nicht in events.jsonl untergeht.
        if (!File.Exists(DecisionLogPath)) File.WriteAllText(DecisionLogPath, "");
    }

    public void WriteConfig(object config)
        => File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, _json));

    public void WriteToolDiscovery(object discovery)
        => File.WriteAllText(ToolDiscoveryPath, JsonSerializer.Serialize(discovery, _json));

    public void AppendEvent(object evt)
    {
        var line = JsonSerializer.Serialize(evt, _json);
        File.AppendAllText(EventsPath, line + Environment.NewLine);
    }

    public void AppendDecision(object decision)
    {
        // erweiterung: Decision-Events sind keine internen Modellgedanken, sondern deklarierte Handlungsgruende zu beobachtbaren Tool-Effekten.
        var line = JsonSerializer.Serialize(decision, _json);
        File.AppendAllText(DecisionLogPath, line + Environment.NewLine);
    }

    public void AppendAgentEvent(string? agentName, object evt)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        var line = JsonSerializer.Serialize(evt, _json);
        File.AppendAllText(Path.Combine(agentDir, "events.jsonl"), line + Environment.NewLine);
    }

    public void AppendAgentToolCall(string? agentName, object evt)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        var line = JsonSerializer.Serialize(evt, _json);
        File.AppendAllText(Path.Combine(agentDir, "tool-calls.jsonl"), line + Environment.NewLine);
    }

    public void AppendAgentDecision(string? agentName, object decision)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        var line = JsonSerializer.Serialize(decision, _json);
        File.AppendAllText(Path.Combine(agentDir, "decisions.jsonl"), line + Environment.NewLine);
    }

    public string? WriteAgentTextFile(string? agentName, string fileName, string content)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return null;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        var fullPath = Path.Combine(agentDir, fileName);
        File.WriteAllText(fullPath, content, Encoding.UTF8);
        return Path.GetRelativePath(RunDir, fullPath).Replace('\\', '/');
    }

    public string WriteDiffFile(string relativePath, string content)
    {
        var safeName = relativePath
            .Replace('\\', '_')
            .Replace('/', '_')
            .Replace(':', '_');

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeName}.diff";
        var full = Path.Combine(DiffsDir, fileName);
        File.WriteAllText(full, content, Encoding.UTF8);
        return Path.GetRelativePath(RunDir, full).Replace('\\', '/');
    }

    public string ApprovalsDir => Path.Combine(RunDir, "approvals");

    private string GetAgentLogDir(string agentName)
    {
        var safeAgentName = NormalizePathSegment(agentName);
        return Path.Combine(AgentsLogDir, safeAgentName);
    }

    private static string NormalizePathSegment(string value)
    {
        var safe = value
            .Replace('\\', '_')
            .Replace('/', '_')
            .Replace(':', '_')
            .Trim();

        return string.IsNullOrWhiteSpace(safe) ? "unknown" : safe;
    }

    public bool ApprovalExists(string action)
    {
        var dir = ApprovalsDir;
        if (!Directory.Exists(dir)) return false;

        foreach (var file in Directory.GetFiles(dir, "approval_*.json"))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(file));
                var root = doc.RootElement;

                var act = root.TryGetProperty("action", out var a) ? a.GetString() : null;
                if (string.Equals(act, action, StringComparison.Ordinal))
                    return true;
            }
            catch { }
        }
        return false;
    }
}
