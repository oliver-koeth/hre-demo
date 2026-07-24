using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Persistence;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using Microsoft.Extensions.Logging;

namespace AuthModule.CoreSecurity.Application.Governance;

public sealed class SecurityAlertService(IAuditEventSink auditEventSink, ILogger<SecurityAlertService> logger) : ISecurityAlertService
{
    public async Task EmitLockoutThresholdReachedAsync(string emailNormalized, int lockoutCount, RequestContext context)
    {
        logger.LogWarning(
            "Lockout threshold reached for email {EmailNormalized}. lockoutCount={LockoutCount} correlationId={CorrelationId}",
            emailNormalized,
            lockoutCount,
            context.CorrelationId);

        await auditEventSink.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.BruteForceDetected,
            ActorId = null,
            CorrelationId = context.CorrelationId,
            SourceIp = context.SourceIp,
            Timestamp = context.Timestamp,
            Result = OperationResult.Failure,
            Details = $"Lockout threshold reached for {emailNormalized}; count={lockoutCount}",
        }, context);
    }
}
