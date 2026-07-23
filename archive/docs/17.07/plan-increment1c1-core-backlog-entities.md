# Plan — Increment 1c-1: Features/PBIs als persistente Core-Entitaeten (Datenmodell + Seed)

Status: **PLAN / DESIGN.** Noch nicht gebaut. Der load-bearing Baustein von 1c (`plan-increment1c-*.md` §5):
aus „Output-Dateien eines re-clarify-Laufs" werden **langlebige, versionierte Projektobjekte** im Core.
Dieser Schnitt beruehrt das Datenmodell — deshalb VOR dem Code festgehalten (falsche Abstraktion hier macht
DB, GitHub-Sync und Steward-Agent spaeter unnoetig schwer).

Zwei Entscheidungen sind hier **festgelegt** (Review-Empfehlung uebernommen): E-A Rich-PBI-Modell, E-B ID-Vergabe.

---

## Entscheidung E-A — CoreItem-Basis + typisierte Payload (KEIN reiner Text)

Ein PBI ist kein Textartefakt, sondern ein fachliches Arbeitsobjekt (goal, acceptanceCriteria, linkedRequirements,
openDecisions, priorityRank, status, githubMapping, traceability, history). Also: **eigener strukturierter Typ**,
nicht `ProjectStateItem.Text` + loser Payload.

Pragmatischer Mittelweg (Reuse + Struktur): `ProjectStateItem` **bleibt die CoreItem-Basis** (id, itemType, status,
version, identityKey, history) und die doc-level `relations` bleiben der Graph. NEU: optionale **typisierte
Payloads** je kind. Requirements nutzen weiter die flachen Felder (`Text` etc.) = ihre Payload.

```text
ProjectStateItem  (= CoreItem-Basis, schemaVersion 2 -> 3)
  itemId · itemType · status · version · identityKey · history · (flache Requirement-Felder: text, sourceClaimIds, …)
  + feature : FeaturePayload?     (nur wenn itemType=feature)
  + pbi     : PbiPayload?         (nur wenn itemType=pbi)

FeaturePayload
  label · rationale · coreRequirementIds[] · crossCuttingRequirementIds[]

PbiPayload
  goal · description · acceptanceCriteria[] · linkedRequirementIds[] · openDecisionRefs[] ·
  priorityRank · mvp? · readiness? · traceability · githubMapping?   (githubMapping spaeter, Tor 3)
  (openDecisionRefs[] = VERWEISE auf decision-Entitaeten, KEINE zweite Decision-Wahrheit im Payload.
   Beim Seed uebergangsweise als Ref auf/aus dem alten PBI-openDecision; Ziel: echte decision-Items.)
```

- **Serialisierung:** zwei nullable, typisierte Felder (`feature`, `pbi`) statt Polymorphismus — kein STJ-
  Discriminator-Aufwand, abwaertskompatibel (v2-Dateien laden, Felder null). Nur das zu `itemType` passende Feld
  ist gesetzt; ein Gate prueft das.
- **DB-later:** `feature`/`pbi` werden eigene Tabellen (join by `itemId`), Requirements bleiben ihre Tabelle —
  sauberer als ein generisches Payload-Blob.

## Entscheidung E-B — ID-Vergabe + Remap beim Seed

- **PBIs → neue Core-IDs `PBI-001`, `PBI-002` …** (fortlaufend, einmalig, Core-autorisiert). Das bisherige
  Schema `PBI-FC006-01` ist abgeleitet/churny → wird NICHT als Identitaet uebernommen. Die alte ID bleibt als
  `legacyId`/`sourceId` in Metadata + als `identityKey` (Matching-Hilfe) erhalten → alter re-clarify-Output bleibt
  rueckverfolgbar, aber die Wahrheit liegt im Core.
- **Features → Core-IDs `FC-001`, `FC-002` …** Die bestehenden Cluster sind bereits `FC-<n>` → **werden ab
  Seed-Zeitpunkt als Core-IDs uebernommen** (nicht mehr Run-IDs; klar dokumentiert). `cluster.identityKey` →
  Core-`identityKey` (Matching-Hilfe). (Falls kuenftige re-clarify-Laeufe FC-* neu vergeben, sind das nur
  Vorschlaege; die Core-`FC-<n>` ist autoritativ.)
- **Lifecycle:** Seed setzt alle als `active`, `version=1`, `history=[]`.
- **ID-Autoritaet:** wie bei Requirements praegt der Core die ID (max+1 je Prefix). Eingehende re-clarify-IDs sind
  Vorschlaege/legacy, nie Core-Identitaet.

## Der Seed (Kern von 1c-1)

Einmaliges Hochziehen des bestehenden re-clarify-Stands (applied `feature-clusters.json` +
`product-backlog.json`) in den Core — deterministisch, kein LLM:

