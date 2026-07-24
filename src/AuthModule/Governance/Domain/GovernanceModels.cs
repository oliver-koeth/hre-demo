using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;

namespace AuthModule.Governance.Domain;

public enum DataSubjectRequestType
{
    Retrieve,
    Export,
}

public enum DataSubjectRequestStatus
{
    Pending,
    Completed,
    BlockedByHold,
}

public enum RetentionAction
{
    Delete,
    Anonymize,
    Archive,
}

public enum LifecycleOutcome
{
    Applied,
    BlockedByHold,
    Skipped,
}

public enum GovernanceIncidentSeverity
{
    Low,
    Medium,
    High,
    Critical,
}

public enum GovernanceIncidentStatus
{
    Open,
    Investigating,
    Resolved,
    Closed,
}

public enum BackupEvidenceType
{
    Full,
    Incremental,
}

public enum BackupEvidenceStatus
{
    Pending,
    Completed,
    Failed,
    Verified,
}

public sealed class EvidenceRecord : BaseStoreEntity
{
    public Guid EvidenceId { get; set; }
    public string EvidenceType { get; set; } = string.Empty;
    public string SubjectEntityType { get; set; } = string.Empty;
    public Guid SubjectEntityId { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTimeOffset CapturedAt { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string[] ControlMappingIds { get; set; } = [];
    public DateTimeOffset? RetentionExpiresAt { get; set; }
    public bool LegalHoldActive { get; set; }
    public string? LegalHoldReason { get; set; }
    public override Guid Id => EvidenceId;
}

public sealed class ProcessingActivityEntry : BaseStoreEntity
{
    public Guid ActivityId { get; set; }
    public string ActivityName { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string LawfulBasis { get; set; } = string.Empty;
    public string[] DataCategories { get; set; } = [];
    public int RetentionDays { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public override Guid Id => ActivityId;
}

public sealed class DataSubjectRequestRecord : BaseStoreEntity
{
    public Guid RequestId { get; set; }
    public Guid SubjectUserId { get; set; }
    public DataSubjectRequestType RequestType { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid RequestedByUserId { get; set; }
    public DataSubjectRequestStatus Status { get; set; } = DataSubjectRequestStatus.Pending;
    public DateTimeOffset? CompletedAt { get; set; }
    public string? BlockReason { get; set; }
    public string? ExportPayload { get; set; }
    public Guid CorrelationId { get; set; }
    public override Guid Id => RequestId;
}

public sealed class RetentionRuleRecord : BaseStoreEntity
{
    public Guid RuleId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int RetentionPeriodDays { get; set; }
    public RetentionAction Action { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public override Guid Id => RuleId;
}

public sealed class LifecycleDecisionRecord : BaseStoreEntity
{
    public Guid DecisionId { get; set; }
    public Guid RuleId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public DateTimeOffset EvaluatedAt { get; set; }
    public RetentionAction Action { get; set; }
    public LifecycleOutcome Outcome { get; set; }
    public string? BlockReason { get; set; }
    public Guid CorrelationId { get; set; }
    public override Guid Id => DecisionId;
}

public sealed class RetentionFingerprintRecord : BaseStoreEntity
{
    public Guid FingerprintId { get; set; }
    public Guid RuleId { get; set; }
    public Guid EntityId { get; set; }
    public string Fingerprint { get; set; } = string.Empty;
    public LifecycleOutcome LastOutcome { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public override Guid Id => FingerprintId;
}

public sealed class GovernanceIncidentRecord : BaseStoreEntity
{
    public Guid IncidentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public GovernanceIncidentSeverity Severity { get; set; }
    public string ServiceImpact { get; set; } = string.Empty;
    public bool BreachReportable { get; set; }
    public GovernanceIncidentStatus Status { get; set; } = GovernanceIncidentStatus.Open;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public Guid CorrelationId { get; set; }
    public override Guid Id => IncidentId;
}

public sealed class BackupMetadataRecord : BaseStoreEntity
{
    public Guid BackupId { get; set; }
    public BackupEvidenceType BackupType { get; set; }
    public string StorePath { get; set; } = string.Empty;
    public BackupEvidenceStatus Status { get; set; } = BackupEvidenceStatus.Pending;
    public DateTimeOffset ExecutedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public Guid CorrelationId { get; set; }
    public override Guid Id => BackupId;
}

public sealed record EvidenceExportManifest(
    Guid ExportId,
    DateTimeOffset RequestedAt,
    int EvidenceCount,
    Guid RequestedByUserId,
    Guid CorrelationId);

public sealed record EvidenceExportPackage(EvidenceExportManifest Manifest, IReadOnlyList<EvidenceRecord> Records);
