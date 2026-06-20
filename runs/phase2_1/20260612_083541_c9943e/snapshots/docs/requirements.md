## Functional Requirements
- Das System muss ein Kundenportal bereitstellen, das primär webbasiert (Web-first) ist.
- Nutzer sollen sich per E-Mail und Passwort anmelden können, mit Double-Opt-In zur Einhaltung der DSGVO.
- Optional soll ein Single Sign-On (SSO) unterstützt werden, z. B. über Azure AD oder Google (noch offen).
- Das Portal muss die Angebotserstellung ermöglichen, inklusive der Anzeige von Produktdaten, Preisen und Rabattlogik aus SAP.
- Angebote müssen einen Freigabeprozess unterstützen, bei dem Rabatte über 15 Prozent mindestens eine Managerfreigabe erfordern; Rabatte über 30 Prozent möglicherweise zusätzlich Freigabe von Finance (Details offen).
- Kunden sollen Rechnungen einsehen und herunterladen können.
- Es muss ein Rollen- und Berechtigungskonzept geben mit mindestens den Rollen Admin, Sales, Manager, Support und Kunde.
- Ein Audit-Trail muss protokollieren, wer welche Daten wann geändert oder eingesehen hat.
- Das System muss eine API-Schicht bereitstellen, die OAuth unterstützt (finale Entscheidung offen).
- Backup- und Disaster-Recovery-Verfahren sind für das MVP erforderlich.
- Ein Kontaktformular für Supportanfragen soll verfügbar sein, ohne Persistenz von Tickets im MVP.
- KPI-Messungen sind erforderlich, insbesondere Conversion Rate und Zeit von Angebot bis Bestellung.

## Non-functional Requirements
- Das System muss DSGVO-konform betrieben werden, insbesondere bezüglich Datenminimierung, Löschkonzepten, Double-Opt-In und Datenresidenz.
- Hosting muss in der EU erfolgen, mit nachweisbarer Datenresidenz.
- Das System soll Managed Services nutzen; ein eigener neuer Datenbankserver ist ausgeschlossen.
- Ein Security Review ist Pflicht, jedoch ist die Dauer kritisch im Zeitplan von 8 Wochen MVP.
- Das System soll skalierbar sein für eine unklare Nutzerzahl (geschätzter Bereich 200 bis 20.000 Nutzer).
- System- und Audit-Logs müssen getrennt geführt werden, um personenbezogene Daten im technischen Logging zu vermeiden.
- Test-, Entwicklungs- und Produktionsumgebungen sind bereitzustellen, wobei Testdaten keine echten Kundendaten enthalten dürfen.
- Die Performance muss Nutzern erlauben, z.B. mehrere Rechnungen ohne schlechte UX herunterzuladen, inklusive Paginierung und Rate-Limiting.

## Constraints/Compliance
- Keine Doppel-Frontend-Entwicklung für Native Mobile App im MVP aufgrund Budgetrestriktionen.
- API-Gateway-Warteliste von 6 Wochen stellt eine technische Einschränkung dar.
- Im MVP sind keine manuellen Sonderrabatte erlaubt, die Freigabeprozesse umgehen.
- Support-Tickets werden im MVP nicht verwaltet, stattdessen E-Mail-Kommunikation über Kontaktformular (bewusste Einschränkung mit Risiko).
- Handelsrechtliche Aufbewahrungsfristen und DSGVO- Löschrechte sind konfliktär und müssen für Datenklassifikation und Retention berücksichtigt werden.
- Es darf keine neue Datenbank eingeführt werden; Managed Services sind vorzuziehen.
- Datenschutz- und Sicherheitsanforderungen (Double-Opt-In, Audit, Löschkonzept) sind zwingend einzuhalten.

## Assumptions and Open Points
- Ob und wann eine mobile Version (native App oder responsive Web) umgesetzt wird, ist offen.
- Die genaue Ausgestaltung des SSO ist unklar und noch zu entscheiden.
- Die Pilotkundenklärung (Schweiz vs. DACH) ist offen und hat Einfluss auf Datenschutz und Währungsanforderungen.
- Die Freigabeprozesse für Rabatte mit konkreten Schwellen und Verantwortlichkeiten müssen noch final definiert werden.
- Supportprozess und Umgang mit Supportanfragen sind im MVP unklar und unterliegen Risiken.
- Kosten für EU-only Hosting und Managed Services sind unklar und müssen geschätzt werden.
- Backup- und Disaster-Recovery-Details bedürfen weiterer Ausarbeitung.
- Die Aktualität der SAP-Preisdaten und der Umgang mit Offline-Szenarien sind kritisch und müssen noch geklärt werden.
- API-Technologie und OAuth-Implementierung sind noch nicht final entschieden.
- KPI-Messungen und Analytics sind im MVP begrenzt geplant.

## Traceability
- Die funktionalen und nicht-funktionalen Anforderungen basieren auf den Diskussionen im Stakeholder-Transkript (input/transcripts/T9999_chaos.txt).
- Annahmen und offene Punkte sind bewusst bewusst getrennt, um Risiken und Unklarheiten sichtbar zu halten.
- Constraints reflektieren explizite technische und organisatorische Restriktionen aus dem Transkript.
- Die Rollen- und Berechtigungskonzepte sowie der Freigabeprozess basieren auf Äußerungen von Anna, Ben, Clara, Eva und David.
- Datenschutz- und Compliance-Anforderungen wurden insbesondere von Clara, aber auch von Eva und Farid hervorgehoben.
- Technische Herausforderungen und Architekturrestriktionen stammen vor allem von Ben und Farid.
- Support-Einschränkungen wurden von David und Clara eingebracht.
- MVP-Zeitplan und Scope-Abgrenzung wurden von Anna und dem Team diskutiert.

---

*Dieses Dokument wurde unter Berücksichtigung des MAF Workflow-Kontexts und der Stakeholder-Kommunikation aus input/transcripts/T9999_chaos.txt erstellt.*