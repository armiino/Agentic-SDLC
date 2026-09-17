# Verifikation meeting-2 (Lauf 12d046) — Gold-Blatt (P1-Coverage) · v2 gehärtet

> **Prüfregeln (verbindlich, aus w2-matchregeln.md):**
> 1. Mehrere F-Aussagen dürfen GEMEINSAM eine Gold-Aussage decken; ein F-Claim darf mehrere Gold-Aussagen decken.
> 2. `partial` = der Kern ist da, aber eine WESENTLICHE Qualifikation fehlt: Bedingung, Negation, Position,
>    Zahlenwert, Verpflichtungsgrad oder Offenheitsstatus.
> 3. Falsche Negation oder ENTGEGENGESETZTE Entscheidung = `none` (kein Match, egal wie ähnlich der Wortlaut).
> 4. Quellenrichtigkeit zählt hier NICHT — nur der Inhalt. (Quellen werden im Claim-Blatt geprüft.)
>
> **Suchraum:** Die NONE-Urteile beruhen auf einer Suche im GESAMTEN F-Bestand (nicht nur der Quell-Region).
> Der Block zeigt Region + genannte Kandidaten; bei Zweifel: Anhang A (kompletter F-Bestand) durchsehen.
>
> **Eskalation:** Sind >10 % deiner geprüften Urteile falsch, wird die Full-Stichprobe verdoppelt.

---

### PFLICHT · G-002 [requirement] — mein Urteil: **NONE**

  GOLD:
    Die About-Me-Seite soll weiterhin einen persönlichen ersten Eindruck des jeweiligen Bewohners
    vermitteln.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-001] Die About-Me-Seite soll unverändert bleiben.
  BEGRÜNDUNG: KEIN Claim trägt den Zweck 'persönlicher erster Eindruck' (C-001 nur Bestands-Stopp; AU-0005 von F als non_relevant disponiert)
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-003 [requirement] — mein Urteil: **NONE**

  GOLD:
    Die bestehende No-Go-Seite soll weiterhin als Liste mit Plus-Button gestaltet sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-002] Die No-Go-Seite soll ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen.
  BEGRÜNDUNG: KEIN Claim zur Listen-Darstellung mit Plus-Button ('Liste ist gut' nicht erfasst)
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-009 [requirement] — mein Urteil: **NONE**

  GOLD:
    Der bestehende Sofortinfo-Bereich soll auf der Profil-Detailansicht beibehalten werden.
  F-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zum Erhalt des Sofortinfo-Bereichs ('Der ist gut' nicht erfasst)
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-030 [open_question] — mein Urteil: **PARTIAL**

  GOLD:
    Der aufgrund der Android-10-Mindestversion notwendige Austausch älterer Geräte im Haus ist noch
    mit dem Träger zu klären.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-035] Als Mindestanforderung werden Android-Geräte ab Version 10 unterstützt.
    [C-036] Ältere Geräte als Android 10 müssten ausgetauscht werden.
  BEGRÜNDUNG: Austausch-Notwendigkeit ✓, aber der KERN 'noch mit dem Träger zu klären' fehlt (nur uncertainty-Flag an C-035/36)
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

## Bilanz-/Signal-Checks (PFLICHT)

- [ ] Fs unresolved-Liste ist LEER → 'keine Lücke signalisiert' korrekt?

## Full-Stichprobe (geschichtete Seed-Auswahl: SHA-256(id+'w2seed42'), 15 % je Gold-Typ → 7 von 27)

---

### STICHPROBE · G-029 [architecture] — mein Urteil: **FULL**

  GOLD:
    Android 10 ist die Mindestversion der App; unterstützt werden Android 10 und neuere Versionen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-035] Als Mindestanforderung werden Android-Geräte ab Version 10 unterstützt.
    [C-036] Ältere Geräte als Android 10 müssten ausgetauscht werden.
  BEGRÜNDUNG: Mindestversion Android 10 ✓
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-007 [decision] — mein Urteil: **FULL**

  GOLD:
    No-Gos bleiben bewohnerbezogen; eine globale Geltung für alle Bewohner wurde ausdrücklich
    verworfen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-005] No-Gos bleiben pro Bewohner und gelten nicht global für alle Bewohner.
    [C-006] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl
    angeboten werden.
    [C-007] Aus einer Vorlagen-Liste ausgewählte No-Gos gelten nicht automatisch global für alle
    Bewohner.
  BEGRÜNDUNG: pro Bewohner + nicht global; Verwerfungs-Akt sinngemäß
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-025 [open_question] — mein Urteil: **FULL**

  GOLD:
    Ob Angehörige selbst Inhalte in der App eintragen dürfen oder nur lesenden Zugriff erhalten, ist
    noch nicht entschieden.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-028] Ob Angehörige selbst Inhalte eintragen oder nur lesen dürfen, ist noch offen.
    [C-029] Die Entscheidung über Bearbeitungsrechte von Angehörigen wird wegen Datenschutzfragen
    mit der Datenschutzbeauftragten geklärt.
    [C-030] Bis zur Klärung bleibt die Frage der Bearbeitungsrechte von Angehörigen unentschieden.
  BEGRÜNDUNG: C-029/C-030 ergänzend
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-023 [requirement] — mein Urteil: **FULL**

  GOLD:
    Das Zugriffsprotokoll über Einsichtnahmen in Bewohnerdaten muss revisionssicher sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-025] Das Zugriffsprotokoll muss revisionssicher sein.
  BEGRÜNDUNG: via C-025
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-005 [requirement] — mein Urteil: **FULL**

  GOLD:
    No-Go-Einträge sollen nachträglich bearbeitet werden können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-003] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet werden können.
    [C-004] Einträge auf der No-Go-Seite müssen gelöscht werden können.
  BEGRÜNDUNG: via C-003
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-012 [requirement] — mein Urteil: **FULL**

  GOLD:
    Noch nicht synchronisierte, offline erfasste Änderungen sollen für den Nutzer sichtbar
    gekennzeichnet werden.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-010] Noch nicht synchronisierte offline erfasste Inhalte müssen für den Nutzer erkennbar
    sein.
    [C-011] Wenn zwei Nutzer denselben Eintrag offline geändert haben, muss eine klare
    Konfliktanzeige angezeigt werden.
    [C-012] Bei Offline-Konflikten darf nichts stillschweigend überschrieben werden.
  BEGRÜNDUNG: via C-010
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-026 [requirement] — mein Urteil: **FULL**

  GOLD:
    Ein Dunkelmodus soll als nachrangiger Wunsch berücksichtigt werden.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-031] Ein Dunkelmodus ist als Wunsch erfasst, hat aber keine hohe Priorität.
    [C-032] Eine einstellbare Schriftgröße ist als Wunsch erfasst, hat aber keine hohe Priorität.
  BEGRÜNDUNG: 'keine hohe Priorität' = nachrangig
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

