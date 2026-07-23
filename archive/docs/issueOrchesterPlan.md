# Impact- und Issue-Orchestrierung für das Agentic-SDLC-System

**Dokumenttyp:** Architektur- und Implementierungsplan
**Version:** 0.2
**Status:** Zielarchitektur mit schrittweiser Umsetzung
**Primärer Scope:** Evidence Ledger, quellsichere Artefakte, kontrollierte Ableitungen und GitHub Issues
**Später erweiterbar um:** User Stories, Acceptance Criteria, Wiki, ADRs, Pull Requests und weitere SDLC-Artefakte
**Nicht im ersten Release:** Codeänderungen, Pull-Request-Erstellung, Deployment und vollständige bidirektionale GitHub-Synchronisation

---

## 1. Ziel des Systems

Das System soll Gespräche aus dem Softwareentwicklungsprozess automatisiert verarbeiten und daraus:

1. eine quellsichere Darstellung des tatsächlich Besprochenen,
2. kontrollierte fachliche Ableitungen,
3. überprüfte SDLC-Artefakte,
4. und daraus versionierbare GitHub Issues

erzeugen oder aktualisieren.

Dabei wird bewusst zwischen zwei Wissensebenen unterschieden:

```text
Evidence Baseline
= Was wurde tatsächlich gesagt, beschlossen oder offengelassen?

Derived Knowledge
= Was folgt fachlich daraus, obwohl es nicht ausdrücklich gesagt wurde?
```

Diese Trennung ist die zentrale Architekturentscheidung des Systems.

Der erste Artifact Agent erzeugt daher nicht zwingend das endgültige Arbeitsergebnis. Er erzeugt eine geprüfte, quellsichere Baseline. Nachgelagerte Derivation Agents dürfen darauf aufbauend neue Inhalte erzeugen, sofern sie ihre Ausgangspunkte, Ableitungslogik und Annahmen transparent machen.

---

## 2. Leitprinzipien

### 2.1 Evidence first

Jede direkt aus einem Gespräch übernommene fachliche Aussage muss auf freigegebene Claims zurückgeführt werden können.

```text
Artifact Item
→ Claim ID
→ Evidence Span
→ Transcript
```

### 2.2 Extraktion und Inferenz bleiben getrennt

Direkt belegte Aussagen und fachlich abgeleitete Aussagen dürfen nicht vermischt werden.

```text
extracted
→ im Ledger beziehungsweise Evidence Package gedeckt

derived
→ aus geprüften Artefakten nachvollziehbar abgeleitet

assumed
→ benötigt eine zusätzliche explizite Annahme

proposed
→ fachlicher oder technischer Vorschlag
```

### 2.3 Jeder Verarbeitungsschritt erzeugt eine neue Provenienzstufe

```text
Transcript
→ Claims
→ Evidence-Baseline-Artefakte
→ Derived Artifacts
→ GitHub Issues
```

Jede Stufe referenziert die vorherige Stufe über stabile IDs.

### 2.4 Agenten übernehmen semantische Aufgaben

LLM-Agenten werden dort eingesetzt, wo Interpretation, Klassifikation, Synthese, Ableitung oder Formulierung notwendig ist.

### 2.5 Executors übernehmen deterministische Kontrolle

IDs, Versionierung, Persistenz, Validierung, Routingregeln, GitHub-Schreiboperationen und Idempotenz werden durch normalen Code kontrolliert.

### 2.6 Workflows bilden den expliziten Kontrollfluss

Microsoft Agent Framework dient als Orchestrierungsschicht für typisierte Executors, Agent Executors, Edges, Bedingungen, Fan-out/Fan-in, Subworkflows und Human Review. MAF-Workflows verbinden Agenten und Custom Logic in einem expliziten Graphen; ein Subworkflow kann dabei als einzelner Executor in einen Parent-Workflow eingebunden werden.

### 2.7 Der Project State ist nicht die Agenten-Historie

Der dauerhafte Projektzustand liegt in einem strukturierten Project Store. MAF Shared State dient dagegen dem gemeinsamen Zustand innerhalb eines Workflow-Runs.

---

## 3. Aktueller Ist-Zustand

Der aktuelle Aufbau enthält bereits:

```text
Kickoff-Transkript
    ↓
Evidence-Ledger-Workflow
    ↓
Human Approval
    ↓
consumable.json
    ↓
Artifact Agents
    ├─ RequirementsAgent
    ├─ RisksAgent
    ├─ ArchitectureAgent
    └─ OpenQuestionsAgent
    ↓
Checker-Repair-Logik
```

Der Evidence Ledger:

* zerlegt das Transkript in Claims,
* vergibt Claim-IDs,
* speichert Proposition, Evidence und Notes,
* ordnet Claims den relevanten Artefaktdomänen zu,
* unterstützt Human Approval,
* erzeugt eine freigegebene `consumable.json`.

Die Artifact Agents:

* verwenden freigegebene Claims,
* führen Claim-IDs mit,
* sollen Modalität, Status und Scope nicht verstärken,
* erzeugen Requirements, Risiken, Architekturpunkte und offene Fragen.

Der Checker-Repair-Teil umfasst bereits:

* deterministische Contract Checks,
* Citation- und Coverage-Prüfungen,
* einen bounded Evidence-Support-Critic,
* Wiederholungs- beziehungsweise Vote-Logik,
* begrenzte Reparaturzyklen,
* Human-Review-Routing,
* einen getrennten, quellgenerischen Checker-Repair-Workflow.

Der aktuelle Stand ist daher keine Sackgasse, sondern die erste Hälfte der Zielarchitektur. Was bislang fehlt, ist vor allem:

1. die explizite Komposition als Fan-out,
2. die stabile strukturierte Artefaktidentität,
3. die produktive Inferenzschicht,
4. der Project Store,
5. und die kontrollierte GitHub-Integration.

---

## 4. Gesamtarchitektur

Das System wird in fünf Schichten gegliedert:

```text
┌────────────────────────────────────────────────────┐
│ 1. Evidence Layer                                  │
│ Transcript → Claims → Approval → Consumable        │
├────────────────────────────────────────────────────┤
│ 2. Evidence-Baseline Layer                         │
│ Claims → quellsichere Artefakte                    │
├────────────────────────────────────────────────────┤
│ 3. Derivation Layer                                │
│ geprüfte Artefakte → neue fachliche Ableitungen    │
├────────────────────────────────────────────────────┤
│ 4. Assurance Layer                                 │
│ Extraction-/Inference-Checks → Repair → Review     │
├────────────────────────────────────────────────────┤
│ 5. Integration Layer                               │
│ approved Artifact Changes → GitHub Issues          │
└────────────────────────────────────────────────────┘
```

