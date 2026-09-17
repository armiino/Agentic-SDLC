#!/usr/bin/env python3
"""Read-only check of input identity, scoped rating revision and arithmetic."""
from pathlib import Path
from hashlib import sha256
from collections import Counter
from statistics import median
import json

P=Path(__file__).resolve().parent
load=lambda rel:json.loads((P/rel).read_text())
manifest=load('input-manifest.json')
for rel,meta in manifest['files'].items():
 raw=(P/'originals'/rel).read_bytes()
 assert sha256(raw).hexdigest()==meta['sha256'] and len(raw)==meta['bytes'],rel
base=load('originals/thesis-evidence/w2/ledger-repetitions-20260915/results/claim-quality-details.json')
oldmetrics=load('originals/thesis-evidence/w2/ledger-repetitions-20260915/results/results.json')['metrics']
revision=load('revisionen.json')
assert len(revision['changes'])==2
ledger=next(x for x in revision['changes'] if x['case']=='A41')
target=next(x for x in base if x['review_id']=='A41')
assert all(target[k]==v for k,v in ledger['old'].items())
target.update(ledger['new']);target['reason']=ledger['reason'];target['revision']='K01, 2026-09-16'
assert base==load('claim-quality-current.json'),'Unlisted change in Ledger ratings'
assert len(base)==135
metrics=[]
for old in oldmetrics:
 m=dict(old);rows=[x for x in base if x['run']==old['tag']]
 cc=Counter(x['correctness'] for x in rows);sc=Counter(x['support'] for x in rows)
 m.update(correct_claims=cc['no_distortion_found'],correctness=cc['no_distortion_found']/len(rows),direct_support=sc['directly_supported'],inferential_support=sc['inferentially_supported'],partial_support=sc['partially_supported'],unsupported=sc['unsupported'],semantic_support=(sc['directly_supported']+sc['inferentially_supported'])/len(rows))
 metrics.append(m)
recorded=load('results.json')
assert metrics==recorded['ledger_metrics']
agg={k:{'median':median(x[k] for x in metrics),'minimum':min(x[k] for x in metrics),'maximum':max(x[k] for x in metrics)} for k in ['correctness','semantic_support']}
assert agg==recorded['ledger_aggregates']
f=load('originals/runs/arm-f/20260906_143052_6540e2/output.json')['statements']
assert len(f)==117 and len({x['id'] for x in f})==117
assert next(x for x in f if x['id']=='C-086')['statement']=='Als bevorzugte Entwicklungsumgebung wurde Android Studio vereinbart.'
oldf=load('originals/thesis-evidence/w2/eval/w2-official-eval-6540e2.json')['blockA']
assert oldf['strictPrecision']==0.991 and oldf['semanticSupport']==0.991 and 'einziger Nicht-voll-Claim: C-088' in oldf['praezisionsBefund']
fr=load('f-claim-revisions.json');assert fr['current_not_fully_correct']==['C-086','C-088']
assert fr['current_not_fully_supported']==['C-086','C-088']
nf=len(f)-len(fr['current_not_fully_correct'])
assert nf==recorded['f_stress']['correct_claims']==recorded['f_stress']['fully_supported_claims']==115
assert nf/len(f)==recorded['f_stress']['correctness']==recorded['f_stress']['semantic_support']
au=load('autorenprotokoll.json')
assert au['author_attests_full_html_readthrough'] and not au['full_criterion_by_criterion_original_source_check_documented']
assert not au['independent_human_annotation'] and not au['new_individual_answer_export_received']
print(json.dumps({'passed':True,'claim_cases_changed':2,'judgment_fields_changed':4,'ledger_metrics':metrics,'f_stress':recorded['f_stress'],'scope':'Source identity and arithmetic only; no automated validation of semantic truth or author activity.'},ensure_ascii=False,indent=2))
