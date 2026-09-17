"""Read-only check of an author export. No LLM calls, no changes to thesis/results.
Usage: python3 auswerten.py /path/to/export.json
"""
import json,sys,collections,gzip
from pathlib import Path
P=Path(__file__).resolve().parent
d=json.loads((P/'pruefdaten.json').read_text() if (P/'pruefdaten.json').exists() else gzip.decompress((P/'pruefdaten.json.gz').read_bytes()).decode())
x=json.loads(Path(sys.argv[1]).read_text())
assert x['schema']=='autorenpruefung-v1' and x['package_id']==d['package_id'],'Falsches Paket'
fields={c['id']+'::'+f['key']:(c,f) for c in d['cases'] for f in c['fields']}
answers=x['answers'];assert set(answers)<=set(fields),'Unbekannte Antwortkennungen'
for id,a in answers.items():
 c,f=fields[id]
 assert a['case_id']==c['id'] and a['field_key']==f['key']
 assert a['status'] in ['confirmed','corrected','unclear','assessed']
 assert a['reviewed_originals'] and a['reviewer'].strip() and a['reviewed_at']
 assert a['ai_value']==f['ai_value']
 if a['status']=='confirmed':assert f['ai_value'] is not None and a['author_value']==f['ai_value']
 else:assert a['reason'].strip()
 if a['status'] in ['corrected','assessed']:assert a['author_value']
report={'mode':d['mode'],'package_id':d['package_id'],'answers':len(answers),'answer_fields':len(fields),'modules':{},'coverage':{}}
for m,meta in d['modules'].items():
 ids=[i for i,(c,f) in fields.items() if c['module']==m]
 cnt=collections.Counter(answers[i]['status'] if i in answers else 'open' for i in ids)
 report['modules'][m]={'title':meta['title'],'status_counts':dict(cnt),'all_fields_processed':not cnt['open'],'unresolved_present':bool(cnt['unclear'])}
for run in sorted({c['id'].split('-')[1] for c in d['cases'] if c['id'].startswith('COV-')}):
 ids=[i for i,(c,f) in fields.items() if c['id'].startswith('COV-'+run+'-')]
 vals=[answers.get(i,{}).get('author_value') for i in ids]
 if not all(v in ['full','partial','none'] for v in vals):
  report['coverage'][run]={'status':'noch nicht vollständig konsolidierbar','denominator':len(ids),'decidable':sum(v in ['full','partial','none'] for v in vals)}
 else:
  cnt=collections.Counter(vals);report['coverage'][run]={'status':'vollständig menschlich bewertet; Abgleich und Textintegration noch erforderlich','counts':dict(cnt),'denominator':len(ids),'full_coverage':cnt['full']/len(ids),'touched_coverage':(cnt['full']+cnt['partial'])/len(ids)}
report['limits']=['Keine unabhängige Interrater-Reliabilität: KI-Urteile waren sichtbar.','Unklare Urteile sind bearbeitet, aber keine bestätigte Erfüllung.','Eine neue Referenzentscheidung erfordert Prüfung aller betroffenen Bewertungen; keine automatische Goldänderung.','Korrekturen sind vor Thesisübernahme anhand der Begründung zu konsolidieren.','Diese technische Prüfung beurteilt nicht die fachliche Richtigkeit.']
print(json.dumps(report,ensure_ascii=False,indent=2))
