#!/usr/bin/env python3
"""DISK-12 Validierungs-Experiment: 3-in-1-Judge vs. dedizierter FALSE_CLAIM-Judge.
Gleiches Modell (gpt-4.1-mini), gleiche eingefrorene c0ca55-Dokumente, gleiches Transkript.
Einzige Variable: Aufgabenbreite (3 Kategorien vs. nur FALSE_CLAIM).
"""
import json, os, sys, urllib.request

ROOT = "/Users/armino/devProjects/Agentic-SDLC"
RUN = f"{ROOT}/runs/phase2B/20260616_125401_c0ca55"
OUT = f"{RUN}/jury/_fcexp"
MODEL = "openai/gpt-4.1-mini"

def load_env_key():
    for line in open(f"{ROOT}/.env"):
        if line.startswith("OPENROUTER_API_KEY"):
            return line.split("=", 1)[1].strip().strip('"')
    sys.exit("no key")

KEY = load_env_key()
TRANSCRIPT = open(f"{ROOT}/input/transcripts/T9999_chaos.txt").read()

FINDINGS_SCHEMA = {
    "type": "object",
    "properties": {
        "findings": {"type": "array", "items": {"type": "object", "properties": {
            "category": {"type": "string"}, "severity": {"type": "string"},
            "artifactQuote": {"type": "string"}, "transcriptEvidence": {"type": "string"},
            "reason": {"type": "string"}},
            "required": ["category", "severity", "artifactQuote", "transcriptEvidence", "reason"],
            "additionalProperties": False}}},
    "required": ["findings"], "additionalProperties": False}

# --- Baseline: aktueller 3-in-1-Synthese-Prompt (Header+Profil+Body), zusammengesetzt wie im Host ---
def read(p): return open(f"{ROOT}/AgenticSdlc.Host/Prompts/jury/{p}.txt").read()
SYN_HEADER, SYN_BODY = read("synthesis-header"), read("synthesis-body")
PROFILE = {"requirements.md": read("profile-requirements"), "risks.md": read("profile-risks")}
def synthesis_prompt(art): return SYN_HEADER + "\n\n" + PROFILE[art] + "\n\n" + SYN_BODY

# --- Dediziert: NUR FALSE_CLAIM generieren (fokussierte Einzelaufgabe = Vorschlag 1 / G3) ---
FC_PROMPT = """Du bist Qualitätsprüfer für SDLC-Artefakte aus einem Stakeholder-Meeting.

Du erhältst das originale Transkript (Ground Truth) und ein daraus erzeugtes Artefakt.
Deine EINZIGE Aufgabe in diesem Durchgang: FALSE_CLAIM-Fehler finden. Ignoriere alles andere
(keine fehlenden Themen, keine zu sichere Darstellung, kein Stil).

category "FALSE_CLAIM":
Eine Aussage im Artefakt widerspricht dem Transkript ODER kommt im Transkript nicht vor.
Voraussetzung: Das Thema / die konkrete Festlegung ist im Transkript NICHT vorhanden oder
steht im direkten Widerspruch zu einer konkreten Transkriptaussage.

Achte besonders auf un-geerdete KONKRETE Spezifika, die das Transkript nicht hergibt:
- konkrete Zahlen, Prozentwerte, Fristen, Mengen (z. B. Antwortzeiten, Verfügbarkeit %,
  Nutzerzahlen, RTO/RPO-Stunden, Aufbewahrungstage, Rate-Limits),
- benannte Technologien, Protokolle, Provider, Services, Crypto-/KMS-Verfahren.
Wenn das Transkript zu einem Punkt ausdrücklich Unwissen ausdrückt ("Weiß ich nicht",
"vielleicht", "müssen wir klären"), dann ist eine konkrete Zahl/Festlegung im Artefakt dazu
ein FALSE_CLAIM (erfundene Konkretisierung), KEINE bloße Unsicherheit.

ABGRENZUNG: Wenn das Thema im Transkript VORKOMMT und nur der Gewissheitsgrad zu hoch ist,
ist das NICHT dein Fall (das wäre FALSE_CERTAINTY) — melde es hier NICHT. Nur erfundene oder
widersprüchliche Aussagen zählen.

Belege JEDEN Befund mit woertlichem Artefakt-Zitat UND der Transkriptstelle (oder
"nicht vorhanden", wenn das Thema im Transkript fehlt).

## SCHWERE
KRITISCH = Kernthema/Compliance/Datenschutz/Security oder falsche Folgeentscheidung.
MITTEL = fachlich relevant. GERING = kleine Ungenauigkeit.

## AUSGABE (ausschließlich dieses JSON-Objekt)
{ "findings": [ { "category": "FALSE_CLAIM", "severity": "KRITISCH|MITTEL|GERING",
  "artifactQuote": "...", "transcriptEvidence": "... oder 'nicht vorhanden'",
  "reason": "ein Satz" } ] }
Keine Fehler: {"findings": []}. Niemals Text außerhalb des JSON-Objekts."""

def call(system, artifact_text, artifact_name):
    user = f"TRANSKRIPT (Ground Truth):\n{TRANSCRIPT}\n\nARTEFAKT ({artifact_name}):\n{artifact_text}"
    body = json.dumps({"model": MODEL, "temperature": 0.0,
        "messages": [{"role": "system", "content": system}, {"role": "user", "content": user}],
        "response_format": {"type": "json_schema", "json_schema": {
            "name": "sdlc_findings", "strict": True, "schema": FINDINGS_SCHEMA}}}).encode()
    req = urllib.request.Request("https://openrouter.ai/api/v1/chat/completions", data=body,
        headers={"Authorization": f"Bearer {KEY}", "Content-Type": "application/json"})
    resp = json.load(urllib.request.urlopen(req, timeout=180))
    return resp["choices"][0]["message"]["content"]

def fc_findings(raw):
    try:
        fs = json.loads(raw).get("findings", [])
    except Exception:
        return None
    return [f for f in fs if f.get("category", "").upper() == "FALSE_CLAIM"]

os.makedirs(OUT, exist_ok=True)
for art in ["requirements.md", "risks.md"]:
    text = open(f"{RUN}/snapshots/docs/{art}").read()
    print(f"\n################ {art} (Modell {MODEL}) ################")
    for label, sysprompt in [("3in1-synthesis", synthesis_prompt(art)), ("dedicated-FALSE_CLAIM", FC_PROMPT)]:
        raw = call(sysprompt, text, art)
        open(f"{OUT}/{art}.{label}.raw.json", "w").write(raw)
        fcs = fc_findings(raw)
        print(f"\n--- {label}: FALSE_CLAIM = {len(fcs) if fcs is not None else 'PARSE_FAIL'}")
        for f in (fcs or []):
            print(f"   [{f['severity']}] {f['artifactQuote'][:95]}")
            print(f"        reason: {f['reason'][:110]}")
print(f"\nRohdaten: {OUT}")
