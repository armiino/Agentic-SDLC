## Projektkontext – Zusammenfassung aus den Stakeholder‑Transkripten

**Projektziel (MVP‑Fokus)**
- Schnellere Angebotserstellung für den Vertrieb (Conversion Rate Offer → Bestellung) + Kunden‑Self‑Service (Bestellungen & Rechnungsdownload). 
- Das Kundenportal ist das Mittel, kein Endziel. 
- Ziel‑MVP‑Umfang (8 Wochen):
  1. **Login** (E‑Mail + Passwort, Double‑Opt‑In, grundsätzliche TLS‑Absicherung)
  2. **Angebotserstellung** auf Basis von SAP‑Produkt‑ und Preis‑Daten (lesender SAP‑API‑Layer, keine Sonderrabatte > Standard‑Satz)
  3. **Rechnungs‑ und Bestellungs‑Download** für bestehende Kunden
  4. **Rollenmodell** – Admin, Sales, Kunde (minimal)
  5. **Audit‑Trail** – Wer hat was gesehen/geändert (nur Kern‑Events)
  6. **EU‑Only Managed Hosting** (DSGVO‑konform, Backup & Disaster‑Recovery) 
  7. **Grundlegende Backup‑Strategie** (regelmäßige Snapshots, 30‑Tage‑Retention)

**Bewusste Einschränkungen im MVP**
- **Keine SSO‑Integration** (Azure AD/Google SSO nur später) 
- **Kein vollständiger Support‑Ticket‑System** – lediglich ein Kontaktformular, Support‑Daten werden per E‑Mail verarbeitet (bewusste Risiko‑Einschränkung). 
- **Kein komplexer Rabatt‑Freigabe‑Workflow** – Sonderrabatte > Standard werden im MVP nicht unterstützt. 
- **Kein vollwertiger API‑Gateway** (Warteliste 6 Wochen) – stattdessen ein einfacher interner API‑Layer (OAuth‑Ansatz geplant, aber nicht final). 
- **Keine Mehr‑Währungs‑ oder Internationalisierungs‑Funktion** (EUR + optional CHF‑Pilot, weitere Währungen später). 
- **Kein automatisches Pricing‑Cache** – Preis‑ und Rabattdaten werden in Echtzeit aus SAP gelesen; SAP‑Ausfall = kein Angebot (kritische Abhängigkeit). 
- **Kein umfangreiches Monitoring/Logging von personenbezogenen Daten** – technische Logs enthalten keine PII, getrennt von Audit‑Logs. 
- **Keine vollständige Dokumenten‑Management‑ oder PDF‑Template‑Versionierung** – minimaler PDF‑Export für Angebote, Revisionen werden im Audit‑Log erfasst. 
- **Keine vollständige Daten‑Retention‑Policy** – nur ein minimaler rechtlicher Aufbewahrungsplan (Handelsrecht × Jahre) wird später ausgearbeitet. 
- **Keine Test‑Daten‑Maskierung** – Entwicklungsumgebung muss synthetische Daten nutzen, kein Direktzugriff auf produktive SAP‑Daten. 

