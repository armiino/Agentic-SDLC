# E44-05 — Referenzentscheidung und Adjudikationsstufe

Die Notizen R1–R3 trennen die deterministische Annotationsvorlage, die autorbestätigte 11er-Fixture und die zurückgezogene modellgenerierte 99er-Referenz. Die produktive Adjudikation ist separat als Implementierungsstand beigefügt.

**Belegstatus:** Originalbelege bereitgestellt.

**Aussagegrenze:** Die vollständige menschliche Bearbeitung aller 136 Vorlagensegmente ist durch diese Dateien nicht nachgewiesen. Die 11er-Fixture ist eine begrenzte bestätigte Teilmenge. Keine Recall-Werte als gültige Vollständigkeitsmessung ausweisen. Aktuelle Codekopien belegen keinen damaligen Ausführungsstand.

Die Auswahl wurde am 2026-09-13 bereitgestellt. Kopierte Originaldateien sind bytegleich; Notizauszüge sind als solche gekennzeichnet. Vollständige Originalpfade, Dateihashes und die Auswahl innerhalb der Run-Ordner stehen im [Manifest](../manifest.json). Querverweise in Rohdateien behalten ihre historischen Pfade; die Tabelle ordnet die für diesen Beleg nötigen Dateien zu.

## Lesestart

- [../_kontext/input/eval-labels/semantic-ledger-interview-spike.json](../_kontext/input/eval-labels/semantic-ledger-interview-spike.json)
- [notizen/referenzentscheidung-r1-r3.md](notizen/referenzentscheidung-r1-r3.md)

## Vollständiger Dateiindex

| Datei | Funktion |
|---|---|
| [semantic-ledger-interview-spike.json](../_kontext/input/eval-labels/semantic-ledger-interview-spike.json) | Fixture des explorativen Vergleichs; Historischer Referenz-/Vorlagenstand; nicht aktuelle W2-Goldannotation |
| [Interview-Einrichtung.reference-ledger.json](../_kontext/input/eval-labels/Interview-Einrichtung.reference-ledger.json) | Historischer Referenz-/Vorlagenstand; nicht aktuelle W2-Goldannotation |
| [Interview-Einrichtung.reference-template.md](../_kontext/input/eval-labels/Interview-Einrichtung.reference-template.md) | Historischer Referenz-/Vorlagenstand; nicht aktuelle W2-Goldannotation |
| [ledger-iteration-notes.md](notizen/referenzentscheidung-r1-r3.md) | Wörtlicher Auszug aus Entwicklungsnotiz; historische Einordnung, keine neue Messung; Originalzeilen 299–430, vollständiger Quellhash im Manifest |
| [AdjudicationCompletenessGate.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/AdjudicationCompletenessGate.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [AdjudicationModels.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/AdjudicationModels.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [AdjudicationRefine.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/AdjudicationRefine.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [FacetAssigner.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/FacetAssigner.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [HumanAdjudicationExecutor.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/HumanAdjudicationExecutor.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [LedgerAdjudicateRefineRunner.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/LedgerAdjudicateRefineRunner.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [LedgerAdjudicateRunner.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/LedgerAdjudicateRunner.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [LedgerAdjudicateUiRunner.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/LedgerAdjudicateUiRunner.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [LedgerAdjudicationAdapter.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/LedgerAdjudicationAdapter.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [LedgerAdjudicationReviewAdapter.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/01-ledger/adjudication/LedgerAdjudicationReviewAdapter.cs) | Aktueller lesend gesicherter Implementierungsbeleg vom 2026-09-13 |
| [AdjudicationStageExecutor.cs](../_kontext/AgenticSdlc.Host/FullWorkflow/08-pipeline/fullworkflow/AdjudicationStageExecutor.cs) | Aktuelles Human-Gate; kein historischer Runbeleg |
| [Interview-Einrichtung.txt](../_kontext/input/transcripts/Interview-Einrichtung.txt) | Quelltranskript; Nutzungs-/Materialstatus bleibt Gegenstand B-50 |
