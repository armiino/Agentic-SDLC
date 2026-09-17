# Inhaltliche Herkunft und Fortschreibung: Ergebnis der ergänzenden Fallprüfung

Stand: 15.09.2026. Acht nach festgelegter Regel ausgewählte historische Einordnungsoperationen und vier getrennte bekannte Kontrollfälle. Die Auswahl wurde nicht verändert. Kein neuer System-/Modelllauf, keine Codeänderung, keine unabhängige Annotation. [Prüfprotokoll](../protokoll.md), [Auswahl](../selected-cases.json), [Einzelurteile](assessments.json), [Autorenfragen und Antworten](authors.json).

## 1. Ergebnis und Bedeutung für die Arbeit

**Der Zusatz liefert inhaltliche Belege für die laufübergreifende Verarbeitung, die über technisch gültige Verweise hinausgehen.** Besonders N07 lässt eine neue, kenntlich gemachte Entwurfsableitung vom bestehenden Projektwissen über Freigabe und Anforderung bis zum inhaltlich erweiterten Arbeitspaket und zur protokollierten GitHub-Aktualisierung verfolgen. N04 zeigt eine eng begrenzte Präzisierung bei erhaltenen fachlichen Einschränkungen. N03 ergänzt den bekannten REQ-42-Gegenfall um eine weitere dokumentierte Ablehnung einer unvollständigen Agentenvorlage.

Die Ergebnisse tragen zugleich keine pauschale Qualitätsbehauptung. Eine Teilüberlappung kann zur zu weit gehenden Einordnung als bereits erfasste Frage führen (N06). Ein früher Lauf übernimmt eine Anforderung, erreicht aber die vorgesehene Feature-/PBI-Bildung nicht (N01). Eine richtige Konfliktbehandlung kann neben einer ungeklärten oder sachfremden vorgelagerten Quellenangabe stehen (N08). Das sind unterschiedliche Befunde: Quellenwiedergabe, Zuordnung, Autorisierung und nachfolgende Verarbeitung dürfen nicht zu einem einzigen Erfolgswert zusammengezogen werden.

Für die Forschungsfrage ist die beobachtbare Aufgabenverteilung entscheidend: Die Agenten lesen und vergleichen Projektwissen, schlagen Zuordnungen und Texte vor. Die angebotene Operation begrenzt die mögliche Zustandswirkung; eine Freigabe oder historische automatische Torantwort bestimmt ihre Übernahme. Die gespeicherten Fassungen und Beziehungen erlauben anschließend zu prüfen, was tatsächlich entstand. Weder die feste Schreiblogik noch ein freigegebener Vorschlag garantieren dabei semantische Richtigkeit. Die Fälle begründen die untersuchte Kombination aus Agentik und kontrollierter Verarbeitung; sie beweisen nicht deren allgemeine Überlegenheit oder Notwendigkeit.

## 2. Untersuchungsumfang und tatsächliches Vorgehen

Auswahlbasis waren 154 Operationen aus 65 vorhandenen Anforderungsplänen unter 143 Laufverzeichnissen. Je ein zusätzlicher Fall pro Eingangsgruppe und Auswahlfamilie wurde nach der zuvor gesicherten Hash-Rangfolge gewählt: vier Eingangsgruppen, jeweils Neuanlage oder Bestandsbezug. Kein Fall wurde nach Betrachtung seines Ergebnisses ausgetauscht. Vier bereits bekannte Beispiele werden separat geführt. Dies ist eine geschichtete, regelgebundene Auswahl aus vorhandenen Vorschlägen, keine Zufallsstichprobe unabhängiger realer Projekte.

Vor der Bewertung wurden benötigte Dateien kopiert und gehasht. Zunächst wurden Quellen/Delta und Zielkontexte aller acht zusätzlichen Fälle gelesen; anschließend Vorschläge, Entscheidungen und gespeicherte Ergebnisse. Die Lesung erfolgte damit in zwei Durchgängen über die feste Fallfolge, nicht als verblindete Untersuchung. Fehlende oder zunächst anders benannte Dateien wurden anhand der tatsächlich vorhandenen Schemata aufgelöst: `human-decisions.json` und `ingest-gate-decisions.json` sind historische Varianten; das Eingangsobjekt liegt in diesen Fällen unter `04-delta/project-state.json`. Diese Anpassungen betreffen die Belegerschließung, nicht Auswahl oder Bewertungsmaßstab.

