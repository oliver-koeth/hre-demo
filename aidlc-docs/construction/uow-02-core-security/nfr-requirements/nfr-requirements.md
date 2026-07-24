# NFR Requirements — UOW-02 Core Auth and Authorization Control Plane

## Scope
This document defines UOW-02 non-functional requirements for authentication, token validation, privileged step-up, governed role-permission approvals, and authorization enforcement.

## Inherited Baseline
- Language/runtime: C# + Docker
- Foundation services and JSON persistence from UOW-01
- Observability stack: Serilog + OpenTelemetry
- Security baseline: deny-by-default, fail-closed validation
- Resiliency baseline: warm standby strategy

---

## UOW-02 NFR Targets

### 1. Performance and Scalability
- **NFR-U02-001**: `/auth/login` sustained throughput target is **50 req/s**.
- **NFR-U02-002**: Protected endpoint token validation P99 latency target is **< 50 ms**.
- **NFR-U02-003**: Login and token validation handlers must expose separate latency and error metrics for SLO tracking.

### 2. Availability and Reliability
- **NFR-U02-004**: Governed role-permission approval operations must target **99.5% monthly availability**.
- **NFR-U02-005**: User disable operation must enforce **strong consistency** for token invalidation before success response.
- **NFR-U02-006**: Approval workflow state updates must be durable and recoverable without invalid backward transitions.

### 3. Security Controls
- **NFR-U02-007**: Repeated lockouts must trigger an operational security alert at **5 lockouts per account within 10 minutes**.
- **NFR-U02-008**: Sensitive admin step-up must use an **external MFA provider integration** (e.g., Microsoft Authenticator-compatible path).
- **NFR-U02-009**: API abuse protection must enforce **per-IP and per-account rate limits** on login and token-validation endpoints.
- **NFR-U02-010**: JWT signing remains **HMAC symmetric** in V1, with strict key handling via mounted secret files.
- **NFR-U02-011**: Auth/admin security events minimum retention is **90 days**.

### 4. Maintainability and Operability
- **NFR-U02-012**: Error responses must remain mapped to RFC7807 Problem Details and include stable `errorCode` and `correlationId`.
- **NFR-U02-013**: Authorization policy changes must preserve the `resource:action` key convention to keep endpoint checks deterministic.
- **NFR-U02-014**: Operational dashboards must expose lockout alerts, token rejection rates, step-up failures, and approval workflow backlog.

### 5. Testing Quality (PBT)
- **NFR-U02-015**: Property-based testing scope for UOW-02 is **auth/token invariants only**.
- **NFR-U02-016**: PBT must run with FsCheck and include shrinking and reproducible failing seeds.
- **NFR-U02-017**: Example-based tests remain mandatory for business-critical auth/admin paths.

---

## Compliance Trace Notes
- **PBT-09**: Satisfied by continuing FsCheck framework usage in .NET test stack.
- **Security baseline alignment**: lockout alert threshold, MFA externalization, rate limits, strict token checks.
- **Resiliency baseline alignment**: strong consistency on disable revocation and measurable approval availability target.
