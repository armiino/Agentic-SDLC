# P10: HITL-Fall und Abgleich des getesteten Kettenstands

Stand: 15.09.2026. Abschluss der beiden Prüfaufträge aus dem [Phase-10-Plan](./Phase-10-Gesamtabnahme-2026-09-13.md). Retrospektive Bestandsanalyse und zwei kleine Textergänzungen; kein neuer Modelllauf, keine Änderung des Produktionscodes oder historischer Ergebnisse.

## Ergebnis und Entscheidung

**Der historische Adjudikationsfall ist für eine begrenzte inhaltliche Nachbewertung geeignet.** Maschineller Vorzustand, ausgefüllte Entscheidungsvorlage und Endbestand sind vorhanden. Der Zustand nach Anwendung der Entscheidungen, aber vor Facettenvervollständigung, lässt sich aus den gespeicherten Ergebnisobjekten ableiten. Er ist ausdrücklich eine Rekonstruktion, keine zusätzlich erhaltene Originaldatei. Die Auswertung kann menschliche Eingriffe und nachgelagerte Facettenänderungen getrennt betrachten.

**Ein neuer vollständiger E2E ist aufgrund dieser Gegenprüfung nicht die nächste Pflichtarbeit.** Der aktuelle Bestand stimmt in allen 685 erfassten Dateien mit dem gesicherten Ausgangsbestand der technischen Übernahme vom 11.09. zuzüglich der damals übernommenen Änderungen überein. Darunter liegen sämtliche derzeit inventarisierten 530 Host-, drei UI- und 138 Testdateien der Typen `.cs`, `.csproj`, `.txt` und `.json`, ohne generierte `bin`-/`obj`-Dateien. Weitere erfasste Dateien betreffen unter anderem Eingaben, Konfiguration und Core. Die Testprotokolle vom 11.09. sind daher weiterhin diesem Code zuordenbar. Das ist eine Dateizuordnung zu einem datierten Nachweis, kein neu ausgeführter Test und kein Nachweis beliebiger Eingaben.

**Nächster Schritt:** Den vorhandenen HITL-Fall nach dem unten festgelegten Umfang inhaltlich bewerten. Er ergänzt die bisherige Trennung zwischen maschineller Erzeugung, menschlicher Bearbeitung und anschließender Modellarbeit. Die bestehende Ledger/F-Messung bleibt unverändert. Ein neuer Integrationslauf bleibt eine Option für eine konkret beanspruchte, durch die vorhandenen Nachweise nicht gedeckte Eigenschaft; eine neue allgemeine Messkampagne ist nicht beschlossen.

## 1. Vorhandene Belegkette des HITL-Falls

Lauf: `runsArchive/ledger/20260704_131614_018865/`. Das Extraktionsmodell ist laut `config.json` GPT-5.4. Die Entwicklungsgeschichte mit kleineren Modellen bleibt davon getrennt; dieser Fall belegt keine besondere Wirksamkeit bei kleinen Modellen.

| Zustand / Schritt | Vorhandener Beleg | Was daran prüfbar ist |
|---|---|---|
| Quelle | `input/transcripts/Interview-Einrichtung.txt`, eingebettete 136 Units, Git-Stand `1579f8f…` | Die aktuelle Transkriptdatei ist bytegleich mit der im damaligen Commit. Alle 136 Unit-Texte finden sich bei normalisierten Leerzeichen im Transkript. Damit ist der Quellenanschluss gut abgesichert; ein im damaligen Lauf erfasster vollständiger Quellhash liegt hier nicht vor. |
| Maschineller Vorzustand | `step-03-facet-validation/output.json` | 42 Claims: 22 vom Modell als `grounded` eingeordnet, 19 `partial`, einer `overstated`. Dies sind Modellurteile, keine Goldannotation. |
| Vorlage und Entscheidungen | `step-03b-adjudicated/queue.json`, `adjudicated-ledger.json` | 52 übereinstimmend zuordenbare Entscheidungen: 20 zu vorhandenen Claims und 32 zu Unit-Hinweisen. Autor-Adjudikation ist in der Entwicklungsnotiz beschrieben; das gespeicherte Label `author` allein authentifiziert keine Person. |
| Nach Anwendung, vor Refine | Ergebnisobjekte in `adjudicated-ledger.json` | 22 übernommene Claims und die resultierenden Objekte der Entscheidungen ergeben 60 Claims. Bei den neun Anhängeaktionen werden die angereicherten Zielobjekte verwendet. 18 Claims tragen noch vorläufige Facetten. |
| Nach Facettenvervollständigung | `consumable.json` | 60 Claims, keine ausstehenden Facetten. Die 42 bestehenden Claims entsprechen dem abgeleiteten Zustand nach Anwendung vollständig. Bei den 18 neuen bleiben Kennung, Aussage, Evidenz und Unit-Verweise identisch; die Facetten und zugehörigen Notizen ändern sich. |

