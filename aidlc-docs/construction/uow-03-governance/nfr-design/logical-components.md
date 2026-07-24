# Logical Components — UOW-03 NFR Design

## Deployment Shape
UOW-03 uses one deployable runtime: **GovernanceService**.
NFR responsibilities are isolated through internal modules with strict boundaries.

## LC-U03-01 AuditQueryModule
- Applies time-window and page-size constraints.
- Serves security-event query API only (V1 scope).

## LC-U03-02 EvidenceModule
- Captures immutable evidence records with control mapping IDs.
- Streams JSON export payloads with manifest metadata.

## LC-U03-03 DataSubjectModule
- Handles retrieval/export request lifecycle.
- Delegates legal-hold evaluation to policy guard before response generation.

## LC-U03-04 RetentionModule
- Executes manual lifecycle invocation commands.
- Applies deterministic decision and persisted fingerprint strategy.
- Records lifecycle outcomes and emits failure alerts.

## LC-U03-05 IncidentModule
- Manages incident create/update/status transitions.
- Enforces durable commit boundary before success response.

## LC-U03-06 BackupEvidenceModule
- Records backup metadata append events.
- Coordinates freshness path with cache invalidation.

## LC-U03-07 LegalHoldPolicyGuard
- Centralized fail-closed policy gate for hold-constrained operations.
- Returns reason-coded block outcomes for all guarded modules.

## LC-U03-08 AlertModule
- Emits structured operational/security alert events on trigger conditions.
- Standardizes event schema and correlation propagation.

---

## Interaction Flow
1. Request enters GovernanceService.
2. If operation is hold-sensitive, LegalHoldPolicyGuard evaluates first.
3. Target module executes (AuditQuery/Evidence/DataSubject/Retention/Incident/BackupEvidence).
4. AlertModule emits immediate alerts on retention failures or blocked bypass attempts.
5. Response mapping preserves correlation ID and fail-closed semantics.

---

## Responsibility Matrix
| Concern | Logical Component |
|---|---|
| Security event queries with pagination | AuditQueryModule |
| Evidence capture/export streaming | EvidenceModule |
| Data-subject retrieval/export | DataSubjectModule |
| Manual retention execution and idempotence | RetentionModule |
| Durable incident persistence | IncidentModule |
| Backup metadata ingestion/read freshness | BackupEvidenceModule |
| Legal-hold enforcement | LegalHoldPolicyGuard |
| Immediate operational alert emission | AlertModule |

