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
  (`docs/aktiv/core-relationen-konzept.md`).
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
