# Plan — Tor 2: Decision-Ingestion (Stakeholder beantwortet Open Decision → Core auflösen → entblocken)

Status: **PLAN / DESIGN.** Noch nicht gebaut. Dritter und letzter Ingress-Tor auf den lebenden Core, neben
Tor 1 (Meeting → Requirements, gebaut) und Tor 3 (GitHub → Status, T3.1–T3.7 gebaut).

Muster wie überall: **Maker → deterministisches Gate → HumanReview → deterministischer Apply**, Core NUR über den
`ICoreRepository`-Port + Audit-Snapshot.

**Rev 2 (Review-Schärfung, übernommen):** (1) **E-B fest**: KEEP_ORIGINAL → PBI `active`, ADOPT_NEW/REFINE → PBI
`needs_clarify` (geänderte Wahrheit ⇒ Zuschnitt/Kriterien/Coverage neu prüfen). (2) **Supersede-Reuse verbindlich**:
ADOPT_NEW nutzt den bestehenden `pbi-update`-SUPERSEDE-/Swap-Mechanismus (wiederverwenden ODER sauber extrahieren) —
**kein zweiter, leicht anderer Wahrheitsübergang**. (3) **Widerspruch nachvollziehbar auflösen**: die `contradicts`-
Relation wird NICHT hart gelöscht, sondern geschlossen/aufgelöst; die Entscheidung bleibt belegt (DEC `resolved` +
History mit vorheriger `contradicts`-Beziehung + `resolutionOutcome` = KEEP_ORIGINAL/ADOPT_NEW/REFINE).

---

## 1. Warum Tor 2 nötig ist (nicht nur „was")

**Problem (konkret, schon im Code sichtbar).** Der Core sammelt **offene Entscheidungen** an, aber nichts löst sie
auf. Zwei Quellen erzeugen sie:
- Tor 1 CONTRADICT → Item `DEC-<n>` (`itemType="decision"`, `status="open_decision"`) + Relation
  `DEC --contradicts--> requirement` (IngestionApply).
- Re-Clarify/L4 → PBIs mit `openDecisionRefs` (Stakeholder-Entscheidungen, die vor Umsetzung nötig sind).

Diese Open Decisions **blockieren Arbeit** — auf zwei Wegen:
1. **explizit:** PBI-Status `blocked_by_decision` + `openDecisionRef=DEC-<n>` (aus `BLOCK_PBI`, pbi-update).
2. **abgeleitet:** die `github-sync`-View setzt `BlockedByOpenDecision=true`, wenn ein vom PBI gedecktes Requirement
   Ziel einer `contradicts`-Relation ist (`CoreViews.GithubSync`).

**Relevanz.** Tor 3 reagiert auf beide Blockaden mit `HOLD_BLOCKED` (kein Issue, warten). Das ist bewusst so — aber
**ohne Tor 2 ist es eine Einbahnstraße:** blockierte PBIs stauen sich für immer, die Arbeit erreicht GitHub nie.
Der End-to-End-Anspruch des lebenden Backlogs (neues Meeting → Core → Backlog → GitHub) hat damit ein **Loch genau
dort, wo Stakeholder-Klärung passiert** — dem Kern der frühen SDLC-Phase, um die es in der Thesis geht.

**Timing.** Jetzt ist der richtige Moment: Tor 1 + Tor 3 stehen, die HOLD_BLOCKED-Mechanik ist gebaut und getestet
(T3.3/T3.7), die Blockade-Semantik ist im Code fixiert. Tor 2 ist der **kleinste Baustein, der den Kreis schließt** —
und er macht die schon vorhandenen `HOLD_BLOCKED`/`blocked_by_decision`-Zustände erst sinnvoll (sonst toter Zustand).

**Trade-off / Discernment (Thesis-relevant).** Tor 2 ist der sauberste Ort, die Frage „Agent vs. deterministisch"
erneut scharf zu stellen: das **Deuten** einer freien Stakeholder-Antwort ist echtes Urteil (Agent), das **Entblocken**
der abhängigen PBIs ist reiner Graph-Traversal (deterministisch). Beides in EINEM Tor macht den Schnitt explizit
messbar — statt alles dem Agenten zu geben oder alles zu verdrahten.

