## Risiken und Unsicherheiten – MVP Kundenportal

### Fachliche Risiken
| Risiko | Beschreibung | Mögliche Auswirkung | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|----------------------|--------------------------------|
| **Unklare Rabatt‑Freigabe** | Keine Sonderrabatte > Standard‑Satz im MVP, aber Business‑Stakeholder erwarten später Freigabe‑Workflows (15 % / 30 % Schwellen). | Fehlende Preis‑Flexibilität, Umsatz‑Einbußen, Compliance‑Risiko bei nachträglicher Preisänderung. | Frühzeitige Definition der Freigabe‑Logik, prototypische Implementierung in Phase 2, klare Dokumentation der Ausnahme im MVP. |
| **Support‑Prozess nicht automatisiert** | Nur Kontaktformular & E‑Mail, keine Ticket‑Datenbank. | Verzögerte Bearbeitung, unübersichtliche Nachverfolgung, DSGVO‑Risiko (personenbezogene Daten in E‑Mails). | Konzept für Ticket‑System entwickeln, zumindest strukturierte E‑Mail‑Archivierung mit PII‑Schutz. |
| **SAP‑Verfügbarkeit als kritische Abhängigkeit** | MVP nutzt lesenden SAP‑API‑Layer ohne Cache oder Fallback. | Angebots‑Erstellung nicht möglich bei SAP‑Ausfall → Service‑Unterbrechung. | Risiko‑Register SAP‑Ausfall, Monitoring & Alerting, ggf. Cache‑Strategie in Phase 2. |
| **Mehr‑Währungs‑ und Internationalisierungs‑Ausklammerung** | CHF / USD und weitere Länder/Sprachen nicht im MVP enthalten. | Pilot‑Kunde aus Schweiz kann ggf. nicht bedient werden → Vertragsverlust. | Entscheidung über Pilot‑Kunden früh klären, ggf. minimale CHF‑Unterstützung im MVP einplanen. |
| **Kosten‑Schätzung für EU‑Only Hosting fehlt** | Hosting‑Kosten noch nicht konkretisiert. | Budget‑Überschreitung, Verzögerungen bei Provider‑Auswahl. | Schnellstmögliche Kosten‑Ermittlung, ggf. konservative Budget‑Puffer einplanen. |

### Technische Risiken
| Risiko | Beschreibung | Mögliche Auswirkung | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|----------------------|--------------------------------|
| **Fehlendes API‑Gateway** (6‑Wochen‑Warteliste) | MVP verwendet einfachen internen Proxy statt zentralem Gateway. | Sicherheits‑Lücken (fehlende Rate‑Limiting, Auth‑Zentralisierung), Skalierbarkeits‑Probleme. | Interim‑Proxy mit Grund‑Auth & Rate‑Limiting implementieren, Migration zu Gateway in Phase 2 planen. |
| **Unzureichendes Logging / Audit‑Trennung** | Gefahr, dass PII in technischen Logs landen. | DSGVO‑Verstoß, Reputations‑Risiko. | Strikte Logging‑Richtlinien, Trennung von Audit‑Logs (keine PII) und Application‑Logs, automatisierte Prüfungen. |
| **Backup & Disaster Recovery noch nicht definiert** | Backup‑Strategie nur als tägliche Snapshots erwähnt. | Datenverlust bei Ausfall, fehlende Wiederherstellungstests. | Auswahl eines Managed‑Backup‑Providers, Test‑Restore‑Prozesse etablieren. |
| **Secrets‑Management nicht implementiert** | API‑Keys, DB‑Credentials liegen ggf. im Code. | Kompromittierung von Systemen, unautorisierter Datenzugriff. | Einführung von Vault/gleichwertigem Secrets‑Store vor Produktions‑Go‑Live. |
| **Entwicklungsumgebung ohne synthetische Testdaten** | Gefahr, produktive SAP‑Daten in Dev/Test zu nutzen. | Datenschutz‑Verstoß, Datenlecks. | Generierung von synthetischen bzw. pseudonymisierten Test‑Datasätzen, Verbot von Prod‑Datenzugriff. |
| **Rate‑Limiting & Missbrauchserkennung fehlt** | Keine Beschränkung für PDF‑Downloads / API‑Calls. | DDoS‑Angriff, Kostenexplosion, rechtliche Folgen bei Datenmissbrauch. | Implementierung einfacher Rate‑Limiting im Proxy, Auditing von Download‑Muster. |

