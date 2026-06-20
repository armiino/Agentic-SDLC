# Risiken für das Kundenportal (MVP)

## 1. Fachliche Risiken
- **Unklare Pilotkunden‑Region**: Unklar, ob der Pilotkunde in Deutschland (EUR) oder Schweiz (CHF) sitzt → kann zu falscher Währungs‑ und Hosting‑Auswahl führen.
- **Rabatt‑Freigabelogik nicht definiert**: Fehlende Schwellenwerte (z. B. >15 % Rabatt) können zu fehlerhaften Freigaben oder Vertriebs‑Risiken führen.
- **Support‑Prozess ohne Ticket‑System**: Das reine Kontaktformular kann zu unübersichtlichen Anfragen und schlechter Service‑Qualität führen.
- **KPIs und Reporting offen**: Ohne klare KPI‑Definition ist Erfolgsmessung erschwert, was das Stakeholder‑Management gefährdet.
- **Mehrsprachigkeit / Mehrwährung**: Englisch‑UI ist geplant, CHF‑Preisanzeige unklar – kann zu Missverständnissen bei internationalen Nutzern führen.

## 2. Technische Risiken
- **API‑Gateway‑Warteliste (6 Wochen)**: Der geplante Gateway steht erst nach 6 Wochen zur Verfügung – das MVP muss mit einem Ersatz (z. B. Reverse‑Proxy) starten, was zu erhöhtem Entwicklungsaufwand und Sicherheitslücken führen kann.
- **Security‑Review >6 Wochen**: Der Review überschneidet sich mit der 8‑Wochen‑Deadline – unvollständige Reviews können zu ungeprüften Schwachstellen führen.
- **Keine neue Datenbank**: Nutzung eines bestehenden Managed‑DB‑Service kann zu Leistungs‑ und Skalierbarkeitsproblemen führen, wenn das Datenvolumen wächst.
- **Rate‑Limiting & Missbrauchserkennung**: Nur ein Basis‑Rate‑Limiting ist geplant; fehlende erweiterte Missbrauchserkennung erhöht das Risiko von DDoS‑Angriffen.
- **Backup & DR Parameter unsicher**: RPO 1 h / RTO 4 h sind Annahmen, nicht bestätigt – kann zu Datenverlust oder langen Ausfallzeiten führen.

## 3. Compliance‑ und Datenschutzrisiken
- **DSGVO‑Konformität (Double‑Opt‑In, Löschkonzept)**: Offene Fragen zu Aufbewahrungsfristen und Recht auf Vergessenwerden → Risiko von Verstößen gegen DSGVO.
- **EU‑Only Hosting**: Unklare Hosting‑Region (Pilotkunde Schweiz) kann gegen EU‑Only‑Anforderung verstoßen.
- **Audit‑Trail & unveränderbare Logs**: Implementierung muss sicherstellen, dass Logs nicht manipuliert werden – technische Umsetzung unsicher.
- **Technisches Monitoring ohne PII**: Fehlende klare Trennung zwischen technischer und personenbezogener Überwachung kann Datenschutzverletzungen verursachen.
- **Datenminimierung im Support‑Formular**: Unklare Datenerhebung im Kontaktformular kann überflüssige personenbezogene Daten sammeln.

## 4. Widersprüche und Unsicherheiten
- **Zeitplan vs. Umfang**: 8‑Wochen‑Deadline kollidiert mit umfangreichen Compliance‑ und Sicherheitsanforderungen.
- **Budget vs. Managed Services**: Kosten für EU‑Only‑Hosting und Security‑Review sind noch nicht geschätzt – kann zu Budgetüberschreitungen führen.
- **SAP‑Zugriff ohne Cache**: Kritische Abhängigkeit vom SAP‑System ohne Fallback‑Cache erhöht Ausfallrisiko.
- **Retention‑Fristen unklar**: Fehlende Vorgaben für Aufbewahrungsdauer von Angeboten, Rechnungen und Logs.
- **Testing mit echten SAP‑Daten**: Unsicherheit, wie synthetische Testdaten sicher verwendet werden können.

## 5. Mögliche Auswirkungen
- Projektverzögerungen & verpasste MVP‑Release‑Frist.
- Rechtsfolgen bei DSGVO‑Verstößen (Bußgelder, Reputationsschaden).
- Kundenunzufriedenheit durch fehlerhafte Rabatt‑Freigaben, fehlende Support‑Nachverfolgung oder falsche Währungsdarstellung.
- Finanzielle Mehrkosten durch Nachbesserungen, zusätzliche Sicherheits‑ oder Infrastruktur‑Maßnahmen.
- Verlust von Daten bei unzureichenden Backup‑/DR‑Strategien.

## 6. Mögliche Gegenmaßnahmen / Klärungsbedarfe
- **Pilotkunde‑Klärung**: Frühzeitige Entscheidung über Region, Währung und Hosting‑Standort.
- **Rabatt‑Freigabelogik definieren**: Klare Schwellenwerte und Rollen für Freigaben festlegen.
- **Alternative API‑Gateway‑Lösung**: Schnell umsetzbare Proxy‑ oder Service‑Mesh‑Option evaluieren und sichern.
- **Security‑Review beschleunigen**: Parallel zum Entwicklungs‑Sprint Early‑Security‑Checks, Threat‑Modelling und Pen‑Test‑Planung.
- **Backup‑ und DR‑SLA finalisieren**: Vorgaben prüfen und entsprechende Service‑Level vertraglich festhalten.
- **Compliance‑Checkliste**: Detaillierte Prüfpunkte für DSGVO, Audit‑Trail, Logging und Datenlöschung erarbeiten.
- **Support‑Prozess spezifizieren**: Datenflüsse, Aufbewahrung und Löschfristen für das Kontaktformular definieren.
- **Kosten‑Schätzung einholen**: Bis Freitag konkrete Zahlen für Managed Services, Security‑Review und eventuell zusätzliche Infrastruktur sammeln.
- **Retention‑Policy definieren**: Gesetzliche Vorgaben (z. B. 10 Jahre für Rechnungen) übernehmen und technisch umsetzen.
- **Testdaten‑Strategie**: Nutzung von synthetischen SAP‑Daten in isolierten Testumgebungen, ggf. Maskierung echter Daten.
- **Monitoring‑Design**: Separate Pipelines für technisches Monitoring und Audit‑Logs, PII‑Maskierung sicherstellen.
- **Risikopriorisierung**: Kritische Risiken (Security Review, API‑Gateway, DSGVO) hoch priorisieren, mitigierende Maßnahmen früh einplanen.

---
*Dieses Risiko‑Artefakt wurde aus dem bereitgestellten Projekt‑Kontext, den fachlichen und nicht‑funktionalen Requirements sowie den erkannten Konflikten und offenen Fragen abgeleitet.*