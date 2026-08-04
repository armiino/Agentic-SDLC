# Archiv: die dormante Alt-L4-Backlog-Kette (Rückbau 04.08.2026)

> Status: HISTORIE — archiviert am 04.08.2026 (Autor-Entscheid, überschreibt „dormant belassen" vom 22./26.07.).
> **Die vollständige Genealogie + das WARUM der Ablösung: `docs/aktiv/backlog-genealogie.md`.**
> Verifikations-Grundlage des Schnitts: Dormant-Subsystem-Karte in `docs/aktiv/done/2026-08-03/hitl-befunde.md`.

## Was hier liegt

| Ordner | Inhalt |
|---|---|
| `06-backlog/` | Die 9 Alt-Stufen: `baseline · consolidation · quality · readiness · completion · clarification · openrequirements · operationalization-audit · requirementsdoc` (46 .cs-Dateien) |
| `prompts/` | Die 4 nur von der Alt-Kette genutzten Agenten-Prompts: `ClarificationPlanningAgent · L4CompletionAgent · L4AdequacyFeedbackAgent · L4RequirementsConsolidationAgent` |
| `tests/` | Die 4 Adapter-Testdateien (13 Tests): Clarification/L4Completion/L4Review/OpenRequirements-ReviewAdapterTests |
| `contracts/` | `ClarificationPlanModels.cs` + `OpenRequirementModels.cs` — nach dem Schnitt ohne lebenden Nutzer (dokumentieren die Artefakt-Formate der Alt-Läufe in `runsArchive/`) |

## WARUM entfernt (Kurzfassung — Langfassung in der Genealogie)

1. **Funktional ersetzt seit 17.07.:** `l4-re-clarify` löst das Wurzelproblem der Alt-Kette (Readiness-**Filter**
   zerriss Features, weil er Requirements EINZELN beurteilte) durch die umgekehrte Reihenfolge: erst Feature-Cluster
   mit Coverage-Garantie, dann PBIs. Der frische E2E (23.07., leer → 192 Core-Items → 39 Issues) nutzte die
   Alt-Kette **kein einziges Mal**.
2. **Der letzte Haltegrund fiel mit R-14 (04.08.):** `l4-completion` war der einzige Decision-Minter
   („parken bis R-14"). Seit R-14 mintet das System Decisions doppelt operativ (Ingest-CONTRADICT + D2
   `to_decision` am pbi-Gate) — die Alt-Kette hielt nichts mehr, was das lebende System braucht.
3. **Rückbau-Verifikation (04.08., klassenweiser Sweep):** KEIN lebender Code-Pfad referenzierte die 9 Ordner —
   alle externen Referenzen waren CLI-Registrierung (`BacklogCommands`), Tests der Alt-Adapter selbst oder
   Alt-Ordner untereinander. Smoke/UI/tools/run-config: null Treffer. Nach dem Schnitt: Build 0/0 ·
   332 Tests grün · Smoke 14/0.

## Was BLIEB (bewusst)

- **`contracts/` im Host** bis auf die 2 verwaisten Dateien: `L4Models` / `L4QualityModels` /
  `RequirementsReadinessModels` / `IssuePlanningModels` haben lebende Nutzer (`CoreToBaseline`, re-clarify,
  issuplanning, 04-delta-Views). Einzelne Alt-Typen darin (`Consolidation*`, `RequirementsReadiness*`) bleiben als
  Formatdoku der archivierten Läufe.
- **04-delta-Views:** Das View-KONZEPT lebt (`CanonicalRequirementsView`, `IssuePlanningView`); entfernt wurden nur
  die 3 verwaisten Projektionen (`ReadinessView`, `ClarificationPlanningView`, `AcceptedClarificationPlanView`)
  + 5 tote Repository-Methoden. `AcceptedIssuePlanView`/`ProductBacklogView` leben als Records in re-clarify.
- **`l4-issuplanning`** (LIVE/Bridge) + die komplette **re-clarify-Familie**.
- **Alle Läufe** (`runs/`, `runsArchive/`) — Thesis-Evidenz, unangetastet.

## Nicht ersetzte Fähigkeiten (ehrliche Lücken — dokumentiert, keine hängt an diesem Code)

- **Bootstrap-Konsolidierung** (Dedup über die ganze Initial-Baseline): stromaufwärts gelöst (Ledger-Kanonisierung/
  Adjudikation) + inkrementell in Tor 1; nie ein R-Eintrag.
- **Kapitel-Adequacy / kreative Req-Vervollständigung:** bewusst geparkt und GRÖSSER neu gedacht —
  Diskussions-Parkplatz **`docs/aktiv/aufgefallen.md §2a-9f`** (Steward-Fähigkeit über die gated Schreibwege).
- **RE-Dokument-Renderer:** reine Projektion, bei Bedarf Neubau gegen den Core.

Ein Wiedereinbau würde in JEDEM Fall neu gegen den heutigen Core gebaut (Maker + Gate), nicht aus diesem Code —
das Archiv dient als Referenz und Beweis, nicht als Ersatzteillager.
