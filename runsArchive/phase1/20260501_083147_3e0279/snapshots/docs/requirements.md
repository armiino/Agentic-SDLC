# Anforderungen

## Funktionalanforderungen
- Kundenportal zur Erstellung von Angeboten und Downloads von Rechnungen
- Login mit E-Mail und Passwort, optional SSO (z.B. Azure AD oder Google)
- Rollenmanagement für Admins, normalen Users, Manager und Support
- SAP Integration für Produkt-, Preis- und Rabattlogik
- Kein neuer DB Server, Managed Services verwenden
- API Layer zur Integration von Angebotserstellung und Rechnungsdownload
- Backup und Disaster Recovery
- Skalierbarkeit für bis zu 20.000 Nutzer

## Nicht-funktionale Anforderungen
- DSGVO-konforme Datenverarbeitung (Double-Opt-In, Logging, Löschkonzepte)
- Verschlüsselung (TLS)
- Auditierbarkeit und Zugriffskontrolle
- Keine native App für Mobilgeräte im MVP
- API muss OAuth-basiert sein
- KPIs messen: Conversion Rate, Zeit bis Angebot
