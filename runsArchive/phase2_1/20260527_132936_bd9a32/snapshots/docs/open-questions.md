# Offene Fragen und Klärungsbedarfe Kundenportal MVP

## 1. Fachliche offene Fragen

- Wer ist der endgültige Pilotkunde (Müller AG Schweiz oder Hansa GmbH Deutschland)?
- Welche Länder und Währungen sind im MVP zwingend zu unterstützen (DACH, Schweiz, USA; EUR, CHF, USD)?
- Wie genau soll der Rabatt-Freigabeprozess aussehen, insbesondere Schwellenwerte (z.B. 15%, 20%, 30%) und Verantwortlichkeiten (Manager, Finance)?
- Welche KPIs sind für das MVP und spätere Phasen tatsächlich notwendig und messbar?
- Wie soll der Supportprozess nach dem MVP aussehen, speziell bezüglich Ticket- oder Kontaktformular und deren Persistenz?

## 2. Technische offene Fragen

- Welche Managed Services für Hosting und Datenbanken kommen zum Einsatz, und wie lassen sich diese EU-Datenschutz- und DSGVO-konform betreiben?
- Wie kann kurzfristig das API Gateway ersetzt oder umgangen werden, wenn die 6-Wochen Wartezeit für den MVP-Termin kritisch ist?
- Wie wird die Authentifizierung final umgesetzt, insbesondere Nutzung von SSO (Azure AD, Google) und Integration eines Identity Providers?
- Wie werden Backup- und Disaster-Recovery-Strategien im Detail ausgestaltet und umgesetzt?
- Wie soll der Umgang mit Entwicklungs- und Testumgebungen im Hinblick auf Datenschutz und echte/pseudonymisierte Daten geregelt sein?
- Wie werden das Logging und Auditierbarkeit technisch und datenschutzrechtlich sauber getrennt und umgesetzt?
- Welche technischen Maßnahmen sind für Performance und Skalierbarkeit zu treffen bei unklaren Nutzerzahlen (200 bis 20.000)?
- Wie wird die Schnittstelle zum SAP-System im Detail gestaltet, insbesondere mit Blick auf Echtzeitfähigkeit und Cache-Strategien?
- Wie werden Rate Limits und Missbrauchserkennung für das API- und Portal-System umgesetzt?

## 3. Widersprüche und Unsicherheiten

- Stand und Umfang des Security Reviews: Wie lässt sich ein ausreichender Sicherheitsstandard im engen MVP-Zeitrahmen gewährleisten?
- Umfang der Rollen- und Berechtigungslogik im MVP vs. Handhabbarkeit und Performance.
- Support-Funktionalität im MVP: Kontaktformular ohne Persistenz vs. notwendige Nachvollziehbarkeit und Effizienz.
- Nutzung von SAP-Testdaten in der Entwicklung versus Datenschutz-Compliance.
- Datenminimierung vs. Verfügbarkeit: Speicherung von Kundendaten im Portal vs. Abruf live aus SAP.
- Auswirkungen einer Verzögerung oder Nichtverfügbarkeit des API Gateways auf den MVP-Release.
- Umgang mit widersprüchlichen Anforderungen bezüglich Mobile (native App vs. Web responsive).
- Definition des MVP-Scopes hinsichtlich Features wie Sonderrabatte, Online-Akzeptanz von Angeboten, Push Notifications.

## 4. Organisatorische offene Fragen

- Wer übernimmt die Rolle des Systemarchitekten zur Sicherstellung der technischen und dokumentarischen Qualität?
- Wie und wann erfolgt die finale Entscheidung über den MVP-Scope und die bewussten Ausschlüsse?
- Welche Rollen und Verantwortlichkeiten werden für Datenschutz, Security und Support definiert?
- Wie wird die Koordination mit dem API Gateway-Team und IT Operations organisiert angesichts der Wartezeiten?
- Wer liefert die Kostenschätzung für Vorstand und wie wird diese gegen Unsicherheiten abgesichert?

---

*Diese offenen Fragen wurden aus dem Stakeholder-Transkript sowie dem Projektkontext extrahiert und systematisch aufbereitet. Die Klärung ist essentiell, um die Planung und Umsetzung des Kundenportals MVP zielgerichtet und risikoarm zu gestalten.*