Quer dazu existieren:

```text
Project Store
→ dauerhafter fachlicher Projektzustand

MAF Workflow State
→ temporärer Zustand eines konkreten Runs

Audit und Events
→ nachvollziehbare Ausführung und Reproduktion
```

---

## 5. Die zwei fachlichen Verarbeitungsebenen

## 5.1 Ebene A: quellsichere Baseline

Die erste Artifact-Generation-Stufe beantwortet:

> Was wurde tatsächlich gesagt, beschlossen, gefordert oder offengelassen?

Diese Stufe arbeitet closed-world gegen den freigegebenen Ledger.

Beispiele für Agenten:

```text
EvidenceBoundRequirementsAgent
EvidenceBoundRisksAgent
EvidenceBoundArchitectureAgent
EvidenceBoundOpenQuestionsAgent
```

Die Agenten dürfen:

* Claims normalisieren,
* redundante Claims zusammenführen,
* implizite Requirements sprachlich sichtbar machen,
* Informationen strukturieren,
* Claim-Facetten korrekt in Artefaktsprache übertragen.

Sie dürfen nicht:

* Best Practices ergänzen,
* neue Risiken erfinden,
* neue Architekturentscheidungen treffen,
* fehlende Anforderungen stillschweigend ergänzen,
* Vorschläge als besprochene Fakten darstellen.

Der Output ist eine **Evidence Baseline** und nicht automatisch das endgültige Projektartefakt.

---

## 5.2 Ebene B: kontrollierte Ableitungen

Die zweite Generationsebene beantwortet:

> Was folgt fachlich aus den geprüften Baseline-Artefakten?

Beispiele:

```text
DerivedRisksAgent
RequirementsGapAgent
UserStoryDerivationAgent
AcceptanceCriteriaAgent
ArchitectureProposalAgent
MitigationProposalAgent
```

Diese Agenten dürfen neue Inhalte erzeugen.

Sie müssen aber für jedes neue Item angeben:

* welche Baseline-Items als Ausgangspunkt dienten,
* wie die Ableitung zustande kam,
* welche Annahmen notwendig waren,
* ob es sich um eine Ableitung, Annahme oder Empfehlung handelt,
* ob Human Review erforderlich ist.

Beispiel:

```json
{
  "itemId": "DRISK-0007",
  "origin": "derived",
  "assertionKind": "derived_risk",
  "text": "Parallele Offline-Änderungen können zu Synchronisationskonflikten führen.",
  "sourceArtifactItemIds": [
    "REQ-0012-R01",
    "REQ-0012-R07"
  ],
  "derivationType": "requirement_interaction",
  "rationale": "Offline-Bearbeitung und spätere Synchronisation ermöglichen konkurrierende Änderungen.",
  "assumptions": [
    "Mehrere Geräte können denselben Datensatz unabhängig verändern."
  ],
  "reviewStatus": "pending"
}
```

Die Provenienz bleibt damit über mehrere Hops erhalten:

```text
DRISK-0007
→ REQ-0012-R01 und REQ-0012-R07
→ CLM-0042 und CLM-0051
→ Transcript Evidence
```

---

## 6. Baseline-Artefakte dürfen nicht überschrieben werden

Ein Derivation Agent darf die quellsichere Baseline nicht unbemerkt verändern.

Nicht empfohlen:

```text
requirements.md
→ freier Agent überschreibt dieselbe Datei
```

Empfohlen:

```text
Requirements Baseline
+ Derived Requirements/Gaps
+ Human-approved Enrichment
= zusammengesetzte Projektsicht
```

Die Datenhaltung unterscheidet mindestens:

```text
baselineItems
derivedItems
approvedDerivedItems
rejectedDerivedItems
```

Damit bleibt jederzeit sichtbar:

* was gesagt wurde,
* was daraus abgeleitet wurde,
* was ein Mensch bestätigt hat,
* was verworfen wurde.

---

## 7. MAF-native Rollenverteilung

## 7.1 Agent

Ein Agent wird eingesetzt, wenn semantisches Urteilen oder Generieren erforderlich ist.

Beispiele:

```text
ClaimExtractionAgent
EvidenceBoundRequirementsAgent
DerivedRisksAgent
ClaimReconciliationAgent
ImpactPlannerAgent
GitHubIssuePlannerAgent
```

Ein Agent:

* erhält einen typisierten Input,
* erzeugt einen strukturierten Output,
* vergibt keine stabilen IDs,
* verändert nicht direkt den Project Store,
* führt keine unkontrollierten externen Schreibzugriffe aus.

In einem MAF-Workflow wird ein Agent über einen Agent Executor in das typisierte Workflow-Modell eingebunden.

---

## 7.2 Custom Executor

Ein Custom Executor wird eingesetzt, wenn Code das Ergebnis eindeutig bestimmen kann.

Beispiele:

```text
LoadLedgerProjectionExecutor
LoadProjectContextExecutor
AssignStableIdsExecutor
ContractCheckerExecutor
ValidateArtifactRelationsExecutor
SaveArtifactVersionExecutor
LoadGitHubStateExecutor
ValidateGitHubPlanExecutor
GitHubIssueExecutor
SaveGitHubMappingExecutor
```

Grundregel:

```text
Semantische Entscheidung?
→ Agent

Eindeutige technische Regel?
→ Custom Executor

Hohe Unsicherheit oder Tragweite?
→ Human Review
```

---

## 7.3 Workflow

Ein Workflow bildet einen expliziten Prozess ab.

Beispiele:

```text
EvidenceLedgerWorkflow
EvidenceBaselineWorkflow
DerivationWorkflow
CheckerRepairWorkflow
GitHubIssueSyncWorkflow
IncrementalTranscriptWorkflow
```

Ein Workflow koordiniert Executors und Edges und kann bedingtes Routing, Fan-out und weitere Kontrollflüsse enthalten.

---

## 7.4 Subworkflow

Ein Subworkflow ist ein eigenständig testbarer Workflow, der im Parent-Workflow wie ein Executor verwendet wird.

Geeignete Subworkflows:

```text
ExtractionCheckerRepairWorkflow
InferenceCheckerWorkflow
EvidenceArtifactBranchWorkflow
DerivedArtifactBranchWorkflow
GitHubIssueSyncWorkflow
HumanApprovalWorkflow
```

Der Parent-Workflow kennt nur den Input- und Output-Contract des Subworkflows, nicht dessen interne Schleifen.

---

## 8. Wiederverwendung und Erweiterbarkeit

Es soll nicht für jeden Use Case ein komplett neuer Agent entstehen.

Wiederverwendung erfolgt über:

```text
fachliche Fähigkeit
+ typisierter Auftrag
+ relevante Context-Projektion
+ Policy
```

