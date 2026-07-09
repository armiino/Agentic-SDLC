using System.Text;
using System.Text.Json;

namespace AgenticSdlc.Host.Run;

/// <summary>
/// Verwaltet Pfade und Log-Dateien für einen einzelnen Run.
/// </summary>
/// <remarks>
/// Verantwortlichkeit: Pfade kennen, Verzeichnisse anlegen, JSONL/Text schreiben.
/// Keine fachliche Logik — kein Wissen über Agenten, Tools oder SDLC-Inhalte.
/// </remarks>
public sealed class RunContext
{
    // UTF-8 ohne BOM für alle Textdateien.
    // Utf8NoBom hat in .NET ein BOM-Preamble das Markdown-Renderer
    // und viele Tools stört — deshalb explizit ohne BOM.
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

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

    // Serialisiert Appends an die GETEILTEN Run-Dateien (events.jsonl, decision-log.jsonl): unter Fan-out schreiben
    // mehrere Zweige parallel dieselbe Datei → ohne Lock gehen Zeilen verloren (Logging-Race, recipe 5b745e:
    // 1 statt 2 BASELINE_SELECTED). NUR für geteilte Dateien — per-Agent-Logs sind nach Agentname partitioniert und
    // dürfen NICHT global gelockt werden (globale Serialisierung aller Zweig-Logs störte das Timing so, dass die
    // Fan-in-Barrier ausfiel: recipe 4ec4d0, 0 statt 2 collected).
    private readonly object _fileLock = new();

