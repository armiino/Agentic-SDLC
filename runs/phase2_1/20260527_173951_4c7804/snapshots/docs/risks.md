# Risikoartefakt für das Kundenportal MVP

## 1. Fachliche Risiken

- **Unklare Zieldefinition und Scope:** Die umfangreichen Anforderungen und unterschiedliche Prioritäten der Stakeholder (z.B. Support, Rabattfreigabe, KPI-Tracking) führen zu Widersprüchen im Scope. Risiko von Fehlentwicklungen und unerfüllten Erwartungen.
- **Rabattfreigabe und Angebotsworkflow:** Fehlende klare Definition und Automatisierung des Freigabeprozesses erhöhen das Risiko von fehlerhaften Angeboten und finanziellem Schaden.
- **Supportprozess ohne Persistenz:** MVP nutzt nur ein Kontaktformular ohne Ticketsystem, was die Supporteffizienz stark einschränkt und zu unstrukturiertem Arbeiten führt.

## 2. Technische Risiken

- **SAP-Verfügbarkeit und Integrationskomplexität:** Verfügbarkeit der SAP-Systeme ist kritisch, fehlende Echtzeitpreise oder Wartungsfenster können Angebotserstellung beeinträchtigen.
- **API Gateway Warteliste:** Das zentrale API-Gateway hat eine Warteliste von 6 Wochen, was den 8-Wochen-MVP-Zeitrahmen stark gefährden kann.
- **Fehlendes zentrales IAM:** Keine klare Identity- und Access-Management Lösung, z.B. für SSO und Rollen, kann zu Sicherheitsproblemen führen.
- **Kein eigener DB-Server erlaubt:** Abhängigkeit von Managed Services könnte Begrenzungen und höhere Kosten verursachen.
- **Logging und Monitoring-Komplexität:** Unterschiedliche Log-Typen (Audit, Security, Application) müssen sauber getrennt werden, sonst Risiko von Datenschutzverletzungen.
- **API Rate Limiting und Missbrauchserkennung:** Fehlende oder unzureichende Mechanismen können zu Überlastungen oder Missbrauch führen.
- **Testdatenmanagement:** Nutzung von echten Kundendaten in Testsystemen ist datenschutzrechtlich problematisch und kann Entwicklung und Testing behindern.

## 3. Compliance- und Datenschutzrisiken

- **DSGVO-Konformität:** Unsicherheiten beim Double-Opt-In, Löschkonzepten, Auditierung und Datenresidenz (EU-only Hosting) sind kritisch, insbesondere bei internationalem Scope und multinationale Kunden.
- **Widerspruch Datenschutz vs. Aufbewahrungspflichten:** Gesetzliche Aufbewahrungspflichten kollidieren mit Löschanfragen, Risikopotenzial bezüglich Compliance-Verstößen.
- **Datenhosting und Backup:** EU-only Datenhosting erfordert Nachweis und entsprechende Verträge; globale Backups sind problematisch.
- **Supportdaten per E-Mail:** Verarbeitung von Supportanfragen via E-Mail ohne persistente Speicherung erschwert Datenschutz und Nachvollziehbarkeit.

## 4. Widersprüche und Unsicherheiten

- **Mobile App vs. Budget/Zeit:** Wunsch nach mobiler App kollidiert mit technischem und finanziellem Aufwand sowie MVP-Zeitplan.
- **API-Sicherheitsmechanismen:** OAuth bevorzugt, aber noch nicht final entschieden; einfache API Keys sind unsicherer.
- **Pilotkunde unklar:** Schweiz vs. Deutschland beeinflusst Datenschutzanforderungen, Währungen und projektbezogene Risiken.
- **KPI-Tracking und Analytics:** Noch unklar im MVP, mögliche spätere Compliance- und Privacy-Fragen.
- **Freigabeprozess für Rabatte und Angebotsworkflow unklar:** Erfordert weitere Klärung, sonst Gefahr von Fehlprozessen.

## 5. Mögliche Auswirkungen

- Verzögerungen im Projektzeitplan durch API Gateway Wartezeiten und notwendige Security Reviews.
- Finanzielle Risiken durch fehlerhafte Rabattfreigaben und Angebotsfehler.
- Compliance-Verstöße mit möglichen Bußgeldern durch mangelhafte Datenschutzmaßnahmen.
- Erhöhter Supportaufwand und unzufriedene Kunden durch intransparente Supportprozesse.
- Technische Risiken können zu Systemausfällen, schlechter Performance oder Sicherheitslücken führen.

## 6. Mögliche Gegenmaßnahmen oder Klärungsbedarfe

- Klare Definition des MVP-Scope mit bewussten Ausschlüssen und dokumentierten Risiken.
- Priorisierung und Implementierung eines minimalen, aber sicheren Rabattfreigabeprozesses.
- Planung und Evaluation von API Gateway Alternativen oder temporären Workarounds.
- Einführung eines Identity- und Access-Management Systems möglichst frühzeitig.
- Klärung und Dokumentation von Datenschutzmaßnahmen mit Data Protection Officer.
- Planung von Backup- und Disaster-Recovery-Strategien mit Managed Services.
- Etablierung eines Testdatenmanagements mit Pseudonymisierung.
- Festlegung von Compliance-konformen Logging- und Monitoring-Konzepten.
- Festhalten der bewussten Einschränkung beim Support im MVP und Vorbereitung für Verbesserung.
- Klarheit über Pilotkunden und deren spezifische Anforderungen erlangen.
- Regelmäßige Abstimmung der Anforderungen und Risiken mit Stakeholdern und dem Projektteam.

---

Diese Risiken wurden aus dem bereitgestellten Kontext sowie dem umfassenden Stakeholder-Transkript abgeleitet und priorisiert, um eine fundierte Risikoübersicht für die weitere Projektplanung zu schaffen.