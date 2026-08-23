# 05-core — Die Projektwahrheit & ihr Wachhund

> Status: LEBEND (README-bei-Code, B3 07.08.2026).

## Rolle in der Kette

Hier wohnt der Zugriff auf die EINE lebendige Projektwahrheit (`state/core/project-state.json`, Repo-Wurzel —
bewusst NICHT im Run-Ordner: Läufe sind Episoden, die Wahrheit ist lauf-übergreifend) und alles, was sie
schützt und projiziert.

## Bausteine

- **`ICoreRepository`** — DIE Naht (auch DB-Swap-Naht: eine spätere DB-Implementierung ersetzt nur diese;
  Invariante: der Kangal-Aufruf gehört zur Save-Semantik JEDER Implementierung).
- **`JsonCoreRepository`** — Datei-Implementierung; jeder inhaltsändernde `SaveAsync` schreibt VORHER einen
  History-Snapshot (`state/core/history/`, lokal) und ruft **`CoreKangal.Check`**.
- **`CoreKangal`** (S4, „der Wachhund") — EINE Prüf-Logik, EIN Aufruf am Save: I1-Endpunkt-Matrix je
  Relationstyp (`part_of_feature` pbi/req→feature · `covers` pbi→**Wahrheits-Item** [req|arch, E-R2] ·
  `constrained_by(+ _superseded)` pbi→arch · `supersedes` **aspekt-GLEICH** [R-39] · `contradicts(+_resolved)`
  dec→any · `implemented_by_issue` 1:1) + weitere Invarianten (I2–I8 lt. Spec). I1/I5 = Abbruch, Rest = laute
  Warnung; unbekannte Typen = `UNKNOWN_RELATION_TYPE`-Warnung. **Spec zuerst, Wächter folgt**
  (`Thesis-Docs/aktiv/core-relationen-konzept.md`).
- **`CoreViews`** — deterministische Projektionen: `GithubSync` (je PBI: covered-REQs REIN req ·
  `Constraints` [constrained_by] · `CoveredArchitecture` [work-Umsetzung] → Issue-Body-Sektionen) ·
  `AffectedItems` (Blast-Radius, 1 Hop + 2. Hop über Rahmen-Kanten, byAspect-Bucket `Architecture`).
- **`RequirementSwap` / `ConstraintSwap`** — die geteilten Wahrheitsübergänge bei Ablösung: covers-Swap
  (req) bzw. constrained_by-UMZUG (arch; alte Kante → `constrained_by_superseded` mit movedTo/decisionId,
  Rev-2-Muster: nie löschen). Nutzer: Tor 2 ADOPT_NEW UND pbi-update — EINE Quelle.
- `CoreGithubMapping` — PBI↔Issue-Mapping als Core-Relation (persistent, T3.1).

## Merksätze

GitHub ist Projektion, nie Quelle · kein Schreiber kommt am Kangal vorbei (auch künftige Steward-Pfade
nicht — der Steward schreibt ohnehin nie selbst) · Beweis-Cores parken als `state/core-*`-Ordner.

## Der C4-Kreislauf — Architektur-Unklarheiten mit Lebenslauf (22.08.2026)

**Das Problem:** In echten Projekten verrotten Architektur-Fragen — sie stehen in einem Diagramm oder
Protokoll, niemand trackt sie, irgendwann sind sie „irgendwie geklärt", ohne dass nachvollziehbar ist,
WAS jetzt gilt. Der Kreislauf gibt jeder Architektur-Unklarheit einen **Lebenslauf**, den das System
erzwingt statt erhofft.

### Der Lebenslauf einer Lücke (die eine Grafik zum Verstehen)

```
?-Kasten im C4  ──einkippen──►  DEC im Entscheidungs-Topf (aspect=architecture)
                                        │
              ┌─────────────────────────┼──────────────────────────┐
              ▼                         ▼                          ▼
   ANTWORT-DIKTAT              „keine Festlegung nötig"    nur mit Worten geschlossen
   (decisionRef=DEC-x)         (3. Gate-Option,            (KEEP mit Begründung)
        │                       Begründung Pflicht)                │
        ▼                               │                          ▼
   ARCH/ADR-Wahrheit                    │                  ⚠ „geklärt ohne
   trägt answersDecision                │                   Architektur-Nachweis"
        │                               │                   (bleibt stehen, bis
        ▼                               ▼                    Wahrheit nachkommt)
   Lücke verschwindet NACHWEISBAR aus §3 · Wächter mahnt den Diagramm-Kasten an
        │
        ▼
   „aktualisiere C4" → ?-Kasten wird belegter Kasten (Autor-Freigabe)
```

### Die Bausteine (alle in dieser Stufe bzw. an geteilten Nähten)

| Baustein | Datei | Was er tut |
|---|---|---|
| Aspekt-Färbung | `DecisionKnowledgeMeta.cs` | `aspect=architecture` an DECs — gesetzt von der Quelle (Steward-Einkipp, Analyst-arch-Linse, Diktat); NUR von 3 additiven Konsumenten gelesen, ungefärbte DECs verhalten sich byte-identisch (gepinnt) |
| Antwort-Anker | `DecisionKnowledgeMeta.cs` | Diktat mit `decisionRef=DEC-x` ⇒ das entstehende Wahrheits-Item trägt `answersDecision` (Carry über die geteilte IngestMeta-Naht) — macht „Klärung hat Wahrheit erzeugt" DETERMINISTISCH prüfbar; wirkt auch nachträglich (Heilung) |
| §3-Projektion | `C4GapSection.cs` | die Sektion „Offene Architektur-Lücken" in `docs/c4.md` ist KEIN Text mehr, sondern eine Projektion des DEC-Topfs (Section-Replace, Diagramme unangetastet, idempotent); Zustände: offen · ⚠ · sauber-weg (Nachweis/Verzicht/Ziel-Auflösung) |
| Beleg-Stand-Wächter | `AuthoredDocDrafting.C4FrischeNotiz` | meldet in der Lage-Übersicht NUR das DELTA seit dem letzten Autor-C4-Save (Stempel in §3, R-71) + zitierte ABGELÖSTE Belege — nie den Voll-Bestand (Diagramm-Vollständigkeit ist Deutung) |
| Zeichner | `AuthoredDocDrafting.cs` + `ArtifactDraftAgent/C41.txt` | Diagramme = Redaktion: Agent entwirft NUR Belegtes (Input: Digest + volle ARCH/ADRs + WAHRHEITS-KANTEN), Autor gibt frei (⚿); ?-Kästen tragen DEC-Ids |
| Gate-Wachen | `IngestionGate` (ANSWER_NEEDS_TRUTH, R-74) · `DecisionResolutionGate/-Derivation` (NO_TRUTH_NEEDED nur ziellos+aspekt) | deterministische Zäune: eine Antwort kann nicht in die Frage gefaltet werden; der Verzicht ist nur dort wählbar, wo er gilt |

### Das Drei-Stufen-Garantie-Modell (Kernaussage für die Thesis)

1. **Nie still falsch** (deterministisch): Anker-Transport, Projektion, Wachen — kein Prompt kann
   Konsistenz brechen.
2. **Immer sichtbar** (deterministisch): jeder Fehlweg landet als ⚠, Gate-Fehler oder Wächter-Delta —
   nie im Nichts. (Live bewiesen: R-73/R-74 — zwei Modell-Patzer, beide vom Netz angezeigt, null Schaden.)
3. **Komfort** (Prompt): das ERKENNEN „dieses Diktat beantwortet DEC-x" ist LLM-Semantik; Degradation
   definiert = ein expliziter Autor-Satz bzw. ⚠. Struktur-Endform (parkiert, `aufgefallen.md`):
   vierter Gate-Ausgang „beantworten mit Festlegung" — dann ist auch das Stufe 1.

### Deklarierte Grenzen

Kein Risiko-Register (Risiken = DECs via QuestionLane) · Diagramm-Vollständigkeit ist bewusst Deutung
(nur der Analyst ERSCHLIESST Fehlendes, gated) · DEC-Matching skaliert abfrage-förmig (ListCap 50, laut;
Retriever-Naht vorbereitet) · L3/L4 erst, wenn der Core Komponenten-Wahrheit trägt.

**Bau-Historie + Live-Abnahme-Belege:** `Thesis-Docs/aktiv/team-sichtbarkeit-slice.md` (⚖-Serie) ·
E2E-RUNBOOK R-63…R-74 (jeder Fund der Abnahme mit Fix).

## Die Story-Map-Tafel — die Nutzer-Reise als lebendes Fortschritts-Bild (22.08.2026)

Gleiche Bauform wie der C4-Kreislauf, auf das Backlog angewendet (`docs/storymap.md`, Endform +
Szenario-Katalog S1–S14: `Thesis-Docs/aktiv/leitfaden-abdeckung.md §4/§4a`). Zwei Zonen:

- **Erzähl-Zone (Redaktion, ⚿):** Reise-Backbone der Kernpersona (Schritte ≠ Features!), je Schritt ein
  Satz, MVP-Vorschlag (Momentaufnahme der Scoping-Phase). Entwurf vom Drafting-Agenten — die Zuordnung
  Schritt→PBI kommt als TYPISIERTER Vertrag (`ForJsonSchema` + `ChatClientAgentRunOptions`), nie als
  Prosa-Rückparse.
- **Tafel (deterministisch, `StoryMapSection`):** Kärtchen = PBI-Story (`goal`) · Prio · ✓ fertig
  (Issue-Kreislauf!) · ⚠ in Klärung (keine erfundene Story für Unklares) — live aus dem Core bei jedem
  Berühren (Save + Refresh-Naht). Uneingeordnete aktive PBIs landen sichtbar im Sammelbecken; die
  Zuordnung lebt als Stempel-Kommentar im Doc (`ManagedDocSection` = die aus C4 extrahierte geteilte
  Mechanik). Beleg-Wachen am Save: unbekannte Id/Doppel-Zuordnung/leere Reise = LAUT.

Frische (Lage-Frage, pull): nur „N PBIs warten auf Einordnung" + „Personas neuer als die Reise" —
kein PBI-Churn-Rauschen, denn Kärtchen-Inhalte ziehen von selbst nach. Pins: `StoryMapSectionTests`.

## Die drei Sichten aufs Backlog — warum die Aufteilung so ist (⚖ Autor 23.08.2026)

EIN PBI, DREI Sichten — jede mit einem anderen Job, alle aus derselben Quelle gerendert:

| Sicht | Real-Welt-Äquivalent | Job | zeigt |
| --- | --- | --- | --- |
| `docs/backlog.md` | Board-/Listen-Ansicht (Jira/ADO) | **steuern** | Titel · Status · Klärung · Prio · Schätzung · Requirements · Issue-Nr |
| GitHub-**Issue** | das geöffnete Work Item | **arbeiten** | die VOLL-Form: Story (`goal`) · AKs · Rahmen · Herkunft (Stil V2) |
| `docs/storymap.md` | Story-Map-Wand (Miro) | **verstehen & schneiden** | Reise + Story-Sätze + ✓-Fortschritt + MVP-Vorschlag |

Kein Tool der Branche zeigt AK-Texte in der Listen-Ansicht — die Trennung Liste ↔ Item-Detail ist
Standard. **Der Unterschied zum Standard ist nicht die Aufteilung, sondern die Drift-Freiheit:** in
realen Projekten laufen Board/Wiki/Map auseinander (die Map stirbt nach dem Workshop); hier KÖNNEN die
Sichten nicht driften, weil alle aus dem Core gerendert werden — einzige Redaktion ist die Reise-
Einordnung (versioniert, ⚿, Beleg-gewacht).

**Sync-Garantien (belegt, nicht behauptet):** EIN Wahrheits-Schreibpunkt (`JsonCoreRepository.SaveAsync`
+ Kangal) → Refresh der Projektionen an der geteilten Naht (`RefreshDeterministicProjectionsAsync`,
zusätzlich am Artefakt-Save) · Schein-Änderungs-Wachen (Fingerprint R-63 · SameBody R-72) · Publikation
NUR gated (forward-gate, In-Sync-Stempel). Beweise: §3-Manipulations-Heilung (Lauf `20260822_162504`,
GitHub war nie falsch → null Ops) · Leitfaden-Livetest (Lauf `20260822_220612`: exakt 1 Doc-Upsert +
1 legitimes Issue-Update, 42× NO_CHANGE).

**Deklarierte Schwächen (bewusst benannt):** `backlog.md` überlappt teilweise mit GitHubs eigener
Issue-Liste (Mehrwert: Feature-Gruppierung, Requirements-Verweise, Schätzung, im Repo versioniert) ·
drei Sichten = Lern-Moment „wo schaue ich wofür" (gemildert durch Quer-Verweise) · die Story Map trägt
erst voll nach der Scoping-Runde (Prio/✓ leben dann).
