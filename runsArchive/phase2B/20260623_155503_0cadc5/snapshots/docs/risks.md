# Risikoanalyse für die Kommunikations-App

## Fachliche Risiken

- **Balance zwischen Funktionsumfang und Bedienbarkeit**: Die Anforderungen umfassen viele Funktionen, die App soll aber benutzerfreundlich und übersichtlich bleiben. Eine Überfrachtung kann die Akzeptanz und Nutzbarkeit beeinträchtigen.
- **Nutzerrollen und Rechteverwaltung**: Die klare und sichere Abbildung komplexer Rollen (Admin, Betreuer, Angehörige, Bewohner mit eingeschränkten Zugriffsrechten) kann fehleranfällig sein und zu ungewolltem Datenzugriff oder -verlust führen.

## Technische Risiken

- **Offene technische Architektur der Datenbank und Synchronisation**: Ohne eine frühzeitige, konkrete technische Ausgestaltung besteht das Risiko von Implementierungsverzögerungen, Performanceproblemen oder inkonsistenten Daten.
- **Plattformübergreifende Entwicklung mit Flutter/Dart**: Flutter ist mächtig, aber es können technische Einschränkungen oder Kompatibilitätsprobleme auftreten, die den Entwicklungsaufwand erhöhen.

## Compliance- und Datenschutzrisiken

- **Ungeklärte Datenschutz- und Rechtsvorgaben**: Da der Datenschutz ein gesondertes Projekt ist und Details noch offen sind, besteht das Risiko, dass gesetzliche Anforderungen nicht vollständig eingehalten werden, was rechtliche Konsequenzen nach sich ziehen kann.
- **Keine freie Registrierung, nur Zugang über Accounts der Einrichtung**: Dies muss technisch und organisatorisch sicher umgesetzt werden, um unerlaubte Zugriffe zu verhindern.

## Widersprüche und Unsicherheiten

- Die genaue Ausgestaltung der technischen Basis (Datenbank, Synchronisation) ist offen und birgt Unsicherheiten.
- Die Datenschutzaspekte und -maßnahmen sind noch nicht definiert.
- Es ist unklar, wie die Balance zwischen Funktionsumfang und Bedienbarkeit optimal erreicht wird.

## Mögliche Auswirkungen

- Verzögerungen im Projektzeitplan
- Akzeptanzprobleme bei den Nutzern durch schlechte Usability oder unklare Rechtevergabe
- Rechtliche Probleme und Datenschutzverstöße
- Performance- und Stabilitätsprobleme der App

## Mögliche Gegenmaßnahmen und Klärungsbedarfe

- Frühzeitige und enge Abstimmung mit dem Datenschutzprojekt, um Anforderungen und Maßnahmen zu klären.
- Definition und Prototypen der technischen Architektur für Datenbank und Synchronisation vor Implementierungsstart.
- Iterative Usability-Tests zur Sicherstellung einer angemessenen Bedienbarkeit trotz Funktionsumfang.
- Klare und sichere Implementierung der Nutzer- und Rechteverwaltung mit Fokus auf Datenschutz und Benutzerfreundlichkeit.
- Risiko- und Change-Management etablieren, um auf offene Punkte flexibel reagieren zu können.

---

*Dieses Risikoartefakt basiert auf dem bereitgestellten Projektkontext und den Requirements (Stand 23.06.2026).*