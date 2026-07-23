# Plan — Increment 1c: Incrementeller Downstream (affected-view → Cluster/PBI-Update)

Status: **PLAN / DESIGN.** Noch nicht gebaut. Baut auf Inc 1b (Full-Core-Modus, `core-baseline` → re-clarify)
auf und macht den Downstream **incrementell**: ein neues Meeting-Delta aktualisiert die bestehenden Cluster/PBIs
gezielt, statt alles neu zu bauen. Damit wird aus dem Mechanismus ein echter **lebender Sprint-Prozess**.

Enthält die Review-Anmerkungen (Core = versionierter Projektgraph, Lifecycle-Status, Views, präziser Blast-Radius).
**Rev 2 (2. Review):** NO_CHANGE als Operation entfernt (Plan bleibt sparse); PBI-Identität = stabiler,
einmalig vergebener Primärschlüssel (nicht aus Titel abgeleitet); done-Semantik geschärft (nicht aus issue-closed).

---

## 0. Kurz: 1b vs. 1c

```text
Inc 1b (fertig): core-baseline (GANZER Core) → re-clarify baut Cluster/PBIs KOMPLETT neu.
                 → Voll-Neubau, neue PBI-IDs, GitHub muss raten was sich aenderte.
Inc 1c:          affected-view (nur Blast-Radius eines Deltas) → re-clarify/PBI-Update AKTUALISIERT gezielt.
                 → stabile PBI-IDs, sauberes GitHub-Delta, effizient.
```

## 1. Problem

Full-Core-Neubau bei jedem Meeting churnt Identitaet: Cluster-/PBI-IDs werden neu vergeben, unveraenderte Features
werden unnoetig neu geschnitten, und der GitHub-Agent muss aus einem komplett neuen Plan herausrechnen, was
wirklich neu/geaendert ist (Duplikat-Gefahr). Der lebende Prozess braucht: **nur der betroffene Ausschnitt fliesst
durch, unveraenderte PBIs behalten ihre ID.**

## 2. Der Core als versionierter Projektgraph (Anmerkung)

Der Core ist **nicht** eine endlos wachsende Liste, sondern ein **versionierter Graph** aus typisierten Entitaeten
(`requirement`, spaeter `feature`, `pbi`, `decision`, `evidence`, `github_mapping`) mit Versionen, Relationen und
**Lifecycle-Status**. Neues wird gegen den Core **aufgeloest** (bestaetigen/verfeinern/neu/ersetzen/widersprechen),
nicht stumpf angehaengt. Alte/erledigte/ersetzte Dinge **bleiben erhalten**, aber ueber Status eingeordnet:

```text
Lifecycle-Status (formalisiert):
  active       aktuelle, gueltige Wahrheit (arbeitsfaehig)
  done         umgesetzt/erledigt (z.B. Issue geschlossen + verifiziert)
  superseded   durch neuere Fassung ersetzt (SUPERSEDE)
  retired      zurueckgezogen / nicht mehr relevant
  open_decision offene Entscheidung (nur decision-Entitaeten)
```

Damit waechst der Core langfristig, bleibt aber **strukturiert und rueckverfolgbar** (Historie + Provenienz).

## 3. Views als arbeitsfaehige Ausschnitte (Anmerkung)

Agenten/Workflows bekommen **nicht** den ganzen Core, sondern gezielte Views (hinter dem View-Port, DB-later):

```text
active-backlog-view   nur active Requirements/PBIs — die arbeitsfaehige Sicht (heute: core-baseline ~ Voll-active).
affected-items-view   der Blast-Radius eines Deltas (§4) — Input fuer den incrementellen Downstream.
archive-view          done/superseded/retired — Historie/Audit, NICHT im Arbeitsfluss.
github-sync-view      PBIs + github_mappings + operational status — Input fuer den GitHub-Agenten.
```

Der Core ist die **vollstaendige Wahrheit mit Historie**; Views sind die **Arbeitsausschnitte**. Dieselbe Logik
migriert spaeter in eine DB (Items/Relationen/Versionen/Evidence/State-Ops → Tabellen/Collections statt JSON).

## 4. Die affected-view als BLAST-RADIUS (praezise Regel)

Kernpunkt (Anmerkung): 1c arbeitet **nicht blind nur mit dem Delta**. Das Delta wird **erst gegen den vollen Core
aufgeloest** (das macht die Ingestion bereits — Resolver sieht den ganzen Core). **Danach** entsteht die affected-
view = der betroffene Ausschnitt, den Downstream bekommt.

Ein Item X gehoert in die affected-view eines Deltas D, wenn:
```text
(a) DIREKT getoucht: X wurde von D erzeugt/verfeinert/bestaetigt/ersetzt/widersprochen (die heutige affected-view).
(b) TRANSITIV betroffen (1 Hop ueber Relationen):
    - ein PBI/Feature, das ein von D geaendertes Requirement realisiert/enthaelt (part_of_feature / realized_by).
    - eine decision, die ein geaendertes Item betrifft (contradicts).
    - ein github_mapping, das auf ein geaendertes PBI zeigt.
    - ein done/superseded/retired Item, das von D WIEDERERUEFFNET oder referenziert wird.
```

