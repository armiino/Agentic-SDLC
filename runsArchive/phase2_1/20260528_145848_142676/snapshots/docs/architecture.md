# Architekturüberblick (frühe SDLC‑Phase)

## 1. Systemkontext
- **Akteure**
  - **Kunde** – greift über das Web‑Portal (Desktop) auf Angebote, Rechnungen und Kontoinformationen zu.
  - **Sales‑Mitarbeiter** – loggt sich ein, erstellt Angebote (nur Standard‑Rabatte) und sendet diese an Kunden.
  - **Support** – nutzt ein Kontaktformular, das Anfragen dem zugehörigen Kundenkonto zuordnet. (Kein vollwertiges Ticket‑System im MVP.)
  - **Finance** – erhält PDFs von Angeboten und Rechnungen, prüft Standard‑Rabatte (≥ 15 % werden erst nach MVP freigegeben).
  - **SAP‑System** – liefert Produkt‑ und Preis‑Daten (Read‑Only während MVP). 
  - **Managed Hosting Provider** – stellt EU‑only Infrastruktur (App‑Service, Managed DB, Storage, Backup).
  - **External Identity Provider** (optional) – Azure AD / Google für optionale SSO.
  - **Monitoring / Auditing Service** – sammelt Audit‑Logs (ohne personenbezogene Daten) und technische Logs getrennt.

## 2. Wichtige Komponenten (MVP‑Fokus)
| Ebene | Komponente | Kurzbeschreibung |
|-------|-----------|-------------------|
| **Präsentation** | **Web‑Frontend** (SPA, React/Vue) | Auth‑Flow (Login + Double‑Opt‑In), Angebot‑Wizard, Rechnungs‑Download, Rollen‑basiertes UI. |
| **Applikationslogik** | **API‑Gateway (optional)** | Wird erst nach MVP verfügbar (6 Wochen Warteliste). Im MVP wird ein leichter interner Router verwendet, der grundlegendes Rate‑Limiting bietet. |
| | **Business‑Service** | Kernlogik für Auth, Angebotserstellung, PDF‑Generierung, Rollen‑Check, Audit‑Logging. |
| | **SAP‑Adapter** | Thin‑Layer (REST‑Client) zum Lesen von Produkt‑/Preis‑Daten aus SAP (Read‑Only). |
| | **PDF‑Generator** | Service (z. B. wkhtmltopdf/LibrePDF) für Angebot‑ und Rechnungs‑PDFs inkl. rechtlicher Fußnoten. |
| | **Audit‑Log‑Service** | Schreibt unveränderliche Ereignisse (Login, Offer‑Create/Send, Download) in eine separate, DSGVO‑konforme Log‑Store (z. B. Azure Log‑Analytics oder Elastic). |
| **Datenhaltung** | **Managed DB (EU‑only)** | Relationale DB für Benutzer, Rollen, Angebote, Rechnungs‑Meta‑Daten, Audit‑Einträge. Keine neue eigenständige DB‑Instanz, sondern Managed Service (z. B. Azure PostgreSQL, AWS RDS EU‑Region). |
| | **Object Storage** | Blob‑Store für generierte PDFs (verschlüsselt, EU‑Region). |
| | **Backup Service** | Tägliche Snapshots des DB‑ und Storage‑Layers, Wiederherstellungs‑Test < 24 h RPO. |
| **Cross‑Cutting** | **Secrets Management** | Zentralisierte Verwaltung von DB‑Passwörtern, API‑Keys, Zertifikaten (Managed Secrets Service). |
| | **Monitoring & Alerting** | Basis‑Metriken (CPU, Latency, Error‑Rate) + Audit‑Log‑Monitoring, ohne personenbezogene Daten. |
| | **Identity Provider (optional)** | SSO‑Integration über OAuth2/OpenID Connect (Azure AD, Google). |

## 3. Schnittstellen / Integrationspunkte
- **Frontend ↔ API** – REST‑Endpoints (JSON) über HTTPS, OAuth2‑Bearer‑Token (oder API‑Key fallback). 
- **API ↔ SAP** – SAP‑Adapter ruft SAP‑OData‑Services (Read‑Only) für Produkt‑ und Preisinformationen ab. 
- **API ↔ Managed DB** – CRUD‑Operationen für User, Rollen, Angebote, Invoice‑Metadata. 
- **API ↔ Object Storage** – Upload/Download von PDF‑Dokumenten (signed URLs). 
- **API ↔ Audit‑Log Service** – Write‑Only‑API für Ereignisse; keine Lese‑Funktion aus dem Frontend. 
- **API ↔ Secrets Management** – Laufzeitabruf von Credentials (z. B. DB‑Connection‑String). 
- **Optional: Frontend ↔ External Identity Provider** – OAuth‑Redirect‑Flow für SSO. 

