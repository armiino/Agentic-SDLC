# Response Text — BranchMaker-requirements

## Model Round 1

- No tool calls (standalone text response)
- Text length: 3324 chars

> # Requirements
> 
> ## Funktionale Anforderungen
> 
> - Die About-Me-Seite darf im MVP nicht verändert werden, da sie den gewünschten ersten Eindruck pro Bewohner gut vermittelt und im Team bewährt ist. [canonical-about-me-page-unchanged]
> 
> - Die No-Go-Seite muss im MVP ein rotes Stopp-Symbol als deutliches Warnsignal enthalten, um kritische Einträge sofort erkennbar zu machen. [canonical-no-go-page-stop-symbol]
> 
> - Einträge auf der No-Go-Seite müssen im MVP nachträglich bearbeitet und gelöscht werden können, um Flexibilität im Alltag zu gewährleisten. [canonical-no-go-page-edit-delete]
> 
> - No-Gos bleiben im MVP individuell pro Bewohner, es wird jedoch eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl beim Anlegen angeboten; global gelten keine automatisch. [canonical-no-go-per-resident-with-template]
> 
> - Der Sofortinfo-Bereich ist im MVP auf maximal fünf Einträge zu begrenzen, und beim Anlegen eines Bewohners ist mindestens ein Sofortinfo-Eintrag verpflichtend. [canonical-immediate-info-limit-and-mandatory]
> 
> - Bei Offline-Erfassung muss im MVP eine klare Konfliktanzeige erfolgen, wenn zwei Nutzer denselben Eintrag offline geändert haben, um stille Überschreibungen zu verhindern. [canonical-offline-conflict-indication]
> 
> - Pro Schicht wird im MVP eine Übergabe-Notiz benötigt, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird. [canonical-shift-hand-over-note]
> 
> - Übergabe-Notizen müssen im MVP bewohnerbezogen verlinkbar sein, sodass direkt auf das Profil des betreffenden Bewohners verwiesen werden kann. [canonical-shift-hand-over-note-link-to-resident]
> 
> - Übergabe-Notizen werden im MVP nach sieben Tagen automatisch archiviert, bleiben jedoch auffindbar, um die Ansicht übersichtlich zu halten. [canonical-shift-hand-over-note-archiving]
> 
> - Es wird im MVP eine Suche über alle Bewohnerprofile hinweg nach Stichworten benötigt, z.B. um No-Gos zu finden. [canonical-search-across-residents]
> 
> - Das Zugriffsprotokoll darf im MVP nur von Administratoren eingesehen werden, nicht von normalen Nutzern. [canonical-access-log-viewable-only-by-admins]
> 
> - Jede Einsichtnahme in Bewohnerdaten muss im MVP revisionssicher protokolliert werden, inklusive wer, wann welches Profil angesehen hat. [canonical-compliance-logging-access-to-resident-data]
> 
> ## Technische Anforderungen
> 
> - Firebase Firestore wird im MVP als Datenbank verwendet; alternative Lösungen werden nicht weiter diskutiert. [canonical-decision-use-firestore]
> 
> - Die App unterstützt im MVP mindestens Android 10; ältere Geräte im Haus müssen gegebenenfalls ausgetauscht werden. [canonical-requirement-min-android-version-10]
> 
> ## Offene Fragen / Kontext
> 
> - Es ist noch offen, ob Angehörige im System selbst Inhalte eintragen dürfen oder nur lesend zugreifen können; Datenschutzfragen dazu sind noch zu klären. [canonical-open-question-relatives-edit-rights]
> 
> - Die Piktogramme sollten in einem größeren Kontext weitergedacht und erweitert werden, um ihre Wirkung zu verbessern; wie dies geschehen soll, ist noch unklar. [ADJ-GAP-AU-0033]
> 
> ## Nicht-funktionale Anforderungen (wünschenswert, nicht MVP)
> 
> - Ein Dunkelmodus für den Nachtdienst und eine einstellbare Schriftgröße für ältere Kolleginnen sind als spätere Verbesserungen wünschenswert, aber nicht zwingend erforderlich im MVP. [canonical-optional-wishes-dark-mode-and-font-size]

---

