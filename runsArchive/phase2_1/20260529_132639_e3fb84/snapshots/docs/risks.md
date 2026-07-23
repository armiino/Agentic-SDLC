## Risiken für das Kundenportal‑MVP

### Fachliche Risiken
| ID | Risiko | Ursache / Konflikt | Mögliche Auswirkung | Priorität* |
|----|--------|---------------------|----------------------|-----------|
| FR‑01 | **Unklare Preis‑/Rabatt‑Logik** | Sonderrabatte sind im MVP nicht erlaubt, aber Sales erwartet flexible Rabatte. | Falsche Angebote → Umsatz‑ und Reputationsverlust, Nachbearbeitung im Support. | Hoch |
| FR‑02 | **Support‑Prozess nicht definiert** | Kein Ticket‑System, nur Kontaktformular. | Manuelle Bearbeitung, Verzögerungen, DSGVO‑Verletzung bei E‑Mail‑Speicherung. | Mittel |
| FR‑03 | **Mehrsprachigkeit & Internationalisierung** | MVP nur Deutsch, jedoch bereits Interesse an englischen Kunden. | Fehlende Markt‑Erschließung, zusätzliche Aufwand für Nach‑Nachbesserung. | Niedrig |
| FR‑04 | **Unvollständige SAP‑Stammdaten** | SAP enthält lückenhafte Produkt‑/Preis‑Informationen. | Angebote können unvollständig oder falsch sein → Lost Deals. | Mittel |
| FR‑05 | **Fehlende Freigabe‑Workflows für Rabatte** | Rabatt‑Freigabe erst in Phase 2 geplant. | Gefahr von nicht‑autorisierten Preisnachlässen. | Hoch |
| FR‑06 | **Kosten‑Unsicherheit EU‑Only Hosting** | Hosting‑Kosten noch nicht geschätzt, EU‑Only kann teurer sein. | Budget‑Überschreitung, Projekt‑Stop oder Scope‑Reduktion. | Mittel |

### Technische Risiken
| ID | Risiko | Ursache / Konflikt | Mögliche Auswirkung | Priorität* |
|----|--------|---------------------|----------------------|-----------|
| TR‑01 | **SAP‑Verfügbarkeit** | MVP hängt von SAP‑Lesezugriff ab; kein Fallback. | Angebotserstellung blockiert → SLA‑Verletzung. | Hoch |
| TR‑02 | **Fehlendes API‑Gateway** | 6‑Wochen‑Warteliste, direkter API‑Expose geplant. | Sicherheits‑ und Skalierbarkeits‑Probleme, späterer Migrationsaufwand. | Hoch |
| TR‑03 | **Technische Logs enthalten personenbezogene Daten** | Logging‑Anforderungen wurden nicht klar getrennt. | DSGVO‑Verstoß, Bußgelder. | Hoch |
| TR‑04 | **Backup / Disaster Recovery unklar** | Backup‑Strategie nur grob definiert. | Datenverlust bei Ausfall, rechtliche Folgen. | Mittel |
| TR‑05 | **Rate‑Limiting / Missbrauch** | Kein fertiges Rate‑Limiting, nur geplante Limits. | Service‑DoS, Datenexfiltration. | Mittel |
| TR‑06 | **Keine neue Datenbank, aber persistente Daten nötig** | Nutzung von Managed‑DB‑Service ohne klare Auswahl. | Leistungs‑Engpässe, Kosten‑Unsicherheit. | Mittel |
| TR‑07 | **Umgebung & Testdaten** | SAP‑Testsystem enthält reale Kundendaten. | Datenschutz‑Risiko bei Entwicklung/Tests. | Hoch |
| TR‑08 | **Performance‑Skalierung** | Erwartete Nutzerzahl stark variabel (200‑20 000). | Ressourcen‑Engpässe nach Release. | Mittel |

