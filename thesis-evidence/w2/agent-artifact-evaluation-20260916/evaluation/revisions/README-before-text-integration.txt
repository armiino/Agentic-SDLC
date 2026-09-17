# Zusatzbewertung: Agentenbeitrag und Qualität fortgeschriebener Arbeitspakete

16.09.2026 · **KI-Erstbewertung abgeschlossen, drei punktuelle Autorenrückmeldungen sowie die unten getrennt dokumentierten Folgeantworten erfasst; keine vollständige Autorenprüfung oder unabhängige Annotation.** Die Tabellen sind vorläufige Auswertungsergebnisse dieses Materials und noch kein eingearbeitetes Thesis-Ergebnis. [Festgelegtes Protokoll](../protokoll-v1.md), [Einzelurteile](./ratings-first-pass.json), [Gegenprüfung](./autorenpruefung.md).

## 1. Erkenntnisgewinn und Reichweite

Der historische Fall zeigt eine zusammenhängende agentische Planungsarbeit: vorhandenen Projektbestand durchsuchen, Ergänzungen herleiten, durch einen Kritiker filtern, ausgewählte Vorschläge einreichen, an Gates freigeben und in PBIs/Issues weiterführen. Die Zusatzbewertung untersucht die fachliche Tragfähigkeit dieser Ausgaben. Sie ergänzt die bereits vorhandenen Kontroll-, Herkunfts- und Integrationsnachweise; sie ersetzt diese nicht.

Das Ergebnis ist differenziert: Die Vorschläge sind im untersuchten Bestand überwiegend nachvollziehbare Planungsbeiträge. Einige Herleitungen sind ungenau, und ein Teil der angeblich neuen Inhalte ist bereits vorhanden. Der Kritiker entfernt auch Vorschläge mit nachvollziehbarem Klärungsbedarf. Die drei PBI-Updates übernehmen alle zwölf vorab bezeichneten neuen Teilinhalte. Die ausreichende Ausarbeitung des gesamten Arbeitspakets einschließlich alten Umfangs folgt daraus nicht. Der Besuchsfall erhält seine alten Inhalte; bei Rechte-/Übergabearbeitspaket bleiben spezifische Grenzen.

Bewertet wird das konfigurierte System einschließlich vorhandener Daten und historischer Freigaben. Es gibt keinen Komponentenvergleich, keine unabhängige Stichprobe und keine Wirkungszuschreibung allein an MAF, Agenten oder Modell. Die 17 Vorschläge stammen aus einem Lauf mit fünf Perspektiven; drei PBI-/Issue-Paare sind drei abhängige Folgeartefakte dieses Falls. Keine praktische Zeitersparnis und keine allgemeine Zuverlässigkeit daraus ableiten.

### Fachliche Qualität und Übertragung nach GitHub

Die fachliche Bewertung bezieht sich auf die im Core gespeicherten Arbeitspakete mit ihrem verknüpften Kontext. Der Analyst schlägt zusätzliche Inhalte vor; Einordnung und PBI-Fortschreibung bestimmen anschließend, wie diese in den Projektbestand eingehen. Die gespeicherten menschlichen Entscheidungen autorisieren ihre Anwendung. Die ausgewählten GitHub-Updates stellen diesen Stand durch eine deterministische Projektion dar. Die Projektion übernimmt auch Texte verknüpfter Architekturvorgaben; sie ist daher eine aus dem Core zusammengesetzte Sicht und keine eigenständige zweite fachliche Ausarbeitung.

Der ergänzende [Feldabgleich](./projection-chain.json) verfolgt für alle drei Paare **Core → gespeicherter Synchronisationseintrag → Forward-Plan → archivierte Issue-Rücklesung**. Geprüft wurden Titel, Ziel, Akzeptanzkriterien, die über `covers` bestimmten Anforderungskennungen und die über `constrained_by` eingebundenen Architekturtexte; in diesen drei Fällen liegt keine zusätzliche über `covers` eingebundene Architekturarbeit vor. Alle geprüften Inhalte stimmen an den jeweiligen Übergängen überein. Zwischen Forward-Plan und späterer Rücklesung stimmen außerdem jeweils der vollständige Titel und Body überein. Der Abgleich präzisiert die vorhandene Projektionsprüfung anhand derselben historischen Daten; er ist kein neuer Systemlauf.

