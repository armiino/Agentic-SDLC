"""Recompute tables from saved human/AI judgments and immutable source copies.

Usage: python recompute.py PACKAGE_DIRECTORY OUTPUT_DIRECTORY
No model calls and no new semantic judgments. Writes only OUTPUT_DIRECTORY.
"""
from pathlib import Path
from hashlib import sha256
from collections import Counter
from statistics import median
import csv
import copy
import json
import re
import sys

P=Path(sys.argv[1]).resolve(); O=Path(sys.argv[2]).resolve(); O.mkdir(parents=True,exist_ok=True)
def read(rel):return json.loads((P/rel).read_text())
def digest(value):return sha256(json.dumps(value,sort_keys=True,ensure_ascii=False,separators=(',',':')).encode()).hexdigest()
originals=read('originals-manifest.json')['files']
assert len(originals)==39
for item in originals:
    data=(P/item['copy']).read_bytes()
    assert len(data)==item['bytes'] and sha256(data).hexdigest()==item['sha256'],item['copy']
frozen=read('protocol-freeze.json')
assert sha256((P/'leitfaden.md').read_bytes()).hexdigest()==frozen['protocol_sha256']
for rel,h in frozen['sources'].items():assert sha256((P/'originals'/rel).read_bytes()).hexdigest()==h,rel
scope=read('umfangsnachtrag-freeze.json')
assert sha256((P/'umfangsnachtrag.md').read_bytes()).hexdigest()==scope['sha256']
for name,h in scope['initial_judgments_sha256'].items():assert sha256((P/name).read_bytes()).hexdigest()==h,name
gold=read('originals/input/eval-labels/Interview-Einrichtung.w2-gold.json')['units']
G={g['id']:g for g in gold}; assert len(G)==109
base=read('originals/thesis-evidence/w2/eval/w2-nachreview-konsolidierung.json')['beziehungstabelle']
assert {x['gold'] for x in base}==set(G)
runids={'R':'20260905_164650_8189ea','A':'20260905_165151_2dde4d','B':'20260905_165658_1271bb'}
coverage={};quality={};conditions={};claims_by_tag={};allrows=[];qrows=[];metrics=[]
code={'F':'full','P':'partial','N':'none'}
support={'D':'directly_supported','I':'inferentially_supported','P':'partially_supported','U':'unsupported'}
canonical_raw=(P/'originals/input/eval-labels/Interview-Einrichtung.numbered.txt').read_text()
chunks=re.split(r'^\[(AU-\d+)\]\s*',canonical_raw,flags=re.M)
canonical={chunks[i]:' '.join(chunks[i+1].split()) for i in range(1,len(chunks),2)}
assert len(canonical)==136
for tag,run in runids.items():
    prefix=f'originals/runs/ledger/{run}'
    raw=read(prefix+'/capture/lcr-machine.json')
    units={u['id']:u for u in read(prefix+'/step-00-atomic-units/output.json')['units']}
    assert len(units)==136
    for uid,u in units.items():assert canonical[uid]==' '.join((u['speaker']+': '+u['text']).split()),uid
    claims={f'{tag}{i+1:02d}':x['entry'] for i,x in enumerate(raw['entries'])}
    claims_by_tag[tag]=claims
    assert len(claims)==(41 if tag=='B' else 47)
    if tag=='R':
        coverage[tag]={x['gold']:{'verdict':x['ledgerEnd'],'claim_ids':[f'R{int(n):02d}' for n in re.findall(r'CAN-(\d+)',x['ledgerBeleg'])],'reason':x['ledgerBeleg'],'provenance':'consolidated_2026-09-08_not_reannotated'} for x in base}
    else:
        rows=list(csv.reader((P/f'coverage-{tag}-final.tsv').open(),delimiter='\t'))
        assert len(rows)==109
        coverage[tag]={f'G-IE-{n}':{'verdict':code[v],'claim_ids':ids.split(',') if ids!='-' else [],'reason':why,'provenance':'new_2026-09-15_with_consistency_review'} for n,v,ids,why in rows}
    assert set(coverage[tag])==set(G)
    for gid,row in coverage[tag].items():
        assert all(cid in claims for cid in row['claim_ids']),(tag,gid,row)
        if row['verdict']=='full':assert row['claim_ids'],(tag,gid)
        row['inspected_claim_ids']=row['claim_ids']
        row['matched_claim_ids']=row['claim_ids'] if row['verdict']!='none' else []
        row['actual_claim_ids']=[claims[c]['id'] for c in row['matched_claim_ids']]
        row['statement']=G[gid]['statement'];row['sourceUnitIds']=G[gid]['sourceUnitIds'];row['gold_note']=G[gid].get('note','')
        allrows.append({'run':tag,'run_id':run,'gold_id':gid,**row})
    rows=list(csv.reader((P/f'quality-{tag}-final.tsv').open(),delimiter='\t'))
    assert len(rows)==len(claims) and {r[0] for r in rows}==set(claims)
    quality[tag]={}
    for cid,correct,s,why in rows:
        c=claims[cid];refs=c['sourceUnitIds'];assert refs and all(r in units for r in refs)
        row={'claim_id':c['id'],'statement':c['proposition'],'sourceUnitIds':refs,'correctness':'no_distortion_found' if correct=='N' else 'distortion_found','support':support[s],'reason':why,'exact_pair_sha256':digest({'statement':c['proposition'],'sourceUnitIds':sorted(set(refs)),'source_units':{k:units[k]['text'] for k in sorted(set(refs))}})}
        quality[tag][cid]=row;qrows.append({'run':tag,'run_id':run,'review_id':cid,**row})
    cnt=Counter(row['verdict'] for row in coverage[tag].values());sc=Counter(row['support'] for row in quality[tag].values());qc=Counter(row['correctness'] for row in quality[tag].values())
    m={'tag':tag,'run_id':run,'claims':len(claims),'gold':109,'full':cnt['full'],'partial':cnt['partial'],'none':cnt['none'],'full_coverage':cnt['full']/109,'touched_coverage':(cnt['full']+cnt['partial'])/109,'correct_claims':qc['no_distortion_found'],'correctness':qc['no_distortion_found']/len(claims),'direct_support':sc['directly_supported'],'inferential_support':sc['inferentially_supported'],'partial_support':sc['partially_supported'],'unsupported':sc['unsupported'],'semantic_support':(sc['directly_supported']+sc['inferentially_supported'])/len(claims),'valid_references':sum(len(c['sourceUnitIds']) for c in claims.values()),'reference_presence':1.0,'reference_validity':1.0}
    assert cnt.total()==109 and sc.total()==len(claims)
    metrics.append(m)
    config=read(prefix+'/config.json');calls=[]
    for line in (P/prefix/'logs/otel-traces.jsonl').read_text().splitlines():
        t=json.loads(line);tags=t.get('tags',{});rawmsg=tags.get('gen_ai.input.messages')
        if not rawmsg:continue
        msgs=json.loads(rawmsg) if isinstance(rawmsg,str) else rawmsg
        calls.append({'time':t['startTimeUtc'],'input':digest(msgs),'system':digest([x for x in msgs if x.get('role')=='system']),'model':tags.get('gen_ai.request.model'),'response_model':tags.get('gen_ai.response.model'),'temperature':str(tags.get('gen_ai.request.temperature'))})
    calls.sort(key=lambda x:x['time']);assert calls
    capture=read(prefix+'/capture/capture-report.json');assert capture['preAdjudication']
    for a in capture['artifacts']:
        assert sha256((P/prefix/a['path']).read_bytes()).hexdigest().lower()==a['sha256'].lower(),(tag,a['path'])
    final=read(prefix+'/step-03-facet-validation/output.json')['entries']
    assert [x.get('entry',x) for x in final]==[x['entry'] for x in raw['entries']]
    conditions[tag]={'config':digest({k:v for k,v in config.items() if k not in ['runId','timestampUtc']}),'units_sha256':sha256((P/prefix/'step-00-atomic-units/output.json').read_bytes()).hexdigest(),'first_request':calls[0]['input'],'system_prompt_set':sorted({c['system'] for c in calls}),'call_count':len(calls),'models':sorted({c['model'] for c in calls}),'response_models':sorted({c['response_model'] for c in calls}),'temperatures':sorted({c['temperature'] for c in calls})}
