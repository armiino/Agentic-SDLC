# Risikoartefakt

## 1. Fachliche Risiken

| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|----------------------|---------------------------------|
| Unklarer Funktionsumfang (z. B. Mehrwährungs‑Support) | Entscheidung, ob CHF im MVP unterstützt wird, ist noch offen. | Verzögerungen bei Entwicklung und Test, mögliche Nacharbeiten nach Release. | Frühzeitige Entscheidung im Sprint‑Planning; ggf. Feature‑Toggle für optionale Währung. |
| Mobile‑First vs. Web‑First | Unterschiedliche Prioritäten zwischen Sales (Mobile) und Produkt (Web). | Ressourcenverschwendung, UX‑Inkonsistenzen, verpasste Marktchancen. | Klare Priorisierung im Product‑Backlog; ggf. separate Mobile‑Sprint nach MVP. |
| Rabatt‑Freigabe‑Workflow | Komplexer Freigabeprozess für Rabatte ≥ 15 % kann die Angebotserstellung verlangsamen. | Längere Angebotszeiten, Frustration bei Sales, niedrigere Conversion‑Rate. | Minimaler Freigabe‑Workflow implementieren, Automatisierung prüfen, klare UI‑Hinweise. |
| Support‑Prozess (Kontaktformular) | Kein Ticket‑System im MVP; unklare Nachverfolgung von Anfragen. | Unzufriedene Kunden, fehlende SLA‑Einblicke. | Definiere SLA‑Regeln für das Kontaktformular; später Ticket‑System einplanen. |

## 2. Technische Risiken

| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|----------------------|---------------------------------|
| Fehlendes API‑Gateway im MVP | Direkte Service‑Aufrufe ohne Gateway; späteres Nachrüsten nötig. | Sicherheitslücken (fehlende zentrale Auth), Skalierbarkeitsprobleme, Integrationsaufwand. | Ausführliche Sicherheits‑Reviews für direkte Aufrufe; frühe Planung des Gateways; ggf. Proxy‑Lösung. |
| Skalierbarkeit & Performance (200‑20 000 Nutzer) | Hohe Nutzerzahlen erfordern effektives Rate‑Limiting und Ressourcen‑Management. | Leistungsengpässe, langsame UI/Export, unzufriedene Nutzer. | Last‑Testing im Voraus; implementiere horizontale Skalierung über Managed Services; später Rate‑Limiting erweitern. |
| Backup & Recovery ohne Disaster‑Recovery‑Plan | Nur tägliche Backups, Wiederherstellung innerhalb 12 h. | Datenverlust bei Katastrophen, lange Ausfallzeiten. | Risiko‑Analyse des Datenverlusts; ggf. erweiterten DR‑Plan bereits im MVP‑Scope aufnehmen. |
| Secrets‑Management & Schlüsselrotation | Nutzung von Managed Services, jedoch keine klare Rotation‑Strategie erwähnt. | Sicherheitsvorfälle, Kompromittierung von API‑Keys. | Implementiere automatisierte Rotation via Cloud‑Provider; Dokumentation im Ops‑Handbuch. |

## 3. Compliance‑ und Datenschutz‑Risiken

| Risiko | Beschreibung | Mögliche Auswirkungen | Gegenmaßnahme / Klärungsbedarf |
|--------|--------------|----------------------|---------------------------------|
| Double‑Opt‑In‑Implementierung | Muss zwingend sein, aber enge Zeitplanung. | Verzögerungen, mögliche Nicht‑Konformität bei Release. | Frühzeitige Implementierung und Tests; rechtliche Review vor Go‑Live. |
| Datenresidenz (EU‑only) vs. Pilotkunde Schweiz | Unklar, ob Schweizer Daten ebenfalls in EU‑Regionen gespeichert werden dürfen. | Rechtsverletzungen, Bußgelder. | Klare Entscheidung über Datenstandorte; ggf. separate Hosting‑Umgebung für Schweiz. |
| Aufbewahrungs‑ vs. Löschfristen | Keine definierten Retention‑Regeln für Angebote, Rechnungen, Logs. | Verstoß gegen gesetzliche Aufbewahrungspflichten oder DSGVO‑Recht‑auf‑Vergessen‑werden. | Erarbeite Retention‑Policy (z. B. 10 Jahre für Rechnungen, 2 Jahre für Angebote) und automatisierte Löschroutinen. |
| Logging ohne personenbezogene Daten | Grundlegendes Logging kann unbeabsichtigt personenbezogene Daten enthalten. | Datenschutzverletzungen. | Logging‑Policy prüfen; Maskieren/Pseudonymisieren von PII in Logs. |

