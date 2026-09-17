# Verwaiste Pause geräumt — 2026-08-10

Dieser Lauf (E-R4 Cross-CONTRADICT-Test, `--from-delta crossreq-delta.json`, 06.08.) pausierte am
decision-gate (DEC-001: Firebase REQ-70 vs. lokale-Speicherung-Widerspruch). Der ingest-Apply schrieb
DEC-001 in den Core; die Core-Mutation wurde danach per git-Restore zurückgerollt (Core zurück auf 194,
REQ-70 aktiv, kein crossreq-DEC). Der Pause-Zeiger blieb liegen → `pipeline-full status` meldete eine
Phantom-Pause. Resume wäre inkonsistent (DEC-001 existiert im Core nicht mehr).

`checkpoints/pointer.json` → `pointer.json.orphaned-20260810` umbenannt (reversibel; Run bleibt als
Test-Beleg). Befund vermerkt in E2E-RUNBOOK (R-45) + aufgefallen.md.
