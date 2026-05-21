
### Architektur

Hier ist ein grundlegender Entwurf, der die vom Team beschriebenen Anforderungen berücksichtigt:

#### Komponenten
- **Frontend**: Kundenportal (Web-Ansicht) mit optionaler Mobile App.
  - Technologie: Responsive Web Design oder Native App für spätere Implementierung.
- **Backend**: API Layer für Datenzugriff und integrierte Anwendungen (z.B. SAP).
  - Sicherheit: OAuth-Basierte Authentifizierung, TLS für Verschlüsselung.
  - Performance: Managed Services zur Skalierbarkeit ohne neuen DB Server.
- **SAP Integration**: Erweiterung des Backends um SAP-Datenzugriff (Produkte, Preise, Rabattlogik).

#### Datenverwaltung
- DSGVO-konforme Lösungen für Kundendatenspeicherung und -bearbeitung.
  - Logging: Protokollierung von Zugriffen und Änderungen.
  - Löschkonzept: Implementierung nach DSGVO-Vorschriften.

#### Compliance & Sicherheit
- Compliance mit DSGVO durch Einhaltung der Vorgaben für Datenschutz und Rechte der Betroffenen.
- Sicherheitsmaßnahmen wie OAuth-Basierte Authentifizierung und TLS-verschlüsselter Datenverkehr.