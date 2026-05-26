# Architekturübersicht

## Überblick
Die Lösung besteht aus einer **Web‑First Kundenportal‑Applikation** (React/Angular) die über einen **API‑Gateway** mit einem **Backend‑Service** kommuniziert. Das Backend stellt die Geschäftslogik für Angebotserstellung, Rechnungs‑Download und SAP‑Integration bereit und ist über **OAuth 2.0** abgesichert. Daten werden in **Managed Services** (z. B. Azure SQL Database) gehostet, ausschließlich in EU‑Regionen, um DSGVO‑Konformität zu gewährleisten.

## Bausteine
| Baustein | Beschreibung | Technologie‑Optionen |
|----------|--------------|----------------------|
| **Frontend** | Kundenportal (Web) – responsive, später mobile App möglich. | React, Angular, Vue.js; Hosted on Azure Static Web‑Apps oder AWS S3+CloudFront (EU) |
| **API‑Gateway** | Zentraler Einstiegspunkt, Routing, Authentifizierung, Rate‑Limiting. | Azure API Management, AWS API Gateway, Kong |
| **Backend Service** | Geschäftslogik: Angebote, Rechnungen, Rollen, Logging. | .NET Core, Node.js (Express), Java Spring Boot – deployed as Container (Azure App Service / AWS ECS) |
| **Authentication / SSO** | Identity‑Provider‑Integration (Azure AD, Google). | Azure AD B2C, Auth0, Keycloak (OAuth 2.0 / OpenID Connect) |
| **SAP Integration** | Zugriff auf Stammdaten, Produkt‑ und Preisinformationen. | SAP OData/REST‑Connector, Middleware (MuleSoft, SAP CPI) |
| **Datenbank** | Persistenz für Nutzer, Rollen, Angebote, Logs. | Azure SQL Database (Managed), PostgreSQL (Azure Database for PostgreSQL) – EU‑Region |
| **Logging & Audit** | Änderungs‑ und Zugriffs‑Protokollierung. | Azure Monitor, ELK‑Stack, Splunk (EU) |
| **Backup / DR** | Regelmäßige Snapshots, Wiederherstellungs‑SLA < 4 h. | Azure Backup, AWS Backup – EU‑Region |
| **Monitoring & KPIs** | Erfassung von Conversion‑Rate, Zeit‑bis‑Angebot. | Azure Application Insights, Grafana + Prometheus |
| **Managed Services** | Keine eigene DB‑Server‑Installation, Nutzung von Cloud‑Managed‑Services. | Azure, AWS (EU) |

## Datenfluss (Kurz)
1. **User** greift über Browser auf das **Portal** zu → Authentifiziert über **OAuth** beim **Identity Provider**.
2. Auth‑Token wird an das **API‑Gateway** gesendet → Weiterleitung an **Backend Service**.
3. Backend ruft bei Bedarf **SAP** über OData‑Connector ab (Produkt‑/Preis‑Daten).
4. Ergebnisse (Angebote, Rechnungen) werden im **Datenbank**‑Service gespeichert.
5. **Logging/Audit** schreibt jede Änderung in den **Log‑Service**.
6. **Backup** erstellt periodisch Snapshots der Datenbank.
7. **Monitoring** sammelt KPI‑Daten und stellt sie im Dashboard bereit.

## Nicht‑funktionale Aspekte
- **Sicherheit**: TLS 1.2+, OAuth 2.0, Rollen‑basiertes Access‑Control, Audit‑Trail.
- **Skalierbarkeit**: Horizontal skalierbare Container, Auto‑Scaling über Cloud‑Provider.
- **Verfügbarkeit**: 99,5 % SLA durch Multi‑Region‑Deployment (innerhalb EU).
- **Compliance**: Datenhaltung ausschließlich in EU‑Regionen, DSGVO‑konforme Prozesse (Double‑Opt‑In, Lösch‑Konzept).
- **Performance**: Antwortzeit < 2 s für Kern‑APIs, Load‑Testing im MVP‑Umfeld.

## Architekturdiagramm (textuell)
```
[Browser] --> [Frontend (Web)] --> [API‑Gateway] --> [Backend Service] --> [Managed DB]
                                 |                     |
                                 |                     --> [SAP OData Connector]
                                 |
                                 --> [Auth Provider (Azure AD / Google)]
                                 |
                                 --> [Logging Service]
                                 |
                                 --> [Backup Service]
```

*Hinweis*: Das Diagramm ist als Ausgangspunkt zu verstehen und wird im Detail während der Architektur‑Workshops weiter ausgearbeitet.
