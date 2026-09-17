# Evidence-first Semantic-Ledger-Spike

## Run

```text
dotnet run --project AgenticSdlc.Host -- \
  evidence-first-spike \
  /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-interview-spike.json \
  open-questions \
  phase2B \
  20260630_145558_68149f \
  openai/gpt-5.4
```

## Inputs

```text
Fixture:      /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-interview-spike.json
Old artifact: /Users/armino/devProjects/Agentic-SDLC/runs/phase2B/20260630_145558_68149f/snapshots/docs/open-questions.md
Artifact:     open-questions
Model:        openai/gpt-5.4
```

## Outputs

```text
Generated artifact: /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.open-questions.phase2B_20260630_145558_68149f.openai_gpt-5.4.md
Generated claims:   /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.open-questions.phase2B_20260630_145558_68149f.openai_gpt-5.4.claims.json
Old claims:         /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.open-questions.phase2B_20260630_145558_68149f.openai_gpt-5.4.old-claims.json
Verification:       /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.open-questions.phase2B_20260630_145558_68149f.openai_gpt-5.4.verification.json
```

## Metrics

Old artifact:

```json
{
  "total": 11,
  "artifactClaims": 52,
  "requiredDispositions": 5,
  "semanticRepresentationCoverage": 0.7273,
  "sourceIdDispositionCoverage": 0.5455,
  "requiredSourceIdDispositionCoverage": 0,
  "semanticPreservation": 0,
  "requiredSemanticRepresentationCoverage": 1,
  "requiredSemanticPreservation": 0,
  "requiredOmitted": 0,
  "requiredMisrepresented": 0,
  "preserved": 0,
  "partial": 4,
  "omitted": 3,
  "misrepresented": 3,
  "contradicted": 0,
  "unclear": 1,
  "statusShifts": 3,
  "scopeShifts": 1,
  "unsupportedAssumptions": 52
}
```

Evidence-first artifact:

```json
{
  "total": 11,
  "artifactClaims": 6,
  "requiredDispositions": 5,
  "semanticRepresentationCoverage": 0.5455,
  "sourceIdDispositionCoverage": 1,
  "requiredSourceIdDispositionCoverage": 1,
  "semanticPreservation": 0.3636,
  "requiredSemanticRepresentationCoverage": 1,
  "requiredSemanticPreservation": 0.6,
  "requiredOmitted": 0,
  "requiredMisrepresented": 2,
  "preserved": 4,
  "partial": 0,
  "omitted": 5,
  "misrepresented": 2,
  "contradicted": 0,
  "unclear": 0,
  "statusShifts": 1,
  "scopeShifts": 0,
  "unsupportedAssumptions": 0
}
```

Completeness issues:

```text
Old artifact verifier issues:        1
Evidence-first verifier issues:      0
```

## Interpretation Template

Dieser Spike misst Semantic Preservation und Disposition Coverage gegen eine bestaetigte Fixture.
Er misst nicht die Vollstaendigkeit der automatischen Ledger-Extraction.