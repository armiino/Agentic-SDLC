# Offene Fragen und Klärungsbedarfe Kundenportal / Angebotsplattform

## 1. Offene fachliche Fragen

- Was ist die finale und verbindliche Zieldefinition des Projekts? Priorisierung zwischen Angebotserstellung, Rechnungsdownload, Push Notifications und weiteren Features fehlt eindeutig.
- Welche konkreten Rollen werden im Rollen- und Berechtigungskonzept benötigt? Gibt es eine Support-Rolle im MVP oder später?
- Wer ist verantwortlich für Stammdatenpflege und Supportprozesse?
- Wie sollen Marketing-Push-Notifications datenschutzkonform umgesetzt werden, insbesondere im Hinblick auf Einwilligungen?
- Welche KPIs sollen genau erhoben werden, und wie werden sie technisch erfasst und ausgewertet?

## 2. Offene technische Fragen

- Welche Frontend-Technologie wird final gewählt: Responsive Web, native App oder hybride Lösung?
- Wie wird die API Layer implementiert, insbesondere: OAuth oder API Keys als Absicherungsmechanismus?
- Wer übernimmt die Architektur-Verantwortung und wie wird der Security Review Prozess integrativ und effizient gestaltet?
- Welche konkreten Managed Services werden zur Datenhaltung, Backup und Disaster Recovery verwendet?
- Wie soll das Löschkonzept technisch umgesetzt werden, und wer steuert die Löschprozesse?
- Wie genau ist die Skalierbarkeit der Lösung zu planen, wenn Nutzerzahlen zwischen 200 und 20.000 variieren?
- Wie wird die Analytics-Infrastruktur aufgebaut, zumindest für die im MVP relevanten KPIs?
- Gibt es konkrete Anforderungen an Performance und Verfügbarkeit, insbesondere im MVP?
- Ist die geplante Hosting-Umgebung DSGVO-konform oder nur EU-only, und welche Anbieter kommen in Frage?

## 3. Widersprüche und Klärungsbedarfe

- Konflikt zwischen Zeitplan (8 Wochen MVP) und notwendigem Security Review (6 Wochen) muss aufgelöst werden.
- Widersprüchliche Anforderungen bezüglich Mobile (native vs. responsive Web) und Ressourcen (Budget, Backend-Readiness).
- Unterschiedliche Auffassungen über Scope (z.B. Push Notifications, Support-Tickets) und deren Priorisierung.
- Diskrepanz zwischen DSGVO-Anforderungen und technische Machbarkeit innerhalb des engen Zeitrahmens.

## 4. Fehlende Informationen

- Fehlen eines Architekten oder dedizierten technischen Leiters im Team.
- Genaue Nutzerzahlen und Erwartungen sind unsicher bzw. variieren stark.
- Detaillierte technische Dokumentation und Verantwortlichkeiten sind aktuell nicht vorhanden.
- Fehlen klar definierter Schnittstellen-Spezifikationen der SAP Integration.
- Unklare oder fehlende Daten zu Backuplösungen und Disaster Recovery Plänen.

## 5. Mögliche Ansprechpartner oder Rollen zur Klärung

- Product Owner / Sales (Anna) für Anforderungen und Priorisierung.
- Technischer Verantwortlicher / Backend (Ben) für technische Realisierbarkeit und Integration.
- Datenschutzbeauftragte (Clara) für DSGVO und Compliance-Fragen.
- Möglicher externer Architekturberater oder erfahrener Entwickler zur Unterstützung bei Architektur und Security.
- IT-Abteilung bezüglich Managed Services, Hosting und Infrastruktur.

---

*Dieses Dokument wurde auf Basis des Stakeholder-Transkripts (input/transcripts/T9999_chaos.txt), des kontextuellen Projektzusammenhangs (runs/phase2_1/20260526_140545_d7a2f1/state/context.md) sowie der abgeleiteten Anforderungen (docs/requirements.md), Risiken (docs/risks.md) und Architektur (docs/architecture.md) erstellt.*

