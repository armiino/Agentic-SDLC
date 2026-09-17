# Offene Fragen und Klärungsbedarf

## Fachliche Fragen
- **Zielregion & Währung**: Welcher Pilot‑Kunde (DACH oder Schweiz) wird im MVP eingesetzt? Darf das Portal bereits CHF‑Preise unterstützen oder bleibt es ausschließlich EUR?  
  *Quelle*: Dialog‑Abschnitte zu Pilot‑Kunde, Währung (Zeilen 165‑176). 
- **Rabatt‑Freigabe‑Prozess**: Wie genau soll die Freigabe‑Logik für Rabatte > 15 % aussehen (Rollen, Schwellenwerte, Genehmigungs‑Workflow)?  
  *Quelle*: Aussagen von Eva zu Freigaben (Zeilen 134‑142, 194‑202). 
- **Support‑Prozess**: Ist ein reines Kontaktformular langfristig akzeptabel oder muss ein strukturiertes Ticket‑System (z. B. CRM‑Integration) bereits im MVP geplant werden?  
  *Quelle*: Diskussion mit David (Zeilen 122‑132, 150‑156). 
- **Mobile‑Strategie**: Wird ein reines Responsive‑Web‑Design als ausreichend betrachtet oder ist ein nativer Mobile‑App‑Ansatz für den Vertrieb zwingend erforderlich?  
  *Quelle*: Unterschiedliche Erwartungen von Anna und Sales (Zeilen 1‑4, 24‑30). 
- **SSO‑Umfang**: Soll SSO (Azure AD / Google) nur optional sein oder komplett weggelassen werden? Welche Impact‑Analyse gibt es für spätere Integration?  
  *Quelle*: Diskussion SSO/Identity‑Provider (Zeilen 46‑52, 71‑78). 
- **KPI‑Erfassung**: Wie sollen Conversion‑Rate und Time‑to‑Offer technisch gemessen werden, wenn keine externe Analytics‑Infrastruktur vorhanden ist?  
  *Quelle*: Hinweis auf KPI‑Erfassung ohne Analytics (Zeilen 98‑104). 
- **Backup‑ und DR‑Details**: Welche konkreten RPO/RTO‑Werte werden für das Backup‑ und Disaster‑Recovery‑Konzept erwartet?  
  *Quelle*: Erwähnung von Backup (Zeilen 222‑230) ohne genaue Vorgaben. 
- **Daten‑Retention vs. Lösch‑anfrage**: Wie wird das Konflikt‑Management zwischen gesetzlicher Aufbewahrungspflicht (z. B. 2 Jahre) und dem Recht auf Vergessenwerden umgesetzt?  
  *Quelle*: Diskussion zu Lösch‑ und Retention‑Regeln (Zeilen 84‑88, 166‑172). 
- **API‑Gateway Migration**: Bis wann soll das zentrale Unternehmens‑API‑Gateway an das Portal angebunden werden und welche Zwischenschritte sind für die Eigen‑Proxy‑Lösung geplant?  
  *Quelle*: Hinweis auf 6‑Wochen‑Warteliste (Zeilen 242‑250). 
- **SAP‑Verfügbarkeit & SLA**: Welche Service‑Level‑Agreements werden mit dem SAP‑Team für Leseberechtigungen definiert (Verfügbarkeit, Reaktionszeit bei Ausfall)?  
  *Quelle*: Kritische Abhängigkeit SAP (Zeilen 71‑78, 140‑148). 

## Technische Fragen
- **Cache‑Strategie für SAP‑Daten**: Soll ein Read‑Through‑Cache (z. B. Redis) eingesetzt werden, um SAP‑Ausfälle zu puffern, und wie wird die Daten‑konsistenz gewährleistet?  
  *Quelle*: Diskussion zu Cache‑Risiken (Zeilen 140‑148, 166‑176). 
- **Rate‑Limiting & Missbrauchserkennung**: Welche konkreten Schwellenwerte (Requests/Minute, Burst‑Size) werden im eigenen Proxy implementiert, bis das zentrale API‑Gateway verfügbar ist?  
  *Quelle*: Hinweis auf Rate‑Limiting (Zeilen 140‑148, 242‑250). 
- **Secrets‑Management‑Tool**: Welches Managed‑Secrets‑Service (Azure Key Vault, HashiCorp Vault, etc.) soll im MVP verwendet werden?  
  *Quelle*: Erwähnung Secrets‑Management (Zeilen 140‑148, 242‑250). 
