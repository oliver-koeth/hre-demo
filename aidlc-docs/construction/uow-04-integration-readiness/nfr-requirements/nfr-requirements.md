# NFR Requirements — UOW-04 Integration Readiness and Quality Gate

## Input Decisions Applied
| Question | Selected |
|---|---|
| Q1 Gate evaluation throughput | 5 evaluations/minute |
| Q2 Gate decision latency | P95 < 15 seconds |
| Q3 Traceability completeness | 100% of US-01..US-12 mapped to implementation files |
| Q4 Gate availability | 99.5% monthly |
| Q5 Blocking findings policy | Immediate hard block, no bypass |
| Q6 Error contract strictness | All endpoints must comply; single failure blocks gate |
| Q7 Runtime artifact validation depth | Presence-only checks (`docker-compose.yml`, policy template) |
| Q8 Alert trigger | Any gate failure |
| Q9 Gate evidence retention | 90 days |
| Q10 PBT scope | No new PBT in UOW-04 |

---

## 1. Scalability and Performance

### NFR-U04-001 Gate Throughput
- Integration-readiness evaluation shall support **5 full gate evaluations/minute** in V1.

### NFR-U04-002 Gate Decision Latency
- Full gate decision (including traceability and contract checks) shall meet **P95 < 15 seconds**.

### NFR-U04-003 Traceability Completeness
- Story-to-file evidence map shall be **100% complete** for US-01..US-12 before gate pass.

---

## 2. Availability and Reliability

### NFR-U04-004 Gate Availability
- Integration gate workflow shall target **99.5% monthly availability**.

### NFR-U04-005 Blocking-Finding Enforcement
- Any blocking finding shall immediately set gate result to fail.
- No bypass or emergency override is permitted in V1.

### NFR-U04-006 Deterministic Gate Outcome
- For unchanged inputs (unit status, contract checks, traceability state), repeated evaluations shall return identical pass/fail outcomes.

---

## 3. Security and Compliance

### NFR-U04-007 Cross-Unit Error Contract Consistency
- All endpoints across all units must expose RFC7807-compatible error payloads with both `errorCode` and `correlationId`.
- Any nonconforming endpoint is a blocking finding.

### NFR-U04-008 Auditability of Gate Decisions
- Every gate run shall emit retained evidence containing final status and short explanatory notes.

---

## 4. Runtime and Operability

### NFR-U04-009 Runtime Artifact Presence Checks
- Gate workflow shall verify presence of:
  1. `docker-compose.yml`
  2. `config/policy.template.json`
- Missing artifact is a blocking failure.

### NFR-U04-010 Alerting
- Any gate failure shall trigger immediate operational alert emission in the standard structured logging path.

### NFR-U04-011 Evidence Retention Window
- Gate decision evidence shall be retained for **90 days** in V1.

---

## 5. Testability and Quality

### NFR-U04-012 Property-Based Testing Scope
- UOW-04 introduces **no new PBT suites**.
- Compliance is satisfied by reusing prior unit-level PBT evidence from UOW-02 and UOW-03.

### NFR-U04-013 Example-Based Verification
- Example-based checks remain mandatory for:
  - blocking-finding fail behavior,
  - full traceability completeness enforcement,
  - error-contract consistency detection,
  - runtime artifact presence checks.
