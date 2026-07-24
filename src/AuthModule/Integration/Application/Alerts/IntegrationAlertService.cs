using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Integration.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace AuthModule.Integration.Application.Alerts;

public sealed class IntegrationAlertService(
    IAuditStoreRepository auditStoreRepository,
    ILogger<IntegrationAlertService> logger) : IIntegrationAlertService
{
    public async Task EmitGateFailureAsync(string reason, RequestContext context)
    {
        logger.LogError("Integration gate failed. reason={Reason} correlationId={CorrelationId}", reason, context.CorrelationId);
        await auditStoreRepository.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.TokenRejected,
            CorrelationId = context.CorrelationId,
            Timestamp = context.Timestamp,
            Result = OperationResult.Failure,
            Reason = reason,
            Details = "Integration gate failure.",
        }, context);
    }
}
