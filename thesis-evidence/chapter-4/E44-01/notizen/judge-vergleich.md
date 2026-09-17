## Iteration B19 - Kontrolliertes Judge-Modell-Head-to-Head: Evaluation ist stark modellabhaengig (Recall)

**Kontext:** Nutzer-Hypothese „die aktuelle Evaluation haengt zu stark vom Judge-Modell ab;
gpt-4.1-mini macht zu viele Fehler". Geprueft per **kontrolliertem** eval-offline auf den
EINGEFRORENEN Dokumenten von Run `20260616_125401_c0ca55`: gleiche Artefakte, gleiches Transkript
(T9999_chaos), gleiche Policy (`falseClaim=true, falseCertainty=true, missingTopic=true, batch=8`),
**nur das Judge-Modell variiert**. Bewertungsdateien liegen beide im jury/-Ordner von c0ca55
(`*.synth.openai_gpt-5-mini.evaluator.json` vs. `*.synth.openai_gpt-4.1-mini.evaluator.json`).

**Ergebnis (identische Inputs):**

| Artefakt | gpt-5-mini | gpt-4.1-mini |
|---|---|---|
| requirements | 23 | 4 |
| risks | 16 | 3 |
| architecture | 23 | 4 |
| open-questions | 24 | 0 |

→ 5–6-facher Score-Unterschied bei identischer Eingabe, open-questions 24 vs. 0. **Hypothese
bestaetigt: die Metrik ist massiv judge-modellabhaengig.**

**Fehler-Richtung praezisiert (qualitativ belegt):**
- gpt-4.1-mini = **False Negatives / Recall-Problem**. architecture: fand denselben echten
  FALSE_CLAIM wie gpt-5-mini (ELK/CloudWatch erfunden), uebersah aber 3 transkript-belegte echte
  Luecken (Secrets-Management „Ben: CI/CD braucht Secrets Management"; Dev/Test/Prod-Umgebungen
  „Farid: Wir brauchen Umgebungen"; Rabatt-Freigabe >15%). requirements/risks: 0 FALSE_CLAIM, waehrend
  gpt-5-mini erfundene konkrete Zahlen fing, die das Transkript explizit verneint („2 s", „99,5%",
  „10.000 Nutzer" gegen „Anna: Weiss ich nicht", „4 h RTO", „100 Req/Min").
- gpt-5-mini ist **kein sauberes Orakel**: bei open-questions flaggt es als offene Frage genannte
  Technologie-Optionen (WORM-Bucket, PostgreSQL-as-a-Service) als FALSE_CLAIM → eher False Positives.
  „Einfach gpt-5-mini" tauscht nur die Fehlerart und ist teurer.

**Verknuepfung zu B18:** B18 hatte (single-model) bereits festgestellt, dass `falseClaim=true`
wirkungslos bleibt, weil **Call 1 keine FALSE_CLAIM-Kandidaten erzeugt** → architecture rutscht
faelschlich auf Score 0. B19 quantifiziert dieselbe Ursache ueber Modelle hinweg: **die
Modellabhaengigkeit sitzt in Call 1 (Finding-Generierung/Recall), nicht in Call 2 (Verifikation).**
Kein Verifikationspass (DISK-7/8/9 = Praezision) kann Findings retten, die Call 1 nie erzeugt hat.

**Learning / Konsequenz:** Die Loesung ist NICHT „teureres Modell" (kaschiert, teuer, nicht
zukunftsrobust), sondern Judge-Last reduzieren + Methodik (vgl. `evaluationrecherche.md`). Designplan
→ **DISK-10** (G1 Judge einfrieren, G2 deterministische Kandidaten-Vorgenerierung fuer un-geerdete
Spezifika, G3 Call-1 pro Kategorie entkoppeln, G4 Kalibrierungs-Set). Bis dahin gilt fuer A/B/C: EINEN
Judge fixieren, Scores NICHT ueber Judge-Modelle hinweg vergleichen.

---

