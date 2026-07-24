# Business Logic Model — UOW-01 Foundation and Persistence Baseline

## Overview

UOW-01 establishes the technical and data foundation for all downstream units. It has no runtime user-facing behaviour of its own — it exposes contracts, primitives, and persistence infrastructure consumed by UOW-02, UOW-03, and UOW-04. This model describes the logic embedded in the persistence gateway, encryption/HMAC lifecycle, concurrency control, schema migration, and configuration bootstrapping.

---

## 1. Store File Lifecycle

### 1.1 Startup Initialisation Flow

```
Service Starts
  |
  +--> Load PolicyConfiguration from config source
  |      Fail fast if any required field is missing or invalid
  |
  +--> Load EncryptionKey from EncryptionKeyPath (mounted secret)
  +--> Load HmacKey from HmacKeyPath (mounted secret)
  |
  +--> For each store file:
  |      |
  |      +--> Does file exist?
  |             No  --> Create empty store file (SchemaVersion="1.0", RecordCount=0)
  |                     Encrypt and sign
  |             Yes --> Verify HMAC (BR-05)
  |                       Fail --> Startup error: IntegrityViolation
  |                     Decrypt and parse (BR-06)
  |                       Fail --> Startup error: EncryptionError
  |                     Check SchemaVersion
  |                       Newer than service --> Startup error: UnknownSchemaVersion
  |                       Older than service --> Run migration (BR-04)
  |                                              Re-encrypt and re-sign
  |                       Current             --> Continue
  |
  +--> Service Ready
```

### 1.2 Read Flow (GetAsync / SearchAsync)

```
Caller: GetAsync<T>(StoreQuery, RequestContext)
  |
  +--> Resolve store file path from entity type and StoreBasePath (BR-11)
  +--> Read file bytes from disk
  +--> Verify HMAC over file bytes (BR-05)
  |      Fail --> Return Result.Failure(IntegrityViolation)
  +--> Decrypt bytes using AES-256-GCM; verify GCM tag (BR-06)
  |      Fail --> Return Result.Failure(IntegrityViolation)
  +--> Parse JSON; extract Records list
  +--> Apply query predicate (ID match, search filter)
  +--> Filter out IsDeleted=true unless query explicitly includes deleted
  +--> Return Result.Success(T?) or Result.Success(IReadOnlyList<T>)
```

### 1.3 Write Flow (SaveAsync)

```
Caller: SaveAsync<T>(StoreCommand<T>, RequestContext)
  |
  +--> Resolve store file path
  +--> Acquire in-process write mutex for this store file (prevents concurrent writes)
  |
  +--> Read + HMAC verify + decrypt current file (as in Read Flow)
  |
  +--> Is this a CREATE or UPDATE?
  |
  |    CREATE:
  |      +--> Assign new UUID v4 to entity ID
  |      +--> Set CreatedAt = RequestContext.Timestamp
  |      +--> Set UpdatedAt = RequestContext.Timestamp
  |      +--> Set Version = 0
  |      +--> Validate uniqueness constraints (BR-01)
  |             Violated --> Return Result.Failure(Conflict)
  |      +--> Append entity to Records list
  |
  |    UPDATE:
  |      +--> Locate entity by ID; not found --> Return Result.Failure(NotFound)
  |      +--> Compare stored Version to command Version (BR-02)
  |             Mismatch --> Return Result.Failure(Conflict)
  |      +--> Apply field mutations
  |      +--> Set UpdatedAt = RequestContext.Timestamp
  |      +--> Increment Version by 1
  |
  +--> Serialise Records list to JSON
  +--> Update RecordCount in header
  +--> Generate new 96-bit nonce; encrypt serialised JSON using AES-256-GCM (BR-06)
  +--> Write encrypted bytes to temp file (e.g., {file}.tmp)
  +--> Compute HMAC-SHA256 over encrypted bytes; write to temp .sig file
  +--> Atomically rename temp files to final paths (BR-05)
  +--> Release write mutex
  |
  +--> Return Result.Success(savedEntity)
```

