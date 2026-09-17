# Bewertungsprotokoll: agentische Planungsarbeit und vollständige Arbeitspakete

Version 1.0 · 16.09.2026 · vor den zusätzlichen Qualitätsurteilen festgelegt. Retrospektive Bewertung bekannter historischer Ausgaben; keine Vorregistrierung der damaligen Läufe. Der globale Messrahmen bleibt der [Evaluationsplan, §6](../../../Thesis-Docs/Writing/claude-writing/P10-Evaluationsplan-A1-A10-und-Agentenbeitrag-2026-09-16.md). Die [A1–A10-Anspruchsmatrix](../../../Thesis-Docs/Writing/claude-writing/P10-Anspruchsmatrix-und-Bewertungsfreigabe-2026-09-16.md) begründet, welche vorhandenen Nachweise erhalten bleiben und welchen Zusatz dieses Paket liefert.

## 1. Fragestellung, Auswahl und Einheiten

Untersucht wird die fachliche Tragfähigkeit bestandsbezogener agentischer Vorschläge, die Angemessenheit ihrer Filterung und die Qualität der daraus fortgeschriebenen Planungsartefakte. Das untersuchte System umfasst Modell, Eingaben, Werkzeuge, Workflow und menschliche Entscheidungen. Eine Wirkung wird keinem Einzelmechanismus kausal zugeschrieben.

Die Auswahl steht fest: Analyst-Lauf `20260821_203951_db6050`, alle 17 Berichtseinträge einschließlich vier vom Kritiker ausgesonderter Vorschläge; Folgelauf `20260821_204808_042823`, seine drei aktualisierten PBIs 003/032/046 und die gekoppelten Issues #3/#32/#44. Der spätere archivierte Rückleselauf ist `20260821_211304_bfbbd7`. Der Steward-Aufruf ist in `20260821_203903_d71697` erhalten. Auswahlgrund ist die zusammenhängende Belegkette, keine vorab festgestellte gute Qualität. PBI-046 vertieft den bereits berichteten Fall D.12/N07 und erhöht die Zahl unabhängiger Fälle nicht.

Einheiten: ein Vorschlag, ein Kriterium, ein gekoppeltes PBI/Issue, ein definierter Zustandsübergang. 17 Vorschläge sind keine 17 Systemwiederholungen; drei PBIs und ihre drei Issues sind drei Paare, nicht sechs unabhängige Artefakterzeugungen. Einzelne Werkzeugaufrufe sind Handlungsbelege, keine erfolgreichen Aufgaben.

## 2. Material und Messpunkte

- **S0:** `07-ingest/applied/core-before.json` des Folgelaufs, 230 Items/509 Relationen. Sein projektseitiger Fingerabdruck `cc2a5f4d357982d6` stimmt mit dem Analyst-Bericht überein. Geprüft werden dafür Items und Relationstripel. Der Fingerabdruck umfasst weder die gesamte Core-Datei noch Personas, Prompts oder Laufkonfiguration. Dies stützt die Zuordnung des fachlichen Ausgangsbestands, nicht die vollständige Rekonstruktion der Sitzung.
- **Vorschlag und Filter:** Berichtseintrag, gespeicherte Herleitung, Anker und Kritikerentscheidung getrennt. AN-01 bis AN-17 bezeichnen unveränderlich die Reihenfolge im Bericht, keine neuen System-IDs.
- **Einreichung und Autorisierung:** Auswahl-Delta, Anforderungs-/Architekturplan, jeweilige Gate-Antwort und angewandter Zustand. Die Auswahl von vier der 13 behaltenen Vorschläge ist kein Qualitätsurteil über die neun übrigen. Gate-Annahme ist ebenfalls keine externe Qualitätsbewertung.
- **PBI-Vorstand:** `07-pbi-update/applied/core-before.json`, nach Einordnung der neuen Inhalte, vor der Arbeitspaketfortschreibung.
- **PBI-Vorschlag:** die drei vollständigen Alignments in `pbi-change-plan.json`; Human-Antwort in `human-decisions.json`.
- **PBI-Nachstand:** `07-github/applied/core-before.json`, also vor der Außenprojektion und nach der PBI-Anwendung.
- **Außensicht vorher:** `07-github/github-snapshot.json` des Folgelaufs; laut Ereignisfolge vor dem Forward eingelesen. Er enthält die noch nicht fortgeschriebenen Issues.
- **Außensicht nachher:** tatsächlicher Anwendungsbericht sowie vollständige Titel/Bodies im späteren archivierten GitHub-Snapshot. Plan-Entsprechung wird getrennt von fachlicher Güte bewertet.