Lifecycle-Regel: **done/superseded/archivierte Items bleiben im Core**, kommen aber **nur** in die affected-view,
wenn sie von D wirklich betroffen sind (Referenz, Konflikt, Wiederoeffnung, Mapping). So bleibt der Core
vollstaendig, aber Agenten arbeiten fokussiert.

> Heute (Inc 1b) enthaelt `affected-view.json` nur (a) + direkte Relationen. 1c erweitert sie auf den echten
> transitiven Blast-Radius inkl. PBIs/Features/Mappings.

## 5. Voraussetzung (load-bearing): Cluster/Features + PBIs als persistente Core-Entitaeten

Um PBIs zu **aktualisieren statt neu zu bauen**, braucht es einen **Vorzustand**: Cluster/Features und PBIs muessen
**persistente Core-Entitaeten mit stabiler Identitaet + Lifecycle-Status** sein — nicht Run-Artefakte pro Lauf.
**Stabile Identitaet (Rev 2) = einmalig vergebener Primaerschluessel (`pbiId`/`featureId`) + History + Relationen**
— NICHT aus Titel/Zuschnitt abgeleitet (die aendern sich). `identityKey` ist nur Seed-/Matching-Hilfe (E2).
Das ist der groesste Teil von 1c und verbindet:
- `plan-pb.md` Flag A (stabile `pbiId` — als vergebener Schluessel, nicht abgeleitet),
- P3 (Feature-als-Entitaet, `plan-core-ingestion.md` §4),
- „PBIs/Mappings sind Teil des Core" (`plan-core-ingestion.md` §2).

Ohne diese Persistenz gibt es keinen Vorzustand zum Diffen/Mergen → 1c nicht moeglich. **Deshalb ist 1c-1 (unten)
der erste Baustein.**

## 6. Der incrementelle Update-Mechanismus (dasselbe Muster eine Ebene hoeher)

Symmetrie zur Requirement-Ingestion — jetzt auf der PBI/Feature-Schicht:

```text
affected-view (Blast-Radius)
  → MAKER (Agent/deterministisch): je betroffenem Requirement/Feature eine PBI-Operation:
       NEW_PBI          neues Requirement/Feature -> neues PBI (im bestehenden oder neuen Cluster)
       EXTEND_PBI       neues verwandtes Requirement -> bestehendes PBI erweitern (stabile ID)
       MARK_CHANGED     ein referenziertes Requirement wurde verfeinert -> PBI "needs re-clarify"
       BLOCK_PBI        neue open_decision betrifft PBI -> "blocked_by_decision"
       SUPERSEDE_PBI    Requirement superseded -> PBI-Status anpassen
       (KEIN NO_CHANGE, Rev 2: unberuehrte PBIs erscheinen NICHT im Plan — sie bleiben IMPLIZIT unveraendert.
        Taucht NO_CHANGE auf, ist der Blast-Radius zu gross/unscharf. Der Plan ist sparse = nur betroffene Ops.)
  → GATE (det.): Coverage (jedes betroffene Requirement platziert), STABILE-ID-Erhalt (unberuehrte PBIs bleiben
       unangetastet und erscheinen nicht im Plan), DoR unveraendert.
  → HUMANREVIEW (generische UI): Operationen apply/skip.
  → APPLY (det.): Update-by-Identity in den Core (PBI-Entitaeten), unberuehrte PBIs unangetastet.
```

Ergebnis: eine **aktualisierte PBI-Menge mit stabilen IDs** — nur die betroffenen PBIs sind neu/geaendert/blockiert.
Der GitHub-Agent bekommt daraus die `github-sync-view` = kleines, praezises Delta → **keine Duplikat-Issues**.

## 7. MAF-Nativitaet / Determinismus

