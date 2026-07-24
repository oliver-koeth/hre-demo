# Performance Test Instructions

## Purpose
Validate that integration-gate control-plane behavior remains within selected UOW-04 NFR bounds.

## Performance Requirements
- **Gate Decision Latency**: P95 < 15 seconds
- **Gate Throughput**: 5 evaluations/minute
- **Concurrent Requests**: bounded parallel requests in-process
- **Error Rate**: 0% for healthy baseline inputs

## Setup Performance Test Environment

### 1. Prepare Environment
```bash
dotnet build AuthModule.slnx --no-restore
```

### 2. Configure Test Parameters
- **Test Duration**: 5 minutes
- **Ramp-up**: linear to target request rate
- **Virtual Users**: enough to sustain 5 evaluations/minute

## Run Performance Tests

### 1. Execute Baseline Loop (lightweight script approach)
```bash
for i in {1..25}; do dotnet test tests/AuthModule.Integration.Tests/AuthModule.Integration.Tests.csproj --nologo --filter "FullyQualifiedName~IntegrationGateTests.GateOutcome_ShouldBeDeterministic_ForUnchangedInputs"; done
```

### 2. Execute Stress Variant (artifact/conformance perturbation)
```bash
dotnet test tests/AuthModule.Integration.Tests/AuthModule.Integration.Tests.csproj --nologo --filter "FullyQualifiedName~IntegrationGateTests"
```

### 3. Analyze Results
- **Latency**: compare observed run durations to target
- **Throughput**: confirm stable completion rate at target gate frequency
- **Error Rate**: confirm no unexpected failures in deterministic baseline
- **Bottlenecks**: investigate file I/O and route-source parsing if latency degrades

## Optimization Guidance
1. reduce repeated file reads via controlled caching only if determinism is preserved
2. tighten checker scope to required endpoints/stories
3. rerun tests after each optimization
