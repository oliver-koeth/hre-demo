# Unit of Work Definitions

## Decomposition Strategy
- **Grouping**: Business-domain aligned units.
- **Granularity**: Fewer, broader units (4 units).
- **Execution model**: Strictly sequential.
- **Primary dependency driver**: Data model and persistence readiness first.

## UOW-01 Foundation and Persistence Baseline
- **Purpose**: Establish the technical and data foundation required by all downstream units.
- **Scope**:
  - Solution/project skeleton (single service project).
  - Core contracts and shared primitives (IDs, result types, error contracts, correlation context).
  - Domain-aligned persistence modules/stores (`AuthStore`, `AuthzStore`, `AuditStore`, `GovernanceStore`).
  - JSON schema/versioning and integrity boundary conventions.
  - Security infrastructure baseline hooks (request context, validation, transport policy integration points).
- **Primary Deliverables**:
  - Foundational module structure and interfaces.
  - Persistence repository interfaces and baseline models.
  - Policy/configuration service contracts.

## UOW-02 Core Auth and Authorization Control Plane
- **Purpose**: Deliver the security-critical runtime core for authentication and authorization.
- **Scope**:
  - Auth API and Auth Domain core behavior.
  - Authorization domain and policy enforcement.
  - Privileged change approval workflow (SoD state machine).
  - Admin API core management paths for users/roles/permissions/assignments.
- **Primary Deliverables**:
  - End-to-end auth and token lifecycle path.
  - End-to-end authorization decisions (deny-by-default).
  - Governed role/permission change workflow.

## UOW-03 Governance and Evidence Domains
- **Purpose**: Deliver compliance-facing capabilities and separate governance documentation hooks.
- **Scope**:
  - Audit and evidence services (synchronous critical capture, export contracts).
  - Privacy governance operations (data-subject operations, lawful-basis and purpose tags, processing metadata export).
  - Incident and continuity evidence model (incident classification, backup/restore evidence capture surfaces).
  - **Separate governance documentation stream** for non-product constraints (third-party risk process, formal change process, resilience-testing process).
- **Primary Deliverables**:
  - Evidence and audit export pathways.
  - Privacy/governance operational surfaces.
  - Governance-constraint documentation package references.

## UOW-04 Integration Readiness and Quality Gate
- **Purpose**: Own the final INCEPTION integration-readiness handoff before CONSTRUCTION.
- **Scope**:
  - Validate cross-unit integration contracts and dependency consistency.
  - Consolidate interface definitions and design assumptions.
  - Define quality-gate checklist for entry into Functional Design/NFR stages.
  - Confirm all stories are covered and traceable.
- **Primary Deliverables**:
  - Integration-readiness checklist and unresolved-decision register (if any).
  - Finalized unit traceability and dependency validation report.

## Greenfield Code Organization Strategy
Selected strategy: **Single service project with internal module folders per unit**.

Proposed structure:
- `src/AuthModule/Foundation/` (UOW-01)
- `src/AuthModule/CoreSecurity/` (UOW-02)
- `src/AuthModule/Governance/` (UOW-03)
- `src/AuthModule/Integration/` (UOW-04 integration contracts/checklists)

## Unit Boundary Validation Summary
- Each unit has distinct responsibility and handoff outputs.
- Downstream unit start criteria depend on prior unit outputs only.
- Non-product governance constraints are isolated in UOW-03 documentation stream while runtime hooks remain in scope.
