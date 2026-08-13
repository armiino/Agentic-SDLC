# `state/` — die lebende Projektwahrheit + ihre Backups

> Status: LEBEND (angelegt 10.08.2026). Beschreibt, was hier liegt und die **Backup-/Rollback-Disziplin**
> der Betriebs-Abnahme. `state/` steht bewusst AUSSERHALB `runs/` (Wahrheit ≠ Lauf-Chronik).

## Was hier liegt

| Ordner | Rolle |
|---|---|
| **`core/`** | **DIE Wahrheit** — `project-state.json` (hinter `ICoreRepository`, DB-Swap-Naht). Jeder inhaltsändernde Save schreibt einen Snapshot nach `core/history/`. |
| `core/history/` | automatische Save-Snapshots (Wiederherstell-Schutz je Save). |
| `core-b6-lauf1/`, `core-b6-lauf2/`, `core-u3-lauf/` | **committete Beweis-Cores** (Bootstrap-/Ein-Graph-Beweisläufe = Thesis-Evidenz). NICHT anfassen. |
| `core-ux-*` / `core-ckpt-*` | **lokale Test-Backups** (gitignored) — Sicherheitsnetze der Abnahme, temporär. |
| `core-<beweisname>/` (ohne `ux-`/`ckpt-`) | **Meilenstein-Beweis** eines Zustands, der etwas festhält → bewusst committen (b6-Konvention). |
| `steward/sessions/` | Chat-Verläufe des Stewards (gitignored, lokal). |
| `steward/{author-front,sweep-answers}/` | Autor-Diktate/Antworten (committbar — Beleg-Inputs mit Provenance). |

## Backup-/Rollback-Disziplin (⚖ Autor 10.08.)

**Grundhaltung: der Core WÄCHST bewusst.** Die Abnahme läuft im Betriebszweig auf dem echten Core; neue
REQs/PBIs/DECs sind gewollt (das ist ja der Sinn). Also NICHT stur auf eine Ur-Baseline zurückschalten —
sondern **vorwärts mit Checkpoints**:

1. **Baseline (Start der Abnahme):** `state/core-ux-baseline-<datum>/` — der committete git-Stand ist der
   zweite Rückkehrpunkt (`git checkout -- state/core/`).
2. **Inkrementelle Checkpoints:** VOR jedem riskanten Block (bzw. NACH jedem sauber abgeschlossenen) ein
   Backup: `cp -r state/core state/core-ckpt-<block>-<zeit>` (gitignored). So entsteht eine Kette guter Stände.
3. **Rollback = auf den NEUESTEN GUTEN Checkpoint** (nicht auf die Ur-Baseline!), NUR wenn ein Block etwas
   „kaputt gemacht" hat (Kangal-Fehler, ungewollte Mutation, kaputter Lauf): `rm -rf state/core &&
   cp -r state/core-ckpt-<neuester-guter> state/core`.
4. **Meilenstein festhalten:** beweist ein Zustand etwas → als `state/core-<beweisname>/` benennen und
   committen (Thesis-Beleg).
5. **Prüfung „gut?":** nach jedem Block `pipeline-full status` → Kangal 0 Fehler + erwartete Item-Zahl.
   Grün = Checkpoint behalten und weiter; rot = Rollback auf letzten guten.

**Gitignore:** `state/core-ux-*` und `state/core-ckpt-*` bleiben lokal (temporäre Netze). Beweis-Cores
(`b6-lauf*`, `u3-lauf`, benannte Meilensteine) sind committet. Chat-Sessions lokal.

## Restore-Kommandos (Spickzettel)
- **git (committeter Stand):** `git checkout -- state/core/`
- **auf einen Checkpoint:** `rm -rf state/core && cp -r state/core-ckpt-<name> state/core`
- **Lage prüfen:** `shasum state/core/project-state.json` · `pipeline-full status` (Kangal/Parkplatz)
