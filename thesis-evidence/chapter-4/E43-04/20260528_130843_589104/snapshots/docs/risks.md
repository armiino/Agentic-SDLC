# Risikobericht – Kundenportal (MVP)

## 1. Fachliche Risiken
| ID | Risiko | Ursprung / Kontext | Mögliche Auswirkungen | Gegenmaßnahme / Klärungsbedarf |
|----|--------|--------------------|-----------------------|--------------------------------|
| FR‑01 | Unklare Zieldefinition (Portal vs. Mobile App) | Unterschiedliche Erwartungen von Sales (mobile) und Product Owner (Web‑First) | Verwirrung im Team, Verzögerungen, Scope‑Creep | Dokumentierte Entscheidung: Mobile erst in Phase 2, klare Kommunikation im Team. |
| FR‑02 | Sonderrabatte ohne Freigabe | Finance‑Anforderung (Rabatt > 15 %) vs. MVP‑Beschränkung (keine Sonderrabatte) | Finanzielle Verluste, Compliance‑Verstöße | Im MVP keine Sonderrabatte erlauben. Freigabe‑Workflow in späterer Phase definieren. |
| FR‑03 | Support‑Prozess nicht abgedeckt | Kein Ticket‑System, nur Kontaktformular | Schlechte Kundenzufriedenheit, fehlende Nachverfolgung von Anfragen | Risiko bewusst akzeptiert; später Ticket‑System implementieren, Dokumentation der Übergangs‑Prozedur. |
| FR‑04 | Mehrwährungs‑ und Internationalisierungs‑Unsicherheit | Pilotkunde Schweiz (CHF) vs. MVP‑Budget | Fehlende Währungs‑Unterstützung für Pilot, zusätzliche Entwicklungsaufwand | MVP auf EUR beschränken, optional CHF nur bei bestätigtem Pilot‑Kunden; Klarheit im Scope. |
| FR‑05 | Unklare KPI‑Definitionen | Conversion‑Rate, Zeit bis Angebot werden genannt, aber nicht spezifiziert | Keine messbare Erfolgskontrolle, schweres Reporting | Definition konkreter Messgrößen und Erfassungsmechanismus in Phase 2. |
| FR‑06 | Rollen‑ und Berechtigungskonflikte (Support vs. Finance) | Unterschiedliche Sichtweisen zu Datenzugriff | Datenlecks, unautorisierter Rabatt‑Zugriff | Minimal‑Rollenmodell (Admin, Sales, Kunde) im MVP; detailliertes Rollen‑Konzept später ausarbeiten. |

