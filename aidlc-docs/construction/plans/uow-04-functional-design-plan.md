# UOW-04 Functional Design Plan

## Unit: UOW-04 Integration Readiness and Quality Gate

### Plan Checklist
- [x] Collect and validate functional design answers
- [x] Define integration-readiness domain artifacts
- [x] Define quality-gate business rules
- [x] Define cross-unit logic model
- [x] Generate `domain-entities.md`
- [x] Generate `business-rules.md`
- [x] Generate `business-logic-model.md`

---

## Context
UOW-04 owns cross-unit integration readiness for UOW-01..UOW-03 and defines the final quality gate before Build and Test execution. It must verify consistency of contracts, traceability of US-01..US-12, unresolved dependency handling, and handoff criteria for test-stage entry.

Stories in scope: **Integration ownership across US-01..US-12 (traceability and readiness only)**

Dependencies: UOW-01 Foundation, UOW-02 Core Security, UOW-03 Governance code and docs.

---

## Functional Design Questions

### Q1: Integration Readiness Record Scope
What should a readiness record cover in V1?

A. Unit-level status only (UOW complete/incomplete with notes)
B. Unit + contract checksum + unresolved-item list
C. Unit + contract checksum + unresolved-item list + owner + target resolution date
X. Other

[Answer]:A

---

### Q2: Contract Consistency Validation Surface
Which cross-unit contracts should be validated explicitly in V1?

A. Public API request/response contracts only
B. Public APIs + shared domain primitives + repository interfaces
C. B + runtime configuration contracts and policy keys
X. Other

[Answer]:B

---

### Q3: Story Traceability Granularity
How should US-01..US-12 traceability be represented?

A. Story-to-unit coverage only
B. Story-to-unit + story-to-files evidence map
C. Story-to-unit + story-to-files + story-to-tests evidence map
X. Other

[Answer]:B

---

### Q4: Unresolved Decision Register Policy
How should unresolved items be handled at the gate?

A. Any unresolved item blocks gate passage
B. Only high-impact unresolved items block; low-impact items can be deferred with owner/date
C. Unresolved items never block; all are deferred to Build and Test notes
X. Other

[Answer]:A

---

### Q5: Quality Gate Entry Criteria
What is the minimum criterion set to enter Build and Test?

A. All units implemented + no blocking unresolved items
B. A + targeted unit tests passing for each unit
C. B + contract consistency checklist completed and signed in docs
X. Other

[Answer]:A

---

### Q6: Cross-Unit Error Contract Consistency
What should be enforced for error semantics across units?

A. RFC7807 shape + `errorCode` + `correlationId` present everywhere
B. A + unit-specific error code namespace prefixes
C. B + shared error catalog document with canonical mappings
X. Other

[Answer]:A

---

### Q7: Runtime Topology Readiness Validation
Which runtime artifacts should UOW-04 validate before handoff?

A. `docker-compose.yml` and policy template presence
B. A + service dependency chain and internal-network constraints
C. B + environment-specific overlay readiness checklist
X. Other

[Answer]:A

---

### Q8: Integration Checklist Ownership Model
How should ownership be captured for integration checklist items?

A. Single owner for whole checklist
B. Per-item owner with status
C. Per-item owner + due date + escalation contact
X. Other

[Answer]:A

---

### Q9: Evidence of Completion for Integration Checks
What completion evidence is required for each integration check?

A. Status + short note
B. Status + linked file/test/contract reference
C. B + timestamped reviewer acknowledgement
X. Other

[Answer]:A

---

### Q10: PBT-Targeted Properties for UOW-04
Which property-based checks should be included in this integration unit?

A. None in UOW-04 (reuse unit-level PBT evidence only)
B. Deterministic story-to-traceability-map completeness property
C. B + contract-list normalization/idempotence property
X. Other

[Answer]:A
