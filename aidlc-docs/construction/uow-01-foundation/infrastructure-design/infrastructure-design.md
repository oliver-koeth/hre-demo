# Infrastructure Design — UOW-01 Foundation and Persistence Baseline

## Selected Infrastructure Decisions
| Question | Decision |
|---|---|
| Q1 Deployment environment | Local Docker Compose only |
| Q2 Compute | Docker Compose service (single host) |
| Q3 Storage | Mounted persistent volume for encrypted JSON stores |
| Q4 Backup/restore | Scheduled encrypted backup snapshots to object storage |
| Q5 Messaging | In-process queues only |
| Q6 Networking | Internal network only, no public ingress in UOW-01 |
| Q7 Monitoring | Serilog stdout + OpenTelemetry metrics/traces + centralized log sink |
| Q8 Shared strategy | Shared base infrastructure across UOWs with namespace boundaries |
| Q9 Topology | Dev environment only |
| Q10 Preview runtime release control | Allow promotion with explicit approval note + risk warning |

---

## 1. Deployment and Compute Mapping

### Runtime Platform
- **Compute host**: Local machine / dev host.
- **Orchestration**: Docker Compose.
- **Service shape**: Single API container for UOW-01 foundation runtime.
- **Environment scope**: Dev only (no staging/prod manifests in this unit).

### Container Requirements
- Pin explicit image tags (no `latest`).
- Mount secret files for encryption/HMAC keys into read-only container paths.
- Mount persistent volume for JSON store directories.
- Expose service only on internal Docker network.

---

## 2. Storage and Data Protection Mapping

### Primary Store
- **Type**: Encrypted JSON files on mounted persistent volume.
- **Paths (logical)**:
  - `auth-store/`
  - `authz-store/`
  - `audit-store/`
  - `governance-store/`
- **Encryption**: AES-256-GCM at application layer.
- **Integrity**: HMAC-SHA256 sidecar signatures.

### Backup Strategy
- Scheduled backup job creates encrypted archives from mounted volume.
- Backup archives are pushed to object storage bucket/container.
- Backup manifest includes timestamp, store set, schema version markers, and integrity metadata.
- Recovery workflow supports quarantine + restore for integrity failures.

---

## 3. Messaging and Concurrency Mapping

### Messaging
- No external broker in UOW-01.
- Per-store write queues are implemented in-process.
- Recovery orchestration jobs are internal/background in process.

### Concurrency
- Per-store queue serializes writes for deterministic ordering.
- Record-level optimistic concurrency remains enforced in application logic.

---

## 4. Networking and Exposure

### Network Topology
- Single internal Docker network for all UOW-01 containers.
- No public load balancer, API gateway, or internet-exposed ingress in UOW-01.
- Direct internal service exposure only for local integration and tests.

### Security Controls
- Secrets mounted from filesystem; never injected as hardcoded values.
- Internal-only network plus app-level deny-by-default access rules.

---

## 5. Monitoring and Observability

### Logging
- Serilog writes structured logs to stdout.
- Log routing collects stdout to centralized sink.
- Required fields: timestamp, correlation ID, level, message, component, event type.

### Telemetry
- OpenTelemetry metrics and traces enabled for:
  - read/write latency histograms
  - retry counters
  - integrity failure counters
  - restore attempt/success/failure counters

### Alerts (Dev Baseline)
- Integrity verification failure.
- Key loading failure at startup.
- Backup job failure.
- Repeated optimistic concurrency conflict bursts.

---

## 6. Shared Infrastructure Boundaries

Shared base infrastructure is defined for downstream units:
- Shared Docker network.
- Shared observability pipeline (Serilog + OTel exporter + centralized sink).
- Shared secret-mounting convention.

Namespace boundaries remain strict:
- Each unit keeps its own store paths and service-specific config sections.
- Downstream units reuse shared foundation without cross-unit store coupling.

---

## 7. Runtime Preview Release Governance

- Current runtime remains .NET 10 preview for this phase.
- Promotion is allowed only with:
  1. Explicit approval note in release record.
  2. Runtime risk warning attached to release summary.
- Governance requirement: release note must include recommended migration to supported GA/LTS runtime before production go-live.
