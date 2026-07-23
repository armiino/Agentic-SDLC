# Plan 16.07 — Repository/View Boundary

## Ziel

Der naechste Schritt ist nicht direkt GitHub und nicht direkt eine echte Datenbank.

Der naechste Schritt ist eine saubere Repository-/View-Grenze:

```text
Agent/Workflow-Knoten
  -> fachlicher View
  -> Repository-Port
  -> heute JSON/Run-Artefakte
  -> spaeter Datenbank
```

Damit sollen L4 Completion, Readiness, IssuePlanning und spaetere Knoten wie GitHub Reconciliation oder Sprint Delta
nicht mehr an konkrete Dateipfade gebunden sein. Die Agenten sollen fachliche Umwelt konsumieren, nicht wissen, ob diese
aus `runs/.../*.json`, SQLite, Postgres oder einem anderen Backend kommt.

Wichtig: User Stories sind nur ein moeglicher spaeterer Artefakttyp. Der Code soll nicht speziell fuer User Stories
gebaut werden. Er soll so erweiterbar sein, dass neue Artefaktarten und Modi ueber Config, Views und Repository-Adapter
eingebunden werden koennen.

## Warum jetzt

Wir haben inzwischen einen funktionierenden End-to-End-Durchstich:

```text
L1/L2/L3
  -> ProjectState
  -> L4 Consolidation
  -> L4 Apply
  -> L4 Quality
  -> Requirements Document
  -> L4 Completion
  -> Completion Review/Apply
  -> Requirements Readiness
  -> L4 IssuePlanning
  -> IssuePlanning Review/Apply
  -> accepted-issue-plan.json
```

Der Stand ist fachlich stark genug, um den naechsten Anschluss zu bauen. Gleichzeitig waere es zu frueh, sofort eine
produktive Datenbank oder einen schreibenden GitHub-Agenten zu bauen.

Wenn wir jetzt direkt GitHub Reconciliation auf konkrete Run-Dateien setzen, entsteht wieder Dateipfad-Kopplung. Wenn wir
jetzt direkt eine DB bauen, legen wir Schemaentscheidungen fest, bevor Sprint-Delta, GitHub-Mappings, Issue-Reconciliation
und weitere Artefakttypen ausreichend validiert sind.

Die Repository-/View-Boundary ist deshalb der richtige Zwischenschritt:

- klein genug fuer JSON-first
- stark genug fuer spaetere DB
- kein Scheingenerik-Framework
- keine verfruehte DB-Festlegung
- Grundlage fuer austauschbare MAF-Knoten

## Leitprinzipien

### 1. Knoten konsumieren Views, keine Dateien

Schlecht:

```text
l4-issuplanning agent runs/l4-completion/.../completion/applied
```

Zielbild:

```text
l4-issuplanning agent --scope current_baseline
```

Oder in einem spaeteren Graph:

```json
{
  "projectId": "pflege-app",
  "analysisType": "initial-analysis",
  "scopeType": "current_baseline",
  "viewType": "issue_planning"
}
```

Der Runner darf fuer den Uebergang weiter Pfade akzeptieren. Intern soll er aber moeglichst frueh in einen fachlichen
View uebersetzen.

### 2. Repository-Port statt DB-Abhaengigkeit

Wir bauen jetzt keine Datenbank. Wir bauen Ports und JSON-Adapter.

```text
IProjectStateRepository
  -> JsonProjectStateRepository
  -> spaeter DbProjectStateRepository
```

Der DB-Typ ist bewusst offen. Es muss nicht SQLite sein. Die Schnittstelle soll so geschnitten sein, dass spaeter SQLite,
Postgres oder eine andere Persistenz moeglich ist.

### 3. Generisch dort, wo es fachlich stabil ist

Nicht alles soll abstrakt werden.

Requirements bleiben Requirements, wenn der Knoten wirklich Requirements verarbeitet. Aber die Umgebung soll generisch
genug sein, um spaeter andere Itemtypen zu tragen:

```text
requirement
open_decision
architecture_decision
risk
issue_plan
user_story
test_case
sprint_delta
...
```

Neue Typen sollen nicht bedeuten, dass Agententools freie Dateien lesen. Sie sollen als neue Views oder als neue
View-Projektionen aus dem ProjectState entstehen.

### 4. Modi und RunConfig steuern Verhalten

Langfristig sollen Modi nicht durch neue Spezial-Runner wild wachsen, sondern ueber Config/View-Auswahl steuerbar sein.

Beispiele:

```text
analysisType = initial-analysis
analysisType = sprint-delta
analysisType = requirements-refresh
analysisType = architecture-impact
analysisType = issue-planning-only
analysisType = github-reconciliation
```

Diese Modi bestimmen, welcher Scope und welcher View gebaut wird. Der Agentenknoten bleibt moeglichst austauschbar.

### 5. Project State bleibt fachliche Wahrheit

Markdown, Requirements-Dokumente und GitHub-Issues sind Projektionen oder operative Ebenen.

Autoritativ bleibt:

```text
ProjectState
  - Items
  - Versionen
  - Relationen
  - Provenienz
  - Decisions
  - Runs
  - IssuePlans
  - spaeter GitHub-Mappings
```

## Aktueller Ist-Zustand

Gut:

- JSON-first ProjectState existiert.
- `project-state-build` erzeugt einen stabilen Snapshot.
- L4 arbeitet auf einem ProjectState-Dokument.
- L4 erzeugt eine kanonische Requirements-Baseline.
- `requirements-readiness` erzeugt einen sauberen `issue-planning-input.json`.
- `l4-issuplanning` arbeitet bereits auf einem eingeschraenkten View, nicht auf rohen L1/L2/L3-Artefakten.
- Agententools sind fachlich:
  - `list_issue_planning_items`
  - `search_issue_planning_items`
  - `get_issue_planning_item`
  - `get_seed_issue_plan`
  - `check_issue_plan`
  - `save_issue_plan`