Entscheidungsregel:

```text
Nur Scope oder Input ändern sich
→ gleichen Agenten wiederverwenden

Create, Update oder Reconcile ändern sich
→ gleicher Agent mit Operation-Contract,
  sofern die fachliche Fähigkeit gleich bleibt

Erlaubte Inferenz ändert sich deutlich
→ eigener Agent oder eigene Agent Policy

Prüfsemantik ändert sich
→ gleiche Checker-Struktur mit anderer Checker Policy

Kontrollfluss ändert sich
→ neuen Workflow aus vorhandenen Komponenten komponieren

Externer Zielkanal ändert sich
→ neuen Integration-Adapter ergänzen
```

Beispiel:

```text
EvidenceBoundRequirementsAgent
→ CreateInitial
→ Update
→ Reconcile
```

Dagegen:

```text
EvidenceBoundRisksAgent
≠ DerivedRisksAgent
```

weil die erste Rolle extrahiert und die zweite neue Risiken inferiert.

---

## 9. Stabile Identitäten

Stabile IDs werden früh eingeführt, weil sie später für Project State, Inferenzketten und GitHub-Mappings benötigt werden.

Mindestens:

```text
Project ID
Source ID
Claim ID
Artifact ID
Artifact Item ID
Run ID
Operation ID
External Reference
```

Beispiel:

```text
Project:        PRJ-001
Transcript:     TR-0001
Claim:          CLM-0042
Artifact:       REQ-0012
Artifact Item:  REQ-0012-R03
Derived Item:   DRISK-0007
Run:            RUN-0107
GitHub Operation: GHOP-0182
```

### 9.1 Artefakt und Artefakt-Item unterscheiden

```text
REQ-0012
= Requirements-Artefakt oder Requirements-Scope

REQ-0012-R03
= einzelne Requirement innerhalb dieses Artefakts
```

Ein abgeleitetes Risiko referenziert die Item-IDs, nicht nur das gesamte Dokument.

### 9.2 IDs werden deterministisch vergeben

Ein Agent erzeugt Kandidaten:

```json
{
  "candidateId": "candidate-3",
  "artifactType": "requirement",
  "text": "...",
  "sourceClaimIds": ["CLM-0042"]
}
```

Ein Executor beziehungsweise Project Store reserviert:

```json
{
  "artifactId": "REQ-0012",
  "itemId": "REQ-0012-R03",
  "version": 1
}
```

Das LLM darf keine dauerhaften IDs frei wählen.

---

## 10. Strukturiertes Artifact-Modell

Markdown bleibt ein Ausgabeformat für Menschen und GitHub, darf aber nicht das einzige Datenformat sein.

Für jedes Artefakt werden gespeichert:

```text
artifact.md
artifact.json
checker-report.json
```

Beispielmodell:

```csharp
public sealed record ArtifactAssertion(
    string ItemId,
    string Text,
    ArtifactType ArtifactType,
    AssertionKind AssertionKind,
    ArtifactOrigin Origin,
    IReadOnlyList<string> SourceClaimIds,
    IReadOnlyList<string> SourceArtifactItemIds,
    string? DerivationRationale,
    IReadOnlyList<string> Assumptions,
    ReviewStatus ReviewStatus,
    ProducerMetadata Producer
);
```

### AssertionKind

```csharp
public enum AssertionKind
{
    NormalizedRequirement,
    ElicitedRisk,
    DerivedRisk,
    OpenQuestion,
    ArchitectureConstraint,
    ArchitectureDecision,
    ArchitectureProposal,
    UserStory,
    AcceptanceCriterion,
    Assumption
}
```

### ArtifactOrigin

```csharp
public enum ArtifactOrigin
{
    Extracted,
    Derived,
    Assumed,
    Proposed
}
```

---

## 11. Gemeinsamer Agenten-Input-Contract

Artifact Agents sollen nicht fest an eine vollständige `consumable.json` oder eine bestimmte Datei gekoppelt sein.

```csharp
public sealed record ArtifactGenerationRequest(
    string ProjectId,
    string RunId,
    ArtifactType ArtifactType,
    ArtifactOperation Operation,
    ProjectScope Scope,
    IReadOnlyList<ConsumableClaim> Claims,
    IReadOnlyList<ArtifactAssertion> SourceAssertions,
    IReadOnlyList<ExistingArtifact> ExistingArtifacts,
    ArtifactGenerationPolicy Policy
);
```

### Operationen

```csharp
public enum ArtifactOperation
{
    CreateInitial,
    Create,
    Update,
    Reconcile,
    Reassess
}
```

### Kickoff-Baseline

```json
{
  "operation": "CreateInitial",
  "scope": {
    "type": "project"
  },
  "claims": ["freigegebene Ledger Claims"],
  "sourceAssertions": [],
  "existingArtifacts": [],
  "policy": {
    "mode": "extraction"
  }
}
```

### Derived Risks

```json
{
  "operation": "Create",
  "scope": {
    "type": "project"
  },
  "claims": [],
  "sourceAssertions": [
    "geprüfte Requirements",
    "geprüfte explizite Risiken"
  ],
  "existingArtifacts": [
    "bereits existierende Derived Risks"
  ],
  "policy": {
    "mode": "inference"
  }
}
```

### Späterer Sprint

```json
{
  "operation": "Reconcile",
  "scope": {
    "type": "feature",
    "id": "calendar"
  },
  "claims": [
    "neue Claims",
    "relevante bestehende Claims"
  ],
  "sourceAssertions": [
    "betroffene aktuelle Artefakt-Items"
  ],
  "existingArtifacts": [
    {
      "artifactId": "REQ-0012",
      "version": 2
    }
  ]
}
```

---

## 12. Zwei Checker-Semantiken

## 12.1 Extraction Checker

Der Extraction Checker prüft eine closed-world-Aussage.

Frage:

> Ist das Artefakt vollständig und korrekt durch die angegebenen Claims gedeckt?

Prüfungen:

* Claim-ID existiert,
* Citation ist auflösbar,
* Inhalt ist durch das Claim-Paket gedeckt,
* Modalität wurde nicht verstärkt,
* Scope wurde nicht erweitert,
* Required Claims wurden berücksichtigt,
* keine unbelegten Details wurden hinzugefügt.

```text
mode = extraction
```

Der vorhandene Contract Checker und C7-Critic gehören in diesen Modus.

---

## 12.2 Inference Checker

Der Inference Checker prüft keine reine Textdeckung.

Frage:

> Sind die Ausgangspunkte korrekt, die Ableitung plausibel und die neuen Inhalte transparent gekennzeichnet?

Prüfungen:

