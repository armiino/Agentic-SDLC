# Projektkontext Zusammenfassung

## Projektziel
- **Hauptziel:** Schnellere Angebotserstellung für das Vertriebsteam (Conversion Rate erhöhen, Zeit bis Angebot reduzieren).
- **Sekundärziele:** Kundenportal für Bestellungs‑ und Rechnungsübersicht, Unterstützung von Mobile‑Zugriff, spätere Erweiterungen (Push‑Notifications, Analytics).

## Stakeholder & Rollen
- **Anna (Product Owner / Sales‑Vertreterin)** – Fokus auf MVP, schnelle Time‑to‑Market, definiert Kern‑Features (Portal, Login, Rollen, Angebot, Rechnung, KPI‑Messung).
- **Ben (Technical Lead / Backend‑Entwickler)** – Verantwortlich für API‑Layer, Integration mit SAP, Sicherheits‑ und Infrastruktur‑Fragen, realistische Aufwand‑Einschätzungen.
- **Clara (Compliance / Datenschutz‑Beauftragte)** – Kümmert sich um DSGVO‑Konformität, Logging, Audit‑Trails, Lösch‑ und Rollen‑Konzepte, Daten‑Hosting‑Standort.

## Fachliche Themen
| Thema | Relevante Punkte |
|------|-------------------|
| **Kundenportal** | Web‑first, Mobile optional, MVP in 8 Wochen, keine neue DB (Managed Services), EU‑only Hosting. |
| **Login & Auth** | E‑Mail/Passwort, optional SSO (Azure AD/Google), OAuth vs. API‑Keys, TLS ausreichend. |
| **Rollen & Berechtigungen** | Admin, User, Manager, Support; Rollen‑ und Berechtigungskonzept nötig. |
| **SAP‑Integration** | Stammdaten, Produkt‑/Preis‑/Rabatt‑Logik, Audit‑Trails. |
| **API‑Layer** | Notwendig für Integration, muss gesichert (OAuth empfohlen). |
| **DSGVO / Compliance** | Double‑Opt‑In, Logging, Audit‑Trail, Löschkonzept, Daten‑Hosting in EU, Tracking‑Einwilligung. |
| **Security Review** | Erforderlich, aber zeitkritisch (8 Wochen), möglicher Einsatz von Managed Services. |
| **KPIs** | Conversion Rate (Angebot → Bestellung), Zeit bis Angebot, ggf. später Analytics. |
| **Backup / DR** | Daten‑Backup und Disaster Recovery für Kundendaten zwingend. |
| **Skalierbarkeit** | Erwartete Nutzerzahlen 200 – 20 000, Architektur muss skalierbar, aber nicht über‑engineered sein. |

## Konflikte & Unsicherheiten
- **Zeit vs. Sicherheit:** MVP in 8 Wochen vs. notwendiger Security Review und DSGVO‑Compliance.
- **Budget vs. Infrastruktur:** Keine neue DB, jedoch Managed Services (Kosten vs. Skalierbarkeit).
- **Mobile vs. Web‑First:** Sales wünscht Mobile, aber Ressourcen knapp.
- **Feature‑Umfang:** Push‑Notifications und Analytics diskutiert, aber nicht im MVP.
- **Rollenkonzept & Dokumentation:** Notwendig für Compliance, aber wenig Zeit für ausführliche Docs.

## Offene Fragen / Risiken
- Wie detailliert muss das Lösch‑ und Rollen‑konzept für das MVP sein?
- Welcher Identity Provider (SSO) wird letztlich gewählt?
- Welche konkreten KPI‑Messungen werden implementiert?
- Wie wird die Skalierbarkeit technisch realisiert (z. B. Auto‑Scaling von Managed Services)?
- Gibt es interne Ressourcen für einen Architekten oder muss extern unterstützt werden?

## Quellen
- Transkript `input/transcripts/T9999_chaos.txt` (gesamter Dialog).

*Hinweis: Alle genannten Punkte leiten sich eindeutig aus den im Transkript geäußerten Aussagen der Stakeholder ab.*