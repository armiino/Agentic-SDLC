using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Adr;

// R-11 A5/U5 (06.08.) — die Review-CLI des adr-gate (Klon des arch-classify-review-Musters):
// liest die vom Finalize persistierte Anfrage (adr-request.json), öffnet die U5-Abnahme-UI und schreibt
// adr-decisions.json — die liest der pipeline-full-Resume und beantwortet damit den Port.
// CLI: adr-review <fullworkflow-run|07-adr-dir> [--no-browser]
public static class AdrReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    public static async Task<int> RunAsync(string[] args, Configuration.HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: adr-review <fullworkflow-run|07-adr-dir> [--no-browser]");
            return 2;
        }

        var dir = ResolveDir(repoRoot, args[1]);
        var requestPath = dir is null ? null : Path.Combine(dir, "adr-request.json");
        if (requestPath is null || !File.Exists(requestPath))
        {
            Console.Error.WriteLine($"[adr-review] adr-request.json nicht gefunden fuer '{args[1]}' — pausiert der Lauf am adr-gate?");
            return 2;
        }

        var request = JsonSerializer.Deserialize<AdrReviewRequest>(await File.ReadAllTextAsync(requestPath).ConfigureAwait(false), Json)
                      ?? throw new InvalidOperationException($"Anfrage nicht lesbar: {requestPath}");
        var decisionsPath = Path.Combine(dir!, "adr-decisions.json");

        var session = AdrReviewAdapter.BuildSession(request.RunId, request);
        if (File.Exists(decisionsPath))
        {
            var existing = JsonSerializer.Deserialize<AdrDecisionsFile>(await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), Json);
            AdrReviewAdapter.MergeExistingDecisions(session, existing);
        }

        // U5-Feinschliff (06.08.): Katalog-Details rechts — Core LIVE + read-only (Muster arch-classify-review).
        var coreRepo = new FullWorkflow.Core.JsonCoreRepository(repoRoot);
        var core = await coreRepo.ExistsAsync().ConfigureAwait(false) ? await coreRepo.LoadAsync().ConfigureAwait(false) : null;

        var (file, outcome) = await ReviewUiFlow.RunAsync(
            session, decisionsPath,
            AdrReviewAdapter.Resolved,
            resolveContext: (_, key) => Task.FromResult(AdrReviewAdapter.ResolvePreview(key, request)),
            apply: s => AdrReviewAdapter.Apply(request.RunId, s),
            openBrowser: !args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase),
            resolveReference: core is null ? null
                : id => Task.FromResult(AdrReviewAdapter.BuildTruthReference(core, id)),
            resolveReferenceContext: core is null ? null
                : (id, key) => Task.FromResult(AdrReviewAdapter.ResolveTruthReferenceContext(core, id, key))).ConfigureAwait(false);
        // 1c-② Teil-Fertig: kommt DEKLARATIV aus der Session (AllowPartialFinish im Adapter) — EINE Quelle
        // für Server-Erlaubnis UND Fertig-Knopf; ein „Alle vertagen"-Bulk wäre identisch mit Fertig-Klicken.

        Console.WriteLine($"[adr-review] {outcome}: {file.Decisions.Count} freigegeben, {request.Items.Count - file.Decisions.Count} vertagt -> {Path.GetRelativePath(repoRoot, decisionsPath)}");
        // 1g-C-Endform (19.08., Zwei-Bahnen-Regel — Autor-Fund „ketten den Resume nicht selbst?"):
        // „Fertig" kettet die Fortsetzung IM RUNNER (Haus-Muster 3b-②/R-43) — damit verhalten sich CLI-
        // und Steward-Bahn identisch. Wachen: --no-resume (Inspektions-Opt-out) · Cancelled ⇒ Pause bleibt ·
        // KEIN Pause-Zeiger (z. B. Sandbox-Verzeichnis) ⇒ nichts zu ketten.
        var pointer = Path.Combine(repoRoot, "runs", "fullworkflow", request.RunId, "checkpoints", "pointer.json");
        if (args.Contains("--no-resume", StringComparer.OrdinalIgnoreCase)
            || outcome != AgenticSdlc.HumanReview.ReviewOutcome.Finished
            || !File.Exists(pointer))
        {
            Console.WriteLine($"[adr-review] Weiter: pipeline-full resume {request.RunId}");
            return 0;
        }
        Console.WriteLine($"[adr-review] R-43: resume {request.RunId} laeuft automatisch an (Folgestufen inkl. LLM) …");
        return await Pipeline.PipelineFullRunner.RunAsync(["pipeline-full", "resume", request.RunId], settings, repoRoot, null).ConfigureAwait(false);
    }

    private static string? ResolveDir(string repoRoot, string token)
    {
        var direct = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (File.Exists(Path.Combine(direct, "adr-request.json"))) return direct;
        var root = Path.Combine(repoRoot, "runs", "fullworkflow");
        if (!Directory.Exists(root)) return null;
        return Directory.EnumerateDirectories(root)
            .Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase))
            .Select(d => Path.Combine(d, "07-adr"))
            .FirstOrDefault(d => File.Exists(Path.Combine(d, "adr-request.json")));
    }
}