## 4. Widersprüche und Unsicherheiten

- **Security‑Review (6 Wochen) vs. 8‑Wochen‑MVP**: Der Review könnte Anpassungen erfordern, die den Zeitplan gefährden.
- **Budget vs. Feature‑Umfang**: Nutzung von Managed Services ohne neue DB, aber API‑Gateway und erweiterte Sicherheitsfeatures könnten das Budget sprengen.
- **Pilotkunde‑Entscheidung**: Währungs‑ und Datenschutz‑Implikationen hängen vom Kunden (DE vs. CH) ab.
- **Testdaten‑Strategie**: Nutzung echter SAP‑Daten vs. synthetische Daten ist offen, beeinflusst Testabdeckung und Compliance.

## 5. Mögliche Auswirkungen (übergreifend)

- **Verzögerungen im Release‑Zeitplan** wegen unklarer Entscheidungen, zusätzlicher Sicherheits‑Reviews oder fehlender Komponenten (API‑Gateway, DR‑Plan).
- **Kostenüberschreitung** durch unerwartete Infrastruktur‑Bedarfe (z. B. zusätzlicher Hosting‑Kosten für Schweiz).
- **Rechtliche Konsequenzen** bei Nichteinhaltung von DSGVO‑Pflichten (Double‑Opt‑In, Datenlöschung, Aufbewahrung).
- **Nutzer‑Unzufriedenheit** durch langsame Performance, fehlende Mobile‑Optimierung oder unklare Support‑Prozesse.
- **Sicherheitsvorfälle** durch fehlendes API‑Gateway und unzureichendes Secrets‑Management.

## 6. Erste Gegenmaßnahmen / Klärungsbedarfe

1. **Entscheidungs‑Roadmap** für Mehrwährungs‑Support und Pilotkunde bis zum Ende von Sprint 2.
2. **Security‑Review‑Plan** mit klaren Meilensteinen, um Review‑Ergebnisse rechtzeitig zu integrieren.
3. **Retention‑Policy** definieren und automatisierte Löschverfahren implementieren.
4. **API‑Gateway‑Roadmap**: Kurzfristige Proxy‑Lösung für Sicherheits‑Layer, langfristige Integration nach MVP.
5. **Backup‑ und DR‑Strategie** erweitern: Mindestens ein Hot‑Standby in einer zweiten EU‑Region prüfen.
6. **Secrets‑Management** etablieren: Nutzung von Cloud‑KMS mit automatischer Rotation.
7. **Support‑Definition**: SLA‑Richtlinien für Kontaktformular festlegen; später Ticket‑System einplanen.
8. **Performance‑Tests** bereits in Sprint 3 durchführen, um Skalierbarkeit zu validieren.
9. **Datenschutz‑Check** für Schweiz‑Pilot: ggf. separate Rechtsgrundlage oder Hosting‑Region definieren.
10. **Kommunikation**: Kontinuierlicher Abgleich zwischen Sales, Produkt und Technik zu Mobile‑First vs. Web‑First.

*Dieses Dokument stellt den initialen Entwurf des Risiko‑Artefakts dar und soll als Basis für weitere Detailanalysen und Priorisierungen dienen.*