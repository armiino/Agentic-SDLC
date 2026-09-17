namespace AgenticSdlc.Host.FullWorkflow.Core;

// Der Core (die lebende Projektwahrheit) liegt an EINER festen, lauf-unabhaengigen Stelle - bewusst
// ausserhalb runs/. JSON jetzt, DB spaeter (hinter ICoreRepository, gleicher Ort/Scope-Begriff).
public static class CorePaths
{
    public static string CoreDir(string repoRoot) => Path.Combine(repoRoot, "state", "core");

    public static string CoreFile(string repoRoot) => Path.Combine(CoreDir(repoRoot), "project-state.json");
}
