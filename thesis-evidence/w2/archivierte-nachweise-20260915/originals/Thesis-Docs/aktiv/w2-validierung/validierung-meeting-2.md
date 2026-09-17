# Validierungs-Blatt meeting-2-extended (30 Units · Stand AI-assisted candidate v05 (Autor-Feedback 05.09. eingearbeitet))

> Je Unit: Aussage lesen → Zitate darunter lesen → passt es? NUR Abweichungen notieren.
> Nicht Notiertes gilt nach deinem Durchgang als BESTÄTIGT. ★ = Grenzfall, dein Urteil.

---

## G-M2-001 · `requirement`

**Die bestehende About-Me-Seite soll unverändert beibehalten werden.**

*Note: v02 (Review): Typ decision→requirement — Leitfaden §3b führt bestätigten Bestand ausdrücklich als requirement; kein decision-Zusatzgehalt (keine verworfene Alternative). Explizite Bestätigung: 'Daran bitte nichts ändern'.*

> [AU-0004] Pflegekraft: Die About-Me-Seite kommt im Team wirklich gut an, genau so hatten wir uns
> den ersten Eindruck pro Bewohner vorgestellt. Daran bitte nichts ändern.

---

## G-M2-002 · `requirement`

**Die No-Go-Seite soll ein rotes Stopp-Symbol als deutliches Warnsignal enthalten.**

> [AU-0006] Pflegekraft: Bei der No-Go-Seite gibt es aber konkrete Wünsche. Die Liste mit dem Plus-
> Button ist gut, aber die Seite braucht ein rotes Stopp-Symbol als deutliches Warnsignal, damit man
> sofort sieht: Achtung, das hier ist kritisch.

---

## G-M2-003 · `requirement`

**No-Go-Einträge sollen nachträglich bearbeitet werden können.**

> [AU-0007] Leitung: Und Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und auch wieder
> gelöscht werden können. Im Moment wäre ja alles fest, das geht im Alltag nicht.

---

## G-M2-004 · `requirement`

**No-Go-Einträge sollen nachträglich gelöscht werden können.**

> [AU-0007] Leitung: Und Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und auch wieder
> gelöscht werden können. Im Moment wäre ja alles fest, das geht im Alltag nicht.

---

## G-M2-005 · `decision`

**No-Gos bleiben bewohnerbezogen; eine globale Geltung für alle Bewohner wurde ausdrücklich verworfen.**

*Note: Explizite Entscheidung inklusive verworfener Alternative.*

> [AU-0008] Angehörige: Ich fände ja weiterhin, No-Gos sollten für alle Bewohner global gelten.

> [AU-0009] Leitung: Das hatten wir letztes Mal schon diskutiert. Nein – No-Gos bleiben pro
> Bewohner, das ist entschieden. Was wir aber machen können: eine Vorlagen-Liste mit häufigen No-
> Gos, aus der man beim Anlegen auswählen kann. Global gilt davon nichts automatisch.

---

## G-M2-006 · `requirement`

**Beim Anlegen eines No-Gos soll aus einer Vorlagen-Liste häufiger No-Gos ausgewählt werden können; Einträge daraus gelten nicht automatisch global.**

*Note: Die fehlende automatische globale Geltung ist eine wesentliche Qualifikation.*

> [AU-0009] Leitung: Das hatten wir letztes Mal schon diskutiert. Nein – No-Gos bleiben pro
> Bewohner, das ist entschieden. Was wir aber machen können: eine Vorlagen-Liste mit häufigen No-
> Gos, aus der man beim Anlegen auswählen kann. Global gilt davon nichts automatisch.

---

## G-M2-007 · `requirement`

**Der Sofortinfo-Bereich soll auf maximal fünf Einträge begrenzt sein.**

*Note: Die Obergrenze von fünf Einträgen ist wesentlich.*

> [AU-0012] Pflegekraft: Genau, der Sofortinfo-Bereich sollte auf maximal fünf Einträge begrenzt
> werden, und beim Anlegen eines Bewohners sollte mindestens ein Sofortinfo-Eintrag verpflichtend
> sein, sonst steht da bei neuen Bewohnern gar nichts.

> [AU-0013] Leitung: Einverstanden, so machen wir das.

---

## G-M2-008 · `requirement`

**Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend erfasst werden.**

*Note: Mindestanzahl und Verpflichtungsgrad sind wesentlich.*

