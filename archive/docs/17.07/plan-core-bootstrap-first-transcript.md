# Plan — core-bootstrap-first-transcript (Start bei null: erstes Transkript, noch kein Core)

Status: **PLAN / DESIGN.** Noch nicht gebaut. Schliesst die in `TODO.md` benannte Lücke: der lebende Core hat einen
sauberen **Delta-Modus** (ab dem 2. Transkript), aber keinen orchestrierten **Start-bei-null-Pfad**.

Muster wie überall: deterministische Orchestrierung + klare Modus-Erkennung; die agentischen Teilschritte behalten
ihre bestehenden HumanReview-Gates. Core NUR über den `ICoreRepository`-Port.

---

## 1. Warum nötig (nicht nur „was")

**Problem (im Code sichtbar).** `ingest-requirements` bricht mit „Core fehlt - erst 'core-seed' fahren" ab. Die
gesamte Core-/Ingestion-/Tor-Logik IST der Delta-Modus — sie setzt einen bestehenden Core voraus. Für das *erste*
Transkript gibt es keinen orchestrierten Einstieg; man muss 5 Schritte manuell und in der richtigen Reihenfolge
verketten (project-state-build → core-seed → core-baseline → l4-re-clarify → core-seed-backlog).

**Relevanz.** Der ganze Beleg „lebender Backlog von messy Transkript bis GitHub" beginnt beim *ersten* Transkript.
Ohne runden Start-bei-null ist die Kette vorne offen: die Demo muss von Hand gebootet werden, und ein Fehler in der
Reihenfolge (z.B. core-baseline vor core-seed) führt zu unklaren Folgefehlern. Für die Thesis ist „reproduzierbar von
null" genauso wichtig wie „reproduzierbar im Delta".

**Timing.** Jetzt ideal: Delta-Modus (Ingestion) + Tor 2 + Tor 3 stehen und sind belegt. Der fehlende Baustein ist
klein und rein orchestrierend — die Fach-Bausteine existieren alle bereits (siehe §3). Es ist der letzte offene
Rand der Kern-Kette.

