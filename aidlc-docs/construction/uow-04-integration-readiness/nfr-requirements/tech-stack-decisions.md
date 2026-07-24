# Tech Stack Decisions — UOW-04 Integration Readiness and Quality Gate

## Baseline Stack Reuse
- **Language/runtime**: C# on .NET (existing baseline).
- **Deployment context**: Docker-based internal runtime.
- **Persistence context**: JSON-backed project artifacts and configuration.
- **Observability channel**: Structured logging with correlation metadata.

---

## UOW-04 Decision Set

### TSD-U04-001 Gate Execution Model
- Keep gate evaluation as a lightweight application-level workflow with deterministic checks.
- Rationale: integration scope requires orchestration, not new domain runtime surfaces.

### TSD-U04-002 Throughput/Latency Targeting
- Optimize for low-frequency, high-confidence gate runs: 5 evaluations/minute with P95 < 15s.
- Rationale: this unit is a quality gate, not a high-volume transactional service.

### TSD-U04-003 Strict Blocking Policy
- Enforce hard fail for any blocking finding with no bypass mechanism.
- Rationale: preserves release integrity and compliance posture.

### TSD-U04-004 Error Contract Consistency Scope
- Validate RFC7807 + `errorCode` + `correlationId` across all unit endpoints.
- Rationale: consistent cross-unit failure semantics for auditability and supportability.

### TSD-U04-005 Runtime Validation Depth
- Use presence-only checks for `docker-compose.yml` and `config/policy.template.json` in V1.
- Rationale: aligns with selected NFR scope and avoids introducing environment-overlay complexity.

### TSD-U04-006 Alerting and Evidence Retention
- Emit immediate alert on gate failure through existing structured logs and retain gate evidence for 90 days.
- Rationale: reuses existing observability path while preserving operational decision traceability.

### TSD-U04-007 PBT Strategy
- No new PBT in UOW-04; rely on evidence from prior units.
- Rationale: UOW-04 is integration-readiness control logic, with low algorithmic novelty versus UOW-02/UOW-03.

---

## Deferred Decisions (Out of Scope in V1)
- Environment-overlay topology validation.
- Emergency override workflow for blocking findings.
- New dedicated gate-metrics pipeline beyond existing logs.
- New UOW-04-specific property-based test suites.
