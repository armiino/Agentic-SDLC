# Requirements Dokument

## Functional Requirements
- **FR1: Kundenportal** – Das System stellt ein Web‑basiertes Kundenportal bereit, über das angemeldete Kunden ihre Bestellungen einsehen und Rechnungen herunterladen können. [Anna]
- **FR2: Angebotserstellung** – Sales‑Mitarbeiter können im Portal Angebote für Kunden erzeugen, inkl. Produktdaten, Preise und Rabattlogik, basierend auf SAP‑Stammdaten. [Ben]
- **FR3: Login** – Nutzer können sich per E‑Mail und Passwort authentifizieren. Optional muss SSO über Azure AD und/oder Google unterstützt werden. [Anna]
- **FR4: Rollen‑ und Berechtigungskonzept** – Rollen Admin, Manager, User und Support mit differenzierten Zugriffsrechten auf Angebots‑, Bestell‑ und Rechnungsdaten. [Anna]
- **FR5: SAP‑Integration** – Das System muss über eine API‑Schicht auf SAP‑Stammdaten (Produkt, Preis, Rabatt) zugreifen können. [Ben]
- **FR6: KPI‑Erfassung** – Das System erfasst mindestens die Conversion Rate (Angebot → Bestellung) und die Zeit bis zum Angebot. [Anna]
- **FR7: Push‑Notification (optional MVP)** – Möglichkeit, Kunden per Push‑Notification zu informieren, vorausgesetzt es liegt eine gültige Einwilligung vor. [Anna]
- **FR8: Datenlöschung** – Kunden können die Löschung ihrer personenbezogenen Daten initiieren; das System führt das Löschkonzept DSGVO‑konform aus. [Clara]

## Non-functional Requirements
- **NFR1: DSGVO‑Konformität** – Alle personenbezogenen Daten werden in der EU gehostet, Double‑Opt‑In beim Registrieren, Audit‑Trails, Rollen‑ und Löschkonzepte. [Clara]
- **NFR2: Sicherheit** – Kommunikation erfolgt ausschließlich über TLS; OAuth 2.0 wird für die API‑Authentisierung bevorzugt, API‑Keys als Alternative. End‑to‑End‑Verschlüsselung ist nicht erforderlich. [Ben]
- **NFR3: Logging & Audit** – Alle relevanten Aktionen (Login, Angebots‑Erstellung, Datenänderungen) werden protokolliert und auditierbar gespeichert. [Clara]
- **NFR4: Verfügbarkeit & Backup** – Managed Service mit automatisierten Backups und Disaster‑Recovery, um Datenverlust zu vermeiden. [Clara]
- **NFR5: Performance & Skalierbarkeit** – Das System muss 200 – 20 000 gleichzeitige Nutzer unterstützen, ohne Over‑Engineering; horizontale Skalierung über Managed Services. [Ben]
- **NFR6: Zeitrahmen** – Das MVP muss innerhalb von **8 Wochen** geliefert werden. [Anna]

## Constraints / Compliance
- **C1: Keine neue Datenbank** – Nutzung von Managed Database‑Services, keine Eigeninstallation. [Anna]
- **C2: EU‑Only Hosting** – Daten dürfen nur in Rechenzentren innerhalb der EU gespeichert werden. [Clara]
- **C3: Budget** – Lösung muss kostengünstig sein; Managed Services dürfen preislich vertretbar bleiben. [Anna]
- **C4: Security Review** – Ein Security Review ist zwingend erforderlich, muss jedoch im 8‑Wochen‑Zeitplan berücksichtigt werden. [Clara]

## Traceability
| Anforderung | Quelle | Hinweis |
|-------------|--------|--------|
| FR1 | Anna (Portal‑Diskussion) | Kundensicht, Bestellungen/ Rechnungen |
| FR2 | Ben (SAP‑Integration) | Angebotserstellung über SAP‑Daten |
| FR3 | Anna (Login) | E‑Mail/Passwort, optional SSO |
| FR4 | Anna (Rollen) | Admin, Manager, User, Support |
| FR5 | Ben (API Layer) | Notwendig für SAP‑Anbindung |
| FR6 | Anna (KPIs) | Conversion Rate, Zeit bis Angebot |
| FR7 | Anna (Push) | Optional, DSGVO‑Einwilligung nötig |
| FR8 | Clara (Löschkonzept) | DSGVO‑Pflicht |
| NFR1 | Clara (DSGVO) | EU‑Hosting, Double‑Opt‑In, Audit Trails |
| NFR2 | Ben (Security) | TLS, OAuth vs API‑Keys |
| NFR3 | Clara (Logging) | Audit‑Trail erforderlich |
| NFR4 | Clara (Backup) | Disaster Recovery notwendig |
| NFR5 | Ben (Skalierbarkeit) | 200‑20 000 Nutzer erwartet |
| NFR6 | Anna (MVP‑Zeit) | 8 Wochen Lieferzeit |
| C1 | Anna (Kein DB‑Server) | Managed Services nutzen |
| C2 | Clara (EU‑Only) | Datenstandort EU‑only |
| C3 | Anna (Kosten) | Günstige Lösung bevorzugt |
| C4 | Clara (Security Review) | Review nicht vernachlässigen |

*Hinweis*: Anforderungen, bei denen Unsicherheiten bestehen (z.B. Scope für Push‑Notifications, finaler Auth‑Ansatz), sind als Annahmen gekennzeichnet und müssen im weiteren Projektverlauf validiert werden.