### Compliance‑ / Datenschutz‑Risiken
| ID | Risiko | Ursache / Konflikt | Mögliche Auswirkung | Priorität* |
|----|--------|---------------------|----------------------|-----------|
| CR‑01 | **DSGVO‑Konformität unvollständig** | Double‑Opt‑In, Lösch‑Mechanismen nur minimal definiert. | Bußgelder, Image‑Schaden. | Hoch |
| CR‑02 | **Datenresidenz** | EU‑Only Hosting noch nicht bestätigt. | Verstöße gegen EU‑Datenschutz, rechtliche Folgen. | Hoch |
| CR‑03 | **Lösch‑ und Auskunfts‑Anfragen** | Kein automatisiertes Löschkonzept, nur manueller Prozess. | Verzögerungen, mögliche Rechtsverstöße. | Mittel |
| CR‑04 | **Audit‑Trail vs. personenbezogene Daten** | Gefahr, dass Audit‑Logs personenbezogene Details enthalten. | DSGVO‑Verstoß. | Hoch |
| CR‑05 | **Support‑E‑Mails mit Kundendaten** | Kontaktformular leitet Anfragen per E‑Mail weiter. | Unverschlüsselte personenbezogene Daten, Risiko Datenleck. | Mittel |

### Widersprüche und Unsicherheiten
| ID | Widerspruch / Unsicherheit | Relevanz für Risiko |
|----|-----------------------------|----------------------|
| W‑01 | **Mobile‑App vs. Web‑First** – Diskussion über Priorität, aber MVP nur Web. | Risiko von Fehlannahmen im Scope, späterer Mehraufwand. |
| W‑02 | **SSO optional vs. geplante Nutzung** – Keine SSO im MVP, aber Sales wünscht Azure AD/Google. | Risiko von Nachrüstarbeiten, Integrationskomplexität. |
| W‑03 | **Mehrwährungs‑Support** – Diskussion über CHF, USD, später. | Keine Auswirkungen im MVP, aber späteres Re‑Engineering. |
| W‑04 | **Kosten‑Schätzung für Hosting** – Noch offen. | Budget‑Risiko, mögliche Scope‑Reduktion. |
| W‑05 | **Retention‑Plan Details** – Minimal definiert, weitere rechtliche Auflagen offen. | Risiko von Nicht‑Einhaltung gesetzlicher Aufbewahrungspflichten. |
| W‑06 | **API‑Gateway Migration** – Warteliste vs. 8‑Wochen‑Zeitplan. | Sicherheits‑ und Skalierbarkeits‑Risiko. |

*Priorität: Hoch, Mittel, Niedrig – basierend auf Impact‑ und Likelihood‑Bewertung aus den Gesprächen.*

### Mögliche Gegenmaßnahmen / Klärungsbedarfe
- **Preis‑/Rabatt‑Klärung**: Frühzeitige Abstimmung mit Finance, Definition eines Minimal‑Rabatt‑Sets für MVP.
- **Support‑Prozess**: Entscheidung für ein leichtgewichtiges Ticket‑Tool oder DSGVO‑konforme E‑Mail‑Verschlüsselung.
- **SAP‑Verfügbarkeit**: Fallback‑Strategie (z. B. Cache mit begrenzter Gültigkeit) und Monitoring‑Alarme.
- **API‑Gateway**: Temporärer OAuth‑Proxy, Evaluation von schnellen Managed‑API‑Gateways.
- **Logging‑Policy**: Trennung von Audit‑Logs (personenbezogene Daten) und technischen Logs, Maskierung sensibler Felder.
- **Backup‑Plan**: Detaillierte RPO/RTO‑Definition, Test‑Wiederherstellung vor Go‑Live.
- **Rate‑Limiting**: Implementierung von Throttling auf API‑Level, ggf. CDN‑basiert.
- **Testdaten‑Management**: Nutzung synthetischer Daten, Anonymisierung von SAP‑Testdaten, Zustimmung des Datenschutz‑Beauftragten.
- **Kosten‑Schätzung**: Schnell‑Analyse von EU‑Only Managed‑Service‑Anbietern, Budget‑Freigabe bis Freitag.
- **Retention & Löschkonzept**: Minimal‑Retention‑Plan fertigstellen, rechtliche Abstimmung für Ausnahmen (z. B. Rechnungen 7 Jahre).
- **Dokumentation**: Klarer Scope‑ und Risiko‑Log für MVP, inkl. offener Punkte für Phase 2.

---
*Alle Risiken, Widersprüche und Unsicherheiten basieren ausschließlich auf dem bereitgestellten Transkript `input/transcripts/T9999_chaos.txt` und den daraus abgeleiteten Requirements in `docs/requirements.md`.*