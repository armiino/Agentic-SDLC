# Projektkontext

## Quelle
- Primärquelle: `input/transcripts/Interview-Einrichtung.txt`
- Das Dokument enthält mehrere Abschnitte mit unterschiedlichen Perspektiven:
  1. Interview mit Gesamtleitung der Einrichtung
  2. Interview mit Heilerziehungspflegerin
  3. Erste studentische Ausarbeitung
  4. Interview mit Fachpersonal
  5. Zweite studentische Ausarbeitung
  6. Dritte studentische Ausarbeitung
  7. (Abschnitt 7 fehlt im Transkript)
  8. Design-Ausarbeitung

## Projektziel
Ziel des Vorhabens ist die Entwicklung einer **digitalen Unterstützung für die Kommunikation** zwischen Menschen mit Beeinträchtigungen und Personen, die sie noch nicht gut kennen oder ihre individuellen Ausdrucksweisen nicht verstehen. Im Verlauf der Gespräche konkretisiert sich dies zu einer **App**, die Wissen über individuelle Kommunikationsweisen, persönliche Informationen und situative Hinweise zu Bewohnern strukturiert erfasst und für berechtigte Nutzer schnell abrufbar macht.

Wesentliche Zielrichtung:
- nicht: automatische Übersetzung zwischen individueller Ausdrucksweise und Standardsprache
- sondern: **Dokumentation, Aufbereitung und schneller Zugriff auf vorhandenes Erfahrungswissen** von Betreuungspersonen und Angehörigen

## Ausgangslage
Die Einrichtung betreut Menschen mit Beeinträchtigungen in Einrichtungen und Wohngemeinschaften. Laut Gesamtleitung kommunizieren die betreuten Personen teils verbal in sehr individueller Weise, teils nonverbal, z. B. über Gebärden oder eigene Ausdrucksformen. Dieses Wissen ist häufig vor allem bei langjährigen Mitarbeitenden vorhanden.

Daraus ergibt sich ein Kernproblem:
- Neue oder fachfremde Personen verstehen Bewohner oft nicht direkt.
- Viel relevantes Wissen ist erfahrungsgebunden.
- Klassische Akten existieren zwar, werden aber aus Sicht des Fachpersonals nicht immer als praxistauglich erlebt, weil sie umfangreich sind und gesuchte Informationen schwer auffindbar sein können.

## Stakeholder und Rollen

### Gesamtleitung der Einrichtung
- vertritt die Einrichtung im Kick-off
- formuliert den Bedarf nach einer digitalen Lösung
- priorisiert den Nutzen, Bewohner besser verstehen zu können
- kann organisatorische Unterstützung geben, z. B. Zugang zu Einrichtungen und Einbindung weiteren Fachpersonals

### Heilerziehungspflegerin / Fachpersonal
- verfügt über implizites Alltagswissen zur Kommunikation einzelner Bewohner
- beschreibt Einarbeitungsprozesse neuer Mitarbeitender
- bewertet Nutzen der App hoch, insbesondere für neue Betreuungspersonen
- bringt fachliche Anforderungen ein, z. B. Datenschutzsensibilität, No-Go-Inhalte, Videoeinsatz, Angehörigenbeteiligung

### Angehörige
- werden als potenziell wichtige Wissensquelle gesehen
- sollen ggf. Zugriff erhalten und Informationen beisteuern können
- könnten insbesondere bei neuen Bewohnern schon vorab Inhalte befüllen

### Bewohner / Menschen mit Beeinträchtigungen
- sind die zentrale Bezugsgruppe des Projekts
- sollen indirekt profitieren, indem andere sie besser verstehen
- später kommt die Idee auf, ihnen ggf. einen eingeschränkten eigenen Account zu geben, damit sie ihr eigenes Profil sehen und in begrenztem Umfang Inhalte beitragen können

### Studentisches / technisches Projektteam
- führt Anforderungsanalyse durch
- bringt technische und sozialpädagogische Perspektiven zusammen
- entwickelt nach und nach ein App-Konzept samt Rollen, Screens und technischer Umsetzungsannahmen

## Fachliche Hauptthemen

### 1. Unterstützte Kommunikation
Kern des Projekts ist die Frage, wie individuelle verbale und nonverbale Kommunikationsweisen so erfasst werden können, dass auch weniger erfahrene Personen sie deuten können.

Beispiele aus dem Transkript:
- individuelle Sprache
- nonverbale Signale
- situationsbezogene Interpretation bestimmter Bewegungen
- Bedarf, Wissen schnell und praxisnah auffindbar zu machen

