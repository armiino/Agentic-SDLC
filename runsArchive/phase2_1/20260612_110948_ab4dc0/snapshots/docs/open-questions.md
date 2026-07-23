# Offene Fragen und Klärungsbedarfe

## 1. Fachliche Fragen und Unsicherheiten

- Welche konkreten Rollendefinitionen und Berechtigungen sollen im MVP umgesetzt werden? (z.B. Support-Zugriff auf Angebote und Rabatte nicht klar definiert)
- Soll im MVP ein Rabatt-Freigabeprozess eingeführt werden oder erst in Folgephasen? Wie genau sieht der Freigabeprozess aus (Grenzwerte, Rollen)?
- Wer ist der Pilotkunde und in welchen Ländern operiert das Portal initial? (DACH, EU, USA?)
- Soll Mehrwährungsunterstützung (EUR, CHF, USD) im MVP oder erst später eingeführt werden?
- Wie soll der Supportprozess gestaltet werden, wenn kein Ticketsystem zum MVP implementiert wird? Welche Risiken und Maßnahmen sind mit einem einfachen Kontaktformular verbunden?
- Wie wird mit Kundenanfragen bezüglich Löschung und Auskunft umgegangen, insbesondere ohne formale Ticketlösung?
- Wie soll mit unvollständigen oder sich spät ändernden Produkt- und Preisdaten aus SAP umgegangen werden? 

## 2. Technische Fragen und Risiken

- Wie wird die SAP-Integration genau implementiert? (lesender Zugriff, schreibender Zugriff, Fallback, Cache)
- Ist und wie wird ein zentrales Identity- und Access-Management (IAM) im MVP realisiert? Welche SSO-Lösungen (OAuth, Azure AD, Google) werden unterstützt?
- Wann und wie wird das API Gateway verfügbar sein? Gibt es alternative Absicherungen bis dahin?
- Wie wird die EU-only Datenresidenz technisch sichergestellt und dokumentiert?
- Welche konkreten Backup- und Disaster Recovery Prozesse werden implementiert?
- Wie wird die Auditierung und das Logging technisch umgesetzt, um Security Review Anforderungen zu erfüllen?
- Gibt es Vorgaben zum Umgang mit sensiblen Daten in technischen Logs und Monitoring-Systemen?
- Werden Dev, Test und Produktionsumgebungen eingerichtet? Wie wird mit echten Kundendaten im Test umgegangen?
- Wie wird das Secrets Management für API-Schlüssel, Credentials etc. organisiert?
- Gibt es definierte Limits (Rate Limiting) und Missbrauchserkennung für APIs?

## 3. Organisatorische und Prozessfragen

- Wer übernimmt die Architekturverantwortung und die Pflege der Dokumentation, um Widersprüche und offene Punkte transparent zu halten?
- Wer ist Ansprechpartner für Datenschutz und Compliance während der Entwicklung?
- Wer verantwortet die Kostenabschätzung für EU-Managed Hosting und weitere Infrastruktur vor Vorstandsterminen?
- Wie wird der notwendige Security Review in den engen MVP-Zeitplan eingeplant oder organisiert?
- Wie sollen Schnittstellenverantwortlichkeiten (z.B. SAP, Identity Provider, Managed Services) geklärt und gesteuert werden?
- Wie werden Scope-Änderungen dokumentiert und kommuniziert, insbesondere im Hinblick auf Pilotkunden und geplante Funktionserweiterungen?

## 4. Widersprüche und offene Konflikte

- Zeitplan (8 Wochen MVP) vs. umfassende Compliance, Security und Support-Anforderungen
- Wunsch nach schneller MVP-Umsetzung vs. notwendige technische Reife (API Gateway, IAM, SAP Anbindung)
- DSGVO-konformität vs. pragmatische Reduktion von Funktionen zur Einhaltung der Fristen
- Rollen und Berechtigungen vs. Support-Zugriff und Datenschutz
- Hosting in EU-only vs. Kosten und Verfügbarkeit von Services
- Backup und Logging notwendige Sicherheitsmaßnahmen vs. verfügbare Ressourcen und Zeit

## 5. Mögliche Ansprechpartner / Rollen

- Projektleitung: Anna
- Technik / Entwicklung: Ben
- Datenschutz / Compliance: Clara
- Support: David
- Finance / Freigabeprozesse: Eva
- IT Operations / Hosting: Farid

---

_Dieses Dokument fasst systematisch offene Fragen und Klärungsbedarfe zusammen, um strukturierte Anforderungen und Risiken ableiten zu können. Die offenen Punkte basieren vorrangig auf dem Transkript T9999_chaos.txt und dem initialen Architekturüberblick.