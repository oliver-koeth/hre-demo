# Performance Test Instructions

## Purpose
Validate local performance baselines without a dedicated load rig by measuring:
- micro-latency for a hot authorization path
- light concurrency behavior for health/readiness traffic
- basic runtime bottleneck indicators (p95/p99/max, throughput, GC collection deltas)

## Implemented Performance Requirements
- **NFR-PERF-001**: authorization evaluation stays within local p95 budget for sequential requests.
- **NFR-PERF-003**: health endpoint sustains light parallel traffic with zero failures and bounded p95 latency.
- **NFR-U02-018**: user search stays within local p99 budget for up to 100 returned results.

## Test Artifacts
- **Project**: `tests/AuthModule.ServiceHost.Tests/AuthModule.ServiceHost.Tests.csproj`
- **Telemetry output**: JSON files in `PERF_RESULTS_DIR` (or `bin/**/perf-artifacts` by default)
- **CI visualization**: GitHub Actions Step Summary table + uploaded JSON artifacts

## Run Performance Tests Locally

### 1. Build once
```bash
dotnet build AuthModule.slnx --configuration Release --nologo
```

### 2. Run micro-latency test
```bash
PERF_RESULTS_DIR="$(pwd)/artifacts/perf" dotnet test tests/AuthModule.ServiceHost.Tests/AuthModule.ServiceHost.Tests.csproj --configuration Release --no-build --filter "PerfType=Micro" --nologo
```

### 3. Run user search micro-latency test
```bash
PERF_RESULTS_DIR="$(pwd)/artifacts/perf" dotnet test tests/AuthModule.ServiceHost.Tests/AuthModule.ServiceHost.Tests.csproj --configuration Release --no-build --filter "PerfType=Micro" --nologo
```

### 4. Run light-concurrency test
```bash
PERF_RESULTS_DIR="$(pwd)/artifacts/perf" dotnet test tests/AuthModule.ServiceHost.Tests/AuthModule.ServiceHost.Tests.csproj --configuration Release --no-build --filter "PerfType=Concurrency" --nologo
```

### 5. Inspect telemetry
Each JSON artifact includes:
- `RequirementId`, `Scenario`, `Endpoint`
- `P50Ms`, `P95Ms`, `P99Ms`, `MaxMs`
- `ThroughputPerSecond`
- `FailedRequests`
- `Gen0Collections`, `Gen1Collections`, `Gen2Collections`

## Bottleneck Interpretation Guide
- **High p95 + low p50**: tail-latency spikes, likely contention or intermittent I/O stalls.
- **Throughput drop with stable latency**: thread-pool or connection saturation.
- **GC deltas jump with latency growth**: allocation pressure in request path.
- **Error count > 0**: treat as blocking regardless of latency.
