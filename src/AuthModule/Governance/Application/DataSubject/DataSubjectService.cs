using System.Text.Json;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Governance.Application.Common;
using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Domain;
using AuthModule.Governance.Persistence;

namespace AuthModule.Governance.Application.DataSubject;

public sealed class DataSubjectService(
    IGovernanceStateStore stateStore,
    ILegalHoldPolicyGuard legalHoldPolicyGuard) : IDataSubjectService
{
    public async Task<Result<DataSubjectRequestRecord, DomainError>> SubmitAsync(DataSubjectRequestCommand request, RequestContext context)
    {
        var guard = await legalHoldPolicyGuard.EnsureNotBlockedAsync(request.SubjectUserId, context);
        if (guard.IsFailure)
        {
            var blocked = new DataSubjectRequestRecord
            {
                RequestId = Guid.NewGuid(),
                SubjectUserId = request.SubjectUserId,
                RequestType = request.RequestType,
                RequestedAt = context.Timestamp,
                RequestedByUserId = context.UserId ?? Guid.Empty,
                Status = DataSubjectRequestStatus.BlockedByHold,
                BlockReason = guard.Error.Message,
                CorrelationId = context.CorrelationId,
                Version = 0,
            };
            stateStore.SaveDataSubjectRequest(blocked);
            return Result<DataSubjectRequestRecord, DomainError>.Failure(guard.Error);
        }

        var record = new DataSubjectRequestRecord
        {
            RequestId = Guid.NewGuid(),
            SubjectUserId = request.SubjectUserId,
            RequestType = request.RequestType,
            RequestedAt = context.Timestamp,
            RequestedByUserId = context.UserId ?? Guid.Empty,
            Status = DataSubjectRequestStatus.Completed,
            CompletedAt = context.Timestamp,
            CorrelationId = context.CorrelationId,
            ExportPayload = request.RequestType == DataSubjectRequestType.Export
                ? JsonSerializer.Serialize(new { subjectUserId = request.SubjectUserId, exportedAt = context.Timestamp })
                : null,
            Version = 0,
        };
        stateStore.SaveDataSubjectRequest(record);
        return Result<DataSubjectRequestRecord, DomainError>.Success(record);
    }
}
