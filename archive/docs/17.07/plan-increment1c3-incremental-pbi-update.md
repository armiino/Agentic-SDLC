# Plan — Increment 1c-3: Incrementeller PBI-Update-Mechanismus

Status: **PLAN / DESIGN.** Noch nicht gebaut. Schliesst 1c ab: aus der affected-items-view (1c-2) werden **nur die
betroffenen PBIs** gezielt aktualisiert (stabile IDs), statt den Backlog neu zu bauen. Muster identisch zum Rest:
`Maker → deterministisches Gate → HumanReview → deterministischer Apply` — jetzt auf der PBI-Schicht.

Voraussetzungen erfuellt: 1c-1 (PBIs/Features als Core-Entitaeten, stabile IDs) + 1c-2 (affected-view = Blast-Radius).

---

## 0. Kurz

```text
neues Meeting → Ingestion → Core-Update → affected-items-view (Blast-Radius)
  → 1c-3: PBI-StateChangePlan (nur betroffene PBIs) → Gate → HumanReview → Apply
  → aktualisierte PBIs (stabile IDs) + kleines github-sync-Delta
```

## 1. Problem / Ziel

Nach einem Delta sollen sich nicht 27 PBIs neu bauen, sondern nur die betroffenen aendern — unveraenderte PBIs
behalten ID + Inhalt. Das ist der eigentliche „living"-Update und der letzte Baustein, bevor der GitHub-Agent ein
kleines, stabiles Delta bekommt.

## 2. Input

- **affected-items-view** (1c-2): `directIds` + transitiv betroffene `requirements/features/pbis/openDecisions`.
- **Ingestion-Delta** (`delta.json`, `AppliedOperation[]`): sagt, WAS je Requirement passiert ist
  (added/refined/reaffirmed/superseded/contradicted). Noetig, um die richtige PBI-Operation abzuleiten.
- **Core** (aktueller Stand, ueber den Port).

## 3. PBI-StateChangePlan — Operationen (sparse, KEIN NO_CHANGE)

Eine Operation NUR fuer betroffene PBIs; unberuehrte PBIs erscheinen NICHT im Plan (Rev-2-Disziplin aus 1c).

```text
NEW_PBI        neues Requirement, in KEINEM PBI gedeckt -> neues PBI (Feature aus part_of_feature)   [praegt PBI-<n>]
EXTEND_PBI     neues verwandtes Requirement -> bestehendes PBI deckt es zusaetzlich (stabile ID)      [+covers, +needs_clarify]
MARK_CHANGED   gedecktes Requirement wurde REFINE't -> PBI "needs_clarify" (nur MARKER: Zuschnitt/Acceptance
               Criteria pruefen; das PBI wird NICHT inhaltlich neu formuliert — das ist ein spaeterer re-clarify)
BLOCK_PBI      neue open_decision widerspricht gedecktem Requirement -> PBI "blocked_by_decision" (+openDecisionRef)
SUPERSEDE_PBI  gedecktes Requirement superseded -> Requirement-SWAP: altes raus UND Ersatz-Requirement rein
               (via supersedes-Relation, deterministisch), PBI "needs_clarify" — kein Verlust der Abdeckung
```

## 4. Maker — deterministisch wo moeglich, agentisch nur fuer echte Urteile (E1 entschieden)

```text
DETERMINISTISCH (aus Delta + Relationen, kein LLM):
  refined  Requirement  -> alle PBIs die es covern        -> MARK_CHANGED
  contradicted (open_decision) -> alle PBIs die das Ziel covern -> BLOCK_PBI (+ openDecisionRef auf die DEC)
  superseded Requirement -> alle PBIs die es covern -> SUPERSEDE_PBI mit Ersatz aus der supersedes-Relation
    (REQ-neu --supersedes--> REQ-alt): altes Requirement raus, Ersatz rein (eindeutig -> deterministisch)
AGENTISCH (nur der Platzierungs-Urteilsfall):
  NEUES Requirement (added/NEW_RELATED), in KEINEM PBI gedeckt:
    - deterministischer Vorschlag: part_of_feature -> Feature -> dessen PBIs (Kandidaten fuer EXTEND_PBI).
    - Agent entscheidet EXTEND_PBI (welches) vs NEW_PBI, mit Beleg (Requirement-Overlap/Feature).
    - Fallback ohne part_of_feature: Agent ordnet Feature zu oder schlaegt NEW_PBI vor.
```

