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
    IAuthorizationService authorizationService,
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

    public async Task<Result<SearchUsersResponse, DomainError>> SearchUsersAsync(SearchUsersRequest request, RequestContext context)
    {
        if (context.UserId is null)
        {
            return Result<SearchUsersResponse, DomainError>.Failure(
                ErrorFactory.Unauthorized("Authenticated actor is required to search users.", context));
        }

        var authorization = await authorizationService.AuthorizeAsync(
            new AuthorizationRequest(context.UserId.Value, "users", "search"),
            context);
        if (authorization.IsFailure)
        {
            return Result<SearchUsersResponse, DomainError>.Failure(
                ErrorFactory.Forbidden("User search authorization could not be evaluated.", context));
        }

        if (!authorization.Value.Allowed)
        {
            return Result<SearchUsersResponse, DomainError>.Failure(
                ErrorFactory.Forbidden("Caller lacks permission to search users.", context));
        }

        var query = request.Query.Trim();
        if (query.Length is < 2 or > 100)
        {
            return Result<SearchUsersResponse, DomainError>.Failure(
                ErrorFactory.Validation("Search query must be between 2 and 100 characters after trimming.", context));
        }

        var allUsers = await userRepository.SearchAsync(new StoreSearchQuery<User>(_ => true), context);
        if (allUsers.IsFailure)
        {
            return Result<SearchUsersResponse, DomainError>.Failure(
                ErrorFactory.Internal("User search failed.", context));
        }

        var normalizedQuery = query.ToLowerInvariant();
        var matched = allUsers.Value
            .Where(u => u.DisplayName.ToLowerInvariant().Contains(normalizedQuery))
            .OrderBy(u => u.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(u => u.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var pageSize = Math.Clamp(request.PageSize is <= 0 ? 20 : request.PageSize, 1, 100);
        var page = Math.Max(request.Page, 1);
        var paged = matched
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserSearchResult(
                u.UserId,
                u.Username,
                u.DisplayName,
                u.Email,
                u.Status))
            .ToList();

        await auditEventSink.AppendSecurityEventAsync(new SecurityAuditEvent
        {
            EventId = Guid.NewGuid(),
            EventType = SecurityEventType.UserSearchExecuted,
            ActorId = context.UserId,
            CorrelationId = context.CorrelationId,
            Timestamp = context.Timestamp,
            Result = OperationResult.Success,
            Details = $"query={query}; total={matched.Count}; page={page}; pageSize={pageSize}",
        }, context);

        return Result<SearchUsersResponse, DomainError>.Success(new SearchUsersResponse(paged, matched.Count, page, pageSize));
    }
}
