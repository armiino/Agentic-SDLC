using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Delta;

// =====================================================================================================================
// §5 Statusmodell-Refactor — die EINE zentrale Autorität für den Core-Lifecycle-Status.
//
// WARUM: Heute steckt der Item-Status in EINEM rohen String-Feld (`ProjectStateItem.Status`), das mehrere Fragen
// vermischt (gilt? · Fortschritt? · wer bestätigt?) + einen Blocker (needs_clarify/blocked_by_decision, per PbiStatus.Max-
// Hack). Verstreut über ~30 Dateien mit rohen String-Vergleichen (Tippfehler erst zur Laufzeit sichtbar). Zwei Werte
// (baseline/accepted/active) bedeuten DASSELBE ("aktiv"), nur mit anderer Governance. Und "done" (Arbeit fertig) wird mit
// "superseded" (Wahrheit ersetzt) in einen Topf geworfen. Details + Bau-Plan: docs/aktiv/status-modell-refactor.md.
//
// WO: bei ProjectStateItem (04-delta), das es typisiert — Delta bleibt self-contained (keine Abhängigkeit auf Core;
// Core → Delta einseitig).
//
// STAND (S1/S2): Enums + Mapping + die additiven Item-Felder existieren; Alt-`Status`-String bleibt bis S7 die QUELLE.
// From/ToLegacy dienen S3–S6 als transitorisches Lazy-Derive und fliegen mit dem Alt-Feld in S7 raus (kein bleibender Adapter).
//
// SPRACHE: Enum-/Identifier englisch (Projektregel); Doc-Texte + Labels deutsch (Inhalt).
// SCOPE: ausschließlich ProjectStateItem.Status. NICHT: Ledger-Facetten-Status, GitHub-Issue-Status, ProjectStateProposal.Status.
// =====================================================================================================================

/// <summary>① Gültigkeit — gilt das Item noch? Alle Item-Typen. („löschen" = superseded, Historie bleibt.)</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Validity
{
    /// <summary>Aktuell gültige Wahrheit.</summary>
    Active,
    /// <summary>Durch eine neuere Fassung ersetzt — keine Wahrheit mehr, aber als Historie erhalten.</summary>
    Superseded,
}

/// <summary>② Fortschritt — wie weit ist die Umsetzung? NUR PBIs. Orthogonal zur <see cref="Validity"/>
/// (ein done-PBI ist weiterhin gültig — done ≠ superseded).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Progress
{
    /// <summary>Arbeit noch offen.</summary>
    Open,
    /// <summary>Arbeit abgeschlossen (entsteht nur über verifiziertes GithubReverse PBI_DONE, E4).</summary>
    Done,
    // in_progress bewusst (noch) nicht modelliert — heute 0× genutzt; billig nachrüstbar, wenn gebraucht.
}

/// <summary>Blocker — ein PBI ist zwar <see cref="Validity.Active"/>, kann aber NICHT weiter. NUR PBIs.
/// Eigenes Feld statt des alten PbiStatus.Max-Hacks (der Blocker und Gültigkeit in ein Feld quetschte).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Blocker
{
    /// <summary>Kein Blocker.</summary>
    None,
    /// <summary>Anforderung geändert, PBI-Inhalt noch nicht angeglichen (R-26-C löst das auf).</summary>
    NeedsClarify,
    /// <summary>Wartet auf eine offene Entscheidung (Widerspruch) — es entsteht kein GitHub-Issue (HOLD).</summary>
    BlockedByDecision,
}

/// <summary>Decision-Lebenszyklus — NUR DEC-Items (Widersprüche). CONTRADICT mintet <see cref="Open"/>, Tor 2 löst zu
/// <see cref="Resolved"/>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DecisionState
{
    /// <summary>Offene Entscheidung — blockiert die betroffenen PBIs, bis Tor 2 sie auflöst.</summary>
    Open,
    /// <summary>Durch Tor 2 aufgelöst.</summary>
    Resolved,
}

/// <summary>③ Governance-Marker — WIE das Item zu seiner Gültigkeit kam. Orthogonal zur Gültigkeit; speist die
/// Thesis-Autorisierungs-Metrik (human vs. auto). Die Info existiert heute implizit (baseline=agent, accepted=human) —
/// weglassen wäre Datenverlust (E-10).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Confirmation
{
    /// <summary>Ein Mensch hat es autorisiert (Alt-Status `accepted`).</summary>
    Human,
    /// <summary>Ein Agent hat es erzeugt/verankert (Alt-Status `baseline`).</summary>
    Agent,
    /// <summary>Governance nicht aufgezeichnet (Alt-Status `active` — trug nie einen Marker; ehrlich unmarkiert, nicht erfunden).</summary>
    Unmarked,
}