- **Testdaten‑Strategie**: Wie werden synthetische Testdaten erzeugt, um das SAP‑Testsystem von echten Kundendaten zu entkoppeln?  
  *Quelle*: Hinweis auf reale Daten im SAP‑Testsystem (Zeilen 140‑148). 
- **Backup‑Implementierung**: Welche Technologie (z. B. Managed Cloud Snapshots, pg_dump) wird für das tägliche Backup des Managed DB‑Services verwendet?  
  *Quelle*: Backup‑Anforderung (Zeilen 222‑230). 
- **Audit‑Log‑Schema**: Welche Felder (User‑ID, Action, Entity, Timestamp, ggf. vorheriger/neuer Wert) sollen im Minimal‑Audit‑Log enthalten sein?  
  *Quelle*: Minimal‑Audit‑Log (Zeilen 84‑88, 166‑172). 
- **Hosting‑Kosten‑Schätzung**: Wie hoch ist das erwartete Preis‑niveau für einen EU‑only Managed Service im Vergleich zu einem Standard‑Anbieter?  
  *Quelle*: Diskussion zu Hosting‑Kosten (Zeilen 222‑230, 242‑250). 
- **Monitoring‑Konfiguration**: Welche Metriken (CPU, Antwortzeit, Fehlerrate) und Alert‑Kriterien werden ohne personenbezogene Daten im Monitoring definiert?  
  *Quelle*: Hinweis auf Monitoring ohne PII (Zeilen 242‑250). 

## Widersprüche, die geklärt werden müssen
- **Mobile‑First vs. Web‑First** – Sales möchte mobile Arbeit, PO fokussiert auf Web. Entscheidung für MVP muss eindeutig dokumentiert werden.  
- **SSO vs. E‑Mail‑Login** – SSO wurde mehrfach erwähnt, aber im MVP nur E‑Mail‑Login mit Double‑Opt‑In geplant.  
- **Rabatt‑Logik vs. Preis‑Aktualität** – SAP aktualisiert Rabatte nachts, aber Sales will sofort Angebote erstellen – potenzielles Risiko falscher Rabatte.  
- **Support‑Ticket vs. Kontaktformular** – Unterschiedliche Erwartungen an Support‑Prozesse, die im MVP nicht vollständig abgedeckt sind.  
- **API‑Gateway vs. Eigen‑Proxy** – Beide Lösungen werden diskutiert, aber keine endgültige Entscheidung für den MVP.  

## Fehlende Informationen / Artefakte
- **Architekturdokumente**: Es fehlt ein detailliertes Diagramm (C4‑ oder Component‑Diagram) zur Visualisierung der Komponenten‑ und Datenflüsse.  
- **Security‑Review‑Plan**: Konkreter Zeitplan, Ressourcen und Scope des vollständigen Security‑Reviews sind noch nicht definiert.  
- **Kosten‑Kalkulation**: Eine formale Kostenschätzung für EU‑only Managed Services und eventuelle Cloud‑Provider‑Auswahl ist noch ausstehend.  
- **Vertragsunterlagen**: Auftrags‑Verarbeitungs‑Verträge (AVV) für SAP‑ und Managed‑Service‑Provider liegen noch nicht vor.  
- **SLA‑Definitionen**: Service‑Level‑Agreements für SAP‑Verfügbarkeit und API‑Gateway‑Nutzung fehlen.  

## Mögliche Ansprechpartner / Rollen
- **Produktowner / Sales** – Anna (für Zielregion, Mobile‑Strategie, MVP‑Scope). 
- **Technical Lead / Architektur** – Ben (für SAP‑Integration, API‑Gateway, Cache‑Strategie). 
- **Compliance / Datenschutz** – Clara (für DSGVO‑Umsetzung, Lösch‑/Retention‑Konzept). 
- **Customer Support Lead** – David (für Support‑Prozess, Ticket‑System). 
- **Finance Lead** – Eva (für Rabatt‑Freigabe, Mehrwährung, Kosten‑Kalkulation). 
- **IT Operations / Infrastruktur** – Farid (für Hosting‑Kosten, EU‑Only‑Policy, Monitoring). 

---
*Alle offenen Fragen leiten sich ausschließlich aus den im Transkript festgehaltenen Aussagen und den bereits erstellten Artefakten (Context, Requirements, Risks, Architecture) ab.*