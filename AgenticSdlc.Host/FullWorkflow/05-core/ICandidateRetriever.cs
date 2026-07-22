using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Retrieval-Port (plan-core-ingestion §7). Stufe 0 = "alle zeigen". Spaetere Stufen (lexikalisch,
// Embeddings, Vector-DB) implementieren denselben Port -> Resolver/Gate/Apply bleiben unberuehrt.
public interface ICandidateRetriever
{
    IReadOnlyList<ProjectStateItem> GetCandidates(string incomingText, ProjectStateDocument core);
}
