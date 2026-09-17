# Steward — Bedienungsanleitung

> Status: LEBEND (README-bei-Code, angelegt 17.09.2026) — bei Tool-/Prompt-Änderungen mitpflegen.
> Quellen aller Angaben: der ausgelieferte Prompt `Prompts/steward/StewardAgent/StewardAgent1.txt`
> und die Tool-Dateien in diesem Ordner. Zeilenangaben beziehen sich auf den Prompt.

Der Steward ist die Gesprächsoberfläche über dem System: ein MAF-AIAgent mit 36 Werkzeugen
(17 zustimmungspflichtig ⚿), der Läufe startet, Gates vorlegt und die Wahrheit liest —
**aber nie selbst entscheidet.**

## Starten, beenden, Sitzungen

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- steward --session <name>
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- steward --once "<eine Frage>"
```

- **`/exit`** beendet die Sitzung (steht auch im Startbanner). Neuer `--session`-Name = neue
  Sitzung, bekannter Name = fortgesetzt; ohne Flag läuft `default`. Pro Arbeitsthema eine
  frische Sitzung — der ganze Verlauf geht sonst bei jedem Zug mit ans Modell (das Banner
  warnt ab wachsender Größe).
- **⚿-Zustimmung:** vor einem zustimmungspflichtigen Werkzeug fragt die Konsole
  `Zustimmen? (ja/nein) >`. Nur ein frisch getipptes `ja` gibt frei; gepufferte Eingaben aus
  einem Paste werden vorher verworfen (`StewardChatRunner.cs:231-237`). Es gibt **keine
  Prosa-Vorfrage** davor — die Zustimmungsabfrage selbst ist die Bestätigung
  (Ein-Bestätigungs-Regel, Prompt Z. 28).
- **`--once`** ist nicht-interaktiv: keine Zustimmungen möglich; will ein ⚿-Werkzeug laufen,
  nennt der Steward stattdessen den Befehl zum interaktiven Fortsetzen.

## Wie der Steward spricht

- Human-Stops heißen im Gespräch **Checkpoints**: „Checkpoint ‚Requirements-OQ-Freigabe'
  (ingest-gate)" (Prompt Z. 15-20).
- Jede Lage-Meldung kommt als **Kompass** (Prompt Z. 22-27):
  `✓ Gelaufen … · ▶ Jetzt: Checkpoint „…" wartet auf DICH · Kanal: Chat | UI · → Danach …`
- **Entscheidungs-Optionen kommen aus dem Werkzeug**, nicht aus dem Modell: jede Gate-Vorlage
  liefert `optionen` (Code + Label) aus derselben Quelle wie die Review-UI
  (`StewardGateVocabulary.cs`; Prompt Z. 43-44). Chat und UI zeigen immer dieselbe Auswahl.

## Die Rezepte — was du sagst, was passiert

| Du sagst… | Der Weg (Werkzeuge in Reihenfolge) |
|---|---|
| „Wie ist die Lage?" | `get_core_overview` → Kompass; wartende Reviews, Kangal, Pausen meldet er von selbst |
| „Ich will, dass …" (neue Anforderung/Architektur/Frage/Risiko) | **Autor-Front** (Z. 55-57, 251-260): Vorprüfung (Core + `search_rejections`) → Coaching (Warum/AK/Rahmen) → Fassung vorlesen → `save_author_statements` → ⚿ `run_pipeline_from_delta` → Gates |
| „Löse DEC-xxx" | **DEC-Rezept** (Z. 112-116): Lauf mit `input/deltas/leer-delta.json` → hält am Checkpoint „Offene Entscheidungen" → genau diese DEC vorgelegt, Rest vertagen |
| Antworten auf `needs_clarify`-PBIs | **Klärungs-Kreis** (Z. 58, 193-195): `collect_clarify_katalog` → Fragen im Chat → `save_sweep_answers` (wörtlich) → ⚿ `run_clarify_via_graph` |
| „Was ist neu auf GitHub?" | **Ernte** (Z. 177-186): ⚿ `run_pipeline_from_github` — pullt selbst, stoppt bei nichts Tor-Fähigem ohne Modellkosten; danach immer `read_run_report` mit den harvest-Zahlen |
| „Analysiere den Core auf Lücken" | ⚿ `run_core_analysis` (4 Linsen) → `read_analysis_report` legt Funde **wörtlich** vor, Aussortiertes mit Kritiker-Grund → Auswahl per `curate_analysis_delta(indices)` → ⚿ Tor-Lauf; Nicht-Gewähltes bleibt offen, Text ändern nur per Diktat |
| „Erstelle/aktualisiere Vision / Personas / Glossar / C4 / Story Map" | **Autor-Artefakte** (Z. 59-99): ⚿ `draft_authored_doc` → Entwurf wörtlich im Chat → Iteration → die ⚿-Abfrage von `save_authored_doc` ist die Freigabe; beim Update zeigt er nur den Unterschied |
| „Prio von PBI-x auf hoch" / „PBI-y ist ein L" | `propose_pbi_fields` (kein LLM) → wartendes pbi-Gate → `open_review_ui`; erst dein Entscheid dort setzt das Feld (Z. 99-102) |
| GitHub hängt hinter dem Core (Forward scheiterte) | **Recovery**: ⚿ `run_reproject` — Delta aus dem Core, hält am Forward-Gate; nur auf die dokumentierten Anlässe (Z. 104) |
| „Verarbeite dieses Transkript" | **Kann er nicht** — er sagt es ehrlich und nennt `pipeline-full run` (Z. 105-106) |

