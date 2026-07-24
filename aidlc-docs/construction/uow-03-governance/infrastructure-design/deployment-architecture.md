# Deployment Architecture — UOW-03 Governance and Evidence Domains

## Environment Scope
- **Active environment**: Development only.
- **Execution model**: Existing local Docker Compose stack reused from UOW-01/UOW-02.

## Deployable Components
| Component | Deployment Form | Network Scope | Notes |
|---|---|---|---|
| GovernanceService | Container | Internal Docker network | Hosts audit query, evidence, retention, incident, backup evidence modules |
| Shared JSON Persistence Volume | Mounted volume | Host-local | Stores governance namespace data and fingerprints |
| Shared Observability Pipeline | Shared logging/telemetry path | Internal only | Receives structured alert/security events |

---

## Request and Processing Paths

### Audit/Evidence APIs
1. Internal caller invokes governance endpoint.
2. GovernanceService applies legal-hold policy guard where required.
3. Module executes query/export logic.
4. Response returns with correlation metadata.

### Retention Invocation
1. Internal operator/caller triggers retention API.
2. Retention module evaluates records synchronously.
3. On failure: immediate fail response + structured alert event.
4. On success: lifecycle outcomes persisted.

### Incident Writes
1. Caller submits incident mutation.
2. GovernanceService writes to JSON store with strict durable commit boundary.
3. Success response only after durability confirmation.

---

## Configuration and Secret Topology
- Shared secret mount convention retained (`/run/secrets/...`).
- Governance-specific config remains isolated under unit namespace.
- No public TLS ingress config required for this phase due to internal-only exposure.

---

## Scaling and Evolution Notes
- Single-instance service assumption for dev.
- No queue, event bus, or external cache introduced in this stage.
- Future staged evolution (out of scope here): async export/retention workers, multi-instance coherence mechanisms, replicated durability targets.

---

## Release Governance Note
- Preview-runtime governance remains consistent with earlier units: explicit approval note plus risk warning for release artifacts.