## 4. Daten‑ und Sicherheitsaspekte (bekannte Anforderungen)
| Aspekt | Entscheidung (MVP) |
|--------|--------------------|
| **Authentifizierung** | E‑Mail + Passwort, Double‑Opt‑In (DSGVO). Optional SSO über OAuth2 (später). |
| **Transport** | TLS 1.2+ für alle Verbindungen (Frontend ↔ API, API ↔ SAP, API ↔ DB/Storage). |
| **Daten im Ruhezustand** | Verschlüsselung durch Managed Service (at‑rest encryption). PDFs ebenfalls serverseitig verschlüsselt. |
| **Datenminimierung** | Nur für das aktuelle Angebot/Invoice notwendige Kundendaten werden im Portal gespeichert; weitere SAP‑Daten per On‑Demand‑Abruf. |
| **Audit‑Logging** | Ereignisse (Login, Offer‑Create, Offer‑Send, PDF‑Download) werden pseudonymisiert (User‑ID ohne Klartext‑PII) in einer separaten Log‑Store. |
| **Technisches Logging** | Keine personenbezogenen Daten; nur Fehlermeldungen, Performance‑Metriken. |
| **Backup / DR** | Tägliche Snapshots, Aufbewahrung ≥ 30 Tage, Wiederherstellungstest alle 2 Wochen. |
| **Secrets Management** | Verwendung eines Managed Secrets Service; keine Hard‑Coded‑Credentials im Code‑Repo. |
| **Rate‑Limiting** | Einfacher Token‑Bucket pro User (z. B. 10 Requests / Minute) im Service‑Layer – Platzhalter bis API‑Gateway verfügbar. |
| **Hosting‑Region** | EU‑Only (nachweisbare Datenresidenz) – Provider‑Auswahl noch offen (Kosten‑Schätzung ausstehend). |
| **Compliance** | DSGVO‑Pflichten (Double‑Opt‑In, Lösch‑/Auskunfts‑Konzept, AVV mit Provider). |

## 5. Offene Architekturentscheidungen (MVP‑Auswirkungen)
1. **API‑Gateway** – Wird erst nach 6 Wochen verfügbar. MVP muss mit internem Rate‑Limiter auskommen; später Migration zum zentralen Gateway planen.
2. **Managed DB Provider** – Noch nicht final (Azure PostgreSQL, AWS RDS EU, etc.). Entscheidung beeinflusst Kosten, Backup‑Strategie und Secrets‑Management.
3. **SSO‑Umfang** – Optionales Feature; kein fester Provider festgelegt. Im MVP nur Grund‑Login.
4. **Mehrwährung** – EUR als Standard, CHF optional nur wenn Pilotkunde Schweiz ist. Unterstützung erst nach MVP.
5. **Support‑Ticket‑System** – Aktuell nur Kontaktformular. Entscheidung über ein Ticket‑Tool (z. B. Jira Service Management) wird nach MVP getroffen.
6. **Caching‑Strategie** – Keine Preis‑Caches im MVP wegen Daten‑Freshness‑Risiko. Nur statische Produkt‑Infos könnten gecached werden.
7. **Internationalisierung** – Minimal‑i18n mit DE/EN. Weitere Sprachen und länderspezifische Rechts‑Hinweise später.
8. **Backup‑RPO/RTO** – Grund‑Backup definiert, genaue RPO (< 24 h) und RTO noch zu konkretisieren.
9. **Retention‑Policy** – Baseline‑Retention (z. B. 7 Jahre) implementiert, detaillierte Lösch‑Ausnahmen (gesetzliche Aufbewahrung vs. DSGVO‑Löschung) noch offen.

## 6. Architektur‑Diagramm (textuell)
```
+-------------------+       HTTPS       +-------------------+
|    Kunden/Users   |<---------------->|   Web Frontend    |
+-------------------+                  +-------------------+
                                                |
                                                | REST API (OAuth2/Bearer)
                                                v
+-------------------+   +-------------------+   +-------------------+
|   Business‑Service|---|   SAP Adapter    |---|   SAP System      |
+-------------------+   +-------------------+   +-------------------+
        |   ^                     |
        |   | DB Access (Managed)  |
        v   |                     v
+-------------------+   +-------------------+   +-------------------+
|   Managed DB      |   |   Object Storage  |   |   Audit Log Store |
+-------------------+   +-------------------+   +-------------------+
        ^                     ^
        | Secrets Mgmt        | Backup Service (daily snapshots)
        +---------------------+
```

## 7. Zusammenfassung
Der frühe Architekturüberblick definiert ein **MVP‑fokussiertes, EU‑konformes Kundenportal** mit den Kernfeatures **Login (Double‑Opt‑In), Angebotserstellung (SAP‑Read‑Only), Rechnungs‑Download** und einem **minimalen Rollen‑/Audit‑Modell**. Die Systemlandschaft besteht aus einem Web‑Frontend, einem Business‑Service‑Layer, einem SAP‑Adapter, einer Managed DB, Object Storage für PDFs und einer getrennten Audit‑Log‑Komponente. Sicherheits‑ und Datenschutz‑Aspekte (TLS, at‑rest Encryption, Secrets Management, DSGVO‑konforme Logging) werden bereits berücksichtigt. Offene Entscheidungen (API‑Gateway, DB‑Provider, SSO, Mehrwährung, Support‑Ticket‑System) sind explizit dokumentiert und beeinflussen das weitere Vorgehen nach dem MVP.

*Alle Informationen stammen aus dem Kontext‑File `runs/phase2_1/20260528_145848_142676/state/context.md`, den Requirements `docs/requirements.md`, den Risiken `docs/risks.md` und dem Transkript `input/transcripts/T9999_chaos.txt`.*