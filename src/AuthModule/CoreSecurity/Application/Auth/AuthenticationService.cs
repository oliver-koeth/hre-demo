using AuthModule.CoreSecurity.Application.Common;
using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Application.Tokens;
using AuthModule.CoreSecurity.Configuration;
using AuthModule.CoreSecurity.Domain;
using AuthModule.CoreSecurity.Persistence;
using AuthModule.Foundation.Configuration;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;

namespace AuthModule.CoreSecurity.Application.Auth;

public sealed class AuthenticationService(
    IStoreRepository<User> userRepository,
    ICoreSecurityStateStore stateStore,
    IPasswordVerificationService passwordVerificationService,
    ITokenIssueService tokenIssueService,
    PolicyConfiguration policyConfiguration,
    CoreSecurityConfiguration coreSecurityConfiguration,
    IAuditEventSink auditEventSink,
    ISecurityAlertService securityAlertService) : IAuthenticationService
{
    public async Task<Result<LoginResponse, DomainError>> LoginAsync(LoginRequest request, RequestContext context)
    {
        var emailNormalized = Normalization.NormalizeEmail(request.Email);
        var attempt = stateStore.GetOrCreateAttempt(emailNormalized);
        if (attempt.LockedUntil is not null && attempt.LockedUntil > context.Timestamp)
        {
            await LogFailureAsync(context, SecurityEventType.AccountLocked, "Account is currently locked.");
            return Result<LoginResponse, DomainError>.Failure(ErrorFactory.Unauthorized("Authentication failed.", context));
        }

        var userLookup = await userRepository.SearchAsync(
            new StoreSearchQuery<User>(x => string.Equals(x.Email, emailNormalized, StringComparison.OrdinalIgnoreCase)),
            context);
        if (userLookup.IsFailure)
        {
            return Result<LoginResponse, DomainError>.Failure(userLookup.Error);
        }

        var user = userLookup.Value.FirstOrDefault();
        if (user is null || user.Status != UserStatus.Active)
        {
            await TrackFailedAttemptAsync(attempt, emailNormalized, context, "Invalid credentials or inactive account.");
            return Result<LoginResponse, DomainError>.Failure(ErrorFactory.Unauthorized("Authentication failed.", context));
        }

        var credential = stateStore.GetCredential(user.UserId);
        if (credential is null || !passwordVerificationService.Verify(credential, request.Password))
        {
            await TrackFailedAttemptAsync(attempt, emailNormalized, context, "Credential verification failed.");
            return Result<LoginResponse, DomainError>.Failure(ErrorFactory.Unauthorized("Authentication failed.", context));
        }

        stateStore.ResetAttempt(emailNormalized, context.Timestamp);
        var tokenVersion = stateStore.GetTokenVersion(user.UserId);
        var sessionId = Guid.NewGuid();
        stateStore.SaveSession(new AuthSession
        {
            SessionId = sessionId,
            UserId = user.UserId,
            TokenVersionSnapshot = tokenVersion,
            IssuedAt = context.Timestamp,
            ExpiresAt = context.Timestamp.AddSeconds(request.IsPrivilegedSession
                ? policyConfiguration.AdminTokenLifetimeSeconds
                : policyConfiguration.TokenLifetimeSeconds),
            IsPrivileged = request.IsPrivilegedSession,
        });

        await LogSuccessAsync(context, user.UserId);
        return tokenIssueService.Issue(user, sessionId, tokenVersion, request.IsPrivilegedSession, context);
    }

    private async Task TrackFailedAttemptAsync(AuthAttemptState attempt, string emailNormalized, RequestContext context, string reason)
    {
        attempt.FailedAttempts += 1;
        attempt.LastFailureAt = context.Timestamp;
        if (attempt.FailedAttempts >= coreSecurityConfiguration.MaxLoginAttempts)
        {
            attempt.LockedUntil = context.Timestamp.AddSeconds(coreSecurityConfiguration.LockoutDurationSeconds);
            await securityAlertService.EmitLockoutThresholdReachedAsync(emailNormalized, attempt.FailedAttempts, context);
        }

        stateStore.SaveAttempt(attempt);
        await LogFailureAsync(context, SecurityEventType.LoginFailure, reason);
    }

    private Task LogSuccessAsync(RequestContext context, Guid userId) =>
        auditEventSink.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.LoginSuccess,
            ActorId = userId,
            CorrelationId = context.CorrelationId,
            SourceIp = context.SourceIp,
            Timestamp = context.Timestamp,
            Result = OperationResult.Success,
            Details = "Login success.",
        }, context);

    private Task LogFailureAsync(RequestContext context, SecurityEventType eventType, string reason) =>
        auditEventSink.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = eventType,
            ActorId = null,
            CorrelationId = context.CorrelationId,
            SourceIp = context.SourceIp,
            Timestamp = context.Timestamp,
            Result = OperationResult.Failure,
            Reason = reason,
            Details = reason,
        }, context);
}
