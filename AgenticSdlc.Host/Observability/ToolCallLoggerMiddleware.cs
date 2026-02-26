using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Observability;

/*
 * Logging umgubung eingebaut um in den LOGS besser nachvollziehen zu können
 * was ist wann gestartet usw.
 * Telemetry logs etc wären auch ne option aber fürs erste interessiert mich
 * Was und wann und keine deeperen Infos
 * für weitere Forschung
 * https://learn.microsoft.com/de-de/agent-framework/agents/observability?pivots=programming-language-csharp
 */
public sealed class ToolCallLoggerMiddleware
{
    private readonly RunContext _run;

    public ToolCallLoggerMiddleware(RunContext run) => _run = run;

    public async ValueTask<object?> InvokeAsync(
        AIAgent agent,
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        _run.AppendEvent(new
        {
            type = "TOOL_CALL_STARTED",
            tool = context.Function?.Name,
            arguments = context.Arguments,
            timestampUtc = DateTime.UtcNow
        });

        try
        {
            var result = await next(context, cancellationToken).ConfigureAwait(false);

            _run.AppendEvent(new
            {
                type = "TOOL_CALL_FINISHED",
                tool = context.Function?.Name,
                result,
                timestampUtc = DateTime.UtcNow
            });

            //wenn etwas file geschrieben - log eintrag mit DOC_WRITTEN
            if (string.Equals(context.Function?.Name, "fs_write", StringComparison.OrdinalIgnoreCase))
            {
                if (TryGetStringArgument(context.Arguments, "path", out var path) &&
                    path.StartsWith("docs/", StringComparison.Ordinal))
                {
                    _run.AppendEvent(new
                    {
                        type = "DOC_WRITTEN",
                        path,
                        timestampUtc = DateTime.UtcNow
                    });
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _run.AppendEvent(new
            {
                type = "TOOL_CALL_FAILED",
                tool = context.Function?.Name,
                error = ex.Message,
                timestampUtc = DateTime.UtcNow
            });
            throw;
        }
    }
    private static bool TryGetStringArgument(object? args, string key, out string value)
    {
        value = string.Empty;

        if (args is null)
            return false;

        if (args is IDictionary<string, object?> dictObj &&
            dictObj.TryGetValue(key, out var raw) &&
            raw is not null)
        {
            value = raw.ToString() ?? string.Empty;
            return value.Length > 0;
        }

        if (args is IDictionary<string, object> dict &&
            dict.TryGetValue(key, out var raw2) &&
            raw2 is not null)
        {
            value = raw2.ToString() ?? string.Empty;
            return value.Length > 0;
        }

        return false;
    }
}