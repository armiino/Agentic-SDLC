# Offene Fragen und Klärungsbedarf (Phase 2.1)

## 1. Fachliche Fragen
| ID | Frage | Quelle / Kontext | Priorität |
|----|-------|-------------------|-----------|
| F-1 | Welche genauen **Rabatt‑Grenzen** gelten im MVP und welche Freigabe‑Stufen (z. B. 15 % → Manager, 30 % → Finance) sollen später implementiert werden? | Transkript Zeilen 150‑168, 200‑220 | Hoch |
| F-2 | Wie soll das **Lösch‑ und Auskunfts‑Konzept** konkret aussehen (Retention‑Perioden, automatisierte Lösch‑Jobs, Umgang mit gesetzlicher Aufbewahrungspflicht)? | Transkript Zeilen 6‑8, 108‑112, 180‑190 | Hoch |
| F-3 | Welche **Kosten** fallen für ein reines **EU‑Only Managed Hosting** (Provider, Instanz‑Größe, Datentransfer) an? | Transkript Zeilen 102‑108, 140‑146 | Hoch |
| F-4 | Welcher **Auth‑Provider** soll langfristig genutzt werden (eigenes OAuth‑Server‑Setup, Keycloak, Azure AD, Google IdP) und welche Integration ist im MVP erforderlich? | Transkript Zeilen 45‑48, 80‑90 | Mittel |
| F-5 | Wie soll der **Support‑Prozess** nach dem MVP aussehen (Ticket‑System, Zuordnung zu Kundenkonten, DSGVO‑Konformität von Support‑E‑Mails)? | Transkript Zeilen 130‑144, 188‑200 | Mittel |
| F-6 | Welche **Währungs‑ und Länder‑Anforderungen** gelten für den Pilot‑Kunden (Schweiz CHF, ggf. EU‑EUR, später USA) und wie beeinflussen sie Preis‑ und Rechtskonformität? | Transkript Zeilen 210‑244 | Mittel |
| F-7 | Welche **KPIs** sollen wirklich gemessen werden (Conversion‑Rate, Zeit bis Angebot, Nutzer‑Engagement) und wie soll das Tracking technisch umgesetzt werden? | Transkript Zeilen 70‑78 | Niedrig |
| F-8 | Welche **Rollen‑ und Berechtigung‑Matrix** ist für Support‑Mitarbeiter, Sales und Finance notwendig (Einsicht vs. Edit‑Rechte für Rabatte, Angebote, Rechnungen)? | Transkript Zeilen 80‑100, 190‑210 | Hoch |
| F-9 | Wie wird das **Backup‑SLA** (RPO/RTO) konkret definiert und welche Tests sind dafür erforderlich? | Transkript Zeilen 122‑128 | Mittel |
| F-10 | Welche **Test‑Daten‑Strategie** wird verwendet (synthetische Daten, Pseudonymisierung) um das SAP‑Testsystem DSGVO‑konform zu nutzen? | Transkript Zeilen 250‑258 | Hoch |

## 2. Technische Fragen
| ID | Frage | Quelle / Kontext | Priorität |
|----|-------|-------------------|-----------|
| T-1 | Wie soll das **interim‑Mini‑Gateway** konkret implementiert werden (Kong, API‑Umbrella, Eigenentwicklung) bis das zentrale API‑Gateway verfügbar ist? | Transkript Zeilen 250‑258 | Hoch |
| T-2 | Welche **Rate‑Limiting‑Parameter** (Requests/s, Download‑Limits) sind realistisch für das MVP und wie werden sie technisch umgesetzt? | Transkript Zeilen 258‑266 | Mittel |
| T-3 | Welche **Secrets‑Management‑Lösung** (Cloud‑Provider‑Store, HashiCorp Vault) wird eingesetzt und wie wird sie in CI/CD integriert? | Transkript Zeilen 140‑152, 250‑260 | Hoch |
| T-4 | Wie wird das **Audit‑Log‑Schema** definiert (Felder, Aufbewahrungsdauer) um DSGVO‑Konformität zu gewährleisten? | Transkript Zeilen 108‑112, 176‑182 | Hoch |
| T-5 | Welche **Fallback‑Strategie** für SAP‑Ausfälle wird umgesetzt (Produkt‑Cache, statische Preisdaten, Grace‑Period)? | Transkript Zeilen 140‑152, 260‑270 | Mittel |
| T-6 | Wie wird das **Backup‑ und Disaster‑Recovery‑Verfahren** technisch automatisiert (Provider‑API, Terraform) und getestet? | Transkript Zeilen 122‑128 | Mittel |
| T-7 | Welche **Monitoring‑Metriken** (Latency, Error‑Rate, Audit‑Log‑Durchsatz) werden erfasst und wie wird sichergestellt, dass keine PII in Logs landet? | Transkript Zeilen 108‑112, 176‑182 | Mittel |
| T-8 | Wie wird das **PDF‑Template‑Management** (Versionierung, rechtliche Fußnoten) im MVP technisch umgesetzt? | Transkript Zeilen 84‑96, 236‑244 | Niedrig |
| T-9 | Welche **CI/CD‑Pipeline** (Tooling, Stages) wird genutzt, um Deployments in die EU‑Only Umgebung zu automatisieren? | Transkript Zeilen 250‑260 (implizit) | Niedrig |

