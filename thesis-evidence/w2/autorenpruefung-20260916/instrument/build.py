from pathlib import Path
import json, re, hashlib, shutil, collections, datetime, html

R=Path('/Users/armino/devProjects/Agentic-SDLC')
HERE=Path(__file__).resolve().parent
OUT=HERE/'paket'
OUT.mkdir(exist_ok=True)
(OUT/'belege').mkdir(exist_ok=True)
T='Thesis-Docs/Writing/claude-writing'
E='thesis-evidence/w2'
AR=E+'/agent-artifact-evaluation-20260916'
CR=E+'/core-content-audit-20260915'
LR=E+'/ledger-repetitions-20260915'
sources={}; contexts={}; cases=[]; notes=[]; copied={}
def sha(b):return hashlib.sha256(b).hexdigest()
def source(p):
 p=Path(p);p=p if p.is_absolute() else R/p;p=p.resolve()
 assert p.is_relative_to(R),p
 rel=str(p.relative_to(R))
 if rel in copied:return copied[rel]
 b=p.read_bytes();sid='S'+sha(rel.encode())[:14]
 dest='belege/'+sid+p.suffix
 (OUT/dest).write_bytes(b)
 sources[sid]={'id':sid,'path':rel,'file':dest,'sha256':sha(b),'bytes':len(b),'text':b.decode('utf-8-sig')}
 copied[rel]=sid
 return sid
def read(p):return json.loads(sources[source(p)]['text'])
def txt(p):return sources[source(p)]['text']
def context(cid,label,data,source_ids=()):
 contexts[cid]={'id':cid,'label':label,'data':data,'sources':list(source_ids)};return cid
def field(key,label,value,reason,options=None,origin=None):
 return {'key':key,'label':label,'ai_value':value,'ai_reason':reason or 'Keine gesonderte Begründung in dieser Datei.', 'options':options or [],'origin':origin or ''}
def case(cid,module,title,question,blocks,fields,ctx=(),src=(),warning=''):
 assert fields
 cases.append({'id':cid,'module':module,'title':title,'question':question,'blocks':blocks,'fields':fields,'contexts':list(dict.fromkeys(ctx)),'sources':list(dict.fromkeys(src)),'warning':warning})
def block(label,data):return {'label':label,'data':data}
def number_key(s):return int(re.search(r'(\d+)$',s).group(1))
def verdict(d):
 if isinstance(d,str):return d
 for k,v in d.items():
  if k.startswith('verd') or k in ['A','verdict','urteil']:return v
 raise ValueError(d)
def units(p):
 raw=txt(p);parts=re.split(r'^\[(AU-\d+)\]\s*',raw,flags=re.M)
 return {parts[i]:parts[i+1].strip() for i in range(1,len(parts),2)}
U={'IE':units('input/eval-labels/Interview-Einrichtung.numbered.txt'),'M2':units('input/eval-labels/meeting-2-extended.numbered.txt')}
G={}
for tag,fn in [('IE','Interview-Einrichtung'),('M2','meeting-2-extended')]:
 gp='input/eval-labels/'+fn+'.w2-gold.json';np='input/eval-labels/'+fn+'.numbered.txt'
 g=read(gp);G[tag]={x['id']:x for x in g['units']}
 context('quelle-'+tag,'Vollständige nummerierte Quelle',U[tag],[source(np)])
 context('gold-'+tag,'Vollständiger Referenzbestand v2',g,[source(gp)])
def excerpts(tag,ids):
 valid=[]
 for uid in ids:
  if uid not in U[tag]:raise ValueError((tag,uid))
  valid.append({'Kennung':uid,'Originaltext':U[tag][uid]})
 return valid
def gkey(tag,old):return f'G-{tag}-{number_key(old):03d}'
RULES=[('match','input/eval-labels/w2-matchregeln.md'),('gold','input/eval-labels/w2-gold-leitfaden.md'),('revision','input/eval-labels/w2-gold-aenderungslog-v2.md'),('ledger',LR+'/leitfaden.md'),('analyst',AR+'/protokoll-v1.md'),('core',CR+'/protokoll.md'),('methodik',T+'/kapitel-7/tex-v2/chap7.2/chap7.2.tex'),('anhang',T+'/anhang/anhang-evaluation.tex')]
for key,p in RULES:context('regeln-'+key,'Prüfregeln: '+key,txt(p),[source(p)])
MODS={
 '01':'01 · Analyst: 17 Vorschläge', '02':'02 · Drei PBIs vor und nach der Änderung',
 '03':'03 · Zwölf Fortschreibungsfälle', '04':'04 · Abdeckung der vier Hauptläufe',
 '05':'05 · Zwei Ledger-Wiederholungen', '06':'06 · Claim-Korrektheit und Quellenstützung',
 '07':'07 · Stufenanalyse vor der Kanonisierung', '08':'08 · Lückentypen, Zitate und Sonderzuordnung',
 '09':'09 · Historische Fallserie und Prozessbelege', '10':'10 · Referenzbestand und Quellvollständigkeit',
 '11':'11 · Ergänzende Modellreihe (alter Goldstand)'}