> [AU-0012] Pflegekraft: Genau, der Sofortinfo-Bereich sollte auf maximal fünf Einträge begrenzt
> werden, und beim Anlegen eines Bewohners sollte mindestens ein Sofortinfo-Eintrag verpflichtend
> sein, sonst steht da bei neuen Bewohnern gar nichts.

> [AU-0013] Leitung: Einverstanden, so machen wir das.

---

## G-M2-009 · `requirement`

**Noch nicht synchronisierte, offline erfasste Änderungen sollen für den Nutzer sichtbar gekennzeichnet werden.**

*Note: Im Gespräch als bereits zuvor besprochene Anforderung bestätigt.*

> [AU-0014] Pflegekraft: Zum Offline-Thema: Wenn ich unterwegs etwas erfasse und es noch nicht
> synchronisiert ist, will ich das ja sehen – das war schon besprochen. Zusätzlich brauchen wir aber
> eine klare Konfliktanzeige, wenn zwei Leute denselben Eintrag offline geändert haben. Da darf
> nichts stillschweigend überschrieben werden.

---

## G-M2-010 · `requirement`

**Wenn zwei Personen denselben Eintrag offline geändert haben, soll ein Konflikt klar angezeigt werden.**

> [AU-0014] Pflegekraft: Zum Offline-Thema: Wenn ich unterwegs etwas erfasse und es noch nicht
> synchronisiert ist, will ich das ja sehen – das war schon besprochen. Zusätzlich brauchen wir aber
> eine klare Konfliktanzeige, wenn zwei Leute denselben Eintrag offline geändert haben. Da darf
> nichts stillschweigend überschrieben werden.

> [AU-0015] Entwickler: Gut, dann zeigen wir Konflikte an und lassen den Nutzer entscheiden, welche
> Version gilt.

---

## G-M2-011 · `requirement`

**Bei konkurrierenden Offline-Änderungen darf keine Version stillschweigend überschrieben werden.**

> [AU-0014] Pflegekraft: Zum Offline-Thema: Wenn ich unterwegs etwas erfasse und es noch nicht
> synchronisiert ist, will ich das ja sehen – das war schon besprochen. Zusätzlich brauchen wir aber
> eine klare Konfliktanzeige, wenn zwei Leute denselben Eintrag offline geändert haben. Da darf
> nichts stillschweigend überschrieben werden.

---

## G-M2-012 · `requirement`

**Bei einem Konflikt zwischen Offline-Änderungen soll der Nutzer entscheiden, welche Version gilt.**

> [AU-0015] Entwickler: Gut, dann zeigen wir Konflikte an und lassen den Nutzer entscheiden, welche
> Version gilt.

---

## G-M2-013 · `requirement`

**Für jede Schicht soll eine Übergabe-Notiz erfasst werden können.**

> [AU-0016] Leitung: Jetzt ein neues Thema, das aus dem Team kam: die Schichtübergabe. Wir brauchen
> pro Schicht eine Übergabe-Notiz, die die nächste Schicht beim Öffnen der App prominent angezeigt
> bekommt.

---

## G-M2-014 · `requirement`

**Die Übergabe-Notiz einer Schicht soll der nächsten Schicht beim Öffnen der App prominent angezeigt werden.**

> [AU-0016] Leitung: Jetzt ein neues Thema, das aus dem Team kam: die Schichtübergabe. Wir brauchen
> pro Schicht eine Übergabe-Notiz, die die nächste Schicht beim Öffnen der App prominent angezeigt
> bekommt.

---

## G-M2-015 · `requirement`

**Übergabe-Notizen sollen mit Bewohnerprofilen verlinkt werden können.**

> [AU-0017] Pflegekraft: Und diese Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein – wenn
> ich schreibe, dass bei Herrn M. heute etwas vorgefallen ist, will ich direkt auf sein Profil
> verweisen können.

---

## G-M2-016 · `requirement`

**Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden.**

*Note: Die Frist von sieben Tagen ist wesentlich.*

> [AU-0018] Leitung: Die Notizen sollen nach sieben Tagen automatisch archiviert werden, damit die
> Ansicht nicht zumüllt. Archivierte Notizen müssen aber auffindbar bleiben.

---

## G-M2-017 · `requirement`

**Archivierte Übergabe-Notizen sollen weiterhin auffindbar bleiben.**

> [AU-0018] Leitung: Die Notizen sollen nach sieben Tagen automatisch archiviert werden, damit die
> Ansicht nicht zumüllt. Archivierte Notizen müssen aber auffindbar bleiben.