/// <summary>
/// Der Core-Lifecycle-Status als saubere, getrennte Achsen — die typisierte Ablösung des einen rohen
/// <c>ProjectStateItem.Status</c>-Strings. Value-Object (Record); die Felder hängen additiv am Item (S2).
/// </summary>
/// <param name="Validity">① gilt es noch (alle Typen).</param>
/// <param name="Progress">② Umsetzung (nur PBI; sonst null).</param>
/// <param name="Blocker">Blocker (nur PBI; sonst <see cref="Delta.Blocker.None"/>).</param>
/// <param name="ConfirmedBy">③ wer bestätigt hat.</param>
/// <param name="ConfirmedInRun">Verweis auf den autorisierenden Lauf (= das vorhandene sourceRunId; Audit, E-10).</param>
/// <param name="Decision">Decision-Lebenszyklus (nur DEC-Items; sonst null).</param>
public sealed record CoreStatus(
    Validity Validity,
    Progress? Progress,
    Blocker Blocker,
    Confirmation ConfirmedBy,
    string? ConfirmedInRun,
    DecisionState? Decision)
{
    /// <summary>Fällt das Item aus der aktiven Sicht? (ersetzt CoreViews.Archived = {done, superseded, retired}) —
    /// jetzt achsen-explizit: ersetzte Wahrheit ODER fertige Arbeit. `retired` gestrichen (nie gesetzt).</summary>
    public bool IsArchived => Validity == Validity.Superseded || Progress == Delta.Progress.Done;

    /// <summary>Ist das eine offene Entscheidung? (ersetzt CoreViews.IsOpenDecision = Status=="open_decision").</summary>
    public bool IsOpenDecision => Decision == DecisionState.Open;

    /// <summary>Eskaliert einen Blocker auf diesen Status — die typisierte, verhaltensgleiche Ablösung des alten
    /// <c>PbiStatus.Max</c>-Hacks (S5). Rang: BlockedByDecision(3) > Superseded(2) > NeedsClarify(1) > aktiv(0). Der
    /// stärkere gewinnt; bei Eskalation bleiben Governance/Fortschritt/Audit erhalten (nur Gültigkeit/Blocker ändern sich).</summary>
    public CoreStatus Escalate(Blocker added)
    {
        static int Rank(Validity v, Blocker b)
            => b == Blocker.BlockedByDecision ? 3 : v == Validity.Superseded ? 2 : b == Blocker.NeedsClarify ? 1 : 0;
        var addedRank = added == Blocker.BlockedByDecision ? 3 : added == Blocker.NeedsClarify ? 1 : 0;
        return addedRank > Rank(Validity, Blocker)
            ? this with { Validity = Validity.Active, Blocker = added }
            : this;
    }

    // -- Verlustfreies Mapping alt↔neu. Round-Trip alt→neu→alt ist bit-gleich für die 5 heutigen + 2 operativen Werte
    //    (Test: CoreStatusMappingTests). Transitorisches Lazy-Derive für S3–S6; fliegt mit dem Alt-Feld in S7 raus.

    /// <summary>Parst einen Alt-<c>Status</c>-String (+ optional sourceRunId für den Audit-Verweis) in die Achsen.
    /// Wirft bei unbekanntem Wert (LAUT statt still falsch mappen — wichtig für die Migration).</summary>
    public static CoreStatus From(string legacyStatus, string? sourceRunId = null)
    {
        var s = (legacyStatus ?? string.Empty).Trim().ToLowerInvariant();
        return s switch
        {
            // ① Aktiv, unterschieden nur durch Governance:
            "baseline"            => new(Validity.Active,     null,                Blocker.None,              Confirmation.Agent,    sourceRunId, null),
            "accepted"            => new(Validity.Active,     null,                Blocker.None,              Confirmation.Human,    sourceRunId, null),
            "active"              => new(Validity.Active,     null,                Blocker.None,              Confirmation.Unmarked, sourceRunId, null),
            // ② / Blocker (PBI):
            "needs_clarify"       => new(Validity.Active,     Delta.Progress.Open, Blocker.NeedsClarify,      Confirmation.Unmarked, sourceRunId, null),
            "blocked_by_decision" => new(Validity.Active,     Delta.Progress.Open, Blocker.BlockedByDecision, Confirmation.Unmarked, sourceRunId, null),
            "done"                => new(Validity.Active,     Delta.Progress.Done, Blocker.None,              Confirmation.Unmarked, sourceRunId, null),
            // ① Ersetzt:
            "superseded"          => new(Validity.Superseded, null,                Blocker.None,              Confirmation.Unmarked, sourceRunId, null),
            // DEC-Lebenszyklus:
            "open_decision"       => new(Validity.Active,     null,                Blocker.None,              Confirmation.Agent,    sourceRunId, DecisionState.Open),
            "resolved"            => new(Validity.Active,     null,                Blocker.None,              Confirmation.Unmarked, sourceRunId, DecisionState.Resolved),
            _ => throw new ArgumentOutOfRangeException(nameof(legacyStatus), legacyStatus,
                     "Unbekannter Alt-Status — Mapping fehlt (§5/status-modell-refactor.md §2). Bewusst LAUT statt still falsch mappen."),
        };
    }

    /// <summary>Rekonstruiert den Alt-<c>Status</c>-String aus den Achsen (Round-Trip-Beweis der Verlustfreiheit).
    /// Reihenfolge der Prüfung ist bewusst (Decision → Blocker → Superseded → Done → aktiv-nach-Governance).</summary>
    public string ToLegacyString() =>
        Decision == DecisionState.Open       ? "open_decision"
      : Decision == DecisionState.Resolved   ? "resolved"
      : Blocker == Blocker.BlockedByDecision ? "blocked_by_decision"
      : Blocker == Blocker.NeedsClarify      ? "needs_clarify"
      : Validity == Validity.Superseded      ? "superseded"
      : Progress == Delta.Progress.Done      ? "done"
      : ConfirmedBy == Confirmation.Agent    ? "baseline"
      : ConfirmedBy == Confirmation.Human    ? "accepted"
      :                                        "active";

    /// <summary>Bequemer Einstieg vom Item (nutzt dessen Alt-<c>Status</c> + <c>SourceRunId</c>). Solange bis S3+ die
    /// typisierten Felder am Item gefüllt sind — danach entfällt der Umweg über den Alt-String.</summary>
    public static CoreStatus From(ProjectStateItem item) => From(item.Status, item.SourceRunId);
}

