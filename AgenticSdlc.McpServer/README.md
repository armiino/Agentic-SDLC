# AgenticSdlc.McpServer — der frühe MCP-Spike (Exploration, eingefroren)

> Status: HISTORIE — Explorations-Station vom Februar–Mai 2026 (Ära `v-s1-phase2-dag`).
> Baut als Teil der Solution mit, wird aber **vom heutigen System nicht verwendet.**

## Was das ist

Ein eigener **MCP-Server über stdio** (`ModelContextProtocol`-SDK), gebaut in der frühen
Explorationsphase, um zwei Fragen praktisch zu beantworten: *Wie fühlt sich ein selbst
gebauter MCP-Server an?* und *Lassen sich Dateizugriff und Freigaben über eine
Werkzeug-Grenze absichern?*

## Werkzeuge

| Tool | Datei | Verhalten |
|---|---|---|
| `FsRead` / `FsWrite` / `FsList` / `FsExists` | `Tools/FileSystemsTools.cs` | Dateizugriff unter der **RootPolicy**: lesen nur aus `input/`, `docs/`, `runs/`; schreiben nur nach `docs/`, `runs/`; Lese-Deckel 5 MB. `FsWrite` nimmt `intent`/`reason`/`evidence` als Begründungsfelder mit |
| `RequestApproval` | `Tools/ApprovalTools.cs` | dateibasierte Freigabe: schreibt `runs/<runId>/approvals/approval_*.json`, höchstens eine je RunId — **derzeit nicht registriert** (in `Program.cs` auskommentiert, Freigaben wanderten zum Host) |
| `GetRandomNumber` | `Tools/RandomNumberTools.cs` | Überbleibsel der C#-MCP-Projektvorlage, nie Teil der Exploration |

Registriert ist in `Program.cs` nur `FileSystemTools`.

## Was davon weiterlebt — und was nicht

Die hier erprobten Ideen sind ins Hauptsystem gewandert, der Server selbst nicht:

- **Freigaben** laufen heute über die MAF-RequestPort-Gates des Ein-Graphen und die
  ⚿-ToolApprovals des Stewards — nicht mehr über Approval-Dateien.
- **Pfad-Wächter-Denken** (RootPolicy) findet sich in der festen Artefakt-Whitelist des
  Stewards (`05-core/AuthoredDocument.cs`) wieder.
- **MCP im lebenden System** ist etwas anderes: der Steward mountet zur Laufzeit die
  Lesewerkzeuge des **offiziellen github-mcp-servers** (Remote, serverseitig readonly —
  `AgenticSdlc.Host/Mcp/GithubMcp.cs`). Kein Code des Hosts referenziert dieses Projekt.

## Eingefrorene Spike-Belege

- `runs/20260220_164829_33b72c/` — ein vollständiger Spike-Lauf vom 20.02.2026 samt
  Approval-Datei und geschriebenen Dokument-Kopien.
- `docs/` — die damaligen Frühphasen-Artefakte (requirements, architecture, risks,
  open-questions, issues.json) im Stand der Exploration. **Nicht** mit dem heutigen,
  aus dem Core generierten `docs/` an der Repo-Wurzel verwechseln.
- `.mcp/server.json` — noch mit den Platzhaltern der Projektvorlage; der Server wurde
  nie als Paket veröffentlicht.

Letzte inhaltliche Änderung: 21.05.2026 (`git log -- AgenticSdlc.McpServer`).
