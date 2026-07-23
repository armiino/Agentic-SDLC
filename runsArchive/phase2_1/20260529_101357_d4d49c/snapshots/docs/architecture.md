# Architekturüberblick – Kundenportal (MVP)

## 1. Systemkontext
Das Kundenportal ist ein webbasiertes Front‑End, das primär von **Sales‑Mitarbeitern** und **Kunden** (inkl. Administrations‑User) genutzt wird. Es verbindet sich mit folgenden externen Systemen:
- **SAP ERP** – Nur Lesezugriff für Produkt‑, Preis‑ und Rabattdaten (keine Schreibrechte im MVP). 
- **Managed DB Service (EU‑Only)** – Persistenz für Nutzer‑Accounts, Rollen, Audit‑Logs und Angebots‑Metadaten. 
- **E‑Mail‑Provider** – Double‑Opt‑In‑Flows und Benachrichtigungen (z. B. für Passwort‑Reset). 
- **Backup‑Service** – Periodische (wöchentliche) Snapshots des Managed DB Service. 
- **Monitoring / Logging‑Infrastruktur** – Technische Logs (z. B. Application Insights) getrennt von Audit‑Logs.

> **Grenzen**: Kein separates Ticket‑System, kein API‑Gateway (Warteliste), kein eigenständiger Datenbank‑Server – alles über Managed Services.

## 2. Wichtige Komponenten
| Komponente | Aufgabe | Bemerkung |
|-----------|--------|-----------|
| **Web‑Frontend** (HTML/JS, responsive) | UI für Login, Angebotserstellung, Rechnungs‑Download, Kontakt‑Formular. | Fokus auf **Deutsch** (Englisch optional). |
| **Auth Service** | E‑Mail/Passwort‑Login, Double‑Opt‑In, optionale SSO (Azure AD / Google) – **nicht verpflichtend** im MVP. | Implementiert als Managed Identity‑Provider (z. B. Azure AD B2C) oder Eigen‑Lösung. |
| **API Layer** | REST‑Endpoints für Frontend‑Aufrufe (Offer‑Wizard, Rechnungs‑Abruf, Nutzer‑Management). | Minimaler Satz, OAuth‑2‑Token‑Absicherung (Resource‑Owner‑Password‑Flow) – interne OAuth‑Implementation, kein zentrales Gateway. |
| **SAP Connector** | Service, der SAP‑Web‑Services oder OData‑APIs konsumiert, um Produkt‑/Preis‑Daten zu lesen. | Echtzeit‑Lesen → kritische Abhängigkeit (TR‑1). |
| **Managed DB (EU‑Only)** | Speicherung von Nutzer‑Profilen, Rollen, Audit‑Events, Angebots‑Meta (nicht die kompletten Angebots‑Daten). | Keine neue DB‑Instanz, Nutzung eines bestehenden Cloud‑Managed‑DB‑Produkts. |
| **Audit‑Log Service** | Write‑only, unveränderlich, speichert wer wann Angebote angesehen/geändert hat sowie Login‑Events. | Trennung von technischen Logs (Compliance‑Risiko CD‑4). |
| **Backup Service** | Wöchentliche Snapshots des Managed DB, Aufbewahrung nach gesetzlichen Vorgaben. | Teil des MVP‑Scope (Backup/DR). |
| **Monitoring & Alerting** | Health‑Checks, Performance‑Metriken, Rate‑Limiting‑Statistiken. | Nicht‑personenbezogene Daten. |
| **Email Service** | Versand von Double‑Opt‑In‑Mails, Passwort‑Reset, ggf. Angebots‑PDF‑Benachrichtigung. | GDPR‑konform, keine personenbezogenen Daten im Mail‑Header. |

## 3. Schnittstellen / Integrationspunkte
1. **SAP → SAP Connector** – OData/REST‑Call, Auth über Service‑User, Lese‑Only. 
2. **Frontend ↔ API Layer** – HTTPS, JSON, OAuth‑Bearer‑Token. 
3. **API Layer ↔ Managed DB** – Standard‑SQL‑/NoSQL‑Treiber (je nach gewähltem Service). 
4. **Auth Service ↔ Email Provider** – SMTP/REST für Double‑Opt‑In‑Mails. 
5. **Backup Service ↔ Managed DB** – API‑basiertes Snapshot‑Triggern. 
6. **Monitoring ↔ API Layer / DB** – Export von Metriken (z. B. Prometheus‑Exporter). 