## 3. Widersprüche / Klärungsbedarf
| ID | Widerspruch | Mögliche Auswirkung | Offene Entscheidung |
|----|-------------|---------------------|----------------------|
| W-1 | **Mobile‑First vs. Web‑First** – Diskussion über native App vs. responsive Web (Zeilen 1‑4, 45‑48). | Fehlende Priorisierung kann zu Over‑Engineering führen. | Klare Plattform‑Strategie (MVP: Web‑first, Mobile später). |
| W-2 | **SSO / Azure AD vs. Google vs. kein SSO** (Zeilen 80‑90). | Unterschiedliche Erwartungen an Authentifizierung, zusätzlicher Aufwand. | Entscheidung über SSO‑Integration nach MVP‑Release. |
| W-3 | **Budget‑Schätzung für EU‑Only Hosting** (Zeilen 140‑146) vs. aktuelle Annahme „günstig“. | Risiko von Budget‑Überschreitung und Projektverzögerung. | Kosten‑Kalkulation bis Freitag einholen. |
| W-4 | **Support‑Ticket‑System vs. Kontakt‑Formular** (Zeilen 130‑144). | Fehlende Nachverfolgbarkeit, DSGVO‑Risiko bei E‑Mail‑Daten. | Kurzfristig Prozess definieren, langfristig Ticket‑System planen. |
| W-5 | **Rabatt‑Freigabe‑Workflow** (keine Sonderrabatte im MVP) vs. Finance‑Anforderung (Zeilen 200‑220). | Finanzielle Risiken, mögliche Regelverstöße. | Klar definierte Grenze für MVP, Freigabe‑Logik später ergänzen. |
| W-6 | **Datenresidenz EU‑Only vs. Schweiz‑Kunde** (Zeilen 236‑244). | Rechtskonflikt bei Datenverarbeitung für Schweizer Kunden. | Entscheidung über separate CH‑Instanz oder vertragliche Ausnahmeregelung. |

## 4. Fehlende Informationen / Artefakte
| ID | Fehlendes Artefakt | Warum relevant |
|----|-------------------|----------------|
| M-1 | **Kosten‑Kalkulation EU‑Only Managed Service** (Provider‑Preise, Daten‑Transfer). | Grundlage für Budget‑Freigabe gegenüber Vorstand. |
| M-2 | **Detailliertes Lösch‑ und Retention‑Konzept** (gesetzliche Aufbewahrung vs. DSGVO). | Erfüllung rechtlicher Vorgaben. |
| M-3 | **Rollen‑ und Berechtigungs‑Matrix** (inkl. Support‑Rollen). | Verhindert unautorisierten Datenzugriff. |
| M-4 | **Finale KPI‑Definition und Tracking‑Plan**. | Messbarkeit des Projekterfolgs. |
| M-5 | **Entscheidungsvorlage für Auth‑Provider / SSO**. | Technische Umsetzung und Lizenz‑Kosten. |
| M-6 | **Backup‑SLA (RPO/RTO) und Test‑Plan**. | Sicherstellung der Disaster‑Recovery‑Fähigkeit. |
| M-7 | **Auswahl des Mini‑Gateway‑Produkts** (Kong, API‑Umbrella, Eigenentwicklung). | Architektur‑Entscheidung und Implementierungsaufwand. |
| M-8 | **Test‑Daten‑Strategie für SAP‑Umgebung** (Maskierungs‑Tool, Synthetic‑Data‑Generator). | Compliance‑Risiko bei Entwicklung. |

---
*Alle offenen Fragen, Widersprüche und fehlenden Artefakte wurden aus den bisherigen Artefakten (Context, Requirements, Risks, Architecture und dem Transkript) abgeleitet.*