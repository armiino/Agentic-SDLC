
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenTelemetry.Trace;

namespace AgenticSdlc.Host.Observability;

public sealed class ToolCallLoggerMiddleware
{
    private static readonly ActivitySource ActivitySource = new("AgenticSdlc.Host");
    private readonly RunContext _run;
    private int _toolStep = 0;

    public ToolCallLoggerMiddleware(RunContext run) => _run = run;

    public async ValueTask<object?> InvokeAsync(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        var toolName = context.Function?.Name ?? "unknown_tool";
        TryGetStringArgument(context.Arguments, "path", out var path);
        TryGetStringArgument(context.Arguments, "content", out var newContent);
        TryGetStringArgument(context.Arguments, "intent", out var intent);
        TryGetStringArgument(context.Arguments, "reason", out var reason);
        TryGetStringArgument(context.Arguments, "evidence", out var evidence);

        var normalizedPath = string.IsNullOrWhiteSpace(path) ? null : NormalizePath(path!);
        var normalizedIntent = string.IsNullOrWhiteSpace(intent) ? null : NormalizeDecisionText(intent!);
        var normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : NormalizeDecisionText(reason!);
        var normalizedEvidence = string.IsNullOrWhiteSpace(evidence) ? null : NormalizeDecisionText(evidence!);
        var rationaleComplete = HasCompleteRationale(normalizedIntent, normalizedReason, normalizedEvidence);
        var step = Interlocked.Increment(ref _toolStep);

        using var span = ActivitySource.StartActivity($"tool.{toolName}", ActivityKind.Internal);

        span?.SetTag("operation", "tool.invoke");
        span?.SetTag("tool.name", toolName);
        span?.SetTag("agent.name", agent?.Name);
        span?.SetTag("run.id", _run.RunId);
        span?.SetTag("agent.iteration", step);
        span?.SetTag("agent.step.kind", "tool_execution");

        if (!string.IsNullOrWhiteSpace(normalizedPath))
            span?.SetTag("target.path", normalizedPath);

        if (toolName.Equals("fs_write", StringComparison.OrdinalIgnoreCase))
        {
            // erweiterung: Die vom Modell deklarierte Write-Absicht wird an den Span gehängt, damit OTel und JSONL dieselbe Handlung erklären.
            span?.SetTag("fs.write.intent", normalizedIntent);
            span?.SetTag("fs.write.reason", normalizedReason);
            span?.SetTag("fs.write.evidence", normalizedEvidence);
            // erweiterung: Vollständigkeit wird separat getrackt, damit fehlende Gründe nicht mehr den Write blockieren.
            span?.SetTag("fs.write.rationale_complete", rationaleComplete);
        }

        FileState? beforeState = null;
        if (toolName.Equals("fs_write", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(normalizedPath))
        {
            beforeState = TryReadFileState(normalizedPath!);
            SetBeforeTags(span, beforeState);
        }

        _run.AppendEvent(new
        {
            type = "TOOL_CALL_STARTED",
            tool = toolName,
            targetPath = normalizedPath,
            arguments = RedactArguments(context.Arguments),
            toolStep = step,
            timestampUtc = DateTime.UtcNow
        });

        try
        {
            var result = await next(context, cancellationToken).ConfigureAwait(false);
            var toolResultError = TryGetToolResultError(result);

            if (toolResultError is null)
            {
                span?.SetTag("outcome", "success");
                span?.SetStatus(ActivityStatusCode.Ok);
            }
            else
            {
                // erweiterung: MCP kann Fehler als Tool-Ergebnis zurückgeben, ohne eine Exception zu werfen.
                // erweiterung: Solche Ergebnisfehler werden als eigener Outcome markiert, damit TOOL_CALL_FINISHED nicht wie ein Erfolg wirkt.
                span?.SetTag("outcome", "error_result");
                span?.SetTag("reason.code", "MCP_TOOL_RESULT_ERROR");
                span?.SetTag("tool.result.is_error", true);
                span?.SetTag("tool.result.error", toolResultError);
                span?.SetStatus(ActivityStatusCode.Error, toolResultError);
            }

            _run.AppendEvent(new
            {
                type = "TOOL_CALL_FINISHED",
                tool = toolName,
                targetPath = normalizedPath,
                result = RedactResult(result),
                outcome = toolResultError is null ? "success" : "error_result",
                reasonCode = toolResultError is null ? null : "MCP_TOOL_RESULT_ERROR",
                toolResultError,
                toolStep = step,
                timestampUtc = DateTime.UtcNow
            });

            if (toolResultError is not null)
            {
                _run.AppendEvent(new
                {
                    type = "TOOL_CALL_RETURNED_ERROR",
                    tool = toolName,
                    targetPath = normalizedPath,
                    toolStep = step,
                    outcome = "error_result",
                    reasonCode = "MCP_TOOL_RESULT_ERROR",
                    error = toolResultError,
                    timestampUtc = DateTime.UtcNow
                });
            }

            if (toolName.Equals("fs_write", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(normalizedPath))
            {
                var afterState = TryReadFileState(normalizedPath!);
                SetAfterTags(span, afterState);

                var effect = ComputeWriteEffect(beforeState, afterState);
                span?.SetTag("fs.write_effect", effect);

                string? diffPath = null;
                DiffSummary? summary = null;

                if (beforeState?.Content is not null && afterState?.Content is not null)
                {
                    summary = BuildDiffSummary(beforeState.Content, afterState.Content);
                    if (summary.HasChanges)
                    {
                        var diffText = BuildUnifiedLikeDiff(normalizedPath!, beforeState.Content, afterState.Content);
                        diffPath = _run.WriteDiffFile(normalizedPath!, diffText);
                    }
                }
                else if (beforeState is null && afterState?.Content is not null)
                {
                    summary = BuildDiffSummary("", afterState.Content);
                    var diffText = BuildUnifiedLikeDiff(normalizedPath!, "", afterState.Content);
                    diffPath = _run.WriteDiffFile(normalizedPath!, diffText);
                }

                if (summary is not null)
                {
                    span?.SetTag("diff.changed", summary.HasChanges);
                    span?.SetTag("diff.lines_added", summary.LinesAdded);
                    span?.SetTag("diff.lines_removed", summary.LinesRemoved);
                    span?.SetTag("diff.changed_lines", summary.ChangedLines);
                }

                if (!string.IsNullOrWhiteSpace(diffPath))
                    span?.SetTag("diff.file", diffPath);

                _run.AppendEvent(new
                {
                    type = "FILE_WRITE_ANALYZED",
                    path = normalizedPath,
                    toolStep = step,
                    writeEffect = effect,
                    // erweiterung: Diese Felder erklären den Side-Effect aus Sicht des Modells und bleiben mit Hash/Diff prüfbar.
                    declaredRationale = new
                    {
                        intent = normalizedIntent,
                        reason = normalizedReason,
                        evidence = normalizedEvidence,
                        // erweiterung: Dieser Wert zeigt, ob der Agent den erklärungspflichtigen Write-Contract vollständig eingehalten hat.
                        complete = rationaleComplete
                    },
                    before = beforeState is null ? null : new
                    {
                        exists = true,
                        size = beforeState.Size,
                        sha256 = beforeState.Sha256
                    },
                    after = afterState is null ? null : new
                    {
                        exists = true,
                        size = afterState.Size,
                        sha256 = afterState.Sha256
                    },
                    diff = summary is null ? null : new
                    {
                        changed = summary.HasChanges,
                        linesAdded = summary.LinesAdded,
                        linesRemoved = summary.LinesRemoved,
                        changedLines = summary.ChangedLines,
                        file = diffPath
                    },
                    timestampUtc = DateTime.UtcNow
                });

                _run.AppendDecision(new
                {
                    type = "WRITE_DECISION_RECORDED",
                    runId = _run.RunId,
                    agentName = agent?.Name,
                    toolStep = step,
                    path = normalizedPath,
                    // erweiterung: intent/reason/evidence sind model-declared rationale, nicht die garantierte interne Ursache.
                    intent = normalizedIntent,
                    reason = normalizedReason,
                    evidence = normalizedEvidence,
                    // erweiterung: Fehlende rationale-Felder sind jetzt ein auswertbarer Befund statt ein harter Tool-Abbruch.
                    rationaleComplete,
                    writeEffect = effect,
                    before = beforeState is null ? null : new
                    {
                        exists = true,
                        size = beforeState.Size,
                        sha256 = beforeState.Sha256
                    },
                    after = afterState is null ? null : new
                    {
                        exists = true,
                        size = afterState.Size,
                        sha256 = afterState.Sha256
                    },
                    diff = summary is null ? null : new
                    {
                        changed = summary.HasChanges,
                        linesAdded = summary.LinesAdded,
                        linesRemoved = summary.LinesRemoved,
                        changedLines = summary.ChangedLines,
                        file = diffPath
                    },
                    timestampUtc = DateTime.UtcNow
                });
            }

            if (!string.IsNullOrWhiteSpace(normalizedPath) &&
                (toolName.Equals("fs_write", StringComparison.OrdinalIgnoreCase) ||
                 toolName.Equals("fs_read", StringComparison.OrdinalIgnoreCase) ||
                 toolName.Equals("fs_list", StringComparison.OrdinalIgnoreCase) ||
                 toolName.Equals("fs_exists", StringComparison.OrdinalIgnoreCase)))
            {
                _run.AppendEvent(new
                {
                    type = "ARTIFACT_TOUCHED",
                    op = toolName,
                    path = normalizedPath,
                    toolStep = step,
                    timestampUtc = DateTime.UtcNow
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            var outcome = MapOutcome(ex, out var reasonCode);

            span?.SetTag("outcome", outcome);
            span?.SetTag("reason.code", reasonCode);
            if (span is not null)
            {
                span.AddException(ex);
                span.SetStatus(ActivityStatusCode.Error, ex.Message);
            }

            _run.AppendEvent(new
            {
                type = "TOOL_CALL_FAILED",
                tool = toolName,
                targetPath = normalizedPath,
                toolStep = step,
                outcome,
                reasonCode,
                errorType = ex.GetType().FullName,
                error = ex.Message,
                timestampUtc = DateTime.UtcNow
            });

            throw;
        }
    }

    private static void SetBeforeTags(Activity? span, FileState? state)
    {
        span?.SetTag("fs.exists_before", state is not null);
        if (state is null) return;
        span?.SetTag("fs.size_before", state.Size);
        span?.SetTag("fs.sha256_before", state.Sha256);
    }

    private static void SetAfterTags(Activity? span, FileState? state)
    {
        span?.SetTag("fs.exists_after", state is not null);
        if (state is null) return;
        span?.SetTag("fs.size_after", state.Size);
        span?.SetTag("fs.sha256_after", state.Sha256);
    }

    private static string ComputeWriteEffect(FileState? before, FileState? after)
    {
        if (before is null && after is not null) return "create";
        if (before is not null && after is not null && before.Sha256 == after.Sha256) return "no_change";
        if (before is not null && after is not null) return "overwrite";
        return "unknown";
    }

    private static FileState? TryReadFileState(string relativePath)
    {
        var full = Path.GetFullPath(relativePath);
        if (!File.Exists(full)) return null;

        var content = File.ReadAllText(full, Encoding.UTF8);
        var bytes = Encoding.UTF8.GetBytes(content);
        return new FileState(
            content,
            bytes.Length,
            Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
        );
    }

    private static DiffSummary BuildDiffSummary(string before, string after)
    {
        var a = SplitLines(before);
        var b = SplitLines(after);

        var max = Math.Max(a.Length, b.Length);
        var added = 0;
        var removed = 0;
        var changed = 0;

        for (var i = 0; i < max; i++)
        {
            var hasA = i < a.Length;
            var hasB = i < b.Length;

            if (hasA && !hasB)
            {
                removed++;
                continue;
            }

            if (!hasA && hasB)
            {
                added++;
                continue;
            }

            if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                changed++;
        }

        return new DiffSummary(
            HasChanges: added > 0 || removed > 0 || changed > 0,
            LinesAdded: added,
            LinesRemoved: removed,
            ChangedLines: changed
        );
    }

    private static string BuildUnifiedLikeDiff(string path, string before, string after)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"--- before/{path}");
        sb.AppendLine($"+++ after/{path}");

        var a = SplitLines(before);
        var b = SplitLines(after);
        var max = Math.Max(a.Length, b.Length);

        for (var i = 0; i < max; i++)
        {
            var hasA = i < a.Length;
            var hasB = i < b.Length;

            if (hasA && hasB && string.Equals(a[i], b[i], StringComparison.Ordinal))
                continue;

            if (hasA) sb.AppendLine($"- {a[i]}");
            if (hasB) sb.AppendLine($"+ {b[i]}");
        }

        return sb.ToString();
    }

    private static string[] SplitLines(string text)
        => text.Replace("\r\n", "\n").Split('\n');

    private static object? RedactResult(object? result)
    {
        return result switch
        {
            null => null,
            string s when s.Length > 200 => $"<string len={s.Length}>",
            string s => s,
            JsonElement json => RedactJsonElement(json),
            Array a => $"<array len={a.Length}>",
            _ => $"<{result.GetType().Name}>"
        };
    }

    private static string? TryGetToolResultError(object? result)
    {
        // erweiterung: MCP Toolfehler kommen häufig als JSON-Ergebnis mit isError=true statt als Exception.
        if (result is not JsonElement json ||
            json.ValueKind != JsonValueKind.Object ||
            !json.TryGetProperty("isError", out var isError) ||
            isError.ValueKind != JsonValueKind.True)
        {
            return null;
        }

        return TryExtractTextFromToolResult(json) ?? "MCP tool returned isError=true.";
    }

    private static object RedactJsonElement(JsonElement json)
    {
        // erweiterung: Fehlerhafte Tool-Ergebnisse werden sichtbar gemacht, normale JSON-Ergebnisse bleiben kurz.
        if (json.ValueKind == JsonValueKind.Object &&
            json.TryGetProperty("isError", out var isError) &&
            isError.ValueKind == JsonValueKind.True)
        {
            return new
            {
                isError = true,
                text = TryExtractTextFromToolResult(json)
            };
        }

        return "<JsonElement>";
    }

    private static string? TryExtractTextFromToolResult(JsonElement json)
    {
        if (!json.TryGetProperty("content", out var content) ||
            content.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var item in content.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Object &&
                item.TryGetProperty("text", out var text) &&
                text.ValueKind == JsonValueKind.String)
            {
                return text.GetString();
            }
        }

        return null;
    }

    private static object? RedactArguments(object? args)
    {
        if (args is null) return null;

        if (args is IDictionary<string, object?> d1)
        {
            var safe = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in d1)
            {
                if (k.Equals("content", StringComparison.OrdinalIgnoreCase) && v is not null)
                {
                    var s = v.ToString() ?? "";
                    safe[k] = $"<redacted len={s.Length}>";
                }
                else
                {
                    safe[k] = v?.ToString();
                }
            }
            return safe;
        }

        return $"<{args.GetType().Name}>";
    }

    private static string MapOutcome(Exception ex, out string reasonCode)
    {
        if (ex is UnauthorizedAccessException)
        {
            reasonCode = "POLICY_DENIED";
            return "denied";
        }

        if (ex is OperationCanceledException)
        {
            reasonCode = "CANCELLED";
            return "cancelled";
        }

        reasonCode = "TOOL_ERROR";
        return "error";
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/').Trim();

    private static string NormalizeDecisionText(string value)
    {
        // erweiterung: Begründungen werden einzeilig normalisiert, damit decision-log.jsonl stabil grepbar und vergleichbar bleibt.
        return value.Replace("\r\n", " ").Replace('\n', ' ').Trim();
    }

    private static bool HasCompleteRationale(string? intent, string? reason, string? evidence)
    {
        // erweiterung: Die drei Felder bilden zusammen die model-declared rationale fürr den Side-Effect.
        return !string.IsNullOrWhiteSpace(intent) &&
               !string.IsNullOrWhiteSpace(reason) &&
               !string.IsNullOrWhiteSpace(evidence);
    }

    private static bool TryGetStringArgument(object? args, string key, out string? value)
    {
        value = null;
        if (args is null) return false;

        if (args is IDictionary<string, object?> dictObj &&
            dictObj.TryGetValue(key, out var raw) &&
            raw is not null)
        {
            value = raw.ToString();
            return !string.IsNullOrWhiteSpace(value);
        }

        if (args is IDictionary<string, object> dict &&
            dict.TryGetValue(key, out var raw2) &&
            raw2 is not null)
        {
            value = raw2.ToString();
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }

    private sealed record FileState(string Content, int Size, string Sha256);
    private sealed record DiffSummary(bool HasChanges, int LinesAdded, int LinesRemoved, int ChangedLines);
}
