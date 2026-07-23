# Architekturüberblick – Kundenportal MVP

## Systemkontext
Das System dient der schnellen Angebotserstellung und Verwaltung von Bestellungen/Rechnungen über ein **Web‑First Kundenportal**. Es muss EU‑Only gehostet werden, DSGVO‑konform sein und lesend auf ein **SAP‑System** zugreifen. Nutzer (Kunden, Sales, Admin) interagieren über das Portal, während interne Prozesse (Support, Finance) ebenfalls über das System bedient werden.

## Wichtige Komponenten
| Komponente | Beschreibung | Relevante Anforderungen |
|------------|--------------|------------------------|
| **Web‑Frontend** | Responsives Web‑UI (React/Angular) – MVP fokussiert auf Browser, Mobile optional später. | Login, Rollen‑UI, Angebot‑/Rechnungsanzeige, PDF‑Download, Support‑Formular |
| **Auth‑Service** | Authentifizierung via E‑Mail/Passwort, Double‑Opt‑In, OAuth2‑Token‑Ausgabe. | FR‑1, NFR‑7, DSGVO‑Konformität |
| **API‑Gateway** (optional) | Eingangsfilter für alle internen APIs, zentraler Ort für Rate‑Limiting, Monitoring. | NFR‑7, Risiko Rate‑Limiting, Sicherheit |
| **SAP‑Read‑Adapter** | Lesender Adapter zum SAP‑System, inkl. Caching‑Layer für Produkt‑/Preis‑Daten. | FR‑5, C‑4, Risiko SAP‑Verfügbarkeit |
| **Angebots‑ & Rechnungsservice** | Business‑Logik zur Erstellung, Anzeige und PDF‑Export von Angeboten/Rechnungen. | FR‑3, FR‑4, PDF‑Export, KPI‑Tracking |
| **Audit‑Log‑Service** | Unveränderliche, getrennte Speicherung von Audit‑Einträgen (Login, Datenänderungen, Angebotserstellung). | FR‑6, NFR‑1, Risiko Audit‑Log‑Integrität |
| **PDF‑Generator** | Erzeugt PDF‑Dokumente aus Angebots‑ bzw. Rechnungsdaten. | FR‑8 |
| **Monitoring & Logging** | Technische Logs (ELK/AWS CloudWatch) getrennt von Audit‑Logs, Alerts, Dashboard. | NFR‑6 |
| **Backup & DR Service** | Tägliche Backups, 4‑Stunden RTO, Aufbewahrung von 30 Tagen (Produktivdaten) – noch zu detailieren. | NFR‑5, Risiko Backup & DR |
| **Support‑Kontakt** | Einfaches E‑Mail‑basiertes Formular (später Ticket‑System). | FR‑7 |

## Schnittstellen / Integrationspunkte
- **Frontend ↔ Auth‑Service**: JSON‑API über HTTPS (OAuth2 Token). 
- **Frontend ↔ API‑Gateway**: REST‑Endpoints für alle Geschäftsservices.
- **API‑Gateway ↔ SAP‑Read‑Adapter**: SOAP/REST‑Aufrufe zum SAP‑Read‑Interface (Lesend).
- **API‑Gateway ↔ Angebots‑/Rechnungsservice**: interne Service‑zu‑Service Kommunikation (gRPC/REST).
- **Angebots‑/Rechnungsservice ↔ PDF‑Generator**: lokaler Aufruf via Bibliothek oder Mikroservice.
- **Audit‑Log‑Service ↔ Storage**: Immutable Storage (WORM‑Bucket) in EU‑Region.
- **Monitoring ↔ All Services**: Export von Metriken, Logs über zentralen Collector.
- **Backup Service ↔ Data Stores**: Daily snapshots, verschlüsselt, EU‑Only.

## Daten‑ und Sicherheitsaspekte
- **Datenresidenz**: Alle Daten (User‑Profile, Logs, Backups) werden ausschließlich in EU‑Rechenzentren gespeichert (Farid). 
- **Verschlüsselung**: TLS 1.2+ für alle Netzwerkverbindungen; ruhende Daten verschlüsselt (AES‑256). 
- **Authentifizierung & Autorisierung**: OAuth2 mit Rollen‑basierten Zugriffskontrollen (RBAC). 
- **Audit‑Logs**: Write‑Once‑Read‑Many (WORM) Speicher, digitale Signaturen, getrennt von technischen Logs. 
- **DSGVO‑Maßnahmen**: Double‑Opt‑In‑Workflow, automatisierte Lösch‑Jobs nach 30 Tagen (Ausnahme Buchhaltungsdaten), Datenminimierung. 
- **Backup & Disaster Recovery**: Tägliche verschlüsselte Backups, RTO ≤ 4 Stunden, Aufbewahrung 30 Tage (Produktivdaten) – weitere Details offen. 
- **Rate‑Limiting**: Grundlegendes Limit (z. B. 100 Requests/Min. pro Nutzer) über API‑Gateway – Detailausgestaltung noch offen.

## Offene Architekturentscheidungen
1. **Datenbank / Persistenz‑Technologie** – Welche Managed Service (z. B. PostgreSQL‑as‑a‑Service, Cloud‑SQL, NoSQL) wird genutzt, da keine eigene DB installiert werden soll?
2. **API‑Gateway Auswahl** – Entscheidung zwischen Cloud‑Provider‑Gateway (z. B. AWS API GW, Azure API Management) oder Open‑Source (Kong, Traefik). 
3. **Rate‑Limiting & Missbrauchserkennung** – Konkrete Mechanismen und Schwellenwerte müssen noch definiert werden.
4. **Backup‑ und DR‑Details** – RPO, Aufbewahrungsfristen für verschiedene Datenklassen (z. B. Buchhaltungsdaten) und Test‑Restore‑Prozesse.
5. **Support‑Ticket‑Lösung** – Ob ein leichtgewichtiges Ticket‑System (z. B. Jira Service Management) später integriert wird.
6. **Mobile‑Strategie** – Responsives Design vs. native App – Entscheidung nach MVP.
7. **SSO‑Integration** – Vorbereitung der Auth‑Komponente für spätere Azure AD/Google‑SSO‑Anbindung.

*Hinweis: Alle nicht eindeutig bestimmten Punkte sind als offene Entscheidungen im weiteren Projektverlauf zu klären.*
