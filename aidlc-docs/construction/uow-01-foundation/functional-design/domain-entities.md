# Domain Entities — UOW-01 Foundation and Persistence Baseline

## Design Decisions Applied
| Decision | Answer |
|---|---|
| Entity IDs | UUID v4 |
| Result contract | `Result<TValue, TError>` discriminated union |
| User attributes | UserId, Username, Email, DisplayName, Status, CreatedAt, UpdatedAt, CreatedBy |
| Credential storage | Separate `Credential` record in AuthStore |
| Role/Permission hierarchy | Flat (no inheritance) |
| Store file organisation | One JSON file per entity type per store |
| Concurrency control | Optimistic (Version field / ETag) |
| Delete strategy | Soft delete (IsDeleted + DeletedAt) |
| Schema versioning | File-level SchemaVersion header, migrated at startup |
| Integrity protection | HMAC-SHA256 sidecar `.sig` file (signs encrypted bytes) |
| At-rest encryption | AES-256-GCM, key from mounted Docker secret |
| Correlation context | CorrelationId, UserId, SourceIp, Timestamp, SessionId |
| Assignment time-binding | ValidFrom / ValidUntil enforced in V1 |
| Policy/config scope | Full policy schema in UOW-01 |

---

## Shared Primitives

### EntityId
- Type alias: `Guid` (UUID v4)
- Generated at entity creation; immutable thereafter.

### RequestContext
```
RequestContext {
  CorrelationId : Guid           // Required; generated at entry point if absent
  UserId        : Guid?          // Null for unauthenticated requests
  SourceIp      : string?        // Best-effort; absence is not an error
  Timestamp     : DateTimeOffset // Server-side operation time; not client-supplied
  SessionId     : Guid?          // Set when a session token is present
}
```

### Result<TValue, TError>
```
Result<TValue, TError> =
  | Success(Value : TValue)
  | Failure(Error : TError)
```
- Callers MUST handle both arms; no implicit unwrapping.

### DomainError
```
DomainError {
  Code          : DomainErrorCode  // Enum (see below)
  Message       : string
  Details       : Dictionary<string, string>  // Optional contextual pairs
  CorrelationId : Guid
}

DomainErrorCode =
  | NotFound
  | Conflict
  | ValidationFailed
  | IntegrityViolation
  | PolicyViolation
  | Unauthorized
  | Forbidden
  | Internal
```

### StoreVersion (file-level header)
```
StoreFileHeader {
  SchemaVersion : string   // Semver major.minor, e.g. "1.0"
  StoreType     : string   // e.g. "AuthStore/Users"
  RecordCount   : int      // Written on flush; validated on read
}
```

---

## AuthStore Entities

### File layout
```
data/auth-store/
  users.json           (encrypted, SchemaVersion header)
  users.json.sig       (HMAC-SHA256 of encrypted bytes)
  credentials.json     (encrypted, SchemaVersion header)
  credentials.json.sig
```

### User
```
User {
  UserId      : Guid             // UUID v4; immutable
  Username    : string           // Unique (case-insensitive); max 64 chars
  Email       : string           // Unique (case-insensitive); max 254 chars
  DisplayName : string           // Max 128 chars; not unique
  Status      : UserStatus       // Enum
  CreatedAt   : DateTimeOffset   // Set at creation; immutable
  UpdatedAt   : DateTimeOffset   // Updated on every mutation
  CreatedBy   : Guid             // UserId of creating admin
  IsDeleted   : bool             // Soft-delete flag
  DeletedAt   : DateTimeOffset?  // Set on soft-delete
  DeletedBy   : Guid?            // Admin who soft-deleted
  Version     : int              // Optimistic concurrency counter; starts at 0
}

UserStatus =
  | PendingActivation   // Created but not yet activated
  | Active              // Normal operating state
  | Locked              // Locked due to failed authentication attempts
  | Inactive            // Administratively disabled
```

### Credential
```
Credential {
  CredentialId : Guid             // UUID v4; immutable
  UserId       : Guid             // FK → User.UserId
  Algorithm    : HashAlgorithm    // Enum
  Hash         : string           // Base64-encoded credential hash
  Salt         : string           // Base64-encoded salt
  CreatedAt    : DateTimeOffset
  ExpiresAt    : DateTimeOffset?  // Null = no expiry
  IsActive     : bool             // True for current active credential
  Version      : int
}

HashAlgorithm =
  | Argon2id  // Default; preferred
  | BCrypt    // Supported for migration compatibility
```

---

## AuthzStore Entities

### File layout
```
data/authz-store/
  roles.json               (encrypted + .sig)
  permissions.json         (encrypted + .sig)
  role-permissions.json    (encrypted + .sig)
  user-roles.json          (encrypted + .sig)
```

