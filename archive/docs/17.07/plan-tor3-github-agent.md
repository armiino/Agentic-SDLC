# Plan — Tor 3: GitHub-Agent (Forward-Reconciliation + Reverse Feedback-Ingestion)

Status: **PLAN / DESIGN.** Noch nicht gebaut. Hebt den fruehen Vorschlag `Github-Agent.md` auf den **1c-Stand**:
der Agent bekommt jetzt NICHT mehr den ganzen Backlog und muss raten, was sich geaendert hat — er bekommt das
**kleine, stabile github-sync-Delta** aus 1c-3. Damit schliesst Tor 3 den urspruenglichen Anlass end-to-end:
neues Meeting → Core → lebender Backlog → **kleines GitHub-Delta → keine Duplikat-Issues**.

`Github-Agent.md` bleibt gueltig fuer Sicherheit/Autoritaet/Vergleichspfad; dieser Plan aktualisiert **Input**
(Delta statt Voll-Backlog) und ergaenzt die **Rueckrichtung** (GitHub-Feedback-Ingestion, E4).

**Rev 2 (2. Review):** CREATE nur nach ausgefuehrter GitHub-Suche ohne Treffer (unmapped-Fall ist agentisch, nicht
deterministisch — Anti-Duplikat); UPDATE_ISSUE = vorgeschlagener Patch/Kommentar, kein Body-Overwrite (§4).
**Rev 3 (Gate-Invariante):** CREATE_ISSUE ohne `searchedQueries[]`/`searchEvidence` wird vom Gate abgelehnt
(`CREATE_WITHOUT_SEARCH_EVIDENCE`) — die Anti-Duplikat-Regel ist deterministisch erzwungen, nicht nur im Prompt (§4).

---

## 0. Was sich gegenueber Github-Agent.md aendert

```text
FRUEHER (Github-Agent.md):  Agent bekommt Voll-Backlog + GitHub, muss selbst herausrechnen, was neu/geaendert ist.
JETZT (post-1c):            Agent bekommt github-sync-Delta (newPbis/updatedPbis + Status) + Mappings + GitHub.
                            Die fachliche Delta-Berechnung ist SCHON passiert (Ingestion + 1c-3).
```

## 1. Ziel

Den github-sync-Delta operativ gegen GitHub abgleichen — kontrolliert, ohne Duplikate, ohne dass GitHub still zur
Wahrheit wird. Beweis: Sprint-Re-Run erzeugt keine 27 neuen Issues, sondern nur die wenigen Delta-Operationen.

## 2. Input (post-1c)

- **github-sync-Delta** (`runs/pbi-update/<run>/plan/applied/github-sync-delta.json`): `newPbis`, `updatedPbis`,
  `entries[]` (pbiId, title, status, readiness, coveredRequirementIds, blockedByOpenDecision, githubIssue).
- **github-sync-view** des Core (`core-view github-sync`): bestehende `pbi ↔ issue`-Mappings (Basis fuer Dedup).
- **GitHub live** (read): Issues/Labels/Kommentare (MCP-read ODER schmaler Client als Agent-Tools).

## 3. Zwei Richtungen (beide gehoeren zu Tor 3)

```text
FORWARD  Core-Delta -> GitHub   : reconcile -> CREATE/UPDATE/COMMENT/NO_CHANGE/FLAG_DRIFT (gated Write).
REVERSE  GitHub -> Core         : GitHub-Feedback-Ingestion — geschlossen/verifiziert kommt NUR als GEPRUEFTER
                                  StateChange in den Core (nie auto). done entsteht hier (E4).
```

## 4. Forward — Hybrid (deterministischer Vorfilter + Agent fuer den Rest)

```text
DETERMINISTISCH (billig, sicher) - NUR der eindeutige Mapping-Fall:
  PBI hat Mapping (pbi -> #n) + Issue existiert -> updatedPbi: UPDATE_ISSUE-Vorschlag (s.u.) ; newPbi w/ Mapping -> NO_CHANGE.
  PBI blocked_by_decision -> KEIN CREATE (HOLD) + FLAG (wartet auf Tor 2 Unblock); bestehendes Issue -> COMMENT/label "blocked".
AGENTISCH (jedes PBI OHNE Mapping - KONSERVATIV, Anti-Duplikat):
  Erst GitHub SUCHEN. Gibt es einen plausiblen (auch menschlich angelegten) Kandidaten -> LINK (semantisch zuordnen).
  CREATE_ISSUE NUR, wenn die Suche tatsaechlich AUSGEFUEHRT wurde und KEINEN plausiblen Kandidaten liefert.
  Drift: geschlossenes Issue, dessen PBI aktiv/needs_clarify ist -> FLAG_DRIFT.
```

