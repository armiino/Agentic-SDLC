using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// CLI: decision-unblock-test [--out <file>]
//
// T2.3 — Kreis-Test (Tor 2 → Tor 3): blockierte Arbeit fliesst nach der Auflösung. Ein synthetischer In-Memory-Core
// (kein echter Core, kein LLM, kein GitHub) mit einem durch eine Open Decision blockierten PBI wird durch die ECHTEN
// Bausteine gefahren: GithubForwardSeed (VOR) → DecisionResolutionApply (Tor 2) → GithubForwardSeed (NACH).
// Beweis: VOR = HOLD_BLOCKED, NACH = UPDATE (gemappt) bzw. unmapped/CREATE-Kandidat (nicht gemappt). Exit 0 = PASS.
public static class DecisionUnblockTest
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        string? outArg = null;
        for (var i = 1; i < args.Length; i++)
            if (string.Equals(args[i], "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) outArg = args[++i];

        var core = BuildScenario();
        // Snapshot: Issue #77 offen (Mapping-Ziel von PBI-1).
        IReadOnlyList<GithubIssueSnapshot> issues =
            [new(77, "u/77", "PBI-1 Issue", "x", "open", [], null, DateTime.UtcNow)];

        // ---- VOR: Forward-Seed ----
        var before = SeedFor(core, issues);
        var holdBefore = before.DeterministicOps.Where(o => o.Kind == GithubForwardKind.HoldBlocked).Select(o => o.PbiId).ToHashSet(StringComparer.Ordinal);

        // ---- Tor 2: Decision auflösen (KEEP_ORIGINAL, deterministisch über die echte Kette) ----
        var input = new DecisionResolutionInput([new DecisionResolutionRequest("DEC-1", DecisionOutcome.KeepOriginal, null, "Stakeholder: Original bleibt.")]);
        var (ops, _) = DecisionResolutionDerivation.Derive(core, input);
        var plan = new DecisionResolutionPlanDocument(DecisionResolutionPlanDocument.CurrentSchemaVersion, "circle-test", DateTime.UtcNow, ops);
        var (coreAfter, applyReport) = DecisionResolutionApply.Apply(core, plan, new HashSet<int> { 0 }, "circle-test");

        // ---- NACH: Forward-Seed auf dem aufgelösten Core ----
        var after = SeedFor(coreAfter, issues);
        var holdAfter = after.DeterministicOps.Where(o => o.Kind == GithubForwardKind.HoldBlocked).Select(o => o.PbiId).ToHashSet(StringComparer.Ordinal);
        var updateAfter = after.DeterministicOps.Where(o => o.Kind == GithubForwardKind.UpdateIssue).Select(o => o.PbiId).ToHashSet(StringComparer.Ordinal);
        var unmappedAfter = after.UnmappedPbis.Select(e => e.PbiId).ToHashSet(StringComparer.Ordinal);

        // Erwartung: VOR beide HOLD; NACH keiner HOLD; PBI-1 -> UPDATE (gemappt), PBI-2 -> unmapped (CREATE-Kandidat).
        var pass =
            holdBefore.SetEquals(new[] { "PBI-1", "PBI-2" }) &&
            holdAfter.Count == 0 &&
            updateAfter.Contains("PBI-1") &&
            unmappedAfter.Contains("PBI-2") &&
            applyReport.Resolved.Contains("DEC-1") && applyReport.UnblockedPbis.Count == 2;

        var report = new
        {
            before = new { hold = holdBefore.OrderBy(x => x).ToArray() },
            tor2 = new { resolved = applyReport.Resolved, unblocked = applyReport.UnblockedPbis },
            after = new { hold = holdAfter.OrderBy(x => x).ToArray(), update = updateAfter.OrderBy(x => x).ToArray(), unmappedCreateCandidates = unmappedAfter.OrderBy(x => x).ToArray() },
            result = pass ? "PASS" : "FAIL",
            timestampUtc = DateTime.UtcNow
        };

        Console.WriteLine($"[decision-unblock-test] VOR: HOLD_BLOCKED = {string.Join(", ", holdBefore.OrderBy(x => x))}");
        Console.WriteLine($"[decision-unblock-test] Tor 2: resolved={string.Join(",", applyReport.Resolved)} unblocked={string.Join(",", applyReport.UnblockedPbis)}");
        Console.WriteLine($"[decision-unblock-test] NACH: HOLD={holdAfter.Count} UPDATE={string.Join(",", updateAfter)} unmapped(CREATE)={string.Join(",", unmappedAfter)}");
        Console.WriteLine(pass
            ? "[decision-unblock-test] PASS — die durch die Decision blockierte Arbeit fliesst nach der Auflösung nach GitHub (HOLD -> UPDATE/CREATE)."
            : "[decision-unblock-test] FAIL — Arbeit bleibt blockiert!");

        if (outArg is not null)
        {
            var outPath = Path.IsPathRooted(outArg) ? outArg : Path.Combine(repoRoot, outArg);
            Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? ".");
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);
            Console.WriteLine($"[decision-unblock-test] -> {Path.GetRelativePath(repoRoot, outPath)}");
        }
        return pass ? 0 : 1;
    }

    // Forward-Seed auf dem aktuellen Core: Delta-Einträge aus der echten github-sync-View, Mappings aus dem Core.
    private static GithubForwardSeedResult SeedFor(ProjectStateDocument core, IReadOnlyList<GithubIssueSnapshot> issues)
    {
        var entries = CoreViews.GithubSync(core).Entries.Where(e => e.PbiId is "PBI-1" or "PBI-2").ToList();
        return GithubForwardSeed.Seed(entries, CoreGithubMapping.ByPbi(core), issues);
    }

    // Synthetischer Core: REQ-1 (von PBI-1+PBI-2 gedeckt), DEC-1 (open) contradicts REQ-1, beide PBIs blockiert;
    // PBI-1 mit Mapping gh#77, PBI-2 ohne Mapping.
    private static ProjectStateDocument BuildScenario()
    {
        var items = new List<ProjectStateItem>
        {
            Req("REQ-1", "Die App zeigt die Info pro Bewohner."),
            Pbi("PBI-1", "Info-Anzeige A", blocked: true, ["REQ-1"], ["DEC-1"]),
            Pbi("PBI-2", "Info-Anzeige B", blocked: true, ["REQ-1"], ["DEC-1"]),
            Decision("DEC-1", "Widerspruch zu REQ-1: global statt pro Bewohner", "REQ-1"),
        };
        var relations = new List<ProjectStateRelation>
        {
            RequirementSwap.Covers("PBI-1", "REQ-1", "test"),
            RequirementSwap.Covers("PBI-2", "REQ-1", "test"),
            new("DEC-1", "REQ-1", DecisionRelations.Contradicts, "test", new Dictionary<string, string>()),
            new("PBI-1", CoreGithubMapping.IssueRef(77), CoreGithubMapping.RelationType, CoreGithubMapping.RelationSource,
                new Dictionary<string, string>(StringComparer.Ordinal) { ["issueNumber"] = "77", ["operationalStatus"] = "open" }),
        };
        return new ProjectStateDocument("circle-test", ProjectStateDocument.CurrentSchemaVersion, DateTime.UtcNow, [], items, relations, [], []);
    }

    private static ProjectStateItem Req(string id, string text)
        => new ProjectStateItem(id, "requirement", text, "test", null, 1, null, null, null, null, null, [], [],
            new Dictionary<string, string>(), null, [], null, null).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateItem Decision(string id, string text, string targetReq)
        => new ProjectStateItem(id, "decision", text, "test", null, 1, null, null, null, null, null, [], [],
            new Dictionary<string, string>(StringComparer.Ordinal) { [DecisionTargetMeta.Key] = targetReq }, null, [], null, null).WithStatus(CoreStatus.From(DecisionStatus.Open));

    private static ProjectStateItem Pbi(string id, string title, bool blocked, IReadOnlyList<string> reqs, IReadOnlyList<string> decRefs)
        => new ProjectStateItem(id, "pbi", title, "test", null, 1, null, null, null, null, null, [], [],
            new Dictionary<string, string>(), null, [], null,
            new PbiPayload(null, title, [], reqs, decRefs, null, "ready", null, null)).WithStatus(CoreStatus.From(blocked ? "blocked_by_decision" : "active"));
}
