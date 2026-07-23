## Functional Requirements

- Es wird ein Kundenportal als MVP entwickelt, das den schnellen Login mit E-Mail und Passwort inklusive Double-Opt-In ermöglicht.
- Das Portal erlaubt die Erstellung und Verwaltung von Angeboten mittels SAP-Integrationen (lesend).
- Kunden können Bestellungen und Rechnungen im Portal einsehen und Rechnungen als Download abrufen.
- Es werden mindestens drei Rollen implementiert: Admin, Sales und Kunde, mit klar definierten Berechtigungen.
- Es besteht eine begrenzte Auditierung, die nachvollziehbar macht, wer wann Änderungen an Angeboten oder Kundendaten vorgenommen hat.
- Der Support ist im MVP nur über ein Kontaktformular realisiert, ohne Ticketsystem oder Persistenz von Support-Anfragen.
- Das System bietet eine API-Layer zur Integration mit SAP und anderen Komponenten; OAuth wird bevorzugt, ist aber noch nicht final entschieden.
- Backup und Disaster Recovery sind im MVP vorgesehen.

## Non-functional Requirements

- Hosting erfolgt ausschließlich in der EU und muss DSGVO-konform sein, inklusive nachweisbarer Datenresidenz.
- Nutzung von Managed Services ohne neuen Datenbankserver.
- Die Lösung ist skalierbar von etwa 200 bis zu 20.000 Nutzern, wobei Overengineering vermieden wird.
- Performance und Verfügbarkeit der SAP-Schnittstellen sind kritisch für das System.
- Es gelten strikte Datenschutzmaßnahmen bezüglich Logging (keine personenbezogenen Daten in technischen Logs, getrennte Audit-Logs).
- Test- und Entwicklungsumgebungen müssen mit pseudonymisierten oder synthetischen Daten betrieben werden.
- Nutzung von Secrets Management und API-Rate Limiting.

## Constraints/Compliance

- Das System muss vollständig DSGVO-konform sein, inklusive Umgang mit Löschanfragen und Aufbewahrungspflichten.
- Angebote mit Rabatt über dem Standardsatz dürfen im MVP nicht erstellt werden (kein Sonderrabatt-Workflow im MVP).
- Das zentrale API Gateway ist Pflicht, steht aber erst nach einer sechs-wöchigen Warteliste zur Verfügung.
- Backup und Disaster Recovery müssen im Umfeld existierender IT-Richtlinien umgesetzt werden.
- Aufbewahrungsfristen für Geschäfts- und Angebotsdaten müssen rechtlich geprüft und eingehalten werden.
- Keine Einführung eines Ticketsystems im MVP, wodurch Supportanfragen per E-Mail bearbeitet werden und dies als bewusste Einschränkung mit Risiken akzeptiert wird.

## Assumptions and Open Points

- Die finale Entscheidung zum Pilotkunden (Schweiz oder Deutschland) steht noch aus. Daraus resultieren Fragen bezüglich Mehrwährung und Datenschutz.
- Die Integration von Single Sign-On (z.B. Azure AD, Google) ist optional und noch nicht entschieden.
- Rabattfreigabeprozesse und Workflow-Status für Angebote werden im MVP nicht umgesetzt.
- Keine Online-Akzeptanz von Angeboten im MVP.
- SAP-Datenverfügbarkeit kann Limitierungen bei der Angebotserstellung verursachen.
- Supportprozess im MVP ohne strukturierte Tickets, was potenzielle Risiken für Qualität und Datenschutz birgt.
- API-Sicherung mittels OAuth ist bevorzugt, aber noch offen.
- Monitoring muss datenminimiert und compliant gestaltet werden.
- Mehrsprachigkeit und Mehrwährung im MVP begrenzt, Erweiterungen für Phase 2 vorgesehen.
- Rollen- und Berechtigungskonzepte benötigen weitere Klärung und Detaillierung.
- Security Review ist zeitlich herausfordernd bei MVP-Zeitplan.

## Traceability

- Anforderungen resultieren aus der Analyse des Stakeholder-Meetings (Transkript input/transcripts/T9999_chaos.txt).
- Kontexte und Rahmenbedingungen wurden im Projektkontext artefakt (runs/phase2_1/20260601_155933_152d47/state/context.md) dokumentiert.
- Datenschutzanforderungen basieren auf Aussagen von Clara und weiteren Stakeholdern im Transkript.
- Technische Voraussetzungen (API Layer, OAuth, Backup) stammen aus den Diskussionen mit Ben und Farid.
- Geschäftsanforderungen (Angebot, Rechnungen, Rollen) wurden von Anna, Eva und David erklärt.
- Eingeschränkter Support und Risikohinweise kamen aus Gesprächen zwischen David, Clara und Ben.

