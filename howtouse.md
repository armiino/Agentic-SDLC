# How to use — den Steward bedienen, Schritt für Schritt

> Status: LEBEND (angelegt 17.09.2026) — Ergänzung zum [README](README.md): dort steht das *Was*,
> hier steht das *Wie*, als 1:1-Anleitung. Jede Angabe ist am Code verifiziert (Dateiverweise
> jeweils in Klammern); der Dialog in Beispiel 1 ist die wörtliche Ausgabe eines echten Laufs
> vom 17.09.2026.

---

## 0. Was du wissen musst, bevor du tippst

**Die Wahrheit liegt an genau einer Stelle:** `state/core/project-state.json` — fest verdrahtet
(`05-core/CorePaths.cs`), nicht konfigurierbar. Alles andere (GitHub-Issues, `docs/`, Backlog,
Story Map) sind erzeugte **Sichten** darauf. Der Steward kann diese Datei **nie direkt
schreiben** — jede Änderung läuft über einen Workflow mit menschlichen Freigabe-Stellen (Gates).

**Vor jedem inhaltsändernden Speichern** passiert zweierlei automatisch: ein Schnappschuss des
Vorzustands nach `state/core/history/` und die Invariantenprüfung **CoreKangal** — bei hartem
Verstoß bricht das Speichern ab. Rückweg jederzeit:

```bash
git checkout -- state/core/        # den ausgelieferten Core wiederherstellen (237 Items)
```

**Was der Steward wo ablegt** (alles unter `state/steward/`, lokal):

| Pfad | Inhalt |
|---|---|
| `state/steward/sessions/<name>.json` | dein Gesprächsverlauf, je Zug gespeichert |
| `state/steward/author-front/<zeitstempel>-delta.json` | aus deinen Diktaten erzeugte Eingangspakete |
| `state/steward/sweep-answers/` | deine Klärungsantworten |
| `runs/steward/<runId>/` | Logs jedes Steward-Aufrufs |

**Voraussetzung:** `.env` mit `OPENROUTER_API_KEY` (`cp .env.example .env`, Schlüssel von
openrouter.ai → Keys; ohne ihn bricht der erste LLM-Befehl mit einer eindeutigen Meldung ab,
nichts hängt still). GitHub-Token nur
für GitHub-Aktionen nötig; Schreibzugriffe sind per `run-config.json → fullworkflow.execute:false`
ohnehin aus.

---

## 1. Starten und beenden

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- steward --session mein-thema
```

Die erste Zeile ist das Banner — so liest du es (Format: `StewardChatRunner.cs:208`):

```
[steward] Sitzung 'mein-thema' (neu) · github-live=6 Lese-Tools (MCP readonly)
          · Logs: runs/steward/20260917_… · /exit beendet
```

- `(neu)` / `(fortgesetzt)`: gleicher `--session`-Name setzt fort, neuer Name beginnt frisch.
  **Pro Arbeitsthema eine frische Sitzung** — der ganze Verlauf geht bei jedem Zug mit ans
  Modell; ab 150 KB warnt das Banner mit ⚠ und empfiehlt eine neue Session
  (`StewardChatRunner.cs:114`).
- `github-live=…` erscheint nur mit GitHub-Token und Netz; ohne fällt das Live-Lesen still aus,
  alles andere läuft weiter.
- **`/exit`** beendet. Die Sitzung bleibt gespeichert und ist unter demselben Namen fortsetzbar.

**Einmal-Frage ohne Chat** (skriptbar, kann nichts freigeben):

```bash
… -- steward --once "Wie ist die Lage?"
```

---

## 2. Beispiel 1 — Lage abfragen (echter Lauf vom 17.09.2026, wörtlich)

```
$ … -- steward --once "Wie ist die Lage?"
[steward] Sitzung 'default' (fortgesetzt) · Session 135 KB · github-live=6 Lese-Tools (MCP readonly)
          · Logs: runs/steward/20260917_095216_4f7c26 · /exit beendet
  ⋯ 1: get_core_overview

steward> Ich lese den Überblick der Projektwahrheit, damit ich dir die aktuelle Lage knapp
und belastbar zusammenfasse (get_core_overview).
Aktuelle Lage:

- **Projektwahrheit:** 237 Items — 107 Requirements, 49 PBIs, 50 Architektur-Items,
  16 Features, 15 Decisions
- **Offen:** 8 offene Decisions (DEC-001, DEC-002, DEC-004, DEC-006, DEC-007, DEC-011,
  DEC-012, DEC-013) und 12 needs_clarify-PBIs (PBI-023, PBI-027, …)
