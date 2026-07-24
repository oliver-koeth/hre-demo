namespace AuthModule.Foundation.Domain.Entities;

public enum IncidentSeverity
{
    Low,
    Medium,
    High,
    Critical,
}

public enum IncidentStatus
{
    Open,
    Investigating,
    Resolved,
    Closed,
}

public enum BackupType
{
    Full,
    Incremental,
}

public enum BackupStatus
{
    Pending,
    Completed,
    Failed,
    Verified,
    RestoredOk,
    RestoreFailed,
}

public sealed class ProcessingActivity : BaseStoreEntity
{
    public Guid ActivityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string LawfulBasis { get; set; } = string.Empty;
    public string[] DataCategories { get; set; } = [];
    public int RetentionPeriodDays { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public override Guid Id => ActivityId;
}

public sealed class IncidentRecord : BaseStoreEntity
{
    public Guid IncidentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public string ServiceImpact { get; set; } = string.Empty;
    public string? DataImpact { get; set; }
    public string? BusinessImpact { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public bool BreachReportable { get; set; }
    public override Guid Id => IncidentId;
}

public sealed class BackupMetadata : BaseStoreEntity
{
    public Guid BackupId { get; set; }
    public BackupType BackupType { get; set; }
    public string StorePath { get; set; } = string.Empty;
    public BackupStatus Status { get; set; } = BackupStatus.Pending;
    public DateTimeOffset ExecutedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? RestoredAt { get; set; }
    public string? ReconciliationResult { get; set; }
    public override Guid Id => BackupId;
}

