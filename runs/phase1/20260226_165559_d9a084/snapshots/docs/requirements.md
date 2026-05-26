# Anforderungen

## Funktionale Anforderungen:
1. Kundenportal mit Web-Interface.
2. Integration von SAP für Produktinformationen (Preise, Rabatte usw.).
3. Erstellung von Angeboten durch Sales.
4. Rechnungsdownload.
5. Logging und Auditierbarkeit.

## Nicht-funktionale Anforderungen:
1. DSGVO-Kompatibilität.
2. KPIs: Conversion Rate (Angebot → Bestellung) und Zeit bis Angebot.
3. Verschlüsselung über TLS.
4. Keine E2E-Verschlüsselung erforderlich.
5. Skalierbarkeit für verschiedene Anzahl von Benutzern.
6. DSGVO-konforme Datenspeicherung (EU only).
7. Sicherheitsüberprüfungen, aber in 8 Wochen nicht realistisch.

## Einschränkungen/Compliance:
1. Keine neue DB-Server Installation möglich.
2. Managed Services verwenden, wenn möglich.
3. Backup und Disaster Recovery für Kundendaten.
4. Logging mit Double Opt-In für E-Mail Login.