# Risikoartefakt für Kundenportal / Angebotsplattform

## 1. Fachliche Risiken

- Unklare Zieldefinition und MVP-Scope: Unterschiedliche Erwartungen (z.B. Support, Rabattfreigabe, Mobile, KPI-Tracking) können zu Scope Creep und Verzögerungen führen.
- Widersprüchliche Anforderungen im Rabattfreigabeprozess bzw. keine klare Regelung im MVP, was finanzielle Risiken durch falsche Angebote birgt.
- Supportprozess im MVP unzureichend definiert (nur Kontaktformular, keine Ticketpersistenz), führt zu operativen Risiken und möglicher schlechter Kundenzufriedenheit.
- Unklare Pilotkunde-Auswahl (Deutschland vs. Schweiz) mit Einfluss auf Datenschutz, Währung und rechtliche Anforderungen.

## 2. Technische Risiken

- API Gateway zentrale Komponente mit 6-Wochen-Warteliste; nicht verfügbar im MVP, führt zu fehlender zentraler API-Security und Integrationsproblematik.
- SAP-Anbindung ist wesentliche Abhängigkeit; Verfügbarkeitsprobleme (Wartungsfenster) können Angebotserstellung oder Datenkonsistenz gefährden.
- Fehlender Architekturverantwortlicher erschwert klare technische Entscheidungen und kann zu inkonsistenter Architektur führen.
- Keine API- oder Frontend-Entscheidung zu SSO, OAuth, Mobile App; Unsicherheiten gefährden die Umsetzung oder erhöhen Aufwand.
- Gefahr von Overengineering vs. Skalierbarkeit: unklare Balance kann zu unnötigen Verzögerungen oder schlechter Performance führen.
- Backup, Monitoring und Audit-Funktionalitäten sind aufwendig und müssen Datenschutzanforderungen erfüllen, sonst Compliance-Risiken.
- Nutzung von Managed Services ohne klare Auswahl, insbesondere für Hosting, API Gateway, Backend-Services, erzeugt Unsicherheit bezüglich Kosten und Funktionalität.
- Fehlende Secrets Management und Testdatenpseudonymisierung können zu Sicherheitslücken und Compliance-Verstößen führen.

## 3. Compliance- und Datenschutzrisiken

- DSGVO-Konformität ist zwingend, aber komplexe Anforderungen (Double-Opt-In, Löschkonzept, Auditierung, Datenresidenz) sind nur teilweise im MVP abgedeckt.
- EU-only Hosting mit richtiger Datenresidenz ist kostspielig und technisch herausfordernd (Backup-Replikation global könnte Konflikte verursachen).
- Technische Logs dürfen keine personenbezogenen Daten enthalten; Unterscheidung Audit-Logs vs. Application Logs unsicher.
- Nutzung von echten Kundendaten in Testumgebungen ist kritisch und erfordert Pseudonymisierung oder synthetische Daten.
- Rechtliche Aufbewahrungsfristen kollidieren mit Löschanfragen; fehlende Datenklassifizierung und Retention Policies bergen Rechtsrisiken.

## 4. Widersprüche und Unsicherheiten

- Zeitlicher Druck (8 Wochen MVP) vs. notwendige Security Reviews und Compliance-Anforderungen sind nicht kompatibel.
- Financial und Compliance Anforderungen (z.B. Rabattfreigabe, Aufbewahrungsfristen) sind noch nicht ausreichend definiert oder technisch umgesetzt.
- Supportprozess und Kundenkommunikation sind schwach definiert und bergen Datenschutz- und Betriebsrisiken.
- Unklarheit bei Pilotkunde Standort, Währung, Datenschutz-Vorgaben führt zu Projektunsicherheiten.
- API Security (OAuth vs. API Keys) ist noch offen, was sich auf Architektur und Entwicklungsaufwand auswirkt.

## 5. Mögliche Auswirkungen

- Verzögerungen im Projekt durch nicht verfügbare zentrale Komponenten (API Gateway) und langwierige Security Reviews.
- Finanzielle Risiken durch fehlerhafte oder nicht freigegebene Rabatte und fehlende Freigabeprozesse.
- Gesetzesverstöße durch unzureichende DSGVO-Implementierung und fehlende Datenlöschung oder Auditierung.
- Kundenunzufriedenheit und erhöhtes Supportaufkommen wegen fehlendem Ticketsystem und schlechter Prozessunterstützung.
- Technische Instabilität oder schlechte Performance durch unklare Architektur und fehlende Skalierbarkeitsentscheidungen.

## 6. Empfehlungen und Gegenmaßnahmen

- Klare Abgrenzung des MVP-Scopes mit bewusster Dokumentation ausgeschlossener Funktionen und Risiken.
- Prioritäre Planung und Begleitung der API Gateway Integration oder Suche nach temporären Alternativen.
- Festlegung eines Architekturverantwortlichen zur Koordinierung technischer Entscheidungen und Qualität.
- Frühzeitige Sicherheitsreviews mit pragmatischen Kompromissen für MVP, um Compliance-Risiken zu reduzieren.
- Implementierung eines minimalen und DSGVO-konformen Lösch- und Auditkonzepts im MVP.
- Klärung und Dokumentation des Pilotkunden und deren rechtlicher Anforderungen so früh wie möglich.
- Planung eines schrittweisen Supports mit klarer Kommunikation der Einschränkungen im MVP.
- Aufbau eines Secrets Managements und Nutzung pseudonymisierter Testdaten zur Minimierung von Sicherheitsrisiken.
- Budget- und Zeitplanung unter Berücksichtigung möglicher technischer und regulatorischer Aufwände.

---

*Dieses Risikoartefakt basiert auf der detaillierten Stakeholder-Diskussion (input/transcripts/T9999_chaos.txt) und den abgeleiteten Requirements (docs/requirements.md) sowie Projektkontext (runs/phase2_1/20260529_131036_dc98b8/state/context.md).*