Die ursprünglichen Workflow-Ereignisse enden vor der späteren Adjudikation. Für den Refine-Schritt nennt die Entwicklungsnotiz GPT-5.4; im Fallordner wurde kein eigener gleichwertiger Aufruftrace dieses späteren Schritts gefunden. Der gespeicherte Zustandsvergleich trägt die Änderungsbilanz. Er ersetzt keinen vollständigen Ausführungstrace der gesamten damaligen Bedienung.

### Tatsächliche Anwendung der 52 Entscheidungen

| Entscheidung | Anzahl | Beobachtbare Wirkung |
|---|---:|---|
| Facetten korrigieren (`apply_repair`) | 10 | Änderungen an Modalität, Zeitbezug oder Status vorhandener Claims. Die Aussagen selbst bleiben erhalten. |
| Vorhandenen Claim übernehmen (`accept_gap` in diesen Fällen) | 10 | Übernahme ohne die vorgeschlagene Facettenkorrektur. Bei einem dieser Claims kommt durch eine separate Entscheidung noch Evidenz hinzu. |
| Belege anhängen (`attach_evidence`) | 9 | Neun unterschiedliche bestehende Claims erhalten zusätzliche Evidenz, Unit-Verweise und eine Notiz. Zwei dieser Claims wurden außerdem zuvor an Facetten geändert. |
| Als bereits erfasst markieren (`mark_covered_by`) | 1 | Zuordnung im Entscheidungsprotokoll; kein zusätzlicher Claim. |
| Hinweis verwerfen (`reject`) | 4 | Kein zusätzlicher Claim. Es werden hier keine vier vorhandenen Claims gelöscht. |
| Eigenen Claim aufnehmen (`promote_to_claim`) | 18 | 18 neue Claims mit Quellenanschluss und zunächst vorläufigen Facetten. |

Von den ursprünglichen 42 Claims sind insgesamt 17 verändert und 25 im Endbestand exakt unverändert. Die Zahlen zehn Korrekturen und neun Anreicherungen dürfen wegen der zwei Überschneidungen nicht zu 19 verschiedenen geänderten Claims addiert werden. Alle referenzierten Unit-Kennungen der verglichenen Claim-Zustände sind im erhaltenen Unit-Bestand auflösbar. Das ist eine strukturelle Herkunftsaussage; die Zitate und Beziehungen sind damit noch nicht semantisch bewertet.

Ein konkreter positiver Mechanismus ist schon sichtbar: Bei der Anforderungsanalyse schlägt das System unter anderem die Abschwächung von `decided` zu `open` vor. Die gespeicherte Entscheidung behält `decided` bei und ändert stattdessen Modalität und Zeitbezug. Die Bearbeitung übernimmt somit nicht einfach den gesamten Modellvorschlag. Ob diese Auswahl fachlich zutrifft, ist Gegenstand der anschließenden Nachbewertung.

### Was daraus noch nicht folgt

Die historische Datei `recall-fast.consumable.json` meldet 96 von 99 über Segmentüberlappung und bezeichnet das Maß selbst als Obergrenze. Die unveränderte historische Referenz ist keine nachträglich unabhängig geprüfte Goldreferenz. **96/99 wird deshalb nicht als fachlicher Qualitätsgewinn in Kapitel 7 übernommen.** Auch 18 zusätzliche Claims und ein bestandenes Gate sind keine 18 nachgewiesenen Verbesserungen.