Noch nicht ideal:

- Runner und Resolver kennen noch konkrete Run-Pfade.
- Readiness, Requirements-Dokument und IssuePlanning laden direkt aus Dateien.
- Es gibt noch keinen klar typisierten Scope-Vertrag.
- Es gibt noch keine zentrale View-Aufloesung:
  - `GetCanonicalRequirementsView(scope)`
  - `GetReadinessView(scope)`
  - `GetIssuePlanningView(scope)`
  - `GetAcceptedIssuePlan(scope)`
- GitHub-Reconciliation wuerde aktuell wahrscheinlich wieder direkt auf Dateien zugreifen, wenn wir die Boundary nicht
  vorher bauen.

## Zielarchitektur

```text
RunConfig / Graph Config
  -> ProjectScope
  -> Repository
  -> ViewBuilder
  -> Agent/Workflow-Knoten
  -> Gate
  -> HumanReview
  -> Apply
  -> Repository/ProjectState Update oder neues Proposal-Artefakt
```

### ProjectScope

Ein Scope beschreibt, welchen Ausschnitt des Projektzustands ein Knoten sehen darf.

Beispiel:

```json
{
  "projectId": "pflege-app",
  "analysisType": "initial-analysis",
  "scopeType": "current_baseline",
  "baselineId": "BASE-20260716",
  "itemIds": [],
  "includeMappings": false,
  "includeLatestQuality": true
}
```

Moegliche `scopeType`:

```text
current_baseline
affected_items
changed_or_stale_items
ready_without_mapping
items_with_changed_github_state
explicit_item_ids
full_audit
```

### Views

Views sind fachliche Projektionen aus dem ProjectState.

Erste Views:

```text
CanonicalRequirementsView
ReadinessView
IssuePlanningView
AcceptedIssuePlanView
ProvenanceView
```

Spaetere Views:

```text
SprintDeltaView
ArchitectureImpactView
StoryPlanningView
GithubReconciliationView
TestPlanningView
```

Wichtig: Ein View ist kein DB-Table-Design. Ein View ist der fachliche Inputvertrag eines Knotens.

## Meilenstein 1 — Bestehende ProjectState-Schicht inventarisieren

Ziel:

- Verstehen, welche Repository-/ProjectState-Klassen bereits existieren.
- Keine Doppelstruktur bauen.
- Bestehende Modelle wiederverwenden.

Zu pruefen:

- ProjectState-Ordner
- `IProjectStateRepository`
- JSON Loader/Snapshot Builder
- vorhandene ProjectItem-/Relation-/Provenance-Modelle
- L4-Inputmodelle
- RequirementsReadiness-Modelle
- IssuePlanning-Modelle

DoD:

- [x] Liste vorhandener Klassen und Verantwortlichkeiten dokumentiert.
- [x] Entscheidung, welche Klasse erweitert statt ersetzt wird.
- [x] Keine neue parallele Repository-Welt.

Status 2026-07-16:

- Vorhandene Schicht:
  - `projectstate/IProjectStateRepository.cs`
  - `projectstate/JsonProjectStateRepository.cs`
  - `projectstate/ProjectStateModels.cs`
  - `projectstate/ProjectStateBuildRunner.cs`
  - `projectstate/ProjectStateBuilder.cs`
- Entscheidung:
  - Diese Schicht wird erweitert, nicht ersetzt.
  - View-Boundary kommt additiv in denselben fachlichen Bereich.
  - L4-/IssuePlanning-Modelle werden wiederverwendet; keine parallelen DTOs fuer dieselben Artefakte.

## Meilenstein 2 — ProjectScope einfuehren

Ziel:

- Einen kleinen, stabilen Scope-Vertrag bauen, der spaeter aus RunConfig oder Graph Config kommen kann.

Minimalmodell:

```text
ProjectScope
  projectId
  analysisType
  scopeType
  baselineId?
  itemIds
  includeMappings
  includeLatestQuality
```

Vorgaben:

- Kein DB-spezifisches Feld.
- Keine GitHub-spezifische Pflicht.
- `analysisType` und `scopeType` als Strings oder klar validierte Wertemengen.
- Unbekannte Modi sollen frueh und klar fehlschlagen.

DoD:

- [x] `ProjectScope` Modell existiert.
- [x] Resolver kann aus CLI-Argumenten heutigen Pfadmodus in einen Scope/Source-Kontext uebersetzen.
- [x] Bestehende CLI-Pfade bleiben fuer Reproduzierbarkeit weiter nutzbar.

Status 2026-07-16:

- Neu:
  - `projectstate/ProjectStateViews.cs`
- `ProjectScope.FromSourcePath(...)` erlaubt den Uebergang von heutigen CLI-Pfaden zu einem fachlichen Scope-Kontext.
- `sourcePath` ist bewusst ein Uebergangsfeld, damit bestehende reproduzierbare Runs weiter funktionieren.

## Meilenstein 3 — View-Interfaces definieren

Ziel:

- Knoten sollen nicht mehr wissen, ob ihr Input aus Dateien, JSON-Snapshot oder DB kommt.

Erste Port-Methoden:

```text
GetCanonicalRequirementsView(ProjectScope scope)
GetReadinessView(ProjectScope scope)
GetIssuePlanningView(ProjectScope scope)
GetAcceptedIssuePlanView(ProjectScope scope)
GetProvenance(itemId, ProjectScope scope)
```

Wichtig:

- Methoden sollen fachlich benannt sein.
- Keine Methode wie `ReadFile` oder `LoadRunDirectory`.
- Fuer den Uebergang darf ein JSON-Adapter intern weiterhin Dateien lesen.

DoD:

- [x] Interface/Service definiert.
- [x] Bestehende Models werden wiederverwendet.
- [x] Kein Agent-Prompt muss geaendert werden.

