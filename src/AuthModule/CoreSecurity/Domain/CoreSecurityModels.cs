namespace AuthModule.CoreSecurity.Domain;

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Applied,
}

public enum StepUpStatus
{
    Pending,
    Satisfied,
    Failed,
    Expired,
}

public sealed class AuthAttemptState
{
    public required string EmailNormalized { get; init; }
    public int FailedAttempts { get; set; }
    public DateTimeOffset? LastFailureAt { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? LastSuccessAt { get; set; }
}

public sealed class AuthSession
{
    public required Guid SessionId { get; init; }
    public required Guid UserId { get; init; }
    public required int TokenVersionSnapshot { get; init; }
    public required DateTimeOffset IssuedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required bool IsPrivileged { get; init; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokeReason { get; set; }
}

public sealed class AccessTokenClaimsModel
{
    public required Guid SubjectUserId { get; init; }
    public required Guid SessionId { get; init; }
    public required int TokenVersion { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required DateTimeOffset IssuedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required IReadOnlyList<string> PermissionKeys { get; init; }
    public required bool IsPrivileged { get; init; }
}

public sealed class AuthorizationRequestModel
{
    public required Guid UserId { get; init; }
    public required string Resource { get; init; }
    public required string Action { get; init; }
}

public sealed class AuthorizationDecisionModel
{
    public required bool Allowed { get; init; }
    public required string ReasonCode { get; init; }
    public required string PermissionEvaluated { get; init; }
}

public sealed class ApprovalTicket
{
    public required Guid TicketId { get; init; }
    public required string ChangeType { get; init; }
    public required Guid RoleId { get; init; }
    public required Guid PermissionId { get; init; }
    public required Guid RequestedByUserId { get; init; }
    public required DateTimeOffset RequestedAt { get; init; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public sealed class StepUpChallenge
{
    public required Guid ChallengeId { get; init; }
    public required Guid UserId { get; init; }
    public required Guid SessionId { get; init; }
    public required string OperationKey { get; init; }
    public required DateTimeOffset IssuedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? CompletedAt { get; set; }
    public StepUpStatus Status { get; set; } = StepUpStatus.Pending;
}
