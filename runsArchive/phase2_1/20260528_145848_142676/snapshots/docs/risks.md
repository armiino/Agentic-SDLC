# Risiken für das Kundenportal (MVP)

## Fachliche Risiken
| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|------------------------|--------------------------------|
| **Unklare Zieldefinition / Scope‑Creep** | Unterschiedliche Erwartungen (Portal, Mobile, Support, Mehrwährung, Rabatte) führen zu ständiger Erweiterung des MVP. | Projektverzug, Überschreitung des 8‑Wochen‑Zeitplans, Budgetüberschuss. | Festes Scope‑Dokument mit Prioritäten; Change‑Control‑Prozess etablieren. |
| **Rabatt‑Freigabeprozess fehlt** | Sonderrabatte >15 % sollen erst nach MVP freigegeben werden, aber keine klare Regelung im MVP. | Finanzielle Verluste, Compliance‑Risiko bei nicht autorisierten Rabatten. | Im MVP ausdrücklich keine Sonderrabatte zulassen; später definierten Freigabe‑Workflow dokumentieren. |
| **Support‑Workflow unklar** | Nur ein Kontaktformular, kein Ticket‑System, aber hoher Support‑Aufwand erwartet. | Verzögerte Kundenreaktionen, Unzufriedenheit, manuelle Fehler. | Bewusste Einschränkung im MVP kennzeichnen; Plan für Ticket‑System nach MVP erstellen. |
| **Mehrwährungs‑ und Länderspezifische Anforderungen** | Pilotkunde könnte Schweiz sein (CHF, Schweizer Datenschutz). | Fehlende Funktionalität, rechtliche Probleme, zusätzliche Entwicklungszeit. | Mehrwährung und Schweizer Datenschutz als offene Punkte dokumentieren; zunächst nur EUR. |
| **KPI‑Messung ohne Analytics** | Conversion‑Rate soll gemessen werden, aber keine Analytics‑Infrastruktur. | Keine Erfolgskontrolle, Fehlentscheidungen. | Basis‑Analytics (z. B. Google Analytics) erst nach MVP; MVP‑KPIs manuell erfassen. |
| **Unklare Daten‑Lösch‑ und Auskunftsprozesse** | DSGVO verlangt Lösch‑/Auskunftsrechte, aber Verfahren fehlen. | Bußgelder, Vertrauensverlust. | Minimal‑Löschkonzept (Account‑Deletion) implementieren; detaillierte Prozesse später ausarbeiten. |

## Technische Risiken
| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|------------------------|--------------------------------|
| **SAP‑Verfügbarkeit (kritische Abhängigkeit)** | MVP benötigt SAP‑Lesezugriff für Produkt‑ und Preisdaten. | Angebotserstellung blockiert, Ausfallzeiten. | Fallback‑Cache nur für Produktdaten; klare Fehlermeldung bei SAP‑Ausfall; SLA mit SAP‑Team definieren. |
| **API‑Gateway‑Wartezeit (6 Wochen)** | Sicherheits‑ und Rate‑Limiting‑Funktionen sollen über Gateway laufen, aber nicht verfügbar im MVP. | Erhöhtes Sicherheits‑ und Skalierungs‑Risiko. | Interim‑Rate‑Limiting im Service implementieren; klare Dokumentation der Lücke. |
| **Keine neue Datenbank – Managed DB** | Nutzung eines Managed DB Service muss EU‑only sein; Kosten und Performance unklar. | Budget‑Überschreitung, Performance‑Engpässe. | Frühzeitige Provider‑Auswahl; Load‑Test im Prototyp‑Setup. |
| **Logging & Audit vs. DSGVO** | Technische Logs dürfen keine personenbezogenen Daten enthalten, aber Audit‑Logs benötigen Detailinformationen. | Datenschutzverletzung, nicht‑erfüllte Audit‑Pflichten. | Trennung von Log‑Kanälen; Pseudonymisierung in Audit‑Logs; Log‑Retention‑Plan definieren. |
| **Backup & Disaster Recovery (Mindest‑Level)** | Backup‑Strategie ist nur grob definiert. | Datenverlust, lange Wiederherstellungszeit (RPO/RTO). | Backup‑Plan mit definierten RPO < 24 h, regelmäßige Restore‑Tests. |
| **Secrets Management** | API‑Credentials, DB‑Passwörter müssen sicher verwaltet werden. | Kompromittierung von Systemen, Datenleck. | Einsatz von Managed Secrets Service (z. B. Azure Key Vault) oder HashiCorp Vault; Integration bereits im MVP. |
| **Rate Limiting / Missbrauchserkennung** | Ohne API‑Gateway fehlen robuste Schutzmechanismen. | DDoS‑Risiko, Service‑Ausfall bei Massendownloads. | Simple token‑basierte Rate‑Limiter im Backend; später erweitern. |
| **Testdaten‑Umgebung mit realen SAP‑Daten** | SAP‑Testsystem enthält echte Kundendaten. | Datenschutzverstoß, GDPR‑Verletzung. | Daten pseudonymisieren bzw. synthetische Testdaten verwenden; klare Policy. |
| **Caching von Preisdaten** | Preis‑ und Rabatt‑Logik ist kundenspezifisch und ändert nachts. | Veraltete Angebote, rechtliche Nachfragen. | Kein Cache für Preisdaten im MVP; nur statische Produktinformationen cachen. |
| **Internationalisierung (i18n)** | UI muss DE/EN unterstützen; weitere Sprachen später. | Fehlende Übersetzungen, Usability‑Probleme für Nicht‑DE‑User. | Minimal‑i18n‑Framework einbinden, Inhalte externalisieren. |

