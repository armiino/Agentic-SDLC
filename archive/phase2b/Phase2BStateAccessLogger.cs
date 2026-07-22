using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.Phases.Phase2.Phase2B;

/// <summary>
/// Protokolliert Shared-State-Lese-/Schreibzugriffe pro Agent (Evidenz für Phase 2.1B).
/// </summary>
/// <remarks>
/// State-Zugriffe sind keine MCP-Tool-Calls → sie erscheinen NICHT in tool-calls.jsonl.
/// Dieser Logger schreibt sie nach <c>logs/agents/&lt;agent&gt;/state-access.jsonl</c> und spiegelt sie
/// als <c>STATE_ACCESS</c>-Event in die globale Timeline. Damit ist belegbar, welchen State-Inhalt
/// jeder Agent erhalten (read) bzw. erzeugt (write) hat — das B-Pendant zu input-context.md.
/// </remarks>
public sealed class Phase2BStateAccessLogger
{
    private const int PreviewChars = 400;

    private readonly RunContext _run;

    public Phase2BStateAccessLogger(RunContext run) => _run = run;

    public void LogRead(string agentName, string key, string? content)
        => Log(agentName, direction: "read", key, content);

    public void LogWrite(string agentName, string key, string? content)
        => Log(agentName, direction: "write", key, content);

    /// <summary>Protokolliert einen fehlgeschlagenen Read (Key nicht im State vorhanden).</summary>
    public void LogMissingRead(string agentName, string key)
    {
        var record = new
        {
            type = "STATE_ACCESS",
            runId = _run.RunId,
            agentName,
            direction = "read",
            scope = Phase2BState.Scope,
            key,
            present = false,
            timestampUtc = DateTime.UtcNow
        };

        _run.AppendAgentStateAccess(agentName, record);
        _run.AppendEvent(record);
    }

    private void Log(string agentName, string direction, string key, string? content)
    {
        content ??= string.Empty;

        var record = new
        {
            type = "STATE_ACCESS",
            runId = _run.RunId,
            agentName,
            direction,
            scope = Phase2BState.Scope,
            key,
            present = true,
            length = content.Length,
            sha256 = Sha256(content),
            preview = Truncate(content, PreviewChars),
            timestampUtc = DateTime.UtcNow
        };

        _run.AppendAgentStateAccess(agentName, record);
        _run.AppendEvent(record);
    }

    /// <summary>
    /// K4 (Overwrite-Diagnose, NUR Logging): zählt aus dem Agent-Tool-Log, wie oft der Agent sein
    /// Pflichtartefakt per fs_write geschrieben hat. Bei mehr als einem Write wird ein
    /// <c>ARTIFACT_SUSPICIOUS_OVERWRITE</c>-Event emittiert (kein Hard-Block — ein zweiter Write kann
    /// legitim besser sein; das Logging liefert erst die Evidenz, ob eine härtere Policy nötig ist).
    /// Belegfall: Run b4fb45, wo ein zweiter Write ein gutes Artefakt mit Testinhalt überschrieb.
    /// </summary>
    public void LogOverwriteDiagnosis(string agentName, string docRelPath, int finalLength)
    {
        var toolCallsPath = Path.Combine(_run.AgentsLogDir, agentName, "tool-calls.jsonl");
        if (!File.Exists(toolCallsPath))
            return;

        var fileName = Path.GetFileName(docRelPath);
        var writeCount = 0;

        foreach (var line in File.ReadLines(toolCallsPath))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                if (!root.TryGetProperty("type", out var t) || t.GetString() != "TOOL_CALL_STARTED")
                    continue;
                if (!root.TryGetProperty("tool", out var tool) || tool.GetString() != "fs_write")
                    continue;
                if (root.TryGetProperty("targetPath", out var tp)
                    && string.Equals(Path.GetFileName(tp.GetString()), fileName, StringComparison.Ordinal))
                    writeCount++;
            }
            catch (JsonException)
            {
                // Robust: einzelne unparsebare Zeile ignorieren.
            }
        }

        if (writeCount <= 1)
            return;

        var evt = new
        {
            type = "ARTIFACT_SUSPICIOUS_OVERWRITE",
            runId = _run.RunId,
            agentName,
            path = docRelPath,
            fsWriteCount = writeCount,
            finalLength,
            note = "Mehrfacher fs_write auf dasselbe Pflichtartefakt — finaler State entspricht dem letzten Write.",
            timestampUtc = DateTime.UtcNow
        };

        _run.AppendAgentStateAccess(agentName, evt);
        _run.AppendEvent(evt);
    }

    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max] + $"... <truncated len={value.Length}>";
}
