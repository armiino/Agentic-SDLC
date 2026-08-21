using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.ArchClassify;

// R-11 A2-3 (06.08.) — die Review-CLI des arch-classify-gate (Klon des E0.8-Musters DecisionGateReviewRunner):
// liest die vom Finalize persistierte Anfrage (arch-classify-request.json), öffnet die U2-Edit-UI und schreibt
// classify-decisions.json — die liest der pipeline-full-Resume und beantwortet damit den Port.
// CLI: arch-classify-review <fullworkflow-run|07-arch-classify-dir> [--no-browser]
public static class ArchClassifyReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    public static async Task<int> RunAsync(string[] args, Configuration.HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: arch-classify-review <fullworkflow-run|07-arch-classify-dir> [--no-browser]");
            return 2;
        }

        var dir = ResolveDir(repoRoot, args[1]);
        var requestPath = dir is null ? null : Path.Combine(dir, "arch-classify-request.json");
        if (requestPath is null || !File.Exists(requestPath))
        {
            Console.Error.WriteLine($"[arch-classify-review] arch-classify-request.json nicht gefunden fuer '{args[1]}' — pausiert der Lauf am arch-classify-gate?");
            return 2;
        }

        var request = JsonSerializer.Deserialize<ArchClassifyReviewRequest>(await File.ReadAllTextAsync(requestPath).ConfigureAwait(false), Json)
                      ?? throw new InvalidOperationException($"Anfrage nicht lesbar: {requestPath}");
        var decisionsPath = Path.Combine(dir!, "classify-decisions.json");

        var session = ArchClassifyReviewAdapter.BuildSession(request.RunId, request);
        if (File.Exists(decisionsPath))
        {
            var existing = JsonSerializer.Deserialize<ArchClassifyDecisionsFile>(await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), Json);
            ArchClassifyReviewAdapter.MergeExistingDecisions(session, existing);
        }

        // U2v2: PBI-Detail-Panel rechts — Core LIVE + read-only laden (Muster decision-gate-review).
        var coreRepo = new Core.JsonCoreRepository(repoRoot);
        var core = await coreRepo.ExistsAsync().ConfigureAwait(false) ? await coreRepo.LoadAsync().ConfigureAwait(false) : null;

        var (file, outcome) = await ReviewUiFlow.RunAsync(
            session, decisionsPath,
            ArchClassifyReviewAdapter.Resolved,
            resolveContext: null,
            apply: s => ArchClassifyReviewAdapter.Apply(request.RunId, s),
            openBrowser: !args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase),
            resolveReference: core is null ? null
                : id => Task.FromResult(ArchClassifyReviewAdapter.BuildPbiReference(core, id)),
            resolveReferenceContext: core is null ? null
                : (id, key) => Task.FromResult(ArchClassifyReviewAdapter.ResolvePbiReferenceContext(core, id, key))).ConfigureAwait(false);

        Console.WriteLine($"[arch-classify-review] {outcome}: {file.Decisions.Count} klassifiziert, {request.Items.Count - file.Decisions.Count} vertagt -> {Path.GetRelativePath(repoRoot, decisionsPath)}");
        // 1g-C-Endform (19.08., Zwei-Bahnen-Regel — Autor-Fund „ketten den Resume nicht selbst?"):
        // „Fertig" kettet die Fortsetzung IM RUNNER (Haus-Muster 3b-②/R-43) — damit verhalten sich CLI-
        // und Steward-Bahn identisch. Wachen: --no-resume (Inspektions-Opt-out) · Cancelled ⇒ Pause bleibt ·
        // KEIN Pause-Zeiger (z. B. Sandbox-Verzeichnis) ⇒ nichts zu ketten.
        var pointer = Path.Combine(repoRoot, "runs", "fullworkflow", request.RunId, "checkpoints", "pointer.json");
        if (args.Contains("--no-resume", StringComparer.OrdinalIgnoreCase)
            || outcome != AgenticSdlc.HumanReview.ReviewOutcome.Finished
            || !File.Exists(pointer))
        {
            Console.WriteLine($"[arch-classify-review] Weiter: pipeline-full resume {request.RunId}");
            return 0;
        }
        Console.WriteLine($"[arch-classify-review] R-43: resume {request.RunId} laeuft automatisch an (Folgestufen inkl. LLM) …");
        return await Pipeline.PipelineFullRunner.RunAsync(["pipeline-full", "resume", request.RunId], settings, repoRoot, null).ConfigureAwait(false);
    }

    private static string? ResolveDir(string repoRoot, string token)
    {
        var direct = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (File.Exists(Path.Combine(direct, "arch-classify-request.json"))) return direct;
        var root = Path.Combine(repoRoot, "runs", "fullworkflow");
        if (!Directory.Exists(root)) return null;
        return Directory.EnumerateDirectories(root)
            .Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase))
            .Select(d => Path.Combine(d, "07-arch-classify"))
            .FirstOrDefault(d => File.Exists(Path.Combine(d, "arch-classify-request.json")));
    }
}
