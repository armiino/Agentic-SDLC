# Agentic SDLC — Agenten in den frühen Phasen des Software-Development-Lifecycle

Artefakt einer explorativen Masterarbeit. Das System führt ein **Meeting-Transkript** über eine
belegte Evidenzschicht bis zu **GitHub-Issues** — und zwar so, dass **jede Änderung an der
Projektwahrheit von einem Menschen autorisiert** wird.

```
Transkript → Evidenz-Ledger (+ Human-Adjudikation) → Baselines → Gap → Core (die Wahrheit)
           → Backlog/PBIs → GitHub-Issues
```

Gebaut mit dem **Microsoft Agent Framework (MAF) 1.15.0** in C#/.NET 10. Leitprinzip: kleine
Agent-Knoten in deterministischen MAF-Workflows, mit Gates, Repair-Schleifen und dem Muster
*Maker → Gate → HumanReview → Apply*. GitHub ist **Projektion, nie Quelle**.

> Alle Zahlen und Befehle in diesem Dokument wurden am **17.09.2026** auf diesem Stand
> ausgeführt und am Code geprüft.

---

## 1. Bauen und Testen

**Voraussetzung:** .NET SDK **10.0.103** (in `global.json` gepinnt). Sonst nichts — Bauen und
Testen brauchen **keinen API-Schlüssel und keine Netzverbindung**.

```bash
dotnet build Agentic-SDLC.slnx
dotnet test  AgenticSdlc.Tests/AgenticSdlc.Tests.csproj
bash tools/smoke-hitl.sh
```

| Schritt | Ergebnis | Dauer |
|---|---|---|
| Build (3 Projekte) | **0 Warnungen, 0 Fehler** | ~2 s |
| Tests | **701 erfolgreich, 0 Fehler** | ~5 s |
| Smoke-Netz | **14 PASS, 0 FAIL** | ~15 s |

Das **Smoke-Netz** fährt jede der fünf Gate-Stufen einmal `start → PAUSE → resume → Apply`,
ohne LLM und ohne Netz. Es sichert `state/core/` vorher und stellt es hinterher wieder her
(geprüft: SHA-256 vor und nach dem Lauf identisch).

<details>
<summary>Wenn der Build hakt</summary>

```bash
pkill -f "dotnet test"; pkill -f testhost; pkill -f "MSBuild.dll"; pkill -f VBCSCompiler
sleep 2 && dotnet build-server shutdown
dotnet build AgenticSdlc.Host/AgenticSdlc.Host.csproj -m:1 -p:UseSharedCompilation=false
dotnet test  AgenticSdlc.Tests/AgenticSdlc.Tests.csproj --no-build
```
</details>

---

## 2. Die drei Bedien-Ebenen

