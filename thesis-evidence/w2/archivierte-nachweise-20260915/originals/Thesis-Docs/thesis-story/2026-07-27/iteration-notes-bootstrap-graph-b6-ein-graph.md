# Iteration Notes 26./27.07.2026 — Bootstrap-Graph (B0–B6) + Ein-Graph-Entscheid

> Status: MOMENTAUFNAHME 27.07.2026 — aktueller Stand: `Thesis-Docs/aktiv/STATUS.md` | Thesis-Story-Evidenz.

## Was in dieser Iteration passiert ist (Kurzfassung)

Der **Graph-Pol wurde vervollständigt**: pipeline-full konnte bisher nur den Betriebs-Loop
(bestehender Core → Update); jetzt kann er auch den **Bootstrap (leer → Core → Backlog → GitHub-Plan)**
— als durable MAF-Phasen am selben CheckpointManager, unbeaufsichtigt, mit zwei NEUEN RequestPort-Gates.
Beweis: zwei echte Läufe. Danach: Entscheid + Baustart, die Phasen zu EINEM MAF-Graph zu vereinheitlichen.

## 1. Die Bau-Slices (alle LLM-frei verifiziert; Details `Thesis-Docs/aktiv/pipeline-full-bootstrap-plan.md §7`)

- **B0** `fullworkflow.mode` (auto|bootstrap|operational) + Phasen-Verzweigung (`UseBootstrapBranch`,
  wahrheitstabellen-getestet) — der „Core fehlt"-Skip wurde zur echten Verzweigung.
- **B1** core-bootstrap als Graph-Knoten — die Kerne (`CoreSeeder.Seed`, `CoreToBaseline.Project`) waren
  schon pur; nur dünner Executor nötig.
- **B3** re-clarify **cluster** als direkte Komposition (4 bestehende Executors unverändert) +
  `cluster-review-gate` = **erstes Gate mit echter Replay-Datei-Anbindung** (human-decisions per OpId).
- **B4** re-clarify **clarify** + `backlog-review-gate` = **erstes EDIT-fähiges Gate** (eigener
  `BacklogReviewResolver`: accept-all = keine Edits; replay spielt `EditedPbiJson` 1:1 ein — M-1-konform)
  + Apply/Traceability + core-seed-backlog. Gehobene Kerne: eine Quelle für CLI UND Graph.
- **B5** deterministischer initial-sync (R-15-Kern): `CoreViews.GithubSync` → Voll-CREATE-Delta —
  ersetzt das handgelegte Artefakt vom 20.07. durch Produktcode.
- Architektur-Notiz: Bootstrap läuft als **Sub-Phasen am selben manager** (Konstruktionszeit-Abhängigkeit
  `RelationLookup`); bewusst dokumentiert, Auflösung = Ein-Graph (s. §3).

## 2. B6 — der Beweis (zwei Läufe, beide RunIds als Evidenz behalten)

- **Lauf 1** `runs/fullworkflow/20260727_052405_3c8e13`: Bootstrap-Kern bewiesen (Core **0→71**,
  beide neue Gates accept-all beantwortet), aber Forward plante 0 Ops → **R-27** gefunden
  (`ctx.DryRun ⇒ kein Agent`-Kurzschluss; nur durch Ausführung findbar) + **R-28** (metrics-Fenster
  im Bootstrap-Zweig falsch → 2×312k Phantom-Tokens).
- **Fixes**: det. Initial-Sync-PLAN (`GithubInitialSync.BuildCreateOps`, R-17-Präzedenz; HOLD-Governance
  unberührt; Integrations-Test gegen das ECHTE Gate) + bootstrap-fähige Metrik-Fenster
  (`06-backlog-cluster`/`-clarify`).
- **Lauf 2** `runs/fullworkflow/20260727_055348_bb9b1a`: **empty → GitHub-Dry-Run komplett, exit 0** —
  Core **0→79** (42 req + 12 features + 25 PBIs, 198 Relationen), 3 Gates beantwortet, Forward
  **25/25 CREATE deterministic, Gate pass (0 Fehler, attempt 1), Apply-Dry-Run success, KEIN
  Forward-LLM-Call**, metrics korrekt. **DoD des Bootstrap-Plans erfüllt.**
