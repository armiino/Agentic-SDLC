from pathlib import Path
from hashlib import sha256
from datetime import datetime, timezone
from collections import Counter
import json, re, shutil

W=Path(__file__).resolve().parent
R=Path('/Users/armino/devProjects/Agentic-SDLC')
T=R/'Thesis-Docs/Writing/claude-writing'
P=W/'package'
assert not P.exists(), 'Preparation already exists; do not silently rerun selection'
P.mkdir()
def h(b): return sha256(b).hexdigest()
def emit(name,obj):
    path=P/name;path.parent.mkdir(parents=True,exist_ok=True)
    path.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n')
def canonical(x):return json.dumps(x,ensure_ascii=False,sort_keys=True,separators=(',',':'))
originals=[]
def capture(path,role):
    rel=path.relative_to(R).as_posix();b=path.read_bytes()
    target=P/'originals'/rel;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(b)
    row={'repository_path':rel,'package_path':target.relative_to(P).as_posix(),'sha256':h(b),'bytes':len(b),'role':role}
    originals.append(row)
    return b,row
known_files=[
 'thesis-evidence/w2/z2-fallblaetter.md',
 'Thesis-Docs/Writing/claude-writing/P10-HITL-Governance-Nachweis-2026-09-15.md',
 'Thesis-Docs/Writing/claude-writing/P10-HITL-Kettenabgleich-2026-09-15.md',
 'Thesis-Docs/Writing/claude-writing/P10-E2E-Bestandspruefung-2026-09-15.md',
 'Thesis-Docs/Writing/claude-writing/P10-4a-Umsetzung-2026-09-14.md',
 'Thesis-Docs/Writing/claude-writing/P10-4a-Nachvollziehbarkeit-Erklaerung-und-Nachweis-2026-09-14.md',
 'Thesis-Docs/Writing/claude-writing/P10-3b-Herkunftsaudit-v4-2026-09-13.md'
]
known=set(); known_sources=[]
for rel in known_files:
    b,row=capture(R/rel,'known-case-basis')
    ids=sorted(set(re.findall(r'20\d{6}_\d{6}_[a-f0-9]{6}',b.decode())))
    known.update(ids);known_sources.append({**row,'run_ids':ids})
controls=[
 {'case_id':'K01','run_id':'20260818_084712_765eea','incoming_id':'REQ-01','known_context':'F1: rejected REFINE of REQ-42'},
 {'case_id':'K02','run_id':'20260817_123342_f23f06','incoming_id':'AF-1','known_context':'F2: REQ-42 refinement and human PBI editing'},
 {'case_id':'K03','run_id':'20260804_121433_eb181a','incoming_id':'REQ-05','known_context':'F3: contradiction of REQ-32 and decision effects'},
 {'case_id':'K04','run_id':'20260820_172043_802e63','incoming_id':'GH-45','known_context':'GitHub addition in visit example'}
]
known.update(c['run_id'] for c in controls)
groups=['transcript','author_delta','github','other_delta']
families={'new_requirement':['NEW','NEW_RELATED'],'existing_reference':['REFINE','SUPERSEDE','CONTRADICT','RESTATE','ALREADY_DECIDED']}
shutil.copy2(W/'protokoll.md',P/'protokoll.md')
rules={
 'version':1,'created_utc':datetime.now(timezone.utc).isoformat(),'protocol_sha256':h((P/'protokoll.md').read_bytes()),
 'timing':'After structural metadata inventory, before additional case selection or semantic judgments',
 'scope':'All immediate runs/fullworkflow directories; final 07-ingest/plan.json operations. Requirement path only.',
 'route_order':groups,'family_order':list(families),'families':families,'max_new_cases':8,
 'ranking':'SHA256 UTF-8 p10-core-content-v1|<runId>|<zero-based operation index>|<incomingItemId>, ascending lexicographic digest',
 'one_selected_operation_per_run':True,'exclude_known_run_ids':sorted(known),'known_sources':known_sources,
 'no_outcome_filter':True,'no_replacement_on_missing_or_negative_evidence':True,'controls':controls,
 'not_covered':'OPEN_QUESTION has no new quota; clarification-only, projection-only, architecture ingestion and non-fullworkflow callers outside selection',
 'no_statistical_independence_or_representativeness_claim':True
}
emit('selection-rules.json',rules)

