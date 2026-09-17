## Functional Requirements
- **Kundenportal**: Kunden können Angebote, Bestellungen und Rechnungen einsehen und als PDF herunterladen.
- **Login**: Authentifizierung per E‑Mail + Passwort mit Double‑Opt‑In; optionale SSO‑Integration (Azure AD, Google) wird im MVP nicht umgesetzt.
- **Rollen & Berechtigungen**: Rollen Admin, Sales, Manager, Support. Support‑Mitarbeiter dürfen Kundendaten einsehen, jedoch keine Preis‑/Rabatt‑Details.
- **Angebots‑Workflow**: Zustände Draft → Pending Approval → Approved → Sent. Freigabe erforderlich bei Rabatt > **15 %** (Schwellenwert muss noch bestätigt werden).
- **SAP‑Integration**: Lesezugriff auf Produkt‑ und Preisdaten aus SAP; Schreibzugriff (Auftragserstellung) ist für spätere Phasen vorgesehen.
- **PDF‑Export**: Rechtlich konforme PDF‑Erstellung mit Fußnoten für Angebote und Rechnungen.
- **Mehrwährung**: Unterstützte Währungen EUR (Standard) und CHF (Pilotkunde); USD später.
- **Analytics/KPIs**: Erfassung von Conversion‑Rate, Zeit bis Angebot, Nutzung‑Statistiken; Consent‑Management für Tracking ist erforderlich.
- **Support‑Kontakt**: Kontaktformular für Kunden; kein Ticket‑System im MVP.

## Non-functional Requirements
- **DSGVO‑Konformität**: Double‑Opt‑In, umfassendes Logging, Audit‑Trail, Löschkonzept, Datenminimierung.
- **Security**: TLS‑Verschlüsselung, grundsätzlicher Security Review (Detail‑Review ggf. nach MVP).
- **Hosting & Infrastruktur**: EU‑only Managed Service, keine eigene Datenbank, Backup & Disaster Recovery inkl..
- **Performance**: API‑Gateway (Verfügbarkeit nach 6‑Wochen‑Warteliste), Rate‑Limiting und Pagination für Downloads.
- **Availability**: Ziel‑Uptime 99,5 % (Annahme).
- **Monitoring**: Trennung von technischen Logs und Audit‑Logs, festgelegte Aufbewahrungsfristen.
- **Scalability**: Architektur ermöglicht spätere Mobile‑First‑ bzw. native App‑Entwicklung.
- **Accessibility**: Responsive Web‑Design (Web‑first Ansatz).

## Constraints/Compliance
- EU‑only Hosting (Datenresidenz).
- Nutzung ausschließlich Managed Services; keine eigenständige Datenbank.
- DSGVO‑Anforderungen (Logging, Audit, Löschkonzept, Double‑Opt‑In).
- SSO‑Integration und native App nicht im MVP‑Umfang.
- API‑Gateway‑Implementierung erst nach 6‑Wochen‑Warteliste verfügbar.

## Assumptions and Open Points
- **SSO‑Integration** wird erst nach MVP implementiert.
- **Rabatt‑Freigabe‑Schwellenwert** (15 %?) und Verantwortlichkeiten (Manager vs. Finance) sind noch offen.
- Entscheidung über **Pilotkunde** (Schweiz vs. Deutschland) beeinflusst Währung und rechtliche Vorgaben.
- **Daten‑Minimierung vs. Echtzeit‑SAP‑Abfrage**: Entscheidung über Caching‑Strategie fehlt.
- Umfang von **Backup/Disaster Recovery** im MVP ist nicht final definiert.
- **Analytics/Tracking‑Infrastruktur** (Consent‑Management) muss noch spezifiziert werden.
- **Support‑Prozess**: Kein Ticket‑System, aber strukturierte Nachverfolgung wird gefordert.
- Nutzung von **SAP‑Testdaten** mit echten Kundendaten muss datenschutzkonform gelöst werden.
- **Security Review** von 6 Wochen kollidiert mit 8‑Wochen‑MVP‑Zeitplan – mögliche Verschiebung.

## Traceability
| Anforderung | Quelle (Stakeholder) |
|---|---|
| Kundenportal Grundfunktion | Anna |
| Login mit Double‑Opt‑In | Anna, Clara |
| Rollen & Berechtigungen | Anna, David |
| Angebots‑Workflow mit Freigabe | Anna, Eva |
| SAP Leseintegration | Ben |
| PDF‑Export | Eva |
| Mehrwährung (EUR, CHF) | Anna |
| Analytics/KPIs | Anna |
| DSGVO Logging & Audit | Clara |
| EU‑only Hosting | Farid |
| Managed Service, keine DB | Farid |
| Security Review | Ben, Clara |
| Backup/DR | Farid |
| SSO‑Integration (optional) | Ben |
| Mobile vs Web‑first Entscheidung | Anna, Ben |
| Rabatt‑Schwellenwert Klärung | Eva |
| Pilotkunde Entscheidung | Anna |
| Support‑Kontaktformular | David |
| Testdaten Datenschutz | Clara |
