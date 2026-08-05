# Archiv: `pipeline-hitl` (PipelineComposedRunner) — abgelöst 05.08.2026

> Status: HISTORIE — ersetzt durch `pipeline-full run --from-delta` (Schritt 5 ③).
> Voller Entscheidungs-Weg + Bau-Doku: `docs/thesis-story/2026-08-05/iteration-notes-eingangsvertrag.md`
> + `docs/aktiv/aufgefallen.md §2a-Schritt-5-③` + Matrix §D (verifizierte Tabelle 05.08.).

## Was das war

Der ERSTE durable Kompositions-Beweis (~20.–24.07.): zwei/drei Human-Gates in EINEM MAF-Graphen über die
Hinterhälfte (MeetingDelta-Datei → Ingest → [Gate] → Apply → decision-scan → [Gate] → Bridge → Placement →
[Gate] → Apply), prozessübergreifend resumebar. Der Trittstein, aus dem W1e′/pipeline-full wuchs.

## Warum abgelöst (Kurzfassung)

1. **Teilmenge geworden:** pipeline-full enthält dieselbe Hinterhälfte aus DENSELBEN AddTo-Quellen
   (`PipelineComposedWorkflow.cs` — die Executors + Kanten LEBEN dort weiter, nur diese CLI-Schale ist archiviert).
2. **Die einzige Einzigartigkeit — Delta-Datei-Einstieg — ist BESSER umgezogen:** typisierter Eingangs-Vertrag
   am Ein-Graph (`PipelineFullEntry` → Entry-Dispatcher → BranchDetector; MAF-nativ per Input-Typ-Routing),
   damit automatisch für BEIDE Zweige und mit dem ECHTEN Resume (die hier eingebaute noAgent-Resume-Falle —
   Align-Folgepfade warfen im Resume — existiert im Nachfolger nicht).
3. **Smoke-Funktion umgezogen und verbessert:** Block 5/5 prüft jetzt den EIN-Graph-Zyklus
   (Delta-Einstieg → Pause ingest-gate → Resume mit echten Factories → FERTIG) statt des Zwillings.

## Verifikation der Ablösung (nichts verloren)

Sweep 05.08.: einziger externer Nutzer war `AgentFactory` → vorab nach `PipelineAgents.Factory` (shared)
umgezogen. Beweise: Routing-Graph-Test `PipelineEntryRoutingTests` · Live-Durchstich Runs
`20260805_125359_50aba4` (Pause→Resume→FERTIG, Core unverändert) · Smoke 14/0 mit neuem Block.
Alte pipeline-hitl-Läufe (`runs/pipeline/…`) bleiben als Thesis-Beleg erhalten.
