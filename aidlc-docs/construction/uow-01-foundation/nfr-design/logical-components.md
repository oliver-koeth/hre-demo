# Logical Components — UOW-01 NFR Design

## Component Set

## LC-01 StorePersistenceService
- Orchestrates read/write flows per store.
- Uses retry policy for transient I/O.
- Coordinates encryption/integrity operations.
- Owns per-store write queue workers and atomic commit path.

## LC-02 EncryptionService
- Performs AES-256-GCM encrypt/decrypt.
- Uses key material from KeyProvider.
- Returns explicit failures on authentication tag errors.

## LC-03 IntegrityService
- Computes and verifies HMAC-SHA256 signatures.
- Performs constant-time signature comparison.
- Provides file quarantine trigger signal on mismatch.

## LC-04 KeyProvider
- Loads encryption/HMAC keys once at startup.
- Exposes in-memory key handles to crypto services.
- Fails startup on key absence or invalid key format.

## LC-05 StoreWriteCoordinator
- Maintains independent async queue per store.
- Guarantees ordered writes within each store.
- Applies optimistic concurrency validation before commit.

## LC-06 StoreIndexService
- Maintains in-memory indexes for key lookups.
- Rebuilds indexes at startup, post-migration, and post-restore.
- Applies in-transaction index updates after successful write commit.

## LC-07 RecoveryOrchestrator
- Handles quarantine + auto-restore flow for integrity failures.
- Blocks affected store operations during recovery.
- Revalidates integrity before returning store to active status.

## LC-08 SecurityEventLogger
- Emits mandatory foundation security events:
  - integrity failures
  - migration executions
  - concurrency conflicts
  - key loading failures
- Ensures structured log format and correlation fields.

## LC-09 RuntimeReleaseAdvisor
- Detects preview runtime marker during release checks.
- Emits advisory warning (non-blocking) with risk context.

---

## Interaction Flow (Read Path)
1. `StorePersistenceService` receives read request.
2. It loads store metadata and asks `IntegrityService` to verify signature.
3. It asks `EncryptionService` to decrypt payload.
4. It queries `StoreIndexService` for direct lookup/filter acceleration.
5. On transient failure, retry policy is applied (max 3 attempts).
6. On integrity failure, `RecoveryOrchestrator` is triggered.

## Interaction Flow (Write Path)
1. `StorePersistenceService` submits mutation to `StoreWriteCoordinator` queue for target store.
2. Worker validates optimistic concurrency.
3. Worker encrypts content via `EncryptionService`.
4. Worker signs encrypted bytes via `IntegrityService`.
5. Worker atomically commits file and `.sig`.
6. Worker updates indexes via `StoreIndexService`.
7. `SecurityEventLogger` records conflict/integrity/migration/key events as applicable.

---

## Responsibility Matrix
| Concern | Owning Component |
|---|---|
| Transient I/O retries | StorePersistenceService |
| Per-store write ordering | StoreWriteCoordinator |
| Crypto operations | EncryptionService |
| Integrity verification/signing | IntegrityService |
| Key lifecycle (startup load) | KeyProvider |
| Auto-restore on integrity failure | RecoveryOrchestrator |
| Lookup acceleration | StoreIndexService |
| Mandatory security logging | SecurityEventLogger |
| Preview runtime release warning | RuntimeReleaseAdvisor |

