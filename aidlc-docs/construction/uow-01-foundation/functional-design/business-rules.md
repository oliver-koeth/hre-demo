# Business Rules — UOW-01 Foundation and Persistence Baseline

## BR-01: Entity Creation Rules

- Every entity MUST be assigned a UUID v4 at creation time; the ID is immutable thereafter.
- `Username` MUST be unique across all non-deleted User records (case-insensitive comparison).
- `Email` MUST be unique across all non-deleted User records (case-insensitive comparison).
- `Role.Name` and `Permission.Name` MUST each be unique across non-deleted records of their respective types (case-insensitive).
- `Permission.Resource` + `Permission.Action` combined MUST be unique across non-deleted Permission records.
- `CreatedAt` MUST be set to server-side UTC time at creation and MUST NOT change on any subsequent mutation.
- `UpdatedAt` MUST be set to server-side UTC time on every successful mutation.
- `Version` MUST be initialised to `0` on creation.
- `IsSystem` entities (built-in roles and permissions) MUST NOT be created through the regular admin API; they are seeded at service startup.

---

## BR-02: Optimistic Concurrency Rules

- Every write operation that mutates an existing entity MUST supply the caller's last-known `Version` value.
- Before applying the write, the stored `Version` MUST be compared to the supplied value.
  - If they match: apply the write and increment `Version` by 1.
  - If they do not match: reject the write with `DomainErrorCode.Conflict`; include the current server-side `Version` in the error `Details`.
- New entity creation does not require a Version check (no prior record exists).
- Soft-delete operations are treated as mutations and MUST also pass a matching `Version`.
- Version is a non-negative integer and MUST be strictly monotonically increasing per entity.

---

## BR-03: Soft Delete Rules

- Deleting an entity sets `IsDeleted = true`, `DeletedAt = now`, `DeletedBy = actorId`.
- Deleted entities MUST be excluded from all active-record queries by default (i.e., filtered unless an explicit include-deleted flag is passed).
- A deleted entity's `Username` / `Name` / `Email` is retained in the store but MUST NOT be treated as occupying the uniqueness namespace — a new entity with the same identifier MUST be permitted after soft-deletion.
  - Exception: if the deleted entity is still within the legal-hold or retention period, the admin UI SHOULD surface a warning.
- Hard physical removal of records from store files is NOT permitted within this service; removal is handled by the retention/anonymisation workflow in UOW-03.
- `IsSystem` entities (roles and permissions) MUST NOT be soft-deleted.

---

## BR-04: File-Level Schema Versioning Rules

- Each store JSON file MUST contain a top-level `SchemaVersion` header field (semver major.minor, e.g. `"1.0"`).
- A `StoreType` header field MUST also be present and MUST match the expected store type for validation.
- On service startup, before accepting any requests, each store file MUST be checked:
  - If `SchemaVersion` matches current: proceed.
  - If `SchemaVersion` is older: apply the relevant migration(s) and write the upgraded file.
  - If `SchemaVersion` is newer than the service understands: fail fast with a startup error.
- Migration MUST be idempotent: running the same migration twice produces identical output.
- Migration MUST preserve `RecordCount` (no records may be lost during migration).
- If a store file does not exist at startup, create an empty store file with the current `SchemaVersion`.

---

## BR-05: HMAC Integrity Rules

- Before reading any store file, the HMAC-SHA256 signature MUST be verified:
  1. Read the `.sig` sidecar file (same path + `.sig` extension).
  2. Compute `HMAC-SHA256(encrypted_file_bytes, hmac_key)`.
  3. Compare to the stored signature using a constant-time comparison function.
  4. If verification fails: return `DomainErrorCode.IntegrityViolation`; do not return any data.
- After every successful write:
  1. Encrypt the new content.
  2. Compute the HMAC over the encrypted bytes.
  3. Write both the encrypted file and the `.sig` file atomically (write to temp files first, then rename).
- If the `.sig` sidecar file is absent: treat as integrity violation (do not silently ignore).
- The HMAC key MUST be loaded from the mounted Docker secret path configured in `PolicyConfiguration.HmacKeyPath`.
- HMAC keys MUST NOT be logged, included in error messages, or stored anywhere other than the mounted secret.

---

## BR-06: At-Rest Encryption Rules

- Each store file MUST be encrypted using AES-256-GCM before writing to disk.
- A unique, cryptographically random 96-bit (12-byte) nonce/IV MUST be generated for each encryption operation.
- The nonce MUST be prepended to the ciphertext in the stored file (first 12 bytes = nonce, remainder = ciphertext + authentication tag).
- Decryption MUST succeed and the GCM authentication tag MUST be verified before any JSON parsing occurs.
- If decryption or GCM tag verification fails: return `DomainErrorCode.IntegrityViolation`.
- The encryption key MUST be loaded from `PolicyConfiguration.EncryptionKeyPath` (mounted Docker secret).
- Encryption keys MUST NOT be hardcoded, logged, or stored in environment variables or configuration files.
- Re-encryption with a new key (key rotation) MUST be performed as an atomic decrypt-then-re-encrypt-and-sign operation.

