## Functional Requirements

1. Das System muss ein Kundenportal bereitstellen, das primär webbasiert ist und später durch mobile Zugänge erweitert werden kann.
2. Das Kundenportal muss die Möglichkeit bieten, Angebote mithilfe von SAP-Lesedaten zu erstellen.
3. Kunden sollen ihre Rechnungen im Portal einsehen und herunterladen können.
4. Es muss ein Login-System mit E-Mail/Passwort und Double-Opt-In-Verfahren implementiert werden.
5. Optionale Single Sign-On (SSO) Integration mit Identity Providern wie Azure AD und Google soll möglich sein.
6. Das System muss Rollen für Admin, Sales, Kunde sowie mögliche Manager und Support bereitstellen.
7. Ein minimaler Audit-Trail zur Nachvollziehbarkeit von Änderungen an Angeboten und Kundendaten ist zu implementieren.
8. Eine API Layer als zentrale Integrationsschicht mit OAuth als bevorzugtem Authentifizierungsverfahren ist notwendig.
9. Ein Kontaktformular für Supportanfragen ohne Persistenz eines Ticketsystems ist im MVP enthalten.

## Non-functional Requirements

1. Das Kundenportal muss innerhalb von 8 Wochen als MVP funktionsfähig bereitgestellt werden.
2. Hosting muss EU-only und DSGVO-konform über Managed Services erfolgen, ohne neue Datenbanken vor Ort zu installieren.
3. Das System soll Backup- und Disaster-Recovery-Fähigkeiten bieten.
4. Eine Skalierbarkeit für eine Nutzerspanne von 200 bis potenziell 20.000 Nutzern ist angedacht, aber kein Overengineering wird im MVP verfolgt.
5. Logging und Audit-Maßnahmen müssen personenbezogene Daten in technischen Logs ausschließen.
6. Das System muss Anbindungsmöglichkeiten an SAP bereitstellen, auch wenn SAP-Verfügbarkeit als kritische Abhängigkeit gilt.

## Constraints/Compliance

1. Die Implementierung muss vollständig DSGVO-konform sein, inklusive Double-Opt-In, Löschkonzepten und Auftragsverarbeitungsverträgen.
2. Es dürfen im MVP keine Sonderrabatte ohne Freigabeprozess (mindestens ab 15 % Rabatt) gewährt werden.
3. Ein Security Review muss durchgeführt werden, auch wenn die Zeit dafür knapp ist.
4. Die Datenresidenz muss EU-only nachweisbar realisiert werden.
5. Technische Logs dürfen keine personenbezogenen Daten enthalten.
6. Testumgebungen dürfen keine echten Kundendaten ungeschützt verwenden.

## Assumptions and Open Points

1. Der endgültige Pilotkunde (Deutschland oder Schweiz) und die daraus resultierenden Datenschutz- und Währungsanforderungen sind noch offen.
2. KPI-Details und Analytics-Infrastruktur sind noch nicht definiert.
3. Umfang und Technologie der mobilen Nutzung sind noch unklar.
4. OAuth-Integration sowie Single Sign-On sind gewünscht, aber nicht final entschieden.
5. Backup- und Monitoring-Details müssen noch ausgearbeitet werden.
6. Ein vollwertiges Support-Ticketsystem ist im MVP nicht enthalten; Support erfolgt initial über ein einfaches Kontaktformular.
7. API Gateway Verfügbarkeit und damit Zusammenwirken mit anderen IT-Systemen müssen noch geklärt werden.
8. Die Möglichkeit von manuellen Sonderrabatten wird im MVP ausgeschlossen.
9. Internationale Nutzung (insbesondere USA) und Internationalisierung sind geplante Folgephasen.
10. Offline-Verfügbarkeit der SAP-Daten und Cache-Strategien sind noch nicht definiert.

## Traceability

Alle Anforderungen, Offenheiten und Rahmenbedingungen sind aus dem Stakeholder-Transkript input/transcripts/T9999_chaos.txt und der daraus strukturierten Zusammenfassung in runs/phase2_1/20260612_081029_1b1442/state/context.md ableitbar.
