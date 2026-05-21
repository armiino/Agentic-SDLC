# Architekturübersicht

- Web-basiertes Kundenportal als MVP, mobile App optional später
- API Layer als zentrale Integrationsschicht für SAP und andere Systeme
- Nutzung von Managed Services für Datenbank und Infrastruktur (kein eigener DB-Server)
- Authentifizierung: E-Mail/Passwort, optional SSO (Azure AD, Google)
- Rollen- und Berechtigungskonzept mit Admin, User, Manager, Support (optional)
- Logging und Audit Trails zur Nachvollziehbarkeit von Änderungen
- DSGVO-konformes Hosting in der EU
- TLS-Verschlüsselung für Datenübertragung
- Backup und Disaster Recovery als Pflicht
- Skalierbarkeit für unterschiedliche Nutzerzahlen (200 bis 20.000)
- Verzicht auf native mobile App im MVP aus Budget- und Zeitgründen
- Dokumentation für Security Review
