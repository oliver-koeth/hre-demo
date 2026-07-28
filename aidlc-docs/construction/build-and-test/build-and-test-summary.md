# Build and Test Summary

## Scope
This package covers build and test instructions for completed construction units:
- UOW-01 Foundation
- UOW-02 Core Security
- UOW-03 Governance
- UOW-04 Integration Readiness

## Instruction Set Generated
- `build-instructions.md`: full solution restore/build sequence and troubleshooting
- `unit-test-instructions.md`: per-project unit test execution path
- `integration-test-instructions.md`: cross-unit contract/readiness verification + HTTP integration tests (WebApplicationFactory)
- `performance-test-instructions.md`: baseline performance validation approach for integration gate

## Quality Gate Criteria
- Build completes with zero errors
- Unit tests pass across all four unit test projects
- Integration tests pass for conformance, traceability, runtime artifact, and gate-decision behavior
- HTTP integration tests pass across all 25 test cases covering the full API surface
- Performance baseline remains within UOW-04 targets

## Known Constraints
- Performance guidance currently relies on repeated deterministic test execution; dedicated benchmarking harness can be added later if required.
