# Requirements Document

Hinweis: Dieses Dokument ist ein initialer Entwurf basierend auf dem MAF-Shared-State (Quelle: input/transcripts/T9999_chaos.txt). Unsicherheiten und offene Punkte sind separat gekennzeichnet.

## Functional Requirements

- FR-01: Nutzer-Authentifizierung
  - Beschreibung: Das Portal muss eine Anmeldung per E-Mail und Passwort unterstützen und beim Erst-Account die Double-Opt-In-Verifikation per E-Mail durchführen.
  - Akzeptanzkriterien: Ein neuer Nutzer erhält eine Verifikations-E-Mail; der Account bleibt inaktiv bis zur Bestätigung; Anmeldung mit bestätigter E-Mail ist möglich.
  - Quelle/Stakeholder: Kurzbeschreibung, Anna, Transkript.

- FR-02: Optionales SSO (nicht-MVP)
  - Beschreibung: SSO-Integrationen (z. B. Unternehmens-SSO) sind als geplante Option zu dokumentieren, aber nicht Voraussetzung für das MVP.
  - Akzeptanzkriterien: SSO-Anforderung ist dokumentiert; MVP funktioniert ohne SSO.
  - Quelle/Stakeholder: Kurzbeschreibung, Anna, Ben.

- FR-03: Angebots-Erstellung
  - Beschreibung: Sales-Nutzer müssen Angebote erstellen, speichern (Draft) und bearbeiten können. Ein Angebot kann als PDF exportiert werden.
  - Akzeptanzkriterien: Ein Sales-Nutzer kann ein Angebot anlegen, Felder ausfüllen, speichern, einen PDF-Export erzeugen und den Status auf Draft → Pending Approval → Approved → Sent ändern (Statusänderungen im MVP: Draft, Pending Approval, Approved, Sent).
  - Einschränkung (MVP): Manuelle Sonderrabatte sind im MVP deaktiviert; Rabattlogik ist auf Standardrabatte beschränkt.
  - Quelle/Stakeholder: Projektziel, Vorgeschlagener MVP-Schnitt (Transkript), Eva.

- FR-04: Rechnungsanzeige und Download
  - Beschreibung: Kunden müssen Rechnungen einsehen und als PDF herunterladen können.
  - Akzeptanzkriterien: Ein eingeloggter Kunde sieht zu seinem Account zugeordnete Rechnungen und kann jede Rechnung als PDF herunterladen.
  - Quelle/Stakeholder: Fachliche Themen, Kurzbeschreibung.

- FR-05: Anzeige von Bestellungen (read-only)
  - Beschreibung: Kunden sollen eine Übersicht über ihre Bestellungen sehen; initial read-only aus SAP oder dem Backend.
  - Akzeptanzkriterien: Bestellungseinträge werden angezeigt; keine Schreiboperationen im MVP.
  - Quelle/Stakeholder: Fachliche Themen, Vorgeschlagener MVP-Schnitt, Ben.

- FR-06: Integration – SAP Read-Only
  - Beschreibung: Das Portal muss Produkt-, Preis- und Kundenstammdaten aus SAP (oder dem definierten System) lesend integrieren. Live-Preise sind zu zeigen, sofern die Verbindung zum SAP-Read-System gegeben ist.
  - Akzeptanzkriterien: Produkt- und Preisdaten werden bei Angebots-Erstellung angezeigt; fehlende Live-Verfügbarkeit führt zu definiertem Fallback (z. B. Fehlermeldung oder Hinweis).
  - Einschränkung: Schreibzugriffe auf SAP sind nicht Bestandteil des MVP.
  - Quelle/Stakeholder: Fachliche Themen, Konflikte und Spannungsfelder.

- FR-07: Minimales Rollenmodell
  - Beschreibung: Das System muss mindestens die Rollen Admin, Sales und Kunde unterstützen mit folgenden Berechtigungen:
    - Admin: Benutzer- und Rollenverwaltung, Einsicht aller Daten.
    - Sales: Angebote erstellen und bearbeiten, Kunden zuordnen.
    - Kunde: Einsicht eigener Bestellungen und Rechnungen, Download-Funktionalität.
  - Akzeptanzkriterien: Rollenzuordnung funktioniert; Berechtigungen verhindern nicht-autorisierte Aktionen.
  - Quelle/Stakeholder: Fachliche Themen.

