> Pfad-Hinweis 24.07.: Doku-Umzug — `AgenticSdlc.Host/FullWorkflow/docs/…` heißt jetzt `Thesis-Docs/aktiv/…`; `thesis-story/`, `refactoring-karten/` liegen unter `docs/`. Historische Pfad-Zitate in eingefrorenen Einträgen bleiben absichtlich original.

# E2E-Runbook — FRISCHER Komplett-Durchlauf: leeres Projekt → Core → GitHub-Issues

**Setup dieses Laufs (2026-07-23):** Der bisherige Core ist **geparkt** (`runs/_e2e-backup/state-core-alt/`
— er entstand aus Interview-Einrichtung + einem meeting-2-Delta, das den Ledger ÜBERSPRANG). Jetzt läuft
alles einmal frisch und vollständig: **Phase A** Bootstrap aus `Interview-Einrichtung.txt` (Core + Backlog
anlegen), **Phase B** erstes Betriebs-Meeting `meeting-2-extended.txt` (gezielt gebautes Test-Transkript) — diesmal MIT Ledger — bis zu echten
Issues im neuen Repo **`armiino/Agentic-GitHub-refactor`** (Token: `.env → GITHUB_AGENTIC_REFACTOR_TOKEN`).
Das Ergebnis ist zugleich der Vergleich „alt (ohne Ledger-Front) vs. neu (ganze Kette)".

**Zweiter Zweck dieses Laufs:** Reibung erfassen. Trage in der Beleg-Tabelle in die Spalte
**Reibung/Befund** ein, was dir an der jeweiligen Stufe nicht passt (UI, Wartezeit, unklare Ausgabe,
schlechtes Modell-Ergebnis, fehlender Schritt …) — das wird die Arbeitsliste für die nächsten Wellen.

**Rollen:** **[DU]** = deine Gate-Entscheidung · **[LLM]** = kostet Modell-Calls (openrouter, gpt-5.4).
**Diagnose-Regel bei Fehlern:** erst `dotnet test` (76/76 grün ⇒ Mechanik ok ⇒ Befund liegt beim
Modell-Output → `logs/`, `*-attempts.json`, Gate-Reports des Runs lesen; rot ⇒ Mechanik-Bug, erst fixen).
**Achtung:** Solange kein Core existiert (bis A4), ist `tools/smoke-hitl.sh` nicht fahrbar — normal.

**Beleg- und Reibungs-Tabelle (beim Fahren ausfüllen):**

| # | Stufe | RunId | Kern-Zahl (Checkpunkt) | Reibung/Befund |
|---|---|---|---|---|
| A1 | ledger-build-units (Interview) | RID-A1 = `20260723_093443_a85a80` (4. Lauf; 3. = `090059` verworfen wg. engl. Propositions) | gate pass, validated=45, **Propositions DEUTSCH (R-6 bewiesen)** | R-1 (Läufe 1+2), R-6 (Lauf 3) |
| A2 | Adjudikation | (in RID-A1) | 63 Items entschieden; **consumable: 49 Claims, pending=0**, Gate pass; refine erledigt (4 ADJ-GAP-Claims voll facettiert, 0 pending) — **A2 KOMPLETT** | R-9 (apply musste separat nachgeholt werden) |
| A3 | recipe (Arm B) | RID-A3 = `20260723_113115_35335a` | **requirements=55, architecture=45**, DEUTSCH (R-6 hält auch in 02); Checker: `decision=HumanReview` (= normaler Endzustand, identisch Alt-Beleg 20260709); **Derivations via load-Recipe nachgeholt (Run `20260723_114515_f3f537`): risks=7, gap=9, deutsch** | R-10 (build-Derivations kaputt → Workaround load) |
| A3.5 | l3 (03-gap) + Review | RID-A3.5 = `20260723_122625_136540` | 35 Kandidaten (15 SupportedAnchored auto / 15 Unreferenced / 5 Contradicted); 20 reviewt → **15 promotet (L3-REQ-001…015), 5 abgelehnt**; deutsch | Sprache ok ohne Härtung (03-Prompts nativ deutsch) |
| A4 | core-bootstrap (+--l3-run) | `runs/bootstrap/20260723_124020_fc231e` | **Core angelegt: 115 Items** (req=70 [55 Baseline + 15 L3-accepted], arch=45), relations=134, provenance=115; core-baseline: 70 reqs, 0 openDecisions | |
| A5 | Backlog-Schnitt + Seed | Cluster `20260723_130243_a96fda` → Backlog `20260723_131421_80a11e` | 14 Cluster (11 Korrekturen reviewt) → **30 PBIs** (30/30 reviewt, 0 dropped, Gate pass) → Seed: **Core 115→159** (+14 feature +30 pbi, relations=203, 0 unresolved); **1. History-Snapshot ✓ (115er-Stand)** — **PHASE A KOMPLETT** | R-12 (clarify-Schritt fehlte in Ansage + eigene RunId) |
| B1 | ledger-build-units (meeting-2-extended) | RID-B1 = `20260723_133055_48b8cf` | gate **pass** 0/0; units=36 (29 used, 4 noise, 3 needs_human) 26→20→20; **E-Checks auf Ledger-Ebene: E1✓ (Triage subklassifiziert smalltalk/low_signal/acknowledgement) · E2✓ (Piktogramm BESSER als erwartet: eigener Claim `uncertain\|must_clarify` statt needs_human) · E4✓ alle 4 · E5✓ · E6✓ `compliance_constraint\|required\|must` (Rasierklinge!) · E7✓ `decided\|must_not` + Vorlagen optional · E8✓ `open_question\|open\|must_clarify` · E9✓ beide `desired\|later_possible` · E10✓ Firestore `decision\|decided\|must` + Android-10 nuanciert als uncertain (Träger-Klärung erkannt!)** | needs_human=3 sind Moderations-Reste → in Queue rejecten |
| B2 | Adjudikation | (in RID-B1) | claims = , pending = 0 | |
| B3 | recipe (Arm B) | RID-B3 = `20260723_134102_c63c89` | req=24, arch=16, deutsch; **E9✓** (Dunkelmodus/Schriftgröße als „Wunsch", kein „muss" — C3 wirkt); **Derivations liefen DIESMAL im build (risks=6, gap=10) → R-10 = intermittierendes Race, nicht deterministisch**; ⚠ 3 Meta-Claims mitgeflossen (accept_gap-all in B2, unrefined) → bei B5 rejecten (Staffel-Gate-Beleg) | R-10-Update; Autor-Lehrstück Gate-Wirkung |
| B4 | project-state-build (Delta) | `runs/project-state/e2e-meeting2/meeting-delta.json` | delta items = **24** (nur requirements — arch bleibt designbedingt zurück = E10/R-11) | |
| B5 | ingest-hitl | RID-B5 = `20260723_135427_9260f1` | 24/24 akzeptiert → APPLIED: 8 NEW + 11 NEW_RELATED + **4 REFINE (REQ-32, L3-REQ-002!, L3-REQ-004!, REQ-08) + 1 SUPERSEDE (REQ-70)** — **E4✓ bewiesen: Identity-Matching, L3-Items leben, kein Duplikat**; Core **159→179**, History-Snapshot #2 ✓ | ⚠ 3 Meta-Items nun im Core (Blanko-Akzept über 2 Gates); letzte Filter-Chance B6 — oder bewusst als Lehrstück behalten |
| B6 | pbi-update-hitl | RID-B6 = `20260723_141409_8ea1c5` | 25/25 akzeptiert → **newPbis=13, updatedPbis=8**, relations +33/−1; Core 179→**192** (43 PBIs), Snapshot #3 ✓; E8: PBI-003 `needs_clarify` (Rechtemodell Angehörige) → B6b; **E10-Ist SCHÄRFER als Soll: Firestore-Beschluss via requirements-Route in REQ-70 („festgelegt", SUPERSEDE) angekommen, aber ARCH-39+REQ-55 sagen weiter „vorläufig" ⇒ WAHRHEIT GESPALTEN — stärkster R-11-Beleg; Android 10 überlebte als open_question/risk (PBI-036/037)** | ⚠ Meta-PBIs 039–041 jetzt im Backlog → bei B7 rejecten (sonst „Es ist unklar…"-Issues) |
| B7 | github-forward (Initial-Sync, det. Plan) | `runs/github-forward/_initial-sync-det` (3 Anläufe: 43er-Agent R-17, 22er-Agent R-17, det. Plan ✓; davor Stale-Snapshot R-16 vom Dry-Run gefangen) | Review 39 apply / 4 skip (Meta-PBIs 039–041 + Dunkelmodus draußen) → Dry-Run `wouldCreate=39, rejected=0` → **EXECUTED: created=39, failed=0** (1. Versuch 39×404 = Repo-Namens-Dreher `github-agentic-refactor`→`Agentic-GitHub-refactor`, nichts geschrieben); API-Gegenprobe: Issues #1–#39 deutsch im Repo; **39 `implemented_by_issue`-Relationen im Core**, Snapshot #4 ✓ — **🎯 G-1 KOMPLETT: Transkript → GitHub-Issue, jede Mutation autorisiert, durchgängige Provenance** | R-16, R-17, R-15 (Initial-Sync-Kommando bauen); Mikro: `issueUrl` im Report leer (nur Nummer) |

---

## Reibungs-Log — wird zur Schärfungs-TODO-Liste des E2E

*Regel: Jede Fehlermeldung, die ein echtes Problem aufdeckt, bekommt hier einen Eintrag (R-n) mit
Run-Verweis, Befund und abgeleitetem TODO. Kurzverweis „R-n" zusätzlich in die Tabellen-Spalte.*

