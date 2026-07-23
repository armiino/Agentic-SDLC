# Grober Architektur‑Überblick für das Kundenportal‑MVP (8 Wochen)

## 1. Überblick & Zielsetzung
Das Dokument beschreibt die **frühe Architektur** für das MVP des Kundenportals, das innerhalb von **8 Wochen** fertiggestellt werden muss. Ziel ist die schnelle Erstellung von Angeboten, der Rechnungsdownload und ein grundlegender Kunden‑Login. Alle Entscheidungen orientieren sich an den bereits ermittelten **Functional Requirements**, **Non‑functional Requirements**, den **Risiken** und den offenen Punkten.

## 2. Kern‑Komponenten
| Komponente | Kurzbeschreibung | Primäre Schnittstelle(n) | Hinweis / offene Entscheidung |
|------------|-------------------|--------------------------|--------------------------------|
| **Web‑Frontend** (Portal UI) | React‑ bzw. Angular‑basiertes Single‑Page‑Application (SPA) – liefert Login, Rollen‑Dashboard, Angebotserstellung, Rechnungsdownload. | Auf API‑Layer (REST) | Wahl des Frameworks (React vs. Angular) – beide machbar, kein Einfluss auf MVP‑Zeitplan. |
| **API‑Layer** (Backend) | Node.js/Express (oder Spring Boot) Service, stellt **REST‑Endpoints** bereit. Implementiert Authentifizierung, Autorisierung, Geschäftslogik. | Frontend, **SAP‑Adapter**, **DB‑Service**, **PDF‑Generator**, **Audit‑Log‑Service**, **Backup‑Service** | Entscheidung zu **OAuth‑2.0** (Authorization‑Code‑Flow) vs. API‑Key (bis API‑Gateway verfügbar). |
| **Auth‑Service** (Identity) | Verantwortlich für Login, Double‑Opt‑In, Token‑Issuing. Nutzt Managed Identity Provider (z. B. Azure AD, Google) **optional**; alternativ ein einfaches JWT‑basiertes System. | API‑Layer, Frontend | Auswahl des Identity‑Providers – im MVP kann ein leichter JWT‑Service ausreichen, später Integration von SSO. |
| **SAP‑Adapter** | Kleiner Service, der SAP‑Web‑Services oder OData‑Endpoints **nur lesend** nutzt, um Produkt‑ und Preis‑Daten zu holen. | API‑Layer, SAP‑System | Entscheidung ob **OData** oder **REST‑Wrapper** verwendet wird; Performance‑Caching wird im MVP bewusst **nicht** eingesetzt (kritisch für Daten‑Minimierung). |
| **PDF‑Generator** | Service (z. B. wkhtmltopdf, PDF‑Lib) erzeugt Angebots‑ und Rechnungs‑PDFs inkl. rechtlicher Fußnoten & DSGVO‑Hinweise. | API‑Layer, DB‑Service | Auswahl der Bibliothek; ggf. Lizenz‑Abhängigkeit. |
| **Managed DB‑Service** | Datenbank‑as‑a‑Service (z. B. Azure PostgreSQL, AWS RDS‑PostgreSQL) – speichert Nutzer‑Accounts, Rollen, Angebote, Audit‑Einträge. | API‑Layer, Backup‑Service | Provider‑Auswahl (Kosten, EU‑Only‑Region) – offene Entscheidung. |
| **Audit‑Log‑Service** | Schreib‑only Service, speichert **wer, wann, was** (Login, Angebot‑Änderung). Trennung von **technischen Logs** (keine PII) und **Audit‑Logs** (enthält PII). | API‑Layer, Monitoring‑Service | Storage‑Medium (z. B. Log‑Analytics, CloudWatch) – muss DSGVO‑konform sein. |
| **Backup‑/Disaster‑Recovery‑Service** | Tägliches, verschlüsseltes Backup des DB‑Volumes; Wiederherstellung < 4 h. | Managed DB, Storage‑Provider | Detail‑Auswahl (S3‑Glacier, Azure Blob) – noch offen. |
| **Monitoring & Alerting** | Infrastruktur‑ und Applikations‑Monitoring (z. B. Prometheus + Grafana oder Cloud‑Native). | Alle Komponenten | Konfiguration ohne PII; Trennung von Sicherheits‑Logs. |
| **Hosting‑Infrastruktur** | Managed Service in EU‑Only Rechenzentrum (z. B. Azure Germany Cloud, AWS EU‑West). | Alle Komponenten | Auswahl des Providers – Kosten‑Unsicherheit bleibt. |

## 3. Schnittstellen & Datenflüsse
1. **Login‑Flow**: Frontend → Auth‑Service (Double‑Opt‑In) → JWT‑Token → API‑Layer (Authorization).  
2. **Angebot‑Erstellung**: Frontend → API‑Layer → SAP‑Adapter (Produkt‑/Preis‑Daten) → DB‑Service (Speichern) → PDF‑Generator (PDF‑Export) → Audit‑Log.  
3. **Rechnungs‑Download**: Frontend → API‑Layer → DB‑Service (Rechnung) → PDF‑Generator → Audit‑Log.  
4. **Backup**: DB‑Service → Backup‑Service (tägliches Snapshot).  
5. **Monitoring**: Jede Komponente sendet Metriken → Monitoring‑Service; Alerts bei Fehlermeldungen oder SLA‑Verletzungen.  
6. **Audit‑Logging**: Jede geschäftskritische Aktion (Login, Angebot anlegen/ändern, Rechnung downloaden) wird an Audit‑Log‑Service geschrieben.

