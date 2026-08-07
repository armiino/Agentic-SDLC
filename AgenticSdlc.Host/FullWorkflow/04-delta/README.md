# 04-delta — Das Meeting-Delta & die Core-Datenmodelle

> Status: LEBEND (README-bei-Code, B3 07.08.2026) — bei Modell-/Vertrags-Änderungen mitpflegen.

## Rolle in der Kette

`04-delta` ist ZWEIERLEI: (a) die **Stufe**, die aus den Baselines das *Meeting-Delta* formt
(`project-state.json` im Run: die aufbereiteten Items DIESES Meetings — der Betriebs-Eingang für Tor 1),
und (b) das **Zuhause der geteilten Datenmodelle**, die die ganze Kette spricht.

## Die zentralen Typen (alle hier)

- **`ProjectStateDocument`** — Items · Relations · Sources · Proposals; schemaVersion **4**.
- **`ProjectStateItem`** — EIN Wahrheits-/Arbeits-Item (requirement · architecture · pbi · feature ·
  decision · open_question). Provenienz: Origin/SourceRunId/SourceClaimIds/History (Versionen frieren die
  alte Fassung ein — „die alte Herkunft lügt nie").
- **§5-Statusmodell (`CoreStatus.cs`):** der Alt-String `Status` ist nur noch PROJEKTION — die Wahrheit sind
  die typisierten Achsen `Validity` (Active/Superseded/Done) · `Progress` · `Blocker` (None/NeedsClarify/
  BlockedByDecision) · `Confirmation` · `DecisionState`. Schreiben NUR über `WithStatus(...)`
  (History-Notiz inklusive), Lesen über `ReadStatus()`, Eskalation über `Escalate(...)`.
- **Payloads je itemType** (`ProjectStateBacklogPayloads.cs`): `PbiPayload` (Goal/Title/AK/Linked/DecisionRefs)
  · `FeaturePayload` · **`ArchitecturePayload`** (R-11: `Roles` constraint|work|design · `Rationale` ·
  `AdrId`/`AdrStatus` = A5-Projektion).
- **Relationen** (`ProjectStateRelation`): typisierte Kanten — Endpunkt-Regeln wacht der Kangal (05-core);
  die Semantik-Spec ist `docs/aktiv/core-relationen-konzept.md` (Spec-zuerst-Regel!).

## Verträge / Nähte

- `JsonProjectStateRepository` lädt/schreibt Delta-Dokumente (Run-Artefakte) — NICHT den Core (der wohnt
  hinter `ICoreRepository`, s. 05-core).
- Das Delta eines Laufs liegt unter `runs/**/04-delta/project-state.json`; die arch-aktiv-Scopes der
  späteren Stufen (Classify/ADR/A4-Scan) prüfen GENAU diese Datei („hat DIESER Lauf arch berührt?").

## Tests

Modell-/Statusachsen-Tests in `AgenticSdlc.Tests` (u. a. Status-Migrations-Golden, Payload-Roundtrips).
