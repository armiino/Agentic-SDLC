# Risikenübersicht für das Kundenportal‑MVP

## 1. Fachliche Risiken
- **Unklare Rabatt‑Freigabeprozesse**: Die Schwelle für Rabatt‑Freigaben (15 % / 20 % / 30 %) und die zugehörigen Rollen sind noch nicht definiert. Gefahr von falscher Preisgestaltung oder Verzögerungen bei Angebotsfreigaben.
- **Pilot‑Kunde / Länderwahl (DE vs. CH)**: Ohne klare Entscheidung können Währungs‑ und Datenschutz‑Regeln nachträglich geändert werden, was zu Re‑Work und Verzögerungen führt.
- **Support‑Workflow**: Unklar, ob ein einfaches Kontakt‑Formular ausreicht oder ein Ticket‑System implementiert werden muss. Risiko von ineffizientem Kunden‑Support und erhöhten Eskalationszeiten.
- **Mehrwährungs‑ und Internationalisierungs‑Unterstützung**: Noch nicht final definiert (EUR, CHF, evtl. USD). Gefahr von falschen Rechnungen und rechtlichen Problemen bei grenzüberschreitenden Transaktionen.

## 2. Technische Risiken
- **Direkte SAP‑Leseschnittstelle ohne API‑Gateway**: Erhöht die Kopplung an SAP, reduziert zentrale Sicherheitskontrollen und kann bei SAP‑Ausfällen zu Ausfall des Portals führen.
- **Performance & Skalierbarkeit**: Erwartete Nutzerzahlen (200 – 20.000) erfordern robustes Caching, Pagination und Rate‑Limiting. Da konkrete Parameter (z. B. Schwellenwerte) noch offen sind, besteht das Risiko von Engpässen.
- **Backup‑ und Disaster‑Recovery‑Strategie**: Der Detailgrad (z. B. RTO ≤ 4 h) ist noch nicht festgelegt, dadurch Gefahr von Datenverlust oder langen Ausfallzeiten.
- **Kosten‑Unsicherheit EU‑only Managed Hosting**: Fehlende Budget‑Freigabe kann zu Kompromissen bei Infrastruktur‑Sicherheit oder zur Verletzung der EU‑Datenresidenz führen.
- **Rate‑Limiting / Missbrauchserkennung**: Noch nicht spezifiziert (z. B. 100 Anfragen/Minute). Unzureichende Begrenzung kann zu Denial‑of‑Service‑Angriffen führen.

## 3. Compliance‑ und Datenschutzrisiken
- **Datenminimierung vs. direkter SAP‑Zugriff**: Mehr Daten werden exponiert, als für das MVP zwingend nötig – möglicher Verstoß gegen das Prinzip der Datenminimierung (DSGVO Art. 5).
- **Double‑Opt‑In Umsetzung**: Wenn nicht korrekt implementiert, besteht das Risiko von DSGVO‑Verstößen und Bußgeldern.
- **Retention‑ und Lösch‑Regeln**: Noch nicht definiert, wodurch Aufbewahrungspflichten oder das Recht auf Vergessenwerden (Art. 17 DSGVO) verletzt werden könnten.
- **Datenresidenz**: Unklare Hosting‑Entscheidung könnte dazu führen, dass Daten außerhalb der EU gespeichert werden – Verstoß gegen C1.

## 4. Widersprüche und Unsicherheiten
- **Security Review (6 Wochen) vs. 8‑Wochen‑MVP‑Deadline** – Risiko, dass das MVP ohne vollständige Sicherheitsprüfung veröffentlicht wird.
- **API‑Gateway Verfügbarkeit** – 6‑Wochen‑Wartezeit verhindert Nutzung, alternativ wird direkte SAP‑Anbindung gefordert.
- **Kosten‑Schätzung für EU‑Only Hosting** – Noch nicht quantifiziert, könnte Budget‑Überschreitung verursachen.
- **Rabatt‑Freigabeprozess** – Noch nicht festgelegt, kann zu inkonsistenter Preisgestaltung führen.
- **Backup‑ und DR‑Detailgrad** – Unklar, Risiko von Datenverlust.
- **Support‑Prozess** – Unklar, ob ein Ticket‑System nötig ist.
- **Retention‑ und Lösch‑Regeln** – Nicht definiert, rechtliche Risiken.

## 5. Mögliche Auswirkungen
- **Projektverzögerungen** aufgrund nachträglicher Änderungen (z. B. Währung, Hosting, Sicherheitsreview).
- **Rechtliche Konsequenzen** bei DSGVO‑Verstößen (Bußgelder, Schadenersatz).
- **Kundenzufriedenheit** leidet durch Performance‑Probleme, unklare Rabatt‑ bzw. Support‑Prozesse oder fehlerhafte Rechnungen.
- **Finanzielle Mehrbelastungen** durch unerwartete Hosting‑Kosten oder notwendige Nachbesserungen.
- **Systemausfälle** bei SAP‑Ausfall oder unzureichendem Rate‑Limiting.

## 6. Gegenmaßnahmen / Klärungsbedarfe
- **Frühzeitige Definition des Rabatt‑Freigabeprozesses** inkl. Schwellenwerten und Rollen; Dokumentation im Requirements‑Artefakt.
- **Entscheidung für Pilot‑Kunden und Länder** bis spätestens Woche 2, um Währungs‑ und Datenschutz‑Implikationen zu fixieren.
- **Temporärer Security‑Review‑Plan**: Priorisierte Sicherheits‑Checks (z. B. Pen‑Test, Code‑Review) für MVP‑Release, mit nachträglicher Vollprüfung.
- **Implementierung eines leichten API‑Proxies** (z. B. Service‑Mesh) als Übergangslösung, um zentrale Sicherheits‑Controls zu ermöglichen.
- **Provisorisches Rate‑Limiting** etablieren (z. B. 100 Req/Min) und Monitoring‑Alarme für Missbrauch einrichten.
- **Ausarbeitung einer Backup‑ und DR‑Strategie** mit klaren RPO/RTO‑Zielen für das MVP und Review mit IT‑Operations.
- **Kosten‑Freigabe für EU‑Only Managed Services** einholen; Alternativ Optionen prüfen und Risikoanalyse dokumentieren.
- **DPIA (Data‑Protection‑Impact‑Assessment)** durchführen für den SAP‑Datenzugriff, um Datenminimierung nachzuweisen.
- **Retention‑ und Lösch‑Policy** definieren (z. B. Angebote 12 Monate, Rechnungen 7 Jahre, Log‑Daten 6 Monate) und automatisierte Lösch‑Jobs implementieren.
- **Support‑Workflow-Entscheidung** treffen und ggf. ein leichtes Ticket‑System (z. B. Jira Service Management) einführen.

---
**Evidence**: Aussagen aus Projektkontext (Projekt‑Ziel, Konflikte, Unsicherheiten) und `docs/requirements.md`.
**Reason**: Erstellung eines initialen Risikodokuments für das Kundenportal‑MVP basierend auf vorhandenen Artefakten.
