# Architekturüberblick – Kundenportal (MVP)

## 1. Systemkontext
Das System ist ein **Kundenportal** für das Unternehmen, das folgende externen Systeme und Akteure einbindet:

- **Kunden (End‑User)** – greifen über Web‑Browser (Desktop, später responsive Mobile) auf das Portal zu.
- **Sales‑Mitarbeiter** – nutzen das Portal zur Angebotserstellung.
- **Support‑Mitarbeiter** – erhalten Anfragen über ein Kontakt‑Formular, das im Portal hinterlegt ist.
- **SAP ERP** – liefert Produkt‑, Preis‑ und Kundendaten (Read‑Only). Alle Angebots‑ und Rechnungsinformationen werden aus SAP gelesen; Schreibzugriff wird im MVP nicht benötigt.
- **Managed PostgreSQL‑Service (EU‑Only)** – persistenter Datenspeicher für Nutzer‑Konten, Rollen, Angebote (Draft/Approved), Audit‑Logs und das Kontakt‑Formular.
- **Managed Object‑Storage (EU‑Only)** – Speicherung von PDF‑Dokumenten (Rechnungen, Angebots‑PDFs).
- **Identity Provider (optional, später)** – Azure AD / Google für mögliche SSO.
- **Monitoring / Logging‑Infrastruktur** – Prometheus + Grafana (Performance‑Metrics) und ein separates Audit‑Log‑Store (z. B. Elasticsearch) – beide ohne personenbezogene Daten.
- **Backup‑Service** – automatisierte tägliche inkrementelle und wöchentliche Vollbackups des Datenbank‑ und Storage‑Buckets.

## 2. Wichtige Komponenten & deren Verantwortlichkeiten
| Komponente | Technologie‑Hinweis (offen) | Aufgabe |
|------------|----------------------------|--------|
| **Web‑Frontend** | React/Angular (SPA) – **offen** | UI für Kunden, Sales und Support; authentifiziert über API‑Gateway/Authentication‑Service; unterstützt DE/EN, responsive Design. |
| **API‑Gateway (MVP‑intern)** | Eigen‑implementierter lightweight Reverse‑Proxy (z. B. NGINX + Lua) – ersetzt externes Gateway bis das offizielle Gateway verfügbar ist. | Exponiert REST‑Endpoints, zentralisiertes Rate‑Limiting, TLS‑Terminierung, Routing zu Microservices, OAuth‑2.0 Token‑Validierung (optional). |
| **Authentication Service** | OAuth‑2.0 / OpenID‑Connect (z. B. Keycloak – **offen**) | Login per E‑Mail + Passwort, Double‑Opt‑In Workflow, Token‑Ausgabe, Verwaltung von Password‑Resets. |
| **User & Role Service** | Microservice (Node/Java) | Verwaltung von Nutzer‑Profiles, Rollen (Admin, Sales, Kunde) und Berechtigungen; Schnittstelle zu Auth‑Service. |
| **Offer Service** | Microservice | Erzeugt Angebote, liest Produkt‑/Preis‑Daten aus SAP, speichert Draft/Approved‑Status, erstellt PDF‑Export via Template‑Engine, triggert Audit‑Log‑Einträge. |
| **Invoice Service** | Microservice | Liest Rechnungs‑ und Bestellinformationen aus SAP, erzeugt PDF‑Download, prüft Berechtigungen. |
| **SAP Connector** | Adapter‑Service (REST/ SOAP) – **Read‑Only** | Kapselt sämtliche SAP‑APIs, liefert Produkt‑, Preis‑ und Kundendaten; implementiert Retry‑ und Timeout‑Logik. |
| **Audit‑Log Service** | Separate Log‑Store (Elasticsearch/Opensearch) | Persistiert sicherheitsrelevante Ereignisse (Login, Angebotserstellung, Änderungen) mit Zeitstempel, getrennt von technischen Logs. |
| **Backup Service** | Cloud‑Backup‑Tool (z. B. AWS S3‑Glacier EU‑Region, Azure Backup) | Periodische Sicherung von DB‑ und Storage‑Daten, Test‑Restore‑Prozesse. |
| **Monitoring / Alerting** | Prometheus + Grafana | Erfasst System‑Metriken, Performance, Rate‑Limits; stellt Alerts bei Anomalien bereit. |
| **Secrets Management** | Vault / AWS Secrets Manager (EU‑Region) – **offen** | Zentralisierte Verwaltung von API‑Keys, DB‑Credentials, TLS‑Zertifikaten. |

