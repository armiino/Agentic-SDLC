#!/usr/bin/env python3
"""W2-Pilot: heuristisches VORSCHLAGS-Matching (P1/P2-Bänder) — KEINE offiziellen Zahlen.

Verfahren (deterministisch): Kandidaten-Paarung über AU-Referenz-Überlappung, Ähnlichkeits-Score
(difflib + Token-Überlappung), Zahlen-/Negations-Wächter. Klassifikation je Paar:
voll-Vorschlag / GRENZFALL / kein. P1/P2 werden als BAND berichtet (strict = nur voll-Vorschläge ·
optimistisch = voll + Grenzfälle). Die offizielle Auswertung ist die Evaluator-Adjudikation über
die verblindeten Match-Blätter (Matchregeln §4/§8) — dieses Skript liefert nur Vorschläge (§6.4).
"""
import json, re, difflib
from pathlib import Path
from statistics import median

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "runs/w2-pilot-eval"
GOLD = {
    "meeting-2-extended": ROOT / "input/eval-labels/meeting-2-extended.w2-gold.json",
    "Interview-Einrichtung": ROOT / "input/eval-labels/Interview-Einrichtung.w2-gold.json",
}
STOP = set("der die das den dem des ein eine einen einem einer und oder soll sollen muss müssen kann können wird werden bleibt bleiben ist sind auf im in an mit für von zu zur zum bei aus als auch nicht nur noch über unter nach je mindestens beziehungsweise bzw etwa app".split())

def norm(s): return " ".join(re.sub(r"[^\wäöüß ]", " ", s.lower()).split())
def stem(t):
    for suf in ("ungen", "ung", "en", "er", "es", "em", "e", "n", "s"):
        if len(t) > 4 and t.endswith(suf): return t[: -len(suf)]
    return t
def toks(s): return {stem(t) for t in norm(s).split() if t not in STOP and len(t) > 2}
def zahlen(s): return set(re.findall(r"\b(?:\d+|fünf|sieben|drei|zehn|zwei|vier|sechs|acht|neun)\b", s.lower()))
NEG = re.compile(r"\b(nicht|kein|keine|keinen|niemand|nie)\b")

def score(gs, ss):
    r = difflib.SequenceMatcher(None, norm(gs), norm(ss)).ratio()
    tg, ts = toks(gs), toks(ss)
    j = len(tg & ts) / len(tg | ts) if tg | ts else 0
    ueberdeckung = len(tg & ts) / len(tg) if tg else 0     # Gold-Token im Arm-Text (längen-robust)
    s = 0.25 * r + 0.35 * j + 0.40 * ueberdeckung
    zg, zs = zahlen(gs), zahlen(ss)
    if zg and not (zg & zs): s = min(s, 0.60)        # Gold-Zahl fehlt/abweichend ⇒ nie voll (Wortgrenzen!)
    if bool(NEG.search(gs)) != bool(NEG.search(ss)): s = min(s, 0.60)  # Oberflächen-Negation ⇒ höchstens Grenzfall
    return s

def lade():
    rep = json.loads((OUT / "deterministic-report.json").read_text())
    obs = []
    for r in rep:
        if "fehler" in r: continue
        rd = ROOT / ("runs/arm-f" if r["arm"] == "F" else "runs/ledger") / r["runId"]
        if r["arm"] == "F":
            sts = [dict(statement=s["statement"], refs=s["sourceUnitIds"]) for s in json.loads((rd / "output.json").read_text())["statements"]]
        elif r["arm"] == "L":
            sts = [dict(statement=e["proposition"], refs=e.get("sourceUnitIds") or []) for e in json.loads((rd / "capture/l-machine.json").read_text())["entries"]]
        else:
            sts = [dict(statement=e["entry"]["proposition"], refs=e["entry"].get("sourceUnitIds") or []) for e in json.loads((rd / "capture/lcr-machine.json").read_text())["entries"]]
        obs.append(dict(**{k: r[k] for k in ("fall", "arm", "runId")}, statements=sts))
    return obs

