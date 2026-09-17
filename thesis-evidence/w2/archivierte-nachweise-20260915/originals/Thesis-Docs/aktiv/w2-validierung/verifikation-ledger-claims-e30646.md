# Verifikation LEDGER meeting-2 (Lauf e30646) — Claim-Blatt (P2/P4) · v3.1

> Claim gegen zitierte Quell-Units: ① gestützt? ② präzise (Gold-Treffer oder gestützter Zusatz)?
> Eskalation: >10 % Fehlurteile → Stichprobe verdoppeln.

## Stichprobe (Seed-Schichtung 15 % je kind → 7 von 20; keine Nicht-voll-Claim-Urteile in diesem Lauf)

---

### STICHPROBE · zugriffsprotokoll-nur-admins [compliance_constraint/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Das Zugriffsprotokoll dürfen nur Admins einsehen; normale Nutzer dürfen keinen Zugriff darauf
    haben.
  ZITIERTE QUELL-UNITS (AU-0023):
    [AU-0023] Leitung: Genau. Und dieses Protokoll dürfen nur Admins einsehen, nicht die normalen
    Nutzer.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · firestore-fixiert [decision/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Firebase Firestore bleibt endgültig die Datenbanklösung; Alternativen werden nicht weiter
    diskutiert.
  ZITIERTE QUELL-UNITS (AU-0029, AU-0030):
    [AU-0029] Entwickler: Von meiner Seite noch zwei technische Sachen. Erstens: Wir hatten Firebase
    Firestore ja nur vorläufig gesetzt. Nach den Tests der letzten Wochen sage ich – wir bleiben
    endgültig bei Firestore, die Diskussion um Alternativen machen wir nicht mehr auf.
    [AU-0030] Leitung: Gut, dann ist das jetzt fix.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · sofortinfo-max-fuenf [non_functional_requirement/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Der Sofortinfo-Bereich auf der Profil-Detailansicht soll auf maximal fünf Einträge begrenzt
    werden.
  ZITIERTE QUELL-UNITS (AU-0011, AU-0012, AU-0013):
    [AU-0011] Leitung: Nächster Punkt, der Sofortinfo-Bereich auf der Profil-Detailansicht. Der ist
    gut, aber er läuft über, wenn Leute zu viel eintragen.
    [AU-0012] Pflegekraft: Genau, der Sofortinfo-Bereich sollte auf maximal fünf Einträge begrenzt
    werden, und beim Anlegen eines Bewohners sollte mindestens ein Sofortinfo-Eintrag verpflichtend
    sein, sonst steht da bei neuen Bewohnern gar nichts.
    [AU-0013] Leitung: Einverstanden, so machen wir das.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · angehoerige-schreibrechte-offen [open_question/must_clarify] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen dürfen, ist noch offen und muss mit
    der Datenschutzbeauftragten geklärt werden.
  ZITIERTE QUELL-UNITS (AU-0024, AU-0025):
    [AU-0024] Angehörige: Ich hätte noch ein Thema: Dürfen wir als Angehörige eigentlich auch selbst
    Inhalte eintragen, zum Beispiel auf der About-Me-Seite? Oder nur lesen?
    [AU-0025] Leitung: Das ist ehrlich gesagt noch offen. Da hängen Datenschutzfragen dran, und ich
    will das nicht heute im kleinen Kreis entscheiden. Ich nehme das mit und kläre es mit unserer
    Datenschutzbeauftragten – bis dahin bleibt das unentschieden.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · android10-mindestanforderung [open_requirement/must_consider] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Android 10 aufwärts soll als Mindestanforderung eingeplant werden; die endgültige Abstimmung zu
    betroffenen Altgeräten mit dem Träger ist noch ausstehend.
  ZITIERTE QUELL-UNITS (AU-0031, AU-0032):
    [AU-0031] Entwickler: Zweitens: Als Mindestanforderung unterstützen wir Android 10 aufwärts.
    Ältere Geräte im Haus müssten dann ausgetauscht werden.
    [AU-0032] Leitung: Das klären wir mit dem Träger, aber plant erstmal so.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · no-go-bearbeiten-loeschen [requirement/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können.
  ZITIERTE QUELL-UNITS (AU-0007):
    [AU-0007] Leitung: Und Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und auch
    wieder gelöscht werden können. Im Moment wäre ja alles fest, das geht im Alltag nicht.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · uebergabe-notiz-pro-schicht [requirement/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Es muss pro Schicht eine Übergabe-Notiz geben, die der nächsten Schicht beim Öffnen der App
    prominent angezeigt wird.
  ZITIERTE QUELL-UNITS (AU-0016):
    [AU-0016] Leitung: Jetzt ein neues Thema, das aus dem Team kam: die Schichtübergabe. Wir
    brauchen pro Schicht eine Übergabe-Notiz, die die nächste Schicht beim Öffnen der App prominent
    angezeigt bekommt.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

