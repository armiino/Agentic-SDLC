# Architekturüberblick – Frühe SDLC‑Phase

## 1. Systemkontext
- **Kundenportal (Web‑Frontend)**: Bietet UI für Login/Double‑Opt‑In, Angebotserstellung, Rechnung‑/Bestellungs‑Download und Support‑Kontaktformulare.
- **Backend‑Service‑Layer**: Setzt sich zusammen aus mehreren Micro‑ bzw. Modular‑Services, die über ein **interims‑API‑Gateway/Proxy** kommunizieren.
- **Externe SAP‑Systeme**: Lesender Zugriff (Read‑Only) auf Produkt‑, Preis‑ und Rabattdaten, später Schreib‑Zugriff für Bestellungen (Phase 2).
- **Managed Service Hosting (EU‑Only)**: Bereitstellung von Compute, Storage und Datenbanken innerhalb der EU, inkl. Backup & Disaster‑Recovery.
- **Stakeholder‑Interaktionen**: Produkt‑/Projektleitung, technische Entwicklung, Compliance, Finance, Support, IT‑Operations.

## 2. Wichtige Komponenten
| Komponente | Aufgabe | Hinweis/Offene Entscheidung |
|------------|---------|-----------------------------|
| **Web‑Frontend** | Angular/React‑basierte SPA (Responsive UI) | Technologie‑Stack nicht festgelegt – Auswahl nach Team‑Kompetenz. |
| **Auth‑Service** | Benutzer‑Login (E‑Mail/Passwort) mit Double‑Opt‑In, optionales SSO (Azure AD/Google) | SSO im MVP optional – Entscheidung ausstehend. |
| **API‑Gateway / Proxy** | Eingangs‑Punkt für alle Backend‑Aufrufe, Grund‑Rate‑Limiting & IP‑Allowlist (Interims‑Lösung) | Endgültiges API‑Gateway erst nach 6‑Wochen‑Warteliste. |
| **Angebots‑Service** | Erzeugt und verwaltet Angebots‑Workflows (Draft → Sent → Accepted …) | Rabatt‑Freigabe > 15 % erst für Phase 2; temporäre manuelle Freigabe optional. |
| **Produkt‑/Preis‑Adapter** | Liest Produkt‑ und Preisdaten aus SAP (Read‑Only) und cached sie für Performance. | Cache‑Invalidierung bei nächtlichen Preis‑Updates muss definiert werden. |
| **Rollen‑ & Berechtigungs‑Service** | Verwaltung von Rollen (Admin, Sales, Kunde) und feingranularen Zugriffsrechten. |
| **Audit‑Log‑Service** | Erfasst Nutzer‑aktionen (wer hat was gesehen/geändert) pseudonymisiert, getrennt von technischen Logs. |
| **Backup & DR Service** | Automatisierte tägliche Backups, Wiederherstellung (RPO≈4 h, RTO≈2 h) innerhalb EU‑Rechenzentrum. |
| **Monitoring & Alerting** | Grundlegendes Monitoring (CPU, RAM, HTTP‑Status, Rate‑Limits) ohne PII. |
| **Secrets‑Management** | Sichere Speicherung von Credentials, API‑Keys (z. B. Vault‑ähnliche Lösung). |

## 3. Schnittstellen / Integrationspunkte
- **Frontend ↔ API‑Gateway**: REST‑/JSON‑Aufrufe über HTTPS (TLS 1.2+).
- **API‑Gateway ↔ Micro‑Services**: Interner HTTP/gRPC‑Aufruf, mit JWT‑basiertem Auth‑Token.
- **Angebots‑Service ↔ SAP‑Adapter**: SOAP/REST‑Connector (Lesender Zugriff) – definiert durch SAP‑OData‑API.
- **Auth‑Service ↔ Optionales SSO‑Provider**: OpenID Connect / SAML (falls aktiviert).
- **Audit‑Log‑Service ↔ Logging‑Infrastruktur**: Schreibzugriff auf getrennte Log‑Datenbank (z. B. Elasticsearch) für Pseudonym‑Logs, technische Logs in separatem System.
- **Backup‑Service ↔ Managed Storage**: Verschlüsselte Backups in EU‑Only Object‑Store.

## 4. Daten‑ und Sicherheitsaspekte
- **Datenschutz / DSGVO**: Alle personenbezogenen Daten verbleiben in EU‑Rechenzentren; Double‑Opt‑In für Marketing‑E‑Mails; Pseudonymisierung in Audit‑Logs; Trennung von technischen Logs und personenbezogenen Daten.
- **Transport‑Sicherheit**: TLS‑Verschlüsselung für sämtliche Netzwerkverbindungen (Frontend ↔ Backend, Backend ↔ SAP).
- **Auth‑ und Autorisierung**: JWT‑basiertes Token‑System, Rollen‑basiertes Zugriffskontrollmodell (RBAC).
- **Rate‑Limiting**: Basis‑Limits (z. B. 100 Requests/Minute/User) über Interims‑Proxy, Monitoring zur Anpassung.
- **Secrets‑Management**: Zentralisierte Verwaltung von API‑Keys, DB‑Credentials; Zugriff nur für autorisierte Services.
- **Backup & Disaster Recovery**: Tägliche verschlüsselte Snapshots, Aufbewahrung gemäß rechtlichen Vorgaben, Restore‑Tests monatlich.
- **Logging Governance**: Technische Logs ohne PII, Audit‑Logs mit pseudonymisierten Nutzer‑IDs, automatisierte Maskierung.

## 5. Offene Architekturentscheidungen
1. **API‑Gateway‑Strategie**: Interim‑Proxy (IP‑Allowlist + API‑Key) vs. schnelles Einbinden eines externen Managed API‑Gateway‑Anbieters.
2. **SSO‑Integration**: Ob das optionale Azure AD/Google‑SSO im MVP implementiert wird oder erst später.
3. **Managed Service Provider**: Auswahl des EU‑Only Providers (Kosten‑ und SLA‑Verhandlung steht aus).
4. **Rate‑Limiting‑Parameter**: Konkrete Schwellenwerte und dynamische Anpassungen müssen definiert werden.
5. **Backup‑RPO/RTO**: Vorgeschlagene Werte (RPO 4 h, RTO 2 h) müssen mit Operations‑Team finalisiert werden.
6. **Pilot‑Kunde (Deutschland vs. Schweiz)**: Beeinflusst mögliche separate Hosting‑Option für die Schweiz und Währungs‑/Datenschutz‑Anforderungen.
7. **Preis‑Cache‑Invalidierung**: Mechanismus zur Aktualisierung von Preis‑ und Produktdaten bei nächtlichen SAP‑Updates.

---
*Dieser Architekturüberblick wurde basierend auf den bereitgestellten Requirements, Risks und dem Projektkontext erstellt.*