for k in ['config','units_sha256','first_request','system_prompt_set']:
    assert len({json.dumps(c[k],sort_keys=True) for c in conditions.values()})==1,k
assert all(c['models']==['openai/gpt-5.4'] and c['response_models']==['openai/gpt-5.4'] and c['temperatures']==['0'] for c in conditions.values())
seen={};duplicates=[]
for row in qrows:
    key=row['exact_pair_sha256'];verdict=(row['correctness'],row['support'])
    if key in seen:
        assert seen[key]['verdict']==verdict,('inconsistent identical pair',row['review_id'])
        duplicates.append({'first':seen[key]['id'],'reused':row['review_id']})
    else:seen[key]={'id':row['review_id'],'verdict':verdict}
fullsets={k:{g for g,v in rs.items() if v['verdict']=='full'} for k,rs in coverage.items()}
allfull=set.intersection(*fullsets.values());anyfull=set.union(*fullsets.values())
aggregate={key:{'median':median(m[key] for m in metrics),'minimum':min(m[key] for m in metrics),'maximum':max(m[key] for m in metrics)} for key in ['full_coverage','touched_coverage','correctness','semantic_support']}
overlap={'full_in_all':sorted(allfull),'full_in_any':sorted(anyfull),'full_status_varies':sorted(anyfull-allfull),'never_full':sorted(set(G)-anyfull),'all_labels_same':sorted(g for g in G if len({coverage[t][g]['verdict'] for t in runids})==1)}
result={'date':'2026-09-15','kind':'retrospective evaluation; no model execution','coverage_judgments_new':218,'claim_cases_new':135,'claim_dimensions_per_case':2,'author_review':read('human-review.json'),'coverage_R_source':'historical consolidated judgments 2026-09-08','quality_R_source':'complete new source/support review, historical output unchanged','metrics':metrics,'aggregates':aggregate,'overlap':overlap,'conditions':conditions,'identical_pairs':duplicates,'coverage':coverage,'claim_quality':quality,'limits':['N=3, one familiar source and one recorded model configuration; no claim of general reliability.','Coverage of original run inherited from prior consolidated round, new runs judged later; consistency review is not independent reannotation.','Semantic judgments by Codex; three selected author checks, no independent human gold or full second annotation in this extension.','Exact immutable provider weights and historical code/environment snapshot not reconstructed.','Reference validity is not semantic support; claim coverage excludes quotations and pointers as extra content.','No new F repeat, hint-quality, facet-quality or post-HITL outcome measurement.']}
def sensitivity(name,why,coverage_edits=(),quality_edits=()):
    cv=copy.deepcopy(coverage);qv=copy.deepcopy(quality)
    for tag,gid,old,new in coverage_edits:
        assert cv[tag][gid]['verdict']==old
        cv[tag][gid]['verdict']=new
    for tag,cid,field,old,new in quality_edits:
        assert qv[tag][cid][field]==old
        qv[tag][cid][field]=new
    values=[]
    for tag in runids:
        c=Counter(x['verdict'] for x in cv[tag].values());q=qv[tag]
        good=sum(x['correctness']=='no_distortion_found' for x in q.values())
        supported=sum(x['support'] in ['directly_supported','inferentially_supported'] for x in q.values())
        values.append({'tag':tag,'full':c['full'],'partial':c['partial'],'none':c['none'],'full_coverage':c['full']/109,'touched_coverage':(c['full']+c['partial'])/109,'correct_claims':good,'correctness':good/len(q),'supported_claims':supported,'semantic_support':supported/len(q)})
    return {'id':name,'reason':why,'coverage_edits':coverage_edits,'quality_edits':quality_edits,'metrics':values}