**Trade-off / Discernment (Thesis-relevant).** Der erste Lauf braucht den Requirement-Resolver NICHT: es gibt keine
historische Wahrheit, gegen die er RESTATE/REFINE/SUPERSEDE entscheiden könnte. `core-seed` hebt schlicht alle validen
Requirements als NEW mit frischer Core-ID — **deterministisch, kein Agent**. Das ist ein sauberer Discernment-Punkt:
Bootstrap = deterministisch (es gibt nichts zu „verstehen"), Delta = Resolver-Agent (Bedeutung gegen Historie). Genau
das sollte der Command sichtbar machen, statt den Resolver mit einem `--bootstrap`-Sonderfall zu überladen.

**Wenn wir es NICHT bauen:** die Kette bleibt vorne un-orchestriert; jeder Erst-Lauf ist fehleranfällige Handarbeit,
und der „von null"-Beleg fehlt.

## 2. Was der Command ist

Ein **deterministischer Orchestrator mit Modus-Erkennung**, der die vorhandenen Bausteine in der richtigen Reihenfolge
verkettet, validiert und klar meldet — NICHT ein neuer Fach-Mechanismus.

```text
Kein Core        -> Bootstrap fahren (dieser Command)
Core existiert   -> abbrechen mit Hinweis: nutze den Delta-Modus (ingest-requirements)
```

## 3. Datenlage — die Bausteine existieren alle (verifiziert)

```text
project-state-build <artifact.json>... [--l3-run <run|path>] [--out] [--project-id]   -> project-state.json
core-seed <project-state.json>          -> Core (Requirements + Architektur, alle NEW, frische Core-IDs)   [det.]
core-baseline [--out]                   -> L4-Baseline-Triplet aus dem Core                                 [det.]
l4-re-clarify <baseline> ... (+ review/apply, backlog-review/apply)  -> Cluster -> PBIs   [AGENTISCH + HumanReview]
core-seed-backlog <l4-re-clarify-run>   -> Core (Features + PBIs)                                           [det.]
```

**Wichtig (Design-Konsequenz):** der Abschnitt `l4-re-clarify` ist **agentisch UND enthält HumanReview-Gates** in der
Mitte. Ein einziger vollautomatischer Command kann diese Gates nicht ohne Menschen durchlaufen, ohne das
Human-in-the-Loop-Prinzip zu verletzen. Deshalb wird der Bootstrap in eine **deterministische Vorstufe** (voll
orchestrierbar) und eine **agentische Backlog-Stufe** (behält ihre Gates) getrennt.

## 4. Aufbau (zwei Stufen, damit die Human-Gates erhalten bleiben)

```text
STUFE 1 — core-bootstrap-first-transcript <artifacts|--l3-run>   [DETERMINISTISCH, ein Command]
  0. Modus prüfen: Core existiert? -> Abbruch + Hinweis (Delta-Modus). Sonst weiter.
  1. project-state-build (aus L1-L3-Artefakten bzw. --l3-run) -> project-state.json (in den Run-Ordner).
  2. core-seed <project-state.json> -> initialer Core (Requirements/Arch, alle NEW).
  3. core-baseline -> L4-Baseline-Triplet.
  4. Validieren + klar melden: Core-Item-Zahl, Baseline geschrieben, NÄCHSTER Schritt = die agentische Backlog-Stufe.

STUFE 2 — bestehende Kette, unverändert (agentisch, mit Human-Gates):
  l4-re-clarify <baseline> -> l4-re-clarify-review -> l4-re-clarify-apply
  -> l4-re-clarify-backlog-review -> l4-re-clarify-backlog-apply
  -> core-seed-backlog <l4-re-clarify-run>   -> Core enthält jetzt Features + PBIs
  Ab hier: normaler Delta-Modus (ingest-requirements funktioniert, weil Core existiert).
```

Optional (B2, Komfort): ein dünner `core-bootstrap-backlog <l4-re-clarify-run>`-Wrapper um `core-seed-backlog` mit
Validierung/Meldung — nur wenn sich Handhabung als lästig zeigt; sonst reicht das bestehende `core-seed-backlog`.

## 5. „Alles als NEW" — warum kein Resolver-Sonderfall

Der erste Lauf hat keine historische Wahrheit. `core-seed` schreibt jedes valide Requirement als frische Core-Entität
(NEW, eigene ID) — genau das gewünschte Bootstrap-Verhalten, **ohne** RESTATE/REFINE/SUPERSEDE (die bräuchten alte
Items). Der Resolver-Agent ist im ersten Lauf also fachlich überflüssig; ihn mit `--bootstrap` zu überladen wäre
schlechteres Design (TODO). Optionaler späterer Ausbau: Dubletten/Widersprüche INNERHALB des ersten Transkripts
erkennen — aber das ist eine Kür, kein Muss für den runden Start.

## 6. Modus-Erkennung (die eigentliche neue Logik)

```text
ICoreRepository.ExistsAsync()
  == true   -> BOOTSTRAP_ABORTED_CORE_EXISTS: "Core existiert (state/core/). Für weitere Transkripte:
               ingest-requirements <meeting-delta>. Bootstrap ist nur für den Start bei null."
  == false  -> Bootstrap Stufe 1 fahren.
```

`core-seed` verweigert ohnehin das Überschreiben eines bestehenden Core (Doppel-Seed bricht ab) — die Modus-Erkennung
im Bootstrap macht das nur früh und mit klarer Meldung, statt mitten in der Kette.

## 7. Abgrenzung (bewusst NICHT in diesem Command)

- **Transkript → L1-L3-Artefakte** (Ledger/Extraction/Adjudikation): eigener, schwererer Upstream (P1-Machbarkeits-
  Fund). Der Bootstrap startet bei den **bereits erzeugten** L1-L3-Artefakten bzw. einem `--l3-run`. Der volle
  Transkript-Start bleibt separater Capstone.
- **Die agentischen Backlog-Gates** werden NICHT auto-akzeptiert (Human-in-the-Loop bleibt).
- **Re-Bootstrap/Überschreiben** eines vorhandenen Core: bewusst abgelehnt (Delta-Modus ist der Weg); ein
  ausdrückliches Reset wäre ein separater, gefährlicher Command — hier out of scope.

## 8. Bauschritte

```text
B1  [DONE] core-bootstrap-first-transcript <artifacts…> [--l3-run] [--project-id] (core/CoreBootstrapRunner):
    Modus-Erkennung (Core da -> BOOTSTRAP_ABORTED_CORE_EXISTS) + Stufe-1-Orchestrierung (project-state-build ->
    core-seed -> core-baseline) + Next-Step-Meldung. Wiederverwendung der Runner, kein Fach-Code. runs/bootstrap/<id>/.
B2  (optional, offen) core-bootstrap-backlog <l4-re-clarify-run>: dünner Validierungs-Wrapper um core-seed-backlog.
B3  [DONE, mit B1 belegt] Bootstrap-Test: state/core/ entfernt -> B1 mit recipe-Artefakten -> Core items=93/req=57,
    Modus kippt (ingest-requirements läuft, core-req=57). Committeter Core (144) danach restauriert. Build grün.
```

## 9. MAF-Nativität / Determinismus

Rein deterministische Orchestrierung (keine neuen Agenten) — der Bootstrap ist genau der Fall „deterministischer
Mechanismus reicht" (kein Verstehen nötig, es gibt keine Historie). Die agentische Bedeutungsarbeit sitzt bewusst
erst in Stufe 2 (Cluster/Clarify, mit Gates) und danach im Delta-Modus (Resolver). Konsistent mit dem Rest:
Agent nur, wo Bedeutung regiert.

## 10. Offene Entscheidungen

- **E-A — Input-Form:** L1-L3-Artefakt-Dateien direkt vs. `--l3-run <runId>` (project-state-build kann beides).
  *Empfehlung:* beides durchreichen (project-state-build unterstützt es schon).
- **E-B — Stufe 2 verketten?** Stufe 1 stoppt und meldet die nächsten (agentischen) Schritte *(Empfehlung)* vs.
  Stufe 1 ruft l4-re-clarify direkt auf und stoppt am ersten Human-Gate. *Empfehlung:* stoppen + melden (klarer,
  keine halb-durchlaufene Agent-Kette).
- **E-C — Run-Ordner:** eigener `runs/bootstrap/<runId>/` mit project-state.json + baseline *(Empfehlung)* vs.
  in state/core/ schreiben. *Empfehlung:* Run-Ordner für Audit, Core nur über den Port.
