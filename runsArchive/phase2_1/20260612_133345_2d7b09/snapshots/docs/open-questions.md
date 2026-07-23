# Offene Fragen und Klärungsbedarf Kundenportal

## 1. Fachliche Fragen

- **MVP-Scope:** Welche Funktionen sind verbindlich im 8-Wochen-MVP enthalten, und welche Features (z.B. Rabattfreigabe, Support-Tickets, Mehrwährungen, PDF-Export mit Rechtskonformität) werden auf spätere Phasen verschoben? 
- **Pilotkunden & Internationalisierung:** Welcher Pilotkunde wird final fokussiert? Welche Anforderungen an Währungen, Sprachen und Datenschutz ergeben sich daraus (DACH, Schweiz, EU, USA)?
- **Supportprozess:** Wie wird der Supportprozess ohne Ticketsystem im MVP konkret datenschutzkonform gestaltet? Ist eine spätere Einführung eines Ticketsystems geplant und wie wird der Übergang gestaltet?

## 2. Technische Fragen

- **API Gateway Verfügbarkeit:** Wie wird mit der möglichen Verzögerung des zentralen API Gateways umgegangen? Gibt es alternative Architekturkonzepte oder temporäre Lösungen für die Integration?
- **Authentifizierung und SSO:** Welche Identity Provider (z.B. Azure AD, Google) werden im MVP unterstützt? Wie wird das Rollen- und Berechtigungskonzept detailliert ausgestaltet, insbesondere hinsichtlich Support-Visibility und Datenschutz?
- **SAP Integration:** Wie soll der Umgang mit Preis- und Rabattdaten erfolgen, insbesondere bei teils nächtlichen Aktualisierungen? Sind Schreibzugriffe im Portal langfristig vorgesehen und wie wird das technisch realisiert?
- **Testdaten & Entwicklung:** Welche Richtlinien gelten für den Umgang mit personenbezogenen Daten in Entwicklungs- und Testumgebungen? Gibt es Vorgaben zur Anonymisierung oder Ersatzdaten?

## 3. Compliance- und Datenschutzfragen

- **EU-only Hosting und Datenresidenz:** Wie wird die Einhaltung von EU-only Hosting bzw. strenger DSGVO-Konformität nachweisbar sichergestellt, insbesondere bei Nutzung von Managed Services?
- **Lösch- und Aufbewahrungskonzepte:** Wie werden gesetzliche Aufbewahrungspflichten und das Recht auf Datenlöschung technisch und organisatorisch umgesetzt und priorisiert?
- **Datenschutz im Support:** Wie kann die unstrukturierte E-Mail-basierte Supportbearbeitung DSGVO-konform gestaltet werden, gerade in Bezug auf Datenlöschung, Zugriffsrechte und Nachvollziehbarkeit?
- **Auditierung und Logging:** Wie wird eine Balance zwischen notwendigen Audit-Logs und datenschutzrechtlichen Vorgaben hinsichtlich Umfang und Speicherdauer gefunden?

## Ansprechpartner oder Rollen für Klärungen

- Produktmanagement (Anna) für MVP Scope, Pilotkunden, fachliche Priorisierung
- Technische Leitung/Architektur (Ben) für API Gateway, SAP Integration, Authentifizierung, Testdaten
- Datenschutzbeauftragte (Clara) für DSGVO-Anforderungen, Löschkonzepte, Hosting
- Support-Verantwortliche (David) für Supportprozesse und Kundenkommunikation
- Finanzverantwortliche (Eva) für Rabattfreigaben und finanzielle Risiken
- IT Operations (Farid) für Hosting, Backup, Datensicherheit

---

*Dieses Dokument fasst identifizierte offene Fragen und Klärungsbedarfe aus dem bisherigen Projektkontext, den Anforderungen, der Risikoanalyse und Architektur ab. Es dient als transparente Grundlage für weitere Entscheidungen und Priorisierungen im Projekt.*