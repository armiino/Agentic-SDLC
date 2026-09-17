#!/usr/bin/env python3
"""Read-only evidence consistency checks. No semantic re-annotation or model calls."""
from pathlib import Path
import argparse,json,hashlib,sys
p=argparse.ArgumentParser();p.add_argument('--preparation',type=Path);a=p.parse_args()
E=Path(__file__).resolve().parent.parent;P=a.preparation or E.parent
checks=[]
def check(name,condition):checks.append({'name':name,'passed':bool(condition)})
def read(path):return json.loads(path.read_text())
def same(a,b):return json.dumps(a,sort_keys=True,ensure_ascii=False)==json.dumps(b,sort_keys=True,ensure_ascii=False)
def edges(x):return {json.dumps(r,sort_keys=True) for r in x}
def item(x,id):return next(i for i in x['items']if i['itemId']==id)
m=read(E/'evidence-manifest.json');sel=read(P/'selected-cases.json');scores=read(E/'assessments.json')
check('fixed selection hash',hashlib.sha256((P/'selected-cases.json').read_bytes()).hexdigest()==m['selection_sha256'])
fixed=sel['additional_cases']+sel['controls']; check('same eight additional and four controls in fixed order',[c['case_id']for c in scores['cases']]==[c['case_id']for c in fixed])
check('roles preserved',[c['role']for c in scores['cases']]==[c['role']for c in fixed])
for f in m['files']:
 q=E/f['package_path'];check('evidence '+f['package_path'],q.is_file() and hashlib.sha256(q.read_bytes()).hexdigest()==f['sha256'])
valid={'getragen','teilweise getragen','abweichend','mehrdeutig','nicht beurteilbar','nicht anwendbar','nicht untersucht'}
for c in scores['cases']:
 id=c['case_id'];d=read(E/c['dossier']);s=next(z for z in fixed if z['case_id']==id);base=E/d['source_base'];raw=read(base/'07-ingest/plan.json')['operations'][s['operation_index']]
 check(id+' selected proposal matches source',same(raw,d['proposal']) and raw['incomingItemId']==s['incoming_id'])
 check(id+' extracted incoming matches source',same(d['incoming'],item(read(base/'04-delta/project-state.json'),s['incoming_id'])))
 check(id+' raw selected plan hash',hashlib.sha256((base/'07-ingest/plan.json').read_bytes()).hexdigest()==s['plan_sha256'])
 check(id+' complete criteria',set(c['criteria'])=={'K1','K2','K3','K4','K5'})
 for k,v in c['criteria'].items():
  vals=[v['proposal'],v['recorded_result']] if k in ['K1','K2','K3'] else [v]
  check(id+' '+k+' rating vocabulary',all(z['rating']in valid and z['reason']for z in vals))
 for extra in c['additional_evidence']:check(id+' extra source '+extra,(E/extra).is_file())
def dossier(id):return read(E/'dossiers'/f'{id}.json')
def base(id):return E/dossier(id)['source_base']
def core(id,name):return read(base(id)/name)
def before(id):return core(id,'07-ingest/applied/core-before.json')
def mid(id):return core(id,'07-pbi-update/applied/core-before.json')
n=dossier('N01');i=next(i for i in n['after_ingest']if i['itemId']=='REQ-81');check('N01 stored statement equals proposal',i['text']==n['proposal']['statement']);check('N01 feature skip recorded',any('REQ-81' in x and 'Create-Draft' in x for x in n['pbi_apply_report']['skipped']));check('N01 zero new PBI or relations in apply report',n['pbi_apply_report']['newPbis']==[] and n['pbi_apply_report']['relationsAdded']==0)
b=item(before('N02'),'REQ-61');z=item(mid('N02'),'REQ-61');check('N02 same text version and original run',all(b[k]==z[k]for k in ['text','version','sourceRunId']));check('N02 added sole new claim',set(z['sourceClaimIds'])-set(b['sourceClaimIds'])=={'canonical-shift-hand-over-note'})
for id in ['N03','N05','N06']:
 d=dossier(id);check(id+' rejected operation not applied',d['applied_entries']==[] and any(x['incomingItemId']==d['case']['incoming_id'] and x['decision']=='reject' for x in d['ingest_decision']['decisions']));check(id+' all items unchanged over ingest interval',same(before(id)['items'],mid(id)['items']))
