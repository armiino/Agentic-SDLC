namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// R-11 A1a (05.08.) — die ASPEKT-NAHT der Ingestion (DerivationSpec-Muster, r11-entscheidung Teil 4 + 5/D-9):
/// alles Aspekt-Spezifische des Tor-1-Kerns als DATEN. Der Match-/Gate-/Apply-Fluss ist aspekt-blind;
/// der Requirement-Pfad = „Kern mit diesem Profil". Ein neuer Aspekt (arch, A1d) = eine weitere Instanz —
/// KEINE Code-Duplikation. Die Registry (Profil→Zweig) kommt mit dem Router (A1b).
/// </summary>
/// <remarks>
/// Bit-gleich-Garantie A1a-1: die <see cref="Requirement"/>-Instanz reproduziert Tool-Namen, -Beschreibungen,
/// Task-Text, Filter und ID-Präfix EXAKT (Golden-Test). Querschnitte bleiben bewusst außerhalb des Profils:
/// der 9g-Fragen-Pfad (kind-basiert, Teil 5/D-2), `list_open_decisions` und `search_rejections` (E-R3 entscheidet
/// je Aspekt beim A1d-Bau, ob sie ins arch-Profil wandern).
/// </remarks>
public sealed record AspectIngestionProfile(
    string Aspect,            // itemType-Filterwert der Auswahl-Seite ("requirement")
    string IdPrefix,          // NEU-ID-Präfix im Apply ("REQ" -> REQ-<n>)
    string ItemLabel,         // Anzeige-/Beschreibungs-Label ("Requirement") — Templates ergeben req-bit-gleiche Texte
    string AgentName,         // PromptProvider-Ordner (System-Prompt)
    string PromptName,        // PromptProvider-Datei
    string ListCoreToolName,  // Tool-Name der Kandidaten-Liste (Vertrag mit dem Prompt!)
    string ResolverTaskText,  // die User-Task des Resolvers (Maker + wörtlich im Repair)
    string ExecutorIdPrefix,  // A1d: eindeutige Executor-IDs je Strip (req-IDs bleiben WÖRTLICH die heutigen!)
    string EventPrefix,       // Audit-Ehrlichkeit: Event-Typen je Strip (req WÖRTLICH "REQ_INGEST", arch "ARCH_INGEST")
    string GateName,          // R-50: das Human-Gate dieses Strips ("ingest-gate"/"arch-ingest-gate") — Leer-Skip-Event/Id
    // 9i (09.08.): die 9g-Fragen-Spur gehört GENAU EINEM Strip (E-R3 „arch ohne Fragen-Block" als Daten) —
    // sonst müsste bei gemischten Deltas (req+arch+Fragen) JEDER Strip die Fragen covern und der
    // identitäts-freie OpenQuestion-Apply würde doppelt prägen (Doppel-DEC).
    bool CarriesQuestionLane)
{
    /// <summary>Das heutige Verhalten 1:1 als Daten (Golden-Referenz).</summary>
    public static readonly AspectIngestionProfile Requirement = new(
        Aspect: "requirement",
        IdPrefix: "REQ",
        ItemLabel: "Requirement",
        AgentName: "RequirementIngestionAgent",
        PromptName: "RequirementIngestionAgent1",
        ListCoreToolName: "list_core_requirements",
        ResolverTaskText: IngestionResolveTask.Text,
        ExecutorIdPrefix: "RequirementIngestion",
        EventPrefix: "REQ_INGEST",
        GateName: "ingest-gate",
        CarriesQuestionLane: true);

    /// <summary>R-11 A1d (05.08.) — der zweite Aspekt: Architektur als vollwertiger Tor-1-Bürger.
    /// Vokabular bewusst OHNE NEW_RELATED (featureKey = req-Semantik) und OHNE Fragen-Block (E-R3);
    /// CONTRADICT erlaubt — arch-Widersprüche münden als DEC in den EINEN Topf (decision-gate).</summary>
    public static readonly AspectIngestionProfile Architecture = new(
        Aspect: "architecture",
        IdPrefix: "ARCH",
        ItemLabel: "Architektur",
        AgentName: "ArchitectureIngestionAgent",
        PromptName: "ArchitectureIngestionAgent1",
        ListCoreToolName: "list_core_architecture",
        ResolverTaskText: """
                          Loese die eingehenden Architektur-Aussagen dieses Meetings gegen den bestehenden Core auf.
                          1. get_incoming_items · 2. list_core_architecture / search_core / get_core_entity.
                          2b. search_rejections: Wurde etwas inhaltlich Gleiches frueher ABGELEHNT? Wenn ja,
                              relatedRejectionId (REJ-*) auf der Operation setzen — die Operation trotzdem
                              normal vorschlagen, die Entscheidung trifft der Mensch am Gate.
                          2c. QUER-SICHT (nur lesen): list_core_requirements zeigt die Wahrheit des ANDEREN
                              Aspekts. Widerspricht eine eingehende Aussage einer AKTIVEN Anforderung, nutze
                              CONTRADICT mit deren entityId — NIE stilles NEW daneben. RESTATE/REFINE/SUPERSEDE
                              bleiben strikt architektur-intern.
                          3. Je eingehendem Item GENAU EINE Operation:
                             RESTATE/REFINE/SUPERSEDE/CONTRADICT (mit targetEntityId) · NEW (ohne targetEntityId)
                             · ALREADY_DECIDED (DEC-*). KEIN NEW_RELATED (featureKey ist Requirement-Semantik).
                             Vor CONTRADICT: list_open_decisions pruefen (sonst ALREADY_DECIDED).
                          4. check_state_change_plan (muss pass sein), dann save_state_change_plan (genau einmal).
                          Beleg-Pflicht: claimIds je Operation; im Zweifel NEW statt raten.
                          """,
        ExecutorIdPrefix: "ArchitectureIngestion",
        EventPrefix: "ARCH_INGEST",
        GateName: "arch-ingest-gate",
        CarriesQuestionLane: false);

    /// <summary>② E-R4 (06.08.): der jeweils ANDERE Wahrheits-Aspekt — speist die Quer-LESE-Sicht des
    /// Resolvers (Cross-CONTRADICT). Bewusst als Paar req↔arch; ein dritter Wahrheits-Aspekt bräuchte
    /// hier eine echte Registry (dann aufbohren, nicht raten).</summary>
    public AspectIngestionProfile TruthPartner => ReferenceEquals(this, Architecture) ? Requirement : Architecture;

    public bool Matches(AgenticSdlc.Host.FullWorkflow.Delta.ProjectStateItem item)
        => string.Equals(item.ItemType, Aspect, StringComparison.OrdinalIgnoreCase);
}
