using System.Globalization;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Tor 3 / T3.1 — Mapping "PBI <-> GitHub-Issue" als CORE-RELATION (nicht mehr nur Run-Artefakt).
//
// Die pre-1c-Reconciliation (l4/githubreconciliation) hielt das Mapping als lose --mappings-JSON bzw. als
// github-mappings-created.json im Run-Ordner — das ist die "alte Luecke" aus plan-tor3-github-agent.md §5:
// beim naechsten Sprint-Re-Run war die Zuordnung nicht mehr da -> Duplikat-Issues. T3.1 schliesst das, indem
// das Mapping in den lebenden Core wandert:  pbi --implemented_by_issue--> gh#<n>  (+ Metadata).
//
// Damit ist die Relation die Dedup-Basis fuer den naechsten Delta-Lauf UND die Quelle der github-sync-view.
// Rein deterministisch, kein LLM, kein GitHub-Call: dieser Baustein SCHREIBT nur das bereits (gated/manuell)
// ermittelte Mapping in den Core-Graphen. Der Core wird ausschliesslich ueber den Repository-Port persistiert.
public static class CoreGithubMapping
{
    public const string RelationType = "implemented_by_issue";
    public const string RelationSource = "github-sync";

    // Kanonische Zielseite der Relation. Ein GitHub-Issue ist KEIN Core-Item, daher eine stabile Referenz-ID
    // (kein itemId). Views, die Items aufloesen (AffectedItems), ignorieren nicht-aufloesbare toIds bereits.
    public static string IssueRef(int issueNumber) => $"gh#{issueNumber}";

    public static (ProjectStateDocument Core, GithubMappingReport Report) Apply(
        ProjectStateDocument core,
        IReadOnlyList<GithubMappingOp> ops)
    {
        var now = DateTime.UtcNow;
        var pbiIds = core.Items
            .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var relations = core.Relations.ToList();

        var linked = new List<string>();
        var remapped = new List<string>();
        var closed = new List<string>();
        var reopened = new List<string>();
        var removed = new List<string>();
        var unchanged = new List<string>();
        var skipped = new List<string>();

        foreach (var op in ops)
        {
            if (string.IsNullOrWhiteSpace(op.PbiId) || !pbiIds.Contains(op.PbiId))
            {
                skipped.Add($"{op.PbiId ?? "<null>"}: kein bekanntes PBI im Core (UNKNOWN_PBI)");
                continue;
            }

            // Ein PBI haelt hoechstens EINE implemented_by_issue-Relation (1:1-Mapping). Falls mehrere existieren
            // (Alt-Daten), operiere auf der zum issueNumber passenden bzw. der ersten.
            var idx = relations.FindIndex(r =>
                string.Equals(r.RelationType, RelationType, StringComparison.Ordinal) &&
                string.Equals(r.FromId, op.PbiId, StringComparison.Ordinal) &&
                (op.Kind == GithubMappingKind.Link || IssueNumberOf(r) == op.IssueNumber));
            var existing = idx >= 0 ? relations[idx] : null;

            switch (op.Kind)
            {
                case GithubMappingKind.Link:
                {
                    if (existing is null)
                    {
                        relations.Add(MakeRelation(op, "open", now, linkedUtc: now));
                        linked.Add($"{op.PbiId} -> {IssueRef(op.IssueNumber)}");
                    }
                    else if (IssueNumberOf(existing) == op.IssueNumber)
                    {
                        // Idempotent: gleiches Issue erneut verlinkt -> Metadata auffrischen (url/repo/status=open);
                        // C2a-2: bestehender Schreib-Stempel bleibt erhalten, wenn die Op keinen neuen trägt.
                        relations[idx] = MakeRelation(op, "open", now, linkedUtc: LinkedUtcOf(existing), previous: existing);
                        unchanged.Add($"{op.PbiId} -> {IssueRef(op.IssueNumber)}");
                    }
                    else
                    {
                        // Umverdrahtung auf ein anderes Issue -> alte Relation ersetzen (kein Duplikat).
                        // C2a-2: BEWUSST ohne previous — der alte Stempel gehört zum ALTEN Issue.
                        var old = IssueNumberOf(existing);
                        relations[idx] = MakeRelation(op, "open", now, linkedUtc: now);
                        remapped.Add($"{op.PbiId}: gh#{old} -> {IssueRef(op.IssueNumber)}");
                    }
                    break;
                }
                case GithubMappingKind.Close:
                {
                    if (existing is null) { skipped.Add($"{op.PbiId}: CLOSE ohne bestehendes Mapping"); break; }
                    relations[idx] = WithStatus(existing, "closed", now);
                    closed.Add($"{op.PbiId} -> {IssueRef(op.IssueNumber)}");
                    break;
                }
                case GithubMappingKind.Reopen:
                {
                    if (existing is null) { skipped.Add($"{op.PbiId}: REOPEN ohne bestehendes Mapping"); break; }
                    relations[idx] = WithStatus(existing, "open", now);
                    reopened.Add($"{op.PbiId} -> {IssueRef(op.IssueNumber)}");
                    break;
                }
                case GithubMappingKind.Unlink:
                {
                    if (existing is null) { skipped.Add($"{op.PbiId}: UNLINK ohne bestehendes Mapping"); break; }
                    relations.RemoveAt(idx);
                    removed.Add($"{op.PbiId} -x- {IssueRef(op.IssueNumber)}");
                    break;
                }
                default:
                    skipped.Add($"{op.PbiId}: unbekannte Mapping-Operation {op.Kind}");
                    break;
            }
        }

        var updated = core with
        {
            SchemaVersion = ProjectStateDocument.CurrentSchemaVersion,
            Relations = relations
        };
        var report = new GithubMappingReport(linked, remapped, closed, reopened, removed, unchanged, skipped);
        return (updated, report);
    }

