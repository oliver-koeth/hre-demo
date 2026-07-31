# Security Test Traceability Matrix

## Purpose
Map security-related requirements to executable security tests and CI scan evidence.

## Requirement-to-Test Mapping

| Requirement ID | Requirement intent | Automated tests/scans | Evidence |
|---|---|---|---|
| FR-03 | Privileged operations require governance controls and separation of duties | `ApprovalWorkflowTests.*`, `ApprovalSecurityTests.*` | test results in `AuthModule.CoreSecurity.Tests` and `AuthModule.ServiceHost.Tests` |
| FR-U02-007 | User search requires a valid authenticated actor and a permission grant | `UserSearchTests.SearchUsers_WithoutActorHeader_ShouldReturn401`, `UserSearchTests.SearchUsers_WithInvalidActorHeader_ShouldReturn401`, `UserSearchTests.SearchUsers_ByUnauthorizedActor_ShouldReturn403` | test results in `AuthModule.ServiceHost.Tests` |
| FR-U02-008 | Search query length is bounded to prevent abuse | `UserSearchTests.SearchUsers_WithShortQuery_ShouldReturn400`, `UserSearchTests.SearchUsers_WithLongQuery_ShouldReturn400` | test results in `AuthModule.ServiceHost.Tests` |
| FR-05 | Token lifecycle validation and rejection of invalid/unsafe tokens | `TokenSecurityNegativeTests.*` | test results in `AuthModule.CoreSecurity.Tests` |
| NFR-02 | Security-first blocking quality constraints | `ci-main.yml` security test steps | GitHub Actions run logs |
| NFR-10 | Supply-chain and third-party risk controls for CI/CD dependencies | NuGet vulnerability scan in `security-scans.yml` | `artifacts/security/nuget-vulnerabilities.json`, step summary |
| NFR-10 | Repository/container surface vulnerability visibility | Trivy filesystem scan in `security-scans.yml` | workflow logs |
| NFR-03 | Secrets must not be exposed in source/config | Gitleaks scan in `security-scans.yml` | workflow logs |

## Evidence Locations
- CI workflow: `.github/workflows/security-scans.yml`
- Security scan artifact: `artifacts/security/nuget-vulnerabilities.json`
- Security test instructions: `aidlc-docs/construction/build-and-test/security-test-instructions.md`
