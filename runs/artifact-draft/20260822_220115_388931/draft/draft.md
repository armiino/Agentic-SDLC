# User Story Map

_Die Reise ist Deutung des Zeichners (vom Autor freigegeben); Kaertchen, Prio und Stand kommen aus der Projektwahrheit._

```mermaid
flowchart LR
  A[Anmelden] --> B[Orientieren] --> C[Bewohner finden] --> D[Profil verstehen] --> E[About Me] --> F[Kommunikation klären] --> G[No-Gos prüfen] --> H[Schicht sichern]
  classDef schritt fill:#438dd5,color:#fff
  class A,B,C,D,E,F,G,H schritt
```

## Anmelden
Ich melde mich auf dem Diensthandy mit meinem Account an, weil ich zu Beginn meiner Schicht schnell in die App kommen und nur die Bewohner meiner Einrichtung sehen will.

## Orientieren
Ich orientiere mich nach dem Login über die konsistente Navigation, Hilfen und sichtbare Hinweise, damit ich mich auch unter Zeitdruck sicher durch die App bewege.

## Bewohner finden
Ich starte in der Profilübersicht und suche Bewohner schnell nach Namen oder Stichworten, damit ich im Alltag ohne langes Blättern das passende Profil finde.

## Profil verstehen
Ich öffne das Bewohnerprofil und verschaffe mir über Profilbild, Sofortinfos und Hauptbereiche rasch einen ersten arbeitsrelevanten Überblick.

## About Me
Ich nutze die About-Me-Seite für einen ersten persönlichen Eindruck und ergänze bei Bedarf neue Bilder in der Timeline.

## Kommunikation klären
Ich sehe verbale und nonverbale Kommunikation klar getrennt und ergänze durchsuchbares Kommunikationswissen mit Text, Bildern und Videos, damit ich den Bewohner besser verstehen kann.

## No-Gos prüfen
Ich prüfe bewohnerbezogene No-Go-Hinweise und pflege sie kompakt nach, damit ich problematische Themen oder Handlungen in Gegenwart des Bewohners vermeide.

## Schicht sichern
Ich erhalte Übergabe-Notizen, dokumentiere bzw. prüfe medikamentenbezogene Informationen und behalte angekündigte Besuche im Blick, damit meine Schicht verlässlich übergeben und abgestimmt bleibt.

## MVP-Vorschlag

**Nutzen für die Kernpersona**  
Für das MVP sind aus Sicht von Sandra vor allem die Schritte **Anmelden**, **Orientieren**, **Bewohner finden**, **Profil verstehen**, **About Me**, **Kommunikation klären** und **No-Gos prüfen** nötig. Diese Kette deckt den belegten Kernnutzen ab: schnell einloggen, den richtigen Bewohner finden und bei Verständnisschwierigkeiten sofort auf Profil-, About-Me-, Kommunikations- und Warnwissen zugreifen. In **Schicht sichern** würde ich im MVP nur die **Übergabe-Notiz** mit prominenter Anzeige und bewohnerbezogener Verlinkung vorsehen, weil sie den direkten Schichtalltag der Kernpersona stützt; Besuchskoordination, Angehörigenansichten, Medikationsübersichten für Angehörige, Dunkelmodus und spätere Accessibility-Erweiterungen bleiben zunächst außerhalb.

**Risiko**  
Das größte Risiko im MVP liegt nicht in den sichtbaren Screens, sondern in den Rahmenbedingungen: Einrichtungsgrenzen und Rollen müssen tragfähig sein, Zugriffe müssen revisionssicher protokolliert werden, und Bildnutzung braucht geklärte Einwilligungen. Deshalb gehören die zugehörigen Basis-PBIs trotz geringer Sichtbarkeit in den MVP-Unterbau. Dagegen erhöhen Offline-first, tiefe Dokumentationsintegration, Videos, Angehörigenbeiträge und Besuchsprozesse die Komplexität deutlich und können ehrlich später folgen.

