#!/usr/bin/env python3
"""E2E-Traceability-Audit v3 — Provenienzketten NUR ueber explizite Kanten.

Reparatur nach Kollegen-Review 06.09. (FINAL-REVIEW-06-09.md §3.6):
1. IDs scope-gebunden: Claim-DEFINITIONEN (Objekt mit proposition/statement/evidence) werden nur
   in der Upstream-CLOSURE des Quell-Runs gesucht (config.json-Referenzen, Tiefe<=3) — keine
   globale ID-Suche.
2. Nur explizite Kanten: sourceRunId + config-referenzierte Runs + richtungsgebundene Relationen
   (covers: PBI->REQ · part_of_feature: X->FC). Fehlt die Kante: Pfad OFFEN.
3. Snapshots (core-before/after, snapshots/, checkpoints) zaehlen NICHT als Erzeugungsbeleg.
4. Alle direkten Referenztypen werden ausgewertet (Claim/ArtifactItem/Candidate/Decision).
5. Zitat-Pruefung: VOLLES normalisiertes Zitat (Unicode-vereinheitlicht), Mindestlaenge 40,
   vorrangig gegen das RUN-SPEZIFISCHE Transkript (config 'transcript'); Korpus-Treffer ohne
   Run-Bezug oder mehrdeutige Treffer werden als 'diagnostisch' markiert, nicht als Nachweis.
6. Referenzielle Aufloesung (Code) ist von semantischer Stuetzung (separate Stichprobe) getrennt.
7. Stufe 1b = maschineller VOLL-Audit der referenziellen Aufloesbarkeit aller Items.
8. Reproduzierbarkeit: SHA-256 von Core + Skript, Zeitstempel, Mehrdeutigkeits-Zaehler im JSON.

Aufruf:  python3 tools/eval/w2_traceability_audit.py [SAMPLE_N]   |   --selftest
"""
import json, hashlib, glob, os, re, sys, unicodedata, datetime

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
os.chdir(ROOT)

RUNID_RE = re.compile(r'\b(\d{8}_\d{6}_[0-9a-f]{6})\b')
SNAPSHOT_MARKERS = ('core-before', 'core-after', '/snapshots/', 'checkpoint', 'core-ckpt', '/logs/', 'project-state.json')
GOVERNANCE_ENUM = {  # exact origin -> Wurzeltyp (Klassifikations-HINWEIS; Beleg = Definition im Run)
    'GithubInbound': 'external_inbound',
    'CoreAnalyst': 'analyst_generated',
    'INGESTION_CONTRADICTION': 'human_gate_decision',
    'INGESTION_REJECTION': 'human_gate_decision',
}
MIN_QUOTE_LEN = 40


def is_snapshot(path):
    p = path.replace('\\', '/')
    return any(m in p for m in SNAPSHOT_MARKERS)


def run_dir(run_id):
    for base in ('runs', 'runsArchive'):
        hits = glob.glob(f'{base}/*/{run_id}') + glob.glob(f'{base}/{run_id}')
        if hits:
            return hits[0]
    return None


def upstream_closure(run_id, depth=3):
    """Explizite Kanten: alle in config.json (+run-report) des Runs referenzierten RunIds."""
    seen, order = set(), []
    frontier = [run_id]
    for _ in range(depth + 1):  # +1: sonst wird die letzte Hop-Ebene nur gesammelt, nie aufgenommen (T9)
        nxt = []
        for rid in frontier:
            if rid in seen:
                continue
            seen.add(rid)
            order.append(rid)
            rdir = run_dir(rid)
            if not rdir:
                continue
            edge_files = (glob.glob(f'{rdir}/**/config.json', recursive=True)
                          + glob.glob(f'{rdir}/**/run-report.json', recursive=True))
            for cf in [f for f in edge_files if not is_snapshot(f)]:
                try:
                    txt = open(cf, encoding='utf-8').read()
                except Exception:
                    continue
                nxt += [m for m in RUNID_RE.findall(txt) if m not in seen]
        frontier = nxt
    return order


