> **Zählkorrektur vom 15.09.2026:** Für F1 sind im geprüften Abschnitt elf neue Core-Items belegt, nicht vierzehn. Der [datierte Nachtrag](f1-zaehlkorrektur-20260915.md) ersetzt die Zählangaben zu F1 im folgenden historischen Bericht. Die übrigen Urteile behalten ihren ausgewiesenen Umfang.

# Z2 — Fünf Fallblätter der fachlichen Fallprüfung (Messprotokoll §4 / Umsetzungsplan §6.2)

> Status: ERSTELLT 07.09.2026, REVIDIERT 07.09. spät (Kollegen-Review: Belege und
> Einzelurteile geschärft, F5 getrennt, F1-Kette feld-verglichen) — retrospektive Nachprüfung
> HISTORISCHER Läufe mit **retrospektiven, dokumentierten Prüfkriterien** (§0; die
> historischen Fälle waren aus dem Projektgedächtnis bekannt — keine verblindete Vorab-
> Fixierung beansprucht). KEINE Erfolgsquote aus fünf gezielt gewählten Fällen; F1/F5a teilen
> denselben Lauf — fünf FALLTYPEN, nicht fünf unabhängige End-to-End-Ausführungen.
> **Urteils-Legende (strikt):** erfüllt = am benannten Beleg gezeigt · abweichend = Soll
> verletzt · **n. b.** = erwartete Evidenz fehlt/unzureichend · **n. a.** = Übergang
> vertragsgemäß nicht vorgesehen · **nicht untersucht** = Prüfumfang bewusst begrenzt (z. B.
> Dry-run) — keiner dieser drei Zustände ist eine Erfüllung.

## 0 · Dokumentierte Prüfkriterien je Falltyp (aus den fachlichen Verträgen, retrospektiv angelegt)

1. **Neue Anforderung:** Vorschlag an Tor 1 → menschliche Autorisierung → Core-Item mit
   Herkunft → Placement/PBI → deterministische Issue-Projektion; keine Wahrheits-Mutation am
   Gate vorbei. Rollenklarheit: Der Core bleibt der maßgebliche fachliche Projektstand;
   GitHub dient als Projektion und kann zugleich externe Änderungsvorschläge oder Antworten
   liefern — solche Eingaben werden erst nach Prüfung und Freigabe fachliche Core-Änderung.
2. **Präzisierung:** Vorprüfung erkennt den Bestand → REFINE statt Neu-Item; menschliche
   Freigabe; Versionierung (v1→v2, Historie erhalten) statt Überschreiben; PBI über den
   Update-Weg.
3. **Widerspruch:** CONTRADICT erkannt statt still überschrieben → menschliche Auflösung am
   decision-gate → DEC + supersedes; alte Wahrheit bleibt (superseded), Abhängige werden
   angeglichen.