* Source Artifact Item IDs existieren,
* referenzierte Baseline-Items sind freigegeben,
* Prämissen wurden korrekt wiedergegeben,
* Ableitung ist relevant und plausibel,
* Annahmen sind explizit,
* Ergebnis ist als `derived`, `assumed` oder `proposed` markiert,
* kein Widerspruch zum Ledger oder zu freigegebenen Artefakten,
* keine irreführende Darstellung als besprochener Fakt,
* mögliche Duplikate werden erkannt.

```text
mode = inference
```

Im Inference-Modus gilt ausdrücklich nicht:

```text
Kein neues Detail außerhalb der Quelle.
```

Das neue Detail ist dort der gewünschte Output.

---

## 12.3 Proposal Checker

Für Architektur- oder Lösungsvorschläge kann ein dritter Modus verwendet werden:

```text
mode = proposal
```

Prüfungen:

* zugrunde liegende Constraints sind korrekt,
* Vorschlag widerspricht keinen beschlossenen Anforderungen,
* Annahmen und Trade-offs sind sichtbar,
* Vorschlag wird nicht als bereits beschlossene Architektur dargestellt,
* Human Approval ist bei hoher Tragweite erforderlich.

---

## 13. Checker-Repair-Subworkflow

Der vorhandene Checker-Repair-Workflow bleibt ein eigenständiger Subworkflow.

### Extraction Mode

```text
ArtifactCheckRequest
    ↓
ContractCheckerExecutor
    ↓
EvidenceCriticAgentExecutor
    ↓
VerdictRouterExecutor
    ├─ PASS → Finalize
    ├─ REPAIR → bounded Repair → Recheck
    └─ HUMAN_REVIEW
```

### Inference Mode

```text
DerivedArtifactCheckRequest
    ↓
AnchorValidationExecutor
    ↓
InferenceCriticAgentExecutor
    ↓
VerdictRouterExecutor
    ├─ PASS_WITH_REVIEW → Human Review
    ├─ MAKER_REVISION → ursprünglicher Derivation Agent
    ├─ HUMAN_REVIEW
    └─ REJECT
```

Bei Inferenz ist subtraktiver Auto-Repair vorsichtiger zu verwenden. Das Entfernen eines neuen Spans kann den eigentlichen Sinn einer Ableitung zerstören.

Daher:

```text
Form- oder Kennzeichnungsfehler
→ lokaler Repair

schwache Ableitung
→ zurück zum Derivation Agent

mehrdeutige Plausibilität
→ Human Review
```

MAF unterstützt Human-in-the-loop über Request-/Response-Mechanismen, bei denen ein Workflow auf externe Rückmeldung warten und anschließend fortgesetzt werden kann.

---

## 14. Kickoff-Workflow

Der Kickoff-Workflow baut den initialen Projektzustand auf.

```text
Kickoff Transcript
    ↓
EvidenceLedgerWorkflow
    ↓
Human Approval
    ↓
Approved Consumable Snapshot
    ↓
Evidence-Baseline Fan-out
    ├─ EvidenceBoundRequirementsAgent
    │      ↓
    │  CheckerRepair(extraction)
    │
    ├─ EvidenceBoundRisksAgent
    │      ↓
    │  CheckerRepair(extraction)
    │
    ├─ EvidenceBoundArchitectureAgent
    │      ↓
    │  CheckerRepair(extraction)
    │
    └─ EvidenceBoundOpenQuestionsAgent
           ↓
       CheckerRepair(extraction)
    ↓
Verified Baseline Fan-in
    ↓
Derivation Fan-out
    ├─ DerivedRisksAgent
    │      ↓
    │  Checker(inference)
    │
    ├─ RequirementsGapAgent
    │      ↓
    │  Checker(inference)
    │
    └─ weitere konfigurierte Derivation Agents
           ↓
       Checker(inference/proposal)
    ↓
Human Review für Derived Items
    ↓
Approved Artifact Set
    ↓
GitHub Issue Planning
    ↓
GitHub Issue Sync
```

Nicht jeder Derivation Agent muss bereits im ersten Release umgesetzt sein. Die Struktur muss jedoch neue Ableitungszweige zulassen.

---

## 15. Fan-out und Fan-in

Die initialen Evidence Agents laufen parallel, weil sie dieselbe freigegebene Primärquelle nutzen und keine Chat-History voneinander benötigen.

```text
                         ┌─ Requirements Baseline
                         ├─ Risks Baseline
Approved Consumable ─────┼─ Architecture Baseline
                         └─ Open Questions Baseline
```

Danach werden nur geprüfte Baselines zusammengeführt:

```text
Verified Requirements ────┐
Verified Elicited Risks ───┼→ DerivedRisksAgent
Verified Open Questions ───┘
```

Eine sequenzielle Edge ist erst sinnvoll, wenn eine echte Datenabhängigkeit besteht.

Beispiel:

```text
Verified Requirements
+ Verified Elicited Risks
→ Derived Risks
```

---

## 16. Modulare Derivation Agents

Jeder neue Agent deklariert:

```text
reads
writes
checkerPolicy
humanReviewPolicy
```

Beispiel:

```json
{
  "agentId": "DerivedRisksAgent",
  "reads": [
    "normalized_requirement",
    "elicited_risk",
    "open_question"
  ],
  "writes": [
    "derived_risk"
  ],
  "checkerPolicy": "inference-risk-v1",
  "humanReviewPolicy": "required"
}
```

Weitere spätere Agenten:

```json
{
  "agentId": "UserStoryDerivationAgent",
  "reads": [
    "normalized_requirement"
  ],
  "writes": [
    "user_story"
  ],
  "checkerPolicy": "inference-user-story-v1"
}
```

Damit können neue Agenten an vorhandene Outputs angeschlossen werden, ohne den Evidence Layer neu zu bauen.

---
## 17. Derivation Context Builder

Ein Derivation Agent soll weder nur ein einzelnes Artefakt noch ungefiltert den gesamten Projektkontext erhalten.

Stattdessen wird vor jedem Derivation Agent ein deterministischer Context Builder ausgeführt:

```text
Verified Baseline Set
    ↓
BuildDerivationContextExecutor
    ↓
Derivation Agent
```

### Typ

```text
Custom Executor
```

### Zweck

Der `BuildDerivationContextExecutor` erzeugt für einen konkreten Derivation Agent eine typisierte, relevante und ausschließlich aus geprüften Quellen bestehende Context Projection.

Er entscheidet nicht fachlich, welche neue Ableitung erzeugt werden soll. Er bestimmt nur, welche bereits vorhandenen und freigegebenen Informationen gemäß Agentenvertrag als Input bereitgestellt werden.

### Grundlage

Jeder Derivation Agent deklariert über seinen Vertrag:

```text
reads
writes
scopePolicy
checkerPolicy
humanReviewPolicy
```

