namespace AuthModule.Foundation.Domain.Entities;

public enum OperationResult
{
    Success,
    Failure,
}

public enum SecurityEventType
{
    LoginAttempt,
    LoginSuccess,
    LoginFailure,
    AccountLocked,
    TokenIssued,
    TokenValidated,
    TokenRejected,
    AccountDisabled,
    PrivilegedAccess,
    BruteForceDetected,
}

public enum EntityChangeType
{
    Create,
    Update,
    Delete,
}

public sealed class SecurityAuditEvent
{
    public Guid EventId { get; set; }
    public SecurityEventType EventType { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorUsername { get; set; }
    public string? SourceIp { get; set; }
    public OperationResult Result { get; set; }
    public string? Reason { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid? SessionId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? Details { get; set; }
}

public sealed class AdminChangeEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid ActorId { get; set; }
    public Guid? ApprovalTicketId { get; set; }
    public string TargetEntityType { get; set; } = string.Empty;
    public Guid TargetEntityId { get; set; }
    public EntityChangeType ChangeType { get; set; }
    public string? BeforeSnapshot { get; set; }
    public string? AfterSnapshot { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

