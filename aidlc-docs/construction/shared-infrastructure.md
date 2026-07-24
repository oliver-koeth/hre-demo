# Shared Infrastructure Baseline (Construction Phase)

## Scope
Shared baseline used by UOW-01 and intended for reuse by downstream units (UOW-02..UOW-04).

## Shared Components
- Shared internal Docker network for service-to-service communication.
- Shared observability pipeline:
  - Serilog structured logging conventions
  - OpenTelemetry metrics/traces exporters
  - Centralized log sink destination
- Shared secret mount conventions:
  - `/run/secrets/encryption-key`
  - `/run/secrets/hmac-key`
- Shared backup archive destination pattern (object storage path namespace by unit/store).

## Isolation Boundaries
- Each unit owns its own store paths and schemas.
- No cross-unit direct file access is allowed.
- Unit-specific runtime configuration remains isolated by namespace prefix.

## Governance Notes
- Shared baseline can be extended by later units but must remain backward-compatible with UOW-01 assumptions.
- Any change to shared observability or secret conventions requires update of all dependent unit designs.

## UOW-02 Reuse Notes
- UOW-02 reuses the shared internal network, observability pipeline, and secret mounting conventions.
- UOW-02 keeps auth control plane runtime configuration and persistence paths isolated by unit namespace.
- UOW-02 lockout alerts are emitted via structured logs into the shared observability pipeline (no direct notification channel in this phase).

## UOW-03 Reuse Notes
- UOW-03 reuses shared internal networking, observability pipeline, and mounted secret-file conventions.
- UOW-03 keeps governance runtime configuration and persistence paths isolated by governance namespace.
- UOW-03 retention/hold alert events are emitted as structured logs into the shared observability pipeline (no direct notification transport in this phase).

## UOW-04 Reuse Notes
- UOW-04 reuses shared internal networking, observability pipeline, and shared data volume conventions.
- UOW-04 keeps integration gate evidence and blocker-registry records isolated by integration namespace.
- UOW-04 gate failure alerts are emitted as structured logs/security events into the shared observability pipeline (no direct notification transport in this phase).