Beispiel:

```json
{
  "agentId": "DerivedRisksAgent",
  "reads": [
    "normalized_requirement",
    "elicited_risk",
    "architecture_constraint",
    "open_question"
  ],
  "writes": [
    "derived_risk"
  ],
  "scopePolicy": "same_or_related_scope",
  "checkerPolicy": "inference-risk-v1",
  "humanReviewPolicy": "required"
}
```

### Aufgaben des Executors

Der `BuildDerivationContextExecutor`:

* liest den `reads`-Vertrag des Zielagenten,
* berücksichtigt nur geprüfte oder freigegebene Baseline-Items,
* filtert nach Projekt-, Feature- oder Artifact-Scope,
* lädt direkte und fachlich relevante Nachbarartefakte,
* ergänzt bereits vorhandene Derived Items zur Duplikatprüfung,
* erhält die stabilen Artifact- und Item-IDs,
* übernimmt Review- und Versionsinformationen,
* begrenzt die Kontextgröße deterministisch,
* protokolliert, welche Items ausgewählt oder verworfen wurden.

Er erzeugt keine neuen fachlichen Aussagen und verändert keine Artefakte.

### Context Contract

```csharp
public sealed record DerivationContextRequest(
    string ProjectId,
    string RunId,
    string AgentId,
    ProjectScope Scope,
    IReadOnlyList<string> TriggerItemIds,
    DerivationContextPolicy Policy
);
```

```csharp
public sealed record DerivationContext(
    string ProjectId,
    string RunId,
    string AgentId,
    ProjectScope Scope,
    IReadOnlyList<ArtifactAssertion> PrimaryAssertions,
    IReadOnlyList<ArtifactAssertion> SupportingAssertions,
    IReadOnlyList<ArtifactAssertion> ExistingDerivedItems,
    IReadOnlyList<ContextSelectionDecision> SelectionDecisions
);
```

### Primary und Supporting Assertions

Der Kontext unterscheidet zwischen primären und unterstützenden Aussagen.

```text
Primary Assertions
= Aussagen, die den direkten Ableitungsauftrag auslösen

Supporting Assertions
= weitere geprüfte Aussagen, die zur korrekten Bewertung notwendig sind
```

Beispiel für den `DerivedRisksAgent`:

```json
{
  "primaryAssertions": [
    {
      "itemId": "REQ-0012-R01",
      "assertionKind": "normalized_requirement"
    }
  ],
  "supportingAssertions": [
    {
      "itemId": "ARCH-0003-C02",
      "assertionKind": "architecture_constraint"
    },
    {
      "itemId": "OQ-0024",
      "assertionKind": "open_question"
    }
  ],
  "existingDerivedItems": [
    {
      "itemId": "DRISK-0007",
      "assertionKind": "derived_risk"
    }
  ]
}
```

### Beispiel

Ein Requirement lautet:

```text
Das System muss offline nutzbar sein.
```

Ein Architecture Constraint lautet:

```text
Änderungen werden erst bei erneuter Verbindung synchronisiert.
```

Das Requirement allein liefert möglicherweise nicht genug Kontext für eine belastbare Risikoableitung.

Der Context Builder kombiniert deshalb die relevanten geprüften Items:

```text
REQ-0012-R01
+ ARCH-0003-C02
→ DerivedRisksAgent
```

Der Agent kann daraufhin nachvollziehbar ein mögliches Synchronisationsrisiko untersuchen.

### Context-Auswahl

Die Context-Auswahl erfolgt vorzugsweise in mehreren Stufen:

```text
1. direkte Trigger-Items
2. explizit referenzierte Parent- oder Related-Items
3. Items desselben Scopes
4. gemäß reads-Vertrag passende Artefakttypen
5. optional semantisch ähnliche geprüfte Items
6. bestehende Outputs desselben Derivation-Typs
```

Deterministische Beziehungen und IDs haben Vorrang vor semantischer Ähnlichkeit.

Semantische Suche darf zur Kandidatensuche verwendet werden, entscheidet aber nicht allein darüber, ob eine Quelle gültig ist.

### Nicht zulässiger Kontext

Der Executor übergibt standardmäßig nicht:

```text
vollständige Agenten-Chat-History
ungeprüfte Maker-Outputs
komplettes Transkript
vollständigen Ledger ohne Filter
alle Projektartefakte
alle historischen Artefaktversionen
```

Bei Bedarf kann der Agent zusätzlich gezielte Claims oder Evidence Spans erhalten. Diese dienen der Verifikation einzelner Prämissen und ersetzen nicht die geprüften Baseline-Artefakte als primäre Quelle der Derivation Layer.

### Initialer Kickoff

Im initialen Kickoff kann der Context Builder auf dem vollständigen, aber bereits geprüften Baseline Set arbeiten:

```text
Verified Requirements
Verified Elicited Risks
Verified Architecture Constraints
Verified Open Questions
    ↓
BuildDerivationContextExecutor
    ↓
DerivedRisksAgent
```

Auch dabei erhält ein Agent nur die Typen, die sein `reads`-Vertrag erlaubt.

### Späterer Sprint

Bei einem späteren Sprint wird der Kontext zusätzlich durch den Impact Plan begrenzt:

```text
Approved Claim Delta
    ↓
Impact Plan
    ↓
betroffener Scope und Trigger-Items
    ↓
BuildDerivationContextExecutor
    ↓
nur relevanter Derivation Agent
```

Dadurch müssen weder alle Baseline-Artefakte noch alle Derivation Agents erneut verarbeitet werden.

### Reproduzierbarkeit

Der Executor speichert eine Context Selection Record:

```json
{
  "agentId": "DerivedRisksAgent",
  "scope": {
    "type": "feature",
    "id": "calendar"
  },
  "selectedItemIds": [
    "REQ-0012-R01",
    "ARCH-0003-C02",
    "OQ-0024"
  ],
  "excludedItemIds": [
    "REQ-0045-R03"
  ],
  "policyVersion": "derivation-context-v1",
  "reasoningMode": "deterministic-plus-semantic-candidates"
}
```

Damit kann später nachvollzogen werden, auf welchem konkreten Kontext eine Ableitung beruhte.

### Architekturregel

```text
Nicht nur ein Artefakt.
Nicht alles ungefiltert.
Sondern eine typisierte, scope-begrenzte und geprüfte Context Projection.
```

Der `BuildDerivationContextExecutor` bildet damit die kontrollierte Grenze zwischen der Evidence-Baseline Layer und der Derivation Layer.

## 18. Project State

Der Project State ist der dauerhaft gespeicherte fachliche Zustand.

Er enthält:

