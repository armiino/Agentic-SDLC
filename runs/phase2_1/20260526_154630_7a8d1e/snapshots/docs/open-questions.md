## Offene fachliche Fragen
- **Zieldefinition Klarheit**: Welche konkreten Ziele stehen über den MVP hinaus (z.B. Push‑Notifications, Analytics) und wann sollen diese umgesetzt werden? (Quelle: Transkript, Konflikt zwischen Anna und Clara)
- **KPI‑Messung**: Wie genau soll die Conversion Rate und die "Zeit bis Angebot" technisch erfasst und gespeichert werden? (Quelle: Transkript, Anna)
- **Rollen‑ und Berechtigungskonzept**: Welche genauen Rollen (Admin, User, Manager, Support) benötigen welche Berechtigungen und wie detailliert muss das Konzept für das MVP sein? (Quelle: Transkript, Clara)
- **Lösch‑Workflow**: Wie soll das Verfahren für Datenlöschungen (Kundendaten auf Anfrage) konkret aussehen und welche Fristen gelten? (Quelle: Transkript, Clara)
- **SAP‑Datenqualität**: Welche Maßnahmen sind nötig, um die unvollständigen Stammdaten in SAP zu ergänzen bzw. zu konsolidieren? (Quelle: Transkript, Ben)

## Offene technische Fragen
- **Auth‑Mechanismus**: Wird OAuth‑2.0 im MVP implementiert oder reicht ein einfaches E‑Mail/Passwort‑Login aus? (Quelle: Diskussion zu SSO, Ben)
- **Identity Provider**: Welcher IdP (Azure AD, Google oder beide) wird letztlich integriert, und wie beeinflusst das den Zeitplan? (Quelle: Anna, Ben)
- **API‑Layer Design**: Welche Minimal‑API‑Endpoints sind für das MVP zwingend erforderlich und welche können später folgen? (Quelle: Ben)
- **Managed DB‑Auswahl**: Welcher Managed Service (z.B. Azure PostgreSQL, AWS RDS) wird verwendet, und welche Kosten‑/Skalierbarkeits‑Aspekte sind zu beachten? (Quelle: Ben, Anna)
- **Backup & Disaster Recovery**: Welches RPO/RTO wird für das MVP definiert und wie wird das Testing der Wiederherstellung durchgeführt? (Quelle: Clara, Ben)
- **Performance‑Testing**: Wie wird das System für die breite Nutzerzahl (200‑20.000) getestet, insbesondere bezüglich Antwortzeit <2 s? (Quelle: Ben)
- **Logging & Audit‑Trail**: Welcher konkrete Logging‑Service wird verwendet (z.B. Cloud‑Log‑Service) und wie wird die Unveränderlichkeit sichergestellt? (Quelle: Clara)

## Widersprüche, die geklärt werden müssen
- **MVP‑Zeitplan vs. Security Review**: Wie kann das notwendige Security Review innerhalb der 8‑Wochen‑Frist durchgeführt werden? (Quelle: Anna vs. Clara)
- **Budget vs. Skalierbarkeit**: Wie wird die Anforderung "günstig" mit der Notwendigkeit einer skalierbaren Infrastruktur vereinbart? (Quelle: Ben, Anna)
- **Mobile‑First vs. Web‑First**: Welche Strategie wird für die mobile Unterstützung priorisiert? (Quelle: Anna, Ben)
- **Feature‑Umfang**: Sind Push‑Notifications und Tracking Teil des MVP oder erst nachfolgend? (Quelle: Anna, Clara)

## Fehlende Informationen
- **Projektbudget**: Konkrete Budgetobergrenze für Managed Services und ggf. externe Architekten.
- **Verfügbare Ressourcen**: Gibt es internen Architekten oder muss externes Fachpersonal eingeplant werden?
- **Genaues Hosting‑Requirement**: Definition "EU‑only" vs. "DSGVO‑konform" – welche Regionen genau sind zulässig?
- **Stakeholder‑Entscheidungsfindung**: Wer ist final befugt, Entscheidungen zu Auth‑Mechanismus und IdP zu treffen?

## Mögliche Ansprechpartner / Rollen
- **Product Owner (Anna)** – Entscheidungsfindung zu Kernfeatures, Mobile‑Strategie, MVP‑Umfang.
- **Technical Lead (Ben)** – Technische Architektur‑Entscheidungen, API‑Design, Infrastruktur‑Auswahl.
- **Compliance / Datenschutz (Clara)** – DSGVO‑Anforderungen, Logging, Lösch‑Workflow, Security Review.
- **IT‑Operations / Infrastruktur-Team** – Auswahl und Verwaltung von Managed Services, Backup/DR.
- **SAP‑Entwickler / Fachbereich** – Sicherstellung der Datenqualität und Integration.
