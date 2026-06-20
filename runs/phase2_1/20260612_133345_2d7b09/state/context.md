# Projektkontext Kundenportal

## Projektziel
Das Projektziel ist die Entwicklung eines Kundenportals (bzw. einer Plattform), das primär dazu dient, die Angebotserstellung zu beschleunigen und den Kunden Bestellungen sowie Rechnungen bereitzustellen. Das MVP soll innerhalb von 8 Wochen realisiert werden.

## Sprecherrollen
- Anna: Produktmanagement, treibt das Portal und die MVP-Ziele voran.
- Ben: Technische Leitung/Architektur, fokussiert auf technische Machbarkeit und Integrationen.
- Clara: Datenschutz/Compliance, stellt sicher, dass DSGVO-Anforderungen umgesetzt werden.
- David: Kundenservice/Support, bringt Sicht auf Supportprozesse und Kundenanfragen ein.
- Eva: Finance, sieht finanzielle Risiken und Steuerungsaspekte im Angebotsprozess.
- Farid: IT-Betrieb, verantwortlich für Hosting, Infrastruktur und Sicherheitsaspekte.

## Fachliche Themen
- Kundenportal mit Web-First-Strategie, Mobile optional später
- Angebotserstellung mit SAP-Anbindung für Produktdaten, Preise und Rabattlogik
- Rechnungsdownload und Einsicht für Kunden
- Login-Konzept mit E-Mail/Passwort und optional SSO (Azure AD, Google)
- Rollen- und Berechtigungskonzepte (Admin, User, Manager, Support)
- DSGVO-konforme Speicherung und Verarbeitung personenbezogener Daten (Double-Opt-In, Löschkonzepte, Audit-Trails)
- Sicherheitsaspekte (Logging, Auditierbarkeit, Security Review, Verschlüsselung mit TLS)
- Hosting in der EU oder zumindest DSGVO-konform
- Nutzung von Managed Services, kein neuer eigener DB-Server
- API Layer für Integration (OAuth bevorzugt, API Gateway mit Warteliste)
- Backup und Disaster Recovery
- KPIs wie Conversion Rate und Angebotsdauer
- Rabattfreigabeprozesse (mindestens ab 15% Rabatt eine Freigabe erforderlich)
- PDF-Export von Angeboten mit Versionsmanagement und rechtlichen Fußnoten
- Performance- und Skalierbarkeitsaspekte (mit Nutzerzahlen von 200 bis 20.000)
- Supportprozesse ohne Ticketsystem im MVP, aber mit Kontaktformular und Datenschutz
- Mehrwährungsfähigkeit (EUR, CHF, USD später)
- Internationalisierung (Start DACH, später EU und USA)

## Konflikte und Unsicherheiten
- Mobile App native vs. responsive Web: Budget und Zeit unklar
- Zeitdruck für Security Review vs. MVP Zeitrahmen
- Unklare Supportprozesse im MVP (kein Ticketsystem, dafür manuelle E-Mail-Verwaltung)
- API Gateway Warteliste steht im Widerspruch zur MVP Timeline
- Hosting-Anforderungen EU-only vs. Kosten
- Unklare Pilotkunden mit Auswirkungen auf Datenschutz und Währungsanforderungen
- SAP Anbindung nur lesend im MVP, Schreibzugriff unklar
- Cache Nutzung für Produkt- und Preisdaten problematisch wegen Datenschutz
- Rabattfreigabeprozess für Sonderrabatte erst in späterer Phase
- Risiko und Komplexität hoher Audit- und Compliance-Anforderungen im engen Zeitrahmen
- Unklare Testdatenstrategie und Umgang mit personenbezogenen Daten in Tests
- Support und Finance haben teils widersprüchliche Berechtigungs- und Einsichtsbedarfe
- Kein finaler Architekturentscheid, keine fertige technische Lösung für API Layer
- Backup, Monitoring und Logging unterschieden, unterschiedliche Datenschutzanforderungen
- Umgang mit Löschanfragen vs. gesetzlicher Aufbewahrungspflichten

## Quellenhinweise
Das Dokument basiert ausschließlich auf dem Stakeholder-Transkript input/transcripts/T9999_chaos.txt, in dem mehrere Beteiligte sehr detailliert ihre Sichtweisen, Anforderungen, und Bedenken diskutieren. Es enthält viele direkte Zitate und spontane Anmerkungen zu technischen, rechtlichen und organisatorischen Aspekten.

---

Dieser Kontext versucht, den komplexen und zum Teil widersprüchlichen Stand des Projekts neutral und umfassend abzubilden. Offene Fragen und Risiken werden bewusst markiert, um im weiteren Projektverlauf adressiert zu werden.
