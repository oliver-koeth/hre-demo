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
- `integration-test-instructions.md`: cross-unit contract/readiness verification flow
- `performance-test-instructions.md`: baseline performance validation approach for integration gate

## Quality Gate Criteria
- Build completes with zero errors
- Unit tests pass across all four unit test projects
- Integration tests pass for conformance, traceability, runtime artifact, and gate-decision behavior
- Performance baseline remains within UOW-04 targets

## Known Constraints
- Integration project is currently implemented as a library module; host wiring should be validated in runtime orchestration when host composition is introduced.
- Performance guidance currently relies on repeated deterministic test execution; dedicated benchmarking harness can be added later if required.
