using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// Inc 1c-3: incrementeller PBI-Update. Aus der affected-items-view + dem Ingestion-Delta werden NUR betroffene
// PBIs gezielt aktualisiert (stabile IDs). Sparse: unberuehrte PBIs erscheinen NICHT im Plan.
public sealed record PbiStateChangePlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceIngestionRun")] string SourceIngestionRun,
    [property: JsonPropertyName("operations")] IReadOnlyList<PbiStateChangeOperation> Operations,
    // R-26-C (A1, additiv/abwaertskompatibel): Angleichungs-Vorschlaege je betroffenem PBI (MARK_CHANGED/
    // SUPERSEDE). Alte Plaene ohne dieses Feld => null. Der Mensch autorisiert/editiert im PBI-Update-Review;
    // der Apply uebernimmt nur die AKZEPTIERTEN und setzt needs_clarify -> active.
    [property: JsonPropertyName("alignments")] IReadOnlyList<PbiAlignment>? Alignments = null)
{
    public const int CurrentSchemaVersion = 1;
}

// R-26-C: Vorschlag zur inhaltlichen Angleichung eines PBI an eine geaenderte Anforderung. Leeres Feld =
// Original behalten. Der Agent erzeugt den Vorschlag; die menschliche Fassung (accept/edit) gilt.
public sealed record PbiAlignment(
    // align/extend: das Ziel-PBI (echte Core-ID). create (NEW_PBI): null — das PBI existiert noch nicht.
    [property: JsonPropertyName("pbiId")] string? PbiId,
    [property: JsonPropertyName("proposedTitle")] string? ProposedTitle,
    [property: JsonPropertyName("proposedStatement")] string? ProposedStatement,
    [property: JsonPropertyName("proposedAcceptanceCriteria")] IReadOnlyList<string>? ProposedAcceptanceCriteria,
    [property: JsonPropertyName("rationale")] string Rationale,
    // Welche geaenderten/ersetzten Requirements diese Angleichung ausgeloest haben (Provenance + Review-Kontext).
    [property: JsonPropertyName("triggerRequirementIds")] IReadOnlyList<string> TriggerRequirementIds,
    // O3b (create/NEW_PBI, Variante 2): das Draft-Ziel, wenn noch KEIN PBI existiert. PbiId bleibt dann null;
    // das Draft wird ueber die Ziel-Requirement-ID an die NEW_PBI-Operation gebunden. KEIN Fake-PbiId.
    [property: JsonPropertyName("targetRequirementId")] string? TargetRequirementId = null,
    [property: JsonPropertyName("targetFeatureId")] string? TargetFeatureId = null,
    // Slice S Teil 2 (⚖ Agent-Vorschlag beim Entstehen): Prio/Schätzung des Drafts (Speicher-Form high|medium|low
    // bzw. S|M|L). Leer = kein Vorschlag; ungültige Werte werden im Apply verworfen (nie stiller Müll im Core).
    [property: JsonPropertyName("proposedPriority")] string? ProposedPriority = null,
    [property: JsonPropertyName("proposedEstimate")] string? ProposedEstimate = null)
{
    // Einheitlicher Draft-Schluessel: bestehendes PBI (align/extend) ODER die NEW_PBI-Ziel-Requirement (create).
    [System.Text.Json.Serialization.JsonIgnore]
    public string DraftKey => PbiId ?? TargetRequirementId ?? "";
}

// B1/B2 (Fall-C-Bündelung, Lösungsweg B): der Mensch hat im Review die Feature-Zuordnung eines NEW_PBI korrigiert.
// Getragen von der Entscheidung bis zum Apply, der die Op VOR dem Schreiben umschreibt. Genau EINES der Ziele ist
// gesetzt: FeatureId (B1: bestehendes Feature) ODER ProposedFeatureLabel (B2: ein im selben Plan vorgeschlagenes
// NEUES Feature — der Apply konvertiert die Op dann zu NEW_FEATURE, damit die Label-Gruppierung greift).
// Abwärtskompatibel: kein Override => Vorschlag des Agenten gilt.
public sealed record PbiFeatureOverride(
    [property: JsonPropertyName("opId")] string OpId,
    [property: JsonPropertyName("featureId")] string? FeatureId = null,
    [property: JsonPropertyName("proposedFeatureLabel")] string? ProposedFeatureLabel = null);

