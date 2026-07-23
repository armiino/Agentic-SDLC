# Anforderungen

## Funktionalanforderungen:
1. Ein Kundenportal mit Web und möglicherweise mobiler Version.
2. Login-Verfahren per E-Mail und Passwort, optional Single Sign-On (SSO).
3. Rollenmanagement: Admin, normaler Benutzer, Manager.
4. Integration in SAP für Produkt- und Preisdaten sowie Rabattlogik.
5. Erstellung von Angeboten durch das Portal.
6. Download von Rechnungen aus dem Portal.
7. Möglichkeit zur Verschlüsselung und Sicherheitsüberprüfung (TLS).
8. Logging und Auditierbarkeit der Systemeinsätze.
9. DSGVO-konforme Lösung, einschließlich Datenschutzbestimmungen wie Double-Opt-In, Auditrails, Zugriffskontrolle und Rollenmodelle.
10. KPIs messen: Conversion Rate von Angebot zu Bestellung und Zeit bis Angebot.
11. Messaging-Funktionalität (Push Notifications).
12. Anbindung an Managed Services ohne neuen Datenbank-Server.
13. API-Layer mit OAuth für sichere Abwicklung.

## Nicht-funktionale Anforderungen:
1. Skalierbarkeit: Das System muss für 200 bis maximal 20.000 Nutzer skalieren können.
2. Sicherheit: Verschlüsselung, TLS für End-to-End-Verschlüsselung, OAuth als API-Sicherheitsmethode.
3. Compliance: DSGVO-konforme Lösungen, Datenschutzbestimmungen und Recht zur Datenspeicherung/Löschkonzept.
4. Zeitplan: MVP in 8 Wochen.

## Einschränkungen/Compliance:
1. Kein neuer Datenbank-Server.
2. EU-only Hosting für Kundendaten oder zumindest DSGVO-konform.
3. Backup und Disaster Recovery Konzepte im Falle von Kundendatenverlusten.
4. Dokumentation zur Architektur, Rollenmanagement und Sicherheitspraktiken.