#!/usr/bin/env python3
"""Read-only, scoped provenance audit. See pruefvertrag.md; standard library only.

Usage: audit_v4.py --repo ROOT --core FILE --old-audit FILE --out NEW_DIRECTORY
The output directory must not exist. No source files or original results are written.
"""
from __future__ import annotations
import argparse, collections, hashlib, json, re, unicodedata
from pathlib import Path

VERSION = '4.0'
RID = re.compile(r'\b\d{8}_\d{6}_[0-9a-f]{6}\b')
LINK_KEYS = {'consumable','fromRun','fromDelta','fromClarify','clusters','baseline',
             'transcript','sourcePath','sourceMeetingDeltaPath','sourceIngestionRun',
             'ledgerRunId','ledgerRunDir','recipeRunDir','artifactPaths','upstream',
             'meetingDeltaPath','sourceBaseline','inputPath'}
MIN_QUOTE = 40

def norm(s):
    s = unicodedata.normalize('NFKC', s or '')
    s = s.translate(str.maketrans({'„':'"','“':'"','”':'"','‘':"'",'’':"'",'–':'-','—':'-'}))
    return re.sub(r'\s+', ' ', s).strip()

def walk(obj, ptr=''):
    if isinstance(obj, dict):
        yield ptr, obj
        for k,v in obj.items():
            yield from walk(v, ptr+'/'+str(k).replace('~','~0').replace('/','~1'))
    elif isinstance(obj, list):
        for i,v in enumerate(obj): yield from walk(v,ptr+'/'+str(i))

def strings(v):
    if isinstance(v,str): yield v
    elif isinstance(v,list):
        for x in v: yield from strings(x)
    elif isinstance(v,dict):
        for x in v.values(): yield from strings(x)

def snapshot(path):
    p='/'+str(path).replace('\\','/')
    return any(s in p for s in ('core-before','core-after','/snapshots/','/checkpoints/',
                                '/logs/','core-ckpt','checkpoint')) or p.endswith('/project-state.json')

def quote_result(quote, text, transcript):
    nq=norm(quote)
    result={'quote':quote,'transcript':transcript,'normalizedLength':len(nq)}
    if text is None: status='no_bound_transcript'
    elif len(nq)<MIN_QUOTE: status='short'
    elif nq in norm(text): status='full_match'
    else:
        # Report a body-only match separately; NEVER promote it to full_match.
        body=nq.split(':',1)[-1].strip() if ':' in nq[:40] else nq
        status='body_only_prefix_differs' if body!=nq and len(body)>=MIN_QUOTE and body in norm(text) else 'no_match'
    return {**result,'status':status}