COV=['full','partial','none'];QUAL=['E','T','N','NB','NA']
CLAIMS={};RUNS={}
def add_run(key,run,tag,arm,rawpath=None):
 p=rawpath or (f'runs/ledger/{run}/capture/lcr-machine.json' if arm=='Ledger' else f'runs/arm-f/{run}/output.json')
 d=read(p);raw=d.get('entries',d.get('statements',[]));assert raw
 cs=[]
 for r in raw:
  x=r.get('entry',r)
  cs.append({'id':x['id'],'statement':x.get('proposition',x.get('statement')),'sourceUnitIds':x.get('sourceUnitIds',[]),'evidence':x.get('evidence',[]),'raw':x})
 assert len({c['id'] for c in cs})==len(cs)
 CLAIMS[key]={c['id']:c for c in cs};RUNS[key]={'run':run,'tag':tag,'arm':arm,'source':source(p)}
 context('claims-'+key,f'Gesamter Ausgabe-Bestand: {arm}, {run}',cs,[source(p)])
 if 'missSignal' in d:context('signals-'+key,'Gespeicherte Prüfhinweise',d['missSignal'],[source(p)])
 return cs
add_run('LR','20260905_164650_8189ea','IE','Ledger')
add_run('LA','20260905_165151_2dde4d','IE','Ledger',LR+'/originals/runs/ledger/20260905_165151_2dde4d/capture/lcr-machine.json')
add_run('LB','20260905_165658_1271bb','IE','Ledger',LR+'/originals/runs/ledger/20260905_165658_1271bb/capture/lcr-machine.json')
add_run('LM','20260905_164206_e30646','M2','Ledger')
add_run('FI','20260906_143052_6540e2','IE','F')
add_run('FM','20260906_141518_12d046','M2','F')
add_run('ST','20260905_164650_8189ea','IE','Ledger', 'runs/ledger/20260905_164650_8189ea/step-01-candidate/output.json')
def relevant(key,ids,reason=''):
 cs=CLAIMS[key]
 mentioned=set(re.findall(r'\b(?:CAN|C|REQ|DEC|PROC|ARCH|RISK|OQ)-\d{2,4}\b',reason))
 return [c for c in cs.values() if c['id'] in mentioned or set(c['sourceUnitIds'])&set(ids)]
def covcase(key,gid,v,reason,src,module,extra=None,ref=None):
 run=RUNS[key];tag=run['tag'];g=ref or G[tag][gid]
 b=[block('Referenzaussage',g),block('Originalquelle: zugeordnete Einheiten',excerpts(tag,g['sourceUnitIds'])),block('Naheliegende Ausgaben (Kennung im Urteil oder gemeinsame Quell-Unit; keine abschließende Trefferliste)',relevant(key,g['sourceUnitIds'],reason))]
 if extra:b.append(block('Zusätzlicher historischer Kontext',extra))
 case('COV-'+key+'-'+gid,module,f'{gid} · {run["arm"]} · {run["run"][-6:]}', 'Deckt der Aussagentext des gesamten Ausgabebestands die Referenzaussage vollständig, teilweise oder nicht ab? Mehrere Claims dürfen gemeinsam tragen. Gespeicherte Zitate und die nur verlinkte Rohquelle zählen hier nicht als Claim-Inhalt. Bei partial/none auch den gesamten Ausgabebestand durchsuchen.',b,[field('coverage','Abdeckung',v,reason,COV,src)],['claims-'+key,'quelle-'+tag,'regeln-match','regeln-revision'],[source(src),run['source']], 'Einheiten daneben und spätere Festlegungen können die Lesart verändern. Der vollständige Kontext ist unten verfügbar.')
# Endstand: Stress aus Konsolidierung vom 08.09.; Meeting aus Gold-v2-Blättern.
conp=E+'/eval/w2-nachreview-konsolidierung.json';con=read(conp)
for row in con['beziehungstabelle']:
 gid=row['gold']
 for key,prefix,reasonkey in [('LR','ledger','ledgerBeleg'),('FI','f','fClaims')]:
  reason=row[reasonkey]
  changes=[x for x in con['entscheidungen']+con['bleibtMitBerichtigterBegruendung'] if x.get('id')==gid and x.get('konfig')==('ledger' if key=='LR' else 'f')]
  if changes:reason+='\nKonsolidierung: '+'; '.join(x.get('grund','') for x in changes)
  covcase(key,gid,row[prefix+'End'],reason,conp,'04')
