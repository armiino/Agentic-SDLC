# archive/ — eingefrorene Forschungs-Stationen (Thesis-Historie, außerhalb des Builds)

**Was das hier ist:** Der Code der frühen Forschungsstationen (S-0 bis S-3) dieses explorativen Projekts.
Er ist **nicht gelöscht, nicht verloren, nicht „schlecht"** — er ist die dokumentierte Vorgeschichte, aus deren
Learnings das heutige System (Ledger → Core → GitHub) entstanden ist. Er liegt hier **außerhalb des
`AgenticSdlc.Host`-Projekts** und wird deshalb **nicht mehr mitkompiliert**; die zugehörigen CLI-Kommandos und
die `AGENT_PHASE`-Selektoren `phase1`/`phase2_1` wurden aus `Program.cs` entfernt (Move 3, 2026-07-22).

**Die verlässlichen Referenzen sind Git-Tags + `runs/<…>/<runId>`** (dauerhaft, zitierfähig). Die genannten
Notes sind lokale Arbeitsdokumente (gitignored) auf dem Rechner des Autors.

| Ordner | Herkunft (Originalpfad) | Station · Tag | Erklärt in (Note, lokal) | Runs / Evidenz |
|---|---|---|---|---|
| `phase1/` | `AgenticSdlc.Host/Phases/Phase1/` | S-0 · `v-s0-phase1` | `phase1notes/phase01-iteration-notes.md` | `runs/phase1` (76) |
| `phase2-dag/` | `AgenticSdlc.Host/Phases/Phase2/{Phase2Runner,Phase2Workflow,Phase2AgentFactory,Phase2Artifacts,Phase2ApprovalRecorder}.cs` + `Validation/` | S-1 · `v-s1-phase2-dag` | `phase2notes/phase02-iteration-notes.md` | `runs/phase2_1` (41) |
| `phase2b/` | `AgenticSdlc.Host/Phases/Phase2/Phase2B/` | S-3 · `v-s3-evaluator-review` | `phase2notes/phase02B-iteration-notes.md` | `runs/phase2B` (24) |
| `evaluation/` | `AgenticSdlc.Host/Phases/Phase2/Evaluation/` (nur der CODE: root, `PerItem/`-Rest, `Review/`, `ReviewAgent/`) | S-3/S-4-Umfeld · `v-s3-evaluator-review` | `Evaluation/NextStep/roter-faden-review.md`, `ReviewWorkflow-Evidence.md`, `ZUSATZ-entscheidungsnotiz-ledger-review.md` | `runs/{phase2B,reviewpilot}`, `thesis-evidence/*` |
| `evaluation-tests/` | `AgenticSdlc.Tests/Review/` (alle 28 Testdateien) | S-3/S-4 | testeten die Evaluation-/Spike-Fläche | — |

**Was NICHT hier liegt (bewusst):**
- Der **Ledger-L1/L2-Kern** (5 Dateien, früher `Evaluation/PerItem/`) ist **Produkt** und lebt in
  `AgenticSdlc.Host/Phases/Phase2/Ledger/core/` (Move 2).
- Die **Doku-Ordner** (`LedgerProgressDocumentation/`, `NextStep/`, `analyse/`) blieben in
  `…/Phase2/Evaluation/` — sie sind das Narrativ, ihre Pfade bleiben stabil.
- `research/{github-reconciliation,github-write}` ist ein anderer Fall: **SUPERSEDED** (abgelöste
  Produkt-Vorläufer, wird via csproj weiter MITkompiliert, Kommandos aufrufbar).

## Wie man das wieder verwendet

1. **Historischen Stand LAUFFÄHIG ansehen (empfohlen):** `git checkout <tag>` (z.B. `v-s3-evaluator-review`) —
   dort ist alles verdrahtet, kompilierbar und mit den damaligen Kommandos (`eval-offline`, `review`,
   `phase1`-Run, Spikes …) ausführbar. Danach zurück: `git checkout evaluator`.
2. **Einzelne Klasse im heutigen System wiederbeleben:** Datei zurück unter `AgenticSdlc.Host/` verschieben
   (Namespaces sind unverändert erhalten — z.B. `…Phases.Phase2.Evaluation.PerItem`), Dispatch-Block in
   `Program.cs` wieder anlegen, Build + `tools/smoke-hitl.sh`. Vorsicht: erst prüfen, ob die Fähigkeit nicht
   längst einen Nachfolger im Produkt hat (siehe lokale `CODEBASE-MAP.md`/`PROJECT-GENEALOGY.md`).
3. **Für die Thesis zitieren:** Tag + runId (+ `thesis-evidence/*`), nicht diesen Ordner.
