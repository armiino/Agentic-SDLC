# Testplan — Mini-A/B: Ledger-Artefakt vs. Transkript-Artefakt

> **Zweck.** Wie wir *knapp und ehrlich* nachweisen, dass der Weg `Transkript → Ledger → Artefakt` (Arm B)
> nachvollziehbarere, evidenzgebundenere und leichter reviewbare Requirements liefert als der direkte Weg
> `Transkript → Artefakt` (Arm A). **Kein** großes kalibriertes Instrument, **keine** k-Läufe, **keine**
> Behauptung „der Ledger ist allgemein besser". Kontext: `ReusePlan.md`, Verlauf: `iteration-notes.md`.

---

## 1. Die scoped These (was genau bewiesen wird)

> Der Ledger **löst nicht jedes Review-Problem**, aber er verschiebt es in eine **strukturierte Evidenzschicht**:
> macht Entscheidungen explizit, reduziert freie Verstärkung (False Certainty) und erleichtert Review/Repair —
> **bei akzeptabler Coverage**.

Bewusst **eng**. Wir zeigen **nicht**:
- dass der Ledger *immer mehr Details* enthält (Coverage-Überlegenheit) — nur **akzeptable** Coverage (Sanity),
- eine statistisch signifikante Aussage (n=1 Artefaktpaar) — nur eine **deskriptive** Gegenüberstellung.

Das war die ursprüngliche Wurzel-Ursache: `Artefakt → Rohtranskript` war schwer zu prüfen, Grounding instabil,
Coverage schwer sauber zu bewerten. Genau *diese* Erleichterung weisen wir nach — nicht mehr.

---

## 2. Was verglichen wird

| | Arm A (Baseline) | Arm B (Ledger) |
|---|---|---|
| Weg | Transkript → requirements.md | Transkript → **Ledger** → requirements.md |
| Input | `input/transcripts/Interview-Einrichtung.txt` | `runs/ledger/20260704_131614_018865/step-03b-adjudicated/consumable.json` |
| Quellen-IDs im Artefakt | nein | ja (`[sourceClaimId]`) |
| Erzeugung | `agentPhase=phase2_evidence, source=transcript` | `… source=ledger` |

**Wichtig:** dasselbe Transkript beidseitig (der Ledger wurde daraus gebaut) — sonst kein valider Vergleich.
Beide Arme teilen sich denselben Prompt-**Kern** (`_shared-core.txt`); nur der Input-Header unterscheidet sich.

---

## 3. Voraussetzung: Arm B nutzt den Ledger VOLL (vor dem Test zu fixen)

Der aktuelle `ProjectLedger()` gibt eine reduzierte Sicht (id/Proposition/Facetten). Da der Nachweis den
*Ledger-Weg* bewertet, muss Arm B den Ledger **wie vorgesehen** sehen:

- [ ] **Projektion erweitern:** pro Claim zusätzlich `evidence` (Transkript-Zitat) + `notes` mitgeben.
- [ ] **ADJ-GAP-Propositionen fixen:** die 18 promoted-Claims tragen Meta-Propositionen
  (z. B. „Spezifiziert für die Videoseite …"); der konkrete Inhalt steckt in der Evidenz. Entweder im **Refine
  (A10)** eine saubere requirement-artige Proposition erzeugen, oder dem Agenten die Evidenz explizit als Basis geben.

> **Wichtig: Testmodus ≠ Produktionsmodus.** Im Testmodus wird **kein harter „cite-or-exclude"-Zwang** gesetzt,
> weil das Coverage per Prompt erzwingen und die Messung konfundieren würde. Im späteren Produktions-Agenten ist
> `cite-or-exclude` dagegen sinnvoll: Jeder relevante Claim sollte entweder zitiert oder bewusst ausgelassen werden.
> Für diesen Mini-A/B-Test bleibt es aber bei: Artefakt frei erzeugen lassen, danach Coverage/Traceability messen.
>
> Ebenfalls zurückgestellt → Kapitel C / Produktions-Agent: `optional`-Vollständigkeits-Handling,
> Manager/Repair-Loops und automatische Nachbesserung.

---

## 4. Der Test: Mini-A/B, tabellarisch

**Stichprobe:** 10–15 Anforderungen. Ziehung: die ersten ~15 Requirements aus **Arm B** (sie tragen IDs → prüfbar)
plus die thematisch entsprechenden aus **Arm A**. (Bei Bedarf zusätzlich 5 Arm-A-Aussagen, die in B **fehlen**,
für den Coverage-Sanity.)

**Bewertungstabelle (pro Anforderung):**

| # | Anforderung (Kurz) | Arm | Quellen-ID? | Quelle auffindbar? | False Certainty? | Anmerkung |
|---|---|---|---|---|---|---|
| 1 | … | A | nein | schwer | ja/nein | … |
| 1 | … | B | `[canon_…]` | 1 Klick | ja/nein | … |

**Aggregat** (die zitierbaren Zahlen):
```
                          Arm A      Arm B
Quellen-ID vorhanden       0/15      ~15/15
Quelle in <30s auffindbar  x/15       ~15/15
False Certainty (Fälle)    a          b        (b sollte << a)
wichtige Themen fehlen     (Sanity: keine grobe Lücke in B)
```

---

## 5. Wie jedes Kriterium gemessen wird (Methodik)

**K1 — Quellen-ID vorhanden?** *(Traceability, strukturell)*
- Deterministisch: enthält die Zeile ein `[id]`? Arm B ≈ 100 %, Arm A = 0 %. Reine Inspektion/Zählung.

**K2 — Quelle schnell auffindbar?** *(Reviewbarkeit)*
- Arm B zählt als **direkt auffindbar**, wenn die Requirement-Zeile eine Claim-ID hat und diese ID im
  `consumable.json` auf einen Claim mit `evidence`/`sourceUnitIds` führt. Reviewpfad:
  `[id] → Claim → evidence/sourceUnitIds → Transkript-Turn`.
- Arm A zählt nur dann als **direkt auffindbar**, wenn der Reviewer ohne längere Suche eine eindeutige
  Transkriptstelle nennen kann. Muss der Reviewer frei suchen/keyword-raten, zählt es als „Suche nötig".
- Optional als Zeitmaß: pro Aussage max. 30 Sekunden. Wenn die Quelle bis dahin nicht eindeutig gefunden ist:
  `nicht direkt auffindbar`.

**K3 — False Certainty (Verstärkung)?** *(der Wurzel-Ursache-Nachweis)*
- Arm B (fast deterministisch): Aussage zitiert `[id]` → Facetten des Claims nachschlagen. Überstärkt die
  Formulierung? (Claim `status=open`/`modality=desired`/`timeScope=…unclear`, aber Requirement sagt
  „muss/entschieden/MVP"). Ja = False-Certainty-Fall.
- Arm A: keine Facetten-Referenz → Aussage gegen das Transkript prüfen: sagt die Quelle „muss/entschieden", oder
  hat der Agent verstärkt? (10–15 Fälle → per Hand oder 1 LLM-Urteil pro Aussage; kein großes Instrument nötig).

**K4 — wichtige Themen fehlen?** *(Coverage-Sanity, KEINE Überlegenheitsaussage)*
- Nicht „wer hat mehr", sondern: übersieht Arm B etwas **offensichtlich Wichtiges**, das Arm A/das Transkript hat?
- Billig via Zitate: welche `requirements=required`-Claim-IDs des Ledgers werden in Arm B **nicht** zitiert?
  (deterministisch). Nur grobe Lücken zählen — Detail-Tiefe ist explizit NICHT das Kriterium.

> Instrument-Disziplin: **bounded pro Item** (jede Aussage genau ein Urteil), **nie** open-ended „finde alle
> Fehler" (das war die Varianz-Falle des alten Review-Prozesses). Coverage und Treue bleiben **getrennte** Spalten,
> werden **nicht** zu einem Score verrechnet.

---

## 6. Was der Test bewusst NICHT tut (Scope-Wächter)

- Kein kalibriertes NLI-Großinstrument, keine k=3–5-Verteilung (n=1 genügt für die *deskriptive* Illustration).
- Keine Coverage-Überlegenheits-Behauptung (nur Sanity).
- Kein Erzwingen von Vollständigkeit im Prompt (würde Coverage konfundieren).
- Kein Produktions-Agent (Multi-Agent/Repair) — das ist Kapitel C.

---

## 7. Erwartete Story (wie das Ergebnis formuliert wird)

Erwartet (Hypothese, nicht Vorwegnahme des Ergebnisses):
- **K1/K2:** Arm B strukturell traceable (jede Anforderung 1-Klick-belegbar), Arm A nicht → Review deutlich leichter.
- **K3:** Arm A zeigt mehr False-Certainty-Fälle (freie Verstärkung), Arm B bleibt an den adjudizierten Facetten.
- **K4:** Arm B ohne grobe Coverage-Lücke.

Ehrliche Formulierung (auch wenn der Effekt kleiner ausfällt): *„Der Ledger ersetzt nicht jedes Review-Problem,
aber er verschiebt es in eine strukturierte Evidenzschicht, macht Entscheidungen explizit, reduziert freie
Verstärkung und erleichtert Review/Repair erheblich."* Die Asymmetrie (Arm B hat den Ledger, Arm A nicht) wird
**benannt**, nicht versteckt — sie IST der getestete Weg.

---

## 8. Ablauf (Schritte)

```
1. Arm B auf vollen Ledger heben       ProjectLedger() um evidence/notes; ADJ-GAP-Propositionen fixen (Refine).
2. Beide Artefakte (neu) erzeugen      source=transcript und source=ledger, gleiches Modell (z. B. gpt-5.4).
                                       Testmodus: kein cite-or-exclude-Zwang.
3. Stichprobe ziehen                   10–15 Requirements (aus Arm B + entsprechende aus Arm A).
4. Tabelle ausfüllen (K1–K4)           K1/K2/K4 großteils deterministisch; K3 per Facetten-Vergleich / Transkript.
5. Aggregat + Story                    zitierbare Tabelle + der ehrliche Absatz (oben).
6. iteration-notes-Eintrag + Commit    Ergebnis + Run-IDs festhalten.
```

**Verortung der Ergebnisse:** `runs/phase2evidenz-agent/{transcript,ledger}/<runId>/requirements.md` (die Artefakte)
+ die ausgefüllte Tabelle als Markdown neben diesem Plan.
