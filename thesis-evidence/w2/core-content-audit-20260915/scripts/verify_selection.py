"""Read-only verification of the archived inventory and case selection (stdlib)."""
from pathlib import Path
from hashlib import sha256
from collections import Counter
import json, re

P=Path(__file__).resolve().parent.parent
def h(p):return sha256(p.read_bytes()).hexdigest()
def digest(s):return sha256(s.encode()).hexdigest()
def read(name):return json.loads((P/name).read_text())
manifest=read('originals-manifest.json')['files']
rules=read('selection-rules.json'); inv=read('inventory.json'); sel=read('selected-cases.json')
assert h(P/'protokoll.md')==rules['protocol_sha256']
assert h(P/'selection-rules.json')==sel['rules_sha256']
assert h(P/'inventory.json')==sel['inventory_sha256']
for row in manifest:
    p=(P/row['package_path']).resolve()
    assert p.is_relative_to(P/'originals')
    assert p.stat().st_size==row['bytes'] and h(p)==row['sha256'],row['package_path']
known=set()
for src in rules['known_sources']:
    txt=(P/src['package_path']).read_text()
    found=sorted(set(re.findall(r'20\d{6}_\d{6}_[a-f0-9]{6}',txt)))
    assert found==src['run_ids']
    known.update(found)
known.update(c['run_id'] for c in rules['controls'])
assert sorted(known)==rules['exclude_known_run_ids']

raw_count=0
seen=set()
for run in inv['runs']:
    assert run['run_id'] not in seen;seen.add(run['run_id'])
    if not run['plan_path']:continue
    p=P/run['plan_path']; plan=json.loads(p.read_text());assert h(p)==run['plan_sha256']
    assert len(plan['operations'])==run['operation_count']
    actual=[o for o in inv['operations'] if o['run_id']==run['run_id']]
    assert len(actual)==len(plan['operations'])
    for n,(o,orig) in enumerate(zip(actual,plan['operations'])):
        assert o['operation_index']==n and o['incoming_id']==orig['incomingItemId']
        assert o['proposed_kind']==orig['kind'] and o['proposed_target']==orig.get('targetEntityId')
        assert o['feature_hint']==orig.get('featureKey')
        assert o['operation_sha256']==digest(json.dumps(orig,ensure_ascii=False,sort_keys=True,separators=(',',':')))
        assert o['rank_sha256']==digest('p10-core-content-v1|'+o['operation_key'])
        assert o['known_run']==(run['run_id'] in known)
        raw_count+=1
    c=json.loads((P/run['config_path']).read_text())
    ep=c.get('entryPoint');d=str(c.get('fromDelta') or '').replace('\\','/')
    if ep=='github':route='github'
    elif ep=='transcript' or (not ep and c.get('transcript')):route='transcript'
    elif ep=='delta' or (not ep and d):route='author_delta' if 'steward/author-front/' in d else 'other_delta'
    else:route=ep or 'unknown'
    assert route==run['route_group']
    assert all(o['route_group']==route for o in actual)
assert raw_count==inv['summary']['operations']==len(inv['operations'])
assert len(inv['runs'])==inv['summary']['run_directories']==143
assert sum(bool(r['plan_path']) for r in inv['runs'])==inv['summary']['plans']==65
assert sum(bool(r['config_path']) for r in inv['runs'])==inv['summary']['configs']==136

selected=[];used=set();slot_index=0
for route in rules['route_order']:
 for family in rules['family_order']:
    available=sorted([o for o in inv['operations'] if o['route_group']==route and o['proposed_kind'] in rules['families'][family] and o['run_id'] not in known],key=lambda o:(o['rank_sha256'],o['operation_key']))
    slot=sel['slots'][slot_index];slot_index+=1
    assert slot['ranked_operation_keys']==[o['operation_key'] for o in available]
    assert slot['eligible_count']==len(available)
    possible=[o for o in available if o['run_id'] not in used]
    chosen=possible[0] if possible else None
    assert slot['selected_operation_key']==(chosen['operation_key'] if chosen else None)
    if chosen:selected.append(chosen['operation_key']);used.add(chosen['run_id'])
assert selected==[c['operation_key'] for c in sel['additional_cases']]
assert len(selected)==8 and len(used)==8
assert not (used & known)
assert Counter(c['route_group'] for c in sel['additional_cases'])==dict.fromkeys(rules['route_order'],2)
assert len(sel['controls'])==4
for c in sel['controls']:
    candidates=[o for o in inv['operations'] if o['run_id']==c['run_id'] and o['incoming_id']==c['incoming_id']]
    assert len(candidates)==1 and c['operation_key']==candidates[0]['operation_key']
t=read('assessment-template.json')
assert len(t['cases'])==12 and all(v is None for c in t['cases'] for v in c['ratings'].values())
assert {c['case_id'] for c in t['cases']}=={c['case_id'] for c in sel['additional_cases']+sel['controls']}
print(json.dumps({'status':'passed','archived_originals':len(manifest),'run_dirs':143,'plans':65,'operations':raw_count,'additional_cases':8,'known_controls':4,'empty_slots':0,'semantic_judgments':0},ensure_ascii=False))