## Compliance‑ und Datenschutzrisiken
| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|------------------------|--------------------------------|
| **DSGVO‑Konformität (Double‑Opt‑In, Löschung, Datenresidenz)** | Unklare Umsetzung von Double‑Opt‑In, Löschkonzept und EU‑only Hosting. | Bußgelder, Rechtsstreitigkeiten, Vertrauensverlust. | Double‑Opt‑In sofort implementieren; Hosting‑Provider mit EU‑Only‑Garantie wählen; Lösch‑Workflow dokumentieren. |
| **Datenminimierung vs. Integration** | Gesamte Kundendaten aus SAP werden bei jedem Aufruf geladen. | Erhöhte Datenexposition, unnötige Verarbeitung. | Datenzugriff nur on‑demand; keine redundante Speicherung im Portal. |
| **Aufbewahrungs‑ und Retention‑Regeln** | Unterschiedliche gesetzliche Aufbewahrungsfristen (Handelsrecht vs. Recht‑auf‑Löschung). | Konflikt zwischen Aufbewahrungspflicht und DSGVO‑Löschpflicht. | Baseline‑Retention (z. B. 7 Jahre) mit Ausnahmeregelungen für Löschanfragen; rechtliche Klärung nach MVP. |
| **Audit‑Log vs. technische Log‑Trennung** | Gefahr, dass personenbezogene Daten in System‑Logs gelangen. | Datenschutzverletzung, Audit‑Mängel. | Konkrete Log‑Policy definieren; technische Logs anonymisieren. |
| **Vertragliche Auftragsverarbeitung (AVV)** | Nutzung von Managed Services erfordert AVV mit Anbietern. | Fehlender AVV = Vertragsbruch. | AVV vor Go‑Live mit Provider abschließen. |

## Widersprüche und Unsicherheiten
- **Mobile‑First vs. Web‑First** – keine klare Entscheidung, erhöhtes Risiko für Scope‑Änderungen.
- **SSO‑Integration** – optional, aber unklare technische Umsetzung und Aufwand.
- **Kosten‑Schätzung für EU‑Only Hosting** – noch nicht quantifiziert, beeinflusst Budget.
- **Pilot‑Kunde (DE vs. CH)** – entscheidet über Mehrwährung, Datenschutz (CH) und Hosting‑Anforderungen.
- **API‑Gateway‑Verfügbarkeit** – Konflikt mit 8‑Wochen‑MVP‑Deadline.
- **Support‑Prozess** – Kontaktformular als MVP‑Lösung, aber hohe Unsicherheit über Bearbeitungsaufwand.
- **Freigabe‑Workflow für Rabatte** – wird erst nach MVP definiert, birgt finanzielles Risiko bei falscher Umsetzung.

## Gesamtbewertung & Priorisierung
1. **Kritisch**: SAP‑Verfügbarkeit, DSGVO‑Konformität (Double‑Opt‑In, Löschkonzept), API‑Gateway‑Ausfall, Daten‑Lösch‑/Auskunftsprozesse.
2. **Hoch**: Sicherheits‑Logging, Secrets Management, Backup/DR, Rate Limiting.
3. **Mittel**: Mehrwährung, Support‑Workflow, i18n, Rabatt‑Freigabe.
4. **Niedrig**: KPI‑Messung, erweiterte Analytics, vollständige Ticket‑System‑Integration.

*Alle Risiken leiten sich ausschließlich aus dem Transkript `input/transcripts/T9999_chaos.txt`, dem Kontext‑File `runs/phase2_1/20260528_145848_142676/state/context.md` und den Requirements in `docs/requirements.md` ab.*