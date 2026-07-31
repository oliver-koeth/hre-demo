using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Domain;
using AuthModule.CoreSecurity.Tests.Support;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Persistence.Contracts;
using FsCheck;
using FsCheck.Xunit;

namespace AuthModule.CoreSecurity.Tests.PropertyBased;

public sealed class CoreSecurityPropertyTests
{
    [Property]
    public bool EmailNormalization_ShouldBeIdempotent(NonNull<string> raw)
    {
        var once = Normalization.NormalizeEmail(raw.Get);
        var twice = Normalization.NormalizeEmail(once);
        return once == twice;
    }

    [Fact]
    public async Task DisableUser_ShouldBeIdempotent()
    {
        var sut = await TestContextFactory.CreateAsync();
        var user = (await sut.Users.SearchAsync(new AuthModule.Foundation.Persistence.Contracts.StoreSearchQuery<AuthModule.Foundation.Domain.Entities.User>(_ => true), sut.RequestContext)).Value.Single();

        var first = await sut.UserAdministrationService.DisableUserAsync(new DisableUserRequest(user.UserId, "first"), sut.RequestContext);
        var second = await sut.UserAdministrationService.DisableUserAsync(new DisableUserRequest(user.UserId, "second"), sut.RequestContext);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
    }

    [Property]
    public bool PermissionKeyNormalization_ShouldBeDeterministic(NonNull<string> resource, NonNull<string> action)
    {
        var k1 = Normalization.BuildPermissionKey(resource.Get, action.Get);
        var k2 = Normalization.BuildPermissionKey(resource.Get, action.Get);
        return k1 == k2;
    }

    [Fact]
    public async Task SearchUsers_ExactDisplayName_ShouldReturnMatchingUser()
    {
        var sut = await TestContextFactory.CreateAsync();
        var admin = (await sut.Users.SearchAsync(
            new StoreSearchQuery<User>(_ => true),
            sut.RequestContext)).Value.Single();

        var role = (await sut.Roles.SaveAsync(new Role
        {
            RoleId = Guid.NewGuid(),
            Name = "user-searcher",
            Description = "can search users",
            CreatedAt = sut.RequestContext.Timestamp,
            UpdatedAt = sut.RequestContext.Timestamp,
            CreatedBy = sut.RequestContext.UserId ?? Guid.Empty,
        }, null, sut.RequestContext)).Value;

        var permission = (await sut.Permissions.SaveAsync(new Permission
        {
            PermissionId = Guid.NewGuid(),
            Name = "users:search",
            Resource = "users",
            Action = "search",
            Description = "search users",
            CreatedAt = sut.RequestContext.Timestamp,
            UpdatedAt = sut.RequestContext.Timestamp,
            CreatedBy = sut.RequestContext.UserId ?? Guid.Empty,
        }, null, sut.RequestContext)).Value;

        sut.StateStore.UpsertUserRoleAssignment(new UserRoleAssignment
        {
            AssignmentId = Guid.NewGuid(),
            UserId = admin.UserId,
            RoleId = role.RoleId,
            CreatedAt = sut.RequestContext.Timestamp,
            CreatedBy = sut.RequestContext.UserId ?? Guid.Empty,
            ValidFrom = sut.RequestContext.Timestamp.AddMinutes(-1),
        });
        sut.StateStore.ApplyRolePermissionAssignment(role.RoleId, permission.PermissionId, admin.UserId, sut.RequestContext.Timestamp);

        var targetUser = (await sut.Users.SaveAsync(new User
        {
            UserId = Guid.NewGuid(),
            Username = "target-user",
            Email = "target@example.com",
            DisplayName = "Target Display Name",
            Status = UserStatus.Active,
            CreatedAt = sut.RequestContext.Timestamp,
            UpdatedAt = sut.RequestContext.Timestamp,
            CreatedBy = admin.UserId,
        }, null, sut.RequestContext)).Value;

        var result = await sut.UserAdministrationService.SearchUsersAsync(
            new SearchUsersRequest("Target Display Name", 1, 20),
            sut.RequestContext with { UserId = admin.UserId });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Results);
        Assert.Equal(targetUser.UserId, result.Value.Results[0].UserId);
    }
}