### 2. Wissenssicherung und Wissensweitergabe
Es besteht der Bedarf, Erfahrungswissen von langjährigen Mitarbeitenden nicht nur informell weiterzugeben, sondern systematisch zu erfassen.

Geplante Stoßrichtung:
- strukturierte digitale Erfassung
- dynamische Erweiterbarkeit
- schneller Abruf in konkreten Kommunikationssituationen

### 3. Bewohnerprofile / personenbezogene Informationen
Die App-Idee umfasst persönliche Profilbereiche, um einen ersten Eindruck von Bewohnern zu vermitteln.

Genannte Inhalte:
- Name
- Alter
- Hobbys
- Bilder mit Beschreibungen
- allgemeine Informationen über die Person

### 4. Kommunikationsdokumentation
Ein zentraler Funktionsbereich soll Kommunikationsweisen dokumentieren.

Genannte Ausprägungen:
- Unterteilung in verbal / nonverbal
- Texte, Bilder und ggf. Videos zur Veranschaulichung
- dynamisches Hinzufügen neuer Beobachtungen
- Such- und Filtermöglichkeiten

### 5. Videos als Interpretationshilfe
Im Verlauf wird die Idee eingebracht, typische Situationen oder Kommunikationsmuster per Video festzuhalten, damit andere diese wiedererkennen können. Später wird diskutiert, die Videofunktion nicht als separaten Bereich zu führen, sondern direkt in die Kommunikationsseiten zu integrieren.

### 6. No-Go-Informationen / kritische Hinweise
Ein eigener Bereich für Dinge, die im Umgang mit einzelnen Bewohnern unbedingt vermieden werden sollen, wird als wichtig eingeschätzt.

Beispiele für solche Inhalte:
- sensible Trigger
- unverträgliche Reize
- klare Verbote oder Warnhinweise im Umgang

### 7. Kalender / Termine / Medikamente
Mehrfach wird die Idee erwähnt, zusätzlich Termine und ggf. Medikamenteninformationen abzubilden. Gleichzeitig wird auch betont, dass die App nicht überfrachtet werden sollte und Medikamente besonders vertraulich sind.

Daher ist dieser Bereich im Kontext eher als **Erweiterungs- oder Prüfpunkt** zu verstehen als als eindeutig priorisierter Kern.

### 8. Rollen- und Rechtemanagement
Aus Datenschutz- und Organisationsgründen wird ein differenziertes Rechtekonzept diskutiert.

Genannte Rollen:
- Admin
- User (z. B. Mitarbeitende, Angehörige je nach Ausgestaltung)
- Bewohner-Account mit stark eingeschränkten Rechten

### 9. Zugriffsbeschränkung nach Einrichtung
Im Fachgespräch wird thematisiert, dass Mitarbeitende nicht automatisch einrichtungsübergreifend auf alle Profile zugreifen sollten. Dies wird als sinnvolle datenschutzbezogene Anforderung festgehalten.

### 10. Barrierefreiheit und Benutzerfreundlichkeit
In der Designphase werden Bedienbarkeit und Zugänglichkeit explizit angesprochen.

Genannte Aspekte:
- große Schrift
- ausreichender Kontrast
- klare Navigation
- Hilfe/Tutorial
- reduzierte Ablenkung
- ggf. alternative Eingabemethoden

## Vorläufiges Lösungsbild
Aus den Gesprächen entsteht schrittweise das Bild einer mobilen App mit ungefähr folgenden Bausteinen:
- Login ohne offene Selbstregistrierung
- Profilübersicht mit Suchfunktion
- Profilseiten je Bewohner
- About-Me-Bereich
- Kommunikationsbereich (verbal/nonverbal)
- Einbindung von Bildern, Texten und Videos
- No-Go-Bereich
- ggf. Kalender-/Terminbereich
- Einstellungen / Rechteverwaltung

Wichtig: Dieses Lösungsbild ist im Transkript noch **Anforderungs- und Konzeptstand**, keine final bestätigte Spezifikation.

## Konflikte, Spannungen und Abgrenzungen

### 1. Wunsch nach breiter Funktionalität vs. Fokus
Einerseits werden viele mögliche Funktionen genannt (Kommunikation, Bilder, Videos, Kalender, Medikamente, No-Gos, Benachrichtigungen, Hilfe, Rollenverwaltung). Andererseits wird ausdrücklich darauf hingewiesen, dass zu viel Funktionsumfang die App schwerer, langsamer und potenziell weniger nützlich machen könnte.

### 2. Kommunikation unterstützen vs. automatische Übersetzung
Die Idee einer echten Übersetzungsinstanz zwischen Bewohner und Außenwelt wird früh verworfen. Begründung:
- individuell zu stark auf einzelne Personen zugeschnitten
- technisch bzw. wirtschaftlich nicht sinnvoll im Projektkontext

