# Deployment Architecture — UOW-02 Core Auth and Authorization Control Plane

## Environment Scope
- **Active environment in this unit**: Development only.
- **Execution model**: Local Docker Compose stack shared with UOW-01 baseline.

## Deployable Components
| Component | Deployment Form | Network Scope | Notes |
|---|---|---|---|
| AuthControlPlaneService | Container | Internal Docker network | Hosts login, validation, authorization, approval, lockout, and MFA adapter flows |
| Local MFA Mock Adapter | In-process adapter endpoint/module | Internal only | Used for synchronous MFA verification behavior without external connectivity |
| Shared Observability Pipeline | Shared runtime sidecar/sink path | Internal only | Structured logs + traces/metrics exporters |
| JSON Persistence Volume | Mounted volume | Host-local | Stores unit-scoped persistent approval/audit/auth state |

---

## Request Path (Dev)
1. Internal caller invokes AuthControlPlaneService endpoint.
2. Service applies rate-limit check and token/auth pipeline.
3. For sensitive operations, service performs synchronous verification against local MFA mock adapter.
4. Approval operations persist synchronously to shared JSON volume with bounded retries.
5. Security events and alerts are emitted as structured logs to shared observability pipeline.

---

## Configuration and Secret Topology
- Secrets use shared mounted-file convention from baseline (`/run/secrets/...`).
- UOW-02 runtime configuration is namespaced and isolated from other units.
- No public certificate or ingress-controller configuration is required in this phase.

---

## Availability and Scale Assumptions
- Single-service-instance assumption in dev.
- In-memory rate-limit counters and token cache are instance-local by design.
- Multi-instance coherence (distributed cache/counters) is intentionally out of scope for this unit and reserved for later architecture evolution.

---

## Release Governance Note
- Deployment artifacts continue using preview-runtime policy consistent with UOW-01.
- Promotion requires explicit risk acknowledgement in release documentation.