check('N03 no rejection added in preserved snapshots',same(before('N03')['proposals'],mid('N03')['proposals']) and dossier('N03')['rejections']==[])
for id,rid in [('N05','REJ-018'),('N06','REJ-015')]:check(id+' rejection tied to exact source plan',any(p['proposalId']==rid and p['metadata']['statement']==dossier(id)['proposal']['statement'] for p in dossier(id)['rejections']))
n=dossier('N04');b=item(before('N04'),'REQ-88');z=next(i for i in n['after_ingest']if i['itemId']=='REQ-88');check('N04 v1→v2 and old text in history',b['version']==1 and z['version']==2 and z['history'][-1]['text']==b['text']);check('N04 only time added to requirement',z['text']==b['text'].replace('am Vortag per','am Vortag um 18 Uhr per'));check('N04 exact projection statement approved',n['projection_entries'][0]['statement']==n['pbi_human_decisions']['alignmentDecisions'][0]['editedStatement']);check('N04 no live write',n['github_report']['dryRun'] and not n['github_report']['executed'])
n=dossier('N07');post=core('N07','07-github/applied/core-before.json');req=item(post,'REQ-91');pbi=item(post,'PBI-046');check('N07 same analyst delta proposal and stored requirement',req['text']==n['incoming']['text']==n['proposal']['statement']);check('N07 prior and new requirements in PBI',set(pbi['pbi']['linkedRequirementIds'])=={'REQ-82','REQ-91'});check('N07 proposed and accepted PBI text exactly stored',pbi['pbi']['goal']==n['pbi_alignments'][0]['proposedStatement']);
for f,t,kind in [('PBI-046','REQ-91','covers'),('REQ-91','FC-15','part_of_feature')]:
 check('N07 new '+kind,any(r['fromId']==f and r['toId']==t and r['relationType']==kind and r['source']=='pbi-update' for r in post['relations']) and not any(r['fromId']==f and r['toId']==t and r['relationType']==kind for r in mid('N07')['relations']))
check('N07 actual issue 44 update reported',n['github_report']['executed'] and any(o['pbiId']=='PBI-046' and o['resultIssueNumber']==44 and o['status']=='updated' for o in n['github_report']['operations']))
n=dossier('N08');check('N08 prior REQ70 unchanged',same(next(i for i in n['after_ingest']if i['itemId']=='REQ-70'),item(before('N08'),'REQ-70')));check('N08 actual decision-gate pause',any(e['type']=='PIPELINE_PAUSED' and e.get('gate')=='decision-gate' for e in n['events']));check('N08 contradicts relation exists',any(r['fromId']=='DEC-001' and r['toId']=='REQ-70' and r['relationType']=='contradicts' for r in core('N08','07-ingest/applied/affected-view.json')['relations']))
check('K01 REQ42 text and version preserved',all(item(before('K01'),'REQ-42')[k]==item(mid('K01'),'REQ-42')[k] for k in ['text','version']));check('K01 rejected proposal retained as REJ002',any(r['proposalId']=='REJ-002' for r in dossier('K01')['rejections']))
n=dossier('K02');b=item(before('K02'),'REQ-42');z=next(i for i in n['after_ingest']if i['itemId']=='REQ-42');check('K02 calendar is old text and history only','Kalender' in b['text'] and 'Kalender' not in z['text'] and any('Kalender' in h['text']for h in z['history']));check('K02 PBI edited fields do not imply changed content',n['pbi_alignments'][0]['proposedStatement']==n['pbi_human_decisions']['alignmentDecisions'][0]['editedStatement'])
n=dossier('K03');check('K03 decision source gives new text',item(mid('K03'),'REQ-78')['text']==n['decision_resolutions']['resolutions'][0]['newStatement']);check('K03 old requirement superseded',item(mid('K03'),'REQ-32')['status']=='superseded');
n=dossier('K04');check('K04 issue45 update reported',n['github_report']['executed'] and any(o['pbiId']=='PBI-047'and o['status']=='updated'and o['resultIssueNumber']==45 for o in n['github_report']['operations']))
for id in ['N01','N02','N08']:check(id+' auto acceptance explicit',any(e['type']=='GATE_ANSWERED' and e.get('gate')=='ingest-gate' and e.get('policy')=='AcceptAll' for e in dossier(id)['events']))
for id in ['N03','N04','N05','N06','N07']:check(id+' interactive acceptance explicit',any(e['type']=='GATE_ANSWERED' and e.get('gate')=='ingest-gate' and e.get('policy')=='Interactive' for e in dossier(id)['events']))
check('three author answers only',[a['case_id'] for a in read(E/'authors.json')['answers']]==['N01','N06','N08'])
result={'scope':'Read-only selection, source integrity and selected factual assertions; not independent semantic validation','checks':len(checks),'passed':sum(x['passed']for x in checks),'failed':[x for x in checks if not x['passed']]};print(json.dumps(result,ensure_ascii=False,indent=2));sys.exit(bool(result['failed']))
