#!/usr/bin/env python3
"""Read-only verification of this dated evaluation package; no system execution."""
from pathlib import Path
from hashlib import sha256
from collections import Counter
import json
ROOT=Path(__file__).resolve().parent

def load(path): return json.loads((ROOT/path).read_text())
def verify():
 problems=[]
 manifest=load('material-manifest.json')
 for entry in manifest['originals']:
  p=ROOT/entry['package_path']
  if not p.is_file() or sha256(p.read_bytes()).hexdigest()!=entry['sha256']:problems.append('original_changed:'+entry['package_path'])
 inventory=load('package-manifest.json')
 for rel,meta in inventory['files'].items():
  p=ROOT/rel
  if not p.is_file() or sha256(p.read_bytes()).hexdigest()!=meta['sha256']:problems.append('package_changed:'+rel)
 a=Path('originals/runs/core-analysis/20260821_203951_db6050')
 f=Path('originals/runs/fullworkflow/20260821_204808_042823')
 g=Path('originals/runs/fullworkflow/20260821_211304_bfbbd7')
 report=load(a/'analysis/report.json'); core=load(f/'07-ingest/applied/core-before.json')
 ids={i['itemId'] for i in core['items']}
 cases=load('analyst-cases.json')
 assert len(cases)==len(report['eintraege'])==17
 assert all(c['finding']==report['eintraege'][n]['fund'] for n,c in enumerate(cases))
 assert all(i in ids for e in report['eintraege'] for i in e['fund']['ankerIds'])
 events=[json.loads(l) for l in (ROOT/a/'logs/events.jsonl').read_text().splitlines() if l.strip()]
 saves=[e for e in events if e.get('type')=='TOOL_CALL_STARTED' and e.get('tool')=='save_findings']
 findings=[item for e in saves for item in json.loads(e['arguments']['findings'])]
 assert Counter(x['statement'] for x in findings)==Counter(e['fund']['statement'] for e in report['eintraege'])
 selected=load(a/'analysis/delta-auswahl.json')['items']
 assert len(selected)==4
 assert all(x['text'] in {e['fund']['statement'] for e in report['insDelta']} for x in selected)
 snap={i['issueNumber']:i for i in load(g/'07-github/github-snapshot.json')}
 post={i['itemId']:i for i in load(f/'07-github/applied/core-before.json')['items']}
 plan=load(f/'07-pbi-update/pbi-change-plan.json')
 plans={p['pbiId']:p for p in plan['alignments']}
 forward=load(f/'07-github/github-forward-plan.json')['operations']
 for pbi in ['PBI-003','PBI-032','PBI-046']:
  op=next(o for o in forward if o.get('pbiId')==pbi and o['kind']=='UPDATE_ISSUE')
  assert op['title']==snap[op['targetIssueNumber']]['title'] and op['body']==snap[op['targetIssueNumber']]['body']
  assert post[pbi]['pbi']['title']==plans[pbi]['proposedTitle']
  assert post[pbi]['pbi']['goal']==plans[pbi]['proposedStatement']
  assert post[pbi]['pbi']['acceptanceCriteria']==plans[pbi]['proposedAcceptanceCriteria']
 ratings=load('ratings-template.json')
 assert ratings['semantic_evaluation_started'] is False
 assert len(ratings['rows'])==138
 assert all(r['verdict'] is None and r['author_review']['status']=='pending' for r in ratings['rows'])
 return {'passed':not problems,'problems':problems,'original_files':len(manifest['originals']),'package_files':len(inventory['files']),'report_entries':len(cases),'saved_finding_statements_match':True,'selected_entries':len(selected),'pbi_issue_pairs':3,'empty_rating_cells':len(ratings['rows']),'scope':'File identity and deterministic material correspondences only; no semantic judgments or model/workflow/GitHub execution.'}
if __name__=='__main__':
 result=verify();print(json.dumps(result,ensure_ascii=False,indent=2));raise SystemExit(0 if result['passed'] else 1)
