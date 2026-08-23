# 08-pipeline — pipeline-full: die GANZE Kette als EIN durabler MAF-Graph

> Status: LEBEND (README-bei-Code, B3 07.08.2026).

## Rolle

`pipeline-full` ist der EIN-GRAPH: Transkript ODER Delta rein → komplette Kette → Forward — als EIN
MAF-Workflow mit Checkpoints (Pause/Resume an jedem Human-Gate), zentralem Gate-Responder und metrics.json.
CLI: `pipeline-full run <transkript>` · `run --from-delta <delta.json>` · `resume <runId> [--accept-all]` ·
`status` (pointer.json existiert NUR während Pause).

## Aufbau (die EINE Bauzeit-Stelle: `PipelineFullWorkflow.Assemble`)

```
Entry (typisiert: Transkript|LoadBaselineRequest|Delta) → Front (LedgerIntake [Drift-Guard] →
[LedgerCapsule, BindGateFree] → Summary → Adjudikation) → Baselines/Recipe → Branch (Auto: Core da?)
├─ Bootstrap-Ast: CoreBootstrap → Classify-Spiegel → ADR-Spiegel → Cluster/Backlog → Forward
└─ Betriebs-Ast:  Ingest → [ingest-gate] → Apply → ArchIngest-Strip → [arch-ingest-gate] →
   Classify-Strip → [arch-classify-gate] → ADR-Strip → [adr-gate] → DecisionScan → [decision-gate] →
   PbiUpdate (E-8/A4/Align) → [pbi-gate] → Forward-Snapshot → [github-forward-gate] → Execute/DryRun
```

**10 Human-Gates** (RequestPorts): adjudication · cluster · backlog · ingest · arch-ingest · arch-classify ·
adr · decision · pbi · github-forward. Zweig-Verdrahtung per `AddTo` aus den Stufen-Workflows (eine
Kanten-Quelle für CLI UND Graph — keine Kopien).

## Das Schienennetz — Karte, Routen, Referenzen (Autor-Sitzung 21.08.)

**Lesehilfe:** TÜR = Einstieg (Entry-Fach, wo eine Fahrt beginnt) · `*name*` = Human-Gate (RequestPort:
Zug steht, bis DU entscheidest) · WEICHE = Inhalts-Entscheid im Knoten, materialisiert als Nachrichten-TYP.
Kein Lauf besucht alle Knoten — die Karte ist der Möglichkeitsraum, die Fahrt eine Route.

```text
TÜR 1 Transkript
  |    LedgerIntake -> [Ledger-Kapsel] -> Summary
  |    -> *adjudication-gate* -> Baselines -> Delta-Bau
  v
TÜR 2 Delta ------------------> WEICHE "Branch": Core leer?
 (GitHub-Runde = Ernte-             |
  Vorstufe im Runner:               +-- BOOTSTRAP-AST (Core leer)
  Snapshot -> Destillat             |     CoreBootstrap -> Classify-/ADR-Spiegel
  -> Drafts, dann Tür 2)            |     -> Cluster  *cluster-review-gate*
                                    |     -> Backlog  *backlog-review-gate*
                                    |     -> Seed  ................................ A
                                    |
                                    +-- BETRIEBS-AST (Core gefüllt)
                                          Ingest-Resolver -> Checker
                                          -> *ingest-gate* -> Apply
                                          -> Arch-Strip -> *arch-ingest-gate* -> Apply
                                          -> Classify   -> *arch-classify-gate*
                                          -> ADR        -> *adr-gate*
                                          -> DecisionScan -> *decision-gate*
                                          -> PbiUpdate/Align -> *pbi-gate* ........ A

TÜR 3 Clarify   -> ClarifyEntry (Plan+Validate) -> pbi-Schwanz *pbi-gate* ......... A
TÜR 4 Reproject -> ReprojectEntry (Sync-Delta AUS dem Core) ....................... A

A = gemeinsamer FORWARD-SCHWANZ:
    Snapshot (R-16) -> Forward-Seed (+Vermerke) -> *github-forward-gate* -> Apply | Dry-Run
```