Alle Originale liegen byteidentisch unter `originals/`; `material-manifest.json` enthält Herkunft, Größe und SHA-256. `dossiers/` enthält Gegenüberstellungen und Belegauszüge. Das vollständige JSON-Artefakt bleibt maßgeblich, wenn ein Lesedossier verkürzt ist.

**Rekonstruktionsgrenzen:** Historischer Analyst-Modellname, Temperatur und exakter damaliger Promptstand sind in den ausgewerteten Laufdaten nicht gesichert. Ein Modellfeld des Folgelaufs wird dem Analysten nicht zugeschrieben. Mitkopierter Code/Prompt ist der heutige Lesestand und erklärt einen Mechanismus, beweist aber keine damalige Ausführungsidentität. Werkzeugenden enthalten `<JsonElement>` statt Rückgabeinhalten. Parallele Linsen teilen Agentennamen und mehrfach verwendete Schrittzähler. Deshalb werden keine lückenlosen per-Linse-Verläufe oder kausalen Informationsnutzungen aus Reihenfolge/Anzahl konstruiert. Kontext aus S0 wird als heutige Rekonstruktion gekennzeichnet. Eine personenbezogene Persona-Validierung ist nicht Gegenstand; die fachlichen Anker des Persona-Vorschlags werden wie alle übrigen bewertet.

Diese Grenzen verhindern die vorgesehenen bestands- und ausgabebezogenen Inhaltsurteile nicht; sie schließen genaue historische Modellzuordnung und vollständige Prozessrekonstruktion aus. Wird für ein konkretes Urteil weiterer Kontext benötigt, bleibt es NB, bis dieser Kontext belegt ist.

## 3. Bewertungsmaßstab und Literaturbezug

Das Raster ist eine eigene, begründete Operationalisierung für diese Entwicklungsartefakte, keine validierte Gesamtskala und keine Normkonformitätsprüfung. Die Auswahl von Richtigkeit, ausreichender Beschreibung, Verständlichkeit, Konsistenz und Überprüfbarkeit lehnt sich an ISO/IEC/IEEE 29148:2018, §5.2.5–5.2.6 (S. 12–13), an. Die Norm unterscheidet Einzelanforderung und Anforderungsmenge; hier wird Vollständigkeit ausdrücklich nur für den festgelegten Umfang des jeweiligen Arbeitspakets beurteilt. [Lokales Original](../../../Thesis-Docs/Writing/quellen/29148-2018-Systems%20and%20software%20engineering%20%E2%80%94%20Life%20cycle%20processes%20%E2%80%94%20Requirements%20engineering.pdf).

