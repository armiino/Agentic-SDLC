# Requirements
## Functional Requirements
- Customer portal
- Offer creation and invoice download
- User roles (Admin, User, Manager, Support)
- Login via Email/Password, optional SSO (e.g., Azure AD or Google)
- SAP integration for product information, prices, discounts
- API layer for integration
## Non-functional Requirements
- GDPR compliance and data protection (Double Opt-In, Logging, deletion concepts)
- Security (TLS encrypted connections, OAuth for API access)
- Compliance (Audit trails, access control, role models)
- Scalability and performance
- Backups and disaster recovery
## Constraints/Compliance
- No new DB server, managed services considered
- GDPR compliance (EU-only hosting or at least GDPR-compliant solution)
- 8-week MVP timespan
- No overengineering
## Traceability
Each requirement is linked to relevant transcript sections.