Status 2026-07-16:

- Neu in `ProjectStateViews.cs`:
  - `CanonicalRequirementsView`
  - `ReadinessView`
  - `IssuePlanningView`
  - `AcceptedIssuePlanView`
  - `IProjectStateViewRepository`
- View-Port:
  - `GetCanonicalRequirementsViewAsync`
  - `GetReadinessViewAsync`
  - `GetIssuePlanningViewAsync`
  - `GetAcceptedIssuePlanViewAsync`

## Meilenstein 4 — JSON View Adapter bauen

Ziel:

- Heute weiter aus vorhandenen Artefakten laden.
- Pfadauflösung aber in einem Adapter kapseln.

Erste Adapter-Faehigkeiten:

```text
from l4 applied dir:
  canonical-requirements-baseline.json
  provenance-map.json
  quality-report.json
  requirements-readiness.json
  issue-planning-input.json

from l4-issuplanning run:
  issue-plan.json
  issue-plan-gate-report.json
  human-decisions.json
  applied/accepted-issue-plan.json
```

DoD:

- [x] Adapter kann die aktuellen Artefakte laden.
- [x] Runner koennen ihn nutzen, ohne selbst JSON-Dateien direkt zu kennen.
- [x] Bestehende Outputs bleiben schema-kompatibel.

Status 2026-07-16:

- Neu:
  - `projectstate/JsonProjectStateViewRepository.cs`
- Der Adapter kapselt:
  - L4 applied dirs aus `runs/l4/.../consolidation/applied`
  - L4 Completion applied dirs aus `runs/l4-completion/.../completion/applied`
  - `issue-planning-input.json`
  - akzeptierte IssuePlans aus `runs/l4-issuplanning/.../plan/applied`
- Wichtig:
  - Der Adapter liest weiterhin JSON.
  - Die Knoten muessen aber nicht mehr selbst wissen, welche Run-Layout-Varianten existieren.

## Meilenstein 5 — RequirementsReadiness auf View Boundary stellen

Ziel:

- Readiness bleibt deterministisch, konsumiert aber einen View statt direkt einen Pfad.

Vorher:

```text
requirements-readiness runs/l4/.../consolidation/applied
```

Nachher intern:

```text
scope -> CanonicalRequirementsView + QualityView + ProvenanceView -> ReadinessView
```

CLI darf weiter Pfade akzeptieren. Der interne Weg soll aber ueber den Adapter laufen.

DoD:

- [x] Verhalten bleibt gleich.
- [x] Bestehende Runs koennen reproduziert werden.
- [x] Der spaetere DB-Adapter muss nur denselben View liefern.

Status 2026-07-16:

- `RequirementsReadinessRunner` nutzt intern `JsonProjectStateViewRepository.GetCanonicalRequirementsViewAsync`.
- CLI bleibt unveraendert:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- requirements-readiness runs/l4-completion/20260716_104851_ded72d/completion/applied --out /tmp/agentic-sdlc-readiness-view-test
```

Validierung:

```text
[requirements-readiness] requirements=69 ready=31
[requirements-readiness] needsDecision=28 needsBreakdown=8 deferredOrOptional=2 blockedByTraceability=0
[requirements-readiness] issuePlanningInput=31
```

## Meilenstein 6 — IssuePlanning auf View Boundary stellen

Ziel:

- `L4IssuePlanningAgent` bleibt fachlich gleich.
- Sein Input kommt aus `GetIssuePlanningView(scope)`.

Vorher:

```text
l4-issuplanning agent runs/l4-completion/.../completion/applied
```

Nachher intern:

```text
scope -> IssuePlanningView -> L4IssuePlanningAgent[Tools]
```

Wichtig:

- Toolnamen bleiben stabil.
- Prompt bleibt stabil.
- `IssuePlanningInput` kann als konkrete View-DTO erhalten bleiben.
- Der Agent sieht weiter nur freigegebene planungsrelevante Items.

DoD:

- [x] Aktueller Run `20260716_132545_823a03` bleibt reproduzierbar.
- [x] Gate-Ergebnis bleibt gleich oder fachlich erklaerbar.
- [x] Keine neue Dateisystemlogik im Agenten.

Status 2026-07-16:

- `IssuePlanningRunner` nutzt intern `JsonProjectStateViewRepository.GetIssuePlanningViewAsync`.
- CLI bleibt unveraendert.
- Validierung:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- l4-issuplanning seed runs/l4-completion/20260716_104851_ded72d/completion/applied --out /tmp/agentic-sdlc-issueplan-seed-view-test.json
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- l4-issuplanning check runs/l4-completion/20260716_104851_ded72d/completion/applied runs/l4-issuplanning/20260716_132545_823a03/plan/issue-plan.json --out /tmp/agentic-sdlc-issueplan-view-check.json
```

Ergebnis:

```text
seed: items=31 gate=pass errors=0 warnings=0
check: gate=pass errors=0 warnings=1
```

## Meilenstein 7 — AcceptedIssuePlanView fuer GitHub vorbereiten

Ziel:

- Der naechste GitHub-Reconciliation-Knoten bekommt nicht direkt einen Run-Pfad, sondern einen akzeptierten Plan-View.

View:

```text
AcceptedIssuePlanView
  acceptedIssuePlan
  gateReport
  sourceIssuePlanningInput
  sourceBaselineId
  provenance
  mappings? spaeter
```

DoD:

- [x] `accepted-issue-plan.json` kann ueber View geladen werden.
- [x] GitHub-Reconciliation kann spaeter darauf aufbauen.
- [x] Mappings sind vorbereitet, aber noch nicht zwingend implementiert.

Status 2026-07-16:

- `AcceptedIssuePlanView` und `GetAcceptedIssuePlanViewAsync` sind definiert.
- Der aktuelle GitHub-Reconciliation-Knoten kann als naechsten Input diesen View verwenden.
- Noch nicht gebaut:
  - GitHub-Mapping-View
  - GitHub read-only state view
  - Reconciliation-Agent

