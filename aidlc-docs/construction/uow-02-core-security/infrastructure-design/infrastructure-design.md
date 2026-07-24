# Infrastructure Design — UOW-02 Core Auth and Authorization Control Plane

## Selected Infrastructure Decisions
| Question | Decision |
|---|---|
| Q1 Runtime hosting | Reuse same local Docker Compose stack as UOW-01 |
| Q2 Network exposure | Internal-only exposure in local network |
| Q3 MFA connectivity | Local mock MFA adapter only in this phase |
| Q4 Rate limit state | In-memory counters (single-instance assumption) |
| Q5 Token validation cache | In-process memory cache only |
| Q6 Approval persistence support | Existing UOW-01 JSON store volume |
| Q7 Lockout alert delivery | Structured log event only |
| Q8 Shared strategy | Reuse shared baseline; isolate runtime config and data paths |
| Q9 Environment topology | Dev only |
| Q10 Runtime governance | Allow preview runtime with explicit approval note + risk warning |

---

## 1. Deployment and Compute Mapping

- **Runtime**: `AuthControlPlaneService` as a container in the existing local Compose stack.
- **Topology**: Single instance for this unit in dev.
- **Container policy**:
  - pinned image tags only,
  - read-only secret mounts following shared conventions,
  - mounted persistent volume for unit-owned JSON data paths.

---

## 2. Storage and State Mapping

### Persistent State
- Approval workflow persistence reuses the shared JSON persistence volume established in UOW-01.
- UOW-02 data remains namespace-isolated under unit-specific paths and schema boundaries.

### Ephemeral State
- Rate-limit fixed-window counters are in-process memory structures.
- Token validation cache (active/inactive + token version) is in-process memory with short TTL.

### Consequences
- This design is correct for single-instance dev and preserves low latency.
- Horizontal scaling is intentionally deferred; distributed counters/cache are future evolution items.

---

## 3. Networking and Exposure

- Service remains on internal Docker network only.
- No public ingress, no internet-facing gateway, and no external TLS termination in this unit.
- Protected endpoint access for testing/integration is via internal network routes only.

---

## 4. MFA Integration Infrastructure

- UOW-02 uses a **local mock MFA adapter** only.
- No external provider call path is required in this phase.
- Runtime wiring still preserves synchronous verification control flow so the adapter can later be swapped to external HTTPS integration without changing auth pipeline semantics.

---

## 5. Observability and Alerting

- Reuses shared Serilog + OpenTelemetry observability baseline.
- Lockout threshold events are emitted as structured security log records only.
- No direct notification transport (webhook/email/incident tool) is introduced in this phase.

Required log attributes for lockout events:
- `eventType`
- `accountId` (or anonymized identifier per policy)
- `windowStartUtc`
- `windowDurationSeconds`
- `lockoutCount`
- `correlationId`

---

## 6. Shared Infrastructure and Isolation

Shared baseline reused from `aidlc-docs/construction/shared-infrastructure.md`:
- internal network,
- secret mount conventions,
- observability pipeline.

Isolation constraints for UOW-02:
- no cross-unit direct data-path access,
- unit-scoped configuration section for auth control plane settings,
- unit-owned persistence prefixes for approval/audit-related state.

---

## 7. Runtime Preview Governance

- Runtime policy remains aligned with UOW-01:
  1. preview runtime allowed for dev progression,
  2. explicit approval note required in release record,
  3. risk warning must be included in release summary.

This maintains continuity while preserving a documented gate before any production-go-live decision.

