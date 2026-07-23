# Architektur

- Web-basiertes Kundenportal mit späterer mobiler Erweiterung
- API Layer als zentrale Integrationsschicht
- Integration mit SAP für Stammdaten, Produktdaten, Preise und Rabattlogik
- Authentifizierung: E-Mail/Passwort, optional SSO (OAuth)
- Rollen- und Berechtigungskonzept zur Zugriffskontrolle
- Logging und Audit Trails für Compliance
- Nutzung von Managed Services, kein eigener DB Server
- Hosting in der EU oder DSGVO-konform
- Backup und Disaster Recovery vorgesehen
- Skalierbarkeit für variable Nutzerzahlen (200 bis 20.000)
- MVP Fokus auf Kernfunktionen: Angebote, Rechnungen, Login