for key,name in [('LM','ledger-e30646'),('FM','f-12d046')]:
 p=E+'/eval/w2-goldv2-eval-'+name+'.json';d=read(p)
 for old,row in d['goldUrteile'].items():covcase(key,gkey('M2',old),verdict(row),row.get('beleg',str(row)),p,'04')
p=LR+'/results/coverage-details.json'
for row in read(p):
 if row['run']=='R':continue
 key='L'+row['run'];reason=row['reason']+'\nArchivierte Claim-Zuordnung: '+', '.join(row.get('actual_claim_ids',[]))
 covcase(key,row['gold_id'],row['verdict'],reason,p,'05')
# Quellenprüfung: jüngster Ledger-Stand, nicht überholte 47/47-Sammelwertung.
p=LR+'/results/claim-quality-details.json'
for row in read(p):
 key='L'+row['run'];grefs=row['sourceUnitIds']
 cs=CLAIMS[key][row['claim_id']]
 case('CLM-'+key+'-'+row['claim_id'],'06',row['claim_id']+' · Ledger '+row['run_id'][-6:], 'Zwei getrennte Fragen: Verfälscht der Claim die Quelle? Und tragen gerade seine angegebenen Quell-Units den gesamten Inhalt? Eine plausible Zusatzannahme kann korrekt erscheinen, aber nicht vollständig gestützt sein.',[block('Claim',cs),block('Tatsächlich referenzierte Originalstellen',excerpts('IE',grefs))],[field('correctness','Inhaltliche Korrektheit',row['correctness'],row['reason'],['no_distortion_found','distortion_found'],p),field('support','Semantische Quellenstützung',row['support'],row['reason'],['directly_supported','inferentially_supported','partially_supported','unsupported'],p)],['quelle-IE','regeln-ledger'],[source(p),RUNS[key]['source']])
for key,name in [('LM','ledger-e30646'),('FI','6540e2'),('FM','12d046')]:
 p=E+'/eval/w2-official-'+('ledger-eval-e30646' if key=='LM' else 'eval-'+name)+'.json';d=read(p)
 for cid,c in CLAIMS[key].items():
  old=d.get('claimUrteile',{}).get(cid)
  reason=json.dumps(old,ensure_ascii=False) if old else d['blockA'].get('praezisionsBefund','Historisches Sammelurteil')
  v='nicht vollständig' if key=='FI' and cid=='C-088' else 'vollständig'
  if not old:reason='Nur auf Gesamtebene dokumentiertes Urteil; keine nachträglich erfundene Einzelbegründung. '+reason
  fs=[field('correctness','Historisches Korrektheitsurteil',v,reason,['vollständig','nicht vollständig'],p),field('support','Historische Quellenstützung',v,reason,['vollständig','teilweise','nicht gestützt'],p)]
  if old and 'gold' in old:fs.append(field('goldmatch','Einordnung gegenüber dem Referenzbestand',old['gold'],reason,['gematcht','gold_escape'],p))
  case('CLM-'+key+'-'+cid,'06',cid+' · '+RUNS[key]['arm']+' '+RUNS[key]['run'][-6:],'Prüfe Quellenwiedergabe und Stützung getrennt. Die damaligen zusammenfassenden Urteile werden sichtbar übernommen; die fehlende Einzelbegründung bleibt gekennzeichnet.',[block('Claim',c),block('Angegebene Originalstellen',excerpts(RUNS[key]['tag'],c['sourceUnitIds']))],fs,['quelle-'+RUNS[key]['tag'],'gold-'+RUNS[key]['tag'],'regeln-match','regeln-methodik'],[source(p),RUNS[key]['source']])
# Stufenanalyse inkl. fünf nachträglich korrigierter A-Urteile.
p=E+'/eval/w2-z1-stufenanalyse-8189ea.json';d=read(p)
for gid,row in d['urteilsblatt'].items():
 v=row['A'];reason=row['belegA']
 if gid in con['aBAbgleich']['einzel']:
  corr=con['aBAbgleich']['einzel'][gid];v=re.search(r'->(full|partial|none)',corr).group(1);reason+='\nKonsolidierung vom 08.09.: '+corr
 covcase('ST',gid,v,reason,p,'07',{'Endzustand separat in Modul 04':next(x['ledgerEnd'] for x in con['beziehungstabelle'] if x['gold']==gid),'Konsolidierungsdatei':conp})