### 1.4 Soft Delete Flow

```
Caller: SaveAsync<T>(StoreCommand.SoftDelete, RequestContext)
  |
  +--> Treated as UPDATE (passes through Write Flow)
  +--> Validates Version match
  +--> Sets IsDeleted=true, DeletedAt=RequestContext.Timestamp, DeletedBy=RequestContext.UserId
  +--> Increments Version
```

### 1.5 Integrity Verification Flow (VerifyIntegrityAsync)

```
Caller: VerifyIntegrityAsync(StoreIntegrityCheckRequest)
  |
  +--> For each specified store file:
  |      +--> Read raw encrypted bytes
  |      +--> Recompute HMAC-SHA256 over bytes
  |      +--> Compare to stored .sig (constant-time comparison)
  |      +--> Record result: Pass | Fail
  |
  +--> Return StoreIntegrityResult {
         AllPassed : bool
         FileResults : Dictionary<string, IntegrityCheckOutcome>
       }
```

---

## 2. Schema Migration Logic

### 2.1 Migration Decision Table

| Current SchemaVersion | Service Expected | Action |
|---|---|---|
| 1.0 | 1.0 | No action; proceed |
| < current | Any newer | Run applicable migration chain |
| > current | Any older | Startup error: UnknownSchemaVersion |
| File absent | Any | Create empty file at current version |

### 2.2 Migration Algorithm

```
MigrateStoreFile(filePath, fromVersion, toVersion)
  |
  +--> Parse plaintext Records from decrypted JSON
  +--> Identify migration steps: fromVersion → step1 → ... → toVersion
  +--> For each step:
  |      +--> Apply record transformer (adds fields, renames fields, changes types)
  |      +--> RecordCount MUST remain unchanged after step
  |      +--> Update StoreFileHeader.SchemaVersion to step target version
  +--> Re-serialise, re-encrypt, re-sign, write atomically
  +--> Migration is idempotent: running the same step twice yields identical output (BR-04)
```

---

## 3. Optimistic Concurrency Control

### 3.1 Conflict Resolution Model

```
Write Request arrives with { EntityId, Version = V_caller }
  |
  +--> Read stored entity; stored Version = V_stored
  |
  +--> V_caller == V_stored?
  |      Yes --> Apply write; stored Version becomes V_stored + 1
  |      No  --> Return Failure(Conflict) with:
  |                DomainError.Details["CurrentVersion"] = V_stored.ToString()
  |                DomainError.Details["SuppliedVersion"] = V_caller.ToString()
```

### 3.2 Retry Guidance for Callers

Callers receiving a `Conflict` error SHOULD:
1. Re-read the entity to obtain the current state and Version.
2. Re-apply their intended mutation to the fresh state.
3. Re-submit the write with the updated Version.

---

## 4. Credential Lifecycle

### 4.1 Create New Credential

```
CreateCredential(UserId, PlaintextPassword, Algorithm, RequestContext)
  |
  +--> Validate Algorithm ∈ { Argon2id, BCrypt } (BR-07)
  +--> Hash password using chosen algorithm; generate salt
  +--> Encode Hash and Salt as Base64
  +--> Begin atomic write to credentials.json:
  |      +--> Mark all existing Credentials for UserId as IsActive=false (version-checked)
  |      +--> Append new Credential { IsActive=true, Version=0, ... }
  +--> Write, encrypt, sign (Write Flow)
  +--> Return Result.Success(CredentialId)
  |
  +--> IMPORTANT: PlaintextPassword is zeroed from memory after hashing; MUST NOT appear in logs or errors
```

---

## 5. Assignment Active-Status Evaluation

Used by permission resolution (UOW-02); defined here as a shared primitive.

```
IsAssignmentActive(assignment, requestTime) : bool
  :=  NOT assignment.IsDeleted
  AND (assignment.ValidFrom == null OR assignment.ValidFrom <= requestTime)
  AND (assignment.ValidUntil == null OR assignment.ValidUntil >= requestTime)
```

