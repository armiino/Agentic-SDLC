# Systemarchitektur – Überblick (Frühe SDLC‑Phase)

## 1. Systemkontext
- **Stakeholder**
  - *Produkt‑Owner (Anna)* – möchte ein MVP‑Kundenportal innerhalb von 8 Wochen.
  - *Technischer Lead (Ben)* – kümmert sich um API‑Layer, SAP‑Integration und Skalierbarkeit.
  - *Compliance / Datenschutz (Clara)* – stellt DSGVO‑Konformität, Auditing und Security Review sicher.
- **Externe Systeme**
  - **SAP ERP** – Quelle für Stammdaten (Produkt, Preise, Kundendaten) und Ziel für Änderungen (z. B. Angebotsstatus).
  - **Identity Provider** (optional) – Azure AD, Google; werden später für SSO angebunden.
  - **Managed Cloud Services** – Hosting‑Umgebung innerhalb der EU (z. B. EU‑Region eines Cloud‑Anbieters). Keine eigenen DB‑Server.
- **Nutzer**
  - **Kunden** (Web‑Frontend, optional Mobile) – nutzen das Portal zum Erstellen von Angeboten, Einsehen von Bestellungen und Download von Rechnungen.
  - **Sales‑Mitarbeiter** – nutzen das gleiche Portal (Rolle *Manager/Support*) für die Angebotserstellung.
  - **Administratoren** – verwalten Rollen, Berechtigungen und Systemkonfiguration.

## 2. Wichtige Komponenten (grobes Layout)
```
+-------------------+      +--------------------+      +-------------------+
|  Web‑Frontend /   | <--->|  API‑Gateway /     | <--->|  Managed Cloud DB |
|  (optionale      |      |  Auth & Rate‑Limit |      |  (EU‑Region)      |
|  Mobile‑App)      |      +--------------------+      +-------------------+
+-------------------+                ^
           ^                         |
           |                         |
           |                +-------------------+
           |                |  OAuth‑2 /         |
           |                |  Identity Provider |
           |                +-------------------+
           |                         |
           |                     (optional)
           |
+-------------------+      +--------------------+      +-------------------+
|  SAP‑Adapter      | <--->|  Integration‑Layer | <--->|  SAP ERP System   |
|  (Batch / Sync)   |      |  (Message‑Queue)   |      +-------------------+
+-------------------+      +--------------------+
```
**Kurzbeschreibung**
- **Web‑Frontend** (React‑ähnliches SPA, Web‑First) – stellt das Kunden‑ und Sales‑Portal bereit.
- **API‑Gateway** – En‑trifft alle eingehenden Aufrufe, übernimmt Authentifizierung (OAuth‑2 / JWT), Rate‑Limiting und leitet an den internen **Business‑Service‑Layer** weiter.
- **Business‑Service‑Layer** – Kernlogik für Angebote, Bestellungen, Rechnungen, Rollen‑ und Berechtigungskontrolle.
- **Managed Cloud DB** – Relationale DB (z. B. PostgreSQL) in EU‑Region; speichert Kunden‑ und Angebotsdaten, wird per Managed Service betrieben (keine eigene DB‑Server‑Beschaffung).
- **Integration‑Layer** – Event‑basiert (Message‑Queue, z. B. Azure Service Bus) zur asynchronen Kommunikation mit **SAP‑Adapter**.
- **SAP‑Adapter** – Synchronisiert Stammdaten (Produkte, Preise) und schreibt Angebots‑/Bestellungs‑Updates zurück ins SAP‑System.
- **Identity Provider** – Optional, für SSO (Azure AD / Google). Bei MVP kann zunächst simples E‑Mail/Passwort‑Login verwendet werden.
- **Logging & Audit Service** – Zentralisiertes Log‑System (z. B. ELK‑Stack) zur Erfüllung von Audit‑Trail‑Anforderungen.
- **Backup / Disaster Recovery** – Tägliche Snapshots der Managed DB und Region‑übergreifende Replication (innerhalb EU).

