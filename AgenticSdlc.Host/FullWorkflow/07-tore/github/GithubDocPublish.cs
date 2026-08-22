using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// Slice S Teil 1 (⚖ Autor 21.08., Bauplan `docs/aktiv/team-sichtbarkeit-slice.md`): DOC-PUBLISH —
/// die lokalen Doc-Projektionen (Anforderungsdokument, architecture.md, ADRs) als ONE-WAY-Projektion
/// ins Team-Repo. Haus-Muster 1:1 wie Issues: deterministischer Seed (In-Sync ⇒ keine Op) →
/// bestehendes github-forward-gate → Apply (Contents-API) → Stempel IM CORE (Kangal-gedeckt).
/// Stempel-Zuhause = proposals-Ledger (`github_doc_projection`) — Wissen ohne Item, wie die
/// R-35-Rejections. Bewusste Grenze: KEINE Doc-Ernte (Diskussion gehört in Issues/Kommentare,
/// nicht in generierte Dateien); Fremd-Edits werden am Apply LAUT benannt und bewusst überschrieben.
/// </summary>
public static class GithubDocPublish
{
    public const string ProposalType = "github_doc_projection";
    public const string Header =
        "<!-- GENERIERT aus der Projektwahrheit (Agentic-SDLC) — NICHT von Hand editieren: "
        + "Änderungen werden beim nächsten Abgleich überschrieben. Diskussion bitte als Issue/Kommentar. -->\n\n";

    // Deterministische Projektionen + ALLE Autor-Artefakte (Whitelist-Quelle: AuthoredDocument.Arten).
    private static readonly string[] FixedDocs =
        [.. Core.AuthoredDocument.Arten.Select(a => a.RelPath), "docs/anforderungen.md", "docs/backlog.md", "docs/architecture.md"];

    /// <summary>Erweiterungs-Naht (Autor-Frage 21.08.): NEUES Doc publizieren = ① ggf. deterministischen
    /// Renderer bauen ② Pfad HIER eintragen — Seed/Gate/Apply/Stempel/In-Sync kommen geschenkt.
    /// „Lebend"-Garantie: die deterministischen Projektionen (Anforderungen, Backlog) werden vor dem Seed
    /// FRISCH gerendert — aber NUR bei geändertem Core-Fingerabdruck (sonst würde der Versions-Zähler
    /// jede Runde hochzählen und die Docs wären nie in-sync).</summary>
    public static async Task RefreshDeterministicProjectionsAsync(string repoRoot, ProjectStateDocument core)
    {
        var fp = RequirementsDocumentProjection.Fingerprint(core);
        if (IsStale(Path.Combine(repoRoot, "docs", "anforderungen.md"), fp))
            await RequirementsDocumentProjection.RunAsync(repoRoot).ConfigureAwait(false);
        if (IsStale(Path.Combine(repoRoot, "docs", "backlog.md"), fp))
            await BacklogDocumentProjection.RunAsync(repoRoot).ConfigureAwait(false);
        // C4-Kreislauf: die Lücken-Sektion im c4 folgt dem DEC-Topf — idempotenter Section-Replace
        // (No-op ohne c4-Datei/ohne Änderung; die Diagramme bleiben unangetastet).
        C4GapSection.Ensure(repoRoot, core);
    }

    private static bool IsStale(string path, string fingerprint)
        => !File.Exists(path) || !File.ReadAllText(path).Contains($"Fingerabdruck: {fingerprint}", StringComparison.Ordinal);

    /// <summary>Publizierbare Docs: die festen Projektionen + alle ADRs (inkl. Index).</summary>
    public static IReadOnlyList<string> EnumerateDocs(string repoRoot)
    {
        var result = FixedDocs.Where(rel => File.Exists(Path.Combine(repoRoot, rel))).ToList();
        var adrDir = Path.Combine(repoRoot, "docs", "adr");
        if (Directory.Exists(adrDir))
            result.AddRange(Directory.EnumerateFiles(adrDir, "*.md")
                .Select(p => Path.Combine("docs", "adr", Path.GetFileName(p)).Replace('\\', '/'))
                .OrderBy(x => x, StringComparer.Ordinal));
        return result;
    }

    /// <summary>Publizierter Inhalt = GENERIERT-Kopf + lokaler Stand (deterministisch).</summary>
    public static string PublishedContent(string repoRoot, string relPath)
        => Header + File.ReadAllText(Path.Combine(repoRoot, relPath));

    /// <summary>Seed: je Doc GENAU EINE UPSERT_FILE-Op — NUR wenn der Inhalts-Hash vom Core-Stempel
    /// abweicht (In-Sync-Muster der Issues). Title=Repo-Pfad, Body=Inhalt.</summary>
    public static IReadOnlyList<GithubForwardOp> SeedOps(string repoRoot, ProjectStateDocument core)
    {
        var ops = new List<GithubForwardOp>();
        foreach (var rel in EnumerateDocs(repoRoot))
        {
            var content = PublishedContent(repoRoot, rel);
            var hash = GithubProjectionHash.Compute(content);
            var stamp = StampOf(core, rel);
            if (stamp is not null && stamp.TryGetValue("contentHash", out var h) && h == hash) continue;
            ops.Add(new GithubForwardOp(
                GithubForwardKind.UpsertFile, $"doc:{rel}", null, rel, content, null, null, null,
                Anchor: $"doc {rel}",
                Rationale: stamp is null
                    ? "Doc-Projektion erstmals im Team-Repo veröffentlichen (One-way, GENERIERT-Kopf)."
                    : "Doc-Projektion hat sich gegenüber dem publizierten Stand geändert — aktualisieren.",
                Origin: "deterministic"));
        }
        return ops;
    }

    public static IReadOnlyDictionary<string, string>? StampOf(ProjectStateDocument core, string relPath)
        => core.Proposals.LastOrDefault(p =>
                string.Equals(p.ProposalType, ProposalType, StringComparison.Ordinal)
                && string.Equals(p.PayloadPath, relPath, StringComparison.Ordinal))?.Metadata;

    /// <summary>Stempel-Upsert im Core (ein Proposal je Pfad, stabile Id) — gleicher Save wie das
    /// Issue-Mapping, Kangal-gedeckt.</summary>
    public static ProjectStateDocument ApplyStamps(
        ProjectStateDocument core,
        IReadOnlyList<(string Path, string ContentHash, string Sha)> stamps,
        string repository, string sourceRunId)
    {
        if (stamps.Count == 0) return core;
        var proposals = core.Proposals.ToList();
        foreach (var (path, hash, sha) in stamps)
        {
            var id = "DOC-" + path.Replace('/', '-').Replace(".md", "", StringComparison.Ordinal);
            var meta = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["contentHash"] = hash, ["sha"] = sha, ["repository"] = repository,
                ["updatedUtc"] = DateTime.UtcNow.ToString("O"),
            };
            var idx = proposals.FindIndex(p => string.Equals(p.ProposalId, id, StringComparison.Ordinal));
            var entry = new ProjectStateProposal(id, ProposalType, "applied", sourceRunId, path, meta);
            if (idx >= 0) proposals[idx] = entry; else proposals.Add(entry);
        }
        return core with { Proposals = proposals };
    }
}
