using System.Text.Json;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Governance.Application.Common;
using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Configuration;
using AuthModule.Governance.Domain;
using AuthModule.Governance.Persistence;

namespace AuthModule.Governance.Application.AuditEvidence;

public sealed class AuditQueryService(
    IAuditStoreRepository auditStoreRepository,
    GovernanceConfiguration configuration) : IAuditQueryService
{
    public async Task<Result<AuditQueryResponse, DomainError>> QuerySecurityEventsAsync(AuditQueryRequest request, RequestContext context)
    {
        if (request.Page < 1)
        {
            return Result<AuditQueryResponse, DomainError>.Failure(ErrorFactory.Validation("Page must be >= 1.", context));
        }

        var pageSize = request.PageSize <= 0 ? configuration.DefaultQueryPageSize : Math.Min(request.PageSize, configuration.MaxQueryPageSize);
        var events = await auditStoreRepository.QuerySecurityEventsAsync(context);
        if (events.IsFailure)
        {
            return Result<AuditQueryResponse, DomainError>.Failure(events.Error);
        }

        IEnumerable<SecurityAuditEvent> query = events.Value;
        if (request.DateFrom is not null) query = query.Where(x => x.Timestamp >= request.DateFrom);
        if (request.DateTo is not null) query = query.Where(x => x.Timestamp <= request.DateTo);
        if (request.EventType is not null) query = query.Where(x => x.EventType == request.EventType.Value);
        if (request.ActorId is not null) query = query.Where(x => x.ActorId == request.ActorId);

        var ordered = query.OrderByDescending(x => x.Timestamp).ToList();
        var paged = ordered.Skip((request.Page - 1) * pageSize).Take(pageSize).ToList();
        return Result<AuditQueryResponse, DomainError>.Success(new AuditQueryResponse(ordered.Count, paged));
    }
}

public sealed class EvidenceService(
    IGovernanceStateStore stateStore,
    GovernanceConfiguration configuration) : IEvidenceService
{
    public Task<Result<EvidenceRecord, DomainError>> CaptureAsync(EvidenceCaptureRequest request, RequestContext context)
    {
        if (string.IsNullOrWhiteSpace(request.EvidenceType))
        {
            return Task.FromResult(Result<EvidenceRecord, DomainError>.Failure(ErrorFactory.Validation("EvidenceType is required.", context)));
        }

        var now = context.Timestamp;
        var record = new EvidenceRecord
        {
            EvidenceId = Guid.NewGuid(),
            EvidenceType = request.EvidenceType,
            SubjectEntityType = request.SubjectEntityType,
            SubjectEntityId = request.SubjectEntityId,
            CorrelationId = context.CorrelationId,
            CapturedAt = now,
            Payload = request.Payload,
            ControlMappingIds = request.ControlMappingIds.ToArray(),
            RetentionExpiresAt = request.RetentionExpiresAt ?? now.AddDays(configuration.EvidenceRetentionMinimumDays),
            LegalHoldActive = request.LegalHoldActive,
            LegalHoldReason = request.LegalHoldReason,
            Version = 0,
        };

        stateStore.SaveEvidence(record);
        return Task.FromResult(Result<EvidenceRecord, DomainError>.Success(record));
    }

    public Task<Result<EvidenceExportPackage, DomainError>> ExportAsync(EvidenceExportRequest request, RequestContext context)
    {
        var records = stateStore.QueryEvidence(request.EvidenceType, request.SubjectEntityType, request.SubjectEntityId);
        var manifest = new EvidenceExportManifest(
            ExportId: Guid.NewGuid(),
            RequestedAt: context.Timestamp,
            EvidenceCount: records.Count,
            RequestedByUserId: context.UserId ?? Guid.Empty,
            CorrelationId: context.CorrelationId);

        // Serialize once to ensure payload remains machine-readable and stable.
        _ = JsonSerializer.Serialize(new { manifest, records });
        return Task.FromResult(Result<EvidenceExportPackage, DomainError>.Success(new EvidenceExportPackage(manifest, records)));
    }
}
