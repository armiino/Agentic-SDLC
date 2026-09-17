# Iteration Notes 23.07.2026 — Frischer E2E-Durchstich (G-1) + Betriebszyklus-Beweis (E11)

## 1. Kontext und Ziel

Erster vollständiger End-to-End-Lauf NACH dem genealogischen Refactoring (Struktur = FullWorkflow-Kette,
Namespaces = Struktur, Mechanik in shared/, 80 Tests). Vier Ziele in einem Lauf:

1. **G-1 (der „schwere Beweis"):** frisches Transkript → Ledger → Adjudikation → Baselines → Core →
   Backlog → echte GitHub-Issues — jede Mutation autorisiert, durchgängige Provenance.
2. **Frischer Start als Vergleich:** Der Alt-Core (144 Items) entstand teils OHNE Ledger-Front
   (meeting-2-delta übersprang sie); der Neuaufbau fährt die GANZE Kette. Alt-Core geparkt:
   `runs/_e2e-backup/state-core-alt/`; Alt-Läufe: `runsArchive/` (Struktur 1:1).
3. **Mängel-Aufnahme:** Jeder Fund → Reibungs-Log R-n (E2E-RUNBOOK, lokal) mit Beleg-Run + TODO.
4. **Test mit Vorab-Erwartungen:** Für Phase B wurde das Transkript GEZIELT konstruiert
   (`input/transcripts/meeting-2-extended.txt`), Erwartungen E1–E10 vorab schriftlich.

Setup: Modell durchgehend `openai/gpt-5.4` (openrouter; `jury.judgeModel` von gpt-4.1-mini auf 5.4
gestellt — die Ledger-Runner bevorzugen judgeModel!). Ziel-Repo NEU: `armiino/Agentic-GitHub-refactor`
(fine-grained PAT `GITHUB_AGENTIC_REFACTOR_TOKEN`, lokal in .env). Core-History-Härtung aktiv
(Snapshot je inhaltsänderndem Save nach `state/core/history/`, seit heute — Log #38).

## 2. Phase A — Bootstrap aus `Interview-Einrichtung.txt`

| Schritt | Run | Ergebnis |
|---|---|---|
| A1 ledger-build-units | `20260723_093443_a85a80` (4. Lauf) | gate pass, 45 validierte Claims, Propositions DEUTSCH |
| A2 Adjudikation + Refine | (in A1) | 63 Queue-Items entschieden → consumable **49 Claims, pending=0**; 4 ADJ-GAP-Claims per Refine facettiert |
| A3 recipe (Arm B) | `20260723_113115_35335a` | Baselines **requirements=55, architecture=45**, deutsch; Checker-Endzustand `HumanReview` (= Design, identisch Alt-Beleg 09.07.) |
| A3-Derivations (load-Workaround) | `20260723_114515_f3f537` | **risks=7, gap=9**, deutsch (build-Derivations liefen nicht → R-10) |
| A3.5 l3 (03-gap) | `20260723_122625_136540` | 35 Kandidaten → 15 auto-supported / 20 reviewt → **15 promotet (L3-REQ-001…015)**, 5 abgelehnt |
| A4 core-bootstrap (+`--l3-run`) | `runs/bootstrap/20260723_124020_fc231e` | **Core: 115 Items** (70 req = 55+15L3, 45 arch), relations=134, Provenance je Item |
| A5 Schnitt + Seed | Cluster `…130243_a96fda` → Backlog `…131421_80a11e` | 14 Feature-Cluster → **30 PBIs** (30/30 reviewt) → Seed: **Core 159**; 1. History-Snapshot (115er-Stand) |

Wichtige A-Läufe, die NICHT auf Anhieb klappten (Details §5): A1 brauchte 4 Anläufe (R-1 Referenz-Repair
2×, R-6 Sprache 1×); A2-Apply musste separat nachgeholt werden (R-9); A5 hatte einen fehlenden Schritt in
der Bootstrap-Ansage (`l4-re-clarify clarify`, R-12).

## 3. Phase B — `meeting-2-extended.txt` durch die ganze Kette (Erwartungen vorab)

Das Transkript wurde als Testinstrument gebaut: 10 Erwartungen (E1–E10), verankert an existierenden
Core-Items (REQ-32, L3-REQ-002/004, REQ-08, ARCH-39 …).

| Schritt | Run | Ergebnis |
|---|---|---|
| B1 ledger-build-units | `20260723_133055_48b8cf` | gate pass 0/0; 36 Units (29 used, 4 noise, 3 needs_human), 26→20→20 Claims |
| B2 Adjudikation | (in B1) | 23 Claims (Autor: accept_gap-all inkl. 3 Meta — bewusst als Staffel-Gate-Lehrstück verfolgt) |
| B3 recipe | `20260723_134102_c63c89` | req=24, arch=16; Derivations liefen DIESMAL im build (→ R-10 ist intermittierend!) |
| B4 project-state-build | `runs/project-state/e2e-meeting2/` | MeetingDelta 24 requirement-Items |
| B5 Tor 1 (ingest-hitl) | `20260723_135427_9260f1` | APPLIED 24: 8 NEW, 11 NEW_RELATED, **4 REFINE (REQ-32, L3-REQ-002, L3-REQ-004, REQ-08), 1 SUPERSEDE (REQ-70)**; Core 159→179 |
| B6 Placement (pbi-update-hitl) | `20260723_141409_8ea1c5` | newPbis=13, updatedPbis=8; Core 179→**192** (43 PBIs) |
| B7 Initial-Sync + Write | `runs/github-forward/_initial-sync-det` | Review 39 apply / 4 skip → **EXECUTED created=39, failed=0**; 39 `implemented_by_issue`-Relationen im Core |

**Erwartungs-Bilanz E1–E10:** alle erfüllt, drei davon lehrreicher als geplant:
- E2: Die vage Piktogramm-Aussage wurde nicht needs_human, sondern **eigener Claim, der sich selbst als
  unscharf markiert** (`uncertain|must_clarify`) — besseres Verhalten als erwartet.
- E4: Identity-Matching bewiesen — insbesondere: **L3-Promotions werden im Betrieb weiterentwickelt**
  (REFINE auf L3-REQ-002/004), kein einziges Duplikat; SUPERSEDE als stärkste Form.
- E10 (schärfer als Soll): Der „Firestore endgültig"-Beschluss ging nicht verloren, sondern **spaltete
  die Wahrheit**: via requirements-Route kam „festgelegt" in REQ-70 an, ARCH-39/REQ-55 sagen weiter
  „vorläufig" → stärkster Beleg für das GROSSE Architektur-TODO (R-11: kein Update-Pfad für arch-Items;
  Tor 1 filtert `itemType=requirement`).
- E6-Detail: Heimaufsichts-Auflage exakt als `compliance_constraint|required|must` (Rasierklingen-Regel
  hielt); E9: weiche Wünsche blieben durch C3 im Artefakt weich („Wunsch", kein „muss").

**Initial-Sync-Erkenntnis (R-15/R-16/R-17):** Für ein frisches Repo fehlt der Initial-Sync als
Produktschritt. Drei Anläufe: (a) Agent mit 43 PBIs → Gate block, Agent schafft ~11–25 Ops/Antwort UND
der Plan-Repair ERSETZT statt zu mergen (R-17); (b) davor fing der **Dry-Run** die Stale-Snapshot-Falle
(alter `runs/github-snapshot` hätte 22 PBIs auf Issues des ALTEN Repos gelinkt — R-16; Altbestand
archiviert); (c) Lösung: **deterministischer Plan** aus Core-Payloads (43 CREATE-Ops, echtes Planformat,
Review+Apply über die normalen Kommandos) = gelebter Prototyp des künftigen `github-initial-sync`.

## 4. E11 — der Betriebszyklus (Mini-Meeting-3, ohne Front)

Frage des Autors: „Wir hatten nur EINEN GitHub-Lauf — wurde je etwas GEÄNDERT statt erstellt?" → Richtig:
Der Update-Pfad war unbewiesen (Initial-Sync lief NACH der Meeting-Einarbeitung). Beweis per synthetischem
2-Item-Delta (`runs/project-state/mini-meeting-3/`): Archivfrist 7→14 Tage (zielt auf REQ-63→PBI-033→Issue #33)
+ PDF-Export (Neuheit).

Ablauf mit Funden:
1. Tor 1 `20260723_150814_4d792d`: Resume endete zweimal „ohne Apply-Report" → **R-18: die HitlShell
   verschluckte Executor-Fehler-Events still** (Race-Fehldiagnose ehrlich korrigiert); dahinter **R-19:
   `Union(null)`-Crash bei Deltas ohne sourceClaimIds**. Beide gefixt → APPLIED 2/2 (1 NEW→REQ-77,
   1 SUPERSEDE REQ-63→REQ-76).
2. Placement `20260723_151741_f00984`: updatedPbis=[PBI-032, PBI-033] (PDF-Export als Erweiterung
   einsortiert statt neues PBI — vertretbar). **R-20 (Autor-Feedback): Placement-Review-UI zeigt Ops ohne
   PBI-Kontext — „man versteht gar nichts" → faktisch Blanko-Akzept.**
3. Forward ohne Snapshot `…152036`: nur `FLAG_DRIFT` → **Betriebs-Takt-Erkenntnis: `github-snapshot`
   (Read-Tor) gehört VOR jeden Forward.**
4. Snapshot `…152421` (39 Issues) → Forward `…152440`: Plan korrekt 2× UPDATE_ISSUE. Execute-Anlauf 1:
   `alreadyApplied=2, updated=0` → **R-21 (Hauptfund): Die Idempotenz-Wache machte den GESAMTEN
   Update-Pfad zu totem Code** („auf dasselbe Issue gemappt = schon angewendet" — für UPDATE genau
   falsch). Anlauf 2 nach Fix: `failed=2, GitHub 422` → **R-22: REST-Client sendete `state:null`**.
   Anlauf 3: **`updated=2, failed=0`** — Issues #32/#33 nachweislich geändert (API-Gegenprobe, updated_at).
5. **R-23 (offen): Inhalts-Qualität des Updates** — der Agent ersetzte den reichen Issue-Body durch einen
   Stub, die eigentliche Änderung (14 Tage / PDF-Export) steht nirgends im Issue; Titel veraltet.
   Mechanik ✓, Inhalt regressiv → deterministische Update-Bodies bzw. Kommentar-mit-Diff (Familie R-15/R-17).

**Kernaussage E11:** Zwei Produktions-Bugs (R-21, R-22) waren NUR durch reales Ausführen des Update-Pfads
auffindbar — kein Test, kein Smoke, kein Dry-Run hätte sie gezeigt. Der „schwere Beweis" hat sich doppelt
bezahlt gemacht.

## 5. Reibungs-Log-Bilanz (Details + TODOs: E2E-RUNBOOK, lokal)

**Behoben (12):** R-1 (Referenz-Repair unused-compare: Prompt-Härtung + Nachfrage-Pass + Downgrade;
Prompt-Härtung allein drehte das Verhalten), R-2 (Miss-Signal in Adjudikations-Queue), R-5 (UI-Sprache
Labels), R-6 (Sprach-Pflicht in 10 Ledger-Prompts — Propositions deutsch, laufzeit-bewiesen), R-9
(adjudicate-ui: validated als 2. Argument → auto-apply), R-12 (clarify-Schritt in Bootstrap-Ansage),
R-16 (Stale-Snapshot archiviert; **Dry-Run-Doppelboden hat real gewirkt**), R-18 (Shell meldet
Executor-Fehler laut), R-19 (Union null-tolerant), R-21 (Update-Pfad-Idempotenz), R-22 (state:null),
sowie die Sofort-Doku R-4-Teil (READMEs 01/02/03).

**Offen — die Schärfungsliste (11):**
- **R-11 GROSS: Architektur-Rolle** (Autor: „arch gehört genauso ins PBI/Issue") — Beleg: gespaltene
  Wahrheit REQ-70 vs. ARCH-39; Tor 1 auf arch erweitern + PBI-Modell-Technik-Bezug ODER begründet verwerfen.
- **R-10: Recipe-Derivations intermittierendes Race** (build-Modus; Zweig→Collector; Workaround load-Recipe).
- **R-15/R-17/R-23-Familie: Tor-3-Qualität** — `github-initial-sync` deterministisch, Plan-Repair-Merge,
  deterministische Update-Bodies/Kommentar-Diffs.
- **R-14: Decision-Brücke** — offene Fragen werden nie `itemType=decision` (nur l4-completion mintet
  `ADD_OPEN_DECISION`, fehlt im Takt) → Tor 2 läuft im Standard-Pfad leer (E8 nur halb bewiesen).
- **R-20 + R-7: UI-Kontext/Hilfe-Tiefe** vereinheitlichen (Placement-Review zuerst).
- R-3 (Referenz-Repair als eigener Graph-Knoten), R-8 (Facetten-Taxonomie feiner als Konsumenten:
  maschinell zählt v. a. hart↔weich via Checker C3), R-13 (Kontext-HINWEISE in Adjudikation, ohne
  Extraktions-Einfluss), R-4-Rest (READMEs 04–08).

## 6. Endzustand & Zahlen

- **Core `state/core/project-state.json`: 192 Items** (90 requirement, 45 architecture, 14 feature,
  30 pbi) + 39 `implemented_by_issue`-Relationen; 4+ History-Snapshots; jede Mutation autorisiert
  (Adjudikation, L3-Review, 2× Backlog-Review, Tor-1/Placement/Tor-3-Reviews) und per Audit belegt.
- **GitHub `armiino/Agentic-GitHub-refactor`: 39 deutsche Issues** (#1–#39), davon #32/#33 per
  Betriebszyklus aktualisiert; bewusst NICHT synchronisiert: 3 Meta-PBIs (039–041) + Dunkelmodus (042).
- Provenance-Kette durchgängig: Issue → PBI → Requirement → adjudizierter Claim → Transkript-Zitat.
- Vergleich Alt/Neu: 144 → 159 (Bootstrap) → 192 Items; strukturgleiche Zusammensetzung, diesmal
  vollständig gate-autorisiert; Alt-Core als Vergleichsmaterial erhalten.
- Verifikations-Netz während des Tages durchgehend grün gehalten: Build 0/0, **80/80 Tests**
  (+ UnusedUnitCompareRepairTests, JsonCoreRepositoryHistoryTests neu), Smoke 14/0 (wo Core vorhanden).

## 7. Methodische Learnings (Thesis-relevant)

1. **Vorab-Erwartungen machen den E2E zum Experiment:** E1–E10 schriftlich vor dem Lauf → jede Abweichung
   wurde Erkenntnis statt Anekdote (E2 besser, E10 schärfer als erwartet).
2. **Gestaffelte Gates wirken — und Blanko-Akzepte propagieren:** Die 3 Meta-Claims passierten zwei
   durchgewunkene Gates und wurden erst am letzten (Tor 3, skip) gestoppt. Gate-Disziplin ist
   Autor-Disziplin; UI-Kontext-Armut (R-20) begünstigt Durchwinken.
3. **Toter Code ist nur durch echtes Ausführen falsifizierbar:** R-21/R-22 lagen unter Smoke- und
   Dry-Run-Radar. Der „schwere Beweis" ist als Methode gerechtfertigt.
4. **Deterministisch schlägt agentisch bei mechanischen Massen-Ops:** Initial-Sync (43 CREATEs) scheiterte
   am Agenten (Output-Grenze + Repair-Ersetzen), gelang deterministisch aus den Payloads in Minuten.
5. **Kontextfreie Front + Kontext an den Toren** bestätigt (Arm-B-Treue bleibt), aber: Tor 1 muss der
   VOLLSTÄNDIGE Kontext-Punkt werden (arch! R-11), sonst fragmentiert Wahrheit.
6. **Read-before-Write gilt auch für Tor 3:** ohne frischen Snapshot kein sinnvoller Update (nur Drift-Flag).
7. Fehler-Sichtbarkeit ist Infrastruktur: ein verschlucktes Fehler-Event kostete zwei Fehlversuche und
   eine Fehldiagnose (Race), der Ein-Zeilen-Shell-Fix machte die echte Ursache in Sekunden sichtbar.

## 8. Nächste Schritte

1. Commits (Bugfix-Paket + Doku + State) — ausstehend, macht der Autor.
2. Schärfungs-Wellen nach Prioritität: R-11 (Architektur-Design-Entscheid) → Tor-3-Qualität
   (R-15/R-17/R-23) → R-10-Race-Debug → R-20/R-7 UI → R-14 Decision-Takt.
3. PAT `GITHUB_AGENTIC_REFACTOR_TOKEN` nach Abschluss der Thesis-Läufe rotieren.
