# Anforderungen

## Functional Requirements
- Kundenportal (Web‑First, Mobile optional) für Kunden zur Ansicht von Bestellungen und zum Download von Rechnungen.
- Angebotserstellung im Portal für Sales inkl. Produkt‑, Preis‑ und Rabattlogik.
- Login per E‑Mail/Passwort, optional SSO (Azure AD, Google) – Identity‑Provider‑Integration.
- Rollen‑ und Berechtigungskonzept (Admin, User, Manager, Support).
- Integration mit SAP für Stammdaten, Produkt‑ und Angebotsdaten.
- API‑Layer (REST) zur Anbindung von Frontend, SAP und zukünftigen Services, gesichert via OAuth (oder API‑Keys).
- Logging aller Änderungen (Audit‑Trail) und Aktivitäts‑Logging.
- DSGVO‑konforme Funktionen: Double‑Opt‑In, Einwilligungs‑Management, Datenlöschung, EU‑only Hosting.
- KPI‑Erfassung (Conversion Rate, Zeit bis Angebot) – optional Analytics.
- Backup & Disaster Recovery für Kundendaten.
- Skalierbarkeit für 200‑20.000 Nutzer, Managed Services, keine eigene DB‑Instanz.

## Non-functional Requirements
- Sicherheit: TLS für Datenübertragung, OAuth für API, Rollen‑basiertes Zugriffskontrollsystem, Audit‑Logs.
- Datenschutz: DSGVO‑Konformität, EU‑Region Hosting, Löschkonzept, Double‑Opt‑In.
- Performance: Antwortzeit < 2 s für Kern‑Funktionen, skalierbare Infrastruktur.
- Verfügbarkeit: 99,5 % Uptime, Backup/Recovery innerhalb von 4 h.
- Wartbarkeit: Dokumentation des Architektur‑ und Sicherheitskonzepts, minimaler Over‑Engineering‑Ansatz.
- Zeitrahmen: MVP in 8 Wochen, Security Review muss parallel stattfinden.

## Constraints/Compliance
- Keine neue Datenbank‑Server‑Installation – Nutzung von Managed Services.
- EU‑only Datenhosting, DSGVO‑Konformität.
- Budget‑beschränkt – keine teuren nativen Mobile‑Apps, primär responsive Web.
- Security Review ist Pflicht, darf nicht ausgelassen werden.

## Traceability
| Anforderung | Quelle |
|-------------|--------|
| Kundenportal & Angebote | Anna (Bedarf an schnellem Angebotserstellung) |
| Rechnungsdownload | Anna |
| Login & SSO | Anna, Ben |
| Rollen & Berechtigungen | Clara, Anna |
| SAP Integration | Ben |
| API Layer | Ben |
| DSGVO (Double‑Opt‑In, Logging, Löschkonzept) | Clara |
| KPI‑Messung | Anna |
| Backup/DR | Clara |
| Skalierbarkeit & Managed Services | Ben |
| MVP 8 Wochen | Anna |
| Security Review | Clara |
