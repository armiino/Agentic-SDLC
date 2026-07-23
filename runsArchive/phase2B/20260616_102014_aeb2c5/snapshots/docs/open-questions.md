## Offene fachliche Fragen

1. **Pilotkunde‑Auswahl**: Soll der Pilotkunde die Müller AG (Schweiz) oder die Hansa GmbH (Deutschland) sein? Welche Auswirkungen hat die Entscheidung auf Währung (CHF vs. EUR) und Daten‑Residenz?
2. **Mehrwährungs‑Support**: Wird CHF neben EUR bereits im MVP unterstützt oder erst in späteren Releases?
3. **MVP‑Umfang**: Welche konkreten Features werden bewusst ausgeschlossen (z. B. Push‑Notifications, Ticket‑System, Mobile‑First UI)?
4. **API‑Gateway‑Verfügbarkeit**: Ist das geplante API‑Gateway rechtzeitig für das MVP einsetzbar oder benötigen wir eine Übergangslösung?
5. **Retention‑Policy**: Welche Aufbewahrungsfristen gelten für Angebote, Rechnungen und Log‑Daten (gesetzliche Vorgaben vs. DSGVO‑Löschrecht)?
6. **Testdaten‑Strategie**: Werden synthetische Daten oder reale SAP‑Daten im Test‑Setup verwendet?
7. **Support‑Prozess & SLA**: Welche Service‑Level‑Agreements gelten für das Kontaktformular im MVP?
8. **Rabatt‑Freigabe‑Workflow**: Wie detailliert muss der Freigabe‑Workflow (Automatisierung, UI‑Hinweise) implementiert werden?
9. **Kosten‑Schätzung für EU‑Hosting**: Wie hoch sind die erwarteten Kosten und gibt es zusätzliche Kosten für ein mögliches Schweizer‑Hosting?
10. **Daten‑Residenz für Schweizer Pilot**: Darf das System Daten des Schweizer Kunden in EU‑Regionen speichern oder ist ein separates Hosting nötig?

## Offene technische Fragen

1. **Rate‑Limiting ohne API‑Gateway**: Wie soll grundlegendes Rate‑Limiting technisch umgesetzt werden (z. B. Middleware, Service‑Mesh)?
2. **Secrets‑Management & Schlüsselrotation**: Welche konkrete Lösung (z. B. Cloud‑KMS) wird genutzt und wie wird die Rotation automatisiert?
3. **Backup‑ & Disaster‑Recovery‑Strategie**: Reicht das tägliche Backup oder wird ein Hot‑Standby in einer zweiten EU‑Region benötigt?
4. **Logging‑Policy ohne PII**: Wie werden personenbezogene Daten in Logs maskiert bzw. pseudonymisiert?
5. **Performance‑Tests**: Wie wird das Last‑Testing für 20.000 gleichzeitige Nutzer geplant und durchgeführt?
6. **PDF‑Export & Versionierung**: Wie wird die Versionierung der PDFs technisch umgesetzt (Datenbank, Object‑Store)?
7. **SAP‑Connector Integration**: Welche Fehler‑ und Timeout‑Strategien sind für den Lesezugriff auf das SAP‑System vorgesehen?
8. **Auth‑Service & Double‑Opt‑In**: Welche Schritte umfasst der Double‑Opt‑In‑Flow und wie wird die Verifikation sicher gestellt?
9. **Mobile‑First vs. Web‑First Priorisierung**: Welche UI‑Entwicklungsstrategie wird verfolgt, bis die Entscheidung getroffen ist?
10. **Monitoring & Alerting**: Welche Metriken werden überwacht und welche Alarmierungsmechanismen werden eingesetzt?

## Widersprüche, die geklärt werden müssen

- **Security‑Review (6 Wochen) vs. 8‑Wochen‑MVP**: Der Review könnte Änderungen verlangen, die den Zeitplan gefährden.
- **Budget‑Beschränkungen vs. benötigte Sicherheitskomponenten**: Managed Services sind budgetfreundlich, aber das Fehlen eines API‑Gateways und erweiterter DR‑Pläne könnte zusätzliche Kosten verursachen.
- **EU‑Only‑Hosting vs. Schweizer Pilot**: Unklar, ob Daten des Schweizer Kunden in EU‑Regionen zulässig sind.
- **Kein neues DB‑System vs. Speicherung von Angeboten, Rechnungen und Audit‑Logs**: Es fehlt eine klare Entscheidung, wie diese Daten persistent gespeichert werden.

## Fehlende Informationen

- Detaillierte **User Stories** und **Akzeptanzkriterien** für jede Rolle (Admin, Sales, Kunde).
- Konkrete **SLA‑Definition** für die System‑Uptime (z. B. 99,5 % vs. 99,9 %).
- **Finanzierung** für mögliche zusätzliche Infrastruktur (z. B. API‑Gateway, Hot‑Standby).
- Endgültige **Entscheidung** über den **Mehrwährungs‑Support** im MVP.
- Ausführliche **Retention‑Policy** inkl. automatisierter Löschroutinen.

## Mögliche Ansprechpartner / Rollen

- **Anna** (Produkt‑/Business Owner) – Entscheidung über Pilotkunde, MVP‑Umfang, Priorisierung Mobile vs. Web.
- **Ben** (Technischer Lead / Architekt) – Technische Machbarkeit API‑Gateway, SAP‑Connector, Secrets‑Management.
- **Clara** (Compliance & Datenschutz) – Daten‑Residenz, Retention‑Policy, DSGVO‑Konformität.
- **Eva** (Finance / Controlling) – Kosten‑Schätzung Hosting, Budget‑Freigaben.
- **David** (Customer Support) – Support‑Prozess und SLA für das Kontaktformular.
- **Farid** (IT Operations) – Backup‑Strategie, Disaster‑Recovery, Monitoring.

*Alle genannten Punkte leiten sich direkt aus den bereitgestellten Artefakten ab und sollten in den nächsten Abstimmungsrunden geklärt werden.*