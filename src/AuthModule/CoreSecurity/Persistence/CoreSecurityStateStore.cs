using System.Collections.Concurrent;
using AuthModule.CoreSecurity.Domain;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;

namespace AuthModule.CoreSecurity.Persistence;

public sealed record StoredCredential(Guid UserId, HashAlgorithm Algorithm, string Hash, string Salt, bool IsActive = true);

public interface ICoreSecurityStateStore
{
    AuthAttemptState GetOrCreateAttempt(string emailNormalized);
    void SaveAttempt(AuthAttemptState attemptState);
    void ResetAttempt(string emailNormalized, DateTimeOffset successAt);
    AuthSession SaveSession(AuthSession session);
    AuthSession? GetSession(Guid sessionId);
    int GetTokenVersion(Guid userId);
    int IncrementTokenVersion(Guid userId);
    void SetTokenVersion(Guid userId, int version);
    StoredCredential? GetCredential(Guid userId);
    void UpsertCredential(StoredCredential credential);
    IReadOnlyList<UserRoleAssignment> GetRoleAssignments(Guid userId, DateTimeOffset at);
    IReadOnlyList<RolePermissionAssignment> GetPermissionAssignments(Guid roleId, DateTimeOffset at);
    void UpsertUserRoleAssignment(UserRoleAssignment assignment);
    void UpsertRolePermissionAssignment(RolePermissionAssignment assignment);
    void ApplyRolePermissionAssignment(Guid roleId, Guid permissionId, Guid actorUserId, DateTimeOffset at);
    void RevokeRolePermissionAssignment(Guid roleId, Guid permissionId, Guid actorUserId, DateTimeOffset at);
    ApprovalTicket SaveApproval(ApprovalTicket ticket);
    ApprovalTicket? GetApproval(Guid ticketId);
    void SaveChallenge(StepUpChallenge challenge);
    StepUpChallenge? GetChallenge(Guid challengeId);
}

public sealed class InMemoryCoreSecurityStateStore : ICoreSecurityStateStore
{
    private readonly ConcurrentDictionary<string, AuthAttemptState> _attempts = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<Guid, AuthSession> _sessions = new();
    private readonly ConcurrentDictionary<Guid, int> _tokenVersions = new();
    private readonly ConcurrentDictionary<Guid, StoredCredential> _credentials = new();
    private readonly ConcurrentDictionary<Guid, UserRoleAssignment> _userRoleAssignments = new();
    private readonly ConcurrentDictionary<Guid, RolePermissionAssignment> _rolePermissionAssignments = new();
    private readonly ConcurrentDictionary<Guid, ApprovalTicket> _approvalTickets = new();
    private readonly ConcurrentDictionary<Guid, StepUpChallenge> _challenges = new();

    public AuthAttemptState GetOrCreateAttempt(string emailNormalized) =>
        _attempts.GetOrAdd(emailNormalized, key => new AuthAttemptState { EmailNormalized = key });

    public void SaveAttempt(AuthAttemptState attemptState) => _attempts[attemptState.EmailNormalized] = attemptState;

    public void ResetAttempt(string emailNormalized, DateTimeOffset successAt)
    {
        var attempt = GetOrCreateAttempt(emailNormalized);
        attempt.FailedAttempts = 0;
        attempt.LockedUntil = null;
        attempt.LastSuccessAt = successAt;
        _attempts[emailNormalized] = attempt;
    }

    public AuthSession SaveSession(AuthSession session)
    {
        _sessions[session.SessionId] = session;
        return session;
    }

    public AuthSession? GetSession(Guid sessionId) => _sessions.GetValueOrDefault(sessionId);

    public int GetTokenVersion(Guid userId) => _tokenVersions.GetValueOrDefault(userId, 0);

    public int IncrementTokenVersion(Guid userId) =>
        _tokenVersions.AddOrUpdate(userId, 1, (_, current) => current + 1);

    public void SetTokenVersion(Guid userId, int version) => _tokenVersions[userId] = version;

    public StoredCredential? GetCredential(Guid userId) => _credentials.GetValueOrDefault(userId);

    public void UpsertCredential(StoredCredential credential) => _credentials[credential.UserId] = credential;

    public IReadOnlyList<UserRoleAssignment> GetRoleAssignments(Guid userId, DateTimeOffset at) =>
        _userRoleAssignments.Values.Where(x => x.UserId == userId && x.IsActiveAt(at)).ToList();

    public IReadOnlyList<RolePermissionAssignment> GetPermissionAssignments(Guid roleId, DateTimeOffset at) =>
        _rolePermissionAssignments.Values.Where(x => x.RoleId == roleId && x.IsActiveAt(at)).ToList();

    public void UpsertUserRoleAssignment(UserRoleAssignment assignment) => _userRoleAssignments[assignment.AssignmentId] = assignment;

    public void UpsertRolePermissionAssignment(RolePermissionAssignment assignment) =>
        _rolePermissionAssignments[assignment.AssignmentId] = assignment;

    public void ApplyRolePermissionAssignment(Guid roleId, Guid permissionId, Guid actorUserId, DateTimeOffset at)
    {
        var existing = _rolePermissionAssignments.Values.FirstOrDefault(x => x.RoleId == roleId && x.PermissionId == permissionId && !x.IsDeleted);
        if (existing is not null)
        {
            return;
        }

        var assignment = new RolePermissionAssignment
        {
            AssignmentId = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permissionId,
            CreatedAt = at,
            CreatedBy = actorUserId,
            ValidFrom = at,
            ValidUntil = null,
            Version = 0,
            IsDeleted = false,
        };
        _rolePermissionAssignments[assignment.AssignmentId] = assignment;
    }

    public void RevokeRolePermissionAssignment(Guid roleId, Guid permissionId, Guid actorUserId, DateTimeOffset at)
    {
        var existing = _rolePermissionAssignments.Values.FirstOrDefault(x => x.RoleId == roleId && x.PermissionId == permissionId && !x.IsDeleted);
        if (existing is null)
        {
            return;
        }

        existing.IsDeleted = true;
        existing.DeletedBy = actorUserId;
        existing.DeletedAt = at;
        existing.Version += 1;
        _rolePermissionAssignments[existing.AssignmentId] = existing;
    }

    public ApprovalTicket SaveApproval(ApprovalTicket ticket)
    {
        _approvalTickets[ticket.TicketId] = ticket;
        return ticket;
    }

    public ApprovalTicket? GetApproval(Guid ticketId) => _approvalTickets.GetValueOrDefault(ticketId);

    public void SaveChallenge(StepUpChallenge challenge) => _challenges[challenge.ChallengeId] = challenge;

    public StepUpChallenge? GetChallenge(Guid challengeId) => _challenges.GetValueOrDefault(challengeId);
}

public interface IAuditEventSink
{
    Task<Result<Unit, DomainError>> AppendSecurityEventAsync(SecurityAuditEvent evt, RequestContext context);
    Task<Result<Unit, DomainError>> AppendAdminChangeEventAsync(AdminChangeEvent evt, RequestContext context);
}

public sealed class AuditEventSink(IAuditStoreRepository auditRepository) : IAuditEventSink
{
    public Task<Result<Unit, DomainError>> AppendSecurityEventAsync(SecurityAuditEvent evt, RequestContext context) =>
        auditRepository.AppendSecurityEventAsync(evt, context);

    public Task<Result<Unit, DomainError>> AppendAdminChangeEventAsync(AdminChangeEvent evt, RequestContext context) =>
        auditRepository.AppendAdminChangeEventAsync(evt, context);
}
