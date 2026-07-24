# Infrastructure Design — UOW-03 Governance and Evidence Domains

## Selected Infrastructure Decisions
| Question | Decision |
|---|---|
| Q1 Runtime hosting | Reuse same local Docker Compose stack as UOW-01/UOW-02 |
| Q2 API exposure | Internal-only local network exposure |
| Q3 Retention execution | Synchronous API-triggered operation in GovernanceService |
| Q4 Idempotence fingerprint storage | Existing JSON store volume (governance namespace) |
| Q5 Incident durability support | Existing single JSON store volume with strict write semantics |
| Q6 Backup metadata freshness | Immediate append + in-process cache invalidation |
| Q7 Alert delivery | Structured log/security event only |
| Q8 Shared strategy | Reuse shared baseline, isolate governance config/data paths |
| Q9 Environment topology | Dev only |
| Q10 Runtime governance | Allow preview runtime with explicit approval note + risk warning |

---

## 1. Deployment and Compute Mapping

- **Runtime**: `GovernanceService` container added to existing Compose stack.
- **Topology**: single-instance in dev (aligned with current unit assumptions).
- **Execution model**:
  - retention invocation executes synchronously in request path,
  - audit/evidence/data-subject/incident/backup modules run in the same runtime process.

Container policy:
- pinned image tags,
- read-only secret mounts via shared conventions,
- mounted persistent volume for governance data namespace.

---

## 2. Storage and Durability Mapping

### Persistent State
- Governance evidence, retention fingerprints, lifecycle decisions, incident records, and backup metadata persist in UOW-01 JSON storage boundary under unit-isolated governance paths.
- Idempotence fingerprints are persisted in the same governance namespace to preserve deterministic repeat behavior across restarts.

### Durability Semantics
- Incident writes enforce synchronous durable commit semantics (including fsync boundary before success response).
- Retention invocation failures are fail-closed and do not return partial success.

### Consequences
- This fits the current dev-only single-store model and preserves deterministic behavior.
- Multi-store replication/high-availability persistence remains deferred.

---

## 3. Networking and Exposure

- Governance endpoints are internal-only on shared Docker internal network.
- No public ingress, external gateway, or internet-facing exposure in this phase.
- Query/export operations are reachable only by internal callers for integration/testing.

---

## 4. Retention, Freshness, and Alerting Infrastructure

### Retention Invocation Surface
- API-triggered synchronous operation in GovernanceService process.
- No async queue/worker path in this phase.

### Backup Metadata Freshness
- Metadata is appended immediately to persistence.
- In-process read-through cache invalidation keeps query visibility within the 30-second target.

### Alert Delivery
- Immediate alerts for retention failures and blocked hold-bypass attempts are emitted as structured security/operational log events only.
- No webhook/email/queue transport added in this unit.

---

## 5. Shared Infrastructure and Isolation

Reused shared baseline from `aidlc-docs/construction/shared-infrastructure.md`:
- internal Docker network,
- observability pipeline (structured logging + telemetry exporters),
- secret file mount conventions.

Isolation boundaries for UOW-03:
- unit-scoped configuration for governance runtime,
- governance-owned store prefixes and schemas,
- no cross-unit direct file-path access.

---

## 6. Runtime Preview Governance

- Runtime gate remains aligned with prior units:
  1. preview runtime allowed for dev progression,
  2. explicit approval note required in release record,
  3. risk warning required in release summary.

- Migration to supported GA/LTS runtime remains a pre-production governance requirement.