Das Paket enthält 750 unveränderte Belegdateien (einschließlich des Textstands für B-60): die zwölf ausgewählten Laufverzeichnisse ohne Checkpoint-Dateien, direkt bezeichnete verfügbare Eingaben sowie drei zusätzlich benötigte Quellenläufe. Diese Kopie ist kein vollständiger Repository-/Umgebungs-Freeze. Ein heutiger Dateipfad oder Kopierhash beweist allein nicht die Identität einer Datei zum historischen Ausführungszeitpunkt. Bei N01/N02 wurden daher zusätzlich die gespeicherten Atomic Units gelesen; bei GitHub-Fällen die damaligen archivierten Issue-Texte/Kommentare; bei N07 der bezeichnete Analystenbericht und das ausgewählte Delta. Die ursprüngliche temporäre Datei von N08 fehlt. Die erste Erfassungsroutine hatte logische Quellenkennungen, URLs und ein Verzeichnis fälschlich als fehlende Dateien behandelt; die korrigierte Klassifikation und der ursprüngliche Erfassungsstand bleiben erhalten.

Pro Fall wurden Inhalt/Herkunft (K1), Einordnung (K2), Erhaltung (K3), Freigabe/Wirkung (K4) und Beziehungen/Folgeergebnisse (K5) geprüft. K1–K3 unterscheiden Vorschlag und gespeicherten Zustand. Die genauen Bezugsgegenstände stehen in den Fallkarten; ein getragenes K4 besagt beispielsweise nur, dass die dokumentierte Torantwort mit der beobachteten Wirkung übereinstimmt. Es besagt nicht, dass eine fachkundige Person geprüft hat oder deren Urteil richtig war.

Drei Rückfragen an den Autor ergaben keine zusätzliche sichere Tatsachenfeststellung: N01 blieb hinsichtlich Modalität uneindeutig, N06 erhielt eine vorsichtige Erinnerung an einen Testzweck, N08 blieb hinsichtlich Eingabeentstehung ungeklärt. Die Antworten werden wörtlich erhalten. Keine unabhängige Zweitannotation, keine menschliche Vollprüfung der zwölf Fälle und keine künstliche Einigung aus unsicheren Antworten.

## 3. Acht zusätzliche Fälle

| Fall | Eingang und Vorschlag | Nachgewiesene Wirkung | Inhaltliches Ergebnis / Grenze |
|---|---|---|---|
| **N01** | Meeting → neuer Medikamentennachweis | REQ-81 angelegt; automatische Annahme; NEW_FEATURE mangels Create-Draft übersprungen | Delta-Inhalt bleibt erhalten. Verpflichtungsgrad zur Rohquelle mehrdeutig; Feature/PBI-Folge nicht erreicht. |
| **N02** | Meeting → RESTATE auf REQ-61 | Text und Version unverändert; zusätzliche Claim-ID | Inhaltlich passende Bestätigung. Neuer Herkunftsbeitrag am Item ohne eigenen Laufbezug; kein neuer Verlust des ursprünglichen Texts. |
| **N03** | Autor-Delta → Besuchsstatus als NEW_RELATED | Mit konkreter Fehlstellenbegründung abgewiesen; keine neue Anforderung | Drei ausdrücklich vorgegebene Statuswerte fehlen im Vorschlag. Menschliche Ablehnung verhindert hier die unvollständige Übernahme. |
| **N04** | Autor-Delta → REFINE der Besuchserinnerung | REQ-88 v2 mit 18 Uhr und alter Fassung; angeglichene PBI-Ausgabe | Neuer Zeitpunkt und bisheriger fachlicher Umfang erhalten. GitHub nur vorbereitet; kein tatsächlicher Write. |
| **N05** | GitHub-Issue-Text → neuer PDF-Export | Als Testzeile abgewiesen; REJ-018 gespeichert | Keine ungeprüfte Umwandlung des Testbedarfs in gültige Anforderung. Scope-Entscheidung, kein Qualitätsfehler einer echten Exportfunktion. |
| **N06** | GitHub-Kommentar → ALREADY_DECIDED auf DEC-006 | Abgewiesen; Zielentscheidung unverändert; REJ-015 | Ziel trägt nur eine Teilfrage. Ablehnung belegt keine vollständige vorherige Abdeckung; ihr Testhintergrund bleibt unsicher. |
| **N07** | CoreAnalyst-Delta → NEW_RELATED | REQ-91 → PBI-046/FC-15 → protokolliertes Update von Issue 44 | Neue Entwurfsregel kenntlich abgeleitet und freigegeben; alte und neue PBI-Inhalte sowie passende Beziehungen belegt. |
| **N08** | Sonstiges Delta → CONTRADICT auf REQ-70 | Offene DEC-001 mit Konfliktkante; alter Text bleibt; Pause am Entscheidungstor | Delta→Konflikt getragen. Ursprung vor Delta nicht rekonstruierbar; sachfremde Claim-Angabe ist kein Inhaltsbeleg. |