Damit beantworten die beiden Prüfungen verschiedene Fragen: **Ist das fortgeschriebene Core-Arbeitspaket fachlich ausreichend ausgearbeitet? Wird es mit dem vorgesehenen Kontext korrekt nach GitHub übertragen?** Die Grenzen bei Rechte- und Übergabetext bestehen bereits im übernommenen PBI-Vorschlag und im gespeicherten Core. Sie entstehen in diesen Fällen nicht erst beim GitHub-Rendering. Aus ihrer Freigabe lässt sich weder eine eigenständige menschliche Textkorrektur noch eine vollständige fachliche Prüfung ableiten.

Ein PBI und sein projiziertes Issue bleiben ein abhängiges Paar. Die drei Übereinstimmungen sind weder drei zusätzliche Qualitätsfälle noch ein Nachweis allgemeiner Agentenzuverlässigkeit. Labels, Kommentare, Dokumentexporte und die fachliche Bedeutung der Readiness-Anzeige werden durch diesen Feldabgleich nicht beurteilt. Die Projektion kann Metadaten ableiten, weshalb nicht jedes gleichnamige PBI-Feld unverändert kopiert werden muss: Im gespeicherten Synchronisationseintrag ist `readiness` jeweils `ready`; abweichende ältere PBI-Unterfelder werden im Prüfbericht separat gezeigt und nicht still als Inhaltsfehler gezählt.

## 2. Vorgehen und Urteilsrollen

Alle 17 Vorschläge einschließlich aller vier Kritiker-Aussortierungen sowie alle drei aktualisierten PBI-/Issue-Paare wurden nach Rubrik v1.0 geprüft. Historischer Ausgangsbestand, Vorlagen, Antworten und Zustände stammen aus dem unveränderten Originalpaket. Die frühere Vorbereitung sichert den zum Analyst-Bericht passenden Fingerabdruck. Suchmuster und Fundstellen stehen in [search-results.json](./search-results.json); die Neuheitsurteile beziehen sich auf den zugeordneten Bestand und die dokumentierte Suche, nicht auf alle denkbaren Projektinformationen.

Die KI-Ersturteile zu K1–K4 und Neuheit wurden vor der jetzigen Auswertung des Kritikerurteils separat gespeichert. Eine Verblindung wird nicht behauptet: historische Filter und Teile der Ausgaben waren zuvor zugänglich. Drei interne Konsistenzkorrekturen sind in `review_revisions` festgehalten; die Rubrik wurde dabei nicht geändert. Auch die früheren Erstfassungen sind erhalten. Diese Selbstkorrektur ist keine unabhängige Kontrolle.

Der Autor gab Rückmeldung zur Performance-Vorlage, zur Rechtefrage und zur Ausarbeitung des Übergabe-PBI. Seine Aussagen sind wörtlich und mit begrenztem Bezug in [author-feedback.json](./author-feedback.json) dokumentiert. Sie bestätigen sinnvolle Themen beziehungsweise Ergänzungsbedarf und zugleich nachvollziehbare Gegenargumente. Sie sind weder eine pauschale Annahme aller Ersturteile noch eine vollständige Neubewertung dieser Fälle. Es liegen **null vollständig neu annotierte Fälle durch den Autor** vor. Der Autor ist zugleich Entwickler; eine unabhängige Zweitannotation fehlt.

E = erfüllt, T = teilweise, N = nicht erfüllt, NB = nicht beurteilbar, NA = nicht anwendbar. Es gibt keinen Gesamtqualitätsscore. Eine neue Planungsfrage muss bearbeitbar sein; sie muss ihre noch zu erforschende Antwort nicht bereits liefern. Ebenso ist „prüfbar“ ein Urteil über einen Akzeptanzpunkt, kein durchgeführter Produkttest.

## 3. Analyst: Begründung, Neuheit und Filterung

| Kriterium | E | T | N | NB | NA | Bezugsgröße |
|---|---:|---:|---:|---:|---:|---|
| K1 Projektrelevanz | 17 | 0 | 0 | 0 | 0 | 17 Vorschläge desselben Laufs |
| K2 Bestandsanker | 15 | 2 | 0 | 0 | 0 | 17 Vorschläge desselben Laufs |
| K3 Herleitung | 16 | 1 | 0 | 0 | 0 | 17 Vorschläge desselben Laufs |
| K4 Bearbeitbarkeit als Planungsauftrag | 17 | 0 | 0 | 0 | 0 | 17 Vorschläge desselben Laufs |

