# Risikoartefakt für Kundenportal MVP

## Fachliche Risiken

- **Unklare Definition des Pilotkunden (Länder- und Währungsproblematik):** Die Entscheidung zwischen einem Schweizer oder deutschen Pilotkunden ist noch offen. Dies kann zu Anforderungen führen, die inkompatibel sind oder zu Nacharbeiten im Datenschutz (DSGVO vs. Schweizer Datenschutz), der Währungslogik (EUR vs. CHF etc.) und Sprachanforderungen.
- **Fehlender Freigabeprozess für Rabatte im MVP:** Ohne klare Freigabelogik (mind. bei Rabatten über 15 %) könnten fehlerhafte Angebote die Kunden erreichen, was finanzielle Risiken (Verluste, Verlust von Kundenvertrauen) nach sich zieht.
- **Unklare Supportprozesse:** Das Fehlen eines Ticketsystems im MVP und die Nutzung eines Kontaktformulars ohne Persistenz kann zu ineffizienter Bearbeitung, Datenverlust und schlechter Kundenzufriedenheit führen.

## Technische Risiken

- **Zeitdruck für MVP und fehlender Security Review:** Der 8-Wochen-Zeitplan steht im Widerspruch zu nötigen Sicherheitsprüfungen. Eine verzögerte oder nicht durchgeführte Security Review kann zu Sicherheitslücken führen.
- **SAP-Verfügbarkeit als kritische Abhängigkeit:** Die Unzuverlässigkeit und begrenzte Echtzeitfähigkeit von SAP wirkt sich direkt auf die Angebotsgenauigkeit und Funktionalität aus.
- **API Gateway Warteliste als Blocker:** Sechs Wochen Warteliste für das zentrale API Gateway können die Integrationsfähigkeit und damit den gesamten MVP-Launch behindern.
- **Fehlende Testabdeckung aufgrund von Datenschutz im Testumfeld:** Das Nutzen von echten Kundendaten in Testumgebungen ist problematisch, jedoch ist eine saubere Testdatenbereitstellung noch offen.
- **Datenschutzkonflikte durch Logging und Auditing:** Die Balance zwischen Nachvollziehbarkeit und Datenschutz ist kritisch, insbesondere bei der Vermeidung personenbezogener Daten in technischen Logs.

## Compliance- und Datenschutzrisiken

- **DSGVO-Konformität nicht abschließend geklärt:** Die Anforderungen an Double-Opt-In, Löschkonzepte und Auftragsverarbeitungsverträge sind komplex und können Compliance-Verstöße verursachen.
- **Datenresidenz und EU-only Hosting:** Ohne nachweisbare Datenresidenz können Bußgelder und Imageverluste drohen.
- **Lösch- und Aufbewahrungskonflikte:** Rechtliche Aufbewahrungsfristen stehen im Konflikt mit dem Recht auf Datenlöschung, was zu organisatorischen und technischen Herausforderungen führt.

## Widersprüche und Unsicherheiten

- MVP-Zeitrahmen versus notwendige Sicherheits- und Datenschutzprüfungen.
- Wunsch nach umfangreichen Funktionalitäten (z. B. SSO, Push Notifications, KPIs) versus pragmatischem MVP-Scope.
- Unterschiedliche Anforderungen und Erwartungen der Stakeholder (Sales, Datenschutz, IT, Support).
- Unklare oder nicht abschließend definierte KPI-Indikatoren und Analytics-Strategien.

## Mögliche Auswirkungen

- Verzögerung des MVP-Launches oder Erhöhung der Kosten durch Nacharbeiten und fehlende klare Spezifikationen.
- Sicherheitslücken und Compliance-Verstöße mit rechtlichen und finanziellen Folgen.
- Nutzerunzufriedenheit aufgrund unvollständiger oder fehlerhafter Funktionalitäten.
- Risiken für Kundenbeziehungen durch falsche Angebotsdaten oder mangelhafte Supportprozesse.

## Gegenmaßnahmen und Klärungsbedarfe

- Klare Definition des Pilotkunden und finaler Scope bzgl. Geografie, Währungen und Datenschutz.
- Festlegung und Umsetzung eines einfachen, aber verbindlichen Freigabeprozesses für Rabatte im MVP.
- Planung und zeitliche Reservierung für einen Security Review im Projektplan.
- Entwicklung von sauberen Testdatenstrategien und Pseudonymisierung im Testbereich.
- Frühzeitige Kommunikation und Koordination mit dem API Gateway Team, ggf. temporäre Workarounds.
- Dokumentation und Rollen- und Berechtigungskonzepte zur Datenschutzkonformität insbesondere bei Audit und Support.
- Definition pragmatischer, auf MVP-Level beschränkter KPI und Analytics-Anforderungen.
- Regelmäßige Abstimmung der Stakeholder zur Minimierung von Widersprüchen und zur Klärung von Anforderungen.

---

Dieses Risikoartefakt basiert auf der Analyse des Stakeholder-Transkripts und des daraus abgeleiteten Projektkontexts sowie der Requirements-Dokumentation, die in den Artefakten runs/phase2_1/20260612_081029_1b1442/state/context.md und docs/requirements.md dokumentiert sind.