# Grober Architekturüberblick für das Kundenportal MVP

## 1. Systemkontext
Das Kundenportal MVP ist als Web-basierte Plattform konzipiert, die primär den Prozess der schnelleren Angebotserstellung, Rechnungsanzeige und -download für Kunden unterstützt. Es wird eng mit dem SAP-System integriert, um Produktdaten, Preise und Rabattlogik zu beziehen. Nutzer aus dem Vertrieb, Support und der Kundenverwaltung greifen über Rollen mit differenzierten Berechtigungen auf das Portal zu. Das Portal ist ein Teil eines größeren IT-Ökosystems mit bestehenden Operation- und Security-Services.

## 2. Wichtige Komponenten
- **Frontend:** Web-Client mit Login (E-Mail/Passwort), optional später mit SSO. Mobile native App ist nicht Teil des MVP.
- **Backend/API Layer:** Serviert die geschäftliche Logik, Schnittstellen zu SAP, Nutzerverwaltung und Berechtigungskonzept, Angebot- und Rechnungsmanagement. OAuth wird als bevorzugte API-Sicherheitsmaßnahme angestrebt.
- **SAP-Integration:** Lesender Zugriff auf Produkt- und Preisdaten, Rabattlogik, mögliche spätere Erweiterung um Schreibzugriffe.
- **Rollen- und Berechtigungssystem:** Definition und Durchsetzung von Zugriffrechten für Admin, Sales, Manager, Support, mit Fokus auf strikte Trennung privilegierter Funktionen.
- **Audit- und Logging-Komponente:** Revisionssichere Protokollierung von Änderungen, Zugriffen und Freigaben, DSGVO-konform, keine Speicherung personenbezogener Daten in technischen Logs.
- **Backup und Disaster Recovery:** Nutzung von Managed Services mit EU-DSGVO-konformem Hosting und Backups. Keine neuen Datenbank-Server im MVP.
- **Support-Komponente:** Kontaktformular im MVP ohne Ticketsystem, mit klar dokumentierter Einschränkung und späterer Erweiterungsplanung.

## 3. Schnittstellen und Integrationspunkte
- **SAP-System:** Kritische Datenquelle für Produktinformationen, Preise und Rabattlogik. Die Stabilität und Verfügbarkeit des SAP-Systems ist eine der größten technischen Risiken.
- **API Gateway:** Vorgeschriebene Nutzung für neue externe Portale, jedoch mit 6 Wochen Warteliste, was zu Übergangslösungen führt.
- **Authentifizierungssysteme:** E-Mail/Passwort-Login im MVP; SSO-Lösungen (Azure AD, Google) als optionale Erweiterung.
- **Backup- und Monitoring-Infrastruktur:** Integriert über Managed Services und bestehende IT-Policies.

## 4. Daten- und Sicherheitsaspekte
- **DSGVO-Konformität:** Double-Opt-In für Nutzerregistrierung, Löschkonzepte, Audit-Logs, Zugriffskontrolle und Datenminimierung.
- **Hosting:** Datenhaltung strikt innerhalb der EU oder DSGVO-konform, mit Nachweis der Datenresidenz.
- **Verschlüsselung:** TLS für Datenübertragung, keine Ende-zu-Ende-Verschlüsselung aufgrund von Komplexität und Realitätscheck für MVP.
- **Audit und Logging:** Minimale notwendige Protokollierung unter Wahrung des Datenschutzes.
- **Secret Management:** Notwendigkeit eines strikten Konzepts für API-Keys, OAuth Tokens und sonstige Zugangsdaten.
- **Backup und DR:** Regelmäßige Backups und Disaster Recovery abgesichert durch Managed Services.

## 5. Offene Architekturentscheidungen und Risiken
- **API Gateway Verfügbarkeit:** Übergangslösung muss gefunden werden, da die Warteliste des zentralen Gateways nicht mit dem MVP-Zeitplan übereinstimmt.
- **Supportprozess:** Kein Ticketsystem im MVP, was Risiken der Nachvollziehbarkeit und Supportqualität birgt.
- **Rabattfreigabeprozesse:** Noch keine Implementierung, daher keine Rabattaktionen über Standardwerte im MVP.
- **Pilotkunde und Lokalisierung:** Noch nicht final definiert, besonders relevant für Hosting, Datenschutz und Mehrwährung.
- **Sicherheitsreviews:** Zeitliche und organisatorische Abstimmung erforderlich, da vollständige Reviews den MVP-Zeitplan sprengen.
- **Skalierbarkeit:** Angestrebte Nutzerzahlen sind weit gefasst (200 bis 20.000) – Planung muss diese Spannbreite berücksichtigen.
- **SAP-Verfügbarkeit:** Kritische Abhängigkeit, die Fallback-Mechanismen erfordert.

## 6. Zusammenfassung
Dieses Architekturmodell spiegelt die aktuellen Erkenntnisse, Anforderungen und Restriktionen aus Stakeholder-Diskussionen wider. Es stellt eine Balance aus Time-to-Market, Compliance und Technik dar, wobei Widersprüche und offene Punkte transparent ausgewiesen sind. Das MVP ist bewusst reduziert, um es in 8 Wochen realisieren zu können, mit klarem Fokus auf Kernfunktionen und minimalen Risiken.

---

*Dokument basierend auf den organisatorischen und technischen Erkenntnissen aus dem Stakeholder-Transkript T9999_chaos.txt, der Kontext- und Anforderungsanalyse sowie Risikoabschätzung.*