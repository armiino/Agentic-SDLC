# Phase 1 Technischer & methodischer Report als zsmfassung

## 1. Ziel und Scope von Phase 1

**Ziel:** Ein technischer Durchstich, bei dem ein einzelner Agent aus unstrukturierten Stakeholder-Transkripten frühe SDLC-Artefakte erzeugt; "agentisch" in diesem Kontext: der Agent entscheidet selbst, welche Tools er nutzt. Der Host darf keine Pipeline “skripten”, sondern stellt nur Umgebung, Tools und Outputs bereit. 
-> wenn der Host mehr übernimmt nähert man sich einer RPA. Dass will ich verhindern und es agentsich wie nur möglich darstelln.
-> TODO: in der Arbeit anhand der "Definition" eines Agenten orientieren!

**Input:** Texttranskripte (Kickoff etc.) unter `input/transcripts/` (hier: `T0001.txt` usw.)

**Output (Repo):**
- `docs/requirements.md`
- `docs/open-questions.md`
- `docs/risks.md`
- `docs/architecture.md`  
  (Option: `docs/issues.json` als Draft)

**Output (Run-Artefakte):**
- `runs/<runId>/config.json`
- `runs/<runId>/logs/events.jsonl`
- `runs/<runId>/logs/tool-discovery.json`
- `runs/<runId>/approvals/approval_*.json`
- `runs/<runId>/snapshots/docs/*`

**Governance:** Aktionen (Commit/Push, GitHub Issues) sind in Phase 1 nicht aktiv. Stattdessen erzeugt der Agent eine Approval-Request als Datei. (für später bwzüglich HumanInTheLoop).


## 2. Systemübersicht: Komponenten und Verantwortlichkeiten

Phase 1 besteht aus zwei ausführbaren Projekten:

### 2.1 AgenticSdlc.Host (Host / Run Driver)
Der Host:
1. Generiert `runId` und legt Run-Struktur an.
2. Verbindet Local MCP Server (Pflicht) und GitHub MCP Server (erst mal nur Discovery).
3. Discovert Tools und speichert `tool-discovery.json` (welche gibts überhaupt zur verfügung).
4. Baut LLM-Client und aktiviert Function/Tool-Invocation (nötig für middleware-logging etc. Invocation sehr sehr wichtiges feature).
5. Baut den MAF Agent, übergibt Tool Allowlist, startet mit “Begin.”.
6. Loggt Toolcalls (events.jsonl) und snapshot’t `docs/`.

**Wichtig:** Der Host erzeugt keine SDLC-Inhalte..nur Infrastruktur (trennung für mehr "agentisch" überlegt)

### 2.2 AgenticSdlc.McpServer (Local MCP Tool Server)
**Aufgabe:** Tool-Schicht + serverseitige Policies.  
Der MCP Server stellt Tools bereit:
- Filesystem: `fs_list`, `fs_exists`, `fs_read`, `fs_write`
- Governance: `request_approval`

Zusätzlich erzwingt er Root-Policies (ReadRoots/WriteRoots) und blockt absolute Pfade.

**Warum diese Trennung?**
MCP ist in diesem Projekt bewusst die Tool-Schicht. Policies und Governance müssen serverseitig "enforced" sein, damit sie nicht nur “Prompt-Regeln” sind.
TODO: in der Arbeit detailierter beschreibung warum policies nötig.. promptinjection etc.

## 3. Ordnerstruktur und Zweck

### 3.1 Repo-Ebene
- `input/` – Hier sind die Transskripte aus den Gesrpächen: Werden nur gelesen.  
- `docs/` – generierte SDLC-Artefakte (Outputs).  
- `runs/` – Forschungs-/Reproduzierbarkeitsartefakte pro Ausführung (RunId, Logs, Approvals, Snapshots..).

### 3.2 Host-Projektstruktur (Beispiel)
- `Host/Run/*` – RunContext, RunId, File-Layout, config/events
- `Host/Mcp/*` – Verbindungsaufbau zu MCP (Local + GitHub), Tool discovery.
- `Host/Observability/*` – Middleware/Logging für Toolcall-Events. (an invocation gehängt.. TODO: überlegung telemetry observability von MAF?)