## Meilenstein 8 — RunConfig/Modus-Anschluss vorbereiten

Ziel:

- Nicht alle Modi sofort bauen, aber die Stelle klaeren, an der Modi spaeter konfiguriert werden.

Beispiel:

```json
{
  "phase2Evidence": {
    "projectState": {
      "backend": "json",
      "defaultProjectId": "pflege-app"
    },
    "views": {
      "issuePlanning": {
        "analysisType": "initial-analysis",
        "scopeType": "current_baseline"
      }
    }
  }
}
```

DoD:

- Dokumentierte Config-Keys als Vorschlag.
- Noch keine breite Config-Migration erzwingen.
- CLI bleibt arbeitsfaehig.

## Meilenstein 9 — GitHub Reconciliation plan-only auf View Boundary

Ziel:

- Den naechsten Anschlussknoten nicht mehr auf rohe L4-/IssuePlanning-Dateien bauen.
- Input ist der akzeptierte IssuePlan als View.
- GitHub bleibt vorerst read-only/plan-only; es gibt keine Writes.

Kette:

```text
AcceptedIssuePlanView
  + existing GitHub issue snapshots
  + existing GitHub mappings
  -> GithubReconciliationInput
  -> GithubActionPlan
  -> GithubActionPlanGate
```

Warum so:

- `AcceptedIssuePlanView` ist die kontrollierte Grenze aus dem Requirements-/IssuePlanning-Prozess.
- GitHub ist operative Ebene und darf nicht selbst Requirements-Wahrheit rekonstruieren.
- Der spaetere GitHub-MCP-Server oder eine DB fuellen nur `existingIssues` und `existingMappings`.
- Der Agent/Connector arbeitet dann gegen denselben fachlichen Vertrag.

Status 2026-07-16:

- Erster lokaler plan-only Durchstich umgesetzt:
  - `l4/githubreconciliation/GithubReconciliationModels.cs`
  - `l4/githubreconciliation/GithubActionPlanFactory.cs`
  - `l4/githubreconciliation/GithubActionPlanGate.cs`
  - `l4/githubreconciliation/GithubReconciliationRunner.cs`
- Neue CLI:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation seed <accepted-issue-plan-dir|runId> [--out <dir>] [--repo owner/name] [--issues <issues.json>] [--mappings <mappings.json>]
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation check <accepted-issue-plan-dir|runId> <github-action-plan.json> [--out <report.json>] [--repo owner/name] [--issues <issues.json>] [--mappings <mappings.json>]
```

Validierung:

```bash
dotnet build AgenticSdlc.Host/AgenticSdlc.Host.csproj
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation seed 20260716_132545_823a03 --out /tmp/agentic-sdlc-github-reconciliation-seed --repo local/pflege-app
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation check 20260716_132545_823a03 /tmp/agentic-sdlc-github-reconciliation-seed/github-action-plan.json --out /tmp/agentic-sdlc-github-reconciliation-check.json --repo local/pflege-app
```

Ergebnis:

```text
Build: 0 Fehler, 0 Warnungen
seed: actions=19 gate=pass errors=0 warnings=0
check: gate=pass errors=0 warnings=0
operations:
  CREATE=9
  NEEDS_REVIEW=4
  NO_CHANGE=6
existingIssues=0
existingMappings=0
```

Bewertung:

- Der Knoten schreibt nichts nach GitHub.
- Er erzeugt nur einen `GithubActionPlan`.
- Ohne Issues/Mappings spiegelt er den akzeptierten IssuePlan in GitHub-nahe Aktionen.
- Sobald lokale Mappings oder GitHub-read Snapshots vorhanden sind, kann das gleiche Gate `LINK`, `UPDATE`, `REOPEN` und
  `NO_CHANGE` gegen vorhandenen GitHub-Zustand pruefen.

Naechster Schritt:

- Agentischen Reconciliation-Knoten mit Tools bauen. `[done]`
  - `list_accepted_issue_plan_items`
  - `get_issue_plan_item`
  - `list_existing_mappings`
  - `search_existing_issues`
  - `check_github_action_plan`
  - `save_github_action_plan`
- Danach HumanReview/Apply fuer `GithubActionPlan`.
- Erst danach echten GitHub-MCP-read Adapter anbinden.

## Fortschreibung 2026-07-16 — Agentischer Reconciliation-Knoten

Umgesetzt:

- `GithubReconciliationTools`
- `GithubReconciliationWorkflow`
- Prompt `Prompts/phase2_evidence/GithubReconciliationAgent/GithubReconciliationAgent1.txt`
- `github-reconciliation agent`

MAF-Graph:

```text
GithubReconciliationInput
  -> GithubReconciliationAgent[Tools]
  -> GithubActionPlanGate[det]
  -> GithubReconciliationFinalize
```

Tools:

```text
list_accepted_issue_plan_items
get_issue_plan_item
list_existing_mappings
search_existing_issues
get_seed_github_action_plan
check_github_action_plan
save_github_action_plan
```

CLI:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation agent 20260716_132545_823a03 --dry-run --repo local/pflege-app
```

Validierung:

```text
Dry-run: Graph Build()-bar
Build: 0 Fehler, 0 Warnungen
Seed: actions=19 gate=pass errors=0 warnings=0
Check: gate=pass errors=0 warnings=0
```

Bewertung:

- Der Agent ist ein echter MAF-Knoten, aber weiterhin plan-only.
- GitHub-Zustand bleibt read-only Input (`existingIssues`, `existingMappings`).
- Der aktuelle lokale Durchstich nutzt leere GitHub-Umwelt; das ist absichtlich der Seed-Fall.
- Der spaetere MCP/GitHub-Adapter soll nur diese Umwelt fuellen, nicht den fachlichen Planungsprozess umgehen.

