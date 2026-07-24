using AuthModule.CoreSecurity.Application.Common;
using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Domain;
using AuthModule.CoreSecurity.Persistence;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Foundation.Persistence.Contracts;

namespace AuthModule.CoreSecurity.Application.Authorization;

public sealed class AuthorizationService(
    IStoreRepository<Permission> permissionRepository,
    ICoreSecurityStateStore stateStore) : IAuthorizationService
{
    public async Task<Result<AuthorizationDecisionModel, DomainError>> AuthorizeAsync(AuthorizationRequest request, RequestContext context)
    {
        var permissionKey = Normalization.BuildPermissionKey(request.Resource, request.Action);
        var activeRoles = stateStore.GetRoleAssignments(request.UserId, context.Timestamp);
        if (activeRoles.Count == 0)
        {
            return Result<AuthorizationDecisionModel, DomainError>.Success(new AuthorizationDecisionModel
            {
                Allowed = false,
                ReasonCode = "DEFAULT_DENY",
                PermissionEvaluated = permissionKey,
            });
        }

        var permissionRecords = await permissionRepository.SearchAsync(new StoreSearchQuery<Permission>(_ => true), context);
        if (permissionRecords.IsFailure)
        {
            return Result<AuthorizationDecisionModel, DomainError>.Failure(ErrorFactory.Internal("Permission resolution failed.", context));
        }

        var permissionById = permissionRecords.Value.ToDictionary(p => p.PermissionId);
        foreach (var roleAssignment in activeRoles)
        {
            var grants = stateStore.GetPermissionAssignments(roleAssignment.RoleId, context.Timestamp);
            foreach (var grant in grants)
            {
                if (!permissionById.TryGetValue(grant.PermissionId, out var permission))
                {
                    continue;
                }

                var grantKey = Normalization.BuildPermissionKey(permission.Resource, permission.Action);
                if (string.Equals(grantKey, permissionKey, StringComparison.Ordinal))
                {
                    return Result<AuthorizationDecisionModel, DomainError>.Success(new AuthorizationDecisionModel
                    {
                        Allowed = true,
                        ReasonCode = "PERMISSION_MATCHED",
                        PermissionEvaluated = permissionKey,
                    });
                }
            }
        }

        return Result<AuthorizationDecisionModel, DomainError>.Success(new AuthorizationDecisionModel
        {
            Allowed = false,
            ReasonCode = "DEFAULT_DENY",
            PermissionEvaluated = permissionKey,
        });
    }
}
