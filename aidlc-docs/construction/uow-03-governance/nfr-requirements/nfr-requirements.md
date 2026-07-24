# NFR Requirements — UOW-03 Governance and Evidence Domains

## Input Decisions Applied
| Question | Selected |
|---|---|
| Q1 Audit query throughput | 20 req/s sustained |
| Q2 Evidence export latency (10k records) | P95 < 5 seconds |
| Q3 Retrieval/export availability | 99.5% monthly |
| Q4 Retention invocation reliability | At-least-once execution + idempotent outcomes |
| Q5 Legal-hold strictness | Hard fail-closed block + reason + audit event |
| Q6 Incident evidence durability | Single-store durable write (JSON boundary) |
| Q7 Backup metadata freshness | Queryable within 30 seconds |
| Q8 Evidence retention minimum | 180 days |
| Q9 Immediate alert trigger | Any failed retention invocation or blocked hold-bypass attempt |
| Q10 PBT scope | Retention decision idempotence only |

---

## 1. Scalability and Performance

### NFR-U03-001 Audit Query Throughput
- Security-audit query endpoints shall sustain **20 requests/second** in V1.
- Scope includes filtered reads over security-event records only.

### NFR-U03-002 Evidence Export Latency
- JSON evidence export up to **10,000 records** shall meet **P95 < 5 seconds**.
- Export manifest generation time is included in this SLO.

### NFR-U03-003 Backup Metadata Freshness
- Newly recorded backup metadata shall be queryable within **30 seconds** of write acceptance.

---

## 2. Availability and Reliability

### NFR-U03-004 Retrieval/Export Availability
- Data-subject retrieval and export operations shall target **99.5% monthly availability**.

### NFR-U03-005 Retention Invocation Reliability
- Manual retention lifecycle invocation shall run with **at-least-once** execution semantics.
- Outcome recording must be idempotent for unchanged record state.

### NFR-U03-006 Incident Record Durability
- Incident records must be durably persisted to the primary JSON store before success response.
- No success response may be returned before durable write completion.

---

## 3. Security and Compliance

### NFR-U03-007 Legal-Hold Fail-Closed Enforcement
- Any legal-hold constrained operation must be blocked fail-closed.
- Blocked responses shall include a documented reason and correlated audit event.
- No override path is allowed in V1.

### NFR-U03-008 Evidence Retention Minimum
- Governance evidence records must be retained for at least **180 days**.
- Retention expiry metadata shall be stored on each record for lifecycle evaluation.

### NFR-U03-009 Correlation and Traceability
- Governance operations must carry and persist correlation IDs across evidence, lifecycle, incident, and backup records.

---

## 4. Monitoring and Alerting

### NFR-U03-010 Immediate Alert Conditions
- An immediate operational alert shall be emitted for:
  1. any failed retention invocation,
  2. any blocked legal-hold bypass attempt.

### NFR-U03-011 Observability Requirements
- Alert events must include operation name, entity type/id (if applicable), error classification, and correlation ID.
- Structured logs remain the canonical event channel in V1.

---

## 5. Testability and Quality

### NFR-U03-012 Property-Based Testing Scope
- Mandatory PBT coverage for UOW-03 is limited to **retention decision idempotence**.
- Property assertion: for same input state and rule, repeated evaluation returns same lifecycle outcome.

### NFR-U03-013 Example-Based Tests
- Example-based tests remain required for:
  - legal-hold fail-closed blocking behavior,
  - export manifest shape and count consistency,
  - incident durable-write success/failure paths.

