# Requirements Dokument

## Functional Requirements

1. **Login & Authentifizierung**
   - Nutzer können sich per E‑Mail und Passwort registrieren und einloggen (Double‑Opt‑In). (Quelle: Anna) 
   - Optionales SSO (Azure AD / Google) wird nicht im MVP verpflichtend implementiert. (Quelle: Anna)
2. **Angebotserstellung**
   - Sales‑Mitarbeiter können Angebote über einen Wizard erstellen, wobei Produkt‑ und Preisdaten aus SAP gelesen werden. (Quelle: Ben)
   - Angebote enthalten keine manuellen Sonderrabatte (> Standard‑Rabatt). Sonderrabatte erfordern einen Freigabe‑Workflow, der erst in Phase 2 implementiert wird. (Quelle: Eva)
3. **Rechnungs‑ und Bestellübersicht**
   - Kunden können ihre Bestellungen einsehen und Rechnungen als PDF herunterladen. (Quelle: Anna)
4. **Rollen & Berechtigungen**
   - Minimal‑Rollenmodell: **Admin**, **Sales**, **Kunde**.
   - Rollen definieren Lese‑/Schreibrechte für Angebote, Rechnungen und Kundendaten. (Quelle: Anna, Clara)
5. **Audit‑Trail**
   - Jede Erstellung bzw. Änderung eines Angebots wird protokolliert (Wer, wann, Aktion). (Quelle: Clara)
6. **DSGVO‑Konformität**
   - Double‑Opt‑In beim Registrieren.
   - Grundlegendes Lösch‑Workflow für Nutzerkonten (Anfrage → Datenlöschung). (Quelle: Clara)
   - Keine personenbezogenen Daten in technischen Logs. (Quelle: Clara)
7. **API‑Layer**
   - REST‑API mit OAuth‑2.0 (Client‑Credentials) für interne Nutzung (SAP‑Lesezugriff, Frontend‑Kommunikation). (Quelle: Ben)
8. **Managed Hosting (EU‑Only)**
   - Anwendung wird in einem EU‑Managed Service betrieben, um Datenresidenz sicherzustellen. (Quelle: Farid)
9. **Backup & Disaster Recovery**
   - Tägliche Backups des Managed Service, Aufbewahrung gemäß EU‑Standard. (Quelle: Farid)
10. **Rate Limiting**
    - Grundlegendes Rate‑Limiting (z. B. 10 Requests / Sekunde pro Nutzer) zum Schutz vor Missbrauch. (Quelle: Ben)
11. **Internationalisierung (MVP)**
    - UI in Deutsch und Englisch verfügbar. (Quelle: Anna)
12. **PDF‑Export**
    - Angebots‑PDF mit generischer Vorlage, Versions‑ID und rechtlichen Hinweisen. (Quelle: Eva)
13. **Umgebungen**
    - Drei getrennte Umgebungen (Dev, Test, Prod) mit synthetischen Testdaten; kein Zugriff auf produktive SAP‑Daten in Dev/Test. (Quelle: Clara)

## Non-functional Requirements

- **Performance**: Antwortzeiten < 2 s für Kern‑APIs unter Last von bis zu 2 000 gleichzeitigen Nutzern. (Quelle: Anna)
- **Skalierbarkeit**: Managed Service muss horizontal skalieren können; keine Over‑Engineering im MVP. (Quelle: Clara)
- **Sicherheit**: TLS 1.2+ für alle Verbindungen; OAuth‑2.0 für API‑Zugriff; getrennte Audit‑ und technische Logs. (Quelle: Ben, Clara)
- **Verfügbarkeit**: 99,5 % im Jahresmittel, inkl. täglicher Backups. (Quelle: Farid)
- **Compliance**: DSGVO‑konform (Double‑Opt‑In, Löschkonzept, EU‑Datenresidenz). (Quelle: Clara)
- **Wartbarkeit**: Code‑Repository mit CI/CD, Secrets‑Management für API‑Keys und Datenbank‑Credentials. (Quelle: Farid)

## Constraints / Compliance

- **Zeitrahmen**: MVP muss innerhalb von 8 Wochen lieferbar sein. (Quelle: Anna)
- **Budget**: Keine neue physische Datenbank; Nutzung von Managed Services. (Quelle: Anna)
- **SAP‑Integration**: Nur Lesezugriff im MVP; keine Schreibrechte. (Quelle: Ben)
- **Keine Sonderrabatte**: Manuelle Rabatte > Standard‑Rabatt nicht erlaubt; Freigabe‑Workflow erst später. (Quelle: Eva)
- **Kein Ticket‑System**: Support erfolgt über einfaches Kontakt‑Formular ohne Persistenz. (Quelle: David)
- **API‑Gateway**: Direkter API‑Expose, da zentrale Gateway‑Warteliste 6 Wochen überschreitet. (Quelle: Ben)
- **Hosting‑Kosten**: EU‑Only Hosting wird verwendet; Kostenschätzung ist noch offen. (Quelle: Farid)

## Traceability

| Requirement | Quelle (Transkript) |
|-------------|----------------------|
| FR1 Login & Double‑Opt‑In | Anna (Zeile 1‑4) |
| FR2 Angebotserstellung (SAP‑Lesezugriff) | Ben (Zeile 9‑12) |
| FR3 Rechnungs‑Download | Anna (Zeile 24‑26) |
| FR4 Rollenmodell | Anna (Zeile 30‑34) |
| FR5 Audit‑Trail | Clara (Zeile 41‑44) |
| FR6 DSGVO‑Pflichten | Clara (Zeile 20‑23, 57‑60) |
| FR7 API‑OAuth | Ben (Zeile 86‑92) |
| FR8 EU Managed Hosting | Farid (Zeile 180‑185) |
| FR9 Backup | Farid (Zeile 190‑193) |
| FR10 Rate Limiting | Ben (Zeile 260‑267) |
| FR11 Internationalisierung | Anna (Zeile 300‑306) |
| FR12 PDF‑Export | Eva (Zeile 320‑326) |
| FR13 Umgebungen & Testdaten | Clara (Zeile 340‑350) |
| NFR1 Performance | Anna (Zeile 120‑124) |
| NFR2 Skalierbarkeit | Clara (Zeile 150‑154) |
| NFR3 Sicherheit (TLS, OAuth) | Ben, Clara (Zeile 86‑92, 41‑44) |
| NFR4 Verfügbarkeit | Farid (Zeile 190‑193) |
| NFR5 Compliance (DSGVO) | Clara (Zeile 20‑23) |
| NFR6 Wartbarkeit (CI/CD, Secrets) | Farid (Zeile 352‑357) |
| CON1 Zeitrahmen 8 Wochen | Anna (Zeile 1‑4) |
| CON2 Budget – keine DB | Anna (Zeile 84‑90) |
| CON3 SAP‑Nur Lesenzugriff | Ben (Zeile 97‑100) |
| CON4 Keine Sonderrabatte | Eva (Zeile 380‑386) |
| CON5 Kein Ticket‑System | David (Zeile 410‑418) |
| CON6 API‑Gateway Warteliste | Ben (Zeile 430‑435) |
| CON7 EU‑Only Hosting | Farid (Zeile 180‑185) |

*Alle Zeilenangaben beziehen sich auf das Transkript `input/transcripts/T9999_chaos.txt`.*