_DEF_INDEX = {}  # rdir -> {objId: (relpath, obj)}

def definition_index(rdir):
    """Einmal je Run: alle Objekt-DEFINITIONEN (id + proposition/statement/evidence/title)."""
    if rdir in _DEF_INDEX:
        return _DEF_INDEX[rdir]
    idx = {}
    for jf in glob.glob(f'{rdir}/**/*.json', recursive=True):
        if is_snapshot(jf):
            continue
        try:
            doc = json.load(open(jf, encoding='utf-8'))
        except Exception:
            continue
        rel = os.path.relpath(jf, ROOT)
        def walk(o):
            if isinstance(o, dict):
                for idk in ('id', 'claimId', 'candidateId', 'decisionId', 'issueId'):
                    oid = o.get(idk)
                    if isinstance(oid, str) and any(k in o for k in
                            ('proposition', 'statement', 'evidence', 'title', 'text', 'rationale')):
                        idx.setdefault(oid, (rel, o))
                for v in o.values():
                    walk(v)
            elif isinstance(o, list):
                for v in o:
                    walk(v)
        walk(doc)
    _DEF_INDEX[rdir] = idx
    return idx


_CONSUMABLE_IDX = None

def consumable_index():
    """Typisierte autorisierte Claim-Definitionsquelle: adjudizierte Consumables/Ledger.
    Kein globaler Freitext-Hop — nur dieser EINE Artefakt-Typ, mit Run-Zuordnung."""
    global _CONSUMABLE_IDX
    if _CONSUMABLE_IDX is None:
        _CONSUMABLE_IDX = {}
        for jf in (glob.glob('runs/**/step-03b-adjudicated/consumable.json', recursive=True)
                   + glob.glob('runs/**/01-ledger/consumable.json', recursive=True)
                   + glob.glob('runsArchive/**/step-03b-adjudicated/consumable.json', recursive=True)
                   + glob.glob('runsArchive/**/01-ledger/consumable.json', recursive=True)):
            m = RUNID_RE.search(jf)
            rid = m.group(1) if m else '?'
            try:
                doc = json.load(open(jf, encoding='utf-8'))
            except Exception:
                continue
            rel = os.path.relpath(jf, ROOT)
            def walk(o):
                if isinstance(o, dict):
                    oid = o.get('id') or o.get('claimId')
                    if isinstance(oid, str) and ('proposition' in o or 'evidence' in o):
                        _CONSUMABLE_IDX.setdefault(oid, []).append((rid, rel, o))
                    for v in o.values():
                        walk(v)
                elif isinstance(o, list):
                    for v in o:
                        walk(v)
            walk(doc)
    return _CONSUMABLE_IDX


def find_definition(claim_ids, allowed_runs):
    """Definition innerhalb der Upstream-Closure; fuer Ledger-Claims zusaetzlich die
    typisierte Consumable-Quelle. Rueckgabe (runId, relpath, obj)."""
    for rid in allowed_runs:
        rdir = run_dir(rid)
        if not rdir:
            continue
        idx = definition_index(rdir)
        for cid in claim_ids:
            if cid in idx:
                rel, obj = idx[cid]
                return rid, rel, obj
    ci = consumable_index()
    for cid in claim_ids:
        hits = [h for h in ci.get(cid, []) if h[0] in allowed_runs]
        if len(hits) == 1:
            return hits[0]
        if len(hits) > 1:
            return 'MEHRDEUTIG', ';'.join(sorted({h[0] for h in hits})), None
    return None, None, None


def norm(s):
    s = unicodedata.normalize('NFKC', s)
    s = s.translate(str.maketrans({'„': '"', '“': '"', '”': '"',
                                   '‘': "'", '’': "'", '–': '-', '—': '-'}))
    return re.sub(r'\s+', ' ', s).strip()


_TRANSCRIPTS = None

