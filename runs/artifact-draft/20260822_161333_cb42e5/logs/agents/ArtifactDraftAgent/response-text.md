# Response Text — ArtifactDraftAgent

## Model Round 1

- No tool calls (standalone text response)
- Text length: 4602 chars

> # Architektur-Landkarte (C4)
> 
> > Version: 5 · Stand: 2026-08-22 16:13 UTC
> > Freigabe: Autor (Steward-Chat) · Entwurf: Steward aus der Projektwahrheit (Core)
> 
> # Level 1 — Kontext
> 
> _Kaesten und Luecken sind belegt; Kanten sind Deutung des Zeichners (vom Autor freigegeben)._
> 
> ```mermaid
> flowchart TB
>   admin[/"Admin"/]
>   user[/"Betreuer"/]
>   relative[/"Angehörige"/]
>   resident[/"Bewohner"/]
>   records["Akten"]
>   fcm["FCM"]
> 
>   subgraph app["Kommunikations-App"]
>     core["App"]
>   end
> 
>   admin -->|verwaltet| core
>   user -->|nutzt| core
>   relative -->|nutzt| core
>   resident -->|nutzt| core
> 
>   core -->|berücksichtigt| records
>   core -->|sendet Push| fcm
> 
>   classDef person fill:#08427b,color:#fff
>   classDef container fill:#438dd5,color:#fff
>   classDef ext fill:#999,color:#fff
>   classDef gap fill:#f5a623,color:#000,stroke-dasharray: 5 5
> 
>   class admin,user,relative,resident person
>   class core container
>   class records,fcm ext
> ```
> 
> # Level 2 — Container, NUR BELEGT
> 
> _Kaesten und Luecken sind belegt; Kanten sind Deutung des Zeichners (vom Autor freigegeben)._
> 
> ```mermaid
> flowchart TB
>   admin[/"Admin"/]
>   user[/"Betreuer"/]
>   relative[/"Angehörige"/]
>   resident[/"Bewohner"/]
> 
>   subgraph app["Kommunikations-App"]
>     mobileApp["Mobile App"]
>     adminScreen["Admin-Screen"]
>     auditLog["Audit-Log"]
>     serverNotification["Benachr.-Rahmen"]
>     searchFrame["Suchrahmen"]
>     firestore["Firestore"]
>     pushService["Push-Dienst"]
>     tenantRules["Mandantenregeln"]
>     q1["? Auth-Backend"]
>     q3["? Medienspeicher (DEC-011)"]
>     q4["? Lokal-Cache (DEC-003)"]
>     q5["? Einladungsrahmen (DEC-005/DEC-007)"]
>   end
> 
>   admin -->|nutzt| mobileApp
>   user -->|nutzt| mobileApp
>   relative -->|nutzt| mobileApp
>   resident -->|nutzt| mobileApp
> 
>   mobileApp -->|öffnet| adminScreen
>   mobileApp -->|liest schreibt| firestore
>   mobileApp -->|empfängt Push| pushService
>   mobileApp -->|meldet an| q1
>   mobileApp -->|nutzt Cache| q4
>   mobileApp -->|lädt Medien| q3
>   mobileApp -->|sucht| searchFrame
>   mobileApp -->|protokolliert| auditLog
>   mobileApp -->|prüft Grenze| tenantRules
> 
>   searchFrame -->|nutzt Daten| firestore
>   searchFrame -->|achtet Grenze| tenantRules
>   auditLog -->|achtet Grenze| tenantRules
>   serverNotification -->|plant Push| pushService
>   serverNotification -->|nutzt Status| firestore
>   firestore -->|erzwingt Regeln| tenantRules
> 
>   classDef person fill:#08427b,color:#fff
>   classDef container fill:#438dd5,color:#fff
>   classDef ext fill:#999,color:#fff
>   classDef gap fill:#f5a623,color:#000,stroke-dasharray: 5 5
> 
>   class admin,user,relative,resident person
>   class mobileApp,adminScreen,auditLog,serverNotification,searchFrame,firestore,pushService,tenantRules container
>   class q1,q3,q4,q5 gap
> ```
> 
> | Kasten | Technologie/Detail | Beleg |
> |---|---|---|
> | Mobile App | Flutter / Dart; plattformübergreifend für iPhone und Android | ARCH-01, ARCH-03, ARCH-36, ARCH-37 |
> | Admin-Screen | Separater Bildschirm für Account- und Rechteverwaltung, nur für Admins erwogen | ARCH-13 |
> | Audit-Log | Serverseitig, append-only; Einsicht nur für Admins | ARCH-47, ADR 0002-unveraenderlicher-audit-log-rahmen-fuer-einsichtnahmen.md |
> | Benachr.-Rahmen | Serverseitige zentrale Planung und Rücknahme von Besuchserinnerungen | ARCH-48, ADR 0003-serverseitiger-benachrichtigungsrahmen-fuer-besuchserinnerun.md |
> | Suchrahmen | Serverseitige Suche mit Einrichtungsgrenze für bewohnerprofilübergreifende Stichwortsuche | ARCH-11, ADR 0004-einrichtungsgebundener-serverseitiger-suchrahmen.md |
> | Firestore | Firebase Firestore als vorläufig vorgesehene Datenbanklösung | ARCH-39 |
> | Push-Dienst | Firebase Cloud Messaging für Push-Benachrichtigungen | ARCH-46, ADR 0001-firebase-cloud-messaging-fuer-push-benachrichtigungen.md |
> | Mandantenregeln | Serverseitige Einrichtungsgrenze in Firestore über Einrichtungs-ID je Collection und Firestore Security Rules | ARCH-50, ADR 0006-serverseitige-einrichtungsgrenze-in-firestore.md |
> | ? Auth-Backend | Authentifizierung / Account-Backend ungeklärt; keine Selbstregistrierung, interne Accounts; mögliche Einladungscodes für Angehörige weiter offen | ARCH-07, ARCH-08, ARCH-12, DEC-005, DEC-007 |
> | ? Medienspeicher (DEC-011) | Medienspeicherung für Bilder und Videos ungeklärt | ARCH-19, ARCH-20, ARCH-22, ARCH-23, DEC-011 |
> | ? Lokal-Cache (DEC-003) | Lokaler Gerätespeicher / Cache ungeklärt; DEC-003 widerspricht dem bisherigen Stand | ARCH-40, DEC-003 |
> | ? Einladungsrahmen (DEC-005/DEC-007) | Selbstregistrierung für Angehörige per Einladungscode sowie Missbrauchsbegrenzung, Ablauf, Mehrfachnutzung und Sperrung ungeklärt | DEC-005, DEC-007, REQ-84 |

---

