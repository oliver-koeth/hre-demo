using AuthModule.CoreSecurity.Domain;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;

namespace AuthModule.CoreSecurity.Application.Contracts;

public sealed record LoginRequest(string Email, string Password, bool IsPrivilegedSession = false);
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, Guid SessionId);
public sealed record ValidateTokenRequest(string AccessToken);
public sealed record CreateStepUpChallengeRequest(Guid UserId, Guid SessionId, string OperationKey);
public sealed record VerifyStepUpChallengeRequest(Guid ChallengeId, string VerificationCode);
public sealed record AuthorizationRequest(Guid UserId, string Resource, string Action);
public sealed record DisableUserRequest(Guid UserId, string Reason);
public sealed record CreateUserRequest(string Username, string Email, string DisplayName, Guid CreatedBy);
public sealed record UpdateUserRequest(Guid UserId, string DisplayName);
public sealed record ApprovalRequest(Guid RoleId, Guid PermissionId, string ChangeType);
public sealed record ApprovalDecisionRequest(Guid TicketId, bool Approved, string? RejectionReason);

public interface IAuthenticationService
{
    Task<Result<LoginResponse, DomainError>> LoginAsync(LoginRequest request, RequestContext context);
}

public interface ITokenValidationService
{
    Task<Result<AccessTokenClaimsModel, DomainError>> ValidateAsync(ValidateTokenRequest request, RequestContext context);
}

public interface IAuthorizationService
{
    Task<Result<AuthorizationDecisionModel, DomainError>> AuthorizeAsync(AuthorizationRequest request, RequestContext context);
}

public interface IApprovalWorkflowService
{
    Task<Result<ApprovalTicket, DomainError>> RequestApprovalAsync(ApprovalRequest request, RequestContext context);
    Task<Result<ApprovalTicket, DomainError>> DecideApprovalAsync(ApprovalDecisionRequest request, RequestContext context);
}

public interface IMfaVerificationService
{
    Task<Result<StepUpChallenge, DomainError>> CreateChallengeAsync(CreateStepUpChallengeRequest request, RequestContext context);
    Task<Result<Unit, DomainError>> VerifyChallengeAsync(VerifyStepUpChallengeRequest request, RequestContext context);
    Task<Result<Unit, DomainError>> EnsureSatisfiedAsync(Guid challengeId, RequestContext context);
}

public interface ISecurityAlertService
{
    Task EmitLockoutThresholdReachedAsync(string emailNormalized, int lockoutCount, RequestContext context);
}

public interface IUserAdministrationService
{
    Task<Result<User, DomainError>> CreateUserAsync(CreateUserRequest request, RequestContext context);
    Task<Result<User, DomainError>> UpdateUserAsync(UpdateUserRequest request, RequestContext context);
    Task<Result<User, DomainError>> DisableUserAsync(DisableUserRequest request, RequestContext context);
}
