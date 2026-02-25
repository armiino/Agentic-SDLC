using System.Text.Json;

namespace AgenticSdlc.Host.Run;

//zum tracken der genauen logs etc.
public sealed class RunContext
{
    public string RunId { get; }
    public string RunDir { get; }
    public string LogsDir => Path.Combine(RunDir, "logs");
    public string SnapshotsDir => Path.Combine(RunDir, "snapshots");
    public string DocsSnapshotDir => Path.Combine(SnapshotsDir, "docs");

    public string EventsPath => Path.Combine(LogsDir, "events.jsonl");
    public string ToolDiscoveryPath => Path.Combine(LogsDir, "tool-discovery.json");
    public string ConfigPath => Path.Combine(RunDir, "config.json");

    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public RunContext(string runId)
    {
        RunId = runId;
        RunDir = Path.Combine("runs", runId);
    }

    public void EnsureFolders()
    {
        Directory.CreateDirectory(RunDir);
        Directory.CreateDirectory(LogsDir);
        Directory.CreateDirectory(SnapshotsDir);
        Directory.CreateDirectory(DocsSnapshotDir);
        if (!File.Exists(EventsPath)) File.WriteAllText(EventsPath, "");
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
}