def main():
    obs = lade()
    # L≡LCR-Programmcheck je Ledger-Lauf
    print("== L vs LCR (inhaltsgleich?) ==")
    for rid in {o["runId"] for o in obs if o["arm"] == "L"}:
        l = next(o for o in obs if o["runId"] == rid and o["arm"] == "L")
        c = next(o for o in obs if o["runId"] == rid and o["arm"] == "LCR")
        gleich = [s["statement"] for s in l["statements"]] == [s["statement"] for s in c["statements"]]
        print(f"  {rid}: {'IDENTISCH' if gleich else 'VERSCHIEDEN'}")
    ergebnisse, grenzfaelle = [], []
    for o in obs:
        gold = json.loads(GOLD[o["fall"]].read_text())["units"]
        # P1: je Gold-Unit bester Kandidat (Paarung: Ref-Überlappung ODER Score-Screen)
        p1v = p1g = 0
        for g in gold:
            grefs = set(g["sourceUnitIds"])
            kand = [s for s in o["statements"] if grefs & set(s["refs"])] or o["statements"]
            best = max(((score(g["statement"], s["statement"]), s) for s in kand), key=lambda x: x[0])
            if best[0] >= 0.72: p1v += 1
            elif best[0] >= 0.45:
                p1g += 1
                grenzfaelle.append(dict(fall=o["fall"], arm=o["arm"], runId=o["runId"], gold=g["id"],
                                        score=round(best[0], 2), goldText=g["statement"], armText=best[1]["statement"]))
        # P2: je Aussage bestes Gold (nur ref-überlappende Kandidaten + Screen)
        p2v = p2g = 0
        for s in o["statements"]:
            srefs = set(s["refs"])
            kand = [g for g in gold if srefs & set(g["sourceUnitIds"])] or gold
            best = max(score(g["statement"], s["statement"]) for g in kand)
            if best >= 0.72: p2v += 1
            elif best >= 0.45: p2g += 1
        n, gN = len(o["statements"]), len(gold)
        ergebnisse.append(dict(fall=o["fall"], arm=o["arm"], runId=o["runId"], statements=n,
            p1_strict=round(p1v / gN, 3), p1_optimistisch=round((p1v + p1g) / gN, 3),
            p2_strict=round(p2v / n, 3), p2_optimistisch=round((p2v + p2g) / n, 3)))
    (OUT / "proposal-metrics.json").write_text(json.dumps(dict(
        hinweis="HEURISTISCHES VORSCHLAGS-MATCHING (deterministisch) — Band strict..optimistisch; KEINE offiziellen Zahlen (Evaluator-Adjudikation ausstehend).",
        ergebnisse=ergebnisse, grenzfaelle=grenzfaelle), ensure_ascii=False, indent=2))
    print(f"\n== Vorschlags-Bänder (strict … optimistisch) — je Beobachtung ==")
    for e in ergebnisse:
        print(f"  {e['fall'][:12]:12} {e['arm']:3} {e['runId'][-6:]}  n={e['statements']:3}  "
              f"P1 {e['p1_strict']:.2f}…{e['p1_optimistisch']:.2f}  P2 {e['p2_strict']:.2f}…{e['p2_optimistisch']:.2f}")
    print("\n== Median je Bedingung/Fall (strict|optimistisch) ==")
    for fall in GOLD:
        for arm in ("F", "L", "LCR"):
            es = [e for e in ergebnisse if e["fall"] == fall and e["arm"] == arm]
            if not es: continue
            print(f"  {fall[:12]:12} {arm:3}  P1 {median(e['p1_strict'] for e in es):.2f}|{median(e['p1_optimistisch'] for e in es):.2f}"
                  f"  P2 {median(e['p2_strict'] for e in es):.2f}|{median(e['p2_optimistisch'] for e in es):.2f}")
    print(f"\nGrenzfälle gesamt: {len(grenzfaelle)} → proposal-metrics.json")

if __name__ == "__main__":
    main()
