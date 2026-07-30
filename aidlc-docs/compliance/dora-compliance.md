# DORA Compliance Position Paper (Development & Local Integration Scope)

## 1. Document Control and Scope
- **Document Owner**: Engineering (Auth Module Team)
- **Version**: 0.1.0
- **Status**: Draft
- **Scope**: Development and local integration testing evidence for `hre-demo`; no production runtime/operations evidence is asserted here.
- **System**: Auth Module (`Foundation`, `CoreSecurity`, `Governance`, `Integration`, `ServiceHost`)

## 2. Regulatory Context (DORA)
This document captures a development-stage control posture aligned to DORA expectations for ICT risk management, secure software delivery, testing evidence, and traceability. It is intentionally limited to repository-backed controls and test evidence. Production operations controls are explicitly separated.

## 3. Compliance Posture Summary
| Area | Current Status | Notes |
|---|---|---|
| SDLC governance and approvals | Implemented | AI-DLC stage progression and approvals recorded in `aidlc-docs/aidlc-state.md` and `aidlc-docs/audit.md` |
| Secure change controls | Implemented | Build/test quality gates in `.github/workflows/ci-main.yml` |
| Vulnerability management | Implemented | CodeQL, NuGet SCA gate, Trivy, Gitleaks in `.github/workflows/security-scans.yml` |
| Security testing | Implemented | Security tests and traceability in `aidlc-docs/construction/build-and-test/security-traceability.md` |
| Resilience/performance evidence | Implemented | Performance tests and traceability in `aidlc-docs/construction/build-and-test/performance-traceability.md` |
| Incident/BCDR/ops monitoring controls | Partial | will be provided by operations |

## 4. System and Service Description
This service is a backend authentication module for a reinsurance subledger context, implemented in .NET 10 with JSON-file persistence and Docker runtime support. Core components:
- `Foundation`: storage, cryptography, integrity, telemetry primitives
- `CoreSecurity`: login, token validation, authorization, MFA, user administration
- `Governance`: evidence/audit workflows, retention, incidents, data-subject workflows
- `Integration`: conformance, traceability, and gate-readiness checks
- `ServiceHost`: ASP.NET minimal API exposing module endpoints

Evidence: `README.md`, `src/AuthModule/*`, `tests/*`.

## 5. SDLC Governance and Approval Gates
- AI-DLC lifecycle executed through Inception, Construction, and Build-and-Test handoff.
- Stage/unit approvals and sequencing are tracked in `aidlc-docs/aidlc-state.md`.
- Decision trail and approvals are recorded in `aidlc-docs/audit.md`.
- Construction outputs include explicit traceability matrices for security and performance controls.

Evidence: `aidlc-docs/aidlc-state.md`, `aidlc-docs/audit.md`, `aidlc-docs/construction/build-and-test/*.md`.

## 6. Secure Change and Release Controls
- `ci-main.yml` enforces restore, build, unit tests, property-based tests, compliance checks, security tests, HTTP integration tests, and performance checks on push to `main`.
- Pipeline fails on test failure or quality-gate regression.
- Performance telemetry is summarized in workflow step summary and stored as artifact.

Evidence: `.github/workflows/ci-main.yml`.

## 7. Vulnerability Management
- SAST: CodeQL analysis (`codeql` job).
- Dependency vulnerabilities: NuGet scan with JSON output plus fail-on-findings behavior.
- Container/filesystem vulnerability scanning: Trivy HIGH/CRITICAL, fail-on-findings.
- Secret scanning: Gitleaks in CI.
- On successful workflow completion, run metadata is appended to this document under **CI Security Scan Run Log**.

Evidence: `.github/workflows/security-scans.yml`.

## 8. Security Testing Evidence
- JWT negative-path validation tests (tampered token, expiry/issuer/audience/claim failures).
- Approval actor-enforcement tests at unit and HTTP layers.
- Security traceability maps requirements to tests and CI scans.

Evidence:
- `tests/AuthModule.CoreSecurity.Tests/Tokens/TokenSecurityNegativeTests.cs`
- `tests/AuthModule.CoreSecurity.Tests/Governance/ApprovalWorkflowTests.cs`
- `tests/AuthModule.ServiceHost.Tests/Api/ApprovalSecurityTests.cs`
- `aidlc-docs/construction/build-and-test/security-traceability.md`

## 9. Resilience and Performance Testing Evidence
- Micro-latency and light-concurrency tests run in CI and locally.
- Telemetry captures p50/p95/p99/max latency, throughput, failed request count, and GC deltas.
- Requirement-to-test mappings are maintained in performance traceability documentation.

Evidence:
- `tests/AuthModule.ServiceHost.Tests/Performance/MicroLatencyPerformanceTests.cs`
- `tests/AuthModule.ServiceHost.Tests/Performance/ConcurrencyPerformanceTests.cs`
- `tests/AuthModule.ServiceHost.Tests/Performance/PerformanceTelemetry.cs`
- `aidlc-docs/construction/build-and-test/performance-traceability.md`

## 10. Incident Detection, Response, and Regulatory Reporting
will be provided by operations

## 11. Business Continuity and Disaster Recovery
will be provided by operations

## 12. Operational Monitoring and Logging
will be provided by operations

## 13. Third-Party ICT Risk Management
will be provided by operations

## 14. Data Protection and Record Retention
will be provided by operations

