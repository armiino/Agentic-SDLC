# Risikoanalyse Kundenportal

## 1. Fachliche Risiken
- **Unklare Zieldefinition und Scope:** Unterschiedliche Anforderungen (z.B. Angebotserstellung, Rechnungsdownload, Support) sind nicht final priorisiert. MVP-Schnitt ist bewusst eng, birgt jedoch Risiken, dass Stakeholder-Erwartungen nicht erfüllt werden.
- **Support-Prozesse unzureichend:** Fehlen eines Ticketsystems im MVP führt zu manueller und fehleranfälliger Bearbeitung von Kundenanfragen und Datenschutzrisiken bei Nutzung von E-Mail oder Kontaktformular.
- **Konflikte bei Rollen und Berechtigungen:** Unterschiedliche Ansprüche an Sichtbarkeit und Rechte (z.B. Sales vs. Support) können zu Inkonsistenzen und Sicherheitslücken führen.

## 2. Technische Risiken
- **SAP-Integration kritisch:** SAP-Schnittstellen sind wartungsanfällig, haben Wartungsfenster und können Verfügbarkeitsrisiken verursachen, die Angebotserstellung zeitweise blockieren oder zu Dateninkonsistenzen führen.
- **API Gateway Wartezeiten:** Geplante Nutzung eines zentralen API Gateways hat eine Warteliste von sechs Wochen und kann den geplanten 8-Wochen-MVP-Zeitrahmen gefährden.
- **Kein neuer DB-Server, Managed Services:** Abhängigkeit von externen Services kann Kosten und Skalierbarkeit einschränken; Auswahl und Integration sind noch nicht erfolgt.
- **Testdaten und Umgebungen:** Nutzung von SAP-Testsystem mit echten Kundendaten birgt Datenschutzrisiken; fehlende Pseudonymisierung könnte Compliance-Verstöße verursachen.

## 3. Compliance- und Datenschutzrisiken
- **DSGVO-Konformität zwingend:** Anforderungen an Double-Opt-In, Löschkonzepte, Audit-Trails und Zugriffskontrollen sind komplex und könnten den MVP verzögern.
- **Audit und Logging:** Balance zwischen ausreichender Auditierbarkeit und Vermeidung der Protokollierung personenbezogener Daten ist schwierig und birgt Risiken bei Datenverlust oder Compliance-Verstößen.
- **Backup und Hosting:** EU-only Hosting und Backup-Anforderungen sind noch unklar und können Mehrkosten und Verzögerungen verursachen.
- **Retention und Löschung:** Widersprüchliche Anforderungen zwischen handelsrechtlichen Aufbewahrungspflichten und Datenschutzrecht (Recht auf Löschung) bergen juristische Risiken.

## 4. Widersprüche und Unsicherheiten
- **Zeitdruck kontra Sicherheitsreview:** Ein notwendiger Security Review kann den knappen MVP-Termin (8 Wochen) sprengen.
- **Funktionaler Umfang:** Umfangreiche Funktionen (Freigabeprozesse, Multiwährung, Mehrsprachigkeit, Push Notifications) sind offen und werden für MVP zurückgestellt, was spätere Integrationsrisiken birgt.
- **Pilotkunde und Markt:** Unklare Pilotkundenwahl (DACH vs. Schweiz) beeinflusst Datenschutz, Währung, Sprache und Infrastruktur.
- **Preis- und Rabattlogik:** Komplexität der Rabattfreigaben und Aktualität der SAP-Preise sind nicht final geklärt, was finanzielle Risiken birgt.

## 5. Potenzielle Auswirkungen
- Verzögerungen und Budgetüberschreitungen durch unklare Scope-Definition und fehlende Architekturentscheidungen.
- Qualitätsverluste bei Compliance und Sicherheit durch Zeitdruck.
- Erhöhte Supportkosten und schlechter Kundenservice aufgrund fehlender Supportprozesse.
- Sicherheitsvorfälle durch unzureichende Zugriffskontrollen und mangelhafte Auditierung.
- Rechtliche Risiken durch Nichtbeachtung von DSGVO und Aufbewahrungspflichten.

## 6. Gegenmaßnahmen und Klärungsbedarfe
- **Klares MVP-Scoping:** Bewusste und transparente Ausgrenzung riskanter Funktionen und offene Punkte, Dokumentation dieser Entscheidungen.
- **Security Review pragmatisch planen:** Fokus auf kritische Sicherheitsaspekte für MVP, um Verzögerungen zu minimieren.
- **Supportprozess sauber kommunizieren:** Klare Risikohinweise für Kundenservice und geplante Übergangslösung mit Kontaktformular.
- **Enges Controlling der SAP-Integration:** Monitoring der Schnittstellenverfügbarkeit und Notfallkonzepte bei Ausfall.
- **Datenschutzkonforme Testdaten und Umgebungen entwickeln:** Nutzung von Pseudonymisierung und synthetischen Testdaten verpflichtend machen.
- **Hosting- und Backup-Strategie klären:** Frühzeitige Auswahl eines DSGVO-konformen EU-Anbieters inklusive Kostenabschätzung.
- **Freigabeprozesse und Rabattlogik für spätere Phasen vorsehen:** MVP-Funktionalität begrenzen und klare Anforderungen für spätere Erweiterungen definieren.
- **Transparente Kommunikation aller Risiken an Management und Stakeholder:** Offene Risiken sichtbar halten zur Vermeidung falscher Erwartungen.

---

_Diese Risikoanalyse basiert auf dem umfangreichen Transkript input/transcripts/T9999_chaos.txt, der Kontextzusammenfassung runs/phase2_1/20260612_112129_92e6a3/state/context.md sowie den daraus abgeleiteten Requirements docs/requirements.md._