**Weglass-Test**  
Ohne **Anmelden** und Rechte-/Einrichtungsgrenzen verliert der Auftraggeber die kontrollierte, sichere Nutzung im Einrichtungsrahmen. Ohne **Bewohner finden** verliert der Auftraggeber den schnellen Alltagseinstieg statt schwer nutzbarer Akten. Ohne **Profil verstehen**, **About Me**, **Kommunikation klären** und **No-Gos prüfen** verliert der Auftraggeber den eigentlichen fachlichen Kern, nämlich Wissen über Person und Kommunikationsweise für bessere Verständigung im Alltag. Ohne **Übergabe-Notiz** in **Schicht sichern** verliert der Auftraggeber einen unmittelbar belegten Nutzen für den Schichtwechsel. Ausgeschlossen aus dem MVP sind daher vorerst Profilanlage, Besuchskoordination, Angehörigen-Foto-Uploads, Medikamenten-Monatsübersicht für Angehörige, Dunkelmodus, Schriftgrößenwunsch, Tablet-/Alternativeingaben sowie offene Forschungs- und Klärungsbedarfe, soweit sie nicht zur Absicherung des MVP-Kerns zwingend sind.

## Story-Map-Tafel

> Diese Tafel wird DETERMINISTISCH aus Zuordnung + Projektwahrheit gerendert (Story-Satz,
> Prio und ✓-Stand kommen live aus dem Core) — nicht von Hand und nicht vom Agenten pflegen.
<!-- storymap-zuordnung: {"personasVersion":2,"schritte":[{"key":"login","titel":"Anmelden","pbiIds":["PBI-005","PBI-002","PBI-003","PBI-025","PBI-036","PBI-037"]},{"key":"orient","titel":"Orientieren","pbiIds":["PBI-007","PBI-008","PBI-006","PBI-034","PBI-024","PBI-030"]},{"key":"find-resident","titel":"Bewohner finden","pbiIds":["PBI-009","PBI-031","PBI-011","PBI-010"]},{"key":"understand-profile","titel":"Profil verstehen","pbiIds":["PBI-012"]},{"key":"about-me","titel":"About Me","pbiIds":["PBI-013","PBI-035","PBI-014","PBI-015","PBI-016","PBI-017","PBI-044"]},{"key":"communication","titel":"Kommunikation klären","pbiIds":["PBI-018","PBI-019","PBI-020","PBI-021","PBI-022","PBI-004"]},{"key":"no-gos","titel":"No-Gos prüfen","pbiIds":["PBI-023"]},{"key":"shift-support","titel":"Schicht sichern","pbiIds":["PBI-032","PBI-033","PBI-045","PBI-027","PBI-028","PBI-047","PBI-048","PBI-046","PBI-049"]}]} -->

### Anmelden

| PBI | Story | Prio | Stand |
| --- | --- | --- | --- |
| PBI-005 | Als Nutzer will ich mich über einen klaren Login-Screen mit E-Mail und Passwort anmelden und als Angehöriger bei Bedarf mit einem Einladungscode der Einrichtung selbst registrieren können, wobei die Einrichtungszuordnung bei der Registrierung unveränderlich mitgegeben wird, damit ich sicher in die App einsteigen kann, die Selbstregistrierung nur im vorgesehenen Fall möglich ist und Accounts sowie Bewohnerprofile genau einer Einrichtung zugeordnet sind. | — |  |
| PBI-002 | Als Admin will ich interne Accounts mit Basisrollen verwalten und Angehörigen die Selbstregistrierung per Einladungscode der Einrichtung mit unveränderlicher Einrichtungszuordnung ermöglichen, damit für Admin, User und Bewohner der Zugang weiter nur über intern vergebene Accounts erfolgt, Angehörige sich im vorgegebenen Rahmen selbst registrieren können und jeder Account sowie jedes Bewohnerprofil genau einer Einrichtung zugeordnet ist. | — |  |
| PBI-003 | Als Product Owner will ich das Rechtemodell für Einrichtungs-Personal, Angehörige und Leitung sowie einen serverseitigen Suchrahmen für die einrichtungsgebundene Stichwortsuche über alle Bewohnerprofile festlegen, damit Zugriffe und Suchtreffer die Einrichtungsgrenze fachlich korrekt und technisch nicht umgehen. | — |  |
| PBI-025 | Als Nutzer will ich die App auf iPhone und Android in gut lesbarer, nicht ablenkender Form verwenden können, damit sie im Alltag zuverlässig und zugänglich einsetzbar ist. | — |  |
| PBI-036 | Für die Planung soll aktuell von einer Mindestunterstützung ab Android 10 ausgegangen werd... — ⚠ in Klärung | — |  |
| PBI-037 | Falls im Haus Geräte unter Android 10 vorhanden sind, besteht ein Risiko durch nötigen Ger... — ⚠ in Klärung | — |  |

