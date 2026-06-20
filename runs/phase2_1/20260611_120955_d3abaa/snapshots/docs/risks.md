# Risikoanalyse Kundenportal MVP

## 1. Fachliche Risiken

### Unklare Scope-Definition und Zielkonflikte
- Der MVP-Umfang ist sehr groß und umfasst viele unterschiedliche Funktionen (Angebotserstellung, Rechnungsanzeige, SAP-Integration, Rollen und Berechtigungen, Supportprozess etc.).
- Zielkonflikte zwischen schneller Umsetzung (8 Wochen MVP) und notwendiger Qualität, Compliance und Sicherheit.
- Unklare Priorisierung von Features, insbesondere Support (Kontaktformular ja, Ticketsystem nein) und Rabatt-Freigabe (erst später).
- Risiko, dass unvollständiger oder chaotischer Scope zu Verzögerungen oder unzufriedenen Stakeholdern führt.

### Pilotkunde und Zielregion unklar
- Unklarheit über Pilotkunden (Deutschland vs. Schweiz) beeinflusst Datenschutz, Mehrsprachigkeit, Währung und rechtliche Anforderungen.
- Risiko von Mehrkosten oder nicht erfüllten Anforderungen bei falscher Vorannahme.

### Supportprozess eingeschränkt
- Im MVP ist nur ein Kontaktformular geplant, kein strukturiertes Ticketsystem.
- Risiken bei Nachverfolgbarkeit, Bearbeitbarkeit und Datenschutz von Supportanfragen.

## 2. Technische Risiken

### SAP-Integration
- SAP-Datenqualität und Verfügbarkeit sind teilweise unklar.
- Keine Echtzeit-Verfügbarkeit von Preisen und Rabatten, Nachtupdates könnten falsche Angebote erzeugen.
- Fehlende SAP-Schreibzugriffe blockieren automatisierte Prozesse (z.B. Auftragsanlage).

### Infrastruktur und Hosting
- EU-only Hosting wird gefordert, aber Kosten und Verfügbarkeitsrisiken sind unklar.
- Zentrales API-Gateway hat sechs Wochen Warteliste, könnte MVP verzögern.
- Kein neuer Datenbankserver, Managed Services müssen passen.
- Backup, Disaster Recovery und Sicherheit sind zeitlich eng und in der Umsetzung anspruchsvoll.

### Skalierbarkeit und Performance
- Unklare Benutzerzahlen von 200 bis 20.000 erfordern flexible Skalierung.
- Performance bei Angebotserstellung und Rechnungsanzeige ist kritisch für Nutzerakzeptanz.

### API-Sicherheit und Authentifizierung
- Noch nicht finalisiertes Authentifizierungsverfahren (OAuth vs. API Keys).
- Komplexität von OAuth könnte Zeitplan sprengen.

### Logging und Audit
- Anforderung an Audit und Logging für Compliance und Nachvollziehbarkeit kann hohen Implementierungsaufwand verursachen.
- Technische Logs dürfen keine personenbezogenen Daten enthalten, Risiko von Datenschutzverletzungen.

## 3. Compliance- und Datenschutzrisiken

### DSGVO-Konformität
- Notwendigkeit von Double-Opt-In beim Login.
- Löschkonzepte müssen implementiert werden, stehen jedoch im Widerspruch zu gesetzlichen Aufbewahrungspflichten.
- Datenschutz bei Supportanfragen per E-Mail unklar und potenziell riskant.
- Datenresidenz muss EU-only nachweisbar sein, Einhaltung ist technisch und organisatorisch herausfordernd.

### Aufbewahrungspflichten vs. Löschrechte
- Konflikt zwischen handelsrechtlichen Aufbewahrungsfristen (z.B. Angebote, Rechnungen) und Löschanfragen der Nutzer.
- Risiko fehlender rechtlicher Absicherung bei nicht konformer Datenhaltung.

### Sicherheit
- Verzögerung oder fehlender Security Review stellt Risiko dar.
- Verschlüsselung ist auf TLS beschränkt, Ende-zu-Ende-Verschlüsselung nicht realistisch.
- Secrets Management und Zugangskontrolle müssen stringent sein, um Missbrauch zu verhindern.

## 4. Widersprüche und Unsicherheiten

- Zeitlicher Zielkonflikt: 8 Wochen MVP vs. notwendige Security Reviews, Testing und Dokumentation.
- Technische Machbarkeit vs. funktionale Anforderungen (z.B. OAuth, API Gateway-Verfügbarkeit).
- Compliance-Anforderungen vs. notwendige Funktionalität und Supportprozesse.
- Unklare Verantwortlichkeiten und fehlende Architektur- und Technologieentscheidungen erhöhen Projektunsicherheit.
- Umfang und Tiefe der Dokumentation sind nicht klar definiert.

## 5. Mögliche Auswirkungen

- Verzögerungen beim Release durch technische oder Compliance-Probleme.
- Erhöhte Kosten durch Nacharbeiten oder Änderungen im Projektverlauf.
- Datenschutzverstöße mit rechtlichen und finanziellen Konsequenzen.
- Unzufriedenheit bei Stakeholdern aufgrund unvollständiger Funktionen oder schlechter Benutzererfahrung.
- Sicherheitsvorfälle durch unzureichend abgesicherte APIs oder mangelndes Logging.

## 6. Mögliche Gegenmaßnahmen und Klärungsbedarfe

- Klare Scope- und Prioritätsdefinition durch alle Stakeholder, inklusive bewusster Abgrenzung des MVP.
- Klärung des Pilotkunden und Zielregion, um Anforderungen transparent zu machen.
- Definierte Supportprozesse auch mit temporären Lösungen, um Datenschutz und Nachvollziehbarkeit zu gewährleisten.
- Frühzeitige Entscheidung und Planung des API-Gateways sowie Authentifizierungsmechanismen.
- Einbindung eines Architekten oder technischen Leiters zur Koordination und Dokumentation.
- Priorisierung von Security Review und Compliance-Themen mit ggf. externem Support.
- Nutzung von Managed Services zur Reduktion von Infrastrukturaufwand.
- Einrichtung von Testumgebungen mit pseudonymisierten Daten zur Einhaltung der Datenschutzanforderungen.
- Kontinuierliche Risikoüberwachung und Eskalationsprozesse bei Verzug oder Compliance-Verstößen.

---

*Dieses Risikoartefakt basiert auf der Analyse des Transkripts input/transcripts/T9999_chaos.txt und den damit zusammenhängenden Kontext- und Requirements-Dokumenten.*