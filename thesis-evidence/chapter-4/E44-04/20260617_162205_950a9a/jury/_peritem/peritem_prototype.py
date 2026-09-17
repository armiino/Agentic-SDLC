#!/usr/bin/env python3
"""Per-Item-Prototyp (Stabilitaetstest): statt offener Fehler-GENERIERUNG eine begrenzte
Per-Item-KLASSIFIKATION. (1) Artefakt deterministisch in Einheiten parsen (kein LLM). (2) Pro
Einheit: grounded | overstated | fabricated gegen das Transkript. (3) 3x scoren auf demselben
eingefrorenen Artefakt → ist der Score/das Flag-Set stabil (im Gegensatz zu errorScore 4↔14 offen)?
"""
import json, os, re, sys, urllib.request
from collections import Counter, defaultdict

ROOT = "/Users/armino/devProjects/Agentic-SDLC"
RUN = f"{ROOT}/runs/phase2B/20260617_162205_950a9a"
ARTIFACT = sys.argv[1] if len(sys.argv) > 1 else f"{RUN}/snapshots/docs/risks.md"
MODEL = "openai/gpt-4.1-mini"
N_RUNS = 3

def key():
    for line in open(f"{ROOT}/.env"):
        if line.startswith("OPENROUTER_API_KEY"):
            return line.split("=", 1)[1].strip().strip('"')
    sys.exit("no key")
KEY = key()
TRANSCRIPT = open(f"{ROOT}/input/transcripts/T9999_chaos.txt").read()

# ---------- (1) Deterministischer Unit-Parser (kein LLM, null Varianz) ----------
def strip_md(s): return s.replace("**", "").replace("`", "").strip()

def extract_units(md):
    units = []
    for raw in md.splitlines():
        line = raw.strip()
        if not line:
            continue
        if line.startswith("#"):                     # Section-Header -> kein Claim
            continue
        if line.startswith("|"):                     # Tabellenzeile
            cells = [strip_md(c) for c in line.strip("|").split("|")]
            if not cells or not cells[0]:
                continue
            if re.match(r"^[-:\s]+$", cells[0]):      # Separatorzeile |---|
                continue
            if cells[0].lower() in ("risiko",):        # Header-Zeile
                continue
            # Unit = Risikoname + Beschreibung (Spalten 0+1); Gegenmassnahme ist Vorschlag, kein Claim
            desc = cells[1] if len(cells) > 1 else ""
            units.append(f"{cells[0]}: {desc}")
            continue
        m = re.match(r"^[-*]\s+(.*)", line) or re.match(r"^\d+\.\s+(.*)", line)
        if m:
            units.append(strip_md(m.group(1)))
    return units

UNITS = extract_units(open(ARTIFACT).read())

# ---------- (2) Per-Item-Klassifikation (begrenzt) ----------
SYS = """Du pruefst ein Risiko-Artefakt gegen das originale Stakeholder-Transkript (Ground Truth).
Du bekommst das VOLLSTAENDIGE Transkript und eine NUMMERIERTE Liste von Artefakt-Einheiten
(je eine Risiko-Aussage). Klassifiziere JEDE Einheit mit GENAU EINEM Verdikt:

grounded   = Der zugrundeliegende Sachverhalt/Konflikt/die Unsicherheit wird im Transkript
  besprochen (auch sinngemaess/paraphrasiert). Risiko-Sprache ("unklar", "koennte", "Risiko, dass",
  "noch offen") ist normal und KEIN Fehler. Auch ein im Transkript offenes Thema, das das Artefakt
  korrekt als offen/unklar markiert, ist grounded.

overstated = Das Thema kommt im Transkript vor, ABER die Einheit stellt etwas als entschieden/gesetzt
  dar, das offen ist, ODER nennt eine konkrete Zahl/Technologie/Frist als gesetzt, die das Transkript
  so nicht hergibt (z. B. erfundene SLA/RTO-Zahl). Wenn die Einheit selbst als Annahme/offen markiert
  ist, ist es NICHT overstated, sondern grounded.

fabricated = Der Sachverhalt kommt im Transkript UEBERHAUPT NICHT vor (frei erfunden).

Antworte ausschliesslich mit JSON:
{ "results": [ { "index": 0, "verdict": "grounded|overstated|fabricated", "reason": "kurz" } ] }
Genau ein Ergebnis pro Einheit. Kein Text ausserhalb des JSON."""