### Orientieren

| PBI | Story | Prio | Stand |
| --- | --- | --- | --- |
| PBI-007 | Als eingeloggter Nutzer will ich auf jeder Seite eine konsistente Appbar mit Orientierung und Grundaktionen haben, damit ich mich sicher durch die App bewegen kann. | — |  |
| PBI-008 | Als neuer Nutzer will ich beim ersten Login eine kurze Einführung und später einen wiederaufrufbaren Hilfebereich nutzen, damit ich die App ohne lange Einarbeitung bedienen kann. | — |  |
| PBI-006 | Als Nutzer will ich nach dem Login meinen zuletzt genutzten Bewohner oder meine zuletzt geöffnete Einrichtung schneller wiederfinden, damit wiederkehrende Arbeitsschritte weniger Zeit kosten. | — |  |
| PBI-034 | Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließl... — ⚠ in Klärung | — |  |
| PBI-024 | Als Product Owner will ich das Offline-first-Verhalten mit lokalem Cache in SQLite, die Verschlüsselung lokal zwischengespeicherter Bewohnerdaten und Medien sowie das automatische Synchronisations- und Konfliktverhalten festlegen, damit Erfassungen lokal gespeichert, im Ruhezustand geschützt, bei bestehender Verbindung automatisch synchronisiert und nach Abmeldung oder Berechtigungsentzug im jeweiligen Kontext lokal nicht mehr lesbar sind. | — |  |
| PBI-030 | Als Product Owner will ich Flutter für die App, Riverpod für das State-Management und SQLite als lokale Datenbank verbindlich festlegen, damit die Umsetzung die Offline-first-Anforderung unterstützt und auf vorhandener Flutter-Erfahrung im Team aufbaut. | — |  |

### Bewohner finden

| PBI | Story | Prio | Stand |
| --- | --- | --- | --- |
| PBI-009 | Als Nutzer will ich in einer Profilübersicht berechtigte Bewohnerprofile sehen, nach Namen suchen und ein Profil öffnen, damit ich relevante Informationen schnell finde. | — |  |
| PBI-031 | Die Suche ist serverseitig und einrichtungsgebunden gemäß ARCH-Suchrahmen aus ADR 0004. Sie durchsucht Name, About-Me und Kommunikationsseiten. Treffer werden nur innerhalb des eigenen Berechtigungsrahmens angezeigt. | — |  |
| PBI-011 | Als Nutzer will ich Profile sortieren, ihren Status erkennen und neue oder aktualisierte Inhalte sehen, damit ich in der Übersicht schneller priorisieren kann. | — |  |
| PBI-010 | Als berechtigter Nutzer will ich ein neues Bewohnerprofil mit Name, Geburtsdatum, Einrichtung und Zimmer anlegen können, damit neue Bewohner strukturiert erfasst und mögliche Dubletten vor dem Anlegen erkannt werden. | — |  |

### Profil verstehen

| PBI | Story | Prio | Stand |
| --- | --- | --- | --- |
| PBI-012 | Als Nutzer will ich auf einer Profil-Detailseite ein oben mittig platziertes, mindestens doppelt so großes Profilbild, genau drei Sofortinfos und große Kacheln zu den wichtigsten Profilbereichen sehen, damit ich in Verständnissituationen sofort relevantes Wissen erfasse und mit einem Tap in den passenden Bereich wechseln kann. | — |  |

### About Me

