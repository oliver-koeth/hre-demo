# UOW-04 NFR Requirements Plan

## Unit: UOW-04 Integration Readiness and Quality Gate

## Context Summary
UOW-04 is an integration-only unit that validates cross-unit readiness for UOW-01..UOW-03 before Build and Test. NFR scope focuses on gate reliability, consistency, traceability completeness, and operational decision latency.

---

### Plan Checklist
- [x] Collect and validate NFR answers
- [x] Generate `nfr-requirements.md`
- [x] Generate `tech-stack-decisions.md`

---

## NFR Questions

### Q1: Gate Evaluation Throughput Target
What sustained throughput should the integration-readiness gate evaluation support in V1?

A. 5 evaluations/minute

B. 20 evaluations/minute

C. 60 evaluations/minute

X. Other

[Answer]: A

---

### Q2: Gate Decision Latency SLO
What P95 latency target should apply for producing a full gate decision (including traceability checks)?

A. < 5 seconds

B. < 15 seconds

C. < 30 seconds

X. Other

[Answer]:B

---

### Q3: Traceability Completeness Requirement
What minimum completeness is required for story-to-file traceability at gate time?

A. 100% of US-01..US-12 must have at least one mapped implementation file

B. 95% minimum with documented gaps

C. 90% minimum with deferred gaps

X. Other

[Answer]: A

---

### Q4: Gate Availability Target
What monthly availability target should apply to the integration gate workflow?

A. 99.0%

B. 99.5%

C. 99.9%

X. Other

[Answer]: B

---

### Q5: Blocking-Finding Handling Policy
How should blocking findings be handled operationally in V1?

A. Immediate hard block; no bypass

B. Hard block with explicit emergency override flag

C. Soft block with warning and continuation allowed

X. Other

[Answer]: A

---

### Q6: Error Contract Conformance Strictness
What strictness should be enforced for RFC7807 + `errorCode` + `correlationId` consistency checks?

A. All endpoints across all units must comply; any single failure blocks gate

B. Compliance required for externally exposed endpoints only

C. Compliance required for new/changed endpoints only

X. Other

[Answer]: A

---

### Q7: Runtime Artifact Validation Depth
How deep should runtime topology validation go in UOW-04 NFR scope?

A. Presence-only checks for `docker-compose.yml` and policy template

B. Presence + dependency-chain and internal-network constraint validation

C. B + environment overlay validation

X. Other

[Answer]: A

---

### Q8: Observability and Alerting Trigger
Which condition should trigger immediate alerting for integration readiness?

A. Any gate failure

B. Consecutive gate failures (2+)

C. Only gate failures caused by contract inconsistencies

X. Other

[Answer]: A

---

### Q9: Evidence Retention for Gate Runs
How long should gate decision evidence (status + note outputs) be retained in V1?

A. 30 days

B. 90 days

C. 180 days

X. Other

[Answer]: B

---

### Q10: PBT Scope for UOW-04 NFR
What property-based testing requirement should apply in this unit?

A. No new PBT; rely on prior unit-level PBT evidence

B. Add PBT for deterministic checklist status computation

C. Add PBT for deterministic checklist computation + traceability completeness invariants

X. Other

[Answer]: A
