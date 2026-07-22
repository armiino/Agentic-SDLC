#!/usr/bin/env python3
"""
Mini-A/B-Auswertung (Testplan.md K1-K4) — reproduzierbar & deterministisch.

Vergleicht ein Transkript-Artefakt (Arm A) und ein Ledger-Artefakt (Arm B) auf:
  K1  Quellen-ID vorhanden?        (deterministisch, beide Arme)
  K2  Quelle auffindbar? (Arm B)   (deterministisch: zitierte id -> Claim mit evidence/sourceUnitIds)
  K3  False-Certainty-Kandidaten   (Arm B: harte Formulierung trotz weicher Facette des zitierten Claims)
  K4  Coverage-Sanity (Arm B)      (welche requirements=required-Claims werden NICHT zitiert)

Nutzung:
  python3 eval-mini-ab.py <armA_requirements.md> <armB_requirements.md> <consumable.json>
Gibt einen Markdown-Report auf stdout aus (in results-*.md umleiten).
"""
import json, re, sys

REQ_LINE = re.compile(r"^\s*[-*]\s+")            # Markdown-Listenpunkt = eine Anforderung
ID_RX    = re.compile(r"\[([a-zA-Z0-9_\-, ]+)\]") # [id] bzw. [id1, id2]

# Weiche Facetten (Quelle NICHT hart) — hier darf das Artefakt nicht "muss/entschieden/MVP" behaupten.
SOFT_STATUS   = {"open", "uncertain"}
SOFT_MODALITY = {"desired", "optional", "must_note", "must_clarify", "must_consider"}
SOFT_TIME     = {"later_possible", "mvp_or_later_unclear"}
# Harte Sprachmarker im Requirement-Text (Verstärkung), grob.
HARD_RX = re.compile(r"\b(muss|müssen|verpflichtend|zwingend|zwingend erforderlich|ist erforderlich|entschieden|festgelegt|im MVP|MVP-)\b", re.I)
SOFT_RX = re.compile(r"\b(soll|sollte|sollen|gewünscht|wunsch|optional|kann|könnte|offen|unklar|geplant|später|ggf|eventuell|möglicherweise|zu klären|prüfen)\b", re.I)

def req_lines(md):
    return [ln.strip() for ln in md.splitlines() if REQ_LINE.match(ln)]

def ids_in(line):
    out = []
    for m in ID_RX.findall(line):
        for tok in m.split(","):
            tok = tok.strip()
            if tok and not re.fullmatch(r"\d+", tok):  # keine reinen Zahlen ([1])
                out.append(tok)
    return out

def main():
    a_md = open(sys.argv[1], encoding="utf-8").read()
    b_md = open(sys.argv[2], encoding="utf-8").read()
    claims = {c["id"]: c for c in json.load(open(sys.argv[3], encoding="utf-8"))["claims"]}

    a_reqs, b_reqs = req_lines(a_md), req_lines(b_md)

    # K1
    a_cited = sum(1 for r in a_reqs if ids_in(r))
    b_cited = sum(1 for r in b_reqs if ids_in(r))

    # K2 (Arm B): zitierte id -> Claim mit evidence?
    b_ids = [i for r in b_reqs for i in ids_in(r)]
    b_ids_uniq = sorted(set(b_ids))
    resolvable = [i for i in b_ids_uniq if i in claims and (claims[i].get("evidence") or [])]
    unresolvable = [i for i in b_ids_uniq if i not in claims]

    # K3 (Arm B): harte Sprache trotz weicher Facette des zitierten Claims
    k3 = []
    for r in b_reqs:
        cids = ids_in(r)
        soft = any(c in claims and (claims[c]["status"] in SOFT_STATUS
                    or claims[c]["modality"] in SOFT_MODALITY
                    or (claims[c].get("timeScope") in SOFT_TIME)) for c in cids)
        if soft and HARD_RX.search(r) and not SOFT_RX.search(r):
            facs = "; ".join(f"{c}: status={claims[c]['status']},mod={claims[c]['modality']},time={claims[c].get('timeScope')}" for c in cids if c in claims)
            k3.append((r, facs))

    # K4 (Arm B): required-Claims NICHT zitiert
    required = [cid for cid, c in claims.items()
               if (c.get("disposition") or {}).get("requirements", {}).get("applicability") == "required"]
    missing_required = sorted(set(required) - set(b_ids))

    p = print
    p("## Mini-A/B — deterministische Auswertung\n")
    p(f"- Arm A: {len(a_reqs)} Anforderungen | Arm B: {len(b_reqs)} Anforderungen\n")
    p("### K1 — Quellen-ID vorhanden?")
    p(f"- Arm A: {a_cited}/{len(a_reqs)} mit [id]")
    p(f"- Arm B: {b_cited}/{len(b_reqs)} mit [id]\n")
    p("### K2 — Quelle auffindbar (Arm B, deterministisch)")
    p(f"- zitierte ids (distinct): {len(b_ids_uniq)}")
    p(f"- auflösbar auf Claim mit evidence: {len(resolvable)}/{len(b_ids_uniq)}")
    p(f"- NICHT im consumable auflösbar: {len(unresolvable)}  {unresolvable if unresolvable else ''}\n")
    p("### K3 — False-Certainty-Kandidaten (Arm B, heuristisch → manuell bestätigen)")
    p(f"- Kandidaten: {len(k3)}")
    for r, facs in k3:
        p(f"  - REQ: {r[:130]}")
        p(f"    FACETTEN: {facs}")
    p("")
    p("### K4 — Coverage-Sanity (Arm B)")
    p(f"- requirements=required-Claims gesamt: {len(required)}")
    p(f"- davon NICHT zitiert: {len(missing_required)}")
    for m in missing_required:
        p(f"  - {m}: {claims[m]['proposition'][:90]}")

if __name__ == "__main__":
    main()