# Zusatzanalysen: Lückentypen plus gespeicherter Beleginhalt.
for row in con['beziehungstabelle']:
 for key,prefix in [('LR','ledger'),('FI','f')]:
  typ=row[prefix+'Typ']
  if typ is None:continue
  g=G['IE'][row['gold']]
  case('TYP-'+key+'-'+g['id'],'08',g['id']+' · Lückentyp '+RUNS[key]['arm'],'Welchen Typ hat der tatsächlich fehlende Bestandteil? Die Kategorien sind keine Wichtigkeitsrangfolge.',[block('Referenzaussage',g),block('Originalstellen',excerpts('IE',g['sourceUnitIds'])),block('Naheliegende Claims',relevant(key,g['sourceUnitIds'],row['ledgerBeleg' if key=='LR' else 'fClaims']))],[field('type','Typ des fehlenden Inhalts',typ,con['lueckentypen']['regel'],['F','U','Q','O'],conp)],['claims-'+key,'quelle-IE'],[source(conp)])
reviews=list((R/T/'kapitel-7').glob('Review-Coverage-109-*.md'));assert len(reviews)==1,reviews
rp=str(reviews[0].relative_to(R));rt=txt(rp)
for m in re.finditer(r'^### (G-IE-\d+).*?\n([\s\S]*?)(?=^### |^## |\Z)',rt,re.M):
 gid,body=m.groups();hit=re.search(r'\*\*Mit gespeicherter Evidenz:\*\* (voll|teilweise|kein Match)\.',body)
 if not hit:continue
 g=G['IE'][gid];v={'voll':'full','teilweise':'partial','kein Match':'none'}[hit.group(1)]
 # endgültige Claim-Urteile überschreiben keine separate Belegbetrachtung.
 if gid=='G-IE-107':
  pass
 case('EVI-'+gid,'08',gid+' · Claim plus tatsächlich gespeichertes Zitat','Prüfe den erweiterten Umfang: Claims plus gespeicherte Zitate. Ein Inhalt, der nur in der über IDs erreichbaren Rohquelle steht, zählt hier nicht. Ergebnis nicht zur Claim-Abdeckung addieren.',[block('Referenzaussage',g),block('Naheliegende Claims einschließlich gespeicherter Zitate',relevant('LR',g['sourceUnitIds'],body)),block('Historischer Einzelbefund (P1-Angaben darin können vor der Konsolidierung liegen)',body)],[field('evidence_coverage','Abdeckung einschließlich gespeicherter Zitate',v,hit.group(0)+body[hit.end():],COV,rp)],['claims-LR','quelle-IE','regeln-match'],[source(rp),source(conp)],'Diese Zusatzprüfung stammt aus der Review-Runde; G-IE-049 wurde anschließend ausdrücklich bestätigt. Nur das Belegurteil dieser Karte prüfen, keine historischen P1-Zahlen übernehmen.')
g=G['IE']['G-IE-028'];sig=next(x for x in contexts['signals-LR']['data'] if x['unitId']=='AU-0124')
case('SIGNAL-028','08','G-IE-028 · zusätzliche semantische Hinweiszuordnung','Trägt der Hinweis AU-0124 fachlich eine Zuordnung zur Lücke G-IE-028, obwohl die deterministische Unit-Schnittmenge dafür nicht genügt? Es geht um Adressierbarkeit, nicht um tatsächlich erfolgte Reparatur.',[block('Referenzaussage',g),block('Quelle der Referenz und des Hinweises',excerpts('IE',sorted(set(g['sourceUnitIds']+['AU-0124'])))),block('Gespeicherter Hinweis',sig)],[field('semantic_link','Semantische Zusatzzuordnung','getragen',con['ledgerHinweisUndSzenario']['regel'],['getragen','nicht getragen','mehrdeutig'],conp)],['quelle-IE','signals-LR'],[source(conp)])
for gid in ['G-IE-062','G-IE-107']:
 g=G['IE'][gid]
 case('EVI-'+gid,'08',gid+' · Zusatzprüfung nach korrigiertem Claim-Urteil','Für diese später herabgestufte Claim-Abdeckung ist im ausgelesenen Einzelabschnitt kein gesondertes finales Zitat-Urteil ausgewiesen. Bitte den erweiterten Umfang selbst prüfen; ein fehlendes Einzelurteil wird nicht durch eine neue KI-Annahme ersetzt.',[block('Referenzaussage',g),block('Originalstellen',excerpts('IE',g['sourceUnitIds'])),block('Claims und gespeicherte Zitate',relevant('LR',g['sourceUnitIds']))],[field('evidence_coverage','Abdeckung einschließlich gespeicherter Zitate',None,'Eigenes finales Zitat-Einzelurteil nicht gesichert. Die aggregierte Angabe 60/109 wird hier nicht in ein erfundenes Einzelurteil zurückübersetzt.',COV,rp)],['claims-LR','quelle-IE'],[source(rp),source(conp)])
