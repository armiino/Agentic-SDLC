## Functional Requirements
- **Login**: Nutzer können sich per E‑Mail und Double‑Opt‑In authentifizieren. Optionales Single‑Sign‑On (SSO) via Azure AD und/oder Google wird als Erweiterung definiert, jedoch nicht zwingend im MVP.
- **Rollen‑ und Berechtigungskonzept**: Vier Rollen werden bereitgestellt – Admin, Sales, Kunde, Manager (ggf. Support). Jede Rolle hat definierte Zugriffsrechte auf Angebote, Rechnungen und administrative Funktionen.
- **Angebots‑Workflow**: Das System unterstützt die Zustände Draft → Pending Approval → Approved → Sent. Änderungen werden protokolliert.
- **Rechnungs‑Download**: Kunden können PDFs ihrer Rechnungen herunterladen.
- **SAP‑Integration (Read‑Only)**: Das System greift lesend auf Produkt‑ und Preisdaten aus SAP zu. Schreibzugriff ist für spätere Phasen vorgesehen.
- **Audit‑Trail & Logging**: Jeder relevanter Vorgang (Erstellung, Änderung, Freigabe) wird audit‑tauglich erfasst, ohne personenbezogene Daten in technischen Logs.
- **PDF‑Export & Dokumenten‑Versionierung**: Angebote und Rechnungen werden als PDF exportiert; Versionierung wird unterstützt.
- **Mehrsprachigkeit**: UI wird in Deutsch und Englisch bereitgestellt (weitere Sprachen optional später).
- **Mehrwährung**: Unterstützte Währungen im MVP: EUR und CHF. USD und weitere Währungen können später ergänzt werden.
- **Support‑Kontakt**: Ein Kontaktformular wird bereitgestellt; kein persistentes Ticket‑System im MVP.
- **API‑Layer & OAuth**: Externe Systeme können via OAuth‑basiertem API‑Layer auf Angebots‑ und Rechnungs‑Daten zugreifen.

## Non-functional Requirements
- **Performance**: UI‑Reaktionszeit ≤ 2 s bei normalen Lasten; API‑Antwortzeit ≤ 500 ms.
- **Verfügbarkeit**: 99,5 % Uptime im Produktionsbetrieb.
- **Sicherheit**: TLS 1.2+ für alle Netzwerkverbindungen; OAuth für API‑Zugriff; keine End‑to‑End‑Verschlüsselung erforderlich im MVP.
- **Datenschutz / DSGVO**: Double‑Opt‑In für Nutzerregistrierung; Lösch‑ und Auftragsverarbeitungs‑Verträge; keine personenbezogenen Daten in technischen Logs; Datenaufbewahrung gemäß rechtlicher Vorgaben.
- **Hosting**: EU‑only Managed Hosting, inkl. regelmäßiger Backups und Disaster‑Recovery (RPO/RTO werden noch definiert).
- **Scalability**: System muss mindestens 100 gleichzeitige Nutzer*innen unterstützen; horizontale Skalierung über Container/Orchestrierung vorgesehen.
- **Testing**: Test‑Umgebungen (Dev/Test/Prod) mit pseudonymisierten Testdaten.
- **Rate‑Limiting & Missbrauchserkennung**: Grundlegende API‑Rate‑Limits (z. B. 100 Requests/Minute pro Nutzer) werden implementiert; konkrete Schwellenwerte noch offen.
- **Caching**: Produkt‑ und Preis‑Daten können gecached werden; Kundendaten‑Cache nur nach GDPR‑Prüfung erlaubt.

## Constraints/Compliance
- **Zeitplan**: MVP muss innerhalb von 8 Wochen lieferbar sein.
- **Budget**: Keine Einführung einer neuen Datenbank vorgesehen („keine neue DB“).
- **Compliance**: Einhaltung von DSGVO, EU‑only Hosting, Audit‑Trail, Double‑Opt‑In, Löschkonzept.
- **SAP‑Abhängigkeit**: Das System ist von der Verfügbarkeit des SAP‑Systems abhängig; kein Fallback definiert.
- **Security Review**: Geplanter Security Review dauert 6 Wochen – kann das MVP‑Release beeinflussen.

## Assumptions and Open Points
- **Mobile‑First vs. Web‑First**: Priorität für das MVP ist derzeit Web‑First; mobile Optimierung ist optional.
- **SSO‑Provider**: Entscheiden zwischen Azure AD, Google oder beiden – noch offen.
- **API‑Gateway**: Verzögerung von 6 Wochen; ein interimistisches Gateway wird erwogen, Entscheidung steht aus.
- **Hosting‑Kosten**: Kosten für EU‑only Managed Hosting sind noch nicht quantifiziert.
- **Pilot‑Kunde**: Auswahl zwischen Schweiz (Müller AG) und Deutschland (Hansa GmbH) beeinflusst Währung und Datenschutz‑Betrachtung.
- **Rabatt‑Freigabe**: Schwellenwerte (15 % / 20 % / 30 %) und Verantwortlichkeiten sind noch zu klären.
- **Support‑Integration**: Kontakt‑Formular ohne Persistenz vs. Ticket‑System – Entscheidung offen.
- **Backup‑ und DR‑Strategie**: Konkrete RPO/RTO‑Werte noch nicht definiert.
- **Logging vs. Audit‑Logging**: Trennung und Aufbewahrungsfristen müssen präzisiert werden.
- **Test‑Daten in SAP**: Nutzung synthetischer vs. realer Daten – rechtliche Bewertung ausstehend.
- **Rate‑Limiting Schwellenwerte**: Konkrete Werte für Download‑ und API‑Nutzung noch offen.
- **Caching‑Strategie**: Welche Daten dürfen ohne DSGVO‑Risiko gecached werden – noch zu bestimmen.
- **Weitere Sprachen / Währungen**: FR, IT, USD etc. geplant für zukünftige Phasen.
- **PDF‑Template‑Management**: Versionierung und rechtliche Fußnoten müssen noch spezifiziert werden.

## Traceability
- **Login** – PO Anna, Lead Engineer Ben (Projektziel, Stakeholder‑Liste).
- **Rollen & Berechtigungen** – PO Anna, Lead Engineer Ben (Stakeholder‑Tabelle).
- **Angebots‑Workflow** – PO Anna, Finance Eva (Unterziel, Prozessdefinition).
- **Rechnungs‑Download** – PO Anna, Finance Eva (Unterziel).
- **SAP‑Integration** – Lead Engineer Ben, Finance Eva (Fachliche Themen).
- **DSGVO / Compliance** – Clara (Compliance & Data‑Protection), PO Anna (Projektziel).
- **Security Review** – Ben, Clara (Risiken, Security Review Dauer).
- **Hosting & EU‑only** – Farid (IT Operations), Clara (Compliance).
- **Mehrsprachigkeit & Mehrwährung** – PO Anna, Finance Eva (Fachliche Themen).
- **Open Points** – Alle offenen Fragen aus der Tabelle „Offene Fragen / Unsicherheiten“ referenzieren das Transkript T9999_chaos.txt.