```text
Project Metadata
Sources
Claims
Claim Relations
Artifacts
Artifact Versions
Artifact Items
Artifact Relations
Review Decisions
GitHub Mappings
Sync Events
```

Der Project State ist nicht:

* Agenten-Chat-History,
* MAF Shared State,
* MAF Checkpoint,
* Memory-Zusammenfassung,
* vollständige Repo-Historie im Prompt.

### Minimaler Start

```text
project-state/
├─ project.json
├─ sources.json
├─ claims.json
├─ claim-relations.json
├─ artifacts.json
├─ artifact-items.json
├─ artifact-versions.json
├─ artifact-relations.json
├─ review-decisions.json
├─ github-mappings.json
└─ sync-events.jsonl
```

Später kann dies durch SQLite oder PostgreSQL ersetzt werden.

---

## 18. Project-Store-Schnittstelle

```csharp
public interface IProjectStore
{
    Task<ProjectContext> GetRelevantContextAsync(
        ProjectContextQuery query,
        CancellationToken cancellationToken);

    Task<string> ReserveArtifactIdAsync(
        string projectId,
        ArtifactType artifactType,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ReserveArtifactItemIdsAsync(
        string artifactId,
        int count,
        CancellationToken cancellationToken);

    Task SaveArtifactVersionAsync(
        ArtifactVersion artifact,
        CancellationToken cancellationToken);

    Task SaveArtifactRelationsAsync(
        IReadOnlyList<ArtifactRelation> relations,
        CancellationToken cancellationToken);

    Task SaveReviewDecisionAsync(
        ReviewDecision decision,
        CancellationToken cancellationToken);

    Task<ExternalMapping?> GetExternalMappingAsync(
        string artifactId,
        string externalSystem,
        CancellationToken cancellationToken);

    Task SaveExternalMappingAsync(
        ExternalMapping mapping,
        CancellationToken cancellationToken);

    Task AppendSyncEventAsync(
        SyncEvent syncEvent,
        CancellationToken cancellationToken);
}
```

Implementierungen:

```text
JsonProjectStore
SqliteProjectStore
PostgresProjectStore
```

Agenten und Workflows dürfen nicht an eine konkrete Implementierung gekoppelt sein.

---

## 20. GitHub-Issue-Integration

Im ersten Integrationsausbau werden nur GitHub Issues betrachtet.

Die GitHub-Integration wird nicht als allmächtiger Agent gebaut.

```text
Approved Artifact Changes
    ↓
LoadGitHubStateExecutor
    ↓
GitHubIssuePlannerAgent
    ↓
ValidateGitHubPlanExecutor
    ↓
Human Approval, falls notwendig
    ↓
GitHubIssueExecutor
    ↓
SaveGitHubMappingExecutor
```

Der Planner Agent formuliert und plant. Der Executor führt nur validierte Side Effects aus.

---

## 21. GitHub-Issue-Plan

```csharp
public sealed record GitHubIssueOperation(
    string OperationId,
    GitHubIssueAction Action,
    string ArtifactId,
    IReadOnlyList<string> ArtifactItemIds,
    int ArtifactVersion,
    int? ExistingIssueNumber,
    string Title,
    string Body,
    IReadOnlyList<string> Labels,
    IReadOnlyList<int> RelatedIssueNumbers,
    bool HumanApprovalRequired
);
```

Aktionen:

```text
Create
Update
CreateFollowUp
Reopen
Close
NoOp
HumanReview
```

Deterministische Policy:

```text
neues Artifact ohne Mapping
→ Create

bestehendes Artifact mit offenem Issue
→ Update

bestehendes Artifact mit geschlossenem Issue
+ neue Erweiterung
→ CreateFollowUp

alte Umsetzung war unvollständig
→ Reopen prüfen

unsichere Zuordnung oder Konflikt
→ HumanReview
```

---

## 23. Sprint-Transkripte: inkrementeller Ablauf

Ein späteres Sprint-Transkript durchläuft nicht erneut den gesamten Kickoff-Workflow.

Es nutzt dieselben Schichten inkrementell:

```text
Sprint Transcript
    ↓
EvidenceLedgerWorkflow
    ↓
Human Approval
    ↓
Approved Claim Delta
    ↓
Load Relevant Project Context
    ↓
Claim Reconciliation
    ↓
Impact Planning
    ↓
nur betroffene Baseline-Workflows
    ↓
nur betroffene Derivation-Workflows
    ↓
Checker und Human Review
    ↓
GitHub Create / Update / Follow-up
    ↓
Project State N+1
```

---

## 22. Claim Reconciliation

Neue Claims werden gegen relevante bestehende Claims abgeglichen.

```text
New
Duplicate
Extends
Refines
Supersedes
Contradicts
Unclear
```

Ablauf:

```text
New Approved Claims
    ↓
CandidateSearchExecutor
    ↓
ClaimReconciliationAgent
    ↓
ValidateReconciliationExecutor
```

Der Candidate Search lädt nicht den vollständigen Ledger, sondern:

* Claims desselben Scopes,
* Claims betroffener Artefakte,
* semantisch ähnliche Claims,
* aktive und kürzlich supersedierte Claims.

---

## 24. Impact Planner

Der Impact Planner entscheidet nicht frei über das gesamte System. Er erstellt einen begründeten Ausführungsplan innerhalb einer festen Routing Policy.

Beispiel:

```json
{
  "impacts": [
    {
      "artifactType": "requirements",
      "action": "update",
      "targetArtifactIds": ["REQ-0012"],
      "confidence": 0.94,
      "reason": "Die bestehende Kalenderanforderung wird erweitert."
    },
    {
      "artifactType": "user_story",
      "action": "evaluate",
      "confidence": 0.86
    },
    {
      "artifactType": "derived_risk",
      "action": "evaluate",
      "confidence": 0.71
    },
    {
      "artifactType": "architecture",
      "action": "skip",
      "confidence": 0.89
    }
  ]
}
```

Wichtig:

```text
Agent wird gestartet
≠
Agent muss ein neues Artefakt erzeugen
```

Ein Agent darf zurückgeben:

```json
{
  "decision": "no_change",
  "reason": "Keine neue fachlich relevante Auswirkung erkannt."
}
```

---

## 25. Routing Policy

Beispiel:

```json
{
  "rules": [
    {
      "condition": "new_or_changed_functional_requirement",
      "run": ["requirements"],
      "consider": ["user_story", "derived_risk"]
    },
    {
      "condition": "explicit_risk_or_dependency",
      "run": ["elicited_risk"]
    },
    {
      "condition": "integration_or_data_constraint",
      "consider": ["architecture", "derived_risk"]
    },
    {
      "condition": "open_or_conflicting_claim",
      "run": ["open_questions"]
    }
  ]
}
```

