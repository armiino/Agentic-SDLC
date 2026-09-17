# Inhaltliche Herkunft und Fortschreibung: Prüfprotokoll v1

Stand: 15.09.2026. **Vorbereitung einer ergänzenden retrospektiven Fallprüfung. Noch keine neuen Inhaltsurteile.** Die Regeln werden nach einer strukturellen Inventur der erhaltenen Dateien und vor der Auswahl und Bewertung der zusätzlichen Fälle festgehalten. Gegenstand und bekannte Beispielbefunde sind entwicklungsbekannt; weder eine verblindete Untersuchung noch ein vor den historischen Ausführungen festgelegtes Experiment wird behauptet.

## 1. Erkenntnisziel und methodischer Anschluss

**Prüffrage:** Inwiefern lässt sich an ausgewählten vorhandenen Einordnungsoperationen fachlich begründet nachvollziehen, welche Eingangsinformation oder Entscheidung zu welchem gespeicherten Inhalt und welchen Beziehungen führte?

Dies konkretisiert die bestehenden Ziele aus A3/A5/A7 und, bei vorhandener Folgeprojektion, A8. Kapitel 3 beschreibt ausdrücklich die rückblickende Analyse ausgewählter Entwicklungsfälle neben referenzgestützten Laufbewertungen und Kontrolltests. Peffers et al. unterscheiden qualitative und quantitative Lösungsziele (S. 55); Evaluation vergleicht diese Ziele mit beobachteten Ergebnissen (S. 56). Daraus folgt kein vorgeschriebener Kennzahlenkatalog und keine Pflicht zu einem neuen Vergleichslauf für jede Entwurfsentscheidung. Es folgt aber die Pflicht, den beanspruchten Nutzen mit dafür geeigneter Evidenz zu beurteilen.

Das bisherige Herkunftsaudit v4 prüft technische Auflösung und bezeichnete Text-/Versionsbezüge. Die neue Untersuchung prüft zusätzlich die inhaltliche Tragfähigkeit ausgewählter Verbindungen. Sie ersetzt weder dieses Audit noch Ledger/F, Governance-Tests oder die bestehende Fallserie. Die historischen Ausführungen bleiben historische Entwicklungsfälle; deren spätere kriterienspezifische Bewertung wird mit Datum und Prüfzweck ausgewiesen. Eine formative Entstehungsgeschichte wird dadurch nicht rückwirkend zum geplanten Experiment.

**Beitrag zur Forschungsfrage:** Für jeden Fall getrennt zeigen, welche semantische Einordnung der Agent vorgeschlagen hat, welche Freigabe oder Redaktion dokumentiert ist und welche feste Anwendungslogik den gespeicherten Zustand erzeugt hat. Das erlaubt Aussagen über die realisierte Aufgabenverteilung und ihre Grenzen. Es beweist weder die Notwendigkeit genau dieser Architektur noch ihre Überlegenheit gegenüber freien Agenten oder anderen Frameworks. Ein erfolgreicher Speichervorgang ist kein Beweis semantischer Richtigkeit; eine menschliche Auswahl ist keine unabhängige Quellenannotation.

## 2. Grundgesamtheit der Inventur und begrenzter Gegenstand

Inventarisiert werden sämtliche unmittelbaren Laufverzeichnisse unter `runs/fullworkflow/`, nicht nur die neuesten oder erfolgreich abgeschlossenen. Erfasst werden ihre Konfiguration und, sofern vorhanden, `07-ingest/plan.json` samt allen darin enthaltenen Operationen. Fehlende Dateien, leere Pläne und unbekannte Eingangstypen bleiben sichtbar. Ein Verzeichnis ohne Anforderungsplan ist nicht automatisch ein Fehlversuch: Es kann einen anderen Einstieg oder eine andere Aufgabe betreffen.

