# runsArchive — alle Läufe bis zum genealogischen Refactoring (Stand 2026-07-23)

Vor dem ersten frischen FullWorkflow-E2E-Lauf wurde `runs/` geleert: **alle Alt-Läufe liegen hier,
Struktur 1:1 erhalten.** Zitier-Regel für iteration notes / Thesis: ein Pfad `runs/X/...` aus den
Notes heißt jetzt `runsArchive/X/...` — sonst ändert sich nichts.

Diese Läufe sind **Thesis-Evidenz** (S-0 bis S-9: phase1, phase2_1, phase2B, ledger, recipe,
derivation, l3, l4, ingestion, pbi-update, decision, github-forward, pipeline, …). Nicht löschen.
Alte pausierte HITL-Checkpoints hierin sind seit dem Namespace-Umbau (R6, 2026-07-22) ohnehin
nicht mehr resumebar.

**Im Live-Pfad verblieben** (bewusst NICHT archiviert):

- `runs/l4-re-clarify/` — der neueste Run darin ist die aktuelle Backlog-Quelle
  (`JsonProjectStateViewRepository` löst „latest run" als Input der 06-backlog-Stufen auf).
- `runs/github-snapshot/` — neuester Snapshot = optionaler Kontext für Tor 3 (github-forward).
