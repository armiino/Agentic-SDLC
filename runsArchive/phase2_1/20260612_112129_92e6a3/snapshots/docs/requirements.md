# Kundenportal Requirements

## Functional Requirements
- Das System muss ein Kundenportal bereitstellen, in dem Kunden ihre Angebote erstellen und Rechnungen einsehen können.
- Die Angebotserstellung muss eine Integration zu SAP für Produkt- und Preisdaten besitzen (lesend).
- Kunden müssen sich per E-Mail und Passwort anmelden können; ein Double-Opt-In für DSGVO-Konformität ist verpflichtend.
- Rollen müssen mindestens Admin, Sales und Kunde abdecken. Support-Rollen sind im MVP ausgeschlossen.
- Kunden sollen ihre Bestellungen und Rechnungen herunterladen können, inklusive PDF-Export mit rechtlicher Nachvollziehbarkeit.
- Das System soll minimale Audit- und Logging-Funktionalitäten für Nutzeraktionen und Angebotsänderungen haben.
- Es soll eine einfache Rabattlogik ohne manuelle Sonderrabatte im MVP unterstützen (keine Freigabeprozesse im MVP).
- Ein API-Layer muss implementiert werden, der die Integration mit externen Systemen ermöglicht; OAuth ist bevorzugt, API Keys sind möglich.
- Das Kundenportal muss ein Kontaktformular für Supportanfragen bieten, jedoch kein vollständiges Ticketsystem im MVP.

## Non-functional Requirements
- Das System muss innerhalb von 8 Wochen als MVP bereitgestellt werden.
- Hosting muss in der EU erfolgen, um DSGVO-Vorschriften einzuhalten, inklusive nachweisbarer Datenresidenz.
- Es dürfen keine neuen Datenbankserver betrieben werden; Managed Services sind zu bevorzugen.
- Die Performance muss so beschaffen sein, dass zwischen 200 und (unsicher) 20.000 Nutzer ohne Overengineering bedient werden können.
- Backup- und Disaster-Recovery-Konzepte müssen umgesetzt werden.
- API und Portal müssen sicher betrieben werden mit Audit-Log-Funktionalität, Logging ohne personenbezogene Daten, und Verschlüsselung auf Transportebene (TLS).
- Es sind mindestens drei Umgebungen (Dev, Test, Prod) mit pseudonymisierten oder synthetischen Testdaten zu etablieren.

## Constraints/Compliance
- Das System muss vollständig DSGVO-konform sein, insbesondere im Umgang mit personenbezogenen Daten, Löschkonzepten und Double-Opt-In.
- Audit Trails müssen revisionssicher sein und Änderungen an Angeboten protokollieren.
- Kein manueller Upload von Angebotsdaten im MVP, um Datenqualität und Compliance sicherzustellen.
- Speicherung und Verarbeitung von Daten darf nur innerhalb der EU erfolgen, inklusive Backup und Hosting.
- Keine Sonderrabatte ohne vorherige Freigabe außerhalb des MVP.
- Ein Security Review ist erforderlich, darf jedoch den MVP nicht verzögern; pragmatische Umsetzungen sind erlaubt.
- Supportanfragen über das Kontaktformular sind datenschutzkonform zu behandeln, da kein Ticketsystem besteht.
- Retention Policies sind zu beachten, die Löschung personenbezogener Daten müssen mit handelsrechtlichen Aufbewahrungspflichten abgewogen werden.

## Assumptions and Open Points
- SSO-Integration (Azure AD, Google) ist optional und noch nicht final definiert.
- Freigabeprozesse für Rabatte sind nicht im MVP enthalten und in späteren Phasen zu klären.
- Support-Ticketsystem wird im MVP nicht realisiert; Kontaktformular ist temporäre Lösung.
- Skalierung und erwartete Nutzerzahlen sind sehr unsicher (200 bis 20.000 Nutzer).
- Pilotkunde, Mehrwährungen und Mehrsprachigkeit sind noch offen und beeinflussen Scope und Anforderungen.
- API Gateway Nutzung ist geplant, aber Wartezeit von 6 Wochen könnte den MVP gefährden.
- SAP-Schnittstellen sind kritisch und können Wartungsfenster haben, was Verfügbarkeit beeinflussen kann.
- Datenschutzrisiken im Umgang mit Testdaten aus SAP-Testumgebungen sind noch unklar und zu klären.
- Datenschutzrechtliche Abwägungen bezüglich Protokollierung, Tracking und Push Notifications sind für MVP zurückgestellt.
- Preisaktualität und Rabattlogik sind eingeschränkt; es gibt keinen Echtzeit-Zugriff auf SAP-Sonderpreise im MVP.

## Traceability
- Anforderungen basieren auf dem Transkript input/transcripts/T9999_chaos.txt, welches umfangreiche Diskussionen zwischen Stakeholdern Anna, Ben, Clara, David, Eva und Farid dokumentiert.
- Zielsetzung, Rollen, Hauptthemen, Konflikte und Risiken sind im Kontextruns/phase2_1/20260612_112129_92e6a3/state/context.md zusammengefasst.
- Beim Umgang mit widersprüchlichen Anforderungen wurden bewusste Abgrenzungen für das MVP getroffen, um den Zeitrahmen und Machbarkeit zu sichern.
- Compliance-Anforderungen und Datenschutz stammen insbesondere aus den Beiträgen von Clara und sind im Kontext dokumentiert.
- Technische und operationelle Randbedingungen insbesondere von Ben und Farid sind im Kontext berücksichtigt.

---

_Dieses Dokument wurde erstellt basierend auf MAF Workflow Kontext und dem Transkript input/transcripts/T9999_chaos.txt. Anforderungen sind als testbar formuliert, offene Punkte und Annahmen explizit gekennzeichnet._