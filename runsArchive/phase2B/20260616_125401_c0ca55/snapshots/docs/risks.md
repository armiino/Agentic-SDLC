# Risikodokument – Kundenportal MVP (8‑Wochen)

## 1. Fachliche Risiken

| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahmen / Klärungsbedarf |
|--------|--------------|----------------------|----------------------------------|
| **Zeitplan vs. Umfang** | Das MVP soll in 8 Wochen geliefert werden, wobei umfangreiche Anforderungen (Security‑Review, API‑Gateway‑Warteliste, Mehrwährung, Internationalisierung) bereits definiert sind. | Verzögerungen, Scope‑Creep, Qualitätsverlust. | Priorisierung der Kern‑Features (Login, Rollen, SAP‑Read, Angebot/Bestellung, Audit‑Log). Iteratives Release‑Planing mit klaren Milestones. | 
| **SAP‑Abhängigkeit** | Das System liest Daten ausschließlich aus SAP. Verfügbarkeit, Latenz und API‑Limits sind kritisch. | Ausfall der Angebots‑ und Rechnungsanzeige, schlechte User‑Experience. | Caching‑Layer mit definiertem Fallback, SLA‑Klärung mit SAP‑Team, Monitoring der SAP‑Integrations‑Metriken. | 
| **Unklare Mobile‑Strategie** | Entscheidung zwischen responsivem Web‑Design und nativer Mobile‑App noch offen. | Ressourcen‑Umverteilung, mögliche Nacharbeiten, Budget‑Überschreitung. | Entscheidung bis Sprint‑2, ggf. Aufschub in Phase 2, klare Definition des Scope für MVP. | 
| **SSO / Identity Provider** | Azure AD/Google‑SSO wird im MVP ausgeschlossen, könnte später nachträglich integriert werden müssen. | zusätzlicher Integrationsaufwand, mögliche Änderungen am Auth‑Flow. | Dokumentation des zukünftigen SSO‑Designs, modulare Auth‑Architektur (z. B. über OpenID‑Connect). | 
| **Rabatt‑Freigabe‑Workflow (>15 % Rabatt)** | Workflow wird im MVP bewusst weggelassen. | Manuelle Freigaben, Compliance‑Risiko bei evtl. nicht dokumentierten Rabatten. | Definieren, ob temporäre manuelle Prozesse ausreichen, später automatisierte Workflow‑Implementierung planen. |
| **Support‑Ticket‑System** | Unklar, ob ein strukturiertes Ticket‑System oder reines E‑Mail‑basiertes Verfahren verwendet wird. | Ineffiziente Support‑Prozesse, unklare Verantwortlichkeiten, Datenschutz‑Probleme bei E‑Mail‑Logs. | Entscheidung bis Sprint‑3, ggf. Integration eines leichtgewichtigen Ticket‑Tools (z. B. Jira Service Management) mit DSGVO‑Konformität. |

## 2. Technische Risiken

| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahmen / Klärungsbedarf |
|--------|--------------|----------------------|----------------------------------|
| **Security Review Timing** | Geplanter Security‑Review dauert 6 Wochen und überschneidet sich stark mit der 8‑Wochen‑MVP‑Deadline. | Fehlende Sicherheits‑Abnahme, Release‑Verzögerung, mögliche Nachbesserungen im Live‑Betrieb. | Parallel‑Running von Review‑ und Entwicklungs‑Sprints, minimale Sicherheits‑MVP‑Kriterien (HTTPS, OAuth2, Input‑Validation) frühzeitig umsetzen. |
| **EU‑Only Hosting Kosten** | Nutzung von Managed Services ausschließlich in EU‑Rechenzentren kann teurer sein als Standard‑Angebote. | Budget‑Überschreitung, Einschränkung bei Auswahl von Services (z. B. Datenbank, CDN). | Kosten‑Analyse bis Sprint‑1, ggf. Verhandlung mit Anbieter, mögliche Hybrid‑Ansätze (z. B. EU‑Edge‑Cache). |
| **Keine neue Datenbank** | Verzicht auf eigenständige DB, Nutzung bestehender Managed Services. | Einschränkungen bei Datenmodell, Skalierbarkeit, Performance‑Optimierung. | Bewertung vorhandener Daten‑Store‑Optionen (z. B. PostgreSQL as a Service), ggf. Einführung von schemalosen Stores für bestimmte Use‑Cases. |
| **Rate‑Limiting & Missbrauchserkennung** | Mechanismus muss noch definiert werden, Implementierung ist aufwändig. | API‑Missbrauch, Service‑Denial‑of‑Service, unerwartete Kosten. | Early‑Design eines API‑Gateways (z. B. Kong, AWS API GW) mit Grund‑Rate‑Limits, später Feintuning. |
| **Backup & Disaster Recovery (DR)** | Detaillierte Backup‑Strategie, Aufbewahrungsfristen und Wiederherstellungszeit (RTO) sind offen. | Datenverlust, lange Ausfallzeiten, Compliance‑Verstöße. | Definition von RPO/RTO bis Sprint‑2, tägliche Backups mit 4‑Stunden RTO, Test‑Restore‑Prozesse etablieren. |
| **Monitoring & Logging Trennung** | Technische Logs müssen von Audit‑Logs getrennt werden, aber keine konkrete Umsetzung definiert. | Fehlende Nachvollziehbarkeit, mögliche Regulierungs‑Verstöße. | Einsatz von getrennten Log‑Pipelines (z. B. Elasticsearch für Tech‑Logs, immutable Storage für Audit‑Logs). |
| **Skalierbarkeit & Performance** | Ziel: 10.000 gleichzeitige Nutzer, Antwortzeit ≤ 2 s. | System‑Ausfälle bei Traffic‑Spitzen, schlechtes Nutzererlebnis. | Horizontal skalierbare Architektur (Container‑Orchestrierung), Load‑Testing in frühen Sprints, Performance‑Monitoring. |

## 3. Compliance‑ und Datenschutz‑Risiken

| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahmen / Klärungsbedarf |
|--------|--------------|----------------------|----------------------------------|
| **DSGVO‑Pflichten (Double‑Opt‑In, Löschkonzept, Audit‑Log)** | Vorgaben sind definiert, aber konkrete technische Umsetzung fehlt. | Bußgelder, Reputations‑Schaden, Nicht‑Konformität bei Audits. | Implementierung von Double‑Opt‑In-Workflow, automatisierte Lösch‑Jobs, immutable Audit‑Log‑Speicherung mit Zugriffskontrolle. |
| **Datenresidenz (EU‑Only)** | Alle Daten müssen in EU‑Rechenzentren verbleiben. | Risiko bei Nutzung von Dritt‑Services außerhalb der EU, mögliche Rechtsverstöße. | Auswahl von EU‑zertifizierten Cloud‑Providern, Prüfung von Sub‑Processor‑Verträgen. |
| **Testdaten mit echten Kundendaten** | Nutzung von SAP‑Testsystemen enthält reale Kundendaten. | Datenschutz‑Verletzung, rechtliche Konsequenzen. | Pseudonymisierung oder Generierung synthetischer Testdaten, strenge Zugriffskontrollen im Test‑Umfeld. |
| **Aufbewahrungs‑ und Löschfristen** | Unterschiedliche gesetzliche Aufbewahrungsfristen (z. B. Buchhaltungsdaten) vs. gewünschte 30‑Tage‑Löschung. | Konflikt zwischen rechtlichen Vorgaben und Projekt‑Zielen. | Erstellung eines Retention‑Plans, der beide Anforderungen abdeckt (z. B. separate Archive für Buchhaltungsdaten). |
| **Audit‑Log‑Integrität** | Unklar, wie Audit‑Logs unveränderlich und gesichert werden sollen. | Manipulation von Log‑Daten, Prüfungs‑Risiko. | Einsatz von Write‑Once‑Read‑Many (WORM) Storage, digitale Signaturen, regelmäßige Log‑Integrity‑Checks. |