## Anhang A — kompletter F-Bestand (für NONE-Gegenproben)

- [C-001] Die About-Me-Seite soll unverändert bleiben.
- [C-002] Die No-Go-Seite soll ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen.
- [C-003] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet werden können.
- [C-004] Einträge auf der No-Go-Seite müssen gelöscht werden können.
- [C-005] No-Gos bleiben pro Bewohner und gelten nicht global für alle Bewohner.
- [C-006] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl angeboten
- werden.
- [C-007] Aus einer Vorlagen-Liste ausgewählte No-Gos gelten nicht automatisch global für alle
- Bewohner.
- [C-008] Der Sofortinfo-Bereich auf der Profil-Detailansicht soll auf maximal fünf Einträge
- begrenzt werden.
- [C-009] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend sein.
- [C-010] Noch nicht synchronisierte offline erfasste Inhalte müssen für den Nutzer erkennbar sein.
- [C-011] Wenn zwei Nutzer denselben Eintrag offline geändert haben, muss eine klare Konfliktanzeige
- angezeigt werden.
- [C-012] Bei Offline-Konflikten darf nichts stillschweigend überschrieben werden.
- [C-013] Bei Offline-Konflikten soll der Nutzer entscheiden, welche Version gilt.
- [C-014] Es muss pro Schicht eine Übergabe-Notiz geben.
- [C-015] Die Übergabe-Notiz einer Schicht muss der nächsten Schicht beim Öffnen der App prominent
- angezeigt werden.
- [C-016] Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein.
- [C-017] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden.
- [C-018] Archivierte Übergabe-Notizen müssen auffindbar bleiben.
- [C-019] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
- [C-020] Die Suche soll Anwendungsfälle wie das Finden von Bewohnern anhand von No-Go-Stichworten
- unterstützen.
- [C-021] Jede Einsichtnahme in Bewohnerdaten muss protokolliert werden.
- [C-022] Das Zugriffsprotokoll muss erfassen, wer welches Profil wann angesehen hat.
- [C-023] Das Zugriffsprotokoll ist verpflichtend, weil die Heimaufsicht dies als Auflage verlangt.
- [C-024] Ohne Umsetzung des Zugriffsprotokolls darf die App nicht eingesetzt werden.
- [C-025] Das Zugriffsprotokoll muss revisionssicher sein.
- [C-026] Das Zugriffsprotokoll dürfen nur Admins einsehen.
- [C-027] Normale Nutzer dürfen das Zugriffsprotokoll nicht einsehen.
- [C-028] Ob Angehörige selbst Inhalte eintragen oder nur lesen dürfen, ist noch offen.
- [C-029] Die Entscheidung über Bearbeitungsrechte von Angehörigen wird wegen Datenschutzfragen mit
- der Datenschutzbeauftragten geklärt.
- [C-030] Bis zur Klärung bleibt die Frage der Bearbeitungsrechte von Angehörigen unentschieden.
- [C-031] Ein Dunkelmodus ist als Wunsch erfasst, hat aber keine hohe Priorität.
- [C-032] Eine einstellbare Schriftgröße ist als Wunsch erfasst, hat aber keine hohe Priorität.
- [C-033] Firebase Firestore wird endgültig als Lösung beibehalten.
- [C-034] Die Diskussion über Alternativen zu Firebase Firestore wird nicht wieder aufgenommen.
- [C-035] Als Mindestanforderung werden Android-Geräte ab Version 10 unterstützt.
- [C-036] Ältere Geräte als Android 10 müssten ausgetauscht werden.
- [C-037] Der Punkt zu Piktogrammen ist fachlich noch nicht konkret genug und bleibt offen.
