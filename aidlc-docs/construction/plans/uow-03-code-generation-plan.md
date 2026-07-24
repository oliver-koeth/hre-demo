# UOW-03 Code Generation Plan

## Unit Context
- **Unit**: UOW-03 Governance and Evidence Domains
- **Stories implemented directly**: US-07, US-08, US-09, US-10, US-11, US-12
- **Dependencies**:
  - UOW-01 Foundation persistence contracts, primitives, and observability baseline
  - UOW-02 CoreSecurity event and correlation conventions
  - Shared internal Docker network and secret-mount conventions
- **Expected interfaces and contracts**:
  - JSON evidence export contract with manifest metadata
  - Legal-hold fail-closed policy guard contract
  - Retention invocation idempotence and lifecycle decision recording contract
  - Incident durable-write semantics before success response

## Story Traceability
- **US-07** Security audit trail capture/query (security events only in V1).
- **US-08** Evidence capture and machine-readable export with control-mapping IDs.
- **US-09** Data-subject retrieval/export workflows with hold checks.
- **US-10** Manual retention lifecycle invocation with idempotent outcomes.
- **US-11** Incident classification and lifecycle progression records.
- **US-12** Backup metadata evidence capture and query freshness handling.

---

## Detailed Execution Steps (Single Source of Truth)

### Step 1: Project and solution wiring updates
- [x] Add UOW-03 project structure under workspace root:
  - `src/AuthModule/Governance/`
  - `tests/AuthModule.Governance.Tests/`
- [x] Add project references and solution entries in `AuthModule.slnx`.

### Step 2: Domain models and value objects generation
- [x] Implement UOW-03 entities/value objects in `src/AuthModule/Governance/Domain/`:
  - `EvidenceRecord`, `DataSubjectRequest`, `RetentionRule`, `LifecycleDecisionRecord`
  - `IncidentRecord`, `BackupMetadataRecord`, RoPA `ProcessingActivity`
  - retention fingerprint and manifest value objects.

### Step 3: Service contracts and module interfaces generation
- [x] Define application service interfaces in `src/AuthModule/Governance/Application/Contracts/` for:
  - audit querying,
  - evidence capture/export,
  - data-subject retrieval/export,
  - retention invocation,
  - incident lifecycle handling,
  - backup evidence capture,
  - legal-hold policy guard and alert emission.

### Step 4: Audit query and evidence services generation
- [x] Implement audit/evidence flows in `src/AuthModule/Governance/Application/AuditEvidence/`:
  - security-event-only query surface with window filters and bounded paging,
  - immutable evidence persistence,
  - streaming JSON export with manifest metadata.

### Step 5: Data-subject operations and legal-hold guard generation
- [x] Implement data-subject retrieval/export in `src/AuthModule/Governance/Application/DataSubject/`:
  - Retrieve/Export request lifecycle,
  - centralized legal-hold guard fail-closed checks,
  - reason-coded blocked outcomes.

### Step 6: Retention lifecycle and idempotence logic generation
- [x] Implement retention flows in `src/AuthModule/Governance/Application/Retention/`:
  - manual API-triggered invocation,
  - deterministic decision function,
  - persisted idempotence fingerprint behavior,
  - lifecycle outcome recording.

### Step 7: Incident and backup evidence logic generation
- [x] Implement incident/backup logic in `src/AuthModule/Governance/Application/Operations/`:
  - incident status progression rules and durable write boundary,
  - backup metadata append/update progression,
  - freshness-oriented read behavior hooks.

### Step 8: Persistence adapters and repository integration generation
- [x] Implement UOW-03 persistence adapters in `src/AuthModule/Governance/Persistence/`:
  - mappings to UOW-01 repositories and governance namespace paths,
  - legal-hold metadata persistence,
  - retention fingerprint storage,
  - query helpers for export and audit filters.

### Step 9: API endpoints and error-mapping generation
- [x] Implement APIs in `src/AuthModule/Governance/Api/`:
  - audit query endpoints,
  - evidence capture/export endpoints,
  - data-subject retrieve/export endpoints,
  - retention invocation endpoint,
  - incident and backup evidence endpoints,
  - standardized ProblemDetails responses with correlation ID.

### Step 10: Composition root and runtime configuration generation
- [x] Implement DI + configuration in `src/AuthModule/Governance/Bootstrap/` and `Configuration/`:
  - wire module services and guards,
  - bind retention, paging, export, and durability policy settings,
  - enforce startup validation.

### Step 11: Unit and behavior tests generation
- [x] Implement test coverage in `tests/AuthModule.Governance.Tests/` for:
  - legal-hold fail-closed blocks,
  - retention lifecycle outcomes and no-partial-success behavior,
  - incident lifecycle transitions and durability gates,
  - audit query filtering/pagination constraints.

### Step 12: Property-based testing (PBT) generation
- [x] Add FsCheck tests in `tests/AuthModule.Governance.Tests/PropertyBased/` for:
  - retention decision idempotence on unchanged entity/rule/input state.

### Step 13: Deployment and runtime artifact updates
- [x] Update runtime artifacts in workspace root:
  - extend `docker-compose.yml` for Governance service wiring (internal-only),
  - add/update config templates for UOW-03 policies,
  - keep preview-runtime governance alignment with prior units.

### Step 14: Documentation updates for generated code
- [x] Generate code summary docs in `aidlc-docs/construction/uow-03-governance/code/`:
  - file map (created/modified),
  - story-to-code traceability,
  - test map and extension compliance notes.

### Step 15: End-to-end generation readiness checkpoint
- [x] Confirm all plan steps are executable in sequence and ready for implementation pass.
- [x] Keep this plan as the single source of truth for UOW-03 code generation execution.

### Step 16: Transition readiness for next unit/build phase
- [x] Confirm dependencies exported for UOW-04 or Build & Test stage handoff.
- [x] Record unresolved items (if any) in code summary documentation.

---

## Execution Notes
- Application code must be generated only in workspace root (never in `aidlc-docs/`).
- Documentation artifacts are markdown-only in `aidlc-docs/construction/uow-03-governance/code/`.
- During implementation, each completed step must be marked `[x]` immediately.
- This plan is the **single source of truth** for UOW-03 code generation.
