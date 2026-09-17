using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.3 — deterministischer Vorfilter (plan-tor3 §4). NUR die klaren Faelle, kein Urteil:
//   - blocked_by_decision  -> HOLD_BLOCKED (NIE ein CREATE; wartet auf Tor 2), bestehendes Issue nur kommentieren.
//   - needs_clarify + unmapped + holdUnclearNewPbis -> HOLD_CLARIFY (R-26: unbeaufsichtigt kein Auto-Issue, geparkt).
//   - Mapping vorhanden + Issue offen        -> UPDATE_ISSUE (vorgeschlagener Patch, KEIN Auto-Overwrite, Rev 2).
//   - Mapping vorhanden + Issue geschlossen, PBI aktiv/needs_clarify -> FLAG_DRIFT.
//   - Mapping vorhanden, Issue nicht im Snapshot                     -> FLAG_DRIFT (Mapping veraltet).
// Alles OHNE Mapping und NICHT blockiert bleibt der agentischen Stufe (Suche + LINK/CREATE, konservativ).
public static class GithubForwardSeed
{
    private static readonly HashSet<string> Activeish = new(StringComparer.OrdinalIgnoreCase) { "active", "needs_clarify" };

    public static GithubForwardSeedResult Seed(
        IReadOnlyList<GithubSyncEntry> deltaEntries,
        IReadOnlyDictionary<string, GithubMappingRecord> mappingByPbi,
        IReadOnlyList<GithubIssueSnapshot> issues,
        bool holdUnclearNewPbis = false,
        // 9m: pbiId -> Ursprungs-Issue aus der Adoption (GithubOriginMeta.AdoptedIssueByPbi) — deterministischer
        // LINK statt Such-Match/CREATE-Duplikat, wenn das PBI ein geerntetes Issue deckt.
        IReadOnlyDictionary<string, int>? adoptedIssueByPbi = null)
    {
        var issueByNumber = issues.GroupBy(i => i.IssueNumber).ToDictionary(g => g.Key, g => g.Last());
        var deterministic = new List<GithubForwardOp>();
        var unmapped = new List<GithubSyncEntry>();

        foreach (var e in deltaEntries)
        {
            var blocked = e.BlockedByOpenDecision
                || string.Equals(e.Status, "blocked_by_decision", StringComparison.OrdinalIgnoreCase);
            var mapping = mappingByPbi.GetValueOrDefault(e.PbiId);

            if (blocked)
            {
                deterministic.Add(new GithubForwardOp(
                    GithubForwardKind.HoldBlocked, e.PbiId, mapping?.IssueNumber, null, null, null, null, null,
                    Anchor: mapping is null ? $"pbi {e.PbiId}" : $"pbi {e.PbiId} -> {CoreGithubMapping.IssueRef(mapping.IssueNumber)}",
                    Rationale: "PBI ist durch eine offene Decision blockiert — kein Issue anlegen (wartet auf Tor 2 Unblock).",
                    Origin: "deterministic"));
                continue;
            }

            if (mapping is null)
            {
                // 9m: das PBI deckt ein adoptiertes Issue (eindeutige Herkunft) → deterministischer LINK aufs
                // Original — NIE ein CREATE-Duplikat, NIE dem Such-Match überlassen. Gated wie jede Op.
                if (adoptedIssueByPbi?.TryGetValue(e.PbiId, out var adoptedIssue) == true)
                {
                    deterministic.Add(new GithubForwardOp(
                        GithubForwardKind.Link, e.PbiId, adoptedIssue, null, null, null, null, null,
                        Anchor: $"pbi {e.PbiId} -> {CoreGithubMapping.IssueRef(adoptedIssue)} (adoptiert)",
                        Rationale: $"PBI deckt Wahrheit, die aus Issue #{adoptedIssue} adoptiert wurde — deterministischer Link aufs Ursprungs-Issue (9m).",
                        Origin: "deterministic"));
                    continue;
                }

                // R-26: neues, noch unklares PBI ohne Mapping + niemand da, der ein Issue autorisiert (accept-all) →
                // PARKEN statt agentisch CREATE erzwingen. Deterministisch, sichtbar, keine Sackgasse. Gemappte
                // needs_clarify bleiben unberuehrt (UPDATE, s. u.). Aufloesen = Folgeschritt (Option C / Parkplatz-Core).
                if (holdUnclearNewPbis && string.Equals(e.Status, "needs_clarify", StringComparison.OrdinalIgnoreCase))
                {
                    deterministic.Add(new GithubForwardOp(
                        GithubForwardKind.HoldClarify, e.PbiId, null, null, null, null, null, null,
                        Anchor: $"pbi {e.PbiId}",
                        Rationale: "PBI ist neu und noch unklar (needs_clarify) — im unbeaufsichtigten Lauf kein Auto-Issue; geparkt bis zur Klaerung (needs_clarify->active).",
                        Origin: "deterministic"));
                    continue;
                }
                unmapped.Add(e);
                continue;
            }

            var anchor = $"pbi {e.PbiId} -> {CoreGithubMapping.IssueRef(mapping.IssueNumber)}";
            if (!issueByNumber.TryGetValue(mapping.IssueNumber, out var issue))
            {
                deterministic.Add(new GithubForwardOp(
                    GithubForwardKind.FlagDrift, e.PbiId, mapping.IssueNumber, null, null, null, null, null, anchor,
                    "Gemapptes Issue nicht im aktuellen Snapshot — Mapping evtl. veraltet, manuell pruefen.", "deterministic"));
            }
            else if (issue.State.Equals("closed", StringComparison.OrdinalIgnoreCase) && Activeish.Contains(e.Status))
            {
                deterministic.Add(new GithubForwardOp(
                    GithubForwardKind.FlagDrift, e.PbiId, mapping.IssueNumber, null, null, null, null, null, anchor,
                    $"Issue #{mapping.IssueNumber} ist geschlossen, PBI aber '{e.Status}' — Drift, manuell pruefen.", "deterministic"));
            }
            else if (GithubDriftCheck.Check(mapping, issue) == GithubDrift.HumanEdited)
            {
                // C2a-3 (§7 c2-inbound-plan): DRIFT-SPERRE — ein Mensch hat Titel/Body seit unserem letzten
                // Write editiert. Der Edit darf NICHT still überschrieben werden: erst ernten (github-inbound)
                // oder am Gate bewusst auflösen. Konvergenz: nach Ernte→Tor→Apply stempelt der Write neu.
                // R-60-Fix (20.08.): „bewusst auflösen" ist jetzt GEBAUT — die Op trägt die Core-Projektion
                // (Title/Body/Labels) mit; NUR der explizite Gate-Entscheid `overwrite` (nie apply/accept-all)
                // schreibt sie und stempelt neu. Nötig, weil weder Ernte noch Reject die Sperre lösen können
                // (nur ein Write stempelt) — vorher war ein geernteter+abgelehnter Edit eine Ewig-Sackgasse.
                deterministic.Add(new GithubForwardOp(
                    GithubForwardKind.FlagDrift, e.PbiId, mapping.IssueNumber, e.Title, ProposedBody(e),
                    GithubIssueLabels.For(e, issue.Labels), null, null, anchor,
                    $"DRIFT-SPERRE: Issue #{mapping.IssueNumber} wurde seit dem letzten eigenen Write MANUELL editiert — "
                    + "kein Update-Vorschlag, der menschliche Edit ginge verloren. Erst ernten (github-inbound) — "
                    + "oder HIER bewusst aufloesen (Entscheid 'overwrite': schreibt die Core-Projektion und stempelt neu; "
                    + "nur waehlen, wenn der Edit geerntet/entschieden ist).",
                    "deterministic"));
            }
            else
            {
                // In-Sync-Erkennung (Autor-Fund 13.08. nach dem Voll-Stempel-Lauf): OHNE Inhalts-Vergleich schlug
                // jeder Forward fuer JEDE gemappte PBI ewig UPDATE vor — der Plan war kein Drift-Report, und
                // idempotente Re-Writes churnten die Issues. Der Render ist deterministisch und der Apply stempelt
                // EXAKT den gesendeten Render (Compute(op.Title/Body)) — also gilt: Drift==None (Stempel==Snapshot,
                // kein Mensch-Edit) UND Render-Hash==Stempel (Core unveraendert) ⇔ vollstaendig in Sync ⇒ NoChange.
                // Drift 'Unknown' (Alt-Mapping ohne Stempel) bleibt bewusst UPDATE — der Write heilt/stempelt (C2a-3).
                var drift = GithubDriftCheck.Check(mapping, issue);
                var body = ProposedBody(e);
                if (drift == GithubDrift.None
                    && GithubProjectionHash.Compute(e.Title) == mapping.ProjectedTitleHash
                    && GithubProjectionHash.Compute(body) == mapping.ProjectedBodyHash)
                {
                    deterministic.Add(new GithubForwardOp(
                        GithubForwardKind.NoChange, e.PbiId, mapping.IssueNumber, null, null, null, null, null, anchor,
                        $"In Sync: Issue #{mapping.IssueNumber} entspricht exakt der aktuellen Core-Projektion — nichts zu schreiben.",
                        "deterministic"));
                    continue;
                }

                // Projektions-Nachzug ④ (20.08., löst die R-30-Enge sauber): Labels werden am UPDATE
                // bewusst als VOLLE Liste gesetzt — berechnet aus Status-Achsen + MERGE mit den
                // Fremd-Labels des Snapshots (GithubIssueLabels). Vorher galt null = "nicht anfassen",
                // wodurch die Juli-Labels (needs-clarify etc.) für immer einfroren und logen.
                var unknownNote = drift == GithubDrift.Unknown
                    ? " · Hinweis: noch kein Drift-Stempel (Alt-Issue) — manueller Edit waere aktuell nicht erkennbar; dieser Write stempelt."
                    : "";
                deterministic.Add(new GithubForwardOp(
                    GithubForwardKind.UpdateIssue, e.PbiId, mapping.IssueNumber, e.Title, body,
                    GithubIssueLabels.For(e, issue.Labels),
                    null, null, anchor,
                    "PBI hat sich geaendert — vorgeschlagener Patch/Kommentar (kein Auto-Overwrite)." + unknownNote, "deterministic"));
            }
        }

        return new GithubForwardSeedResult(deterministic, unmapped);
    }

    // Vorgeschlagener Issue-Body aus dem Core-Zustand (Beleg, kein freier Text) — C2a-1: geteilte
    // Struktur-Naht GithubIssueTemplate (Render+Parse als Paar, §12 c2-inbound-plan).
    // ⑥ Sync-Metadaten-Klartext (Autor-Fund 20.08., #45): interne Reibungs-Log-Codes gehören ins
    // Runbook, nicht in ein nach außen sichtbares Issue.
    private static string ProposedBody(GithubSyncEntry e)
        => GithubIssueTemplate.Render(e, "Automatisches Update aus dem Projekt-Backlog");
}

public sealed record GithubForwardSeedResult(
    IReadOnlyList<GithubForwardOp> DeterministicOps,
    IReadOnlyList<GithubSyncEntry> UnmappedPbis);
