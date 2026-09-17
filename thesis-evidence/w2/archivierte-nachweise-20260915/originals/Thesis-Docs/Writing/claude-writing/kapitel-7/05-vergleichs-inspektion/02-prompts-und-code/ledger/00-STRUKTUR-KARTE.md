# Struktur-Karte: der KOMPLETTE 01-ledger (53 .cs, 1:1-Spiegel vom 07.09.)

> Jede Datei einer von fünf Rollen zugeordnet. „Gemessener Pfad" = exakt die Kette, die
> `ledger-build-units` verdrahtet (verifiziert an `units/LedgerBuildUnitsRunner.cs`
> Zeilen 103–117: sechs LLM-Executors + BuildUnitCoverage-Workflow).

## 1 · Gemessener Pfad — LLM-Stufen (Agent + zugehöriger Executor)

| Stufe | Fach-Logik (Prompt) | Workflow-Executor |
| --- | --- | --- |
| step-01 Extraktion | `units/UnitAwareSemanticLedgerExtractor.cs` | `units/UnitAwareCandidateExtractionExecutor.cs` |
| step-01c Triage | `units/UnusedUnitTriageReviewer.cs` | `units/UnusedUnitTriageExecutor.cs` |
| step-01d Compare | `units/UnusedUnitLedgerComparer.cs` + `units/UnusedUnitReviewer.cs` | `units/UnusedUnitLedgerCompareExecutor.cs` + `units/UnusedUnitReviewExecutor.cs` |
| step-02 Kanonisierung | `core/SemanticLedgerCanonicalizer.cs` | `build/CanonicalizationExecutor.cs` |
| step-02 Repair | `build/CanonicalCoverageRepairer.cs` | `build/CanonicalCoverageRepairExecutor.cs` |
| step-03 Facetten | `build/FacetValidator.cs` (+`adjudication/FacetAssigner.cs` Dispositions-Regeln) | `build/FacetValidationExecutor.cs` |

Alle Prompt-Texte lesbar in `PROMPTS-KLARTEXT.md`.

## 2 · Gemessener Pfad — deterministische Teile

- `units/AtomicUnitSegmenter.cs` + `AtomicUnitSegmentationExecutor.cs` + `AtomicUnitModels.cs` —
  step-00 Segmentierung (im VERGLEICH herauskontrolliert: beide Arme bekamen die fertigen AUs;
  im Ledger-Lauf läuft sie trotzdem — auf dieselbe Quelle)
- `units/UnitCoverageGate.cs` + `UnitCoverageGateExecutor.cs` — step-01b: jede AU verwendet/unbenutzt
- `units/UnusedUnitCompareRepair.cs` — Referenz-Reparatur + ehrlicher Downgrade (Audit-Spur)
- `build/CanonicalCheckExecutor.cs` — der deterministische Kanonisierungs-Check (Verlust-Erkennung)
- `gate/LedgerQualityGate.cs` — finales Quality-Gate (Fehler=0, Traces intakt)
- `units/LedgerSourceUnitTrace.cs` · `units/UnusedUnitTrace.cs` — Nachvollzieh-Traces
- `build/LedgerBuilderWorkflow.cs` — der Graph, der alles verdrahtet ·
  `build/LedgerMessages.cs` · `build/LedgerRunArtifacts.cs` — typisierte Nachrichten/Artefakte
- `units/LedgerBuildUnitsRunner.cs` — CLI-Einstieg des gemessenen Laufs ·
  `units/UnusedUnitTriageModels.cs` · `units/UnusedUnitReviewModels.cs` ·
  `build/FacetValidationModels.cs` — Datenmodelle

## 3 · Messwerkzeuge (erzeugen die Vergleichsdaten, verändern nichts)

- `measure/LedgerCaptureRunner.cs` — erzeugt die L-/LCR-Captures (in `05-output-ledger/`)
- `measure/LedgerValidateRunner.cs` · `LedgerReferenceRecall*.cs` ·
  `FacetValidationEvalRunner.cs` · `LedgerReferenceTemplateRunner.cs` — ältere/zusätzliche
  Mess-CLIs, im W2-Vergleich NICHT verwendet

## 4 · HITL-Adjudikation — HINTER der Messgrenze (gehört zum System, NICHT zur Messung)

- `adjudication/`: HumanAdjudicationExecutor, LedgerAdjudicate*Runner, AdjudicationAdapter/
  ReviewAdapter, AdjudicationCompletenessGate, AdjudicationModels — das menschliche Gate nach
  LCR. Der Vergleich endet bewusst DAVOR (Messgrenze). FacetAssigner liegt hier, seine
  Dispositions-Regeln wirken aber schon in step-01/03 (deshalb in Rolle 1 gelistet).

## 5 · Alt-/Nebenpfade (im gemessenen Weg NICHT aufgerufen)

- `core/SemanticLedgerExtractor.cs` — ältere, nicht-unit-aware Extraktion (Vorgänger)
- `core/TranscriptSegmenter.cs` — älterer Segmenter (Vorgänger von AtomicUnitSegmenter)
- `core/SemanticLedgerRecallMatcher.cs` · `core/EvidenceFirstSpikeModels.cs` — Spike-/Alt-Reste
- `build/CandidateExtractionExecutor.cs` — nicht-unit-aware Executor-Variante
- `build/LedgerBuildRunner.cs` — älterer CLI-Weg ohne Unit-Coverage
- `LedgerCommands.cs` — CLI-Registrierung aller Ledger-Kommandos

FAZIT für die Fairness-Prüfung: Der Vergleich misst die Rollen 1+2 (bis LCR); Rolle 3 hat die
Messdaten erzeugt; Rolle 4 ist der bewusst abgeschnittene Mensch-Schritt; Rolle 5 ist tote
bzw. historische Masse und geht in KEINE Zahl ein.