/// <summary>Schreib-Naht für den Status (S3). EINE Stelle, die den Status eines Items ändert — setzt die neuen
/// Achsen-Felder UND (transitorisch bis S7) den Alt-<c>Status</c>-String synchron via <see cref="CoreStatus.ToLegacyString"/>.
/// So bleiben Leser auf dem Alt-String (bis S4) und auf den neuen Feldern (ab S4) konsistent. In S7 entfällt der Alt-Teil.</summary>
public static class CoreStatusWrite
{
    /// <summary>Ändert den Status eines bestehenden Items auf <paramref name="status"/> — beide Repräsentationen synchron.
    /// Mit <paramref name="historyReason"/> wird die VORHERIGE Fassung als History-Eintrag festgehalten (schließt die
    /// E-10-Audit-Lücke: jeder Statuswechsel hinterlässt eine Notiz).</summary>
    public static ProjectStateItem WithStatus(this ProjectStateItem item, CoreStatus status, string? historyReason = null)
    {
        var history = historyReason is null
            ? item.History
            : (item.History ?? []).Append(new ProjectStateItemVersion(
                  item.Version, item.Text, item.Status, item.Origin, item.SourceRunId, item.SourceClaimIds,
                  DateTime.UtcNow, historyReason)).ToList();

        return item with
        {
            Status = status.ToLegacyString(),   // transitorisch (bis S7) — hält Alt-String-Leser synchron
            Validity = status.Validity,
            Progress = status.Progress,
            Blocker = status.Blocker,
            ConfirmedBy = status.ConfirmedBy,
            ConfirmedInRun = status.ConfirmedInRun,
            Decision = status.Decision,
            History = history,
        };
    }
}

/// <summary>Lese-Naht für den Status (S4) — Gegenstück zu <see cref="CoreStatusWrite.WithStatus"/>. EINE Stelle, die den
/// Status eines Items LIEST: typisierte Felder wenn gesetzt (ab S3/Migration S6), sonst transitorisch aus dem Alt-String
/// abgeleitet (<see cref="CoreStatus.From"/>). Read-Sites fragen <c>item.ReadStatus().Blocker</c> statt roher String-Vergleiche.
/// In S7 (nach Migration sind die Felder immer gesetzt) entfällt der From-Fallback.</summary>
public static class CoreStatusRead
{
    /// <summary>Liefert den Status als typisierte Achsen — aus den neuen Feldern, sonst (un-migriert) aus dem Alt-String.</summary>
    public static CoreStatus ReadStatus(this ProjectStateItem item)
        => item.Validity is { } v
            ? new CoreStatus(v, item.Progress, item.Blocker ?? Blocker.None,
                             item.ConfirmedBy ?? Confirmation.Unmarked, item.ConfirmedInRun, item.Decision)
            : CoreStatus.From(item.Status, item.SourceRunId);
}

// (S1 hatte hier eine zentrale `CoreStatusLabels`-Klasse angelegt — in S4 (01.08.) entfernt: die Review-Adapter-Glossare
//  sind kontext-spezifisch [PbiUpdate „noch nicht angeglichen" vs. Forward „geparkt/HOLD"], keine zu zentralisierende
//  Dublette, und keine item.Status-Reads. Damit war die Klasse ungenutzt → sauber raus statt toten/vorschnellen Code lassen.)