Untersuchungseinheit ist **eine vorgeschlagene Anforderungs-Einordnungsoperation in einem bestimmten Lauf**, mit eindeutigem Listenindex und `incomingItemId`. Folge-Items, Beziehungen und Projektionen gehören zur zugehörigen Belegkette; sie werden nicht als unabhängige weitere Fälle gezählt. Die Inventur stratifiziert nach der *vorgeschlagenen* Operationsart. Ob diese fachlich angemessen ist, ist gerade Gegenstand der späteren Prüfung.

Vier Eingangsgruppen werden für die zusätzlichen Fälle verwendet:

1. Transkript: expliziter Transkript-Eingang oder im alten Konfigurationsschema belegter Transkriptpfad ohne `entryPoint`; diese beiden Belegarten getrennt erfassen.
2. Autor-Delta: Delta-Eingang mit protokolliertem Pfad unter `state/steward/author-front/`. Dies belegt zunächst die Eingabeklasse; die tatsächliche Werkzeugwahl des Stewards benötigt ihren eigenen Lauf-/Werkzeugbeleg.
3. GitHub: expliziter GitHub-Eingang. Die Herkunft eines konkreten Inhalts muss anschließend am archivierten Kommentar/Issue und Delta nachgewiesen werden.
4. Sonstiger Delta-Eingang: übrige explizite Delta-Eingänge oder ältere eindeutig belegte Delta-Pfade. Kein pauschales Umbenennen in Diktat, Meeting oder Agenteneingabe.

Klärungs- und reine Projektionsläufe ohne Anforderungsplan sowie die getrennte Architektur-Einordnung gehören nicht zur Fallauswahl dieses Zusatzes. Daraus folgt keine Aussage über ihre Güte. Die Untersuchung soll einen überschaubaren Kern der laufübergreifenden Anforderungsverarbeitung prüfen, nicht alle Systemwege oder jeden Core-Typ zertifizieren.

## 3. Auswahlregel: höchstens acht zusätzliche und vier bekannte Kontrollfälle

Die zusätzlichen Fälle werden ohne Verwendung von Freigabe, Apply-Erfolg, Ergebnisqualität, vorhandenen Nachzuständen oder günstigen Schlussbefunden ausgewählt. Aus jedem der vier Eingangswege wird höchstens je eine Operation aus zwei Gruppen gewählt:

| Auswahlgruppe | Protokolliertes Vorschlagsvokabular | Untersuchungszweck |
|---|---|---|
| Neues Anforderungs-Item vorgeschlagen | `NEW`, `NEW_RELATED` | Quellenübernahme und gegebenenfalls spätere Einordnung in Feature/PBI |
| Bezug auf vorhandenen Bestand vorgeschlagen | `REFINE`, `SUPERSEDE`, `CONTRADICT`, `RESTATE`, `ALREADY_DECIDED` | Angemessenheit des Bestandsbezugs und der vorgesehenen Zustandswirkung |

Die Gruppen sind nur Auswahlhilfen: Eine vorgeschlagene Neuanlage kann fachlich ein Duplikat sein; eine Bestätigung ist keine Inhaltsänderung. `OPEN_QUESTION` bleibt vollständig in der Inventur, ist aber kein eigenes zusätzliches Auswahlfach. Die bestehende Untersuchung offener Fragen bleibt erhalten; der bekannte Konfliktfall dient zusätzlich als Kontrolle für Entscheidungs-/Relationswirkung. Es wird keine Abdeckung aller acht Operationsarten beansprucht.

**Bekannte Belege:** Alle vollständigen Laufkennungen, die in den in `selection-rules.json` bezeichneten bisherigen Fall-/Prüfberichten vorkommen, werden vor Auswahl in einer bekannten Laufmenge eingefroren. Deren Operationen bleiben im Inventar, werden aber nicht als zusätzlich ausgewählte Fälle gezählt. Das ist keine Garantie, dass alle übrigen Projektfälle unbekannt sind; die gesamte Untersuchung bleibt entwicklungsbekannt.

