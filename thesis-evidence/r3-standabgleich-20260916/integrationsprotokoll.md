# Begrenzter Integrationsfall R3 – vor Ausführung festzulegen

Status 16.09.2026: **vorbereitet, nicht ausgeführt**. Keine erfundenen Ergebnisse. Anlass ist die gemeinsame Ausführung des späteren Transkriptpfads, nicht das vermeintliche Fehlen historischer Ende-zu-Ende-Belege. Der historische F1-Lauf bleibt bestehen. Ein neuer Fall wäre eine technische Demonstration am neuen Stand, keine unabhängige Replikation der fachlichen Ergebnisse.

## Gegenstand und Kandidat

Eine isolierte Arbeitskopie des tatsächlich abzugebenden Codes einschließlich unversionierter Dateien, Paketdefinitionen, Prompts und nicht geheimen Laufparametern. Vor Beginn dateibasiertes Manifest sichern; HEAD allein genügt nicht. API-Schlüssel werden nicht in das Belegpaket übernommen. Ausgangsbestand und alle lokalen Daten-/Laufpfade müssen auf die Kopie zeigen; solange dies nicht nachgewiesen ist, wird nicht gestartet.

**Gewählter Eingang:** bereits entwicklungsbekanntes `input/transcripts/meeting-4-ux-block-l.txt` und als Ausgangs-Core die archivierte F1-Vorlage `runs/fullworkflow/20260818_084712_765eea/07-ingest/applied/core-before.json`. Beide unverändert kopieren und vor Beginn hashen; Quellenbezüge mitkopieren. Diese bewusste bekannte Aufgabe erlaubt einen begrenzten aktuellen Integrationscheck, keine neue externe Validität. Vorprüfung: Core lesbar, keine Strukturverletzung, Besuchsanforderungen noch nicht als Ergebnisse enthalten; das Archiv enthält den ursprünglichen Ausgangsbestand. Scheitert eine Vorbedingung, den Kandidaten vor Ausführung ändern und Änderung begründen.

Modell/Endpunkt, Modellparameter, Promptdateien, Laufprofil, reale Eingangsdatei, Ausgangs-Core und Zielverzeichnis werden im tatsächlichen Startprotokoll dokumentiert. Fachliche Tore bleiben interaktiv, keine automatische Sammelannahme. Ein menschliches gemeinsames Freigeben einer sichtbar vorgelegten Auswahl wäre gesondert zu protokollieren. Vorher festlegen: keine externen Schreibaktionen; Projektion zunächst lokal. Ein später ausdrücklich gewählter Test-Repository-Schreibversuch ist ein eigener Erweiterungsschritt und braucht sein konkretes Ziel und seinen Umfang.

## Vorab festgelegte Prüfkriterien

