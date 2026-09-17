# Verifikation LEDGER meeting-2 (Lauf e30646) — Gold-Blatt (P1-Coverage) · v3.1

> **Prüfregeln (verbindlich, aus w2-matchregeln.md):**
> 1. Mehrere Claims dürfen GEMEINSAM eine Gold-Aussage decken; ein Claim darf mehrere Gold-Aussagen decken.
> 2. `partial` = Kern da, aber WESENTLICHE Qualifikation fehlt (Bedingung/Negation/Position/Zahlenwert/
>    Verpflichtungsgrad/Offenheitsstatus). 3. Falsche Negation/Gegenentscheidung = `none`.
> 4. Quellenrichtigkeit zählt hier NICHT — nur Inhalt (→ Claim-Blatt).
> **Suchraum:** NONE-Urteile = Suche im GESAMTEN Claim-Bestand; Gegenprobe: Anhang A.
> **Eskalation:** >10 % Fehlurteile → Full-Stichprobe verdoppeln.

---

### PFLICHT · G-002 [requirement] — mein Urteil: **NONE**

  GOLD:
    Die About-Me-Seite soll weiterhin einen persönlichen ersten Eindruck des jeweiligen Bewohners
    vermitteln.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [about-me-unveraendert] Die About-Me-Seite soll in ihrer aktuellen Form unverändert bleiben.
  BEGRÜNDUNG: KEIN Claim trägt den Zweck 'persönlicher erster Eindruck' — about-me-unveraendert nur Bestands-Stopp (Detail nur im Evidenz-Zitat, nicht in der Proposition; P1 urteilt auf Propositions-Ebene)
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-003 [requirement] — mein Urteil: **NONE**

  GOLD:
    Die bestehende No-Go-Seite soll weiterhin als Liste mit Plus-Button gestaltet sein.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [no-go-stopp-symbol] Die No-Go-Seite muss ein rotes Stopp-Symbol als deutliches Warnsignal
    anzeigen.
  BEGRÜNDUNG: KEIN Claim zur Listen-Darstellung mit Plus-Button (Detail in verwendeter Unit AU-0006 verloren)
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-012 [requirement] — mein Urteil: **NONE**

  GOLD:
    Noch nicht synchronisierte, offline erfasste Änderungen sollen für den Nutzer sichtbar
    gekennzeichnet werden.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [offline-konfliktanzeige] Bei Offline-Konflikten muss eine klare Konfliktanzeige erfolgen;
    Änderungen dürfen nicht stillschweigend überschrieben werden, und der Nutzer entscheidet über
    die gültige Version.
  BEGRÜNDUNG: KEIN Claim zur sichtbaren Kennzeichnung noch nicht synchronisierter Offline-Änderungen — offline-konfliktanzeige deckt nur den Konfliktfall (Detail in verwendeter Unit AU-0014 verloren)
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-030 [open_question] — mein Urteil: **PARTIAL**

  GOLD:
    Der aufgrund der Android-10-Mindestversion notwendige Austausch älterer Geräte im Haus ist noch
    mit dem Träger zu klären.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [android10-mindestanforderung] Android 10 aufwärts soll als Mindestanforderung eingeplant
    werden; die endgültige Abstimmung zu betroffenen Altgeräten mit dem Träger ist noch ausstehend.
  BEGRÜNDUNG: android10-mindestanforderung: Träger-Abstimmung zu Altgeräten ✓ ausstehend, aber die AUSTAUSCH-Notwendigkeit ist nur impliziert ('betroffene Altgeräte'), nicht ausgesagt — symmetrische Strenge zur F-Seite (dort partial wegen fehlender Träger-Klärung)
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

## Signal-Zuordnungs-Checks (PFLICHT)

- [ ] AU-0002 needs_human: Gesprächskontext — kein Gold-Inhalt betroffen (kein Treffer, aber billige Human-Prüfung) — Zuordnung korrekt?
- [ ] AU-0008 attach_as_evidence: Gegenrede zu no-gos-pro-bewohner — Gold G-007 ist bereits GEDECKT (kein Miss-Treffer) — Zuordnung korrekt?
- [ ] AU-0026 needs_human: Wichtigkeitsbekundung ohne Bezug — kein Gold-Inhalt betroffen — Zuordnung korrekt?

## Full-Stichprobe (Seed-Schichtung 15 % je Typ → 7 von 27)

---

