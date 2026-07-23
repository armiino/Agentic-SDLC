# Projektkontext Kundenportal / Angebotsplattform

## Projektziel
- Hauptziel: Schnelleres Erstellen von Angeboten für Kunden (Sales Fokus).
- Kundenportal als Mittel zum Zweck, Ermöglichung von Bestellübersicht und Rechnungsdownload.
- Marketing möchte Push Notifications (später, im MVP nicht zwingend).
- MVP-Zeitplan: 8 Wochen.

## Stakeholder & Rollen
- Anna: Product Owner / Sales (Sales-Anforderungen und Zielvorstellung).
- Ben: Technische Umsetzung / Backend-Verantwortlicher.
- Clara: Datenschutz und Compliance (DSGVO, Audit, Security).

## Fachliche Themen
- Kundenportal (Web first, mobile App evtl. später).
- Login-Mechanismen: E-Mail/Passwort MVP, SSO optional (Azure AD, Google).
- Rollenmodell: Admin, normaler User, Manager, Support (Diskussion über Support-Funktion).
- SAP-Integration für Stammdaten, Angebotspreise, Rabattlogik.
- Angebotserstellung zentrales Feature.
- Rechnungsdownload und Bestellübersicht.
- Push Notifications und Tracking (DSGVO-relevant, evtl. später).
- API Layer zur Integration, OAuth bevorzugt, aber komplex.
- Logging, Audit Trails und Löschkonzept (DSGVO und Security).
- Backup und Disaster Recovery für Kundendaten.
- Hosting: EU-only oder DSGVO-konform.
- Skalierbarkeit: Unsicherheit über Nutzeranzahl (200 bis 20.000). Kein Overengineering.

## Konflikte und Unsicherheiten
- Frontend-Strategie (responsive Web vs. native App, mobiles Arbeiten).
- Budgetlimitationen und technische Umsetzbarkeit (kein neues DB-Server Deployment, Managed Services).
- Time-to-market vs. notwendige Security Reviews (8 Wochen MVP vs. 6 Wochen Security Review).
- Datenschutz und Compliance vs. Zeit- und Komplexitätsdruck.
- Unklare Zieldefinition zu Beginn, jetzt Fokus auf Angebotserstellung.
- Unklare Entscheidungen zu KPIs und Analytics (Conversion Rate, Zeit bis Angebot).
- Diskrepanz EU-DSGVO Konformität vs. tatsächliche Hosting-Anforderungen.
- Rollen- und Berechtigungskonzept unklar, Support nicht im MVP vorgesehen.
- Wer erstellt Architektur und Dokumentation nicht geklärt.

## Quellenhinweise
- Transkript: input/transcripts/T9999_chaos.txt

---

Dieses Kontextdokument fasst Schlüsselinformationen aus Stakeholder-Transkript zusammen zum Kundenportal-Projekt. Es dient als belastbare Grundlage für weitere Analyse- und Planungsarbeiten.