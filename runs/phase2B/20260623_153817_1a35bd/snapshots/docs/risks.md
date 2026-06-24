# Risikoanalyse für die Kommunikations-App

## Fachliche Risiken
- **Unklare Zugriffsrechte:** Noch offener Klärungsbedarf bei differenziertem Zugriff auf Profile verschiedener Einrichtungen birgt das Risiko von unautorisiertem Datenzugriff oder versehentlicher Datenfreigabe.
- **Funktionsumfang vs. Benutzerfreundlichkeit:** Die Balance zwischen umfangreichen Funktionen und einfacher Bedienbarkeit ist noch nicht final geklärt. Ein zu komplexes Interface könnte die Akzeptanz der Nutzer mindern.

## Technische Risiken
- **Firebase Firestore Nutzung:** Abhängigkeit von Firebase als Cloud-Datenbank mit lokaler Zwischenspeicherung erfordert genaue Prüfung der Datensicherheit und Stabilität, insbesondere offline.
- **Plattformübergreifende Entwicklung:** Flutter/Dart müssen alle geplanten Funktionen auf iOS und Android (schwerpunktmäßig Android 11) zuverlässig unterstützen; Inkompatibilitäten oder Plattform-Limitationen können Entwicklungen verzögern.
- **Videoeinbindung:** Noch bestehende Unsicherheit zur Integration von Videos in die Kommunikationsseiten kann die Funktionalität mindern oder technische Verzögerungen verursachen.

## Compliance- und Datenschutzrisiken
- **Datenschutzrechtliche Absicherung:** Die genaue Umsetzung des Datenschutzes und die rechtliche Absicherung sind noch offen, was das Risiko von Verstößen gegen gesetzliche Vorgaben erhöht.
- **Registrierungsprozess:** Zentral durch Administratoren gesteuerte Registrierung muss sicherstellen, dass keine unbefugten Accounts erstellt werden und das Identitätsmanagement zuverlässig funktioniert.

## Widersprüche und Unsicherheiten
- Unklare Umsetzung der Datenschutzanforderungen und Zugriffsrechte schaffen Unsicherheit für die weitere Entwicklung.
- Noch abzustimmende Videointegration kann zu Verzögerungen oder Anpassungen führen.
- Balance zwischen Funktionalität und Benutzerfreundlichkeit ist noch nicht definiert, was die Zielerreichung gefährdet.

## Mögliche Auswirkungen
- Datenschutzverstöße können rechtliche Konsequenzen und Imageverlust verursachen.
- Fehlende oder unklare Zugriffsrechte führen zu Datenlecks oder unerwünschtem Informationszugang.
- Technische Limitierungen oder Verzögerungen schwächen die Nutzerakzeptanz und verzögern Projektfortschritt.
- Komplexe oder unübersichtliche App kann die Nutzung durch Betreuer und Angehörige erschweren.

## Gegenmaßnahmen und Klärungsbedarfe
- Klare und rechtssichere Definition der Datenschutz- und Zugriffsrichtlinien mit juristischer Beratung.
- Umsetzung strenger Registrierungs- und Accountverwaltungsprozesse durch Administratoren.
- Technische Prüfung und Tests der Firebase Firestore Nutzung speziell bzgl. Offline-Funktionalität und Datensicherheit.
- Festlegung der Videoeinbindung nach technischer Machbarkeitsanalyse.
- Nutzerzentrierte Design- und Usability-Tests zur Sicherstellung der Balance zwischen Funktionalität und Benutzerfreundlichkeit.
- Regelmäßige Abstimmung und Dokumentation offener Punkte und Annahmen im Projektteam.