Alles läuft über das Host-Projekt:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- <befehl> [argumente]
```

| Ebene | Befehl | Rolle |
|---|---|---|
| **Graph** | `pipeline-full` | **der Betriebsweg** — die ganze Kette als ein durabler MAF-Graph |
| **Steward** | `steward` | **die Gesprächsoberfläche** darüber: startet Läufe, bedient Gates, liest die Wahrheit |
| **CLI-Werkbank** | ~85 weitere Befehle | Bau-, Debug- und Messprüfstand — jede Stufe einzeln |

Für einen Test sind **Graph und Steward** relevant. Die Werkbank ist die Genealogie des Systems:
jede Stufe wurde erst isoliert gebaut und dann in den Graphen gehoben. Siehe Abschnitt 8.

---

## 3. Wo liegt was — die vier Fragen vorweg

### Zuerst der Rahmen: System und Beispielprojekt

Dieses Repository enthält **zwei Dinge**: das System (`AgenticSdlc.*`) und **eine damit
aufgebaute Projektinstanz** — `einrichtung-fresh`, eine Kommunikations-App für eine
Pflegeeinrichtung (`projectId` im Core). Zu dieser Instanz gehören zusammenhängend:

| Baustein | Ort |
|---|---|
| die Meeting-Transkripte | `input/transcripts/` |
| die daraus autorisierte Wahrheit | `state/core/` — 237 Items |
| die publizierten Artefakte | `docs/` |
| die Laufhistorie | `runs/` + `runsArchive/` |
| die GitHub-Projektion | `armiino/Agentic-GitHub-refactor` |

**Das GitHub-Repo ist privat** (geprüft 17.09.2026: unauthentifiziert HTTP 404) — die dort
projizierten Issues sind ohne den Token des Autors nicht einsehbar. Die Belege der
GitHub-Wirkung liegen deshalb im Repo selbst: Forward-Pläne, `run-report.json` und
archivierte Issue-Rücklesungen in den jeweiligen `runs/…/07-github/`-Ordnern.

**Experimentieren ist gefahrlos.** Der ausgelieferte Core-Stand ist committet und identisch
mit der Platte (verifiziert: 237 Items in HEAD wie im Arbeitsverzeichnis). Wer per Diktat
oder Lauf die Wahrheit verändert, kommt jederzeit zurück:

```bash
git checkout -- state/core/        # den ausgelieferten Core wiederherstellen
```

Externe Wirkung ist doppelt verriegelt: `execute: false` hält GitHub-Schreibzugriffe aus,
und ohne Token schlägt selbst ein bewusst scharf geschalteter Forward fehl.

**Eigenes Ziel-Repo verwenden:** `fullworkflow.repo` und `fullworkflow.tokenEnv` in
`run-config.json` umstellen, Token in `.env` setzen, dann zuerst `github-snapshot` frisch
ziehen — veraltete Snapshots fremder Repos führen zu Drift-Fehlmeldungen.

### Die Projektwahrheit (der Core)

**`state/core/project-state.json`** — fest verdrahtet in `05-core/CorePaths.cs`, **nicht
konfigurierbar**. Das Repo-Wurzelverzeichnis wird beim Start selbst gefunden.

Aktuell (kann sich jederzeit ändern): **237 Items** (107 Anforderungen · 50 Architektur · 49 PBIs · 16 Features ·
15 Entscheidungen), **515 Beziehungen**, `schemaVersion: 4`.

| Pfad | Inhalt |
|---|---|
| `state/core/project-state.json` | die Wahrheit |
| `state/core/history/` | Schnappschuss des Vorzustands vor **jedem** inhaltsändernden Save (nicht versioniert; Restore = zurückkopieren) |
| `state/core/views/` | generierte Sichten aus `core-view` |
| `state/core-ckpt-*/`, `state/core-ux-*/` | manuell gesetzte Sicherungspunkte früherer Abnahmen |

Vor jedem Save prüft der Wächter **CoreKangal** die Invarianten — bei einem harten Verstoß
bricht der Save ab.

### Die Transkripte

Sie liegen in **`input/transcripts/`** — sechs Stück, darunter `meeting-2-extended.txt`
(der Benchmark) und `Interview-Einrichtung.txt`. Der Ordner ist Konvention, kein Zwang:
jeder Pfad funktioniert, relative Pfade gelten ab Repo-Wurzel.

**Welches Transkript genommen wird, in dieser Reihenfolge:**

1. das **Argument** hinter `pipeline-full run`, wenn eines angegeben ist
2. sonst **`run-config.json → fullworkflow.transcript`** (aktuell `input/transcripts/meeting-4-ux-block-l.txt`)
3. sonst Abbruch mit Exit-Code 2 und dem Hinweis, was fehlt

```bash
… -- pipeline-full run                                        # nimmt den Eintrag aus run-config.json
… -- pipeline-full run input/transcripts/meeting-2-extended.txt   # überschreibt ihn
```

Der Eintrag `evidenceAgent.transcript` weiter unten in derselben Datei gehört **nicht** hierher:
er steuert die Werkbank-Befehle rund um `recipe` und die Baselines. Der Kommentar über dem
Block sagt es im Detail.

### Das GitHub-Ziel

Alles in **`run-config.json → fullworkflow`**:

```jsonc
"execute": false,                                  // externe Schreibzugriffe AUS (Default)
"repo":     "armiino/Agentic-GitHub-refactor",     // wohin projiziert wird
"tokenEnv": "GITHUB_AGENTIC_REFACTOR_TOKEN"        // NAME der Umgebungsvariablen mit dem Token
```

`tokenEnv` enthält **nicht** den Token, sondern den **Namen der Variablen**, die ihn hält —
gesetzt wird sie in `.env`. Ist `tokenEnv` leer, greift die Reihenfolge
`GITHUB_TEST_TOKEN` → `githubtoken`.

> **Wichtig:** Das Flag `--repo owner/name` steuert **nur**, aus welchem Repo `--from-github`
> liest. **Geschrieben wird immer nach `fullworkflow.repo`.** Wer das Ziel ändern will, ändert
> die Konfiguration — nicht das Flag.

### Die Ergebnisse

`runs/<stufe>/<runId>/`, wobei die RunId das Muster `JJJJMMTT_HHMMSS_hex6` hat. Ein
Betriebslauf legt je Stufe einen Unterordner an (`01-ledger`, `07-ingest`, `07-decision`,
`07-github`, …) plus `checkpoints/`, `logs/` und `config.json` — den eingefrorenen Stand
der Konfiguration dieses Laufs. Ältere Läufe liegen unter `runsArchive/`.

Der Steward legt ab unter `state/steward/`: `author-front/` (die aus Diktaten erzeugten Deltas),
`sessions/` und `sweep-answers/`.

---

## 4. Erste Schritte — nach Kosten und Risiko gestaffelt

> **Schritt-für-Schritt-Anleitung:** [`howtouse.md`](howtouse.md) führt 1:1 durch die
> Steward-Bedienung — mit einem echten Beispiel-Dialog, den Zustimmungsfragen im Wortlaut
> und der Erklärung, was bei jedem Schritt wo auf der Platte passiert.

### Stufe 0 · gratis, verändert nichts

Kein API-Schlüssel nötig.

```bash
… -- pipeline-full status     # wo steht das System? pausierte Läufe, Parkplatz, Kangal-Lage
… -- pipeline-full run --dry-run    # baut den ganzen Graphen, ruft kein Modell auf
… -- core-view active-backlog       # Sicht auf die Wahrheit nach state/core/views/
```

`pipeline-full status` ist der beste erste Befehl: er zeigt wartende Gates samt
Weiter-Kommando, offene Entscheidungen, Klärungsbedarf und das Ergebnis des Integritätswächters.

### Stufe 1 · den Steward fragen (Cent-Bereich, liest nur)

Ab hier braucht es `.env` mit `OPENROUTER_API_KEY`.

```bash
… -- steward --session mein-test
… -- steward --once "Wie ist die Lage?"
```

Der Steward meldet beim Start von sich aus wartende Reviews, die Kangal-Lage und pausierte
Läufe. Gute Einstiegsfragen: *„Was steht gerade offen?"* · *„Zeig mir PBI-046"* ·
*„Welche Entscheidungen sind offen?"*

**Bedienung im Chat:** `/exit` beendet die Sitzung (steht auch im Startbanner). Will ein
zustimmungspflichtiges Werkzeug ⚿ laufen, fragt der Steward `Zustimmen? (ja/nein) >` — nur ein
frisch getipptes `ja` gibt frei; gepufferte Eingaben aus einem Paste werden vorher verworfen.
Im `--once`-Modus gibt es keine Zustimmungen: der Steward nennt stattdessen den Befehl zum
interaktiven Fortsetzen.

### Stufe 2 · ein Diktat durch die Kette ⚠ ändert den Core

**Das ist der Kernkreislauf — wer ihn einmal gefahren ist, hat das System verstanden.**

```
Du:       Ich will, dass Angehörige Besuche online absagen können.
Steward:  [prüft Core + frühere Ablehnungen, fragt nach Warum / Akzeptanzkriterien / Rahmen]
          Fassung: „…" — passt das?
