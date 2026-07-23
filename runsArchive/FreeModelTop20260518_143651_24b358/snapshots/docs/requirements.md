# Anforderungen

## Functional Requirements
- Kundenportal (Web‑First, Mobile optional) für Kunden zur Ansicht von Bestellungen und zum Download von Rechnungen.
- Möglichkeit für Sales, Angebote im Portal zu erstellen.
- Login per E‑Mail/Passwort, optional SSO (Azure AD, Google) – Grundfunktionalität muss vorhanden sein.
- Rollen‑ und Berechtigungskonzept (Admin, User, Manager, Support) inkl. Audit‑Logging.
- Integration von SAP‑Stammdaten (Produktdaten, Preise, Rabattlogik) für Angebotserstellung.
- API‑Layer zur Anbindung von Frontend und SAP, gesichert (OAuth bevorzugt, alternativ API‑Keys).
- KPI‑Erfassung: Conversion Rate (Angebot → Bestellung) und Zeit bis Angebot.
- Datenlöschkonzept für Kunden‑Daten‑Löschungen.
- Backup & Disaster Recovery für Kundendaten.

## Non‑functional Requirements
- DSGVO‑Konformität: Double‑Opt‑In, Einwilligungs‑Management, Audit‑Trails, EU‑only Hosting.
- Sicherheit: TLS für Datenübertragung, Logging, Rollen‑basiertes Zugriffskontrollmodell.
- Skalierbarkeit: Unterstützung von 200 bis 20.000 gleichzeitigen Nutzern, Managed Services, keine eigene DB‑Instanz.
- Performance: akzeptable Antwortzeiten für Angebotserstellung und Portal‑Nutzung.
- Verfügbarkeit: Backup/DR, 99,9 % Uptime angestrebt.
- Dokumentation: Architektur‑ und Sicherheitsdokumentation für Review.

## Constraints/Compliance
- Keine neue Datenbank‑Server‑Installation – Nutzung von Managed Services.
- EU‑Only Datenhosting bzw. DSGVO‑konforme Infrastruktur.
- Security Review muss innerhalb des 8‑Wochen‑MVP‑Zeitplans berücksichtigt werden.
- MVP‑Zeitrahmen: 8 Wochen, daher minimale Kernfunktionalität ohne Over‑Engineering.

## Traceability
| Quelle (Transkript) | Anforderung | Abschnitt |
|---------------------|-------------|-----------|
| Anna (Portal, Angebote, Rechnungen) | Kundenportal, Angebotserstellung, Rechnungsdownload | Functional Requirements |
| Ben (API Layer, Integration, Security) | API‑Layer, SAP‑Integration, OAuth, TLS, Backup | Functional / Non‑functional |
| Clara (DSGVO, Logging, Rollen) | DSGVO‑Pflichten, Logging, Rollen‑ und Berechtigungskonzept, Löschkonzept | Non‑functional |
| Anna (KPIs) | KPI‑Erfassung (Conversion Rate, Zeit bis Angebot) | Functional |
| Ben (Performance, Skalierbarkeit) | Skalierbarkeit, Managed Services, Performance | Non‑functional |
| Clara (Compliance) | EU‑Only Hosting, Audit‑Trails | Constraints/Compliance |
