# Architektur‑Überblick (MVP) – Kundenportal

## 1. Systemkontext
Das **Kundenportal** ist die zentrale Anlaufstelle für Kunden (Rolle *Kunde*) und das Sales‑Team (Rolle *Sales*). Es stellt folgende Hauptfunktionen bereit:
- Authentifizierung (E‑Mail + Passwort) mit Double‑Opt‑In.  
- Angebotserstellung / Anzeige, basierend auf Produkt‑ und Preis‑Daten aus dem **SAP‑System** (lesender Zugriff).  
- Rechnungs‑ und Bestellungs‑Download (PDF).  
- Rollen‑basiertes UI (Admin, Sales, Kunde) und minimaler Audit‑Trail.

**Externe Schnittstellen**
- **SAP ERP** (Lesender API‑Zugriff für Produkt‑/Preis‑Daten).  
- **Managed Identity Provider** (optional, SSO – *nicht* Teil des MVP).  
- **E‑Mail‑Service** (Double‑Opt‑In‑Bestätigung, Benachrichtigungen).  
- **Monitoring‑/Logging‑Service** (technisches Logging, getrennt von Audit‑Logging).  
- **Backup‑Storage** (EU‑Only, für Datenbank‑Backups).  

**Betroffene Stakeholder**
- Product Owner (Anna) – definiert MVP‑Scope und Zeitplan.  
- Architekt / Backend (Ben) – bestimmt API‑Layer, Integration zu SAP.  
- Datenschutz (Clara) – legt DSGVO‑Pflichten fest.  
- Support (David) – arbeitet mit dem Kontakt‑Formular.  
- Finance (Eva) – definiert Rabatt‑ und PDF‑Anforderungen.  
- IT Operations (Farid) – stellt Managed Services (Hosting, Monitoring) bereit.

## 2. Wichtige Komponenten (MVP)
| Komponente | Aufgabe | Technische Details (keine konkreten Produkte) |
|------------|--------|-----------------------------------------------|
| **Web‑Frontend** | UI für Kunden & Sales (Login, Angebotserstellung, Rechnungs‑Download). | Responsive Web‑App, statisch gehostet (z. B. CDN), greift über HTTPS auf das API‑Gateway zu. |
| **API‑Gateway / API‑Layer** | Exponiert REST‑Endpunkte für Frontend, schützt Backend, führt Auth‑Check (OAuth‑2.0). | Eigenständige API‑Instanz (Managed Service), OAuth‑Token‑Validierung, Rate‑Limiting (Basis). |
| **Auth‑Service** | Benutzer‑Management, Double‑Opt‑In‑Workflow, Passwort‑Hashing. | Service‑only (keine IAM‑Integration). TLS‑gesichert, speichert nur notwendige Identitätsdaten (E‑Mail, gehashte Passwörter). |
| **Offer‑Service** | Geschäftslogik für Angebotserstellung, Nutzung von SAP‑Daten, Audit‑Log. | Liest Produkt‑/Preis‑Daten aus SAP, erzeugt Angebot‑Entität, schreibt minimalen Audit‑Eintrag (Wer, wann, was). |
| **Invoice‑Service** | Zugriff auf Rechnungs‑ und Bestellungs‑Dokumente, PDF‑Generierung. | Holt Rechnungen aus SAP (oder aus einem Dateispeicher), erzeugt PDF on‑the‑fly, loggt Zugriff im Audit‑Log. |
| **SAP‑Adapter** | Wrapper für SAP‑REST‑/SOAP‑Schnittstelle (nur Leserechte). | Abstract‑Layer, kann später auf Schreibrechte erweitert werden. |
| **Audit‑Log** | Revisionssichere Aufzeichnung kritischer Aktionen (Angebot erstellt/geändert, Rechnung heruntergeladen). | Separate Log‑Store, keine personenbezogenen Daten im technischen Log, Aufbewahrung nach Retention‑Policy. |
| **Backup‑Service** | Periodische Sicherung der Datenbank und des Audit‑Logs. | EU‑Only Storage, tägliche Snapshots, Wiederherstellungs‑SLAs definiert. |
| **Monitoring / Metrics** | System‑Health, API‑Aufruf‑Statistiken, ohne personenbezogene Daten. | Exportiert anonymisierte Metriken, integriert mit zentralem Monitoring‑Tool. |
| **Contact‑Formular** (Support) | Eingabeformular für Kunden‑Anfragen (ohne Ticket‑Persistenz). | Sendet E‑Mail an Support‑Team, verknüpft optional mit Kundendaten‑ID (nur lesend). |

## 3. Schnittstellen und Integrationspunkte
1. **Frontend ↔ API‑Gateway** – HTTPS, OAuth‑Bearer‑Token, JSON‑Payloads.  
2. **API‑Gateway ↔ Auth‑Service** – Login‑Endpoint (`/auth/login`), Double‑Opt‑In‑Bestätigung (`/auth/verify`).  
3. **API‑Gateway ↔ Offer‑Service** – CRUD‑Endpunkte `/offers/*`, intern Aufruf des SAP‑Adapters für Produkt‑/Preis‑Daten.  
4. **API‑Gateway ↔ Invoice‑Service** – `/invoices/*`, ruft Rechnungen aus SAP oder Dateispeicher ab.  
5. **Offer‑Service ↔ SAP‑Adapter** – Lesender Aufruf (`GET /sap/products`, `GET /sap/prices`).  
6. **Invoice‑Service ↔ SAP‑Adapter** – Lesender Aufruf für Rechnungsinformationen.  
7. **Auth‑Service ↔ E‑Mail‑Service** – Versand von Double‑Opt‑In‑Links.  
8. **Audit‑Log ↔ Backup‑Service** – Regelmäßige Sicherung der Log‑Daten.  
9. **Monitoring ↔ API‑Gateway / Services** – Export von Metriken (Request‑Count, Error‑Rate) ohne personenbezogene Daten.  
10. **Contact‑Formular ↔ E‑Mail‑Service** – Weiterleitung der Support‑Anfrage an Support‑Mailbox.