## 3. Schnittstellen / Integrationspunkte
| Quelle / Ziel | Protokoll / Technologie (wenn ableitbar) | Hinweis / Unsicherheit |
|---------------|------------------------------------------|------------------------|
| Frontend ↔ API‑Gateway | HTTPS/REST (JSON) | Web‑First, Mobile optional – Technologie des Frontends noch offen. |
| API‑Gateway ↔ Business‑Service | interne HTTP‑Aufrufe (REST) | Kann auch gRPC sein – noch nicht spezifiziert. |
| Business‑Service ↔ Managed DB | JDBC/SQL (PostgreSQL) | Managed Service, keine eigene DB. |
| Business‑Service ↔ Integration‑Layer | Async Messaging (Queue) | Event‑basierte SAP‑Synchronisation – Details (Topic/Queue‑Name) unklar. |
| Integration‑Layer ↔ SAP‑Adapter | SOAP/REST (je nach SAP‑Exposition) | SAP‑Schnittstelle nicht definiert – ggf. IDoc, OData oder SOAP. |
| API‑Gateway ↔ Identity Provider | OAuth‑2.0 (Authorization Code) | Optionales SSO, muss ggf. erst später aktiviert werden. |
| Logging/Audit Service ↔ DB/Queue | Syslog/ELK‑Forwarder | Konkretes Logging‑Framework noch nicht festgelegt. |
| Backup Service ↔ Managed DB | Cloud‑Provider Backup‑API | Service‑Konstruktion abhängig vom gewählten Cloud‑Provider. |

## 4. Daten‑ und Sicherheitsaspekte (erkennbar aus Transkript)
- **DSGVO**: Daten ausschließlich in EU‑Regionen speichern; Double‑Opt‑In für Registrierung; Möglichkeit zum Datenlösch‑Request; Rollen‑ und Berechtigungskonzept; Audit‑Trail.
- **Authentifizierung**: Basis‑Login per E‑Mail/Passwort (MVP); optional SSO via Azure AD/Google (später). Transport‑Sicherheit per TLS 1.2+ obligatorisch.
- **Authorization**: Rollen‑basiertes Access‑Control (RBAC) – Admin, User, Manager, Support.
- **Verschlüsselung**: TLS für Daten in Transit; ruhende Daten werden vom Managed DB‑Service bereits verschlüsselt bereitgestellt (keine zusätzliche End‑to‑End‑Verschlüsselung notwendig).
- **Logging / Auditing**: Alle sicherheitsrelevanten Aktionen (Login, Angebotsänderungen, Löschungen) werden protokolliert und müssen nachvollziehbar sein.
- **Backup & DR**: Tägliche Backups, Replication innerhalb EU, Wiederherstellungs‑SLA muss definiert werden.
- **Security Review**: Leichter, aber dokumentierter Review‑Prozess muss parallel zum MVP‑Entwicklungs‑Sprint stattfinden (Risiko: Zeit‑Budget). 

## 5. Offene Architekturentscheidungen (unsichere Annahmen)
- **Frontend‑Technologie**: Web‑First (React/Angular/Vue) vs. native Mobile‑App – Entscheidung noch offen.
- **API‑Authentifizierung**: OAuth‑2.0 (komplex) vs. API‑Keys (einfacher) – MVP könnte mit API‑Keys starten, später zu OAuth wechseln.
- **Datenbank‑Typ**: Relationale Managed DB (PostgreSQL) wird angenommen, jedoch nicht explizit im Transkript genannt.
- **Integration‑Methode zu SAP**: Synchronous REST vs. Asynchronous Messaging (Queue) – beide sind möglich, genaue SAP‑Schnittstelle (IDoc, OData) noch zu klären.
- **Logging‑Stack**: ELK, Splunk oder Cloud‑Native Logging – noch nicht spezifiziert.
- **Backup‑Strategie**: Voll‑Backup täglich + Point‑in‑Time‑Recovery vs. nur Snapshots – genaue Anforderungen müssen noch definiert werden.
- **Security Review Umfang**: Welche Tests (Pen‑Test, Code‑Review, Threat‑Model) zwingend sind, ist noch offen.
- **Skalierbarkeit‑Parameter**: Konkrete Ziel‑TPS, Autoscaling‑Regeln und Load‑Balancing‑Strategie müssen noch spezifiziert werden.

---
*Hinweis*: Dieser Entwurf fasst ausschließlich das aus dem bereitgestellten Stakeholder‑Transkript ableitbare Wissen zusammen. Technologie‑ oder Implementierungs‑Details, die nicht explizit genannt wurden, wurden bewusst nicht festgelegt und als offene Entscheidungen markiert.
