# NFR Design Patterns — UOW-01 Foundation and Persistence Baseline

## Input Decisions Applied
| Question | Selected |
|---|---|
| Q1 Resilience pattern | A — bounded exponential retry (max 3) |
| Q2 Integrity failure handling | B — quarantine + auto-restore from latest verified backup |
| Q3 Write concurrency pattern | A — per-store async write queue + optimistic concurrency |
| Q4 Scalability threshold | B — 50 MB/store-file trigger |
| Q5 Read latency pattern | A — in-memory indexes for key lookups |
| Q6 Write latency pattern | B — immediate per-operation atomic write |
| Q7 Secret access pattern | A — load once at startup in protected key provider |
| Q8 Mandatory security logs | A — integrity failures, migrations, concurrency conflicts, key-loading failures |
| Q9 Crypto/integrity component boundary | A — separate EncryptionService + IntegrityService, orchestrated by StorePersistenceService |
| Q10 Runtime release control | B — advisory warning at release time |

---

## 1. Resilience Patterns

### RP-01: Transient I/O Retry Policy
- Applies to store read/write filesystem operations classified as transient (e.g., file temporarily locked, short-lived I/O interruption).
- Strategy: exponential backoff with jitter, max 3 attempts.
- Terminal behavior: if retries exhausted, return structured failure (fail-closed, no partial success).

### RP-02: Integrity Failure Quarantine and Recovery
- Trigger: HMAC mismatch or AES-GCM authentication failure.
- Action sequence:
  1. Move affected file and signature to quarantine location.
  2. Emit high-severity security log and alert.
  3. Restore from latest verified backup snapshot.
  4. Re-run integrity verification before reopening store operations.
- During recovery window: store operations are blocked for affected store.

### RP-03: Startup Validation Barrier
- Service startup remains gated by required key/config checks and schema compatibility checks.
- Runtime serves requests only after all required stores pass validation and (if needed) recovery.

---

## 2. Scalability Patterns

### SP-01: Per-Store Write Isolation
- Each store has an independent write queue and worker.
- Avoids head-of-line blocking between unrelated stores.
- Preserves deterministic write order within each store.

### SP-02: Store Size Threshold and Partition Trigger
- Trigger threshold: 50 MB per store file.
- At threshold breach, emit warning and open partition/rollover action item for downstream implementation.
- Partition objective: keep active store file size under threshold while retaining auditability.

---

## 3. Performance Patterns

### PP-01: In-Memory Lookup Indexes
- Maintain indexes for high-frequency keys:
  - User: `UserId`, normalized `Username`, normalized `Email`
  - Role/Permission: IDs and normalized names
  - Assignments: `UserId`, `RoleId`, `PermissionId`
- Index lifecycle:
  - Built on startup from validated store files.
  - Updated atomically with successful writes.
  - Rebuilt after restore/migration events.

### PP-02: Immediate Atomic Writes
- No batching window in UOW-01.
- Every mutation performs immediate atomic write (`tmp` + rename) and signature update.
- Supports deterministic consistency at the cost of lower peak write throughput.

### PP-03: SLO-Constrained Operation Targets
- Read path target: P99 < 100 ms.
- Write path target: P99 < 250 ms.
- Required instrumentation: separate histograms/timers for read and write pipelines.

---

## 4. Security Patterns

### SecP-01: Key Management in Process
- Encryption and HMAC keys are loaded once at startup from mounted secret files.
- Keys are provided to consumers via protected in-memory key provider interface.
- No per-request disk reads for key material.

### SecP-02: Separation of Crypto Concerns
- `EncryptionService`: encryption/decryption only.
- `IntegrityService`: HMAC sign/verify only.
- `StorePersistenceService`: orchestrates read/write flow and composes both services.

### SecP-03: Mandatory Foundation Security Event Logging
- Security logs required for:
  - Integrity verification failures
  - Schema migration executions
  - Optimistic concurrency conflicts
  - Key loading/initialization failures
- Events include timestamp, correlation ID, severity, and store identifier.

---

## 5. Risk-Control Patterns

### RCP-01: Runtime Preview Governance
- Selected approach: advisory-only warning for preview runtime during release.
- Advisory content must include unsupported-runtime risk and recommendation to move to supported runtime before production.

### RCP-02: Release Advisory Hook
- A release check step records warning when preview runtime is detected.
- Does not fail release by design per selected decision.

---

## 6. Pattern-to-NFR Mapping
| NFR | Implemented by |
|---|---|
| NFR-UOW01-004/005 (latency) | PP-01, PP-02, PP-03 |
| NFR-UOW01-007/008 (encryption + integrity) | SecP-01, SecP-02, RP-02 |
| NFR-UOW01-012/013/014/015 (reliability) | RP-01, RP-02, RP-03, SP-01 |
| NFR-UOW01-010/011 (logging and redaction) | SecP-03 |
| NFR-UOW01-020/021/022 (maintainability/versioning) | PP-01, RP-03, SP-02 |