**Einordnung der vielen E-Urteile:** Das Raster prüft begründete Planungsbeiträge, keine fertig implementierbaren und vollständig spezifizierten Anforderungen. Die ausgewählte Sitzung sollte gerade solche bestandsbezogenen Beiträge erzeugen. Hohe Relevanz-/Bearbeitbarkeitswerte sind daher keine generelle Agenten-Erfolgsquote und kein Nachweis der Güte auf unbekannten Aufgaben. „E“ für K4 der Performance-Vorlage bedeutet: Ziele festzulegen ist ein konkreter Auftrag; „ohne wahrnehmbare Verzögerung“ ist noch kein messbarer Leistungswert.

**Konkrete Gegenbefunde:** AN-01 nennt ARCH-24 fälschlich einen Rahmen der Namenssuche; dort steht Kommunikationseintragssuche. Seine Herleitung nennt außerdem eine „einrichtungsübergreifende“ Suche, obwohl die Vorlage die Einrichtungsgrenze sichern soll. Der tragende Bezug zwischen REQ-60 und REQ-84 bleibt dennoch nachvollziehbar. AN-11 behandelt Aufbewahrungs-/Löschregeln pauschal als fehlend, obwohl ARCH-47 nachträgliche Löschung ausdrücklich ausschließt. Eine erneute Klärung kann sinnvoll sein, muss aber diesen bestehenden Konflikt benennen. Eine rechtliche Prüfung dieser Produktvorgabe wurde nicht vorgenommen.

**Neuheit:** Acht Vorschläge ergänzen den untersuchten Bestand, neun enthalten sowohl bereits vorhandene als auch zusätzliche Teilaspekte. Kein Vorschlag wird als vollständig redundant gewertet. Beispiele für Überschneidungen: Erinnerungsrücknahme bei Absage steht bereits in ARCH-48 (AN-04), Änderungsaccount/Zeitpunkt in L3-REQ-001/PBI-004 (AN-05), Prüfung von Einwilligungsblockaden in PBI-015 (AN-08). Die Normierung auf 17 Vorschläge sagt nichts über die Menge sämtlicher unentdeckter Lücken; ein Recall dafür wird nicht berechnet.

**Kritiker:** 13 Vorschläge wurden behalten, vier aussortiert. Das Behalten ist im gewählten Raster jeweils vertretbar, ohne die einzelnen Qualitätsmängel aufzuheben. Bei allen vier Aussortierungen ist die Begründung nur teilweise überzeugend:

| Fall | Tragfähiger Teil der Kritik | Verbleibender sinnvoller Beitrag |
|---|---|---|
| AN-07 Rechteklärung | Die Frage bündelt mehrere Rollen/Operationen; Aufteilung kann helfen. | Ein zusammenhängender, im Bestand teilweise offener bzw. widersprüchlicher Rechteumfang ist konkret benannt. |
| AN-12 Performance | Es fehlen abnahmefähige Schwellen; „wahrnehmbar“ ist unbestimmt. | Die Vorlage fordert ausdrücklich deren Festlegung für drei vorhandene Funktionen. |
| AN-14 Offline-Risiken | Es gibt Überschneidungen mit dem Offline-/Sync-Thema. | Duplikation, Reihenfolge und verspätete Klärung sind nicht identisch mit der Anzeige des Offline-Betriebsmodus. |
| AN-17 Gewöhnung an Hinweise | Eintritt und Gegenmaßnahmen sind hypothetisch; keine fertige Produktregel. | Konkrete kombinierte Hinweise motivieren einen prüfbaren Nutzungs-/Gestaltungsbedarf. |

Dies belegt keine vier Fehlentscheidungen des historischen Kritikers. Sein genauer damaliger Prompt ist nicht gesichert; zudem hängt die Bewertung von der gewünschten Reife einer Gate-Vorlage ab. Das Ergebnis zeigt einen Zielkonflikt: Strenge Vorlagereife kann den automatisch weitergegebenen Klärungsumfang einschränken. Die ausgesonderten Vorschläge bleiben im Bericht auffindbar. Keine Kritiker-Präzision oder Qualitätsverbesserung in Prozent wird daraus berechnet.