## 2. Technische Risiken
| ID | Risiko | Ursprung / Kontext | Mögliche Auswirkungen | Gegenmaßnahme / Klärungsbedarf |
|----|--------|--------------------|-----------------------|--------------------------------|
| TR‑01 | Backend‑API‑Readiness fehlt | Ben: "Backend noch nicht API‑ready" | Verzögerung beim Aufbau des API‑Layers, Integrationsprobleme mit SAP | Priorisierung: API‑Layer schnell bauen (lesend), ggf. Mock‑Services für MVP. |
| TR‑02 | API‑Gateway‑Warteliste (6 Wochen) vs. 8‑Wochen‑MVP | Farid & Ben | MVP kann das zentrale Gateway nicht nutzen → eigene API‑Instanz nötig, erhöhtes Sicherheits‑ und Betriebsrisiko | Direktes Deploy einer eigenständigen API‑Instanz (z. B. Managed Service) mit OAuth; später Migration zum Gateway. |
| TR‑03 | Sicherheits‑Review dauert länger (6 Wochen) | Ben & Clara | MVP wird ohne vollständige Security‑Review released → mögliche Schwachstellen | Minimal‑Sicherheitsmaßnahmen (TLS, OAuth, Audit‑Log) implementieren; Review‑Plan für Phase 2 definieren. |
| TR‑04 | EU‑Only Hosting kostet mehr | Farid & Anna | Budget‑Überschreitung, mögliche Verzögerung | Kosten‑Schätzung einholen, ggf. Kompromiss mit EU‑Region‑SLA; Risiko dokumentieren. |
| TR‑05 | SAP‑Verfügbarkeit (Wartungsfenster am Wochenende) | Ben & Farid | Angebotserstellung kann fehlschlagen, schlechte UX | Fallback‑Cache für Produkt‑/Preis‑Daten (ohne personenbezogene Daten) planen; SLA‑Absprachen mit SAP‑Team. |
| TR‑06 | Daten‑Hosting und Datenschutz (Datenresidenz) | Clara & Farid | Nicht‑EU‑Daten können DSGVO‑Verstöße auslösen | Verträge mit Managed Service prüfen, Nachweis der EU‑Datenresidenz sichern. |
| TR‑07 | Logging‑ und Monitoring‑Konflikt (personenbezogene Daten in Logs) | Clara & Farid | DSGVO‑Verstöße, Bußgelder | Trennung von Application‑Logs und Audit‑Logs, Pseudonymisierung in Monitoring, klare Retention‑Policy. |
| TR‑08 | Keine neue Datenbank (Managed Service) – Auswahl noch offen | Anna & Ben | Architektur‑Unsicherheit, Migrationsrisiko | Auswahl eines geeigneten Managed DB‑Dienstes (z. B. PostgreSQL‑as‑a‑Service) im MVP‑Zeitrahmen; Risiko‑Log + Entscheidungs‑Dokument. |
| TR‑09 | Test‑Daten‑Qualität (echte SAP‑Testdaten) | Ben & Clara | Datenschutz‑Risiko, fehlerhafte Tests | Verwenden von synthetischen oder pseudonymisierten Testdaten; automatisierte Daten‑Maskierung. |
| TR‑10 | Rate‑Limiting & Missbrauchserkennung fehlt | Ben & Clara | DDoS‑Risiko, Datenexfiltration | Basis‑Rate‑Limiting in API implementieren; erweiterte Schutzmaßnahmen in Phase 2. |
| TR‑11 | Backup & Disaster Recovery nicht detailliert | Clara & Anna | Datenverlust, Service‑Ausfälle | Tägliche Backups mit EU‑Only Storage; Recovery‑Test im Sprint 2, Dokumentation. |

## 3. Compliance‑ / Datenschutzrisiken
| ID | Risiko | Ursprung / Kontext | Mögliche Auswirkungen | Gegenmaßnahme / Klärungsbedarf |
|----|--------|--------------------|-----------------------|--------------------------------|
| CR‑01 | Unvollständige Double‑Opt‑In‑Implementierung | Anna & Clara | Rechtswidrige Verarbeitung, Bußgelder | Double‑Opt‑In verpflichtend im MVP; Dokumentation des Prozesses. |
| CR‑02 | Fehlende Lösch‑ und Aufbewahrungskonzepte | Clara & Anna | Nicht‑Erfüllung des Rechts auf Löschung, Aufbewahrungspflichten | Minimaler Retention‑Plan (z. B. 2 Jahre für Rechnungen, 6 Monate für Angebote) mit Ausnahmen für gesetzliche Pflichten; später ausbauen. |
| CR‑03 | Datenminimierung nicht gewährleistet (Cache mit Kundendaten) | Ben & Clara | Unnötige Verarbeitung personenbezogener Daten → DSGVO‑Verstoß | Cache nur für produktbezogene Daten; keine personenbezogenen Daten speichern. |
| CR‑04 | Unklare Datenresidenz (EU‑Only vs. Schweizer Daten) | Farid & Clara | Rechtsunsicherheit, mögliche Verstöße gegen schweizerisches Datenschutzrecht | Vertragliche Klarstellung für Schweizer Kunden, ggf. Separate Datenhaltung. |
| CR‑05 | Keine Auftragsverarbeitungs‑Verträge (AVV) für Managed Services | Clara | Vertragsverletzungen, Bußgelder | AVV mit Provider vor MVP‑Launch abschließen; Risiko dokumentieren falls nicht rechtzeitig. |
| CR‑06 | Keine personenbezogenen Daten in Monitoring‑Logs | Clara & Farid | Gefahr von Datenlecks über Monitoring‑Systeme | Logging‑Policy definieren, Pseudonymisierung, getrennte Log‑Streams. |
| CR‑07 | Fehlende Audit‑Logs für Änderungen an Angeboten | Clara | Fehlende Nachvollziehbarkeit, Compliance‑Probleme | Minimaler Audit‑Trail im MVP (wer hat Angebot erstellt/geändert) implementieren; später erweitern. |

