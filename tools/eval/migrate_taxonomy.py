#!/usr/bin/env python3
"""Signal-erhaltende Taxonomie-Migration der Referenz-Ledger/Fixtures (#3, 2026-07-02).

Regeln (siehe ledger-taxonomy.md §Migration):
  status=undecided -> status=open
  status=desired   -> status=open   (Modalitaet BEHALTEN; nur modality=desired falls leer)
  status=optional  -> status=open   (Modalitaet BEHALTEN; nur modality=optional falls leer)
  modality=open    -> modality=must_clarify
Idempotent. Meldet pro Datei die Anzahl geaenderter Felder.
"""
import json, sys, pathlib

FILES = [
    "input/eval-labels/T9999_chaos.reference-ledger.json",
    "input/eval-labels/Interview-Einrichtung.reference-ledger.json",
    "input/eval-labels/semantic-ledger-interview-spike.json",
    "input/eval-labels/semantic-ledger-evidence-first-spike.json",
]

def migrate_entry(e):
    changed = []
    st = e.get("status")
    mo = e.get("modality")
    if st in ("undecided", "desired", "optional"):
        if st == "desired" and not mo:
            e["modality"] = "desired"; changed.append("modality:->desired")
        if st == "optional" and not mo:
            e["modality"] = "optional"; changed.append("modality:->optional")
        e["status"] = "open"; changed.append(f"status:{st}->open")
    if e.get("modality") == "open":
        e["modality"] = "must_clarify"; changed.append("modality:open->must_clarify")
    return changed

def main():
    root = pathlib.Path(__file__).resolve().parents[2]
    total = 0
    for rel in FILES:
        p = root / rel
        if not p.exists():
            print(f"[skip] {rel} (fehlt)"); continue
        d = json.loads(p.read_text(encoding="utf-8"))
        entries = d["entries"] if isinstance(d, dict) and "entries" in d else d
        n = 0
        for e in entries:
            if migrate_entry(e):
                n += 1
        p.write_text(json.dumps(d, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"[ok] {rel}: {n} Eintraege geaendert")
        total += n
    print(f"[done] {total} Eintraege migriert")

if __name__ == "__main__":
    main()