Confidence Policy:

```text
hohe Sicherheit
→ automatisch routen

mittlere Sicherheit
→ Analyseworkflow ausführen,
  Output reviewpflichtig

niedrige Sicherheit
→ Human Review oder Open Question
```

---

## 26. Beispiel eines späteren Sprint-Ablaufs

Im Sprint-Meeting wird besprochen:

```text
„Die bestehende Kalenderansicht soll zusätzlich
eine Wochenansicht erhalten.“

„Ob mehrere Einträge gleichzeitig verschoben werden
können, ist noch offen.“
```

### Schritt 1: Ledger Delta

```text
CLM-0184:
Wochenansicht erforderlich.

CLM-0185:
Mehrfachverschiebung offen.
```

### Schritt 2: Reconciliation

```text
CLM-0184 extends bestehende Calendar Requirement.
CLM-0185 ist eine neue offene Entscheidung.
```

### Schritt 3: Impact Plan

```text
Requirements Update
Open Question Create
User Story Evaluate
Derived Risks Evaluate
Architecture Skip
```

### Schritt 4: Baseline Update

```text
EvidenceBoundRequirementsAgent(operation=Update)
→ REQ-0012 v2

EvidenceBoundOpenQuestionsAgent(operation=Create)
→ OQ-0024
```

### Schritt 5: Extraction Checks

```text
REQ-0012 v2
→ Claims CLM-0072 + CLM-0184

OQ-0024
→ Claim CLM-0185
```

### Schritt 6: Derived Analysis

```text
Updated Requirement
+ Open Question
→ DerivedRisksAgent
```

Der Agent kann entweder:

```text
neues Derived Risk erzeugen
```

oder:

```text
no_new_risk
```

### Schritt 7: GitHub

```text
REQ-0012 hat offenes Issue
→ Update

REQ-0012 hat geschlossenes Issue
→ Follow-up Issue

OQ-0024 ist neu
→ neues Issue
```

Es wird nicht erneut die komplette Projektanalyse ausgeführt.

---

## 27. Aktueller und späterer Kontext

Der vollständige Project Store darf nach einem Jahr groß sein. Kein Agent erhält ihn vollständig.

```text
Project Store
    ↓
ProjectContextQuery
    ↓
relevante Projektion
    ↓
Agent
```

Ein Agent erhält beispielsweise:

* neue Claims,
* aktive verwandte Claims,
* aktuelle betroffene Artefakte,
* direkte Vorgängerversion,
* relevante offene Issues,
* kürzlich geschlossene verwandte Issues,
* bestehende Derived Items zur Duplikatprüfung.

Er erhält nicht automatisch:

* alle Transkripte,
* alle Claims,
* alle Commits,
* alle Issues,
* die komplette Agenten-Historie.

---

## 28. Shared State, Checkpoints und Project State

### MAF Shared State

Geeignet für:

```text
runId
projectId
ledgerSnapshotId
changeSetId
aktuelle Branch-Ergebnisse
repairIteration
pendingReviewId
```

### Checkpoint

Geeignet für:

```text
Pausieren und Fortsetzen desselben Workflow-Runs
```

### Project Store

Geeignet für:

```text
dauerhafte Claims
Artefakte
Versionen
Relationen
GitHub-Mappings
Review-Entscheidungen
```

Diese drei Zustandsformen dürfen nicht vermischt werden.

---

## 29. Reproduzierbarkeit und Konfiguration

Das bestehende Config- und Run-Snapshot-System bleibt verpflichtend.

Neue Konfigurationen:

```text
artifactOrchestration:
  linear | fanout

artifactStage:
  evidence_baseline | derivation

artifactOperation:
  create_initial | create | update | reconcile | reassess

checkerMode:
  extraction | inference | proposal

projectStateMode:
  none | json | sqlite | postgres

githubMode:
  disabled | plan_only | execute
```

Jeder Run speichert:

```text
effective config
run ID
model versions
prompt versions
source IDs
ledger snapshot ID
checker policies
review policies
```

Bestehende Modi bleiben unverändert reproduzierbar.

---

## 30. Events und Observability

Wichtige Events:

```text
LEDGER_APPROVED
BASELINE_ARTIFACT_GENERATED
EXTRACTION_CHECK_COMPLETED
BASELINE_ARTIFACT_APPROVED
DERIVATION_STARTED
DERIVED_ITEM_GENERATED
INFERENCE_CHECK_COMPLETED
HUMAN_REVIEW_REQUESTED
DERIVED_ITEM_APPROVED
ARTIFACT_VERSION_SAVED
GITHUB_PLAN_CREATED
GITHUB_OPERATION_EXECUTED
PROJECT_STATE_UPDATED
WORKFLOW_COMPLETED
```

Dadurch ist sichtbar, welche Aussagen aus Evidence stammen und welche später abgeleitet wurden.

---

## 30. Implementierungsphasen

## Phase 1 – vorhandenen Checker ehrlich abschließen

* CheckerRepair einmal real end-to-end ausführen.
* C5-Naht korrigieren.
* Runtime-Evidenz für Check → Repair → Recheck erzeugen.

## Phase 2 – Datenverträge stabilisieren

Einführen:

* Project ID,
* Source ID,
* Artifact ID,
* Artifact Item ID,
* Version,
* `ArtifactOrigin`,
* `AssertionKind`,
* `sourceClaimIds`,
* `sourceArtifactItemIds`,
* Producer-Metadaten.

## Phase 3 – Evidence-Baseline Fan-out

```text
Consumable
→ vier parallele Evidence Agents
→ je Branch Extraction Checker
→ Verified Baseline Fan-in
```

Keine Agenten-Chat-History zwischen den Branches.

## Phase 4 – strukturierte Outputs

Zusätzlich zu Markdown:

```text
artifact.json
checker-report.json
lineage.json
```

## Phase 5 – erste Inferenzschicht

Zunächst:

```text
Verified Requirements
+ Verified Elicited Risks
→ DerivedRisksAgent
→ Inference Checker
→ Human Review
```

Der DerivedRisksAgent ist der erste produktive Nachweis der zweiten Schicht.

## Phase 6 – minimaler Project Store

Zunächst:

```text
IProjectStore
JsonProjectStore
```

Persistiert werden:

* Claims,
* Baseline-Artefakte,
* Derived Items,
* Versionen,
* Relationen,
* Review-Entscheidungen.

## Phase 7 – GitHub Issue Planning und Execution

```text
GitHubIssuePlannerAgent
ValidateGitHubPlanExecutor
Human Approval
GitHubIssueExecutor
SaveGitHubMappingExecutor
```

## Phase 8 – erster inkrementeller Test

Ein zweites kleines Transkript:

* neuer Claim,
* Erweiterung eines bestehenden Claims,
* Update eines Baseline-Artefakts,
* Prüfung eines Derived Risks,
* neues oder Follow-up-Issue.

## Phase 9 – weitere Agenten modular ergänzen

Beispiele:

```text
UserStoryDerivationAgent
AcceptanceCriteriaAgent
RequirementsGapAgent
ArchitectureProposalAgent
```

Jeder Agent erhält einen klaren Read-/Write-/Checker-Vertrag.

---

## 31. Zielgraph des ersten erweiterten Releases

```text
Kickoff Transcript
    ↓
Evidence Ledger
    ↓
Human Approval
    ↓
Approved Consumable
    ↓
Evidence-Baseline Fan-out
    ├─ Requirements Baseline
    ├─ Risks Baseline
    ├─ Architecture Baseline
    └─ Open Questions Baseline
    ↓
jeweils Extraction CheckerRepair
    ↓
Verified Baseline Fan-in
    ↓
DerivedRisksAgent
    ↓
Inference Checker
    ↓
Human Review
    ↓
Approved Artifact Set
    ↓
GitHub Issue Planner
    ↓
GitHub Plan Validator
    ↓
Human Approval
    ↓
GitHub Issue Executor
    ↓
Project State aktualisieren
```

---

## 32. Zielgraph für spätere Sprint-Transkripte

```text
Sprint Transcript
    ↓
Evidence Ledger
    ↓
Approved Claim Delta
    ↓
Relevant Project Context
    ↓
Claim Reconciliation
    ↓
Impact Plan
    ↓
Dynamic Baseline Fan-out
    ├─ Create
    ├─ Update
    ├─ Reconcile
    └─ Skip
    ↓
Extraction Checks
    ↓
Dynamic Derivation Fan-out
    ├─ Derived Risks
    ├─ User Stories
    ├─ Acceptance Criteria
    └─ weitere konfigurierte Module
    ↓
Inference Checks und Human Review
    ↓
GitHub Create / Update / Follow-up
    ↓
Project State N+1
```

---

## 33. Architekturentscheidungen

### Entscheidung 1

Der Ledger bleibt der Evidence-Nullpunkt.

### Entscheidung 2

Der erste Artifact Agent erzeugt eine geprüfte Baseline, nicht zwingend das Endergebnis.

### Entscheidung 3

Neue fachliche Inhalte entstehen in einer getrennten Derivation Layer.

### Entscheidung 4

Baseline und Derived Items werden niemals stillschweigend vermischt.

### Entscheidung 5

Jeder Hop der Provenienzkette erhält stabile IDs.

### Entscheidung 6

Extraction und Inference verwenden unterschiedliche Checker-Semantiken.

### Entscheidung 7

Derivation Agents arbeiten nur auf geprüften Baseline-Artefakten.

### Entscheidung 8

Derived Items benötigen Human Review, wenn sie neue Annahmen oder fachlich relevante Vorschläge enthalten.

### Entscheidung 9

MAF orchestriert; der Project Store persistiert.

### Entscheidung 10

GitHub-Planung und GitHub-Ausführung bleiben getrennt.

### Entscheidung 11

Sprint-Transkripte erzeugen Deltas und lösen nur betroffene Workflows aus.

### Entscheidung 12

Neue Agenten werden über typisierte Read-/Write-/Checker-Verträge ergänzt, nicht durch Umbau des gesamten Graphen.

---

## 34. Abnahmekriterien des ersten erweiterten Releases

Der Release gilt als abgeschlossen, wenn:

1. Der Ledger einen human-approved Consumable Snapshot erzeugt.
2. Die vier Evidence Agents im Fan-out laufen.
3. Jeder Evidence Agent unabhängig auf der freigegebenen Quelle arbeitet.
4. Jeder direkte Baseline-Inhalt Claim-IDs referenziert.
5. Jeder Baseline-Branch den Extraction Checker durchläuft.
6. Baseline-Artefakte stabile Artifact- und Item-IDs besitzen.
7. Baseline-Artefakte strukturiert und als Markdown gespeichert werden.
8. Mindestens ein Derivation Agent auf geprüften Baselines läuft.
9. Jeder Derived Output Source Artifact Item IDs referenziert.
10. Der Inference Checker die Ableitung und nicht reine Textdeckung prüft.
11. Derived Items einen Human-Review-Status besitzen.
12. Baseline und Derived Items getrennt gespeichert werden.
13. Approved Items in GitHub Issue Plans überführt werden.
14. GitHub-Writes deterministisch validiert werden.
15. Artifact-to-Issue-Mappings persistiert werden.
16. Ein wiederholter Run keine doppelten Issues erzeugt.
17. Alle wesentlichen Schritte Audit Events erzeugen.
18. Config und Prompt-Versionen für jeden Run gespeichert werden.

---

## 35. Nichtziele des ersten erweiterten Releases

Noch nicht umzusetzen:

* vollständige bidirektionale GitHub-Synchronisation,
* automatische Verarbeitung aller GitHub-Kommentare,
* universelles Sprint-Routing,
* vollständige User-Story- und Acceptance-Criteria-Pipeline,
* Wiki- und ADR-Synchronisation,
* Pull Requests,
* Codeänderungen,
* Graphdatenbank,
* Langzeit-Memory als primärer Project Store,
* ein allmächtiger autonomer Repository-Agent.

---

## 36. Zusammenfassung

Die Zielarchitektur besteht nicht nur aus:

```text
Ledger
→ Artifact Agent
→ Checker
→ GitHub
```

Sondern aus:

```text
Transcript
→ Evidence Ledger
→ quellsichere Baseline
→ Extraction Checker
→ kontrollierte Ableitungen
→ Inference Checker
→ Human Review
→ GitHub Issues
```

Die quellsichere Baseline beantwortet:

```text
Was wurde tatsächlich gesagt?
```

Die Derivation Layer beantwortet:

```text
Was folgt fachlich daraus?
```

Der Project State speichert beide Ebenen getrennt und verbindet sie über stabile IDs.

Bei späteren Sprint-Transkripten wird nicht alles neu erzeugt. Stattdessen werden neue Claims reconciled, betroffene Baselines gezielt aktualisiert und nur relevante Derivation Agents erneut ausgeführt.

Der langfristige Grundsatz lautet:

```text
Evidence sichern
→ Baseline verifizieren
→ neue Erkenntnisse kontrolliert ableiten
→ Herkunft über jeden Hop erhalten
→ nur freigegebene Änderungen nach GitHub übertragen
```

Damit bleibt das System nachvollziehbar und kontrollierbar, kann aber gleichzeitig beliebig um neue Agenten, neue Ableitungstypen und spätere Integrationsziele erweitert werden.
