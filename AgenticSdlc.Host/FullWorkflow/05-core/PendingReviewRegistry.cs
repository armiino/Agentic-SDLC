using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Globalization;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// C4d (09.08.2026, c4-plan §10/§11 + K12) — die PENDING-REGISTRY: „nichts, das auf einen Menschen wartet,
/// lebt nur in einem Run-Ordner." Wartende Gate-Vorschläge der Standalone-Bahnen werden als
/// `pending_review`-Proposal MIT VOLLER NUTZLAST im Core registriert (Register), vom GATED Apply
/// geschlossen (Close) und beim Lesen gegen den AKTUELLEN Core auf Frische geprüft (ListOpen → ÜBERHOLT).
/// K12-Grenze: dieser Registrar schreibt AUSSCHLIESSLICH die proposals-Liste — nie Items/Relationen
/// (Grenz-Test in PendingReviewRegistryTests). Läufe/Status ZEIGEN Pendings, konsumieren sie NIE (K12-2).
/// </summary>
public static class PendingReviewRegistry
{
    public const string Type = "pending_review";
    public const string StatusOpen = "open";

    /// <summary>Registriert einen wartenden Gate-Vorschlag (volle Nutzlast; pbiIds+Versionen = Frische-Anker).</summary>
    public static (ProjectStateDocument Core, string ProposalId) Register(
        ProjectStateDocument core, string bahn, string gateCommand, JsonElement payload,
        IReadOnlyList<string> pbiIds, string sourceRunId)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var proposalId = $"PEND-{sourceRunId}";
        var meta = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["bahn"] = bahn,
            ["gateCommand"] = gateCommand,
            ["createdUtc"] = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            // Frische-Anker: Version je Ziel-PBI zum Registrier-Zeitpunkt (ÜBERHOLT wird beim LESEN berechnet).
            ["pbiVersions"] = string.Join(",", pbiIds.Select(id =>
                $"{id}:{(byId.TryGetValue(id, out var it) ? it.Version : 0)}")),
        };
        var proposal = new ProjectStateProposal(proposalId, Type, StatusOpen, sourceRunId, null, meta, payload);
        return (core with { Proposals = [.. core.Proposals.Where(p => p.ProposalId != proposalId), proposal] }, proposalId);
    }

    /// <summary>Schließt einen Eintrag — NUR aus dem gated Apply heraus rufen. Anti-Zumüll-Regel (Autor
    /// 09.08.): erledigte Pendings werden ENTFERNT, nicht als Stub gehortet — die Beleg-Kette lebt komplett
    /// woanders (Run-applied/-Report · Item-History-Notiz · Core-Snapshot-Historie). Anders als R-35-
    /// Rejections (ingest_rejection = WISSEN, bleibt) ist ein erledigtes Pending reine Prozess-Buchhaltung.</summary>
    public static ProjectStateDocument Close(ProjectStateDocument core, string proposalId, string outcome)
        => core with { Proposals = [.. core.Proposals.Where(p => !(p.ProposalId == proposalId && p.Status == StatusOpen))] };

    public sealed record PendingEntry(
        string ProposalId, string Bahn, string GateCommand, string CreatedUtc,
        IReadOnlyList<string> PbiIds, bool Ueberholt, string? UeberholtGrund);

    /// <summary>Offene Einträge mit Frische-Urteil gegen den AKTUELLEN Core (nichts wird gespeichert).</summary>
    public static IReadOnlyList<PendingEntry> ListOpen(ProjectStateDocument core)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        return core.Proposals
            .Where(p => string.Equals(p.ProposalType, Type, StringComparison.Ordinal) && p.Status == StatusOpen)
            .OrderBy(p => p.ProposalId, StringComparer.Ordinal)
            .Select(p =>
            {
                var anchors = (p.Metadata.GetValueOrDefault("pbiVersions") ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Split(':')).Where(x => x.Length == 2)
                    .Select(x => (Id: x[0], Version: int.TryParse(x[1], out var v) ? v : 0)).ToList();
                // Slice S Teil 2: die Frische-Regel ist BAHN-Semantik — ein clarify-sweep-Pending hängt an der
                // Klärungs-Lage (needs_clarify weg ⇒ überholt); ein Feld-Pending (pbi-fields) nur an Existenz+Version.
                var bahn = p.Metadata.GetValueOrDefault("bahn") ?? "?";
                var requiresClarify = string.Equals(bahn, "clarify-sweep", StringComparison.Ordinal);
                string? grund = null;
                foreach (var (id, version) in anchors)
                {
                    if (!byId.TryGetValue(id, out var it)) { grund = $"{id} existiert nicht mehr"; break; }
                    if (requiresClarify && it.ReadStatus().Blocker != Blocker.NeedsClarify) { grund = $"{id} ist nicht mehr needs_clarify"; break; }
                    if (it.Version != version) { grund = $"{id} wurde zwischenzeitlich geändert (v{version}→v{it.Version})"; break; }
                }
                return new PendingEntry(p.ProposalId,
                    bahn,
                    p.Metadata.GetValueOrDefault("gateCommand") ?? "?",
                    p.Metadata.GetValueOrDefault("createdUtc") ?? "?",
                    anchors.Select(a => a.Id).ToList(), grund is not null, grund);
            })
            .ToList();
    }
}