SCHEMA = {"type":"object","properties":{"results":{"type":"array","items":{"type":"object",
    "properties":{"index":{"type":"integer"},"verdict":{"type":"string"},"reason":{"type":"string"}},
    "required":["index","verdict","reason"],"additionalProperties":False}}},
    "required":["results"],"additionalProperties":False}

CHUNK = 12  # DISK-9-Lektion: kleine Batches, sonst truncatet das Modell (51-in-1 lieferte nur 31)

def classify_chunk(global_idx, units):
    listing = "\n".join(f"{k}: {u}" for k, u in enumerate(units))
    body = json.dumps({"model": MODEL, "temperature": 0.0,
        "messages":[{"role":"system","content":SYS},
                    {"role":"user","content":f"TRANSKRIPT:\n{TRANSCRIPT}\n\nARTEFAKT-EINHEITEN:\n{listing}"}],
        "response_format":{"type":"json_schema","json_schema":{"name":"peritem","strict":True,"schema":SCHEMA}}}).encode()
    req = urllib.request.Request("https://openrouter.ai/api/v1/chat/completions", data=body,
        headers={"Authorization": f"Bearer {KEY}", "Content-Type":"application/json"})
    resp = json.load(urllib.request.urlopen(req, timeout=180))
    out = []
    for r in json.loads(resp["choices"][0]["message"]["content"])["results"]:
        if 0 <= r["index"] < len(units):
            out.append({**r, "index": global_idx[r["index"]]})
    return out

def classify():
    results = []
    for off in range(0, len(UNITS), CHUNK):
        gidx = list(range(off, min(off+CHUNK, len(UNITS))))
        results += classify_chunk(gidx, [UNITS[i] for i in gidx])
    return results

WEIGHT = {"grounded":0, "overstated":1, "fabricated":2}

os.makedirs(f"{RUN}/jury/_peritem", exist_ok=True)
print(f"Deterministisch extrahierte Einheiten: {len(UNITS)} (fixer Nenner)\n")
runs = []
for r in range(1, N_RUNS+1):
    res = classify()
    verdict = {x["index"]: x["verdict"] for x in res}
    score = sum(WEIGHT.get(verdict.get(i,"grounded"),0) for i in range(len(UNITS)))
    flagged = {i:verdict[i] for i in verdict if verdict[i] != "grounded"}
    runs.append((score, verdict))
    json.dump(res, open(f"{RUN}/jury/_peritem/run{r}.json","w"), indent=2, ensure_ascii=False)
    print(f"Run {r}: errorScore={score}  verdicts={dict(Counter(verdict.values()))}  flagged_units={sorted(flagged)}")

# ---------- (3) Stabilitaets-Auswertung ----------
print("\n=== STABILITAET ===")
scores=[s for s,_ in runs]
print(f"Scores: {scores}  (min={min(scores)} max={max(scores)} spanne={max(scores)-min(scores)})")
perunit=defaultdict(list)
for _,v in runs:
    for i in range(len(UNITS)): perunit[i].append(v.get(i,"grounded"))
unstable=[i for i in perunit if len(set(perunit[i]))>1]
print(f"Einheiten mit identischem Verdikt ueber alle {N_RUNS} Runs: {len(UNITS)-len(unstable)}/{len(UNITS)}")
print(f"Wackelnde Einheiten: {len(unstable)} -> {unstable}")
for i in unstable:
    print(f"   [{i}] {UNITS[i][:70]} : {perunit[i]}")
print("\n=== Kontroll-Check: werden die fruehered FP (gegruendete Konflikte) korrekt 'grounded'? ===")
for i,u in enumerate(UNITS):
    low=u.lower()
    if any(k in low for k in ["sso","oauth","eu-only","eu‑only","security review","gateway"]):
        print(f"   [{i}] {u[:60]} -> {[v.get(i) for _,v in runs]}")