**Reproduzierbare Reihenfolge:** Für jede verbleibende Operation wird SHA-256 über `p10-core-content-v1|<runId>|<zero-based operation index>|<incomingItemId>` berechnet. Die lexikografisch kleinste Kennung gewinnt je Auswahlfach. Abarbeitungsfolge: Transkript, Autor-Delta, GitHub, sonstiger Delta; jeweils Neuanlage vor Bestandsbezug. Ein bereits ausgewählter Lauf wird für weitere Fächer übersprungen; diese Ausschlüsse werden protokolliert. Bei einer leeren Gruppe bleibt der Platz leer. Es gibt keinen neuen Seed, keine Ersatzwahl nach Sichtung des Inhalts und keine Bevorzugung vollständiger oder erfolgreicher Läufe.

Vier **bekannte Kontrollfälle** werden getrennt geführt:

- F1: `765eea`, `REQ-01` → vorgeschlagene Verfeinerung von REQ-42, menschlich abgewiesen; bereits bekannter Inhaltsverlust im nicht übernommenen Ersatztext.
- F2: `f23f06`, `AF-1` → Verfeinerung von REQ-42 mit dokumentierter menschlicher PBI-Redaktion.
- F3: `eb181a`, `REQ-05` → Widerspruch zu REQ-32, Entscheidungsauflösung und begrenzte Folgeangleichung; interaktives und automatisches Gate-Verhalten trennen.
- GitHub-Besuchsfortschreibung: `802e63`, `GH-45` → REQ-89 und Erweiterung des Besuchsfalls; Quellenübernahme und PBI-/Projektionsbezug trennen.

Diese Kontrollen dienen der Konsistenz der Kriterien und dem Anschluss an vorhandene Befunde, nicht als vier neue Bestätigungen des Systems. Gleiche Eingaben, ähnliche Inhalte, gemeinsame Core-Stände und historische Experiment-Restores können zusätzliche Fälle abhängig machen. Abhängigkeiten und Stände werden pro Fall ausgewiesen; eine Repräsentativität oder statistische Unabhängigkeit wird nicht behauptet.

## 4. Bewertungsablauf und Kriterien

Die acht neuen Fälle werden in der festgelegten Reihenfolge bearbeitet. Zunächst werden Quelle bzw. Delta und der damalige Bestandskontext gelesen und die wesentlichen Bestandteile notiert: Akteur, Handlung, Objekt, Bedingungen, Verbindlichkeit, Zeitbezug und offene Festlegungen. Danach werden Vorschlag, Freigabe und gespeichertes Ergebnis verglichen. Diese Lesereihenfolge begrenzt Bestätigungsfehler; sie ist keine Verblindung gegenüber der bekannten Arbeit. Für historische Fälle mit fehlender Rohquelle kann nur die belegte Delta→Core-Teilkette beurteilt werden. Diese Einschränkung ist sichtbar festzuhalten.