Für User Stories stützt QUS insbesondere die Betrachtung von verständlicher Bedeutung, erkennbarem Umfang und Widerspruchsfreiheit. Das dortige Gesamtkriterium einer vollständigen Anwendung wird nicht auf drei PBIs übertragen. Ebenso werden Schätzbarkeit und Unabhängigkeit ohne Teamkontext nicht behauptet. [Lucassen et al., Abschnitt 3/Tabelle 1](https://webspace.science.uu.nl/~dalpi001/papers/luca-dalp-werf-brin-16-rej.pdf).

Die Trennung von Werkzeughandlung, Zwischenzuständen und fachlichem Aufgabenergebnis hat einen Literaturanschluss bei [ToolSandbox](https://aclanthology.org/2025.findings-naacl.65/). Es wird weder dieser Benchmark ausgeführt noch sein Score übernommen. Historische Belege und KI-Unterstützung sind offenzulegen.

## 4. Einheitliche Urteilsregeln

- **E – erfüllt:** Die unter dem Kriterium benannten Bedingungen sind im betrachteten Umfang belegt; kein entgegenstehender Befund.
- **T – teilweise:** Mindestens ein konkreter Teil ist erfüllt und mindestens ein anderer verletzt. Beide Teile und ihre Belege werden angegeben. Kein rechnerischer halber Punkt.
- **N – nicht erfüllt:** Eine zentrale Bedingung ist nachweislich verletzt. Eine einzige bedeutungsändernde Gegenstelle kann hierfür genügen; die Anzahl wohlformulierter Sätze gleicht sie nicht aus.
- **NB – nicht beurteilbar:** Fehlender Kontext oder nicht auflösbare Mehrdeutigkeit verhindert ein Urteil. NB wird mitgezählt und begründet, nicht als Erfolg oder Fehlfunktion gewertet.
- **NA – nicht anwendbar:** Das Kriterium passt zum betrachteten Objekt nicht. Leere Akzeptanzkriterien eines zur Umsetzung gedachten PBI sind keine NA, sondern ein bewertbarer Mangel. Für eine reine offene Frage wird hingegen keine fertige Lösung verlangt.

Aussagen werden innerhalb ihrer ursprünglichen Geltung gelesen: Hypothese, offene Frage, verbindliche Anforderung und bereits bestehende Festlegung sind verschieden. Ein neuer plausibler Entwurf braucht nicht wörtlich im Transkript zu stehen. Er braucht einen nachvollziehbaren Anlass, eine korrekte Wiedergabe seiner Ausgangsfakten und die Kennzeichnung als Vorschlag. Weder konkrete Schwellenwerte noch Rechtsbehauptungen werden vom Bewertenden frei erfunden.

Zuerst Quelle/Aufgabe/Altbestand lesen und die erforderlichen Inhalte festhalten, dann Vorschlag beziehungsweise Nachzustand prüfen. Da die Ausgaben bereits zugänglich waren, wird damit keine Verblindung behauptet. Neue Anforderungen dürfen gültige Altinhalte nur ablösen, wenn dies aus Eingang oder dokumentierter Entscheidung hervorgeht. Reine Annahme einer Vorlage beweist keinen bewusst beabsichtigten Wegfall sämtlicher nicht wiederholter Altinhalte. Ein Konflikt im Altbestand wird als vorbestehender Konflikt berichtet.

## 5. Analyst-Rubrik – vier getrennte Urteile je Vorschlag

| ID | Gegenstand | Beobachtbare Bedingungen |
|---|---|---|
| K1 | Projektrelevanz | Benennt einen konkreten betroffenen Akteur, Vorgang, Datenbestand oder technischen Bezug dieses Projekts und erläutert die Konsequenz beziehungsweise zu klärende Frage. Eine beliebige Standardforderung mit angehängter Kennung genügt nicht. |
| K2 | Tragfähigkeit der Bestandsanker | Die Anker sind auflösbar und enthalten die behaupteten Ausgangstatsachen im richtigen Status/Geltungsbereich. Alle tragenden Tatsachenbehauptungen in der Herleitung werden gegen ihren benannten Bestand geprüft. Existenz der Kennung allein ist kein semantischer Erfolg. |
| K3 | Nachvollziehbare Ableitung | Zwischen Bestandswissen und vorgeschlagenem Ergänzungs-/Klärungsbedarf besteht ein erklärbarer Zusammenhang. Zusatzannahmen bleiben als solche erkennbar; der Vorschlag wird nicht als bereits erhobener Stakeholderwunsch ausgegeben. Behauptete Lücken werden gegen thematisch relevante Gegenstellen geprüft. |
| K4 | Konkrete Bearbeitbarkeit | Das Team kann erkennen, was es entscheiden, klären oder ergänzen soll. Betroffener Gegenstand und Handlungs-/Klärungspunkt sind bestimmbar. Eine präzise offene Frage darf bearbeitbar sein, ohne bereits ihre Antwort oder einen Zahlenwert zu enthalten. Mehrere zusammengehörige Bedingungen dürfen einen Vorgang beschreiben; unverbundene Themenbündel werden markiert. |

**Neuheit separat (NOV):** zusätzlich / teilweise bereits vorhanden / bereits abgedeckt / NB. Vor einem solchen Urteil relevante Volltexte, Relationen und offene/abgelöste Entscheidungen in S0 durchsuchen; Suchbegriffe und IDs dokumentieren. Neuheit betrifft den identifizierten Bestand, nicht wissenschaftliche Neuheit. Die bloße Behauptung des Agenten „fehlt“ reicht nicht. Es gibt keinen Recall über alle denkbaren Produktideen.

**Kritiker separat (CRIT):** Zuerst K1–K4 und NOV festhalten, danach historischen Filterstatus vergleichen. Zu beurteilen sind die sachliche Tragfähigkeit des Ablehnungsgrunds und die Angemessenheit der Entscheidung unter den im Lauf dokumentierten Filtergründen. Der heutige Prompt wird nicht als gesicherter damaliger Auftrag vorausgesetzt; hängt ein Urteil von einer nicht archivierten Instruktion ab, bleibt dieser Teil NB. Eine fehlende fertige Antwort entwertet nicht automatisch eine ausdrücklich erlaubte Klärungsfrage. Die Aufbewahrung im Bericht macht eine Aussortierung nicht folgenlos: Sie entfernt den Vorschlag aus dem automatisch angebotenen Delta, löscht ihn aber nicht.

Berichtet werden die vier ausgesonderten Fälle einzeln, einschließlich möglicher sinnvoller Anteile; ebenso bleiben Mängel der 13 behaltenen sichtbar. Keine binäre Verwechslungs- oder Präzisionsmatrix ohne zusätzliche, ausdrücklich dokumentierte Referenzentscheidung „vorlagefähig“. Insbesondere werden K1–K4 nicht nachträglich zu einer willkürlichen Gesamtnote verrechnet.

## 6. PBI-/Issue-Rubrik – Vor- und Nachzustand

| ID | Gegenstand | Beobachtbare Bedingungen |
|---|---|---|
| B1 | Fachliche Richtigkeit und Status | Titel, Ziel, Akzeptanzkriterien und eingeblendete Architekturhinweise widersprechen den zugehörigen geltenden Referenzinhalten nicht; Modalität, Rolle, Geltungsbereich und Herkunft einer neuen Festlegung sind korrekt. Freigabe und fachliche Richtigkeit getrennt. |
| B2 | Erforderlicher Inhalt und Erhaltung | Pflichtinhalte der neuen Ergänzung und weiterhin geltende Altinhalte bleiben für das Arbeitspaket erkennbar. Neu ergänzte, erhaltene, fehlende und ausdrücklich abgelöste Teilinhalte getrennt. Ein Verweis auf eine Anforderung ist eine verfügbare Nachschlagemöglichkeit, ersetzt aber nicht automatisch einen verschwundenen zentralen Akzeptanzpunkt. |
| B3 | Verständlichkeit und Umfang | Aufgabe, betroffener Akteur/Gegenstand und erwartetes Ergebnis lassen sich aus dem Artefakt und seinen expliziten Bezügen bestimmen. Fachwörter oder Mehrdeutigkeiten werden konkret benannt. Keine allgemeine Nutzerverständlichkeit ohne Leseruntersuchung beanspruchen. |
| B4 | Überprüfbarkeit der Akzeptanzkriterien | Für jedes Kriterium lässt sich eine beobachtbare Bedingung und eine unterscheidbare Erfüllung/Nichterfüllung angeben. Auch ein Planungs-PBI kann durch eine konkrete Entscheidung oder ein beschriebenes Dokument abnehmbar sein. Nicht jede Bedingung benötigt Zahlen. Bloße unbestimmte Qualitätsversprechen oder fehlende Kriterien werden ausgewiesen. |
| B5 | Konsistenz und Verbindungen | Verknüpfte Anforderungen/Features passen fachlich; explizit noch geltende Vorgaben werden nicht widersprüchlich. Erwartete Verbindungen sind vorhanden. Vorbestehende Konflikte, neue Konflikte und bloß offene Klärung werden getrennt. Ein Relationstripel allein belegt keine inhaltliche Deckung. |
| B6 | Herkunft und Außendarstellung | Änderungsanlass, Vorschlag, Freigabe und gespeicherte Wirkung sind zugeordnet; im Issue stimmen die ausgewählten maschinell gepflegten Inhalte mit dem freigegebenen PBI überein. B6a Herkunft und B6b Projektion getrennt begründen. Gute Projektionstreue gleicht Mängel bei B1–B5 nicht aus. |

Die Vorherbewertung bezieht sich auf den damals bereits geltenden Umfang; das Fehlen der erst jetzt eingereichten Ergänzung ist im alten Artefakt kein Fehler. Der Zustand vor dem PBI-Update enthält bereits neue Requirements und die verfeinerte Architektur. Für frühere Geltung und Architekturtexte wird deshalb zusätzlich S0 herangezogen.

Für B2 wird pro PBI vor dem Qualitätsurteil eine nummerierte Soll-Inhaltsliste aus Eingang, relevanten Requirements und Altbestand erstellt. Jede Zeile erhält Herkunft/Status und später erhalten/teilweise/fehlend/absichtlich abgelöst/NB. Neue und alte Inhalte erhalten getrennte Nenner. B6a und B6b erhalten eigene Unterurteile. Für B4 werden alle tatsächlich vorhandenen Akzeptanzkriterien einzeln geprüft; ihre Zahl ist keine Qualitätsnote. Relationale Nachschlagbarkeit und Eigenständigkeit des Issue-Texts werden nebeneinander festgehalten.

Alle drei Paare werden nach demselben Raster geprüft, einschließlich der vollständigen technischen Rahmenbedingungen im Issue. Der Untersuchungsumfang ist nicht auf die neu hinzugekommenen Sätze oder den günstigsten Besuchsfall begrenzt. Fehlender Altinhalt im schon vorher unvollständigen PBI wird von zusätzlichem Verlust unterschieden.

## 7. Agentische Handlung und kontrollierte Wirkung

Für den ausgewählten Fall gelten die folgenden Prüfpunkte. Das ist eine festgelegte Prozessprüfung, kein neues Zufallsexperiment:

| Punkt | Maßstab und Beleg |
|---|---|
| H1 Informationsbeschaffung | Tatsächliche `search_core`-/`get_core_item`-Aufrufe mit Argumenten und technischen Endmeldungen nachweisen. Suche aufgabengerecht begründen; keine internen Denkprozesse oder fehlenden Rückgaben ergänzen. Direkte Einzelabrufe sind nicht für jede Kennung verpflichtend, weil auch der Digest Kontext bereitstellt. |
| H2 Vorschlagseinreichung | Gespeicherte `save_findings`-Inhalte den 17 Berichtseinträgen zuordnen. Fünf Linsen sind Perspektiven eines Laufs, keine fünf unabhängigen Modellläufe. |
| H3 Filterung und Auswahl | 17 Berichtseinträge → 13 nach Kritiker → vier ausgewählte Einträge getrennt beschreiben. Nichtgewählte Einträge sind nicht automatisch menschlich abgelehnt. Das Auswahl-Delta muss die gewählten Vorschläge korrekt übernehmen. |
| H4 Einordnung und Freigabe | Gewählte Vorschläge → Eingangsoperationen → Gate-Antwort → gespeicherter Inhalt zuordnen. Die getrennten Schritte ARCH-11-Verfeinerung, REQ-91/92-Neuzugänge und offene Entscheidung nicht als identische Operationen behandeln. |
| H5 Arbeitspaketfortschreibung | Für alle drei PBIs Agentenplan, menschliche Antwort und tatsächlichen Vorher-/Nachher-Zustand vergleichen. Felder mit `edited` im Namen beweisen keine menschliche Textbearbeitung. |
| H6 Projektion | Tatsächliche drei Updates und spätere archivierte Rücklesung zuordnen. Keine erneuten GitHub-Schreibaktionen und kein heutiger Außenstand für diese historische Frage erforderlich. |

H1–H6 werden als belegt / teilweise belegt / widersprochen / nicht beurteilbar mit Fundstelle berichtet. Der Umfang der tatsächlichen Steward-Anweisung wird nur bewertet, soweit dessen ursprünglicher Wortlaut erhalten ist. Ein Tool-Aufruf allein belegt keinen selbständig gewählten Weg gegenüber einer möglichen expliziten Benutzeranweisung. Bei der historischen Sitzung ist kein vollständiger Benutzerwortlaut gesichert; deshalb ist sie hier ein Ausführungs- und Integrationsbeleg, kein neuer Test freier Steward-Werkzeugwahl.

## 8. Auswertung, Urteilsrollen und Wiederholungen

Bericht je Kriterium: Anzahl E/T/N/NB/NA bei vollständigem Nenner 17 beziehungsweise drei Paaren je Messpunkt. Vorher/Nachher sind gepaart und abhängig. Fehlende Bewertungen bleiben als offen sichtbar. Keine Konfidenzintervalle, Signifikanztests, repräsentativen Erfolgsquoten oder Verrechnung von Kriterien, Aufrufen und Ausführungen. Veränderungen innerhalb des Falls sind keine isolierten kausalen Architekturvorteile.

KI bereitet Belege und zunächst gekennzeichnete Ersturteile vor. Der Autor prüft die semantischen Urteile anhand der Gegenüberstellungen; Umfang, Datum und Korrekturen werden je Fall erfasst. Wenn nicht alle Fälle menschlich gelesen werden, darf kein menschlicher Vollprüfungsstatus berichtet werden. Historische Gate-Freigaben zählen nicht als diese neue Bewertung. Eine zweite KI-Lektüre oder die Autorenprüfung durch den Entwickler ist keine unabhängige Zweitannotation. Optionaler Plan B bleibt getrennt.

**Wiederholungsentscheidung:** Für diese retrospektive Beschreibung der ausgewählten Ausgaben werden keine neuen Analyst-/Steward-Läufe benötigt. Eine Zahl „x von 17 in diesem Lauf“ begründet keine Wiederholungspflicht. Bei einem zusätzlich gewünschten Stabilitätsanspruch müsste die Untersuchung vorab um gleiche Ausgangsstände, feste Konfiguration, vollständige Bewertung aller Wiederholungen und geeignete Berichterstattung erweitert werden. Drei Läufe wären keine allgemeine Zuverlässigkeitsgarantie. Die Messbereiche der übrigen Evaluation behalten ihre eigenen Nenner und Wiederholungsentscheidungen aus der Anspruchsmatrix.

## 9. Abschluss, Zeit und Abweichungen

Endlicher Umfang: 17 Vorschläge, vier Kritiker-Ablehnungen sowie drei ganze gekoppelte Arbeitspakete mit Vorher/Nachher; H1–H6 und die zehn Anforderungsbilanzen. Kein umfassender Quellen-/Domänenvergleich, keine gesamte neue E2E-Kampagne und kein Refactoring. Die vier neuen Steward-Aufgaben und die optionale Analyst-Wiederholungsserie werden für diese Auswertung zunächst nicht gestartet. Erst ein konkret nicht beantwortbarer zentraler Anspruch kann eine gezielte zusätzliche Ausführung begründen; diese benötigt ein eigenes vor Ausführung festgehaltenes Fallblatt und genug Zeit für Bewertung und Textintegration.

Technische Vorbereitung abgeschlossen heißt nicht, dass die Autorenbewertung zeitlich bereits zugesichert oder erledigt ist. Aufwand für die erste Bewertungscharge wird erfasst; vor einer zusätzlichen Laufentscheidung bleiben Textintegration, Abgleich Kapitel 3–8 und PDF-Abnahme als notwendige Arbeiten reserviert. Reicht die Zeit für die menschliche Vollprüfung nicht, wird ihr tatsächlicher Teilumfang offengelegt. Kein zusätzliches unbewertetes Laufmaterial als Fortschritt verbuchen.

Die Analyse ist erfolgreich abgeschlossen, wenn alle ausgewählten Fälle mit Gründen bilanziert sind und ihre Ergebnisse die Forschungsfrage innerhalb der belegten Reichweite beantworten. Positive Ergebnisse sind keine Abschlussbedingung. Ein bedeutsamer Inhaltsverlust oder eine problematische Filterentscheidung wird berichtet und nicht durch Fallwechsel entfernt. Code-/Promptänderungen werden separat entschieden, nicht in diese historische Bewertung eingeschoben.

Änderungen an Rubrik oder Umfang nach Beginn der Urteile erhalten Datum, Anlass, betroffene Fälle und erneute konsistente Anwendung; Version 1.0 bleibt erhalten. Ergebnisintegration vorgesehen in 7.5/7.6, knappes Verfahren in 7.2, Bilanz/Grenzen in 7.7/7.8 und ausführliche Fallblätter im Belegpaket. Kein neues Kapitel zur Systemdokumentation.