Overlap detection for duplicate-assignment prevention:
```
DoPeriodsOverlap(existingAssignment, newAssignment) : bool
  :=  NOT existingAssignment.IsDeleted
  AND (newAssignment.ValidFrom == null OR existingAssignment.ValidUntil == null
       OR newAssignment.ValidFrom <= existingAssignment.ValidUntil)
  AND (newAssignment.ValidUntil == null OR existingAssignment.ValidFrom == null
       OR newAssignment.ValidUntil >= existingAssignment.ValidFrom)
```

---

## 6. Policy Configuration Loading

### 6.1 Startup Loading Algorithm

```
LoadPolicyConfiguration()
  |
  +--> Read configuration from source (appsettings.json or environment-injected)
  +--> For each required field: present and non-empty?
  |      No  --> throw StartupException("Missing required policy field: {fieldName}")
  +--> Validate constraint rules (BR-10):
  |      TokenLifetimeSeconds > 0
  |      AdminTokenLifetimeSeconds < TokenLifetimeSeconds
  |      MaxLoginAttempts ∈ [3, 20]
  |      LockoutDurationSeconds > 0
  |      All retention day fields ≥ 0
  |      Fail any --> throw StartupException("Invalid policy: {detail}")
  +--> Register as singleton IPolicyConfigurationService
  +--> Log configuration loaded (omit key paths from log output)
```

---

## 7. Repository Interface Contracts

All persistence operations are expressed through the following technology-agnostic interfaces:

### IStoreRepository<T>
```
interface IStoreRepository<T> where T : IEntity
{
  Task<Result<T?, DomainError>> GetAsync(Guid id, RequestContext context)
  Task<Result<IReadOnlyList<T>, DomainError>> SearchAsync(StoreSearchQuery<T> query, RequestContext context)
  Task<Result<T, DomainError>> SaveAsync(T entity, int expectedVersion, RequestContext context)
  Task<Result<T, DomainError>> SoftDeleteAsync(Guid id, int expectedVersion, RequestContext context)
}
```

### IAuditStoreRepository
```
interface IAuditStoreRepository
{
  Task<Result<Unit, DomainError>> AppendSecurityEventAsync(SecurityAuditEvent evt, RequestContext context)
  Task<Result<Unit, DomainError>> AppendAdminChangeEventAsync(AdminChangeEvent evt, RequestContext context)
  Task<Result<IReadOnlyList<SecurityAuditEvent>, DomainError>> QuerySecurityEventsAsync(AuditQuery query, RequestContext context)
  Task<Result<IReadOnlyList<AdminChangeEvent>, DomainError>> QueryAdminChangeEventsAsync(AuditQuery query, RequestContext context)
}
```

### IStoreIntegrityService
```
interface IStoreIntegrityService
{
  Task<StoreIntegrityResult> VerifyAllStoresAsync(RequestContext context)
  Task<StoreIntegrityResult> VerifyStoreAsync(StoreType storeType, RequestContext context)
}
```

### IPolicyConfigurationService
```
interface IPolicyConfigurationService
{
  PolicyConfiguration GetConfiguration()
}
```

---

## 8. Error Code Reference for UOW-01

| Code | Trigger Conditions |
|---|---|
| `NotFound` | Entity ID not found in store |
| `Conflict` | Version mismatch (optimistic concurrency); duplicate unique field; overlapping assignment |
| `ValidationFailed` | Null/empty required field; invalid enum value; `ValidFrom > ValidUntil`; credential with past expiry; MaxLoginAttempts out of range |
| `IntegrityViolation` | HMAC signature verification failed; AES-GCM tag verification failed; missing `.sig` file |
| `PolicyViolation` | Attempt to mutate or delete an audit record; attempt to delete an IsSystem entity; credential algorithm not in approved list |
| `Internal` | Unexpected I/O error; serialisation failure; key-loading failure after startup |
