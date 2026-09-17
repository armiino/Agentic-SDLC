#!/usr/bin/env python3
"""Reproduziert die ABGELEITETEN Review-Zahlen (Faktenblatt „Sorte B") aus den committeten Run-Dateien.

Aufruf (vom Repo-Root):  python3 tools/eval/eval_review.py

Sorte-A-Zahlen (groundingScore, counts.fabricated, die Defekt-Liste) stehen DIREKT in den JSONs und
brauchen kein Skript. Dieses Skript belegt nur die Zahlen, die durch Rechnung/Zählung/Urteil entstehen:
  F3  Gate-Entscheidung (war nicht persistiert) -> aus defects nachgerechnet
  F5  Hand-Label-Bilanz   -> aus input/eval-labels/28a52b.grounding-handlabels.md
  F6  Grounding-2x2        -> groundingScore/fabricated je Zelle
  F7  Agent-Findings 8/6/7 -> aus den Freitext-review.md (Summary-Zeilen geparst)
"""
import json, os, re
from collections import Counter

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
def P(*a): return os.path.join(ROOT, *a)
def load(f): return json.load(open(f, encoding="utf-8"))


# ---------------------------------------------------------------- F3: Gate (v1 vs v2) ----
def gate_v2(review):
    if review["status"] == "Failed": return "Failed"
    believed = [d for d in review["defects"] if d["verificationStatus"] in ("Confirmed", "Partial")]
    if any(d["severity"] == "Critical" for d in believed): return "Repair"
    if any(d["verificationStatus"] == "Unverified" for d in review["defects"]) or review["status"] == "Partial":
        return "HumanReview"
    if any(d["severity"] in ("Medium", "Low") for d in believed): return "PassWithWarnings"
    return "Pass"

def gate_v1(review):  # alte Regel: irgendein bestaetigter/partieller Defekt -> Repair
    if review["status"] == "Failed": return "Failed"
    if any(d["verificationStatus"] in ("Confirmed", "Partial") for d in review["defects"]): return "Repair"
    if any(d["verificationStatus"] == "Unverified" for d in review["defects"]) or review["status"] == "Partial":
        return "HumanReview"
    return "Pass"

def f3():
    runs = ["20260612_133345_2d7b09", "20260612_112129_92e6a3", "20260612_083541_c9943e"]
    arts = ["requirements", "risks", "architecture", "open-questions"]
    v1, v2 = Counter(), Counter()
    for r in runs:
        for a in arts:
            d = load(P("runs/phase2_1", r, "jury/_review", f"{a}.review.openai_gpt-4.1-mini.json"))
            v1[gate_v1(d)] += 1
            v2[gate_v2(d)] += 1
    print("F3  Gate v1 (12 Messungen):", dict(v1))
    print("F3  Gate v2 (12 Messungen):", dict(v2))


# ---------------------------------------------------------------- F5: Hand-Labels ----
def f5():
    txt = open(P("input/eval-labels/28a52b.grounding-handlabels.md"), encoding="utf-8").read()
    c = Counter()
    for line in txt.splitlines():
        m = re.match(r"\|\s*\d+\s*\|", line)            # nur die Tabellen-Zeilen mit laufender Nummer
        if not m: continue
        cells = [x.strip() for x in line.split("|")]
        label = cells[4].lower()                         # Spalte „Hand-Label"
        if "korrekt" in label:   c["korrekt"] += 1
        elif "falsch" in label:  c["falsch"] += 1
        elif "borderline" in label: c["borderline"] += 1
    print("F5  Hand-Labels (10 Flags):", dict(c))


# ---------------------------------------------------------------- F6: Grounding-2x2 ----
def f6():
    base = P("runs/phase2B/20260623_154552_28a52b/jury")
    cells = {
        "v1+mini ": f"{base}/_review/{{a}}.review.openai_gpt-4.1-mini.json",     # aus review (defects)
        "v2+mini ": f"{base}/_units/{{a}}.grounding.openai_gpt-4.1-mini.v2.json",
        "v1+stark": f"{base}/_units/{{a}}.grounding.openai_gpt-5.4.json",
        "v2+stark": f"{base}/_units/{{a}}.grounding.openai_gpt-5.4.v2.json",
    }
    for a in ("risks", "open-questions"):
        print(f"F6  [{a}] groundingScore / fabricated je Zelle:")
        for name, tmpl in cells.items():
            d = load(tmpl.format(a=a))
            if "counts" in d:                            # _units-Format
                gs, fab = d["groundingScore"], d["counts"].get("fabricated", 0)
            else:                                        # _review-Format -> aus defects ableiten
                gs = d["metrics"]["groundingScore"]
                fab = sum(1 for x in d["defects"] if x["category"] == "Grounding.Fabricated")
            print(f"       {name}: {gs} / {fab}")


# ---------------------------------------------------------------- F7: Agent-Findings ----
def f7():
    # Zaehlregel: BODY-Findings = nummerierte "### N)"-Punkte MINUS explizit als "kein Fehler" markierte.
    # (Die Agent-Summary ist unzuverlaessig: Run 969dc5 listet 5 Missing im Body, zaehlt aber 4 in der Summary.)
    runs = ["20260623_175026_5e7d1c", "20260623_175430_3edf9d", "20260623_175521_969dc5"]
    cats = ["FALSE_CLAIM", "FALSE_CERTAINTY", "MISSING_TOPIC"]
    print("F7  Agent risks-Findings (BODY-Zaehlung; + Summary-Diskrepanz):")
    for r in runs:
        f = P("runs/reviewpilot", r, "reviews/risks.review.md")
        if not os.path.exists(f): continue
        txt = open(f, encoding="utf-8").read()
        numbered = len(re.findall(r"(?m)^### \d", txt))             # nummerierte Punkte
        kein_fehler = len(re.findall(r"kein Fehler", txt, re.I))    # davon Nicht-Befunde
        body = numbered - kein_fehler
        summary = sum(int(re.findall(rf"{c}[:`*\s]+(\d+)", txt)[-1]) for c in cats
                      if re.findall(rf"{c}[:`*\s]+(\d+)", txt))
        flag = "" if body == summary else f"  ⚠ Agent-Summary={summary} (Selbst-Unterzaehlung)"
        print(f"       {r[-6:]}: body-findings={body}{flag}")


if __name__ == "__main__":
    f3(); print(); f5(); print(); f6(); print(); f7()
