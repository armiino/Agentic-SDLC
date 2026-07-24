using System.Text.Json;

namespace AgenticSdlc.Host.Configuration;

/// <summary>
/// Beschreibt die public Runtime-config eines Runs.
/// </summary>
/// <remarks>
/// Diese Datei trennt bewusst experimentelle Run-Parameter von lokalen Secrets.
/// Phase, Modell, Provider und Observability-Flags sind Forschungsparameter und
/// sollen deshalb in `run-config.json` sichtbar und referenzierbar sein.
/// API Keys und lokale Endpunkte bleiben dagegen weiterhin in `.env` oder echten Umgebenungsvar

/// </remarks>
public sealed class RunConfig
{
    public string? AgentPhase { get; set; }
    public string? Phase2ContextStrategy { get; set; }
    public string? LlmProvider { get; set; }
    public string? AgentModel { get; set; }

    /*
     * Prompt-Auswahl, gruppiert nach Phase -> Strategie -> Agent -> Promptname.
     *
     *   "prompts": {
     *     "phase1":    { "default":         { "Phase1SinglePass": "Phase1_3Prompt" } },
     *     "phase2_1":  { "message_passing": { "Phase2ContextAgent": "ContextPrompt3", ... },
     *                    "artifact_state":  { "Phase2ContextAgent": "ContextPrompt3", ... } }
     *   }
     *
     * Vorteil: A<->B wird allein durch "phase2ContextStrategy" umgeschaltet — kein manuelles
     * Tauschen einzelner Agent-Prompts mehr. Phase 1 nutzt die Pseudo-Strategie "default",
     * damit die Struktur einheitlich 3-stufig bleibt.
     *
     * Aufgelöst wird das in HostSettings.BuildPromptSelection zu einem flachen Agent->Name-Dict;
     * alle Aufrufer fragen weiterhin generisch über GetPromptName(agentName).
     */
    public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Prompts { get; set; }
        = new(StringComparer.Ordinal);

    public ObservabilityConfig Observability { get; set; } = new();
    public LlmPreviewConfig LlmPreview { get; set; } = new();
    public JuryConfig Jury { get; set; } = new();
    public Phase2BStateConfig Phase2BState { get; set; } = new();
    public LedgerConfig Ledger { get; set; } = new();
    public EvidenceAgentConfig EvidenceAgent { get; set; } = new();
    public L3Config L3 { get; set; } = new();
    // W1e': nur vom pipeline-full-Runner gelesen. Ganze Kette als EIN durabler Graph.
    public FullWorkflowConfig FullWorkflow { get; set; } = new();

    /// <summary>
    /// Lädt `run-config.json` aus dem Repo-Root
    /// </summary>
    public static RunConfig Load(string repoRoot)
    {
        var path = Path.Combine(repoRoot, "run-config.json");

        if (!File.Exists(path))
            return new RunConfig();

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        return JsonSerializer.Deserialize<RunConfig>(json, options) ?? new RunConfig();
    }
}

public sealed class ObservabilityConfig
{
    public bool? EnableOtel { get; set; }
    public bool? EnableOtelSensitive { get; set; }
    public bool? EnableOtelRaw { get; set; }
    //Pro inner-cycle Reasoning Logging: ChatDecisionLogger innerhalb FunctionInvocation positionieren
    public bool? InnerCycleLogging { get; set; }

    // W1a: max. Zeichen für den response-text.md-Volltext je Runde. 0 = unbegrenzt (Forschungs-Default).
    // Getrennt von LlmPreview.Chars, das NUR die kompakten Event-Previews (CHAT_RESPONSE_TEXT) kappt.
    public int? ResponseTextMaxChars { get; set; }

    // W1a: reasoning-Feld-Steuerung je Lauf: "off" | "optional" | "enforced" (Default enforced).
    // "off" = Baseline-vergleichbar (M-1, 0 Extra-Tokens); "enforced" = garantiert (Fehleranalyse).
    public string? CaptureReasoning { get; set; }
}

/// <summary>
/// W1e': `run-config.fullworkflow` — Konfiguration der GANZEN Kette als EIN durabler MAF-Graph.
/// Roh-POCO (nullable, JSON-Bindung); aufgelöst zu <c>FullWorkflowSettings</c> (Pipeline-Namespace).
/// </summary>
public sealed class FullWorkflowConfig
{
    public string? Transcript { get; set; }
    public string? Repo { get; set; }
    public string? TokenEnv { get; set; }
    // Default false — KEIN externer GitHub-Write; jeder Write braucht explizite Policy. (Gates sind heilig.)
    public bool? Execute { get; set; }
    // "interactive" | "accept-all" | "replay:<pfad>" — Standard-Policy für alle Gates ohne eigenen Eintrag.
    public string? PolicyProfile { get; set; }
    public Dictionary<string, string>? Models { get; set; }      // stufe -> modell (W2-Ablation)
    public Dictionary<string, int>? MaxAttempts { get; set; }    // stufe -> n
    public Dictionary<string, string>? Stages { get; set; }      // z. B. { "l3": "off" } (v1)
    public Dictionary<string, GateConfig>? Gates { get; set; }   // gateName -> { policy }
}

