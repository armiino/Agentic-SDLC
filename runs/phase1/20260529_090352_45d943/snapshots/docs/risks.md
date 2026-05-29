# Risiken

## Technische Risiken
1. **API‑Gateway‑Verfügbarkeit** – Das zentrale API‑Gateway hat eine 6‑Wochen‑Warteliste. Ohne Zugriff kann das MVP nicht die geplante OAuth‑Absicherung nutzen und muss auf einen internen Proxy ausweichen, was Zeit und Sicherheit reduziert.
2. **SAP‑Abhängigkeit** – Das System ist stark von SAP‑Lesezugriffen abhängig. SAP‑Wartungsfenster oder Ausfälle verhindern Angebotserstellung und Preisaktualität.
3. **Backup & Disaster Recovery** – Fehlende getestete Backup‑Strategie kann bei Datenverlust die Wiederherstellungszeit (RTO) überschreiten.
4. **Managed‑Service‑Kosten** – EU‑only Managed Hosting kann teurer sein als geplant, was das Budget gefährdet.
5. **Rate‑Limiting & Missbrauch** – Ohne implementiertes Rate‑Limiting besteht das Risiko von DDoS‑Angriffen oder Datenexfiltration bei massiven Rechnungsdownloads.
6. **Secrets Management** – Fehlende zentrale Verwaltung von API‑Keys und Datenbank‑Credentials erhöht das Risiko von Credential‑Leaks.
7. **Logging‑Compliance** – Technische Logs dürfen keine personenbezogenen Daten enthalten; falsche Implementierung kann DSGVO‑Verstöße auslösen.
8. **Umgebungs‑Daten** – Nutzung von echten Kundendaten in Dev/Test‑Umgebungen kann zu Datenschutzverstößen führen.

## Organisatorische Risiken
1. **Unklare Rollen‑ und Berechtigungskonzepte** – Unterschiedliche Erwartungen (Sales, Support, Finance) an Zugriffsrechte können zu Fehlkonfigurationen und Datenlecks führen.
2. **Freigabe‑Workflow für Rabatte** – Keine definierte Schwelle im MVP führt zu finanziellen Risiken (falsche Preisgestaltung).
3. **Support‑Prozess** – Fehlendes Ticket‑System erschwert Nachverfolgung von Kundenanfragen und kann SLA‑Verletzungen verursachen.
4. **Pilotkunde‑Entscheidung** – Unklare Ziel‑Pilotkunde (Deutschland vs. Schweiz) beeinflusst rechtliche Anforderungen (DSGVO vs. DSG‑VO) und Währungsunterstützung.
5. **Kosten‑Schätzung** – Ohne klare Architektur‑Entscheidungen ist die Kostenschätzung für den Vorstand unsicher, was Budget‑Genehmigungen gefährden kann.
6. **Zeitplan‑Druck** – 8‑Wochen‑MVP mit umfangreichen Compliance‑ und Sicherheitsanforderungen kann zu Qualitäts‑ und Sicherheitskompromissen führen.

## Finanzielle Risiken
1. **Sonderrabatte ohne Freigabe** – Wenn Sonderrabatte im MVP erlaubt werden, kann das zu unerwarteten Umsatzverlusten führen.
2. **Hosting‑Kosten** – EU‑only Managed Services können das geplante Budget überschreiten.
3. **Zusätzliche Lizenz‑Kosten** – OAuth‑Provider, Monitoring‑Tools oder externe PDF‑Generatoren können zusätzliche Lizenzkosten verursachen.

## Compliance‑Risiken
1. **DSGVO‑Verletzungen** – Fehlendes Double‑Opt‑In, unzureichendes Löschkonzept oder personenbezogene Daten in Logs können zu Bußgeldern führen.
2. **Schweizer Datenschutz** – Pilotkunde aus der Schweiz erfordert zusätzliche Datenschutz‑Maßnahmen, die im MVP nicht berücksichtigt werden.
3. **Aufbewahrungspflichten vs. Recht auf Löschung** – Konflikt zwischen gesetzlichen Aufbewahrungsfristen (Handelsrecht) und DSGVO‑Löschanforderungen.
4. **Datenresidenz** – Nicht‑nachweisbare EU‑Datenresidenz kann rechtliche Konsequenzen nach sich ziehen.

## Risikominderungs‑Maßnahmen (Kurz‑ und Mittelfrist)
- **API‑Gateway**: Temporärer interner Reverse‑Proxy mit späterer Migration.
- **SAP‑Fallback**: Cache‑Strategie nur für Produkt‑ und Preisdaten (keine Kundendaten), mit kurzer TTL.
- **Backup**: Tägliche Snapshots über Managed Service, RPO < 24 h, RTO < 4 h.
- **Secrets**: Nutzung von Cloud‑KMS oder HashiCorp Vault (Managed Service) bereits im MVP.
- **Logging**: Trennung von Audit‑ und technischen Logs, Maskierung personenbezogener Felder.
- **Testdaten**: Generierung synthetischer Testdatensätze, keine echten Kundendaten.
- **Rollen‑Workshop**: Frühzeitiger Abstimmungs‑Workshop mit Sales, Finance, Support und IT.
- **Freigabe‑Workflow**: Definieren einer klaren Schwelle (z. B. 15 % Rabatt) und Implementierung eines einfachen Genehmigungs‑Buttons im MVP.
- **Support‑Kontaktformular**: Eingebettetes Formular mit automatischer Zuordnung zu Kundenkonten, Speicherung in gesicherten DB‑Tabellen (Managed Service).
- **Kosten‑Schätzung**: Grobe Schätzung basierend auf Managed‑Service‑Preisen (EU‑Region, PostgreSQL, Backup, Monitoring) bis Freitag bereitstellen.
