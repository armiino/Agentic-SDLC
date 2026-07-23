## Functional Requirements

- **FR1**: Benutzer*innen müssen sich per E‑Mail und Passwort anmelden können; der Registrierungsprozess beinhaltet ein Double‑Opt‑In für Marketing‑Einwilligungen. *(Quelle: Projektziel, Stakeholder‑Anna)*
- **FR2**: Das System unterstützt mindestens die Rollen **Admin**, **Sales** und **Kunde** mit jeweils definierten Berechtigungen (z. B. Angebots‑Erstellung für Sales, Ansicht für Kunden). *(Quelle: Rollen‑&‑Berechtigungskonzept, Stakeholder‑Anna, Ben)*
- **FR3**: Sales kann Angebote erstellen, wobei im MVP nur Standard‑Rabatte (keine Sonderrabatte) zulässig sind. *(Quelle: MVP‑Umfang, Stakeholder‑Anna, Ben)*
- **FR4**: Kunden können ihr Angebot einsehen, akzeptieren und eine Bestellung auslösen. *(Quelle: Projektziel, Stakeholder‑Anna)*
- **FR5**: Nach Bestellung erhalten Kunden die Rechnung als PDF‑Download. *(Quelle: MVP‑Umfang, Stakeholder‑Anna)*
- **FR6**: Das System liest Produkt‑ und Preisdaten aus dem SAP‑System über eine rein lesende Schnittstelle. *(Quelle: SAP‑Integration, Stakeholder‑Ben)*
- **FR7**: Ein Basis‑Audit‑Trail protokolliert kritische Aktionen (Login, Angebotserstellung, Rechnungsgenerierung, Datenlöschung). *(Quelle: Basis‑Audit‑Trail, Stakeholder‑Anna, Clara)*
- **FR8**: Double‑Opt‑In wird im Registrierungsprozess durchgeführt, um DSGVO‑Konformität sicherzustellen. *(Quelle: DSGVO / Compliance, Stakeholder‑Clara)*
- **FR9**: Angebots‑ und Rechnungsdokumente werden als PDF erzeugt, enthalten rechtliche Fußnoten und Versionsinformationen. *(Quelle: Dokumente & Templates, Stakeholder‑Eva)*
- **FR10 (optional)**: Push‑Notifications können Kunden über Statusänderungen informieren (nicht zwingend für MVP). *(Quelle: Fachliche Themen, Stakeholder‑Alle)

## Non-functional Requirements

- **NFR1**: Das System muss in einer EU‑only Managed‑Hosting‑Umgebung betrieben werden. *(Quelle: Constraints/Compliance, Stakeholder‑Farid, Anna)*
- **NFR2**: Das System muss DSGVO‑konform sein (Datenminimierung, Recht auf Vergessenwerden, Löschkonzepte). *(Quelle: DSGVO / Compliance, Stakeholder‑Clara)*
- **NFR3**: Verfügbarkeit soll hoch sein; Ziel‑Verfügbarkeitsgrad ≥ 99 % im Produktionsbetrieb. *(Quelle: Diskussion Security vs. MVP, Stakeholder‑Clara, Ben – Annahme)*
- **NFR4**: Antwortzeiten für UI‑Interaktionen ≤ 2 s, SAP‑Datenabruf ≤ 3 s, Unterstützung von 200 – 20 000 gleichzeitigen Nutzer*innen. *(Quelle: Performance & Skalierbarkeit, Stakeholder‑Ben)*
- **NFR5**: Rate‑Limiting zum Schutz vor Missbrauch (z. B. 100 Anfragen/Minute pro Nutzer*in). *(Quelle: Rate‑Limiting & Missbrauchserkennung, Stakeholder‑Ben, Clara – Annahme)*
- **NFR6**: Tägliche Backups; Wiederherstellungszeit (RTO) ≤ 4 h für kritische Daten. *(Quelle: Backup‑ und Disaster‑Recovery‑Strategie, Stakeholder‑Ben, Clara – Annahme)*
- **NFR7**: Vollständiges Logging und Monitoring (Audit‑Trail, Fehler‑Logs, System‑Metriken) ohne personenbezogene Daten in Reporting‑KPIs. *(Quelle: Logging & Monitoring, Stakeholder‑Clara)*
- **NFR8**: Architektur muss horizontale Skalierbarkeit ermöglichen; Caching wird für SAP‑Daten eingesetzt, um Ausfallrisiken zu reduzieren. *(Quelle: Performance & Skalierbarkeit, Stakeholder‑Ben)*

## Constraints/Compliance

- **C1**: Hosting muss ausschließlich in der EU erfolgen (EU‑only Managed Services). *(Stakeholder‑Farid, Anna)*
- **C2**: Im MVP darf kein zentrales API‑Gateway verwendet werden (aufgrund 6‑Wochen‑Wartezeit); stattdessen Direktschnittstelle zum SAP. *(Stakeholder‑Farid, Ben)*
- **C3**: Keine Sonderrabatte im MVP; nur Standard‑Rabatte bis zur definierten Schwelle (noch nicht final festgelegt). *(Stakeholder‑Anna, Ben)*
- **C4**: Datenresidenz: Kundendaten und Logs dürfen nicht außerhalb der EU gespeichert werden. *(Stakeholder‑Clara, Farid)*
- **C5**: Double‑Opt‑In gemäß DSGVO muss im Anmeldeprozess implementiert sein. *(Stakeholder‑Clara)*
- **C6**: Datenschutz‑Prinzipien (Datenminimierung, Pseudonymisierung von Testdaten) sind einzuhalten. *(Stakeholder‑Clara, Ben)*
- **C7**: Budget für EU‑only Managed Services ist noch nicht quantifiziert; muss vor finaler Entscheidung geklärt werden. *(Stakeholder‑Farid, Anna)*

