#!/usr/bin/env python3
"""W2 E2b — Ledger-Mechanismenprüfung (Konzept §13.3c), deterministisch auf Pilot-Daten.

Je Ledger-Lauf: ① Stufen-Verlust-Analyse (Kandidaten→kanonisch, je Gold-Unit) ② Gate-Zulauf +
Wiederherstellungs-POTENZIAL + blinde Misses (analytisch, KEIN Gate-Durchgang) ③ Provenienz-
Audit (Kette Claim→candidateIds→sourceUnitIds) ④ Themen-Coverage (alle Arme) + Kompressions-
faktor. Gold-Deckungs-Urteile hier = HEURISTIK (wie w2_match_proposals; angerissen ≥0.45,
voll-Vorschlag ≥0.72) — offizielle Werte nach Evaluator-Adjudikation.
"""
import json, sys
from pathlib import Path
from statistics import median

sys.path.insert(0, str(Path(__file__).resolve().parent))
from w2_match_proposals import score  # geteilte Heuristik

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "runs/w2-pilot-eval"
GOLD = {
    "meeting-2-extended": ROOT / "input/eval-labels/meeting-2-extended.w2-gold.json",
    "Interview-Einrichtung": ROOT / "input/eval-labels/Interview-Einrichtung.w2-gold.json",
}
ANGERISSEN, VOLL = 0.45, 0.72

def best(gs, texte):
    return max((score(gs, t) for t in texte), default=0.0)

def themen(gold):
    """Zusammenhangskomponenten der Gold-Units über geteilte AU-Referenzen (deterministisch)."""
    parent = {u["id"]: u["id"] for u in gold}
    def find(x):
        while parent[x] != x: parent[x] = parent[parent[x]]; x = parent[x]
        return x
    von_au = {}
    for u in gold:
        for r in u["sourceUnitIds"]:
            if r in von_au:
                parent[find(u["id"])] = find(von_au[r])
            else:
                von_au[r] = u["id"]
    gruppen = {}
    for u in gold: gruppen.setdefault(find(u["id"]), []).append(u["id"])
    return list(gruppen.values())