    public RunContext(string runId, string phaseSelector)
    {
        RunId = runId;
        // PhaseSelector darf Unter-Ordner enthalten (z.B. "phase2evidenz-agent/transcript"): pro Segment
        // normalisieren, Verzeichnisstruktur erhalten. Einzelsegment-Selektoren (phase2_1, phase2B, …) bleiben unverändert.
        var segments = phaseSelector.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizePathSegment).Where(s => s.Length > 0).ToArray();
        if (segments.Length == 0) segments = ["unknown"];
        PhaseSelector = string.Join('/', segments);
        RunDir = Path.Combine(new[] { "runs" }.Concat(segments).Append(runId).ToArray());
    }

    /// <summary>
    /// Ergibt einen (angelegten) Ausgabe-Unterordner im Run, z. B. "baselines/requirements" oder
    /// "derivations/derived-risks". Leer/null = RunDir selbst. Zweck: bei Fan-out/Rezept schreibt JEDER Zweig bzw.
    /// jede Ableitung in einen eigenen Unterordner → keine Kollision fester Dateinamen. Logs bleiben oben (ein Strom).
    /// </summary>
    public string OutputDir(string? scope)
    {
        var dir = string.IsNullOrWhiteSpace(scope)
            ? RunDir
            : Path.Combine(new[] { RunDir }.Concat(
                scope.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries).Select(NormalizePathSegment)).ToArray());
        Directory.CreateDirectory(dir);
        return dir;
    }

    public void EnsureFolders()
    {
        Directory.CreateDirectory(RunDir);
        Directory.CreateDirectory(LogsDir);
        Directory.CreateDirectory(SnapshotsDir);
        Directory.CreateDirectory(DocsSnapshotDir);
        Directory.CreateDirectory(DiffsDir);
        Directory.CreateDirectory(AgentsLogDir);

        // Leere Dateien vorab anlegen, damit sie im Run-Verzeichnis sichtbar sind
        // auch wenn kein einziges Event geschrieben wurde (z.B. bei frühem Abbruch).
        if (!File.Exists(EventsPath)) File.WriteAllText(EventsPath, "");
        if (!File.Exists(DecisionLogPath)) File.WriteAllText(DecisionLogPath, "");
    }

    public void WriteConfig(object config)
        => File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, _json));

    public void WriteToolDiscovery(object discovery)
        => File.WriteAllText(ToolDiscoveryPath, JsonSerializer.Serialize(discovery, _json));

    public void AppendEvent(object evt)
    {
        var line = JsonSerializer.Serialize(evt, _json);
        lock (_fileLock) File.AppendAllText(EventsPath, line + Environment.NewLine);
    }

    public void AppendDecision(object decision)
    {
        // decision-log.jsonl: fokussierter Log für FILE_WRITE_ANALYZED-Events.
        // Enthält model-declared rationale (intent/reason/evidence) und belegbare
        // Write-Fakten (SHA256, writeEffect, diff). Kein Ersatz für events.jsonl.
        var line = JsonSerializer.Serialize(decision, _json);
        lock (_fileLock) File.AppendAllText(DecisionLogPath, line + Environment.NewLine);
    }

    public void AppendAgentEvent(string? agentName, object evt)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        // Per-Agent-Datei: nach Agentname partitioniert (im Fan-out haben Zweige distinkte Namen → verschiedene
        // Dateien) → kein globaler Lock nötig. Ein globaler Lock hier würde ALLE Zweig-Logs serialisieren und das
        // Nebenläufigkeits-Timing so stören, dass die Fan-in-Barrier ausfällt (belegt: recipe 5b745e/4ec4d0).
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

    public void AppendAgentInputContext(string? agentName, object inputContext)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        var line = JsonSerializer.Serialize(inputContext, _json);
        File.AppendAllText(Path.Combine(agentDir, "input-context.jsonl"), line + Environment.NewLine);
    }

    /// <summary>
    /// Protokolliert einen MAF-Shared-State-Zugriff (Lesen/Schreiben) eines Agenten.
    /// </summary>
    /// <remarks>
    /// B-Pendant zu input-context: State-Zugriffe sind KEINE MCP-Tool-Calls und erscheinen daher
    /// nicht in tool-calls.jsonl. Dieses Log belegt, welchen State-Inhalt jeder Agent in Phase 2.1B
    /// (artifact_state) erhalten bzw. erzeugt hat (Key, Scope, Hash, Laenge, Preview).
    /// </remarks>
    public void AppendAgentStateAccess(string? agentName, object stateAccess)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        var line = JsonSerializer.Serialize(stateAccess, _json);
        File.AppendAllText(Path.Combine(agentDir, "state-access.jsonl"), line + Environment.NewLine);
    }

    public void AppendAgentInputContextMarkdown(string? agentName, string markdown)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        File.AppendAllText(Path.Combine(agentDir, "input-context.md"), markdown, Utf8NoBom);
    }

    /// <summary>
    /// Hängt einen lesbaren Markdown-Abschnitt an die Response-Text-Datei des Agenten an.
    /// </summary>
    /// <remarks>
    /// response-text.md akkumuliert den Text-Output des Modells pro Chat-Iteration.
    /// Das ist die direkt lesbare Forschungsquelle für die Frage:
    /// "Was hat das Modell in dieser Iteration begründet oder formuliert?"
    ///
    /// Die Datei wird beim ersten Aufruf mit einem Agentnamen-Header angelegt.
    /// Jeder weitere Aufruf hängt einen neuen Iterationsabschnitt an.
    /// </remarks>
    public void AppendAgentResponseText(string? agentName, string markdown)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        var filePath = Path.Combine(agentDir, "response-text.md");

        // Header nur beim ersten Schreiben setzen, damit die Datei als eigenständiges Dokument lesbar ist.
        if (!File.Exists(filePath))
            File.WriteAllText(filePath, $"# Response Text — {agentName}\n\n", Utf8NoBom);

        File.AppendAllText(filePath, markdown, Utf8NoBom);
    }

    public string? WriteAgentTextFile(string? agentName, string fileName, string content)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return null;

        var agentDir = GetAgentLogDir(agentName);
        Directory.CreateDirectory(agentDir);

        var fullPath = Path.Combine(agentDir, fileName);
        File.WriteAllText(fullPath, content, Utf8NoBom);
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
        File.WriteAllText(full, content, Utf8NoBom);
        return Path.GetRelativePath(RunDir, full).Replace('\\', '/');
    }

    public string ApprovalsDir => Path.Combine(RunDir, "approvals");
    public string JuryDir => Path.Combine(RunDir, "jury");

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