| Schritt | Erwartung / erlaubte Ergebnisse | Zu sichernder Beleg |
|---|---|---|
| Transkriptfront | Sprecherwechsel korrekt segmentiert; Units verweisen auf die Eingabe. Ledger und selektive Adjudikation werden ausgeführt. Mensch kann einen fachlich gerechtfertigten neuen Claim ergänzen; keine künstliche Einfügung nur zum Erzwingen eines grünen Tests. | Eingabehash, Units, maschineller Claim-Bestand, Queue und tatsächliche Antworten. |
| Facetten und Ableitung | Wenn durch die Antwort neue ausstehende Facetten entstehen: Verfeinerung vor Ableitung, anschließend 0 ausstehend. Ohne ausstehende Facetten: Durchreichpfad ohne Modellaufruf. Welcher Zweig tatsächlich eintrat, wird getrennt berichtet. | Ereignisse, vor/nach gespeicherte Claims; neue Zwischenkopien als Instrumentierung kennzeichnen. Baseline-/Delta-Ausgaben. |
| Ingest-Pause | Vor Freigabe kein fachliches Apply in der gewählten Fortschreibung. Mindestens eine inhaltlich passende neue Besuchsanforderung wird vorgeschlagen. Falls sie fehlt oder die Stufe HumanReview/Fehler meldet, ist dieses Ergebnis zu berichten. | Ereignis, Gate-Vorlage, Core-Vorher-Hash, noch kein Apply für die betreffenden Operationen. |
| Entscheidung und Anwendung | Mensch vergleicht Operationen mit Quelle/Bestand; freigegebene Menge = angewandte Menge, abgelehnte Operationen ohne fachliche Übernahme. Neue Elemente mit nachvollziehbarem Eingang und Herkunft. Keine bestimmte numerische ID erzwingen. | Antworten vor Resume separat sichern, Anwendungsausgabe, Core nach Apply. Ablehnungsvermerke getrennt von fachlichen Items prüfen. |
| Pause / Resume | Mindestens eine echte Gate-Unterbrechung; danach Fortsetzung derselben Laufkennung. Bereits beendete Ledger-Stufe nicht erneut ausgeführt. | Ereignisfolge und Checkpoints. Keine Behauptung Prozessneustart, solange kein echter Prozesswechsel protokolliert ist. |
| PBI und Beziehungen | Besuchsinhalt wird einem passenden Feature/PBI zugeordnet oder eine fachlich begründete neue Zuordnung vorgeschlagen. Freigabe vor Anwendung; alle neuen internen Endpunkte auflösbar; Erzeugerangabe und passende Quellenkette vorhanden. | Plan, Antwort, Core vor/nach, ausgewählte Relations-/Herkunftsauflösung. Fachliche Zuordnung nach Quelle prüfen, nicht nur ID-Existenz. |
| Lokale Projektion | Freigegebene Core-Titel, Zieltext, Akzeptanzkriterien und verknüpfte Anforderungskennungen erscheinen entsprechend im lokal erzeugten Issue-Entwurf. | Lokaler Entwurf und Feldvergleich zum Core. Kein externer Veröffentlichungserfolg aus einem Dry-Run. |
| Abschluss | Genau die beobachteten Teilstrecken bilanzieren; reguläres Ende oder dokumentierter Grund der Unterbrechung. Produktiver Core unverändert. | Abschlussereignis, Core-/Artefakt-Hashes, Vergleich zum Ausgangszustand der Produktivumgebung. |

## Auswertung und Abbruchregeln

Ein Fall, keine Erfolgsquote und keine neue Schätzung der Modellstreuung. Je Kriterium **erfüllt / abweichend / nicht ausgelöst / nicht beurteilbar** mit Beleg. Vorab festgelegte Reichweite: aktueller Transkripteingang, kontrollierte Bestandsänderung, nachvollziehbare Beziehungen und lokale Projektion. Github-Ernte, alle anderen Eingänge, allgemeine Absturzsicherheit, Zeitersparnis und Überlegenheit bleiben außerhalb.

Bei fehlendem Zugang, Konfigurations- oder Modellfehler werden die bis dahin entstandenen Daten archiviert. Kein Wiederholen bis zum gewünschten Ergebnis. Ein zweiter Versuch wird mit Grund, Änderung und eigener Kennung ausgewiesen. Ein bestätigter Kernfunktionsfehler wird zuerst beurteilt, nicht nebenbei behoben; technische Korrektur, Test und neuer Stand sind gesondert zu dokumentieren.

Keinen Live-Lauf mit einem zweiten unabhängigen Live-Lauf als isolierten Resume-Vergleich behandeln. Wiederaufnahme ohne Doppelwirkung braucht für einen isolierten Vergleich kontrollierte Eingaben und Modellantworten; vorhandene deterministische Tests bleiben dafür die getrennte Belegart.

**Textfolge nach Durchführung:** Tatsächlichen Stand und Beobachtungen knapp in D.6 zuordnen, die entsprechende Grenze in 7.8 anpassen, bei Abschluss D.9/6.1/6.9 füllen. Historische W2-Messwerte unverändert lassen. Erst ein positives tatsächlich protokolliertes Ergebnis schließt die konkrete Integrationslücke.