## 4. Daten‑ und Sicherheitsaspekte
- **Datenfluss**: Kundendaten (Name, E‑Mail, Bestell‑/Rechnungs‑Infos) werden nur im Managed DB gespeichert; Produkt‑ und Preisdaten stammen in Echtzeit aus SAP.
- **Verschlüsselung**: TLS 1.2+ für sämtliche Netzwerk‑Kommunikation. Daten‑at‑Rest werden vom Managed DB Service verschlüsselt.
- **Authentifizierung & Autorisierung**: OAuth 2.0 (Password‑Grant für MVP). Rollen‑basiert (Admin, Sales, Kunde). 
- **Audit‑Logging**: Write‑only Audit‑Log (JSON) enthält **User‑ID**, **Timestamp**, **Operation**, **Objekt‑ID** – keine PII in technischen Logs. 
- **DSGVO**: Double‑Opt‑In für Registrierung, Möglichkeit zur Datenlöschung über Kontakt‑Formular (nicht automatisch im MVP). Daten‑Residenz EU‑only, Hosting‑Provider muss Nachweis erbringen. 
- **Backup & DR**: Wöchentliche immutable Snapshots, Aufbewahrung 30 Tage (kann je nach rechtlicher Vorgabe erweitert werden). 
- **Rate‑Limiting**: Token‑Bucket‑Mechanismus (z. B. 100 Requests / Minute pro User) auf API‑Ebene; schützt vor Missbrauch bis ein API‑Gateway verfügbar ist. 

## 5. Offene/Unklare Architekturentscheidungen (MVP‑Bewusst‑Ausgeschlossene)
| Entscheidung | Grund für Offenlegung | Geplanter Ansatz (nach MVP) |
|--------------|-----------------------|----------------------------|
| **API‑Gateway** | Warteliste 6 Wochen, nicht im 8‑Wochen‑Zeitrahmen. | Migration zu zentrales Unternehmens‑Gateway, Nutzung von OAuth‑Scopes. |
| **Managed DB Anbieter** | Kosten und EU‑Only‑Garantie noch nicht quantifiziert. | Auswahl zwischen Azure PostgreSQL EU‑Region, AWS RDS EU, etc., inkl. Kosten‑Schätzung bis Vorstandspräsentation. |
| **SSO Integration** | Azure AD / Google vorgeschlagen, aber optional und zeitintensiv. | Implementierung als Add‑On nach MVP, ggf. über OpenID‑Connect. |
| **Caching von SAP‑Daten** | Daten‑Minimierung vs. Performance – Off‑Topic für MVP. | Einführung eines read‑through Caches (Redis) mit kurzer TTL in Phase 2, inkl. Datenschutz‑Review. |
| **Erweiterte Logging / SIEM** | Trennung von Audit‑ und technischen Logs definiert, aber konkrete Umsetzung fehlt. | Integration von strukturierten Audit‑Logs in zentralen Log‑Store (z. B. Azure Log Analytics) nach MVP. |
| **Secrets‑Management** | Noch nicht implementiert, aber kritisch für DB‑ und API‑Credentials. | Nutzung von Managed Key‑Vault (Azure Key Vault / AWS Secrets Manager) ab Phase 2. |
| **Kosten‑Schätzung für Hosting/Backup** | Noch offen – Risiko für Vorstand. | Erarbeiten eines Budget‑Modells (CAPEX/OPEX) bis Sprint‑0‑Review. |
| **Support‑Workflow** | Kein Ticket‑System im MVP, nur Kontakt‑Formular. | Definition eines minimalen Ticket‑Backlog‑Systems (z. B. ServiceNow) nach MVP. |
| **Mehrwährungs‑ und Mehrsprachigkeits‑Support** | Schweiz‑Pilot vs. DACH‑Launch unsicher. | Internationalisierung (i18n) und Währungs‑Engine (EUR/CHF/USD) in Phase 2. |

## 6. Zusammenfassung
Der Entwurf fokussiert sich auf ein **minimal funktionsfähiges Kundenportal** für das 8‑Wochen‑MVP, das **Login**, **Angebots‑Erstellung (SAP‑Read‑Only)**, **Rechnungs‑Download**, **Rollen‑ und Audit‑Log** sowie **EU‑Managed‑Hosting mit Backup** bereitstellt. Kritische Abhängigkeiten (SAP‑Verfügbarkeit, fehlendes API‑Gateway) werden bewusst als **Risiken** gekennzeichnet und durch einfache Fallback‑Strategien (Echtzeit‑Lesen, interne OAuth‑Schicht) adressiert. Sicherheits‑ und Datenschutz‑Grundlagen (TLS, Double‑Opt‑In, getrennte Audit‑Logs) sind vorhanden, jedoch werden weiterführende Maßnahmen (SSO, Caching, Secrets‑Management, umfassendes Monitoring) für spätere Phasen reserviert.

*Dieses Architektur‑Artefakt wurde aus den Kontext‑ und Risiko‑Dokumenten (runs/phase2_1/20260529_101357_d4d49c/state/context.md, docs/risks.md) sowie dem Stakeholder‑Transkript (input/transcripts/T9999_chaos.txt) abgeleitet.*