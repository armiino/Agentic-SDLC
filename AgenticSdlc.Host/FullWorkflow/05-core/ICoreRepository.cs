using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// Schreib-/Lese-Port des Core. JSON jetzt (JsonCoreRepository), DB spaeter (gleicher Port).
// Bewusst der EINZIGE Schreibweg in den Core (kein File.Write daneben) -> DB-tauschbar (IST-Zustand §6/§7).
public interface ICoreRepository
{
    Task<bool> ExistsAsync(CancellationToken ct = default);

    Task<ProjectStateDocument> LoadAsync(CancellationToken ct = default);

    Task SaveAsync(ProjectStateDocument core, CancellationToken ct = default);
}
