# Infrastructure Design — UOW-04 Integration Readiness and Quality Gate

## Selected Infrastructure Decisions
| Question | Decision |
|---|---|
| Q1 Runtime hosting | Reuse same local Docker Compose stack as UOW-01..UOW-03 |
| Q2 Gate invocation | Internal-only API endpoint |
| Q3 Contract-check source | Repository source and route definitions at evaluation time |
| Q4 Gate evidence persistence | Existing JSON store volume under integration namespace |
| Q5 Blocker registry storage | Persisted JSON records in existing data volume |
| Q6 Artifact validation scope | Direct filesystem checks for required files in repo root |
| Q7 Alert delivery | Structured log/security event only |
| Q8 Shared strategy | Reuse shared baseline, isolate integration artifacts by namespace |
| Q9 Environment topology | Dev only |
| Q10 Runtime governance | Allow preview runtime with explicit approval note + risk warning |

---

## 1. Deployment and Compute Mapping
- **Runtime**: `IntegrationGateService` executes in the existing local Compose ecosystem.
- **Execution surface**: internal API route only (no public ingress).
- **Topology**: single-instance service behavior in development.
- **Compute profile**: low-throughput control-plane workflow (5 evaluations/minute target).

---

## 2. Contract and Traceability Check Inputs
- Contract conformance checks read repository source and route definitions at evaluation time.
- No pre-generated external manifest is required in V1.
- Traceability checks read story-to-file evidence data from repository artifacts.

Consequences:
- Ensures checks are aligned with current workspace state.
- Avoids drift between runtime checks and committed source definitions.

---

## 3. Storage and Durability Mapping

### Gate Evidence Store
- Append-only gate run records are stored in the existing JSON persistence volume under an integration namespace.
- Each record includes decision status, short note, and retention-expiry metadata (90-day policy).

### Blocking Findings Registry
- Open blocking findings are persisted as JSON records in the same integration namespace.
- Re-runs consume persisted blockers to ensure consistent fail/pass behavior across executions.

---

## 4. Networking and Exposure
- UOW-04 reuses shared internal Docker network conventions.
- Integration gate invocation remains internal-only.
- No external gateway, internet-facing route, or public load-balancer integration is added in this phase.

---

## 5. Artifact Validation and Alerting Infrastructure

### Required Artifact Presence Checks
- Direct filesystem checks validate:
  1. `docker-compose.yml`
  2. `config/policy.template.json`
- Missing artifact creates explicit blocking finding.

### Alert Delivery
- Any gate failure emits structured operational/security events into the existing observability pipeline.
- No webhook, queue, or dedicated notification transport is introduced in this stage.

---

## 6. Shared Infrastructure and Isolation
Reused baseline from `aidlc-docs/construction/shared-infrastructure.md`:
- shared internal networking,
- shared observability pipeline,
- shared secret and volume conventions.

UOW-04 isolation:
- integration evidence and blocker registry paths are namespaced,
- no cross-unit direct data-path ownership transfer,
- runtime checks remain read-oriented over existing unit artifacts.

---

## 7. Runtime Preview Governance
- Preview runtime handling remains aligned with prior units:
  1. allowed for current dev progression,
  2. explicit approval note required,
  3. risk warning required in release/governance documentation.
