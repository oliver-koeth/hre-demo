# Integration Test Instructions

## Purpose
Validate interaction boundaries across units (Foundation, CoreSecurity, Governance, Integration Gate) and ensure cross-unit contracts remain consistent. Also validates the full HTTP API surface via in-process integration tests.

## Part 1: Cross-Unit / Gate Tests

These tests run in-process without Docker and validate RFC7807 contract conformance, integration gate readiness decisions, and traceability coverage.

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

### Run Cross-Unit / Gate Tests
```bash
dotnet test tests/AuthModule.Integration.Tests/AuthModule.Integration.Tests.csproj --nologo
```

---

## Part 2: HTTP Integration Tests (WebApplicationFactory)

These tests start the full `ServiceHost` in-process using `WebApplicationFactory<Program>` and exercise every API endpoint over real HTTP using `HttpClient`. No Docker or external services are required — each test class gets its own isolated temp directory with generated AES/HMAC keys.

### Coverage
| Test class | Endpoints covered |
|---|---|
| `ServiceStatusTests` | `GET /` — service status |
| `FoundationDiagnosticsTests` | `GET /api/health`, `POST /api/governance/integrity` |
| `OpenApiTests` | `GET /openapi/v1.json`, `GET /docs`, CORS preflight |
| `AuthenticationTests` | `POST /api/auth/login` (bad credentials, non-existent user, malformed token, lockout) |
| `AuthorizationTests` | `POST /api/authz/evaluate` |
| `MfaTests` | `POST /api/mfa/challenge`, `POST /api/mfa/verify` |
| `UserAdminTests` | `POST /api/users`, `PUT /api/users/{id}`, `POST /api/users/{id}/disable` |
| `GovernanceTests` | `GET /api/governance/audit/security-events`, `POST /api/governance/evidence`, `POST /api/governance/evidence/export`, `POST /api/governance/incidents`, `PUT /api/governance/incidents/{id}/status`, `POST /api/governance/backups/metadata`, `PUT /api/governance/backups/{id}/status` |

### Run HTTP Integration Tests
```bash
dotnet test tests/AuthModule.ServiceHost.Tests/AuthModule.ServiceHost.Tests.csproj --nologo
```

### Run HTTP Security-Focused Tests
```bash
dotnet test tests/AuthModule.ServiceHost.Tests/AuthModule.ServiceHost.Tests.csproj --filter "Security=True" --nologo
```

Security-focused HTTP scenarios include:
- Approval workflow endpoints reject requests without authenticated actor headers.
- Approval decision endpoint rejects unauthenticated approval attempts.

### Run All Integration Tests
```bash
dotnet test tests/AuthModule.Integration.Tests/ tests/AuthModule.ServiceHost.Tests/ --nologo
```

### Test Isolation
- Each `IClassFixture<ServiceHostFactory>` creates its own `TestPolicyConfig` with a unique temp directory and generated AES/HMAC keys.
- Host initialization is serialized with a `SemaphoreSlim` so parallel test classes don't race on environment variable injection.
- Temp directories are deleted in `DisposeAsync`.
