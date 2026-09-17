# pipeline-full — Bootstrap-Erweiterung: die GANZE Kette (empty → GitHub) als EIN durabler Graph
> Status: **ERLEDIGT / BAU-REFERENZ — verschoben 07.08.2026 nach done** (Aufräum-Runde; aktueller Stand: `../../STATUS.md` + `../../steward/README.md`). Alt-Kopf darunter = Historie.

> Status: LEBEND — **IN UMSETZUNG seit 26.07.** (Machbarkeit code-verifiziert §2b; Slices §7; Autor-Entscheid
> 26.07.: VORGEZOGEN vor W2 — Graph-Pol soll vor dem Messen die ganze Kette können). Schwester-Doc zu
> `pipeline-full-design.md` (der Betriebs-Loop, GEBAUT).
> Ziel: die heute CLI-only **Bootstrap-Hälfte** (core-bootstrap + re-clarify + seed) nach demselben Muster in den
> durablen MAF-Graphen heben → ein durchgängiges **empty → GitHub MAF-natives System**. Löst die in
> `backlog-genealogie.md §1c` dokumentierte Asymmetrie. Grundlage: `pipeline-full-design.md` (Bau-Muster),
> `backlog-genealogie.md` (was re-clarify ist), `E2E-RUNBOOK.md` (der reale Bootstrap-Ablauf A1–A5).
> **Nicht W1/W2-blockierend** — eigener Track; foreshadowed in `pipeline-full-design.md §2` (l3 „Bootstrap-Modus") + §9 (`variant`-Feld).

## 0. Warum & Abgrenzung

**Warum:** Der durable Graph deckt heute nur den **Betriebs-Loop** (bestehender Core → update). Der Bootstrap
(leer → Core → Backlog) hat nur CLI. Für das Thesis-Endbild „Orchestrierungs-Vergleich CLI vs. Graph vs. Agent"
sollte der **Graph-Pol die GANZE Kette** können — sonst vergleicht man nur die halbe Kette. Und: die Front-Stufen +
re-clarify sind **schon MAF-nativ**; es fehlt v. a. **Verdrahtung + eine Verzweigung**, kaum neue Fach-Logik.

**Abgrenzung:** Kein Neuschreiben von Stufen-Logik (Design-Prinzip „eine Quelle, keine Kopie"). Kein neuer
Orchestrierungs-Stil — dasselbe `Assemble`+Gate-Responder-Muster. Ergebnis ist EINE `pipeline-full`, die **beide Modi**
kann (Bootstrap **und** Betrieb), nicht ein zweites Kommando.

## 1. Der Bau-Muster-Baukasten (aus `pipeline-full-design.md`, bereits bewiesen)

Was pipeline-full etabliert hat und wir 1:1 wiederverwenden:
- **EINE `Assemble(...)`-Methode** = der ganze Graph inkl. Conditional Edges.
- **Wrapper-Executoren in 4 Geschmacksrichtungen** (je nach Stufe): (a) **det. Executor** um reine Funktion
  (Delta/Snapshot); (b) **Black-Box-Wrapper** (eigene `InProcessExecution` im Knoten, Ledger v1); (c) **`BindAsExecutor`
  echter Sub-Workflow** (Recipe); (d) **direkte Graph-Komposition** (Ingest/Pbi/Forward).
- **Bridges** — in-memory Artefakt-Übergaben statt CLI/run-config-Edits.
- **Zentraler Gate-Responder (R-25)** — RequestPort-Gates per **PortId** dispatchen, Antwort aus **Policy**
  (interactive|accept-all|replay).
- **Durability** — `FileSystemJsonCheckpointStore` + `CheckpointManager`, Streaming, `pointer.json`.
- **Sichtbarkeit** — Faden-Ordner + `events.jsonl` + `metrics.json`.

## 2. Wie es in den ECHTEN Code passt (verifiziert 26.07. — KEIN Bolt-on, durabel wie der Rest)

**Der reale Bau (`PipelineFullRunner.RunGraphAsync`):** pipeline-full ist HEUTE **nicht** ein einzelner
`Assemble`-Graph, sondern eine **Sequenz durabler Phasen**, die sich EINEN `CheckpointManager` teilen:
```
Phase 1: PipelineFullWorkflow.Assemble(ledger→adjudikation→baselines→delta)      [RunStreamingAsync, manager]
Phase 2: RunBackHalfAsync  (Delta→Ingest→Pbi,  reuse PipelineComposedWorkflow)   [RunStreamingAsync, SELBER manager]
Phase 3: RunForwardAsync   (Snapshot→Forward,  reuse GithubForwardHitlWorkflow)  [RunStreamingAsync, SELBER manager]
```
Jede Phase ist durabel (Checkpoints), alle über **denselben `manager`** → durchgängig durabel. (Die „ein Graph"-
Vereinheitlichung ist bewusst deferred, s. `pipeline-full-design.md §9` Ledger v1/v2.)

**Bootstrap fügt sich als GESCHWISTER-PHASE ein — identisches Muster:**
```
Phase 1 (unverändert): Assemble front (Ledger→Adjudikation→Baselines→Delta)
   ── Branch im Runner: coreRepo.ExistsAsync()? ──   (das Signal existiert schon: heute der "Core fehlt"-Fehler)
   ├─ Core da → RunBackHalfAsync    (heute)     → RunForwardAsync (update-delta)
   └─ leer    → RunBootstrapHalfAsync (NEU)     → RunForwardAsync (initial-sync)
        └ l3(Gate) → core-bootstrap → re-clarify cluster(Gate) → clarify(Gate) → seed
```
- **`RunBootstrapHalfAsync` wird EXAKT wie `RunBackHalfAsync` gebaut:** gleiche Signatur, **derselbe geteilte
  `manager`** (→ durabel wie der Rest), derselbe `RunWorkflowStreamingAsync`-Treiber, **Reuse** der re-clarify-Workflows
  (wie RunBackHalf `PipelineComposedWorkflow` reused). **Kein neues Orchestrierungs-Muster, kein Bolt-on.**
- **Die Verzweigung = ~2 Zeilen** in `RunGraphAsync` (`if (await coreRepo.ExistsAsync()) RunBackHalf else RunBootstrapHalf`).
- **Geteilte Front + geteiltes Ende bleiben unverändert** — nur die Mitte verzweigt, als Phasen-Wahl.

**Spätere Vereinheitlichung zu EINEM Graph — MAF-nativ MÖGLICH (offizielle Docs verifiziert 26.07.):** Die C#-Sequenzierung
der (Sub-)Phasen ist eine **bewusste Wahl, KEINE MAF-Grenze.** MAF kann die ganze Kette als EINEN Graph:
- **Executoren empfangen Input als LAUFZEIT-Nachricht** (`[MessageHandler] HandleAsync(message, ctx)`), nicht zur Bauzeit
  → ein Knoten kann `RelationLookup` im Handler bauen, wenn die Baseline als Nachricht eintrifft (mein früheres „kann nicht
  vor der Eingabe konstruiert werden" war falsch).
- **`BindAsExecutor`** bindet einen ganzen Workflow als EINEN Knoten; sein `YieldOutputAsync`-Output wird als Nachricht an
  den nächsten Knoten weitergereicht. Gates (RequestPort) laufen drin mit **qualifizierter Port-ID**; Checkpointing über die
  Komposition. (Quellen: MAF Executors + Sub-Workflows Doc.)
- **Im Repo schon bewiesen:** Recipe (02-baselines) ist bereits per `BindAsExecutor` gebunden (v2). Das Muster fehlt nur an
  den Phasen-Nähten.
- **Weg dahin:** Stage-Executoren auf Message-Input umstellen (Konstruktor-Injektion → Nachricht) + die (Sub-)Workflows per
  `BindAsExecutor` verketten → ein `WorkflowBuilder`-Graph, ein Checkpoint-Strom, Branch als Conditional Edge.
- **Kosten/Nutzen:** echter Refactor + Nesting-Overhead (Docs: „each nesting level adds execution overhead") + R-10-Fan-in
  → **bewusst DEFERRED** (gilt für ALLE Phasen, nicht nur Bootstrap); Nutzen = das Exponat „ganze Kette als EIN durabler
  MAF-Graph". Der phasen-basierte Stand ist die ehrliche, lauffähige Zwischenform — identisch zum bestehenden Teil.

**✅ GEGEN-VERIFIKATION 27.07. (offizielle Sub-Workflows-Doc, C#-Pivot, Stand 10.07.2026) — Abschnitt BESTÄTIGT,
plus 3 Präzisierungen für den späteren Umbau:**
- **Bestätigt wörtlich:** ① Requests aus gebundenen Sub-Workflows erreichen den Parent-Stream mit **qualifizierter
  Port-ID** („the sub-workflow executor's ID is prepended … e.g. `SubWorkflow.GuessNumber`"; „From the parent workflow
  caller's perspective, there is no difference … the framework handles the routing transparently" — in C# transparent,
  nur in Go muss man qualifizierte Ports von Hand anlegen). ② `YieldOutputAsync`-Outputs fließen per Default als
  Messages an die Parent-Kanten (`AutoSendMessageHandlerResultObject=true`). ③ Checkpoint/Restore über
  Sub-Workflow-Grenzen inkl. Zustand. ④ Overhead-Zitat exakt. — Damit ist auch meine B3-Session-Aussage „verschachtelte
  Gates kämen nicht sauber im äußeren Stream an" **KORRIGIERT: falsch für BindAsExecutor** (sie kommen an, qualifiziert);
  richtig bleibt nur, dass der Ledger-v1-**Black-Box-Wrapper** (eigene InProcessExecution IM Knoten) Gates verschluckt.
- **Präzisierung 1 — Responder-Konsequenz:** Unser zentraler Gate-Dispatch matcht Port-IDs EXAKT
  (`portId == "cluster-review-gate"`). Nach der Vereinheitlichung kämen sie qualifiziert an
  (`<BindingId>.cluster-review-gate`) → Dispatch + `GateFor` auf **Suffix-Match** umstellen, sonst enden alle Gates
  still in „interactive/unbekannt" (Exit 5).
- **Präzisierung 2 — Statelessness:** Docs-Warnung: konkurrierende Ausführungen eines gebundenen Sub-Workflows teilen
  die Instanz → Executors stateless halten (bei uns unkritisch: sequentielle Kette, aber beim Umbau prüfen).
- **Präzisierung 3 — Wechselwirkung mit H1:** Die Vereinheitlichung würde H1 VEREINFACHEN (ein Stream, ein
  Checkpoint-Faden → **kein Phasen-Pointer nötig**; Go-Doc bestätigt sogar Re-Publish pending Requests nach Restore).
  Reihenfolge-Empfehlung daher: B6-Beweislauf auf dem Phasen-Stand (eine Variable auf einmal!) → dann ENTSCHEID
  „Vereinheitlichung vor H1" (spart den Phasen-Pointer) vs. „H1 zuerst auf Phasen" (schneller, kleiner).

## 2b. Machbarkeits-Verifikation (26.07., gegen Code — Ergebnis: MACHBAR, 3 Korrekturen)

Session-Review vor Baustart; §2-Behauptungen gegen `PipelineFullRunner.cs` + Code-Inventur der Subsysteme geprüft:

- ✅ **Einhängepunkt exakt wie geplant:** Phasen-Sequenz mit EINEM geteilten `CheckpointManager`
  (`RunGraphAsync`, PipelineFullRunner.cs:143–151); das Branch-Signal liegt heute als **Skip** (nicht Throw) in
  `RunBackHalfAsync` („Core fehlt — Hinterhälfte übersprungen", Exit 0) und wird in B0 eine Ebene hochgezogen.
- ✅ **re-clarify ist wrappbar:** `ReClarifyClusterWorkflow.Build` (Agent→Gate→ReviewAgent→Finalize) und
  `ReClarifyBacklogWorkflow.Build` (Agent→Gate→Finalize) sind echte WorkflowBuilder-Graphen; die deterministischen
  Kerne sind pure Statics (`ReClarifyClusterApply.Apply`, `ReClarifyTraceabilityEnricher.Enrich`,
  `ReClarifyBacklogToIssuePlan.Project`). Die Adapter #14/#15 liefern `ReviewSession` + Decision-Records mit genau
  den Replay-Schlüsseln, die §5 braucht (`ClusterHumanDecision.OpId`, `BacklogHumanDecision.PbiId`).
- ✅ **Gate-Registrierung = bekanntes 3-Stellen-Muster:** `StagePlan`-Liste + `else if`-Zweig im
  Responder-Dispatch (`RunWorkflowStreamingAsync`) + Policy via `FullWorkflowSettings.GateFor`. `Stages`-Dict
  existiert schon; **das `mode`-Feld fehlt (→ B0)**.
- 🔧 **Korrektur 1 — B1 ist BILLIGER als geplant:** core-bootstrap ist bereits sauber faktorisiert
  (`CoreSeeder.Seed`, `CoreToBaseline.Project`, `CoreBacklogSeeder.Seed` = reine Funktionen); nur die
  Orchestrierungs-Reihenfolge sitzt im CLI-Runner. „Kern faktorisieren" entfällt — dünne Executoren genügen.
- 🔧 **Korrektur 2 — B5 ist GRÖSSER als geplant:** Es gibt KEINEN initial-sync-Code. Der 23.07.-„Workaround" war
  ein **handgelegtes Run-Artefakt** (`runsArchive/pbi-update/initial-sync-20260720/`), kein Kommando. Die
  deterministische Quelle existiert aber: `CoreViews.GithubSync` (liefert die unmapped PBIs). B5 = echtes
  Neubau-Paket (R-15), kein Lückenschluss.
- 🔧 **Korrektur 3 — l3 raus aus dem kritischen Pfad:** l3 ist dormant/frozen und im Graph per Default
  `stages.l3=off`. B2 wird ein OPTIONALER End-Slice, keine Voraussetzung für B3/B4.
  **Bau-Reihenfolge daher: B0 → B1 → B3 → B4 → B5 → B6 (→ B2 optional).**
- ⚠️ **Ehrliche v1-Grenzen (gelten heute genauso für den Betriebs-Teil):** `pipeline-full resume` ist No-op;
  `interactive`-Gates enden mit Exit 5 („Pause-Resume folgt") → auch die Bootstrap-Gates laufen v1 nur
  `accept-all`/`replay`. Lauf-Timeout aktuell 20 min (`cts` in `RunGraphAsync`) — der Bootstrap-Zweig bringt
  2 zusätzliche LLM-Stufen mit → in B3 prüfen/anheben.

## 3. Die Unterscheidung „Bootstrap vs. Betrieb" (das WO/WIE — Autor-Frage)

**Signal (existiert implizit schon):** `ICoreRepository.ExistsAsync()`. Heute wirft Ingest `"Core fehlt."` — wir
machen aus diesem **Fehler eine Verzweigung**:
- **kein Core** → Bootstrap-Zweig.
- **Core existiert** → Betriebs-Zweig.

**Steuerung (run-config, für Messkampagne/Ablation):** `fullworkflow.mode: "auto" | "bootstrap" | "operational"`
(Default `auto` = ExistsAsync-Detektion). `auto` ist der Produktweg; die expliziten Modi für kontrollierte Läufe.

**Verzweigungs-Punkt:** genau **nach Phase 1 (Front, nach Baselines/Delta)** — als **Phasen-Wahl im Runner**
(`RunBackHalf` vs. `RunBootstrapHalf`), NICHT als Assemble-Edge (weil pipeline-full heute phasen-basiert ist, s. §2).
Das ist exakt der Punkt, an dem der Runner heute schon Phase 1 → Phase 2 sequenziert. Davor identisch (Requirements
entstehen gleich), danach fundamental verschieden (**Erststruktur clustern** vs. **inkrementell aktualisieren**).

> **Korrektur (02.08.):** Die frühere Aussage „pbi-update kann keine Features anlegen, re-clarify schon" ist
> **überholt.** Seit der Operational-Backlog-Kette (O4 / `NEW_FEATURE`, 29.07.) legt auch der Betriebs-Pfad bei
> Bedarf ein **einzelnes neues Feature** an — über denselben `CoreBacklogSeeder`-Adapter (`PbiUpdateApply.ApplyNewFeatures`),
> autorisiert durch das PBI-Update-Gate (+ B2-UI, das ein `NEW_PBI` in ein `NEW_FEATURE` konvertieren kann). Der reale
> Unterschied ist heute: **re-clarify (Bootstrap) clustert die Erststruktur** (viele Features auf einmal), **pbi-update
> (Betrieb) erschafft ein Feature einzeln**, nur wenn ein neues Requirement in kein bestehendes Feature passt. Beide
> schreiben Features über denselben Seeder in den Core.

## 4. Was schon da ist vs. was NEU gebaut werden muss

| Bootstrap-Stufe | Status heute | Zu bauen |
|---|---|---|
| **l3-Gen (03-gap)** | MAF-Workflow existiert; im Graph `off`; dormant | **(OPTIONAL, End-Slice B2)** Wrapper + **l3-Review-Gate** (Adapter existiert) |
| **core-bootstrap** | ✅ Kerne schon PUR (`CoreSeeder.Seed`, `CoreToBaseline.Project`); nur Orchestrierung im CLI-Runner | dünner **det. Executor** (wie Delta) — KEINE Faktorisierung nötig (§2b K1) |
| **re-clarify cluster** | **MAF-Workflow** (Agent→Gate→ReviewAgent→Finalize) ✅ | Wrapper/`BindAsExecutor` + **cluster-Review-Gate** (Adapter #15) + Resolver (s. §5) |
| **cluster-apply** | det. Runner | dünner det. Executor |
| **re-clarify clarify** | **MAF-Workflow** (Agent→Gate→Finalize) ✅ | Wrapper/`BindAsExecutor` + **backlog-Review-Gate** (Adapter #14) + Resolver |
| **backlog-apply** (+Traceability) | det. Runner | dünner det. Executor |
| **core-seed-backlog** | det. Runner | dünner det. Executor |
| **Forward initial-sync** | ❌ KEIN Code — handgelegtes Artefakt 23.07. (§2b K2); det. Quelle existiert: `CoreViews.GithubSync` | **det. `github-initial-sync`** (R-15) — Core-PBIs → Voll-CREATE-Delta (Neubau) |
| Verzweigung + Detektion | – | **NEU:** ExistsAsync-Branch + `mode`-Config + Bridges |

**Bilanz:** ~5 dünne det. Executoren + 2 Workflow-Wrapper + **3 neue Gates** + Verzweigung + initial-sync. **Fast keine
neue Fach-Logik** — Verdrahtung + Wiederverwendung. (Die 3 Gates sind exakt die HITL-UIs #8/#14/#15 aus `done/2026-08-03/hitl-befunde.md`.)

## 5. Die neuen Gates — und die ehrliche Komplexität (Review-Resolver)

Drei neue RequestPort-Gates in den zentralen Responder: `l3-review-gate`, `cluster-review-gate`, `backlog-review-gate`.

**⚠️ Nicht-trivial (wie beim Adjudikations-Gate):** Der generische `GateResponder` (accept/reject) reicht NICHT für
**edit-fähige** Reviews. cluster-Review adjudiziert **Operationen** (apply/skip), backlog-Review **PBIs**
(accept/edit/reject/revise). Wie die Adjudikation einen `AdjudicationResolver` brauchte (action-typisiert), brauchen
diese zwei **eigene Resolver** für accept-all/replay:
- **cluster-review-Resolver:** Operationen — accept-all = alle apply; replay = per OpId matchen.
- **backlog-review-Resolver:** PBIs — accept-all = alle accept (kein edit); replay = per pbiId matchen.
- **l3-review:** Promotions accept/reject — dem generischen Responder am nächsten (evtl. direkt nutzbar).

**Interactive-Modus:** UI-Anbindung wie bei den bestehenden Gates (die Adapter #8/#14/#15 liefern die `ReviewSession`
schon). → **Dieser Track deckt sich mit der HITL-Verfeinerung** (#14/#15 durchsehen) — beides zahlt aufeinander ein.

## 6. Forward: initial-sync-Modus (R-15)

Bootstrap endet mit einem **frischen Repo** → alle PBIs müssen als **CREATE** raus (nicht update-delta). Heute per
synthetischem Voll-Delta überbrückt (E2E B7). **Zu bauen:** det. `github-initial-sync` (Core-PBIs → Voll-CREATE-Delta,
echtes Planformat) als Forward-**`variant`** (Design-Note §9 „variant-Feld"). Der Betriebs-Zweig nutzt weiter das
update-Delta. Beide münden in denselben Forward-Gate + Apply (Dry-Run-Default).

## 7. Bauplan (vertikale Slices — PRÄZISIERT 26.07. nach §2b; Reihenfolge B0→B1→B3→B4→B5→B6→(B2))

- ✅ **B0 — Verzweigung + Config (GEBAUT 26.07., kein LLM):** `FullWorkflowConfig.Mode` (RunConfig.cs) +
  `PipelineMode`-Enum `auto|bootstrap|operational` + pure Regeln `ParseMode`/`UseBootstrapBranch` in
  `PipelineFullConfig.cs` (Default `auto`, unbekannt→`auto`); Phasen-Wahl in `RunGraphAsync` (`ExistsAsync` EINMAL
  vor der Hinterhälfte, sichtbar als `PIPELINE_BRANCH`-Event + `pipelineMode` in config.json) +
  `RunBootstrapHalfAsync`-Stub (Signatur wie `RunBackHalfAsync`, SELBER `manager`). *Verifiziert: 14 neue
  Unit-Tests (`PipelineModeTests`, inkl. Wahrheitstabelle) — Build 0/0 · **141 Tests** · Smoke 14/0.*
- ✅ **B1 — core-bootstrap-Executor (GEBAUT 26.07.):** `CoreBootstrapStage.ExecuteAsync` (public, testbarer Kern:
  `CoreSeeder.Seed` → `repo.SaveAsync` → `CoreToBaseline.Project` → Triplet nach `05-core/`) +
  `CoreBootstrapStageExecutor` (Graph-Knoten, Input = Phase-1-`ProjectStateDocument`, Output =
  `CoreBootstrapOutput` mit Baseline-Pfad) + `PipelineFullWorkflow.AssembleBootstrap` (wächst mit B3/B4) +
  `RunBootstrapHalfAsync` fährt den Bootstrap-Graph über denselben Streaming-Treiber/`manager`. „Core existiert"
  = LAUTER Abbruch (kein Überschreiben). *Verifiziert: 2 Unit-Tests (`CoreBootstrapStageTests`: Seed+Triplet aus
  Delta-Dokument, Abbruch bei existierendem Core) — Build 0/0 · **143 Tests** · Smoke 14/0.*
- ✅ **B3 — re-clarify cluster + `cluster-review-gate` (GEBAUT 26.07.):** Statt Wrapper die MAF-nähere **direkte
  Komposition**: die vier BESTEHENDEN Cluster-Executors unverändert im Graph (`AssembleBootstrapCluster`:
  Agent→Gate(det)→ReviewAgent→Finalize→[RequestPort `cluster-review-gate`]→Apply). Bootstrap-Hälfte läuft als
  **Sub-Phasen am selben manager** (Konstruktionszeit-Abhängigkeit: `RelationLookup` braucht die B1-Baseline).
  Wiederverwendung statt Neubau: Agent-Factories via `ReClarifyRunner.BuildClusterAgentFactories` (gehoben),
  Apply-Kern via `ReClarifyClusterApplyRunner.ExecuteAsync` (gehoben, eine Quelle CLI+Graph, Muster
  `IngestionApplyExec`); Gate-Semantik = UI #15 (ein Item je Operation, apply/skip), Responder = generischer
  `GateResponder` + **`ClusterReviewReplay`** — das ERSTE Gate mit echter Replay-Datei-Anbindung
  (`human-decisions.json` per OpId, unmatcht→reject geloggt). *Verifiziert: 3 Unit-Tests (`ClusterReviewReplayTests`
  inkl. Responder-Zusammenspiel) — Build 0/0 · **146 Tests** · Smoke 14/0. LLM-Pfad (Agent-Knoten) erst im
  B6-Beweislauf.*
- ✅ **B4 — re-clarify clarify + `backlog-review-gate` + applies + seed (GEBAUT 26.07.):** Direkte Komposition als
  Sub-Phase 3 (`AssembleBootstrapBacklog`: ClarifyAgent→DoR-Gate(det)→Finalize→[RequestPort `backlog-review-gate`]
  →Apply→core-seed-backlog). Das Gate ist **EDIT-fähig** — eigener **`BacklogReviewResolver`** statt generischem
  Responder: accept-all = leere Entscheidungsliste (Apply-Semantik „fehlender Entscheid = accept", keine Edits);
  replay = aufgezeichnete `human-decisions.json` **1:1 inkl. `EditedPbiJson`** (Edits bit-identisch zum CLI, M-1);
  interactive = Pause. Gehobene Kerne (eine Quelle CLI+Graph): `ReClarifyRunner.BuildClarifyAgentFactory`,
  `ReClarifyBacklogApplyRunner.ExecuteAsync` (+Traceability+DoR-Re-Gate), `CoreSeedBacklogRunner.ExecuteAsync`
  (idempotent, Payload-Validierung VOR Save). *Verifiziert: 4 Unit-Tests (`BacklogReviewResolverTests`) —
  Build 0/0 · **150 Tests** · Smoke 14/0. LLM-Pfad erst im B6-Beweislauf.*
- ✅ **B5 — Forward initial-sync (R-15, GEBAUT 26.07.):** `GithubInitialSync.BuildDelta` (07-tore/github, pure
  Funktion: unmapped+aktive PBIs der `CoreViews.GithubSync`-View → `GithubSyncDeltaDocument` mit newPbis=alle,
  exakt das pbi-update-Format — verifiziert gegen das handgelegte 20.07.-Artefakt). `RunForwardAsync` nutzt es
  als Bootstrap-Variante (kein pbi-update-Delta vorhanden → det. Voll-CREATE-Delta aus dem Core, Artefakt
  `07-github/github-sync-delta.json`, Event `STAGE_INITIAL_SYNC_DELTA`); Betriebs-Zweig unverändert; beide münden
  in denselben Forward-Gate+Apply (Dry-Run-Default). *Verifiziert: 2 Unit-Tests (`GithubInitialSyncTests`:
  gemappte/archivierte PBIs bleiben draußen; leerer Core → leeres Delta) — Build 0/0 · **152 Tests** ·
  Smoke 14/0. CLI-Kommando `github-initial-sync` bewusst offen (R-15-Rest, nur falls außerhalb des Graphen nötig).*
- ▶ **B6 — Beweislauf empty→GitHub-Dry-Run: VORBEREITET 27.07., Start durch Autor.** Vorbereitung: ① Core geparkt
  → `state/core-parked-b6-20260727/` (inkl. history; Restore = zurück-mv) ② run-config.fullworkflow: transcript=
  Interview-Einrichtung.txt, mode=auto, timeoutMinutes=90 (NEU konfigurierbar, +2 Tests), Modelle 01/02 auf gpt-5.4
  (Autor-Entscheid „stabiler E2E", R-1), **repo/tokenEnv AUS** (leerer Snapshot ⇒ Forward plant reine CREATEs =
  23.07.-Vergleich), alle 6 Gates accept-all EXPLIZIT (inkl. der 2 neuen; policyProfile bleibt interactive als
  lauter Fallback) ③ Skeleton-Check grün (Run `20260727_052134_ea4181`). **Start:**
  `dotnet run --project AgenticSdlc.Host -- pipeline-full run` → unbeaufsichtigt bis Forward-Dry-Run + metrics.json.
  Vergleich gegen 23.07.-CLI-Zahlen (115→192 Items, 14 Cluster, 30 PBIs); Abweichungen → R-Eintrag.
  **ERGEBNIS Lauf 1 (27.07., Run `20260727_052405_3c8e13`, exit 0): Bootstrap-KERN BEWIESEN, Forward offen.**
  ✅ Verzweigung real (mode=Auto, coreExists=false → Bootstrap) · Core **0→71** (38 req + 12 features + 21 PBIs,
  194 Relationen) unbeaufsichtigt gebaut · beide neuen Gates per accept-all beantwortet (cluster: 5 Ops apply;
  backlog: 21 PBIs, 0 Edits) · initial-sync-Delta det. erzeugt (21 CREATE-Kandidaten). ❌ Forward-Dry-Run plante
  0 Ops → Gate-block ×21 (**R-27**: `ctx.DryRun ⇒ kein Agent`-Kurzschluss) · metrics.json im Bootstrap-Zweig
  falsch attribuiert (**R-28**). **BEIDE FIXES GEBAUT + getestet 27.07.** (det. Initial-Sync-PLAN via
  `InitialSyncDeterministic` + Bootstrap-Metrik-Fenster; 156 Tests inkl. Echt-Gate-Integrationstest, Smoke 14/0).
  **✅ B6 BEWIESEN — Lauf 2 (27.07., Run `20260727_055348_bb9b1a`, exit 0, unbeaufsichtigt):** Core **0→79**
  (42 req + 12 features + 25 PBIs, 198 Relationen) · alle 3 Bootstrap-/Forward-Gates beantwortet (cluster,
  backlog, github-forward) · **Forward-Plan 25/25 CREATE deterministic, Gate pass (0 Fehler, attempt 1), Apply-
  Dry-Run success=True, kein Forward-LLM-Call** · metrics.json korrekt: `06-backlog-cluster` (49k in, Gate pass)
  + `06-backlog-clarify` (78k in, DoR pass) statt Phantom-Ingest/Pbi (R-28 live bestätigt). **DoD von §7 erfüllt:
  der Graph-Pol kann die GANZE Kette empty→GitHub-Dry-Run.** LLM-Varianz Lauf1↔Lauf2: 38→42 req, 21→25 PBIs
  (Cluster stabil 12) — W2-relevant (N≥3). Offen als Tracks: H1 (Pause/Resume) · B2 (l3) · Ein-Graph-
  Vereinheitlichung (§2) · Core-Entscheid (194er geparkt / 71er Lauf1 / 79er aktiv). Zahlen-Vergleich 23.07.:
  12 Cluster/21 PBIs vs. 14/30 — erklärbar durch ① nur-requirements-Baseline (KEIN arch in pipeline-full v1;
  23.07. hatte req+arch) ② accept-all statt Human-Adjudikation ③ LLM-Varianz. Konsole zeigte die
  Output-Zeilen der Terminal-Knoten nicht (SendMessage ohne YieldOutput — kosmetisch, Events vollständig).
- **B2 (OPTIONAL, nach B6) — l3 + l3-Review-Gate:** Wrapper + Gate in den Responder (accept-all zuerst), weiter
  `stages.l3=off` als Default. *Verif: l3-Promotions im Graph bei `l3=on`.*
- ✅ **H1 — interactive Pause/Resume (GEBAUT 27.07., auf dem Ein-Graph):** löst die §2b-v1-Grenze.
  **Pause:** interactive-Gate ohne Entscheid → Checkpoint mit offenem Gate sichern (HitlShell-Mechanik:
  `SuperStepCompleted.HasPendingRequests`), `pointer.json` (Mode = Gate-Name) + Event `PIPELINE_PAUSED` +
  Review-Anleitung, Exit „PAUSIERT". Durch den Ein-Graph: EIN Restore-Punkt — **der geplante Phasen-Pointer
  entfiel ersatzlos** (U-Track-Dividende). **Resume:** `pipeline-full resume <runId> [--accept-all|--accept ids]`
  — identischer Graph via `BuildGraph`, `OpenStreamingAsync`+`RestoreCheckpointAsync`, MAF re-emittiert das
  offene Gate, Antwort aus den **Entscheid-Quellen je Gate**: cluster/backlog = `human-decisions.json` aus den
  ECHTEN Review-UIs (`l4-re-clarify-review`/`-backlog-review` akzeptieren die Faden-Ordner direkt, inkl.
  PBI-Edits); forward = `github-forward-decisions.json` (bekanntes Format); ingest/pbi = CLI-Flags
  (pipeline-hitl-Muster). Ohne Entscheid → erneut sauber pausieren (nächstes Gate ebenso). Metrics erst am
  echten Lauf-Ende. *Verifiziert: Build 0/0 · 159 Tests · Smoke 14/0 · --dry-run · Resume-Fehlerpfade
  (kein Lauf / kein pointer) sauber.*
- ✅ **H2 — Adjudikation als echter RequestPort (GEBAUT 27.07.):** die letzte Masterplan-Schuld („Hebung ist
  W1e'-Kern") geschlossen. `AdjudicationGateRequestExecutor` (Queue-Bau, materialisiert `01-ledger/queue.json`
  — **UI-fähig:** `ledger-adjudicate-ui <queuePath>` arbeitet direkt darauf) → RequestPort `adjudication-gate`
  → `AdjudicationApplyExecutor` (Antwort-Aktionen via Resolver-Replay-Semantik → consumable). Fach-Kern
  unverändert (Adapter/Resolver/Projection); Responder: accept-all=Heuristik, replay=queue.json,
  interactive=Pause. **Alle 7 Gates sind jetzt RequestPorts.**
- ✅ **H1+H2 LIVE BEWIESEN (27.07., mini-Smoke, 2 Läufe):** Lauf `20260727_074919`: Pause@adjudication
  (Checkpoint mit offenem Gate, pointer.json, UI-Anleitung) → resume --accept-all → Restore OHNE
  Ledger-Wiederholung, Gate beantwortet (66/21 Heuristik) → Kette lief weiter. Dabei **R-29** gefunden
  (Resume-Flags galten global → auch das 2. Gate beantwortet) → Fix (`ResumeAnswers` konsumierbar: Flags =
  genau EIN Gate; Entscheid-DATEIEN = jedes Gate) → Lauf `20260727_080638`: Pause 1 → resume → **echte
  Pause 2 am cluster-review-gate** (neuer Checkpoint/pointer). **Der Mehr-Gate-Zyklus
  Pause→Flags→Resume→nächste Pause ist real bewiesen.** Modell-Notiz: mini-Clarify lieferte 0 PBIs
  (saved=false) — Kette degradierte sauber (Forward übersprungen); bestätigt „mini nur bewusst".
  **Noch NICHT live geübt (bewusst — der Flags-Weg war der Mechanik-Beweis): der UI-Roundtrip an einer
  Pause.** Ablauf dann: Pause → UI selbst STARTEN (`ledger-adjudicate-ui <queuePath>` bzw.
  `l4-re-clarify-review <dir>` — die Pause-Konsole nennt das exakte Kommando; die UI startet NIE
  automatisch — durable Design: der Prozess ENDET bei Pause) → Entscheide speichern → `resume` liest sie.
  Datei-Formate identisch zu den UIs (code-verifiziert); Übungs-Durchlauf = 5-Minuten-Schritt bei nächster
  Gelegenheit.
- ✅ **H3 — `interactive-inline` + `--open-ui` + `status` + Pointer-Lebenszyklus (GEBAUT 27.07.):**
  ① **Neue Gate-Policy `interactive-inline`** (4. Policy-Wert, pro Gate mischbar): der Prozess bleibt am Gate
  STEHEN (Host-Muster A), öffnet mit `--open-ui` die passende UI automatisch (adjudication/cluster/backlog —
  eigene Host-Instanz auf den Faden-Ordner), wartet auf ENTER→Entscheid-Datei-Prüfung ('a'=accept-all,
  'p'=pausieren); **ohne Terminal (stdin EOF) degradiert er sauber zur durablen Pause** — Checkpoints laufen
  in beiden Mustern weiter. ② **`pipeline-full status [<runId>]`** = die EINE Antwort auf „wo ist der letzte
  Stand?": listet pausierte Läufe (Gate, seit, checkpointId) MIT Review-Anleitung + exaktem
  Weiter-Kommando; Einzelabfrage zeigt sonst das letzte PIPELINE_*-Event. ③ **Pointer-Lebenszyklus:**
  `pointer.json` existiert NUR während einer Pause — nach echtem Lauf-Ende wird er GELÖSCHT
  (`PIPELINE_POINTER_CLEARED`-Event); status live verifiziert (fand die 2 Test-Pointer — der stale 074919
  aus der Vor-Fix-Ära demonstrierte genau das Problem; beide Test-Pointer bewusst entwertet, Läufe bleiben
  als Beweise). *Verifiziert: Build 0/0 · **162 Tests** (+3 Policy-Parse) · Smoke 14/0 · --dry-run ·
  status live.*
- ✅ **UI-ROUNDTRIP LIVE BEWIESEN (27.07., Run `20260727_085644_5e09ef`, mini):** inline-Gate erreicht →
  `--open-ui` startete die Adjudikations-UI automatisch (Web-UI, 64 Items, Autosave) → stdin-EOF →
  **Degradation zur durablen Pause funktionierte live** → Autor entschied in der UI GENAU EIN Item
  (`RR::CAN-009 → accept_gap`) → `resume` OHNE Flags → **`GATE_ANSWERED source=queue.json, accepted=1,
  rejected=63 (unmatched, geloggt)`** — die Antwort kam beweisbar aus den echten Human-Entscheiden, der
  Governance-Fallback (unentschieden ⇒ reject) griff sichtbar → Kette lief weiter → planmäßige Pause am
  cluster-review-gate. Damit sind ALLE H-Bausteine live belegt. Kosmetik-TODO: `--open-ui` übergibt der
  Adjudikations-UI noch nicht die optionale validated-ledger.json (Kontext-Anreicherung zeigte 0 Claims).

**DoD:** `pipeline-full start <bootstrap-transcript>` auf leerem Core baut **Core + Backlog + GitHub-Dry-Run**
unbeaufsichtigt **als EIN durabler Graph**; Build/Tests/Smoke grün + `metrics.json`. Damit ist der Graph-Pol
**vollständig** (empty→GitHub), der Orchestrierungs-Vergleich fair.

## 8. Risiken & offene Punkte (ehrlich)

- **Review-Resolver (§5)** = die eigentliche neue Arbeit (2 Stück, wie `AdjudicationResolver`). Rein testbar bauen.
- **Mehr Verschachtelung → R-10:** re-clarify bindet Workflows (cluster/clarify) → dieselbe Fan-in-Naht wie Recipe.
  Auf MAF 1.15 nicht mehr reproduziert (backlog-genealogie/masterplan), aber Watch-Schleifen laut halten.
- **Refactor-Schuld:** re-clarify-Kerne (cluster-apply/backlog-apply/seed) sind CLI-Runner → faktorisieren wie bei
  Recipe/Ledger (dasselbe erprobte Muster).
- **R-15 initial-sync** ist Voraussetzung für einen sauberen Bootstrap-Forward-Gate-Pass.
- **Adoptions-Kandidat R-14** (Decision-Minting) ist orthogonal — kann im Bootstrap-Zweig mitgedacht werden
  (l4-completion-Adequacy als optionaler Schritt), aber nicht Voraussetzung.
- **Scope-Disziplin:** das ist ein **eigener Track**, kein W1/W2-Blocker. Erst wenn der Betriebs-Loop gemessen ist
  (oder parallel als bewusste Entscheidung), sonst wächst die Messfläche unkontrolliert (aspekt-modell §8 Risiko 4).

## 9. Bezug zur Forschungsfrage

Vollendet den **Orchestrierungs-Pol „Graph"**: nicht nur der Betriebs-Loop, sondern **die ganze frühe SDLC-Kette
(leer → GitHub)** als EIN governter, durabler MAF-Graph. Macht den Vergleich CLI vs. Graph vs. Agent über die
**volle** Kette fair — und ist zugleich das MAF-Tiefe-Exponat „Sub-Workflow-Komposition + Conditional-Branch +
zentrale Governance über eine mehrstufige, verzweigte Kette".
