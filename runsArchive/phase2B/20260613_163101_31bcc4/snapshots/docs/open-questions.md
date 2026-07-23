# Offene Fragen und Klärungsbedarfe im Kundenportal MVP

## 1. Offene fachliche Fragen

- Wer ist der finale Pilotkunde, und wie beeinflusst seine Auswahl die Anforderungen an:
  - Mehrwährung und Mehrsprachigkeit?
  - Datenschutzanforderungen insbesondere im internationalen Kontext (z.B. Schweiz, USA)?
- Wie soll der Supportprozess im MVP genau gestaltet werden?
  - Soll es bei der geplanten Minimalfunktion (Kontaktformular) bleiben?
  - Gibt es Pläne für Persistenz und Nachverfolgbarkeit von Supportanfragen?
- Wie wird mit Preisgültigkeit und Cache-Mechanismen bei der SAP-Anbindung umgegangen?
- Welche KPI- und Analytics-Anforderungen sind für später geplant, und welche sind explizit nicht Teil des MVP?

## 2. Offene technische Fragen

- Welches API-Sicherheitssystem wird final eingesetzt?
  - Wird OAuth verwendet, und wenn ja, in welchem Umfang (Client Credentials, Authorization Code, etc.)?
- Wie genau soll das Backup- und Monitoring-Konzept für das MVP aussehen?
  - Welche Daten werden gesichert, und wie oft?
  - Welche Monitoring-Tools und -Prozesse werden eingesetzt?
- Wie wird die Performance- und Skalierbarkeitsanforderung für spätere Releases dokumentiert und geplant?
- Wie wird die Integration des API-Gateways umgesetzt, vor allem vor dem Hintergrund vorhandener Budgetrestriktionen?
- Welche technischen Verantwortlichkeiten und Ansprechpartner werden für API-Security, Backup/Monitoring und Supportprozesse benannt?

## 3. Widersprüche und Spannungsfelder, die geklärt werden müssen

- Konflikt zwischen schnellem MVP-Zeitplan (8 Wochen) und notwendiger Tiefe bei Security Reviews und Compliance-Prüfungen.
- Budgetrestriktionen versus Anforderungen an Managed Services, API-Gateway und EU-only Hosting.
- Minimalistisches Rollen- und Freigabekonzept versus tatsächliche Anforderungen an Zugriffs- und Berechtigungskontrolle.
- Reduzierte Supportfunktionalität versus tatsächlicher Supportbedarf und Nutzererwartungen.
- Unklare Definition der API-Security in Kombination mit kritischer Abhängigkeit von SAP-Anbindung.

## 4. Fehlende Informationen

- Finaler Pilotkunde und dessen spezifische Anforderungen.
- Umfang und Zeitplan der geplanten Security Reviews.
- Backup- und Monitoring-Konzept inklusive technischer Details.
- Konkrete Dokumentation von Performance- und Skalierbarkeitsanforderungen für zukünftige Entwicklungen.
- Verantwortlichkeiten und Ansprechpartner zur Klärung offener technischer Themen.

## 5. Mögliche Ansprechpartner und Rollen

- Produktmanagement/Vertrieb (Anna) für fachliche Klärungen und Pilotkundenentscheidung.
- IT/Entwicklung (Ben) für API-Security, Performance, Backup- und Monitoringfragen.
- Datenschutz/Compliance (Clara) für DSGVO-relevante Fragestellungen.
- IT Operations (Farid) für Hosting, Backup und Betriebsaspekte.
- Kundenservice/Support (David) für Supportprozesse und Nutzerfeedback.

---

Diese offenen Fragen und Klärungen sind essenziell, um Risiken zu minimieren und den Projekterfolg sicherzustellen. Eine zeitnahe Beantwortung und die Benennung von Verantwortlichen wird empfohlen.
