# Offene Fragen und Klärungsbedarfe für das Kundenportal MVP

## 1. Fachliche Fragen
- Wer ist der endgültige Pilotkunde (DACH-Region, Schweiz oder international)? Dies beeinflusst Datenschutz, Hosting und Währungsanforderungen.
- Wie genau soll der Freigabeprozess für Rabatte im Angebot gestaltet werden? Welche Freigaberollen sind notwendig und wie wird dies technisch abgebildet?
- Wie soll der Supportprozess im MVP konkret aussehen, insbesondere ohne Ticketsystem? Wie werden Support-Anfragen dokumentiert und nachverfolgt?
- Welche konkreten KPIs sollen gemessen werden, und wie soll das KPI-Tracking technisch und organisatorisch erfolgen?
- Gibt es abschließende Entscheidungen zur Unterstützung mehrerer Währungen und Sprachen im MVP?

## 2. Technische Fragen
- Wie wird die Abhängigkeit vom SAP-System hinsichtlich Verfügbarkeit und Datenaktualität zuverlässig gemanagt? Sind Fallback-Strategien vorhanden?
- Wie wird die verpflichtende Nutzung des zentralen API-Gateways garantiert beziehungsweise wie sehen Übergangslösungen aus angesichts der langen Warteliste?
- Welche konkreten Maßnahmen für Backup und Disaster Recovery werden genutzt, und wie werden diese umgesetzt?
- Wie soll das geheime Schlüssel- und Secrets-Management erfolgen (z.B. API-Keys, OAuth Tokens)?
- Welche Lösung wird im Bereich Identity and Access Management (IAM) und SSO implementiert, und in welchem Zeitrahmen?
- Welche Maßnahmen zur Skalierbarkeit werden geplant, um mit Nutzerzahlen von 200 bis 20.000 umzugehen?

## 3. Compliance- und Sicherheitsfragen
- Wie wird die EU-DSGVO im Detail umgesetzt, insbesondere bezüglich Double-Opt-In, Löschkonzepten und differenzierten Zugriffskontrollen?
- Wie wird die Einhaltung der Datenresidenz sichergestellt, und welche Cloud- oder Managed-Service-Anbieter sind zugelassen?
- Wie werden Audit- und Logging-Anforderungen technisch und organisatorisch getrennt, um Datenschutz und Nachvollziehbarkeit zu gewährleisten?
- Wie wird mit echten Kundendaten in Testumgebungen umgegangen, um Datenschutzverstöße zu vermeiden?
- Wie werden Security-Reviews in den engen MVP-Zeitplan integriert?

## 4. Organisatorische und Prozessfragen
- Wer übernimmt die Verantwortung und Koordination für die Architektur- und Sicherheitsentscheidungen?
- Wie wird die Dokumentation im Projekt so gestaltet, dass sie den Anforderungen für Security Reviews und Compliance genügt, ohne den Zeitplan zu sprengen?
- Welche Rolle soll ein Architekt im Projekt einnehmen, und wie wird diese Position besetzt?
- Wie werden Risiken und Konflikte im Verlauf laufend kommuniziert und gesteuert?

---

Diese Liste ist ein lebendes Dokument und sollte fortlaufend aktualisiert werden, um Transparenz für alle Stakeholder zu gewährleisten und die weitere Projektarbeit strukturiert zu unterstützen.

*Basierend auf den Artefakten runs/phase2_1/20260602_135337_3c5370/state/context.md, docs/requirements.md, docs/risks.md, docs/architecture.md und dem Stakeholder-Transkript T9999_chaos.txt.*