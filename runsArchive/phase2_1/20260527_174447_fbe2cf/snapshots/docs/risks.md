# Risikoartefakt zum Kundenportal-Projekt

## 1. Fachliche Risiken

### 1.1 Unklare Zieldefinition und Scope-Änderungen
- Die Zieldefinition ist nicht final, z.B. Pilotkunde (DACH oder Schweiz) und dadurch unterschiedliche Datenschutz- und Währungsanforderungen.
- Mehrere Stakeholder haben unterschiedliche Anforderungen, z.B. Support, Rabattfreigabe, Analytics, Push, die nicht alle im MVP berücksichtigt werden können.

### 1.2 Supportprozess ist unklar und eingeschränkt
- Im MVP kein persistentes Ticketsystem, sondern nur ein Kontaktformular, was Supportqualität und Kundenzufriedenheit gefährden kann.
- Datenschutzrelevante Supportdaten werden per E-Mail manuell verarbeitet, was Risiken für DSGVO-Compliance erhöht.

### 1.3 Rabattfreigabeprozess unvollständig
- Im MVP sind Sonderrabatte nicht möglich, was zu Unzufriedenheit oder Workarounds führen kann.
- Kein klar definierter, automatisierter Freigabeworkflow für Rabatte im MVP.

## 2. Technische Risiken

### 2.1 SAP-Verfügbarkeit als kritische Abhängigkeit
- Angebotserstellung hängt von Echtzeitdaten aus SAP ab; SAP-Wartungsfenster oder Ausfälle können Angebotserstellung verhindern oder verlangsamen.
- Fehlender Fallback oder Caching-Strategie birgt Risiken für Datenkonsistenz und Datenschutz.

### 2.2 Fehlende API-Gateway-Verfügbarkeit im MVP
- Zentrales API Gateway hat 6 Wochen Warteliste; MVP kann somit nicht die bevorzugte Gateway-Lösung inkl. OAuth-Sicherung verwenden.
- Alternative Lösungen können weniger sicher, weniger skalierbar und wartungsintensiver sein.

### 2.3 Sicherheit und Compliance vs. Zeitrahmen
- Security Review dauert 6 Wochen, der MVP-Zeitplan ist 8 Wochen; Risiko, dass Sicherheitsprüfungen nicht rechtzeitig durchgeführt werden.
- Komplexe Sicherheitsfeatures wie OAuth, SSO, Audit Logs sind noch nicht final und könnten Zeitrahmen sprengen.

### 2.4 Hosting und Datenschutz
- EU-only Hosting mit nachweisbarer Datenresidenz ist gefordert, gleichzeitig ist die Kostenschätzung offen und höher als alternative Hosting-Modelle.
- Backup, Disaster Recovery und Secrets Management müssen zeitnah und konform umgesetzt werden.

### 2.5 Daten in Logs und Audit-Trails
- Unterschiedliche Anforderungen an technische Logs, Audit-Logs und Security Logs erschweren einheitliche Umsetzung.
- Gefahr, dass personenbezogene Daten ungewollt in technischen Logs landen und DSGVO-Verstöße verursachen.

### 2.6 Testdaten und Entwicklungsumgebung
- Nutzung realer Kundendaten in SAP-Testsystemen ohne Pseudonymisierung gefährdet Datenschutz.
- Pseudonymisierte oder synthetische Testdaten sind technisch aufwendig, aber unverzichtbar.

### 2.7 Performance und Skalierbarkeit
- Erwartete Nutzerzahlen variieren stark (20 bis 20.000), Gefahr von Overengineering oder unzureichender Skalierbarkeit.
- API Rate Limiting und Missbrauchserkennung sind im MVP nicht garantiert aufgrund fehlendem API Gateway.

## 3. Compliance- und Datenschutzrisiken

### 3.1 Widersprüchliche Anforderungen Löschung vs. Aufbewahrung
- Gesetzliche Aufbewahrungspflichten können Löschanfragen von Kunden widersprechen und erfordern differenzierte Datenklassifikation.

### 3.2 Fehlerhafte Datenverarbeitung im Support
- Manuelle E-Mail-basierte Supportprozesse erhöhen Risiko von Fehlern und Datenschutzverletzungen.

### 3.3 Datenschutz bei Push Notifications und Tracking nicht geregelt
- Push Notifications und Analytics mit Tracking sind im MVP ausgeschlossen, aber fehlen für langfristige Anforderungen.

### 3.4 Unklare Rollen- und Berechtigungskonzepte
- Widersprüche zwischen Anforderungen von Support, Sales und Finance bzgl. Zugriffsrechten auf Angebote und Rabatte.

## 4. Widersprüche und Unsicherheiten

- Zeitrahmen von 8 Wochen vs. notwendige Sicherheitsprüfungen und komplexe Anforderungen.
- Wunsch nach MVP mit umfangreichen Funktionen vs. technisch machbare und finanzierbare Lösung.
- Fehlende finale Entscheidungen zu Identity Management, Hosting-Anbieter, API-Sicherheitskonzept.
- Unklare Anforderungen an Pilotkunden (Geografie, Währung) und ihren Einfluss auf Datenschutz und Funktionalität.
- Konflikte zwischen schneller Angebotserstellung und Datenkonsistenz auf Basis SAP.
- Risiko von falschen Preisdaten bzw. nicht aktuellen Rabatten.

## 5. Mögliche Auswirkungen

- Verzögerungen bei MVP-Release aufgrund technischer oder organisatorischer Blockaden.
- Erhöhter Aufwand durch Nachbesserungen bei Sicherheits- und Datenschutzanforderungen.
- Finanzielle Risiken durch falsche Rabattfreigaben oder unvollständige Freigabeprozesse.
- Unzufriedene Kunden und Nutzer aufgrund eingeschränkter Supportmöglichkeiten.
- Compliance-Verstöße mit möglichen rechtlichen Konsequenzen.
- Mangelnde Skalierbarkeit und Performance können Nutzerakzeptanz mindern.

## 6. Mögliche Gegenmaßnahmen und Klärungsbedarfe

- Klare MVP-Definition und Scope-Freeze, um Zielkonflikte zu minimieren.
- Planung und frühe Einbindung des Security Reviews mit evtl. parallelen Reviews.
- Entwicklung eines minimal funktionsfähigen Supportprozesses mit klaren Datenschutzvorgaben.
- Priorisierung der SAP-Integration und Planung von Fallbackmechanismen.
- Frühzeitige Auswahl von Hosting-Provider mit EU-only Garantie und Kostenschätzung.
- Aufbau einer Datenklassifikation mit definierten Retention- und Löschregeln.
- Entscheidung über API-Gateway-Alternativen und OAuth/SO-Strategie mit Pflichtenheft.
- Erstellung von Testdaten-Richtlinien und sicherstellung der Pseudonymisierung.
- Stakeholder-Workshops zur Klärung von Rollen- und Berechtigungsanforderungen.
- Berücksichtigung von Skalierbarkeit und Performance in Architekturentscheidungen, Vermeidung von Overengineering.
- Dokumentation aller offenen Fragen und Widersprüche transparent halten.

---

*Dieses Risikoartefakt basiert auf den Eingabedaten in den Dateien runs/phase2_1/20260527_174447_fbe2cf/state/context.md und input/transcripts/T9999_chaos.txt im aktuellen Projektlauf.*