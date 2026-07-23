## Functional Requirements

1. Das Kundenportal muss einen Login mit Double-Opt-In-Verfahren unterstützen. Single Sign-On (SSO) ist optional für das MVP.
2. Das Kundenportal ermöglicht Angebotserstellung basierend auf SAP-Lesedaten ohne Berücksichtigung von Sonderrabatten im MVP.
3. Nutzer können Rechnungen im Portal herunterladen.
4. Rollenmanagement umfasst mindestens die Rollen Admin, Sales und Kunde mit entsprechenden Berechtigungen.
5. Das System implementiert Audit- und Logging-Funktionalitäten, die DSGVO-konform und minimal im MVP ausgelegt sind.
6. Das Kundenportal ist webbasiert, eine mobile Unterstützung ist für zukünftige Releases vorgesehen, aber nicht Teil des MVP.
7. Backup- und Managed Hosting erfolgen ausschließlich in der EU.
8. SAP-Anbindung erfolgt lesend, zur Verfügungstellung von Preis- und Rabattlogik gemäß Anforderungen.

## Non-functional Requirements

1. Das MVP muss innerhalb eines Zeitrahmens von 8 Wochen geliefert werden.
2. Das System muss DSGVO-konform sein, insbesondere im Bereich Datenschutz, Double-Opt-In und Audit-Trails.
3. Hosting und Backup müssen EU-only Managed Services nutzen.
4. Die Performance und Skalierbarkeit sind im MVP sekundär, aber Anforderungen dafür sollen dokumentiert und in zukünftigen Iterationen berücksichtigt werden.
5. Security Reviews müssen durchgeführt werden, Umfang und Dauer sind noch zu klären.
6. Das System unterstützt Mehrwährung und Mehrsprachigkeit vorrangig für die DACH-Region im MVP; spätere Erweiterungen sind vorgesehen.

## Constraints/Compliance

1. Das Kundenportal muss die DSGVO-Vorgaben einhalten.
2. SAP-Anbindung ist eine kritische Abhängigkeit und nur lesend im MVP möglich.
3. Das Rollen- und Berechtigungskonzept ist im MVP minimalistisch ausgelegt, komplexere Freigabeprozesse sind ausgenommen.
4. Budgetrestriktionen limitieren die Nutzung von Managed Services und API-Gateway.
5. Die Supportfunktionalitäten im MVP sind reduziert und umfassen möglicherweise nur ein Kontaktformular ohne Persistenz.
6. EU-Hosting und Backup sind zwingend vorgeschrieben.

## Assumptions and Open Points

1. Der finale Pilotkunde ist noch nicht bestimmt, weshalb Anforderungen an Währungen, Sprachen und Datenschutz sich noch ändern können.
2. Die genaue Ausgestaltung der API-Security (z.B. Nutzung von OAuth) ist noch offen.
3. Der Supportprozess und das Handling von Supportdaten sind noch nicht entschieden.
4. Umfang und Details des Backup- und Monitoring-Konzepts müssen noch definiert werden.
5. Der Umgang mit Preisgültigkeit und Cache bei SAP-Anbindung ist unklar.
6. Es ist unklar, ob und wie KPI-Messungen bzw. Analytics ins MVP aufgenommen werden.
7. Dokumentationsaufwand und verfügbare Ressourcen können die Umsetzung beeinflussen.

## Traceability

- Die Anforderungen stammen aus dem MAF-Shared-State, basierend auf dem Stakeholder-Transkript T9999_chaos.txt.
- Diskussionen und Konfliktpunkte im Stakeholder-Transkript wurden berücksichtigt und widerspiegeln Spannungsfelder in Scope, Zeit und Compliance.
- Unsicherheiten im Projektkontext sind als offene Punkte dokumentiert und fließen in Annahmen ein.
