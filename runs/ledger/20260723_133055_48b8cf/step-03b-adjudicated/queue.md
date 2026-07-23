# Adjudikations-Queue (lesbar) — fülle die Aktionen in der .queue.json aus

Aktionen: `accept_gap | promote_to_claim | attach_evidence | merge_existing | mark_covered_by | reject | apply_repair | defer`
(promote_to_claim = unit als eigener Claim; attach_evidence/merge_existing/mark_covered_by brauchen `referenceTarget` (attach: Fallback claimId); apply_repair nutzt alle systemSuggestions.)

## RR::about-me-unveraendert  [review_required_claim / normal]
- proposition: Die About-Me-Seite soll in ihrer aktuellen Form nicht verändert werden.
- systemSuggestions: facet_repair status: decided->open | facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Pflegekraft: Die About-Me-Seite kommt im Team wirklich gut an, genau so hatten wir uns den ersten Eindruck pro Bewohner vorgestellt. Daran bitte nichts ändern.
- reason: verdict=partial; Die Aussage "Daran bitte nichts ändern" stützt den Kern, aber Entscheidung und Zeithorizont werden so nicht ausdrücklich festgelegt.
- ACTION: ____   REASON: ____

## RR::no-go-bearbeiten-loeschen  [review_required_claim / normal]
- proposition: Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können.
- systemSuggestion: facet_repair status: open->required
- evidence: Leitung: Und Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und auch wieder gelöscht werden können. Im Moment wäre ja alles fest, das geht im Alltag nicht.
- reason: verdict=partial; Die Funktion ist klar gefordert und direkt von der Leitung als Muss formuliert; nur der Status ist zu schwach angesetzt.
- ACTION: ____   REASON: ____

## RR::no-go-vorlagenliste-optional  [review_required_claim / normal]
- proposition: Für die Anlage von No-Gos kann optional eine Vorlagen-Liste mit häufigen No-Gos angeboten werden; daraus gilt jedoch nichts automatisch global.
- systemSuggestion: facet_repair modality: optional->must_consider
- evidence: Leitung: Was wir aber machen können: eine Vorlagen-Liste mit häufigen No-Gos, aus der man beim Anlegen auswählen kann. Global gilt davon nichts automatisch. | Pflegekraft: Das ist ein guter Kompromiss, damit kann ich leben.
- reason: verdict=partial; Die Vorlagen-Liste mit nicht-globaler Wirkung ist inhaltlich gedeckt, aber die Facette modality ist als "optional" zu schwach bzw. unpassend für den beschlossenen Kompromiss.
- ACTION: ____   REASON: ____

## RR::uebergabe-notizen-schichtworkflow  [review_required_claim / normal]
- proposition: Es wird pro Schicht eine Übergabe-Notiz benötigt, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird.
- systemSuggestion: facet_repair status: open->decided
- evidence: Leitung: Wir brauchen pro Schicht eine Übergabe-Notiz... | Leitung: ... die die nächste Schicht beim Öffnen der App prominent angezeigt bekommt.
- reason: verdict=partial; Die Anforderung ist klar durch das Transcript gedeckt; nur der Status ist zu schwach als offen markiert.
- ACTION: ____   REASON: ____

## RR::uebergabe-notizen-bewohnerverlinkung  [review_required_claim / normal]
- proposition: Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein.
- systemSuggestion: facet_repair status: open->decided
- evidence: Pflegekraft: Und diese Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein – wenn ich schreibe, dass bei Herrn M. heute etwas vorgefallen ist, will ich direkt auf sein Profil verweisen können.
- reason: verdict=partial; Die Aussage ist inhaltlich gedeckt, aber der Status ist im Eintrag unnötig offen.
- ACTION: ____   REASON: ____

## RR::uebergabe-notizen-archivierung-und-auffindbarkeit  [review_required_claim / normal]
- proposition: Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden, müssen im Archiv aber weiterhin auffindbar bleiben.
- systemSuggestions: facet_repair status: open->decided | facet_repair modality: must->desired
- evidence: Leitung: Die Notizen sollen nach sieben Tagen automatisch archiviert werden, damit die Ansicht nicht zumüllt. | Leitung: Archivierte Notizen müssen aber auffindbar bleiben.
- reason: verdict=partial; Der Kern ist gedeckt, aber der Eintrag bündelt zwei unterschiedlich starke Formulierungen und markiert den Gesamtpunkt zu entschieden.
- ACTION: ____   REASON: ____

## RR::firestore-fixiert  [review_required_claim / normal]
- proposition: Firebase Firestore ist als Persistenztechnologie für die Lösung festgelegt; Alternativen werden nicht weiter diskutiert.
- systemSuggestion: facet_repair modality: must->must_note
- evidence: Entwickler: Wir hatten Firebase Firestore ja nur vorläufig gesetzt. Nach den Tests der letzten Wochen sage ich – wir bleiben endgültig bei Firestore, die Diskussion um Alternativen machen wir nicht mehr auf. | Leitung: Gut, dann ist das jetzt fix.
- reason: verdict=partial; Die Festlegung auf Firestore und das Ende der Alternativendiskussion sind klar gedeckt; die Modalität ist jedoch etwas zu stark.
- ACTION: ____   REASON: ____

## US::AU-0002  [unit_signal / unit]
- proposition: Es ist unklar, ob aus dem Hinweis auf die anstehende Übergabe eine eigenständige fachliche Anforderung zur Unterstützung schneller Übergabeprozesse abgeleitet werden soll.
- systemSuggestion: compare_classification needs_human
- reason: Die Aussage betrifft vor allem Gesprächsorganisation und Zeitdruck vor der Übergabe. Ein möglicher Bezug zu Übergabe-Notizen ist nur indirekt und ohne klaren fachlichen Claim nicht belastbar.
- ACTION: ____   REASON: ____

## US::AU-0026  [unit_signal / unit]
- proposition: Unklar, welcher zuvor genannte Punkt aus Sicht der Angehörigen besonders wichtig ist und ob daraus ein eigener Claim folgt.
- systemSuggestion: compare_classification needs_human
- reason: Die Aussage bekräftigt nur die Wichtigkeit eines zuvor besprochenen Punkts, nennt aber selbst keinen konkreten Gegenstand. Ohne eindeutigen Bezug kann sie keinem bestehenden Kandidaten sicher als Evidence zugeordnet werden.
- ACTION: ____   REASON: ____

## US::AU-0035  [unit_signal / unit]
- proposition: Gegebenenfalls ist zu klären, ob Aufgabenverfolgung oder Terminplanung als eigene organisatorische Anforderungen an das Projekt festgehalten werden sollen.
- systemSuggestion: compare_classification needs_human
- reason: Die Aussage enthält Moderations- und Terminorganisationshinweise (Zusammenfassung, Aufgabenverteilung, nächster Termin), aber keine klar formulierte Produktanforderung. Ein Bezug zu bestehenden Kandidaten ist nicht konkret genug.
- ACTION: ____   REASON: ____