public sealed record PbiStateChangeOperation(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("pbiId")] string? PbiId,                    // Ziel-PBI (EXTEND/MARK/BLOCK/SUPERSEDE)
    [property: JsonPropertyName("featureId")] string? FeatureId,            // NEW_PBI
    [property: JsonPropertyName("replacementRequirementId")] string? ReplacementRequirementId, // SUPERSEDE_PBI
    [property: JsonPropertyName("openDecisionRef")] string? OpenDecisionRef, // BLOCK_PBI
    [property: JsonPropertyName("rationale")] string Rationale,
    [property: JsonPropertyName("proposedFeatureLabel")] string? ProposedFeatureLabel = null, // O4: NEW_FEATURE
    // C4b (09.08., c4-plan §8): Autor-Antwort als EIGENE Herkunft — NIE als Fake-Requirement verkleidet.
    // Gesetzt nur vom Klärungs-Sweep: Ref `chat:<session>#<n>` + der wörtliche Antwort-Text (Review-Note).
    [property: JsonPropertyName("authorAnswerRef")] string? AuthorAnswerRef = null,
    [property: JsonPropertyName("authorAnswerText")] string? AuthorAnswerText = null,
    // Slice S Teil 2: der Wert einer SET_PRIORITY/SET_ESTIMATE-Op (Speicher-Form: high|medium|low bzw. S|M|L).
    // Für Set-Ops ist requirementId leer — sie ändern kein Deckungs-Verhältnis, nur ein Feld.
    [property: JsonPropertyName("value")] string? Value = null);

public sealed record PbiUpdateGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<PbiUpdateGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<PbiUpdateGateIssue> Warnings);

public sealed record PbiUpdateGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("requirementId")] string? RequirementId,
    [property: JsonPropertyName("pbiId")] string? PbiId,
    // R7: reparierbar = Platzierungs-Agent kann per GateFeedback fixen; sonst hard/needs_human. Loop-Primitive: Core.
    [property: JsonPropertyName("repairability")] string Repairability = Core.Repairability.Hard);

public static class PbiUpdateKind
{
    public const string NewPbi = "NEW_PBI";
    public const string ExtendPbi = "EXTEND_PBI";
    public const string MarkChanged = "MARK_CHANGED";
    public const string BlockPbi = "BLOCK_PBI";
    public const string SupersedePbi = "SUPERSEDE_PBI";
    // O4 (Fall C): neues Requirement passt in kein bestehendes Feature -> neues Feature + erstes PBI (atomar,
    // via CoreBacklogSeeder-Adapter im Apply). Der PBI-Inhalt kommt aus dem O3b-Create-Draft (TargetRequirementId).
    public const string NewFeature = "NEW_FEATURE";
    // Slice S Teil 2 (⚖ 21.08.): reine FELD-Pflege an einem bestehenden PBI (priority/estimate) — gated wie
    // jede Wahrheits-Mutation, aber OHNE Status-Wirkung (kein needs_clarify: Feld-Pflege ist keine Klärungs-Lage).
    public const string SetPriority = "SET_PRIORITY";
    public const string SetEstimate = "SET_ESTIMATE";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal) { NewPbi, ExtendPbi, MarkChanged, BlockPbi, SupersedePbi, NewFeature, SetPriority, SetEstimate };

    // Operationen mit Ziel-PBI (bestehend).
    public static readonly IReadOnlySet<string> RequirePbi =
        new HashSet<string>(StringComparer.Ordinal) { ExtendPbi, MarkChanged, BlockPbi, SupersedePbi, SetPriority, SetEstimate };

    // Slice S Teil 2: die Feld-Setz-Ops (Wert Pflicht + valide; Wertebereich = Core.PbiFields).
    public static readonly IReadOnlySet<string> FieldSet =
        new HashSet<string>(StringComparer.Ordinal) { SetPriority, SetEstimate };

    // Platzierung neuer Requirements (agentisch): EXTEND (bestehendes PBI) · NEW_PBI (bestehendes Feature) ·
    // NEW_FEATURE (kein passendes Feature).
    public static readonly IReadOnlySet<string> Placement =
        new HashSet<string>(StringComparer.Ordinal) { NewPbi, ExtendPbi, NewFeature };
}

// PBI-Status-Praezedenz beim Merge mehrerer Ursachen (MULTI_CAUSE_MERGE).
public static class PbiStatus
{
    public const string Active = "active";
    public const string NeedsClarify = "needs_clarify";
    public const string BlockedByDecision = "blocked_by_decision";
    public const string Superseded = "superseded";
    // §5-S5: der Max/Rank-Hack (Blocker + Gültigkeit in EIN ranged Feld gequetscht) ENTFERNT — ersetzt durch die
    // typisierte Achsen-Eskalation `CoreStatus.Escalate` (verhaltensgleich, gegen das alte Ranking getestet). Die
    // Konstanten bleiben, solange Creation-Sites den Alt-String erzeugen (bis S7).
}
