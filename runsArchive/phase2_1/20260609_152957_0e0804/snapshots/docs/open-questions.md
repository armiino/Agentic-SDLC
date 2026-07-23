# Offene Fragen und Klärungsbedarfe

## 1. Fachliche Fragen

- Wie genau ist die finale Zieldefinition für das MVP? Welche Features müssen zwingend enthalten sein, welche können verschoben werden?
- Wie soll der Supportprozess konkret umgesetzt werden? Ist ein Kontaktformular mit manueller Bearbeitung ausreichend oder wird ein Ticketsystem benötigt?
- Wie wird der Rabattfreigabeprozess ausgestaltet? Welche Rollen sind beteiligt und wie wird die Freigabe technisch unterstützt?
- Wer übernimmt die Pflege der Stammdaten und Angebotsinhalte?
- Gibt es eine klare Entscheidung zu Pilotkunde(n) und damit verbundener regionaler Ausdehnung (DACH, Schweiz, EU, USA)?

## 2. Technische Fragen

- Wird das zentrale API Gateway für den MVP genutzt, trotz aktueller Warteliste von 6 Wochen?
- Falls nein, welche temporären Alternativen sind vorgesehen?
- Welche Authentifizierungsmethode wird final gewählt? OAuth oder lediglich E-Mail/Passwort-Login?
- Wie wird der Fallback bei SAP-System-Ausfällen realisiert?
- Welche Managed Services sind für Datenhaltung und Backup konkret vorgesehen?
- Wie werden Test- und Entwicklungsumgebungen mit Blick auf Datenschutz gestaltet? Welche Daten dürfen dort verwendet werden?
- Welche Mechanismen für Secrets Management werden eingesetzt?
- Wie werden Rate Limits, Paging und Missbrauchserkennung technisch umgesetzt?

## 3. Datenschutz und Compliance

- Wie wird die EU-only-Datenresidenz technisch sichergestellt und nachgewiesen?
- Welche Datenklassifikation und Retentionsrichtlinien gelten konkret?
- Wie wird das Löschkonzept mit gesetzlichen Aufbewahrungspflichten in Einklang gebracht?
- Welche Logging- und Audit-Standards werden implementiert?
- Welche konkreten Maßnahmen sind für Double-Opt-In, Löschung und Nachvollziehbarkeit erforderlich?

## 4. Organisatorische Fragen

- Wer verantwortet Architektur und Security im Projekt, wenn derzeit keine dedizierten Architekten vorhanden sind?
- Wie wird die Dokumentation organisiert, um Security Review und Compliance sicherzustellen?
- Wer ist Ansprechpartner für Datenschutzfragen?
- Gibt es definierte Prozesse für die Abstimmung mit SAP-, API Gateway- und Hosting-Teams?
- Wie wird die Kostenschätzung für EU-Hosting und Managed Services genau erstellt?

## 5. Risiken und Widersprüche

- Wie sollen die Zielkonflikte zwischen Zeitrahmen (8 Wochen MVP) und notwendigen Sicherheitsprüfungen gehandhabt werden?
- Wie werden Widersprüche zwischen Wunsch nach Funktionalität (z.B. Rabattfreigabe, Support) und MVP-Scope gelöst?
- Wie wird mit Unsicherheiten bei SAP-Verfügbarkeit und SAP-Datenintegrität umgegangen?
- Wie wird internationaler Datenschutz (z.B. Schweiz, USA) im Projektverlauf adressiert?

---

Diese Liste basiert auf der Auswertung des Stakeholder-Transkripts, der Anforderungen, Risikoanalyse und Architekturübersicht. Sie soll als Grundlage für gezielte Klärungen und Projektsteuerung dienen.
