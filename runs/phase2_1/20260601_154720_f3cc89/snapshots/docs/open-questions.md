# Offene Fragen und Klärungsbedarfe

## 1. Fachliche offene Fragen

### 1.1 Pilotkunde und Scope
- Wer ist der finale Pilotkunde? DACH-Region oder Schweiz? Welche rechtlichen und währungsspezifischen Anforderungen müssen berücksichtigt werden?
- Welcher konkrete Kundenkreis soll mit dem MVP angesprochen werden?

### 1.2 Rabattfreigabeprozess
- Wie soll der Freigabeprozess für Sonderrabatte gestaltet und implementiert werden?
- Welche Rabattstufen benötigen welche Freigabe (z.B. ab 15%, 20%, 30%)?
- Ist der Rabatt-Freigabeprozess Teil des MVP oder Folgephasen?

### 1.3 Supportprozess
- Wie soll der Supportprozess langfristig gestaltet werden?
- Ist ein Ticketsystem geplant, und wenn ja, wann und wie wird es eingeführt?
- Wie sollen Supportanfragen datenschutzkonform dokumentiert werden?

### 1.4 KPIs und Monitoring
- Welche KPIs werden konkret gemessen und wie?
- Wie wird das Monitoring datenschutzkonform umgesetzt, insbesondere hinsichtlich Audit- vs. technischen Logs?

### 1.5 Internationalisierung und Mehrwährung
- Soll das Kundenportal zum Launch mehrsprachig (mindestens Deutsch und Englisch) angeboten werden?
- Welche Währungen müssen im MVP unterstützt werden? (EUR sicher, CHF bei Pilotkunden, USD später)

### 1.6 Mobile Nutzung
- Soll das Kundenportal von Beginn an mobilfähig sein? Falls ja, native App oder responsive Web?
- Wie priorisiert das Projekt die mobile Nutzung gegenüber Webfirst?

## 2. Technische offene Fragen

### 2.1 SSO und Identity Management
- Welche Identity Provider sollen eingebunden werden (z.B. Azure AD, Google)?
- Wird SSO verpflichtend umgesetzt im MVP oder erst später?
- Wie wird die Integration technisch realisiert (OAuth, andere Protokolle)?

### 2.2 SAP-Integration
- Welche SAP-Schnittstellen stehen zur Verfügung (lesend und evtl. schreibend)?
- Wie wird mit der Verzögerung bei Preis- und Rabattaktualisierungen umgegangen?
- Wie wird die Konsistenz der Daten bei Angeboten sichergestellt?

### 2.3 API Gateway
- Wird das zentrale API Gateway für das MVP genutzt trotz der Warteliste?
- Welche temporären Alternativen gibt es für API-Sicherheit und -Management?

### 2.4 Backup und Disaster Recovery
- Welche Backup- und Recovery-Anforderungen müssen im MVP abgedeckt werden?
- Wie werden die Anforderungen technisch umgesetzt?

### 2.5 Rollen- und Berechtigungskonzept
- Wie detailliert soll das Rollenmodell sein?
- Wie werden Widersprüche im Zugriff zwischen Support, Sales und Finance gelöst?
- Welche Auditing- und Logging-Anforderungen sind für Zugriffe und Änderungen vorgesehen?

### 2.6 Testdaten und Umgebungen
- Wie wird der Umgang mit Kundendaten in Test- und Entwicklungssystemen geregelt?
- Werden synthetische oder pseudonymisierte Daten genutzt?
- Wie wird die Qualität und Sicherheit der Testdaten sichergestellt?

### 2.7 PDF-Export und Versionsmanagement
- Wie wird die Versionsverwaltung der Angebots-Templates realisiert?
- Welche rechtlichen und datenschutzrelevanten Fußnoten müssen im PDF enthalten sein?

### 2.8 Performance und Skalierbarkeit
- Welche Nutzerzahlen sind für das MVP realistisch?
- Wie wird die Skalierbarkeit ohne Overengineering sichergestellt?

## 3. Widersprüche, Risiken und Unklarheiten

- Zielkonflikt zwischen umfangreichen Sicherheits- und Auditanforderungen und engem 8-Wochen-MVP-Termin.
- Spannungsfeld zwischen DSGVO-konformen Datenhaltungsanforderungen und dem Wunsch nach schneller Umsetzung ohne neuen Datenbankserver.
- Inkonsistenzen im Rollen- und Berechtigungskonzept, insbesondere im Support-Zugriff auf vertrauliche Daten.
- Mögliche Konflikte zwischen Aufbewahrungs- und Löschpflichten bei Kundendaten und Angeboten.
- Unklare Priorität und Umsetzung der API-Sicherheit vor Hintergrund fehlendem API Gateway und alternativen Lösungen.
- Offene Entscheidungen zu Pilotkunden und deren Einfluss auf Datenschutz-, Währungs- und Sprachkonzepte.

## 4. Ansprechpartner und Rollen (Vorschläge)

- Projektleitung / Product Owner: Anna
- IT-Architekt: Ben (aber aktuell nicht offiziell)
- Datenschutzbeauftragte: Clara
- Customer Support Lead: David
- Finance Verantwortliche: Eva
- IT Operations Verantwortlicher: Farid

---

**Hinweis:** Diese offenen Fragen basieren auf den einsehbaren Kontext-, Risiko- und Anforderungsdokumenten sowie dem Stakeholder-Transkript. Sie dienen als Grundlage für weitere Klärungen und dienen der Vermeidung von Missverständnissen im weiteren Projektverlauf.