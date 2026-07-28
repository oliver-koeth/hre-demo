# Security Test Instructions

## Purpose
Validate security-critical behavior and run baseline open-source security scans in local and CI environments.

## Security Test Coverage

### 1. Authorization workflow actor enforcement
- `ApprovalWorkflowTests.Approval_ShouldRejectUnauthenticatedRequester`
- `ApprovalWorkflowTests.Approval_Decision_ShouldRejectUnauthenticatedApprover`
- `ApprovalSecurityTests.RequestApproval_ShouldReject_WhenActorHeaderMissing`
- `ApprovalSecurityTests.DecideApproval_ShouldReject_WhenActorHeaderMissing`

### 2. JWT negative-path validation
- `TokenSecurityNegativeTests.Validate_ShouldFail_ForTamperedToken`
- `TokenSecurityNegativeTests.Validate_ShouldFail_ForExpiredToken`
- `TokenSecurityNegativeTests.Validate_ShouldFail_ForWrongIssuer`
- `TokenSecurityNegativeTests.Validate_ShouldFail_ForWrongAudience`
- `TokenSecurityNegativeTests.Validate_ShouldFail_WhenRequiredClaimsMissing`

## Run Security Tests Locally

```bash
dotnet test tests/AuthModule.CoreSecurity.Tests/AuthModule.CoreSecurity.Tests.csproj --filter "Security=True" --nologo
dotnet test tests/AuthModule.ServiceHost.Tests/AuthModule.ServiceHost.Tests.csproj --filter "Security=True" --nologo
```

## Run Security Scans Locally (Open-Source Tooling)

### NuGet vulnerability scan
```bash
dotnet list AuthModule.slnx package --vulnerable --include-transitive
```

### Trivy filesystem scan (HIGH/CRITICAL)
```bash
trivy fs --severity HIGH,CRITICAL --ignore-unfixed .
```

### Gitleaks secret scan
```bash
gitleaks detect --source . --verbose
```

## CI Security Workflows
- `ci-main.yml`: executes security-tagged automated tests
- `security-scans.yml`: runs CodeQL, NuGet vulnerability gate, Trivy scan, and Gitleaks
