from pathlib import Path
import hashlib,json,collections,argparse
p=argparse.ArgumentParser();p.add_argument('--repo',default='/Users/armino/devProjects/Agentic-SDLC');args=p.parse_args()
W=Path(__file__).resolve().parent;R=Path(args.repo).resolve()
d=json.loads((W/'final-result/audit.json').read_text());manifest=json.loads((W/'final-result/sources.json').read_text())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
assert d['basis']['core']['sha256']=='afcba4bac58ba82fb30b4a413d977f6474be384a377765c0bdb0dafc7bcba22c'
assert d['basis']['oldAudit']['sha256']=='0a540e2fc9bda50afe12ee589b7f488914a6ceb670b9fdcdd093c579b6298d04'
assert sha(R/'tools/eval/w2_traceability_audit.py')=='d51332394628c9753596e5e2271211f86b94b0553f6da16cf00a480201e8ed88'
for rel,meta in manifest.items():assert sha(R/rel)==meta['sha256'],rel
for file in ['audit.json','sources.json']:
 assert (W/'final-result'/file).read_bytes()==(W/'reproduction'/file).read_bytes(),file
cache={};checked=0;objects=0
def resolve(rel,ptr):
 if rel not in cache:cache[rel]=json.loads((R/rel).read_text(encoding='utf-8-sig'))
 value=cache[rel]
 for part in ptr.split('/')[1:]:
  part=part.replace('~1','/').replace('~0','~');value=value[int(part)] if isinstance(value,list) else value[part]
 return value
def verify(o):
 global checked,objects
 if isinstance(o,dict):
  if set(['path','pointer'])<=set(o) and o['path'] in manifest and o['path'].endswith('.json'):
   actual=resolve(o['path'],o['pointer']);checked+=1
   if 'object' in o:assert actual==o['object'],(o['path'],o['pointer']);objects+=1
  for x in o.values():verify(x)
 elif isinstance(o,list):
  for x in o:verify(x)
verify(d)
assert len(d['items'])==237 and len({r['itemId'] for r in d['items']})==237
assert not d['readErrors'] and not d['inputAmbiguities']
testlog=(W/'controls-final.log').read_text();assert 'Ran 34 tests' in testlog and testlog.rstrip().endswith('OK')
cases={r['itemId']:r for r in d['items']}
# Acceptance checks grounded in the earlier independent artifact diagnosis.
for iid in ['REQ-42','ARCH-11','REQ-89','REQ-90']:
 assert any(x['evidenceLevel']=='plan_apply_input' for x in cases[iid]['inputLinks']),iid
for iid in ['REQ-08','REQ-32']:
 assert cases[iid]['latestChange']['status']=='no_linked_change',iid
assert {x['targetItemId'] for x in cases['PBI-047']['contributions']}=={'REQ-83','REQ-89','REQ-90'}
assert not cases['ARCH-40']['referenceOrInputPath']
assert sum(bool(r['ledgerEdgeFieldComparison']['differentWithExistingEdges']) for r in d['items'])==5
assert all(e['reference']['status']=='resolved' for r in d['items'] for e in r['ledgerEdgeChecks'])
facts={'sourceFilesVerifiedUnchanged':len(manifest),'jsonPointerChecks':checked,'embeddedObjectsVerified':objects,
       'byteIdenticalReproduction':True,'oldAuditScriptUnchanged':True,'controlledFixtureTests':34,
       'realCaseAcceptanceChecks':['REQ-42','ARCH-11','REQ-89','REQ-90','REQ-08','REQ-32','PBI-047','ARCH-40'],
       'noReadErrors':True,'resultSha256':sha(W/'final-result/audit.json'),'scriptSha256':sha(W/'audit_v4.py')}
(W/'verification.json').write_text(json.dumps(facts,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(facts,ensure_ascii=False,indent=2))
