# Architekturüberblick – Kundenportal (MVP)

## 1. Systemkontext
- **Ziel**: Bereitstellung eines webbasierten Kundenportals (Web‑First, responsive) zur schnellen Angebotserstellung und Rechnungsverwaltung. Das Portal soll innerhalb von **8 Wochen** als MVP lieferbar sein.
- **Stakeholder**: Produktowner (Anna), Technical Lead (Ben), Datenschutz (Clara), Support (David), Finance (Eva), IT Operations (Farid).
- **Externe Systeme**:
  - **SAP** – Lesender Zugriff auf Produkt‑, Preis‑ und Kundendaten (Schreibzugriff derzeit offen).
  - **Managed Service Provider** – EU‑konforme Hosting‑Umgebung (keine eigene Datenbank).
  - **Identity Provider (optional)** – Azure AD / Google für spätere SSO‑Integration.
  - **E‑Mail‑Service** – Double‑Opt‑In, Passwort‑Reset, Benachrichtigungen.

## 2. Wichtige Komponenten (MVP‑Scope)
| Komponente | Beschreibung | Verantwortlich |
|------------|--------------|----------------|
| **Web‑Frontend** | Responsive UI (Deutsch/Englisch) für Login, Angebots‑Erstellung, Rechnungs‑Download. | Frontend‑Team |
| **Auth Service** | E‑Mail/Passwort‑Login mit Double‑Opt‑In. SSO‑Hook (Platzhalter) für spätere Phasen. | Security‑Team |
| **API‑Gateway (einfach)** | Direkt‑exponierte REST‑API (grundlegende Authentifizierung, Rate‑Limiting). Ersatz‑Gateway wegen 6‑Wochen‑Warteliste. | Backend‑Team |
| **Angebots‑Service** | CRUD‑Operationen für Angebote, Status‑Workflow (Draft → Pending → Approved → Sent). Keine Sonderrabatte > 15 % im MVP. | Business‑Logic‑Team |
| **Rechnungs‑Service** | Bereitstellung und Download von Rechnungen (PDF). | Business‑Logic‑Team |
| **Rollen‑ & Berechtigung‑Service** | Minimal‑RBAC mit Rollen *Admin*, *Sales*, *Kunde*. Unterstützt zukünftige Erweiterungen (Support, Manager). | Security‑Team |
| **Audit‑Trail Service** | Aufzeichnung von Änderungen an Angeboten und Zugriffen (User‑ID, Timestamp). Trennung von technischen Logs (keine personenbezogenen Daten). | Compliance‑Team |
| **Backup / Disaster Recovery** | Tages‑Snapshots des Managed‑Service‑Speichers, Wiederherstellungspunkt‑Ziel (RPO) ≤ 24 h. | IT‑Operations |
| **SAP Integration Adapter** | Lesender Connector zu SAP (Produkt‑/Preis‑ und Kundendaten). Fehler‑Handling bei SAP‑Ausfall (Read‑Only‑Mode + Hinweis). | Integration‑Team |
| **Monitoring & Secrets‑Management** | Basis‑Monitoring (Health‑Checks, Error‑Rates). Secrets‑Store (z. B. HashiCorp Vault) für API‑Keys, DB‑Credentials. | Ops‑Team |

## 3. Schnittstellen / Integrationspunkte
- **Frontend ↔ Auth Service** – HTTPS POST/GET für Login, Double‑Opt‑In‑Flow.
- **Frontend ↔ API‑Gateway** – REST‑Endpoints für Angebote, Rechnungen, Rollen‑Info.
- **API‑Gateway ↔ Angebots‑/Rechnungs‑Service** – interne Service‑Calls (HTTP/JSON).
- **Angebots‑Service ↔ SAP Adapter** – Lesender API‑Call für Produkt‑/Preis‑Daten; keine Schreiboperationen im MVP.
- **Audit‑Trail ↔ Logging Infrastructure** – Event‑Streaming (z. B. Kafka/Message‑Queue) oder simple Log‑Datei, getrennt von Anwendungs‑Logs.
- **Backup Service ↔ Managed Storage** – Automatisierte Snapshots via Provider‑API.
- **Monitoring ↔ All Services** – Export von Metriken (Prometheus) und Alerts (Alertmanager).

