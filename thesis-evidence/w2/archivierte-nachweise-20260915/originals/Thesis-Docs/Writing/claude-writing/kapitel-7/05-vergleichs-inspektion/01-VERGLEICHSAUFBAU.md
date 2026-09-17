# Der Vergleich im Detail — was jede Seite tut, woran gemessen wurde

> Status: MOMENTAUFNAHME 07.09. — Inspektions-Dokument für die Fairness-Betrachtung des Autors.
> Alle referenzierten Dateien liegen als Kopie in DIESEM Ordner; Originale: `runs/`,
> `input/eval-labels/`, `AgenticSdlc.Host/`.

## A · Gemeinsame Grundlage (für beide Konfigurationen identisch)

- **Quelle:** dasselbe kanonisch nummerierte Transkript (`03-gold-und-regeln/*.numbered.txt`).
  Die deterministische Segmentierung (step-00) ist damit HERAUSKONTROLLIERT — kein Teil des
  gemessenen Unterschieds.
- **Modell/Parameter:** openai/gpt-5.4 via OpenRouter, temperature 0, Structured Output.
- **Fachlicher Auftrag:** konsolidierte, belegte, bilanzierte Evidenzbasis als Grundlage für
  Anforderungen/Backlog.
- **Messgrenze:** beide werden VOR menschlichem Eingriff gemessen (F: Einreichung; Ledger:
  LCR-Stand vor Adjudikations-Gate).

## B · Konfiguration F — der freie Agent

**Ein einzelner Agent, eine Mission, freie Arbeitsweise.** Prompt: `02-prompts-und-code/arm-f/ArmFAgent2.txt` (vollständig; Kern:)

- **Teil 1 — kanonischer Claim-Bestand:** vollständig · atomar · kanonisch (keine Duplikate,
  Qualifikationen erhalten) · belegt (sourceUnitIds) · typisiert · ehrlich
  (derivation/uncertainty).