Beleg-Pflicht: jede Operation nennt das betroffene PBI (bzw. Feature bei NEW_PBI) + die ausloesenden Requirement/
Decision-IDs. So bleibt der Update nachvollziehbar.

## 5. Gate (deterministisch)

```text
STABLE_ID_PRESERVED   unberuehrte PBIs sind NICHT im Plan (kein NO_CHANGE); EXTEND/MARK/BLOCK/SUPERSEDE behalten die PBI-ID.
COVERAGE              jedes NEUE (ungedeckte) Requirement der affected-view hat GENAU EINE Platzierung (NEW_PBI|EXTEND_PBI).
UNKNOWN_TARGET        EXTEND/MARK/BLOCK/SUPERSEDE referenzieren ein existierendes PBI; NEW_PBI ein existierendes Feature.
BLOCK_NEEDS_DECISION  BLOCK_PBI referenziert eine existierende open_decision (openDecisionRef).
MULTI_CAUSE_MERGE     MEHRERE Operationen auf demselben PBI sind ERLAUBT (z.B. refined + blockiert). Das Gate
                      verbietet sie NICHT, sondern der Apply fuehrt sie geordnet zusammen (Status-Praezedenz §7),
                      History enthaelt ALLE Gruende. (Verboten bleibt nur echt Widerspruechliches, das es hier
                      nicht gibt — die Ops sind additiv.)
DOR                   (unveraendert) neue/erweiterte PBIs erfuellen die DoR (Testbarkeit) — sonst needs_clarify.
```

## 6. HumanReview

Generische UI, 1 Item je Operation (apply/skip + reason), wie ingest-review/cluster-review. Der Mensch
autorisiert Backlog-Aenderungen. Re-Launch/Autosave wie gehabt.

## 7. Apply (deterministisch, Update-by-Identity)

```text
NEW_PBI       -> neue Core-PBI-Entitaet (PBI-<n>), part_of_feature-Relation, covers-Relationen, status=active|needs_clarify.
EXTEND_PBI    -> PBI.pbi.linkedRequirementIds += reqId; +covers-Relation; status=needs_clarify; version++; history.
MARK_CHANGED  -> PBI.status=needs_clarify; version++; history (Grund: Requirement X verfeinert). NUR Marker, kein Rewrite.
BLOCK_PBI     -> PBI.status=blocked_by_decision; pbi.openDecisionRefs += decId; version++; history.
SUPERSEDE_PBI -> Requirement-SWAP: linkedRequirementIds/covers: altes raus, Ersatz (supersedes-Relation) rein;
                 status=needs_clarify; history (Grund: REQ-alt superseded durch REQ-neu).
```

**Mehrfach betroffenes PBI (MULTI_CAUSE_MERGE):** treffen mehrere Operationen dasselbe PBI, fuehrt der Apply sie
**geordnet zusammen** — EINE Version++, additive Wirkung, ALLE Gruende in der History. Status per Praezedenz:

```text
blocked_by_decision  >  superseded  >  needs_clarify  >  active
```
(z.B. refined + blockiert -> Status blocked_by_decision, History enthaelt beide Gruende; kein Datenverlust.)

Unberuehrte PBIs bleiben unangetastet (ID + Inhalt + Version). Core NUR ueber den Port + Audit-Snapshot.

## 8. Output

- Aktualisierter Core (PBI-Entitaeten mit stabilen IDs, neuen Versionen/Status).
- **github-sync-view-Delta**: welche PBIs neu / needs_clarify / blocked → der GitHub-Agent bekommt ein kleines,
  praezises Set (keine Duplikat-Issues). (Mappings/Tor 3 spaeter.)

