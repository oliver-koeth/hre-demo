using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Tests.Support;
using AuthModule.Foundation.Domain.Entities;
using AuthModule.Foundation.Persistence.Contracts;
using FluentAssertions;

namespace AuthModule.CoreSecurity.Tests.Authorization;

public sealed class PermissionResolutionTests
{
    [Fact]
    public async Task Authorize_ShouldAllow_WhenMatchingActiveGrantExists()
    {
        var sut = await TestContextFactory.CreateAsync();
        var user = (await sut.Users.SearchAsync(new StoreSearchQuery<User>(_ => true), sut.RequestContext)).Value.Single();

        var role = (await sut.Roles.SaveAsync(new Role
        {
            RoleId = Guid.NewGuid(),
            Name = "admin",
            Description = "admin",
            CreatedAt = sut.RequestContext.Timestamp,
            UpdatedAt = sut.RequestContext.Timestamp,
            CreatedBy = sut.RequestContext.UserId ?? Guid.Empty,
        }, null, sut.RequestContext)).Value;

        var permission = (await sut.Permissions.SaveAsync(new Permission
        {
            PermissionId = Guid.NewGuid(),
            Name = "users:disable",
            Resource = "users",
            Action = "disable",
            Description = "disable users",
            CreatedAt = sut.RequestContext.Timestamp,
            UpdatedAt = sut.RequestContext.Timestamp,
            CreatedBy = sut.RequestContext.UserId ?? Guid.Empty,
        }, null, sut.RequestContext)).Value;

        sut.StateStore.UpsertUserRoleAssignment(new UserRoleAssignment
        {
            AssignmentId = Guid.NewGuid(),
            UserId = user.UserId,
            RoleId = role.RoleId,
            CreatedAt = sut.RequestContext.Timestamp,
            CreatedBy = sut.RequestContext.UserId ?? Guid.Empty,
            ValidFrom = sut.RequestContext.Timestamp.AddMinutes(-1),
        });
        sut.StateStore.ApplyRolePermissionAssignment(role.RoleId, permission.PermissionId, sut.RequestContext.UserId ?? Guid.Empty, sut.RequestContext.Timestamp);

        var decision = await sut.AuthorizationService.AuthorizeAsync(
            new AuthorizationRequest(user.UserId, "users", "disable"),
            sut.RequestContext);

        decision.IsSuccess.Should().BeTrue();
        decision.Value.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Authorize_ShouldDeny_WhenNoGrantExists()
    {
        var sut = await TestContextFactory.CreateAsync();
        var user = (await sut.Users.SearchAsync(new StoreSearchQuery<User>(_ => true), sut.RequestContext)).Value.Single();
        var decision = await sut.AuthorizationService.AuthorizeAsync(
            new AuthorizationRequest(user.UserId, "users", "disable"),
            sut.RequestContext);

        decision.IsSuccess.Should().BeTrue();
        decision.Value.Allowed.Should().BeFalse();
        decision.Value.ReasonCode.Should().Be("DEFAULT_DENY");
    }
}
