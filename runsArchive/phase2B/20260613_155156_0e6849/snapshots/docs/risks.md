# Risikoübersicht für das Kundenportal-Projekt

## Fachliche Risiken

- **Unklare und nicht final definierte Freigabeprozesse für Rabatte**  
  Die Schwellenwerte (1520 %) und Verantwortlichkeiten sind noch offen, was zu fehlerhaften oder versp2teten Freigaben f52hren kann und den Sales-Prozess behindert.  
  *Gegenma2nahme:* Fr52hzeitige Kl2rung und Dokumentation der Freigaberegeln im MVP.

- **Supportprozess ohne Ticketsystem im MVP**  
  Nur ein Kontaktformular ist vorgesehen, was zu ineffizientem Support und fehlender Nachverfolgbarkeit von Anfragen f52hrt. M56gliche Compliance-Probleme durch mangelnde Dokumentation.  
  *Gegenma2nahme:* Definition klarer Handhabung von Supportanfragen und Evaluierung eines Ticketsystems f52r sp2tere Releases.

- **Rollen- und Berechtigungskonflikte, insbesondere Support-Zugriff auf sensible Angebotsdaten**  
  Unklare Zugriffsbeschr2nkungen k46nnen Datenschutzverletzungen und Sicherheitsl5cken verursachen.  
  *Gegenma2nahme:* Pr52zise Rollendefinition und Zugriffskontrolle implementieren.

## Technische Risiken

- **Mangel an dedizierten Architekturressourcen**  
  Kein fest zugeteilter Architekt, was zu inkonsistenten Designentscheidungen und technischen Schulden f52hren kann.  
  *Gegenma2nahme:* Sicherstellung von Architektenbeteiligung zumindest bei kritischen Entscheidungen.

- **Knappes Zeitfenster (ca. 8 Wochen) f52r MVP bei gleichzeitigem Bedarf an Sicherheitsreviews, Architektur- und API-Gateway-Entscheidungen**  
  Risiko von Verz46gerungen und Qualit2tsverlust.  
  *Gegenma2nahme:* Priorisierung der MVP-Funktionalit2ten und geplante Zeitpuffer f52r Reviews.

- **Unklare SAP-Datenaktualit2t und Fallbacks bei Nichtverf56gbarkeit**  
  Fehlende Daten k46nnen die Angebotserstellung behindern und Fehler verursachen.  
  *Gegenma2nahme:* Definition robuster Fallback-Strategien und Monitoring der SAP-Schnittstellen.

- **Unentschiedene Identity Provider Auswahl (Azure AD, Google oder andere)**  
  Verz46gerungen bei Integration und Sicherheitsrisiken durch unsichere oder unvollst2ndige Implementierungen.  
  *Gegenma2nahme:* Fr52hzeitige Entscheidung und technische Evaluation.

## Compliance- und Datenschutzrisiken

- **Strikte Einhaltung der DSGVO mit Datenminimierung, L46schkonzepten und Auditierbarkeit**  
  Komplexe Anforderungen k46nnen Umsetzung verz46gern und Fehler in Datenschutz verursachen.  
  *Gegenma2nahme:* Einbindung von Datenschutz-Expertise (Clara) und fr52hzeitige Reviews.

- **Hosting ausschlie2lich in EU-Datenzentren mit m46glichem Kostendruck**  
  Einschr2nkungen k46nnten zu h46heren Kosten und Infrastrukturengp2ssen f52hren.  
  *Gegenma2nahme:* Kosten-Nutzen-Analyse und Wahl passender Managed Services.

- **Umfang des Audit- und Logging-Konzepts unklar (Balance zwischen Minimal- und Vollanforderungen)**  
  Risiko von Compliance-Verst462en oder Overengineering mit Performanceauswirkungen.  
  *Gegenma2nahme:* Klare Anforderungen definieren und pragmatische Umsetzung im MVP.

## Widerspr56che und Unsicherheiten

- **Widerspruch zwischen ambitioniertem MVP-Zeitplan und notwendigen Sicherheits- und Architektur-Reviews**  
  Kann zu Abstrichen bei Sicherheit oder Funktionalit2t f52hren.  
  *Gegenma2nahme:* Fokus auf Kernelemente und iterative Verbesserungen nach MVP.

- **Offene Frage nach Pilotkunde und regionalen Anforderungen (Schweiz vs. Deutschland)**  
  M46gliche Nachr56stung oder Anpassungen verz46gern Release.  
  *Gegenma2nahme:* Klare Zielgruppendefinition vor Finalisierung.

- **Gefahr von Overengineering bei Skalierbarkeit**  
  Ressourcenverschwendung und Verz46gerungen.  
  *Gegenma2nahme:* MVP-Prinzip streng einhalten.

---
