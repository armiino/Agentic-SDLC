# Architekturübersicht (MVP)

## Überblick
Das MVP besteht aus einer **Web‑Frontend‑Applikation** (React/Angular) und einem **Backend‑Service** (Node.js/Java Spring) die über eine **REST‑API** kommunizieren. Das Backend stellt die Geschäftslogik für Login, Angebotserstellung, Rechnungsdownload und Rollen‑/Berechtigungsprüfung bereit und greift auf folgende externe Systeme zu:

1. **SAP‑System (Read‑Only)** – Bereitstellung von Produkt‑, Preis‑ und Rabattdaten via OData/REST.
2. **Managed PostgreSQL (EU‑only)** – Persistenz von Nutzer‑Accounts, Rollen, Audit‑Logs, Angebots‑ und Rechnungs‑Metadaten.
3. **Managed Object Storage (EU‑only, z. B. S3‑Kompatibel)** – Speicherung von PDF‑Rechnungen und Angebots‑Templates.
4. **Identity Provider (optional)** – Azure AD / Google für SSO (nur im späteren Release, im MVP nur E‑Mail/Passwort).
5. **API‑Gateway (temporär)** – Interner Reverse‑Proxy (NGINX) bis das zentrale API‑Gateway verfügbar ist.
6. **Backup‑Service** – Tägliche Snapshots des PostgreSQL und Object‑Storage.

## Komponenten

| Komponente | Aufgabe | Technologie (MVP) | Hinweis |
|------------|---------|-------------------|--------|
| **Frontend** | UI für Kunden, Sales, Admin | React (Create‑React‑App) | Internationalisierung (i18n) für DE/EN, Währungsformatierung EUR/CHF |
| **Auth Service** | Registrierung, Login, Double‑Opt‑In, Passwort‑Reset | Node.js (Express) + JWT | SSO‑Hook später möglich |
| **Business Service** | Angebotserstellung, Rabatt‑Logik (nur Standard‑Rabatte), PDF‑Generierung | Spring Boot (Java) | SAP‑Read‑Only Integration via OData Client |
| **Invoice Service** | Rechnungs‑Lookup, PDF‑Download | Node.js (Express) | PDFs aus Object Storage |
| **Audit Service** | Erfassen von Aktionen (Wer, wann, was) | Spring Boot + PostgreSQL Audit‑Table |
| **API Layer** | Exponiert REST‑Endpoints, schützt mit OAuth (MVP‑Entwurf) | Spring Security OAuth2 | Implementierung nach API‑Gateway Verfügbarkeit |
| **Gateway / Proxy** | Temporärer Reverse‑Proxy bis zentrales API‑Gateway verfügbar | NGINX (Docker) | Weiterleitung zu Backend‑Services |
| **Managed DB** | Persistenz von Kern‑Daten | Managed PostgreSQL (EU‑Region) | Keine eigene DB‑Instanz, Kosten‑Schätzung nötig |
| **Object Storage** | PDFs, Templates | Managed S3‑Kompatibel (EU) | 
| **Backup Service** | Tägliche Snapshots, Restore‑Tests | Managed Backup (Provider) | RPO < 24 h, RTO < 4 h |
| **Secrets Management** | Verwaltung von DB‑Credentials, API‑Keys | Cloud KMS / HashiCorp Vault (Managed) | 
| **Monitoring** | Health‑Checks, Metrics, Alerting | Prometheus + Grafana (Managed) | Audit‑Logs getrennt von technischen Logs |

## Datenfluss (Kern‑Szenario)
1. **Login** – Nutzer gibt E‑Mail/Passwort ein → Auth Service prüft Credentials, sendet Double‑Opt‑In‑E‑Mail → nach Bestätigung wird JWT ausgestellt.
2. **Angebot erstellen** – Sales wählt Produkte → Business Service ruft SAP‑Read‑API für aktuelle Preise → Angebot wird im Backend gespeichert, Audit‑Eintrag erstellt.
3. **Rechnung anzeigen** – Kunde wählt Rechnung → Invoice Service holt Metadaten aus DB, PDF aus Object Storage, liefert an Frontend.
4. **Audit** – Jeder API‑Call schreibt einen Eintrag in die Audit‑Tabelle (User‑ID, Aktion, Timestamp, IP).
5. **Backup** – Täglicher Snapshot von DB und Object Storage, gespeichert in separatem EU‑Bucket.

## Nicht‑funktionale Aspekte
- **Sicherheit**: TLS 1.2+, OAuth‑2.0 (Entwurf), JWT‑Signatur, Secrets Management, getrennte Audit‑Logs.
- **Performance**: Caching von Produkt‑/Preisdaten (5 min TTL) im Backend, um SAP‑Latenz zu reduzieren.
- **Skalierbarkeit**: Container‑Deployment (Docker/K8s) ermöglicht horizontale Skalierung; Managed Services übernehmen Skalierung von DB/Storage.
- **Verfügbarkeit**: 99,5 % SLA durch Managed Services, interner Proxy reduziert Gateway‑Wartezeit.
- **Compliance**: Daten bleiben in EU‑Region, Double‑Opt‑In, Lösch‑API, keine personenbezogenen Daten in technischen Logs.

## Offene Architektur‑Entscheidungen (nach MVP)
- Auswahl des finalen **API‑Gateway‑Providers** und Migration von NGINX.
- Integration von **SSO** (Azure AD / Google) und vollständige OAuth‑Implementierung.
- Erweiterung um **Support‑Ticket‑System** und **Workflow‑Engine** für Rabatt‑Freigaben.
- **Mehrwährungs‑Support** (USD) und erweiterte **Internationalisierung** (weitere Sprachen).
- **Cache‑Strategie** für SAP‑Daten inkl. Invalidierung und Datenschutz‑Prüfung.
- **Full‑Text‑Search** für Angebote/Rechnungen (Elasticsearch) falls nötig.

# Fazit
Die vorliegende Architektur liefert ein minimal funktionsfähiges MVP, das die Kern‑Geschäftsfälle (Login, Angebot, Rechnung) abdeckt, die wichtigsten Compliance‑Anforderungen (DSGVO, EU‑Datenresidenz) erfüllt und gleichzeitig klare Pfade für zukünftige Erweiterungen (SSO, Support, Multi‑Currency, API‑Gateway) definiert.