### 3. Dokumentation vorhanden vs. praktisch schwer nutzbar
Es gibt bereits Akten und schriftliche Dokumentation. Zugleich schildert das Fachpersonal, dass diese in der Praxis zu umfangreich oder schwer durchsuchbar sein können. Die App adressiert damit eher **Auffindbarkeit und Alltagstauglichkeit** als das völlige Fehlen von Informationen.

### 4. Digitalisierungschance vs. Datenschutzgrenzen
Die Einrichtung sieht Digitalisierung positiv und als Chance. Gleichzeitig betreffen die Inhalte hochsensible personenbezogene Daten. Datenschutz wird mehrfach angesprochen, aber im Transkript nicht abschließend geklärt.

### 5. Interner Nutzen vs. Beteiligung externer Akteure
Die App ist zunächst klar als internes Werkzeug gedacht. Gleichzeitig wird eine Beteiligung von Angehörigen befürwortet, was zusätzliche Rechte- und Datenschutzfragen eröffnet.

## Unsicherheiten und offene Punkte
- Wie genau Datenschutz, Einwilligungen und Bild-/Videonutzung geregelt werden können, bleibt offen.
- Ob Bewohnerakten tatsächlich eingesehen oder in Teilen genutzt werden dürfen, ist ungeklärt.
- Ob und in welchem Umfang Medikamente und Kalender Bestandteil der App werden sollen, ist nicht final entschieden.
- Ob Videos als eigener Bereich oder integriert in Kommunikationsseiten umgesetzt werden, entwickelt sich im Verlauf und ist konzeptionell noch im Fluss.
- Die genaue Rollen- und Rechteverwaltung ist noch nicht ausdefiniert.
- Ob Tablets zusätzlich zu Smartphones unterstützt werden, wird nur als Anforderung aufgenommen, nicht bestätigt.
- Technische Vorschläge aus den studentischen Ausarbeitungen (z. B. Flutter, Firebase, lokale Speicherung) sind Umsetzungsüberlegungen, aber keine von der Einrichtung bestätigten fachlichen Anforderungen.
- Die im Transkript erwähnte Organisationsbezeichnung ist nicht überall konsistent (z. B. Einrichtung-NoName, KYOS/KYOSW); diese Bezeichnungen sollten als uneinheitliche Quellenlage betrachtet werden.
- Der Abschnittszähler springt von 6 auf 8; ein siebter Abschnitt fehlt im vorliegenden Material.

## Ableitbare Prioritäten
Hohe Priorität aus dem Transkript:
- Bewohner besser verstehen
- Erfahrungswissen für neue Mitarbeitende verfügbar machen
- individuelle Kommunikationsweisen strukturiert dokumentieren
- einfache und schnelle Nutzbarkeit im Alltag
- digitale, möglichst mobile Verfügbarkeit

Mittlere bzw. zu prüfende Priorität:
- Angehörigenzugriff
- Videos
- Such- und Filterlogik
- No-Go-Bereich
- Benachrichtigungen
- Bewohner-Accounts

Eher offen / auszuarbeiten:
- vollständige Dokumentationsfunktion
- Medikamentenverwaltung
- Kalender als Kernbestandteil
- genaue technische Architektur

## Quellenhinweise nach Themen
- **Projektanlass und Zielbild**: Abschnitt 1, Interview mit Gesamtleitung
- **Probleme bestehender Dokumentation / Einarbeitung**: Abschnitt 2, Interview mit Heilerziehungspflegerin
- **App-Struktur und frühe Funktionsideen**: Abschnitte 1–3
- **Datenschutz, Rollen, einrichtungsbezogene Zugriffe, Suche**: Abschnitt 4, Interview mit Fachpersonal
- **Bewohner-Account, Admin-Verwaltung, Suchmuster**: Abschnitt 5
- **Technologieüberlegungen**: Abschnitt 6
- **Design, Barrierefreiheit, Hilfefunktion**: Abschnitt 8

## Kurzfazit
Das Projekt ist aus einem realen Kommunikationsproblem im Betreuungsalltag entstanden: Individuelle Ausdrucksweisen von Bewohnern sind oft nur erfahrenen Personen bekannt. Die angedachte App soll dieses Wissen in alltagstauglicher, digitaler Form zugänglich machen. Der fachliche Kern liegt in unterstützter Kommunikation und Wissensweitergabe; Datenschutz, Rollenmodell, Funktionsumfang und technische Umsetzung sind wichtige, aber noch nicht abschließend geklärte Punkte.