Naechster Schritt:

- Echten `github-reconciliation agent` Lauf fahren. `[done]`
- Danach `GithubActionPlan` HumanReview/Apply analog IssuePlanning bauen. `[done]`

## GithubReconciliationAgent echter Lauf

Command:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation agent 20260716_132545_823a03 --repo local/pflege-app
```

Ergebnis:

```text
[github-reconciliation] running workflow runId=20260716_144616_19be8e model=openai/gpt-5.4
[github-reconciliation] gate=pass pass=True errors=0 warnings=0
[github-reconciliation] saved=True toolCheckRounds=1 actions=19
[github-reconciliation] -> runs/github-reconciliation/20260716_144616_19be8e/plan
```

Gate:

```text
acceptedIssuePlanItems=19
actions=19
coveredIssuePlanItems=19
uncoveredIssuePlanItems=0
existingIssues=0
existingMappings=0
operations:
  CREATE=9
  NEEDS_REVIEW=4
  NO_CHANGE=6
```

Bewertung:

- Der Agentenknoten funktioniert end-to-end.
- Weil `existingIssues` und `existingMappings` leer sind, entspricht der Plan erwartbar dem Seed-Verhalten.
- Das ist fuer den ersten Boundary-Test korrekt: Ohne GitHub-Umwelt darf der Agent keine LINK/UPDATE/REOPEN-Operationen
  erfinden.
- Der naechste fachliche Mehrwert entsteht durch einen read-only GitHub/MCP- oder lokalen Snapshot-Adapter, der
  `existingIssues` und `existingMappings` fuellt.

## GithubActionPlan HumanReview/Apply

Implementiert:

- `github-reconciliation-review`
- `github-reconciliation-apply`
- generische HumanReview-UI ueber `GithubReconciliationReviewAdapter`
- deterministischer Apply nach `accepted-github-action-plan.json`
- erneuter `GithubActionPlanGate` nach Apply

Validierung:

```bash
dotnet build AgenticSdlc.Host/AgenticSdlc.Host.csproj
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation-review 20260716_144616_19be8e --file
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation-review 20260716_144616_19be8e --file --scope all
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation-apply 20260716_144616_19be8e
```

Ergebnis:

```text
Build: 0 Fehler, 0 Warnungen
Default Review: scope=needs-human 4 GitHubActionPlanItems
All Review: scope=all 19 GitHubActionPlanItems
Apply ohne human-decisions.json: blockiert erwartbar
```

Bewertung:

- Der GitHub-Schreibpfad ist weiterhin nicht aktiv.
- Der Agent erzeugt nur einen Vorschlag.
- HumanReview autorisiert oder editiert den Vorschlag.
- Apply materialisiert erst danach einen freigegebenen `accepted-github-action-plan`.
- Dieser freigegebene Plan ist der spaetere Input fuer einen separaten GitHub-Write-Knoten.
- Default-Review zeigt bewusst nur reviewpflichtige Actions; `--scope all` bleibt als Kontrollsicht verfuegbar.

## GitHub Issue Snapshot Knoten

Ziel:

- `github-reconciliation` soll echte GitHub-Umwelt konsumieren koennen, ohne selbst GitHub zu lesen oder zu schreiben.
- Vorhandene Issues werden read-only in das interne `GithubIssueSnapshot`-Format normalisiert.
- Dieser Schritt ist deterministisch und austauschbar: Datei, GitHub REST API, optional `gh`; spaeter kann MCP/DB denselben Output liefern.

Implementiert:

- `github-snapshot issues`
- Datei-Provider fuer reproduzierbare Tests
- GitHub REST API Provider als Default
- optionaler `gh` Provider
- Ausgabe:

```text
github-issues-snapshot.json
github-issues-snapshot-summary.json
```

Validierung:

```bash
dotnet build AgenticSdlc.Host/AgenticSdlc.Host.csproj
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-snapshot issues --file /tmp/agentic-sdlc-gh-issues.json --repo local/testrepo --out /tmp/agentic-sdlc-github-snapshot-test-2
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation seed 20260716_132545_823a03 --repo local/testrepo --issues /tmp/agentic-sdlc-github-snapshot-test/github-issues-snapshot.json --out /tmp/agentic-sdlc-reconciliation-with-snapshot
```

Ergebnis:

```text
Build: 0 Fehler, 0 Warnungen
Snapshot: provider=file issues=1 open=1 closed=0
Reconciliation seed mit Snapshot: actions=19 gate=pass errors=0 warnings=0
```

Hinweis:

- `AgenticSdlc.McpServer/docs/issues.json` ist kein GitHub-Snapshot, sondern eine alte geplante Issue-Liste ohne Issue-Nummern.
- Der Snapshot-Knoten blockiert solche Inputs korrekt, weil LINK/UPDATE/REOPEN eine stabile `issueNumber` brauchen.

Testrepo-Befehl:

```bash
export GITHUB_TOKEN=<token-fuer-private-repos-falls-noetig>
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-snapshot issues owner/name --out runs/manual/github-snapshot-test
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- github-reconciliation agent 20260716_132545_823a03 --repo owner/name --issues runs/manual/github-snapshot-test/github-issues-snapshot.json
```

Bewertung:

- Damit ist die DB-/MCP-Zukunft vorbereitet: Reconciliation erwartet nur noch einen fachlichen Snapshot, nicht eine konkrete GitHub-Bibliothek.
- Der GitHub-Read ist ein eigener Knoten und kann spaeter durch einen MCP-Adapter oder Repository-View ersetzt werden.
- GitHub-Write bleibt weiter getrennt und darf erst den akzeptierten `accepted-github-action-plan` konsumieren.

## Alternative: direkter GitHub-MCP-Agent als Vergleichspfad

Architekturentscheidung:

- Der aktuelle Hauptpfad bleibt snapshot-/ProjectState-basiert.
- Ein spaeterer Vergleichspfad mit direktem GitHub-MCP-Read bleibt ausdruecklich vorgesehen.
- Ziel ist nicht, Agency zu verhindern, sondern die Agenten-Agency gegen reproduzierbare Umweltzustaende vergleichbar zu machen.

Vergleichsarme:

```text
A) Snapshot/State-basiert
   github-snapshot -> GithubIssueSnapshot -> GithubReconciliationAgent[SnapshotTools]