4. **Wiederholung:** kein Duplikat; Bestand wird erkannt (RESTATE/ALREADY_DECIDED bzw.
   „Schon einmal…"-Hinweis); kein Auto-Skip — der Mensch sieht die Bilanz.
5. **Offene Frage + Rückweg:** Frage wird als OPEN_QUESTION geführt und bleibt offene DEC
   (kein erfundener Beschluss); der GitHub-Kommentar-Rückweg belegt strikt NUR den
   dokumentierten externen Eingang und seine gated Verarbeitung — nicht allgemeine
   Feld-Änderungs- oder Drift-Überwachung.

---

## Fallblatt Z2-F1 — Neue Anforderung (Meeting-4 mit Real-Writes)

1. **Fall/Typ/Auswahl:** Z2-F1 · Neue Anforderung · im Messprotokoll §4 VORAB benanntes
   Material · Anforderung: gated Wahrheitsbildung bis zur realen Projektion.
2. **Lauf/Stand:** HISTORISCH, `runs/fullworkflow/20260818_084712_765eea` (18.08.,
   Block-K-Stand: 552 Tests · Mutations-Basis SHA `a40320e7…→920c92e9…`). Ausgangs-Core:
   Checkpoint `core-ckpt-vor-blockL`; Endlage nach Block L: Core 212. Quelle:
   `input/transcripts/meeting-4-ux-block-l.txt` — **gezielt gebautes 8-Aussagen-Transkript
   (synthetisch, offengelegt)**.
3. **Soll:** §0-Kriterium 1 (retrospektiv).
4. **Quellbeleg:** u. a. NEW „Angehörige müssen Besuche … vorab in der App mit Datum und
   Uhrzeit ankündigen können" · REFINE mit Zahlwert/Format „maximal fünf Fotos, nur JPG- oder
   PNG-Formate" (beide wörtlich in `07-ingest/plan.json`).
5. **Maschineller Vorschlag:** 12 Ops (NEW · REFINE→REQ-79 · CONTRADICT→REQ-05 ·
   OPEN_QUESTION „Aufbewahrung einnahmebezogener Bemerkungen") — typisiert, mit Rationale.
6. **Menschliche Entscheidung:** Ingest-Gate 8 apply / 4 reject, wörtliche Skip-Begründungen
   (`07-ingest/human-decisions.json`, reviewer „human (review-ui)"; z. B. „Doppelung: …läuft
   als CONTRADICT — keine zweite Frage-DEC nötig"); PBI-Gate Sammel-Freigabe (accept-all im
   UI, dokumentiert).
7. **Core-Änderung:** +14 Items (REQ-80–86, DEC-004/005, ARCH-46, FC-15, PBI-045–047);
   REQ-05→superseded; OPEN_QUESTION → DEC-004 (Meeting-Bahn).
8. **Projektion:** REAL-WRITE belegt (`07-github/applied/github-forward-apply-report.json`,
   `dryRun:false, executed:true`): created 3 → **gh#43/#44/#45** (= PBI-045/046/047, per
   Snapshot `20260818_125520_cb157c` bestätigt), updated 2 (#28/#40), 2 held; Nach-Ernte
   drift=0.
9. **Urteile (REVIDIERT — auf die VERFOLGTE NEW-Kette gestützt, Feldvergleich 07.09.):**
   Verfolgt wurde die NEW-Aussage „Angehörige müssen Besuche bei ihrer Bezugsperson vorab in
   der App **mit Datum und Uhrzeit** ankündigen können": Plan-Statement (07-ingest/plan.json,
   wörtlich) → apply-Entscheid → Core **REQ-82** (WORTIDENTISCH, im Live-Core bis heute v1) →
   PBI-046 → **gh#44, Issue-Titel: „Besuche bei der Bezugsperson mit Datum und Uhrzeit vorab
   ankündigen"** (Snapshot 18.08.) — die wesentlichen Qualifikationen (vorab, Datum, Uhrzeit)
   überleben bis in den Issue-Titel. Zweitkette REQ-83→PBI-047→gh#45: Sync-Delta-Felder zur
   Laufzeit (Titel/AK/coveredRequirementIds) = Issue-Titel #45 identisch.
   Einordnung **erfüllt** (alle vier Op-Arten korrekt typisiert; übrige Ops = Kontext) ·
   Inhaltserhaltung **erfüllt für die verfolgten Ketten** (REQ-82-Kette + REFINE-Zahlwerte;
   kein Pauschal-Urteil über alle 14 Items) · Freigabebezug **erfüllt für die belegten Ops**
   (apply-Entscheide je incomingItemId; nicht alle 14 einzeln durchgeprüft) · Herkunft
   **erfüllt** (incomingItemIds→Entity-IDs→Issue-Mapping durchgängig) ·
   Projektionsentsprechung **erfüllt für die geprüften Felder** (Titel-Vergleich #44/#45 +
   snapshot-bestätigte Zuordnung; drift=0 nur bezogen auf den Prüfbereich der Sofort-Ernte).
10. **Abweichung/Grenze:** Der execute-Umschalter (config `execute:false` → Apply-Report
    `executed:true` nach Resume) ist NICHT als eigenes Artefakt im Ordner nachvollziehbar —
    der Apply-Report belegt den ausgeführten Write, NICHT, wie die Umschaltung autorisiert
    wurde (Freigabe der Ops und Ausführungskonfiguration sind verschiedene Aussagen) ·
    „Schon-geklärt"-Note nicht gesichtet (Testplan-Notiz; `search_rejections` lief,
    returned:0) · L7b-Fehlfall ungetestet · Aussagegrenze: gelenktes synthetisches
    Transkript, EIN Lauf.

