# Application Design Overview

## Selected Architecture and Technology Decisions
- **Architecture style**: Modular monolith.
- **API framework**: Hybrid ASP.NET Core approach (Minimal API for auth endpoints; controllers for admin/governance).
- **Persistence boundary strategy**: Dedicated domain persistence modules (`AuthStore`, `AuthzStore`, `AuditStore`, `GovernanceStore`).
- **Token key strategy**: Symmetric HMAC signing using mounted secret files with rotation planning.
- **Audit/evidence integration**: Synchronous in-request capture for critical operations.
- **Privileged governance orchestration**: Single workflow service with approval-state machine.
- **Privacy organization**: Dedicated privacy-governance component.
- **Non-product governance constraints**: Maintained external to runtime components, with minimum trace hooks emitted by application services.

## Component Set
See:
- `components.md`
- `component-methods.md`

## Service Orchestration
See:
- `services.md`

## Dependency and Communication Design
See:
- `component-dependency.md`

## Design Completeness Check
- Core capabilities from requirements and user stories are represented.
- Explicit coverage exists for auth, authorization, SoD governance, audit evidence, privacy operations, and incident/continuity evidence.
- Component and service boundaries are defined at high level without entering detailed business-rule implementation.
- Detailed algorithmic and rule-level behavior is intentionally deferred to Functional Design during CONSTRUCTION.
