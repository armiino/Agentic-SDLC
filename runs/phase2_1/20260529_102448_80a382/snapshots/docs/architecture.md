# Architektur‑Überblick (frühe SDLC‑Phase)

## 1. Systemkontext
| System | Rolle / Verantwortung |
|--------|------------------------|
| **Kundenportal (Web‑App)** | Frontend für Kunden und Sales‑Mitarbeiter – Login, Angebotserstellung, Rechnungs‑Download, Rollen‑basiertes UI. |
| **API‑Gateway / API‑Layer** | Eingangs‑Schnittstelle für das Frontend, zentrale Authentifizierung, Rate‑Limiting, OAuth‑/API‑Key‑Mechanismus (Platzhalter im MVP). |
| **SAP‑Backend** | Quelle für Produkt‑, Preis‑ und Rabatt‑Stammdaten (Lese‑Zugriff im MVP). Schickt bestätigte Aufträge (Phase 2). |
| **Managed Hosting Provider (EU‑Only)** | Infrastruktur (Web‑Server, App‑Server, Daten‑Speicher) – muss DSGVO‑konform sein, tägliches Backup, Disaster‑Recovery‑Basis. |
| **Identity Provider (optional)** | Azure AD / Google für spätere SSO‑Integration (MVP: E‑Mail + Double‑Opt‑In). |
| **Support‑E‑Mail / Kontakt‑Formular** | Minimaler Support‑Kanal – leitet Anfragen per E‑Mail an Support‑Team (kein Ticket‑System im MVP). |
| **Monitoring / Logging Service** | Technisches Logging, Health‑Checks, Alerts; muss Trennung zwischen technischem Log und Audit‑Log sicherstellen. |
| **Finance / Legal Systeme (extern)** | Nutzen PDF‑Export aus dem Portal; benötigen rechtliche Fußnoten, Signatur‑Infos (später). |

*Der Kontext zeigt, dass das Portal stark von SAP‑Daten und einer EU‑Only Managed‑Hosting‑Umgebung abhängt. Alle externen Systeme müssen über das zentrale API‑Layer kommunizieren.*

## 2. Wichtige Komponenten (MVP‑Scope)
1. **Frontend (Web‑UI)** – Single‑Page‑Application (z. B. React/Angular – Technologie‑Entscheidung bewusst offen). Verantwortlich für:
   - Authentifizierung (E‑Mail + Passwort, Double‑Opt‑In).
   - Rollen‑basierte UI (Admin, Sales, Kunde).
   - Formular für Angebotserstellung (Daten aus SAP).
   - PDF‑Download von Rechnungen.
   - Internationalisierung (Deutsch/Englisch) – i18n‑Framework.
2. **Auth‑Service (MVP‑Implementierung)** – Einfacher Nutzer‑Store (Managed DB Service, da keine eigene DB erlaubt). Funktionen:
   - Registrierung mit Double‑Opt‑In‑E‑Mail‑Bestätigung.
   - Passwort‑Hashing (bcrypt/argon2).
   - Session‑Management (JWT mit kurzer Lebenszeit). 
3. **API‑Layer** – REST‑API, in zwei Sub‑Bereiche getrennt:
   - **Public API** (Login, Registrierung, Angebot‑Erstellung, Rechnung‑Download). 
   - **Internal API** (SAP‑Adapter, Audit‑Log, Monitoring). 
   *Im MVP wird OAuth nur als zukünftige Option gekennzeichnet; derzeitiger Auth‑Service liefert JWT.*
4. **SAP‑Adapter Service** – Wrapper‑Komponente, die SAP‑OData‑/RFC‑Aufrufe ausführt (nur Lese‑Operationen im MVP). Verantwortlich für:
   - Produkt‑ und Preis‑Abruf.
   - Validierung von Kundendaten‑Referenzen.
5. **Audit‑Log Service** – Minimaler, append‑only Log (z. B. Cloud‑Log‑Store). Erfasst:
   - Nutzer‑ID, Aktion (Login, Angebot‑Create, Download), Timestamp, Objekt‑ID.
   - Keine PII im technischen Log – Trennung von Audit‑Log und Application‑Log.
6. **Managed Data Store** – Schlüssel‑Wert‑Store / Blob‑Storage (z. B. Azure Blob, AWS S3 – Provider‑wahl offen). Nutzt:
   - User‑Credentials (verschlüsselt).
   - Angebots‑ und Rechnungs‑Meta‑Daten (Referenz‑IDs, Status).
   - PDFs (Generated und/oder aus SAP). 
7. **Backup & DR** – Daily Snapshots des Data Stores, Aufbewahrung mindestens 7 Tage, Wiederherstellung über Managed‑Service‑Funktion.
8. **Monitoring & Alerting** – Health‑Checks (Liveness, Readiness), Rate‑Limiting‑Metriken, Fehler‑Alert per E‑Mail/Slack.
9. **Support‑Kontakt‑Formular** – Frontend‑Komponente, sendet E‑Mail an Support‑Team; Daten werden nicht persistent gespeichert (MVP).

## 3. Schnittstellen / Integrationspunkte
| Quelle/Ziel | Protokoll / Technologie (MVP‑Stufe) | Zweck |
|-------------|--------------------------------------|------|
| Frontend ↔ API‑Layer | HTTPS/REST (JSON) | Nutzer‑Interaktionen (Login, Angebot, Rechnung). |
| API‑Layer ↔ Auth‑Service | Internal HTTP (JWT) | Authentifizierung, Token‑Ausgabe. |
| API‑Layer ↔ SAP‑Adapter | HTTPS/OData oder RFC over HTTP (Managed SAP‑Connector) | Lese‑Zugriff auf Produkt‑/Preis‑Daten. |
| API‑Layer ↔ Audit‑Log Service | HTTPS/POST (JSON) | Write‑Only Audit‑Eintrag. |
| API‑Layer ↔ Managed Data Store | SDK / REST (Blob/Key‑Value) | Persistenz von Benutzer‑ und Angebots‑Meta‑Daten. |
| Frontend ↔ Support‑Formular | HTTPS (POST) an Mail‑Gateway (z. B. SendGrid) | E‑Mail‑Versand an Support‑Team. |
| Monitoring ↔ Managed Hosting | Cloud‑Native Monitoring‑API | Metriken, Alerts, Log‑Export. |

