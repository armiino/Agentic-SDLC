# SourceClaim Coverage Matrix — Adjudicated Draft

Stand: 2026-06-27

Basis:

- Matrix-Run: `source-claim-coverage-matrix.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json`
- Fixture: `input/eval-labels/source-claim-coverage-spike.json`
- Artefakte: `runs/phase2_1/20260612_133345_2d7b09/snapshots/docs/*.md`

Status:

```text
Single-Reviewer-Adjudication auf Basis der vier Artefakte und der 20 SourceClaims.
Keine finale Multi-Reviewer-Gold-Truth, aber deutlich staerker als das reine LLM-Matrixurteil.
```

Labels:

- `applicability`: `required | optional | context | not_applicable | unclear`
- `coverage`: `covered | partial | missing | contradicted | not_applicable | unclear`

Regel:

```text
context = Claim hilft als Hintergrund, ist aber keine eigene Coverage-Pflicht fuer dieses Artefakt.
not_applicable = fachlich nicht sinnvoll fuer dieses Artefakt.
```

## Summary

```text
80 Matrixzellen

Applicability:
required:        74
optional:         3
context:          0
not_applicable:   3

Coverage:
covered:         28
partial:         29
missing:         18
contradicted:     2
not_applicable:   3
```

Interpretation:

```text
SourceClaims sind haeufig artefaktuebergreifend, aber nicht jede Cross-Artefact-Relevanz ist eine harte Pflicht.
Die LLM-Matrix ueberverwendete partial, weil sie context/optional oft wie echte Coverage-Pflichten behandelte.
```

## Matrix