def transcripts():
    global _TRANSCRIPTS
    if _TRANSCRIPTS is None:
        _TRANSCRIPTS = {f: norm(open(f, encoding='utf-8').read())
                        for f in glob.glob('input/transcripts/*.txt')}
    return _TRANSCRIPTS


def run_transcript(rid):
    rdir = run_dir(rid)
    if not rdir:
        return None
    try:
        c = json.load(open(f'{rdir}/config.json'))
        t = c.get('transcript')
        if t and os.path.exists(t):
            return t
    except Exception:
        pass
    return None


def quote_check(obj, defining_run):
    """VOLLES normalisiertes Zitat gegen das run-spezifische Transkript. Rueckgabe:
    ('nachweis'|'diagnostisch'|None, zitat, fundort, anmerkung)"""
    quotes = [ev.get('quote') for ev in (obj.get('evidence') or [])
              if isinstance(ev, dict) and ev.get('quote')]
    for q in quotes:
        nq = norm(q)
        core_q = nq.split(':', 1)[-1].strip() if ':' in nq[:40] else nq
        if len(core_q) < MIN_QUOTE_LEN:
            continue
        rt = run_transcript(defining_run)
        if rt and core_q in transcripts()[rt]:
            return 'nachweis', core_q[:110], rt, 'volles Zitat im run-spezifischen Transkript'
        hits = [f for f, t in transcripts().items() if core_q in t]
        if len(hits) == 1:
            return 'diagnostisch', core_q[:110], hits[0], 'voller Korpus-Treffer, aber ohne Run-Transkript-Bindung'
        if len(hits) > 1:
            return 'diagnostisch', core_q[:110], ';'.join(hits), 'MEHRDEUTIG (mehrere Transkripte)'
    # kurze Zitate nur diagnostisch melden
    for q in quotes:
        nq = norm(q)
        if 0 < len(nq) < MIN_QUOTE_LEN:
            return 'diagnostisch', nq, None, f'Zitat kuerzer als {MIN_QUOTE_LEN} Zeichen'
    return None, None, None, 'kein Evidenz-Zitat am Definitionsobjekt'


def directed_relation_targets(item, rels):
    """NUR richtungsgebundene Ableitungspfade: PBI --covers--> REQ · X --part_of_feature--> FC
    (fuer FC: eingehende part_of_feature-Kanten; fuer PBI: ausgehende covers-Kanten)."""
    iid, typ = item['itemId'], item['itemType']
    targets = []
    if typ == 'pbi':
        targets = [r['toId'] for r in rels if r.get('fromId') == iid and r.get('relationType') == 'covers']
    elif typ == 'feature':
        targets = [r['fromId'] for r in rels if r.get('toId') == iid and r.get('relationType') == 'part_of_feature']
    return targets


def direct_refs(item):
    return ([c for c in (item.get('sourceClaimIds') or []) if c]
            + [c for c in (item.get('sourceArtifactItemIds') or []) if c]
            + [x for x in (item.get('sourceCandidateId'), item.get('sourceDecisionId')) if x])