- **Sauberkeit/Nächster Hinweis:** keine blockierten PBIs, keine wartenden Reviews,
  Kangal 0 Fehler / 0 Warnungen; außerdem wartet die Story Map auf Nachzeichnung:
  docs/storymap.md hat 9 PBI(s) ohne Einordnung
```

**Was hier passiert ist:** Die Zeile `⋯ 1: get_core_overview` zeigt den Werkzeugaufruf — der
Steward hat die Zahlen **nicht aus dem Gedächtnis**, sondern frisch aus dem Core gelesen
(Grundgesetz im Prompt: nie aus der Sitzung beantworten, was ein Werkzeug aktuell liefern kann).
Lesen kostet keine Zustimmung. Gute Folgefragen: *„Zeig mir PBI-046"* · *„Welche Entscheidungen
sind offen?"* · *„Was steht in DEC-007?"*

---

## 3. Beispiel 2 — ein Diktat durch die Kette (der Kernkreislauf)

*Beispielhafter Verlauf; die Mechanik je Schritt ist code-verifiziert.*

**Schritt 1 — du formulierst ein Anliegen:**

```
du> Ich will, dass Angehörige Besuche online absagen können.
```

Der Steward prüft zuerst den Bestand (`list_core_items`, `search_rejections` — „wurde so etwas
schon einmal abgelehnt, und warum?") und coacht die Fassung: Warum? Akzeptanzkriterien? Rahmen?
Dann liest er dir die Fassung vor.

**Schritt 2 — er sichert das Diktat:** `save_author_statements` schreibt ein Delta nach
`state/steward/author-front/<zeitstempel>-delta.json`. **Das ist noch keine Wahrheit** — nur ein
Eingangspaket mit Herkunft „Autor-Diktat".

**Schritt 3 — die Zustimmungsfrage (⚿):** Für den Lauf-Start fragt die Konsole direkt:

```
[steward] Tool 'run_pipeline_from_delta' will laufen (deltaPath: state/steward/author-front/…).
Zustimmen? (ja/nein) >
```

Nur ein **frisch getipptes `ja`** gibt frei — gepuffert eingefügte Zeilen aus einem Paste werden
vorher verworfen (`StewardChatRunner.cs:231-237`). Es gibt bewusst keine Prosa-Vorfrage davor:
diese Abfrage *ist* die Bestätigung. Im `--once`-Modus erscheint stattdessen:
`ZUSTIMMUNG NÖTIG für Tool '…' — bitte interaktiv fortsetzen: steward --session <name>`
(`StewardChatRunner.cs:225`).

**Schritt 4 — der Lauf startet im Hintergrund** desselben Prozesses und meldet sofort seine
runId (`StewardRunTools.cs:480 ff.`). Du kannst weiterfragen. Zwei Läufe gleichzeitig gehen
nicht — der zweite Start wird mit `STEWARD_BUSY` abgelehnt (`StewardRunTools.cs:244`).

**Schritt 5 — der Lauf hält am ersten Checkpoint** (so heißen die Gates im Gespräch):

```
steward> Lauf 20260917_… hält am Checkpoint „Requirements-OQ-Freigabe" (ingest-gate).
         1 Vorlage: REQ-Vorschlag „Angehörige können Besuche online absagen …"
         Hier im Chat entscheiden oder die Review-UI öffnen?
```

Die Optionen (übernehmen/ablehnen, je mit Wortlaut) kommen **aus dem Werkzeug selbst** — aus
derselben Quelle wie die UI (`StewardGateVocabulary.cs`), Chat und UI zeigen also immer dasselbe.

**Schritt 6 — du entscheidest, als ein Sammel-Akt:**

```
du> Übernimm den Vorschlag.
```

`submit_ingest_gate_decisions` (⚿) reicht **alle** Urteile auf einmal ein; was du nicht nennst,
ist vertagt, eine Ablehnung braucht immer eine Begründung. Der Submit kettet die Fortsetzung
automatisch — der Lauf fährt zum nächsten Checkpoint oder bis „fertig".

**Schritt 7 — das Ergebnis:** Nach den restlichen Checkpoints meldet `read_run_report`, was
geliefert wurde — z. B. neues Requirement → angepasstes Arbeitspaket → (bei `execute:true` und
Token) aktualisiertes GitHub-Issue. Erst der **Apply nach deiner Freigabe** hat den Core
verändert; kontrollieren: `„Wie ist die Lage?"` oder `git diff state/core/`.

---

