# Risiken für das Kundenportal‑MVP

## 1. Fachliche Risiken
- **Unklare Scope‑Entscheidungen** (Mobile‑First vs. Web‑First, SSO, Mehrwährung) können zu Nachträgen und Verzögerungen führen.\
- **Rabatt‑Freigabe‑Prozess fehlt** – Gefahr von fehlerhaften Preisen und finanziellen Verlusten (siehe Eva, Zeile 180‑187).\
- **Support‑Prozess nicht definiert** – kein Ticket‑System, potenziell lange Reaktionszeiten und Unzufriedenheit (David, Zeile 142‑144).\
- **KPIs nicht messbar** ohne klare Erfassungsstrategie, erschwert Erfolgskontrolle (Anna, Zeile 70‑73).

## 2. Technische Risiken
- **API‑Gateway‑Warteliste (6 Wochen)** verhindert zentrale Rate‑Limiting‑ und Auth‑Mechanismen und erhöht Angriffsfläche (Ben, Zeile 292‑295).\
- **SAP‑Verfügbarkeit als kritische Abhängigkeit** – Ausfall führt zu fehlenden Preis‑/Rabattdaten und blockiert Angebotsgenerierung (Ben, Zeile 292‑295).\
- **Managed Service / EU‑Only Hosting Kosten‑Unsicherheit** – Budget‑Überschreitung möglich (Farid, Zeile 260‑270).\
- **Keine neue Datenbank** – Nutzung von Managed DB‑Services kann Einschränkungen bei Skalierung oder Features mit sich bringen (Anna, Farid).\
- **Backup & Disaster Recovery** nicht vollständig spezifiziert (Farid, Zeile 260‑270); Wiederherstellungs‑SLA unbekannt.\
- **Testdaten enthalten reale Kundendaten** – Risiko von Datenschutzverstößen in CI/CD (Ben, Zeile 292‑295).\
- **Monitoring ohne personenbezogene Daten** – Fehlkonfiguration kann zu illegaler Datenverarbeitung führen (Clara, Zeile 9‑12).\
- **Secrets Management fehlt** – Gefahr von Credential‑Leak (Farid).\

## 3. Compliance‑ und Datenschutzrisiken
- **Double‑Opt‑In Umsetzung** unklar – Nicht‑Erfüllung der DSGVO‑Anforderung (Clara, Zeile 9‑12).\
- **Logging kann PII enthalten**; Trennung von Audit‑ und technischen Logs ist noch nicht gesichert (Clara).\
- **Datenresidenz**: Managed Service könnte Daten global replizieren, was DSGVO‑Konformität gefährdet (Farid).\
- **Löschkonzept vs. gesetzliche Aufbewahrung** – Konflikt zwischen Recht auf Vergessenwerden und Aufbewahrungspflichten (Clara, Eva).\
- **Support‑Kommunikation per E‑Mail** – Unstrukturierte personenbezogene Daten, potenziell unsicher (David).\
- **PDF‑Export von Angeboten/Rechnungen** muss Datenschutz‑Hinweise und ggf. Pseudonymisierung enthalten (Clara, Eva).

## 4. Widerspruche und Unsicherheiten
- **Mobile vs. Web‑First** – noch nicht entschieden, wirkt sich auf Architektur aus.\
- **SSO/Identity Provider** – optional, aber kein klarer MVP‑Plan.\
- **Budget für Managed Services & EU‑Only Hosting** – Kosten‑Unsicherheit bleibt offen.\
- **Security Review Dauer (6 Wochen)** kollidiert mit 8‑Wochen‑MVP‑Zeitplan.\
- **Mehrwährung & Internationalisierung** – nur EUR im MVP, spätere Währungen noch ungeklärt.

## 5. Mögliche Auswirkungen
- Projektverzögerungen und Kostenüberschreitungen.\
- Rechtliche Konsequenzen (DSGVO‑Verstöße, fehlende Audit‑Logs).\
- Finanzielle Verluste durch fehlerhafte Rabatt‑ bzw. Preis‑Anwendungen.\
- Verlust von Kundenzufriedenheit und Reputation durch unklaren Support‑Prozess.\
- Technische Instabilität bei SAP‑Ausfällen oder fehlender Rate‑Limiting‑Implementierung.

## 6. Gegenmaßnahmen / Klärungsbedarf
- **Scope‑Klärung**: Dokumentierte Entscheidung zu Mobile, SSO, Mehrwährung vor Development‑Start.\
- **Rabatt‑Freigabe‑Workflow** definieren und im MVP als technische Einschränkung festhalten.\
- **Support‑Konzept**: Minimal‑Ticket‑System oder gesichertes Kontaktformular inkl. Datenschutz‑Hinweise.\
- **Frühzeitiges Security Review** oder Mini‑Review für MVP (z. B. Threat‑Model‑Check).\
- **Backup‑ und DR‑Plan** detaillieren (Speicherort, Wiederherstellungs‑SLA).\
- **Testdaten‑Strategie**: Anonymisierung oder synthetische Daten für CI/CD.\
- **Logging‑Richtlinie** erstellen: technische Logs ohne PII, Audit‑Logs mit kontrolliertem Zugriff und Aufbewahrungsfrist.\
- **Hosting‑Provider‑Auswahl**: Vertragliche Nachweise für EU‑Only‑Datenresidenz einholen.\
- **Secrets Management** implementieren (z. B. Vault).\
- **Rate‑Limiting** temporär im Service implementieren, bis API‑Gateway verfügbar ist.\
- **Klarheit über Aufbewahrungspflichten** für Rechnungen vs. Löschanfragen definieren.

---
**Evidenz**: Kontext‑Datei `runs/phase2_1/20260602_105417_e37966/state/context.md`, Requirements‑Datei `docs/requirements.md` und das Transkript `input/transcripts/T9999_chaos.txt` (Zeilen‑Referenzen im Kontext‑Dokument).
