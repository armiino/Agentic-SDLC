# Risiken für das Kundenportal‑MVP

## 1. Fachliche Risiken
- **Unklare Zieldefinition / Scope‑Creep**: Viele Stakeholder‑Wünsche (Mobile‑App, Support‑Ticket‑System, Rabatt‑Freigabe, Mehrwährung) wurden nicht eindeutig priorisiert. Gefahr von nachträglichen Funktions‑Ergänzungen, die das 8‑Wochen‑Ziel gefährden.
- **Support‑Prozess**: Nur ein Kontakt‑Formular ohne strukturiertes Ticket‑System. Leads zu unübersichtlichen E‑Mails → erhöhtes Fehlerrisiko, Datenschutz‑Probleme und schlechter Kundenservice.
- **Rabatt‑Freigabe**: Im MVP keine Sonderrabatte > 15 %. Fehlende Klarheit über künftige Freigabe‑Workflows kann zu finanziellen Fehlbuchungen führen.
- **Mehrwährung / Internationalisierung**: Pilot‑Kunde aus der Schweiz (CHF) und zukünftige USA‑Expansion wurden nur vage erwähnt. Fehlende Entscheidungen können zu rechtlichen und steuerlichen Problemen.
- **KPI‑Tracking**: Conversion‑Rate und Zeit‑bis‑Angebot werden gemessen, aber das Tracking‑Framework fehlt. Datenqualität und Interpretation unsicher.
- **SAP‑Verfügbarkeit**: Das Portal ist stark von SAP‑Lesedaten abhängig. Bei SAP‑Ausfall können keine Angebote erstellt werden → Geschäftsunterbrechung.
- **Daten‑Minimierung**: Unklar, welche Kundendaten im Portal gespeichert werden müssen. Gefahr von überflüssiger Datenhaltung und damit erhöhtem DSGVO‑Risiko.

## 2. Technische Risiken
- **Zeitplan vs. Infrastruktur‑Abhängigkeiten**: API‑Gateway‑Team hat 6‑Wochen‑Warteliste, EU‑Only Managed Service muss noch ausgewählt werden. Beide können das 8‑Wochen‑MVP leicht überschreiten.
- **Security Review**: Kein vollständiges Security Review im MVP (nur minimaler Review). Erhöhtes Risiko von Schwachstellen (z. B. fehlende OWASP‑Maßnahmen, unsichere Authentifizierung).
- **Audit‑Log & Logging**: Minimaler Audit‑Trail implementiert, aber technische Logs dürfen keine personenbezogenen Daten enthalten. Fehlende Trennung kann zu DSGVO‑Verstößen.
- **Backup & Disaster Recovery**: Nur tägliches Backup geplant, kein detailliertes DR‑Szenario. Datenverlust bei Ausfall möglich.
- **Rate‑Limiting / Missbrauchserkennung**: Einfaches Rate‑Limiting im MVP, aber keine umfassende Abuse‑Detection. Potenzial für DoS‑Angriffe oder unautorisiertes Daten‑Scraping.
- **Secrets Management**: Noch nicht definiert, Gefahr von Credential‑Lecks in Code‑Repos.
- **Test‑Umgebung**: SAP‑Testdaten enthalten ggf. reale Kundendaten. Ohne Pseudonymisierung Gefahr von Datenleckage während Entwicklung.
- **Performance / Skalierbarkeit**: Erwartete Nutzerzahl schwankt stark (200–20 000). Ohne Skalierbarkeits‑Design (Load‑Balancing, Auto‑Scaling) kann das System bei Erfolg überlastet werden.
- **PDF‑Export & Dokumenten‑Versionierung**: Noch nicht spezifiziert, Gefahr von inkonsistenten rechtlichen Dokumenten.

## 3. Compliance‑/Datenschutz‑Risiken
- **DSGVO – Double‑Opt‑In & Löschkonzept**: Double‑Opt‑In ist vorgesehen, aber ein vollständiges Lösch‑ und Auftragsverarbeitungs‑Konzept fehlt. Gefahr von Verstößen gegen Art. 17 (Recht auf Vergessenwerden) und Art. 28 (AV‑Vertrag).
- **Datenresidenz**: EU‑Only Hosting muss nachweislich erfolgen; Standard‑Provider repliziert Backups global. Ohne vertragliche Guarantees kann es zu Rechtsverstößen kommen.
- **Audit‑ und Zugriffskontrolle**: Rollen‑ und Berechtigungskonzept ist nur minimal definiert. Fehlende feinkörnige Zugriffskontrollen können zu unautorisiertem Datenzugriff führen.
- **Kunden‑Daten in Support‑E‑Mails**: Support‑Anfragen per E‑Mail können personenbezogene Daten enthalten, die nicht im Löschkonzept erfasst sind.
- **Mehrwährungs‑ und Länder‑Spezifische Datenschutz‑Bestimmungen**: Schweiz (PDPA) und USA haben abweichende Vorgaben. Ohne klare Trennung entstehen regulatorische Risiken.
- **Retention vs. Recht auf Löschung**: Handelsrechtliche Aufbewahrungspflichten kollidieren mit DSGVO‑Löschanforderungen – fehlende klare Policy kann zu Verstößen führen.

