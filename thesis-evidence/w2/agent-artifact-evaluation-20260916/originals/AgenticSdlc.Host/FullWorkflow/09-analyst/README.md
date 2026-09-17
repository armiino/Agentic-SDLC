# 09-analyst — der Core-Analyst („Agent für das Ungesagte")

> Status: LEBEND — bei Änderungen an diesem Ordner mitpflegen. Design-Beschluss: `Thesis-Docs/aktiv/core-analyst-design.md`.

**Was dieser Ordner ist:** KEIN Kettenglied (01–08 sind die Meeting-Kette) — sondern die **bestellbare
Analyse-Fähigkeit** über den Core-STAND: „Was fehlt, das niemand gesagt hat?" Die unbesetzte RE-Rolle
(Vervollständigung), als kleiner MAF-Graph, dessen Maker-Knoten Agenten sind.

## Der Graph (AnalystWorkflow.Build — je Lauf frisch gebaut, State-Isolation)

```
Trigger ─► [AnalystDispatch = Collect, det.]           Digest (aktive Wahrheit) + Kollektor-Funde
              │  Fan-out (Kanten-Prädikat je Lens.Key)
              ▼
   [Lens-funktional] [Lens-nfr] [Lens-arch] [Lens-risiko]   Maker-Agenten, check-vor-save (AnalystLensTools)
              │  Fan-in-BARRIER
              ▼
        [AnalystMerge, det.]     Dedup: Linsen-Dubletten · Core · Entscheidungen · R-35-Ablehnungen
              ▼
        [AnalystKritiker]        Substanz-Filter (LLM) — Aussortiertes bleibt SICHTBAR im Report
              ▼
        [AnalystPersist]         report.md/json + delta.json → runs/core-analysis/<id>/analysis/
```

**Kein Gate, kein Wahrheits-Write:** der Lauf LIEST nur. Die Wirkung entscheidet der Autor danach separat —
`pipeline-full run --from-delta <delta.json>` → Tor 1 legt jeden Fund einzeln vor (Ablehnungen füttern das
R-35-Gedächtnis; der Analyst schlägt Abgelehntes nie wieder vor).

## Rauschen-Trichter (vierstufig)

1. **Form-Checker IM Maker** (`AnalystLensTools.save_findings`): Core-Anker-Pflicht (mind. 1 existierendes
   Item), Kategorie/Disposition-Validierung, Herleitungs-Pflicht — Fehlerliste zurück, Agent iteriert.
2. **Deterministischer Dedup** (Merge): gegen aktive Items, ALLE DECs, REJ-Einträge, Linsen-Dubletten.
3. **Kritiker** (adversarial): projektspezifisch + vorlagefähig? Aussortiert = sichtbar mit Grund.
4. **Human-Gate** (Tor 1) + R-35-Gedächtnis.

## Verträge

- **Funde** → `AnalystDeltaBuilder`: Autor-Front-Format, `origin: CoreAnalyst`, Metadata `linse` +
  `analystKategorie` (functional | nfr:&lt;merkmal&gt; | process — strukturiert das Anforderungsdokument) +
  wörtliche `herleitung` + `ankerIds`.
- **Report**: NEU vs. WEITERHIN OFFEN (Vergleich gegen jüngsten Vorgänger-Report per IdentityKey) vs.
  aussortiert/dedup — kein stiller Cap, alles sichtbar.
- **Bedienung (K13, eine Naht `AnalystRunner.RunCoreAnalysisAsync`):** CLI `core-analysis run` ·
  Steward `run_core_analysis` (⚿; Funde reisen IM Ergebnis mit) + Lese-Tool `read_analysis_report`
  (Klasse-Regel, 20.08.); der Tor-Lauf ist IMMER ein separates ⚿ des Autors.
- **Aussortierte retten = Diktat** (save_author_statements, wörtlich — Adoption ist dann des AUTORS,
  Herkunft AuthorFront). NIE ins Analyst-Delta schreiben (Run-Artefakt = Beleg). Kritiker-aussortiert ≠
  abgelehnt: R-35-Gedächtnis entsteht NUR durch Gate-Ablehnung — Aussortiertes darf wiederkommen.
- **Delta = Datei, Läufe nacheinander:** run_pipeline_from_delta fährt jedes Delta (Analyst, Diktat, …);
  ein aktiver Lauf je Steward-Sitzung; Delta-Dateien bleiben Beleg; Doppelt-Fahren = ungefährlich (N3).

## Bewusste Grenzen (v1)

Nur auf Zuruf (Bootstrap-Opt-in = Nach-W2-⚖) · Risiko-Linse liefert FRAGEN (9g-Rampe; Risk-Typ mit
definiertem Auslöse-Trigger nach W2) · Linsen-Anzahl = Stellschraube (Slice S ① 21.08.: fünfte Linse
„Persona-Abdeckung" — läuft NUR, wenn `docs/personas.md` freigegeben existiert [AuthoredDocument];
ihr Auftrag trägt die Personas als linsen-eigenen Kontext, Funde nur aus „Belegt"-Zonen) ·
kein privates Agent-Memory
(Gedächtnis = Blackboard: Core + REJ + DEC + Vorgänger-Report). Begründungen: Design-Beschluss §5–§8.
