# NFR Design Patterns — UOW-03 Governance and Evidence Domains

## Input Decisions Applied
| Question | Selected |
|---|---|
| Q1 Retention failure handling | Immediate fail response + structured alert + no partial success |
| Q2 Idempotence enforcement | Deterministic decision function + persisted decision fingerprint |
| Q3 Audit query performance | Time-window filters + bounded page size |
| Q4 Export latency pattern | Streaming JSON writer + chunked pagination |
| Q5 Legal-hold enforcement | Central policy guard service |
| Q6 Incident durability | Synchronous write + fsync before success |
| Q7 Backup metadata freshness | Immediate append + read-through cache invalidation |
| Q8 Alert emission | Structured security event + operational log hook in request path |
| Q9 Retention/archival | Retention-expiry metadata + manual archival/deletion command path |
| Q10 Component partitioning | Monolith `GovernanceService` with internal modules |

---

## 1. Resilience Patterns

### RP-U03-01 Fail-Closed Retention Execution
- Retention invocation returns failure immediately on execution error.
- No partial-success response is returned.
- Failure always emits structured alert metadata with correlation ID.

### RP-U03-02 Deterministic Idempotence Fingerprinting
- Lifecycle decision uses deterministic inputs: entity state hash + rule id + evaluation window.
- Persisted decision fingerprint prevents divergent outcomes for unchanged state.
- Repeat evaluations return the same outcome without duplicate side effects.

### RP-U03-03 Durable Incident Commit Boundary
- Incident operations complete only after synchronous write and durability confirmation (fsync boundary).
- Prevents success responses before durable persistence.

---

## 2. Scalability and Performance Patterns

### SP-U03-01 Windowed Audit Query Pattern
- Query shape enforces date-window filters and bounded pagination defaults.
- Avoids full scans under steady 20 req/s audit read load.

### PP-U03-01 Streaming Evidence Export
- Exports use chunked data fetch and streaming JSON serialization.
- Keeps memory bounded while meeting P95 latency for large exports.

### PP-U03-02 Backup Metadata Freshness Path
- Metadata append is synchronous to primary store.
- Read-through cache invalidation guarantees fresh query visibility target.

---

## 3. Security and Compliance Patterns

### SecP-U03-01 Central Legal-Hold Policy Guard
- Single guard service validates legal-hold policy before governance data output/mutation operations.
- All blocked paths are fail-closed and reason-coded.

### SecP-U03-02 Structured Alert Event Contract
- Retention failures and hold-bypass blocks emit standardized alert events.
- Required fields: operation, entity reference, error class, correlation ID, timestamp.

### SecP-U03-03 Retention-Expiry Metadata Model
- Evidence records carry explicit retention-expiry metadata.
- Manual archival/deletion commands evaluate expiry and hold state before action.

---

## 4. Pattern-to-NFR Mapping
| NFR | Applied Patterns |
|---|---|
| NFR-U03-001 | SP-U03-01 |
| NFR-U03-002 | PP-U03-01 |
| NFR-U03-003 | PP-U03-02 |
| NFR-U03-004/005 | RP-U03-01, RP-U03-02 |
| NFR-U03-006 | RP-U03-03 |
| NFR-U03-007/010/011 | SecP-U03-01, SecP-U03-02 |
| NFR-U03-008/009 | SecP-U03-03 |
| NFR-U03-012 | RP-U03-02 |