### STICHPROBE · G-029 [architecture] — mein Urteil: **FULL**

  GOLD:
    Android 10 ist die Mindestversion der App; unterstützt werden Android 10 und neuere Versionen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [android10-mindestanforderung] Android 10 aufwärts soll als Mindestanforderung eingeplant
    werden; die endgültige Abstimmung zu betroffenen Altgeräten mit dem Träger ist noch ausstehend.
  BEGRÜNDUNG: android10-mindestanforderung ('Android 10 aufwärts als Mindestanforderung')
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-007 [decision] — mein Urteil: **FULL**

  GOLD:
    No-Gos bleiben bewohnerbezogen; eine globale Geltung für alle Bewohner wurde ausdrücklich
    verworfen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [no-gos-pro-bewohner] No-Gos bleiben bewohnerbezogen und dürfen nicht automatisch global für
    alle Bewohner gelten.
    [no-go-vorlagenliste] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur
    Auswahl angeboten werden.
  BEGRÜNDUNG: no-gos-pro-bewohner: bewohnerbezogen + nicht automatisch global (Verwerfung sinngemäß, must_not-Decision)
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-025 [open_question] — mein Urteil: **FULL**

  GOLD:
    Ob Angehörige selbst Inhalte in der App eintragen dürfen oder nur lesenden Zugriff erhalten, ist
    noch nicht entschieden.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [angehoerige-schreibrechte-offen] Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen
    dürfen, ist noch offen und muss mit der Datenschutzbeauftragten geklärt werden.
  BEGRÜNDUNG: angehoerige-schreibrechte-offen
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-023 [requirement] — mein Urteil: **FULL**

  GOLD:
    Das Zugriffsprotokoll über Einsichtnahmen in Bewohnerdaten muss revisionssicher sein.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [zugriffsprotokoll-pflicht] Jede Einsichtnahme in Bewohnerdaten muss revisionssicher
    protokolliert werden, einschließlich wer wann welches Profil angesehen hat.
  BEGRÜNDUNG: zugriffsprotokoll-pflicht ('revisionssicher protokolliert')
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-005 [requirement] — mein Urteil: **FULL**

  GOLD:
    No-Go-Einträge sollen nachträglich bearbeitet werden können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [no-go-bearbeiten-loeschen] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und
    gelöscht werden können.
  BEGRÜNDUNG: no-go-bearbeiten-loeschen (bearbeitet)
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-026 [requirement] — mein Urteil: **FULL**

  GOLD:
    Ein Dunkelmodus soll als nachrangiger Wunsch berücksichtigt werden.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [dark-mode-wunsch] Ein Dunkelmodus ist als Wunsch für den Nachtdienst vorgesehen, hat aber keine
    hohe Priorität.
    [schriftgroesse-wunsch] Eine einstellbare Schriftgröße ist als Wunsch vorgesehen, um einzelnen
    älteren Kolleginnen zu helfen, hat aber keine hohe Priorität.
  BEGRÜNDUNG: dark-mode-wunsch ('keine hohe Priorität' = nachrangig)
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-021 [requirement] — mein Urteil: **FULL**

  GOLD:
    Die App soll eine Stichwortsuche über alle Bewohnerprofile hinweg ermöglichen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [globale-bewohnersuche] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten
    geben.
  BEGRÜNDUNG: globale-bewohnersuche
  - [ ] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

## Anhang A — kompletter Ledger-Claim-Bestand

- [about-me-unveraendert] Die About-Me-Seite soll in ihrer aktuellen Form unverändert bleiben.
- [no-go-stopp-symbol] Die No-Go-Seite muss ein rotes Stopp-Symbol als deutliches Warnsignal
- anzeigen.
- [no-go-bearbeiten-loeschen] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und
- gelöscht werden können.
- [no-gos-pro-bewohner] No-Gos bleiben bewohnerbezogen und dürfen nicht automatisch global für alle
- Bewohner gelten.
- [no-go-vorlagenliste] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur
- Auswahl angeboten werden.
- [sofortinfo-max-fuenf] Der Sofortinfo-Bereich auf der Profil-Detailansicht soll auf maximal fünf
- Einträge begrenzt werden.
- [sofortinfo-mindestens-eins] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag
- verpflichtend erfasst werden.
- [offline-konfliktanzeige] Bei Offline-Konflikten muss eine klare Konfliktanzeige erfolgen;
- Änderungen dürfen nicht stillschweigend überschrieben werden, und der Nutzer entscheidet über die
- gültige Version.
- [uebergabe-notiz-pro-schicht] Es muss pro Schicht eine Übergabe-Notiz geben, die der nächsten
- Schicht beim Öffnen der App prominent angezeigt wird.
- [uebergabe-notiz-bewohnerverlinkung] Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein,
- sodass direkt auf ein Bewohnerprofil verwiesen werden kann.
- [uebergabe-notizen-archivierung] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert
- werden und danach weiterhin auffindbar bleiben.
- [globale-bewohnersuche] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten
- geben.
- [zugriffsprotokoll-pflicht] Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert
- werden, einschließlich wer wann welches Profil angesehen hat.
- [zugriffsprotokoll-nur-admins] Das Zugriffsprotokoll dürfen nur Admins einsehen; normale Nutzer
- dürfen keinen Zugriff darauf haben.
- [angehoerige-schreibrechte-offen] Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen
- dürfen, ist noch offen und muss mit der Datenschutzbeauftragten geklärt werden.
- [dark-mode-wunsch] Ein Dunkelmodus ist als Wunsch für den Nachtdienst vorgesehen, hat aber keine
- hohe Priorität.
- [schriftgroesse-wunsch] Eine einstellbare Schriftgröße ist als Wunsch vorgesehen, um einzelnen
- älteren Kolleginnen zu helfen, hat aber keine hohe Priorität.
- [firestore-fixiert] Firebase Firestore bleibt endgültig die Datenbanklösung; Alternativen werden
- nicht weiter diskutiert.
- [android10-mindestanforderung] Android 10 aufwärts soll als Mindestanforderung eingeplant werden;
- die endgültige Abstimmung zu betroffenen Altgeräten mit dem Träger ist noch ausstehend.
- [piktogramme-unklar] Der Wunsch, das Thema Piktogramme größer zu denken, ist noch nicht konkret
- genug und muss vor einer Ableitung von Anforderungen präzisiert werden.