alternatives=[
    sensitivity('B28_strict_optional','Strengere Erstlesart; Hauptwertung folgt der fallbezogenen Autorenantwort.',
                [('B','G-IE-100','full','partial'),('B','G-IE-101','full','partial')],
                [('B','B28','correctness','no_distortion_found','distortion_found'),('B','B28','support','directly_supported','partially_supported')]),
    sensitivity('A_G048_name_sufficient','Namenssuche allein als ausreichende Wiedergabe der Suchanforderung lesen.',
                [('A','G-IE-048','partial','full')]),
    sensitivity('G064_explicit_role_assignment','Explizite Rollenvergabe bei Anlage verlangen; gleicher Maßstab für alle drei Ausgaben.',
                [(tag,'G-IE-064','full','partial') for tag in runids]),
    sensitivity('B04_strict_context','Den generischen Analyse-/Beobachtungszweck nicht aus dem Kennenlerntermin inferieren.',
                quality_edits=[('B','B04','support','inferentially_supported','partially_supported')])
]
(O/'sensitivities.json').write_text(json.dumps({'date':'2026-09-15','application':'Each alternative separately against final main judgments; not combined, not confidence intervals.','alternatives':alternatives},ensure_ascii=False,indent=2)+'\n')
(O/'results.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
for fn,rows in [('coverage-details.json',allrows),('claim-quality-details.json',qrows)]:
    (O/fn).write_text(json.dumps(rows,ensure_ascii=False,indent=2)+'\n')
with (O/'coverage-grid.tsv').open('w') as f:
    w=csv.writer(f,delimiter='\t');w.writerow(['gold_id','statement','R','A','B','reason_R','reason_A','reason_B'])
    for gid in G:w.writerow([gid,G[gid]['statement'],*[coverage[t][gid]['verdict'] for t in runids],*[coverage[t][gid]['reason'] for t in runids]])
with (O/'claim-quality.tsv').open('w') as f:
    w=csv.writer(f,delimiter='\t');w.writerow(['run_id','review_id','claim_id','statement','sourceUnitIds','correctness','support','reason'])
    for row in qrows:w.writerow([row['run_id'],row['review_id'],row['claim_id'],row['statement'],','.join(row['sourceUnitIds']),row['correctness'],row['support'],row['reason']])
print(json.dumps({'runs':metrics,'same_label_ids':len(overlap['all_labels_same']),'full_all':len(allfull),'full_any':len(anyfull),'reused_exact_pairs':duplicates,'verification':'input hashes, canonical text, IDs, references, counts, captures, recorded conditions passed'},ensure_ascii=False))
