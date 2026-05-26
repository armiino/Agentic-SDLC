# Risiken und Gegenmaßnahmen

## Fachliche Risiken
| Risiko | Beschreibung | mögliche Auswirkung | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|--------------------|--------------------------------|
| Unklare MVP‑Scope | Unterschiedliche Prioritäten (schnelle Feature‑Lieferung vs. Sicherheit & Compliance) führen zu ständigem Scope‑Wechsel. | Verzögerungen, Scope‑Creep, nicht erfüllte KPI‑Ziele. | Gemeinsame Priorisierung‑Workshop, klare Definition der MVP‑Inhalte, schriftliche Scope‑Freigabe. |
| Fehlende Produktdaten aus SAP | Stammdaten in SAP sind unvollständig, wodurch Angebote fehlerhaft sein können. | Unvollständige/fehlerhafte Angebote → geringere Conversion Rate. | Datenqualitäts‑Check in SAP, ggf. Daten‑Bereinigung vor Integration, fallback‑Mechanismus für fehlende Felder. |
| Unklare KPI‑Definition | Conversion Rate und „Zeit bis Angebot“ werden genannt, aber Messmethodik fehlt. | Keine Messbarkeit des Projekterfolgs. | KPI‑Definition erarbeiten, Messpunkte festlegen, Reporting‑Dashboard planen. |
| Fehlende Mobile‑Strategie | Diskussion über Mobile‑App vs. responsive Web bleibt offen. | Nutzer:innen können das Portal nicht wie gewünscht nutzen → geringere Adoption. | Entscheidung für Responsive Web im MVP, Mobile‑App als späteres Release planen. |
| Unzureichende Nutzer‑zugriffs‑ und Rollen‑Definition | Rollen (Admin, Manager, User, Support) werden genannt, aber kein detailliertes Berechtigungskonzept. | Unauthorisierte Datenänderungen, Compliance‑Verstöße. | Rollen‑ und Berechtigungsmatrix erstellen, Review durch Clara. |

## Technische Risiken
| Risiko | Beschreibung | mögliche Auswirkung | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|--------------------|--------------------------------|
| Backend‑API nicht API‑ready | Aktuell ist das Backend nicht API‑ready, aber ein API‑Layer wird gefordert. | Verzögerung beim Frontend‑Start, Integrationsprobleme. | Early‑Prototype des API‑Layers, Refactoring‑Plan, klare Schnittstellen‑Definition. |
| Fehlende Security Review innerhalb der 8‑Wochen | Security Review (6 Wochen) kollidiert mit MVP‑Zeitplan. | Sicherheitslücken, DSGVO‑Verstöße, Launch‑Verzögerung. | Parallel‑Security‑Sprint, Managed Security Services, Minimal‑Security‑Check vor Go‑Live, Nach‑Go‑Live Review. |
| Managed Service Auswahl (Kosten vs. Skalierbarkeit) | Günstige Managed DB vs. skalierbare Lösung; Konflikt zwischen Budget und Performance. | Performance‑Engpässe oder Budget‑Überschreitung. | Kosten‑Nutzen‑Analyse, Auswahl eines skalierbaren EU‑Managed DB (z. B. Serverless), Monitoring‑Setup. |
| Fehlende Authentifizierungs‑ und SSO‑Architektur | Wunsch nach SSO, aber kein zentrales IAM vorhanden. | Komplexe Implementierung, Verzögerungen, mögliche Sicherheitslücken. | Entscheidung für ein Identity‑Provider‑Modell (Azure AD), ggf. hybrider Ansatz (SSO nur für Admins). |
| Daten‑Hosting‑Region Unsicherheit | Anforderung EU‑only vs. DSGVO‑konform ist nicht eindeutig definiert. | Rechtsrisiken, mögliche Strafen, Vertrauensverlust. | Klare Vorgabe: Daten ausschließlich in EU‑Regionen (z. B. Azure EU‑West), Compliance‑Check. |
| Performance‑Skalierung von 200 bis 20 000 Nutzern | Unterschiedliche Last‑Szenarien erfordern Auto‑Scaling, aber kein Over‑Engineering. | Überlastung oder unnötige Kosten. | Nutzung von auto‑scaling Managed Services, Last‑Tests mit realistischen Szenarien, Monitoring. |
| Backup & Disaster Recovery nicht implementiert | Backup und DR werden erst diskutiert, aber nicht geplant für MVP. | Datenverlust, Nicht‑Erfüllung von Compliance. | Minimal‑Backup‑Plan (tägliches Snapshot), DR‑Test im Sprint, Dokumentation. |

