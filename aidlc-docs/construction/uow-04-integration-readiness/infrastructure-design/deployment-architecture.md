# Deployment Architecture — UOW-04 Integration Readiness and Quality Gate

## Environment Scope
- **Active environment**: Development only.
- **Execution model**: Existing local Docker Compose stack reused from UOW-01..UOW-03.

## Deployable Components
| Component | Deployment Form | Network Scope | Notes |
|---|---|---|---|
| IntegrationGateService | Container/internal service process | Internal Docker network | Hosts integration gate execution endpoint and orchestration logic |
| Integration Gate JSON Namespace | Mounted shared data volume | Host-local | Stores append-only gate evidence and blocker registry records |
| Shared Observability Pipeline | Shared logging/telemetry path | Internal only | Receives gate failure structured events |

---

## Request and Processing Paths

### Gate Invocation
1. Internal caller invokes integration gate endpoint.
2. Service reads repository route/source definitions for contract checks.
3. Service evaluates contract conformance, traceability completeness, runtime artifacts, and blockers in canonical order.
4. Service persists gate result and notes to append-only evidence store.
5. If failed, immediate structured alert event is emitted.

### Re-run Path
1. Internal caller triggers gate re-run.
2. Service reads persisted unresolved blockers and current repository/runtime artifacts.
3. Service re-evaluates and returns deterministic pass/fail outcome.

---

## Configuration and Secret Topology
- Shared config mount conventions retained.
- Integration gate runtime reads policy/runtime files from repository root paths.
- No public TLS ingress configuration required in this phase due to internal-only invocation.

---

## Scaling and Evolution Notes
- Single-instance behavior in dev.
- No queue, cache, or worker fleet introduced in this stage.
- Future out-of-scope evolution:
  - scheduled gate execution,
  - externalized alert transports,
  - precomputed contract manifests for larger-scale environments.

---

## Release Governance Note
- Preview-runtime governance remains consistent with prior units: explicit approval note plus risk warning for release artifacts.
