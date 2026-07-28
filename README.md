# Authentication Module V1 (Backend)

This repository contains a backend-only authentication module for a reinsurance subledger system, implemented in **C#/.NET** with **JSON-file persistence** and a **Docker runtime**.

It includes four domain modules plus one host:
- **Foundation**: storage, crypto, integrity, telemetry primitives
- **Core Security**: login, token validation, authorization, MFA, user administration
- **Governance**: audit/evidence, retention, incidents, data-subject workflows
- **Integration**: conformance/traceability and readiness gate checks
- **ServiceHost**: ASP.NET minimal API host that exposes all module endpoints

## AI-DLC (brief)

This project was developed using an AI-assisted development lifecycle (AI-DLC):
1. **Inception**: requirements, user stories, architecture, and unit planning
2. **Construction**: per-unit design and implementation (UOW-01 to UOW-04)
3. **Build & Test**: validation and readiness handoff artifacts
4. **Operations**: deployment/operations placeholder and follow-up guidance

Detailed artifacts and the clickable walkthrough are in `aidlc-docs/`.

## Build and run locally

### Prerequisites
- .NET 10 SDK
- Docker + docker-compose
- `openssl` (for local secret generation)

### Option A: Run with Docker (recommended)
```bash
./scripts/dev-up.sh
```

This script:
1. Generates local secrets in `secrets/` (if missing)
2. Builds and starts the API container via `docker-compose`

Stop with:
```bash
docker-compose down
```

### Option B: Run directly with dotnet
```bash
./scripts/generate-dev-secrets.sh
POLICY_CONFIG_PATH=config/policy.local.json dotnet run --project src/AuthModule/ServiceHost/ServiceHost.csproj --urls http://localhost:8080
```

## API docs and key endpoints

After startup:
- Swagger UI: `http://localhost:8080/docs`
- OpenAPI JSON: `http://localhost:8080/openapi/v1.json`
- Service status: `http://localhost:8080/`
- Foundation health: `http://localhost:8080/internal/foundation/health`

## Build and test

Build:
```bash
dotnet build AuthModule.slnx
```

Run tests:
```bash
dotnet test AuthModule.slnx
```

Run performance tests (micro-latency + light concurrency):
```bash
PERF_RESULTS_DIR="$(pwd)/artifacts/perf" dotnet test tests/AuthModule.ServiceHost.Tests/AuthModule.ServiceHost.Tests.csproj --filter "PerfType=Micro|PerfType=Concurrency"
```

## Walkthrough for colleagues

Open the static walkthrough:
- `aidlc-docs/v1-ai-dlc-walkthrough.html`

Serve it locally with auto-restart on changes:
```bash
./scripts/serve-walkthrough.sh
```
