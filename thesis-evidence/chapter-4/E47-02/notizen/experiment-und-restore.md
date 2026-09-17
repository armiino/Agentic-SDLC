
## 10. G1-Bau-Plan (04.08., code-verifiziert) — **G1-a ✅ GEBAUT 04.08.**

> **Bau-Stand:** `DecisionStageExecutors.cs` (pure `DecisionStage`-Logik + Scan/ComposedApply), Kanten in der EINEN
> `AddTo`-Quelle (pipeline-full UND pipeline-hitl), StagePlan `07-decision`/`decision-gate` (= 7. Human-Gate),
> defer-all an ALLEN drei Responder-Stellen (In-Run-Policy · pipeline-full-Resume · pipeline-hitl-Resume).
> **Semantik-Korrektur am Code bewiesen (Test deckte es auf):** bei ADOPT_NEW hat der Tor-2-Apply den Coverage-SWAP
> bereits SELBST erledigt (`RequirementSwap`) — das synthetische Delta ist deshalb REFINE-förmig aufs NEUE Requirement
> (⇒ MARK_CHANGED am schon geswappten PBI = Align-Ziel), NICHT SUPERSEDE (fände nichts mehr). 10 Tests
> (`DecisionStageTests`: Scan/DeferAll/KEEP/REFINE/ADOPT als purer Durchstich Views→Derive→Gate→Apply→Merge +
> Derivation-Zusammenspiel: Flip⇒kein BLOCK, REFINE⇒MARK_CHANGED, ADOPT⇒MARK_CHANGED ohne Re-Swap).
> Build 0 · **329 Tests** · Smoke 14/0 · dry-run Build()-bar (Konsole/Kommentare: 6→7 Gates).
>
> **G1-b ✅ GEBAUT 04.08. (= E0.8):** `PipelineDecisionReviewAdapter` — das EDIT-fähige Gate (die Auflösung entsteht
> in der UI): leerer Default · vier Klartext-Ausgänge mit Wirkungs-Text (behalten/übernehmen/verfeinern/vertagen) ·
> `newStatement` mit Meeting-Prefill, Pflicht bei übernehmen/verfeinern (`VisibleWhen`) · vertagen frei (P2a) ·
> Blast-Radius-Note · gruppiertes Glossar · strukturierte Hilfe. Anbindung: `DecisionScan` persistiert
> `decision-gate-request.json` → CLI **`decision-gate-review <run|dir>`** (ReviewUiFlow, Autosave) →
> `decision-gate-decisions.json` → pipeline-full-Resume liest die Datei als Port-Antwort (DECs ohne Eintrag =
> sicher vertagt); `--open-ui` + ReviewHints verdrahtet; Flags/accept-all vertagen weiterhin laut. UI-Sandbox:
> `runs/_ui-sandbox/decision-gate/`. **Autor-Befund direkt eingebaut (04.08.): Einfluss ANKLICKBAR** — Ziel-REQ und
> jedes blockierte PBI als Kontext-Block; der Runner lädt den Core LIVE (read-only) und rendert Klartext-Details
> (REQ: Text/Status/Version/deckende PBIs · PBI: Titel/Ziel/AKs/gedeckte REQs). 6 Adapter-Tests.
> Build 0 · **335 Tests** · Smoke 14/0.
>
> **G1 LIVE BELEGT ✅ (04.08., Run `20260804_121433_eb181a`, isoliertes Experiment mit Core-Restore):** kompletter
> Zyklus in EINEM pipeline-full-Faden — 6-Zeilen-Delta-Transkript → Ledger/Adjudikation/Ingest (accept-all) →
> **CONTRADICT mintet DEC-001** (vs. REQ-32 „No-Gos bewohnerbezogen") → **Scan pausiert am decision-gate**
> (Checkpoint; Blockiert=[] — D3 zahlt sichtbar: vor pbi-update wurde noch nichts geblockt) → Auflösung per
> `decision-gate-decisions.json` (ADOPT_NEW) → Resume liest die Datei als Port-Antwort (`resolved=1, deferred=0`) →
> Apply: **REQ-78 (saubere neue ID) supersedes REQ-32 (superseded, v2)** · `contradicts_resolved(ADOPT_NEW)` ·
> DEC-001 resolved · covers-Swap PBI-012/023 → REQ-78 · **1 synthetische Op** → Derive: 3 MARK_CHANGED →
> **Align-Agent lief LIVE im Resume mit 3 Vorschlägen** → pbi-gate/forward-gate (accept-all) → Forward-Dry-Run
> 3 Ops → **PIPELINE FERTIG, exit 0**; Kangal still über 3 Saves. Ehrlich vermerkt: die Angleichungs-INHALTE
> wurden im accept-all bewusst NICHT blind übernommen (keine Alignment-Entscheide in der Bulk-Response) —
> PBIs stehen ehrlich needs_clarify; interaktive Annahme = der bewiesene R-26-C-Pfad. Core danach byte-identisch
> restauriert (Experiment; Beweis = Run-Ordner). **Offen im Slice: nur noch D2-Knopf + D4-Zähler.**
>
> **D2/D4 ✅ GEBAUT 04.08. — R-14 DAMIT KOMPLETT.** D2: dritte pbi-Gate-Option `to_decision` („→ Entscheidung nötig
> (Stakeholder)", Begründung PFLICHT = die Frage) → `DecisionRequestMint` prägt `DEC-<max+1>` (open_decision,
> targetEntityId-Metadatum, OHNE contradicts-Kante) + blockt das antragende PBI — im SELBEN pbi-Apply-Save
> (ein Snapshot, ein Kangal-Pass); Response additiv `DecisionRequests` (accept-all beantragt nie); Scan zeigt
> die Anträge mit Herkunft „Klärungs-Antrag — am pbi-Gate". Voller Kreis test-bewiesen: Antrag → Scan (Fallback-Ziel)
> → Auflösung KEEP → Unblock. D4: `CoreParkplatz` (pure) + `pipeline-full status` zeigt IMMER Parkplatz +
> **Kangal-Integritätszeile** (live am echten Core: 21 needs_clarify sichtbar, 0/0 Kangal). 4 Tests · **339 Tests** ·