- FR-08: Minimales Audit-Trail
  - Beschreibung: Das System muss Änderungen an Angeboten (Erstellung, Statuswechsel, PDF-Export) mit Zeitstempel, Benutzer-ID und Aktion protokollieren.
  - Akzeptanzkriterien: Für jede relevante Aktion ist ein Audit-Eintrag vorhanden; Logs enthalten keine sensiblen PII-Felder (siehe Compliance).
  - Quelle/Stakeholder: Compliance/Clara, Vorgeschlagener MVP-Schnitt.

- FR-09: Support-Kontakt (eingeschränkt für MVP)
  - Beschreibung: Das MVP stellt ein Kontaktformular oder E-Mail-Trigger zur Verfügung; kein vollständiges Ticketing-System.
  - Akzeptanzkriterien: Ein eingeloggter Nutzer kann Support-Anfragen absenden; die Anfrage erzeugt eine E-Mail oder einen Eintrag zur manuellen Weiterverarbeitung.
  - Quelle/Stakeholder: Vorgeschlagener MVP-Schnitt, David.

## Non-functional Requirements

- NFR-01: Datenschutz und Sicherheit
  - Beschreibung: TLS-Minimum für alle Verbindungen; Verschlüsselung ruhender personenbezogener Daten.
  - Akzeptanzkriterien: HTTPS für alle Endpunkte; Nachweis, dass personenbezogene Daten serverseitig verschlüsselt abgelegt werden (oder ein entsprechendes Secrets-Management existiert).
  - Quelle/Stakeholder: Compliance/Clara, Farid.

- NFR-02: Datenresidenz
  - Beschreibung: Produktionsdaten und Backups müssen in der EU gehostet werden (EU-only). Entwicklungs- und Testdaten sind gesondert vom Produktivsystem zu halten.
  - Akzeptanzkriterien: Nachweis der Datenresidenz (Region/Standorte); Backups sind in EU-Regionen und werden gemäß Retention-Richtlinie gehalten.
  - Quelle/Stakeholder: Compliance/Clara, Betrieb/Nicht-funktional.

- NFR-03: Verfügbarkeit und DR (MVP Mindestanforderung)
  - Beschreibung: Grundlegende Backup-/Disaster-Recovery-Prozesse müssen existieren; SLAs für das MVP sind minimal (z. B. Produktions-Availability-Ziel als Annahme).
  - Akzeptanzkriterien: Backup-Plan dokumentiert; Wiederherstellbarkeitstest-Szenario definiert.
  - Annahme: Ziel-Availability für MVP = 99% (kennzeichnen als Annahme; final zu klären).
  - Quelle/Stakeholder: Betrieb/Nicht-funktional, Farid.

- NFR-04: Skalierbarkeit
  - Beschreibung: App muss für erwarteten MVP-Traffic skaliert werden können; genaue Nutzerzahlen sind unbekannt.
  - Akzeptanzkriterien: Nachweis eines skalierbaren Deployments (z. B. Auto-scaling möglich) oder Dokumentation der Betriebsgrenzen.
  - Annahme: Startlast für MVP wird als klein/Medium angenommen (offen).
  - Quelle/Stakeholder: Betrieb/Nicht-funktional.

- NFR-05: Logging & Audit-Retention
  - Beschreibung: Audit-Logs sind mandatsgerecht aufzubewahren; technische Logs dürfen keine personenbezogenen Daten enthalten.
  - Akzeptanzkriterien: Log-Retention-Dauer dokumentiert; Beispiele zeigen keine PII in technischen Logs.
  - Offen: Genaue Retention-Dauer (gesetzliche Aufbewahrungspflichten vs. Löschanfragen) muss geklärt werden.
  - Quelle/Stakeholder: Compliance/Clara.

## Constraints/Compliance

- C-01: DSGVO-Konformität
  - Anforderungen: Double-Opt-In bei Registrierung; klare Prozesse für Betroffenenrechte (Auskunft, Berichtigung, Löschung) und Retention-Policies, die mit gesetzlichen Aufbewahrungsfristen abgleichen.
  - Akzeptanzkriterien: Prozessdokumente und Implementierungsnachweis für Löschanfragen; Auditfähigkeit von Zugriffs- und Löschvorgängen.
  - Quelle/Stakeholder: Compliance/Clara.

