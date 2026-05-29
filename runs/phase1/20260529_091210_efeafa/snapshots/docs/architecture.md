# Architektur

- Kundenportal als Webanwendung, mobile native App später möglich
- API Layer als zentrale Integrationsschicht, OAuth bevorzugt, noch nicht final
- Nutzung von Managed Services, keine neuen Datenbanken
- EU-only Hosting mit nachweisbarer Datenresidenz
- Backup und Disaster Recovery als Pflicht
- Rollen- und Berechtigungskonzept mit Admin, Sales, Kunde
- Minimaler Audit Trail für Änderungen und Zugriff
- Logging getrennt nach technischen Logs und Audit Logs
- Test-, Entwicklungs- und Produktionsumgebungen mit Pseudonymisierung
- API Gateway Integration geplant, aber Warteliste blockiert MVP
- SAP-Integration nur lesend im MVP
- Keine Online-Akzeptanz von Angeboten im MVP
- PDF-Export mit Versionierung und rechtlichen Fußnoten
- KPI-Messung und Monitoring mit Datenschutz
- Secrets Management für CI/CD
- Rate Limiting und Missbrauchserkennung, wenn API Gateway verfügbar
- Offene Architekturentscheidungen dokumentieren
