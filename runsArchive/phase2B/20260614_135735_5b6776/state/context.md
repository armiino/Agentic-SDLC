# Projektkontext aus Stakeholder-Transkript T9999_chaos.txt

## Projektziel
Das Hauptziel des Projekts ist die Entwicklung eines Kundenportals (bzw. Kundenplattform), mit dem Kunden schneller Angebote erhalten, ihre Rechnungen einsehen und herunterladen sowie Bestellungen verwalten können. Das MVP soll in ca. 8 Wochen bereitgestellt werden. Mobile Nutzung, erweiterte Support- und Freigabeprozesse sowie komplexe Rabattlogiken sind eher nach dem MVP geplant.

## Sprecherrollen
- Anna (offensichtlich Projekt- oder Produktverantwortliche mit Fokus auf MVP und Vertrieb)
- Ben (technischer Experte, Entwickler oder Architekt, kritisch bei Zeit und Ressourcen)
- Clara (Datenschutz- und Compliance-Beauftragte)
- David (Customer Support)
- Eva (Finanzbereich, Fokus auf Rabatte und Freigabeprozesse)
- Farid (IT Operations, Infrastruktur und Hosting)

## Fachliche Hauptthemen
- Kundenportal mit Login, Rollen (Admin, Sales, Kunde, Manager, Support optional)
- Angebotserstellung mit Anbindung an SAP (Produktdaten, Preise, Rabattlogik)
- Rechnungsdownload und Bestellübersicht
- Sicherheits- und Datenschutzmaßnahmen (DSGVO, Double Opt-In, Löschkonzepte, Audit Trails, Logging)
- Managed Services für Datenhosting (EU-only, kosteneffizient, Backup und Disaster Recovery)
- API Layer und Integration (OAuth bevorzugt, API Gateway Engpass)
- MVP-Scope vs. umfassende Anforderungen (Zeitkonflikte, Priorisierung notwendig)
- Reporting und KPIs (Conversion Rate, Angebotszeit)
- Komplexe Berechtigungs- und Freigabelogiken (Rabattfreigaben ab bestimmten Schwellen)
- Risiken & Konflikte: Supportprozesse nicht vollständig definiert, SAP-Verfügbarkeit kritisch, keine neue DB laut IT, unklare Pilotkunde mit unterschiedlichen Datenschutzanforderungen

## Konflikte und Unsicherheiten
- Zeitliche Herausforderung: 8 Wochen MVP vs. notwendige Security Reviews und komplexe Anforderungen
- Rollen- und Berechtigungskonflikte insbesondere für Support und Rabattfreigaben
- Technische Einschränkungen: nicht API-readyes Backend, API Gateway mit langer Warteliste
- Datenschutz und Compliance: DSGVO-konforme Datenhaltung mit Nachweisbarkeit, Audits und Löschkonzepten vs. notwendige schnelle Umsetzung
- Hosting: EU-only vs. Kosten und technische Einschränkungen
- Supportprozesse ohne Ticketsystem im MVP geplant, Risiko für Kundenzufriedenheit
- Pilotkunde unklar (DACH, Schweiz mit besonderem Datenschutz, USA möglich in Zukunft)
- Rabattprozess & Freigabe in MVP eingeschränkt (keine Sonderrabatte ohne Freigabe)
- SAP-Preisdatenaktualität vs. Bedarf an Echtzeit-Angeboten

## Quellenhinweise
- Vollständiges Stakeholder-Transkript in input/transcripts/T9999_chaos.txt

---

*Diese Zusammenfassung wurde automatisiert aus dem Stakeholder-Transkript extrahiert (Run-ID 20260614_135735_5b6776).*