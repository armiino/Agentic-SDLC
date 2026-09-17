# Steward — Iteration Notes (chronologisches Bau-Log)

> Status: LEBEND — je Iteration: Was/Warum/Änderung/Verifikation + Funde. Neueste unten anfügen.
> Plan-Referenz: `README.md` (Fahrplan) · `steward-vorlauf.md` (A/B/K) · `k-entscheidungen.md` (⚖ + Quellen).

---

## I-0 · 07.08.2026 — Startbahn steht (Vorlauf A+B+K)

- **A1–A2, K1–K4 ⚖:** Scope (Option 1 als Start, alle Features als Slices) · MAF-nativ · Endbild Graph vs.
  Agent · K-Befunde quellenbasiert (Session-API offiziell · long-running = Anwendungsschicht [#4265] ·
  ApprovalRequiredAIFunction [#5189 gepinnt]).
- **B1 R-16 ✅** (`GithubSnapshotGuard`, laute Ablehnung fremder/ungestempelter Snapshots, 1 Test) ·
  **B2 R-10 ✅** (RecipeRunner: „-1" ⇒ Exit 5 + Event) · **B3 ✅** (READMEs 04–08 + gate-landkarte-Refresh).
- Verifikation: 428 Tests · Smoke 14/0.

## I-1 · 07.08.2026 — Mini-Spike: Session-Persistenz + Approval-Roundtrip (#5189-Kante)

**✅ ERLEDIGT — 3/3 grün** (`StewardFoundationSpikeTests`, LLM-frei am echten `ChatClientAgent` via
Fake-IChatClient). Ergebnisse: (a) Session-Persist-Roundtrip über die offizielle API funktioniert —
**API-Realität auf unserem Pin: `SerializeSessionAsync`/`DeserializeSessionAsync`** (die Doku nennt schon
das neuere synchrone `SerializeSession` — bei Upgrade angleichen). (b) Approval-Roundtrip exakt wie
dokumentiert: Run endet mit Request OHNE Ausführung, `CreateResponse(true)` ⇒ genau EINE Ausführung.
**Fund #1 (Version):** in M.E.AI **10.6.0 heißen die Typen `ToolApprovalRequestContent`/
`ToolApprovalResponseContent`** (bis 10.3: FunctionApproval… — deckt sich mit dem #5189-Titel).
(c) **#5189-Kante GRÜN auf unserem Stand:** Approval-Antwort NACH Serialize/Deserialize führt das Tool
korrekt genau einmal aus ⇒ **C5 ist Neustart-sicher baubar**; der Pinning-Test bleibt als
Upgrade-Stolperdraht (bricht er nach einem Paket-Update, ist #5189 zurück).
Ziel: die zwei MAF-Fundament-Mechaniken des Stewards LLM-frei am echten `ChatClientAgent` beweisen,
BEVOR C1 darauf baut: (a) `CreateSessionAsync` → Runs → `SerializeSession` → Datei → `DeserializeSessionAsync`
→ Folge-Run mit erhaltenem Verlauf; (b) `ApprovalRequiredAIFunction`: Run endet mit
`FunctionApprovalRequestContent` OHNE Tool-Ausführung → Zustimmung zurückgeben → Tool läuft;
(c) die #5189-Kante: Serialize/Deserialize ZWISCHEN Approval-Anfrage und -Antwort — Verhalten PINNEN.
Technik: Fake-`IChatClient` (skriptete FunctionCalls/Texte) — kein LLM, echte Agent-Pipeline.

## I-2 · 07.08.2026 — B4: Forward-Haken-Lauf (scharfer Execute ans Test-Repo)

**✅ ERLEDIGT — EXECUTED updated=2 failed=0** (Run `20260807_135142_d35f95`; Snapshot frisch+gestempelt
`135044`-Ära, Guard aktiv im Pfad). Weg: synthetisches 2-PBI-Sync-Delta (`runs/pbi-update/_b4-haken-20260807`,
Fixture-Muster wie _initial-sync) → det. UPDATE-Plan → accept-all → **--execute** ans Test-Repo.
**Live-verifiziert an Issue #1 (API-Read):** Body v2 beginnt mit dem STATEMENT, enthält
Akzeptanzkriterien-Sektion + Reqs-Zeile + Sync-Fußzeile ⇒ **R-23-Live-Beweis** (Stub-Regression tot) ·
Labels blieben `initial-sync, pbi` ⇒ **R-30-Live-Beweis** (kein Label-Ersatz) · deterministische
Plan-Familie ⇒ **R-15 live**. Damit ist die EINZIGE seit R-11 ungetestete Strecke (reale Writes) zu —
**Pipeline-Haken vollständig gesetzt.** (arch-Sektionen im Body = weiterhin Rest-Risiko (c), erst nach
scharfem Klassifikations-Apply — Autor-Akt.)
Ziel: die einzige seit R-11 ungetestete Strecke (reale GitHub-Writes) + Live-Trias R-15/R-23/R-30 in EINEM
kleinen Lauf. Weg: frischer Snapshot (`--repo armiino/Agentic-GitHub-refactor`, Guard aktiv!) → forward
(det. Plan, 1–2 UPDATE-Ops) → Review → `--execute` → Issue-Inhalte v2 (Statement/AK) real verifizieren.

## I-3 · 07.08.2026 — A3: Workflow-Tool-Vertrag (`PipelineRunStatusReader`)

**WARUM nötig:** Der Steward braucht EINE typisierte Sprache über die Läufe („wo steht Lauf X, was ist als
Nächstes zu tun?"). Diese Wahrheit existierte, aber nur als Konsolen-Prosa an ZWEI Stellen (status-CLI +
Pause-Meldung, je eigener Code über pointer/events) — ein Steward-Tool hätte sie ein DRITTES Mal
nachgebaut (Drift-Garantie).

**WAS gebaut (nicht additiv — Kern-Hebung):** `PipelineRunStatusReader` (08-pipeline) = der geteilte Kern:
`PipelineRunStatus` (RunId · State Paused/Finished/NotPausedNotFinished [ehrlich: Artefakte können
laufend/abgebrochen nicht unterscheiden] · PausedGate+CheckpointId+Seit · LastPipelineEvent ·
**NextRequiredAction** · Artefakt-Pfade) + `ReadAsync`/`ReadPausedAsync` + **`NextRequiredAction(gate,runId)`
= die EINE Gate→Anleitung-Quelle** (die ReviewHints WOHNEN jetzt hier; der Runner delegiert nur noch —
Duplikat gelöscht). `pipeline-full status` ist reiner RENDERER über dem Kern geworden.

**WAS wir davon haben:** (1) Das C1-Tool `get_run_status` ist eine Ein-Zeilen-AIFunction über `ReadAsync` —
identische Wahrheit wie CLI und Pause-Meldung, für immer drift-frei. (2) `NextRequiredAction` ist genau das
Feld, mit dem der Steward dem Autor „was jetzt?" erklärt (und später C5 die Gate-Vorlage baut). (3) Der
Vertrag ist LLM-frei getestet (2 Tests: Paused mit Hint-Identität · Finished/Unklar/fehlend) und live am
CLI verifiziert (findet den echten pausierten Lauf `115016`). Verifikation: 433 Tests · Smoke 14/0.

## I-4 · 07.08.2026 — C1a: Read-/Status-Tools (erste Datei der Steward-SCHICHT)

**Move (Kollegen-Schnitt C1a–c übernommen, im Vorlauf festgeschrieben):** C1a = read-only zuerst.
**Gebaut:** `AgenticSdlc.Host/Steward/StewardReadTools.cs` — die NEUE Schicht (eigener Ordner, Haken-Prinzip):
`get_run_status` (= Ein-Zeiler über A3 `ReadAsync`) · `list_paused_runs` (= `ReadPausedAsync`) ·
`get_core_overview` (Zählung je Typ + Parkplatz + Kangal-Lage, read-only über die ICoreRepository-Naht).
KEIN Tool schreibt. **Vertrags-Fix am A3-Typ:** `PipelineRunState` serialisiert jetzt als STRING
(LLM-/menschen-lesbar) — am Vertrag gefixt, nicht im Tool umgangen. **Bewusst NICHT geändert:** die
Gate-Hints bleiben verbatim die des Runners (pbi-/ingest-gate hatten nie eigene Zeilen → Default; ein
Hint-Ausbau wäre eine eigene kleine Verbesserung, kein C1a-Scope). LLM-freie Tests invoken die AIFunctions
direkt (A3-Feld-Identität · RUN_NOT_FOUND laut · Read-only-Beweis: Core-Datei byte-gleich nach Overview).
Verifikation: 435 Tests · Smoke 14/0. ▶ C1b (Agent-Hülle + Session) → C1c (startende Tools, ApprovalRequired).

## I-5 · 07.08.2026 — C1b: die Agent-Hülle — DER STEWARD SPRICHT

**Gebaut:** `Steward/StewardChatRunner.cs` — `BuildAgent` (EINE Bau-Naht, Client injizierbar für Tests):
ChatClientAgent + deutscher System-Prompt (`Prompts/steward/StewardAgent/StewardAgent1.txt`, R-6;
Rollen-Leitplanken „liest/erklärt/führt — schreibt nie, beantwortet keine Gates") + C1a-Tool-Katalog +
DIESELBEN Observability-Nähte wie alle Stufen-Agenten (AgentChatPipelineBuilder + ToolCallLogger; Logs je
Sitzung unter `runs/steward/<id>/`). Session-Persistenz über die OFFIZIELLE API nach jedem Zug
(`state/steward/sessions/<name>.json`, K1/I-1-Muster). CLI: `steward [--session name] [--once "Frage"]`
(registriert im Host-Dispatch, R1-Muster). LLM-freier Test: Hülle trägt 3 Tools + Session überlebt den
Datei-Roundtrip AM ECHTEN BAU (nicht nur am Spike).

**ERSTE ECHTE ANTWORT (Live-Probe, 1 Chat-Zug, Sitzung `live-probe`, Logs `runs/steward/20260807_143107`):**
Der Steward fand via `list_paused_runs`+`get_core_overview` den pausierten Lauf `115016` (decision-gate),
zitierte die `nextRequiredAction` WÖRTLICH (A3-Vertrag als Sprache — genau das Design) und antwortete
deutsch/klartext. Session-Datei angelegt. Verifikation: 436 Tests · Smoke 14/0.
▶ C1c: startende Tools (run_pipeline_full/resume_run, ApprovalRequired, start-async, A3-Rückgabe).

## I-6 · 07.08.2026 — C1c: startende Tools — ★ C1 KOMPLETT

**Gebaut:** (1) **runId-Naht am Runner** (`PipelineFullRunner.RunAsync(..., Action<string>? onRunId)`-Overload;
CLI unverändert — der Steward bekommt die runId SOFORT beim Start, K2 sauber statt Warte-Hack).
(2) `Steward/StewardRunTools`: `run_pipeline_full(deltaPath)` + `resume_run(runId, acceptAll)` — beide in
`ApprovalRequiredAIFunction` (K3), in-process start-async (Task im Hintergrund, Rückgabe sofort mit runId +
get_run_status-Hinweis); **STEWARD_BUSY**-Schutz (ein aktiver Lauf je Sitzung, lautes Ablehnen);
Startfehler LAUT (`RUN_START_FAILED` mit exitCode); Test-Naht = injizierbarer Runner (LLM-freie Tests:
Wrapping · runId-vor-Ende · BUSY/Frei-Zyklus · Fehler · resume-Args). (3) **Chat-Schleife führt den
K3-Approval-Roundtrip** (offizielles Muster: nach jedem Run offene `ToolApprovalRequestContent` prüfen →
Autor ja/nein → `CreateResponse` zurück); `--once` stimmt NIE stillschweigend zu (Anfrage überlebt die
Session-Persistenz — I-1c-Beweis — und wird interaktiv beantwortet).

**Fund #2 (live gefunden):** der C1b-Prompt verbot noch das Starten („MVP startet keine Läufe") — der
Steward rief das Tool schlicht NICHT (Governance-by-Prompt funktioniert!). Prompt auf C1c-Stand gezogen
(zustimmungspflichtiges Starten erlaubt, nie selbst zustimmen).

**Live-Probe (Ablehnungs-Pfad, 2 Chat-Züge, Sitzung `c1c-probe2`):** Steward rief `run_pipeline_full` →
System fragte „Zustimmen? (ja/nein)" → „nein" → Tool NICHT ausgeführt, kein Lauf entstand, der Steward
erklärte die Abweisung. Der Zustimmungs-Pfad ist unit-bewiesen (runId sofort) — ein echter Ja-Start ist
jederzeit per Chat möglich und braucht keinen Extra-Beweis-Lauf (Kosten).
Verifikation: **439 Tests · Smoke 14/0 · 0 Warnungen. ★ C1 (a+b+c) KOMPLETT.**

## I-7 · 07.08.2026 — C3: Read-Backend — der geteilte Lese-Werkzeugkasten (⚖ K5/K7)

**Gebaut:** (1) `05-core/CoreQueryTools` — der EINE Core-Lese-Kasten um die `ICoreRepository`-Naht
(DB-Swap-neutral): `get_core_overview` (aus StewardReadTools hierher umgezogen — ein Konzept, ein Ort),
`list_core_items` (itemType/query/includeSuperseded, Superseded per Default verborgen, Cap 50 mit
dropped-Zähler = kein stilles Kappen), `get_core_item` (Voll-Item inkl. Beziehungen BEIDER Richtungen mit
Gegenseiten-Text; Extern-Ziele ohne Core-Text = nur Id). (2) `07-tore/github/GithubSnapshotQueryTools` —
`search_github_issues` aus dem NEUESTEN Snapshot über den aus GithubReverseRunner GEHOBENEN geteilten
`GithubSnapshotLocator` (Zwei-Bahnen-Regel); jede Antwort trägt repository+snapshotUtc, alte/ungestempelte
Snapshots werden per staleNote LAUT (R-16-Bewusstsein im Lese-Pfad). (3) Steward-Montage = Rollen-Teilmenge
(8 Tools) + Prompt-Zeilen (Inhalts-Fragen; GitHub = Momentaufnahme, Frische nennen). (4) **Session-Wache**
(K6-Vorstufe, deterministisch): Startbanner zeigt Session-KB, warnt >150 KB mit Empfehlung neue Session.

**Live-Proben (--once, je 1 Zug):** „erste 3 PBIs?" → PBI-001/002/003 wörtlich zitiert ✅ · GitHub-Frage fand
**R-41** (Umlaut-Varianz + geratene OR-Syntax) → Fix an der Quelle (geteilte Match-Naht `shared/QueryText`
+ expliziter Tool-Vertrag) → Wiederholungs-Probe: `#39` „Schriftgröße" via Query „Schriftgroesse" ✅.
Verifikation: **445 Tests · Smoke 14/0 · 0 Warnungen.** Offen als M1: echte Memory-Modi (MAF-Reducer).

**I-7-Nachschliff (07.08. abends, aus Autor-Chat-Beobachtung):** Status-Fragen kosteten ~20 Einzel-`get_core_item`
(kein Status-Filter an der Liste) → `list_core_items` um `status`-Filter ergänzt (exakter Wert der
Status-Projektion) + Prompt-Regel „Status-Fragen = EIN Listen-Aufruf". Live-Beweis Session `c3-statusfilter`:
1 Tool-Call statt 20, alle 21 needs_clarify-PBIs. **446 Tests.**

## I-8 · 07.08.2026 — M1: Memory-Modi am offiziellen Reducer-Slot (⚖ K6) — ★ M1 KOMPLETT

**Spike zuerst (R-38-Methodik), 3/3 grün auf Anhieb:** E1 `MessageCountingChatReducer` wirkt als
Schiebefenster UND überlebt Serialize/Deserialize · E2 schwebende `ToolApprovalRequest` überlebt das
kleinste Fenster (N=2, nach 6 Überroll-Zügen) · E3 **Moduswechsel per Neustart ist tragfähig** (Session
ohne Reducer angelegt, unter Reducer-Config geöffnet: läuft, Fenster greift — die offizielle
„nicht reusen"-Warnung beißt auf unserem Stand nicht; Test = Pinning).
**Spike-Fund #1:** die gelieferten Reducer sind in M.E.AI 10.6 **EXPERIMENTELL (MEAI001)** — bewusster
Einsatz: Slot (`ChatClientAgentOptions.ChatHistoryProvider`) ist stabil, Implementierung gepinnt, lokales
dokumentiertes Suppress, Spike-Tests = Upgrade-Stolperdraht. Ehrlicher Feature-Matrix-Befund.

**Gebaut:** `BuildAgent(..., memory)` auf den Options-Weg umgestellt (EIN Bau-Pfad:
`ChatClientAgentOptions{ChatOptions.Instructions/Tools, ChatHistoryProvider}`); `CreateReducer`:
`count[:N]` (Default 40) → MessageCounting · `summarize` → SummarizingChatReducer über den PIPELINE-Client
(Verdichtungs-Calls laufen durch Logging/otel — kein unsichtbares LLM) · unbekannter Modus = LAUT Exit 2.
CLI: `--memory count[:N]|summarize` + `--fresh` (Bestand rotiert nach `<name>.prev.json`, nie still
gelöscht); Banner nennt Modus (summarize mit Kosten-Hinweis) + Rotations-Ziel. Modus = Start-Konfiguration
(K6-Detail); Web-Frontend füttert später dieselbe Naht.
**Live-Probe** `m1-probe` (`--memory count:6 --once`): Banner `memory=count:6`, Antwort korrekt (194 Items,
Kangal 0/0) inkl. proaktivem needs_clarify-Hinweis. **451 Tests · Smoke 14/0 · 0 Warnungen.**
**Autor-Wunsch (geparkt):** Memory-Modus auch über run-config steuerbar → M1b-Spur im README.

## I-9 · 08.08.2026 — C2a-1: `GithubIssueTemplate` — die Struktur-Naht steht (erster C2-Move)

**Vorab:** C2-Detail-Plan `c2-inbound-plan.md` §0–§17 in Autor+Kollegen-Diskussion gehärtet (Drift-Wächter
mit Hash-Stempel+Konvergenz · Template-Sektionen als det. Happy Path · Flächen-Leitregel Body/Kommentare/NiC ·
Spiegel-Invariante 1↔1 · Verarbeitungs-Gedächtnis · Ein-Graph-Endform --from-github · Fähigkeits-Ehrlichkeit ·
K11 MCP-Trennlinie · Feinschnitt C2a-1…4).

**C2a-1 gebaut (verhaltensneutral):** `GithubIssueBodySections` (war schon DIE eine Render-Quelle für
CREATE+UPDATE, E0.1c/E0.2/R-23) → **`GithubIssueTemplate`**: `Render` wörtlich übernommen + NEU `Parse`
als Gegenstück IM SELBEN Vertrag (formale Sektions-Marker EINMAL definiert; Sync-Fuß-Felder; alles
Unzuordenbare → `FreeText` = Ernte-Fläche, nie stiller Verlust). 2 Host- + 3 Test-Aufrufer umgestellt.
**Pinning doppelt:** Roundtrip `Parse(Render(x))==x` (Drift-Stolperdraht Ersteller↔Leser) + BYTE-Pinning
des R-23-Formats (jede bewusste Format-Änderung muss den Test anfassen = Render+Parse gemeinsam) ·
GithubForwardRerunTest unverändert grün = Verhaltensneutralität belegt.
**456 Tests · Smoke 14/0 · 0 Warnungen.** ▶ C2a-2: Hash-Stempel-Naht + GithubDriftCheck.

## I-10 · 08.08.2026 — C2a-2: Hash-Stempel-Naht + `GithubDriftCheck` (die eine Drift-Quelle)

**Gebaut:** (1) **Stempel am gated Apply:** die beiden ECHTEN Schreib-Zweige des Forward-Apply
(CREATE/UPDATE) hashen exakt das GESENDETE (Titel+Body) und tragen es als `projectedTitle/BodyHash` +
`projectedUtc` additiv in die Mapping-Metadata (B4-Naht; `GithubMappingOp` um optionale Hash-Felder
erweitert). **Preserve-Regel:** stempel-lose Link-Ops (COMMENT/LINK-Zweige) erhalten den bestehenden
Anker (MakeRelation-previous-Fallback — sonst hätte jeder Kommentar den Drift-Anker gelöscht!); Remap auf
ANDERES Issue verwirft bewusst (alter Stempel gehört zum alten Issue → Unknown). (2) **`GithubProjectionHash`:**
SHA-256/16 Hex über normalisiertem Text (\r\n→\n + hängende Leerzeilen — GitHub liefert Web-Edits mit \r\n;
nur ECHTE Inhalts-Änderung zählt als Drift). (3) **`GithubDriftCheck`** (geteilte Quelle für Forward-Sperre
C2a-3 UND Inbound-Detect C2a-4): misst gegen den LETZTEN SCHREIB-STEMPEL (nicht gegen den Core-Render —
der Konvergenz-Kern aus §7): `None` (Snapshot==Stempel) · `HumanEdited` (Ernte-Fall F1) · `Unknown`
(Alt-Mapping ohne Stempel → warnen, nicht blocken; erster Apply heilt).
**Tests:** Normalisierung · Stempel-Fluss durch Apply inkl. Preserve+Remap-Verwurf · drei Drift-Verdikte
inkl. \r\n-Snapshot-Variante am echten Template-Render. **459 Tests · Smoke 14/0 · 0 Warnungen.**
▶ C2a-3: Forward-Drift-Sperre (FlagDrift statt UpdateIssue) + 1↔1-Mapping-Wächter (Kangal).

## I-11 · 08.08.2026 — C2a-3: Forward-Drift-Sperre + Spiegel-Wächter I5b

**Gebaut:** (1) **Drift-Sperre am Forward-Seed** (die Verdrahtungs-Stelle hatte alles in der Hand: mapping
mit Stempeln + Snapshot-Issue): `HumanEdited` ⇒ `FlagDrift` STATT `UpdateIssue` — Rationale benennt Sperre
UND Ausweg („erst ernten oder bewusst auflösen"); `Unknown` (Alt-Mapping ohne Stempel) blockt NICHT,
sondern trägt den lauten Hinweis „dieser Write stempelt" (Konvergenz-Start, §7: erster Zyklus heilt).
(2) **I5b im Kangal** (Spiegel-Invariante §15, Gegenrichtung des bestehenden I5): ein Issue darf nur EIN
PBI spiegeln — Abbruch-Fehler wie I5, an der EINEN Prüf-Naht (S4). **Vorab am Produktiv-Core verifiziert:**
39 Mappings, 0 I5-/I5b-Verletzungen ⇒ scharf schalten gefahrlos; der „Echter Core passiert"-Test läuft mit.
**Tests:** Sperre (Kollegen-Testfall g) · CRLF-Snapshot ⇒ normales Update (kein Falsch-Alarm) ·
Unknown-Hinweis · I5b-Fehler. **463 Tests · Smoke 14/0 · 0 Warnungen.**
Bewusst offen (dokumentiert): explizite „trotzdem überschreiben"-Gate-Entscheidung kommt, wenn die Ernte
existiert (bis dahin gibt es keine gestempelt+editierten Issues, die man freigeben müsste).
▶ C2a-4: minimaler Inbound-Detect (Sektions-Mapping §8, NiC §9/F6, Report).

## I-12 · 08.08.2026 — C2a-4: der deterministische Ernte-Detektor + Zwischenbahn `github-inbound`

**Gebaut:** (1) `inbound/GithubInboundDetect` (LLM-frei, urteilt nicht, schreibt nicht): klassifiziert jeden
Snapshot-Issue — **F6 NiC** (Titel-Präfix `NiC:` ODER Label `not-in-core`, zuerst geprüft) · **F2** ungemappt+
offen (mit geparstem Body; geschlossen+ungemappt = Skip mit Grund) · gemappt via geteilte `GithubDriftCheck`:
None→unchanged · Unknown→UNKNOWN_STAMP-Warn-Fund · **F1 HumanEdited** mit deterministischem Sektions-Diff
gegen die CoreViews-Sync-Sicht (Titel/Statement/AK neu+entfernt) + JEDE Freitext-Zeile benannt (Leit-Testfall
a: nichts geht stillschweigend verloren). Kein stilles Verwerfen: jedes Issue hat Kategorie oder Skip-Grund.
(2) Zwischenbahn-CLI `github-inbound [--issues]` (§2): neuester Snapshot via geteiltem Locator,
**Frische-Schutz LAUT** (>24h ⇒ SNAPSHOT_STALE Exit 2), Report → `runs/github-inbound/<id>/plan/
harvest-report.json` + Konsolen-Funnel. **Echt-Probe** (Run `20260808_104706_292f97`, Snapshot 21h):
39 Issues → unchanged=0 · drift=0 · neu=0 · nic=0 · **unstamped=39** · skipped=0 — das EHRLICHE Vorher-Bild
(Stempel existieren erst nach dem ersten ausgeführten Forward-Write; genau die §7-Konvergenz-Startlage).
**466 Tests · Smoke 14/0 · 0 Warnungen.** Fix unterwegs: PbiStatus ist seit §5 kein Typ mehr (Readiness=string).
**★ C2a KOMPLETT (1–4).** ▶ C2b: InboundAgent-Fallback + Delta-Ausgabe.

## I-13 · 08.08.2026 — C2b: InboundAgent-Fallback + Delta-Ausgabe — die Ernte kann durch die Tore

**Gebaut:** (1) `GithubInboundDraftTools` (Haus-Muster PbiAlignTools: get_inbound_finds + save_inbound_drafts
SAVE-ONCE mit harter Validierung — jeder Fund GENAU ein Draft, Dispositions-Vokabular
requirement|architecture|open_question|noise, draftText-Pflicht außer noise; Fehler gehen als Text an den
Agenten zurück = Retry-fähig). (2) `GithubInboundDraftAgent` (A2+ per K9; kein Save = LAUTE Exception statt
leerem Delta) + deutscher Prompt `phase2_evidence/GithubInboundAgent/GithubInboundAgent1` (belegtreu, nichts
erfinden, R-6). (3) `GithubInboundDeltaBuilder` (deterministisch): requirement/architecture-Drafts →
Delta-Items im Meeting-Ketten-Vertrag (itemType=Aspekt-Profil-Match; Metadata trägt die ANKER:
githubIssueNumber/Url für §9-Adoption + harvestedTitle/BodyHash für den §11-Stempel am Apply + mappedPbiId
bei F1; sources/provenance je Issue `harvested_from`). **Bewusste Grenze (kein stiller Verlust):**
open_question-Drafts gehen NICHT ins Delta (nur 2 Aspekt-Profile existieren; die 9g-Fragen-Schiene ist ein
eigener Anschluss = C2c/9i) — sie bleiben SICHTBAR in inbound-drafts.json + Konsole. (4) Runner `--draft` =
der bewusste LLM-Schritt (Default bleibt LLM-frei; otel/ToolCallLogger wie alle Stufen).
**Fund unterwegs:** §5-Guard wirkt wie designed — Delta-Items ohne Status-Achsen werfen beim Serialisieren
(ReadStatus laut) → Builder setzt `baseline` wie die Meeting-Kette.
**Mini-Live-Probe** (Run `110150_ff340d`, Fixture-Snapshot vs. echter Core read-only): #900 → F2 → Agent-Draft
requirement (belegtreu inkl. Offline-Randbedingung) → Delta-Item GH-900 mit Ankern · #901 `NiC:` → F6, nie
geerntet · Konsole nennt den nächsten Schritt (--from-delta). **468 Tests · Smoke 14/0 · 0 Warnungen.**
▶ C2c: --from-github-Eingang im Ein-Graph + Steward-Seile + Fragen-Schienen-Anschluss + End-Probe.

## I-14 · 08.08.2026 — C2c: Ein-Graph-Eingang + Steward-Seile — ★ C2 KOMPLETT (Meilenstein)

**Gebaut:** (1) Ernte-Kern in die geteilte Naht `GithubInboundHarvest` gehoben (Zwei-Bahnen: Zwischenbahn-CLI
UND Ein-Graph nutzen DIESELBE Quelle; createOtel-Schalter gegen Doppel-Spans). (2) **`pipeline-full run
--from-github [--issues]`** = dritter Eingang (§13): Ernte läuft NACH Run-Erzeugung im Faden (Logs/otel im
Lauf, Ordner 00-github-inbound/), Delta → identischer Graph wie --from-delta; entryPoint=github im Config.
(3) **Steward-Seile** (alle ApprovalRequired, Aux-Runner-Test-Naht): `run_pipeline_from_github` (zeigt auf
den Ein-Graph-Eingang, BUSY-geschützt via geteiltem StartPipelineAsync) · `pull_github_snapshot` ·
`run_github_inbound(draft)` · `run_github_reverse` (⚖③ Beifang) — Steward jetzt 12 Tools, Prompt nachgezogen.
**Bewusst geparkt:** 9i-Fragen-Schienen-Anschluss (Mint hängt an der Ledger-Fragen-Projektion, nicht am
Delta) — open_question-Drafts bleiben sichtbar; eigene Spur.
**End-Probe + R-42:** Der --from-github-Lauf `111048_d5ecd0` bewies den VOLLEN Kreis (Ernte→Draft→Delta→
Tor 1→REQ-78→PBI-044→Forward-dryRun→FERTIG, Kangal grün, Anker in Metadata) — lief aber DURCH statt zu
pausieren: run-config trug ALLE Gates auf accept-all (Experiment-Reste; Probe ohne Policy-Check+Backup =
mein Prozess-Fehler). Core via git restauriert (194 byte-identisch); **Härtung:** lautes
⚠ EXPERIMENT-MODUS-Banner am Lauf-Start, wenn irgendein Gate nicht interactive ist (R-42 im Runbook).
**470 Tests · Smoke 14/0 · 0 Warnungen · Core clean. ★ C2 (a1–4/b/c) = AUTOR-COMMIT-MEILENSTEIN.**

## I-15 · 09.08.2026 — C4-Plan ⚖-fertig (§8: Trigger-Naht-Leitplanke) + C4a ✅

C4-Plan §0–§8 (alle ⚖ angenommen; Kollegen-Leitplanke: Autor-Antworten NIE als Fake-REQ — Trigger-ID
`chat:<session>#<n>`, eigene Review-Note, Provenance targetType author-answer). **C4a gebaut:**
`ClarifySweepCollector` (det. Lücken-Diagnose: STATEMENT_FEHLT/AK_LEER/HISTORIE/UNSPEZIFISCH + REQ-Kontext,
zweig-agnostisch via Origin-Test) + CLI `clarify-sweep collect` → `runs/clarify-sweep/<id>/plan/
sweep-katalog.json`. **Live: 21 Klärungen erkannt — die HISTORIE-Diagnosen liefern dem Steward fertiges
Frage-Material** (z. B. PBI-023: 3 EXTEND-Begründungen ungeklärt). **471 Tests · Smoke-relevant unverändert.**
▶ C4b: Antworten→Plan+Alignments→pbi-update-Review (Trigger-Naht!).

## I-16 · 09.08.2026 — C4b: Antworten→Plan→bestehende Review-Bahn (Trigger-Naht sauber)

**Gebaut:** (1) §8-Leitplanke als Code: `PbiStateChangeOperation` ADDITIV um `authorAnswerRef`/`authorAnswerText`
erweitert — Autor-Antworten sind EIGENE Herkunft (`chat:<session>#<n>`), RequirementId bleibt LEER (kein
Fake-REQ); Review-Adapter zeigt eigene Note „Autor-Antwort via steward-chat (ref)" (E0-Muster).
(2) `ClarifySweepAnswer` + `ClarifySweepPlanBuilder` (det.): Validierung LAUT (unbekannt/leer/nicht-mehr-
needs_clarify ⇒ Skip mit Grund, §4-Koexistenz) → MARK_CHANGED-Ops + PbiAlignTargets mit Antwort als Trigger.
(3) `clarify-sweep run --answers <file>`: §4-Kollisions-Warnung (pausiertes pbi-gate via Status-Reader) →
Plan-Bau → Alignment-Drafting über die EXISTIERENDE PbiAlign-Naht (gleicher Prompt/Tools wie R-26-C, eigener
Auftragstext „Trigger = Autor-Antwort") → `pbi-change-plan.json` mit Alignments → Übergabe-Hinweis
`pbi-update-review <dir>` — Gate und Apply UNVERÄNDERT. (4) Heredoc-Falle erneut (\n) → Edit-Tool.
**Tests (§5 b/c/d):** Validierungs-Trias · Herkunfts-Naht (ref+leere RequirementId+Rationale-Quelle) ·
**Apply-Roundtrip: accept+Alignment ⇒ Blocker fällt · accept ohne Alignment ⇒ BLEIBT · reject ⇒ BLEIBT**
(kein stiller Erfolg — der Autor-Leitsatz „keine Überraschungen"). **473 Tests · Smoke 14/0 · 0 Warnungen.**
▶ C4c: Steward-Seil `run_clarify_sweep` + Zielschleifen-Prompt + Live-Probe (mit R-42-Pflicht: Gate-Check+Backup).

## I-17 · 09.08.2026 — C4c: Steward-Zielschleife — ★ C4 KOMPLETT

**Gebaut:** (1) `collect_clarify_katalog` als LESE-Tool (in-process, kein Run, LLM-frei — Collector direkt
an der Naht). (2) `save_sweep_answers` (kein Wahrheits-Write: stempelt Quelle „author via steward-chat" +
Session, validiert LAUT, schreibt `state/steward/sweep-answers/<ts>.json`). (3) `run_clarify_sweep`
(ApprovalRequired, Aux-Bahn) — Steward jetzt **15 Tools** (7 davon zustimmungspflichtig). (4) Prompt:
C4-ZIELSCHLEIFE als 4-Schritt-Anleitung (Diagnose vorlesen → konkrete Frage DARAUS · wörtlich sichern ·
zustimmungspflichtig fahren · Gate-Befehl nennen, NIE selbst beantworten · DoD „alle ausgewählten
adressiert, skip zählt" + ehrliche Schluss-Meldung) — der A4-Loop ist damit VERDRAHTET (K9).
**Live-Probe (R-42-Ritual: SHA-Check vor/nach = a9655f3a unverändert; Sweep schreibt keine Wahrheit):**
1 echte Antwort (PBI-030 Technologiestack: Flutter/SQLite/Riverpod) → `ba73ae`: op MARK_CHANGED mit
`chat:c4c-probe#1`, reqId LEER, Alignment belegtreu (3 verbindliche AK aus der Antwort) → wartet auf
Autor am Gate: `pbi-update-review runs/clarify-sweep/20260809_091459_ba73ae/plan`.
**474 Tests · Smoke 14/0 · 0 Warnungen. ★ C4 (a/b/c) = zweiter Commit-Meilenstein-Kandidat.**

## I-18 · 09.08.2026 — C4d: Pending-Registry — „Core = System, runs = Chronik" ★ C4 (a–d) KOMPLETT

**Gebaut (K12+§10/§11, kein Draufbau — 3 gezielte Refactorings):** (1) `ProjectStateProposal` ADDITIV um
`payload` (JsonElement?) — volle Nutzlast wandert in den Core. (2) **`PendingReviewRegistry`** (05-core):
Register (Frische-Anker = PBI-Versionen) · Close (nur gated Apply; Nutzlast entfällt, Beleg = Run-Kopie) ·
ListOpen (ÜBERHOLT beim LESEN berechnet: PBI weg / nicht mehr needs_clarify / Version geändert). K12-Grenze
GETESTET: Items/Relations reference-gleich, Kangal grün. (3) Sweep-Bahn REGISTRIERT (SaveAsync→Kangal);
Konsole nennt `pbi-update-review --pending PEND-<runId>`. (4) Review-Runner: `--pending <id>` MATERIALISIERT
die Registry-Nutzlast in einen frischen Review-Run (+pending-ref.json; ÜBERHOLT ⇒ laute Warnung);
Apply SCHLIESST via pending-ref (applied|rejected). (5) Parkplatz-Report + Render additiv um
PendingReviews → `pipeline-full status` UND Steward-`get_core_overview` zeigen Wartendes automatisch;
Prompt: proaktive Meldung + `open_review_ui`-Seil (ApprovalRequired, Aux-Bahn) — Steward **16 Tools** (8 zust.).
**Migration-Hinweis:** `ba73ae` (vor-Registry) bleibt klassisch bedienbar (`pbi-update-review <dir>`) oder
nach Autor-Entscheid neu fahren. **476 Tests · Smoke 14/0 · 0 Warnungen.** K12 damit GEBAUT (⚖-Bestätigung
beim Commit). ★ C4 KOMPLETT = Commit-Meilenstein.

**I-18-Live-Beweis (09.08., Autor-Auftrag „mini sweep als test"):** Sweep `d403e0` (PBI-002-Antwort aus dem
belegten EXTEND-Kontext) → Registry-Eintrag `PEND-…d403e0` OPEN mit Voll-Payload im Core · K12-Grenze live
(194 Items/422 Relationen unverändert, nur proposals) · **`pipeline-full status` zeigt „1 wartende(s)
Review(s)"** — die Auffindbarkeit, die der Autor bestellt hat. Beleg-Kopie im Run. R-43-Gegenprobe offen
beim Autor: `pbi-update-review --pending PEND-20260809_102419_d403e0` (Apply/Reject schließt den Eintrag).

## I-19 · 09.08.2026 — R-43-Endform: „Fertig" kettet den Apply

pbi-update-review: nach `ReviewOutcome.Finished` läuft der Apply AUTOMATISCH an (Entscheidungs-Datei wird
weiter ZUERST geschrieben = Replay/W2 unberührt) · `--no-apply` = Inspektions-Opt-out · Cancelled/„Später"
⇒ kein Apply, lauter Hinweis, Registry hält den Eintrag offen. Damit ist der stille Nicht-Effekt (2× live)
konstruktiv unmöglich; der --pending-Fluss schließt jetzt in EINEM Akt (Gate-Ja ⇒ Wahrheit ⇒ Registry zu).
**Nachzug 09.08. ✅:** reverse-Review kettet VOLL (Core-Write wie pbi-update) · forward-Review kettet die
APPLY-VORSCHAU und lässt `--execute` bewusst als eigenen Akt (irreversible Außen-Grenze GitHub, R-16-Frische
zählt am Write-Zeitpunkt) · beide mit --no-apply + Cancelled-Schutz. **476 Tests · Smoke 14/0 · 0 Warnungen.**

## I-20 · 09.08.2026 — C5a: Gespräch als Gate-Kanal (Registry/pbi-update) ✅

**Gebaut (c5-plan §1–§4, alle ⚖ Empfehlungen):** (1) Materialisierungs-Naht aus dem --pending-Block GEHOBEN
(`PendingReviewMaterializer`, absolute Pfade — Fund: OutputDir war CWD-relativ, in Prod unsichtbar) —
UI- und Chat-Kanal nutzen DIESELBE Naht. (2) `StewardGateTools`: `get_pending_review` = TREUE Vorlage über
die EXISTIERENDE Adapter-Naht (E0-Wissen: Wirkung/Herkunft/Warn-Notes wörtlich, inkl. Autor-Antwort-Note)
· `submit_gate_decisions` (ApprovalRequired) = Governance-DOPPELSCHLOSS: Autor-Diktat + MAF-Approval-
Zusammenfassung als typisierter Bestätigungs-Akt; schreibt die IDENTISCHE Entscheidungs-Datei wie die UI
(EIN Vertrag, reviewer=`author via steward-chat`) → kettet R-43-Apply → Registry schließt. **P2a im Tool
erzwungen** (nicht-apply ohne Grund ⇒ Validierungsfehler ⇒ Agent muss nachfragen); edit ohne Felder LAUT.
(3) Prompt: Kanal-WAHL (§1: Chat vs. open_review_ui, Empfehlung nach Item-Zahl/Edit-Bedarf; Autor wählt) +
treue-Vorlage-Pflicht. Steward jetzt **18 Tools** (9 zustimmungspflichtig).
**Integrations-Test (LLM-frei, echter Kreis):** Fixture-Core+Registry → Vorlage trägt Autor-Antwort-Note →
P2a-Reject → Submit apply+accept ⇒ ECHTER Apply: Blocker fällt, Titel übernommen, Registry leer,
Datei-Vertrag mit Chat-reviewer. **478 Tests · Smoke 14/0.** Offen: C5b (Ein-Graph-Responder, R-38-Ports) ·
Live-Chat-Probe = nächster echter Sweep komplett im Gespräch (Autor).

## I-21 · 09.08.2026 — C5b-1: Ein-Graph-Gate im Chat (github-forward-gate) ✅

**Kern-Einsicht (macht C5b klein):** die durable Pause externalisiert Graph-Gates BEREITS auf Dateien —
persistierter Plan (07-github/github-forward-plan.json) + Entscheid-Datei, die der resume-Responder liest
(PipelineFullRunner: LoadForwardDecisions → Port-Antwort; execute bleibt POLICY-gebunden). Der 5. Responder
braucht KEINE Port-Chirurgie (R-38 unberührt). **Gebaut:** `get_paused_gate(runId)` (treue Op-Vorlage aus
dem persistierten Plan; nicht unterstützte Gates ⇒ LAUT mit Verweis auf Review-UI) +
`submit_paused_gate_decisions` (ApprovalRequired; apply|skip-Vokabular hart; VOLLSTÄNDIGKEITS-Zwang wie C5a
— Vertagen = nicht submitten, Lauf bleibt pausiert; schreibt github-forward-decisions.json mit
reviewer=author via steward-chat = EXAKT der Vertrag des Responders) → danach bewusst separat: resume_run
(eigene Zustimmung; GitHub-Write nur mit execute-Policy). Steward **20 Tools** (10 zustimmungspflichtig).
**Tests:** Vorlage (inkl. DRIFT-SPERRE-Rationale sichtbar) · Sammel-Akt-Zwang · Datei-Vertrag-Roundtrip.
**479 Tests · Smoke 14/0 · 0 Warnungen.** Offen: weitere Graph-Gates (gleiche Schablone: Plan-Artefakt +
Entscheid-Datei je Gate) — mechanische Folge-Moves nach Bedarf; ingest/decision zuerst, wenn bestellt.

## I-22 · 09.08.2026 — K13-Hebung: Audit + K13-1 (Apply-Trio typisiert)

**Audit (Autor-Frage „wo noch CLI falsch?"):** ❌ Steward-Aux-Seile + R-43-Ketten (String-Args als
Integration) · ⚠️ Bootstrap-Kette = dokumentierte Altlast · ✅ CLI-Map/Smoke/Tests = Eingänge. Geparktes
in aufgefallen §K13-3 (Bootstrap · StatusReader-generisch · Partial-Submits · weitere Chat-Gates).
**K13-1 ✅:** alle drei Apply-Runner in typisierten Kern `ApplyFromPlanDirAsync(planDir, repoRoot[, execute…])`
+ dünne CLI-Haut gesplittet; R-43-Ketten (3× review→apply) und StewardGateTools rufen den KERN (Steward-
Test-Naht jetzt Func<string,Task<int>> planDir-typisiert). Verhalten byte-gleich gepinnt durch bestehende
Integrations-Tests (voller Chat-Gate-Kreis läuft durch die neue Naht) + Smoke. **479 Tests.**
▶ K13-2: die 5 Steward-Seile auf typisierte Nähte (inbound=Harvest direkt · snapshot/reverse/sweep heben ·
open_review_ui typed) + _aux-Ersatz durch Je-Naht-Delegates.

## I-23 · 09.08.2026 — K13-2 ✅: die Steward-Seile typisiert — ★ K13-HEBUNG KOMPLETT (1+2; 3 geparkt)

**Gebaut:** vier Kerne gehoben (`GithubIssueSnapshotRunner.PullIssuesAsync(repo)` · `GithubReverseRunner.
RunReverseAsync` · `ClarifySweepAnswersRunner.RunFromAnswersAsync` · `GithubInboundRunner.RunHarvestAsync`
[dünn über der existierenden Harvest-Naht]) + `PbiUpdateReviewRunner.RunPendingAsync` als bewusste
UI-EINGANGS-Façade (Args-Form = SEIN CLI-Vertrag). **StewardRunTools: _aux ERSETZT durch fünf je-Naht-
Delegates** (pullSnapshot/runInbound/runReverse/runSweep/openReview — kompilierfest, Test-injizierbar);
keine Aux-/Apply-INTEGRATION läuft mehr über CLI-String-Args. PRÄZISIERUNG (Kollege 09.08.):
run_pipeline_full/resume_run bauen weiter ["pipeline-full", …]-Args — das ist der LEGITIME durable
Graph-EINGANG (K13: Langläufer über den Graph-Runner), nicht die kritisierte Aux-Integration.
CLI-Runner = nur noch Häute (parse → Kern).
**Pinning:** Aux-Args-Tests zu Naht-Aufruf-Tests umgeschrieben (Verhalten identisch), Integrations-Tests
(Chat-Gate-Kreis, R-43-Ketten) + Smoke unverändert grün. **479 Tests · Smoke 14/0 · 0 Warnungen.**
K13-Endbild erreicht: Executor=Graph · Naht=Fähigkeit · CLI/Steward/UI=Eingänge. K13-3 geparkt
(aufgefallen: Bootstrap-Altlast · StatusReader-generisch [Trigger: erster durabler Teil-Workflow] ·
Partial-Submits · weitere Chat-Gates).

## I-24 · 09.08.2026 — W2-Freeze-Checkliste verankert + M1b ✅

**Freeze-Definition** (Autor: „W2 = fertig, danach kein Bauen mehr") als `w2-freeze-checkliste.md` mit der
angenommenen Kollegen-Reihenfolge (M1b → Chat-Gates ingest+decision → arch-Sweep → 9i → ⚖ C2d/A5 →
Topf-3-Einzeldurchsprache [Autor: nichts pauschal verschieben!] → A4-Agent kurz vor W2 → Mess-Vorbereitung
+ FERTIG-Abnahme). **M1b ✅:** `run-config.steward.memory` als Projekt-Default (RunConfig-Sektion +
HostSettings.StewardMemoryMode additiv, getrimmt gemappt; CLI --memory überstimmt; Banner zeigt
„(run-config)"); gleiche K6-Validierung/Modi. Test: Mapping+Leer-Fall. **480 Tests · Smoke 14/0.**
▶ Checkliste 2: Chat-Gates ingest + decision (je Gate: Plan-Artefakt+Entscheid-Datei-Vertrag lesen,
C5b-1-Schablone, P2a/Sammel-Akt).

## I-25 · 09.08.2026 — Freeze-Schritt 2 ✅: Chat-Gates ingest + decision (+ R-44)

**Warum (Autor-Erklärformat):** Tor 1 = das häufigste Gate (jeder Meeting-/GitHub-Lauf pausiert dort),
decision-gate = das gesprächigste (Auflösung ENTSTEHT am Gate, vertagen erlaubt) — mit beiden im Chat
schließt sich die A4-Schleife ohne Kontextwechsel, und W2 misst UI-vs-Chat an den Gates, die zählen.
Workflow-seitig NICHTS neu: gleiche Datei-Verträge, die der resume-Responder liest.
**Fund R-44:** ingest hatte gar KEINEN Datei-Vertrag (nur accept-all-Flags) → neu:
`ingest-gate-decisions.json` + Responder liest Datei vor Flags (Event trägt source).
**Gebaut:** get_paused_gate = Gate-Switch (forward | ingest/arch-ingest aus plan.json | decision aus
decision-gate-request.json, Vorlage mit Wahrheit-vs-Meeting-Gegenüberstellung) · 2 neue ApprovalRequired-
Submits (ingest: apply|reject, P2a bei reject; decision: resolve[KEEP_ORIGINAL|ADOPT_NEW|REFINE+newStatement]
|defer, Wiederverwendung des EXISTIERENDEN PipelineDecisionResolution-Records = ein Vertrag) · Sammel-Akt-
Zwang überall · Prompt-Anleitung. Steward **22 Tools** (12 zust.). Tests: Vorlagen, P2a, Vokabular,
Responder-Kompatibilität (Loader/Deserialisierung der ECHTEN Verträge). **482 Tests · Smoke 14/0.**

## I-26 · 09.08.2026 — Freeze 3b-1 ✅: K12-Lese-Kategorien + R-44b + Auto-Resume + open_gate_ui

**⚖ geklärt (Autor: „Report in den Core?"):** NEIN — K12 hat DREI Lese-Kategorien: ①Wahrheit=Core ·
②Wartendes=Registry/Checkpoint · ③Rückblick/Beleg=Chronik (lesen richtig, nie Prozess-Transport);
Report=③, Geliefertes steht ohnehin als Wahrheit im Core (in k-entscheidungen verankert).
**Gebaut:** ① **R-44b**: pbi-gate-Responder liest jetzt 07-pbi-update/human-decisions.json (Plan liegt
dort; AcceptedFromDecisions→op-Ids) vor den Flags — die BESTEHENDE pbi-Review-UI kann direkt auf die
Pipeline-Stufe zeigen. ② **Auto-Resume**: decision-gate-review „Fertig" ⇒ resume automatisch (LAUT:
Folgestufen inkl. LLM; --no-resume Opt-out; Cancelled ⇒ Pause) · pbi-update-review ERKENNT Pipeline-Stufen
(TryGetPipelineRunId) ⇒ resume STATT Standalone-Apply (Doppel-Apply-Schutz, getestet). ③ **open_gate_ui**-
Seil (ApprovalRequired, typisierte Façaden RunForRunAsync/RunForPipelineRunAsync) — Steward **23 Tools**.
**483 Tests · Smoke 14/0.** ▶ 3b-2: ingest-UI-Brücke (Standalone-Schema ↔ R-44-Datei) — dann 3c Autor-Front.

## I-27 · 09.08.2026 — Freeze 3b-2 ✅ = ★ 3b KOMPLETT: ingest-UI-Brücke

**Glücksfund:** Standalone- und Pipeline-Schema sind feld-gleich (incomingItemId/decision/reason) — die
Brücke ist ein Loader-Fallback: `IngestGateDecisions.TryLoadAny(stageDir)` liest den Chat-Vertrag ODER die
human-decisions.json der BESTEHENDEN ingest-UI (Responder umgestellt, Test mit Datei-Umbenennung).
Dazu: ingest-UI kettet auf Pipeline-Stufen den resume (TryGetPipelineRunId-Wiederverwendung, --no-resume,
Cancelled ⇒ Pause) · `RunForPipelineRunAsync(runId, stage)`-Façade · open_gate_ui deckt jetzt decision |
pbi | ingest | arch-ingest. **483 Tests · Smoke 14/0.** ★ 3b: alle vier Chat-Gates haben UI-Zwilling mit
Auto-Fortsetzung — Kanal-Wahl ist überall real. ▶ 3c Autor-Front.

## I-28 · 09.08.2026 — Freeze 3c ✅: die AUTOR-FRONT — die dritte Evidenz-Quelle

**Gebaut:** (1) `AuthorFrontDeltaBuilder` (04-delta, K13 in-process): Diktat [{text, disposition
requirement|architecture, rationale?}] → Delta im Meeting-Ketten-Vertrag (AF-n-Items, §5-baseline-Achsen,
Herkunft „author via steward-chat"+Session, Source/Provenance dictated_by; Validierung LAUT). Mächtigkeit =
GitHub-Front (⚖ beantwortet: ab Tor 1 sind ALLE Fronten identisch; Meeting bleibt bewusst tiefer
[Ledger/Jury], Autor braucht keine Jury — er IST die Quelle). (2) `save_author_statements` (kein
Truth-Write; state/steward/author-front/<ts>-delta.json) → Hinweis run_pipeline_full(deltaPath) — Tor 1
pausiert dann IM CHAT (3b!). (3) `read_run_report` (K12-③ Chronik-Read auf den R-40-Vertrag: applied je
Item inkl. entityId) + `sourceRunId`-Filter an list_core_items (Wahrheits-seitige Lauf-Bilanz). (4) Prompt:
4-Schritt-Anleitung inkl. „berichte in EINEM Satz, was geliefert wurde".
**Schlüssel-Test:** die Diktat-Datei lädt über DIESELBE Naht wie --from-delta (JsonProjectStateRepository)
= Pipeline-Kompatibilität bewiesen, LLM-frei. Steward **25 Tools**. **485 Tests · Smoke 14/0.**
Der Endzustand des Autor-Wunsches steht: „ich will PBI X" → Diktat → Kette → Gates im Chat → Report-Satz.

**I-28-Nachzug (Autor-Fragen 09.08.):** ① Interpretation statt Diktat-Zwang — Prompt: VERSTEHEN →
präzise Fassung VORLESEN → erst nach Autor-OK speichern (Interpretation mit Unterschrift). ② **Schritt 0
Vorprüfung** (immer zuerst): Core-Suche nach dem Thema (Bestand nennen — evtl. Erweiterung statt Neuanlage)
+ NEUES Lese-Tool `search_rejections` (R-35-Naht via IngestionRejections.Of + QueryText: „wurde so etwas
schon abgelehnt, und warum?" — kein Auto-Skip, Autor entscheidet). ③ **Vollständigkeits-Coaching** im
Prompt: aktiv nach Warum/Nutzen (→Statement), prüfbaren Kriterien (→AK), Rahmen (→arch-Aussage) und
Bestand-Konflikten (→sonst DEC) fragen — verhindert needs_clarify/DECs an der QUELLE. Steward **26 Tools**.

## I-29 · 09.08.2026 — Freeze-Schritt 3 ✅: der ARCH-SWEEP (9k(b))

**Warum (Autor-Format):** Architektur-Unklarheit hat kein Blocker-Flag — sie ist RELATIONAL und seit R-11
ausrechenbar. Der Sweep macht die drei Lücken-Arten zum Gespräch: Arbeit ohne technischen Rahmen (PBIs ohne
constrained_by) · design-Entscheide ohne ADR · unklassifizierte arch-Items.
**Bau (bewusst minimal — der Antwort-Weg EXISTIERT):** `ArchGapCollector` (05-core, det., Feature-Kontext
aufgelöst, superseded raus) + Lese-Tool `collect_arch_katalog` + Prompt-Schleife: Antworten = Architektur-
Aussagen → AUTOR-FRONT (disposition architecture) → arch-Tor-1 (chat-fähig) → classify → adr-Gate. KEINE
neue Bahn, kein neues Delta-Format — K13-Wiederverwendung pur. Fähigkeiten-Doc gepflegt (27 Tools, Kreislauf 4).
**Echt-Zahlen (LLM-frei, Produktiv-Core):** arbeitOhneRahmen=43 (fast alle PBIs — die ①-Relation entstand
erst spät im R-11-Wirkungs-Gate) · designOhneAdr=0 (A5 hat geliefert!) · unklassifiziert=45. Der Katalog
zeigt ehrliche Realität; der Autor wählt Teilmengen (überspringen erlaubt, DoD wie C4).
**486 Tests · Smoke 14/0.** Lern-Notiz: interaktive Befehle nie ohne --once in Proben (10-min-Timeout).

## I-30 · 09.08.2026 — Freeze-Schritt 4 ✅: 9i-Fragen-Adapter + 9m-Herkunfts-Naht

**Begriffs-Klärung vorab:** „9i" existierte doppelt — Parkplatz-9i (AUSWÄRTS: DEC→Klärungs-Issue, bleibt W4)
vs. Freeze-Schritt-4-9i (EINWÄRTS: Fragen von außen → DEC-Topf). Gebaut wurde EINWÄRTS.
**Autor-Leitplanke:** EIN vorgegebenes Issue-Format ist der Vertrag; bewusste Diskussions-/Sonder-Issues =
`NiC:`-Opt-out; Issue-Typologie (work/disk/…) = bewusster Ausblick. **Autor-Signal für die ⚖-Weiche
(Schritt 5):** „die meiste Thematik wird sich in Kommentaren abspielen" → wertet C2d auf (in Checkliste notiert).

**Gebaut (alles an bestehenden Nähten, keine neue Bahn):**
1. **`GithubOriginMeta`** (05-core, die EINE Herkunfts-Naht): Issue-Nr/URL/Hashes als Item-Metadata;
   `CarryOver` am gated Apply (IngestMeta + MeetingQuestionMint — bahn-neutral, no-op ohne Herkunft);
   `ClaimsByIssue` (Ernte-Gedächtnis aus der Wahrheit selbst, K12: kein eigener Stempel-Store);
   `AdoptedIssueByPbi` (covers→Item mit Herkunft, NUR bei Eindeutigkeit).
2. **Stecker GitHub:** DeltaBuilder nimmt open_question-Drafts mit (Disposition IST der itemType) →
   9g-Schiene → ingest-Gate → DEC mit Herkunft. Detect: adoptierte Issues unverändert = skip (unchanged),
   erneut editiert = **F7_ADOPTED_DRIFT** (neuer Fund-Typ, erntbar, mit adoptedItemId).
3. **9m GEFIXT (Nebenfund aus der Vorlauf-Verifikation):** die geernteten Herkunfts-Felder hatten KEINEN
   Konsumenten (IngestMeta baute Metadata neu auf = Evidenz verworfen; Forward fand adoptierte Issues nur
   per Such-Match → Duplikat-Risiko). Jetzt: ForwardSeed linkt unmapped PBIs mit eindeutiger Adoption
   DETERMINISTISCH aufs Ursprungs-Issue (LINK-Op, gated wie alles).
4. **Stecker Autor-Front:** dritte Disposition `question` → itemType open_question (Autor-Sprache ↔
   9g-Vertrag im Builder gemappt); Prompt-Routing-Weiche + Tool-Beschreibung nachgezogen.
5. **Spur-Exklusiv-FUND+FIX (Code-Lektüre, latent seit A1d):** die Fragen-Spur war in JEDEM Aspekt-Strip
   Coverage-Pflicht (Incoming() + Gate profil-blind) — bei gemischten Deltas (req+arch+Fragen) hätte auch
   der arch-Strip die Fragen covern müssen; der identitäts-freie OpenQuestion-Apply hätte DOPPELT geprägt
   (Doppel-DEC). Fix als Daten: `AspectIngestionProfile.CarriesQuestionLane` (req=true, arch=false = der
   E-R3-Entscheid „arch ohne Fragen-Block" endlich als Code). Löst D-2 („Fragen-Pfad ist Querschnitt")
   BEWUSST ab — Pinning-Test umgezogen.
**Tests:** neue Datei `GithubQuestionOriginTests` (7 Fälle: Delta-Mitfahrt, CarryOver NEW+OPEN_QUESTION+
bahn-neutral, Spur-Exklusivität beidseitig, Detect skip/F7, Seed-LINK, Eindeutigkeits-Regel, Autor-Front-
question); 2 Alt-Pinnings bewusst gehoben (Drafting „nicht im Delta", Profil „Querschnitt").
**493 Tests · Smoke 14/0 · 0 Warnungen.** NICHT gebaut (bewusst): Auswärts-9i (DEC→Issue, W4) · C2d-Kommentare (⚖ Schritt 5).

## I-31 · 09.08.2026 — Freeze-Schritt 5 / C2d ① KERN GEBAUT (Bausteine 1–4+6): der Kommentar-Kreis

**⚖-Vorlauf im Plan festgezurrt** (`c2d-kommentar-destillat-plan.md`): 5 Kollegen-Schärfungen (eigener
comments-snapshot · Gedächtnis in Core-/Mapping-Metadata, KEIN Zweit-Store · clarify_answer = expliziter
C4-Adapter-Sonderfall · Vermerk erst nach Wahrheits-Entscheid · ③ nach dem Kern) + 2 Autor-Entscheide
(AUTO-PULL am --from-github-Eingang · HERKUNFTS-TREUE im reject+Neu-Diktat-Korrekturpfad) + K11-Begründung
„warum der Kern nicht MCP wird" dokumentiert.

**Gebaut + getestet (alles LLM-frei verifizierbar):**
1. **Kommentar-Pull:** `github-snapshot comments` (repo-weiter API-Endpoint, EIN paginierter Call,
   aufsteigend nach created; Issue-Snapshot = PR-Filter; eigenes Artefakt issue-comments.json + Summary;
   typisierte Naht PullCommentsAsync; Locator FindLatestComments).
2. **`GithubCommentMeta`** (05-core): High-Water-Mark `lastProcessedCommentId` IN der Wahrheit —
   Mapping-Relation vor Item-Claim (GithubOriginMeta), monoton, NewSince-Filter; dokumentierte Grenze:
   Issue ohne Core-Anker → Kommentare kommen mit R-35-Note wieder (bewusst, kein Auto-Skip).
3. **Destillat:** `GithubCommentDistill` — det. Collector (Anker-Filter, NiC-Opt-out, Core-Kontext inkl.
   needsClarify-Signal) + Agent im Save-Once-Zaum (5 Dispositionen; Beleg-Pflicht commentIds;
   clarify_answer braucht targetPbiId; MEHRERE Drafts je Fund erlaubt) + Prompt
   GithubCommentDistillAgent1 · DeltaBuilder-Erweiterung: Mehrfach-Drafts je Issue = eindeutige IDs
   (GH-20, GH-20-2), EINE Quelle, `commentAnchor`-Metadata.
4. **Anker-Stempel am gated Apply:** IngestionApplyExec stempelt für JEDES plan-abgedeckte Incoming mit
   Anker (Annahme UND Ablehnung, §11) im selben Kangal-Save; anker-lose Ziele LAUT benannt.
   **clarify_answer-Adapter:** ToClarifyAnswers → C4-Antworten mit AnswerRef `gh-comment:<issue>#<id>`
   (ClarifySweepAnswer additiv um AnswerRef erweitert).
5. **Zwischenbahn-CLI `github-comment-distill`** (collect LLM-frei · --draft = bewusster LLM-Schritt;
   Frische-Guard COMMENTS_STALE >24h) + **2 Steward-Seile ⚿** pull_issue_comments/run_comment_distill
   (typisierte Delegates, Prompt-KOMMENTAR-KREIS) → **29 Tools / 13 ⚿**.
**Tests:** GithubCommentMetaTests (3) + GithubCommentDistillTests (5); 2 Pinning-Tests bewusst gehoben
(9→11 ⚿, 27→29 Tools). **501 Tests · Smoke 14/0 · 0 Warnungen.**

**NOCH OFFEN in ① (nächste Moves):** Baustein 5 Vermerk-Seed (COMMENT-Op aus dem Lauf-Report in den
Forward-Plan) · §3-1-Auto-Pull am --from-github-Eingang · §3-7 Herkunfts-Option an save_author_statements.
Danach ② MCP-Spike · ③ Rückfrage-Seil.

## I-32 · 09.08.2026 — C2d ① KOMPLETT: Vermerk + Auto-Pull + Herkunfts-Treue (die drei Rest-Moves)

**1. Abschluss-Vermerk (§3-5) — als ehrliche VERTRAGS-Erweiterung statt Workaround:** neuer Forward-Op-Typ
`NOTE_COMMENT` (COMMENT hätte ein Mapping geprägt = Remap-Risiko aufs Ursprungs-Issue; NOTE_COMMENT schreibt
NUR den Kommentar, ist von der Coverage-Regel ausgenommen [Betreff = Wahrheits-Item, kein Delta-PBI], behält
Ziel- und Anker-Wachen). `GithubCommentVermerk.Derive` = reine Funktion aus Lauf-Report (R-40) + Delta +
human-decisions: „✔ Eingepflegt: REQ-x (KIND)" nach Apply · „✕ Bewusst nicht übernommen — Begründung: …"
nach P2a-Reject · UNBELEGTES bekommt NIE einen Vermerk (Experiment-Modi ohne decisions-Datei). Seed-Anschluss
im GithubForwardSeedExecutor (TryDeriveFromRun; Standalone-Forward ohne Ingest-Report = vermerk-frei,
dokumentierte Zwei-Bahnen-Grenze). Ein Vermerk je Issue, mehrere Aussagen = Zeilen.
**2. Auto-Pull (§3-1):** `pipeline-full run --from-github [--repo o/n]` pullt Issues+Kommentare SELBST in
`00-github-inbound/snapshot/` (Run-Artefakt = Beweis-Basis; repo aus --repo oder run-config.fullworkflow.repo,
sonst LAUT); `--issues [--comments]` = Replay-Weg. **Harvest = echter Fan-in:** optionaler Kommentar-Snapshot
→ Distill-Collector + ZWEITER Deutungs-Agent im selben Lauf-Faden → Issue-Drafts + Kommentar-Destillate in
EIN Delta (geteilte Konvertierung ToInboundDrafts/ToSyntheticFinds — Zwischenbahn-Runner nutzt DIESELBE Naht);
clarify_answer → sweep-answers.json + Konsolen-Hinweis (C4-Bahn, bewusste ①-Grenze).
**3. Herkunfts-Treue (§3-7):** AuthorStatement +githubIssueNumber; Builder mit githubOrigin-Resolver-Naht
(Delta-Schicht bleibt github-frei) — der Steward löst aus dem letzten Snapshot Url+Hashes auf
(Stand-beim-Diktat als Drift-Anker); Tool-Beschreibung instruiert „IMMER setzen, wenn Anstoß aus Issue".
Detect-Wache: Herkunft OHNE Hash-Anker = sichtbare Skip-Note „Drift nicht prüfbar" statt Dauer-F7.
**Tests:** GithubCommentVermerkTests (4: Derive ✔/✕/nie-unbelegt · Gate-Coverage-Ausnahme+Ziel-Wache ·
Autor-Front-Herkunft · Detect-Wache). CLI-Haut-Proben LLM-frei (laute Ablehnungen korrekt).
**505 Tests · Smoke 14/0 · 0 Warnungen. ★ C2d ① KOMPLETT.** Offen: ② MCP-Spike (K-Methodik) · ③ Rückfrage-Seil.
Unbewiesen bleibt (ehrlich): Live-Destillat mit echtem Kommentar + Vermerk-Write = FERTIG-Abnahme (R-42-Ritual).

## I-33 · 09.08.2026 — ★ C2d KOMPLETT: ② MCP-Spike GRÜN + Live-Einbau · ③ Rückfrage-Seil

**② Doku-Check (K-Methodik, quellenbasiert):** ModelContextProtocol C# SDK ist STABIL bei **2.1.0**
(nuget.org, 05.08.2026; unser 0.9.0-preview.1 = Museum) · offizieller `github/github-mcp-server` hat
Governance SERVER-SEITIG: lokal `GITHUB_READ_ONLY=1`+`GITHUB_TOOLSETS`, Remote `/x/{toolset}/readonly`-URL
(readonly = strikter Filter mit Vorrang; github.blog Changelog 12/2025) · **`McpClientTool : AIFunction`**
(csharp-sdk getting-started) — MCP-Tools SIND unsere Tool-Sorte, kein Adapter. K11-Kern-Grenze bestätigt.
**Spike-Bau:** Paket → 2.1.0 (kompilierte OHNE API-Anpassung) · totes `McpConnections`-Dummy GELÖSCHT →
Besteller-Naht `Mcp/GithubMcp` (Remote HTTP+PAT-Bearer auf `/x/issues/readonly` · Lokal docker-stdio,
Token als Prozess-Env nie CLI-Arg) · CLI-Probe `mcp-probe github [--local]` mit **readonly-WACHE**
(schreib-verdächtige Tool-Namen ⇒ Spike ROT).
**Spike-EMPIRIE GRÜN (1. Versuch, Remote):** server=github-mcp-server/remote · **6 reine Lese-Tools**
(get_label, issue_read, list_issue_fields, list_issue_types, list_issues, search_issues) · Wache PASS ·
PAT reicht (kein Copilot-Lizenz-Blocker).
**Einbau:** run-config `steward.githubLive` (Default an, Kill-Switch) · StewardChatRunner mountet die
MCP-Tools FAIL-SOFT (ohne Token/Netz: laute Note, nie blockierend; Client-Lifetime = Session, await using) ·
BuildAgent additiv `liveTools` · Prompt-Regel: Live = Gespräch („live von GitHub" sagen), Verarbeitung =
weiter Snapshot+Gates (Live-Gelesenes ist kein Beleg). **LIVE-BEWEIS** (--once, Session mcp-live-probe,
Run 193426_a06652): „github-live=6 Lese-Tools" → Steward beantwortete „wie viele offene Issues JETZT?"
live via list_issues (39, totalCount) MIT Quellen-Nennung. **= MAF-Feature-Matrix-Lücke MCP GESCHLOSSEN
(reales Szenario: MAF konsumiert offiziellen fremden Server, governance-gefiltert).**
**③ Rückfrage-Seil:** ⚿ `post_issue_comment(repo, issueNumber, text)` (GithubRestIssueClient-Naht,
Token via geteilte ResolveToken-Quelle; Doppelschloss: Prompt erzwingt WÖRTLICHES Vorlesen VOR dem Aufruf,
dazu MAF-ToolApproval; Fehler als typisiertes COMMENT_POST_FAILED). Kommentar = Diskussionsraum, nie Wahrheit.
**Steward: 30 Tools / 14 ⚿ (+ zur Laufzeit die 6 MCP-Live-Tools). 505 Tests · Smoke 14/0 · 0 Warnungen.**
**★ C2d ①+②+③ KOMPLETT.** Offen (bewusst): A5-⚖ · Live-Beweis Destillat+Vermerk-Write = FERTIG-Abnahme.

## I-34 · 09.08.2026 — Kurs-Klärung (Autor) + UX-Abnahme-Testplan angelegt

⚖ Autor: **Schritt 6/Topf 3 bleibt OFFEN als starkes Ausblick-Thema** (evtl. großes Refactoring NACH der
Messung, v. a. 9j-Kapseln) — kein Vorab-Verwerfen; Fokus jetzt: **Messen** (6c: Konzept festigen, Autor-Idee
„Stufen-Ersatz durch freie Agenten" [z. B. Ledger-Agent, Treue messen] = erweiterte Ablation) und
**Steward ausgiebig testen**. Dafür: `steward-ux-testplan.md` (Blöcke A–K, ~40 Schritte: Wache/Lesen/
Memory/Live-MCP/Autor-Front/Sweep/GitHub-Runde inkl. Vermerk+Gedächtnis-Beweis/Rückfrage/Fehlerpfade/
Reverse/Abschluss; je Schritt Erwartung + Claude-Log-Prüfung; R-42-Ritual als Pflicht-§0; Befund-Tabelle
lebend). MAF-Audit fürs Protokoll: alle relevanten Bausteine am offiziellen Slot genutzt; bewusste
Nicht-Nutzung nur Handoff (A5-⚖ vertagt auf Schritt 7), A2A (K7), Hosting/Web-Chat (CLI-Artefakt —
HumanReview-Web-UI ist ASP.NET fürs Gate-Review, KEIN MAF-Agent-Hosting).

## I-35 · 10.08.2026 — UX-/E2E-Testplan lückengeschlossen (Abdeckungs-Audit)

Testplan gegen das GESAMT-Gebaute geprüft (30 Tools + 6 MCP · 10 Gates · beide Bahnen · beide Kanäle ·
alle Delta-Op-Sorten). Gefundene Lücken geschlossen: **§0.1** bewusst-nicht-getestet (Bootstrap/Experiment-
Modi/A5/Kangal-Feuer, je begründet) · **L5b** ADR-Artefakte (docs/adr/*, README-Index, architecture.md) ·
**L5c** re-clarify-Gates cluster-review + backlog-review (waren im Plan gar nicht!) · **Block O**
Zwei-Bahnen & UI-Kanal (standalone pull_issue_comments/run_comment_distill, standalone github-inbound,
open_gate_ui für Graph-Gates, forward-Gate im Chat via submit_paused_gate_decisions, get_run_status,
mcp-probe-Regressions-Check) · **Block P** Delta-Semantik-Tiefe (SUPERSEDE, NEW_RELATED/featureKey [R-36-v2],
ALREADY_DECIDED, Cross-CONTRADICT req↔arch [E-R4], E-8-Weckruf, 9g-Meeting-Frage, Body-Round-Trip-
Projektions-Invariante, Treue-Rückverfolgung). Abschluss-Block K + Checkliste 6b nachgezogen; K trägt jetzt
eine Abdeckungs-Matrix als Selbstkontrolle.

## I-35b · 10.08.2026 — KORREKTUR (Autor-Frage „re-clarify Problem?"): re-clarify ist BOOTSTRAP-only

Verifiziert an PipelineFullWorkflow.cs: cluster-review + backlog-review (re-clarify) hängen NUR im
Bootstrap-Zweig („Bootstrap: core-bootstrap→Cluster+Gate→PBIs+Gate→Seed | Betrieb: Ingest+Gate→Pbi+Gate").
Im operativen Lauf feuern sie NICHT — neue Backlog-Arbeit läuft über pbi-update-Placement. Mein L5c
(„feuern bei Backlog-Arbeit") war FALSCH → korrigiert zu NEGATIV-Check (dürfen operativ NICHT erscheinen).
re-clarify-Test = neuer **Block Q** (optional, Bootstrap auf SCRATCH-Core: Produktiv-Core physisch weglegen
[Core-Pfad fest, kein Override] → Bootstrap → cluster/backlog-Gates → Core zurück, SHA-Gleichheit zwingend);
sonst durch state/core-b6-lauf* + Unit-Tests belegt. §0.1 + Abdeckungs-Matrix nachgezogen. Re-clarify-Zweck
fürs Protokoll: Requirements→Feature-Cluster (Recall-Gate: nichts verloren)→PBIs mit AK+expliziten Lücken
(DoR-Gate) — ersetzt seit 17.07. die alte L4-Kette.

## I-35c · 10.08.2026 — Präzisierung (Autor-Erinnerung „geteilte Feature-Logik?"): CoreBacklogSeeder

Verifiziert: Betriebszweig nutzt re-clarify NICHT (eigene Placement-Agenten + pbi-update-Gate), teilt aber
die deterministische Persistenz-Naht: **`CoreBacklogSeeder.Seed`** (05-core) wird von BEIDEN gerufen —
Bootstrap-Backlog-Seed (CoreSeedBacklogRunner) UND Betriebs-NEW_FEATURE (PbiUpdateApply:247 „O4b/Fall C").
Dazu R-36-v2-Seed-Regel (REQ→part_of_feature aus bestätigter Deckung) geteilt in IngestionApply +
PbiUpdateApply. Placement = operatives, inkrementelles Äquivalent zu re-clarifys Cluster/Backlog:
MARK_CHANGED/EXTEND_PBI/NEW_PBI/NEW_FEATURE (+ B1/B2 Feature-Editor). Testplan L7 auf die vier Fälle +
NEW_FEATURE-Seeder-Bahn geschärft; L7b B1/B2; L5c präzisiert (Gates bootstrap-only, aber Feature-Arbeit
operativ via geteilte Naht — kein „Betrieb ohne Feature-Logik").

## I-36 · 10.08.2026 — Testplan gehärtet (Kollegen-Review) + System-Landkarte + Code-Hygiene-Register

**Kollegen-Review Testplan → 4 Härtungen eingebaut:** (1) **§0.2 In WELLEN fahren** (1 risikoarm A–D/I/O6/O7
→ 2 kleine Schreibkreise F/H → 3 GitHub-Kreis G/J/O1–O3 → 4 schwer E/L/M/N/P/O4-5 → opt. Q → K zuletzt;
Start = Welle 1). (2) **R-42-Ritual vor JEDEM schweren Block wirklich fahren** (Core-Backup+SHA+Gate-Check).
(3) **Konkrete Werte = „Soll aktuell prüfen", keine Invariante** (github-live=6, #12, PBI-030, 19-x wandern
mit Core/GitHub → bei Abweichung erst Plan-Aktualität prüfen, dann Bug). (4) **Block G: Repo/Issues explizit
notieren + G9 CLEANUP-Entscheidung** (behalten-als-Beleg ODER aufräumen; kein Müll-vs-Beleg-Rätsel später).
**Zwei neue lebende Docs (Autor-Aufträge):** `system-landkarte.md` (Skelett — Komponenten/wann-feuert-was/
Zusammenspiel, zweischichtig stabil/volatil, Belege je RunId; = Thesis-Kap. 4+5-Rohbau) · `code-hygiene-
register.md` (Smells/toter Code/Clean-Code/Duplikate — Sofort-Refactor nur trivial+sicher, sonst loggen+
abwägen; toter-Code-Sorgfalt: 3-fach prüfen vor „tot"). **Test-Arbeitsmodus = DREIFACH je Block:** Verhalten
(→Testplan/R-Log) · Zusammenspiel (→Landkarte) · Code-Hygiene (Code lesen →Register). In STATUS + Checkliste
(6a Landkarte, 6b Abnahme) markiert.

## I-37 · 10.08.2026 — 4. Test-Linse „Mess-Bereitschaft" + 2 Kollegen-Präzisierungen + 1 Hygiene-Fix

**Vierte Prüf-Linse (Autor: Thesis/W2-Synergie):** je Block prüft Claude zusätzlich, ob die W2-Metrik-Quelle
(Treue/Recall/Autorisierung/Effizienz/Robustheit) sauber aufzeichnet → §0.4-Tabelle (Quelle je Metrik + wo im
Test); Lücken = R-/aufgefallen VOR W2, markante Läufe = „W2-Anker-Beleg" (Graph-Arm, den W2 gegen Agent/frei
vergleicht). HARTE GRENZE festgeschrieben: Abnahme = Beweise + Kalibrierung, NIE W2-Ergebnisse (die brauchen
Freeze+Replay+Gold+N≥3). Fließt in Schritt 6c. **Kollegen-Präzisierungen:** (1) Block K „Gate-Sequenzen inkl.
re-clarify" → „nur falls Q gefahren; operativ dürfen cluster/backlog NICHT erscheinen" (L5c-konsistent).
(2) `mcp-probe github` ignorierte das Arg still → **Hygiene-Fix** (1. Eintrag im code-hygiene-register):
explizites Parsing (optional `github` + `--local`, Unbekanntes LAUT Exit 2); Build 0 / 505 Tests grün.

## I-38 · 10.08.2026 — Governance-Recherche (OWASP/MAF) + governance.md + adversariale Tests GEBAUT

Autor-Auftrag: Governance-Kapitel-Fundament. Web-Recherche (OWASP AI Agent Security Cheat Sheet, LLM Top 10
LLM01, Design-Patterns-Paper arXiv 2506.08837, MAF-Docs/Middleware/FunctionApproval, Agent Governance Toolkit,
Maturity-Model). **`Thesis-Docs/aktiv/governance.md`** angelegt (9 Abschnitte: Prinzip · Bedrohung · Schutz-Architektur
[eine Kangal-Naht + 6-Schichten „vorschlagen statt handeln" + MAF-nativ-vs-unser] · Standard-Abgleich-Tabelle
[erfüllt/Lücke/Enterprise-Scope] · Injection-Resistenz · Lücken · Ausblick · 11 Quellen). Abgleich-Ergebnis:
architektonisch auf Stand der Technik (Least Privilege · Human-Approval MAF-nativ · kein Agent-fragt-Agent ·
Audit); einzige Bau-Lücke = adversariale Tests. **Diese GEBAUT:** `AdversarialGovernanceTests` (6 Tests, LLM-frei,
Worst-Case „Agent voll gekapert → bösartiger Vorschlag" → Governance fängt: kein-DELETE-Vokabular · Human-Autor.
· UNKNOWN_TARGET · QUESTION_KIND_MISMATCH · Kangal-I1 · Kommentar-bleibt-Proposal). **511 Tests · 0 Warnungen.**
Belegt Sicherheits-These EMPIRISCH. Verankert: STATUS-Doc-Index, governance.md §7.1, aufgefallen.
