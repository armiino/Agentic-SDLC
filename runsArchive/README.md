# runsArchive — alle Läufe bis zum genealogischen Refactoring (Stand 2026-07-23)

Vor dem ersten frischen FullWorkflow-E2E-Lauf wurde `runs/` geleert: **alle Alt-Läufe liegen hier,
Struktur 1:1 erhalten.** Zitier-Regel für iteration notes / Thesis: ein Pfad `runs/X/...` aus den
Notes heißt jetzt `runsArchive/X/...` — sonst ändert sich nichts.

Diese Läufe sind **Thesis-Evidenz** (S-0 bis S-9: phase1, phase2_1, phase2B, ledger, recipe,
derivation, l3, l4, ingestion, pbi-update, decision, github-forward, pipeline, …). Nicht löschen.
Alte pausierte HITL-Checkpoints hierin sind seit dem Namespace-Umbau (R6, 2026-07-22) ohnehin
nicht mehr resumebar.

**Im Live-Pfad verblieben** (bewusst NICHT archiviert):

- `runs/l4-re-clarify/` — Alt-/Support-Werkstatt, **kein Live-State**.
- `runs/github-snapshot/` — neuester Snapshot = optionaler Kontext für Tor 3 (github-forward).

> **Korrektur 17.09.2026 (code-verifiziert).** Frühere Fassungen dieser Datei nannten
> `runs/l4-re-clarify/` die „aktuelle Backlog-Quelle", deren neuester Lauf automatisch
> aufgelöst werde. Das trifft nicht zu:
>
> - `JsonProjectStateViewRepository` durchsucht als Rückfall `runs/l4` und `runs/l4-completion`
>   (`ExistingRunRoots("l4", "l4-completion")`, Zeilen 64 und 93) — **nie** `runs/l4-re-clarify`.
>   Beide Ordner existieren in diesem Repo nicht.
> - Eine „neuester Lauf"-Automatik gibt es dort nicht. Beide Resolver suchen ausschließlich
>   nach Verzeichnissen, deren Name den **ausdrücklich übergebenen** Token
>   (`scope.SourcePath ?? scope.BaselineId`) enthält.
> - `runs/l4-re-clarify/` lesen nur die Standalone-Support-Befehle
>   `core-seed-backlog <run>`, `l4-re-clarify-issueplan <run>` und `l4-re-clarify-backlog-doc <run>` —
>   jeweils mit explizit benanntem Lauf als Argument (`CoreSeedBacklogRunner:112-119`).
> - Der Betriebsweg `pipeline-full` liest den Ordner nicht; er ist selbst-enthalten in
>   `runs/fullworkflow/<runId>/`.
>
> Die Angabe zu `runs/github-snapshot/` bleibt richtig: `GithubForwardHitlRunner.ResolveSnapshotPath`
> wählt ohne `--issues`-Argument tatsächlich den zuletzt geschriebenen Snapshot
> (`OrderByDescending(File.GetLastWriteTimeUtc)`).
