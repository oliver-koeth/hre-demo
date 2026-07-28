# Performance Test Traceability Matrix

## Purpose
Map performance-focused non-functional requirements to executable tests and telemetry evidence.

## Requirement-to-Test Mapping

| Requirement ID | Requirement intent | Test case | Measurement/evidence |
|---|---|---|---|
| NFR-PERF-001 | Bound sequential authorization response time for local baseline runs | `MicroLatencyPerformanceTests.NFR_PERF_001_AuthorizationEvaluate_ShouldStayWithinLocalLatencyBudget` | p50/p95/p99/max latency, throughput, failed request count, GC deltas |
| NFR-PERF-003 | Ensure light in-process concurrency remains stable without failures | `ConcurrencyPerformanceTests.NFR_PERF_003_HealthEndpoint_ShouldHandleLightConcurrencyWithinBudget` | p50/p95/p99/max latency, throughput, failed request count, GC deltas |

## Evidence Locations
- Local runs: `PERF_RESULTS_DIR` (recommended `artifacts/perf`)
- CI runs: uploaded artifact `performance-telemetry` and GitHub Actions Step Summary table

## Traceability Rules
1. Performance tests must keep requirement IDs in test method names and telemetry payloads.
2. Any new performance NFR requires a matching automated test and matrix entry.
3. Threshold changes must be recorded in this file and in test assertions.
