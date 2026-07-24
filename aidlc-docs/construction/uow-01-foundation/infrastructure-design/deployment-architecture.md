# Deployment Architecture — UOW-01

## Environment Scope
- **Environment**: `dev` only.
- **Deployment model**: Local Docker Compose.
- **Availability model**: Single-host development deployment.

## Logical-to-Infrastructure Mapping
| Logical Component | Infrastructure Mapping |
|---|---|
| StorePersistenceService | `auth-module` container process |
| StoreWriteCoordinator (per-store queues) | In-process workers in `auth-module` |
| EncryptionService / IntegrityService / KeyProvider | In-process services in `auth-module`; keys from mounted secrets |
| StoreIndexService | In-memory structures in `auth-module` |
| RecoveryOrchestrator | In-process background worker in `auth-module` |
| SecurityEventLogger | Serilog pipeline from `auth-module` stdout |
| RuntimeReleaseAdvisor | CI/release workflow check step |

## Deployment Topology (Text)
1. `auth-module` container runs on internal Docker network.
2. Persistent volume is mounted into `auth-module` for encrypted JSON stores.
3. Secret volume is mounted read-only for encryption/HMAC keys.
4. Backup scheduler container/job reads mounted store volume and writes encrypted archives to object storage.
5. Log collector/forwarder sends structured stdout logs to centralized sink.
6. OTel exporter emits metrics/traces to telemetry backend.

## Docker Compose Service Set
- `auth-module` (C# API + foundation services)
- `backup-scheduler` (scheduled backup/restore utility)
- `telemetry-agent` (optional local agent/exporter)

## Data Paths
- Store volume mount:
  - `/data/auth-store`
  - `/data/authz-store`
  - `/data/audit-store`
  - `/data/governance-store`
- Secret mount (read-only):
  - `/run/secrets/encryption-key`
  - `/run/secrets/hmac-key`

## Operational Flows

### Normal Write Flow
1. Request enters `auth-module`.
2. Write queued in per-store in-process queue.
3. Payload encrypted and signed.
4. Atomic file commit on mounted volume.
5. Index update and telemetry/log emission.

### Integrity Failure Flow
1. Integrity check fails in `auth-module`.
2. File moved to quarantine path.
3. Alert emitted through logging/monitoring pipeline.
4. Backup snapshot restored from object storage.
5. Integrity re-verified before unblocking store.

## Security Posture (UOW-01)
- No public ingress.
- Internal-only network communication.
- Application-level encryption and integrity controls for all store data.
- Secrets isolated in mounted files; no hardcoded credentials.

## Release Governance (Preview Runtime)
- Runtime preview usage is allowed in this phase.
- Release process requires explicit approval note and risk warning when preview runtime is detected.