## Compliance‑ und Datenschutzrisiken
| Risiko | Beschreibung | mögliche Auswirkung | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|--------------------|--------------------------------|
| Unvollständige DSGVO‑Umsetzung | Double‑Opt‑In, Lösch‑ und Export‑Feature, Audit‑Logs werden gefordert, aber kein konkretes Konzept. | Bußgelder, Rechtliche Schritte, Vertrauensverlust. | DSGVO‑Checkliste erstellen, Data‑Protection‑Officer einbinden, Privacy‑By‑Design umsetzen. |
| Fehlende Auftragsverarbeitungsverträge (AVV) | Integration von SAP‑Daten erfordert AVVs mit SAP‑Provider. | Vertragsverletzungen, Rechtsrisiken. | AVVs prüfen/abschließen, Dokumentation im Projektplan. |
| Unzureichende Logging‑ und Audit‑Trail‑Umsetzung | Logging wird gefordert, aber weder Umfang noch Aufbewahrungsdauer definiert. | Nicht‑nachweisbare Änderungen, Compliance‑Verstöße. | Logging‑Richtlinie definieren (z. B. 12 Monate), zentralisiertes Log‑Management (SIEM). |
| Fehlende Datenlöschung auf Kunden‑Anfrage | Prozess für Datenlöschung nicht definiert. | Verstoß gegen Art. 17 DSGVO, Bußgelder. | Lösch‑Workflow implementieren, automatisierte Lösch‑Requests im Portal. |
| Unsichere Datenübertragung bei optionaler SSO | Wenn SSO per Drittanbieter (Google) eingesetzt wird, muss OAuth‑Flow sicher sein. | Phishing, Credential‑Leak. | OAuth‑Flows nach OWASP‑Guidelines, regelmäßige Pen‑Tests. |

## Widersprüche und Unsicherheiten
| Punkt | Widerspruch / Unsicherheit | mögliche Auswirkung | Auflösung / Klärungsbedarf |
|-------|---------------------------|--------------------|----------------------------|
| Mobile vs. Web‑First | Anna: "Mobile später" vs. Ben: "Sales will mobil arbeiten". | Unklare Priorisierung kann Ressourcen binden. | Entscheidung im Scope‑Workshop; MVP: Responsive Web only. |
| SSO vs. fehlendes IAM | Wunsch nach SSO, aber kein zentrales IAM. | Implementierungsaufwand, Verzögerungen. | Auswahl eines externen IdP (Azure AD) oder Verzicht im MVP. |
| Security Review vs. 8‑Wochen‑MVP | Ben fordert 6‑Wochen‑Review, Anna will ohne Review starten. | Risiko von Sicherheitslücken, Compliance‑Verstoß. | Parallel‑Security‑Sprint, minimaler Review vor Go‑Live. |
| Hosting‑Region: EU‑only vs. DSGVO‑konform | Unterschiedliche Interpretation von "EU only". | Rechtsunsicherheit. | Klare Vorgabe: ausschließlich EU‑Regionen (z. B. Azure West Europe). |
| Umfang der KPIs | Nur Conversion Rate & Zeit bis Angebot genannt, keine Messmethodik. | Nicht‑Messbarer Erfolg. | KPI‑Definition und Reporting‑Setup finalisieren. |

## Gesamtbewertung & Priorisierung
- **Höchste Priorität**: DSGVO‑Konformität (Privacy‑By‑Design, Logging, Lösch‑Workflow), Security Review, API‑Readiness.
- **Mittlere Priorität**: SAP‑Datenqualität, Rollen‑/Berechtigungskonzept, Hosting‑Region Klarheit.
- **Niedrigere Priorität** (für MVP): Mobile App, Push‑Notifications, erweiterte KPI‑Analytics.

*Hinweis*: Weitere Risiken können entstehen, sobald detaillierte Architekturbeschreibungen und konkrete Dienstleister feststehen.