## 4. Ganze PBI-/Issue-Paare

Die folgenden Einzelfallprofile gehören zusammen: E/T/N/NB bezeichnen je Kriterium einen Zustand desselben Paars. Die erste Spalte ist der Zustand vor der PBI-Änderung, die zweite der Nachzustand.

| Paar | B1 Richtigkeit | B2 Inhalt | B3 Verständlichkeit | B4 AK prüfbar | B5 Beziehungen/Konsistenz | B6 Herkunft/Projektion |
|---|---|---|---|---|---|---|
| PBI-003 | N → E | T → T | T → E | T → E | N → E | NB → E |
| PBI-032 | E → E | T → T | T → E | N → E | E → E | NB → E |
| PBI-046 | E → E | E → E | E → E | E → E | E → E | NB → E |

**NB → E bei B6 ist kein gemessener Herkunftsgewinn.** Das Paket rekonstruiert gezielt die neue Änderungskette, nicht sämtliche ursprünglichen Entstehungs-/Freigabeketten. Die alten Außensichten sind für den Inhaltsvergleich vorhanden. Bei PBI-003 weicht außerdem die Readiness-Angabe der alten Issue-Fassung vom herangezogenen Core-Vorstand ab; zeitliche Entstehung und Ursache sind hier ungeklärt. Diese Beobachtung ist kein Nachweis einer heute bestehenden Fehlfunktion.

**PBI-046 / Besuch:** Alle zwei alten und fünf neuen Teilinhalte sind ausdrücklich vorhanden. Die sechs neuen Akzeptanzpunkte beschreiben Ankündigung, Änderung/Absage der beiden Ausgangsstatus, Frist und Folgen. Dies ist der bereits als N07 teilweise untersuchte Besuchsfall, nun als vollständiges Paar vertieft; keine zusätzliche unabhängige Beobachtung.

**PBI-032 / Übergabe:** Alle vier neuen Teilinhalte sind enthalten. Eine Notiz pro Schicht bleibt benannt. Die prominente Anzeige für die nächste Schicht und der wöchentliche PDF-Export sind im ausgearbeiteten PBI weiterhin nicht abgedeckt. Der Export fehlte schon vorher; der alte, abgeschnittene Titel enthielt immerhin den Hinweis auf nächste Schicht/App-Öffnen. Beide Requirements bleiben verlinkt und erreichbar. Deshalb ist die Aussage „gesamtes Wissen verloren“ falsch; ebenso zu stark wäre „vollständiges, eigenständig ausgearbeitetes Arbeitspaket“. Die Autorenrückmeldung sieht sowohl Erweiterungspotenzial als auch Wert der Verknüpfungen.

**PBI-003 / Rechte:** Alle drei neuen Suchinhalte sind enthalten. Die sieben alten Akzeptanzpunkte wurden vollständig durch drei Suchkriterien ersetzt, während das Ziel weiterhin die Rechteklärung einschließt. Die alte Nur-Lese-Regel widerspricht bereits erlaubten Angehörigen-Uploads; weitere Altregeln sind im Verhältnis zu Bewohner-Account, Einladungscode-Zugang und Medikamentenübersicht unklar. Daher werden nicht sieben verlorene gültige Anforderungen gezählt. Feststellbar ist: Die Rollenklärung besitzt nachher keinen eigenen Akzeptanzpunkt mehr. Die Beziehungen zu REQ-08/09/10 und ARCH-08 bis ARCH-11 bleiben erhalten.

**Zusätzliche Inhaltsbeobachtung:** ARCH-11 wird im Ingest vom allgemeinen Zugriffsschutz für Mitarbeiter auf den Suchpfad verengt. Die allgemeine Grenze ist weiterhin in REQ-09 vorhanden. Die Aussage des Ingest-Agenten, die allgemeine Architekturformulierung bleibe durch die Suchkonkretisierung fachlich vollständig erhalten, wird nicht als Prüfergebnis übernommen. Dies illustriert die Differenz zwischen erhaltener Information im Gesamtbestand und Umfang eines einzelnen fortgeschriebenen Textes.

