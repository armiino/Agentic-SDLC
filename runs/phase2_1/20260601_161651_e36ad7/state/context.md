# Projektkontext aus Transkript T9999_chaos.txt

## Projektziel
Das zentrale Ziel ist die Entwicklung eines Kundenportals zur schnelleren und effizienteren Angebotserstellung und Rechnungsanzeige. Das Portal soll außerdem SAP-Daten integrieren und Rollenkonzepte für unterschiedliche Nutzer abbilden. Ein MVP (Minimum Viable Product) soll in etwa 8 Wochen realisiert werden.

## Sprecherrollen
- Anna: Projektmanagerin / Product Owner
- Ben: Technischer Experte / Entwickler
- Clara: Datenschutzbeauftragte
- David: Support-Verantwortlicher
- Eva: Finance / Controlling
- Farid: IT Operations / Infrastruktur

## Fachliche Themen
- Kundenportal (Web, mobile eventuell später)
- Angebotserstellung mit SAP-Produkt- und Preisdaten
- Rechnungsdownload
- Login mit E-Mail und Passwort, optional SSO (Azure AD, Google)
- Rollen und Berechtigungen (Admin, User, Manager, Support)
- DSGVO-Compliance (Double-Opt-In, Löschung, Audit-Trails, Datenresidenz EU)
- API Layer mit OAuth-Sicherung (noch offen)
- Managed Services für Hosting und Datenbank (kein neuer DB-Server)
- Backup und Disaster Recovery
- KPI-Messung (Conversion Rate, Zeit bis Angebot)
- PDF-Export von Angeboten mit rechtlichen Fußnoten
- Freigabeprozesse für Rabatte (MVP eingeschränkt)
- Supportprozess (Kontaktformular, aber kein Ticket-System im MVP)
- Mehrwährung (EUR, CHF, USD eventuell später)
- Lokalisierung und Internationalisierung (DACH geplant, EU und USA als Ausblick)

## Konflikte und Herausforderungen
- Zeit- und Budgetdruck (MVP in 8 Wochen vs. notwendige Security Reviews und Architektur)
- Unklare Scope-Abgrenzung und offene Fragen zu Pilotkunde und rechtlichen Anforderungen
- DSGVO und Datenschutz: Datenresidenz, Löschkonzepte, Audit, Tracking und Einwilligungen
- Technische Abhängigkeiten: SAP-Integration mit teilweise veralteten Systemen und Nichtverfügbarkeit
- Widersprüche zwischen Skalierbarkeit, Kosteneffizienz und Einfachheit
- Fehlende zentrale Identity und Access Management Lösung
- Support ohne Verwendung eines persistierenden Ticketsystems
- Hosting-Anforderungen: EU-only vs. Kosten
- API Gateway Verzögerungen und deren Auswirkungen auf Architektur

## Unsicherheiten und offene Fragen
- Endgültiger MVPScope in Bezug auf Rabatt-Freigabe, Support und erweitertes Rollenmodell
- Wahl des Identity Providers und Authentifizierungsverfahren
- Pilotkunde und geographischer Scope (DACH vs. EU vs. USA)
- Konkrete Hosting-Provider und deren DSGVO-Konformität
- Detaillierter Freigabeprozess für Sonderrabatte
- Umgang mit SAP-Systemverfügbarkeiten und Cache-Strategien
- Umfang und technische Umsetzung des Auditings
- Supportprozess und Datenpersistenz für Kundenanfragen
- Testumgebungen und Datenschutz bei Entwicklungsdaten

## Quellenhinweise
Diese Zusammenfassung basiert ausschließlich auf dem Stakeholder-Transkript T9999_chaos.txt vom Projektteam, in dem alle obigen Punkte diskutiert wurden.

## Zusammenfassung
Das Projekt befindet sich in einer frühen Phase mit vielen offenen Fragen und divergierenden Stakeholder-Erwartungen. Ein klarer MVP-Schnitt wurde angestrebt, der aber noch nicht abschließend validiert ist. Datenschutz, Security und technische Integration sind kritische Erfolgsfaktoren. Risiken und Zwiespälte sind dokumentiert und sollten bei weiteren Planungsschritten berücksichtigt werden.