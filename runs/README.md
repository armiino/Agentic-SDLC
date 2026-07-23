# runs — frische FullWorkflow-Läufe (seit 2026-07-23)

Alles vor dem genealogischen Refactoring liegt in `runsArchive/` (Repo-Root, Struktur 1:1 — siehe
dessen README). Hier entsteht ab jetzt nur noch Output der aktuellen Kette:
`runs/<stufe>/<runId>/` (ledger, recipe, project-state, ingestion, pbi-update, decision,
github-forward, pipeline, …).

Zwei Ordner sind **Live-Input-State**, kein Wegwerf-Output:

- `l4-re-clarify/` — neuester Run = aktuelle Backlog-Quelle der 06-backlog-Stufen.
- `github-snapshot/` — neuester Snapshot = optionaler Tor-3-Kontext.

Die Wahrheit selbst liegt NICHT hier, sondern in `state/core/`.