## Fallblatt Z2-F2 — Präzisierung (Steward-Diktat → REFINE REQ-42)

1. **Fall/Typ/Auswahl:** Z2-F2 · Präzisierung · **Auswahlbegründung (Pflicht):** vollständigst
   belegter reiner REFINE-Fall (alle Verträge vorhanden). Verworfene Kandidaten: Run
   `20260729_153821_c7fc4a` ist ein NEW_FEATURE-Fall (zudem defekt-behaftet: 3/4 Reqs in
   FC-12 statt FC-14; rückgerollt — FC-14/PBI-047-IDs heute anders vergeben) · CA-5→REQ-36
   ist primär als R-59-Bugfall dokumentiert · E11 (Issue #32/#33, 23.07.) ist technisch ein
   SUPERSEDE.
2. **Lauf/Stand:** HISTORISCH, `runs/fullworkflow/20260817_123342_f23f06` (17.08.,
   Steward-Bahn, Block-K-Kampagne). Quelle: Autor-Diktat via Steward-Chat (AuthorFront-Delta,
   Incoming AF-1; Diktat wörtlich im AF-Delta).
3. **Soll:** §0-Kriterium 2 (retrospektiv).
4. **Quellbeleg:** Diktat präzisiert die vage Prüf-Idee (Kalender/Medikamente) zu einer
   konkreten Anforderung (7-Tage-Übersicht für freigegebene Angehörige).
5. **Maschineller Vorschlag:** GENAU 1 REFINE auf REQ-42 (`applied/delta.json`:
   `refined:1`, alles andere 0) — die Vorprüfung fand den Bestand; kein Neu-Item.
6. **Menschliche Entscheidung:** `ingest-gate-decisions.json` reviewer „author via
   steward-chat", AF-1 apply · PBI-Alignment accept MIT menschlich editiertem Titel/Statement
   (PBI-028) — **menschliche Ergänzung, getrennt ausgewiesen** (Nach-Gate-Inhalt, keine
   maschinelle Erstleistung).
7. **Core-Änderung:** REQ-42 v1→**v2** (Historie erhalten; Item ist heute v3 = spätere
   Fortschreibung) · PBI-028→v2 „Übersicht der Medikamenten-Einnahmen…".
8. **Projektion:** Update- statt Neu-Weg; `github-sync-delta.json` enthält PBI-028.
9. **Urteile:** Einordnung **erfüllt** (REFINE statt NEW) · Inhaltserhaltung **erfüllt** —
   beides geprüft: die Präzisierung ist in v2 AUFGENOMMEN (7-Tage-Umfang + „freigegebene
   Angehörige" aus dem Diktat stehen im v2-Text) UND v1 bleibt in der Historie erhalten;
   die PBI-Redaktion (editierter Titel/Statement) ist als menschlicher Nach-Gate-Eingriff
   dokumentiert (§6), nicht als maschinelle Leistung gezählt · Freigabebezug **erfüllt** ·
   Herkunft **erfüllt** (AF-1 + Reviewer-Vermerk) · Projektionsentsprechung: Kopplung
   **erfüllt** (Sync-Delta), real ausgeführter Issue-Write **n. b.** (kein Apply-Report
   gesichtet).
10. **Abweichung/Grenze:** keine Soll-Verletzung beobachtet. Aussagegrenze:
    Steward-/Diktat-Bahn (kein Meeting-Transkript); Endtext des PBI enthält menschliche
    Redaktion.

## Fallblatt Z2-F3 — Widerspruch (R-14-Zyklus)

1. **Fall/Typ/Auswahl:** Z2-F3 · Widerspruch · im Messprotokoll §4 VORAB benanntes Material.
2. **Lauf/Stand:** HISTORISCH, `runs/fullworkflow/20260804_121433_eb181a` (04.08.,
   R-14-Stand 335→339 Tests; Interactive-Profil, `execute:false` = deklariertes EXPERIMENT).
   Ausgangs-Core: 194er (`coreItemsBefore:194`). Quelle:
   `input/transcripts/meeting-2-delta.txt` Zeile 6: „Eigentlich sollten No-Gos
   bewohnerübergreifend gelten, also global für alle, nicht pro Bewohner."
3. **Soll:** §0-Kriterium 3 (retrospektiv).
4. **Quellbeleg:** widerspricht REQ-32 („bewohnerbezogen, nicht global") — Negation kippt.
5. **Maschineller Vorschlag:** Ingest-Agent: Op REQ-05 `CONTRADICT` auf REQ-32 mit Rationale
   („…stellt diese Festlegung wieder als offene fachliche Klärung infrage…"); Apply mintet
   DEC-001 (`outcome:"contradicted"`); DecisionScan füllt die Gate-Vorlage (Prefill).
6. **Menschliche Entscheidung:** decision-gate INTERACTIVE (einziges der 5 Gates; die
   übrigen 4 AcceptAll = deklarierter Experiment-Modus): Pause 12:15:41→12:18:02 (~2,5 min),
   reviewer „author-go (Live-Beleg-Experiment 04.08.)", DEC-001 resolve → **ADOPT_NEW** mit
   neuem Statement + Begründung („Bedeutung kippt, daher Ablösung statt Verfeinerung").
7. **Core-Änderung:** `decision-apply-report.json`: REQ-78 NEU · REQ-32 **superseded**
   (erhalten, v2) · DEC-001 resolved · covers-Swap PBI-012/023 · 3× MARK_CHANGED ·
   PbiAlignment lief live (3 Vorschläge); Alignment-Inhalte bewusst NICHT blind übernommen —
   PBIs blieben `needs_clarify`. Core 194→196. Kangal 0 Verletzungen über 3 Saves.
8. **Projektion:** **n. a.** — `execute:false`, Forward dryRun (0 Issues), bewusst.
9. **Urteile (REVIDIERT — Ergebnis AUFGETEILT statt Sammelurteil):**
   ① Konflikterkennung + menschlich autorisierte Ablösung: **erfüllt** (CONTRADICT statt
   still überschreiben; ADOPT_NEW-Statement des Menschen = übernommener neuer Inhalt) ·
   ② Relations-/Bestandswirkung: **erfüllt** (REQ-32 superseded statt gelöscht, v2; REQ-78
   neu; covers-Swap vollzogen) · ③ fachliche PBI-ANGLEICHUNG: **blieb als Klärungsbedarf
   OFFEN** (PBI-012/023 `needs_clarify` — die Alignment-Inhalte wurden bewusst nicht blind
   übernommen; das ist der dokumentierte Vertrag [Vorschläge + Markierung], KEINE stille
   Soll-Umdeutung: eine Weiterführung bis zu aktualisierten Issues wurde in diesem Experiment
   nicht untersucht) · Freigabebezug: **erfüllt NUR für das interaktive decision-gate**; die
   übrigen 4 Gates liefen AcceptAll (deklarierter Experiment-Modus, gesondert genannt) ·
   Herkunft **abweichend (Anzeige):** das `origin`-Feld der Gate-Vorlage nennt einen fremden
   Recipe-Lauf `20260804_121518_4a8870` (Prefill-Artefakt) — ein eigener Befund; der technisch
   stimmende Planbezug neutralisiert die irreführende Anzeige am menschlichen
   Entscheidungspunkt nicht · Projektionsentsprechung: **nicht untersucht** (execute:false,
   Dry-run — Aussage nur über diesen Prüfumfang).
10. **Abweichung/Grenze:** Der behauptete „byte-identische" Core-Restore hat KEIN
    maschinelles Artefakt (nur narrativ; indirekt gestützt: heutige REQ-78-ID ist anderweitig
    neu vergeben) — erster sichtbarer Ort: Run-Ordner ohne Restore-Beleg ·
    `logs/decision-log.jsonl` leer (Verlauf stattdessen in events.jsonl + Vertragsdateien) ·
    Lauf meldete FERTIG trotz offener needs_clarify-PBIs (dokumentierte Absicht, als solche
    ausgewiesen). Aussagegrenze: Experiment ohne Real-Write, 4/5 Gates AcceptAll.

## Fallblatt Z2-F4 — Wiederholte Information (Zweit-Einspeisung desselben Deltas)

1. **Fall/Typ/Auswahl:** Z2-F4 · Wiederholung · Block-K-Fall N2 (vorab benannt; ≠ Replay
   derselben Plan-ID).
2. **Lauf/Stand:** HISTORISCH, `runs/fullworkflow/20260818_123055_a0b172` (18.08., Block-K-
   Stand): `entryPoint:"delta"`, `fromDelta:"input/deltas/session2-teilb-delta.json"` — die
   ZWEITE Einspeisung desselben Deltas (Erstverarbeitung: `20260818_114850_eab853`).
3. **Soll:** §0-Kriterium 4 (retrospektiv).
4. **Quellbeleg:** identischer Delta-Inhalt wie im Erstlauf.
5. **Maschineller Vorschlag:** `ingestion-summary.json`: **RESTATE:2 (→REQ-85/86) +
   ALREADY_DECIDED:1 (→DEC-002)** — Bestand erkannt, keine Zweit-DEC (Claim angeheftet),
   0 neue Items.
6. **Menschliche Entscheidung:** Bilanz „nur bestätigt" am Gate gesehen; keine Mutation
   freizugeben; Folgegates liefen als sauberes Leer-Ballett (R-50-Fix).
7. **Core-Änderung (präzisiert):** keine neuen fachlichen Items und keine festgestellten
   sachlichen Duplikate (delta: 0 added/refined/superseded/contradicted); zulässige
   Evidenz-/Herkunfts-Anhänge (Claim-Merge an Bestandsitems durch RESTATE) davon getrennt.
8. **Projektion:** **n. a.** (nichts zu projizieren).
9. **Urteile (REVIDIERT):** Einordnung **erfüllt** (Bestand erkannt: RESTATE/ALREADY_DECIDED,
   keine Zweit-DEC) · Inhaltserhaltung **n. b.** (Wiederholung prüft gerade die Erhaltung des
   Bestands — ein Text-Vorher/Nachher-Vergleich der Bestandsitems wurde nicht durchgeführt;
   die delta-Zähler „0 Inhaltsänderungen" sind Indiz, kein Feldvergleich) · Freigabebezug
   **erfüllt** (kein Auto-Skip; leere Gates sind der geprüfte R-50-Pfad) · Herkunft
   **erfüllt** (ALREADY_DECIDED verweist auf DEC-002) · Projektionsentsprechung **n. a.**
10. **Abweichung/Grenze:** Zähl-Diskrepanz Doku („4/4") vs. Run-Artefakt (3 Ops) — das
    Artefakt zählt; erster sichtbarer Ort: Testplan-Bilanz · geprüft wurde ein wiederholtes
    DELTA, NICHT ein wiederholtes Meeting-TRANSKRIPT (offene Lücke) · die R-35-„Schon einmal
    abgelehnt"-Note feuerte hier nicht (kein Rejection-Match); ihr Live-Beleg liegt in Z2-F5
    (REJ-018-Kette).

## Fallblatt Z2-F5 — Offene Frage + GitHub-Kommentar-Rückweg

1. **Fall/Typ/Auswahl:** Z2-F5 · Doppel-Fall laut Messprotokoll: (a) 9g-Frage bleibt offen ·
   (b) der dokumentierte Kommentar-RÜCKWEG als externer Eingang.
2. **Lauf/Stand:** (a) primär `20260818_084712_765eea` (OPEN_QUESTION→**DEC-004**, Meeting-
   Bahn; Block-K-Stand) + Mechanik-Beleg im Ein-Graph `20260805_131538_899463` (05.08.:
   2 Fragen → OPEN_QUESTION-Ops → DECs `question_opened`, Attempt-1-Repair
   UNPLACED_INCOMING→Pass, defer-all; Core danach restauriert — Experiment). (b)
   Kommentar-Rundweg **20.08.** (K1–K8), Läufe `20260820_172921_1ca9d2` (EMPTY_HARVEST),
   `…173141_6e75fa` (Team-Antwort→REQ-90, UPDATE #45), `…183855_b7747b` (NOTE_COMMENT #43
   failed = R-62b), `…190220_895d18` (FLAG_DRIFT→overwrite #43); Stand 603→609 Tests,
   Core 226.
3. **Soll:** §0-Kriterium 5 (retrospektiv).
**— F5a: EINE offene Frage (Kette getrennt, REVIDIERT 07.09.) —**

4. **Quellbeleg F5a:** Die verfolgte Frage ist die von DEC-004 aus Lauf `765eea`:
   OPEN_QUESTION-Op wörtlich „**Wie lange sollen einnahmebezogene Bemerkungen bei der
   Medikamenten-Gabe aufbewahrt werden?**" (07-ingest/plan.json).
5. **Maschineller Vorschlag F5a:** OPEN_QUESTION-Op → Meeting-Bahn → DEC `question_opened`,
   Origin `MEETING_OPEN_QUESTION` (geteilter MeetingQuestionMint).
6. **Menschliche Entscheidung F5a:** Aufnahme am Ingest-Gate (apply); am decision-gate NICHT
   aufgelöst — die Frage bleibt bewusst offen (genau das Soll).
7. **Core-Wirkung F5a:** **DEC-004 offen im Live-Core** (Status open_decision). ZUSATZBELEGE
   (ausdrücklich getrennt, andere Fragen/Läufe): DEC-001/002 aus Steward-Läufen 17.08. ebenso
   offen; Ein-Graph-MECHANIK-Beleg = Lauf `131538` (2 Fragen — u. a. „Dürfen Angehörige
   selbst Inhalte eintragen … oder nur lesen?" — → 2 DECs `question_opened`, Attempt-1-Repair
   → Pass; Core danach restauriert, Experiment).
8. **Projektion F5a:** **n. a.** — offene Frage-DECs haben vertragsgemäß KEINEN eigenen
   Projektionskanal (9i = unbebauter Parkplatz); daraus folgt KEINE Außensicht offener
   Fragen. Unverknüpfte BEOBACHTUNG (belegt NICHT die Projektion der gewählten DEC): ein
   ähnlich benannter Klärungsbedarf (Angehörigen-Schreibrechte) erscheint in
   `docs/anforderungen.md` als REQ-08 v2 — als Anforderungs-Text, nicht als Frage-Objekt.
9. **Urteile F5a:** Einordnung **erfüllt** (Frage bleibt Frage, kein erfundener Beschluss) ·
   Inhaltserhaltung **erfüllt** (Fragetext wörtlich in der DEC) · Freigabebezug **erfüllt** ·
   Herkunft **erfüllt** (MEETING_OPEN_QUESTION-Origin; ein identischer origin-TYP belegt die
   Herkunfts-KATEGORIE, die individuelle Kette belegt der Lauf-Plan) · Projektion **n. a.**

**— F5b: EIN externer Kommentar-Rückweg (Kette getrennt) —**

4. **Quellbeleg F5b:** Der verfolgte externe Eingang ist der Kommentar-Vorschlag
   „Die Besuchsübersicht sollte die Besuche der **nächsten 14 Tage** zeigen" (K1, 20.08.).
   WEITERE Eingänge der Serie (je eigene Teilurteile, nicht Teil der Hauptkette): Wetter-Idee
   (K2), Team-Antwort „Ja, bitte nur für Admins." (K4 — Antwort auf die Steward-Rückfrage zur
   Sichtbarkeit, Geltungsbereich darüber nachvollziehbar), Blockquote-AK-Edit #43 (K6),
   Whitespace-only #7 (K8).
5. **Maschineller Vorschlag F5b:** Ernte via `pull_issue_comments` → Draft am Gate;
   EMPTY_HARVEST korrekt leer (K3, 45 geprüft); AK-Edit deterministisch erkannt (K6);
   Zweitanlauf: R-35-Warnung „Schon einmal abgelehnt: REJ-018…".
6. **Menschliche Entscheidung F5b:** Hauptkette: apply → REQ-89. Nebenketten wörtlich:
   reject „Wetter ist außerhalb des Produktkerns", reject „Abnahme-Testzeile — kein echter
   Bedarf", overwrite „…Core-Projektion soll gelten. Rest apply."
7. **Core-Wirkung F5b:** Hauptkette: **REQ-89** neu (14-Tage-Vorschlag) → PBI-047
   aktualisiert. Nebenketten: REQ-90 (Team-Antwort), Wetter → REJ + ✕-Vermerk; REJ-Stand 17.
8. **Projektion F5b (je Teilpfad getrennt):** Hauptkette **erfüllt** — Issue #45 nach
   REQ-89/90 real neu geschrieben (Apply-Report `6e75fa`: updated:1 + NOTE_COMMENT #45).
   ✕-Vermerk auf #43: zunächst **failed** (R-62b, Lauf `b7747b`); der spätere erfolgreiche
   Vermerk-Write ist nur als „dritter Durchlauf" beschrieben — Run-Zuordnung **n. b.**
   FLAG_DRIFT→overwrite #43 **erfüllt** (Lauf `895d18`, status overwritten). System-Echo-
   Schutz (K5) erfüllt.
9. **Urteile F5b:** Einordnung **erfüllt** (Kommentar als Draft, nicht als Direkt-Mutation) ·
   Inhaltserhaltung **erfüllt laut Abnahme-Doc** (14-Tage-Qualifikation in REQ-89; als
   zitierte narrative Evidenz gekennzeichnet — eigener Feldvergleich der REQ-89-Fassung nicht
   durchgeführt) · Freigabebezug **erfüllt** (alle Übernahmen/Abweisungen menschlich, Zitate)
   · Herkunft **erfüllt** (REJ-018-Kette; Vermerk-Historie) · Projektion: Hauptkette
   **erfüllt**, Vermerk-Write-Run **n. b.** (kein Sammel-Grün für die ganze Serie).
10. **Abweichung/Grenze — STRIKT:** Belegt ist der KOMMENTAR-Eingang + die AK-Blockquote-
    Konvention + FLAG_DRIFT-mit-menschlichem-overwrite an EINEM Issue — KEINE allgemeine
    Issue-Feld-Änderungs- oder Drift-Überwachung (Whitespace-Fall #7 = bewusster
    Nicht-Auslöser). Abweichungen: NOTE_COMMENT #43 zunächst stumm/failed (R-62/R-62b) ·
    für 131538 fehlt die per-Item-Gate-Datei (defer-all nur narrativ belegt) ·
    „Schon-geklärt"-Note (Fragen-Pendant zu R-35) ohne Live-Auslösungs-Beleg.

---

## Kompakte Ergebnistabelle (für 7.5; keine Quote — fünf gezielt gewählte FALLTYPEN;
Legende s. Kopf: n. b. / n. a. / nicht untersucht sind KEINE Erfüllung)

| Fall | Lauf (Stand) | Einordnung | Inhalt | Freigabe | Herkunft | Projektion |
| --- | --- | --- | --- | --- | --- | --- |
| F1 Neu | 765eea (18.08.) | erfüllt | erfüllt (verfolgte Ketten) | erfüllt (belegte Ops) | erfüllt | erfüllt (geprüfte Felder; real gh#43–45) |
| F2 Präzisierung | f23f06 (17.08.) | erfüllt | erfüllt | erfüllt | erfüllt | Kopplung erfüllt · Write n. b. |
| F3 Widerspruch | eb181a (04.08.) | erfüllt | erfüllt (Ablösung/Relationen) · PBI-Angleichung OFFEN | erfüllt (nur decision-gate interaktiv) | **abweichend** (Anzeige) | nicht untersucht (Dry-run) |
| F4 Wiederholung | a0b172 (18.08.) | erfüllt | **n. b.** (kein Text-Vorher/Nachher) | erfüllt | erfüllt | n. a. |
| F5a Frage | 765eea (+131538 Mechanik) | erfüllt | erfüllt | erfüllt | erfüllt | n. a. (kein Kanal, 9i) |
| F5b Rückweg | 20.08.-Serie | erfüllt | erfüllt (lt. Abnahme-Doc) | erfüllt | erfüllt | Hauptkette erfüllt · Vermerk-Run n. b. |

**Quer-Befunde (gehören in 7.5, nicht in die Limitations verbannt):** ① Experiment-Rollbacks
sind narrativ statt artefakt-belegt (F3; 131538) — ein dokumentierter Restore-Beleg fehlt als
Werkzeug. ② Herkunfts-ANZEIGE kann vom Herkunfts-FAKT abweichen (F3-origin-Prefill).
③ Offene Frage-DECs haben keinen Projektionskanal (bewusster Parkplatz 9i). ④ Der
execute-Umschalter im Resume hinterlässt kein eigenes Artefakt (F1). ⑤ Wiederholung ist nur
auf Delta-, nicht auf Transkript-Ebene geprüft (F4).
