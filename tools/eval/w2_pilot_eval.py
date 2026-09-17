#!/usr/bin/env python3
"""W2-Pilot-Auswertung (deterministischer Teil — Matchregeln §1/§6, KEIN LLM).

Liest das Kampagnen-Manifest (runs/w2-pilot-manifest.txt), normalisiert alle Beobachtungen
(F aus output.json · L/LCR aus capture/) auf die gemeinsame Auswertungssicht und berechnet
die SKRIPTBAREN Groessen: Reference Presence, P3 Reference Validity, Aussagen-Zahlen,
Maschinenaufwand (Tokens aus OTel, WallMs wo vorhanden). Erzeugt zusaetzlich je Fall ein
VERBLINDETES Match-Blatt (Beobachtungen anonymisiert + gemischt, deterministischer Seed) fuer
die Evaluator-Adjudikation von P1/P2/P4. Offizielle P1/P2/P4 entstehen NIE hier — nur im
Evaluator-Durchgang (Matchregeln §4/§7).
"""
import json, re, sys, hashlib
from pathlib import Path
from collections import Counter

ROOT = Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "runs/w2-pilot-manifest.txt"
OUT = ROOT / "runs/w2-pilot-eval"
GOLD = {
    "meeting-2-extended": ROOT / "input/eval-labels/meeting-2-extended.w2-gold.json",
    "Interview-Einrichtung": ROOT / "input/eval-labels/Interview-Einrichtung.w2-gold.json",
}

def fall_von(source: str) -> str:
    return "meeting-2-extended" if "meeting-2" in source else "Interview-Einrichtung"

def sum_otel_tokens(traces: Path):
    if not traces.exists(): return (None, None)
    tin = tout = 0; found = False
    for line in traces.read_text().splitlines():
        if not line.strip(): continue
        try: d = json.loads(line)
        except json.JSONDecodeError: continue
        if not str(d.get("name", "")).startswith("chat"): continue
        tags = d.get("tags") or {}
        for key, acc in (("gen_ai.usage.input_tokens", "in"), ("gen_ai.usage.output_tokens", "out")):
            v = tags.get(key)
            try: v = int(v)
            except (TypeError, ValueError): continue
            found = True
            if acc == "in": tin += v
            else: tout += v
    return (tin, tout) if found else (None, None)

def beobachtungen():
    """Manifest → Liste (fall, arm, runId, statements[{statement,refs,type}], aufwand)."""
    text = MANIFEST.read_text()
    obs = []
    for rid in re.findall(r"\[arm-f\] Lauf (\S+):", text):
        rd = ROOT / "runs/arm-f" / rid
        if not (rd / "output.json").exists():
            obs.append(dict(fall="?", arm="F", runId=rid, statements=None, fehler="OHNE gültige Einreichung")); continue
        o = json.loads((rd / "output.json").read_text())
        m = json.loads((rd / "metrics.json").read_text())
        pp = json.loads((rd / "process-profile.json").read_text())
        obs.append(dict(fall=fall_von(o["source"]), arm="F", runId=rid,
            statements=[dict(statement=s["statement"], refs=s["sourceUnitIds"], type=s["type"]) for s in o["statements"]],
            aufwand=dict(tokensIn=m["inputTokens"], tokensOut=m["outputTokens"], wallMs=m["wallMs"]), profil=pp))
    for rid in re.findall(r"\[ledger-capture\] (\S+):", text):
        rd = ROOT / "runs/ledger" / rid
        cfg = json.loads((rd / "config.json").read_text())
        fall = fall_von(cfg.get("transcript") or cfg.get("input") or "")
        tin, tout = sum_otel_tokens(rd / "logs/otel-traces.jsonl")
        l = json.loads((rd / "capture/l-machine.json").read_text())
        lcr = json.loads((rd / "capture/lcr-machine.json").read_text())
        obs.append(dict(fall=fall, arm="L", runId=rid,
            statements=[dict(statement=e["proposition"], refs=e.get("sourceUnitIds") or [], type=e.get("kind")) for e in l["entries"]],
            aufwand=dict(tokensIn=tin, tokensOut=tout, wallMs=None)))
        obs.append(dict(fall=fall, arm="LCR", runId=rid,
            statements=[dict(statement=e["entry"]["proposition"], refs=e["entry"].get("sourceUnitIds") or [], type=e["entry"].get("kind")) for e in lcr["entries"]],
            aufwand=dict(tokensIn=None, tokensOut=None, wallMs=None),  # Tokens stecken im L-Lauf (gleicher Lauf)
            missSignal=len(lcr.get("missSignal") or []), qc=lcr.get("qc")))
    return obs

