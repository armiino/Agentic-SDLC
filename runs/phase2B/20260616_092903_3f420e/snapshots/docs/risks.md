# Risikodokumentation

## 1. Fachliche Risiken
| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahmen / Klärungsbedarf |
|--------|--------------|----------------------|---------------------------------|
| **Unklarer MVP‑Umfang** | Viele nicht‑kritische Features (SSO, Rabatt‑Freigabe, Mobile, Mehrwährung) sind im Projekt‑Backlog, deren finaler Einschluss unklar ist. | Scope‑Creep, Verzögerungen, Stakeholder‑Unzufriedenheit. | Frühe Abstimmung über finale MVP‑Features, klare Scope‑Definition und Change‑Control‑Prozess. |
| **SAP‑Zugriff (nur Leserechte)** | Im MVP ist nur lesender Zugriff auf Produkt‑ und Preisdaten vorgesehen. Schreibzugriff für Bestellungen fehlt. | Inkonsistente Datenflüsse, manuelle Work‑arounds, spätere Integrations‑Aufwände. | Frühzeitige Planung des Schreib‑Zugriffs, klare Schnittstellen‑Definition für Phase 2, Test‑Konzepte. |
| **Support‑Workflow ohne Ticket‑System** | Support wird nur über ein Kontaktformular abgewickelt. | Unvollständige Nachverfolgung von Anfragen, Risiko von DSGVO‑Verstößen bei Datenlöschung. | Minimal‑Ticket‑Tracking einführen, klare Dokumentation von Support‑Interaktionen, Integration in Audit‑Trail. |
| **Freigabe‑Workflow für Rabatte > 15 %** | Vorgesehen für Phase 2, aber Finance fordert bereits klare Prozesse. | Fehlende Kontrolle, mögliche Preis‑Manipulation, Compliance‑Risiko. | Definition eines temporären manuellen Freigabe‑Prozesses für MVP, Dokumentationspflicht. |

## 2. Technische Risiken
| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahmen / Klärungsbedarf |
|--------|--------------|----------------------|---------------------------------|
| **API‑Gateway‑Verzögerung (6 Wochen‑Warteliste)** | Sicherer API‑Gateway‑Zugang ist erst nach 6 Wochen verfügbar. | Unsichere Direkt‑API‑Aufrufe, fehlendes OAuth/Rate‑Limiting. | Interim‑Proxy‑Lösung mit IP‑Allowlist, temporäres API‑Key‑Management, ASAP‑Plan für Gateway‑Integration. |
| **Rate‑Limiting (nur Minimal‑Implementierung)** | Konkrete Schwellenwerte fehlen. | Missbrauch, DoS‑Angriffe, Service‑Verfügbarkeit gefährdet. | Festlegung von Basis‑Limits (z. B. 100 Requests/Minute/User), Monitoring und Anpassung nach Load‑Test. |
| **Performance & Skalierbarkeit (200‑20 000 gleichzeitige Nutzer)** | Erwarteter Nutzeranstieg erfordert Pagination, Caching. | Latenz‑Spitzen, Ausfall bei Lastspitzen, SLA‑Verletzung (99,5 %). | Load‑Testing, horizontale Skalierung via Container/Orchestrierung, Caching‑Strategie definieren. |
| **Logging‑Trennung (technische Logs vs. personenbezogene Audit‑Logs)** | Gefahr, dass PII versehentlich in technische Logs gelangt. | DSGVO‑Verstoß, Bußgelder, Reputationsschaden. | Logging‑Framework mit Maskierung, klare Log‑Kategorien, regelmäßige Audits. |
| **Backup & Disaster Recovery (RPO/RTO undefiniert)** | Keine konkreten Werte für Wiederherstellungspunkte/-zeiten. | Datenverlust, lange Ausfallzeiten, Compliance‑Risiko. | Definition von RPO (z. B. 4 h) und RTO (z. B. 2 h), regelmäßige DR‑Tests, Dokumentation. |
| **Kosten‑Unsicherheit Managed Services** | EU‑Only Managed Service Provider noch nicht ausgewählt, Budget‑Risiken. | Budgetüberschreitung, Projektverzögerung bei Vertragsverhandlungen. | Frühzeitige Anbieter‑Evaluation, Kostenschätzung mit Puffer, Vertrags‑Check‑Liste. |