B) Live-MCP-basiert
   GithubReconciliationAgent[GitHubMcpReadTools] -> live GitHub read -> Plan
```

Zu vergleichen:

- findet der Live-Agent mehr oder bessere Matches?
- erzeugt er weniger Duplikate?
- ist sein Ergebnis stabil genug fuer HumanReview?
- wie gut ist die Reproduzierbarkeit gegenueber dem Snapshot-Pfad?
- wie viele Toolcalls/Fehlgriffe entstehen?
- wie gut laesst sich ein Lauf spaeter auditieren?

Regel:

- Auch der MCP-Arm bleibt zunaechst read-only und plan-only.
- GitHub-Writes bleiben ein separater deterministischer Knoten nach HumanReview/Apply.
- Der MCP-Arm darf den produktiven Snapshot-Pfad erst ersetzen, wenn er stabiler oder klar besser ist.

## GitHub Write Dry-Run Knoten

Ziel:

- Nach `github-reconciliation-apply` soll nicht sofort nach GitHub geschrieben werden.
- Zuerst muss ein deterministischer Dry-Run zeigen, welche konkreten Operationen aus dem akzeptierten Plan folgen wuerden.

Implementiert:

- `github-write dry-run`
- Input: `accepted-github-action-plan.json`
- Output:

```text
github-write-dry-run.json
github-write-dry-run-summary.json
```

Aktueller Test:

```text
source=20260716_160450_cdeabe
actions=19
create=7
update=0
link=1
reopen=1
noChange=6
needsReview=4
blocked=4
readyForExecute=false
```

Interpretation:

- `CREATE` wuerde spaeter `POST /repos/{owner}/{repo}/issues` ausloesen.
- `REOPEN` wuerde spaeter `PATCH /repos/{owner}/{repo}/issues/{number} state=open` ausloesen.
- `LINK` schreibt nicht nach GitHub, sondern persistiert spaeter ein lokales Mapping.
- `NO_CHANGE` ist ein No-op.
- `NEEDS_REVIEW` blockiert echten Write.

Bewertung:

- Der Knoten passt zur DB-Zukunft, weil LINK/Mappings spaeter in den ProjectState gehoeren.
- GitHub bleibt operative Ebene, nicht fachliche Wahrheit.
- Der spaetere echte Write-Executor kann denselben Dry-Run-Vertrag konsumieren.

## Operationalization Audit

Ziel:

- Vor echtem GitHub-Write eine deterministische Kontrollsicht ueber die komplette Kette erzeugen.
- Klaeren, ob Requirements verloren gingen oder bewusst nicht operativ geplant wurden.

Implementiert:

- `operationalization-audit`
- Input: GitHub-Reconciliation-Run oder `accepted-github-action-plan.json`
- Output:

```text
operationalization-audit.json
operationalization-audit.md
```

Aktueller Lauf:

```text
canonical=69
readiness=69
issueInput=31
issuePlan=19
githubActions=19
dryRunOperations=19
coveredIssueInput=31/31
blockedRequirements=8
noChangeOnly=6
readyForGithubWrite=false
findings=17
```

Interpretation:

- Alle 31 Requirements im IssuePlanningInput sind durch IssuePlan/GitHubAction abgedeckt.
- 38 Canonical Requirements sind nicht im IssuePlanningInput, weil Readiness sie nicht direkt fuer operative Planung freigegeben hat.
- 8 Requirements sind durch `NEEDS_REVIEW` noch nicht write-faehig.
- 6 Requirements sind bewusst `NO_CHANGE`.

DB-/ProjectState-Bezug:

- Der Audit ist heute dateibasiert, arbeitet aber bereits entlang fachlicher Artefakte.
- Spaeter wird das eine ProjectState-Abfrage:

```text
list canonical requirements
join readiness
join issue plans
join github actions
join dry-run/write state
```

- Damit ist der Schritt eine direkte Vorbereitung fuer DB-gestuetzte Traceability.

## Open Requirements Review

Problem:

- Der Operationalization Audit zeigt 38 Canonical Requirements, die nicht in Delivery-IssuePlanning gelandet sind.
- Diese Requirements sind nicht verloren, aber sie brauchen einen eigenen Pfad.

Loesung:

```text
Operationalization Audit
  -> open-requirements-review
  -> open-requirements-apply
  -> clarification-planning-input
  -> clarification-agent plan
```

Review-Entscheidungen:

```text
create_clarification_issue
break_down_required
defer
out_of_scope
promote_to_delivery_planning
merge_with_existing
keep_open_decision
```

Aktueller Durchstich:

```text
open=38
needs-breakdown=8
deferred=2
apply:
  create_clarification_issue=28
  break_down_required=8
  defer=2
  clarificationItems=36
```

Output:

```text
accepted-open-requirement-decisions.json
clarification-planning-input.json
open-requirements-apply-report.json
```

Architektur:

- Delivery-IssuePlanning und ClarificationPlanning sind getrennte Modi/Knoten.
- Beide koennen spaeter aus dem ProjectStateRepository lesen.
- ClarificationIssues sind echte operative Arbeit, aber Ziel ist Entscheidung/Zerlegung, nicht direkte Implementierung.
- Spaeter kann ein Agent Loesungsvorschlaege fuer diese offenen Punkte erzeugen oder Antworten aus GitHub-Kommentaren extrahieren.

Umgesetzter Clarification-Knoten:

```text
clarification-agent
  mode plan:
    ClarificationPlanningInput
      -> ClarificationPlanningAgent[Tools]
      -> ClarificationPlanGate[det]
      -> Finalize

  mode resolve:
    TBD contract
    spaeter: ClarificationResolutionInput
      -> ClarificationResolutionAgent[ProjectState/GitHub-read Tools]
      -> ResolutionGate
      -> HumanReview
      -> Apply