| Kriterium | Konkrete Prüfung | Grenze |
|---|---|---|
| K1: Inhalt und Herkunft | Sind wesentliche Aussagen durch die bezeichnete Quelle getragen? Welche Inhalte stammen aus einer kenntlichen fachlichen Ableitung oder zusätzlichen menschlichen Festlegung? | Quellenwiedergabe, Entwurfsableitung und autorisierte neue Entscheidung sind verschiedene Aussagen. Eine sinnvolle Entwurfsentscheidung muss nicht wörtlich im Transkript stehen. Quellenverfälschung wird durch Freigabe nicht rückwirkend zur korrekten Quellenwiedergabe. |
| K2: Bestandsbezogene Einordnung | Ist die vorgeschlagene Operationsart anhand von Quelle und damaligem Zielbestand vertretbar? Passt das bezeichnete Ziel fachlich? Bleibt bei echter Unvereinbarkeit eine Entscheidung erforderlich? | Nicht jede Alternative ist ein Fehler. Eine plausible, begründete Auswahl kann bei mehreren zulässigen Zielen getragen sein. Ohne ausreichenden damaligen Bestand ist ein behauptetes „kein passendes Ziel vorhanden“ nicht voll prüfbar. |
| K3: Fachliche Erhaltung | Bleiben relevante Einschränkungen, bisheriger gültiger Inhalt und neuer Beitrag im Vorschlag sowie nach Übernahme erhalten? Bei Bestätigung: Bleibt der geltende Inhalt unverändert? | Archivierte alte Fassungen ersetzen nicht die Erhaltung weiterhin geltender Inhalte im aktuellen Text. Explizit autorisierte Bedeutungsänderungen sind getrennt von unbeabsichtigten Verlusten auszuweisen. |
| K4: Freigabe und gespeicherte Wirkung | Welche Auswahl/Redaktion ist tatsächlich dokumentiert? Welche Operation wurde angewandt, abgewiesen oder blieb ausstehend? Entspricht der nachgewiesene Zustand dieser Entscheidung? | UI-/Chat-Metadaten, maschinelle Annahmen im Experiment und heutige Autorenprüfung unterscheiden. Ohne Freigabe nicht pauschal eine unautorisierte Mutation behaupten; zunächst Protokollvollständigkeit prüfen. |
| K5: Beziehungen und Weiterverarbeitung | Welche tatsächlichen Verbindungen entstanden, und ist ihre Bedeutung durch Inhalt und Entscheidung gerechtfertigt? Soweit vorgesehen: Feature-/PBI-Ziel, Inhaltsangleichung und dokumentierte Projektion prüfen. | Ein Feature-Hinweis im Ingest ist noch keine gespeicherte Feature-Beziehung. Der Erzeugerschritt einer Kante ist kein Zeitstempel. Keine vollständige Aktualisierung aller abhängigen PBIs oder Live-Issues aus einem einzelnen Feldvergleich ableiten. |

Für K1–K3 werden Agentenvorschlag und nachgewiesener Übernahmezustand getrennt beurteilt. Eine abgewiesene Operation kann fachlich mangelhaft sein, während die Nichtübernahme den geltenden Inhalt schützt. Die Begründung des Menschen wird dokumentiert; ihre unbekannten Hintergründe werden nicht erfunden. Geprüft wird die erklärbare Zustandswirkung, nicht eine allgemeine Qualitätsquote menschlicher Entscheidungen.

**Belegregel:** Pro Urteil Quelle/Objektkennung, maßgeblicher Textausschnitt, Stand und Begründung festhalten. Ein behaupteter Nachzustand muss an einem eindeutig zugeordneten Snapshot, einer erhaltenen passenden Version oder einem inhaltlich ausreichenden gespeicherten Ergebnisobjekt belegt werden. Ein Apply-Zähler allein genügt nicht für Inhaltserhaltung. Spätere Snapshots nur mit belegbarer Zuordnung verwenden; eine aktuelle Core-ID allein verbindet keine historischen Zustände. `run-report.json` und `delta.json` können verschiedene protokollierte Umfänge besitzen; Unterschiede offen erklären, nicht den günstigeren auswählen. Aktueller Code erklärt heutige Regeln, beweist aber keinen unverändert ausgeführten historischen Codepfad.

Fehlende Artefakte, unterbrochene Ketten und abgelehnte Vorschläge bleiben Teil des ausgewählten Falls. Keine fehlenden historischen Zustände durch eine heutige Neuausführung ersetzen. Bei vollständigem Prüfplan kann auch eine Ausführungsgrenze ein Ergebnis sein; ohne ausreichenden Beleg bleibt die betreffende Inhaltsfrage offen.

## 5. Urteile, Autorenprüfung und Auswertung

Je Kriterium und geprüftem Zustand: **getragen**, **teilweise getragen**, **abweichend**, **mehrdeutig**, **nicht beurteilbar**, **nicht anwendbar** oder **nicht untersucht**. Teilurteile benennen die jeweils getragenen und offenen/verlorenen Bestandteile. Mehrdeutig bedeutet mehrere nach Quellenlage substantiiert mögliche Auslegungen; fehlende Dateien sind dagegen nicht beurteilbar. Nicht untersucht kennzeichnet eine bewusst ausgesparte Auswertung, nicht ein günstiges Ergebnis. Keiner der letzten vier Zustände zählt als bestätigte Erfüllung.

