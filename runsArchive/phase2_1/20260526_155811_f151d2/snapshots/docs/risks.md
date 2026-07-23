# Risikobericht für das Kundenportal‑MVP

## 1. Fachliche Risiken
| Risiko | Beschreibung | Auswirkung | Mögliche Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|------------|------------------------------------------|
| Unklare Zielgruppe (DACH vs. Schweiz) | Pilot‑Kunde und Markt‑Region (EUR vs. CHF) sind nicht festgelegt. | Fehlende Währungs‑/Steuer‑Logik, falsche Hosting‑Compliance (EU‑only vs. Schweiz). | Entscheidung über Zielregion und Währung bis Sprint‑1‑Planung; ggf. optionale CHF‑Unterstützung als Feature‑Toggle. |
| Fehlende Rabatt‑Freigabe im MVP | Sonderrabatte > 15 % dürfen nicht erstellt werden, aber Sales könnte sie manuell eingeben. | Finanzielle Verluste, Vertragsrisiken. | UI‑Validierung, Business‑Rule‑Check im Backend, klare Kommunikation, später automatisierter Freigabe‑Workflow. |
| Support‑Prozess nur Kontaktformular | Keine Ticket‑Persistenz, manuelle Excel‑Listen. | Verzögerte Bearbeitung, Datenverlust, Compliance‑Risiko (personenbezogene Daten in E‑Mails). | Bewusste Einschränkung im MVP, aber ASAP ein strukturiertes Ticket‑System oder CRM‑Integration planen. |
| KPI‑Erfassung ohne Analytics‑Infra | Conversion‑Rate wird nur client‑seitig erfasst. | Unzuverlässige Daten, Fehlentscheidungen. | Minimal‑Tracking implementieren, später externe Analytics (z. B. Matomo) einbinden. |
| Anforderungen an Mehrwährung | Kunden aus Schweiz benötigen CHF, evtl. spätere USD‑Unterstützung. | Preis‑Inkonsistenzen, rechtliche Folgen. | MVP auf EUR beschränken, Flag für CHF‑Pilot, später Währungs‑Engine. |
| Unklare Freigabe‑Workflow‑Details | Rollen‑ und Genehmigungslogik für Rabatte unbestimmt. | Verzögerungen, fehlerhafte Angebote. | Definition von Rollen‑Matrix und Status‑Übergängen im Detail (Phase 2). |

## 2. Technische Risiken
| Risiko | Beschreibung | Auswirkung | Mögliche Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|------------|------------------------------------------|
| SAP‑Verfügbarkeit (kritische Abhängigkeit) | Das Portal liest Produkt‑/Preis‑Daten aus SAP; bei Ausfall kann kein Angebot erstellt werden. | System‑Ausfall, negative Nutzer‑Erfahrung. | UI‑Hinweis bei SAP‑Fehler, Cache‑Strategie für Preis‑Stammdaten (nur Produkt, keine Rabatte) prüfen. |
| Verzögerte API‑Gateway‑Integration | Zentrales API‑Gateway hat 6‑Wochen‑Warteliste. | Keine standardisierte Zugriffskontrolle, höherer Aufwand für Eigen‑Proxy. | Kurzfristig eigen‑hosted Proxy (OAuth 2.0) einsetzen, Migration zum Gateway nach MVP planen. |
| Sicherheit‑Review dauert ≥ 6 Wochen | Vollständiger Security‑Review kann den 8‑Wochen‑Zeitplan gefährden. | Unzureichende Sicherheits‑Maßnahmen im Release. | Minimal‑Security‑Set (TLS, OAuth, Audit‑Log) implementieren, Review in Sprint 2 nach MVP. |
| Hosting‑Kosten EU‑Only | EU‑only Managed Service kann teurer sein als Standard‑Option. | Budget‑Überschreitung, Projekt‑Genehmigung riskieren. | Grobe Kostenschätzung bis Freitag (wie gefordert), ggf. Budget‑Freigabe einholen. |
| Keine neue Datenbank (Managed Service) | Nutzung von Managed DB kann Einschränkungen bei Customisation und Backup haben. | Risiko bei Datenverlust, Performance‑Grenzen. | Auswahl eines robusten EU‑Managed Service (z. B. PostgreSQL‑aaS), Backup‑Strategie (täglich, 24 h RPO). |
| Fehlende Trennung von Logging‑Typen | Technisches Logging enthält potenziell personenbezogene Daten. | DSGVO‑Verstoß, Bußgeldgefahr. | Klare Vorgabe: Technical Logs ohne PII, Audit‑Logs mit PII getrennt, Logging‑Policy definieren. |
| Secrets‑Management nicht etabliert | API‑Keys, DB‑Credentials werden manuell verwaltet. | Sicherheits‑Leak, unautorisierter Zugriff. | Einsatz eines Managed Secrets‑Service (z. B. HashiCorp Vault, Azure Key Vault) im MVP. |
| Testdaten‑Qualität (SAP‑Testsystem) | SAP‑Testumgebung enthält reale Kundendaten. | Datenschutz‑Verstöße im Test, Compliance‑Risiko. | Synthetic Testdaten erzeugen, Pseudonymisierung, oder Test‑SAP‑System ohne reale Daten nutzen. |
| Skalierbarkeit bis 2 000 Nutzer
| Architektur muss ohne große Änderungen skalieren. | Performance‑Probleme bei höherer Last. | Stateless Frontend, Container‑basiert, Load‑Balancer, horizontale DB‑Scaling‑Optionen prüfen. |
| Rate‑Limiting & Missbrauchserkennung | Ohne API‑Gateway keine integrierte Rate‑Limits; Gefahr von Daten‑Exfiltration. | Service‑Ausfall, Daten‑Leak. | Implementierung einfacher Rate‑Limiting im eigenen Proxy, Monitoring für ungewöhnliche Muster. |

