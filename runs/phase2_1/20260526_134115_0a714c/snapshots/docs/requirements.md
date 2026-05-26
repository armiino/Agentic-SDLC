# Anforderungen des Kundenportals

## Funktionale Anforderungen

1. Kundenportal (Web First) zur Anzeige von Bestellungen und Rechnungen.
2. Möglichkeit für Kunden, Angebote zu erstellen (Sales-Hauptziel).
3. Login-System mit E-Mail und Passwort im MVP.
4. Optionale Single-Sign-On (SSO) Integration (Azure AD, Google) als Wunsch, aber noch unklar.
5. Rollenmodell mit mindestens Admin, normaler User, Manager; Support-Rolle wird diskutiert, aber initial nicht eingeplant.
6. Angebotsgenerierung soll Produktdaten, Preise und Rabattlogik aus SAP integrieren.
7. Kunden sollen Rechnungen downloaden können.
8. Logging und Audit Trails, vor allem wer was wann geändert hat.
9. Push Notifications werden als Marketingwunsch genannt, aber im MVP eventuell nicht berücksichtigt.

## Nicht-funktionale Anforderungen

1. Datenschutz und Compliance:
   - DSGVO-konforme Speicherung und Verarbeitung von Kundendaten.
   - Double-Opt-In für Login per E-Mail.
   - Löschkonzepte für Kundendaten müssen vorhanden sein.
   - Auftragsverarbeitungsverträge sind notwendig.
   - Auditierbarkeit (Logging) ist verpflichtend.
2. Sicherheit:
   - Verschlüsselung per TLS ausreichend, keine Ende-zu-Ende Verschlüsselung notwendig.
   - API-Sicherung bevorzugt mit OAuth, API Keys als Alternative.
   - Security Review ist verpflichtend, dauert jedoch 6 Wochen, daher Konflikt mit MVP-Zeitplan.
3. Infrastruktur:
   - Nutzung von Managed Services bevorzugt.
   - Kein neuer Datenbankserver.
   - Hosting innerhalb der EU oder DSGVO-konform.
4. Skalierbarkeit: 
   - Unsicherheiten zur Anzahl der Nutzer (200 bis 20.000) und entsprechend erforderliche Skalierbarkeit.
5. Backup und Disaster Recovery sind erforderlich, speziell für Kundendaten.
6. Performance und KPI-Messung:
   - KPIs wie Conversion Rate und Zeit bis Angebot.
   - Analytics Infrastruktur ist im MVP nicht vorgesehen, könnte später ergänzt werden.

## Constraints und Compliance Anforderungen

1. MVP Zeitrahmen 8 Wochen.
2. Kein Overengineering trotz Skalierbarkeitsanforderungen.
3. Komplexe Sicherheits- und Datenschutzmaßnahmen müssen innerhalb des Zeitrahmens realisierbar sein.
4. Unvollständige Stammdaten in SAP stellen eine Unsicherheit dar.

## Offene Punkte und Annahmen

- Entscheidung zu native App vs. responsive Web ist noch offen.
- Umfang und Zeitplan für Security Review müssen noch geklärt werden, da Zeitrahmen knapp ist.
- Umfang der KPIs und ob Analytics im MVP enthalten ist, ist noch unsicher.
- Ob SSO im MVP realisiert wird, ist unklar.
- Nutzung von Push Notifications im MVP ist unklar.
- Support/Ticketsystem wird vorerst ausgeschlossen, aber künftige Integration möglich.
- Unklarheit über genaue Nutzerzahlen führt zu Unsicherheit bei Skalierungsanforderungen.
- Dokumentationsaufwand für Security Review ist als kritisch angesehen.

## Traceability

Alle Anforderungen und offenen Punkte basieren auf den Aussagen aus dem Stakeholder-Transkript (input/transcripts/T9999_chaos.txt) und dem extrahierten Kontext (runs/phase2_1/20260526_134115_0a714c/state/context.md).