## 4. Beispiel 3 — eine bestimmte offene Entscheidung lösen

```
du> Löse DEC-007.
```

Der Steward fährt einen Lauf mit dem **Leer-Delta** `input/deltas/leer-delta.json` — der speist
nichts Neues ein und hält direkt am Checkpoint „Offene Entscheidungen". Dort legt er genau diese
DEC mit Kontext vor (Ziel-Item, betroffene PBIs, „Schon-einmal-abgelehnt"-Hinweis); du löst mit
den vorgesehenen Optionen (behalten / übernehmen / neu formulieren) oder vertagst. Vertagen ist
ein gültiger Ausgang. (Rezept im Steward-Prompt, `StewardAgent1.txt:112 ff.`)

---

## 5. Wenn ein Lauf wartet — Pause ist kein Absturz

Ein Lauf, der auf dich wartet, beendet seinen Prozess; der Zustand liegt als Checkpoint auf
Platte. Nachsehen und fortsetzen — im Chat (*„Welche Läufe warten?"* → `list_paused_runs`,
`resume_run`) oder direkt:

```bash
… -- pipeline-full status              # wer wartet an welchem Checkpoint + Weiter-Kommando
… -- pipeline-full resume <runId>      # Entscheid aus Chat/UI übernehmen und weiterfahren
```

Ohne vorliegenden Entscheid pausiert der Lauf **unverändert erneut** — nie stille Zustimmung,
nie hängen (`PIPELINE_STILL_PAUSED`, `PipelineFullRunner.EventPump.cs:271`). Der Steward prüft
vor jedem Neustart selbst, ob schon ein Lauf wartet, und empfiehlt dann das Weiterführen.

---

## 6. Spickzettel — was du sagst, was er tut, wo es landet

| Du sagst… | Er nutzt… | Ergebnis landet in… |
|---|---|---|
| „Wie ist die Lage?" | `get_core_overview` | Antwort im Chat |
| „Zeig mir PBI-046" | `get_core_item` | Antwort (wörtlicher Text + Beziehungen) |
| „Ich will, dass …" | `save_author_statements` → ⚿ `run_pipeline_from_delta` | Delta in `state/steward/author-front/`, Lauf in `runs/fullworkflow/` |
| „Löse DEC-xxx" | Leer-Delta-Lauf bis zum Entscheidungs-Checkpoint | Auflösung im Core, nach deinem Entscheid |
| „Was ist neu auf GitHub?" | ⚿ `run_pipeline_from_github` (pullt selbst; stoppt leer ohne Modellkosten) | Bericht via `read_run_report` |
| „Analysiere den Core auf Lücken" | ⚿ `run_core_analysis` (4–5 Prüf-Linsen) → `read_analysis_report` | `runs/core-analysis/<id>/` — Übernahme nur per separatem ⚿-Tor-Lauf |
| „Erstelle eine Vision" | ⚿ `draft_authored_doc` → Vorlage → ⚿ `save_authored_doc` = Freigabe | `docs/vision.md` mit Versions-/Freigabe-Kopf |
| „Öffne die UI" | ⚿ `open_gate_ui` / `open_review_ui` | Browser auf `127.0.0.1` (freier Port); „Fertig" setzt den Lauf fort |
| „Verarbeite dieses Transkript" | **kann er nicht** — er nennt dir ehrlich den Befehl | `pipeline-full run <transkript>` (nur CLI) |

**Faustregel für Gates:** Urteil im Chat, Redaktion (Mehrfeld-Edit, Diffs) in der UI.

---

## 7. Worauf du dich verlassen kannst

Der Steward **schreibt nie selbst Wahrheit** und **beantwortet nie ein Gate** — er legt vor, du
entscheidest. Die ⚿-Zustimmung, einen Lauf zu *starten*, ist **keine** Freigabe seines späteren
Änderungsvorschlags — das sind zwei getrennte Schlösser. Er liest immer frisch über Werkzeuge
statt aus dem Sitzungsgedächtnis. Und GitHub ist Projektion: solange `execute:false` steht,
verlässt nichts das System (Forward läuft als Dry-Run; ohne Token schlägt selbst ein scharf
geschalteter Write hart fehl, `GithubForwardApply.cs:46`).

**Vertiefung:** Werkzeug-Referenz und Gate-Details → [`AgenticSdlc.Host/Steward/README.md`](AgenticSdlc.Host/Steward/README.md) ·
Architektur der Kette → [`AgenticSdlc.Host/FullWorkflow/README.md`](AgenticSdlc.Host/FullWorkflow/README.md)
