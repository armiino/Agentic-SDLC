"""Read-only retrospective inventory; writes only to --out. No model calls.

The intermediate Ledger is derived from preserved decision records, not an
original before-refine file. Structural comparisons are not semantic ratings.
"""
import argparse, collections, hashlib, json, pathlib, subprocess
import xml.etree.ElementTree as ET

ap = argparse.ArgumentParser()
ap.add_argument('--repo', type=pathlib.Path, required=True)
ap.add_argument('--tested-manifest', type=pathlib.Path, required=True)
ap.add_argument('--out', type=pathlib.Path, required=True)
args = ap.parse_args()
r, out = args.repo.resolve(), args.out.resolve()
out.mkdir(parents=True, exist_ok=True)
seen = {}
def raw(rel):
    p = r / rel
    b = p.read_bytes()
    seen[str(p.relative_to(r))] = hashlib.sha256(b).hexdigest()
    return b
def js(rel): return json.loads(raw(rel))
def save(name, data):
    (out/name).write_text(json.dumps(data, ensure_ascii=False, indent=2, sort_keys=True)+'\n')
def diff(a, b):
    return {k: {'before': a.get(k), 'after': b.get(k)}
            for k in sorted(a.keys() | b.keys()) if a.get(k) != b.get(k)}
def unique(rows, field):
    result = {x[field]: x for x in rows}
    assert len(result) == len(rows), (field, 'duplicate')
    return result
def git(*cmd):
    return subprocess.check_output(['git', *cmd], cwd=r)

run = 'runsArchive/ledger/20260704_131614_018865/'
units = unique(js(run+'step-00-atomic-units/output.json')['units'], 'id')
validated = js(run+'step-03-facet-validation/output.json')['entries']
before = unique([x['entry'] for x in validated], 'id')
queue = unique(js(run+'step-03b-adjudicated/queue.json')['items'], 'itemId')
audit = js(run+'step-03b-adjudicated/adjudicated-ledger.json')
records = unique(audit['records'], 'itemId')
final = unique(js(run+'step-03b-adjudicated/consumable.json')['claims'], 'id')
gate = js(run+'step-03b-adjudicated/gate.json')
old_recall = js(run+'step-03b-adjudicated/recall-fast.consumable.json')
config = js(run+'config.json')
raw('Thesis-Docs/thesis-story/ledger-aera/ledger-iteration-notes.md')
raw(run+'logs/events.jsonl')
assert queue.keys() == records.keys()
assert all(queue[k]['action'] == records[k]['action'] for k in queue)

# Project uses claim-producing actions first, then attach operations. In this
# preserved queue, attach targets are unique; their records contain final targets.
middle = unique(audit['carriedApprovedClaims'], 'id').copy()
for rec in audit['records']:
    if rec.get('resultingClaim') and rec['action'] != 'attach_evidence':
        middle[rec['resultingClaim']['id']] = rec['resultingClaim']
attach = [x for x in audit['records'] if x['action']=='attach_evidence']
assert len({x['referenceTarget'] for x in attach}) == len(attach)
for rec in attach:
    assert rec['resultingClaim']['id'] == rec['referenceTarget']
    middle[rec['referenceTarget']] = rec['resultingClaim']
assert middle.keys() == final.keys()

source_rel = 'input/transcripts/Interview-Einrichtung.txt'
source = raw(source_rel)
reference_rel = 'input/eval-labels/Interview-Einrichtung.reference-ledger.json'
reference = raw(reference_rel)
july = '1579f8f577edffa259124560e877b86c370ff8f1'
norm = lambda s: ' '.join(s.split())
source_matches = {
    'gitCommit': july,
    'transcriptEqualToCommittedBytes': git('show', july+':'+source_rel)==source,
    'referenceEqualToCommittedBytes': git('show', july+':'+reference_rel)==reference,
    'unitsNotFoundInCurrentTranscriptIgnoringWhitespace': [k for k,u in units.items()
        if norm(u['text']) not in norm(source.decode('utf-8-sig'))],
    'limit': 'Commit equality plus embedded source text; no independently signed run-time source hash.'}
assert source_matches['transcriptEqualToCommittedBytes']
assert not source_matches['unitsNotFoundInCurrentTranscriptIgnoringWhitespace']

paired = []
for key in sorted(final):
    b, m, f = before.get(key), middle[key], final[key]
    involved = [rec['itemId'] for rec in audit['records']
                if rec.get('resultingClaim', {}) and rec['resultingClaim']['id']==key]
    ids = set((b or {}).get('sourceUnitIds',[])) | set(m.get('sourceUnitIds',[])) | set(f.get('sourceUnitIds',[]))
    paired.append({'id':key, 'before':b, 'afterApplyDerived':m, 'afterRefineStored':f,
        'applyChanges':diff(b,m) if b else None, 'refineChanges':diff(m,f),
        'decisionItems':involved, 'sourceUnits':[units[i] for i in sorted(ids) if i in units],
        'unresolvedUnitIds':sorted(ids-units.keys()), 'semanticRating':'not_performed'})
decision_rows=[]
for key, rec in records.items():
    q=queue[key]
    decision_rows.append({'itemId':key,'queue':q,'record':rec,
        'unit':units.get(q.get('unitId')), 'semanticRating':'not_performed'})