## 4. Daten‑ und Sicherheitsaspekte
- **Datenminimierung**: Nur für das MVP notwendige Kundendaten (Login‑Info, Angebots‑Meta, Rechnungs‑Referenz) werden gespeichert.
- **Verschlüsselung**:
  - **In‑Transit**: TLS 1.2+ für alle Netzwerkverbindungen.
  - **At‑Rest**: Managed Service Provider stellt server‑seitige Verschlüsselung bereit (AES‑256).
- **DSGVO‑Compliance**:
  - Double‑Opt‑In beim Login.
  - Löschkonzept für Nutzer‑ und Angebotsdaten (Retention‑Plan noch offen).
  - Keine personenbezogenen Daten in technischen Logs.
  - EU‑Only‑Datenresidenz (Managed Service mit explizitem EU‑Region‑Tag).
- **Audit‑Log**: Jeder Change an Angeboten wird mit User‑ID, Timestamp und Aktion gespeichert – unterstützt spätere Revision und rechtliche Nachvollziehbarkeit.
- **Rate‑Limiting**: Grund‑Rate‑Limit (z. B. 100 Requests / Minute / User) über API‑Gateway‑Proxy, um DoS‑Risiken zu mindern.
- **Secrets‑Management**: Zugangsdaten für SAP‑Adapter und Datenbank werden sicher im Secrets‑Store verwaltet, nicht im Code.

## 5. Offene Architekturentscheidungen (MVP‑Scope & Risiken)
| Entscheidung | Status | Hinweis |
|--------------|--------|--------|
| **OAuth / SSO** | *Offen* – geplant für Phase 2, nicht im MVP implementiert. |
| **Cache‑Strategie** | *Offen* – kein Cache wegen DSGVO‑Risiko; Performance‑Risiko bei hoher Last. |
| **API‑Gateway** | *Temporär* – direkte API mit Basis‑Auth; später Umstieg auf zentrales Gateway nach Warteliste. |
| **Support‑Prozess** | *Bewusste Einschränkung* – nur Kontaktformular, kein Ticket‑System im MVP. Dokumentations‑Risiko. |
| **Mehr‑Währung** | *Offen* – EUR obligatorisch, CHF/​USD nur für spätere Phasen. |
| **SAP‑Schreibzugriff** | *Offen* – lesender Zugriff im MVP; Schreibzugriff für Auftrags‑/Bestellungs‑Synchronisation später. |
| **Backup‑Umfang** | *Definiert* – tägliche Snapshots; detaillierter RTO/RPO noch zu spezifizieren. |
| **Retention / Löschkonzept** | *Offen* – muss gesetzliche Aufbewahrungspflichten vs. Recht‑auf‑Löschung abwägen. |
| **Kosten für EU‑Only‑Hosting** | *Unklar* – Kostenschätzung noch ausstehend, beeinflusst Budget‑Freigabe. |
| **Test‑Daten** | *Offen* – Pseudonymisierung oder synthetische Daten für Dev/Test erforderlich. |

## 6. Zusammenfassung
Der grobe Architektur‑Entwurf definiert die Kern‑Komponenten, deren Schnittstellen und die wichtigsten Sicherheits‑ und Datenschutz‑Maßnahmen für das MVP. Offene Entscheidungen werden explizit gelistet, um Transparenz zu schaffen und zukünftige Phasen zu planen. Dieser Überblick leitet die Detail‑Design‑ und Implementierungs‑Schritte für das Kundenportal.

---
*Evidenz: Der Architekturüberblick stammt aus dem Projekt‑Kontext (`runs/phase2_1/20260602_094550_ed820c/state/context.md`), den abgeleiteten Requirements (`docs/requirements.md`) und den identifizierten Risiken (`docs/risks.md`).*