def route(c):
    ep=c.get('entryPoint'); delta=str(c.get('fromDelta') or '').replace('\\','/')
    if ep=='github':return 'github','explicit entryPoint'
    if ep=='transcript':return 'transcript','explicit entryPoint'
    if not ep and c.get('transcript'):return 'transcript','legacy transcript field; no entryPoint'
    if ep=='delta' or (not ep and delta):
        return ('author_delta' if 'steward/author-front/' in delta else 'other_delta'),('explicit delta + stored path' if ep else 'legacy fromDelta field')
    return ep or 'unknown','other explicit entryPoint' if ep else 'no sufficient route metadata'

runs=[];ops=[];errors=[]
for directory in sorted(p for p in (R/'runs/fullworkflow').iterdir() if p.is_dir()):
    config_path=directory/'config.json';plan_path=directory/'07-ingest/plan.json'
    c={};plan={};config_source=None;plan_source=None
    for path,label in [(config_path,'config'),(plan_path,'ingest_plan')]:
        if not path.exists():continue
        b,row=capture(path,'inventory_'+label)
        try:x=json.loads(b)
        except Exception as e:
            errors.append({'path':str(path.relative_to(R)),'error':str(e)});continue
        if label=='config':c=x;config_source=row
        else:plan=x;plan_source=row
    g,why=route(c)
    plan_ops=plan.get('operations',[])
    rr={
      'run_id':directory.name,'route_group':g,'route_basis':why,'entryPoint':c.get('entryPoint'),
      'transcript_path':c.get('transcript'),'from_delta_path':c.get('fromDelta'),'from_clarify_path':c.get('fromClarify'),
      'timestamp_utc':c.get('timestampUtc'),'model_label':c.get('ledgerModel'),
      'config_path':config_source['package_path'] if config_source else None,'config_sha256':config_source['sha256'] if config_source else None,
      'plan_path':plan_source['package_path'] if plan_source else None,'plan_sha256':plan_source['sha256'] if plan_source else None,
      'plan_id':plan.get('planId'),'plan_created_utc':plan.get('createdUtc'),'source_delta_path':plan.get('sourceMeetingDeltaPath'),
      'operation_count':len(plan_ops),'known_run':directory.name in known
    }
    runs.append(rr)
    for i,op in enumerate(plan_ops):
        key=f'{directory.name}|{i}|{op.get("incomingItemId","")}'
        family=next((f for f,kinds in families.items() if op.get('kind') in kinds),None)
        ops.append({
         'operation_key':key,'run_id':directory.name,'operation_index':i,'incoming_id':op.get('incomingItemId'),
         'proposed_kind':op.get('kind'),'proposed_target':op.get('targetEntityId'),'feature_hint':op.get('featureKey'),
         'route_group':g,'route_basis':why,'family':family,'known_run':rr['known_run'],
         'plan_path':rr['plan_path'],'plan_sha256':rr['plan_sha256'],
         'operation_sha256':h(canonical(op).encode()),'rank_sha256':h(('p10-core-content-v1|'+key).encode()),
         'has_statement':bool(op.get('statement'))
        })
assert not errors,errors
summary={
 'run_directories':len(runs),'configs':sum(bool(r['config_path']) for r in runs),'plans':sum(bool(r['plan_path']) for r in runs),
 'nonempty_plans':sum(r['operation_count']>0 for r in runs),'empty_plans':sum(bool(r['plan_path']) and not r['operation_count'] for r in runs),
 'operations':len(ops),'run_groups':dict(Counter(r['route_group'] for r in runs)),
 'operation_groups':{g:dict(Counter(o['proposed_kind'] for o in ops if o['route_group']==g)) for g in groups},
 'known_run_count_in_inventory':sum(r['known_run'] for r in runs),
 'dates_are_historical':True,'semantic_judgments_performed':False
}
emit('inventory.json',{'summary':summary,'runs':runs,'operations':ops,'parse_errors':errors})
selected=[];slots=[];used=set()
for g in groups:
 for f in families:
    eligible=sorted([o for o in ops if o['route_group']==g and o['family']==f and not o['known_run']],key=lambda o:(o['rank_sha256'],o['operation_key']))
    skipped=[o['operation_key'] for o in eligible if o['run_id'] in used]
    pick=next((o for o in eligible if o['run_id'] not in used),None)
    slot={'route':g,'family':f,'eligible_count':len(eligible),'ranked_operation_keys':[o['operation_key'] for o in eligible],'skipped_already_selected_run':skipped,'selected_operation_key':pick['operation_key'] if pick else None}
    slots.append(slot)
    if pick:
      case={**pick,'case_id':f'N{len(selected)+1:02d}','role':'additional','selected_slot':g+'/'+f}
      selected.append(case);used.add(pick['run_id'])
