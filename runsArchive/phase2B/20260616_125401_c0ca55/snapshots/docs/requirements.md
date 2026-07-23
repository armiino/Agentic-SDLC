## Functional Requirements

- **Login und Authentifizierung**: Das System muss Kunden die Anmeldung per E‑Mail und Passwort ermöglichen. Beim ersten Login ist ein Double‑Opt‑In‑Verfahren (E‑Mail‑Bestätigung) erforderlich. (Quelle: Anna, Ben)
- **Rollen‑ und Berechtigungskonzept**: Unterstützt mindestens die Rollen **Admin**, **Sales**, **Kunde** (und optional **Support/Manager**) mit differenzierten Zugriffsrechten auf Portalfunktionen. (Quelle: Anna, Ben)
- **Angebotserstellung und -anzeige**: Kunden können im Portal Angebote einsehen, herunterladen (PDF) und den Status verfolgen. Angebote basieren auf Daten aus dem SAP‑Read‑Interface. (Quelle: Anna, Ben, Eva)
- **Bestell‑ und Rechnungsverwaltung**: Kunden sollen ihre Bestellungen und zugehörigen Rechnungen im Portal einsehen und als PDF herunterladen können. (Quelle: Anna, David)
- **SAP‑Read‑Integration**: Das System muss lesend auf SAP zugreifen können, um Produkt‑/Preisdaten, Stammdaten und Rabattlogik abzurufen. (Quelle: Ben)
- **Audit‑Trail / Audit‑Log**: Jeder relevanten Aktion (Login, Datenänderung, Angebotserstellung) muss ein unveränderbarer Audit‑Log zugeordnet werden. (Quelle: Clara, Ben)
- **Support‑Kontaktformular**: Kunden können über ein einfaches Formular Support‑Anfragen stellen; diese werden per E‑Mail an das Support‑Team weitergeleitet. (Quelle: David)
- **PDF‑Export**: Angebot‑ und Rechnungsdokumente können als PDF generiert und heruntergeladen werden. (Quelle: Anna, Eva)
- **Grundlegendes KPI‑Tracking**: Das System erfasst Conversion‑Rate und Zeit bis zur Angebotserstellung für Reporting‑Zwecke. (Quelle: Anna)

## Non-functional Requirements

- **DSGVO‑Konformität**: Das System muss Double‑Opt‑In, ein Löschkonzept, Audit‑Logs, Datenminimierung und Datenresidenz in der EU sicherstellen. (Quelle: Clara)
- **EU‑Only Hosting**: Alle Daten müssen ausschließlich in EU‑basierten Rechenzentren gespeichert werden. (Quelle: Farid)
- **Performance**: Angebotserstellung und Datenanzeige sollen innerhalb von 2 Sekunden nach Benutzerinteraktion reagieren. (Annahme: definiert in MVP‑Sprint)
- **Verfügbarkeit**: Systemverfügbarkeit von 99,5 % im Produktionsbetrieb, mit geplanten Wartungsfenstern von maximal 4 Stunden pro Monat. (Quelle: Ben)
- **Backup & Disaster Recovery**: Tägliche Backups mit einer Wiederherstellungszeit von maximal 4 Stunden; Aufbewahrungsfrist von 30 Tagen für produktive Daten. (Offener Punkt: Detail‑Definition)
- **Monitoring & Logging**: Technische Logs getrennt von Audit‑Logs; kontinuierliches Monitoring von Systemmetriken und automatisierte Alerts bei Fehlfunktionen. (Quelle: Ben)
- **Sicherheit**: Nutzung von HTTPS, OAuth 2.0 für API‑Zugriff, und optional API‑Keys; Rate‑Limiting zum Schutz vor Missbrauch (Mechanismus noch zu definieren). (Quelle: Ben)
- **Scalability**: Architektur muss horizontal skalieren können, um bis zu 10.000 gleichzeitige Nutzer zu unterstützen. (Annahme für MVP‑Erweiterung)

## Constraints/Compliance

- **Keine neue Datenbank**: Nutzung von bestehenden Managed Services, keine eigenständige Datenbankinstallation. (Quelle: Farid)
- **Kosten**: Managed Services sollen kostengünstig sein; EU‑Only Hosting kann höhere Kosten verursachen – muss im Budget berücksichtigt werden. (Quelle: Farid)
- **Security Review**: Der Security‑Review ist für 6 Wochen geplant und kollidiert potenziell mit der 8‑Wochen‑MVP‑Deadline. (Quelle: Ben)
- **SAP‑Verfügbarkeit**: SAP‑System ist kritische Abhängigkeit; es muss ein Caching‑Mechanismus mit Fallback‑Strategie geben, um SAP‑Ausfälle zu tolerieren. (Quelle: Ben)
- **Mobile vs. Web‑First**: Das MVP fokussiert sich auf eine Web‑First‑Lösung; Mobile wird als spätere Erweiterung definiert. (Quelle: Anna)
- **SSO / Identity Provider**: Azure AD/Google‑SSO ist ein Wunsch, wird aber im MVP nicht implementiert. (Quelle: Ben)
- **Mehrwährung / Internationalisierung**: Nur EUR und ggf. CHF im MVP; weitere Währungen und Länder werden später berücksichtigt. (Quelle: Anna)

