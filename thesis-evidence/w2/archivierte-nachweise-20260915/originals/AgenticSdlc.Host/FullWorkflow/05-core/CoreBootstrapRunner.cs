using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// CLI: core-bootstrap-first-transcript <artifact.json>... [--l3-run <run|path>] [--project-id <id>]
//
// B1 (plan-core-bootstrap-first-transcript §4) — der Start-bei-null-Pfad: deterministische Orchestrierung mit
// Modus-Erkennung. KEIN neuer Fach-Mechanismus — verkettet die bestehenden Runner in der richtigen Reihenfolge:
//   0. Core existiert? -> Abbruch (Delta-Modus ist der Weg)   1. project-state-build   2. core-seed   3. core-baseline
// Danach folgt die agentische Backlog-Stufe (l4-re-clarify …) MIT ihren HumanReview-Gates — die wird hier nur klar
// angesagt, nicht auto-durchlaufen. `core-seed` hebt alle Requirements als NEW (frische Core-IDs) — kein Resolver noetig.
public static class CoreBootstrapRunner
{
    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        var artifacts = new List<string>();
        string? l3Run = null, projectId = null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--l3-run", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { l3Run = args[++i]; continue; }
            if (string.Equals(a, "--project-id", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { projectId = args[++i]; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[core-bootstrap] unbekanntes Argument: {a}"); Usage(); return 2; }
            artifacts.Add(a);
        }

        // 0. Modus-Erkennung — der eigentliche neue Punkt.
        var repo = new JsonCoreRepository(repoRoot);
        if (await repo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine($"[core-bootstrap] BOOTSTRAP_ABORTED_CORE_EXISTS: Core existiert bereits ({Path.GetRelativePath(repoRoot, CorePaths.CoreFile(repoRoot))}).");
            Console.Error.WriteLine("[core-bootstrap] Fuer weitere Transkripte den Delta-Modus nutzen: ingest-requirements <meeting-delta>. Bootstrap ist nur fuer den Start bei null.");
            return 1;
        }
        if (artifacts.Count == 0)
        {
            Console.Error.WriteLine("[core-bootstrap] mindestens ein L1-L3-artifact.json ist erforderlich.");
            Usage();
            return 2;
        }

        var run = new RunContext(RunId.New(), "bootstrap");
        Directory.CreateDirectory(run.RunDir);
        var psPath = Path.Combine(run.RunDir, "project-state.json");
        var baselineDir = Path.Combine(run.RunDir, "baseline");

        // 1. project-state-build (aus L1-L3-Artefakten / --l3-run) -> project-state.json in den Run-Ordner.
        Console.WriteLine("[core-bootstrap] 1/3 project-state-build ...");
        var psArgs = new List<string> { "project-state-build" };
        psArgs.AddRange(artifacts);
        if (l3Run is not null) { psArgs.Add("--l3-run"); psArgs.Add(l3Run); }
        psArgs.Add("--out"); psArgs.Add(psPath);
        if (projectId is not null) { psArgs.Add("--project-id"); psArgs.Add(projectId); }
        var rc1 = await ProjectStateBuildRunner.RunAsync(psArgs.ToArray(), repoRoot).ConfigureAwait(false);
        if (rc1 != 0) { Console.Error.WriteLine("[core-bootstrap] project-state-build fehlgeschlagen - Bootstrap abgebrochen."); return rc1; }

        // 2. core-seed -> initiale Projektwahrheit (Requirements/Arch, alle NEW).
        Console.WriteLine("[core-bootstrap] 2/3 core-seed ...");
        var rc2 = await CoreSeedRunner.RunAsync(["core-seed", psPath], repoRoot).ConfigureAwait(false);
        if (rc2 != 0) { Console.Error.WriteLine("[core-bootstrap] core-seed fehlgeschlagen - Bootstrap abgebrochen."); return rc2; }

        // 3. core-baseline -> L4-Baseline-Triplet fuer die Backlog-Stufe.
        Console.WriteLine("[core-bootstrap] 3/3 core-baseline ...");
        var rc3 = await CoreBaselineRunner.RunAsync(["core-baseline", "--out", baselineDir], repoRoot).ConfigureAwait(false);
        if (rc3 != 0) { Console.Error.WriteLine("[core-bootstrap] core-baseline fehlgeschlagen - Bootstrap abgebrochen."); return rc3; }

        // Validieren + klare Next-Step-Meldung.
        var core = await repo.LoadAsync().ConfigureAwait(false);
        var reqCount = core.Items.Count(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase));
        var baselineRel = Path.GetRelativePath(repoRoot, Path.Combine(baselineDir, "canonical-requirements-baseline.json"));

        Console.WriteLine($"[core-bootstrap] OK — initialer Core angelegt: items={core.Items.Count} (requirements={reqCount}).");
        Console.WriteLine("[core-bootstrap] Stufe 2 (agentisch, MIT HumanReview-Gates) — Backlog aufbauen:");
        Console.WriteLine($"[core-bootstrap]   l4-re-clarify cluster {baselineRel} <model>");
        Console.WriteLine("[core-bootstrap]   -> l4-re-clarify-review -> l4-re-clarify-apply");
        Console.WriteLine("[core-bootstrap]   -> l4-re-clarify clarify <l4-re-clarify-run>   (Cluster -> PBIs; erzeugt den Backlog-Lauf)");
        Console.WriteLine("[core-bootstrap]   -> l4-re-clarify-backlog-review -> l4-re-clarify-backlog-apply");
        Console.WriteLine("[core-bootstrap]   -> core-seed-backlog <l4-re-clarify-run>   (Features + PBIs in den Core)");
        Console.WriteLine("[core-bootstrap] Danach normaler Delta-Modus: ingest-requirements <meeting-delta>.");
        Console.WriteLine($"[core-bootstrap] -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static void Usage()
        => Console.Error.WriteLine("Usage: core-bootstrap-first-transcript <artifact.json>... [--l3-run <run|path>] [--project-id <id>]");
}
