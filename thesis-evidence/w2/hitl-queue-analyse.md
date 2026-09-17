# HITL-Queue-Analyse des Stresslaufs (Übergabe · Anzeige · Ergebnis-Mechanik)

> Status: MOMENTAUFNAHME 08.09.2026 — teilweise ÜBERHOLT durch die Nachreview-Konsolidierung
> (runs/w2-nachreview-konsolidierung.json + Anhang D.8): §6-Obergrenze und die 26er-„grundsätzlich
> unerreichbar"-Lesart gelten nicht mehr (Queue-Zuordnung neu: 29; Szenario 76,1 % als bedingte
> Rechnung; Claim-Review-Zitate = zweiter Kanal). §0–§5 (Queue-Mechanik/Erzeugung) und §7
> (already_covered-Nachprüfung + Entscheidungsort) bleiben gültig.

> Status: ERSTELLT 08.09.2026 — Prüfung von Queue-Erzeugung, tatsächlicher Anzeige und
> Adjudikationsartefakten des Stresslaufs `20260905_164650_8189ea` (Auftrag: Kollegen-Analyse
> „50,5 % sind der maschinelle Zwischenstand — was passiert an der HITL-Stufe?").
> Beleg: deterministisch erzeugte ECHTE Queue in `thesis-evidence/w2/hitl-queue-8189ea/`
> (queue.json + queue.md; CLI `ledger-adjudicate`, kein LLM, historischer Run unangetastet).

## 0 · Ausgangsbefund

Im Stresslauf existieren KEINE Adjudikationsartefakte — der Pilot endete am Pre-Gate-Capture
(Messpunkt der 50,5 %). Die Queue-Erzeugung ist jedoch reine deterministische Transformation
(`LedgerAdjudicationAdapter.Build`, kein LLM) und wurde für diese Analyse real ausgeführt.

## 1 · Übergabe: Was gelangt tatsächlich in die Queue?

**45 Items**, aus zwei Quellen:
- **18 `review_required_claim`** — die von der Facetten-Validierung beanstandeten Claims
  (mit Facet-Repair-Vorschlägen).
- **27 `unit_signal`** — Miss-Signale mit Verdikt `attach_as_evidence` (24), `needs_human` (2),
  `missing_claim` (1). **Die 21 `already_covered_indirectly`-Signale werden bewusst NICHT
  aufgenommen** („bereits aufgelöst; kein Handlungsbedarf" — Code-Kommentar).

## 2 · Exposition der 54 Coverage-Lücken in der echten Queue

| Klasse | n | IDs / Charakter |
| --- | --- | --- |
| In der Queue mit **explizitem Fehlend-Vorschlag** (missing_claim inkl. suggestedProposition) | **2 (+1)** | G-IE-088/089 direkt; G-IE-028 über die dokumentierte semantische Zuordnung desselben Items (AU-0124) |
| In der Queue als **Anheft-/Prüf-Vorschlag** (attach/needs_human) | **25** | der Mensch sieht die Unit-BEGRÜNDUNG (reason) — **0 von 24 attach-Items tragen einen eigenen Vorschlagstext**; der fehlende Inhalt ist im Kontext enthalten, nicht ausdrücklich beanstandet |
| **Fällt aus der Queue**, weil das einzige zugeordnete Signal `already_covered` ist | **8** | G-IE-006, 014, 030, 031, 032, 048, 061, 108 — diese zählten in der 36er-„Zuordenbarkeit" mit! |
| Ohne jedes Signal (still) | **18** | wie gemessen |

**Kernbefund:** „36 zuordenbar" ≠ „36 vorgelegt". Tatsächlich in der Queue vertreten sind
**28 von 54 Lücken** — und nur **3 davon mit ausdrücklicher Fehlend-Beanstandung samt
Formulierungsvorschlag**. Der bewusste already_covered-Ausschluss kostet 8 der 36
zuordenbaren Lücken ihre Vorlage (Design-Entscheidung mit messbarem Preis). Die
Adressierbarkeit 0,835 beschreibt die Zuordnungs-, nicht die Vorlage-Ebene; die
Vorlage-Ebene liegt bei (55+28)/109 = **0,761**, die explizite Beanstandungs-Ebene bei
(55+3)/109 = **0,532**.

## 3 · Anzeige: Was sieht der Mensch?

- `unit_signal`: Unit-ID, Verdikt als System-Vorschlag, Begründung (reason), bei
  missing_claim zusätzlich der fertige Claim-Vorschlag, verwandte Claim-IDs als Referenzen.
- `review_required_claim`: Claim-Text, wörtliche Evidenz-Zitate, Facet-Repair-Vorschläge
  (beanstandet sind FACETTEN, nicht fehlende Inhalte — Inhaltslücken dort höchstens implizit
  im Kontext sichtbar).

## 4 · Ergebnis-Mechanik: Welche Ergänzungen erlaubt der reguläre Ablauf?

Aktionen je Item: `accept_gap` (neuer Claim), `promote_to_claim` (Unit trotz
attach-Vorschlag als EIGENER Claim — der Override, der aus einem Kontext-Item einen
Coverage-Gewinn machen kann), `attach_evidence` (Evidenz anheften — **schließt KEINE
Inhaltslücke**, kein neuer Claim), `merge_existing`/`mark_covered_by` (nur Audit),
`reject`, `apply_repair` (Facette), `defer`. Apply persistiert `adjudicated`/`consumable`
(`ledger-adjudicate-apply`). Coverage-Steigerung im regulären Ablauf ist damit für die 28
vorgelegten Lücken MÖGLICH (via accept_gap/promote_to_claim), für die übrigen 26 nicht.

## 5 · Konsequenzen

1. **Für den Kapiteltext (unabhängig vom weiteren Vorgehen):** Die Vorlage-Statistik
   (28/54 vorgelegt · 3 explizit beanstandet · 8 durch already_covered-Filter
   ausgeschlossen) gehört als zusätzliche Ebene neben die 36er-Zuordenbarkeit — sonst bleibt
   die Lücke zwischen „zuordenbar" und „vorgelegt" offen. Queue-Artefakt = Beleg.
2. **Für den Vorher-Nachher-Nachweis (Entscheid offen):**
   - **Option A — reale Post-HITL-Messung:** EINE menschliche Adjudikations-Session auf der
     erzeugten 45er-Queue (UI vorhanden), Apply, dann Neubewertung des Post-HITL-Bestands
     gegen Gold v2. Beantwortet die offene Frage direkt am selben Lauf. ⚠ Verzerrungsrisiko:
     Der Autor KENNT inzwischen das Gold — entweder dritte Person bedienen lassen oder die
     Gold-Kenntnis als harte Grenze der Messung ausweisen; Regeln vorab dokumentieren.
   - **Option B — nur berichten:** Vorlage-Statistik als Ergebnis aufnehmen, Post-HITL-Frage
     explizit offen lassen (Kapitel 8/Ausblick). Kein neuer Aufwand, ehrlich, aber die
     Kollegen-Frage bleibt unbeantwortet.

## 6 · „Perfekter-Mensch"-Obergrenze: Was wäre maximal schließbar? (08.09.)

Annahme: Der Mensch beantwortet alle 45 Queue-Items perfekt — d. h. er liest zu jedem
Signal-Item die Quell-Unit und formuliert per accept_gap/promote_to_claim einen vollwertigen
Claim (nur das eine missing_claim-Item liefert den Text fertig; bei den 25 attach/needs_human-
Items muss der Mensch selbst formulieren).

| Klasse | n | Wirkung |
| --- | --- | --- |
| **A: Queue-schließbar** (Signal-Unit trägt den fehlenden Inhalt) | **28** (12 partial + 16 none) | → bei perfekter Antwort full |
| **B: NIE vorgelegt — already_covered-Filter** (einziges Signal ausgefiltert) | **8** (7 partial + 1 none: G-IE-006, 014, 030, 031, 032, 048, 061, 108) | unerreichbar im regulären Ablauf |
| **C: NIE vorgelegt — Units verwendet, kein Signal** (intra-unit-Verlust) | **18** (11 partial + 7 none: G-IE-011, 020, 021, 023, 025, 037, 038, 043, 066, 069, 079, 080, 085, 086, 087, 099, 105, 109) | unerreichbar; enthält die beiden Kanonisierungs-Verluste (021/023) |

**Obergrenze bei perfekter Bearbeitung: Full Coverage 83/109 = 0,761 (von 0,505) ·
Touched 101/109 = 0,927.** 26 Referenzaussagen (23,9 %) bleiben im regulären Ablauf
GRUNDSÄTZLICH unerreichbar — nicht wegen menschlicher Fehler, sondern weil die Vorlage-
Mechanik nur unverwendete Units kennt (C) bzw. relevante Signale herausfiltert (B).
Vollständige Einzel-Listen ALLER 109 Referenzaussagen mit Statement, v2-Beleg und Weg:
**`hitl-lueckenkarte-8189ea.md`** (Listen A–D; Basis = Queue-Artefakt + v2-Urteilsblatt +
01d-Signale + Claim-Bestand, deterministisch abgeleitet).

## 7 · Nachprüfung der 8 „already_covered"-Fälle + Entscheidungsort (08.09.)

**Wer entscheidet, wo:** Das Verdikt fällt der LLM-Agent `UnusedUnitLedgerComparer`
(Stufe 01d, unused-unit-ledger-compare). Prompt-Maßstab: „Wenn die Unit VOLL semantisch
enthalten ist: already_covered_indirectly; wenn sie nur zusätzliches Detail für einen
bestehenden Claim ist: attach_as_evidence; fehlt eine eigenständige Aussage: missing_claim;
unklar: needs_human" — mit Referenzpflicht (Deckung nie ohne Kandidaten-ID; deterministisch
nachgeprüft und sonst zu needs_human downgegradet, UnusedUnitCompareRepair). Der FILTER ist
dagegen deterministischer Code: `LedgerAdjudicationAdapter.DecidableCompareVerdicts`
nimmt nur missing_claim/needs_human/attach in die Queue („bereits aufgelöst; kein
Handlungsbedarf").

**Sind die 8 wirklich voll gedeckt? Nein — Einzelprüfung gegen die v2-Belege:**

| Fall | LLM-Begründung (Kurz) | Tatsächlich |
| --- | --- | --- |
| G-IE-006 ← AU-0023 | „schnelles Abrufen … bereits abgedeckt" (REQ-007) | REQ-007 trägt nur „nachschauen" — GENAU die behauptete Qualifikation (jederzeit/schnell) fehlt |
| G-IE-014 ← AU-0033 | „Screens/Hinzufügen enthalten" | Kern ✓; Plus-Button, Position, Dialog fehlen |
| G-IE-030/031/032 ← AU-0125 | „Infobox, Timeline, Plus-Button, neueste-oben enthalten" | Kern ✓; ganz-oben, Feed/scrollbar, Position-unten+Dialog fehlen |
| G-IE-048 ← AU-0084 | „entspricht der Profilsuche" | Suchleiste ✓; Suchkriterien (Name/Profildaten) fehlen |
| G-IE-061 ← AU-0086 | „durch Hilfe/Tutorial abgedeckt" | Hilfebereich ✓; die OFFENE Teilfrage (Eingabe-/Such-Erläuterung) fehlt |
| G-IE-108 ← AU-0136 | „…Branding … bereits abgedeckt" | **faktisch falsch**: KEIN Kandidat trägt Branding (v2: none) — die Begründung listet Nicht-Existentes als abgedeckt |

**Muster:** In 7 von 8 Fällen ist das Urteil KERN-wahr, aber am Prompt-Maßstab („VOLL
semantisch enthalten") falsch — korrekt wäre `attach_as_evidence` gewesen, was die Fälle in
die Queue gebracht hätte; in einem Fall (108) ist es schlicht falsch. Die unsichtbare Lücke
entsteht also aus der Kette **LLM-Fehlklassifikation (voll statt teilweise) × deterministischer
already_covered-Filter**. Zwei benennbare Stellschrauben (Kapitel-8-Material, nicht gebaut):
Prompt-Maßstab schärfen („voll = einschließlich aller Qualifikationen") oder Filter-Politik
ändern (already_covered nachrangig anzeigen statt verwerfen).
