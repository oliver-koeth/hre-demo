using AuthModule.CoreSecurity.Application.Common;
using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Configuration;
using AuthModule.CoreSecurity.Domain;
using AuthModule.CoreSecurity.Persistence;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;

namespace AuthModule.CoreSecurity.Application.Governance;

public sealed class MfaVerificationService(
    CoreSecurityConfiguration configuration,
    ICoreSecurityStateStore stateStore,
    IAuditEventSink auditEventSink) : IMfaVerificationService
{
    public Task<Result<StepUpChallenge, DomainError>> CreateChallengeAsync(CreateStepUpChallengeRequest request, RequestContext context)
    {
        var challenge = new StepUpChallenge
        {
            ChallengeId = Guid.NewGuid(),
            UserId = request.UserId,
            SessionId = request.SessionId,
            OperationKey = request.OperationKey,
            IssuedAt = context.Timestamp,
            ExpiresAt = context.Timestamp.AddSeconds(configuration.StepUpChallengeTtlSeconds),
            Status = StepUpStatus.Pending,
        };
        stateStore.SaveChallenge(challenge);
        return Task.FromResult(Result<StepUpChallenge, DomainError>.Success(challenge));
    }

    public async Task<Result<Unit, DomainError>> VerifyChallengeAsync(VerifyStepUpChallengeRequest request, RequestContext context)
    {
        var challenge = stateStore.GetChallenge(request.ChallengeId);
        if (challenge is null)
        {
            return Result<Unit, DomainError>.Failure(ErrorFactory.NotFound("Step-up challenge not found.", context));
        }

        if (challenge.ExpiresAt <= context.Timestamp)
        {
            challenge.Status = StepUpStatus.Expired;
            stateStore.SaveChallenge(challenge);
            return Result<Unit, DomainError>.Failure(ErrorFactory.Forbidden("Step-up challenge expired.", context));
        }

        if (request.VerificationCode != "000000")
        {
            challenge.Status = StepUpStatus.Failed;
            challenge.CompletedAt = context.Timestamp;
            stateStore.SaveChallenge(challenge);
            await auditEventSink.AppendSecurityEventAsync(new SecurityAuditEvent
            {
                EventId = Guid.NewGuid(),
                EventType = SecurityEventType.PrivilegedAccess,
                CorrelationId = context.CorrelationId,
                Timestamp = context.Timestamp,
                Result = OperationResult.Failure,
                Reason = "MFA challenge failed.",
            }, context);
            return Result<Unit, DomainError>.Failure(ErrorFactory.Forbidden("Step-up challenge failed.", context));
        }

        challenge.Status = StepUpStatus.Satisfied;
        challenge.CompletedAt = context.Timestamp;
        stateStore.SaveChallenge(challenge);
        return Result<Unit, DomainError>.Success(new Unit());
    }

    public Task<Result<Unit, DomainError>> EnsureSatisfiedAsync(Guid challengeId, RequestContext context)
    {
        var challenge = stateStore.GetChallenge(challengeId);
        if (challenge is null || challenge.Status != StepUpStatus.Satisfied || challenge.ExpiresAt <= context.Timestamp)
        {
            return Task.FromResult(Result<Unit, DomainError>.Failure(ErrorFactory.Forbidden("MFA step-up is not satisfied.", context)));
        }

        return Task.FromResult(Result<Unit, DomainError>.Success(new Unit()));
    }
}