## 4. Widersprüche & Unsicherheiten (als Risiko‑Quelle)
- **Mobile‑First vs. Web‑First**: Keine klare Entscheidung – kann zu Verzögerungen und zusätzlichem Entwicklungsaufwand führen.
- **SSO (Azure AD/Google) vs. einfacher E‑Mail‑Login**: Unterschiedliche Erwartungen der Stakeholder; Integration ohne klare Vorgabe kann zu Inkonsistenzen führen.
- **Kosten‑Schätzung vs. fehlende Architektur‑Entscheidung**: Vorstand verlangt Kostenschätzung, aber fehlende Entscheidungen (Provider, DB‑Service, API‑Gateway) erzeugen Unsicherheit über Budget.
- **Support‑Erwartungen vs. MVP‑Ausgestaltung**: Support verlangt Ticket‑System, MVP plant nur Kontakt‑Formular – führt zu operativem Risiko.
- **Rabatt‑Freigabe vs. MVP‑Einschränkung**: Finanz muss Rabatte prüfen, MVP lässt keine Sonderrabatte zu – möglicher Konflikt bei Vertrieb.
- **SAP‑Lese‑Only vs. notwendige Schreib‑Zugriffe (Bestellungen)**: Ohne Schreib‑Zugriff können keine Aufträge zurück in SAP, was das End‑to‑End‑Geschäfts‑Modell einschränkt.

## 5. Mögliche Auswirkungen
| Risiko | Mögliche Folge(n) | Auswirkung (H/M/L) |
|--------|-------------------|--------------------|
| Zeit‑Risiko (Infrastruktur‑Wartezeiten) | Projektverzug, MVP‑Launch verpasst | H |
| Compliance‑Lücken (Kein Löschkonzept) | Bußgelder, Reputationsschaden | H |
| Security‑Mängel (Kein Review) | Datenverlust, Angriff, Kundendaten-Leak | H |
| SAP‑Ausfall | Keine Angebotserstellung, Umsatzverlust | M |
| Support‑Prozess‑Defizit | Unzufriedene Kunden, erhöhter Aufwand | M |
| Fehlende Rabatt‑Freigabe | Vertriebs‑Frust, verlorene Deals | M |
| Unklare Kosten | Budget‑Überschreitung, Finanz‑Risiko | M |
| Skalierbarkeits‑Mangel | System‑Ausfall bei hoher Nutzerzahl | M |
| Daten‑Residency‑Verletzung | Rechtsstreit, Vertragsstrafen | H |

## 6. Gegenmaßnahmen / Klärungsbedarfe
- **Zeitplan‑Management**: Frühzeitige Entscheidung für Managed Service Provider und ggf. Alternativ‑API‑Gateway (z. B. direkte API‑Proxy‑Lösung). Buffer von +2 Wochen im Projektplan einplanen.
- **DSGVO‑Check**: Sofortiges Erarbeiten eines Lösch‑ und Auftragsverarbeitungs‑Konzepts; Datenschutz‑Beauftragter einbinden.
- **Security Review**: Mini‑Security‑Audit (Threat‑Modelling, OWASP‑Checkliste) noch vor MVP‑Go‑Live, externe Pen‑Test‑Option prüfen.
- **SAP‑Fallback**: Cache‑Strategie für Produkt‑/Preis‑Stammdaten (ohne Kundendaten) definieren; klare SLA‑Vereinbarung mit SAP‑Team.
- **Support‑Prozess**: Minimal‑Ticket‑Tracking via externes SaaS‑Tool (z. B. Freshdesk) mit DSGVO‑Konformität, bis eigenes System gebaut wird.
- **Rabatt‑Policy**: Dokumentierte Schwellenwerte (≤ 15 % ohne Freigabe) im MVP festlegen, später Freigabe‑Workflow definieren.
- **Kosten‑Kalkulation**: Schnell‑Analyse für EU‑Only Managed Service (Kosten‑Pro‑Monat, Daten‑Transfer, Backup) und für Event‑uales API‑Gateway.
- **Secrets Management**: Einführung von Vault‑Lösung (z. B. HashiCorp Vault) oder Cloud‑Native Secrets Store vor Code‑Deployment.
- **Test‑Daten‑Policy**: Anonymisierung von SAP‑Testdaten, Nutzung von synthetischen Datensätzen für CI/CD.
- **Rate‑Limiting**: Implementierung von Basis‑Rate‑Limits (z. B. 100 Requests/min pro IP) bereits im MVP, Monitoring‑Alert einrichten.
- **Retention‑Policy**: Grund‑Retention (z. B. 7 Jahre für Rechnungen) definieren, Ausnahmen für Löschanfragen dokumentieren.
- **Internationalisierung**: Entscheidung für Sprachen und Währungen im Scope‑Kick‑off; ggf. separate Feature‑Flag für Schweiz.
- **Audit‑Log‑Erweiterung**: Log‑Schema planen, das Nutzer‑ID, Aktion, Timestamp und Quelle erfasst, aber keine PII im technischen Log.

---
*Dieses Risiko‑Artefakt wurde aus dem Projekt‑Kontext (runs/phase2_1/20260529_102448_80a382/state/context.md) und dem vollständigen Stakeholder‑Transkript (input/transcripts/T9999_chaos.txt) abgeleitet.*