### 3.3 MCP-Server-Projektstruktur (Beispiel)
- `McpServer/Tools/*` – Tool-Implementierungen (Filesystem, Approval).
- `McpServer/Infrastructure/*` – RootPolicy + RepoRoot (Sicherheitsgrenze).


## 4. End-to-End "Call-Flow" (was passiert beim Run?)

### 4.1 Run Start (Host)
1. `RunId.New()` erzeugt eine eindeutige RunId
2. `RunContext.EnsureFolders()` legt Run-Verzeichnisstruktur an
3. `config.json` wird gespeichert (klassische config details)
4. `events.jsonl` bekommt ein `RUN_STARTED` Event etc. hier genaue infos zur Nachvollziehbarkeit

### 4.2 MCP (Host)
1. `McpConnections.ConnectLocalAsync()` startet den lokalen MCP Server als Subprozess (über stdio..).
2. hier noch optional `ConnectGitHubAsync()` für Discovery (keine Nutzung in Phase 1 aber elemantar für den eigentlichen plan).
3. Host ruft `localMcp.ListToolsAsync()` und schreibt `tool-discovery.json`.

### 4.3 Tool Allowlist -> Agent (Host -> MAF)
1. Die gefundenen MCP Tools werden als `AITool` gesammelt. (hier "cast" benutzt..)
2. Der Agent wird gebaut "und erhält:
    - Zielbeschreibung + Output "Contracts"
    - Constraints (ReadRoots/WriteRoots)
    - Tool Allowlist (nur discoverte Tools)
    - Startprompt: “Begin.” (dummy)

### 4.4 LLM Client + Function Invocation (Host)
Der Host nutzt `IChatClient` und baut eine "Pipeline" mit `UseFunctionInvocation()`.
-> IChatClient war nötig.. openAI API calls lieferten nur JSONs statt echte toolcalls..
-> Docs von MAF sehen dies auch vor bei der nutzung von lokalen KI-modellen (ollama)
**Wirkung:**
- Toolcalls werden nicht nur als Text erzeugt,
- sondern automatisch ausgeführt (Function Invocation),
- und die Ergebnisse werden wieder in den Agenten-Kontext zurückgeführt.

Das ist zentral, um agentischen Tool-Use im Log zu sehen (`TOOL_CALL_*`)

### 4.5 Toolcalls (Agent -> MCP -> Tools)
Typischer Ablauf (agentisch, nicht vom Host geskriptet):
1. Agent ruft `fs_list("input/transcripts/")` auf -> findet Transkripte.
2. Agent ruft `fs_read("input/transcripts/T000X.txt")` auf -> liest Inhalte.
3. Agent erzeugt SDLC Artefakte und schreibt sie mit `fs_write("docs/...")`.
4. Agent ruft `request_approval(action="phase1_review", payload=...)` auf -> Approval-Datei wird im Run gespeichert.

### 4.6 Observability (Host)
Jeder Toolcall wird in `runs/<runId>/logs/events.jsonl` protokolliert:
- `TOOL_CALL_STARTED` (tool + args)
- `TOOL_CALL_FINISHED` (result / error)
-> nötig um zu beweisen wie die toolcalls waren..

### 4.7 Snapshot (Host)
Am Ende kopiert der Host `docs/` nach `runs/<runId>/snapshots/docs/`.
Damit sind Artefakte gesaved pro Run (denke wichtig für verlgeiche später) 


## 5. Root Policies (ReadRoots/WriteRoots):

### 5.1 Zweck
Root Policies definieren eine technische "Sandbox":
- **ReadRoots:** `input/`, `docs/`, `runs/`
- **WriteRoots:** `docs/`, `runs/`
- **Absolute Pfade verboten** (nur relative Pfade erlaubt) -> hier als beweis das agent eingeschränkt ist..

Also: Tool-Autonomie möglich, ohne dass der Agent beliebige Pfade lesen/schreiben kann.

