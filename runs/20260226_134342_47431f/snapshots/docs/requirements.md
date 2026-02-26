# Funktionale Anforderungen
- Kundenportal erstellen
- Angebote generieren und verwalten
- Rechnungsdownload ermöglichen
- Login-System implementieren (E-Mail und Passwort)
- Rollenmodelle für unterschiedliche Nutzer definiert: Admin, normaler User, Manager?
- SAP-Datenintegration für Produktinformationen und Preise

# Nicht-funktionale Anforderungen
- DSGVO-konform (Double Opt-In, Logging, Löschkonzept)
- API Layer mit OAuth oder API Keys Sicherheit
- Skalierbarkeit bis zu 20.000 Nutzern
- Verschlüsselung durch TLS
- Disaster Recovery und Backup für Kundendaten
- Performance: Schnelles Angebotserstellen

# Beschränkungen/Compliance
- Keine neuen DB-Server aufsetzen, Managed Services bevorzugen
- Daten in EU gehostet oder zumindest DSGVO-konform
- Compliance mit personenbezogenen Daten (Audit Trails)
- Security Review durchführen, jedoch ohne Overengineering
- Dokumentation für einen Security Review bereitstellen