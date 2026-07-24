# NFR Design Patterns — UOW-04 Integration Readiness and Quality Gate

## Decision Summary
| Question | Selected Pattern |
|---|---|
| Q1 Failure resilience | Immediate fail + alert + deterministic re-run |
| Q2 Determinism | Canonical evaluation order + stable rules + normalized inputs |
| Q3 Throughput/concurrency | Single in-process evaluator with bounded parallel requests |
| Q4 Latency control | Bounded checks with per-step timeout budgets and early fail |
| Q5 Error-contract conformance | Centralized checker with per-endpoint assertions |
| Q6 Blocking policy | Mandatory blocker registry; open blocker => fail |
| Q7 Runtime artifact validation | Direct filesystem presence checks with explicit findings |
| Q8 Alerting | Structured event + error log emission in request path |
| Q9 Evidence retention | Append-only gate-run records with retention metadata |
| Q10 Logical partitioning | Single `IntegrationGateService` with internal helper modules |

---

## 1. Resilience Patterns

### NFRD-U04-001 Fail-Fast Gate Execution
- Any failed check returns immediate gate failure.
- Failure emits structured alert event and error log in the same execution path.
- Re-run remains deterministic by re-evaluating unchanged inputs with identical ordering.

### NFRD-U04-002 No-Bypass Block Enforcement
- `BlockerRegistry` semantics are embedded in gate evaluation.
- Presence of any open blocker hard-fails the gate.
- No operator override branch exists in V1.

---

## 2. Scalability and Concurrency Patterns

### NFRD-U04-003 Bounded In-Process Evaluation
- A single in-process evaluator handles gate logic.
- Request handling is bounded to protect determinism and avoid race-driven divergence.
- Throughput target (5 evaluations/minute) is achieved without distributed coordination.

---

## 3. Performance Patterns

### NFRD-U04-004 Step-Budgeted Evaluation
- Gate flow enforces strict timeout budgets per check category:
  1. Contract conformance
  2. Traceability completeness
  3. Runtime artifact presence
  4. Blocking-item resolution
- Early fail is triggered as soon as a blocking condition is found.

### NFRD-U04-005 Deterministic Evaluation Order
- Checklist rules execute in a canonical, fixed order.
- Inputs are normalized before evaluation to avoid ordering drift.

---

## 4. Security and Compliance Patterns

### NFRD-U04-006 Centralized Error-Contract Verification
- A centralized conformance checker validates RFC7807 shape plus `errorCode` and `correlationId` for all endpoints.
- Per-endpoint assertions produce explicit findings for auditability.

### NFRD-U04-007 Integration Evidence Auditability
- Each gate run writes append-only evidence (status + short note + retention metadata).
- Evidence records support 90-day retention policy enforcement.

---

## 5. Operability Patterns

### NFRD-U04-008 Runtime Artifact Presence Checks
- Gate checks filesystem presence of:
  - `docker-compose.yml`
  - `config/policy.template.json`
- Missing files produce explicit blocking findings.

### NFRD-U04-009 Failure Alert Emission
- Every gate failure emits:
  1. Structured operational event payload
  2. Correlated error log entry
- Alerting remains on existing observability channels.
