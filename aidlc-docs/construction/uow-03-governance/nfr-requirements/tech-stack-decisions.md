# Tech Stack Decisions — UOW-03 Governance and Evidence Domains

## Baseline Stack Reuse
- **Language/runtime**: C# on .NET (existing project baseline).
- **Deployment**: Docker-based local/internal environment.
- **Persistence**: JSON file persistence (single durable store boundary).
- **Observability**: Structured logging with correlation metadata.

---

## UOW-03 Decision Set

### TSD-U03-001 Audit Query Surface Scope
- Keep V1 query service constrained to security-event data only.
- Rationale: aligns with selected throughput target and minimized initial compliance surface.

### TSD-U03-002 Evidence Export Serialization
- Export format is JSON-only; no CSV in V1.
- Rationale: machine-readable compliance packages with minimal serialization complexity.

### TSD-U03-003 Retention Invocation Execution Model
- Use manual API-triggered retention evaluation with at-least-once processing.
- Rationale: explicit operator control and deterministic idempotence semantics without scheduler dependency.

### TSD-U03-004 Legal-Hold Enforcement Pattern
- Enforce hard fail-closed checks in application layer before any data-output action.
- Rationale: consistent compliance posture and no unsafe override paths.

### TSD-U03-005 Incident Durability Boundary
- Treat primary JSON store write acknowledgement as durability gate for incident create/update operations.
- Rationale: consistent with current persistence architecture and selected reliability level.

### TSD-U03-006 Alerting Channel
- Immediate alert conditions emit structured operational events in the existing logging pipeline.
- Rationale: reuse of current telemetry path and no additional messaging infrastructure in V1.

---

## Deferred Decisions (Explicitly Out of Scope in V1)
- Exactly-once retention processing.
- Secondary replicated durability proofs for incident records.
- CSV/dual-format evidence exports.
- Automated retention scheduler.
- Expanded PBT scope beyond idempotence.

