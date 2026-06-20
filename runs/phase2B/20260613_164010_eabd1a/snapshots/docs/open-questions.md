# Open Questions — Kundenportal / Angebotsportal (Initial Draft)

Quelle: input/transcripts/T9999_chaos.txt; Artefakte: docs/requirements.md, docs/risks.md, docs/architecture.md
run_id: 20260613_164010_eabd1a

Kurzbeschreibung
Dieses Dokument fasst offene fachliche und technische Fragen, Widersprüche und fehlende Informationen zusammen, die aus den bisherigen Artefakten ableitbar sind. Ziel ist eine priorisierte, quellenbegründete Liste zur Entscheidungsunterstützung (Steering Board / Architektenteam / Legal).

1) Priorisierte Kritische Fragen (Kurz)
- H1: API-Gateway-Zugang / temporäre Integrations-Alternative (Quelle: docs/risks.md, docs/architecture.md)
- H2: Umfang und Zeitplan des Security-Reviews (Quelle: docs/risks.md, docs/requirements.md)
- H3: Legal-Klärung: Retention-Dauern vs. Löschanfragen (Quelle: docs/requirements.md, docs/risks.md)
- H4: SAP-Scope & Verfügbarkeit von Testsystemen / Testdaten (Quelle: docs/requirements.md, docs/architecture.md)
- H5: Entscheidung Pilotmarkt (Schweiz vs. DACH) — Auswirkungen auf Währung, Verträge, ggf. lokale Hosting-Vorgaben (Quelle: context)

2) Offene fachliche Fragen
(Quelle jeweils in Klammern)
- Welche Pilotkund*innen / Zielmarkt werden für das MVP verwendet (Schweiz oder DACH)? Auswirkungen: Währung, rechtliche Texte, Vertragsanforderungen, evtl. lokale Hosting/Vertragsklauseln. (context, docs/requirements.md)
- Reicht für das MVP ein einfaches Kontaktformular/E-Mail für Support oder wird ein persistentes Ticketing-System verlangt? Wenn Kontaktformular, welche SLAs/Prozesse gelten für Support-Anfragen? (docs/requirements.md, docs/risks.md)
- Welche Freigabeschwellen für Rabatte sind final (z. B. 15% / 20% / andere)? Wer entscheidet diese Schwellen (Finance/Eva, Management)? (docs/requirements.md, context)
- Detaillierung Rollen/Berechtigungen: Reicht das minimale Modell (Admin, Sales, Kunde) für Pilotbetrieb, oder sind temporäre Rollen/Abweichungen (Support, Finance, Manager) notwendig? Wenn ja, welche granularity ist zwingend für MVP? (docs/requirements.md, docs/architecture.md)
- Umgang mit Sonderrabatten im MVP: vollständig deaktiviert oder temporäre Admin-Override erlaubt (mit Audit)? Wer bewilligt Ausnahmen? (docs/requirements.md, docs/risks.md)
- Muss das PDF-Angebot/Rechnung bestimmte rechtliche Inhalte/Metadaten (z. B. steuerliche Angaben, Rechnungsnummern) schon im MVP enthalten sein? Wer legt die Pflichtfelder fest? (context, docs/requirements.md)

Prioritätsempfehlung: Klären H5 und Freigabeschwellen (Auswirkungen auf Anforderungen) vor finaler MVP-Abnahme; Support-Workflow und Rollenmatrix vor Pilot-Start.

3) Offene technische Fragen
(Quelle jeweils in Klammern)
- API-Gateway: Gibt es eine priorisierte Zuteilung / verbindlichen Zeitrahmen? Falls nein: welche temporären Alternativen sind akzeptabel (BFF-Proxy, Direkt-API, Mock-Service)? Wie wird Auth/Authorisierung über die temporäre Route gehandhabt? (docs/risks.md, docs/architecture.md)
- SAP-Scope: Ist SAP definitiv read-only im MVP? Gibt es eine Test-/Sandbox-Instanz mit geeigneten (anonymisierten oder synthetischen) Daten? Wer ist Owner des SAP-Interfaces? (docs/requirements.md, docs/architecture.md)
- SSO: Soll SSO in MVP unterstützt werden? Falls ja: welcher Provider wird priorisiert (Azure AD, Google oder beides)? Ein konkreter Entscheid reduziert Integrationsaufwand erheblich. (context, docs/requirements.md)
- Hosting-Provider & Regionen: Welcher Cloud-Provider/Region wird für Prod und Backups gewählt? Gibt es vertragliche Nachweise, dass Backups/DR in EU verbleiben? (docs/requirements.md, docs/architecture.md)
- Security-Review: Welche Prüfungen sind für MVP zwingend (z. B. PenTest, Threat Model, Code-Scan, Konfigurations-Review)? Gibt es ein risikobasiertes, zeitlich begrenztes Review-Scope, um Release nicht zu blockieren? (docs/risks.md, docs/requirements.md)
- Logging/Audit: Konkrete Retention-Dauer für Audit-Logs vs. technischen Logs? Welche Felder dürfen in technischen Logs nicht auftauchen (Liste der PII-Felder)? Wer genehmigt Retention-Dauer? (docs/requirements.md, docs/architecture.md)
- Secrets-Management: Existiert ein zugelassener KMS/Secrets-Store in der Zielinfrastruktur? Wer liefert Zugang/Policies? (docs/architecture.md)
- Testdaten-Strategie: Wie werden Testdaten bereitgestellt/anonymisiert? Gibt es ein Verfahren, das reale Kundendaten in Testsystemen ausschließt? (docs/risks.md)
- PDF-Storage & Lifecycle: Wie lange müssen Angebot-/Rechnungs-PDFs aufbewahrt (Retention/Archivierung)? Müssen PDFs in einem speziellen Archivformat oder nur verschlüsseltes Object-Storage sein? (docs/architecture.md, docs/requirements.md)
- Monitoring/DR SLAs: Ist die Annahme von 99% Verfügbarkeit für MVP verbindlich? Welche Wiederherstellzeiten (RTO/RPO) gelten vor Produktion? (docs/requirements.md)

