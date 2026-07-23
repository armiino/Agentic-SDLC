# Offene Fragen und Klärungsbedarf

## Fachliche Fragen
- **Pilotkunde und Zielmarkt**: Wer ist der finale Pilotkunde (Deutschland vs. Schweiz)? Dies beeinflusst Währung (EUR/CHF), Datenschutz (EU vs. CH) und Hosting‑Vertrag. *(Quelle: Transkript – Eva, Farid)*
- **Mehrwährungs‑Support**: Müssen im MVP CHF‑Rechnungen erstellt werden oder reicht EUR? Welche Währungslogik ist zwingend? *(Quelle: Transkript – Eva, Farid)*
- **Rabatt‑Freigabe‑Prozess**: Wie genau soll der Freigabe‑Workflow für Rabatte > 15 % aussehen (Rollen, Schwellen, Eskalation zu Finance)? *(Quelle: Transkript – Eva)*
- **Support‑Workflow**: Wird ein Ticket‑System im MVP benötigt oder reicht ein strukturiertes Kontaktformular mit Zuordnung zum Kundenkonto? Wie wird das Datenschutz‑Risiko bei E‑Mail‑Support gelöst? *(Quelle: Transkript – David, Clara)*
- **KPI‑Definition**: Welche konkreten Kennzahlen sollen gemessen werden (Conversion‑Rate, Time‑to‑Offer, etc.) und wie werden sie technisch erfasst? *(Quelle: Transkript – Anna, Ben)*
- **Finaler Mobile‑Ansatz**: Wird ein responsives Web‑Portal ausreichend sein oder ist eine native App (iOS/Android) Teil des MVP? *(Quelle: Transkript – Anna, Ben)*
- **SSO / Identity Provider**: Ist Azure AD oder Google Identity im MVP überhaupt zulässig oder nur für spätere Phasen? *(Quelle: Transkript – Anna, Ben)*
- **Backup‑ und Disaster‑Recovery‑Details**: Welche RPO/RTO‑Werte sind akzeptabel und wer ist verantwortlich für die Implementierung? *(Quelle: Transkript – Clara, Ben)*
- **Retention‑ und Aufbewahrungspflichten**: Wie viele Jahre müssen Angebote/Rechnungen gesetzlich archiviert werden und wie wird das mit dem Recht auf Löschung vereinbart? *(Quelle: Transkript – Clara, Eva)*
- **Test‑Daten‑Strategie**: Welche Methode (synthetische Daten, Maskierung) wird für Development‑ und Test‑Umgebungen verwendet, um DSGVO‑Verstöße zu vermeiden? *(Quelle: Transkript – Ben, Clara)*

## Technische Fragen
- **Managed DB‑Service Auswahl**: Welcher EU‑Only Managed Database‑Service (z. B. Azure PostgreSQL, AWS RDS EU) soll verwendet werden und welche Kosten entstehen? *(Quelle: Transkript – Farid, Ben)*
- **API‑Gateway‑Alternative für MVP**: Welche Sicherheits‑Mechanismen (OAuth, API‑Keys, Rate‑Limiting) werden ohne das zentrale API‑Gateway implementiert? *(Quelle: Transkript – Ben, Farid)*
- **Secrets‑Management**: Welches Tool/Provider (HashiCorp Vault, Cloud‑Native Secrets) wird bereits im MVP eingesetzt? *(Quelle: Transkript – Farid)*
- **Logging‑ und Audit‑Log‑Implementierung**: Wie wird die Trennung zwischen Application‑Log, Audit‑Log und Security‑Log technisch umgesetzt und welche Anonymisierungs‑Regeln gelten? *(Quelle: Transkript – Clara, Ben)*
- **Hosting‑ und Datenresidenz‑Nachweis**: Wie wird nachgewiesen, dass sämtliche Daten ausschließlich in EU‑Regionen gespeichert werden (DPA, Vertragsklauseln)? *(Quelle: Transkript – Farid, Clara)*
- **Rate‑Limiting & Missbrauchserkennung**: Welche konkreten Limits (z. B. Requests / Minute pro Nutzer) und welche Monitoring‑Tools werden für das MVP genutzt? *(Quelle: Transkript – Ben, Clara)*
- **SAP‑Integrationsdetails**: Welche SAP‑BAPIs oder OData‑Services werden für das Lesen von Produkt‑/Preis‑Daten verwendet und gibt es ein SLA für deren Verfügbarkeit? *(Quelle: Transkript – Ben)*
- **PDF‑Template‑Management**: Wie wird die Versionierung und revisionssichere Erstellung von Angebots‑/Rechnungs‑PDFs technisch umgesetzt? *(Quelle: Transkript – Eva, Farid)*
- **Internationalisierung (i18n)**: Sind Deutsch und Englisch bereits vollständig lokalisiert oder müssen weitere Ressourcen (Übersetzungs‑Dateien, Locale‑Switch) implementiert werden? *(Quelle: Transkript – Anna, David)*
- **Backup‑Strategie im Managed Service**: Unterstützt der gewählte Provider Point‑in‑Time‑Recovery und wie wird die Backup‑Retention konfiguriert? *(Quelle: Transkript – Clara, Ben)*

## Widersprüche, die geklärt werden müssen
- **Mobile vs. Web‑First** – Auf der einen Seite wird Mobile first gefordert, auf der anderen nur ein Web‑Portal als MVP. Wie wird dieser Konflikt gelöst? *(Transkript – Anna vs. Ben)*
- **SSO vs. Simple Login** – SSO (Azure AD/Google) wird als Wunsch genannt, aber das MVP soll nur E‑Mail‑Login mit Double‑Opt‑In haben. Ist das ein bewusster Ausschluss? *(Transkript – Anna, Ben)*
- **Support‑Ticket‑System vs. Kontaktformular** – Support‑Team fordert ein Ticket‑System, das Projektteam will nur ein Kontaktformular. Welcher Ansatz wird im MVP realisiert? *(Transkript – David vs. Anna)*
- **API‑Gateway‑Wartezeit vs. 8‑Wochen‑MVP** – Das zentrale API‑Gateway hat eine 6‑Wochen‑Warteliste, das MVP‑Zeitfenster ist 8 Wochen. Wie wird die notwendige Sicherheit ohne das Gateway gewährleistet? *(Transkript – Farid, Ben)*
- **Rabatt‑Freigabe vs. MVP‑Ausschluss** – Sonderrabatte > 15 % sollen erst später, aber das Finanzteam sieht das als kritisches Risiko. Wie wird das im MVP‑Scope behandelt? *(Transkript – Eva vs. Anna)*
- **Daten‑Retention vs. Recht auf Vergessenwerden** – Gesetzliche Aufbewahrungspflichten kollidieren mit dem DSGVO‑Löschrecht. Welche Priorität hat jede Anforderung? *(Transkript – Clara, Eva)*

## Mögliche Ansprechpartner / Rollen
- **Produkt‑Owner / Business Stakeholder** – Anna (Projektleitung)
- **Technische Leitung** – Ben (Entwicklung)
- **Datenschutz / Compliance** – Clara
- **Finanzen / Controlling** – Eva
- **Customer Support Lead** – David
- **IT Operations / Infrastruktur** – Farid
- **Legal / Vertragsprüfung** – (nicht im Transcript genannt, aber notwendig für Datenresidenz & Aufbewahrung)

*Alle offenen Fragen leiten sich eindeutig aus den vorhandenen Artefakten (Transkript, Kontext‑ und Risiko‑Dokumente) ab und sind für die weitere Anforderungs‑ und Architektur‑Klärung notwendig.*