- C-02: EU-Datenresidenz
  - Anforderungen: Produktionsdaten und Backups müssen in der EU bleiben.
  - Akzeptanzkriterien: Infrastruktur- und Backup-Standorte in EU-Regionen nachgewiesen.
  - Quelle/Stakeholder: Betrieb/Nicht-funktional, Farid.

- C-03: Keine PII in technischen Logs
  - Anforderungen: Maskierung/Anonymisierung sensibler Felder in technischen Logs; Audit-Logs dürfen notwendige personenbezogene Informationen enthalten, sofern berechtigt und dokumentiert.
  - Akzeptanzkriterien: Logging-Policy-Dokument; Beispiele und Tests zeigen Maskierung.
  - Quelle/Stakeholder: Compliance/Clara.

- C-04: Minimales Verschlüsselungsniveau
  - Anforderungen: TLS für Daten-in-Transit; Verschlüsselung ruhender Daten.
  - Akzeptanzkriterien: Zertifikate/Verfahren dokumentiert.
  - Quelle/Stakeholder: Compliance, Farid.

## Assumptions and Open Points

- AOP-01: Pilotkunde / Zielmarkt
  - Offen: Welcher Pilotkunde (Schweiz vs. DACH) entscheidet über Währung, Vertragsanforderungen und ggf. lokale Compliance.
  - Wirkung: Beeinflusst Währungsanzeige, rechtliche Texte und ggf. Datenverarbeitungsvorgaben.

- AOP-02: SAP-Scope
  - Offen: Ist SAP nur read-only (MVP) oder sind Schreiboperationen geplant? Aktuelle Annahme: Read-only im MVP.

- AOP-03: API-Gateway-Verfügbarkeit
  - Offen: Zentrale API-Gateway-Warteliste (6 Wochen) ist ein kritischer Pfad. Alternative Integrationswege für MVP müssen geprüft.

- AOP-04: SSO Ausprägung
  - Offen: Konkreter SSO-Anbieter (Azure AD, Google) nicht entschieden — MVP darf ohne SSO auskommen.

- AOP-05: Freigabeschwellen für Rabatte
  - Offen: Schwellenwerte (15%/20%/andere) sind nicht final; MVP beschränkt Sonderrabatte.

- AOP-06: Retention vs. Löschung
  - Offen: Wie Löschanfragen mit gesetzlichen Aufbewahrungspflichten interagieren; Verfahren erforderlich.

- AOP-07: Nutzerzahlen
  - Offen: Erwartete Nutzerzahlen sind unsicher; Annahmen zur Skalierung sind zu validieren.

- AOP-08: Hosting-Provider / konkrete Regionen
  - Offen: Konkreter Provider/Region innerhalb der EU noch offen; nur EU-Residenz vorgeben.

- AOP-09: Support-Workflow im MVP
  - Offen: Volles Ticketing ausgeschlossen; Entscheidung ob Kontaktformular genügt verbleibt offen.

## Traceability

- FR-01, FR-02 (Login / SSO) <- Kontext: Kurzbeschreibung; Stakeholder: Anna; Quelle: input/transcripts/T9999_chaos.txt
- FR-03 (Angebots-Erstellung, PDF) <- Kontext: Projektziel; Vorgeschlagener MVP-Schnitt; Stakeholder: Anna, Eva
- FR-04 (Rechnungsanzeige/Download) <- Kontext: Kurzbeschreibung; Stakeholder: Anna, Finance/Eva
- FR-05, FR-06 (Bestellungen, SAP read-only) <- Kontext: Fachliche Themen; Konflikte (SAP-Verfügbarkeit); Stakeholder: Ben
- FR-07 (Rollenmodell) <- Kontext: Fachliche Themen; Stakeholder: Anna, David
- FR-08 (Audit-Trail) <- Kontext: Compliance-Anforderungen; Stakeholder: Clara
- FR-09 (Support-Kontakt) <- Kontext: Vorgeschlagener MVP-Schnitt; Stakeholder: David
- NFR-01..NFR-05 / C-01..C-04 <- Kontext: Compliance / Betrieb / Nicht-funktional; Stakeholder: Clara, Farid

Quelle der Ableitungen: MAF shared state (Kontextzusammenfassung aus input/transcripts/T9999_chaos.txt), Extraktionsdatum 2026-06-13, run_id: 20260613_164010_eabd1a.

-- Ende des initialen Entwurfs --