    // Aktuelle Mappings als flache Projektion (Dedup-Basis fuer den naechsten Delta-Lauf).
    public static IReadOnlyList<GithubMappingRecord> CurrentMappings(ProjectStateDocument core)
        => core.Relations
            .Where(r => string.Equals(r.RelationType, RelationType, StringComparison.Ordinal))
            .Select(r => new GithubMappingRecord(
                PbiId: r.FromId,
                IssueNumber: IssueNumberOf(r),
                IssueUrl: r.Metadata.GetValueOrDefault("issueUrl"),
                Repository: r.Metadata.GetValueOrDefault("repository"),
                OperationalStatus: r.Metadata.GetValueOrDefault("operationalStatus") ?? "open",
                ProjectedTitleHash: r.Metadata.GetValueOrDefault("projectedTitleHash"),
                ProjectedBodyHash: r.Metadata.GetValueOrDefault("projectedBodyHash")))
            .OrderBy(m => m.PbiId, StringComparer.Ordinal)
            .ToList();

    // pbiId -> Mapping (letzte gewinnt), fuer die github-sync-view.
    public static IReadOnlyDictionary<string, GithubMappingRecord> ByPbi(ProjectStateDocument core)
        => CurrentMappings(core)
            .GroupBy(m => m.PbiId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

    private static ProjectStateRelation MakeRelation(GithubMappingOp op, string operationalStatus, DateTime now, DateTime linkedUtc,
        ProjectStateRelation? previous = null)
    {
        var meta = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["issueNumber"] = op.IssueNumber.ToString(CultureInfo.InvariantCulture),
            ["operationalStatus"] = operationalStatus,
            ["linkedUtc"] = linkedUtc.ToString("O", CultureInfo.InvariantCulture),
            ["updatedUtc"] = now.ToString("O", CultureInfo.InvariantCulture)
        };
        if (!string.IsNullOrWhiteSpace(op.IssueUrl)) meta["issueUrl"] = op.IssueUrl!;
        if (!string.IsNullOrWhiteSpace(op.Repository)) meta["repository"] = op.Repository!;
        if (!string.IsNullOrWhiteSpace(op.Origin)) meta["origin"] = op.Origin!;
        // C2a-2: Schreib-Stempel — neuer Hash vom Schreib-Zweig gewinnt; sonst bleibt der bestehende Stempel
        // erhalten (Link-Ops ohne Schreib-Akt dürfen den Drift-Anker nicht löschen).
        var titleHash = op.ProjectedTitleHash ?? previous?.Metadata.GetValueOrDefault("projectedTitleHash");
        var bodyHash = op.ProjectedBodyHash ?? previous?.Metadata.GetValueOrDefault("projectedBodyHash");
        if (titleHash is not null) meta["projectedTitleHash"] = titleHash;
        if (bodyHash is not null) meta["projectedBodyHash"] = bodyHash;
        if (op.ProjectedBodyHash is not null) meta["projectedUtc"] = now.ToString("O", CultureInfo.InvariantCulture);
        else if (previous?.Metadata.GetValueOrDefault("projectedUtc") is { } prevUtc) meta["projectedUtc"] = prevUtc;
        return new ProjectStateRelation(op.PbiId, IssueRef(op.IssueNumber), RelationType, RelationSource, meta);
    }

