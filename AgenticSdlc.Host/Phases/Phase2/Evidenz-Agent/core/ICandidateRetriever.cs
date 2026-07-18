using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// Retrieval-Port (plan-core-ingestion §7). Stufe 0 = "alle zeigen". Spaetere Stufen (lexikalisch,
// Embeddings, Vector-DB) implementieren denselben Port -> Resolver/Gate/Apply bleiben unberuehrt.
public interface ICandidateRetriever
{
    IReadOnlyList<ProjectStateItem> GetCandidates(string incomingText, ProjectStateDocument core);
}
