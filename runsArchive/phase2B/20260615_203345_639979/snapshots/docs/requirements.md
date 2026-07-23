## Functional Requirements

- **Login & Identity Management**: Der Nutzer muss sich per E‑Mail/Passwort anmelden können, mit Double‑Opt‑In zur Bestätigung. Optional muss eine SSO‑Anbindung (Azure AD oder Google) unterstützt werden, wenn dies im Projekt beschlossen wird.
- **Rollen‑ und Berechtigungskonzept**: Es sind die Rollen *Admin*, *Sales*, *Manager*, *Support* und *Kunde* zu definieren. Rollen bestimmen die Sichtbarkeit von Preisen, Rabatten und Angebotsstatus.
- **Angebots‑Workflow**: Ein Angebot durchläuft die Stati *Draft*, *Pending*, *Approved* und *Rejected*. Discount‑Freigaben: 
  - >15 % Rabatt erfordert Freigabe durch einen *Manager*.
  - >30 % Rabatt erfordert Freigabe durch *Finance* (Eva).
- **SAP‑Integration (Lesen)**: Das System muss Produkt‑, Preis‑ und Kundendaten aus dem bestehenden SAP‑System lesen können. Der Lese‑Zugriff muss nächtlich synchronisiert werden; Echtzeit‑Preisabruf ist ein offener Punkt.
- **Angebots‑PDF‑Export**: Angebote müssen als rechtssichere PDFs generiert, versioniert und zum Download bereitgestellt werden. Templates und rechtliche Fußnoten sind vorzugeben.
- **Rechnungs‑Anzeige & Download**: Kunden können ihre Rechnungen einsehen und als PDF herunterladen.
- **Audit‑Trail**: Jeder Erstellungs‑, Änderungs‑ und Freigabe‑Vorgang eines Angebots muss protokolliert werden (ohne PII in technischen Logs, separate Audit‑Logs).
- **Support‑Kontakt**: Ein Kontaktformular muss integriert werden; eine mögliche Erweiterung zu einem Ticket‑System ist noch offen.
- **API‑Layer / Gateway**: Das Backend stellt REST‑APIs bereit, geschützt via OAuth 2.0 (alternativ API‑Keys). Der zentrale API‑Gateway ist erwartungsgemäß nach 6 Wochen verfügbar – derzeit ein offener Punkt.
- **Internationalisierung**: UI in Deutsch und Englisch; Währungen EUR, CHF (später USD). Rechtskonforme Anpassungen für Schweiz/USA sind zu berücksichtigen.

## Non-functional Requirements

- **Performance & Skalierbarkeit**: Das System muss 200 bis 20 000 gleichzeitige Nutzer unterstützen, ohne über­engineering. Antwortzeiten < 2 s für Kern‑Funktionen.
- **Verfügbarkeit & Resilience**: 99,5 % Verfügbarkeit im Produktionsbetrieb; Backup‑ und Disaster‑Recovery‑Strategie für personenbezogene Daten.
- **Hosting & Datenresidenz**: EU‑only Managed Hosting wird gefordert; Daten dürfen nicht außerhalb der EU gespeichert werden.
- **Datenschutz & DSGVO**: Double‑Opt‑In, Löschkonzept, Datenminimierung, Auftrags‑Verarbeitungs‑Vertrag, getrennte technische und Audit‑Logs ohne PII.
- **Security**: OAuth 2.0 für API‑Zugriff, optional API‑Key fallback, Rate‑Limiting zum Schutz vor Missbrauch beim Rechnungs‑Download.
- **Monitoring & Logging**: Technische Logs ohne PII, Audit‑Logs mit vollständiger Nachverfolgbarkeit; Monitoring zur Fehlermeldung und Missbrauchserkennung.
- **Testing & Environments**: Separate Dev/Test/Prod‑Umgebungen; Testdaten aus SAP müssen pseudonymisiert sein.

## Constraints/Compliance

