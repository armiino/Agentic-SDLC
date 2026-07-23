# Architekturüberblick Kundenportal

## Systemkontext
Das Kundenportal ist Teil eines größeren IT-Ökosystems, in das vor allem SAP für Produkt-, Preis- und Kundendaten integriert wird. Ziel ist, Kunden eine Plattform für Angebotserstellung, Bestellübersicht und Rechnungsdownload bereitzustellen. Das Portal soll als Weblösung starten, Mobile Nutzung ist geplant, aber nicht im MVP. Für Integration, Sicherheit und Hosting bestehen strikte Anforderungen und Restriktionen.

## Wichtige Komponenten

- **Kundenportal Frontend:** Webanwendung mit Login, Rollen- und Berechtigungssteuerung, Anzeige von Angeboten, Bestellungen und Rechnungen.
- **API Layer:** Verantwortlich für die sichere Anbindung externer Systeme wie SAP und das zentrale API Gateway. Umsetzung mit OAuth (bevorzugt) für Authentifizierung.
- **SAP Integration:** Lesender Zugriff auf Produkt-, Preis- und Rabattdaten aus SAP. Schreibzugriffe (z.B. für Bestellungen) sind für spätere Phasen vorgesehen.
- **Authentifizierungs- und Identity Management:** Unterstützung von E-Mail/Passwort-Login und optional SSO über Azure AD oder Google. Rollenmanagement mit mind. Admin, Kunde, Sales, Manager und Support.
- **Audit- und Logging-Komponenten:** Protokollierung sicherheitsrelevanter Aktionen zur Einhaltung von DSGVO und Revisionssicherheit.
- **Backup und Disaster Recovery:** Sicherstellung von Datenverfügbarkeit und Ausfallsicherheit gemäß Compliance-Anforderungen.
- **Hosting-Service:** Mandantenfähige, DSGVO-konforme EU-basierte Managed Cloud Infrastruktur, keine neue DB-Installation.

## Schnittstellen und Integrationspunkte

- **SAP System:** Wesentliche Datenquelle für Produktinformationen, Preise, Rabattlogik und Kundenstammdaten.
- **API Gateway (zentrales Infrastruktur-Component):** Vorgeschriebene Integrationsebene, allerdings mit Verzögerungsrisiko durch Warteliste.
- **Identity Provider (optional):** Externe SSO-Anbieter für vereinfachte Authentifizierung.
- **Support-Backend (nicht im MVP):** Künftige Integration für Ticketsystem und Kundenanfragen.

## Daten- und Sicherheitsaspekte

- **DSGVO-Konformität ist zentral:** Implementierung von Double-Opt-In, Löschkonzepten, Datenschutz bei Logging, Audit und Backup.
- **Verschlüsselung:** TLS für Datenübertragung, Ende-zu-Ende-Verschlüsselung wird als unrealistisch betrachtet.
- **Rollen- und Berechtigungsmodell:** Feingranulare Steuerung, insbesondere Einschränkungen für Support im Umgang mit sensitiven Daten.
- **Auditierbarkeit:** Nachvollziehbarkeit von Datenänderungen und Zugriffen ist Pflicht, aber Balance zwischen Umfang und Aufwand notwendig.
- **Backup und Disaster Recovery:** Muss den Anforderungen der Datenresidenz in der EU genügen.

## Offene Architekturentscheidungen

- Mobile Umsetzung: Native App vs. responsive Web-Anwendung bleibt unentschieden.
- API Gateway Nutzung und alternative Strategien, wenn Verfügbarkeit nicht rechtzeitig gegeben ist.
- Integration und Auswahl von SSO-Anbietern (Azure AD, Google).
- Supportprozess: Kein Ticketsystem im MVP, dennoch Bedarf an Datenschutz-konformer Bearbeitung.
- Rabattfreigabeprozess mit Workflows für Freigaben über Schwellenwerte ist noch unvollständig.
- Pilotkunden und deren Einfluss auf Datenschutz, Internationalisierung und Mehrwährung.
- Testdatenstrategie und Umgang mit personenbezogenen Daten in Entwicklung und Testumgebungen.
- Umgang mit Preis- und Rabattschwankungen bei SAP-Daten und möglichen Caching-Mechanismen.

## Zusammenfassung
Die Architektur des Kundenportals steht vor signifikanten Herausforderungen, insbesondere durch enge Zeitpläne, komplexe Compliance-Anforderungen und noch offene Entscheidungen bei Integration und Sicherheit. Die vorläufige Architektur fokussiert sich auf ein Webbasiertes MVP mit klarer Trennung von Komponenten, strikter DSGVO-Konformität und Nutzung bewährter Managed Services. Offene Fragen und Risiken sind dokumentiert und müssen in weiteren Projektphasen gelöst werden.

---

*Dieses Dokument basiert auf der Analyse des Stakeholder-Transkripts und den abgeleiteten Requirements und Risiken. Es bildet eine Grundlage für die weitere Architekturplanung und Entscheidungsfindung.*