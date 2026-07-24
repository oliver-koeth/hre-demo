using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Governance.Application.Common;
using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Domain;
using AuthModule.Governance.Persistence;

namespace AuthModule.Governance.Application.Operations;

public sealed class IncidentService(IGovernanceStateStore stateStore) : IIncidentService
{
    public Task<Result<GovernanceIncidentRecord, DomainError>> CreateAsync(CreateIncidentRequest request, RequestContext context)
    {
        var incident = new GovernanceIncidentRecord
        {
            IncidentId = Guid.NewGuid(),
            Title = request.Title,
            Severity = request.Severity,
            ServiceImpact = request.ServiceImpact,
            BreachReportable = request.BreachReportable,
            Status = GovernanceIncidentStatus.Open,
            CreatedAt = context.Timestamp,
            UpdatedAt = context.Timestamp,
            CorrelationId = context.CorrelationId,
            Version = 0,
        };
        stateStore.SaveIncident(incident);
        return Task.FromResult(Result<GovernanceIncidentRecord, DomainError>.Success(incident));
    }

    public Task<Result<GovernanceIncidentRecord, DomainError>> AdvanceStatusAsync(AdvanceIncidentStatusRequest request, RequestContext context)
    {
        var incident = stateStore.GetIncident(request.IncidentId);
        if (incident is null)
        {
            return Task.FromResult(Result<GovernanceIncidentRecord, DomainError>.Failure(ErrorFactory.NotFound("Incident not found.", context)));
        }

        if (!IsTransitionAllowed(incident.Status, request.TargetStatus))
        {
            return Task.FromResult(Result<GovernanceIncidentRecord, DomainError>.Failure(ErrorFactory.Conflict("Invalid incident status transition.", context)));
        }

        incident.Status = request.TargetStatus;
        incident.UpdatedAt = context.Timestamp;
        if (request.TargetStatus == GovernanceIncidentStatus.Resolved)
        {
            incident.ResolvedAt = context.Timestamp;
        }

        incident.Version += 1;
        // Synchronous store update forms the durability gate for success response in this phase.
        stateStore.SaveIncident(incident);
        return Task.FromResult(Result<GovernanceIncidentRecord, DomainError>.Success(incident));
    }

    private static bool IsTransitionAllowed(GovernanceIncidentStatus current, GovernanceIncidentStatus target) =>
        current switch
        {
            GovernanceIncidentStatus.Open => target == GovernanceIncidentStatus.Investigating,
            GovernanceIncidentStatus.Investigating => target == GovernanceIncidentStatus.Resolved,
            GovernanceIncidentStatus.Resolved => target == GovernanceIncidentStatus.Closed,
            GovernanceIncidentStatus.Closed => false,
            _ => false,
        };
}

public sealed class BackupEvidenceService(IGovernanceStateStore stateStore) : IBackupEvidenceService
{
    public Task<Result<BackupMetadataRecord, DomainError>> AppendAsync(BackupMetadataAppendRequest request, RequestContext context)
    {
        var record = new BackupMetadataRecord
        {
            BackupId = Guid.NewGuid(),
            BackupType = request.BackupType,
            StorePath = request.StorePath,
            Status = BackupEvidenceStatus.Pending,
            ExecutedAt = context.Timestamp,
            CorrelationId = context.CorrelationId,
            Version = 0,
        };
        stateStore.SaveBackupMetadata(record);
        return Task.FromResult(Result<BackupMetadataRecord, DomainError>.Success(record));
    }

    public Task<Result<BackupMetadataRecord, DomainError>> TransitionStatusAsync(BackupMetadataStatusRequest request, RequestContext context)
    {
        var existing = stateStore.GetBackupMetadata(request.BackupId);
        if (existing is null)
        {
            return Task.FromResult(Result<BackupMetadataRecord, DomainError>.Failure(ErrorFactory.NotFound("Backup metadata not found.", context)));
        }

        if (!IsTransitionAllowed(existing.Status, request.TargetStatus))
        {
            return Task.FromResult(Result<BackupMetadataRecord, DomainError>.Failure(ErrorFactory.Conflict("Invalid backup status transition.", context)));
        }

        existing.Status = request.TargetStatus;
        if (request.TargetStatus == BackupEvidenceStatus.Verified)
        {
            existing.VerifiedAt = context.Timestamp;
        }

        existing.Version += 1;
        stateStore.SaveBackupMetadata(existing);
        return Task.FromResult(Result<BackupMetadataRecord, DomainError>.Success(existing));
    }

    private static bool IsTransitionAllowed(BackupEvidenceStatus current, BackupEvidenceStatus target) =>
        current switch
        {
            BackupEvidenceStatus.Pending => target is BackupEvidenceStatus.Completed or BackupEvidenceStatus.Failed,
            BackupEvidenceStatus.Completed => target == BackupEvidenceStatus.Verified,
            BackupEvidenceStatus.Failed => false,
            BackupEvidenceStatus.Verified => false,
            _ => false,
        };
}