Bezugsmenge dieser Tabelle sind genau acht Operationen: drei wurden abgewiesen, fünf angewandt (zwei Neuanlagen, eine Verfeinerung, eine Bestätigung, eine Konfliktöffnung). Das sind **Ausführungsarten, keine Qualitätsquote**. Bei fünf Fällen sind Entscheidungsdateien mit menschlicher/Autorenrolle vorhanden; drei historische Ingest-Tore wurden nach Ereignisprotokoll automatisch mit `AcceptAll` beantwortet. Eine andere Voreinstellung in der Konfiguration oder ein Reviewer-Text überstimmt diesen Ausführungsbeleg nicht.

### N07: der stärkste zusätzliche positive Inhalts- und Relationsbeleg

Der Analystenbericht leitet aus bestehender Besuchsankündigung, Pflegeentscheidung und Erinnerung eine neue Regel ab: Angehörige sollen angefragte oder bestätigte Besuche bis zum Beginn ändern oder absagen können; Änderungen führen zurück zu „angefragt“, nach Absagen entfallen Erinnerungen. Dies ist fachlich plausibel, aber nicht logisch zwingend aus den Altanforderungen ableitbar. Es bleibt ein neuer Entwurfsvorschlag, der erst durch die dokumentierte Übernahme Projektgeltung erhält.

Die Einordnung ergänzt eine eigene REQ-91. Die spätere PBI-Zuordnung erweitert PBI-046, das schon die Ankündigung mit Datum und Uhrzeit abdeckt. Der gespeicherte Nachzustand erhält diese beiden alten Vorgaben und ergänzt alle vier neuen Aspekte. Gleichzeitig entstehen die Beziehungen `PBI-046 → REQ-91` (Abdeckung) und `REQ-91 → FC-15` (Feature-Zugehörigkeit). Beide tragen den Erzeugerschritt der PBI-Fortschreibung. Die vorhandene Verbindung zum GitHub-Issue 44 bleibt; dessen Update wird im Ausführungsbericht als durchgeführt ausgewiesen. Der Erzeugerschritt einer Kante ist weiterhin keine vollständige Zeitgeschichte.

Der Analyst und die Einordnungsagenten haben laut Werkzeugprotokollen tatsächlich auf Projektwissen zugegriffen. Diese Protokolle belegen die Ausführung, nicht, dass jedes verfügbare Element vollständig verstanden oder alle denkbaren Konflikte gefunden wurden. Auch die ausgefüllten PBI-Entscheidungsfelder belegen keine eigenständige menschliche Textverbesserung: Die übermittelten Werte entsprechen hier dem Vorschlag.

### N03 und N05: verschiedene Gründe für menschliche Nichtübernahme

Bei N03 nennt das Autor-Delta ausdrücklich genau drei mögliche Statuswerte. Im gekürzten Agentenvorschlag fehlen sie. Die historische Ablehnung benennt diese Auslassung und fordert eine vollständige Neuaufnahme. Der fehlende Ingest und die unveränderten Items belegen den konkreten Schutz vor dieser unvollständigen Vorlage. Eine erfolgreiche spätere Neuaufnahme wird diesem Fall nicht zugerechnet. In den beiden erhaltenen Core-Snapshots findet sich für N03 auch kein neuer REJ-Eintrag; die Ablehnung ist jedoch separat dokumentiert. Die Existenz gespeicherter Ablehnungsproposals in anderen Fällen wird daher nicht ungeprüft auf diesen historischen Lauf übertragen.

N05 betrifft dagegen keine nachweislich gewollte Produktanforderung: Der Autor kennzeichnet die PDF-Zeile als Abnahmetest und lehnt sie deshalb ab. Der Agent hatte den technischen Inhalt korrekt erkannt und sinnvoll von der vorhandenen Anzeigeanforderung getrennt, aber den Testcharakter im normativen Vorschlag nicht erhalten. REJ-018 bewahrt Vorschlag und Grund. Der Fall zeigt, weshalb Inhaltsinterpretation und Produktentscheidung unterschiedliche Aufgaben sind.

### Grenzen, die nicht miteinander verwechselt werden dürfen

