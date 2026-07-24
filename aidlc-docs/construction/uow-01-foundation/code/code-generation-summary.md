# UOW-01 Code Generation Summary

## Created Application Code
- `src/AuthModule/Foundation/Domain/Primitives/RequestContext.cs`
- `src/AuthModule/Foundation/Domain/Primitives/Result.cs`
- `src/AuthModule/Foundation/Domain/Primitives/DomainError.cs`
- `src/AuthModule/Foundation/Domain/Primitives/IStoreEntity.cs`
- `src/AuthModule/Foundation/Domain/Entities/BaseStoreEntity.cs`
- `src/AuthModule/Foundation/Domain/Entities/AuthEntities.cs`
- `src/AuthModule/Foundation/Domain/Entities/AuthzEntities.cs`
- `src/AuthModule/Foundation/Domain/Entities/AuditEntities.cs`
- `src/AuthModule/Foundation/Domain/Entities/GovernanceEntities.cs`
- `src/AuthModule/Foundation/Persistence/Serialization/StoreFileContracts.cs`
- `src/AuthModule/Foundation/Configuration/PolicyConfiguration.cs`
- `src/AuthModule/Foundation/Security/SecurityServices.cs`
- `src/AuthModule/Foundation/Persistence/Contracts/RepositoryContracts.cs`
- `src/AuthModule/Foundation/Persistence/Repositories/JsonRepositories.cs`
- `src/AuthModule/Foundation/Runtime/RuntimeServices.cs`
- `src/AuthModule/Foundation/Observability/ObservabilityExtensions.cs`
- `src/AuthModule/Foundation/Bootstrap/ServiceCollectionExtensions.cs`
- `src/AuthModule/Foundation/Api/DiagnosticsEndpoints.cs`

## Created Test Code
- `tests/AuthModule.Foundation.Tests/Domain/EntityInvariantTests.cs`
- `tests/AuthModule.Foundation.Tests/Persistence/TestSecrets.cs`
- `tests/AuthModule.Foundation.Tests/Persistence/RepositoryBehaviorTests.cs`
- `tests/AuthModule.Foundation.Tests/Persistence/StoreIntegrityServiceTests.cs`
- `tests/AuthModule.Foundation.Tests/PropertyBased/FoundationPropertyTests.cs`

## Deployment and Configuration Artifacts
- `Dockerfile`
- `docker-compose.yml`
- `config/policy.template.json`
- `scripts/backup.sh`

## Extension Rule Trace Notes
- **Security baseline**: encryption/integrity services, secret-file key loading, structured security events, fail-closed integrity handling.
- **Resiliency baseline**: retry policy, per-store write coordination, quarantine/restore orchestration hook, backup script.
- **PBT baseline**: FsCheck property tests for round-trip, idempotency, and invariant behaviors.

## Dependency Security Maintenance
- Upgraded OpenTelemetry packages in `src/AuthModule/Foundation/Foundation.csproj`:
  - `OpenTelemetry.Exporter.Console` `1.9.0` -> `1.17.0`
  - `OpenTelemetry.Extensions.Hosting` `1.9.0` -> `1.17.0`
  - `OpenTelemetry.Instrumentation.Runtime` `1.9.0` -> `1.17.0`
- Result: transitive `OpenTelemetry.Api` vulnerability (`GHSA-g94r-2vxg-569j`) is no longer reported.
- Normalized test dependency to remove restore warnings:
  - `FsCheck.Xunit` `3.1.6` -> `3.2.0`