| PBI | Story | Prio | Stand |
| --- | --- | --- | --- |
| PBI-013 | Als Nutzer will ich eine About-Me-Seite mit persönlichen Kurzinfos eines Bewohners sehen, damit ich schnell einen ersten Eindruck der Person bekomme. | — |  |
| PBI-035 | Die About-Me-Seite darf im MVP in ihrer aktuellen Form nicht verändert werden. — ⚠ in Klärung | — |  |
| PBI-014 | Als berechtigter Nutzer will ich in der About-Me-Seite Bilder mit Beschreibung hinzufügen und in einer Timeline sehen, damit persönliche Eindrücke aktuell und nachvollziehbar erweitert werden können. | — |  |
| PBI-015 | Als Product Owner will ich die Einwilligungs- und Datenschutzregeln für Bilder festlegen, damit Fotos im About-Me-Bereich rechtssicher genutzt werden können. | — |  |
| PBI-016 | Als verbundener Nutzer will ich erkennen oder gemeldet bekommen, wenn im About-Me-Bereich neue Inhalte vorliegen, damit ich aktuelles Wissen schneller wahrnehme. | — |  |
| PBI-017 | Als Product Owner will ich festlegen, ob Bilder und andere Medien im About-Me-Bereich mit Textzusammenfassungen oder ähnlichen Hilfen ergänzt werden, damit Inhalte zugänglicher und besser auffindbar werden. | — |  |
| PBI-044 | Als Angehörige will ich zu Einträgen eigene Fotos hochladen können, damit diese den Einträgen zugeordnet werden; pro Eintrag sollen maximal fünf Fotos zulässig sein, und es sollen nur JPG- oder PNG-Formate akzeptiert werden. | — |  |

### Kommunikation klären

| PBI | Story | Prio | Stand |
| --- | --- | --- | --- |
| PBI-018 | Als Nutzer will ich Kommunikationswissen in klar getrennten verbalen und nonverbalen Bereichen sehen, damit ich Signale und Ausdrucksweisen schneller richtig einordnen kann. | — |  |
| PBI-019 | Als berechtigter Nutzer will ich neue Kommunikationsweisen mit Text und Bildern hinzufügen können, damit wachsendes Erfahrungswissen zum Bewohner laufend dokumentiert wird. | — |  |
| PBI-020 | Als Product Owner will ich ein strukturiertes Beschreibungsmuster für Kommunikationsweisen und die darauf aufbauende Suchlogik festlegen, damit Kommunikationswissen später systematisch gefunden und gefiltert werden kann. | — |  |
| PBI-021 | Als berechtigter Nutzer will ich Videos von Kommunikationssituationen direkt in der Kommunikationsansicht hinzufügen und sehen, damit wichtige Ausdrucksweisen realitätsnah dokumentiert werden. | — |  |
| PBI-022 | Als Product Owner will ich festlegen, welche zusätzlichen Beschreibungen Kommunikationsmedien erhalten sollen, damit Inhalte zugänglicher und besser auffindbar werden. | — |  |
| PBI-004 | Als Nutzer will ich bei Inhaltsänderungen nachvollziehen können, wer einen Eintrag erstellt oder geändert hat und wann das passiert ist, damit Wissen vertrauenswürdig und nachverfolgbar bleibt. | — |  |

### No-Gos prüfen

| PBI | Story | Prio | Stand |
| --- | --- | --- | --- |
| PBI-023 | Als Nutzer will ich kritische No-Gos kompakt sehen und ergänzen können, damit ich in Gegenwart des Bewohners problematische Themen oder Handlungen vermeide. | — |  |

### Schicht sichern

