# Requirements

## Functional Requirements
1. **Kundenportal** - A web-based customer portal that enables users to view orders, download invoices, and receive push notifications.
2. **Angebote Generieren** - The system must generate offers for customers based on product data available in SAP ERP and price rules.
3. **Login & SSO Options** - Users should be able to log into the platform via email/password combinations or Single Sign-On (SSO) using Azure AD, Google, etc., as per customer preference.
4. **SAP Integration** - The system must integrate with SAP for accessing product data and performing updates if necessary.
5. **Logging & Audit Trails** - Comprehensive logging of user activities and modifications to ensure compliance with DSGVO requirements.

## Non-functional Requirements
1. **Security** - Ensure end-to-end encryption, TLS for secure communication between components, and proper handling of person-related data according to GDPR guidelines.
2. **Scalability** - The platform should handle varying loads ranging from 750 to possibly up to 20,000 users depending on demand; solutions must avoid over-engineering while ensuring scalability.
3. **Performance & Reliability** - Optimize system performance and ensure high reliability with disaster recovery plans in place for critical data (KPI tracking).
4. **Compliance & Legalities** - Adhere strictly to the DSGVO, implement necessary mechanisms like Double-Opt-In processes, access control models, API security measures via OAuth or API keys, and audit trails.
5. **API Layer** - Develop an intermediary layer allowing for easier integration with external systems.

## Constraints/Compliance
1. **MVP Deadline** - The Minimum Viable Product must be ready within 8 weeks from now.
2. **No New DB Server** - No new database server installations are allowed; look into Managed Services instead.
3. **Managed Services Preference** - Opt for affordable yet scalable managed services over self-hosting infrastructure whenever possible.
4. **EU Data Hosting** - All data must be hosted within the EU or comply with DSGVO rules otherwise.