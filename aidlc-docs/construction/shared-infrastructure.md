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