**Wenn wir es NICHT bauen:** der Beleg „lebender Backlog schließt den Kreis" bleibt unvollständig; jede Demo mit einem
Widerspruch/einer Stakeholder-Frage endet in einem dauerhaft gehaltenen PBI. Das ist die auffälligste offene Lücke.

## 2. Was Tor 2 ist

Der Ingress-Tor **„Stakeholder beantwortet eine Open Decision → Core-Decision aufgelöst → betroffene Items
aktualisiert/entblockt"**. Spiegelbild zu Tor 1/Tor 3, gleiche Kette, gleiche Sicherheits-Governance.

```text
Stakeholder-Antwort -> Maker (Auflösung deuten: welche Aussage gewinnt?) -> Gate -> HumanReview
                    -> Apply (Core: DEC resolved + Widerspruch auflösen + betroffene PBIs entblocken; History;
                              neues affected-view + github-sync-Delta -> Tor 3 nimmt die PBIs wieder auf)
```

## 3. Input & Datenlage

- **Offene Decision(s):** Core-Items `itemType="decision"`, `status="open_decision"` (+ `contradicts`-Relation,
  Metadata `targetEntityId`).
- **Stakeholder-Antwort:** die Auflösung. Zwei Ausbaustufen (s. §11): T2.1 strukturiertes Resolution-Input
  (`decisionId` + Outcome + optional neue Aussage), T2.2 agentische Deutung eines freien Statements/Transkript-Schnipsels.
- **Abhängige Items (deterministisch ermittelbar):** PBIs mit `openDecisionRef==DEC` (explizit) **und** PBIs, die ein
  vom Widerspruch betroffenes Requirement decken (abgeleitet, über `covers`/`contradicts`) = `CoreViews.AffectedItems`.

## 4. Discernment — agentisch vs. deterministisch

```text
AGENTISCH (echtes Urteil, 4. agentischer Knoten):  die freie Antwort -> welche Auflösung + was sie fürs
                                                    widersprochene Requirement heißt (bestätigt/ersetzt/verfeinert).
DETERMINISTISCH (Graph + Status):                  Blast-Radius berechnen, contradicts auflösen, PBIs entblocken,
                                                    Status-/History-Übergänge, github-sync-Delta.
```

Konsistent mit dem Rest: Agent nur, wo Bedeutung regiert. Deshalb **T2.1 deterministisch zuerst** (Auflösung als
Input), **T2.2 Agent obendrauf** — der Agent ist ein optionaler, messbarer Aufsatz, kein Muss für die Kernmechanik.

## 5. Operationen (StateChange auf DEC + Downstream)

- **RESOLVE_DECISION:** `open_decision -> resolved`, mit gewählter Antwort + Rationale + `resolutionOutcome` +
  History-Eintrag am DEC-Item. Die `contradicts`-Relation wird **geschlossen/aufgelöst, nicht hart gelöscht** (Rev 2):
  entweder als Relation mit Metadata `status=resolved` markiert oder in die DEC-History überführt — die
  Entscheidung bleibt nachvollziehbar (welches Requirement widersprach, wie aufgelöst).
- **Widerspruch-Auflösung** (bestimmt das Outcome fürs Requirement — mappt auf bestehende Ingestion-Semantik):
  - `KEEP_ORIGINAL` — die neue widersprechende Aussage wird verworfen: `contradicts` auflösen, Requirement bleibt
    aktiv (≈ RESTATE). **PBI → `active`** (Rev 2 E-B).
  - `ADOPT_NEW` — die neue Aussage gewinnt: Requirement `superseded`, neues Requirement + `supersedes`-Relation; PBIs
    decken das Ersatz-Requirement. **Der Swap läuft über den bestehenden `pbi-update`-SUPERSEDE-Mechanismus
    (wiederverwenden/extrahieren, kein Zweit-Übergang, Rev 2).** **PBI → `needs_clarify`** (Coverage neu prüfen).
  - `REFINE` — beide zusammenführen: Requirement-Text neu (Version+1, History), `contradicts` auflösen (≈ REFINE).
    **PBI → `needs_clarify`** (Rev 2 E-B).
- **UNBLOCK_PBI (deterministisch, Downstream):** PBIs mit `blocked_by_decision` + `openDecisionRef==DEC` → Zielstatus
  je Outcome (s.o.: active bei KEEP_ORIGINAL, sonst needs_clarify), History. Der *abgeleitete* Block klärt sich
  automatisch, weil die `contradicts`-Relation aufgelöst ist (github-sync rechnet neu).

