# W2-Matchregeln — v01 (ENTWURF zur Fixierung durch den Autor)

> Status: MOMENTAUFNAHME 04.09.2026 — wird beim Gold-Freeze FIXIERT (Hash in `benchmark-checksums.txt`);
> danach unveränderlich. Protokoll-Kontext: `Thesis-Docs/Writing/W2-Evaluationskonzept-Kapitel-7.md`
> (§6.5, §8.1, §12.0, §27). Nach der Fixierung sind Änderungen an dieser Datei verboten —
> Grenzfälle werden IM Matchlog entschieden und begründet, nicht durch Regel-Nachbesserung.

## 0. Geltung

Gilt für den E2-Kernvergleich (Arme **F / L / LCR**, beide Benchmarkfälle, je Fall getrennt
ausgewertet) und definiert: Normalisierung auf den gemeinsamen Vertrag, Match-Stufen für
P1/P2, Split/Merge/Partial, die Typ-Diagnose, P3/P4-Urteilsregeln und das Protokoll.
**Zweistufiger Freeze (v01-Fix2, ehrliche Formulierung — Kollegen-P0):** ① Diese Regeln +
Gold-Leitfaden + die nummerierten Quellen werden VOR der primären menschlichen Validierung und
Finalisierung des Referenzbestands gehasht (soweit ein KI-gestützter Erstentwurf bereits zuvor
existierte, ist das über die Provenienzangabe der Gold-Datei + Threats offengelegt — der Hash
beweist, dass Validierung/Zweitannotation/Auswertung unter eingefrorenen Regeln stattfanden);
② nach Validierung/Zweitannotation + Gold-Adjudikation wird der finale Goldbestand gehasht
(Konzept §29). Danach ist beides unveränderlich. Der Umgang mit Fehlläufen/Aggregation ist NICHT Teil dieser Datei
(separater §27-Entscheid); festgehalten wird nur: eine Beobachtung ohne gültige Einreichung
liefert 0 Aussagen und wird als solche berichtet, nie stillschweigend wiederholt.

## 1. Einheiten und Normalisierung (deterministisch, per Skript — nie per LLM)

- **Gold-Unit** = human-validierte und adjudizierte atomare deutsche Aussage (KI-gestützter
  Erstentwurf zulässig, sofern offengelegt — Leitfaden §1.2) + Typ
  (requirement | architecture | decision | open_question | risk) + ≥1 AU-Referenz.
- **Arm-Aussage** (gemeinsame Sicht): `statement` + `sourceUnitIds` + `type`. Die Selbst-Labels
  `derivation`/`uncertainty` existieren NUR bei F und reisen als Diagnosefelder mit — sie haben
  KEINEN Einfluss auf P1–P4 (P4 urteilt unabhängig davon, §7); für L/LCR gibt es kein
  Selbst-Label und daher keine Abbildungsregel.
- **★ v1.2 (05.09., pilot-motivierte Vertrags-Erweiterung — Konzept §11 F-v2): QUELLENBILANZ.**
  Beide Konfigurationen liefern je AU genau eine Disposition (`used`→Claim-IDs ·
  `non_relevant` · `unresolved`); bei F Pflicht-Abgabeteil (deterministisch validiert), beim
  Ledger nativ (Unit-Bilanz + Miss-Signal). Die Bilanz beeinflusst P1–P4 NICHT; aus ihr werden
  symmetrisch bewertet: **Detection Precision/Recall** (F-`unresolved` bzw. Ledger-Miss-Signal
  gegen adjudizierte Gold-Misses) und **Silent-Miss-Rate** (Gold-Misses ohne jedes Signal).
  Der offizielle F ist der missionsgleiche v2 (Pilot-F = nur Pilotmaterial, fließt NIE in
  offizielle Aggregate).
  - **F:** direkt aus `output.json`.
  - **L:** `entries[].proposition` + `entries[].sourceUnitIds` aus `capture/l-machine.json`.
  - **LCR:** `entries[].entry.proposition` + `…sourceUnitIds` aus `capture/lcr-machine.json`.
    **Nur die Claims zählen für P1/P2.** Die Miss-Signal-Flags (missing_claim/needs_human/attach)
    sind KEINE Aussagen — sie gehen in die separate **Detektionsdiagnose**
    (treffen die `missing_claim`-Flags Gold-Units, die L verfehlt hat?).
  - *(Die capture-Pfade sind CODE-HART seit 04.09.: `ledger-capture`/`LedgerCaptureRunner` +
    `LedgerCaptureTests`, Live-Beleg Lauf `20260821_100215_beef4a` — kein spekulativer Vertrag.)*