## Assumptions and Open Points

- **A1**: Pilot‑Kunde und Länderwahl (Deutschland vs. Schweiz) – Einfluss auf Währung, Datenschutz und Hosting‑Strategie ist noch offen. *(Stakeholder‑Anna, Eva, David)*
- **A2**: Schwelle für den Rabatt‑Freigabeprozess (15 % / 20 % / 30 %) und zugehörige Rollen‑Zuweisung sind noch zu definieren. *(Stakeholder‑Eva, Ben, Clara)*
- **A3**: Detailgrad der Backup‑ und Disaster‑Recovery‑Strategie für das MVP muss noch spezifiziert werden. *(Stakeholder‑Ben, Clara)*
- **A4**: Entscheidung über Support‑Prozess – ob ein einfaches Kontakt‑Formular ausreichend ist oder ein Ticket‑System implementiert werden muss. *(Stakeholder‑David, Clara)*
- **A5**: Retention‑ und Lösch‑Regeln für Angebote, Rechnungen und Log‑Daten (z. B. Aufbewahrungsfristen) sind noch zu bestimmen. *(Stakeholder‑Clara, Eva)*
- **A6**: Konkrete Parameter für Rate‑Limiting und Missbrauchserkennung (Schwellenwerte, Algorithmen) müssen noch festgelegt werden. *(Stakeholder‑Ben, Clara)*
- **A7**: Umfang der Multi‑Währungs‑Unterstützung (EUR, CHF, evtl. USD) und Internationalisierung (Deutsch/Englisch) ist noch nicht final definiert. *(Stakeholder‑Alle)*
- **A8**: Kosten‑Schätzung für EU‑only Managed Services ist ausstehend. *(Stakeholder‑Farid, Anna)*
- **A9**: Vorgehen mit SAP‑Testdaten (pseudonymisiert vs. synthetisch) muss noch abgestimmt werden. *(Stakeholder‑Clara, Ben)*

## Traceability

| Requirement ID | Beschreibung | Quelle / Stakeholder |
|----------------|--------------|----------------------|
| FR1 | Login mit E‑Mail/Passwort + Double‑Opt‑In | Projektziel, Anna |
| FR2 | Rollen‑ und Berechtigungskonzept (Admin, Sales, Kunde) | Rollen & Verantwortungsbereiche, Anna, Ben |
| FR3 | Angebotserstellung ohne Sonderrabatte | MVP‑Umfang, Anna, Ben |
| FR4 | Angebotseinblick & Bestellauslösung durch Kunde | Projektziel, Anna |
| FR5 | Rechnungs‑PDF‑Download | MVP‑Umfang, Anna |
| FR6 | SAP‑Leseschnittstelle für Produkt‑/Preisdaten | SAP‑Integration, Ben |
| FR7 | Basis‑Audit‑Trail | Basis‑Audit‑Trail, Anna, Clara |
| FR8 | Double‑Opt‑In für DSGVO | DSGVO / Compliance, Clara |
| FR9 | PDF‑Export mit rechtlichen Fußnoten | Dokumente & Templates, Eva |
| FR10 (opt) | Push‑Notifications | Fachliche Themen, Alle |
| NFR1 | EU‑only Managed Hosting | Constraints/Compliance, Farid, Anna |
| NFR2 | DSGVO‑Konformität | DSGVO / Compliance, Clara |
| NFR3 | Verfügbarkeit ≥ 99 % | Security vs. MVP‑Diskussion, Clara, Ben |
| NFR4 | Performance‑Ziele (Antwortzeit, Nutzerzahl) | Performance & Skalierbarkeit, Ben |
| NFR5 | Rate‑Limiting (100 req/min) | Rate‑Limiting, Ben, Clara |
| NFR6 | Backup & DR (tägliche Backups, RTO ≤ 4 h) | Backup‑Strategie, Ben, Clara |
| NFR7 | Logging & Monitoring ohne personenbezogene Daten | Logging, Clara |
| NFR8 | Skalierbarkeit & Caching für SAP‑Daten | Performance, Ben |
| C1 | EU‑only Hosting | Constraints, Farid, Anna |
| C2 | Kein API‑Gateway im MVP | API‑Gateway Warteliste, Farid, Ben |
| C3 | Keine Sonderrabatte im MVP | MVP‑Umfang, Anna, Ben |
| C4 | Datenresidenz EU | DSGVO, Clara, Farid |
| C5 | Double‑Opt‑In Umsetzung | DSGVO, Clara |
| C6 | Datenminimierung & Pseudonymisierung | DSGVO, Clara, Ben |
| C7 | Kosten‑Unsicherheit EU‑Hosting | Kosten‑Schätzung, Farid, Anna |
| A1‑A9 | Offene Punkte und Annahmen (siehe Abschnitt) | Diverse Stakeholder |

