# UOW-02 Code Generation Plan

## Unit Context
- **Unit**: UOW-02 Core Auth and Authorization Control Plane
- **Stories implemented directly**: US-01, US-02, US-03, US-04, US-05, US-06
- **Dependencies**:
  - UOW-01 Foundation repositories, primitives, policy configuration, and observability baseline
  - Shared internal Docker network and secret-mount conventions
- **Expected interfaces and contracts**:
  - RFC7807 `ProblemDetails` error contract with `errorCode` and `correlationId`
  - JWT validation contract (signature, expiry, issuer, audience, account status, token version)
  - SoD approval workflow contract for role-permission assign/revoke only

## Story Traceability
- **US-01 Credential Authentication**: email login, dual hash verification, lockout behavior, token issue.
- **US-02 Server-Side Token Validation**: strict token validation pipeline and fail-closed denies.
- **US-03 Privileged Session Controls**: per-operation MFA step-up verification.
- **US-04 User Lifecycle Administration**: create/update/disable user with immediate token-version revocation.
- **US-05 Role and Permission Catalog Management**: role/permission admin + governed assignment/revocation.
- **US-06 Authorization Enforcement**: exact `resource:action` permission evaluation with default-deny.

---

## Detailed Execution Steps (Single Source of Truth)

### Step 1: Project and solution wiring updates
- [x] Add UOW-02 project structure under workspace root:
  - `src/AuthModule/CoreSecurity/`
  - `tests/AuthModule.CoreSecurity.Tests/`
- [x] Add project references and solution entries in `AuthModule.slnx`.

### Step 2: Domain models and value objects generation
- [x] Implement UOW-02 entities/value objects in `src/AuthModule/CoreSecurity/Domain/`:
  - `AuthAttemptState`, `AuthSession`, `AccessTokenClaimsModel`
  - `AuthorizationRequestModel`, `AuthorizationDecisionModel`
  - `ApprovalTicket`, `StepUpChallenge`
  - permission key and login identity normalization helpers.

### Step 3: Service contracts and module interfaces generation
- [x] Define application service interfaces in `src/AuthModule/CoreSecurity/Application/Contracts/` for:
  - authentication,
  - token validation,
  - authorization evaluation,
  - approval workflow,
  - MFA verification,
  - security alert emission.

### Step 4: Authentication and lockout business logic generation
- [x] Implement login flow in `src/AuthModule/CoreSecurity/Application/Auth/`:
  - email normalization,
  - Argon2id/BCrypt verification compatibility,
  - failed-attempt increments and hard lockout,
  - success path resets and token issue handoff.

### Step 5: Token issuance and validation pipeline generation
- [x] Implement token services in `src/AuthModule/CoreSecurity/Application/Tokens/`:
  - token issuance policy for normal/privileged sessions,
  - strict validation checks,
  - short-TTL in-process cache for account status/token version.

### Step 6: Authorization and permission-resolution logic generation
- [x] Implement authorization evaluation in `src/AuthModule/CoreSecurity/Application/Authorization/`:
  - `${resource}:${action}` exact-match resolution,
  - role and role-permission join logic,
  - default-deny reason mapping.

### Step 7: Step-up MFA and approval workflow logic generation
- [x] Implement privileged-operation controls in `src/AuthModule/CoreSecurity/Application/Governance/`:
  - MFA challenge enforcement per sensitive operation,
  - governed role-permission assignment/revocation approval lifecycle,
  - non-self-approval and terminal-state invariants.

### Step 8: User lifecycle and revocation consistency generation
- [x] Implement user admin flows in `src/AuthModule/CoreSecurity/Application/Users/`:
  - create/update/disable,
  - disable + token-version increment strong-consistency sequence,
  - idempotent disable behavior.

### Step 9: Persistence adapters and repository integration generation
- [x] Implement UOW-02 persistence adapters in `src/AuthModule/CoreSecurity/Persistence/`:
  - mappings to UOW-01 repository contracts and store schemas,
  - approval persistence with bounded retries,
  - synchronous security-event persistence hooks.

### Step 10: API endpoints and error-mapping generation
- [x] Implement HTTP APIs in `src/AuthModule/CoreSecurity/Api/`:
  - auth/token/authorization/admin/governance endpoints,
  - standardized `ProblemDetails` responses,
  - correlation ID propagation and fail-closed status codes.

### Step 11: Composition root and runtime configuration generation
- [x] Implement DI + configuration in `src/AuthModule/CoreSecurity/Bootstrap/` and `Configuration/`:
  - wire all modules,
  - bind lockout/token/MFA/policy settings,
  - enforce startup validation rules.

### Step 12: Unit and behavior tests generation
- [x] Implement test coverage in `tests/AuthModule.CoreSecurity.Tests/` for:
  - authentication/lockout scenarios,
  - token validation failure matrix,
  - permission resolution allow/deny behavior,
  - approval workflow transitions and SoD constraints.

### Step 13: Property-based testing (PBT) generation
- [x] Add FsCheck tests in `tests/AuthModule.CoreSecurity.Tests/PropertyBased/` for approved properties:
  - permission resolution invariant,
  - token check conjunctive denial invariant,
  - email normalization idempotence,
  - disable-user idempotence,
  - approval-state transition invariants.

### Step 14: Deployment and runtime artifact updates
- [x] Update runtime artifacts in workspace root:
  - extend `docker-compose.yml` for CoreSecurity service wiring (internal-only),
  - add/update config templates for UOW-02 policies,
  - keep preview-runtime governance note alignment with UOW-01.

### Step 15: Documentation updates for generated code
- [x] Generate code summary docs in `aidlc-docs/construction/uow-02-core-security/code/`:
  - file map (created/modified),
  - story-to-code traceability,
  - test map and extension compliance notes.

### Step 16: End-to-end generation readiness checkpoint
- [x] Confirm all plan steps are executable in sequence and ready for implementation pass.
- [x] Keep this plan as the single source of truth for UOW-02 code generation execution.

---

## Execution Notes
- Application code must be generated only in workspace root (never in `aidlc-docs/`).
- Documentation artifacts are markdown-only in `aidlc-docs/construction/uow-02-core-security/code/`.
- During implementation, each completed step must be marked `[x]` immediately.
- This plan is the **single source of truth** for UOW-02 code generation.
