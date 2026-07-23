# Functional Requirements

- Kundenportal (Web-first, mobile später)
- Login mit E-Mail und Passwort, Double-Opt-In für DSGVO
- Rollen: Admin, Sales, Kunde (minimal)
- Angebotserstellung mit SAP-Lesedaten (keine Sonderrabatte im MVP)
- Rechnungsdownload
- API Layer für Integration (OAuth bevorzugt, noch nicht final)
- Backup und Disaster Recovery für Kundendaten
- EU-only Managed Hosting, DSGVO-konform
- Minimaler Audit Trail (wer hat wann was geändert)
- KPI-Messung: Conversion Rate, Zeit bis Angebot

# Non-functional Requirements

- Skalierbarkeit für 200 bis 20.000 Nutzer (ohne Overengineering)
- Sicherheit: TLS-Verschlüsselung, kein E2E im MVP
- Logging und Audit getrennt, keine personenbezogenen Daten in technischen Logs
- Test-, Entwicklungs- und Produktionsumgebungen mit Pseudonymisierung oder synthetischen Testdaten
- Rate Limiting und Missbrauchserkennung (sofern API Gateway verfügbar)

# Constraints/Compliance

- DSGVO-konformität zwingend, inklusive Löschkonzept und Auftragsverarbeitungsverträge
- Keine neuen Datenbanken, Nutzung von Managed Services
- EU-only Hosting mit nachweisbarer Datenresidenz
- Keine Sonderrabatte ohne Freigabe im MVP
- Keine Support-Ticketpersistenz im MVP, nur Kontaktformular (Risiko)
- Handelsrechtliche Aufbewahrungspflichten beachten
- Keine OAuth-Integration im MVP, nur Vorbereitung

# Traceability

- Anforderungen basieren auf Stakeholder-Transkript T9999_chaos.txt
- Widersprüche und offene Fragen dokumentiert in open-questions.md
- Risiken dokumentiert in risks.md
- Architekturentscheidungen noch offen, dokumentiert in architecture.md