- **EU‑Only Datenhaltung**: Alle Daten müssen innerhalb der EU gespeichert werden (Farid).
- **DSGVO‑Konformität**: Double‑Opt‑In, Recht auf Vergessenwerden, Auditing, minimale Datenspeicherung (Clara).
- **Legal Requirements für PDFs**: Rechtssichere Fußnoten und Versionierung (Eva).
- **Hosting‑Kosten**: EU‑only Managed Service ist teurer; Budget‑Entscheidung noch offen (Farid, Anna).
- **API‑Gateway Verfügbarkeit**: Erwartete Wartezeit 6 Wochen; darf den MVP‑Zeitplan nicht gefährden (Ben, Farid).

## Assumptions and Open Points

- **Frontend‑Strategie**: Ob ein Web‑first oder Mobile‑first Ansatz verfolgt wird, ist noch nicht entschieden (Anna ↔ Ben).
- **SSO‑Provider**: Auswahl zwischen Azure AD, Google oder keiner SSO‑Lösung ist offen (Anna ↔ Ben).
- **Echtzeit‑Preis‑Abfrage**: Aktuell ist nur nächtliche SAP‑Synchronisation geplant; Echtzeit‑Preis‑Abruf muss evaluiert werden (Ben).
- **API‑Gateway**: Verfügbarkeit nach 6 Wochen ist angenommen, aber nicht gesichert (Farid, Ben).
- **Discount‑Freigabe‑Workflow**: Konkrete Rollen und Schwellenwerte (15 % / 30 %) wurden genannt, aber die endgültige Genehmigungslogik muss noch definiert werden (Eva).
- **Support‑Prozess**: Derzeit nur Kontaktformular, mögliche Erweiterung zu Ticket‑System unklar (David).
- **Pilot‑Kunde**: Zielmarkt (DE vs. CH) ist offen; beeinflusst Währung und rechtliche Vorgaben (Anna, David, Eva).
- **Hosting‑Kosten‑Decision**: Finaler Provider und Kostenmodell stehen noch aus (Farid, Anna).
- **Rate‑Limiting Details**: Konkrete Schwellenwerte und Implementierungsdetails fehlen (David, Ben).
- **Caching bei SAP‑Ausfall**: Konzept diskutiert, aber Datenschutz‑Risiko noch offen (Ben, Clara).

## Traceability

| Requirement | Herkunft (Stakeholder / Quelle) |
|-------------|-----------------------------------|
| Login & Double‑Opt‑In | Anna (Projektziel), Clara (DSGVO) |
| SSO‑Optionen | Anna, Ben (Diskussion) |
| Rollen‑ und Berechtigungskonzept | Anna, Ben, Eva |
| Angebots‑Workflow & Status | Anna, Eva |
| Rabatt‑Freigabe >15 % / >30 % | Eva |
| SAP‑Read‑Integration | Ben |
| PDF‑Export & rechtliche Fußnoten | Eva, Farid |
| Audit‑Trail | Clara, Anna |
| Support‑Kontaktformular | David |
| API‑Layer & OAuth | Ben, Farid |
| Internationalisierung (Sprachen/Währungen) | Anna, David, Eva |
| Performance & Skalierbarkeit | Ben, Anna |
| EU‑Only Hosting | Farid, Anna |
| DSGVO‑Compliance (Double‑Opt‑In, Löschkonzept) | Clara, Anna |
| Backup/DR | Farid |
| Monitoring & Logging | Clara |
| Rate‑Limiting | David, Ben |
| Test‑Daten‑Pseudonymisierung | Farid, Ben |
| Frontend‑Strategie Entscheidung | Anna, Ben |
| API‑Gateway Wartezeit | Farid, Ben |
| Echtzeit‑Preis‑Abruf | Ben |
| Support‑Ticket‑System | David |
| Pilot‑Kunde Auswahl | Anna, David, Eva |
| Hosting‑Kosten‑Entscheidung | Farid, Anna |
| Caching‑Strategie bei SAP‑Ausfall | Ben, Clara |

*Alle Angaben basieren auf dem MAF‑Shared‑State: Projektkontext (oben).*