## 4. Daten‑ & Sicherheitsaspekte
- **Authentifizierung & Autorisierung**: JWT‑Token mit Claims (role). Token‑Signatur mit **asymmetrischer Schlüssel** (Public‑Private) – Schlüssel im **Secrets‑Manager**.  
- **Daten‑verschlüsselung**:   
  - **In‑Transit**: TLS 1.2+ überall (Frontend ↔ API, API ↔ SAP, API ↔ DB).   
  - **At‑Rest**: DB‑Service verschlüsselt (Transparent Data Encryption).   
  - **Backups**: Verschlüsselt, Schlüssel im Secrets‑Manager.  
- **DSGVO‑Konformität**:   
  - Double‑Opt‑In bei Registrierung (Requirements FR1).   
  - **Audit‑Logs** enthalten notwendige personenbezogene Angaben, werden für 12 Monate aufbewahrt und nur autorisierten Personen zugänglich.   
  - **Technische Logs** dürfen **keine PII** enthalten.   
  - **Daten‑Residenz**: Alle Daten (DB, Backups, Logs) liegen ausschließlich in EU‑Regionen.   
- **Secrets‑Management**: Zentraler Secrets‑Store (z. B. HashiCorp Vault, AWS Secrets‑Manager) für DB‑Passwörter, JWT‑Signing‑Key, API‑Keys.  
- **Rate‑Limiting**: Da das zentrale API‑Gateway erst nach 6 Wochen verfügbar ist, implementiert die API‑Layer ein **lokales Rate‑Limiting** (z. B. token‑bucket) für alle öffentlichen Endpoints.

## 5. Offene Architekturentscheidungen (zur Klärung vor oder nach MVP)
| Entscheidung | Mögliche Optionen | Bewertung/Einfluss auf MVP |
|---------------|-------------------|----------------------------|
| **Identity Provider / SSO** | Azure AD, Google Identity, Eigenes JWT‑System | JWT‑System ist für MVP schnell umsetzbar; SSO kann später integriert werden. |
| **Managed DB‑Provider** | Azure PostgreSQL (Germany), AWS RDS‑PostgreSQL (EU‑West), GCP CloudSQL | Auswahl beeinflusst Kosten & Daten‑Residenz – Entscheidung bis Sprint 2 erforderlich. |
| **Hosting‑Provider** | Azure Germany Cloud, AWS EU, GCP EU | Muss EU‑Only garantieren; Kosten‑Schätzung erforderlich. |
| **API‑Gateway** | Nutze Azure API‑Management (nach 6 Wochen) oder eigenes Rate‑Limiting (MVP) | Für MVP lokales Rate‑Limiting ausreichend, später Integration. |
| **PDF‑Library** | wkhtmltopdf (Open‑Source), iText (kommerziell) | wkhtmltopdf genügt für MVP, Lizenz‑Risiko prüfen. |
| **Monitoring‑Tool** | Prometheus+Grafana (self‑hosted), CloudWatch / Azure Monitor | Selbst‑hosted erfordert extra Infrastruktur – Cloud‑Native für MVP bevorzugt. |
| **Backup‑Storage** | Cloud‑Object‑Storage (S3‑Glacier, Azure Blob‑Cold) vs. Managed DB‑Backup | Beide unterstützen Verschlüsselung – Kosten‑Abschätzung nötig. |
| **Internationalisierung** | i18n‑Framework im Frontend (React‑i18next) | Grundlegende Unterstützung für Deutsch/Englisch wird im MVP implementiert; weitere Sprachen später. |
| **Rate‑Limiting‑Implementierung** | In‑Process Token‑Bucket vs. External Redis‑Based | In‑Process ist schneller einsetzbar, reicht für MVP‑Last. |

## 6. Bewertung des Architektur‑Risikos
- **Abhängigkeit von SAP** ist kritisch – muss klare Timeout‑Strategie und Fehlermeldungen haben.  
- **Fehlende API‑Gateway‑Funktionalität** erhöht Angriffsfläche; lokales Rate‑Limiting und Input‑Validierung mindern das Risiko.  
- **Hosting‑Kosten‑Unsicherheit** kann Budget überschreiten – frühzeitige Provider‑Auswahl und Kosten‑Schätzung wichtig.  
- **Secrets‑Management** muss vor Produktionsstart eingerichtet sein, sonst Risiko von Credential‑Leaks.  
- **DSGVO‑Umsetzung** (Double‑Opt‑In, Logging, Daten‑Residenz) ist zwingend; fehlende Umsetzung führt zu rechtlichen Konsequenzen.

## 7. Nächste Schritte (für das Entwicklungsteam)
1. **Provider‑Auswahl** (DB + Hosting) bis Ende Sprint 1.  
2. **Implementierung des JWT‑Auth‑Service** (Double‑Opt‑In).  
3. **Aufsetzen der Managed DB** inkl. Verschlüsselung & Secrets‑Management.  
4. **Entwicklung des SAP‑Adapters** (nur Lese‑Zugriff).  
5. **Einführung lokales Rate‑Limiting** im API‑Layer.  
6. **Einrichtung von Monitoring & Audit‑Log‑Service**.  
7. **Implementierung von Backup‑Job** (tägliche Snapshots).  
8. **Frontend‑Skeleton** (Login, Dashboard, Angebots‑Formular).  
9. **PDF‑Generator‑Integration** für Angebots‑ und Rechnungs‑Export.  

---
*Erstellt aus dem Kontext `runs/phase2_1/20260602_105417_e37966/state/context.md`, den Requirements `docs/requirements.md`, den Risiko‑Report `docs/risks.md` und dem Transkript `input/transcripts/T9999_chaos.txt`.*