## 15. Control Mapping Matrix
| Control ID | Control Objective | Implementation Summary | Evidence | Owner | Status |
|---|---|---|---|---|---|
| DORA-CONTROL-001 | Governed SDLC with explicit approval points | AI-DLC stage tracking and approval records | `aidlc-docs/aidlc-state.md`, `aidlc-docs/audit.md` | Engineering | Implemented |
| DORA-CONTROL-002 | Secure change gates before merge | CI runs build + multi-layer test gates on `main` pushes | `.github/workflows/ci-main.yml` | Engineering | Implemented |
| DORA-CONTROL-003 | Vulnerability and secret detection in delivery pipeline | CodeQL + NuGet SCA + Trivy + Gitleaks in CI | `.github/workflows/security-scans.yml` | Engineering | Implemented |
| DORA-CONTROL-004 | Security behavior verification | Automated security tests and traceability mapping | `tests/AuthModule.CoreSecurity.Tests/*`, `tests/AuthModule.ServiceHost.Tests/Api/ApprovalSecurityTests.cs`, `aidlc-docs/construction/build-and-test/security-traceability.md` | Engineering | Implemented |
| DORA-CONTROL-005 | Performance baseline and bottleneck visibility | Automated local/CI performance tests with telemetry artifacts | `tests/AuthModule.ServiceHost.Tests/Performance/*`, `.github/workflows/ci-main.yml`, `aidlc-docs/construction/build-and-test/performance-traceability.md` | Engineering | Implemented |
| DORA-CONTROL-006 | Incident management and regulatory reporting execution | Operational incident response and reporting processes | will be provided by operations | Operations | Partial |
| DORA-CONTROL-007 | Business continuity and disaster recovery execution | RTO/RPO, backup/restore, failover operating model | will be provided by operations | Operations | Partial |

## 16. Gap Register and Action Plan
| Gap ID | Description | Owner | Status | Target Date | Dependency |
|---|---|---|---|---|---|
| DORA-GAP-001 | Production incident response workflow and regulatory notification procedures not documented in this repo | Operations | Open | TBD | Incident process ownership |
| DORA-GAP-002 | Production BCDR procedures (RTO/RPO, recovery tests, failover runbooks) not documented in this repo | Operations | Open | TBD | Platform resilience ownership |
| DORA-GAP-003 | Production observability/SOC integration evidence not documented in this repo | Operations | Open | TBD | Monitoring stack ownership |
| DORA-GAP-004 | Third-party ICT risk register and supplier due diligence not documented in this repo | Operations | Open | TBD | Vendor risk management process |

## 17. Evidence Index
| Evidence ID | Type | Location | Purpose |
|---|---|---|---|
| E-001 | CI Workflow | `.github/workflows/ci-main.yml` | Build/test/security/performance quality gates |
| E-002 | Security Workflow | `.github/workflows/security-scans.yml` | SAST, SCA, secret scan, vulnerability scan |
| E-003 | Build and Test Summary | `aidlc-docs/construction/build-and-test/build-and-test-summary.md` | Consolidated quality criteria and scope |
| E-004 | Security Traceability | `aidlc-docs/construction/build-and-test/security-traceability.md` | Requirement-to-security-control mappings |
| E-005 | Performance Traceability | `aidlc-docs/construction/build-and-test/performance-traceability.md` | Requirement-to-performance-control mappings |
| E-006 | State Tracking | `aidlc-docs/aidlc-state.md` | Lifecycle status and stage completion evidence |
| E-007 | Audit Trail | `aidlc-docs/audit.md` | Decision and approval auditability |
| E-008 | Project Overview | `README.md` | Architecture and execution context |
| E-009 | CI Security Scan Run Log | `aidlc-docs/compliance/dora-compliance.md` (Section 19) | Historical run-level evidence (ID, URL, result, timestamp) |
| E-010 | Security Run Evidence Artifact | GitHub Actions artifact `security-scan-run-evidence` | Per-run machine-readable evidence payload |

## 18. Integrity Stamp
- **Repository**: oliver-koeth/hre-demo
- **Branch**: oliver-koeth-musical-waddle
- **Commit SHA (40-char)**: 403a819992378384acce663626bcd36438fbd6c8
- **Document SHA-256**: 7f327c1957f208b58b31c090640cc48e6c31710a4b6c037e355091c853ac50ce
- **Generated At (UTC, ISO 8601)**: 2026-07-30T14:15:23Z
- **Generator Identity**: GitHub Actions security-scans workflow

Stamp method note: `Document SHA-256` is calculated over this document body up to (but excluding) the `## 18. Integrity Stamp` section to avoid self-referential hashing.

## 19. CI Security Scan Run Log
| Timestamp (UTC) | Workflow | Run ID | Attempt | Result | URL | CodeQL Job | Dependency/Secrets/Container Job |
|---|---|---:|---:|---|---|---|---|
| 2026-07-30T14:15:23Z | Security Scans | 30550622706 | 1 | success | https://github.com/oliver-koeth/hre-demo/actions/runs/30550622706 | success | success |
| 2026-07-30T14:02:20Z | Security Scans | 30549521668 | 1 | success | https://github.com/oliver-koeth/hre-demo/actions/runs/30549521668 | success | success |
| 2026-07-30T08:58:16Z | Security Scans | 30528615432 | 1 | success | https://github.com/oliver-koeth/hre-demo/actions/runs/30528615432 | success | success |
| 2026-07-30T06:08:57Z | Security Scans | 30518555114 | 1 | success | https://github.com/oliver-koeth/hre-demo/actions/runs/30518555114 | success | success |
| 2026-07-29T16:50:09Z | Security Scans | 30472354754 | 1 | success | https://github.com/oliver-koeth/hre-demo/actions/runs/30472354754 | success | success |
| 2026-07-29T15:57:53Z | Security Scans | 30468278517 | 1 | success | https://github.com/oliver-koeth/hre-demo/actions/runs/30468278517 | success | success |
