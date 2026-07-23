# Projektkontext Kundenportal MVP

## Projektziel
- Entwicklung eines Kundenportals als Web-Anwendung (Web first, mobile eventuell später) mit Fokus auf schneller Angebotserstellung und Rechnungsanzeige.
- Portal dient primär der Angebotsgenerierung, Rechnungsübersicht und Login-Funktionalität.
- Integration mit bestehendem SAP-System für Stammdaten, Preise und Rabattlogik.
- MVP-Zeitplan: 8 Wochen

## Stakeholder und Rollen
- Anna (Sales, Projektmanagement)
- Ben (Technik, Architektur, Backend)
- Clara (Datenschutz, Compliance)
- David (Customer Support)
- Eva (Finance)
- Farid (IT Operations)

## Wichtige fachliche Themen
- Login mit E-Mail/Passwort, optional SSO (Azure AD, Google) geplant
- DSGVO-Compliance: Double-Opt-In, Löschkonzept, Auditierbarkeit, Rollen- und Berechtigungskonzepte
- Angebote erstellen mit SAP-Daten, inklusive Rabatt- und Freigabelogik
- Rechnungsanzeige und Download (PDF mit rechtlichen Fußnoten und Versionsverwaltung)
- Rollen: mindestens Admin, Sales, Kunde; Support minimal in MVP (Kontaktformular ohne Ticketsystem)
- API Layer als integrative Schnittstelle, OAuth bevorzugt
- Managed Services und kein eigener Datenbankserver
- EU-Hosting mit DSGVO-konformer Datenresidenz
- Backup und Disaster Recovery als Teil des MVP
- KPIs (z.B. Conversion Rate, Zeit bis Angebot) in späteren Phasen

## Konflikte und Unsicherheiten
- Mobile App vs. Web: Budget und technische Machbarkeit
- Komplexität und Zeitrahmen des Security Reviews vs. MVP-Termin
- Umfang der Rollen- und Berechtigungsmodelle vs. einfache Bedienbarkeit
- Supportprozess (kein Ticketsystem im MVP, manuelle E-Mail Verarbeitung als Risiko)
- Echtzeitdaten von SAP sind limitiert, Preis- und Rabattgültigkeit unsicher
- API Gateway mit 6 Wochen Wartezeit vs. 8 Wochen MVP-Zeitplan
- Datenschutzkonflikte: Audit Logs vs. DSGVO, Backup und Retention vs. Löschrecht
- EU-only Hosting vs. Kostendruck
- Unklare Pilotkunden und deren Einfluss auf Scope (DACH, Schweiz, USA)
- Mehrwährungs- und Länderregeln noch offen, evtl. nicht im MVP
- Supportsprache Deutsch und Englisch
- Keine Sonderrabatte im MVP ohne Freigabe
- Gefahr von Overengineering vs. Skalierbarkeit
- Fehlender Architekt und unklare Dokumentationspflichten

## Offene Fragen und Entscheidungen
- Endgültiger Pilotkunde (Müller AG Schweiz oder Hansa GmbH Deutschland)
- Finaler Scope der Rabatte und Freigabeprozess
- Nutzung von API Gateway und OAuth
- Backup- und Disaster Recovery Details
- Supportprozess und Ticketsystem für nach MVP
- Detaillierte Kostenschätzung für Vorstand (Deadline Freitag)
- Umgang mit SAP-Test- und Produktionsdaten im Entwicklungsprozess
- Umgang mit Logs und Monitoring in Bezug auf Datenschutz
- Entscheidung über Online-Akzeptanz von Angeboten

## Quellenhinweise
- Ausführliches Stakeholder-Transkript "T9999_chaos.txt" mit Diskussionsverlauf zu Scope, Risiken, Anforderungen und technischen Rahmenbedingungen

---

*Hinweis: Der Kontext bildet aktuelle Widersprüche, Einschränkungen und Risiken transparent ab, ohne diese zu glätten. Dies dient der realistischen Risikobewertung und klaren Abgrenzung des MVPs.*