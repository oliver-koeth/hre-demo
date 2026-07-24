using AuthModule.CoreSecurity.Application.Common;
using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Application.Auth;
using AuthModule.CoreSecurity.Persistence;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;

namespace AuthModule.CoreSecurity.Application.Users;

public sealed class UserAdministrationService(
    IStoreRepository<User> userRepository,
    ICoreSecurityStateStore stateStore,
    IPasswordVerificationService passwordVerificationService,
    IAuditEventSink auditEventSink) : IUserAdministrationService
{
    public async Task<Result<User, DomainError>> CreateUserAsync(CreateUserRequest request, RequestContext context)
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username.Trim(),
            Email = Domain.Normalization.NormalizeEmail(request.Email),
            DisplayName = request.DisplayName.Trim(),
            Status = UserStatus.Active,
            CreatedAt = context.Timestamp,
            UpdatedAt = context.Timestamp,
            CreatedBy = request.CreatedBy,
            IsDeleted = false,
            Version = 0,
        };

        var saved = await userRepository.SaveAsync(user, expectedVersion: null, context);
        if (saved.IsFailure)
        {
            return saved;
        }

        stateStore.SetTokenVersion(saved.Value.UserId, 0);
        stateStore.UpsertCredential(passwordVerificationService.HashForNewCredential(saved.Value.UserId, "ChangeMe!123"));
        return saved;
    }

    public async Task<Result<User, DomainError>> UpdateUserAsync(UpdateUserRequest request, RequestContext context)
    {
        var existing = await userRepository.GetAsync(new StoreQuery(request.UserId), context);
        if (existing.IsFailure || existing.Value is null)
        {
            return Result<User, DomainError>.Failure(ErrorFactory.NotFound("User not found.", context));
        }

        existing.Value.DisplayName = request.DisplayName.Trim();
        existing.Value.UpdatedAt = context.Timestamp;
        var saved = await userRepository.SaveAsync(existing.Value, existing.Value.Version, context);
        if (saved.IsFailure)
        {
            return saved;
        }

        return saved;
    }

    public async Task<Result<User, DomainError>> DisableUserAsync(DisableUserRequest request, RequestContext context)
    {
        var existing = await userRepository.GetAsync(new StoreQuery(request.UserId), context);
        if (existing.IsFailure || existing.Value is null)
        {
            return Result<User, DomainError>.Failure(ErrorFactory.NotFound("User not found.", context));
        }

        var user = existing.Value;
        if (user.Status != UserStatus.Inactive)
        {
            user.Status = UserStatus.Inactive;
            user.UpdatedAt = context.Timestamp;
            var saved = await userRepository.SaveAsync(user, user.Version, context);
            if (saved.IsFailure)
            {
                return saved;
            }
        }

        stateStore.IncrementTokenVersion(user.UserId);
        await auditEventSink.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.AccountDisabled,
            ActorId = context.UserId,
            CorrelationId = context.CorrelationId,
            Timestamp = context.Timestamp,
            Result = OperationResult.Success,
            Details = request.Reason,
        }, context);

        return Result<User, DomainError>.Success(user);
    }
}