expanded_controls=[]
for control in controls:
    matches=[o for o in ops if o['run_id']==control['run_id'] and o['incoming_id']==control['incoming_id']]
    assert len(matches)==1,control
    expanded_controls.append({**matches[0],**control,'role':'known_control'})
selection={'created_utc':datetime.now(timezone.utc).isoformat(),'rules_sha256':h((P/'selection-rules.json').read_bytes()),'inventory_sha256':h((P/'inventory.json').read_bytes()),'slots':slots,'additional_cases':selected,'controls':expanded_controls,'status':'selected; no semantic judgments'}
emit('selected-cases.json',selection)

fields=['source_to_proposal','source_to_applied_state','classification','content_preservation_proposal','content_preservation_applied_state','authorization_and_effect','relations_and_downstream']
template={'status':'UNASSESSED; blank fields are not negative judgments','criteria_protocol':'protokoll.md','cases':[{'case_id':c['case_id'],'operation_key':c['operation_key'],'role':c['role'],'evidence':[],'source_components':[],'human_input':[],'ratings':{f:None for f in fields},'notes':[]} for c in selected+expanded_controls]}
emit('assessment-template.json',template)

# Record presence only after the choice. It cannot influence the ranking or replace a case.
case_files=[
 '07-ingest/ingestion-gate-report.json','07-ingest/ingestion-summary.json','07-ingest/human-decisions.json','07-ingest/ingest-gate-decisions.json',
 '07-ingest/applied/core-before.json','07-ingest/applied/delta.json','07-ingest/applied/run-report.json','07-ingest/applied/affected-view.json',
 '07-pbi-update/pbi-change-plan.json','07-pbi-update/human-decisions.json','07-pbi-update/applied/core-before.json','07-pbi-update/applied/pbi-update-apply-report.json','07-pbi-update/applied/github-sync-delta.json',
 '07-decision/decision-plan.json','07-decision/decision-gate-decisions.json','07-decision/applied/core-before.json','07-decision/applied/decision-apply-report.json',
 '07-github/github-forward-plan.json','07-github/applied/core-before.json','07-github/applied/github-forward-apply-report.json',
 '00-github-inbound/inbound-delta.json','00-github-inbound/snapshot/issue-comments.json','00-github-inbound/snapshot/github-issues-snapshot.json',
 '01-ledger/consumable.json','01-ledger/source-units.json','04-delta/meeting-delta.json','04-delta/project-state.json',
 'logs/events.jsonl'
]
availability=[]
for c in selected+expanded_controls:
    d=R/'runs/fullworkflow'/c['run_id']
    row={'case_id':c['case_id'],'files':{f:(d/f).exists() for f in case_files},'note':'Existence only; no judgment of completeness, association, approval or success'}
    availability.append(row)
emit('evidence-presence-after-selection.json',availability)