- **Messwert-Vorgeschmack (W2):** Varianz Lauf1↔Lauf2 bei identischer Config: 38→42 Requirements,
  21→25 PBIs, Cluster stabil 12.
- Kontext: 23.07.-CLI-Bootstrap hatte 14 Cluster/30 PBIs — Abweichung erklärt (nur-requirements-Baseline,
  accept-all statt Hand-Adjudikation, Varianz). Cores geparkt: `state/core-b6-lauf1/` + `-lauf2/`;
  Produktiv-194er restauriert.

## 3. Ein-Graph-Vereinheitlichung — Klärung + Entscheid (`Thesis-Docs/aktiv/ein-graph-vereinheitlichung.md`)

- **MAF-Wissens-Korrektur (gegen offizielle Sub-Workflows-Doc, 27.07.):** Gates in per `BindAsExecutor`
  gebundenen Sub-Workflows erreichen den Parent-Stream SEHR wohl (qualifizierte Port-IDs, transparentes
  Routing) — meine frühere Gegenaussage war falsch; nur der Black-Box-Wrapper (eigene InProcessExecution)
  verschluckt Gates.
- **Entscheid: BAUEN, aber Variante B (direkte Komposition statt Binding)** — kein Nesting-Overhead,
  keine qualifizierten IDs, Responder unverändert; einzige Konstruktionszeit-Abhängigkeit
  (`RelationLookup`) ist ein 3-Zeilen-Umbau. Kern-Erkenntnis: Die Phasen waren Baugerüst der
  inkrementellen Strategie, nie eine MAF-Grenze — die Vereinheitlichung LÖSCHT ~150 Zeilen
  Custom-Orchestrierung und gibt sie an MAF-Kanten zurück.
- **U0 GEBAUT (27.07.):** Branch als Graph-Knoten (`BranchDetectorExecutor` +
  `BootstrapDelta`/`OperationalDelta`, Typ-Routing).
- **U1 GEBAUT (27.07.):** Front + Branch + kompletter Bootstrap-Zweig = EIN `Assemble` (Typ-Routing-
  Verzweigung); `RunBootstrapHalfAsync` + 3 Teil-Assembles GELÖSCHT; `RelationLookup` zur Laufzeit (die
  einzige Konstruktionszeit-Abhängigkeit ist weg); `--dry-run` = LLM-freie Build()-Validierung.
- **U2 GEBAUT (27.07.):** Betriebs-Zweig + Forward eingezogen — **die GANZE Kette ist EIN MAF-Graph**
  (~40 Knoten, 7 Gates; Bridges nach IngestPbiBridge-Muster; `AddTo`-Hebung = eine Kanten-Quelle für
  Standalone- UND Ein-Graph; `RunBackHalfAsync`/`RunForwardAsync` GELÖSCHT). Der Runner ist Setup + EIN
  Stream + Metrics — die Design-Regel „Assemble = die eine Orchestrierungsstelle" gilt jetzt wörtlich.
- **U3 REGRESSIONSBEWEIS (27.07., Run `20260727_070619_ddf48a`):** **EIN Checkpoint-Stream (34 Checkpoints,
  eine Stream-ID — die Phasen-Nähte sind messbar verschwunden)**; gleiche Zahlenklasse wie der Phasen-Stand
  (Core 0→84: 41 req/13 features/30 PBIs; 30/30 CREATE det., Gate pass, Dry-Run success; metrics korrekt).
  **Track ABGESCHLOSSEN** → H1 (Pause/Resume) braucht keinen Phasen-Pointer mehr.