class Audit:
    def __init__(self, root):
        self.root=Path(root).resolve(); self.cache={}; self.manifest={}; self.errors=[]
        self.runs={}; self.contexts={}; self.indexes={}; self.claim_checks={}
        self.input_cache={}; self.change_cache={}; self.input_ambiguities=[]
        for base in ('runs','runsArchive'):
            for p in sorted((self.root/base).glob('*')):
                if p.is_dir() and RID.fullmatch(p.name): self.runs.setdefault(p.name,[]).append(p)
                elif p.is_dir():
                    for q in sorted(p.glob('*')):
                        if q.is_dir(): self.runs.setdefault(q.name,[]).append(q)

    def rel(self,p): return str(Path(p).resolve().relative_to(self.root))

    def path(self, value):
        if not isinstance(value,str) or '\x00' in value or len(value)>4096 or any(len(x.encode())>255 for x in value.split('/')):return None
        p=Path(value)
        p=(p if p.is_absolute() else self.root/p).resolve()
        if not p.is_relative_to(self.root): return None
        return p

    def read(self,p,text=False):
        p=Path(p).resolve()
        if not p.is_relative_to(self.root): return None
        if not p.is_file(): return None
        if p not in self.cache:
            raw=p.read_bytes(); self.manifest[self.rel(p)]={'sha256':hashlib.sha256(raw).hexdigest(),'bytes':len(raw)}
            self.cache[p]=raw.decode('utf-8-sig')
        if text:return self.cache[p]
        try:return json.loads(self.cache[p])
        except (ValueError,UnicodeError) as e:
            err={'path':self.rel(p),'error':str(e)}
            if err not in self.errors:self.errors.append(err)
            return None

    def run(self,rid):
        found=self.runs.get(rid,[])
        return found[0] if len(found)==1 else None

    def anchor(self,p,ptr,obj=None):
        self.read(p)
        return {'path':self.rel(p),'pointer':ptr,**({'object':obj} if obj is not None else {})}

    def context(self,rid):
        if rid in self.contexts:return self.contexts[rid]
        out={'runs':{},'files':{},'edges':[],'unavailable':[],'depthLimited':[]}
        queue=[('run',rid,0,None)]; seen={}
        while queue:
            kind,value,depth,via=queue.pop(0)
            key=(kind,value)
            if key in seen and seen[key]<=depth:continue
            seen[key]=depth
            if depth>3:
                out['depthLimited'].append({'kind':kind,'value':value,'via':via});continue
            if kind=='run':
                d=self.run(value)
                if d is None:
                    out['unavailable'].append({'runId':value,'reason':'ambiguous_run' if len(self.runs.get(value,[]))>1 else 'missing_run'});continue
                out['runs'][value]={'path':self.rel(d),'depth':depth}
                manifests=[p for p in d.rglob('*.json') if not snapshot(p) and
                           (p.name in {'config.json','run-report.json','ledger-run.json'} or p.name.endswith('-stage.json'))]
                for p in sorted(manifests): queue.append(('file',self.rel(p),depth,{'run':value,'role':'manifest'}))
            else:
                p=self.path(value)
                if p is None or not p.is_file():
                    out['unavailable'].append({'path':value,'via':via,'reason':'missing_file'});continue
                out['files'][self.rel(p)]={'depth':depth,'via':via}
                if p.suffix.lower()!='.json': self.read(p,True);continue
                doc=self.read(p)
                if not isinstance(doc,dict):continue
                # Only named reference fields, not every incidental run-like string.
                for ptr,o in walk(doc):
                    if ptr and p.name not in {'config.json','run-report.json'} and not p.name.endswith('-stage.json'):continue
                    for k,v in o.items():
                        if k not in LINK_KEYS:continue
                        for s in strings(v):
                            if not s:continue
                            if not (RID.search(s) or '/' in s and s.endswith(('.json','.txt'))):continue
                            edge={'from':self.rel(p),'pointer':ptr+'/'+k,'value':s}
                            if edge not in out['edges']:out['edges'].append(edge)
                            target=self.path(s)
                            matches=RID.findall(s)
                            owner=RID.findall(self.rel(p)); own=owner[-1] if owner else None
                            hop=0 if not matches or matches[-1]==own else 1
                            if target is not None and (target.is_file() or '/' in s and s.endswith(('.json','.txt'))):
                                queue.append(('file',self.rel(target),depth+hop,edge))
                            for ref in matches:
                                queue.append(('run',ref,depth+(ref!=own),edge))
        self.contexts[rid]=out;return out

    def candidates(self,rid):
        if rid in self.indexes:return self.indexes[rid]
        ctx=self.context(rid); files={}
        for r,desc in ctx['runs'].items():
            d=self.root/desc['path']
            for p in sorted(d.rglob('*.json')):
                if snapshot(p):continue
                if any(x in p.name for x in ('attempt','report','request','plan','metrics','config','summary')):continue
                files[self.rel(p)]=desc['depth']
        for p,desc in ctx['files'].items():
            if p.endswith('.json') and not snapshot(p): files[p]=min(files.get(p,99),desc['depth'])
        idx=collections.defaultdict(list)
        for rel,depth in sorted(files.items()):
            p=self.root/rel;doc=self.read(p)
            if doc is None:continue
            for ptr,o in walk(doc):
                # ID-bearing definitions only. Mere references do not qualify.
                if not any(isinstance(o.get(k),str) and o[k] for k in ('proposition','statement','text','title','rationale','decision')):continue
                for key in ('id','claimId','candidateId','decisionId','itemId','pbiId','clusterId'):
                    ident=o.get(key)
                    if not isinstance(ident,str):continue
                    authority=0 if p.name=='consumable.json' or '/applied/' in rel and p.name in {'product-backlog.json','feature-clusters.json'} else 1 if 'adjudicat' in rel else 2
                    kind='claim' if ('proposition' in o or key=='claimId') else 'candidate' if key=='candidateId' else 'decision' if key=='decisionId' else 'artifact'
                    if key=='candidateId' and not isinstance(o.get('text'),str) and not isinstance(o.get('statement'),str):continue
                    if key in {'pbiId','clusterId'} and not any(isinstance(o.get(k),str) for k in ('title','label','goal')):continue
                    idx[ident].append({'path':rel,'pointer':ptr,'object':o,'authority':authority,'depth':depth,'kind':kind})
        self.indexes[rid]=idx;return idx

    def find(self,ident,rid,kind='any'):
        if kind=='decision':
            targets=[(i,x) for i,x in enumerate(getattr(self,'core',{}).get('items',[])) if x['itemId']==ident and x['itemType']=='decision']
            if len(targets)==1:return {'id':ident,'runId':rid,'status':'resolved_core','definitionKind':'core_decision','corePointer':'/items/'+str(targets[0][0]),'targetItemId':ident}
        hits=self.candidates(rid).get(ident,[])
        if kind=='claim':
            claim_hits=[h for h in hits if h['kind']=='claim']
            hits=claim_hits or [h for h in hits if h['kind']=='artifact'] # AF/GH IDs can occupy SourceClaimIds.
        elif kind in {'candidate','decision'}:
            typed=[h for h in hits if h['kind']==kind]
            hits=typed # Never substitute an unrelated item with the same local ID.
        if not hits:return {'id':ident,'status':'unresolved','runId':rid,'expectedKind':kind}
        rank=min((h['authority'],h['depth']) for h in hits)
        selected=[h for h in hits if (h['authority'],h['depth'])==rank]
        # Different semantic definitions at the same authority remain ambiguous.
        variants=collections.defaultdict(list)
        for h in selected:
            obj=h['object']; fingerprint=json.dumps({k:obj[k] for k in ('proposition','statement','text','title','rationale','evidence','sourceUnitIds','sourceRunId','sourceArtifactId') if k in obj},sort_keys=True,ensure_ascii=False)
            variants[fingerprint].append(h)
        result={'id':ident,'runId':rid,'expectedKind':kind,'status':'resolved' if len(variants)==1 else 'ambiguous',
                'definitions':[{'path':h['path'],'pointer':h['pointer'],'kind':h['kind']} for h in selected],
                'variantCount':len(variants),'otherStageCandidates':len(hits)-len(selected)}
        if len(variants)==1:
            h=selected[0];result['definition']=self.anchor(self.root/h['path'],h['pointer'],h['object'])
            result['definitionKind']=h['kind']
            if h['kind']=='claim':result['claimChecks']=self.check_claim(h,rid)
        return result

    def producer(self,it):
        """Exact artifact role and producer: do not confuse a review decision with its target."""
        meta=it.get('metadata') or {};rid=meta.get('sourceRunId') if meta.get('legacyPbiId') or meta.get('legacyClusterId') else it.get('sourceRunId')
        d=self.run(rid)
        if not d:return []
        ident=meta.get('legacyPbiId') or meta.get('legacyClusterId') or meta.get('ingestedFrom') or it['itemId']
        if meta.get('legacyPbiId'):patterns=['backlog/applied/product-backlog.json'];keys=['pbiId']
        elif meta.get('legacyClusterId'):patterns=['clusters/applied/feature-clusters.json'];keys=['clusterId']
        elif it.get('sourceArtifactType') in {'requirements','architecture','risks','open-questions'}:
            patterns=['baselines/'+it['sourceArtifactType']+'/artifact.json'];keys=['itemId']
        else:return []
        hits=[]
        for pattern in patterns:
            for p in d.glob(pattern):
                for ptr,o in walk(self.read(p)):
                    if any(o.get(k)==ident for k in keys) and any(isinstance(o.get(k),str) for k in ('text','title','label')):
                        hits.append({'scope':'original' if rid!=it.get('sourceRunId') or it.get('version',1)>1 else 'producer',
                                     'id':ident,'runId':rid,'status':'resolved','definitionKind':'producer_artifact',
                                     'definition':self.anchor(p,ptr,o),'currentTextMatchesDefinition':norm(o.get('text') or o.get('title') or o.get('label'))==norm(it.get('text'))})
        if len(hits)>1:return []
        return hits

    def check_claim(self,h,scope):
        key=(h['path'],h['pointer'],scope)
        if key in self.claim_checks:return self.claim_checks[key]
        owners=RID.findall(h['path']);owner=owners[-1] if owners else scope
        ctx=self.context(owner); transcripts=[]
        for e in ctx['edges']:
            if e['pointer'].endswith('/transcript'):
                p=self.path(e['value'])
                if p is not None and p.is_file():transcripts.append(self.rel(p))
        transcripts=sorted(set(transcripts));transcript=transcripts[0] if len(transcripts)==1 else None
        tx=self.read(self.root/transcript,True) if transcript else None
        obj=h['object'];qs=[quote_result(e['quote'],tx,transcript) for e in obj.get('evidence',[]) if isinstance(e,dict) and isinstance(e.get('quote'),str) and e['quote']]
        unitfiles=[]
        for rid,desc in ctx['runs'].items():
            p=self.root/desc['path']/'step-00-atomic-units/output.json'
            if p.is_file():unitfiles.append(p)
        units=[]
        for uid in obj.get('sourceUnitIds',[]) or []:
            hits=[]
            for p in unitfiles:
                doc=self.read(p)
                for ptr,u in walk(doc):
                    if u.get('id')==uid and 'text' in u:hits.append((p,ptr,u))
            units.append({'id':uid,'status':'resolved' if len(hits)==1 else 'ambiguous' if hits else 'unresolved',
                          'definitions':[self.anchor(p,ptr) for p,ptr,u in hits],
                          'textInBoundTranscript':bool(len(hits)==1 and tx and norm(hits[0][2]['text']) in norm(tx))})
        result={'boundTranscript':transcript,'transcriptCandidates':transcripts,'quotes':qs,'quoteStatus':'no_quote' if not qs else 'assessed',
                'units':units,'semanticSupport':'not_assessed'}
        self.claim_checks[key]=result;return result

    def input_file(self,rid,plan):
        """Prefer the explicitly named plan input; also preserve the original configured input."""
        d=self.run(rid); cfg=self.read(d/'config.json') if d else None; result=[]
        if isinstance(plan,dict) and plan.get('sourceMeetingDeltaPath'):
            p=self.path(plan['sourceMeetingDeltaPath'])
            if p and p.is_file():result.append(p)
        if isinstance(cfg,dict) and cfg.get('fromDelta'):
            p=self.path(cfg['fromDelta'])
            if p and p.is_file():result.append(p)
        if d:
            for name in ('00-github-inbound/inbound-delta.json','04-delta/project-state.json','meeting-delta.json'):
                p=d/name
                if p.is_file() and (name!='04-delta/project-state.json' or result):result.append(p)
        return sorted(set(result))

    def input_links(self,it,rid):
        cachekey=(it['itemId'],rid)
        if cachekey in self.input_cache:return self.input_cache[cachekey]
        d=self.run(rid); results=[]
        if not d:return results
        plans=[p for p in d.rglob('plan.json') if not snapshot(p)]
        for pp in sorted(plans):
            plan=self.read(pp)
            if not isinstance(plan,dict) or not isinstance(plan.get('operations'),list):continue
            ap=pp.parent/'applied/delta.json'; adoc=self.read(ap)
            if not isinstance(adoc,dict):continue
            for ai,applied in enumerate(adoc.get('applied',[])):
                if applied.get('entityId')!=it['itemId']:continue
                ident=applied.get('incomingItemId')
                ops=[(j,o) for j,o in enumerate(plan['operations']) if o.get('incomingItemId')==ident and o.get('kind')==applied.get('kind')
                     and (o.get('targetEntityId') in (None,it['itemId']) or o.get('kind') in {'NEW','NEW_RELATED','SUPERSEDE','CONTRADICT','QUESTION'})]
                if len(ops)!=1:continue
                j,op=ops[0];inputs=[];duplicate=False
                for ip in self.input_file(rid,plan):
                    found=[(ptr,obj) for ptr,obj in walk(self.read(ip)) if re.fullmatch(r'/items/\d+',ptr) and obj.get('itemId')==ident and isinstance(obj.get('text'),str)]
                    if len(found)>1:
                        duplicate=True;self.input_ambiguities.append({'path':self.rel(ip),'id':ident,'coreItem':it['itemId']})
                    else:
                        inputs.extend(self.anchor(ip,ptr,obj) for ptr,obj in found)
                # All aliases preserved. Differing prepared/raw text is a transformation, not ID ambiguity.
                if inputs and not duplicate:
                    results.append({'runId':rid,'inputId':ident,'kind':applied.get('kind'),
                        'apply':self.anchor(ap,'/applied/'+str(ai),applied),'plan':self.anchor(pp,'/operations/'+str(j),op),
                        'inputs':inputs,'currentTextMatchesStatement':norm(op.get('statement'))==norm(it.get('text')),
                        'evidenceLevel':'plan_apply_input'})
        # Two older mini-delta cases have an explicit metadata link but no usable apply report.
        if not results and not any(x['coreItem']==it['itemId'] for x in self.input_ambiguities):
            ident=(it.get('metadata') or {}).get('ingestedFrom')
            if ident:
                for ip in self.input_file(rid,{}):
                    found=[(ptr,obj) for ptr,obj in walk(self.read(ip)) if re.fullmatch(r'/items/\d+',ptr) and obj.get('itemId')==ident and isinstance(obj.get('text'),str)]
                    if len(found)==1:
                        ptr,obj=found[0];results.append({'runId':rid,'inputId':ident,'inputs':[self.anchor(ip,ptr,obj)],
                                                      'evidenceLevel':'metadata_input_only','currentTextMatchesStatement':False})
        self.input_cache[cachekey]=results;return results

    def pbi_changes(self,it,rid):
        d=self.run(rid);out=[]
        if not d:return out
        for pp in sorted(d.rglob('pbi-change-plan.json')):
            p=self.read(pp);ap=pp.parent/'applied/pbi-update-apply-report.json';a=self.read(ap)
            if not isinstance(p,dict) or not isinstance(a,dict):continue
            updated=a.get('updatedPbis',[])
            if it['itemId'] not in updated:continue
            ops=[(j,o) for j,o in enumerate(p.get('operations',[])) if o.get('pbiId')==it['itemId']]
            align=[(j,o) for j,o in enumerate(p.get('alignments',[])) if o.get('pbiId')==it['itemId']]
            if not ops and not align:continue
            triggers=sorted({o['requirementId'] for _,o in ops if o.get('requirementId')}|{x for _,o in align for x in o.get('triggerRequirementIds',[])})
            out.append({'runId':rid,'evidenceLevel':'pbi_plan_apply','apply':self.anchor(ap,'/updatedPbis/'+str(updated.index(it['itemId']))),
                        'operations':[self.anchor(pp,'/operations/'+str(j),o) for j,o in ops],
                        'alignments':[self.anchor(pp,'/alignments/'+str(j),o) for j,o in align],
                        'triggerRequirementIds':triggers,
                        'alignmentComponentComparisons':[{'title':norm(o.get('proposedTitle'))==norm((it.get('pbi') or {}).get('title')),
                             'goal':norm(o.get('proposedStatement'))==norm((it.get('pbi') or {}).get('goal')),
                             'acceptanceCriteria':o.get('proposedAcceptanceCriteria')==(it.get('pbi') or {}).get('acceptanceCriteria')} for _,o in align],
                        'currentTitleMatchesAlignment':any(norm(o.get('proposedTitle'))==norm(it.get('text')) for _,o in align)})
        return out

    def change(self,it):
        history=it.get('history') or []; version=it.get('version',1)
        if version<=1:return {'status':'not_applicable','scope':'stored_version_changes_only'}
        runs=[]
        if it.get('sourceRunId'):runs.append(it['sourceRunId'])
        last=max(history,key=lambda h:h.get('versionId',0)) if history else {}
        # Note on previous version may name the transition to the current version.
        note_runs=RID.findall(last.get('note') or '')
        runs+=note_runs
        evidence=[]
        for rid in dict.fromkeys(runs):
            evidence.extend(x for x in self.input_links(it,rid) if x['evidenceLevel']=='plan_apply_input' and x.get('kind') in {'REFINE','SUPERSEDE'})
            if it['itemType']=='pbi':evidence.extend(self.pbi_changes(it,rid))
        valid=[x for x in evidence if x.get('currentTextMatchesStatement') or x.get('currentTitleMatchesAlignment')]
        return {'status':'linked_change_matching_text' if valid else 'linked_change_without_text_match' if evidence else 'no_linked_change',
                'runAnchors':list(dict.fromkeys(runs)),'historyVersions':[h.get('versionId') for h in history],
                'storedTextChangedFromPrevious':norm(it.get('text'))!=norm(last.get('text')) if last else None,
                'primaryTextComparison':'ingest_statement_or_pbi_title; see additional PBI component comparisons',
                'evidence':evidence,'scope':'latest_stored_version_only','fullHistoryComplete':'not_assessed'}

    def item(self,it):
        rid=it.get('sourceRunId');meta=it.get('metadata') or {};refs=[]
        for field,kind in [('sourceClaimIds','claim'),('sourceArtifactItemIds','any'),('sourceCandidateId','candidate'),('sourceDecisionId','decision')]:
            values=it.get(field) or [];values=[values] if isinstance(values,str) else values
            for ident in dict.fromkeys(values):refs.append({'field':field,**self.find(ident,rid,kind)})
        links=self.input_links(it,rid)
        # AF/GH/Analyst refs sometimes occupy the claim slot; exact linked input overrides only that same ID.
        for ref in refs:
            matches=[x for x in links if x.get('inputId')==ref['id']]
            if len(matches)==1 and ref.get('definitionKind')!='claim':
                previous={k:v for k,v in ref.items() if k not in {'field','id'}}
                ref.update({'status':'resolved_input','inputLinks':matches,'definitionKind':'input','unscopedFallbackLookup':previous,
                            'scopeResolution':'exact_plan_apply_input'})
        artifacts=self.producer(it)
        legacy=meta.get('legacyPbiId') or meta.get('legacyClusterId')
        prior=[]
        for h in it.get('history') or []:
            rs=[self.find(x,h.get('sourceRunId'),'claim') for x in h.get('sourceClaimIds') or []]
            if rs:prior.append({'version':h.get('versionId'),'runId':h.get('sourceRunId'),'references':rs})
        change=self.change(it)
        linktypes=set()
        for x in links:
            for inp in x['inputs']:
                o=inp['object'];typ=o.get('sourceArtifactType')
                sourcectx=self.context(o.get('sourceRunId')) if o.get('sourceRunId') else {'files':{}}
                hasledger=any(p.endswith('/consumable.json') for p in sourcectx['files'])
                linktypes.add({'author-statement':'author','github-issue':'github','core-analysis':'analyst'}.get(typ,
                    'analyst' if o.get('origin')=='CoreAnalyst' else 'ledger' if hasledger else 'prepared_delta'))
        if not linktypes:
            if any(x.get('definitionKind')=='claim' for x in refs):linktypes.add('ledger')
            elif it.get('sourceDecisionId'):linktypes.add('decision_reference')
            elif legacy:linktypes.add('backlog_derivation')
        direct=any(x['status'] in {'resolved','resolved_input'} for x in refs)
        return {'itemId':it['itemId'],'itemType':it['itemType'],'originLabel':it.get('origin'),'version':it.get('version'),
                'sourceRunId':rid,'identifiedInputKinds':sorted(linktypes) or ['unidentified'],
                'references':refs,'referenceCounts':dict(collections.Counter(x['status'] for x in refs)),
                'inputLinks':links,'artifactDefinitions':artifacts,'historicalReferences':prior,'latestChange':change,
                'directDefinition':direct,'inputDefinition':bool(links),'producerDefinition':bool(artifacts),
                'contributions':[],'semanticSupport':'not_assessed'}

    def all(self,core,old):
        self.core=core
        rows=[self.item(it) for it in core['items']];byid={r['itemId']:r for r in rows}
        relations=core.get('relations') or []
        for r in rows:
            for ref in r['references']:
                if ref['status']=='resolved_core':
                    target=byid.get(ref['targetItemId']);supported=bool(target and any(target[k] for k in ('directDefinition','inputDefinition','producerDefinition')))
                    r['contributions'].append({'targetItemId':ref['targetItemId'],'sourceField':'sourceDecisionId','targetHasDefinition':supported,'scope':'decision_origin_only'})
            for j,e in enumerate(relations):
                tid=None
                if r['itemType']=='pbi' and e.get('relationType')=='covers' and e.get('fromId')==r['itemId']:tid=e.get('toId')
                if r['itemType']=='feature' and e.get('relationType')=='part_of_feature' and e.get('toId')==r['itemId']:tid=e.get('fromId')
                if tid:
                    target=byid.get(tid);supported=bool(target and any(target[k] for k in ('directDefinition','inputDefinition','producerDefinition')))
                    r['contributions'].append({'targetItemId':tid,'relationPointer':'/relations/'+str(j),'targetHasDefinition':supported,
                                               'scope':'contribution_only'})
            r['anyDefinitionOrContribution']=any(r[k] for k in ('directDefinition','inputDefinition','producerDefinition')) or any(x['targetHasDefinition'] for x in r['contributions'])
            r['referenceOrInputPath']=r['directDefinition'] or r['inputDefinition'] or any(
                (byid.get(x['targetItemId'],{}).get('directDefinition') or byid.get(x['targetItemId'],{}).get('inputDefinition')) for x in r['contributions'])
            edges=[e for e in relations if e.get('fromId')==r['itemId'] and e.get('relationType')=='evidenced_by_ledger_claim']
            current={x['id'] for x in r['references'] if x['field']=='sourceClaimIds'}
            r['ledgerEdgeFieldComparison']={'edgeIds':sorted({e['toId'] for e in edges}),'currentFieldIds':sorted(current),
                'differentWithExistingEdges':bool(edges and {e['toId'] for e in edges}!=current),
                'edgeVersionBinding':'not_explicit'}
            origins=[l['targetId'] for p in core.get('provenance',[]) if p.get('itemId')==r['itemId']
                     for l in p.get('links',[]) if l.get('relation')=='produced_by' and l.get('targetType')=='run']
            r['ledgerEdgeChecks']=[]
            for e in edges:
                originruns=sorted(set(origins))
                if len(originruns)==1:
                    ref=self.find(e['toId'],originruns[0],'claim')
                    r['ledgerEdgeChecks'].append({'id':e['toId'],'scope':'origin_register_not_current_version','reference':ref})
                else:r['ledgerEdgeChecks'].append({'id':e['toId'],'scope':'no_unique_origin_register','reference':{'status':'unscoped'}})
        oldmap={r['itemId']:r for r in old.get('alleErgebnisse',[])}
        for r in rows:r['oldClass']=oldmap.get(r['itemId'],{}).get('klasse')
        def counts(group):
            return {'items':len(group),'directDefinition':sum(x['directDefinition'] for x in group),
                    'inputDefinition':sum(x['inputDefinition'] for x in group),'producerDefinition':sum(x['producerDefinition'] for x in group),
                    'anyDefinitionOrContribution':sum(x['anyDefinitionOrContribution'] for x in group),
                    'referenceOrInputPathExcludingProducerOnly':sum(x['referenceOrInputPath'] for x in group),
                    'withUnresolvedOrAmbiguousCurrentReferences':sum(any(y['status'] in {'unresolved','ambiguous'} for y in x['references']) for x in group),
                    'latestChange':dict(collections.Counter(x['latestChange']['status'] for x in group))}
        byinput={k:counts([r for r in rows if k in r['identifiedInputKinds']]) for k in sorted({k for r in rows for k in r['identifiedInputKinds']})}
        # Quote/Unit aggregation is by unique definition source+pointer, not multiplied per dependent Core item.
        unique={}
        for r in rows:
            refs=r['references']+[y for h in r['historicalReferences'] for y in h['references']]+[e['reference'] for e in r['ledgerEdgeChecks']]
            for x in refs:
                if x.get('claimChecks'):
                    a=x['definition'];unique[(a['path'],a['pointer'])]=x['claimChecks']
        return {'auditVersion':VERSION,'contract':'pruefvertrag.md','scope':'historical_snapshot_descriptive',
                'summary':counts(rows),'byItemType':{k:counts([r for r in rows if r['itemType']==k]) for k in sorted({r['itemType'] for r in rows})},
                'byIdentifiedInputKind':byinput,'inputGroupingNote':'May overlap; classifies identified evidence, not Origin label or complete execution coverage.',
                'oldClassComparison':{k:counts([r for r in rows if r['oldClass']==k]) for k in sorted(oldmap and {x.get('klasse') for x in oldmap.values()} or [])},
                'uniqueClaimDefinitions':len(unique),
                'quoteChecks':dict(collections.Counter(q['status'] for c in unique.values() for q in c['quotes'])),
                'unitReferences':dict(collections.Counter(u['status'] for c in unique.values() for u in c['units'])),
                'items':rows,'contexts':self.contexts,'readErrors':self.errors,'inputAmbiguities':self.input_ambiguities,
                'limits':['No semantic annotation','No whole-change-history completeness','No new system/model run','No automatic authorization proof from Apply record','No causal benefit measurement']}

