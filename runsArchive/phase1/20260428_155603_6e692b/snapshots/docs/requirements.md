# Anforderungen

## Funktional Requirements
- Kundendatenmanagement: Kunden sollen ihre Daten verwalten können.
- Portale: Ein Webbasierter Kundengateway ist erforderlich, um Angebote zu erstellen und Rechnungen herunterzuladen. Eine mobile App ist optional.
- Rollenmodell: Es gibt mindestens drei Rollen (Administrator, Normaler Benutzer, Support).
- Anbieten von Angeboten und Bestellungen.
- SAP-Integration für Produktinformationen und Preislogik.
- Login mit E-Mail und Passwort oder Single Sign-On (SSO) Integration.
- Verschlüsselung: TLS-Basierte Datenverschlüsselung.
- Logging und Audit Trails.

## Non-functional Requirements
- Skalierbarkeit: Das System muss flexibel sein, um bis zu 20.000 Benutzer zu verarbeiten.
- Performance: Schnelles Antwortzeiten für Anfragen.
- Kompatibilität mit Standards wie OAuth und DSGVO.

## Constraints/Compliance
- DSGVO-Konformität: Kundendaten müssen in den EU gehostet werden, Datensicherheit ist eine Priorität.
- Kein neuer DB Server: Es wird kein neuer Datenbankserver bereitgestellt, Managed Services sind die Lösung für Skalierbarkeit.

## Traceability
- Kundenportal-Funktionalität (Sektor 1 bis 3 im Transkript).
- Rollenmodell und Berechtigungen (Sektor 4 im Transkript).
- Datensicherheit und Verschlüsselung (Sektor 5 im Transkript).