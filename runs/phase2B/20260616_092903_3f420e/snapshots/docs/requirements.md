## Functional Requirements
- **Login**: Kunden können sich mit E‑Mail und Passwort anmelden. Der Login muss ein Double‑Opt‑In Verfahren enthalten. (Stakeholder: Anna, Ben)
- **Optionales SSO**: Das System unterstützt optional SSO via Azure AD oder Google, jedoch ist SSO im MVP nicht verpflichtend. (Stakeholder: Ben)
- **Angebotserstellung**: Vertriebsmitarbeiter können Angebote anlegen, wobei Produkt‑ und Preisinformationen aus SAP im Lesemodus gezogen werden. (Stakeholder: Anna, Ben)
- **Angebots‑Workflow**: Das Angebot durchläuft die Stati Draft, Pending‑Approval, Approved, Sent, Accepted, Rejected. Für Rabatte > 15 % ist ein Freigabe‑Workflow vorgesehen (geplant für Phase 2, nicht MVP). (Stakeholder: Eva)
- **Rollen‑ und Berechtigungs‑Modell**: Es gibt mindestens drei Rollen – Admin, Sales, Kunde – mit entsprechender Zugriffskontrolle auf Funktionen. (Stakeholder: Anna, Ben)
- **Rechnungs‑ und Bestellungs‑Ansicht**: Kunden können Rechnungen und Bestellungen als PDF herunterladen. (Stakeholder: Anna, David)
- **Audit‑Trail**: Das System protokolliert, welcher Nutzer welche Änderungen oder Ansichten durchgeführt hat (ohne personenbezogene Daten in technischen Logs). (Stakeholder: Clara, Ben)
- **Logging**: Technische Logs werden geführt, jedoch keine personenbezogenen Daten darin gespeichert. (Stakeholder: Clara)
- **Backup & Disaster Recovery**: Automatisierte tägliche Backups und ein DR‑Plan müssen vorhanden sein, um Produktionsdaten zu schützen. (Stakeholder: Farid)

## Non-functional Requirements
- **Performance**: Das System muss 200 – 20.000 gleichzeitige Nutzer unterstützen, Pagination und Download‑Limits nutzen, um Lastspitzen zu bewältigen. (Stakeholder: Ben)
- **Verfügbarkeit**: Ziel‑Verfügbarkeit von 99,5 % im Produktionsbetrieb. (Stakeholder: Farid)
- **Sicherheit**: TLS‑Verschlüsselung für alle Datenübertragungen, Rate‑Limiting (minimal‑Implementierung im MVP) und Secrets‑Management für Credentials. (Stakeholder: Clara)
- **Datenschutz / DSGVO**: Double‑Opt‑In, Lösch‑ und Audit‑Konzepte, Datenresidenz ausschließlich in der EU, Trennung von technischen Logs und personenbezogenen Audit‑Logs. (Stakeholder: Clara)
- **Hosting**: Managed Service Hosting innerhalb der EU (EU‑Only), Kosten‑ und Vertrags‑Unsicherheit beachten. (Stakeholder: Farid)
- **Monitoring**: Grundlegendes Monitoring ohne personenbezogene Daten, inkl. Alarmierung bei Ausfällen. (Stakeholder: Ben)
- **Scalability**: Architektur muss horizontale Skalierung ermöglichen, jedoch komplexe Skalierbarkeits‑Architektur ist für das MVP nicht vorgesehen. (Stakeholder: Ben)

## Constraints/Compliance
- **MVP‑Umfang**: Keine vollständige SSO‑Integration, keine Mobile‑App, kein Mehrwährungs‑Support, kein umfangreicher Freigabe‑Workflow (> 15 % Rabatt), kein Ticket‑System, keine umfangreiche Analytics. (Stakeholder: Anna)
- **SAP‑Zugriff**: Nur lesender Zugriff auf Produkt‑ und Preisdaten im MVP; Schreibzugriff erst in späteren Phasen. (Stakeholder: Ben)
- **EU‑Only Datenresidenz**: Alle Daten müssen in EU‑Rechenzentren gespeichert werden (inkl. Backup). (Stakeholder: Farid, Clara)
- **Compliance**: Einhaltung DSGVO‑Vorgaben, insbesondere bei Logging, Audit‑Trail und Lösch‑konzepten. (Stakeholder: Clara)
- **Budget**: Managed Services sollen kostengünstig sein; konkrete Kosten‑ und Vertragsdetails sind noch offen. (Stakeholder: Farid)

## Assumptions and Open Points
- **Finaler MVP‑Umfang**: Es bleibt unklar, ob das optionale SSO letztlich im MVP enthalten sein darf. (Offene Klärung)
- **Hosting‑Provider**: Auswahl und Kosten für den EU‑Only Managed Service Provider sind noch nicht definiert. (Offene Klärung)
- **API‑Gateway‑Strategie**: Das geplante API‑Gateway befindet sich in einer Warteliste von 6 Wochen; es muss geprüft werden, ob eine Interim‑Proxy‑Lösung eingesetzt wird. (Offene Klärung)
- **Backup‑RPO/RTO**: Konkrete Werte für Recovery‑Point‑Objective und Recovery‑Time‑Objective fehlen noch. (Annahme)
- **Retention‑ und Lösch‑Policy**: Detaillierte Vorgaben für Aufbewahrung vs. Recht auf Vergessenwerden sind noch zu definieren. (Offene Klärung)
- **Rate‑Limiting**: Minimal‑Implementierung für MVP wird angenommen, genaue Schwellenwerte sind offen. (Annahme)
- **Preis‑ und Rabatt‑Logik**: Umgang mit nächtlichen Preis‑Updates (z. B. Cache‑Invalidierung) ist noch nicht festgelegt. (Offene Klärung)
- **Pilot‑Kunde**: Ob Deutschland oder Schweiz als Pilot‑Kunde dient, wirkt sich auf Währungs‑ und Datenschutz‑Spezifika aus. (Offene Klärung)

## Traceability
- **Login & Double‑Opt‑In** – Anna (Produkt), Ben (Technik)
- **SSO‑Option** – Ben (Technik)
- **Angebotserstellung mit SAP‑Daten** – Anna, Ben
- **Angebots‑Workflow & Rabatt‑Freigabe** – Eva (Finance)
- **Rollen‑ und Berechtigungs‑Modell** – Anna, Ben
- **Rechnungs‑/Bestellungs‑Download** – Anna, David (Support)
- **Audit‑Trail & Logging** – Clara (Compliance), Ben
- **Backup & DR** – Farid (Operations)
- **Performance & Skalierbarkeit** – Ben
- **Sicherheit (TLS, Rate‑Limiting, Secrets‑Management)** – Clara
- **Datenschutz / DSGVO** – Clara
- **Hosting & EU‑Only** – Farid
- **Monitoring** – Ben
- **Offene Punkte** – Alle Stakeholder in den jeweiligen Fachbereichen