def main():
    rep = json.loads((OUT / "deterministic-report.json").read_text())
    ergebnis = {"hinweis": "E2b-Pilot (HEURISTIK-Basis; offizielle Werte nach Adjudikation)", "ledger": [], "themenCoverage": [], "kompression": []}

    for fall, gp in GOLD.items():
        gold = json.loads(gp.read_text())["units"]
        tg = themen(gold)
        # Themen-Coverage + Kompression für ALLE Beobachtungen dieses Falls
        for r in rep:
            if r.get("fall") != fall or "fehler" in r or r["arm"] == "LCR": continue
            rd = ROOT / ("runs/arm-f" if r["arm"] == "F" else "runs/ledger") / r["runId"]
            if r["arm"] == "F":
                sts = [s["statement"] for s in json.loads((rd / "output.json").read_text())["statements"]]
            else:
                sts = [e["proposition"] for e in json.loads((rd / "capture/l-machine.json").read_text())["entries"]]
            deck = sum(1 for grp in tg if any(best(u["statement"], sts) >= ANGERISSEN
                       for u in gold if u["id"] in grp))
            ergebnis["themenCoverage"].append(dict(fall=fall, arm=r["arm"], runId=r["runId"],
                themen=len(tg), angerissen=deck, quote=round(deck / len(tg), 3)))
            ergebnis["kompression"].append(dict(fall=fall, arm=r["arm"], runId=r["runId"],
                aussagenProGoldUnit=round(len(sts) / len(gold), 2)))

        # Ledger-spezifisch: Stufen / Gate-Potenzial / Provenienz
        for r in rep:
            if r.get("fall") != fall or r.get("arm") != "L": continue
            rd = ROOT / "runs/ledger" / r["runId"]
            kand = [e["proposition"] for e in json.loads((rd / "step-01-candidate/output.json").read_text())["entries"]]
            can = json.loads((rd / "capture/l-machine.json").read_text())["entries"]
            cant = [e["proposition"] for e in can]
            lcr = json.loads((rd / "capture/lcr-machine.json").read_text())
            flag_aus = {i["unitId"] for i in lcr["missSignal"]}
            rr_aus = {a for e in lcr["entries"] if e.get("claimStatus") == "review_required"
                      for a in (e["entry"].get("sourceUnitIds") or [])}
            stufen = dict(extraktionUebersehen=[], kanonisierungVerlust=[], kanonischAngerissen=[], kanonischVoll=[])
            gate_erreichbar, blind = [], []
            for u in gold:
                bk, bc = best(u["statement"], kand), best(u["statement"], cant)
                if bc >= VOLL: stufen["kanonischVoll"].append(u["id"])
                elif bc >= ANGERISSEN: stufen["kanonischAngerissen"].append(u["id"])
                elif bk >= ANGERISSEN: stufen["kanonisierungVerlust"].append(u["id"])
                else: stufen["extraktionUebersehen"].append(u["id"])
                if bc < ANGERISSEN:   # L-Miss (heuristisch): über Gate-Items erreichbar?
                    (gate_erreichbar if set(u["sourceUnitIds"]) & (flag_aus | rr_aus) else blind).append(u["id"])
            # Provenienz-Audit: Kette Claim→candidateIds→sourceUnitIds vollständig?
            kand_ids = {e["id"] for e in json.loads((rd / "step-01-candidate/output.json").read_text())["entries"]}
            kette = sum(1 for e in can if (e.get("candidateIds") and set(e["candidateIds"]) <= kand_ids
                        and e.get("sourceUnitIds")))
            misses = len(gate_erreichbar) + len(blind)
            ergebnis["ledger"].append(dict(fall=fall, runId=r["runId"],
                stufen={k: len(v) for k, v in stufen.items()},
                gateZulauf=dict(missSignal=len(lcr["missSignal"]), reviewRequired=sum(1 for e in lcr["entries"] if e.get("claimStatus") == "review_required")),
                lMissesHeuristisch=misses, davonUeberGateErreichbar=len(gate_erreichbar),
                blindeMisses=len(blind),
                blindQuote=round(len(blind) / misses, 3) if misses else None,
                provenienzKetteVollstaendig=f"{kette}/{len(can)}",
                stufenDetail=stufen, blindListe=blind))

    (OUT / "e2b-report.json").write_text(json.dumps(ergebnis, ensure_ascii=False, indent=2))
    print("== Themen-Coverage (angerissen/Themen) ==")
    for t in ergebnis["themenCoverage"]:
        print(f"  {t['fall'][:12]:12} {t['arm']:2} {t['runId'][-6:]}  {t['angerissen']}/{t['themen']}  ({t['quote']:.2f})")
    print("== Kompression (Aussagen je Gold-Unit) ==")
    for fall in GOLD:
        for arm in ("F", "L"):
            ks = [k["aussagenProGoldUnit"] for k in ergebnis["kompression"] if k["fall"] == fall and k["arm"] == arm]
            if ks: print(f"  {fall[:12]:12} {arm}: median {median(ks)}")
    print("== Ledger: Stufen · Gate-Potenzial · Provenienz ==")
    for l in ergebnis["ledger"]:
        s = l["stufen"]
        print(f"  {l['fall'][:12]:12} {l['runId'][-6:]}  voll={s['kanonischVoll']} angerissen={s['kanonischAngerissen']} "
              f"kanonVerlust={s['kanonisierungVerlust']} extraktÜbersehen={s['extraktionUebersehen']} | "
              f"Zulauf US={l['gateZulauf']['missSignal']} RR={l['gateZulauf']['reviewRequired']} | "
              f"Misses={l['lMissesHeuristisch']} → über Gate erreichbar={l['davonUeberGateErreichbar']}, "
              f"BLIND={l['blindeMisses']} ({l['blindQuote']}) | Kette {l['provenienzKetteVollstaendig']}")

if __name__ == "__main__":
    main()