| ID | Stufe | Beleg-Run | Befund | TODO (Schärfung) | Status |
|---|---|---|---|---|---|
| R-1 | A1 unused-compare | `runs/ledger/20260723_082646_950181` (`missingTarget=2`) und **reproduziert** `runs/ledger/20260723_084102_33155c` (`missingTarget=11` — exakt der letzte 30er-Batch AU-0095…0115) | LLM-Compare vergab Deckungs-Urteile (`attach_as_evidence`/`already_covered_indirectly`) mit LEERER references-Liste. Wurzelursache: Batch-Effekt + der Prompt verlangte die Referenz nicht hart (Schema erlaubt `[]`). Trace-Check lehnt korrekt ab, aber es gab keinen Repair-Loop ⇒ Ganz-Lauf-Fail wegen eines Modell-Fouls. | Dreifach-Fix in `UnusedUnitLedgerComparer` (2026-07-23): ① STRENGE-REGEL im Prompt (Deckung NUR mit existierender Candidate-ID, sonst needs_human) ② gezielter Nachfrage-Pass nur für Verstoß-Units (Repair-Antworten werden auf existierende IDs sanitisiert) ③ deterministisches Downgrade → `needs_human` (landet via Miss-Signal in der Adjudikations-Queue). Konsole zeigt `[unused-compare] reference-repair: offenders/repaired/downgraded`. +4 Tests (`UnusedUnitCompareRepairTests`), 80/80. | **behoben** — Laufzeit-Beweis: nächster A1-Run |
| R-2 | A2 Adjudikation (Runbook-Lücke) | entdeckt bei der R-1-Diagnose (Code: `ledger-adjudicate <validated> [miss-signal.json]`) | Das Runbook-A2-Kommando übergab das Miss-Signal (unused-compare-Ergebnis) NICHT — `missing_claim`/`needs_human`-Units (inkl. R-1-Downgrades!) hätten die Adjudikations-Queue nie erreicht: stiller Coverage-Verlust am Human-Gate. | A2/B2-Kommandos um `step-01d-unused-unit-ledger-compare/output.json` als 2. Argument ergänzt. | **behoben** (Runbook) |
| R-3 | A1 unused-compare (MAF-Sichtbarkeit) | Autor-Wunsch nach R-1-Fix (Run `20260723_090059` beweist die Funktion) | Der Referenz-Repair-Pass lebt IN der Comparer-Komponente, nicht als eigener Executor-Knoten im MAF-Graphen (anders als `CanonicalCoverageRepairExecutor`) — funktional gleich, aber im Workflow-Event-Stream unsichtbar. | Repair als eigenen Workflow-Knoten heben (nach dem E2E; rein strukturell, keine Fachlogik-Änderung). | **GEFIXT (05.08., mit Schritt 5 ④):** `UnusedCompareReferenceRepairExecutor` = eigener sichtbarer Knoten (Compare sendet typisierten Draft `UnusedCompareDraftMessage`, Repair-Knoten ruft die UNVERÄNDERTE Fachlogik `RepairCoverageReferencesAsync`, schreibt step-01d exakt wie zuvor, Event `LEDGER_UNUSED_REFERENCE_REPAIR`). |
| R-4 | alle Stufen (Doku) | Autor-Wunsch 2026-07-23 | Es fehlt eine nachschlagbare Doku je Kettenstufe: welche Steps/Executor-Knoten gibt es, was ist LLM vs. deterministisch, welche Artefakte entstehen, wie greifen sie ineinander. | **README je Stufenordner** (`01-ledger/README.md` …). Vorgehen: je Stufe DANN dokumentieren, wenn der E2E sie durchläuft (frisches Wissen + Run-Beleg). 01-ledger + 02-baselines + 03-gap: erledigt 2026-07-23. | in Arbeit |
| R-6 | A1 Prompt-Sprache | Queue des Runs `20260723_090059`: Propositions ENGLISCH (Begründungen deutsch); alter Core war durchgehend deutsch | KEINER der 01-ledger-Prompts hatte eine Sprach-Vorgabe — das Modell driftet bei Extraktion/Kanonisierung ins Englische. Propositions sind DIE Produkt-Texte und fließen bis Core/Backlog/Issues ⇒ Sprach-Regression + verzerrter Alt/Neu-Vergleich. | „SPRACHE (Pflicht): Deutsch für propositions/reasons/Freitexte, Enums/IDs bleiben englisch" in alle 10 content-erzeugenden Prompts (Extractor beide Pfade, Canonicalizer, CoverageRepairer, FacetValidator, FacetAssigner, Comparer inkl. Repair, Triage, Reviewer). Build 0/0 · 80/80. **A1 muss neu laufen** (4. Lauf); Sprache der 02-baselines-Artefakte bei A3 beobachten. | **behoben & bewiesen** (Lauf `093443`: Propositions deutsch) |
| R-7 | alle Review-UIs (Einheitlichkeit) | Autor-Wunsch 2026-07-23 (bei R-5/R-6-Arbeit) | i-Button-Hilfe (`ReviewHelp` + Sektionen) hatten nur die 4 Tor-UIs (pbi-update, decision, github-forward/-reverse); Adjudikation hatte KEINE, übrige Review-UIs (L3, Backlog-Familie: issuplanning, clarification, openrequirements, re-clarify, completion …) vermutlich auch nicht. UIs sollen maximal einheitlich sein (gleiche Shell, gleiche Hilfe-Tiefe: Felder + Werte + WIRKUNG). | Adjudikation: erledigt (9 Hilfe-Sektionen, synchron mit README + `AdjudicationCompletenessGate.Project`). Restliche UIs: Hilfe-Abdeckung prüfen und nachziehen, jeweils wenn der E2E die Stufe erreicht (wie R-4-READMEs). | in Arbeit |
| R-9 | A2 Adjudikations-UI (Apply-Stolperfalle) | Run `20260723_093443`: „FERTIG — … kein validated Ledger übergeben → apply separat" — User erwartete den consumable nach Klick auf Fertig | Runbook-A2 startete die UI OHNE das optionale 2. Argument (validated ledger) — dann speichert Fertig nur die Queue, apply ist ein separates Kommando. Kein Datenverlust (Autosave), aber verwirrende UX am wichtigsten Human-Gate. | Runbook-A2/B2: UI-Aufruf MIT validated ledger (Fertig → auto-apply) + Fallback dokumentiert; apply für den laufenden Run nachgeholt (49 Claims, pending=0, Gate pass). Kandidat für später: UI könnte den validated-Pfad selbst aus der Run-Struktur ableiten statt ihn als Argument zu verlangen. | **behoben** (Runbook) |
| R-10 | A3 recipe/Derivations (Regression) | Run `20260723_113115_35335a`: Konsole „derived.json = **-1 items**", `derivations/*/` LEER; `events.jsonl` hat NULL `BASELINE_SELECTED`/`DERIVATION_*`-Events (Workflow endet nach `BRANCH_IDS_ASSIGNED`). Gegenbeweis: Alt-Run `runsArchive/recipe/20260709_143733_299d8c` (ebenfalls mode=build) HAT alle Derivation-Events + derived.json | Die Derivation-Fan-out-Stufe des Recipe-Graphen läuft seit dem Refactoring nicht mehr an — und der Runner meldet das nur als „-1 items" statt als Fehler (silent-cap-Verstoß). Baselines selbst sind intakt. | **Diagnose-Ergebnis (23.07.):** `mode:load`-Repro (Run `20260723_114515_f3f537`) FUNKTIONIERT — 7 Risiko- + 9 Gap-Items, deutsch. Bruch damit exakt isoliert: **Zustellung Zweig→Collector INNERHALB des gebundenen Fan-out (nur build-Modus)**; Select/Derivation intakt; Code+MAF-Paket seit dem funktionierenden Stand UNVERÄNDERT (Diff leer, Versionen gepinnt) — Ursache noch unklar. **Workaround etabliert:** zweistufig fahren (build für Baselines → load-Recipe für Derivations). **Update 23.07. (Run B3 `134102`): Derivations liefen im build-Modus DURCH — der Defekt ist INTERMITTIEREND (Race in der verschachtelten Zustellung Zweig→Collector; passt zur historischen Concurrency-Notiz im Collector: 09.07. waren auch 2 von 3 Läufen betroffen).** Fix-Welle offen: Race mit Stub-ChatClient reproduzieren/fixen (Dry-Run führt den Graph nicht aus); Runner härten („-1" ⇒ Fehler-Exit). **Update 25.07. (MAF-Upgrade rc1→1.15.0, Branch `upgrade`):** ① Doku-Recherche präzisierte die Ursachen-Lage: der Collector-Kommentar belegt NEBENLÄUFIGE Barrier-Zustellung an den Einzel-Nachricht-Collector (Lauf 5b745e: 1 statt 2 collected) — der `lock`-Fix im Collector ist das korrekte Muster (offizielle Doku: Barrier „streamed to the target" = Ziel akkumuliert selbst; Thread-Safety = unsere Pflicht laut State-Isolation-Doku). MAF-seitig sind Fan-in-Bugs bekannt (#1863 offen, #2157 closed) + Release Notes nennen „message-order fixes". ② **Empirie auf 1.15.0: 4/4 recipe-build-Läufe (2 Baselines + 2 Derivations, exakt die Race-Naht) mit VOLLER Event-Kette** (BASELINE_SELECTED/DERIVATION_*/FINALIZED alle da, nie „-1") — auf rc1 rissen ~2 von 3. Race nicht mehr reproduzierbar (starkes, nicht abschließendes Signal). Läufe `runs/recipe/20260725_1429*–1432*`. Runner-Härtung („-1"/Idle ⇒ Fehler-Exit) bleibt sinnvoll + offen. | **Workaround verfügbar; auf MAF 1.15.0 nicht mehr reproduziert (4/4 grün)** — Härtung offen |
| R-17 | B7 Forward-Plan: Skalengrenze + Repair ersetzt statt merged | Run `20260723_143518_97518a` (Details hier festgehalten; Run-Ordner bereinigt — entgegen der Behalten-Regel, mea culpa): 43 Delta-PBIs → Versuch 1: nur ~18 adressiert (25× `PBI_NOT_ADDRESSED`, decision=Repair); Versuch 2: die 25 ergänzt, aber 18 ZUVOR adressierte verloren → `MaxAttemptsReached`, Gate block (korrekt!) | Zwei Schwächen: ① Plan-Agent schafft ~20–25 Ops pro Antwort (Output-Grenze) ② der Repair-Durchlauf ERSETZT den Plan statt fehlende Ops zu MERGEN ⇒ große Deltas konvergieren nie. Gate + MaxAttempts haben sauber blockiert (kein kaputter Apply). | ① Repair-Merge im Forward (vorherige Ops behalten, nur Fehlende ergänzen) ② `github-initial-sync` deterministisch bauen (CREATEs aus PBI-Payload brauchen keinen Agenten — R-15) ③ **Workaround FINAL (23.07.): deterministischer Plan statt Agent** — 2. Fehlversuch (`runs/github-forward/20260723_144216_c743ce`, bleibt als Beleg) zeigte: Limit liegt bei ~11 Ops/Antwort, Chunking hieße 4–5 Reviews. Stattdessen synthetischer Forward-Run `runs/github-forward/_initial-sync-det` mit 43 deterministisch aus Core-Payloads gebauten CREATE_ISSUE-Ops (echtes Plan-Format; Review+Apply laufen über die NORMALEN Kommandos+Gates). = gelebter Prototyp des R-15-Kommandos. | **Workaround aktiv (det. Plan)**, Merge-Fix offen — **Vertiefung 10.08. direkt unter der Tabelle (WARUM + 3 Lösungen + Verallgemeinerung); Autor-Entscheid: JETZT nicht bauen, später abwägen.** |
| R-18 | E11 ingest-resume (Stream endet vor Apply-Output) | Run `20260723_150814_4d792d`: UI 2/2 → Response gesendet → Events zeigen `REQ_INGEST_APPLY_START` (15:09:03) → danach NICHTS (kein applied/, kein Report); Shell: „Resume beendet ohne Apply-Report" | **KORRIGIERT nach Reproduktion: KEIN Race.** `HitlShell.ResumeAsync` behandelte nur Request-/Output-Events — `ExecutorFailedEvent`/`WorkflowErrorEvent` wurden STILL verschluckt, der Stream „endete einfach". Dahinter steckte R-19 (Null-Bug im Apply). Race-Verdacht war Fehldiagnose (ehrlich vermerkt); R-10 bleibt davon unberührt (dortige Evidenz ist event-basiert anders). | **Shell-Fix:** ResumeAsync meldet Executor-/Workflow-Fehler jetzt LAUT (Exit 3) statt „ohne Apply-Report". | **behoben** (Build 0/0 · 80/80) |
| R-24 | Orchestrierung: pipeline-full + Steward als Forschungs-Vergleich | Autor-Frage 23.07. („ist es überhaupt ein FULL workflow?"); Beweis-Basis: `pipeline-hitl` (EIN durabler MAF-Lauf ingest→pbi, 2 RequestPort-Gates, prozessübergreifend resumierbar) | Der Full-Workflow existiert heute als Kommando-Kette (Governance-bedingt: Gates!), nicht als EIN Graph. Beides ist baubar: **(a) pipeline-full** = Transkript→GitHub als EIN durabler MAF-Graph mit N Human-Gates (~2–4 Sessions; Brocken: Adjudikation als RequestPort-Gate); **(b) Steward** (Plan 20.07) = agent-orchestriert, ruft Stufen-Workflows als Tools. | Beide bauen und als **Orchestrierungs-Vergleich** messen (human/CLI- vs. graph- vs. agent-orchestriert, gleiche Aufgabe = meeting-2-extended-Benchmark E1–E11). | offen (Thesis-Kapitel!) |
| R-25 | Experiment-Steuerung via run-config | Autor-Wunsch 23.07. (Phase-2-Gefühl: „run config, wo ich das ganze steuern konnte") | Stufen lesen run-config bereits (evidenceAgent, jury, l3-Modi), aber es fehlt eine `fullworkflow`-Sektion: Modelle je Stufe, maxAttempts, **Gate-POLICIES** (`interactive` \| `accept-all` \| `accept-list`), Transkript/Repo/Ziel. | Sektion bauen; Policies ausdrücklich als EXPERIMENT-Modus deklarieren (auto-accept = Messbetrieb für N≥3-Wiederholungen, NICHT Produktions-Governance). Voraussetzung für die Messmethodik (N≥3, Ablationen). | **gebaut (code-verifiziert 30.07.):** `run-config.json` `fullworkflow`-Sektion mit Policy je Gate (`accept-all`) + GateResponder; als EXPERIMENT-Modus deklariert. Restfeinschliff (det. Gates härten) optional. |
| R-26 | pipeline-full: Klärungs-Stufe (06-backlog) fehlt → `needs_clarify` erreicht Forward ungeklärt | Voller pipeline-full-Dry-Run 25.07. (Run `20260725_095352_23c332`, Mini-Transkript): Forward-Gate `PBI_NOT_ADDRESSED` für PBI-044/045 (beide Status `needs_clarify`, neu, kein Issue) — auch mit gpt-5.4, nicht nur mini. Plan hatte 1 Op (UPDATE_ISSUE #5 für das gemappte PBI-005), die zwei neuen unklaren Punkte still übersprungen. | **Zwei Ursachen isoliert:** ① synthetische Testdaten (3-Satz-Transkript vs. reales 39-Issue-Repo — erfundene PBIs passen nicht sauber). ② **pipeline-full überspringt die 06-backlog-Clarification-Stufe** (Code-Zweck: „offene Klärungsitems agentisch lösen/vorschlagen"). `needs_clarify` wird beim **Pbi-Placement** (`MARK_CHANGED`) gesetzt, aber **nie aufgelöst**, bevor es zum Forward geht → der Forward sieht rohe unklare PBIs und verweigert **korrekt** (Governance, kein Bug). **Zusatz-Fund:** unauflösbare/`needs_clarify`-Fälle enden am Forward in `MaxAttemptsReached` (Sackgasse) — der Human-Gate sitzt erst NACH dem det. Quality-Gate, wird also bei Dauer-Fail nie erreicht. | ① **06-backlog-Clarification als echte Stufe VOR den Forward einhängen** (pipeline-full ist ohne sie nicht die volle Kette; dann kommen nur geklärte Punkte an). ② Optional: Conditional Edge „`needs_clarify`/unauflösbar → HITL" (Eskalation zum Menschen statt Sackgasse) — der Placement/pbi-gate ist der frühe, natürliche Klärungs-Punkt. Wiring-Gerüst steht; Forward-Gate arbeitet korrekt. **Vertiefung 25.07. (Bau-Versuch b):** Einhängen ist KEIN mechanischer Slice — 06-backlog ist ein mehrstufiges L4-Sub-System (seed→agent→review→apply, eigenes Gate). Der `ClarificationAgentRunner` liest eine Core-View, produziert aber **separate Klärungs-Issues** (`CreateOneClarificationIssuePerItem`) und **löst die `needs_clarify`-PBIs NICHT auf** → das Forward-Gate bliebe `PBI_NOT_ADDRESSED`. **Echte offene Design-Entscheidung (der R-26-Kern):** wie fließen `needs_clarify`-PBIs nach GitHub — (i) Status am pbi-gate/Clarification auflösen, (ii) als expliziter `defer/skip`-Op im Forward markieren (statt still weglassen), oder (iii) als Klärungs-Issue projizieren? Erst DIESE Entscheidung, dann bauen — nicht umgekehrt. **PRÄZISIONS-KORREKTUR 25.07. (Code-Verifikation):** ⓐ Das Forward-Gate prüft `needs_clarify` GAR NICHT — es verlangt nur je Delta-PBI genau 1 Op (Coverage); der Fail war eine Coverage-Lücke bei NEUEN PBIs (Agent lieferte keinen CREATE-Op), kein Governance-Refusal wegen Unklarheit. ⓑ Option (iii) ist tot: der `ClarificationAgentRunner` arbeitet auf OFFENEN REQUIREMENTS, nicht auf `needs_clarify`-PBIs — er projiziert die PBIs nicht. **ENTSCHEID + FIX 25.07. (Option B gebaut):** neues, unmapped `needs_clarify`-PBI wird im unbeaufsichtigten Lauf (`github-forward-gate=accept-all`) deterministisch als `HOLD_CLARIFY` geparkt (Spiegelbild zu `HOLD_BLOCKED`, nie CREATE) → erfüllt Coverage, keine Sackgasse, sichtbar. Policy-getrieben: `interactive`/`replay` = Flag aus, bit-identisch zum CLI (M-1). Gemappte `needs_clarify` bleiben `UPDATE_ISSUE`. Build 0/0 · 124 Tests (7 neu) · Smoke 14/0. Das AUFLÖSEN (`needs_clarify`→`active`) bleibt Folgeschritt (Option C / Steward) + **Parkplatz-Mechanismus im Core** (geparkte Items führen/freigeben) — vermerkt in design-note §9. | **B gebaut 25.07. · C GEBAUT + BEWIESEN + COMMITTED 28.07. (= R-26-C / A1).** Auflöse-Pfad steht: `PbiAlignmentAgent` schlägt bei MARK_CHANGED/SUPERSEDE den angeglichenen PBI-Inhalt vor → edit-fähiges PBI-Update-Review (gruppiert am Op, zwei getrennte Entscheidungen) → `PbiUpdateApply` schreibt akzeptierte Angleichung, `needs_clarify → active`, R-31-Provenance. In ALLEN vier Workflow-Pfaden vor dem RequestPort-Human-Gate; Align-Kante nur bei `GateDecision.Pass`. 179 Tests · Smoke 14/0 · LLM-Beweis Run `20260728_122036_79a00b` (4 wahrheitstreue deutsche Vorschläge). Detail: `done/2026-08-03/hitl-abschluss-plan.md §0b A1`. **Rest von R-26 (unklare NEUE PBIs ohne Mapping → HOLD_CLARIFY) bleibt; die Skelett-PBI-Ausarbeitung ist Thema von `done/2026-08-07/operational-backlog-fortfuehrung-plan.md`.** **Update 30.07.: Operational-Backlog GEBAUT (O1–O5 + B1/B2, live bewiesen) → R-26 vollständig geschlossen; die unauflösbare Restklasse (needs_clarify → Decision) fällt in R-14.** |
| R-23 | Tor 3 UPDATE-Op-Inhaltsqualität | Issue #33 nach E11-Update: Body = Stub („PBI PBI-033 — Status … Abgedeckte Requirements: REQ-76"), Titel weiterhin „sieben Tagen" | Der Agent ERSETZTE den reichen Initial-Body (Ziel/Requirement-Texte/Akzeptanzkriterien) durch eine dünne Statuszeile; die eigentliche Änderung (14 Tage, PDF-Export) steht NIRGENDS im Issue. Mechanik bewiesen, Inhalt regressiv. | Update-Ops deterministisch bauen wie der Initial-Sync-Body (R-15-Kommando-Familie) ODER Änderungen als KOMMENTAR mit Diff statt Body-Ersatz; Titel-Refresh. Gehört zur Qualitäts-Familie R-17/R-20. | **Fix GEBAUT 27.07. (E0.1c/E0.2):** EINE det. Body-Quelle `GithubIssueBodySections.Build` für CREATE **und** UPDATE-Vorschlag (Statement+AK+Reqs, interne Zustände als Sync-Metadaten-Fußzeile); Test beweist Statement+AK im Body durchs echte Gate. Zusätzlich zeigt das Forward-Review seit E0.2 den semantischen Diff (Stub-Regression wäre sofort sichtbar gewesen). Live-Beweis am nächsten operativen Update-Lauf offen. |
| R-27 | pipeline-full B6: Forward-Dry-Run plant 0 Ops — Maker-Kurzschluss `ctx.DryRun ⇒ kein Agent` | B6-Beweislauf 27.07. (Run `runs/fullworkflow/20260727_052405_3c8e13`): Bootstrap-Zweig komplett grün (Core 0→71: 38 req + 12 features + 21 PBIs; beide neuen Gates accept-all beantwortet; initial-sync-Delta 21 CREATE-Kandidaten), aber Forward-Gate FAIL `PBI_NOT_ADDRESSED` ×21, attempts=2, `MaxAttemptsReached`; summary `agentOps=0`; KEIN GithubForwardAgent-Ordner in logs/agents (kein LLM-Call). | **Mechanik, kein Modell-Thema:** `GithubForwardAgentRunner.RunAsync` (GithubForwardWorkflow.cs:171) returnt bei `ctx.DryRun` bewusst `[]` (Spar-/Smoke-Logik) — kollidiert mit dem B6-Ziel „Dry-Run plant N CREATEs". Zusätzlich bestätigt: bekannte R-26-Sackgassen-Mechanik (Human-Gate sitzt NACH dem det. Gate, wird bei Dauer-Fail nie erreicht). | **Fix-Empfehlung = R-17-Präzedenz:** deterministischer Initial-Sync-PLAN (CREATE-Ops aus Core-Payloads, `origin=deterministic`, ehrliche searchEvidence „frisches Repo/leerer Snapshot") als Bootstrap-/Dry-Run-Variante — der Agent kann ohne Repo/Token ohnehin nicht suchen; Agent-Plan bleibt als W2-Ablationsarm (b). | **Fix GEBAUT + getestet 27.07.:** `GithubInitialSync.BuildCreateOps` (det. CREATE-Ops aus Core-Payloads, Label initial-sync, ehrliche Such-Evidenz) via `ctx.InitialSyncDeterministic` VOR dem DryRun-Kurzschluss; Seed-HOLD-Governance unberührt. Integrations-Test: Seed+Ops bestehen das ECHTE Gate (inkl. HOLD_BLOCKED/HOLD_CLARIFY). **Live-BEWIESEN 27.07.** (Lauf `20260727_055348_bb9b1a`): 25/25 CREATE_ISSUE deterministic, Gate pass (0 Fehler, attempt 1, finalDecision Pass), github-forward-gate beantwortet, Apply-Dry-Run success=True, KEIN Forward-LLM-Call. |
| R-29 | H1-Resume: `--accept-all` beantwortete ALLE folgenden interactive-Gates statt genau eines | H1/H2-Live-Beweis 27.07., Lauf 1 (Run `20260727_074919_b2c654`, mini): Pause@adjudication ✅, resume --accept-all ✅ (Restore ohne Ledger-Wiederholung, 66/21 Aktionen per Heuristik) — aber das cluster-review-gate wurde MIT beantwortet (`source: cli-flags`) statt erneut zu pausieren; Kette lief bis exit 0 durch (mini-Clarify lieferte 0 PBIs → Forward sauber übersprungen). | **Mechanik:** die Resume-Flags galten global für den Reststream — pipeline-hitl beantwortet pro Resume genau EIN Gate („responded"). | **Fix GEBAUT + LIVE BEWIESEN 27.07.** (Lauf 2, Run `20260727_080638_9d4ae8`): `ResumeAnswers` konsumierbar (TakeFor/TakeAcceptAll — Flags einmalig; Entscheid-DATEIEN weiter an jedem Gate) → resume beantwortete NUR die Adjudikation und pausierte echt erneut am cluster-review-gate (checkpointId 47190c…). Mehr-Gate-Zyklus komplett bewiesen. | **gefixt + live bewiesen** |
| R-28 | pipeline-full B6: metrics.json-Attribution im Bootstrap-Zweig falsch | Gleicher Run: `07-ingest` und `07-pbi-update` zeigen je wallMs=0 aber tokens_in=312.259/out=79.518 (liefen NIE — Bootstrap-Zweig!); die echten LLM-Stufen (06-backlog cluster/clarify, ~13 Model-Rounds) fehlen als Stufen komplett; 07-github tokens=0 (hier korrekt, da kein Call — s. R-27). | **Mechanik:** `MetricsFinalizer` fenstert per `STAGE_BACKHALF_START`→`PIPELINE_BRIDGE`→`STAGE_FORWARD_START` — diese Events existieren im Bootstrap-Zweig nicht → Fenster kollabieren, der ganze Faden wird Ingest/Pbi doppelt zugeschrieben. | MetricsFinalizer bootstrap-fähig machen: Stufen `06-backlog-cluster`/`06-backlog-clarify` (Fenster `STAGE_BOOTSTRAP_START`→`CLUSTER_GATE_REQUEST`→`BACKLOG_GATE_REQUEST`→`STAGE_FORWARD_START`), Ingest/Pbi im Bootstrap-Zweig = 0/absent. Bis dahin: Bootstrap-metrics.json NICHT für Messwerte verwenden. | **Fix GEBAUT + getestet 27.07.:** Bootstrap-Erkennung via `STAGE_BOOTSTRAP_START`; Stufen `06-backlog-cluster`/`06-backlog-clarify` mit Fenstern [Bootstrap..ClusterApplied..Forward], Ingest/Pbi entfallen im Bootstrap-Zweig (Fixture-Test: keine Doppelzählung). Alte Bootstrap-metrics (Lauf 052405) bleiben ungültig. |
| R-30 | Tor 3 UPDATE ersetzt Issue-LABELS durch Requirement-IDs | Fund im E0.2-UI-Review 27.07. (Autor sah „+ Labels: REQ-61, REQ-77" im Diff; Sandbox-Plan `152440`, Issue-#32-Snapshot-Labels: initial-sync, needs-clarify, pbi) | **Mechanik:** `GithubForwardSeed` setzt bei UPDATE `op.Labels = CoveredRequirementIds`; `GithubForwardApply` → `UpdateIssueAsync` PATCHt das `labels`-Feld — GitHub-REST-Semantik: **kompletter Ersatz der Label-Liste**. Ein ausgeführtes UPDATE löscht also initial-sync/pbi/needs-clarify und legt REQ-IDs als GitHub-Labels an. Vermutlich am 23.07. bei #32/#33 real passiert (beim nächsten Snapshot prüfbar). | **Entscheid nötig (Folge-Logik!):** (a) Labels bei UPDATE unangetastet lassen (Feld im PATCH weglassen) oder (b) Requirement-Labels bewusst als Feature (dann additiv statt Ersatz + auch beim CREATE). Bis dahin: das Forward-Review zeigt die Ersetzung als WARNUNG am Item (E0.2) — der Mensch kann skip entscheiden. | **Fix (a) GEBAUT 27.07. (Autor-Go):** Seed setzt bei UPDATE keine Labels mehr (null = nicht anfassen); Client lässt `labels` im PATCH per WhenWritingNull weg (R-22-Muster); Apply reicht null durch statt `?? []` (hätte die Labels GELEERT). Test: UPDATE-Op trägt Labels=null. UI-Warnung bleibt für Alt-Pläne/Agent-Ops aktiv. Live-Beweis am nächsten Execute-Lauf offen. |
| R-31 | Provenienz: geänderte Requirements verankern die Herkunft der Änderung nicht | Fund im E0.3-UI-Review 27.07. (Autor an op-4, MARK_CHANGED REQ-08): die neue Fassung „Ob Angehörige selbst Inhalte eintragen dürfen oder nur Leserechte erhalten, muss noch geklärt werden" entspricht wörtlich der Meeting-2-Nachfrage (`meeting-2-extended.txt` Z.34); die alte, inhaltlich reichere Fassung („Angehörige sollen … Inhalte beitragen können; Rechte vorgesehen") stammt aus dem Interview + Baseline-Lauf `113115`. | **Mechanik:** Das Core-Item REQ-08 trägt für die NEUE Fassung (v2) `sourceRunId`=`20260723_113115_35335a` — also den **Baseline-Lauf, der nur die ALTE Fassung produzierte** (dessen `maker.md`+`final.md` belegen ausschließlich die alte Formulierung). Die Herkunft „diese Änderung kam aus Meeting-2" ist im Item **nicht** verankert; die History-Note nennt keinen Meeting-/Ingest-Bezug. Rückverfolgbarkeit (Kernprinzip!) an dieser Stelle löchrig. **Zusatzbefund (inhaltlich, = R-26-Beleg):** die „Verfeinerung" ist eine semantische REGRESSION — eine bloße Nachfrage im Meeting schwächte eine im Interview GEKLÄRTE Anforderung zu einer offenen Frage ab. | Beim Ingestion-Update `sourceRunId` + History-Note der neuen Fassung auf den auslösenden Meeting-/Ingest-Lauf setzen (statt den Baseline-Lauf zu erben). **NEU-Beleg 03.08. (systematisch, nicht Einzelfall):** REQ-56 trägt als sourceRunId den recipe-Lauf `134102`, `ingestedFrom`=nur die Incoming-ID — der verursachende Ingest-Lauf ist am Item NICHT auffindbar (musste über Run-Ordner rekonstruiert werden). **= Invariante I7 (`core-relationen-konzept.md`).** | **GEFIXT 04.08. (I7, Writer-seitig):** `IngestionApply` bekommt den Auslöser-Lauf (`ingestRunId`) durchgereicht (CLI aus dem planDir-Namen, HITL/pipeline aus `run.RunId`) — jede INHALTS-Mutation (NEW/NEW_RELATED, REFINE, SUPERSEDE-Neuanlage, CONTRADICT-DEC) trägt ihn als `sourceRunId`; der Delta-Lauf bleibt via Metadatum `ingestedFromRun` auffindbar; die REFINE-History-Note nennt Lauf + Incoming. RESTATE/ALREADY_DECIDED = reine Claim-Merges → Provenance unberührt (PbiUpdate-Regel). 6 Tests (`IngestionApplyProvenanceTests`), 345 gesamt · Smoke 14/0. **Bestand wird NICHT rückwirkend umgeschrieben** (Alt-Items = ehrliche Historie; der Ausnahme-Befund REQ-76/77 ohne Claim-Anker bleibt als Daten-Notiz in aufgefallen §2). |
| R-32 | Baseline-Checker-Verdikt versickert im operativen Graph (nicht sichtbar) | Code-Verifikation 01.08. (Vor-R-14-Audit, `done/2026-08-07/r14-entscheidung.md §3b SP2` + §SP2-Fix): ein Baseline-Artefakt, bei dem der maker-checker aufgab (`MaxIterationsReached`) oder „Rest → Mensch" flaggte (`HumanReview`), fließt operativ **ohne Human-Sicht** in den Core. | **Mechanik:** `BaselineFanOutRunner.cs:189` gibt `return 0` UNABHÄNGIG vom Checker-Verdikt (nur echte Executor-Abstürze → 3, `:168`); `BaselineStageExecutor.cs:47` prüft nur `exit!=0 || artifact.json fehlt`, **nicht** das Verdikt. Das „Rest → Mensch"-Flag lebt nur in `baselines/<art>/final-report.json`, die im Betrieb niemand öffnet. Der Standalone-`artifact-branch` surfacet es korrekt via Exit 1 (`ArtifactBranchRunner.cs:190`: `decision==Pass ? 0 : 1`) — der operative `recipe`/Fan-out-Pfad NICHT. | **Fix (eigener kleiner Slice, VOR R-14, entscheidungs-unabhängig):** Checker-Verdikt im operativen Graph an ein Signal/Gate hängen — (a) Fan-out/Recipe propagiert Nicht-Pass an nicht-Null-Exit, ODER (b, MAF-nah) `BaselineStageExecutor` liest `final-report.json.decision` und eskaliert bei ≠Pass (Event `STAGE_BASELINES_NEEDS_HUMAN` + konditionale Kante zu HITL/Warnung) statt still weiter. Nur aufdecken/eskalieren, keine inhaltliche Auflösung. | **GEFIXT (04.08., Option b MAF-nah):** `BaselineFinalReport.TryRead` (pure Leser-Klasse in 02-baselines) + `BaselineStageExecutor` liest das Verdikt neben `artifact.json`: ≠Pass ⇒ Event `STAGE_BASELINES_NEEDS_HUMAN` + lauter Terminal-Stopp (Muster FAILED-Pfad), KEIN `ProjectStateBuildRequest` — nichts fließt still Richtung Core. Fehlender/kaputter Report = fail-open (Warn-Event `STAGE_BASELINES_VERDICT_MISSING`, Alt-Läufe brechen nicht). Pass-Pfad unverändert. Beleg des Lochs: realer Lauf `runs/recipe/20260727_055907_0f5dd9` (decision=MaxIterationsReached, floss damals still). 5 Tests · 320/320 · Smoke 14/0. CLI-Pfade bewusst unverändert (artifact-branch macht es schon richtig). |
| R-36 | Ingest NEW_RELATED schreibt `part_of_feature` auf den featureKey-STRING statt auf ein Feature-Item | **Messung am Produktiv-Core 03.08.** (Core-Wahrheits-Audit): **12 hängende Relationen** `REQ-56…` → „no-go" u. a. — das Relations-Ziel existiert nicht als Item. | **Mechanik:** `IngestionApply.cs:85` (NEW_RELATED): `AddRel(newId, op.FeatureKey, "part_of_feature")` — FeatureKey ist ein semantischer Schlüssel („no-go", „medikamente"), keine Feature-Item-ID (`FC-nn`). Einziger AKUTER Integritäts-Defekt der Messung (alle anderen Prüfungen grün: 0 Waisen-PBIs, 0 uncovered, 0 covers→superseded, 0 ohne sourceRunId). | **GEFIXT 03.08. (v2 deterministisch):** ① `IngestionApply` schreibt bei NEW_RELATED KEINE Relation mehr — featureKey = reines Hinweis-Metadatum für die Placement-Stufe (Gate-Check UNKNOWN_FEATURE → Warnung); ② `PbiUpdateApply` leitet REQ→Feature deterministisch aus der bestätigten Deckung ab (Seed-Regel „aus den PBIs abgeleitet", idempotent; NEW_PBI/EXTEND/SUPERSEDE-Ersatz); ③ Bestand per Einmal-Werkzeug `core-heal-features` selbst-geheilt: **12→0** (11 retargeted nach Deckungs-PBI, REQ-63 superseded → entfernt; 0 ungelöst), Audit `runs/core-heal-features/20260803_172914_4bfacc`, Werkzeug danach entfernt (S6-Muster). Der Agent benennt keine Feature-Keys mehr als Relationsziel — der Fehlerweg existiert nicht mehr. S4-Port-Validator (I1) sichert die Klasse dauerhaft. | **GEFIXT (03.08.)** |
| R-37 | Fan-out/Recipe: ALLE Artefakt-Zweige sahen die requirements-gekeyte Ledger-Projektion — kein Zweig außer requirements sah je seine eigene Dispositions-Weiche | 9g-Resttest 05.08.: OQ-Fanout auf frischem Extraktions-Stand lieferte 7 statt 2 Fragen — der Agent formulierte 5 `open-questions=not_applicable`-Claims trotz Prompt-Verbots zu „Wie wird umgesetzt…?"-Fragen um (Runs `20260805_110317` + Kontroll-Lauf `110543` nach erstem Filter-Versuch: unverändert 7). | **Mechanik (zweistufig gefunden):** ① Erst-Diagnose „Agent bricht Prompt-Verbot bei plausiblem Inhalt" → deterministischer not_applicable-Filter in `EvidenceLedgerProjection` — griff NICHT. ② Wurzel: `BaselineFanOutRunner:147` baute EINEN Broadcast-Block mit hartem Schlüssel `"requirements"` für ALLE Zweige (Kommentar dokumentierte es als Abkürzung: „Für Fan-out reicht EINE Projektion"); `RecipeRunner` analog mit `baselineArtifacts[0]` → **betrifft auch die 9g-Pipeline-Bahn**. Der OQ-Agent hat seine Weiche schlicht NIE gesehen; die scheinbare Juli-Disziplin war Korrelation, nicht Steuerung. | **GEFIXT 05.08.:** ① `EvidenceLedgerProjection` filtert not_applicable-Claims der jeweiligen Spur deterministisch heraus (geteilte Naht, alle 5 Aufrufer; „was der Agent nie sieht, kann er nicht fragen" — zugleich die erste Hälfte des 9g-Spur-Gates); ② `BuildBranch` gibt jedem Zweig via `sourceOverride` SEINE dispositions-gekeyte Projektion (`ArtifactBranchMakerExecutor`), Broadcast-Input = nur noch Trigger — wirkt für Fan-out-CLI UND Recipe/Ein-Graph (eine Naht, beide Bahnen). Beweis: Wiederholungs-Lauf `20260805_110816`: **exakt 2** Fragen (die echten); Delta+Resolver `110833`: 2× OPEN_QUESTION, Gate pass 0/0. 2 neue Tests (`EvidenceLedgerProjectionFilterTests`). **ENDFORM (gleicher Tag, Autor-Review „MAF-nah statt Override"):** das Übergangs-Muster (sourceOverride + "START"-Sentinel) KOMPLETT entfernt — der Fan-out-Dispatch nimmt das Consumable als typisierten Workflow-Input, prägt je Spur eine typisierte `BranchSource(artifactType, projektion)` und routet sie über Kanten-Prädikate (`AddEdge<BranchSource>(…, m => m.ArtifactType == type)`, das Haus-Muster der Loops); der Maker ist `Executor<BranchSource>` mit lautem Mismatch-Guard. Gilt für Fan-out-CLI, Recipe/Ein-Graph, Standalone-Branch UND Chain (alle Aufrufer umgestellt, kein Alt-Weg übrig). Verifikation: In-Process-Graph-Test des ECHTEN Fan-outs mit Fake-Zweigen (`BaselineFanOutRoutingTests`: je Zweig genau EINE eigene gekeyte+gefilterte Quelle, Barrier+Collector) + LLM-Bestätigung `20260805_114802` (wieder exakt 2 Fragen). 350 Tests · Smoke 14/0 · 0 Warnungen. MAF-Learnings: gebundene Subworkflows forwarden `YieldOutputAsync` (nicht SendMessage); `SendsMessage`/`YieldsOutput`-Kontrakte werden zur Laufzeit LAUT erzwungen. | **GEFIXT (05.08., MAF-native Endform)** |
| R-38 | UNVERDRAHTETER RequestPort in gebundenem Sub-Workflow = STILLER Deadlock (lautlos gedroppte ExternalRequest-Nachricht) | Gezielter Spike 05.08. (Kapsel-Endbild-Frage des Autors vor Schritt 5 ②): der Naiv-Test hing ENDLOS — Eltern-Stream zeigte die Kapsel-Executors, aber NIE ein `RequestInfoEvent`, NIE ein Fehler-Event; erst `kill` beendete den Testhost. **Erst-Verdikt „Framework-Grenze, nicht möglich" war ZU STARK — Autor-Einwand („100% sicher, dass es nicht an unserem Code liegt?") führte zum Doku-/Quellen-Check und zur Korrektur am selben Tag.** | **Mechanik (korrigiert, quellen-verifiziert):** die Kapsel (`WorkflowHostExecutor`) forwardet innere Port-Requests SEHR WOHL — aber als `ExternalRequest`-NACHRICHT in den Eltern-Graph, die man **explizit an einen Eltern-Port verdrahten muss** (offizielles Muster: Repo-Sample `09_Subworkflow_ExternalRequest` + Learn „Sub-Workflows/Requests and Responses"; `ForwardMessage<ExternalRequest>(kapsel→port)` + `ForwardMessage<ExternalResponse>(port→kapsel)`, Eltern-Port mit `AllowWrapped`-Default). OHNE diese Kanten greift die unmatched-message-Semantik: die Nachricht wird LAUTLOS gedroppt, der innere Port wartet ewig — der Deadlock war ALSO fehlende Verdrahtung bei uns, kein Framework-Verbot. Kontroll-Test top-level + verdrahteter Test isolieren das sauber. | **ABGESICHERT + AUFGELÖST 05.08.:** ① `Aufloesung_`-Test: verdrahtete Kapsel-Rundreise GRÜN (RequestInfoEvent surfaced als ELTERN-Port `spike-parent-gate` → Runner-PortId-Dispatch bliebe unverändert; Antwort fließt zurück, Output `PARENT:INNER:JA`) · ② `Befund_`-Test pinnt die stille Falle (unverdrahtet = Drop+Deadlock) · ③ `BindGateFree`-Wächter an allen 8 Bind-Stellen (`PORT_IN_CAPSULE`; Fehlermeldung nennt jetzt BEIDE legale Wege: flach ODER bewusste Forward-Verdrahtung + nackter Bind). Konsequenz: Kapsel-Endbild ist MÖGLICH → Design-Entscheid geparkt (aufgefallen 9j). | **ABGESICHERT + KORRIGIERT (05.08., Spike + offizielles Muster verifiziert)** |
| R-39 | `supersedes`-Endpunktregel war req-hart — der Kangal wies die ERSTE echte arch-Ablösung ab | R-11-A1d-Echt-Lauf ② (05.08., gemischtes Delta `runs/fullworkflow/20260805_211636*`): der arch-Resolver wählte SUPERSEDE ARCH-39 → `CORE_KANGAL: I1_TARGET_INVALID: supersedes … Ziel ist kein Requirement`, Save LAUT abgebrochen, Exit 3 — **exakt der in Teil 5/D-7 vorhergesagte Konflikt-Typ, nur früher (supersedes statt covers)**; req-Teil war bereits applied → Core aus Backup restauriert (Experiment-Disziplin). | **Mechanik:** `CoreKangal.cs` supersedes-Case prüfte hart `Ziel==requirement` (Endpunkt-Matrix vom 04.08. kannte nur req→req); der arch-Strip erzeugt legitime arch→arch-Ablösungen. Der Wachhund tat exakt seinen Job: kein korrupter Write. | **GEFIXT 05.08. (D-7-Regel „Spec zuerst, Wächter folgt"):** `core-relationen-konzept` I1: supersedes = **aspekt-GLEICH** (Quelle.itemType == Ziel.itemType; req→req unverändert, arch→arch neu, künftige Aspekte automatisch); `CoreKangal.SameItemType`; 1 Test (pass/pass/cross-verboten). Wiederholungs-Lauf `211842`: FERTIG. Rest der D-7-Matrix (covers→arch, constrained_by) kommt planmäßig mit A3/A4. | **GEFIXT (05.08.)** |
| R-35 | Ingest-Skip ohne „vertagen"/Aufzeichnung — abgelehnte Meeting-Anforderung verschwindet spurlos | Code-Verifikation 03.08. (bei E0.7). Das Ingest-Gate ist strikt binär apply/skip; kein defer/park. | **Mechanik:** `AcceptedFromDecisions` (`IngestionApplyExec.cs:61-63`) = nur `apply`; `IngestionApply.cs:39` iteriert nur akzeptierte Ops; der `Skipped`-Report-Zähler zählt NUR fehlgeschlagene Applies, **nicht** menschliche Skips → ein Skip hinterlässt keine Spur, wird nicht geparkt, nicht später neu vorgelegt (Idempotenz auf `PlanId`). Verstoß gegen „nichts geht verloren" (weich). | **GESCHÄRFT 03.08. (E0.9-P2a gebaut = Erfassungs-Seite):** skip verlangt jetzt an allen Gates eine Begründung (UI-hart, `Resolved`) — das Warum existiert ab jetzt in `human-decisions.json`. **Rest-Fix (der eigentliche R-35-Slice): Wiedervorlage** — abgelehnte Ingest-Vorschläge als `ProjectStateProposal(status=rejected, reason, sourceRunId)` in den **Core** heben (die Proposal-Schiene mit `rejected`-Status existiert, heute L3-genutzt) → dauerhaft abrufbar + der nächste Ingest-Lauf/Adapter kann sie als Kontext lesen („am 03.08. abgelehnt, Grund: …"). Koppelt den offenen **ProposalStatus-Cleanup** (§5-Nachzügler) + Parkplatz-Spec (`done/2026-08-07/stabilisierungs-plan.md § accept_gap`). | **GEFIXT (04.08., Teil 2 gebaut):** ① `IngestionRejections.Record` in `IngestionApplyExec` (EINE Stelle, gleicher Save/Snapshot/Kangal-Pass) hebt Skips als `ProjectStateProposal(REJ-n, ingest_rejection, rejected)` mit Statement/Begründung/IdentityKey/planId in den Core (idempotent je planId+incomingId; fail-open ohne human-decisions.json = Experiment-Modi) · ② Resolver-Tool `search_rejections(query)` (leer = alle; Signatur = Austausch-Naht für späteren Index) + Prompt-Schritt 2b (`relatedRejectionId` als Hinweis, KEIN Auto-Skip) · ③ Gate-Warnung `UNKNOWN_REJECTION_REF` (fail-open wie featureKey) · ④ UI-Note „Schon einmal abgelehnt" (Datum+Grund) über Resolver-Hinweis ODER deterministischen IdentityKey-Exakt-Match. 8 Tests; 315/315 · Smoke 14/0. **Live-BEWIESEN (04.08., isolierter Sandbox-Lauf `runs/ingestion/20260804_091155_583e29`, gpt-5.4):** Resolver rief `search_rejections` autonom (Event `INGEST_TOOL_REJECTIONS`), erkannte die ANDERS formulierte Wiederholung semantisch („Nachrichten schreiben" ↔ abgelehnte „Chat-Funktion") und setzte `relatedRejectionId=REJ-001` — die fremde Kontroll-Anforderung (Wetter) blieb korrekt ohne Verweis; Gate Pass. Setup: Test-Ablehnung in Core-ARBEITSKOPIE injiziert, danach byte-identisch restauriert (cmp-verifiziert; Wahrheit unberührt). Bestätigt die L3-v7-Design-Lehre: als expliziter Prompt-Schritt (2b) instruiert wird das Tool genutzt — nicht nur angeboten. Nur Tor 1 (Leitregel: nur Wahrheits-Aussagen sind Projekt-Wissen). |
| R-34 | Kein Core-Integritäts-Check „aktives PBI deckt superseded Requirement ab" (hängende `covers`-Relation) | Code-Verifikation 03.08. (bei E0.7, Kettenfrage). SUPERSEDE im Ingest setzt alt→`superseded`, entfernt aber die alten `covers`-Relationen NICHT. | **Mechanik:** `IngestionApply.cs:92-103` (SUPERSEDE) fasst keine `covers`-Relation/PBI an. Reparatur nur downstream via `SUPERSEDE_PBI` (`PbiUpdateDerivation.cs:48-51`), und das Gate **warnt** nur (`SUPERSEDE_NO_REPLACEMENT`, `PbiUpdateGate.cs:50-53`), blockt nicht. **Kein** Core-Level-Validator prüft `covers`→superseded. Läuft pbi-update nicht (DryRun/MaxAttempts/Standalone-Ingest), bleibt eine hängende Relation — niemand fängt sie. | **Fix-Kandidat:** ein Core-Integritäts-Validator (thematisch **mit R-33-S4** zusammendenken — EIN geteilter Core-Integritäts-Check: keine hängenden Relationen, kein PBI ohne Feature, kein `covers`→superseded). | **ABGESICHERT (04.08.) via `CoreKangal` (R-33-S4):** Port-Validator warnt laut bei `covers`→superseded (I4) an JEDEM Save — die Lücke kann nicht mehr still bleiben. Voll-Fix (Swap-Erzwingung) bleibt Teil des Betriebs-Flusses (pbi-update). Details: `aufgefallen.md §2② R-34` |
| R-33 | re-clarify-Stufen ohne kanonischen Checker-Repair-Loop (weiche Coverage-Durchsetzung) + Feld-Mismatch RequirementIds/Traceability | Code-Verifikation 03.08. (aus E0.6). Ursprünglich als „Seeder-Verlinkung" notiert; nach Verifikation umgeschrieben — Ursache liegt höher. Der kanonische Maker-Checker-Repair (`GateLoop.cs`, genutzt von GithubForward/pbi-update/Decision/Requirement-Resolver) wird von den re-clarify-Stufen NICHT verwendet. | **Mechanik:** (1) `ReClarifyBacklogWorkflow`/`ClusterWorkflow` sind linear `agent→gate→finalize`, Gate = passiver Recorder, keine Repair-Kante. Coverage nur per Agent-Selbstcheck (`check_pbis`-Tool, prompt-getrieben); `save_pbis` (`ReClarifyBacklogTools.cs:78`) erzwingt Pass NICHT; `BacklogFinalizeExecutor` schreibt unabhängig von `Pass`; downstream nur CLI-Exit-1 + Human-Sicht, kein In-Graph-Block; Seed re-prüft nicht. (2) **„Feld-Mismatch" ENTSCHÄRFT (E-A geklärt 03.08.):** Gate prüft `RequirementIds`, Seed verlinkt über `Traceability` — aber `Traceability` ist deterministisch aus `RequirementIds` abgeleitet (`ReClarifyTraceabilityEnricher`, im geteilten Apply-Kern CLI+Graph; operativ explizit `PbiUpdateApply.cs:55`) → grünes Gate überträgt sich, KEIN Alignment nötig. Rest nur mechanisch: `ResolveFeatureId`==null / Canonical→Core-Mapping-Loch → orphan (optionaler Validator). (3) cross-cutting nicht als Relation geseedet (`:98,105`) → R-11. Betrifft Bootstrap-Seed UND operational `ApplyNewFeatures`. | **Fix = Option 1 (planen):** re-clarify (backlog+cluster) auf `GateLoop`-Checker-Repair-Form heben (Repair-Knoten im MAF-Graph) + Feld-Alignment (`RequirementIds`↔`Traceability`). NICHT `save_pbis` hart machen, NICHT Bolt-on-Post-Seed-Validator als Haupt-Fix (Validator = optionale Defense-in-Depth). cross-cutting → R-11. Plan: `done/2026-08-04/reclarify-checker-repair-plan.md`. | **GEFIXT (03./04.08., S0–S2+S4 gebaut):** beide Stufen auf kanonischer GateLoop-Form (Repairability an der Regel, typisiertes Decide, Repair-Executors mit wörtlichem GateFeedback, GateAttempt-Historie, geteilte AddTo-Kanten-Quelle CLI+Ein-Graph, `--max-attempts`, Cluster-Kritiker nur bei Pass); Loop-Mechanik am echten Graphen in-process bewiesen (LLM-freie Fake-Agents). S4 = `CoreKangal` am Port. **Live-LLM-Beleg ✅ ERBRACHT (04.08., 3 isolierte CLI-Läufe):** ① cluster `20260804_073649_5eac06` (gpt-5.4: 18 Cluster, Gate Pass Attempt 1, Review nach Pass) · ② clarify `20260804_073840_1df827` (gpt-5.4: 36 PBIs, Pass Attempt 1) · ③ clarify `20260804_074419_acab74` (**bewusstes Experiment mit gpt-4.1-mini**: Attempt 1 maker rot [70× UNCOVERED_CORE, nichts gespeichert] → `RE_CLARIFY_BACKLOG_REPAIR` feuert → Attempt 2 source=repair **Pass**, 19 PBIs — durch Tool-/Event-Log bewiesen; zugleich erster Live-Datenpunkt „GateFeedback hilft" für W2). Plan: `done/2026-08-04/reclarify-checker-repair-plan.md` |
| R-22 | Tor 3 REST-Client (Update-PATCH 422) | Run `152440`, Execute nach R-21-Fix: `failed=2, GitHub API 422 „oneOf: nil is not a string/object"` | `UpdateIssueAsync` sendete `"state": null` mit (Feld ohne WhenWritingNull) — GitHubs PATCH-Schema lehnt null ab. Zweiter Beleg, dass der Update-Pfad nie real lief (der Fehler hätte sonst längst gezündet). | `JsonIgnore(WhenWritingNull)` auf `state`. Build 0/0. | **behoben** — Beweis: E11-Execute |
| R-21 | Tor 3 UPDATE-Pfad war TOTER CODE (E11-Hauptfund!) | Run `20260723_152440_f1e0f6`: Plan enthielt korrekt 2× `UPDATE_ISSUE` auf #32/#33, Execute aber: `updated=0, alreadyApplied=2` — nichts geschrieben | Die Idempotenz-Wache `AlreadyApplied` kurzschloss Ziel-Ops, wenn das PBI bereits auf DASSELBE Issue gemappt ist — für LINK korrekt, für **UPDATE/COMMENT fatal falsch** (Mapping = Voraussetzung, nicht Anwendungs-Beweis) ⇒ kein Inhalts-Update konnte je durchkommen; nie zuvor bemerkt, weil der Update-Pfad in keinem E2E real lief. Vorstufe (Run `152036`): ohne frischen Snapshot nur `FLAG_DRIFT` — Betriebszyklus braucht `github-snapshot` (Read-Tor) vor dem Forward. | Guard differenziert (UPDATE/COMMENT ausgenommen, Kommentar erklärt warum; Re-Run-Doppel-Kommentar als Restrisiko dokumentiert). Runbook: Snapshot-Schritt in den Betriebs-Takt. Build 0/0 · 80/80. | **behoben** — Beweis: E11-Execute |
| R-20 | B6/pbi-update-Review (UI-Kontext-Armut) | Autor-Feedback E11-Lauf (`20260723_151741_f00984`): „die UI bringt auf der Ebene 0 — man versteht gar nichts, man weiß nicht was welches PBI ist" | Die Placement-Review zeigt Operationen (EXTEND/MARK/…) mit PBI-IDs, aber ohne PBI-Titel/-Inhalt, ohne Vorher/Nachher der Änderung und ohne das auslösende Requirement — informierte Entscheidung so nicht möglich, faktisch Blanko-Akzept. | Review-Adapter anreichern: PBI-Titel+Goal als Kontext-Block, betroffenes Requirement (Text!), Diff-Vorschau der Änderung; gehört zur R-7-UI-Welle (gleiche Hilfe-/Kontext-Tiefe wie Adjudikation). | **behoben (E0.3-A, 27.07.; code-verifiziert 30.07.):** pbi-update-Review zeigt PBI-Titel/Goal, Vorher/Nachher + auslösendes Requirement. |
| R-19 | Ingestion-Apply (Null-Robustheit) | Run `150814`, sichtbar erst nach Shell-Fix: `RequirementIngestionApply: Value cannot be null (Parameter 'second')` | `Union(t.SourceClaimIds, op.ClaimIds)` warf bei Delta-Items OHNE `sourceClaimIds` (schema-legal — synthetische/fremde Deltas tragen keine Claim-Provenance). B5 lief nur, weil Front-Deltas immer Claims haben. | `Union` null-tolerant (`?? []`). Retry: **APPLIED 2/2** (1 added, 1 superseded). | **behoben** (80/80) |
| R-16 | B7 Stale-Snapshot-Falle (vom Dry-Run gefangen!) | Run `20260723_142742_fce7b4`: Dry-Run meldete `wouldLink=22` bei LEEREM Ziel-Repo; Plan: `LINK PBI-001 → #40` — Issue-Nummern aus dem ALTEN Repo `armiino/Agentic-GitHub` (Snapshot `runs/github-snapshot/20260720_…`) | Der Forward lädt „latest run" aus `runs/github-snapshot` als Issue-Bestand — beim Archivieren (Log #37) bewusst im Live-Pfad belassen (Kontext fürs alte System), für den FRISCH-Start aber giftig: 22 Falsch-Mappings wären beim Execute als Core-Relationen geschrieben worden. **Der zweistufige Boden (Dry-Run vor Execute) hat exakt gewirkt.** | Snapshot-Altbestand → `runsArchive/github-snapshot/`; vergifteter Forward-Run gelöscht; B7 neu (ohne Snapshot ⇒ alles unmapped ⇒ 43 CREATEs erwartet). **TODO: Forward sollte Snapshot-Repo gegen `--repo` prüfen und fremde Snapshots ablehnen statt still matchen.** | **✅ GEFIXT (07.08., B1):** Repo-Guard `GithubSnapshotGuard` — fremde/ungestempelte Snapshots werden LAUT abgelehnt (`SNAPSHOT_REPO_MISMATCH`/`_UNSTAMPED`), beide Forward-Bahnen. Tabellen-Status nachgezogen 10.08. |
| R-15 | B7 Initial-Sync fehlt (Autor-Fund) | User vor B7: „wir haben doch noch gar nichts auf GitHub" — korrekt: Forward projiziert NUR das Sync-Delta des letzten Placement-Laufs (21 von 43 PBIs); die 22 unberührten Bootstrap-PBIs bekämen nie Issues, bis ein Meeting sie anfasst | Für ein frisches Repo fehlt der **Initial-Sync als Produkt-Schritt** (im Alt-System nie gebraucht — Repo wuchs mit). | Überbrückt per synthetischem Run `runs/pbi-update/_initial-sync-20260723` (Sync-Delta mit ALLEN 43 PBIs aus den Core-Payloads, Smoke-Fixture-Muster, kein Produktcode) — B7 fährt darüber. **TODO: echtes Kommando `github-initial-sync`** (deterministisch: Core-PBIs → Voll-Delta) für künftige frische Repos. | **Kern GEBAUT 26.07. (B5):** `GithubInitialSync.BuildDelta` (det., unmapped PBIs der github-sync-View → Voll-CREATE-Delta, exakt das pbi-update-Format) läuft als Forward-Variante des Bootstrap-Zweigs in pipeline-full (+2 Tests). CLI-Kommando weiterhin optional offen |
| R-14 | B6b Tor 2 (Decision-Brücke fehlt im Pfad) | `decision-resolve-hitl start --answer …` → „keine offenen Decisions im Core" (trotz E8: offene Frage als REQ-08-Refinement + PBI-003 `needs_clarify` vorhanden) | Tor 2 sucht Core-Items `itemType=decision, status=open_decision`. Die historische **l4-completion**-Familie hatte mit `ADD_OPEN_DECISION` bereits Decision-Minting-Logik im L4-Baseline-Kontext, hängt aber weder im Bootstrap- noch im Delta-Pfad des Runbooks und ist nicht 1:1 der heutige Core-`DEC-*`-Pfad. Offene Fragen fließen als `needs_clarify` durch, werden aber nie zu auflösbaren Decision-Entitäten ⇒ Tor 2 läuft im Standard-Pfad leer (E8 nur halb erfüllt). | Nach dem E2E entscheiden: Ingestion/Placement mintet Decisions direkt, wenn ein Claim `open_question\|must_clarify` ist; die alte l4-completion dient nur als Orientierung für Felder, Provenance und Review-Semantik. Bis dahin: Tor 2 bleibt smoke-gedeckt. **→ Herkunfts-/Adoptions-Analyse (26.07.): die Decision-Minting-Funktion stammt aus der abgelösten `l4-completion` — vollständige Genealogie + Adoptions-Wege in `backlog-genealogie.md §5`.** **UPDATE 01.08. (Run-Beleg, G4): der HEUTIGE `DEC-*`-Mint-Pfad FEUERT operativ** — CONTRADICT in der Ingestion mintet `DEC-001` `open_decision` (`origin=INGESTION_CONTRADICTION`) in 2 fullworkflow-Läufen (`20260725_142151`: Widerspruch REQ-32; `20260728_214408`: Widerspruch REQ-76). **Tor 2 lief in KEINEM** (grep leer) → die DECs parken unaufgelöst. Also NICHT „kein Minting-Pfad" (das galt für die alte l4-completion), sondern **Tor 2 nicht verdrahtet**. R-14-Kern = **G1** (Tor 2 operativ anschließen). **G1 ✅ GEBAUT + LIVE BELEGT (04.08., Run `20260804_121433_eb181a`):** decision-gate = 7. Human-Gate zwischen Ingest-Apply und Pbi-Bridge; kompletter Zyklus live: CONTRADICT→DEC-001→Pause am Gate→Auflösung per UI-Datei (ADOPT_NEW)→**REQ-78 supersedes REQ-32**+Flip+Swap→synthetische Op→3 MARK_CHANGED→Align-Agent live (3 Vorschläge)→Forward-Dry-Run→FERTIG; Kangal still; Core danach byte-identisch restauriert (Experiment). E0.8-UI (`decision-gate-review`, edit-fähig, E0-Endstand) ✅. defer-all-Politik für accept-all/replay/Flags ✅. **D2 ✅ + D4 ✅ (04.08.):** `to_decision`-Option am pbi-Gate (Begründung=Frage, `DecisionRequestMint` prägt DEC ohne contradicts-Kante + blockt PBI; Kreis test-bewiesen bis Unblock) · `pipeline-full status` zeigt IMMER Parkplatz + Kangal-Integritätszeile (live: 21 needs_clarify sichtbar). Details: `done/2026-08-07/r14-entscheidung.md §10`. | **✅ KOMPLETT (04.08.: G1+E0.8+Live-Beleg+D2+D4)** |
| R-13 | Front-Stufen (Kontext-Design) | Autor-Frage 23.07. („fehlt nicht Kontext in vielen Schritten?") | Design bestätigt: Front bewusst kontextFREI (Treue — Arm-A/B-Beweis 0/114 vs. 59/59; bounded prüfbar), Kontext konzentriert an den Toren (Tor 1 `SearchCore` = der Merge-Punkt). Echte Kosten: ① Autor re-adjudiziert Bekanntes ohne Hinweis ② meeting-lokale Urteile sehen Updates auf Bestehendes nicht (für Architektur wegen R-11 sogar Totalverlust) ③ Meeting-„Baselines" sind Delta-Material, kein Projekt-Artefakt (Namens-Schärfe). | Nach dem E2E: **Kontext-HINWEISE in der Adjudikations-UI** („ähnliches Core-Item: REQ-32") als reine Anzeige — Hinweis ja, Einfluss auf Extraktion nein; Doku-Schärfung Meeting-Baseline vs. Projekt-Artefakt; hängt zusammen mit R-11 (Tor 1 als VOLLSTÄNDIGER Kontext-Punkt). | offen |
| R-12 | A5 Bootstrap-Ansage (fehlender Schritt) | User-Fehlermeldung: `l4-re-clarify-backlog-review … Backlog-Lauf nicht gefunden` (Run `a96fda`) | Die vom Bootstrap gedruckte Kommando-Kette (und Runbook-A5) übersprang **`l4-re-clarify clarify <run>`** — den Schritt, der aus den akzeptierten Clustern die PBIs schneidet und damit den „Backlog-Lauf" (product-backlog.json + gate) ERST erzeugt. backlog-review kann ohne ihn nichts finden. Mechanik intakt, reine Ansage-/Doku-Lücke. | Bootstrap-Konsolen-Ansage um die clarify-Zeile ergänzt (CoreBootstrapRunner) + Runbook-A5 korrigiert. **Zusatz-Stolperfalle:** clarify erzeugt einen NEUEN Run (`80a11e`) — backlog-review/apply/seed laufen über DESSEN ID, nicht die Cluster-ID (2. Fehlversuch des Users; Runbook markiert). UX-Kandidat: backlog-Kommandos könnten die Cluster-ID auf den jüngsten Backlog-Lauf auflösen. | **behoben** (Build 0/0) |
| R-11 | Core/Architektur-Items (Rolle im Fokus-Pfad) | Autor-Frage 23.07.; Grep-Befunde: `core-baseline` exportiert NUR canonical-REQUIREMENTS-baseline; re-clarify: 0 Treffer „architecture"; PbiUpdate kennt requirement/feature/pbi; **Tor-1-SearchCore filtert `itemType==requirement`** | Architektur-Items sind im heutigen Fokus-Pfad (req→PBI→Issue) **Passagiere**: kein deterministischer Konsument NACH dem Bootstrap und **kein Update-Pfad im Betrieb** (bootstrap-frozen — spätere Meeting-Beschlüsse zur Technik erreichen den Core nicht!). Ihr belegter Wert: adjudizierte Wahrheits-Doku technischer Entscheidungen + Input der Risiko-Ableitung (req×arch) + B-Experiment-Generalität. | **⚠ GROSSES TODO (Autor-Entscheid 23.07.): Architektur-Rolle grundsätzlich adressieren und hinterfragen — „gerade ist der Sinn nicht wirklich gegeben."** Autor-Position: Architektur gehört GENAUSO ins PBI und in die Issues bzw. MUSS mit den Requirements zusammengehören (ein PBI trägt fachliche UND technische Seite). Zu klären nach dem E2E: Tor 1 auf architecture erweitern (Betriebs-Update-Pfad) · Backlog-Schnitt/PBI-Modell um Technik-Bezug erweitern (arch-Referenzen am PBI, arch-Kontext in Planung/Klärung) · Issue-Projektion inkl. Technik-Kontext · ODER begründet verwerfen. NICHT aus dem Core nehmen. | **✅ KOMPLETT (06.08.):** die ganze R-11-Architektur-Serie A1–A5 gebaut + E2E-belegt (arch = vollwertiger Core-Bürger, 10 Gates; Runs `145812`/`151204`). Tabellen-Status nachgezogen 10.08. |
| R-8 | Adjudikation/Taxonomie (Aussagekraft) | Autor-Frage 2026-07-23; Grep-Befund: `.Modality`/`.TimeScope` wird außerhalb 01-ledger NUR von `EvidenceLedgerProjection` + Maker-Checker (`ContractChecker` C3, Critic, Repair) gelesen; 04-delta/05-core/06-backlog: KEINE Facetten-Logik | Die Facetten-Taxonomie ist **feiner als ihre Konsumenten**: maschinell zählt v. a. die Grenze hart↔weich (C3 verbietet harte Artefakt-Sprache bei weicher Facette); Feinstufen innerhalb „weich" (desired vs. optional vs. must_note …) und `merge` vs. `mark_covered_by` sind reine Doku/Audit. Frühere Doku behauptete Backlog-Schnitt-Wirkung von timeScope — korrigiert (Hilfe + README präzisiert auf belegte Konsumenten). | Nach dem E2E entscheiden: Konsumenten nachziehen (z. B. timeScope → deterministischer MVP-Filter im Backlog-Schnitt, must_clarify → Klärungs-Queue) ODER Taxonomie vereinfachen. Bis dahin: Autor-Energie auf Aktion + hart↔weich + Referenz-Ziele konzentrieren. | **ENTSCHIEDEN 04.08. (Schritt 4 Teil B): Taxonomie BLEIBT unverändert — als W2-Messinstrument** (Gold-Labels/Recall-Matcher/C3-Ablation bleiben vergleichbar). Dokumentierte Wirkungs-Wahrheit (code-verifiziert): ① hart↔weich = die maschinell wirkende Grenze (C3/MC3-Drift-Wächter); ② `must_clarify`/`must_consider` wirken zusätzlich: Open-Questions-Prompt-Routing + eigene MC3-Treue-Regel (Klärungs-Sprache ist treu); ③ Rest (`must_note`/`desired`/`optional`) = Kontext fürs Consumable, keine typisierte Logik. **Wiedervorlage NACH W2** als Auswertungs-Brille auf denselben Messdaten (Fehler/Korrekturen innerhalb der weichen Gruppe vs. über die Grenze) — dann empirischer Entscheid „7 vs. ~4 Werte". UI-Vereinfachung geprüft und bewusst VERWORFEN (kein greifbarer Nutzen). **Beifang-Fix:** `scope` fiel als einzige Facette an der Projektions-Grenze weg (adjudiziert, aber nie einem Konsumenten gezeigt — die Audit-Notiz) → `EvidenceLedgerProjection` reicht sie jetzt mit (1 Zeile, VOR dem W2-Freeze). **Echter Befund aus der Diskussion:** offene Fragen (`must_clarify`-Terminus) haben KEIN Zuhause in der Wahrheit (Deltas empirisch nur requirement-Items, Core 0 open_question-Items, Parkplatz kennt sie nicht) → Design-Lücke, NICHT Taxonomie-Thema → **Parkplatz aufgefallen §2a-9g**. |
| R-5 | A2 Adjudikations-UI (Sprache) | Autor-Fund in der laufenden UI-Session (Run `20260723_090059`) | Englische Überschriften in der sonst deutschen UI: Feld-Labels aus dem Adjudikations-Adapter („Repair Status/Modality/TimeScope/Scope") + Shell-Kopf („Human Review", „Claim Ledger", inkl. JS-Titel-Fallback). | Labels eingedeutscht („Repair: Status/Modalität/Zeitbezug/Geltungsbereich"), Shell-Kopf → „Human-Review"/„Claim-Ledger" (4 Stellen). Feld-KEYS unverändert → laufende queue.json voll kompatibel. Build 0/0 · 80/80. **Merkposten:** Inhalts-Texte (Modell-Begründungen) können je nach Modell-Laune englisch ausfallen — das wäre ein Prompt-Thema, kein UI-Thema; bei Bedarf als eigener R-Eintrag. | **behoben** — UI neu starten |

### R-17 — Vertiefung (10.08., Autor-Frage „warum, und wie gelöst?") — ⏸ BEWUSST NICHT JETZT

**WARUM gibt es die Grenze?** Zwei Ursachen: **① LLM-Ausgabe pro Aufruf begrenzt (Physik):** „je Item eine
Op" heißt bei 50 Items ~50 strukturierte JSON-Ops in EINER Antwort — das sprengt das Ausgabe-Token-Budget,
das Modell hört mittendrin auf (~11–25 Ops). **② Repair-Loop sammelt nicht (hausgemacht):** der Reparatur-
Durchlauf ruft den Agenten neu und nimmt dessen Antwort KOMPLETT, statt die gedeckten Ops zu behalten und nur
die fehlenden nachzufordern → große Deltas konvergieren nie.

**VERALLGEMEINERUNG (wichtig, stand nirgends):** Das ist KEINE Forward-Eigenheit, sondern eine Eigenschaft
JEDER agentischen „ein-Op-je-Item"-Stufe (Tor 1 Ingest, Placement, Forward). Nur am Forward gemessen/geloggt
(43 PBIs). Ob Ingest/Placement bei ~50 sauber durchlaufen, ist NICHT bewiesen — gleiche Bauart, gleicher
Verdacht. **Aber immer fail-safe:** Gate + MaxAttempts blockieren laut (HumanReview), nie ein kaputter Write.

**DREI LÖSUNGEN (bei Bedarf):**
- **A — sofort, ohne Code:** Delta in Chunks < Grenze aufteilen, sequenziell fahren (Kosten: mehr Reviews).
- **B — der saubere Fix (mittel):** Repair-Loop von *ersetzt* → *sammelt* (gedeckte Ops behalten, nur Lücken
  nachfordern) ⇒ konvergiert automatisch, „just works". Scope: Repair-Executor im GithubForwardWorkflow + Test
  mit großem synthetischem Delta.
- **C — deterministischer Rückfall (Baustein existiert):** `GithubForwardDeterministicMatcher` (Keyword-Overlap
  → LINK/CREATE, gleiches Op-Schema, LLM-frei) als Fallback bei großen Batches.

**WANN tritt es auf?** Bootstrap-Bulk ist bereits deterministisch gelöst (`InitialSyncDeterministic`);
operative Meetings sind inkrementell (wenige PBIs). Der Rest-Fall = ein einzelnes Betriebs-Meeting mit ~50
BRANDNEUEN PBIs (bootstrap-groß, untypisch). **⚖ Autor-Entscheid 10.08.: brauchen wir jetzt NICHT — später
abwägen (B oder C), wenn ein realer großer Betriebs-Batch auftaucht. Bis dahin: Workaround A + fail-safe-Block.**

---

# ⏳ OFFENE LIVETESTS VOR W2 (Stand 23.08.2026 — VOR der W2-Vorbereitung abfahren, hier abhaken)

> Je Test: Zweck · Ablauf-Beispiel · Erwartung. Funde → R-Eintrag wie immer. STATUS ▶ zeigt hierher.
> Alle Tests sind billig (0–1 kleiner LLM-Lauf); Steward vorher FRISCH starten (neuer Build + Prompt).

- [ ] **L1 · Forward-UI + Relevanz + Doc-Diff** (Bau 23.08., R-64-Ausbau — ungetestet).
  *Ablauf:* Irgendeinen Lauf bis zum github-forward-gate bringen (am billigsten: „Setze die Prio von
  PBI-005 auf hoch" → propose_pbi_fields → Gate-Durchgang → Abgleich; oder direkt die Scoping-Runde L4
  nutzen). Am pausierten forward-gate: erst `get_paused_gate` im Chat ansehen, DANN `open_gate_ui` wählen.
  *Erwartung:* ① Chat listet NUR Änderungs-Ops + eine Summenzeile („dazu N× geprüft, nichts zu tun") —
  nie wieder 44 Zeilen ② die UI zeigt dieselben Änderungs-Ops mit Vorher/Nachher je Issue und bei
  Doc-Ops die „Doc-Diff"-Note ③ „Fertig" in der UI kettet den resume automatisch; GitHub-Write nur bei
  execute=true. *Nuance Erstlauf:* der Doc-Spiegel füllt sich erst BEIM Apply — beim allerersten
  UPSERT_FILE sagt die Diff-Note ehrlich „kein lokaler Spiegel (Vor-Spiegel-Ära)"; ab dem ZWEITEN
  Abgleich desselben Docs kommt der echte Sektions-Diff („Version 5 → 6 · geändert: …").
- [ ] **L2 · MEETING_RISK-Bahn** (einzige nie gefahrene Risiko-Herkunft; Autor-⚿, EIN kleiner LLM-Lauf).
  *Ablauf:* Mini-Transkript (Claude schreibt es) mit 1–2 klaren Risiko-Sätzen („Es besteht das Risiko,
  dass die Einrichtungen die Tablets nicht rechtzeitig beschaffen…") → Tür 1 mit Bestellzettel inkl.
  risks → Kette durchfahren. *Erwartung:* Risiko erscheint an Tor 1 (Frage-Vokabular) → nach apply
  eine **DEC mit Herkunft MEETING_RISK** → decision-gate bietet die Risiko-Behandlung (resolve mit
  akzeptiert/mitigiert-Begründung | defer). *Danach fällig (⚖):* `GITHUB_RISK`-Konstante — weiterhin
  ohne Besteller ⇒ tilgen ODER Ernte-Vokabular erweitern.
- [ ] **L3 · Glossar-Erstrunde** (Testplan-A4 wurde nie ausgeführt — docs/glossar.md fehlt).
  *Ablauf:* „Erstell mir ein Glossar" → draft (⚿) → feilen → ⚿-Save → nächster Abgleich publiziert.
  *Erwartung:* Begriffe Core-belegt, Mehrdeutiges als Klärungs-Kandidat markiert; UPSERT_FILE
  docs/glossar.md im nächsten Forward-Plan. (Alternative: Art bewusst als „ungenutzt" deklarieren —
  dann diesen Punkt streichen + in leitfaden-abdeckung vermerken.)
- [ ] **L4 · Scoping-Runde** (Betrieb — füllt Story Map + Backlog; liefert Thesis-Screenshots für Kap. 6.2).
  *Ablauf:* Im Steward die Prios entlang des MVP-Vorschlags der Story Map diktieren (Batch: „alle PBIs
  der MVP-Schritte hoch, Besuchskoordination/Angehörigen-Ansichten niedrig, Rest mittel") →
  propose_pbi_fields-Pendings → EIN pbi-Gate-Durchgang → Abgleich. *Erwartung:* backlog.md-Prio-Spalte
  + Story-Map-Tafel-Prio füllen sich automatisch; Issues bekommen prio-Labels; danach wirkt die Map
  „bewirtschaftet" statt halb leer. Kombinierbar mit L1 (der Abgleich am Ende IST der Forward-Gate-Test).
- [ ] **L5 (optional) · Story-Map-Einordnungs-Runde:** „Ordne die 9 wartenden PBIs ein" (ggf. neuer
  Schritt „Projektrahmen" für PO-/Meta-Items). *Erwartung:* Sammelbecken leert sich, Lage-Frische-Zeile
  verstummt; ⚠-Klärungs-PBIs dürfen eingeordnet bleiben (Marker bleibt sichtbar).

# Phase A — Bootstrap: Interview-Einrichtung → Core + Backlog

## A0) Vorbedingungen (erledigt am 2026-07-23)

- Alter Core geparkt: `runs/_e2e-backup/state-core-alt/` (Rückweg: `mv` zurück nach `state/core`).
- `.env`: `OPENROUTER_API_KEY` + `GITHUB_AGENTIC_REFACTOR_TOKEN` (fine-grained, nur Issues im neuen Repo).
- Checks: `dotnet build AgenticSdlc.Host/AgenticSdlc.Host.csproj --no-restore` und `dotnet test` (76/76).

## A1) Ledger  **[LLM]**

```bash
dotnet run --project AgenticSdlc.Host -- ledger-build-units input/transcripts/Interview-Einrichtung.txt
```

**Checkpunkt ZUERST:** die Startzeile muss `model=openai/gpt-5.4` zeigen (die Ledger-Runner ziehen
`jury.judgeModel` aus run-config — seit 2026-07-23 auf 5.4; bei Abweichung abbrechen statt weiterfahren).
Dann: `runs/ledger/<RID-A1>/` mit step-00…03; Konsole `candidate=N -> canonical=M -> validated=M`;
`gate/ledger-quality.json` **pass** (Violation-Codes = die aus `LedgerQualityGateTests`; bei error NICHT weiterfahren).

## A2) Adjudikation  **[DU]**

```bash
# Queue bauen — MIT Miss-Signal als 2. Argument (R-2!), sonst fehlen missing_claim/needs_human in der Queue:
dotnet run --project AgenticSdlc.Host -- ledger-adjudicate runs/ledger/<RID-A1>/step-03-facet-validation/output.json runs/ledger/<RID-A1>/step-01d-unused-unit-ledger-compare/output.json
# UI MIT validated Ledger als 2. Argument (R-9!) — dann wendet "Fertig" automatisch an (consumable entsteht direkt):
dotnet run --project AgenticSdlc.Host -- ledger-adjudicate-ui runs/ledger/<RID-A1>/step-03b-adjudicated/queue.json runs/ledger/<RID-A1>/step-03-facet-validation/output.json
# (Falls ohne 2. Argument gestartet: Entscheidungen sind gespeichert, apply separat:
#  ledger-adjudicate-apply <queue.json> <step-03-facet-validation/output.json>)
# nur falls "pending>0" gemeldet wird (neu gemintete Claims) — Refine [LLM]:
dotnet run --project AgenticSdlc.Host -- ledger-adjudicate-refine runs/ledger/<RID-A1>/step-03b-adjudicated/consumable.json input/transcripts/Interview-Einrichtung.txt
```

**Checkpunkt:** `step-03b-adjudicated/consumable.json`, Konsole `consumable claims=N (pending=0)`.

## A3) Baselines (Arm B)  **[LLM]**

`run-config.json` → `evidenceAgent.ledgerRun` auf **deinen** consumable umstellen (transcript stimmt schon):

```jsonc
"ledgerRun": "runs/ledger/<RID-A1>/step-03b-adjudicated/consumable.json"
```

```bash
dotnet run --project AgenticSdlc.Host -- recipe AgenticSdlc.Host/FullWorkflow/02-baselines/recipe/examples/build-req-arch-then-risks-and-gap.recipe.json
```

**Checkpunkt:** `runs/recipe/<RID-A3>/baselines/{requirements,architecture}/artifact.json` mit Items > 0.

## A3.5) 03-gap: Open-World-Lücken (l3) — Autor-Entscheid 23.07.: eingeschoben  **[LLM + DU]**

*(Parität zum alten Core: der wurde aus `l3_v2` geseedet — der frische soll nicht dünner starten.
Default-Modus ohne Experiment-Flags = der historisch belegte Pfad, `mode: generate`.)*

```bash
# 1) Kandidaten generieren → verankern → routen (4 Klassen) → Review-Paket:
dotnet run --project AgenticSdlc.Host -- l3 \
  runs/recipe/<RID-A3>/baselines/requirements/artifact.json \
  runs/recipe/<RID-A3>/baselines/architecture/artifact.json
# 2) DEINE Review-Session (Promotions entscheiden):
dotnet run --project AgenticSdlc.Host -- l3-review <RID-A3.5> --interactive
# 3) Entscheidungen anwenden (promoted items entstehen):
dotnet run --project AgenticSdlc.Host -- l3-apply <RID-A3.5>
```

**Checkpunkt:** `runs/l3/<RID-A3.5>/` mit Review-Paket; nach apply: promoted items > 0 (nur die von DIR
akzeptierten); Sprache deutsch (03-Prompts sind NICHT R-6-gehärtet — beobachten!).

## A4) Core anlegen (deterministisch, orchestriert) — MIT den l3-Promotions

```bash
dotnet run --project AgenticSdlc.Host -- core-bootstrap-first-transcript \
  runs/recipe/<RID-A3>/baselines/requirements/artifact.json \
  runs/recipe/<RID-A3>/baselines/architecture/artifact.json \
  --l3-run <RID-A3.5> \
  --project-id einrichtung-fresh
```

Läuft intern `project-state-build → core-seed → core-baseline` und bricht ab, falls doch ein Core existiert.
**Checkpunkt:** `[core-bootstrap] OK — initialer Core angelegt: items=N`; `state/core/project-state.json`
existiert. Ab jetzt schreibt jeder inhaltsändernde Save automatisch Snapshots nach `state/core/history/`.

## A5) Backlog-Schnitt + Seed  **[LLM + DU]**

Die Kommandos sagt der Bootstrap am Ende selbst an (Baseline-Pfad aus seiner Ausgabe übernehmen):

```bash
dotnet run --project AgenticSdlc.Host -- l4-re-clarify cluster <canonical-requirements-baseline.json aus A4-Ausgabe>
dotnet run --project AgenticSdlc.Host -- l4-re-clarify-review <RID-A5> --interactive     # [DU]
dotnet run --project AgenticSdlc.Host -- l4-re-clarify-apply <RID-A5>
dotnet run --project AgenticSdlc.Host -- l4-re-clarify clarify <RID-A5>     # Cluster -> PBIs: erzeugt EINEN NEUEN Run <RID-A5b> (R-12!) [LLM]
# ⚠ ab hier gilt die NEUE RunId aus der clarify-Ausgabe (<RID-A5b>), nicht mehr die Cluster-ID:
dotnet run --project AgenticSdlc.Host -- l4-re-clarify-backlog-review <RID-A5b> --interactive   # [DU]
dotnet run --project AgenticSdlc.Host -- l4-re-clarify-backlog-apply <RID-A5b>
dotnet run --project AgenticSdlc.Host -- core-seed-backlog <RID-A5b>
```

**Checkpunkt:** `product-backlog.json` im Run; `core-seed-backlog` meldet die in den Core gehobenen PBIs;
Core-Item-Zahl notieren: `python3 -c "import json;print(len(json.load(open('state/core/project-state.json'))['items']))"`.
**Phase A fertig = das Projekt hat eine frische Wahrheit + Backlog. (Ab jetzt ist auch der Smoke wieder fahrbar.)**

---

# Phase B — Erstes Betriebs-Meeting: meeting-2-extended.txt (diesmal MIT Ledger) → Issues

**Test-Design (23.07.):** `input/transcripts/meeting-2-extended.txt` wurde GEZIELT gebaut (ersetzt das
5-Zeilen-meeting-2-delta) — jeder Gesprächsblock adressiert einen Kettenmechanismus und dockt an
EXISTIERENDE Core-Items an. Die Erwartungen stehen VORAB fest; nach jedem Schritt Soll/Ist abgleichen:

## Erwartungs-Checkliste (vorab festgelegt — beim Fahren abhaken)

| # | Eingebauter Test | Erwartung | Ist |
|---|---|---|---|
| E1 | Smalltalk (Kaffee, Terminfindung, „muss zur Übergabe") | B1: Triage → noise, erreicht NIE die Queue | |
| E2 | Vage Aussage („Piktogramme größer denken") | B1/B2: needs_human → landet in DEINER Queue | |
| E3 | Lob ohne Neuigkeit (About-Me „nichts ändern") | B1: already_covered/attach MIT Referenz (R-1-Netz); B5: KEINE Core-Op | |
| E4 | Updates auf Bestehendes: No-Go Stopp-Symbol + bearbeiten/löschen (REQ-32) · Sofortinfo max 5 + Pflichteintrag (**L3-REQ-002!**) · Offline-Konfliktanzeige (**L3-REQ-004!**) | B5: UPDATE/Erweiterung per Identity-Match statt Duplikat — insbesondere Beweis, dass **L3-Promotions „leben"** | |
| E5 | Neue Themen: Schichtübergabe (Notiz+prominent, bewohnerbezogen verlinkbar, 7-Tage-Archiv), Profil-Suche, No-Go-Vorlagenliste | B1: missing/new Claims → B5: NEW → B6: **NEW_PBI** → B7: CREATE-Issues | |
| E6 | Externe Auflage (Heimaufsicht: Zugriffsprotokoll, „nicht verhandelbar") | B1: `status=required` + `modality=must` (Rasierklinge!, sonst Gate-Error); durchgängig als harte Pflicht | |
| E7 | In-Meeting ENTSCHIEDENER Konflikt (No-Gos global? → Nein, pro Bewohner + Vorlagen-Kompromiss) | B1: als decided erfasst, KEIN Widerspruchs-Chaos; ggf. Contradicted-Handling in Adjudikation | |
| E8 | Explizit OFFENE Entscheidung (Angehörigen-Schreibrechte → Datenschutzklärung) | B1: `status=open`/`must_clarify` → B5/B6: PBI **blocked/needs_clarify** → **B6b: Tor 2 löst auf (decision-resolve)** | |
| E9 | Weiche Wünsche (Dunkelmodus, Schriftgröße — „nicht kriegsentscheidend") | B1: `desired`/`optional` → B3: C3-Checker verhindert „muss"-Formulierung im Artefakt | |
| E10 | **Technik-Beschlüsse: „Firestore ENDGÜLTIG" (Widerspruch zu ARCH-39 „vorläufig"!) + „Android 10 Minimum"** | **R-11-LIVE-BEWEIS: gehen an Tor 1 VERLOREN** (filtert itemType=requirement) — der frische Core behält „vorläufig". Erwarteter, dokumentierter Verlust = Beleg fürs GROSSE TODO | Ist: SCHÄRFER — via req-Route kam „festgelegt" in REQ-70 an, ARCH-39/REQ-55 sagen „vorläufig" ⇒ Wahrheit gespalten |
| E11 | **OFFEN (Reihenfolge-Artefakt):** GitHub-**Update**-Pfad (gemapptes PBI → UPDATE/COMMENT statt CREATE) | Nie real bewiesen — Initial-Sync lief NACH der Meeting-Einarbeitung, daher nur CREATEs sichtbar; Mappings (39×) liegen bereit | **✅ BEWIESEN 23.07. (Mini-Meeting-3, ohne Front):** synth. Delta (2 Items) → Tor 1 `APPLIED 2/2` (1 NEW→REQ-77, 1 SUPERSEDE REQ-63→REQ-76; nach R-19-Fix) → Placement: `updatedPbis=[PBI-032, PBI-033]` (0 neu — PDF-Export als Erweiterung einsortiert) → `github-snapshot` (39 Issues) → Forward: 2× `UPDATE_ISSUE` → **Execute `updated=2, failed=0`**, Issues #32/#33 nachweislich geändert. Unterwegs 3 echte Bugs gefunden+gefixt (R-19 Null-Apply, R-21 toter Update-Pfad, R-22 422 state:null) + 2 Qualitätsfunde (R-20 UI-Kontext, R-23 Update-Body-Stub). Ohne Snapshot nur FLAG_DRIFT (Betriebs-Takt: snapshot VOR forward!) |

## B1–B2) Ledger + Adjudikation  **[LLM + DU]**

Wie A1/A2, nur mit `input/transcripts/meeting-2-extended.txt` → RID-B1, eigener consumable
(Queue MIT Miss-Signal, UI MIT validated-Ledger — R-2/R-9!).

## B3) Baselines  **[LLM]**

`run-config.json` ZWEIMAL anfassen: `evidenceAgent.transcript` → `input/transcripts/meeting-2-extended.txt`
und `evidenceAgent.ledgerRun` → `runs/ledger/<RID-B1>/step-03b-adjudicated/consumable.json`. Dann `recipe` wie A3 → RID-B3.
**Checkpunkt zusätzlich (E9):** Dunkelmodus/Schriftgröße dürfen im Artefakt NICHT als „muss" stehen.

## B4) MeetingDelta (deterministisch)

```bash
dotnet run --project AgenticSdlc.Host -- project-state-build \
  runs/recipe/<RID-B3>/baselines/requirements/artifact.json \
  --out runs/project-state/e2e-meeting2/meeting-delta.json --project-id einrichtung-fresh
```

**Checkpunkt:** `meeting-delta.json` mit items > 0.

## B5) Tor 1 (ingestion)  **[LLM + DU]**

```bash
dotnet run --project AgenticSdlc.Host -- ingest-requirements-hitl start runs/project-state/e2e-meeting2/meeting-delta.json
dotnet run --project AgenticSdlc.Host -- ingest-requirements-hitl resume <RID-B5> --ui
```

Bitte `--ui` bzw. `--accept ID,…` — die Beweis-Aussage ist „jede Mutation autorisiert", nicht „durchgewunken".
**Checkpunkt:** `APPLIED`; Core-Item-Zahl vorher→nachher; History-Snapshot in `state/core/history/` entstanden.

## B6) Tor Placement (pbi-update)  **[LLM + DU]**

```bash
dotnet run --project AgenticSdlc.Host -- pbi-update-hitl start <RID-B5>
dotnet run --project AgenticSdlc.Host -- pbi-update-hitl resume <RID-B6> --ui
```

**Checkpunkt:** `APPLIED`; `runs/pbi-update/<RID-B6>/plan/applied/github-sync-delta.json` mit new/updated PBIs.

## B6b) Tor 2 (decision) — löst die eingebaute offene Entscheidung E8 auf  **[LLM + DU]**

Das Transkript enthält ABSICHTLICH eine offene Entscheidung (Angehörigen-Schreibrechte →
Datenschutzklärung). Nachdem sie als blocked/needs_clarify im Backlog gelandet ist, spielst du die
eingetroffene Stakeholder-Antwort ein — damit testet der E2E auch **Tor 2** real (bisher nur smoke-gedeckt):

```bash
dotnet run --project AgenticSdlc.Host -- decision-resolve-hitl start --answer "Die Datenschutzbeauftragte hat entschieden: Angehörige dürfen Inhalte auf der About-Me-Seite vorschlagen; jeder Vorschlag muss von einer Pflegekraft freigegeben werden, bevor er sichtbar wird. Direktes Schreiben ohne Freigabe ist nicht erlaubt."
dotnet run --project AgenticSdlc.Host -- decision-resolve-hitl resume <RID-B6b> --ui
```

**Checkpunkt:** Entscheidung aufgelöst, betroffenes PBI entblockt/aktualisiert; neuer Core-History-Snapshot.

## B7) Tor 3 (github) — Issues im NEUEN Repo  **[LLM + DU, zweistufiger Boden]**

```bash
# a) Plan + DEINE Op-Review + DRY-RUN (kein Write, kein Token noetig):
dotnet run --project AgenticSdlc.Host -- github-forward-hitl start <RID-B6> --repo armiino/Agentic-GitHub-refactor
dotnet run --project AgenticSdlc.Host -- github-forward-hitl resume <RID-B7> --ui
# -> "DRY-RUN": plan/applied/github-forward-apply-report.json LESEN (Titel/Bodies/Labels plausibel?)

# b) ERST NACH SICHTUNG — der echte externe Write (irreversibel, NUR deine akzeptierten Ops):
dotnet run --project AgenticSdlc.Host -- github-forward-apply <RID-B7> --execute \
  --repo armiino/Agentic-GitHub-refactor --token-env GITHUB_AGENTIC_REFACTOR_TOKEN
```

**Checkpunkt:** Apply-Report `dryRun=false`, Issue-URLs; Gegenprobe im Browser: Issues in
`github-agentic-refactor` tragen die PBI-Inhalte. Das Repo ist frisch → unmapped PBIs werden als CREATE geplant.
**Hinweis zur Philosophie:** Es entstehen Issues für die vom Meeting-Delta **berührten** PBIs — der
Bootstrap-Backlog selbst wird nicht pauschal nach GitHub gespiegelt (GitHub ist Projektion der Änderungen,
nie Quelle). Wenn du ALLE Bootstrap-PBIs als Issues willst, ist das ein eigener (kleiner) Produkt-Schritt — als Reibungs-Befund notieren.

## B8) Abschluss

1. Beleg-/Reibungs-Tabelle vollständig ausfüllen (RunIds + Zahlen + Befunde).
2. Je Stufe trennen: **durch Tool-Log bewiesen** vs. **vom Workflow behauptet** (`logs/events.jsonl`, `decision-log.jsonl`).
3. Der frische Core IST jetzt die Wahrheit — geparkten Alt-Core (`runs/_e2e-backup/state-core-alt/`) als Vergleichs-/Thesis-Material behalten.
4. Iteration-Note schreiben (frischer E2E: Datum, Transkripte, alle RIDs, Reibungs-Liste) — Notes sind Primär-Evidenz.

---

## Bekannte Stolpersteine

- **Kein Core bis A4** → Smoke und alle Tore verweigern sauber („Core fehlt") — erwartet.
- Alte pausierte HITL-Checkpoints (vor R6) sind nicht resumebar — immer frisch `start`en.
- `recipe` Exit 4, wenn `evidenceAgent.source` ≠ `ledger`; Exit 2, wenn `ledgerRun` nicht auf eine existierende consumable.json zeigt (nach A3/B3-Umstellung prüfen).
- `github-forward-hitl start` verlangt einen pbi-update-Run **mit** `plan/applied/github-sync-delta.json`.
- 0 Ops ist kein Fehler (Delta zu nah am Bestand) — als Befund notieren statt erzwingen.
- Der gepostete PAT ist ein Chat-/Terminal-Secret gewesen: nach Abschluss der Thesis-Läufe rotieren.

## R-40 — arch-Ops verloren am decision-gate-Roundtrip (06.08., Run `20260806_145812_c29ed2`) — ✅ GEFIXT
Befund: pbi-update `deterministic:0/unplaced:0` trotz 45 klassifizierter archs — die Decision-Stufe lädt den
Ingest-Report per Datei-Vertrag (`applied/delta.json`, req-only); die ③-Report-Vereinigung reiste nur in der
Message → E-8-Weckruf und A4-Placement bekamen die arch-Auslöser nie. Mechanik-Thema (Naht), kein Modell.
Fix: `ArchComposedApply` persistiert `applied/delta-merged.json`; Decision-Stufe liest merged-vor-req
(Fallback = Alt-Verhalten; delta.json bleibt req-Beleg). **ENDFORM statt Vorrang-Magie (Autor-Einwand):
EIN Lauf-Report-Vertrag `applied/run-report.json`** — req-Apply schreibt initial, arch-Apply schreibt FORT,
Decision liest nur diese Datei (delta.json = per-Aspekt-Beleg; Fallback nur Alt-Lauf-Kompat). 426 Tests.
**✅ LIVE BEWIESEN (Run `20260806_151204_3c19d2`):** PBI_UPDATE_DERIVE deterministic:3/unplaced:5 (statt 0/0)
— E-8-Weckruf real (REFINE ARCH-01 → MARK_CHANGED PBI-001/025/030 via constrained_by) + A4-Placement real
(5 work-archs → Placement-Agent → EXTEND_PBI, **5 covers→ARCH im Core**, Kangal grün) · ADR 17 Dateien ·
7 Forward-Ops · PIPELINE FERTIG · Core-Restore byte-identisch.

## R-41 — Lese-Tool-Suche: Umlaut-Varianz + geratene Such-Syntax (07.08., Steward-Proben `164105`/`164358`) — ✅ GEFIXT

**Befund (2 Teile, C3-Live-Proben):** (a) Modell-Query „Schriftgroesse" traf den Issue-Titel „Schriftgröße"
NICHT (wörtliches Substring-Match); (b) das Modell riet Suchmaschinen-Syntax (`"A OR B OR C"` als EIN
query-String) → 0 Treffer. **Fix an der Quelle:** (a) geteilte Match-Naht `shared/QueryText.cs` — beidseitige
Umlaut-Faltung (ä→ae/ö→oe/ü→ue/ß→ss), von CoreQueryTools UND GithubSnapshotQueryTools genutzt; (b) Tool-VERTRAG
(Description) explizit: „genau EIN wörtlicher Suchbegriff, KEINE Operatoren, für Alternativen mehrfach aufrufen"
— Vertrag statt Prompt-Hoffnung. **Wirkung live:** Probe `164358` (OR-Query, 0 Treffer) → nach Fix Probe
`164735ff` (Einzelbegriff „Schriftgroesse") findet `#39` „Schriftgröße" + Body-Treffer. Test-Pinning:
GithubSnapshotQueryToolsTests (ASCII-Query ↔ Umlaut-Titel).

## R-42 — Vergessene accept-all-Experiment-Policies → ungewollter Core-Write bei der C2c-End-Probe (08.08., Run `20260808_111048_d5ecd0`) — ✅ AUFGERÄUMT + GEHÄRTET

**Befund:** Die --from-github-End-Probe sollte am ingest-gate PAUSIEREN — stattdessen lief sie DURCH und
schrieb REQ-78+PBI-044 (Fixture-Inhalt!) in den echten Core (194→196). Ursache KEIN Code-Bug: run-config.json
trug ALLE 9 Gates auf `accept-all` (Experiment-Reste früherer E2E-Läufe); `GATE_ANSWERED policy=AcceptAll`
in den Events. Prozess-Fehler doppelt: Probe ohne Gate-Policy-Check UND ohne Core-Backup gefahren.
**Aufräumung:** Core via git restauriert (194 Items, byte-identisch zum Commit-Stand; History-Snapshot des
Vorfalls bleibt als lokaler Beleg). **Härtung:** `pipeline-full run` druckt jetzt LAUT ein
⚠ EXPERIMENT-MODUS-Banner, wenn irgendein Gate nicht interactive ist (Quelle+Wirkung benannt) — vergessene
Experiment-Config kann nie mehr still feuern. **Ehrlicher Neben-Gewinn:** der Lauf IST der End-Beweis des
vollen --from-github-Kreises (Ernte→Draft→Delta→Tor 1→Apply→PBI-Drafting→Forward-dryRun→FERTIG, Kangal grün,
GH-900→REQ-78→PBI-044 mit Ankern) — nur eben im deklarierten Experiment-Modus statt interactive.

## R-43 — Review-UI-Accept ohne Apply = stiller Nicht-Effekt (09.08., Sweep `ba73ae`) — ✅ AUFGELÖST + DESIGN BESTÄTIGT

**Befund:** Autor bediente das pbi-update-Review (decisions korrekt: apply+accept, reviewer human) und hielt
die Arbeit für erledigt — der SEPARATE `pbi-update-apply`-Schritt lief nie ⇒ Core unverändert, ohne jedes
Signal. **Auflösung:** aufgezeichneten Entscheid deterministisch ausgeführt (PBI-030 needs_clarify→active,
v3, Flutter-AK; 21→20; Kangal grün; erster voller C4-Kreis LIVE). **Design-Bestätigung:** genau dafür ist
die C4d-Registry gebaut — künftige Sweeps bleiben OPEN sichtbar (status/Steward), bis der Apply schließt;
ba73ae war vor-Registry. **Anti-Zumüll-Regel dazu (Autor):** Close ENTFERNT erledigte Pendings (Beleg-Kette
= Run-applied + Item-History + Core-Snapshots); R-35-Rejections bleiben bewusst (Wissen ≠ Buchhaltung).
**R-43-ENDFORM (⚖ Autor 09.08., BESTELLT — nächster Bau-Schritt):** Standalone-Review-„Fertig" kettet den
Apply AUTOMATISCH (Entscheidungs-Datei wird weiter ZUERST geschrieben = Replay/W2 unberührt; `--no-apply`
als Inspektions-Opt-out). Begründung: Governance = EIN menschliches Urteil AM GATE; der Apply ist
deterministische Ausführung — der manuelle Zweitbefehl war Relikt (Ein-Graph macht es längst so) und
reines Vergess-Risiko (2× live getroffen). Steward sieht Erledigung automatisch (liest Core frisch:
Registry zu, History-Note, Parkplatz 0).

## R-44 — Tor 1 war beim resume nicht per-Item entscheidbar (09.08., Vertrags-Lesen C5-Schritt2) — ✅ GEFIXT

**Befund:** der zentrale resume-Responder kannte für ingest-/arch-ingest-gate KEINEN Datei-Vertrag — nur
Konsolen-Flags ('a' = accept-all). Per-Item-Entscheide am häufigsten Gate waren nach durabler Pause
unmöglich (nur inline-UI im laufenden Prozess). **Fix:** neuer Datei-Vertrag `ingest-gate-decisions.json`
(IngestGateDecisionsFile: incomingItemId, apply|reject+reason; Items ohne apply-Eintrag werden NICHT
übernommen — nichts rutscht still durch); Responder liest die Datei VOR den Flags (source im
GATE_ANSWERED-Event); arch-ingest erbt via Stage-Mapping. Schreiber: Steward-Chat (C5-Schritt2) — die
Review-UI kann denselben Vertrag später nutzen. Tests: Loader+Chat-Roundtrip. **R-44b (09.08.):** dieselbe
Lücke am pbi-gate → Responder liest 07-pbi-update/human-decisions.json (bestehender UI-Vertrag!) vor Flags;
pbi-Review-UI auf Pipeline-Stufe = resume statt Standalone-Apply (Doppel-Apply-Schutz).

## R-45 — git-Restore des Cores räumt den Pause-Zeiger NICHT mit (10.08., Steward-Lauf-Analyse) — ⚠ HYGIENE

**Befund (Beleg-Lauf `20260806_115016_01715f`):** ein E-R4-Cross-CONTRADICT-Test (`--from-delta
crossreq-delta.json`) pausierte am decision-gate (DEC-001: REQ-70 Firebase ↔ „nur lokale Speicherung").
Der ingest-Apply schrieb DEC-001 in den Core; danach wurde die Core-Mutation per git-Restore zurückgerollt
(Core zurück auf 194, REQ-70 aktiv, kein crossreq-DEC — verifiziert). **ABER** der Pause-Zeiger
`checkpoints/pointer.json` blieb liegen ⇒ `pipeline-full status` meldete ~4 Tage eine PHANTOM-Pause; ein
`resume` wäre inkonsistent (DEC-001 existiert im Core nicht mehr). **Mechanik-Lücke:** Pause-Zeiger (Run-
Ordner) und Core-Wahrheit (state/core, git) sind entkoppelt — Core-Restore ohne Zeiger-Räumung = Geister-
Pause. **Sofort-Fix:** Zeiger neutralisiert (`pointer.json` → `pointer.json.orphaned-20260810`, reversibel;
Run bleibt als Test-Beleg, `CLEARED.md` erklärt); `status` wieder sauber (0 Pausen, Kangal 0/0). **Prozess-
Lehre (R-42-Cousin):** nach einem Experiment mit Core-Restore IMMER `pipeline-full status` prüfen und ggf.
den Zeiger räumen. **Nachprüfbar rein aus Artefakten:** config.json (Start) · events.jsonl (Fluss) ·
pointer.json (Pause) · 07-decision/decision-gate-request.json (Wartegrund) · 07-ingest/applied/delta.json
(Core-Write) · Gegenprobe state/core. Offen (Ausblick, kein Nah-Ziel): optionaler `pipeline-full
cancel <runId>`/Auto-Verwaisungs-Check beim Restore — bis dahin manuell.

## R-50 — Human-Gates pausieren auch bei 0 Operationen (Leer-Gate-Pausen, 17.08., Testplan Block G7) — ✅ GEFIXT (MAF-nativ, Typ-Routing)

> **✅ GEBAUT 17.08.:** Regel „Ein Human-Gate ruft nur, wenn es etwas zu entscheiden gibt — ein Skip ist immer
> LAUT." **Form = TYP-ROUTING** (Entry-/Branch-Muster): die Finalizes senden bei 0 Ops einen MARKER
> (`PbiUpdateGateEmpty`/`GithubForwardGateEmpty`) statt des Review-Requests; die Kanten routen typgenau zum
> geteilten **`EmptyGateAutoResponder`** (sichtbarer Graph-Knoten, `GATE_SKIPPED_EMPTY`-Event) → leere Antwort
> auf dem NORMALEN Apply-Pfad. + Bridge-Prädikat semantisch (`entries==0` ⇒ ForwardSkipped, kein Snapshot-Pull)
> + ForwardSkipped als benanntes FERTIG. Verdrahtet an den GETEILTEN AddTo-Quellen ⇒ Standalone-CLI UND
> Ein-Graph identisch. **Zwei MAF-Erkenntnisse auf dem Weg (Feature-Matrix!):** ① **Prädikat-Kanten auf
> RequestPorts werden IGNORIERT** (Doppel-Zustellung an Port UND Zweig — deshalb Typ-Routing statt conditional
> edge; R-38-Familie) ② `HitlShell.StartAsync` deutete ein Lauf-Ende ohne Request als Pause (End-Checkpoint →
> Phantom-„PAUSIERT") — Start-Callbacks behandeln jetzt den Apply-Report. **Smoke-Wahrheit:** die Empty-Fixtures
> [2]/[4] NUTZTEN die alte Leer-Gate-Pause als LLM-freien Pause-Trick — auf R-50-Erwartung umgestellt
> (Skip laut + Apply im selben Lauf; Pause/Resume-Mechanik weiter durch [1]/[3]/[5] gedeckt).
> 3 Wächter-Tests (**543**) · Smoke **14/0** · Live: Leer-Pipeline endet in EINEM Lauf mit 2 lauten Skips.
>
> **✅ VERVOLLSTÄNDIGT (17.08., Autor-Audit „sitzt der Knoten überall?"):** dieselbe Marker+Responder-Form auf
> die drei restlichen Sender gezogen — **ingest-/arch-ingest-gate** (profilbewusst: `GateName` neu im
> AspectIngestionProfile), **adjudication-gate** (leere Queue = perfekter Ledger), **cluster-review-gate**
> (0 Korrektur-Ops). Damit gilt die Regel BEWEISBAR an ALLEN 10 Gates: 7× Marker/Scan-Skip (die 5 Responder +
> decision-scan [hatte es schon] + adjudication) · 3× Bridge-Skip (arch-ingest/classify/adr — bestand).
> **Vierte MAF-Erkenntnis dabei (R-38-Familie!):** ein `SendMessage` mit NICHT per `[SendsMessage]`
> deklariertem Typ wird STILL GEDROPPT — der Ingest-Marker verschwand lautlos, bis das Attribut gesetzt war.
> **Smoke ehrlich nachgezogen:** [1] nutzte die ingest-Leer-Pause als LLM-freien Trick (→ Skip+Apply-Erwartung);
> [5] bekam einen DETERMINISTISCHEN Pause-Anker (Fixture-DEC nach dem Core-Backup injiziert → Pause am
> decision-gate → resume → Skips → „Forward übersprungen"; Fixture vor Zähl-Check entfernt, Restore räumt).
> +1 Sammel-Test (**544**) · Smoke **14/0** · Live: Leer-Pipeline = ingest-Skip → decision-Pause → defer →
> pbi-Skip → forward-Skip, alle laut.

**Befund (Lauf `20260817_102027_b9c264` — eine „alles abgelehnt"-Ernte-Runde):** nach dem reject des einzigen
Items pausierte der Lauf ZWEIMAL sinnlos: ① **pbi-gate mit `operations: 0`** (Derive 0, Maker 0, Gate Pass —
trotzdem HUMAN_GATE + Checkpoint-Pause; `open_gate_ui` kehrt „ok" zurück und zeigt NICHTS an [„keine
Operationen"] — für den Autor wirkt die UI kaputt) ② danach **forward-gate mit 0 Ops**: der pbi-Apply schreibt
auch bei 0 Änderungen ein LEERES `github-sync-delta.json`, die `OperationalForwardBridge` prüft nur
**Datei-Existenz** statt `entries > 0` → ForwardPrep(0) → Snapshot-Pull (unnötiger API-Call) → Seed 0 Ops →
wieder Human-Pause. Ausweg war je 2× `resume --accept-all` über leere Gates (harmlos, aber Experiment-Flag für
NICHTS). **Das ist der 9g-Parkplatz „Leer-Spur-Skip" — jetzt mit doppeltem Live-Beleg.**
**Fix-Skizze (klein, 3 Stellen + Tests):** ① `PbiUpdateHitlFinalize`: `ops==0` ⇒ KEIN RequestPort-Gang —
sichtbares Event `PBI_GATE_SKIPPED_EMPTY` + direkt weiter ② `OperationalForwardBridge`: Skip-Bedingung
`entries == 0` (statt nur Datei fehlt) ⇒ ForwardSkipped, kein Snapshot-Pull ③ `GithubForwardHitlFinalize`:
`ops==0` ⇒ Gate-Skip analog. Leitplanke: Skip IMMER als lautes Event (kein stilles Durchrutschen — E0.9).

## R-51 — Steward-Prozess hält den Checkpoint-Store: resume anderswo stirbt still (17.08., Block E) — ✅ ENTSCHÄRFT (Kern-Ursache = R-52)

> **✅ ENTSCHÄRFT 17.08. (Analyse an der Wurzel):** alle 6 Store-Erzeugungen waren längst `using` — der Halter
> war IMMER ein NIE zurückkehrender Runner-Frame im langlebigen Steward-Prozess, und die bekannte Quelle dafür
> ist der R-52-Hänger (Resume ohne Antwort). **R-52-Fix ⇒ jeder Frame terminiert ⇒ Store wird am Pause-Ende
> frei** — kein Kind-Prozess-Umbau nötig, die K13-Entscheidung (in-process, MAF-nativ) bleibt intakt. Dazu zwei
> Steward-Härtungen: **① `LoudOnFault`** — Lauf-Tasks laufen nie mehr unbeobachtet (Fault → laute stderr-Meldung
> + Exit −1 statt „resuming true", dann Stille) · **② `RUN_STILL_ACTIVE`-Wache** am resume_run (arbeitet der Lauf
> noch in diesem Prozess, kommt die ehrliche Sofort-Antwort statt des Store-Konflikts). Verbleibende GRENZE
> (korrekt, kein Bug): während ein Lauf AKTIV arbeitet, hält sein Prozess den Store — Single-Writer-Semantik;
> ein Fremd-Resume scheitert dann LAUT. 2 Wächter-Tests (**547**) · Smoke 14/0.
> **LIVE-BELEG (17.08. abend, Gegenprobe-Lauf `20260817_192742_ef60d3`, EINE Steward-Sitzung ohne /exit):**
> ① UI-„Fertig"-Kette am pbi-gate lief bei OFFENEM Steward durch (19:36:24, reviewer human/review-ui,
> alignments=1) ② forward-gate im Chat entschieden + `resume_run` in-process → `GITHUB_FWD_DONE success` →
> PIPELINE FERTIG exit 0 (19:49:17) — das E8-Todesszenario, widerlegt. Diktat→Core: REQ-42 v3 + PBI-028
> aktualisiert, dry-run, Kangal-Save grün.

**Befund (2 Belege, Block E):** startet der STEWARD einen pipeline-full-Lauf, hält sein Prozess das Handle auf
`checkpoints/index.jsonl` des MAF-`FileSystemJsonCheckpointStore`, solange er lebt (lsof-Beleg PID 61158). Jeder
resume in einem ANDEREN Prozess (gekettetes UI-„Fertig" in E5 · `resume_run` über denselben Steward in E8) starb —
„The store at '…/checkpoints' is already in use by another process" bzw. STILL (kein PIPELINE_RESUME-Event, oder
Event und dann Tod) — und die UI meldete fälschlich „fehlgeschlagen", obwohl NUR der Resume-Teil betroffen war.
**5. MAF-Erkenntnis (R-38-Familie, Feature-Matrix):** der Checkpoint-Store ist prozess-exklusiv — ein langlebiger
Agent-Host + externe Resume-Prozesse schließen sich aus. **Ausweg (etabliert, Prompt kennt ihn):** Resumes von
Steward-gestarteten Läufen per CLI in FRISCHEM Prozess; Steward-`/exit` gibt das Handle frei. **Fix-Kandidat
(nicht dringend):** Store je Operation öffnen/schließen ODER Steward delegiert resume an einen Spawn-Prozess.

## R-52 — resume ohne Antwort-Quelle HÄNGT statt erneut zu pausieren (17.08., Block E) — ✅ GEFIXT

> **✅ GEFIXT 17.08. (exakt die Fix-Skizze):** erkennt der zentrale Responder beim Resume ein unbeantwortetes
> Gate, übernimmt er den RESTAURIERTEN Checkpoint als Pause (`pendingCp = restoreFrom`) und steigt SOFORT aus —
> pointer bleibt auf demselben Checkpoint gültig, Event **`PIPELINE_STILL_PAUSED`** (unterscheidbar von der
> Erst-Pause), Konsole „UNVERÄNDERT PAUSIERT … kein Entscheid gefunden", Exit 6 mit den Gate-Hinweisen.
> Zwei-Bahnen-Prüfung: die Standalone-Bahn (`HitlShell.ResumeAsync`) ist bewusst NICHT betroffen — dort ist die
> Antwort Pflicht-Parameter (`makeResponse`), „Resume ohne Antwort" existiert nur am zentralen pipeline-full-
> Responder. Wächter-Test fährt den echten Zyklus (frische Pause → Resume ohne Entscheid → terminiert,
> Checkpoint identisch) gegen die ECHTE MAF-Runtime. **547 Tests** · Smoke 14/0.
> **LIVE-BELEG (17.08. abend, Gegenprobe-Lauf `20260817_192742_ef60d3`):** bewusster Leer-Resume am pbi-gate →
> `PIPELINE_RESUME` 19:35:12 → `PIPELINE_STILL_PAUSED` in <1 s, identische checkpointId, Steward-Sitzung lief
> ungestört weiter — exakt der Befehl, der mittags endlos hing.

**Befund (2× live am E8-Lauf `20260817_130520_7334e9`):** ein `pipeline-full resume` an einem Gate OHNE
Entscheid-Datei druckt „pausiere (Checkpoint mit offenem Gate wird gesichert)…" und wartet dann EWIG (0 % CPU,
kein Event nach PIPELINE_RESUME, keine neue Pause materialisiert): die Pause-Materialisierung wartet auf einen
NEUEN Checkpoint mit offenem Gate — beim Resume schreitet aber kein Superstep voran, also entsteht nie einer.
**Asymmetrie:** der frische Lauf kann leer pausieren, der Resume nicht. **Gefahr: keine** — alter Checkpoint +
pointer bleiben intakt, der hängende Prozess ist gefahrlos killbar; danach Entscheid-Datei schreiben und erneut
resumen. **Fix-Skizze:** erkennt der Resume-Pfad ein unbeantwortetes Gate, sofort mit dem BESTEHENDEN
Checkpoint re-pausieren (pointer erneuern, LAUT beenden) statt auf `pendingCp` zu warten.

## R-53 — forward-gate-Resume las nur die Steward-Entscheid-Datei, nicht die der Review-UI (17.08., Block E) — ✅ GEFIXT

**Befund (E8-Abschluss):** die Review-UI (`github-forward-review`) schreibt `human-decisions.json`, der zentrale
Resume-Responder las am forward-gate NUR `github-forward-decisions.json` (die Steward-`decide_gate`-Bahn) — der
Autor-Entscheid lag korrekt auf Platte, der Resume fand ihn nicht und lief in den R-52-Hänger. Zwei-Bahnen-
Verstoß; das pbi-gate machte es längst richtig (R-44b liest den UI-Vertrag). **Fix:** Responder liest BEIDE
Quellen (Steward-Datei ?? UI-Datei; Schema identisch, derselbe Loader); Status-Reader-Hinweis nennt beide Bahnen.
**Live-Beweis:** E8-End-Resume `GATE_ANSWERED accepted=1, execute=false` → HOLD verbucht → PIPELINE FERTIG.
Build 0 · **544 Tests** grün.

## R-54 — Adjudikations-UI kennt das Kapsel-Layout nicht: „Transkript-Kontext" bleibt leer (18.08., Block L) — ✅ GEFIXT

> **✅ GEFIXT 18.08. (exakt die Fix-Skizze):** neue testbare Naht `ResolveEnrichmentRunDir(queuePath, repoRoot)` —
> Standard bleibt das Standalone-Layout (step-00 neben der Queue); fehlt es UND liegt der H1-Anker
> `ledger-run.json` neben der Queue, wird die Anreicherung LAUT auf den Kapsel-Sub-Run `runs/ledger/<id>`
> umgelenkt („Kapsel-Layout erkannt"). Best-effort wie zuvor (kaputter Anker ⇒ Standard, nie werfen).
> 3 Wächter-Tests (Standalone bleibt · Anker gewinnt · kaputter Anker fällt zurück).

**Befund (Lauf `20260818_084712_765eea`, adjudication-gate):** der Knopf „Transkript-Kontext öffnen" zeigt
nichts. Ursache code-verifiziert: `ledger-adjudicate-ui` sucht seine Anreicherung (Units/Claim-Katalog)
best-effort NEBEN der queue.json (`<runDir>/step-00-atomic-units/output.json` usw.) — das ist das
STANDALONE-Ledger-Layout. Im Ein-Graph liegt die queue unter `01-ledger/`, die Step-Ordner leben im
Kapsel-SUB-RUN (`runs/ledger/<ledgerRunId>/`), und der Anker dafür (`01-ledger/ledger-run.json`, der
H1-Anker aus Schritt 5 ②!) liegt direkt neben der queue — wird von der UI aber nicht gelesen ⇒ 0 Units,
0 Referenz-Katalog (Konsole sagt es leise: „0 Units (Evidenz)"), Kontext-Knopf leer, Referenz-Autocomplete
fehlt ebenso. Zwei-Bahnen-Lücke: die UI-Anreicherung wurde vor der Kapsel gebaut und nie nachgezogen.
**Fix-Skizze (klein):** fehlt `step-00…` neben der Queue UND existiert `ledger-run.json` → runDir auf
`runs/ledger/<ledgerRunId>` umlenken (eine Stelle in `LedgerAdjudicateUiRunner`, beide Bahnen bedient) +
Wächter-Test auf die Umlenkung. Adjudikation selbst war NICHT blockiert (Evidenz-Zitate stehen im Item).

## R-55 — decision-gate ist beim Resume NICHT interaktiv: Auto-defer-all statt Pause (18.08., Block L) — ✅ GEFIXT

> **✅ GEFIXT 18.08. (exakt die Fix-Skizze, R-52-Zwilling):** Defer-all am decision-gate ist jetzt an das
> EXPLIZITE `--accept-all` gebunden (`answers.TakeAcceptAll()` statt totem `answers is null`) und stempelt
> seine Herkunft laut ins Event (`source=accept-all-flag`). Ohne Flag + ohne Entscheid-Datei bleibt das Gate
> unbeantwortet → R-52-Re-Pause (`PIPELINE_STILL_PAUSED`) → decision-gate-UI → nächster Resume liest die
> Datei. Datei-Weg und Politik-Pfad (accept-all-PROFIL vertagt weiterhin, R-14-G1) unverändert; Smoke [5]
> nutzte das explizite Flag schon immer — Verhalten dort identisch, nur ehrlich deklariert. Drei-Fakten-
> Wächter gegen die echte MAF-Runtime (frisch pausiert · ohne Flag re-pausiert OHNE GATE_ANSWERED · mit Flag
> defer-all inkl. Event-Stempel). Build 0 · **548 Tests** · Smoke 14/0. **Wiedervorlage-Effekt:** die im
> Block L dokumentierten Autor-Entscheide (DEC-003 KEEP · DEC-005 ADOPT_NEW) werden im nächsten Lauf am
> jetzt bedienbaren Tor real nachgeholt (= P4-Rest-Beleg).
> **LIVE-BELEG (18.08., Wiedervorlage-Lauf `20260818_112553_8203f5`):** ① Leer-Resume am decision-gate →
> `UNVERÄNDERT PAUSIERT` in <1 s (gestern: `deferred=5` stumm) ② Autor löste in der UI: DEC-003
> KEEP_ORIGINAL + DEC-005 **ADOPT_NEW** → die volle Maschine feuerte: REQ-05→Superseded · **REQ-84** mit
> Autor-Wortlaut · supersedes-Kante · `contradicts_resolved` beidseitig · **Deckungs-Swap** (PBI-002/005
> covers→REQ-84) · 3 PBIs entblockt · 2 Angleichungen formuliert+angewendet (PBI-Texte tragen
> „Einladungscode") · Forward: gestern HOLD_BLOCKED auf gh#2/#5 → heute UPDATE-Ops (dry belassen) ·
> FERTIG, Kangal 0/0, Core 213. L6/P4-Rest damit NACHGEHOLT.

**Befund (Lauf `20260818_084712_765eea`):** der Autor löste DEC-003 (KEEP_ORIGINAL) und DEC-005 (ADOPT_NEW mit
Wortlaut) in der decision-gate-UI auf — die Datei `decision-gate-decisions.json` wurde korrekt geschrieben,
aber NIE gelesen: der Resume nach dem adr-Gate hatte das decision-gate bereits SELBST mit „alle 5 VERTAGT"
beantwortet (`GATE_ANSWERED deferred=5, reviewer author/resume-defer-all`) — der Lauf pausiert dort auf dem
Resume-Pfad NIE. **Ursache (code-verifiziert):** der Wächter `if (answers is null) return false; // warten:
UI nutzen` ist auf dem Resume-Pfad TOTES RECHT — `pipeline-full resume` konstruiert IMMER ein
`ResumeAnswers`-Objekt (auch ohne Flags), also greift stets das Defer-all-Fallback („Flags können nicht
auflösen"). Interaktives Tor 2 geht damit NUR, wenn die Entscheid-Datei schon VOR dem Scan existiert —
faktisch nie, denn die UI braucht den Request des Laufs. In E8 (17.08.) maskiert, weil defer-all dort
GEWOLLT war. **Kein Schaden:** DECs bleiben offen geparkt (Wiedervorlage im nächsten Lauf), die
Autor-Entscheide liegen dokumentiert in der Datei. **Fix-Skizze (klein, R-52-Zwilling):** im decision-Fall
Defer-all NUR bei explizitem `--accept-all`/`--accept` feuern (`answers.TakeAcceptAll()`-Prüfung statt
`answers is null`); ohne Flags → `return false` → die R-52-Mechanik re-pausiert sauber → UI → Resume liest
die Datei. + Wächter-Test.

## R-63 — Doc-Publish-DAUER-CHURN: der Wahrheits-Fingerabdruck hashte den GANZEN Core (21.08., Slice-S-Live-Abnahme, Lauf `20260821_182305_a51f7b`) — ✅ GEFIXT
**Befund (Autor-Live-Test, zweiter Abgleich direkt nach dem ersten):** `anforderungen.md` + `backlog.md`
kamen als „hat sich geändert" ans Gate, obwohl sich KEINE Wahrheit geändert hatte. Ursache:
`RequirementsDocumentProjection.Fingerprint` hashte den serialisierten GANZEN Core — inklusive der
Apply-BUCHHALTUNG (Doc-Publish-Stempel/Pendings in den Proposals mit `updatedUtc`, Kommentar-Lesezeichen
in Relations-Metadata). Jeder Execute schreibt Stempel ⇒ Abdruck ändert sich ⇒ Frische-Wache re-rendert
die Projektionen (Version+1) ⇒ neuer Hash ≠ Stempel ⇒ UPDATE-Op ⇒ Apply schreibt neue Stempel ⇒ … —
die beiden Docs wären NIE in-sync gewesen. **Fix (an der Quelle):** der Anker hasht NUR, was die
Projektionen rendern — Items + Relations-TRIPEL (FromId/ToId/Typ, ohne Metadata): neue/entfernte Kanten
zählen, Buchhaltung an bestehenden nicht. **Konvergenz-Hinweis:** der ERSTE Abgleich nach dem Fix
re-rendert noch einmal (Datei trägt den Alt-Format-Abdruck) — danach Ruhe. Mechanik-Klasse (dotnet test
grün, Logik-Fehler im deterministischen Teil); Lehre = Fingerprint-Zutaten IMMER gegen die
Schreib-Nebenwirkungen des eigenen Apply prüfen (Stempel dürfen nie Teil des Frische-Ankers sein).

## R-64 — Fehlangebot am forward-gate: Steward bot open_gate_ui an, das Gate hat keinen UI-Weg (21.08., gleiche Session) — ✅ ENTSCHÄRFT (ehrlich gemacht)
**Befund:** Am pausierten `github-forward-gate` bot der Steward „Chat | UI" an; `open_gate_ui` lehnte
korrekt ab („kein UI-Weg"), aber erst NACH zwei Anläufen + einer verwirrenden Zwischenmeldung (Anlauf 1
scheiterte zusätzlich am Zustimmungs-Tippfehler „jaa" ⇒ als Nein gewertet, ohne das zu sagen). Verstoß
gegen die 1b-Regel „angeboten wird nur, was geht". **Fix:** Prompt-Kanal-Matrix trägt jetzt die explizite
NIE-UI-Ansage für forward; `steward-faehigkeiten.md`-Tabelle korrigiert. **Kandidat (nicht gebaut, W2-Eis):**
echter forward-UI-Weg via open_gate_ui (Forward-Review-UI existiert in der CLI-Werkbank) · Zustimmungs-
Parser tolerant („jaa"/„yes") oder laute „als NEIN gewertet"-Meldung.
**AUSBAU ✅ GEBAUT (23.08., Phase-1i ②):** github-forward-gate in open_gate_ui verdrahtet
(GithubForwardReviewRunner.RunForPipelineRunAsync — schreibt human-decisions.json an die bestehende
Zwei-Bahnen-Responder-Naht, kettet pipeline-full-resume; execute bleibt Policy-gebunden) · Prompt-Kanal-
Matrix gedreht (Chat ODER UI an ALLEN Urteils-Gates) · dazu **Doc-Diff** an UPSERT_FILE (GithubDocsMirror:
Spiegel des publizierten Stands beim Apply, Versions-Sprung + Sektions-Diff in der Review-Karte) und
**Relevanz-Anzeige** (Autor-Fund „44 Ops jedes Mal": UI + Chat listen NUR Änderungs-Ops, NO_CHANGE als
Summenzeile; Chat-Sammel-Akt nur noch über Änderungs-Ops, Rest deterministisch aufgefüllt). 668 Tests.

## R-73 — Antwort-Anker-Angebot verpasst: die Diktat-Vorprüfung suchte Items + Ablehnungen, aber NICHT den DEC-Topf (22.08., Schritt 5a) — ✅ ENTSCHÄRFT (Prompt-Pflicht)

## R-74 — Provenienz-Kanten unvollständig materialisiert: 36/237 Core-Items ohne maschinell auflösbare Herkunfts-Kette (06.09., Traceability-Audit v2) — BEFUND, offen
- Fund (deterministisch, `tools/eval/w2_traceability_audit.py`, nur explizite Kanten, Snapshots/Checkpoints exkludiert): 171/237 Items bis zum wörtlichen Zitat im run-spezifischen Transkript auflösbar, 30 referenziell (Definition ohne belastbares Zitat), **36 OFFEN** in zwei Klassen:
  - **A (21): Item trägt KEINE direkten Referenz-Felder** und keinen gerichteten Relationspfad — überwiegend Autor-/Gate-geprägte Herkünfte (AUTHOR_OPEN_QUESTION/RISK, MEETING_OPEN_QUESTION, DEC-Topf, AuthorFront). Die Herkunft existiert (Gate/Diktat), ist aber nicht als Referenz-Kante am Item materialisiert.
  - **B (15): Referenz vorhanden, aber die DEFINITION liegt außerhalb der Run-Kanten-Closure** (CoreAnalyst-CAs, GithubInbound-GHs, AuthorFront-AFs, Steward-Ära-Claims) — die Kante Quell-Run → Definitions-Run ist in den Run-Artefakten nicht verzeichnet.
- Einordnung: KEIN Widerspruch zur Gate-Governance (die Items sind autorisiert); es fehlt die MASCHINENLESBARE Herkunfts-Kante für die späten Herkunftsklassen. Die Ledger-Bahn (Klasse Extracted früher Phasen) ist nahezu vollständig auflösbar.
- TODO (nach Thesis / Steward-Slice): beim Minting von Gate-/Analyst-/Inbound-Items die Definitions-Referenz (Run+Artefakt) als sourceArtifactId/sourceDecisionId materialisieren; Backfill-Werkzeug optional.
- Beleg: `runs/e2e-evidenz/traceability-audit.json` (inkl. Core-/Skript-SHA).
- ★ KORREKTUR 06.09. spät (Audit v2.3, Zweitprüfer fand Rest-Hop im Consumable-Index): strengere
  Zahlen 137 bis Zitat · 38 referenziell · **62 OFFEN** (A 21 = Referenzfelder leer · B 41 =
  Definitions-Kante nicht in config/run-report verzeichnet — größter Block: 24 re-clarify-PBIs,
  deren Läufe die Ledger-Consumable-Kante nicht in ihren Run-Artefakten führen).
**Befund:** Beim Mandantenrahmen-Diktat (beantwortet wörtlich die offene DEC-009) machte der Steward eine
gute Vorprüfung (Item-Suche, REJ-Gedächtnis, Fassungs-Coaching) — bot aber KEIN decisionRef an: er hatte
die offenen Entscheidungen nicht auf Passung geprüft. Ohne Anker wäre die spätere Schließung als
⚠ „geklärt ohne Nachweis" gelandet. **Fix:** Pflicht-Vorprüfung im Prompt — jedes Architektur-Diktat
prüft die offenen DECs auf inhaltliche Passung und benennt den gesetzten Anker laut. (Modell-Verhalten;
die deterministische Absicherung dahinter existiert bereits: ohne Anker zeigt §3 ehrlich ⚠ — der Fund
wäre also sichtbar geworden, nur später.) Wirkt ab dem nächsten Steward-Start; in laufender Session:
decisionRef explizit mitdiktieren. **Nachtrag (Redo-Versuch):** die Regel griff nur für OFFENE DECs —
beim Heilungs-Fall („DEC schon geschlossen") ließ der Steward den Anker begründet weg. Regel geschärft:
auch geschlossene DECs prüfen; „geschlossen" ist kein Anker-Verzicht (der Anker ist Nachweis, keine
Auflösung — er heilt ⚠ nachträglich).

## R-72 — Kopfzeilen-Push: fremde Wahrheits-Änderungen re-renderten Projektionen ohne Inhalts-Änderung (22.08., Schritt-4-Lauf `20260822_140910_eb82f5`) — ✅ GEFIXT
**Befund (Autor: „bei Backlog wurde einfach nur ein neuer Fingerprint gemacht — wollen wir das?"):**
Die DEC-015-Schließung änderte den Wahrheits-Fingerabdruck (deckt ALLE Items) ⇒ anforderungen+backlog
wurden neu gerendert und publiziert — beim Backlog war die EINZIGE Änderung die Kopfzeile
(Version/Stand/Fingerabdruck). Ein Push je fremder Wahrheits-Änderung = Version-Churn light.
**Fix:** beide Projektions-Runner schreiben NUR bei echter INHALTS-Änderung (geteilte `SameBody`-Wache:
Vergleich ohne die volatile Stand-Zeile; unverändert ⇒ kein Write, Version bleibt). Nebeneffekt sauber:
Versionen zählen ab jetzt nur noch bei echten Inhalts-Änderungen. Pin: fremde Änderung ⇒ byte-identisch/
Version hält · echte Änderung ⇒ Write/Version+1. **Feil-Kandidat notiert (kein Fix nötig):** der Steward
meldet Lauf-ENDE nicht proaktiv (nur Pausen) — „✅ fertig"-Meldung als UX-Kandidat.

## R-71 — Kreislauf-Kalibrierung: falsche ⚠ für Widerspruchs-Auflösungen + „30 Belege"-Wächter-Rauschen (22.08., T1-Erstrender, Lauf `20260822_105831_859ff4`) — ✅ GEFIXT
**Befund a (falsche ⚠):** §3 zeigte DEC-003/005/008 als „⚠ geklärt ohne Architektur-Nachweis" — das sind
aufgelöste WIDERSPRUCHS-DECs (mit Ziel-Requirement): ihre Schließung ist durch die Auflösungs-Maschine
legitimiert (ADOPT/REFINE/KEEP am Ziel), ein answersDecision-Anker existiert dort konstruktiv nie.
**Fix:** ziel-behaftete DECs (deterministisches Signal: contradicts(_resolved)-Kante) sind von der
⚠-Regel ausgenommen — ⚠ gilt exakt der Klasse, für die es gebaut wurde: ziellose Architektur-Fragen.
**Befund b (Rauschen):** die Frische-Notiz verlangte, dass JEDES aktive ARCH-Item im C4 erwähnt ist →
„30 nicht berücksichtigte Belege" als Dauer-Meldung. Falscher Maßstab: WELCHE Rahmen einen Kasten
verdienen, ist Deutung. **Fix:** Beleg-Stand-STEMPEL (unsichtbarer Kommentar in §3, geschrieben NUR beim
Autor-Save — der Zeichner hatte den Bestand im Input, „gesehen"; der Abgleich-Refresh ERHÄLT den Stempel,
damit sich die Meldung nicht selbst zum Schweigen bringt) — gemeldet wird nur das DELTA („N NEUE Belege
seit dem letzten C4-Stand") + weiterhin tote Zitate. Pins: Ziel-Ausnahme · Beleg-Stand-Zyklus am echten
Save (Ruhe→Delta→Ruhe) · Delta-Notiz + tote Zitate. Nebenbefund korrigiert: DEC-003/005/008 sind KEINE
Phantom-Ids (Fehldeutung 22.08. vormittags) — sie existieren als aufgelöste Widerspruchs-DECs.

## R-70 — Chat-Bahn kannte die dritte Auflöse-Option nicht: globale Options-Liste statt per-Item-Palette (22.08., Kreislauf-Test T1a, Lauf `20260822_105831_859ff4`) — ✅ GEFIXT
**Befund (Autor: „irgendwie fehlt da was"):** Die decision-gate-CHAT-Vorlage projizierte EINE globale
Options-Liste (alte Vierer-Palette) — die neue per-Item-Wahl `NO_TRUTH_NEEDED` (nur aspekt-markierte
DECs) erschien nie; zusätzlich zählten Submit-Tool-BESCHREIBUNG und Submit-VALIDIERUNG die Outcomes hart
auf (drei Werte) — der Chat hätte die Option selbst auf Zuruf abgelehnt. Klassiker der R-60-Klasse:
neuer Vertrags-Wert muss ALLE Schichten in EINEM Zug treffen (hier fehlten drei Chat-Schichten; die UI
war korrekt, weil sie die per-Item-FieldOptions liest). **Fix:** EINE Palette-Quelle
`PipelineDecisionReviewAdapter.OptionsFor(targetless, aspect)` — UI-Item, Chat-Vokabular
(`ForDecision(targetless, aspect)` inkl. `resolve/NO_TRUTH_NEEDED`-Code) und Gate-Vorlage (per-Item
`optionen` statt global; liest `aspect` aus dem persistierten Request) projizieren daraus; Submit-
Validierung nutzt `DecisionOutcome.All` + reason-Pflicht für NO_TRUTH_NEEDED; Tool-Beschreibung
nachgezogen. Pin: Vokabular-Paletten (arch=3 mit NO_TRUTH · Frage=2 · Widerspruch=4 unverändert).

## R-69 — Doppel-Kopf im Artefakt: der Update-Zyklus stempelte über den mitgeschleppten Alt-Kopf (21.08., c4.md v2) — ✅ GEFIXT
**Befund:** c4.md v2 begann mit dem frischen Versions-Kopf UND direkt darunter dem kompletten alten
(„Version: 1"). Ursache: der Drafting-Agent bekommt den Stand INKLUSIVE System-Kopf als Input und
übernahm ihn regelkonform „wörtlich"; der Save stempelte davor. **Fix (deterministisch, kein
Prompt-Vertrauen):** `AuthoredDocument.StripStampedHeader` streift führende „# Titel"+Stempel-Blöcke
vor dem Stempeln ab (wiederholt, falls gestapelt) — Pin: Save mit mitgeschlepptem Kopf ⇒ genau EIN
Kopf, Alt-Stempel weg, Inhalt unangetastet. Bestehende Doppel-Kopf-Datei heilt sich beim nächsten
Update von selbst.

## R-68 — C4 auf GitHub unleserlich: experimentelle Mermaid-C4-Syntax + Detail-Texte in den Kästen (21.08., c4.md v1 live) — ✅ GEFIXT (Rezept)
**Befund (Autor):** Das publizierte C4 rendert, aber „viele Sachen nicht wirklich lesbar" — Mermaids
`C4Context/C4Container` ist experimentell: feste Mini-Kästen, lange deutsche Beschreibungen abgeschnitten,
ab ~8 Knoten Layout-Kollaps; dazu vier Lücken-Kästen, die alle wörtlich „?" hießen. **Fix (nur Rezept,
kein Code):** C41-Prompt verbietet die C4-Syntax → normales `flowchart TB` + `subgraph`-Grenze; Labels
kurz (Details wandern in die Beleg-Tabelle, jetzt mit Technologie-Spalte); Lücken-Kästen mit Kurznamen
(„? Auth-Backend"). Nächstes „aktualisiere das C4" erzeugt die lesbare Fassung; Semantik (Ebenen, Grenze,
Beleg-Pflicht, Lücken) unverändert.

## R-67 — All-in-Sync-Abgleich hielt am Gate für 43× NO_CHANGE an (21.08., In-Sync-Zweitlauf `20260821_202923_e249b8`) — ✅ GEFIXT
**Befund (Autor: „ist das nicht sinnlos, dass es soweit geht?"):** Der Zweitlauf nach der Erst-Publikation
war komplett in sync (0 Schreib-Ops) — trotzdem pausierte das forward-gate mit einem Plan aus 43×
NO_CHANGE und verlangte einen Sammel-Akt. Zeremonie ohne Entscheidung: R-50 prüfte nur „0 Ops", nicht
„0 ENTSCHEIDBARE Ops". **Fix:** Finalize routet auch den Nur-NO_CHANGE-Plan über den lauten
Leer-Gate-Weg (Event `GITHUB_FWD_GATE_ALL_IN_SYNC` + Konsole „alles in sync, nichts zu entscheiden");
der Auto-Responder akzeptiert die NO_CHANGE-Ids, damit die Summary ehrlich `noChange` zählt (nicht
`skipped`). Der In-Sync-Beweis liest sich jetzt so, wie er sich anfühlen soll: Lauf FERTIG ohne Halt.

## R-66 — Lauf endete OHNE Forward, obwohl frische Artefakte auf Erst-Publikation warteten (21.08., Rampen-Test, Lauf `20260821_194944_90c445`) — ✅ GEFIXT
**Befund (Autor: „nach dem dec gate kam dann nichts mehr"):** Der Diktat-Lauf (nur Fragen/Risiko → DECs,
keine PBI-Änderung) übersprang die Forward-Stufe — glossar/c4 blieben unpubliziert. Ursache: das
Skip-Prädikat der `OperationalForwardBridge` fragte Sync-Delta (R-50) und Vermerk-Quelle (R-62), aber
NICHT die Doc-Publish-Quelle — die DRITTE Schicht derselben Familie. **Fix:** `CountStaleDocsAsync` im
Skip (gleiche EINE Quelle wie der Seed: Refresh + `GithubDocPublish.SeedOps`; ohne Ziel-Repo weiterhin 0);
Konsole/Event benennen jetzt alle drei Quellen. Pin: Brücken-Graph-Test „läuft bei leerem Delta, wenn
Doc-Änderungen anstehen". **Familien-Lehre (R-50→R-62→R-66):** JEDE neue Forward-Fracht-Quelle muss im
selben Move ins Brücken-Prädikat — sonst wird sie in PBI-losen Läufen still verschluckt.
**Folge-Fund im Tripel: das Smoke-Netz war workspace-abhängig und potenziell SCHARF** — sein
pipeline-full-Check verließ sich darauf, dass leere Läufe den Forward skippen; mit R-66 + zufällig stale
Docs + `execute:true` (Live-Abnahme!) lief der Smoke-Forward mit accept-all möglicherweise ECHT
(Doc-Writes; Core-Restore verwarf danach die Stempel ⇒ nächster Abgleich zeigt diese Docs einmal erneut —
konvergiert, kein Schaden). **Smoke-Härtung:** run-config wird wie der Core gesichert und für die
Smoke-Dauer entschärft (`repo:""` + `execute:false` ⇒ offline, deterministisch, nie scharf), Restore am
Ende. Smoke wieder 14/0.

## R-65 — Doppel-Freigabe-Schleife am Artefakt-Save: zwei Prompt-Regeln bissen sich (21.08., Slice-S-Live-Abnahme, A5/C4) — ✅ GEFIXT
**Befund (Autor-Session):** Beim C4-Save fragte der Steward in Prosa um Freigabe, las auf „ok" erneut vor,
ein Zug blieb leer („…"), gespeichert wurde nichts — der Autor musste dreimal bestätigen. Ursache: der
neue AUTOR-ARTEFAKTE-Ablauf verlangte „ERST auf das explizite Autor-Ja → save" und kollidierte mit der
**Ein-Bestätigungs-Regel** der Sprech-Schicht (Politur 1a: keine Prosa-Vorfrage vor ⚿-Tools — die
Zustimmungs-Abfrage IST die Bestätigung). **Fix:** Ablauf-Schritt 3 neu: Zustimmungs-Signal des Autors
(„passt"/„speichern") ⇒ SOFORT save_authored_doc rufen, die ⚿-Harness-Abfrage ist die Freigabe; kein
erneutes Vorlesen. Lehre: neue Prompt-Abläufe IMMER gegen die bestehenden Sprech-Regeln lesen (dieselbe
Klasse wie R-60: neuer Vertrag muss ALLE Schichten in einem Zug treffen — hier die Regel-Schicht).
**R-65b (gleiche Session, Schritt 3 des Rampen-Tests):** dasselbe Muster am GATE-SUBMIT — auf „beide
aufnehmen" las der Steward den Sammel-Akt erneut vor, wartete auf „ja", produzierte einen LEEREN Zug
(„…"), erst das zweite „ja" feuerte submit_ingest_gate_decisions. Fix: explizite SAMMEL-AKT-REGEL im
Prompt (Urteil im Chat = Auftrag ⇒ submit SOFORT im selben Zug; kein Vorlese-Echo, nie ein leerer Zug;
knappe Nachfrage NUR bei echter Mehrdeutigkeit). Der leere Zug selbst ist Modell-Verhalten (gpt-5.4)
auf Bestätigungs-Kaskaden — die Regel entfernt die Fehlerfläche, statt das Modell zu therapieren.

## R-62 — Reine Ablehnungs-Läufe skippten den Forward: der ✕-Vermerk blieb STUMM (20.08., Nachprobe K7) — ✅ GEFIXT

**Befund (Lauf `20260820_182614_a5f6b3`, Nachprobe-Playbook Akt 8):** Body-Edit geerntet → am Tor
ABGELEHNT → Lauf endete nach der PBI-Stufe: `07-github` LEER, kein ✕-Vermerk auf #43. **Ursache:** die
`OperationalForwardBridge` skippt bei leerem github-sync-delta (R-50-Leer-Regel) — reine Ablehnungen
erzeugen aber KEINE PBI-Änderung, und die Vermerke werden erst in der Forward-SEED abgeleitet: der
②-Ablehnungs-Vermerk hing damit an der Bedingung „es wurde auch etwas angenommen" (im K1/K2-Lauf maskiert,
weil das apply den Forward erzwang). **Fix:** das Skip-Prädikat fragt die Vermerk-Quelle mit
(`GithubCommentVermerk.TryDeriveFromRun`) — bei anstehenden Vermerken läuft der Forward auch mit leerem
Delta (Plan = nur NOTE_COMMENT-Ops; das Gate nimmt sie ohne Coverage-Pflicht, test-gepinnt). Live-Beweis =
Wiederholung der Nachprobe (die Testzeile steht noch in #43 — Zweitanlauf bringt zudem die
R-35-„Schon-einmal-abgelehnt"-Warnung als Bonus-Beleg). **608 Tests · Smoke 14/0.**
**R-62b — zweite Schicht derselben Familie (Lauf `183855`, erster Vermerk-ONLY-Plan überhaupt):** der Lauf
erreichte dank R-62 das Forward-Gate, aber der Apply starb an einer NullReference — die Client-Erstellung
(„braucht der Plan echte Writes?") kannte NOTE_COMMENT nicht; Vermerke ritten bisher immer auf Updates mit,
der Client existierte ZUFÄLLIG. Fix: `RequiresClient` als testbare Naht inkl. NoteComment; Pin
`Vermerk_only_Plan_verlangt_den_GitHub_Client`. **609 Tests.**
**Lehre (Zwei-Bahnen-Verwandte):** eine Fähigkeit, die auf einer NACHGELAGERTEN Stufe reitet, braucht ein
Skip-Prädikat, das SIE kennt — sonst stirbt sie in genau dem Fall, für den sie gebaut wurde; und der
„zufällig vorhandene" Client der Nachbar-Ops ist dieselbe Falle eine Ebene tiefer.
**Rest-Beobachtung (Kandidat, kein Sofort-Bau):** ein bei WRITE-FEHLER verfallener Vermerk ist NICHT
nachholbar — Vermerke sind lauf-gebunden (aus Gate-Entscheiden abgeleitet), `run_reproject` heilt nur
Core-Projektionen. Heil-Weg heute: die Drift erneut ernten → erneut entscheiden → neuer Vermerk.
**Glättungs-Kandidat (Autor-Frage 20.08. spät, kein Sofort-Bau):** der Ablehnungs-Lauf eines Body-Edits
könnte die overwrite-Op DIREKT im selben Forward-Gate mit anbieten (Reject + Aufräumen = EIN Halt statt
Wiedervorlage-Schleife bis zum nächsten Abgleich) — der Lauf kennt Drift UND Reject-Entscheid ja bereits.
Faustregel bis dahin: Kommentare/adoptierte Edits konvergieren von selbst; NUR „Body-Edit + reject"
braucht das eine bewusste overwrite-Aufräumen (an JEDEM künftigen Forward-Gate miterledigbar).

## R-61 — Reproject löschte die Kommentar-Anker: „Geister-Kommentare" kamen als neue Ernte-Beute zurück (20.08. spät) — ✅ GEFIXT

**Befund (Autor: „warum wurden die vorhin bei den Ernten nicht gefunden?"):** Die Selbst-Proben-Ernte
`165512` legte plötzlich LÄNGST VERARBEITETE Kommentare ans Tor — die #7-Ur-Idee (wurde am 17.08. zu
REQ-78) und die per Steward gepostete Team-Rückfrage auf #12 (deren Frage längst als DEC-006 lebt).
**Ursache (code-verifiziert):** Die Kommentar-Anker (`lastProcessedCommentId`) leben in der Metadata der
Mapping-Relation — und `CoreGithubMapping.MakeRelation` baute die Map beim Refresh FRISCH (rettete nur die
Hash-Stempel). Jeder Reproject-Write ersetzte alle Mapping-Relationen ⇒ ALLE Anker weg ⇒ alle je
verarbeiteten Kommentare wieder „neu". Belegkette: Ernten 131504/151033 sahen auf #7 nur die 2 Kommentare
ÜBER dem Anker; nach den Reprojects sah 165512 alle drei + #12 + #34. **Kein Wahrheits-Schaden** (Resolver
ordnete die Wiedergänger korrekt als RESTATE/ALREADY_DECIDED der bestehenden Wahrheit zu, Gate hielt) —
aber Wiedergänger-Rauschen. **Fix:** MakeRelation ERBT die komplette Vorgänger-Metadata und überschreibt
nur Systemfelder (Remap bleibt bewusst anker-frei — der Anker gehört zum alten Issue). Pin:
`Mapping_Refresh_erbt_den_Kommentar_Anker_statt_ihn_zu_loeschen`. Selbstheilung des Bestands: das APPLY der
anstehenden Wiedergänger am Gate stempelt die Anker neu (kein Heil-Werkzeug nötig). Verwandt:
Echo-Schutz am Collector (System-Vermerke nie Beute — POLITUR-LOG 20.08. spät). **608 Tests.**

## R-60 — Abgelehnter Fremd-Edit hält die DRIFT-SPERRE für immer: der Konvergenz-Kreis endet bei REJECT in einer Sackgasse (20.08., Projektions-Nachzug) — ✅ GEFIXT (beide Teile)

**Befund (Ernte `20260820_151033_17a2f2` + Reproject-Plan `151313`, Issue #45):** Die Test-Rahmen-Zeile aus
Akt 7 wurde geerntet und am Gate ABGELEHNT (REJ-012) — aber die DRIFT-SPERRE des Forward-Seeds kennt das
R-35-Gedächtnis nicht: #45 bleibt dauerhaft FLAG_DRIFT („erst ernten") und wird vom Reproject NIE neu
geschrieben, obwohl der Autor längst entschieden hat. Konvergenz geht nur durch exaktes manuelles
Zurück-Editieren des Bodys (Whitespace-genau — fragil). **Zweiter Teil-Befund (Übergangs-Artefakt, ✅
sofort GEFIXT):** der neue Markdown-Parser las Alt-Stil-Bodies nicht → die Drift-Beschreibung für #45 war
FALSCH („Statement weicht ab" + „AK ENTFERNT" statt der echten Rahmen-Zeile) — Fix: Legacy-Kopf-Aliase im
Parse (Lesbarkeit für Alt-Issues + Beleg-Läufe; geschrieben wird nur neuer Stil), Pin
`Parse_versteht_den_Alt_Stil_der_Vor_Nachzug_Issues`; **603 Tests**. **Rest-Fix ✅ GEBAUT (20.08. spät, „bewusst auflösen" — der im Seed-Kommentar VERSPROCHENE, nie gebaute
Weg):** Der Sperren-Zweig hatte KEINEN Ausgang — Ernte stempelt nicht, Reject stempelt nicht, nur ein Write
stempelt, und den blockte die Sperre (live erlebt: #45 blieb nach Ernte→REJECT→manuellem Rück-Edit wegen
Whitespace-Hash ewig FLAG_DRIFT). Bau: die FLAG_DRIFT-Op trägt die Core-Projektion (Title/Body/Labels) MIT;
am Gate gibt es den EIGENEN Entscheid-Wert `overwrite` („⚠ Drift bewusst überschreiben") → Apply schreibt
die Projektion und STEMPELT neu. Sicherheit: `apply`/accept-all/Policy-Modi lösen NIE aus (sonst würde ein
Sammel-„alle ausführen" Drifts still überschreiben) — nur der explizite Autor-Entscheid. Pins:
`FlagDrift_schreibt_NUR_mit_explizitem_overwrite_Entscheid` + Seed trägt Body/Labels. **604 Tests.**
Die R-35-Matching-Variante (Sperre erkennt abgelehnte Edits selbst) bleibt bewusst UNGEBAUT — der
menschliche overwrite-Entscheid ist die ehrlichere Governance (kein Text-Matching-Risiko). **Nachwehe ✅
(Autor-Wunsch):** Trailing-Whitespace je Zeile ist jetzt hash-normalisiert (informationslos ⇒ nie Sperre;
eigene Render tragen keins ⇒ Alt-Stempel bleiben gültig; Inhalts-Edits zählen weiter). **605 Tests.**
**Bugfix am eigenen Fix (Lauf `154940`, alle 43 Ops „already-applied"/0 Writes):** ① der S3-Idempotenz-
Kurzschluss („PBI bereits gemappt") fing FLAG_DRIFT VOR dem overwrite-Zweig ab — jetzt ausgenommen, wenn
die Op im overwrite-Set liegt (bewusster Re-Write ≠ Doppel-Write). ② Beobachtung im selben Lauf: der
Steward reichte `apply` MIT Überschreib-Begründung ein — die Sicherung verweigerte korrekt (apply
überschreibt nie); der explizite Entscheid-Wert `overwrite` ist Pflicht. Live-Beweis des Wegs steht noch aus
(nächster Reproject).

## R-59 — REFINE-Apply verschluckte die Analyst-Herkunft (`analystKategorie`) (20.08., Analyst-Abnahme Akt 4) — ✅ GEFIXT

**Befund (Tor-Lauf `20260820_095055_5c8547`, Analyse-Lauf `20260820_075153_7878ac`):** der kuratierte
Analyst-Fund CA-5 (nfr:security, „lokale Bewohnerdaten verschlüsselt") wurde vom Resolver als **REFINE auf
REQ-36** aufgelöst — und der REFINE-Zweig in `IngestionApply` baute die neue Fassung mit `t with { Text, … }`
**ohne die Metadata des Incomings zu mergen**: `analystKategorie`/`herleitung`/`linse` (und ebenso
GitHub-Anker) starben still am Apply. Wirkung: die NFR-Sektion des Anforderungsdokuments (Z4) wäre trotz
Autor-Adoption leer geblieben — dieselbe „Besteller-Falle" wie beim Vorprüfungs-Fund vom 19.08., nur auf dem
REFINE- statt NEW-Pfad (`AnalystOriginMeta.CarryOver` hing nur an `IngestMeta` = New/NewRelated/Contradict).
**Fix (an der Naht):** REFINE merged jetzt Ziel-Metadata + `GithubOriginMeta.CarryOver` +
`AnalystOriginMeta.CarryOver` des Incomings in die neue Fassung. Pin:
`Refine_merged_die_Analyst_Herkunft_des_Incomings_in_die_neue_Fassung`. Build 0 · **597 Tests**.
**Lehre:** CarryOver-Metadata sind an JEDEM inhaltsschreibenden Apply-Zweig fällig, nicht nur beim Item-Neubau.

## R-58 — Rejections-Suche war Phrasen-Match: die Wiedervorlage-Warnung fand ihre Beute nicht (18.08., Abnahme 3.0) — ✅ GEFIXT

**Befund (Session `abnahme3`, Lauf `20260818_200738_bbe6ba`):** die Schreib-Seite von R-57 sass (REJ-007/008
mit woertlicher Begruendung im Core, console.log Z. 11 belegt) — aber beim Nach-Diktat kam KEINE Note:
`search_rejections` suchte „Besuchs-Erinnerung Angehoerige bestaetigt …" als GANZE PHRASE (QueryText.Contains)
→ 0 Treffer. Parallel-Fund: das Tor-1-Resolver-Tool hatte eine EIGENE (Token-)Suche ohne Umlaut-Faltung/
Bindestrich-Split — zwei Implementierungen, beide unvollstaendig.
> **✅ GEFIXT 18.08.:** EINE Kern-Naht `IngestionRejections.Search` — Stichwort-Recall (QueryText.Tokens:
> Umlaut-Faltung + Bindestrich-Split + ≥4-Zeichen-Tokens), Token-ODER mit Treffer-Ranking; BEIDE Tools
> (Steward + Tor-1-Resolver) rufen sie. Bewusste Kontrast-Doku: die grosse Wahrheits-Suche
> (list_core_items) bleibt UND-strikt (R-41 — kein OR-Raten im 200+-Store); Recall-first gilt NUR fuer den
> kleinen Warn-Bestand. Wächter: exakt die fehlgeschlagene Live-Query findet jetzt den Eintrag ·
> Ranking · Leer-Query-Vertrag. Build 0 · **572 Tests**.

## R-57 — Chat-Ablehnungen an Tor 1 landen NICHT im R-35-Gedächtnis (18.08., Politur-Abnahme) — ✅ GEFIXT

> **✅ GEFIXT 18.08. (1d-Slice, Clean-Code durch LÖSCHEN):** `IngestGateDecisions.TryLoadAnyFile` ist jetzt
> DIE EINE Lese-Stelle für Tor-1-Entscheide (Chat- ODER UI-Vertrag, schema-gleich, EIN Zieltyp) — der
> R-35-Ablehnungs-Rekorder UND die Standalone-Werkbank (`ingest-apply`, N3-Fund gleich mit erledigt) rufen
> sie statt eigener Datei-Zugriffe. **Zweite Schicht des Funds beim Bau entdeckt:** der Rekorder filterte
> auf das UI-Wort „skip" — der Chat sagt „reject"; Filter jetzt „alles außer apply" (P2a-Semantik: jede
> begründete Nicht-Übernahme ist Wissen). 3 Wächter-Tests (Chat-reject → ingest_rejection mit wörtlicher
> Begründung · Chat-Datei-Vorrang · UI-Weg unverändert). Build 0 · **570 Tests** · Smoke 14/0.

**Befund (Lauf `20260818_183319_b3d6e6`, Abnahme-Szenario):** der Autor lehnte AF-1 im CHAT ab (P2a-Begründung
„Status-Detail fehlt"); beim Nach-Diktat kam KEINE „Schon-einmal-abgelehnt"-Note. Beweis in Sekunden über das
NEUE Lauf-Protokoll (`logs/console.log` Z. 11 — die 1b-Weiche zahlt sich am ersten Tag aus):
`[ingest-apply] R-35: keine human-decisions.json im Plan-Ordner — Ablehnungen werden nicht aufgezeichnet.`
Core-Gegenprobe: 6 ingest_rejection-Proposals, ALLE aus UI-entschiedenen Läufen — keiner vom Chat-Reject.
**Ursache:** der R-35-Ablehnungs-Rekorder im Ingest-Apply liest NUR den UI-Vertrag (`human-decisions.json`);
der Chat-Vertrag (`ingest-gate-decisions.json`, R-44) wird beim ANTWORTEN (Resume-Responder, TryLoadAny)
gelesen — beim AUFZEICHNEN nicht. Zwei-Bahnen-Lücke, dieselbe Familie wie der N3-Nebenfund (Standalone-Apply).
**Fix-Skizze (1d-Slice):** dieselbe TryLoadAny-Naht am Rejection-Rekorder (oder sauberer: der Apply bekommt
die Entscheide vom Responder GEREICHT statt Dateien doppelt zu lesen — EINE Lese-Stelle) + Wächter-Test
„Chat-Reject erzeugt ingest_rejection". Bis dahin: Ablehnungs-Gedächtnis nur über den UI-Weg vollständig.

## R-56 — ADR-Nummer: UI-Vorschau sagt 0018, Datei wird 0001 (18.08., Block L) — ✅ GEFIXT

**Befund (Lauf `20260818_084712_765eea`, adr-gate) — Ursache beim Bau KORRIGIERT:** beide Seiten nutzen
DIESELBE Quelle (`NextAdrNumber` aus dem Core; Erst-Diagnose „zwei Quellen" war falsch). Der echte Fehler:
die Vorschau nummerierte nach LISTEN-Position (`nextNo + i` — als würden alle Entwürfe davor freigegeben),
der Apply vergibt nach FREIGABE-Reihenfolge ⇒ bei Teil-Freigabe log die Vorschau zwingend (FCM Position 18
→ „0018", freigegeben nur er → Datei 0001). Eine konkrete Nummer ist vorab prinzipiell NICHT versprechbar.
Prüfauftrag positiv erledigt: ARCH-46 trägt `architecture.adrId=ADR-0001, adrStatus=accepted` korrekt im
Core (nächste freie = 0002, Kette konsistent).
> **✅ GEFIXT 18.08.:** die Vorschau verspricht KEINE konkrete Nummer mehr — Platzhalter `ADR-XXXX` im Titel
> + ehrliche Kopfzeile „…(fortlaufend ab ADR-000N)" mit der echten nächsten freien Nummer aus dem Core;
> Apply unverändert (war korrekt). Löst den U5-Feinschliff „prospektive Nummer" (06.08.) bewusst ab —
> der Pinning-Test hat den Vertragswechsel wie vorgesehen angeschlagen und wurde mitgezogen. Wächter pinnt
> Platzhalter + Kopfzeilen-Nummer. Zusammen mit R-54: Build 0 · **552 Tests** · Smoke 14/0.

## R-49 — GitHub-Pull lief ANONYM (404 seit Privat-Schaltung); toter Lauf-Task im Chat als „läuft noch" (17.08., Testplan Block G1) — ✅ GEFIXT

**Befund (Lauf `20260817_093809_99851a`, `--from-github` via Steward):** der Auto-Pull bekam **404** — der
otel-Span zeigt den GET **ohne Auth**: `LoadWithGitHubApiAsync` fiel ohne explicitToken auf `GITHUB_TOKEN`/
`GH_TOKEN` zurück (beide ungesetzt) ⇒ **anonymer Request** ⇒ privates Repo unsichtbar. Funktionierte nur,
solange das Repo public war (bis 11.08.) — **R-47s Zwilling auf der LESE-Seite** (betroffen: from-github-Auto-
Pull, `pull_github_snapshot`, CLI `github-snapshot`; reproject/SnapshotExecutor sauber, da expliziter Token).
**Zweiter Teil:** der Runner starb NACH der runId-Meldung mit exit 2 — kein Event, kein Artefakt ⇒ der Steward
sagte „läuft noch (kann dauern)" über einen **toten Task** (R-18-Geschwister; Diagnose: 0 TCP-Verbindungen,
CPU eingefroren). **Fixes:** ① geteilte Lese-Token-Kette `ResolveReadToken` (explicit → AGENTIC → TEST →
GITHUB_TOKEN → GH_TOKEN — identische Priorität wie MCP/post_comment; nie anonym) in Issues- UND Kommentar-Pull.
② Frühausstiege des from-github-Blocks schreiben **`PIPELINE_ABORTED`** (Grund) → `PipelineRunStatusReader`
neuer Zustand **Aborted** („ABGEBROCHEN: <Grund> · NEU starten, kein resume") → Prompt-Pflicht „nie als läuft-
noch vertrösten". 2 Tests (**540**) · Live: CLI-Pull holt wieder Snapshots · Smoke 14/0 (clean-Gates).
Toter Lauf `093809` bleibt als Beleg. **Nebenbei bewiesen (Block G0):** die Warn-Note feuerte LIVE
(`unharvested-note.json`: neu=2 — exakt die Köder #40/#41, NiC #42 korrekt nicht gezählt).

## R-48 — Dry-Run + unmapped PBI = zwangsläufige MaxAttempts-Sackgasse; Steward meldete den Fehlschlag als „fertig ✓" (13.08., Testplan Block F) — ✅ GEFIXT

**Befund (Lauf `20260813_131946_573041`, Klärungs-Kreis PBI-042):** Wahrheits-Teil sauber (pbi-gate interactive,
Alignments griffen, PBI-042→active), aber der Forward endete `gatePass:false, MaxAttemptsReached`. Ursache
(`GithubForwardAgentRunner`): **`if (ctx.DryRun) return [];`** — im Dry-Run wurde die Agent-Bahn KOMPLETT
übersprungen ⇒ 0 Ops für das unmapped PBI ⇒ das eigene Gate riss (`PBI_NOT_ADDRESSED`) ⇒ Repair (auch leer) ⇒
MaxAttemptsReached. **Jeder Dry-Lauf mit unmapped aktivem PBI endete so** — nie zuvor sichtbar, weil PBI-010/012
gemappt waren (deterministischer Pfad). Widerspruch zur eigenen Regel („der Plan ist der Zweck des Dry-Runs",
R-27-Kommentar). **Zweiter Fund:** `read_run_report` las nur Apply-Reports → der Checker-Fehlschlag war im
Bericht UNSICHTBAR, der Steward meldete „fertig, geliefert ✓". **Fixes:** ① Dry-Run plant jetzt ALLES —
unmapped ⇒ deterministischer CREATE-Vorschlag über die geteilte R-27-Naht (`BuildCreateOps` mit ehrlicher
Dry-Run-Rationale/Evidenz, 0 LLM, gate-konform); Write bleibt übersprungen; Real-Lauf (execute:true) unverändert
Agent-Dedup. ② `read_run_report` liest zusätzlich `github-forward-summary.json` (Stage `forwardPlan`:
gatePass/finalDecision) + Prompt-Pflicht „Stufen-Fehlschlag NIE als Voll-Erfolg melden". 2 Tests (**538**) ·
**Live-Beweis** (gleiches Szenario, dry): `ops=1 gate=pass attempt 1 finalDecision=Pass` (Run `20260813_134434`).
Smoke 14/0 (13/1-Artefakt = Kampagnen-Gates interactive, mit clean-Gates bewiesen).

## R-47 — pipeline-full Forward-Apply ignorierte `fullworkflow.tokenEnv` → falscher Token (GITHUB_TEST_TOKEN) beim echten Write (11.08., reproject Live-Test) — ✅ GEFIXT

**Befund (Runs `20260811_155913`/`162247`/`162810`, alle 403/404 am Write trotz nachweislich schreibberechtigtem
`GITHUB_AGENTIC_REFACTOR_TOKEN`):** der Graph-Apply-Knoten wurde als `new GithubForwardApplyExecutor(run, repoRoot,
githubOutDir, null, null)` gebaut — **`tokenEnv = null`**. Im Apply fällt `ResolveToken(null)` (`GithubForwardApply.cs:213`)
dann auf **`GITHUB_TEST_TOKEN`** zurück — einen ANDEREN Token, der den (privaten) Ziel-Repo nicht schreiben/sehen darf.
**Diagnose-Kern:** der **Snapshot-Read** nutzt `fw.TokenEnv` (GITHUB_AGENTIC_REFACTOR_TOKEN) → 39 Issues gelesen ✓; der
**Write** nutzte nie diesen Token → 403 (public) / 404 (private). Die Token-Rechte-Fixes des Autors gingen ins Leere,
weil der Write-Pfad den konfigurierten Token gar nicht benutzte. Definitiv bewiesen: identischer `curl PATCH issues/12`
mit `$GITHUB_AGENTIC_REFACTOR_TOKEN` → **200**, Pipeline mit demselben `.env` → 404. **Fix:** `PipelineFullRunner`
reicht `fw.Repo`/`fw.TokenEnv` an den Apply-Knoten durch (`GithubForwardApplyExecutor(run, repoRoot, githubOutDir,
fw.Repo, fw.TokenEnv)`). Betraf JEDEN pipeline-full-Real-Write (operational/clarify/reproject) — nur nie zuvor mit
execute:true+privatem Repo gefahren, daher erst jetzt sichtbar. Build 0 · 528 Tests. **Betriebs-Lehre:** nach
`.env`-/Token-Änderung Steward NEU starten (`DotNetEnv` lädt `.env` clobbernd nur beim Prozess-Start; `resume` im
selben Prozess nutzt den eingefrorenen Token).

## R-46 — Real-Write scheiterte an Token-Rechten (403); Re-Projektions-Loop am externen Rand nicht geschlossen (11.08., Steward Real-Write-Test PBI-012) — ✅ BAUSTEIN ① LIVE-BELEGT (13.08.) / ② OFFEN

> **✅ LIVE-BELEG 13.08. (Testplan Block R):** erster GRÜNER Real-Write durch den durablen Graphen — Run
> `20260813_104957_d3a4b2`: run_reproject (Steward, direkt) → forward-gate → per-Op „nur gh#12 apply, 38 skip"
> → resume → **`executed=true success=TRUE, updated:1`** (gh#12 = PBI-012-Klärung live), Stempel gesetzt, Core
> unverändert, Kangal 0/0. **Voll-Stempel** Run `20260813_105912_40fd4d`: alle 39 apply → `updated:39, failed:0`,
> **39/39 Mappings gestempelt → Drift-Sperre flächendeckend scharf**, Bodies = kanonische Projektion. Damit ist
> der Recovery-Loop Ende-zu-Ende bewiesen (inkl. R-47-Fix im echten Write-Pfad). execute danach zurück auf false.
> **+ In-Sync-Erkennung (13.08., Autor-Fund):** der Seed schlug für JEDE gemappte PBI ewig UPDATE vor (kein
> Inhalts-Diff → Plan kein Drift-Report, idempotenter Write-Churn). Fix an der geteilten Seed-Naht: Drift==None ∧
> Render-Hash==Stempel ⇒ **NoChange** (Unknown heilt weiter, HumanEdited sperrt weiter); 4 Wächter-Tests (533) ·
> Live-Beweis `20260813_121353_3ed653`: **39× NO_CHANGE, 0 UPDATE** — wirkt in ALLEN Forwards (Zwei-Bahnen).

> **✅ Baustein ① GEBAUT (11.08.) — ONLY-STEWARD Re-Projektion.** `run_reproject` / `pipeline-full run --reproject`:
> der `ReprojectEntryExecutor` leitet das Sync-Delta AUS DEM CORE ab (`CoreViews.GithubSync`, **gemappt-only**,
> deterministisch — KEIN Run-Ordner, kein L4-all-vs-all) und speist den vorhandenen Forward-Schwanz (Snapshot →
> Maker → github-forward-gate HUMAN → Apply). Truth-first, durabel, chat-gegatet. Statt der ursprünglich skizzierten
> Dateinamen-Naht (die den Run-Ordner gelesen hätte — vom Autor als Chronik-Kopplung verworfen). Build 0 · **528 Tests**
> (Delta-aus-Core-Wächter + Routing + Seil) · Graph `--dry-run` Build()-bar. Volle Doku:
> `steward/reprojektion-only-steward.md`. **Offen:** echter Live-Write über den Steward (Token `newToken_11.08` gesetzt)
> · **Baustein ② (Retry-Klassifikation** transient/permanent + Backoff) als eigener Robustheits-Slice.



**Beleg-Lauf `20260811_143113_110e07`** (Klärung PBI-012 über `run_clarify_via_graph`, execute:true, repo/token AN,
pbi-gate + github-forward-gate interactive): Core-Write ✅ (PBI-012 verfeinert + `active`, raus aus needs_clarify),
aber `github-forward-apply` → **`GitHub API 403 Forbidden: Resource not accessible by personal access token`**
(Fine-grained Token ohne `Issues: write`). **Kein Bug:** der Graph verbuchte den fehlgeschlagenen Op als `failed`
und emittierte den Report als terminalen Output — „PIPELINE FERTIG (Forward): success=False", exit 0 (R-18-Prinzip:
laut gemeldet, nicht still geendet). Artefakte: `07-github/applied/github-forward-apply-report.json` (op-0 UPDATE_ISSUE
#12 failed) · `07-github/github-forward-plan.json` + `github-forward-decisions.json` (akzeptiert).

**Kern-Fund (Architektur — code-verifiziert):** unsere **Durability ist Pause/Resume für MENSCHLICHE Gates, nicht
Fehler-Durability.** Ein fehlgeschlagener externer Write ist keine Pause → kein resumebarer Punkt (`pointer.json`
weg nach Abschluss). Und die **Re-Projektion nach externem Fehler ist nicht als *eine* Aktion aufrufbar** — drei
Nähte offen:
1. **Steward = nur Inbound.** GitHub-Tools alle Ernte-Richtung (`pull_github_snapshot`, `run_github_inbound`,
   `run_github_reverse`, `run_pipeline_from_github`, …) — **kein Outbound-Re-Forward/Re-Apply**.
2. **Standalone-Apply passt fast:** `github-forward-apply <dir>` akzeptiert einen expliziten Plan-Dir (`ResolvePlanDir`),
   findet im pipeline-full-Ordner den Plan — sucht aber `human-decisions.json`, während die pipeline-full-Stufe die
   **typgleiche** Datei (`GithubForwardDecisionsFile`: runId/reviewer/decisions[opId,decision,reason]) unter
   `github-forward-decisions.json` schreibt. Nur der Dateiname trennt die zwei Forward-Welten.
3. **Apply retryt/klassifiziert nicht** (`GithubForwardApply.cs:171` per-Op try/catch → `failed`): keine
   transient-vs-permanent-Unterscheidung, kein Backoff. Der 403 wurde **korrekt NICHT** geretryt (permanent) — aber
   transiente Fehler (429/5xx/Timeout) eben auch nicht. (Der Maker/Checker-Loop retryt schon: `maxAttempts=2` — aber
   für die PLAN-Erzeugung bei Gate-Fehler, nicht für den HTTP-Write.)

**Einordnung:** nichts verloren/beschädigt — Wahrheit sicher (Core), GitHub = Projektion, **einen Update hinterher**.
Lücke = *Ergonomie/Vollständigkeit der Recovery am externen Rand*, nicht Korrektheit.

**Lösung (general, 2 Bausteine — noch NICHT gebaut):**
- **① Re-Projektion als idempotente Erstklasse-Aktion:** `ApplyFromPlanDirAsync` akzeptiert die Decisions-Datei unter
  BEIDEN Namen (vereint Standalone- + pipeline-full-Welt an der geteilten Apply-Naht) + dünner Steward-Outbound-Tool
  `retry_forward` (Muster wie `run_clarify_via_graph`). → schließt den Loop für Graph **und** Steward. Generell: *jede*
  externe Projektion, die scheitert → aus der Wahrheit neu projizieren (idempotent, weil Wahrheit sicher).
- **② Fehler-Klassifikation + bounded Retry** am Write-Rand: transient → Retry+Backoff; permanent → laut stoppen (wie
  jetzt). = Robustheits-Metrik.
- *(③ Verworfen als Over-Engineering: den fehlgeschlagenen Write als durablen Pause-Punkt modellieren — Baustein ①
  gibt dieselbe Recovery billiger, weil Wahrheit-zuerst die Transaktion schon garantiert.)*

**Sofort-Zustand (11.08.):** Core korrekt (PBI-012 `active`, **nicht** im Checkpoint `state/core-ckpt-vor-realwrite`);
run-config Real-Write-Schalter **noch AN** (repo/tokenEnv/execute:true/gates interactive) — nach Fix+Retry zurück auf
false. **Nächster Schritt:** Token `Issues: write` fixen → Retry über Baustein ① (dann UPDATE gh#12). Voller Diskurs:
`steward/graph-entry-vs-werkbank.md` + `aufgefallen.md`.

## B-Härtung (07.08., Steward-Vorlauf B1/B2) — R-16 + R-10-Runner ✅ GEFIXT
**R-16 → GEFIXT:** `GithubSnapshotGuard` (geteilte Naht): „latest"-Snapshots werden gegen den Repo-Stempel der
Schwester-Summary geprüft — fremd (`SNAPSHOT_REPO_MISMATCH`) oder ungestempelt (`SNAPSHOT_UNSTAMPED`) ⇒ LAUTE
Ablehnung mit Neu-Zieh-Hinweis. Verdrahtet in beiden Forward-Bahnen (CLI `github-forward` + `github-forward-hitl`,
je `--repo`-Kontext); die Pipeline zieht ohnehin FRISCH je Lauf (kein Latest-Gift); Grenze dokumentiert:
`github-reverse` hat keinen `--repo`-Kontext (kein Mapping-Write-Pfad; bei Bedarf nachrüsten). 1 Test.
**R-10-Härtung → ✅:** RecipeRunner behandelt „-1 items" (Artefakt fehlt/unlesbar = Fan-out lief nicht durch)
jetzt als FEHLER (Exit 5 + `RECIPE_ARTIFACTS_MISSING`-Event) statt stiller Konsolen-Notiz — silent-cap-Verstoß zu.
428 Tests · Smoke 14/0.

## B4-Forward-Haken-Lauf (07.08., Run `20260807_135142_d35f95`) — Live-Trias ✅
Scharfer Execute ans Test-Repo (2× UPDATE, failed=0), Issue #1 per API verifiziert:
**R-23 ✅ LIVE** (v2-Body: Statement+AK+Reqs+Sync-Fuß statt Stub) · **R-30 ✅ LIVE** (Labels unangetastet) ·
**R-15 ✅ LIVE** (det. Plan-Familie trägt den Write-Pfad). R-16-Guard war im Pfad aktiv (frischer
gestempelter Snapshot). Details: steward/steward-iteration-notes.md I-2.

## R-75 — Baselines-Stufe crasht statt LAUT zu enden (09.09.; Korrektur vervollständigt und gezielt getestet 11.09.) — ✅ GEFIXT
- **Beleg-Lauf:** `20260909_190157_506d47` (Mini-Meeting, Front-Pfad): Baseline-Checker meldete korrekt
  `STAGE_BASELINES_NEEDS_HUMAN (decision=HumanReview, open-questions)` → der vorgesehene laute Ausstieg
  (`YieldOutputAsync`-Klartext, SP2/R-32-Fix vom 05.08.) warf stattdessen
  „Cannot output object of type String. Expecting one of []" → `workflow error`, exit=3.
- **Ursache und Korrektur:** Zwei Ebenen des MAF-Vertrags waren unvollständig: Die Stufe war zunächst nicht mit `WithOutputFrom(front.Baselines)` als Workflow-Ausgang registriert; zusätzlich fehlte `[YieldsOutput(typeof(string))]` am Executor selbst. Die Registrierung vom 09.09. allein reichte nicht. Ein gezielter Lauf am 11.09. reproduzierte denselben Typfehler trotz vorhandener Registrierung.
- **Absicherung 11.09.:** Ausgabetyp ergänzt; tatsächlichen Baseline-Executor unter MAF mit fehlendem Ledger sowie vorbereiteten HumanReview-/MaxIterationsReached-Rezept-Ergebnissen ausgeführt. Alle drei Pfade liefern terminalen Text ohne Fehlerereignis oder Delta-Weitergabe. Pass und der bestehende Fall eines fehlenden Reports behalten die Weiterleitung. Der optionale interne Rezept-Testzugang verwendet standardmäßig unverändert `RecipeRunner.ExecuteAsync`; der produktive Aufrufer übergibt keinen Ersatz.
- **Umfang:** Einmaliger Build (0 Warnungen/Fehler), komplette Testsuite **701/701**, Offline-Smoke **14/0** ohne Neubuild. Smoke mit kopiertem Core (237 Items), leerem GitHub-Ziel und deaktivierter Ausführung; Core und Konfiguration bytegenau restauriert. Smoke prüft `--from-delta`, die R-75-Stufentests prüfen den sonst übersprungenen Fehlerzweig. Kein vollständiger neuer Transkript-E2E-Lauf; der historische A10-Lauf bleibt unverändert.
- **Kontext des Funds:** entdeckt bei der A10-Verifikation (Facetten-Refine als Ein-Graph-Knoten
  `PipelineAdjudicationRefine` zwischen Adjudikations-Apply und Baselines, geteilte Naht
  `AdjudicationRefine` mit der CLI-Bahn; Beleg-Events `STAGE_ADJUDICATION_REFINE_DONE`
  pendingBefore=1/refined=1/stillPending=0 in BEIDEN Läufen; Gap-Claim `ADJ-GAP-AU-0001` real facettiert).
  Einordnung (Kollege): Lauf 2 = **gezielter Integrationslauf bis zum ingest-gate** — Beleg für „Funktion
  wird im Gesamtworkflow aufgerufen", KEIN Beleg für Recall-Verbesserung oder Übernahme einer normativen
  Lücke bis Core/GitHub. Der Lauf steht bewusst PAUSIERT (keine Core-Wirkung) — behalten oder verwerfen.
  **Nachtrag-Fund (Kollege, sofort gefixt):** der erste Kern-Stand überschrieb `ConsumableLedger.PendingCount`
  (= Zähler VERTAGTER Positionen/defer) mit dem Facetten-Stand — zwei getrennte pending-Begriffe vermischt;
  Fix: Feld bleibt unverändert, Facetten-Stand nur via PendingBefore/StillPending; Regressionstest
  „Gap-Claim + defer" ergänzt (693/693). Die frühere Lesart „pendingCount-Nachzug = CLI-Konsistenzfix"
  war FALSCH und ist zurückgenommen.

## R-76 / B-20 — Bindestrich-Rolle wird nicht als Sprecherwechsel erkannt (11.09.) — ✅ GEFIXT

- **Beleg:** F1-Quelle `meeting-4-ux-block-l.txt`, Rolle `Angehörigen-Vertreterin:`. Historische Unit AU-0012 im Ledgerlauf `20260818_084712_567daf` enthält den Sprecherwechsel als Text unter Metadatum „Leitung“; bereits der Extraktionskandidat enthält das abweichende Präfix. Gezielter Dreiturn-Test gegen unveränderten Parser: nur zwei Turns.
- **Fix:** `TranscriptSegmenter` erlaubt im Doppelpunktformat zusätzlich Bindestriche zwischen Buchstabenfolgen. Keine Promptänderung, keine neue Freitext-Zitaterzeugung. Acht neue Testfälle insgesamt für R-75/B-20; vollständige Suite 701/701 und Offline-Smoke 14/0.
- **Reichweite:** F1 erhält bei neuer Segmentierung 17 statt 13 Units, Besuchsplanung AU-0016. Vollständig serialisierte Units der übrigen fünf geprüften Transkripte einschließlich beider W2-Eingaben sind unverändert. Positionale IDs können sich in betroffenen neuen Läufen verschieben; historische Artefakte und Auswertungen bleiben unverändert. Keine allgemeine Garantie für beliebige Sprecherformate oder Zitattreue.

Technischer Übernahmebericht für R-75/R-76: [Stand, Testprotokolle und Thesis-Nachtrag](../Writing/claude-writing/Technische-Uebernahme-R75-B20-2026-09-11.md).