```

Warum zwei Modi:

- `plan` ist der aktuelle produktive Modus: offene Requirements werden zu klaerbarer operativer Arbeit.
- `resolve` ist die spaetere agentische Alternative: ein Agent kann Loesungsvorschlaege erzeugen, ohne direkt Projektwahrheit zu schreiben.
- Beide Modi bleiben hinter derselben fachlichen Knoten-Grenze und koennen spaeter DB-backed Views konsumieren.

DB-Bezug:

- Der Knoten konsumiert `ClarificationPlanningView` statt konkrete Run-Dateien direkt.
- Die erste Implementierung ist JSON-backed.
- Eine spaetere DB-Implementierung muss nur denselben View liefern.
- Damit koennen auch Sprint-Inputs spaeter offene Punkte aus dem ProjectState in denselben Clarification-Knoten geben.

## Was bewusst NICHT Teil dieses Schritts ist

Nicht jetzt:

- echte Datenbank einbauen
- DB-Typ festlegen
- GitHub schreiben
- GitHub-MCP hart anbinden
- alle Artefakttypen generisch umbauen
- Requirements durch ein unkonkretes Universalmodell ersetzen
- L1/L2/L3 voll refactoren
- User Stories als eigenen Knoten bauen

Stattdessen:

- Ports/Views so schneiden, dass diese Dinge spaeter sauber moeglich sind.

## Erwarteter Nutzen

Nach diesem Schritt gilt:

- Jeder neue Knoten bekommt fachliche Views.
- JSON bleibt reproduzierbar.
- DB kann spaeter hinter dem gleichen Port kommen.
- GitHub-Reconciliation kann plan-only/read-only gebaut werden, ohne Dateipfad-Kopplung.
- Sprint-Deltas koennen spaeter als `ProjectScope`/`SprintDeltaView` integriert werden.
- Weitere Artefakttypen koennen ueber neue Views/Itemtypen kommen, ohne bestehende Agenten wild umzubauen.

## Erfolgskriterium fuer diesen Plan

Der Schritt ist erfolgreich, wenn der aktuelle End-to-End-Stand weiterhin funktioniert und mindestens diese Kette intern
ueber Views lauffaehig ist:

```text
L4 applied artefacts
  -> JsonProjectState/View Adapter
  -> ReadinessView
  -> IssuePlanningView
  -> L4IssuePlanningAgent
  -> IssuePlanGate
  -> HumanReview
  -> IssuePlanningApply
  -> AcceptedIssuePlanView
  -> OperationalizationAudit
  -> OpenRequirementsReview/Apply
  -> ClarificationPlanningView
  -> ClarificationPlanningAgent
  -> ClarificationPlanGate
  -> HumanReview
  -> ClarificationPlanningApply
  -> AcceptedClarificationPlanView
  -> GithubReconciliation
```

Status 2026-07-17:

- `ClarificationPlanningView` ist vorhanden.
- `AcceptedClarificationPlanView` ist vorhanden.
- `clarification-agent plan` erzeugt einen MAF-nahen Agentenknoten mit Tools/Gate/Finalize.
- `clarification-agent resolve` ist als Contract/TBD vorbereitet.
- `clarification-agent-review` und `clarification-agent-apply` verwenden dieselbe HumanReview-/Apply-Logikfamilie wie L4-IssuePlanning.
- `github-reconciliation` kann `accepted-clarification-plan.json` ueber eine Projektion konsumieren.

Durchstich:

```text
Clarification Agent Run 20260717_084008_691527:
  input=36 offene Items
  output=18 ClarificationPlanItems
  gate=pass

Clarification Apply:
  accepted=18/18

GitHub Reconciliation Seed ueber accepted ClarificationPlan:
  actions=18
  gate=pass

GitHub Write Dry-Run:
  create=16
  noChange=2
  blocked=0
  readyForExecute=True
```

DB-Bewertung:

- Die neue AcceptedClarificationPlan-Grenze ist bewusst ein View.
- Bei DB-Einfuehrung muss nicht der Agent umgebaut werden, sondern nur der View-Adapter.
- Fuer Sprint-Deltas kann spaeter ein Scope nur offene/betroffene ClarificationItems liefern.
- Delivery-Issues und Clarification-Issues bleiben getrennte fachliche Projektionen, koennen aber denselben GitHub-Reconciliation-Knoten verwenden.

Ohne:

- DB
- GitHub Write
- freie Dateizugriffe im Agenten
- Verlust von Traceability

## Status 2026-07-17: Issue Operationalization Contract

Problem:

- Der technische GitHub-Durchstich erzeugte Issues, aber die Issues waren fuer ein echtes Entwicklerteam noch zu duenn.
- Die Kette hatte bereits mehr Informationen als im Issue sichtbar wurden.
- Offene Punkte waren teilweise vorhanden, aber nicht sauber mit Delivery-Issues und Requirement-Abdeckung verbunden.

Entscheidung:

- Keine neue grosse Agentenschicht einfuehren.
- Stattdessen die vorhandene Grenze `IssuePlanning -> GithubReconciliation -> GithubWrite` staerken.
- Der `L4IssuePlanningAgent` bleibt der operative Knoten fuer die Uebersetzung von Requirements in Arbeitspakete.
- Der GitHub Writer bleibt deterministische Projektion.

Neuer minimaler Issue-Vertrag:

```text
IssuePlanItem
  knownContext              // bereits geklaert und rueckfuehrbar
  implementationHints       // naheliegende Umsetzungspunkte, nicht als Quelle behauptet
  openQuestions             // offen fuer Entwickler, Product, Architektur oder spaeteren Agenten
  relatedClarificationIds   // Verbindung zu Klaerungsarbeit
  readiness                 // ready_for_dev | ready_with_open_questions | needs_refinement | blocked_by_clarification
