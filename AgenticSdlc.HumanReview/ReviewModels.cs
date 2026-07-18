using System.Text.Json.Serialization;

namespace AgenticSdlc.HumanReview;

/// <summary>
/// Generische, domaenen-agnostische Human-in-the-Loop-Review-Modelle.
///
/// Diese Schicht weiss NICHTS ueber Ledger, Adjudikation, SDLC oder irgendeine konkrete Domaene.
/// Sie beschreibt nur: „ein Mensch soll eine Liste von Items durchsehen und pro Item ein paar Felder
/// ausfuellen; zu jedem Item kann es lazily nachladbaren Kontext geben". Jede Domaene projiziert ihre
/// eigenen Objekte ueber einen Adapter in eine <see cref="ReviewSession"/> hinein (fuer die
/// Ledger-Adjudikation: <c>LedgerAdjudicationReviewAdapter</c> im Host-Projekt).
///
/// Dadurch ist derselbe Server + dasselbe Frontend spaeter fuer andere Review-Aufgaben nutzbar
/// (Artefakt-Review, Agenten-Output-Freigabe, Facetten-Stichprobe, ...).
/// </summary>
public enum ReviewInputType
{
    /// <summary>Auswahl aus <see cref="ReviewFieldSpec.AllowedValues"/> (Dropdown).</summary>
    Dropdown,
    /// <summary>Freitext-Eingabe.</summary>
    FreeText,
    /// <summary>Mehrzeilige Freitext-Eingabe fuer laengere Texte oder listenartige Werte.</summary>
    MultiLine,
    /// <summary>Nur-Lese-Anzeige (kein Edit; z. B. eine ID/Referenz).</summary>
    Readonly
}

/// <summary>Ein vorschlagbarer Wert für ein Feld (Dropdown-Option oder FreeText-Autocomplete via datalist).</summary>
public sealed record ReviewOption(string Value, string Label);

/// <summary>Optionale UI-Sichtbarkeitsbedingung fuer ein Review-Feld.</summary>
public sealed record ReviewFieldVisibility(
    string FieldKey,
    IReadOnlyList<string> Values);

/// <summary>Beschreibt EIN editierbares Feld pro Item (das Schema gilt session-weit fuer alle Items).</summary>
public sealed record ReviewFieldSpec(
    string FieldKey,
    string Label,
    ReviewInputType InputType,
    IReadOnlyList<string> AllowedValues,
    bool Required,
    string? Help = null,
    IReadOnlyList<ReviewOption>? Options = null,
    ReviewFieldVisibility? VisibleWhen = null);

/// <summary>Konkreter Wert eines Feldes fuer ein Item (mutabel: der Server aktualisiert ihn bei jedem Save).</summary>
public sealed record ReviewFieldValue(string FieldKey, string? Value);

public enum ContextBlockKind { Reference, Excerpt, Quote, Generic }

/// <summary>
/// Ein lazily nachladbarer Kontextblock zu einem Item (Evidenz, Transkript-Ausschnitt, referenzierter
/// Datensatz ...). Der Inhalt wird erst bei Klick ueber <c>GET /api/context/{itemId}/{resolverKey}</c>
/// aufgeloest — die Session bleibt schlank.
/// </summary>
public sealed record ContextBlock(ContextBlockKind Kind, string Label, string ResolverKey);

/// <summary>Art einer Notiz — steuert nur die Darstellung im UI (Callout/Farbe), keine Logik.</summary>
public enum ReviewNoteKind { Info, Suggestion, Reason, Warning }

/// <summary>Eine hervorgehobene, IMMER sichtbare Notiz zu einem Item (z. B. System-Vorschlag, Grund).
/// Im Gegensatz zu <see cref="ContextBlock"/> nicht lazy — kurzer Text, direkt gerendert.</summary>
public sealed record ReviewNote(ReviewNoteKind Kind, string Label, string Text);

/// <summary>Detailansicht zu einer auswählbaren Referenz (z. B. Claim aus einem Katalog). Generisch:
/// Der Core kennt nur Titel, Summary, Notes und lazy ContextBlocks.</summary>
public sealed record ReviewReferenceDetails(
    string ReferenceId,
    string Title,
    string Summary,
    IReadOnlyList<ReviewNote> Notes,
    IReadOnlyList<ContextBlock> ContextBlocks);

/// <summary>Domaenenspezifische Hilfe fuer die generische Review-UI.</summary>
public sealed record ReviewHelp(
    string Title,
    string Summary,
    IReadOnlyList<ReviewHelpSection> Sections);

public sealed record ReviewHelpSection(
    string Title,
    string Text);

/// <summary>Ein zu bearbeitendes Item. Feldwerte + Resolved-Flag sind mutabel (Server-Autosave).</summary>
public sealed class ReviewItem
{
    public required string ItemId { get; init; }
    public required string Summary { get; init; }
    /// <summary>Kurzes Label (z. B. der itemType) fuer Gruppierung/Farbe im UI. Optional.</summary>
    public string? Badge { get; init; }
    /// <summary>Hervorgehobene, direkt sichtbare Notizen (System-Vorschlag, Grund, Warnung ...).</summary>
    public IReadOnlyList<ReviewNote> Notes { get; init; } = [];
    public IReadOnlyList<ContextBlock> ContextBlocks { get; init; } = [];
    public List<ReviewFieldValue> FieldValues { get; set; } = [];
    /// <summary>Wird nach jedem Save neu berechnet (domaenen-spezifisch, s. ReviewServerOptions.RecomputeResolved).</summary>
    public bool Resolved { get; set; }
}

/// <summary>Eine komplette Review-Session: Titel, Feldschema (session-weit) und die Items.</summary>
public sealed class ReviewSession
{
    public required string SessionId { get; init; }
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
    public ReviewHelp? Help { get; init; }
    public IReadOnlyList<ReviewFieldSpec> FieldSchema { get; init; } = [];
    public required IReadOnlyList<ReviewItem> Items { get; init; }

    public bool AllResolved() => Items.All(i => i.Resolved);
    public int ResolvedCount() => Items.Count(i => i.Resolved);
}
