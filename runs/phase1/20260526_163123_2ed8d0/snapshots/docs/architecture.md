# Architekturübersicht

## Überblick
Die Lösung ist ein **Kundenportal** (Web‑App) mit einem **REST‑API‑Backend**. Das Backend stellt die Geschäftslogik bereit, greift lesend auf SAP zu und verwaltet Nutzer, Rollen, Angebote und Rechnungen. Alle Komponenten laufen auf einem **EU‑only Managed Service** (z. B. Kubernetes‑Cluster oder PaaS) – es wird keine eigene Datenbank betrieben, stattdessen wird ein Managed relationaler Service (z. B. PostgreSQL) genutzt, der bereits im EU‑Rechenzentrum gehostet wird.

## Komponenten
| Komponente | Aufgabe | Technologie (Beispiel) | Hinweis |
|------------|---------|-----------------------|--------|
| **Frontend** | Kunden‑ und Sales‑UI, Internationalisierung (de/en) | React / Angular, i18next | Läuft als statische Site auf CDN (EU) |
| **API Layer** | Exponiert REST‑Endpoints für Frontend, SAP‑Read‑Only, Auth & Authorisation | Node.js/Express oder Spring Boot, OAuth‑2 (Authorization Server) | Direktes OAuth, kein zentrales API‑Gateway im MVP |
| **Auth Service** | Nutzer‑Registrierung, Double‑Opt‑In, Login, optional SSO | Managed Identity Provider (z. B. Auth0, Azure AD B2C) – EU‑Region | SSO optional, nicht zwingend im MVP |
| **SAP Connector** | Liest Produkt‑, Preis‑ und Rabattdaten aus SAP (Read‑Only) | SAP OData / RFC Adapter | Fallback‑Verhalten bei SAP‑Ausfall definiert |
| **Offer Service** | Geschäftslogik für Angebotserstellung, PDF‑Export, Audit‑Log | Microservice (Node/Java) | Sonderrabatte >15 % nur mit Freigabe (nicht im MVP) |
| **Invoice Service** | Anzeige und Download von Rechnungen (PDF) | Microservice | Zugriff nur für berechtigte Rollen |
| **Audit Log Service** | Revisionssichere Aufzeichnung von Aktionen | Append‑only Log (z. B. CloudWatch Logs, ELK) | Keine personenbezogenen Daten in technischen Logs |
| **Backup Service** | Tägliche Snapshots des Managed DB und Storage | Managed Backup (z. B. AWS RDS Snapshots) | Aufbewahrung 7 Tage, Wiederherstellung <4 h |
| **Monitoring & Alerting** | System‑Health, Performance, Rate‑Limiting, Missbrauchserkennung | Prometheus + Grafana, Alerts via PagerDuty | Keine personenbezogenen Daten im Monitoring |
| **Contact Form Service** | Empfang von Support‑Anfragen per E‑Mail | Serverless Function (z. B. AWS Lambda) | Keine Persistenz, Hinweis auf Datenverarbeitung |

## Datenfluss (Kern‑Szenario)
1. **Registrierung** – Nutzer gibt E‑Mail ein, erhält Double‑Opt‑In‑Link, bestätigt und legt Passwort fest.
2. **Login** – JWT wird vom Auth Service ausgestellt, Frontend speichert Token.
3. **Angebot erstellen** – Frontend ruft `/offers` (POST) auf, API Layer ruft SAP‑Connector für Produkt‑/Preis‑Daten, erstellt Angebot, schreibt Eintrag in Offer Service DB und Audit Log.
4. **Rechnung einsehen** – Nutzer ruft `/invoices/{id}` (GET) auf, Invoice Service prüft Rolle, liefert PDF.
5. **Audit** – Jede Aktion (Create/Read/Update) wird an Audit Log Service gesendet (wer, wann, was).
6. **Backup** – Täglicher Snapshot des Managed DB, gespeichert im selben EU‑Rechenzentrum.
7. **Monitoring** – Metriken (Response‑Time, Error‑Rate, API‑Calls) werden gesammelt, Alerts bei Überschreitung von Rate‑Limits.

## Sicherheitsaspekte (MVP)
- **Transport Layer Security (TLS)** für alle Verbindungen.
- **OAuth‑2** mit scoped Tokens für API‑Zugriff.
- **Secrets Management** über Managed Service (z. B. Azure Key Vault, AWS Secrets Manager) – keine Secrets im Code‑Repository.
- **Audit‑Log** getrennt von technischen Logs, enthält nur Nutzer‑ID, Aktion, Timestamp.
- **Logging‑Richtlinie**: Keine PII in System‑Logs, nur anonymisierte IDs.
- **Backup‑Verschlüsselung**: Daten‑at‑Rest verschlüsselt (AES‑256).
- **Data Residency**: Alle Daten (DB, Backups, Logs) ausschließlich in EU‑Region gespeichert.

## Skalierbarkeit & Verfügbarkeit
- **Horizontal skalierbare Pods/Instances** hinter einem Load Balancer (EU‑Region).
- **Stateless API** – Session‑Daten im JWT, keine serverseitige Session.
- **Auto‑Scaling** basierend auf CPU/Request‑Rate.
- **Failover**: Mehrere Availability Zones, automatischer Neustart bei Node‑Ausfall.
- **Rate‑Limiting** (MVP): 100 API‑Calls/Minute pro Nutzer, 10 Rechnungs‑Downloads/Minute – implementiert im API Layer.

## Nicht im MVP (Future Enhancements)
- Vollständiges **API‑Gateway** mit zentralem Policy‑Engine.
- **SSO** für Azure AD / Google als Standard‑Login.
- **Support‑Ticket‑System** mit persistenter Speicherung.
- **Mehrwährungs‑Support** (CHF, USD) und **Internationalisierung** über weitere Sprachen.
- **Erweiterter Freigabe‑Workflow** für Sonderrabatte >15 %.
- **Analytics‑Platform** (z. B. Snowflake) für tiefere KPI‑Analyse.
- **Event‑Driven Architecture** für Echtzeit‑Sync mit SAP (Write‑Back).

---
*Dieses Architektur‑Dokument fasst den geplanten MVP‑Umfang zusammen, berücksichtigt die im Transkript genannten Constraints und legt die Basis für weitere Iterationen.*