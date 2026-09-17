# Risks

## High Risks

- **SAP write access is not decided for the MVP.** Current direction is closer to SAP read-only usage in the MVP, and write scenarios such as online acceptance are not decided. This creates delivery and scope risk if downstream expectations assume SAP write capability too early.
- **Push notifications are not decided and are explicitly not part of the MVP.** They were raised as a marketing wish and may be considered later, so there is scope risk if they are assumed in MVP planning.
- **Data residency remains unresolved.** EU-only hosting and generally GDPR-compliant hosting are not the same, so hosting constraints and compliance expectations may diverge until data residency is clarified.
- **Discount approval above 15% requires workflow support.** Offers with discounts above 15% require approval, and workflow states are needed (`draft`, `pending approval`, `approved`, `sent`, `accepted`, `rejected`). If workflow support is incomplete or delayed, offer handling may be blocked or non-compliant with the business rule.

## Medium Risks

- **Expected user volume is uncertain.** The expected scale ranges from about 200 to 20,000 users, so performance and scalability need to be considered without relying on a fixed load assumption. This creates sizing and architecture risk for the MVP.
- **Analytics are desired, but implementation and compliance are open.** KPIs are wanted, while tracking approach, technical implementation, measurement concept, and privacy justification remain unresolved. This creates risk of delayed analytics delivery or rework.
- **Handling SAP price and discount changes is unresolved.** The approach for price freshness, discount changes, fallback behavior, and caching is still open, which creates correctness and integration risk.
- **Caching or fallback for SAP data conflicts with freshness and privacy constraints.** Cache invalidation is complex, and caching/fallback approaches may conflict with data freshness, retention, and privacy requirements.
- **No ticketing system in the MVP creates support and privacy risk.** If support is handled through unstructured email instead, this may lead to unstructured processing and data protection issues.

## Low Risks / Notable Constraints

- **MVP authentication is defined as email/password with mandatory double opt-in.** This is a decided constraint and currently appears low risk compared with the open items above.

## Risk Monitoring Focus

- Clarify whether MVP remains SAP read-only or whether any SAP write scenarios are needed.
- Prevent later-only scope items such as push notifications from being treated as MVP commitments.
- Clarify hosting/data residency expectations beyond general GDPR compliance.
- Validate scalability expectations early because the user range is still uncertain.
- Resolve analytics scope together with tracking and privacy constraints.
- Decide how SAP price freshness, caching, fallback, and retention/privacy constraints will be handled.