# Referenz: vorhandene menschliche Konsolidierung bleibt Geschichte, neue Felder sind offen.
for tag in G:
 for gid,g in G[tag].items():
  case('GOLD-'+gid,'10',gid+' · Referenzaussage','Ist die Referenzaussage quellengetreu, sinnvoll abgegrenzt und über die angegebenen Units begründet? Prüfe Typ und Verbindlichkeit. Diese neue Runde ist nicht bereits durch die frühere Konsolidierung erledigt.',[block('Referenzeintrag',g),block('Originalstellen',excerpts(tag,g['sourceUnitIds']))],[field('gold','Referenzeintrag','als Referenz enthalten','KI-Entwurf, bereits durch Autor konsolidiert; aktuelle Gegenprüfung noch offen.',None,'input/eval-labels')],['quelle-'+tag,'regeln-gold','regeln-revision'])
 for uid,text in U[tag].items():
  matches=[g for g in G[tag].values() if uid in g['sourceUnitIds']]
  case('BACK-'+tag+'-'+uid,'10',uid+' · Rückprüfung Quelle → Referenz','Enthält diese Quell-Unit nach dem Annotationsleitfaden noch relevante Inhalte, die im gesamten Referenzbestand fehlen? Die angezeigten Zuordnungen sind nur die direkte ID-Schnittmenge. Prüfe auch verteilte Aussagen und spätere Präzisierungen.',[block('Original-Unit',text),block('Direkt referenzierende Gold-Einträge',matches)],[field('completeness','Vollständigkeit der Referenz aus Quellsicht',None,'Keine neue KI-Bewertung vorbereitet; dies ist eine zusätzliche menschliche Rückprüfung.', ['keine weitere relevante Aussage gefunden','fehlende Referenzaussage gefunden','mehrdeutig'])],['quelle-'+tag,'gold-'+tag,'regeln-gold'])
# Analyst und PBI: Bewertungszellen exakt aus letzter gespeicherter Datei.
ratingsp=AR+'/evaluation/ratings-first-pass.json';ratings=read(ratingsp)
corep=AR+'/originals/runs/fullworkflow/20260821_204808_042823/07-ingest/applied/core-before.json'
core=read(corep);context('core-S0','Vollständiger damaliger Core vor der Einordnung',core,[source(corep)])
criterion_desc={}
for line in txt(AR+'/protokoll-v1.md').splitlines():
 bits=[x.strip() for x in line.split('|')]
 if len(bits)>=5 and re.fullmatch(r'[KB][1-6]',bits[1]):criterion_desc[bits[1]]=bits[2]+': '+bits[3]
criterion_desc.update({'NOV':'Neuheit gegenüber dem damaligen Projektbestand, nicht wissenschaftliche Neuheit. Gesamten S0 durchsuchen.','CRIT':'Tragfähigkeit der historischen Kritikerentscheidung; fehlender damaliger Prompt bleibt eine Grenze.'})
def register_evidence(rows,base):
 found=[]
 for row in rows:
  for e in row.get('evidence',[]):
   f=e.get('file') if isinstance(e,dict) else e
   if not f:continue
   p=(R/base/f).resolve()
   if not p.exists():
    notes.append('Nicht automatisch aufgelöster Originalverweis: '+str(p));continue
   found.append(source(p))
 return list(dict.fromkeys(found))