    private static ProjectStateRelation WithStatus(ProjectStateRelation r, string operationalStatus, DateTime now)
    {
        var meta = new Dictionary<string, string>(r.Metadata, StringComparer.Ordinal)
        {
            ["operationalStatus"] = operationalStatus,
            ["updatedUtc"] = now.ToString("O", CultureInfo.InvariantCulture)
        };
        return r with { Metadata = meta };
    }

    private static int IssueNumberOf(ProjectStateRelation r)
    {
        if (int.TryParse(r.Metadata.GetValueOrDefault("issueNumber"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
            return n;
        // Fallback: aus der Referenz-ID "gh#<n>" lesen.
        var tail = r.ToId.StartsWith("gh#", StringComparison.Ordinal) ? r.ToId[3..] : r.ToId;
        return int.TryParse(tail, NumberStyles.Integer, CultureInfo.InvariantCulture, out var m) ? m : 0;
    }

    private static DateTime LinkedUtcOf(ProjectStateRelation r)
        => DateTime.TryParse(r.Metadata.GetValueOrDefault("linkedUtc"), CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var t) ? t : DateTime.UtcNow;
}

public enum GithubMappingKind { Link, Close, Reopen, Unlink }

// Eine Mapping-Operation (aus dem gated Write-Apply bzw. manuell). issueNumber ist die GitHub-Issue-Nummer,
// pbiId die Core-PBI-ID. Kind steuert Anlegen/Statuswechsel/Entfernen.
public sealed record GithubMappingOp(
    string PbiId,
    int IssueNumber,
    string? IssueUrl = null,
    string? Repository = null,
    GithubMappingKind Kind = GithubMappingKind.Link,
    string? Origin = null,
    // C2a-2 (Drift-Wächter §7): Hash des GESCHRIEBENEN Titels/Bodys — gesetzt NUR von den echten
    // Schreib-Zweigen des gated Forward-Apply (CREATE/UPDATE); Link-Ops ohne Schreib-Akt (COMMENT/LINK)
    // lassen sie null und der bestehende Stempel bleibt erhalten (Preserve in MakeRelation).
    string? ProjectedTitleHash = null,
    string? ProjectedBodyHash = null);

public sealed record GithubMappingRecord(
    string PbiId,
    int IssueNumber,
    string? IssueUrl,
    string? Repository,
    string OperationalStatus,
    // C2a-2: der letzte Schreib-Stempel (null = Alt-Mapping vor C2a-2 → GithubDrift.Unknown).
    string? ProjectedTitleHash = null,
    string? ProjectedBodyHash = null);

public sealed record GithubMappingReport(
    IReadOnlyList<string> Linked,
    IReadOnlyList<string> Remapped,
    IReadOnlyList<string> Closed,
    IReadOnlyList<string> Reopened,
    IReadOnlyList<string> Removed,
    IReadOnlyList<string> Unchanged,
    IReadOnlyList<string> Skipped);