### 5.2 Warum serverseitig?
Weil Policies sonst “nur Text im Prompt” wären. Serverseitig bedeutet:
- Tools verweigern Operationen technisch,
- der Host kann nicht “aus Versehen” was anders machen,
- und die Logs zeigen echte Failures (isError=true)..

### 5.3 Explorative Evidenz: konkreter Failure Mode
Ein beobachtetes Problem war: `fs_list` lieferte absolute Pfade, die RootPolicy blockte.
Evidenz war im Run-Log sichtbar als `isError=true` bei `fs_read`/`fs_exists`.
Fix: `fs_list` wurde so angepasst, dass es repo-relative Pfade zurückgibt.

Damit wurde die RootPolicy nicht entfernt, sondern korrekt konfiguriert (Policy bleibt strikt)

## 6. Governance / Approval: was passiert und warum?

### 6.1 Ziel
Gewisse Aktionen müssen human-in-the-loop sein. (Auf GIT später bezogen..)
In Phase 1 wird nur eine Approval Request Datei erzeugt, aber kein Push/Commit ausgeführt.

### 6.2 Mechanik
- Agent ruft `request_approval(action, payload)` auf.
- MCP Server schreibt `runs/<runId>/approvals/approval_*.json`.
- erste IDee: Human kann später `approved=true` setzen (oder später Phase 2+: approval.check).

**Warum nötig in Phase 1?**
Weil ich finde das Governance als Prinzip bereits in Phase 1 sichtbar sein soll


## 7. MAF vs Custom: Was ist Framework und was ergänze ich bewusst. 

### 7.1 MAF 
Phase 1 nutzt MAF als agentisches "Rückgrat":
- Agent als zentrale Einheit (nicht Host Pipeline)
- Tools werden am Agent registriert (AITool Allowlist)
- Middleware/Observability im Agent Host
- "Multi-turn" Agent run (`RunAsync`) (hier genauer beschreiben..)

Zusätzlich aus Microsoft.Extensions.AI:
- `IChatClient` Abstraktion
- `ChatClientBuilder`
- `UseFunctionInvocation()` für Tool Execution

### 7.2 Custom (bewusst minimal)
- MCP Tool Implementierungen (`fs_*`, `request_approval`)
- RootPolicy (ReadRoots/WriteRoots, relative-only) (war nötig)
- RunContext (config + events.jsonl + snapshots) (docs zwecke..)
- ToolCallLoggerMiddleware (nur Logging, keine Domänenlogik) (telemetiry über MAF möglich)


## 8. Explorative Methodik:


### 8.1 Learning 1 – “Toolcalls als Text” nicht gleich “Toolcalls als Execution”
**Beobachtung:** Im Eventlog fehlten `fs_write`/`request_approval`, obwohl das Modell Toolcall-JSON im Text “ankündigte”.  
**Hypothese:** Tool Invocation Pipeline ist nicht aktiv.  
**Fix:** Wechsel auf `IChatClient` + `UseFunctionInvocation()` (mit OllamaSharp)
**Ergebnis:** `events.jsonl` zeigt echte `TOOL_CALL_STARTED/FINISHED` für `fs_write` + Artefakte existieren

### 8.2 Learning 2 – Policy Alignment: relative Pfade
**Beobachtung:** Toolausgabe (absolute Pfade) kollidierte mit RootPolicy (relative-only).  
**Fix:** `fs_list` liefert repo-relative Pfade.  
**Evidenz:** `fs_read` funktioniert, keine isError=true Events.

### 8.3 Learning 3 – Approval Tool Contract
**Beobachtung:** `request_approval` schlug fehl (sigrantur fehler) 
**Fix:** Stabiler Contract: `request_approval(action,payload)` + `runId` im JSON Payload; Server extrahiert runId und schreibt unter `runs/<runId>/approvals`.  
**Evidenz:** “Approval request recorded.” + `approval_*.json` existiert im Run.

