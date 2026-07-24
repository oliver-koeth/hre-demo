# Components

## C-01 Auth API Component
- **Purpose**: Expose authentication and token-validation endpoints for users and internal services.
- **Responsibilities**:
  - Receive credential and token-validation requests.
  - Apply request validation and call application services.
  - Return normalized success/error responses and correlation IDs.
- **Primary Interfaces**:
  - `POST /auth/login`
  - `POST /auth/validate`

## C-02 Admin API Component
- **Purpose**: Expose privileged management endpoints for users, roles, permissions, and assignments.
- **Responsibilities**:
  - Enforce admin authentication context.
  - Route admin commands to authorization and governance services.
  - Expose separated endpoints for role/permission governance actions.
- **Primary Interfaces**:
  - `/admin/users/*`
  - `/admin/roles/*`
  - `/admin/permissions/*`
  - `/admin/role-assignments/*`

## C-03 Auth Domain Component
- **Purpose**: Implement core authentication, credential, and token lifecycle behavior.
- **Responsibilities**:
  - Credential verification and account status checks.
  - JWT issuance and server-side validation hooks.
  - Lockout and brute-force protection policy evaluation.
- **Notes**: HMAC signing with mounted secret files is the selected initial strategy.

## C-04 Authorization Domain Component
- **Purpose**: Implement hybrid RBAC + permission model and enforcement logic.
- **Responsibilities**:
  - Resolve effective permissions from roles and assignments.
  - Enforce deny-by-default policy decisions.
  - Apply role/permission policy checks to protected operations.

## C-05 Approval Workflow Component
- **Purpose**: Handle separation-of-duties for high-impact role/permission changes.
- **Responsibilities**:
  - Maintain approval-state machine for governed admin mutations.
  - Validate two-person approval requirements where policy applies.
  - Publish approval context for audit evidence.

## C-06 Audit and Evidence Component
- **Purpose**: Capture tamper-evident security and governance evidence.
- **Responsibilities**:
  - Synchronously record critical authentication/admin events.
  - Store actor/action/result/correlation context.
  - Provide machine-readable evidence exports for compliance review.

## C-07 Privacy Governance Component
- **Purpose**: Provide data-subject and processing-governance operations.
- **Responsibilities**:
  - Handle retrieval/correction/restriction/export/erasure workflows.
  - Tag processing purpose and lawful-basis references.
  - Provide RoPA-supporting metadata exports.

## C-08 Incident and Continuity Component
- **Purpose**: Record incident evidence and continuity readiness signals.
- **Responsibilities**:
  - Classify incident metadata (severity, service impact, data impact).
  - Record backup/restore/reconciliation evidence references.
  - Support breach and crisis-evidence workflows.

## C-09 Persistence Gateway Components
- **Purpose**: Isolate JSON persistence per domain area.
- **Responsibilities**:
  - Domain-specific stores: `AuthStore`, `AuthzStore`, `AuditStore`, `GovernanceStore`.
  - Enforce schema/version boundaries and atomic write strategy per store.
  - Abstract file access and integrity controls from domain logic.

## C-10 Security Infrastructure Component
- **Purpose**: Provide cross-cutting security infrastructure for API layer.
- **Responsibilities**:
  - TLS enforcement and transport safeguards.
  - Request identity/context extraction.
  - Input validation and error handling policy integration.