## 6. Gate-Invarianten (deterministisch)

- Ziel-DEC existiert und ist `open_decision` (kein doppeltes Auflösen).
- Outcome ∈ {KEEP_ORIGINAL, ADOPT_NEW, REFINE}; bei ADOPT_NEW/REFINE ist eine neue Aussage vorhanden.
- Jedes als „entblockt" gemeldete PBI referenziert die DEC (explizit oder über ein betroffenes Requirement) — kein
  PBI wird ohne Bezug entblockt.
- Keine DEC mehrfach in einem Plan.
- **Nachvollziehbarkeit (Rev 2):** die Auflösung darf die Historie nicht verlieren — DEC endet auf `resolved` mit
  gesetztem `resolutionOutcome`, und die vorherige `contradicts`-Beziehung ist belegt (Relation-Status `resolved`
  ODER DEC-History). Der Apply lehnt eine Auflösung ohne dokumentiertes Outcome ab.
- **Coverage (Rev 2, ADOPT_NEW):** der Requirement-Swap darf keine PBI-Coverage verlieren (gleiche Prüfung wie
  SUPERSEDE_PBI in 1c-3) — kein „nacktes" PBI nach dem Swap.

## 7. Review + Apply

- **HumanReview:** der Mensch autorisiert die Auflösung (die Stakeholder-Antwort ist der Beleg). Generische
  HumanReview-UI, apply/skip je Operation — wie ingest-review / github-forward-review.
- **Apply (deterministisch, Core über Port + Audit-Snapshot):** DEC resolved, Widerspruch aufgelöst, PBIs entblockt,
  History überall. Schreibt ein neues **affected-view** + **github-sync-Delta** der entblockten PBIs → der nächste
  `github-forward` nimmt sie als UPDATE/CREATE auf. Damit ist der Kreis geschlossen.

## 8. Wiederverwendung (kein Neubau)

Ingestion-Skelett (`StateChangeKind` hat `Restate`/`Refine`/`Supersede`/`AlreadyDecided` bereits), `IngestionApply`-
Muster (Supersede-Swap, History), `CoreViews.AffectedItems` (Blast-Radius), `PbiStatus.Max`-Präzedenz,
`CoreGithubMapping`/github-sync-Delta-Schreibung, HumanReview-UI, Gate/Review/Apply-Runner-Struktur.

**Rev 2 (verbindlich):** der SUPERSEDE-/Swap-Übergang für ADOPT_NEW ist **derselbe** wie in `pbi-update`
(`PbiUpdateApply`/`PbiUpdateDerivation` SUPERSEDE_PBI). Falls er nicht direkt aufrufbar ist, wird er in eine geteilte
Funktion **extrahiert** und von beiden Toren genutzt — NIE ein zweiter, parallel gepflegter Wahrheitsübergang. Ein
erster Bauschritt in T2.1 prüft/extrahiert diese Wiederverwendbarkeit, bevor ADOPT_NEW implementiert wird.

## 9. Der geschlossene Kreis

```text
Tor 1  Meeting     -> Requirements (+ CONTRADICT -> DEC, blockt)     [gebaut]
Tor 2  Stakeholder -> DEC resolved -> PBIs entblockt -> Delta        [DIESER PLAN]
Tor 3  GitHub      <- entblockte PBIs jetzt CREATE/UPDATE            [gebaut]
```

Erst mit Tor 2 fließt geklärte Arbeit durch die ganze Kette — der eigentliche End-to-End-Beleg.

## 10. Offene Entscheidungen

- **E-A — Antwort-Quelle:** strukturiertes Resolution-Input (T2.1) vs. agentische Deutung (T2.2). *Empfehlung:* mit
  dem einfachen Fall starten (wie Tor 1), Agent als zweite Stufe.
- **E-B — PBI-Unblock-Ziel:** **ENTSCHIEDEN (Rev 2):** KEEP_ORIGINAL → `active`; ADOPT_NEW/REFINE → `needs_clarify`
  (geänderte Wahrheit ⇒ Zuschnitt/Kriterien/Coverage neu prüfen).
- **E-C — Supersede-Kaskade:** **ENTSCHIEDEN (Rev 2):** bestehenden `pbi-update`-SUPERSEDE-Swap wiederverwenden bzw.
  in eine geteilte Funktion extrahieren — genau EIN Swap-Mechanismus, kein inline-Zweit-Übergang.