def main():
    p=argparse.ArgumentParser();p.add_argument('--repo',required=True);p.add_argument('--core',required=True);p.add_argument('--old-audit',required=True);p.add_argument('--out',required=True)
    args=p.parse_args();dest=Path(args.out)
    if dest.exists():raise SystemExit('Output already exists; use a new directory.')
    a=Audit(args.repo);corep=a.path(args.core);oldp=a.path(args.old_audit)
    core=a.read(corep);old=a.read(oldp)
    result=a.all(core,old)
    for rel,meta in a.manifest.items():
        if hashlib.sha256((a.root/rel).read_bytes()).hexdigest()!=meta['sha256']:raise RuntimeError('Source changed: '+rel)
    result['basis']={'core':a.manifest[a.rel(corep)],'oldAudit':a.manifest[a.rel(oldp)],
                     'scriptSha256':hashlib.sha256(Path(__file__).read_bytes()).hexdigest()}
    dest.mkdir(parents=True)
    (dest/'audit.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
    (dest/'sources.json').write_text(json.dumps(a.manifest,ensure_ascii=False,indent=2,sort_keys=True)+'\n')
    print(json.dumps({k:result[k] for k in ('summary','byItemType','quoteChecks','unitReferences','uniqueClaimDefinitions','readErrors')},ensure_ascii=False,indent=2))

if __name__=='__main__':main()