### 8.4 Ergebnis: Phase-1 Baseline
Ein erfolgreicher Run zeigt:
- Tool discovery vorhanden
- Agentische Toolcalls (fs_list/read/write) im Log
- docs/ Artefakte vorhanden
- approval file vorhanden
- snapshots vorhanden


## 9. Phase-1 "ende"

1. **3 Runs mit 3 Transkripten** erfolgreich (Muss aber längere Transskripte nehmen)
2. Agent nutzt Tools autonom (Log belegt Toolcalls).
3. Artefakte vorhanden und folgen Output Contract.
4. Approval Request wird erzeugt.
5. Root Policies verhindern misuse (Test: Write außerhalb roots muss failen).
6. Iteration Notes existieren pro Run.

## 10. Referenz-Run (Beispiel)

**RunId:** `20260220_173744_53cad8`  
Belege:
- `runs/<runId>/logs/events.jsonl` enthält `fs_read`, `fs_write`, `request_approval`
- `runs/<runId>/approvals/approval_*.json` existiert
- `docs/*.md` existiert

## erweiterung mit langem/unstrukturiertem Tranksskript

**RunId:** `20260225_123947_560ab9`  
**Input:** `input/transcripts/T9999_chaos.txt` (generiert: 3 Sprecher, widersprüchliche Ziele, Prioritätskonflikte, Kontextsprünge).

### Beobachtete Probleme

1) **Nicht-deterministische Artefakt-Erstellung (mehrfache Überschreibungen)**
   In `events.jsonl` sind mehrere `fs_write`-Aufrufe auf dieselben Dateien sichtbar (z.B. `docs/requirements.md` wird mehrfach überschrieben).  
   **Implikation:** Single-Pass produziert Draft/Revisionen ohne explizite Konsolidierungsphase. Das erschwert Reproduzierbarkeit und Vergleichbarkeit zwischen Runs.

2) **Governance-Routine instabil (mehrfache Approval Requests)**
   `request_approval` wird im selben Run dreimal aufgerufen-entsprechend entstehen drei Approval-Dateien.  
    Ohne klaren Workflow-Endstep “Approval exactly once” schließt der Agent mehrfach ab 

3) **Output Contract nicht zuverlässig erfüllt**
   Im Run werden Requirements/Open-Questions/Risks geschrieben, aber `docs/architecture.md` wird nicht aktualisiert (kein `fs_write` dazu im Log).  
   Im `docs/` Ordner können Artefakte aus früheren Runs “stehen bleiben” und verfälschen den Eindruck. Daher sind Snapshots (`runs/<runId>/snapshots/docs`) bzw. eine deterministische Finalisierung zwingend.

4) **Faktenstabilität sinkt bei hoher Komplexität**
   Ein Beispiel ist ein Werte-Drift bei erwarteten Nutzerzahlen (Output enthält Werte, die im Transkript nicht vorkommen).  
   Bei langen Inputs steigt die Gefahr von Detailverlust/Drift; reine Single-Pass “Summarize & Write”-Strategien scheinen nicht robust.

5) **Traceability bleibt unvollständig**
   Der Output enthält keine robuste Traceability (Chunk-IDs/Marker) und “verschiebt” sie implizit (“coming soon”).  
   Traceability erfordert strukturelle Vorarbeit (Chunking + Referenzmarker) und ist als nachträglicher “Anhang” im Single-Pass unzuverlässig.

### Alternative Erklärungen
Ein Teil der beobachteten Instabilität (mehrfaches Approval, mehrfaches Überschreiben) ist theoretisch durch strengere Prompt-Regeln reduzierbar-> wird weiter getestet

### Ableitung: Warum Phase 2 notwendig ist
Die beobachteten Failure Modes motivieren zu einer weiteren Phase als strukturelle Weiterentwicklung:
- **Chunking & IDs** als erster Workflow-Step -> stabile Traceability.
- **Fakten/Constraints Extraktion** getrennt von “Schreiben” -> weniger Drift.
- **KonfliktSchritt** -> Widersprüche explizit statt geglättet
- **Finalisierung** -> genau einmal pro Artefakt schreiben
- **Governance Endstep** → genau einmal Approval am Ende

