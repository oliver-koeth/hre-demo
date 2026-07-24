# Business Logic Model — UOW-03 Governance and Evidence Domains

## Story Coverage
- **US-07** Security Audit Trail Capture
- **US-08** Access and Change Evidence Retrieval
- **US-09** Controlled Personal-Data Operations
- **US-10** Retention-Driven Deletion and Anonymization
- **US-11** Incident Evidence and Classification Support
- **US-12** Backup, Recovery, and Continuity Readiness

---

## 1. Audit Trail Query (US-07)

V1 exposes security events only.

1. Authorized caller invokes audit query with optional filters (dateFrom, dateTo, eventType, actorId).
2. Service resolves matching security audit events from the audit store.
3. Returned records include: eventId, eventType, actorId, timestamp, result, correlationId, sourceIp.
4. Response includes total count and event list.

Decision table:
| Condition | Outcome |
|---|---|
| Caller authorized | Return filtered security events |
| Admin-change events requested | Return not-supported (V1 scope) |
| No events match filters | Return empty list with zero count |

---

## 2. Evidence Record Capture and Export (US-08)

### 2a. Evidence Capture
1. Caller supplies: evidenceType, subjectEntityType, subjectEntityId, payload, controlMappingIds, retentionExpiresAt.
2. Service creates `EvidenceRecord` with correlationId from request context.
3. Record is persisted immutably; no further modification is allowed.

### 2b. Evidence Export
1. Authorized caller requests an evidence export for a subject entity or evidence type.
2. Service retrieves matching `EvidenceRecord` entries.
3. Service wraps records in an export manifest:
   - exportId, requestedAt, evidenceCount, requestedByUserId, correlationId
   - records array (JSON)
4. Returns JSON package; no CSV or rendering in V1.

---

## 3. Data-Subject Retrieval and Export (US-09)

1. Authorized operator creates a `DataSubjectRequest` (type: Retrieve or Export).
2. Service checks if any records for the subject userId are under legal hold.
3. If legal hold active on any record → request is blocked (`BlockedByHold`), reason documented.
4. If no hold → service collects personal data records for subject userId from persistence.
5. For Retrieve: returns structured data summary.
6. For Export: serializes to JSON bundle and stores as `ExportPayload`; marks request Completed.

Decision table:
| Condition | Outcome |
|---|---|
| Legal hold active | Hard block, document reason, no data exposure |
| Erasure/restriction requested | Return NOT_SUPPORTED error (V1) |
| No records found | Return empty result, mark Completed |

---

## 4. Retention Lifecycle Evaluation (US-10)

1. Authorized operator triggers lifecycle evaluation for a specific entity type (via API).
2. Service resolves active `RetentionRule` for that entity type.
3. For each record of that type:
   a. Check if `LegalHoldActive = true` → outcome: `BlockedByHold`.
   b. Check if record age >= RetentionPeriodDays → eligible for lifecycle action.
   c. Apply action (Delete, Anonymize, Archive) or log `Skipped` if not yet expired.
4. For each evaluated record, create a `LifecycleDecisionRecord`.

Idempotence guarantee:
- Evaluating the same record again with unchanged state produces the same `LifecycleDecisionRecord` outcome.

Decision table:
| Condition | Outcome |
|---|---|
| Legal hold active | BlockedByHold, no data mutation |
| Record age < retention period | Skipped |
| Record age >= retention period, no hold | Applied (Delete / Anonymize / Archive per rule) |

---

## 5. Incident Classification and Management (US-11)

1. Operator creates an incident with: title, severity, serviceImpact, breachReportable.
2. Incident starts with `Status = Open`.
3. Operator transitions status: Open → Investigating → Resolved → Closed.
4. `BreachReportable = true` is set by the operator when the incident may require regulatory notification.
5. No backward state transition is permitted.

Decision table:
| Current Status | Valid Next Status |
|---|---|
| Open | Investigating |
| Investigating | Resolved |
| Resolved | Closed |
| Closed | (terminal) |

---

## 6. Backup Evidence Capture (US-12)

1. Backup executor (operator or script) creates a `BackupMetadataRecord` at job start: type, storePath, status=Pending, executedAt.
2. On job completion: operator updates status to Completed or Failed.
3. On backup verification: operator updates status to Verified and sets verifiedAt.
4. All updates are append-style status progressions; raw backup data is never stored in this module.

Decision table:
| Current Status | Valid Next Status |
|---|---|
| Pending | Completed or Failed |
| Completed | Verified |
| Failed | (terminal in V1) |
| Verified | (terminal) |

---

## 7. API Contract Mapping
Domain `Result` maps to HTTP same as UOW-02:
| Domain Error | HTTP Status | errorCode |
|---|---|---|
| ValidationFailed | 400 | VALIDATION_FAILED |
| Unauthorized | 401 | UNAUTHORIZED |
| Forbidden | 403 | FORBIDDEN |
| NotFound | 404 | NOT_FOUND |
| Conflict | 409 | CONFLICT |
| Internal | 500 | INTERNAL_ERROR |
| PolicyViolation (legal hold) | 422 | LEGAL_HOLD_ACTIVE |

Each response includes `correlationId`.

---

## 8. PBT-Focused Behaviors
- PBT-U03-01: Retention decision idempotence — evaluating the same unchanged record under the same rule produces the same outcome on repeat invocations.

