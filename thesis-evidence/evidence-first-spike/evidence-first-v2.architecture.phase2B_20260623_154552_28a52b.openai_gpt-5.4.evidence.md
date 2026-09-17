# Evidence-first Semantic-Ledger-Spike

## Run

```text
dotnet run --project AgenticSdlc.Host -- \
  evidence-first-spike \
  /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-interview-spike.json \
  architecture \
  phase2B \
  20260623_154552_28a52b \
  openai/gpt-5.4
```

## Inputs

```text
Fixture:      /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-interview-spike.json
Old artifact: /Users/armino/devProjects/Agentic-SDLC/runs/phase2B/20260623_154552_28a52b/snapshots/docs/architecture.md
Artifact:     architecture
Model:        openai/gpt-5.4
```

## Outputs

```text
Generated artifact: /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.architecture.phase2B_20260623_154552_28a52b.openai_gpt-5.4.md
Generated claims:   /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.architecture.phase2B_20260623_154552_28a52b.openai_gpt-5.4.claims.json
Old claims:         /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.architecture.phase2B_20260623_154552_28a52b.openai_gpt-5.4.old-claims.json
Verification:       /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.architecture.phase2B_20260623_154552_28a52b.openai_gpt-5.4.verification.json
```

## Metrics

Old artifact:

```json
{
  "total": 11,
  "artifactClaims": 35,
  "requiredDispositions": 7,
  "semanticRepresentationCoverage": 0.7273,
  "sourceIdDispositionCoverage": 0.3636,
  "requiredSourceIdDispositionCoverage": 0,
  "semanticPreservation": 0,
  "requiredSemanticRepresentationCoverage": 0.8571,
  "requiredSemanticPreservation": 0,
  "requiredOmitted": 1,
  "requiredMisrepresented": 6,
  "preserved": 0,
  "partial": 0,
  "omitted": 3,
  "misrepresented": 8,
  "contradicted": 0,
  "unclear": 0,
  "statusShifts": 7,
  "scopeShifts": 4,
  "unsupportedAssumptions": 35
}
```

Evidence-first artifact:

```json
{
  "total": 11,
  "artifactClaims": 12,
  "requiredDispositions": 7,
  "semanticRepresentationCoverage": 1,
  "sourceIdDispositionCoverage": 1,
  "requiredSourceIdDispositionCoverage": 1,
  "semanticPreservation": 0.9091,
  "requiredSemanticRepresentationCoverage": 1,
  "requiredSemanticPreservation": 0.8571,
  "requiredOmitted": 0,
  "requiredMisrepresented": 0,
  "preserved": 10,
  "partial": 1,
  "omitted": 0,
  "misrepresented": 0,
  "contradicted": 0,
  "unclear": 0,
  "statusShifts": 0,
  "scopeShifts": 0,
  "unsupportedAssumptions": 0
}
```

Completeness issues:

```text
Old artifact verifier issues:        0
Evidence-first verifier issues:      0
```

## Interpretation Template

Dieser Spike misst Semantic Preservation und Disposition Coverage gegen eine bestaetigte Fixture.
Er misst nicht die Vollstaendigkeit der automatischen Ledger-Extraction.