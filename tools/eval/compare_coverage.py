#!/usr/bin/env python3
"""
compare_coverage.py — Test D (Z10 / v02): wirkt sich die separate relevantFor-Klassifikation (v02) auf die
DOWNSTREAM-Coverage aus? Vergleicht je Artefakt die topic-coverage-Ergebnisse mit v01-Gate (Frozen-Fixture)
gegen v02-Gate (Sidecar), bei sonst identischem Lauf (gleiche Topics, gleicher Judge, gleiches Artefakt).

  v01-Lauf:  coverage-topics ... (ohne Override)   -> <art>.topic-coverage.<model>.review.json
  v02-Lauf:  coverage-topics ... <base>.relevance.<m>.json -> <art>.topic-coverage.<model>.relv02.review.json

KERN-FRAGE (B43-Befund): das v02-Über-Trimmen von open-questions könnte echte Missing-Findings VERSTECKEN.
Diese Liste = Topics, die v01 als 'missing' fand, die v02 aber GAR NICHT mehr prüft (Gate gedroppt). Plus:
neue/weggefallene Missing-Findings je Artefakt. Reines Lesen erzeugter JSONs, keine LLM-Aufrufe.

Aufruf:
  python3 tools/eval/compare_coverage.py <phase> <runId> [base] [coverageModelSlug] [relevanceSlug]
  relevanceSlug = welcher Sidecar das v02-Gate lieferte (Default openai_gpt-5.4 = v01-Prompt;
                  für v02-Prompt: openai_gpt-5.4.v02).
  z.B. python3 tools/eval/compare_coverage.py phase2_1 20260612_133345_2d7b09 T9999_chaos openai_gpt-5.4 openai_gpt-5.4.v02
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ARTIFACTS = ["requirements", "risks", "architecture", "open-questions"]  # Datei-Stamm == Artefakttyp


def relevant_by_type(topics: list[dict]) -> dict[str, set[str]]:
    """type -> set(topicId) laut relevantFor."""
    out = {a: set() for a in ARTIFACTS}
    for t in topics:
        for a in t.get("relevantFor", []):
            if a in out:
                out[a].add(t["topicId"])
    return out


def missing_tids(review_path: Path, summary_to_tid: dict[str, str]) -> set[str] | None:
    """topicIds der 'missing'-Defects (über sourceQuote==summary gemappt). None = Datei fehlt."""
    if not review_path.exists():
        return None
    data = json.loads(review_path.read_text())
    tids = set()
    for d in data.get("defects", []):
        if str(d.get("category", "")).endswith("MissingTopic"):
            sq = d.get("sourceQuote", "")
            if sq in summary_to_tid:
                tids.add(summary_to_tid[sq])
            else:
                tids.add("?:" + sq[:40])  # Summary nicht gefunden → sichtbar machen, nicht verschlucken
    return tids


def main() -> None:
    if len(sys.argv) < 3:
        print(__doc__); sys.exit(2)
    phase, run_id = sys.argv[1], sys.argv[2]
    base = sys.argv[3] if len(sys.argv) > 3 else "T9999_chaos"
    model = sys.argv[4] if len(sys.argv) > 4 else "openai_gpt-5.4"
    relevance_slug = sys.argv[5] if len(sys.argv) > 5 else "openai_gpt-5.4"  # welcher Sidecar das v02-Gate lieferte

    topics_dir = ROOT / "input" / "topics"
    fixture = json.loads((topics_dir / f"{base}.topics.json").read_text())["topics"]
    sidecar = json.loads((topics_dir / f"{base}.relevance.{relevance_slug}.json").read_text())["topics"]
    summary_to_tid = {t["summary"]: t["topicId"] for t in fixture}

    rel_v01 = relevant_by_type(fixture)
    # v02-relevantFor je topicId aus dem Sidecar, zurueck auf die Fixture-Topics gelegt:
    v02_rel = {t["topicId"]: set(t.get("relevantFor", [])) for t in sidecar}
    rel_v02 = {a: {tid for tid, rels in v02_rel.items() if a in rels} for a in ARTIFACTS}

    units = ROOT / "runs" / phase / run_id / "jury" / "_units"
    print(f"# Test D — {base}  run {phase}/{run_id}  coverage-judge={model}")
    print(f"# v01 = Frozen-Fixture-Gate, v02 = Sidecar-Gate (.relv02). KERN = 'versteckt' (v01-missing, von v02 nicht mehr geprueft).\n")

    for art in ARTIFACTS:
        v01p = units / f"{art}.topic-coverage.{model}.review.json"
        v02p = units / f"{art}.topic-coverage.{model}.relv02.review.json"
        m01 = missing_tids(v01p, summary_to_tid)
        m02 = missing_tids(v02p, summary_to_tid)
        print(f"## {art}")
        print(f"  relevant (Gate-Nenner): v01={len(rel_v01[art])}  v02={len(rel_v02[art])}")
        if m01 is None or m02 is None:
            miss = [p.name for p, m in [(v01p, m01), (v02p, m02)] if m is None]
            print(f"  (review-Datei(en) fehlen: {miss} — Lauf noch nicht gemacht)\n")
            continue
        print(f"  missing-Findings:       v01={len(m01)}  v02={len(m02)}")
        hidden = (m01 & set()) | {t for t in m01 if t not in rel_v02[art]}   # v01-missing, in v02 nicht mehr relevant
        gained = m02 - m01
        lost_still_checked = {t for t in (m01 - m02) if t in rel_v02[art]}    # v02 prueft, findet aber nicht mehr missing
        if hidden:
            print(f"  ⚠ VERSTECKT (v02 prueft nicht mehr, v01 war missing): {sorted(hidden)}")
        if gained:
            print(f"  + neu missing in v02: {sorted(gained)}")
        if lost_still_checked:
            print(f"  - nicht mehr missing in v02 (aber weiter geprueft): {sorted(lost_still_checked)}")
        if not (hidden or gained or lost_still_checked):
            print("  = keine Missing-Differenz")
        print()


if __name__ == "__main__":
    main()
