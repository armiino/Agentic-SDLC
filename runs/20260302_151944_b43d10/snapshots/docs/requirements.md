### Funktionsanforderungen:
- Ein Kundenportal mit Web-Ansicht als MVP erstellt werden.
- Eine Möglichkeit zur Erstellung von Angeboten für Sales.
- Rechnungsdownload für Kunden ermöglicht.
- Logging und Sicherheitsfunktionen implementiert (z.B. TLS, OAuth, E-Mail Double Opt-In).
- SAP-Datenintegration (Produkte, Preise, Rabattlogik).
- SSO ist optional aber erwähnt.
- Keine neuen Datenbankserver, sondern Managed Services nutzen.

### Nicht-funktionale Anforderungen:
- Schnelle Erstellung von Angeboten als Kernfunktionalität.
- DSGVO-konforme Lösung mit Audit Trails und Löschkonzepten.
- Skalierbarkeit unter Berücksichtigung unterschiedlicher User-Volumina (200 bis 20.000).
- Compliance mit Datenverwaltung und Sicherheitsspezifikationen.

### Einschränkungen/Compliance:
- Kein neuer DB Server, sondern Managed Services nutzen.
- DSGVO-konforme Lösung mit Audit Trails und Löschkonzepten.
- EU-only Data Hosting oder zumindest DSGVO-kompatibel.
- Security Review trotz kurzen Zeitrahmens.