```text
1. Features: je Cluster -> CoreItem(itemType=feature, id=FC-<n>, identityKey=cluster.identityKey,
   feature=FeaturePayload{label, rationale, core/crossCutting requirementIds}), legacy=cluster.clusterId.
2. PBIs: je PBI -> CoreItem(itemType=pbi, id=PBI-<n> [neu], identityKey=<normalisiert>,
   pbi=PbiPayload{goal, acceptanceCriteria, linkedRequirementIds, openDecisionRefs, priorityRank, traceability}),
   legacyId/sourceId=<altes PBI-FC###-##>.
3. Relationen (der Graph Requirement -> Feature -> PBI):
   - requirement --part_of_feature--> feature        (loest die ingestion-featureKey-Strings gegen FC-<n> auf, P3)
   - pbi         --part_of_feature--> feature
   - pbi         --covers-->          requirement     (die linkedRequirementIds des PBIs)
4. Provenienz/traceability der PBIs bleibt erhalten (die deterministische Enrichment-Kette aus re-clarify).
5. Idempotenz: zweiter Seed erkennt bestehende Entitaeten **primaer via legacyId/sourceId**, `identityKey` nur
   als Fallback/Diagnose — praegt NICHT neu (kein Duplikat). Ohne vorhandenen Backlog-Core: Abbruch mit Hinweis.
```

**CLI (Vorschlag):** `core-seed-backlog <l4-re-clarify-run|dir>` — hebt Cluster+PBIs eines re-clarify-Laufs in
den Core. (Analog `core-seed` fuer Requirements.)

## Was bleibt / was aendert

- **Bleibt:** re-clarify erzeugt weiter seine Run-Outputs (cluster/PBIs); `core-seed`/`core-baseline`;
  das Maker→Gate→Human→Apply-Muster. KEINE Aenderung am re-clarify-Graph.
- **Aendert:** `ProjectStateItem` + `feature`/`pbi`-Payload (schemaVersion 3); neuer `core-seed-backlog`;
  der Core enthaelt jetzt Requirements + Features + PBIs + Relationen als EINEN Graphen.

## Abgrenzung (was 1c-1 NICHT ist)

- **NICHT** der incrementelle Update-Mechanismus (das ist 1c-3) und **NICHT** die Blast-Radius-Views (1c-2).
  1c-1 baut nur die **persistente Schicht + den einmaligen Seed**. Danach existiert der Vorzustand, gegen den
  1c-3 diffen/mergen kann.

## DB-later

`feature`/`pbi` als Tabellen (Felder + join by id), `relations` als Kanten-Tabelle, `history` je Entitaet,
`traceability`/Evidence als Rueckverfolgung. `JsonCoreRepository` → `DbCoreRepository` hinter demselben Port.

## Offene Kleinentscheidungen

- **K1 — Relationsrichtung/Namen:** `pbi --covers--> requirement` vs. `requirement --realized_by--> pbi`
  (Empfehlung: `covers`, aktiv vom PBI aus — konsistent mit `part_of_feature`).
- **K2 — openDecisions — ENTSCHIEDEN als Referenzen:** das PBI traegt `openDecisionRefs[]` (Verweise auf
  `decision`-Entitaeten), KEINE zweite Decision-Wahrheit im Payload. Beim Seed uebergangsweise aus dem alten
  PBI-openDecision; Ziel: die Refs zeigen auf echte `decision`-Items (aus Ingestion/Tor 3). Eine Wahrheit, viele Verweise.
- **K3 — Feature FC-* uebernehmen vs. sauber remappen:** Empfehlung uebernehmen (sind schon FC-<n>), legacy=clusterId.

## Bauschritte 1c-1

```text
1. Modell: FeaturePayload + PbiPayload; ProjectStateItem + feature?/pbi?; schemaVersion 3; Gate „payload passt zu itemType".
2. core-seed-backlog: Cluster+PBIs eines re-clarify-Laufs -> Core-Entitaeten (IDs praegen, legacy erhalten).
3. Relationen Requirement->Feature->PBI erzeugen (featureKey-Strings gegen FC-<n> aufloesen).
4. Idempotenz (legacyId/sourceId primaer, identityKey nur Fallback) + Verifikation (Counts, Stichprobe,
   Relationen, zweiter Seed = No-Op).
```

## Erwartete Wirkung

Nach 1c-1 ist der Core ein **Graph aus Requirements + Features + PBIs + Decisions + Relationen** mit stabilen
Core-IDs und Historie — die Grundlage, auf der 1c-2 (Blast-Radius-Views) und 1c-3 (incrementelles PBI-Update)
aufsetzen. Aus Output-Dateien sind langlebige Projektobjekte geworden.