**Gezählte Teilinhalte:** 12/12 neue Soll-Inhalte sind nach der Änderung ausdrücklich vorhanden: 3/3 im Rechte-, 4/4 im Übergabe- und 5/5 im Besuchsfall. Für die zwölf alten Prüfeinheiten gilt: sechs als anwendbar, eine widersprüchlich, fünf hinsichtlich Fortgeltung ungeklärt. Deshalb wird keine pauschale Altinhalt-Erhaltungsquote gebildet. Jede Einheit ist in `required_content_rows` sichtbar; „weiter verlinkt“ und „im PBI ausformuliert“ werden getrennt festgehalten. Diese Zahlen beschreiben die gewählte Zerlegung, keine gleichgewichteten unabhängigen Qualitätsfälle.

**Akzeptanzkriterien:** Vorher neun vorhandene Kriterien (7/0/2), nachher zwölf (3/3/6). Im Ersturteil sind vorher sechs eindeutig, drei teilweise prüfbar; nachher zwölf eindeutig prüfbar. Das PBI ohne alte Kriterien bleibt für B4 „nicht erfüllt“ und wird nicht wegen des leeren Kriteriumsbestands aus der Bewertung entfernt. Gute Prüfbarkeit vorhandener Kriterien gleicht fehlenden fachlichen Umfang nicht aus.

## 5. Agentischer Beitrag und kontrollierte Anwendung

56 Such- und 36 Einzelabrufe, fünf Einreichungen und eine Kritikerabgabe sind in Start-/Endereignissen protokolliert. Alle 17 gespeicherten Statements stimmen mit den Einreichungen überein. Die Auswahl-Delta-IDs CA-1 bis CA-4 entsprechen AN-01/04/05/13; diese Zuordnung wird anhand der Inhalte geprüft. Die Steward-Indizes [1,4,5,11] beziehen sich auf die 13 behaltenen Vorschläge, nicht auf die ursprünglichen 17.

Die ausgewählten Inhalte werden zu einer Architekturverfeinerung (ARCH-11), zwei neuen Requirements (REQ-91/92) und einer offenen Entscheidung (DEC-013). Die Pläne, gespeicherten Gate-Antworten und Zustände belegen diesen Weg. Die drei PBI-Alignments wurden akzeptiert; ihre Titel-/Ziel-/AK-Texte stimmen mit dem gespeicherten Ergebnis überein. Die `edited`-Felder enthalten dieselben Werte wie die Vorlage und belegen keine eigenständige menschliche Textkorrektur. Ein tatsächlicher Forward-Bericht und eine spätere archivierte Rücklesung belegen die drei Issue-Updates; Titel und vollständiger Body stimmen mit dem Forward-Plan überein.

Die Zahlen der Werkzeugaufrufe sind keine Qualitätswerte. Die Inhalte der Rückgaben fehlen, sodass Informationsnutzung und individuelle Suchstrategie je Vorschlag nur begrenzt nachvollzogen werden können. Der ursprüngliche Benutzerwortlaut der Steward-Sitzung ist nicht vollständig gesichert. Die Sitzung belegt daher Integration und Ausführung, keine autonome Auftragswahl. Es ist keine Repair-Runde in dieser Analyst-Untersuchung beobachtet. H1 ist entsprechend teilweise, H2–H6 sind im definierten Prozessumfang belegt.

## 6. Bedeutung für A1–A10 und die Forschungsfrage

- **A1/A2:** Beobachtbare Werkzeughandlungen und Übergaben sind konkreter belegt; fehlende Rückgaben begrenzen die Prozessrekonstruktion.
- **A3/A4:** Bestandsanker ermöglichen Inhaltskontrolle. Kritikerfilterung ist sichtbar, aber kein selbstverständlicher fachlicher Qualitätsgewinn.
- **A5/A7:** Laufübergreifende Bestandsnutzung und kontrollierte Fortschreibung erzeugen nachvollziehbare Ergänzungen. Relationen allein sichern die ausreichende Ausarbeitung jedes neuen Textstands nicht.
- **A8:** Drei reale Projektionen stimmen mit ihrem freigegebenen Plan überein. Fachliche Artefaktqualität bleibt eine eigene Prüffrage.
- **A6/A9/A10:** Die vorhandenen Governance-, Durabilitäts- und Bediennachweise bleiben maßgeblich. Die Zusatzkette erweitert den Fallzugang, ersetzt keine fehlenden Kontroll-/Ausfall-/Autonomienachweise.