> **Rev 2 (Review-Schaerfung):** „kein Kandidat -> CREATE" ist NIE ein Default. CREATE ist nur zulaessig nach
> AUSGEFUEHRTER Suche ohne plausiblen Treffer — sonst drohen wieder Duplikate. Deshalb ist der unmapped-Fall
> agentisch (Suche + Urteil), nicht deterministisch.
>
> **Rev 3 (Gate-Invariante):** Die Anti-Duplikat-Regel ist nicht nur Prosa, sondern eine **harte Gate-Bedingung**:
> jede `CREATE_ISSUE`-Operation MUSS `searchedQueries[]` (die tatsaechlich ausgefuehrten Suchbegriffe) + eine
> `searchEvidence` (Ergebnis-Zusammenfassung „keine plausiblen Treffer") mitfuehren. Fehlt der Beleg -> Gate
> `CREATE_WITHOUT_SEARCH_EVIDENCE` -> Operation abgelehnt. Damit kann der Maker keinen CREATE ohne ausgefuehrte
> Suche durchschmuggeln — die Regel ist deterministisch erzwungen, nicht nur im Prompt erhofft.

Muster: **Maker (Agent, Umwelt A=Delta/Mappings + Umwelt B=GitHub-read) → deterministisches Gate → HumanReview →
gated Write.** KEIN autonomer Write (extern + kaum reversibel; LLM-Varianz darf nicht auf create/close sitzen).

Operationen: `CREATE_ISSUE | UPDATE_ISSUE | COMMENT | LINK | NO_CHANGE | FLAG_DRIFT | HOLD_BLOCKED`.
- **UPDATE_ISSUE = vorgeschlagener PATCH/Kommentar, KEIN automatisches Titel/Body/Status-Ueberschreiben** (Rev 2).
  Bei manuell bearbeiteten Issues bevorzugt COMMENT statt Body-Rewrite; Gate/HumanReview entscheidet die Form.
- **CREATE_ISSUE = nur mit `searchedQueries[]` + `searchEvidence`** (Rev 3, Gate-Invariante) — sonst
  `CREATE_WITHOUT_SEARCH_EVIDENCE` -> abgelehnt.
- Beleg-Pflicht: jede MATCH/UPDATE/LINK nennt den Anker (pbiId + Mapping ODER pbiId + Requirement-IDs im Issue-Body).

## 5. Mapping-Persistenz (schliesst die alte Luecke)

Das `pbi ↔ issueNumber`-Mapping wird **im Core** gehalten (nicht mehr nur Run-Artefakt): Relation
`pbi --implemented_by_issue--> #n` + Metadata (issueNumber, url, operationalStatus open/closed). Basis fuer den
naechsten Delta-Lauf (Dedup) und fuer die github-sync-view. Schreiben ueber den Core-Port (deterministischer Apply).

## 6. Reverse — GitHub-Feedback-Ingestion (E4)

GitHub-Zustand wird NIE automatisch Wahrheit. Ein Rueckkanal-Lauf liest GitHub-Events (Issue geschlossen, Label,
Kommentar) und erzeugt einen **geprueften StateChange** — gleiches Muster wie Ingestion:

```text
GitHub-Event -> Maker (schlaegt StateChange vor: z.B. PBI done? Requirement verifiziert?) -> Gate -> HumanReview
             -> Apply (Core: PBI status=done NUR nach Verifikation/Freigabe; History; Mapping-Status=closed).
```

`done` entsteht ausschliesslich hier (geschlossenes Issue + Verifikation/Freigabe), nie aus issue-closed allein (E4).

## 7. Autoritaet / Regeln (aus Github-Agent.md §7, unveraendert gueltig)

- Core = fachliche Wahrheit; GitHub = operative Projektion.
- geschlossenes/verwaistes Issue -> FLAG, nie stille Wahrheits-Aenderung.
- Write NUR nach Gate + HumanReview; MCP-read breit ok, MCP/Client-write ausschliesslich ueber den gated Apply.

## 8. Tools

```text
GitHub-read (Agent-Tools):  list_issues, get_issue, search_issues, get_comments, get_labels
   -> Variante A: GitHub-MCP-Server (MAF-nativ)   Variante B: vorhandene GithubRestIssueClient-Read-Ops als AIFunction
Core-Sync-View:             get_sync_delta, get_pbi, get_existing_mappings, get_pbi_traceability
Plan:                       check_github_action_plan (det. Vorpass), save_github_action_plan (einmal)
Write (gated Apply):        create/update/comment/close  -> NUR ueber den kontrollierten Apply, NICHT im Maker.
```

## 9. Vergleichspfad (agentisch vs deterministisch) — jetzt auf dem sauberen Delta

Der Vergleich aus Github-Agent.md §8 wird auf dem **kleinen Delta** viel sauberer messbar: beide Pfade erzeugen
dasselbe `GithubActionPlan`-Schema; Drift-Fixtures (Paraphrase, entfernte Requirement-IDs, geschlossenes Issue bei
aktivem PBI) zeigen, wo der Agent den deterministischen Matcher schlaegt (Dedup-Recall) und wo Determinismus reicht.

## 10. MAF-Nativitaet / Determinismus

Agentisch: der Forward-Maker (semantische Zuordnung zu menschlichen Issues, Drift) + der Reverse-Feedback-Maker.
Deterministisch: Vorfilter (Mapping-Dedup), Gate, gated Write-Apply, Mapping-Persistenz, Status-Uebergaenge.
Konsistent mit dem Rest: Agent nur wo Bedeutung regiert.

## 11. Bauschritte

```text
T3.1  [DONE] Mapping als Core-Relation (pbi implemented_by_issue gh#n) + Persistenz ueber den Port.
             CoreGithubMapping (idempotenter Upsert Link/Close/Reopen/Unlink) + github-map-CLI + github-sync-View
             angebunden (githubIssueStatus). Beleg: iteration-notes-ingestion-core.md „Tor 3 / T3.1".
T3.2  [DONE] GitHub-read als Agent-Tools (Variante B/Snapshot: GithubIssueQueries + GithubReadTools +
             github-read-CLI; list/get/search/labels; get_comments zurueckgestellt = MCP/Live-Client). Reproduzierbar
             ueber github-snapshot; MCP dokumentierte Alternative. Beleg: iteration-notes „Tor 3 / T3.2".
T3.3  [DONE] Forward-Maker (Agent) + deterministischer Vorfilter -> GithubForwardPlan; Gate (inkl. Rev-3-Invariante
             CREATE_WITHOUT_SEARCH_EVIDENCE); HumanReview-Adapter. tor3/GithubForward{Seed,Tools,Gate,Runner,
             ReviewAdapter,ReviewRunner} + GithubForwardAgent-Prompt; CLI github-forward / github-forward-review.
             Seed/Coverage/Review deterministisch belegt; agentischer LINK/CREATE + Rev-3 e2e = mit T3.4-Lauf.
T3.4  [DONE] Gated Write-Apply (create/update/comment/link) + Mapping-Update im Core. Safe-by-default (Dry-Run ohne
             --execute); Rev-3 am Write-Rand nochmals erzwungen; CreateCommentAsync (Rev 2). tor3/GithubForwardApply*
             + CLI github-forward-apply. Dry-Run deterministisch belegt; echter --execute = autor-getrieben.
T3.5  [DONE] Reverse GitHub-Feedback-Ingestion (Detektor->Gate->Human->Apply; done nur hier, E4). Deterministischer
             Zustandsdiff (PBI_DONE?/Mapping-Sync/Drift); Gate-Invariante requiresVerification; done nur nach Freigabe
             + History + Mapping-Close (T3.1). tor3/GithubReverse* + CLI github-reverse{,-review,-apply}. E4 Test A/B
             belegt. Agentische Kommentar-Deutung offen (braucht get_comments, Live/MCP).
T3.6  [DONE] Vergleichspfad-Harness (agentisch vs deterministisch) auf Drift-Fixtures. GithubForwardDeterministicMatcher
             + GithubForwardCompare (DedupRecall/Precision) + CLI github-forward-compare [--plan] + drift-fixtures.json.
             Belegt: det. dedupRecall 0.333 (precision 1.0) vs agent-sim 1.0 (Δ +0.667). Echter Agent-Plan = --plan.
T3.7  [DONE] Test: Sprint-Re-Run -> nur Delta-Operationen, KEINE Duplikat-Issues. GithubForwardRerunTest + CLI
             github-forward-rerun-test. Belegt: Pass1 create=3/hold=1, Pass2 (Re-Run) create=0/update=3/hold=1,
             issuesStable, duplicateCreates=0 => PASS. Tor 3 (T3.1-T3.7) deterministisch komplett.
```

## 12. Offene Entscheidungen

- **E-A — blocked_by_decision-PBIs:** HOLD (kein Issue) + FLAG *(Empfehlung)* vs. Issue mit „blocked"-Label.
  (Unblock kommt ueber Tor 2 — solange Tor 2 fehlt, sammeln sich geflaggte/gehaltene PBIs; bewusst so.)
- **E-B — GitHub-read:** MCP-Server vs. schmaler `GithubRestIssueClient` als Tools *(Empfehlung: Client-Tools
  zuerst — kein Extra-Prozess; MCP als dokumentierte Alternative; MAF-vs-custom-Eintrag).*
- **E-C — Mapping-Ort:** Core-Relation `implemented_by_issue` + Metadata *(Empfehlung)* vs. separater Mapping-Store.
- **E-D — `done`-Kriterium (E4):** geschlossenes Issue + explizite Verifikation/Freigabe im Reverse-Review — nie auto.

## 13. Abgrenzung zu Tor 2

Tor 3 setzt `blocked_by_decision` nur als Status voraus und HAELT betroffene PBIs zurueck. Das **Aufloesen** einer
Decision (Unblock) ist **Tor 2** (Decision-Ingestion). Ohne Tor 2 sammelt der Core blockierte PBIs an → Tor 2 ist
die notwendige Ergaenzung, sobald echte Decisions beantwortet werden.

## 14. Erwartete Wirkung (zu belegen)

Sprint-Re-Run mit dem 4-PBI-Delta (Test B, 1c-3): der Agent erkennt bestehende Mappings (NO_CHANGE/UPDATE),
erstellt nur fuer das eine neue PBI ein Issue, haelt das blockierte zurueck → **statt 27 Issues nur wenige, gezielte
Operationen, keine Duplikate.** Reverse: ein manuell geschlossenes Issue wird NICHT still `done`, sondern erst nach
geprueftem Feedback-StateChange.