**Wesentliche Fachthemen & Anforderungen**
- **DSGVO / Compliance**: Double‑Opt‑In, Löschkonzept, Auftrags‑Verarbeitungs‑Verträge, Daten‑Residenz (EU‑Only), Rollen‑ & Berechtigungskonzept, getrennte Audit‑ vs. technische Logs, Aufbewahrungs‑ vs. Löschfristen. 
- **SAP‑Integration**: Nur lesender Zugriff für Produkt‑/Preis‑Daten im MVP; Schreibzugriff (Bestellungen, Freigaben) erst später. 
- **Security**: TLS‑Verschlüsselung, minimaler Audit‑Trail, 6‑Wochen‑Security‑Review later, Unterschied zwischen Application‑Logs, Audit‑Logs und Security‑Logs. 
- **Performance / Skalierbarkeit**: Erwartete Nutzerzahl stark variabel (200 – 20 000). MVP soll nicht over‑engineered, aber **skalierbare Architektur** (Managed Services, EU‑Region) planen. 
- **Backup & Disaster Recovery**: Regelmäßige Snapshots, 30‑Tage‑Retention, Wiederherstellungstests. 
- **KPI‑Tracking**: Conversion‑Rate, Zeit‑bis‑Angebot (keine personenbezogenen Daten). 
- **Reporting / PDF**: Angebots‑PDF mit rechtlichen Fußnoten, Versions‑ID, minimaler Audit‑Log‑Eintrag. 
- **Support‑Risiko**: Keine Ticket‑Datenbank → manuelle E‑Mail‑Verarbeitung (Datenschutz‑Risiko). 
- **Rollen & Berechtigungen**: Admin, Sales, Kunde (Support‑Rolle nur Leserechte, keine Preis‑ oder Rabatt‑Einblicke). 
- **Mehrsprachen**: MVP‑Sprachen = Deutsch + Englisch (später Internationalisierung). 
- **Umgebungen**: Dev / Test / Prod, synthetische Test‑Daten, Secrets‑Management (z. B. Vault), CI/CD‑Pipeline. 

**Identifizierte Konflikte & offene Fragen**
- **SSO vs. MVP‑Zeitplan** – SSO‑Integration würde Zeitbudget sprengen. 
- **Support‑Prozess** – Kontaktformular reicht nicht für SLA‑Anforderungen, aber wird als bewusste Einschränkung akzeptiert. 
- **Rabatt‑Freigabe** – Keine Sonderrabatte im MVP, muss später definiert werden (15 % / 30 % Freigaben). 
- **Daten‑Hosting** – EU‑Only Hosting ist teurer, genaue Kosten noch unbekannt (Kosten‑Schätzung bis Freitag nötig). 
- **SAP‑Verfügbarkeit** – kritische Abhängigkeit, Fallback‑Strategie (Cache) wird als Risiko notiert, aber nicht im MVP umgesetzt. 
- **Internationalisierung / Schweiz** – Schweiz‑Pilot könnte EU‑Hosting & DSGVO‑Erweiterungen benötigen. 
- **Mehr‑Währung** – CHF‑Pilot wird derzeit nicht im MVP berücksichtigt. 
- **Backup‑ und DR‑Details** – Auswahl des Managed‑Service‑Providers muss noch erfolgen. 
- **API‑Gateway‑Warteliste** – 6‑Wochen‑Verzögerung; MVP muss ggf. eigenen einfachen Gateway‑Proxy nutzen. 
- **Retention‑ vs. Löschkonzept** – Konflikt zwischen rechtlicher Aufbewahrungspflicht und Recht‑auf‑Vergessen‑Anspruch muss später geklärt werden. 

**Quellenhinweise** – Alle genannten Punkte stammen aus den Transkripten `input/transcripts/T9999_chaos.txt` (Stakeholder: Anna, Ben, Clara, David, Eva, Farid, und weitere). 

**Zusammenfassung**
Der Kontext zeigt ein ambitioniertes Vorhaben mit vielen fachlichen, sicherheits‑ und compliancerelevanten Anforderungen. Das MVP wird auf ein **minimal funktionsfähiges Kundenportal** reduziert, das Login, Angebotserstellung (ohne Sonderrabatte), Rechnungsdownload, ein simples Rollen‑ und Audit‑Modell sowie EU‑konformes Hosting und Backup beinhaltet. Alle nicht‑kritischen, risikobehafteten oder zeitintensiven Features (SSO, vollständiger Support‑Workflow, Rabatt‑Freigaben, API‑Gateway, Mehr‑Währung, Internationalisierung, umfassende Logging‑Strategien) werden bewusst ausgeschlossen, jedoch als **offene Risiken** dokumentiert. Diese Struktur ermöglicht eine realistische Planung für das 8‑Wochen‑MVP und legt die Basis für nachfolgende Phasen, in denen die offenen Punkte adressiert werden können.