public sealed class GateConfig
{
    public string? Policy { get; set; }   // "interactive" | "accept-all" | "replay:<pfad>"
}

public sealed class LlmPreviewConfig
{
    public int? Chars { get; set; }
}

public sealed class JuryConfig
{
    /// <summary>Jury nach jedem erfolgreichen Run automatisch ausführen.</summary>
    public bool? Enabled { get; set; }

    /// <summary>
    /// Optionales Judge-Modell (zb "openai/gpt-4.1"). Wenn leer, wird das Run-Modell verwendet.
    /// </summary>
    public string? JudgeModel { get; set; }

    /// <summary>
    /// Strukturierte Judge-Ausgabe (response_format = json_schema) erzwingen. Default: true.
    /// Bei Judge-Modellen, die kein json_schema unterstützen (zB manche OpenRouter-Modelle),
    /// auf false setzen-> dann fängt der evaluation_failed-Pfad (B8) ungültiges JSON ab.
    /// </summary>
    public bool? StructuredOutput { get; set; }

    /// <summary>
    /// DISK-12/G3: Call 1 (Finding-Generierung) pro Kategorie in fokussierte Einzel-Judges splitten
    /// (FALSE_CLAIM / FALSE_CERTAINTY / MISSING_TOPIC) statt eines 3-in-1-Calls. Hebt den Recall schwacher Modelle (B20). 
    /// Default: true. false = altes 3-in-1-Verhalten (für Instrument-Vergleich).
    /// </summary>
    public bool? SplitGeneration { get; set; }

    /// <summary>
    /// DISK-12/B22: aktive Fehlerkategorien PRO ARTEFAKTTYP (requirements/risks/architecture/
    /// open-questions/generic) -> Werte aus FALSE_CLAIM / FALSE_CERTAINTY / MISSING_TOPIC. 
    /// Überschreibt den Code-Default (der FALSE_CLAIM für open-questions deaktiviert). Fehlt der Block -> nur Defaults.
    /// Gilt für Generierung UND Verifikation. Landet im Snapshot (reproduzierbare Policy, FORSCH-2).
    /// </summary>
    public Dictionary<string, List<string>>? Categories { get; set; }

    /// <summary>
    /// Pro Fehlerkategorie steuerbarer zweiter Verifikations-Pass (DISK-7). Fehlt der Block,
    /// gelten die konservativen Defaults (missingTopic=true, falseCertainty=true, falseClaim=false).
    /// </summary>
    public JuryVerificationConfig? Verification { get; set; }
}

/// <summary>
/// Schaltet den kategoriespezifischen Verifikations-Pass der Jury pro Fehlerkategorie (DISK-7).
/// </summary>
/// <remarks>
/// Jede aktivierte Kategorie bekommt einen eigenen Batch-Verifikations-Call (nur bei vorhandenen
/// Kandidaten). Die Flags landen im Config-Snapshot, damit A/B/C-Vergleichsruns reproduzierbar
/// dieselbe Jury-Policy verwenden.
/// </remarks>
public sealed class JuryVerificationConfig
{
    /// <summary>FALSE_CLAIM verifizieren. Default: false (teurer Transkript-Re-Read; konservativer Verifier).</summary>
    public bool? FalseClaim { get; set; }

    /// <summary>FALSE_CERTAINTY verifizieren. Default: true (größter False-Positive-Treiber).</summary>
    public bool? FalseCertainty { get; set; }

    /// <summary>MISSING_TOPIC verifizieren. Default: true (bewährt seit v3).</summary>
    public bool? MissingTopic { get; set; }

    /// <summary>
    /// Max. Anzahl MISSING_TOPIC-Kandidaten pro Verifikations-Call (DISK-9). Default: 8.
    /// Größere Batches in mehrere Calls aufteilen (Index-Remapping pro Chunk), weil der Verifier
    /// bei großen Listen die Trennschärfe verliert und auf „fehlt" defaultet (Run 639979: 23/23
    /// fälschlich confirmed). Betrifft NUR MISSING_TOPIC; FALSE_CERTAINTY/FALSE_CLAIM unverändert.
    /// </summary>
    public int? MissingTopicBatchSize { get; set; }

    /// <summary>
    /// Custom-Verifier (Default: false). Wenn true: ein einziger ausgelagerter Prompt
    /// (<c>Prompts/jury/verify-custom.txt</c>) prüft ALLE Kandidaten und ersetzt die drei
    /// eingebauten Verifier. Nur zum Experimentieren/Kalibrieren — nicht für A/B/C-Vergleiche.
    /// </summary>
    public bool? Custom { get; set; }
}

