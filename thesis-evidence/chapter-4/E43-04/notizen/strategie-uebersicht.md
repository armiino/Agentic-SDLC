# Phase 2B - Iteration Notes

## Ziel der Phase 2B

Phase 2B untersucht dieselbe fachliche Phase-2.1-Kette wie zuvor, aber mit einem anderen Kontexttransport:

```text
Context -> Requirements -> Risks -> Architecture -> OpenQuestions
```

Der Unterschied liegt nicht in der fachlichen Agentenrolle, sondern im Transportweg. In Phase 2A wurde der Kontext im Wesentlichen ueber das normale MAF-Message-Passing weitergereicht. In Phase 2B wird ein expliziter `artifact_state`-Ansatz getestet:

- der Context-Agent liest das Transkript und schreibt `state/context.md`;
- Custom Executors lesen und schreiben MAF Shared State;
- Specialist Agents bekommen benoetigte Upstream-Artefakte aus dem State in ihre Eingabenachricht injiziert;
- die Specialist Agents sollen `fs_read` nicht als Transportweg fuer `context.md` oder `docs/*.md` nutzen;
- jedes Pflichtartefakt wird weiterhin per `fs_write` geschrieben, damit die bestehende Artefakt- und Observability-Struktur vergleichbar bleibt.

Methodisch ist Phase 2B wichtig, weil sie prueft, ob ein MAF-naher Shared-State-Mechanismus den Kontextfluss kontrollierbarer macht, ohne die Specialist Agents komplett zu deterministischen Funktionen zu degradieren.

---

## Begriff & Bezug

- "Phase 2B" = Phase 2.1, Variante **B** (`phase2ContextStrategy = artifact_state`). Logisch bleibt es
  `phaseSelector: phase2_1`; nur der Output-Ordner ist `runs/phase2B/`. Variante A = `message_passing`
  (Ordner `runs/phase2_1/`), Variante C = `independent_source_reads` (geplant, `runs/phase2C/`).
- Implementierungsdetails + Designbegruendungen: `AgenticSdlc.Host/Phases/Phase2/Phase2B/IMPLEMENTATION_PLAN.md`.
- Schliesst an `phase2notes/phase02-iteration-notes.md` an (Iteration 27 = B-Implementierung,
  Iteration 28 = strategie-bedingte Prompt-Sections in run-config.json).

---

## Phase2B-Run-Uebersicht

| Run | Generator | Judge | Ergebnis | Kernaussage |
| --- | --- | --- | --- | --- |
| `20260613_153732_c73b5c` | `openai/gpt-4.1-mini` | `openai/gpt-4.1` | fehlgeschlagen | Context-Agent las Transkript, schrieb aber kein `state/context.md`; Artifact-Gate griff. |
| `20260613_154257_530bf0` | `openai/gpt-4.1-mini` | `openai/gpt-4.1` | fehlgeschlagen | Context-State wurde geschrieben, danach MAF-Protokollfehler: `Phase2BHandoff` durfte nicht gesendet werden. |
| `20260613_155156_0e6849` | `openai/gpt-4.1-mini` | `openai/gpt-4.1` | technisch erfolgreich | Erster valider `artifact_state`-Run; State wurde korrekt genutzt; Jury fand viele reale Luecken. |
| `20260613_163101_31bcc4` | `openai/gpt-4.1-mini` | `openai/gpt-4.1-mini` | technisch erfolgreich, Jury instabil | Guenstiger Judge erzeugte Parsefehler und false negatives. |
| `20260613_164010_eabd1a` | `openai/gpt-5-mini` | `openai/gpt-5-mini` | technisch erfolgreich, Jury teilweise invalid | Generator deutlich besser, aber Judge-JSON erneut nicht robust. |
| `20260613_200805_b4fb45` | `openai/gpt-oss-120b` | `openai/gpt-5-mini` | technisch passed, fachlich nicht verwendbar | Risks-Agent ueberschrieb gutes Artefakt mit Testinhalt; State wurde dadurch kontaminiert; Jury-Parsefehler maskierte den Fehler. |

### Fix-Timeline (Problem -> Ursache -> Fix -> Ergebnis)

Chronologische Bring-up-Kette der Phase2B-Pipeline (B1 -> B3 = bis zum ersten lauffaehigen Run):

| # | Run | Problem | Ursache | Fix | Ergebnis |
| --- | --- | --- | --- | --- | --- |
| B1 | `…c73b5c` | Context-Agent schrieb kein `context.md`, Workflow brach am Artifact-Gate ab | transienter Modell-Flake ("announce instead of act"), KEIN Code-Bug (durch B2 belegt) | kein Code-Fix noetig; Gate fing es korrekt ab; B2 = schlichter Re-Run | Gate-Mechanismus als sinnvoll bestaetigt |
| B2 | `…530bf0` | nach erfolgreichem `context.md`-Write: `Executor 'Phase2ContextAgent' cannot send messages of type …Phase2BHandoff` | MAF-Protokollvertrag: ausgehende Message-Typen muessen am Executor deklariert sein | `[SendsMessage(typeof(Phase2BHandoff))]` an Context + Specialists, `[YieldsOutput(typeof(string))]` am letzten Specialist | Workflow-Routing freigeschaltet |
| B3 | `…0e6849` | — | — | (laeuft mit dem B2-Fix) | erster valider `artifact_state`-Run; State-Transport + -Isolation belegt |

Ab B3 ist die Pipeline lauffaehig; B4–B6 sind keine Bring-up-Fixes mehr, sondern Erkenntnisse zu
Judge-Stabilitaet (B4/B5) und State-/Write-Qualitaet (B6) — siehe die jeweiligen Iterationen und
"Offene Anpassungen".

---

