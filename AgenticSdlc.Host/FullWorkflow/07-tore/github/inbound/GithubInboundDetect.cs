using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;

/// <summary>
/// C2a-4 (08.08.2026, c2-inbound-plan §1/§8/§9) — der deterministische ERNTE-DETEKTOR (LLM-frei): läuft über
/// den Snapshot und klassifiziert jedes Issue in die F-Fälle. Drift-Urteil kommt aus der geteilten Quelle
/// <see cref="GithubDriftCheck"/> (gegen den letzten Schreib-Stempel), Sektions-Zerlegung aus der geteilten
/// Struktur-Naht <see cref="GithubIssueTemplate"/>. Der Detect URTEILT nicht inhaltlich und schreibt nichts —
/// er liefert den Fund-Report; Vorschlags-Drafting (Agent-Fallback + Delta-Ausgabe) ist C2b.
/// Kein stilles Verwerfen: JEDES Issue landet in genau einer Kategorie oder in Skipped mit Grund.
/// </summary>
public static class GithubInboundDetect
{
    public const string NicTitlePrefix = "NiC:";
    public const string NicLabel = "not-in-core";

    public static GithubInboundReport Detect(ProjectStateDocument core, IReadOnlyList<GithubIssueSnapshot> issues)
    {
        var mappingByIssue = CoreGithubMapping.CurrentMappings(core)
            .GroupBy(m => m.IssueNumber).ToDictionary(g => g.Key, g => g.Last());
        // 9i/9m: Adoptions-Gedächtnis — Items, die ein Issue als Herkunft tragen (kein eigener Stempel-Store).
        var adoptedByIssue = GithubOriginMeta.ClaimsByIssue(core);
        var syncByPbi = CoreViews.GithubSync(core).Entries
            .GroupBy(e => e.PbiId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        var finds = new List<GithubInboundFind>();
        var skipped = new List<string>();
        var unchanged = 0;

        foreach (var issue in issues.GroupBy(i => i.IssueNumber).Select(g => g.Last()).OrderBy(i => i.IssueNumber))
        {
            // F6 zuerst: bewusst kern-fremd (Titel-Präfix für jedermann, Label für Mitarbeiter) — nie ernten.
            if (issue.Title.TrimStart().StartsWith(NicTitlePrefix, StringComparison.OrdinalIgnoreCase)
                || issue.Labels.Any(l => string.Equals(l, NicLabel, StringComparison.OrdinalIgnoreCase)))
            {
                finds.Add(new GithubInboundFind(GithubInboundCategory.NicOptOut, issue.IssueNumber, issue.Title, null,
                    ["Bewusst nicht im Core (NiC-Opt-out, §9) — wird nie geerntet, nie überschrieben."]));
                continue;
            }

            var mapping = mappingByIssue.GetValueOrDefault(issue.IssueNumber);
            if (mapping is null)
            {
                // 9i/9m: Ernte-Gedächtnis aus der Wahrheit selbst — ein Core-Item (REQ/ARCH/DEC) hat dieses
                // Issue bereits adoptiert. Unverändert seit der Ernte ⇒ überspringen (kein wiederkehrendes
                // Rauschen vor dem Gate); editiert ⇒ eigener Fund F7 (der Agent deutet den neuen Stand).
                if (adoptedByIssue.TryGetValue(issue.IssueNumber, out var claimant))
                {
                    // §3-7-Grenze: Herkunft ohne Hash-Anker (Diktat ohne Snapshot-Treffer) — Drift ist nicht
                    // prüfbar; SICHTBAR benennen statt Dauer-F7 (analog UnknownStamp, nur ohne Fund-Rauschen).
                    if (!claimant.Metadata.ContainsKey(GithubOriginMeta.HarvestedTitleHash)
                        && !claimant.Metadata.ContainsKey(GithubOriginMeta.HarvestedBodyHash))
                    {
                        skipped.Add($"#{issue.IssueNumber} '{Trunc(issue.Title)}': von '{claimant.ItemId}' adoptiert, aber ohne Hash-Anker — Drift nicht prüfbar.");
                        continue;
                    }
                    var titleUnchanged = string.Equals(claimant.Metadata.GetValueOrDefault(GithubOriginMeta.HarvestedTitleHash),
                        GithubProjectionHash.Compute(issue.Title), StringComparison.Ordinal);
                    var bodyUnchanged = string.Equals(claimant.Metadata.GetValueOrDefault(GithubOriginMeta.HarvestedBodyHash),
                        GithubProjectionHash.Compute(issue.Body), StringComparison.Ordinal);
                    if (titleUnchanged && bodyUnchanged) { unchanged++; continue; }
                    finds.Add(new GithubInboundFind(GithubInboundCategory.AdoptedDrift, issue.IssueNumber, issue.Title, null,
                        [$"Issue wurde bereits als '{claimant.ItemId}' in die Wahrheit adoptiert und seither MANUELL editiert — den neuen Stand deuten (Vorschlag an Tor 1, kein Auto-Update)."],
                        GithubIssueTemplate.Parse(issue.Body), AdoptedItemId: claimant.ItemId));
                    continue;
                }

                if (issue.State.Equals("closed", StringComparison.OrdinalIgnoreCase))
                { skipped.Add($"#{issue.IssueNumber} '{Trunc(issue.Title)}': ungemappt UND geschlossen — keine Ernte."); continue; }
                finds.Add(new GithubInboundFind(GithubInboundCategory.UnmappedNew, issue.IssueNumber, issue.Title, null,
                    ["Neu-/Fremd-Issue ohne Mapping — Kandidat für Tor 1 (Adoption oder Merge, §9/§15; Drafting = C2b)."],
                    GithubIssueTemplate.Parse(issue.Body)));
                continue;
            }

            switch (GithubDriftCheck.Check(mapping, issue))
            {
                case GithubDrift.None:
                    unchanged++;
                    break;
                case GithubDrift.Unknown:
                    finds.Add(new GithubInboundFind(GithubInboundCategory.UnknownStamp, issue.IssueNumber, issue.Title, mapping.PbiId,
                        ["Kein Drift-Stempel (Alt-Mapping vor C2a-2) — manueller Edit wäre nicht erkennbar; der nächste Forward-Write stempelt."]));
                    break;
                case GithubDrift.HumanEdited:
                    var parsed = GithubIssueTemplate.Parse(issue.Body);
                    finds.Add(new GithubInboundFind(GithubInboundCategory.MappedDrift, issue.IssueNumber, issue.Title, mapping.PbiId,
                        DescribeDrift(parsed, issue, syncByPbi.GetValueOrDefault(mapping.PbiId)), parsed));
                    break;
            }
        }

        return new GithubInboundReport(issues.Count, finds, skipped, unchanged);
    }

    // Deterministische Fund-Beschreibung (§8): Sektions-Diff gegen den CORE-Stand, FreeText immer benannt.
    private static IReadOnlyList<string> DescribeDrift(ParsedIssueBody parsed, GithubIssueSnapshot issue, GithubSyncEntry? entry)
    {
        var details = new List<string>();
        if (entry is not null)
        {
            if (!string.Equals(issue.Title, entry.Title, StringComparison.Ordinal))
                details.Add($"Titel geändert: '{Trunc(entry.Title)}' → '{Trunc(issue.Title)}'");
            if (parsed.Statement is not null && !string.Equals(parsed.Statement, entry.Statement ?? "", StringComparison.Ordinal))
                details.Add("Statement-Sektion weicht vom Core ab.");
            var coreAk = entry.AcceptanceCriteria ?? [];
            foreach (var a in parsed.AcceptanceCriteria.Except(coreAk, StringComparer.Ordinal))
                details.Add($"AK NEU im Issue: '{Trunc(a)}'");
            foreach (var a in coreAk.Except(parsed.AcceptanceCriteria, StringComparer.Ordinal))
                details.Add($"AK im Issue ENTFERNT: '{Trunc(a)}'");
            // 1g-D / 9k(a)-Rest (19.08.): Änderungen in der RAHMEN-Sektion präzise benennen (Arch-Parität
            // zur AK-Zeile) — vorher fiel ein Rahmen-Edit in den generischen Agent-Fallback.
            var coreRahmen = entry.Constraints ?? [];
            foreach (var c in parsed.Constraints.Except(coreRahmen, StringComparer.Ordinal))
                details.Add($"RAHMEN NEU im Issue: '{Trunc(c)}' (Architektur-Kandidat)");
            foreach (var c in coreRahmen.Except(parsed.Constraints, StringComparer.Ordinal))
                details.Add($"RAHMEN im Issue ENTFERNT: '{Trunc(c)}'");
        }
        foreach (var line in parsed.FreeText)
            details.Add($"Freitext außerhalb der Sektionen: '{Trunc(line)}'");
        if (details.Count == 0)
            details.Add("Abweichung vom letzten Schreib-Stand (z. B. Formatierung/Sektions-Umbau) — Detail-Deutung = Agent-Fallback (C2b).");
        return details;
    }

    private static string Trunc(string s) => s.Length <= 80 ? s : s[..80] + "…";
}

public static class GithubInboundCategory
{
    public const string MappedDrift = "F1_MAPPED_DRIFT";
    public const string UnmappedNew = "F2_UNMAPPED_NEW";
    public const string NicOptOut = "F6_NIC_OPTOUT";
    // 9i/9m: adoptiertes (ungemapptes) Issue wurde NACH der Ernte erneut editiert — F7, erntbar wie F1/F2.
    public const string AdoptedDrift = "F7_ADOPTED_DRIFT";
    // C2d: synthetischer Fund-Typ des Kommentar-Destillats (Delta-Metadata inboundCategory).
    public const string CommentDistill = "C2D_COMMENT";
    public const string UnknownStamp = "UNKNOWN_STAMP";
}

public sealed record GithubInboundFind(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("issueNumber")] int IssueNumber,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("pbiId")] string? PbiId,
    [property: JsonPropertyName("details")] IReadOnlyList<string> Details,
    [property: JsonPropertyName("parsed"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ParsedIssueBody? Parsed = null,
    // 9i/9m (F7): das Core-Item, das dieses Issue adoptiert hat — Kontext fuer Agent + Report.
    [property: JsonPropertyName("adoptedItemId"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? AdoptedItemId = null);

public sealed record GithubInboundReport(
    [property: JsonPropertyName("totalIssues")] int TotalIssues,
    [property: JsonPropertyName("finds")] IReadOnlyList<GithubInboundFind> Finds,
    [property: JsonPropertyName("skipped")] IReadOnlyList<string> Skipped,
    [property: JsonPropertyName("unchanged")] int Unchanged);