Du:       Ja.
Steward:  [save_author_statements → Delta in state/steward/author-front/]
          Soll ich den Lauf starten?  ⚿
Du:       Ja.
Steward:  Lauf 20260917_… hält am Checkpoint „Requirements-OQ-Freigabe" (ingest-gate).
          [legt jedes Item zum Urteil vor: übernehmen / ablehnen mit Begründung]
…         weitere Checkpoints, dann: „geliefert: REQ-93 → PBI-050 → Issue #46"
```

### Stufe 3 · eine bestimmte Entscheidung lösen

```
Du: Löse DEC-007.
```

Der Steward fährt einen Lauf mit dem Leer-Delta `input/deltas/leer-delta.json`, der genau am
Entscheidungs-Checkpoint hält. Billig und sehr anschaulich.

### Stufe 4 · ein Transkript durch die volle Kette 💸

```bash
… -- pipeline-full run input/transcripts/meeting-2-extended.txt
```

Fünf LLM-Stufen, sieben Gates, Laufzeitbudget 90 Minuten. **Nur per CLI** — der Steward kann
Läufe starten, aber keinen Transkript-Lauf; er sagt das von sich aus und nennt den Befehl.

Der Lauf **pausiert am ersten Gate und der Prozess endet** — das ist kein Absturz, sondern
der Checkpoint. Weiter geht es mit:

```bash
… -- pipeline-full status                        # welcher Lauf wartet wo?
… -- pipeline-full resume <runId>                # Entscheid aus Chat/UI übernehmen
… -- pipeline-full resume <runId> --accept-all   # alles übernehmen (Experimentmodus)
```

### Stufe 5 · GitHub ⚠ Außenwirkung

```bash
… -- pipeline-full run --from-github --repo owner/name    # Ernte: Issues + Kommentare als Eingang
```

Die Ernte stoppt sauber und ohne Modellkosten, wenn nichts Verarbeitbares da ist. Der
Forward am Ende jedes Laufs läuft als **Dry-Run**, solange `execute: false` steht.

---

## 5. Der Steward

**Die vollständige Bedienungsanleitung — was du sagst, was passiert, wie Gates im Chat
funktionieren — steht in [`AgenticSdlc.Host/Steward/README.md`](AgenticSdlc.Host/Steward/README.md).**
Hier der Überblick:

Ein MAF-AIAgent mit **36 Werkzeugen, davon 17 zustimmungspflichtig** (⚿ — festgenagelt durch
einen Test). Zur Laufzeit kommen die Lesewerkzeuge des offiziellen github-mcp-servers dazu,
sofern Token und Netz vorhanden sind — serverseitig auf **readonly** gefiltert, mit einer
Wache im Code, die bei sichtbaren Schreibwerkzeugen laut abbricht. Abschaltbar über
`run-config.json → steward.githubLive: false`.

**Seine Grundgesetze:** Er schreibt **nie** selbst Wahrheit. Er beantwortet **nie** ein Gate —
er legt vor, du entscheidest. Teure Läufe startet er nur nach deiner ausdrücklichen Zustimmung.
Und er liest immer frisch aus dem Core, nie aus dem Gesprächsgedächtnis.

| Gruppe | Beispiele |
|---|---|
| **Lage lesen** | `get_core_overview` · `list_core_items` · `get_core_item` · `list_paused_runs` · `read_run_report` · `search_rejections` („schon einmal abgelehnt, warum") |
| **Diktat sichern** | `save_author_statements` (Anforderung · Architektur · Frage · Risiko) · `save_sweep_answers` |
| **Läufe starten** ⚿ | `run_pipeline_from_delta` · `run_pipeline_from_github` · `run_clarify_via_graph` · `run_core_analysis` · `resume_run` · `run_reproject` |
| **Gates bedienen** ⚿ | `submit_ingest_gate_decisions` · `submit_decision_gate_resolutions` · `submit_paused_gate_decisions` · `open_gate_ui` |
| **Autor-Artefakte** ⚿ | `draft_authored_doc` / `save_authored_doc` für Vision, Personas, Glossar, C4, Story Map |
| **Analyse** ⚿ | `run_core_analysis` · `read_analysis_report` · `curate_analysis_delta` |

**Sitzungen:** `--session <name>` — ein neuer Name beginnt eine Sitzung, ein bekannter setzt
sie fort. Voreinstellung ist die **volle Historie**; sie wird je Zug gespeichert. Pro
Arbeitsthema eine frische Sitzung.

**Was er bewusst nicht kann:** ein Transkript starten (CLI-Grenze, er sagt es) · direkt nach
GitHub schreiben (nur über die Forward-Policy) · die Bootstrap-Gates im Chat bedienen.

---

## 6. Gates verstehen

Sieben Human-Gates im Betriebszweig, in dieser Reihenfolge:

```
ingest-gate → arch-ingest-gate → arch-classify-gate → adr-gate
            → decision-gate → pbi-gate → github-forward-gate