new_ids = sorted(final.keys()-before.keys())
old_ids = sorted(before)
summary = {
    'status':'structural feasibility analysis; no semantic quality score',
    'runId':config['runId'],'extractionModel':config['model'],
    'source':source_matches, 'units':len(units),
    'beforeClaims':len(before),'queueItems':len(queue),
    'actions':dict(collections.Counter(x['action'] for x in records.values())),
    'afterClaims':len(final),'addedClaims':len(new_ids),
    'removedClaimIds':sorted(before.keys()-final.keys()),
    'existingClaimsExactlyUnchangedBeforeToFinal':sum(before[k]==final[k] for k in old_ids),
    'existingClaimsExactlyUnchangedDerivedApplyToFinal':sum(middle[k]==final[k] for k in old_ids),
    'pendingFacetsInDerivedApply':sum(x.get('facetStatus')=='pending' for x in middle.values()),
    'pendingFacetsInStoredFinal':sum(x.get('facetStatus')=='pending' for x in final.values()),
    'newClaimsUnchangedIdPropositionEvidenceUnitsDuringRefine':sum(
        all(middle[k].get(f)==final[k].get(f) for f in ['id','proposition','evidence','sourceUnitIds']) for k in new_ids),
    'allClaimUnitRefsResolved':not any(x['unresolvedUnitIds'] for x in paired),
    'gatePass':gate['pass'], 'adjudicatedByLabel':audit['adjudicatedBy'],
    'adjudicatedAt':audit['adjudicatedAt'],
    'oldOverlapMetric':old_recall.get('metrics'),
    'limits':['The operator label does not authenticate a human or prove independent review.',
      'The intermediate state is derived; the original consumable was overwritten by refinement.',
      'Original acquisition logs end before adjudication; refinement model attributed from development notes.',
      'Added claims, repaired fields and resolved source IDs do not establish semantic benefit.']}
save('hitl-summary.json',summary)
save('paired-claims.json',paired)
save('decision-items.json',decision_rows)

tested=json.loads(args.tested_manifest.read_text())
adoption_rel='Thesis-Docs/Writing/claude-writing/Technische-Uebernahme-R75-B20-2026-09-11/'
adoption=js(adoption_rel+'uebernahmeprotokoll.json')
for x in adoption['codeFiles']: tested[x['path']]=x['after']
save('tested-files-20260911.json',tested)
comparison=[]
for p,h in sorted(tested.items()):
    actual=hashlib.sha256(raw(p)).hexdigest() if (r/p).exists() else None
    comparison.append({'path':p,'testedSha256':h,'currentSha256':actual,'equal':actual==h})
extensions={'.cs','.csproj','.txt','.json'}
inventory={}
for base in ['AgenticSdlc.Host','AgenticSdlc.HumanReview','AgenticSdlc.Tests']:
    files=[p for p in (r/base).rglob('*') if p.is_file() and
           not {'bin','obj'}.intersection(p.parts) and p.suffix in extensions]
    inventory[base]={'currentFiles':len(files),'outsideTestedManifest':[
        str(p.relative_to(r)) for p in files if str(p.relative_to(r)) not in tested]}

ns={'t':'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
trx=ET.fromstring(raw(adoption_rel+'full-suite.trx'))
test_results=[x.attrib for x in trx.findall('.//t:UnitTestResult',ns)]
names=['AdjudicationRefineTests','BaselineStageOutputRegressionTests','TranscriptSpeakerRegressionTests','PipelineFullMessagesTests']
selected=[x for x in test_results if any(n in x['testName'] for n in names)]
families=collections.Counter(x['testName'].split('(')[0].rsplit('.',1)[0] for x in test_results)
save('dated-test-families.json',dict(families))
raw(adoption_rel+'smoke.log')
raw(adoption_rel+'build.log')
events={}
for run_id in ['20260818_084712_765eea','20260909_190518_f58d6a','20260909_190157_506d47']:
    events[run_id]=[]
    for line in raw('runs/fullworkflow/'+run_id+'/logs/events.jsonl').decode().splitlines():
        x=json.loads(line); typ=x.get('type','')
        if typ.startswith('STAGE_') or typ in ['PIPELINE_END','PIPELINE_PAUSED','REQ_INGEST_GATE','REQ_INGEST_HUMAN_GATE']:
            events[run_id].append(x)
    js('runs/fullworkflow/'+run_id+'/config.json')
save('selected-run-events.json',events)
save('current-tested-comparison.json',comparison)
save('selected-test-results.json',selected)
(out/'current-tracked-code.diff').write_bytes(git('diff','--','AgenticSdlc.Host','AgenticSdlc.Tests','AgenticSdlc.HumanReview'))
# The commit immediately before the historical run is only a date anchor;
# the historical working tree may have contained uncommitted changes.
date_anchor=git('log','-1','--before=2026-08-18T08:47:12Z','--format=%H').decode().strip()
committed_names=git('diff','--name-only',date_anchor,'HEAD','--','AgenticSdlc.Host','AgenticSdlc.HumanReview','AgenticSdlc.Tests').decode().splitlines()
save('chain-summary.json',{
    'head':git('rev-parse','HEAD').decode().strip(),
    'currentComparedToTestedFiles':len(comparison),'unequalFiles':[x['path'] for x in comparison if not x['equal']],
    'currentCodeInventory':inventory, 'datedTestCounters':trx.find('.//t:Counters',ns).attrib,
    'selectedTestResults':len(selected),'selectedOutcomes':dict(collections.Counter(x['outcome'] for x in selected)),
    'smoke':js(adoption_rel+'smoke-protocol.json'),
    'historicalDateAnchorNotRuntimeCodeIdentity':date_anchor,
    'committedFilesChangedAfterDateAnchor':committed_names,
    'limit':'File correspondence to a dated tested snapshot, not new execution or a full current E2E proof.'})
save('source-manifest.json',seen)
assert all(hashlib.sha256((r/p).read_bytes()).hexdigest()==h for p,h in seen.items())
print(json.dumps({'hitl':summary,'filesCompared':len(comparison),
                  'codeDifferences':sum(not x['equal'] for x in comparison),
                  'readSourcesUnchanged':len(seen)},ensure_ascii=False,indent=2))