### Role
```
Role {
  RoleId      : Guid
  Name        : string           // Unique (case-insensitive); max 64 chars
  Description : string           // Max 512 chars
  IsSystem    : bool             // True = built-in; cannot be deleted via API
  CreatedAt   : DateTimeOffset
  UpdatedAt   : DateTimeOffset
  CreatedBy   : Guid
  IsDeleted   : bool
  DeletedAt   : DateTimeOffset?
  DeletedBy   : Guid?
  Version     : int
}
```

### Permission
```
Permission {
  PermissionId : Guid
  Name         : string   // Unique (case-insensitive); max 64 chars
  Resource     : string   // Logical resource name (e.g. "users", "roles")
  Action       : string   // Logical action (e.g. "read", "write", "delete")
  Description  : string   // Max 512 chars
  IsSystem     : bool
  CreatedAt    : DateTimeOffset
  UpdatedAt    : DateTimeOffset
  CreatedBy    : Guid
  IsDeleted    : bool
  DeletedAt    : DateTimeOffset?
  DeletedBy    : Guid?
  Version      : int
}
```

### RolePermissionAssignment
```
RolePermissionAssignment {
  AssignmentId : Guid
  RoleId       : Guid              // FK → Role.RoleId
  PermissionId : Guid              // FK → Permission.PermissionId
  ValidFrom    : DateTimeOffset?   // Null = effective immediately from creation
  ValidUntil   : DateTimeOffset?   // Null = no expiry
  CreatedAt    : DateTimeOffset
  CreatedBy    : Guid
  IsDeleted    : bool
  DeletedAt    : DateTimeOffset?
  DeletedBy    : Guid?
  Version      : int
}
```

### UserRoleAssignment
```
UserRoleAssignment {
  AssignmentId : Guid
  UserId       : Guid              // FK → User.UserId
  RoleId       : Guid              // FK → Role.RoleId
  ValidFrom    : DateTimeOffset?
  ValidUntil   : DateTimeOffset?
  CreatedAt    : DateTimeOffset
  CreatedBy    : Guid
  IsDeleted    : bool
  DeletedAt    : DateTimeOffset?
  DeletedBy    : Guid?
  Version      : int
}
```

---

## AuditStore Entities

### File layout
```
data/audit-store/
  security-events.json       (encrypted + .sig)
  admin-change-events.json   (encrypted + .sig)
```

### SecurityAuditEvent
Append-only; never mutated or soft-deleted.
```
SecurityAuditEvent {
  EventId       : Guid
  EventType     : SecurityEventType  // Enum
  ActorId       : Guid?              // Null for anonymous/pre-auth events
  ActorUsername : string?
  SourceIp      : string?
  Result        : OperationResult    // Enum: Success | Failure
  Reason        : string?            // Failure reason code or description
  CorrelationId : Guid
  SessionId     : Guid?
  Timestamp     : DateTimeOffset
  Details       : string?            // JSON-encoded supplemental context
}

SecurityEventType =
  | LoginAttempt
  | LoginSuccess
  | LoginFailure
  | AccountLocked
  | TokenIssued
  | TokenValidated
  | TokenRejected
  | AccountDisabled
  | PrivilegedAccess
  | BruteForceDetected
```

### AdminChangeEvent
Append-only; never mutated or soft-deleted.
```
AdminChangeEvent {
  EventId            : Guid
  EventType          : AdminChangeEventType  // Enum
  ActorId            : Guid
  ApprovalTicketId   : Guid?                 // Set for governed changes (SoD)
  TargetEntityType   : string                // e.g. "User", "Role", "Permission"
  TargetEntityId     : Guid
  ChangeType         : EntityChangeType      // Enum: Create | Update | Delete
  BeforeSnapshot     : string?               // JSON-encoded entity state before change
  AfterSnapshot      : string?               // JSON-encoded entity state after change
  CorrelationId      : Guid
  Timestamp          : DateTimeOffset
}

AdminChangeEventType =
  | UserCreated | UserUpdated | UserDeleted
  | RoleCreated | RoleUpdated | RoleDeleted
  | PermissionCreated | PermissionUpdated | PermissionDeleted
  | RolePermissionAssigned | RolePermissionRevoked
  | UserRoleAssigned | UserRoleRevoked
  | ApprovalGranted | ApprovalRejected
```

---

## GovernanceStore Entities

### File layout
```
data/governance-store/
  processing-activities.json   (encrypted + .sig)
  incident-records.json        (encrypted + .sig)
  backup-metadata.json         (encrypted + .sig)
```

### ProcessingActivity
```
ProcessingActivity {
  ActivityId         : Guid
  Name               : string    // e.g. "Authentication", "Admin Audit Logging"
  Purpose            : string    // Plain-language processing purpose
  LawfulBasis        : string    // e.g. "Legitimate Interest", "Contract", "Legal Obligation"
  DataCategories     : string[]  // e.g. ["Username", "Email", "IP Address", "Credential Hash"]
  RetentionPeriodDays: int       // 0 = no fixed retention (governed by legal hold only)
  IsActive           : bool
  CreatedAt          : DateTimeOffset
  UpdatedAt          : DateTimeOffset
  Version            : int
}
```