Einige neue Aussagen beschreiben ihre Funktion gegenüber bestehenden Claims, etwa „Ergänzt Darstellungsdetails …“, statt alle Quelldetails selbst auszuformulieren. Die zugehörigen Quellzitate sind vorhanden. Bei der Inhaltsprüfung ist deshalb auseinanderzuhalten, ob nur ein Quellenzugang ergänzt wurde oder ein bislang fehlender fachlicher Inhalt tatsächlich als Aussage erschlossen ist. Diese Sichtung ist ein Grund für die Bewertung, noch keine vollständige Fehlerzählung.

Die 22 zunächst freigegebenen Claims gingen nicht als Einzelprüffälle in die damalige Queue ein. Der Fall trägt folglich eine **selektive menschliche Adjudikation**, keine dokumentierte menschliche Vollprüfung aller Inhalte. Das fachliche Urteil des Entwicklers wird durch die geplante KI-gestützte Nachbewertung nicht unabhängig.

## 2. Festgelegter Umfang der anschließenden Inhaltsprüfung

Dies ist ein am 15.09. festgelegtes Protokoll für eine nachträgliche Fallauswertung. Der Fall und einzelne Ergebnisse sind bereits bekannt; es handelt sich nicht um Präregistrierung oder eine blind ausgewählte Stichprobe.

**Frage:** Welche fachlichen Änderungen entstehen durch die Bearbeitung der vorgelegten Hinweise, und welche Grenzen verbleiben nach der nachgelagerten Facettenzuweisung?

1. Alle 52 Entscheidungen bewerten, einschließlich Übernahmen, Verwerfungen und der Markierung als bereits erfasst. Keine Auswahl nur erfolgreicher Korrekturen. Die 42 bestehenden und 18 neuen Claims werden anhand ihrer Quellen im Vor-, Zwischen- und Endzustand betrachtet; identische Texte benötigen keine doppelte Bewertung. Die 22 außerhalb der Queue mitgeführten Claims bleiben als Kontrollbestand im Vergleich.
2. Getrennt erfassen: inhaltliche Stützung durch die Quelle; Erhalt konkreter Bedingungen und Einschränkungen; zutreffender Status, Modalität und Zeitbezug; fachlich passender Beleganschluss; Nutzen eines neuen Claims gegenüber dem bereits vorhandenen Bestand. Facettenzuweisung wird nach der historischen Taxonomie bewertet, nicht nach späteren Regeländerungen.
3. Pro Beurteilung Quelle, vorherige Aussage, Entscheidung, Ergebnis, Begründung und Unsicherheit festhalten. Ergebnisgruppen: fachlich verbessert, fachlich unverändert, verschlechtert/neu problematisch, nicht entscheidbar. Zusätzliche Quellenanreicherung separat zählen. Bei neuen Claims zunächst prüfen, ob ihr Inhalt vorher fehlte oder bereits vorhanden war; reine Mehrfacherfassung ist kein automatischer Gewinn.
4. Menschliche Auswahl/Überarbeitung und nachgelagerte modellbasierte Facettenänderung getrennt bilanzieren. Die Aufteilung beschreibt beobachtete Zustandsänderungen; sie ist kein kontrolliertes Kausalexperiment zur isolierten Wirkung von HITL.
5. Fallzahlen mit den jeweiligen Bezugsgrößen berichten; keine Erfolgsquote über alle Transkriptanforderungen, keine neue globale Recall-Aussage ohne entsprechende vollständige Referenz. Vorhandene Fehler und ungeklärte Urteile ebenso sichtbar machen wie Verbesserungen. Entwickler-/KI-Beteiligung und fehlende unabhängige Nachprüfung ausweisen.

**Aufwandsgrenze:** Ein vorhandener Fall, keine neuen Modellaufrufe, keine neue Goldannotation des gesamten Transkripts, keine Zeit-/Usabilitystudie. Eine vollständige, begründete Inhaltsbewertung der genannten Objekte ist noch zu leisten; die automatisch erstellten Vergleichsdateien enthalten bewusst `semanticRating: not_performed`. Ergibt die Bewertung keinen klaren fachlichen Mehrwert, bleibt der Befund eine differenzierte Mechanismusprüfung. Historische Originale werden nicht korrigiert.

## 3. Aktueller Kettenstand und vorhandene Absicherung

