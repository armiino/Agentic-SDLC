# Projektkontext Kundenportal - Angebots- und Rechnungsplattform

## Projektziel
- Entwicklung eines Kundenportals (Web-first) mit Fokus auf schneller Angebotserstellung und Anzeige von Rechnungen.
- Integration von SAP-Daten zur Nutzung von Produktinformationen, Preisen und Kundenstammdaten.
- MVP (Minimum Viable Product) soll in 8 Wochen geliefert werden.
- Ziel ist primär die Unterstützung von Sales durch schnellere Angebotserstellung, Anzeige von Bestellungen und Rechnungen, sowie ein einfacher Login mit Rollen (Admin, Sales, Kunde).
- Weitere Funktionen wie Support, Rabattfreigabe, Push Notifications und detaillierte Analytics sind bewusst außerhalb des MVP oder als Folgephasen geplant.

## Beteiligte Sprecher/Rollen
- Anna (Projektleitung / Sales)
- Ben (Technik / IT-Architektur)
- Clara (Datenschutz / Compliance)
- David (Customer Support)
- Eva (Finanzen)
- Farid (IT Operations)

## Fachliche Themen und Anforderungen
- Login mit E-Mail/Passwort, Double-Opt-In und evtl. SSO (Azure AD/Google) - SSO optional und noch nicht final entschieden.
- Rollen- und Berechtigungskonzept mit mindestens Admin, Sales, Kunde; Support-Rollen eingeschränkt oder ohne direkten MVP-Zugang.
- Angebotserstellung mit SAP-Daten, keine Sonderrabatte im MVP ohne Freigabe (Freigabeprozess außerhalb MVP).
- Anzeigen und Download von Rechnungen.
- Minimaler Audit Trail und Logging zur Nachvollziehbarkeit von Datenänderungen und Zugriffen.
- EU-only Hosting (DSGVO-konform mit nachweisbarer Datenresidenz), Backup und Disaster Recovery.
- API Layer zur Integration (OAuth bevorzugt, aber noch nicht final), zentrales API Gateway wird erwartet, aber hat lange Warteliste.
- KPIs sollen später gemessen werden (Conversion Rate Angebot zu Bestellung, Zeit bis Angebot), aber nicht im MVP enthalten.

## Konflikte und offene Punkte
- Zeit- und Scope-Konflikt: MVP in 8 Wochen vs. Komplexität und notwendige Security Reviews.
- Keine finale Architekturentscheidung bzgl. Cloud Provider; keine Produktbindung ohne Entscheidung.
- Supportprozess unklar: Kein Ticketsystem im MVP, nur Kontaktformular mit E-Mail-Verarbeitung, was Risiken bei Datenschutz und Audit verursacht.
- Rabattfreigabeprozess fehlt im MVP, führt zu finanziellen Risiken, muss aber später umgesetzt werden.
- API Gateway-Verfügbarkeit in Zeitplan fraglich (6 Wochen Warteliste).
- SAP Anbindung kritisch: Preise nicht immer in Echtzeit aktuell, Auswirkungen auf Angebotsgültigkeit.
- Datenschutzrichtlinien erfordern umfassende technische und organisatorische Maßnahmen (Double Opt-In, Löschkonzept, Audit, keine personenbezogene Daten in technischen Logs).
- Testdatenproblematik: SAP-Testsystem beinhaltet echte Kundendaten, Problem für DSGVO-Konformität und Integrationstests.
- Hosting in der EU mit Datenresidenz kann teurer sein, Auswirkungen auf Projektkosten.
- Unklare Zielgruppe hinsichtlich Pilotkunde: Schweiz (mit zusätzlichen Datenschutz- und Währungsfragen) oder Deutschland.
- Widersprüche zwischen Anforderungen (z.B. Skalierbarkeit vs. Einfachheit, Security Review Zeit vs. MVP-Termin) bleiben sichtbar und müssen transparent behandelt werden.

## Unsicherheiten
- Endgültige Entscheidung zu Pilotkunde und daraus folgende Anforderungen (Länder, Währungen, Datenschutz).
- Umfang und Details des Freigabeprozesses für Rabatte.
- Wahl des Identity Providers und Art der Integration (SSO, OAuth).
- Verfügbarkeitsanforderungen und Fallbacks bei SAP-Ausfällen.
- Konkrete Kosten für EU-only Hosting und Managed Services.
- API Gateway Zugang innerhalb des Zeitplans.
- Detailtiefe des Audit Trails und Aufbewahrungsfristen.

## Quellenhinweise
- Transkript: input/transcripts/T9999_chaos.txt
- Erste Stakeholderrunde mit Anna, Ben, Clara, David, Eva, Farid

---

Diese Zusammenfassung bildet die Kernaspekte und Widersprüche des Gespräches ab, ohne sie aufzulösen. Sie ermöglicht eine belastbare Grundlage für nächste Analysephasen und die Weiterentwicklung der Requirements mit expliziten Risiken und offenen Fragen.