def resolve_item(item, by_id, rels, depth=0):
    """Kette aufloesen. Rueckgabe rec mit klasse in:
    kette_bis_zitat | referenziell_aufgeloest(+zitatStatus) | governance_belegt | offen"""
    rec = {'itemId': item['itemId'], 'typ': item['itemType'], 'origin': item.get('origin'),
           'sourceRunId': item.get('sourceRunId'), 'klasse': 'offen', 'anmerkung': None}
    rid = item.get('sourceRunId')
    if not rid or not run_dir(rid):
        rec['anmerkung'] = 'kein Quell-Run(-Ordner)'
        return rec
    refs = direct_refs(item)
    via = None
    if not refs and depth == 0:
        for tid in directed_relation_targets(item, rels):
            t_item = by_id.get(tid)
            if t_item:
                sub = resolve_item(t_item, by_id, rels, depth=1)
                if sub['klasse'] in ('kette_bis_zitat', 'referenziell_aufgeloest', 'governance_belegt'):
                    rec.update({'klasse': sub['klasse'], 'relationsHop': tid,
                                'anmerkung': f"via gerichtete Relation -> {tid}: {sub.get('anmerkung')}",
                                'zitat': sub.get('zitat'), 'fundort': sub.get('fundort')})
                    return rec
        rec['anmerkung'] = 'keine direkten Referenzen und kein gerichteter Relationspfad aufloesbar'
        return rec
    if not refs:
        rec['anmerkung'] = 'keine direkten Referenzen (Tiefe>0)'
        return rec
    allowed = upstream_closure(rid)
    def_run, def_path, obj = find_definition(refs, allowed)
    if def_run == 'MEHRDEUTIG':
        rec['anmerkung'] = f'MEHRDEUTIG: Referenz in mehreren zulaessigen Consumables ({def_path})'
        return rec
    if not obj:
        rec['anmerkung'] = f'keine DEFINITION der Referenzen {refs[:3]} in der Upstream-Closure ({len(allowed)} Runs)'
        return rec
    rec.update({'definitionsRun': def_run, 'definitionsArtefakt': def_path})
    status, zitat, fundort, note = quote_check(obj, def_run)
    origin = item.get('origin') or ''
    if status == 'nachweis':
        rec.update({'klasse': 'kette_bis_zitat', 'zitat': zitat, 'fundort': fundort, 'anmerkung': note})
    elif origin in GOVERNANCE_ENUM:
        rec.update({'klasse': 'governance_belegt', 'wurzeltyp': GOVERNANCE_ENUM[origin],
                    'anmerkung': f'Definition im Upstream-Run belegt ({note})'})
    else:
        rec.update({'klasse': 'referenziell_aufgeloest', 'zitatStatus': status,
                    'zitat': zitat, 'fundort': fundort, 'anmerkung': note})
    return rec


