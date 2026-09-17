#!/usr/bin/env python3
"""Recount disclosed judgments and verify selected archived state correspondences. No model or system execution."""
from pathlib import Path
from hashlib import sha256
from collections import Counter
import json,argparse
E=Path(__file__).resolve().parent
ap=argparse.ArgumentParser(description=__doc__);ap.add_argument("--package-root",type=Path,default=E.parent);K=ap.parse_args().package_root.resolve()
load=lambda p:json.loads(p.read_text())
def count(rows):return dict(sorted(Counter(x['verdict'] for x in rows).items()))
def main():
 r=load(E/'ratings-first-pass.json');a=K/'originals/runs/core-analysis/20260821_203951_db6050';f=K/'originals/runs/fullworkflow/20260821_204808_042823';g=K/'originals/runs/fullworkflow/20260821_211304_bfbbd7'
 assert len(r['rows'])==138 and len({(x['case_id'],x.get('phase'),x['criterion']) for x in r['rows']})==138
 for x in r['rows']:
  allowed=r['allowed_verdicts']['NOV' if x['criterion']=='NOV' else 'CRIT' if x['criterion']=='CRIT' else 'K1-K4/B1-B6']
  assert x['verdict'] in allowed and x['reason']
  for ref in x['evidence']:
   path=(K/ref['file'][3:]) if ref['file'].startswith('../') else E/ref['file'];assert path.is_file(),str(path)
   data=load(path)
   for token in ref.get('pointer','').lstrip('/').split('/'):
    if not token:continue
    data=data[int(token)] if isinstance(data,list) else data[token.replace('~1','/').replace('~0','~')]
  assert x['author_review']['status'] in ['pending','partial_contextual_feedback']
 assert len(r['acceptance_criteria_rows'])==21 and len(r['required_content_rows'])==24
 pbis=load(K/'pbi-cases.json');orig=load(K/'analyst-cases.json');report=load(a/'analysis/report.json')
 assert len(orig)==len(report['eintraege'])==17
 for case in orig:assert case['finding']==report['eintraege'][case['report_index_zero_based']]['fund']
 events=[json.loads(l) for l in (a/'logs/events.jsonl').read_text().splitlines() if l.strip()]
 starts=[x for x in events if x.get('type')=='TOOL_CALL_STARTED'];ends=[x for x in events if x.get('type')=='TOOL_CALL_FINISHED']
 findings=[v for x in starts if x.get('tool')=='save_findings' for v in json.loads(x['arguments']['findings'])]
 assert Counter(x['statement'] for x in findings)==Counter(x['fund']['statement'] for x in report['eintraege'])
 delta=load(a/'analysis/delta-auswahl.json')['items']
 selection=[]
 for item in delta:
  matches=[c['case_id'] for c in orig if c['finding']['statement']==item['text']];assert len(matches)==1
  selection.append({'incoming_item_id':item['itemId'],'case_id':matches[0]})
 post={x['itemId']:x for x in load(f/'07-github/applied/core-before.json')['items']}
 apply=load(f/'07-ingest/applied/run-report.json')['applied'];maps={x['incomingItemId']:x for x in apply}
 for d in delta:
  op=maps[d['itemId']];assert post[op['entityId']]['text']==d['text']
 forward=load(f/'07-github/github-forward-plan.json')['operations'];snap={x['issueNumber']:x for x in load(g/'07-github/github-snapshot.json')}
 pchecks=[]
 for p in pbis:
  id=p['case_id'];op=next(x for x in forward if x.get('pbiId')==id and x['kind']=='UPDATE_ISSUE');issue=snap[op['targetIssueNumber']]
  checks={'case_id':id,'issue_number':issue['issueNumber'],'title_matches':op['title']==issue['title'],'body_matches':op['body']==issue['body'],'stored_title_matches_proposal':p['after']['pbi']['title']==p['proposal']['proposedTitle'],'stored_goal_matches_proposal':p['after']['pbi']['goal']==p['proposal']['proposedStatement'],'stored_ak_matches_proposal':p['after']['pbi']['acceptanceCriteria']==p['proposal']['proposedAcceptanceCriteria']}
  assert all(v for key,v in checks.items() if key not in ['case_id','issue_number'])
  pchecks.append(checks)
 semantic={'analyst':{c:count([x for x in r['rows'] if x['criterion']==c]) for c in ['K1','K2','K3','K4','NOV','CRIT']},'pbi':{phase:{c:count([x for x in r['rows'] if x.get('phase')==phase and x['criterion']==c]) for c in ['B1','B2','B3','B4','B5','B6']} for phase in ['before','after']},'acceptance_conditions':count(r['acceptance_criteria_rows']),'new_content_units':dict(Counter(x['after_verdict'] for x in r['required_content_rows'] if x['origin_group']=='neu')),'old_content_applicability':dict(Counter(x['applicability_verdict'] for x in r['required_content_rows'] if x['origin_group']=='alt')),'process':count(r['process_rows']),'judgment_cells':len(r['rows']),'author_responses':len(r['author_responses']),'fully_author_rated_cases':0,'author_contextual_feedback_cells':sum(x['author_review']['status']=='partial_contextual_feedback' for x in r['rows'])}
 result={'scope':'Counts of disclosed AI first judgments, not automated verification of semantic truth; technical correspondences checked separately.','technical':{'analyst_report_entries':len(report['eintraege']),'tool_starts':dict(Counter(x['tool'] for x in starts)),'tool_ends':dict(Counter(x['tool'] for x in ends)),'tool_end_outcomes':dict(Counter(x.get('outcome') for x in ends)),'finding_statements_match':True,'selected':selection,'selected_statements_equal_applied_items':True,'pbi_projection_checks':pchecks},'semantic_first_pass_counts':semantic}
 target=E/'summary.json'
 if target.exists():assert load(target)==result,'Stored summary differs from recomputation'
 print(json.dumps(result,ensure_ascii=False,indent=2))
if __name__=='__main__':main()
