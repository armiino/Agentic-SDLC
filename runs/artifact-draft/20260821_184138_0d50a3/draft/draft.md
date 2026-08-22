# Level 1 — Kontext

```mermaid
C4Context
title Architektur-Landkarte – Kontext

Person(admin, "Admin", "Verwaltet Accounts und Rechte")
Person(user, "Mitarbeitende / Betreuer", "Nutzen die App, um Bewohner besser zu verstehen und Inhalte beizutragen")
Person(relative, "Angehörige", "Erhalten Zugriff auf die App und können Wissen bzw. Inhalte beitragen")
Person(resident, "Bewohner", "Kann nur das eigene Profil sehen und eingeschränkte Funktionen nutzen")

System(app, "Kommunikations-App", "Digitale App zur Förderung der Kommunikation rund um Bewohnerprofile und Kommunikationswissen")

System_Ext(records, "Bestehende Akten / Dokumentationen", "Vorhandene klassische Akten als Bestandssituation")
System_Ext(fcm, "Firebase Cloud Messaging", "Push-Benachrichtigungen an Angehörige und Pflegende")

Rel(admin, app, "meldet sich an, verwaltet Accounts und Rechte")
Rel(user, app, "meldet sich an, sucht Profile, sieht Kommunikationswissen ein, ergänzt Inhalte")
Rel(relative, app, "meldet sich an, sieht freigegebene Inhalte ein und trägt Inhalte/Wissen bei")
Rel(resident, app, "meldet sich an und sieht das eigene Profil eingeschränkt ein")

Rel(app, records, "berücksichtigt bestehende Informationen", "fachliche Bestandssituation")
Rel(app, fcm, "sendet Push-Benachrichtigungen über", "ARCH-46 / ADR-0001")
```

# Level 2 — Container, NUR BELEGT

```mermaid
C4Container
title Architektur-Landkarte – Container (nur belegte Kästen)

Person(admin, "Admin")
Person(user, "Mitarbeitende / Betreuer")
Person(relative, "Angehörige")
Person(resident, "Bewohner")

System_Boundary(appBoundary, "Kommunikations-App") {
  Container(mobileApp, "Mobile App", "Flutter / Dart", "Plattformübergreifende App für iPhone und Android")
  Container(adminScreen, "Admin-Bildschirm", "?", "Separater Bildschirm für Account- und Rechteverwaltung, nur für Admins erwogen")
  ContainerDb(auditLog, "Audit-Log", "Serverseitig, append-only", "Revisionssichere Protokollierung von Einsichtnahmen")
  Container(serverNotification, "Serverseitiger Benachrichtigungsrahmen", "?", "Zentrale Planung und Rücknahme von Besuchserinnerungen")
  ContainerDb(firestore, "Firestore", "Firebase Firestore", "Vorläufig vorgesehene Datenbanklösung")
  Container(pushService, "Push-Benachrichtigungsdienst", "Firebase Cloud Messaging", "Versand von Push-Benachrichtigungen")
  Container(q1, "?", "?", "Authentifizierung / Account-Backend ungeklaert")
  Container(q2, "?", "?", "Serverseitige Durchsetzung der Einrichtungsgrenze ungeklaert")
  Container(q3, "?", "?", "Medienspeicherung für Bilder und Videos ungeklaert")
  Container(q4, "?", "?", "Lokaler Gerätespeicher / Cache ungeklaert")
}

Rel(admin, mobileApp, "nutzt")
Rel(user, mobileApp, "nutzt")
Rel(relative, mobileApp, "nutzt")
Rel(resident, mobileApp, "nutzt")

Rel(mobileApp, adminScreen, "öffnet für Admin-Aufgaben")
Rel(mobileApp, firestore, "liest/schreibt Profildaten, About-Me, Kommunikationswissen, No-Gos")
Rel(mobileApp, pushService, "empfängt Push-Benachrichtigungen über")
Rel(mobileApp, q1, "meldet Nutzer an")
Rel(mobileApp, q4, "nutzt optional für lokale Speicherung / Caching")

Rel(serverNotification, pushService, "stößt Besuchserinnerungen an")
Rel(serverNotification, firestore, "nutzt bestätigten Besuchsstatus als Grundlage")
Rel(serverNotification, q2, "muss Einrichtungszuordnung berücksichtigen")

Rel(mobileApp, auditLog, "löst Protokollierung von Einsichtnahmen aus")
Rel(auditLog, q2, "muss Admin-Leserechte und Einrichtungsbezug absichern")

Rel(mobileApp, q3, "lädt Bilder und Videos hoch / zeigt sie an")
```

| Kasten | Beleg |
|---|---|
| Mobile App | ARCH-01, ARCH-03, ARCH-36, ARCH-37 |
| Admin-Bildschirm | ARCH-13 |
| Audit-Log | ARCH-47, ADR 0002-unveraenderlicher-audit-log-rahmen-fuer-einsichtnahmen.md |
| Serverseitiger Benachrichtigungsrahmen | ARCH-48, ADR 0003-serverseitiger-benachrichtigungsrahmen-fuer-besuchserinnerun.md |
| Firestore | ARCH-39 |
| Push-Benachrichtigungsdienst | ARCH-46, ADR 0001-firebase-cloud-messaging-fuer-push-benachrichtigungen.md |
| ? (Authentifizierung / Account-Backend ungeklaert) | ARCH-07, ARCH-08, ARCH-12 |
| ? (Serverseitige Durchsetzung der Einrichtungsgrenze ungeklaert) | ARCH-11 |
| ? (Medienspeicherung für Bilder und Videos ungeklaert) | ARCH-19, ARCH-20, ARCH-22, ARCH-23 |
| ? (Lokaler Gerätespeicher / Cache ungeklaert) | ARCH-40 |

## Offene Architektur-Lücken

- Wie wird die Authentifizierung und Account-Verwaltung technisch umgesetzt, wenn es keine Selbstregistrierung geben darf und Accounts intern vergeben werden müssen?
- Wie wird die Einrichtungsgrenze technisch und serverseitig durchgesetzt, sodass Mitarbeitende nur Profile ihrer eigenen Einrichtung sehen und bearbeiten können?
- Wo und wie werden Bilder und Videos für About Me und Kommunikationsseiten technisch gespeichert und ausgeliefert?
- Ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen, einschließlich Verhalten bei Offline-Nutzung und Schutz lokal gespeicherter Daten?