## 3. Schnittstellen / Integrationspunkte
- **Frontend ↔ API‑Gateway** – HTTPS‑Calls, JWT‑Authorization Header.
- **API‑Gateway ↔ Microservices** – interne HTTP/REST‑Calls; jedes Service besitzt eigene OpenAPI‑Spec.
- **Offer/Invoice Service ↔ SAP Connector** – REST‑Wrapper um SAP‑BAPIs (Read‑Only). Fehlertoleranz mit klarer Fehlermeldung bei SAP‑Ausfall.
- **Authentication Service ↔ User & Role Service** – User‑Lookup für Rollen‑Prüfung.
- **Audit‑Log Service ↔ Microservices** – Event‑Publishing via Message‑Queue (z. B. RabbitMQ – **offen**) oder direkte HTTP‑POSTs.
- **Backup Service ↔ Managed PostgreSQL & Object‑Storage** – automatisierte Snapshot‑Jobs.
- **Monitoring ↔ Services** – Export von Prometheus‑Metrics via `/metrics` Endpunkt.
- **Secrets Management ↔ Alle Services** – Laufzeit‑Abruf von Secrets via Vault‑Sidecar.

## 4. Daten‑ und Sicherheitsaspekte
- **Datenhaltung**: Alle Daten werden ausschließlich in EU‑regionen gespeichert (Managed PostgreSQL + Object‑Storage). Backup‑Kopien ebenfalls EU‑only.
- **Verschlüsselung**: In‑Transit über TLS 1.2+; at‑rest Verschlüsselung wird vom Managed Service bereitgestellt (AES‑256). Keine End‑to‑End‑Verschlüsselung für Inhalte, da TLS ausreichend für das MVP.
- **Authentifizierung & Autorisierung**: OAuth‑2.0 Access‑Token, Rollen‑basiertes Access‑Control (RBAC). Minimal‑Rollenmodell (Admin, Sales, Kunde).
- **Audit‑Logging**: Jeder sicherheitsrelevante Vorgang wird unveränderlich in den Audit‑Log‑Store geschrieben (immutable, append‑only). Technische Logs (z. B. Fehler‑Stacks) enthalten keine personenbezogenen Daten.
- **DSGVO‑Erfüllung**: Double‑Opt‑In beim Registrieren, Lösch‑ und Retention‑Mechanismen (Rechte‑auf‑Vergessen, gesetzliche Aufbewahrung von Angeboten bis 7 Jahre). Keine personenbezogenen Daten in Monitoring‑Logs.
- **Rate‑Limiting**: Grund‑Limit 100 Requests/Minute pro Nutzer, implementiert im internen API‑Gateway; weitere Limits (z. B. Download‑Grenzen) auf Service‑Ebene.
- **Backup & Disaster Recovery**: Tägliche inkrementelle Backups, wöchentliche Vollbackups, Wiederherstellungstest mindestens einmal pro Woche. RTO ≤ 4 Stunden, RPO ≤ 12 Stunden (MVP‑Level).
- **Secret‑Management**: Secrets werden nicht im Code, sondern über einen zentralen Vault bezogen; automatische Rotation möglich.

## 5. Offene Architekturentscheidungen (Bewusst nicht finalisiert)
1. **API‑Gateway‑Technologie** – Nutzung eines leichten Reverse‑Proxy (NGINX + Lua) bis das interne Enterprise‑Gateway verfügbar ist.
2. **Identity Provider für SSO** – Azure AD / Google SSO geplant für spätere Phasen, aber nicht im MVP.
3. **Managed DB‑Produkt** – Auswahl zwischen Azure‑Postgres, AWS RDS‑Postgres oder Google CloudSQL (alle EU‑Region). Noch keine Entscheidung, da Kosten noch nicht geschätzt.
4. **Message‑Queue für Audit‑Events** – Direkt‑HTTP‑POST vs. RabbitMQ vs. Kafka – Wahl wird nach MVP‑Launch getroffen.
5. **Caching‑Strategie** – Nur Produktdaten (nicht kundenbezogen) sollen ggf. im Service‑Cache gehalten werden; genaue Implementierung offen.
6. **Mehrwährungs‑Support** – CHF‑Anzeige im UI wird als Feature‑Toggle definiert; tatsächliche Preis‑Berechnung bleibt im MVP bei EUR.
7. **Support‑Workflow** – Derzeit nur ein Kontakt‑Formular, später eventuell ein leichtgewichtiges Ticket‑System (z. B. Jira Service Management) – Entscheidung offen.

## 6. Zusammenfassung
Der vorliegende Architektur‑Entwurf liefert ein **minimal tragfähiges System** für das MVP des Kundenportals, das die wichtigsten funktionalen und nicht‑funktionalen Anforderungen erfüllt und gleichzeitig die kritischen Risiken (DSGVO‑Compliance, SAP‑Verfügbarkeit, Daten‑Residenz) adressiert. Die modulare Microservice‑Aufteilung ermöglicht ein einfaches Hinzufügen von Features (SSO, erweiterte Rabatt‑Freigabe, Ticket‑System, Multi‑Währung) nach dem initialen Launch, ohne die Grundarchitektur grundlegend zu verändern.

---
*Alle genannten Elemente basieren auf den im Kontext‑ und Risiko‑Artefakt (runs/phase2_1/20260527_124935_08b86f/state/context.md, docs/requirements.md, docs/risks.md) dokumentierten Aussagen. Technologie‑Entscheidungen, die nicht eindeutig ableitbar waren, bleiben bewusst offen und werden im weiteren Projektverlauf konkretisiert.*