## 4. Daten‑ & Sicherheitsaspekte (erkennbare Punkte)
- **Datenschutz (DSGVO)**
  - Double‑Opt‑In bei Registrierung (nachweisbare Einwilligung).
  - Minimaler Datenumfang: Nur E‑Mail, Passwort‑Hash, Rollen‑Info, Angebots‑/Rechnungs‑Meta‑Daten.
  - Lösch‑Konzept: Nutzer‑Account kann auf Anfrage vollständig gelöscht werden; Audit‑Log bleibt erhalten nur solange gesetzlich nötig.
  - Datenresidenz: Alle Daten (User‑Store, PDFs, Backups) liegen ausschließlich in EU‑Regionen des Managed‑Providers.
- **Sicherheitsmaßnahmen (MVP)**
  - TLS 1.2+ für alle Netzwerk‑Verbindungen.
  - Passwort‑Hashing mit Argon2/Bcrypt.
  - JWT‑Signatur mit asymmetrischen Schlüsseln, regelmäßiger Schlüssel‑Rotation.
  - Rate‑Limiting (z. B. 100 Requests/min pro IP) am API‑Gateway.
  - Trennung von Audit‑Log und Application‑Log, keine PII in technischen Logs.
  - Secrets Management über Managed Secret Store (z. B. Azure Key Vault) – noch zu wählen, aber vor Deployment definiert.
- **Backup & Disaster Recovery**
  - Daily immutable snapshots, Aufbewahrung 7 Tage.
  - Wiederherstellung über Managed Service UI – Ziel < 4 Stunden.
- **Auditing**
  - Immutable Append‑Only Log, gespeichert im selben Managed Storage, verschlüsselt.
  - Zugriff nur für Compliance‑Team, Leserechte über IAM‑Rollen.

## 5. Offene Architekturentscheidungen (Bewusste Unsicherheiten)
| Entscheidung | Offene Punkte / Konsequenz |
|--------------|----------------------------|
| **API‑Gateway‑Provider** | Derzeit kein konkreter Anbieter (AWS API GW, Azure APIM, Kong etc.). Wahl beeinflusst Rate‑Limiting, Auth‑Optionen, Kosten. |
| **Managed DB / Storage** | Keine feste DB‑Technologie (SQL vs. NoSQL). Muss DSGVO‑konform sein, und darf keine eigene DB‑Instanz erfordern (laut Stakeholder). |
| **SSO‑Integration** | Azure AD / Google SSO ist nur optional – Implementierung erst nach MVP. |
| **OAuth 2.0 vs. JWT‑Only** | OAuth als langfristiges Ziel, aber im MVP nur JWT‑basiertes Auth. Entscheidung über späteren Wechsel muss getroffen werden. |
| **Mehrwährungs‑Support** | Pilot‑Kunde Schweiz (CHF) – muss im Datenmodell berücksichtigt werden, aber nicht im MVP implementiert. |
| **Support‑Ticket‑System** | MVP nutzt nur E‑Mail‑basiertes Formular. Entscheidung über externes SaaS‑Ticket‑Tool vs. Eigenbau bleibt offen. |
| **Caching‑Strategie für SAP‑Daten** | Kein Cache im MVP (Daten‑Minimierung & Risiko). Spätere Phase könnte ein read‑through Cache einführen – muss Daten‑Konsistenz‑ und Lösch‑Regeln berücksichtigen. |
| **Backup‑Granularität** | Daily Snapshots ausreichend für MVP, aber keine hourly / point‑in‑time‑Recovery – muss ggf. erweitert werden. |
| **Monitoring‑Tooling** | Auswahl zwischen Cloud‑Native (Azure Monitor, AWS CloudWatch) oder Open‑Source (Prometheus + Grafana). Entscheidung wirkt sich auf Alert‑Routing und Kosten aus. |

## 6. Zusammenfassung
Der vorliegende Architektur‑Entwurf fokussiert sich auf ein **minimal skalierbares MVP** für ein Kundenportal, das innerhalb von 8 Wochen geliefert werden soll. Kernpunkte sind:
- **Web‑Frontend** + **JWT‑basiertes Auth** als schnell umsetzbare Auth‑Lösung.
- **REST‑API‑Layer** mit klaren, getrennten Bereichen für Public‑ und Internal‑Endpoints.
- **SAP‑Adapter** (Read‑Only) als einzige externe Abhängigkeit.
- **Managed EU‑Only Hosting** mit täglichem Backup, um DSGVO‑Konformität zu gewährleisten.
- **Minimaler Audit‑Log** und **Technisches Logging** mit strikter Trennung von PII.
- **Bewusste MVP‑Aus‑Klammerungen** (Kein SSO, kein Ticket‑System, keine Sonderrabatte, keine vollständige Security‑Review).

Alle genannten Komponenten, Schnittstellen und Sicherheitsaspekte leiten sich eindeutig aus den Stakeholder‑Statements und dem Kontext‑Artefakt ab. Offene Entscheidungen werden dokumentiert, damit sie in den nächsten Phasen (Detail‑Design, Implementation) gezielt adressiert werden können.
