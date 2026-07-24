using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Governance.Application.Common;
using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Persistence;
using Microsoft.Extensions.Logging;

namespace AuthModule.Governance.Application.Security;

public sealed class AlertService(
    IAuditStoreRepository auditStoreRepository,
    ILogger<AlertService> logger) : IAlertService
{
    public async Task EmitRetentionFailureAsync(string entityType, string reason, RequestContext context)
    {
        logger.LogError("Retention invocation failed for {EntityType}. reason={Reason} correlationId={CorrelationId}", entityType, reason, context.CorrelationId);
        await auditStoreRepository.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.TokenRejected,
            CorrelationId = context.CorrelationId,
            Timestamp = context.Timestamp,
            Result = OperationResult.Failure,
            Reason = reason,
            Details = $"Retention failure: {entityType}",
        }, context);
    }

    public async Task EmitLegalHoldBlockedBypassAsync(Guid subjectEntityId, string reason, RequestContext context)
    {
        logger.LogWarning("Legal-hold blocked operation for {SubjectEntityId}. reason={Reason} correlationId={CorrelationId}", subjectEntityId, reason, context.CorrelationId);
        await auditStoreRepository.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.PrivilegedAccess,
            CorrelationId = context.CorrelationId,
            Timestamp = context.Timestamp,
            Result = OperationResult.Failure,
            Reason = reason,
            Details = $"Legal-hold blocked subject={subjectEntityId}",
        }, context);
    }
}

public sealed class LegalHoldPolicyGuard(
    IGovernanceStateStore stateStore,
    IAlertService alertService) : ILegalHoldPolicyGuard
{
    public async Task<Result<Unit, DomainError>> EnsureNotBlockedAsync(Guid subjectEntityId, RequestContext context)
    {
        if (!stateStore.HasLegalHold(subjectEntityId, out var reason))
        {
            return Result<Unit, DomainError>.Success(new Unit());
        }

        await alertService.EmitLegalHoldBlockedBypassAsync(subjectEntityId, reason, context);
        return Result<Unit, DomainError>.Failure(ErrorFactory.PolicyViolation($"Legal hold active: {reason}", context));
    }
}
