#!/usr/bin/env python3
"""
Test-Retest-Aggregator für den C7-Critic (§4.4-Selbstvalidierung).

Der Critic ist ein LLM (temp=0, aber nicht bit-deterministisch). Diese Auswertung misst seine STABILITÄT als
Messinstrument: k unabhängige Läufe auf DEMSELBEN Artefakt -> stimmen die Zeilen-Verdikte überein?

Nutzung:
  python3 critic-retest.py <critic-report-1.json> <critic-report-2.json> [...]
Gibt einen Markdown-Report auf stdout.

Wichtig: Vorfilter-Zeilen (byLlm=false) sind DETERMINISTISCH (kein LLM) -> per Konstruktion stabil; die Varianz
lebt nur in den LLM-geprüften Zeilen. Beide werden getrennt ausgewiesen.
"""
import json, sys
from collections import defaultdict, Counter

def main():
    files = sys.argv[1:]
    if len(files) < 2:
        print("Usage: critic-retest.py <report1.json> <report2.json> [...]"); sys.exit(2)
    reports = [json.load(open(f, encoding="utf-8")) for f in files]
    K = len(reports)

    verdicts = defaultdict(list)   # lineNo -> [verdict je Lauf]
    by_llm, text = {}, {}
    for r in reports:
        for v in r["verdicts"]:
            ln = v["lineNumber"]
            verdicts[ln].append(v["verdict"])
            by_llm[ln] = by_llm.get(ln, False) or v["byLlm"]
            text[ln] = v["line"]

    # nur Zeilen, die in ALLEN k Läufen vorkommen (gleiches Artefakt -> normal alle)
    complete = {ln: vs for ln, vs in verdicts.items() if len(vs) == K}
    llm_lines = [ln for ln in complete if by_llm[ln]]
    prefilter = [ln for ln in complete if not by_llm[ln]]

    def agree(lines):
        return [ln for ln in lines if len(set(complete[ln])) == 1]
    def flip(lines):
        return [ln for ln in lines if len(set(complete[ln])) > 1]

    llm_agree, llm_flip = agree(llm_lines), flip(llm_lines)

    p = print
    p("## C7-Critic — Test-Retest (Instrument-Stabilität)\n")
    p(f"- Läufe (k): **{K}**")
    p(f"- Zeilen gesamt: {len(complete)} | Vorfilter (deterministisch, 0 LLM): {len(prefilter)} | LLM-geprüft: {len(llm_lines)}\n")

    p("### Übereinstimmung der Verdikte (nur die LLM-geprüften Zeilen zählen für Varianz)")
    rate = (len(llm_agree) / len(llm_lines) * 100) if llm_lines else 100.0
    p(f"- **unverändert über alle {K} Läufe: {len(llm_agree)}/{len(llm_lines)} ({rate:.1f}%)**")
    p(f"- geflippt: {len(llm_flip)}")
    for ln in sorted(llm_flip):
        p(f"  - L{ln}: {dict(Counter(complete[ln]))}  | {text[ln][:80]}")
    p("")

    p("### Pass-Stabilität")
    passes = [r["pass"] for r in reports]
    p(f"- pass je Lauf: {passes}")
    for i, r in enumerate(reports):
        vc = Counter(v["code"] for v in r["violations"])
        p(f"  - Lauf {i+1}: pass={r['pass']} violations={len(r['violations'])} {dict(vc)}")
    p("")

    p("### Violation-Zeilen — flaggen sie STABIL?")
    viol_lines = sorted({v["lineNumber"] for r in reports for v in r["violations"]})
    if not viol_lines:
        p("- keine Violation in irgendeinem Lauf.")
    for ln in viol_lines:
        seen = complete.get(ln, ["(fehlt)"] * K)
        stable = "STABIL" if len(set(seen)) == 1 else "INSTABIL"
        p(f"- L{ln} [{stable}]: {dict(Counter(seen))}  | {text.get(ln,'')[:70]}")

if __name__ == "__main__":
    main()
