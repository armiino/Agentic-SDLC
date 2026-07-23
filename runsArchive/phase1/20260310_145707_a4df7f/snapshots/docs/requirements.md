# Requirements
## Funktionaler Anforderungen
- **Kundenportal**: Zugriff auf Kundeninformationen, Bestellungen, Rechnungen.
- **Angebote erstellen** (Sales-Funktion): Schnelle Erstellung von Angeboten basierend auf Produkt-, Preis- und Rabattlogik.
## Nicht-funktionaler Anforderungen
- **Zeitrahmen**: MVP in 8 Wochen.
- **Wartbarkeit/Erweiterbarkeit**: API Layer, OAuth2 für Sicherheit.
- **Durchsetzbarkeit/Geschwindigkeit**: Schnelles Erstellen von Angeboten.
## Einschränkungen/Komplianzen
### Datenschutz (DSGVO Compliance)
- Login via E-Mail und Passwort
- Double Opt-In
- Logging
- Audit Trails
- SAP Integration
### Skalierbarkeit
- Mögliche Nutzung von Managed Services
- Keine neuen DB-Server