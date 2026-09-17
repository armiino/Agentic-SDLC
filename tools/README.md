# `tools/` — Smoke-Netz und Auswertungsskripte

> Status: LEBEND (README-bei-Code, angelegt 17.09.2026) — bei neuen Skripten mitpflegen.

## `smoke-hitl.sh` — das Regressionsnetz

```bash
bash tools/smoke-hitl.sh        # Exit 0 = alles grün
```

**14 deterministische Prüfungen, kein LLM, kein Netz.** Fährt jede der fünf Gate-Stufen einmal
`start → PAUSE → resume → Apply` (Tor 1, Placement, Tor 2, Tor 3 als Dry-Run, Ein-Graph via
Delta-Einstieg) und prüft danach die Core-Integrität. Sichert `state/core/` vor dem Lauf und
stellt es hinterher byte-identisch wieder her. Vor und nach jeder Code-Änderung fahren.

## `eval/` — die Auswertungsskripte (alle deterministisch, kein LLM)

Alle vom Repo-Root aufrufen: `python3 tools/eval/<skript>.py`. Jede Datei trägt ihren
Zweck als Docstring im Kopf — hier die Kurzfassung mit Ausgabeort:

| Skript | Zweck | schreibt nach |
|---|---|---|
| `evidenz-index.py` | verbindet jeden Run-Ordner mit seiner Erzählung: scannt `runs/` + `runsArchive/` und grept die Docs nach jeder RunId | `runs/EVIDENZ-INDEX.md` |
| `w2_pilot_eval.py` | W2-Pilot: die skriptbaren Größen (Reference Presence, P3-Validität, Aussagen-Zahlen) nach Matchregeln §1/§6 | `runs/w2-pilot-eval/` |
| `w2_match_proposals.py` | heuristisches Vorschlags-Matching als P1/P2-**Band** — ausdrücklich keine offiziellen Zahlen | `runs/w2-pilot-eval/` |
| `w2_e2b_eval.py` | Ledger-Mechanismenprüfung: Stufen-Verluste je Gold-Unit, Gate-Zulauf, blinde Misses | `runs/w2-pilot-eval/` |
| `w2_traceability_audit.py` | E2E-Traceability-Audit v3 — Provenienzketten nur über explizite Kanten | `runs/e2e-evidenz/traceability-audit.json` |
| `eval_review.py` | reproduziert die abgeleiteten Review-Zahlen („Sorte B") aus committeten Run-Dateien | stdout |
| `compare_dispositions.py` | Dispositions-Güte der Produktions-Extraktion gegen den handannotierten Referenz-Ledger (9h③-Nachmessung) | stdout |

### Explorations-Werkzeuge (historische Messungen, Ära der v01→v02-Umstellung)

Funktionsfähig, aber an eingefrorene Fixtures/Fragen von damals gebunden — für den
heutigen Betrieb nicht nötig:

| Skript | Was es damals maß |
|---|---|
| `compare_coverage.py` | Test D (Z10/v02): Wirkung der separaten relevantFor-Klassifikation auf die Downstream-Coverage, gegen Frozen-Fixtures |
| `compare_relevantfor.py` | Test B (Z10/v02): deterministischer v01↔v02-Diff der relevantFor-Zuordnung |
| `migrate_taxonomy.py` | signal-erhaltende Taxonomie-Migration der Referenz-Ledger (#3, 02.07.2026) — Einmal-Migration, als Beleg behalten |

Die Eingaben liegen unter `input/eval-labels/`, die W2-Belegpakete unter `thesis-evidence/w2/`
(dort mit eigenen `verify*.py`-Nachrechenskripten je Paket).