Agentisch nur dort, wo Bedeutung regiert (Feature-Zuordnung neuer Requirements, „extend vs. new PBI"). Gate,
Stabile-ID-Erhalt, Apply, View-Erzeugung, Lifecycle-Uebergaenge = deterministisch. Muster identisch zum Rest:
`Maker → Gate → HumanReview → Apply`.

## 8. DB-later

Genau dieser Schritt macht das Modell DB-mappbar: Features/PBIs sind dann keine losen Outputs eines re-clarify-
Laufs mehr, sondern **langlebige, versionierte Core-Entitaeten**. Die JSON-Datei ist nur die **lokale Speicherform**,
nicht die fachliche Architektur. Natuerliche Abbildung:

```text
Items          Entitaeten (typ, status, version, stabile ID)      -> Tabelle/Collection
Relations      verbinden Items (part_of_feature, supersedes, ...)  -> Tabelle
Evidence       Rueckverfolgbarkeit zu Transkripten/Claims          -> Tabelle
History        Aenderungen je Entitaet                              -> Tabelle
StateChangePlans geprüfte Aenderungsoperationen                     -> Tabelle
GitHubMappings Core-Item <-> Issue/PR/Kommentar                     -> Tabelle
Views (active-backlog/affected/archive/github-sync)                 -> Queries
```

JSON jetzt, DB-Adapter spaeter hinter demselben Port (`JsonCoreRepository` → `DbCoreRepository`), ohne die
fachliche Logik neu zu erfinden. Die State-Operationen (Update-by-Identity) sind natuerliche DB-UPSERTs.

## 9. Abgrenzung zum GitHub-Agenten

1c liefert dem GitHub-Agenten die `github-sync-view` (betroffene PBIs + Mappings + Status). Der Agent macht nur den
**operativen** Abgleich gegen GitHub — die fachliche Delta-Berechnung ist schon in 1c passiert. Damit ist das
urspruengliche Ziel erreicht: Sprint-Re-Run → keine Duplikat-Issues, nur gezielte Updates.

## 10. Bauschritte (inkrementell — 1c ist mehrstufig, ehrlich)

```text
1c-0  Lifecycle-Status formalisieren (active/done/superseded/retired/open_decision) + Uebergaenge im Apply.
1c-1  Cluster/Features + PBIs als persistente Core-Entitaeten (kind=feature|pbi, stabile Core-vergebene ID
      PBI-<n>/FC-<n> [einmalig vergeben, NICHT abgeleitet], identityKey NUR Seed-/Matching-Hilfe, Lifecycle +
      History). Einmaliger Seed aus dem bestehenden re-clarify-Stand. = groesster Baustein (Voraussetzung).
1c-2  affected-view auf den echten Blast-Radius erweitern (§4) + Views (active-backlog/affected/archive/
      github-sync) als View-Repo-Methoden.
1c-3  Incrementeller PBI-Update-Mechanismus (§6): Maker + PBI-StateChangePlan + Gate + Review + Apply.
1c-4  Test: Meeting-Delta → Ingestion → affected-view → PBI-Update → belegen: unveraenderte PBIs behalten ID,
      nur betroffene neu/geaendert/blockiert; GitHub-sync-view = kleines Delta.
```

## 11. Offene Entscheidungen

- **E1 — Feature-Zuordnung neuer Requirements:** deterministisch (ueber `part_of_feature`, das die Ingestion schon
  setzt) ODER agentisch (Feature-Resolver)? *(Empfehlung: deterministisch, wenn `part_of_feature` gesetzt; sonst
  agentischer Fallback — vermeidet unnoetigen LLM.)*
- **E2 — PBI/Feature-Identitaet — ENTSCHIEDEN:** stabile, **Core-autorisierte, einmalig vergebene ID**
  (`PBI-001`, `PBI-002` / `FC-001`, `FC-002`) beim ersten Auftreten — analog zur Requirement-ID-Autoritaet im Core.
  Ab dann **stabil**, auch wenn Titel/Beschreibung/Zuschnitt/Requirements/Status sich spaeter aendern. Die
  Identitaet liegt im **Core**, NICHT in einem berechneten Schluessel. `identityKey` = ausschliesslich **Seed-/
  Matching-Hilfe** (Hochziehen bestehender re-clarify-Outputs in den Core; spaeter Kandidaten-Vorschlag) — NIE die
  Identitaet. Cross-Meeting-Wiedererkennung laeuft ueber **Requirement-Overlap/Relationen**, nicht Titel-Match.
- **E3 — Blast-Radius-Tiefe:** 1 Hop (Empfehlung, ausreichend + billig) vs. transitive Huelle.
- **E4 — done-Semantik (Rev 2, nicht unterschaetzen):** ein geschlossenes GitHub-Issue ist **NICHT automatisch**
  `done` im Core. `done` erfordert eine explizite Regel: GitHub-geschlossen PLUS Review/Verifikation ODER ein
  akzeptierter GitHub-Feedback-StateChange (Tor 3 / GitHub-Feedback-Ingestion). NIE aus issue-closed auto-abgeleitet.
  Hier nur den Status vorsehen; die Uebergangsregel gehoert zu Tor 3.

## 12. Erwartete Wirkung (zu belegen, nicht vorwegnehmen)

- Meeting 20 „No-Go bearbeiten": nur der No-Go-Cluster + betroffene PBIs werden angefasst; die anderen ~12
  Cluster / ~26 PBIs behalten ID + Inhalt. Verfeinertes REQ-17 → referenzierende PBIs „needs re-clarify".
  DEC → betroffenes PBI „blocked". github-sync-view = wenige PBIs → GitHub-Agent erzeugt keine Duplikate.
- Der Core bleibt vollstaendige Wahrheit mit Historie; Downstream + GitHub arbeiten fokussiert auf dem Blast-Radius.
