# Unit of Work Dependency Matrix

## Execution Order
1. UOW-01 Foundation and Persistence Baseline
2. UOW-02 Core Auth and Authorization Control Plane
3. UOW-03 Governance and Evidence Domains
4. UOW-04 Integration Readiness and Quality Gate

## Dependency Matrix
| Unit | Depends On | Dependency Type | Rationale |
|---|---|---|---|
| UOW-01 | None | Root | Foundational baseline |
| UOW-02 | UOW-01 | Hard | Requires shared contracts, persistence boundaries, and policy/config service interfaces |
| UOW-03 | UOW-01, UOW-02 | Hard | Requires persistence baseline and core security/admin event surfaces for evidence/governance flows |
| UOW-04 | UOW-01, UOW-02, UOW-03 | Hard | Final integration and quality-gate ownership spans all prior units |

## Dependency Characteristics
- **Parallelization**: Disabled by decision (strictly sequential path).
- **Critical Path**: UOW-01 → UOW-02 → UOW-03 → UOW-04.
- **Risk Concentration**:
  - UOW-01 carries schema/interface foundation risk.
  - UOW-02 carries primary security-control risk.
  - UOW-03 carries compliance-evidence traceability risk.
  - UOW-04 carries integration-handoff risk.

## Handoff Contracts
- UOW-01 → UOW-02:
  - Stable domain model and persistence interfaces.
  - Policy/configuration contract stubs.
- UOW-02 → UOW-03:
  - Auth/admin event surfaces and approval workflow outputs.
  - Authorization decision model.
- UOW-03 → UOW-04:
  - Evidence export contracts and governance metadata model.
  - Incident/continuity evidence interfaces.