**Start-Vorprüfung (immer):** vor jedem Lauf-Start prüft er `list_paused_runs` — wartet schon
ein Lauf, empfiehlt er Weiterführen vor Neustart (Z. 107-109). Ein Lauf je Sitzung: parallele
Starts lehnt das Werkzeug mit `STEWARD_BUSY` ab (`StewardRunTools.cs:244`).

## Gates im Chat bedienen

An jedem Checkpoint bietet er die **Kanal-Wahl** an: im Chat klären oder UI öffnen (Z. 210).
Faustregel: **Urteil = Chat, Redaktion = UI.**

- **Chat-Gates:** Vorlage kommt item-weise mit wörtlichem Text; entschieden wird als **ein
  Sammel-Akt** — was du nicht nennst, ist vertagt (Z. 216). Ablehnung braucht immer eine
  Begründung. Submits: `submit_ingest_gate_decisions` (apply|reject),
  `submit_decision_gate_resolutions` (KEEP_ORIGINAL | ADOPT_NEW | REFINE | defer — Frage-DECs
  nur „geklärt mit Begründung" oder „vertagen", Z. 245-248), `submit_paused_gate_decisions`
  (Forward: apply|skip je Op). Der Submit kettet die Fortsetzung automatisch; danach Kompass.
- **UI-Gates** (Mehrfeld-Edit, Referenzlisten, Datei-Vorschau): Product-Backlog-Änderungen,
  Architektur-Rollen-Einordnung, ADR-Bestätigung — `open_gate_ui` startet die lokale
  Review-Oberfläche auf `127.0.0.1` (freier Port); „Fertig" dort kettet den `resume`.
  Das Forward-Gate kann seit dem R-64-Ausbau beides; die UI zeigt Vorher/Nachher je Issue
  und den Doc-Diff je Datei (Z. 223-237).
- **Nicht im Chat bedienbar:** die Bootstrap-Gates (Claim-Beleg-Prüfung, Themen-Bündelung,
  Backlog-Abnahme) — eigene UIs der Bootstrap-Bahn.

## GitHub: drei getrennte Wege

1. **Snapshot-Fragen** (`search_github_issues`): Momentaufnahme — er nennt immer Repository
   und Snapshot-Zeitpunkt, Veraltetes mit Warnnote (Z. 170-172).
2. **Live-Lesen** (MCP, falls Token/Netz): „was steht JETZT da?" — ausdrücklich als „live von
   GitHub" markiert, serverseitig readonly, nur Gespräch, nie Beleg (Z. 173-177).
3. **Verarbeitung in die Wahrheit**: ausschließlich der gestempelte Weg
   `run_pipeline_from_github` → Destillat → Gates. Er destilliert **nie** selbst im Chat
   (Z. 183). Rückfragen an ein Issue (`post_issue_comment` ⚿) liest er wörtlich vor und
   postet erst nach deinem OK.

## Grundgesetze (worauf du dich verlassen kannst)

Er schreibt nie selbst Wahrheit · er beantwortet nie ein Gate (Z. 198: „DU beantwortest das
Gate NIE selbst" — an den Agenten gerichtet) · teure Läufe nur nach ⚿-Zustimmung · er liest
immer frisch aus den Werkzeugen statt aus dem Sitzungsgedächtnis (Z. 109-110) · Sitzung ≠
Wissen: die Wahrheit wohnt im Core.

## Ablageorte

| Was | Wo |
|---|---|
| Prompt (die Verhaltensquelle) | `Prompts/steward/StewardAgent/StewardAgent1.txt` |
| Werkzeug-Code | `StewardReadTools.cs` · `StewardRunTools.cs` · `StewardGateTools.cs` · `05-core/CoreQueryTools.cs` |
| Sitzungen (lokal, nicht versioniert) | `state/steward/sessions/` |
| Diktat-Deltas / Klärungs-Antworten | `state/steward/author-front/` · `state/steward/sweep-answers/` |
| Logs je Aufruf | `runs/steward/<runId>/` |
