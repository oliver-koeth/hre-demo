using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Governance.Application.AuditEvidence;
using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Application.DataSubject;
using AuthModule.Governance.Application.Operations;
using AuthModule.Governance.Application.Retention;
using AuthModule.Governance.Application.Security;
using AuthModule.Governance.Configuration;
using AuthModule.Governance.Domain;
using AuthModule.Governance.Persistence;
using Microsoft.Extensions.Logging.Abstractions;

namespace AuthModule.Governance.Tests.Support;

public sealed class TestContext
{
    public required RequestContext RequestContext { get; init; }
    public required InMemoryAuditStoreRepository AuditStore { get; init; }
    public required InMemoryGovernanceStateStore StateStore { get; init; }
    public required IAuditQueryService AuditQueryService { get; init; }
    public required IEvidenceService EvidenceService { get; init; }
    public required IDataSubjectService DataSubjectService { get; init; }
    public required IRetentionService RetentionService { get; init; }
    public required IIncidentService IncidentService { get; init; }
    public required IBackupEvidenceService BackupEvidenceService { get; init; }
}

public static class TestContextFactory
{
    public static TestContext Create()
    {
        var auditStore = new InMemoryAuditStoreRepository();
        var stateStore = new InMemoryGovernanceStateStore();
        var configuration = new GovernanceConfiguration();
        var context = new RequestContext(Guid.NewGuid(), Guid.NewGuid(), "127.0.0.1", DateTimeOffset.UtcNow, Guid.NewGuid());

        var alertService = new AlertService(auditStore, new NullLogger<AlertService>());
        var holdGuard = new LegalHoldPolicyGuard(stateStore, alertService);
        var auditQuery = new AuditQueryService(auditStore, configuration);
        var evidence = new EvidenceService(stateStore, configuration);
        var dataSubject = new DataSubjectService(stateStore, holdGuard);
        var retention = new RetentionService(stateStore, alertService);
        var incident = new IncidentService(stateStore);
        var backup = new BackupEvidenceService(stateStore);

        // Seed events and rules for query/retention tests.
        auditStore.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.LoginFailure,
            Timestamp = context.Timestamp.AddMinutes(-5),
            CorrelationId = context.CorrelationId,
            Result = OperationResult.Failure,
        }, context).GetAwaiter().GetResult();

        stateStore.SaveRetentionRule(new RetentionRuleRecord
        {
            RuleId = Guid.NewGuid(),
            EntityType = "User",
            RetentionPeriodDays = 180,
            Action = RetentionAction.Archive,
            CreatedAt = context.Timestamp,
            UpdatedAt = context.Timestamp,
            Version = 0,
        });

        return new TestContext
        {
            RequestContext = context,
            AuditStore = auditStore,
            StateStore = stateStore,
            AuditQueryService = auditQuery,
            EvidenceService = evidence,
            DataSubjectService = dataSubject,
            RetentionService = retention,
            IncidentService = incident,
            BackupEvidenceService = backup,
        };
    }
}