Für die Forschungsfrage lässt sich nach abgeschlossener Autorenprüfung entsprechend begrenzt argumentieren: Bestandsbezogene Agenten können fachliche Erweiterungen und Klärungsbedarf vorschlagen; Werkzeuge erschließen den Bestand, Workflows führen die Übergaben und Freigaben, und deterministische Operationen übernehmen/projizieren die Entscheidungen. Die untersuchten Fälle zeigen gleichzeitig, dass formale Kontrolle, Quellenbezüge und Freigaben keine vollständige semantische Qualität garantieren. Ein kausaler Vorteil gegenüber einem festen Modellaufruf oder freier Agentenkooperation wurde nicht untersucht.

## 7. Abschlussstatus und nächste Arbeit

Die KI-Erstauswertung des vereinbarten Materials ist vollständig abgelegt: 17 Vorschläge, vier Kritiker-Aussortierungen, drei Paare vor/nach der Änderung, 21 vorhandene Akzeptanzpunkte, 24 Soll-Inhaltseinheiten und H1–H6. Diese Summen dürfen nicht als Zahl unabhängiger Versuche addiert werden. Drei Autorenantworten sind punktuell eingearbeitet; eine menschliche Vollprüfung ist offen. [Autorenprüfblatt](./autorenpruefung.md) führt von Kurzbefund zu vollständig begründeten Einzelurteilen und Originalen.

Für die historischen Fragen fehlt kein zusätzlicher Lauf. Ein neuer Lauf würde Stabilität oder einen neuen Stand untersuchen, nicht die alten Urteile unabhängig machen. Zunächst die inhaltlichen Urteile gegenprüfen; anschließend Verfahren/Ergebnisse in 7.2/7.5/7.6 und Bilanz/Grenzen in 7.7/7.8 integrieren sowie Kapitel 3/8 abgleichen. Keine automatische Code- oder Promptänderung aufgrund dieser Bewertung. Als mögliche technische Folgefrage ist die Erhaltung gültiger Altinhalte beim PBI-Alignment konkret benannt; über eine Änderung wäre gesondert anhand des aktuellen Codes und gezielter Regressionstests zu entscheiden.

**Overleaf:** Noch keine TeX-, Bib- oder Grafikänderung. Diese Ergebnisvorlage braucht derzeit keinen Sync.

## 8. Nachrechnung

`python3 check-projection-chain.py` prüft die oben bezeichneten Übergänge ausschließlich anhand der archivierten Dateien; `projection-chain.json` enthält Einzelvergleiche und Dateiidentitäten. Die semantischen Ersturteile und ihre Zählungen wurden durch diese Ergänzung nicht verändert.

`python3 measure.py` zählt die gespeicherten Urteile erneut und kontrolliert ausgewählte archivierte Inhalts-/Zustandsentsprechungen. `summary.json` enthält die zugehörigen Zahlen. Dies bestätigt die Rechnung und Materialzuordnung, nicht die Richtigkeit eines semantischen Urteils. [Eingangsinventar](./input-manifest.json) und [Ergebnisinventar](./result-manifest.json) sichern den hier verwendeten Stand.

## Redaktionsstand

Ergänzung vom 16.09.2026: Nach Autorenhinweis Core-Artefaktqualität und GitHub-Übertragung ausdrücklich getrennt und die Feldentsprechung über den gespeicherten Synchronisationseintrag nachgerechnet. Keine Änderung der Rubrik oder semantischen Bewertungen. Die vorherige Berichtsfassung bleibt in [revisions/README-before-projection-clarification.txt](./revisions/README-before-projection-clarification.txt) erhalten. Die anschließende Autorenprüfung ist in [author-followup.json](./author-followup.json) dokumentiert: AN-01 erhielt allgemeine Zustimmung zur Idee bei ausdrücklich genannten Verständnisgrenzen; damit wurde kein Einzelkriterium validiert. AN-11 wird als nennenswerter, nachrangiger Klärungsbedarf eingeschätzt; das Verhältnis zur bestehenden Nichtlöschungsregel wird damit nicht bewertet. Die Antwort zu PBI-003 steht aus.
