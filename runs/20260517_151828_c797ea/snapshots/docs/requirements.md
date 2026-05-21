# Anforderungen
## Funktional
- Kundenportal mit Web-Anwendung
- Login per E-Mail und Passwort, optional SSO (z.B. Azure AD oder Google)
- Rollenmanagement (Admin, normaler User, Manager, Support)
- SAP Integration für Produkt-, Preis- und Rabattdaten
- Angebotsgenerierung
- Rechnungsdownload
- Push-Benachrichtigungen
- API-Schicht für Integrierung
- Backup und Disaster Recovery
## Nicht-funktional
- DSGVO-Konformität (Double-Opt-In, Logging, Löschkonzepte)
- Verschlüsselung (TLS)
- Auditierbarkeit (Audit Trails)
- Skalierbarkeit
- Performance (bis zu 20.000 Benutzer)
## Beschränkungen/Compliance
- Keine neuen DB-Serwer, Managed Services verwenden
- EU-only Datenhaltung
- Security Review in 6 Wochen