#!/usr/bin/env python3
"""
compare_relevantfor.py — Test B (Z10 / v02, Call 2 separat): deterministischer v01<->v02-Diff der
relevantFor-Zuordnung pro Transkript.

  v01 = relevantFor aus der eingefrorenen Fixture  input/topics/<base>.topics.json
  v02 = relevantFor aus dem separaten Classifier    input/topics/<base>.relevance.<model>.json

Join-Schluessel = topicId. Reines Lesen committeter/erzeugter JSONs, keine LLM-Aufrufe → die in B43/
Faktenblatt F8 zitierten abgeleiteten Zahlen sind hierueber 1:1 reproduzierbar.

Aufruf:
  python3 tools/eval/compare_relevantfor.py                       # T9999 + Interview, Fixture(v01) vs gpt-5.4-Sidecar
  python3 tools/eval/compare_relevantfor.py <base> <model>        # einzeln, Fixture(v01) vs <model>-Sidecar
  python3 tools/eval/compare_relevantfor.py <base> <model> <base-slug>
        # Baseline = ein ANDERER Sidecar statt der Fixture, z.B. v02-Prompt vs v01-Prompt:
        #   ... T9999_chaos openai_gpt-5.4.v02 openai_gpt-5.4
"""
from __future__ import annotations

import json
import sys
from pathlib import Path

ARTIFACTS = ["requirements", "risks", "architecture", "open-questions"]
TOPICS_DIR = Path(__file__).resolve().parents[2] / "input" / "topics"


def load_v01(base: str) -> dict[str, set[str]]:
    data = json.loads((TOPICS_DIR / f"{base}.topics.json").read_text())
    return {t["topicId"]: set(t.get("relevantFor", [])) for t in data["topics"]}


def load_v02(base: str, model: str) -> dict[str, set[str]]:
    data = json.loads((TOPICS_DIR / f"{base}.relevance.{model}.json").read_text())
    return {t["topicId"]: set(t.get("relevantFor", [])) for t in data["topics"]}


def counts(rel: dict[str, set[str]]) -> dict[str, int]:
    return {a: sum(1 for s in rel.values() if a in s) for a in ARTIFACTS}


def compare(base: str, model: str, baseline_slug: str | None = None) -> None:
    # Baseline = Fixture (v01-integriert) ODER ein anderer Sidecar (z.B. v01-Prompt) für Sidecar-vs-Sidecar.
    if baseline_slug is None:
        v01, base_label = load_v01(base), "v01 fixture"
    else:
        v01, base_label = load_v02(base, baseline_slug), f"sidecar {baseline_slug}"
    v02 = load_v02(base, model)
    c01, c02 = counts(v01), counts(v02)

    print(f"\n=== {base}  ({base_label}  ->  sidecar {model}) ===")
    print(f"Topics: v01={len(v01)}  v02={len(v02)}  gemeinsam(topicId)={len(v01.keys() & v02.keys())}")
    print(f"{'artifact':<16}{'v01':>5}{'v02':>5}{'delta':>7}")
    tot01 = tot02 = 0
    for a in ARTIFACTS:
        tot01 += c01[a]; tot02 += c02[a]
        print(f"{a:<16}{c01[a]:>5}{c02[a]:>5}{c02[a]-c01[a]:>+7}")
    print(f"{'SUMME(tags)':<16}{tot01:>5}{tot02:>5}{tot02-tot01:>+7}")

    added = removed = 0           # ueber alle Topics: einzelne (topic,artifact)-Zuordnungen
    changed: list[str] = []
    for tid in sorted(v01.keys() & v02.keys()):
        plus = v02[tid] - v01[tid]
        minus = v01[tid] - v02[tid]
        added += len(plus); removed += len(minus)
        if plus or minus:
            parts = []
            if minus: parts.append("-{" + ",".join(sorted(minus)) + "}")
            if plus:  parts.append("+{" + ",".join(sorted(plus)) + "}")
            changed.append(f"  {tid:<34} {' '.join(parts)}")

    print(f"\nGeaenderte Topics: {len(changed)}/{len(v01.keys() & v02.keys())}  "
          f"(entfernte Tags={removed}, hinzugefuegte Tags={added}, netto={added-removed:+d})")
    for line in changed:
        print(line)

    only01 = v01.keys() - v02.keys()
    only02 = v02.keys() - v01.keys()
    if only01: print(f"WARN nur in v01 (Classifier hat topicId nicht zurueckgegeben): {sorted(only01)}")
    if only02: print(f"WARN nur in v02 (unbekannte topicId): {sorted(only02)}")


def main() -> None:
    if len(sys.argv) == 4:
        compare(sys.argv[1], sys.argv[2], sys.argv[3])
    elif len(sys.argv) == 3:
        compare(sys.argv[1], sys.argv[2])
    else:
        compare("T9999_chaos", "openai_gpt-5.4")
        compare("Interview-Einrichtung", "openai_gpt-5.4")


if __name__ == "__main__":
    main()
