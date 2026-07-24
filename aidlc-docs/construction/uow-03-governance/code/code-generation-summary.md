# UOW-03 Governance Code Generation Summary

## Story-to-Code Traceability
| Story | Implemented Components |
|---|---|
| US-07 Audit trail query | `Application/AuditEvidence/AuditQueryService`, `Api/GovernanceEndpoints` (`/audit/security-events`) |
| US-08 Evidence capture/export | `Application/AuditEvidence/EvidenceService`, `Domain/EvidenceRecord`, `Api/GovernanceEndpoints` (`/evidence`, `/evidence/export`) |
| US-09 Data-subject retrieval/export with legal hold | `Application/DataSubject/DataSubjectService`, `Application/Security/LegalHoldPolicyGuard`, `Domain/DataSubjectRequestRecord` |
| US-10 Manual retention invocation + idempotence | `Application/Retention/RetentionService`, `Domain/RetentionRuleRecord`, `Domain/RetentionFingerprintRecord`, `/retention/invoke` |
| US-11 Incident lifecycle tracking | `Application/Operations/IncidentService`, `Domain/GovernanceIncidentRecord`, `/incidents`, `/incidents/status` |
| US-12 Backup metadata evidence | `Application/Operations/BackupEvidenceService`, `Domain/BackupMetadataRecord`, `/backups`, `/backups/status` |

## Created and Modified Files
- **Solution/Test wiring**: `AuthModule.slnx`, `src/AuthModule/Governance/Governance.csproj`, `tests/AuthModule.Governance.Tests/AuthModule.Governance.Tests.csproj`
- **Governance module code**: `src/AuthModule/Governance/{Domain,Configuration,Application,Persistence,Api,Bootstrap}/**/*.cs`
- **Tests and PBT**: `tests/AuthModule.Governance.Tests/{Audit,Evidence,Retention,Operations,PropertyBased,Support}/**/*.cs`
- **Runtime artifacts**: `config/policy.template.json`, `docker-compose.yml`

## Test Map
- `Audit/AuditQueryTests.cs`: security-event query paging.
- `Evidence/LegalHoldTests.cs`: fail-closed legal-hold behavior.
- `Retention/RetentionServiceTests.cs`: deterministic retention outcomes.
- `Operations/IncidentLifecycleTests.cs`: valid and invalid incident status transitions.
- `PropertyBased/RetentionPropertyTests.cs`: idempotent retention outcomes for unchanged inputs.

## Extension Rule Compliance
| Extension Rule | Status | Notes |
|---|---|---|
| Security Baseline | Compliant | Legal-hold fail-closed path and reason-coded policy violations implemented; correlation IDs included in API error responses. |
| Resiliency Baseline | Compliant | Retention failure alerting implemented; incident progression applies durable state write before success response; backup status lifecycle modeled. |
| Property-Based Testing | Compliant | FsCheck-based retention idempotence property test added. |

## Unresolved Items
- None for UOW-03 scope.
