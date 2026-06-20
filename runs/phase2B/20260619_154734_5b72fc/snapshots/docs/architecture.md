# Architekturüberblick – Kundenportal (MVP)

## 1. Systemkontext

- **Endnutzer**: Vertriebsmitarbeiter (Sales), Administratoren, Kunden (via Browser/App).
- **Externe Systeme**:
  - **SAP ERP** – Lesezugriff auf Produkt‑, Preis‑ und Rabattdaten (keine Schreibrechte im MVP).
  - **E‑Mail‑Provider** – Versand von Double‑Opt‑In‑Mails, Benachrichtigungen und PDF‑Download‑Links.
  - **Identity Provider (optional)** – Azure AD oder Google für zukünftiges SSO.
  - **Hosting‑Provider (EU‑Only Managed Service)** – Bereitstellung von Compute, Managed‑DB und Storage in einer EU‑Region.
- **Interne Stakeholder**: Product Owner, Architekt, Datenschutz, Finance, IT‑Operations, Support.

## 2. Wichtige Komponenten

| Komponente | Verantwortung | Hinweis |
|-----------|---------------|--------|
| **Web‑Frontend** | React‑/Vue‑basierte SPA, liefert UI in DE/EN, greift über HTTPS auf das API‑Gateway zu. | UI‑Internationalisierung (i18n) für DE/EN bereits vorgesehen. |
| **API‑Gateway / Proxy** | Einstiegspunkt für alle REST‑Aufrufe, übernimmt Auth‑Validierung, Rate‑Limiting, TLS‑Termination. | Ersatz‑Proxy (z. B. Nginx) wird bis zum API‑Gateway‑Release eingesetzt. |
| **Auth‑Service** | Verwaltung von Benutzerkonten, Double‑Opt‑In‑Workflow, Passwort‑Hashing, Token‑Ausgabe (JWT). | Optionaler SSO‑Connector wird als Plug‑in geplant. |
| **RBAC‑Service** | Rollen‑ und Berechtigungsprüfung (Admin, Sales, Kunde). | Konfiguration über Policy‑Store (z. B. JSON‑Datei, später DB). |
| **Offer‑Service** | Geschäftslogik für Angebotserstellung, liest Produkt‑/Preisdaten aus SAP, speichert Angebote im DB. | Rabatt‑Freigabelogik muss später konfigurierbar sein (offene Entscheidung). |
| **Invoice/Order Service** | Stellt Rechnungs‑ und Bestellungs‑PDFs bereit, verwaltet Download‑Links. | PDF‑Erzeugung via templating engine (z. B. wkhtmltopdf). |
| **Audit‑Log Service** | Unveränderbare Protokollierung von Login, Angebots‑/Rechnungs‑Events. | Schreib‑only, immutable storage (z. B. Append‑Only‑Log). |
| **Managed DB (EU‑Only)** | Persistenz für Nutzer‑, Angebots‑, Rechnungs‑ und Audit‑Daten. | Keine neue Datenbank, bestehender Managed‑Service wird genutzt. |
| **Backup & DR Service** | Tägliche Snapshots, Wiederherstellung innerhalb 4 h, Datenverlust ≤ 1 h. |
| **Monitoring & Alerting** | Technisches Monitoring (Performance, Infrastruktur) und separates Audit‑Monitoring (keine PII). |

## 3. Schnittstellen / Integrationspunkte

- **Frontend ↔ API‑Gateway**: HTTPS/REST (JSON), JWT‑basiert für Authentifizierung.
- **API‑Gateway ↔ Microservices**: interne HTTP‑Aufrufe (REST) innerhalb des privaten Netzwerks.
- **Offer‑Service ↔ SAP**: REST‑ oder OData‑Client, read‑only Zugriff auf Produkt‑/Preis‑Endpunkte.
- **Invoice Service ↔ Storage**: Schreib‑/Lesezugriff auf Blob‑Storage (z. B. S3‑kompatibel) für PDFs.
- **Auth‑Service ↔ E‑Mail‑Provider**: SMTP / API für Double‑Opt‑In‑Mails.
- **Optional SSO‑Connector ↔ Identity Provider**: OpenID Connect / SAML.
- **Backup Service ↔ Managed DB**: automatisierte Snapshot‑API des Cloud‑Providers.
- **Monitoring ↔ Services**: Export von Metriken (Prometheus) und Logs (ELK) – Trennung von technischen und audit‑relevanten Daten.

