#!/usr/bin/env python3
"""Read-only integrity and count check of the retrospective first assessment."""
from pathlib import Path
from hashlib import sha256
import json,argparse,subprocess,sys
E=Path(__file__).resolve().parent
ap=argparse.ArgumentParser(description=__doc__);ap.add_argument('--package-root',type=Path,default=E.parent);K=ap.parse_args().package_root.resolve()
def load(p):return json.loads(p.read_text())
problems=[]
for filename in ['input-manifest.json','result-manifest.json']:
 for rel,expected in load(E/filename)['files'].items():
  p=K/rel[3:] if rel.startswith('../') else E/rel
  if not p.is_file():problems.append({'file':rel,'error':'missing'});continue
  b=p.read_bytes()
  if len(b)!=expected['bytes'] or sha256(b).hexdigest()!=expected['sha256']:problems.append({'file':rel,'error':'content_changed'})
run=subprocess.run([sys.executable,str(E/'measure.py'),'--package-root',str(K)],capture_output=True,text=True)
if run.returncode:problems.append({'error':'count_or_material_check_failed','detail':run.stderr})
else:
 recomputed=json.loads(run.stdout)
 if recomputed!=load(E/'summary.json'):problems.append({'error':'summary_mismatch'})
projection=subprocess.run([sys.executable,str(E/'check-projection-chain.py'),'--package-root',str(K)],capture_output=True,text=True)
if projection.returncode:problems.append({'error':'projection_chain_failed','detail':projection.stderr or projection.stdout})
print(json.dumps({'passed':not problems,'problems':problems,'scope':'File identities, rating completeness, evidence pointers, recount and selected archived correspondences. Does not validate semantic judgments or independent annotation.','criterion_cells':138,'analyst_cases':17,'pbi_issue_pairs':3,'first_pass_author_feedback_cases':3,'followup_feedback_cases':len(load(E/'author-followup.json')['answers']),'pending_author_questions':load(E/'author-followup.json')['pending_cases'],'fully_author_rated_cases':0},ensure_ascii=False,indent=2))
raise SystemExit(0 if not problems else 1)