### Compliance‑ und Datenschutzrisiken
| Risiko | Beschreibung | Mögliche Auswirkung | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|----------------------|--------------------------------|
| **Double‑Opt‑In & Löschkonzept nur Ansatz** | Anforderungen definiert, aber Implementierung nicht detailliert. | Nicht‑Konformität zu DSGVO, Bußgelder. | Detailliertes Lösch‑ und Opt‑In‑Verfahren implementieren, rechtliche Review. |
| **Auftrags‑Verarbeitungs‑Verträge (AVV) unklar** | Integration von SAP, Managed Service Provider, ggf. Dritt‑Provider. | Rechtsunsicherheit, Haftungsrisiko. | AVVs mit allen externen Partnern abschließen, Dokumentation. |
| **Daten‑Residenz (EU‑Only) vs. Kosten** | EU‑Only Hosting ist teurer, aber nötig. | Budget‑Risiko, mögliche Nutzung von nicht‑EU‑Regionen (Verstoß). | Hosting‑Provider mit garantierter EU‑Datenhaltung wählen, Kosten prüfen. |
| **Rechte‑auf‑Vergessen vs. Aufbewahrungspflichten** | Konflikt zwischen gesetzlicher Aufbewahrung (Handelsrecht) und DSGVO‑Löschrecht. | Rechtsstreit, Inkonsistente Prozesse. | Definition einer Retention‑Policy, die beides berücksichtigt, mit Ausnahmeregeln für Finanzdaten. |
| **Support‑Daten per E‑Mail** | Persönliche Daten können in E‑Mails landen. | DSGVO‑Verstoß, fehlende Audit‑Fähigkeit. | Verschlüsselte Mail‑Gateway, automatisierte Speicherung in strukturiertem System. |

### Widersprüche & Unsicherheiten
- **SSO vs. MVP‑Zeitplan** – SSO gewünscht, aber im MVP ausgeschlossen (Zeit‑Budget). → bewusstes Risiko, Priorisierung später.
- **Mehr‑Währung vs. Pilot‑Kunde Schweiz** – Pilot‑Kunde könnte CHF benötigen, aber MVP unterstützt nur EUR. → Entscheidung über Pilot‑Kunde muss vor Go‑Live getroffen werden.
- **Kosten‑Schätzung EU‑Hosting** – Noch offen, wirkt sich auf Budget‑Freigabe aus.
- **API‑Gateway Warteliste** – 6‑Wochen‑Verzögerung vs. 8‑Wochen‑MVP‑Deadline. → Risiko von Zeitverzögerung.
- **SAP‑Schreibzugriff** – Nur lesend im MVP, aber später notwendig für Bestellungs‑Sync. → späterer Integrationsaufwand.

### Zusammenfassung & Priorisierung
1. **Kritisch**: SAP‑Verfügbarkeit, Datenschutz (Opt‑In/Löschung, PII‑Logs), Backup/DR, Secrets‑Management.
2. **Hoch**: fehlendes API‑Gateway, Support‑Prozess, EU‑Only Hosting‑Kosten, Rabatt‑Freigabe.
3. **Mittel**: Mehr‑Währung, Internationalisierung, Rate‑Limiting, Retention vs. Löschung.
4. **Niedrig**: UI‑Internationalisierung (Deutsch/Englisch), optionales Mobile‑Design.

*Alle Risiken leiten sich unmittelbar aus den Stakeholder‑Transkripten (`input/transcripts/T9999_chaos.txt`), dem zusammengefassten Kontext (`runs/phase2_1/20260529_110440_365ef4/state/context.md`) und den definierten Anforderungen (`docs/requirements.md`) ab.*