# FullWorkflow — die Produktkette Transkript → GitHub-Issue

Die Ordner SIND die Kette (Nummern = Reihenfolge). Jede Stufe ist intern ein MAF-Workflow
(`WorkflowBuilder`-Graph aus Executor-Knoten, Muster `Seed → Maker(Agent) → Gate → [Repair] → Finalize`);
ZWISCHEN den Stufen übergeben Datei-Artefakte mit Human-Gates (bewusst — jede Mutation der Wahrheit ist autorisiert).

| Ordner | Aufgabe | Kern-Artefakt |
|---|---|---|
| `01-ledger/` | Transkript → belegte, facettierte Claims + Human-Adjudikation | `consumable.json` (freigegebene Evidenzschicht) |
| `02-baselines/` | consumable → geprüfte Artefakt-Baselines (Evidence-Agent „Arm B" + Recipe/Fan-out; Bausteine: MakerChecker/Chain/Derivation, Messwerkzeug Fidelity) | `baselines/{type}/artifact.json` |
| `03-gap/` | Open-World Gap/Coverage-Prüfung (l3) — optionaler Zusatz, Promotions via Human-Review | promoted items |
| `04-delta/` | die gemeinsame ProjectState-SPRACHE (Modelle/Repos/Views) + Bau von Erst-State & MeetingDeltas | `project-state.json` / MeetingDelta |
| `05-core/` | **DIE WAHRHEIT** — der lebende ProjectState hinter dem `ICoreRepository`-Port. Versioniert heißt konkret: `schemaVersion` + `provenance` im Dokument, UND seit 2026-07-23 legt jeder inhaltsändernde `SaveAsync` den Vorzustand als Snapshot nach `state/core/history/` (lokal, gitignored — Restore = zurückkopieren) | `state/core/project-state.json` |
| `06-backlog/` | kanonische Baseline → Readiness → PBI-Schnitt (re-clarify) → IssuePlanning/Clarification | `product-backlog.json`, `accepted-issue-plan.json` |
| `07-tore/` | die kontrollierten Zugänge zur Wahrheit: `ingestion` (Tor 1) · `pbiupdate` (Placement) · `decision` (Tor 2) · `github` (Tor 3, Forward/Reverse) | `github-sync-delta.json`, GitHub Issues |
| `08-pipeline/` | HITL-Super-Workflow über den Toren (RequestPort-Gates, durable Checkpoints) | Checkpoint/Resume |
| `contracts/` | geteilte Datenverträge (quer zur Kette) | — |
| `docs/` `fixtures/` `hitlspike/` | Notes (lokal/gitignored), Test-Fixtures, HITL-Spike | — |

**Ablauf-Geschichten:**
- **Bootstrap (einmal, `core-bootstrap-first-transcript`):** 01 → 02 → (03) → 04 → `core-seed` (05 existiert) → 06-Schnitt → `core-seed-backlog`.
- **Laufender Betrieb (pro Meeting):** 01 → 02 → 04 (= MeetingDelta) → 07 Tore mutieren 05 → `07-tore/github` projiziert nach GitHub. GitHub ist NIE Quelle — Rückweg nur als kontrolliertes Feedback (Reverse).

Historie/Genealogie: Tags `v-s0…v-s9`, `archive/README.md`, lokale Karten (`PROJECT-GENEALOGY.md` u.a.).
Namespaces = Struktur seit R6 (2026-07-22): `AgenticSdlc.Host.FullWorkflow.{Ledger, Ledger.Core, Backlog, Gap, Delta, Core, PbiUpdate, Decision, Tore.Github, Pipeline, …}`. Rest-Notizen: NS `Core` deckt core+ingestion; contracts-Modelle im `Backlog`-NS.
