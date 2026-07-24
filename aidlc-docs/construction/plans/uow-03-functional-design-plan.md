# UOW-03 Functional Design Plan

## Unit: UOW-03 Governance and Evidence Domains

### Plan Checklist
- [x] Collect and validate functional design answers
- [x] Define UOW-03 domain entities
- [x] Define business rules
- [x] Define business logic model
- [x] Generate `domain-entities.md`
- [x] Generate `business-rules.md`
- [x] Generate `business-logic-model.md`

---

## Context
UOW-03 delivers compliance-facing capabilities: audit and evidence services, privacy governance operations (data-subject rights, lawful-basis tagging, RoPA support), incident classification and breach-notification evidence, backup and recovery evidence capture, and retention-driven lifecycle management.

Stories in scope: **US-07, US-08, US-09, US-10, US-11, US-12**

Dependencies: UOW-01 Foundation repositories and primitives; UOW-02 audit event sink and security event capture hooks.

---

## Functional Design Questions

### Q1: Audit Trail Queryability
Which audit query surfaces should be directly supported in V1?

A. Security events only (authentication and token events)
B. Security events + admin-change events (full audit trail)
C. Security + admin-change + evidence export endpoints (all audit surfaces)
X. Other

[Answer]: A

---

### Q2: Evidence Export Format
What export format should compliance evidence packages use in V1?

A. JSON only (machine-readable, no special rendering)
B. JSON + CSV
C. JSON with optional structured text summary
X. Other

[Answer]: A

---

### Q3: Control Mapping Support
Should evidence items carry a linkable control-identifier field (SoA/control catalog reference) in V1?

A. Yes — include a `controlMappingIds` field on evidence records for manual tagging
B. No — defer control mapping to post-processing or later iteration
C. Yes — include a free-text `controlReference` annotation field
X. Other

[Answer]: A

---

### Q4: Processing Activity (RoPA) Metadata Scope
Which RoPA-style metadata fields should be supported in V1?

A. Minimal: activityName, purpose, lawfulBasis, dataCategories, retentionDays
B. Standard: add responsibleTeam, dataSubjectTypes, crossBorderTransfer flag
C. Full: add regulatoryReferences, privacyNoticeLink, annualReviewDate
X. Other

[Answer]: A

---

### Q5: Data-Subject Rights Operations
Which data-subject operations should be supported as first-class V1 workflows?

A. Retrieval and export only
B. Retrieval + erasure (subject to legal-hold check) + restriction
C. Retrieval + erasure + restriction + correction + portability export
X. Other

[Answer]: A

---

### Q6: Legal Hold and Retention Override Behavior
When erasure/restriction is requested but a legal-hold or audit-obligation exists, what is the V1 behavior?

A. Hard block with documented reason stored on the record
B. Flag record as hold-pending; defer deletion until hold is released
C. Allow erasure but retain anonymized retention marker with reason
X. Other

[Answer]: A

---

### Q7: Retention Policy Enforcement Trigger
How should retention-driven deletion/anonymization be triggered in V1?

A. Manual operator invocation only (API-triggered)
B. Scheduled background sweep
C. Manual for now; structured hooks for future scheduling
X. Other

[Answer]: A

---

### Q8: Incident Classification Model
What severity levels and classification fields should incidents carry in V1?

A. Severity (Low/Medium/High/Critical) + serviceImpact + breachReportable flag
B. Same as A + dataImpact field + businessImpact field
C. Same as B + regulatoryNotificationRequired + notificationDeadlineAt
X. Other

[Answer]: A

---

### Q9: Backup and Recovery Evidence Model
What recovery evidence fields are required in V1?

A. Backup metadata only (path, type, status, executedAt, verifiedAt)
B. Add restore execution record + reconciliation result field
C. Add continuity test event record (separate from backup/restore events)
X. Other

[Answer]: A

---

### Q10: PBT-Targeted Properties for UOW-03
Which behavioral properties should FsCheck tests cover in this unit?

A. Retention decision idempotence only
B. Retention decision idempotence + legal-hold cannot be bypassed by erasure
C. B + audit-record append idempotence under duplicate event IDs
X. Other

[Answer]: A