# ---------------- Selftest (Pflicht-Negativtests, Review §3.7) ----------------
def selftest():
    import tempfile, shutil
    ok = []
    tmp = tempfile.mkdtemp(prefix='w2audit-selftest-')
    try:
        # Fixture: zwei Runs, Claim 'c-x' DEFINIERT nur im UNVERBUNDENEN Run B
        ra = os.path.join(tmp, 'runs', 'x', '20200101_000000_aaaaaa')
        rb = os.path.join(tmp, 'runs', 'x', '20200101_000000_bbbbbb')
        os.makedirs(ra); os.makedirs(rb)
        json.dump({'runId': '20200101_000000_aaaaaa'}, open(f'{ra}/config.json', 'w'))  # KEINE Kante zu B
        json.dump({'items': [{'id': 'c-x', 'proposition': 'p', 'evidence': [{'quote': 'q' * 50}]}]},
                  open(f'{rb}/artifact.json', 'w'))
        json.dump({'ref': 'c-x'}, open(f'{ra}/uses-ref-only.json', 'w'))  # nur Referenz, keine Definition
        globals()['run_dir'] = lambda rid, _t=tmp: (f'{_t}/runs/x/{rid}'
                                                    if os.path.isdir(f'{_t}/runs/x/{rid}') else None)
        _DEF_INDEX.clear()
        # T1: gleiche Claim-ID im unverbundenen Run -> darf NICHT aufloesen
        r, p, o = find_definition(['c-x'], upstream_closure('20200101_000000_aaaaaa'))
        ok.append(('T1 unverbundener Run loest nicht auf', o is None))
        # T2: ID nur als Referenz (keine Definition) -> offen
        idx = definition_index(f'{tmp}/runs/x/20200101_000000_aaaaaa')
        ok.append(('T2 blosse Referenz ist keine Definition', 'c-x' not in idx))
        # T3: Definition nur in core-before.json -> kein Erzeugungsbeleg
        json.dump({'items': [{'id': 'c-y', 'proposition': 'p'}]},
                  open(f'{ra}/core-before.json', 'w'))
        _DEF_INDEX.clear()
        idx = definition_index(f'{tmp}/runs/x/20200101_000000_aaaaaa')
        ok.append(('T3 Snapshot (core-before) zaehlt nicht', 'c-y' not in idx))
        # T4: Zitat nach Zeichen 160 manipuliert -> Volltext-Match muss scheitern
        base = 'Dies ist ein sehr langes echtes Zitat aus dem Transkript, ' * 5
        globals()['_TRANSCRIPTS'] = {'t1.txt': norm(base)}
        obj = {'evidence': [{'quote': base[:160] + ' MANIPULIERTER REST DER NICHT IM TRANSKRIPT STEHT'}]}
        globals()['run_transcript'] = lambda rid: 't1.txt'
        status, *_ = quote_check(obj, 'egal')
        ok.append(('T4 manipuliertes Zitat-Ende faellt durch', status != 'nachweis'))
        # T5: kurzes generisches Zitat -> nur diagnostisch
        obj = {'evidence': [{'quote': 'Gute Idee, nehmen wir auf.'}]}
        status, *_ , note = quote_check(obj, 'egal')
        ok.append(('T5 Kurz-Zitat nur diagnostisch', status == 'diagnostisch' and 'kuerzer' in note))
        # T6: Governance-origin ohne Definitions-Beleg -> offen
        it = {'itemId': 'X-1', 'itemType': 'requirement', 'origin': 'GithubInbound',
              'sourceRunId': '20200101_000000_aaaaaa', 'sourceClaimIds': ['GH-99']}
        rec = resolve_item(it, {}, [])
        ok.append(('T6 Governance ohne Artefakt-Definition bleibt offen', rec['klasse'] == 'offen'))
        # T7: Relation in falscher Richtung -> kein Provenienzpfad
        it = {'itemId': 'FC-X', 'itemType': 'feature', 'sourceRunId': '20200101_000000_aaaaaa',
              'sourceClaimIds': []}
        rels = [{'fromId': 'FC-X', 'toId': 'REQ-X', 'relationType': 'part_of_feature'}]  # falsche Richtung
        rec = resolve_item(it, {'REQ-X': {'itemId': 'REQ-X', 'itemType': 'requirement'}}, rels)
        ok.append(('T7 falsche Relationsrichtung = kein Pfad', rec['klasse'] == 'offen'))
        # T9: Closure verarbeitet wirklich drei Upstream-Hops (a->b->c->d)
        for rid_,nxt_ in [('20200101_000000_a00001','20200101_000000_b00002'),
                          ('20200101_000000_b00002','20200101_000000_c00003'),
                          ('20200101_000000_c00003','20200101_000000_d00004'),
                          ('20200101_000000_d00004',None)]:
            dd=os.path.join(tmp,'runs','x',rid_); os.makedirs(dd,exist_ok=True)
            json.dump({'runId':rid_, 'upstream':nxt_ or ''},open(f'{dd}/config.json','w'))
        cl = upstream_closure('20200101_000000_a00001')
        ok.append(('T9 drei Upstream-Hops in der Closure', '20200101_000000_d00004' in cl))
        # T8: Claim-Definition in UNVERBUNDENEM Consumable -> darf nicht aufloesen
        globals()['_CONSUMABLE_IDX'] = {'c-z': [('20200101_000000_ffffff', 'fremd/consumable.json',
                                                 {'id': 'c-z', 'proposition': 'p'})]}
        r8, p8, o8 = find_definition(['c-z'], ['20200101_000000_aaaaaa'])
        ok.append(('T8 unverbundenes Consumable loest nicht auf', o8 is None and r8 is None))
    finally:
        shutil.rmtree(tmp, ignore_errors=True)
    for name, passed in ok:
        print(('PASS ' if passed else 'FAIL ') + name)
    return all(p for _, p in ok)


