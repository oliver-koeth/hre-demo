# Integration Test Instructions

## Purpose
Validate interaction boundaries across units (Foundation, CoreSecurity, Governance, Integration Gate) and ensure cross-unit contracts remain consistent.

## Test Scenarios

### Scenario 1: CoreSecurity ↔ Governance error-contract consistency
- **Description**: Validate RFC7807 + `errorCode` + `correlationId` behavior across cross-unit API surfaces.
- **Setup**: build all units; ensure source files are present.
- **Test Steps**:
  1. run `dotnet test tests/AuthModule.Integration.Tests/AuthModule.Integration.Tests.csproj --nologo`
  2. inspect `ContractConformanceTests`
- **Expected Results**: conformance checks pass when required fields exist and fail with blocking findings otherwise.
- **Cleanup**: none.

### Scenario 2: Integration Gate readiness decision
- **Description**: Validate deterministic gate pass/fail and blocking behavior for missing artifacts or story coverage gaps.
- **Setup**: local repository with `docker-compose.yml` and `config/policy.template.json`.
- **Test Steps**:
  1. run `dotnet test tests/AuthModule.Integration.Tests/AuthModule.Integration.Tests.csproj --nologo`
  2. inspect `IntegrationGateTests`
- **Expected Results**: deterministic outcomes for unchanged inputs; fail-fast on blockers.
- **Cleanup**: none.

## Setup Integration Test Environment

### 1. Start Required Services (optional runtime validation)
```bash
docker-compose up -d auth-module-foundation auth-module-core-security auth-module-governance auth-module-integration
```

### 2. Configure Endpoints
```bash
export POLICY_CONFIG_PATH=config/policy.template.json
```

## Run Integration Tests

### 1. Execute Integration Test Suite
```bash
dotnet test tests/AuthModule.Integration.Tests/AuthModule.Integration.Tests.csproj --nologo
```

### 2. Verify Service Interactions
- **Scenarios covered**: conformance checks, traceability checks, artifact checks, gate decision persistence
- **Expected Results**: all integration tests pass
- **Logs Location**: test runner output and `tests/**/TestResults/` when available

### 3. Cleanup
```bash
docker-compose down
```