Prioritätsempfehlung: API-Gateway-Plan und Security-Review-Timeline sind kritisch (H); SAP-Testumgebung und Hosting-Regionen sind hoch; Logging/Retention und Secrets-Mgmt sind zeitkritisch für Compliance.

4) Widersprüche / Konflikte, die geklärt werden müssen
(Quelle jeweils in Klammern)
- Zeitplan (MVP in 8 Wochen) vs. Security-Review (~6 Wochen) und API-Gateway-Warteliste (~6 Wochen) — mehrere kritische Pfade parallel (docs/risks.md, docs/architecture.md).
- Wunsch nach EU-only Managed Services vs. vorhandene Managed-Service-Angebote / Budgetrestriktionen (docs/requirements.md, docs/risks.md).
- Minimales Rollenmodell (Admin, Sales, Kunde) vs. Bedarf an Support/Finance-Einsicht und Rabatte-Freigaben — Konflikt zwischen Einfachheit und tatsächlichen Nutzungsanforderungen (docs/requirements.md, docs/risks.md).
- Löschanfragen (DSGVO) vs. gesetzliche Aufbewahrungspflichten — Verfahren unklar (docs/requirements.md, docs/risks.md).
- Ausschluss manueller Sonderrabatte im MVP vs. Sales-Bedarf nach Flexibilität — Geschäftsanforderung vs. Risikovermeidung (docs/requirements.md, docs/risks.md).

5) Fehlende Informationen (konkret aufzubereiten)
- Erwartete Nutzerzahlen / Lastprofile für MVP (Peak/Dauer/Geographie) — notwendig für Kapazitätsplanung und Skalierungsstrategie. (docs/requirements.md)
- Konkrete Auswahl oder Shortlist von Hosting-Providern/Regionen inkl. Vertrags-/Data-Processing-Agreement-Status. (docs/architecture.md)
- Verbindliche Liste der PII-Felder, die niemals in technischen Logs auftauchen dürfen. (docs/requirements.md)
- Verfügbarkeits-/Zeitplan für API-Gateway-Freigabe bzw. Owner/Contact zur Eskalation. (docs/risks.md)
- Existenz und Zugangsprozess für SAP-Testsystem(e) und Verantwortliche für Testdaten-Bereitstellung/Anonymisierung. (docs/architecture.md)
- Finaler Entscheid zu SSO (ja/nein) und bevorzugtem Provider (Azure AD / Google / beide). (context)
- Konkrete Freigabeschwellen und Prozessbeschreibung für Rabatte (inkl. temporärer Ausnahmeprozesse für MVP). (docs/requirements.md)
- Konkrete Retention-Fristen für Audit-Logs, Rechnungen und Angebots-PDFs (Legal/Finance Vorgaben). (docs/requirements.md, docs/risks.md)

6) Mögliche Ansprechpartner / Rollen (aus dem Transkript erkennbar)
- Anna — Produkt / Business (Sales/Product Owner) — Fragen zu Pilotkunde, MVP-Priorisierung, Freigabeschwellen (fachlich).
- Ben — Technische Leitung / Developer — Fragen zur API-Gateway-Integration, SAP-Adapter, BFF/Mock-Strategie, Monitoring.
- Clara — Datenschutz / Compliance — Fragen zu Retention, Löschung, Audit-Logging, PII in Logs.
- David — Support — Fragen zu Support-Workflow (Kontaktformular vs. Ticketing), notwendige Einsichtsrechte.
- Eva — Finance — Fragen zu Rabattfreigrenzen, Rechnungsanforderungen, Vertragsanforderungen.
- Farid — IT Operations — Fragen zu Hosting, Backups, Secrets-Management, DR.

Empfohlene zusätzliche Rollen / Verantwortlichkeiten (falls nicht zugeordnet)
- Integrations- / API-Gateway-Owner (für Eskalation und Zeitplanung)
- Legal / Datenschutz-Lead (für verbindliche Retention- und Löschregeln)
- Architekt / Integrator (operative Verantwortung für BFF/Adapter-Prototypen)
- Cloud Procurement / Ops-Contract Owner (Provider-Auswahl & EU-Residenz-Nachweise)
- Testdaten-Owner (Anonymisierung & Bereitstellung von synthetischen Datensätzen)

7) Vorschlag für nächstes Vorgehen (Kurz)
- Sofortig: Eskalation / Klärung API-Gateway-Freigabe oder Festlegung temporärer BFF/Mock-Integration (Owner: Ben / API-Gateway-Owner). (H1)
- Parallel: Definition eines risikobasierten Security-Review-Scope für MVP (Clara + Ben + Security-Team). (H2)
- Legal: Kurzfristige Entscheidung zu Retention/ Löschprozess (Clara + Legal + Eva). (H3)
- Entscheidung Pilotmarkt & SSO: Anna + Eva + Rechts/Procurement (Mittel/hoch). (H4/H5)
- Operational: Auswahl eines EU-Region Managed-Providers mit Nachweisen für Backup-Residenz (Farid + Procurement). (hoch)

8) Nachweis / Evidence
- Grundlage: input/transcripts/T9999_chaos.txt; docs/requirements.md; docs/risks.md; docs/architecture.md
- run_id: 20260613_164010_eabd1a

---
Ende: docs/open-questions.md (initial draft)
