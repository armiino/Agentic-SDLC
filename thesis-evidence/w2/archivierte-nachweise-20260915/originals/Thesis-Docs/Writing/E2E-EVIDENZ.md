# E2E-EVIDENZ — Gesamtsystem-Nachweis (Evaluationsschichten 2–4)

> Status: MOMENTAUFNAHME 06.09.2026 (v3) — Gegenstück zu `W2-ERGEBNISSE.md`.
> VERWENDUNG: Evidenz-INVENTAR fürs Kapitel — die vier Schichten sind KEINE Kapitelgliederung
> (Kapitelstruktur = Schreibblaupause 7.1–7.12; Reibungs-Log/MAF-Erfahrungen gehören primär
> in Kapitel 4/6 und die Diskussion).
> Konsolidiert: technische Machbarkeits-/Integrationsdemonstration (E2E), formative Evaluation
> (Reibungs-Log), deterministische Integritäts-/Provenienzprüfungen, Framework-Exploration.
> Primärbelege: `Thesis-Docs/aktiv/E2E-RUNBOOK.md` · `state/core/project-state.json` ·
> `runs/e2e-evidenz/traceability-audit.json`.

## 0 · Evidenzklassen (jede Aussage unten trägt eine)

**[B]** kontrollierter Benchmark · **[VA]** deterministischer Voll-Audit · **[SP]** deterministische
Stichprobe · **[D]** skriptierte/exemplarische Demonstration · **[F]** formative Beobachtung.

## 1 · Einordnung: vier Schichten, getrennte Evidenzstärken

Schicht 1 (`W2-ERGEBNISSE.md`) = kontrollierter Konfigurationsvergleich [B]. Die Schichten hier
belegen technische Machbarkeit, Integration und ausgewählte Systemeigenschaften — NICHT
fachliche Wirksamkeit, Qualität oder Generalisierbarkeit. Mapping zur Protokoll-Systematik:
Schicht 1 = E2 (+E3-Befund L≡LCR) · Schicht 2 = E1 + exemplarische Anteile von E5/E6 ·
Schicht 3 = formative Evidenz + ausgeführte E4-Anteile · Schicht 4 = Framework-Charakterisierung.

## 2 · Schicht 2 — Exemplarische E2E-Demonstration (Machbarkeit + Integration)