## 3. Compliance‑ und Datenschutzrisiken
| Risiko | Beschreibung | Auswirkung | Mögliche Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|------------|------------------------------------------|
| Unvollständige Double‑Opt‑In Umsetzung | Double‑Opt‑In wird gefordert, aber Prozess nicht detailliert definiert. | Nicht‑konforme Einwilligungen, Bußgelder. | Konkretes Double‑Opt‑In‑Workflow (E‑Mail‑Bestätigung, Log) implementieren, Audit‑Log prüfen. |
| Daten‑Lösch‑ und Retention‑Konflikt | Gesetzliche Aufbewahrungspflicht vs. Recht auf Vergessenwerden. | Risiko von Verstößen, unklare Daten‑Lösch‑Prozesse. | Minimal‑Retention‑Plan (z. B. 2 Jahre) festlegen, Lösch‑Requests dokumentieren, später Verfahrensdokumentation. |
| Verarbeitung von Schweizer Kundendaten (nicht EU) | Schweiz hat eigenes Datenschutzgesetz. | EU‑Only Hosting kann nicht ausreichen, Vertragliche Prüfungen nötig. | Klare Entscheidung über Zielmarkt; ggf. Dual‑Hosting‑Strategie oder Zusatz‑AVV. |
| Auftrags‑Verarbeitungs‑Vertrag (AVV) fehlt | Integration mit SAP und Managed Service erfordert AVVs. | Nicht‑konforme Auftragsverarbeitung, Bußgelder. | AVVs für SAP‑Zugriff und Managed Service sofort prüfen, vor MVP finalisieren. |
| Fehlende Audit‑Logs für kritische Aktionen | Nur Minimal‑Audit‑Log implementiert. | Unzureichende Nachvollziehbarkeit bei Änderungen (z. B. Preis‑Änderungen). | Erweiterung des Audit‑Logs um Felder: User‑ID, Timestamp, Action, Entity. |
| Personenbezogene Daten in technischen Logs | Gefahr, dass Debug‑Logs PII enthalten. | DSGVO‑Verstoß. | Logging‑Policy, Anonymisierung, Review vor Deployment. |

## 4. Widersprüche und Unsicherheiten
- **Mobile‑First vs. Web‑First** – Unterschiedliche Erwartungen von Sales (mobile) und PO (Web). Entscheidung für MVP: Web‑First, responsive Design, Mobile optional später.
- **SSO vs. E‑Mail‑Login** – SSO (Azure AD/Google) wurde erwähnt, aber im MVP nur E‑Mail‑Login mit Double‑Opt‑In.
- **Preis‑ und Rabatt‑Logik** – SAP liefert aktuelle Preise, aber Rabatte können nachts aktualisiert werden – Risiko von falschen Angeboten.
- **Support‑Anfrage‑Verarbeitung** – Kontaktformular vs. Ticket‑System – bewusste Einschränkung, aber potenzielles Risiko für SLA und Datenschutz.
- **API‑Gateway vs. Eigen‑Proxy** – Unterschiedliche Lösungen, beide haben Vor‑ und Nachteile – klare Entscheidung erst nach MVP.
- **Backup‑Strategie** – Tages‑Backup definiert, aber RPO/RTO nicht exakt festgehalten – muss für Disaster‑Recovery‑Plan konkretisiert werden.

## 5. Mögliche Auswirkungen (Kurz‑ und Langfristig)
- **Projektverzögerung** – Wenn kritische technische Abhängigkeiten (SAP, API‑Gateway, Security‑Review) nicht rechtzeitig gelöst werden, kann das 8‑Wochen‑Ziel brechen.
- **Finanzielle Verluste** – Fehlende Rabatt‑Freigabe oder falsche Preisangaben können zu Verlusten oder Rechtsstreitigkeiten führen.
- **Compliance‑Verstöße** – Unvollständige DSGVO‑Umsetzung kann zu Bußgeldern und Reputationsschaden führen.
- **Kundenzufriedenheit** – Fehlender Support‑Ticket‑Prozess und mögliche SAP‑Ausfälle führen zu schlechter User‑Experience.
- **Technische Schulden** – Eigen‑Proxy statt API‑Gateway, manuelle Secrets‑Verwaltung, unzureichendes Monitoring können spätere Refactorings erforderlich machen.

## 6. Fazit & Priorisierung (empfohlen)
1. **SAP‑Verfügbarkeit & API‑Gateway‑Alternative** – Kritisch, muss sofort adressiert werden (Fallback‑Message & eigener Proxy).
2. **DSGVO‑Kernanforderungen** – Double‑Opt‑In, Audit‑Log, Secrets‑Management – unverzichtbar für MVP‑Go‑Live.
3. **Rabatt‑Freigabe‑Validierung** – UI‑Checks verhindern Finanzrisiken.
4. **Support‑Prozess‑Klärung** – Kontaktformular als Übergangslösung, aber schnelle Planung eines Ticket‑Systems.
5. **Hosting‑Kosten & Budget‑Schätzung** – Für Vorstand‑Präsentation bis Freitag benötigen.
6. **Backup & Disaster Recovery** – Tägliches Backup aktivieren, RPO/RTO definieren.

*Dieses Risiko‑Artefakt wurde ausschließlich aus dem bereitgestellten Transkript und dem Kontext‑Dokument abgeleitet.*