---

## G-M2-018 · `requirement`

**Die App soll eine Stichwortsuche über alle Bewohnerprofile hinweg ermöglichen.**

> [AU-0019] Pflegekraft: Was mir im Alltag außerdem fehlt: eine Suche. Ich will über alle
> Bewohnerprofile hinweg nach Stichworten suchen können, zum Beispiel nach einem Nahrungsmittel, um
> zu sehen, bei wem das ein No-Go ist.

> [AU-0020] Leitung: Gute Idee, nehmen wir auf.

---

## G-M2-019 · `requirement`

**Jede Einsichtnahme in Bewohnerdaten muss protokolliert werden, einschließlich wer wann welches Profil angesehen hat.**

*Note: Nicht verhandelbare externe Auflage; Akteur, Zeitpunkt und Profil sind wesentliche Bestandteile. v05 (Autor-Feedback 05.09.): AU-0023 entfernt (trägt die Einsichts-Beschränkung = G-M2-021).*

> [AU-0021] Leitung: Dann ein Punkt von der Trägerschaft, und der ist nicht verhandelbar: Die
> Heimaufsicht verlangt, dass jede Einsichtnahme in Bewohnerdaten protokolliert wird – wer hat wann
> welches Profil angesehen. Das ist eine Auflage, die müssen wir umsetzen, sonst dürfen wir die App
> gar nicht einsetzen.

> [AU-0022] Entwickler: Verstanden, also ein revisionssicheres Zugriffsprotokoll als Pflicht.

---

## G-M2-020 · `requirement`

**Das Zugriffsprotokoll über Einsichtnahmen in Bewohnerdaten muss revisionssicher sein.**

> [AU-0022] Entwickler: Verstanden, also ein revisionssicheres Zugriffsprotokoll als Pflicht.

---

## G-M2-021 · `requirement`

**Das Zugriffsprotokoll darf nur von Admins eingesehen werden; normale Nutzer dürfen keinen Zugriff darauf haben.**

> [AU-0023] Leitung: Genau. Und dieses Protokoll dürfen nur Admins einsehen, nicht die normalen
> Nutzer.

---

## G-M2-022 · `open_question`

**Ob Angehörige selbst Inhalte in der App eintragen dürfen oder nur lesenden Zugriff erhalten, ist noch nicht entschieden.**

*Note: Die Klärung wird ausdrücklich wegen Datenschutzfragen vertagt.*

> [AU-0024] Angehörige: Ich hätte noch ein Thema: Dürfen wir als Angehörige eigentlich auch selbst
> Inhalte eintragen, zum Beispiel auf der About-Me-Seite? Oder nur lesen?

> [AU-0025] Leitung: Das ist ehrlich gesagt noch offen. Da hängen Datenschutzfragen dran, und ich
> will das nicht heute im kleinen Kreis entscheiden. Ich nehme das mit und kläre es mit unserer
> Datenschutzbeauftragten – bis dahin bleibt das unentschieden.

---

## G-M2-023 · `requirement`

**Ein Dunkelmodus ist als nachrangiger Wunsch vorgesehen.**

*Note: Im Gespräch ausdrücklich als Wunsch mit geringerer Priorität als die übrigen Punkte eingeordnet.*

> [AU-0027] Pflegekraft: Zwei kleinere Sachen noch, eher Wünsche als Muss: Ein Dunkelmodus wäre
> angenehm für den Nachtdienst, und eine einstellbare Schriftgröße würde einigen älteren Kolleginnen
> helfen. Beides ist aber nicht kriegsentscheidend.

> [AU-0028] Leitung: Notieren wir als Wünsche, Priorität haben die anderen Punkte.

---

## G-M2-024 · `requirement`

**Eine einstellbare Schriftgröße ist als nachrangiger Wunsch vorgesehen.**

*Note: Im Gespräch ausdrücklich als Wunsch mit geringerer Priorität als die übrigen Punkte eingeordnet.*

> [AU-0027] Pflegekraft: Zwei kleinere Sachen noch, eher Wünsche als Muss: Ein Dunkelmodus wäre
> angenehm für den Nachtdienst, und eine einstellbare Schriftgröße würde einigen älteren Kolleginnen
> helfen. Beides ist aber nicht kriegsentscheidend.

> [AU-0028] Leitung: Notieren wir als Wünsche, Priorität haben die anderen Punkte.

---

## G-M2-025 · `decision`