## 4. Daten‑ und Sicherheitsaspekte

- **Datenschutz (DSGVO)**:
  - Double‑Opt‑In bei Registrierung, klare Einwilligungs‑Logs.
  - Datenminimierung: Nur notwendige personenbezogene Daten werden gespeichert (E‑Mail, Name, Rollen). 
  - Löschen‑Mechanismus für Kunden‑ und Log‑Daten gemäß noch zu definierender Retention‑Policy.
  - Audit‑Log ist unveränderlich, verschlüsselt im Speicher und nur lesbar durch autorisierte Rollen.
- **Sicherheit**:
  - TLS 1.2+ für alle Netzwerkverbindungen.
  - JWT‑Signatur mit kurzen Lebenszeiten, Refresh‑Token‑Mechanismus.
  - Basis‑Rate‑Limiting im API‑Gateway (z. B. 100 Requests/Minute pro IP).
  - Secrets‑Management via Cloud‑KMS (Passwörter, DB‑Credentials, JWT‑Signing‑Key).
  - Regelmäßige Security‑Reviews (Threat‑Modelling, Pen‑Tests) – Ziel < 6 Wochen vor Go‑Live.
- **Backup & Disaster Recovery**:
  - Tägliche vollständige Backups, Aufbewahrung 30 Tage.
  - RPO = 1 Stunde, RTO = 4 Stunden (nach Freigabe durch Operations).
- **Verfügbarkeit**:
  - Ziel‑Uptime 99,5 % im Produktions‑Umfeld, redundante Pods/Instanzen hinter Load‑Balancer.

## 5. Offene Architekturentscheidungen

1. **Ersatz‑API‑Gateway**: Welcher Proxy (NGINX, Traefik, API‑Management‑Lite) wird bis zum offiziellen Gateway eingesetzt?
2. **Managed DB‑Anbieter**: Welcher EU‑only Managed‑DB‑Service (z. B. Azure‑PostgreSQL, AWS‑RDS EU‑Region) wird final gewählt?
3. **SSO‑Integration**: Ob Azure AD, Google oder ein anderer IdP unterstützt wird und wie das Fallback‑Login ohne SSO aussehen soll.
4. **Mehrwährung**: Ob CHF‑Preisanzeige bereits im MVP unterstützt wird oder erst in Phase 2.
5. **Retention‑Policy**: Konkrete Aufbewahrungsfristen für Angebote, Rechnungen und Audit‑Logs (z. B. 10 Jahre für Rechnungen).
6. **Audit‑Log‑Speicher**: Ob ein Append‑Only‑Log‑Service (z. B. Cloud‑Watch Logs, Azure Log Analytics) oder ein relationaler Table verwendet wird.
7. **Backup‑Dienst**: Welche konkreten Service‑Level‑Agreements (SLA) und Kosten‑Modelle werden akzeptiert?
8. **Monitoring‑Trennung**: Wie wird sichergestellt, dass PII nicht in das technische Monitoring‑System gelangt?
9. **Rate‑Limiting Details**: Ob IP‑basiert, Nutzer‑basiert oder kombinierte Policies eingesetzt werden.
10. **Support‑Kontaktformular**: Wie werden dort erhobene Daten gespeichert, welche Löschfristen gelten?

---
*Dieser Architekturüberblick basiert auf den bereitgestellten Anforderungen, Risiken und Annahmen. Weitere Detailentwürfe (Komponenten‑Diagramme, Datenfluss‑Diagramme) werden in nachfolgenden Phasen erstellt.*
