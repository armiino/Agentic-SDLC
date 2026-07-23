# Plan 17.07 - Bauweise / Implementierungskonventionen fuer den RE-Backlog-Knoten

Status: Entwurf. Companion zu `plan-re-backlog.md` (Prozess) und `plan-pb.md` (Produkt).
Diese Datei haelt fest, WIE gebaut wird: MAF-nativ, Clean Code, bestehende Bausteine
wiederverwenden, DB-later-tauglich (View/Repository-Boundary). Geerdet an den realen Vorbildern
(gelesen 17.07): `l4/issuplanning/*`, `projectstate/*`.

**Benennung (entschieden):** CLI `l4-re-clarify` · Ordner `l4/reclarify/` · Agent `L4ReClarifyAgent`
· Klassen-Praefix `ReClarify`. Produkt-Artefakte bleiben `ProductBacklogView` / `product-backlog.json`.

---

## 1. Leitplanken (nicht verhandelbar)

- **MAF-nativ** wie die bestehenden Knoten: `Executor<TInput>` + `[SendsMessage]/[YieldsOutput]`,
  `WorkflowBuilder(...).AddEdge(...).WithOutputFrom(...).Build()`, `InProcessExecution.Default.RunAsync`.
- **Muster wie bisher:** Agent[Tools] -> deterministisches Gate -> Finalize; danach HumanReview -> Apply.
- **Wiederverwenden statt neu bauen** (siehe 2). Kein neues Orchestrierungs-Framework.
- **DB-later:** Knoten liest NUR ueber die View/Repository-Ports (`IProjectStateViewRepository`,
  `IProjectStateRepository`) - NIE direkte `File.Read` auf Projektwahrheit. Apply schreibt so, dass
  eine DB-Impl spaeter den JSON-Adapter ersetzt (siehe 6).
- **Generik:** Linsen/Kern-Set als Prompt + geteilte Spec (Analogon `CoverageSpec`/`DerivationSpec`),
  NICHT hartkodiert; deterministischer Assembler arbeitet ueber Relationen/Entitaeten/Text, nicht Keywords.

## 2. Reuse-Map (bestehende Bausteine)

```text
Run/RunContext, Run/RunId                     Run-Ordner, Events, WriteConfig, OutputDir  (wie IssuePlanningRunner)
Prompts/PromptProvider.Load                   Prompt laden (phase2_evidence, versioniert)
Llm/ChatClientFactory.Create                  Chat-Client aus HostSettings
Observability/AgentChatPipelineBuilder.Build  Input/Decision/Tool-Logging-Pipeline
Observability/ToolCallLoggerMiddleware        Tool-Call-Logging (agent.AsBuilder().Use(...))
Observability/OtelRunExporters.TryCreate      OTel JSONL
projectstate/IProjectStateViewRepository      Lese-Views (DB-Boundary)  -> NEUE View ergaenzen
projectstate/IProjectStateRepository          fachliche Ops (Relations/Provenance/Proposal/ApplyDecision)
projectstate/JsonProjectStateViewRepository   JSON-Impl der Views  -> NEUE View-Methode ergaenzen
AgenticSdlc.HumanReview (Projekt)             generische Review-UI  -> ReviewAdapter wie L4/IssuePlanning
l4/issuplanning/*                             DAS Vorbild fuer den ganzen Knoten (1:1 Struktur spiegeln)
```

## 3. Datei-Layout des neuen Knotens (spiegelt `l4/issuplanning/`)

Ordner `l4/reclarify/`, namespace `AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4` (wie issuplanning).

```text
ReClarifyModels.cs          PBI, FeatureContext, OpenDecision, PbiDocument, GateReport, RunSummary
ReClarifyAssembler.cs       DETERMINISTISCHE Kandidatenbildung (Feature-Kontext aus Relationen/
                            Source-Items/Entitaeten/Text) - Analogon IssuePlanFactory (Seed)
ReClarifyTools.cs           Agent-Tools (Build() -> AIFunctionFactory.Create), siehe 5
ReClarifyWorkflow.cs        3 Executors + Build()  (Analogon IssuePlanningWorkflow)
ReClarifyGate.cs            deterministisches, STRUKTURELLES Gate (nicht per-Linse), siehe plan-re-backlog §12
ReClarifyRunner.cs          CLI-Dispatch (agent/check/dry-run), View laden, Workflow bauen+laufen
ReClarifyReviewAdapter.cs   Mapping PbiDocument -> ReviewSession (HumanReview)  (Analogon IssuePlanningReviewAdapter)
ReClarifyReviewRunner.cs    Review-CLI
ReClarifyApply.cs           deterministischer Apply -> ProductBacklogView materialisieren
ReClarifyApplyModels.cs
ReClarifyApplyRunner.cs
```

Prompt: `Prompts/phase2_evidence/L4ReClarifyAgent/L4ReClarifyAgent1.txt`
(nie ueberschreiben; neue Version = neue Datei, wie CLAUDE.md verlangt).

## 4. MAF-nativer Graph (exakt nach IssuePlanning-Vorbild)

```text
Input: CanonicalRequirementsView (+ Relationen/Provenance ueber IProjectStateRepository)

ReClarifyAgentExecutor : Executor<TInput>            [SendsMessage(typeof(ReClarifyDraft))]
  baut ReClarifyTools(view, repo, run), agentFactory(tools.Build()), agent.RunAsync(task),
  liest tools.SavedPbis, run.AppendEvent(RE_CLARIFY_AGENT_DONE), context.SendMessageAsync(draft)
ReClarifyGateExecutor : Executor<ReClarifyDraft>     [SendsMessage(typeof(ReClarifyVerdict))]
  ReClarifyGate.Check(...), Event, SendMessage(verdict)
ReClarifyFinalizeExecutor : Executor<ReClarifyVerdict> [YieldsOutput(typeof(ReClarifyResult))]
  schreibt product-backlog.json + backlog-gate-report.json + backlog-summary.json + clarification-report.json,
  Event, context.YieldOutputAsync

Build: new WorkflowBuilder(agent).WithName("L4-ReClarify").WithDescription(...)
       .AddEdge(agent, gate).AddEdge(gate, finalize).WithOutputFrom(finalize).Build();
```