for cid in [f'AN-{i:02d}' for i in range(1,18)]+['PBI-003','PBI-032','PBI-046']:
 dp=AR+'/dossiers/'+cid+'.json';d=read(dp);rows=[x for x in ratings['rows'] if x['case_id']==cid]
 fields=[]
 for n,row in enumerate(rows):
  k=row['criterion'];phase=row.get('phase','')
  opt=ratings['allowed_verdicts']['NOV' if k=='NOV' else 'CRIT' if k=='CRIT' else 'K1-K4/B1-B6']
  fields.append(field(k+'-'+phase,(('Vorher: ' if phase=='before' else 'Nachher: ' if phase=='after' else '')+criterion_desc.get(k,k)),row['verdict'],row['reason'],opt,ratingsp))
 src=[source(dp),source(ratingsp)]+register_evidence(rows,AR+'/evaluation')
 if cid.startswith('AN'):
  blocks=[block('Agentenvorschlag und Herleitung',d['finding']),block('Historischer Filterstatus',{'Status':d['reported_status'],'Grund':d['critic_reason']}),block('Bestandsanker und weitere erwähnte Elemente',d['anchors_and_mentions']),block('Benachbarte Elemente',d['neighbor_items']),block('Beziehungen',d['one_hop_relations'])]
  case(cid,'01',cid+' · '+d['finding']['statement'][:100],'Prüfe alle Kriterien getrennt. Ein sinnvoller Planungsauftrag muss die spätere Antwort noch nicht enthalten; seine Ausgangstatsachen müssen stimmen. Frühere Chat-Kommentare gelten nicht automatisch als Kriterienfreigabe.',blocks,fields,['core-S0','regeln-analyst'],src)
 else:
  extras=[x for x in ratings['required_content_rows'] if x['id'].startswith(cid)]
  aks=[x for x in ratings['acceptance_criteria_rows'] if x['case_id']==cid]
  for row in extras:
   for k,label in [('applicability_verdict','Fortgeltung'),('before_verdict','Vorher enthalten'),('after_verdict','Nachher enthalten')]:
    fields.append(field(row['id']+'-'+k,row['id']+' · '+label+' · '+row['content'],row[k],row['reason'],['applicable','conflicting','unresolved','NA'] if k=='applicability_verdict' else ['erhalten','teilweise','fehlend','absichtlich abgelöst','NB','NA'],ratingsp))
  for row in aks:fields.append(field('AK-'+row['phase']+'-'+str(row['criterion_index_one_based']),('Vorher' if row['phase']=='before' else 'Nachher')+' AK '+str(row['criterion_index_one_based'])+': '+row['text'],row['verdict'],row['observable_condition']+'\n'+row['reason'],QUAL,ratingsp))
  blocks=[block('Vorher: vollständiges PBI',d['before']),block('Agentenvorschlag zur Änderung',d['proposal']),block('Historische menschliche Entscheidung',d['human_decision']),block('Nachher: vollständiges PBI',d['after']),block('Anforderungen und Architektur vor der Änderung',d['reference_items_before_alignment']),block('Referenzen am Analyst-Start',d['reference_items_at_analyst_start']+d.get('additional_reference_items_at_analyst_start',[])),block('Issue vorher',d.get('issue_before_forward')),block('Issue nachher: archivierte Rücklesung',d['readback_issue']),block('Soll-Inhalte: Begründung ihrer Auswahl separat prüfen',extras),block('Beziehungen vorher',d['relations_before']),block('Beziehungen nachher',d['relations_after'])]
  fields.insert(0,field('sollbasis','Ist die Soll-Inhaltsliste aus den Quellen sachgerecht und vollständig abgeleitet?','verwendete Soll-Liste', 'Die Soll-Liste ist eine eigene Operationalisierung. Prüfe ausdrücklich fehlende, ungültige oder zu stark formulierte Soll-Einheiten; nicht nur die späteren Erhaltungsurteile.',None,AR+'/soll-inhalte.json'))
  case(cid,'02',cid+' · Vorher / Nachher','Bewerte das gespeicherte Arbeitspaket einschließlich seiner expliziten Bezüge. Neue Ergänzungen dürfen nicht rückwirkend als Pflicht des alten PBI gewertet werden. GitHub ist eine abhängige Projektion desselben Inhalts, kein zweiter unabhängiger Qualitätserfolg.',blocks,fields,['core-S0','regeln-analyst'],src+register_evidence(aks,AR+'/evaluation'))
# Prozessprüfung: eigener Block, kein neuer Qualitätsfall.
for row in ratings['process_rows']:
 case('PROC-'+row['criterion'],'09',row['criterion']+' · Prozessbeleg','Prüfe die Reichweite des berichteten Belegs. Ein ausgeführter Tool-Aufruf beweist weder innere Nutzung noch selbständige Wahl gegenüber einer unbekannten Benutzeranweisung.',[],[field('process',row['criterion'],row['verdict'],row['reason'],ratings['allowed_verdicts']['H1-H6'],ratingsp)],['regeln-analyst'],[source(ratingsp)]+register_evidence([row],AR+'/evaluation'))
