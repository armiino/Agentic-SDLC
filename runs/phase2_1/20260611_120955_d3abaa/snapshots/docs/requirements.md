## Functional Requirements

- Das System muss ein Kundenportal bereitstellen, primär für die schnelle Erstellung von Angeboten (MVP-Ziel).
- Das Kundenportal muss es erlauben, Rechnungen und Bestellungen einzusehen und Rechnungen herunterzuladen.
- Login-Funktionalität mit E-Mail und Passwort sowie Double-Opt-In für Datenschutzkonformität.
- Rollen- und Berechtigungskonzept mit mindestens Rollen für Admin, Sales, Manager und Support.
- Angebotserstellung inklusive Nutzung von Stammdaten, Preisen und Rabattlogik aus SAP.
- Rabatt-Freigabeprozess außerhalb des MVP (Phase 2), im MVP keine manuellen Sonderrabatte über Standard hinaus.
- Audit Trail für Änderungen an Angeboten und Benutzeraktionen, mindestens minimal loggen wer was wann geändert hat.
- API-Layer zur Integration mit SAP und anderen Systemen, mit gesicherter Authentifizierung (OAuth bevorzugt, final offen).
- Anzeige und Download von Rechnungen im Kundenportal.
- Kontaktformular für Supportanfragen im MVP, jedoch ohne persistentes Ticketsystem.

## Non-functional Requirements

- EU-only Hosting des Systems und der Daten, inklusive Nachweis der Datenresidenz.
- Backup und Disaster Recovery müssen implementiert werden, deren Umfang ist MVP-spezifisch zu definieren.
- Das Portal muss für Web-Nutzung (responsive) ausgelegt sein; native mobile App ist für später geplant.
- System muss datenschutzkonform nach DSGVO sein, inkl. Löschkonzept und Auditierbarkeit.
- Sicherheit durch Verschlüsselung per TLS für Datenübertragung.
- System muss skalierbar sein, mindestens für 200 bis 2000 Nutzer, mit Option zur Erweiterung.
- System muss eine angemessene Performance bieten, insbesondere bei Angebotserstellung und Rechnungsanzeige.
- Dokumentation und Security Review sind verpflichtend, auch wenn zeitlich herausfordernd.

## Constraints/Compliance

- Keine neuen Datenbank-Server, Nutzung von Managed Services bevorzugt.
- Nutzung eines zentralen API-Gateways ist vorgesehen, allerdings mit möglicher Verzögerung (Warteliste).
- Keine Nutzung konkreter Cloud-Produkte oder Provider ohne finale Entscheidungsgrundlage.
- Einhaltung der DSGVO, insbesondere bei Double-Opt-In, Datenlöschung und Audit Logs.
- Handelsrechtliche Aufbewahrungspflichten müssen berücksichtigt werden, ggf. konfligierend mit Löschanfragen.
- Supportprozesse im MVP sind auf ein Kontaktformular beschränkt, ohne formelles Ticketsystem.
- Backup und Disaster Recovery müssen den gesetzlichen und internen Anforderungen genügen.
- Preis- und Rabattdaten müssen mit SAP synchronisiert werden, Echtzeitverfügbarkeit kann limitiert sein.

## Assumptions and Open Points

- Pilotkunde und Zielregion noch nicht endgültig festgelegt, beeinflusst Datenschutzanforderungen und Mehrwährung.
- Freigabeprozesse für Rabatte werden im MVP nicht umgesetzt, sondern in späteren Phasen.
- Supportprozess ohne persistentes Ticketsystem ist eine bewusste Einschränkung mit Risiken für den Support.
- API-Authentifizierungsmethode (OAuth versus API Keys) ist noch nicht finalisiert.
- Mobile App ist geplant, wird aber nicht im MVP berücksichtigt.
- Backup- und Security Review-Prozesse müssen noch genau definiert werden.
- Umgang mit SAP-Verfügbarkeit (Echtzeitdaten vs Batch-Updates) und deren Einfluss auf Angebotserstellung offen.
- Dokumentationstiefe und Auditierungsumfang müssen exakt festgelegt werden.
- Unklare Verantwortlichkeiten und fehlende Architekturentscheidungen können die Umsetzung verzögern.
- KPI-Messung und Analytics sind für spätere Phasen vorgesehen.

## Traceability

- Alle funktionalen und nicht-funktionalen Anforderungen basieren auf den Aussagen und Diskussionen im Transkript input/transcripts/T9999_chaos.txt.
- Konflikte, Annahmen und offene Punkte wurden explizit aus den divergierenden Sichtweisen der Beteiligten (Anna, Ben, Clara, David, Eva, Farid) extrahiert.
- Compliance- und Datenschutzanforderungen orientieren sich an den regulatorischen Hinweisen von Clara und dem IT-Operations-Input von Farid.