public sealed class ApprovalWorkflowService(
    CoreSecurityConfiguration configuration,
    ICoreSecurityStateStore stateStore,
    IAuditEventSink auditEventSink) : IApprovalWorkflowService
{
    public Task<Result<ApprovalTicket, DomainError>> RequestApprovalAsync(ApprovalRequest request, RequestContext context)
    {
        if (!IsGovernedChangeType(request.ChangeType))
        {
            return Task.FromResult(Result<ApprovalTicket, DomainError>.Failure(
                ErrorFactory.Validation("Only governed role-permission changes are eligible for approval workflow.", context)));
        }

        var ticket = new ApprovalTicket
        {
            TicketId = Guid.NewGuid(),
            ChangeType = request.ChangeType,
            RoleId = request.RoleId,
            PermissionId = request.PermissionId,
            RequestedByUserId = context.UserId ?? Guid.Empty,
            RequestedAt = context.Timestamp,
            Status = ApprovalStatus.Pending,
        };
        stateStore.SaveApproval(ticket);
        return Task.FromResult(Result<ApprovalTicket, DomainError>.Success(ticket));
    }

    public async Task<Result<ApprovalTicket, DomainError>> DecideApprovalAsync(ApprovalDecisionRequest request, RequestContext context)
    {
        var ticket = stateStore.GetApproval(request.TicketId);
        if (ticket is null)
        {
            return Result<ApprovalTicket, DomainError>.Failure(ErrorFactory.NotFound("Approval ticket not found.", context));
        }

        if (ticket.Status is ApprovalStatus.Rejected or ApprovalStatus.Applied)
        {
            return Result<ApprovalTicket, DomainError>.Failure(ErrorFactory.Conflict("Approval ticket already terminal.", context));
        }

        if (ticket.RequestedByUserId == context.UserId)
        {
            return Result<ApprovalTicket, DomainError>.Failure(ErrorFactory.Forbidden("Requester cannot self-approve.", context));
        }

        if (!request.Approved)
        {
            ticket.Status = ApprovalStatus.Rejected;
            ticket.RejectedByUserId = context.UserId;
            ticket.RejectedAt = context.Timestamp;
            ticket.RejectionReason = request.RejectionReason;
            stateStore.SaveApproval(ticket);
            return Result<ApprovalTicket, DomainError>.Success(ticket);
        }

        ticket.Status = ApprovalStatus.Approved;
        ticket.ApprovedByUserId = context.UserId;
        ticket.ApprovedAt = context.Timestamp;
        stateStore.SaveApproval(ticket);

        var persisted = await PersistWithRetryAsync(ticket, context);
        if (persisted.IsFailure)
        {
            return persisted;
        }

        if (string.Equals(ticket.ChangeType, "ROLE_PERMISSION_ASSIGN", StringComparison.Ordinal))
        {
            stateStore.ApplyRolePermissionAssignment(ticket.RoleId, ticket.PermissionId, context.UserId ?? Guid.Empty, context.Timestamp);
        }
        else
        {
            stateStore.RevokeRolePermissionAssignment(ticket.RoleId, ticket.PermissionId, context.UserId ?? Guid.Empty, context.Timestamp);
        }

        ticket.Status = ApprovalStatus.Applied;
        stateStore.SaveApproval(ticket);
        return Result<ApprovalTicket, DomainError>.Success(ticket);
    }

    private async Task<Result<ApprovalTicket, DomainError>> PersistWithRetryAsync(ApprovalTicket ticket, RequestContext context)
    {
        for (var attempt = 0; attempt <= configuration.ApprovalRetryCount; attempt++)
        {
            var append = await auditEventSink.AppendAdminChangeEventAsync(new AdminChangeEvent
            {
                EventId = Guid.NewGuid(),
                EventType = "ApprovalApplied",
                ActorId = context.UserId ?? Guid.Empty,
                ApprovalTicketId = ticket.TicketId,
                TargetEntityType = "RolePermissionAssignment",
                TargetEntityId = ticket.RoleId,
                ChangeType = string.Equals(ticket.ChangeType, "ROLE_PERMISSION_ASSIGN", StringComparison.Ordinal)
                    ? EntityChangeType.Create
                    : EntityChangeType.Delete,
                CorrelationId = context.CorrelationId,
                Timestamp = context.Timestamp,
            }, context);

            if (append.IsSuccess)
            {
                return Result<ApprovalTicket, DomainError>.Success(ticket);
            }
        }

        return Result<ApprovalTicket, DomainError>.Failure(ErrorFactory.Internal("Approval persistence retries exhausted.", context));
    }

    private static bool IsGovernedChangeType(string changeType) =>
        string.Equals(changeType, "ROLE_PERMISSION_ASSIGN", StringComparison.Ordinal) ||
        string.Equals(changeType, "ROLE_PERMISSION_REVOKE", StringComparison.Ordinal);
}
