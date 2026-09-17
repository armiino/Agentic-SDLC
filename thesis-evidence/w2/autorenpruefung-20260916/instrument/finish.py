from pathlib import Path
import json, hashlib, collections
P=Path(__file__).resolve().parent/'paket';d=json.loads((P/'pruefdaten.json').read_text())
rows='\n'.join(f'| {v["title"]} | {v["cards"]} | {v["judgments"]} |' for v in d['modules'].values())
readme=f'''# Autorenprüfung der Evaluation – 16.09.2026

**Status: vorbereitet, noch nicht menschlich durchgeführt.** Dieses Paket überträgt vorhandene Bewertungen und Originalbelege in eine bearbeitbare Oberfläche. Es erzeugt keine neuen KI-Inhaltsurteile und schreibt keine Ergebnisse in die Thesis zurück.

## Start

1. [Prüfung im Browser öffnen](pruefung.html). Den gesamten Ordner zusammenhalten, weil die Prüfoberfläche ihre Daten und Gestaltung aus den danebenliegenden Dateien lädt. Bei einer reinen Quelltextvorschau die Datei in einem normalen Browser öffnen.
2. Deinen Namen eintragen. Modul, Prüfkarte und Kriterium auswählen. Zunächst stehen alle Antwortfelder auf **offen**.
3. Quelle, Ausgabe, Kriterium und KI-Begründung lesen. Für jedes Kriterium **bestätigen**, **korrigieren** oder **unklar** wählen. Wo kein eigenes historisches KI-Urteil vorliegt, steht stattdessen „Eigenes Urteil erfasst“ zur Verfügung.
4. „Urteil speichern“ oder „Speichern und weiter“ verwenden. Die Prüfung der erforderlichen Originalbelege wird für jedes Urteil ausdrücklich bestätigt. Es gibt keine Sammelfreigabe.
5. Nach jeder Sitzung **Antworten exportieren**. Die heruntergeladene JSON-Datei ist der abzugebende Antwortstand. Sie lässt sich später wieder importieren. Die lokale Browserablage dient nur als Zwischenspeicher; sie verändert keine Repo-Datei.
6. Die exportierte Antwortdatei hier wieder zur Auswertung vorlegen oder im Repo beim Paket sichern. Erst dann folgen Korrekturabgleich, Neuberechnung betroffener Kennzahlen und Textnachzug.

## Was du konkret beurteilst

Die ursprünglichen KI-Urteile sind sichtbar. Es handelt sich ausdrücklich um eine **nicht verblindete Autoren-Gegenprüfung** durch den Entwickler des Systems, nicht um eine unabhängige Zweitannotation. Eine Bestätigung bedeutet: „Ich habe die erforderlichen Originalbelege geprüft und trage dieses Urteil einschließlich der vorhandenen Begründung mit.“ Sie bedeutet nicht, dass das System im Fall erfolgreich war: Auch ein negatives oder nicht beurteilbares KI-Urteil kann zutreffend bestätigt werden.

Bei einer Bestätigung kann die bisherige Begründung übernommen werden. Bei einer anderen Begründung bitte auch diese eintragen. Bei Korrekturen, eigenen Urteilen und Unsicherheiten ist ein eigener Grund mit konkreter Belegstelle erforderlich. Ein abweichendes menschliches Urteil wird nicht allein aufgrund seines Ursprungs zur Wahrheit erklärt; bei der anschließenden Konsolidierung werden Regel, Originalbeleg und Begründung abgeglichen. Unklare Fälle bleiben erkennbar.

Die Suchausschnitte sind Hilfen, keine begrenzte Prüfbasis. Besonders bei „teilweise“ und „nicht abgedeckt“ den gesamten Claim-Bestand prüfen. Bei Quellenstützung die tatsächlich referenzierten Units von zusätzlichem Gesprächskontext unterscheiden. Die vollständigen nummerierten Quellen und vollständigen Ausgabebestände stehen unter der Karte zur Verfügung. Bei Analyst-Neuheit den vollständigen damaligen Core durchsuchen. Fehlende historische Belege werden nicht durch heutige Annahmen ergänzt.

## Umfang

| Modul | Prüfkarten | Antwortfelder |
|---|---:|---:|
{rows}
| Gesamt | {len(d['cases'])} | {sum(len(c['fields']) for c in d['cases'])} |

**Antwortfelder sind keine unabhängigen Versuchsfälle.** Die vielen Felder entstehen durch Kriterien, Quellenfälle, Läufe und Messpunkte; dieselben Texte werden unter verschiedenen Fragen betrachtet. Analyst und PBI bleiben 17 Vorschläge eines Laufs und drei abhängige Vorher-/Nachherpaare. Core-Fälle und Beispiele überlappen teilweise.

- **Module 01–09:** Gegenprüfung der im Haupttext ausgewerteten Inhalte und der zugehörigen Interpretationen. Technische Zahlen werden nicht pauschal in subjektive Qualitätsurteile verwandelt.
- **Modul 10:** Gesonderte Referenzprüfung. Die Referenz wurde bereits vom Autor konsolidiert. Die vorbereiteten 140 Vorwärts- und 172 Rückwärtsprüfungen sind ein zusätzliches Angebot zur vollständigen erneuten Prüfung; sie werden erst mit tatsächlicher Bearbeitung als neue Prüfung gezählt. Rückwärts heißt: Jede Quell-Unit darauf prüfen, ob relevante Inhalte im gesamten Goldbestand fehlen. Eine frühere Chat-Zusage wird nicht als neue Antwort vorausgefüllt.
- **Modul 11:** Abgetrennte Modellreihe im Anhang auf dem tatsächlichen alten Referenzstand mit 108 Aussagen. Dieser Originalstand ist beigelegt. Ein Wechsel auf Gold v2 wäre eine neue Auswertung und muss gesondert dokumentiert werden.

Du darfst in Etappen arbeiten. Nach einer Teilprüfung wird deren tatsächlicher Umfang berichtet. Eine abgeschlossene Analyst-Prüfung erlaubt keine Aussage „alle Inhaltsurteile in Kapitel 7 menschlich geprüft“. Eine vollständige erneute Referenzprüfung ist wiederum etwas anderes als die Prüfung der Ausgabebewertungen.

## Maßstäbe – unverändert lassen

- **Abdeckung:** vollständiger Sachverhalt einschließlich wesentlicher Bedingungen, Negation und Verbindlichkeit = full; wesentlicher Teil fehlt = partial; kein Match = none. Die historisch dokumentierte Negationsregel und späteren Auslegungen bleiben sichtbar. Bündelung mehrerer Claims ist erlaubt. Gespeicherte Zitate zählen in der normalen Claim-Abdeckung nicht mit.
- **Korrektheit:** Wird die Quelle verfälscht oder ihr widersprochen? **Stützung:** Tragen die angegebenen Quellstellen den gesamten Claim? Diese Fragen sind getrennt. Quellengetragene Inhalte außerhalb des Goldbestands sind nicht automatisch falsch.
- **Wichtige historische Abweichung:** Die alte Regeldatei enthält „strikte Gold-Precision“. In der Thesis wird stattdessen die tatsächlich angewandte inhaltliche Korrektheit berichtet. Bei dieser Gegenprüfung darf das eine nicht unbemerkt durch das andere ersetzt werden. Kapitel 7.2 mit der ausdrücklichen Abgrenzung ist beigefügt.
- **Analyst:** K1 Relevanz, K2 Tragfähigkeit der Bestandsanker, K3 Herleitung, K4 konkrete Bearbeitbarkeit. NOV Neuheit und CRIT Kritikerentscheidung separat. Eine sinnvolle Klärungsfrage muss noch keine fertige Lösung oder Zahlenschwelle enthalten.
- **PBI:** B1 Richtigkeit, B2 erforderlicher Inhalt und Erhaltung, B3 Verständlichkeit, B4 prüfbare Akzeptanzkriterien, B5 Konsistenz und Verbindungen, B6 Herkunft und Projektion. Alte und neue Soll-Inhalte getrennt betrachten; ihre Geltung ebenfalls prüfen. GitHub ist die Projektion des Core-Inhalts, kein unabhängiges zweites Qualitätsergebnis.
- **Fortschreibung:** K1 Herkunft/Inhalt, K2 Einordnung, K3 Erhaltung, K4 dokumentierte Freigabe/Wirkung, K5 Beziehungen/Weiterverarbeitung. Die K-Nummern dieses Moduls sind eine andere Rubrik als beim Analysten. Proposal und gespeicherter Zustand bleiben getrennt.

Die vollständigen ursprünglichen Protokolle liegen im Kontextbereich und als unveränderte Quellenkopien vor. Eine notwendige Regeländerung ist als neue Entscheidung mit Grund, Datum und konsistenter Anwendung auf betroffene Fälle zu dokumentieren. Nicht zugunsten eines gewünschten Ergebnisses im Einzelfall den Maßstab wechseln.

## Quellenstände und bewusste Grenzen dieses Pakets

1. Die Stressfall-Abdeckung folgt der Konsolidierung vom 08.09., nicht den überholten Werten der Dateien vom 07.09. Die Claim-Prüfung des Ledgers verwendet den Stand vom 15.09. Die beiden neuen Ledger-Ausgaben werden nicht erneut ausgeführt.
2. Die Stufenanalyse enthält die fünf späteren A-Korrekturen. Der B-Bestand wird im Hauptvergleich geprüft und nicht als weiterer unabhängiger Lauf gezählt.
3. Bei mehreren älteren Claim-Bewertungen existiert nur ein Sammelbefund. Die betreffenden Karten kennzeichnen das ausdrücklich. Es wurden keine fehlenden Einzelbegründungen erfunden.
4. Für zwei spät herabgestufte Claims (G-IE-062 und G-IE-107) enthält der ausgelesene Review-Einzelabschnitt kein gesondertes finales Urteil für „Claim plus gespeichertes Zitat“. Diese Felder bleiben ohne KI-Vorurteil; die aggregierte Zahl wurde nicht rückwärts in Einzelurteile übersetzt. Die Autorenprüfung kann sie nun ausdrücklich entscheiden.
5. Die historische Fallserie wird anhand ihrer aktuellen Thesisdarstellung geprüft. Alte Fallblätter sind nur Kontext, weil insbesondere die F1-Zählung und die F2-Redaktionsannahme später korrigiert wurden. Überlappende Fälle sind keine zusätzlichen Erfolgsbelege.
6. Hashes, Feldgleichheit, Tokenzählung, Testausgänge und die Auflösbarkeit von Referenzen beruhen auf technischen Prüfungen. Dieses Paket ersetzt deren technische Reproduktion nicht. Es macht keine neuen Qualitätsurteile über jede einzelne der 237 Core-Entitäten und prüft nicht sämtliche Codepfade.
7. Frühere formative Beobachtungen aus Kapitel 4 und sämtliche historischen KI-Reviews sind nicht jeweils erneut zu annotieren, um den aktuellen Ergebnisstand zu prüfen. Gegenstand ist der ausgewiesene Evaluationsbestand einschließlich der getrennten Modellreihe, nicht jede jemals erzeugte KI-Aussage im Projekt.

## Danach zulässige Beschreibung

Erst nach der tatsächlichen Durchführung, mit eingesetztem Umfang und Datum:

> Die Bewertungen wurden zunächst KI-gestützt erstellt und anschließend durch den Autor anhand der Originalbelege geprüft. Die nicht verblindete Gegenprüfung umfasste [tatsächlicher Umfang]. Bestätigungen, Korrekturen und nicht entscheidbare Fälle wurden dokumentiert. Der Autor war zugleich Entwickler des Systems; eine unabhängige menschliche Zweitannotation fand nicht statt.

„Vollständig“ darf nur für einen tatsächlich vollständig bearbeiteten, genau bezeichneten Umfang verwendet werden. Offene oder unklare Ergebnisse werden nicht als positive Erfüllung gezählt. Bestätigungs-/Korrekturzahlen beschreiben die Autorenprüfung, keine unabhängige Interrater-Reliabilität und keine allgemeine Validierung des KI-Bewerters. Kleine und entwicklungsbekannte Datenbasis bleiben Grenzen.

## Dateien und Nachrechnung

- `pruefung.html`: Einstieg in die lokale Prüfoberfläche.
- `pruefdaten.json`: vollständiger, unveränderter Datenstand des Prüfungspakets.
- `antworten-leer.json`: leere Antwortvorlage; niemals als erledigte Prüfung ausgeben.
- `quellenmanifest.json`: Pfade, Größen und SHA-256 der {len(d['sources'])} beigelegten Originaldateien.
- `belege/`: unveränderte Originalkopien. Die Oberfläche zeigt ihren ursprünglichen Pfad und Inhalt.
- `auswerten.py`: prüft einen späteren Antwortexport technisch und berichtet den tatsächlichen Umfang. Es fällt keine Inhaltsurteile und überschreibt keine historischen Ergebnisse.
- `paketpruefung.json`: technische Abnahme dieses Pakets; keine semantische Autorenprüfung.

Paketkennung: `{d['package_id']}`. Ein Antwortimport aus einem anderen Paket wird abgewiesen. Exportdateien enthalten die ursprüngliche KI-Wertung, deine Bewertung, Begründung, Belegverweise, Prüferangabe, Zeitpunkt und Änderungsverlauf. Bitte auch nach einer vollständigen Prüfung die Originale und frühere Antwortstände erhalten.
'''
(P/'README.md').write_text(readme)
assert len({c['id'] for c in d['cases']})==len(d['cases'])
assert sum(c['id'].startswith('COV-') for c in d['cases'])==1039
for mod,n in [('01',17),('02',3),('03',12),('04',280),('05',218),('06',309),('07',109)]:assert d['modules'][mod]['cards']==n
cov=collections.defaultdict(collections.Counter)
for c in d['cases']:
 if c['id'].startswith('COV-'):
  k=c['id'].split('-')[1];cov[k][c['fields'][0]['ai_value']]+=1
