using System.Text.Json.Serialization;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

/// <summary>
/// Reflect-Sub-Workflow (NEEDS_REVISION, §5.3/§9): der REVISE-Agent überarbeitet je Kandidat den vorigen Entwurf anhand
/// des menschlichen Feedbacks (Self-Refine: y_t + fb_t → y_{t+1}). Danach läuft der revidierte Kandidat durch die
/// wiederverwendete Pipeline (AnchorResolve → Validate → Judge → Routing → Finalize) und wird NEU klassifiziert. Die
/// Schranke (max Revisionen) prüft der Runner VOR diesem Knoten; hier kommen nur Kandidaten an, die revidiert werden dürfen.
/// </summary>
[SendsMessage(typeof(L3Candidates))]
internal sealed class L3ReviseExecutor(AIAgent agent, RunContext run) : Executor<L3ReviseSet>("L3-Revise")
{
    public override async ValueTask HandleAsync(L3ReviseSet msg, IWorkflowContext context, CancellationToken ct = default)
    {
        var revised = new List<L3Candidate>(msg.Items.Count);
        if (msg.Items.Count > 0)
        {
            var response = await agent.RunAsync([new ChatMessage(ChatRole.User, L3Prompts.BuildReviseUser(msg.Env, msg.Items))], cancellationToken: ct).ConfigureAwait(false);
            var byId = (L3Json.Deserialize<RawRevisions>(response.Text)?.Revisions ?? [])
                .Where(r => !string.IsNullOrWhiteSpace(r.CandidateId))
                .ToDictionary(r => r.CandidateId!, r => r, StringComparer.Ordinal);

            foreach (var item in msg.Items)
            {
                var prev = item.PrevCandidate;
                var newId = $"{prev.CandidateId}-rev{item.PriorRevisions + 1}";
                if (byId.TryGetValue(prev.CandidateId, out var r) && !string.IsNullOrWhiteSpace(r.Text))
                    revised.Add(new L3Candidate(newId, prev.TargetType, r.Text!.Trim(), r.Rationale ?? prev.Rationale, r.Assumptions ?? prev.Assumptions));
                else
                    // Keine Revision vom Agenten → vorigen Text behalten (wird erneut klassifiziert; landet i. d. R. wieder beim Menschen).
                    revised.Add(prev with { CandidateId = newId });
            }
        }
        run.AppendEvent(new { type = "L3_REVISED", runId = run.RunId, count = revised.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new L3Candidates(msg.Env, revised)).ConfigureAwait(false);
    }

    private sealed record RawRevisions([property: JsonPropertyName("revisions")] IReadOnlyList<RawRevision>? Revisions);
    private sealed record RawRevision(
        [property: JsonPropertyName("candidateId")] string? CandidateId,
        [property: JsonPropertyName("text")] string? Text,
        [property: JsonPropertyName("rationale")] string? Rationale,
        [property: JsonPropertyName("assumptions")] IReadOnlyList<string>? Assumptions);
}
