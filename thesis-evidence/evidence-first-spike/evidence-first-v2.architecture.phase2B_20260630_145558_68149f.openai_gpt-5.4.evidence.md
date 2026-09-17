# Evidence-first Semantic-Ledger-Spike

## Run

```text
dotnet run --project AgenticSdlc.Host -- \
  evidence-first-spike \
  /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-interview-spike.json \
  architecture \
  phase2B \
  20260630_145558_68149f \
  openai/gpt-5.4
```

## Inputs

```text
Fixture:      /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-interview-spike.json
Old artifact: /Users/armino/devProjects/Agentic-SDLC/runs/phase2B/20260630_145558_68149f/snapshots/docs/architecture.md
Artifact:     architecture
Model:        openai/gpt-5.4
```

## Outputs

```text
Generated artifact: /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.architecture.phase2B_20260630_145558_68149f.openai_gpt-5.4.md
Generated claims:   /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.architecture.phase2B_20260630_145558_68149f.openai_gpt-5.4.claims.json
Old claims:         /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.architecture.phase2B_20260630_145558_68149f.openai_gpt-5.4.old-claims.json
Verification:       /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.architecture.phase2B_20260630_145558_68149f.openai_gpt-5.4.verification.json
```

## Metrics

Old artifact:

```json
{
  "total": 11,
  "artifactClaims": 174,
  "requiredDispositions": 7,
  "semanticRepresentationCoverage": 0.9091,
  "sourceIdDispositionCoverage": 0.3636,
  "requiredSourceIdDispositionCoverage": 0,
  "semanticPreservation": 0,
  "requiredSemanticRepresentationCoverage": 1,
  "requiredSemanticPreservation": 0,
  "requiredOmitted": 0,
  "requiredMisrepresented": 5,
  "preserved": 0,
  "partial": 4,
  "omitted": 1,
  "misrepresented": 6,
  "contradicted": 0,
  "unclear": 0,
  "statusShifts": 5,
  "scopeShifts": 3,
  "unsupportedAssumptions": 174
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
  "semanticPreservation": 0.8182,
  "requiredSemanticRepresentationCoverage": 1,
  "requiredSemanticPreservation": 0.7143,
  "requiredOmitted": 0,
  "requiredMisrepresented": 1,
  "preserved": 9,
  "partial": 1,
  "omitted": 0,
  "misrepresented": 1,
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