```

**Ein Lauf pausiert am Gate und der Prozess endet.** Der Checkpoint bleibt liegen;
`pipeline-full status` zeigt ihn, `resume` fährt weiter. Läuft ein Gate ohne Entscheid, wird
unverändert **neu pausiert** — nie stillschweigend zugestimmt, nie hängengeblieben.

| Checkpoint (Bediener-Name) | Technik-ID | Kanal |
|---|---|---|
| Requirements-OQ-Freigabe | `ingest-gate` | **Chat** oder UI |
| Architektur-Freigabe | `arch-ingest-gate` | **Chat** oder UI |
| Architektur-Rollen-Einordnung | `arch-classify-gate` | UI |
| Architektur-ADR-Bestätigung | `adr-gate` | UI |
| Offene Entscheidungen | `decision-gate` | **Chat** oder UI |
| Product-Backlog-Änderungen | `pbi-gate` | UI |
| GitHub-Freigabe | `github-forward-gate` | **Chat** oder UI |

**Faustregel: Urteil geht im Chat, Redaktion in der UI.** Die UI startet einen lokalen Server
auf `127.0.0.1` mit einem freien Port und zeigt Vorher/Nachher je Issue sowie den Doc-Diff
je Datei. Die Optionen je Gate liefern beide Kanäle aus derselben Quelle — Chat und UI zeigen
immer dieselben Wahlmöglichkeiten.

**Die Sicherheitsvorgaben im Auslieferungszustand:**

- **`execute: false`** — GitHub-Schreibzugriffe sind aus; der Forward läuft als Dry-Run.
- **`policyProfile: "interactive"`** — ein unbekanntes Gate führt zu einem lauten Abbruch,
  nie zu stiller Zustimmung. `accept-all` und `replay` sind ausgewiesene Experimentmodi.

---

## 7. Umgebungsvariablen

```bash
cp .env.example .env        # dann OPENROUTER_API_KEY eintragen (Schlüssel: openrouter.ai → Keys)
```

Ohne `.env` laufen Build, Tests und Smoke-Netz vollständig. Der erste LLM-Befehl bricht ohne
Schlüssel laut und eindeutig ab: `OPENROUTER_API_KEY is required when LLM_PROVIDER=openrouter`
(`Llm/ChatClientFactory.cs:48`). Alternativ trägt der Code einen lokalen Ollama-Weg
(`run-config.json → llmProvider: "ollama"` + `OLLAMA_BASE_URL`); ausgeliefert und erprobt ist
`openrouter`. Gelesen werden:

| Variable | wofür |
|---|---|
| `OPENROUTER_API_KEY` | alle LLM-Aufrufe (Anbieter und Modell stehen in `run-config.json`) |
| `GITHUB_AGENTIC_REFACTOR_TOKEN` | das in `fullworkflow.tokenEnv` eingetragene Ziel-Repo |
| `GITHUB_TEST_TOKEN`, `GITHUB_TOKEN` | Rückfall-Zugänge — die genauen Reihenfolgen stehen als Kommentare in `.env.example` |

Der Denk-Faden-Mitschnitt (`CAPTURE_REASONING`) ist nur der Rückfall für
`run-config.json → observability.captureReasoning` — und der steht bereits auf `enforced`;
die Umgebungsvariable hat im Auslieferungszustand keine Wirkung.

---

## 8. Wo der Code liegt

`AgenticSdlc.Host/FullWorkflow/` — **die Ordner sind die Kette**, die Nummern die Reihenfolge.
**Jeder hat ein eigenes README, das erklärt, was in dieser Stufe passiert und warum.**

| Ordner | Aufgabe |
|---|---|
| `01-ledger/` | Transkript → belegte, facettierte Claims + Human-Adjudikation |
| `02-baselines/` | freigegebene Evidenz → geprüfte Artefakt-Baselines |
| `03-gap/` | Open-World-Abdeckungsprüfung (optional) |
| `04-delta/` | die gemeinsame ProjectState-Sprache; Bau von Erst-State und MeetingDeltas |
| `05-core/` | **die Wahrheit** hinter dem `ICoreRepository`-Port, plus CoreKangal |
| `06-backlog/` | Baseline → Readiness → PBI-Schnitt → Issue-Planung |
| `07-tore/` | die kontrollierten Zugänge: `ingestion` · `pbiupdate` · `decision` · `github` |
| `08-pipeline/` | der durable Super-Workflow über den Toren — **enthält die Schienennetz-Karte** |
| `09-analyst/` | der Core-Analyst, neben der Kette, ohne eigenen Schreibzugriff |
| `10-armf/` | freier Ziel-Agent als Mess-Arm |

`AgenticSdlc.Host/FullWorkflow/README.md` erklärt die Kette als Ganzes und ist der beste
zweite Einstieg nach diesem Dokument.

### Die CLI-Werkbank

Neben `pipeline-full` und `steward` sind **87 Befehle** registriert — jede Stufe der Kette
einzeln, für Debugging, Replay und Messung. Die vollständige Liste gibt das Programm selbst
aus, wenn man einen unbekannten Befehl übergibt:

```bash
… -- hilfe          # gruppierte Landkarte aller Befehle auf stderr
```

Die Gruppen: `ledger-*` (12) · `github-*` (14) · `l4-*` (10) · `decision-*` (7) · `core-*` (6) ·
`ingest-*` (5) · `l3-*` (4) · `pbi-update-*` (4). **Diese Bahnen sind Prüfstand, nicht
Betriebsweg** — für einen Systemtest genügen Graph und Steward.

### Alles andere

| Pfad | Inhalt |
|---|---|
| `docs/` | die publizierten Artefakte — **generiert aus dem Core, nie von Hand pflegen**: Vision, Personas, C4, Story Map, Anforderungen, Backlog, Architektur, ADRs |
| `input/transcripts/` | die sechs Eingabe-Transkripte |
| `input/deltas/` | Fixtures, u. a. `leer-delta.json` für gezielte Gate-Läufe |
| `input/eval-labels/` | Referenz-Labels und Auswertungsregeln der Messungen |
| `runs/` | Laufartefakte — über 12.000 Dateien; Wegweiser: `runs/README.md` |
| `runsArchive/` | ältere Läufe (8.381 Dateien), Struktur 1:1 wie `runs/` |
| `thesis-evidence/` | Belegpakete zur Arbeit, mit eigenen Nachrechenskripten |
| `tools/` | `smoke-hitl.sh` und die Auswertungsskripte unter `tools/eval/` |
| `archive/` | abgelöste Codestände — **nicht Teil des Builds** |

> **Zum Umfang:** rund 19.700 der gut 20.800 versionierten Dateien sind Laufartefakte.
> Der Quellcode liegt in `AgenticSdlc.Host/` (536 Dateien) und `AgenticSdlc.Tests/`
> (131 Dateien). Wer den Code sucht, fängt dort an — nicht in `runs/`.

**Projektmappe:** `Agentic-SDLC.slnx` mit **Host** (Kette und CLI), **McpServer** (eingefrorener
MCP-Spike aus der Exploration — vom System nicht verwendet, siehe dessen README; die lebende
MCP-Nutzung ist der offizielle github-mcp-server via `Host/Mcp/`) und **Tests**.
`AgenticSdlc.HumanReview` (die lokale Review-Oberfläche) wird als Abhängigkeit des Host mitgebaut.

**Historie:** Die Tags `v-s0-phase1` … `v-s9-maf-native-hitl` markieren die Entwicklungsstationen.
