# Anforderungen

## Functional Requirements
- **FR1 Kundenportal (Web‑First)**: Ein webbasiertes Kundenportal muss bereitgestellt werden, das es Kunden ermöglicht, Angebote zu erstellen, Bestellungen einzusehen und Rechnungen herunterzuladen. (Quelle: Anna)
- **FR2 Authentifizierung**: Login per E‑Mail und Passwort ist zwingend. Optional kann SSO via Azure AD und/oder Google integriert werden. (Anna, Ben)
- **FR3 Rollen‑ und Berechtigungskonzept**: Unterstützt mindestens die Rollen Admin, User, Manager und Support mit entsprechenden Zugriffsrechten. (Anna, Clara)
- **FR4 Angebots‑Workflow**: Sales‑Mitarbeiter können über das Portal Angebote erstellen, speichern und an Kunden senden. (Anna, Ben)
- **FR5 Rechnungs‑ und Bestellungsanzeige**: Kunden können ihre Bestellungen einsehen und zugehörige Rechnungen herunterladen. (Anna)
- **FR6 SAP‑Integration**: Das System muss Produkt‑, Preis‑ und Kundendaten aus dem bestehenden SAP‑System beziehen und Änderungen zurückschreiben können. (Ben)
- **FR7 API‑Layer**: Bereitstellung einer REST‑API für die Frontend‑ und SAP‑Integration. Authentifizierung über OAuth 2.0 wird bevorzugt; alternativ API‑Keys. (Ben, Clara)
- **FR8 DSGVO‑konforme Login‑ und Registrierungsprozesse**: Double‑Opt‑In für E‑Mail‑Adressen, Einwilligung für Marketing‑Push‑Notifications und Möglichkeit zum Datenlösch‑Request. (Clara, Anna)
- **FR9 Logging & Audit‑Trail**: Alle relevanten Aktionen (Login, Angebotsänderungen, Datenlöschung) werden protokolliert und können nachverfolgt werden. (Clara)
- **FR10 Backup & Disaster Recovery**: Tägliche Backups der Kundendaten und Wiederherstellungs‑Mechanismus gemäß EU‑Datenschutz‑Standards. (Clara)
- **FR11 KPI‑Erfassung (MVP)**: Erfassung von Conversion Rate (Angebot → Bestellung) und Zeit bis zum Angebot. (Anna)
- **FR12 Optional Mobile‑Zugriff**: Möglichkeit, das Portal später responsiv oder als native App bereitzustellen. (Anna, Ben)

## Non-functional Requirements
- **NFR1 Performance**: Das System muss bis zu 20 000 gleichzeitige Nutzer unterstützen; mindestens 200 TPS für API‑Aufrufe. (Ben)
- **NFR2 Skalierbarkeit**: Architektur muss auf Managed Cloud Services basieren, um horizontale Skalierung zu ermöglichen, ohne neue eigene DB‑Server zu provisionieren. (Anna, Ben)
- **NFR3 Verfügbarkeit**: 99,5 % Jahresverfügbarkeit, inkl. automatischem Failover. (Clara)
- **NFR4 Sicherheit**: TLS 1.2 oder höher für alle Kommunikationswege; OAuth 2.0 für API‑Zugriff; keine End‑to‑End‑Verschlüsselung erforderlich (TLS reicht). (Ben, Clara)
- **NFR5 Datenschutz**: Daten ausschließlich in EU‑Regionen gespeichert; Einhaltung DSGVO (Double‑Opt‑In, Löschkonzept, Audit‑Trail). (Clara)
- **NFR6 Wartbarkeit**: Dokumentation des Architektur‑ und Sicherheits‑Designs, jedoch minimal gehalten, um MVP‑Zeitplan zu schützen. (Anna, Ben)
- **NFR7 Time‑to‑Market**: MVP muss innerhalb von **8 Wochen** nach Projektstart bereit sein. (Anna)
- **NFR8 Kosten**: Nutzung von günstigen Managed Services; kein neuer eigenständiger Datenbank‑Server. (Anna)

## Constraints/Compliance
- **C1 DSGVO**: Alle Datenverarbeitungen müssen DSGVO‑konform sein (Double‑Opt‑In, Löschungen, EU‑Hosting). (Clara)
- **C2 Security Review**: Ein formeller Security Review muss durchgeführt werden; dies kollidiert mit 8‑Wochen‑MVP‑Ziel – daher muss ein leichter, aber dokumentierter Review‑Prozess implementiert werden. (Clara, Ben)
- **C3 Infrastruktur**: Keine zusätzlichen On‑Premise‑Datenbank‑Server; ausschließlich Managed Cloud Services innerhalb der EU nutzen. (Anna, Ben)
- **C4 Budget**: Lösung muss kostengünstig sein; teure Native‑App‑Entwicklung ist ausgeschlossen. (Anna)
- **C5 Zeitplan**: 8‑Wochen‑MVP; Security Review von 6 Wochen muss parallel oder verkürzt werden. (Anna, Ben, Clara)

## Traceability
| Anforderung | Quelle | Kommentar |
|-------------|--------|-----------|
| FR1 | Anna: *„Kundenportal…“* | Kernfunktionalität
| FR2 | Anna & Ben | Login & SSO Diskussion
| FR3 | Anna & Clara | Rollenmodell
| FR4 | Anna & Ben | Angebotserstellung
| FR5 | Anna | Rechnungen & Bestellungen
| FR6 | Ben | SAP‑Integration nötig
| FR7 | Ben & Clara | API‑Layer & OAuth
| FR8 | Clara & Anna | DSGVO‑Login & Double‑Opt‑In
| FR9 | Clara | Logging & Audit‑Trail
| FR10 | Clara | Backup & DR
| FR11 | Anna | KPI‑Messung im MVP
| FR12 | Anna & Ben | Mobile optional
| NFR1 | Ben | Nutzer‑/TPS‑Annahme
| NFR2 | Anna & Ben | Managed Services, keine DB
| NFR3 | Clara | Verfügbarkeit
| NFR4 | Ben & Clara | TLS & OAuth
| NFR5 | Clara | EU‑Only Hosting
| NFR6 | Anna & Ben | Minimaldokumentation
| NFR7 | Anna | 8‑Wochen‑MVP
| NFR8 | Anna | Kostenbeschränkung
| C1 | Clara | DSGVO‑Pflicht
| C2 | Clara & Ben | Security Review nötig
| C3 | Anna & Ben | Keine neue DB
| C4 | Anna | Budgetrestriktion
| C5 | Anna, Ben, Clara | Zeitplan‑Konflikt

*Annahmen / offene Klärungen*:
- Mobile‑Priorisierung (Web‑First vs. Mobile‑First) muss noch final entschieden werden.
- Detail‑KPIs (außer Conversion Rate & Zeit bis Angebot) sind noch offen.
- Umfang des Security Reviews (welche Tests sind zwingend) muss spezifiziert werden.
- Exakte Skalierbarkeits‑Zielwerte (TPS, Nutzerzahl) sind Schätzungen; ggf. im Detail zu validieren.
