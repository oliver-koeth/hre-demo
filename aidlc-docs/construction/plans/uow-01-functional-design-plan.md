# UOW-01 Functional Design Plan

## Unit: UOW-01 Foundation and Persistence Baseline

### Plan Checklist
- [x] Collect and validate answers to clarifying questions
- [x] Generate domain entity models (User, Role, Permission, RolePermission, CredentialMetadata, AuditEvent, ProcessingActivity, IncidentRecord, BackupMetadata)
- [x] Define store schemas and file organization for all four stores (AuthStore, AuthzStore, AuditStore, GovernanceStore)
- [x] Define shared primitives and contracts (entity IDs, result types, error contracts, correlation context)
- [x] Define business rules for foundation layer (schema validation, integrity checks, concurrency control)
- [x] Define persistence repository interfaces
- [x] Define policy/configuration service contracts
- [x] Identify testable properties per PBT-01 for all components
- [x] Generate `domain-entities.md`
- [x] Generate `business-rules.md`
- [x] Generate `business-logic-model.md`

---

## Clarifying Questions

### Q1: Entity ID Strategy
What type of identifier should be used for domain entities?

A. GUID/UUID v4 — random, globally unique, no ordering guarantee  
B. ULID — sortable by creation time + globally unique (easier to sort audit logs)  
C. Sequential integer IDs — simple, but not suitable for distributed or portable contexts  
X. Other  

[Answer]: A

---

### Q2: Core Result Type Pattern
What pattern should be used for the shared result/error contract returned by all domain operations?

A. `Result<TValue, TError>` discriminated union — functional style, forces callers to handle both paths  
B. Custom response envelope with `Success` flag, `Value`, and `Errors` list — familiar to REST-style APIs  
C. Exception-based — throw on error, no result wrapper; keep it simple  
X. Other  

[Answer]: A

---

### Q3: User Entity Minimum Attributes
Which attribute set should the User entity carry?

A. UserId, Username, Email, DisplayName, Status, CreatedAt, UpdatedAt, CreatedBy  
B. UserId, Username, Email, Status, CreatedAt, UpdatedAt (no display name — GDPR data-minimisation)  
C. UserId, Username, Status, CreatedAt, UpdatedAt (email is optional / stored as credential, not identity)  
X. Other  

[Answer]: A

---

### Q4: Credential Metadata Storage
How should password credential metadata be stored relative to the User entity?

A. Separate `Credential` record per user in AuthStore (CredentialId, UserId, Algorithm, Hash, Salt, CreatedAt, ExpiresAt, IsActive)  
B. Embedded credential sub-object within the User record in AuthStore  
C. Dedicated per-user credential file (one file per user credential, outside the main user store)  
X. Other  

[Answer]: A

---

### Q5: Role and Permission Hierarchy
Should roles and permissions support a hierarchy, or remain flat?

A. Flat — no inheritance; roles contain permissions directly; simpler to audit and reason about  
B. Hierarchical — parent/child role inheritance; child roles inherit parent permissions  
C. Role groups only — roles can be grouped for display but there is no permission inheritance  
X. Other  

[Answer]: A

---

### Q6: JSON Store File Organisation
How should JSON records be organised on disk?

A. One JSON file per entity type per store (e.g., `auth-store/users.json`, `auth-store/credentials.json`)  
B. One JSON file per entity instance (e.g., `auth-store/users/{id}.json`)  
C. One aggregate JSON file per store domain (single file: `auth-store.json`, `authz-store.json`, etc.)  
X. Other  

[Answer]: A

---

### Q7: Concurrency Control
How should concurrent write access to JSON stores be handled within a single service process?

A. Optimistic concurrency — version/ETag field per record; reject writes that conflict with stale version  
B. Single-writer async queue / in-process mutex — all writes serialized within the process  
C. Pessimistic file locking — exclusive OS-level lock held during each write  
X. Other  

[Answer]: A

---

### Q8: Soft vs Hard Delete
Should entity deletion be soft (flagged) or hard (physically removed)?

A. Soft delete — `IsDeleted` flag + `DeletedAt` timestamp; record retained for audit, legal hold, and recovery  
B. Hard delete with synchronous audit event — record removed; deletion captured in AuditStore  
C. Archival pattern — record moved to a separate archive partition before being removed from active store  
X. Other  

[Answer]: A

---

### Q9: JSON Schema Versioning
How should JSON schema evolution across future versions be handled?

A. Embedded `SchemaVersion` field in every record; migration logic reads old versions and upgrades on read  
B. File-level version header in each store file; a single migration pass upgrades all records at startup  
C. Separate schema-version manifest file alongside each store; migration applied on demand  
X. Other  

[Answer]: B

---

### Q10: Persistence Integrity Protection
How should the integrity of JSON store files be protected at rest?

A. HMAC-SHA256 signature over serialised file content, stored in a sidecar `.sig` file; key from mounted secret  
B. SHA-256 content hash stored in a manifest file; re-computed and compared on every read  
C. Cryptographic hash chain — each record references the hash of the previous record (append-safe log style)  
X. Other  

[Answer]: A

---

### Q11: Correlation and Request Context Structure
What fields should the shared `RequestContext` / `CorrelationContext` carry across all operations?

A. CorrelationId (UUID), UserId (if authenticated), SourceIp, Timestamp, SessionId  
B. TraceId, SpanId, CorrelationId, UserId, SourceIp, Timestamp (OpenTelemetry-aligned)  
C. RequestId, UserId, SourceIp, Timestamp (minimal — add more fields incrementally)  
X. Other  

[Answer]: A

---

### Q12: Role-Permission Assignment Time-Binding
Should role-permission assignments support time-bounded validity (ValidFrom / ValidUntil)?

A. Yes — assignments carry ValidFrom and ValidUntil; expired assignments are excluded from permission resolution  
B. No — assignments are permanent until explicitly removed; simpler model for V1  
C. Planned extension — model the fields now but do not enforce expiry at runtime in V1  
X. Other  

[Answer]: A

---

### Q13: At-Rest Encryption Scope
What scope should application-level at-rest encryption cover for JSON stores?

A. File-level AES-256 encryption for each store file; key provided via mounted Docker secret  
B. Field-level encryption for PII fields only (passwords are hashed so excluded); other data stored in plaintext  
C. Defer to infrastructure — rely on volume/disk encryption only; no application-level encryption in V1  
X. Other  

[Answer]: A

---

### Q14: Policy and Configuration Service Scope in UOW-01
What should the Policy/Configuration service contract expose as part of the UOW-01 foundation?

A. Full policy config schema now: token lifetime/claims, lockout thresholds, retention policy IDs, store paths, encryption key refs  
B. Auth-scoped policies only in UOW-01 (token, lockout); governance/retention policies defined when UOW-03 is designed  
C. Stub interface only in UOW-01 — define the contract but leave values unpopulated until downstream units contribute their policies  
X. Other  

[Answer]: A
