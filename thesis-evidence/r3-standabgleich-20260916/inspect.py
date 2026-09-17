"""Read-only R3 inventory; writes only to explicitly supplied output folder.
No model calls, no new tests, no semantic annotation. Python standard library.
"""
from pathlib import Path
import argparse, hashlib, json, collections, subprocess

ap=argparse.ArgumentParser()
ap.add_argument('--repo',type=Path,required=True)
ap.add_argument('--out',type=Path,required=True)
a=ap.parse_args(); r=a.repo.resolve(); out=a.out.resolve(); out.mkdir(parents=True,exist_ok=True)
seen={}
def read(rel):
    p=r/rel; b=p.read_bytes(); seen[str(p.relative_to(r))]={'sha256':hashlib.sha256(b).hexdigest(),'bytes':len(b)}; return b
def js(rel): return json.loads(read(rel))
def save(name,data): (out/name).write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n')
rows=[]; errors=[]
selected={}
suffixes={'765eea','f23f06','eb181a','a0b172','802e63','6e75fa','b7747b','895d18','f58d6a','506d47'}
for p in sorted((r/'runs/fullworkflow').iterdir()):
    if not p.is_dir(): continue
    rel=str(p.relative_to(r)); ep=p/'logs/events.jsonl'; es=[]
    if ep.exists():
        for n,line in enumerate(read(rel+'/logs/events.jsonl').decode().splitlines(),1):
            if not line.strip(): continue
            try: es.append({'line':n,**json.loads(line)})
            except Exception as ex: errors.append({'run':p.name,'line':n,'error':str(ex)})
    config=js(rel+'/config.json') if (p/'config.json').exists() else {}
    ends=[x for x in es if x.get('type')=='PIPELINE_RUN_DONE']
    row={'run':p.name,'eventsPresent':ep.exists(),'entryPoint':config.get('entryPoint'),
         'transcript':config.get('transcript'),'lastEvent':es[-1] if es else None,
         'end':ends[-1] if ends else None,
         'ledgerDone':sum(x.get('type')=='STAGE_LEDGER_DONE' for x in es),
         'forward':[x for x in es if x.get('type')=='GITHUB_FWD_DONE']}
    rows.append(row)
    if p.name.split('_')[-1] in suffixes:
        artifacts={}
        for f in sorted(p.rglob('*.json')):
            if 'checkpoints' in f.parts: continue
            if 'decisions' in f.name or f.name in {'run-report.json','delta.json','pbi-update-apply-report.json','github-forward-apply-report.json','decision-apply-report.json'}:
                artifacts[str(f.relative_to(p))]=js(str(f.relative_to(r)))
        selected[p.name]={'config':config,'events':[x for x in es if x.get('type','').startswith(('PIPELINE_','STAGE_')) or x.get('type') in {'GATE_ANSWERED','PBI_UPDATE_DONE','GITHUB_FWD_DONE','INGEST_APPLIED','DECISION_APPLIED'}],'artifacts':artifacts}
completed=[x for x in rows if x['end'] and x['end'].get('exit')==0]
save('fullworkflow-inventory.json',{'scope':'All run directories and event/configuration contents; no general success rate.','runs':rows,'parseErrors':errors})
save('governance-artifacts.json',selected)
base='Thesis-Docs/Writing/claude-writing/P10-HITL-Kettenabgleich-2026-09-15/'
tested=js(base+'tested-files-20260911.json')
comparison=[]
for p,h in sorted(tested.items()):
    actual=hashlib.sha256(read(p)).hexdigest() if (r/p).is_file() else None
    comparison.append({'path':p,'testedSha256':h,'currentSha256':actual,'equal':actual==h})
extra=[]
for folder in ['AgenticSdlc.Host','AgenticSdlc.Tests','AgenticSdlc.HumanReview']:
    for p in sorted((r/folder).rglob('*')):
        if p.is_file() and not {'bin','obj'}.intersection(p.parts) and p.suffix in {'.cs','.csproj','.txt','.json'}:
            rel=str(p.relative_to(r))
            if rel not in tested: extra.append(rel); read(rel)
save('current-tested-comparison.json',{'files':comparison,'newFilesWithinOriginalScope':extra,'scope':'File identity against dated tested inventory, not renewed test execution; excludes build outputs.'})
summary={'runDirectories':len(rows),'missingEvents':sum(not x['eventsPresent'] for x in rows),'parseErrors':len(errors),
         'terminalExitZero':len(completed),'completedTranscriptWithForward':[x['run'] for x in completed if x['transcript'] and x['ledgerDone'] and x['forward']],
         'runsFromSeptember':[x['run'] for x in rows if x['run']>='20260901'],
         'testedFilesCompared':len(comparison),'changedTestedFiles':[x['path'] for x in comparison if not x['equal']],
         'newFilesWithinOriginalScope':extra,
         'headNotFinalArtifact':subprocess.check_output(['git','rev-parse','HEAD'],cwd=r,text=True).strip(),
         'limitation':'No exact historical working-tree identity inferred from run date or HEAD; no current full E2E inferred from file equality.'}
save('summary.json',summary); save('source-manifest.json',seen)
assert all(hashlib.sha256((r/p).read_bytes()).hexdigest()==v['sha256'] for p,v in seen.items())
print(json.dumps(summary,ensure_ascii=False,indent=2))