**Firebase Firestore wird verbindlich als Datenbanklösung beibehalten; die Diskussion über Alternativen wird geschlossen.**

*Note: v05 (Autor-Feedback 05.09.): Typ architecture→decision — ausdrücklicher Abschluss-Akt ('jetzt fix', Alternativen-Diskussion geschlossen) = Entscheidungsbaum Schritt 1. Abgrenzung: die Interview-Unit G-IE-057 ('zunächst') bleibt architecture — dort war die Wahl noch vorläufig.*

> [AU-0029] Entwickler: Von meiner Seite noch zwei technische Sachen. Erstens: Wir hatten Firebase
> Firestore ja nur vorläufig gesetzt. Nach den Tests der letzten Wochen sage ich – wir bleiben
> endgültig bei Firestore, die Diskussion um Alternativen machen wir nicht mehr auf.

> [AU-0030] Leitung: Gut, dann ist das jetzt fix.

---

## G-M2-026 · `architecture`

**Die App soll Android ab Version 10 unterstützen.**

*Note: Als technische Mindestanforderung formuliert; Planungsannahme — die offene Träger-Klärung des Geräteaustauschs trägt G-M2-028.*

> [AU-0031] Entwickler: Zweitens: Als Mindestanforderung unterstützen wir Android 10 aufwärts.
> Ältere Geräte im Haus müssten dann ausgetauscht werden.

> [AU-0032] Leitung: Das klären wir mit dem Träger, aber plant erstmal so.

---

## G-M2-027 · `open_question`

**Wie Piktogramme über ihren bisherigen Einsatz hinaus berücksichtigt werden sollen, bleibt noch offen.**

*Note: Der Wunsch wird geäußert, aber ausdrücklich noch nicht konkretisiert ('ich weiß auch nicht genau, wie ich das meine'). v05 (Autor-Feedback 05.09.): Formulierung quellennäher — keine Vorab-Konkretisierung ('umfassender/weiterentwickelt') mehr.*

> [AU-0033] Pflegekraft: Ach, und das mit den Piktogrammen müsste man irgendwie größer denken, finde
> ich. Aber ich weiß auch nicht genau, wie ich das meine.

> [AU-0034] Leitung: Das lassen wir mal so stehen, vielleicht wird das beim nächsten Mal konkreter.

---

## G-M2-028 · `open_question`

**Der wegen der Android-10-Mindestanforderung erforderliche Austausch älterer Geräte ist noch mit dem Träger zu klären.**

*Note: Die Android-10-Unterstützung dient zunächst als technische Planungsannahme; der dadurch erforderliche Austausch älterer Geräte bleibt mit dem Träger zu klären.*

> [AU-0031] Entwickler: Zweitens: Als Mindestanforderung unterstützen wir Android 10 aufwärts.
> Ältere Geräte im Haus müssten dann ausgetauscht werden.

> [AU-0032] Leitung: Das klären wir mit dem Träger, aber plant erstmal so.

---

## G-M2-029 · `requirement`

**Die No-Go-Seite ist als Liste mit Plus-Button vorgesehen.**

*Note: Die Formulierung 'ist gut' wird gemäß Leitfaden §3b als Bestätigung eines bestehenden Produktmerkmals behandelt. (Autor-Entscheid 05.09.: bleibt im Gold.)*

> [AU-0006] Pflegekraft: Bei der No-Go-Seite gibt es aber konkrete Wünsche. Die Liste mit dem Plus-
> Button ist gut, aber die Seite braucht ein rotes Stopp-Symbol als deutliches Warnsignal, damit man
> sofort sieht: Achtung, das hier ist kritisch.

---

## G-M2-030 · `requirement`

**Die About-Me-Seite soll weiterhin einen persönlichen ersten Eindruck des jeweiligen Bewohners vermitteln.**

*Note: v05 (Autor-Feedback 05.09.) NEU: AU-0004/0005 bestätigen nicht nur den Bestand (G-M2-001), sondern WELCHE Produkteigenschaft erhalten bleibt (erster Eindruck pro Bewohner, 'wirkt sehr persönlich').*

> [AU-0004] Pflegekraft: Die About-Me-Seite kommt im Team wirklich gut an, genau so hatten wir uns
> den ersten Eindruck pro Bewohner vorgestellt. Daran bitte nichts ändern.

> [AU-0005] Angehörige: Dem kann ich mich nur anschließen, das wirkt sehr persönlich.