# Inhaltliche Fortschreibung: ausgewählte ursprüngliche Rohartefakte vollständig erreichbar.
asp=CR+'/evaluation/assessments.json';ad=read(asp)
for a in ad['cases']:
 cid=a['case_id'];dp=CR+'/evaluation/'+a['dossier'];d=read(dp);fs=[]
 for k,v in a['criteria'].items():
  for phase,cell in (v.items() if 'rating' not in v else [('gesamt',v)]):
   fs.append(field(k+'-'+phase,k+' · '+{'proposal':'Agentenvorschlag','recorded_result':'Gespeichertes Ergebnis','gesamt':'Gesamt'}.get(phase,phase),cell['rating'],cell['reason'],['getragen','teilweise getragen','abweichend','mehrdeutig','nicht beurteilbar','nicht anwendbar','nicht untersucht'],asp))
 bl=[block('Historischer Eingang',d.get('incoming')),block('Maschineller Vorschlag',d.get('proposal')),block('Vorheriger Bestand',d.get('before')),block('Gate-Entscheidung',d.get('ingest_decision')),block('Nach dem Ingest',d.get('after_ingest')),block('Spätere PBI-Vorschläge',d.get('pbi_operations')),block('PBI-Ausarbeitung',d.get('pbi_alignments')),block('PBI-Entscheidung',d.get('pbi_human_decisions')),block('Weiterverarbeitung: Bestand',d.get('after_pbi_before_forward')),block('Beziehungen',{'vorher':d.get('relations_before_ingest'),'nachher':d.get('relations_after_pbi')}),block('Bisherige Fallinterpretation',{k:a[k] for k in ['source_interpretation','agent_contribution','decision','observed_effect','limits']})]
 sr=[source(dp),source(asp)]
 for rel in a.get('additional_evidence',[]):
  p=CR+'/evaluation/'+rel
  if (R/p).exists():
   obj=read(p) if p.endswith('.json') else txt(p);sr.append(source(p));bl.append(block('Zusätzlicher Originalbeleg: '+rel,obj))
 # Originale des zugeordneten Laufabschnitts, ohne volatile aktuelle Core-Dateien.
 runbase=R/CR/'evaluation'/a['run_evidence']
 if runbase.exists():
  for p in sorted(runbase.rglob('*')):
   if p.is_file() and p.suffix in ['.json','.txt','.md'] and 'checkpoints' not in p.parts:
    sr.append(source(p))
 case('CORE-'+cid,'03',cid+' · '+a['title'],'Prüfe Eingang, damaligen Bestand, Vorschlag und tatsächliches Ergebnis getrennt. Historische Gate-Freigabe ist kein heutiges Qualitätsurteil. Fehlende Quellen und mehrdeutige Absichten bleiben offen.',bl,fs,['regeln-core'],sr,a['limits'])
# Historische Fallserie: aktuelle zusammenfassende Aussagen statt überholter F2-Redaktionsbehauptung.
livep=T+'/kapitel-7/tex-v2/chap7.5/chap7.5.tex';live=txt(livep)
for i,(title,body) in enumerate(re.findall(r'\\textbf\{([^}]+)\}([\s\S]*?)(?=\\textbf\{|\\subsection|\Z)',live)):
 if not any(x in title for x in ['(F1)','(F2)','(F3)','(F4)','(F5)']):continue
 shared=['CORE-K01','CORE-K02','CORE-K03','CORE-K04'];id_='FALL-'+str(i+1)
 src=[source(livep),source(E+'/z2-fallblaetter.md')]
 # Die konkreten Run-Belege dieser Fallserie werden hinzugefügt.
 runids={'(F1)':['20260818_084712_765eea'],'(F2)':['20260817_123342_f23f06'],'(F3)':['20260804_121433_eb181a'],'(F4)':['20260818_123055_a0b172','20260818_114850_eab853'],'(F5)':['20260818_084712_765eea','20260805_131538_899463','20260820_172043_802e63','20260820_173141_6e75fa','20260820_183855_b7747b','20260820_190220_895d18']}
 for marker,rr in runids.items():
  if marker not in title:continue
  for rid in rr:
   base=R/'runs/fullworkflow'/rid
   for part in ['00-github-inbound','07-ingest','07-decision','07-pbi-update','07-github']:
    for p in sorted((base/part).rglob('*.json')):
     if p.stat().st_size<2500000:src.append(source(p))
 case(id_,'09',title,'Prüfe die fachlichen Aussagen der aktuellen Fallbeschreibung anhand der unten zugänglichen Originale. Die detaillierten überlappenden Fälle werden zusätzlich in Modul 03 geprüft. Ein bestätigter Textvergleich ersetzt keine allgemeine Qualitätsaussage.',[block('Aktueller Thesistext (LaTeX, wortgetreu)',body)], [field('interpretation','Fachliche Reichweite der aktuellen Fallbeschreibung','so im Haupttext berichtet','Gegenstand ist der aktuelle Text; frühere Fallblätter sind historischer Kontext und enthalten ausdrücklich spätere Korrekturen.',None,livep)],['regeln-core'],src)
