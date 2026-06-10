# Projektkontext – Zusammenfassung der Stakeholder‑Transkripte

## Projektziel
- Entwicklung eines **Kundenportals** (Web‑First, Mobile optional) zur **schnellen Erstellung und Verwaltung von Angeboten** sowie zum **Abruf von Rechnungen** für Kunden.
- Das Portal dient primär als *MVP* mit einer Lieferzeit von **8 Wochen**.
- Ziel ist es, **Angebote schneller zu erstellen** und damit die **Conversion Rate** (Angebot → Bestellung) zu erhöhen.
- Weitere angestrebte Funktionen (nicht im MVP) sind: Push‑Notifications, Online‑Angebots‑Akzeptanz, Mehr‑Währungs‑Support, umfassende Support‑Ticket‑Lösung, umfangreiche Analyse‑ und Tracking‑Features.

## Sprecherrollen (Stakeholder)
| Sprecher | Rolle / Verantwortungsbereich |
|----------|------------------------------|
| Anna | Product Owner / Business Stakeholder (zentraler Ansprechpartner, definiert Ziel & Prioritäten) |
| Ben | Technical Lead / Entwickler (architektonische und technische Fragen) |
| Clara | Datenschutz & Compliance (DSGVO, Auditing, Logging) |
| David | Customer Support (Support‑Prozesse, Kontaktmöglichkeiten) |
| Eva | Finance / Controlling (Rabatt‑Freigabe, rechtliche Anforderungen) |
| Farid | IT Operations / Infrastruktur (Hosting, EU‑Data‑Residency, Monitoring) |
| (Optional) weitere Stakeholder (z. B. Sales, Marketing) werden implizit erwähnt |

## Fachliche Themen (Kern‑ und Nebenbereiche)
1. **Login & Authentifizierung** – E‑Mail‑Login mit Double‑Opt‑In, optional SSO (Azure AD/Google) – MVP‑Version nur E‑Mail/Passwort.
2. **Angebots‑Workflow** – Erstellen, speichern, Freigabe (keine Sonderrabatte > 15 % im MVP), Status‑Tracking (Draft, Pending, Approved, Sent …).
3. **Rechnungs‑ und Bestellungs‑Download** – Kunden können Rechnungen einsehen und herunterladen.
4. **Rollen‑ und Berechtigungskonzept** – Admin, Sales, Kunde (und später Support); minimale Rollen im MVP.
5. **SAP‑Integration** – Lesender Zugriff auf Produkt‑/Preis‑ und Kundendaten; Schreibzugriff noch offen.
6. **Audit‑ und Logging‑Requirements** – Minimaler Audit‑Trail (Wer hat was geändert/gesehen), technische Logs ohne personenbezogene Daten.
7. **DSGVO‑Compliance** – Double‑Opt‑In, Löschkonzept, Auftrags‑Verarbeitungs‑Verträge, Daten‑Minimierung, EU‑Only‑Hosting.
8. **Managed Services & Hosting** – Nutzung von EU‑konformen Managed Services, keine neue DB‑Instanz.
9. **Backup & Disaster Recovery** – Basis‑Backup für Kundendaten im MVP.
10. **API‑Layer** – REST‑API, gesichert (OAuth bevorzugt, aber noch nicht final), Rate‑Limiting.
11. **Internationalisierung** – MVP: Deutsch & Englisch; später EU‑ und US‑Märkte.
12. **Mehr‑Währungs‑Support** – EUR verpflichtend, CHF optional für Pilot, USD später.
13. **PDF‑Export & Vorlagen** – Rechtlich konforme Angebots‑PDFs mit Versions‑ und Signatur‑Hinweisen.
14. **Monitoring & Secrets‑Management** – Trennung von Audit‑ und technischen Logs, Secrets‑Management für API‑Zugriff.

## Konflikte & Unsicherheiten (offene Punkte)
- **Umfang vs. Zeitplan**: Der aktuelle Funktionsumfang übersteigt das 8‑Wochen‑MVP, es müssen bewusste Einschränkungen definiert werden.
- **SSO & OAuth**: Erwünscht, aber nicht realistisch im MVP‑Zeitrahmen.
- **SAP‑Schreibzugriff**: Noch nicht geklärt, beeinflusst Angebots‑Freigabe‑Workflow.
- **Rabatt‑Freigabe**: Keine Sonderrabatte > 15 % im MVP, aber Finanz‑Stakeholder fordert später Freigabe‑Prozesse.
- **Support‑Prozess**: Kein Ticket‑System im MVP, nur Kontaktformular – Risiko für Datenschutz und UX.
- **Hosting‑Kosten EU‑Only**: Unklare Kostenschätzung, mögliche Budget‑Überschreitung.
- **API‑Gateway‑Verfügbarkeit**: Warteliste von 6 Wochen blockiert vorgesehenes Gateway.
- **Daten‑Retention & Löschkonzept**: Konflikt zwischen gesetzlichen Aufbewahrungspflichten und Recht‑auf‑Löschung.
- **Test‑Umgebungen**: Nutzung von echten SAP‑Daten in Dev/Test – Datenschutz‑Risiko.
- **Cache‑Strategie**: Kundenspezifische Preis‑ und Rabattdaten im Cache könnten DSGVO‑Probleme verursachen.
- **Backup‑ und Disaster‑Recovery‑Umfang**: Wie umfassend muss das im MVP sein?
- **Internationalisierung & Mehr‑Währung**: Pilot‑Kunde (Schweiz) unklar, beeinflusst Währungs‑ und Datenschutz‑Anforderungen.
- **Rate‑Limiting & Missbrauchserkennung**: Grundlegende Rate‑Limits nötig, aber kein vollständiges Abuse‑Monitoring im MVP.

## Quellenhinweise (Evidence)
- Transkript **T9999_chaos.txt** aus `input/transcripts/` (vollständiger Dialog aller genannten Stakeholder).
- Keine weiteren Dokumente wurden verwendet.

---
*Dieses Dokument fasst den aus den Transkripten abgeleiteten Projektkontext zusammen. Offene Punkte und bewusste Einschränkungen sind ausdrücklich gekennzeichnet, um spätere Entscheidungsfindungen zu unterstützen.*