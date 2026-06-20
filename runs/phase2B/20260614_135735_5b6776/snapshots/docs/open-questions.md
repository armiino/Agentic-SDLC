# Offene Fragen und Klärungsbedarfe im Kundenportal-Projekt

## 1. Fachliche offene Fragen
- Wie genau sollen die Supportprozesse im MVP gestaltet werden, um trotz rudimentärer Umsetzung (kein Ticketsystem) eine akzeptable Kundenzufriedenheit zu gewährleisten?
- Welche konkreten Anforderungen und Rollenzuweisungen gelten für die Support-Rolle im MVP?
- Wie wird der Rabattfreigabeprozess nach dem MVP ausgestaltet, um spätere Nacharbeiten und Abstimmungsprobleme zu minimieren?
- Wer sind die zuständigen Ansprechpartner für Klärungen zu Rabattlogiken und Supportprozessen?

## 2. Technische offene Fragen
- Wie wird die Integration mit dem nicht vollständig API-readyen SAP Backend technisch erfolgen, insbesondere im Hinblick auf Echtzeit-Angebote und Preisdatenaktualität?
- Wie genau soll die Datenhaltung im MVP umgesetzt werden, da keine neue Datenbank eingeführt werden darf? Welche bestehenden Systeme sind dafür vorgesehen?
- Wie wird die Einbindung des API-Gateways trotz Warteliste und Kapazitätsengpässen technisch und zeitlich realisiert?
- Wie wird der Anspruch auf EU-only Managed Hosting technisch und organisatorisch sichergestellt, insbesondere Backup- und Disaster-Recovery-Konzept?

## 3. Widersprüche und Unklarheiten
- Wie wird der zeitliche Konflikt zwischen dem 8-Wochen-MVP-Zeitfenster und notwendigen vollständigen Sicherheitsreviews, Tests und Compliance-Anforderungen gelöst?
- Wie kann der rudimentäre Support ohne Ticketsystem mit der angestrebten Kundenzufriedenheit in Einklang gebracht werden?
- Wer ist der Pilotkunde konkret (DACH, Schweiz, USA) und welche konkreten Datenschutzvorgaben ergeben sich daraus?
- Wie werden Audit-Trails, Löschkonzepte und DSGVO-konforme Nachweise final definiert und umgesetzt?

## 4. Fehlende Informationen
- Endgültige und detaillierte Spezifikation der Audit-Trails, Löschkonzepte und Sicherheitskonzepte.
- Klare Definition der Datenschutzanforderungen abhängig vom Pilotkunden.
- Konkrete technische Spezifikation der vorhandenen Datenhaltungssysteme und der SAP-Schnittstellen.

## 5. Potenzielle Ansprechpartner und Rollen
- Anna (Projekt-/Produktverantwortliche) – fachliche Priorisierung und MVP-Scope
- Ben (Technischer Experte/Architekt) – SAP-Integration, API-Layer, technische Infrastruktur
- Clara (Datenschutz/Compliance) – DSGVO, Audit-Trails, Löschkonzepte
- Eva (Finanzen) – Rabattlogiken, Freigabeprozesse
- Farid (IT Operations) – Managed Hosting, Backup, Infrastruktur
