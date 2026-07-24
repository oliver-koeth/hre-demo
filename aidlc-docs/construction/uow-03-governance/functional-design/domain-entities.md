# Domain Entities — UOW-03 Governance and Evidence Domains

## Scope
UOW-03 implements US-07..US-12 using UOW-01 persistence foundations and UOW-02 security event hooks. It introduces runtime entities for audit trail querying (security events only), compliance evidence export, RoPA metadata, data-subject retrieval/export, incident classification, and backup evidence capture.

## Applied Decisions
| Topic | Decision |
|---|---|
| Audit query surfaces | Security events only in V1 |
| Evidence export format | JSON only |
| Control mapping | `controlMappingIds` field on evidence records |
| RoPA metadata scope | Minimal (activityName, purpose, lawfulBasis, dataCategories, retentionDays) |
| Data-subject rights V1 scope | Retrieval and export only |
| Legal hold / retention override | Hard block with documented reason on record |
| Retention trigger | Manual operator invocation (API-triggered) |
| Incident classification | Severity + serviceImpact + breachReportable flag |
| Backup evidence model | Backup metadata only |
| PBT scope | Retention decision idempotence |

---

## Runtime Entities

### EvidenceRecord
Wrapper for a compliance-facing evidence snapshot that can carry control mapping identifiers.
```
EvidenceRecord {
  EvidenceId           : Guid
  EvidenceType         : string        // e.g. ACCESS_REVIEW, APPROVAL_AUDIT, SECURITY_EVENT
  SubjectEntityType    : string        // e.g. User, Role, Permission
  SubjectEntityId      : Guid
  CorrelationId        : Guid
  CapturedAt           : DateTimeOffset
  Payload              : string        // JSON-serialized event or change snapshot
  ControlMappingIds    : string[]      // e.g. ["SOC2-CC6.1", "ISO27001-A.9.2"]
  RetentionExpiresAt   : DateTimeOffset?
  LegalHoldActive      : bool
  LegalHoldReason      : string?
}
```

### ProcessingActivity (RoPA metadata)
Minimal RoPA entry for a data-processing activity.
```
ProcessingActivity {
  ActivityId           : Guid
  ActivityName         : string
  Purpose              : string
  LawfulBasis          : string        // e.g. LEGITIMATE_INTEREST, CONTRACT, LEGAL_OBLIGATION
  DataCategories       : string[]      // e.g. ["identity", "authentication-logs"]
  RetentionDays        : int
  IsActive             : bool
  CreatedAt            : DateTimeOffset
  UpdatedAt            : DateTimeOffset
}
```

### DataSubjectRequest
Tracks a data-subject retrieval/export request and its lifecycle.
```
DataSubjectRequest {
  RequestId            : Guid
  SubjectUserId        : Guid
  RequestType          : DataSubjectRequestType   // Retrieve | Export
  RequestedAt          : DateTimeOffset
  RequestedByUserId    : Guid
  Status               : DataSubjectRequestStatus // Pending | Completed | BlockedByHold
  CompletedAt          : DateTimeOffset?
  BlockReason          : string?
  ExportPayload        : string?       // JSON serialized result when complete
  CorrelationId        : Guid
}

DataSubjectRequestType = Retrieve | Export
DataSubjectRequestStatus = Pending | Completed | BlockedByHold
```

### RetentionRule
Policy record controlling when data should be deleted or flagged for lifecycle review.
```
RetentionRule {
  RuleId               : Guid
  EntityType           : string        // e.g. User, SecurityAuditEvent, IncidentRecord
  RetentionPeriodDays  : int
  Action               : RetentionAction   // Delete | Anonymize | Archive
  IsActive             : bool
  CreatedAt            : DateTimeOffset
  UpdatedAt            : DateTimeOffset
}

RetentionAction = Delete | Anonymize | Archive
```

### LifecycleDecisionRecord
Records the outcome of a retention-driven lifecycle invocation.
```
LifecycleDecisionRecord {
  DecisionId           : Guid
  RuleId               : Guid
  EntityType           : string
  EntityId             : Guid
  EvaluatedAt          : DateTimeOffset
  Action               : RetentionAction
  Outcome              : LifecycleOutcome   // Applied | BlockedByHold | Skipped
  BlockReason          : string?
  CorrelationId        : Guid
}

LifecycleOutcome = Applied | BlockedByHold | Skipped
```

### IncidentRecord
Incident classification and breach-flag record.
```
IncidentRecord {
  IncidentId           : Guid
  Title                : string
  Severity             : IncidentSeverity     // Low | Medium | High | Critical
  ServiceImpact        : string
  BreachReportable     : bool
  Status               : IncidentStatus       // Open | Investigating | Resolved | Closed
  CreatedAt            : DateTimeOffset
  UpdatedAt            : DateTimeOffset
  ResolvedAt           : DateTimeOffset?
  CorrelationId        : Guid
}
```

### BackupMetadataRecord
Backup execution evidence for a single backup operation.
```
BackupMetadataRecord {
  BackupId             : Guid
  BackupType           : BackupType       // Full | Incremental
  StorePath            : string
  Status               : BackupStatus     // Pending | Completed | Failed | Verified
  ExecutedAt           : DateTimeOffset
  VerifiedAt           : DateTimeOffset?
  CorrelationId        : Guid
}
```

---

## Derived Keys and Value Objects
- **EvidenceType**: string constant; extensible without schema change.
- **LawfulBasis codes**: string constants aligned to GDPR Article 6 (e.g. `LEGITIMATE_INTEREST`, `LEGAL_OBLIGATION`, `CONTRACT`).
- **ControlMappingIds**: string array; empty by default, populated at evidence-capture time by caller.

---

## Testable Properties (PBT-03)
| ID | Category | Property |
|---|---|---|
| PBT-U03-01 | Idempotence | Retention decision for the same entityId produces the same outcome when record has not changed |

