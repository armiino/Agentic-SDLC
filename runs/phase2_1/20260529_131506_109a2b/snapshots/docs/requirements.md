## Functional Requirements
- Kundenportal als Webanwendung mit responsive Design für späteren Mobile Support.
- Benutzer-Login mit E-Mail und Passwort sowie Double-Opt-In zur DSGVO-Konformität.
- Rollenbasiertes Berechtigungssystem mit mindestens den Rollen Admin, Sales, Kunde und Support (eingeschränkter Zugriff).
- Angebote erstellen basierend auf SAP-Daten mit Einschränkung auf Standardrabatte im MVP.
- Rechnungsdownload für Kunden über das Portal.
- Integration einer lesenden SAP-Schnittstelle für Produktdaten, Preise und Rabattinformationen.
- API-Layer zur Integration, OAuth als bevorzugte Authentifizierungsmethode (noch nicht final).
- Minimaler Audit-Trail zur Protokollierung von Änderungen und Zugriffen.
- Backup und Disaster Recovery Mechanismen für Kundendaten.
- Kontaktformular für Supportanfragen ohne persistentes Ticketsystem im MVP.
- Mehrsprachigkeit (Deutsch, Englisch) und Mehrwährung (EUR, ggf. CHF) als Option für spätere Erweiterungen.

## Non-functional Requirements
- EU-only Hosting oder nachweislich DSGVO-konformes Hosting mit Datenresidenz.
- TLS-Verschlüsselung für die Übertragung von Daten, keine End-to-End Verschlüsselung notwendig.
- Skalierbarkeit für Nutzerzahlen zwischen 200 und 20.000, jedoch ohne Overengineering.
- Sicherheit: Security Review verpflichtend, Logging mit klarer Trennung zwischen Application Logs, Audit Logs und Security Logs.
- Testumgebungen mit anonymisierten oder synthetischen Daten, Vermeidung von Echtkundendaten im Test.
- Secrets Management und API Rate Limiting zur Verhinderung von Missbrauch.
- Produktionsumgebungen mit klarer Unterscheidung zu Entwicklungs- und Testumgebungen.

## Constraints/Compliance
- Einhaltung der DSGVO inklusive Löschkonzept, Double-Opt-In, Auditierbarkeit und Datenminimierung.
- Keine Speicherung personenbezogener Daten in technischen Logs.
- Handelsrechtliche Aufbewahrungsfristen für Angebote und Rechnungen müssen berücksichtigt werden.
- Keine neuen Datenbankserver, Einsatz von Managed Services für Datenbank- und Backup-Funktionen.
- Einschränkungen für Gutschein- und Rabattlogik im MVP: keine Sonderrabatte ohne Freigabe.
- API Gateway Nutzung erforderlich, aber Verfügbarkeit nicht innerhalb des MVP-Zeitrahmens garantiert.
- Supportdatenverarbeitung über E-Mail als bewusste Einschränkung mit damit verbundenen Compliance-Risiken.
- Hosting und Datenspeicherung in der EU mit Berücksichtigung der speziellen Bedingungen für Schweizer Kunden.

## Assumptions and Open Points
- MVP wird eine Webanwendung sein, native mobile Apps sind für spätere Releases vorgesehen.
- SSO-Integration ist optional und wird nicht für das MVP erwartet.
- Supportprozess ohne strukturiertes Ticketsystem, nur Kontaktformular als MVP.
- Rabattfreigaben und komplexe Angebotsworkflows werden in späteren Phasen behandelt.
- Pilotkunde und deren Standort (DACH oder Schweiz) sind noch nicht final entschieden.
- Schreibzugriff auf SAP für Bestellungen ist unklar und wird im MVP nicht umgesetzt.
- Backup, Monitoring und Security Review müssen noch konkret geplant und abgestimmt werden.
- API Gateway kann innerhalb des MVP-Zeitrahmens nicht garantiert werden, mögliche manuelle Workarounds.
- KPIs und Analytics sind nicht Bestandteil des MVP, aber zukünftige Anforderungen.
- Cache-Strategien und Fallbacks bei SAP-Ausfällen sind nicht final entschieden.
- Testdaten und Umgang mit SAP-Testsystem mit realen Kundendaten müssen geregelt werden.

## Traceability
- Anforderungen basieren auf der umfassenden Analyse des Transkripts input/transcripts/T9999_chaos.txt.
- Konflikte und Risiken aus dem Gespräch wurden nicht glättet, sondern als offene Punkte dokumentiert.
- Die durch Stakeholder genannten Wünsche und Bedenken wurden berücksichtigt und in klare Anforderungen, Annahmen und Constraints differenziert.
- Das MVP-Scope ist als Kompromiss zwischen Business-Zielen, Compliance und Zeitrestriktionen formuliert.
- Es besteht ein direkter Bezug zu Datenschutzanforderungen und technischen Restriktionen, die im Gespräch mehrfach thematisiert wurden.
