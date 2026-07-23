# Offene Fragen und Klärungsbedarfe

## 1. Fachliche Fragen
- **Support‑Prozess**: Wie soll das Kontakt‑Formular technisch umgesetzt werden, damit Support‑Anfragen DSGVO‑konform erfasst und nachverfolgt werden? (Quelle: Transkript, Zeilen zu David)
- **Rabatt‑Freigabe‑Workflow**: Welcher genaue Schwellenwert (15 % vs. 20 % vs. 30 %) löst eine Freigabe durch Manager bzw. Finance aus? (Quelle: Eva‑Statements)
- **SAP‑Schreibzugriff**: Wird im MVP überhaupt Schreibzugriff benötigt (z. B. Bestell‑Übergabe) oder erst in Phase 2? (Quelle: Ben, Eva)
- **Kunden‑Löschkonzept**: Welche konkreten Schritte müssen umgesetzt werden, damit ein Kunde seine Daten vollständig löschen kann, während gesetzliche Aufbewahrungspflichten gewährleistet bleiben? (Quelle: Clara)
- **Retention‑Policy**: Wie lange müssen Angebote, Rechnungen und Log‑Einträge aufbewahrt werden (Handelsrecht vs. DSGVO)? (Quelle: Clara, Eva)
- **Mehrwährungs‑Support**: Ist CHF im MVP verpflichtend (Pilotkunde Schweiz) oder erst in Phase 2? Welche Währungs‑Konvertierungslogik ist nötig? (Quelle: Eva, Farid)
- **Internationalisierung**: Welche Sprachen sollen im MVP unterstützt werden (Deutsch, Englisch) und welche Regionen (DACH vs. EU vs. USA)? (Quelle: Anna, David)
- **KPI‑Tracking**: Wie soll das Tracking (Conversion Rate, Time‑to‑Offer) technisch realisiert werden, ohne personenbezogene Daten zu erfassen? (Quelle: Anna, Ben)

## 2. Technische Fragen
- **API‑Gateway‑Wahl**: Welcher Provider (AWS API GW, Azure APIM, Kong etc.) wird verwendet, um die 6‑Wochen‑Warteliste zu umgehen oder zu akzeptieren? (Quelle: Ben, Farid)
- **Managed Data Store**: Welche konkrete Datenbank/Blob‑Lösung (z. B. Azure Blob, AWS S3, PostgreSQL as a Service) erfüllt die Vorgabe „keine neue DB“ und ist DSGVO‑konform? (Quelle: Ben, Farid)
- **Secrets Management**: Welches Tool (HashiCorp Vault, Cloud‑Native Key‑Vault) wird für die Verwaltung von API‑Keys, DB‑Credentials usw. eingesetzt? (Quelle: Farid)
- **Backup & Disaster Recovery**: Welche RPO/RTO (Recovery Point/Object Time) sind für das MVP gefordert und welche Backup‑Strategie (daily snapshots, point‑in‑time) wird gewählt? (Quelle: Clara, Farid)
- **Rate‑Limiting & Abuse‑Detection**: Wie wird das einfache Rate‑Limiting (z. B. 100 Requests/min) technisch implementiert und welche Alerts sollen bei Missbrauch ausgelöst werden? (Quelle: Ben, Clara)
- **Test‑Umgebung & Datenmaskierung**: Wie werden SAP‑Testdaten pseudonymisiert, damit keine echten Personen‑daten in Entwicklungs‑ und CI‑Umgebungen landen? (Quelle: Ben, Clara)
- **Logging‑Trennung**: Wie wird physisch zwischen technischem Log (ohne PII) und Audit‑Log (mit Nutzer‑ID, Aktion) getrennt, um DSGVO‑Konformität zu sichern? (Quelle: Clara)
- **PDF‑Export & Dokumenten‑Versionierung**: Welche Bibliothek/Service wird für PDF‑Generierung und Versionierung verwendet, und wie wird die rechtliche Signatur bzw. Fußnote eingebunden? (Quelle: Eva)
- **Hosting‑Kosten**: Welche konkreten Kosten entstehen für ein EU‑Only Managed Service (Compute, Storage, Backup) und wie wird das im Budget‑Sheet dargestellt? (Quelle: Farid, Anna)

## 3. Widersprüche, die geklärt werden müssen
- **Mobile‑First vs. Web‑First**: Stakeholder diskutieren, ob Mobile‑App zuerst entwickelt werden soll, obwohl das MVP nur ein Web‑Portal vorsieht. (Quelle: Anna, Ben)
- **SSO (Azure AD/Google) vs. einfacher E‑Mail‑Login**: Unterschiedliche Erwartungen – SSO wäre praktisch, aber erhöht Aufwand und kostet ggf. Lizenz. (Quelle: Anna, Ben)
- **Support‑Erwartungen vs. MVP‑Umfang**: Support fordert ein Ticket‑System, während das MVP nur ein Kontakt‑Formular vorsieht. (Quelle: David, Anna)
- **Kosten‑Schätzung für Vorstand vs. fehlende Architektur‑Entscheidungen**: Vorstand verlangt Zahlen, aber Provider‑ und Datenbank‑Wahl ist noch offen. (Quelle: Anna, Farid)
- **Rabatt‑Freigabe vs. MVP‑Ausschluss von Sonderrabatten**: Finance verlangt klare Freigabeverfahren, während das MVP Sonderrabatte komplett ausschließt. (Quelle: Eva, Anna)

## 4. Fehlende Informationen / nächste Schritte
- **Stakeholder‑Priorisierung**: Welche Themen (Support, Mehrwährung, SSO) gelten als „must‑have“ für das MVP und welche dürfen nach Phase 2 verschoben werden? (Quelle: Gesamtes Transkript)
- **Rechtliche Vorgaben**: Konkretisierung von Aufbewahrungsfristen (Handelsrecht) und Löschfristen (DSGVO) durch die Rechts‑/Compliance‑Abteilung. (Quelle: Clara)
- **SAP‑SLA**: Welche Service‑Level‑Agreements gelten für Lesedaten (Verfügbarkeit, Latenz) und mögliche Schreib‑Erweiterungen? (Quelle: Ben)
- **Budget‑Freigabe**: Wie hoch ist das maximale Budget für Managed Hosting, API‑Gateway und ggf. externe Ticket‑System‑Lösung? (Quelle: Anna, Farid)
- **Performance‑Erwartungen**: Erwartete Nutzer‑Last (200 – 20 000 Concurrent Users) – welche Skalierungs‑Strategie muss bereits im MVP vorgesehen werden? (Quelle: Ben)

## 5. Mögliche Ansprechpartner / Rollen
- **Product Owner / Business Analyst** – Anna
- **Technical Lead / Architekt** – Ben
- **Compliance / Datenschutz** – Clara
- **Customer Support Lead** – David
- **Finance Lead** – Eva
- **IT Operations / Infrastruktur** – Farid
- **Legal / Contract** – ggf. externe Rechtsabteilung (für Auftragsverarbeitungsvertrag)

*Alle offenen Fragen leiten sich direkt aus den Transkripten, dem Kontext‑Artefakt und den bereits erstellten Requirements‑, Risks‑ und Architecture‑Dokumenten ab.*