- **Teil 2 — Pflicht-Quellenbilanz:** für JEDE AU genau ein Eintrag: `used` (mit claimIds) /
  `non_relevant` / `unresolved` („Sei hier EHRLICH — diese Liste ist der Wert deiner Bilanz").
- **Werkzeuge:** `read_transcript` (beliebig nachlesen) · `validate_draft` (deterministische
  Formprüfung: Bilanz lückenlos, jede AU genau einmal, used→existierende claimIds) ·
  `submit_result` (nur gültige Einreichung beendet; Submit-Once). Tool-Budget 40, Budget
  blockt nie das Einreichen. Code: `arm-f/ArmFTools.cs` (ValidateBilanz), `ArmFAgents.cs`.
- **Nicht vorgegeben:** Zerlegung, Reihenfolge, Prüfschritte, Anzahl Revisionen — „WIE du
  vorgehst, entscheidest du selbst."

## C · Konfiguration LCR — die mehrstufige Ledger-Erzeugung

Jede Stufe mit Zweck, Art und Code-/Prompt-Ort (alle Dateien in `02-prompts-und-code/ledger/`):

| Stufe | Zweck | Art | Datei (Prompt inline) |
| --- | --- | --- | --- |
| step-01 Kandidaten-Extraktion | alle fachlichen Aussagen als KANDIDATEN-Claims mit sourceUnitIds, wörtlichen Evidenz-ZITATEN, kind/status/modality/scope + Ziel-Dispositionen | LLM | `UnitAwareSemanticLedgerExtractor.cs` |
| step-01b Unit-Coverage | jede AU als verwendet/unbenutzt bilanzieren → Unused-Liste | deterministisch | (Gate-Logik, s. LEDGER-README) |
| step-01c Unused-Triage | unbenutzte Units klassifizieren: potenziell relevant vs. Rauschen | LLM | `UnusedUnitTriageReviewer.cs` |
| step-01d Unused-Compare | je potenziell relevante Unit gegen den Bestand urteilen: `missing_claim` / `needs_human` / `already_covered` / `attach_as_evidence` (+Vorschlags-Proposition) → das sind die MISS-SIGNALE | LLM | `UnusedUnitLedgerComparer.cs`, `UnusedUnitReviewer.cs` |
| step-01d Referenz-Reparatur | kaputte/halluzinierte Referenzen der Compare-Urteile deterministisch erkennen, reparieren oder EHRLICH zu needs_human downgraden (Audit-Spur) | det. + LLM-Repair | `UnusedUnitCompareRepair.cs` |
| step-02 Kanonisierung | Kandidaten → konsolidierte KANONISCHE Claims (Verdichtung, Duplikat-Zusammenführung) | LLM | `SemanticLedgerCanonicalizer.cs` |
| step-02 CanonicalCheck ⇄ Repair | deterministisch: ist JEDER Kandidat im kanonischen Bestand gedeckt? Bei Verlust: LLM-Repair mit wörtlichem Gate-Befund, Schleife mit Versuchslimit, Terminal LAUT | det. Check + LLM-Repair | `CanonicalCoverageRepairer.cs` |
| step-03 Facetten-Validierung | jeder Eintrag gegen das Transkript: Proposition gedeckt? Verstärkung (offen→decided, gewünscht→must)? Evidenz stützt? Dispositionen sinnvoll (open-questions=required NUR bei echter Offenheit)? | LLM (kleine Batches, fixer Nenner) | `FacetValidator.cs`, `FacetAssigner.cs` |
| Quality-Gate | formale Gesamtprüfung (Fehler=0, Traces intakt) | deterministisch | (Gate-Logik) |

**Messpunkte:** `L` = Maker-Stand nach Kanonisierung, VOR maschineller QC ·
`LCR` = nach QC (Facetten-Validierung + Miss-Signale + QC-Protokolle), VOR Mensch.
Empirischer Befund der Kampagne: L ≡ LCR auf Claim-Ebene (QC wirkte als Detektor).

## D · Aufgaben-Gegenüberstellung (wer erfüllt was womit?)

| Fachliche Aufgabe | F erfüllt sie durch … | Ledger erfüllt sie durch … |
| --- | --- | --- |
| Vollständig extrahieren | Mission Teil 1 (LLM, frei) | step-01 (LLM, spezialisiert) |
| Kanonisch konsolidieren | Mission Teil 1 „kanonisch/keine Duplikate" (LLM entscheidet selbst) | step-02 (eigene LLM-Stufe) + det. Verlust-Check |
| Belegen | sourceUnitIds (nur IDs!) | sourceUnitIds + wörtliche Evidenz-ZITATE |
| Quellen-Rechenschaft | Teil-2-Bilanz: LLM FÜLLT sie aus, det. Guard prüft nur die FORM | step-01b–01d: Code ZÄHLT unbenutzte Units, LLM klassifiziert sie → Signale entstehen strukturell |
| Selbstprüfung | `validate_draft` (nur formal) | CanonicalCheck (det., inhaltlicher Verlust) + FacetValidator (LLM, semantisch) |
| Reparatur | freie Selbstrevision (im Stressfall: 0–3 beobachtet) | Repair-Schleifen nur bei Gate-Fail, mit Limit + lautem Abbruch |

**Der Kern-Unterschied in einem Satz:** Bei F ist die Rechenschaft eine *Selbstauskunft des
Modells* (formal erzwungen, inhaltlich frei); beim Ledger ist sie eine *Buchführung des Codes*
(unbenutzte Units KÖNNEN nicht unbemerkt bleiben — nur ihre Klassifikation ist LLM).

## E · Woran gemessen wurde

- **Gold-Referenzbestand** (`03-gold-und-regeln/*.w2-gold.json`): Treue-Fall 31 / Stressfall
  108 atomare Propositionen mit sourceUnitIds; Entstehung: KI-Erstentwurf, autor-konsolidiert
  (Zweitannotation per Autor-Entscheid entfallen). Regeln gehasht (`benchmark-checksums.txt`).
- **Matchregeln** (`w2-matchregeln.md`): voll/teilweise/kein; Bündelung erlaubt (mehrere
  Claims → eine Proposition und umgekehrt); fehlende wesentliche Qualifikation = partial;
  falsche Negation/Gegenentscheidung = none; Quellenrichtigkeit zählt separat.
- **Urteilsverfahren:** KI-Adjudikation (jedes Urteil mit Beleg) → regelgebundener, nicht
  verblindeter KI-Verifikationsdurchgang (11 Kippungen, 90,9–100 % Übereinstimmung) →
  Autor-Stichprobe (Stressfall). Alle Einzelurteile: `06-messung/w2-official-*eval*.json`
  (Feld `goldUrteile`, Kippungen mit `korrektur`-Vermerk).
- **Metriken:** Full/Touched Coverage, Strict Precision, Referenzgültigkeit, semantische
  Stützung; Silent-Miss-Rate, Adressierbarkeit (Prüfpotenzial!), Signal-Präzision; Tokens.
  Ergebnisse konsolidiert: `06-messung/W2-ERGEBNISSE.md`; Chronik inkl. aller Korrekturen:
  `06-messung/w2-official-manifest.txt`.
- **Die vier bewerteten Läufe:** F Stressfall `20260906_143052_6540e2` · F Treue-Fall
  `20260906_141518_12d046` (Outputs: `04-output-arm-f/`) · Ledger Stressfall
  `20260905_164650_8189ea` · Ledger Treue-Fall `20260905_164206_e30646`
  (L-/LCR-Captures: `05-output-ledger/`).

## F · Fairness-Betrachtung: die ehrliche Asymmetrie-Liste

**Symmetrisch (by design):** gleiche Quelle/AUs · gleiches Modell/Parameter · gleicher
fachlicher Auftrag inkl. Konsolidierungs- UND Rechenschaftspflicht · gleiche Messgrenze (vor
Mensch) · gleiches Gold, gleiche Regeln, gleiches Urteilsverfahren · Auswahlregeln vorab
deklariert.

**Asymmetrien, die man beim Urteilen kennen muss (in BEIDE Richtungen):**

*Zu Lasten des Ledgers:*
1. **Das Gold ist atomar** — es honoriert Fs feingranulare Ausgabeform; die Verdichtung
   (Kanonisierungs-Auftrag) zahlt auf der Coverage-Metrik drauf. (Milderung: Bündel-Regel.)
2. **Unbewertete Mehrleistung:** Der Ledger produziert Dinge, die die Messung nicht
   honoriert — wörtliche Evidenz-Zitate, kind/status/modality/scope, Ziel-Dispositionen
   (Bestellzettel für nachgelagerte Artefakte), Facetten-Protokolle. F liefert davon nichts.
3. **Messgrenze vor HITL** schneidet dem Ledger seinen designierten Qualitätsschritt ab
   (Mensch hebt die Miss-Signale) — vorregistriert, aber real.

*Zu Lasten von F:*
4. **Ein Agent gegen eine Spezialisten-Kette:** Der Ledger nutzt ~1,7–2× Tokens und viele
   fokussierte Aufrufe; F muss alles in einer Mission leisten (Budget 40 Tools).
5. **Fs Bilanz ist nur formal erzwungen:** Der Guard prüft Lückenlosigkeit der FORM, nicht
   den Inhalt — F kann (und tat es) alles als „erledigt" bilanzieren. Das ist allerdings
   zugleich der Messgegenstand von Achse B, keine Benachteiligung der Messung.

*Neutral, aber wissenswert:*
6. F-Claims tragen KEINE Evidenz-Zitate (nur AU-IDs) — der spätere Traceability-Weg „bis zum
   wörtlichen Zitat" ist bei F prinzipiell kürzer belegt.
7. F-Fehlläufe (schwache Modelle) wurden je 1× wiederholt; Ledger-Läufe stammen aus der
   Pilot-Serie (chronologisch erster je Fall).

**Leitfragen für deine Prüfung:** ① Ist der fachliche Auftrag in beiden Prompts wirklich
äquivalent (B vs. C lesen)? ② Honoriert das Gold eine Seite strukturell (Stichprobe:
5 Gold-Propositionen gegen beide Outputs legen)? ③ Sind die Miss-Signale des Ledgers und
Fs `unresolved`-Liste WIRKLICH dasselbe Konstrukt (D letzte Zeile)? ④ Würdest du eine der
Asymmetrien 1–5 als entscheidend fürs Ergebnis einstufen — und steht sie in 7.12?