Die reine Liste der zehn nach HEAD veränderten Host-Dateien reicht als Standvergleich nicht. Zwischen dem letzten Commit vor dem Lauf vom 18.08. und dem HEAD vom 23.08. liegen bereits weitere Änderungen, darunter Steward-, Gate- und Forward-Funktionen. Der Datumsanker `896f0d1…` ist **nicht** als exakte Laufversion verifiziert: Der damalige Arbeitsbaum kann uncommittete Änderungen enthalten haben. Daher wird der E2E nicht nachträglich diesem Commit zugeschrieben.

Die tragfähige Zuordnung erfolgt über den gesicherten Testbestand vom 11.09.: Ursprüngliches Manifest plus fünf dokumentierte Übernahmen ergeben 685 Hashvergleiche ohne Abweichung. Die erneute Inventur findet in den drei erfassten Projekten keine zusätzlichen Quelldateien der oben genannten Typen außerhalb dieses Manifests. Die originale TRX-Datei weist 701 ausgeführte und bestandene Tests aus. Sie enthält die fünf Refine-Fälle, fünf Baseline-Ausgabefälle, drei Segmentierungsfälle und vier Message-/Metrikfälle ausdrücklich als bestanden. Die Tests wurden heute nicht wiederholt.

| Änderung / Aussage | Bereits vorhandener Nachweis | Grenze / Konsequenz |
|---|---|---|
| Adjudikations-Refine ist vor der Baseline-Ableitung eingebunden. | Aktuelle Graphverdrahtung; fünf Tests des gemeinsamen Refine-Kerns; Lauf `f58d6a` vom 09.09.: 1 ausstehender Claim → 1 vervollständigt → 0 ausstehend, danach vier Baseline-Artefakte und Ingest-Gate. | Der Lauf endet an der Ingest-Pause. Er belegt keine ausgeführte Ingest-Mutation und keinen ganzen aktuellen Transkript–GitHub-Durchlauf. |
| Fehlende Ledger-Eingabe oder Restprobleme der Baseline werden als terminale Ergebnisse ausgegeben. | R-75-Änderung; fünf bestandene Ausgabefälle im echten MAF-Testgraphen einschließlich `HumanReview`, Iterationsgrenze und Weitergabe bei Pass. | Rezeptaufrufe werden in den gezielten Tests ersetzt. Der Standardpfad verwendet weiterhin `RecipeRunner.ExecuteAsync`; kein neuer Beweis semantischer Modellqualität. |
| Sprecher mit Bindestrich werden korrekt segmentiert. | Drei Regressionstests und dokumentierter Korpusvergleich: F1 ändert sich, die zwei W2-Quellen und drei weitere Eingaben bleiben in der Segmentausgabe identisch. | Historische F1-Unit-IDs bleiben historische IDs; kein rückwirkender Austausch ihrer Belege. |
| Interne Metrik heißt `internalQaRecall`, nicht offizielle Gold-Coverage. | Nachrichten-/JSON-Tests und aktueller Writer. | Eine Schema-/Benennungsänderung; keine zusätzliche fachliche Leistungsfähigkeit. Die aktuelle Differenz an `MetricsFinalizer` ist keine neue Tokenberechnungsänderung. |
| Neuere Gate-, Steward- und Forward-Funktionen werden technisch geprüft. | Hashgleicher Testbestand; einschlägige Testfamilien, u. a. Forward-UI/Adapter, Ingest-Entscheidungsvertrag, Steward-Gates und Wiederaufnahme. Smoke vom 11.09. mit 14 bestandenen Kontrollen. | Adapter- und Funktionsprüfungen ersetzen keine allgemeine Bedienungsstudie. Der Smoke verwendet kontrollierte Eingaben und leere Gates; er umfasst einen Delta-Einstieg mit Pause/Wiederaufnahme, keinen vollständigen Transkriptpfad. |
| Gesamtkette kann mit echten Entscheidungen bis zur Projektion durchlaufen. | Historischer Lauf `765eea`; zusätzlich spätere vollständige Delta-Läufe, siehe bestehende E2E-Inventur. | Historischer Funktionsnachweis. Kein vollständiger E2E nach sämtlichen aktuellen Änderungen. |
| Arm F und Ledger-Capture sind zusätzliche Evaluationszugänge. | Eigene Befehle und passende Testfamilien im Testbestand. Die Produktionsverdrahtung wurde separat gelesen. | Ihre Unversioniertheit ersetzt keinen Fehlernachweis an der Produktionskette. Für die Abgabe trotzdem im endgültigen Artefaktmanifest erfassen. |

