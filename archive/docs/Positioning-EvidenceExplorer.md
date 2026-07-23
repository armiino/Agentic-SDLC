# Positionierung & aktuelle Architektur — Evidence Explorer

> **Zweck.** Diese Notiz hält zweierlei fest: (1) **was das System heute IST** — wo LLM-Urteil sitzt, wo Determinismus,
> wo (bisher als einziges) echte Agency, und wie der MAF-Workflow das zusammenhält. (2) Eine **saubere Trennung** der
> Evidence-Explorer-Idee in **Teil A = Architektur-These (begründet)** und **Teil B = offene, zu messende Fragen**.
> Grund für die Trennung: der ursprüngliche Explorer-Text vermischt „vorgeschlagene Architektur" (stark) mit
> „bewiesenem Nutzen" (noch offen). Dieses Dokument hält beide Register auseinander, damit die Aussagen kapitel-fest
> sind. Belege: Läufe `ebf4cd` (agentic v1), `26eea5` (v2 + Drill-down), `bbe15c` (v3 --narrate).

---

## Teil 0 — Die Einordnung in einem Satz

Das System ist **weder RPA noch ein autonomes Multi-Agenten-System**, sondern eine **MAF-basierte, provenienz-
gesteuerte LLM-Pipeline mit zuschaltbaren agentischen Armen** — gebaut, um zu *messen*, wo Agency in frühen
SDLC-Schritten nützt und wo sie Overengineering ist. Der Determinismus ist **Studiendesign** (Kontrollbedingung),
nicht ein Mangel an Agenten.

Vier Kategorien zur Verortung:

```text
RPA                   feste Regeln, KEIN semantisches Urteil.                     → NICHT wir (unser Urteil ist irreduzibel LLM).
Bounded-LLM-Pipeline  fester Ablauf, LLM füllt semantische Slots.                 → das MEISTE von uns.
Autonomer Agent       LLM plant Aktionen/Reihenfolge/Wann-fertig selbst.          → NUR der Derivation-Arm (--agentic).
Multi-Agenten-System  mehrere autonome Agenten interagieren.                      → NICHT wir (kein Overclaim).
```

---

## Teil 1 — Die aktuelle Architektur, ausdrücklich

Die Evidenzkette hat **drei Schichten**. Nach jeder steht ein deterministisch geprüftes, an die Vorschicht
verankertes Artefakt.

```text
Transkript → [LEDGER] consumable.json → [BASELINE] artifact.json je Typ → [DERIVATION] derived.json
             Evidenz-Claims             extrahierte, geprüfte Items         inferierte Items
             (human-adjudiziert)        (anker: sourceClaimIds → Ledger)     (anker: sourceArtifactItemIds → Baseline)
```

### 1.1 Schicht Ledger — die kontrollierte Wissensbasis (deterministisch/governt)
- `consumable.json`: aus dem Rohtranskript destillierte **Claims** (proposition, kind, status, evidence, scope),
  **human-adjudiziert und eingefroren**. Das ist die **Grundwahrheit** und die einzige erlaubte Evidenzquelle.
- **Bewusst NICHT agentisch.** Ein Fundament, das die Open-World reduziert, muss stabil und reproduzierbar sein —
  Agency hier würde genau seine Rolle untergraben.

### 1.2 Schicht Baseline — Extraktion (LLM-Urteil im festen Ablauf)
Je Artefakttyp (requirements, architecture, risks, open-questions) ein Zweig, **parallel** per MAF-Fan-out:

```text
Maker(LLM)  →  Checker(LLM C7-Evidence-Vote + det. MC0-Struktur)  →  Repair-Loop(LLM)  →  ID-Gate(det.)  →  artifact.json
maker.md       step-check-NN/report.json                            final.md              REQ-01.. mit sourceClaimIds
```
- **LLM-Power:** Maker (extrahiert Items aus dem Ledger), Checker (semantisches „trägt die Evidenz das?").
- **Determinismus:** MC0-Strukturprüfung, ID-Gate (stabile IDs), Anker an `sourceClaimIds`, Persistenz.
- **Agentik: keine.** Der Ablauf ist fix, die LLMs füllen Slots. Provenienz-kritisch + closed-world → bewusst bounded.

### 1.3 Schicht Derivation — Inferenz (ZWEI Modi, config/flag-schaltbar)
Aus 1..N Baseline-Artefakten neue Items ableiten (Risiken, Gap-Requirements). Hier — und nur hier — sitzt die Agency.

**Strukturierter Modus (Mess-Default):** der Agent ist eine **Transformationsfunktion**.
```text
Host lädt Items → Generate(LLM, tools:[]) → Anchor(det.) → InferenceCheck(LLM-Judge) → Host schreibt derived.json
```

**Agentischer Modus (`--agentic`, Stufe 1):** der Agent ist ein **Akteur**.
```text
EIN Executor-Knoten:
  Agent(LLM + Tools) entscheidet selbst: liest via get_baseline_items, prüft via check_anchor,
    schreibt via save_derived  (+ optional Drill-down get_source_claims / find_related_items)
  → danach UNABHÄNGIGE Assurance: Anker-Validierung(det.) + InferenceCheck(LLM-Judge)  [re-schreibt NICHT]
```
- **LLM-Power:** die Inferenz selbst (welche Risiken folgen aus dem Zusammenspiel).
- **Determinismus:** Anker-Validierung, ID-Vergabe (im Tool), Assurance-Reports.
- **Agentik (echt):** der Agent wählt Aktionen/Tools/Wann-fertig selbst — belegt in `tool-calls.jsonl`.

### 1.4 Der MAF-Workflow (was alles zusammenhält)
- `WorkflowBuilder` + `Executor<TInput>` als Knoten; Kanten `AddEdge`/`AddFanOutEdge`/`AddFanInBarrierEdge`;
  `WithOutputFrom`. Sub-Workflows per `BindAsExecutor` einhängbar.
- Agenten laufen **in** Executoren; deterministische Schritte sind ebenfalls Executoren (beides MAF-nativ).
- **Middleware/Observability** (phase-unabhängig verdrahtet): `InputContextLogger` (was das Modell sah),
  `ChatDecisionLogger` (Assistant-Text je Modell-Runde; via `innerCycleLogging:true`), `ToolCallLogger`
  (jeder Tool-Aufruf → `tool-calls.jsonl`). Plus OTel-Export.

### 1.5 Wo also LLM-Power, wo Determinismus, wo Agentik — kompakt

| Schritt | Wer | Kategorie |
|---|---|---|
| Ledger-Claims (Extraktion+Adjudikation) | Mensch + Governance | deterministisch/governt |
| Baseline-Maker (Items aus Ledger) | **LLM** | LLM-Urteil, fester Ablauf |
| Checker C7 (Evidence-Vote) | **LLM** | LLM-Urteil, fester Ablauf |
| MC0-Struktur / ID-Gate / Anker-Validierung | Code | deterministisch |
| Derivation-Generate (Risiken/Gaps) | **LLM** | LLM-Urteil |
| Derivation `--agentic` (Tool-Wahl, Lesen, Schreiben, Wann-fertig) | **Agent** | **echte Agency** |
| InferenceCheck (Treue je Item) | **LLM-Judge** | bounded LLM-Urteil |
| Persistenz, Run-Snapshot, Orchestrierung | Code / MAF | deterministisch |

**Fazit Teil 1:** Semantisches Urteil ist überall LLM (irreduzibel → kein RPA). Verbindliche Invarianten sind
deterministisch (Provenienz/Reproduzierbarkeit). **Echte Handlungs-Autonomie existiert an genau einer Stelle** (dem
agentischen Derivation-Arm) — bewusst, als messbarer Kontrast zum strukturierten Arm.

---

## Teil A — Architektur-These (begründet, tragfähig)

> Kalibrierte Fassung: der Schwerpunkt liegt **nicht** auf „mehr Agenten/mehr Autonomie", sondern auf der
> **systematischen Evaluation von Freiheitsgraden** gegen den strukturierten Workflow als Kontrollbedingung.

**A1. Kein nachträglicher Maximal-Umbau — der strukturierte Workflow IST die Kontrollbedingung.** Es erscheint wenig
sinnvoll, den Workflow nachträglich möglichst agentisch umzubauen oder Agenten nur einzuführen, um den „Agentic-AI-
Anteil" zu erhöhen. Die Architektur baut bewusst eine kontrollierte, nachvollziehbare, reproduzierbare Evidenzkette;
der Ledger reduziert die Open-World und ist der zentrale Projektzustand. Diese Eigenschaften würden durch eine
Verlagerung der frühen Extraktions-/Strukturierungsschritte auf autonome Agenten **geschwächt, nicht gestärkt**. Der
strukturierte Workflow ist deshalb **kein Mangel an Agentik, sondern die notwendige Kontrollbedingung**, gegen die
sich agentisches Verhalten überhaupt erst sinnvoll untersuchen lässt.

**A2. Nicht „wie viele Agenten", sondern „welche Freiheitsgrade bringen messbaren Mehrwert".** Der Fokus liegt darauf,
gezielt zu untersuchen, welche zusätzlichen Entscheidungsfreiheiten einem Agenten tatsächlich Nutzen bieten. Der
**Derivation-Agent** ist der geeignetste Untersuchungsgegenstand: seine Aufgabe ist nicht reine Extraktion, sondern
**Ableitung neuer Artefakte aus bereits verifizierten Projektinformationen** — genau hier existiert Interpretations-
und Entscheidungsspielraum, den Agency sinnvoll nutzen *könnte* (ob sie es tut → Teil B).

**A3. Generator → Evidence Explorer in einer kontrollierten Projektumwelt.** Der Agent wird nicht als reiner Generator
verstanden, sondern als **explorativer Problemlöser**. Seine Umwelt = der **persistierte Projektzustand**: Verified
Baseline Set (Requirements, Architektur, Risiken, Open Questions), optional Glossar/Review-Entscheidungen/Assurance-
Reports/Ledger-Claim-Referenzen. **Kein Rohtranskript, keine beliebige Quelle** → Closed-World + Ergebnis-Provenienz
bleiben. **Der Host definiert die Umwelt und stellt die Tools bereit; innerhalb davon entscheidet der Agent selbst**,
welche Artefakte er konsultiert, welche Tools er einsetzt und wann genug Evidenz vorliegt. Agency = **eigener
Arbeitsweg innerhalb klarer Grenzen**, nicht unbegrenzte Freiheit.

**A4. Schmale, domänenspezifische, sichere Tools.** Vorhanden: `get_baseline_items`, `check_anchor`, `save_derived`,
`get_source_claims`, `find_related_items`. Ergänzbar **bei Bedarf**: `search_items`, `get_item`. Entscheidend ist
**nicht die Anzahl**, sondern dass der Agent selbst entscheidet, **ob und wann** ein Tool nötig ist — diese
Werkzeugauswahl (inkl. bewusster Nicht-Nutzung) ist selbst eine Form agentischen Verhaltens. *(Ob sie Mehrwert bringt →
zwingend an eine Ergebnis-Metrik gebunden, s. B1/B6.)*

**A5. Zielbasiertes statt schrittweises Prompting.** Der Prompt beschreibt **Ziel + verfügbare Tools + Grenzen**
(„leite möglichst gut begründete Risiken ab; nutze ausschließlich die Tools; nimm nichts außerhalb des Projektzustands
an") statt der einzelnen Arbeitsschritte. *(Preis: Prozess-Reproduzierbarkeit, s. B2 — bewusster Tausch.)*

**A6. Gedächtnis im persistierten Projektzustand, nicht im LLM.** Im Sprint-/Evolutionsszenario wächst der
Projektzustand (neue Artefakte, Review-Entscheidungen, historische Ableitungen) und wird faktisch zum
**Langzeitgedächtnis**. Der Agent braucht kein internes Chat-Memory; er greift pro Lauf erneut auf den aktuellen
Zustand zu und wählt, welche Teile er konsultiert. **Das Gedächtnis des Systems liegt in Ledger + Artefakten.**

**A7. Arbeitsteilung + wissenschaftlicher Beitrag.** Deterministische Workflows sichern **alle verbindlichen
Invarianten** (Persistenz, ID-Stabilität, Provenienz, unabhängige Validierung); der Agent agiert **explorativ
innerhalb** dieser Umgebung. Der Beitrag besteht **nicht** darin, möglichst viele Agenten/viel Autonomie zu bauen,
sondern **empirisch zu bestimmen, welche Formen agentischer Autonomie innerhalb einer evidenzbasierten SDLC-Pipeline
tatsächlich Mehrwert bieten — und wo zusätzliche Agentik Overengineering ist.**

---

## Teil B — Offene, zu MESSENDE Fragen (noch nicht bewiesen)

**B0. VORBEDINGUNG jeder Evaluation: die Qualitäts-/Mehrwert-Metrik für Ableitungen definieren.** Der gesamte Plan
(„verändert ein Freiheitsgrad die *Qualität*?") hängt an einer Metrik, die **noch nicht definiert ist** — und für
Ableitungen ist das *genuin schwer*. Vor jedem A/B muss festgelegt werden, was „bessere Ableitung" heißt. Kandidaten
und ihre Fallstricke:
```text
Treue (Inference-Check)      folgt jedes Item plausibel aus seinen Ankern?   → schon vorhanden, aber MISST NICHT „besser",
                                                                                nur „nicht halluziniert" (Filter, kein Gütemaß).
Menschliches Rating          Experte bewertet Relevanz/Schärfe der Risiken.  → aussagekräftig, teuer, subjektiv → Kalibrierung nötig.
Valide Cross-Artifact-Bezüge Zahl korrekt verankerter req×arch-Kopplungen.   → gut messbar, closed-world-konform.
Vollständigkeit/Coverage     „mehr/fehlende Risiken gefunden?"               → ÖFFNET das zurückgestellte Completeness-Problem
                                                                                (open-world, nicht valide messbar) → NICHT als Primärmetrik.
```
Empfehlung: **Treue als Pflicht-Gate** (darf nicht sinken) **+** eine **primäre Gütemetrik** (menschliches Rating auf
kleiner Stichprobe ODER valide Cross-Artifact-Bezüge), **Vollständigkeit bewusst NICHT**. Ohne diese Festlegung ist
keine der folgenden Fragen (B1, B3, B6) auswertbar. **Das ist der nächste Arbeitsschritt, nicht ein weiterer Bau.**

**B1. „Ungenutzte Drill-down-Tools = sinnvolle Werkzeugauswahl" ist NOCH NICHT belegt.** Befund bisher: in `26eea5`
und `bbe15c` nutzte der Agent `get_source_claims`/`find_related_items` **nicht** und begründete (v3) kohärent, die
Item-Texte reichten. Das ist ein **wertvoller Hinweis** („angeboten ≠ gebraucht"), aber **kein Nachweis optimaler
Auswahl**: n≈2–3, eine Aufgabe, ein Modell — und **es wurde nie geprüft, ob Drill-down das Ergebnis *verbessert*
hätte**. „Nicht genutzt + selbst als ausreichend bezeichnet" könnte auch Selbstüberschätzung sein. *Zu messen:*
erzwungenes Drill-down vs. nicht, **Ergebnis-Qualität** gegenübergestellt. Bis dahin: **claimed, not proven.**

**B2. Zielbasiertes Prompting opfert PROZESS-Reproduzierbarkeit — Trade-off benennen.** Je freier der Agent seinen Weg
plant, desto **weniger reproduzierbar der Lauf** (verschiedene Läufe konsultieren verschiedene Artefakte). Die
**Ergebnis-Provenienz bleibt** (Anker geprüft), aber der **Prozess-Determinismus wird bewusst gegen Exploration
eingetauscht**. Das ist vertretbar — aber es ist ein Tausch, kein „alles behalten". *Zu messen:* Prozess-Varianz über
Wiederholläufe (welche Tools/Artefakte, in welcher Reihenfolge, Streuung).

**B3. Der Explorer-Nutzen ist eine Hypothese — der eine Lauf zeigte Generator-Verhalten.** In allen drei agentischen
Läufen verhielt sich der „Explorer" wie der Generator (beide Typen lesen → verankern → schreiben); **kein**
exploratives Verhalten (kein Drill-down, keine Suche, keine iterative Sammlung). Der Explorer hat also **nicht
exploriert**. Wert vermutlich erst bei **dünneren/schwereren Aufgaben** (z. B. `requirements-gap`, Inkonsistenz-
Erkennung über mehrere Artefakte, Auswirkung neuer Sprint-Transkripte) oder im **Sprint-Szenario**. *Zu messen:* eine
Aufgabe wählen, die Exploration **plausibel nahelegt** (NICHT erzwingt — sonst engineert man das Ergebnis), und
**beobachten**, ob der Bedarf das Explorationsverhalten *von selbst* auslöst. „Erzwingen" beantwortet „kann er?",
nicht „wählt er sinnvoll?".

**B4. `find_uncovered_claims()` würde das bewusst ZURÜCKGESTELLTE Completeness-Problem wieder öffnen.** Gap-Audit/
Completeness gilt als „open-world, nicht valide messbar" und wurde zurückgestellt. Ein Coverage-Tool widerspricht dem.
*Entscheidung nötig:* entweder begründen, dass „unabgedeckte **bekannte** Ledger-Claims" gegen den **geschlossenen**
Ledger valide ist (im Gegensatz zu „fehlt in der Welt etwas?"), oder das Tool weglassen. **Nicht** kommentarlos aufnehmen.

**B5. Tool-Erweiterung bedarfsgetrieben, nicht spekulativ.** Weitere Tools (`search_items`, `get_item`, …) erst
einführen, wenn eine Aufgabe sie **nachweislich** braucht — sonst widerspricht die Erweiterung genau dem Befund B1/B3
(der Agent nutzt schon die vorhandenen Zusatz-Tools nicht). Least-Privilege + Tool-Budget beibehalten.

**B6. Selbstversiegelungs-Falle vermeiden: Agency-Nutzen an das ERGEBNIS binden, nicht an die Tool-Entscheidung.**
„Tool genutzt = agentisch" UND „Tool bewusst nicht genutzt = auch agentisch (sinnvolle Auswahl)" ist ein
**Heads-I-win-tails-I-win** — so ist die These unfalsifizierbar. Ausweg: Agency zählt nur dann als *nützlich*, wenn sie
eine **Ergebnis-Metrik** (B0) bewegt — **unabhängig davon, ob Tools benutzt wurden**. Die Bewertung hängt am Outcome,
nicht an der Handlung. Konkret: strukturierter Arm vs. agentischer Arm → gleiche Baselines, gleiches Treue-Gate →
**bewegt sich die Gütemetrik?** Wenn nein, ist die zusätzliche Agency (ob genutzt oder bewusst ungenutzt) für diese
Aufgabe Overengineering — ein valides, publizierbares Ergebnis.

---

## Teil C — Ehrliche Reifegrad-Bilanz

| Aussage | Status |
|---|---|
| System ist provenienz-gesteuerte LLM-Pipeline mit einem echten agentischen Arm | **belegt** (Code + Läufe) |
| Agentischer Modus funktioniert: Agent liest/prüft/schreibt selbst, Provenienz erhalten | **belegt** (`ebf4cd`, tool-calls.jsonl) |
| Reasoning-Spur sichtbar machbar (Prompt v3), kein Beobachtereffekt in diesem Fall | **belegt** (`bbe15c`) |
| Drill-down-Tools sind gebaut, gescopet, geloggt | **belegt** (Build + angeboten) |
| Drill-down-Tools liefern korrekte Daten | **ungeprüft** (Agent rief sie nie auf → deterministischer Direkttest offen) |
| „Nicht-Nutzung = gute Auswahl" | **Hypothese** (B1) |
| Explorer bringt Mehrwert ggü. Generator | **Hypothese** (B3) |
| Ergebnis-Qualität agentic vs. structured | **ungemessen** (kein A/B mit Wiederholungen) |
| Qualitäts-/Mehrwert-Metrik für Ableitungen | **undefiniert** — Vorbedingung B0, blockiert jede Eval |

---

## Teil D — Mögliche nächste Schritte (zur Diskussion)

**0. VORBEDINGUNG — Metrik definieren (B0).** Erst festlegen, was „bessere Ableitung" heißt (Treue-Gate + eine
   primäre Gütemetrik; Vollständigkeit bewusst NICHT). Ohne das sind 2.–4. nicht auswertbar. **Nächster Schritt, kein Bau.**
1. **Tool-Korrektheit sichern:** deterministischer Direkttest von `get_source_claims`/`find_related_items` (da vom
   Agenten nie aufgerufen) — kleine, LLM-freie Prüfung.
2. **B3 beobachten:** eine Aufgabe wählen, die Exploration plausibel nahelegt (`requirements-gap` / Inkonsistenz-
   Erkennung), und sehen, ob der Agent **von selbst** exploriert — nicht erzwingen.
3. **B1 messen:** freies vs. erzwungenes Drill-down, Ergebnis-Qualität (per B0-Metrik) vergleichen.
4. **A/B Mess-Hygiene (an B0-Metrik + B6):** agentic vs. structured, je ≥3 Wiederholungen — Gütemetrik, Prozess-Varianz
   (B2), Kosten/Laufzeit. Agency zählt nur, wenn die Gütemetrik sich bewegt (nicht die Tool-Nutzung an sich).
5. **Härtung:** `save_derived` via structured output (Glitch-Klasse aus `ebf4cd` schließen).
6. **Entscheidung B4:** `find_uncovered_claims` — begründen oder verwerfen.

> **Leitsatz.** Vorgeschlagene Architektur (Teil A) ≠ bewiesener Nutzen (Teil B). Die Explorer-These ist stark und
> übernehmenswert; ihr Mehrwert ist eine offene, saubere Messfrage. Genau diese Trennung ist der wissenschaftliche
> Beitrag: nicht „Agenten sind toll", sondern **„wo innerhalb einer kontrollierten Evidenzumgebung trägt Agency — und
> wo ist sie Overengineering?"**