N02 bestätigt den passenden Inhalt, ergänzt aber am Item keine separat auflösbare vollständige Herkunft der erneuten Bestätigung. Das entspricht der bereits bekannten Grenze B-51. N06 betrifft dagegen eine semantisch nur teilweise gedeckte Zuordnung eines Fragenbündels. N08 besitzt eine nachvollziehbare Konfliktwirkung, aber eine nicht belegbare Vorgeschichte der Eingabe. N01 endet in diesem frühen Stand mit gespeicherter Anforderung und ausdrücklich übersprungener Feature-Erstellung. Aus keinem dieser Befunde folgt pauschal, dass das heutige System gar keine nachvollziehbaren Beziehungen erzeugt. Umgekehrt darf ein positiver N07-Fall diese Grenzen nicht verdecken.

## 4. Bekannte Kontrollfälle, getrennt ausgewiesen

| Kontrolle | Bestätigter Mechanismus | Grenze der Aussage |
|---|---|---|
| **K01 / F1** | Abgewiesener REFINE hätte Bemerkungsregeln von REQ-42 verdrängt; v3 bleibt erhalten, REJ-002 führt Grund und Vorlage. | Bekannter Fall, kein zusätzlicher unabhängiger Erfolg. Änderungen an PBI-028 im gleichen Lauf haben andere Auslöser. |
| **K02 / F2** | Neue Angehörigen-/Medikationsregeln in REQ-42 und PBI-028 erhalten, alte Fassung historisiert, Übernahme dokumentiert. | Der alte Kalender-Prüfauftrag steht danach nur noch in der History; seine Rücknahme ist nicht eigens begründet. Formularfelder entsprechen dem Vorschlag, kein isolierter redaktioneller Qualitätsgewinn. GitHub nur Dry-Run. |
| **K03 / F3** | Menschlicher ADOPT_NEW-Text erzeugt neue Anforderung, löst alte ab und ändert Abdeckungsbeziehungen. | Automatischer Ingest und interaktive Auflösung unterscheiden. Geänderte Beziehungen ergeben keine vollständige semantische Aktualisierung aller PBI-Texte. |
| **K04 / Besuchsbeispiel** | GitHub-Kommentar → REQ-89 → PBI-047/FC-15; alter Übersichtszweck plus 14-Tage-Zeitraum; Issue-Update protokolliert. | Bekannter Fall. Kein Vollständigkeitsnachweis für beliebige Kommentare und kein unabhängiger Wiederholungsversuch. |

Die Kontrollen bestätigen nicht alle denkbaren Inhaltsversprechen. Insbesondere K02 ist ein Beleg autorisierter Konkretisierung und Zustandswirkung, aber kein Beleg verlustfreier Erhaltung sämtlicher Altinhalte. Das war auch nicht automatisch dadurch gegeben, dass die Altversion archiviert blieb. Die neue Bewertung ersetzt keine historischen Originaldateien oder die bisherige Fallserie; ihr engerer Inhaltsbefund bleibt hier sichtbar.

## 5. Beitrag zu Methodik und Evaluation

Der Zusatz operationalisiert bestehende qualitative Ziele der Arbeit: A3 (bezeichnete Herkunft), A5/A7 (laufübergreifende Projektzustände und Einordnung) sowie A8 (bezogene Projektion). Er passt zur in Kapitel 3 bereits beschriebenen rückblickenden Analyse ausgewählter Entwicklungsfälle. Im Sinne des im Vorbereitungsprotokoll belegten DSRM-Verständnisses wird die gewünschte Eigenschaft mit beobachteten Ergebnissen verglichen. Die Untersuchung gewinnt ihren Wert aus diesem überprüfbaren Vergleich, nicht aus der Zahl der Tabellen oder einer künstlich erzeugten Gesamtmetrik.

Die Evaluation ist damit in einem wichtigen Bereich stärker: Für ausgewählte Verbindungen wird nicht nur gefragt, ob IDs vorhanden sind, sondern ob die verbundenen Inhalte zusammenpassen, Einschränkungen erhalten bleiben und die gespeicherte Wirkung der dokumentierten Entscheidung entspricht. Das ergänzt die breite technische Herkunftsbilanz; es ersetzt sie nicht. Die Arbeitsaufteilung zwischen Agent, Mensch und fester Anwendungslogik wird an tatsächlichen Eingängen und Zuständen beurteilt.

