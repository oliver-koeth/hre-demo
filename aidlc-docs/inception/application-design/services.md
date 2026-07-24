# Services

## S-01 Authentication Service
- **Coordinates**: Auth API Component + Auth Domain Component + Audit and Evidence Component.
- **Orchestration Pattern**:
  1. Validate request and credential payload.
  2. Execute authentication and account status checks.
  3. Issue token and return response.
  4. Synchronously record authentication audit event.

## S-02 Authorization Service
- **Coordinates**: Admin API Component + Authorization Domain Component + Persistence Gateways.
- **Orchestration Pattern**:
  1. Resolve caller permissions.
  2. Evaluate operation policy (deny-by-default).
  3. Execute authorized mutation/read.
  4. Return decision and reason metadata.

## S-03 Privileged Change Governance Service
- **Coordinates**: Admin API Component + Approval Workflow Component + Authorization Domain Component + Audit and Evidence Component.
- **Orchestration Pattern**:
  1. Classify admin mutation as governed or non-governed.
  2. For governed changes, create approval ticket and pause execution until approved.
  3. Apply approved mutation.
  4. Capture approval context and before/after evidence.

## S-04 Evidence and Audit Service
- **Coordinates**: Audit and Evidence Component + Persistence Gateways.
- **Orchestration Pattern**:
  1. Capture event in request path for critical operations.
  2. Persist immutable/tamper-evident record.
  3. Support evidence retrieval/export by authorized reviewer persona.

## S-05 Privacy Governance Service
- **Coordinates**: Privacy Governance Component + GovernanceStore + Audit and Evidence Component.
- **Orchestration Pattern**:
  1. Validate rights-request context and policy eligibility.
  2. Execute data operation (retrieve/correct/restrict/export/erase).
  3. Tag outputs with purpose and lawful-basis metadata.
  4. Record governance action in evidence trail.

## S-06 Incident and Continuity Service
- **Coordinates**: Incident and Continuity Component + Audit and Evidence Component + GovernanceStore.
- **Orchestration Pattern**:
  1. Record incident/breach/continuity event metadata.
  2. Attach remediation and communication evidence.
  3. Record backup/restore/reconciliation artifacts.
  4. Expose reviewable continuity evidence bundles.

## S-07 Policy and Configuration Service
- **Coordinates**: Cross-component policy resolution.
- **Responsibilities**:
  - Centralize token policy, lockout thresholds, retention policy IDs, and governance flags.
  - Expose read-only policy snapshots for runtime enforcement.

## S-08 Governance Constraint Integration Notes
- Selected approach is to keep non-product governance obligations external to runtime components.
- Application services still emit minimum trace hooks needed for compliance evidence (IDs, timestamps, actor context), while vendor governance/change governance/resilience-testing procedures remain process-owned outside runtime orchestration.
