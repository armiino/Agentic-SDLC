## Functional Requirements

- Das System soll ein Kundenportal bereitstellen, das primär die Angebotserstellung beschleunigt.
- Kunden sollen Bestellungen und Rechnungen im Portal einsehen und Rechnungen herunterladen können.
- Ein Login mit E-Mail und Passwort ist obligatorisch; Single Sign-On (SSO) ist optional.
- Rollenbasierte Zugriffskontrolle mit mindestens den Rollen Admin, Sales, Kunde und Manager muss unterstützt werden.
- Integration einer API-Schicht zur Anbindung von SAP zur Produkt- und Preisdatenabfrage.
- Angebotserstellung darf nur Standardrabatte im MVP zulassen, Sonderrabatte sind erst in späteren Phasen vorgesehen.
- Ein PDF-Export von Angeboten mit Versionsmanagement und rechtlichen Fußnoten ist erforderlich.
- Backend-API muss OAuth für Authentifizierung unterstützen (optional im MVP, bevorzugte Methode).
- Backup- und Disaster-Recovery-Maßnahmen müssen für das System implementiert sein.

## Non-functional Requirements

- Das Kundenportal muss innerhalb von 8 Wochen als MVP einsatzbereit sein.
- Das System soll EU-konform gehostet werden und sämtliche Datenschutzanforderungen der DSGVO erfüllen.
- Das Backend muss TLS-verschlüsselte Verbindungen unterstützen; Ende-zu-Ende-Verschlüsselung ist nicht erforderlich.
- Logging und Audit Trails sind für alle sicherheitsrelevanten Aktionen notwendig, um Nachvollziehbarkeit sicherzustellen.
- Die Plattform soll skalierbar sein, um Nutzerzahlen von ca. 200 bis 20.000 zu unterstützen.
- Das System muss eine minimale Performance gewährleisten, sodass Angebotserstellungen in akzeptabler Zeit erfolgen.
- API Gateway Nutzung ist vorgesehen, jedoch gibt es eine 6-wöchige Warteliste; alternative Lösungen sind zu prüfen.

## Constraints/Compliance

- Keine neue Datenbank soll für das MVP installiert werden; Managed Services sind erlaubt, müssen jedoch kostengünstig sein.
- Doppelopt-in Verfahren für E-Mail-Login ist verpflichtend.
- Datenhosting muss in der EU oder mindestens DSGVO-konform erfolgen, exakte Datenresidenz ist zu definieren.
- Datenschutzrechtliche Löschkonzepte und datenschutzkonforme Aufbewahrungsfristen müssen umgesetzt werden.
- Backup und Disaster Recovery müssen den Compliance-Anforderungen entsprechen.
- Supportprozesse im MVP beschränken sich auf Kontaktformular ohne Ticketpersistenz; E-Mail-Bearbeitung ist unstrukturierte Ausnahme.
- Rabattfreigabeprozesse sind im MVP eingeschränkt; Sonderrabatte sind ohne Freigabe nicht erlaubt.
- Es besteht eine interne Policy, dass neue externe Portale über das zentrale API Gateway laufen müssen.

## Assumptions and Open Points

- Die Entscheidung, ob das Portal als native Mobile App oder responsive Web-App umgesetzt wird, ist noch offen.
- Integration und Nutzung eines SSO-Systems (Azure AD, Google) ist noch nicht abschließend entschieden.
- Support mit Ticketsystem ist im MVP nicht enthalten; E-Mail-basierte Anfragen sind umständlich.
- Pilotkunden und deren Einfluss auf Währungs- und Datenschutzanforderungen sind noch unklar.
- SAP-Anbindung ist im MVP nur lesend vorgesehen, Schreibzugriffe sind für spätere Phasen geplant.
- API Gateway Verfügbarkeit könnte die MVP-Zeitplanung beeinträchtigen.
- Backup- und Security-Review-Prozesse sind komplex und gefährden die 8-Wochen-Frist.
- Konkretisierung der KPI-Messungen und deren technische Umsetzung ist noch ausstehend.
- Umgang mit Testdaten und deren Datenschutz ist nicht abschließend geklärt.
- Internationalisierung (Sprachen, Währungen) ist für spätere Phasen vorgesehen.
- Preisaktualisierung und Cache-Mechanismen für SAP-Daten müssen noch evaluiert werden.

## Traceability

- Die funktionalen und nicht-funktionalen Anforderungen wurden aus dem Transkript input/transcripts/T9999_chaos.txt extrahiert und im Kontextdokument runs/phase2_1/20260612_133345_2d7b09/state/context.md zusammengefasst.
- Konflikte und offene Punkte wurden aus denselben Quellen identifiziert und explizit gekennzeichnet.
- Compliance-Richtlinien basieren auf den Aussagen von Datenschutzzuständigen (Clara) und Finanzverantwortlichen (Eva).
- Technische Rahmenbedingungen (API Layer, Hosting, Backup) stammen aus den Beiträgen von Ben und Farid.
