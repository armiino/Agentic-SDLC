# Offene Fragen und Klärungsbedarfe für das Kundenportal MVP

## Fachliche Fragen

1. Pilotkunde / Zielmarkt
   - Wer ist der finale Pilotkunde: Müller AG (Schweiz) oder Hansa GmbH (Deutschland)?
   - Beeinflusst die Wahl des Pilotkunden Umfang und Funktionen (Mehrwährung, Datenschutz etc.)?

2. Rabattfreigabe und Workflow
   - Wie genau sollen Rabattfreigaben gestaltet werden (ab welchen Schwellenwerten, wer genehmigt)?
   - Soll der Freigabeprozess zeitnah in Phase 2 umgesetzt oder später geplant werden?

3. Support-Prozess
   - Wie soll der Support im MVP genau organisiert werden, wenn kein Ticketsystem vorgesehen ist?
   - Welche Risiken und organisatorischen Maßnahmen sind für den manuellen E-Mail-basierten Support vorgesehen?

4. Mobile Nutzung
   - Soll mobile Nutzung nativ (App) oder als responsive Web-App priorisiert werden?
   - Wie wichtig ist Mobile im MVP und wann ist eine Erweiterung geplant?

5. KPIs und Analytics
   - Welche KPIs sind für das MVP wirklich zwingend zu messen?
   - Ist Analytics im MVP vorgesehen oder erst in späteren Phasen?

6. Online-Akzeptanz von Angeboten
   - Soll eine Online-Akzeptanz von Angeboten schon im MVP möglich sein oder später?

## Technische Fragen

1. API-Sicherheit
   - Wird OAuth endgültig als API-Sicherheitsmechanismus für das MVP genommen?
   - Falls nicht, welche Alternativen sind akzeptabel?

2. API Gateway
   - Wie umgehen wir die 6-wöchige Warteliste für das zentrale API Gateway im MVP?
   - Gibt es genehmigte temporäre oder manuelle Umgehungslösungen?

3. SAP-Verfügbarkeit
   - Wie soll mit temporären SAP-Ausfällen umgegangen werden?
   - Sind Caches vorgesehen, und wenn ja, wie adressieren wir Datenschutz und Datenaktualität?

4. Backup und Disaster Recovery
   - Welcher Umfang und welche SLAs gelten für Backup und Disaster Recovery im MVP?

5. Test- und Entwicklungsdaten
   - Wie wird der Zugriff auf SAP-Testdaten kontrolliert und pseudonymisiert?
   - Gibt es bereits eine Testdatenstrategie?

6. Secrets Management
   - Wie soll Secrets Management technisch umgesetzt werden?

7. Skalierbarkeit
   - Welche Nutzerzahlen sind realistisch im MVP und wie skalierbar muss die Infrastruktur sein?

## Organisatorische Fragen

1. Architekturressourcen
   - Wie ist die Versorgung mit Architekturunterstützung gesichert, wenn aktuell kein Architekt vorhanden ist?

2. Zeitplan
   - Wie sind Security Review und API-Gateway-Verfügbarkeit in den 8-Wochen-MVP-Zeitplan einzubinden?

3. Dokumentation
   - Welches Mindestmaß an Dokumentation ist für den MVP erforderlich, um z.B. Security Reviews zu ermöglichen?

4. Datenschutz und Compliance
   - Wer ist der Ansprechpartner für Datenschutzprüfungen und Freigaben?
   - Wie wird das Lösch- und Aufbewahrungskonzept finalisiert und durchgesetzt?

5. Entscheidung Mehrwährung und Internationalisierung
   - Wann wird die Entscheidung getroffen, ob und welche Länder/Währungen initial unterstützt werden?

## Widersprüche und Klärungsbedarfe (sichtbar erhalten)

- Wunsch nach schneller Implementierung und MVP in 8 Wochen steht im Konflikt mit komplexen DSGVO-Anforderungen, Security Reviews und API-Gateway-Verfügbarkeit.
- Erwartung an Skalierbarkeit und parallele Feature-Komplexität gegen Vermeidung von Overengineering.
- Support ohne Ticketsystem gegen die Notwendigkeit von Nachverfolgbarkeit und Datenschutz.
- Nutzung von Managed Services mit EU-Only Hosting-Anforderung gegen Kosten- und Leistungsdruck.

## Mögliche Ansprechpartner oder Rollen

- Projektleitung / Product Owner (Anna) für fachliche und Business-Fragen.
- Technik- und Architekturriege (Ben) für technische Fragen und Architekturentscheidungen.
- Datenschutzbeauftragte / Compliance (Clara) für DSGVO und Datenschutzthemen.
- IT Operations (Farid) für Infrastruktur, Hosting, Backup und Monitoring.
- Finance (Eva) für Fragen rund um Rabattfreigabe und Finanzprozesse.
- Support (David) für Supportprozesse und Kundenbetreuung.

---

_Erstellungsdatum: 2026-06-01_
