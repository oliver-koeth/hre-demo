# Build and Test Summary

## Scope
This package covers build and test instructions for completed construction units:
- UOW-01 Foundation
- UOW-02 Core Security
- UOW-03 Governance
- UOW-04 Integration Readiness
- UOW-05 User Search

## Instruction Set Generated
- `build-instructions.md`: full solution restore/build sequence and troubleshooting
- `unit-test-instructions.md`: per-project unit test execution path
- `integration-test-instructions.md`: cross-unit contract/readiness verification + HTTP integration tests (WebApplicationFactory)
- `performance-test-instructions.md`: executable micro-latency + light-concurrency performance test flow
- `performance-traceability.md`: requirement-to-performance-test mapping and evidence locations
- `security-test-instructions.md`: security tests + open-source scan execution
- `security-traceability.md`: requirement-to-security-test/scan mapping
- `pages-deployment.md`: GitHub Pages build/deploy and walkthrough link-validation flow

## Quality Gate Criteria
- Build completes with zero errors
- Unit tests pass across all four unit test projects
- Integration tests pass for conformance, traceability, runtime artifact, and gate-decision behavior
- HTTP integration tests pass across all 35 test cases covering the full API surface
- Performance tests pass for NFR-PERF-001, NFR-PERF-003, and NFR-U02-018 with telemetry artifacts captured
- Security tests pass for token-validation negative paths and approval actor enforcement
- Security scans pass (CodeQL, NuGet vulnerabilities, Trivy, Gitleaks)

## Known Constraints
- Performance baselines are local/CI comparative signals; they are not a substitute for external high-load capacity testing.