---

## BR-07: Credential Rules

- A User MUST have at most one active Credential record (`IsActive = true`) at any time.
- When a new Credential is created for a user, all previous Credential records for that user MUST be set `IsActive = false` in the same atomic write.
- `Credential.Algorithm` MUST be one of the enumerated approved values: `Argon2id` (default) or `BCrypt` (migration compatibility only).
- New credentials MUST use `Argon2id`; `BCrypt` is permitted only when migrating existing hashes.
- `Credential.Hash` and `Credential.Salt` MUST be stored in Base64-encoded (standard) format.
- Plaintext passwords MUST NOT appear in Credential records, log entries, error messages, or correlation context.
- `ExpiresAt` if set MUST be in the future at credential creation time; attempting to create a credential with a past expiry MUST return `DomainErrorCode.ValidationFailed`.

---

## BR-08: Assignment Time-Binding Rules (Roles and Permissions)

- `ValidFrom` and `ValidUntil` are optional on both `RolePermissionAssignment` and `UserRoleAssignment`.
- If both `ValidFrom` and `ValidUntil` are provided: `ValidFrom MUST ≤ ValidUntil`; violation returns `DomainErrorCode.ValidationFailed`.
- An assignment is considered **active** if all of the following hold at `requestTime`:
  - `IsDeleted = false`
  - `ValidFrom` is null OR `ValidFrom ≤ requestTime`
  - `ValidUntil` is null OR `ValidUntil ≥ requestTime`
- Permission resolution and role membership checks MUST only consider active assignments.
- Creating a duplicate assignment for the same `(RoleId, PermissionId)` pair where the validity periods of existing non-deleted assignments **overlap** MUST return `DomainErrorCode.Conflict`.
- Similarly for `(UserId, RoleId)` on `UserRoleAssignment`.

---

## BR-09: Request Context Rules

- Every domain operation signature MUST accept a `RequestContext` parameter.
- `CorrelationId` MUST be a non-null UUID. If the inbound request does not supply one, the API entry-point middleware MUST generate a new UUID and set it before invoking any domain logic.
- `Timestamp` in `RequestContext` MUST reflect the server-side wall-clock time at the start of the request; it is set once at the entry point and MUST NOT be updated mid-request.
- `SourceIp` is best-effort; its absence does not cause a validation failure.
- `UserId` and `SessionId` are set only when a valid authenticated session is present; they MUST be null for anonymous/pre-authentication requests.
- `RequestContext` is immutable once created; downstream components MUST NOT mutate it.

---

## BR-10: Policy Configuration Rules

- `PolicyConfiguration` is loaded exactly once at service startup from the configuration source.
- All fields in `PolicyConfiguration` that are marked required MUST be present and valid; service startup MUST fail fast with a descriptive error if any required field is missing, empty, or out of range.
  - Required fields: `TokenLifetimeSeconds`, `AdminTokenLifetimeSeconds`, `TokenIssuer`, `TokenAudience`, `MaxLoginAttempts`, `LockoutDurationSeconds`, `StoreBasePath`, `EncryptionKeyPath`, `HmacKeyPath`.
- `PolicyConfiguration` is immutable at runtime — hot-reload is not supported; a service restart is required to apply configuration changes.
- All domain components MUST consume policy values via the `IPolicyConfigurationService` interface, not by reading environment variables or files directly.
- `TokenLifetimeSeconds` MUST be a positive integer.
- `AdminTokenLifetimeSeconds` MUST be less than `TokenLifetimeSeconds`.
- `MaxLoginAttempts` MUST be between 3 and 20 (inclusive).
- `LockoutDurationSeconds` MUST be a positive integer.
- Retention day fields MUST be non-negative integers; 0 indicates no fixed retention period (legal-hold only).

---

## BR-11: Store File Path and Naming Rules

- The base path for all store files is `PolicyConfiguration.StoreBasePath`.
- Each store directory and file name is fixed:
  - `{StoreBasePath}/auth-store/users.json` and `users.json.sig`
  - `{StoreBasePath}/auth-store/credentials.json` and `credentials.json.sig`
  - `{StoreBasePath}/authz-store/roles.json`, `permissions.json`, `role-permissions.json`, `user-roles.json` (and `.sig` siblings)
  - `{StoreBasePath}/audit-store/security-events.json`, `admin-change-events.json` (and `.sig` siblings)
  - `{StoreBasePath}/governance-store/processing-activities.json`, `incident-records.json`, `backup-metadata.json` (and `.sig` siblings)
- Store directories MUST be created if they do not exist at startup.

---

## BR-12: Audit Store Append-Only Rules

- `SecurityAuditEvent` and `AdminChangeEvent` records are append-only; no mutation or soft-delete operations are permitted.
- Any attempt to update or delete an audit record MUST return `DomainErrorCode.PolicyViolation`.
- Audit store records carry no `Version` or `IsDeleted` fields.
- HMAC integrity and encryption rules (BR-05, BR-06) still apply to audit store files.