| Aussage | Klasse | Beleg |
| --- | --- | --- |
| Der Pfad leeres Projekt → 2 Transkripte → Core → 39 deutsche GitHub-Issues inkl. Update-Zyklus (#32/#33) ist AUSFÜHRBAR; jedes erzeugte Issue trägt Herkunfts-Referenzen | [D] | E2E-RUNBOOK Beleg-Tabelle (23.07., alle RunIds) |
| Betriebs-Inventar (Umfang, KEIN Qualitätsnachweis): 237 autorisierte Items (107 REQ · 50 ARCH · 49 PBI · 16 FEAT · 15 DEC), 71 Proposals, 515 typisierte Relationen (43 `implemented_by_issue`) | [D] | `state/core/project-state.json` |
| Steward-Betrieb mit Real-Writes (gh#43–45), Wiedervorlage/ADOPT_NEW, Zwei-Bahnen | [D]/[F] | Block-K-Testplan (~14 Live-Läufe, 17.–18.08.) |
| **Einordnungsfall (A7, vollständiger Zyklus):** Eingang mit Widerspruch → Tor-1 CONTRADICT → Decision geprägt → menschlicher ADOPT-Entscheid am decision-gate → REQ-78 supersedes REQ-32 → Align-Lauf zieht Projektionen nach | [D] | Run `20260804_121433_eb181a` (R-14-Live-Zyklus) |
| Ein-Graph resume-fähig im GETESTETEN Fortsetzungspfad (Kapsel-Sub-Run, Core SHA-identisch restauriert) — allgemeine Durabilität und doppelte externe Writes UNGETESTET | [D] | Echt-Lauf `144056_dc711f` |

## 3 · Schicht 2b — Traceability-Audit v2 (deterministisch; NUR explizite Kanten)

Skript `tools/eval/w2_traceability_audit.py` (v2 nach Review-Reparaturplan: scope-gebundene
Definitionen, Snapshots/Checkpoints exkludiert, richtungsgebundene Relationen, VOLLES
normalisiertes Zitat gegen das RUN-SPEZIFISCHE Transkript, 9 Pflicht-Negativtests grün (T1–T9, inkl. Consumable-Hop und Closure-Tiefe),
Reproduzierbarkeits-SHAs im JSON):

- **[VA] Voll-Audit v3, alle 237 Items** (letzter Consumable-Hop geschlossen [Zweitprüfer-Fund],
  Kanten nur aus config/run-report, Mehrdeutigkeit → offen, alle 237 Einzelergebnisse in der
  JSON; 9 Negativtests grün): Von 237 Core-Items waren **137 bis zu einem vollständigen Zitat
  im run-spezifischen Transkript** und weitere **38 bis zu einem referenzierten
  Definitionsobjekt** maschinell auflösbar; bei **62 Items blieb der Provenienzpfad offen.**
  Die offenen Fälle verteilen sich auf UNTERSCHIEDLICHE Herkunftsklassen (u. a. 24 re-clarify,
  11 Extracted, Autor-/Analyst-/Inbound-Herkünfte) und zeigen unvollständig materialisierte
  Provenienzkanten (Befund R-74): A (21) = keine Referenz-Felder am Item · B (41) = Referenz
  vorhanden, Definitions-Kante nicht in den Run-Artefakten verzeichnet.
- „Jedes Item rückverfolgbar" wird NICHT behauptet; semantische Stützung ist von der
  referenziellen Auflösung getrennt und nicht Teil dieses Audits.
- **Scope-Trennung zu W2:** Die W2-Aussage „vollständige formale Provenienz" gilt für den
  LEDGER-INTERNEN Bestand der ausgewerteten Läufe (Reference Validity 1.0); DIESER Audit misst
  die kern-weite Kettenauflösung über alle Eingangspfade (Ledger, Gates, Analyst, Inbound,
  re-clarify) — die 62 offenen Pfade widersprechen der W2-Aussage daher nicht.

## 4 · Schicht 3 — Formative Evaluation + deterministische Integrität

- **[F] Reibungs-Log R-1 … R-74:** jede Reibung mit Beleg-Run, Mechanik/Modell-Trennung,
  Fix-Status — informiert die Effizienz-Diskussion; misst KEINE Produktivität/Usability.
- **[VA] CoreKangal:** EINE Prüf-Logik vor jedem Save (I1/I5 = Abbruch); der AKTUELLE Bestand
  prüft 0 Fehler / 0 Warnungen. (Aussage über den Bestand, nicht über alle historischen Saves.)
- **[D] Snapshot-Historie:** 277 Core-Snapshots vorhanden; eine Reconciliation „jede Mutation ↔
  ein Snapshot" wurde nicht durchgeführt (offene Grenze).
- **[F] Governance:** Maker→Gate→HumanReview→Apply für die neun INVENTARISIERTEN
  Anwendungsschreibpfade; 10 Gates; `accept-all`/`replay` als deklarierte Experiment-Modi.
- **[VA] Fehlerklassen-Matrix (06.09., `runs/e2e-evidenz/fehlerklassen-matrix.md`):** Die
  BESTEHENDE Test-Suite deckt den Wächter-/Challenge-Katalog deterministisch ab — Beleg-Lauf
  21/21 grün (Kangal-Angriffe inkl. „SaveAsync wirft bei I1 und schreibt NICHTS";
  Ledger-Fehlerklassen inkl. Lauter-Terminal und ehrlichem Downgrade). Ehrliche Grenzen in der
  Matrix: Verstärkungs-/Dispositions-Prüfung = LLM-Stufen; Detailverlust in verwendeten Units =
  bekannte blinde Klasse (in W2 vermessen); Repair-WIRKSAMKEIT nur exemplarisch. NEU §B2: die
  kompakte Governance-Challenge (ungültige Relation · Snapshot bei gültiger Mutation ·
  idempotenter Save · idempotente Projektionen · Reject-Begründungspflicht · Stale-Snapshot-
  Wächter) via bestehende Tests, Beleg-Lauf **36/36 grün**; „Writer-Bypass"-Angriff bleibt
  architektonisch begründet, nicht getestet. Adversariale Injection in LAUFENDE Pipelines
  wurde nicht gefahren (Grenze).

## 5 · Schicht 4 — Framework-Exploration (MAF) [F]

MAF-Feature-Matrix (Agents, Workflows, RequestPorts, Checkpoints/Resume, Middleware, MCP,
Memory, Handoff, Structured Output — real erprobt; Befunde u. a. Spike R-38, „dosierte
Agent-Form"). Orchestrierungs-Vergleich Graph (pipeline-full) vs. Agent (Steward) als
qualitativer Erfahrungs-Vergleich (⚖ 07.08.: bewusst kein Zahlen-Benchmark).
Werkbank: 668 Tests · Smoke 14/0.

## 6 · Gesamtbild

| Schicht | Evidenzklasse(n) | Datei/Beleg |
| --- | --- | --- |
| 1 · W2-Vergleich | [B] | `W2-ERGEBNISSE.md` |
| 2 · E2E-Demonstration | [D] + [VA]/[SP] Traceability | dieses Blatt §2–3 |
| 3 · Formativ + Integrität | [F] + [VA] Kangal | dieses Blatt §4 |
| 4 · Framework | [F] | dieses Blatt §5 |

## 7 · Limitationen

Schichten 2–4 demonstrieren und prüfen deterministisch — sie vergleichen nicht; Überlegenheit
wird daraus nicht abgeleitet. Nicht ausgeführt und als Grenze sichtbar: adversariale
Live-Pipeline-Injection und Writer-Bypass-Test (die deterministische Fehlerklassen-/
Governance-Challenge selbst IST ausgeführt, s. §4), Durabilität jenseits des getesteten
Resume-Pfads, Mutations-Snapshot-Reconciliation, semantische Stützungs-Stichprobe des Traceability-Audits, aktueller
Golden-Path-Lauf auf dem finalen Codestand (historische E2E-Läufe sind formativ/exemplarisch).
R-74 dokumentiert die unvollständig materialisierten Provenienz-Kanten später
Herkunftsklassen. Zu A8 (Projektion/Außenkopplung) ist strukturell belegt: Idempotenz,
Repository-Zugehörigkeits-Guard, Stale-Snapshot-Ablehnung, ein konkreter Kommentar-Rückweg
(Block K); NICHT untersucht sind die SEMANTISCHE Treue aller Projektionen und die
VOLLSTÄNDIGE Erkennung externer Änderungen.