# Historische Modellreihe: ihren tatsächlichen Goldstand verwenden, keine stille Umstellung.
oldgp=T+'/kapitel-7/05-vergleichs-inspektion/03-gold-und-regeln/Interview-Einrichtung.w2-gold.json';oldg=read(oldgp);assert len(oldg['units'])==108
OG={x['id']:x for x in oldg['units']};context('gold-IE-v1','Historischer Referenzbestand v1 (108)',oldg,[source(oldgp)])
for key,run,arm,fn in [('MF','20260906_163431_f0d718','F','f-gpt5mini'),('ML','20260906_164332_90a2c4','Ledger','ledger-gpt5mini'),('MO','20260906_162750_aa7592','Ledger','ledger-gptoss'),('MM','20260906_161816_2c354c','Ledger','mini')]:
 add_run(key,run,'IE',arm)
 p=E+'/eval/w2-modellsensitivitaet-eval-'+fn+'.json';d=read(p)
 for old,row in d['goldUrteile'].items():
  gid=gkey('IE',old);assert gid in OG,gid
  covcase(key,gid,verdict(row),row.get('beleg',str(row)),p,'11',{'Historischer Nenner':108,'Keine stillschweigende Gold-v2-Neubewertung':True},OG[gid])
 # Sekundäre qualitative Sammelbehauptung ebenfalls prüfbar halten, ohne Einzelurteile zu erfinden.
 ba=d.get('blockA',d.get('ledger',{}).get('blockA',{}))
 case('MODEL-QUAL-'+key,'11',arm+' · '+run[-6:]+' · historischer Inhaltsbefund','Prüfe nur die überlieferte qualitative Sammelaussage. Es existiert hier kein neu erzeugtes vollständiges Claim-Einzelurteil. Eine Gegenprüfung darf die alte Modellreihe nicht unbemerkt mit Gold-v2-Ergebnissen vermischen.',[block('Historischer Inhaltsbefund',ba),block('Vollständige Claims',list(CLAIMS[key].values()))],[field('summary','Historischer Inhaltsbefund',ba.get('praezisionsBefund',''), 'Bisheriger Sammelbefund, keine unabhängig bestätigte Einzelannotation.',None,p)],['quelle-IE','gold-IE-v1','regeln-match'],[source(p),RUNS[key]['source']])
# Referenz-Vollständigkeit / technische Befunde sind getrennte Prüfumfänge.
cases.sort(key=lambda c:(c['module'],c['id']))
allids=[c['id'] for c in cases];assert len(allids)==len(set(allids))
for c in cases:
 keys=[x['key'] for x in c['fields']];assert len(keys)==len(set(keys)),(c['id'],keys)
 for x in c['contexts']:assert x in contexts,x
 for x in c['sources']:assert x in sources,x
summary={m:{'title':title,'cards':sum(c['module']==m for c in cases),'judgments':sum(len(c['fields']) for c in cases if c['module']==m)} for m,title in MODS.items()}
source_texts={s['sha256']:s.pop('text') for s in sources.values()}
payload={'version':'1.0','created':'2026-09-16','mode':'nicht verblindete Autoren-Gegenprüfung; keine unabhängige Zweitannotation','modules':summary,'cases':cases,'contexts':contexts,'sources':sources,'source_texts':source_texts,'notes':sorted(set(notes))}
payload['package_id']=sha(json.dumps(payload,ensure_ascii=False,sort_keys=True).encode())
(OUT/'pruefdaten.json').write_text(json.dumps(payload,ensure_ascii=False,indent=2)+'\n')
(OUT/'pruefdaten.js').write_text('window.REVIEW_DATA = '+json.dumps(payload,ensure_ascii=False).replace('</','<\\/')+';\n')
manifest={'package_id':payload['package_id'],'created':'2026-09-16','source_files':[{k:v for k,v in s.items() if k!='text'} for s in sources.values()],'modules':summary,'notes':payload['notes']}
(OUT/'quellenmanifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
(OUT/'antworten-leer.json').write_text(json.dumps({'package_id':payload['package_id'],'schema':'autorenpruefung-v1','reviewer':'','answers':{},'history':[]},ensure_ascii=False,indent=2)+'\n')
print(json.dumps({'modules':summary,'cards':len(cases),'judgments':sum(len(c['fields']) for c in cases),'sources':len(sources),'data_bytes':(OUT/'pruefdaten.json').stat().st_size,'notes':payload['notes']},ensure_ascii=False,indent=2))
