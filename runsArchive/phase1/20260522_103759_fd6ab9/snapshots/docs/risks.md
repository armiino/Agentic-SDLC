# Risiken

## Fachliche Risiken
- **Unklare Zieldefinition**: Unterschiedliche Schwerpunkte (Angebotserstellung, Kundenportal, Mobile, Push‑Notifications) führen zu Scope‑Creep und verzögern das MVP. *Unsicherheit*: Welche Funktionen sind zwingend im MVP enthalten? [„Wir haben noch keine klare Zieldefinition.“]
- **Mobile‑Strategie nicht festgelegt**: Unklar, ob native App oder responsive Web benötigt wird, erschwert die Frontend‑Planung und Ressourcenallokation. [„Web first. Mobile später. Oder andersrum?“]
- **Feature‑Überladung**: Wunsch nach SSO, OAuth, Push‑Notifications und Analytics kann das Projektbudget und den Zeitplan übersteigen. [„Komplexer heißt mehr Zeit.“]
- **Unklare Nutzerzahl**: Schätzungen von 200 bis 20 000 Nutzern beeinflussen Skalierbarkeits‑Entscheidungen stark. [„Vielleicht 200 … vielleicht 20.000.“]
- **Fehlende Rollen‑ und Berechtigungsspezifikation**: Rollen (Admin, Manager, User, Support) sind genannt, aber detaillierte Berechtigungen fehlen, was zu Fehlkonfigurationen führen kann. [„Rollen‑ und Berechtigungskonzept.“]

## Technische Risiken
- **Backend‑Readiness**: Das aktuelle Backend ist nicht API‑ready; eine API‑Layer ist zwingend notwendig, aber Zeit‑ und Ressourcenengpässe bestehen. [„Unser Backend noch nicht mal API‑ready.“]
- **Security Review vs. MVP‑Zeitplan**: Security Review (6 Wochen) überlappt stark mit dem 8‑Wochen‑MVP‑Zeitplan, sodass Compliance‑Probleme auftreten können. [„Security Review dauert 6 Wochen … sprengt die 8 Wochen.“]
- **Managed Services ohne eigene Datenbank**: Nutzung von Managed Services kann Kosten‑ und Skalierbarkeits‑Probleme erzeugen; zugleich fehlt klare Entscheidung über DB‑Typ und Backup‑Strategie. [„Kein neuer DB Server … Managed Services … Backup.“]
- **Authentication‑Komplexität**: Entscheidung zwischen SSO (Azure AD/Google), OAuth oder API‑Keys beeinflusst Implementierungsaufwand und Sicherheit. [„OAuth wäre besser. Aber komplexer.“]
- **Performance‑Skalierung**: Unterschiedliche Nutzerzahlen erfordern flexible Skalierung; Over‑Engineering soll vermieden werden, was zu Unter‑Dimensionierung führen kann. [„Overengineering und skalierbar sind ein Spannungsfeld.“]

## Compliance‑ und Datenschutzrisiken
- **DSGVO‑Konformität**: Fehlende Double‑Opt‑In‑Implementierung, unklare Löschkonzepte und Audit‑Trails können zu Verstößen führen. [„Double‑Opt‑In … Löschkonzepte … Audit Trails.“]
- **Datenhosting‑Standort**: Unklar, ob das Hosting wirklich EU‑only ist; DSGVO‑Konformität ist nicht gleichbedeutend mit EU‑Only‑Hosting. [„EU only … oder zumindest DSGVO‑konform.“]
- **Logging & Audit**: Unzureichendes Logging kann das erforderliche Audit‑Trail für Änderungen an personenbezogenen Daten verhindern. [„Logging … Auditierbarkeit ist Pflicht.“]
- **Push‑Notifications & Tracking**: Einführung von Push‑Notifications erfordert Einwilligung; fehlende Einwilligungs‑ und Tracking‑Mechanismen stellen ein rechtliches Risiko dar. [„Push heißt Einwilligung. Und Tracking ist auch ein Thema.“]

## Widersprüche und Unsicherheiten
- **Zeit vs. Security**: MVP in 8 Wochen vs. notwendigem Security Review (6 Wochen) – potenzielles Risiko, dass Security‑Kontrollen ausgelassen werden. [„Dann machen wir es ohne Security Review?“]
- **Feature‑Scope**: Diskussion, ob SSO, OAuth, Push‑Notifications und Analytics im MVP enthalten sein müssen. [„Komplexer heißt mehr Zeit.“]
- **Hosting‑Anforderungen**: Unterschied zwischen „DSGVO‑konform“ und „EU‑only“ ist nicht geklärt. [„EU only … oder zumindest DSGVO‑konform.“]
- **Rollen‑ und Löschkonzept**: Es gibt ein Bedarf, aber kein klares Konzept oder Verantwortliche. [„Wer macht das?“]

## Mögliche Auswirkungen
- **Projektverzögerung**: Scope‑Creep, fehlende Architekturentscheidungen und ausstehende Security‑Reviews können das 8‑Wochen‑Ziel verfehlen.
- **Kostenüberschreitung**: Zusätzliche Ressourcen für SSO, OAuth, Managed Services und Compliance‑Erfüllung können das Budget überschreiten.
- **Rechtliche Konsequenzen**: Nicht‑eingehaltener DSGVO‑Anforderungen (z. B. fehlende Einwilligungen, unzureichende Löschprozesse) können zu Bußgeldern und Reputationsschäden führen.
- **Qualitätsverlust**: Unterdimensionierte Performance‑ und Sicherheitsmaßnahmen könnten zu Ausfällen, Datenverlust oder Sicherheitsvorfällen führen.
- **Nutzerakzeptanz**: Fehlende mobile Optimierung oder unklare Rollen‑Berechtigungen können die Nutzerzufriedenheit und damit die Conversion Rate negativ beeinflussen.

*Alle Risiken basieren ausschließlich auf den im Transkript und Kontext genannten Aussagen.*