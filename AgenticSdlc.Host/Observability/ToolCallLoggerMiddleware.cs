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
}