| PBI | Story | Prio | Stand |
| --- | --- | --- | --- |
| PBI-032 | Als Pflegekraft will ich eine Übergabe-Notiz pro Schicht bis zur Archivierung bearbeiten können, damit Änderungen vor der Archivierung möglich sind und die zuletzt gültige Fassung jeweils mit Änderungszeitpunkt und bearbeitendem Account nachvollziehbar bleibt. | — |  |
| PBI-033 | Übergabe-Notizen sollen nach dreißig Tagen automatisch archiviert werden; archivierte Notizen bleiben weiterhin auffindbar — ⚠ in Klärung | — |  |
| PBI-045 | Als pflegende Person will ich am Ende meiner Schicht eine Tages-Zusammenfassung aller von mir dokumentierten Medikamenten-Gaben sehen können, damit ich vor der Übergabe prüfen kann, ob alles erfasst ist. | — |  |
| PBI-027 | Als Product Owner will ich festlegen, wie stark die App vorhandene Dokumentation integriert oder Daten daraus übernimmt, damit der Fokus auf unterstützender Kommunikation erhalten bleibt und Doppeldokumentation begrenzt wird. | — |  |
| PBI-028 | Als freigegebene/r Angehörige/r will ich für meine jeweilige Bezugsperson eine Monatsübersicht der Medikamenten-Einnahmen der letzten dreißig Tage einsehen, damit ich den Verabreichungsstatus nachvollziehen kann, ohne Dosierungsdetails zu sehen oder Einträge ändern zu können; vorhandene einnahmebezogene Bemerkungen sollen dabei mit angezeigt werden. Pflegende müssen in der Übersicht der Medikamenten-Einnahmen überfällige, noch nicht dokumentierte Gaben deutlich rot hervorgehoben sehen. | — |  |
| PBI-047 | Als pflegende Person will ich eine Übersicht der angekündigten Besuche der nächsten 14 Tage sehen können, damit ich bevorstehende Besuche im relevanten Zeitraum einsehen und bereits erledigte oder nicht mehr aktive angekündigte Besuche als solche erkennen kann. | — |  |
| PBI-048 | Als pflegende Person der jeweiligen Einrichtung will ich angekündigte Besuche bestätigen oder ablehnen können, damit Angehörige zu ihrer Ankündigung einen eindeutigen Status sehen und bei einer Ablehnung eine kurze Begründung erhalten. | — |  |
| PBI-046 | Als Angehörige/r will ich Besuche bei meiner Bezugsperson in der App vorab mit Datum und Uhrzeit ankündigen sowie bereits angefragte oder bestätigte Besuchsankündigungen bis zum Beginn des Besuchs ändern oder absagen können, damit Besuchstermine vor dem Termin erfasst und bei Änderungen korrekt neu angefragt bzw. bei Absage ohne weitere Erinnerung behandelt werden. | — |  |
| PBI-049 | Als Angehörige:r will ich einmal am Vortag um 18 Uhr per Push-Mitteilung an meinen bestätigten Besuch erinnert werden, damit ich den Besuch nicht vergesse. | — |  |

### Noch nicht eingeordnet

| PBI | Story | Prio | Stand |
| --- | --- | --- | --- |
| PBI-001 | Als Product Owner will ich das konkrete Produktziel und den zulässigen Lösungsrahmen der App festlegen, damit spätere Features die unterstützte Kommunikation verbessern, ohne in eine beidseitige Übersetzungslösung abzugleiten. | — |  |
| PBI-026 | Als Product Owner will ich bewerten, ob Tablet-Nutzung und alternative Eingabemethoden später unterstützt werden sollen, damit Erweiterungen an realen Bedarfen ausgerichtet sind. | — |  |
| PBI-029 | Als Product Owner will ich Anforderungen mit Beteiligten vor Ort erheben und messbare Pilot-Erfolgskriterien definieren, damit Produktentscheidungen auf realen Nutzungssituationen basieren. | — |  |
| PBI-038 | Zum Thema Piktogramme besteht ein noch nicht ausreichend konkretisierter Änderungswunsch; ... — ⚠ in Klärung | — |  |
| PBI-039 | Es ist unklar, ob aus dem Hinweis auf die anstehende Übergabe eine eigenständige fachliche... — ⚠ in Klärung | — |  |
| PBI-040 | Es ist unklar, welcher zuvor genannte Punkt aus Sicht der Angehörigen besonders wichtig is... — ⚠ in Klärung | — |  |
| PBI-041 | Es ist zu klären, ob Aufgabenverfolgung oder Terminplanung als eigene organisatorische Anf... — ⚠ in Klärung | — |  |
| PBI-042 | Als Nutzer will ich den Dunkelmodus in den Einstellungen manuell ein- und ausschalten, damit ich die Darstellung bei Bedarf ohne Automatik anpassen kann. | — |  |
| PBI-043 | Eine einstellbare Schriftgröße ist ein Wunsch zur Unterstützung älterer Kolleginnen, mit n... — ⚠ in Klärung | — |  |