## 3. Compliance‑ und Datenschutzrisiken
| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahmen / Klärungsbedarf |
|--------|--------------|----------------------|---------------------------------|
| **Double‑Opt‑In‑Umsetzung** | Muss DSGVO‑konform sein. | Nicht‑konforme Einwilligungen → rechtliche Folgen. | Rechts‑Check, klare UI‑Flows, Dokumentation der Einwilligungen. |
| **EU‑Only Datenresidenz vs. Schweizer Kunden** | Schweiz hat spezifische Datenschutz‑Anforderungen. | Rechtsverletzung, mögliche Datenübertragung ohne Angemessenheitsbeschluss. | Klärung, ob Schweizer Daten in EU‑Rechenzentren zulässig sind, ggf. separate Hosting‑Option. |
| **Audit‑Trail ohne personenbezogene Daten** | Risiko, dass personenbezogene Identifikatoren im Audit‑Log gespeichert werden. | DSGVO‑Verstoß, Datenschutz‑Bedenken. | Design des Audit‑Logs mit pseudonymisierten Nutzer‑IDs, Trennung von Log‑Systemen. |
| **Testdaten enthalten reale Kundendaten** | SAP‑Testsystem könnte echte Daten enthalten. | Datenleck in Entwicklungs‑/CI‑Umgebung, Compliance‑Verstoß. | Anonymisierung / Pseudonymisierung vor Nutzung, Daten‑Maskierungs‑Tool einsetzen. |
| **Retention‑ und Lösch‑Policy nicht definiert** | Unklare Aufbewahrungsfristen vs. Recht auf Vergessenwerden. | Risiko von Verstößen gegen Aufbewahrungspflichten oder Löschrechte. | Definition einer Retention‑Matrix, Automatisierung von Löschvorgängen, Dokumentation. |

## 4. Widersprüche und Unsicherheiten
- **Optionales SSO**: Noch nicht entschieden, ob im MVP enthalten. → kann Scope‑ und Sicherheits‑Risiken verändern.
- **Pilot‑Kunde (Deutschland vs. Schweiz)**: Einfluss auf Währungs‑ und Datenschutz‑Anforderungen.
- **Hosting‑Provider Auswahl**: Kosten‑ und Vertragsdetails offen, wirkt sich auf Budget und EU‑Only‑Policy aus.
- **Rate‑Limiting‑Schwellenwerte**: Noch nicht festgelegt, Risiko von Over‑/Under‑Protection.
- **Backup‑RPO/RTO**: Fehlende Spezifikationen, beeinflussen DR‑Strategie.
- **Preis‑ und Rabatt‑Logik (nächtliche Preis‑Updates)**: Keine klare Cache‑Invalidierungs‑Strategie → mögliche Inkonsistenzen.

## 5. Mögliche Auswirkungen (Zusammenfassung)
- **Projektverzögerungen** durch unklare Anforderungen, fehlende Infrastrukturteile (API‑Gateway, SSO).
- **Kostenüberschreitungen** wegen unklarer Managed‑Service‑Verträge und zusätzlicher Sicherheits‑Implementierungen.
- **Regulatorische Konsequenzen** (Bußgelder, Rechtsstreit) bei Nichteinhaltung von DSGVO‑Anforderungen (Opt‑In, Datenresidenz, Löschkonzepte).
- **Sicherheitsvorfälle** durch unzureichendes Rate‑Limiting, fehlende Trennung von Logs, unsichere Zwischen‑Proxy‑Lösung.
- **Kundenzufriedenheit** sinkt bei fehlendem Support‑Ticket‑System oder unklaren Rabatt‑Freigaben.

## 6. Gegenmaßnahmen / Klärungsbedarf (Empfehlungen)
1. **Scope‑Klärung**: Formaler Beschluss über finale MVP‑Features (SSO, Rabatt‑Workflow) bis Projekt‑Kick‑off.
2. **Interim‑API‑Sicherheit**: Implementierung einer Proxy‑Lösung mit IP‑Allowlist, API‑Key‑Management und Grund‑OAuth‑Flows.
3. **Rate‑Limiting‑Policy**: Definition von Mindest‑Limits, Monitoring‑Dashboard für Überschreitungen.
4. **Logging‑Governance**: Einführung eines Logging‑Frameworks mit automatischer PII‑Maskierung, getrennte Speicherorte für Audit‑Logs.
5. **Backup‑/DR‑Plan**: Festlegung von RPO = 4 h, RTO = 2 h, monatliche Wiederherstellungstests.
6. **Datenschutz‑Check**: Rechts‑Review des Double‑Opt‑In‑Flows, Datenresidenz und Lösch‑Policy, Anonymisierung von Testdaten.
7. **Kosten‑ und Provider‑Analyse**: Frühe Auswahl eines EU‑Only Managed‑Service‑Providers, Kosten‑Modell prüfen, Vertragsklauseln für SLA.
8. **Support‑Workflow**: Minimal‑Ticket‑System (z. B. Jira Service Management) oder strukturierte E‑Mail‑Protokollierung, Verknüpfung mit Audit‑Trail.
9. **Preis‑Cache‑Strategie**: Klare Regeln für Cache‑Invalidierung bei nächtlichen Preis‑Updates, ggf. Event‑basiertes Refresh.
10. **Pilot‑Kunden‑Entscheidung**: Schnellentscheidung, um Währungs‑ und Datenschutz‑Anforderungen zu finalisieren.

*Dieses Risikodokument leitet sich aus dem bereitgestellten Projekt‑Kontext, den funktionalen und nicht‑funktionalen Anforderungen sowie den identifizierten offenen Punkten und Unsicherheiten ab.*