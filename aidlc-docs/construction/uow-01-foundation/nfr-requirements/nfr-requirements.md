# NFR Requirements — UOW-01 Foundation and Persistence Baseline

## Scope
This document defines non-functional requirements for the UOW-01 foundation layer: shared contracts, JSON persistence gateways, encryption/integrity controls, and startup configuration bootstrapping.

## Baseline Constraints Carried from INCEPTION
- Language: C#
- Runtime: Docker
- Persistence: JSON file stores
- DR Strategy: Warm Standby (RTO/RPO in minutes)
- CI/CD: GitHub Actions
- Deployment style: Blue/Green with swap-back rollback
- Security baseline: deny-by-default, adaptive password hashing, secrets from mounted files

## Unit-Specific NFR Requirements

### 1. Runtime and Platform
- **NFR-UOW01-001**: The service targets **.NET 10 preview** for development in this unit.
- **NFR-UOW01-002**: Production promotion is **blocked** until runtime is moved to a supported GA/LTS/STS release and validated in downstream stages.
- **NFR-UOW01-003**: Container images must pin explicit base image tags (no `latest`) and be rebuilt on runtime patch updates.

### 2. Performance
- **NFR-UOW01-004**: Persistence read operations (`HMAC verify + decrypt + deserialize + query`) must meet **P99 < 100 ms** under baseline workload.
- **NFR-UOW01-005**: Persistence write operations (`serialize + encrypt + atomic write + HMAC sign`) must meet **P99 < 250 ms** under baseline workload.
- **NFR-UOW01-006**: Metrics must separately emit read/write latency histograms so both targets are measurable in CI/perf tests.

### 3. Security
- **NFR-UOW01-007**: All store files must be encrypted at rest using **AES-256-GCM** with keys loaded from mounted secret files.
- **NFR-UOW01-008**: All encrypted store files must carry sidecar integrity signatures using **HMAC-SHA256** and fail closed on signature mismatch.
- **NFR-UOW01-009**: Input and schema validation errors must return structured failures without leaking secrets, keys, plaintext credentials, or internal file paths.
- **NFR-UOW01-010**: Structured logging must be implemented with **Serilog**, and logs must include timestamp, level, correlation ID, and message.
- **NFR-UOW01-011**: Logging policy must redact or exclude password material, token contents, key material, and direct PII fields.

### 4. Reliability and Availability
- **NFR-UOW01-012**: Startup must fail fast when schema version is unsupported, integrity verification fails, or required key/config inputs are missing.
- **NFR-UOW01-013**: Store writes must be atomic (temp-file write + rename) so partial writes cannot produce accepted store state.
- **NFR-UOW01-014**: Optimistic concurrency conflicts must be explicit (`Conflict` result) and never silently overwrite newer state.
- **NFR-UOW01-015**: Integrity verification is repeatable and idempotent; repeated checks on unchanged files must produce identical outcomes.

### 5. Testability and Quality
- **NFR-UOW01-016**: Unit and component tests use **xUnit + FluentAssertions**.
- **NFR-UOW01-017**: Property-based testing uses **FsCheck** integrated with xUnit.
- **NFR-UOW01-018**: PBT execution must preserve shrinking and seed reproducibility, and failing seeds must be emitted in test output.
- **NFR-UOW01-019**: UOW-01 must carry both example-based and property-based tests for critical persistence and integrity behaviors.

### 6. Maintainability and Operability
- **NFR-UOW01-020**: JSON serialization/deserialization uses **System.Text.Json** with explicit options to avoid implicit format drift.
- **NFR-UOW01-021**: Shared abstractions (result contract, error codes, request context, repository interfaces) must remain stable and versioned for downstream units.
- **NFR-UOW01-022**: Store schema changes require explicit migration steps and migration idempotency verification.

## Compliance Trace Notes
- **PBT-09 satisfied** by selecting FsCheck and requiring integration/reproducibility controls.
- Security and resiliency controls remain inherited and explicitly bound to this unit's persistence design.
