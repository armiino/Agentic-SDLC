using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// 1g-A (19.08.2026, core-analyst-design §3/§4-1): das ANFORDERUNGSDOKUMENT als deterministische
/// Core-Projektion (Muster: GitHub-Issues, docs/architecture.md, ADR-Index). Grundsatz: das Dokument
/// ERSTELLT nie Inhalt — es rendert den autorisierten Stand; jede Fassung ist über Version + Zeitstempel +
/// Core-Fingerabdruck eindeutig einem Wahrheits-Stand zuordenbar. EINE Fassung, Stakeholder-lesbar,
/// IDs in Klammern als Entwickler-Anker.
/// </summary>
public static class RequirementsDocumentProjection
{
    public const string RelativePath = "docs/anforderungen.md";
    public const string KategorieKey = "analystKategorie";   // §4-1: functional | nfr:<merkmal> | process

    /// <summary>Geteilter Kern (K13): lädt Core, liest Vorgänger-Version, rendert, schreibt. CLI und
    /// Steward-Tool sind Häute über dieser Naht.</summary>
    public static async Task<(string Path, int Version, int Items)> RunAsync(string repoRoot, DateTime? utcNow = null)
    {
        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            throw new InvalidOperationException("Core fehlt (state/core) — nichts zu rendern.");
        var core = await repo.LoadAsync().ConfigureAwait(false);

        var path = Path.Combine(repoRoot, RelativePath);
        var previous = File.Exists(path) ? await File.ReadAllTextAsync(path).ConfigureAwait(false) : null;
        var version = NextVersion(previous);
        var markdown = Render(core, version, utcNow ?? DateTime.UtcNow);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, markdown).ConfigureAwait(false);
        return (path, version, core.Items.Count);
    }

    /// <summary>Fortlaufende Version aus der Kopfzeile der Vorgänger-Datei (+1); ohne Vorgänger = 1.</summary>
    public static int NextVersion(string? previousContent)
        => previousContent is not null && Regex.Match(previousContent, @"Version:\s*(\d+)") is { Success: true } m
            ? int.Parse(m.Groups[1].Value) + 1
            : 1;

    public static string Render(ProjectStateDocument core, int version, DateTime generatedUtc)
    {
        var active = core.Items.Where(i => i.ReadStatus().Validity == Validity.Active).ToList();
        var reqs = active.Where(i => Is(i, "requirement")).ToList();
        var featureById = active.Where(i => Is(i, "feature"))
            .ToDictionary(i => i.ItemId, i => i.Feature?.Label ?? i.Text, StringComparer.Ordinal);
        var featureOfReq = core.Relations
            .Where(r => r.RelationType == "part_of_feature" && reqs.Any(q => q.ItemId == r.FromId))
            .GroupBy(r => r.FromId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().ToId, StringComparer.Ordinal);

        var sb = new StringBuilder();
        sb.AppendLine($"# Anforderungsdokument — {core.ProjectId}");
        sb.AppendLine();
        sb.AppendLine($"> Version: {version} · Stand: {generatedUtc:yyyy-MM-dd HH:mm} UTC · Core: {core.Items.Count} Items · Fingerabdruck: {Fingerprint(core)}");
        sb.AppendLine("> Projektion aus der Projektwahrheit (Core) — GENERIERT, nie von Hand pflegen; jede Fassung entspricht exakt einem autorisierten Wahrheits-Stand.");

        // ── 1. Funktionale Anforderungen je Feature (nfr-kategorisierte Items wandern in Abschnitt 2) ──
        sb.AppendLine().AppendLine("## 1. Funktionale Anforderungen");
        var functional = reqs.Where(r => !IsNfr(r)).ToList();
        foreach (var fc in featureById.OrderBy(f => f.Key, StringComparer.Ordinal))
        {
            var inFeature = functional.Where(r => featureOfReq.GetValueOrDefault(r.ItemId) == fc.Key)
                .OrderBy(r => r.ItemId, StringComparer.Ordinal).ToList();
            if (inFeature.Count == 0) continue;
            sb.AppendLine().AppendLine($"### {fc.Value} ({fc.Key})").AppendLine();
            foreach (var r in inFeature) sb.AppendLine(ReqLine(r));
        }
        var unplaced = functional.Where(r => !featureOfReq.ContainsKey(r.ItemId))
            .OrderBy(r => r.ItemId, StringComparer.Ordinal).ToList();
        if (unplaced.Count > 0)
        {
            sb.AppendLine().AppendLine("### Ohne Feature-Zuordnung").AppendLine();
            foreach (var r in unplaced) sb.AppendLine(ReqLine(r));
        }

        // ── 2. NFRs je Qualitätsmerkmal (Ehrlichkeits-Vermerk: Kategorien-Nachzug für Alt-Bestand offen) ──
        sb.AppendLine().AppendLine("## 2. Nicht-funktionale Anforderungen");
        sb.AppendLine();
        sb.AppendLine("> Enthält die kategorisierten Items (`analystKategorie`); die Kategorisierung des Alt-Bestands steht aus — unkategorisierte Anforderungen stehen in Abschnitt 1 bei ihrem Feature.");
        var nfrByMerkmal = reqs.Where(IsNfr)
            .GroupBy(r => Kategorie(r)!["nfr:".Length..], StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal).ToList();
        if (nfrByMerkmal.Count == 0) sb.AppendLine().AppendLine("_(noch keine kategorisierten NFRs im Core)_");
        foreach (var g in nfrByMerkmal)
        {
            sb.AppendLine().AppendLine($"### {g.Key}").AppendLine();
            foreach (var r in g.OrderBy(r => r.ItemId, StringComparer.Ordinal)) sb.AppendLine(ReqLine(r));
        }

        // ── 3. Technische Rahmenbedingungen (ARCH constraint-Rolle; Details: docs/architecture.md) ──
        sb.AppendLine().AppendLine("## 3. Technische Rahmenbedingungen");
        sb.AppendLine();
        var constraints = active.Where(i => Is(i, "architecture")
                && i.Architecture?.Roles.Contains("constraint", StringComparer.Ordinal) == true)
            .OrderBy(i => i.ItemId, StringComparer.Ordinal).ToList();
        if (constraints.Count == 0) sb.AppendLine("_(keine)_");
        foreach (var a in constraints) sb.AppendLine($"- {a.Text} ({a.ItemId})");
        if (constraints.Count > 0) sb.AppendLine().AppendLine("_Voller Architektur-Stand inkl. Entscheidungen (ADRs): docs/architecture.md_");

        // ── 4. Offene Entscheidungen / Fragen (der ehrliche Offenheits-Ausweis des Dokuments) ──
        sb.AppendLine().AppendLine("## 4. Offene Entscheidungen und Fragen");
        sb.AppendLine();
        var open = active.Where(i => Is(i, "decision") && i.ReadStatus().IsOpenDecision)
            .OrderBy(i => i.ItemId, StringComparer.Ordinal).ToList();
        if (open.Count == 0) sb.AppendLine("_(keine — alle Entscheidungen sind aufgelöst)_");
        foreach (var d in open) sb.AppendLine($"- {d.Text} ({d.ItemId})");

        return sb.ToString();
    }

    private static string ReqLine(ProjectStateItem r)
        => $"- {r.Text} ({r.ItemId}{(r.Version > 1 ? $", v{r.Version}" : "")})";

    private static bool Is(ProjectStateItem i, string type) => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase);
    private static string? Kategorie(ProjectStateItem i) => i.Metadata.GetValueOrDefault(KategorieKey);
    private static bool IsNfr(ProjectStateItem i) => Kategorie(i)?.StartsWith("nfr:", StringComparison.Ordinal) == true;

    /// <summary>Kurzer, deterministischer Wahrheits-Stand-Anker (SHA-256/16 über den serialisierten Core).</summary>
    public static string Fingerprint(ProjectStateDocument core)
        => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(core, JsonFiles.Json)))[..16].ToLowerInvariant();
}
