# NFR Design Patterns — UOW-02 Core Auth and Authorization Control Plane

## Input Decisions Applied
| Question | Selected |
|---|---|
| Q1 Login throttling | Fixed-window counters + hard deny |
| Q2 Token validation performance | Local in-memory cache for active user status + token version (short TTL) |
| Q3 MFA integration | Synchronous external provider call per verification |
| Q4 Approval reliability | Synchronous persistence + bounded retries + fail-closed |
| Q5 Disable consistency | Single transaction-style disable + token version increment before response |
| Q6 Lockout alerting | In-process rolling counter + immediate alert emission |
| Q7 JWT key lifecycle | Single active HMAC key + manual rotation procedure |
| Q8 Error response reliability | Global exception/response middleware with centralized mapper |
| Q9 Retention/archival | Hot retention + periodic archival snapshots |
| Q10 Component partitioning | Monolithic `AuthControlPlaneService` with internal modules |

---

## 1. Resilience Patterns

### RP-U02-01 Bounded Retry + Fail-Closed Persistence
- Governed approval persistence uses bounded retries for transient failures.
- On retry exhaustion, operation fails closed with explicit error response.
- No silent fallback path that could bypass SoD or persistence guarantees.

### RP-U02-02 Transaction-Style Disable Consistency
- Disable-user flow is atomic at service boundary:
  1. mark user inactive
  2. increment token version
  3. persist both before returning success
- If any step fails, no partial success is returned.

### RP-U02-03 Rolling Counter Alert Reliability
- Lockout detector keeps an in-process rolling 10-minute window per account.
- Crossing 5 lockouts triggers immediate alert emission and security event write.

---

## 2. Scalability and Performance Patterns

### SP-U02-01 Fixed-Window Throttling
- Rate limits tracked per-IP and per-account in fixed windows.
- Hard deny when threshold reached; responses include retry-after metadata.

### PP-U02-01 Short-TTL Validation Cache
- Token validation path uses short-lived in-memory cache for:
  - active/inactive account state
  - current token version
- Cache TTL is short enough to preserve near-immediate disable consistency.
- Cache miss path falls back to direct store lookup.

### PP-U02-02 Monolith Internal Module Boundaries
- `AuthControlPlaneService` remains single deployable unit for V1.
- Internal module boundaries keep hot paths isolated:
  - Auth module
  - Validation module
  - Authorization module
  - Approval module
  - Rate-limit module
  - MFA integration module

---

## 3. Security Patterns

### SecP-U02-01 External MFA Verification
- Sensitive admin operations synchronously call external MFA provider.
- Operation proceeds only on explicit positive verification.
- Provider errors default to deny (fail-closed).

### SecP-U02-02 HMAC Single-Key Signing with Rotation Procedure
- Active signing key is sourced from mounted secret file.
- Rotation procedure is manual and controlled:
  - update key material
  - coordinated rollout
  - token reissue window

### SecP-U02-03 Centralized RFC7807 Error Mapping
- Global middleware converts domain failures to consistent Problem Details.
- Required metadata always included: `errorCode`, `correlationId`.
- Sensitive internals never exposed in error bodies.

### SecP-U02-04 Retention and Archival
- Security events remain hot for at least 90 days.
- Periodic archival snapshots preserve evidence continuity.

---

## 4. Pattern-to-NFR Mapping
| NFR | Applied Patterns |
|---|---|
| NFR-U02-001/002/003 | SP-U02-01, PP-U02-01, PP-U02-02 |
| NFR-U02-004/005/006 | RP-U02-01, RP-U02-02 |
| NFR-U02-007/008/009/010/011 | RP-U02-03, SecP-U02-01, SecP-U02-02, SecP-U02-04 |
| NFR-U02-012/013/014 | SecP-U02-03, PP-U02-02 |
| NFR-U02-015/016/017 | (Carried to code-generation test strategy) |
