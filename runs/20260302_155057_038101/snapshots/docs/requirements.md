# Anforderungen
## Funktionale Anforderungen
- **Kundenportal:** Ein Web-basiertes Kundenportal zur Interaktion mit der Plattform.
- **Angebote Erstellen:** Nutzer können Angebote für Produkte erstellen, die auf dem Portal verfügbar sind.
- **Rechnungsdownload:** Nutzer haben Zugriff auf Rechnungen, die aus ihren Bestellungen generiert wurden.
- **Login:** Ein Login-System ist erforderlich, mit der Möglichkeit zur E-Mail-Benachrichtigung und Passwort-Anmeldung. Die Option für SSO (Single Sign-On) über Azure AD oder Google sollte erörtert werden.
- **Rollenmodelle:** Definierung von Rollen wie Admin, Normaler Benutzer, Manager und Support benutzer.
- **SAP Integration:** Das Portal muss in der Lage sein, Daten aus SAP zu integrieren um Angebote zu generieren.
- **Keine neue DB Server:** Es wird eine Lösung gesucht, die ohne neuen Data Base Server ausgeführt werden kann.
## Nicht-funktionale Anforderungen
- **Zeitliche Frist für MVP (Minimum Viable Product):** Das Produkt sollte in 8 Wochen fertiggestellt sein.
- **DSGVO Compliance:** Das Projekt muss DSGVO-Kompatibilität gewährleisten, einschließlich der Einwilligung für Push-Benachrichtungen und Tracking-Aktivitäten.
- **Verschlüsselung:** TLS-verschlüsselte Verbindungen sind notwendig für sicherheitsrelevante Datenübertragungen.
- **Auditierbarkeit:** Das System muss in der Lage sein, Audit Logs zu generieren, die Änderungsaktivitäten aufzeichnen und dokumentieren.
- **API Layer:** Ein API-Layer ist erforderlich um eine Integration mit externen Diensten zu ermöglichen.
- **OAuth-Security:** Die OAuth-Authentifizierung wird als optimale Lösung zur Sicherheitsabdeckung der API empfohlen.
## Compliance-Anforderungen
- **DSGVO-Kompatibilität**: Eine detaillierte Implementierung, die den Anforderungen des Datenschutzes entspricht.
- **Zugriffskontrolle:** Die Systemarchitektur muss eine Zugriffskontrolle implementieren.