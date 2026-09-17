## R1 — Referenz-Ledger-Annotations-Tooling (Completeness/Recall vorbereiten) — 2026-06-30

**Warum (Entscheidung).** Frage „fehlen wirklich Sachen aus dem Transkript?" = Ledger-vs-Transkript-
Completeness (open-world, **nicht valide messbar ohne Referenz**, smallVersion.md §Auslöser 2). Lösung ist
NICHT sofort die Gap-Audit-Maschine (deren Netto-Nutzen ist ohne Referenz nicht belegbar), sondern zuerst
ein **hand-vollständiger Referenz-Ledger für ein Transkript** (= die Messlatte). Diskussion: ein LLM darf
diese Referenz NICHT allein bauen (Zirkularität/geteilter blinder Fleck; „kein LLM ist Ground Truth",
grounding-spotcheck). Arbeitsteilung: **Tool reicht die Last (Segmentierung + Kandidaten-Zuordnung), der
Autor adjudiziert** das Vollständigkeits-Urteil pro Segment. Status der fertigen Referenz =
„modell-entworfen, autor-adjudiziert", Single-Labeler (E5) — wie die bestehenden Fixtures.

**Gebaut (deterministisch, KEIN LLM — bewusst, damit keine Modell-Blindflecken ins Hilfsmittel kommen):**
```text
AgenticSdlc.Host/Phases/Phase2/Ledger/LedgerReferenceTemplateRunner.cs
Program.cs   Dispatch `ledger-reference-template <transcript.txt> <candidate-ledger.json> [out.md]`
```
Wiederverwendet `TranscriptSegmenter` (Sprecher-Turns) + Evidence-Quote-Fragment-Substring-Matching
(Speaker-Prefix strippen, auf „..." splitten, Fragmente ≥20/≥10 Zeichen gegen normalisierten Turn-Text).
Segmente OHNE Kandidat werden als „⚠ KEIN KANDIDAT" markiert (Haupt-Miss-Verdacht).

**Run (Interview, vorhandener Kandidaten-Ledger — kein neuer LLM-Call):**
```text
dotnet run --project AgenticSdlc.Host -- ledger-reference-template \
  input/transcripts/Interview-Einrichtung.txt \
  thesis-evidence/evidence-first-spike/semantic-ledger-extract.Interview-Einrichtung.openai_gpt-5.4.extracted.json
-> segments=136 candidates=60 aligned=60 orphans=0 emptySegments=66
-> input/eval-labels/Interview-Einrichtung.reference-template.md
```

**Beobachtung.** Matching trifft sauber: alle 60 Kandidaten zugeordnet, **0 Orphans**; Mehrsegment-Claims
(z. B. L003/L004) erscheinen unter allen relevanten Turns; konversationelle Turns korrekt als „kein
Kandidat" markiert. 136 Segmente / 66 ohne Kandidat = bounded, aber substanzielle Autor-Aufgabe.

**Learning.** Die Last-vs-Urteil-Trennung ist umgesetzt: das Tool liefert eine reproduzierbare, segment-
weise Adjudikations-Vorlage ohne Modell-Urteil; der wissenschaftliche Wert entsteht erst durch die echte
Autor-Adjudikation (nicht Durchwinken — sonst zurück zur Zirkularität).

**Nächster Schritt.** (1) Autor adjudiziert `Interview-Einrichtung.reference-template.md` segmentweise
(MISS/NOISE). (2) Daraus die Referenz-JSON assemblieren (behaltene Kandidaten + neue Miss-Einträge mit
Evidence/Facetten). (3) Tool 2: Recall des Auto-Ledgers gegen die Referenz messen = erste echte (B)-Zahl.

---

## R2 — Recall-Messung (Tool 2) + erste NICHT-zirkuläre (B)-Zahl — 2026-06-30

**Warum so (Validität).** Eine komplett modellgebaute Voll-Referenz wäre zirkulär (LLM-gegen-LLM,
geteilte Blindflecken) + birgt Verbatim-Zitat-Fehler. Daher zuerst die **nicht-zirkuläre** Messung gegen
die bereits **autor-bestätigte** 11er-Fixture (`semantic-ledger-interview-spike.json`, E5).

**Gebaut:** `AgenticSdlc.Host/Phases/Phase2/Ledger/LedgerReferenceRecallRunner.cs` (Befehl
`ledger-reference-recall <reference.json> <auto-ledger.json> [model] [out.json]`); Dispatch in Program.cs.
Matcht jeden Referenz-Eintrag via `SemanticLedgerRecallMatcher` gegen den Auto-Ledger; protokolliert
`referenceStatus` (autor-bestätigt = nicht-zirkulär vs. modell-entworfen = vorläufig). Build grün.

**Run (Sorte A):**
```text
dotnet run --project AgenticSdlc.Host -- ledger-reference-recall \
  input/eval-labels/semantic-ledger-interview-spike.json \
  thesis-evidence/evidence-first-spike/semantic-ledger-extract.Interview-Einrichtung.openai_gpt-5.4.extracted.json \
  openai/gpt-5.4
-> exact 8/11 · partial 3/11 · missed 0/11 · recall(exact-or-partial) = 1.0
-> thesis-evidence/evidence-first-spike/ledger-reference-recall.semantic-ledger-interview-spike.json
referenceStatus: author-confirmed (non-circular)
```

**Befund.** Gegen die autor-bestätigten **kritischen** Claims **missed 0/11** — der Auto-Ledger (60
Kandidaten) findet alle. Die 3 `partial` sind **Facetten-/Scope-Abweichungen** (Kalender „optional/später"
statt „decided eigener Bereich" L019; Medikation über L019+L021 verteilt; Notify-Scope enger L039), KEINE
Recall-Misses.

**Interpretation (ehrlich, Grenzen).**
- **Für die messbare (kritische) Teilmenge ist Recall NICHT der Engpass** — Facetten sind es. Deckt sich mit
  L0/L1 und der gesamten Evidenzkette (Status/Modalität/Disposition = die harte Wand).
- **NICHT belegt:** Voll-Completeness (open-world). Diese Zahl sagt nichts über *nicht-kritische/implizite*
  relevante Claims, die weder Autor noch Modell markiert haben. Dafür bräuchte es die Voll-Referenz (3).
- Referenz = autor-korrigierter Single-Labeler (E5); Matcher = LLM (temp 0) → Verdikt-Rauschen möglich, aber
  `missed 0` mit Pro-Eintrag-Begründung ist belastbar. Auto-Ledger = Kandidaten (60, breiteste Recall-Fläche).

**Folge für die Roadmap.** Das **senkt die Dringlichkeit der Voll-Referenz (3)** für die Recall-Frage:
kritischer Recall ist auf diesem Transkript voll. Der Hebel bleibt **Facetten (L2/L3)**. (3) misst weiterhin
den breiten/nicht-kritischen Recall und bleibt das offene, sauber begrenzte Item (Reaktivierungsbedingung
smallVersion.md) — aber nicht der nächste Pflichtschritt.

**Nächster Schritt.** L2 (Cluster-Trace + ResponseFormat im Canonicalizer) → L3 (FacetValidationExecutor,
trifft die `partial`-Ursache). (3) optional/später.

---

## R3 — Deterministischer Recall-Screen (kein LLM) + breite (B)-Zahl gegen 99er-Referenz — 2026-06-30

**Warum.** Die LLM-Variante (R2) brauchte 1 gpt-5.4-Call PRO Eintrag → 99×LLM = zu teuer/langsam
(abgebrochen). Für die reine **Recall**-Frage ist der reiche Facetten-Matcher Overkill. Beide Ledger
zitieren aus DEMSELBEN Transkript → Recall ist deterministisch über **Segment-Overlap** messbar (gratis).

**Gebaut:** `AgenticSdlc.Host/Phases/Phase2/Ledger/LedgerReferenceRecallFastRunner.cs` (Befehl
`ledger-reference-recall-fast <transcript> <reference> <auto-ledger> [out]`), KEIN LLM. Verankert jeden
Eintrag via Evidence-Fragment-Substring an Turns (TranscriptSegmenter); Referenz-Eintrag „covered", wenn ein
Auto-Eintrag ein Segment teilt. Build grün.

**Run (Sorte A, gratis, sofort):**
```text
dotnet run -- ledger-reference-recall-fast input/transcripts/Interview-Einrichtung.txt \
  input/eval-labels/Interview-Einrichtung.reference-ledger.json \
  thesis-evidence/evidence-first-spike/semantic-ledger-extract.Interview-Einrichtung.openai_gpt-5.4.extracted.json
-> reference=99 auto=60 ; covered(Obergrenze)=76/99 · missed(verlässlich)=23/99 · recall ≤ 0.7677
-> thesis-evidence/evidence-first-spike/ledger-reference-recall-fast.Interview-Einrichtung.reference-ledger.json
(noSegRef=0 -> alle 99 Referenz-Einträge an Segmente verankert; die 23 Misses sind echt, kein Match-Artefakt)
```

**Methodik/Grenze (ehrlich).** `covered` = OBERGRENZE (Segment-Overlap überschätzt: ein Segment kann
mehrere Claims enthalten). `missed` = verlässliche Untergrenze (kein Auto-Eintrag im Segment). Keine
Facettenbewertung (das ist L3). Referenz = `Interview-Einrichtung.reference-ledger.json` (99, sehr granular;
Autorschaft/Granularität noch zu bestätigen → falls modell-erzeugt: Zahl vorläufig).

**Befund.** Breiter Recall ≤ 77 % (≥ 23/99 Miss) — deutlich unter der kritischen 11er-Teilmenge (R2: 1.0).
Die 23 Misses sind **echte relevante Claims** und clustern: (a) **UI-/Design-Details** der Design-Session
(Login-Layout, Profil-Kacheln, Symbole, Kalender-Icons, Barrierefreiheit, Appbar — fast die ganze
„8. Design-Ausarbeitung") und (b) **Kontext/Prozess** (Projekt-Ursprung, tägl. Bedarf, Beobachtungstermin,
Dev-Setup, JetX). Fast alle `desired`/`must_note`/`must_consider`.

**VALIDITÄT (korrigiert 2026-06-30):** Die 99er-Referenz ist **modell-erzeugt** (NICHT autor-adjudiziert,
bestätigt durch User). Damit ist der Recall dagegen **zirkulär/vorläufig** (Modell-gegen-Modell, geteilte
Blindflecken) — **keine** gültige (B)-Ground-Truth-Zahl. (Hinweis: die referenceStatus-Heuristik im
LLM-Runner würde „reference-ledger.json" fälschlich als nicht-zirkulär labeln — Dateiname trügt.)

**Was die Zahl LEGITIM zeigt (Modell-vs-Modell):** Ein zweiter, granularerer Extraktions-Pass (99) findet
~23 plausible Claims, die der Produktions-Pass (60) nicht hat → die Extraktion ist **granularitäts-/
prompt-sensitiv**; ein einzelner Pass lässt Detail liegen (v. a. UI-Design + Kontext). Real, aber nicht
validiert. **Gültig bleibt nur R2** (kritischer Recall 11/11 gegen die autor-bestätigte Fixture).

**Methodisch wichtig (hält unabhängig):** Recall braucht KEINE teure LLM-Bewertung — deterministischer
Segment-Overlap reicht für die Miss-Untergrenze; LLM/Facetten nur auf kleiner Menge (L3).
