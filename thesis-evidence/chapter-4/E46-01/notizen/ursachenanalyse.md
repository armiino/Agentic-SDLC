## 5. Integritäts-Messung (die Taten) — Ergebnis: weitgehend SAUBER, 1 echter Defekt

| Prüfung | Ergebnis |
|---|---|
| PBIs ohne Feature (Waisen) | **0** / 43 ✅ |
| aktive Requirements ohne PBI-Abdeckung | **0** / 90 ✅ |
| `covers` → superseded Requirement (R-34-Fall) | **0** ✅ (latent, nicht akut) |
| Features ohne PBI | **0** / 14 ✅ |
| Items ohne sourceRunId | **0** / 194 ✅ |
| Duplikat-Relationen | **0** ✅ |
| History-Snapshots je Save | ✅ implementiert + getestet |
| **`part_of_feature` → nicht-existentes Ziel** | ❌ **12 Relationen zeigen auf den featureKey-STRING („no-go") statt auf ein Feature-Item** — Verursacher: `IngestionApply` NEW_RELATED schreibt `AddRel(req, op.FeatureKey, "part_of_feature")` (**NEU → R-36**) |
| Requirements ohne Claim-Anker | 16: 13×L3 (by design — Anker = HDEC/CAND-Referenz) + **REQ-36, REQ-76, REQ-77 ohne jede Claim-Referenz** (76/77 = E11-Ingest-Zugänge) → kleiner Trace-Riss, R-31-Familie |
| „Hängende" Relationen gesamt | 185 — davon **173 by design** (Verweise auf Nicht-Core-Artefakte: 104 Claims · 39 `gh#n`-Issues · 15 HDEC · 15 CAND) + die 12 echten (s. o.) |

