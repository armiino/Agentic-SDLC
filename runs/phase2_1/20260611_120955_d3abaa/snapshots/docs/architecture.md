# Grober Architekturüberblick Kundenportal MVP

## Systemkontext
Das Kundenportal soll als webbasiertes Portal eine zentrale Anlaufstelle für Kunden bieten, um Angebote zu erstellen, Rechnungen einzusehen und Bestellungen zu verwalten. Das Portal ist eng mit SAP-Systemen integriert, um Stammdaten, Preise und Rabattlogik zu nutzen. Weitere Systeme sind vorgesehen, z.B. für Support (Kontaktformular) und eventuell externe Identity Provider für SSO.

Das Portal wird in einer EU-only-Hosting-Umgebung betrieben, basierend auf Managed Services ohne eigenen Datenbankserver. Sämtliche Datenübertragung nutzt TLS. Die Nutzer sind primär Sales-Mitarbeiter, Kunden, Administratoren und Support-Mitarbeiter.

## Hauptkomponenten

- **Benutzerverwaltung und Authentifizierung:** Login über E-Mail/Passwort mit Double-Opt-In für DSGVO, optionale SSO-Anbindung über Identity Provider (Azure AD, Google etc.), Rollen- und Berechtigungskonzept (Admin, Sales, Manager, Support).
- **Angebotsmanagement:** Erstellung, Verwaltung und Freigabe von Angeboten mit Anbindung an SAP für Preisdaten, Rabattlogik und Stammdaten.
- **Rechnungsverwaltung:** Anzeige und Download von Rechnungen, synchronisiert mit SAP.
- **API Layer:** Schnittstelle zwischen Portal und Backend-Systemen, insbesondere SAP, mit Authentifizierung (OAuth bevorzugt), darunter auch Managed Service Nutzung und Verschnittstellen.
- **Support-Kontaktformular:** Einfaches Formular zur Supportanfrage ohne persistentes Ticketsystem im MVP.
- **Audit- und Logging-Komponente:** Erfassung und Speicherung von Benutzeraktionen und Systemänderungen zur Einhaltung der Compliance und Nachvollziehbarkeit.
- **Backup und Disaster Recovery:** Geplante Funktionen zur Datensicherung und Wiederherstellung, entsprechend den Compliance-Anforderungen.

## Schnittstellen und Integrationspunkte

- **SAP-System:** Hauptdatenquelle für Preise, Kunden- und Produktdaten, Integration vorrangig lesend, Batch-Updates bei Preis- und Rabattdaten.
- **Identity Provider:** Für optionale SSO-Integration (OAuth, Azure AD, Google).
- **API Gateway:** Zentrale Verwaltungsinstanz für APIs, ggf. mit Warteliste und Verzögerung.
- **Managed Services:** Datenbank- und Hosting-Services mit EU-only-Verfügbarkeitsgarantie.
- **Externe Services:** Backup und Monitoring.

## Daten- und Sicherheitsaspekte

- **Datenschutz:** DSGVO-konforme Speicherung, Double-Opt-In beim Login, Löschkonzepte und Datenresidenz strikt in der EU.
- **Sicherheit:** Nutzung von TLS für Datenübertragung, OAuth als bevorzugtes Authentifizierungsverfahren, restriktive Rollen- und Berechtigungskonzepte.
- **Audit und Compliance:** Audit-Logs zur Erfassung von Änderungen und Zugriffen, getrennt von regulärem Logging, Minimierung personenbezogener Daten in technischen Logs.
- **Backup:** Regelmäßige Datensicherungen über Managed Services.

## Offene Architekturentscheidungen

- Auswahl und Integration eines Identity Providers für SSO (Azure AD, Google oder andere).
- Finalisierung des Authentifizierungsverfahrens (OAuth vs. API Keys).
- Umgang mit der SAP-Integration hinsichtlich Echtzeitdaten und Schreibzugriffen.
- Nutzung und Beschaffung des zentralen API Gateways unter Berücksichtigung der Wartelistenproblematik.
- Detaillierung der Backup- und Disaster-Recovery-Strategien.
- Umsetzung eines Freigabeprozesses für Rabatte in späteren Phasen.
- Definition eines Supportprozesses über das Kontaktformular hinaus in Folgephasen.
- Skalierungskonzepte zur Abdeckung von Nutzerzahlen zwischen 200 und 20.000.
- Entscheidung zur mobilen Nutzung (responsive Web vs. native App) für das MVP.

## Zusammenfassung
Dieser grobe Architekturüberblick stellt die funktionalen Kernkomponenten des geplanten Kundenportals dar und berücksichtigt die wesentlichen technischen, datenschutzrechtlichen und organisatorischen Rahmenbedingungen. Viele Details in der technischen Umsetzung und zukünftigen Erweiterungen bleiben bewusst offen, um Flexibilität bei nachfolgenden Architekturentscheidungen zu ermöglichen.

---

*Quelle: Abgeleitet aus dem Kontext- und Requirements-Dokument sowie der Risikoanalyse basierend auf dem Transkript input/transcripts/T9999_chaos.txt.*
