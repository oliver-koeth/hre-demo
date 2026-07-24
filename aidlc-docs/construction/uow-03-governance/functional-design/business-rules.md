# Business Rules — UOW-03 Governance and Evidence Domains

## BR-U03-01 Audit Trail Scope (V1)
- V1 audit query surface covers **security events only** (authentication and token lifecycle events).
- Admin-change events remain persisted (UOW-02 hooks) but are not exposed via a query API in V1.
- Audit records returned must include: eventType, actorId, timestamp, result, correlationId, sourceIp.

## BR-U03-02 Evidence Record Structure
- Every evidence record must carry: evidenceType, subjectEntityType, subjectEntityId, correlationId, capturedAt, payload.
- Evidence records accept a `controlMappingIds` array for SoA/control catalog tagging.
- Control mapping identifiers are provided by the caller; the service stores them without validation.
- Evidence records are immutable once written; no update or delete path is exposed.

## BR-U03-03 Evidence Export Format
- Evidence export packages are JSON only.
- Export output must be structured and machine-readable with no rendering dependencies.
- Each export must include a manifest section: exportId, requestedAt, evidenceCount, requestedByUserId, correlationId.

## BR-U03-04 Processing Activity (RoPA) Lifecycle
- RoPA entries must carry at minimum: activityName, purpose, lawfulBasis, dataCategories, retentionDays.
- A RoPA entry can be marked inactive (soft-deactivated) but not hard-deleted.
- `lawfulBasis` must be one of the defined string constants.

## BR-U03-05 Data-Subject Operations (V1 Scope)
- V1 supports **retrieval** and **export** only.
- Retrieval returns all personal data records associated with the subject user ID.
- Export returns a structured JSON bundle of the retrieved records.
- Erasure and restriction are **out of scope in V1** — requests for these operations return a not-supported error.

## BR-U03-06 Legal Hold Hard Block
- When a data-subject request targets a record with `LegalHoldActive = true`, the request is hard-blocked.
- Blocked requests are recorded with `Status = BlockedByHold` and a documented `BlockReason`.
- No data is modified, exported, or deleted when a hold is active.

## BR-U03-07 Retention Trigger
- Retention-driven lifecycle evaluation is **manually triggered** by an authorized operator via API call.
- The service evaluates records against active retention rules and records a `LifecycleDecisionRecord` for each entity evaluated.
- If a legal hold is active on a record, the decision outcome is `BlockedByHold` regardless of retention rule.
- The same entity evaluated again with unchanged state must produce the same outcome (idempotence rule).

## BR-U03-08 Incident Classification
- Incidents are classified by severity: Low, Medium, High, Critical.
- Required fields: title, severity, serviceImpact, breachReportable.
- `BreachReportable = true` signals that the incident may require regulatory notification.
- Incidents progress through status: Open → Investigating → Resolved → Closed (no backward transition).

## BR-U03-09 Backup Evidence Capture
- Backup metadata is recorded per backup execution: path, type, status, executedAt, verifiedAt (when verified).
- Backup records are append-only; no in-place update after initial capture.
- `Status` transitions: Pending → Completed or Failed; Completed → Verified.

## BR-U03-10 Audit Event Correlation
- All governance operations must carry a `CorrelationId` sourced from request context.
- Evidence records, lifecycle decisions, incident records, and backup records all inherit correlation from the triggering request context.