**Routen je Tür (mit Beleg-Läufen):**

| Tür | Wer startet sie (Steward-Seil) | Typische Route | Beleg-Lauf |
| --- | --- | --- | --- |
| 1 Transkript | Meeting-Runde | Front → Weiche → Betriebs-Ast → Forward | Meeting-4-E2E `20260818_084712` |
| 1 (Core leer) | Meeting-Runde | Front → Weiche → Bootstrap-Ast → Forward | B6-Beweisläufe (`state/core-b6-*`) |
| 2 Delta | `run_pipeline_from_delta` | direkt Weiche → Betriebs-Ast | Analyst-Tor `20260820_095055` |
| 2 via Ernte | `run_pipeline_from_github` | Ernte-Vorstufe → Weiche → Betriebs-Ast | Kommentar-Runde `20260820_171903`-Serie |
| 3 Clarify | `run_clarify_via_graph` | nur pbi-Schwanz → Forward | A′-Beweis `131538` |
| 4 Reproject | `run_reproject` | nur Forward-Schwanz | Stil-V2 `20260820_164939` |

**Wo was ERKLÄRT ist (die Zeiger-Tabelle — bei Fragen ZUERST hier schauen):**

| Thema | Datei |
| --- | --- |
| Die 4 Türen + Typ-Routing („genau EIN Fach") | `fullworkflow/PipelineEntryExecutor.cs` (Kopf-Kommentar + Code) |
| ALLE Kanten (die eine Verdrahtungs-Stelle) | `fullworkflow/PipelineFullWorkflow.cs` → `Assemble` |
| Start/Pause/Resume/Event-Pumpe/Gate-Responder | `fullworkflow/PipelineFullRunner.cs` (Kern) + `.EventPump.cs` / `.GateResponder.cs` / `.Cli.cs` (9e-light-Neuzuschnitt 21.08., EINE partial class) + Abschnitt „Lebenslauf" unten |
| Die Weiche | `fullworkflow`-BranchDetector (Typ-Wahl Bootstrap/Operational) |
| Die Vier-Schritt-Halte-Figur (Maker→Checker→Halt→Apply) | `../07-tore/README.md` · kanonische Form: `Thesis-Docs/aktiv/done/2026-08-04/reclarify-checker-repair-plan.md` (R-33) |
| Innenleben je Stufe | `../01-…`–`../09-…/README.md` |
| Der C4-Kreislauf (Architektur-Lücken mit Lebenslauf: §3-Projektion, Antwort-Anker, Drei-Stufen-Garantien) | `../05-core/README.md` §„Der C4-Kreislauf" |
| Die drei Sichten aufs Backlog (backlog.md ↔ Issue ↔ Story Map: Jobs, Sync-Garantien, deklarierte Schwächen) + Story-Map-Tafel | `../05-core/README.md` §„Die drei Sichten" + §„Die Story-Map-Tafel" |
| Autor-Artefakte + Doc-Publish (vision/personas/glossar/c4 → Team-Repo, Drei-Klassen-Ordnung) | `Thesis-Docs/aktiv/team-sichtbarkeit-slice.md` (Bauplan + GitHub-Rückfluss-Landkarte) |
| Artefakt-Anatomie eines Laufs | `runs/README.md` · Route nachlesen: `runs/fullworkflow/<id>/logs/events.jsonl` |
| MAF-Einordnung („ist das framework-gewollt?") + Belege | `Thesis-Docs/aktiv/maf-feature-matrix.md` |
| Kapsel-Endbild (warum flach + eine Kapsel) | `Thesis-Docs/aktiv/aufgefallen.md` §9j · R-38 im E2E-RUNBOOK |

## Lebenslauf eines Laufs — der Faden vom Befehl zum Knoten (Autor-Frage 21.08.)

1. **Befehl:** Steward-Seil (`run_pipeline_from_delta`) und CLI sind HÄUTE über derselben Naht — beide rufen
   `PipelineFullRunner.RunAsync(["pipeline-full","run","--from-delta",…])` (K13, kein Duplikat).
2. **Bauzeit (je Lauf neu):** der Runner konstruiert die Executor-GRUPPEN (Front/Bootstrap/Operational/
   Classify/Adr/Forward — je Gruppe eine Zeile) und übergibt sie an `PipelineFullWorkflow.Assemble`. Dort:
   `new WorkflowBuilder(front.Entry)` — **der Start-Knoten wird im Builder-KONSTRUKTOR benannt** — dann alle
   `AddEdge`-Zeilen. Ergebnis: EIN versiegeltes `Workflow`-Objekt. Es gibt keine zweite Verdrahtungs-Stelle.
3. **Start ≠ Methodenaufruf:** die Runtime legt die EINGANGS-NACHRICHT (`PipelineFullEntry` mit GENAU EINEM
   gesetzten Feld: Transkript | Delta | Clarify-Batch) in die Mailbox des Start-Executors. Der Entry-
   Dispatcher sendet je nach Feld EINEN Nachrichten-Typ, und die TYPISIERTEN Kanten routen: Transkript →
   LedgerIntake · Delta → BranchDetector (dein from-delta-Fall überspringt die Front) · Clarify → ClarifyEntry.
   „Aufgerufen" wird ein Knoten also immer dadurch, dass eine Nachricht seines Typs bei ihm ankommt.
4. **Laufzeit:** `InProcessExecution` + `FileSystemJsonCheckpointStore` (Checkpoint je Superstep in
   `checkpoints/`). Der Runner pumpt den EVENT-Strom: Fehler-Events laut (R-18), `RequestInfoEvent` → der
   zentrale Gate-Responder (PortId-Dispatch, Politik je Gate). Interactive ohne Entscheid-Datei ⇒
   `pointer.json` schreiben + Prozess-ENDE — das ist die Pause.
5. **Resume:** `pipeline-full resume <runId>` baut per DEMSELBEN Assemble den IDENTISCHEN Graph neu,
   `RestoreCheckpointAsync` lädt den Zustand, die Entscheid-Dateien beantworten den offenen Port. Deshalb
   sind Port-/Kanten-Namen eingefroren (Checkpoint-Kompatibilität) und Graph-Wiring nur per
   In-Process-Run-Test beweisbar.

## Schlüssel-Mechaniken

- **Zentraler Gate-Responder** (PortId-Dispatch): Politik je Gate aus run-config (accept-all | interactive |
  replay); interactive liest die jeweilige `*-decisions.json` MIT VORRANG vor --accept-all; decision-gate
  vertagt unter accept-all IMMER (R-14-Governance).
- **Bahnen-Strips** (Classify/ADR): zwei dünne Bridges (Betrieb arch-aktiv-gescoped [Smoke-Schutz!] ·
  Bootstrap-Spiegel) speisen EINEN geteilten Strip; Passagier überlebt den Port-Roundtrip per Datei-Vertrag
  (passenger-*.json + lane.json) und wird TYP-GENAU re-emittiert.
- **Lauf-Report-Vertrag** (R-40): `07-ingest/applied/run-report.json` — req-Apply schreibt initial,
  arch-Apply schreibt FORT, Decision-Stufe liest genau diese Datei („ein Konzept = EIN Vertrag").
- **Kapsel-Regel** (R-38): unverdrahtete Ports in Sub-Workflows deadlocken STILL → `BindGateFree`-Wächter
  (shared/) macht daraus einen Konstruktions-Fehler; HITL-in-Kapsel geht offiziell via ForwardMessage.
- **`PipelineAgents.Factory`** — die EINE Agent-Fabrik der Stufen (Prompt + Middleware: ToolCallLogger,
  otel/gen_ai, input-context/response-text; W1a-Denk-Faden am `captureReasoning`-Schalter).

## Beweise (Auswahl)

Ledger-Kapsel-Resume `144056` · R-14-Zyklus `121433` · R-11-E2E `145812`/`151204` · Pause/Resume aller
neuen Gates `181332`. Artefakt-Anatomie: `runs/README.md`.
