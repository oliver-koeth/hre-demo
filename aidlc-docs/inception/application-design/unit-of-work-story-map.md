# Unit of Work Story Map

## Story-to-Unit Mapping
| Story | Domain | Assigned Unit | Notes |
|---|---|---|---|
| US-01 Credential Authentication | D-01 Authentication | UOW-02 | Core auth flow |
| US-02 Server-Side Token Validation | D-01 Authentication | UOW-02 | Token validation control plane |
| US-03 Privileged Session Controls | D-01 Authentication | UOW-02 | Privileged security controls |
| US-04 User Lifecycle Administration | D-02 Authorization/Admin | UOW-02 | Admin lifecycle operations |
| US-05 Role and Permission Catalog Management | D-02 Authorization/Admin | UOW-02 | RBAC+permission governance |
| US-06 Authorization Enforcement on Protected Endpoints | D-02 Authorization/Admin | UOW-02 | Deny-by-default enforcement |
| US-07 Security Audit Trail Capture | D-03 Audit/Evidence | UOW-03 | Audit capture and retention |
| US-08 Access and Change Evidence Retrieval | D-03 Audit/Evidence | UOW-03 | Evidence export/control mapping |
| US-09 Controlled Personal-Data Operations | D-04 Privacy/Governance | UOW-03 | Data-subject operations |
| US-10 Retention-Driven Deletion and Anonymization | D-04 Privacy/Governance | UOW-03 | Retention/legal-hold workflows |
| US-11 Incident Evidence and Classification Support | D-05 Incident/Continuity | UOW-03 | Incident evidence and classification |
| US-12 Backup, Recovery, and Continuity Readiness | D-05 Incident/Continuity | UOW-03 | Continuity evidence surfaces |

## Foundational Coverage
UOW-01 provides shared foundations used by all mapped stories:
- Shared contracts and data models.
- Domain persistence boundaries.
- Cross-cutting security/policy interfaces.

## Integration and Handoff Coverage
UOW-04 provides story-spanning integration readiness:
- End-to-end traceability verification for US-01..US-12.
- Cross-unit interface consistency and quality-gate ownership.

## Coverage Validation
- All 12 stories are assigned to implementation units.
- No story is orphaned.
- Foundation and integration units provide supporting scope without replacing direct story ownership.