**Abnahmeentscheidung:** Es ist kein neuer zentraler Funktionsfehler bestätigt, der jetzt einen breiten E2E oder einen Codeumbau erzwingt. Vorhandene historische Integration, späterer Frontabschnitt und aktuelle datierte Tests bilden unterschiedliche, ergänzende Nachweise. In der Arbeit bleiben ihre Zeitstände und Reichweiten getrennt. Eine zusammengesetzte Beleglage darf nicht als ein einziger aktueller E2E ausgegeben werden.

Für B-05 bleibt die Festlegung und Bereitstellung des tatsächlichen Abgabestands offen. Bei unverändertem Code ist die vorliegende Hashzuordnung nutzbar. Spätere Änderungen verlangen einen erneuten gezielten Abgleich. Die separat nicht gebaute MCP-Serverkomponente und die Betriebsumgebung werden nicht durch die hier gezählten Host-/UI-/Testdateien vollständig abgenommen.

## 4. Kleine Textkorrektur zur menschlichen Bedienung

Die vorhandene Governance-Erklärung in 5.6 sowie die Trennung in 6.4 bleiben erhalten. Neu erläutert 6.4 knapp, dass ein Review-Aufruf einen lokalen Webserver für die Browseroberfläche startet, welche Arten von Informationen und Optionen die Oberfläche bereitstellt und wie gespeicherte Entscheidungen in die Wiederaufnahme eingehen. 6.6 grenzt die unterstützten Chat-Gates ab, nennt den alternativen UI-Aufruf und erklärt, dass die Dialogsteuerung die konkrete Werkzeugeinreichung durch den Menschen bestätigen lässt.

Damit werden Agentenaufgabe, menschliches Urteil und feste Ausführung unterscheidbar: Der Steward vermittelt und wählt Werkzeuge; die fachliche Entscheidung kommt vom Menschen; Einreichung und Anwendung folgen dem jeweiligen kontrollierten Verfahren. Werkzeugzustimmung, fachliche Freigabe und Strukturprüfung bleiben getrennt. Keine zusätzlichen API-, Port- oder Installationsdetails, keine neue Grafik.

Overleaf-Sync:

- `chap6/chap6.4/chap6.4.tex` → `Kapitel/06/06.4.tex`
- `chap6/chap6.6/chap6.6.tex` → `Kapitel/06/06.6.tex`

Die vier kleinen früheren Nachsync-Dateien aus der [PDF-Abnahme](./P10-4c-PDF-Abnahme-2026-09-15.md) bleiben separat zu prüfen. Kein zusätzlicher Sync von Bibliografie, Hauptdatei oder Draw.io nötig. PDF-Abnahme der beiden neuen Ergänzungen nach dem Sync.

## Reproduzierbarkeit und Prüfumfang

Das [Begleitverzeichnis](./P10-HITL-Kettenabgleich-2026-09-15/) enthält das Auswertungsskript, alle 60 Claim-Vergleiche, alle 52 Entscheidungseinträge, die strukturelle Bilanz, ausgewählte Originalereignisse, Testzuordnung und Prüfsummen. Die Einträge sind Belegaufbereitung, noch keine semantischen Bewertungen. `tested-files-20260911.json` ist eine abgeleitete Prüfsummenliste aus dem erhaltenen Originalmanifest und dem dokumentierten Übernahmeprotokoll; keine nachträglich erfundene Testausführung.

Das Skript liest Quellen und schreibt ausschließlich in das angegebene Ausgabeverzeichnis. Es verifiziert am Ende, dass die 707 gelesenen Quelldateien unverändert sind. Ein Wiederholungslauf wurde auf identische JSON-Ergebnisse geprüft. Die TeX-Ergänzungen wurden gegen den lokalen Code, ihre Querverweise und die umgebenden Abschnitte geprüft; kein Overleaf-Build wurde behauptet. Keine neue Literaturbehauptung, keine neue B-ID: Die Stand- und Herkunftsgrenzen bleiben in den bestehenden Abschlussaufgaben.