## 4. Widersprüche, Unsicherheiten & offene Punkte

| Thema | Beobachteter Widerspruch / Unsicherheit | Konsequenz für das Risiko‑Management |
|-------|-------------------------------------------|-------------------------------------|
| **Mobile vs. Web‑First** | Projektziel ist Web‑First, aber Mobile wird später benötigt. | Risiko, dass spätere Mobile‑Entwicklung Grundarchitektur neu gestaltet. | Frühzeitige Entscheidung über Responsive Design + API‑First‑Ansatz, um Mobile‑Nachfolge zu erleichtern. |
| **SSO Ausklammerung** | Wunsch nach Azure AD/Google‑SSO, aber kein Aufwand im MVP. | Gefahr, dass späteres Hinzufügen erhebliche Refactoring‑Kosten verursacht. | Modulare Auth‑Komponente planen, klare Schnittstellen definieren. |
| **Kosten EU‑Only Managed Service** | Noch keine finale Kosten‑Abschätzung. | Budget‑Überschreitung, mögliche Scope‑Reduktion. | Kosten‑Modell bis Sprint‑1 erstellen, ggf. Alternativen prüfen. |
| **Rate‑Limiting Mechanismus** | Noch nicht definiert, aber als Sicherheits‑Feature nötig. | Unklarer Schutz vor API‑Missbrauch, mögliche Performance‑Probleme. | Minimal‑Rate‑Limit (z. B. 100 Requests/Min pro User) in Sprint‑2 implementieren, später anpassen. |
| **Backup & DR Details** | Aufbewahrungsfristen, RTO, Test‑Restore‑Prozesse fehlen. | Risiko von Datenverlust und Nicht‑Erfüllung von SLA. | Detaillierten Backup‑Plan bis Sprint‑2 erarbeiten, Test‑Restore‑Durchläufe einplanen. |
| **Support‑Ticket‑Lösung** | Unklar, ob E‑Mail‑basiert oder Ticket‑System verwendet wird. | Unklare Prozesse, potenzielle Datenschutz‑Probleme bei E‑Mail‑Logs. | Entscheidung und Implementierung bis Sprint‑3, DSGVO‑konforme Lösung wählen. |

## 5. Zusammenfassung & Priorisierung

1. **Zeitplan‑ vs. Umfang** – höchste Priorität, da es das gesamte Projekt gefährdet. 
2. **Security Review Timing** – eng mit Zeitplan verbunden, muss parallel laufen. 
3. **DSGVO‑Konformität (Double‑Opt‑In, Löschkonzept, Audit‑Log)** – rechtlich zwingend, muss früh umgesetzt werden. 
4. **SAP‑Verfügbarkeit & Caching** – kritische technische Abhängigkeit, muss stabil sein. 
5. **Backup & DR** – Datenintegrität und Business‑Continuity, mittlere Priorität. 
6. **Kosten EU‑Only Hosting** – Budget‑Risk, sollte zeitnah geklärt werden. 
7. **Rate‑Limiting & API‑Gateway** – wichtig für Sicherheit, aber technisch umsetzbar nach Kern‑Funktionalität. 
8. **Offene Entscheidungen (Mobile, SSO, Support‑Ticket)** – sollten nach MVP‑Launch definiert werden, um Scope‑Creep zu vermeiden.

---
*Dieses Risiko‑Artefakt ist ein initialer Entwurf, basierend auf dem aktuellen Projekt‑ und Anforderungs‑Kontext. Weitere Detail‑Analysen und Stakeholder‑Reviews sind notwendig, um die genannten Gegenmaßnahmen zu konkretisieren und Prioritäten exakt zu gewichten.*