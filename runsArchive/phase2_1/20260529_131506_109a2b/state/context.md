# Projektkontext Kundenportal MVP

## Projektziel
Das Ziel ist die Entwicklung eines Kundenportals zur schnelleren Angebotserstellung und -verwaltung, inklusive Rechnungsdownload, mit einem MVP-Zeitrahmen von 8 Wochen. Das Portal dient als Plattform für Kunden, Sales, Finance und Support mit Fokus auf Nutzerrollen, Datenschutz und SAP-Integration.

## Stakeholder und Rollen
- Anna (Sales, Projektleitung)
- Ben (IT-Architekt/Backend)
- Clara (Datenschutz, Compliance)
- David (Customer Support)
- Eva (Finance)
- Farid (IT Operations)

## Fachliche Themen
- Kundenportal als Webportal, mobile Nutzung optional und später
- Angebote: Erstellung, Freigabeprozess (ab späterer Phase), PDF-Export mit rechtlicher Nachvollziehbarkeit
- Rechnungsdownload durch Kunden
- Login: E-Mail/Passwort, Double-Opt-In für DSGVO, SSO optional und noch unklar
- Rollenmodell: Admin, Sales, Kunde, Manager, Support (eingeschränkte Sicht)
- SAP Integration: Lesender Zugriff für Produktdaten, Preise, Rabattlogik; Schreibzugriff ist unklar
- KPI-Tracking: Conversion Rate Angebot zu Bestellung, Zeit bis Angebot
- Datenschutz: DSGVO-konform, personenbezogene Daten, Löschkonzept, Audit- und Protokollierungspflichten
- Hosting: EU-Only oder DSGVO-konform mit Nachweis, Risiko Mehrkosten
- Backup und Disaster Recovery
- API Layer für Integration, OAuth bevorzugt, API Gateway mit langer Wartezeit
- Support: Kontaktformular im MVP, kein Ticketsystem, manuelle E-Mail-Verarbeitung als bewusste Einschränkung
- Mehrwährung und Internationalisierung später (Pilotkunde Schweiz oder DACH, noch offen)
- Leistungsanforderungen: Skalierbarkeit (200 bis 20.000 Nutzer), Performance, Caching, Fallback bei SAP-Ausfall
- Sicherheit: TLS, kein End-to-End-Verschlüsselung, Security Review erforderlich, Kontrolle über Logs und Monitoring
- Testumgebungen mit anonymisierten/synthetischen Daten, Secrets Management, Rate Limiting

## Konflikte und offene Fragen
- Mobile native App versus Web responsive: Budget und Backend-Readiness begrenzen Optionen
- Datenschutz versus schnelle Umsetzung: DSGVO-Auflagen, Audit-Mechanismen versus 8-Wochen-MVP
- Rollen- und Berechtigungsmodell: Supportzugriff versus Datenschutz und Vertraulichkeit
- Rabatt-Freigaben: Komplexer Freigabeprozess versus MVP Scope
- SAP-Integration: Echtzeitdaten, Preissynchronität und Schreibrechte unklar
- Hosting-Kosten und EU-Datenresidenz versus Kostendruck
- API Gateway-Warteliste (6 Wochen) versus MVP-Termin
- Supportprozess als bewusste Einschränkung ohne Ticketsystem mit Risiken
- Angebotserstellung ohne Online-Akzeptanz, Auswirkungen auf Conversion Tracking
- Backup, Monitoring und Sicherheitslogging mit Trennung von Audit Logs
- Testdaten und SAP-Testsystem mit echten Kundendaten
- Noch keine finale Entscheidung zu Pilotkunde-Standort und Währung

## Risiken
- Unklare oder fehlende Architektur und IAM für SSO
- Compliance-Probleme durch manuelle Supportdatenverarbeitung
- Fehlerpotenzial durch manuelle Uploads und fehlende Freigabeprozesse
- Zeitdruck bei Security Review und API Gateway Bereitstellung
- Performance- und Skalierungsthemen bei unklarer Nutzerzahl
- Gesetzliche Aufbewahrungspflichten versus Löschanfragen
- Datenminimierung versus Performance bei SAP-Datenabruf
- Komplexität des Rabatt- und Freigabewesens im MVP
- Fehlende Integration von Support und Finance Anforderungen im MVP
- Risiken durch Cache und Fallback Logiken bei SAP-Ausfällen

## Quellennachweis
Basierend auf Transkript: input/transcripts/T9999_chaos.txt

---
Dies ist eine faktenbasierte Zusammenfassung des Gesprächs, die widersprüchliche Anforderungen, offene Punkte und Risiken bewusst abbildet ohne sie zu glätten. Der Fokus liegt auf einer neutralen Darstellung des gegenwärtigen Projektkontexts für weitere Anforderungenserhebung und Architekturarbeit.