Für fachlich folgenreiche Auslegungsfragen wird der Autor zunächst mit Quelle, damaligem Bestand und Vorschlag bzw. Ergebnis konfrontiert, ohne vorgegebenes KI-Verdikt. Eine Antwort wird wörtlich samt Frage erhalten. Die Entwicklungsvorprägung und der Rollenverbund Autor/Entwickler bleiben eine Grenze. Bekannte frühere Antworten werden als frühere Prüfung gekennzeichnet, nicht neu gezählt. Ist eine Frage nicht beantwortbar, bleibt das Urteil entsprechend offen; fehlende unabhängige Prüfer werden nicht durch vermeintlich unabhängige KI-Runden ersetzt.

Berichtet werden eine kompakte Fallmatrix und begründete positive, negative oder unklare Befunde. Deskriptive Anzahlen sind nur mit Bezugsmenge, Auswahlgruppe, Kriterium, Zustand und Belegstatus zulässig; die vier Kontrollen stehen separat. Keine gepoolte Erfolgsquote, kein Gesamtqualitätswert, keine Behauptung allgemeiner Einordnungsgenauigkeit oder Fehlervermeidung durch HITL. Die Auswahl aus vorhandenen Operationen misst insbesondere **nicht**, ob alle relevanten Eingangsanforderungen erkannt wurden. Sie misst auch nicht Suchaufwand, Usability, Reparaturgewinn oder den isolierten Effekt von MAF.

Regeländerungen nach Beginn der Bewertung erhalten einen datierten Nachtrag mit Grund, betroffenen Fällen und Ergebnisfolge. Fehlgeschlagene oder unklare Fälle werden nicht nachträglich ausgetauscht. Ein neuer technischer Verdacht wird mit Beleg und erwarteter Wirkung in die bestehende Befundliste aufgenommen; eine bloße Prüffrage ist noch kein Fehler. Keine Codeänderung im Rahmen dieser Prüfung. Ein zentraler bestätigter Funktionsfehler wird vor einer Fixentscheidung mit dem Autor besprochen.

## 6. Abschluss, Textfolge und Abbruchgrenze

Dieser Vorbereitungsschritt ist abgeschlossen, wenn Inventar, Auswahlregeln, konkrete Fallkennungen, bekannte Kontrollen, leere Auswahlfächer, benötigte Belegarten und noch unbewertete Ergebnisfelder gesichert sind. Die spätere Fallprüfung endet nach diesen ausgewählten Fällen einschließlich ausgewiesener Nachweislücken; neue interessante Pfade erweitern sie nicht automatisch.

Bei substanziell neuem Erkenntniswert folgt ein kurzer, datierter Anschluss an 7.5 und die Ergebnis-/Grenzendarstellung in 7.7/7.8 sowie gegebenenfalls 8.1. Verfahren und Einzelfälle gehören in Anhang/Belegpaket. Kapitel 3 erfordert keinen Umbau; nur eine tatsächlich neue, dort nicht gedeckte Verfahrensart würde eine Präzisierung begründen. Die bestehenden F1–F5, Herkunftsaudit-v4-Zahlen und historischen Bewertungsdateien werden nicht still überschrieben. Auch ein ernüchterndes Ergebnis verbessert die Belegqualität, ohne damit die Leistungsbewertung des Systems automatisch zu erhöhen.

**Keine neue Systemdokumentation:** Im Haupttext sollen Problem, Aufgabenverteilung und beobachtete Wirkung verständlich werden. Klassennamen, alle Felder und sämtliche internen Pfade werden nicht zusätzlich erklärt. Der Gesamtplan bleibt bestehen: gezielter Textnachzug, Beleg-/Materialabschluss, Grafik/Satz, Gesamtabnahme, Repository-Dokumentation und zuletzt der reale Artefakt-Freeze.