### IncidentRecord
```
IncidentRecord {
  IncidentId      : Guid
  Title           : string
  Severity        : IncidentSeverity  // Enum
  ServiceImpact   : string            // Free text; describes affected services
  DataImpact      : string?           // Describes personal data affected, if any
  BusinessImpact  : string?
  Status          : IncidentStatus    // Enum
  CreatedAt       : DateTimeOffset
  UpdatedAt       : DateTimeOffset
  ResolvedAt      : DateTimeOffset?
  BreachReportable: bool              // True = GDPR 72h notification threshold may apply
  Version         : int
}

IncidentSeverity = | Low | Medium | High | Critical
IncidentStatus   = | Open | Investigating | Resolved | Closed
```

### BackupMetadata
```
BackupMetadata {
  BackupId              : Guid
  BackupType            : BackupType      // Enum
  StorePath             : string          // Logical path/reference (not a plaintext absolute path)
  Status                : BackupStatus    // Enum
  ExecutedAt            : DateTimeOffset
  VerifiedAt            : DateTimeOffset?
  RestoredAt            : DateTimeOffset? // Set if this backup was used in a restore
  ReconciliationResult  : string?         // Summary of post-restore reconciliation
  Version               : int
}

BackupType   = | Full | Incremental
BackupStatus = | Pending | Completed | Failed | Verified | RestoredOk | RestoreFailed
```

---

## Policy Configuration

### PolicyConfiguration (loaded at startup, immutable at runtime)
```
PolicyConfiguration {
  // Token policy
  TokenLifetimeSeconds            : int    // Standard user access token lifetime
  AdminTokenLifetimeSeconds       : int    // Privileged user shorter lifetime
  TokenIssuer                     : string
  TokenAudience                   : string

  // Lockout policy
  MaxLoginAttempts                : int    // Failed attempts before lockout
  LockoutDurationSeconds          : int    // Duration of automatic lockout

  // Storage
  StoreBasePath                   : string // Base directory for all store files
  EncryptionKeyPath               : string // Path to mounted Docker secret key file
  HmacKeyPath                     : string // Path to mounted Docker secret HMAC key file

  // Retention policy references (IDs match ProcessingActivity records)
  AuditEventRetentionDays         : int
  UserRecordRetentionDays         : int
  IncidentRecordRetentionDays     : int

  // Governance flags
  SodApprovalRequiredForRoleChanges : bool
}
```

---

## Testable Properties (PBT-01)

### C-09 Persistence Gateway

| ID | Property Category | Description |
|---|---|---|
| PBT-P01 | Round-trip | `SaveAsync(entity)` followed by `GetAsync(id)` returns a structurally equivalent entity |
| PBT-P02 | Round-trip | JSON serialize → AES-256-GCM encrypt → decrypt → JSON deserialize = original entity for any valid entity type |
| PBT-P03 | Invariant | After any successful `SaveAsync`, `VerifyIntegrityAsync` returns Success (HMAC consistent post-write) |
| PBT-P04 | Idempotence | `VerifyIntegrityAsync` on an unchanged store file always returns the same result regardless of how many times it is called |
| PBT-P05 | Invariant | `SearchAsync` result count is always ≤ total non-deleted records in the store |
| PBT-P06 | Invariant | `Version` after a successful `SaveAsync` = pre-write `Version + 1` for any entity |
| PBT-P07 | Idempotence | Schema migration applied twice to a store file at SchemaVersion N produces identical output as applying it once |
| PBT-P08 | Invariant | Schema migration preserves record count (no records lost) |

### Result<TValue, TError> Primitive

| ID | Property Category | Description |
|---|---|---|
| PBT-P09 | Invariant | `Result.Success(v)` is never Failure; `Result.Failure(e)` is never Success — for all v, e |
| PBT-P10 | Round-trip | `Result.Success(v).GetValueOrDefault()` returns `v` for all non-null values |

### Role-Permission Assignment Time-Binding

| ID | Property Category | Description |
|---|---|---|
| PBT-P11 | Invariant | Permission resolution never returns assignments where `ValidUntil < requestTime` |
| PBT-P12 | Invariant | For any assignment where both ValidFrom and ValidUntil are present: `ValidFrom ≤ ValidUntil` always holds |

### Concurrency Control

| ID | Property Category | Description |
|---|---|---|
| PBT-P13 | Invariant | Two concurrent writes with the same stale Version result in exactly one success and at least one `Conflict` error |

### Correlation Context

| ID | Property Category | Description |
|---|---|---|
| PBT-P14 | Invariant | CorrelationId is non-null on every RequestContext passed to any domain operation |
