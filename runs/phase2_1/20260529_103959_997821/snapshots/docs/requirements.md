# Functional Requirements

1. Kundenportal-Webanwendung mit Login per E-Mail und Passwort (Double Opt-In erforderlich).
2. Rollenbasierte Zugriffssteuerung mit mindestens folgenden Rollen: Admin, Sales, Kunde, und Support mit eingeschränkten Rechten.
3. Erstellung und Verwaltung von Angeboten mit Statusmanagement (Draft, Pending Approval, Approved, Sent, Accepted, Rejected), jedoch im MVP nur ohne Freigabeprozesse für Rabatte über dem Standardsatz.
4. Anzeige und Download von Rechnungen und Bestellübersichten.
5. Integration mit SAP für lesenden Zugriff auf Produktdaten, Preise und Kundenstammdaten.
6. Export von Angeboten als PDF mit rechtlichen Fußnoten, Versionsnummern und datenschutzrelevanten Hinweisen.
7. Unterstützung von Managed Services für Datenhaltung und Betrieb, kein eigener Datenbankserver im MVP.
8. Umsetzung eines minimalen Audit Trails und Logging für Änderungen an Angeboten und Benutzeraktionen.
9. Implementierung eines Backup- und Disaster-Recovery-Konzepts.
10. API-Layer für Integration mit OAuth-Absicherung (bevorzugt, aber nicht final entschieden).
11. Unterstützung von KPIs wie Conversion Rate und Zeit bis Angebotserstellung.
12. Unterstützung von Web-Frontend mit mobiler Nutzung als mögliche spätere Erweiterung.
13. Support über Kontaktformular ohne persistentes Ticketsystem im MVP.
14. Umgebungskonzept mit getrennten Dev-, Test- und Produktionsumgebungen, Testdaten pseudonymisiert oder synthetisch.
15. Rate Limiting und Pagination für API-Endpunkte zur Vermeidung von Missbrauch.

# Non-functional Requirements

1. Das System muss DSGVO-konform sein, insbesondere hinsichtlich Datenlöschung, Auftragsverarbeitungsverträgen und Auditierbarkeit.
2. Hosting ausschließlich innerhalb der EU oder gemäß nachweislich DSGVO-konformen Richtlinien.
3. Sicherheit durch TLS-Verschlüsselung für Datenübertragung; Ende-zu-Ende Verschlüsselung ist nicht erforderlich.
4. Logs müssen zwischen technischen Logs, Audit-Logs und Security-Logs getrennt werden mit unterschiedlichen Aufbewahrungsfristen.
5. Das System muss skalierbar sein, jedoch ohne Overengineering, mit einer Benutzerzahl von ca. 200 bis 20.000 als Zielwert.
6. Dokumentation muss für Security Review und zukünftige Architekturzwecke vorhanden, aber minimal gehalten werden.
7. Backup- und Monitoring-Lösungen sind Pflicht und müssen Managed Services nutzen.
8. System muss Mehrsprachigkeit (mindestens Deutsch und Englisch) und Mehrwährungsfähigkeit (EUR, CHF, später USD) unterstützen.

# Constraints/Compliance

1. MVP-Zeitrahmen von 8 Wochen darf nicht für aufwändige Security Reviews oder Mehrfach-Frontends überschritten werden.
2. Keine neue Datenbank im MVP; nur Managed Services erlaubt.
3. Keine Sonderrabatte über Standardsatz im MVP ohne Freigabeprozess (Constraint zur Risikominderung).
4. Support mit nur Kontaktformular ohne persistente Ticketverwaltung ist eine bewusste Einschränkung und Risiko.
5. Hosting und Backup müssen EU-DSGVO-konform sein, auch wenn das Kostenrisiken birgt.
6. Daten dürfen in Logs keine personenbezogenen Informationen enthalten, um Datenschutzvorgaben einzuhalten.
7. API Gateway Nutzung ist geplant, aber wartelistenbedingt erst in späteren Projektphasen realistisch.
8. Pilotkunde noch nicht final definiert (Schweiz oder Deutschland), was Scope, Datenschutz und Währungsfragen beeinflusst.
9. Echtzeitpreisaktualisierung über SAP ist limitiert; Preisgültigkeit und Warnhinweise sind erforderlich.
10. Löschkonzepte und Datenklassifizierung sind zwingend, aber vollständig noch nicht im MVP abgedeckt.

# Traceability

| Requirement Nr. | Quelle |
|-----------------|---------|
| Functional Requirements 1-15 | input/transcripts/T9999_chaos.txt (Anna, Ben, Clara, David, Eva, Farid) |
| Non-functional Requirements 1-8 | input/transcripts/T9999_chaos.txt (Anna, Ben, Clara, David, Eva, Farid) |
| Constraints 1-10 | input/transcripts/T9999_chaos.txt (Anna, Ben, Clara, David, Eva, Farid) |

# Annahmen und offene Punkte

- SSO (z.B. Azure AD, Google) ist eine mögliche Erweiterung, aber nicht im MVP enthalten.
- Supportprozess mit Ticketsystem bleibt außerhalb des MVPs und ist als Risiko gekennzeichnet.
- API Layer Authentifizierung mittels OAuth wird präferiert, aber noch nicht final entschieden.
- Pilotkunde (Schweiz oder Deutschland) beeinflusst Datenschutz- und Mehrwährungsanforderungen und ist noch offen.
- Mobile Nutzung wird vorerst als responsive Web realisiert, native App ist ausgeschlossen im MVP.
- Ausbau der Rabatt-Freigabeprozesse erfolgt in späteren Phasen.
- Backup und Disaster Recovery sind Pflicht, aber Detailkonzepte noch zu präzisieren.
- API Gateway-Verfügbarkeit limitiert durch Warteliste, kurzfristig alternativer Zugang notwendig.
- Mehrsprachigkeit und Internationalisierung beginnen im DACH-Raum, Erweiterungen später.
