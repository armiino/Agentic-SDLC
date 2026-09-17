# Evidence-first Semantic-Ledger-Spike

## Run

```text
dotnet run --project AgenticSdlc.Host -- \
  evidence-first-spike \
  /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-interview-spike.json \
  risks \
  phase2B \
  20260623_154552_28a52b \
  openai/gpt-5.4
```

## Inputs

```text
Fixture:      /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-interview-spike.json
Old artifact: /Users/armino/devProjects/Agentic-SDLC/runs/phase2B/20260623_154552_28a52b/snapshots/docs/risks.md
Artifact:     risks
Model:        openai/gpt-5.4
```

## Outputs

```text
Generated artifact: /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.risks.phase2B_20260623_154552_28a52b.openai_gpt-5.4.md
Generated claims:   /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.risks.phase2B_20260623_154552_28a52b.openai_gpt-5.4.claims.json
Old claims:         /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.risks.phase2B_20260623_154552_28a52b.openai_gpt-5.4.old-claims.json
Verification:       /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.risks.phase2B_20260623_154552_28a52b.openai_gpt-5.4.verification.json
```

## Metrics

Old artifact:

```json
{
  "total": 11,
  "artifactClaims": 37,
  "requiredDispositions": 8,
  "semanticRepresentationCoverage": 0.5455,
  "sourceIdDispositionCoverage": 0.2727,
  "requiredSourceIdDispositionCoverage": 0,
  "semanticPreservation": 0,
  "requiredSemanticRepresentationCoverage": 0.75,
  "requiredSemanticPreservation": 0,
  "requiredOmitted": 2,
  "requiredMisrepresented": 1,
  "preserved": 0,
  "partial": 1,
  "omitted": 5,
  "misrepresented": 1,
  "contradicted": 0,
  "unclear": 4,
  "statusShifts": 2,
  "scopeShifts": 0,
  "unsupportedAssumptions": 37
}
```

Evidence-first artifact:

```json
{
  "total": 11,
  "artifactClaims": 11,
  "requiredDispositions": 8,
  "semanticRepresentationCoverage": 1,
  "sourceIdDispositionCoverage": 1,
  "requiredSourceIdDispositionCoverage": 1,
  "semanticPreservation": 0.9091,
  "requiredSemanticRepresentationCoverage": 1,
  "requiredSemanticPreservation": 1,
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
Old artifact verifier issues:        4
Evidence-first verifier issues:      0
```

## Interpretation Template

Dieser Spike misst Semantic Preservation und Disposition Coverage gegen eine bestaetigte Fixture.
Er misst nicht die Vollstaendigkeit der automatischen Ledger-Extraction.