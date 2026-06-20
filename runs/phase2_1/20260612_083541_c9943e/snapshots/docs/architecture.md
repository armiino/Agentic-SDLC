# Architekturüberblick für das Kundenportal-MVP

## 1. Systemkontext
Das System besteht im Kern aus einem Kundenportal, das hauptsächlich webbasiert ist und Kunden ermöglicht, Angebote zu erhalten, Rechnungen einzusehen und Supportkontakte zu pflegen. 

Das Portal ist integriert mit einem SAP-System, das Stammdaten, Preisinformationen und Rabattlogiken bereitstellt. Identitäts- und Zugriffsmanagement ist teils über zentrale Identity Provider (z.B. Azure AD, Google) geplant, aber noch nicht final entschieden.

Die Infrastruktur basiert auf Managed Cloud Services mit Hosting ausschließlich in der EU, um DSGVO-Konformität zu gewährleisten.

Interne Nutzerrollen umfassen u.a. Admin, Sales, Manager und Support, mit unterschiedlichen Berechtigungen.


## 2. Wichtige Komponenten
- **Frontend:** Webportal basierend auf responsivem Design; mobile native App unklar und nicht Teil des MVP.
- **Backend & API Layer:** Bietet Geschäftslogik für Angebotserstellung, Freigabeprozesse, Rechnungseinblick sowie Schnittstellen für Frontend und SAP Integration.
- **Authentifizierungs- und Autorisierungskomponente:** Unterstützt E-Mail/Passwort-Login mit Double-Opt-In und optional SSO (OAuth oder andere Technologien noch offen).
- **SAP-Integrationsschnittstelle:** Realisiert den Datenzugriff auf Stammdaten, Preise und Rabattlogik; umfasst auch die Behandlung von Offline-Szenarien und Datenvalidierung.
- **Datenhaltung und Audit-Logging:** Nutzt Managed Services für Speicherung und Backup; trennt technisches Logging von personenbezogenen Daten; unterstützt Audit-Trails für Compliance.
- **Backup und Disaster Recovery:** Umsetzung eines Backupkonzepts entsprechend den Compliance-Anforderungen.
- **Support-Kontaktformular:** Einfaches Kontaktformular ohne Ticketsystem im MVP, als bewusste funktionale Einschränkung.


## 3. Schnittstellen und Integrationspunkte
- **Frontend zu Backend:** RESTful API mit Authentifizierung und Autorisierung.
- **Backend zu SAP:** API-Schnittstelle oder Middleware mit Anbindung an SAP-System.
- **Identity Provider Integration:** OAuth-basierte Anbindung an externe Identity Provider (Azure AD, Google) geplant.
- **API Gateway (reverse proxy):** Zentrale Steuerung und Absicherung der APIs, aktuell mit Warteliste als Risiko für MVP-Zeitplan.
- **Hosting-Services:** Infrastruktur und Managed Cloud Services innerhalb der EU für Datenresidenz.


## 4. Daten- und Sicherheitsaspekte
- **DSGVO Compliance:** Umsetzung von Double-Opt-In, Löschkonzepten und Datenminimierung, um Datenschutz sicherzustellen.
- **Rollen- und Berechtigungskonzept:** Granulare Zugriffskontrolle für Angebotserstellung, Freigabeprozesse und Rechnungszugriff.
- **Audit Trails und Logging:** Getrennte Protokollierung von personen- und technischen Daten zur Sicherheitsüberwachung und Nachvollziehbarkeit.
- **TLS-Verschlüsselung:** Sicherstellung der Datenübertragungssicherheit.
- **EU-only Hosting:** Verpflichtung zur Einhaltung der europäischen Datenschutzvorgaben.
- **Security Review:** Planung und Durchführung, jedoch kritisch aus Zeitgründen.


## 5. Offene Architekturentscheidungen
- **SSO-Implementierung:** Konkrete Wahl der Technologie (OAuth oder API Keys) und Integration sind noch offen.
- **API Gateway Verfügbarkeit:** Wartelistenproblem und potentielle Ausweichszenarien.
- **SAP-Integration:** Umgang mit Nichtverfügbarkeit, Cache oder Fallback-Mechanismen.
- **Mobile Strategie:** Ob native App oder reines Responsive Web im Zeitplan realisierbar ist.
- **Supportprozess:** Weitere Entwicklung der Supportfunktionalität über den MVP hinaus.
- **Freigabeprozesse:** Detaillierung der Rollen, Genehmigungslogik und workflows.
- **Backup und Disaster Recovery:** Präzise operationalisierte Konzepte und Tools.
- **Wahl der Managed Services:** Konkrete Anbieter und deren DSGVO-Konformität.


## Zusammenfassung
Diese Architektur stellt eine pragmatische Balance zwischen den fachlichen Anforderungen, Compliance-Vorgaben und technischen Beschränkungen dar. Die MVP-Fokusierung unterstützt eine schnelle Bereitstellung, setzt aber bewusste Grenzen (z.B. kein Ticketsystem, kein Native Mobile App), um wichtige Risiken zu reduzieren. Offene Entscheidungen erfordern weitere Abstimmung und Exploration in der Folgephase.

---

*Dieses Dokument basiert auf den Stakeholderdiskussionen aus T9999_chaos.txt, dem kontextuellen Verständnis und den abgeleiteten Anforderungen und Risiken.*