basis_files=[
 'Thesis-Docs/Writing/claude-writing/chap3/3.1/chap3.1.tex',
 'Thesis-Docs/Writing/claude-writing/chap3/3.2/chap3.2.tex',
 'Thesis-Docs/Writing/claude-writing/chap4/chap4.10table-a.tex',
 'Thesis-Docs/Writing/claude-writing/kapitel-7/tex-v2/chap7.5/chap7.5.tex',
 'AgenticSdlc.Host/FullWorkflow/07-tore/ingestion/IngestionModels.cs',
 'AgenticSdlc.Host/FullWorkflow/07-tore/pbiupdate/PbiUpdateModels.cs',
 'AgenticSdlc.Host/FullWorkflow/07-tore/ingestion/IngestionApply.cs',
 'AgenticSdlc.Host/FullWorkflow/07-tore/ingestion/IngestionGate.cs'
]
for rel in basis_files:capture(R/rel,'current_protocol_basis_not_historical_execution_proof')
emit('originals-manifest.json',{'captured_utc':datetime.now(timezone.utc).isoformat(),'files':originals})
shutil.copy2(Path(__file__),P/'scripts/build_preparation.py') if (P/'scripts').exists() else None
(P/'scripts').mkdir(exist_ok=True);shutil.copy2(Path(__file__),P/'scripts/build_preparation.py')
table=['| Fall | Eingang | Lauf | Eingangs-ID | Vorgeschlagene Art |','|---|---|---|---|---|']
for c in selected+expanded_controls:table.append(f'| {c["case_id"]} | {c["route_group"]} | `{c["run_id"]}` | `{c["incoming_id"]}` | `{c["proposed_kind"]}` |')
readme='''# Ergänzende Core-Inhaltsprüfung: Vorbereitung

Stand: 15.09.2026. **Auswahl und Kriterien gesichert; keine neuen Inhaltsurteile und keine neuen Qualitätszahlen.**

Das [Protokoll](protokoll.md) konkretisiert den bestehenden Phase-10-Auftrag. Es ergänzt den technischen Herkunftsnachweis um die inhaltliche Prüfung ausgewählter Einordnungs- und Fortschreibungsketten. Die Auswertung ist retrospektiv und entwicklungsbekannt; sie setzt keinen Methodenwechsel voraus.

## Inventar und Auswahl

'''+f'Inventarisiert: {summary["run_directories"]} Laufverzeichnisse, {summary["configs"]} Konfigurationen, {summary["plans"]} Anforderungspläne ({summary["nonempty_plans"]} mit Operationen, {summary["empty_plans"]} leer), {summary["operations"]} vorgeschlagene Operationen. Die Hauptdaten stehen in [inventory.json](inventory.json). Keine Erfolgszählung.\n\n'+f'Ausgewählt: {len(selected)} zusätzliche Fälle und {len(expanded_controls)} bekannte Kontrollfälle. Die [Regeln](selection-rules.json) wurden vor der Auswahl gesichert; [selected-cases.json](selected-cases.json) enthält Rangfolge und etwaige leere Fächer. Die Auswahl nutzt weder Apply-Berichte noch Inhaltsurteile.\n\n'+'\n'.join(table)+'''

## Prüfbarkeit und nächster Schritt

Die [Originaldateien](originals-manifest.json) sichern die Konfigurationen, Pläne, bekannten Fallberichte und die bezeichnete aktuelle Kriteriengrundlage mit Originalpfad und SHA-256. Diese Sicherung enthält noch nicht alle Rohquellen, damaligen Zielbestände und Nachzustände für die spätere Inhaltsprüfung. Deren Dateiexistenz wurde erst nach Auswahl erfasst ([Präsenzliste](evidence-presence-after-selection.json)); Existenz belegt weder eine vollständige Kette noch einen Erfolg. Die Fallauswahl wird wegen fehlender Belege nicht ausgetauscht. Ein späteres Belegpaket muss die tatsächlich benötigten Quellen-/Zustandsartefakte vor Bewertung ergänzen und einzeln hashen.

Als Nächstes Quellen und historische Zustandsketten für diese Fälle sichern, lesbare Gegenüberstellungen erstellen und K1–K5 anwenden. Fachlich relevante Mehrdeutigkeiten dem Autor ohne vorgegebenes KI-Verdikt vorlegen. Das [leere Bewertungsgerüst](assessment-template.json) enthält noch keine Urteile. Die bekannte Entwicklungssituation und fehlende unabhängige Annotation bleiben ausgewiesen. Aus Kontrollen und zusätzlich ausgewählten Fällen wird keine gemeinsame Erfolgsquote gebildet.

Die Datei `scripts/verify_selection.py` prüft die gesicherten Originalhashes, rekonstruiert die Inventur aus den Kopien und berechnet die Auswahl erneut. Sie bewertet keine Inhalte. Der Erzeuger unter `scripts/build_preparation.py` dokumentiert die einmalige Vorbereitung mit lokalen Pfaden; für die portable Prüfung ist ausschließlich `verify_selection.py` vorgesehen.

Der [aktive Phase-10-Plan](../../../Thesis-Docs/Writing/claude-writing/Phase-10-Gesamtabnahme-2026-09-13.md) bleibt die einzige Aufgabenführung. Aus dieser Vorbereitung entsteht kein Overleaf-Sync, Modelllauf oder Codeeingriff.
'''
(P/'README.md').write_text(readme)
print(json.dumps({'summary':summary,'selected':[{k:c[k] for k in ['case_id','run_id','incoming_id','route_group','proposed_kind']} for c in selected],'empty_slots':[s for s in slots if not s['selected_operation_key']],'original_files':len(originals),'original_bytes':sum(r['bytes'] for r in originals)},ensure_ascii=False,indent=2))
