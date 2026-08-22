#!/usr/bin/env python3
"""9h③-Nachmessung (21.08.2026): Dispositions-Güte der PRODUKTIONS-Extraktion gegen den
handannotierten Referenz-Ledger (Juni-Spike-Vergleichswert: disposition-exact 20–30 %).

Ablauf: Referenz-Eintrag ↔ Auto-Claim werden über EVIDENZ-ZITATE gematcht (beide zitieren dasselbe
Transkript wörtlich; Normalisierung + Jaccard-Fallback). Je Match wird geprüft, ob die vom
Referenz-`kind` erwartete Dispositions-Spur im Auto-Claim `applicability=required` trägt.

Kind→Spur-Mapping (dokumentierter Mess-Entscheid; per-kind-Aufschlüsselung im Report, damit die
Gesamtzahl kein Mapping-Artefakt ist):
  requirement | non_functional_requirement | open_requirement | decision -> requirements
  constraint | compliance_constraint | process_constraint             -> architecture
  open_question                                                        -> open-questions
  risk                                                                 -> risks
  scope | context | meta                                               -> (ausgenommen)

Usage: compare_dispositions.py <reference.json> <canonical-output.json> [out.json]
"""
import json, re, sys, pathlib

KIND2TRACK = {
    "requirement": "requirements",
    "non_functional_requirement": "requirements",
    "open_requirement": "requirements",
    "decision": "requirements",
    "constraint": "architecture",
    "compliance_constraint": "architecture",
    "process_constraint": "architecture",
    "open_question": "open-questions",
    "risk": "risks",
}
EXCLUDED = {"scope", "context", "meta"}

def norm(q: str) -> str:
    q = re.sub(r"^\[?speaker\s*\d+\]?\s*:\s*", "", q.strip(), flags=re.I)
    q = re.sub(r"^[\wäöüß .-]{0,25}:\s*", "", q)          # Sprecher-Präfix („Pflegekraft:")
    q = re.sub(r"[^\wäöüß ]", " ", q.lower())
    return re.sub(r"\s+", " ", q).strip()

def toks(q: str) -> set:
    return set(norm(q).split())

def main():
    ref_path, auto_path = sys.argv[1], sys.argv[2]
    out_path = sys.argv[3] if len(sys.argv) > 3 else None
    ref = json.load(open(ref_path))["entries"]
    auto_doc = json.load(open(auto_path))
    auto = auto_doc["entries"] if isinstance(auto_doc, dict) else auto_doc

    auto_quotes = []  # (claim, [norm-quotes], [token-sets])
    for c in auto:
        qs = [norm(e.get("quote", "")) for e in (c.get("evidence") or [])]
        auto_quotes.append((c, qs, [set(q.split()) for q in qs]))

    rows, per_kind = [], {}
    matched = exact = 0
    unmatched_kinds = {}
    for r in ref:
        kind = r.get("kind")
        if kind in EXCLUDED or kind not in KIND2TRACK:
            continue
        track = KIND2TRACK[kind]
        best, best_score = None, 0.0
        for c, qs, tsets in auto_quotes:
            score = 0.0
            for rq in (r.get("evidence") or []):
                nrq, trq = norm(rq.get("quote", "")), toks(rq.get("quote", ""))
                for q, tq in zip(qs, tsets):
                    if not q:
                        continue
                    if nrq and (nrq in q or q in nrq):
                        score = max(score, 1.0)
                    elif trq and tq:
                        j = len(trq & tq) / len(trq | tq)
                        score = max(score, j)
            if score > best_score:
                best, best_score = c, score
        stat = per_kind.setdefault(kind, {"gold": 0, "matched": 0, "exact": 0})
        stat["gold"] += 1
        if best is None or best_score < 0.5:
            unmatched_kinds[kind] = unmatched_kinds.get(kind, 0) + 1
            rows.append({"ref": r["id"], "kind": kind, "match": None, "score": round(best_score, 2)})
            continue
        matched += 1
        stat["matched"] += 1
        disp = best.get("disposition") or {}
        required = sorted(t for t, v in disp.items() if (v or {}).get("applicability") == "required")
        ok = track in required
        if ok:
            exact += 1
            stat["exact"] += 1
        rows.append({"ref": r["id"], "kind": kind, "erwartet": track, "match": best.get("id"),
                     "score": round(best_score, 2), "required": required, "exact": ok})

    considered = sum(s["gold"] for s in per_kind.values())
    report = {
        "referenz": ref_path, "auto": auto_path,
        "gold_betrachtet": considered, "gematcht": matched,
        "disposition_exact_auf_matches": round(exact / matched, 3) if matched else None,
        "disposition_exact_auf_gold": round(exact / considered, 3) if considered else None,
        "per_kind": per_kind, "unmatched_je_kind": unmatched_kinds,
        "hinweis": "Match via Evidenz-Zitat (>=0.5 Jaccard/Containment); unmatched = Recall-Thema, "
                   "NICHT Dispositions-Fehler. Juni-Vergleichswert: 20-30 % exact.",
        "rows": rows,
    }
    print(json.dumps({k: v for k, v in report.items() if k != "rows"}, ensure_ascii=False, indent=2))
    if out_path:
        pathlib.Path(out_path).write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
        print(f"[ok] Voll-Report: {out_path}")

if __name__ == "__main__":
    main()
