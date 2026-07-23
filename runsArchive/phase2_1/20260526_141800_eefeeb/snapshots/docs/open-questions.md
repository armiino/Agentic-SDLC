# Offene Fragen und Klärungsbedarfe

## Fachliche Fragen
- **Priorisierung von Feature‑Sets**
  - Welche Funktionen gehören zum MVP (Angebotserstellung, Rechnungsdownload, Login) und welche können in späteren Releases verschoben werden (Push‑Notifications, Mobile App, Analytics)?  
  - Quelle: Transkript (Diskussion über Kern‑ und Nice‑to‑Have‑Features).
- **KPI‑Definition und Messmethodik**
  - Wie genau sollen Conversion Rate und "Zeit bis Angebot" gemessen werden? Welche Tools werden dafür eingesetzt und wie wird DSGVO‑Konformität gewährleistet?  
  - Quelle: Transkript (Anna nennt KPIs, aber keine Messmethodik).
- **Rollen‑ und Berechtigungskonzept**
  - Welche konkreten Berechtigungen haben die Rollen Admin, Manager, User und Support? Wer ist für die Definition und Pflege verantwortlich?  
  - Quelle: Transkript (Unklarheit, wer das Rollen‑Konzept erarbeitet).
- **Lösch‑ und Auftragsverarbeitungs‑Prozess**
  - Wie wird das Recht auf Datenlöschung technisch umgesetzt? Welche Prozesse müssen dokumentiert werden?  
  - Quelle: Transkript (Kunden können Daten löschen, aber kein Prozess definiert).
- **Scope‑Definition des Portals**
  - Soll das Portal nur für Angebote und Rechnungen dienen oder auch weitere Self‑Service‑Funktionen (z. B. Vertragsverwaltung) enthalten?  
  - Quelle: Transkript (Mehrere mögliche Zielsetzungen genannt).

## Technische Fragen
- **API‑Layer‑Design**
  - Welche konkreten Endpunkte werden für die SAP‑Integration benötigt? Soll die API synchron oder asynchron (Message‑Queue) aufgebaut werden?  
  - Quelle: Architektur (Offene Entscheidung zu SAP‑Anbindung).
- **Authentifizierungs‑ und Autorisierungs‑Mechanismus**
  - Wird OAuth 2.0 als Standard für alle Services verwendet oder nur für das Frontend, während interne Calls API‑Keys nutzen?  
  - Quelle: Transkript & Architektur (OAuth vs. API‑Key als offenes Thema).
- **Identity Provider Auswahl**
  - Welche IdPs (Azure AD, Google) sollen unterstützt werden und ist SSO bereits im MVP‑Scope enthalten?  
  - Quelle: Transkript (SSO als optionales Feature).
- **Frontend‑Strategie**
  - Web‑first (responsive) oder Mobile‑first (native App) für das MVP? Welche Auswirkungen hat die Entscheidung auf Zeitplan und Budget?  
  - Quelle: Transkript (Diskussion über Mobile vs. Web).
- **Performance‑ und Skalierbarkeits‑Planung**
  - Wie wird die erwartete Nutzerzahl (200 – 20.000) für das MVP definiert und welche Last‑Tests sind geplant?  
  - Quelle: Transkript (unsichere Nutzerprognose).
- **Managed Service Provider Auswahl**
  - Welcher Cloud‑Provider (Azure, AWS, GCP) erfüllt EU‑only‑Hosting und Kostenanforderungen am besten?  
  - Quelle: Risiken (Kosten vs. Skalierbarkeit).
- **Backup & Disaster Recovery Umsetzung**
  - Welche konkreten RPO/RTO‑Ziele gelten und wie wird das Restore‑Testing organisiert?  
  - Quelle: Risks & Architecture (Backup notwendig, aber Details fehlen).
- **Audit‑Logging Detailgrad**
  - Soll jeder Datenbank‑Change komplett geloggt werden (Field‑Level) oder reicht Business‑Event‑Logging?  
  - Quelle: Risiken (Trade‑off zwischen Detailgrad und Performance).

## Widersprüche, die geklärt werden müssen
- **Zeitplan vs. Security Review**
  - 8‑Wochen‑MVP steht im Konflikt mit einem 6‑Wochen‑Security‑Review und vollständiger DSGVO‑Umsetzung. Wie wird dieser Konflikt gelöst?  
  - Quelle: Risiken & Transkript.
- **Kosten vs. Skalierbarkeit**
  - Günstige Managed Services vs. notwendiges Auto‑Scaling für bis zu 20.000 Nutzer – welche Kostengrenzen gelten?  
  - Quelle: Risiken.
- **Frontend‑Umfang**
  - Web‑only MVP vs. gleichzeitige Entwicklung einer nativen Mobile‑App – was ist realistisch innerhalb des Zeitrahmens?  
  - Quelle: Transkript.
- **Compliance‑Umfang im MVP**
  - Welche Minimum‑DSGVO‑Maßnahmen (Double‑Opt‑In, Logging, Backup) müssen bereits im MVP implementiert sein, um rechtlich abgesichert zu sein?  
  - Quelle: Risiken & Transkript.

## Fehlende Informationen / Artefakte
- **Projektplan mit Meilensteinen**
  - Detaillierter Zeitplan (Sprint‑Aufteilung, Review‑Termine) fehlt.
- **Budget‑Übersicht**
  - Konkrete Kostenschätzung für Managed Services, Security Review und eventuell nötige Lizenz‑Gebühren (IdP, SAP‑Adapter).
- **Rollen‑ und Verantwortlichkeitsmatrix**
  - Wer übernimmt das Architekturreview, das Security‑Review und das Compliance‑Management?
- **Detail‑Spezifikation der SAP‑Schnittstelle**
  - Welche SAP‑Module werden genutzt, welche Datenformate und Auth‑Methoden sind nötig?
- **Auswahlkriterien für Cloud‑Provider**
  - Kriterien‑Katalog für EU‑only‑Hosting, Kosten, Service‑Level‑Agreements.
- **KPI‑Messstrategie**
  - Gewünschte Analytics‑Toolchain (z. B. Matomo, Google Analytics) und Datenschutz‑Plan.

## Mögliche Ansprechpartner / Rollen
- **Produktowner / Business Analyst** – Anna (Klärung von Feature‑Prioritäten, KPI‑Definition).
- **Technischer Lead / Architekt** – Ben (API‑Design, Infrastruktur‑Auswahl, Performance‑Planung).
- **Compliance‑Officer / Datenschutz‑Beauftragter** – Clara (DSGVO‑Umsetzung, Double‑Opt‑In, Audit‑Logging).
- **Security Engineer** – Externer oder interner Spezialist für den Security Review.
- **SAP‑Integration Specialist** – Verantwortlich für SAP‑Adapter und Datenkohärenz.
- **Cloud‑Provider Account Manager** – Unterstützung bei EU‑Only‑Hosting und Kostenoptimierung.

*Diese Fragen wurden aus den vorliegenden Artefakten (Transkript, Kontext, Requirements, Risks, Architecture) abgeleitet und bilden die Basis für die weitere Klärung in Phase 3.*