```

GitHub-Rendering:

```text
Beschreibung
Bereits geklaert
Erwartete Umsetzung
Offen / zu klaeren
Abhaengigkeiten
Verwandte Klaerungen
Readiness
Acceptance Criteria
Traceability
Rationale
```

Backlog-Coverage-Gate:

`operationalization-audit` bewertet jetzt nicht nur Delivery und Dry-Run, sondern optional auch den ClarificationPlan.

Requirement-Status:

```text
delivery
delivery_with_clarification
clarification
no_change
deferred
planned_without_github_action
issue_planning_input_uncovered
missing_operational_coverage
```

Damit gilt fachlich:

- Ein Requirement muss nicht zwingend ein eigenes GitHub-Issue haben.
- Ein GitHub-Issue kann mehrere Requirements abdecken.
- Ein Requirement kann mehrere Issues oder Klaerungsissues benoetigen.
- Wichtig ist: jedes Requirement muss im ProjectState eine operative Einordnung haben.

Aktueller Nachweis:

```text
canonical=69
issueInput=31
coveredIssueInput=31/31
deliveryCoverage=17
clarificationCoverage=36
missingCoverage=0
```

Nutzen fuer DB/Sprint-Zukunft:

- Die Logik haengt an fachlichen Views und Artefakten, nicht an GitHub als Wahrheit.
- Eine DB muss spaeter denselben Backlog-/Operationalization-View liefern.
- Sprint-Deltas koennen denselben Audit auf einen eingeschraenkten Scope anwenden:
  - neue Requirements
  - geaenderte Requirements
  - betroffene offene Entscheidungen
  - bestehende GitHub-Mappings
- GitHub-Issues werden dadurch nicht zur Wahrheit, sondern bleiben verlinkte operative Arbeitseinheiten.

## Fortschreibung 2026-07-17: Issue Quality Gate vor GitHub Write

Problem:

- Ein spaeterer UPDATE-/Repair-Prozess reicht nicht als Standardloesung.
- Der Normalfall muss verhindern, dass duenne oder unvollstaendige Issues ueberhaupt initial nach GitHub geschrieben werden.
- Alte Test-Issues koennen migriert, geschlossen oder aktualisiert werden; das ist aber ein Testrepo-/Historienproblem und nicht der produktive Hauptpfad.

Entscheidung:

- `GithubIssueQualityRules` ist die neue deterministische Qualitaetsschicht fuer GitHub-Schreibaktionen.
- `GithubActionPlanGate` nutzt diese Regeln bereits vor Apply/Write als Gate.
- `GithubWriteDryRunFactory` nutzt dieselben Regeln als harte Ausfuehrungssperre.
- Dadurch kann HumanReview weiterhin editieren, aber ein fachlich zu duennes `CREATE`/`UPDATE` wird nicht automatisch geschrieben.

Geprueft wird fuer `CREATE` und `UPDATE`:

- belastbarer Titel
- ausreichend konkreter Body
- mindestens zwei Acceptance Criteria
- SourceRequirementIds fuer Traceability
- `knownContext`, damit bereits geklaerte Informationen aus L1-L4 im Issue sichtbar bleiben
- `implementationHints` fuer normale Dev-Issues
- `readiness`
- bei `DISK`-/Klaerungsissues explizite offene Fragen oder Entscheidungsfragen

Wichtig:

- `LINK`, `NO_CHANGE` und `REOPEN` erzeugen keinen neuen Issue-Body und werden deshalb nicht wie Schreibinhalte bewertet.
- `LINK` bleibt Mapping/Reconciliation und verbessert bestehende duenne Issues nicht automatisch.
- Wenn bestehende GitHub-Issues schon duenn sind, braucht es einen separaten kontrollierten `UPDATE`-Plan.
- Fuer neue Laeufe ist der Hauptpfad aber: IssuePlanning erzeugt reichhaltige IssuePlans, Reconciliation erzeugt Actions, QualityGate blockiert duenne Writes, erst danach schreibt der deterministische GitHub Writer.

Validierung:

```text
dotnet build AgenticSdlc.Host/AgenticSdlc.Host.csproj
  -> 0 warnings, 0 errors

github-write dry-run runs/github-reconciliation/20260717_111629_906d93/plan/applied
  -> actions=16 create=4 update=0 link=9 reopen=0 noChange=3 needsReview=0 blocked=0
  -> readyForExecute=True
```

Testrepo-Reset:

- GitHub Issues werden nicht geloescht, sondern kontrolliert geschlossen.
- `github-snapshot close-open` ist ein bewusstes Testwerkzeug fuer ein sauberes Open-Issue-Board.
- Es braucht `--confirm-close` und liest den Token aus `GITHUB_TEST_TOKEN`.
- Der Reset ist nicht Teil des produktiven Requirements-/GitHub-Flows.

```text
github-snapshot close-open armiino/Agentic-GitHub --confirm-close
```

Clean-Test 2026-07-17:

```text
close-open:
  openBefore=26
  closed=26
  failed=0

clean reconciliation seed:
  actions=16
  gate=pass

clean github-write dry-run:
  create=13
  noChange=3
  blocked=0
  readyForExecute=True

github-write execute:
  created=13
  skipped=3
  failed=0
  newIssues=#27-#39

snapshot danach:
  open=13
  closed=26
```

Damit ist der aktuelle Teststand ein sauberes Open-Issue-Board mit neuen, reichhaltig gerenderten Issues.
Die geschlossenen Alt-Issues bleiben nur als Testhistorie im Repo.
