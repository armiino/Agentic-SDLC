# Anforderungen
## Funktional Requirements
- Ein Kundenportal mit folgenden Funktionen:
  - Angebotserstellung
  - Rechnungs-Download
  - Login per E-Mail und Passwort
  - Mögliche SSO-Integration (Azure AD, Google)
  - Integration in SAP für Produkt-, Preis- und Rabattinformationen
- Rollensystem mit mindestens Admin, Benutzer und Manager.
## Non-Funktionale Requirements
- DSGVO-Konformität (Double Opt-In, Logging, Löschkonzepte)
- Verschlüsselung (TLS für Kommunikation)
- Skalierbarkeit
- Backup und Disaster Recovery Plan
## Constraints/Compliance
- Keine neuen Datenbankserver – Managed Services in Betracht ziehen.
- Echtzeit-Zugriff auf SAP-Daten (für Angebote).
## Traceability
- Kundenportal und Funktionen: [Transcript Chunk 1](#chunk-1)
- DSGVO-Anforderungen: [Transcript Chunk 2](#chunk-2)
- Verschlüsselung und Sicherheit: [Transcript Chunk 3](#chunk-3)
- Skalierbarkeit: [Transcript Chunk 4](#chunk-4)