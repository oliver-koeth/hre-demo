# UOW-01 Code Generation Plan

## Unit Context
- **Unit**: UOW-01 Foundation and Persistence Baseline
- **Stories implemented directly**: None (foundational unit enabling US-01..US-12)
- **Dependencies**: None (root unit)
- **Interfaces expected by downstream units**:
  - Shared primitives (`RequestContext`, `Result<TValue, TError>`, `DomainError`)
  - Repository contracts for AuthStore/AuthzStore/AuditStore/GovernanceStore
  - Policy configuration service
  - Integrity verification and encryption services

## Story Traceability
- Supports all downstream stories through foundational capabilities:
  - Persistence boundaries and schemas
  - Concurrency and integrity controls
  - Shared contracts and error handling
  - Security/observability baseline

---

## Detailed Execution Steps (Single Source of Truth)

### Step 1: Project structure setup (greenfield)
- [x] Create solution and project skeleton in workspace root:
  - `src/AuthModule/Foundation/`
  - `tests/AuthModule.Foundation.Tests/`
  - `config/`
  - `scripts/`

### Step 2: Shared primitive contracts generation
- [x] Implement domain primitives in `src/AuthModule/Foundation/Domain/Primitives/`:
  - `RequestContext`
  - `Result<TValue, TError>`
  - `DomainError` and error code enum
  - Base entity/version contracts

### Step 3: Domain entity models generation
- [x] Implement UOW-01 entities in `src/AuthModule/Foundation/Domain/Entities/` based on approved design:
  - `User`, `Credential`
  - `Role`, `Permission`, `RolePermissionAssignment`, `UserRoleAssignment`
  - `SecurityAuditEvent`, `AdminChangeEvent`
  - `ProcessingActivity`, `IncidentRecord`, `BackupMetadata`

### Step 4: Serialization contracts and schema headers
- [x] Implement store file envelope/header models and serialization options in `src/AuthModule/Foundation/Persistence/Serialization/`:
  - `StoreFileHeader`
  - record wrappers per store file
  - explicit `System.Text.Json` options

### Step 5: Policy configuration service generation
- [x] Implement `PolicyConfiguration` and `IPolicyConfigurationService` in `src/AuthModule/Foundation/Configuration/` with startup validation rules from NFRs.

### Step 6: Encryption and integrity services generation
- [x] Implement crypto services in `src/AuthModule/Foundation/Security/`:
  - `EncryptionService` (AES-256-GCM)
  - `IntegrityService` (HMAC-SHA256)
  - `KeyProvider` (mounted secret file loading)

### Step 7: Persistence gateway and repository layer generation
- [x] Implement repository interfaces and JSON file repositories in `src/AuthModule/Foundation/Persistence/`:
  - `IStoreRepository<T>`
  - `IAuditStoreRepository`
  - `IStoreIntegrityService`
  - per-store repositories (Auth/Authz/Audit/Governance)
  - optimistic concurrency handling and soft-delete behavior

### Step 8: Write coordination and recovery orchestration generation
- [x] Implement runtime coordination components in `src/AuthModule/Foundation/Runtime/`:
  - per-store write coordinator queue
  - retry policy for transient I/O
  - quarantine and restore orchestrator hooks

### Step 9: Observability baseline generation
- [x] Implement structured logging + telemetry wiring in `src/AuthModule/Foundation/Observability/`:
  - Serilog setup with correlation enrichment
  - OpenTelemetry metrics/traces scaffolding
  - mandatory security event logging points

### Step 10: Dependency injection and composition root
- [x] Implement DI registration and startup composition in `src/AuthModule/Foundation/Bootstrap/` to wire all services and repositories.

### Step 11: Business logic unit testing
- [x] Create tests in `tests/AuthModule.Foundation.Tests/` for:
  - entity and validation invariants
  - optimistic concurrency conflicts
  - soft-delete behavior
  - schema/version handling

### Step 12: Property-based testing (PBT) implementation
- [x] Add FsCheck-based tests in `tests/AuthModule.Foundation.Tests/PropertyBased/` for approved properties:
  - round-trip serialization/encryption/decryption
  - migration idempotency
  - integrity verification invariants
  - assignment validity invariants
  - conflict behavior under stale versions

### Step 13: Repository layer unit testing
- [x] Add repository tests for read/write/search/integrity flows and atomic write behavior.

### Step 14: API surface scaffolding for foundation diagnostics
- [x] Create minimal internal diagnostics endpoints (non-public) in `src/AuthModule/Foundation/Api/` for health and integrity checks needed by downstream units.

### Step 15: Configuration and deployment artifacts generation
- [x] Generate runtime/deployment artifacts in workspace root:
  - `Dockerfile` (pinned tag, non-latest)
  - `docker-compose.yml` (internal network, mounted secrets, mounted data volume)
  - base `config` templates for policy settings and paths

### Step 16: Documentation summary generation
- [x] Generate code-stage documentation summary in:
  - `aidlc-docs/construction/uow-01-foundation/code/`
  - include created file map, test map, and extension-rule trace notes.

---

## Execution Notes
- Application code is generated only in workspace root (never in `aidlc-docs/`).
- This plan is the **single source of truth** for UOW-01 code generation.
- During generation, each completed step must be marked `[x]` immediately.