Runner (nach IssuePlanningRunner):
- View laden: `new JsonProjectStateViewRepository(repoRoot).GetCanonicalRequirementsViewAsync(ProjectScope.FromSourcePath(token, "re-clarify", "current_baseline"))`.
- `RunContext(RunId.New(), "l4-re-clarify")`, `run.EnsureFolders()`, `run.OutputDir("backlog")`, `run.WriteConfig(...)`.
- Prompt via `PromptProvider.Load`, Client via `AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), ...)`.
- `agentFactory = tools => genClient.AsAIAgent(instructions, name, tools).AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();`
- `InProcessExecution.Default.RunAsync(workflow, input, run.RunId, ct)`, danach Artefakte pruefen, Exit-Code.
- `--dry-run`: Graph nur Build()-bar, kein LLM (wie IssuePlanning).

## 5. Tools (Agent-Interface) - deterministisch, geloggt

Analog `IssuePlanningTools`: eine Klasse mit `Build() -> IReadOnlyList<AITool>` via
`AIFunctionFactory.Create(method, "name", "desc")`, jede Methode `run.AppendEvent(...)`, `save` genau einmal.

```text
list_requirements / search_requirements / get_requirement        (aus CanonicalRequirementsView)
get_related_requirements                                          (IProjectStateRepository.GetRelationsAsync)
get_feature_candidates    deterministischer Assemble-Seed (ReClarifyAssembler) - Feature-Kontexte
check_pbis                deterministischer Gate-Vorpass (ReClarifyGate.Check) - vor dem Speichern
save_pbis                 finale PBIs speichern (genau einmal)
```

Der Agent ERKUNDET ueber Tools (kein fs_read-Transport); `get_feature_candidates` liefert den
deterministischen Zusammenfuehrungs-Vorschlag, den der Agent verfeinern/schneiden (CUT) darf.

## 6. DB-Boundary konkret (der wichtigste Punkt fuer spaeter)

Lesen:
- Input ausschliesslich ueber `IProjectStateViewRepository` (bestehende `CanonicalRequirementsView`
  reicht als Start: Baseline + Provenance + Quality) + `IProjectStateRepository` fuer Relationen.
- KEIN direkter Datei-Zugriff im Knoten - der JSON-Pfad lebt nur in der Repository-Impl.

Schreiben (Apply):
- NEUE View `ProductBacklogView` (Scope + PBIs) in `ProjectStateViews.cs` + Methode
  `GetProductBacklogViewAsync(ProjectScope)` in `IProjectStateViewRepository`/JsonImpl.
- `ReClarifyApply` materialisiert die View (JSON heute) so, dass eine spaetere DB-Impl dieselbe
  Methode bedient. PBIs mit stabiler ID (deterministische Identitaets-Funktion, plan-pb Flag A) +
  `version`/`derivationHistory` - schon jetzt DB-freundlich (Upsert per pbiId spaeter trivial).
- Downstream (Readiness/IssuePlanning) konsumieren spaeter `ProductBacklogView` statt `issue-planning-input`.

So bleibt die Regel des Repository-Boundary-Plans (16.07) gewahrt: **Knoten kennen Views/Ops,
nicht Speicher.** DB-Anbindung = neue Repository-Impl, kein Knoten-Umbau.

## 7. Generik (kein Domaenen-Hardcode)

- Kern-Set (5 Fragen) + Volere/FURPS+-Hintergrund leben im **Prompt** + einer geteilten
  `ReClarifyLensSpec` (Analogon `CoverageSpec`), nicht als if/else im Code.
- `ReClarifyAssembler` gruppiert generisch ueber Relationen/gemeinsame Source-Items/Entitaeten/Text -
  keine No-Go-/CRUD-Sonderregeln.
- Gate prueft Struktur/Herkunft/Coverage-Invariante generisch (siehe plan-pb §9), nicht Inhalte.

## 8. Verifikation (CLAUDE.md-Disziplin)

- Zielbau: `dotnet build AgenticSdlc.Host/AgenticSdlc.Host.csproj --no-restore`.
- `--dry-run` (Graph Build()-bar, kein LLM) als erster Test - wie IssuePlanning.
- Erst danach echter Lauf auf Kern-Feature-Auswahl (plan-re-backlog §12), Belegfall No-Go-Trace.
- Kein teurer LLM-Lauf ohne User-Freigabe.

## 9. Reihenfolge (nach Freeze, klein + verifizierbar)

```text
1 ReClarifyModels + ProductBacklogView + Repo-Methode (leer)      -> Build gruen
2 ReClarifyAssembler (deterministisch) + get_feature_candidates   -> read-only, gegen No-Go-Trace testbar
3 ReClarifyGate (strukturell)                                     -> gegen HAND-PBIs testbar (kein LLM)
4 ReClarifyWorkflow + Tools + Runner + Prompt v1 + --dry-run      -> Graph steht
5 ReClarifyReviewAdapter + ReviewRunner (HumanReview reuse)
6 ReClarifyApply + ApplyRunner -> ProductBacklogView materialisieren
7 (spaeter) Readiness/IssuePlanning-Input auf ProductBacklogView
```

Jeder Schritt: Ziel / erwartete Beobachtung / Exit-Kriterium / Evidenzpfad.
