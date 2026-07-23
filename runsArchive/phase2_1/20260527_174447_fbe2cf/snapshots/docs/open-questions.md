# Offene Fragen und Klärungsbedarfe aus Stakeholderdiskussion und Artefakten

## 1. Fachliche offene Fragen

- Wer ist der finale Pilotkunde und in welcher Region ist dieser angesiedelt? (DACH, Schweiz oder später EU/USA)
- Welche genauen Anforderungen und Prioritäten hat der Pilotkunde, insbesondere bzgl. Mehrwährung (EUR, CHF, USD)?
- Wie detailliert muss der Rabattfreigabeprozess im MVP wirklich sein? Welche Rabatthöhen sind freigabepflichtig?
- Wie wird der Supportprozess mittelfristig gestaltet, wenn kein Ticketsystem im MVP vorgesehen ist?
- Welche Rolle übernimmt der Support hinsichtlich Einsicht in Angebote und Kundendaten?
- Wer ist verantwortlich für die Pflege und Verwaltung der Stammdaten, wenn diese teilweise unvollständig und in SAP liegen?

## 2. Technische offene Fragen

- Welche Identity-Management-Lösung wird final eingesetzt? (OAuth, SSO via Azure AD, Google oder anderes)
- Wie wird der API Gateway Mangel (6 Wochen Warteliste) für das MVP pragmatisch gelöst?
- Welcher Managed Service Anbieter wird für EU-only Hosting gewählt, und wie hoch sind die Kosten?
- Wie werden Backup und Disaster Recovery im Detail umgesetzt und dokumentiert?
- Wie wird sichergestellt, dass Testumgebungen keine echten Kundendaten enthalten, insbesondere im SAP-Testsystem?
- Gibt es eine Lösung für die kritische Abhängigkeit auf SAP-Verfügbarkeit (Fallback-, Cache-Strategie)?
- Welche konkreten Verschlüsselungs- und Sicherheitsmaßnahmen (neben TLS) werden im MVP umgesetzt?
- Wie wird sichergestellt, dass keine personenbezogenen Daten in technischen Logs landen?
- Wie und wo findet die Integration und Versionierung der Angebots-PDF-Templates statt?
- Welche Monitoring- und Rate-Limiting-Konzepte können ohne API Gateway genutzt werden?

## 3. Widersprüche und Spannungsfelder

- DSGVO-Anforderungen vs. Zeitrahmen (Security Review vs. 8-Wochen-MVP)
- Wunsch nach MVP mit umfangreichen Funktionen vs. technischer und finanzieller Realisierbarkeit
- Supportqualität vs. Datenschutz (Kontaktformular ohne Persistenz)
- Datenlöschungen vs. gesetzliche Aufbewahrungspflichten
- SAP-Echtzeitdaten für Angebotserstellung vs. SAP-Wartungsfenster und Verfügbarkeit
- Overengineering bzw. Skalierbarkeit vs. Einfachheit im MVP
- API Gateway-Integration vs. Verfügbarkeit und Wartezeit

## 4. Fehlende Informationen

- Genaue Anforderungen an KPIs und deren technische Umsetzung (z.B. Conversion Rate, Zeit bis Angebot)
- Endgültige Entscheidung und Details zum Rollen- und Berechtigungskonzept, insb. für Support, Sales, Finance
- Konkrete Datenschutzmaßnahmen für Supportanfragen via E-Mail (Löschung, Archivierung)
- Detaillierung der Backup- und Recovery-Anforderungen (erwartete RTO/RPO)
- Entscheidungskriterien für die technologische Umsetzung des API-Layers und Authentifizierung
- Dokumentationsumfang und Verantwortlichkeiten in der operativen Phase

## 5. Ansprechpartner und Rollen (erkennbar oder benötigt)

- Projektleitung / Sales (Anna) für fachliche Zielsetzungen und MVP-Definition
- IT Architektur / Backend (Ben) für technische Entscheidungen
- Datenschutz / Compliance (Clara) für DSGVO und Audit
- Customer Support (David) für Supportprozesse und Kundensicht
- Finance (Eva) für Freigabeprozesse und finanzielle Risiken
- IT Operations (Farid) für Hosting, Backup, Monitoring und Sicherheit
- Ein Architekt oder technischer Ansprechpartner wird explizit vermisst zur Koordination und Dokumentation

---

*Dieses Dokument basiert auf der Auswertung des Stakeholder-Transkripts input/transcripts/T9999_chaos.txt, dem Kontext artefakt runs/phase2_1/20260527_174447_fbe2cf/state/context.md sowie Anforderungen und Risikoartefakten.*