## 9. Lifecycle-Status (PBI)

```text
active               arbeitsfaehig
needs_clarify        Zuschnitt/Requirements haben sich geaendert -> re-clarify noetig
blocked_by_decision  offene Entscheidung blockiert
superseded/retired   (Archiv)
done                 NICHT hier gesetzt -> Tor 3 (GitHub geschlossen + Verifikation), E4
```

## 10. MAF-Nativitaet / Determinismus

Agentisch nur die Platzierung neuer Requirements (EXTEND vs NEW_PBI). Alles andere (MARK/BLOCK/SUPERSEDE aus
Relationen, Gate, Apply, Status-Uebergaenge, github-sync-Delta) deterministisch. Minimaler LLM-Einsatz genau am
Urteilspunkt — konsistent mit der Forschungslinie „Agent nur wo Bedeutung regiert".

## 11. DB-later

Update-by-Identity auf PBI-Entitaeten = natuerliche DB-UPSERTs; Status/Version/Relationen als Spalten/Kanten.
JSON jetzt, DB-Adapter hinter demselben Port.

## 12. Abgrenzung zum GitHub-Agenten

1c-3 liefert das github-sync-Delta (betroffene PBIs). Der GitHub-Agent (Tor 3) macht daraus den operativen
Abgleich gegen GitHub (create/update/no-change) — die fachliche Delta-Berechnung ist schon passiert.

## 13. Bauschritte

```text
1c-3.1  PBI-StateChangePlan-Modell (Operationen) + deterministische Ableitung (MARK/BLOCK/SUPERSEDE aus Delta+Relationen).
1c-3.2  Agentischer Platzierungs-Knoten fuer NEUE Requirements (EXTEND vs NEW_PBI), MAF-Workflow + Tools.
1c-3.3  Gate (§5) + HumanReview-Adapter + Apply (Update-by-Identity, §7).
1c-3.4  CLI: pbi-update <ingestion-run> / pbi-update-review / pbi-update-apply.
1c-3.5  Test: Meeting-Delta (No-Go bearbeiten = NEW_RELATED + REFINE + CONTRADICT) → nur betroffene PBIs
        aendern sich (stabile IDs), unveraenderte unangetastet, github-sync-Delta klein.
```

## 14. Offene Entscheidungen

- **E1 — ENTSCHIEDEN:** Maker deterministisch, wo aus Delta/Relationen ableitbar (MARK/BLOCK/SUPERSEDE);
  agentisch NUR fuer die Platzierung neuer Requirements (EXTEND vs NEW_PBI).
- **F1 — EXTEND vs NEW_PBI Schwelle:** ab wann ist ein neues Requirement „dasselbe PBI" (EXTEND) vs eigenes PBI?
  *(Empfehlung: Agent entscheidet mit Beleg; Gate erzwingt nur Coverage + gueltiges Ziel, keine harte Heuristik.)*
- **F2 — MARK_CHANGED-Wirkung:** nur Status `needs_clarify` (Empfehlung, klein) ODER direkt re-clarify des PBIs
  anstossen? *(Empfehlung: nur Status; ein spaeterer re-clarify-Lauf auf `needs_clarify`-PBIs ist separat.)*
- **F3 — Auto-Unblock:** wird ein `blocked_by_decision`-PBI automatisch entblockt, wenn die Decision `resolved`
  ist? *(Gehoert zur Decision-Ingestion/Tor 2; hier nur Status setzen.)*

## 15. Erwartete Wirkung (zu belegen)

Meeting „No-Go bearbeiten": `NEW_RELATED REQ-58` → EXTEND_PBI oder NEW_PBI im No-Go-Feature; `REFINE REQ-17` →
MARK_CHANGED der referenzierenden PBIs; `CONTRADICT → DEC` → BLOCK_PBI. Die anderen ~24 PBIs bleiben unberuehrt
(stabile IDs). github-sync-Delta = wenige PBIs → GitHub-Agent erzeugt keine Duplikate. Damit ist 1c geschlossen.