- **Duplikate innerhalb eines Arms:** decken dieselbe Gold-Unit nur EINMAL (P1); für P2 wird
  jede Aussage einzeln beurteilt (redundant-korrekt bleibt gold-gematcht); Duplicate Rate
  wird separat berichtet.

## 2. Match-Stufen (Kern von P1/P2)

Beurteilt wird der **fachliche Sachverhalt**, nicht der Wortlaut (Paraphrase erlaubt):

- **voll:** Träger/Gegenstand + Kerninhalt + alle WESENTLICHEN Qualifikationen stimmen überein
  (Bedingungen, Negationen, Zahlwerte wie „maximal fünf", Verpflichtungsgrad sinngemäß).
- **teilweise:** Kern getroffen, aber eine wesentliche Qualifikation fehlt oder weicht ab
  (z. B. „Sofortinfo begrenzen" ohne die Fünfer-Grenze; falscher Zahlwert ⇒ höchstens teilweise).
- **kein Match** sonst. **Falsche/fehlende Negation ⇒ kein Match** (Umkehrung des Sinns).

## 3. Split / Merge / Partial

**★ Asymmetrie-Grundsatz (v01-Fix, Kollegen-MUST): P1 darf Arm-Aussagen bündeln — P2 beurteilt
jede Output-Aussage EINZELN.** Sonst ließe sich P2 durch fragmentierte oder überbreite Aussagen
künstlich optimieren.

- **Split (eine Arm-Aussage deckt mehrere Gold-Units):** Für P1 gilt jede voll ausgedrückte
  Gold-Unit als gedeckt. Für P2 zählt die Aussage als Treffer NUR, wenn ihr GESAMTER fachlicher
  Inhalt durch Gold gedeckt ist (Gold-Anteile + Nicht-Gold-Zusatz gemischt ⇒ höchstens
  teilweise). Der Anteil solcher Sammelaussagen wird als **Atomaritäts-Diagnose** berichtet.
- **Merge (mehrere Arm-Aussagen decken zusammen eine Gold-Unit):** Für P1 voll, wenn ihre
  VEREINIGUNG alle wesentlichen Bestandteile widerspruchsfrei ausdrückt. Für P2 wird jede
  beteiligte Aussage einzeln gestuft — eine nur im Verbund vollständige Teilaussage
  (z. B. „Einträge werden automatisch gelöscht" ohne die Sieben-Tage-Frist) bleibt für P2
  **teilweise**.
- **Mehrfachdeckung:** eine Gold-Unit zählt in P1 genau einmal.

## 4. Metriken aus den Match-Stufen

- **P1 Gold-Coverage (primär, strict):** voll gedeckte Gold-Units / alle Gold-Units.
  Sekundär **lenient**: (voll + teilweise) / alle.
- **P2 Gold-Precision (primär, strict):** einzeln voll gold-gematchte Aussagen / alle Aussagen
  des Arms (Einzelbeurteilung je Aussage — §3-Asymmetrie). Sekundär lenient analog.
- **`derived` ohne Gold-Gegenstück (v01-Fix, Kollegen-P0):** Es gibt KEINE „zulässige
  Ableitung" als P2-Treffer-Kategorie — jede Aussage trifft P2 nur über echten Gold-Match
  ihres Inhalts. (P2 fragt: „gehört der Inhalt zum eingefrorenen Referenzbestand?" ·
  P4 fragt: „trägt die Quelle den Inhalt?")
- **gold_escape-Taxonomie (arm-übergreifend — auch L/LCR können Gestütztes außerhalb des
  Golds erzeugen):** `gold_escape` = kein Gold-Match · `supported_gold_escape` = kein
  Gold-Match, aber P4-gestützt · `supported_derived_gold_escape` = optionaler F-Untertyp,
  wenn F zusätzlich selbst `derived` gesetzt hat.
- **„Korrekt, aber nicht im Gold":** eine sachlich richtige, belegte Aussage ohne
  Gold-Gegenstück zählt in P2 NICHT als Treffer (das Gold bleibt nach dem Hash unveränderlich,
  §29) — sie wird im Matchlog als `gold_escape` markiert; **die absolute Zahl und Rate der
  gold_escape-Fälle wird IMMER mitberichtet** (Leser können beurteilen, ob niedrige
  Gold-Precision teils Gold-Unvollständigkeit spiegelt; fließt in die Threats-Diskussion,
  nie in die Metrik).

## 5. Typ-Diagnose (SEKUNDÄR — fließt NIE in P1/P2 ein)

Begründung: R-8 (Feinfacetten ohne maschinellen Konsumenten) + 9h③ (Dispositions-Rauschen) —
Facettenqualität ist ausdrücklich NICHT Gegenstand von P1–P4.

- Typ-Übereinstimmung wird nur auf **voll**-Matches ausgewertet.
- **F:** `type` direkt. **L/LCR:** Abbildung AUSSCHLIESSLICH aus dem groben Feld `kind` —
  NIE aus status/modality/timeScope/disposition:

| Ledger-`kind` | gemappter Typ |
| --- | --- |
| requirement · non_functional_requirement · open_requirement · scope | requirement |
| constraint · process_constraint | architecture |
| open_question | open_question |
| risk | risk |
| *(künftige/unbekannte Werte)* | „nicht zuordenbar" — LAUT ausgewiesen, kein stiller Ausschluss |

- **Gold-Typ `decision`:** liegt außerhalb des Ledger-`kind`-Vokabulars (Entscheidungen tragen
  dort kein eigenes kind). Bei L/LCR wird der Typ-Vergleich für decision-Gold-Units als
  **„außerhalb Vokabular" (n/a)** geführt — weder Treffer noch Fehler, Anzahl ausgewiesen.
  Für F normal auswertbar. (Ehrliche Asymmetrie statt Facetten-Raterei über `status`.)

## 6. P3 Reference Validity + Reference Presence (deterministisch, Skript)

- **Presence** (Wächter): Anteil Aussagen mit ≥1 Referenz (harness-erzwungen; trotzdem berichtet).
- **P3:** Anteil aller abgegebenen Referenzen, die auf eine EXISTIERENDE AU-ID der
  `source-units.json` des jeweiligen Laufs zeigen.

## 7. P4 Semantic Support (Evaluator-Adjudikation — AUSSAGENEBENE, v01-Fix Kollegen-P0)

**Einheit ist die AUSSAGE, nicht die Einzelreferenz** (Konzept §8.1 v5 — dort angeglichen):
Beurteilt wird je Aussage die **VEREINIGUNG ihrer technisch auflösbaren referenzierten
Source-Units** — eine Aussage kann erst durch mehrere Units gemeinsam gestützt sein.
P3 bleibt dagegen rein referenzbezogen (existiert jede einzelne Referenz technisch?).

- **Vier Evaluator-Stufen, UNABHÄNGIG vom Selbst-Label des Arms:**
  `directly_supported` (Inhalt in den Units gesagt/sinngemäß) · `inferentially_supported`
  (folgt aus den zitierten Units OHNE zusätzliche domänenspezifische Annahmen nachvollziehbar —
  bloße Plausibilität genügt NICHT) · `partially_supported` · `unsupported`.
- **P4 = (directly + inferentially supported) Aussagen / Aussagen mit ≥1 auflösbarer Referenz**
  (partially separat berichtet).
- Das `derivation`-Selbst-Label (nur F) wird SEKUNDÄR gegen das Evaluator-Urteil gestellt
  (Derivation Accuracy als Diagnose) — kein Arm kann sich per „derived"-Etikett einen
  weicheren Maßstab geben.
- **Deterministische Wiederverwendung:** identisches Paar (statement + sourceUnitIds, gehasht)
  über Beobachtungen hinweg erhält EIN Urteil (Konsistenz + Aufwand).

## 8. Verfahren und Protokoll

1. Matching beginnt erst NACH dem Gold-/Regel-Hash; Beobachtungen werden in alternierender,
   vorab festgelegter Reihenfolge bewertet.
2. **Verblindung soweit möglich:** die Aussagen-Listen werden dem Evaluator ohne Arm-Etikett
   präsentiert (das Normalisierungs-Skript erzeugt anonymisierte, gemischte Sichten).
3. **Matchlog-Pflicht:** jede nicht-triviale Entscheidung (teilweise, Merge, gold_escape,
   Grenzfall) mit Ein-Satz-Begründung in `matchlog.jsonl` je Fall — der Matchlog ist
   Thesis-Beleg.
4. **Rollenklarheit:** Alle Urteile hier sind **Evaluator-Adjudikation** (Messung) — begrifflich
   strikt getrennt von der Ledger-Adjudikation (System-HITL, §12.0-4).
5. **Zweitblick:** der menschliche Zweitannotator prüft mindestens 10 der als Grenzfall
   markierten Match-Entscheidungen (alle, falls weniger); Uneinigkeiten werden protokolliert.
