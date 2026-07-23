# Risikoartefakt – Kundenportal (MVP)

## 1. Fachliche Risiken
- **Unkontrollierte Rabatte**: Der im MVP ausgeschlossene Freigabe‑Workflow für Rabatte > 15 % kann zu unerwünschten Preisnachlässen führen (vgl. Open 4).
- **Support‑Datenschutz**: Das einfache Kontaktformular könnte personenbezogene Daten erfassen, ohne ausreichende DSGVO‑Konformität (Open 5).
- **Mehrwährungs‑Unsicherheit**: Noch offene Entscheidung, ob CHF unterstützt wird, kann zu Fehlinterpretationen bei Schweizer Pilot‑Kunden führen (Open 6).

## 2. Technische Risiken
- **Fehlender API‑Gateway**: Ohne Gateway fehlen zentrale Sicherheits‑ und Skalierbarkeits‑Mechanismen (Rate‑Limiting, Auth‑Proxy); ein leichter Proxy ist nur als Notlösung geplant (Open 3).
- **Datenbank‑Budget**: Unklare Auswahl einer EU‑Only Managed‑DB kann zu Performance‑ oder Kosten‑Problemen führen (Open 2).
- **Testdaten‑Verwendung**: Nutzung realer Kundendaten im SAP‑Testsystem birgt ein Datenschutz‑Risiko (Open 9).

## 3. Compliance‑ oder Datenschutzrisiken
- **Security‑Review‑Zeitplan**: Der 8‑Wochen‑Zeitplan kollidiert mit einem erwarteten 6‑Wochen‑Security‑Review, wodurch ein minimaler Sicherheits‑Check möglicherweise nicht ausreichend ist (Open 1).
- **Retention‑Policy vs. Löschrecht**: Fehlende Definition von Aufbewahrungs‑ und Löschregeln steht im Widerspruch zu DSGVO‑Anforderungen (Open 8).
- **Audit‑Log‑Daten**: Das Minimal‑Audit‑Log darf keine personenbezogenen Daten enthalten; Gefahr unbeabsichtigter Aufnahme sensibler Infos.

## 4. Widersprüche und Unsicherheiten
- **Zeitplan vs. Sicherheit**: MVP‑Deadline (8 Wochen) vs. notwendiger Security‑Review (6 Wochen).
- **Budget vs. EU‑Only Hosting**: Kosten‑Schätzung fehlt, könnte Infrastruktur‑Entscheidungen beeinflussen.
- **Pilot‑Kunde und Währung**: Unklar, ob CHF unterstützt wird; wirkt sich auf Internationalisierung und Compliance aus.
- **Backup‑Aufwand**: Aufwand für Backup & Disaster Recovery ist nicht im aktuellen Aufwandspuffer enthalten.

## 5. Mögliche Auswirkungen
- Finanzielle Verluste durch ungenehmigte Rabatte.
- Rechts‑ und Reputationsrisiken bei DSGVO‑Verstößen (Support‑Formular, Testdaten, fehlende Löschkonzepte).
- Projektverzögerungen oder Kostenüberschreitungen durch fehlende Infrastruktur‑Entscheidungen.
- Systeminstabilität oder Sicherheitslücken ohne API‑Gateway und vollständigen Security‑Check.

## 6. Mögliche Gegenmaßnahmen / Klärungsbedarfe
- **Rabatt‑Freigabe**: Auch im MVP einen einfachen Schwellen‑Check (> 15 %) implementieren oder einen manuellen Prüfprozess festlegen.
- **Support‑Datenschutz**: Datenschutz‑Check für das Kontaktformular, ggf. Anonymisierung oder Hinweis auf Datenverarbeitung.
- **API‑Gateway‑Ersatz**: Evaluation eines leichten Reverse‑Proxy (z. B. Nginx) mit Grund‑Rate‑Limiting.
- **DB‑Auswahl**: Kosten‑Analyse für EU‑Only Managed‑DB‑Services (z. B. Azure PostgreSQL EU‑Region, AWS RDS EU).
- **Security‑Review‑Plan**: Minimal‑Security‑Check definieren (Threat‑Model, Pen‑Test) und Termin frühzeitig einplanen.
- **Retention‑Policy**: Klare Aufbewahrungsfristen definieren, Löschprozess ins System integrieren.
- **Testdaten‑Strategie**: Verwendung pseudonymisierter oder synthetischer Daten in Testumgebungen sicherstellen.
- **Backup‑Aufwand**: Aufwandsschätzung für tägliche Backups und Wiederherstellungstests durchführen.
- **Hosting‑Kosten**: Kostenschätzung für EU‑Only Managed Hosting erstellen und dem Management präsentieren.

*Dieses Risiko‑Artefakt basiert auf dem bereitgestellten Projektkontext und den Functional/Non‑functional Requirements.*