## Functional Requirements
- **Login**: Das System muss eine Authentifizierung per E‑Mail und Passwort mit Double‑Opt‑In unterstützen. (Stakeholder: Anna, Clara)
- **Rollenbasierte Benutzeroberfläche**: Das Portal stellt drei Rollen (Admin, Sales, Kunde) mit entsprechenden Basisberechtigungen bereit. (Stakeholder: Anna)
- **Angebotserstellung**: Vertriebsmitarbeiter können Angebote erstellen, indem das System Daten (Produkt, Preis, Rabatt) aus SAP **lesend** abruft und ein PDF‑Export ermöglicht. (Stakeholder: Ben, Eva)
- **Rechnungs‑Download**: Kunden können ihre Rechnungen im PDF‑Format herunterladen. (Stakeholder: David)
- **Audit‑Log**: Das System protokolliert minimal, welcher Nutzer welche Aktionen (Anzeigen, Ändern) durchgeführt hat. (Stakeholder: Clara)
- **Kontaktformular**: Ein einfaches Kontaktformular ermöglicht Kunden, Support‑Anfragen zu stellen (ohne Ticket‑System). (Stakeholder: David)
- **Backup & Disaster Recovery**: Automatisierte tägliche Backups und ein Wiederherstellungs‑Plan müssen vorhanden sein. (Stakeholder: Clara)

## Non-functional Requirements
- **Performance**: Das System muss 200 bis 20.000 gleichzeitige Nutzer unterstützen, mit Pagination für Rechnungs‑Downloads. (Stakeholder: Ben)
- **Sicherheit**: Durchgängiges TLS, Double‑Opt‑In, minimale Audit‑Logs ohne personenbezogene Daten, und DSGVO‑Konformität (Logging, Löschkonzept‑Hinweis). (Stakeholder: Clara)
- **Verfügbarkeit**: 99,5 % monatliche Verfügbarkeit, betrieben auf einem EU‑Only Managed Hosting Service. (Stakeholder: Farid)
- **Skalierbarkeit**: Grundlegende Skalierbarkeit für wachsende Nutzerzahlen ohne Änderungen an der Kernarchitektur. (Stakeholder: Ben)
- **Compliance**: EU‑Only Datenhosting, Einhaltung DSGVO, Backup‑ und Disaster‑Recovery‑Richtlinien. (Stakeholder: Clara, Farid)
- **Internationalisierung**: UI in Deutsch und Englisch, primäre Währung EUR; optional Hinweis auf CHF für einen Schweizer Pilot‑Kunden. (Stakeholder: Anna)

## Constraints/Compliance
- **EU‑Only Managed Hosting** muss verwendet werden (Stakeholder: Farid).
- **Kein Schreibzugriff auf SAP** im MVP; nur Leseschnittstelle. (Stakeholder: Ben).
- **Keine vollständige SSO‑Integration** im MVP (nur als zukünftige Erweiterung gekennzeichnet). (Stakeholder: Anna).
- **Kein API‑Gateway** im MVP aufgrund einer 6‑Wochen‑Warteliste; ggf. leichter Proxy als Alternative. (Stakeholder: Farid).
- **Keine Mobile App, Push‑Notifications, komplexe Rabatt‑Freigabe‑Workflows, Ticket‑System, mehrsprachige UI (außer DE/EN) und Multi‑Währungs‑Support (außer EUR + optional CHF)** im MVP. (Stakeholder: Anna).
- **DSGVO‑Grundlagen**: Double‑Opt‑In, Logging‑Richtlinien, Hinweis auf Löschkonzept. (Stakeholder: Clara).

## Assumptions and Open Points
- **Security Review**: Es wird ein minimaler Security‑Check angenommen, genaue Vorgehensweise ist noch offen (Unsicherheit). 
- **Budget für Datenbank**: Annahme, dass ein kostengünstiger EU‑Only Managed DB‑Service gewählt wird; konkrete Lösung fehlt.
- **API‑Gateway**: Entscheidung über Ersatz‑Proxy noch offen.
- **Rabatt‑Freigabe‑Workflow**: Wird im MVP ausgeschlossen; Risiko für Finanzabteilung bleibt bestehen.
- **Support‑Prozess**: Kontaktformular wird genutzt, Datenschutz‑Konformität noch zu prüfen.
- **Pilot‑Kunde & Währung**: Entscheidung über CHF‑Unterstützung noch offen; wirkt sich auf Internationalisierung aus.
- **Hosting‑Kosten**: Kosten‑Schätzung für EU‑Only Hosting fehlt.
- **Retention‑Policy**: Konkrete Aufbewahrungs‑ und Löschregeln müssen definiert werden.
- **Testdaten**: Nutzung pseudonymisierter Testdaten wird angenommen, jedoch fehlen Details zur Umsetzung.
- **Backup‑Aufwand**: Der erforderliche Aufwand ist nicht im aktuellen Aufwandspuffer enthalten.

## Traceability
- **FR1 (Login)** → Anna, Clara (Projektziel, DSGVO).
- **FR2 (Rollenmodell)** → Anna (Projektziel).
- **FR3 (Angebotserstellung)** → Ben, Eva (Fachliche Themen, Finanz).
- **FR4 (Rechnungs‑Download)** → David (Support).
- **FR5 (Audit‑Log)** → Clara (Compliance).
- **FR6 (Kontaktformular)** → David (Support).
- **FR7 (Backup/DR)** → Clara (Compliance).
- **NFR1 (Performance)** → Ben (Performance‑ und Skalierbarkeits‑Anforderung).
- **NFR2 (Security/TLS/DSGVO)** → Clara (Compliance).
- **NFR3 (Verfügbarkeit)** → Farid (Hosting).
- **NFR4 (Scalability)** → Ben.
- **NFR5 (Internationalisierung)** → Anna.
- **C1 (EU‑Only Hosting)** → Farid.
- **C2 (Kein SAP‑Write)** → Ben.
- **C3 (Kein SSO im MVP)** → Anna.
- **C4 (Kein API‑Gateway)** → Farid.
- **C5 (DSGVO‑Grundlagen)** → Clara.
- **Open1 (Security Review)** → Clara.
- **Open2 (DB Budget)** → Ben, Farid.
- **Open3 (API‑Gateway Ersatz)** → Farid.
- **Open4 (Rabatt‑Freigabe)** → Eva.
- **Open5 (Support‑Datenschutz)** → David, Clara.
- **Open6 (Pilot‑Währung)** → Anna.
- **Open7 (Hosting‑Kosten)** → Farid, Anna.
- **Open8 (Retention‑Policy)** → Clara.
- **Open9 (Testdaten‑Pseudonymisierung)** → Ben, Farid.
- **Open10 (Backup‑Aufwand)** → Clara.
