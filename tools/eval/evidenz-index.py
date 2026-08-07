#!/usr/bin/env python3
"""9l (06.08.2026) — Evidenz-Index-Generator: verbindet rohe Run-Ordner mit ihrer Erzählung.

Scannt runs/ (+ runsArchive/) und grep-t die LOKALEN Docs (docs/, thesis-evidence/) nach jeder runId.
Output: runs/EVIDENZ-INDEX.md — je Lauf: Art · Datum · referenziert-in (Doc-Dateien) oder UNREFERENZIERT.
Regenerierbar (deterministisch): python3 tools/eval/evidenz-index.py
"""
import os, re, glob, collections, subprocess

ROOT = subprocess.run(["git", "rev-parse", "--show-toplevel"], capture_output=True, text=True).stdout.strip() or "."
os.chdir(ROOT)

RUN_ID = re.compile(r"20\d{6}_\d{6}(?:_[0-9a-f]{6})?")

# 1. Docs einlesen (lokal, nie committet — die Erzähl-Schicht).
doc_refs = collections.defaultdict(set)   # runIdPrefix -> {docPfad}
for pat in ["docs/**/*.md", "thesis-evidence/**/*.md", "CLAUDE.md"]:
    for f in glob.glob(pat, recursive=True):
        try:
            for rid in set(RUN_ID.findall(open(f, encoding="utf-8", errors="ignore").read())):
                doc_refs[rid].add(f)
        except OSError:
            pass

def refs_for(run_id: str):
    out = set()
    for rid, files in doc_refs.items():
        if run_id.startswith(rid) or rid.startswith(run_id):
            out |= files
    return sorted(out)

# 2. Läufe scannen.
rows = []
for base in ["runs", "runsArchive"]:
    for art_dir in sorted(glob.glob(f"{base}/*/")):
        art = art_dir.rstrip("/").split("/", 1)[1]
        for d in sorted(glob.glob(art_dir + "*")):
            name = os.path.basename(d)
            if not os.path.isdir(d) or not RUN_ID.match(name):
                continue
            rows.append((base, art, name, refs_for(name)))

# 3. Rendern.
out = ["# Evidenz-Index — Lauf → Erzählung",
       "",
       "> GENERIERT — nie von Hand editieren. Neu erzeugen: `python3 tools/eval/evidenz-index.py`",
       "> Ein Lauf ohne Doc-Referenz ist NICHT automatisch wertlos (kann Ketten-Zwischenglied sein) —",
       "> aber ein referenzierter Lauf ist BELEG und darf nie gelöscht werden (Reibungs-Log-Disziplin).",
       ""]
for base in ["runs", "runsArchive"]:
    subset = [r for r in rows if r[0] == base]
    if not subset:
        continue
    out.append(f"## {base}/  ({len(subset)} Läufe, {sum(1 for r in subset if r[3])} referenziert)")
    out.append("")
    for art, group in [(a, [r for r in subset if r[1] == a]) for a in sorted({r[1] for r in subset})]:
        ref_n = sum(1 for r in group if r[3])
        out.append(f"### {art}/ — {len(group)} Läufe, {ref_n} referenziert")
        out.append("")
        out.append("| Lauf | Erzählung (Doc-Referenzen) |")
        out.append("|---|---|")
        for _, _, name, refs in group:
            label = " · ".join(f"`{r}`" for r in refs[:4]) + (" · …" if len(refs) > 4 else "") if refs else "— (unreferenziert)"
            out.append(f"| `{name}` | {label} |")
        out.append("")
open("runs/EVIDENZ-INDEX.md", "w", encoding="utf-8").write("\n".join(out) + "\n")
total = len(rows); ref = sum(1 for r in rows if r[3])
print(f"runs/EVIDENZ-INDEX.md: {total} Läufe, {ref} referenziert, {total - ref} unreferenziert.")
