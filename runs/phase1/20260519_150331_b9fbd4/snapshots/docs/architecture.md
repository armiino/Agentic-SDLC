# Architekturübersicht

## Überblick
Die Lösung besteht aus einer **Web‑basierten Kundenplattform** (Responsive UI) mit optionaler nativer Mobile‑App‑Erweiterung (MVP‑Phase nicht zwingend). Das System wird über einen **API‑Layer** (REST) mit dem bestehenden **SAP‑Backend** verbunden und nutzt **Managed Services** für Datenhaltung und Hosting (EU‑Region).

## Komponenten
| Komponente | Beschreibung | Technologie‑Optionen |
|------------|--------------|----------------------|
| **Frontend** | Kundenportal für Bestellungs‑ und Rechnungs‑Ansicht, Angebotserstellung, Login. | React / Angular (SPA) – responsive Design. Optional: React‑Native für Mobile. |
| **Auth Service** | Authentifizierung per E‑Mail/Passwort, optional SSO (Azure AD, Google). | Auth0, Azure AD B2C, Keycloak (OAuth 2.0 / OpenID Connect). |
| **API‑Gateway / Service Layer** | Exponiert REST‑Endpoints für Frontend, SAP‑Integration, KPI‑Erfassung. | Spring Boot, Node.js (Express) – OAuth 2.0 Schutz. |
| **SAP‑Connector** | Synchronisation von Stammdaten, Produkt‑ und Angebotsinformationen. | SAP OData / RFC‑Adapter, Middleware (MuleSoft, SAP Cloud Platform Integration). |
| **Managed DB** | Persistenz von Nutzer‑ und Anwendungsdaten (keine eigene DB‑Instanz). | Azure SQL Database, AWS RDS (EU‑Region), PostgreSQL‑Managed. |
| **Logging & Audit** | Zentralisiertes Log‑Management, Änderungs‑Audit‑Trail. | Elastic Stack (ELK), Azure Monitor, Splunk (EU). |
| **Backup / DR** | Regelmäßige Snapshots, Wiederherstellung innerhalb von 4 h. | Managed Service Backup (Azure Backup, AWS Backup). |
| **KPI‑Collector** | Erfassung von Conversion‑Rate, Zeit‑bis‑Angebot. | Light‑Weight Event‑Collector, Azure Application Insights oder Open‑Telemetry. |
| **Hosting / Infrastruktur** | Cloud‑Umgebung, EU‑only Region, skalierbar. | Azure West Europe / Germany, AWS EU‑Central‑1. |

## Datenfluss (high‑level)
1. **User** greift über Browser (oder Mobile) auf das Frontend zu.
2. Authentifizierung erfolgt über **Auth Service** (E‑Mail/Passwort oder SSO).
3. Nach erfolgreichem Login ruft das Frontend über das **API‑Gateway** Daten ab (Bestellungen, Rechnungen, Produkte).
4. Das **API‑Gateway** delegiert bei Bedarf an den **SAP‑Connector**, der Daten aus SAP holt.
5. Änderungen (z. B. neue Angebote) werden im **Managed DB** gespeichert und über das **Logging‑System** audit‑geprüft.
6. KPI‑Events werden an den **KPI‑Collector** gesendet.
7. Alle Komponenten laufen in einer **EU‑only Cloud‑Region**, Backups werden regelmäßig erstellt.

## Nicht‑funktionale Aspekte
- **Sicherheit**: TLS 1.2+ für alle Verbindungen, OAuth 2.0 für API, Rollen‑basiertes RBAC im Auth‑Service, Audit‑Logs.
- **Datenschutz**: Datenhaltung in EU, Double‑Opt‑In bei Registrierung, Lösch‑Endpoint für Nutzer‑Daten, regelmäßige DSGVO‑Audits.
- **Skalierbarkeit**: Horizontal skalierbare Frontend‑ und API‑Instanzen, Managed DB mit automatischer Skalierung, Cloud‑Load‑Balancer.
- **Verfügbarkeit**: 99,5 % SLA durch Multi‑AZ‑Deployment, automatisches Failover, Backup‑Strategie.
- **Wartbarkeit**: Dokumentierte OpenAPI‑Spezifikation, Infrastruktur‑als‑Code (Terraform), klare Trennung von Frontend, API und Integrations‑Layer.

## MVP‑Scope (8 Wochen)
- Responsive Web‑Portal (Login, Rollen, Bestellungs‑/Rechnungs‑Ansicht, Angebotserstellung).
- API‑Layer mit Grund‑Endpoints und SAP‑Read‑Only‑Integration.
- Auth Service mit E‑Mail/Passwort (SSO optional, später).
- Grundlegendes Logging & Audit‑Trail.
- DSGVO‑Basis (Double‑Opt‑In, Lösch‑Endpoint, EU‑Hosting).
- Backup‑Setup (Managed Service).
- Minimaler KPI‑Collector (Conversion Rate). 

## Weiterführende Schritte nach MVP
- Native Mobile‑App, erweiterte SSO‑Optionen, Push‑Notifications, erweiterte Analytics, vollständiges Security Review, detailliertes Rollen‑ und Berechtigungskonzept.
