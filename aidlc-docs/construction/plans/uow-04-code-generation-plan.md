# UOW-04 Code Generation Plan

## Unit Context
- **Unit**: UOW-04 Integration Readiness and Quality Gate
- **Stories implemented directly**: Integration ownership across US-01..US-12 (traceability and readiness coverage)
- **Dependencies**:
  - UOW-01 Foundation contracts, primitives, and persistence baseline
  - UOW-02 Core Security API/error-contract surfaces
  - UOW-03 Governance API/error-contract surfaces and shared observability conventions
  - Shared internal Docker network, volumes, and secret-mount conventions
- **Expected interfaces and contracts**:
  - Deterministic integration gate pass/fail contract
  - RFC7807 + `errorCode` + `correlationId` conformance-check contract
  - Story-to-file traceability completeness contract (US-01..US-12)
  - Blocking-finding fail-closed contract (no bypass)
  - Gate evidence append-only record contract (90-day retention metadata)

## Story Traceability
- **US-01..US-06**: Verify UOW-02 surfaces are integration-ready and contract-consistent.
- **US-07..US-12**: Verify UOW-03 surfaces are integration-ready and contract-consistent.
- **Cross-unit coverage**: Ensure complete story-to-file mapping and unresolved-item blocking behavior before Build and Test.

---

## Detailed Execution Steps (Single Source of Truth)

### Step 1: Project and solution wiring updates
- [x] Add UOW-04 project structure under workspace root:
  - `src/AuthModule/Integration/`
  - `tests/AuthModule.Integration.Tests/`
- [x] Add project references and solution entries in `AuthModule.slnx`.

### Step 2: Domain models and value objects generation
- [x] Implement UOW-04 entities/value objects in `src/AuthModule/Integration/Domain/`:
  - `IntegrationReadinessRecord`
  - `ContractConformanceFinding`
  - `TraceabilityCoverageEntry`
  - `BlockingFindingRecord`
  - `GateDecisionRecord` (+ retention-expiry metadata).

### Step 3: Service contracts and module interfaces generation
- [x] Define application service interfaces in `src/AuthModule/Integration/Application/Contracts/` for:
  - gate execution,
  - contract conformance checking,
  - traceability checking,
  - runtime artifact checking,
  - blocker registry persistence,
  - gate evidence persistence,
  - gate alert emission.

### Step 4: Core integration-gate orchestration generation
- [x] Implement gate orchestration in `src/AuthModule/Integration/Application/Gate/`:
  - canonical evaluation order,
  - deterministic pass/fail logic for unchanged inputs,
  - early-fail on first blocking finding,
  - aggregated decision and note output.

### Step 5: Contract conformance checker generation
- [x] Implement RFC7807 conformance checks in `src/AuthModule/Integration/Application/Conformance/`:
  - validate `ProblemDetails` shape,
  - assert presence of `errorCode` and `correlationId`,
  - generate explicit finding records per endpoint.

### Step 6: Story traceability checker generation
- [x] Implement story-to-file coverage checks in `src/AuthModule/Integration/Application/Traceability/`:
  - enforce US-01..US-12 coverage,
  - detect missing/invalid mappings,
  - emit blocking findings for uncovered stories.

### Step 7: Runtime artifact checker generation
- [x] Implement required artifact checks in `src/AuthModule/Integration/Application/Runtime/`:
  - verify `docker-compose.yml` exists,
  - verify `config/policy.template.json` exists,
  - produce explicit blocker records for missing artifacts.

### Step 8: Persistence adapters and registry/evidence integration generation
- [x] Implement UOW-04 persistence adapters in `src/AuthModule/Integration/Persistence/`:
  - append-only gate evidence storage,
  - unresolved-blocker registry storage,
  - JSON namespace isolation for integration records.

### Step 9: Alerting and observability hooks generation
- [x] Implement gate-failure alert emission in `src/AuthModule/Integration/Application/Alerts/`:
  - structured operational/security events,
  - correlation-aware log payloads,
  - no direct webhook/queue transport in this phase.

### Step 10: API endpoints and error-mapping generation
- [x] Implement internal API endpoints in `src/AuthModule/Integration/Api/`:
  - trigger gate execution,
  - query latest gate decision/evidence summary,
  - return standardized `ProblemDetails` responses with correlation ID.

### Step 11: Composition root and runtime configuration generation
- [x] Implement DI + configuration in `src/AuthModule/Integration/Bootstrap/` and `Configuration/`:
  - wire gate services, checkers, and persistence adapters,
  - bind gate timeout/retention settings,
  - enforce startup validation.

### Step 12: Unit and behavior tests generation
- [x] Implement test coverage in `tests/AuthModule.Integration.Tests/` for:
  - deterministic gate outcome for unchanged state,
  - fail-fast blocking behavior and no bypass,
  - conformance finding generation for missing `errorCode`/`correlationId`,
  - runtime artifact-missing blocker behavior,
  - story traceability completeness enforcement.

### Step 13: Property-based testing generation
- [x] Add PBT alignment tests in `tests/AuthModule.Integration.Tests/PropertyBased/`:
  - verify deterministic checklist evaluation order invariants only if needed by implementation;
  - otherwise assert reuse of prior unit-level PBT evidence without new gate-level generators.

### Step 14: Deployment and runtime artifact updates
- [x] Update runtime artifacts in workspace root:
  - extend `docker-compose.yml` for integration gate service wiring (internal-only),
  - add/update config template entries for gate settings (timeouts, retention days),
  - keep preview-runtime governance alignment with prior units.

### Step 15: Documentation updates for generated code
- [x] Generate code summary docs in `aidlc-docs/construction/uow-04-integration-readiness/code/`:
  - file map (created/modified),
  - cross-unit story-to-code traceability,
  - test map and extension compliance notes.

### Step 16: End-to-end generation readiness and handoff
- [x] Confirm all steps are executable in sequence and complete UOW-04 handoff to Build and Test.
- [x] Record unresolved items (if any) in code summary documentation.

---

## Execution Notes
- Application code must be generated only in workspace root (never in `aidlc-docs/`).
- Documentation artifacts are markdown-only in `aidlc-docs/construction/uow-04-integration-readiness/code/`.
- During implementation, each completed step must be marked `[x]` immediately.
- This plan is the **single source of truth** for UOW-04 code generation.
