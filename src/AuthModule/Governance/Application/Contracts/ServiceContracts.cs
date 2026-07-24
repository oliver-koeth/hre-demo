using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;
using AuthModule.Governance.Domain;

namespace AuthModule.Governance.Application.Contracts;

public sealed record AuditQueryRequest(
    DateTimeOffset? DateFrom,
    DateTimeOffset? DateTo,
    SecurityEventType? EventType,
    Guid? ActorId,
    int Page = 1,
    int PageSize = 100);

public sealed record AuditQueryResponse(int TotalCount, IReadOnlyList<SecurityAuditEvent> Events);

public sealed record EvidenceCaptureRequest(
    string EvidenceType,
    string SubjectEntityType,
    Guid SubjectEntityId,
    string Payload,
    IReadOnlyList<string> ControlMappingIds,
    DateTimeOffset? RetentionExpiresAt,
    bool LegalHoldActive = false,
    string? LegalHoldReason = null);

public sealed record EvidenceExportRequest(
    string? EvidenceType,
    string? SubjectEntityType,
    Guid? SubjectEntityId);

public sealed record DataSubjectRequestCommand(Guid SubjectUserId, DataSubjectRequestType RequestType);
public sealed record RetentionInvocationRequest(string EntityType);
public sealed record CreateIncidentRequest(string Title, GovernanceIncidentSeverity Severity, string ServiceImpact, bool BreachReportable);
public sealed record AdvanceIncidentStatusRequest(Guid IncidentId, GovernanceIncidentStatus TargetStatus);
public sealed record BackupMetadataAppendRequest(BackupEvidenceType BackupType, string StorePath);
public sealed record BackupMetadataStatusRequest(Guid BackupId, BackupEvidenceStatus TargetStatus);

public interface IAuditQueryService
{
    Task<Result<AuditQueryResponse, DomainError>> QuerySecurityEventsAsync(AuditQueryRequest request, RequestContext context);
}

public interface IEvidenceService
{
    Task<Result<EvidenceRecord, DomainError>> CaptureAsync(EvidenceCaptureRequest request, RequestContext context);
    Task<Result<EvidenceExportPackage, DomainError>> ExportAsync(EvidenceExportRequest request, RequestContext context);
}

public interface IDataSubjectService
{
    Task<Result<DataSubjectRequestRecord, DomainError>> SubmitAsync(DataSubjectRequestCommand request, RequestContext context);
}

public interface IRetentionService
{
    Task<Result<IReadOnlyList<LifecycleDecisionRecord>, DomainError>> InvokeAsync(RetentionInvocationRequest request, RequestContext context);
}

public interface IIncidentService
{
    Task<Result<GovernanceIncidentRecord, DomainError>> CreateAsync(CreateIncidentRequest request, RequestContext context);
    Task<Result<GovernanceIncidentRecord, DomainError>> AdvanceStatusAsync(AdvanceIncidentStatusRequest request, RequestContext context);
}

public interface IBackupEvidenceService
{
    Task<Result<BackupMetadataRecord, DomainError>> AppendAsync(BackupMetadataAppendRequest request, RequestContext context);
    Task<Result<BackupMetadataRecord, DomainError>> TransitionStatusAsync(BackupMetadataStatusRequest request, RequestContext context);
}

public interface ILegalHoldPolicyGuard
{
    Task<Result<Unit, DomainError>> EnsureNotBlockedAsync(Guid subjectEntityId, RequestContext context);
}

public interface IAlertService
{
    Task EmitRetentionFailureAsync(string entityType, string reason, RequestContext context);
    Task EmitLegalHoldBlockedBypassAsync(Guid subjectEntityId, string reason, RequestContext context);
}