- **Thesis-Aussage damit belegt:** die ganze frühe SDLC-Kette (leer → GitHub-Plan) läuft als EIN governter,
  durabler MAF-Graph mit 7 Human-Gates und Conditional Branch — Orchestrierung vollständig im Framework,
  ~280 Zeilen Custom-Sequenzierung gelöscht. LLM-Varianz über die 3 Bootstrap-Läufe: 38/42/41 req,
  21/25/30 PBIs, 12/12/13 Cluster (W2-Material).

## 4. H1 — interactive Pause/Resume (27.07., direkt auf dem Ein-Graph gebaut)

- Die §2b-„v1-Grenze" (interactive ⇒ Exit 5, resume = No-op) ist GESCHLOSSEN: interactive-Gate ohne
  Entscheid → **saubere Pause** (Checkpoint mit offenem Gate, HitlShell-`pointer.json`, Review-Anleitung) →
  Mensch entscheidet in den **bestehenden UIs** (cluster/backlog zeigen direkt auf die Faden-Ordner) →
  `pipeline-full resume <runId>` → MAF re-emittiert das Gate, Antwort aus `human-decisions.json` bzw.
  `--accept-all`/`--accept` → weiter bis zum nächsten Gate oder Ende. Mehr-Gate-fähig (erneut pausieren).
- **Die U-Track-Dividende real:** der geplante Phasen-Pointer entfiel ersatzlos — EIN Graph = EIN
  Restore-Punkt (`OpenStreamingAsync`+`RestoreCheckpointAsync`, pipeline-hitl-Muster 1:1 übertragen).
- **H2 (gleicher Tag):** Adjudikation zum echten RequestPort gehoben (Request→Port→Apply; Fach-Kern
  unverändert; queue.json im Faden-Ordner ist direkt UI-fähig) — **damit sind ALLE 7 Gates RequestPorts**;
  die letzte W1e'-Masterplan-Schuld ist geschlossen.
- **LIVE-BEWEIS (2 mini-Läufe, `074919` + `080638`):** Pause@adjudication → resume --accept-all → Restore
  ohne Ledger-Wiederholung → Kette weiter → (nach R-29-Fix) **echte zweite Pause @cluster-review-gate**.
  Dabei R-29 nur durch Ausführung gefunden: Resume-Flags galten global statt für EIN Gate → `ResumeAnswers`
  konsumierbar gemacht (Flags einmalig, Entscheid-Dateien mehrfach) — exakt die pipeline-hitl-Semantik.
- **H3 (gleicher Tag):** 4. Gate-Policy `interactive-inline` (Host-Muster A: Prozess wartet am Gate,
  `--open-ui` startet die UI automatisch; ohne Terminal → saubere Degradation zur durablen Pause — beide
  Muster über DIESELBE RequestPort-Mechanik, pro Gate mischbar = Governance-Exponat) + `pipeline-full
  status` (Stand-Verfolgung: pointer.json = SoT „pausiert wo", nach Abschluss gelöscht; Verlauf in
  events.jsonl PIPELINE_PAUSED/RESUME/POINTER_CLEARED/RUN_DONE).
- **UI-ROUNDTRIP LIVE (Run `085644`) — der Schlussstein:** inline-Gate → `--open-ui` startete die
  Adjudikations-UI automatisch → stdin-EOF → Degradation zur durablen Pause (live) → Autor entschied in der
  UI genau EIN Item (`accept_gap`) → resume OHNE Flags → **`GATE_ANSWERED source=queue.json, accepted=1,
  rejected=63 (Governance-Fallback, geloggt)`** → planmäßige nächste Pause. Der Mensch-im-Loop-Zyklus ist
  damit über die ECHTE UI belegt, nicht nur über Flags.
- Verifikation: Build 0/0 · **162 Tests** · Smoke 14/0 · `--dry-run` · 4 Live-Läufe · status live ·
  Reibungs-Log bis R-29.

## Verifikations-Netz am Ende dieser Notes
Build 0/0 · **159 Tests** (23.07: 80) · Smoke 14/0 · Reibungs-Log bis **R-28** (R-27/R-28 gefixt+live bestätigt).