| Claim | Artifact | LLM verdict | Applicability | Coverage | Final note |
|---|---|---:|---|---|---|
| SC-ARCH-SCALABILITY-001 | architecture | missing | required | missing | Architecture lacks explicit open load/scaling/performance assumption. |
| SC-ARCH-SCALABILITY-001 | requirements | partial | required | partial | Requirements state 200-20k/scalability, but not uncertainty/open assumption clearly. |
| SC-ARCH-SCALABILITY-001 | risks | covered | required | covered | Risks explicitly capture 200-20k uncertainty and scaling/performance risk. |
| SC-ARCH-SCALABILITY-001 | open-questions | partial | required | missing | Open questions lack explicit user/load/scaling decision; LLM partial was too lenient. |
| SC-ARCH-ANALYTICS-002 | architecture | missing | required | missing | Architecture lacks analytics/tracking implementation and privacy decision. |
| SC-ARCH-ANALYTICS-002 | requirements | partial | required | covered | Requirements include KPI measurement and technical implementation outstanding. |
| SC-ARCH-ANALYTICS-002 | risks | covered | required | covered | Risks include KPI/analytics technical/privacy risk. |
| SC-ARCH-ANALYTICS-002 | open-questions | missing | required | missing | Open questions omit analytics/KPI/tracking/privacy decision. |
| SC-ARCH-API-GATEWAY-003 | architecture | partial | required | covered | Architecture mentions central API Gateway, delay risk, and alternatives/open strategy. |
| SC-ARCH-API-GATEWAY-003 | requirements | covered | required | covered | Requirements mention API layer, API Gateway waitlist, alternatives to check, and policy. |
| SC-ARCH-API-GATEWAY-003 | risks | covered | required | covered | Risks explicitly cover API Gateway delay and architecture guideline conflict. |
| SC-ARCH-API-GATEWAY-003 | open-questions | covered | required | covered | Open questions ask how to handle Gateway delay and alternatives. |
| SC-ARCH-ENV-SECRETS-004 | architecture | partial | required | partial | Test data strategy present; Dev/Test/Prod and Secrets Management missing. |
| SC-ARCH-ENV-SECRETS-004 | requirements | partial | required | partial | Test data privacy open; Dev/Test/Prod and Secrets Management absent. |
| SC-ARCH-ENV-SECRETS-004 | risks | partial | required | partial | Test data risk present; Dev/Test/Prod and Secrets Management absent. |
| SC-ARCH-ENV-SECRETS-004 | open-questions | partial | required | partial | Test data question present; environments and secrets absent. |
| SC-ARCH-DISCOUNT-WORKFLOW-005 | architecture | partial | required | partial | Workflow/freigabe mentioned, but 15% threshold and status model incomplete. |
| SC-ARCH-DISCOUNT-WORKFLOW-005 | requirements | partial | required | partial | MVP standard rebate constraint present; 15% approval and full status model missing. |
| SC-ARCH-DISCOUNT-WORKFLOW-005 | risks | partial | required | partial | Risk names discount/freigabe uncertainty but lacks threshold/status detail. |
| SC-ARCH-DISCOUNT-WORKFLOW-005 | open-questions | partial | required | partial | MVP scope mentions rabattfreigabe, but threshold/status detail absent. |
| SC-REQ-LOGIN-006 | architecture | partial | required | partial | Auth component includes email/password and SSO, but Double-Opt-In not explicit in architecture component. |
| SC-REQ-LOGIN-006 | requirements | covered | required | covered | Requirements explicitly cover email/password login and Double-Opt-In. |
| SC-REQ-LOGIN-006 | risks | partial | optional | partial | DOI/consent risk is relevant but not the main risk obligation. |
| SC-REQ-LOGIN-006 | open-questions | not_applicable | not_applicable | not_applicable | Login/Double-Opt-In is decided, not an open question. |
| SC-REQ-SAP-WRITE-007 | architecture | partial | required | contradicted | Architecture turns “not decided” into “planned for later phases”; status overcommitted. |
| SC-REQ-SAP-WRITE-007 | requirements | contradicted | required | contradicted | Requirements also state later planned instead of open/undecided. |
| SC-REQ-SAP-WRITE-007 | risks | partial | required | partial | Risks capture missing write access, but not the undecided status precisely. |
| SC-REQ-SAP-WRITE-007 | open-questions | partial | required | partial | Open questions ask long-term write access, but not source status cleanly. |
| SC-REQ-ANALYTICS-008 | architecture | not_applicable | required | missing | Technical implementation/privacy of analytics is architecture-relevant; LLM not_applicable is wrong. |
| SC-REQ-ANALYTICS-008 | requirements | covered | required | covered | KPI measurement and technical implementation outstanding are captured. |
| SC-REQ-ANALYTICS-008 | risks | covered | required | covered | Risks capture KPI/analytics technical/privacy risk. |
| SC-REQ-ANALYTICS-008 | open-questions | missing | required | missing | Open questions omit KPI/analytics implementation/privacy. |
| SC-REQ-SCALABILITY-009 | architecture | missing | required | missing | Architecture does not address 200-20k/load assumptions. |
| SC-REQ-SCALABILITY-009 | requirements | covered | required | covered | Requirements include 200-20k scalability and performance NFR. |
| SC-REQ-SCALABILITY-009 | risks | covered | required | covered | Risks include uncertainty over 200-20k and dimensioning. |
| SC-REQ-SCALABILITY-009 | open-questions | missing | required | missing | Open questions omit final user/load assumption. |
| SC-REQ-PUSH-010 | architecture | not_applicable | not_applicable | not_applicable | Push scope status is not architecture unless implementation is selected. |
| SC-REQ-PUSH-010 | requirements | missing | required | missing | Requirements should mention Push as deferred/excluded/open; absent. |
| SC-REQ-PUSH-010 | risks | partial | required | partial | Risks mention consent generally, but not Push-specific later-scope risk. |
| SC-REQ-PUSH-010 | open-questions | partial | required | missing | Open questions omit Push status/scope/consent; LLM partial too lenient. |
| SC-RISK-API-GATEWAY-011 | architecture | partial | required | covered | Architecture mentions API Gateway delay risk and alternatives/open strategy. |
| SC-RISK-API-GATEWAY-011 | requirements | covered | required | covered | Requirements include waitlist and alternatives to check. |
| SC-RISK-API-GATEWAY-011 | risks | covered | required | covered | Risk artifact explicitly covers Gateway waitlist threatening MVP. |
| SC-RISK-API-GATEWAY-011 | open-questions | partial | required | covered | Open questions cover handling Gateway delay and alternatives. |
| SC-RISK-PUSH-012 | architecture | partial | optional | missing | If Push is not architecture scope, missing is acceptable; if later feature considered, risk is absent. |
| SC-RISK-PUSH-012 | requirements | partial | required | missing | Push-specific consent/later-scope risk absent from requirements. |
| SC-RISK-PUSH-012 | risks | partial | required | partial | Generic consent/DSGVO present, Push-specific risk missing. |
| SC-RISK-PUSH-012 | open-questions | partial | required | missing | Push consent/scope open question absent. |
| SC-RISK-SUPPORT-013 | architecture | partial | optional | partial | Support backend/process mentioned; unstructured email risk not fully architectural. |
| SC-RISK-SUPPORT-013 | requirements | partial | required | covered | Requirements/constraints describe contact form/no ticket persistence/unstructured email exception. |
| SC-RISK-SUPPORT-013 | risks | covered | required | covered | Risk artifact explicitly covers no ticketsystem/unstructured email/compliance. |
| SC-RISK-SUPPORT-013 | open-questions | covered | required | covered | Open questions cover support without ticketsystem and privacy-compliant handling. |
| SC-RISK-TESTDATA-014 | architecture | partial | required | partial | Architecture mentions test data strategy but lacks full “no real customer data”/secrets/environments. |
| SC-RISK-TESTDATA-014 | requirements | partial | required | partial | Requirements mention test data not fully clarified, but not explicit no-real-customer-data rule. |
| SC-RISK-TESTDATA-014 | risks | covered | required | covered | Risk artifact explicitly covers real customer data in tests/dev as privacy risk. |
| SC-RISK-TESTDATA-014 | open-questions | covered | required | covered | Open questions cover personal data in dev/test and anonymization/replacement data. |
| SC-RISK-PRICE-CACHE-015 | architecture | partial | required | partial | Architecture mentions price/rabatt fluctuations and caching, but privacy/retention/invalidierung incomplete. |
| SC-RISK-PRICE-CACHE-015 | requirements | partial | required | partial | Requirements mention cache mechanisms to evaluate, but privacy/update conflict incomplete. |
| SC-RISK-PRICE-CACHE-015 | risks | covered | required | covered | Risk artifact captures cache vs privacy/data freshness. |
| SC-RISK-PRICE-CACHE-015 | open-questions | partial | required | partial | Open questions cover price/rabatt updates, but cache/fallback/privacy incomplete. |
| SC-OQ-MOBILE-016 | architecture | covered | required | covered | Architecture lists native vs responsive as open decision. |
| SC-OQ-MOBILE-016 | requirements | covered | required | covered | Requirements assumptions include native vs responsive open. |
| SC-OQ-MOBILE-016 | risks | covered | required | covered | Risks cover native vs responsive/budget tradeoff. |
| SC-OQ-MOBILE-016 | open-questions | missing | required | missing | Open-questions artifact lacks mobile/native-vs-responsive; original fixture expected covered was wrong. |
| SC-OQ-PUSH-017 | architecture | not_applicable | not_applicable | not_applicable | Push not an architecture obligation without implementation decision. |
| SC-OQ-PUSH-017 | requirements | missing | required | missing | Requirements omit Push status/deferment/consent. |
| SC-OQ-PUSH-017 | risks | partial | required | partial | Risks only generic consent/DSGVO, no Push-specific open scope. |
| SC-OQ-PUSH-017 | open-questions | missing | required | missing | Open questions omit Push as scope/privacy question. |
| SC-OQ-EU-RESIDENCE-018 | architecture | partial | required | partial | Architecture says EU/DSGVO Managed Cloud, but EU-only vs DSGVO distinction/global replication incomplete. |
| SC-OQ-EU-RESIDENCE-018 | requirements | partial | required | partial | Requirements say EU or at least DSGVO; distinction remains unresolved. |
| SC-OQ-EU-RESIDENCE-018 | risks | partial | required | partial | Risks mention EU-only/hosting costs, but distinction/global replication incomplete. |
| SC-OQ-EU-RESIDENCE-018 | open-questions | covered | required | covered | Open questions explicitly cover EU-only/DSGVO/Managed Services. |
| SC-OQ-PRICE-CACHE-019 | architecture | covered | required | partial | Architecture mentions price/rabatt fluctuations/caching, but fallback/cache invalidation detail incomplete. |
| SC-OQ-PRICE-CACHE-019 | requirements | covered | required | partial | Requirements mention cache mechanisms to evaluate, but not full fallback/invalidierung. |
| SC-OQ-PRICE-CACHE-019 | risks | partial | required | partial | Risks cover cache vs privacy/freshness, but open handling detail incomplete. |
| SC-OQ-PRICE-CACHE-019 | open-questions | partial | required | partial | Open questions mention price/rabatt updates, but fallback/cache invalidation missing; fixture covered was too lenient. |
| SC-OQ-ANALYTICS-020 | architecture | partial | required | missing | Architecture lacks analytics/KPI tracking implementation/privacy question. |
| SC-OQ-ANALYTICS-020 | requirements | partial | required | covered | Requirements include KPI measurements and technical implementation outstanding. |
| SC-OQ-ANALYTICS-020 | risks | covered | required | covered | Risks cover KPIs/analytics technical/privacy risk. |
| SC-OQ-ANALYTICS-020 | open-questions | missing | required | missing | Open questions omit KPI/analytics implementation/privacy. |

## Adjudicated Reading

Die Matrix ist nach Adjudication aussagekraeftiger als die reine LLM-Ausgabe:

```text
1. `partial` war im LLM-Run tatsaechlich ueberverwendet.
2. Viele Cross-Artefact-Faelle bleiben aber echte Pflichten, nicht nur Rauschen.
3. `not_applicable` ist selten, aber nicht so extrem selten wie im LLM-Rohurteil:
   mehrere Zellen sind besser als context/not_applicable statt partial zu behandeln.
4. Einige urspruengliche Fixture-Labels waren zu lenient:
   insbesondere Mobile/OpenQuestions und Price-Cache/OpenQuestions.
```

## Naechste Auswertung

Diese Datei kann jetzt als lokale Gold-/Reference-Matrix fuer den 20-Case-Pruefstand dienen.

Sinnvolle Folgeauswertung:

```text
1. Applicability-Accuracy des Matrix-Judges berechnen.
2. Coverage-Accuracy nur fuer applicable cells berechnen.
3. Partial-overuse gegen diese adjudizierte Matrix messen.
4. Daraus schaetzen, wie viel Noise eine 98x4-Matrix erzeugen wuerde.
```