# ---------------- Hauptlauf ----------------
def main():
    sample_n = int(sys.argv[1]) if len(sys.argv) > 1 and sys.argv[1].isdigit() else 20
    core = json.load(open('state/core/project-state.json'))
    items, rels = core['items'], core.get('relations') or []
    by_id = {x['itemId']: x for x in items}

    # Stufe 1: Felder + Run-Existenz (alle Items)
    t1 = {'total': len(items),
          'mitSourceRunIdUndOrdner': sum(1 for it in items if it.get('sourceRunId') and run_dir(it['sourceRunId'])),
          'mitDirektenReferenzen': sum(1 for it in items if direct_refs(it))}

    # Stufe 1b: maschineller VOLL-Audit der referenziellen Aufloesbarkeit (alle Items)
    t1b = {'kette_bis_zitat': 0, 'referenziell_aufgeloest': 0, 'governance_belegt': 0, 'offen': 0,
           'offeneItems': []}
    full_recs = []
    for it in items:
        rec = resolve_item(it, by_id, rels)
        full_recs.append(rec)
        t1b[rec['klasse']] += 1
        if rec['klasse'] == 'offen':
            t1b['offeneItems'].append({'itemId': rec['itemId'], 'origin': rec['origin'],
                                       'anmerkung': rec['anmerkung']})

    # Stufe 2: Seed-Stichprobe (Detail-Ausgabe fuer menschliche Nachpruefung)
    def seedkey(i):
        return hashlib.sha256((i + 'w2seed42').encode()).hexdigest()
    sample_ids = [it['itemId'] for it in sorted(items, key=lambda x: seedkey(x['itemId']))[:sample_n]]
    t2 = {'stichprobe': sample_n, 'seed': "SHA-256(itemId+'w2seed42')",
          'ergebnisse': [r for r in full_recs if r['itemId'] in sample_ids]}

    out = {'audit': 'E2E-Traceability v2 (nur explizite Kanten; Snapshots exkludiert; '
                    'Volltext-Zitat run-spezifisch; Voll-Audit referenziell + Seed-Detail)',
           'reproduzierbarkeit': {
               'befehl': 'python3 tools/eval/w2_traceability_audit.py ' + ' '.join(sys.argv[1:]),
               'coreSha256': hashlib.sha256(open('state/core/project-state.json', 'rb').read()).hexdigest(),
               'skriptSha256': hashlib.sha256(open(__file__, 'rb').read()).hexdigest(),
               'zeitstempelUtc': datetime.datetime.utcnow().isoformat() + 'Z'},
           'stufe1_felder': t1, 'stufe1b_vollAudit_referenziell': t1b,
           'alleErgebnisse': full_recs, 'stufe2_seedDetail': t2}
    os.makedirs('runs/e2e-evidenz', exist_ok=True)
    json.dump(out, open('runs/e2e-evidenz/traceability-audit.json', 'w'), ensure_ascii=False, indent=2)
    print(f"Stufe 1  (n={t1['total']}): RunId+Ordner {t1['mitSourceRunIdUndOrdner']} · direkte Refs {t1['mitDirektenReferenzen']}")
    print(f"Stufe 1b VOLL-AUDIT: bis Zitat {t1b['kette_bis_zitat']} · referenziell {t1b['referenziell_aufgeloest']} · "
          f"governance {t1b['governance_belegt']} · OFFEN {t1b['offen']}")
    for o in t1b['offeneItems'][:15]:
        print('  offen:', o['itemId'], o['origin'], '—', (o['anmerkung'] or '')[:90])
    print(f"Stufe 2  Seed-Detail (n={sample_n}):")
    for r in t2['ergebnisse']:
        print(' ', r['itemId'], r['typ'][:4], '→', r['klasse'],
              ('via ' + r['relationsHop']) if r.get('relationsHop') else '',
              ('[' + (r.get('anmerkung') or '')[:60] + ']'))


if __name__ == '__main__':
    if '--selftest' in sys.argv:
        sys.exit(0 if selftest() else 1)
    main()
