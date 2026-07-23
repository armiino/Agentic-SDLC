## Functional Requirements
- Das System muss ein Kundenportal bereitstellen, das als Webanwendung umsetzbar ist (Mobile Nutzung optional, evtl. später).
- Nutzer sollen sich per E-Mail und Passwort anmelden können, mit Double-Opt-In zur Einhaltung der DSGVO.
- Das Kundenportal muss ein Rollenmanagement unterstützen, mindestens mit den Rollen: Admin, Sales (Vertrieb) und Kunde.
- Möglichkeit der Angebotserstellung für Kunden, basierend auf lesenden Zugriffen auf SAP-Daten (Produktdaten, Preise, Rabattlogik ohne Sonderrabatte im MVP).
- Anzeige und Download von Bestellungen und Rechnungen für Kunden im Portal.
- Ein Audit-Trail muss implementiert werden, um Änderungen und Zugriffe minimal zu protokollieren.
- Kundendaten und alle gespeicherten Informationen müssen DSGVO-konform verarbeitet und vorgehalten werden.
- Das System muss ein Backup- und Disaster-Recovery-Konzept unterstützen.
- Das Kundenportal soll eine API-Schnittstelle mit OAuth-Unterstützung bereitstellen (OAuth bevorzugt, aber noch nicht final).
- Support wird im MVP über ein Kontaktformular ohne persistentes Ticketsystem abgebildet.
- PDF-Export von Angeboten mit rechtlichen Fußnoten und Versionsinformationen ist erforderlich.

## Non-functional Requirements
- Das Hosting der Kundendaten und des Systems muss EU-DSGVO-konform erfolgen, vorzugsweise mit Datenresidenz in der EU.
- Die Lösung soll Managed Services nutzen, ein eigener Datenbankserver ist im MVP ausgeschlossen.
- Die Anwendung muss innerhalb von 8 Wochen als MVP deploybar sein.
- Skalierbarkeit und Performance müssen für mindestens 200 bis 2000 Nutzer im MVP eingeplant sein, ohne Overengineering.
- Sicherheit: Logging und Auditprozesse müssen Datenschutzanforderungen berücksichtigen und kein personenbezogenes Monitoring-Daten enthalten.
- Es sollen mindestens Dev, Test und Produktionsumgebungen bereitgestellt werden, Testdaten sind pseudonymisiert oder synthetisch.
- API Rate Limiting und Missbrauchserkennung sollten umgesetzt werden, soweit möglich ohne Gateway (Warteliste für Gateway 6 Wochen).

## Constraints/Compliance
- Sämtliche Verarbeitung personenbezogener Daten unterliegt der DSGVO inklusive Double-Opt-In, Löschkonzept und Auftragsverarbeitungsverträgen.
- Rollen- und Berechtigungskonzepte müssen implementiert werden, wie Zugriffsbeschränkungen und Protokollierung von Aktionen.
- Backup und Disaster Recovery sind Pflicht, um Datenverlust zu vermeiden.
- Hosting muss entweder nachweislich EU-Only sein oder zumindest DSGVO-konform.
- Rabattfreigaben (Sonderrabatte) sind im MVP nicht erlaubt, da Freigabeprozesse erst in Folgephasen implementiert werden.
- Die Nutzung von OAuth zur Absicherung der API ist vorgesehen, aber noch nicht abschließend entschieden.
- Es besteht eine Verpflichtung zur Einhaltung handelsrechtlicher Aufbewahrungspflichten, diese kollidieren mit dem Recht auf Löschung und erfordern Datenklassifikation und Retentionsregeln.

## Assumptions and Open Points
- Es wird angenommen, dass die mobile Nutzung des Portals nicht im MVP enthalten ist.
- Die endgültige Entscheidung über Pilotkunden (DACH vs. Schweiz) sowie länderspezifische Datenschutz- und Währungsfragen ist noch offen.
- Die Entscheidung über SSO (z.B. Azure AD, Google) und Identity Management blieb unklar und steht noch aus.
- Supportprozess ohne Ticketpersistenz ist bewusst als Einschränkung für MVP notiert, gleiche gilt für Verarbeitung von Supportanfragen per E-Mail.
- Die Integration mit SAP bezüglich Schreibrechten und Echtzeitdaten ist unklar und muss noch geklärt werden.
- PDF-Export Feature für Angebote muss in der Versionierung und rechtlicher Dokumentation ausgebaut werden.
- Es fehlt eine klare Definition und Messbarkeit der KPIs im MVP.
- Security Review und Monitoring Prozesse werden als notwendig erkannt, aber ihre Umsetzung ist zeitlich kritisch.
- Die Implementierung eines API Gateways ist wegen Warteliste unklar im MVP-Release realisierbar.
- Es besteht ein Risiko durch potenzielle Verzögerungen und Einschränkungen aufgrund von SAP-Wartungsfenstern und API-Verfügbarkeiten.

## Traceability
- Alle Anforderungen wurden aus dem Transkript "T9999_chaos.txt" und dem zusammengefassten Projektkontext (runs/phase2_1/20260601_154720_f3cc89/state/context.md) abgeleitet.
- Funktionale Anforderungen basieren auf den Aussagen von Anna, Ben, Clara und weiteren Stakeholdern im Transkript.
- Nicht-funktionale Anforderungen wurden aus den technischen und operativen Diskussionen im Transkript und im Kontextdokument extrahiert.
- Compliance-Anforderungen sind aus den datenschutzrechtlichen und regulatorischen Hinweisen im Transkript (Clara) sowie den darauf Bezug nehmenden Entscheidungen abgeleitet.
- Annahmen und offene Punkte wurden dokumentiert, um die Sichtbarkeit widersprüchlicher oder noch unentschiedener Elemente sicherzustellen und Transparenz im weiteren Projektverlauf zu gewährleisten.