- **E-D — DEC-Zielstatus:** `resolved` vs. `decided`. *Empfehlung:* `resolved` (klar abgegrenzt von `open_decision`).
- **E-E — Mehrere PBIs / mehrere DECs pro Lauf:** additiv erlauben (wie MULTI_CAUSE_MERGE in 1c-3).

## 11. Bauschritte

```text
T2.0  [DONE] Vorab (Rev 2): SUPERSEDE-/Swap-Übergang extrahiert nach `core/RequirementSwap.cs`
      (`SwapCoverage` + `Covers`/`RemoveCovers`); `PbiUpdateApply` nutzt es (verhaltensgleich belegt: SUPERSEDE_PBI
      PBI-001 L3-REQ-001→L3-REQ-002, covers+links geswappt, needs_clarify). Tor 2 ADOPT_NEW ruft dieselbe Funktion.
T2.1  [DONE] Deterministischer Kern (KEIN LLM): `decision/`-Ordner (Models/Derivation/Gate/Apply/Review + 3 Runner),
      CLI decision-resolve / -review / -apply. State-Maschine belegt: ADOPT_NEW (superseded + neues REQ-58 +
      supersedes + Swap via RequirementSwap + Unblock→needs_clarify + contradicts_resolved + github-sync-Delta) und
      KEEP_ORIGINAL (Requirement unverändert, Unblock→active). Core gesichert/restauriert. Build grün.
T2.2  [DONE] Agentischer Resolver: `DecisionResolverTools` (get_open_decisions/get_decision_context/save_resolutions)
      + Prompt DecisionResolverAgent1 + `decision-resolve-agent` → erzeugt DecisionResolutionInput, danach IDENTISCH
      zur T2.1-Kette. Build grün; LLM-Lauf autor-getrieben (deterministischer Downstream in T2.1 belegt). 4. Agent-Knoten.
T2.3  [DONE] Kreis-Test (deterministisch): `DecisionUnblockTest` + CLI `decision-unblock-test`. Synthetischer Core
      durch die echten Bausteine (GithubForwardSeed VOR → DecisionResolutionApply → GithubForwardSeed NACH). Belegt:
      VOR HOLD_BLOCKED=PBI-1,PBI-2; NACH HOLD=0, PBI-1→UPDATE, PBI-2→unmapped/CREATE → PASS. Tor 2 (T2.0–T2.3) komplett.
```

## 12. MAF-Nativität / Determinismus

Agentisch (nur T2.2): der Decision-Resolver (Antwort-Deutung). Deterministisch: Blast-Radius, Widerspruch-Auflösung,
Unblock, alle Status-/History-Übergänge, Gate, Apply, Delta. Der Workflow-Rahmen bleibt MAF-nativ; die Wahrheits-
Mutation ist host-seitig deterministisch. DB-later gilt unverändert (Writes über den Port, stabile IDs).

## 13. Abgrenzung / Risiken

- Tor 2 **erzeugt** keine Decisions — es löst bestehende auf (Erzeugung ist Tor 1 CONTRADICT / Re-Clarify).
- Kein Auto-Resolve: eine Decision wird NIE ohne menschliche Freigabe aufgelöst (Governance wie E4 im Reverse-Tor).
- Risiko Fehlauflösung: ADOPT_NEW ändert die Wahrheit → History + Review sind Pflicht; der Swap darf keine Coverage
  verlieren (Gate prüft, wie SUPERSEDE_PBI in 1c-3).
- Abgeleiteter vs. expliziter Block: beide müssen im selben Apply geklärt werden (contradicts **auflösen** UND Status
  zurücksetzen) — sonst bleibt ein PBI in der github-sync-View scheinbar blockiert.
- Historie-Verlust (Rev 2): `contradicts` darf NICHT hart gelöscht werden — geschlossen/aufgelöst + `resolutionOutcome`
  + DEC-History, sonst ist die Entscheidung nicht mehr rückverfolgbar (widerspricht der Rückverfolgbarkeits-Linie des
  ganzen Projekts).
- Zwei Wahrheitsübergänge (Rev 2): würde Tor 2 eigene Supersede-Logik kopieren, driften Tor-1/pbi-update-Swap und
  Tor-2-Swap auseinander → ein geteilter Mechanismus ist Pflicht.