assert dict(cov['LR'])=={'full':54,'partial':29,'none':26},cov['LR']
assert dict(cov['FI'])=={'full':69,'partial':23,'none':17},cov['FI']
assert dict(cov['ST'])=={'full':56,'partial':28,'none':25},cov['ST']
assert dict(cov['LM'])=={'full':26,'partial':2,'none':3},cov['LM']
assert dict(cov['FM'])=={'full':27,'partial':2,'none':2},cov['FM']
for sid,s in d['sources'].items():
 b=(P/s['file']).read_bytes();assert len(b)==s['bytes'] and hashlib.sha256(b).hexdigest()==s['sha256'],s['path']
 forigin=Path('/Users/armino/devProjects/Agentic-SDLC')/s['path'];assert forigin.read_bytes()==b,s['path']
 assert hashlib.sha256(d['source_texts'][s['sha256']].encode()).hexdigest()==s['sha256'] or b.startswith(b'\xef\xbb\xbf'),s['path']
for c in d['cases']:
 for f in c['fields']:
  assert 'author_review' not in f and 'author_value' not in f
empty=json.loads((P/'antworten-leer.json').read_text());assert empty['answers']=={}
report={'status':'technical preparation verified; no human judgments collected','cases':len(d['cases']),'answer_fields':sum(len(c['fields']) for c in d['cases']),'source_files':len(d['sources']),'sha256_and_original_identity':'all passed','human_answers':0,'coverage_counts':{k:dict(v) for k,v in cov.items()},'module_counts':d['modules'],'scope_notes':'Cards and criteria are dependent review units, not sample sizes. This test does not confirm semantic correctness.'}
(P/'paketpruefung.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(report,ensure_ascii=False,indent=2))