## Assumptions and Open Points

- **Mobile Strategie**: Entscheidung über native Mobile‑App oder Responsive Design ist noch offen. (Offener Punkt)
- **SSO Integration**: Konkrete Entscheidung und Aufwand für Azure AD/Google‑SSO fehlen. (Offener Punkt)
- **Kosten für EU‑Only Managed Service Provider**: Noch nicht finalisiert. (Offener Punkt)
- **Detailierte Rabatt‑Freigabe‑Workflow** für >15 % Rabatt: Im MVP ausgeschlossen, später zu definieren. (Offener Punkt)
- **Support‑Ticket‑System**: Nutzung eines strukturierten Ticket‑Systems versus einfaches E‑Mail‑basiertes Verfahren ist noch offen. (Offener Punkt)
- **Backup‑Strategie & Disaster Recovery**: Konkrete Implementierung und Aufbewahrungsfristen müssen noch spezifiziert werden. (Offener Punkt)
- **API‑Gateway Verfügbarkeit & Rate‑Limiting**: Mechanismus und Integration in das System sind noch zu klären. (Offener Punkt)
- **Retention‑ und Löschkonzepte**: Konkretisierung der Aufbewahrungsfristen im Einklang mit gesetzlichen Vorgaben steht aus. (Offener Punkt)
- **Testdaten & Umgebungskonzept**: Nutzung von SAP‑Testsystemen mit echten Kundendaten birgt Datenschutz‑Risiken; Synthesedaten oder Pseudonymisierung muss noch definiert werden. (Offener Punkt)

## Traceability

- **FR‑1 Login & Double‑Opt‑In** – Anna (Projektziel), Ben (Technik). 
- **FR‑2 Rollen & Berechtigungen** – Anna, Ben. 
- **FR‑3 Angebotserstellung & PDF‑Export** – Anna, Eva. 
- **FR‑4 Bestell‑/Rechnungsanzeige** – Anna, David. 
- **FR‑5 SAP‑Read‑Integration** – Ben. 
- **FR‑6 Audit‑Trail** – Clara, Ben. 
- **FR‑7 Support‑Kontaktformular** – David. 
- **FR‑8 KPI‑Tracking** – Anna. 
- **NFR‑1 DSGVO‑Konformität** – Clara. 
- **NFR‑2 EU‑Only Hosting** – Farid. 
- **NFR‑3 Performance (≤2 s)** – Annahme basierend auf MVP‑Ziel. 
- **NFR‑4 Verfügbarkeit 99,5 %** – Ben. 
- **NFR‑5 Backup/DR** – Offener Punkt. 
- **NFR‑6 Monitoring & Logging** – Ben. 
- **NFR‑7 Sicherheit (HTTPS, OAuth2)** – Ben. 
- **NFR‑8 Skalierbarkeit** – Annahme für zukünftige Erweiterung. 
- **C‑1 Keine neue DB** – Farid. 
- **C‑2 Kosten EU‑Only Managed Services** – Farid. 
- **C‑3 Security Review Timing** – Ben. 
- **C‑4 SAP‑Caching & Fallback** – Ben. 
- **C‑5 Web‑First MVP** – Anna. 
- **C‑6 SSO ausgesetzt** – Ben. 
- **C‑7 Mehrwährung (EUR/CHF)** – Anna.
- **A‑1 Mobile‑Strategie** – Offener Punkt. 
- **A‑2 SSO Entscheidung** – Offener Punkt. 
- **A‑3 EU‑Only Provider Kosten** – Offener Punkt. 
- **A‑4 Rabatt‑Freigabe‑Workflow** – Offener Punkt. 
- **A‑5 Support‑Ticket‑Lösung** – Offener Punkt. 
- **A‑6 Internationalisierung** – Offener Punkt. 
- **A‑7 Backup & DR Details** – Offener Punkt. 
- **A‑8 API‑Gateway & Rate‑Limiting** – Offener Punkt. 
- **A‑9 Retention‑ und Löschkonzepte** – Offener Punkt. 
- **A‑10 Testdaten & Umgebung** – Offener Punkt.