/// <summary>
/// Steuert den config-gesteuerten Human-in-the-Loop-Adjudikationsschritt des Ledger-Workflows (Plan §4).
/// </summary>
/// <remarks>
/// Nur relevant für <c>ledger-build</c>. Fehlt der Block, gilt <c>skip</c> (Baseline-neutral, heutiges
/// Verhalten). <c>manual</c> = queue.json schreiben und enden (Naht); <c>interactive</c> = blockierende
/// lokale Review-UI mit Autosave, „Fertig" wendet direkt an.
/// </remarks>
public sealed class LedgerConfig
{
    /// <summary>skip | manual | interactive. Default (fehlend/unbekannt): skip.</summary>
    public string? AdjudicationMode { get; set; }

    /// <summary>Im Interactive-Modus den Browser automatisch öffnen. Default: true.</summary>
    public bool? AdjudicationOpenBrowser { get; set; }
}

/// <summary>Steuert die L3-Open-World-Ableitung (Human-Review + Kandidaten-Generierung).</summary>
public sealed class L3Config
{
    /// <summary>file | interactive. Default: file (Human-Review-Paket als Datei). interactive = lokale Review-UI.</summary>
    public string? ReviewMode { get; set; }

    /// <summary>Im Interactive-Modus den Browser automatisch öffnen. Default: true.</summary>
    public bool? ReviewOpenBrowser { get; set; }

    /// <summary>selective | exhaustive | research. Default: selective (fokussierte Handvoll). exhaustive = systematische,
    /// ergiebige VERANKERTE Elaboration je Umwelt-Item. research = L3-Über-Agent (Recherche-first: großes Bild erfassen,
    /// erweitern UND Lücken finden; deklariert intent extension|gap + basedOn; impliziert das Provenance-Tool).
    /// CLI-Flags --research/--exhaustive/--selective überschreiben.</summary>
    public string? CandidateMode { get; set; }

    /// <summary>resolve_provenance-Tool für die L3-Agenten aktivieren (Rückverfolgung der Item-Herkunft über die
    /// id-verknüpfte Kette). Default: false (baseline-neutral — die eingefrorenen Gen-Läufe liefen ohne Tool). CLI
    /// --provenance überschreibt. Gegen SourceArtifactSet gebaut → überlebt den späteren DB-Umwelt-Umbau (nur der
    /// Loader wird getauscht).</summary>
    public bool? ProvenanceTool { get; set; }

    /// <summary>Optionaler Pfad zur Ledger-consumable.json, damit resolve_provenance sourceClaimIds bis zum
    /// Claim-Text (→ Evidenz) auflöst statt nur der id. Fehlt er, endet die Kette bei den Claim-ids.</summary>
    public string? LedgerRun { get; set; }
}

/// <summary>
/// Steuert den Evidenz-Agent-Modus (Kapitel B): Artefakt-Generierung aus Ledger vs. Rohtranskript.
/// </summary>
/// <remarks>
/// Nur relevant, wenn <c>agentPhase = phase2_evidence</c>. Fehlt der Block, gelten die Defaults
/// (source=transcript, artifact=requirements). Siehe <c>Phases/Phase2/Evidenz-Agent/ReusePlan.md</c>.
/// </remarks>
public sealed class EvidenceAgentConfig
{
    /// <summary>ledger (Arm B) | transcript (Arm A). Default: transcript.</summary>
    public string? Source { get; set; }

    /// <summary>Zielartefakt. B-Minimal-Bar: nur "requirements". Default: requirements.</summary>
    public string? Artifact { get; set; }

    /// <summary>Pfad zum Roh-Transkript (Arm A). MUSS dasselbe sein, aus dem der Ledger gebaut wurde (Vergleichbarkeit).</summary>
    public string? Transcript { get; set; }

    /// <summary>Nur bei source=ledger: Pfad zur consumable.json des adjudizierten Ledger-Runs.</summary>
    public string? LedgerRun { get; set; }

    /// <summary>k Wiederholungen pro Arm für die Verteilungs-Messung (temp>0). Default: 1.</summary>
    public int? Repetitions { get; set; }
}

/// <summary>
/// Steuert die Shared-State-Policy der Phase-2.1B-Strategie (`artifact_state`).
/// </summary>
/// <remarks>
/// Nur relevant, wenn `phase2ContextStrategy = artifact_state`. Steuert, wie viel/was in den
/// MAF-Shared-State geschrieben und von welchem Specialist gelesen wird.
/// Fehlt der Block, gilt der Default β-full (jeder Specialist liest Context + alle Upstream-Artefakte).
/// </remarks>
public sealed class Phase2BStateConfig
{
    /// <summary>Schreiben die Specialists ihr Artefakt zusätzlich in den Shared State? Default: true.</summary>
    public bool? WriteArtifacts { get; set; }

    /// <summary>
    /// Per-Agent-Lesepolicy: Agentname -> Liste der State-Keys, die der Agent liest
    /// (z.B. "Phase2RisksAgent": ["context","requirements"]). Fehlt der Eintrag, gilt der β-full-Default.
    /// </summary>
    public Dictionary<string, List<string>>? Reads { get; set; }
}
