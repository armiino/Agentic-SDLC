# Steward-Vorlauf (rekalibriert 07.08.2026)

> Status: LEBEND — Teil des aktiven Steward-Strangs (mit `w4-diskussionsplan-steward-und-inbound.md` [Ex-plan-project-steward…] und `README.md`).
> **Ersetzt die Fassung vom 24.07.** („TODO — Vor ProjectStewardAgent"): deren 7-Punkte-Reihenfolge ist überholt —
> Prüfung 07.08. je Punkt: 1 ✅ erledigt · 2+7 = die echten Vorbedingungen · 3+4 = W4-Slices (nicht davor!) ·
> 5 teils erbracht (E2E 23.07.: --execute real, #32/#33; voller Transkript-Durchstich ✅), Rest = Härtung/W2 ·
> 6 = W2-Messpunkt (GateAttempt-Historie ist der Draht). Leitsatz (Kollegen-Review, bestätigt): **„Der Steward
> wird nicht blockiert, er bekommt eine Startbahn."**

---

## A. HARTE Vorbedingungen (vor Bau-Start — nur diese drei)

- [x] **A1 · Stand/Doku eindeutig** — ✅ erledigt 06./07.08.: R-11 committet; STATUS/aufgefallen/r11 auf EINER
  Wahrheit; ⚡-Reihenfolge in aufgefallen §2a; `runs/README.md` + `EVIDENZ-INDEX` (Lauf→Erzählung).
- [x] **A2 · Steward-MVP-Scope ⚖ ENTSCHIEDEN (Autor, 07.08.): Option 1 als STARTPUNKT, nicht als Endbild.**
  „Lesen/Erklären/Starten" ist der MVP — aber ausdrücklich NUR die erste Stufe: ALLE gedachten Fähigkeiten
  (Gespräch-als-Gate-Kanal 9k(d), Inbound, Vervollständigung …) bleiben als C-Slices verbindlich im Plan
  („dies reicht mir aber nicht" — der Slice-Pfad ist Teil des Entscheids). Ursprünglicher Vorschlag:
  **Der Steward liest, erklärt und startet — er schreibt nie selbst.**
  - KANN: Core/PBI/Decision/GitHub-Status erklären (liest `state/core` via ICoreRepository read-only, Run-Ordner,
    EVIDENZ-INDEX) · bestehende Workflows starten (pipeline-full run/resume, Stufen-CLIs) · Gate-/Review-Zustand
    erklären (pointer.json, ReviewHints, attempts) · nächste Aktion nennen.
  - NICHT (MVP): autonomer Apply · direkte GitHub-Writes · direkte Core-Mutation · Gate-Selbst-Beantwortung.
  - OPTIONAL im MVP zu entscheiden: **Gespräch-als-Gate-Kanal** (9k(d), Vier-Regeln-Vertrag) — als 5. Responder,
    Autorisierung bleibt beim Autor.
- [x] **A3 ✅ (07.08., `PipelineRunStatusReader` — Kern-Hebung statt Neubau; s. iteration-notes I-3) · Workflow-Tool-Vertrag** (die Sprache des Stewards über die Workflows). Modernisiert auf den
  heutigen Bestand — die Felder EXISTIEREN bereits als Artefakte, der Vertrag macht sie typisiert abfragbar:
  ```text
  runId · workflowName · finalDecision/gate-Pfad (events/decision-log) · pausedAt+checkpointId (pointer.json)
  nextRequiredAction (= die heutigen ReviewHints!) · summary · artefakte (plan/request/decisions/metrics-Pfade)
  ```
  **⚖ Aufruf-Naht (Autor, 07.08.): MAF-NATIV, nicht CLI.** Der Steward ist ein MAF-Agent, seine Workflow-Tools
  sind AIFunctions um die GETEILTEN Runner-Nähte (in-process: `PipelineFullRunner`-/Stufen-Kerne — dieselben,
  die CLI und Graph heute teilen), KEIN Prozess-Spawn/Shell-out. Tools: `run_pipeline_full` / `resume_run` /
  `get_run_status` + Stufen-Nähte. KEINE neue Logik — nur der typisierte Rückgabe-Vertrag über Bestehendem.

### A2-Detailplanung — die vier Klärungsfragen (Autor 07.08.: „detailliert planen!")

- [ ] **K1 · Laufform des Stewards:** MAF-`AIAgent` mit Tools + **Thread/Session-Persistenz** (= Matrix-Punkt
  Memory/Threads, erster echter Träger); Gesprächs-Frontend zunächst lokale Chat-Schleife (das „CLI eher
  weniger" gilt der WORKFLOW-Aufruf-Naht, nicht dem Frontend), später UI/Kanal offen. Zu planen: Session-Store,
  Kontext-Fenster/Compaction, System-Prompt (deutsch, R-6), Werkzeug-Katalog read-only vs. startend.
- [ ] **K2 · „Workflows starten" konkret:** in-process über die geteilten Nähte; Start asynchron mit runId
  sofort zurück; Pause-Erkennung via pointer.json/Checkpoint; der Steward ERKLÄRT dann Gate+ReviewHint und
  nennt den UI-Befehl (MVP) — beantwortet NICHT selbst (bis 9k(d)-Slice). Zu planen: Nebenläufigkeit
  (ein Lauf zur Zeit? Lock?), Abbruch, Fehler-Bericht (R-18-Events).
- [ ] **K3 · MAF-Matrix-Einbau je Slice:** Memory/Threads → K1 · **ToolApproval** → startende/teure Tools
  (Kosten-Gate!) · Handoff → Steward↔Spezial-Rollen (später) · MCP → C3 Read-Backend · Magentic/Loop →
  prüfen, ob der Steward-Kern selbst als Magentic-Orchestrierung taugt (Thesis-Wert). Je Punkt: Doku prüfen,
  nicht raten (Haus-Regel).
- [ ] **K4 · Thesis-Messung — ⚖ ENDBILD NEU (Autor, 07.08.): GRAPH vs. AGENT** (CLI = nice-to-have, KEIN
  Messwert — „zu viel"). Zu planen: gleiche Aufgabe (Meeting-Delta → Kette) einmal direkt pipeline-full vs.
  über den Steward orchestriert; Vergleichsgrößen aus metrics.json/otel (Zeit/Tokens/Gates/Interventionen);
  Replay-Policy hält Human-Entscheide konstant. W2-Kontrakt daran ausrichten.

## B. Vor-Steward-Härtung (klein; parallel zu A2/A3 machbar, blockiert den Bau nicht)

- [x] **R-16 · Repo-Guard ✅ (07.08., `GithubSnapshotGuard` — s. E2E-RUNBOOK B-Härtung)** — der Steward wird GitHub anfassen; Schutz vor falschem Repo/Stale-Snapshot
  gehört VOR seine ersten Läufe.
- [x] **READMEs 04–08 ✅ (07.08.)** — fünf Stufen-Karten (04-delta Modelle/§5 · 05-core Kangal/Views/Swaps · 06-backlog R-33 · 07-tore ALLE Tore-Tabelle · 08-pipeline Ein-Graph/Responder/R-40) + gate-landkarte-10-Gate-Refresh.
- [x] **R-10-Runner-Härtung ✅ (07.08., Exit 5 + Event statt stillem „-1")** („-1 items"/Idle ⇒ LAUTER Fehler-Exit statt stiller Notiz) — das Race selbst ist auf
  MAF 1.15 nicht mehr reproduziert (4/4 grün), aber der Steward muss sich auf LAUTES Scheitern verlassen können
  (Basis für K2-Fehler-Bericht). Klein, deterministisch.

- [x] **B4 ✅ (07.08., Run 135142: EXECUTED 2/0; R-15/R-23/R-30 LIVE — s. iteration-notes I-2) · Forward-Haken-Lauf (⚖ 07.08., Autor-Sorge „Gesamt-Zusammenspiel + Qualität"):** EIN kleiner
  SCHARFER Execute-Lauf gegen das Test-Repo (snapshot → forward → --execute, 1–2 PBIs) — der einzige Teil des
  Pipeline-Hakens, der seit R-11 fehlt: reale Writes zuletzt 23.07. bewiesen, die NEUEN Body-Inhalte (v2)
  bisher nur unit-belegt. Erbringt in EINEM Lauf die Live-Trias R-15/R-23/R-30. Grenze ehrlich: die
  arch-Sektionen („Technische Rahmenbedingungen"/„Umgesetzte Architektur-Arbeit") erscheinen erst, wenn der
  Autor die Klassifikation SCHARF auf den Produktiv-Core anwendet (Wahrheits-Mutation = Autor-Entscheid,
  eigener Akt — bis dahin bleibt dieser Teil unit-belegt, Rest-Risiko (c)).

**⚖ HAKEN-PRINZIP (Autor, 07.08.):** Nach B gilt die Workflow-Schicht als ABGEHAKT-STABIL; der Steward ist
eine NEUE SCHICHT DARÜBER (ruft nur die Nähte). Alles, was im Steward-Betrieb auffällt und die Workflow-Seite
betrifft, wird DORT sauber nachgebaut (R-Log-Disziplin, kein Steward-Workaround) — nur echte
Steward-Funktionalität entsteht in der Steward-Schicht.

**Stabilitäts-Disposition (07.08., voll verifiziert am R-Log):** Der fullworkflow ist „fertig für seinen
Zweck" — alle Bahnen gebaut+belegt, KEIN offenes R-Item ist ein Mechanik-Defekt. Offene Items nach Sorte:
*Härtung (→B):* R-16 · R-10-Runner · *Live-Beweise (→erster Steward-Ära-Execute-Lauf):* R-15/R-23/R-30 +
Rest-Risiko (a)–(d) aus r11 · *Komfort, je Stufe wenn berührt (→während):* R-7 UI-Hilfe · R-17-Merge
(Workaround det. Plan aktiv) · *Design-bestätigt, später:* R-13 Kontext-Hinweise · Parkplätze E ·
Nachzügler (ProposalStatus-Cleanup, Retriever-Rename).
- [x] **Live-Beweis-Trias R-15/R-23/R-30 ✅ (07.08., mit B4)** — alle drei hängen am NÄCHSTEN echten Execute-/Update-Lauf
  (EINE Gelegenheit, drei Beweise). Darf auch der erste Steward-Ära-Lauf sein — kein Blocker.

## C1-MOVE-BESCHREIBUNG (07.08., Kollegen-Schnitt übernommen — klein, kein Workaround)

- ✅ **C1a (07.08., s. iteration-notes I-4) · Read-/Status-Tools:** `StewardReadTools` in der NEUEN Schicht
  `AgenticSdlc.Host/Steward/` — AIFunctions `get_run_status(runId)` (= A3 `ReadAsync`, Ein-Zeiler) ·
  `list_paused_runs` (= `ReadPausedAsync`) · `get_core_overview` (Item-Zählung je Typ + Parkplatz +
  Kangal-Integritätszeile, read-only via ICoreRepository). Rückgaben = JSON des A3-Vertrags. LLM-freie Tests
  (Functions direkt invoken).
- ✅ **C1b (07.08., I-5 — der Steward SPRICHT; Live-Probe 143107) · Agent-Hülle:** `StewardAgent` (ChatClientAgent, deutscher System-Prompt/R-6, Tool-Katalog aus C1a)
  + lokale Chat-Schleife + Session speichern/laden (I-1-Spike-Muster, `state/steward/sessions/`).
- ✅ **C1c (07.08., I-6 — voller Approval-Kreis live [Ablehnung], runId-Naht am Runner) · Startende Tools:** `run_pipeline_full`/`resume_run` — in-process, start-async, sofort runId (K2);
  **in `ApprovalRequiredAIFunction` gewrappt** (K3: Kosten/Läufe = Zustimmung nötig); Rückgabe wieder A3-Vertrag.

## C. W4-Slices — WÄHREND des Steward-Baus (nicht davor; hier liegen die MAF-Matrix-Lücken MIT ABSICHT)

1. **Steward-Kern (MVP nach A2)** — lesen/erklären/starten; Thread/Memory + Handoff + ToolApproval + ggf.
   Magentic/Loop = genau die offene zweite MAF-Hälfte (`maf-feature-matrix.md` §C „nicht adoptiert") — **sie
   vorab separat abzubauen wäre falsch: sie sind der Sinn von W4.**
2. **GitHubInboundAgent** (Alt-Doc §3 → hierher verschoben; + 9k(a) arch-Spur, 9k(c) ADR-Durchstich):
   Inbound-Fälle → Vorschlag → BESTEHENDE Tore. Zweiter W4-Slice, nicht Vorbedingung.
3. **GitHub-Read-Backend** (Alt-Doc §4 → hierher): eine Tool-Oberfläche, Snapshot/Live/MCP austauschbar —
   natürlicher Träger des MCP-Matrix-Punkts.
4. **9f/9k(b) Vervollständigung** (Open-World-Lücken req+arch) als Steward-Fähigkeit; 9k(d) falls nicht im MVP.

## D. W2-Vorbereitung (nach/mit Steward — ⚖ Endbild 07.08.: **Graph vs. Agent**; CLI nice-to-have, kein Messwert)

N≥3-Wiederholungs-Belege je Agent (Alt-Doc §5-Rest) · Repair-Loop-Empirie quer (Alt-Doc §6; Auswertung der
GateAttempt-Historien = der verlegte Messdraht) · Autorisierungs-Kanal-Vergleich UI vs. Chat (9k(d)-Messpunkt) ·
9g-V2/9h.

## E. Später/optional (bewusst geparkt, Referenz aufgefallen.md)

9c Gate-Loop-Baustein · 9d Resume/Fehler-Semantik (+State-vs-Datei-Notiz; K2 streift es) · 9e Neuzuschnitt ·
9i Frage-Issues (W4-nah, passt zu C2-Inbound) · 9j Kapsel-Endbild · ②b Recipe-Kapsel · 9l-Rest
(Namens-/Plumbing-Review, DB-Checkliste formal) · Weg A/B3.
**⚠ Haken-Prinzip-Konsequenz:** 9j/②b (Kapsel-Umbauten) und 9c (Loop-Abstraktion) sind Graph-STRUKTUR-Umbauten
— bewusst NACH dem Steward-Bau (oder nie): während der Steward auf der Schicht steht, wird sie nicht umgebaut.

---

**Start-Sequenz (Stand 07.08. abends):** A2 ⚖ ✅ → K1–K7 ⚖ ✅ → A3 ✅ → B ✅ → ★ C1 ✅ (a/b/c) → ★ C3 ✅ (⚖ K5/K7 vor C2 gezogen; Lese-Werkzeugkasten + Session-Wache + R-41) → Autor-Commit ✅ → ★ M1 ✅ (Reducer-Slot, Spike 3/3, MEAI001-Fund; M1b run-config geparkt) → **▶ C2 → D.**