def p3_presence(statements, au_ids):
    refs = [r for s in statements for r in s["refs"]]
    valid = sum(1 for r in refs if r in au_ids)
    presence = sum(1 for s in statements if s["refs"]) / len(statements) if statements else 0
    return dict(statements=len(statements), refsTotal=len(refs),
                p3=round(valid / len(refs), 4) if refs else None, presence=round(presence, 4))

def main():
    OUT.mkdir(exist_ok=True)
    au_sets = {f: {u for g in [json.loads(p.read_text())] for u in
                   {r for unit in g["units"] for r in unit["sourceUnitIds"]} | set()} for f, p in GOLD.items()}
    # AU-Gültigkeit gegen die KANONISCHE Quelle, nicht nur gegen im Gold referenzierte:
    au_sets = {"meeting-2-extended": {f"AU-{i:04d}" for i in range(1, 37)},
               "Interview-Einrichtung": {f"AU-{i:04d}" for i in range(1, 137)}}
    obs = beobachtungen()
    report = []
    for o in obs:
        if o.get("statements") is None:
            report.append({k: o[k] for k in ("fall", "arm", "runId", "fehler")}); continue
        r = dict(fall=o["fall"], arm=o["arm"], runId=o["runId"], **p3_presence(o["statements"], au_sets[o["fall"]]),
                 aufwand=o["aufwand"])
        if "missSignal" in o: r["missSignal"] = o["missSignal"]
        if "profil" in o: r["profil"] = o["profil"]
        report.append(r)
    (OUT / "deterministic-report.json").write_text(json.dumps(report, ensure_ascii=False, indent=2))

    # Verblindete Match-Blätter: je Fall alle Beobachtungen, deterministisch gemischt, Arm verdeckt.
    for fall, gp in GOLD.items():
        gold = json.loads(gp.read_text())
        L = [f"# W2-Match-Blatt {fall} — VERBLINDET (Evaluator-Adjudikation nach Matchregeln §2–§4)",
             "", "> Je Beobachtung: jede Aussage gegen die Gold-Units urteilen (voll/teilweise/kein;",
             "> Merge/Split-Regeln §3). Beobachtungs-Codes sind anonymisiert; Auflösung liegt separat.", "",
             "## Gold-Units (Referenz)", ""]
        for u in gold["units"]:
            L.append(f"- **{u['id']}** [{u['type']}] {u['statement']}")
        codes = {}
        fall_obs = [o for o in obs if o.get("fall") == fall and o.get("statements")]
        fall_obs.sort(key=lambda o: hashlib.sha256((o["runId"] + fall).encode()).hexdigest())
        for n, o in enumerate(fall_obs, 1):
            code = f"OBS-{n:02d}"
            codes[code] = dict(runId=o["runId"], arm=o["arm"])
            L += ["", f"## {code} ({len(o['statements'])} Aussagen)", ""]
            for i, s in enumerate(o["statements"], 1):
                L.append(f"{n:02d}.{i:02d} [{','.join(s['refs'])}] {s['statement']}")
        (OUT / f"matchblatt-{fall}.md").write_text("\n".join(L) + "\n")
        (OUT / f"matchblatt-{fall}.aufloesung.json").write_text(json.dumps(codes, ensure_ascii=False, indent=2))
    print(f"[w2-eval] {len(obs)} Beobachtungen → {OUT}/deterministic-report.json + Match-Blätter")
    for r in report:
        print(" ", r.get("fall"), r.get("arm"), r.get("runId"),
              "stmts=", r.get("statements"), "p3=", r.get("p3"), "presence=", r.get("presence"), r.get("fehler", ""))

if __name__ == "__main__":
    sys.exit(main())
