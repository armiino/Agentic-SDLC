# `docs/` — die publizierten Projekt-Artefakte

> Status: LEBEND (README-bei-Code, angelegt 17.09.2026) — bei neuen Arten mitpflegen.

**Alle Dateien in diesem Ordner werden erzeugt. Von Hand bearbeiten lohnt nicht: die nächste
Erzeugung überschreibt die ganze Datei.**

Die Quelle ist immer die Projektwahrheit in `state/core/project-state.json`. Dieser Ordner ist
die Außensicht darauf — dieselbe Rolle wie die GitHub-Issues: **Projektion, nie Quelle.**
Wer einen Inhalt ändern will, ändert ihn im Core (über ein Gate) und erzeugt neu.

## Zwei Klassen

### A · Autor-Artefakte — Entwurf vom Agenten, Freigabe vom Menschen

Feste Whitelist in `05-core/AuthoredDocument.cs` (`Arten`); der Steward kann keine anderen
Dateien schreiben.

| Datei | Titel | Stand |
|---|---|---|
| `vision.md` | Produktvision | vorhanden |
| `personas.md` | Personas | vorhanden |
| `c4.md` | Architektur-Landkarte (C4) | vorhanden |
| `storymap.md` | User Story Map | vorhanden |
| `glossar.md` | Glossar | **in der Whitelist, noch nie erstellt** |

Ablauf: der Steward entwirft aus dem Core (`draft_authored_doc`), legt den Entwurf wörtlich im
Chat vor, und erst die Zustimmungsabfrage von `save_authored_doc` ist die Freigabe. Jeder Save
ersetzt die **ganze** Datei und schreibt den Kopf selbst:

```
> Version: 5 · Stand: 2026-08-22 16:16 UTC
> Freigabe: Autor (Steward-Chat) · Entwurf: Steward aus der Projektwahrheit (Core)
```

Teile dieser Dateien sind auch für den Agenten tabu: Die C4-Sektion „Offene Architektur-Lücken"
und die Kärtchen-Tafel der Story Map rendert das System deterministisch aus dem Core.

### B · Deterministische Projektionen — kein Entwurf, kein Urteil

| Datei | Erzeugt von | Ausgelöst durch |
|---|---|---|
| `anforderungen.md` | `05-core/RequirementsDocumentProjection.cs` | `requirements-doc` bzw. das Steward-Werkzeug `render_requirements_doc` |
| `backlog.md` | `05-core/BacklogDocumentProjection.cs` | frisch beim Backlog-Seed und bei jedem Forward |
| `architecture.md` + `adr/` | `07-tore/adr/AdrProjection` | das `adr-gate` beim Übernehmen eines ADR |

Diese Dateien tragen den Hinweis bereits im Text:

> *„Projektion aus der Projektwahrheit (Core) — GENERIERT, nie von Hand pflegen."*

`adr/` enthält die Architekturentscheidungen im MADR-Format plus einen generierten `README.md`-Index.

## Veröffentlichung

Die Dateien landen nicht von selbst im Team-Repo. Das **Doc-Publish** nimmt jede vorhandene Art
beim nächsten Forward mit — gated wie jeder andere Schreibvorgang, und nur wenn
`run-config.json → fullworkflow.execute` auf `true` steht.

## Sonderstellung im Repo

`docs/` ist seit dem Speicherort-Schnitt vom 23.08.2026 die **reine System-Zone** und normal
versioniert (keine `git add -f` nötig). Die Autor-Notizen liegen getrennt davon unter
`Thesis-Docs/` und werden nie committet.