Die Grenze bleibt erheblich und muss kurz sichtbar sein: wenige entwicklungsbekannte historische Fälle, gemeinsame Projektstände, teils Testeingaben/automatische Tore, unterschiedliche Softwarestände, keine unabhängige Fachannotation und kein Vergleich alternativer Architekturen. Die Auswahl aus erzeugten Vorschlägen misst nicht, ob fehlende Anforderungen überhaupt erkannt wurden. Sie misst weder vollständige fachliche Herkunft für alle 237 Elemente noch Zuverlässigkeit aller Eingangswege, Usability, menschlichen Aufwand oder den isolierten Effekt von MAF. Die zusätzlichen Inhalte werden deshalb nicht als neue summative Laufserie am finalen Abgabestand bezeichnet.

## 6. Konsequenz für den bestehenden Abschlussplan

**B-60 – bestätigte Darstellungsabweichung in 7.5:** Der aktuelle [TeX-Text, Zeilen 95–98](evidence/Thesis-Docs/Writing/claude-writing/kapitel-7/tex-v2/chap7.5/chap7.5.tex) schreibt die redaktionelle PBI-Anpassung bei F2 dem Menschen zu und rechnet sie nicht als maschinelle Leistung. Im `pbi-change-plan.json` stehen aber bereits genau der Titel, der Zieltext und die Akzeptanzkriterien, die später in den `edited…`-Feldern der menschlichen Entscheidung übermittelt werden. Belegt ist somit eine agentisch vorgeschlagene und menschlich freigegebene Inhaltsangleichung; eine eigenständige menschliche Textänderung ist nicht nachgewiesen. Dies ist ein Text-/Leistungszuordnungsfehler, kein Defekt der Speicherung. Korrektur im folgenden Textpaket zusammen mit dem Ergebnisanschluss.

**Empfohlen ist ein knapper Textanschluss, kein Systemumbau und kein weiterer breiter Lauf.** In 7.5 genügen der Zweck der kriterienspezifischen Nachprüfung, N07 als zusätzlicher positiver Inhalts-/Relationsbeleg und N03 als beobachtete Entscheidung gegen eine unvollständige Vorlage. Der Befund zu K02 gehört in die Begrenzung einer etwaigen vollständigen Erhaltungsbehauptung, nicht in eine neue Funktionsbeschreibung. Die Auswahl und eine kompakte Matrix gehören in den Evaluationsanhang bzw. dieses Paket. Das Ergebnisprofil in 7.7, die Grenzen in 7.8 und die Forschungsantwort in 8.1 müssen bei Textintegration zusammen geprüft werden; keine neue Überlegenheitsbehauptung.

Keiner der zusätzlichen Fälle belegt allein einen bislang unbekannten zentralen Fehler des heutigen Codebestands. Der frühe NEW_FEATURE-Skip, die sachfremde Quellenangabe mit unbekannter Entstehung und die unvollständige Fragenzuordnung werden ausdrücklich festgehalten, aber nicht ohne historische Ursachenprüfung zu einem Fixauftrag gemacht. B-51 bleibt die bekannte Bestätigungsgrenze. Ein Eingriff in aktuelle Funktionalität ist in dieser Prüfung nicht erfolgt.

Anschließend bleibt die bestehende Reihenfolge: gezielter Textnachzug und PDF-Abnahme, Beleg-/Materialabschluss, Grafik/Satz/Gesamtabnahme, Repository-Dokumentation und zuletzt der tatsächliche Artefakt-Freeze. Die vorliegende Prüfung schließt den begrenzten Auswertungsauftrag; sie eröffnet keine zwölf neuen Teilstudien.

## 7. Belegzugang und technische Prüfung

- [Fallkarten mit Einzelurteilen](fallkarten.md) und [maschinenlesbare Bewertungen](assessments.json).
- Zwölf `dossiers/*.json` erschließen Originalobjekte aus Quelle/Delta, Vorschlag, Entscheidung, Vor-/Nachzuständen, Beziehungen und Ausführungsbelegen. Sie sind Auszüge, keine neuen historischen Originale.
- [Quellenmanifest](evidence-manifest.json) enthält Originalpfad, Paketpfad, SHA-256, Dateigröße und Rolle. Die Kopien liegen unter `evidence/`.
- [Autorenfragen/-antworten](authors.json) erhalten Wortlaut und begrenzte Aussagefolgen.
- `scripts/verify.py` prüft die unveränderte Auswahl, Paketdateien und konkrete Zustands-/Textbefunde ohne Modellaufruf. Das reproduziert die Beleg- und Konsistenzprüfung, keine unabhängige Neubewertung der fachlichen Urteile.