## 4. Widersprüche & Unsicherheiten (Quellen)
- **SSO vs. MVP‑Umfang** – Azure AD/Google wird diskutiert, aber nicht im MVP (Widerspruch zwischen Anna und Ben).
- **Rabatt‑Freigabe** – Finanz‑Anforderung (Rabatt > 15 %) vs. MVP‑Beschränkung (keine Sonderrabatte). 
- **Support‑Ticket‑System** – Bedarf an strukturiertem Support (David) vs. bewusste Ausklammerung im MVP (Anna).
- **API‑Gateway‑Warteliste** – Wunsch nach zentralem Gateway (Farid) vs. 8‑Wochen‑Zeitplan (Anna).
- **Hosting‑Kosten** – EU‑Only Hosting ist Pflicht (Farid) aber teurer (Anna) ⇒ Kosten‑Unsicherheit.
- **Mehrwährung** – Pilot‑Kunde Schweiz (CHF) vs. MVP‑Budget ⇒ Unklar, ob CHF implementiert werden muss.
- **SAP‑Verfügbarkeit** – Kritische Abhängigkeit, jedoch keine Fallback‑Strategie definiert.
- **Backup‑ und DR‑Detailtiefe** – Grund‑Backup definiert, aber keine detaillierte DR‑Planung.
- **Test‑Daten** – Nutzung von SAP‑Testsystem mit echten Daten vs. DSGVO‑Anforderung pseudonymisierter Testdaten.

## 5. Mögliche Auswirkungen Gesamtprojekt
- **Zeitplan‑Verzögerungen** durch fehlende API‑Readiness, Security‑Review, Hosting‑Entscheidungen.
- **Budget‑Überschreitungen** wegen EU‑Only Hosting, Managed Service Auswahl, ggf. zusätzlicher Lizenzkosten für SSO.
- **Rechtliche Konsequenzen** bei unvollständiger DSGVO‑Umsetzung (Double‑Opt‑In, Löschrechte, Datenresidenz).
- **Qualitäts‑ und Kundenzufriedenheits‑Risiken** durch fehlendes Support‑Ticket‑System und unklare Rabatt‑Freigabe.
- **Technische Schuld** durch schnelle MVP‑Umsetzung ohne kompletten API‑Gateway, ohne vollständige Security‑Review und ohne umfangreiche Test‑Daten‑Strategie.

## 6. Priorisierung (nach Impact × Likelihood – grob geschätzt)
1. **TR‑03 / CR‑01** – Sicherheit & Double‑Opt‑In (hoher Impact, hohe Wahrscheinlichkeit).
2. **TR‑02 / TR‑01** – API‑Readiness & Gateway (hoher Impact, mittel‑hoch Wahrscheinlichkeit).
3. **CR‑02 / FR‑06** – Rollen‑ und Berechtigungskonflikte (mittel Impact, mittel Wahrscheinlichkeit).
4. **TR‑05 / FR‑02** – SAP‑Verfügbarkeit & Rabatt‑Freigabe (mittel Impact, mittel).
5. **CR‑04 / TR‑04** – EU‑Only Hosting Kosten (mittel Impact, mittel).
6. **FR‑03 / CR‑06** – Support‑Prozess & Logging (geringer‑mittel Impact, mittel).
7. **TR‑09 / CR‑03** – Test‑Daten‑Qualität (gering‑mittel Impact, gering).

*Die Priorisierung dient als Leitfaden für das weitere Risikomanagement und die Planung von Gegenmaßnahmen in den kommenden Phasen.*