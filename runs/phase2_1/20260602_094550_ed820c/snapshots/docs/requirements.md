## Functional Requirements
- **Login & Authentifizierung**: Kunden können sich per E‑Mail und Passwort anmelden. Das Login muss ein Double‑Opt‑In Verfahren enthalten. Optionales SSO (Azure AD / Google) ist für spätere Phasen vorgesehen, nicht jedoch im MVP.
- **Rollen‑ und Berechtigungskonzept**: Mindestens drei Rollen werden unterstützt: Admin, Sales (Angebots‑Erstellung) und Kunde (Einblick in eigene Angebote & Rechnungen). Rollen bestimmen, welche Aktionen (Erstellen, Freigeben, Anzeigen, Download) zulässig sind.
- **Angebots‑Workflow**: Sales kann Angebote erstellen, speichern und den Status setzen (Draft, Pending Approval, Approved, Sent). Im MVP dürfen keine Sonderrabatte > 15 % vergeben werden; solche Angebote müssen außerhalb des MVP behandelt werden.
- **Angebots‑Freigabe**: Für Standard‑Rabatte ist keine zusätzliche Freigabe nötig. Sonderrabatte über 15 % sind im MVP nicht erlaubt – dies wird als bewusste Einschränkung festgehalten.
- **PDF‑Export**: Erstellte Angebote können als rechtlich konforme PDF‑Dokumente mit Versions‑ und Signatur‑Hinweisen exportiert und heruntergeladen werden.
- **Rechnungs‑Download**: Kunden können ihre Rechnungen im Portal einsehen und als PDF herunterladen.
- **API‑Layer**: Das System stellt eine REST‑API bereit, die lesenden Zugriff auf Produkt‑ und Preisdaten aus SAP ermöglicht. Die API ist minimal gesichert (Grund‑Auth, später OAuth) und verfügt über Rate‑Limiting.
- **Audit‑Trail**: Jede Änderung an Angeboten (Erstellung, Statuswechsel, Löschung) wird protokolliert, inklusive Nutzer‑ID und Zeitstempel. Technische Logs dürfen keine personenbezogenen Daten enthalten.
- **Backup & Disaster Recovery**: Das Portal nutzt ein Managed Service‑Backup, das mindestens tägliche Snapshots der Kundendaten sicherstellt.
- **Hosting & Datenresidenz**: Das System wird ausschließlich in einer EU‑konformen Managed‑Service‑Umgebung gehostet; keine neue eigenständige Datenbank wird betrieben.
- **Internationalisierung**: Das MVP unterstützt Deutsch und Englisch als UI‑Sprachen.

## Non-functional Requirements
- **Sicherheit**: Alle Netzwerkverbindungen sind per TLS zu verschlüsseln. Persönliche Daten werden nur im Rahmen des Double‑Opt‑In und der Login‑Daten gespeichert. Kein End‑zu‑Ende‑Verschlüsselungs‑Feature wird im MVP bereitgestellt.
- **Datenschutz / DSGVO‑Compliance**: Umsetzung von Double‑Opt‑In, Löschkonzept für Kundendaten, Auftrags‑Verarbeitungs‑Verträge und Datenminimierung. Technische Logs enthalten keine personenbezogenen Daten.
- **Performance & Skalierbarkeit**: Das System muss mindestens 200 gleichzeitige Nutzer unterstützen; Skalierbarkeit wird durch das Managed‑Service‑Modell gewährleistet, ohne Over‑Engineering.
- **Verfügbarkeit**: Durch tägliche Backups und ein Managed‑Service‑Hosting wird eine Verfügbarkeit von ≥ 99 % angestrebt.
- **Usability**: Das UI muss sowohl auf Desktop‑Browsern (Web‑First) als auch auf mobilen Geräten (Responsive) funktionieren, jedoch ist ein native Mobile‑App nicht Teil des MVP.
- **Internationalisierung**: UI‑Texte, Fehlermeldungen und E‑Mails müssen in Deutsch und Englisch verfügbar sein.

## Constraints/Compliance
- **Zeitlicher Rahmen**: Das MVP muss innerhalb von 8 Wochen fertiggestellt sein.
- **Keine neue Datenbank**: Es darf keine eigenständige Datenbank‑Instanz aufgesetzt werden; stattdessen wird ein EU‑konformer Managed Service verwendet.
- **Keine vollständige SSO / OAuth im MVP**: Authentifizierung erfolgt ausschließlich per E‑Mail/Passwort mit Double‑Opt‑In.
- **Rabatt‑Grenze**: Im MVP sind Rabatte > 15 % nicht erlaubt; dies ist eine bewusste Scope‑Einschränkung.
- **Kein Ticket‑System**: Support‑Anfragen werden über ein einfaches Kontaktformular abgewickelt; ein voll integriertes Ticket‑System ist nicht Teil des MVP.
- **API‑Gateway**: Das vorgesehene zentrale API‑Gateway ist wegen einer 6‑Wochen‑Warteliste nicht verfügbar; stattdessen wird eine direkte, gesicherte API eingesetzt.

## Assumptions and Open Points
- **SAP‑Schreibzugriff**: Nur lesender Zugriff auf SAP‑Daten wird im MVP benötigt; Schreibzugriff bleibt offen.
- **Kosten für EU‑Only‑Hosting**: Die genaue Kostenschätzung ist noch offen und wird für die Budget‑Freigabe benötigt.
- **Support‑Prozess**: Wie Support‑Mitarbeiter auf Kundendaten zugreifen können, ist noch nicht definiert und wird als Risiko dokumentiert.
- **Retention & Löschkonzept**: Gesetzliche Aufbewahrungspflichten vs. Recht auf Löschung müssen noch final geklärt werden.
- **Cache‑Strategie**: Es wird kein Cache für personenbezogene Preis‑ oder Rabattdaten eingesetzt, um DSGVO‑Risiken zu vermeiden.
- **Internationalisierung**: Erweiterung auf weitere Sprachen und Länder (z. B. USA) ist geplant, aber nicht Teil des MVP.
- **Mehr‑Währung**: EUR ist verpflichtend; CHF und USD werden für spätere Phasen berücksichtigt.
- **Rate‑Limiting & Missbrauchserkennung**: Grundlegendes Rate‑Limiting wird implementiert; umfassende Abuse‑Detection ist außerhalb des MVP.
- **Test‑Umgebungen**: Nutzung von echten SAP‑Daten in Dev/Test muss pseudonymisiert oder synthetisch ersetzt werden – noch offen.

## Traceability
- Alle funktionalen Anforderungen leiten sich direkt aus den Aussagen der Stakeholder im Transkript `T9999_chaos.txt` (z. B. Login‑ und Double‑Opt‑In von Anna, Angebots‑Workflow von Ben & Eva, Rollen‑ und Berechtigungskonzept von Clara & Anna, EU‑Hosting von Farid). 
- Nicht‑funktionale und Constraints resultieren aus den Projektzielen (8‑Wochen‑MVP, keine neue DB) und den expliziten Beschränkungen, die im Kontext zusammengefasst wurden. 
- Annahmen und offene Punkte spiegeln Unsicherheiten im Dialog wider (z. B. SAP‑Schreibzugriff, Hosting‑Kosten, Support‑Prozess). 

*Evidenz: Der gesamte Kontext stammt aus `runs/phase2_1/20260602_094550_ed820c/state/context.md` und dem zugrundeliegenden Transkript `input/transcripts/T9999_chaos.txt`.*