## 4. Daten‑ und Sicherheitsaspekte (MVP‑Scope)
- **Datenschutz (DSGVO)**: Daten werden ausschließlich in einer EU‑Only Managed‑DB gespeichert. Double‑Opt‑In ist Pflicht. Persönliche Daten (Kunden‑ID, Name, E‑Mail) werden nur im Auth‑ und Offer‑Service gehalten; technische Logs enthalten keine personenbezogenen Daten.  
- **Transport‑Security**: Alle Kommunikation über TLS 1.2 +; kein Plain‑Text.  
- **At‑Rest‑Encryption**: Managed DB verschlüsselt (vom Provider).  
- **Access‑Control**: Rollen‑basiertes RBAC (Admin, Sales, Kunde). Jeder Service prüft die Rolle aus dem OAuth‑Token.  
- **Audit‑Logging**: Minimaler, unveränderlicher Log‑Eintrag für jede kritische Aktion (Angebot erstellt/geändert, Rechnung heruntergeladen).  
- **Backup & DR**: Tägliche Snapshots, Aufbewahrung mindestens 30 Tage, Wiederherstellungstests im Sprint‑2.  
- **Rate‑Limiting**: Grund‑Rate‑Limit (z. B. 100 Requests/min pro Nutzer) im API‑Gateway, um Missbrauch zu reduzieren.  
- **Secrets‑Management**: Zugangsdaten zu SAP, E‑Mail‑Service und DB werden über ein Managed Secrets‑Store gehandhabt – nicht im Code.  
- **Monitoring**: Nur anonymisierte Metriken, getrennte Streams für Application‑Logs und Audit‑Logs, Aufbewahrung nach interner Policy.

## 5. Offene Architekturentscheidungen (MVP‑Einschränkungen)
| Entscheidung | Offene Fragen / Unsicherheiten | Konsequenz für MVP |
|------------|-------------------------------|-------------------|
| **SSO‑Integration** | Azure AD / Google ID Provider – Aufwand vs. Nutzen. | Nicht im MVP, Entscheidung für spätere Phase. |
| **API‑Gateway‑Provider** | Eigenständige Managed API‑Instanz vs. zentrales Unternehmens‑Gateway (6‑Wochen‑Warteliste). | MVP nutzt eigenständige Instanz; Migration später geplant. |
| **Managed DB‑Produkt** | Welche konkrete Managed‑DB (PostgreSQL, MySQL, etc.) wird gewählt? | Auswahl bis Sprint‑1‑Ende; Architektur bleibt abstrakt. |
| **Caching‑Strategie** | Ob Produkt‑/Preis‑Daten zwischenzeitlich gecached werden dürfen (Datenminimierung). | Keine Kundendaten‑Caches im MVP; optionales Produkt‑Cache nur für Performance. |
| **Mehrwährung (CHF)** | Ob die Schweiz‑Pilot‑Kunden wirklich CHF benötigen. | MVP: EUR‑Only; CHF optional nach Pilot‑Entscheidung. |
| **Backup‑Retention‑Policy** | Wie lange müssen Angebote/Bestellungen aufbewahrt werden (gesetzliche Vorgaben). | Minimaler Retention‑Plan (z. B. 2 Jahre für Rechnungen) definiert, detaillierte Policy später. |
| **Support‑Ticket‑System** | Vollständiges Ticket‑Tool vs. simples Kontakt‑Formular. | MVP = Kontakt‑Formular, später Ticket‑System. |
| **SAP‑Schreib‑Zugriff** | Ob das Portal später Bestellungen zurück in SAP schreiben soll. | MVP nur lesend; Schreib‑Zugriff als zukünftiger Scope. |
| **Internationalisierung** | Unterstützung weiterer Sprachen/Länder (USA, weitere EU‑Länder). | MVP: DE + EN; weitere Sprachen später. |

## 6. Zusammenfassung
Der vorgestellte Architektur‑Entwurf konzentriert sich auf ein **minimal funktionsfähiges Kundenportal** innerhalb eines **8‑Wochen‑MVP**. Er erfüllt die zwingenden Kernanforderungen (Login, Angebotserstellung, Rechnungs‑Download, Rollen‑ und Audit‑Mechanismen) und berücksichtigt dabei die wichtigsten **DSGVO‑ und Sicherheitsaspekte**. Alle nicht unbedingt notwendigen, aber diskutierten Erweiterungen (SSO, Sonderrabatte, umfassendes Support‑Ticket‑System, vollständige API‑Gateway‑Integration, Mehrwährung, vollständige Backup‑Policy) werden bewusst als **offene Entscheidungen** gekennzeichnet und sollen in späteren Projektphasen adressiert werden.

*Dieses Dokument basiert ausschließlich auf den in `runs/phase2_1/20260528_130843_589104/state/context.md`, `docs/requirements.md` und `docs/risks.md` enthaltenen Informationen.*